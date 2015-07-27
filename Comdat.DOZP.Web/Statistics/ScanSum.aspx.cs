using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Comdat.DOZP.Core;

namespace Comdat.DOZP.Web.Statistics
{
    public partial class ScanSum : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                this.TitleLabel.Text = String.Format("{0} - {1}", Resources.DOZP.Statistics, Resources.DOZP.StatisticsScanSum);
                this.StatisticsDataSource.SelectParameters["status"].DefaultValue = Convert.ToInt32(StatusCode.Scanned).ToString();
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
                    this.StatisticsGridView.Columns[2].FooterText = summary.FrontCoverScanned.ToString();
                    this.StatisticsGridView.Columns[3].FooterText = summary.TableOfContentsScanned.ToString();
                    this.StatisticsGridView.Columns[4].FooterText = summary.Pages.ToString();
                }
            }
        }
    }
}