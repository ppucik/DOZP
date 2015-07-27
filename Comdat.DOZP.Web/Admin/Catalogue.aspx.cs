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
    public partial class Catalogue : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!Page.IsPostBack)
                {
                    int catalogueID = Convert.ToInt32(Request.QueryString["id"]);

                    if (!Int32.TryParse(Request.QueryString["id"], out catalogueID))
                    {
                        if (Roles.IsUserInRole(RoleConstants.ADMINISTRATOR))
                        {
                            this.TitleLabel.Text = "Vytvořit nový katalog";
                            this.EnabledCheckBox.Checked = true;
                        }
                        else
                        {
                            FormsAuthentication.RedirectToLoginPage();
                        }
                    }
                    else
                    {
                        var catalogue = CatalogueComponent.Instance.GetByID(catalogueID);

                        if (catalogue != null)
                        {
                            this.DatabaseNameTextBox.Enabled = Roles.IsUserInRole(RoleConstants.ADMINISTRATOR);
                            this.DatabaseNameTextBox.Text = catalogue.DatabaseName;
                            this.NameTextBox.Text = catalogue.Name;
                            this.DescriptionTextBox.Text = catalogue.Description;
                            this.ZServerUrlTextBox.Text = catalogue.ZServerUrl;
                            this.ZServerPortTextBox.Text = catalogue.ZServerPort.ToString();
                            this.CharsetTextBox.Text = catalogue.Charset;
                            this.EnabledCheckBox.Checked = catalogue.Enabled;

                            this.DeleteButton.Visible = this.DatabaseNameTextBox.Enabled;
                            this.DeleteButton.OnClientClick = String.Format("return confirm('Skutečně chcete odstranit katalog {0} ?')", catalogue.DatabaseName);
                        }
                        else
                        {
                            ErrorLabel.Text = String.Format("Katalog ID={0} nebyl nalezen", catalogueID);
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
                    int catalogueID = Convert.ToInt32(Request.QueryString["id"]);
                    Comdat.DOZP.Core.Catalogue catalogue = null;

                    if (catalogueID == 0)
                        catalogue = new Comdat.DOZP.Core.Catalogue();
                    else
                        catalogue = CatalogueComponent.Instance.GetByID(catalogueID);

                    catalogue.DatabaseName = this.DatabaseNameTextBox.Text.Trim();
                    catalogue.Name = this.NameTextBox.Text.Trim();
                    catalogue.Description = this.DescriptionTextBox.Text.Trim();
                    catalogue.ZServerUrl = this.ZServerUrlTextBox.Text.Trim();
                    catalogue.ZServerPort = Convert.ToInt32(this.ZServerPortTextBox.Text);
                    catalogue.Charset = this.CharsetTextBox.Text.Trim();
                    catalogue.Enabled = this.EnabledCheckBox.Checked;

                    CatalogueComponent.Instance.Save(catalogue);
                    Response.Redirect("Catalogues.aspx", false);
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
                int catalogueID = Convert.ToInt32(Request.QueryString["id"]);
                CatalogueComponent.Instance.Delete(catalogueID);
                Response.Redirect("Catalogues.aspx", false);
            }
            catch (Exception ex)
            {
                ErrorLabel.Text = String.Format("Chyba: {0}", ex.Message);
            }          
        }

        protected void StornoButton_Click(object sender, EventArgs e)
        {
            //int institutionID = this.InstitutionsDropDownList.SelectedValue;
            //string url = String.Format("Catalogues.aspx?institutionID={0}", institutionID);

            Response.Redirect("Catalogues.aspx");
        }
    }
}