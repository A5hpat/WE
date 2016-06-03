<%@ Page Language="c#" Inherits="AspDotNetStorefrontAdmin.editcustomerlevel" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="customerlevel.aspx.cs" %>

<%@ Register Src="Controls/LocaleField.ascx" TagPrefix="aspdnsf" TagName="LocaleField" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<div class="admin-module">
		<h1>
			<i class="fa fa-list-ol"></i>
			<asp:Label ID="lblHeader" runat="server" />
		</h1>

		<aspdnsf:ReturnUrlTracker runat="server" ID="ReturnUrlTracker" DefaultReturnUrl="CustomerLevels.aspx" />
		
		<aspdnsf:AlertMessage runat="server" ID="ctrlAlertMessage" />

		<asp:Label ID="lblTopInfo" runat="server" Text="<%$Tokens:StringResource, admin.editCustomerLevel.EnterCLevelInfo %>" />

		<div class="item-action-bar">
			<asp:HyperLink runat="server"
				CssClass="btn btn-default"
				NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>"
				Text="<%$Tokens:StringResource, admin.common.close %>" />

			<asp:Button ID="btnSaveAndCloseTop" runat="server" CssClass="btn btn-default" OnClick="btnSaveAndClose_Click" ValidationGroup="CLSubmit" Text="<%$Tokens:StringResource, admin.common.saveandclose %>" />
			<asp:Button ID="btnResetTop" runat="server" CssClass="btn btn-default" OnClick="btnReset_Click" Text="<%$Tokens:StringResource, admin.common.Reset %>" />
			<asp:Button ID="btnSaveTop" runat="server" CssClass="btn btn-primary" OnClick="btnSave_Click" ValidationGroup="CLSubmit" Text="<%$Tokens:StringResource, admin.common.Save %>" />
		</div>
		<div class="white-ui-box">
			<div class="form-group">
				<div class="row admin-row">
					<div class="col-sm-3">
						<span class="text-danger">*</span><asp:Label ID="lblNameTitle" runat="server" Text="<%$Tokens:StringResource, admin.common.Name %>" />:
					</div>
					<div class="col-sm-3">
						<aspdnsf:LocaleField runat="server" ID="NameLocaleField" DefaultLocaleSetting="<%#ThisCustomer.LocaleSetting %>" ValidationGroup="CLSubmit" />
					</div>
					<div class="col-sm-6">
						<asp:Label ID="lblNameNotification" runat="server" Text="<%$Tokens:StringResource, admin.editCustomerLevel.Notification %>" />
					</div>
				</div>
				<div class="row admin-row">
					<div class="col-sm-3">
						<span class="text-danger">*</span><asp:Label AssociatedControlID="txtLevelDiscountPercent" ID="lblLevelDiscountTitle" runat="server" Text="<%$Tokens:StringResource, admin.editCustomerLevel.LevelDiscountPercent %>" />:
					</div>
					<div class="col-sm-3">
						<asp:TextBox CssClass="form-control" MaxLength="100" ID="txtLevelDiscountPercent" runat="Server" />
						<asp:RequiredFieldValidator ControlToValidate="txtLevelDiscountPercent" CssClass="text-danger" ErrorMessage="<%$Tokens:StringResource, admin.customer.level.DiscountPercentRequired %>" ID="RequiredFieldValidator2" ValidationGroup="CLSubmit" SetFocusOnError="true" runat="server" Display="Dynamic" />
					</div>
					<div class="col-sm-6">
						<asp:Label ID="lblLevelDiscount" runat="server" Text="<%$Tokens:StringResource, admin.editCustomerLevel.NotificationValue %>" />
					</div>
				</div>
				<div class="row admin-row">
					<div class="col-sm-3">
						<span class="text-danger">*</span><asp:Label AssociatedControlID="txtLevelDiscountAmount" ID="lblLevelDiscountAmountTitle" runat="server" Text="<%$Tokens:StringResource, admin.editCustomerLevel.LevelDiscountAmount %>" />:
					</div>
					<div class="col-sm-3">
						<asp:TextBox CssClass="form-control" MaxLength="100" ID="txtLevelDiscountAmount" runat="Server" />
						<asp:RequiredFieldValidator ControlToValidate="txtLevelDiscountAmount" CssClass="text-danger" ErrorMessage="<%$Tokens:StringResource, admin.customer.level.DiscountPercentAmount %>" ID="RequiredFieldValidator3" ValidationGroup="CLSubmit" SetFocusOnError="true" runat="server" Display="Dynamic" />
					</div>
					<div class="col-sm-6">
						<asp:Label ID="lblLevelDiscountAmount" runat="server" Text="<%$Tokens:StringResource, admin.editCustomerLevel.NotificationValues %>" />
					</div>
				</div>
				<div class="row admin-row">
					<div class="col-sm-3">
						<span class="text-danger"></span><asp:Label AssociatedControlID="LevelHasFreeShipping" ID="lblLevelIncludesFreeShipping" runat="server" Text="<%$Tokens:StringResource, admin.editCustomerLevel.LevelIncludesFreeShipping %>" />:
					</div>
					<div class="col-sm-3">
						<asp:RadioButtonList ID="LevelHasFreeShipping" runat="server" RepeatColumns="2" RepeatDirection="horizontal" RepeatLayout="Flow">
							<asp:ListItem Value="1" Text="<%$Tokens:StringResource, admin.common.Yes %>" />
							<asp:ListItem Value="0" Text="<%$Tokens:StringResource, admin.common.No %>" />
						</asp:RadioButtonList>
					</div>
					<div class="col-sm-6">
						<asp:Label ID="lblLevelHasFreeShipping" runat="server" Text="<%$Tokens:StringResource, admin.editCustomerLevel.IfYesFreeShipping %>" />
					</div>
				</div>
				<div class="row admin-row">
					<div class="col-sm-3">
						<span class="text-danger"></span><asp:Label AssociatedControlID="LevelAllowsQuantityDiscounts" ID="lblLevelAllowsQuantityDiscountsTitle" runat="server" Text="<%$Tokens:StringResource, admin.editCustomerLevel.LevelAllowsQuantityDiscounts %>" />:
					</div>
					<div class="col-sm-3">
						<asp:RadioButtonList ID="LevelAllowsQuantityDiscounts" runat="server" RepeatColumns="2" RepeatDirection="horizontal" RepeatLayout="Flow">
							<asp:ListItem Value="1" Text="<%$Tokens:StringResource, admin.common.Yes %>" />
							<asp:ListItem Value="0" Text="<%$Tokens:StringResource, admin.common.No %>" />
						</asp:RadioButtonList>
					</div>
					<div class="col-sm-6">
						<asp:Label ID="lblLevelAllowsQuantityDiscounts" runat="server" Text="<%$Tokens:StringResource, admin.editCustomerLevel.IfYesQuantityDiscount %>" />
					</div>
				</div>
				<div class="row admin-row">
					<div class="col-sm-3">
						<span class="text-danger"></span><asp:Label AssociatedControlID="LevelAllowsPO" ID="lblLevelAllowsPurchaseOrder" runat="server" Text="Level Allows Purchase Orders" />:
					</div>
					<div class="col-sm-3">
						<asp:RadioButtonList ID="LevelAllowsPO" runat="server" RepeatColumns="2" RepeatDirection="horizontal" RepeatLayout="Flow">
							<asp:ListItem Value="1" Text="<%$Tokens:StringResource, admin.common.Yes %>" />
							<asp:ListItem Value="0" Text="<%$Tokens:StringResource, admin.common.No %>" />
						</asp:RadioButtonList>
					</div>
					<div class="col-sm-6">
					</div>
				</div>
				<div class="row admin-row">
					<div class="col-sm-3">
						<span class="text-danger"></span><asp:Label AssociatedControlID="LevelHasNoTax" ID="lblLevelHasNoTaxTitle" runat="server" Text="<%$Tokens:StringResource, admin.editCustomerLevel.LevelNoTaxOnOrders %>" />:
					</div>
					<div class="col-sm-3">
						<asp:RadioButtonList ID="LevelHasNoTax" runat="server" RepeatColumns="2" RepeatDirection="horizontal" RepeatLayout="Flow">
							<asp:ListItem Value="1" Text="<%$Tokens:StringResource, admin.common.Yes %>" />
							<asp:ListItem Value="0" Text="<%$Tokens:StringResource, admin.common.No %>" />
						</asp:RadioButtonList>
					</div>
					<div class="col-sm-6">
						<asp:Label ID="lblLEvelHasNoTax" runat="server" Text="<%$Tokens:StringResource, admin.editCustomerLevel.IfYesCustomerOrders %>" />
					</div>
				</div>
				<div class="row admin-row">
					<div class="col-sm-3">
						<span class="text-danger"></span><asp:Label AssociatedControlID="LevelAllowsCoupons" ID="lblLevelAllowsCouponOrdersTitle" runat="server" Text="<%$Tokens:StringResource, admin.editCustomerLevel.LevelAllowsCouponsOnOrders %>" />:
					</div>
					<div class="col-sm-3">
						<asp:RadioButtonList ID="LevelAllowsCoupons" runat="server" RepeatColumns="2" RepeatDirection="horizontal" RepeatLayout="Flow">
							<asp:ListItem Value="1" Text="<%$Tokens:StringResource, admin.common.Yes %>" />
							<asp:ListItem Value="0" Text="<%$Tokens:StringResource, admin.common.No %>" />
						</asp:RadioButtonList>
					</div>
					<div class="col-sm-6">
						<asp:Label ID="lblLevelAllowsCouponOrders" runat="server" Text="<%$Tokens:StringResource, admin.editCustomerLevel.IfYesCustomerCoupon %>" />
					</div>
				</div>
				<div class="row admin-row">
					<div class="col-sm-3">
						<span class="text-danger"></span><asp:Label AssociatedControlID="LevelDiscountsApplyToExtendedPrices" ID="lblLevelDiscountExtendedPriceTitle" runat="server" Text="<%$Tokens:StringResource, admin.editCustomerLevel.LevelDiscountExtendedPricing %>" />:
					</div>
					<div class="col-sm-3">
						<asp:RadioButtonList ID="LevelDiscountsApplyToExtendedPrices" runat="server" RepeatColumns="2" RepeatDirection="horizontal" RepeatLayout="Flow">
							<asp:ListItem Value="1" Text="<%$Tokens:StringResource, admin.common.Yes %>" />
							<asp:ListItem Value="0" Text="<%$Tokens:StringResource, admin.common.No %>" />
						</asp:RadioButtonList>
					</div>
					<div class="col-sm-6">
						<asp:Label ID="lblLevelDiscountExtendedPrice" runat="server" Text="<%$Tokens:StringResource, admin.editCustomerLevel.IfYesDiscountAmount %>" />
					</div>
				</div>
			</div>
		</div>
		<div class="item-action-bar">
			<asp:HyperLink runat="server"
				CssClass="btn btn-default"
				NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>"
				Text="<%$Tokens:StringResource, admin.common.close %>" />

			<asp:Button ID="btnSaveAndClose" runat="server" CssClass="btn btn-default" OnClick="btnSaveAndClose_Click" ValidationGroup="CLSubmit" Text="<%$Tokens:StringResource, admin.common.saveandclose %>" />
			<asp:Button ID="btnReset" runat="server" CssClass="btn btn-default" OnClick="btnReset_Click" Text="<%$Tokens:StringResource, admin.common.Reset %>" />
			<asp:Button ID="btnSave" runat="server" CssClass="btn btn-primary" OnClick="btnSave_Click" ValidationGroup="CLSubmit" Text="<%$Tokens:StringResource, admin.common.Save %>" />
		</div>
	</div>
</asp:Content>
