<%@ Page Language="C#" Inherits="AspDotNetStorefrontAdmin.FailedTransactions" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="failedtransactions.aspx.cs" %>

<%@ Import Namespace="AspDotNetStorefrontCore" %>

<%@ Register TagPrefix="listing" Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls.Listing" %>
<%@ Register TagPrefix="filter" TagName="IntegerFilter" Src="Controls/Listing/IntegerFilter.ascx" %>
<%@ Register TagPrefix="filter" TagName="StringFilter" Src="Controls/Listing/StringFilter.ascx" %>
<%@ Register TagPrefix="filter" TagName="BooleanFilter" Src="Controls/Listing/BooleanFilter.ascx" %>
<%@ Register TagPrefix="filter" TagName="DateRangeFilter" Src="Controls/Listing/DateRangeFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="LinkGroupOrders" Src="Controls/LinkGroupOrders.ascx" %>

<asp:Content ContentPlaceHolderID="bodyContentPlaceholder" runat="Server">
	<h1>
		<i class="fa fa-list"></i>
		<asp:Literal ID="litHeader" runat="server" Text="<%$Tokens:StringResource, admin.menu.OrderManage %>" />
		<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.breadcrumbseparator %>" />
		<asp:Literal ID="Literal1" runat="server" Text="<%$Tokens:StringResource, admin.menu.failedtransactions %>" />
	</h1>

	<aspdnsf:AlertMessage ID="AlertMessageDisplay" runat="server" />
	<div class="list-action-bar">
		<aspdnsf:LinkGroupOrders runat="server" ID="LinkGroupOrders" SelectedLink="failedtransactions.aspx" />
	</div>
	<listing:FilteredListing runat="server"
		ID="FilteredListing"
		SqlQuery="select {0} DBRecNo AS ID, CustomerID, OrderNumber, OrderDate, PaymentGateway, PaymentMethod, TransactionCommand, TransactionResult, ExtensionData, MaxMindDetails, IPAddress, MaxMindFraudScore, RecurringSubscriptionID, CustomerEMailed, UpdatedOn, CreatedOn from dbo.FailedTransaction with(nolock) where {1}"
		SortExpression="OrderDate">
		<Filters>
			<filter:DateRangeFilter runat="server"
				FieldName="CreatedOn" />

			<filter:IntegerFilter runat="server"
				Label="<%$Tokens:StringResource, admin.common.OrderNumber %>"
				FieldName="OrderNumber" />

			<filter:StringFilter runat="server"
				Label="<%$Tokens:StringResource, admin.order.PaymentMethod %>"
				FieldName="PaymentMethod" />

			<filter:BooleanFilter runat="server"
				Label="Customer Emailed"
				FieldName="CustomerEmailed" />
		</Filters>
		<ListingTemplate>
			<div class="white-ui-box" style="overflow: auto;">
				<asp:GridView runat="server"
					ID="Grid"
					CssClass="table js-sortable-gridview"
					GridLines="None"
					DataSourceID="FilteredListingDataSource"
					AllowSorting="true"
					AutoGenerateColumns="false"
					DataKeyNames="ID">
					<EmptyDataTemplate>
						<div class="alert alert-info">
							<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.EmptyDataTemplate.NoResults %>" />
						</div>
					</EmptyDataTemplate>
					<Columns>
						<asp:BoundField
							DataField="OrderDate"
							HeaderText="OrderDate"
							SortExpression="OrderDate"></asp:BoundField>
						<asp:HyperLinkField
							DataNavigateUrlFields="CustomerID"
							DataNavigateUrlFormatString="customer.aspx?customerid={0}"
							DataTextField="CustomerID"
							HeaderText="CustomerID"
							SortExpression="CustomerID"></asp:HyperLinkField>
						<asp:HyperLinkField
							DataNavigateUrlFields="OrderNumber"
							DataNavigateUrlFormatString="order.aspx?ordernumber={0}"
							DataTextField="OrderNumber"
							HeaderText="OrderNumber"
							SortExpression="OrderNumber"></asp:HyperLinkField>
						<asp:HyperLinkField
							DataNavigateUrlFields="RecurringSubscriptionID"
							DataNavigateUrlFormatString="recurringgatewaydetails.aspx?RecurringSubscriptionID={0}"
							DataTextField="RecurringSubscriptionID"
							HeaderText="RecurringSubscriptionID"
							SortExpression="RecurringSubscriptionID"></asp:HyperLinkField>
						<asp:BoundField
							DataField="PaymentGateway"
							HeaderText="PaymentGateway"
							SortExpression="PaymentGateway"></asp:BoundField>
						<asp:BoundField
							DataField="PaymentMethod"
							HeaderText="PaymentMethod"
							SortExpression="PaymentMethod"></asp:BoundField>
						<asp:TemplateField
							HeaderText="TransactionCommand"
							SortExpression="TransactionCommand">
							<ItemTemplate>
								<asp:TextBox ID="txtTransactionCommand" runat="server" TextMode="MultiLine" Text='<%# Bind("TransactionCommand") %>' Height="50px" />
							</ItemTemplate>
						</asp:TemplateField>
						<asp:TemplateField
							HeaderText="TransactionResult"
							SortExpression="TransactionResult">
							<ItemTemplate>
								<asp:TextBox ID="txtTransactionResult" runat="server" TextMode="MultiLine" Text='<%# Bind("TransactionResult") %>' Height="50px" />
							</ItemTemplate>
						</asp:TemplateField>
						<asp:BoundField
							DataField="ExtensionData"
							HeaderText="ExtensionData"
							SortExpression="ExtensionData"></asp:BoundField>
						<asp:BoundField
							DataField="MaxMindDetails"
							HeaderText="MaxMindDetails"
							SortExpression="MaxMindDetails"></asp:BoundField>
						<asp:BoundField
							DataField="IPAddress"
							HeaderText="IPAddress"
							SortExpression="IPAddress"></asp:BoundField>
						<asp:BoundField
							DataField="MaxMindFraudScore"
							HeaderText="MaxMindFraudScore"
							SortExpression="MaxMindFraudScore"></asp:BoundField>
						<asp:BoundField
							DataField="CustomerEMailed"
							HeaderText="CustomerEMailed"
							SortExpression="CustomerEMailed"></asp:BoundField>
						<asp:BoundField
							DataField="UpdatedOn"
							HeaderText="UpdatedOn"
							SortExpression="UpdatedOn"></asp:BoundField>
						<asp:BoundField
							DataField="CreatedOn"
							HeaderText="CreatedOn"
							SortExpression="CreatedOn"></asp:BoundField>
					</Columns>
				</asp:GridView>
			</div>
		</ListingTemplate>
	</listing:FilteredListing>

</asp:Content>
