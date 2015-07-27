using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Comdat.DOZP.Core
{
    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class BookSettings
    {
        public int BookID { get; set; }
        public int CatalogueID { get; set; }
        public string SysNo { get; set; }
        public string ISBN { get; set; }
        public string ISSN { get; set; }
        public string NBN { get; set; }
        public string OCLC { get; set; }
        public string Author { get; set; }
        public string Title { get; set; }
        public string Year { get; set; }
        public string Volume { get; set; }
        public string Barcode { get; set; }
    }
}
