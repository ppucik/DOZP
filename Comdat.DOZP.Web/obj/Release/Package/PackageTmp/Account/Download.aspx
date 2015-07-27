<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Download.aspx.cs" Inherits="Comdat.DOZP.Web.Account.Download" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2><asp:Label ID="TitleLabel" runat="server" Text="<%$ Resources:DOZP,Download %>" ></asp:Label></h2>
    <p>
        Instalace klientské aplikace vyžaduje operačný systém Microsoft Windows XP SP3 a vyšší. 
        Pro běh aplikace je potřebný <a href="http://www.microsoft.com/cs-cz/download/details.aspx?id=17851" target="_blank">.NET Framework 4.0</a>, 
        instaluje se automaticky při instalaci aplikace. Aktualizace jsou prováděny automaticky vždy pri startu aplikace.
    </p>
    <p>
        <asp:HyperLink ID="DozpScanAppHyperLink" runat="server" NavigateUrl="~/Download/Scan/Install.htm" Target="_blank">
            <h3><asp:Label ID="DozpScanAppLabel" runat="server" Text="<%$ Resources:DOZP,DozpScanApp %>" /></h3><br />
            <asp:Image ID="DozpScanAppImage" runat="server" ImageUrl="~/Images/DozpScanApp.png" />
        </asp:HyperLink></p><p>
        <asp:HyperLink ID="DozpOcrAppHyperLink" runat="server" NavigateUrl="~/Download/OCR/Install.htm" Target="_blank">
            <h3><asp:Label ID="DozpOcrAppLabel" runat="server" Text="<%$ Resources:DOZP,DozpOcrApp %>" /></h3><br />
            <asp:Image ID="DozpOcrAppImage" runat="server" ImageUrl="~/Images/DozpOcrApp.png" />
        </asp:HyperLink></p><p>
        Instalace vyžaduje, aby prohlížeč podporoval Microsoft technologii <a href="http://msdn.microsoft.com/cs-cz/library/t71a733d.aspx" target="_blank">ClickOnce</a>.
        Klientskou aplikaci lze instalovat přímo z Internet Exploreru. <br />Instalace z jiného internetového prohlížeče vyžaduje doinstalovaní příslušného doplňku 
        pro podporu technologie ClickOnce: </p><ul>
        <li>Mozilla Firefox vyžaduje <a href="https://addons.mozilla.org/cs/firefox/addon/microsoft-net-framework-assist/" target="_blank">Microsoft .NET Framework Assistant 1.3.1</a> a vyšší.</li>
        <li>Google Chrome vyžaduje <a href="https://chrome.google.com/webstore/detail/clickonce-for-google-chro/eeifaoomkminpbeebjdmdojbhmagnncl" target="_blank">ClickOnce for Google Chrome 2.0.10</a> a vyšší.</li>
        </ul><p>Pro OCR zpracovaní je potřebné nastavit <a href="../Help/FineReaderPDF.mht" target="_blank">FineReader pro vícestránkové PDF</a>.</p>
        <p>VPN Cisco AnyConnect Secure Mobility Client <a href="https://vpn.ff.cuni.cz/+CSCOE+/logon.html"target="_blank">FFVPN</a></p><p>Test WCF služby: <asp:HyperLink ID="TestWcfServiceHyperLink" runat="server" NavigateUrl="~/Service.svc" Text="zde" Target="_blank" />
    </p>
</asp:Content>
