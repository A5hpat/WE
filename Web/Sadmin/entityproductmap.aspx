<%@ Page Language="C#" Inherits="AspDotNetStorefrontAdmin.EntityProductMap" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Theme="Admin_Default" Codebehind="entityproductmap.aspx.cs" %>

<%@ Register TagPrefix="aspdnsf" Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls" %>
<%@ Register TagPrefix="listing" Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls.Listing" %>
<%@ Register TagPrefix="filter" TagName="FlagFilter" Src="Controls/Listing/FlagFilter.ascx" %>
<%@ Register TagPrefix="filter" TagName="IntegerFilter" Src="Controls/Listing/IntegerFilter.ascx" %>
<%@ Register TagPrefix="filter" TagName="StringFilter" Src="Controls/Listing/StringFilter.ascx" %>

<asp:Content runat="server" ContentPlaceHolderID="head">
	<script type="text/javascript">
		$(document).ready(function() {
			$('button.js-select-all').click(function() {
				$('.js-selectable > *').filter(':checkbox').prop('checked', true);
			});
			$('button.js-clear-all').click(function() {
				$('.js-selectable > *').filter(':checkbox').prop('checked', false);
			});
		});
	</script>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<div class="main-breadcrumb">
		<asp:Literal runat="server" ID="EntityBreadcrumb" />
	</div>

	<h1 runat="server" id="header">
		<i class="fa fa-arrows-h"></i>
		<asp:Literal ID="HeaderText" runat="server" />
	</h1>

	<aspdnsf:ReturnUrlTracker runat="server" ID="ReturnUrlTracker" DefaultReturnUrl="Entities.aspx" Blacklist="Product.aspx" />
	<aspdnsf:AlertMessage runat="server" ID="AlertMessageControl" />

	<listing:FilteredListing runat="server"
		ID="FilteredListing"
		SqlQuery=
			"select 
				{0}
				Product.ProductID, 
				coalesce(SelectedLocalization.LocalizedName, DefaultLocalization.Localizedname, UnspecifiedLocalization.LocalizedName) Name,
				case when ProductEntity.EntityID is not null then 1 else 0 end as IsMapped
			from 
				Product with(nolock) 
				left join ProductEntity on
					ProductEntity.EntityType = @entityType
					and ProductEntity.EntityID = @entityId
					and ProductEntity.ProductID = Product.ProductID
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
			where
				Product.Deleted = 0
				and (@ShowUnmapped = 1 or ProductEntity.EntityID is not null)
				and {1}"
		SortExpression="coalesce(SelectedLocalization.LocalizedName, DefaultLocalization.Localizedname, UnspecifiedLocalization.LocalizedName)"
		LocaleSelectionEnabled="true">

		<SqlParameters>
			<asp:QueryStringParameter Name="entityType" QueryStringField="entityType" />
			<asp:QueryStringParameter Name="entityId" QueryStringField="entityId" />
		</SqlParameters>

		<ActionBarTemplate>
			<button type="button" class="btn btn-default js-select-all">Select All</button>
			<button type="button" class="btn btn-default js-clear-all">Clear All</button>
			<asp:HyperLink runat="server"
				CssClass="btn btn-default"
				NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>"
				Text="<%$Tokens:StringResource, admin.common.close %>" />
			<asp:Button runat="server" 
				CssClass="btn btn-default" 
				OnClick="SaveAndClose_Click" 
				Text="<%$Tokens:StringResource, admin.common.SaveAndClose %>" />
			<asp:Button runat="server" 
				CssClass="btn btn-primary" 
				OnClick="Save_Click" 
				Text="<%$Tokens:StringResource, admin.common.Save %>" />
		</ActionBarTemplate>

		<Filters>
			<filter:IntegerFilter runat="server"
				Label="<%$Tokens:StringResource, admin.product.IdSearch %>"
				FieldName="Product.ProductID"
				QueryStringNames="filter.id" />

			<filter:StringFilter runat="server"
				Label="<%$Tokens:StringResource, admin.product.NameSearch %>"
				FieldName="coalesce(SelectedLocalization.LocalizedName, DefaultLocalization.Localizedname, UnspecifiedLocalization.LocalizedName)" 
				QueryStringNames="filter.name" />

			<filter:StringFilter runat="server"
				Label="<%$Tokens:StringResource, admin.product.SkuSearch %>"
				FieldName="SKU"
				QueryStringNames="filter.sku" />

			<filter:FlagFilter runat="server" 
				Label="Show Unmapped Products" 
				QueryStringNames="filter.unmapped"
				SqlParameterNames="ShowUnmapped"
				Default="false" />

		</Filters>

		<ListingTemplate>
			<div class="white-ui-box">
				<asp:GridView runat="server"
					ID="grdMap"
					DataSourceID="FilteredListingDataSource"
					CssClass="table js-sortable-gridview"
					GridLines="None"
					AutoGenerateColumns="false"
					AllowSorting="true"
					DataKeyNames="ProductID, IsMapped">
					<Columns>

						<asp:TemplateField 
							HeaderText="Map" 
							ItemStyle-Width="25px" 
							ItemStyle-HorizontalAlign="Center">
							<ItemTemplate>
								<asp:CheckBox runat="server"
									ID="chkIsMapped"
									CssClass="js-selectable" 
									Checked='<%# (int)Eval("IsMapped") == 1 %>' />
							</ItemTemplate>
						</asp:TemplateField>

						<asp:BoundField
							HeaderText="Id"
							HeaderStyle-Width="50px"
							SortExpression="Product.ProductID" 
							DataField="ProductID" />

						<asp:HyperLinkField
							HeaderText="Product Name"
							SortExpression="coalesce(SelectedLocalization.LocalizedName, DefaultLocalization.Localizedname, UnspecifiedLocalization.LocalizedName)"
							DataTextField="Name"
							DataNavigateUrlFields="ProductID"
							DataNavigateUrlFormatString="product.aspx?productid={0}"
							Target="_top"
							Text="<%$Tokens:StringResource, admin.nolinktext %>" />

					</Columns>
				</asp:GridView>
			</div>
		</ListingTemplate>
	</listing:FilteredListing>
</asp:Content>
