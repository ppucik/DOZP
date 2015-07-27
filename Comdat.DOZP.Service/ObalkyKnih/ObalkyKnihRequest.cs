using System;
using System.Runtime.Serialization;
using System.ServiceModel;
using Newtonsoft.Json;

namespace Comdat.DOZP.Services
{
    /// <summary>
    /// Represent json object of request for original toc and cover, used for Serialization
    /// All attributes have same name as in query in JavaScript API of ObalkyKnih.cz
    /// </summary>
    [DataContract]
    public class ObalkyKnihRequest
    {
        [JsonIgnore]
        [DataMember]
        public string zserverUrl { get; set; }

        [DataMember]
        public string permalink { get; set; }

        [DataMember]
        public ObalkyKnihBibInfo bibinfo { get; set; }

        //[JsonIgnore]
        //[DataMember]
        //public string sysno { get; set; }

        //[DataMember]
        //public string isbn { get; set; }

        //[DataMember]
        //public string nbn { get; set; }

        //[DataMember]
        //public string oclc { get; set; }

        //[DataMember]
        //public string part_year { get; set; }

        //[DataMember]
        //public string part_volume { get; set; }

        //[DataMember]
        //public string part_no { get; set; }

        //[DataMember]
        //public string part_name { get; set; }

        //[JsonIgnore]
        //[DataMember]
        //public bool cache { get; set; }
    }
}
