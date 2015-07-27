using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Comdat.DOZP.Web.Controls
{
    public partial class PagerDropDownList : System.Web.UI.UserControl
    {
        public delegate void SelectedChangedEventHandler(object sender, EventArgs e);
        public event SelectedChangedEventHandler SelectedChanged;

        #region Public properties

        public bool AutoPostback
        {
            get
            {
                return this.DropDownList.AutoPostBack;
            }
            set
            {
                this.DropDownList.AutoPostBack = value;
            }
        }

        public int SelectedValue
        {
            get
            {
                return (String.IsNullOrEmpty(this.DropDownList.SelectedValue) ? 10 : Int32.Parse(this.DropDownList.SelectedValue));
            }
            set
            {
                this.DropDownList.SelectedValue = value.ToString();
            }
        }

        //public string SummaryInfo
        //{
        //    get
        //    {

        //    }
        //    set
        //    {
        //        this.SummaryLabel.Text = String.Format("Počet záznamů: {0}", value);
        //    }
        //}

        #endregion

        #region UserControl events

        protected void Page_Init(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                this.DropDownList.Items.Add(new ListItem("10"));
                this.DropDownList.Items.Add(new ListItem("20"));
                this.DropDownList.Items.Add(new ListItem("50"));
                this.DropDownList.Items.Add(new ListItem("100"));
                this.DropDownList.Items.Add(new ListItem("vše", "0"));

                if (Session["PagerDropDownListValue"] != null)
                {
                    this.SelectedValue = Int32.Parse(Session["PagerDropDownListValue"].ToString());
                }

                //OnSelectedChanged();
            }
        }

        protected void DropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            OnSelectedChanged();
        }

        protected void OnSelectedChanged()
        {
            Session["PagerDropDownListValue"] = this.SelectedValue;

            if (this.SelectedChanged != null)
                this.SelectedChanged(this, new EventArgs());
        }

        #endregion
    }
}