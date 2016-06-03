<%@ Page Language="C#" AutoEventWireup="true" Inherits="_PromotionEditor" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="promotion.aspx.cs" %>

<%@ Register TagPrefix="telerik" Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" %>
<%@ Register TagPrefix="aspdnsf" TagName="GeneralInfo" Src="controls/GeneralInfo.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="CSVHelper" Src="controls/CSVHelper.ascx" %>
<%@ Register TagPrefix="aspdnsfs" TagName="StoreSelector" Src="Controls/StoreSelector.ascx" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="head">
	<link type="text/css" href="css/promotions.css" rel="stylesheet" />
	<script type="text/javascript" src="Scripts/promotions.js"></script>
</asp:Content>

<asp:Content ID="Content2" runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<aspdnsf:ReturnUrlTracker runat="server" ID="ReturnUrlTracker" DefaultReturnUrl="promotions.aspx" />
	<div class="admin-module">
        <div class="row">
            <div id="divMessage" runat="server" visible="false">
                <asp:Label runat="server" ID="lblMessage" />
            </div>
        </div>
		<h1>
			<i class="fa fa-barcode"></i>
			<asp:Literal ID="HeaderText" runat="server" />
		</h1>
		<aspdnsf:AlertMessage ID="ctlAlertMessage" runat="server" />
			<div class="item-action-bar">
				<asp:HyperLink runat="server" ID="btnCloseTop" CssClass="btn btn-default" NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>" Text="<%$Tokens:StringResource, admin.common.close %>" />
				<asp:Button ID="btnSaveAndCloseTop" runat="server" CssClass="btn btn-default" OnClick="btnSaveAndClose_Click" ValidationGroup="Main" Text="<%$Tokens:StringResource, admin.common.SaveAndClose %>" />
				<asp:Button ID="btnSubmitTop" runat="server" CssClass="btn btn-primary" OnClick="btnSubmit_Click" ValidationGroup="Main" Text="<%$Tokens:StringResource, admin.common.Save %>" />
			</div>
		<asp:LinqDataSource ID="PromotionDataSource" runat="server" ContextTypeName="AspDotNetStorefront.Promotions.Data.EntityContextDataContext, AspDotNetStorefrontPromotions" TableName="Promotions" AutoPage="true" AutoSort="true" EnableDelete="true" EnableInsert="true" EnableUpdate="true" />		
		<asp:Panel ID="pnlUpdate" runat="server">
			<fieldset>	
				<asp:Label ID="lblTitle" runat="server" />
				<div class="promotion-top-expand-button tdButtonRow">
					<input runat="server" type="button" value="<%$Tokens:StringResource, admin.PromotionEditor.ExpandAllButton%>" class="btn btn-default btn-sm expandAllExpandables" />
					<input runat="server" type="button" value="<%$Tokens:StringResource, admin.PromotionEditor.CollapseAllButton%>" class="btn btn-default btn-sm collapseAllExpandables" />
				</div>
				<table class="promotionTable">					
					<tr>
						<td valign="top" style="width: 400px;">
							
							<table class="promoFieldTable">
								
								<tr>
									<td class="tdInfo">
										<aspdnsf:GeneralInfo ID="GeneralInfo6" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoPromoTitle%>" />
									</td>
									<td class="tdLabel">
										<span class="text-danger">*</span><asp:Label ID="lblText" Text="<%$Tokens:StringResource, admin.PromotionEditor.TitleLabel%>" AssociatedControlID="txtName" runat="server" />
									</td>
									<td class="tdValue">
										<asp:TextBox ID="txtName" runat="server" CssClass="textfield" Columns="60" />
										<asp:RequiredFieldValidator ID="RequiredFieldValidator1" ValidationGroup="Main" CssClass="error" runat="server" ControlToValidate="txtName" ErrorMessage="<%$Tokens:StringResource, admin.PromotionEditor.TitleRequired%>" Display="Dynamic" EnableClientScript="true" />
									</td>
								</tr>
								<tr>
									<td class="tdInfo">
										<aspdnsf:GeneralInfo ID="GeneralInfo21" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoPromoCode%>" />
									</td>
									<td class="tdLabel">
										<span class="text-danger">*</span><asp:Label ID="lblCode" Text="<%$Tokens:StringResource, admin.PromotionEditor.CodeLabel%>" AssociatedControlID="txtCode" runat="server" />
									</td>
									<td class="tdValue">
										<asp:TextBox ID="txtCode" runat="server" CssClass="textfield" Columns="60" />
										<asp:RequiredFieldValidator runat="server" ValidationGroup="Main" CssClass="error" ControlToValidate="txtCode" ErrorMessage="<%$Tokens:StringResource, admin.PromotionEditor.CodeRequired%>" Display="Dynamic" EnableClientScript="true" />
									</td>
								</tr>
								<tr>
									<td class="tdInfo">
										<aspdnsf:GeneralInfo ID="GeneralInfo22" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoPromoDescription%>" />
									</td>
									<td class="tdLabel">
										<span class="text-danger">*</span><asp:Label ID="lblDescription" Text="<%$Tokens:StringResource, admin.PromotionEditor.DescriptionLabel%>" AssociatedControlID="txtDescription" runat="server" />
									</td>
									<td class="tdValue">
										<asp:TextBox ID="txtDescription" runat="server" CssClass="textfield" Columns="60" />
										<asp:RequiredFieldValidator runat="server" ValidationGroup="Main" CssClass="error" ControlToValidate="txtDescription" ErrorMessage="<%$Tokens:StringResource, admin.PromotionEditor.DescriptionRequired%>" Display="Dynamic" EnableClientScript="true" />
									</td>
								</tr>
								<tr>
									<td class="tdInfo">
										<aspdnsf:GeneralInfo ID="GeneralInfo23" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoPromoPriority%>" />
									</td>
									<td class="tdLabel">
										<span class="text-danger">*</span><asp:Label ID="lblPriority" Text="<%$Tokens:StringResource, admin.PromotionEditor.PriorityLabel%>" AssociatedControlID="txtPriority" runat="server" />
									</td>
									<td class="tdValue">
										<asp:TextBox ID="txtPriority" runat="server" CssClass="textfield" Columns="5" />
										<asp:RequiredFieldValidator ID="RequiredFieldValidator2" ValidationGroup="Main" CssClass="error" runat="server" ControlToValidate="txtPriority" ErrorMessage="<%$Tokens:StringResource, admin.PromotionEditor.PriorityRequired%>" Display="Dynamic" EnableClientScript="true" />
										<asp:CompareValidator ID="cmpValTxtPriority" ValidationGroup="Main" runat="server" CssClass="error" ControlToValidate="txtPriority" ValueToCompare="0" Operator="GreaterThanEqual" ErrorMessage="<%$Tokens:StringResource, admin.PromotionEditor.ValidationIncorrectNumberFormat%>" Display="Dynamic" Type="Integer" />
									</td>
								</tr>
								<tr>
									<td class="tdInfo">
										<aspdnsf:GeneralInfo ID="GeneralInfo24" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoPromoStatus%>" />
									</td>
									<td class="tdLabel">
										<asp:Label ID="lblChk" Text="<%$Tokens:StringResource, admin.PromotionEditor.ActiveLabel%>" AssociatedControlID="rblActive" runat="server" />
									</td>
									<td class="tdValue">
										<asp:RadioButtonList runat="server" ID="rblActive" RepeatDirection="Horizontal">
											<asp:ListItem Value="true" Text="Active" Selected="True"></asp:ListItem>
											<asp:ListItem Value="false" Text="Inactive"></asp:ListItem>
										</asp:RadioButtonList>
									</td>
								</tr>
								<tr>
									<td class="tdInfo">
										<aspdnsf:GeneralInfo ID="GeneralInfo25" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoPromoAutoAssigned%>" />
									</td>
									<td class="tdLabel">
										<asp:Label ID="lblAutoAssigned" Text="<%$Tokens:StringResource, admin.PromotionEditor.AutoAssignedLabel%>" AssociatedControlID="chkAutoAssigned" runat="server" />
									</td>
									<td class="tdValue">
										<asp:CheckBox ID="chkAutoAssigned" runat="server" />
									</td>
								</tr>
								<tr>
									<td class="tdInfo">
										<aspdnsf:GeneralInfo ID="GeneralInfo26" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoPromoUsage%>" />
									</td>
									<td class="tdLabel">
										<asp:Label ID="lblUsage" Text="<%$Tokens:StringResource, admin.PromotionEditor.UsageLabel%>" AssociatedControlID="txtUsageText" runat="server" />
									</td>
									<td class="tdValue">
										<asp:TextBox ID="txtUsageText" runat="server" CssClass="textfield" Columns="60" />
									</td>
								</tr>
								<tr>
									<td class="tdInfo">
										<aspdnsf:GeneralInfo ID="GeneralInfo28" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoPromoProductMessage%>" />
									</td>
									<td class="tdLabel">
										<asp:Label ID="lblCallToAction" Text="<%$Tokens:StringResource, admin.PromotionEditor.CallToActionLabel%>" AssociatedControlID="CallToActionTextbox" runat="server" />
									</td>
									<td class="tdValue">
										<asp:TextBox ID="CallToActionTextbox" runat="server" CssClass="textfield" Columns="60" />
									</td>
								</tr>
								<tr runat="server" id="trStoreMapping">
									<td class="tdInfo promotion-align-top">
										<aspdnsf:GeneralInfo ID="GeneralInfo29" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoPromoStores%>" />
									</td>
									<td class="tdLabel promotion-align-top">
										<asp:Label ID="lblStore" Text="<%$Tokens:StringResource, admin.PromotionEditor.StoreLabel%>" AssociatedControlID="promotionStoreMapper" runat="server" />
									</td>
									<td class="tdValue">
										<aspdnsfs:StoreSelector ID="promotionStoreMapper" runat="server" ShowText="false" SelectMode="MultiCheckList" ShowDefaultForAllStores="false" ListRepeatDirection="Vertical" />
									</td>
								</tr>
								<tr>
                                    <td colspan="3">
										<hr />
                                        <asp:Label ID="lblError" runat="server" CssClass="error" />
                                    </td>
                                </tr>      
							</table>
							<asp:HiddenField ID="txtId" runat="server" />
						</td>
						<td valign="top">
							<asp:Panel ID="pnlRulesDiscounts" runat="server" Visible="false" CssClass="pnlRulesDiscounts">
								<div class="promotion-panels">
									<div class="panel panel-default promotion-panel-adjustments">
										<div class="panel-heading">
											<h4 class="panel-title">
												<a data-toggle="collapse" data-parent="#accordion" href="#collapseOne">&nbsp;<asp:Label CssClass="checkLabel" ID="Label13" Text="<%$Tokens:StringResource, admin.PromotionEditor.DiscountLiteral%>" runat="server" />
												</a>
											</h4>
										</div>
										<div id="collapseOne" class="panel-collapse collapse">
											<div class="panel-body">
												<div class="checkTarget">
													<table width="100%;">
														<tr id="trShippingDiscount" runat="server">
															<td class="emptyCell">&nbsp;</td>
															<td>
																<asp:Panel ID="pnlShippingDiscount" runat="server">
																	<!-- Shipping Discount -->
																	<h3 class="checkHeader">
																		<aspdnsf:GeneralInfo ID="GeneralInfo17" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoShippingDiscount%>" />
																		<asp:CheckBox ID="chkRuleShippingDiscount" runat="server" CssClass="checkToggler" />
																		<asp:Label CssClass="checkLabel" ID="litDiscountShipping" Text="<%$Tokens:StringResource, admin.PromotionEditor.DiscountShippingLiteral%>" runat="server" />
																	</h3>
																	<div class="checkTarget">
																		<table>
																			<tr>
																				<td class="tdLabel">
																					<asp:Label ID="Label1" Text="<%$Tokens:StringResource, admin.PromotionEditor.DiscountTypeLabel%>" AssociatedControlID="ddlRuleShippingDiscountType" runat="server" />
																				</td>
																				<td>
																					<asp:DropDownList ID="ddlRuleShippingDiscountType" runat="server">
																						<asp:ListItem Value="0" Text="<%$Tokens:StringResource, admin.PromotionEditor.DropDownValuesFixed%>" />
																						<asp:ListItem Value="1" Text="<%$Tokens:StringResource, admin.PromotionEditor.DropDownValuesPercentage%>" />
																					</asp:DropDownList>
																				</td>
																			</tr>
																			<tr>
																				<td class="tdLabel">
																					<span class="text-danger">*</span><asp:Label ID="Label2" Text="<%$Tokens:StringResource, admin.PromotionEditor.DiscountAmountLabel%>" AssociatedControlID="txtRuleShippingDiscountAmount" runat="server" />
																				</td>
																				<td>
																					<asp:TextBox ID="txtRuleShippingDiscountAmount" runat="server" />
																					<asp:RangeValidator runat="server" ControlToValidate="txtRuleShippingDiscountAmount" ErrorMessage="<%$Tokens:StringResource, admin.PromotionEditor.ValidationIncorrectNumberFormat%>" Type="Double" Display="Dynamic" MinimumValue="0" MaximumValue="999999999999999999" />
																				</td>
																			</tr>
																			<tr>
																				<td class="multiLineLabel">
																					<asp:Label ID="Label3" Text="<%$Tokens:StringResource, admin.PromotionEditor.ShippingMethodsLabel%>" AssociatedControlID="txtRuleShippingMethodID" runat="server" />
																				</td>
																				<td>
																					<asp:TextBox ID="txtRuleShippingMethodID" runat="server" Style="display: none;" />
																					<aspdnsf:CSVHelper ID="CSVHelper2" runat="server" CSVTextBoxID="txtRuleShippingMethodID" UniqueJSID="txtRuleShippingMethodID" EntityType="ShippingMethod" CSVSearchButtonText="<%$Tokens:StringResource, admin.common.Search%>" />
																				</td>
																			</tr>
																		</table>
																	</div>
																</asp:Panel>
															</td>
														</tr>
														<tr id="trShippingOnlyDiscount" runat="server">
															<td class="emptyCell">&nbsp;</td>
															<td>
																<asp:Panel ID="pnlShippingOnlyDiscount" runat="server">
																	<!-- Shipping Only Discount -->
																	<h3 class="checkHeader">
																		<aspdnsf:GeneralInfo runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoShipping%>" />
																		<asp:RadioButton GroupName="rblDiscountList" ID="chkShippingOnlyDiscount" runat="server" CssClass="checkToggler" />
																		<asp:Label CssClass="checkLabel" ID="Literal2" Text="<%$Tokens:StringResource, admin.PromotionEditor.DiscountShippingOnlyLiteral%>" runat="server" />
																	</h3>
																	<div></div>
																</asp:Panel>
															</td>
														</tr>
														<tr>
															<td class="emptyCell">&nbsp;</td>
															<td>
																<!-- Order Discount -->
																<h3 class="checkHeader">
																	<aspdnsf:GeneralInfo ID="GeneralInfo18" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoEntireOrder%>" />
																	<asp:RadioButton GroupName="rblDiscountList" ID="chkRuleOrderDiscount" runat="server" CssClass="checkToggler" />
																	<asp:Label CssClass="checkLabel" ID="Literal3" Text="<%$Tokens:StringResource, admin.PromotionEditor.DiscountOrderLiteral%>" runat="server" />
																</h3>
																<div class="checkTarget">
																	<table>
																		<tr>
																			<td class="tdLabel">
																				<asp:Label ID="Label44" Text="<%$Tokens:StringResource, admin.PromotionEditor.DiscountTypeLabel%>" AssociatedControlID="ddlRuleOrderDiscountType" runat="server" />
																			</td>
																			<td>
																				<asp:DropDownList ID="ddlRuleOrderDiscountType" runat="server">
																					<asp:ListItem Value="0" Text="<%$Tokens:StringResource, admin.PromotionEditor.DropDownValuesFixed%>" />
																					<asp:ListItem Value="1" Text="<%$Tokens:StringResource, admin.PromotionEditor.DropDownValuesPercentage%>" />
																				</asp:DropDownList>
																			</td>
																		</tr>
																		<tr>
																			<td class="tdLabel">
																				<span class="text-danger">*</span><asp:Label ID="Label4" Text="<%$Tokens:StringResource, admin.PromotionEditor.DiscountAmountLabel%>" AssociatedControlID="txtRuleOrderDiscountAmount" runat="server" />
																			</td>
																			<td>
																				<asp:TextBox ID="txtRuleOrderDiscountAmount" runat="server" />
																				<asp:RangeValidator runat="server" ControlToValidate="txtRuleOrderDiscountAmount" ErrorMessage="<%$Tokens:StringResource, admin.PromotionEditor.ValidationIncorrectNumberFormat%>" Type="Double" Display="Dynamic" MinimumValue="0" MaximumValue="999999999999999999" />
																			</td>
																		</tr>
																	</table>
																</div>
															</td>
														</tr>
														<tr>
															<td class="emptyCell">&nbsp;</td>
															<td>
																<!-- Line Item Discount -->
																<h3 class="checkHeader">
																	<aspdnsf:GeneralInfo ID="GeneralInfo19" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoLineItems%>" />
																	<asp:RadioButton GroupName="rblDiscountList" ID="chkRuleLineItemDiscount" runat="server" CssClass="checkToggler" />
																	<asp:Label CssClass="checkLabel" ID="Literal4" Text="<%$Tokens:StringResource, admin.PromotionEditor.DiscountApplicableLineItemLiteral%>" runat="server" />
																</h3>
																<div class="checkTarget">
																	<table>
																		<tr>
																			<td class="tdLabel">
																				<asp:Label ID="Label5" Text="<%$Tokens:StringResource, admin.PromotionEditor.DiscountTypeLabel%>" AssociatedControlID="ddlRuleLineItemDiscountType" runat="server" />
																			</td>
																			<td>
																				<asp:DropDownList ID="ddlRuleLineItemDiscountType" runat="server">
																					<asp:ListItem Value="0" Text="<%$Tokens:StringResource, admin.PromotionEditor.DropDownValuesFixed%>" />
																					<asp:ListItem Value="1" Text="<%$Tokens:StringResource, admin.PromotionEditor.DropDownValuesPercentage%>" />
																				</asp:DropDownList>
																			</td>
																		</tr>
																		<tr>
																			<td class="tdLabel">
																				<span class="text-danger">*</span><asp:Label ID="Label6" Text="<%$Tokens:StringResource, admin.PromotionEditor.DiscountAmountLabel%>" AssociatedControlID="txtRuleLineItemDiscountAmount" runat="server" />
																			</td>
																			<td>
																				<asp:TextBox ID="txtRuleLineItemDiscountAmount" runat="server" />
																				<asp:RangeValidator runat="server" ControlToValidate="txtRuleLineItemDiscountAmount" ErrorMessage="<%$Tokens:StringResource, admin.PromotionEditor.ValidationIncorrectNumberFormat%>" Type="Double" Display="Dynamic" MinimumValue="0" MaximumValue="999999999999999999" />
																			</td>
																		</tr>
																	</table>
																</div>
															</td>
														</tr>
														<tr>
															<td class="emptyCell">&nbsp;</td>
															<td>
																<!-- Gift With Purchase -->
																<h3 class="checkHeader">
																	<aspdnsf:GeneralInfo ID="GeneralInfo20" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoAutomaticallyAdd%>" />
																	<asp:RadioButton GroupName="rblDiscountList" ID="chkRuleGiftWithPurchase" runat="server" CssClass="checkToggler" />
																	<asp:Label CssClass="checkLabel" ID="Literal5" Text="<%$Tokens:StringResource, admin.PromotionEditor.GiftWithPurchaseLiteral%>" runat="server" />
																</h3>
																<div class="checkTarget">
																	<table>
																		<tr>
																			<td class="tdLabel" valign="top">
																				<asp:Label ID="Label7" Text="<%$Tokens:StringResource, admin.PromotionEditor.ProductsLabel%>" AssociatedControlID="txtRuleGiftWithPurchaseProductId" runat="server" />
																			</td>
																			<td>
																				<asp:TextBox ID="txtRuleGiftWithPurchaseProductId" runat="server" Style="display: none;" />
																				<aspdnsf:CSVHelper ID="CSVHelper3" runat="server" CSVTextBoxID="txtRuleGiftWithPurchaseProductId" UniqueJSID="txtRuleGiftWithPurchaseProductId" CSVSearchButtonText="<%$Tokens:StringResource, admin.common.Search%>" />
																			</td>
																		</tr>
																		<tr>
																			<td class="tdLabel">
																				<span class="text-danger">*</span><asp:Label ID="Label8" Text="<%$Tokens:StringResource, admin.PromotionEditor.DiscountPercentageLabel%>" AssociatedControlID="txtGiftWithPurchaseDiscountAmount" runat="server" />
																			</td>
																			<td>
																				<asp:TextBox ID="txtGiftWithPurchaseDiscountAmount" runat="server" />
																				<asp:RangeValidator runat="server" ControlToValidate="txtGiftWithPurchaseDiscountAmount" ErrorMessage="<%$Tokens:StringResource, admin.PromotionEditor.ValidationIncorrectNumberFormat%>" Type="Double" Display="Dynamic" MinimumValue="0" MaximumValue="999999999999999999" />
																			</td>
																		</tr>
																		<tr>
																			<td class="tdLabel">
																				<asp:Label ID="Label9" Text="<%$Tokens:StringResource, admin.PromotionEditor.MatchQuantitesLabel%>" AssociatedControlID="chkMatchQuantites" runat="server" />
																			</td>
																			<td>
																				<asp:CheckBox ID="chkMatchQuantites" Checked="true" runat="server" />
																			</td>
																		</tr>
																	</table>
																</div>
															</td>
														</tr>
													</table>
												</div>
											</div>
										</div>
										<div class="panel-heading">
											<h4 class="panel-title">
												<a data-toggle="collapse" data-parent="#accordion" href="#collapseTwo">&nbsp;<asp:Label CssClass="checkLabel" ID="Label14" Text="<%$Tokens:StringResource, admin.PromotionEditor.LimitsHeaderLiteral%>" runat="server" />
												</a>
											</h4>
										</div>
										<div id="collapseTwo" class="panel-collapse collapse">
											<div class="panel-body">
												<div class="checkTarget">
													<table width="100%;">
														<tr>
															<td class="emptyCell">&nbsp;</td>
															<td>
																<!-- StartDateRules -->
																<h3 class="checkHeader">
																	<aspdnsf:GeneralInfo ID="StartGeneralInfo" runat="server" />
																	<asp:CheckBox ID="optRuleStartDate" runat="server" CssClass="checkToggler" />
																	<asp:Label CssClass="checkLabel" ID="Literal6" Text="<%$Tokens:StringResource, admin.PromotionEditor.EnableStartDateLiteral%>" runat="server" />
																</h3>
																<div class="checkTarget">
																	<table cellpadding="0" cellspacing="0">
																		<tr>
																			<td class="tdLabel">
																				<span class="text-danger">*</span><asp:Label ID="Label10" Text="<%$Tokens:StringResource, admin.PromotionEditor.StartDateLabel%>" AssociatedControlID="txtRuleStartDate" runat="server" />
																			</td>
																			<td>
																				<telerik:RadDateTimePicker InputMode="DateTimePicker" ID="txtRuleStartDate" runat="server" Style="z-index: 150000;" MaxDate="9999-12-31" MinDate="0001-01-01">
																					<Calendar runat="server" UseColumnHeadersAsSelectors="False" UseRowHeadersAsSelectors="False" ViewSelectorText="x" />
																				</telerik:RadDateTimePicker>
																			</td>
																		</tr>
																	</table>
																</div>
															</td>
														</tr>
														<tr>
															<td class="emptyCell">&nbsp;</td>
															<td>
																<!-- ExpirationDateRule -->
																<h3 class="checkHeader">
																	<aspdnsf:GeneralInfo ID="ExpirationGeneralInfo" runat="server" />
																	<asp:CheckBox ID="optRuleExpirationDate" runat="server" CssClass="checkToggler" />
																	<asp:Label CssClass="checkLabel" ID="Literal7" Text="<%$Tokens:StringResource, admin.PromotionEditor.EnableExpirationDateLiteral%>" runat="server" />
																</h3>
																<div class="checkTarget">
																	<table cellpadding="0" cellspacing="0">
																		<tr>
																			<td class="tdLabel">
																				<span class="text-danger">*</span><asp:Label ID="Label11" Text="<%$Tokens:StringResource, admin.PromotionEditor.ExpirationDateLabel%>" AssociatedControlID="txtRuleExpirationDate" runat="server" />
																			</td>
																			<td>
																				<telerik:RadDateTimePicker InputMode="DateTimePicker" ID="txtRuleExpirationDate" runat="server" Style="z-index: 150000;" MaxDate="9999-12-31" MinDate="0001-01-01">
																					<Calendar runat="server" UseColumnHeadersAsSelectors="False" UseRowHeadersAsSelectors="False" ViewSelectorText="x" />
																				</telerik:RadDateTimePicker>
																			</td>
																		</tr>
																	</table>
																</div>
															</td>
														</tr>
														<tr>
															<td class="emptyCell">&nbsp;</td>
															<td>
																<!-- ExpirationNumberOfUsesDateRule -->
																<h3 class="checkHeader">
																	<aspdnsf:GeneralInfo ID="GeneralInfo1" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoMaxUses%>" />
																	<asp:CheckBox ID="optRuleExpirationNumberOfUses" runat="server" CssClass="checkToggler" />
																	<asp:Label CssClass="checkLabel" ID="Literal8" Text="<%$Tokens:StringResource, admin.PromotionEditor.EnableMaximumNumberofUsesLiteral%>" runat="server" />
																</h3>
																<div class="checkTarget">
																	<table cellpadding="0" cellspacing="0">
																		<tr>
																			<td class="tdLabel">
																				<span class="text-danger">*</span><asp:Label ID="Label12" Text="<%$Tokens:StringResource, admin.PromotionEditor.MaximumNumberofUsesLabel%>" AssociatedControlID="txtRuleExpirationNumberOfUses" runat="server" />
																			</td>
																			<td>
																				<asp:TextBox ID="txtRuleExpirationNumberOfUses" runat="server" Width="100" />
																				<asp:RangeValidator runat="server" ControlToValidate="txtRuleExpirationNumberOfUses" ErrorMessage="<%$Tokens:StringResource, admin.PromotionEditor.ValidationIncorrectNumberFormat%>" Type="Integer" Display="Dynamic" MinimumValue="0" MaximumValue="99999999" />
																			</td>
																		</tr>
																		<tr>
																			<td class="tdLabel">
																				<asp:Literal ID="litPerCustomer2" Text="<%$Tokens:StringResource, admin.PromotionEditor.PerCustomerLiteral%>" runat="server" />
																			</td>
																			<td>
																				<asp:CheckBox ID="chkRuleExpirationNumberOfUsesPerCustomer" runat="server" />
																			</td>
																		</tr>
																	</table>
																</div>
															</td>
														</tr>
													</table>
												</div>
											</div>
										</div>
										<div class="panel-heading">
											<h4 class="panel-title">
												<a data-toggle="collapse" data-parent="#accordion" href="#collapseThree">&nbsp;<asp:Label CssClass="checkLabel" ID="Label18" Text="<%$Tokens:StringResource, admin.PromotionEditor.RequirementsLiteral%>" runat="server" />
												</a>
											</h4>
										</div>
										<div id="collapseThree" class="panel-collapse collapse">
											<div class="panel-body">
												<div class="checkTarget">
													<table width="100%;">
														<tr>
															<td class="emptyCell">&nbsp;</td>
															<td>
																<!-- Product ID Rule -->
																<h3 class="checkHeader">
																	<aspdnsf:GeneralInfo ID="GeneralInfo2" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoValidForProducts%>" />
																	<asp:CheckBox ID="chkRuleProductId" runat="server" CssClass="checkToggler" />
																	<asp:Label CssClass="checkLabel" ID="Literal11" Text="<%$Tokens:StringResource, admin.PromotionEditor.EnableProductRequirementLiteral%>" runat="server" />
																</h3>
																<div class="checkTarget">
																	<table>
																		<tr>
																			<td class="multiLineLabel">
																				<span class="text-danger">*</span><asp:Label ID="Label15" Text="<%$Tokens:StringResource, admin.PromotionEditor.ProductsLabel%>" AssociatedControlID="txtRuleProductIds" runat="server" />
																			</td>
																			<td>
																				<asp:TextBox ID="txtRuleProductIds" runat="server" Width="450" Style="display: none;" />
																				<aspdnsf:CSVHelper ID="relatedHelper" runat="server" CSVTextBoxID="txtRuleProductIds" UniqueJSID="txtRuleProductIds" CSVSearchButtonText="<%$Tokens:StringResource, admin.common.Search%>" />
																			</td>
																		</tr>
																		<tr>
																			<td class="tdLabel">
																				<asp:Label ID="Label16" Text="<%$Tokens:StringResource, admin.PromotionEditor.RequireLabel%>" AssociatedControlID="rblProductsAllOrAny" runat="server" />
																			</td>
																			<td>
																				<asp:RadioButtonList ID="rblProductsAllOrAny" runat="server" RepeatDirection="Horizontal">
																					<asp:ListItem Text="All" Value="all" />
																					<asp:ListItem Text="Any" Value="any" />
																				</asp:RadioButtonList>
																			</td>
																		</tr>
																		<tr>
																			<td class="tdLabel">
																				<asp:Label ID="Label17" Text="<%$Tokens:StringResource, admin.PromotionEditor.RequireQuantityLabel%>" AssociatedControlID="txtRuleProductIdsRequireQuantity" runat="server" />
																			</td>
																			<td>
																				<asp:TextBox ID="txtRuleProductIdsRequireQuantity" runat="server" Width="50" />
																				<asp:RangeValidator runat="server" ControlToValidate="txtRuleProductIdsRequireQuantity" ErrorMessage="<%$Tokens:StringResource, admin.PromotionEditor.ValidationIncorrectNumberFormat%>" Type="Integer" Display="Dynamic" MinimumValue="0" MaximumValue="99999999" />
																			</td>
																		</tr>
																	</table>
																</div>
															</td>
														</tr>
														<tr>
															<td class="emptyCell">&nbsp;</td>
															<td>
																<!-- Category Rule -->
																<h3 class="checkHeader">
																	<aspdnsf:GeneralInfo ID="GeneralInfo3" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoProductFromCategory%>" />
																	<asp:CheckBox ID="chkRuleCategories" runat="server" CssClass="checkToggler" />
																	<asp:Label CssClass="checkLabel" ID="Literal13" Text="<%$Tokens:StringResource, admin.PromotionEditor.EnableCategoryRequirementLiteral%>" runat="server" />
																</h3>
																<div class="checkTarget">
																	<table>
																		<tr>
																			<td class="multiLineLabel">
																				<asp:Label ID="Label21" Text="<%$Tokens:StringResource, admin.PromotionEditor.CategoriesLabel%>" AssociatedControlID="txtRuleCategories" runat="server" />
																			</td>
																			<td>
																				<asp:TextBox ID="txtRuleCategories" runat="server" Width="450" Style="display: none;" />
																				<aspdnsf:CSVHelper runat="server" CSVTextBoxID="txtRuleCategories" UniqueJSID="txtRuleCategories" EntityType="Category" CSVSearchButtonText="<%$Tokens:StringResource, admin.common.Search%>" />
																			</td>
																		</tr>
																	</table>
																</div>
															</td>
														</tr>
														<tr>
															<td class="emptyCell">&nbsp;</td>
															<td>
																<!-- Section Rule -->
																<h3 class="checkHeader">
																	<aspdnsf:GeneralInfo ID="GeneralInfo4" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoProductFromSection%>" />
																	<asp:CheckBox ID="chkRuleSections" runat="server" CssClass="checkToggler" />
																	<asp:Label CssClass="checkLabel" ID="Literal14" Text="<%$Tokens:StringResource, admin.PromotionEditor.EnableSectionRequirementLiteral%>" runat="server" />
																</h3>
																<div class="checkTarget">
																	<table>
																		<tr>
																			<td class="multiLineLabel">
																				<span class="text-danger">*</span><asp:Label ID="Label22" Text="<%$Tokens:StringResource, admin.PromotionEditor.SectionsLabel%>" AssociatedControlID="txtRuleSections" runat="server" />
																			</td>
																			<td>
																				<asp:TextBox ID="txtRuleSections" runat="server" Width="450" Style="display: none;" />
																				<aspdnsf:CSVHelper runat="server" CSVTextBoxID="txtRuleSections" UniqueJSID="txtRuleSections" EntityType="Section" CSVSearchButtonText="<%$Tokens:StringResource, admin.common.Search%>" />
																			</td>
																		</tr>
																	</table>
																</div>
															</td>
														</tr>
														<tr>
															<td class="emptyCell">&nbsp;</td>
															<td>
																<!-- Manufacturer Rule -->
																<h3 class="checkHeader">
																	<aspdnsf:GeneralInfo ID="GeneralInfo5" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoProductFromManufacturer%>" />
																	<asp:CheckBox ID="chkRuleManufacturers" runat="server" CssClass="checkToggler" />
																	<asp:Label CssClass="checkLabel" ID="Literal15" Text="<%$Tokens:StringResource, admin.PromotionEditor.EnableManufacturerRequirementLiteral%>" runat="server" />
																</h3>
																<div class="checkTarget">
																	<table>
																		<tr>
																			<td class="multiLineLabel">
																				<span class="text-danger">*</span><asp:Label ID="Label23" Text="<%$Tokens:StringResource, admin.PromotionEditor.ManufacturersLabel%>" AssociatedControlID="txtRuleManufacturers" runat="server" />
																			</td>
																			<td>
																				<asp:TextBox ID="txtRuleManufacturers" runat="server" Width="450" Style="display: none;" />
																				<aspdnsf:CSVHelper runat="server" CSVTextBoxID="txtRuleManufacturers" UniqueJSID="txtRuleManufacturers" EntityType="Manufacturer" CSVSearchButtonText="<%$Tokens:StringResource, admin.common.Search%>" />
																			</td>
																		</tr>
																	</table>
																</div>
															</td>
														</tr>
														<tr>
															<td class="emptyCell">&nbsp;</td>
															<td>
																<!-- Minimum Cart Amount Rule -->
																<h3 class="checkHeader">
																	<aspdnsf:GeneralInfo ID="GeneralInfo7" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoSubtotalGreater%>" />
																	<asp:CheckBox ID="chkRuleCartAmount" runat="server" CssClass="checkToggler" />
																	<asp:Label CssClass="checkLabel" ID="Literal17" Text="<%$Tokens:StringResource, admin.PromotionEditor.EnableMinimumCartSubtotalRequirementLiteral%>" runat="server" />
																</h3>
																<div class="checkTarget">
																	<table>
																		<tr>
																			<td>
																				<span class="text-danger">*</span><asp:Literal ID="Literal18" Text="<%$Tokens:StringResource, admin.PromotionEditor.MinimumAmountLiteral%>" runat="server" />
																				<asp:TextBox ID="txtRuleCartAmount" runat="server" Width="50" />
																				<asp:RangeValidator runat="server" ControlToValidate="txtRuleCartAmount" ErrorMessage="<%$Tokens:StringResource, admin.PromotionEditor.ValidationIncorrectNumberFormat%>" Type="Double" Display="Dynamic" MinimumValue="0" MaximumValue="999999999999999999" />
																			</td>
																		</tr>
																	</table>
																</div>
															</td>
														</tr>
														<tr>
															<td class="emptyCell">&nbsp;</td>
															<td>
																<!-- Email Address Rule -->
																<h3 class="checkHeader">
																	<aspdnsf:GeneralInfo ID="GeneralInfo8" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoEmailAddressIncluded%>" />
																	<asp:CheckBox ID="chkRuleEmail" runat="server" CssClass="checkToggler" />
																	<asp:Label CssClass="checkLabel" ID="Literal19" Text="<%$Tokens:StringResource, admin.PromotionEditor.EnableEmailAddressRequirementLiteral%>" runat="server" />
																</h3>
																<div class="checkTarget">
																	<table>
																		<tr>
																			<td class="tdLabel">
																				<span class="text-danger">*</span><asp:Label ID="Label26" Text="<%$Tokens:StringResource, admin.PromotionEditor.EmailAddressesLabel%>" AssociatedControlID="txtRuleEmailAddresses" runat="server" />
																			</td>
																			<td>
																				<asp:TextBox ID="txtRuleEmailAddresses" runat="server" Width="450" />
																			</td>
																		</tr>
																		<tr>
																			<td colspan="2">
																				<asp:Literal ID="Literal20" Text="<%$Tokens:StringResource, admin.PromotionEditor.UploadfromcsvfileLiteral%>" runat="server" />
																				<asp:FileUpload ID="fileUpload" runat="server" CssClass="inline-file-upload" />
																				<asp:Button ID="btnUpload" runat="server" Text="<%$Tokens:StringResource, admin.PromotionEditor.UploadButton%>" OnClick="btnUpload_Click" CssClass="btn btn-default" /><br />
																				<asp:Label ID="lblEmailUploadError" runat="server" CssClass="error" />
																			</td>
																		</tr>
																	</table>
																</div>
															</td>
														</tr>
														<tr>
															<td class="emptyCell">&nbsp;</td>
															<td>
																<!-- Customer Level Rule -->
																<h3 class="checkHeader">
																	<aspdnsf:GeneralInfo ID="GeneralInfo9" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoCustomerLevels%>" />
																	<asp:CheckBox ID="chkRuleCustomerLevel" runat="server" CssClass="checkToggler" />
																	<asp:Label CssClass="checkLabel" ID="Literal21" Text="<%$Tokens:StringResource, admin.PromotionEditor.EnableCustomerLevelRequirementLiteral%>" runat="server" />
																</h3>
																<div class="checkTarget">
																	<table>
																		<tr>
																			<td class="multiLineLabel">
																				<span class="text-danger">*</span><asp:Label ID="Label27" Text="<%$Tokens:StringResource, admin.PromotionEditor.CustomerLevelLabel%>" AssociatedControlID="txtRuleCustomerLevels" runat="server" />
																			</td>
																			<td>
																				<asp:TextBox ID="txtRuleCustomerLevels" runat="server" Width="450" Style="display: none;" />
																				<aspdnsf:CSVHelper ID="CSVHelper1" runat="server" CSVTextBoxID="txtRuleCustomerLevels" UniqueJSID="txtRuleCustomerLevels" EntityType="CustomerLevel" CSVSearchButtonText="<%$Tokens:StringResource, admin.common.Search%>" />
																			</td>
																		</tr>
																	</table>
																</div>
															</td>
														</tr>
														<tr>
															<td class="emptyCell">&nbsp;</td>
															<td>
																<!-- State Rule -->
																<h3 class="checkHeader">
																	<aspdnsf:GeneralInfo ID="GeneralInfo10" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoShippingAddressState%>" />
																	<asp:CheckBox ID="chkRuleState" runat="server" CssClass="checkToggler" />
																	<asp:Label CssClass="checkLabel" ID="Literal22" Text="<%$Tokens:StringResource, admin.PromotionEditor.EnableShippingStateRequirementLiteral%>" runat="server" />
																</h3>
																<div class="checkTarget">
																	<table>
																		<tr>
																			<td class="tdLabel">
																				<span class="text-danger">*</span><asp:Label ID="Label28" Text="<%$Tokens:StringResource, admin.PromotionEditor.StatesLabel%>" AssociatedControlID="txtRuleStates" runat="server" />
																			</td>
																			<td>
																				<asp:TextBox ID="txtRuleStates" runat="server" Width="450" />
																			</td>
																		</tr>
																	</table>
																</div>
															</td>
														</tr>
														<tr>
															<td class="emptyCell">&nbsp;</td>
															<td>
																<!-- ZipCode Rule -->
																<h3 class="checkHeader">
																	<aspdnsf:GeneralInfo ID="GeneralInfo11" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoShippingAddressZip%>" />
																	<asp:CheckBox ID="chkRuleZipCode" runat="server" CssClass="checkToggler" />
																	<asp:Label CssClass="checkLabel" ID="Literal23" Text="<%$Tokens:StringResource, admin.PromotionEditor.EnableShippingZipCodeRequirementLiteral%>" runat="server" />
																</h3>
																<div class="checkTarget">
																	<table>
																		<tr>
																			<td class="tdLabel">
																				<span class="text-danger">*</span><asp:Label ID="Label29" Text="<%$Tokens:StringResource, admin.PromotionEditor.ZipCodesLabel%>" AssociatedControlID="txtRuleZipCodes" runat="server" />
																			</td>
																			<td>
																				<asp:TextBox ID="txtRuleZipCodes" runat="server" Width="450" />
																			</td>
																		</tr>
																	</table>
																</div>
															</td>
														</tr>
														<tr>
															<td class="emptyCell">&nbsp;</td>
															<td>
																<!-- Country Rule -->
																<h3 class="checkHeader">
																	<aspdnsf:GeneralInfo ID="GeneralInfo12" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoShippingAddressCountry%>" />
																	<asp:CheckBox ID="chkRuleCountryCodes" runat="server" CssClass="checkToggler" />
																	<asp:Label CssClass="checkLabel" ID="Literal24" Text="<%$Tokens:StringResource, admin.PromotionEditor.EnableShippingCountryRequirementLiteral%>" runat="server" />
																</h3>
																<div class="checkTarget">
																	<table>
																		<tr>
																			<td class="tdLabel">
																				<span class="text-danger">*</span><asp:Label ID="Label30" Text="<%$Tokens:StringResource, admin.PromotionEditor.CountryNamesLabel%>" AssociatedControlID="txtRuleCountryCodes" runat="server" />
																			</td>
																			<td>
																				<asp:TextBox ID="txtRuleCountryCodes" runat="server" Width="450" />
																			</td>
																		</tr>
																	</table>
																</div>
															</td>
														</tr>
													</table>
												</div>
											</div>
										</div>
										<div class="panel-heading">
											<h4 class="panel-title">
												<a data-toggle="collapse" data-parent="#accordion" href="#collapseFour">&nbsp;<asp:Label CssClass="checkLabel" ID="Label19" Text="<%$Tokens:StringResource, admin.PromotionEditor.LoyaltyHeaderLiteral%>" runat="server" />
												</a>
											</h4>
										</div>
										<div id="collapseFour" class="panel-collapse collapse">
											<div class="panel-body">
												<div class="checkTarget">
													<table width="100%;">
														<tr>
															<td class="emptyCell">&nbsp;</td>
															<td>
																<!-- Minimum Orders Rule -->
																<h3 class="checkHeader">
																	<aspdnsf:GeneralInfo ID="GeneralInfo13" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoMorePastOrders%>" />
																	<asp:CheckBox ID="chkRuleMinimumOrders" runat="server" CssClass="checkToggler" />
																	<asp:Label CssClass="checkLabel" ID="Literal25" Text="<%$Tokens:StringResource, admin.PromotionEditor.EnableMinimumNumberofPastOrdersLiteral%>" runat="server" />
																</h3>
																<div class="checkTarget">
																	<table>
																		<tr>
																			<td>
																				<span class="text-danger">*</span><asp:Label ID="Label31" Text="<%$Tokens:StringResource, admin.PromotionEditor.MinimumNumberofPastOrdersLabel%>" AssociatedControlID="txtRuleMinimumOrders" runat="server" />
																			</td>
																			<td>
																				<asp:TextBox ID="txtRuleMinimumOrders" runat="server" Width="50" />
																				<asp:RangeValidator runat="server" ControlToValidate="txtRuleMinimumOrders" ErrorMessage="<%$Tokens:StringResource, admin.PromotionEditor.ValidationIncorrectNumberFormat%>" Type="Integer" Display="Dynamic" MinimumValue="0" MaximumValue="99999999" />
																			</td>
																		</tr>
																		<tr>
																			<td class="tdLabel">
																				<span class="text-danger">*</span><asp:Label ID="Label32" Text="<%$Tokens:StringResource, admin.PromotionEditor.StartDateLabel%>" AssociatedControlID="txtRuleMinimumOrdersCustomStartDate" runat="server" />
																			</td>
																			<td>
																				<telerik:RadDatePicker ID="txtRuleMinimumOrdersCustomStartDate" runat="server" Style="z-index: 150000;" MaxDate="9999-12-31" MinDate="0001-01-01">
																					<Calendar runat="server" UseColumnHeadersAsSelectors="False" UseRowHeadersAsSelectors="False" ViewSelectorText="x" />
																				</telerik:RadDatePicker>
																			</td>
																		</tr>
																		<tr>
																			<td class="tdLabel">
																				<span class="text-danger">*</span><asp:Label ID="Label45" Text="<%$Tokens:StringResource, admin.PromotionEditor.EndDateLabel%>" AssociatedControlID="txtRuleMinimumOrdersCustomEndDate" runat="server" />
																			</td>
																			<td>
																				<telerik:RadDatePicker ID="txtRuleMinimumOrdersCustomEndDate" runat="server" Style="z-index: 150000;" MaxDate="9999-12-31" MinDate="0001-01-01">
																					<Calendar runat="server" UseColumnHeadersAsSelectors="False" UseRowHeadersAsSelectors="False" ViewSelectorText="x" />
																				</telerik:RadDatePicker>
																			</td>
																		</tr>
																	</table>
																</div>
															</td>
														</tr>
														<tr>
															<td class="emptyCell">&nbsp;</td>
															<td>
																<!-- Minimum Order Amount Rule -->
																<h3 class="checkHeader">
																	<aspdnsf:GeneralInfo ID="GeneralInfo14" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoPastOrdersSum%>" />
																	<asp:CheckBox ID="chkRuleMinimumOrderAmount" runat="server" CssClass="checkToggler" />
																	<asp:Label CssClass="checkLabel" ID="Literal26" Text="<%$Tokens:StringResource, admin.PromotionEditor.EnablePastOrdersMinimumAmountLiteral%>" runat="server" />
																</h3>
																<div class="checkTarget">
																	<table>
																		<tr>
																			<td class="tdLabel">
																				<span class="text-danger">*</span><asp:Label ID="Label46" Text="<%$Tokens:StringResource, admin.PromotionEditor.MinimumRequiredLabel%>" AssociatedControlID="txtRuleMinimumOrderAmount" runat="server" />
																			</td>
																			<td>
																				<asp:TextBox ID="txtRuleMinimumOrderAmount" runat="server" Width="100" />
																				<asp:RangeValidator runat="server" ControlToValidate="txtRuleMinimumOrderAmount" ErrorMessage="<%$Tokens:StringResource, admin.PromotionEditor.ValidationIncorrectNumberFormat%>" Type="Double" Display="Dynamic" MinimumValue="0" MaximumValue="999999999999999999" />
																			</td>
																		</tr>
																		<tr>
																			<td class="tdLabel">
																				<span class="text-danger">*</span><asp:Label ID="Label33" Text="<%$Tokens:StringResource, admin.PromotionEditor.StartDateLabel%>" AssociatedControlID="txtRuleMinimumOrderAmountCustomStartDate" runat="server" />
																			</td>
																			<td>
																				<telerik:RadDatePicker ID="txtRuleMinimumOrderAmountCustomStartDate" runat="server" Style="z-index: 150000;" MaxDate="9999-12-31">
																					<Calendar runat="server" UseColumnHeadersAsSelectors="False" UseRowHeadersAsSelectors="False" ViewSelectorText="x" />
																				</telerik:RadDatePicker>
																			</td>
																		</tr>
																		<tr>
																			<td class="tdLabel">
																				<span class="text-danger">*</span><asp:Label ID="Label47" Text="<%$Tokens:StringResource, admin.PromotionEditor.EndDateLabel%>" AssociatedControlID="txtRuleMinimumOrderAmountCustomEndDate" runat="server" />
																			</td>
																			<td>
																				<telerik:RadDatePicker ID="txtRuleMinimumOrderAmountCustomEndDate" runat="server" Style="z-index: 150000;" MaxDate="9999-12-31" MinDate="0001-01-01">
																					<Calendar runat="server" UseColumnHeadersAsSelectors="False" UseRowHeadersAsSelectors="False" ViewSelectorText="x" />
																				</telerik:RadDatePicker>
																			</td>
																		</tr>
																	</table>
																</div>
															</td>
														</tr>
														<tr>
															<td class="emptyCell">&nbsp;</td>
															<td>
																<!-- Minimum Products Ordered Rule -->
																<h3 class="checkHeader">
																	<aspdnsf:GeneralInfo ID="GeneralInfo15" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoPastOrdersProducts%>" />
																	<asp:CheckBox ID="chkRuleMinimumProductsOrdered" runat="server" CssClass="checkToggler" />
																	<asp:Label CssClass="checkLabel" ID="Literal27" Text="<%$Tokens:StringResource, admin.PromotionEditor.EnablePastOrdersMinimumNumberofProductsLiteral%>" runat="server" />
																</h3>
																<div class="checkTarget">
																	<table>
																		<tr>
																			<td class="tdLabel">
																				<span class="text-danger">*</span><asp:Label ID="Label53" Text="<%$Tokens:StringResource, admin.PromotionEditor.QuantityRequiredLabel%>" AssociatedControlID="txtRuleMinimumProductsOrdered" runat="server" />
																			</td>
																			<td>
																				<asp:TextBox ID="txtRuleMinimumProductsOrdered" runat="server" Width="50" />
																				<asp:RangeValidator runat="server" ControlToValidate="txtRuleMinimumProductsOrdered" ErrorMessage="<%$Tokens:StringResource, admin.PromotionEditor.ValidationIncorrectNumberFormat%>" Type="Integer" Display="Dynamic" MinimumValue="0" MaximumValue="999999999" />
																			</td>
																		</tr>
																		<tr>
																			<td class="tdLabel">
																				<span class="text-danger">*</span><asp:Label ID="Label34" Text="<%$Tokens:StringResource, admin.PromotionEditor.StartDateLabel%>" AssociatedControlID="txtRuleMinimumProductsOrderedCustomStartDate" runat="server" />
																			</td>
																			<td>
																				<telerik:RadDatePicker ID="txtRuleMinimumProductsOrderedCustomStartDate" runat="server" Style="z-index: 150000;" MaxDate="9999-12-31" MinDate="0001-01-01">
																					<Calendar runat="server" UseColumnHeadersAsSelectors="False" UseRowHeadersAsSelectors="False" ViewSelectorText="x" />
																				</telerik:RadDatePicker>
																			</td>
																		</tr>
																		<tr>
																			<td class="tdLabel">
																				<span class="text-danger">*</span><asp:Label ID="Label48" Text="<%$Tokens:StringResource, admin.PromotionEditor.EndDateLabel%>" AssociatedControlID="txtRuleMinimumProductsOrderedCustomEndDate" runat="server" />
																			</td>
																			<td>
																				<telerik:RadDatePicker ID="txtRuleMinimumProductsOrderedCustomEndDate" runat="server" Style="z-index: 150000;" MaxDate="9999-12-31" MinDate="0001-01-01">
																					<Calendar runat="server" UseColumnHeadersAsSelectors="False" UseRowHeadersAsSelectors="False" ViewSelectorText="x" />
																				</telerik:RadDatePicker>
																			</td>
																		</tr>
																		<tr>
																			<td class="tdLabel" valign="top">
																				<span class="text-danger">*</span><asp:Label ID="Label51" Text="<%$Tokens:StringResource, admin.PromotionEditor.ProductIdsLabel%>" AssociatedControlID="txtRuleMinimumProductsOrderedProductIds" runat="server" />
																			</td>
																			<td>
																				<asp:TextBox ID="txtRuleMinimumProductsOrderedProductIds" runat="server" Width="450" Style="display: none;" />
																				<aspdnsf:CSVHelper runat="server" CSVTextBoxID="txtRuleMinimumProductsOrderedProductIds" UniqueJSID="txtRuleMinimumProductsOrderedProductIds" CSVSearchButtonText="<%$Tokens:StringResource, admin.common.Search%>" />
																			</td>
																		</tr>
																	</table>
																</div>
															</td>
														</tr>
														<tr>
															<td class="emptyCell">&nbsp;</td>
															<td>
																<!-- Minimum Product Amount Ordered Rule -->
																<h3 class="checkHeader">
																	<aspdnsf:GeneralInfo ID="GeneralInfo16" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoPastOrdersPRoductSum%>" />
																	<asp:CheckBox ID="chkRuleMinimumProductsOrderedAmount" runat="server" CssClass="checkToggler" />
																	<asp:Label CssClass="checkLabel" ID="Literal31" Text="<%$Tokens:StringResource, admin.PromotionEditor.EnablePastOrdersValueofProductsLiteral%>" runat="server" />
																</h3>
																<div class="checkTarget">
																	<table>
																		<tr>
																			<td class="tdLabel">
																				<span class="text-danger">*</span><asp:Label ID="Label52" Text="<%$Tokens:StringResource, admin.PromotionEditor.AmountRequiredLabel%>" AssociatedControlID="txtRuleMinimumProductsOrderedAmount" runat="server" />
																			</td>
																			<td>$<asp:TextBox ID="txtRuleMinimumProductsOrderedAmount" runat="server" Width="100" />
																				<asp:RangeValidator runat="server" ControlToValidate="txtRuleMinimumProductsOrderedAmount" ErrorMessage="<%$Tokens:StringResource, admin.PromotionEditor.ValidationIncorrectNumberFormat%>" Type="Double" Display="Dynamic" MinimumValue="0" MaximumValue="999999999999999999" />
																			</td>
																		</tr>
																		<tr>
																			<td class="tdLabel">
																				<span class="text-danger">*</span><asp:Label ID="Label35" Text="<%$Tokens:StringResource, admin.PromotionEditor.StartDateLabel%>" AssociatedControlID="txtRuleMinimumProductsOrderedAmountCustomStartDate" runat="server" />
																			</td>
																			<td>
																				<telerik:RadDatePicker ID="txtRuleMinimumProductsOrderedAmountCustomStartDate" runat="server" Style="z-index: 150000;" MaxDate="9999-12-31" MinDate="0001-01-01">
																					<Calendar runat="server" UseColumnHeadersAsSelectors="False" UseRowHeadersAsSelectors="False" ViewSelectorText="x" />
																				</telerik:RadDatePicker>
																			</td>
																		</tr>
																		<tr>
																			<td class="tdLabel">
																				<span class="text-danger">*</span><asp:Label ID="Label49" Text="<%$Tokens:StringResource, admin.PromotionEditor.EndDateLabel%>" AssociatedControlID="txtRuleMinimumProductsOrderedAmountCustomEndDate" runat="server" />
																			</td>
																			<td>
																				<telerik:RadDatePicker ID="txtRuleMinimumProductsOrderedAmountCustomEndDate" runat="server" Style="z-index: 150000;" MaxDate="9999-12-31" MinDate="0001-01-01">
																					<Calendar runat="server" UseColumnHeadersAsSelectors="False" UseRowHeadersAsSelectors="False" ViewSelectorText="x" />
																				</telerik:RadDatePicker>
																			</td>
																		</tr>
																		<tr>
																			<td class="tdLabel" valign="top">
																				<span class="text-danger">*</span><asp:Label ID="Label55" Text="<%$Tokens:StringResource, admin.PromotionEditor.ProductIdsLabel%>" AssociatedControlID="txtRuleMinimumProductsOrderedAmountProductIds" runat="server" />
																			</td>
																			<td>
																				<asp:TextBox ID="txtRuleMinimumProductsOrderedAmountProductIds" runat="server" Width="450" Style="display: none;" />
																				<aspdnsf:CSVHelper runat="server" CSVTextBoxID="txtRuleMinimumProductsOrderedAmountProductIds" UniqueJSID="txtRuleMinimumProductsOrderedAmountProductIds" CSVSearchButtonText="<%$Tokens:StringResource, admin.common.Search%>" />
																			</td>
																		</tr>
																	</table>
																</div>
															</td>
														</tr>																											
													</table>
												</div>												
											</div>											
										</div>										
									</div>
									<div class="tdButtonRow promotion-bottom-expand-button">
										<input runat="server" type="button" value="<%$Tokens:StringResource, admin.PromotionEditor.ExpandAllButton%>" class="btn btn-default btn-sm expandAllExpandables" />
										<input runat="server" type="button" value="<%$Tokens:StringResource, admin.PromotionEditor.CollapseAllButton%>" class="btn btn-default btn-sm collapseAllExpandables" />
									</div>
								</div>								
							</asp:Panel>			
						</td>
					</tr>
				</table>
			</fieldset>			
			<div class="item-action-bar">
				<asp:HyperLink runat="server" ID="btnClose" CssClass="btn btn-default" NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>" Text="<%$Tokens:StringResource, admin.common.close %>" />
				<asp:Button ID="btnSaveAndClose" runat="server" CssClass="btn btn-default" OnClick="btnSaveAndClose_Click" ValidationGroup="Main" Text="<%$Tokens:StringResource, admin.common.SaveAndClose %>" />
				<asp:Button ID="btnSubmit" runat="server" CssClass="btn btn-primary" OnClick="btnSubmit_Click" ValidationGroup="Main" Text="<%$Tokens:StringResource, admin.common.Save %>" />
			</div>
		</asp:Panel>
	</div>
</asp:Content>