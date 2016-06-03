<%@ Page Language="c#" Inherits="AspDotNetStorefrontAdmin.editextendedprices" EnableTheming="true" AutoEventWireup="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="editextendedprices.aspx.cs" %>

<%@ Import Namespace="AspDotNetStorefrontCore" %>

<%@ Register TagPrefix="listing" Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls.Listing" %>
<%@ Register TagPrefix="filter" TagName="MultiFieldStringFilter" Src="Controls/Listing/MultiFieldStringFilter.ascx" %>
<%@ Register TagPrefix="filter" TagName="StringFilter" Src="Controls/Listing/StringFilter.ascx" %>
<%@ Register TagPrefix="filter" TagName="BooleanFilter" Src="Controls/Listing/BooleanFilter.ascx" %>

<asp:Content ContentPlaceHolderID="bodyContentPlaceholder" runat="server">
	<aspdnsf:ReturnUrlTracker runat="server" ID="ReturnUrlTracker" DefaultReturnUrl="variants.aspx" />
	<asp:Panel ID="pnlEntityProducts" runat="server">
		<div class="main-breadcrumb">
			<span>
				<asp:HyperLink ID="lnkVariant" runat="server" />
			</span>
		</div>
		<h1>
			<i class="fa fa-cubes f-x3"></i>
			<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.title.editextendedprices %>" />
		</h1>

		<aspdnsf:AlertMessage ID="AlertMessageDisplay" runat="server" />

		<listing:FilteredListing runat="server"
			ID="FilteredListing"
			SqlQuery="
				select {0} 
					dbo.GetMlValue(CustomerLevel.Name, @_locale) [CustomerLevel.Name], 
					CustomerLevel.CustomerLevelID,
					ExtendedPrice.Price, 
					ExtendedPrice.VariantID, 
					ExtendedPrice.ExtendedPriceID
				from
					dbo.ProductCustomerLevel with(nolock)
					join dbo.CustomerLevel with(nolock)
						on ProductCustomerLevel.CustomerLevelID = CustomerLevel.CustomerLevelID
						and ProductCustomerLevel.ProductID = @productid
					left join dbo.ExtendedPrice with(nolock)
						on CustomerLevel.CustomerLevelID = ExtendedPrice.CustomerLevelID 
						and ExtendedPrice.VariantID = @variantid
						and CustomerLevel.Deleted = 0
				where {1}"
			SortExpression="dbo.GetMlValue(CustomerLevel.Name, @_locale)"
			LocaleSelectionEnabled="true">
			<SqlParameters>
				<asp:QueryStringParameter Name="VariantID" QueryStringField="variantid" />
				<asp:QueryStringParameter Name="ProductID" QueryStringField="productid" />
			</SqlParameters>
			<ActionBarTemplate>
				<asp:HyperLink runat="server" CssClass="btn btn-default" NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>" Text="<%$Tokens:StringResource, admin.common.close %>" />
				<asp:Button runat="server" CssClass="btn btn-default" OnClick="btnSaveAndClose_Click" ValidationGroup="Main" Text="<%$Tokens:StringResource, admin.common.saveandclose %>" NavigateUrl="products.aspx" />
				<asp:Button runat="server" CssClass="btn btn-primary" Text="<%$Tokens:StringResource, admin.common.Save %>" OnClick="btnUpdate_Click" />
			</ActionBarTemplate>
			<Filters>
				<filter:StringFilter runat="server"
					Label="<%$Tokens:StringResource, admin.product.NameSearch %>"
					FieldName="Name" />
			</Filters>
			<ListingTemplate>
				<div class="white-ui-box">
					<asp:GridView runat="server"
						ID="Grid"
						CssClass="table js-sortable-gridview"
						GridLines="None"
						DataSourceID="FilteredListingDataSource"
						AllowSorting="true"
						AutoGenerateColumns="false"
						DataKeyNames="CustomerLevelID, Price">
						<EmptyDataTemplate>
							<div class="alert alert-info">
								<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.EmptyDataTemplate.NoResults %>" />
							</div>
						</EmptyDataTemplate>

						<Columns>
							<asp:BoundField 
								HeaderText="<%$Tokens:StringResource, admin.editextendedprices.CustomerLevel %>"
								HeaderStyle-Width="90%"
								SortExpression="dbo.GetMlValue(CustomerLevel.Name, @_locale)" 
								DataField="CustomerLevel.Name" />

							<asp:TemplateField 
								HeaderText="<%$Tokens:StringResource, admin.editextendedprices.ExtendedPrice %>" 
								HeaderStyle-Width="10%"
								SortExpression="Price">
								<ItemTemplate>
									<asp:TextBox runat="server" 
										ID="txtExtendedPrice" 
										Text='<%# Localization.ParseNativeDecimal(Eval("Price").ToString()) %>' />
								</ItemTemplate>
							</asp:TemplateField>

						</Columns>
					</asp:GridView>
			</ListingTemplate>
		</listing:FilteredListing>
	</asp:Panel>
</asp:Content>
