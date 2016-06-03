<%@ Page Language="c#" Inherits="AspDotNetStorefrontAdmin.adjustcharge" Title="Adjust Charge" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Theme="Admin_Default" Codebehind="adjustcharge.aspx.cs" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<aspdnsf:ReturnUrlTracker runat="server" ID="ReturnUrlTracker" DefaultReturnUrl="Orders.aspx" />

	<h1>
		<i class="fa fa-cubes f-x3"></i>
		<asp:Literal ID="ltAdjustOrderTitle" runat="server" Text="<%$Tokens:StringResource, admin.adjustcharge.AdjustChargeForOrder %>" />
	</h1>

	<aspdnsf:AlertMessage ID="AlertMessageDisplay" runat="server" />

	<div class="item-action-bar clearfix">
		<asp:HyperLink runat="server"
			CssClass="btn btn-default"
			NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>"
			Text="<%$Tokens:StringResource, admin.common.close %>" />

		<asp:Button ID="btnSubmit" runat="server" CssClass="btn btn-primary" OnClick="btnSubmit_Click" ValidationGroup="grpAdjCharge" Text="<%$Tokens:StringResource, admin.common.save %>" />
	</div>

	<div class="white-ui-box">
		<div class="white-box-heading">
			<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.adjustcharge.ManualAdjustWarning %>" />
			<br />
			<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.adjustcharge.AppliesToAuthWarning %>" />
		</div>
		<div class="row">
			<div class="col-sm-4">
				<span class="text-danger">*</span><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.adjustcharge.NewOrderTotal %>" AssociatedControlID="txtNewOrderTotal" />
			</div>
			<div class="col-sm-4">
				<asp:TextBox runat="server" ID="txtNewOrderTotal" CssClass="form-control" />
				<asp:CompareValidator Operator="DataTypeCheck" Display="Dynamic" Type="Currency" ErrorMessage="<%$Tokens:StringResource, admin.common.ValidDollarAmountPrompt %>" ControlToValidate="txtNewOrderTotal" ID="rfvOrderTotal" CssClass="text-danger" ValidationGroup="grpAdjCharge" SetFocusOnError="true" runat="server" />
				<asp:RequiredFieldValidator ErrorMessage="<%$Tokens:StringResource, admin.common.FieldRequiredToLeft %>" Display="Dynamic" ControlToValidate="txtNewOrderTotal" ID="rfvAmount" CssClass="text-danger" ValidationGroup="grpAdjCharge" SetFocusOnError="true" runat="server" />
			</div>
		</div>
		<div class="row">
			<div class="col-sm-4">
				<span class="text-danger">*</span><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.adjustcharge.CustomerServiceNotes %>" AssociatedControlID="txtCustomerServiceNotes" />
			</div>
			<div class="col-sm-4">
				<asp:TextBox runat="server" ID="txtCustomerServiceNotes" CssClass="form-control" TextMode="Multiline" />
				<asp:RequiredFieldValidator ErrorMessage="<%$Tokens:StringResource, admin.common.FieldRequiredToLeft %>" Display="Dynamic" ControlToValidate="txtCustomerServiceNotes" ID="rfvServiceNotes" CssClass="text-danger" ValidationGroup="grpAdjCharge" SetFocusOnError="true" runat="server" />
			</div>
		</div>
	</div>
</asp:Content>