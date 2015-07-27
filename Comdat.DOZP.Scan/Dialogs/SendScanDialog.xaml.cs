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

namespace Comdat.DOZP.Scan
{
    using Properties;

    /// <summary>
    /// Interaction logic for SendScanDialog.xaml
    /// </summary>
    public partial class SendScanDialog : Window
    {
        #region Private members
        private ScanImages _scannedImages = null;
        private Book _sendBook = null;
        #endregion

        #region Constructors

        public SendScanDialog(Book sendBook, ScanImages scannedImages)
        {
            InitializeComponent();

            this.ScannedImages = scannedImages;
            this.SendBook = sendBook;
        }

        public SendScanDialog(Window parent, Book sendBook, ScanImages scannedImages)
            : this(sendBook, scannedImages)
        {
            this.Owner = parent;
        }

        ~SendScanDialog()
        {
            _scannedImages = null;
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

        public ScanImages ScannedImages
        {
            get
            {
                return _scannedImages;
            }
            private set
            {
                if (value != null)
                    _scannedImages = value;
                else
                    throw new ArgumentNullException("ScannedImages");
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
                this.CoverTextBlock.Text = ScannedImages.HasCover.ToDisplay(true);
                this.ContentsTextBlock.Text = String.Format("{0} stránek", ScannedImages.ContetsPages);
                this.UsrOCRCheckBox.IsChecked = ScannedImages.HasContents;
                this.UsrOCRCheckBox.IsEnabled = ScannedImages.HasContents;

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

                //ulozi vsechny soubory
                ScannedImages.Save();

                if (ScannedImages.HasCoverNoUrl && !File.Exists(ScannedImages.CoverFullName))
                {
                    messageBoxText = String.Format("Soubor obálky '{0}' neexistuje.", ScannedImages.CoverFullName);
                    MessageBox.Show(messageBoxText, this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                if (ScannedImages.HasContents && !File.Exists(ScannedImages.ContentsFullName))
                {
                    messageBoxText = String.Format("Soubor obsahu '{0}' neexistuje.", ScannedImages.CoverFullName);
                    MessageBox.Show(messageBoxText, this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                this.Cursor = Cursors.Wait;
                this.YesButton.IsEnabled = false;

                //ulozi zaznam publikace
                SendBook.Comment = CommentTexBox.Text;
                Book newBook = DozpController.SaveBook(SendBook);

                //odesle obalku na server
                if (ScannedImages.HasCoverNoUrl)
                {
                    bool obalkyKnihCZ = Settings.Default.ImportObalkyKnihCZ;

                    if (SendBook.FrontCover == null)
                    {
                        if (!DozpController.InsertScanImage(newBook.BookID, PartOfBook.FrontCover, false, ScannedImages.CoverFullName, null, obalkyKnihCZ))
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

                            if (!DozpController.UpdateScanImage(SendBook.FrontCover, ScannedImages.CoverFullName, obalkyKnihCZ))
                            {
                                MessageBox.Show("Nepodařilo se uložit opravenou obálku na server.", this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                                return;
                            }
                        }
                    }
                }
                else
                {
                    //if (NewBook.FrontCover != null && NewBook.FrontCover.Status != StatusCode.Exported)
                    //{
                    //    if (DozpController.DeleteScanFile(NewBook.FrontCover.ScanFileID))
                    //    {
                    //        MessageBox.Show("Nepodařilo odstranit obálku ze serveru.", this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    //        return;
                    //    }
                    //}
                }

                //odesle obsah na server
                if (ScannedImages.HasContents)
                {
                    if (SendBook.TableOfContents == null)
                    {
                        if (!DozpController.InsertScanImage(newBook.BookID, PartOfBook.TableOfContents, UseOCR, ScannedImages.ContentsFullName))
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

                            if (!DozpController.UpdateScanImage(SendBook.TableOfContents, ScannedImages.ContentsFullName))
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
                        if (DozpController.DeleteScanFile(SendBook.TableOfContents.ScanFileID))
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
