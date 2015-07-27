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

namespace Comdat.DOZP.OCR
{
    /// <summary>
    /// Interaction logic for PropertiesDialog.xaml
    /// </summary>
    public partial class PropertiesDialog : Window
    {
        #region Private members
        private ScanFile _scanContents = null;
        private OcrFile _ocrContents = null;
        #endregion

        #region Constructors

        public PropertiesDialog(ScanFile scanContents, OcrFile ocrContents)
        {
            InitializeComponent();

            this.ScanContents = scanContents;
            this.OcrContents = ocrContents;
        }

        public PropertiesDialog(Window parent, ScanFile scanContents, OcrFile ocrContents)
            : this(scanContents, ocrContents)
        {
            this.Owner = parent;
        }

        ~PropertiesDialog()
        {
            _scanContents = null;
            _ocrContents = null;
        }

        #endregion

        #region Properties

        public ScanFile ScanContents
        {
            get
            {
                return _scanContents;
            }
            private set
            {
                if (value == null)
                    throw new ArgumentNullException("ScanContents");

                _scanContents = value;
            }
        }

        public OcrFile OcrContents
        {
            get
            {
                return _ocrContents;
            }
            private set
            {
                _ocrContents = value;
            }
        }

        #endregion

        #region Window events

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.SysNoTextBlock.Text = ScanContents.Book.SysNo;
                this.AuthorTextBlock.Text = ScanContents.Book.Author;
                this.TitleTextBlock.Text = ScanContents.Book.Title;
                this.YearTextBlock.Text = ScanContents.Book.Year;
                this.IsbnTextBlock.Text = ScanContents.Book.ISBN;
                this.BarCodeTextBlock.Text = ScanContents.Book.Barcode;

                this.CreatedTextBlock.Text = ScanContents.Created.ToString("F");
                this.ModifiedTextBlock.Text = ScanContents.Modified.ToString("F");
                this.CommentTextBlock.Text = ScanContents.Comment;
                this.StatusTextBlock.Text = ScanContents.Status.ToDisplay();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Private methods


        #endregion
    }
}
