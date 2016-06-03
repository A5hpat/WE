<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.giftcard" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" CodeBehind="giftcard.aspx.cs" %>

<%@ Register TagPrefix="ajax" Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" %>
<%@ Register TagPrefix="telerik" Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" %>
<%@ Register TagPrefix="aspdnsf" TagName="EntityToStore" Src="controls/EntityToStoreMapper.ascx" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<h1>
		<i class="fa fa-credit-card"></i>
		<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editgiftcard.GiftCardDetails %>" />
	</h1>

	<aspdnsf:ReturnUrlTracker runat="server" ID="ReturnUrlTracker" DefaultReturnUrl="GiftCards.aspx" />

	<aspdnsf:AlertMessage runat="server" ID="AlertMessage" />

	<asp:Panel runat="server" CssClass="wrapper" DefaultButton="btnSubmit">
		<div class="item-action-bar">
			<asp:HyperLink runat="server"
				CssClass="btn btn-default"
				NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>"
				Text="<%$Tokens:StringResource, admin.common.close %>" />

			<asp:Button ID="btnCloseTop" runat="server" CssClass="btn btn-default" OnClick="btnClose_Click" ValidationGroup="main" Text="<%$Tokens:StringResource, admin.common.SaveAndClose %>" />
			<asp:Button ID="btnSubmitTop" runat="server" CssClass="btn btn-primary" OnClick="btnSubmit_Click" ValidationGroup="main" Text="<%$Tokens:StringResource, admin.common.Save %>" />
		</div>

		<div class="white-ui-box">
			<div id="divMain" runat="server">
				<h2>Serial Number:
					<asp:Literal runat="server" ID="ltSerialNumber" />
				</h2>
				<p>
					<asp:Label ID="lblGiftCardUsage" runat="server" Text="<%$Tokens:StringResource, admin.editgiftcard.ClickHere %>" />
					<asp:HyperLink ID="lnkGiftCardUsage" runat="server" Text="here." />
				</p>
				<table class="table">
					<tr>
						<td>
							<asp:Label ID="lblAction" runat="server" Text="<%$Tokens:StringResource, admin.editgiftcard.Action %>" />
						</td>
						<td>
							<asp:RadioButtonList ID="rblAction" runat="server">
								<asp:ListItem Value="0" Text="Enabled" />
								<asp:ListItem Value="1" Text="Disabled" />
							</asp:RadioButtonList>
						</td>
					</tr>
					<tr>
						<td>
							<span class="text-danger">*</span><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editgiftcard.SerialNumber %>" />:
						</td>
						<td>
							<asp:TextBox ValidationGroup="main" ID="txtSerial" runat="server" CssClass="text-md"></asp:TextBox>
							<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$Tokens:StringResource, admin.editgiftcard.tooltip.imgSerial %>">
								<i class="fa fa-question-circle fa-lg"></i>
							</asp:Label>
							<asp:RequiredFieldValidator ValidationGroup="main" CssClass="text-danger" ID="RequiredFieldValidator" runat="server" ControlToValidate="txtSerial" SetFocusOnError="true" ErrorMessage="<%$Tokens:StringResource,admin.editgiftcard.FillSerialNumber %>"></asp:RequiredFieldValidator>
						</td>
					</tr>
					<tr>
						<td>
							<span class="text-danger">*</span><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.coupons.ExpirationDate %>" />:
						</td>
						<td>
							<telerik:RadDatePicker ID="txtDate" runat="server" Style="z-index: 150000;" MaxDate="9999-12-31" MinDate="0001-01-01">
								<Calendar UseColumnHeadersAsSelectors="False" UseRowHeadersAsSelectors="False" ViewSelectorText="x" />
							</telerik:RadDatePicker>
							<asp:RequiredFieldValidator CssClass="text-danger" ErrorMessage="<%$Tokens:StringResource, admin.common.FillinExpirationDate %>" ControlToValidate="txtDate" ID="rfvTxtDate" SetFocusOnError="true" runat="server" ValidationGroup="main" />
						</td>
					</tr>
					<tr runat="server" id="PurchasedByCustomerIDTextRow">
						<td>
							<span class="text-danger">*</span><asp:Label runat="server" Text="<%$Tokens:StringResource,admin.editgiftcard.Purchaser %>" ToolTip="<%$Tokens:StringResource,admin.editgiftcard.tooltip.imgCustomer %>" />:
						</td>
						<td>
							<asp:TextBox ID="txtCustomerEmail" runat="server" CssClass="text-md" ToolTip="<%$Tokens:StringResource,admin.editgiftcard.tooltip.imgCustomer %>" OnFocus="ClearCustomerEmailBackground()" OnBlur="CheckCustomerId()"></asp:TextBox>
							<asp:HiddenField ID="hdnCustomerId" runat="server" Value="" />
							<ajax:AutoCompleteExtender ID="autoCompleteCustomerId" runat="server"
								EnableCaching="true"
								MinimumPrefixLength="3"
								CompletionInterval="250"
								CompletionSetCount="15"
								TargetControlID="txtCustomerEmail"
								UseContextKey="True"
								ServiceMethod="GetCompletionList"
								OnClientItemSelected="AutoCompleteItemSelected"
								CompletionListCssClass="autoCompleteResults"
								FirstRowSelected="true" />
							<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$Tokens:StringResource, admin.editgiftcard.tooltip.imgCustomer %>">
												<i class="fa fa-question-circle fa-lg"></i>
							</asp:Label>
							<asp:RequiredFieldValidator ValidationGroup="main" ErrorMessage="<%$Tokens:StringResource, admin.editgiftcard.Purchaser %>" CssClass="text-danger" ControlToValidate="txtCustomerEmail" ID="reqCustEmail" SetFocusOnError="true" runat="server"></asp:RequiredFieldValidator>

							<script type="text/javascript">// <![CDATA[
								function AutoCompleteItemSelected(sender, e) {
									var hdnCustomerId = document.getElementById('<%=hdnCustomerId.ClientID %>');
									hdnCustomerId.value = e.get_value();
								}
								function CheckCustomerId(sender, e) {
									var hdnCustomerId = document.getElementById('<%=hdnCustomerId.ClientID %>');
									var txtCustomerEmail = document.getElementById('<%=txtCustomerEmail.ClientID %>');

									if(hdnCustomerId.value.length === 0) {
										txtCustomerEmail.style.background = "yellow";
										txtCustomerEmail.style.opacity = ".5";
										txtCustomerEmail.value = "Type to search for valid email...";
									}
								}
								function ClearCustomerEmailBackground(sender, e) {
									var txtCustomerEmail = document.getElementById('<%=txtCustomerEmail.ClientID %>');

									txtCustomerEmail.style.background = "#FFF";
									txtCustomerEmail.style.opacity = "1";
									txtCustomerEmail.value = "";
								}
								// ]]>
							</script>
						</td>
					</tr>
					<tr runat="server" id="PurchasedByCustomerIDLiteralRow">
						<td>
							<span>
								<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editgiftcard.Purchaser %>" />:</span>
						</td>
						<td>
							<asp:Literal ID="ltCustomerEmail" runat="server" />
							[ID:
							<asp:Literal ID="ltCustomerID" runat="server" />]
						</td>
					</tr>
					<tr runat="server" id="OrderNumberRow">
						<td>
							<span>
								<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editgiftcard.OriginalOrderNumber %>" />:</span>
						</td>
						<td>
							<asp:Literal ID="txtOrder" runat="server" />
						</td>
					</tr>
					<tr runat="server" id="InitialAmountTextRow">
						<td>
							<span class="text-danger">*</span><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editgiftcard.InitialAmount %>" />:
						</td>
						<td>
							<asp:TextBox ValidationGroup="main" ID="txtAmount" runat="server" CssClass="singleNormal"></asp:TextBox>
							<asp:Literal ID="Literal1" runat="server" />
							<asp:RequiredFieldValidator ValidationGroup="main" ErrorMessage="<%$Tokens:StringResource, admin.editgiftcard.EnterValue %>" ControlToValidate="txtAmount" CssClass="text-danger" ID="RequiredFieldValidator2" SetFocusOnError="true" runat="server"></asp:RequiredFieldValidator>
						</td>
					</tr>
					<tr runat="server" id="InitialAmountLiteralRow">
						<td>
							<span>
								<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editgiftcard.InitialAmount %>" />:</span>
						</td>
						<td>
							<asp:Literal ID="ltAmount" runat="server" />
						</td>
					</tr>
					<tr runat="server" id="RemainingBalanceRow">
						<td>
							<span>
								<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editgiftcard.RemainingBalance %>" />:</span>
						</td>
						<td>
							<asp:Literal ID="ltCurrentBalance" runat="server" />
						</td>
					</tr>
					<tr runat="server" id="GiftCardTypeSelectRow">
						<td>
							<span class="text-danger">*</span><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editgiftcard.GiftCardType %>" />:
						</td>
						<td>
							<asp:DropDownList ValidationGroup="main" ID="ddType" runat="server">
								<asp:ListItem Value="0" Text="<%$Tokens:StringResource, admin.editgiftcard.SelectType %>" />
								<asp:ListItem Value="102" Text="<%$Tokens:StringResource, admin.editgiftcard.Certificate %>" />
								<asp:ListItem Value="101" Text="<%$Tokens:StringResource, admin.common.E-Mail %>" />
								<asp:ListItem Value="100" Text="<%$Tokens:StringResource, admin.editgiftcard.Physical %>" />
							</asp:DropDownList>
							<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$Tokens:StringResource, admin.editgiftcard.tooltip.imgType %>">
												<i class="fa fa-question-circle fa-lg"></i>
							</asp:Label>
							<asp:RequiredFieldValidator ValidationGroup="main" CssClass="text-danger" ControlToValidate="ddType" InitialValue="0" ErrorMessage="<%$Tokens:StringResource, admin.editgiftcard.SelectGiftCardType %>" ID="RequiredFieldValidator5" runat="server" SetFocusOnError="true"></asp:RequiredFieldValidator>
						</td>
					</tr>
					<tr runat="server" id="GiftCardTypeDisplayRow">
						<td>
							<span>
								<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editgiftcard.GiftCardType %>" />:</span>
						</td>
						<td>
							<asp:Literal ID="ltGiftCardType" runat="server" />
						</td>
					</tr>
					<tr runat="server" id="storeMapperRow">
						<td>
							<asp:Panel ID="litStoreMapperHdr" runat="server">
								<span>
									<asp:Literal ID="Literal2" runat="server" Text="<%$Tokens:StringResource, admin.topic.mapstores %>" />
								</span>
							</asp:Panel>
						</td>
						<td>
							<asp:Panel ID="litStoreMapper" runat="server">
								<aspdnsf:EntityToStore ID="etsMapper" runat="server"
									EntityType="GiftCard"
									Text="" />
							</asp:Panel>
						</td>
					</tr>
					<asp:PlaceHolder runat="server" ID="trEmail">
						<tr>
							<td colspan="2">
								<h2>
									<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editgiftcard.IfGiftCard %>" />
									E-Mail type:
								</h2>
							</td>
						</tr>
						<tr>
							<td>
								<span>
									<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editgiftcard.EmailSubject %>" />:</span>
							</td>
							<td>
								<asp:TextBox ID="txtEmailName" runat="server" CssClass="text-md"></asp:TextBox>
								<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$Tokens:StringResource, admin.editgiftcard.tooltip.imgEmailName %>">
									<i class="fa fa-question-circle fa-lg"></i>
								</asp:Label>
							</td>
						</tr>
						<tr>
							<td>
								<span>
									<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editgiftcard.EmailTo %>" />:</span>
							</td>
							<td>
								<asp:TextBox ID="txtEmailTo" runat="server" CssClass="text-md"></asp:TextBox>
								<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$Tokens:StringResource, admin.editgiftcard.tooltip.imgEmailTo %>">
									<i class="fa fa-question-circle fa-lg"></i>
								</asp:Label>
								<aspdnsf:EmailValidator ID="RegularExpressionValidator1" runat="server" CssClass="text-danger" Display="None" ControlToValidate="txtEmailTo" EnableClientScript="false" ErrorMessage="<%$Tokens:StringResource, admin.editgiftcard.InvalidEmail %>" SetFocusOnError="true"></aspdnsf:EmailValidator>
							</td>

						</tr>
						<tr>
							<td>
								<span>
									<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editgiftcard.EmailBody %>" />:</span>
							</td>
							<td>
								<asp:TextBox ID="txtEmailBody" runat="server" TextMode="MultiLine" CssClass="text-lg text-multiline"></asp:TextBox>
								<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$Tokens:StringResource, admin.editgiftcard.tooltip.imgEmailBody %>">
									<i class="fa fa-question-circle fa-lg"></i>
								</asp:Label>
							</td>
						</tr>
					</asp:PlaceHolder>
				</table>
				<asp:ValidationSummary ValidationGroup="main" ID="validationSummary" runat="server" EnableClientScript="true" ShowMessageBox="true" ShowSummary="false" Enabled="true" />
			</div>
		</div>
	</asp:Panel>

	<div class="item-action-bar">
		<asp:HyperLink runat="server"
			CssClass="btn btn-default"
			NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>"
			Text="<%$Tokens:StringResource, admin.common.close %>" />

		<asp:Button ID="btnClose" runat="server" CssClass="btn btn-default" OnClick="btnClose_Click" ValidationGroup="main" Text="<%$Tokens:StringResource, admin.common.SaveAndClose %>" />
		<asp:Button ID="btnSubmit" runat="server" CssClass="btn btn-primary" OnClick="btnSubmit_Click" ValidationGroup="main" Text="<%$Tokens:StringResource, admin.common.Save %>" />
	</div>

</asp:Content>
