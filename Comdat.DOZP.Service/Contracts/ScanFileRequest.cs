using System;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Comdat.DOZP.Services
{
    [DataContract]
    public class ScanFileRequest
    {
        [DataMember(IsRequired=true)]
        public int ScanFileID { get; set; }

        [DataMember]
        public string Computer { get; set; }

        [DataMember]
        public string Comment { get; set; }
    }
}
