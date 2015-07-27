<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="UserEdit.aspx.cs" Inherits="Comdat.DOZP.Web.Admin.UserEdit" %>
<%@ Register TagPrefix="uc" TagName="InstitutionsDropDownList" Src="~/Controls/InstitutionsDropDownList.ascx" %>
<%@ Register TagPrefix="uc" TagName="RolesDropDownList" Src="~/Controls/RolesDropDownList.ascx" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1><asp:Label ID="TitleLabel" runat="server" Text="Administrace uživatele"></asp:Label></h1>
    <br />
    <table class="formEditTable">
        <tr>
            <th class="required">Uživatel:</th>
            <td><asp:TextBox ID="UserNameTextBox" runat="server" MaxLength="100" Width="200px"></asp:TextBox></td>
            <td><asp:RequiredFieldValidator ID="UserNameRequiredFieldValidator" runat="server" ControlToValidate="UserNameTextBox" ErrorMessage="*" CssClass="failureNotification" Display="Dynamic"></asp:RequiredFieldValidator></td>
        </tr>
        <tr>
            <th>Celé jméno:</th>
            <td><asp:TextBox ID="FullNameTextBox" runat="server" MaxLength="200" Width="100%"></asp:TextBox></td>
            <td><asp:RequiredFieldValidator ID="FullNameRequiredFieldValidator" runat="server" ControlToValidate="FullNameTextBox" ErrorMessage="*" CssClass="failureNotification" Display="Dynamic"></asp:RequiredFieldValidator></td>
        </tr>
        <tr>
            <th>Instituce:</th>
            <td><uc:InstitutionsDropDownList ID="InstitutionsDropDownList" runat="server" /></td>
            <td><asp:RequiredFieldValidator ID="InstitutionsFieldValidator" runat="server" ControlToValidate="InstitutionsDropDownList" ErrorMessage="*" CssClass="failureNotification" Display="Dynamic"></asp:RequiredFieldValidator></td>
        </tr>
        <tr>
            <th>Role:</th>
            <td><uc:RolesDropDownList ID="RolesDropDownList" runat="server" /></td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <th>Email:</th>
            <td><asp:TextBox ID="EmailTextBox" runat="server" MaxLength="100" Width="350px"></asp:TextBox></td>
            <td>
                <asp:RequiredFieldValidator ID="EmaiRequiredFieldValidator" runat="server" ControlToValidate="EmailTextBox" ErrorMessage="*" CssClass="failureNotification" Display="Dynamic"></asp:RequiredFieldValidator>
                <asp:RegularExpressionValidator ID="EmaiRegularExpressionValidator" runat="server" ControlToValidate="EmailTextBox" ErrorMessage="Email adresa je chybná" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" CssClass="failureNotification" Display="Dynamic"></asp:RegularExpressionValidator>
            </td>
        </tr>
        <tr>
            <th>Telefon:</th>
            <td><asp:TextBox ID="TelephoneTextBox" runat="server" MaxLength="100" Width="350px"></asp:TextBox></td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <th>Poznámka:</th>
            <td><asp:TextBox ID="CommentTextBox" runat="server" Width="350px"></asp:TextBox></td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <th>Aktivní:</th>
            <td><asp:CheckBox ID="IsApprovedCheckBox" runat="server" /></td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <th>&nbsp;</th>
            <td colspan="2">
                <asp:Button ID="SaveButton" runat="server" Text="Uložit" SkinID="Button" OnClick="SaveButton_Click" />
                <asp:Button ID="DeleteButton" runat="server" Text="Odstranit" SkinID="Button" OnClick="DeleteButton_Click" OnClientClick="return confirm('Are you sure you want to submit ?')" />
                <asp:Button ID="ChangePasswordButton" runat="server" Text="Změnit heslo" Height="24px" OnClick="ChangePasswordButton_Click" />
                <asp:Button ID="CancelButton" runat="server" Text="Storno" CausesValidation="false" SkinID="Button" OnClick="StornoButton_Click" />
            </td>
        </tr>
    </table>
    <asp:Label ID="ErrorLabel" runat="server" CssClass="failureNotification"></asp:Label>
</asp:Content>
