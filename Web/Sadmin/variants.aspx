<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.Variants" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="variants.aspx.cs" %>

<%@ Import Namespace="AspDotNetStorefrontCore" %>

<%@ Register TagPrefix="listing" Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls.Listing" %>
<%@ Register TagPrefix="filter" TagName="MultiFieldStringFilter" Src="Controls/Listing/MultiFieldStringFilter.ascx" %>
<%@ Register TagPrefix="filter" TagName="StringFilter" Src="Controls/Listing/StringFilter.ascx" %>
<%@ Register TagPrefix="filter" TagName="BooleanFilter" Src="Controls/Listing/BooleanFilter.ascx" %>

<asp:Content ContentPlaceHolderID="head" runat="server">

	<script type="text/javascript">
		var deleteConfirmPrompt = "<asp:Literal runat="server" Text="<%$ Tokens:EscapedStringResource, admin.common.ConfirmDeletion, javascript %>" />";
		var undeleteConfirmPrompt = "<asp:Literal runat="server" Text="<%$ Tokens:EscapedStringResource, admin.common.ConfirmUnDeletion, javascript %>" />";

		jQuery(document).ready(function() {
			$('.confirm-delete > input[type=checkbox]')
				.on('change', null, deleteConfirmPrompt, FilteredListing.checkboxConfirmAndPostback);

			$('.confirm-undelete > input[type=checkbox]')
				.on('change', null, undeleteConfirmPrompt, FilteredListing.checkboxConfirmAndPostback);
		});
	</script>
</asp:Content>

<asp:Content ContentPlaceHolderID="bodyContentPlaceholder" runat="server">
	<aspdnsf:ReturnUrlTracker runat="server" ID="ReturnUrlTracker" DefaultReturnUrl="products.aspx" Blacklist="Variant.aspx,variantsizecolorinventory.aspx" />
	<asp:Panel ID="pnlEntityProducts" runat="server">
		<div class="main-breadcrumb">
			<asp:HyperLink runat="server" ID="lnkProduct" />
			<span>Managing Variants</span>
		</div>
		<h1>
			<i class="fa fa-cubes f-x3"></i>
			<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.Variants %>" />
		</h1>

		<aspdnsf:AlertMessage ID="AlertMessageDisplay" runat="server" />

		<listing:FilteredListing runat="server"
			ID="FilteredListing"
			SqlQuery="select {0} pv.VariantID, pv.ProductID, dbo.GetMlValue(pv.Name, @_locale) Name, p.TrackInventoryBySizeAndColor, pv.Inventory, pv.DisplayOrder, cast(pv.IsDefault as bit) IsDefault, cast(pv.Deleted as bit) Deleted, pv.Published from dbo.ProductVariant pv with(nolock) left join dbo.Product p on pv.ProductID = p.ProductID where pv.ProductID = @productId and {1}"
			SortExpression="VariantID"
			LocaleSelectionEnabled="true">
			<ActionBarTemplate>
				<asp:HyperLink runat="server"
					CssClass="btn btn-default"
					NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>"
					Text="<%$Tokens:StringResource, admin.common.close %>" />
				<asp:Button runat="server"
					ID="btnAdd"
					CssClass="btn btn-action"
					Text="<%$ Tokens:StringResource, admin.common.AddNew %>"
					OnClick="btnAdd_Click"
					Enabled='<%# ProductId > 0 %>' />
				<asp:Button runat="server"
					ID="btnUpdate"
					CssClass="btn btn-primary"
					Text="<%$ Tokens:StringResource, admin.common.Save %>"
					OnClick="btnUpdate_Click"
					Enabled='<%# ProductId > 0 %>' />
			</ActionBarTemplate>
			<SqlParameters>
				<asp:QueryStringParameter Name="productId" QueryStringField="productId" />
			</SqlParameters>
			<Filters>

				<filter:StringFilter runat="server"
					Label="<%$ Tokens:StringResource, admin.product.NameSearch %>"
					FieldName="pv.Name" />

				<filter:BooleanFilter runat="server"
					Label="Deleted"
					FieldName="pv.Deleted"
					NullDisplayName="Any"
					DefaultValue="False" />

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
						DataKeyNames="VariantID, Name"
						OnRowCommand="DispatchGridCommand">
						<EmptyDataTemplate>
							<div class="alert alert-info">
								<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.EmptyDataTemplate.NoResults %>" />
							</div>
						</EmptyDataTemplate>
						<Columns>

							<asp:TemplateField>
								<ItemTemplate>
									<asp:HiddenField ID="VariantID" runat="server" Value='<%# Eval("VariantID") %>' />
								</ItemTemplate>
							</asp:TemplateField>

							<asp:HyperLinkField
								HeaderText="<%$ Tokens: StringResource, admin.common.ID %>"
								HeaderStyle-Width="10%"
								SortExpression="VariantID" 
								DataTextField="VariantID"
								DataNavigateUrlFields="ProductID,VariantID"
								DataNavigateUrlFormatString="variant.aspx?productid={0}&amp;variantid={1}" />

							<asp:HyperLinkField
								HeaderText="<%$ Tokens:StringResource, admin.common.Variant %>"
								SortExpression="dbo.GetMlValue(pv.Name, @_locale)"
								DataTextField="Name"
								DataNavigateUrlFields="ProductID,VariantID"
								DataNavigateUrlFormatString="variant.aspx?productid={0}&amp;variantid={1}"
								Text="<%$Tokens:StringResource, admin.nolinktext %>"
								/>

							<asp:TemplateField
								HeaderText="<%$ Tokens:StringResource, admin.common.Inventory %>"
								HeaderStyle-Width="10%">
								<ItemTemplate>
									<asp:HyperLink runat="server"
										NavigateUrl='<%# "variantsizecolorinventory.aspx?productid=" + Eval("ProductId") + "&amp;variantid=" + Eval("VariantId") %>'
										Visible='<%# (byte)Eval("TrackInventoryBySizeAndColor") == 1 %>'>
										manage
									</asp:HyperLink>
									<asp:Label runat="server"
										Visible='<%# (byte)Eval("TrackInventoryBySizeAndColor") == 0 %>'>
										<%#Eval("Inventory")%>
									</asp:Label>
								</ItemTemplate>
							</asp:TemplateField>

							<asp:TemplateField
								HeaderText="<%$ Tokens:StringResource, admin.common.DisplayOrderHeader %>"
								HeaderStyle-Width="10%"
								SortExpression="DisplayOrder">
								<ItemTemplate>
									<asp:TextBox runat="server"
										ID="txtDisplayOrder"
										ToolTip='<%#  AppLogic.GetString("admin.common.EnterDisplayOrder", ThisCustomer.LocaleSetting)  %>'
										Name='<%# "DisplayOrder_" + Eval("VariantID").ToString() %>'
										Text='<%# Localization.ParseNativeInt(Eval("DisplayOrder").ToString()) %>' />
								</ItemTemplate>
							</asp:TemplateField>

							<asp:TemplateField
								HeaderText="<%$ Tokens:StringResource, admin.common.IsDefaultVariant %>"
								HeaderStyle-Width="15%">
								<ItemTemplate>
									<aspdnsf:CommandRadioButton runat="server"
										ID="rbDefaultVariant"
										GroupName="rbGroupDefaultVariant"
										AutoPostBack="true"
										Checked='<%# Eval("IsDefault") %>'
										CommandName="<%# SetDefaultVariantCommand %>"
										CommandArgument='<%# Eval("VariantID") %>' />
								</ItemTemplate>
							</asp:TemplateField>

							<asp:TemplateField
								HeaderText="<%$ Tokens: StringResource, admin.common.Deleted %>"
								HeaderStyle-Width="8%">
								<ItemTemplate>
									<aspdnsf:CommandCheckBox runat="server"
										CssClass='<%# (bool)Eval("Deleted") ? "confirm-undelete" : "confirm-delete" %>'
										CheckedCommandName="<%# DeleteVariantCommand %>"
										UncheckedCommandName="<%# UndeleteVariantCommand %>"
										CommandArgument='<%# Eval("VariantID") %>'
										Checked='<%# Eval("Deleted") %>' />
								</ItemTemplate>
							</asp:TemplateField>

							<asp:TemplateField
								HeaderText="<%$ Tokens:StringResource, admin.entityProducts.Clone %>"
								HeaderStyle-Width="10%">
								<ItemTemplate>
									<asp:LinkButton runat="Server"
										CssClass="clone-link fa-copy"
										ToolTip="<%$ Tokens: StringResource, admin.product.variantgrid.CloneTooltip %>"
										CommandName='<%# CloneVariantCommand %>'
										CommandArgument='<%# Eval("VariantID") %>'
										Text="<%$ Tokens: StringResource, admin.common.Clone %>" />
								</ItemTemplate>
							</asp:TemplateField>

						</Columns>
					</asp:GridView>
				</div>
			</ListingTemplate>
		</listing:FilteredListing>
	</asp:Panel>
</asp:Content>
