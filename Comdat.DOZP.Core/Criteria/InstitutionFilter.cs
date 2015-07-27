using System;
using System.Runtime.Serialization;

namespace Comdat.DOZP.Core
{
    [DataContract]
    public class InstitutionFilter
    {
        public InstitutionFilter()
        {
        }

        public InstitutionFilter(bool active)
            : this()
        {
            this.Active = active;
        }

        public InstitutionFilter(int id)
            : this()
        {
            this.InstitutionID = id;
        }

        [DataMember]
        public int InstitutionID { get; set; }

        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public bool Active { get; set; }
    }
}
