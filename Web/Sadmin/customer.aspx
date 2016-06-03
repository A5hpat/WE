<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin._customer" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="customer.aspx.cs" %>

<%@ Register TagPrefix="ajax" Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" %>
<%@ Register TagPrefix="aspdnsf" Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls" %>
<%@ Register TagPrefix="aspdnsf" TagName="CustomerToStore" Src="Controls/StoreSelector.ascx" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Import Namespace="AspDotNetStorefrontCore" %>

<asp:Content ContentPlaceHolderID="head" runat="server">
	<script type="text/javascript">
		var impersonateConfirmPrompt = "<asp:Literal runat="server" Text="<%$Tokens:EscapedStringResource, admin.createorder.confirm, javascript %>" />";
	</script>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<h1>
		<i class="fa fa-user"></i>
		<asp:Literal runat="server" ID="HeaderText" />
	</h1>

	<aspdnsf:ReturnUrlTracker runat="server" ID="ReturnUrlTracker" DefaultReturnUrl="Customers.aspx" Blacklist="customeraddress.aspx" />
	<aspdnsf:AlertMessage runat="server" ID="AlertMessageDisplay" />

	<asp:Panel id="pnlCustomerEdit" runat="server" DefaultButton="btnSave">
		<ajax:ConfirmButtonExtender runat="server" ID="cbeFailedTransactions" TargetControlID="btnClearFailedTransactions" ConfirmText="<%$ Tokens:StringResource, admin.customer.confirmFailedTransaction %>" />
		<ajax:ConfirmButtonExtender runat="server" ID="cbeBlockIP" TargetControlID="btnBanIP" ConfirmText="<%$ Tokens:StringResource, admin.customer.confirmBanIP %>" />
		<ajax:ConfirmButtonExtender runat="server" ID="cbeManualPassword" TargetControlID="btnManualPassword" ConfirmText="<%$ Tokens:StringResource, admin.customer.confirmManualPassword %>" />
		<ajax:ConfirmButtonExtender runat="server" ID="cbeRandomPassword" TargetControlID="btnRandomPassword" ConfirmText="<%$ Tokens:StringResource, admin.customer.confirmRandomPassword %>" />
		<ajax:ConfirmButtonExtender runat="server" ID="cbeClearSession" TargetControlID="btnClearSession" ConfirmText="<%$ Tokens:StringResource, admin.customer.confirmClearSession %>" />

		<div class="item-action-bar">
			<asp:Button runat="server"
				ID="btnCreateOrder"
				CssClass="btn btn-action"
				OnClick="btnCreateOrder_Click"
				OnClientClick="return confirm(impersonateConfirmPrompt);"
				ValidationGroup="vldMain"
				Text="<%$ Tokens:StringResource, admin.customer.CreateOrder %>" />

			<asp:HyperLink runat="server"
				CssClass="btn btn-default"
				NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>"
				Text="<%$Tokens:StringResource, admin.common.close %>" />

			<asp:Button runat="server"
				CssClass="btn btn-default"
				OnClick="btnSaveAndClose_Click"
				ValidationGroup="vldMain"
				Text="<%$ Tokens:StringResource, admin.common.SaveAndClose %>" />

			<asp:Button runat="server"
				CssClass="btn btn-primary"
				OnClick="btnSave_Click"
				ValidationGroup="vldMain"
				Text="<%$ Tokens:StringResource, admin.common.Save %>" />
		</div>		
		<asp:Panel runat="server" CssClass="white-ui-box">
			<asp:PlaceHolder runat="server" ID="pnlCustomerID">
				<div class="row form-group">
					<div class="col-sm-3">
						<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.customer.CustomerID %>" />
					</div>
					<div class="col-sm-3">
						<asp:Literal runat="server" ID="ltlCustomerID" />
					</div>
				</div>
			</asp:PlaceHolder>
			<asp:PlaceHolder runat="server" ID="pnlCreatedOn">
				<div class="row form-group">
					<div class="col-sm-3">
						<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.customer.CreatedOn %>" />
					</div>
					<div class="col-sm-3">
						<asp:Literal runat="server" ID="ltlCreatedOn" />
					</div>
				</div>
			</asp:PlaceHolder>
			<div class="row form-group">
				<div class="col-sm-3">
					<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.customer.LocaleSetting %>" />:
				</div>
				<div class="col-sm-3">
					<asp:DropDownList runat="server" ID="ddlCustomerLocaleSetting" CssClass="form-control" />
				</div>
			</div>
			<div class="row form-group">
				<div class="col-sm-3">					
					<span class="text-danger">*</span><asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.customer.FirstName %>" />
				</div>
				<div class="col-sm-3">
					<asp:TextBox CssClass="form-control" ID="txtFirstName" runat="server" MaxLength="100" />
					<asp:RequiredFieldValidator Display="Dynamic" ID="rfvFirstName" runat="server" ControlToValidate="txtFirstName"
						ErrorMessage="<%$ Tokens:StringResource, admin.common.FieldRequiredToLeft %>"
						ValidationGroup="vldMain" CssClass="text-danger" />
				</div>
			</div>
			<div class="row form-group">
				<div class="col-sm-3">					
					<span class="text-danger">*</span><asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.customer.LastName %>" />
				</div>
				<div class="col-sm-3">
					<asp:TextBox CssClass="form-control" ID="txtLastName" runat="server" MaxLength="100" />
					<asp:RequiredFieldValidator Display="Dynamic" ID="rfvLastName" runat="server" ControlToValidate="txtLastName"
						ErrorMessage="<%$ Tokens:StringResource, admin.common.FieldRequiredToLeft %>"
						ValidationGroup="vldMain" CssClass="text-danger" />
				</div>
			</div>
			<div class="row form-group">
				<div class="col-sm-3">		
					<span class="text-danger">*</span><asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.customer.Email %>" />
				</div>
				<div class="col-sm-3">
					<asp:TextBox CssClass="form-control" ID="txtEmail" runat="server" MaxLength="100" AutoCompleteType="Disabled" />
					<aspdnsf:EmailValidator Display="Dynamic" ID="rgxvEmail" runat="server"
						ErrorMessage="<%$ Tokens:StringResource, admin.customer.EmailValidationFailed %>"
						ControlToValidate="txtEmail" ValidationGroup="vldMain" />
					<asp:RequiredFieldValidator runat="server" 
						ID="rfvEmail" 
						CssClass="text-danger" 
						ValidationGroup="vldMain" 
						ControlToValidate="txtEmail"
						Display="Dynamic" 
						ErrorMessage="<%$ Tokens:StringResource, admin.common.FieldRequiredToLeft %>" />										
				</div>
			</div>
			<asp:Panel runat="server" ID="pnlCreateCustomerPassword" CssClass="row form-group" Visible="false">
				<div class="col-sm-3">					
					<span class="text-danger">*</span><asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.topic.password %>" />
				</div>
				<div class="col-sm-3">
					<asp:TextBox CssClass="form-control" ID="txtCreatePassword" runat="server" TextMode="Password" AutoCompleteType="Disabled" />
					<asp:RegularExpressionValidator Display="Dynamic" ID="regexValTxtCreatePassword" runat="server"
						ControlToValidate="txtCreatePassword" ValidationGroup="vldMain" CssClass="text-danger" />
					<asp:RequiredFieldValidator Display="Dynamic" ID="reqValTxtCreatePassword" runat="server" ControlToValidate="txtCreatePassword"
						ErrorMessage="<%$ Tokens:StringResource, admin.common.FieldRequiredToLeft %>"
						ValidationGroup="vldMain" CssClass="text-danger" />
				</div>
			</asp:Panel>
			<div class="row form-group">
				<div class="col-sm-3">
					<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.customer.OKToEmail %>" />
				</div>
				<div class="col-sm-3">
					<asp:CheckBox ID="chkOkToEmail" runat="server" />
				</div>
			</div>
			<div class="row form-group">
				<div class="col-sm-3">				
					<span class="text-danger">*</span><asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.customer.Phone %>" />
				</div>
				<div class="col-sm-3">
					<asp:TextBox runat="server" ID="txtPhone" CssClass="form-control" MaxLength="20" />
					<asp:RequiredFieldValidator runat="server"
						CssClass="text-danger"
						ValidationGroup="vldMain"
						ControlToValidate="txtPhone"
						Display="Dynamic"
						ErrorMessage="<%$ Tokens:StringResource, admin.common.FieldRequiredToLeft %>" />
				</div>
			</div>
			<asp:Panel runat="server" ID="pnlIsRegistered" CssClass="row form-group">
				<div class="col-sm-3">
					<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.customer.IsRegistered %>" />
				</div>
				<div class="col-sm-3">
					<asp:Literal ID="ltlIsRegistered" runat="server" />
				</div>
			</asp:Panel>
			<asp:Panel runat="server" ID="pnlAssignedToStore" CssClass="row form-group">
				<div class="col-sm-3">
					<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.customer.AssignedToStore %>" />
				</div>
				<div class="col-sm-3">
					<aspdnsf:CustomerToStore runat="server" ShowText="false" SelectMode="SingleDropDown" ShowDefaultForAllStores="false" ID="ssOne" />
				</div>
			</asp:Panel>
			<div class="row form-group">
				<div class="col-sm-3">
					<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.customer.Over13 %>" />
				</div>
				<div class="col-sm-3">
					<asp:CheckBox ID="chkOver13" runat="server" />
				</div>
			</div>
			<asp:Panel runat="server" ID="pnlCanViewCC" Visible="false" CssClass="row form-group">
				<div class="col-sm-3">
					<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.customer.CanViewCC %>" />
				</div>
				<div class="col-sm-3">
					<asp:CheckBox ID="chkCanViewCC" runat="server" />
				</div>
			</asp:Panel>
			<asp:Panel runat="server" ID="pnlVATRegID" CssClass="row form-group">
				<div class="col-sm-3">
					<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.customer.VATRegistrationID %>" />
				</div>
				<div class="col-sm-3">
					<asp:TextBox CssClass="form-control" ID="txtVATRegID" runat="server" MaxLength="15" />
					<asp:RegularExpressionValidator ID="rgxvVATRegID" runat="server" ErrorMessage="<%$ Tokens:StringResource, admin.customer.VATValidationFailed %>"
						ControlToValidate="txtVATRegID" ValidationExpression="\B|^[0-9a-zA-Z]{8,12}"
						ValidationGroup="vldMain" />
				</div>
			</asp:Panel>
			<asp:Panel runat="server" Visible="<%# AspDotNetStorefrontCore.AppLogic.MicropayIsEnabled() %>" CssClass="row form-group">
				<div class="col-sm-3">
					<asp:Literal runat="server" Text="<%$ Tokens:StringResource, account.aspx.11 %>" />
					Balance:
				</div>
				<div class="col-sm-3">
					<asp:TextBox CssClass="form-control" ID="txtMicroPay" runat="server" />
				</div>
			</asp:Panel>
			<asp:Panel runat="server" ID="pnlCustomerLevel" CssClass="row form-group">
				<div class="col-sm-3">
					<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.customer.CustomerLevel %>" />
				</div>
				<div class="col-sm-3">
					<asp:DropDownList CssClass="form-control" ID="ddlCustomerLevel" runat="server" />
				</div>
			</asp:Panel>
			<div class="row form-group">
				<div class="col-sm-3">
					<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.customer.Affiliate %>" />
				</div>
				<div class="col-sm-3">
					<asp:DropDownList CssClass="form-control" ID="ddlCustomerAffiliate" runat="server" />
				</div>
			</div>
			<div class="row form-group">
				<div class="col-sm-3">
					<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.customer.Notes %>" />
				</div>
				<div class="col-sm-3">
					<asp:TextBox CssClass="form-control" ID="txtNotes" runat="server" TextMode="MultiLine" Width="450" Rows="10" />
				</div>
			</div>
		</asp:Panel>

		<asp:Panel runat="server" ID="pnlCustomerTools" Visible="false" CssClass="white-ui-box">
			<div class="white-box-heading">
				Tools
			</div>
			<asp:Panel runat="server" ID="pnlAdminSettings" CssClass="row form-group" Visible="false">
				<div class="col-sm-3">
					Admin Settings:
							<asp:Literal runat="server" ID="ltIsAdmin" />
				</div>
				<div class="col-sm-9">
					<asp:Button runat="server" 
						CssClass="btn btn-default btn-sm" 
						Text="Set Admin" 
						CommandName="<%# SetAdminLevelCommand %>" 
						CommandArgument="1"
						Enabled="<%# CustomerId != null && ThisCustomer.IsAdminSuperUser %>" />

					<asp:Button runat="server" 
						CssClass="btn btn-default btn-sm" 
						Text="Set SuperAdmin" 
						CommandName="<%# SetAdminLevelCommand %>" 
						CommandArgument="3"
						Enabled="<%# CustomerId != null && ThisCustomer.IsAdminSuperUser %>" />

					<asp:Button runat="server" 
						CssClass="btn btn-default btn-sm" 
						Text="Set Standard User" 
						CommandName="<%# SetAdminLevelCommand %>" 
						CommandArgument="0"
						Enabled="<%# CustomerId != null && ThisCustomer.IsAdminSuperUser %>" />
				</div>
			</asp:Panel>

			<div class="row form-group">
				<div class="col-sm-3">
					<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.customer.AccountLocked %>" />
				</div>
				<div class="col-sm-3">
					<asp:CheckBox ID="chkAccountLocked" runat="server" />
				</div>
			</div>

			<asp:Panel runat="server" ID="pnlBanIP" CssClass="row form-group">
				<div class="col-sm-3">
					<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.customer.IPAddress %>" />
				</div>
				<div class="col-sm-3">
					<asp:Literal ID="ltlIPAddress" runat="server" />
					<asp:Button ID="btnBanIP" OnClick="btnBanIP_Click" runat="server" Text="Ban/Unban IP Button"
						class="btn btn-danger btn-sm" CausesValidation="false" />
				</div>
			</asp:Panel>
			<asp:Panel runat="server" ID="pnlFailedTransactions" CssClass="row form-group">
				<div class="col-sm-3">
					<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.customer.FailedTransactions %>" />
				</div>
				<div class="col-sm-3">
					<asp:Literal ID="ltlFailedTransactions" runat="server" />
					<asp:Button ID="btnClearFailedTransactions" OnClick="btnClearFailedTransactions_OnClick"
						runat="server" Text="<%$ Tokens:StringResource, admin.customer.ClearFailedTransactions %>"
						class="btn btn-default btn-sm" CausesValidation="false" />
				</div>
			</asp:Panel>
			<asp:Panel runat="server" ID="pnlClearSession" CssClass="row form-group">
				<div class="col-sm-3">
					<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.customer.CustomerSession %>" />
				</div>
				<div class="col-sm-3">
					<asp:Button ID="btnClearSession" OnClick="btnClearSession_OnClick" runat="server"
						Text="<%$ Tokens:StringResource, admin.customer.ClearSession2 %>" class="btn btn-default btn-sm"
						CausesValidation="false" />
				</div>
			</asp:Panel>
			<div class="row form-group">
				<div class="col-sm-3">
					<asp:Literal runat="server" Text="Set Password:" />
					<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="Reset to defined password and send email to customer">
								<i class="fa fa-question-circle fa-lg"></i>
					</asp:Label>
				</div>
				<asp:Panel runat="server" CssClass="col-sm-6" DefaultButton="btnManualPassword">
					<asp:RequiredFieldValidator Display="Dynamic" ID="rfvPassword" runat="server" ControlToValidate="txtManualPassword"
						ErrorMessage="<%$ Tokens:StringResource, admin.common.FieldRequiredToLeft %>"
						ValidationGroup="vldPassword" CssClass="text-danger" />
					<asp:TextBox CssClass="text-md" ID="txtManualPassword" runat="server" />
					<asp:Button ID="btnManualPassword" OnClick="btnManualPassword_OnClick" runat="server"
						Text="<%$ Tokens:StringResource, admin.customer.ManuallySetPassword %>" class="btn btn-default btn-sm"
						ValidationGroup="vldPassword" CausesValidation="true" />
					<asp:RegularExpressionValidator ID="regexValManualPassword" runat="server"
						ControlToValidate="txtManualPassword" ValidationGroup="vldPassword" CssClass="text-danger" ValidationExpression="\S{5,}" />
				</asp:Panel>
			</div>
			<div class="row form-group">
				<div class="col-sm-3">
					<asp:Literal ID="ltlResetPasswordLabel" runat="server" Text="<%$ Tokens:StringResource, admin.customer.ResetPassword %>" />
					<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="Reset to random password and send email to customer">
								<i class="fa fa-question-circle fa-lg"></i>
					</asp:Label>
				</div>
				<div class="col-sm-3">
					<asp:Button ID="btnRandomPassword" OnClick="btnRandomPassword_OnClick" runat="server"
						Text="<%$ Tokens:StringResource, admin.customer.ResetToRandomPassword %>" class="btn btn-default btn-sm"
						CausesValidation="false" />
				</div>
			</div>
		</asp:Panel>

		<div class="item-action-bar">
			<asp:HyperLink runat="server"
				CssClass="btn btn-default"
				NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>"
				Text="<%$Tokens:StringResource, admin.common.close %>" />

			<asp:Button runat="server"
				CssClass="btn btn-default"
				OnClick="btnSaveAndClose_Click"
				ValidationGroup="vldMain"
				Text="<%$ Tokens:StringResource, admin.common.SaveAndClose %>" />

			<asp:Button runat="server"
				ID="btnSave"
				CssClass="btn btn-primary"
				OnClick="btnSave_Click"
				ValidationGroup="vldMain"
				Text="<%$ Tokens:StringResource, admin.common.Save %>" />
		</div>
	</asp:Panel>

	<asp:Panel ID="pnlAddressEdit" runat="server" CssClass="white-ui-box">
		<div class="item-action-bar">
			<a class="btn btn-primary" href='<%# String.Format("customeraddress.aspx?customerId={0}", CustomerId) %>'>
				<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.editaddress.EditAddresses %>" />
			</a>
		</div>

		<div class="white-box-heading">
			<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.customer.editcustomeraddresses %>" />
		</div>
		<div class="row">
			<asp:Panel runat="server" ID="pnlBillingAddress" CssClass="col-sm-6">
				<div class="white-box-heading">
					<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.customer.PrimaryBillingAddress %>" />
				</div>
				<div class="row form-group">
					<div class="col-sm-3">
						<asp:Literal ID="ltlBillingName" runat="server" />
					</div>
				</div>
				<div class="row form-group">
					<div class="col-sm-3">
						<asp:Literal ID="ltlBillingCompany" runat="server" />
					</div>
				</div>
				<div class="row form-group">
					<div class="col-sm-3">
						<asp:Literal ID="ltlBillingAddress1" runat="server" />
					</div>
				</div>
				<div class="row form-group">
					<div class="col-sm-3">
						<asp:Literal ID="ltlBillingAddress2" runat="server" />
					</div>
				</div>
				<div class="row form-group">
					<div class="col-sm-3">
						<asp:Literal ID="ltlBillingSuite" runat="server" />
					</div>
				</div>
				<div class="row form-group">
					<div class="col-sm-3">
						<asp:Literal ID="ltlBillingCityStateZip" runat="server" />
					</div>
				</div>
				<div class="row form-group">
					<div class="col-sm-3">
						<asp:Literal ID="ltlBillingCountry" runat="server" />
					</div>
				</div>
				<div class="row form-group">
					<div class="col-sm-3">
						<asp:Literal ID="ltlBillingPhone" runat="server" />
					</div>
				</div>
				<div class="row form-group">
					<div class="col-sm-3">
						<asp:Literal ID="ltlBillingEmail" runat="server" />
					</div>
				</div>
			</asp:Panel>

			<asp:Panel runat="server" ID="pnlShippingAddress" CssClass="col-sm-6">
				<div class="white-box-heading">
					<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.customer.PrimaryShippingAddress %>" />
				</div>
				<div class="row form-group">
					<div class="col-sm-3">
						<asp:Literal ID="ltlShippingName" runat="server" />
					</div>
				</div>
				<div class="row form-group">
					<div class="col-sm-3">
						<asp:Literal ID="ltlShippingCompany" runat="server" />
					</div>
				</div>
				<div class="row form-group">
					<div class="col-sm-3">
						<asp:Literal ID="ltlShippingAddress1" runat="server" />
					</div>
				</div>
				<div class="row form-group">
					<div class="col-sm-3">
						<asp:Literal ID="ltlShippingAddress2" runat="server" />
					</div>
				</div>
				<div class="row form-group">
					<div class="col-sm-3">
						<asp:Literal ID="ltlShippingSuite" runat="server" />
					</div>
				</div>
				<div class="row form-group">
					<div class="col-sm-3">
						<asp:Literal ID="ltlShippingCityStateZip" runat="server" />
					</div>
				</div>
				<div class="row form-group">
					<div class="col-sm-3">
						<asp:Literal ID="ltlShippingCountry" runat="server" />
					</div>
				</div>
				<div class="row form-group">
					<div class="col-sm-3">
						<asp:Literal ID="ltlShippingPhone" runat="server" />
					</div>
				</div>
				<div class="row form-group">
					<div class="col-sm-3">
						<asp:Literal ID="ltlShippingEmail" runat="server" />
					</div>
				</div>
			</asp:Panel>
		</div>
	</asp:Panel>

</asp:Content>
