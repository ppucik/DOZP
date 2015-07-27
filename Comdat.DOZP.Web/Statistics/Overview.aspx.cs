using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Comdat.DOZP.Core;

namespace Comdat.DOZP.Web.Statistics
{
    public partial class Overview : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                this.TitleLabel.Text = String.Format("{0} - {1}", Resources.DOZP.Statistics, Resources.DOZP.StatisticsOverview);
            }
        }

        protected void BooksDataSource_Selected(object sender, ObjectDataSourceStatusEventArgs e)
        {
            if (e.ReturnValue != null)
            {
                FileSumList bookSumList = (e.ReturnValue as FileSumList);

                if (bookSumList != null)
                {
                    FileSumItem summary = bookSumList.GetTotalSum();
                    this.StatisticsGridView.Columns[0].FooterText = summary.Caption;
                    this.StatisticsGridView.Columns[1].FooterText = summary.FrontCoverScanned.ToString();
                    this.StatisticsGridView.Columns[2].FooterText = summary.TableOfContentsScanned.ToString();
                    this.StatisticsGridView.Columns[3].FooterText = summary.FrontCoverComplete.ToString();
                    this.StatisticsGridView.Columns[4].FooterText = summary.TableOfContentsComplete.ToString();
                    this.StatisticsGridView.Columns[5].FooterText = summary.FrontCoverExported.ToString();
                    this.StatisticsGridView.Columns[6].FooterText = summary.TableOfContentsExported.ToString();
                }
            }
        }

        protected void StatistiscGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                FileSumItem statistics = (e.Row.DataItem as FileSumItem);

                if (statistics != null && statistics.Year > 0 && statistics.Month > 0)
                {
                    if (statistics.Day == 0)
                    {
                        if (statistics.Month == DateTime.Now.Month)
                            e.Row.Cells[0].Font.Bold = true;
                    }
                    else
                    {
                        DateTime dt = new DateTime(statistics.Year, statistics.Month, statistics.Day);
                        if (dt == DateTime.Today)
                            e.Row.Cells[0].Font.Bold = true;
                        if (dt.DayOfWeek == DayOfWeek.Sunday)
                            e.Row.Cells[0].ForeColor = Color.Red;
                    }
                }
            }
        }
    }
}