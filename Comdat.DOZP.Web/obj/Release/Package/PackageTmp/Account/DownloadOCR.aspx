<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="DownloadOCR.aspx.cs" Inherits="Comdat.DOZP.Web.Account.DownloadOCR" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1><asp:Label ID="TitleLabel" runat="server" Text="<%$ Resources:DOZP,DozpOcrApp %>" ></asp:Label></h1>
    <p>
        Instalace klientské aplikace pro OCR zpracování vyžaduje operačný systém Microsoft Windows 7 a vyšší. 
        Pro běh aplikace je potřebný <a href="http://www.microsoft.com/cs-cz/download/details.aspx?id=17851" target="_blank">.NET Framework 4.0</a>, 
        instaluje se automaticky při instalaci aplikace. Aktualizace jsou prováděny automaticky vždy pri startu aplikace.
    </p>
    <p>
        Instalace vyžaduje, aby prohlížeč podporoval Microsoft technologii <b>ClickOnce</b>.
        Klientskou aplikaci lze instalovat přímo z Internet Exploreru.<br />
        Instalace z jiného internetového prohlížeče vyžaduje doinstalovaní příslušného doplňku pro podporu technologie ClickOnce.<br />
        Mozilla Firefox vyžaduje doplňek <a href="https://addons.mozilla.org/cs/firefox/addon/microsoft-net-framework-assist/" target="_blank">Microsoft .NET Framework Assistant</a> a
        Google Chrome vyžaduje <a href="https://chrome.google.com/webstore/detail/clickonce-for-google-chro/eeifaoomkminpbeebjdmdojbhmagnncl" target="_blank">ClickOnce for Google Chrome</a>.
    </p>
    <asp:HyperLink ID="DozpOcrAppHyperLink" runat="server" NavigateUrl="~/Download/OCR/Install.htm" Target="_blank">
        <asp:Image ID="DozpOcrAppImage" runat="server" ImageUrl="~/Images/DozpOcrInstall.png" />
    </asp:HyperLink>
    <h2>ABBYY FineReader</h2>
    <p>
        Pro OCR zpracovaní je potřebné nainstalovat aplikaci <a href="http://www.abbyy.com/finereader/professional/" target="_blank">ABBYY FineReader 12</a>,
        která je umístěna na serveru FF UK. Po naistalovaní je potřebné naimportovat ze serveru do správce úloh soubor <b>DOZP.fta</b>, 
        který obsahuje automatizovanou úlohu pro zpracování obsahu.
    </p>
    <asp:HyperLink ID="FineReaderHyperLink" runat="server" NavigateUrl="\\FFAS04\FineReader\Setup.exe" Target="_blank">
        <asp:Image ID="FineReaderImage" runat="server" ImageUrl="~/Images/FineReaderInstall.png" />
    </asp:HyperLink>
    <p style="color:Red">
        Pokuď nelze aplikaci nainstalovat, kontaktujte prosím administrátora LVT.
    </p>
    <br />
</asp:Content>
