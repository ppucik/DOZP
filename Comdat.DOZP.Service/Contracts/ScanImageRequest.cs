using System;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;

using Comdat.DOZP.Core;

namespace Comdat.DOZP.Services
{
    [DataContract]
    public class ScanImageRequest
    {
        [DataMember(IsRequired = true)]
        public int ScanFileID { get; set; }

        [DataMember(IsRequired = true)]
        public int BookID { get; set; }

        [DataMember]
        public PartOfBook PartOfBook { get; set; }

        [DataMember]
        public bool UseOCR { get; set; }

        [DataMember]
        public string Computer { get; set; }

        [DataMember]
        public string Comment { get; set; }

        [DataMember]
        public byte[] Image { get; set; }

        [DataMember]
        public bool ObalkyKnihCZ { get; set; }
    }
}
