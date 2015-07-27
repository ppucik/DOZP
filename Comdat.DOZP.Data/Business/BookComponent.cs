using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Transactions;

using Comdat.DOZP.Core;
using Comdat.DOZP.Data.Repository;

namespace Comdat.DOZP.Data.Business
{
    public class BookComponent
    {
        private static readonly BookComponent _instance = new BookComponent();

        public static BookComponent Instance
        {
            get
            {
                return _instance;
            }
        }

        public Book GetByID(int bookID)
        {
            if (bookID == 0) throw new ArgumentNullException("bookID");

            BookRepository repository = new BookRepository();
            return repository.Select(bookID);
        }

        public List<Book> GetList(BookFilter filter)
        {
            if (filter == null) throw new ArgumentNullException("filter");
            if (filter.Modified == null) filter.Modified = new DateRange(DateRange.DateType.Modified);

            BookRepository repository = new BookRepository();
            return repository.Select(filter);
        }

        public List<Book> GetByFilter(int catalogueID, DateTime modifiedFrom, DateTime modifiedTo, short? partOfBook, int? status, string sortExpression)
        {
            BookFilter filter = new BookFilter();
            filter.CatalogueID = catalogueID;
            filter.Modified = new DateRange(modifiedFrom, modifiedTo);
            filter.PartOfBook = (PartOfBook?)partOfBook;
            filter.Status = (StatusCode?)status;

            return GetList(filter).OrderBy(sortExpression).ToList();
        }

        public Book Save(Book book)
        {
            if (book == null)
                throw new ArgumentNullException("book");

            if (book.CatalogueID == 0)
                throw new ArgumentException("Neplatný parametr CatalogueID");

            if (String.IsNullOrEmpty(book.SysNo))
                throw new ArgumentNullException("Neplatný parametr SysNo");

            if (book.SysNo.Length > 20)
                throw new ArgumentOutOfRangeException("Parametr SysNo je delší než 20 znaků");

            if (String.IsNullOrEmpty(book.Title))
                throw new ArgumentNullException("Neplatný parametr Title");

            BookRepository repository = new BookRepository();
            Book original = null;

            if (book.BookID == 0)
            {
                List<Book> books = repository.Select(new BookFilter(book.CatalogueID, book.SysNo));

                book.FileIndex = (short)(books != null && books.Count > 0 ? books.Max(i => i.FileIndex) + 1 : 0);
                book.ISBN = book.ISBN.Left(50); //normalize !!!
                book.ISSN = book.ISSN.Left(50);
                book.NBN = book.NBN.Left(50);
                book.OCLC = book.OCLC.Left(50);
                book.Author = book.Author.Left(200);
                book.Title = book.Title.Left(1000);
                book.Year = book.Year.Left(20);
                book.Volume = book.Volume.Left(100);
                book.Barcode = book.Barcode.Left(20);
                book.Created = DateTime.Now;
                book.Modified = book.Created;
                book.Comment = book.Comment.Left(1000);
                book = repository.Create(book);
            }
            else
            {
                original = repository.Select(book.BookID);

                if (original == null)
                    throw new ArgumentNullException(String.Format("Záznam publikace (ID={0}) neexistuje.", book.BookID));

                original.Volume = book.Volume.Left(100);
                original.Comment = book.Comment.Left(1000);
                original.Modified = DateTime.Now;
                book = repository.Update(original);
            }

            return book;
        }

        public bool Export(int bookID, string userName, string computer)
        {
            //kontrola vstupnich parametru
            if (bookID == 0)
                throw new ArgumentNullException("book");

            bool result = false;
            BookRepository repository = new BookRepository();

            //kontrola existence publikace
            Book book = repository.Select(bookID);
            if (book == null)
                throw new ArgumentNullException(String.Format("Záznam publikace (ID={0}) neexistuje.", bookID));

            if (book.FrontCover != null && book.FrontCover.Status == StatusCode.Scanned)
            {
                ScanFileComponent.Instance.ImportObalkyKnih(book.FrontCover.ScanFileID, userName, computer);
                book = repository.Select(bookID);
            }

            if (book.TableOfContents != null && book.TableOfContents.Status == StatusCode.Scanned && !book.TableOfContents.UseOCR)
            {
                ScanFileComponent.Instance.CompleteContents(book.TableOfContents.ScanFileID, userName, computer);
                book = repository.Select(bookID);
            }

            if (book.FrontCover != null && book.TableOfContents != null)
            {
                if (book.FrontCover.Status == StatusCode.Complete && book.TableOfContents.Status == StatusCode.Complete)
                {
                    ScanFileComponent.Instance.Export(book.FrontCover.ScanFileID, userName, computer);
                    ScanFileComponent.Instance.Export(book.TableOfContents.ScanFileID, userName, computer);
                    result = true;
                }
            }
            else if (book.FrontCover != null && book.TableOfContents == null)
            {
                if (book.FrontCover.Status == StatusCode.Complete)
                {
                    ScanFileComponent.Instance.Export(book.FrontCover.ScanFileID, userName, computer);
                    result = true;
                }
            }
            else if (book.FrontCover == null && book.TableOfContents != null)
            {
                if (book.TableOfContents.Status == StatusCode.Complete)
                {
                    ScanFileComponent.Instance.Export(book.TableOfContents.ScanFileID, userName, computer);
                    result = true;
                }
            }
            else
            {
                result = false;
            }

            if (result)
            {
                book.Modified = DateTime.Now;
                book = repository.Update(book);
            }

            return result;
        }

        //[Obsolete]
        //public bool Export2(int bookID, string ftpPath)
        //{
        //    //kontrola vstupnich parametru
        //    if (bookID == 0)
        //        throw new ArgumentNullException("book");

        //    bool result = false;
        //    BookRepository repository = new BookRepository();

        //    //kontrola existence publikace
        //    Book book = repository.Select(bookID);
        //    if (book == null)
        //        throw new ArgumentNullException(String.Format("Záznam publikace (ID={0}) neexistuje.", bookID));

        //    if (book.FrontCover != null && book.FrontCover.Status == StatusCode.Exported)
        //    {
        //        ScanFileComponent.Instance.Export2(book.FrontCover.ScanFileID, ftpPath);
        //        result = true;
        //    }

        //    if (book.TableOfContents != null && book.TableOfContents.Status == StatusCode.Exported)
        //    {
        //        ScanFileComponent.Instance.Export2(book.TableOfContents.ScanFileID, ftpPath);
        //        result = true;
        //    }

        //    return result;
        //}

        public bool Delete(int bookID, string userName)
        {
            if (bookID == 0)
                throw new ArgumentNullException("bookID");

            if (String.IsNullOrEmpty(userName))
                throw new ArgumentException("Neplatný parametr jméno uživatele.");

            BookRepository repository = new BookRepository();
            Book book = repository.Select(bookID);

            //kontrola existence publikace
            if (book == null)
                throw new ApplicationException(String.Format("Publikace (ID={0}) neexistuje.", bookID));

            if (book.IsExported())
                throw new ApplicationException(String.Format("Publikace (ID={0}) byla již exportována do ALEPHu, nelze vymazat.", bookID));

            //vymazani obalky
            if (book.HasPartOfBook(PartOfBook.FrontCover))
            {
                ScanFileComponent.Instance.Delete(book.FrontCover.ScanFileID, userName);
            }

            //vymazani obsahu
            if (book.HasPartOfBook(PartOfBook.TableOfContents))
            {
                ScanFileComponent.Instance.Delete(book.TableOfContents.ScanFileID, userName);
            }

            try
            {
                return repository.Delete(book);
            }
            catch (Exception ex)
            {
                throw new ApplicationException(String.Format("Nepodařilo se vymazat data publikace (ID={0}) z databáze.", bookID), ex);
            }
        }
    }
}
