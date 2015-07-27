<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ScanSum.aspx.cs" Inherits="Comdat.DOZP.Web.Statistics.ScanSum" %>
<%@ Register TagPrefix="uc" TagName="CataloguesDropDownList" Src="~/Controls/CataloguesDropDownList.ascx" %>
<%@ Register TagPrefix="uc" TagName="MonthYearCalendar" Src="~/Controls/MonthYearCalendar.ascx" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1><asp:Label ID="TitleLabel" runat="server" Text="<%$ Resources:DOZP,StatisticsScanSum %>"></asp:Label></h1>
    <table>
        <tr>
            <td><uc:CataloguesDropDownList ID="CataloguesDropDownList" runat="server" AutoPostBack="true" TitleVisible="true" Width="200px" /></td>
            <td><uc:MonthYearCalendar ID="MonthYearCalendar" runat="server" AutoPostBack="true" TitleVisible="true" /></td>
        </tr>
    </table>
    <asp:UpdatePanel ID="StatisticsUpdatePanel" runat="server" UpdateMode="Conditional">
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="CataloguesDropDownList" />
            <asp:AsyncPostBackTrigger ControlID="MonthYearCalendar" />
        </Triggers>
        <ContentTemplate>
            <asp:GridView ID="StatisticsGridView" runat="server" 
                DataSourceID="StatisticsDataSource"
                ShowFooter="True" 
                SkinID="GridView"
                FooterStyle-HorizontalAlign="Center">   
                <Columns>
                    <asp:BoundField DataField="Caption" HeaderText="Uživatel" FooterText="Celkem" FooterStyle-HorizontalAlign="Left" />
                    <asp:BoundField DataField="Comment" HeaderText="Knihovna" FooterStyle-HorizontalAlign="Left" />
                    <asp:BoundField DataField="FrontCoverScanned" HeaderText="Obálky" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" />
                    <asp:BoundField DataField="TableOfContentsScanned" HeaderText="Obsahy" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" />
                    <asp:BoundField DataField="Pages" HeaderText="Stránky" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" />
                </Columns>
            </asp:GridView>
        </ContentTemplate>
    </asp:UpdatePanel>
    <asp:ObjectDataSource ID="StatisticsDataSource" runat="server" 
        SelectMethod="GetDataByUser" 
        TypeName="Comdat.DOZP.Data.Business.StatisticsComponent" 
        OnSelected="BooksDataSource_Selected">
        <SelectParameters>
            <asp:ControlParameter ControlID="CataloguesDropDownList" Name="catalogueID" PropertyName="SelectedValue" Type="Int32" />
            <asp:ControlParameter ControlID="MonthYearCalendar" Name="modifiedYear" PropertyName="SelectedYear" Type="Int32" />
            <asp:ControlParameter ControlID="MonthYearCalendar" Name="modifiedMonth" PropertyName="SelectedMonth" Type="Int32" />
            <asp:Parameter Name="modifiedDay" Type="Int32" ConvertEmptyStringToNull="true" DefaultValue="" />
            <asp:Parameter Name="partOfBook" Type="Int16" ConvertEmptyStringToNull="true" DefaultValue="" />
            <asp:Parameter Name="useOCR" Type="Boolean" ConvertEmptyStringToNull="true" DefaultValue="" />
            <asp:Parameter Name="userName" Type="String" ConvertEmptyStringToNull="true" DefaultValue="" />
            <asp:Parameter Name="status" Type="Int32" ConvertEmptyStringToNull="true" />
        </SelectParameters>
    </asp:ObjectDataSource>
</asp:Content>
