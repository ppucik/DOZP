using System;
using System.Runtime.Serialization;

namespace Comdat.DOZP.Services
{
    [DataContract]
    public class DozpServiceFault
    {
        [DataMember]
        public string Message { get; set; }

        public DozpServiceFault(string message)
        {
            this.Message = message;
        }
    }
}
