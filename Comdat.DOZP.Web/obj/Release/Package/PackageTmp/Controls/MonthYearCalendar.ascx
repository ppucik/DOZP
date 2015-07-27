<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MonthYearCalendar.ascx.cs" Inherits="Comdat.DOZP.Web.Controls.MonthYearCalendar" %>
<asp:Panel ID="MonthYearPanel" runat="server" CssClass="DropDownListControl">
    <asp:Label ID="TitleLabel" runat="server" Text="Časové&nbsp;období:&nbsp;" Visible="false" />
    <asp:DropDownList ID="MonthDropDownList" runat="server" AutoPostBack="true" CssClass="DropDownListItems" EnableViewState="true" Width="120px" OnSelectedIndexChanged="MonthDropDownList_SelectedIndexChanged" />
    <asp:DropDownList ID="YearDropDownList"  runat="server" AutoPostBack="true" CssClass="DropDownListItems" EnableViewState="true" Width="80px" OnSelectedIndexChanged="MonthDropDownList_SelectedIndexChanged" />
</asp:Panel>