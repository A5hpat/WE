<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.CreateOrder" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="createorder.aspx.cs" %>

<%@ Import Namespace="AspDotNetStorefrontCore" %>

<%@ Register TagPrefix="listing" Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls.Listing" %>
<%@ Register TagPrefix="filter" TagName="IntegerFilter" Src="Controls/Listing/IntegerFilter.ascx" %>
<%@ Register TagPrefix="filter" TagName="StringFilter" Src="Controls/Listing/StringFilter.ascx" %>
<%@ Register TagPrefix="filter" TagName="DataQueryFilter" Src="Controls/Listing/DataQueryFilter.ascx" %>
<%@ Register TagPrefix="filter" TagName="MultiFieldStringFilter" Src="Controls/Listing/MultiFieldStringFilter.ascx" %>

<asp:Content ContentPlaceHolderID="head" runat="server">
	<script type="text/javascript">
		var impersonateConfirmPrompt = "<asp:Literal runat="server" Text="<%$Tokens:EscapedStringResource, admin.createorder.confirm, javascript %>" />";
	</script>
</asp:Content>
<asp:Content ContentPlaceHolderID="bodyContentPlaceholder" runat="Server">

	<h1>
		<i class="fa fa-users"></i>
		<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.title.createorder %>" />
	</h1>

	<aspdnsf:AlertMessage runat="server" ID="AlertMessageDisplay" />

	<listing:FilteredListing runat="server"
		ID="FilteredListing"
		SqlQuery="select {0} 
					Customer.CustomerID, 
					Customer.CreatedOn, 
					ltrim(Customer.FirstName + ' ' + Customer.LastName) [Name], 
					Customer.Email, 
					nullif(Customer.CustomerLevelID, 0) [CustomerLevelID],
					case Customer.CustomerLevelID
						when 0 then null
						else dbo.GetMlValue(CustomerLevel.Name, @_locale)
					end [CustomerLevelName],
					case 
						when exists (select * from Orders with(nolock) where CustomerID = Customer.CustomerID) then 1 
						else null
					end [HasOrders]
				from 
					dbo.Customer with(nolock) 
					left join dbo.CustomerLevel with(nolock) on Customer.CustomerLevelID = CustomerLevel.CustomerLevelID
				where {1} AND IsAdmin = 0
				and IsRegistered = 1"
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
					DataKeyNames="CustomerID">
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
							HeaderText="<%$Tokens:StringResource, admin.editaddress.Email %>"
							SortExpression="Customer.Email" 
							DataField="Email" />
						
						<asp:HyperLinkField
							HeaderText="<%$Tokens:StringResource, admin.common.CustomerLevel %>"
							DataNavigateUrlFields="CustomerLevelId"
							DataNavigateUrlFormatString="customerlevel.aspx?CustomerLevelID={0}"
							DataTextField="CustomerLevelName" />

						<asp:HyperLinkField
							HeaderText="<%$Tokens:StringResource, admin.cst_recurring.OrderHistory %>"
							DataNavigateUrlFields="CustomerID"
							DataNavigateUrlFormatString="customer_history.aspx?customerid={0}"
							DataTextField="HasOrders" 
							DataTextFormatString="View" />

						<asp:TemplateField>
							<ItemTemplate>
								<asp:Button ID="btnStartImpersonation"
									runat="server"
									CssClass="btn btn-action"
									OnClick="btnStartImpersonation_Click"
									OnClientClick="return confirm(impersonateConfirmPrompt);"
									Text="Impersonate Customer"
									CommandArgument='<%# DataBinder.Eval(Container.DataItem, "CustomerID") %>' />
							</ItemTemplate>
						</asp:TemplateField>
					</Columns>
				</asp:GridView>
			</div>
		</ListingTemplate>
	</listing:FilteredListing>
</asp:Content>
