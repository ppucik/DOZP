using System;
using System.Runtime.Serialization;

namespace Comdat.DOZP.Core
{
    [DataContract]
    public class ScanFileFilter
    {
        public ScanFileFilter()
        {
            this.Modified = new DateRange(DateRange.DateType.Modified);
        }

        public ScanFileFilter(int id)
            : this()
        {
            this.ScanFileID = id;
        }

        [DataMember]
        public int CatalogueID { get; set; }

        [DataMember]
        public int BookID { get; set; }

        [DataMember]
        public string SysNo { get; set; }

        [DataMember]
        public int ScanFileID { get; set; }

        [DataMember]
        public PartOfBook? PartOfBook { get; set; }

        [DataMember]
        public bool? UseOCR { get; set; }

        [DataMember]
        public DateRange Modified { get; set; }

        [DataMember]
        public StatusCode? Status { get; set; }

        [DataMember]
        public string UserName { get; set; }
    }
}
