using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

using Comdat.DOZP.Core;
using Comdat.DOZP.Services;

namespace Comdat.DOZP.Web.Admin
{
    public partial class Institutions : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Roles.IsUserInRole(RoleConstants.ADMINISTRATOR))
                {
                    this.InsertHyperLink.NavigateUrl = "Institution.aspx";
                    
                }
                else if (Roles.IsUserInRole(RoleConstants.SUPERVISOR))
                {
                    UserProfile profile = UserProfile.GetUserProfile();
                    if (profile != null && profile.InstitutionID.HasValue)
                    {
                        string institutionID = profile.InstitutionID.Value.ToString();
                        this.InstitutionsObjectDataSource.SelectMethod = "GetByID";
                        this.InstitutionsObjectDataSource.SelectParameters.Add("institutionID", DbType.Int32, institutionID);
                    }

                    this.InsertHyperLink.NavigateUrl = null;      
                }
                else
                {
                    FormsAuthentication.RedirectToLoginPage();
                }
            }
        }
    }
}