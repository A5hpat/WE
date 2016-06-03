<%@ Page Language="C#" Inherits="AspDotNetStorefrontAdmin.CustomerLevels"  MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="customerlevels.aspx.cs" %>

<%@ Import Namespace="AspDotNetStorefrontCore" %>

<%@ Register TagPrefix="listing" Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls.Listing" %>
<%@ Register TagPrefix="filter" TagName="IntegerFilter" Src="Controls/Listing/IntegerFilter.ascx" %>
<%@ Register TagPrefix="filter" TagName="StringFilter" Src="Controls/Listing/StringFilter.ascx" %>
<%@ Register TagPrefix="filter" TagName="BooleanFilter" Src="Controls/Listing/BooleanFilter.ascx" %>

<asp:Content ContentPlaceHolderID="head" runat="server">

	<script type="text/javascript">
		var deleteConfirmPrompt = "<asp:Literal runat="server" Text="<%$Tokens:EscapedStringResource, admin.customerlevels.DeleteCustomerLevel, javascript %>" />";
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
		<i class="fa fa-list-ol"></i>
		<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.CustomerLevels %>" />
	</h1>

	<aspdnsf:AlertMessage ID="AlertMessageDisplay" runat="server" />

	<listing:FilteredListing runat="server"
		ID="FilteredListing"
		SqlQuery ="select {0} CustomerLevelID, nullif(dbo.GetMlValue(Name, @_locale),'') Name, Deleted from dbo.CustomerLevel with (NOLOCK) where {1}"
		SortExpression="dbo.GetMlValue(Name, @_locale)"
		LocaleSelectionEnabled="true">
		<ActionBarTemplate>
			<asp:HyperLink CssClass="btn btn-action" runat="server" Text="<%$ Tokens: StringResource, admin.customer.level.CreateCustomerLevel %>" NavigateUrl="customerlevel.aspx" />
		</ActionBarTemplate>
		<Filters>
			<filter:IntegerFilter runat="server" 
				Label="<%$Tokens:StringResource, admin.customerlevels.CustomerLevelId %>" 
				FieldName="CustomerLevelID" />
			
			<filter:StringFilter runat="server" 
				Label="<%$Tokens:StringResource, admin.common.Name %>" 
				FieldName="Name" />
			
			<filter:BooleanFilter runat="server"
				Label="<%$Tokens:StringResource, admin.editCustomerLevel.LevelIncludesFreeShipping %>"
				FieldName="LevelHasFreeShipping" />
			
			<filter:BooleanFilter runat="server"
				Label="Deleted"
				FieldName="Deleted"
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
					DataKeyNames="CustomerLevelID"
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
							SortExpression="CustomerLevelID"
							DataTextField="CustomerLevelID" 
							DataNavigateUrlFields="CustomerLevelID"
							DataNavigateUrlFormatString="customerlevel.aspx?CustomerLevelID={0}" />

						<listing:TooltipHyperLinkField
							HeaderText="<%$ Tokens: StringResource, admin.common.Name %>"
							SortExpression="dbo.GetMlValue(Name, @_locale)"
							ToolTip="<%$ Tokens: StringResource, admin.common.Edit %>"
							DataTextField="Name"
							DataNavigateUrlFields="CustomerLevelID"
							DataNavigateUrlFormatString="customerlevel.aspx?CustomerLevelID={0}"
							Text="<%$Tokens:StringResource, admin.nolinktext %>" />

						<listing:TooltipHyperLinkField
							ToolTip="<%$ Tokens: StringResource, admin.customerlevels.ViewCustomerLevel %>"
							ControlStyle-CssClass="action-link fa-users"
							DataNavigateUrlFields="CustomerLevelID"
							DataNavigateUrlFormatString="customerlevelcustomers.aspx?filter.0.0={0}" 
							Text="<%$ Tokens: StringResource, admin.customerlevels.ViewCustomerLevel %>" />

						<asp:TemplateField
							HeaderText="<%$ Tokens: StringResource, admin.common.Deleted %>"
							HeaderStyle-Width="8%">
							<ItemTemplate>
								<aspdnsf:CommandCheckBox runat="server"
									CssClass='<%# (byte)Eval("Deleted") == 1 ? "confirm-undelete" : "confirm-delete" %>'
									CheckedCommandName="<%# DeleteCustomerLevelCommand %>"
									UncheckedCommandName="<%# UndeleteCustomerLevelCommand %>"
									CommandArgument='<%# Eval("CustomerLevelID") %>'
									Checked='<%# (byte)Eval("Deleted") == 1 %>' />
							</ItemTemplate>
						</asp:TemplateField>

					</Columns>
				</asp:GridView>
			</div>
		</ListingTemplate>
	</listing:FilteredListing>

</asp:Content>
