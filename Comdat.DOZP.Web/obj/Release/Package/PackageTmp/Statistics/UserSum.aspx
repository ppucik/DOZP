<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="UserSum.aspx.cs" Inherits="Comdat.DOZP.Web.Statistics.UserSum" %>
<%@ Register TagPrefix="uc" TagName="CataloguesDropDownList" Src="~/Controls/CataloguesDropDownList.ascx" %>
<%@ Register TagPrefix="uc" TagName="MonthYearCalendar" Src="~/Controls/MonthYearCalendar.ascx" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1><asp:Label ID="TitleLabel" runat="server" Text="<%$ Resources:DOZP,Statistics %>"></asp:Label></h1>
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
            <asp:DetailsView ID="StatisticsDetailsView" runat="server"
                DataSourceID="StatisticsDataSource"
                SkinID="DetailsView"
                Width="300px">
                <Fields>
                    <asp:BoundField DataField="FrontCoverScanned" HeaderText="Obálky:" ItemStyle-HorizontalAlign="Center" />
                    <asp:BoundField DataField="TableOfContentsScanned" HeaderText="Obsahy:" ItemStyle-HorizontalAlign="Center" />
                    <asp:BoundField DataField="TableOfContentsComplete" HeaderText="Obsahy:" ItemStyle-HorizontalAlign="Center" />
                    <asp:BoundField DataField="Pages" HeaderText="Stránky:" ItemStyle-HorizontalAlign="Center" />
                    <asp:BoundField DataField="OcrTime" HeaderText="Čas zpracování:" DataFormatString="{0:%d} dni {0:hh\:mm\:ss}" ItemStyle-HorizontalAlign="Center" />
                </Fields>
            </asp:DetailsView>
        </ContentTemplate>
    </asp:UpdatePanel>
    <asp:ObjectDataSource ID="StatisticsDataSource" runat="server" 
        SelectMethod="GetDataByUser" 
        TypeName="Comdat.DOZP.Data.Business.StatisticsComponent">
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
