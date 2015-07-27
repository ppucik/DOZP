<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ChangePassword.aspx.cs" Inherits="Comdat.DOZP.Web.Admin.ChangePassword" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1><asp:Label ID="TitleLabel" runat="server" Text="Změnit heslo uživatele"></asp:Label></h1>
    <br />
    <table class="formEditTable">
        <tr>
            <th>Uživatel:</th>
            <td>
                <asp:TextBox ID="UserNameTextBox" runat="server" Enabled="false" MaxLength="100" Width="200px"></asp:TextBox>
            </td>
            <td>
                <asp:RequiredFieldValidator ID="UserNameRequiredFieldValidator" runat="server" ControlToValidate="UserNameTextBox" ErrorMessage="*" CssClass="failureNotification" Display="Dynamic"></asp:RequiredFieldValidator>
            </td>
        </tr>
        <tr>
            <th>Nové heslo:</th>
            <td>
                <asp:TextBox ID="NewPasswordTextBox" runat="server" TextMode="Password" MaxLength="100" Width="200px"></asp:TextBox>
            </td>
            <td>
                <asp:RequiredFieldValidator ID="NewPasswordRequiredFieldValidator" runat="server" ControlToValidate="NewPasswordTextBox" ErrorMessage="*" CssClass="failureNotification" Display="Dynamic"></asp:RequiredFieldValidator>
                <asp:RegularExpressionValidator ID="NewPasswordRegularExpressionValidator" runat="server" ControlToValidate="NewPasswordTextBox" ErrorMessage="Minimální délka je 6 znaků" ValidationExpression=".{6}.*" CssClass="failureNotification"></asp:RegularExpressionValidator>
            </td>
        </tr>
        <tr>
            <th>Potvrzení hesla:</th>
            <td>
                <asp:TextBox ID="ConfirmPasswordTextBox" runat="server" TextMode="Password" MaxLength="100" Width="200px"></asp:TextBox>
            </td>
            <td>
                <asp:RequiredFieldValidator ID="ConfirmPasswordRequiredFieldValidator" runat="server" ControlToValidate="ConfirmPasswordTextBox" ErrorMessage="*" CssClass="failureNotification" Display="Dynamic"></asp:RequiredFieldValidator>
                <asp:CompareValidator ID="ConfirmPasswordCompareValidator" runat="server" ControlToCompare="NewPasswordTextBox" ControlToValidate="ConfirmPasswordTextBox" ErrorMessage="Heslo a potvrzení hesla se musí shodovat." CssClass="failureNotification"></asp:CompareValidator>
            </td>
        </tr>
        <tr>
            <th>&nbsp;</th>
            <td colspan="2">
                <asp:Button ID="SaveButton" runat="server" Text="Uložit" SkinID="Button" OnClick="SaveButton_Click" />
                <asp:Button ID="CancelButton" runat="server" Text="Storno" CausesValidation="false" SkinID="Button" OnClick="StornoButton_Click" />
            </td>
        </tr>        
    </table>
    <asp:Label ID="ErrorLabel" runat="server" CssClass="failureNotification"></asp:Label>
</asp:Content>
