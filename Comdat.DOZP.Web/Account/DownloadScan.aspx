<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="DownloadScan.aspx.cs" Inherits="Comdat.DOZP.Web.Account.DownloadScan" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1><asp:Label ID="TitleLabel" runat="server" Text="<%$ Resources:DOZP,DozpScanApp %>" ></asp:Label></h1>
    <p>
        Instalace klientské aplikace pro skenování vyžaduje operačný systém Microsoft Windows 7 a vyšší. 
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
    <asp:HyperLink ID="DozpScanAppHyperLink" runat="server" NavigateUrl="~/Download/App/Install.htm" Target="_blank">
        <asp:Image ID="DozpScanAppImage" runat="server" ImageUrl="~/Images/DozpAppInstall.png" />
    </asp:HyperLink>

    <h3>Nastavení aplikace</h3>
    <p>
        Po naistalovaní aplikace je potřebné vybrat <b>Centrální katalog UK</b> a nastavit připojený skener, vyberte odpovídající nainstalovaný <b>WIA</b> zdroj.
    </p>
    <img alt="" src="../Images/DozpAppOptions.png" />
    <p style="color:Red">
        Pokuď nelze aplikaci nainstalovat, kontaktujte prosím administrátora LVT.
    </p>
    <br />
</asp:Content>
