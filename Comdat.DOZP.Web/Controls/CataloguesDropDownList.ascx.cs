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
    public partial class CataloguesDropDownList : System.Web.UI.UserControl
    {
        #region Private members
        private bool _emptyVisible = false;
        #endregion

        #region Public events
        public delegate void SelectedChangedEventHandler(object sender, EventArgs e);
        public event SelectedChangedEventHandler SelectedChanged;
        #endregion

        #region Public properties

        public bool AutoPostBack
        {
            get
            {
                return this.DropDownList.AutoPostBack;
            }
            set
            {
                this.DropDownList.AutoPostBack = value;
            }
        }

        public bool TitleVisible
        {
            get
            {
                return this.TitleLabel.Visible;
            }
            set
            {
                if (value)
                {
                    this.DropDownListPanel.CssClass = "DropDownListControl";
                    //this.DropDownList.CssClass = "DropDownListItems";
                }

                this.TitleLabel.Visible = value;
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

        public int SelectedValue
        {
            get
            {
                return Int32.Parse(this.DropDownList.SelectedValue);
            }
            set
            {
                this.DropDownList.SelectedValue = value.ToString();
            }
        }

        public string Width
        {
            get
            {
                return this.DropDownList.Width.ToString();
            }
            set
            {
                this.DropDownList.Width = new Unit(value);
            }
        }

        #endregion

        #region UserControl events

        protected void Page_Init(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                IList<Catalogue> catalogues = null;

                if (Roles.IsUserInRole(RoleConstants.ADMINISTRATOR))
                {
                    if (this.EmptyVisible)
                    {
                        this.DropDownList.Items.Add(new ListItem("(Všechny)", "0"));
                    }

                    catalogues = CatalogueComponent.Instance.GetAll();
                }
                else
                {
                    UserProfile profile = UserProfile.GetUserProfile();

                    if (profile != null && profile.InstitutionID.HasValue)
                    {
                        int institutionID = profile.InstitutionID.Value;

                        if (institutionID != 0)
                        {
                            catalogues = CatalogueComponent.Instance.GetByInstitutionID(institutionID, "Name");
                        }
                    }
                }

                if (catalogues != null)
                {
                    foreach (var c in catalogues)
                    {
                        this.DropDownList.Items.Add(new ListItem(c.Name, c.CatalogueID.ToString()));
                    }
                }

                OnSelectedChanged();
            }
        }

        protected void DropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            //string catalogueID = this.DropDownList.SelectedValue;

            //if (!String.IsNullOrEmpty(catalogueID))
            //{
            //    Profile.DefaultCatalogue = catalogueID;
            //    Profile.Save();
            //}

            OnSelectedChanged();
        }

        protected void OnSelectedChanged()
        {
            if (this.SelectedChanged != null)
                this.SelectedChanged(this, new EventArgs());
        }

        #endregion
    }
}