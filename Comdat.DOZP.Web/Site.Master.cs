using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Comdat.DOZP.Core;
using Comdat.DOZP.Data.Business;
using Comdat.DOZP.Services;

namespace Comdat.DOZP.Web
{
    public partial class SiteMaster : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            this.Page.Title = Resources.DOZP.AppTitle;

            if (!IsPostBack)
            {
                if (Request.IsAuthenticated)
                {
                    //Navigační menu 
                    NavigationMenu.Visible = true;

                    CatalogueFilter catalogueFilter = new CatalogueFilter();
                    catalogueFilter.UserRole = Roles.GetRolesForUser().SingleOrDefault();

                    //Přihlášený uživatel
                    UserProfile profile = UserProfile.GetUserProfile();
                    if (profile != null)
                    {
                        if (!String.IsNullOrEmpty(profile.FullName))
                            this.LoginName.FormatString = String.Format(this.LoginName.FormatString, HttpUtility.HtmlEncode(profile.FullName));

                        if (profile.InstitutionID.HasValue)
                            catalogueFilter.InstitutionID = profile.InstitutionID.Value;
                    }

                    //Katalogy
                    IList<Catalogue> catalogues = CatalogueComponent.Instance.GetList(catalogueFilter);
                    if (catalogues != null)
                    {
                        MenuItem cataloguesMenuItem = new MenuItem(Resources.DOZP.Catalogues, "Catalogues");
                        cataloguesMenuItem.Selectable = false;

                        if (catalogues.Count == 1)
                        {
                            cataloguesMenuItem.Text = Resources.DOZP.Catalogue;
                            cataloguesMenuItem.Selectable = true;
                            cataloguesMenuItem.NavigateUrl = String.Format("~/Catalogues/FileList.aspx?id={0}", catalogues[0].CatalogueID);
                        }
                        else
                        {
                            foreach (var c in catalogues)
                            {
                                cataloguesMenuItem.ChildItems.Add(new MenuItem(c.Name, c.DatabaseName, null, String.Format("~/Catalogues/FileList.aspx?id={0}", c.CatalogueID)));
                            }
                        }

                        this.NavigationMenu.Items.Add(cataloguesMenuItem);
                    }

                    //Statistika
                    MenuItem statisticsMenuItem = new MenuItem(Resources.DOZP.Statistics, "Statistics");
                    statisticsMenuItem.Selectable = false;
                    this.NavigationMenu.Items.Add(statisticsMenuItem);

                    //Download
                    MenuItem downloadMenuItem = new MenuItem(Resources.DOZP.Download, "Download");
                    this.NavigationMenu.Items.Add(downloadMenuItem);

                    if (catalogueFilter.UserRole == RoleConstants.ADMINISTRATOR || catalogueFilter.UserRole == RoleConstants.SUPERVISOR)
                    {
                        //Statistika
                        statisticsMenuItem.ChildItems.Add(new MenuItem(Resources.DOZP.StatisticsOverview, "StatisticsOverview", null, "~/Statistics/Overview.aspx"));
                        statisticsMenuItem.ChildItems.Add(new MenuItem(Resources.DOZP.StatisticsScanSum, "StatisticsScan", null, "~/Statistics/ScanSum.aspx"));
                        statisticsMenuItem.ChildItems.Add(new MenuItem(Resources.DOZP.StatisticsOcrSum, "StatisticsOCR", null, "~/Statistics/OcrSum.aspx"));

                        //Download
                        downloadMenuItem.ChildItems.Add(new MenuItem(Resources.DOZP.DozpScanApp, "DozpScanApp", null, "~/Account/DownloadScan.aspx"));
                        downloadMenuItem.ChildItems.Add(new MenuItem(Resources.DOZP.DozpOcrApp, "DozpOcrApp", null, "~/Account/DownloadOCR.aspx"));
                        downloadMenuItem.Selectable = false;

                        //Administrace
                        MenuItem adminMenuItem = new MenuItem(Resources.DOZP.Administration, "Administration");
                        adminMenuItem.ChildItems.Add(new MenuItem(Resources.DOZP.Institution, "Institutions", null, "~/Admin/Institutions.aspx"));
                        adminMenuItem.ChildItems.Add(new MenuItem(Resources.DOZP.Catalogues, "Catalogues", null, "~/Admin/Catalogues.aspx"));
                        adminMenuItem.ChildItems.Add(new MenuItem(Resources.DOZP.Users, "Users", null, "~/Admin/Users.aspx"));
                        adminMenuItem.Selectable = false;
                        this.NavigationMenu.Items.Add(adminMenuItem);
                    }
                    else if (catalogueFilter.UserRole == RoleConstants.CATALOGUER)
                    {
                        downloadMenuItem.NavigateUrl = "~/Account/DownloadScan.aspx";
                        downloadMenuItem.Selectable = true;

                        statisticsMenuItem.NavigateUrl = "~/Statistics/UserSum.aspx";
                        statisticsMenuItem.Selectable = true;
                    }
                    else if (catalogueFilter.UserRole == RoleConstants.USER_OCR)
                    {
                        downloadMenuItem.NavigateUrl = "~/Account/DownloadOCR.aspx";
                        downloadMenuItem.Selectable = true;

                        statisticsMenuItem.NavigateUrl = "~/Statistics/UserSum.aspx";
                        statisticsMenuItem.Selectable = true;
                    }
                    else
                    {
                        throw new System.Security.SecurityException("Přihlášený uživatel nemá nastavenou žádnou roli");
                    }
                }
                else
                {
                    MenuPanel.Height = 0;
                    NavigationMenu.Visible = false;
                    SearchTextBox.Visible = false;
                    SearchImageButton.Visible = false;
                }
            }

            this.LastUpdate.Text = String.Format("{0}: {1}", Resources.DOZP.LastUpdate, ConfigurationManager.AppSettings["LastUpdate"]);
        }

        //private void AddRssLink(string catalogueID, string catalogName)
        //{
        //    HtmlHead header = (Page.Header as HtmlHead);
            
        //    if (header != null)
        //    {
        //        //<link rel="Alternate" href="~/RssFeedHandler.ashx?catalogue=CKS" title="DOZP RSS feed" type="application/rss+xml" />
        //        HtmlLink link = new HtmlLink();
        //        link.Attributes.Add("rel", "Alternate");
        //        link.Attributes.Add("href", String.Format("~/RssHandler.ashx?base={0}", catalogueID));
        //        link.Attributes.Add("title", String.Format("RSS - {0}", catalogName));
        //        link.Attributes.Add("type", "application/rss+xml");
        //        header.Controls.Add(link);
        //    }
        //}

        protected void SearchImageButton_Click(object sender, ImageClickEventArgs e)
        {
            string text = this.SearchTextBox.Text.Trim();
            string url = (!String.IsNullOrEmpty(text) ? String.Format("~/Catalogues/BookFind.aspx?text={0}", text) : "~/Default.aspx");

            Response.Redirect(url);
        }
    }
}
