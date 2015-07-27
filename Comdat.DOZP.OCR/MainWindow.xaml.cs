using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Windows.Controls.Ribbon;

using Comdat.DOZP.Core;
using Comdat.DOZP.Process;

namespace Comdat.DOZP.OCR
{
    using Properties;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        #region Private members
        private ScanFile _scanContents = null;
        private ScanImages _scannedImages = null;
        private OcrFile _ocrContents = null;

        const double ZOOM_MIN = 0.1;
        const double ZOOM_MAX = 4;
        const double ZOOM_STEP = 0.25;
        double _zoom = 1;

        private static RoutedCommand _helpCommand = new RoutedCommand();
        private static RoutedCommand _checkOutContentsCommand = new RoutedCommand();
        private static RoutedCommand _fineReaderCommand = new RoutedCommand();
        private static RoutedCommand _discardContentsCommand = new RoutedCommand();
        private static RoutedCommand _undoContentsCommand = new RoutedCommand();
        private static RoutedCommand _checkInContentsCommand = new RoutedCommand();
        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
            InitializeShortcuts();

            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            string appName = Properties.Resources.ApplicationName;
            string fullName = AuthController.UserIdentity.LoginUser.FullName;
            this.RibbonWindow.Title = String.Format("{0} v{1}.{2}.{3} [{4}]", appName, version.Major, version.Minor, version.Revision, fullName);
            this.WindowState = (Settings.Default.MainWindowMax ? WindowState.Maximized : WindowState.Normal);
        }

        ~MainWindow()
        {
            _scanContents = null;
            _scannedImages = null;
            _ocrContents = null;
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

                this.CommandBindings.Add(new CommandBinding(_checkOutContentsCommand, CheckOutContentsExecuted, CheckOutContentsCanExecute));
                this.InputBindings.Add(new InputBinding(_checkOutContentsCommand, new KeyGesture(Key.F2)));

                this.CommandBindings.Add(new CommandBinding(_fineReaderCommand, FineReaderCommandExecuted, FineReaderCanExecute));
                this.InputBindings.Add(new InputBinding(_fineReaderCommand, new KeyGesture(Key.F5)));

                this.CommandBindings.Add(new CommandBinding(_discardContentsCommand, DiscardContentsExecuted, DiscardContentsCanExecute));
                this.InputBindings.Add(new InputBinding(_discardContentsCommand, new KeyGesture(Key.F6)));

                this.CommandBindings.Add(new CommandBinding(_undoContentsCommand, UndoContentsExecuted, UndoContentsCanExecute));
                this.InputBindings.Add(new InputBinding(_undoContentsCommand, new KeyGesture(Key.F7)));

                this.CommandBindings.Add(new CommandBinding(_checkInContentsCommand, CheckInContentsExecuted, CheckInContentsCanExecute));
                this.InputBindings.Add(new InputBinding(_checkInContentsCommand, new KeyGesture(Key.F9)));
            }
            catch
            {
            }
        }

        #endregion

        #region Properties

        private ScanFile ScanContents
        {
            get
            {
                return _scanContents;
            }
            set
            {
                _scanContents = value;
            }
        }

        private OcrFile OcrContents
        {
            get
            {
                if (_ocrContents == null)
                    _ocrContents = new OcrFile(Settings.Default.OcrFolderPath);

                return _ocrContents;
            }
            set
            {
                _ocrContents = value;
            }
        }

        private ScanImages ScannedImages
        {
            get
            {
                if (_scannedImages == null)
                    _scannedImages = new ScanImages(Settings.Default.ScanFolderPath);

                return _scannedImages;
            }
            set
            {
                _scannedImages = value;
            }
        }

        private ScanImage SelectedImage
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

        private BitmapSource MainImageSource
        {
            get
            {
                return (this.MainImage.Source as BitmapSource);
            }
            set
            {
                this.MainImage.Source = value;
            }
        }

        private string OcrText
        {
            get
            {
                TextRange textRange = new TextRange(OcrRichTextBox.Document.ContentStart, OcrRichTextBox.Document.ContentEnd);

                if (textRange != null)
                    return textRange.Text;
                else
                    return null;
            }
        }

        private double Zoom
        {
            get
            {
                return _zoom;
            }
            set
            {
                if (value < ZOOM_MIN)
                    _zoom = ZOOM_MIN;
                else if (value > ZOOM_MAX)
                    _zoom = ZOOM_MAX;
                else
                    _zoom = value;
            }
        }

        #endregion

        #region Window Events

        private void RibbonWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            try
            {
                //upgrade nastaveni pro novou verzi
                if (Settings.Default.UpgradeSettings)
                {
                    Settings.Default.Upgrade();
                    Settings.Default.UpgradeSettings = false;
                    Settings.Default.Save();
                }

                //kontrola nastaveni
                if (!String.IsNullOrEmpty(Settings.Default.ScanFolderPath))
                {
                    if (!Directory.Exists(Settings.Default.ScanFolderPath))
                        Directory.CreateDirectory(Settings.Default.ScanFolderPath);
                }
                else
                {
                    MessageBox.Show("Není nastavena složka pro ukládání naskenovaných souborů.", this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }

                if (!String.IsNullOrEmpty(Settings.Default.OcrFolderPath))
                {
                    if (!Directory.Exists(Settings.Default.OcrFolderPath))
                        Directory.CreateDirectory(Settings.Default.OcrFolderPath);
                }
                else
                {
                    MessageBox.Show("Není nastavena složka pro ukládání naskenovaných souborů.", this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }

                if (!String.IsNullOrEmpty(Settings.Default.FineReaderPath))
                {
                    if (!File.Exists(Settings.Default.FineReaderPath))
                        MessageBox.Show("Neexistuje soubor pro spuštění ABBYY FineReader aplikace.", this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                else
                {
                    MessageBox.Show("Není nastavenen soubor pro spuštění ABBYY FineReader aplikace.", this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }

                //naskenovane obrazky
                this.DataContext = ScannedImages;

                //zkontroluje rozdelane tituly (napr. po padu aplikace)
                ScanContents = DozpController.GetCheckOutContents();

                if (ScanContents != null)
                {
                    ScannedImages.LoadContents(ScanContents.FileName);
                    ScanImageListView.SelectedIndex = 0;

                    OcrContents.Load(ScanContents.OcrFileName);
                    if (File.Exists(OcrContents.TxtFilePath))
                    {
                        LoadOcrRichTextBox(OcrContents.TxtFilePath);
                    }
                }
                else
                {
                    ClearData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SetBookInfoPanel();
                SetMenuButtonsEnabled();
                this.Cursor = null;
            }
        }

        private void RibbonWindow_Closed(object sender, EventArgs e)
        {
            Settings.Default.MainWindowMax = (this.WindowState == WindowState.Maximized);
            Settings.Default.Save();
        }

        private void ScanImageListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            try
            {

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SetMenuButtonsEnabled();
                SetBookInfoPanel();
                SetImagesStatusBar();
                this.Cursor = null;
            }
        }

        private void MainImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {

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

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
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
            }
            finally
            {
                this.Cursor = null;
            }
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            try
            {
                MessageBox.Show(HelpButton.ToolTipDescription, HelpButton.ToolTipTitle);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                this.Cursor = null;
            }
        }

        private void ExitAppMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        #endregion

        #region Menu HOME

        #region Group OCR

        private void CheckOutContentsButton_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;
            ServerBusyIndicator.IsBusy = true;

            try
            {
                ClearData(false);

                ScanContents = DozpController.GetContentsToOCR();

                if (ScanContents != null)
                {
                    if (ScanContents.Status == StatusCode.Scanned)
                    {
                        OcrContents.Delete();
                        string tifPath = System.IO.Path.Combine(ScannedImages.ScanFolderPath, ScanContents.FileName);
                        DozpController.CheckOutContents(ScanContents.ScanFileID, tifPath);
                    }

                    ScannedImages.LoadContents(ScanContents.FileName);
                    ScanImageListView.SelectedIndex = 0;
                    OcrContents.Load(ScanContents.OcrFileName);
                }
                else
                {
                    ServerBusyIndicator.IsBusy = false;
                    MessageBox.Show("Na serveru již nejsou žádné dokumenty pro zpracování.", CheckOutContentsButton.Header.ToString(), MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                ServerBusyIndicator.IsBusy = false;
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SetBookInfoPanel();
                SetMenuButtonsEnabled();
                ServerBusyIndicator.IsBusy = false;
                this.Cursor = null;
            }
        }

        private void BrowseContentsButton_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            try
            {
                ClearData();

                BrowseContentsDialog dlg = new BrowseContentsDialog(this);
                if (dlg.ShowDialog() == true)
                {
                    ScanContents = dlg.SelectedContents;

                    ScannedImages.LoadContents(ScanContents.FileName);
                    ScanImageListView.SelectedIndex = 0;
                    OcrContents.Load(ScanContents.OcrFileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                e.Handled = true;
                SetMenuButtonsEnabled();
                SetBookInfoPanel();
                this.Cursor = null;
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern IntPtr SetParent(IntPtr hwc,IntPtr hwp);
        private void FineReaderButton_Click(object sender, RoutedEventArgs e)
        {
            if (ScanContents == null)
            {
                MessageBox.Show("Není žádný dokument ve zpracování.", FineReaderButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (ScannedImages == null)
            {
                MessageBox.Show("Není načten naskenovaný obraz.", FineReaderButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!File.Exists(Settings.Default.FineReaderPath))
            {
                MessageBox.Show("Neexistuje soubor pro spuštění ABBYY FineReader aplikace.", FineReaderButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            this.Cursor = Cursors.Wait;

            try
            {
                OcrContents.Delete();
                var process = System.Diagnostics.Process.Start(Settings.Default.FineReaderPath, ScannedImages.ContentsFullName);
                process.WaitForExit();

                if (!File.Exists(OcrContents.OcrFilePath))
                {
                    MessageBox.Show("Neexistuje PDF soubor vytvořený ABBYY FineReader aplikací.", FineReaderButton.Label, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }

                if (File.Exists(OcrContents.TxtFilePath))
                {
                    LoadOcrRichTextBox(OcrContents.TxtFilePath);
                }
                else
                {
                    MessageBox.Show("Neexistuje TXT soubor vytvořený ABBYY FineReader aplikací.", FineReaderButton.Label, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SetMenuButtonsEnabled();
                this.Cursor = null;
            }
        }

        private void DiscardContentsButton_Click(object sender, RoutedEventArgs e)
        {
            if (ScanContents == null)
            {
                MessageBox.Show("Není žádný dokument ve zpracování.", DiscardContentsButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            this.Cursor = Cursors.Wait;

            try
            {
                ActivityDialog dlg = new ActivityDialog(this, ScanContents.ScanFileID, OcrContents, OcrActivity.Discard);
                if (dlg.ShowDialog() == true)
                {
                    ClearData();
                    MessageBox.Show("Dokument byl ÚSPĚŠNĚ vyřazen ze zpracování.", DiscardContentsButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Dokument se NEPODAŘILO vyřadit ze zpracování.", DiscardContentsButton.Label, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SetMenuButtonsEnabled();
                SetBookInfoPanel();
                this.Cursor = null;
            }
        }

        private void UndoContentsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ScanContents == null)
                {
                    //zkontroluje rozdelane tituly (napr. po padu aplikace)
                    ScanContents = DozpController.GetCheckOutContents();
                }

                if (ScanContents != null)
                {
                    ActivityDialog dlg = new ActivityDialog(this, ScanContents.ScanFileID, OcrContents, OcrActivity.Undo);
                    if (dlg.ShowDialog() == true)
                    {
                        ClearData();
                        MessageBox.Show("Dokument byl ÚSPĚŠNĚ vrácen zpět na server.", UndoContentsButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Není žádný dokument pro vrácení zpět na server.", UndoContentsButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Dokument se NEPODAŘILO vrátit zpět na server.", UndoContentsButton.Label, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SetMenuButtonsEnabled();
                SetBookInfoPanel();
                this.Cursor = null;
            }
        }

        private void CheckInContentsButton_Click(object sender, RoutedEventArgs e)
        {
            if (ScanContents == null)
            {
                MessageBox.Show("Není žádný dokument ve zpracování.", CheckInContentsButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!File.Exists(OcrContents.OcrFilePath))
            {
                MessageBox.Show(String.Format("Neexistuje PDF soubor '{0}'.", OcrContents.OcrFilePath), CheckInContentsButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!File.Exists(OcrContents.TxtFilePath))
            {
                MessageBox.Show(String.Format("Neexistuje textový soubor '{0}'.", OcrContents.TxtFilePath), CheckInContentsButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            this.Cursor = Cursors.Wait;

            try
            {
                OcrContents.OcrText = this.OcrText;
                OcrContents.BackupText();

                ActivityDialog dlg = new ActivityDialog(this, ScanContents.ScanFileID, OcrContents, OcrActivity.CheckIn);
                if (dlg.ShowDialog() == true)
                {
                    ClearData();
                    MessageBox.Show("Dokument byl ÚSPĚŠNĚ odeslán na server.", CheckInContentsButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Dokument se NEPODAŘILO odeslat na server.", CheckInContentsButton.Label, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SetMenuButtonsEnabled();
                this.Cursor = null;
            }
        }

        #endregion

        #region Group SCAN

        private void ZoomInButton_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            try
            {
                this.Zoom += ZOOM_STEP;
                SetImageLayout();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SetMenuButtonsEnabled();
                this.Cursor = null;
            }
        }

        private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            try
            {
                this.Zoom -= ZOOM_STEP;
                SetImageLayout();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SetMenuButtonsEnabled();
                this.Cursor = null;
            }
        }

        private void BestFitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            try
            {
                this.Zoom = 1;
                this.ImageScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                this.ImageScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                this.MainImage.LayoutTransform = null;
                this.MainImage.Stretch = Stretch.Uniform;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SetMenuButtonsEnabled();
                this.Cursor = null;
            }
        }

        private void ActualSizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            try
            {
                this.Zoom = 1;
                SetImageLayout();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SetMenuButtonsEnabled();
                this.Cursor = null;
            }
        }

        private void PropertiesButton_Click(object sender, RoutedEventArgs e)
        {
            if (ScanContents == null)
            {
                MessageBox.Show("Není žádný dokument ve zpracování.", PropertiesButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            this.Cursor = Cursors.Wait;

            try
            {
                PropertiesDialog dlg = new PropertiesDialog(this, ScanContents, OcrContents);
                if (dlg.ShowDialog() == true)
                {

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SetMenuButtonsEnabled();
                this.Cursor = null;
            }
        }

        #endregion

        #region Group TEXT

        private void AdobeReaderButton_Click(object sender, RoutedEventArgs e)
        {
            if (ScanContents == null)
            {
                MessageBox.Show("Není žádný dokument ve zpracování.", AdobeReaderButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!File.Exists(OcrContents.OcrFilePath))
            {
                MessageBox.Show("Neexistuje PDF soubor vytvořený ABBYY FineReader aplikací.", AdobeReaderButton.Label, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

            this.Cursor = Cursors.Wait;

            try
            {
                var process = System.Diagnostics.Process.Start(OcrContents.OcrFilePath);
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SetMenuButtonsEnabled();
                this.Cursor = null;
            }
        }

        private void FormatTextButton_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            try
            {
                TextRange textRange = new TextRange(OcrRichTextBox.Document.ContentStart, OcrRichTextBox.Document.ContentEnd);
                textRange.Text = OcrContents.Format(textRange.Text);
                OcrContents.BackupText();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SetMenuButtonsEnabled();
                this.Cursor = null;
            }
        }

        private void UndoFormatButton_Click(object sender, RoutedEventArgs e)
        {
            if (ScanContents == null)
            {
                MessageBox.Show("Není žádný dokument ve zpracování.", UndoFormatButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!File.Exists(OcrContents.BakFilePath))
            {
                MessageBox.Show("Není uložen žádný záložný textový soubor.", UndoFormatButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            this.Cursor = Cursors.Wait;

            try
            {
                LoadOcrRichTextBox(OcrContents.BakFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SetMenuButtonsEnabled();
                this.Cursor = null;
            }
        }

        private void SaveTextButton_Click(object sender, RoutedEventArgs e)
        {
            if (ScanContents == null)
            {
                MessageBox.Show("Není žádný dokument ve zpracování.", SaveTextButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            this.Cursor = Cursors.Wait;

            try
            {
                OcrContents.BackupText();
                SaveOcrRichTextBox(OcrContents.TxtFilePath);
                MessageBox.Show("Textový soubor byl uložen na lokálny počítač.", SaveTextButton.Label, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Textový soubor se NEPODAŘILO uložit.", UndoContentsButton.Label, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SetMenuButtonsEnabled();
                this.Cursor = null;
            }
        }

        #region RichTextBox

        private void LoadOcrRichTextBox(string fileName)
        {
            if (String.IsNullOrEmpty(fileName)) throw new ArgumentNullException("fileName");
            if (!File.Exists(fileName)) throw new ArgumentException(String.Format("Soubor '{0}' s OCR textem neexistuje.", fileName));

            TextRange textRange = new TextRange(OcrRichTextBox.Document.ContentStart, OcrRichTextBox.Document.ContentEnd);
            using (FileStream fs = new FileStream(fileName, FileMode.Open, System.IO.FileAccess.Read))
            {
                textRange.Load(fs, DataFormats.Text);
                fs.Close();
            }

            OcrRichTextBox.IsEnabled = (textRange.Text.Length > 0);
        }

        private void SaveOcrRichTextBox(string fileName)
        {
            if (String.IsNullOrEmpty(fileName)) throw new ArgumentNullException("fileName");

            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                TextRange textRange = new TextRange(OcrRichTextBox.Document.ContentStart, OcrRichTextBox.Document.ContentEnd);
                textRange.Save(fs, DataFormats.Text);
                fs.Close();
            }
        }

        private void ClearOcrRichTextBox()
        {
            this.OcrRichTextBox.Document.Blocks.Clear();
            this.OcrRichTextBox.IsEnabled = false;
        }

        private void OcrRichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        #endregion

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

        //Načte nezpracovaný dokument (F2)
        private void CheckOutContentsExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.CheckOutContentsButton.IsEnabled)
            {
                CheckOutContentsButton_Click(this.CheckOutContentsButton, null);
            }
        }
        private void CheckOutContentsCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        //Zpracovat dokument OCR (F5)
        private void FineReaderCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.FineReaderButton.IsEnabled)
            {
                FineReaderButton_Click(this.FineReaderButton, null);
            }
        }
        private void FineReaderCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        //Vyřadit dokument ze zpracování (F6)
        private void DiscardContentsExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.DiscardContentsButton.IsEnabled)
            {
                DiscardContentsButton_Click(this.DiscardContentsButton, null);
            }
        }
        private void DiscardContentsCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        //Zrušit OCR zpracování (F7)
        private void UndoContentsExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.UndoContentsButton.IsEnabled)
            {
                UndoContentsButton_Click(this.UndoContentsButton, null);
            }
        }
        private void UndoContentsCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        //Odeslat zpracováný dokument (F9)
        private void CheckInContentsExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.CheckInContentsButton.IsEnabled)
            {
                CheckInContentsButton_Click(this.CheckInContentsButton, null);
            }
        }
        private void CheckInContentsCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        #endregion

        #region Private methods

        private void ClearData(bool deleteFiles = true)
        {
            SetBookInfoPanel();
            ClearOcrRichTextBox();

            if (deleteFiles)
            {
                ScannedImages.Delete();
                OcrContents.Delete();
            }

            ScanContents = null;
            ScannedImages = null;
            OcrContents = null;

            ScanImageListView.ItemsSource = ScannedImages;
        }

        private void SetMenuButtonsEnabled()
        {
            bool initialized = (ScanContents != null);
            bool scan = (MainImageSource != null);
            bool ocr = (OcrContents != null);

            //OCR
            this.CheckOutSplitButton.IsEnabled = !initialized;
            this.CheckOutContentsButton.IsEnabled = !initialized;
            this.BrowseContentsButton.IsEnabled = !initialized;

            this.FineReaderButton.IsEnabled = initialized;
            this.DiscardContentsButton.IsEnabled = initialized;
            this.UndoContentsButton.IsEnabled = initialized;
            this.CheckInContentsButton.IsEnabled = initialized && OcrContents.FileExists(FileFormat.Pdf);

            //SCAN
            this.ZoomInButton.IsEnabled = scan && (Zoom < ZOOM_MAX);
            this.ZoomOutButton.IsEnabled = scan && (Zoom > ZOOM_MIN);
            this.BestFitButton.IsEnabled = scan;
            this.ActualSizeButton.IsEnabled = scan;
            this.PropertiesButton.IsEnabled = scan;

            //OCR
            this.OcrRichTextBox.IsEnabled = ocr && OcrContents.FileExists(FileFormat.Txt);
            this.AdobeReaderButton.IsEnabled = this.CheckInContentsButton.IsEnabled;
            this.FormatTextButton.IsEnabled = this.OcrRichTextBox.IsEnabled;
            this.UndoFormatButton.IsEnabled = ocr && OcrContents.FileExists(FileFormat.Bak);
            this.SaveTextButton.IsEnabled = this.OcrRichTextBox.IsEnabled;

            //StatusBar
            if (ScannedImages != null) this.ScanContentsStatusBar.Text = ScannedImages.ContentsFullName;
            if (OcrContents != null) this.OcrContentStatusBar.Text = OcrContents.TxtFilePath;

            ServerBusyIndicator.IsBusy = false;
        }

        private void SetBookInfoPanel()
        {
            if (ScanContents != null && ScanContents.Book != null)
            {
                this.BookSysNoTextBlock.Text = String.Format("SYSNO: {0}", ScanContents.Book.SysNo);
                this.BookTitleTextBlock.Text = ScanContents.Book.Publication;
            }
            else
            {
                this.BookSysNoTextBlock.Text = "SysNo:";
                this.BookTitleTextBlock.Text = "";
            }
        }

        private void SetImageLayout()
        {
            TransformGroup t = new TransformGroup();
            t.Children.Add(new ScaleTransform(this.Zoom, this.Zoom));
            this.ImageScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            this.ImageScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            this.MainImage.Stretch = Stretch.None;
            this.MainImage.LayoutTransform = t;
        }

        private void SetImagesStatusBar()
        {
            string text = "";

            try
            {
                if (ScannedImages == null || ScannedImages.Count == 0 || SelectedImage == null)
                    text = "žádný";
                else
                    text = String.Format("Obsah: {0}/{1}", SelectedImage.Page, ScannedImages.ContetsPages);
            }
            catch
            {
            }

            this.ScannedImagesStatusBar.Text = text;
        }

        #endregion
    }
}
