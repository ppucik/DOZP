using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Comdat.DOZP.Web.Controls
{
    public partial class MonthYearCalendar : System.Web.UI.UserControl
    {
        #region Public events
        public delegate void SelectedChangedEventHandler(object sender, EventArgs e);
        public event SelectedChangedEventHandler SelectedChanged;
        #endregion

        #region Public properties

        public bool AutoPostBack
        {
            get
            {
                return (this.MonthDropDownList.AutoPostBack && this.YearDropDownList.AutoPostBack);
            }
            set
            {
                this.MonthDropDownList.AutoPostBack = value;
                this.YearDropDownList.AutoPostBack = value;
            }
        }

        public bool TitleVisible
        {
            get
            {
                return this.TitleLabel.Visible;
            }
            set
            {
                this.TitleLabel.Visible = value;
            }
        }

        public int? SelectedYear
        {
            get
            {
                if (!String.IsNullOrEmpty(this.YearDropDownList.SelectedValue))
                    return Convert.ToInt32(this.YearDropDownList.SelectedValue);
                else
                    return null;
            }
        }

        public int? SelectedMonth
        {
            get
            {
                if (!String.IsNullOrEmpty(this.MonthDropDownList.SelectedValue))
                    return Convert.ToInt32(this.MonthDropDownList.SelectedValue);
                else
                    return null;
            }
        }

        public int? SelectedDay
        {
            get { return null; }
        }

        public string SelectedText
        {
            get
            {
                string text = String.Empty;

                if (!String.IsNullOrEmpty(this.YearDropDownList.SelectedValue))
                {
                    text = this.YearDropDownList.SelectedItem.Text;

                    if (!String.IsNullOrEmpty(this.MonthDropDownList.SelectedValue))
                        text = String.Format("{0} {1}", this.MonthDropDownList.SelectedItem.Text, text);
                }

                return text;
            }
        }

        public string Width
        {
            get
            {
                return (this.MonthDropDownList.Width.Value + this.YearDropDownList.Width.Value).ToString();
            }
            set
            {
                double halfwidth = new Unit(value).Value / 2;
                this.MonthDropDownList.Width = new Unit(halfwidth);
                this.YearDropDownList.Width = new Unit(halfwidth);
            }
        }

        #endregion

        #region Public methods

        //public void LoadMonthYear(string catalogueID)
        //{
        //    BookStatistics statistics = new BookStatistics();
        //    BookSumList yearSumList = statistics.GetDataByTime(catalogueID, null, null, null, null, null, null, null);

        //    this.YearDropDownList.Items.Clear();

        //    foreach (BookSumItem year in yearSumList)
        //    {
        //        ListItem item = new ListItem(year.Year.ToString(), year.Key);
        //        if (year.Year == DateTime.Today.Year)
        //        {
        //            item.Selected = true;
        //        }
        //        this.YearDropDownList.Items.Add(item); 
        //    }
        //}

        #endregion

        #region UserControl events

        protected void Page_Init(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                this.MonthDropDownList.Items.Add(new ListItem("(Všechny)", String.Empty));
                this.MonthDropDownList.Items.Add(new ListItem("leden", "1"));
                this.MonthDropDownList.Items.Add(new ListItem("únor", "2"));
                this.MonthDropDownList.Items.Add(new ListItem("březen", "3"));
                this.MonthDropDownList.Items.Add(new ListItem("duben", "4"));
                this.MonthDropDownList.Items.Add(new ListItem("květen", "5"));
                this.MonthDropDownList.Items.Add(new ListItem("červen", "6"));
                this.MonthDropDownList.Items.Add(new ListItem("červenec", "7"));
                this.MonthDropDownList.Items.Add(new ListItem("srpen", "8"));
                this.MonthDropDownList.Items.Add(new ListItem("září", "9"));
                this.MonthDropDownList.Items.Add(new ListItem("říjen", "10"));
                this.MonthDropDownList.Items.Add(new ListItem("listopad", "11"));
                this.MonthDropDownList.Items.Add(new ListItem("prosinec", "12"));
                this.MonthDropDownList.SelectedValue = DateTime.Today.Month.ToString();

                for (int year = 2014; year <= DateTime.Today.Year; year++)
                {
                    this.YearDropDownList.Items.Add(new ListItem(year.ToString(), year.ToString()));
                }

                this.YearDropDownList.SelectedValue = DateTime.Today.Year.ToString();
            }
        }

        protected void MonthDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            OnSelectedChanged();
        }

        protected void YearDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            OnSelectedChanged();
        }

        protected void OnSelectedChanged()
        {
            if (this.SelectedChanged != null)
                this.SelectedChanged(this, new EventArgs());
        }

        #endregion
    }
}