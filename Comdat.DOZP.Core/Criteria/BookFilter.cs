using System;
using System.Runtime.Serialization;

namespace Comdat.DOZP.Core
{
    [DataContract]
    public class BookFilter
    {
        public BookFilter()
        {
            this.Modified = new DateRange(DateRange.DateType.Modified);
        }

        public BookFilter(int id)
            : this()
        {
            this.BookID = id;
        }

        public BookFilter(int CatalogueID, string sysno)
            : this()
        {
            this.SysNo = sysno;
        }

        [DataMember]
        public int CatalogueID { get; set; }

        [DataMember]
        public int BookID { get; set; }

        [DataMember]
        public string SysNo { get; set; }

        [DataMember]
        public string ISBN { get; set; }

        [DataMember]
        public string Barcode { get; set; }

        [DataMember]
        public DateRange Modified { get; set; }

        [DataMember]
        public PartOfBook? PartOfBook { get; set; }

        [DataMember]
        public bool? UseOCR { get; set; }

        [DataMember]
        public StatusCode? Status { get; set; }
    }
}
