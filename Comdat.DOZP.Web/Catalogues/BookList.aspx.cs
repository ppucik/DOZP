using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

using Comdat.DOZP.Core;
using Comdat.DOZP.Data.Business;

namespace Comdat.DOZP.Web.Catalogues
{
    public partial class BookList : System.Web.UI.Page
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

        protected void BooksDataSource_Selecting(object sender, ObjectDataSourceSelectingEventArgs e)
        {
            BooksGridViewPageSizeSetup();
        }

        protected void BooksDataSource_Selected(object sender, ObjectDataSourceStatusEventArgs e)
        {
            int count = 0;

            if (e.ReturnValue != null)
            {
                count = (e.ReturnValue as List<Book>).Count;
                this.SummaryLabel.Text = String.Format("Počet záznamů: {0}", count);
            }

            //this.PagerDropDownList.Visible = (count > 0);
        }

        protected void PagerDropDownList_SelectedChanged(object sender, EventArgs e)
        {
            BooksGridViewPagingSetup();
        }

        protected void MenuCalendar_SelectedChanged(object sender, EventArgs e)
        {
            //this.BookUpdatePanel.Update();
        }

        #region Private methods

        private void BooksGridViewPagingSetup()
        {
            this.BooksGridView.PageIndex = 0;
            this.BooksGridViewPageSizeSetup();
        }

        private void BooksGridViewPageSizeSetup()
        {
            if (this.PagerDropDownList.SelectedValue == 0)
            {
                this.BooksGridView.AllowPaging = false;
            }
            else
            {
                this.BooksGridView.AllowPaging = true;
                this.BooksGridView.PageSize = this.PagerDropDownList.SelectedValue;
            }
        }

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