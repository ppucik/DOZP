<%@ Page Title="About Us" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="Contact.aspx.cs" Inherits="Comdat.DOZP.Web.Contact" %>

<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <h1><asp:Label ID="TitleLabel" runat="server" Text="<%$ Resources:DOZP,Contact %>"></asp:Label></h1><br />
    <div class="vcard">
        <div class="fn org" style="font-weight:bold">Knihovna FF UK</div>
        <div class="adr">
            <div class="street-address">nám. Jana Palacha 2</div>
            <div><span class="postal-code">116 38</span> <span class="locality">Praha 1</span></div>
            <div>tel.: <span class="tel">221 619 284</span></div>
            <div>email: <a class="email" href="mailto:knihovna@ff.cuni.cz">knihovna@ff.cuni.cz</a></div>
        </div> 
    </div>
    <br />
    <h3><asp:Label ID="TechnicalSupportLabel" runat="server" Text="<%$ Resources:DOZP,TechnicalSupport %>"></asp:Label>:</h3><br />
    <div class="vcard">
        <div class="fn org"><asp:Label ID="CompanyNameLabel" runat="server" Font-Bold="true" Text="Comdat s.r.o."></asp:Label></div>
        <div class="adr">
            <div class="street-address">Žirovnická 2389</div>
            <div><span class="postal-code">106 00</span> <span class="locality">Praha 10</span></div>
            <div class="tel">tel.: 603 236 329</div>
            <div class="email">email: <a href="mailto:office@comdat.cz">office@comdat.cz</a></div>
        </div> 
    </div>
    <br />
    <div id="vcard-MiroslavBares" class="vcard">
        <div class="fn" style="font-weight:bold" >Ing. Miroslav Bareš</div>
        <div class="role"></div>
        <div>email: <a class="email" href="mailto:mbares@comdat.cz">mbares@comdat.cz</a></div>
    </div>
    <br />
    <div id="vcard-PeterPucik" class="vcard">
        <div class="fn" style="font-weight:bold" >Ing. Peter Púčik</div>
        <div class="role">IT vývoj a správa webového serveru</div>
        <div>email: <a class="email" href="mailto:ppucik@comdat.cz">ppucik@comdat.cz</a></div>
    </div>
    <br />
</asp:Content>
