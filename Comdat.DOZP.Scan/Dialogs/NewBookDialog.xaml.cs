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
    /// Interaction logic for NewBookDialog.xaml
    /// </summary>
    public partial class NewBookDialog : Window
    {
        #region Private members
        private Book _newBook = null;
        private List<Book> _books = null;
        #endregion

        #region Constructors

        public NewBookDialog()
        {
            InitializeComponent();

            this.IsbnLabel.Visibility = Visibility.Collapsed;
            this.IsbnComboBox.Visibility = Visibility.Collapsed;
            this.VolumeLabel.Visibility = Visibility.Collapsed;
            this.ContentsLabel.Visibility = Visibility.Collapsed;
            this.VolumeTextBox.Visibility = Visibility.Collapsed;

#if DEBUG
            //this.SearchValueTextBox.Text = "255275437";
            //this.SearchValueTextBox.Text = "3218082646";

            //this.SearchValueTextBox.Text = "255342063";
            this.SearchValueTextBox.Text = "255116197"; //2551083377

            //this.SearchValueTextBox.Text = "3233257913"; Mapa Prahy
#endif
        }

        public NewBookDialog(Window parent)
            : this()
        {
            this.Owner = parent;
        }

        ~NewBookDialog()
        {
            _newBook = null;
            _books = null;
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
                _newBook = value;
            }
        }

        private List<Book> Books
        {
            get
            {
                return _books;
            }
            set
            {
                _books = value;
            }
        }

        private string SearchField
        {
            get
            {
                return (this.SearchFieldComboBox.SelectedValue != null ? this.SearchFieldComboBox.SelectedValue.ToString() : null);
            }
        }

        private string SearchValue
        {
            get
            {
                return this.SearchValueTextBox.Text;
            }
        }

        private string Volume
        {
            get
            {
                return this.VolumeTextBox.Text;
            }
        }

        private string SelectedISBN
        {
            get
            {
                ComboBoxItem item = (this.IsbnComboBox.SelectedValue as ComboBoxItem);
                return (item != null ? item.Content.ToString() : null);
            }
        }

        #endregion

        #region Window events

        private void IsbnComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ComboBoxItem item = (IsbnComboBox.SelectedItem as ComboBoxItem);
                this.VolumeTextBox.Text = (item != null ? item.Tag.ToString() : String.Empty);
                this.VolumeTextBox.Focus();
                this.VolumeTextBox.Select(this.VolumeTextBox.Text.Length, 0);
            }
            catch
            {
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(SearchField))
            {
                MessageBox.Show("Není vybrána položka pro vyhledání záznamu.", this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (String.IsNullOrWhiteSpace(SearchValue))
            {
                MessageBox.Show(String.Format("Není zadána hodnota pro {0}.", this.SearchFieldComboBox.Text), this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                this.Cursor = Cursors.Wait;
                this.SearchButton.IsEnabled = false;

                Report report = null;
                string barcode = null;

                switch (SearchField)
                {
                    case "barcode":
                        barcode = SearchValue;
                        report = DozpController.SearchBook(Settings.Default.SelectedCatalogueID, barcode: SearchValue);
                        break;
                    case "sysno":
                        report = DozpController.SearchBook(Settings.Default.SelectedCatalogueID, sysno: SearchValue);
                        break;
                    case "isbn":
                        report = DozpController.SearchBook(Settings.Default.SelectedCatalogueID, isbn: SearchValue);
                        break;
                    default:
                        break;
                }

                //nacteni MARC zaznamu z katalogu
                if (report != null)
                {
                    if (report.Success)
                    {
                        Marc21Records records = new Marc21Records(report.XmlData);

                        if (records == null || records.Count == 0)
                        {
                            MessageBox.Show("Publikace nebyla nalezena, zkuste vyhledat podle jiného parametru (SYSNO, ISBN).", this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }
                        else if (records.Count == 1)
                        { 
                            Marc21Record record = (records[0] as Marc21Record);
                            string[] isbns = record.FindAll(Marc21Tag.TAG_DF_InternationalStandardBookNumber, Marc21Tag.CODE_InternationalStandardBookNumber);

                            //kontrola existence publikace - UPRAVA DATABAZY - UniqueKey
                            Books = DozpController.GetBooks(Settings.Default.SelectedCatalogueID, record.RecordIdentifier);

                            if (isbns != null && isbns.Count() <= 1)
                            {
                                if (isbns.Count() == 0)
                                    NewBook = GetBookByISBN(null);
                                else if (isbns.Count() == 1)
                                    NewBook = GetBookByISBN(isbns[0]);
                            }

                            if (NewBook == null)
                            {
                                NewBook = new Book();
                                NewBook.CatalogueID = Settings.Default.SelectedCatalogueID;
                                NewBook.SysNo = record.RecordIdentifier;
                                NewBook.ISBN = GetNormalizedISN(record.Find(Marc21Tag.TAG_DF_InternationalStandardBookNumber, Marc21Tag.CODE_InternationalStandardBookNumber));
                                NewBook.ISSN = GetNormalizedISN(record.Find(Marc21Tag.TAG_DF_InternationalStandardSerialNumber, Marc21Tag.CODE_InternationalStandardSerialNumber));
                                NewBook.NBN = GetFirstOrDefaultCNB(record.FindAll(Marc21Tag.TAG_DF_NationalBibliographyNumber, Marc21Tag.CODE_NationalBibliographyNumber));
                                NewBook.OCLC = GetFirstOrDefaultOCLC(record.FindAll(Marc21Tag.TAG_DF_SystemControlNumber, Marc21Tag.CODE_SystemControlNumber));
                                NewBook.Author = record.Author.TrimEnd(' ', ',');
                                NewBook.Title = record.Title.TrimEnd(' ', ',', ':', '/', '=');
                                NewBook.Year = record.DateOfPublication.TrimStart('c').TrimEnd(' ', '-');
                                NewBook.Volume = record.NumberOfPart.TrimEnd(' ', ',');
                                NewBook.Barcode = barcode;
                            }
                            else
                            {
                                if (NewBook.IsExported())
                                {
                                    MessageBox.Show(String.Format("Publikace SYSNO: {0} [ISBN {1}] byla již exportována do ALEPHu, nelze skenovat!", NewBook.SysNo, NewBook.ISBN), this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                                    return;
                                }
                                else
                                {
                                    if (MessageBox.Show(String.Format("Publikace SYSNO: {0} [ISBN {1}] byla již skenována, chcete pokračovat?", NewBook.SysNo, NewBook.ISBN), this.Title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                                        return;
                                }
                            }

                            //vicesvazkove dila
                            if (isbns != null && isbns.Count() > 1)
                            {
                                this.SearchFieldComboBox.IsReadOnly = true;
                                this.SearchValueTextBox.IsReadOnly = true;

                                //ISBN
                                this.IsbnLabel.Visibility = Visibility.Visible;
                                this.IsbnComboBox.Visibility = Visibility.Visible;
                                foreach (var isbn in isbns)
                                {
                                    Book book = GetBookByISBN(isbn);
                                    string volume = Regex.Match(isbn, @"\(([^)]*)\)").Groups[1].Value;

                                    if (book != null)
                                        volume = book.Volume;
                                    else if (!String.IsNullOrEmpty(NewBook.Volume))
                                        volume = NewBook.Volume;

                                    ComboBoxItem item = new ComboBoxItem();
                                    item.Content = isbn;
                                    item.Foreground = (book != null ? Brushes.Red : Brushes.Black);
                                    item.Tag = volume;
                                    this.IsbnComboBox.Items.Add(item);
                                }

                                //Díl, svazek
                                this.VolumeLabel.Visibility = Visibility.Visible;
                                this.ContentsLabel.Visibility = Visibility.Visible;
                                this.VolumeTextBox.Visibility = Visibility.Visible;

                                this.IsbnComboBox.SelectedIndex = 0;
                                this.SearchButton.Visibility = Visibility.Collapsed;
                                this.OkButton.Visibility = Visibility.Visible;

                                return;
                            }

                            this.DialogResult = true;
                        }
                        else
                        {
                            MessageBox.Show(String.Format("Nalezeno bylo {0} publikací, čárový kód není jedinečný.", records.Count), this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
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
                this.SearchButton.IsEnabled = true;
                this.Cursor = null;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(this.SelectedISBN))
            {
                MessageBox.Show("Není vybráno ISBN publikace.", this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (String.IsNullOrEmpty(this.Volume))
            {
                if (MessageBox.Show("Není zadán popis dílu, svazku, chcete pokračovat?", this.Title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    return;
            }

            try
            {
                this.Cursor = Cursors.Wait;

                Book book = GetBookByISBN(this.SelectedISBN);

                if (book != null)
                {
                    NewBook = book;

                    if (NewBook.IsExported())
                    {
                        MessageBox.Show(String.Format("Publikace SYSNO: {0} [ISBN {1}] byla již exportována do ALEPHu, nelze skenovat!", NewBook.SysNo, NewBook.ISBN), this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    else
                    {
                        if (MessageBox.Show(String.Format("Publikace SYSNO: {0} [ISBN {1}] byla již skenována, chcete pokračovat?", NewBook.SysNo, NewBook.ISBN), this.Title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                            return;
                    }
                }

                NewBook.ISBN = GetNormalizedISN(this.SelectedISBN);
                NewBook.Volume = this.Volume.TrimStart("Obsah").Trim();

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

        private Book GetBookByISBN(string isbn)
        {
            isbn = GetNormalizedISN(isbn);

            if (Books != null && Books.Count > 0)
            {
                if (String.IsNullOrEmpty(isbn))
                    return Books.SingleOrDefault(b => String.IsNullOrEmpty(b.ISBN));
                else
                    return Books.SingleOrDefault(b => b.ISBN.Equals(isbn));
            }

            return null;
        }

        private string GetNormalizedISN(string list)
        {
            if (String.IsNullOrEmpty(list)) return null;

            StringBuilder sb = new StringBuilder();
            char[] array = list.Trim().ToCharArray();

            foreach (char c in array)
            {
                if (Char.IsDigit(c) || c.Equals('-') || Char.ToUpper(c).Equals('X'))
                    sb.Append(c);
                else
                    break;
            }

            return sb.ToString();
        }

        private string GetFirstOrDefaultCNB(string[] list)
        {
            if (list != null && list.Count() > 0)
                return list.FirstOrDefault(i => i.StartsWith("cnb"));
            else
                return null;
        }

        private string GetFirstOrDefaultOCLC(string[] list)
        {
            if (list != null && list.Count() > 0)
                return list.FirstOrDefault(i => i.StartsWith("(OCoLC)"));
            else
                return null;
        }

        #endregion
    }
}
