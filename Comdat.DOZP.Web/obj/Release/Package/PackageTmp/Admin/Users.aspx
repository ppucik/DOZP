<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Users.aspx.cs" Inherits="Comdat.DOZP.Web.Admin.Users" %>
<%@ Register TagPrefix="uc" TagName="InstitutionsDropDownList" Src="~/Controls/InstitutionsDropDownList.ascx" %>
<%@ Register TagPrefix="uc" TagName="PagerDropDownList" Src="~/Controls/PagerDropDownList.ascx" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1><asp:Label ID="TitleLabel" runat="server" Text="Administrace uživatelů"></asp:Label></h1>
    <p>Administrace umožňuje editovat základní údaje a přístupová práva uživatele nebo <asp:HyperLink ID="InsertHyperLink" runat="server" NavigateUrl="UserEdit.aspx">vytvořit nového uživatele</asp:HyperLink>. Odstranit nelze uživatele, který již v systému pracoval, proto v případe, že chcete uživateli zakázat přístup, nastavte ho jako neaktivní.</p>
    <table>
        <tr>
            <td><uc:InstitutionsDropDownList ID="InstitutionsDropDownList" runat="server" AutoPostBack="true" EmptyVisible="true" TitleVisible="true" /></td>
            <td><asp:CheckBox ID="ActiveCheckBox" runat="server" AutoPostBack="true" Text="Zobrazit pouze aktivní uživatele" /></td>
            <td></td>
        </tr>
    </table>
    <asp:UpdatePanel ID="UsersUpdatePanel" runat="server" UpdateMode="Conditional">
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="InstitutionsDropDownList" />
            <%--<asp:AsyncPostBackTrigger ControlID="ActiveCheckBox" />--%>
            <asp:AsyncPostBackTrigger ControlID="PagerDropDownList" />
        </Triggers>
        <ContentTemplate> 
            <asp:GridView ID="UsersGridView" runat="server" 
                AllowPaging="True"
                AllowSorting="True"
                DataKeyNames="UserName" 
                DataSourceID="UsersDataSource" 
                SkinID="GridView">
                <Columns>
                    <asp:CheckBoxField DataField="IsApproved" HeaderText="" SortExpression="IsApproved" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="30" />
                    <asp:HyperLinkField DataTextField="UserName" 
                        HeaderText="Uživatel" 
                        SortExpression="UserName" 
                        DataNavigateUrlFields="UserName" 
                        DataNavigateUrlFormatString="~/Admin/UserEdit.aspx?name={0}" />
                    <asp:BoundField DataField="FullName" HeaderText="Celé jméno" SortExpression="FullName" />
                    <asp:TemplateField HeaderText="Role" SortExpression="RoleName">
                        <ItemTemplate>
                            <asp:Label ID="RoleNameLabel" runat="server" Text='<%# Resources.RolesResource.ResourceManager.GetString(Eval("RoleName").ToString()) %>'></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="Email" HeaderText="Email" SortExpression="Email" />
                    <asp:BoundField DataField="Telephone" HeaderText="Telefon" SortExpression="Telephone" />
                    <asp:BoundField DataField="LastUpdate" HeaderText="Změněno" SortExpression="LastUpdate" DataFormatString="{0:g}" HtmlEncode="False" />
                    <asp:BoundField DataField="Comment" HeaderText="Poznámka" SortExpression="Comment" />
                </Columns>     
            </asp:GridView><hr />
            <asp:Label ID="SummaryLabel" runat="server" Text="Počet uživatelů:"></asp:Label>,&nbsp;<uc:PagerDropDownList ID="PagerDropDownList" runat="server" SelectedValue="10" OnSelectedChanged="PagerDropDownList_SelectedChanged" />.
            <asp:ObjectDataSource ID="UsersDataSource" runat="server" 
                SelectMethod="GetByInstitutionID" 
                TypeName="Comdat.DOZP.Data.Business.UserComponent"
                SortParameterName="sortExpression"
                OnSelecting="UsersDataSource_Selecting"
                OnSelected="UsersDataSource_Selected">
                <SelectParameters>
                    <asp:ControlParameter ControlID="InstitutionsDropDownList" Name="institutionID" PropertyName="SelectedValue" Type="Int32" />
                    <asp:ControlParameter ControlID="ActiveCheckBox" Name="active" PropertyName="Checked" Type="Boolean" />
                </SelectParameters>
            </asp:ObjectDataSource>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
