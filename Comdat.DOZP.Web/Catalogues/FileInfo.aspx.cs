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
    public partial class FileInfo : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                int scanFileID = 0;

                if (Int32.TryParse(Request.QueryString["id"], out scanFileID))
                {
                    ScanFile file = ScanFileComponent.Instance.GetByID(scanFileID);

                    if (file != null)
                    {
                        this.TitleLabel.Text = String.Format("{0} č. {1}", file.PartOfBook.ToDisplay(), file.Book.SysNo);
                        this.FileImage.ImageUrl = String.Format("FileStream.aspx?path={0}&width={1}", file.GetScanFilePath(), this.FileImage.Width.Value);
                        
                        if (file.PartOfBook == PartOfBook.FrontCover && !String.IsNullOrEmpty(file.Book.ISBN))
                        {
                            if (!String.IsNullOrEmpty(file.Book.ISBN))
                            {
                                this.FileHyperLink.NavigateUrl = String.Format("http://obalkyknih.cz/view?isbn={0}", file.Book.ISBN);
                            }
                            this.FilesDetailsView.Fields[16].Visible = false;
                        }
                    }
                    else
                    {
                        throw new ArgumentException(String.Format("Záznam ID={0} nebyl nalezen", scanFileID));
                    }
                }
                else
                {
                    throw new ArgumentException("Neplatný parametr záznamu v dotazu", "ID");
                }
            }
        }

        protected void FilesDetailsView_DataBound(object sender, EventArgs e)
        {
            if (FilesDetailsView.SelectedValue != null)
            {
                //int scanFileID = (int)FilesDetailsView.SelectedValue;
                //string path = ScanFile.GetFilePath(databaseName, fileName);
                //this.FileImage.ImageUrl = String.Format("FileStream.aspx?path={0}&width={1}", path, this.FileImage.Width.Value);
                //this.OcrTextLiteral.Text = String.Format("<b>OBSAH:</b> {0} ({1} znaků)", row.OcrText, row.OcrText.Length);
            }
        }
    }
}