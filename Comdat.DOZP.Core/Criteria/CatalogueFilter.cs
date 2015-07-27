using System;
using System.Runtime.Serialization;

namespace Comdat.DOZP.Core
{
    [DataContract]
    public class CatalogueFilter
    {
        public CatalogueFilter()
        {
        }

        public CatalogueFilter(int id)
            : this()
        {
            this.CatalogueID = id;
        }

        public CatalogueFilter(bool active)
            : this()
        {
            this.Active = active;
        }

        [DataMember]
        public int CatalogueID { get; set; }

        [DataMember]
        public int InstitutionID { get; set; }

        [DataMember]
        public string UserRole { get; set; }

        [DataMember]
        public bool Active { get; set; }
    }
}
