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
using Comdat.ZRIS.Zoom;
using Comdat.ZRIS.Zoom.Marc21;

namespace Comdat.DOZP.Scan
{
    using Properties;

    /// <summary>
    /// Interaction logic for RecordDialog.xaml
    /// </summary>
    public partial class MarcRecordDialog : Window
    {
        #region Private members
        private Book _newBook = null;
        #endregion

        #region Constructors

        public MarcRecordDialog(Book newBook)
        {
            InitializeComponent();

            this.NewBook = newBook;
        }

        public MarcRecordDialog(Window parent, Book newBook)
            : this(newBook)
        {
            this.Owner = parent;
        }

        ~MarcRecordDialog()
        {
            _newBook = null;
        }

        #endregion

        #region Properties

        public Book NewBook
        {
            get
            {
                return _newBook;
            }
            private set
            {
                if (value != null)
                    _newBook = value;
                else
                    throw new ArgumentNullException("NewBook");
            }
        }

        #endregion

        #region Window events

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            try
            {
                Report report = DozpController.SearchBook(Settings.Default.SelectedCatalogueID, sysno: this.NewBook.SysNo);

                if (report != null)
                {
                    if (report.Success)
                    {
                        Marc21Records records = new Marc21Records(report.XmlData);

                        if (records == null || records.Count == 0)
                        {
                            this.MarcRecordTextBox.Text = "Nebyla nalezená žádná publikace.";
                        }
                        else if (records.Count == 1)
                        {
                            Marc21Record record = (records[0] as Marc21Record);
                            this.MarcRecordTextBox.Text = record.ToString();
                        }
                        else
                        {
                            this.MarcRecordTextBox.Text = String.Format("Nalezeno bylo {0} publikací, SysNo není jedinečné.", records.Count);
                        }
                    }
                    else
                    {
                        MessageBox.Show(String.Format("Chyba při vyhledání publikace: {0}", report.Errors), this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        this.DialogResult = false;
                    }
                }
                else
                {
                    MessageBox.Show("Žádná odpověd při vyhledání publikace.", this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    this.DialogResult = false;
                }
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
