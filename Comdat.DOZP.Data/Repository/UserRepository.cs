using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

using Comdat.DOZP.Core;

namespace Comdat.DOZP.Data.Repository
{
    public class UserRepository
    {
        public List<User> Select()
        {
            List<User> list = null;

            using (var db = new DozpContext())
            {
                list = db.Users.Include(e => e.Roles).OrderBy(o => o.FullName).ToList();
            }

            if (list != null)
            {
                foreach (var item in list)
                {
                    SetUserRoleName(item);
                }
            }

            return list;
        }

        public User Select(string userName)
        {
            if (String.IsNullOrEmpty(userName)) return null;

            User item = null;

            using (var db = new DozpContext())
            {
                item = db.Users
                     .Include(e => e.Roles)
                     .Where(pk => pk.UserName == userName)
                     .SingleOrDefault();
            }

            SetUserRoleName(item);

            return item;
        }

        public List<User> Select(UserFilter filter)
        {
            if (filter == null) return null;

            List<User> list = null;

            using (var db = new DozpContext())
            {
                list = (from u in db.Users.Include(e => e.Roles)
                        where (String.IsNullOrEmpty(filter.UserName) || u.UserName == filter.UserName) &&
                              (0 == filter.InstitutionID || u.InstitutionID == filter.InstitutionID) &&
                              (0 == filter.CatalogueID || u.Institution.Catalogues.Count(c => c.CatalogueID == filter.CatalogueID) > 0) &&
                              (String.IsNullOrEmpty(filter.RoleName) || u.Roles.Count(r => r.RoleName == filter.RoleName) == 1) &&
                              (!filter.IsApproved.HasValue || u.IsApproved == filter.IsApproved.Value) &&
                              (u.UserName != "system") 
                        orderby u.FullName
                        select u).ToList();
            }

            if (list != null)
            {
                foreach (var item in list)
                {
                    SetUserRoleName(item);
                }
            }

            return list;
        }

        private void SetUserRoleName(User user)
        {
            if (user != null && user.Roles != null)
            {
                user.RoleName = user.Roles.SingleOrDefault().RoleName;
            }
        }

        public User Update(User user)
        {
            if (user == null) return null;

            using (var db = new DozpContext())
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
            }

            return user;
        }
    }
}
