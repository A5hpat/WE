<%@ Page EnableViewState="true" Language="c#" Inherits="AspDotNetStorefrontAdmin.BulkShipping" EnableEventValidation="false" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" ValidateRequest="false" Codebehind="bulkshipping.aspx.cs" %>

<%@ Register TagPrefix="aspdnsf" Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls.Listing" %>
<%@ Register TagPrefix="aspdnsf" TagName="StringFilter" Src="Controls/Listing/StringFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="IntegerFilter" Src="Controls/Listing/IntegerFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="BooleanFilter" Src="Controls/Listing/BooleanFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="DateRangeFilter" Src="Controls/Listing/DateRangeFilter.ascx" %>

<asp:Content ContentPlaceHolderID="bodyContentPlaceholder" runat="server">
	<div class="admin-module">
		<h1>
			<i class="fa fa-anchor"></i>
			<asp:Literal ID="litHeader" runat="server" Text="<%$Tokens:StringResource, admin.title.bulkshipping %>" />
		</h1>

		<aspdnsf:AlertMessage ID="AlertMessage" runat="server" />

		<aspdnsf:FilteredListing runat="server"
			ID="FilteredListing"
			SqlQuery="select {0} OrderNumber, OrderDate, OrderTotal, Email, IsNew, TransactionState, ShippedOn, ShippedVIA, ShippingTrackingNumber, ShippingMethod FROM Orders WHERE {1}"
			SortExpression="OrderNumber">
			<Filters>
				<aspdnsf:IntegerFilter runat="server"
					Label="<%$Tokens:StringResource, admin.common.OrderNumber %>"
					FieldName="OrderNumber" />

				<aspdnsf:DateRangeFilter runat="server"
					StartLabel="Order Date Start"
					EndLabel="Order Date End"
					FieldName="OrderDate" />

				<aspdnsf:StringFilter runat="server"
					Label="Carrier"
					FieldName="ShippedVIA" />

				<aspdnsf:StringFilter runat="server"
					Label="Shipping Method"
					FieldName="ShippingMethod" />

				<aspdnsf:BooleanFilter runat="server" FieldName="IsNew" Label="New" />

				<aspdnsf:DateRangeFilter runat="server"
					StartLabel="Shipped On Start"
					EndLabel="Shipped On End"
					FieldName="ShippedOn" />

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
						DataKeyNames="OrderNumber"
						AllowSorting="true">
						<Columns>
							<asp:HyperLinkField
								HeaderText="<%$Tokens:StringResource, admin.common.OrderNumber %>"
								DataNavigateUrlFields="OrderNumber"
								DataNavigateUrlFormatString="order.aspx?ordernumber={0}"
								DataTextField="OrderNumber" 
								SortExpression="OrderNumber" />

							<asp:BoundField
								HeaderText="<%$Tokens:StringResource, admin.common.OrderDate %>"
								DataField="OrderDate"
								DataFormatString="{0:d}" 
								SortExpression="OrderDate" />

							<asp:TemplateField
								HeaderText="<%$Tokens:StringResource, admin.common.OrderTotal %>"
								SortExpression="OrderTotal">
								<ItemTemplate>
									<%# ThisCustomer.CurrencyString((decimal)Eval("OrderTotal")) %>
								</ItemTemplate>
							</asp:TemplateField>

							<asp:BoundField
								HeaderText="Shipped On"
								DataField="ShippedOn"
								NullDisplayText="N/A"
								DataFormatString="{0:d}"
								SortExpression="ShippedOn" />

							<asp:TemplateField
								HeaderText="Carrier"
								SortExpression="ShippedVIA" >
								<ItemTemplate>
									<div>
										<%# Eval("ShippedVIA").ToString().Length == 0 ? "N/A" : Eval("ShippedVIA") %>
									</div>
								</ItemTemplate>
							</asp:TemplateField>

							<asp:TemplateField
								HeaderText="Shipping Method">
								<ItemTemplate>
									<div>
										<%# Eval("ShippingMethod").ToString().Length == 0 ? "N/A" : Eval("ShippingMethod") %>
									</div>
								</ItemTemplate>
							</asp:TemplateField>

							<asp:TemplateField
								HeaderText="Tracking Number">
								<ItemTemplate>
									<div>
										<%# Eval("ShippingTrackingNumber").ToString().Length == 0 ? "N/A" : Eval("ShippingTrackingNumber") %>
									</div>
								</ItemTemplate>
							</asp:TemplateField>
						</Columns>

						<EmptyDataTemplate>
							<div class="alert alert-info">
								<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.EmptyDataTemplate.NoResults %>" />
							</div>
						</EmptyDataTemplate>

					</asp:GridView>
				</div>
			</ListingTemplate>
		</aspdnsf:FilteredListing>
		
		<div class="white-ui-box">
			<asp:FileUpload ID="fuShippingImport" CssClass="inline-file-upload" AllowMultiple="false" runat="server" />
			<asp:RequiredFieldValidator ID="rfvImportFile" ControlToValidate="fuShippingImport" ErrorMessage="No File Selected!" runat="server" Display="Dynamic" ValidationGroup="gImport" />

			<asp:Button CausesValidation="true"
				runat="server"
				CssClass="btn btn-primary"
				Text="Import"
				ValidationGroup="gImport"
				OnClick="btnImport_Click" />
			<asp:Button runat="server"
				CssClass="btn btn-primary"
				Text="Export Grid"
				OnClientClick="return confirm('This will export the filtered results of the grid above.  Please be sure to apply any filters you want before continuing.');"
				OnClick="btnExport_Click" />
		</div>
	</div>
</asp:Content>
