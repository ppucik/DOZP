using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

using Comdat.DOZP.Core;

namespace Comdat.DOZP.Web.Account
{
    public partial class Download : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Roles.IsUserInRole(RoleConstants.ADMINISTRATOR))
                {
                    //DozpScanApp.Visible = false;
                    DozpScanAppHyperLink.Visible = true;
                    DozpOcrAppHyperLink.Visible = true;
                    TestWcfServiceHyperLink.Visible = true;
                }
                else if (Roles.IsUserInRole(RoleConstants.SUPERVISOR))
                {
                    DozpScanAppHyperLink.Visible = true;
                }
                else if (Roles.IsUserInRole(RoleConstants.CATALOGUER))
                {
                    DozpScanAppHyperLink.Visible = true;
                }
                else if (Roles.IsUserInRole(RoleConstants.USER_OCR))
                {
                    DozpOcrAppHyperLink.Visible = true;
                }
                else
                {
                    //DozpScanApp.Visible = false;
                    DozpScanAppHyperLink.Visible = false;
                    DozpOcrAppHyperLink.Visible = false;
                    TestWcfServiceHyperLink.Visible = false;
                }
            }
        }
    }
}