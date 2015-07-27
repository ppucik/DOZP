using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Comdat.DOZP.Core;

namespace Comdat.DOZP.Web.Controls
{
    public partial class MenuCalendar : System.Web.UI.UserControl
    {
        public delegate void SelectedChangedEventHandler(object sender, EventArgs e);
        public event SelectedChangedEventHandler SelectedChanged;

        #region Public properties

        public DateTime SelectedDate
        {
            get
            {
                return this.Calendar.SelectedDate;
            }
            set
            {
                this.Calendar.SelectedDate = value;
            }
        }

        public SelectedDatesCollection SelectedDates
        {
            get
            {
                return this.Calendar.SelectedDates;
            }
        }

        public CalendarSelectionMode SelectionMode
        {
            get
            {
                return this.Calendar.SelectionMode;
            }
            set
            {
                this.Calendar.SelectionMode = value;
            }
        }

        #endregion

        #region Extend properties

        public DateTime SelectedDateFrom
        {
            get
            {
                return this.Calendar.SelectedDates[0];
            }
        }

        public DateTime SelectedDateTo
        {
            get
            {
                return this.Calendar.SelectedDates[this.Calendar.SelectedDates.Count - 1];
            }
        }

        public DateRange.Interval SelectedInterval
        {
            get
            {
                switch (this.Calendar.SelectedDates.Count)
                {
                    case 0:
                        return DateRange.Interval.None;
                    case 1:
                        return DateRange.Interval.Day;
                    case 7:
                        return DateRange.Interval.Week;
                    default:
                        return DateRange.Interval.Month;
                }
            }
        }

        public DateRange SelectedDateRange
        {
            get
            {
                return new DateRange(this.SelectedDateFrom, this.SelectedDateTo);
            }
        }

        #endregion

        #region UserControl events

        protected void Page_Init(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                this.Calendar.SelectedDate = DateTime.Today;
            }
        }

        protected void Calendar_SelectionChanged(object sender, EventArgs e)
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