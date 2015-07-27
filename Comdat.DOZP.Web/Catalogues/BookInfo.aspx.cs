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
    public partial class BookInfo : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                int bookID = 0;

                if (Int32.TryParse(Request.QueryString["id"], out bookID))
                {
                    Book book = BookComponent.Instance.GetByID(bookID);

                    if (book != null)
                    {
                        this.TitleLabel.Text = String.Format("Publikace č. {0}", book.SysNo);
                        this.PublicationLabel.Text = String.Format("{0}: {1}, {2}, {3}", book.Author, book.Title, book.Year, book.ISBN);
                    }
                    else
                    {
                        throw new ArgumentException(String.Format("Publikace ID={0} nebyla nalezena", bookID));
                    }
                }
                else
                {
                    throw new ArgumentException("Neplatný parametr publikace v dotazu", "ID");
                }
            }
        }
    }
}