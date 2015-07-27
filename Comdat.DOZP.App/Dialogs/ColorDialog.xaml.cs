using System;
using System.Collections.Generic;
using System.IO;
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

namespace Comdat.DOZP.App
{
    using Properties;

    /// <summary>
    /// Interaction logic for ColorDialog.xaml
    /// </summary>
    public partial class ColorDialog : Window
    {
        #region Private members
        private BitmapSource _adjustImageSource = null;
        private string _imageFilePath = null;
        #endregion

        #region Constructors

        public ColorDialog(Window owner)
        {
            this.Owner = owner;
        }

        public ColorDialog(string imageFilePath)
        {
            InitializeComponent();

            this.ImageFilePath = imageFilePath;

            if (!Settings.Default.AdvancedColorCorection)
            {
                this.GammaGroupBox.Visibility = System.Windows.Visibility.Collapsed;
                this.SaturationGroupBox.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        public ColorDialog(Window owner, string imageFilePath)
            : this(imageFilePath)
        {
            this.Owner = owner;
        }

        ~ColorDialog()
        {
            _adjustImageSource = null;
        }

        #endregion

        #region Properties

        private string ImageFilePath
        {
            get
            {
                return _imageFilePath;
            }
            set
            {
                if (!String.IsNullOrEmpty(value))
                    _imageFilePath = value;
                else
                    throw new ArgumentNullException("ImageFilePath");
            }
        }

        private BitmapSource AdjustImageSource
        {
            get
            {
                if (_adjustImageSource == null && !String.IsNullOrEmpty(this.ImageFilePath))
                {
                    _adjustImageSource = ImageFunctions.Load(this.ImageFilePath, 300);
                }

                return _adjustImageSource;
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
                this.AdjustImage.Source = this.AdjustImageSource;
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

        private bool _working = false;
        private void BrightnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.AdjustImageSource == null) return;

            try
            {
                if (!_working)
                {
                    _working = true;
                    this.AdjustImage.Source = ImageFunctions.ColorCorrections(this.AdjustImageSource, this.Brightness, this.Contrast, this.Gamma, this.Hue, this.Saturation);
                    _working = false;
                }
            }
            catch (Exception ex)
            {
                _working = false;
                MessageBox.Show(ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ContrastSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.AdjustImageSource == null) return;

            try
            {
                this.AdjustImage.Source = ImageFunctions.ColorCorrections(this.AdjustImageSource, this.Brightness, this.Contrast, this.Gamma, this.Hue, this.Saturation);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GammaSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.AdjustImageSource == null) return;

            try
            {
                this.AdjustImage.Source = ImageFunctions.ColorCorrections(this.AdjustImageSource, this.Brightness, this.Contrast, this.Gamma, this.Hue, this.Saturation);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HueSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.AdjustImageSource == null) return;

            try
            {
                this.AdjustImage.Source = ImageFunctions.ColorCorrections(this.AdjustImageSource, this.Brightness, this.Contrast, this.Gamma, this.Hue, this.Saturation);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaturationSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.AdjustImageSource == null) return;

            try
            {
                this.AdjustImage.Source = ImageFunctions.ColorCorrections(this.AdjustImageSource, this.Brightness, this.Contrast, this.Gamma, this.Hue, this.Saturation);
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
                ImageFunctions.ColorCorrections(this.ImageFilePath, this.Brightness, this.Contrast, this.Gamma, this.Hue, this.Saturation);
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                this.DialogResult = false;
            }
            finally
            {
                this.Cursor = null;
            }
        }

        #endregion
    }
}
