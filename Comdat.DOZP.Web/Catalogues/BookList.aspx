<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="BookList.aspx.cs" Inherits="Comdat.DOZP.Web.Catalogues.BookList" %>
<%@ Register TagPrefix="uc" TagName="MenuCalendar" Src="~/Controls/MenuCalendar.ascx" %>
<%@ Register TagPrefix="uc" TagName="EnumDropDownList" Src="~/Controls/EnumDropDownList.ascx" %>
<%@ Register TagPrefix="uc" TagName="PagerDropDownList" Src="~/Controls/PagerDropDownList.ascx" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="sidebar">
        <uc:MenuCalendar ID="ModifiedDateCalendar" runat="server" OnSelectedChanged="MenuCalendar_SelectedChanged" />
        <uc:EnumDropDownList ID="PartOfBookDropDownList" runat="server" EnumType="PartOfBook" HeaderVisible="true" />
        <uc:EnumDropDownList ID="StatusCodeDropDownList" runat="server" EnumType="StatusCode" HeaderVisible="true" />
    </div>
    <div class="content">
        <h1><asp:Label ID="TitleLabel" runat="server" Text="<%$ Resources:DOZP,Catalogues %>"></asp:Label></h1><br />
        <asp:UpdatePanel ID="BookUpdatePanel" runat="server" UpdateMode="Conditional">
            <Triggers>
                <%--<asp:AsyncPostBackTrigger ControlID="ModifiedDateCalendar" />--%>
                <asp:AsyncPostBackTrigger ControlID="PartOfBookDropDownList" />
                <asp:AsyncPostBackTrigger ControlID="StatusCodeDropDownList" />
                <asp:AsyncPostBackTrigger ControlID="PagerDropDownList" />
            </Triggers>
            <ContentTemplate>
                <asp:GridView ID="BooksGridView" runat="server" 
                    AllowPaging="True"
                    AllowSorting="True"
                    DataKeyNames="BookID"         
                    DataSourceID="BooksDataSource" 
                    SkinID="GridView">
                    <Columns>
                        <asp:BoundField DataField="BookID" HeaderText="ID" InsertVisible="False" ReadOnly="True" SortExpression="BookID" Visible="False" />
                        <asp:TemplateField HeaderText="Číslo zázn." SortExpression="SysNo" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="70px">
                            <ItemTemplate>
                                <asp:HyperLink ID="ControlNumberHyperLink" runat="server" NavigateUrl='<%# Eval("BookID", "BookInfo.aspx?id={0}") %>' Text='<%# Eval("SysNo") %>'></asp:HyperLink>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Publikace" SortExpression="Author">
                            <ItemTemplate>
                                <asp:Label ID="PublicationLabel" runat="server" Text='<%# GetPublication(Eval("Book")) %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="Modified" HeaderText="Změněno" SortExpression="Modified" DataFormatString="{0:G}" HtmlEncode="False" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="65px" />
                        <asp:TemplateField HeaderText="Stav" SortExpression="" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="80px">
                            <ItemTemplate>
                                <asp:Label ID="StatusLabel" runat="server" Text='<%# Enumeration.GetDisplayName(Eval("Status")) %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <PagerSettings NextPageText="&lt;" PreviousPageText="&gt;" Mode="NumericFirstLast" PageButtonCount="10" />
                </asp:GridView>
                <p><asp:Label ID="SummaryLabel" runat="server" Text="Počet záznamů:"></asp:Label>,&nbsp;<uc:PagerDropDownList ID="PagerDropDownList" runat="server" SelectedValue="10" OnSelectedChanged="PagerDropDownList_SelectedChanged" />.</p>
                <asp:ObjectDataSource ID="BooksDataSource" runat="server" 
                    SelectMethod="GetByFilter" 
                    TypeName="Comdat.DOZP.Data.Business.BookComponent" 
                    SortParameterName="sortExpression"
                    OnSelecting="BooksDataSource_Selecting"
                    OnSelected="BooksDataSource_Selected">
                    <SelectParameters>
                        <asp:QueryStringParameter Name="catalogueID" Type="Int32" QueryStringField="id" />
                        <asp:ControlParameter ControlID="ModifiedDateCalendar" Name="modifiedFrom" PropertyName="SelectedDateFrom" Type="DateTime" />
                        <asp:ControlParameter ControlID="ModifiedDateCalendar" Name="modifiedTo" PropertyName="SelectedDateTo" Type="DateTime" />            
                        <asp:ControlParameter ControlID="PartOfBookDropDownList" Name="partOfBook" PropertyName="SelectedValue" Type="Int16" ConvertEmptyStringToNull="true" />
                        <asp:ControlParameter ControlID="StatusCodeDropDownList" Name="status" PropertyName="SelectedValue" Type="Int32" ConvertEmptyStringToNull="true" />
                    </SelectParameters>
                </asp:ObjectDataSource>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
</asp:Content>
