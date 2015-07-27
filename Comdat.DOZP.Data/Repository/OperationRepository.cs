using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

using Comdat.DOZP.Core;

namespace Comdat.DOZP.Data.Repository
{
    public class OperationRepository
    {
        public List<Operation> Select(int scanFileID)
        {
            List<Operation> list = null;

            using (var db = new DozpContext())
            {
                list = (from f in db.Operations
                        where f.ScanFileID == scanFileID
                        select f).ToList();
            }

            return list;
        }

        public Operation Create(Operation operation)
        {
            if (operation == null) return null;

            using (var db = new DozpContext())
            {
                db.Entry(operation).State = EntityState.Added;
                db.SaveChanges();
            }

            return operation;
        }

        internal Operation Update(Operation operation)
        {
            if (operation == null) return null;

            using (var db = new DozpContext())
            {
                db.Entry(operation).State = EntityState.Modified;
                db.SaveChanges();
            }

            return operation;
        }

        internal bool Delete(Operation operation)
        {
            if (operation == null) return false;

            using (var db = new DozpContext())
            {
                db.Entry(operation).State = EntityState.Deleted;
                db.SaveChanges();
            }

            return true;
        }

        public int GetCount(string userName)
        {
            int count = 0;

            if (!String.IsNullOrEmpty(userName))
            {
                using (var db = new DozpContext())
                {
                    count = db.Operations.Count(o => o.UserName == userName);
                }
            }

            return count;
        }

        public FileSumList GetTimeStatistics(StatisticsFilter filter)
        {
            if (filter == null) return null;

            FileSumList list = new FileSumList();

            using (var db = new DozpContext())
            {
                if (!filter.Year.HasValue && !filter.Month.HasValue && !filter.Day.HasValue)
                {
                    var stat = from o in db.Operations
                               where (0 == filter.CatalogueID || o.ScanFile.Book.CatalogueID == filter.CatalogueID) &&
                                     (!filter.UseOCR.HasValue || o.ScanFile.UseOCR == filter.UseOCR.Value) &&
                                     (String.IsNullOrEmpty(filter.UserName) || o.UserName == filter.UserName) &&
                                     (!filter.Status.HasValue || o.Status == filter.Status.Value)
                               group o by new { o.ScanFile.PartOfBook, o.Status, o.Executed.Year } into g
                               orderby g.Key.Year, g.Key.PartOfBook, g.Key.Status
                               select new
                               {
                                   partOfBook = g.Key.PartOfBook,
                                   status = g.Key.Status,
                                   year = g.Key.Year,
                                   count = g.Count()
                               };

                    foreach (var o in stat)
                    {
                        FileSumItem item = list.SingleOrDefault(s => s.Year == o.year);
                        if (item == null)
                        {
                            item = new FileSumItem(o.year);
                            list.Add(item);
                        }
                        item.SetVaule(o.partOfBook, o.status, o.count);
                    }
                }
                else if (filter.Year.HasValue && !filter.Month.HasValue && !filter.Day.HasValue)
                {
                    var stat = from o in db.Operations
                               where (0 == filter.CatalogueID || o.ScanFile.Book.CatalogueID == filter.CatalogueID) &&
                                     (!filter.UseOCR.HasValue || o.ScanFile.UseOCR == filter.UseOCR.Value) &&
                                     (String.IsNullOrEmpty(filter.UserName) || o.UserName == filter.UserName) &&
                                     (!filter.Status.HasValue || o.Status == filter.Status.Value) &&
                                     (filter.Year.Value == o.Executed.Year)
                               group o by new { o.ScanFile.PartOfBook, o.Status, o.Executed.Year, o.Executed.Month } into g
                               orderby g.Key.Year, g.Key.Month, g.Key.PartOfBook, g.Key.Status
                               select new
                               {
                                   partOfBook = g.Key.PartOfBook,
                                   status = g.Key.Status,
                                   year = g.Key.Year,
                                   month = g.Key.Month,
                                   count = g.Count()
                               };

                    for (int month = 1; month <= 12; month++)
                    {
                        if (filter.Year.Value < DateTime.Now.Year || month <= DateTime.Now.Month)
                        {
                            list.Add(new FileSumItem(filter.Year.Value, month));
                        }
                    }

                    foreach (var o in stat)
                    {
                        FileSumItem item = list.SingleOrDefault(s => s.Year == o.year && s.Month == o.month);
                        if (item == null)
                        {
                            item = new FileSumItem(o.year, o.month);
                            list.Add(item);
                        }
                        item.SetVaule(o.partOfBook, o.status, o.count);
                    }
                }
                else if (filter.Year.HasValue && filter.Month.HasValue && !filter.Day.HasValue)
                {
                    var stat = from o in db.Operations
                               where (0 == filter.CatalogueID || o.ScanFile.Book.CatalogueID == filter.CatalogueID) &&
                                     (!filter.UseOCR.HasValue || o.ScanFile.UseOCR == filter.UseOCR.Value) &&
                                     (String.IsNullOrEmpty(filter.UserName) || o.UserName == filter.UserName) &&
                                     (!filter.Status.HasValue || o.Status == filter.Status.Value) &&
                                     (filter.Year.Value == o.Executed.Year && filter.Month.Value == o.Executed.Month)
                               group o by new { o.ScanFile.PartOfBook, o.Status, o.Executed.Year, o.Executed.Month, o.Executed.Day } into g
                               orderby g.Key.Year, g.Key.Month, g.Key.Day, g.Key.PartOfBook, g.Key.Status
                               select new
                               {
                                   partOfBook = g.Key.PartOfBook,
                                   status = g.Key.Status,
                                   year = g.Key.Year,
                                   month = g.Key.Month,
                                   day = g.Key.Day,
                                   count = g.Count()
                               };

                    for (int day = 1; day <= DateTime.DaysInMonth(filter.Year.Value, filter.Month.Value); day++)
                    {
                        if (new DateTime(filter.Year.Value, filter.Month.Value, day) <= DateTime.Now)
                        {
                            list.Add(new FileSumItem(filter.Year.Value, filter.Month.Value, day));
                        }
                    }

                    foreach (var o in stat)
                    {
                        FileSumItem item = list.SingleOrDefault(s => s.Year == o.year && s.Month == o.month && s.Day == o.day);
                        if (item == null)
                        {
                            item = new FileSumItem(o.year, o.month, o.day);
                            list.Add(item);
                        }
                        item.SetVaule(o.partOfBook, o.status, o.count);
                    }
                }
                else if (filter.Year.HasValue && filter.Month.HasValue && filter.Day.HasValue)
                {
                    var stat = from o in db.Operations
                               where (0 == filter.CatalogueID || o.ScanFile.Book.CatalogueID == filter.CatalogueID) &&
                                     (!filter.UseOCR.HasValue || o.ScanFile.UseOCR == filter.UseOCR.Value) &&
                                     (String.IsNullOrEmpty(filter.UserName) || o.UserName == filter.UserName) &&
                                     (!filter.Status.HasValue || o.Status == filter.Status.Value) &&
                                     (filter.Year.Value == o.Executed.Year && filter.Month.Value == o.Executed.Month && filter.Day.Value == o.Executed.Day)
                               group o by new { o.ScanFile.PartOfBook, o.Status, o.Executed.Year, o.Executed.Month, o.Executed.Day } into g
                               orderby g.Key.Year, g.Key.Month, g.Key.Day, g.Key.PartOfBook, g.Key.Status
                               select new
                               {
                                   partOfBook = g.Key.PartOfBook,
                                   status = g.Key.Status,
                                   year = g.Key.Year,
                                   month = g.Key.Month,
                                   day = g.Key.Day,
                                   count = g.Count()
                               };

                    foreach (var o in stat)
                    {
                        FileSumItem item = list.SingleOrDefault(s => s.Year == o.year && s.Month == o.month && s.Day == o.day);
                        if (item == null)
                        {
                            item = new FileSumItem(o.year, o.month, o.day);
                            list.Add(item);
                        }
                        item.SetVaule(o.partOfBook, o.status, o.count);
                    }
                }
                else
                {
                    var stat = from o in db.Operations
                               where (0 == filter.CatalogueID || o.ScanFile.Book.CatalogueID == filter.CatalogueID) &&
                                     (!filter.PartOfBook.HasValue || o.ScanFile.PartOfBook == filter.PartOfBook.Value) &&
                                     (!filter.UseOCR.HasValue || o.ScanFile.UseOCR == filter.UseOCR.Value) &&
                                     (String.IsNullOrEmpty(filter.UserName) || o.UserName == filter.UserName) &&
                                     (!filter.Status.HasValue || o.Status == filter.Status.Value)
                               group o by new { o.ScanFile.PartOfBook, o.Status } into g
                               orderby g.Key.PartOfBook, g.Key.Status
                               select new
                               {
                                   partOfBook = g.Key.PartOfBook,
                                   status = g.Key.Status,
                                   count = g.Count()
                               };

                    FileSumItem item = new FileSumItem("summary", "Celkem");

                    foreach (var o in stat)
                    {
                        item.SetVaule(o.partOfBook, o.status, o.count);
                        list.Add(item);
                    }
                }
            }

            return list;
        }

        public FileSumList GetUserStatistics(StatisticsFilter filter)
        {
            if (filter == null) return null;

            FileSumList list = new FileSumList();

            using (var db = new DozpContext())
            {
                var stat = from o in db.Operations
                           where (0 == filter.CatalogueID || o.ScanFile.Book.CatalogueID == filter.CatalogueID) &&
                                 (!filter.PartOfBook.HasValue || o.ScanFile.PartOfBook == filter.PartOfBook.Value) &&
                                 (!filter.UseOCR.HasValue || o.ScanFile.UseOCR == filter.UseOCR.Value) &&
                                 (!filter.Year.HasValue || o.Executed.Year == filter.Year.Value) &&
                                 (!filter.Month.HasValue || o.Executed.Month == filter.Month.Value) &&
                                 (!filter.Day.HasValue || o.Executed.Day == filter.Day.Value) &&
                                 (String.IsNullOrEmpty(filter.UserName) || o.UserName == filter.UserName) &&
                                 (!filter.Status.HasValue || o.Status == filter.Status.Value)
                           group o by new { o.ScanFile.PartOfBook, o.Status, o.UserName, o.User.FullName, o.User.Comment } into g
                           orderby g.Key.FullName, g.Key.PartOfBook, g.Key.Status
                           select new
                           {
                               partOfBook = g.Key.PartOfBook,
                               status = g.Key.Status,
                               username = g.Key.UserName,
                               fullname = g.Key.FullName,
                               comment = g.Key.Comment,
                               pages = g.Sum(e => e.ScanFile.PageCount),
                               seconds = g.Sum(e => e.ScanFile.OcrTime),
                               count = g.Count()
                           };

                foreach (var o in stat)
                {
                    FileSumItem item = list.SingleOrDefault(s => s.Key == o.username);
                    if (item == null)
                    {
                        item = new FileSumItem(o.fullname, o.username, o.comment);
                        list.Add(item);
                    }
                    item.SetVaule(o.partOfBook, o.status, o.count, o.pages, o.seconds);
                }
            }

            return list;
        }

        //public FileSumItem GetSummaryStatistics(int? institutionID)
        //{
        //    FileSumItem summary = new FileSumItem("summary", "Celkem");

        //    using (var db = new DozpContext())
        //    {
        //        var stat = from f in db.ScanFiles
        //                   where (!institutionID.HasValue || f.Book.Catalogue.Institutions.Count(i => i.InstitutionID == institutionID.Value) > 0)
        //                   group f by new { f.Status } into g
        //                   orderby g.Key.Status
        //                   select new
        //                   {
        //                       status = g.Key.Status,
        //                       count = g.Count()
        //                   };

        //        foreach (var s in stat)
        //        {
        //            summary.SetVaule(s.status, s.count);
        //        }
        //    }

        //    return summary;
        //}
    }
}
