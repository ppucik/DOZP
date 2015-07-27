<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Catalogue.aspx.cs" Inherits="Comdat.DOZP.Web.Admin.Catalogue" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1><asp:Label ID="TitleLabel" runat="server" Text="Administrace katalogu"></asp:Label></h1>
    <br />
    <table class="formEditTable">
        <tr>
            <th class="required">Databáze:</th>
            <td><asp:TextBox ID="DatabaseNameTextBox" runat="server" MaxLength="20" Font-Bold="true" Width="200px"></asp:TextBox></td>
            <td><asp:RequiredFieldValidator ID="DatabaseNameRequiredFieldValidator" runat="server" ControlToValidate="DatabaseNameTextBox" ErrorMessage="*" CssClass="failureNotification" Display="Dynamic"></asp:RequiredFieldValidator></td>
        </tr>
        <tr>
            <th>Název:</th>
            <td><asp:TextBox ID="NameTextBox" runat="server" MaxLength="250" Width="350px"></asp:TextBox></td>
            <td><asp:RequiredFieldValidator ID="NameRequiredFieldValidator" runat="server" ControlToValidate="NameTextBox" ErrorMessage="*" CssClass="failureNotification" Display="Dynamic"></asp:RequiredFieldValidator></td>
        </tr>
        <tr>
            <th>Popis:</th>
            <td><asp:TextBox ID="DescriptionTextBox" runat="server" TextMode="MultiLine" Rows="4" Width="350px"></asp:TextBox></td>
        </tr>
        <tr>
            <th>URL:</th>
            <td><asp:TextBox ID="ZServerUrlTextBox" runat="server" MaxLength="100" Width="350px"></asp:TextBox></td>
            <td><asp:RequiredFieldValidator ID="ZServerUrlRequiredFieldValidator" runat="server" ControlToValidate="ZServerUrlTextBox" ErrorMessage="*" CssClass="failureNotification" Display="Dynamic"></asp:RequiredFieldValidator></td>
        </tr>
        <tr>
            <th>Port:</th>
            <td><asp:TextBox ID="ZServerPortTextBox" runat="server" Width="100px"></asp:TextBox></td>
            <td><asp:RequiredFieldValidator ID="ZServerPortRequiredFieldValidator" runat="server" ControlToValidate="ZServerPortTextBox" ErrorMessage="*" CssClass="failureNotification" Display="Dynamic"></asp:RequiredFieldValidator></td>
        </tr>
        <tr>
            <th>Znaková sada:</th>
            <td><asp:TextBox ID="CharsetTextBox" runat="server" MaxLength="20" Width="100px"></asp:TextBox></td>
        </tr>
        <tr>
            <th>Aktivní:</th>
            <td><asp:CheckBox ID="EnabledCheckBox" runat="server" /></td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <th>&nbsp;</th>
            <td colspan="2">
                <asp:Button ID="SaveButton" runat="server" Text="Uložit" SkinID="Button" OnClick="SaveButton_Click" />
                <asp:Button ID="DeleteButton" runat="server" Text="Odstranit" Visible="false" SkinID="Button" OnClick="DeleteButton_Click" OnClientClick="return confirm('Are you sure you want to submit ?')" />
                <asp:Button ID="CancelButton" runat="server" Text="Storno" CausesValidation="false" SkinID="Button" OnClick="StornoButton_Click" />
            </td>
        </tr>        
    </table>
    <asp:Label ID="ErrorLabel" runat="server" CssClass="failureNotification"></asp:Label>
</asp:Content>
