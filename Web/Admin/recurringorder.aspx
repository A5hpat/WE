<%@ Page Language="c#" Inherits="AspDotNetStorefrontAdmin.recurringorder" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" CodeBehind="recurringorder.aspx.cs" %>

<%@ Register TagPrefix="telerik" Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<aspdnsf:ReturnUrlTracker runat="server" ID="ReturnUrlTracker" DefaultReturnUrl="recurringorders.aspx" />
	<aspdnsf:AlertMessage ID="AlertMessage" runat="server" />
	<h1><i class="fa fa-list"></i>
		<asp:Literal ID="litHeader" Text="<%$Tokens:StringResource, admin.title.recurringorder %>" runat="server" /></h1>

	<div runat="server" id="divContent">
		<div class="list-action-bar">
			<asp:HyperLink runat="server" ID="btnCloseTop" CssClass="btn btn-default" NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>" Text="<%$Tokens:StringResource, admin.common.close %>" />
			<asp:LinkButton runat="server" ID="lnkRetry" CssClass="btn btn-default" Visible="false" Text="Retry Payment"
				OnClientClick="return confirm('Are you sure you want to retry the payment on this Recurring AutoBill order?');" />
			<asp:LinkButton runat="server" ID="lnkRestart" CssClass="btn btn-default" Visible="false" Text="Restart Payment"
				OnClientClick="return confirm('Are you sure you want to restart the payment on this Recurring AutoBill order?');" />
			<asp:Button runat="server" ID="btnStopBilling" CssClass="btn btn-default" Text="Stop Future Billing" OnClick="btnStopBilling_Click"
				OnClientClick="return confirm('Are you sure you want to stop future billing & shipment for this order?');" />
			<asp:Button runat="server" ID="btnProcess" CssClass="btn btn-primary" Text="Process Order" OnClick="btnProcess_Click" />
		</div>

		<div class="admin-row">
			<div id="divOrderInfo" class="white-ui-box">
				<div class="row">
					<div class="col-md-4">
						<div class="form-group form-inline">
							<asp:Label runat="server" Text="Original Recurring Order Number:" />
							<asp:Literal runat="server" ID="litOrderNumber" />
						</div>
						<div class="form-group form-inline">
							<asp:Label runat="server" Text="Recurring Index:" />
							<asp:Literal runat="server" ID="litRecurringIndex" />
						</div>
						<div class="form-group form-inline">
							<asp:Label runat="server" Text="Original Order Date:" />
							<asp:Literal runat="server" ID="litOriginalDate" />
						</div>
						<div class="form-group form-inline">
							<asp:Label runat="server" Text="Next Ship Date:" />
							<telerik:RadDatePicker Width="100%" InputMode="DatePicker" ID="dpNextShipDate" runat="server" MaxDate="9999-12-31" MinDate="0001-01-01">
								<Calendar runat="server" UseColumnHeadersAsSelectors="False" UseRowHeadersAsSelectors="False" ViewSelectorText="x" />
							</telerik:RadDatePicker>
						</div>
						<div class="form-group form-inline">
							<asp:Label runat="server" Text="Recurring Interval:" />
							<asp:TextBox runat="server" ID="txtInterval" /><br />
							<asp:CompareValidator Operator="DataTypeCheck" Type="Integer" ErrorMessage="Please enter an integer value!" ControlToValidate="txtInterval" runat="server" Display="Dynamic" ValidationGroup="Main" />
						</div>
						<div class="form-group form-inline">
							<asp:Label runat="server" Text="Interval Type:" />
							<asp:DropDownList runat="server" ID="ddIntervalType" AutoPostBack="false" />
						</div>
						<div class="form-group form-inline">
							<asp:Button runat="server" ID="btnUpdate" ValidationGroup="Main" CssClass="btn btn-primary" Text="<%$Tokens:StringResource, admin.common.Update %>" OnClick="btnUpdate_Click" />
						</div>
						<asp:Panel runat="server" ID="pnlDateEditNotice" Visible="false" CssClass="form-group form-inline">
							<asp:Label runat="server" ID="lblNoDateEditingExplanation" Text="" />
						</asp:Panel>
					</div>
				</div>
			</div>
		</div>

		<div class="admin-row">
			<div runat="server" id="divAddressInfo" class="white-ui-box">
				<div class="row">
					<div class="col-md-12">
						<iframe id="addressFrame" frameborder="0" runat="server" scrolling="no" seamless="seamless" width="1000" height="1200"></iframe>
					</div>
				</div>
			</div>
		</div>

		<div class="admin-row">
			<div runat="server" id="divItemsInfo" class="white-ui-box">
				<div class="row">
					<div class="col-md-12">
						<asp:GridView ID="grdProducts" CssClass="table table-detail" ShowHeader="true" runat="server" GridLines="None" CellSpacing="-1" AutoGenerateColumns="false">
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

	</div>
</asp:Content>


