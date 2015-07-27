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
using System.Windows.Navigation;
using System.Windows.Shapes;

using Comdat.DOZP.Core;
using Comdat.DOZP.Process;

namespace Comdat.DOZP.App
{
    using Properties;

    /// <summary>
    /// Interaction logic for BookInfoBox.xaml
    /// </summary>
    public partial class BookInfoBox : UserControl
    {
        private Book _book = null;

        public BookInfoBox()
        {
            InitializeComponent();

            this.BookInfoGrid.Visibility = Visibility.Collapsed;
        }

        ~BookInfoBox()
        {
            _book = null;
        }

        public Book Book
        {
            get
            {
                return _book;
            }
            private set
            {
                _book = value;
            }
        }

        public bool IsBookLoaded
        {
            get
            {
                return (this.Book != null);
            }
        }

        public void Load(int bookID)
        {
            if (bookID != 0)
            {
                Load(DozpController.GetBook(bookID));
            }
            else
            {
                Clear();
            }
        }

        public void Load(BookSettings settings)
        {
            if (settings != null)
            {
                if (settings.BookID != 0)
                {
                    Load(DozpController.GetBook(Settings.Default.LastScanBook.BookID));
                }
                else
                {
                    Load(new Book(Settings.Default.LastScanBook));
                }
            }
            else
            {
                Clear();
            }
        }

        public void Load(Book book)
        {
            this.Book = book;

            if (this.Book != null)
            {
                Settings.Default.LastScanBook = Book.GetSettings();
                Settings.Default.Save();

                this.BookInfoGrid.Visibility = Visibility.Visible;
                this.SysnoTextBox.Text = Book.SysNo;
                this.AuthorTextBox.Text = Book.Author;
                this.TitleTextBox.Text = Book.Title;
                this.YearTextBox.Text = Book.Year;
                this.VolumeTextBox.Text = Book.Volume;
                this.IsbnTextBox.Text = Book.ISBN;
                this.NbnTextBox.Text = Book.NBN;
                this.OclcTextBox.Text = Book.OCLC;
                this.BarcodeTextBox.Text = Book.Barcode;
            }
            else
            {
                Clear();
            }
        }

        public string GetFullName(PartOfBook? partOfBook)
        {
            string fullName = null;

            if (this.Book != null && partOfBook.HasValue)
            {
                string fileName = null;

                switch (partOfBook)
                {
                    case PartOfBook.FrontCover:
                        fileName = String.Format("{0}.jpg", this.Book.GetFileName());
                        break;
                    case PartOfBook.TableOfContents:
                        fileName = String.Format("{0}.tif", this.Book.GetFileName());
                        break;
                    default:
                        break;
                }

                if (!String.IsNullOrEmpty(fileName))
                {
                    fullName = System.IO.Path.Combine(Settings.Default.ScanFolderPath, fileName);
                }
                
            }

            return fullName;
        }

        public bool HasPartOfBook(PartOfBook? partOfBook)
        {
            return (this.Book != null && partOfBook.HasValue && this.Book.HasPartOfBook(partOfBook.Value));
        }

        public bool CanImageChanged(PartOfBook? partOfBook)
        {
            return (this.Book != null && partOfBook.HasValue && this.Book.CanImageChanged(partOfBook.Value));
        }

        public void SetImageChanged(PartOfBook? partOfBook)
        {
            if (!IsBookLoaded)
            {
                MessageBox.Show("Není načten záznam publikace.", "Informace o publikaci", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            if (this.Book != null && partOfBook.HasValue)
            {
                this.Book.SetImageChanged(partOfBook.Value);
            }
        }

        public bool IsImageExported(PartOfBook? partOfBook)
        {
            return (this.Book != null && partOfBook.HasValue && this.Book.IsExported(partOfBook.Value));
        }

        public void Clear()
        {
            this.Book = null;

            this.BookInfoGrid.Visibility = Visibility.Collapsed;
            this.SysnoTextBox.Text = null;
            this.AuthorTextBox.Text = null;
            this.TitleTextBox.Text = null;
            this.YearTextBox.Text = null;
            this.VolumeTextBox.Text = null;
            this.IsbnTextBox.Text = null;
            this.NbnTextBox.Text = null;
            this.OclcTextBox.Text = null;
            this.BarcodeTextBox.Text = null;

            Settings.Default.LastScanBook = null;
            Settings.Default.Save();
        }
    }
}
