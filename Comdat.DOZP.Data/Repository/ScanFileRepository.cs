using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Interception;
using System.Linq;
using System.Text;

using Comdat.DOZP.Core;

namespace Comdat.DOZP.Data.Repository
{
    public class ScanFileRepository
    {
        public ScanFile Select(int scanFileID)
        {
            if (scanFileID == 0) return null;

            ScanFile item = null;

            using (var db = new DozpContext())
            {
                item = db.ScanFiles
                     .Include(e => e.Book.Catalogue)
                     .Include(e => e.Operations)
                     .Where(pk => pk.ScanFileID == scanFileID)
                     .SingleOrDefault();
            }

            return item;
        }

        public List<ScanFile> Select(ScanFileFilter filter)
        {
            if (filter == null) return null;

            List<ScanFile> list = null;

            using (var db = new DozpContext())
            {
                list = (from f in db.ScanFiles.Include(e => e.Book.Catalogue)
                        where (0 == filter.ScanFileID || f.ScanFileID == filter.ScanFileID) &&
                              (0 == filter.BookID || f.BookID == filter.BookID) &&
                              (0 == filter.CatalogueID || f.Book.CatalogueID == filter.CatalogueID) &&
                              (String.IsNullOrEmpty(filter.SysNo) || f.Book.SysNo == filter.SysNo) &&
                              (!filter.PartOfBook.HasValue || f.PartOfBook == filter.PartOfBook.Value) &&
                              (!filter.UseOCR.HasValue || f.UseOCR == filter.UseOCR.Value) &&
                              (!filter.Modified.From.HasValue || f.Modified >= filter.Modified.From.Value) &&
                              (!filter.Modified.From.HasValue || f.Modified <= filter.Modified.To.Value) &&
                              (!filter.Status.HasValue || f.Status == filter.Status.Value) &&
                              (String.IsNullOrEmpty(filter.UserName) || f.Operations.Count(o => o.UserName == filter.UserName) > 0)
                        select f).ToList();
            }

            return list;
        }

        /*
        SELECT * FROM ScanFile WHERE contains(OcrText, 'Masarykova AND Evropa')
        SELECT * FROM ScanFile WHERE freetext(OcrText, 'Stát národ')
        SELECT * FROM freetexttable(ScanFile, OcrText, 'Masarykova Evropa') AS t JOIN ScanFile a ON t.[KEY] = a.ScanFileID ORDER BY t.[RANK] Desc
         */
        public List<ScanFile> Select(int institutionID, string text)
        {
            if (String.IsNullOrEmpty(text)) return null;

            List<ScanFile> list = null;

            //DbInterception.Add(new FtsInterceptor());
            //string s = FtsInterceptor.Fts(text);

            using (var db = new DozpContext())
            {
                list = (from f in db.ScanFiles.Include(e => e.Book.Catalogue)
                        where (f.Book.SysNo == text) ||
                              (f.Book.ISBN == text) ||
                              (f.Book.Barcode == text) ||
                              (f.Book.Author.Contains(text)) ||
                              (f.Book.Title.Contains(text)) ||
                              (f.OcrText.Contains(text))
                        select f).ToList();
            }

            return list;
        }

        public ScanFile Create(ScanFile scanFile)
        {
            if (scanFile == null) return null;

            using (var db = new DozpContext())
            {
                db.Entry(scanFile).State = EntityState.Added;
                db.SaveChanges();
            }

            return scanFile;
        }

        public ScanFile Update(ScanFile scanFile)
        {
            if (scanFile == null) return null;

            using (var db = new DozpContext())
            {
                db.Entry(scanFile).State = EntityState.Modified;
                db.SaveChanges();
            }

            return scanFile;
        }

        public bool Delete(ScanFile scanFile)
        {
            if (scanFile == null) return false;

            using (var db = new DozpContext())
            {
                db.ScanFiles.Attach(scanFile);
                db.ScanFiles.Remove(scanFile);
                db.SaveChanges();
            }

            return true;
        }
    }
}
