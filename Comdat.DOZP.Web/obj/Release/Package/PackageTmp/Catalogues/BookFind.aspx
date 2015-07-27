<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="BookFind.aspx.cs" Inherits="Comdat.DOZP.Web.Catalogues.BookFind" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1><asp:Label ID="TitleLabel" runat="server" Text="<%$ Resources:DOZP,BookFind %>"></asp:Label></h1><br />
    <asp:GridView ID="FilesGridView" runat="server" 
        AllowPaging="True"
        AllowSorting="True"
        DataKeyNames="ScanFileID"         
        DataSourceID="FilesDataSource" 
        SkinID="GridView">
        <Columns>
            <asp:BoundField DataField="ScanFileID" HeaderText="ID" InsertVisible="False" ReadOnly="True" SortExpression="ScanFileID" Visible="False" />
            <asp:ImageField HeaderText="" DataImageUrlField="Status" DataImageUrlFormatString="~/Images/{0}.png">          
                <ItemStyle HorizontalAlign="Center" VerticalAlign="Middle" Width="20px" />
            </asp:ImageField>
            <asp:TemplateField HeaderText="Záznam č." SortExpression="SysNo" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="75px">
                <ItemTemplate>
                    <asp:HyperLink ID="ControlNumberHyperLink" runat="server" NavigateUrl='<%# Eval("ScanFileID", "FileInfo.aspx?id={0}") %>' Text='<%# Eval("Book.SysNo") %>' Target="_blank"></asp:HyperLink>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Publikace">
                <ItemTemplate>
                    <asp:Label ID="PublicationLabel" runat="server" Text='<%# GetPublication(Eval("Book")) %>'></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Typ" SortExpression="PartOfBook" ItemStyle-HorizontalAlign="Center">
                <ItemTemplate>
                    <asp:Label ID="PartOfBookLabel" runat="server" Text='<%# Enumeration.GetDisplayName(Eval("PartOfBook")) %>'></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="OCR" SortExpression="UseOCR" ItemStyle-HorizontalAlign="Center">
                <ItemTemplate>
                    <asp:Label ID="ProcessingModeLabel" runat="server" Text='<%# ((bool)Eval("UseOCR") ? "Ano" :"Ne") %>'></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="Modified" HeaderText="Změněno" SortExpression="Modified" DataFormatString="{0:G}" HtmlEncode="False" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="65px" />
            <asp:TemplateField HeaderText="Stav" SortExpression="Status" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="85px">
                <ItemTemplate>
                    <asp:Label ID="StatusLabel" runat="server" Text='<%# Enumeration.GetDisplayName(Eval("Status")) %>'></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
        <PagerSettings NextPageText="&lt;" PreviousPageText="&gt;" Mode="NumericFirstLast" PageButtonCount="10" />
    </asp:GridView>
    <asp:ObjectDataSource ID="FilesDataSource" runat="server" 
        SelectMethod="FullTextSearch" 
        TypeName="Comdat.DOZP.Data.Business.ScanFileComponent">
        <SelectParameters>
            <asp:ProfileParameter Name="institutionID" PropertyName="InstitutionID" Type="Int32" />
            <asp:QueryStringParameter Name="text" Type="String" QueryStringField="text" />
        </SelectParameters>
    </asp:ObjectDataSource>
</asp:Content>
