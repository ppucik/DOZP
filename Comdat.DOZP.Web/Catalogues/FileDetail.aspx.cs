using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Comdat.DOZP.Core;
using Comdat.DOZP.Data.Business;

namespace Comdat.DOZP.Web.Catalogues
{
    public partial class FileDetail : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                int catalogueID = 0;

                if (Int32.TryParse(Request.QueryString["id"], out catalogueID))
                {
                    Catalogue catalogue = CatalogueComponent.Instance.GetByID(catalogueID);

                    if (catalogue != null)
                    {
                        this.TitleLabel.Text = catalogue.Name;
                        this.UsersDropDownList.LoadUsers(catalogue.CatalogueID);
                    }
                    else
                    {
                        throw new ArgumentException(String.Format("Katalog ID={0} nebyl nalezen", catalogueID));
                    }
                }
                else
                {
                    throw new ArgumentException("Neplatný parametr katalog v dotazu", "ID");
                }
            }
        }

        protected void FilesDetailsView_DataBound(object sender, EventArgs e)
        {
            if (FilesDetailsView.SelectedValue != null)
            {
                int scanFileID = (int)FilesDetailsView.SelectedValue;

                //string path = ScanFile.GetFilePath(databaseName, fileName);
            }
        }

        protected void FilesDataSource_Selected(object sender, ObjectDataSourceStatusEventArgs e)
        {
            int count = 0;

            if (e.ReturnValue != null)
            {
                count = (e.ReturnValue as List<ScanFile>).Count;
                this.SummaryLabel.Text = String.Format("Počet záznamů: {0}", count);
                this.SummaryLabel.Visible = (count > 0);
            }
        }
    }
}