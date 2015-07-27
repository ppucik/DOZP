using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web.Profile;
using System.Web.Security;

using Comdat.DOZP.Core;

namespace Comdat.DOZP.Services
{
    public class UserProfile : ProfileBase
    {
        #region Private constants
        const string USERNAME = "UserName";
        const string FULLNAME = "FullName";
        const string TELEPHONE = "Telephone";
        const string INSTITUTION_ID = "InstitutionID";
        #endregion

        #region Public methods

        public static UserProfile GetUserProfile()
        {
            if ((Membership.GetUser() != null))
            {
                return (Create(Membership.GetUser().UserName) as UserProfile);
            }
            else
            {
                return null;
            }
        }

        public static UserProfile GetUserProfile(string username)
        {
            if (!String.IsNullOrEmpty(username))
            {
                return (Create(username) as UserProfile);
            }
            else
            {
                throw new ArgumentNullException(USERNAME);
            }
        }

        #endregion

        #region Public properties

        [SettingsAllowAnonymous(false), CustomProviderData("FullName;nvarchar;200")]
        public string FullName
        {
            get
            {
                return (base.GetPropertyValue(FULLNAME) as string);
            }
            set
            {
                if (!String.IsNullOrEmpty(value) && (value.Length <= 200))
                {
                    base.SetPropertyValue(FULLNAME, value);
                }
                else
                {
                    throw new ArgumentException(string.Format("Hodnota {0} není v rozsahu 1 až 200 znaků", FULLNAME));
                }
            }
        }

        [SettingsAllowAnonymous(false), CustomProviderData("Telephone;nvarchar;100")]
        public string Telephone
        {
            get
            {
                return (base.GetPropertyValue(TELEPHONE) as string);
            }
            set
            {
                if (value.Length <= 100)
                {
                    base.SetPropertyValue(TELEPHONE, value);
                }
                else
                {
                    throw new ArgumentException(string.Format("Hodnota {0} není v rozsahu 0 až 100 znaků", TELEPHONE));
                }
            }
        }

        [SettingsAllowAnonymous(false), CustomProviderData("InstitutionID;int")]
        public int? InstitutionID
        {
            get
            {
                return (base.GetPropertyValue(INSTITUTION_ID) as int?);
            }
            set
            {
                if (value != 0)
                {
                    base.SetPropertyValue(INSTITUTION_ID, value);
                }
                else
                {
                    throw new ArgumentException("Hodnota InstitutionID=0 není platná");
                }
            }
        }

        //[SettingsAllowAnonymous(false), CustomProviderData("InstitutionID;int")]
        //public int InstitutionID
        //{
        //    get
        //    {
        //        return Convert.ToInt32(base.GetPropertyValue(INSTITUTION_ID));
        //    }
        //    set
        //    {
        //        int? lid = (value > 0 ? value : default(int?));
        //        base.SetPropertyValue(INSTITUTION_ID, lid);
        //    }
        //}

        #endregion

        #region Util methods

        //public User GetUserInfo()
        //{
        //    User user = new User();
        //    user.FullName = this.FullName;
        //    user.Telephone = this.Telephone;
        //    user.InstitutionID = this.InstitutionID;
        //    return user;
        //}

        public void SaveProfile(User user)
        {
            if ((user == null)) throw new ArgumentNullException("user");

            this.FullName = user.FullName;
            this.Telephone = user.Telephone;
            this.InstitutionID = user.InstitutionID;
            //this.InstitutionID = (user.InstitutionID.HasValue ? user.InstitutionID.Value : 0);
            this.Save();
        }

        #endregion
    }
}
