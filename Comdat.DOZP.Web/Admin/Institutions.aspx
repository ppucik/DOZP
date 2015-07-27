<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Institutions.aspx.cs" Inherits="Comdat.DOZP.Web.Admin.Institutions" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1><asp:Label ID="TitleLabel" runat="server" Text="Administrace institucí"></asp:Label></h1>
    <p>Administrace umožňuje editovat základní údaje o instituci nebo <asp:HyperLink ID="InsertHyperLink" runat="server">vytvořit novou instituci</asp:HyperLink>.</p>
    <asp:UpdatePanel ID="InstitutionsUpdatePanel" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <asp:GridView ID="InstitutionsGridView" runat="server" 
                AllowPaging="True"
                AllowSorting="False"
                DataKeyNames="InstitutionID" 
                DataSourceID="InstitutionsObjectDataSource"
                SkinID="GridView">
                <Columns>
                    <asp:BoundField DataField="InstitutionID" HeaderText="ID" InsertVisible="False" ReadOnly="True" SortExpression="InstitutionID" Visible="False" />
                    <asp:HyperLinkField DataTextField="Sigla" 
                        HeaderText="SIGLA" 
                        SortExpression="Sigla" 
                        DataNavigateUrlFields="InstitutionID" 
                        DataNavigateUrlFormatString="Institution.aspx?id={0}"
                        HeaderStyle-HorizontalAlign="Center"
                        ItemStyle-HorizontalAlign="Center" 
                        ItemStyle-Width="60px" />
                    <asp:BoundField DataField="Name" HeaderText="Název" SortExpression="Name" />
                    <asp:BoundField DataField="Address" HeaderText="Adresa" SortExpression="Address" />
                    <asp:BoundField DataField="Homepage" HeaderText="Web stránky" SortExpression="Homepage" />
                    <asp:BoundField DataField="Email" HeaderText="Email" SortExpression="Email" />
                    <asp:BoundField DataField="Telephone" HeaderText="Telefon" SortExpression="Telephone" />
                </Columns>
            </asp:GridView>
            <asp:ObjectDataSource ID="InstitutionsObjectDataSource" runat="server" 
                SelectMethod="GetAll"
                TypeName="Comdat.DOZP.Data.Business.InstitutionComponent">
            </asp:ObjectDataSource>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
