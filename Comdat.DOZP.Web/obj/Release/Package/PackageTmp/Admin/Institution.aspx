<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Institution.aspx.cs" Inherits="Comdat.DOZP.Web.Admin.Institution" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1><asp:Label ID="TitleLabel" runat="server" Text="Administrace instituce"></asp:Label></h1>
    <br />
    <table class="formEditTable">
        <tr>
            <th class="required">SIGLA:</th>
            <td><asp:TextBox ID="SiglaTextBox" runat="server" Enabled="false" MaxLength="6" Font-Bold="true" Width="100px"></asp:TextBox></td>
            <td><asp:RegularExpressionValidator ID="SiglaRegularExpressionValidator" runat="server" ControlToValidate="SiglaTextBox" ErrorMessage="SIGLA je chybná" ValidationExpression="^[A-Z]{3}[0-9]{3}$" CssClass="failureNotification" Display="Dynamic"></asp:RegularExpressionValidator></td>
        </tr>
        <tr>
            <th>Název:</th>
            <td><asp:TextBox ID="NameTextBox" runat="server" MaxLength="250" Width="350px"></asp:TextBox></td>
            <td><asp:RequiredFieldValidator ID="NameRequiredFieldValidator" runat="server" ControlToValidate="NameTextBox" ErrorMessage="*" CssClass="failureNotification" Display="Dynamic"></asp:RequiredFieldValidator></td>
        </tr>
        <tr>
            <th>Popis:</th>
            <td><asp:TextBox ID="DescriptionTextBox" runat="server" TextMode="MultiLine" Rows="5" Width="350px"></asp:TextBox></td>
        </tr>
        <tr>
            <th>Adresa:</th>
            <td><asp:TextBox ID="AddressTextBox" runat="server" MaxLength="500" Width="350px"></asp:TextBox></td>
        </tr>
        <tr>
            <th>Web stránky:</th>
            <td><asp:TextBox ID="HomepageTextBox" runat="server" MaxLength="100" Width="350px"></asp:TextBox></td>
            <td><asp:RegularExpressionValidator ID="HomepageRegularExpressionValidator" runat="server" ControlToValidate="HomepageTextBox" ErrorMessage="Web adresa je chybná" ValidationExpression="(http(s)?://)?([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?" CssClass="failureNotification" Display="Dynamic"></asp:RegularExpressionValidator></td>
        </tr>
        <tr>
            <th>Email:</th>
            <td><asp:TextBox ID="EmailTextBox" runat="server" MaxLength="100" Width="350px"></asp:TextBox></td>
            <td><asp:RegularExpressionValidator ID="EmaiRegularExpressionValidator" runat="server" ControlToValidate="EmailTextBox" ErrorMessage="Email adresa je chybná" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" CssClass="failureNotification" Display="Dynamic"></asp:RegularExpressionValidator></td>
        </tr>
        <tr>
            <th>Telefon:</th>
            <td><asp:TextBox ID="TelephoneTextBox" runat="server" MaxLength="50" Width="350px"></asp:TextBox></td>
        </tr>
        <tr>
            <th>&nbsp;</th>
            <td colspan="2">
                <asp:Button ID="SaveButton" runat="server" Text="Uložit" OnClick="SaveButton_Click" SkinID="Button" />
                <asp:Button ID="DeleteButton" runat="server" Text="Odstranit" Visible="false" SkinID="Button" OnClick="DeleteButton_Click" OnClientClick="return confirm('Are you sure you want to submit ?')" />
                <asp:Button ID="CancelButton" runat="server" Text="Storno" CausesValidation="false" OnClick="StornoButton_Click" SkinID="Button" />
            </td>
        </tr>
    </table>
    <asp:Label ID="ErrorLabel" runat="server" CssClass="failureNotification"></asp:Label>
</asp:Content>
