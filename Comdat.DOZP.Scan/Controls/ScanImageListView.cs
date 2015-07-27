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
    public class ScanImageListView : ListView
    {
        #region Fields
        public static readonly DependencyProperty ScanImagesProperty;
        #endregion

        #region Constructor

        static ScanImageListView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ScanImageListView), new FrameworkPropertyMetadata(typeof(ScanImageListView)));

            ScanImagesProperty = DependencyProperty.Register("ScanImages", typeof(ScanImages), typeof(ScanImageControl), new PropertyMetadata(null));
        }

        #endregion

        #region Properties

        [Description("The displayed images"), Category("Common Properties")]
        public ScanImages ScanImages
        {
            get
            {
                return (ScanImages)GetValue(ScanImagesProperty);
            }
            set
            {
                SetValue(ScanImagesProperty, value);
            }
        }

        #endregion

        #region Methods

        public void Load(string scanFileName)
        {
            try
            {

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Save()
        {
            if (this.ScanImages == null) return;

            try
            {

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
                this.ScanImages = null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion
    }
}
