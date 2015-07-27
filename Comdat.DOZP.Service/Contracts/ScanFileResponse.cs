using System;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Comdat.DOZP.Services
{
    [DataContract]
    public class ScanFileResponse
    {
        public ScanFileResponse()
        {
        }

        public ScanFileResponse(int scanFileID, bool result)
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
