using System;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Comdat.DOZP.Services
{
    [DataContract]
    public class SearchRequest
    {
        [DataMember]
        public int CatalogueID { get; set; }

        [DataMember]
        public string SysNo { get; set; }

        [DataMember]
        public string ISBN { get; set; }

        [DataMember]
        public string Author { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Year { get; set; }

        [DataMember]
        public string Barcode { get; set; }
    }
}
