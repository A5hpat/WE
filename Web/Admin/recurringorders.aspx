<%@ Page Language="c#" Inherits="AspDotNetStorefrontAdmin.recurringorders" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" CodeBehind="recurringorders.aspx.cs" %>

<%@ Register TagPrefix="listing" Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls.Listing" %>
<%@ Register TagPrefix="filter" TagName="StringFilter" Src="Controls/Listing/StringFilter.ascx" %>
<%@ Register TagPrefix="filter" TagName="DateRangeFilter" Src="Controls/Listing/DateRangeFilter.ascx" %>
<%@ Register TagPrefix="filter" TagName="IntegerFilter" Src="Controls/Listing/IntegerFilter.ascx" %>
<%@ Register TagPrefix="filter" TagName="StoreFilter" Src="Controls/Listing/StoreFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="LinkGroupRecurring" Src="Controls/LinkGroupRecurring.ascx" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<h1>
		<i class="fa fa-list"></i>
		Recurring Orders
	</h1>

	<aspdnsf:AlertMessage ID="AlertMessage" runat="server" />

	<listing:FilteredListing runat="server"
		ID="FilteredListing"
		SortExpression="OriginalRecurringOrderNumber">
		<ActionBarTemplate>
			<aspdnsf:LinkGroupRecurring runat="server" ID="LinkGroupRecurring" SelectedLink="recurringorders.aspx" />
			<asp:Button runat="server"
				ID="btnProcessAll"
				Enabled="<%# OrdersArePendingToday() %>"
				CssClass="btn btn-primary"
				Text="<%$Tokens:StringResource, admin.recurring.ProcessChargesAll %>"
				OnClick="btnProcessAll_Click" />
		</ActionBarTemplate>
		<Filters>
			<filter:IntegerFilter runat="server"
				Label="Order Number"
				FieldName="ShoppingCart.OriginalRecurringOrderNumber" />

			<filter:StringFilter runat="server"
				Label="Customer ID"
				FieldName="ShoppingCart.CustomerID" />

			<filter:StringFilter runat="server"
				Label="Email"
				FieldName="Customer.Email" />

			<filter:DateRangeFilter runat="server"
				StartLabel="Next Ship Date Start"
				EndLabel="Next Ship Date End"
				FieldName="NextRecurringShipDate" />

			<filter:StoreFilter runat="server"
				Label="<%$Tokens:StringResource, admin.order.ForStore %>"
				FieldName="Store.StoreId" />
		</Filters>
		<ListingTemplate>
			<div class="white-ui-box">
				<asp:GridView runat="server"
					CssClass="table js-sortable-gridview"
					GridLines="None"
					DataSourceID="FilteredListingDataSource"
					AllowSorting="true"
					AutoGenerateColumns="False">
					<Columns>
						<asp:HyperLinkField
							HeaderText="Order Number"
							DataNavigateUrlFields="OriginalRecurringOrderNumber"
							DataNavigateUrlFormatString="recurringorder.aspx?originalorderid={0}"
							DataTextField="OriginalRecurringOrderNumber"
							SortExpression="OriginalRecurringOrderNumber" />

						<asp:BoundField
							HeaderText="Customer ID"
							DataField="CustomerID"
							SortExpression="ShoppingCart.CustomerId" />

						<asp:BoundField
							HeaderText="Email"
							DataField="Email"
							SortExpression="Email" />

						<asp:BoundField
							HeaderText="Next Shipping Date"
							DataField="NextRecurringShipDate"
							SortExpression="NextRecurringShipDate" />

						<asp:BoundField
							HeaderText="Store"
							DataField="Name" />
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


