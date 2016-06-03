<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.ProductTypes" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="producttypes.aspx.cs" %>
<%@ Register TagPrefix="aspdnsf" TagName="LinkGroupEntities" Src="Controls/LinkGroupEntities.ascx" %>

<%@ Register TagPrefix="aspdnsf" Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls.Listing" %>
<%@ Register TagPrefix="aspdnsf" TagName="StringFilter" Src="Controls/Listing/StringFilter.ascx" %>
<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<h1>
		<i class="fa fa-archive"></i>
		<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.Entities %>" />
		<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.breadcrumbseparator %>" />
		<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.title.producttypes %>" />
	</h1>
	<aspdnsf:AlertMessage ID="ctlAlertMessage" runat="server" />
	
	<aspdnsf:FilteredListing runat="server"
		ID="FilteredListing"
		SqlQuery="SELECT {0} * FROM ProductType where {1}"
		SortExpression="Name">
		<ActionBarTemplate>
			<aspdnsf:LinkGroupEntities runat="server" SelectedLink='producttypes.aspx' />
			<asp:Button runat="server" CssClass="btn btn-primary" Text="<%$Tokens:StringResource, admin.common.update %>" OnClick="btnUpdate_Click" />
			<asp:HyperLink runat="server" CssClass="btn btn-action" Text="<%$Tokens:StringResource, admin.producttype.addnew %>" NavigateUrl="producttype.aspx" />
		</ActionBarTemplate>
		<Filters>
			<aspdnsf:StringFilter runat="server" Label="Name" FieldName="Name" />
		</Filters>
		<ListingTemplate>
			<div class="white-ui-box">
				<asp:GridView CssClass="table" ID="gMain" runat="server"
					DataSourceID="FilteredListingDataSource"
					PagerSettings-Position="TopAndBottom"
					AutoGenerateColumns="False"
					OnRowCommand="gMain_RowCommand"
					OnRowDataBound="gMain_RowDataBound"
					BorderStyle="None"
					BorderWidth="0px"
					CellPadding="0"
					GridLines="None"
					CellSpacing="-1"
					ShowFooter="True"
					AllowSorting="true">
					<EmptyDataTemplate>
						<div class="alert alert-info">
							<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.EmptyDataTemplate.NoResults %>" />
						</div>
					</EmptyDataTemplate>
					<Columns>
						<asp:TemplateField HeaderText="ID" SortExpression="ProductTypeID">
							<ItemTemplate>
								<asp:Literal runat="server" ID="ProductTypeID" Text='<%# DataBinder.Eval(Container.DataItem, "ProductTypeID") %>' />
							</ItemTemplate>
							<ItemStyle Width="5%" />
						</asp:TemplateField>

						<asp:TemplateField HeaderText="Product Type" SortExpression="Name">
							<ItemTemplate>
								<asp:HyperLink runat="server" NavigateUrl='<%# Eval("ProductTypeID", "producttype.aspx?typeid={0}")%>'>
										<%# CreateLinkText(DataBinder.Eval(Container.DataItem, "Name")) %>
								</asp:HyperLink>
							</ItemTemplate>
						</asp:TemplateField>

						<asp:TemplateField>
							<HeaderTemplate>
								<span class="text-danger">*</span><asp:Literal runat="server" Text="Display Order" />
							</HeaderTemplate>
							<ItemTemplate>
								<asp:TextBox ID="DisplayOrder" runat="Server" CssClass="form-control" Text='<%# DataBinder.Eval(Container.DataItem, "DisplayOrder") %>' ValidationGroup="DisplayOrder"></asp:TextBox>
								<asp:RequiredFieldValidator runat="server" ControlToValidate="DisplayOrder" ErrorMessage="*Required" ValidationGroup="DisplayOrder" Display="Dynamic" CssClass="text-danger" />
							</ItemTemplate>
							<ItemStyle Width="10%" />
							<HeaderStyle HorizontalAlign="Center" />
						</asp:TemplateField>

						<asp:TemplateField HeaderText="Delete">
							<ItemTemplate>
								<asp:LinkButton ID="Delete" ToolTip="Delete" CssClass="delete-link" CommandName="DeleteItem" CommandArgument='<%# Eval("ProductTypeID") %>' runat="Server">
									<i class="fa fa-times"></i> <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.Delete %>" />
								</asp:LinkButton>
							</ItemTemplate>
							<ItemStyle Width="5%" />
						</asp:TemplateField>
					</Columns>
					<FooterStyle CssClass="gridFooter" />
					<RowStyle CssClass="gridRow" />
					<EditRowStyle CssClass="gridEdit2" />
					<PagerStyle CssClass="tablepagerGrid" />
					<HeaderStyle CssClass="gridHeader" />
					<AlternatingRowStyle CssClass="gridAlternatingRow" BorderWidth="0px" />
				</asp:GridView>
			</div>
		</ListingTemplate>
	</aspdnsf:FilteredListing>
</asp:Content>