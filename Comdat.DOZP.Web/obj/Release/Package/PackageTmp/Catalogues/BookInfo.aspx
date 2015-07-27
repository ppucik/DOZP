<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="BookInfo.aspx.cs" Inherits="Comdat.DOZP.Web.Catalogues.BookInfo" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="sidebar">
        <div id="InfoHeader" class="filterHeader" runat="server">Informace</div>
        <asp:HyperLink ID="AlephHyperLink" runat="server" Target="_blank">Zobrazit v ALEPHu</asp:HyperLink><br />
        <asp:HyperLink ID="FileHyperLink" runat="server" Target="_blank">Naskenovaný soubor</asp:HyperLink><br />
        <asp:HyperLink ID="PdfHyperLink" runat="server" Target="_blank">PDF dokument</asp:HyperLink><br />
        <asp:HyperLink ID="OprerationsHyperLink" runat="server">Historie zpracování</asp:HyperLink><br />        
        <asp:HyperLink ID="BookUndoLink" runat="server">Vrátit zpracování zpět</asp:HyperLink><br />
        <asp:HyperLink ID="DeleteHyperLink" runat="server">Odstranit záznam</asp:HyperLink><br />        
    </div>
    <div class="content">
        <h1><asp:Label ID="TitleLabel" runat="server" Text="<%$ Resources:DOZP,BookInfo %>"></asp:Label></h1>
        <p><strong><asp:Label ID="PublicationLabel" runat="server"></asp:Label></strong></p>
        <br />
        <p><u>OBÁLKA:</u></p>
        <br />
        <p><u>OBSAH:</u></p>
    </div>
</asp:Content>
