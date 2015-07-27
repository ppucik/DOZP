<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ScanFrontCover.aspx.cs" Inherits="Comdat.DOZP.Web.Help.ScanFrontCover" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1><asp:Label ID="TitleLabel" runat="server" Text="Nápověda - Skenování obálky"></asp:Label></h1>
    <p>Když je k dokumentu obálka již přiřazena, systém DOZP ji automaticky stáhne z webu <a href="http://obalkyknih.cz" target="_blank">ObalkyKnih.cz</a>, můžeme tedy přejít ke skenování obsahu - výběrově dle typu literatury.</p>
    <p>Pokud dokument nebyl dosud zpracován, vlevo uvidíte sloupec se SYSNEM, autorem, názvem knihy, rokem vydání a popř. ISBN -> je možno zahájit samotné skenování – klávesou <b>F3</b> nebo volbou <b>Skenovat obálku</b>.</p>
    <p>Po naskenování obálky ji lze pomocí ikonek vpravo nahoře různě upravit – oříznout černé nebo bílé okraje, otočit třeba jen o určitý úhel, automaticky vyrovnat, pozměnit barvy aj. Někdy nebude úpravy třeba, sami uvidíte, jak budete tyto funkce potřebovat. Obálku lze i smazat a naskenovat znovu – ikonka odstranit.</p>
    <img alt="" src="ScanFrontCover1.jpg" width="75%" />

    <p><a href="ScanHelp.aspx"><< Zpět</a></p>
</asp:Content>
