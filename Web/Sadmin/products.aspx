<%@ Page Title="<%$Tokens:StringResource, admin.common.Products %>" Language="C#" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Theme="Admin_Default" Inherits="AspDotNetStorefrontAdmin.Products" Codebehind="products.aspx.cs" %>

<%@ Import Namespace="AspDotNetStorefrontCore" %>

<%@ Register TagPrefix="listing" Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls.Listing" %>
<%@ Register TagPrefix="filter" TagName="IntegerFilter" Src="Controls/Listing/IntegerFilter.ascx" %>
<%@ Register TagPrefix="filter" TagName="StringFilter" Src="Controls/Listing/StringFilter.ascx" %>
<%@ Register TagPrefix="filter" TagName="BooleanFilter" Src="Controls/Listing/BooleanFilter.ascx" %>
<%@ Register TagPrefix="filter" TagName="DataQueryFilter" Src="Controls/Listing/DataQueryFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="LinkGroupProducts" Src="Controls/LinkGroupProducts.ascx" %>

<asp:Content ContentPlaceHolderID="head" runat="server">

	<script type="text/javascript">
		var deleteConfirmPrompt = "<asp:Literal runat="server" Text="<%$Tokens:EscapedStringResource, admin.productgrid.DeleteConfirm, javascript %>" />";
		var undeleteConfirmPrompt = "<asp:Literal runat="server" Text="<%$Tokens:EscapedStringResource, admin.productgrid.UnDeleteConfirm, javascript %>" />";

		jQuery(document).ready(function() {
			$('.confirm-delete > input[type=checkbox]')
				.on('change', null, deleteConfirmPrompt, FilteredListing.checkboxConfirmAndPostback);

			$('.confirm-undelete > input[type=checkbox]')
				.on('change', null, undeleteConfirmPrompt, FilteredListing.checkboxConfirmAndPostback);
		});
	</script>

</asp:Content>

<asp:Content ContentPlaceHolderID="bodyContentPlaceholder" runat="Server">
	<h1>
		<i class="fa fa-cube"></i>
		<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.Products %>" />
		<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.breadcrumbseparator %>" />
		<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.productmgr %>" />
	</h1>
	<aspdnsf:AlertMessage runat="server" ID="AlertMessageDisplay" />

	<listing:FilteredListing runat="server"
				ID="FilteredListing"
				SqlQuery="
					select
						{0}
						Product.ProductID, 
						coalesce(SelectedLocalization.LocalizedName, DefaultLocalization.Localizedname, UnspecifiedLocalization.LocalizedName) Name,
						Product.ProductTypeID, 
						Product.SKU, 
						Product.Published, 
						Product.Deleted 
					from dbo.Product Product with(nolock) 
						left join (
							select ObjectId, LocalizedName
							from dbo.LocalizedObjectName
							where ObjectType = 'product' and LocaleId = @_localeId
							) as SelectedLocalization on Product.ProductID = SelectedLocalization.ObjectId
						left join (
							select ObjectId, LocalizedName
							from dbo.LocalizedObjectName
							where ObjectType = 'product' and LocaleId = @_currentCustomerLocaleId
							) as DefaultLocalization on Product.ProductID = DefaultLocalization.ObjectId
						left join (
							select ObjectId, LocalizedName
							from dbo.LocalizedObjectName
							where ObjectType = 'product' and LocaleId is null
							) as UnspecifiedLocalization on Product.ProductID = UnspecifiedLocalization.ObjectId 
					where {1}"
				SortExpression="coalesce(SelectedLocalization.LocalizedName, DefaultLocalization.Localizedname, UnspecifiedLocalization.LocalizedName)"
				LocaleSelectionEnabled="true">
				<ActionBarTemplate>
					<aspdnsf:LinkGroupProducts runat="server" ID="LinkGroupProducts" SelectedLink="products.aspx" />
					<asp:HyperLink runat="server"
						CssClass="btn btn-action"
						NavigateUrl="product.aspx"
						Text="<%$ Tokens: StringResource, common.cs.89 %>" />
				</ActionBarTemplate>
				<Filters>
					<filter:IntegerFilter runat="server"
						Label="<%$Tokens:StringResource, admin.product.IdSearch %>"
						FieldName="ProductID"
						QueryStringNames="filter.id" />

					<filter:StringFilter runat="server"
						Label="<%$Tokens:StringResource, admin.product.NameSearch %>"
						FieldName="coalesce(SelectedLocalization.LocalizedName, DefaultLocalization.Localizedname, UnspecifiedLocalization.LocalizedName)" />

					<filter:StringFilter runat="server"
						Label="<%$Tokens:StringResource, admin.product.SkuSearch %>"
						FieldName="SKU" />

					<filter:BooleanFilter runat="server"
						Label="Deleted"
						FieldName="Deleted"
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
							DataKeyNames="ProductID"
							OnRowCommand="DispatchGridCommand">
							<Columns>

								<asp:HyperLinkField
									HeaderText="<%$ Tokens: StringResource, admin.common.ID %>"
									HeaderStyle-Width="7%"
									SortExpression="ProductID"
									DataTextField="ProductID" 
									DataNavigateUrlFields="ProductID"
									DataNavigateUrlFormatString="product.aspx?productid={0}" />

								<listing:TooltipHyperLinkField
									HeaderText="<%$ Tokens: StringResource, admin.product.NameSearch %>"
									SortExpression="coalesce(SelectedLocalization.LocalizedName, DefaultLocalization.Localizedname, UnspecifiedLocalization.LocalizedName)"
									ToolTip="<%$ Tokens: StringResource, admin.productgrid.NameTooltip %>"
									DataTextField="Name"
									DataNavigateUrlFields="ProductID"
									DataNavigateUrlFormatString="product.aspx?productid={0}"
									Text="<%$Tokens:StringResource, admin.nolinktext %>" />

								<asp:BoundField
									HeaderText="<%$ Tokens: StringResource, admin.product.SkuSearch %>"
									HeaderStyle-Width="10%"
									SortExpression="SKU"
									DataField="SKU" />

								<asp:TemplateField
									HeaderText="<%$ Tokens: StringResource, admin.common.Published %>"
									HeaderStyle-Width="8%">
									<ItemTemplate>
										<aspdnsf:CommandCheckBox runat="server"
											ToolTip='<%# (byte)DataBinder.Eval(Container.DataItem, "Published") == 1 ? AppLogic.GetString("admin.productgrid.UnpublishTooltip", AppLogic.GetCurrentCustomer().LocaleSetting) : AppLogic.GetString("admin.productgrid.PublishTooltip", AppLogic.GetCurrentCustomer().LocaleSetting) %>'
											CheckedCommandName="<%# PublishProductCommand %>"
											UncheckedCommandName="<%# UnpublishProductCommand %>"
											CommandArgument='<%# DataBinder.Eval(Container.DataItem, "ProductID") %>'
											AutoPostBack="true"
											Checked='<%# (byte)DataBinder.Eval(Container.DataItem, "Published") == 1 %>' />
									</ItemTemplate>
								</asp:TemplateField>

								<asp:TemplateField
									HeaderText="<%$ Tokens: StringResource, admin.common.Deleted %>"
									HeaderStyle-Width="8%">
									<ItemTemplate>
										<aspdnsf:CommandCheckBox runat="server"
											CssClass='<%# (byte)DataBinder.Eval(Container.DataItem, "Deleted") == 1 ? "confirm-undelete" : "confirm-delete" %>'
											ToolTip='<%# (byte)DataBinder.Eval(Container.DataItem, "Deleted") == 1 ? AppLogic.GetString("admin.productgrid.UnDeleteTooltip", AppLogic.GetCurrentCustomer().LocaleSetting) : AppLogic.GetString("admin.productgrid.DeleteTooltip", AppLogic.GetCurrentCustomer().LocaleSetting) %>'
											CheckedCommandName="<%# DeleteProductCommand %>"
											UncheckedCommandName="<%# UndeleteProductCommand %>"
											CommandArgument='<%# DataBinder.Eval(Container.DataItem, "ProductID") %>'
											Checked='<%# (byte)DataBinder.Eval(Container.DataItem, "Deleted") == 1 %>' />
									</ItemTemplate>
								</asp:TemplateField>

								<asp:TemplateField
									HeaderText="<%$ Tokens: StringResource, admin.common.Clone %>"
									HeaderStyle-Width="8%">
									<ItemTemplate>
										<asp:LinkButton runat="server"
											CssClass="clone-link fa-copy"
											ToolTip="<%$ Tokens: StringResource, admin.productgrid.CloneTooltip %>"
											CommandName="<%# CloneProductCommand %>"
											CommandArgument='<%# DataBinder.Eval(Container.DataItem, "ProductID") %>'
											Text="<%$ Tokens: StringResource, admin.common.Clone %>" />
									</ItemTemplate>
								</asp:TemplateField>

								<listing:TooltipHyperLinkField
									HeaderText="<%$ Tokens: StringResource, admin.common.Variants %>"
									HeaderStyle-Width="8%"
									ToolTip="<%$ Tokens: StringResource, admin.productgrid.AddEditVariantTooltip %>"
									ControlStyle-CssClass="action-link fa-cubes"
									Text="<%$ Tokens: StringResource, admin.common.Variants %>"
									DataNavigateUrlFields="ProductID"
									DataNavigateUrlFormatString="variants.aspx?productid={0}" />

							</Columns>
							<EmptyDataTemplate>
								<div class="alert alert-info">
									<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.EmptyDataTemplate.NoResults %>" />
								</div>
							</EmptyDataTemplate>
						</asp:GridView>
					</div>
				</ListingTemplate>
			</listing:FilteredListing>
		
</asp:Content>
