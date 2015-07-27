using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using Newtonsoft.Json;

namespace Comdat.DOZP.Services
{
    /// <summary>
    /// Represents BibInfo class of request and response object, used for Serialization
    /// All attributes have same name as in query in JavaScript API of ObalkyKnih.cz
    /// </summary>
    [DataContract]
    public class ObalkyKnihBibInfo
    {
        [JsonIgnore]
        [DataMember]
        public string sysno { get; set; }

        [DataMember]
        public List<string> authors { get; set; }

        [DataMember]
        public string title { get; set; }

        [DataMember]
        public string year { get; set; }

        [DataMember]
        public string ean { get; set; }

        [DataMember]
        public string isbn { get; set; }

        [DataMember]
        public string issn { get; set; }

        [DataMember]
        public string nbn { get; set; }

        [DataMember]
        public string oclc { get; set; }
    }
}
