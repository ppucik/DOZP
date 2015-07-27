using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

using Comdat.DOZP.Core;

namespace Comdat.DOZP.Web.Admin
{
    public partial class Catalogues : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Roles.IsUserInRole(RoleConstants.ADMINISTRATOR))
                {
                    this.InsertHyperLink.NavigateUrl = "Catalogue.aspx";

                }

                int institutionID = Convert.ToInt32(Request.QueryString["institutionID"]);
                this.InstitutionsDropDownList.SelectedValue = institutionID;
            }
        }
    }
}