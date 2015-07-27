using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

using Comdat.DOZP.Core;
using Comdat.DOZP.Data.Business;
using Comdat.DOZP.Services;

namespace Comdat.DOZP.Web.Controls
{
    public partial class UsersDropDownList : System.Web.UI.UserControl
    {
        #region Private members
        private int _institutionID = 0;
        private bool _emptyVisible = true;
        #endregion

        #region Public properties

        public bool HeaderVisible
        {
            get
            {
                return this.FilterHeader.Visible;
            }
            set
            {
                this.FilterHeader.Visible = value;
            }
        }

        public bool EmptyVisible
        {
            get
            {
                return _emptyVisible;
            }
            set
            {
                _emptyVisible = value;
            }
        }

        public string SelectedValue
        {
            get
            {
                return this.DropDownList.SelectedValue;
            }
            set
            {
                this.DropDownList.SelectedValue = value;
            }
        }

        public int CatalogueID
        {
            get
            {
                return _institutionID;
            }
            private set
            {
                _institutionID = value;
            }
        }

        #endregion

        #region Public methods

        public void LoadUsers(int catalogueID)
        {
            this.CatalogueID = catalogueID;

            //Institution institution = InstitutionComponent.Instance.GetByID(this.InstitutionID);

            //if (institution != null)
            //{
            //    this.FilterHeader.InnerText = (institution.Sigla == "") ? "Katalogizátoři" : "Pracovníci OCR";
            //}

            this.DropDownList.Items.Clear();

            if (Roles.IsUserInRole(RoleConstants.ADMINISTRATOR) || Roles.IsUserInRole(RoleConstants.SUPERVISOR))
            {
                UserFilter filter = new UserFilter();
                filter.CatalogueID = this.CatalogueID;
                filter.IsApproved = true;

                this.DropDownList.DataTextField = "FullName";
                this.DropDownList.DataValueField = "UserName";
                this.DropDownList.DataSource = UserComponent.Instance.GetList(filter);
                this.DropDownList.DataBind();

                if (EmptyVisible)
                {
                    this.DropDownList.Items.Insert(0, new ListItem("(Všichni)", String.Empty));
                }
            }
            else
            {
                UserProfile profile = UserProfile.GetUserProfile();
                if (profile != null)
                {
                    this.DropDownList.Items.Add(new ListItem(profile.FullName, profile.UserName));
                }
            }
        }

        #endregion
    }
}