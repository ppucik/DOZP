using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Microsoft.Windows.Controls.Ribbon;

using ImageProcessor;
using ImageProcessor.Imaging.Formats;
using ImageComponents.WPF.Imaging;
using ImageComponents.WPF.Utilities;

using Comdat.DOZP.Core;
using Comdat.DOZP.Process;

namespace Comdat.DOZP.App
{
    using Properties;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        #region Private members
        const string OBALKY_KNIH_FILENAME = "ObalkyKnihCZ.jpg";

        private Institution _institution = null;

        private static readonly object _locker = new object();
        private System.Threading.Thread _scannerThread = null;
        //private Boolean _workingProgress = false;
        private Stopwatch _timer = new Stopwatch();

        //private static RoutedCommand _helpCommand = new RoutedCommand();
        private static RoutedCommand _newBookCommand = new RoutedCommand();
        private static RoutedCommand _scanFrontCoverCommand = new RoutedCommand();
        private static RoutedCommand _scanTableOfContentsCommand = new RoutedCommand();
        private static RoutedCommand _sendScanCommand = new RoutedCommand();
        private static RoutedCommand _imageDeleteCommand = new RoutedCommand();
        private static RoutedCommand _imageRotateAngleCommand = new RoutedCommand();
        private static RoutedCommand _imageDeskewCommand = new RoutedCommand();
        private static RoutedCommand _imageCropCommand = new RoutedCommand();
        private static RoutedCommand _imageColorCommand = new RoutedCommand();
        private static RoutedCommand _imageUndoButton = new RoutedCommand();
        private static RoutedCommand _frontCoverCommand = new RoutedCommand();
        private static RoutedCommand _previousPageCommand = new RoutedCommand();
        private static RoutedCommand _nextPageCommand = new RoutedCommand();
        private static RoutedCommand _scannerPageSizeNoneCommand = new RoutedCommand();
        private static RoutedCommand _scannerPageSizeA4Command = new RoutedCommand();
        private static RoutedCommand _scannerPageSizeA5Command = new RoutedCommand();
        private static RoutedCommand _optionsCommand = new RoutedCommand();
        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
            InitializeShortcuts();

            try
            {
                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                string appName = Properties.Resources.ApplicationName;
                string fullName = AuthController.UserIdentity.LoginUser.FullName;
                this.RibbonWindow.Title = String.Format("{0} v{1}.{2}.{3} [{4}]", appName, version.Major, version.Minor, version.Revision, fullName);
                this.WindowState = (Settings.Default.MainWindowMax ? WindowState.Maximized : WindowState.Normal);
                this.Topmost = Settings.Default.MainWindowTopmost;

                if (Settings.Default.AppLogging)
                {
                    Logger.Open();
                    Logger.Log(String.Format("\n{0}, Version:{1}.{2}.{3} Build:{4}, UserName:{5}, MachineName:{6}", appName, version.Major, version.Minor, version.Revision, version.Build, fullName, Environment.MachineName));
                    SetScannerTrace(ImgScan.TraceOutputType.File);
                }

                this.ScanCoverPixelTypeComboBox.IsEnabled = AuthController.UserIdentity.IsAdministrator;
                this.ScanCoverPixelTypeCategory.ItemsSource = Enumeration.GetList(typeof(TwainPixelTypes));

                this.ScanCoverResolutionComboBox.IsEnabled = AuthController.UserIdentity.IsAdministrator;
                this.ScanCoverResolutionCategory.ItemsSource = ScanImage.RESOLUTIONS;

                this.ScanContentsPixelTypeComboBox.IsEnabled = AuthController.UserIdentity.IsAdministrator;
                this.ScanContentsPixelTypeCategory.ItemsSource = Enumeration.GetList(typeof(TwainPixelTypes));

                this.ScanContentsResolutionComboBox.IsEnabled = AuthController.UserIdentity.IsAdministrator;
                this.ScanContentsResolutionCategory.ItemsSource = ScanImage.RESOLUTIONS;

                this.DataContext = this;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
        }

        ~MainWindow()
        {
            _institution = null;
            _timer = null;
        }

        #endregion

        #region Properties

        private Institution Institution
        {
            get
            {
                if (_institution == null)
                    _institution = DozpController.GetInstitution();

                return _institution;
            }
        }

        private Catalogue SelectedCatalogue
        {
            get
            {
                if (this.Institution != null)
                    return this.Institution.Catalogues.SingleOrDefault(c => c.CatalogueID == Settings.Default.SelectedCatalogueID);
                else
                    return null;
            }
        }

        private PartOfBook? SelectedPartOfBook
        {
            get
            {
                return (CurrentImgEdit != null ? (PartOfBook?)CurrentImgEdit.Tag : null);
            }
        }

        private ImgEdit CurrentImgEdit
        {
            get
            {
                if (FrontCoverImgEdit.Visibility == Visibility.Visible)
                    return FrontCoverImgEdit;
                else if (TableOfContentsImgEdit.Visibility == Visibility.Visible)
                    return TableOfContentsImgEdit;
                else
                    return null;
            }
        }

        private ImgScan CurrentImgScan
        {
            get
            {
                if (FrontCoverImgEdit.Visibility == Visibility.Visible)
                    return FrontCoverImgScan;
                else if (TableOfContentsImgEdit.Visibility == Visibility.Visible)
                    return TableOfContentsImgScan;
                else
                    return null;
            }
        }

        private bool HasScannedFrontCover
        {
            get
            {
                return (FrontCoverImgEdit.PageCount > 0 && !FrontCoverImgEdit.ImageFilePath.Contains(OBALKY_KNIH_FILENAME));
            }
        }

        private bool HasScannedTableOfContents
        {
            get
            {
                return (TableOfContentsImgEdit.PageCount > 0);
            }
        }

        private bool HasScannedImages
        {
            get
            {
                return (HasScannedFrontCover || HasScannedTableOfContents);
            }
        }

        #endregion

        #region Window events

        private void RibbonWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Cursor = Cursors.Wait;
                Logger.Log("START Application");

                this.FrontCoverImgEdit.Tag = PartOfBook.FrontCover;
                this.FrontCoverImgEdit.EnableAsynchronousWork = false;
                this.FrontCoverImgScan.ImgEditComponent = this.FrontCoverImgEdit;
                this.FrontCoverThumbnails.ImgEditComponent = this.FrontCoverImgEdit;

                this.TableOfContentsImgEdit.Tag = PartOfBook.TableOfContents;
                this.TableOfContentsImgEdit.EnableAsynchronousWork = false;
                this.TableOfContentsImgScan.ImgEditComponent = this.TableOfContentsImgEdit;
                this.TableOfContentsThumbnails.ImgEditComponent = this.TableOfContentsImgEdit;

                //upgrade nastaveni pro novou verzi
                if (Settings.Default.UpgradeSettings)
                {
                    Settings.Default.Upgrade();
                    Settings.Default.UpgradeSettings = false;
                    Settings.Default.Save();
                }

                //nastaveni katalogu
                if (Settings.Default.SelectedCatalogueID == 0 && Institution != null && Institution.Catalogues.Count > 0)
                {
                    Settings.Default.SelectedCatalogueID = Institution.Catalogues.First().CatalogueID;
                }

                //nastrojova lista + menu
                this.FileAuxiliaryPaneTitle.Text = (Institution != null ? Institution.Name : "Instituce není nastavena");
                this.FileAuxiliaryPaneContent.Text = (SelectedCatalogue != null ? SelectedCatalogue.Name : "Katalog není vybrán") + Environment.NewLine;
                this.FileAuxiliaryPaneContent.Text += (!String.IsNullOrEmpty(Settings.Default.ScannerSourceName) ? Settings.Default.ScannerSourceName : "Skener není vybrán") + Environment.NewLine;

                //nastaveni katalogu
                foreach (var catalogue in Institution.Catalogues)
                {
                    RibbonMenuItem item = new RibbonMenuItem();
                    item.Style = (LayoutRoot.Resources["RibbonMenuItemCheckable"] as Style);
                    item.IsChecked = (catalogue.CatalogueID == Settings.Default.SelectedCatalogueID);
                    item.Header = catalogue.Name;
                    item.Tag = catalogue.CatalogueID;
                    item.Click += CatalogueMenuItem_Click;
                    this.CatalogueButton.Items.Add(item);
                }

                //nastaveni skeneru
                ArrayList scanners = this.FrontCoverImgScan.GetScannerSources();
                foreach (string name in scanners)
                {
                    RibbonMenuItem item = new RibbonMenuItem();
                    bool active = FrontCoverImgScan.IsSourceActive(name);
                    item.Style = (LayoutRoot.Resources["RibbonMenuItemCheckable"] as Style);
                    item.IsChecked = (name.Equals(Settings.Default.ScannerSourceName));
                    item.Header = name;
                    item.Click += ScannerMenuItem_Click;
                    this.ScannerButton.Items.Add(item);
                }

                //nastveni barevneho modu
                if (String.IsNullOrEmpty(Settings.Default.ScanCoverPixelType))
                    Settings.Default.ScanCoverPixelType = TwainPixelTypes.RGB.ToDisplay();
                if (String.IsNullOrEmpty(Settings.Default.ScanContentsPixelType))
                    Settings.Default.ScanContentsPixelType = TwainPixelTypes.BW.ToDisplay();

                //nastveni velikosti a orientace
                SetScannerPageSize(Enumeration.Parse<ImgScan.ICTwainSupportedSizes>(Settings.Default.ScannerPageSize));
                SetScannerPageOrientation(Enumeration.Parse<ImgScan.ICTwainOrientations>(Settings.Default.ScanCapPaperOrientation));

                //nastaveni automaticke upravy
                SetScannerAutomatic();

                //logovani

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
            finally
            {
                SetMenuButtonsEnabled();
                GetAvailableMemory();
                this.Cursor = null;
            }
        }

        private void RibbonWindow_ContentRendered(object sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.Wait;

                if (!CheckScannerSettings())
                {
                    //FrontCoverImgScan.SelectSource(this);
                }

                if (Settings.Default.SelectedCatalogueID != 0 && Settings.Default.LastScanBook != null)
                {
                    string sysno = Settings.Default.LastScanBook.SysNo;
                    string message = String.Format("Publikace SYSNO: {0} nebyla pravděpodobně dokončena, chcete načíst znova publikaci a obnovit soubory?", sysno);

                    if (MessageBox.Show(message, this.Title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        RecoveryLastBook();
                    else
                        ClearData();
                }

                List<ScanFile> discardContents = DozpController.GetDiscardContents(Settings.Default.SelectedCatalogueID, AuthController.UserIdentity.LoginUser.UserName);
                this.BrowseBookButton.IsEnabled = (discardContents != null && discardContents.Count > 0);

                //ScannerManager.Test(Settings.Default.ScannerSourceName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
            finally
            {
                SetMenuButtonsEnabled();
                GetAvailableMemory(true);
                this.Cursor = null;
            }
        }

        private void RibbonWindow_StateChanged(object sender, EventArgs e)
        {
            RibbonWindow_StateSizeChanged();
        }

        private void RibbonWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RibbonWindow_StateSizeChanged();
        }

        private void RibbonWindow_StateSizeChanged()
        {
            try
            {
                double width = 0;

                if (RibbonWindow.WindowState == WindowState.Maximized)
                {
                    width = TableOfContentsColumn.MaxWidth;
                }
                else if (RibbonWindow.WindowState == WindowState.Normal)
                {
                    if ((this.Width < 1200) && (TableOfContentsColumn.Width.Value > TableOfContentsColumn.MinWidth))
                    {
                        width = TableOfContentsColumn.MinWidth;
                    }
                    else if ((this.Width >= 1200) && (TableOfContentsColumn.Width.Value < TableOfContentsColumn.MaxWidth))
                    {
                        width = TableOfContentsColumn.MaxWidth;
                    }
                }

                if (width != 0)
                {
                    TableOfContentsColumn.Width = new GridLength(width);
                    CurrentImgEdit.FitTo(ImgEdit.ICImageFit.BestFit, true);
                }
            }
            catch
            {
            }
        }

        private void RibbonWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Settings.Default.SelectedCatalogueID = 0;
            Settings.Default.MainWindowMax = (this.WindowState == WindowState.Maximized);
            Settings.Default.Save();

            try
            {
                if (CurrentImgEdit != null && HasScannedImages)
                {
                    CurrentImgEdit.SaveImage();
                }
            }
            catch
            {
            }
        }

        private void RibbonWindow_Closed(object sender, EventArgs e)
        {
            Logger.Log("END Application");
        }

        #endregion

        #region FrontCover events

        #region ImgScan

        private void FrontCoverImgScan_ScanStarted(object sender, ImgScan.ScanStartedEventArgs e)
        {
            Debug.WriteLine("FrontCoverImgScan_ScanStarted");
            this.WorkingGifImage.Visibility = Visibility.Visible;
        }

        private void FrontCoverImgScan_ScanFinished(object sender, ImgScan.ScanEventArgs e)
        {
            Debug.WriteLine(String.Format("FrontCoverImgScan_ScanFinished: [{0}] {1}", e.PagePosition, e.ScannedFilename));

            SetMainWindowTopmost();
            this.WorkingGifImage.Visibility = Visibility.Visible;

            FrontCoverImgEdit.Page = e.PagePosition;
            FrontCoverThumbnails.DefaultThumbnailCaption = "Obálka";
            CurrentBookInfoBox.SetImageChanged(PartOfBook.FrontCover);
            ShowMainImgEdit(PartOfBook.FrontCover);

            if (File.Exists(e.ScannedFilename))
            {
                if (!String.IsNullOrEmpty(Settings.Default.ScannerPageSize))
                {
                    ImgScan.DeviceUICapabilities.ImageCapabilities capabilities = FrontCoverImgScan.DeviceCapabilities.TwainICapabilities;

                    if (capabilities.PaperSizeValue.HasValue && capabilities.PaperSizeValue.Value != ImgScan.ICTwainSupportedSizes.None)
                    {
                        if (FrontCoverImgScan.AvailableDeviceCapabilities.TwainICapabilities.AvailablePaperSizes == null ||
                            FrontCoverImgScan.AvailableDeviceCapabilities.TwainICapabilities.AvailablePaperSizes.Contains(capabilities.PaperSizeValue.Value) == false)
                        {
                            Rect area = GetScannerArea(capabilities.PaperSizeValue, capabilities.PaperOrientationValue, capabilities.ResolutionValue);
                            if (!area.IsEmpty)
                            {
                                FrontCoverImgEdit.SetSelectionRectangle((int)area.Left, (int)area.Top, (int)area.Width, (int)area.Height);
                                FrontCoverImgEdit.CropImage();
                                FrontCoverImgEdit.SaveImageAs(CurrentBookInfoBox.GetFullName(PartOfBook.FrontCover));
                            }
                        }
                    }
                }
            }

            StopTimer();
            GetAvailableMemory(true);
            SetMenuButtonsEnabled();
            this.WorkingGifImage.Visibility = Visibility.Hidden;
            this.Cursor = null;
        }

        private void FrontCoverImgScan_ErrorOccurred(object sender, string ControlName, string MethodName, string ErrorMessage)
        {
            ImageComponentsErrorOccurred(ControlName, MethodName, ErrorMessage);

            this.Cursor = null;
            this.WorkingGifImage.Visibility = Visibility.Hidden;
        }

        #endregion

        #region ImgEdit

        private void FrontCoverImgEdit_ImageLoaded(object sender)
        {
            Debug.WriteLine("FrontCoverImgEdit_ImageLoaded");

            this.ObalkyKnihImage.Visibility = (FrontCoverImgEdit.ImageFilePath.Contains("ObalkyKnihCZ") ? Visibility.Visible : Visibility.Hidden);
        }

        private void FrontCoverImgEdit_PageChanged(object sender, int PagePosition)
        {
            Debug.WriteLine(String.Format("FrontCoverImgEdit_PageChanged [{0}]", PagePosition));

            CurrentBookInfoBox.SetImageChanged(PartOfBook.FrontCover);
        }

        private void FrontCoverImgEdit_ImageChanged(object sender)
        {
            Debug.WriteLine("FrontCoverImgEdit_ImageChanged");

            CurrentBookInfoBox.SetImageChanged(PartOfBook.FrontCover);
        }

        private void FrontCoverImgEdit_ImageSaved(object sender, ImageSavedOutputEventArgs e)
        {
            Debug.WriteLine(String.Format("FrontCoverImgEdit_ImageSaved: {0}", e.OutputFileName));

            //if (File.Exists(e.OutputFileName))
            //{
            //    FrontCoverImgEdit.ImageFilePath = e.OutputFileName;
            //    FrontCoverImgEdit.Display();
            //}
        }

        private void FrontCoverImgEdit_ImageClosed(object sender)
        {
            Debug.WriteLine("FrontCoverImgEdit_ImageClosed");
        }

        private void FrontCoverImgEdit_ErrorOccurred(object sender, string ControlName, string MethodName, string ErrorMessage)
        {
            ImageComponentsErrorOccurred(ControlName, MethodName, ErrorMessage);

            this.Cursor = null;
            this.WorkingGifImage.Visibility = Visibility.Hidden;
        }

        #endregion

        #region Thumbnails

        private void FrontCoverThumbnails_ThumbnailLoaded(object sender, ThumbnailObject ImageThumbnail)
        {
            Debug.WriteLine(String.Format("FrontCoverThumbnails_ThumbnailLoaded [{0}]: {1}", ImageThumbnail.ThumbnailIndex, ImageThumbnail.ThumbnailFilePath));
        }

        private void FrontCoverThumbnails_ThumbnailSelected(object sender, ThumbnailObject ImageThumbnail)
        {
            Debug.WriteLine(String.Format("FrontCoverThumbnails_ThumbnailSelected [{0}]: {1}", ImageThumbnail.ThumbnailIndex, ImageThumbnail.ThumbnailFilePath));

            ShowMainImgEdit(PartOfBook.FrontCover);
        }

        private void FrontCoverThumbnails_ThumbnailMouseLeftButtonDown(object sender, ThumbnailObject ImageThumbnail)
        {
            Debug.WriteLine(String.Format("FrontCoverThumbnails_ThumbnailMouseLeftButtonDown [{0}]: {1}", ImageThumbnail.ThumbnailIndex, ImageThumbnail.ThumbnailFilePath));

            ShowMainImgEdit(PartOfBook.FrontCover);
        }

        #endregion

        #endregion

        #region TableOfContents events

        #region ImgScan

        private void TableOfContentsImgScan_ScanStarted(object sender, ImgScan.ScanStartedEventArgs e)
        {
            Debug.WriteLine("TableOfContentsImgScan_ScanStarted");
            this.WorkingGifImage.Visibility = Visibility.Visible;
        }

        private void TableOfContentsImgScan_ScanPageStarted(object sender, ImgScan.ScanStartedEventArgs e)
        {
            Debug.WriteLine("TableOfContentsImgScan_ScanPageStarted");
        }

        private void TableOfContentsImgScan_ScanPageFinished(object sender, ImgScan.ScanEventArgs e)
        {
            Debug.WriteLine(String.Format("TableOfContentsImgScan_ScanPageFinished: [{0}] {1}", e.PagePosition, e.ScannedFilename));
        }

        private void TableOfContentsImgScan_ScanFinished(object sender, ImgScan.ScanEventArgs e)
        {
            Debug.WriteLine(String.Format("TableOfContentsImgScan_ScanFinished: [{0}] {1}", e.PagePosition, e.ScannedFilename));

            SetMainWindowTopmost();
            this.WorkingGifImage.Visibility = Visibility.Visible;

            TableOfContentsImgEdit.Page = e.PagePosition;
            ShowMainImgEdit(PartOfBook.TableOfContents);
            CurrentBookInfoBox.SetImageChanged(PartOfBook.TableOfContents);

            if (File.Exists(e.ScannedFilename))
            {
                if (!String.IsNullOrEmpty(Settings.Default.ScannerPageSize))
                {
                    ImgScan.DeviceUICapabilities.ImageCapabilities capabilities = TableOfContentsImgScan.DeviceCapabilities.TwainICapabilities;

                    if (capabilities.PaperSizeValue.HasValue && capabilities.PaperSizeValue.Value != ImgScan.ICTwainSupportedSizes.None)
                    {
                        if (TableOfContentsImgScan.AvailableDeviceCapabilities.TwainICapabilities.AvailablePaperSizes == null ||
                            TableOfContentsImgScan.AvailableDeviceCapabilities.TwainICapabilities.AvailablePaperSizes.Contains(capabilities.PaperSizeValue.Value) == false)
                        {
                            Rect area = GetScannerArea(capabilities.PaperSizeValue, capabilities.PaperOrientationValue, capabilities.ResolutionValue);
                            if (!area.IsEmpty)
                            {
                                Debug.WriteLine("=> TableOfContentsImgEdit.CropImage");
                                TableOfContentsImgEdit.SetSelectionRectangle((int)area.Left, (int)area.Top, (int)area.Width, (int)area.Height);
                                TableOfContentsImgEdit.CropImage();
                            }
                        }
                    }
                }

                if (Settings.Default.ScanRotateEvenPage && ((TableOfContentsImgEdit.PageCount) % 2 == 0))
                {
                    if (Convert.ToBoolean(TableOfContentsImgScan.DeviceCapabilities.TwainCapabilities.IsAutoRotationEnabled) &&
                        Convert.ToBoolean(TableOfContentsImgScan.AvailableDeviceCapabilities.TwainCapabilities.IsAutoRotationEnabled) == false)
                    {
                        Debug.WriteLine("=> TableOfContentsImgEdit.Flip");
                        TableOfContentsImgEdit.Flip();
                    }
                }

                if (TableOfContentsImgEdit.Page == 0)
                {
                    Debug.WriteLine("=> TableOfContentsImgEdit.SaveImageAs");
                    TableOfContentsImgEdit.SaveImageAs(CurrentBookInfoBox.GetFullName(PartOfBook.TableOfContents));
                }
            }

            StopTimer();
            GetAvailableMemory(true);
            SetMenuButtonsEnabled();
            this.WorkingGifImage.Visibility = Visibility.Hidden;
            this.Cursor = null;
        }

        private void TableOfContentsImgScan_ErrorOccurred(object sender, string ControlName, string MethodName, string ErrorMessage)
        {
            ImageComponentsErrorOccurred(ControlName, MethodName, ErrorMessage);
            
            this.Cursor = null;
            this.WorkingGifImage.Visibility = Visibility.Hidden;
        }

        #endregion

        #region ImgEdit

        private void TableOfContentsImgEdit_ImageLoaded(object sender)
        {
            Debug.WriteLine("TableOfContentsImgEdit_ImageLoaded");
        }

        private void TableOfContentsImgEdit_PageLoaded(object sender, int PagePosition)
        {
            Debug.WriteLine(String.Format("TableOfContentsImgEdit_PageLoaded [{0}]", PagePosition));
        }

        private void TableOfContentsImgEdit_PageRequested(object sender, ImageRequestedEventArgs e)
        {
            Debug.WriteLine("=== TableOfContentsImgEdit_PageRequested ===");
        }

        private void TableOfContentsImgEdit_PageChanged(object sender, int PagePosition)
        {
            Debug.WriteLine(String.Format("TableOfContentsImgEdit_PageChanged [{0}]", PagePosition));

            CurrentBookInfoBox.SetImageChanged(PartOfBook.TableOfContents);
        }

        private void TableOfContentsImgEdit_PageNavigated(object sender, int PagePosition)
        {
            //Debug.WriteLine(String.Format("TableOfContentsImgEdit_PageNavigated [{0}]", PagePosition));

            //if (TableOfContentsImgEdit.Tag != null && TableOfContentsImgEdit.Tag.Equals(PartOfBook.TableOfContents))
            //{
            //    ContentsThumbnails.SelectedThumbnail = PagePosition;
            //}
        }

        private void TableOfContentsImgEdit_PageSaved(object sender, ImageSavedOutputEventArgs e)
        {
            Debug.WriteLine(String.Format("TableOfContentsImgEdit_PageSaved [{0}]", e.PagePosition));
        }

        private void TableOfContentsImgEdit_ImageChanged(object sender)
        {
            Debug.WriteLine("TableOfContentsImgEdit_ImageChanged");

            CurrentBookInfoBox.SetImageChanged(PartOfBook.TableOfContents);
        }

        private void TableOfContentsImgEdit_ImageSaved(object sender, ImageSavedOutputEventArgs e)
        {
            Debug.WriteLine(String.Format("TableOfContentsImgEdit_ImageSaved: {0}", e.OutputFileName));

            if (File.Exists(e.OutputFileName))
            {
                TableOfContentsImgEdit.ImageFilePath = e.OutputFileName;
                TableOfContentsImgEdit.Display();
            }
        }

        private void TableOfContentsImgEdit_ImageClosed(object sender)
        {
            Debug.WriteLine("TableOfContentsImgEdit_ImageClosed");
        }

        private void TableOfContentsImgEdit_WorkingStarted(object sender)
        {
            Debug.WriteLine("TableOfContentsImgEdit_WorkingStarted");
            this.Cursor = Cursors.Wait;
            this.WorkingGifImage.Visibility = Visibility.Visible;
            //this.WorkingProgressBar.Visibility = Visibility.Visible;
        }

        private void TableOfContentsImgEdit_WorkingProgress(object sender, int Total, int Value)
        {
            Debug.WriteLine(String.Format("TableOfContentsImgEdit_WorkingProgress {0}/{1}", Value, Total));
            this.Cursor = Cursors.Wait;
            this.WorkingGifImage.Visibility = Visibility.Visible;
            //this.WorkingProgressBar.Visibility = Visibility.Visible;
            //this.WorkingProgressBar.Minimum = 0;
            //this.WorkingProgressBar.Value = Value + 1;
            //this.WorkingProgressBar.Maximum = Total;
        }

        private void TableOfContentsImgEdit_WorkingFinished(object sender)
        {
            Debug.WriteLine("TableOfContentsImgEdit_WorkingFinished");
            this.Cursor = null;
            this.WorkingGifImage.Visibility = Visibility.Hidden;
            //this.WorkingProgressBar.Visibility = Visibility.Hidden;
        }

        private void TableOfContentsImgEdit_ErrorOccurred(object sender, string ControlName, string MethodName, string ErrorMessage)
        {
            ImageComponentsErrorOccurred(ControlName, MethodName, ErrorMessage);

            this.Cursor = null;
            this.WorkingGifImage.Visibility = Visibility.Hidden;
        }

        #endregion

        #region Thumbnails

        private void TableOfContentsThumbnails_ThumbnailLoaded(object sender, ThumbnailObject ImageThumbnail)
        {
            Debug.WriteLine(String.Format("TableOfContentsThumbnails_ThumbnailLoaded [{0}]: {1}", ImageThumbnail.ThumbnailIndex, ImageThumbnail.ThumbnailFilePath));
        }

        private void TableOfContentsThumbnails_ThumbnailSelected(object sender, ThumbnailObject ImageThumbnail)
        {
            Debug.WriteLine(String.Format("TableOfContentsThumbnails_ThumbnailSelected [{0}]: {1}", ImageThumbnail.ThumbnailIndex, ImageThumbnail.ThumbnailFilePath));

            ShowMainImgEdit(PartOfBook.TableOfContents);
        }

        private void TableOfContentsThumbnails_ThumbnailMouseLeftButtonDown(object sender, ThumbnailObject ImageThumbnail)
        {
            Debug.WriteLine(String.Format("TableOfContentsThumbnails_ThumbnailMouseLeftButtonDown [{0}]: {1}", ImageThumbnail.ThumbnailIndex, ImageThumbnail.ThumbnailFilePath));

            ShowMainImgEdit(PartOfBook.TableOfContents);
        }

        private void TableOfContentsThumbnails_ThumbnailDragFinished(object sender, ThumbnailObject DragImageThumbnail, ThumbnailObject DropImageThumbnail)
        {
            Debug.WriteLine(String.Format("TableOfContentsThumbnails_ThumbnailDragFinished [{0}]->[{1}]", DragImageThumbnail, DropImageThumbnail));

            if (TableOfContentsImgEdit.PageCount > DragImageThumbnail.ThumbnailIndex)
            {
                if (DropImageThumbnail != null)
                {
                    TableOfContentsImgEdit.MovePage(DragImageThumbnail.ThumbnailIndex, DropImageThumbnail.ThumbnailIndex);
                    TableOfContentsImgEdit.SaveImage();
                }
            }
        }

        #endregion

        #endregion

        #region Menu FILE

        private void FileOpenButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CurrentBookInfoBox.IsBookLoaded)
            {
                MessageBox.Show("Není načten záznam publikace.", FileOpenButton.Header.ToString(), MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                this.Cursor = Cursors.Wait;

                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Title = this.FileOpenButton.ToolTip.ToString();
                dlg.InitialDirectory = Settings.Default.ScanFolderPath;
                dlg.Filter = "Obálka (*.jpg,*.jpeg)|*.jpg;*.jpeg|Obsah (*.tif,*.tiff)|*.tif;*.tiff";

                if (Convert.ToBoolean(dlg.ShowDialog(this)))
                {
                    string extension = System.IO.Path.GetExtension(dlg.FileName).ToLower();

                    switch (extension)
                    {
                        case ".jpg":
                        case ".jpeg":
                            LoadMainImgEdit(PartOfBook.FrontCover, dlg.FileName);
                            FrontCoverImgEdit.SaveImageAs(CurrentBookInfoBox.GetFullName(PartOfBook.FrontCover));
                            break;
                        case ".tif":
                        case ".tiff":
                            LoadMainImgEdit(PartOfBook.TableOfContents, dlg.FileName);
                            TableOfContentsImgEdit.SaveImageAs(CurrentBookInfoBox.GetFullName(PartOfBook.TableOfContents));
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
            finally
            {
                SetMenuButtonsEnabled();
                GetAvailableMemory();
                Logger.Log(FileOpenButton.Header.ToString());
                this.Cursor = null;
            }
        }

        private void FileSaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentImgEdit == null || CurrentImgEdit.PageCount == 0 || !SelectedPartOfBook.HasValue) return;

            if (!CurrentBookInfoBox.CanImageChanged(SelectedPartOfBook))
            {
                MessageBox.Show("Naskenovaný obrázek nelze uložit!", FileSaveButton.Header.ToString(), MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                this.Cursor = Cursors.Wait;
                SaveMainImgEdit(SelectedPartOfBook.Value);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
            finally
            {
                SetMenuButtonsEnabled();
                GetAvailableMemory();
                Logger.Log(FileSaveButton.Header.ToString());
                this.Cursor = null;
            }
        }

        private void FileRefreshMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedCatalogue == null)
            {
                MessageBox.Show("Není nastaven katalog.", FileRefreshMenuItem.Header.ToString(), MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!CurrentBookInfoBox.IsBookLoaded)
            {
                MessageBox.Show("Není načten žádný záznam publikace.", FileRefreshMenuItem.Header.ToString(), MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                this.Cursor = Cursors.Wait;
                RecoveryLastBook();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
            finally
            {
                e.Handled = true;
                SetMenuButtonsEnabled();
                GetAvailableMemory(true);
                Logger.Log(FileRefreshMenuItem.Header.ToString());
                this.Cursor = null;
            }
        }

        private void OptionsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Cursor = Cursors.Wait;
                OpenOptionsSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
            finally
            {
                this.Cursor = null;
            }
        }

        private void WebsiteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Cursor = Cursors.Wait;
                
#if DEBUG
                System.Diagnostics.Process.Start(@"http://localhost:12623/Default.aspx");
#else
                System.Diagnostics.Process.Start(Properties.Settings.Default.AppWebsiteUrl);;
#endif
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
            finally
            {
                this.Cursor = null;
            }
        }

        private void AboutAppMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Cursor = Cursors.Wait;
                AboutDialog dlg = new AboutDialog(this);
                dlg.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
            finally
            {
                this.Cursor = null;
            }
        }

        private void ExitAppMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Shutdown();
            }
            catch
            {
            }
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Cursor = Cursors.Wait;
#if DEBUG          
                System.Diagnostics.Process.Start(@"http://localhost:12623/Help/ScanHelp.aspx");
#else
                System.Diagnostics.Process.Start(String.Format("{0}/Help/ScanHelp.aspx", Properties.Settings.Default.AppWebsiteUrl));
#endif
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
            finally
            {
                SetMenuButtonsEnabled();
                GetAvailableMemory(true);
                this.Cursor = null;
            }
        }

        #endregion

        #region Menu HOME

        #region Group SCAN

        private void NewBookButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedCatalogue == null)
            {
                MessageBox.Show("Není nastaven katalog.", NewBookButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (CurrentBookInfoBox.IsBookLoaded && HasScannedImages)
            {
                if (MessageBox.Show("Naskenovaná publikace nebyla odeslána, chcete ji vymazat?", NewBookButton.Label, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    return;
            }

            try
            {
                this.Cursor = Cursors.Wait;
                ClearData();

                NewBookDialog dlg = new NewBookDialog(this);
                if (dlg.ShowDialog() == true)
                {
                    CurrentBookInfoBox.Load(dlg.NewBook);
                    DownloadCurrentBook();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
            finally
            {
                SetMenuButtonsEnabled();
                GetAvailableMemory(true);
                Logger.Log(NewBookButton.Label);
                this.Cursor = null;
            }
        }

        private void BrowseBookButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedCatalogue == null)
            {
                MessageBox.Show("Není nastaven katalog.", BrowseBookButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (CurrentBookInfoBox.IsBookLoaded && HasScannedImages)
            {
                if (MessageBox.Show("Naskenovaná publikace nebyla odeslána, chcete ji vymazat?", BrowseBookButton.Label, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    return;
            }

            try
            {
                this.Cursor = Cursors.Wait;
                ClearData();

                BrowseBookDialog dlg = new BrowseBookDialog(this, AuthController.UserIdentity.LoginUser.UserName);
                if (dlg.ShowDialog() == true)
                {
                    CurrentBookInfoBox.Load(dlg.SelectedBook);
                    DownloadCurrentBook();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
            finally
            {
                e.Handled = true;
                SetMenuButtonsEnabled();
                GetAvailableMemory(true);
                Logger.Log(BrowseBookButton.Label);
                this.Cursor = null;
            }
        }

        private void ScanFrontCoverButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentBookInfoBox.IsBookLoaded)
            {
                if (CurrentBookInfoBox.HasPartOfBook(PartOfBook.FrontCover))
                {
                    if (CurrentBookInfoBox.Book.FrontCover.Status == StatusCode.InProgress)
                    {
                        MessageBox.Show("Obálka je aktuálně zpracovávana, nelze skenovat!", ScanFrontCoverButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    else if (CurrentBookInfoBox.Book.FrontCover.Status == StatusCode.Complete)
                    {
                        if (MessageBox.Show("Obálka byla již odeslána na server ObalkyKnih.cz, chcete pokračovat?", ScanFrontCoverButton.Label, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                            return;
                    }
                    else if (CurrentBookInfoBox.Book.FrontCover.Status == StatusCode.Exported)
                    {
                        MessageBox.Show("Obálka byla již exportována do ALEPHu, nelze skenovat!", ScanFrontCoverButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                }
                else
                {
                    if (FrontCoverImgEdit.PageCount == 1)
                    {
                        MessageBox.Show("Obálka je již naskenována, nelze skenovat!", ScanFrontCoverButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                }
            }
            else
            {
                MessageBox.Show("Není načten záznam publikace.", ScanFrontCoverButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                this.Cursor = Cursors.Wait;
                Logger.Log(ScanFrontCoverButton.Label);
                Debug.WriteLine("========== ScanFrontCoverButton_Click ==========");

                FrontCoverImgScan.ActiveSourceName = Settings.Default.ScannerSourceName;
                FrontCoverImgScan.ShowScannerUI = Settings.Default.ScanShowScannerUI;
                FrontCoverImgScan.ShowIndicators = Settings.Default.ScanShowIndicators;
                FrontCoverImgScan.EnableMessageLoop = Settings.Default.ScanEnablePreview;
                FrontCoverImgScan.ImageFileDirectory = Settings.Default.ScanFolderPath;
                FrontCoverImgScan.ImageAcquireMode = ImgScan.ICImageAcquireMode.Auto;
                FrontCoverImgScan.SaveImageFormat = ImgScan.ICImageOutputFormat.JPEG;
                FrontCoverImgScan.SaveImageCompression = ImgScan.ICImageCompression.JPEG;
                FrontCoverImgScan.SaveJPGQuality = 80;
                FrontCoverImgScan.DeviceCapabilities.TwainICapabilities.ImageXferMech = Enumeration.Parse<ImgScan.ICImageTransferMode>(Settings.Default.ScanCapTransferMode);
                FrontCoverImgScan.DeviceCapabilities.TwainICapabilities.PixelTypeValue = Enumeration.Parse<ImgScan.ICTwainPixelTypes>(Settings.Default.ScanCoverPixelType);
                FrontCoverImgScan.DeviceCapabilities.TwainICapabilities.ResolutionValue = Settings.Default.ScanCoverResolution;
                FrontCoverImgScan.DeviceCapabilities.TwainICapabilities.PaperSizeValue = Enumeration.Parse<ImgScan.ICTwainSupportedSizes>(Settings.Default.ScannerPageSize);
                FrontCoverImgScan.DeviceCapabilities.TwainICapabilities.PaperOrientationValue = Enumeration.Parse<ImgScan.ICTwainOrientations>(Settings.Default.ScannerPageOrientation);

                FrontCoverImgScan.DeviceCapabilities.TwainCapabilities.IsAutoDeskewEnabled = Settings.Default.ScannerAutomaticDeskew;
                FrontCoverImgScan.DeviceCapabilities.TwainCapabilities.IsAutoBorderDetectionEnabled = Settings.Default.ScannerAutomaticBorderDetection;
                FrontCoverImgScan.DeviceCapabilities.TwainCapabilities.IsAutoRotationEnabled = Settings.Default.ScannerAutomaticRotate;
                FrontCoverImgScan.DeviceCapabilities.TwainICapabilities.IsAutoBrightEnabled = Settings.Default.ScannerAutomaticColorCorection;

                StartImageScan(PartOfBook.FrontCover);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
            finally
            {
                SetMenuButtonsEnabled();
                GetAvailableMemory();
                this.Activate();
                //this.Cursor = null;
            }
        }

        private void ScanTableOfContentsButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentBookInfoBox.IsBookLoaded)
            {
                if (CurrentBookInfoBox.HasPartOfBook(PartOfBook.TableOfContents))
                {
                    if (CurrentBookInfoBox.Book.TableOfContents.Status == StatusCode.InProgress)
                    {
                        MessageBox.Show("Obsah je aktuálně zpracovávan, nelze skenovat!", ScanTableOfContentsButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    else if (CurrentBookInfoBox.Book.TableOfContents.Status == StatusCode.Complete)
                    {
                        if (MessageBox.Show("Obsah je již OCR zpracován, chcete pokračovat?", ScanTableOfContentsButton.Label, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                            return;
                    }
                    else if (CurrentBookInfoBox.Book.TableOfContents.Status == StatusCode.Exported)
                    {
                        MessageBox.Show("Obsah byl již exportován do ALEPHu, nelze skenovat!", ScanTableOfContentsButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                }
            }
            else
            {
                MessageBox.Show("Není načten záznam publikace.", ScanTableOfContentsButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                this.Cursor = Cursors.Wait;
                Logger.Log(ScanTableOfContentsButton.Label);
                Debug.WriteLine("========== ScanTableOfContentsButton_Click ==========");

                bool bw = (Enumeration.Parse<TwainPixelTypes>(Settings.Default.ScanContentsPixelType).Equals(TwainPixelTypes.BW));

                TableOfContentsImgScan.ActiveSourceName = Settings.Default.ScannerSourceName;
                TableOfContentsImgScan.ShowScannerUI = Settings.Default.ScanShowScannerUI;
                TableOfContentsImgScan.ShowIndicators = Settings.Default.ScanShowIndicators;
                TableOfContentsImgScan.EnableMessageLoop = Settings.Default.ScanEnablePreview;
                TableOfContentsImgScan.ImageFileDirectory = Settings.Default.ScanFolderPath;
                TableOfContentsImgScan.ImageAcquireMode = ImgScan.ICImageAcquireMode.Append;
                TableOfContentsImgScan.SaveImageFormat = ImgScan.ICImageOutputFormat.TIFF;
                TableOfContentsImgScan.SaveImageCompression = (bw ? ImgScan.ICImageCompression.CCITT4 : ImgScan.ICImageCompression.LZW);
                TableOfContentsImgScan.DeviceCapabilities.TwainICapabilities.ImageXferMech = Enumeration.Parse<ImgScan.ICImageTransferMode>(Settings.Default.ScanCapTransferMode);
                TableOfContentsImgScan.DeviceCapabilities.TwainICapabilities.PixelTypeValue = Enumeration.Parse<ImgScan.ICTwainPixelTypes>(Settings.Default.ScanContentsPixelType);
                TableOfContentsImgScan.DeviceCapabilities.TwainICapabilities.ResolutionValue = Settings.Default.ScanContentsResolution;
                TableOfContentsImgScan.DeviceCapabilities.TwainICapabilities.PaperSizeValue = Enumeration.Parse<ImgScan.ICTwainSupportedSizes>(Settings.Default.ScannerPageSize);
                TableOfContentsImgScan.DeviceCapabilities.TwainICapabilities.PaperOrientationValue = Enumeration.Parse<ImgScan.ICTwainOrientations>(Settings.Default.ScannerPageOrientation);

                TableOfContentsImgScan.DeviceCapabilities.TwainCapabilities.IsAutoDeskewEnabled = Settings.Default.ScannerAutomaticDeskew;
                TableOfContentsImgScan.DeviceCapabilities.TwainCapabilities.IsAutoBorderDetectionEnabled = Settings.Default.ScannerAutomaticBorderDetection;
                TableOfContentsImgScan.DeviceCapabilities.TwainCapabilities.IsAutoRotationEnabled = Settings.Default.ScannerAutomaticRotate;
                TableOfContentsImgScan.DeviceCapabilities.TwainICapabilities.IsAutoBrightEnabled = Settings.Default.ScannerAutomaticColorCorection;

                StartImageScan(PartOfBook.TableOfContents);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
            finally
            {
                SetMenuButtonsEnabled();
                GetAvailableMemory();
                this.Activate();
                //this.Cursor = null;
            }
        }

        private void SendScanButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentBookInfoBox.Book == null)
            {
                MessageBox.Show("Není načten nový záznam publikace.", SendScanButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!HasScannedImages)
            {
                MessageBox.Show("Nebyla naskenovaná publikace pro odeslání.", SendScanButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                this.Cursor = Cursors.Wait;

                SaveMainImgEdit(PartOfBook.FrontCover);
                SaveMainImgEdit(PartOfBook.TableOfContents);

                SendScanDialog dlg = new SendScanDialog(this, CurrentBookInfoBox.Book, FrontCoverImgEdit.ImageFilePath, TableOfContentsImgEdit.ImageFilePath);
                if (dlg.ShowDialog() == true)
                {
                    MessageBox.Show("Naskenovaná publikace byla ÚSPĚŠNĚ odeslána.", SendScanButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
            finally
            {
                SetMenuButtonsEnabled();
                GetAvailableMemory(true);
                Logger.Log(SendScanButton.Label);
                this.Cursor = null;
            }
        }

        #endregion

        #region Group RECORD

        private void PropertiesButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentBookInfoBox.Book == null) return;

            try
            {
                this.Cursor = Cursors.Wait;
                MarcRecordDialog dlg = new MarcRecordDialog(this, CurrentBookInfoBox.Book);
                dlg.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
            finally
            {
                SetMenuButtonsEnabled();
                Logger.Log(PropertiesButton.Label);
                this.Cursor = null;
            }
        }

        #endregion

        #region Group ORDER

        private void ImageMoveForwardButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentImgEdit == null || CurrentImgEdit.PageCount == 0 || SelectedPartOfBook != PartOfBook.TableOfContents) return;

            if (CurrentBookInfoBox.IsBookLoaded)
            {
                if (CurrentBookInfoBox.IsImageExported(PartOfBook.TableOfContents))
                {
                    MessageBox.Show("Obsah byl již exportován do ALEPHu, stránku nelze přesunout!", ImageMoveForwardButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }
            else
            {
                MessageBox.Show("Není načten záznam publikace.", ImageMoveForwardButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                this.Cursor = Cursors.Wait;

                if (TableOfContentsImgEdit.PageCount > 0 && TableOfContentsImgEdit.Page > 0)
                {
                    TableOfContentsImgEdit.MovePage(TableOfContentsImgEdit.Page, TableOfContentsImgEdit.Page - 1);
                    TableOfContentsImgEdit.Page = TableOfContentsImgEdit.Page - 1;
                    CurrentBookInfoBox.SetImageChanged(PartOfBook.TableOfContents);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
            finally
            {
                SetMenuButtonsEnabled();
                Logger.Log(ImageMoveForwardButton.Label);
                this.Cursor = null;
            }
        }

        private void ImageMoveBackwardButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentImgEdit == null || CurrentImgEdit.PageCount == 0 || SelectedPartOfBook != PartOfBook.TableOfContents) return;

            if (CurrentBookInfoBox.IsBookLoaded)
            {
                if (CurrentBookInfoBox.IsImageExported(PartOfBook.TableOfContents))
                {
                    MessageBox.Show("Obsah byl již exportován do ALEPHu, stránku nelze přesunout!", ImageMoveBackwardButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }
            else
            {
                MessageBox.Show("Není načten záznam publikace.", ImageMoveBackwardButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                this.Cursor = Cursors.Wait;

                if (TableOfContentsImgEdit.PageCount > 0 && TableOfContentsImgEdit.Page < TableOfContentsImgEdit.PageCount - 1)
                {
                    TableOfContentsImgEdit.MovePage(TableOfContentsImgEdit.Page, TableOfContentsImgEdit.Page + 1);
                    TableOfContentsImgEdit.Page = TableOfContentsImgEdit.Page + 1;
                    CurrentBookInfoBox.SetImageChanged(PartOfBook.TableOfContents);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
            finally
            {
                SetMenuButtonsEnabled();
                Logger.Log(ImageMoveBackwardButton.Label);
                this.Cursor = null;
            }
        }

        private void ImageDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentImgEdit == null || CurrentImgEdit.PageCount == 0 || !SelectedPartOfBook.HasValue) return;

            if (CurrentBookInfoBox.IsBookLoaded)
            {
                if (SelectedPartOfBook.Value == PartOfBook.FrontCover)
                {
                    if (CurrentBookInfoBox.IsImageExported(PartOfBook.FrontCover))
                    {
                        MessageBox.Show("Obálka byl již exportována do ALEPHu, nelze odstranit!", ImageDeleteButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                }
                else
                {
                    if (CurrentBookInfoBox.IsImageExported(PartOfBook.TableOfContents))
                    {
                        MessageBox.Show("Obsah byl již exportován do ALEPHu, stránku nelze odstranit!", ImageDeleteButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                }
            }
            else
            {
                MessageBox.Show("Není načten záznam publikace.", ImageDeleteButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                this.Cursor = Cursors.Wait;

                if (MessageBox.Show("Chcete skutečně odstranit naskenovanou stránku?", ImageDeleteButton.Label, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    if (CurrentImgEdit.PageCount == 1)
                    {
                        File.Delete(CurrentImgEdit.ImageFilePath);
                        CurrentImgEdit.Close();

                        if (SelectedPartOfBook.Value == PartOfBook.FrontCover)
                            this.ObalkyKnihImage.Visibility = Visibility.Hidden;                        
                    }
                    else
                    {
                        CurrentImgEdit.DeletePage();
                        CurrentImgEdit.SaveImage();
                    }

                    CurrentBookInfoBox.SetImageChanged(SelectedPartOfBook);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
            finally
            {
                SetMenuButtonsEnabled();
                GetAvailableMemory();
                Logger.Log(ImageDeleteButton.Label);
                this.Cursor = null;
            }
        }

        #endregion

        #region Group TRANSFORM

        private void ImageRotateFlipButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentImgEdit == null || CurrentImgEdit.PageCount == 0) return;

            if (!CurrentBookInfoBox.CanImageChanged(SelectedPartOfBook))
            {
                MessageBox.Show("Naskenovaný obrázek nelze upravovat!", ImageRotateFlipButton.Header.ToString(), MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                this.Cursor = Cursors.Wait;
                CurrentImgEdit.Flip();
                CurrentBookInfoBox.SetImageChanged(SelectedPartOfBook);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
            finally
            {
                e.Handled = false;
                SetMenuButtonsEnabled();
                GetAvailableMemory();
                Logger.Log(ImageRotateFlipButton.Header.ToString());
                this.Cursor = null;
            }
        }

        private void ImageRotateLeftButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentImgEdit == null || CurrentImgEdit.PageCount == 0) return;

            if (!CurrentBookInfoBox.CanImageChanged(SelectedPartOfBook))
            {
                MessageBox.Show("Naskenovaný obrázek nelze upravovat!", ImageRotateLeftButton.Header.ToString(), MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                this.Cursor = Cursors.Wait;
                CurrentImgEdit.RotateLeft();
                CurrentBookInfoBox.SetImageChanged(SelectedPartOfBook);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
            finally
            {
                e.Handled = true;
                SetMenuButtonsEnabled();
                GetAvailableMemory();
                Logger.Log(ImageRotateLeftButton.Header.ToString());
                this.Cursor = null;
            }
        }

        private void ImageRotateRightButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentImgEdit == null || CurrentImgEdit.PageCount == 0) return;

            if (!CurrentBookInfoBox.CanImageChanged(SelectedPartOfBook))
            {
                MessageBox.Show("Naskenovaný obrázek nelze upravovat!", ImageRotateRightButton.Header.ToString(), MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                this.Cursor = Cursors.Wait;
                CurrentImgEdit.RotateRight();
                CurrentBookInfoBox.SetImageChanged(SelectedPartOfBook);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
            finally
            {
                e.Handled = true;
                SetMenuButtonsEnabled();
                GetAvailableMemory();
                Logger.Log(ImageRotateRightButton.Header.ToString());
                this.Cursor = null;
            }
        }

        private void ImageRotateAngleButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentImgEdit == null || CurrentImgEdit.PageCount == 0 || !SelectedPartOfBook.HasValue) return;

            if (!CurrentBookInfoBox.CanImageChanged(SelectedPartOfBook))
            {
                MessageBox.Show("Naskenovaný obrázek nelze upravovat!", ImageRotateAngleButton.Header.ToString(), MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                this.Cursor = Cursors.Wait;
                SaveMainImgEdit(SelectedPartOfBook.Value);

                RotateDialog dlg = new RotateDialog(this, CurrentImgEdit.ImageFilePath);
                if (dlg.ShowDialog() == true)
                {
                    LoadMainImgEdit(SelectedPartOfBook.Value, CurrentImgEdit.ImageFilePath, CurrentImgEdit.Page);
                    CurrentBookInfoBox.SetImageChanged(SelectedPartOfBook);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
            finally
            {
                e.Handled = true;
                SetMenuButtonsEnabled();
                GetAvailableMemory(true);
                Logger.Log(ImageRotateAngleButton.Header.ToString());
                this.Cursor = null;
            }
        }

        private void ImageDeskewButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentImgEdit == null || CurrentImgEdit.PageCount == 0) return;

            if (!CurrentBookInfoBox.CanImageChanged(SelectedPartOfBook))
            {
                MessageBox.Show("Naskenovaný obrázek nelze upravovat!", ImageDeskewButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                this.Cursor = Cursors.Wait;
                CurrentImgEdit.DeskewImage();
                CurrentBookInfoBox.SetImageChanged(SelectedPartOfBook);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
            finally
            {
                SetMenuButtonsEnabled();
                GetAvailableMemory();
                Logger.Log(ImageDeskewButton.Label);
                this.Cursor = null;
            }
        }

        private void ImageCropSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentImgEdit == null || CurrentImgEdit.PageCount == 0) return;

            if (!CurrentBookInfoBox.CanImageChanged(SelectedPartOfBook))
            {
                MessageBox.Show("Naskenovaný obrázek nelze upravovat!", ImageCropButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                this.Cursor = Cursors.Wait;
                CurrentImgEdit.EnableSelectionType = ImgEdit.ICSelectionType.CropSelection;
                CurrentBookInfoBox.SetImageChanged(SelectedPartOfBook);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
            finally
            {
                e.Handled = false;
                SetMenuButtonsEnabled();
                GetAvailableMemory();
                Logger.Log(ImageCropButton.Label);
                this.Cursor = null;
            }
        }

        private void ImageCropAutoButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentImgEdit == null || CurrentImgEdit.PageCount == 0) return;

            if (!CurrentBookInfoBox.CanImageChanged(SelectedPartOfBook))
            {
                MessageBox.Show("Naskenovaný obrázek nelze upravovat!", ImageCropAutoButton.Header.ToString(), MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                this.Cursor = Cursors.Wait;

                if (Settings.Default.ScanRemoveBlackBorders)
                    CurrentImgEdit.RemoveBorders();

                if (Settings.Default.ScannerBackColorBlack)
                    CurrentImgEdit.AutoCropImage(System.Windows.Media.Colors.Black, Settings.Default.ScannerBackColorTolerance);
                else
                    CurrentImgEdit.AutoCropImage();

                CurrentBookInfoBox.SetImageChanged(SelectedPartOfBook);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
            finally
            {
                e.Handled = true;
                SetMenuButtonsEnabled();
                GetAvailableMemory();
                Logger.Log(ImageCropAutoButton.Header.ToString());
                this.Cursor = null;
            }
        }

        private void ImageColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentImgEdit == null || CurrentImgEdit.PageCount == 0 || SelectedPartOfBook != PartOfBook.FrontCover) return;

            if (!CurrentBookInfoBox.CanImageChanged(SelectedPartOfBook))
            {
                MessageBox.Show("Naskenovaný obrázek nelze upravovat!", ImageColorButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                this.Cursor = Cursors.Wait;
                SaveMainImgEdit(SelectedPartOfBook.Value);

                //FrontCoverImgEdit.FilterBrightness(-10);

                ColorDialog dlg = new ColorDialog(this, FrontCoverImgEdit.ImageFilePath);
                if (dlg.ShowDialog() == true)
                {
                    //FrontCoverImgEdit.FilterBrightness(dlg.Brightness);
                    //FrontCoverImgEdit.FilterContrast((sbyte)dlg.Contrast);
                    //FrontCoverImgEdit.FilterGamma(dlg.Gamma, dlg.Gamma, dlg.Gamma);

                    LoadMainImgEdit(PartOfBook.FrontCover, FrontCoverImgEdit.ImageFilePath, FrontCoverImgEdit.Page);
                    CurrentBookInfoBox.SetImageChanged(PartOfBook.FrontCover);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
            finally
            {
                SetMenuButtonsEnabled();
                GetAvailableMemory(true);
                Logger.Log(ImageColorButton.Label);
                this.Cursor = null;
            }
        }

        private void ImageUndoButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentImgEdit == null || CurrentImgEdit.PageCount == 0) return;

            try
            {
                this.Cursor = Cursors.Wait;
                CurrentImgEdit.Undo();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
            finally
            {
                SetMenuButtonsEnabled();
                GetAvailableMemory();
                Logger.Log(ImageUndoButton.Label);
                this.Cursor = null;
            }
        }

        #endregion

        #endregion

        #region Menu VIEW

        #region Group NAVIGATE

        private void FrontCoverButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CurrentBookInfoBox.IsBookLoaded || FrontCoverImgEdit.PageCount == 0) return;

            try
            {
                this.Cursor = Cursors.Wait;
                ShowMainImgEdit(PartOfBook.FrontCover);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
            finally
            {
                SetMenuButtonsEnabled();
                GetAvailableMemory();
                this.Cursor = null;
            }
        }

        private void TableOfContentsButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CurrentBookInfoBox.IsBookLoaded || TableOfContentsImgEdit.PageCount == 0) return;

            try
            {
                this.Cursor = Cursors.Wait;
                ShowMainImgEdit(PartOfBook.TableOfContents);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
            finally
            {
                SetMenuButtonsEnabled();
                GetAvailableMemory();
                this.Cursor = null;
            }
        }

        private void PreviousPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CurrentBookInfoBox.IsBookLoaded || TableOfContentsImgEdit.PageCount == 0) return;

            try
            {
                this.Cursor = Cursors.Wait;
                ShowMainImgEdit(PartOfBook.TableOfContents);
                TableOfContentsImgEdit.PreviousPage();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
            finally
            {
                SetMenuButtonsEnabled();
                this.Cursor = null;
            }
        }

        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CurrentBookInfoBox.IsBookLoaded || TableOfContentsImgEdit.PageCount == 0) return;

            try
            {
                this.Cursor = Cursors.Wait;
                ShowMainImgEdit(PartOfBook.TableOfContents);
                TableOfContentsImgEdit.NextPage();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
            finally
            {
                SetMenuButtonsEnabled();
                this.Cursor = null;
            }
        }

        private void ShowMainImgEdit(PartOfBook? partOfBook)
        {
            FrontCoverImgEdit.Visibility = (partOfBook.HasValue && partOfBook.Value == PartOfBook.FrontCover ? Visibility.Visible : Visibility.Hidden);
            TableOfContentsImgEdit.Visibility = (partOfBook.HasValue && partOfBook.Value == PartOfBook.TableOfContents ? Visibility.Visible : Visibility.Hidden);
            SetMenuButtonsEnabled();
        }

        #endregion

        #region Group ZOOM

        private void ZoomInButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentImgEdit == null || CurrentImgEdit.PageCount == 0) return;

            try
            {
                this.Cursor = Cursors.Wait;
                FrontCoverImgEdit.MoreZoom();
                TableOfContentsImgEdit.MoreZoom();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
            finally
            {
                SetMenuButtonsEnabled();
                GetAvailableMemory();
                this.Cursor = null;
            }
        }

        private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentImgEdit == null || CurrentImgEdit.PageCount == 0) return;

            try
            {
                this.Cursor = Cursors.Wait;
                FrontCoverImgEdit.LessZoom();
                TableOfContentsImgEdit.LessZoom();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
            finally
            {
                SetMenuButtonsEnabled();
                GetAvailableMemory();
                this.Cursor = null;
            }
        }

        private void BestFitButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentImgEdit == null || CurrentImgEdit.PageCount == 0) return;

            try
            {
                this.Cursor = Cursors.Wait;
                FrontCoverImgEdit.FitTo(ImgEdit.ICImageFit.BestFit, true);
                TableOfContentsImgEdit.FitTo(ImgEdit.ICImageFit.BestFit, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
            finally
            {
                SetMenuButtonsEnabled();
                GetAvailableMemory();
                this.Cursor = null;
            }
        }

        private void ActualSizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentImgEdit == null || CurrentImgEdit.PageCount == 0) return;

            try
            {
                this.Cursor = Cursors.Wait;
                FrontCoverImgEdit.FitTo(ImgEdit.ICImageFit.OriginalSize, true);
                TableOfContentsImgEdit.FitTo(ImgEdit.ICImageFit.OriginalSize, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
            finally
            {
                SetMenuButtonsEnabled();
                GetAvailableMemory();
                this.Cursor = null;
            }
        }

        #endregion

        #endregion

        #region Menu OPTIONS

        #region Group SOURCE

        private void CatalogueMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var menuItem = (e.OriginalSource as MenuItem);

                if (menuItem != null)
                {
                    SetCatalogueMenuItem(Int32.Parse(menuItem.Tag.ToString()));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
            finally
            {
                SetMenuButtonsEnabled();
            }
        }

        private void SetCatalogueMenuItem(int catalogueID)
        {
            foreach (MenuItem item in this.CatalogueButton.Items)
            {
                item.IsChecked = (item.Tag.Equals(catalogueID));

                if (item.IsChecked)
                {
                    Settings.Default.SelectedCatalogueID = catalogueID;
                    break;
                }
            }
        }

        private void ScannerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var menuItem = (e.OriginalSource as MenuItem);

                if (menuItem != null)
                {
                    SetScannerMenuItem(menuItem.Header.ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
            finally
            {
                SetMenuButtonsEnabled();
            }
        }

        private void SetScannerMenuItem(string sourceName)
        {
            foreach (MenuItem item in this.ScannerButton.Items)
            {
                item.IsChecked = (item.Header.Equals(sourceName));

                if (item.IsChecked)
                {
                    Settings.Default.ScannerSourceName = sourceName;
                    break;
                }
            }
        }

        #endregion

        #region Group RESOLUTION

        #endregion

        #region Group SIZE

        private void ScannerPageSizeNoneButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetScannerPageSize(ImgScan.ICTwainSupportedSizes.None);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
        }

        private void ScannerPageSizeA4Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetScannerPageSize(ImgScan.ICTwainSupportedSizes.A4);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
        }

        private void ScannerPageSizeA5Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetScannerPageSize(ImgScan.ICTwainSupportedSizes.A5);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
        }

        private void SetScannerPageSize(ImgScan.ICTwainSupportedSizes size)
        {
            this.ScannerPageSizeNoneButton.IsChecked = (size == ImgScan.ICTwainSupportedSizes.None);
            this.ScannerPageSizeA4Button.IsChecked = (size == ImgScan.ICTwainSupportedSizes.A4);
            this.ScannerPageSizeA5Button.IsChecked = (size == ImgScan.ICTwainSupportedSizes.A5);
            Settings.Default.ScannerPageSize = size.ToString();
        }

        #endregion

        #region Group ORIENTATION

        private void ScannerPageOrientationPortraitButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetScannerPageOrientation(ImgScan.ICTwainOrientations.Portrait);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
        }

        private void ScannerPageOrientationLandscapeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetScannerPageOrientation(ImgScan.ICTwainOrientations.Landscape);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
        }

        private void SetScannerPageOrientation(ImgScan.ICTwainOrientations orientation)
        {
            this.ScannerPageOrientationPortraitButton.IsChecked = (orientation == ImgScan.ICTwainOrientations.Portrait);
            this.ScannerPageOrientationLandscapeButton.IsChecked = (orientation == ImgScan.ICTwainOrientations.Landscape);
            Settings.Default.ScannerPageOrientation = orientation.ToString();
        }

        #endregion

        #region Group AUTOMATIC

        private void ScannerAutomaticDeskewCheckBox_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    //Settings.Default.ScannerAutomaticDeskew = (ScannerAutomaticDeskewCheckBox.IsChecked ?? false);
            //    TwainSettings.Rotation.AutomaticDeskew = Settings.Default.ScannerAutomaticDeskew;
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
            //    Logger.Log(String.Format("ERROR: {0}", ex.Message));
            //}
        }

        private void ScannerAutomaticBorderDetectionCheckBox_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    //Settings.Default.ScannerAutomaticBorderDetection = (ScannerAutomaticBorderDetectionCheckBox.IsChecked ?? false);
            //    TwainSettings.Rotation.AutomaticBorderDetection = Settings.Default.ScannerAutomaticBorderDetection;
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
            //    Logger.Log(String.Format("ERROR: {0}", ex.Message));
            //}
        }

        private void ScannerAutomaticRotateCheckBox_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    //Settings.Default.ScannerAutomaticRotate = (ScannerAutomaticRotateCheckBox.IsChecked ?? false);
            //    TwainSettings.Rotation.AutomaticRotate = Settings.Default.ScannerAutomaticRotate;
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
            //    Logger.Log(String.Format("ERROR: {0}", ex.Message));
            //}
        }

        private void SetScannerAutomatic()
        {
            //this.ScannerAutomaticDeskewCheckBox.IsEnabled = Settings.Default.ScanCapAutoDeskew;
            //this.ScannerAutomaticBorderDetectionCheckBox.IsEnabled = Settings.Default.ScanCapAutoBorderDetection;
            //this.ScannerAutomaticRotateCheckBox.IsEnabled = Settings.Default.ScanCapAutoRotation;
            //this.AutomaticColorCorectionCheckBox.IsEnabled = Settings.Default.ScanCapAutoBrightness;
        }

        #endregion

        #endregion

        #region Book metrods

        private void DownloadCurrentBook()
        {
            if (!CurrentBookInfoBox.IsBookLoaded) return;

            if (CurrentBookInfoBox.HasPartOfBook(PartOfBook.FrontCover))
            {
                string frontCoverFilePath = CurrentBookInfoBox.GetFullName(PartOfBook.FrontCover);
                if (DozpController.GetScanImage(CurrentBookInfoBox.Book.FrontCover.ScanFileID, frontCoverFilePath))
                {
                    LoadMainImgEdit(PartOfBook.FrontCover, frontCoverFilePath);
                }
                else
                {
                    string message = String.Format("Nepodařilo se načíst naskenovaný soubor obálky SYSNO: '{0}' ze serveru.", CurrentBookInfoBox.Book.SysNo);
                    MessageBox.Show(message, "Download", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    Logger.Log(String.Format("WARNING: {0}", message));
                }
            }
            else
            {
                DownloadObalkyKnihCZ();
            }

            if (CurrentBookInfoBox.HasPartOfBook(PartOfBook.TableOfContents))
            {
                string tableOfContentsFilePath = CurrentBookInfoBox.GetFullName(PartOfBook.TableOfContents);
                if (DozpController.GetScanImage(CurrentBookInfoBox.Book.TableOfContents.ScanFileID, tableOfContentsFilePath))
                {
                    LoadMainImgEdit(PartOfBook.TableOfContents, tableOfContentsFilePath);
                }
                else
                {
                    string message = String.Format("Nepodařilo se načíst naskenovaný soubor obsahu SYSNO: '{0}' ze serveru.", CurrentBookInfoBox.Book.SysNo);
                    MessageBox.Show(message, "Download", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    Logger.Log(String.Format("WARNING: {0}", message));
                }
            }

            //if (CurrentBookInfoBox.HasPartOfBook(PartOfBook.FrontCover))
            //{
            ShowMainImgEdit(PartOfBook.FrontCover);
            //}
        }

        private void RecoveryLastBook()
        {
            if (Settings.Default.LastScanBook == null) return;

            CurrentBookInfoBox.Load(Settings.Default.LastScanBook);

            if (CurrentBookInfoBox.IsBookLoaded)
            {
                string frontCoverFilePath = CurrentBookInfoBox.GetFullName(PartOfBook.FrontCover);
                if (File.Exists(frontCoverFilePath))
                {
                    LoadMainImgEdit(PartOfBook.FrontCover, frontCoverFilePath);
                }

                string tableOfContentsFilePath = CurrentBookInfoBox.GetFullName(PartOfBook.TableOfContents);
                if (File.Exists(tableOfContentsFilePath))
                {
                    LoadMainImgEdit(PartOfBook.TableOfContents, tableOfContentsFilePath);
                }

                DownloadObalkyKnihCZ();
            }

            ShowMainImgEdit(PartOfBook.FrontCover);
        }

        private void DownloadObalkyKnihCZ()
        {
            try
            {
                if (Settings.Default.CheckObalkyKnihCZ)
                {
                    string frontCoverFilePath = System.IO.Path.Combine(Settings.Default.ScanFolderPath, OBALKY_KNIH_FILENAME);
                    if (DozpController.SearchCoverOK(SelectedCatalogue.ZServerUrl, CurrentBookInfoBox.Book, frontCoverFilePath))
                    {
                        ObalkyKnihImage.Visibility = Visibility.Visible;
                        LoadMainImgEdit(PartOfBook.FrontCover, frontCoverFilePath);
                    }
                }
            }
            catch (Exception ex)
            {
                //string message = "Nepodařilo se načíst obálku ze serveru ObalkyKnih.cz."
                MessageBox.Show(ex.Message, "ObalkyKnih.cz", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                Logger.Log(String.Format("WARNING: {0}", ex.Message));
            }
        }

        private void ClearData()
        {
            try
            {
                this.Cursor = null;
                CurrentBookInfoBox.Clear();
                FrontCoverImgEdit.Close();
                TableOfContentsImgEdit.Close();
                SetMenuButtonsEnabled();
                ImageFunctions.DeleteFiles(Settings.Default.ScanFolderPath);
                GetAvailableMemory(true);
                this.WorkingGifImage.Visibility = Visibility.Hidden;
                this.ObalkyKnihImage.Visibility = Visibility.Hidden;
            }
            catch
            {
            }
        }

        #endregion

        #region Scanner methods

        private void StartImageScan(PartOfBook partOfBook)
        {
            ShowMainImgEdit(partOfBook);

            try
            {
                if (CheckScannerSettings())
                {
                    StartTimer();
                    this.WorkingGifImage.Visibility = Visibility.Visible;

                    if (Settings.Default.ScanEnablePreview)
                    {
                        if (_scannerThread == null)
                        {
                            _scannerThread = new System.Threading.Thread(() =>
                            {
                                lock (_locker)
                                {
                                    switch (partOfBook)
                                    {
                                        case PartOfBook.FrontCover:
                                            FrontCoverImgScan.Acquire();
                                            break;
                                        case PartOfBook.TableOfContents:
                                            TableOfContentsImgScan.Acquire();
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            });
                            _scannerThread.SetApartmentState(System.Threading.ApartmentState.STA);
                            _scannerThread.IsBackground = true;
                            _scannerThread.Start();
                        }
                        else
                        {
                            if (_scannerThread.ThreadState == System.Threading.ThreadState.Stopped)
                            {
                                _scannerThread = null;
                            }
                            else
                            {
                                MessageBox.Show("Proces skenování nebyl ukončen.", "StartImageScan", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                                return;
                            }
                        }
                    }
                    else
                    {
                        switch (partOfBook)
                        {
                            case PartOfBook.FrontCover:
                                FrontCoverImgScan.Acquire();
                                break;
                            case PartOfBook.TableOfContents:
                                TableOfContentsImgScan.Acquire();
                                break;
                            default:
                                break;
                        }
                    }
                }
                else
                {
                    this.WorkingGifImage.Visibility = Visibility.Hidden;
                    this.Cursor = null;

                    OpenOptionsSettings();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void OpenOptionsSettings()
        {
            OptionsDialog dialog = new OptionsDialog(this);
            //dialog.ImgScanComponent = CurrentImgScan;

            if (Convert.ToBoolean(dialog.ShowDialog()))
            {
                SetCatalogueMenuItem(Settings.Default.SelectedCatalogueID);
                SetScannerMenuItem(Settings.Default.ScannerSourceName);
                SetScannerTrace(Settings.Default.AppLogging ? ImgScan.TraceOutputType.File : ImgScan.TraceOutputType.None);
                SetMainWindowTopmost();

                //UpdateScannerSettings();
                SetScannerAutomatic();
            }
        }

        private bool CheckScannerSettings()
        {
            if (String.IsNullOrEmpty(Settings.Default.ScannerSourceName))
            {
                MessageBox.Show("Není nastavený žádný skener.", this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }
            else
            {
                FrontCoverImgScan.ActiveSourceName = Settings.Default.ScannerSourceName;

                if (!FrontCoverImgScan.IsSourceActive(Settings.Default.ScannerSourceName))
                {
                    MessageBox.Show("Nastavený skener není aktivní.", this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return false;
                }
            }

            if (String.IsNullOrEmpty(Settings.Default.ScanFolderPath))
            {
                MessageBox.Show("Není nastavena složka pro ukládání naskenovaných souborů.", this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }
            else
            {
                if (!Directory.Exists(Settings.Default.ScanFolderPath))
                    Directory.CreateDirectory(Settings.Default.ScanFolderPath);
            }

            return true;
        }

        private void UpdateScannerSettings()
        {
            //TableOfContentsImgScan.ActiveSourceName = Settings.Default.ScannerSourceName;
            //TableOfContentsImgScan.ShowScannerUI = Settings.Default.ScanShowScannerUI;
            //TableOfContentsImgScan.EnableMessageLoop = Settings.Default.ScanEnablePreview;
            //TableOfContentsImgScan.ImageFileDirectory = Settings.Default.ScanFolderPath;
            //TableOfContentsImgScan.ImageAcquireMode = ImgScan.ICImageAcquireMode.Auto;
            //TableOfContentsImgScan.ImageBinarizationFilter = ImgScan.ICImageBinarizationFilterType.None;
            //TableOfContentsImgScan.SaveImageFormat = ImgScan.ICImageOutputFormat.DEFAULT;
            //TableOfContentsImgScan.SaveImageCompression = ImgScan.ICImageCompression.DEFAULT;
            //TableOfContentsImgScan.SaveFileNamePrefix = "Tmp";

            //TableOfContentsImgScan.DeviceCapabilities.TwainICapabilities.ImageXferMech = Enumeration.Parse<ImgScan.ICImageTransferMode>(Settings.Default.ScanCapTransferMode);
            //TableOfContentsImgScan.DeviceCapabilities.TwainICapabilities.ImageFileFormatValue = Enumeration.Parse<ImgScan.ICTwainImageFileFormats>(Settings.Default.ScanCapImageFileFormat);
            //TableOfContentsImgScan.DeviceCapabilities.TwainICapabilities.PixelTypeValue = Enumeration.Parse<ImgScan.ICTwainPixelTypes>(Settings.Default.ScanCapPixelType);
            //TableOfContentsImgScan.DeviceCapabilities.TwainICapabilities.ResolutionValue = (Settings.Default.ScanCapResolutions > 0 ? (int?)Settings.Default.ScanCapResolutions : null);
            //TableOfContentsImgScan.DeviceCapabilities.TwainICapabilities.PaperSizeValue = Enumeration.Parse<ImgScan.ICTwainSupportedSizes>(Settings.Default.ScanCapPaperSize);
            //TableOfContentsImgScan.DeviceCapabilities.TwainICapabilities.PaperOrientationValue = Enumeration.Parse<ImgScan.ICTwainOrientations>(Settings.Default.ScanCapPaperOrientation);
            //TableOfContentsImgScan.DeviceCapabilities.TwainICapabilities.BrightnessValue = (Settings.Default.ScanCapBrightness > 0 ?(int?)Settings.Default.ScanCapBrightness : null);
            //TableOfContentsImgScan.DeviceCapabilities.TwainICapabilities.ContrastValue = (Settings.Default.ScanCapContrast > 0 ? (int?)Settings.Default.ScanCapContrast : null);
            //TableOfContentsImgScan.DeviceCapabilities.TwainICapabilities.IsAutoBrightEnabled = Convert.ToBoolean(Settings.Default.ScanCapAutoBrightness);
            //TableOfContentsImgScan.DeviceCapabilities.TwainCapabilities.IsAutoDeskewEnabled = Convert.ToBoolean(Settings.Default.ScanCapAutoDeskew);
            //TableOfContentsImgScan.DeviceCapabilities.TwainCapabilities.IsAutoBorderDetectionEnabled = Convert.ToBoolean(Settings.Default.ScanCapAutoBorderDetection);
            //TableOfContentsImgScan.DeviceCapabilities.TwainCapabilities.IsAutoRotationEnabled = Convert.ToBoolean(Settings.Default.ScanCapAutoRotation);            
        }

        private void SetScannerTrace(ImgScan.TraceOutputType type)
        {
            this.FrontCoverImgScan.ErrorTraceOutputType = type;
            this.TableOfContentsImgScan.ErrorTraceOutputType = type;

            if (type == ImgScan.TraceOutputType.File)
            {
                string errorFilePath = System.IO.Path.Combine(Logger.LOG_PATH, String.Format("ScanLog{0:yyyyMMdd}.txt", DateTime.Now));
                this.FrontCoverImgScan.ErrorFilePath = errorFilePath;
                this.TableOfContentsImgScan.ErrorFilePath = errorFilePath;
            }
        }

        private Rect GetScannerArea(ImgScan.ICTwainSupportedSizes? size, ImgScan.ICTwainOrientations? orientation, int? dpi)
        {
            if (!size.HasValue || !orientation.HasValue || !dpi.HasValue) return Rect.Empty;

            Rect area = Rect.Empty;
            double margin = 0.1 * dpi.Value;
            double width = 0;
            double height = 0;

            switch (size.Value)
            {
                case ImgScan.ICTwainSupportedSizes.A4:
                    width = 8.27 * dpi.Value - 2 * margin;
                    height = 11.69 * dpi.Value - 2 * margin;
                    break;
                case ImgScan.ICTwainSupportedSizes.A5:
                    width = 5.83 * dpi.Value - 2 * margin;
                    height = 8.27 * dpi.Value - 2 * margin;
                    break;
                default:
                    break;
            }

            switch (orientation.Value)
            {
                case ImgScan.ICTwainOrientations.Portrait:
                    area = new Rect(margin, margin, width, height);
                    break;
                case ImgScan.ICTwainOrientations.Landscape:
                    area = new Rect(margin, margin, height, width);
                    break;
                default:
                    break;
            }

            return area;
        }

        #endregion

        #region Image methods

        private void LoadMainImgEdit(PartOfBook partOfBook, string imageFilePath, int thumbnailIndex = 0)
        {
            ShowMainImgEdit(partOfBook);

            if (String.IsNullOrEmpty(imageFilePath))
            {
                CurrentImgEdit.Close();
                return;
            }

            if (File.Exists(imageFilePath))
            {
                //if (imageFilePath != MainImgEdit.ImageFilePath)
                //{
                //    //MainImgEdit.Close();
                //    MainImgEdit.ImageFilePath = imageFilePath;
                //    MainImgEdit.Display();
                //}

                //if (MainImgEdit.Page != thumbnailIndex)
                //{
                //    MainImgEdit.Page = thumbnailIndex;
                //}

                CurrentImgEdit.ImageFilePath = imageFilePath;
                CurrentImgEdit.Display();
                CurrentImgEdit.Page = thumbnailIndex;
            }
            else
            {
                string message = String.Format("Soubor {0} neexistuje.", imageFilePath);
                MessageBox.Show(message, "LoadTableOfContentsImgEdit", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                Logger.Log(message);
            }
        }

        private void SaveMainImgEdit(PartOfBook partOfBook)
        {
            switch (partOfBook)
            {
                case PartOfBook.FrontCover:
                    if (HasScannedFrontCover)
                    {
                        int page = FrontCoverImgEdit.Page;
                        FrontCoverImgEdit.SaveImageFormat = ImgEdit.ICImageOutputFormat.JPEG;
                        FrontCoverImgEdit.SaveImageCompression = ImgEdit.ICImageCompression.JPEG;
                        FrontCoverImgEdit.SaveJPGQuality = 80;
                        FrontCoverImgEdit.SaveImageAs(FrontCoverImgEdit.ImageFilePath);
                        //FrontCoverImgEdit.SaveImage();
                        FrontCoverImgEdit.Page = page;
                    }
                    break;

                case PartOfBook.TableOfContents:
                    if (HasScannedTableOfContents)
                    {
                        bool bw = (Enumeration.Parse<TwainPixelTypes>(Settings.Default.ScanContentsPixelType).Equals(TwainPixelTypes.BW));
                        int page = TableOfContentsImgEdit.Page;
                        TableOfContentsImgEdit.SaveImageFormat = ImgEdit.ICImageOutputFormat.TIFF;
                        TableOfContentsImgEdit.SaveImageCompression = (bw ? ImgEdit.ICImageCompression.CCITT4 : ImgEdit.ICImageCompression.LZW);
                        //TableOfContentsImgEdit.SaveImageAs(TableOfContentsImgEdit.ImageFilePath);
                        TableOfContentsImgEdit.SaveImage();
                        TableOfContentsImgEdit.Page = page;
                    }
                    break;

                default:
                    break;
            }
        }

        protected void ImageComponentsErrorOccurred(string ControlName, string MethodName, string ErrorMessage)
        {
            string message = String.Format("Komponenta: {0}\nFunkce: {1}\nPopis: {2}", ControlName, MethodName, ErrorMessage);
            MessageBox.Show(this, message, "Chyba [Image Components]", MessageBoxButton.OK, MessageBoxImage.Error);
            Logger.Log(String.Format("ERROR: {0}.{1}->{2}", ControlName, MethodName, ErrorMessage));
        }

        #endregion

        #region Shortcuts methods

        private void InitializeShortcuts()
        {
            try
            {
                this.CommandBindings.Add(new CommandBinding(_newBookCommand, NewBookExecuted, NewBookCanExecute));
                this.InputBindings.Add(new InputBinding(_newBookCommand, new KeyGesture(Key.F2)));

                this.CommandBindings.Add(new CommandBinding(_scanFrontCoverCommand, ScanFrontCoverExecuted, ScanFrontCoverCanExecute));
                this.InputBindings.Add(new InputBinding(_scanFrontCoverCommand, new KeyGesture(Key.F3)));

                this.CommandBindings.Add(new CommandBinding(_scanTableOfContentsCommand, ScanTableOfContentsExecuted, ScanTableOfContentsCanExecute));
                this.InputBindings.Add(new InputBinding(_scanTableOfContentsCommand, new KeyGesture(Key.F4)));

                this.CommandBindings.Add(new CommandBinding(_sendScanCommand, SendScanExecuted, SendScanCanExecute));
                this.InputBindings.Add(new InputBinding(_sendScanCommand, new KeyGesture(Key.F9)));

                this.CommandBindings.Add(new CommandBinding(_imageDeleteCommand, ImageDeleteExecuted, ImageDeleteCanExecute));
                this.InputBindings.Add(new InputBinding(_imageDeleteCommand, new KeyGesture(Key.Delete)));

                this.CommandBindings.Add(new CommandBinding(_imageRotateAngleCommand, ImageRotateAngleExecuted, ImageRotateAngleCanExecute));
                this.InputBindings.Add(new InputBinding(_imageRotateAngleCommand, new KeyGesture(Key.R, ModifierKeys.Control)));

                this.CommandBindings.Add(new CommandBinding(_imageDeskewCommand, ImageDeskewExecuted, ImageDeskewCanExecute));
                this.InputBindings.Add(new InputBinding(_imageDeskewCommand, new KeyGesture(Key.V, ModifierKeys.Control)));

                this.CommandBindings.Add(new CommandBinding(_imageCropCommand, ImageCropExecuted, ImageCropCanExecute));
                this.InputBindings.Add(new InputBinding(_imageCropCommand, new KeyGesture(Key.O, ModifierKeys.Control)));

                this.CommandBindings.Add(new CommandBinding(_imageColorCommand, ImageColorExecuted, ImageColorCanExecute));
                this.InputBindings.Add(new InputBinding(_imageColorCommand, new KeyGesture(Key.B, ModifierKeys.Control)));

                this.CommandBindings.Add(new CommandBinding(_imageUndoButton, ImageUndoExecuted, ImageUndoCanExecute));
                this.InputBindings.Add(new InputBinding(_imageUndoButton, new KeyGesture(Key.Z, ModifierKeys.Control)));

                this.CommandBindings.Add(new CommandBinding(_frontCoverCommand, FrontCoverExecuted, FrontCoverCanExecute));
                this.InputBindings.Add(new InputBinding(_frontCoverCommand, new KeyGesture(Key.Home, ModifierKeys.Shift)));

                this.CommandBindings.Add(new CommandBinding(_previousPageCommand, PreviousPageExecuted, PreviousPageCanExecute));
                this.InputBindings.Add(new InputBinding(_previousPageCommand, new KeyGesture(Key.PageUp, ModifierKeys.Shift)));

                this.CommandBindings.Add(new CommandBinding(_nextPageCommand, NextPageExecuted, NextPageCanExecute));
                this.InputBindings.Add(new InputBinding(_nextPageCommand, new KeyGesture(Key.PageDown, ModifierKeys.Shift)));

                this.CommandBindings.Add(new CommandBinding(_scannerPageSizeNoneCommand, ScannerPageSizeNoneExecuted, ScannerPageSizeNoneCanExecute));
                this.InputBindings.Add(new InputBinding(_scannerPageSizeNoneCommand, new KeyGesture(Key.NumPad0, ModifierKeys.Control)));

                this.CommandBindings.Add(new CommandBinding(_scannerPageSizeA4Command, ScannerPageSizeA4Executed, ScannerPageSizeA4CanExecute));
                this.InputBindings.Add(new InputBinding(_scannerPageSizeA4Command, new KeyGesture(Key.NumPad4, ModifierKeys.Control)));

                this.CommandBindings.Add(new CommandBinding(_scannerPageSizeA5Command, ScannerPageSizeA5Executed, ScannerPageSizeA5CanExecute));
                this.InputBindings.Add(new InputBinding(_scannerPageSizeA5Command, new KeyGesture(Key.NumPad5, ModifierKeys.Control)));

                this.CommandBindings.Add(new CommandBinding(_optionsCommand, OptionsExecuted, OptionsCanExecute));
                this.InputBindings.Add(new InputBinding(_optionsCommand, new KeyGesture(Key.F12)));
            }
            catch
            {
            }
        }

        ////Nápověda (F1)
        //private void HelpExecuted(object sender, ExecutedRoutedEventArgs e)
        //{
        //    HelpButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        //}
        //private void HelpCanExecute(object sender, CanExecuteRoutedEventArgs e)
        //{
        //    e.CanExecute = true;
        //    e.Handled = true;
        //}

        //Nová publikace (F2)
        private void NewBookExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            //NewBookButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            NewBookButton_Click(NewBookButton, e);
        }
        private void NewBookCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = NewBookButton.IsEnabled;
            e.Handled = true;
        }

        //Skenovat obálku (F3)
        private void ScanFrontCoverExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ScanFrontCoverButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }
        private void ScanFrontCoverCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ScanFrontCoverButton.IsEnabled;
            e.Handled = true;
        }

        //Skenovat obsah (F4)
        private void ScanTableOfContentsExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ScanTableOfContentsButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }
        private void ScanTableOfContentsCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ScanTableOfContentsButton.IsEnabled;
            e.Handled = true;
        }

        //Odeslat na zpracování (F9)
        private void SendScanExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            SendScanButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }
        private void SendScanCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SendScanButton.IsEnabled;
            e.Handled = true;
        }

        //Odstranit obrázek (Del)
        private void ImageDeleteExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ImageDeleteButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }
        private void ImageDeleteCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ImageDeleteButton.IsEnabled;
            e.Handled = true;
        }

        //Otočit o úhel (Ctrl+R)
        private void ImageRotateAngleExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            //ImageRotateAngleButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            ImageRotateFlipButton_Click(ImageRotateAngleButton, e);
        }
        private void ImageRotateAngleCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ImageRotateFlipButton.IsEnabled;
            e.Handled = true;
        }

        //Vyrovnat (Ctrl+V)
        private void ImageDeskewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ImageDeskewButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }
        private void ImageDeskewCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ImageDeskewButton.IsEnabled;
            e.Handled = true;
        }

        //Oříznout (Ctrl+O)
        private void ImageCropExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            //ImageCropButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            ImageCropAutoButton_Click(ImageCropButton, e);
        }
        private void ImageCropCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ImageCropButton.IsEnabled;
            e.Handled = true;
        }

        //Barvy (Ctrl+B)
        private void ImageColorExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ImageColorButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }
        private void ImageColorCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ImageColorButton.IsEnabled;
            e.Handled = true;
        }

        //Vrátit zpět (Ctrl+Z)
        private void ImageUndoExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ImageUndoButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }
        private void ImageUndoCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ImageUndoButton.IsEnabled;
            e.Handled = true;
        }

        //Zobrazí obálku (Home)
        private void FrontCoverExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            FrontCoverButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }
        private void FrontCoverCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = FrontCoverButton.IsEnabled;
            e.Handled = true;
        }

        //Předchozí strana (PageUp)
        private void PreviousPageExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            PreviousPageButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }
        private void PreviousPageCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = PreviousPageButton.IsEnabled;
            e.Handled = true;
        }

        //Další strana (PageDown)
        private void NextPageExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            NextPageButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }
        private void NextPageCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = NextPageButton.IsEnabled;
            e.Handled = true;
        }

        //Výchozí velikost (Ctrl+0)
        private void ScannerPageSizeNoneExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ScannerPageSizeNoneButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }
        private void ScannerPageSizeNoneCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ScannerPageSizeNoneButton.IsEnabled;
            e.Handled = true;
        }

        //A4 formát (Ctrl+4)
        private void ScannerPageSizeA4Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ScannerPageSizeA4Button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }
        private void ScannerPageSizeA4CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ScannerPageSizeA4Button.IsEnabled;
            e.Handled = true;
        }

        //A5 formát (Ctrl+5)
        private void ScannerPageSizeA5Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ScannerPageSizeA5Button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }
        private void ScannerPageSizeA5CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ScannerPageSizeA5Button.IsEnabled;
            e.Handled = true;
        }

        //Nastavení (F12)
        private void OptionsExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            //OptionsMenuItem.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            OptionsMenuItem_Click(OptionsMenuItem, e);
        }
        private void OptionsCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = OptionsMenuItem.IsEnabled;
            e.Handled = true;
        }

        #endregion

        #region Private methods

        private void SetMainWindowTopmost()
        {
            this.Topmost = true;
            this.Activate();
            this.Topmost = Settings.Default.MainWindowTopmost;
        }

        private void SetMenuButtonsEnabled()
        {
            bool selectedImage = (CurrentImgEdit != null && CurrentImgEdit.PageCount > 0);
            bool tranformImage = selectedImage && !CurrentImgEdit.ImageFilePath.Contains(OBALKY_KNIH_FILENAME);
            bool hasScanner = !String.IsNullOrEmpty(Settings.Default.ScannerSourceName);

            this.RibbonMenu.IsEnabled = true;

            //File
            this.FileOpenButton.IsEnabled = CurrentBookInfoBox.IsBookLoaded;
            this.FileSaveButton.IsEnabled = tranformImage;
            this.FileRefreshMenuItem.IsEnabled = CurrentBookInfoBox.IsBookLoaded;

            //Scan
            this.NewBookButton.IsEnabled = (SelectedCatalogue != null);
            //this.BrowseBookButton.IsEnabled = this.NewBookButton.IsEnabled;
            this.ScanFrontCoverButton.IsEnabled = CurrentBookInfoBox.IsBookLoaded && (FrontCoverImgEdit.PageCount == 0);
            this.ScanTableOfContentsButton.IsEnabled = CurrentBookInfoBox.IsBookLoaded;
            this.SendScanButton.IsEnabled = CurrentBookInfoBox.IsBookLoaded && HasScannedImages;

            //Record
            this.PropertiesButton.IsEnabled = CurrentBookInfoBox.IsBookLoaded;

            //Order
            this.ImageMoveForwardButton.IsEnabled = tranformImage && (CurrentImgEdit.Page > 0);
            this.ImageMoveBackwardButton.IsEnabled = tranformImage && (CurrentImgEdit.Page < CurrentImgEdit.PageCount - 1);
            this.ImageDeleteButton.IsEnabled = selectedImage;

            //Transform
            this.ImageRotateButton.IsEnabled = tranformImage;
            this.ImageRotateAngleButton.IsEnabled = tranformImage && (SelectedPartOfBook == PartOfBook.FrontCover);
            this.ImageRotateLeftButton.IsEnabled = tranformImage;
            this.ImageRotateRightButton.IsEnabled = tranformImage;
            this.ImageRotateFlipButton.IsEnabled = tranformImage;
            this.ImageDeskewButton.IsEnabled = tranformImage;
            this.ImageCropButton.IsEnabled = tranformImage;
            this.ImageCropAutoButton.IsEnabled = tranformImage;
            this.ImageCropSelectionButton.IsEnabled = tranformImage;
            this.ImageColorButton.IsEnabled = tranformImage && (SelectedPartOfBook == PartOfBook.FrontCover);
            this.ImageUndoButton.IsEnabled = tranformImage;

            //Navigate
            this.FrontCoverButton.IsEnabled = (FrontCoverImgEdit.PageCount > 0);
            this.TableOfContentsButton.IsEnabled = (TableOfContentsImgEdit.PageCount > 0);
            this.PreviousPageButton.IsEnabled = this.TableOfContentsButton.IsEnabled && (TableOfContentsImgEdit.Page > 0);
            this.NextPageButton.IsEnabled = this.TableOfContentsButton.IsEnabled && (TableOfContentsImgEdit.Page < TableOfContentsImgEdit.PageCount - 1);

            //Zoom
            this.ZoomInButton.IsEnabled = selectedImage;
            this.ZoomOutButton.IsEnabled = selectedImage;
            this.BestFitButton.IsEnabled = selectedImage;
            this.ActualSizeButton.IsEnabled = selectedImage;

            //Options
            this.ScannerColorModeGroup.IsEnabled = hasScanner;
            this.ScannerResolutionGroup.IsEnabled = hasScanner;
            this.ScannerPageSizeGroup.IsEnabled = hasScanner;
            this.ScannerPageOrientationGroup.IsEnabled = hasScanner;
            this.ScannerAutomaticEditGroup.IsEnabled = hasScanner;
        }

        private void SetMessageStatusBar(string text = "")
        {
            this.MessageStatusBar.Text = text;
            App.DoEvents();
        }

        private void GetAvailableMemory(bool free = false)
        {
            try
            {
                if (free)
                {
                    _scannerThread = null;
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                }

                this.MemoryStatusBar.Text = String.Format("RAM: {0}", System.Diagnostics.Process.GetCurrentProcess().PrivateMemorySize64.ToFileSize());
            }
            catch
            {
            }
        }

        private void StartTimer()
        {
            if (!_timer.IsRunning)
            {
                _timer = new Stopwatch();
                _timer.Start();
            }
        }

        private void StopTimer()
        {
            _timer.Stop();
            this.TimerStatusBar.Text = String.Format("Čas: {0:s\\.ff}s", _timer.Elapsed);
            _timer.Reset();
        }

        #endregion
    }
}
