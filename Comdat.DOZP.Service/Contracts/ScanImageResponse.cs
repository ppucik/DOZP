using System;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Comdat.DOZP.Services
{
    [DataContract]
    public class ScanImageResponse
    {
        public ScanImageResponse()
        {
        }

        public ScanImageResponse(int scanFileID, byte[] image)
        {
            this.ScanFileID = scanFileID;
            this.Image = image;
        }

        [DataMember(IsRequired=true)]
        public int ScanFileID { get; set; }

        [DataMember]
        public byte[] Image { get; set; }
    }
}
