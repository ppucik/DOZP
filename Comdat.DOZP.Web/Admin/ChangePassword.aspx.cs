using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

using Comdat.DOZP.Core;
using Comdat.DOZP.Services;

namespace Comdat.DOZP.Web.Admin
{
    public partial class ChangePassword : System.Web.UI.Page
    {
        protected void Page_Init(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                this.NewPasswordRegularExpressionValidator.ErrorMessage = String.Format("Minimální délka je {0} znaků", Membership.MinRequiredPasswordLength);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                this.UserNameTextBox.Text = Request.QueryString["name"];
            }
        }

        protected void SaveButton_Click(object sender, EventArgs e)
        {
            ErrorLabel.Text = "";

            try
            {
                if (Page.IsValid)
                {
                    string userName = this.UserNameTextBox.Text;

                    if (!String.IsNullOrEmpty(userName))
                    {
                        if (Roles.IsUserInRole(RoleConstants.ADMINISTRATOR) || Roles.IsUserInRole(RoleConstants.SUPERVISOR))
                        {
                            MembershipUser user = Membership.GetUser(userName);
                            if (user != null)
                            {
                                string tempPassword = user.ResetPassword();
                                string newPassword = this.NewPasswordTextBox.Text;

                                if (user.ChangePassword(tempPassword, newPassword))
                                {
                                    UserProfile profile = UserProfile.GetUserProfile(user.UserName);
                                    int institutionID = (profile != null ? profile.InstitutionID.Value : 0);
                                    string url = String.Format("Users.aspx?institutionID={0}", institutionID);
                                    
                                    Response.Redirect(url, false);
                                }
                                else
                                {
                                    ErrorLabel.Text = String.Format("Uživateli {0} se nepodařilo změnit heslo.", userName);
                                }
                            }
                            else
                            {
                                ErrorLabel.Text = String.Format("Uživatel {0} neexistuje.", userName);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLabel.Text = String.Format("Chyba: {0}", ex.Message);
            }
        }

        protected void StornoButton_Click(object sender, EventArgs e)
        {
            string url = String.Format("UserEdit.aspx?name={0}", this.UserNameTextBox.Text);

            Response.Redirect(url);
        }
    }
}