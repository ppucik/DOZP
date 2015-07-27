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
    [ValidationProperty("SelectedValue")]
    public partial class InstitutionsDropDownList : System.Web.UI.UserControl
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
                int institutionID = 0;

                if (Roles.IsUserInRole(RoleConstants.ADMINISTRATOR))
                {
                    if (this.EmptyVisible)
                    {
                        this.DropDownList.Items.Add(new ListItem("(Všechny)", "0"));
                    }

                    IList<Institution> institutions = InstitutionComponent.Instance.GetAll();

                    if (institutions != null)
                    {
                        foreach (var i in institutions)
                        {
                            this.DropDownList.Items.Add(new ListItem(i.Name, i.InstitutionID.ToString()));
                        }
                    }
                }
                else if (Roles.IsUserInRole(RoleConstants.SUPERVISOR))
                {
                    UserProfile profile = UserProfile.GetUserProfile();

                    if (profile != null && profile.InstitutionID.HasValue)
                    {
                        institutionID = profile.InstitutionID.Value;
                        Institution institution = InstitutionComponent.Instance.GetByID(institutionID);

                        if (institution != null)
                        {
                            this.DropDownList.Items.Add(new ListItem(institution.Name, institution.InstitutionID.ToString()));
                        }
                    }
                }
                else
                {
                    throw new System.Security.SecurityException();
                }

                SelectedValue = institutionID;

                OnSelectedChanged();
            }
        }

        protected void DropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
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