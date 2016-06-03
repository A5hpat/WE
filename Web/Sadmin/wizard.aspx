<%@ Page Language="C#" AutoEventWireup="true" Title="Wizard" Inherits="AspDotNetStorefrontAdmin.Wizard" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="wizard.aspx.cs" %>

<%@ Register TagPrefix="ajax" Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" %>
<%@ Register TagPrefix="aspdnsf" TagName="ModalConfigurationAtom" Src="Controls/ModalConfigurationAtom.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="editAppConfigAtom" Src="Controls/editAppConfigAtom.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="GeneralInfo" Src="Controls/GeneralInfo.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
	<script type="text/javascript" src="Scripts/jqueryextensions.js"></script>
	<script type="text/javascript">
		function showGatewayDirections(description) {
			var instructionsHTML = "After successful download, unzip the file and review 'readme.txt' for installation instructions.<br />Your new payment gateway will appear here when you refresh the page.";
			if(description != null && description.length != 0) {
				instructionsHTML = description;
			}
			$("#GatewayInstructionsContent").html(instructionsHTML);

			$find('mpGatewayInstructions').show();
		}
		function pageLoad(sender, args) {
			if(!args.get_isPartialLoad()) {
				$addHandler(document, "keydown", onKeyDown);
			}
		}
		function onKeyDown(e) {
			if(e && e.keyCode == Sys.UI.Key.esc) {
				$('.atomModalClose').click();
			}
		}
	</script>
</asp:Content>
<asp:Content ID="Content2" runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<h1>
		<i class="fa fa-magic"></i>
		<asp:Label ID="lblHeader" runat="server" Text="<%$Tokens:StringResource, admin.title.wizard %>" />
	</h1>
	<div>
		<asp:Label ID="lblHeaderTip" runat="server" Text="<%$Tokens:StringResource, admin.wizard.HeaderTip %>" />
	</div>
	<div>
		<aspdnsf:AlertMessage runat="server" ID="ctrlAlertMessage" />
	</div>
	<div id="divMain" class="configWizard" runat="server">
		<div class="item-action-bar">
			<div class="col-list-action-bar">
				<asp:Button ID="btnSubmitTop" runat="Server" CssClass="btn btn-primary" Text="<%$Tokens:StringResource, admin.common.save %>" OnClick="btnSubmit_Click" />
			</div>
		</div>
		<asp:ScriptManagerProxy ID="ScriptManagerProxy1" runat="server" />
		<div id="container">
			<aspdnsf:ModalConfigurationAtom runat="server" ShowConfigureLink="false" ID="FileConfigurationAtom" />
			<div class="white-ui-box">
				<div class="wrapper">
					<table class="table">
						<tr id="generalSettings">
							<td class="white-box-heading configWizardHeader" colspan="2">
								<h2>
									<asp:Label ID="Label5" runat="server" Text="<%$Tokens:StringResource, admin.wizard.GeneralSettings %>" /></h2>
							</td>
						</tr>
						<tr>
							<td align="right">
								<div class="configTitle">
									<b>Store Name:</b>
								</div>
							</td>
							<td>
								<aspdnsf:editAppConfigAtom runat="server" ID="AtomStoreName" HideLabel="true" ShowSaveButton="false" AppConfig="StoreName" />
							</td>
						</tr>
						<tr>
							<td align="right">
								<div class="configTitle">
									<b>Live Server Domain:</b>
								</div>
							</td>
							<td>
								<aspdnsf:editAppConfigAtom runat="server" ID="AtomLiveServer" HideLabel="true" ShowSaveButton="false" AppConfig="LiveServer" />
							</td>
						</tr>
						<tr>
							<td align="right">
								<div class="configTitle">
									<b>Shipping Origin Zip Code:</b>
								</div>
							</td>
							<td>
								<aspdnsf:editAppConfigAtom runat="server" ID="AtomStoreZip" HideLabel="true" ShowSaveButton="false" AppConfig="RTShipping.OriginZip" />
							</td>
						</tr>
						<tr>
							<td align="right">
								<div class="configTitle">
									<b>Store Currency:</b>
								</div>
							</td>
							<td>
								<aspdnsf:editAppConfigAtom runat="server" ID="AtomStoreCurrency" HideLabel="true" ShowSaveButton="false" AppConfig="Localization.StoreCurrency" />
							</td>
						</tr>
						<tr>
							<td align="right">
								<div class="configTitle">
									<b>Store Currency Numeric Code:</b>
								</div>
							</td>
							<td>
								<aspdnsf:editAppConfigAtom runat="server" ID="AtomStoreCurrencyNumeric" HideLabel="true" ShowSaveButton="false" AppConfig="Localization.StoreCurrencyNumericCode" />
							</td>
						</tr>
						<tr>
							<td align="right">
								<div class="configTitle">
									<b>Use Live Transactions:</b>
								</div>
							</td>
							<td>
								<aspdnsf:editAppConfigAtom runat="server" ID="AtomStoreLiveTransactions" HideLabel="true" ShowSaveButton="false" AppConfig="UseLiveTransactions" />
							</td>
						</tr>
						<tr>
							<td align="right">
								<div class="configTitle">
									<b>Use SSL:</b>
								</div>
							</td>
							<td>
								<aspdnsf:editAppConfigAtom runat="server" ID="AtomStoreUseSSL" HideLabel="true" ShowSaveButton="false" AppConfig="UseSSL" />
							</td>
						</tr>
						<tr id="MachineKeyRow" runat="server">
							<td align="right">
								<div class="configTitle">
									<b>
										<asp:Literal ID="ltStaticMachineKey" runat="server" />
									</b>
								</div>
							</td>
							<td class="atomInfoCell">
								<table>
									<tr>
										<td class="atomAppConfigEditCell">
											<div class="atomAppConfigEditWrap">
												<asp:RadioButtonList ID="rblStaticMachineKey" runat="server" RepeatColumns="2" RepeatDirection="Horizontal" CellPadding="5" CellSpacing="0">
													<asp:ListItem Value="false" Selected="true" Text="No" />
													<asp:ListItem Value="true" Text="Yes" />
												</asp:RadioButtonList>
											</div>
										</td>
										<td>
											<div class="atomDescriptionWrap">
												<asp:Label ID="Label1" runat="server" Text="<%$Tokens:StringResource, admin.wizard.tooltip.imgStaticMachineKey %>" />
											</div>
										</td>
									</tr>
								</table>
							</td>
						</tr>
						<tr id="EncryptWebConfigRow" runat="server">
							<td align="right">
								<div class="configTitle">
									<b>Encrypt the Web.Config:
									</b>
								</div>
							</td>
							<td class="atomInfoCell">
								<table>
									<tr>
										<td class="atomAppConfigEditCell">
											<div class="atomAppConfigEditWrap">
												<asp:RadioButtonList ID="rblEncrypt" runat="server" RepeatColumns="2" RepeatDirection="Horizontal" CellPadding="5" CellSpacing="0">
													<asp:ListItem Value="false" Selected="true" Text="No" />
													<asp:ListItem Value="true" Text="Yes" />
												</asp:RadioButtonList>
											</div>
										</td>
										<td>
											<div class="atomDescriptionWrap">
												<asp:Label ID="Label2" runat="server" Text="<%$Tokens:StringResource, admin.wizard.tooltip.imgEncryptKey %>" />
											</div>
										</td>
									</tr>
								</table>
							</td>
						</tr>
						<tr id="trEmail">
							<td align="right">
								<div class="configTitle">
									<b>Email:</b>
								</div>
							</td>
							<td align="left">
								<table>
									<tr>
										<td class="atomAppConfigEditCell">
											<div class="atomAppConfigEditWrap">
												<asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
													<Triggers>
														<asp:AsyncPostBackTrigger ControlID="btnConfigureEmail" EventName="Click" />
													</Triggers>
													<ContentTemplate>
														<asp:LinkButton ID="btnConfigureEmail" OnClick="ShowModalAtomByXMLFile_Click" CommandArgument="General.Email.xml" Text="Configure Email" runat="server" />
													</ContentTemplate>
												</asp:UpdatePanel>
											</div>
										</td>
										<td></td>
									</tr>
								</table>
							</td>
						</tr>
						<tr id="trSEO">
							<td align="right">
								<div class="configTitle">
									<b>Search Engine (Meta Tags):</b>
								</div>
							</td>
							<td align="left">
								<table>
									<tr>
										<td class="atomAppConfigEditCell">
											<div class="atomAppConfigEditWrap">
												<asp:UpdatePanel ID="UpdatePanel2" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
													<Triggers>
														<asp:AsyncPostBackTrigger ControlID="btnConfigureSEO" EventName="Click" />
													</Triggers>
													<ContentTemplate>
														<asp:LinkButton ID="btnConfigureSEO" OnClick="ShowModalAtomByXMLFile_Click" CommandArgument="General.SEO.xml" Text="Configure SEO" runat="server" />
													</ContentTemplate>
												</asp:UpdatePanel>
											</div>
										</td>
										<td></td>
									</tr>
								</table>
							</td>
						</tr>

						<tr id="trBuySafe">
							<td class="textBoxLabel" align="right">
								<div class="configTitle">
									<b>Increase sales with buySAFE:</b>
								</div>
							</td>
							<td class="atomInfoCell">
								<asp:Panel ID="pnlBuySafeInactive" runat="server">
									<table>
										<tr>
											<td class="atomAppConfigEditCell">
												<div class="atomAppConfigEditWrap">
													<asp:RadioButtonList ID="rblBuySafeEnabled" RepeatDirection="Horizontal" runat="server">
														<asp:ListItem Selected="True" Text="No" />
														<asp:ListItem Text="Yes" />
													</asp:RadioButtonList>
												</div>
											</td>
											<td>
												<div class="atomDescriptionWrap">
													<a href="buysafeSetup.aspx" style="display: block; margin-left: 5px;" target="_blank">buySAFE increases your site conversions by improving shopper confidence. Click here to learn more...</a>
												</div>
											</td>
										</tr>
									</table>
								</asp:Panel>
								<asp:Panel ID="pnlBuySafeActive" Visible="false" runat="server">
									<asp:Literal ID="litBuySafeActiveMsg" runat="server" />
								</asp:Panel>
							</td>
						</tr>
						<tr id="fraudSolutions">
							<td class="white-box-heading configWizardHeader" colspan="2">
								<h2>
									<asp:Label ID="Label6" runat="server" Text="<%$Tokens:StringResource, admin.wizard.FraudSolutions %>" /></h2>
							</td>
						</tr>
						<tr id="trMaxMind">
							<td align="right">
								<div class="configTitle">
									<b>MaxMind:</b>
								</div>
							</td>
							<td align="left">
								<table>
									<tr>
										<td class="atomAppConfigEditCell">
											<div class="atomAppConfigEditWrap">
												<asp:UpdatePanel runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
													<Triggers>
														<asp:AsyncPostBackTrigger ControlID="btnConfigureMaxMind" EventName="Click" />
													</Triggers>
													<ContentTemplate>
														<asp:LinkButton ID="btnConfigureMaxMind" OnClick="ShowModalAtomByXMLFile_Click" CommandArgument="General.MaxMind.xml" Text="Configure MaxMind" runat="server" />
													</ContentTemplate>
												</asp:UpdatePanel>
											</div>
										</td>
										<td></td>
									</tr>
								</table>
							</td>
						</tr>
						<tr id="country">
							<td class="white-box-heading configWizardHeader" colspan="2">
								<h2>
									<asp:Label ID="Label7" runat="server" Text="<%$Tokens:StringResource, admin.wizard.ChooseCountry %>" /></h2>
							</td>
						</tr>
						<tr>
							<td align="right">
								<div class="configTitle">
									<b>Choose your company's country:</b>
								</div>
							</td>
							<td>
								<table>
									<a id="payments"></a>
									<tr>
										<td class="atomAppConfigEditCell">
											<div class="atomAppConfigEditWrap">
												<asp:DropDownList ID="ddlCountries" runat="server" OnSelectedIndexChanged="ddlCountries_SelectedIndexChanged" AutoPostBack="true" />
											</div>
										</td>
										<td></td>
									</tr>
								</table>
							</td>
						</tr>
						<tr id="alternativePaymentMethods">
							<td class="white-box-heading configWizardHeader" colspan="2">
								<h2>
									<asp:Label ID="Label8" runat="server" Text="<%$Tokens:StringResource, admin.wizard.AltPayment %>" /></h2>
							</td>
						</tr>
						<tr id="trCheckoutMethods">
							<td align="right">
								<div class="configTitle">
									<b>Alternative Payment Methods:</b>
								</div>
								<asp:UpdatePanel runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
									<Triggers>
										<asp:AsyncPostBackTrigger ControlID="btnConfigureCheckoutMethodsPerStore" EventName="Click" />
									</Triggers>
									<ContentTemplate>
										<span class="lineHeightFix">
											<asp:LinkButton ID="btnConfigureCheckoutMethodsPerStore" OnClick="ShowModalAtomByXMLFile_Click" CommandArgument="General.PaymentMethods.xml" Text="Configure Per Store" runat="server" />&nbsp;&nbsp;
										</span>
									</ContentTemplate>
								</asp:UpdatePanel>
							</td>
							<td align="left">
								<asp:UpdatePanel ID="UpdatePanel8" UpdateMode="Conditional" ChildrenAsTriggers="false" runat="server">
									<Triggers>
										<asp:AsyncPostBackTrigger ControlID="btnConfigurePayPalExpress" EventName="Click" />
										<asp:AsyncPostBackTrigger ControlID="btnConfigureAmazonPayments" EventName="Click" />
									</Triggers>
									<ContentTemplate>
										<table class="table">
											<tr id="trPayPalExpress" runat="server">
												<td class="configSelect">
													<asp:CheckBox CssClass="paymentCheckBox" ID="cbxPayPalExpress" runat="server" Text="PayPal Express Checkout" />
												</td>
												<td class="configImage">
													<img id="Img3" runat="server" src="Images/PayPal_OnBoarding_ECPaymentIcon.gif" class="paymentIcon paypalCheckoutBtn" />
												</td>
												<td>
													See <a href="http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=10000manual&type=paypalexpress" target="_blank">Manual</a> and <a href="http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=paypalexpress&type=demo" target="_blank">Demo</a>
												</td>
												<td class="paymentRightCell">
													<asp:LinkButton ID="btnConfigurePayPalExpress" OnClick="ShowModalAtomByXMLFile_Click" CommandArgument="PaymentMethod.PayPalExpress.xml" Text="configure" runat="server" />
												</td>
											</tr>
											<tr>
												<td class="configSelect">
													<asp:CheckBox CssClass="paymentCheckBox" ID="cbxAmazonPayments" runat="server" Text="<%$Tokens:StringResource, gateway.amazonpayments.name %>" />
												</td>
												<td class="configImage">
													<img id="Img2" runat="server" src="Images/paywithamazon.png" class="paymentIcon amazonPaymentsCheckoutBtn" />
												</td>
												<td>
													See <a href="http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=10000manual&type=paywithamazon" target="_blank">Manual</a>
												</td>
												<td class="paymentRightCell">
													<asp:LinkButton ID="btnConfigureAmazonPayments" OnClick="ShowModalAtomByXMLFile_Click" CommandArgument="PaymentMethod.AmazonPayments.xml" Text="configure" runat="server" />
												</td>
											</tr>
										</table>
									</ContentTemplate>
								</asp:UpdatePanel>
							</td>
						</tr>
						<tr id="paymentMethods">
							<td class="white-box-heading configWizardHeader" colspan="2">
								<h2>
									<asp:Label ID="Label10" runat="server" Text="<%$Tokens:StringResource, admin.wizard.PaymentMethods %>" /></h2>
							</td>
						</tr>
						<tr id="trPaymentMethods">
							<td align="right">
								<div class="configTitle">
									<b>Payment Methods:</b>
								</div>
								<asp:UpdatePanel ID="UpdatePanel3" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
									<Triggers>
										<asp:AsyncPostBackTrigger ControlID="btnConfigurePaymentMethodsPerStore" EventName="Click" />
									</Triggers>
									<ContentTemplate>
										<span class="lineHeightFix">
											<asp:LinkButton ID="btnConfigurePaymentMethodsPerStore" OnClick="ShowModalAtomByXMLFile_Click" CommandArgument="General.PaymentMethods.xml" Text="Configure Per Store" runat="server" />&nbsp;&nbsp;
										</span>
									</ContentTemplate>
								</asp:UpdatePanel>
							</td>
							<td align="left">
								<asp:UpdatePanel ID="UpdatePanel4" UpdateMode="Conditional" ChildrenAsTriggers="false" runat="server">
									<Triggers>
										<asp:AsyncPostBackTrigger ControlID="btnConfigureCreditCard" EventName="Click" />
									</Triggers>
									<ContentTemplate>
										<table class="table">
											<tr>
												<td class="configSelect">
													<asp:CheckBox CssClass="paymentCheckBox" ID="cbxCreditCard" runat="server" Text="Credit Card" />
												</td>
												<td class="configImage"></td>
												<td>&nbsp;</td>
												<td class="paymentRightCell">
													<asp:LinkButton ID="btnConfigureCreditCard" OnClick="ShowModalAtomByXMLFile_Click" CommandArgument="PaymentMethod.CreditCard.xml" Text="configure" runat="server" />
												</td>
											</tr>
											<tr>
												<td class="configSelect">
													<asp:CheckBox CssClass="paymentCheckBox" ID="cbxRequestQuote" runat="server" Text="Request For Quote" />
												</td>
												<td class="configImage"></td>
												<td>&nbsp;</td>
												<td class="paymentRightCell">&nbsp;</td>
											</tr>
											<tr>
												<td class="configSelect">
													<asp:CheckBox CssClass="paymentCheckBox" ID="cbxPurchaseOrder" runat="server" Text="Purchase Orders" />
												</td>
												<td class="configImage"></td>
												<td>&nbsp;</td>
												<td class="paymentRightCell">&nbsp;</td>
											</tr>
											<tr>
												<td class="configSelect">
													<asp:CheckBox CssClass="paymentCheckBox" ID="cbxCheckByMail" runat="server" Text="Checks" />
												</td>
												<td class="configImage"></td>
												<td>&nbsp;</td>
												<td class="paymentRightCell">&nbsp;</td>
											</tr>
											<tr>
												<td class="configSelect">
													<asp:CheckBox CssClass="paymentCheckBox" ID="cbxCOD" runat="server" Text="C.O.D." />
												</td>
												<td class="configImage"></td>
												<td>&nbsp;</td>
												<td class="paymentRightCell">&nbsp;</td>
											</tr>
											<tr>
												<td class="configSelect">
													<asp:CheckBox CssClass="paymentCheckBox" ID="cbxMicroPay" runat="server" Text="MICROPAY" />
												</td>
												<td class="configImage"></td>
												<td>
													See <a href="http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=10000manual&type=micropay" target="_blank">Manual</a>
												</td>
												<td class="paymentRightCell">&nbsp;</td>
											</tr>
										</table>
									</ContentTemplate>
								</asp:UpdatePanel>
							</td>
						</tr>
						<tr id="paymentGateways">
							<td class="white-box-heading configWizardHeader" colspan="2">
								<h2>
									<asp:Label ID="Label9" runat="server" Text="<%$Tokens:StringResource, admin.wizard.PaymentSolutions %>" /></h2>
							</td>
						</tr>
						<tr id="trGateways">
							<td align="right">
								<div class="configTitle">
									<b>Payment Gateway:</b>
								</div>
								<asp:UpdatePanel ID="UpdatePanel5" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
									<Triggers>
										<asp:AsyncPostBackTrigger ControlID="btnGatewayPerStoreAndBackup" EventName="Click" />
									</Triggers>
									<ContentTemplate>
										<span class="lineHeightFix">
											<asp:LinkButton Style="margin-right: 5px; display: block;" ID="btnGatewayPerStoreAndBackup" OnClick="ShowModalAtomByXMLFile_Click" CommandArgument="General.Gateway.xml" runat="server">Configure Per Store and<br />Backup Gateways</asp:LinkButton><br />
										</span>
									</ContentTemplate>
								</asp:UpdatePanel>
							</td>
							<td align="left">
								<asp:Repeater ID="repGateways" runat="server" OnItemDataBound="repGateways_DataBinding" OnItemCommand="repGateways_ItemCommand">
									<HeaderTemplate>
										<table class="table">
									</HeaderTemplate>
									<ItemTemplate>
										<tr id="trGateway" runat="server">
											<td class="configSelect">
												<asp:UpdatePanel ID="upGatewaySelect" runat="server" UpdateMode="Always" ChildrenAsTriggers="true">
													<Triggers>
														<asp:AsyncPostBackTrigger ControlID="repGateways" EventName="ItemCommand" />
														<asp:AsyncPostBackTrigger ControlID="btnConfigureGateway" EventName="Click" />
													</Triggers>
													<ContentTemplate>
														<aspdnsf:GroupRadioButton CssClass="rbGateway" ID="rbGateway" GroupName="SelectedGateway" Enabled="false" runat="server" value='<%# Eval("GatewayIdentifier") %>' />
														<span class="wizardRadioDisplayName">
															<%# Eval("DisplayName") %>
														</span>
													</ContentTemplate>
												</asp:UpdatePanel>
											</td>
											<td class="configImage">
												<asp:Image ID="imgPayPal" CssClass="paymentIcon paypalGenericOptions" runat="server" ImageUrl="Images/PayPal_PaymentsAccepted.gif" Visible="false" />
											</td>
											<td>
												<div class="lineHeightFix">
													<%# Eval("AdministratorSetupPrompt")%>
												</div>
												<asp:HiddenField ID="hfGatewayIdentifier" Value='<%# Eval("GatewayIdentifier") %>' runat="server" />
												<asp:HiddenField ID="hfGatewayProductIdentifier" Value='<%# Eval("DisplayName") %>' runat="server" />
											</td>
											<td class="paymentRightCell">
												<asp:LinkButton runat="server" ID="btnConfigureGateway" Text="configure" CommandName="ShowConfiguration" CommandArgument='<%# Eval("GatewayIdentifier") %>' />
											</td>
										</tr>
									</ItemTemplate>
									<FooterTemplate>
										</table>
									</FooterTemplate>
								</asp:Repeater>
								<aspdnsf:ModalConfigurationAtom runat="server" OnAtomSaved="GatewayConfigurationAtom_Saved" ShowConfigureLink="false" ID="GatewayConfigurationAtom" ConfigureButtonCssClass="ConfigureGatewayButton" />
							</td>
						</tr>
					</table>
					<asp:ValidationSummary ID="validationSummary" runat="server" EnableClientScript="true" ShowMessageBox="true" ShowSummary="false" Enabled="true" />
				</div>
			</div>
			<div class="item-action-bar">
				<div class="col-list-action-bar">
					<asp:Button ID="btnSubmitBottom" runat="Server" CssClass="btn btn-primary" Text="<%$Tokens:StringResource, admin.common.save %>" OnClick="btnSubmit_Click" />
				</div>
			</div>
		</div>
	</div>
	<ajax:ModalPopupExtender
		ID="mpGatewayInstructions" runat="server"
		PopupControlID="pnlGatewayInstructions"
		TargetControlID="btnShowGatewayInstructions"
		BackgroundCssClass="modal_popup_background"
		CancelControlID="btnCancelConfiguration"
		BehaviorID="mpGatewayInstructions">
	</ajax:ModalPopupExtender>
	<div style="display: none;">
		<asp:LinkButton ID="btnShowGatewayInstructions" runat="server" Text="configure" OnClientClick="showGatewayDirections();return false;" />
		<asp:Panel ID="pnlGatewayInstructions" runat="server" CssClass="modal_popup atom_modal_popup" Width="725px" ScrollBars="None">
			<div class="modal_popup_Header" id="modaldiv" runat="server">
				<asp:Literal ID="Literal1" runat="server" Text="Gateway Installation" />
				<div style="float: right;">
					<asp:ImageButton ID="btnCancelConfiguration" runat="server" src="../App_Themes/Admin_Default/images/delete.png" />
				</div>
			</div>
			<asp:Panel ID="pnlConfigAtomContainer" runat="server" ScrollBars="None">
				<div style="padding: 10px;">
					<strong>Your download has started.</strong><br />
					<br />
					<span id="GatewayInstructionsContent"></span>
				</div>
			</asp:Panel>
		</asp:Panel>
	</div>
</asp:Content>
