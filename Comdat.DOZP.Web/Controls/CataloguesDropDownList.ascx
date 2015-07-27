<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CataloguesDropDownList.ascx.cs" Inherits="Comdat.DOZP.Web.Controls.CataloguesDropDownList" %>
<asp:Panel ID="DropDownListPanel" runat="server">
    <asp:Label ID="TitleLabel" runat="server" Text="Katalog:&nbsp;" Visible="false" />
    <asp:DropDownList ID="DropDownList" runat="server" AutoPostBack="false" CssClass="DropDownListItems" OnSelectedIndexChanged="DropDownList_SelectedIndexChanged" />
</asp:Panel>