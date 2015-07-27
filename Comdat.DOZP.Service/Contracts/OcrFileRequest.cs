using System;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Comdat.DOZP.Services
{
    [DataContract]
    public class OcrFileRequest
    {
        [DataMember(IsRequired = true)]
        public int ScanFileID { get; set; }

        [DataMember]
        public string Computer { get; set; }

        [DataMember]
        public string Comment { get; set; }

        [DataMember]
        public string OcrText { get; set; }

        [DataMember]
        public byte[] PdfFile { get; set; }
    }
}
