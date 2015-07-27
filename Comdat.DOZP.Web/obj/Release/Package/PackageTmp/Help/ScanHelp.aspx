<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ScanHelp.aspx.cs" Inherits="Comdat.DOZP.Web.Help.ScanHelp" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1><asp:Label ID="TitleLabel" runat="server" Text="Nápověda pro aplikaci skenování"></asp:Label></h1>
    <p>Obálky i obsahy se skenují až ve chvíli, kdy je dokument zpracovaný v systému ALEPH</p>
    <ul>
        <li>obálky u všech knih (ne časopisy, diplomové práce, CD, DVD…) – přesnější informace popřípadě dodá ředitelka Knihovny FF UK</li>
        <li>obsahy pouze u odborných knih dle uvážení (ne beletrie, poezie).</li>
    </ul>

    <h3>Postup práce při skenování:</h3>
    <p><a href="ScanAppSetup.aspx">Popis a nastavení aplikace</a></p>
    <p><a href="ScanNewBook.aspx">Vytvoření nové publikace</a></p>
    <p><a href="ScanFrontCover.aspx">Skenování obálky</a></p>
    <p><a href="ScanTableOfContents.aspx">Skenování obsahu</a></p>
    <p><a href="ScanSendToOcr.aspx">Odeslaní na zpracování</a></p>
    
    <h3>Dokumenty ke stažení:</h3>
    <p><a href="NavodDozpSkenovani.pdf" target="_blank">Návod v PDF formátu</a>
    <p><a href="DOZP-Export-1.1.pdf" target="_blank">Export do ALEPHu</a></p>
</asp:Content>
