using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ImageComponents.WPF.Imaging;
using ImageComponents.WPF.Utilities;

using Comdat.DOZP.Core;
using Comdat.DOZP.Process;

namespace Comdat.DOZP.App
{
    using Properties;

    /// <summary>
    /// Interaction logic for OptionsDialog.xaml
    /// </summary>
    public partial class OptionsDialog : Window
    {
        #region Private members
        private ImgScan _imgScanComponent = null;
        #endregion

        #region Constructors

        public OptionsDialog()
        {
            InitializeComponent();
        }

        public OptionsDialog(Window owner)
            : this()
        {
            this.Owner = owner;
        }

        ~OptionsDialog()
        {
            _imgScanComponent = null;
        }

        #endregion

        #region Properties

        public ImgScan ImgScanComponent
        {
            get
            {
                if (_imgScanComponent == null)
                {
                    _imgScanComponent = new ImgScan();
                    _imgScanComponent.Registration = Properties.Resources.ImageComponentRegistration;
                }

                return _imgScanComponent;
            }
            set
            {
                _imgScanComponent = value;
            }
        }

        private string ScanFolderPath
        {
            get
            {
                return this.ScanFolderPathTextBox.Text;
            }
            set
            {
                this.ScanFolderPathTextBox.Text = value;
            }
        }

        #endregion

        #region Window events

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Cursor = Cursors.Wait;
                Institution institution = DozpController.GetInstitution();
                CatalogueComboBox.ItemsSource = (institution != null ? institution.Catalogues : null);
                //ScannerSourceNameComboBox.ItemsSource = (ImgScanComponent != null ? ImgScanComponent.GetScannerSources() : null);
                AppLoggingButton.IsEnabled = File.Exists(Logger.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
            finally
            {
                this.Cursor = null;
                this.Owner.Activate();
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            try
            {
                ScannerSourceNameComboBox.ItemsSource = (ImgScanComponent != null ? ImgScanComponent.GetScannerSources() : null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
            }
        }

        private void AppLoggingButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (File.Exists(Logger.FileName))
                {
                    System.Diagnostics.Process.Start(Logger.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ScanFolderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BrowserFolderDialog dlg = new BrowserFolderDialog();
                dlg.Title = this.ScanFolderPathLabel.Content.ToString();
                dlg.InitialFolder = this.ScanFolderPath;

                if (Convert.ToBoolean(dlg.ShowDialog(this)))
                {
                    this.ScanFolderPath = dlg.SelectedFolder;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ScannerSourceNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                this.Cursor = Cursors.Wait;
                LoadScannerAvailableDeviceCaps(ScannerSourceNameComboBox.SelectedValue.ToString());
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

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            string message = String.Empty;

            if (String.IsNullOrEmpty(ScanFolderPath))
            {
                message = "Není zadaná cesta pro ukládání naskenovaných souborů.";
                MessageBox.Show(message, this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                this.Cursor = Cursors.Wait;

                if (!Directory.Exists(ScanFolderPath))
                    Directory.CreateDirectory(ScanFolderPath);

                //Obecné
                Settings.Default.SelectedCatalogueID = Convert.ToInt32(CatalogueComboBox.SelectedValue);
                Settings.Default.ScanFolderPath = this.ScanFolderPath.TrimEnd(@"\") + @"\";
                Settings.Default.CheckObalkyKnihCZ = Convert.ToBoolean(CheckObalkyKnihCZCheckBox.IsChecked);
                Settings.Default.ImportObalkyKnihCZ = Convert.ToBoolean(ImportObalkyKnihCZCheckBox.IsChecked);
                Settings.Default.MainWindowTopmost = Convert.ToBoolean(MainWindowTopmostCheckBox.IsChecked);
                Settings.Default.AdvancedColorCorection = Convert.ToBoolean(AdvancedColorCorectionCheckBox.IsChecked);
                Settings.Default.ScanRotateEvenPage = Convert.ToBoolean(ScanRotateEvenPageCheckBox.IsChecked);
                Settings.Default.ScanRemoveBlackBorders = Convert.ToBoolean(ScanRemoveBlackBordersCheckBox.IsChecked);
                Settings.Default.AppLogging = Convert.ToBoolean(AppLoggingCheckBox.IsChecked);

                //Skenování
                Settings.Default.ScannerSourceName = ScannerSourceNameComboBox.SelectedValue.ToString();
                Settings.Default.ScanCapTransferMode = ScanCapTransferModeComboBox.SelectedName;
                Settings.Default.ScanCapPixelType = ScanCapPixelTypeComboBox.SelectedName;
                Settings.Default.ScanCapResolutions = Convert.ToInt32(ScanCapResolutionsComboBox.SelectedValue);
                Settings.Default.ScanCapImageFileFormat = ScanCapImageFileFormatComboBox.SelectedName;
                Settings.Default.ScanCapPaperSize = ScanCapPaperSizeComboBox.SelectedName;
                Settings.Default.ScanCapPaperOrientation = ScanCapPaperOrientationComboBox.SelectedName;
                Settings.Default.ScanShowScannerUI = Convert.ToBoolean(ScanShowScannerUICheckBox.IsChecked);
                Settings.Default.ScanEnablePreview = Convert.ToBoolean(ScanEnablePreviewCheckBox.IsChecked);
                Settings.Default.ScanShowIndicators = Convert.ToBoolean(ScanShowIndicatorsCheckBox.IsChecked);
                Settings.Default.ScanCapAutoDeskew = Convert.ToBoolean(ScanCapAutoDeskewCheckBox.IsChecked);
                Settings.Default.ScanCapAutoBorderDetection = Convert.ToBoolean(ScanCapAutoBorderDetectionCheckBox.IsChecked);
                Settings.Default.ScanCapAutoRotation = Convert.ToBoolean(ScanCapAutoRotationCheckBox.IsChecked);
                Settings.Default.ScanCapAutoBrightness = Convert.ToBoolean(ScanCapAutoBrightnessCheckBox.IsChecked);
                Settings.Default.ScannerBackColorBlack = Convert.ToBoolean(ScannerBackColorBlackCheckBox.IsChecked);
                Settings.Default.ScannerBackColorTolerance = Convert.ToDouble(ScannerBackColorToleranceUpDown.Value);

                Settings.Default.Save();
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log(String.Format("ERROR: {0}", ex.Message));
                this.DialogResult = false;
            }
            finally
            {
                this.Cursor = null;
            }
        }

        #endregion

        #region Private Methods

        private void LoadScannerAvailableDeviceCaps(string sourceName)
        {
            if (String.IsNullOrEmpty(sourceName)) return;

            //Capabilities.IsEnabled = ImgScanComponent.AvailableDeviceCapabilities.TwainCapabilities.CustomDSData;

            ImgScanComponent.ActiveSourceName = sourceName;

            ScanCapTransferModeComboBox.Load(ImgScanComponent.AvailableDeviceCapabilities.TwainICapabilities.AvailableImageXferMechs, Settings.Default.ScanCapTransferMode);
            ScanCapImageFileFormatComboBox.Load(ImgScanComponent.AvailableDeviceCapabilities.TwainICapabilities.AvailableImageFileFormats, Settings.Default.ScanCapImageFileFormat);
            ScanCapPixelTypeComboBox.Load(ImgScanComponent.AvailableDeviceCapabilities.TwainICapabilities.AvailablePixelTypes, Settings.Default.ScanCapPixelType);
            ScanCapResolutionsComboBox.Load(ImgScanComponent.AvailableDeviceCapabilities.TwainICapabilities.AvailableXResolutions, Settings.Default.ScanCapResolutions);
            ScanCapPaperSizeComboBox.Load(ImgScanComponent.AvailableDeviceCapabilities.TwainICapabilities.AvailablePaperSizes, Settings.Default.ScanCapPaperSize);
            ScanCapPaperOrientationComboBox.Load(ImgScanComponent.AvailableDeviceCapabilities.TwainICapabilities.AvailablePaperOrientations, Settings.Default.ScanCapPaperOrientation);

            SetCheckBoxValue(ScanCapAutoDeskewCheckBox, ImgScanComponent.AvailableDeviceCapabilities.TwainCapabilities.IsAutoDeskewEnabled);
            SetCheckBoxValue(ScanCapAutoBorderDetectionCheckBox, ImgScanComponent.AvailableDeviceCapabilities.TwainCapabilities.IsAutoBorderDetectionEnabled);
            SetCheckBoxValue(ScanCapAutoRotationCheckBox, ImgScanComponent.AvailableDeviceCapabilities.TwainCapabilities.IsAutoRotationEnabled);
            SetCheckBoxValue(ScanCapAutoBrightnessCheckBox, ImgScanComponent.AvailableDeviceCapabilities.TwainICapabilities.IsAutoBrightEnabled);
        }

        private void SetCheckBoxValue(CheckBox cb, bool? enabled)
        {
            if (cb != null)
            {
                cb.IsEnabled = enabled.HasValue;
                cb.IsChecked = enabled.HasValue && enabled.Value;
            }
        }

        #endregion
    }
}
