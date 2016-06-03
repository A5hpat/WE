<%@ Page Language="C#" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Theme="Admin_Default" Inherits="AspDotNetStorefrontAdmin.Affiliates" Codebehind="affiliates.aspx.cs" %>

<%@ Import Namespace="AspDotNetStorefrontCore" %>

<%@ Register TagPrefix="listing" Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls.Listing" %>
<%@ Register TagPrefix="filter" TagName="IntegerFilter" Src="Controls/Listing/IntegerFilter.ascx" %>
<%@ Register TagPrefix="filter" TagName="StringFilter" Src="Controls/Listing/StringFilter.ascx" %>
<%@ Register TagPrefix="filter" TagName="BooleanFilter" Src="Controls/Listing/BooleanFilter.ascx" %>
<%@ Register TagPrefix="filter" TagName="DataQueryFilter" Src="Controls/Listing/DataQueryFilter.ascx" %>

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
		<i class="fa fa-retweet"></i>
		<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.affiliates.Affiliates %>" />
	</h1>

	<aspdnsf:AlertMessage runat="server" ID="AlertMessageDisplay" />

	<listing:FilteredListing runat="server"
		ID="FilteredListing"
		SqlQuery="select {0} AffiliateID, Name, FirstName, LastName, Published, Deleted from Affiliate with(nolock) where {1}"
		SortExpression="Name"
		LocaleSelectionEnabled="false">
		<ActionBarTemplate>
			<asp:HyperLink runat="server"
				CssClass="btn btn-action"
				NavigateUrl='<%# String.Format("affiliate.aspx")%>'
				Text="<%$ Tokens: StringResource, admin.affiliate.Create %>" />
		</ActionBarTemplate>
		<Filters>
			<filter:IntegerFilter ID="IdFilter" runat="server"
				Label="<%$Tokens:StringResource, admin.entity.IdSearch %>"
				FieldName="AffiliateID"
				QueryStringNames="filter.id" />

			<filter:StringFilter runat="server"
				ID="NameFilter"
				Label="<%$Tokens:StringResource, admin.entity.NameSearch %>"
				FieldName="Name" />

			<filter:BooleanFilter runat="server"
				Label="Deleted"
				FieldName="Deleted"
				DefaultValue="False" />

			<filter:BooleanFilter runat="server"
				Label="Published"
				FieldName="Published" />
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
					DataKeyNames="AffiliateID"
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
							SortExpression="AffiliateID"
							DataTextField="AffiliateID"
							DataNavigateUrlFields="AffiliateID"
							DataNavigateUrlFormatString="affiliate.aspx?affiliateId={0}" />

						<listing:TooltipHyperLinkField
							HeaderText="<%$ Tokens: StringResource, admin.entity.NameSearch %>"
							SortExpression="Name"
							ToolTip="<%$ Tokens: StringResource, admin.entitygrid.NameTooltip %>"
							DataTextField="Name"
							DataNavigateUrlFields="AffiliateID"
							DataNavigateUrlFormatString="affiliate.aspx?affiliateId={0}"
							Text="<%$Tokens:StringResource, admin.nolinktext %>" />

						<asp:TemplateField
							HeaderText="<%$ Tokens: StringResource, admin.common.Published %>"
							HeaderStyle-Width="8%">
							<ItemTemplate>
								<aspdnsf:CommandCheckBox runat="server"
									ToolTip='<%# (byte)Eval("Published") == 1 
										? AppLogic.GetString("admin.entitygrid.PublishTooltip", AppLogic.GetCurrentCustomer().LocaleSetting) 
										: AppLogic.GetString("admin.entitygrid.UnpublishTooltip", AppLogic.GetCurrentCustomer().LocaleSetting) %>'
									CheckedCommandName="<%# PublishAffiliateCommand %>"
									UncheckedCommandName="<%# UnpublishAffiliateCommand %>"
									CommandArgument='<%# Eval("AffiliateID") %>'
									AutoPostBack="true"
									Checked='<%# (byte)Eval("Published") == 1 %>' />
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
										? AppLogic.GetString("admin.entitygrid.DeleteTooltip", AppLogic.GetCurrentCustomer().LocaleSetting) 
										: AppLogic.GetString("admin.entitygrid.UnDeleteTooltip", AppLogic.GetCurrentCustomer().LocaleSetting) %>'
									CheckedCommandName="<%# DeleteAffiliateCommand %>"
									UncheckedCommandName="<%# UndeleteAffiliateCommand %>"
									CommandArgument='<%# Eval("AffiliateID") %>'
									Checked='<%# (byte)Eval("Deleted") == 1 %>' />
							</ItemTemplate>
						</asp:TemplateField>

						<asp:HyperLinkField
							ItemStyle-CssClass="action-column"
							Text="Add"
							DataNavigateUrlFields="AffiliateID"
							DataNavigateUrlFormatString="affiliate.aspx?parentAffiliateId={0}" ControlStyle-CssClass="action-link fa-plus" />

					</Columns>
				</asp:GridView>
			</div>
		</ListingTemplate>
	</listing:FilteredListing>

</asp:Content>
