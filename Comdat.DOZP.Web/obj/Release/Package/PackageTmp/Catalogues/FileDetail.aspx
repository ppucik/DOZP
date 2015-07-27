<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="FileDetail.aspx.cs" Inherits="Comdat.DOZP.Web.Catalogues.FileDetail" %>
<%@ Register TagPrefix="uc" TagName="MenuCalendar" Src="~/Controls/MenuCalendar.ascx" %>
<%@ Register TagPrefix="uc" TagName="UsersDropDownList" Src="~/Controls/UsersDropDownList.ascx" %>
<%@ Register TagPrefix="uc" TagName="EnumDropDownList" Src="~/Controls/EnumDropDownList.ascx" %>
<%@ Register TagPrefix="uc" TagName="PagerDropDownList" Src="~/Controls/PagerDropDownList.ascx" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="sidebar">
        <uc:MenuCalendar ID="ModifiedDateCalendar" runat="server" />
        <uc:UsersDropDownList ID="UsersDropDownList" runat="server" HeaderVisible="true" />
        <uc:EnumDropDownList ID="PartOfBookDropDownList" runat="server" EnumType="PartOfBook" HeaderVisible="true" />
        <uc:EnumDropDownList ID="ProcessingDropDownList" runat="server" EnumType="ProcessingMode" HeaderVisible="true" />
        <uc:EnumDropDownList ID="StatusCodeDropDownList" runat="server" EnumType="StatusCode" HeaderVisible="true" />
        <%--
        <div id="EditHeader" class="filterHeader" runat="server">Úpravy</div>
        <asp:HyperLink ID="AlephHyperLink" runat="server" Target="_blank">Zobrazit v ALEPHu</asp:HyperLink><br />
        <asp:HyperLink ID="PdfHyperLink" runat="server" Target="_blank">PDF dokument</asp:HyperLink><br />
        <asp:HyperLink ID="OprerationsHyperLink" runat="server">Historie zpracování</asp:HyperLink><br />        
        <asp:HyperLink ID="BookUndoLink" runat="server">Vrátit zpracování zpět</asp:HyperLink><br />
        <asp:HyperLink ID="DeleteHyperLink" runat="server">Odstranit záznam</asp:HyperLink><br /> 
        --%> 
    </div>
    <div class="content">
        <h1><asp:Label ID="TitleLabel" runat="server" Text="Katalog"></asp:Label></h1><br />      
        <asp:HyperLink ID="FileHyperLink" runat="server" CssClass="rightImage" Visible="false">
            <asp:Image ID="FileImage" runat="server" />
        </asp:HyperLink>
        <asp:DetailsView ID="FilesDetailsView" runat="server"
            DataKeyNames="ScanFileID"
            DataSourceID="FilesDataSource"
            SkinID="DetailsView"
            OnDataBound="FilesDetailsView_DataBound">
            <Fields>
                <asp:BoundField DataField="ScanFileID" HeaderText="ID:" ReadOnly="true" Visible="false" />
                <asp:BoundField DataField="Book.SysNo" HeaderText="Kontrolní číslo:" />
                <asp:BoundField DataField="Book.Author" HeaderText="Autor:" />
                <asp:BoundField DataField="Book.Title" HeaderText="Název:" />
                <asp:BoundField DataField="Book.Year" HeaderText="Rok vydání:" />
                <asp:BoundField DataField="Book.ISBN" HeaderText="ISBN:" />
                <asp:BoundField DataField="Book.Barcode" HeaderText="Čárový kód:" />
                <asp:TemplateField HeaderText="Část knihy" SortExpression="PartOfBook">
                    <ItemTemplate>
                        <asp:Label ID="PartOfBookLabel" runat="server" Text='<%# Enumeration.GetDisplayName(Eval("PartOfBook")) %>'></asp:Label>
                    </ItemTemplate>
                </asp:TemplateField>
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
            </Fields>
        </asp:DetailsView>
        <div class="clear"></div>
        <p><asp:Label ID="SummaryLabel" runat="server" Text="Počet záznamů:" Visible="false"></asp:Label></p>
        <asp:ObjectDataSource ID="FilesDataSource" runat="server" 
            SelectMethod="GetByFilter" 
            TypeName="Comdat.DOZP.Data.Business.ScanFileComponent" 
            SortParameterName="sortExpression"
            OnSelected="FilesDataSource_Selected">
            <SelectParameters>
                <asp:QueryStringParameter Name="catalogueID" Type="Int32" QueryStringField="id" />
                <asp:ControlParameter ControlID="ModifiedDateCalendar" Name="modifiedFrom" PropertyName="SelectedDateFrom" Type="DateTime" />
                <asp:ControlParameter ControlID="ModifiedDateCalendar" Name="modifiedTo" PropertyName="SelectedDateTo" Type="DateTime" />
                <asp:ControlParameter ControlID="UsersDropDownList" Name="userName" PropertyName="SelectedValue" Type="String" ConvertEmptyStringToNull="true" />
                <asp:ControlParameter ControlID="PartOfBookDropDownList" Name="partOfBook" PropertyName="SelectedValue" Type="Int16" ConvertEmptyStringToNull="true" />
                <asp:ControlParameter ControlID="ProcessingDropDownList" Name="processingMode" PropertyName="SelectedValue" Type="Int16" ConvertEmptyStringToNull="true" />
                <asp:ControlParameter ControlID="StatusCodeDropDownList" Name="status" PropertyName="SelectedValue" Type="Int32" ConvertEmptyStringToNull="true" />
            </SelectParameters>
        </asp:ObjectDataSource>
    </div>
</asp:Content>
