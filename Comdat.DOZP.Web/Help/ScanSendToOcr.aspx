<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ScanSendToOcr.aspx.cs" Inherits="Comdat.DOZP.Web.Help.ScanSendToOcr" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1><asp:Label ID="TitleLabel" runat="server" Text="Nápověda - Odeslaní na zpracování"></asp:Label></h1>
    <p>Až budete s prací hotovi – ikonka <b>Odeslat na zpracování</b> (klávesa <b>F9</b>) – systém vám shrne, co jste naskenovali a zda chcete obsah zpracovat pomocí OCR (pouze u nelatinkového písma prosím nezaškrtávat, obsah se uloží pouze jako PDF soubor a nebude v něm možné vyhledávat).</p>
    <p>Stav naskenovaných obálek a obsahů si můžete zkontrolovat na webu v sekci Katalog, kde se zalogujte stejně jako do systému DOZP (stejné jméno i heslo). Na této stránce také uvidíte, zda byla některá stránka obsahu špatně naskenována - je zamítnuta a vrácena z OCR zpracování. Poté je třeba stránku přeskenovat - špatnou smazat a nahradit nově naskenovanou.</p>
    <img alt="" src="ScanSendToOcr1.jpg" width="75%" />

    <p><a href="ScanHelp.aspx"><< Zpět</a></p>
</asp:Content>
