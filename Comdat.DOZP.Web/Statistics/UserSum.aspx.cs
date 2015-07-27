using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Security;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Comdat.DOZP.Core;

namespace Comdat.DOZP.Web.Statistics
{
    public partial class UserSum : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                
                this.StatisticsDataSource.SelectParameters["userName"].DefaultValue = User.Identity.Name;

                if (Roles.IsUserInRole(RoleConstants.ADMINISTRATOR) || Roles.IsUserInRole(RoleConstants.SUPERVISOR))
                {
                    this.TitleLabel.Text = String.Format("{0} - {1}", Resources.DOZP.Statistics, Resources.DOZP.StatisticsOverview);
                    this.StatisticsDataSource.SelectParameters["status"].DefaultValue = Convert.ToInt32(StatusCode.NotStarted).ToString();
                    this.StatisticsDetailsView.Visible = false;
                }
                else if (Roles.IsUserInRole(RoleConstants.CATALOGUER))
                {
                    this.TitleLabel.Text = String.Format("{0} - {1}", Resources.DOZP.Statistics, Resources.DOZP.StatisticsScanSum);
                    this.StatisticsDetailsView.Fields[2].Visible = false;
                    this.StatisticsDetailsView.Fields[4].Visible = false;
                    this.StatisticsDataSource.SelectParameters["status"].DefaultValue = Convert.ToInt32(StatusCode.Scanned).ToString();
                }
                else if (Roles.IsUserInRole(RoleConstants.USER_OCR))
                {
                    this.TitleLabel.Text = String.Format("{0} - {1}", Resources.DOZP.Statistics, Resources.DOZP.StatisticsOcrSum);
                    this.StatisticsDetailsView.Fields[0].Visible = false;
                    this.StatisticsDetailsView.Fields[1].Visible = false;
                    this.StatisticsDataSource.SelectParameters["partOfBook"].DefaultValue = Convert.ToInt16(PartOfBook.TableOfContents).ToString();
                    this.StatisticsDataSource.SelectParameters["status"].DefaultValue = Convert.ToInt32(StatusCode.Complete).ToString();
                }
            }
        }
    }
}