using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Comdat.DOZP.Core;
using Comdat.DOZP.Process;

namespace Comdat.DOZP.App
{
    using Properties;

    /// <summary>
    /// Interaction logic for SendScanDialog.xaml
    /// </summary>
    public partial class SendScanDialog : Window
    {
        #region Private members
        private Book _sendBook = null;
        private string _frontCoverFilePath = null;
        private string _tableOfContentsFilePath = null;
        #endregion

        #region Constructors

        public SendScanDialog(Book sendBook, string frontCoverFilePath, string tableOfContentsFilePath)
        {
            InitializeComponent();

            this.SendBook = sendBook;
            this.FrontCoverFilePath = frontCoverFilePath;
            this.TableOfContentsFilePath = tableOfContentsFilePath;
        }

        public SendScanDialog(Window parent, Book sendBook, string frontCoverFilePath, string tableOfContentsFilePath)
            : this(sendBook, frontCoverFilePath, tableOfContentsFilePath)
        {
            this.Owner = parent;
        }

        ~SendScanDialog()
        {
            _sendBook = null;
        }

        #endregion

        #region Properties

        public Book SendBook
        {
            get
            {
                return _sendBook;
            }
            private set
            {
                if (value != null)
                    _sendBook = value;
                else
                    throw new ArgumentNullException("NewBook");
            }
        }

        private string FrontCoverFilePath
        {
            get
            {
                return _frontCoverFilePath;
            }
            set
            {
                _frontCoverFilePath = value;
            }
        }

        private string TableOfContentsFilePath
        {
            get
            {
                return _tableOfContentsFilePath;
            }
            set
            {
                _tableOfContentsFilePath = value;
            }
        }

        private bool UseOCR
        {
            get
            {
                if (this.UsrOCRCheckBox.IsChecked.HasValue)
                    return this.UsrOCRCheckBox.IsChecked.Value;
                else
                    return false;
            }
        }

        #endregion

        #region Window events

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.AuthorTextBlock.Text = SendBook.Author;
                this.TitleTextBlock.Text = SendBook.Title;
                this.YearTextBlock.Text = SendBook.Year;
                this.VolumeTextBlock.Text = SendBook.Volume;
                this.IsbnTextBlock.Text = SendBook.ISBN;
                this.CommentTexBox.Text = SendBook.Comment;
                this.CoverTextBlock.Text = FrontCoverFilePath; //ScannedImages.HasCover.ToDisplay(true);
                this.ContentsTextBlock.Text = TableOfContentsFilePath; //String.Format("{0} stránek", ScannedImages.ContetsPages);
                this.UsrOCRCheckBox.IsChecked = !String.IsNullOrEmpty(TableOfContentsFilePath); //ScannedImages.HasContents;
                this.UsrOCRCheckBox.IsEnabled = true; //ScannedImages.HasContents;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string messageBoxText = null;

                if (!String.IsNullOrEmpty(FrontCoverFilePath) && !File.Exists(FrontCoverFilePath))
                {
                    messageBoxText = String.Format("Soubor obálky '{0}' neexistuje.", FrontCoverFilePath);
                    MessageBox.Show(messageBoxText, this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                if (!String.IsNullOrEmpty(TableOfContentsFilePath) && !File.Exists(TableOfContentsFilePath))
                {
                    messageBoxText = String.Format("Soubor obsahu '{0}' neexistuje.", TableOfContentsFilePath);
                    MessageBox.Show(messageBoxText, this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                this.Cursor = Cursors.Wait;
                this.YesButton.IsEnabled = false;

                //ulozi zaznam publikace
                SendBook.Comment = CommentTexBox.Text;
                Book newBook = DozpController.SaveBook(SendBook);

                //odesle obalku na server
                if (!FrontCoverFilePath.Contains("ObalkyKnihCZ.jpg"))
                {
                    if (!String.IsNullOrEmpty(FrontCoverFilePath))
                    {
                        bool obalkyKnihCZ = Settings.Default.ImportObalkyKnihCZ;

                        if (SendBook.FrontCover == null)
                        {
                            if (!DozpController.InsertScanImage(newBook.BookID, PartOfBook.FrontCover, false, FrontCoverFilePath, null, obalkyKnihCZ))
                            {
                                MessageBox.Show("Nepodařilo se uložit novou obálku na server.", this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                                return;
                            }
                        }
                        else
                        {
                            if (SendBook.FrontCover.ImageChanged && SendBook.FrontCover.Status != StatusCode.Exported)
                            {
                                SendBook.FrontCover.UseOCR = false;

                                if (!DozpController.UpdateScanImage(SendBook.FrontCover, FrontCoverFilePath, obalkyKnihCZ))
                                {
                                    MessageBox.Show("Nepodařilo se uložit opravenou obálku na server.", this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                                    return;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (SendBook.FrontCover != null && SendBook.FrontCover.Status != StatusCode.Exported)
                        {
                            if (!DozpController.DeleteScanFile(SendBook.FrontCover.ScanFileID))
                            {
                                MessageBox.Show("Nepodařilo odstranit obálku ze serveru.", this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                                return;
                            }
                        }
                    }
                }

                //odesle obsah na server
                if (!String.IsNullOrEmpty(TableOfContentsFilePath))
                {
                    if (SendBook.TableOfContents == null)
                    {
                        if (!DozpController.InsertScanImage(newBook.BookID, PartOfBook.TableOfContents, UseOCR, TableOfContentsFilePath))
                        {
                            MessageBox.Show("Nepodařilo se uložit naskenovaný obsah na server.", this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            return;
                        }
                    }
                    else
                    {
                        if (SendBook.TableOfContents.ImageChanged && SendBook.TableOfContents.Status != StatusCode.Exported)
                        {
                            SendBook.TableOfContents.UseOCR = this.UseOCR;

                            if (!DozpController.UpdateScanImage(SendBook.TableOfContents, TableOfContentsFilePath))
                            {
                                MessageBox.Show("Nepodařilo se uložit opravený obsah na server.", this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                                return;
                            }
                        }
                    }
                }
                else
                {
                    if (SendBook.TableOfContents != null && SendBook.TableOfContents.Status != StatusCode.Exported)
                    {
                        if (!DozpController.DeleteScanFile(SendBook.TableOfContents.ScanFileID))
                        {
                            MessageBox.Show("Nepodařilo odstranit obsah ze serveru.", this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            return;
                        }
                    }
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
                this.YesButton.IsEnabled = true;
                this.Cursor = null;
            }
        }

        #endregion
    }
}
