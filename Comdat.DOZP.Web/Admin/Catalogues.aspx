<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Catalogues.aspx.cs" Inherits="Comdat.DOZP.Web.Admin.Catalogues" %>
<%@ Register TagPrefix="uc" TagName="InstitutionsDropDownList" Src="~/Controls/InstitutionsDropDownList.ascx" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1><asp:Label ID="TitleLabel" runat="server" Text="Administrace katalogů"></asp:Label></h1>
    <p>Administrace umožňuje editovat základní údaje o katalogu nebo <asp:HyperLink ID="InsertHyperLink" runat="server">vytvořit nový katalog</asp:HyperLink>.</p>
    <uc:InstitutionsDropDownList ID="InstitutionsDropDownList" runat="server" AutoPostBack="true" EmptyVisible="true" TitleVisible="true" />
    <asp:UpdatePanel ID="CataloguesUpdatePanel" runat="server" UpdateMode="Conditional">
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="InstitutionsDropDownList" />
        </Triggers>
        <ContentTemplate>
            <asp:GridView ID="CataloguesGridView" runat="server" 
                AllowPaging="True"
                AllowSorting="True"
                DataKeyNames="CatalogueID" 
                DataSourceID="CataloguesDataSource"
                SkinID="GridView">        
                <Columns>
                    <asp:CheckBoxField DataField="Enabled" HeaderText="" SortExpression="Enabled" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="30" />
                    <asp:BoundField DataField="CatalogueID" HeaderText="ID" InsertVisible="False" ReadOnly="True" SortExpression="CatalogueID" Visible="False" />
                    <asp:HyperLinkField DataTextField="DatabaseName" 
                        HeaderText="Databáze" 
                        SortExpression="DatabaseName" 
                        DataNavigateUrlFields="CatalogueID" 
                        DataNavigateUrlFormatString="Catalogue.aspx?id={0}"
                        HeaderStyle-HorizontalAlign="Center"
                        ItemStyle-HorizontalAlign="Center"
                        ItemStyle-Width="70px" />
                    <asp:BoundField DataField="Name" HeaderText="Název" SortExpression="Name" />
                    <asp:BoundField DataField="ZServerUrl" HeaderText="URL" SortExpression="ZServerUrl" />
                    <asp:BoundField DataField="ZServerPort" HeaderText="Port" SortExpression="ZServerPort" />
                    <asp:BoundField DataField="Charset" HeaderText="Znaková sada" SortExpression="Charset" />
                    <asp:BoundField DataField="Modified" HeaderText="Změněno" SortExpression="Modified" DataFormatString="{0:g}" HtmlEncode="False" />
                </Columns>
            </asp:GridView>            
            <asp:ObjectDataSource ID="CataloguesDataSource" runat="server" 
                SelectMethod="GetByInstitutionID"
                SortParameterName="sortExpression"
                TypeName="Comdat.DOZP.Data.Business.CatalogueComponent">
                <SelectParameters>
                    <asp:ControlParameter ControlID="InstitutionsDropDownList" Name="institutionID" PropertyName="SelectedValue" Type="Int32" />
                </SelectParameters>
            </asp:ObjectDataSource>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
