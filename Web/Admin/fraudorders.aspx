<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.fraudorders" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" EnableEventValidation="false" Codebehind="fraudorders.aspx.cs" %>

<%@ Register Src="Controls/LinkGroupOrders.ascx" TagPrefix="aspdnsf" TagName="LinkGroupOrders" %>

<%@ Register TagPrefix="aspdnsf" Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls.Listing" %>
<%@ Register TagPrefix="aspdnsf" TagName="DateRangeFilter" Src="Controls/Listing/DateRangeFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="IntegerFilter" Src="Controls/Listing/IntegerFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="DataQueryFilter" Src="Controls/Listing/DataQueryFilter.ascx" %>
<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<h1>
		<i class="fa fa-list"></i>
		<asp:Literal ID="litHeader" runat="server" Text="<%$Tokens:StringResource, admin.menu.OrderManage %>" />
		<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.breadcrumbseparator %>" />
		<asp:Literal ID="Literal1" runat="server" Text="<%$Tokens:StringResource, admin.menu.fraudorders %>" />
	</h1>
	<div class="list-action-bar">
		<aspdnsf:LinkGroupOrders runat="server" ID="LinkGroupOrders" SelectedLink="fraudorders.aspx" />
	</div>
	<div class="admin-module">
		<aspdnsf:FilteredListing runat="server"
			ID="FilteredListing"
			SqlQuery="select {0} OrderNumber, OrderDate, OrderTotal, Email, FirstName, LastName, Orders.StoreId, Store.Name [StoreName] FROM Orders left join Store on Orders.StoreID = Store.StoreID WHERE TransactionState = 'FRAUD' AND {1}"
			SortExpression="OrderNumber">
			<Filters>
				<aspdnsf:IntegerFilter runat="server"
					Label="<%$Tokens:StringResource, admin.order.OrderNumberTransactionSubscriptionID %>"
					FieldName="OrderNumber" />

				<aspdnsf:DateRangeFilter runat="server"
					StartLabel="<%$Tokens:StringResource, admin.orders.StartDate %>"
					EndLabel="<%$Tokens:StringResource, admin.orders.EndDate %>"
					FieldName="OrderDate" />

				<aspdnsf:DataQueryFilter ID="DataQueryFilter2" runat="server"
					Label="<%$Tokens:StringResource, admin.order.ForStore %>"
					FieldName="Orders.StoreId"
					DataQuery="select StoreId, Name from Store"
					DataTextField="Name"
					DataValueField="StoreId" />
			</Filters>
			<ListingTemplate>
				<div id="OrderGrid" class="white-ui-box">
					<div class="white-box-heading">
						<asp:Literal ID="litGridHead" runat="server" Text="<%$Tokens:StringResource, admin.common.MatchingOrders %>" />
					</div>
					<asp:GridView runat="server"
						ID="grdOrders"
						DataSourceID="FilteredListingDataSource"
						CssClass="table table-detail"
						GridLines="None"
						AutoGenerateColumns="false"
						DataKeyNames="OrderNumber">
						<EmptyDataTemplate>
							<div class="alert alert-info">
								<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.EmptyDataTemplate.NoResults %>" />
							</div>
						</EmptyDataTemplate>
						<Columns>
							<asp:HyperLinkField
								HeaderText="<%$Tokens:StringResource, admin.common.OrderNumber %>"
								DataTextField="OrderNumber"
								DataNavigateUrlFields="OrderNumber"
								DataNavigateUrlFormatString="order.aspx?ordernumber={0}" />

							<asp:BoundField
								HeaderText="<%$Tokens:StringResource, admin.common.OrderDate %>"
								DataField="OrderDate"
								DataFormatString="{0:d}" />

							<asp:TemplateField
								HeaderText="<%$Tokens:StringResource, admin.common.OrderTotal %>">
								<ItemTemplate>
									<%# ThisCustomer.CurrencyString((decimal)Eval("OrderTotal")) %>
								</ItemTemplate>
							</asp:TemplateField>

							<asp:BoundField
								HeaderText="Store"
								DataField="StoreName" />

							<asp:BoundField
								HeaderText="<%$Tokens:StringResource, admin.common.Contact %>"
								DataField="Email" />

							<asp:BoundField
								HeaderText="First name"
								DataField="FirstName" />

							<asp:BoundField
								HeaderText="Last name"
								DataField="LastName" />

						</Columns>
					</asp:GridView>
				</div>
			</ListingTemplate>
		</aspdnsf:FilteredListing>
	</div>
</asp:Content>
