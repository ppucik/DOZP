using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Comdat.DOZP.Core;

namespace Comdat.DOZP.Scan
{
    using Properties;

    /// <summary>
    /// Interaction logic for ColorDialog.xaml
    /// </summary>
    public partial class ColorDialog : Window
    {
        #region Private members
        private BitmapSource _scanImageSource = null;
        #endregion

        #region Constructors

        public ColorDialog(ScanImage image)
        {
            InitializeComponent();

            this.ScanImageSource = image.GetThumbnail((int)this.Height);

            if (!Settings.Default.AdvancedColorCorection)
            {
                this.GammaGroupBox.Visibility = System.Windows.Visibility.Collapsed;
                this.SaturationGroupBox.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        public ColorDialog(Window parent, ScanImage image)
            : this(image)
        {
            this.Owner = parent;
        }

        ~ColorDialog()
        {
            _scanImageSource = null;
        }

        #endregion

        #region Properties

        private BitmapSource ScanImageSource
        {
            get
            {
                return _scanImageSource;
            }
            set
            {
                if (value != null)
                    _scanImageSource = value;
                else
                    throw new ArgumentNullException("ScanImageSource");
            }
        }

        public int Brightness
        {
            get
            {
                return (int)this.BrightnessSlider.Value;
            }
        }

        public int Contrast
        {
            get
            {
                return (int)this.ContrastSlider.Value;
            }
        }

        public double Gamma
        {
            get
            {
                return this.GammaSlider.Value;
            }
        }

        public int Hue
        {
            get
            {
                return (int)this.HueSlider.Value;
            }
        }

        public float Saturation
        {
            get
            {
                return (float)this.SaturationSlider.Value;
            }
        }

        #endregion

        #region Window events

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Cursor = Cursors.Wait;
                this.AdjustImage.Source = this.ScanImageSource;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                this.Cursor = null;
            }
        }

        private void BrightnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.ScanImageSource == null) return;

            try
            {
                this.AdjustImage.Source = ImageFunctions.ColorCorrections(this.ScanImageSource, this.Brightness, this.Contrast, this.Gamma, this.Hue, this.Saturation);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ContrastSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.ScanImageSource == null) return;

            try
            {
                this.AdjustImage.Source = ImageFunctions.ColorCorrections(this.ScanImageSource, this.Brightness, this.Contrast, this.Gamma, this.Hue, this.Saturation);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GammaSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.ScanImageSource == null) return;

            try
            {
                this.AdjustImage.Source = ImageFunctions.ColorCorrections(this.ScanImageSource, this.Brightness, this.Contrast, this.Gamma, this.Hue, this.Saturation);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HueSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.ScanImageSource == null) return;

            try
            {
                this.AdjustImage.Source = ImageFunctions.ColorCorrections(this.ScanImageSource, this.Brightness, this.Contrast, this.Gamma, this.Hue, this.Saturation);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaturationSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.ScanImageSource == null) return;

            try
            {
                this.AdjustImage.Source = ImageFunctions.ColorCorrections(this.ScanImageSource, this.Brightness, this.Contrast, this.Gamma, this.Hue, this.Saturation);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Cursor = Cursors.Wait;
                this.AdjustImage.Source = ImageFunctions.ColorCorrections(this.ScanImageSource, this.Brightness, this.Contrast, this.Gamma, this.Hue, this.Saturation);
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                this.DialogResult = false;
            }
            //finally
            //{
            //    this.Cursor = null;
            //}
        }

        #endregion
    }
}
