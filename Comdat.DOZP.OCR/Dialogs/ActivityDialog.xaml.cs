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
using Comdat.DOZP.Process;


//http://stackoverflow.com/questions/21712787/is-it-possible-to-change-the-busy-animation-in-the-busyindicator-of-extended-wpf

namespace Comdat.DOZP.OCR
{
    using Properties;

    /// <summary>
    /// Interaction logic for DiscardBookDialog.xaml
    /// </summary>
    public partial class ActivityDialog : Window
    {
        #region Private members
        private int _scanFileID = 0;
        private OcrFile _pdfFile = null;
        private OcrActivity _ocrActivity;
        #endregion

        #region Constructors

        public ActivityDialog(int scanFileID, OcrFile pdfFile, OcrActivity activity)
        {
            InitializeComponent();

            this.ScanFileID = scanFileID;
            this.PdfFile = pdfFile;
            this.Activity = activity;
        }

        public ActivityDialog(Window parent, int scanFileID, OcrFile pdfFile, OcrActivity activity)
            : this(scanFileID, pdfFile, activity)
        {
            this.Owner = parent;
        }

        ~ActivityDialog()
        {
            _pdfFile = null;
        }

        #endregion

        #region Properties

        public int ScanFileID
        {
            get
            {
                return _scanFileID;
            }
            private set
            {
                _scanFileID = value;
            }
        }

        public OcrFile PdfFile
        {
            get
            {
                return _pdfFile;
            }
            private set
            {
                if (value == null)
                    throw new ArgumentNullException("PdfFile");

                _pdfFile = value;
            }
        }

        public OcrActivity Activity
        {
            get
            {
                return _ocrActivity;
            }
            private set
            {
                _ocrActivity = value;
            }
        }

        public string Comment
        {
            get
            {
                return this.CommentTextBox.Text;
            }
        }

        #endregion

        #region Window events

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            try
            {
                switch (Activity)
                {
                    case OcrActivity.CheckOut:
                        this.Title = "Načíst dokument";
                        this.ActivityImage.Tag = "/Images/Home/NewBook32.png";
                        this.QuestionLabel.Content = "Chcete NAČÍST nový dokument pro OCR zpracování";
                        break;

                    case OcrActivity.Discard:
                        this.Title = "Vyřadit dokument";
                        this.ActivityImage.Tag = "/Images/Home/DiscardBook32.png";
                        this.QuestionLabel.Content = "Chcete VYŘADIT dokument z OCR zpracování ?";
                        this.CommentLabel.Content = "Napiště důvod vyřazení dokumentu ze zpracování:";
                        this.YesButton.IsEnabled = false;
                        break;

                    case OcrActivity.Undo:
                        this.Title = "Zrušit zpracování";
                        this.ActivityImage.Tag = "/Images/Home/UndoBook32.png";
                        this.QuestionLabel.Content = "Chcete VRÁTIT dokument zpět na server bez zpracování ?";
                        break;

                    case OcrActivity.CheckIn:
                        this.Title = "Odeslat dokument";
                        this.ActivityImage.Tag = "/Images/Home/SendOcr32.png";
                        this.QuestionLabel.Content = "Chcete ODESLAT zpracovaný OCR dokument na server ?";
                        break;

                    default:
                        break;
                }

                this.Title = String.Format("{0} [{1}]", this.Title, this.ScanFileID);
                this.ActivityImage.Source = new BitmapImage(new Uri(this.ActivityImage.Tag.ToString(), UriKind.Relative));
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

        private void CommentTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.YesButton.IsEnabled = (Activity != OcrActivity.Discard || CommentTextBox.Text.Length > 0);
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            if (Activity == OcrActivity.Discard && String.IsNullOrEmpty(Comment))
            {
                MessageBox.Show("Není zadán důvod vyřazení dokumentu ze zpracování.", this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            this.Cursor = Cursors.Wait;
            //ServerBusyIndicator.IsBusy = true;

            try
            {
                switch (Activity)
                {
                    case OcrActivity.CheckOut:
                        //string tifPath = System.IO.Path.Combine(Settings.Default.ScanFolderPath, ScanFileName);
                        //DozpController.CheckOutContents(ScanFileID, tifPath);
                        break;

                    case OcrActivity.Discard:
                        DozpController.DiscardContents(ScanFileID, Comment);
                        break;

                    case OcrActivity.Undo:
                        DozpController.UndoContents(ScanFileID, Comment);
                        break;

                    case OcrActivity.CheckIn:
                        DozpController.CheckInContents(ScanFileID, PdfFile.OcrText, PdfFile.OcrFilePath, Comment);
                        break;

                    default:
                        break;
                }

                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                this.DialogResult = false;
            }
            finally
            {
                //ServerBusyIndicator.IsBusy = false;
                this.Cursor = null;
            }
        }

        #endregion
    }
}
