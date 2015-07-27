using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;

namespace Comdat.DOZP.Core
{
    [DataContract(IsReference = true)]
    public partial class User
    {
        public User()
        {
            this.Operations = new List<Operation>();
            this.Roles = new List<Role>();
        }

        [DataMember]
        public string UserName { get; set; }

        public byte[] PasswordHash { get; set; }

        public byte[] PasswordSalt { get; set; }

        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public string Comment { get; set; }

        [DataMember]
        public bool IsApproved { get; set; }

        public DateTime DateCreated { get; set; }

        public Nullable<DateTime> DateLastLogin { get; set; }

        public Nullable<DateTime> DateLastActivity { get; set; }

        public DateTime DateLastPasswordChange { get; set; }

        public Nullable<DateTime> DateLastLogout { get; set; }

        public DateTime LastUpdate { get; set; }

        [DataMember]
        public Nullable<int> InstitutionID { get; set; }

        [DataMember]
        public string FullName { get; set; }

        [DataMember]
        public string Telephone { get; set; }

        public virtual Institution Institution { get; set; }
        public virtual ICollection<Operation> Operations { get; set; }
        public virtual ICollection<Role> Roles { get; set; }

        [DataMember]
        public string RoleName { get; set; }
    }
}
