<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.KitVariantItemSelector" MasterPageFile="~/App_Templates/Admin_Default/Popup.master" Codebehind="kitvariantitemselector.aspx.cs" %>

<%@ Import Namespace="AspDotNetStorefrontCore" %>
<%@ Register TagPrefix="filter" TagName="StringFilter" Src="Controls/Listing/StringFilter.ascx" %>
<%@ Register TagPrefix="filter" TagName="BooleanFilter" Src="Controls/Listing/BooleanFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls.Listing" %>

<asp:Content runat="server" ContentPlaceHolderID="head">
	<script type="text/javascript" src="Scripts/core.js"></script>

	<script type="text/javascript">
		function CloseAndUpdate(data) {

			GetRadWindow().Close();
			window.parent.aspdnsf.Pages.EditKit.pushData(data);
		}

		function GetRadWindow() {
			var oWindow = null;
			if (window.radWindow) oWindow = window.radWindow; //Will work in Moz in all cases, including clasic dialog
			else if (window.frameElement.radWindow) oWindow = window.frameElement.radWindow; //IE (and Moz as well)

			return oWindow;
		}
	</script>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<aspdnsf:FilteredListing runat="server"
		ID="FilteredListing"
		LocaleSelectionEnabled="true"
		SqlQuery="select {0} v.ProductId, 
							v.VariantId, 
							coalesce(SelectedProductLocalization.LocalizedName, DefaultProductLocalization.Localizedname, UnspecifiedProductLocalization.LocalizedName) [ProductName], 
							isnull(nullif(coalesce(SelectedVariantLocalization.LocalizedName, DefaultVariantLocalization.Localizedname, UnspecifiedVariantLocalization.LocalizedName), ''), '(Unnamed Variant)') [VariantName], 
							p.SKU, 
							v.Price, 
							v.SalePrice, 
							v.Weight, 
							v.Inventory, 
							v.Published, 
							v.Deleted, 
							v.IsDefault, 
							v.Description 
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
				 WHERE {1}">
		<Filters>
			<filter:StringFilter runat="server"
				Label="<%$Tokens:StringResource, admin.common.ProductName %>"
				FieldName="coalesce(SelectedProductLocalization.LocalizedName, DefaultProductLocalization.Localizedname, UnspecifiedProductLocalization.LocalizedName)" />
			<filter:StringFilter runat="server"
				Label="<%$Tokens:StringResource, admin.common.VariantName %>"
				FieldName="coalesce(SelectedVariantLocalization.LocalizedName, DefaultVariantLocalization.Localizedname, UnspecifiedVariantLocalization.LocalizedName)" />
			<filter:BooleanFilter runat="server"
					Label="Published"
					FieldName="v.Published"
					NullDisplayName="Any"
					DefaultValue="True" />
			<filter:BooleanFilter runat="server"
					Label="Deleted"
					FieldName="v.Deleted"
					NullDisplayName="Any"
					DefaultValue="False" />
		</Filters>
		<ListingTemplate>
			<div class="white-ui-box">
				<asp:Repeater ID="rptVariants" runat="server"
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
									<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.Price %>" />
								</td>
								<td>
									<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.SalePrice %>" />
								</td>
								<td>
									<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.Weight %>" />
								</td>
								<td>
									<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.Inventory %>" />
								</td>
							</tr>
					</HeaderTemplate>
					<ItemTemplate>
						<tr class="itemrow">
							<td>
								<asp:Label runat="server" Text='<%# Eval("ProductId") %>' />
							</td>
							<td>
								<asp:Label runat="server" Text='<%# CreateLinkText(Eval("ProductName")) %>' />
							</td>
							<td style="text-align: center;">
								<asp:Label ID="lblVariantId" runat="server" Text='<%# Eval("VariantId") %>' />
							</td>
							<td>
								<%-- Generate JSON to maintain compatibility with existing editkit page without full rewrite --%>
								<a href="javascript:void(0)"
									runat="server"
									onclick='<%# string.Format("CloseAndUpdate({0})", Newtonsoft.Json.Linq.JObject.FromObject(new { 
										Id = Eval("VariantId"), 
										ProductId = Eval("ProductId"),
										Name =  XmlCommon.GetLocaleEntry(Eval("VariantName").ToString(), ThisCustomer.LocaleSetting, true),
										Description = XmlCommon.GetLocaleEntry(Eval("Description").ToString(), ThisCustomer.LocaleSetting, true),
										IsPublished = Convert.ToBoolean(Eval("Published")),
										IsDeleted = Convert.ToBoolean(Eval("Deleted")),
										IsDefault = Convert.ToBoolean(Eval("IsDefault")),
										InventoryCount = Eval("Inventory"),
										Price = Convert.ToString(Eval("Price")) == String.Empty ? "0" : Eval("Price").ToString(),
										SalePrice = Convert.ToString(Eval("SalePrice")) == String.Empty ? "0" : Eval("SalePrice").ToString(),
										Weight = Convert.ToString(Eval("Weight")) == String.Empty ? "0" : Eval("Weight").ToString()
									})) 
								%>'><%# Eval("VariantName") %></a>
							</td>
							<td>
								<asp:Label runat="server" Text='<%# Eval("Price") %>' />
							</td>
							<td>
								<asp:Label runat="server" Text='<%# Eval("SalePrice") %>' />
							</td>
							<td>
								<asp:Label runat="server" Text='<%# Eval("Weight") %>' />
							</td>
							<td>
								<asp:Label runat="server" Text='<%# Eval("Inventory") %>' />
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
