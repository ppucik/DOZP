using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
//using System.Linq.Expressions;
using System.Text;

using Comdat.DOZP.Core;

namespace Comdat.DOZP.Data.Repository
{
    public class BookRepository
    {
        public Book Select(int bookID)
        {
            if (bookID == 0) return null;

            Book item = null;

            using (var db = new DozpContext())
            {
                item = db.Books
                     .Include(e => e.Catalogue)
                     .Include(e => e.ScanFiles)
                     .Where(pk => pk.BookID == bookID)
                     .SingleOrDefault();
            }

            return item;
        }

        public List<Book> Select(BookFilter filter)
        {
            if (filter == null) return null;

            List<Book> list = null;

            using (var db = new DozpContext())
            {
                list = (from b in db.Books.Include(e => e.Catalogue).Include(e => e.ScanFiles)
                        where (0 == filter.BookID || b.BookID == filter.BookID) &&
                              (0 == filter.CatalogueID || b.CatalogueID == filter.CatalogueID) &&
                              (String.IsNullOrEmpty(filter.SysNo) || b.SysNo == filter.SysNo) &&
                              (String.IsNullOrEmpty(filter.ISBN) || b.Barcode == filter.ISBN) &&
                              (String.IsNullOrEmpty(filter.Barcode) || b.Barcode == filter.Barcode) &&
                              (!filter.Modified.From.HasValue || b.Modified >= filter.Modified.From.Value) &&
                              (!filter.Modified.To.HasValue || b.Modified <= filter.Modified.To.Value) &&
                              //(!filter.Modified.From.HasValue || b.ScanFiles.Count(f => f.Modified >= filter.Modified.From.Value) > 0) &&
                              //(!filter.Modified.To.HasValue || b.ScanFiles.Count(f => f.Modified <= filter.Modified.To.Value) > 0) &&
                              (!filter.PartOfBook.HasValue || b.ScanFiles.Count(f => f.PartOfBook == filter.PartOfBook.Value) > 0) &&
                              (!filter.UseOCR.HasValue || b.ScanFiles.Count(f => f.UseOCR == filter.UseOCR.Value) > 0) &&
                              (!filter.Status.HasValue || b.ScanFiles.Count(f => f.Status == filter.Status.Value) > 0)
                        select b).ToList();
            }

            return list;
        }

        public Book Create(Book book)
        {
            if (book == null) return null;

            using (var db = new DozpContext())
            {
                db.Entry(book).State = EntityState.Added;
                db.SaveChanges();
            }

            return book;
        }

        public Book Update(Book book)
        {
            if (book == null) return null;

            using (var db = new DozpContext())
            {
                db.Entry(book).State = EntityState.Modified;
                db.SaveChanges();
            }

            return book;
        }

        public bool Delete(Book book)
        {
            if (book == null) return false;

            using (var db = new DozpContext())
            {
                db.Entry(book).State = EntityState.Deleted;
                db.SaveChanges();
            }

            return true;
        }
    }
}
