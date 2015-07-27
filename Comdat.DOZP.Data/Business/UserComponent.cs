using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Comdat.DOZP.Core;
using Comdat.DOZP.Data.Repository;

namespace Comdat.DOZP.Data.Business
{
    public class UserComponent
    {
        private static readonly UserComponent _instance = new UserComponent();

        public static UserComponent Instance
        {
            get
            {
                return _instance;
            }
        }

        public List<User> GetAll()
        {
            UserRepository repository = new UserRepository();
            return repository.Select();
        }

        public User GetByName(string userName)
        {
            UserRepository repository = new UserRepository();
            return repository.Select(userName);
        }

        public List<User> GetByInstitutionID(int institutionID, bool active, string sortExpression)
        {
            UserFilter filter = new UserFilter();
            filter.InstitutionID = institutionID;
            filter.IsApproved = (active ? (bool?)true : null);

            return GetList(filter).OrderBy(sortExpression).ToList();
        }

        public List<User> GetList(UserFilter filter)
        {
            if (filter == null) throw new ArgumentNullException("filter");

            UserRepository repository = new UserRepository();
            return repository.Select(filter);
        }

        public bool IsDeletable(string userName)
        {
            User user = GetByName(userName);

            if (user != null && user.RoleName != RoleConstants.ADMINISTRATOR)
            {
                OperationRepository repository = new OperationRepository();
                return (repository.GetCount(user.UserName) == 0);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        /// <example>
        ///User user = UserComponent.Instance.GetByName(userName);
        ///user.InstitutionID = institutionID;
        ///user.FullName = fullName;
        ///user.RoleName = roleName;
        ///user.Telephone = telephone;
        ///user.Email = email;
        ///user.Comment = comment;
        ///user.IsApproved = isApproved;
        ///UserComponent.Instance.Save(user);
        /// </example>
        public User Save(User user)
        {
            if (user == null) throw new ArgumentNullException("user");

            UserRepository repository = new UserRepository();
            User original = repository.Select(user.UserName);

            if (original == null)
                throw new ApplicationException(String.Format("Uživatel '{0}' neexistuje.", user.UserName));

            //original.RoleName = user.RoleName;
            original.Email = user.Email;
            original.Comment = user.Comment;
            original.FullName = user.FullName;
            original.Telephone = user.Telephone;
            original.InstitutionID = user.InstitutionID;
            original.LastUpdate = DateTime.Now;
            original.IsApproved = user.IsApproved;

            return repository.Update(user);
        }
    }
}
