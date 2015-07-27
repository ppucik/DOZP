using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Comdat.DOZP.Core
{
    public class FileSumList : List<FileSumItem>
    {
        #region Constructors

        public FileSumList() { }

        public FileSumList(List<FileSumItem> list) 
        {
            this.AddRange(list);
        }

        #endregion

        #region Public properties

        public FileSumItem GetTotalSum()
        {
            FileSumItem _summary = new FileSumItem("Celkem", "summary");

            foreach (FileSumItem item in this)
            {
                _summary.FrontCoverScanned += item.FrontCoverScanned;
                _summary.FrontCoverComplete += item.FrontCoverComplete;
                _summary.FrontCoverExported += item.FrontCoverExported;

                _summary.TableOfContentsScanned += item.TableOfContentsScanned;
                _summary.TableOfContentsInProgress += item.TableOfContentsInProgress;
                _summary.TableOfContentsDiscarded += item.TableOfContentsDiscarded;
                _summary.TableOfContentsComplete += item.TableOfContentsComplete;
                _summary.TableOfContentsExported += item.TableOfContentsExported;

                _summary.AddPages(item.Pages);
                _summary.AddOcrTime((int)item.OcrTime.TotalSeconds);
            }

            return _summary;
        }

        #endregion

        #region Public methods

        public bool Contains(string key)
        {
            return (this.Count(s => s.Key == key) > 0);
        }

        public FileSumItem FindKey(string key)
        {
            return this.SingleOrDefault(s => s.Key == key);
        }

        public FileSumItem FindKey(int year)
        {
            return this.SingleOrDefault(s => s.Year == year && s.Month == 0 && s.Day == 0);
        }

        public FileSumItem FindKey(int year, int month)
        {
            return this.SingleOrDefault(s => s.Year == year && s.Month == month && s.Day == 0);
        }

        public FileSumItem FindKey(int year, int month, int day)
        {
            return this.SingleOrDefault(s => s.Year == year && s.Month == 0 && s.Day == day);
        }

        #endregion
    }
}
