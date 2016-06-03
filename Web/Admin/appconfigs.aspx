<%@ Page Title="<%$Tokens:StringResource, admin.title.appconfigs %>" Theme="Admin_Default" Language="C#" Inherits="AspDotNetStorefrontAdmin.AppConfigs" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="appconfigs.aspx.cs" %>

<%@ Import Namespace="AspDotNetStorefrontCore" %>

<%@ Register TagPrefix="listing" Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls.Listing" %>
<%@ Register TagPrefix="filter" TagName="MultiFieldStringFilter" Src="Controls/Listing/MultiFieldStringFilter.ascx" %>
<%@ Register TagPrefix="filter" TagName="DataQueryFilter" Src="Controls/Listing/DataQueryFilter.ascx" %>

<asp:Content ContentPlaceHolderID="head" runat="server">
	<script type="text/javascript" src="Scripts/core.js"></script>
</asp:Content>

<asp:Content ContentPlaceHolderID="bodyContentPlaceholder" runat="Server">

	<h1>
		<i class="fa fa-cog f-x3"></i>
        <%-- //nal adamin.common.AppConfigs --%>
		<asp:Literal runat="server" Text="Settings" />
	</h1>

	<aspdnsf:AlertMessage ID="AlertMessageDisplay" runat="server" />

	<listing:FilteredListing runat="server" 
		ID="FilteredListing"
		SqlQuery="select {0} AppConfig.StoreId [Store.StoreID], 
		Store.Name [Store.Name], 
		AppConfig.AppConfigID [AppConfig.AppConfigID], 
		DefaultAppConfig.AppConfigID [AppConfig.DefaultAppConfigID], 
		AppConfig.Name [AppConfig.Name], 
		AppConfig.Description [AppConfig.Description], 
		AppConfig.ConfigValue [AppConfig.ConfigValue], 
		AppConfig.GroupName [AppConfig.GroupName] 
		from dbo.AppConfig with(nolock) 
		left join dbo.Store with(nolock) 
			on AppConfig.StoreID = Store.StoreID 
		left join dbo.AppConfig DefaultAppConfig with(nolock) 
			on DefaultAppConfig.StoreID = 0 
			and DefaultAppConfig.Name = AppConfig.Name 
		where (AppConfig.SuperOnly = 0 or @IsSuperAdmin = 1) and {1}"
		SortExpression="AppConfig.Name {0}, AppConfig.StoreID">
		<ActionBarTemplate>
			<asp:HyperLink CssClass="btn btn-action" runat="server" Text="Create Setting" NavigateUrl="config.aspx?mode=appconfig" />
		</ActionBarTemplate>
		<Filters>
			<filter:DataQueryFilter runat="server"
				Label="Setting Group:"
				QueryStringNames="filter.group"
				FieldName="AppConfig.GroupName"
				DataQuery="select distinct GroupName from AppConfig where GroupName is not null"
				DataValueField="GroupName" />
			<filter:MultiFieldStringFilter runat="server"
				Label="Search Name, Description, Value"
				QueryStringNames="filter.search"
				Fields="AppConfig.Name,AppConfig.Description,AppConfig.ConfigValue" />
		</Filters>
		<ListingTemplate>
			<div class="white-ui-box">
				<asp:GridView runat="server"
					ID="Grid"
					CssClass="table js-sortable-gridview table-fixed"
					GridLines="None"
					DataSourceID="FilteredListingDataSource"
					AllowSorting="true"
					AutoGenerateColumns="false"
					DataKeyNames="AppConfig.AppConfigID">
					<EmptyDataTemplate>
						<div class="alert alert-info">
							<asp:Literal runat="server" Text="No results found. Please try different filter settings." />
						</div>
					</EmptyDataTemplate>
					<Columns>
						<asp:BoundField
							HeaderText="Store"
							HeaderStyle-Width="10%"
							DataField="Store.Name"
							NullDisplayText="<b>Default</b>" />

						<listing:TooltipHyperLinkField
							HeaderText="Name"
							HeaderStyle-Width="20%"
							SortExpression="AppConfig.Name {0}, AppConfig.StoreID"
							ToolTip="Click Here to Edit Item"
							DataTextField="AppConfig.Name"
							DataNavigateUrlFields="AppConfig.DefaultAppConfigID"
							DataNavigateUrlFormatString="config.aspx?mode=appconfig&id={0}"
							Text="<i>(no data specified)</i>" />

						<asp:BoundField
							HeaderText="Description"
							HeaderStyle-Width="40%"
							HtmlEncode="false"
							DataField="AppConfig.Description" />

						<asp:BoundField
							HeaderText="Value"
							HeaderStyle-Width="20%"
							DataField="AppConfig.ConfigValue" ItemStyle-CssClass="wrap-text" />

						<asp:BoundField
							HeaderText="Group"
							HeaderStyle-Width="10%"
							SortExpression="AppConfig.GroupName {0}, AppConfig.Name, AppConfig.StoreID"
							DataField="AppConfig.GroupName" />
					</Columns>
				</asp:GridView>
			</div>
		</ListingTemplate>
	</listing:FilteredListing>

</asp:Content>
