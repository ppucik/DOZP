<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="FileList.aspx.cs" Inherits="Comdat.DOZP.Web.Catalogues.FileList" %>
<%@ Register TagPrefix="uc" TagName="MenuCalendar" Src="~/Controls/MenuCalendar.ascx" %>
<%@ Register TagPrefix="uc" TagName="UsersDropDownList" Src="~/Controls/UsersDropDownList.ascx" %>
<%@ Register TagPrefix="uc" TagName="EnumDropDownList" Src="~/Controls/EnumDropDownList.ascx" %>
<%@ Register TagPrefix="uc" TagName="PagerDropDownList" Src="~/Controls/PagerDropDownList.ascx" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1><asp:Label ID="TitleLabel" runat="server" Text="Katalog"></asp:Label></h1><br />
    <div class="sidebar">
        <uc:MenuCalendar ID="ModifiedDateCalendar" runat="server" OnSelectedChanged="MenuCalendar_SelectedChanged" />
        <uc:UsersDropDownList ID="UsersDropDownList" runat="server" HeaderVisible="true" />
        <uc:EnumDropDownList ID="PartOfBookDropDownList" runat="server" EnumType="PartOfBook" HeaderVisible="true" />
        <uc:EnumDropDownList ID="ProcessingDropDownList" runat="server" EnumType="ProcessingMode" HeaderVisible="true" />
        <uc:EnumDropDownList ID="StatusCodeDropDownList" runat="server" EnumType="StatusCode" HeaderVisible="true" />
    </div>
    <div class="content">
        <asp:UpdatePanel ID="FileUpdatePanel" runat="server" UpdateMode="Conditional">
            <Triggers>
                <%--<asp:AsyncPostBackTrigger ControlID="ModifiedDateCalendar" />--%>
                <asp:AsyncPostBackTrigger ControlID="UsersDropDownList" />
                <asp:AsyncPostBackTrigger ControlID="PartOfBookDropDownList" />
                <asp:AsyncPostBackTrigger ControlID="ProcessingDropDownList" />
                <asp:AsyncPostBackTrigger ControlID="StatusCodeDropDownList" />
                <asp:AsyncPostBackTrigger ControlID="PagerDropDownList" />
            </Triggers>
            <ContentTemplate>
                <asp:GridView ID="FilesGridView" runat="server" 
                    AllowPaging="True"
                    AllowSorting="True"
                    DataKeyNames="ScanFileID"         
                    DataSourceID="FilesDataSource" 
                    SkinID="GridView" 
                    OnRowDataBound="FilesGridView_RowDataBound">
                    <Columns>
                        <asp:BoundField DataField="ScanFileID" HeaderText="ID" InsertVisible="False" ReadOnly="True" SortExpression="ScanFileID" Visible="False" />
                        <asp:ImageField HeaderText="" DataImageUrlField="Status" DataImageUrlFormatString="~/Images/{0}.png">          
                            <ItemStyle HorizontalAlign="Center" VerticalAlign="Middle" Width="20px" />
                        </asp:ImageField>
                        <asp:TemplateField HeaderText="Záznam č." SortExpression="SysNo" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="75px">
                            <ItemTemplate>
                                <asp:HyperLink ID="ControlNumberHyperLink" runat="server" NavigateUrl='<%# Eval("ScanFileID", "FileInfo.aspx?id={0}") %>' Text='<%# Eval("Book.SysNo") %>' Target="_blank"></asp:HyperLink>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Publikace">
                            <ItemTemplate>
                                <asp:Label ID="PublicationLabel" runat="server" Text='<%# GetPublication(Eval("Book")) %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Typ" SortExpression="PartOfBook" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <asp:Label ID="PartOfBookLabel" runat="server" Text='<%# Enumeration.GetDisplayName(Eval("PartOfBook")) %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="OCR" SortExpression="UseOCR" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <asp:Label ID="ProcessingModeLabel" runat="server" Text='<%# ((bool)Eval("UseOCR") ? "Ano" :"Ne") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="Modified" HeaderText="Změněno" SortExpression="Modified" DataFormatString="{0:G}" HtmlEncode="False" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="65px" />
                        <asp:TemplateField HeaderText="Stav" SortExpression="Status" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="85px">
                            <ItemTemplate>
                                <asp:Label ID="StatusLabel" runat="server" Text='<%# Enumeration.GetDisplayName(Eval("Status")) %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <PagerSettings NextPageText="&lt;" PreviousPageText="&gt;" Mode="NumericFirstLast" PageButtonCount="10" />
                </asp:GridView>
                <p><asp:Label ID="SummaryLabel" runat="server" Text="Počet záznamů:"></asp:Label>,&nbsp;<uc:PagerDropDownList ID="PagerDropDownList" runat="server" SelectedValue="10" OnSelectedChanged="PagerDropDownList_SelectedChanged" />.</p>
            </ContentTemplate>
        </asp:UpdatePanel>
        <asp:ObjectDataSource ID="FilesDataSource" runat="server" 
            SelectMethod="GetByFilter" 
            TypeName="Comdat.DOZP.Data.Business.ScanFileComponent" 
            SortParameterName="sortExpression"
            OnSelecting="FilesDataSource_Selecting"
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
