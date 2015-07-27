<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ScanInfo.aspx.cs" Inherits="Comdat.DOZP.Web.Help.ScanInfo" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1><asp:Label ID="TitleLabel" runat="server" Text="Skenování obálek a obsahů"></asp:Label></h1>
    <p>Instrukce ke skenování obsahů a obálek:</p>
    <h3>Pro jaké dokumenty?</h3>
    <ul>
        <li>Pro všechny nově nakoupené dokumenty skenování obálek</li>
        <li>Výběrově pro nakoupené dokumenty skenování obsahů</li>
        <li>Dle zvážení obé u retrokatalogizace / darů</li>
        <li>Všechny jazyky i písma (u nelatinkových písem však u obsahů zaškrtnout, že nebude procházet OCR)</li>
    </ul>
    <h3>Nutné vybavení pro skenování?</h3>
    <ul>
        <li>Skenery - připojení přímo k počítači (dodané knižní skenery Plustek OpticBook - nikoliv síťové)</li>
        <li>Aplikace DOZP - (použití viz <a href="ScanHelp.aspx">návod</a>) - nutno instalovat na konkrétní počítač pod vaším profilem, a to ve spolupráci s LVT</li>
        <li>Počítač, na kterém budete skenovat, nemusí být totožný s počítačem, kde katalogizujete (i knihoven je vhodné např. skener umístit do studovny, kde ho mohou studenti volně používat a kde se bude např. skenovat mimo otevírací dobu knihovny)</li>
        <li>Skenovat lze dávkově (jednou za týden…) nebo rovnou po katalogizaci - záleží na tom, jak vám to bude vyhovovat a např. na tom, jestli máte skener přímo u počítače, kde katalogizujete</li>
        <li>Skenovat může i třeba stipendista na výpůjčním pultu (v knihovně může mít loginy do aplikace libovolné množství osob)</li>
    </ul> 
</asp:Content>
