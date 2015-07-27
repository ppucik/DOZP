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
    /// <summary>
    /// Interaction logic for RotateDialog.xaml
    /// </summary>
    public partial class RotateDialog : Window
    {
        #region Private members
        private BitmapSource _scanImageSource = null;
        #endregion

        #region Constructors

        public RotateDialog(ScanImage image)
        {
            InitializeComponent();

            this.ScanImageSource = image.GetThumbnail((int)this.Height);
            //this.AutoRotateButton.Visibility = Visibility.Hidden;
        }

        public RotateDialog(Window parent, ScanImage image)
            : this(image)
        {
            this.Owner = parent;
        }

        ~RotateDialog()
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
                this.RotateImage.Source = this.ScanImageSource;
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
            if (this.ScanImageSource == null) return;

            try
            {
                this.RotateImage.Source = ImageFunctions.Deskew(this.ScanImageSource, this.Angle);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //private void AutoRotateButton_Click(object sender, RoutedEventArgs e)
        //{
        //    if (this.ScanImageSource == null) return;

        //    try
        //    {
        //        this.Cursor = Cursors.Wait;
        //        this.AngleSlider.Value = ImageFunctions.GetDeskewAngle(this.ScanImageSource);
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //    finally
        //    {
        //        this.Cursor = null;
        //    }
        //}

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Cursor = Cursors.Wait;
                this.RotateImage.Source = ImageFunctions.Deskew(this.ScanImageSource, this.Angle);
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
