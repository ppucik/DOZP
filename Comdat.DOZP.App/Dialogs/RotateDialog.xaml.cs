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

namespace Comdat.DOZP.App
{
    /// <summary>
    /// Interaction logic for RotateDialog.xaml
    /// </summary>
    public partial class RotateDialog : Window
    {
        #region Private members
        private BitmapSource _rotateImageSource = null;
        private string _imageFilePath = null;
        #endregion

        #region Constructors

        public RotateDialog(string imageFilePath)
        {
            InitializeComponent();

            this.ImageFilePath = imageFilePath;
        }

        public RotateDialog(Window owner, string imageFilePath)
            : this(imageFilePath)
        {
            this.Owner = owner;
        }

        ~RotateDialog()
        {
            _rotateImageSource = null;
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

        private BitmapSource RotateImageSource
        {
            get
            {
                if (_rotateImageSource == null)
                {
                    _rotateImageSource = ImageFunctions.Load(this.ImageFilePath, 300);
                }

                return _rotateImageSource;
            }
            //set
            //{
            //    if (value != null)
            //        _rotateImageSource = value;
            //    else
            //        throw new ArgumentNullException("ScanImageSource");
            //}
        }

        public float Angle
        {
            get
            {
                return (float)this.AngleSlider.Value;
            }
        }

        #endregion

        #region Window events

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Cursor = Cursors.Wait;
                this.RotateImage.Source = this.RotateImageSource;
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

        private void AngleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.RotateImageSource == null) return;

            try
            {
                this.RotateImage.Source = ImageFunctions.Deskew(this.RotateImageSource, this.Angle);
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
                ImageFunctions.Rotate(this.ImageFilePath, this.Angle);
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
