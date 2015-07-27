using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

using Comdat.DOZP.Core;

namespace Comdat.DOZP.Scan
{
    public class ScanImageControl : Image
    {
        #region Fields
        public static readonly DependencyProperty ScanImageProperty;
        #endregion

        #region Constructor

        static ScanImageControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ScanImageControl), new FrameworkPropertyMetadata(typeof(ScanImageControl)));

            ScanImageProperty = DependencyProperty.Register("ScanImage", typeof(ScanImage), typeof(ScanImageControl), new PropertyMetadata(null));
        }

        #endregion

        #region Properties

        [Description("The displayed image"), Category("Common Properties")]
        public ScanImage ScanImage
        {
            get
            {
                return (ScanImage)GetValue(ScanImageProperty);
            }
            set
            {
                SetValue(ScanImageProperty, value);
            }
        }

        #endregion

        #region Methods

        public void Load(string fileName, bool isCover)
        {
            try
            {
                this.ScanImage = new ScanImage(fileName, isCover);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Refresh(BitmapSource source)
        {
            if (this.ScanImage == null)
                throw new ApplicationException("Property ScanImage is not initialized");

            try
            {
                this.ScanImage.Source = source;
                this.Source = this.ScanImage.Source;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Save(BitmapSource original)
        {
            if (this.ScanImage == null || original == null) return;

            try
            {
                if (this.ScanImage.SourceChanged)
                {
                    this.ScanImage.Save(original);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Clear()
        {
            try
            {
                this.ScanImage = null;
                //this.Source = ImageFunctions.Convert(Properties.Resources.BlankImage);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion
    }
}
