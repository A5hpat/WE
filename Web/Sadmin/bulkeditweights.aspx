<%@ Page Language="C#" AutoEventWireup="true" Inherits="_bulkeditweights" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="bulkeditweights.aspx.cs" %>

<%@ Register TagPrefix="aspdnsf" Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls.Listing" %>
<%@ Register TagPrefix="aspdnsf" TagName="StringFilter" Src="Controls/Listing/StringFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="BooleanFilter" Src="Controls/Listing/BooleanFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="DecimalRangeFilter" Src="Controls/Listing/DecimalRangeFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="VariantEntityFilter" Src="Controls/Listing/VariantEntityFilter.ascx" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<h1>
		<i class="fa fa-cube"></i>
		<asp:Literal ID="litHeader" runat="server" Text="<%$Tokens:StringResource, admin.title.entityBulkWeight %>" />
	</h1>

	<aspdnsf:AlertMessage ID="AlertMessage" runat="server" />

	<div class="listing-action-row listing-action-row-upper">
		<div class="list-action-bar">
			<asp:Button runat="server"
				CssClass="btn btn-default"
				Text="Export All"
				OnClientClick="return confirm('This will export weight & dimension info for all product + variant combinations on your site.  This may take a while.  Continue?');"
				OnClick="btnExport_Click" />
			<asp:Button CausesValidation="true"
				runat="server"
				CssClass="btn btn-default"
				Text="Import"
				ValidationGroup="gImport" 
				OnClick="btnImport_Click"/>

			<asp:FileUpload ID="fuWeightImport" CssClass="inline-file-upload" AllowMultiple="false" runat="server" />
			<asp:RequiredFieldValidator ID="rfvImportFile" ControlToValidate="fuWeightImport" ErrorMessage="Please choose a file to import." runat="server" Display="Dynamic" ValidationGroup="gImport" />

			<asp:Button runat="server" Text="Save" CssClass="btn btn-primary" OnClick="Save" />
		</div>
	</div>

	<aspdnsf:FilteredListing runat="server"
		ID="FilteredListing"
		LocaleSelectionEnabled="true"
		SqlQuery="select {0} v.ProductId, 
							v.VariantId, 
							coalesce(SelectedProductLocalization.LocalizedName, DefaultProductLocalization.Localizedname, UnspecifiedProductLocalization.LocalizedName) [ProductName], 
							coalesce(SelectedVariantLocalization.LocalizedName, DefaultVariantLocalization.Localizedname, UnspecifiedVariantLocalization.LocalizedName) [VariantName], 
							p.SKU, 
							v.Weight, 
							v.Dimensions,
							v.IsShipSeparately 
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
					WHERE p.Deleted = 0 AND v.Deleted = 0 AND p.IsSystem = 0 AND {1} "
		SortExpression="v.ProductId">
		<Filters>
			<aspdnsf:StringFilter runat="server"
				Label="<%$Tokens:StringResource, admin.common.ProductName %>"
				FieldName="coalesce(SelectedProductLocalization.LocalizedName, DefaultProductLocalization.Localizedname, UnspecifiedProductLocalization.LocalizedName)" />

			<aspdnsf:StringFilter runat="server"
				Label="<%$Tokens:StringResource, admin.common.VariantName %>"
				FieldName="coalesce(SelectedVariantLocalization.LocalizedName, DefaultVariantLocalization.Localizedname, UnspecifiedVariantLocalization.LocalizedName)" />
			
			<aspdnsf:BooleanFilter runat="server"
				Label="<%$Tokens:StringResource, admin.common.IsShipSeparately %>"
				FieldName="v.IsShipSeparately"
				NullDisplayName="Any" />
		</Filters>
		<ExpandableFilters>
			<aspdnsf:DecimalRangeFilter runat="server"
				StartLabel="<%$Tokens:StringResource, admin.common.MinimumWeight %>"
				EndLabel="<%$Tokens:StringResource, admin.common.MaximumWeight %>"
				FieldName="v.Weight" />
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
									<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.Weight %>" />
								</td>
								<td>
									<asp:Literal runat="server" Text="Width" />
								</td>
								<td>
									<asp:Literal runat="server" Text="Height" />
								</td>
								<td>
									<asp:Literal runat="server" Text="Depth" />
								</td>
								<td style="text-align: center;">
									<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.IsShipSeparately %>" />
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
								<asp:HyperLink runat="server" Text='<%# CreateLinkText(Eval("VariantName").ToString()) %>' NavigateUrl='<%# "variant.aspx?variantid=" + Eval("VariantId") + "&productid=" + Eval("ProductId") %>' />
							</td>
							<td>
								<asp:TextBox ID="txtWeight" runat="server" Text='<%# Eval("Weight") is DBNull ? "0" : Eval("Weight") %>' CssClass="text-xs" /><br />
								<asp:RequiredFieldValidator runat="server" ControlToValidate="txtWeight" ErrorMessage="*" CssClass="text-danger" Display="Dynamic" />
								<asp:CompareValidator runat="server" ControlToValidate="txtWeight" Operator="DataTypeCheck" Type="Double" ErrorMessage="<%$ Tokens:StringResource, admin.bulkeditprices.invalidformat %>" CssClass="text-danger" Display="Dynamic" />
								<asp:RangeValidator runat="server" ControlToValidate="txtWeight" Type="Double" MinimumValue="0" ErrorMessage="<%$ Tokens:StringResource, admin.bulkeditprices.invalidformat %>" CssClass="text-danger" Display="Dynamic" />
							</td>
							<td>
								<asp:TextBox ID="txtWidth" runat="server" Text='<%# AspDotNetStorefrontCore.AppLogic.RetrieveProductDimension(Eval("Dimensions").ToString(), "width") %>' CssClass="text-xs" /><br />
								<asp:CompareValidator runat="server" ControlToValidate="txtWidth" Operator="DataTypeCheck" Type="Double" ErrorMessage="<%$ Tokens:StringResource, admin.bulkeditprices.invalidformat %>" CssClass="text-danger" Display="Dynamic" />
								<asp:RangeValidator runat="server" ControlToValidate="txtWidth" Type="Double" MinimumValue="0" ErrorMessage="<%$ Tokens:StringResource, admin.bulkeditprices.invalidformat %>" CssClass="text-danger" Display="Dynamic" />
							</td>
							<td>
								<asp:TextBox ID="txtHeight" runat="server" Text='<%# AspDotNetStorefrontCore.AppLogic.RetrieveProductDimension(Eval("Dimensions").ToString(), "height") %>' CssClass="text-xs" /><br />
								<asp:CompareValidator runat="server" ControlToValidate="txtHeight" Operator="DataTypeCheck" Type="Double" ErrorMessage="<%$ Tokens:StringResource, admin.bulkeditprices.invalidformat %>" CssClass="text-danger" Display="Dynamic" />
								<asp:RangeValidator runat="server" ControlToValidate="txtHeight" Type="Double" MinimumValue="0" ErrorMessage="<%$ Tokens:StringResource, admin.bulkeditprices.invalidformat %>" CssClass="text-danger" Display="Dynamic" />
							</td>
							<td>
								<asp:TextBox ID="txtDepth" runat="server" Text='<%# AspDotNetStorefrontCore.AppLogic.RetrieveProductDimension(Eval("Dimensions").ToString(), "depth") %>' CssClass="text-xs" /><br />
								<asp:CompareValidator runat="server" ControlToValidate="txtDepth" Operator="DataTypeCheck" Type="Double" ErrorMessage="<%$ Tokens:StringResource, admin.bulkeditprices.invalidformat %>" CssClass="text-danger" Display="Dynamic" />
								<asp:RangeValidator runat="server" ControlToValidate="txtDepth" Type="Double" MinimumValue="0" ErrorMessage="<%$ Tokens:StringResource, admin.bulkeditprices.invalidformat %>" CssClass="text-danger" Display="Dynamic" />
							</td>
							<td style="text-align: center;">
								<asp:CheckBox ID="chkIsShipSeparately" runat="server" Checked='<%# (byte)Eval("IsShipSeparately") == 1 %>' />
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
