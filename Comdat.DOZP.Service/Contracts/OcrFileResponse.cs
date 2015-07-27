using System;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Comdat.DOZP.Services
{
    [DataContract]
    public class OcrFileResponse
    {
        public OcrFileResponse()
        {
        }

        public OcrFileResponse(int scanFileID, bool result)
        {
            this.ScanFileID = scanFileID;
            this.Result = result;
        }

        [DataMember(IsRequired = true)]
        public int ScanFileID { get; set; }

        [DataMember]
        public bool Result { get; set; }
    }
}
