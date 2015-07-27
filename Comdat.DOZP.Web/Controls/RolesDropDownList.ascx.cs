using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

using Comdat.DOZP.Core;

namespace Comdat.DOZP.Web.Controls
{
    [ValidationProperty("SelectedValue")]
    public partial class RolesDropDownList : System.Web.UI.UserControl
    {
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

        public string SelectedValue
        {
            get
            {
                return this.DropDownList.SelectedValue;
            }
            set
            {
                if (Roles.RoleExists(value))
                    this.DropDownList.SelectedValue = value;
            }
        }

        public bool Enabled
        {
            get
            {
                return this.DropDownList.Enabled;
            }
            set
            {
                this.DropDownList.Enabled = value;
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
            //string[] roles = Roles.GetAllRoles();
            //string name = String.Empty;
            //
            //foreach (string role in roles)
            //{
            //    name = Resources.RolesResource.ResourceManager.GetString(role);
            //    if (String.IsNullOrEmpty(name))
            //    {
            //        name = role;
            //    }
            //    this.DropDownList.Items.Add(new ListItem(name, role));
            //}

            if (Roles.IsUserInRole(RoleConstants.ADMINISTRATOR))
            {
                this.DropDownList.Items.Add(new ListItem(Resources.RolesResource.ResourceManager.GetString(RoleConstants.ADMINISTRATOR), RoleConstants.ADMINISTRATOR));
            }

            this.DropDownList.Items.Add(new ListItem(Resources.RolesResource.ResourceManager.GetString(RoleConstants.SUPERVISOR), RoleConstants.SUPERVISOR));
            this.DropDownList.Items.Add(new ListItem(Resources.RolesResource.ResourceManager.GetString(RoleConstants.CATALOGUER), RoleConstants.CATALOGUER));
            this.DropDownList.Items.Add(new ListItem(Resources.RolesResource.ResourceManager.GetString(RoleConstants.USER_OCR), RoleConstants.USER_OCR));
        }

        #endregion
    }
}