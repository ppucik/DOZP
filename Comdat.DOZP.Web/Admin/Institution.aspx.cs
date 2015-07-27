using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

using Comdat.DOZP.Core;
using Comdat.DOZP.Data.Business;

namespace Comdat.DOZP.Web.Admin
{
    public partial class Institution : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!Page.IsPostBack)
                {
                    int institutionID = Convert.ToInt32(Request.QueryString["id"]);

                    if (!Int32.TryParse(Request.QueryString["id"], out institutionID))
                    {
                        if (Roles.IsUserInRole(RoleConstants.ADMINISTRATOR))
                        {
                            this.TitleLabel.Text = "Nová instituce";
                            this.SiglaTextBox.Enabled = true;
                        }
                        else
                        {
                            FormsAuthentication.RedirectToLoginPage();
                        }
                    }
                    else
                    {
                        var institution = InstitutionComponent.Instance.GetByID(institutionID);

                        if (institution != null)
                        {
                            this.SiglaTextBox.Text = institution.Sigla;
                            this.NameTextBox.Text = institution.Name;
                            this.DescriptionTextBox.Text = institution.Description;
                            this.AddressTextBox.Text = institution.Address;
                            this.HomepageTextBox.Text = institution.Homepage;
                            this.EmailTextBox.Text = institution.Email;
                            this.TelephoneTextBox.Text = institution.Telephone;

                            this.DeleteButton.Visible = (Roles.IsUserInRole(RoleConstants.ADMINISTRATOR));
                            this.DeleteButton.OnClientClick = String.Format("return confirm('Skutečně chcete odstranit instituci {0} ?')", institution.Sigla);
                        }
                        else
                        {
                            throw new ArgumentException(String.Format("Instituce ID={0} nebyla nalezena", institutionID));
                        }
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
                    int institutionID = Convert.ToInt32(Request.QueryString["id"]);
                    Comdat.DOZP.Core.Institution institution = null;

                    if (institutionID == 0)
                        institution = new Comdat.DOZP.Core.Institution();
                    else
                        institution = InstitutionComponent.Instance.GetByID(institutionID);

                    institution.Sigla = this.SiglaTextBox.Text.ToUpper();
                    institution.Name = this.NameTextBox.Text.Trim();
                    institution.Description = this.DescriptionTextBox.Text.Trim();
                    institution.Address = this.AddressTextBox.Text.Trim();
                    institution.Homepage = this.HomepageTextBox.Text.Trim();
                    institution.Email = this.EmailTextBox.Text.Trim();
                    institution.Telephone = this.TelephoneTextBox.Text.Trim();

                    InstitutionComponent.Instance.Save(institution);
                    Response.Redirect("Institutions.aspx", false);
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
                int institutionID = Convert.ToInt32(Request.QueryString["id"]);
                InstitutionComponent.Instance.Delete(institutionID);
                Response.Redirect("Institutions.aspx", false);
            }
            catch (Exception ex)
            {
                ErrorLabel.Text = String.Format("Chyba: {0}", ex.Message);
            }
        }

        protected void StornoButton_Click(object sender, EventArgs e)
        {
            Response.Redirect("Institutions.aspx");
        }
    }
}