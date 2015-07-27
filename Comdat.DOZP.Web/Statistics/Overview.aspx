<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Overview.aspx.cs" Inherits="Comdat.DOZP.Web.Statistics.Overview" %>
<%@ Register TagPrefix="uc" TagName="CataloguesDropDownList" Src="~/Controls/CataloguesDropDownList.ascx" %>
<%@ Register TagPrefix="uc" TagName="MonthYearCalendar" Src="~/Controls/MonthYearCalendar.ascx" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1><asp:Label ID="TitleLabel" runat="server" Text="<%$ Resources:DOZP,StatisticsOverview %>"></asp:Label></h1>
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
                FooterStyle-HorizontalAlign="Center" 
                OnRowDataBound="StatistiscGridView_RowDataBound">   
                <Columns>
                    <asp:BoundField DataField="Caption" HeaderText="Časové období" FooterText="Celkem" FooterStyle-HorizontalAlign="Left" ItemStyle-Width="125px" />
                    <asp:BoundField DataField="FrontCoverScanned" HeaderText="Naskenované obálky" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" />
                    <asp:BoundField DataField="TableOfContentsScanned" HeaderText="Naskenované obsahy" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" />
                    <asp:BoundField DataField="FrontCoverComplete" HeaderText="Importované ObalkyKnih.cz" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" />
                    <asp:BoundField DataField="TableOfContentsComplete" HeaderText="OCR zpracované obsahy" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" />
                    <asp:BoundField DataField="FrontCoverExported" HeaderText="Exportováné obálky" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" />
                    <asp:BoundField DataField="TableOfContentsExported" HeaderText="Exportováné obsahy" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" />
                </Columns>
            </asp:GridView>
        </ContentTemplate>
    </asp:UpdatePanel>
    <asp:ObjectDataSource ID="StatisticsDataSource" runat="server" 
        SelectMethod="GetDataByTime" 
        TypeName="Comdat.DOZP.Data.Business.StatisticsComponent" 
        OnSelected="BooksDataSource_Selected">
        <SelectParameters>
            <asp:ControlParameter ControlID="CataloguesDropDownList" Name="catalogueID" PropertyName="SelectedValue" Type="Int32" />
            <asp:ControlParameter ControlID="MonthYearCalendar" Name="modifiedYear" PropertyName="SelectedYear" Type="Int32" />
            <asp:ControlParameter ControlID="MonthYearCalendar" Name="modifiedMonth" PropertyName="SelectedMonth" Type="Int32" />
            <asp:Parameter Name="modifiedDay" Type="Int32" ConvertEmptyStringToNull="true" DefaultValue="" />
        </SelectParameters>
    </asp:ObjectDataSource>
</asp:Content>
