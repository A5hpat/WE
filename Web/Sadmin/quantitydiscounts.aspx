<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.quantitydiscounts" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="quantitydiscounts.aspx.cs" %>

<%@ Register TagPrefix="aspdnsf" Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls.Listing" %>
<%@ Register TagPrefix="aspdnsf" TagName="StringFilter" Src="Controls/Listing/StringFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="ListFilter" Src="Controls/Listing/ListFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="LinkGroupProducts" Src="Controls/LinkGroupProducts.ascx" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<h1>
		<i class="fa fa-cube"></i>
		<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.Products %>" />
		<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.breadcrumbseparator %>" />
		<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.quantitydiscounts %>" />
	</h1>
	<aspdnsf:AlertMessage runat="server" ID="AlertMessage" />

	<aspdnsf:FilteredListing runat="server"
		ID="FilteredListing"
		SqlQuery="select {0} * from QuantityDiscount with(nolock) where {1}"
		SortExpression="Name"
		SortDirection="Descending">
		<ActionBarTemplate>
			<aspdnsf:LinkGroupProducts runat="server" ID="LinkGroupProducts" SelectedLink="quantitydiscounts.aspx" />
			<asp:HyperLink runat="server" CssClass="btn btn-action" Text="<%$ Tokens:StringResource, admin.quantitydiscount.Create %>" NavigateUrl="quantitydiscount.aspx" />
		</ActionBarTemplate>
		<Filters>
			<aspdnsf:StringFilter runat="server" Label="Name" FieldName="Name" />
			<aspdnsf:ListFilter runat="server"
				Label="Discount Type"
				FieldName="DiscountType">
				<Items>
					<asp:ListItem Value="0" Text="Percent" />
					<asp:ListItem Value="1" Text="Fixed Amount" />
				</Items>
			</aspdnsf:ListFilter>
		</Filters>
		<ListingTemplate>
			<div class="white-ui-box">
				<asp:GridView CssClass="table" ID="gMain" runat="server"
					DataSourceID="FilteredListingDataSource"
					PagerSettings-Position="TopAndBottom"
					AutoGenerateColumns="False"
					OnRowDataBound="gMain_RowDataBound"
					OnRowCommand="gMain_RowCommand"
					BorderStyle="None"
					BorderWidth="0px"
					CellPadding="0"
					GridLines="None"
					CellSpacing="-1"
					ShowFooter="True">
					<EmptyDataTemplate>
						<div class="alert alert-info">
							<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.EmptyDataTemplate.NoResults %>" />
						</div>
					</EmptyDataTemplate>
					<Columns>
						<asp:TemplateField HeaderText="ID">
							<ItemTemplate>
								<asp:Literal runat="server" ID="QuantityDiscountID" Text='<%# DataBinder.Eval(Container.DataItem, "QuantityDiscountID") %>' />
							</ItemTemplate>
							<ItemStyle Width="5%" />
						</asp:TemplateField>

						<asp:HyperLinkField
							HeaderText="Name"
							DataNavigateUrlFields="QuantityDiscountID"
							DataNavigateUrlFormatString="quantitydiscount.aspx?discountid={0}"
							DataTextField="Name"
							Text="<%$Tokens:StringResource, admin.nolinktext %>" />

						<asp:TemplateField HeaderText="Discount Type">
							<ItemTemplate>
								<asp:Literal runat="server" ID="litDiscountType" Text='<%# DataBinder.Eval(Container.DataItem, "DiscountType").ToString() == "1" ? "Fixed Amount" : "Percent" %>' />
							</ItemTemplate>
						</asp:TemplateField>

						<asp:TemplateField HeaderText="Created On">
							<ItemTemplate>
								<asp:Literal runat="server" ID="CreatedOn" Text='<%# DataBinder.Eval(Container.DataItem, "CreatedOn") %>' />
							</ItemTemplate>
						</asp:TemplateField>

						<asp:TemplateField HeaderText="Display Order">
							<ItemTemplate>
								<asp:Literal runat="server" ID="DisplayOrder" Text='<%# DataBinder.Eval(Container.DataItem, "DisplayOrder") %>' />
							</ItemTemplate>
						</asp:TemplateField>

						<asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.common.Delete %>">
							<ItemTemplate>
								<asp:LinkButton ID="lnkDelete" CommandName="DeleteItem" CommandArgument='<%# Eval("QuantityDiscountID") %>' runat="Server" Text='<i class="fa fa-times"></i> Delete' CssClass="delete-link" />
							</ItemTemplate>
							<ItemStyle CssClass="selectData" Width="5%" />
						</asp:TemplateField>
					</Columns>
				</asp:GridView>
			</div>
		</ListingTemplate>
	</aspdnsf:FilteredListing>
</asp:Content>
