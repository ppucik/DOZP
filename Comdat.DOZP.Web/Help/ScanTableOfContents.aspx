<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ScanTableOfContents.aspx.cs" Inherits="Comdat.DOZP.Web.Help.ScanTableOfContents" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1><asp:Label ID="TitleLabel" runat="server" Text="Nápověda - Skenování obsahu"></asp:Label></h1>
    <p>Po naskenování obálky pokračujete ikonkou <b>Skenovat obsah</b> (klávesa <b>F4</b>) a naskenujte všechny stránky obsahu (každou jednu stranu obsahu skenujte samostatně) – ořez a jiné úpravy provádějte dle vlastního uvážení.</p>
    <p>Pokud naskenujete stránky ve špatném pořadí - lze použít ikonky <b>Posunout vpřed</b> / <b>posunout vzad</b>.</p>
    <p>Pokud kniha nemá obsah nebo jej neskenujete (beletrie apod.), stačí uložit ikonkou <b>Odeslat na zpracování</b> nebo klávesou <b>F9</b>.</p>
    <img alt="" src="ScanTableOfContents1.jpg" width="75%" />

    <p><a href="ScanHelp.aspx"><< Zpět</a></p>
</asp:Content>
