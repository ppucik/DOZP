using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Windows.Controls.Ribbon;

using TwainDotNet;
using TwainDotNet.Win32;
using TwainDotNet.Wpf;
using TwainDotNet.TwainNative;

using Comdat.DOZP.Core;
using Comdat.DOZP.Process;
using Comdat.ZRIS.Zoom;
using Comdat.ZRIS.Zoom.Marc21;

namespace Comdat.DOZP.Scan
{
    using Properties;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        #region Private members
        const string TWAIN_SCAN_FILENAME = "TwainScanImage.bmp";

        private Institution _institution = null;
        private Book _currentBook = null;
        private Twain _twain = null;
        private ScanSettings _twainSettings = null;
        private BitmapSource _scanBitmapSource = null;
        private ScanImages _scannedImages = null;
        private CroppingAdorner _cropper = null;
        private Rect _cropZone = Rect.Empty;
        private Stopwatch _timer = null;

        private static RoutedCommand _helpCommand = new RoutedCommand();
        private static RoutedCommand _newBookCommand = new RoutedCommand();
        private static RoutedCommand _scanFrontCoverCommand = new RoutedCommand();
        private static RoutedCommand _scanTableOfContentsCommand = new RoutedCommand();
        private static RoutedCommand _sendScanCommand = new RoutedCommand();
        private static RoutedCommand _imageDeleteCommand = new RoutedCommand();
        private static RoutedCommand _imageRotateAngleCommand = new RoutedCommand();
        private static RoutedCommand _scannerPageSizeNoneCommand = new RoutedCommand();
        private static RoutedCommand _scannerPageSizeA4Command = new RoutedCommand();
        private static RoutedCommand _scannerPageSizeA5Command = new RoutedCommand();
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
                this.Topmost = Settings.Default.AppAlwaysOnTop;

                if (Settings.Default.AppLogging)
                {
                    Logger.Open();
                    Logger.Log(String.Format("\n{0}, Version:{1}.{2}.{3} Build:{4}, UserName:{5}, MachineName:{6}", appName, version.Major, version.Minor, version.Revision, version.Build, fullName, Environment.MachineName));
                }

                this.ScannerColorModeCoverCategory.ItemsSource = Enumeration.GetList(typeof(ColourSetting));
                this.ScannerColorModeContentsCategory.ItemsSource = Enumeration.GetList(typeof(ColourSetting));
                this.ScannerResolutionCoverCategory.ItemsSource = ScanImage.RESOLUTIONS;
                this.ScannerResolutionContentsCategory.ItemsSource = ScanImage.RESOLUTIONS;

                this.DataContext = this;

                Loaded += delegate
                {
                    try
                    {
                        _twain = new Twain(new WpfWindowMessageHook(this));
                        _twain.TransferImage += TwainTransferImage;
                        _twain.ScanningComplete += TwainScanningComplete;
                        foreach (var name in _twain.SourceNames)
                        {
                            RibbonMenuItem item = new RibbonMenuItem();
                            item.Style = (LayoutRoot.Resources["RibbonMenuItemCheckable"] as Style);
                            item.IsChecked = (name == Settings.Default.ScannerSourceName);
                            item.Header = name;
                            item.Click += ScannerMenuItem_Click;
                            this.ScannerButton.Items.Add(item);
                        }
                    }
                    catch (TwainException tex)
                    {
                        string message = tex.Message + "\n[Pravdepodobně není nainstalován ovladač pro skener]";
                        MessageBox.Show(message, "TWAIN", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        Logger.Log(String.Format("TWAIN: {0}", message));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                        Logger.Log(String.Format("ERROR: {0}", ex.Message));
                    }
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
        }

        private void InitializeShortcuts()
        {
            try
            {
                CommandBinding cb = new CommandBinding(_helpCommand, HelpExecuted, HelpCanExecute);
                this.CommandBindings.Add(cb);
                KeyGesture kg = new KeyGesture(Key.F1);
                InputBinding ib = new InputBinding(_helpCommand, kg);
                this.InputBindings.Add(ib);

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

                this.CommandBindings.Add(new CommandBinding(_scannerPageSizeNoneCommand, ScannerPageSizeNoneExecuted, ScannerPageSizeNoneCanExecute));
                this.InputBindings.Add(new InputBinding(_scannerPageSizeNoneCommand, new KeyGesture(Key.NumPad0, ModifierKeys.Control)));

                this.CommandBindings.Add(new CommandBinding(_scannerPageSizeA4Command, ScannerPageSizeA4Executed, ScannerPageSizeA4CanExecute));
                this.InputBindings.Add(new InputBinding(_scannerPageSizeA4Command, new KeyGesture(Key.NumPad4, ModifierKeys.Control)));

                this.CommandBindings.Add(new CommandBinding(_scannerPageSizeA5Command, ScannerPageSizeA5Executed, ScannerPageSizeA5CanExecute));
                this.InputBindings.Add(new InputBinding(_scannerPageSizeA5Command, new KeyGesture(Key.NumPad5, ModifierKeys.Control)));
            }
            catch
            {
            }
        }

        ~MainWindow()
        {
            _institution = null;
            _currentBook = null;
            _twain = null;
            _twainSettings = null;
            _scanBitmapSource = null;
            _cropper = null;
            _scannedImages = null;
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

        private Book CurrentBook
        {
            get
            {
                return _currentBook;
            }
            set
            {
                _currentBook = value;
            }
        }

        private BitmapSource ScanBitmapSource
        {
            get
            {
                if (_scanBitmapSource == null && MainScanImage != null)
                {
                    _scanBitmapSource = MainScanImage.GetOriginal();
                }

                return _scanBitmapSource;
            }
            set
            {
                if (_scanBitmapSource != value)
                {
                    _scanBitmapSource = value;
                }
            }
        }

        public ScanImages ScannedImages
        {
            get
            {
                if (_scannedImages == null)
                {
                    _scannedImages = new ScanImages(Settings.Default.ScanFolderPath);
                    _scannedImages.CoverDpi = Settings.Default.ScannerResolutionCover;
                    _scannedImages.CoverBW = (Settings.Default.ScannerColorModeCover == ColourSetting.BlackAndWhite);
                    _scannedImages.ContentsDpi = Settings.Default.ScannerResolutionContents;
                    _scannedImages.ContentsBW = (Settings.Default.ScannerColorModeContents == ColourSetting.BlackAndWhite);
                }

                return _scannedImages;
            }
            set
            {
                _scannedImages = value;
            }
        }

        public ScanImage SelectedScanImage
        {
            get
            {
                return (this.ScanImageListView.SelectedItem as ScanImage);
            }
            set
            {
                this.ScanImageListView.SelectedItem = value;
            }
        }

        public ScanImage MainScanImage
        {
            get
            {
                return this.MainScanImageControl.ScanImage;
            }
            set
            {
                this.MainScanImageControl.ScanImage = value;
            }
        }

        private CroppingAdorner Cropper
        {
            get
            {
                return _cropper;
            }
            set
            {
                _cropper = value;
            }
        }

        private Rect CropZone
        {
            get
            {
                return _cropZone;
            }
            set
            {
                _cropZone = value;
            }
        }

        private Stopwatch Timer
        {
            get 
            {
                if (_timer == null)
                    _timer = new Stopwatch();

                return _timer; 
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

                //upgrade nastaveni pro novou verzi
                if (Settings.Default.UpgradeSettings)
                {
                    Settings.Default.Upgrade();
                    Settings.Default.UpgradeSettings = false;
                    Settings.Default.Save();
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

                SetScannerPageSize(Settings.Default.ScannerPageSize);
                SetScannerPageOrientation(Settings.Default.ScannerPageOrientation);

                //kontrola nastaveni
                if (!String.IsNullOrEmpty(Settings.Default.ScanFolderPath))
                {
                    if (!Directory.Exists(Settings.Default.ScanFolderPath))
                        Directory.CreateDirectory(Settings.Default.ScanFolderPath);
                }
                else
                {
                    string message = "Není nastavena složka pro ukládání naskenovaných souborů.";
                    MessageBox.Show(message, this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    Logger.Log(String.Format("WARNING: {0}", message));
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
                GetAvailableMemory(false);
                this.Cursor = null;
            }
        }

        private void RibbonWindow_ContentRendered(object sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.Wait;

                //obnoveni po padu aplikace
                if (Settings.Default.LastScanBook != null)
                {
                    string sysno = Settings.Default.LastScanBook.SysNo;
                    string message = String.Format("Publikace SYSNO: {0} nebyla pravděpodobně dokončena, chcete načíst znova publikaci a obnovit soubory?", sysno);

                    if (MessageBox.Show(message, this.Title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        RecoveryLastBook();
                    else
                        ClearData(); // Settings.Default.LastScanBook = null;
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
                SetBookInfoPanel();
                GetAvailableMemory();
                this.Cursor = null;
            }
        }

        private void RibbonWindow_Closed(object sender, EventArgs e)
        {
            Settings.Default.MainWindowMax = (this.WindowState == WindowState.Maximized);
            Settings.Default.Save();

            Logger.Log("END Application");
        }

        private void ScanImageListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.AddedItems.Count == 1)
                {
                    ScanImageListView.ScrollIntoView(e.AddedItems[0]);

                    if (e.RemovedItems.Count == 1 && CropZone != Rect.Empty)
                    {
                        double nextScanDpi = (double)(e.AddedItems[0] as ScanImage).Dpi.Value;
                        double prevScanDpi = (double)(e.RemovedItems[0] as ScanImage).Dpi.Value;
                        double ratio = nextScanDpi / prevScanDpi;
                        CropZone = new Rect(CropZone.X * ratio, CropZone.Y * ratio, CropZone.Width * ratio, CropZone.Height * ratio);
                    }
                }
            }
            catch
            {
            }

            try
            {
                this.Cursor = Cursors.Wait;

                if (MainScanImageControl.ScanImage != SelectedScanImage)
                {
                    MainScanImageControl.Save(_scanBitmapSource);
                    MainScanImageControl.ScanImage = SelectedScanImage;
                    ScanBitmapSource = null;
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
                SetScannedImagesStatusBar();
                GetAvailableMemory();
                Logger.Log("Změna výběru obrázku");
                this.Cursor = null;
            }
        }

        private void MainScanImageControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Rect newCropZone = Rect.Empty;

            try
            {
                if (CropZone != Rect.Empty && MainScanImage != null && MainScanImage.OriginalSize != Size.Empty)
                {
                    double ratioOriginX = (double)(e.NewSize.Width / MainScanImage.OriginalSize.Width);
                    double ratioOriginY = (double)(e.NewSize.Height / MainScanImage.OriginalSize.Height);

                    Point newCropZonePos = new Point(ratioOriginX * CropZone.X, ratioOriginY * CropZone.Y);
                    Size newCropZoneSize = new Size(ratioOriginX * CropZone.Width, ratioOriginY * CropZone.Height);

                    //limit to position of image
                    if (newCropZoneSize.Width + newCropZonePos.X > e.NewSize.Width)
                        newCropZonePos.X = e.NewSize.Width - newCropZoneSize.Width;

                    if (newCropZoneSize.Height + newCropZonePos.Y > e.NewSize.Height)
                        newCropZonePos.Y = e.NewSize.Height - newCropZoneSize.Height;

                    //limit to size of image
                    if (newCropZoneSize.Width + newCropZonePos.X > e.NewSize.Width)
                        newCropZoneSize.Width = e.NewSize.Width - newCropZonePos.X;

                    if (newCropZoneSize.Height + newCropZonePos.Y > e.NewSize.Height)
                        newCropZoneSize.Height = e.NewSize.Height - newCropZonePos.Y;

                    //new crop zone of image
                    newCropZone = new Rect(newCropZonePos, newCropZoneSize);
                }
            }
            catch
            {
            }

            AddCroperToMainScanImage(newCropZone);
        }

        private void MainScanImageControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this.ImageCropButton.IsEnabled && (e.ChangedButton == MouseButton.Left && e.ClickCount == 2))
            {
                ImageCropButton_Click(this.ImageCropButton, null);
            }
        }

        #endregion

        #region TWAIN

        private Twain Twain
        {
            get
            {
                return _twain;
            }
        }

        private ScanSettings TwainSettings
        {
            get
            {
                if (_twainSettings == null)
                {
                    _twainSettings = new ScanSettings
                    {
                        UseDocumentFeeder = false,
                        ShouldTransferAllPages = true,
                        ShowTwainUI = Settings.Default.ScannerShowTwainUI,
                        ShowProgressIndicatorUI = Settings.Default.ScannerProgressIndicatorUI,
                        Resolution = ResolutionSettings.Fax,

                        //Area = AreaSettings,
                        Page = new PageSettings
                        {
                            Size = Settings.Default.ScannerPageSize,
                            Orientation = Settings.Default.ScannerPageOrientation
                        },

                        Rotation = new RotationSettings
                        {
                            AutomaticDeskew = Settings.Default.ScannerAutomaticDeskew,
                            AutomaticBorderDetection = Settings.Default.ScannerAutomaticBorderDetection,
                            AutomaticRotate = Settings.Default.ScannerAutomaticRotate
                        }
                    };
                }

                return _twainSettings;
            }
            set
            {
                _twainSettings = value;
            }
        }

        private void TwainStartScanning()
        {
            if (String.IsNullOrEmpty(Settings.Default.ScannerSourceName))
            {
                MessageBox.Show("Není nastavený žádný skener.", this.NewBookButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (Twain == null || !Twain.SourceNames.Contains(Settings.Default.ScannerSourceName))
            {
                MessageBox.Show("Nastavený skener není připojený.", this.NewBookButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            Timer.Restart();

            try
            {
                Logger.Log(String.Format("TWAIN: TwainStartScanning {0}", Settings.Default.ScannerSourceName));
                SetControlsDisabled();
                Twain.SelectSource(Settings.Default.ScannerSourceName);
                Twain.StartScanning(TwainSettings);
            }
            catch (TwainException tx)
            {
                string message = String.Format("{0}\nConditionCode={1}\nReturnCode={2}", tx.Message, tx.ConditionCode, tx.ReturnCode);
                MessageBox.Show(message, tx.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("TWAIN: {0}", message));
                SetMenuButtonsEnabled();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
                SetMenuButtonsEnabled();
            }
            finally
            {
                GetAvailableMemory();
                Logger.Log("TwainStartScanning-Finally");
            }
        }

        private void TwainTransferImage(object sender, TransferImageEventArgs e)
        {
            if (MainScanImage == null) throw new ApplicationException("Není nastavený objekt pro ulození naskenovného obrázku.");

            try
            {
                if (e.Image != null)
                {
                    if (Settings.Default.ScannerSaveToFile)
                    {
                        Logger.Log(String.Format("TwainTransferImage-FILE"));
                        //System.Drawing.Bitmap bmp = BitmapFunctions.AutoColorCorrections(e.Image);
                        //Logger.Log(String.Format("TwainTransferImage-AutoColorCorrections"));
                        e.Image.Save(System.IO.Path.Combine(Settings.Default.ScanFolderPath, TWAIN_SCAN_FILENAME), System.Drawing.Imaging.ImageFormat.Bmp);
                    }
                    else
                    {
                        Logger.Log(String.Format("TwainTransferImage-MEMORY"));
                        IntPtr hbitmap = new System.Drawing.Bitmap(e.Image).GetHbitmap();
                        Logger.Log(String.Format("TwainTransferImage-GetHbitmap"));
                        ScanBitmapSource = Imaging.CreateBitmapSourceFromHBitmap(hbitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                        Logger.Log(String.Format("TwainTransferImage-CreateBitmapSource"));
                        Gdi32Native.DeleteObject(hbitmap);
                    }
                }
                else
                {
                    string message = "Nepodařilo se naskenovat obrázek.";
                    MessageBox.Show(message, "TwainTransferImage", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    Logger.Log(String.Format("ERROR: {0}", message));
                }
            }
            catch (TwainException tx)
            {
                string message = String.Format("{0}\nConditionCode={1}\nReturnCode={2}.", tx.Message, tx.ConditionCode, tx.ReturnCode);
                MessageBox.Show(message, tx.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("TWAIN: {0}", message));
                SetMenuButtonsEnabled();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
                SetMenuButtonsEnabled();
            }
            finally
            {
                GetAvailableMemory();
                Logger.Log("TwainTransferImage-Finally");
            }
        }

        private void TwainScanningComplete(object sender, ScanningCompleteEventArgs e)
        {
            if (ScannedImages == null) throw new ApplicationException("Není inicializována kolekce pro uložení naskenovaných obrázků.");

            try
            {
                if (e.Exception == null)
                {
                    if (Settings.Default.ScannerSaveToFile)
                    {
                        Logger.Log(String.Format("TwainScanningComplete-FILE"));
                        string fileName = System.IO.Path.Combine(Settings.Default.ScanFolderPath, TWAIN_SCAN_FILENAME);

                        if (File.Exists(fileName))
                        {
                            ScanBitmapSource = ImageFunctions.Load(fileName);
                            Logger.Log("TwainScanningComplete-Load");
                        }
                        else
                        {
                            string message = String.Format("Nepodařilo se načíst naskenovaný soubor {0}.", TWAIN_SCAN_FILENAME);
                            Logger.Log(String.Format("TWAIN: {0}", message));
                        }
                    }
                    else
                    {
                        Logger.Log(String.Format("TwainScanningComplete-MEMORY"));
                    }

                    if (ScanBitmapSource != null)
                    {
                        if (Settings.Default.AutoRotateEvenPage && ScannedImages.IsNextEvenPage(MainScanImage.IsCover))
                        {
                            ScanBitmapSource = ImageFunctions.Rotate180(ScanBitmapSource);
                            Logger.Log(String.Format("TwainScanningComplete-Rotate180"));
                        }

                        if (Settings.Default.AutomaticColorCorection && !MainScanImage.BlackAndWhite)
                        {
                            ScanBitmapSource = ImageFunctions.AutoColorCorrections(ScanBitmapSource);
                            Logger.Log(String.Format("TwainScanningComplete-AutoColorCorrections"));
                        }

                        MainScanImage.Dpi = TwainSettings.Resolution.Dpi;
                        MainScanImage.BlackAndWhite = (TwainSettings.Resolution.ColourSetting == ColourSetting.BlackAndWhite);
                        MainScanImage.Save(ScanBitmapSource);
                        Logger.Log(String.Format("TwainScanningComplete-Save"));

                        ScannedImages.Add(MainScanImage);
                        SelectedScanImage = MainScanImage;
                        //ScanBitmapSource = null;
                    }
                    else
                    {
                        string message = "Nepodařilo se naskenovat obrázek.";
                        MessageBox.Show(message, "TwainScanningComplete", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        Logger.Log(String.Format("TWAIN: {0}", message));
                    }
                }
                else
                {
                    MessageBox.Show(e.Exception.Message, e.Exception.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                    Logger.Log(String.Format("TWAIN: {0}", e.Exception.Message));
                }
            }
            catch (TwainException tx)
            {
                string message = String.Format("{0}\nConditionCode={1}\nReturnCode={2}.", tx.Message, tx.ConditionCode, tx.ReturnCode);
                MessageBox.Show(message, tx.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("TWAIN: {0}", message));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
            finally
            {
                SetMenuButtonsEnabled();
                SetBookInfoPanel();
                GetAvailableMemory();
                Logger.Log("TwainScanningComplete-Finally");
            }

            Timer.Stop();
            SetTimetatusBar(Timer.Elapsed);
        }

        #endregion

        #region BOOK

        private void DownloadCurrentBook()
        {
            if (CurrentBook == null) return;

            if (CurrentBook.HasPartOfBook(PartOfBook.FrontCover))
            {
                ScanImage cover = ScannedImages.GetNewScanImage(true);
                if (DozpController.GetScanImage(CurrentBook.FrontCover.ScanFileID, cover.FullName))
                {
                    ScannedImages.Add(cover);
                }
                else
                {
                    string message = String.Format("Nepodařilo se načíst naskenovaný soubor obálky SYSNO: '{0}' ze serveru.", CurrentBook.SysNo);
                    MessageBox.Show(message, "Download", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    Logger.Log(String.Format("WARNING: {0}", message));
                }
            }
            else
            {
                DownloadObalkyKnihCZ();
            }

            if (CurrentBook.HasPartOfBook(PartOfBook.TableOfContents))
            {
                string tifPath = System.IO.Path.Combine(ScannedImages.ScanFolderPath, CurrentBook.TableOfContents.FileName);
                if (DozpController.GetScanImage(CurrentBook.TableOfContents.ScanFileID, tifPath))
                {
                    ScannedImages.LoadContents(CurrentBook.TableOfContents.FileName);
                }
                else
                {
                    string message = String.Format("Nepodařilo se načíst naskenovaný soubor obsahu SYSNO: '{0}' ze serveru.", CurrentBook.SysNo);
                    MessageBox.Show(message, "Download", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    Logger.Log(String.Format("WARNING: {0}", message));
                }
            }

            SelectFirstScanImage();
        }

        private void DownloadObalkyKnihCZ()
        {
            try
            {
                if (Settings.Default.CheckObalkyKnihCZ && !ScannedImages.HasCover)
                {
                    string coverUrl = DozpController.SearchCoverUrl(SelectedCatalogue.ZServerUrl, CurrentBook);

                    if (!String.IsNullOrEmpty(coverUrl))
                    {
                        ScanImage cover = new ScanImage(coverUrl, true);
                        cover.BlackAndWhite = false;
                        ScannedImages.Add(cover);
                        App.DoEvents();
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

        private void RecoveryLastBook()
        {
            if (Settings.Default.LastScanBook == null) return;

            if (Settings.Default.LastScanBook.BookID != 0)
                CurrentBook = DozpController.GetBook(Settings.Default.LastScanBook.BookID); //server
            else
                CurrentBook = new Book(Settings.Default.LastScanBook);  //local

            ScannedImages.ScanFileName = CurrentBook.GetFileName();
            ScanImageListView.ItemsSource = ScannedImages;

            ScannedImages.Repair();
            DownloadObalkyKnihCZ();
            //DownloadCurrentBook();
            SelectFirstScanImage();
        }

        private void ClearData()
        {
            CropZone = Rect.Empty;
            CurrentBook = null;
            ScanBitmapSource = null;

            if (ScannedImages != null)
            {
                ScannedImages.Delete();
                ScannedImages = null;
            }

            SetBookInfoPanel();
            ScanImageListView.ItemsSource = ScannedImages;
            MainScanImageControl.Clear();
            Settings.Default.LastScanBook = null;
        }

        #endregion

        #region Menu FILE

        private void OptionsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            try
            {
                OptionsDialog dlg = new OptionsDialog(this);
                if (dlg.ShowDialog() == true)
                {
                    this.Topmost = Settings.Default.AppAlwaysOnTop;
                    TwainSettings = null;
                }
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
            this.Cursor = Cursors.Wait;

            try
            {
                System.Diagnostics.Process.Start(Properties.Settings.Default.AppWebsiteUrl);
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
            this.Cursor = Cursors.Wait;

            try
            {
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

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Cursor = Cursors.Wait;

                MessageBox.Show(HelpButton.ToolTipDescription, HelpButton.ToolTipTitle);
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

        private void ExitAppMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
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

            if (ScannedImages != null && (ScannedImages.HasCoverNoUrl || ScannedImages.HasContents))
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
                    CurrentBook = dlg.NewBook;
                    ScannedImages.ScanFileName = CurrentBook.GetFileName();
                    DownloadCurrentBook();

                    Settings.Default.LastScanBook = CurrentBook.GetSettings();
                    Settings.Default.Save();
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
                SetBookInfoPanel();
                GetAvailableMemory();
                Logger.Log(NewBookButton.Label);
                this.Cursor = null;
            }
        }

        private void BrowseBookMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedCatalogue == null)
            {
                MessageBox.Show("Není nastaven katalog.", BrowseBookMenuItem.Header.ToString(), MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (ScannedImages != null && (ScannedImages.HasCoverNoUrl || ScannedImages.HasContents))
            {
                if (MessageBox.Show("Naskenovaná publikace nebyla odeslána, chcete ji vymazat?", BrowseBookMenuItem.Header.ToString(), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    return;
            }

            try
            {
                this.Cursor = Cursors.Wait;
                ClearData();

                BrowseBookDialog dlg = new BrowseBookDialog(this);
                if (dlg.ShowDialog() == true)
                {
                    CurrentBook = dlg.SelectedBook;
                    ScannedImages.ScanFileName = CurrentBook.GetFileName();
                    DownloadCurrentBook();

                    Settings.Default.LastScanBook = CurrentBook.GetSettings();
                    Settings.Default.Save();
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
                SetBookInfoPanel();
                GetAvailableMemory();
                Logger.Log(BrowseBookMenuItem.Header.ToString());
                this.Cursor = null;
            }
        }

        private void RefreshBookMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedCatalogue == null)
            {
                MessageBox.Show("Není nastaven katalog.", RefreshBookMenuItem.Header.ToString(), MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (CurrentBook == null)
            {
                MessageBox.Show("Není načten žádný záznam publikace.", RefreshBookMenuItem.Header.ToString(), MessageBoxButton.OK, MessageBoxImage.Information);
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
                SetBookInfoPanel();
                GetAvailableMemory();
                this.Cursor = null;
            }
        }

        private void ScanFrontCoverButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentBook != null)
            {
                if (CurrentBook.HasPartOfBook(PartOfBook.FrontCover))
                {
                    if (CurrentBook.FrontCover.Status == StatusCode.InProgress)
                    {
                        MessageBox.Show("Obálka je aktuálně zpracovávana, nelze skenovat!", ScanFrontCoverButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    else if (CurrentBook.FrontCover.Status == StatusCode.Complete)
                    {
                        if (MessageBox.Show("Obálka byla již odeslána na server ObalkyKnih.cz, chcete pokračovat?", ScanFrontCoverButton.Label, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                            return;
                    }
                    else if (CurrentBook.FrontCover.Status == StatusCode.Exported)
                    {
                        MessageBox.Show("Obálka byla již exportována do ALEPHu, nelze skenovat!", ScanFrontCoverButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
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

                MainScanImageControl.Save(_scanBitmapSource);
                MainScanImageControl.ScanImage = ScannedImages.GetNewScanImage(true);
                TwainSettings.Resolution.ColourSetting = Settings.Default.ScannerColorModeCover;
                TwainSettings.Resolution.Dpi = Settings.Default.ScannerResolutionCover;

                TwainStartScanning();

                CurrentBook.SetImageChanged(PartOfBook.FrontCover);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
            finally
            {
                //SetMenuButtonsEnabled();
                GetAvailableMemory();
                this.Cursor = null;
            }
        }

        private void ScanTableOfContentsButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentBook != null)
            {
                if (CurrentBook.HasPartOfBook(PartOfBook.TableOfContents))
                {
                    if (CurrentBook.TableOfContents.Status == StatusCode.InProgress)
                    {
                        MessageBox.Show("Obsah je aktuálně zpracovávan, nelze skenovat!", ScanTableOfContentsButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    else if (CurrentBook.TableOfContents.Status == StatusCode.Complete)
                    {
                        if (MessageBox.Show("Obsah je již OCR zpracován, chcete pokračovat?", ScanTableOfContentsButton.Label, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                            return;
                    }
                    else if (CurrentBook.TableOfContents.Status == StatusCode.Exported)
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

                MainScanImageControl.Save(_scanBitmapSource);
                MainScanImageControl.ScanImage = ScannedImages.GetNewScanImage();
                TwainSettings.Resolution.ColourSetting = Settings.Default.ScannerColorModeContents;
                TwainSettings.Resolution.Dpi = Settings.Default.ScannerResolutionContents;

                TwainStartScanning();

                CurrentBook.SetImageChanged(PartOfBook.TableOfContents);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
            finally
            {
                //SetMenuButtonsEnabled();
                GetAvailableMemory();
                this.Cursor = null;
            }
        }

        private void SendScanButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentBook == null)
            {
                MessageBox.Show("Není načten nový záznam publikace.", SendScanButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (ScannedImages == null || ScannedImages.Count == 0)
            {
                MessageBox.Show("Není naskenovaná publikace.", SendScanButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                this.Cursor = Cursors.Wait;

                //ulozit aktualni naskenovany soubor
                MainScanImageControl.Save(_scanBitmapSource);

                ScannedImages.CoverDpi = Settings.Default.ScannerResolutionCover;
                ScannedImages.CoverBW = (Settings.Default.ScannerColorModeCover == ColourSetting.BlackAndWhite);
                ScannedImages.ContentsDpi = Settings.Default.ScannerResolutionContents;
                ScannedImages.ContentsBW = (Settings.Default.ScannerColorModeContents == ColourSetting.BlackAndWhite);

                SendScanDialog dlg = new SendScanDialog(this, CurrentBook, ScannedImages);
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
                GetAvailableMemory();
                Logger.Log(SendScanButton.Label);
                this.Cursor = null;
            }
        }

        #endregion

        #region Group RECORD

        private void PropertiesButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentBook == null) return;

            try
            {
                this.Cursor = Cursors.Wait;
                MarcRecordDialog dlg = new MarcRecordDialog(this, CurrentBook);
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

        #region Group IMAGE

        private void ImageMoveForwardButton_Click(object sender, RoutedEventArgs e)
        {
            if (ScannedImages == null || SelectedScanImage == null || SelectedScanImage.IsCover) return;

            if (CurrentBook != null)
            {
                if (CurrentBook.HasPartOfBook(PartOfBook.TableOfContents) && CurrentBook.TableOfContents.Status == StatusCode.Exported)
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
                ScannedImages.MoveUp(SelectedScanImage);
                CurrentBook.SetImageChanged(PartOfBook.TableOfContents);
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
            if (ScannedImages == null || SelectedScanImage == null || SelectedScanImage.IsCover) return;

            if (CurrentBook != null)
            {
                if (CurrentBook.HasPartOfBook(PartOfBook.TableOfContents) && CurrentBook.TableOfContents.Status == StatusCode.Exported)
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
                ScannedImages.MoveDown(SelectedScanImage);
                CurrentBook.SetImageChanged(PartOfBook.TableOfContents);
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
            if (ScannedImages == null || SelectedScanImage == null) return;

            if (CurrentBook != null)
            {
                if (SelectedScanImage.IsCover)
                {
                    if (CurrentBook.HasPartOfBook(PartOfBook.FrontCover) && CurrentBook.FrontCover.Status == StatusCode.Exported)
                    {
                        MessageBox.Show("Obálka byl již exportována do ALEPHu, nelze odstranit!", ImageDeleteButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                }
                else
                {
                    if (CurrentBook.HasPartOfBook(PartOfBook.TableOfContents) && CurrentBook.TableOfContents.Status == StatusCode.Exported)
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
                    CurrentBook.SetImageChanged(SelectedScanImage.IsCover ? PartOfBook.FrontCover : PartOfBook.TableOfContents);
                    ScannedImages.Remove(SelectedScanImage);
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

        private void ImageRotateAngleButton_Click(object sender, RoutedEventArgs e)
        {
            if (ScanBitmapSource == null) return;

            if (!CurrentBook.CanImageChanged(SelectedScanImage.IsCover ? PartOfBook.FrontCover : PartOfBook.TableOfContents))
            {
                MessageBox.Show("Naskenovaný obrázek nelze upravovat!", ImageRotateAngleButton.Header.ToString(), MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                this.Cursor = Cursors.Wait;

                RotateDialog dlg = new RotateDialog(this, SelectedScanImage);
                if (dlg.ShowDialog() == true)
                {
                    ScanBitmapSource = ImageFunctions.Deskew(ScanBitmapSource, dlg.Angle);
                    MainScanImageControl.Refresh(ScanBitmapSource);
                    CurrentBook.SetImageChanged(SelectedScanImage.IsCover ? PartOfBook.FrontCover : PartOfBook.TableOfContents);
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
                Logger.Log(ImageRotateAngleButton.Header.ToString());
                this.Cursor = null;
            }
        }

        private void ImageRotateLeftButton_Click(object sender, RoutedEventArgs e)
        {
            if (ScanBitmapSource == null) return;

            if (!CurrentBook.CanImageChanged(SelectedScanImage.IsCover ? PartOfBook.FrontCover : PartOfBook.TableOfContents))
            {
                MessageBox.Show("Naskenovaný obrázek nelze upravovat!", ImageRotateLeftButton.Header.ToString(), MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                this.Cursor = Cursors.Wait;
                ScanBitmapSource = ImageFunctions.RotateLeft(ScanBitmapSource);
                MainScanImageControl.Refresh(ScanBitmapSource);
                CurrentBook.SetImageChanged(SelectedScanImage.IsCover ? PartOfBook.FrontCover : PartOfBook.TableOfContents);
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
            if (ScanBitmapSource == null) return;

            if (!CurrentBook.CanImageChanged(SelectedScanImage.IsCover ? PartOfBook.FrontCover : PartOfBook.TableOfContents))
            {
                MessageBox.Show("Naskenovaný obrázek nelze upravovat!", ImageRotateRightButton.Header.ToString(), MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                this.Cursor = Cursors.Wait;
                ScanBitmapSource = ImageFunctions.RotateRight(ScanBitmapSource);
                MainScanImageControl.Refresh(ScanBitmapSource);
                CurrentBook.SetImageChanged(SelectedScanImage.IsCover ? PartOfBook.FrontCover : PartOfBook.TableOfContents);
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

        private void ImageRotate180Button_Click(object sender, RoutedEventArgs e)
        {
            if (ScanBitmapSource == null) return;

            if (!CurrentBook.CanImageChanged(SelectedScanImage.IsCover ? PartOfBook.FrontCover : PartOfBook.TableOfContents))
            {
                MessageBox.Show("Naskenovaný obrázek nelze upravovat!", ImageRotate180Button.Header.ToString(), MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                this.Cursor = Cursors.Wait;
                ScanBitmapSource = ImageFunctions.Rotate180(ScanBitmapSource);
                MainScanImageControl.Refresh(ScanBitmapSource);
                CurrentBook.SetImageChanged(SelectedScanImage.IsCover ? PartOfBook.FrontCover : PartOfBook.TableOfContents);
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
                Logger.Log(ImageRotate180Button.Header.ToString());
                this.Cursor = null;
            }
        }

        private void ImageFlipButton_Click(object sender, RoutedEventArgs e)
        {
            if (ScanBitmapSource == null) return;

            if (!CurrentBook.CanImageChanged(SelectedScanImage.IsCover ? PartOfBook.FrontCover : PartOfBook.TableOfContents))
            {
                MessageBox.Show("Naskenovaný obrázek nelze upravovat!", ImageFlipButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                this.Cursor = Cursors.Wait;
                ScanBitmapSource = ImageFunctions.FlipHorizontal(ScanBitmapSource);
                MainScanImageControl.Refresh(ScanBitmapSource);
                CurrentBook.SetImageChanged(SelectedScanImage.IsCover ? PartOfBook.FrontCover : PartOfBook.TableOfContents);
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
                Logger.Log(ImageFlipButton.Label);
                this.Cursor = null;
            }
        }

        private void ImageDeskewButton_Click(object sender, RoutedEventArgs e)
        {
            if (ScanBitmapSource == null) return;

            if (!CurrentBook.CanImageChanged(SelectedScanImage.IsCover ? PartOfBook.FrontCover : PartOfBook.TableOfContents))
            {
                MessageBox.Show("Naskenovaný obrázek nelze upravovat!", ImageDeskewButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                this.Cursor = Cursors.Wait;
                float angle = ImageFunctions.GetDeskewAngle(MainScanImageControl.Source as BitmapSource);
                ScanBitmapSource = ImageFunctions.Deskew(ScanBitmapSource);
                MainScanImageControl.Refresh(ScanBitmapSource);
                CurrentBook.SetImageChanged(SelectedScanImage.IsCover ? PartOfBook.FrontCover : PartOfBook.TableOfContents);
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

        private void ImageCropButton_Click(object sender, RoutedEventArgs e)
        {
            if (ScanBitmapSource == null) return;

            if (!CurrentBook.CanImageChanged(SelectedScanImage.IsCover ? PartOfBook.FrontCover : PartOfBook.TableOfContents))
            {
                MessageBox.Show("Naskenovaný obrázek nelze upravovat!", ImageCropButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                this.Cursor = Cursors.Wait;
                CropZone = Cropper.GetCropZone(ScanBitmapSource.PixelWidth, ScanBitmapSource.PixelHeight);
                ScanBitmapSource = ImageFunctions.Crop(ScanBitmapSource, CropZone);
                MainScanImageControl.Refresh(ScanBitmapSource);
                CurrentBook.SetImageChanged(SelectedScanImage.IsCover ? PartOfBook.FrontCover : PartOfBook.TableOfContents);
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
                Logger.Log(ImageCropButton.Label);
                this.Cursor = null;
            }
        }

        private void ImageColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (ScanBitmapSource == null) return;

            if (!CurrentBook.CanImageChanged(SelectedScanImage.IsCover ? PartOfBook.FrontCover : PartOfBook.TableOfContents))
            {
                MessageBox.Show("Naskenovaný obrázek nelze upravovat!", ImageColorButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                this.Cursor = Cursors.Wait;

                ColorDialog dlg = new ColorDialog(this, SelectedScanImage);
                if (dlg.ShowDialog() == true)
                {
                    ScanBitmapSource = ImageFunctions.ColorCorrections(ScanBitmapSource, dlg.Brightness, dlg.Contrast, dlg.Gamma, dlg.Hue, dlg.Saturation);
                    MainScanImageControl.Refresh(ScanBitmapSource);
                    CurrentBook.SetImageChanged(SelectedScanImage.IsCover ? PartOfBook.FrontCover : PartOfBook.TableOfContents);
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
                Logger.Log(ImageColorButton.Label);
                this.Cursor = null;
            }
        }

        private void ImageSaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ScanBitmapSource == null) return;

            try
            {
                this.Cursor = Cursors.Wait;
                MainScanImageControl.Save(ScanBitmapSource);
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
                Logger.Log(ImageSaveButton.Label);
                this.Cursor = null;
            }
        }

        private void ImageUndoButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainScanImage == null) return;

            try
            {
                this.Cursor = Cursors.Wait;
                ScanBitmapSource = MainScanImage.GetOriginal();
                MainScanImageControl.Refresh(ScanBitmapSource);
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

        private void ZoomInButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Cursor = Cursors.Wait;
                MessageBox.Show(ZoomInButton.ToolTipDescription, ZoomInButton.Label);
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

        private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Cursor = Cursors.Wait;
                MessageBox.Show(ZoomOutButton.ToolTipDescription, ZoomOutButton.Label);
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

        private void BestFitButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Cursor = Cursors.Wait;
                MessageBox.Show(BestFitButton.ToolTipDescription, BestFitButton.Label);
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

        private void ActualSizeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Cursor = Cursors.Wait;
                MessageBox.Show(ActualSizeButton.ToolTipDescription, ActualSizeButton.Label);
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
                    Settings.Default.SelectedCatalogueID = Int32.Parse(menuItem.Tag.ToString());
                    foreach (MenuItem item in this.CatalogueButton.Items)
                    {
                        item.IsChecked = (item.Tag == menuItem.Tag);
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
            }
        }

        private void ScannerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var menuItem = (e.OriginalSource as MenuItem);

                if (menuItem != null)
                {
                    Settings.Default.ScannerSourceName = menuItem.Header.ToString();
                    foreach (MenuItem item in this.ScannerButton.Items)
                    {
                        item.IsChecked = (item.Header == menuItem.Header);
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
                SetScannerPageSize(TwainDotNet.TwainNative.PageType.None);
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
                SetScannerPageSize(TwainDotNet.TwainNative.PageType.A4);
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
                SetScannerPageSize(TwainDotNet.TwainNative.PageType.A5);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
        }

        #endregion

        #region Group ORIENTATION

        private void ScannerPageOrientationPortraitButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetScannerPageOrientation(TwainDotNet.TwainNative.Orientation.Portrait);
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
                SetScannerPageOrientation(TwainDotNet.TwainNative.Orientation.Landscape);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
        }

        #endregion

        #region Group AUTOMATIC

        private void ScannerAutomaticDeskewCheckBox_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Settings.Default.ScannerAutomaticDeskew = (ScannerAutomaticDeskewCheckBox.IsChecked ?? false);
                TwainSettings.Rotation.AutomaticDeskew = Settings.Default.ScannerAutomaticDeskew;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
        }

        private void ScannerAutomaticBorderDetectionCheckBox_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Settings.Default.ScannerAutomaticBorderDetection = (ScannerAutomaticBorderDetectionCheckBox.IsChecked ?? false);
                TwainSettings.Rotation.AutomaticBorderDetection = Settings.Default.ScannerAutomaticBorderDetection;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
        }

        private void ScannerAutomaticRotateCheckBox_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Settings.Default.ScannerAutomaticRotate = (ScannerAutomaticRotateCheckBox.IsChecked ?? false);
                TwainSettings.Rotation.AutomaticRotate = Settings.Default.ScannerAutomaticRotate;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
        }

        #endregion

        #endregion

        #region Command bindings methods

        //Nápověda (F1)
        private void HelpExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            HelpButton_Click(HelpButton, null);
        }
        private void HelpCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        //Nová publikace (F2)
        private void NewBookExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.NewBookButton.IsEnabled)
            {
                NewBookButton_Click(NewBookButton, null);
            }
        }
        private void NewBookCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        //Skenovat obálku (F3)
        private void ScanFrontCoverExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.ScanFrontCoverButton.IsEnabled)
            {
                //this.ScanFrontCoverButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                ScanFrontCoverButton_Click(ScanFrontCoverButton, null);
            }
        }
        private void ScanFrontCoverCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        //Skenovat obsah (F4)
        private void ScanTableOfContentsExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.ScanTableOfContentsButton.IsEnabled)
            {
                ScanTableOfContentsButton_Click(ScanTableOfContentsButton, null);
            }
        }
        private void ScanTableOfContentsCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        //Odeslat na zpracování (F9)
        private void SendScanExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.SendScanButton.IsEnabled)
            {
                SendScanButton_Click(SendScanButton, null);
            }
        }
        private void SendScanCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        //Odstranit obrázek (Del)
        private void ImageDeleteExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.ImageDeleteButton.IsEnabled)
            {
                ImageDeleteButton_Click(ImageDeleteButton, null);
            }
        }
        private void ImageDeleteCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        //Otočit o úhel (Ctrl+R)
        private void ImageRotateAngleExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.ImageRotateAngleButton.IsEnabled)
            {
                ImageRotateAngleButton_Click(ImageRotateAngleButton, null);
            }
        }
        private void ImageRotateAngleCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        //Výchozí velikost (Ctrl+0)
        private void ScannerPageSizeNoneExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.ScannerPageSizeNoneButton.IsEnabled)
            {
                ScannerPageSizeNoneButton_Click(ScannerPageSizeNoneButton, null);
            }
        }
        private void ScannerPageSizeNoneCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        //A4 formát (Ctrl+4)
        private void ScannerPageSizeA4Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.ScannerPageSizeA4Button.IsEnabled)
            {
                ScannerPageSizeA4Button_Click(ScannerPageSizeA4Button, null);
            }
        }
        private void ScannerPageSizeA4CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        //A5 formát (Ctrl+5)
        private void ScannerPageSizeA5Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.ScannerPageSizeA5Button.IsEnabled)
            {
                ScannerPageSizeA5Button_Click(ScannerPageSizeA5Button, null);
            }
        }
        private void ScannerPageSizeA5CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        #endregion

        #region Cropping Adorner

        private void AddCroperToMainScanImage(Rect cropZone)
        {
            RemoveCropper(this.MainScanImageControl);

            if (this.MainScanImage != null)
            {
                AddCropperToElement(this.MainScanImageControl, cropZone);
            }
        }

        private void AddCropperToElement(FrameworkElement imageElement, Rect cropZone)
        {
            if (imageElement == null) return;

            //RemoveCropper(imageElement);

            Cropper = new CroppingAdorner(imageElement, cropZone);
            Cropper.CropChanged += CropChanged;

            AdornerLayer layer = AdornerLayer.GetAdornerLayer(imageElement);
            if (layer != null)
            {
                layer.Add(Cropper);
            }
        }

        private void CropChanged(Object sender, RoutedEventArgs rea)
        {
            if (Cropper != null)
            {
                Rect rc = Cropper.ClippingRectangle;
                //Debug.WriteLine(String.Format("[{0:N0}, {1:N0}] [{2:N0} x {3:N0}]", rc.Left, rc.Top, rc.Right - rc.Left, rc.Bottom - rc.Top));
            }
        }

        private void RemoveCropper(FrameworkElement imageElement)
        {
            if (imageElement == null || Cropper == null) return;

            AdornerLayer layer = AdornerLayer.GetAdornerLayer(imageElement);
            if (layer != null)
            {
                layer.Remove(Cropper);
            }

            //Cropper = null;
        }

        private void RemoveAdorners(FrameworkElement imageElement)
        {
            if (imageElement == null) return;

            AdornerLayer layer = AdornerLayer.GetAdornerLayer(imageElement);
            if (layer != null)
            {
                Adorner[] adorners = layer.GetAdorners(imageElement);
                if (adorners != null)
                {
                    foreach (var a in adorners)
                    {
                        layer.Remove(a);
                    }
                }
            }
        }

        #endregion

        #region Private methods

        private void SetMenuButtonsEnabled()
        {
            bool initialized = (CurrentBook != null);
            bool selectedImage = (ScannedImages != null && SelectedScanImage != null);
            bool tranformImage = selectedImage && !SelectedScanImage.IsExternalUrl;
            bool hasScanner = !String.IsNullOrEmpty(Settings.Default.ScannerSourceName);

            //bool editable = (initialized && selectedImage && CurrentBook.IsEditable(SelectedScanImage.IsCover ? PartOfBook.FrontCover : PartOfBook.TableOfContents));

            this.Ribbon.IsEnabled = true;
            this.ScanImageListView.IsEnabled = true;

            //Scan
            this.NewBookButton.IsEnabled = (SelectedCatalogue != null);
            this.NewBookMenuItem.IsEnabled = this.NewBookButton.IsEnabled;
            this.BrowseBookMenuItem.IsEnabled = this.NewBookButton.IsEnabled;
            this.RefreshBookMenuItem.IsEnabled = initialized;
            this.ScanFrontCoverButton.IsEnabled = initialized && !ScannedImages.HasCover;
            this.ScanTableOfContentsButton.IsEnabled = initialized;
            this.SendScanButton.IsEnabled = initialized && (ScannedImages.Count > 0);

            //Record
            this.PropertiesButton.IsEnabled = initialized;

            //Order
            this.ImageMoveForwardButton.IsEnabled = selectedImage && ScannedImages.CanMoveUp(SelectedScanImage);
            this.ImageMoveBackwardButton.IsEnabled = selectedImage && ScannedImages.CanMoveDown(SelectedScanImage);
            this.ImageDeleteButton.IsEnabled = selectedImage;

            //Tranform
            this.ImageRotateButton.IsEnabled = tranformImage;
            this.ImageRotate180Button.IsEnabled = tranformImage;
            this.ImageRotateLeftButton.IsEnabled = tranformImage;
            this.ImageRotateRightButton.IsEnabled = tranformImage;
            this.ImageRotateAngleButton.IsEnabled = tranformImage;
            this.ImageFlipButton.IsEnabled = tranformImage;
            this.ImageDeskewButton.IsEnabled = tranformImage;
            this.ImageCropButton.IsEnabled = tranformImage;
            this.ImageColorButton.IsEnabled = tranformImage && !SelectedScanImage.BlackAndWhite;
            this.ImageSaveButton.IsEnabled = tranformImage;
            this.ImageUndoButton.IsEnabled = tranformImage;

            //View
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

            this.Cursor = null;
        }

        private void SetControlsDisabled()
        {
            this.Ribbon.IsEnabled = false;
            this.ScanImageListView.IsEnabled = false;
        }

        private void SetBookInfoPanel()
        {
            if (CurrentBook != null)
            {
                this.BookSysNoTextBlock.Text = String.Format("SYSNO: {0}", CurrentBook.SysNo);
                this.BookTitleTextBlock.Text = CurrentBook.Publication;
            }
            else
            {
                this.BookSysNoTextBlock.Text = "SYSNO:";
                this.BookTitleTextBlock.Text = "";
            }
        }

        private void SelectFirstScanImage()
        {
            if (ScanImageListView.HasItems) ScanImageListView.SelectedIndex = 0;
        }

        private void SetScannerPageOrientation(TwainDotNet.TwainNative.Orientation orientation)
        {
            Settings.Default.ScannerPageOrientation = orientation;
            this.ScannerPageOrientationPortraitButton.IsChecked = (orientation == TwainDotNet.TwainNative.Orientation.Portrait);
            this.ScannerPageOrientationLandscapeButton.IsChecked = (orientation == TwainDotNet.TwainNative.Orientation.Landscape);
            TwainSettings.Page.Orientation = orientation;
        }

        private void SetScannerPageSize(TwainDotNet.TwainNative.PageType size)
        {
            Settings.Default.ScannerPageSize = size;
            this.ScannerPageSizeNoneButton.IsChecked = (size == TwainDotNet.TwainNative.PageType.None);
            this.ScannerPageSizeA4Button.IsChecked = (size == TwainDotNet.TwainNative.PageType.A4);
            this.ScannerPageSizeA5Button.IsChecked = (size == TwainDotNet.TwainNative.PageType.A5);
            TwainSettings.Area = null;
            TwainSettings.Page.Size = size;
        }

        private void SetScannedImagesStatusBar()
        {
            string thumnails = "";
            string message = "";

            try
            {
                if (ScannedImages == null || ScannedImages.Count == 0 || SelectedScanImage == null)
                {
                    thumnails = "žádný";
                }
                else
                {
                    if (SelectedScanImage.IsCover)
                        thumnails = SelectedScanImage.Page;
                    else
                        thumnails = String.Format("Obsah: {0}/{1}", SelectedScanImage.Page, ScannedImages.ContetsPages);

                    message = SelectedScanImage.FullName;
                }
            }
            catch
            {
            }

            this.ScannedImagesStatusBar.Text = thumnails;
            this.MessageStatusBar.Text = message;
        }

        private void SetMessageStatusBar(string text = "")
        {
            this.MessageStatusBar.Text = text;
        }

        private void SetTimetatusBar(TimeSpan elapsed)
        {
            this.TimerStatusBar.Text = String.Format("Čas: {0:s\\.ff}s", elapsed);
        }

        private void GetAvailableMemory(bool free = true)
        {
            try
            {
                if (free)
                {
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

        #endregion
    }
}
