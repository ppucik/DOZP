<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="InstitutionsDropDownList.ascx.cs" Inherits="Comdat.DOZP.Web.Controls.InstitutionsDropDownList" %>
<asp:Panel ID="DropDownListPanel" runat="server">
    <asp:Label ID="TitleLabel" runat="server" Text="Instituce:&nbsp;" Visible="false" />
    <asp:DropDownList ID="DropDownList" runat="server" AutoPostBack="false" CssClass="DropDownListItems" OnSelectedIndexChanged="DropDownList_SelectedIndexChanged" />
</asp:Panel>