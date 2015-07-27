<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="EventLogs.aspx.cs" Inherits="Comdat.DOZP.Web.Admin.EventLogs" %>
<%@ Register TagPrefix="uc" TagName="CataloguesDropDownList" Src="~/Controls/CataloguesDropDownList.ascx" %>
<%@ Register TagPrefix="uc" TagName="MonthYearCalendar" Src="~/Controls/MonthYearCalendar.ascx" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="sidebar">
        <uc:CataloguesDropDownList ID="CataloguesDropDownList" runat="server" HeaderVisible="true" />
        <uc:MonthYearCalendar ID="MonthYearCalendar" runat="server" HeaderVisible="true" />
    </div>
    <div class="content">
        <h2><asp:Label ID="TitleLabel" runat="server" Text="<%$ Resources:DOZP,EventLogs %>"></asp:Label></h2>
        <p>Administrace všech událostí týkajících se importu, exportu a práce se záznamami, nebo reportované chybové hlášení.</p>

    </div>
</asp:Content>
