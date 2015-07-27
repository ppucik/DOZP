using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Comdat.DOZP.Core;
using Comdat.DOZP.Data.Business;

namespace Comdat.DOZP.Web.Catalogues
{
    public partial class BookFind : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                string sysno = Request.QueryString["text"];
                this.TitleLabel.Text = String.Format("{0} - {1}", Resources.DOZP.BookFind, sysno);
            }
        }

        #region Private methods

        protected string GetPublication(object b)
        {
            Book book = (b as Book);

            if (book != null)
            {
                StringBuilder sb = new StringBuilder();

                if (!String.IsNullOrEmpty(book.Author)) sb.AppendFormat("{0}: ", book.Author);
                if (!String.IsNullOrEmpty(book.Title)) sb.AppendFormat("<i>{0}</i>", book.Title);
                if (!String.IsNullOrEmpty(book.Year)) sb.AppendFormat(", {0}", book.Year);
                if (!String.IsNullOrEmpty(book.ISBN)) sb.AppendFormat(", <nobr>ISBN {0}</nobr>", book.ISBN);
                if (!String.IsNullOrEmpty(book.Volume)) sb.AppendFormat(" ({0})", book.Volume);

                return sb.ToString();
            }
            else
            {
                return String.Empty;
            }
        }

        #endregion
    }
}