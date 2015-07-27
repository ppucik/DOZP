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

namespace Comdat.DOZP.Web.Admin
{
    public partial class UserEdit : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    string userName = Request.QueryString["name"];

                    if (String.IsNullOrEmpty(userName))
                    {
                        this.TitleLabel.Text = "Vytvořit nového uživatele";
                        //this.IsApprovedCheckBox.Visible = false;
                        this.IsApprovedCheckBox.Checked = true;
                        this.ChangePasswordButton.Visible = false;
                        this.DeleteButton.Visible = false;

                        if (Roles.IsUserInRole(RoleConstants.SUPERVISOR))
                        {
                            this.RolesDropDownList.SelectedValue = RoleConstants.CATALOGUER;
                        }
                    }
                    else
                    {
                        User user = UserComponent.Instance.GetByName(userName);
                        this.UserNameTextBox.Enabled = false;
                        this.UserNameTextBox.Text = user.UserName;
                        this.FullNameTextBox.Text = user.FullName;
                        this.InstitutionsDropDownList.SelectedValue = user.InstitutionID.Value;
                        this.RolesDropDownList.SelectedValue = user.RoleName;
                        this.EmailTextBox.Text = user.Email;
                        this.TelephoneTextBox.Text = user.Telephone;
                        this.CommentTextBox.Text = user.Comment;
                        this.IsApprovedCheckBox.Checked = user.IsApproved;

                        this.DeleteButton.Visible = UserComponent.Instance.IsDeletable(user.UserName);
                        this.DeleteButton.OnClientClick = String.Format("return confirm('Skutečně chcete odstranit uživatele {0} ?')", user.UserName);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLabel.Text = String.Format("Chyba: {0}", ex.Message);
            }
        }

        protected void SaveButton_Click(object sender, EventArgs e)
        {
            ErrorLabel.Text = "";

            try
            {
                if (Page.IsValid)
                {
                    string userName = this.UserNameTextBox.Text.Trim();
                    string fullName = this.FullNameTextBox.Text.Trim();
                    int institutionID = this.InstitutionsDropDownList.SelectedValue;
                    string roleName = this.RolesDropDownList.SelectedValue;
                    string email = this.EmailTextBox.Text.Trim();
                    string telephone = this.TelephoneTextBox.Text.Trim();
                    string comment = this.CommentTextBox.Text.Trim();
                    bool isApproved = this.IsApprovedCheckBox.Checked;

                    MembershipUser user = Membership.GetUser(userName);
                    string url = String.Empty;

                    if (user == null)
                    {
                        MembershipCreateStatus status;
                        string password = Membership.GeneratePassword(Membership.MinRequiredPasswordLength, Membership.MinRequiredNonAlphanumericCharacters);
                        user = Membership.CreateUser(userName, password, email, null, null, isApproved, out status);

                        if (status == MembershipCreateStatus.Success)
                        {
                            user.Comment = comment;
                            Membership.UpdateUser(user);
                            Roles.AddUserToRole(user.UserName, roleName);

                            url = String.Format("ChangePassword.aspx?name={0}", userName);
                        }
                        else
                        {
                            ErrorLabel.Text = String.Format("Chyba: {0}", Resources.MembershipResource.ResourceManager.GetString(status.ToString()));
                            return;
                        }
                    }
                    else
                    {
                        user.Email = email;
                        user.Comment = comment;
                        user.IsApproved = isApproved;
                        Membership.UpdateUser(user);

                        if (!Roles.IsUserInRole(userName, roleName))
                        {
                            string[] roleNames = Roles.GetRolesForUser(userName);
                            Roles.RemoveUserFromRoles(userName, roleNames);
                            Roles.AddUserToRole(userName, roleName);
                        }

                        url = String.Format("Users.aspx?institutionID={0}", institutionID);
                    }

                    if (user != null)
                    {
                        UserProfile profile = UserProfile.GetUserProfile(user.UserName);
                        if (profile != null)
                        {
                            profile.FullName = fullName;
                            profile.InstitutionID = institutionID;
                            profile.Telephone = telephone;
                            profile.Save();
                        }
                    }
                    else
                    {
                        ErrorLabel.Text = String.Format("Uživatel {0} neexistuje.", userName);
                    }

                    if (!String.IsNullOrEmpty(url))
                    {
                        Response.Redirect(url, false);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLabel.Text = String.Format("Chyba: {0}", ex.Message);
            }
        }

        protected void DeleteButton_Click(object sender, EventArgs e)
        {
            ErrorLabel.Text = "";

            try
            {
                string userName = this.UserNameTextBox.Text.Trim();
                MembershipUser user = Membership.GetUser(userName);

                if (user != null)
                {
                    UserProfile profile = UserProfile.GetUserProfile(user.UserName);
                    int institutionID = (profile != null ? profile.InstitutionID.Value : 0);

                    if (Membership.DeleteUser(user.UserName))
                    {
                        string url = String.Format("Users.aspx?institutionID={0}", institutionID);
                        Response.Redirect(url, false);
                    }
                    else
                    {
                        ErrorLabel.Text = String.Format("Uživatele {0} se nepodařilo odstranit.", userName);
                    }
                }
                else
                {
                    ErrorLabel.Text = String.Format("Uživatel {0} neexistuje.", userName);
                }
            }
            catch (Exception ex)
            {
                ErrorLabel.Text = String.Format("Chyba: {0}", ex.Message);
            }
        }

        protected void ChangePasswordButton_Click(object sender, EventArgs e)
        {
            string userName = this.UserNameTextBox.Text.Trim();

            if (!String.IsNullOrEmpty(userName))
            {
                string url = String.Format("ChangePassword.aspx?name={0}", userName);
                Response.Redirect(url);
            }
        }

        protected void StornoButton_Click(object sender, EventArgs e)
        {
            int institutionID = this.InstitutionsDropDownList.SelectedValue;
            string url = String.Format("Users.aspx?institutionID={0}", institutionID);

            Response.Redirect(url);
        }
    }
}