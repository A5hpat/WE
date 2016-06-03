<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.orderdetail" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" ValidateRequest="false" MaintainScrollPositionOnPostback="true" Codebehind="order.aspx.cs" %>

<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="bodyContentPlaceholder" runat="server">
	<aspdnsf:ReturnUrlTracker runat="server" ID="ReturnUrlTracker" DefaultReturnUrl="orders.aspx" />
	<h1>
		<i class="fa fa-list"></i>
		<asp:Literal ID="litHeader" runat="server" Text="<%$Tokens:StringResource, admin.orderdetails.OrderInfo %>" />
	</h1>
	<div>
		<aspdnsf:AlertMessage runat="server" ID="ctrlAlertMessage" />
	</div>
	<div class="main-content-wrapper" id="editOrderWrap">
		<div id="divMessage" runat="server" visible="false">
			<asp:Label runat="server" ID="lblMessage" />
		</div>

		<asp:Panel ID="pnlOrderDetails" runat="server" Visible="true">
			<div class="item-action-bar">
				<asp:HyperLink runat="server" ID="btnCloseTop" CssClass="btn btn-default" NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>" Text="<%$Tokens:StringResource, admin.common.close %>" />
				<asp:Button ID="btnViewReceipt" CssClass="btn btn-primary" Text="<%$Tokens:StringResource, admin.orderframe.OpenNewReceiptWindowHere %>" runat="server" OnClick="btnViewReceipt_Click" />
			</div>

			<div id="divOrderContents" runat="server" class="white-ui-box" visible="true">
				<div class="row form-inline">
					<div class="col-md-6">
						<div class="white-box-heading">
							<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderdetails.OrderInfo %>" />
						</div>
						<div class="row">
							<div class="col-md-4">
							 Order Number: 
							</div>
							<div class="col-md-8">
								<asp:Label runat="server" ID="lblOrderNumber" />
							</div>
						</div>
						<div class="row">
							<div class="col-md-4">
								<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.OrderDate %>" />
							</div>
							<div class="col-md-8">
								<asp:Literal ID="litOrderDate" runat="server" />
							</div>
						</div>
						<div id="divStore" class="row" runat="server" visible="false">
							<div class="col-md-4">
								<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.reports.forstore %>" />
							</div>
							<div class="col-md-8">
								<asp:Literal ID="litStoreName" runat="server" />
								(<asp:Literal ID="litStoreId" runat="server" />)
							</div>
						</div>
						<div class="row">
							<div class="col-md-4">
								<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.OrderTotal %>" />
							</div>
							<div class="col-md-8">
								<asp:Literal runat="server" ID="litOrderTotal" />
							</div>
						</div>
						<div id="divLocale" class="row" runat="server" visible="false">
							<div class="col-md-4">
								<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.LocaleSetting %>" />
							</div>
							<div class="col-md-8">
								<asp:Literal ID="litCustomerLocale" runat="server" />
							</div>
						</div>
					</div>
					<div class="col-md-6">
						<div class="white-box-heading">
							<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderdetails.CustomerInfo %>" />
						</div>
						<div class="row">
							<div class="col-md-4">
								<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.customer.CustomerID %>" />
							</div>
							<div class="col-md-8">
								<asp:Literal ID="litCustomerID" runat="server" />
								<asp:HyperLink runat="server" ID="lnkOrderHistory" />
							</div>
						</div>
						<div class="row">
							<div class="col-md-4">
								<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.order.CustomerName %>" />
							</div>
							<div class="col-md-8">
								<asp:HyperLink ID="lnkCustomerName" runat="server" />
							</div>
						</div>
						<div class="row">
							<div class="col-md-4">
								<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.CustomerPhone %>" />
							</div>
							<div class="col-md-8">
								<asp:Literal ID="litCustomerPhone" runat="server" />
							</div>
						</div>
						<div class="row">
							<div class="col-md-4">
								<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.order.CustomerEmail %>" />
							</div>
							<div class="col-md-8">
								<asp:TextBox CssClass="form-control" ID="txtCustomerEmail" runat="server" />
								<aspdnsf:EmailValidator ID="valCustomerEmail" CssClass="text-danger" ControlToValidate="txtCustomerEmail" Display="Dynamic" Text="<%$Tokens:StringResource, admin.customer.EmailValidationFailed %>" runat="server" />
								<asp:Button ID="btnChangeEmail" CssClass="btn btn-primary" Text="<%$Tokens:StringResource, admin.orderframe.ChangeOrderEmail %>" runat="server" OnClick="btnChangeEmail_Click" />
							</div>
						</div>
						<div class="row">
							<div class="col-md-4">
								<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderdetails.RegisteredOn %>" />
							</div>
							<div class="col-md-8">
								<asp:Literal ID="litCustomerRegisterDate" runat="server" />
							</div>
						</div>
						<div class="row">
							<div class="col-md-4">
								<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.AffiliateID %>" />
							</div>
							<div class="col-md-8">
								<asp:Literal ID="litAffiliateID" runat="server" />
							</div>
						</div>
						<div class="row">
							<div class="col-md-4">
								<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.Referrer %>" />
							</div>
							<div class="col-md-8">
								<asp:Literal ID="litReferrer" runat="server" />
							</div>
						</div>
					</div>
				</div>
			</div>

			<div class="row">
				<div id="divLeftColumn" runat="server" class="col-md-6" visible="true">
					<div id="divAddresses" class="white-ui-box">
						<div class="row">
							<div class="col-md-6">
								<div class="white-box-heading">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderdetails.BillingAddress %>" />
								</div>
							</div>
							<div class="col-md-6">
								<div class="white-box-heading">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderdetails.ShippingAddress %>" />
								</div>
							</div>
						</div>
						<div class="row">
							<div class="col-md-6">
								<asp:Literal ID="litBillingAddress" runat="server" />
							</div>
							<div class="col-md-6">
								<asp:Literal ID="litShippingAddress" runat="server" />
							</div>
						</div>
					</div>

					<div id="divDeliveryInfo" class="white-ui-box">
						<div class="white-box-heading">
							<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderdetails.DeliveryInfo %>" />
						</div>
						<div id="divShippingDeliveryInfo" visible="false" runat="server">
							<div class="row">
								<div class="col-md-4">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.OrderWeight %>" />
								</div>
								<div class="col-md-8">
									<asp:TextBox CssClass="text-sm" runat="server" ID="txtOrderWeight" />
									<asp:CompareValidator ID="valOrderWeight" runat="server" CssClass="text-danger" ControlToValidate="txtOrderWeight" Type="Currency" Text="<%$Tokens:StringResource, admin.orderdetails.ValidDecimal %>" Operator="DataTypeCheck" Display="Dynamic" />
									<asp:Button ID="btnAdjustOrderWeight" runat="server" CssClass="btn btn-primary" OnClick="btnAdjustOrderWeight_Click" Text="<%$Tokens:StringResource, admin.orderframe.AdjustWeight %>" />
								</div>
							</div>
							<div class="row">
								<div class="col-md-4">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.ShippingMethod %>" />
								</div>
								<div class="col-md-8">
									<asp:Literal runat="server" ID="litShippingMethod" />
								</div>
							</div>
							<div class="row">
								<div class="col-md-4">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.ShippingPricePaid %>" />
								</div>
								<div class="col-md-8">
									<asp:Literal runat="server" ID="litShippingPricePaid" />
								</div>
							</div>
							<div class="row">
								<div class="col-md-4">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.ShippedOn %>" />
								</div>
								<div class="col-md-8">
									<telerik:RadDatePicker Width="200px" InputMode="DateTimePicker" SelectedDate='<%# System.DateTime.Now %>' ID="dpShippedOn" runat="server" MaxDate="9999-12-31" MinDate="0001-01-01">
										<Calendar runat="server" UseColumnHeadersAsSelectors="False" UseRowHeadersAsSelectors="False" ViewSelectorText="x" />
									</telerik:RadDatePicker>
									<asp:Button ID="btnMarkAsShipped" ValidationGroup="markAsShipped" Enabled="false" runat="server" CssClass="btn btn-primary" OnClick="btnMarkAsShipped_Click" Text="<%$Tokens:StringResource, admin.orderframe.MarkAsShipped %>" />
									<asp:RequiredFieldValidator CssClass="text-danger" ErrorMessage="<%$Tokens:StringResource, admin.common.FillinShippedDate %>" Display="Dynamic" ControlToValidate="dpShippedOn" ID="rfvDpShippedOn" SetFocusOnError="true" runat="server" ValidationGroup="markAsShipped" />
								</div>
							</div>
							<div class="row">
								<div class="col-md-4">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.Carrier %>" />
								</div>
								<div class="col-md-8">
									<asp:TextBox CssClass="form-control" runat="server" ID="txtShippedVia" />
								</div>
							</div>
							<div class="row">
								<div class="col-md-4">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.TrackingNumber %>" />
								</div>
								<div class="col-md-8">
									<asp:TextBox CssClass="form-control" runat="server" ID="txtTrackingNumber" />
								</div>
							</div>
							<div id="divMultiShip" runat="server" class="row form-group" visible="false">
								<div class="col-md-4">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.HasMultipleShippingAddresses %>" />
								</div>
								<div class="col-md-8">
									<asp:Literal runat="server" ID="litMultipleShippingAddresses" />
								</div>
							</div>
							<div class="row">
								<div class="col-md-8 col-md-offset-3">
									<asp:Button ID="btnSendToShipManager" Enabled="false" runat="server" CssClass="btn btn-primary" OnClick="btnSendToShipManager_Click" />
								</div>
							</div>
						</div>
						<div id="divDownloadDeliveryInfo" visible="false" runat="server">
							<div class="row">
								<div class="col-md-4">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.HasDownloadItems %>" />
								</div>
								<div class="col-md-8">
									<asp:Literal ID="litHasDownloadItems" runat="server" />
								</div>
							</div>
							<div class="row">
								<div class="col-md-4">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.AllDownloadItems %>" />
								</div>
								<div class="col-md-8">
									<asp:Literal ID="litAllDownloaditems" runat="server" />
								</div>
							</div>
							<div>
								<div class="admin-table-wrap">
									<div class="table-row-inner">
										<asp:GridView ID="grdDownloadItems" CssClass="table table-detail" ShowHeader="true" PageIndex="0" AllowPaging="true" PageSize="3" OnRowDataBound="grdDownloadItems_OnRowDataBound"
											runat="server" GridLines="None" CellSpacing="-1" OnPageIndexChanging="grdDownloadItems_OnPageIndexChanging" AutoGenerateColumns="false">
											<Columns>
												<asp:TemplateField>
													<HeaderTemplate>
														<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderdetails.DownloadValidDates %>" />
													</HeaderTemplate>
													<ItemTemplate>
														<div>
															<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.DownloadReleasedOn %>" />
															<asp:Literal runat="server" ID="litDownloadReleasedOn" />
														</div>
														<div>
															<asp:Literal runat="server" ID="litDownloadExpirationLabel" />
															<asp:Literal runat="server" ID="litDownloadExpiresOn" />
														</div>
													</ItemTemplate>
												</asp:TemplateField>
												<asp:TemplateField>
													<HeaderTemplate>
														<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.DownloadFile %>" />
													</HeaderTemplate>
													<ItemTemplate>
														<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderdetails.DownloadFileLocation %>" />
														<asp:TextBox runat="server" CssClass="form-control" ID="txtDownloadLocation" />
														<asp:Button ID="btnReleaseDownload" CssClass="btn btn-primary" runat="server" CommandArgument="<%# (Container.DataItem as AspDotNetStorefrontCore.DownloadItem).ShoppingCartRecordId %>" OnClick="btnReleaseDownload_Click" Text="<%$Tokens:StringResource, admin.orderframe.DownloadReleaseItem %>" />
													</ItemTemplate>
												</asp:TemplateField>
											</Columns>
										</asp:GridView>
									</div>
									<div runat="server" visible="false" id="divDelayedDownloadWarning" class="alert alert-danger">
										<asp:Literal runat="server" Visible="false" ID="litDelayedDownloadWarning" Text="<%$Tokens:StringResource, download.aspx.17 %>" />
									</div>
								</div>
							</div>
						</div>
						<div id="divDistributorDeliveryInfo" visible="false" runat="server">
							<div class="row">
								<div class="col-md-4">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.HasDistributorDropShipItems %>" />
								</div>
								<div class="col-md-8">
									<asp:Literal ID="litHasDistributorItems" runat="server" />
								</div>
							</div>
							<div class="row">
								<div class="col-md-4">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.DistributorEmailSentOn %>" />
								</div>
								<div class="col-md-8">
									<asp:Literal ID="litDistributorEmailSentOn" runat="server" />
									<asp:Button ID="btnSendDistributorEmail" runat="server" CssClass="btn btn-primary" OnClick="btnSendDistributorEmail_Click" Text="<%$Tokens:StringResource, admin.orderframe.SendTrackingNumber %>" />
								</div>
							</div>
							<div>
								<a href="javascript:void(0);" class="btn btn-primary" onclick="toggleDistributor();">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderdetails.DistributorInfo %>"></asp:Literal></a>
							</div>
							<div id="divDistributorInfo" style="display: none;">
								<div>
									<asp:Literal ID="litDistributorNotifications" runat="server" />
								</div>
							</div>
						</div>
					</div>

					<div id="divNotes" class="white-ui-box">
						<div class="white-box-heading">
							<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.Notes %>" />
						</div>
						<div class="form-group">
							<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.OrderNotes %>" />
							<asp:TextBox TextMode="MultiLine" Rows="2" runat="server" ID="txtOrderNotes" CssClass="form-control disabled" />
						</div>
						<div class="form-group">
							<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.PrivateNotes %>" />
							<asp:TextBox TextMode="MultiLine" Rows="2" runat="server" ID="txtAdminNotes" CssClass="form-control" />
						</div>
						<div class="form-group">
							<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.CustomerServiceNotes %>" />&nbsp;<asp:Literal runat="server" ID="litCustomerServiceVisible" />
							<asp:TextBox TextMode="MultiLine" Rows="2" runat="server" ID="txtCustomerServiceNotes" CssClass="form-control" />
						</div>
						<div class="form-group">
							<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.FinalizationData %>" />
							<asp:TextBox TextMode="MultiLine" Rows="2" runat="server" ID="txtFinalizationData" CssClass="form-control disabled" />
						</div>
						<asp:Button ID="btnUpdateNotes" CssClass="btn btn-primary" runat="server" OnClick="btnUpdateNotes_Click" Text="<%$Tokens:StringResource, admin.common.Submit %>" />
					</div>
				</div>

				<div id="divRightColumn" runat="server" class="col-md-6" visible="true">
					<div id="divItemInfo" class="white-ui-box">
						<div class="white-box-heading">
							<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderdetails.ProductDetails %>" />
						</div>
						<div>
							<div>
								<div class="table-row-inner">
									<asp:GridView ID="grdProducts" CssClass="table table-detail" ShowHeader="true" PageIndex="0" AllowPaging="true" PageSize="5"
										runat="server" GridLines="None" CellSpacing="-1" OnPageIndexChanging="grdProducts_OnPageIndexChanging" AutoGenerateColumns="false">
										<Columns>
											<asp:TemplateField>
												<HeaderTemplate>
													<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.ID %>" />
												</HeaderTemplate>
												<ItemTemplate>
													<asp:Label runat="server" Text='<%# Eval("ProductID") %>' />
												</ItemTemplate>
											</asp:TemplateField>
											<asp:TemplateField>
												<HeaderTemplate>
													<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.SKU %>" />
												</HeaderTemplate>
												<ItemTemplate>
													<asp:Label runat="server" Text='<%# Eval("SKU") %>' />
												</ItemTemplate>
											</asp:TemplateField>
											<asp:TemplateField>
												<HeaderTemplate>
													<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.ProductName %>" />
												</HeaderTemplate>
												<ItemTemplate>
													<asp:Label runat="server" Text='<%# Eval("ProductName") + " - " + Eval("VariantName") %>' />
												</ItemTemplate>
											</asp:TemplateField>
											<asp:TemplateField>
												<HeaderTemplate>
													<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.Size %>" />
												</HeaderTemplate>
												<ItemTemplate>
													<asp:Label runat="server" Text='<%# AspDotNetStorefrontCore.AppLogic.CleanSizeColorOption(Eval("ChosenSize").ToString()) %>' />
												</ItemTemplate>
											</asp:TemplateField>
											<asp:TemplateField>
												<HeaderTemplate>
													<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.Color %>" />
												</HeaderTemplate>
												<ItemTemplate>
													<asp:Label runat="server" Text='<%# AspDotNetStorefrontCore.AppLogic.CleanSizeColorOption(Eval("ChosenColor").ToString()) %>' />
												</ItemTemplate>
											</asp:TemplateField>
											<asp:TemplateField>
												<HeaderTemplate>
													<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderdetails.Qty %>" />
												</HeaderTemplate>
												<ItemTemplate>
													<asp:Label runat="server" Text='<%# Eval("Quantity") %>' />
												</ItemTemplate>
											</asp:TemplateField>
										</Columns>
									</asp:GridView>
								</div>
							</div>
						</div>
					</div>

					<div id="divPaymentInfo" class="white-ui-box">
						<div class="white-box-heading">
							<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderdetails.PaymentInfo %>" />
						</div>
						<div>
							<div class="row">
								<div class="col-md-4">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderdetails.ReturnActions %>" />
								</div>
								<div class="col-md-8">
									<asp:DropDownList CssClass="form-control" ID="ddlCancelActions" runat="server" Enabled="false" ClientIDMode="Static" />
									<asp:Button ID="btnCancel" Enabled="false" runat="server" CssClass="btn btn-primary" OnClick="btnCancel_Click" Text="<%$Tokens:StringResource, admin.common.Submit %>" />
								</div>
							</div>
							<div runat="server" id="divMaxMind" visible="false" class="row">
								<div class="col-md-4">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.MaxMindFraudScore %>" />
								</div>
								<div class="col-md-8">
									<asp:Literal runat="server" ID="litMaxMindScore" Text="--" />&nbsp;<asp:HyperLink ID="lnkMaxMindDetails" runat="server" Target="_blank" Text="<%$Tokens:StringResource, admin.orderframe.MaxMindFraudExplanation %>" />
									<asp:Button ID="btnGetMaxMind" Enabled="false" runat="server" CssClass="btn btn-primary" OnClick="btnGetMaxMind_Click" Text="<%$Tokens:StringResource, admin.orderframe.GetMaxMind %>" />
								</div>
							</div>
							<div id="divParentOrder" visible="false" runat="server" class="row">
								<div class="col-md-4">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.ParentOrder %>" />
								</div>
								<div class="col-md-8">
									<asp:HyperLink runat="server" ID="lnkParentOrder" />
								</div>
							</div>
							<div id="divRelatedOrder" visible="false" runat="server" class="row">
								<div class="col-md-4">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.RelatedOrder %>" />
								</div>
								<div class="col-md-8">
									<asp:HyperLink runat="server" ID="lnkRelatedOrder" />
								</div>
							</div>
							<div id="divChildOrders" visible="false" runat="server" class="row">
								<div class="col-md-4">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderdetails.ChildOrders %>" />
								</div>
								<div class="col-md-8">
									<asp:Repeater ID="rptChildOrders" runat="server" OnItemDataBound="rptChildOrders_ItemDataBound">
										<ItemTemplate>
											<div><asp:HyperLink ID="childLink" runat="server" Text='<%# Eval("OrderNumber") %>' Target="_blank" /></div>
										</ItemTemplate>
									</asp:Repeater>
								</div>
							</div>
							<div class="row">
								<div class="col-md-4">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.order.PaymentMethod %>" />
								</div>
								<div class="col-md-8">
									<asp:Literal runat="server" ID="litPaymentMethod" />
								</div>
							</div>
							<div class="row">
								<div class="col-md-4">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.default.PaymentGateway %>" />
								</div>
								<div class="col-md-8">
									<asp:Literal runat="server" ID="litPaymentGateway" />
								</div>
							</div>
							<div class="row">
								<div class="col-md-4">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.AVSResult %>" />
								</div>
								<div class="col-md-8">
									<asp:Literal runat="server" ID="litAVSResult" />
								</div>
							</div>
							<div class="row">
								<div class="col-md-4">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.order.TransactionState %>" />
								</div>
								<div class="col-md-8">
									<asp:Literal runat="server" ID="litTransactionState" />
									<asp:Button ID="btnCapture" OnClientClick="return confirm('Are you sure you want to capture the order?');" Enabled="false" CssClass="btn btn-primary" Text="<%$Tokens:StringResource, admin.orderframe.Capture %>" runat="server" OnClick="btnCapture_Click" />
								</div>
							</div>
							<div class="row">
								<div class="col-md-4">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.AuthorizedOn %>" />
								</div>
								<div class="col-md-8">
									<asp:Literal runat="server" ID="litAuthorizedOn" />
								</div>
							</div>
							<div class="row">
								<div class="col-md-4">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.ReceiptEmailSentOn %>" />
								</div>
								<div class="col-md-8">
									<asp:Literal runat="server" ID="litReceiptSentOn" />
									<asp:Button ID="btnReceiptEmail" Enabled="false" CssClass="btn btn-primary" runat="server" OnClick="btnReceiptEmail_Click" />
									<asp:Button ID="btnRegenerateReceipt" CssClass="btn btn-primary" Text="<%$Tokens:StringResource, admin.orderdetails.RegenerateReceipt %>" runat="server" OnClick="btnRegenerateReceipt_Click" />
								</div>
							</div>
							<div class="row">
								<div class="col-md-4">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.CapturedOn %>" />
								</div>
								<div class="col-md-8">
									<asp:Literal runat="server" ID="litCapturedOn" />
								</div>
							</div>
							<div class="row">
								<div class="col-md-4">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.RefundedOn %>" />
								</div>
								<div class="col-md-8">
									<asp:Literal runat="server" ID="litRefundedOn" />
								</div>
							</div>
							<div class="row">
								<div class="col-md-4">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.VoidedOn %>" />
								</div>
								<div class="col-md-8">
									<asp:Literal runat="server" ID="litVoidedOn" />
								</div>
							</div>
							<div class="row">
								<div class="col-md-4">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.FraudedOn %>" />
								</div>
								<div class="col-md-8">
									<asp:Literal runat="server" ID="litFraudedOn" />
								</div>
							</div>
							<div id="divCCInfo" visible="false" runat="server">
								<div class="row">
									<div class="col-md-4">
										<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.CardType %>" />
									</div>
									<div class="col-md-8">
										<asp:Literal runat="server" ID="litCCType" />
									</div>
								</div>
								<div class="row">
									<div class="col-md-4">
										<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.CardNumber %>" />
									</div>
									<div class="col-md-8">
										<asp:Literal runat="server" ID="litCCNumber" />
									</div>
								</div>
								<div class="row">
									<div class="col-md-4">
										<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.LastFour %>" />
									</div>
									<div class="col-md-8">
										<asp:Literal runat="server" ID="litCCLastFour" />
									</div>
								</div>
								<div class="row">
									<div class="col-md-4">
										<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.CardExpiration %>" />
									</div>
									<div class="col-md-8">
										<asp:Literal runat="server" ID="litCCExpirationDate" />
									</div>
								</div>
								<div id="divCCIssueInfo" runat="server" visible="false">
									<div class="row">
										<div class="col-md-4">
											<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.CardIssueNumber %>" />
										</div>
										<div class="col-md-8">
											<asp:Literal runat="server" ID="litCCIssueNumber" />
										</div>
									</div>
									<div class="row">
										<div class="col-md-4">
											<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.CardStartDate %>" />
										</div>
										<div class="col-md-8">
											<asp:Literal runat="server" ID="litCCStartDate" />
										</div>
									</div>
								</div>
							</div>
							<div id="divPayPalInfo" runat="server" visible="false">
								<div class="form-group">
									<asp:Button ID="btnPayPalReauth" CssClass="btn btn-primary" runat="server" Text="<%$Tokens:StringResource, admin.orderframe.Reauthorize %>" OnClick="btnPayPalReauth_Click" />
								</div>
							</div>
						</div>
					</div>

					<div id="divPromoInfo" class="white-ui-box">
						<div class="white-box-heading">
							<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.PromotionUsage %>" />
						</div>
						<div>
							<div>
								<div class="table-row-inner">
									<asp:GridView ID="grdPromotions" CssClass="table table-detail" ShowHeader="true" PageIndex="0" AllowPaging="true" PageSize="5"
										runat="server" GridLines="None" CellSpacing="-1" OnPageIndexChanging="grdPromotions_OnPageIndexChanging" AutoGenerateColumns="false">
										<Columns>
											<asp:TemplateField>
												<HeaderTemplate>
													<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.PromotionCode %>" />
												</HeaderTemplate>
												<ItemTemplate>
													<asp:Label runat="server" Text='<%# Eval("Code") %>' />
												</ItemTemplate>
											</asp:TemplateField>
											<asp:TemplateField>
												<HeaderTemplate>
													<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.ShippingDiscount %>" />
												</HeaderTemplate>
												<ItemTemplate>
													<asp:Label runat="server" Text='<%# Eval("ShippingDiscount") %>' />
												</ItemTemplate>
											</asp:TemplateField>
											<asp:TemplateField>
												<HeaderTemplate>
													<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.LineItemDiscount %>" />
												</HeaderTemplate>
												<ItemTemplate>
													<asp:Label runat="server" Text='<%# Eval("LineItemDiscount") %>' />
												</ItemTemplate>
											</asp:TemplateField>
											<asp:TemplateField>
												<HeaderTemplate>
													<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.OrderLevelDiscount %>" />
												</HeaderTemplate>
												<ItemTemplate>
													<asp:Label runat="server" Text='<%# Eval("OrderDiscount") %>' />
												</ItemTemplate>
											</asp:TemplateField>
											<asp:TemplateField>
												<HeaderTemplate>
													<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.GiftWPurchase %>" />
												</HeaderTemplate>
												<ItemTemplate>
													<asp:CheckBox runat="server" Enabled="false" Checked='<%# (bool)Eval("GiftWithPurchase") %>' />
												</ItemTemplate>
											</asp:TemplateField>
											<asp:TemplateField>
												<HeaderTemplate>
													<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.TotalDiscount %>" />
												</HeaderTemplate>
												<ItemTemplate>
													<asp:Label runat="server" Text='<%# Eval("TotalDiscount") %>' />
												</ItemTemplate>
											</asp:TemplateField>
										</Columns>
									</asp:GridView>
								</div>
							</div>
						</div>
					</div>

					<div id="divDebugInfo" class="white-ui-box">
						<div class="white-box-heading">
							<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderdetails.DebugInfo %>" />
						</div>
						<div id="divTransactionInfoWrap" runat="server" visible="false">
							<div>
								<a href="javascript:void(0);" class="btn btn-primary" onclick="toggleTransactions();">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.TransactionCommand %>"></asp:Literal></a>
							</div>
							<div id="divTransactionInfo" style="display: none;">
								<div id="divTransactionCommand" runat="server" visible="false">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.TransactionCommand %>" />
									<asp:TextBox TextMode="MultiLine" CssClass="form-control disabled" Rows="4" ID="txtTransactionCommand" runat="server" />
								</div>
								<div id="divAuthorizationResult" runat="server" visible="false">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.AuthorizationResult %>" />
									<asp:TextBox TextMode="MultiLine" CssClass="form-control disabled" Rows="4" ID="txtAuthorizationResult" runat="server" />
								</div>
								<div id="divAuthorizationCode" runat="server" visible="false">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.AuthorizationCode %>" />
									<asp:TextBox TextMode="MultiLine" CssClass="form-control disabled" Rows="4" ID="txtAuthorizationCode" runat="server" />
								</div>
								<div id="divCaptureCommand" runat="server" visible="false">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.CaptureTXCommand %>" />
									<asp:TextBox TextMode="MultiLine" CssClass="form-control disabled" Rows="4" ID="txtCaptureCommand" runat="server" />
								</div>
								<div id="divCaptureResult" runat="server" visible="false">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.CaptureTXResult %>" />
									<asp:TextBox TextMode="MultiLine" CssClass="form-control disabled" Rows="4" ID="txtCaptureResult" runat="server" />
								</div>
								<div id="divVoidCommand" runat="server" visible="false">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.VoidTXCommand %>" />
									<asp:TextBox TextMode="MultiLine" CssClass="form-control disabled" Rows="4" ID="txtVoidCommand" runat="server" />
								</div>
								<div id="divVoidResult" runat="server" visible="false">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.VoidTXResult %>" />
									<asp:TextBox TextMode="MultiLine" CssClass="form-control disabled" Rows="4" ID="txtVoidResult" runat="server" />
								</div>
								<div id="divRefundCommand" runat="server" visible="false">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.RefundTXCommand %>" />
									<asp:TextBox TextMode="MultiLine" CssClass="form-control disabled" Rows="4" ID="txtRefundCommand" runat="server" />
								</div>
								<div id="divRefundResult" runat="server" visible="false">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.RefundTXResult %>" />
									<asp:TextBox TextMode="MultiLine" CssClass="form-control disabled" Rows="4" ID="txtRefundResult" runat="server" />
								</div>
								<div id="divCardinalLookup" runat="server" visible="false">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.CardinalLookupResult %>" />
									<asp:TextBox TextMode="MultiLine" CssClass="form-control disabled" Rows="4" ID="txtCardinalLookup" runat="server" />
								</div>
								<div id="divCardinalAuthenticate" runat="server" visible="false">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.CardinalAuthenticateResult %>" />
									<asp:TextBox TextMode="MultiLine" CssClass="form-control disabled" Rows="4" ID="txtCardinalAuthenticate" runat="server" />
								</div>
								<div id="div3dSecure" runat="server" visible="false">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.3DSecureLookupResult %>" />
									<asp:TextBox TextMode="MultiLine" CssClass="form-control disabled" Rows="4" ID="txtThreedSecure" runat="server" />
								</div>
							</div>
						</div>

						<div id="divXmlInfoWrap" runat="server" visible="false">
							<div>
								<a href="javascript:void(0);" class="btn btn-primary" onclick="toggleXml();">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.title.orderXML %>"></asp:Literal></a>
							</div>
							<div id="divXmlInfo" style="display: none;">
								<div>
									<asp:TextBox TextMode="MultiLine" CssClass="form-control disabled" Rows="20" ID="litOrderXml" runat="server" />
								</div>
							</div>
						</div>

						<div id="divShippingInfoWrap" runat="server">
							<div>
								<a href="javascript:void(0);" class="btn btn-primary" onclick="toggleShipping();">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.RTShippingInfo %>"></asp:Literal></a>
							</div>
							<div id="divShippingInfo" style="display: none;">
								<div>
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderdetails.RTShippingRequest %>" />
									<asp:TextBox TextMode="MultiLine" CssClass="form-control disabled" Rows="10" ID="txtRTShippingRequest" runat="server" />
								</div>
								<div>
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderdetails.RTShippingResponse %>" />
									<asp:TextBox TextMode="MultiLine" CssClass="form-control disabled" Rows="10" ID="txtRTShippingResponse" runat="server" />
								</div>
							</div>
						</div>

						<div id="divRecurringInfoWrap" runat="server" visible="false">
							<div>
								<a href="javascript:void(0);" class="btn btn-primary" onclick="toggleRecurring();">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.title.recurringgatewaydetails %>"></asp:Literal></a>
							</div>
							<div id="divRecurringInfo" style="display: none;">
								<div>
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.GatewayAutoBillSubscriptionID %>" />
									<asp:Literal runat="server" ID="litRecurringSubscriptionId" Visible="false" />
									<asp:HyperLink ID="lnkRecurringStatus" runat="server" Visible="false" Target="_blank" />
								</div>
								<div>
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.RecurringBillingSubscriptionCreateCommand %>" />
									<asp:TextBox TextMode="MultiLine" CssClass="form-control disabled" Rows="10" ID="txtRecurringCommand" runat="server" />
								</div>
								<div>
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderframe.RecurringBillingSubscriptionCreateResult %>" />
									<asp:TextBox TextMode="MultiLine" CssClass="form-control disabled" Rows="10" ID="txtRecurringResult" runat="server" />
								</div>
							</div>
						</div>
					</div>
				</div>
			</div>

			<div class="item-action-bar">
				<asp:HyperLink runat="server" ID="btnClose" CssClass="btn btn-default" NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>" Text="<%$Tokens:StringResource, admin.common.close %>" />
				<asp:Button ID="btnViewReceiptBottom" CssClass="btn btn-primary" Text="<%$Tokens:StringResource, admin.orderframe.OpenNewReceiptWindowHere %>" runat="server" OnClick="btnViewReceipt_Click" />
			</div>
		</asp:Panel>
	</div>

	<script type="text/javascript">
		function toggleTransactions() {
			$("#divTransactionInfo").toggle();
		}
		function toggleXml() {
			$("#divXmlInfo").toggle();
		}
		function toggleShipping() {
			$("#divShippingInfo").toggle();
		}
		function toggleRecurring() {
			$("#divRecurringInfo").toggle();
		}
		function toggleDistributor() {
			$("#divDistributorInfo").toggle();
		}
	</script>
</asp:Content>
