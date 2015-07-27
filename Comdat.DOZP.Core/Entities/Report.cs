using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Comdat.DOZP.Core
{
    [DataContract]
    public class Report
    {
        public Report()
        {
            this.Size = 0;
            this.XmlData = null;
            this.Success = false;
            this.Errors = null;
        }

        public Report(int catalogueID, string sysno, string isbn, string barcode)
            : this()
        {
            this.CatalogueID = catalogueID;
            this.SysNo = sysno;
            this.ISBN = isbn;
            this.Barcode = barcode;
        }

        [DataMember]
        public int CatalogueID { get; set; }

        [DataMember]
        public string SysNo { get; set; }

        [DataMember]
        public string ISBN { get; set; }

        [DataMember]
        public string Barcode { get; set; }

        [DataMember]
        public int Size { get; set; }

        [DataMember]
        public string XmlData { get; set; }

        [DataMember]
        public bool Success { get; set; }

        [DataMember]
        public string Errors { get; set; }
    }
}
