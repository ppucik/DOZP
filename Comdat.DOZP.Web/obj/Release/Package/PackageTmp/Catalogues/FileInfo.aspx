<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="FileInfo.aspx.cs" Inherits="Comdat.DOZP.Web.Catalogues.FileInfo" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1><asp:Label ID="TitleLabel" runat="server" Text="Titul"></asp:Label></h1><br />
    <div class="rightImage">
        <asp:HyperLink ID="FileHyperLink" runat="server" CssClass="rightImage" Target="_blank">
            <asp:Image ID="FileImage" runat="server" Width="250px" />
        </asp:HyperLink>
    </div>
    <div> 
        <asp:DetailsView ID="FilesDetailsView" runat="server"
            DataKeyNames="ScanFileID"
            DataSourceID="FilesDataSource"
            SkinID="DetailsView"
            Width="680px"
            OnDataBound="FilesDetailsView_DataBound">
            <Fields>
                <asp:BoundField DataField="ScanFileID" HeaderText="ID:" ReadOnly="true" Visible="false" />
                <%--<asp:BoundField DataField="Book.SysNo" HeaderText="Kontrolní číslo:" />--%>
                <asp:BoundField DataField="Book.Author" HeaderText="Autor:" />
                <asp:BoundField DataField="Book.Title" HeaderText="Název:" />
                <asp:BoundField DataField="Book.Volume" HeaderText="Díl, svazek:" />
                <asp:BoundField DataField="Book.Year" HeaderText="Rok vydání:" />
                <asp:BoundField DataField="Book.ISBN" HeaderText="ISBN:" />
                <asp:BoundField DataField="Book.NBN" HeaderText="ČNB:" />
                <asp:BoundField DataField="Book.OCLC" HeaderText="OCLC:" />
                <asp:BoundField DataField="Book.Barcode" HeaderText="Čárový kód:" />
                <asp:TemplateField HeaderText="Soubor:" SortExpression="FileName">
                    <ItemTemplate>
                        <asp:Label ID="FileNameLabel" runat="server" Text='<%# Eval("FileName") %>'></asp:Label>
                        <asp:HiddenField ID="FileNameHiddenField" runat="server" Value='<%# Eval("FileName") %>'></asp:HiddenField>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="PageCount" HeaderText="Počet stran:" />
                <asp:TemplateField HeaderText="OCR" SortExpression="UseOCR">
                    <ItemTemplate>
                        <asp:Label ID="ProcessingModeLabel" runat="server" Text='<%# ((bool)Eval("UseOCR") ? "Ano" :"Ne") %>'></asp:Label>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="Created" HeaderText="Vytvořeno:" DataFormatString="{0:G}" HtmlEncode="False" />
                <asp:BoundField DataField="Modified" HeaderText="Změněno:" DataFormatString="{0:G}" HtmlEncode="False" />
                <asp:BoundField DataField="Comment" HeaderText="Poznámka:" />
                <asp:TemplateField HeaderText="Stav" SortExpression="Status">
                    <ItemTemplate>
                        <asp:Label ID="StatusLabel" runat="server" Text='<%# Enumeration.GetDisplayName(Eval("Status")) %>'></asp:Label>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="OcrText" HeaderText="Obsah:" HeaderStyle-VerticalAlign="Top" />
            </Fields>
        </asp:DetailsView>
        <div class="clear"></div>
        <asp:ObjectDataSource ID="FilesDataSource" runat="server" 
            SelectMethod="GetByID" 
            TypeName="Comdat.DOZP.Data.Business.ScanFileComponent">
            <SelectParameters>
                <asp:QueryStringParameter Name="scanFileID" QueryStringField="id" Type="Int32" />
            </SelectParameters>
        </asp:ObjectDataSource>
    </div>
</asp:Content>
