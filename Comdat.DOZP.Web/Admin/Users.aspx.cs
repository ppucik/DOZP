using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

using Comdat.DOZP.Core;

namespace Comdat.DOZP.Web.Admin
{
    public partial class Users : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                int institutionID = Convert.ToInt32(Request.QueryString["institutionID"]);
                this.InstitutionsDropDownList.SelectedValue = institutionID;
            }
        }

        protected void UsersDataSource_Selecting(object sender, ObjectDataSourceSelectingEventArgs e)
        {
            UsersGridViewPageSizeSetup();
        }

        protected void UsersDataSource_Selected(object sender, ObjectDataSourceStatusEventArgs e)
        {
            int count = 0;

            if (e.ReturnValue != null)
            {
                count = (e.ReturnValue as List<User>).Count;
                this.SummaryLabel.Text = String.Format("Počet uživatelů: {0}", count);
            }

            //this.PagerDropDownList.Visible = (count > 0);
        }

        protected void PagerDropDownList_SelectedChanged(object sender, EventArgs e)
        {
            UsersGridViewPagingSetup();
        }

        #region Private methods

        private void UsersGridViewPagingSetup()
        {
            this.UsersGridView.PageIndex = 0;
            this.UsersGridViewPageSizeSetup();
        }

        private void UsersGridViewPageSizeSetup()
        {
            if (this.PagerDropDownList.SelectedValue == 0)
            {
                this.UsersGridView.AllowPaging = false;
            }
            else
            {
                this.UsersGridView.AllowPaging = true;
                this.UsersGridView.PageSize = this.PagerDropDownList.SelectedValue;
            }
        }

        #endregion
    }
}