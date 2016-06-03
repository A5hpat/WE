<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.Customers" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="customers.aspx.cs" %>

<%@ Import Namespace="AspDotNetStorefrontCore" %>

<%@ Register TagPrefix="listing" Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls.Listing" %>
<%@ Register TagPrefix="filter" TagName="IntegerFilter" Src="Controls/Listing/IntegerFilter.ascx" %>
<%@ Register TagPrefix="filter" TagName="StringFilter" Src="Controls/Listing/StringFilter.ascx" %>
<%@ Register TagPrefix="filter" TagName="BooleanFilter" Src="Controls/Listing/BooleanFilter.ascx" %>
<%@ Register TagPrefix="filter" TagName="DataQueryFilter" Src="Controls/Listing/DataQueryFilter.ascx" %>
<%@ Register TagPrefix="filter" TagName="LocaleFilter" Src="Controls/Listing/LocaleFilter.ascx" %>
<%@ Register TagPrefix="filter" TagName="MultiFieldStringFilter" Src="Controls/Listing/MultiFieldStringFilter.ascx" %>
<%@ Register TagPrefix="filter" TagName="ListFilter" Src="Controls/Listing/ListFilter.ascx" %>
<%@ Register TagPrefix="filter" TagName="StoreFilter" Src="Controls/Listing/StoreFilter.ascx" %>

<asp:Content ContentPlaceHolderID="head" runat="server">
	<script type="text/javascript">
		var deleteConfirmPrompt = "<asp:Literal runat="server" Text="<%$Tokens:EscapedStringResource, admin.customer.ConfirmDelete, javascript %>" />";
		var undeleteConfirmPrompt = "<asp:Literal runat="server" Text="<%$Tokens:EscapedStringResource, admin.customer.ConfirmUnDelete, javascript %>" />";
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
		<i class="fa fa-users"></i>
		<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.title.customers %>" />
	</h1>

	<aspdnsf:AlertMessage runat="server" ID="AlertMessageDisplay" />

	<listing:FilteredListing runat="server"
		ID="FilteredListing"
		SqlQuery="select {0} 
					Customer.CustomerID, 
					Customer.CreatedOn, 
					ltrim(Customer.FirstName + ' ' + Customer.LastName) [Name], 
					Customer.IsAdmin, 
					case Customer.IsAdmin
						when 0 then 'Normal User'
						when 1 then 'Admin'
						when 3 then 'SuperAdmin'
					end [AdminType],
					Customer.Email, 
					Customer.Deleted, 
					Customer.IsRegistered,
					nullif(Customer.CustomerLevelID, 0) [CustomerLevelID],
					case Customer.CustomerLevelID
						when 0 then null
						else dbo.GetMlValue(CustomerLevel.Name, @_locale)
					end [CustomerLevelName],
					Store.Name [StoreName],
					case 
						when exists (select * from Orders with(nolock) where CustomerID = Customer.CustomerID) then 1 
						else null
					end [HasOrders]
				from 
					dbo.Customer with(nolock) 
					left join dbo.CustomerLevel with(nolock) on Customer.CustomerLevelID = CustomerLevel.CustomerLevelID
					left join dbo.Store with(nolock) on Customer.StoreID = Store.StoreID
				where {1}"
		SortExpression="Customer.CreatedOn"
		LocaleSelectionEnabled="true"
		PageSizes="20,50,100"
		DefaultPageSize="20">
		<ActionBarTemplate>
			<asp:HyperLink runat="server"
				CssClass="btn btn-action"
				NavigateUrl="customer.aspx"
				Text="<%$ Tokens: StringResource, admin.menu.CustomerAdd %>" />
		</ActionBarTemplate>
		<Filters>
			<filter:StringFilter runat="server"
				Label="<%$ Tokens:StringResource, admin.common.Email %>"
				FieldName="Customer.Email" />

			<filter:MultiFieldStringFilter runat="server"
				Label="<%$ Tokens:StringResource, admin.common.Name %>"
				Fields="Customer.FirstName,Customer.LastName" />

			<filter:IntegerFilter runat="server"
				Label="<%$ Tokens:StringResource, admin.common.CustomerId %>"
				FieldName="Customer.CustomerID" />

			<filter:DataQueryFilter runat="server"
				Label="Customer Level"
				FieldName="Customer.CustomerLevelID"
				DataQuery="SELECT CustomerLevelID, dbo.GetMlValue(Name, @_currentCustomerLocale) [Name] FROM CustomerLevel"
				DataTextField="Name"
				DataValueField="CustomerLevelID" />
	
			<filter:ListFilter runat="server"
				Label="Admin"
				FieldName="IsAdmin">
				<Items>
					<asp:ListItem Value="0" Text="Normal User" />
					<asp:ListItem Value="1" Text="Admin" />
					<asp:ListItem Value="3" Text="SuperAdmin" />
				</Items>
			</filter:ListFilter>

			<filter:BooleanFilter runat="server"
				Label="Registered"
				FieldName="Customer.IsRegistered"
				NullDisplayName="Any"
				DefaultValue="True" />

			<filter:BooleanFilter runat="server"
				Label="Deleted"
				FieldName="Customer.Deleted"
				NullDisplayName="Any" />

			<filter:StoreFilter runat="server"
				Label="Store"
				FieldName="Customer.StoreID"
				NullDisplayName="Any" />

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
					DataKeyNames="CustomerID"
					OnLoad="GridLoad"
					OnRowCommand="DispatchGridCommand">
					<EmptyDataTemplate>
						<div class="alert alert-info">
							<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.EmptyDataTemplate.NoResults %>" />
						</div>
					</EmptyDataTemplate>
					<Columns>
						<asp:BoundField
							HeaderText="Create Date"
							HeaderStyle-Width="15%"
							SortExpression="Customer.CreatedOn"
							DataField="CreatedOn" />

						<asp:HyperLinkField
							HeaderText="Name"
							SortExpression="ltrim(Customer.FirstName + ' ' + Customer.LastName)" 
							DataNavigateUrlFields="CustomerID"
							DataNavigateUrlFormatString="customer.aspx?customerid={0}"
							DataTextField="Name"
							Text="<%$Tokens:StringResource, admin.nolinktext %>" />

						<asp:BoundField
							HeaderText="Admin"
							HeaderStyle-Width="8%"
							SortExpression="
								(select case Customer.IsAdmin
									when 0 then 'Normal User'
									when 1 then 'Admin'
									when 3 then 'SuperAdmin'
								end)"
							DataField="AdminType" />

						<asp:BoundField
							HeaderText="<%$Tokens:StringResource, admin.editaddress.Email %>"
							SortExpression="Customer.Email" 
							DataField="Email" />
						
						<asp:HyperLinkField
							HeaderText="<%$Tokens:StringResource, admin.common.CustomerLevel %>"
							DataNavigateUrlFields="CustomerLevelId"
							DataNavigateUrlFormatString="customerlevel.aspx?CustomerLevelID={0}"
							DataTextField="CustomerLevelName" />

						<asp:BoundField
							HeaderText="<%$Tokens:StringResource, admin.common.StoreName %>"
							SortExpression="Store.Name" 
							DataField="StoreName" />

						<asp:HyperLinkField
							HeaderText="<%$Tokens:StringResource, admin.cst_recurring.OrderHistory %>"
							DataNavigateUrlFields="CustomerID"
							DataNavigateUrlFormatString="customer_history.aspx?customerid={0}"
							DataTextField="HasOrders" 
							DataTextFormatString="View" />

						<asp:TemplateField
							HeaderText="<%$ Tokens: StringResource, admin.common.Deleted %>"
							HeaderStyle-Width="8%">
							<ItemTemplate>
								<aspdnsf:CommandCheckBox runat="server"
									CssClass='<%# (byte)DataBinder.Eval(Container.DataItem, "Deleted") == 1 ? "confirm-undelete" : "confirm-delete" %>'
									ToolTip='<%# (byte)DataBinder.Eval(Container.DataItem, "Deleted") == 1 ? AppLogic.GetString("admin.customer.UnDeleteTooltip", AppLogic.GetCurrentCustomer().LocaleSetting) : AppLogic.GetString("admin.customer.DeleteTooltip", AppLogic.GetCurrentCustomer().LocaleSetting) %>'
									CheckedCommandName="<%# DeleteCustomerCommand %>"
									UncheckedCommandName="<%# UndeleteCustomerCommand %>"
									CommandArgument='<%# DataBinder.Eval(Container.DataItem, "CustomerID") %>'
									Checked='<%# (byte)DataBinder.Eval(Container.DataItem, "Deleted") == 1 %>'
									AutoPostBack="false" />
							</ItemTemplate>
						</asp:TemplateField>
					</Columns>
				</asp:GridView>
			</div>
		</ListingTemplate>
	</listing:FilteredListing>
</asp:Content>
