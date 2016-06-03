<%@ Page Language="C#" AutoEventWireup="true" Inherits="BulkEditPrices" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="bulkeditprices.aspx.cs" %>

<%@ Register TagPrefix="aspdnsf" Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls.Listing" %>
<%@ Register TagPrefix="aspdnsf" TagName="StringFilter" Src="Controls/Listing/StringFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="BooleanFilter" Src="Controls/Listing/BooleanFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="DataQueryFilter" Src="Controls/Listing/DataQueryFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="ListFilter" Src="Controls/Listing/ListFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="DecimalRangeFilter" Src="Controls/Listing/DecimalRangeFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="MultiFieldStringFilter" Src="Controls/Listing/MultiFieldStringFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="VariantEntityFilter" Src="Controls/Listing/VariantEntityFilter.ascx" %>

<asp:Content ContentPlaceHolderID="head" runat="server">
	<script type="text/javascript">
		var confirmBatchUpdate = "<asp:Literal runat="server" Text="<%$Tokens:EscapedStringResource, admin.bulkeditprices.ConfirmBatchUpdate, javascript %>" />";
		var confirmBatchClear = "<asp:Literal runat="server" Text="<%$Tokens:EscapedStringResource, admin.bulkeditprices.ConfirmBatchClear, javascript %>" />";

		jQuery(document).ready(function() {
			$('.bulk-save-prices')
				.on('click', function() { return confirm(confirmBatchUpdate); });

			$('.bulk-clear-prices')
				.on('click', function() { return confirm(confirmBatchClear); });
		});
	</script>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<h1>
		<i class="fa fa-tags"></i>
		<asp:Literal ID="litHeader" runat="server" Text="<%$Tokens:StringResource, admin.title.entityBulkPrices %>" />
	</h1>

	<aspdnsf:AlertMessage ID="AlertMessage" runat="server" />

	<div class="item-action-bar">
		<asp:CompareValidator runat="server" ControlToValidate="txtSalesDiscountPercentage" Operator="DataTypeCheck" Type="Double" ErrorMessage="<%$ Tokens:StringResource, admin.bulkeditprices.integerrequired%>" CssClass="text-danger" Display="Dynamic" />
		<asp:RangeValidator runat="server" ControlToValidate="txtSalesDiscountPercentage" MinimumValue="0" MaximumValue="100" Type="Double" ErrorMessage="<%$ Tokens:StringResource, admin.bulkeditprices.rangerequired%>" CssClass="text-danger" Display="Dynamic" />
		<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.bulkeditprices.setsalesdiscount%>" />
		<asp:TextBox ID="txtSalesDiscountPercentage" runat="server" CssClass="text-2 input-sm" />%
		<asp:Button ID="btnBulkSaveSalesPrice" runat="server" Text="<%$ Tokens:StringResource, admin.bulkeditprices.BulkSaveSalesPrice%>" CssClass="bulk-save-prices btn btn-primary" OnClick="btnBulkSaveSalesPrice_Click" />
		<asp:Button ID="btnBulkClearSalesPrice" runat="server" Text="<%$ Tokens:StringResource, admin.bulkeditprices.BulkClearSalesPrice%>" CssClass="bulk-clear-prices btn btn-primary" OnClick="btnBulkClearSalesPrice_Click" />
	</div>

	<aspdnsf:FilteredListing runat="server"
		ID="FilteredListing"
		LocaleSelectionEnabled="true"
		SqlQuery="select {0} v.ProductId, 
							v.VariantId, 
							coalesce(SelectedProductLocalization.LocalizedName, DefaultProductLocalization.Localizedname, UnspecifiedProductLocalization.LocalizedName) [ProductName], 
							coalesce(SelectedVariantLocalization.LocalizedName, DefaultVariantLocalization.Localizedname, UnspecifiedVariantLocalization.LocalizedName) [VariantName], 
							p.SKU,
							v.SKUSuffix,
							v.Price, 
							v.SalePrice, 
							v.Inventory, 
							v.Published 
					from ProductVariant v inner join Product p on p.ProductID = v.ProductID 
							left join (
								select ObjectId, LocalizedName
								from dbo.LocalizedObjectName
								where ObjectType = 'productvariant' and LocaleId = @_localeId
								) as SelectedVariantLocalization on v.VariantId = SelectedVariantLocalization.ObjectId
							left join (
								select ObjectId, LocalizedName
								from dbo.LocalizedObjectName
								where ObjectType = 'productvariant' and LocaleId = @_currentCustomerLocaleId
								) as DefaultVariantLocalization on v.VariantId = DefaultVariantLocalization.ObjectId
							left join (
								select ObjectId, LocalizedName
								from dbo.LocalizedObjectName
								where ObjectType = 'productvariant' and LocaleId is null
								) as UnspecifiedVariantLocalization on v.VariantId = UnspecifiedVariantLocalization.ObjectId 
							left join (
								select ObjectId, LocalizedName
								from dbo.LocalizedObjectName
								where ObjectType = 'product' and LocaleId = @_localeId
								) as SelectedProductLocalization on p.ProductId = SelectedProductLocalization.ObjectId
							left join (
								select ObjectId, LocalizedName
								from dbo.LocalizedObjectName
								where ObjectType = 'product' and LocaleId = @_currentCustomerLocaleId
								) as DefaultProductLocalization on p.ProductId = DefaultProductLocalization.ObjectId
							left join (
								select ObjectId, LocalizedName
								from dbo.LocalizedObjectName
								where ObjectType = 'product' and LocaleId is null
								) as UnspecifiedProductLocalization on p.ProductId = UnspecifiedProductLocalization.ObjectId 
					WHERE p.Deleted = 0 AND v.Deleted = 0 AND {1} "
		SortExpression="v.ProductId">
		<ActionBarTemplate>
			<asp:Button runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
		</ActionBarTemplate>
		<Filters>
			<aspdnsf:StringFilter runat="server"
				Label="<%$Tokens:StringResource, admin.common.ProductName %>"
				FieldName="coalesce(SelectedProductLocalization.LocalizedName, DefaultProductLocalization.Localizedname, UnspecifiedProductLocalization.LocalizedName)" />

			<aspdnsf:StringFilter runat="server"
				Label="<%$Tokens:StringResource, admin.common.VariantName %>"
				FieldName="coalesce(SelectedVariantLocalization.LocalizedName, DefaultVariantLocalization.Localizedname, UnspecifiedVariantLocalization.LocalizedName)" />
		</Filters>
		<ExpandableFilters>
			<aspdnsf:DecimalRangeFilter runat="server"
				StartLabel="<%$Tokens:StringResource, admin.common.MinimumPrice %>"
				EndLabel="<%$Tokens:StringResource, admin.common.MaximumPrice %>"
				FieldName="v.Price" />

			<aspdnsf:DecimalRangeFilter runat="server"
				StartLabel="<%$Tokens:StringResource, admin.common.MinimumSalePrice %>"
				EndLabel="<%$Tokens:StringResource, admin.common.MaximumSalePrice %>"
				FieldName="v.SalePrice" />

			<aspdnsf:DecimalRangeFilter runat="server"
				StartLabel="<%$Tokens:StringResource, admin.common.MinimumInventory%>"
				EndLabel="<%$Tokens:StringResource, admin.common.MaximumInventory %>"
				FieldName="v.Inventory" />

			<aspdnsf:VariantEntityFilter runat="server"
				Label="<%$Tokens:StringResource, admin.common.Category %>"
				EntityType="Category"
				VariantIdColumnName="v.VariantId" />

			<aspdnsf:VariantEntityFilter runat="server"
				Label="<%$Tokens:StringResource, admin.common.Manufacturer %>"
				EntityType="Manufacturer"
				VariantIdColumnName="v.VariantId" />

			<aspdnsf:VariantEntityFilter runat="server"
				Label="<%$Tokens:StringResource, admin.common.Section %>"
				EntityType="Section"
				VariantIdColumnName="v.VariantId" />

			<aspdnsf:VariantEntityFilter runat="server"
				Label="<%$Tokens:StringResource, admin.common.Distributor %>"
				EntityType="Distributor" 
				VariantIdColumnName="v.VariantId" />

			<aspdnsf:BooleanFilter runat="server"
				Label="<%$Tokens:StringResource, admin.common.Published%>"
				FieldName="v.Published"
				NullDisplayName="Any"
				DefaultValue="true" />
		</ExpandableFilters>
		<ListingTemplate>
			<div class="white-ui-box">
				<asp:Repeater ID="repeatMap" runat="server"
					DataSourceID="FilteredListingDataSource">
					<HeaderTemplate>
						<table class="table">
							<tr class="gridheader">
								<td>
									<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.ProductID%>" />
								</td>
								<td>
									<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.ProductName %>" />
								</td>
								<td style="text-align: center;">
									<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.VariantID%>" />
								</td>
								<td>
									<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.VariantName %>" />
								</td>
								<td>
									<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.VariantSKUSuffix %>" />
								</td>
								<td>
									<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.Price %>" />
								</td>
								<td>
									<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.SalePrice %>" />
								</td>
								<td>
									<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.Inventory %>" />
								</td>
								<td>
									<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.Published %>" />
								</td>
							</tr>
					</HeaderTemplate>
					<ItemTemplate>
						<tr class="itemrow">
							<td>
								<asp:Label runat="server" Text='<%# Eval("ProductId") %>' />
							</td>
							<td>
								<asp:HyperLink runat="server" Text='<%# CreateLinkText(Eval("ProductName")) %>' NavigateUrl='<%# "product.aspx?productid=" + Eval("ProductId") %>' />
							</td>
							<td style="text-align: center;">
								<asp:Label ID="lblVariantId" runat="server" Text='<%# Eval("VariantId") %>' />
							</td>
							<td>
								<asp:TextBox ID="txtVariantName" runat="server" Text='<%# Eval("VariantName") %>' CssClass="text-sm" />
							</td>
							<td>
								<asp:TextBox ID="txtVariantSKUSuffix" runat="server" Text='<%# Eval("SKUSuffix") %>' CssClass="text-sm" />
							</td>
							<td>
								<asp:TextBox ID="txtPrice" runat="server" Text='<%# Eval("Price") %>' CssClass="text-xs" />
								<asp:RequiredFieldValidator runat="server" ControlToValidate="txtPrice" ErrorMessage="*" CssClass="text-danger" Display="Dynamic" />
								<asp:CompareValidator runat="server" ControlToValidate="txtPrice" Operator="DataTypeCheck" Type="Double" ErrorMessage="<%$ Tokens:StringResource, admin.bulkeditprices.invalidformat %>" CssClass="text-danger" Display="Dynamic" />
							</td>
							<td>
								<asp:TextBox ID="txtSalePrice" runat="server" Text='<%# Eval("SalePrice") %>' CssClass="text-xs" />
								<asp:CompareValidator runat="server" ControlToValidate="txtSalePrice" Operator="DataTypeCheck" Type="Double" ErrorMessage="<%$ Tokens:StringResource, admin.bulkeditprices.invalidformat %>" CssClass="text-danger" Display="Dynamic" />
							</td>
							<td>
								<asp:TextBox ID="txtInventory" runat="server" Text='<%# Eval("Inventory") %>' CssClass="text-xs" />
								<asp:RequiredFieldValidator runat="server" ControlToValidate="txtInventory" ErrorMessage="*" CssClass="text-danger" Display="Dynamic" />
								<asp:CompareValidator runat="server" ControlToValidate="txtInventory" Operator="DataTypeCheck" Type="Integer" ErrorMessage="<%$ Tokens:StringResource, admin.bulkeditprices.invalidformat %>" CssClass="text-danger" Display="Dynamic" />
							</td>
							<td>
								<asp:CheckBox ID="chkPublished" runat="server" Checked='<%# (byte)Eval("Published") == 1 %>' />
							</td>
						</tr>
					</ItemTemplate>
					<FooterTemplate>
						</table>
					</FooterTemplate>
				</asp:Repeater>
			</div>
		</ListingTemplate>
	</aspdnsf:FilteredListing>
</asp:Content>
