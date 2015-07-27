using System;
using System.Runtime.Serialization;

namespace Comdat.DOZP.Core
{
    [DataContract]
    public class UserFilter
    {
        public UserFilter()
        {
        }

        public UserFilter(string userName)
            : this()
        {
            this.UserName = userName;
        }

        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public int InstitutionID { get; set; }

        [DataMember]
        public int CatalogueID { get; set; }

        [DataMember]
        public string RoleName { get; set; }

        [DataMember]
        public bool? IsApproved { get; set; }
    }
}
