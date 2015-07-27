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

namespace Comdat.DOZP.OCR
{
    using Properties;

    /// <summary>
    /// Interaction logic for BrowseContentsDialog.xaml
    /// </summary>
    public partial class BrowseContentsDialog : Window
    {
        #region Private members

        #endregion

        #region Constructors

        public BrowseContentsDialog()
        {
            InitializeComponent();

            this.OkButton.IsEnabled = false;
        }

        public BrowseContentsDialog(Window parent)
            : this()
        {
            this.Owner = parent;
        }

        #endregion

        #region Properties

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
                ContentsListView.ItemsSource = DozpController.GetUnprocessedContents(catalogueID);
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
            this.OkButton.IsEnabled = (SelectedContents != null);
        }

        private void ContentsListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.OkButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedContents == null)
            {
                MessageBox.Show("Není vybrán dokument pro OCR zpracování.", this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            this.Cursor = Cursors.Wait;

            try
            {
                string tifPath = System.IO.Path.Combine(Settings.Default.ScanFolderPath , SelectedContents.FileName);
                DozpController.CheckOutContents(SelectedContents.ScanFileID, tifPath);

                Settings.Default.Save();
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

        #region Private methods


        #endregion
    }
}
