<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Comdat.DOZP.Web._Default" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent"></asp:Content>

<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <h1><asp:Label ID="TitleLabel" runat="server" Text="<%$ Resources:DOZP,Welcome %>"></asp:Label></h1>
    <p>
        V souvislosti s novelou autorského zákona, která od poloviny roku 2006 umožňuje obohatit bibliografické záznamy o obsahy 
        popisovaných dokumentů, byl zahájen pilotní projekt připojování obsahů dokumentů do bibliografických záznamů. Cílem je 
        poskytnout uživatelům rozšířenou nabídku vyhledávání detailních odborných informací, bibliografické záznamy obohacené 
        o údaje obsahu slouží především studentům a odborným pracovníkům, kteří tento projekt velmi vítají.
    </p>
    <h3>Skenování obálek a obsahů</h3>
    <ul>
        <li>Instrukce ke skenování obsahů a obálek najdete <a href="Help/ScanInfo.aspx">zde</a>.</li>
    </ul>
    <h3>OCR zpracování obsahů</h3>
    <ul>
        <li>Informace k OCR zpracování obsahů najdete <a href="Help/OcrInfo.aspx">zde</a>.</li>
    </ul>
</asp:Content>
