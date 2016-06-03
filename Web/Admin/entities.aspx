<%@ Page Language="C#" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Theme="Admin_Default" Inherits="AspDotNetStorefrontAdmin.Entities" Codebehind="entities.aspx.cs" %>

<%@ Import Namespace="AspDotNetStorefrontCore" %>

<%@ Register TagPrefix="listing" Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls.Listing" %>
<%@ Register TagPrefix="filter" TagName="IntegerFilter" Src="Controls/Listing/IntegerFilter.ascx" %>
<%@ Register TagPrefix="filter" TagName="StringFilter" Src="Controls/Listing/StringFilter.ascx" %>
<%@ Register TagPrefix="filter" TagName="BooleanFilter" Src="Controls/Listing/BooleanFilter.ascx" %>
<%@ Register TagPrefix="filter" TagName="DataQueryFilter" Src="Controls/Listing/DataQueryFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="LinkGroupEntities" Src="Controls/LinkGroupEntities.ascx" %>

<asp:Content ContentPlaceHolderID="head" runat="server">

	<script type="text/javascript">
		var deleteConfirmPrompt = "<asp:Literal runat="server" Text="<%$Tokens:EscapedStringResource, admin.entitygrid.DeleteConfirm, javascript %>" />";
		var undeleteConfirmPrompt = "<asp:Literal runat="server" Text="<%$Tokens:EscapedStringResource, admin.entitygrid.UnDeleteConfirm, javascript %>" />";

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
		<i class="fa fa-sitemap"></i>
		<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.Entities %>" />
		<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.breadcrumbseparator %>" />
		<asp:Literal runat="server" ID="HeaderName" />
	</h1>

	<aspdnsf:AlertMessage runat="server" ID="AlertMessageDisplay" />

	<listing:FilteredListing runat="server"
		ID="FilteredListing"
		SqlQuery="
			select
				{0}
				EntityID,
				EntityType,
				dbo.GetEntityPath(EntityID, EntityType, ' &gt; ', @_locale, @_currentCustomerLocale) Name,
				Published,
				Deleted
			from Entities with(nolock) 
			where EntityType = @entityType and {1}"
		SortExpression="dbo.GetEntityPath(EntityID, EntityType, ' &gt; ', @_locale, @_currentCustomerLocale)"
		LocaleSelectionEnabled="true">
		<ActionBarTemplate>
			<aspdnsf:LinkGroupEntities runat="server" SelectedLink='<%# String.Format("entities.aspx?entityname={0}", EntityType) %>' />
			<asp:HyperLink runat="server"
				CssClass="btn btn-action"
				NavigateUrl='<%# String.Format("entity.aspx?entityName={0}", EntityType)%>'
				Text='<%# String.Format("{0} {1}", "common.cs.90".StringResource(), String.Format("admin.common.{0}",EntityType).StringResource().ToLower()) %>' />
		</ActionBarTemplate>
		<Filters>
			<filter:IntegerFilter ID="IdFilter" runat="server"
				Label="<%$Tokens:StringResource, admin.entity.IdSearch %>"
				FieldName="EntityID"
				QueryStringNames="filter.id" />

			<filter:StringFilter runat="server"
				ID="NameFilter"
				Label="<%$Tokens:StringResource, admin.entity.NameSearch %>"
				FieldName="dbo.GetEntityPath(EntityID, EntityType, ' &gt; ', @_locale, @_currentCustomerLocale)" />

			<filter:BooleanFilter runat="server"
				Label="Deleted"
				FieldName="Deleted"
				NullDisplayName="Any"
				DefaultValue="False" />
		</Filters>
		<SqlParameters>
			<asp:QueryStringParameter Name="entityType" QueryStringField="EntityName" DefaultValue="Category" />
		</SqlParameters>
		<ListingTemplate>
			<div class="white-ui-box">
				<asp:GridView runat="server"
					ID="Grid"
					CssClass="table js-sortable-gridview"
					GridLines="None"
					DataSourceID="FilteredListingDataSource"
					AllowSorting="true"
					AutoGenerateColumns="false"
					DataKeyNames="EntityID"
					OnDataBinding="Grid_DataBinding"
					OnRowCommand="DispatchGridCommand">
					<EmptyDataTemplate>
						<div class="alert alert-info">
							<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.EmptyDataTemplate.NoResults %>" />
						</div>
					</EmptyDataTemplate>
					<Columns>
						<asp:HyperLinkField
							HeaderText="<%$ Tokens: StringResource, admin.common.ID %>"
							HeaderStyle-Width="7%"
							SortExpression="EntityID"
							DataTextField="EntityID" 
							DataNavigateUrlFields="EntityID,EntityType"
							DataNavigateUrlFormatString="entity.aspx?entityid={0}&entityname={1}" />

						<%--Header text for this in set in the code behind in the ondatabinding event for the grid--%>
						<listing:TooltipHyperLinkField
							HeaderText="<%$ Tokens: StringResource, admin.entity.NameSearch %>"
							SortExpression="dbo.GetEntityPath(EntityID, EntityType, ' &gt; ', @_locale, @_currentCustomerLocale)"
							ToolTip="<%$ Tokens: StringResource, admin.entitygrid.NameTooltip %>"
							DataTextField="Name"
							DataNavigateUrlFields="EntityID,EntityType"
							DataNavigateUrlFormatString="entity.aspx?entityid={0}&entityname={1}"
							Text="<%$Tokens:StringResource, admin.nolinktext %>" />

						<asp:TemplateField
							HeaderText="<%$ Tokens: StringResource, admin.common.Published %>"
							HeaderStyle-Width="8%">
							<ItemTemplate>
								<aspdnsf:CommandCheckBox runat="server"
									ToolTip='<%# (int)Eval("Published") == 1 
										? AppLogic.GetString("admin.entitygrid.UnpublishTooltip", AppLogic.GetCurrentCustomer().LocaleSetting) 
										: AppLogic.GetString("admin.entitygrid.PublishTooltip", AppLogic.GetCurrentCustomer().LocaleSetting) %>'
									CheckedCommandName="<%# PublishEntityCommand %>"
									UncheckedCommandName="<%# UnpublishEntityCommand %>"
									CommandArgument='<%# Eval("EntityID") %>'
									AutoPostBack="true"
									Checked='<%# (int)Eval("Published") == 1 %>' />
							</ItemTemplate>
						</asp:TemplateField>

						<asp:TemplateField
							HeaderText="<%$ Tokens: StringResource, admin.common.Deleted %>"
							HeaderStyle-Width="8%">
							<ItemTemplate>
								<aspdnsf:CommandCheckBox runat="server"
									CssClass='<%# (byte)Eval("Deleted") == 1 
										? "confirm-undelete" 
										: "confirm-delete" %>'
									ToolTip='<%# (byte)Eval("Deleted") == 1 
										? AppLogic.GetString("admin.entitygrid.UnDeleteTooltip", AppLogic.GetCurrentCustomer().LocaleSetting) 
										: AppLogic.GetString("admin.entitygrid.DeleteTooltip", AppLogic.GetCurrentCustomer().LocaleSetting) %>'
									CheckedCommandName="<%# DeleteEntityCommand %>"
									UncheckedCommandName="<%# UndeleteEntityCommand %>"
									CommandArgument='<%# Eval("EntityID") %>'
									Checked='<%# (byte)Eval("Deleted") == 1 %>' />
							</ItemTemplate>
						</asp:TemplateField>

						<asp:HyperLinkField
							ItemStyle-CssClass="action-column"
							Text="Add"
							DataNavigateUrlFields="EntityID,EntityType"
							DataNavigateUrlFormatString="entity.aspx?entityParent={0}&EntityName={1}" ControlStyle-CssClass="action-link fa-plus" />

					</Columns>
				</asp:GridView>
			</div>
		</ListingTemplate>
	</listing:FilteredListing>

</asp:Content>
