<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ScanNewBook.aspx.cs" Inherits="Comdat.DOZP.Web.Help.ScanNewBook" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1><asp:Label ID="TitleLabel" runat="server" Text="Nápověda - Nová publikace"></asp:Label></h1>
    <p>Kliknutím na ikonku <b>Nová publikace</b> (nebo tlačítko pro rychlou volbu <b>F2</b>) se otevře okno, ve kterém nejdříve daný dokument vyhledáte v systému ALEPH – defaultně je nastaven čárový kód knihy (nejlépe zadávat čtečkou). Pokud by se dokument nedařilo vyhledat, je možno použít jeho SYSNO z ALEPHu nebo ISBN (zápis je možný s pomlčkami alebo bez).</p>
    <img alt="" src="ScanNewBook1.jpg" />
    <p>Pokud má dokument 2 a více přidělených ISBN (např. jiný nakladatel) nebo má více svazků, systém ukáže nabídku – je nutné jedno ISBN vybrat a buď ponechat popis, který systém napíše – např. Díl. 1 nebo napsat text, který daný svazek rozliší.</p>
    <img alt="" src="ScanNewBook3.jpg" />
    <p>Pokud už byl dokument skenován, systém na to automaticky upozorní nebo nabídne úpravu (např. pokud ho již naskenoval jiný knihovník nebo pokud chcete přeskenovat nekvalitní obálku nebo jste zapomněli naskenovat obsah a chcete se ke skenování vrátit aj.).</p>
    <img alt="" src="ScanNewBook2.jpg" />
    <p>Pokud už byl dokument kompletně (obálka i obsah) exportován do ALEPHu systém na to automaticky upozorní, že již není možné dokument skenovat.</p>
    <img alt="" src="ScanNewBook4.jpg" />

    <p><a href="ScanHelp.aspx"><< Zpět</a></p>
</asp:Content>
