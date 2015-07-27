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

namespace Comdat.DOZP.Scan
{
    using Properties;

    /// <summary>
    /// Interaction logic for BrowseBookDialog.xaml
    /// </summary>
    public partial class BrowseBookDialog : Window
    {
        #region Private members
        private string _userName = null;
        #endregion

        #region Constructors

        public BrowseBookDialog()
        {
            InitializeComponent();

            this.ScanButton.IsEnabled = false;
            this.DeleteButton.IsEnabled = false;
            this.NoOcrButton.IsEnabled = false;
        }

        public BrowseBookDialog(Window parent)
            : this()
        {
            this.Owner = parent;
        }

        #endregion

        #region Properties

        private string UserName
        {
            get
            {
                return _userName;
            }
            set
            {
                if (!String.IsNullOrEmpty(value))
                    _userName = value;
                else
                    throw new ArgumentNullException("UserName");
            }
        }

        public Book SelectedBook
        {
            get
            {
                return (SelectedContents != null ? SelectedContents.Book : null);
            }
        }

        public ScanFile SelectedContents
        {
            get
            {
                return (this.ContentsListView.SelectedValue as ScanFile);
            }
        }

        #endregion

        #region Window events

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            try
            {
                Institution institution = DozpController.GetInstitution();
                this.CatalogueComboBox.ItemsSource = (institution != null ? institution.Catalogues : null);
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

        private void CatalogueComboBox_SelectionChanged(object sender, SelectionChangedEventArgs c)
        {
            this.Cursor = Cursors.Wait;

            try
            {
                int catalogueID = (int)CatalogueComboBox.SelectedValue;
                ContentsListView.ItemsSource = DozpController.GetDiscardContents(catalogueID, this.UserName);
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

        private void ContentsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((SelectedContents != null))
            {
                this.CommentTextBox.Text = SelectedContents.Comment;
                this.ScanButton.IsEnabled = true;
                this.DeleteButton.IsEnabled = true;
                this.NoOcrButton.IsEnabled = true;
            }
            else
            {
                this.CommentTextBox.Text = "";
                this.ScanButton.IsEnabled = false;
                this.DeleteButton.IsEnabled = false;
                this.NoOcrButton.IsEnabled = false;
            }
        }

        private void ContentsListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.ScanButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }

        private void ScanButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedContents == null)
            {
                MessageBox.Show("Není vybrán obsah publikace.", this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            this.Cursor = Cursors.Wait;

            try
            {
                //DozpController.DeleteScanFile(SelectedContents.ScanFileID, "Oprava");
                SelectedContents.Book = DozpController.GetBook(SelectedContents.BookID);
                
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

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedContents == null)
            {
                MessageBox.Show("Není vybrán obsah publikace.", this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            this.Cursor = Cursors.Wait;

            try
            {
                if (DozpController.DeleteScanFile(SelectedContents.ScanFileID, "Odstranený záznam"))
                {
                    MessageBox.Show("Záznam publikace byl odstraněn.", this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                }

                this.DialogResult = false;
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

        private void NoOcrButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedContents == null)
            {
                MessageBox.Show("Není vybrán obsah publikace.", this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            this.Cursor = Cursors.Wait;

            try
            {
                if (DozpController.CancelOcrContents(SelectedContents.ScanFileID))
                {
                    MessageBox.Show("Záznamu publikace bylo zrušeno OCR zpracování.", this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                }

                this.DialogResult = false;
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
