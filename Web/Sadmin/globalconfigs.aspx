<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.GlobalConfigs" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="globalconfigs.aspx.cs" %>

<%@ Register TagPrefix="listing" Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls.Listing" %>
<%@ Register TagPrefix="filter" TagName="MultiFieldStringFilter" Src="Controls/Listing/MultiFieldStringFilter.ascx" %>
<%@ Register TagPrefix="filter" TagName="DataQueryFilter" Src="Controls/Listing/DataQueryFilter.ascx" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<aspdnsf:AlertMessage ID="AlertMessageDisplay" runat="server" />
	<h1>
		<i class="fa fa-cog f-x3"></i>
		<asp:Literal runat="server" Text="<%$ Tokens: StringResource, admin.title.globalconfigs %>" />
	</h1>

	<listing:FilteredListing runat="server"
		ID="FilteredListing"
		SqlQuery="select {0} GlobalConfigID, Name, Description, ConfigValue, GroupName from dbo.GlobalConfig with(nolock) where {1}"
		SortExpression="Name"
		DefaultPageSize="50">
		<Filters>
			<filter:DataQueryFilter runat="server"
				Label="<%$Tokens:StringResource, admin.appconfig.ConfigGroups %>"
				QueryStringNames="filter.group"
				FieldName="GroupName"
				DataQuery="select distinct GroupName from GlobalConfig where GroupName is not null"
				DataValueField="GroupName" />
			<filter:MultiFieldStringFilter runat="server"
				Label="<%$ Tokens: StringResource, common.cs.82 %>"
				QueryStringNames="filter.search"
				Fields="Name,Description" />
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
					DataKeyNames="GlobalConfigID">
					<EmptyDataTemplate>
						<div class="alert alert-info">
							<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.EmptyDataTemplate.NoResults %>" />
						</div>
					</EmptyDataTemplate>
					<Columns>
						<listing:TooltipHyperLinkField
							HeaderText="Name"
							SortExpression="Name"
							ToolTip="<%$ Tokens: StringResource, admin.productgrid.NameTooltip %>"
							DataTextField="Name"
							DataNavigateUrlFields="GlobalConfigID"
							DataNavigateUrlFormatString="config.aspx?mode=globalconfig&id={0}"
							Text="<%$Tokens:StringResource, admin.nolinktext %>" />
						<asp:HyperLinkField
							DataNavigateUrlFormatString="">
							<HeaderStyle Width="20%" />
						</asp:HyperLinkField>
						<asp:BoundField
							DataField="Description"
							HeaderText="Description">
							<HeaderStyle Width="40%" />
						</asp:BoundField>
						<asp:BoundField
							DataField="ConfigValue"
							HeaderText="Value">
							<HeaderStyle Width="20%" />
						</asp:BoundField>
						<asp:BoundField
							DataField="GroupName"
							HeaderText="Group">
							<HeaderStyle Width="10%" />
						</asp:BoundField>
					</Columns>
				</asp:GridView>
			</div>
		</ListingTemplate>
	</listing:FilteredListing>

</asp:Content>
