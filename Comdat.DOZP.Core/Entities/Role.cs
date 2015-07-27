using System;
using System.Collections.Generic;

namespace Comdat.DOZP.Core
{
    public partial class Role
    {
        public Role()
        {
            this.Users = new List<User>();
        }

        public string RoleName { get; set; }
        public string Description { get; set; }

        public virtual ICollection<User> Users { get; set; }
    }
}
