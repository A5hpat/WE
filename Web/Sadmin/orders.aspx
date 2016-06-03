<%@ Page EnableViewState="true" Language="c#" Inherits="AspDotNetStorefrontAdmin.Orders" EnableEventValidation="false" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" ValidateRequest="false" Codebehind="orders.aspx.cs" %>

<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register TagPrefix="aspdnsf" Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls.Listing" %>
<%@ Register TagPrefix="aspdnsf" TagName="StoreSelector" Src="Controls/StoreSelector.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="StringFilter" Src="Controls/Listing/StringFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="IntegerFilter" Src="Controls/Listing/IntegerFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="BooleanFilter" Src="Controls/Listing/BooleanFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="DateRangeFilter" Src="Controls/Listing/DateRangeFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="DataQueryFilter" Src="Controls/Listing/DataQueryFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="ListFilter" Src="Controls/Listing/ListFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="DecimalRangeFilter" Src="Controls/Listing/DecimalRangeFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="MultiFieldStringFilter" Src="Controls/Listing/MultiFieldStringFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="PromotionFilter" Src="Controls/Listing/PromotionFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="OrderItemFilter" Src="Controls/Listing/OrderItemFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="ShippingMethodFilter" Src="Controls/Listing/ShippingMethodFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="LinkGroupOrders" Src="Controls/LinkGroupOrders.ascx" %>

<asp:Content ContentPlaceHolderID="bodyContentPlaceholder" runat="server">
	<div class="admin-module">
		<h1>
			<i class="fa fa-list"></i>
			<asp:Literal ID="litHeader" runat="server" Text="<%$Tokens:StringResource, admin.menu.Orders %>" />
			<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.breadcrumbseparator %>" />
			<asp:Literal ID="Literal1" runat="server" Text="<%$Tokens:StringResource, admin.menu.ordermanage %>" />
		</h1>
		<aspdnsf:FilteredListing runat="server"
			ID="FilteredListing"
			SqlQuery="select {0} OrderNumber, OrderDate, OrderTotal, Email, ShippingPhone, Orders.StoreId, Store.Name [StoreName], IsPrinted, TransactionState, ShippedOn, ShippingState, IsNew FROM Orders left join Store on Orders.StoreID = Store.StoreID WHERE {1}"
			SortExpression="OrderNumber"
			SortDirection="Descending">
			<ActionBarTemplate>
				<aspdnsf:LinkGroupOrders runat="server" ID="LinkGroupOrders" SelectedLink="orders.aspx" />
				<asp:Button runat="server"
					CssClass="btn btn-primary"
					Text="<%$Tokens:StringResource, admin.title.printreceipts %>"
					OnClick="btnPrint_Click" />
			</ActionBarTemplate>
			<Filters>
				<aspdnsf:IntegerFilter runat="server"
					Label="<%$Tokens:StringResource, admin.order.OrderNumberTransactionSubscriptionID %>"
					FieldName="OrderNumber" />

				<aspdnsf:MultiFieldStringFilter runat="server"
					Label="<%$Tokens:StringResource, admin.order.NameSearch %>"
					Fields="FirstName,LastName,Email,BillingFirstName,BillingLastName,BillingCompany,ShippingFirstName,ShippingLastName,ShippingCompany" />

				<aspdnsf:DateRangeFilter runat="server"
					StartLabel="<%$Tokens:StringResource, admin.orders.StartDate %>"
					EndLabel="<%$Tokens:StringResource, admin.orders.EndDate %>"
					FieldName="OrderDate" />
					
				<aspdnsf:BooleanFilter runat="server" FieldName="IsNew" Label="<%$Tokens:StringResource, admin.orders.new %>" />

				<aspdnsf:BooleanFilter runat="server" FieldName="IsPrinted" Label="<%$Tokens:StringResource, admin.orders.printed %>" />

				<aspdnsf:ShippingMethodFilter runat="server"
					Label="Shipping Method"
					FieldName="Orders.ShippingMethod" />
			</Filters>
			<ExpandableFilters>
				<aspdnsf:OrderItemFilter ID="OrderItemFilter1" runat="server"
					Label="<%$Tokens:StringResource, admin.order.SKU %>" />

				<aspdnsf:DecimalRangeFilter ID="NumericRangeFilter1" runat="server"
					StartLabel="<%$Tokens:StringResource, admin.orders.MinimumOrderTotal %>"
					EndLabel="<%$Tokens:StringResource, admin.orders.MaximumOrderTotal %>"
					FieldName="OrderTotal" />

				<aspdnsf:DataQueryFilter runat="server"
					Label="<%$Tokens:StringResource, admin.order.PaymentMethod %>"
					FieldName="PaymentMethod"
					DataQuery="select distinct PaymentMethod from Orders"
					DataTextField="PaymentMethod"
					DataValueField="PaymentMethod" />

				<aspdnsf:DataQueryFilter ID="DataQueryFilter1" runat="server"
					Label="<%$Tokens:StringResource, admin.order.Affiliate %>"
					FieldName="AffiliateId"
					DataQuery="select AffiliateId, Name from Affiliate"
					DataTextField="Name"
					DataValueField="AffiliateId" />

				<aspdnsf:DataQueryFilter ID="DataQueryFilter2" runat="server"
					Label="<%$Tokens:StringResource, admin.order.ForStore %>"
					FieldName="Orders.StoreId"
					DataQuery="select StoreId, Name from Store"
					DataTextField="Name"
					DataValueField="StoreId" />

				<aspdnsf:ListFilter ID="ListFilter2" runat="server"
					Label="<%$Tokens:StringResource, admin.order.TransactionState %>"
					FieldName="TransactionState">
					<Items>
						<asp:ListItem Value="AUTHORIZED" Text='<%$Tokens:StringResource, admin.order.TransactionStateAuthorized %>' />
						<asp:ListItem Value="CAPTURED" Text='<%$Tokens:StringResource, admin.order.TransactionStateCaptured %>' />
						<asp:ListItem Value="VOIDED" Text='<%$Tokens:StringResource, admin.order.TransactionStateVoided %>' />
						<asp:ListItem Value="FORCE VOIDED" Text='<%$Tokens:StringResource, admin.order.TransactionStateForceVoided %>' />
						<asp:ListItem Value="REFUNDED" Text='<%$Tokens:StringResource, admin.order.TransactionStateRefunded %>' />
						<asp:ListItem Value="FRAUD" Text='<%$Tokens:StringResource, admin.order.TransactionStateFraud %>' />
						<asp:ListItem Value="PENDING" Text='<%$Tokens:StringResource, admin.order.TransactionStatePending %>' />
					</Items>
				</aspdnsf:ListFilter>

				<aspdnsf:PromotionFilter ID="PromotionFilter1" runat="server"
					Label="<%$Tokens:StringResource, admin.orders.PromotionCode %>"
					FieldName="OrderNumber"
					PromotionFieldName="OrderId" />

				<aspdnsf:DataQueryFilter ID="DataQueryFilter3" runat="server"
					Label="<%$Tokens:StringResource, admin.order.ShipToState %>"
					FieldName="ShippingState"
					DataQuery="select Abbreviation, Name from State"
					DataTextField="Name"
					DataValueField="Abbreviation" />
			</ExpandableFilters>
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

							<asp:TemplateField
								HeaderText="<%$Tokens:StringResource, admin.common.Contact %>">
								<ItemTemplate>
									<div>
										<asp:HyperLink runat="server"
											Text='<%# Eval("Email") %>'
											NavigateUrl='<%# String.Format("mailto:{0}?subject=RE: {1} order #{2}", Eval("Email"), Eval("StoreName"), Eval("orderNumber")) %>' />
									</div>
									<div>
										<%# Eval("ShippingPhone") %>
									</div>
								</ItemTemplate>
							</asp:TemplateField>

							<asp:TemplateField
								HeaderText="<%$Tokens:StringResource, admin.orders.BulkPrinting.Items %>">
								<ItemTemplate>
									<%# BuildOrderItems((int)Eval("OrderNumber")) %>
								</ItemTemplate>
							</asp:TemplateField>

							<asp:TemplateField
								HeaderText="<%$Tokens:StringResource, admin.orderdetails.OrderStatus %>">
								<ItemTemplate>
									<div>
										<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.order.TransactionState %>" />
										<%# Eval("TransactionState") %>
									</div>
									<div>
										<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.ShippedOn %>" />
										<%# Eval("ShippedOn") == null ? "N/A" : Eval("ShippedOn") %>
									</div>
								</ItemTemplate>
							</asp:TemplateField>
							
							<asp:BoundField HeaderText="<%$Tokens:StringResource, admin.common.StoreName %>" DataField="StoreName" />

							<asp:BoundField
								HeaderText="<%$Tokens:StringResource, admin.orders.BulkPrinting.IsPrinted %>"
								DataField="IsPrinted" />
						</Columns>
					</asp:GridView>
				</div>
			</ListingTemplate>
		</aspdnsf:FilteredListing>
	</div>
</asp:Content>