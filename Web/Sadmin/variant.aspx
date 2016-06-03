<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.Variant" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="variant.aspx.cs" %>

<%@ Register TagPrefix="AjaxToolkit" Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" %>
<%@ Register TagPrefix="telerik" Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" %>

<asp:Content ID="Content1" ContentPlaceHolderID="bodyContentPlaceholder" runat="server">
	<aspdnsf:ReturnUrlTracker runat="server" ID="ReturnUrlTracker" DefaultReturnUrl="products.aspx" Blacklist="variantsizecolorinventory.aspx" />
	<script type="text/javascript">
		function Dimension_CustomValidator(source, args) {
			args.IsValid = true;
			if (($('#<% =txtWidth.ClientID %>').val().length > 0) | ($('#<% =txtHeight.ClientID %>').val().length > 0) | ($('#<% =txtDepth.ClientID %>').val().length > 0)) {
				if (($('#<% =txtWidth.ClientID %>').val().length == 0) | ($('#<% =txtHeight.ClientID %>').val().length == 0) | ($('#<% =txtDepth.ClientID %>').val().length == 0)) {
					args.IsValid = false;
				}
			}
		}
	</script>
	<div class="wrapper row" id="divwrapper" runat="server">
		<asp:Panel ID="pnlContent" runat="server" DefaultButton="btnSubmit">
			<div class="main-breadcrumb">
				<asp:Literal ID="ltProduct" runat="server" />
				&gt;&#32;
					<asp:HyperLink ID="lnkManageVariants" runat="server" Text="Manage Variants" />
				&gt;&#32;
					<asp:Literal ID="ltStatus" runat="server" />
			</div>
			<h1>
				<i class="fa fa-cubes f-x3"></i>
				<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.Variant %>" />
			</h1>
			<div>
				<aspdnsf:AlertMessage runat="server" ID="ctrlAlertMessage" />
			</div>
			<asp:Panel ID="pnlUpdateMappedKitItem" runat="server" Visible="false">
				<div class="alert alert-warning">This variant is mapped to one or more kit items.</div>
			</asp:Panel>
			<div class="item-action-bar">
				<asp:Panel ID="pnlLocale" CssClass="other-actions" runat="server">
					<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.stringresources.Locale %>" AssociatedControlID="LocaleSelector" />
					<aspdnsf:LocaleSelector ID="LocaleSelector" runat="server" OnSelectedLocaleChanged="LocaleSelector_SelectedLocaleChanged" />
				</asp:Panel>
				<asp:Button ID="btnDeleteVariant" runat="server" Text="Delete this Variant" OnClick="btnDeleteVariant_Click" CssClass="btn btn-danger" />
				<asp:Label ID="lblNoDeleteTop" runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$Tokens:StringResource, admin.common.DeleteNAInfoRecurring %>" Visible="false">
					<i class="fa fa-question-circle fa-lg"></i>
				</asp:Label>
				<asp:HyperLink runat="server" ID="btnCloseTop" CssClass="btn btn-default" NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>" Text="<%$Tokens:StringResource, admin.common.close %>" />
				<asp:Button ID="btnSaveAndCloseTop" runat="server" CssClass="btn btn-default" OnClick="btnSaveAndClose_Click" ValidationGroup="Main" Text="<%$Tokens:StringResource, admin.common.saveandclose %>" NavigateUrl="products.aspx" />
				<asp:Button ID="btnSubmit" runat="server" CssClass="btn btn-primary" OnClick="btnSubmit_Click" ValidationGroup="Main" />
			</div>
			<div class="tabcontainer" style="clear: left;">
				<asp:HiddenField ID="hdnTabId" Value="0" runat="server" EnableViewState="true" />
				<!-- Nav tabs -->
				<ul class="nav nav-tabs" role="tablist">
					<li class="active"><a href="#main-tab" role="tab">
						<asp:Literal runat="server" ID="ltTabMain" Text='<%$ Tokens:StringResource, admin.tabs.Main %>' /></a></li>
					<li><a href="#images-tab" role="tab">
						<asp:Literal runat="server" ID="ltTabImages" Text='<%$ Tokens:StringResource, admin.tabs.Images %>' /></a></li>
					<li><a href="#description-tab" role="tab">
						<asp:Literal runat="server" ID="ltTabDescription" Text='<%$ Tokens:StringResource, admin.tabs.Description %>' /></a></li>
					<li><a href="#productfeeds-tab" role="tab">
						<asp:Literal runat="server" ID="ltTabProductFeeds" Text='<%$ Tokens:StringResource, admin.tabs.ProductFeeds %>' /></a></li>
					<li><a href="#attributes-tab" role="tab">
						<asp:Literal runat="server" ID="ltTabAttributes" Text='<%$ Tokens:StringResource, admin.tabs.Attributes %>' /></a></li>
					<li runat="server" id="liRecurringTab"><a href="#recurring-tab" role="tab">
						<asp:Literal runat="server" ID="ltTabRecurring" Text='<%$ Tokens:StringResource, admin.tabs.Recurring %>' /></a></li>
				</ul>
				<!-- Tab panes -->
				<div class="tab-content white-ui-box">
					<div class="tab-pane active" id="main-tab">
						<div class="admin-row">
							<table class="table table-detail">
								<tr>
									<td>Variant Name:
									</td>
									<td>
										<asp:TextBox CssClass="singleNormal" ID="txtName" runat="Server" />
									</td>
								</tr>
								<tr>
									<td>SKU Suffix:
									</td>
									<td>
										<asp:TextBox MaxLength="50" ID="txtSKU" runat="server" CssClass="singleShorter" />
									</td>
								</tr>
								<tr>
									<td>Manufacturer Part #:
									</td>
									<td>
										<asp:TextBox MaxLength="50" ID="txtManufacturePartNumber" runat="server" CssClass="singleShorter" />
									</td>
								</tr>
								<tr>
									<td>GTIN:
									</td>
									<td>
										<asp:TextBox MaxLength="14" ID="txtGTIN" runat="server" Columns="14" />
									</td>
								</tr>
								<tr>
									<td>
										<span class="text-danger">*</span>Published:
									</td>
									<td>
										<asp:RadioButtonList ID="rblPublished" runat="server" RepeatColumns="2" RepeatDirection="horizontal">
											<asp:ListItem Value="0" Text="No" />
											<asp:ListItem Value="1" Text="Yes" Selected="true" />
										</asp:RadioButtonList>
									</td>
								</tr>
								<tr>
									<td>Restricted Quantities:
									</td>
									<td>
										<asp:TextBox MaxLength="250" ID="txtRestrictedQuantities" runat="server" CssClass="singleNormal" />
										<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.tooltip.restrictedqty %>">
												<i class="fa fa-question-circle fa-lg"></i>
										</asp:Label>
									</td>
								</tr>
								<tr>
									<td>Minimum Quantity:
									</td>
									<td>
										<asp:TextBox MaxLength="10" ID="txtMinimumQuantity" runat="server" CssClass="singleShortest" />
										<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.tooltip.minimumqty %>">
											<i class="fa fa-question-circle fa-lg"></i>
										</asp:Label>
									</td>
								</tr>
								<tr>
									<td>
										<span class="text-danger">*</span>Price:
									</td>
									<td>
										<asp:TextBox MaxLength="10" CssClass="singleShortest" ID="txtPrice" runat="Server" />
										<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.tooltip.Price %>">
											<i class="fa fa-question-circle fa-lg"></i>
										</asp:Label>																			
										<asp:Literal ID="ltExtendedPricing" runat="server" />
										<asp:RequiredFieldValidator ControlToValidate="txtPrice" ErrorMessage="<%$ Tokens:StringResource, admin.variant.PriceIsRequired %>"
											ID="rfvName" ValidationGroup="Main" CssClass="text-danger" EnableClientScript="true" SetFocusOnError="true"
											runat="server" Display="Dynamic" />	
									</td>
								</tr>
								<tr>
									<td>Sale Price:
									</td>
									<td>
										<asp:TextBox MaxLength="10" CssClass="singleShortest" ID="txtSalePrice" runat="Server" />
										<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.tooltip.SalePrice %>">
											<i class="fa fa-question-circle fa-lg"></i>
										</asp:Label>
									</td>
								</tr>
								<tr id="trCustomerEntersPrice1" runat="server">
									<td>
										<span class="text-danger">*</span>Customer Enters Price:
									</td>
									<td>
										<asp:RadioButtonList ID="rblCustomerEntersPrice" CssClass="radio-button-align" runat="server" RepeatColumns="2" RepeatDirection="horizontal">
											<asp:ListItem Value="0" Text="No" />
											<asp:ListItem Value="1" Text="Yes" Selected="true" />
										</asp:RadioButtonList>
										<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.tooltip.CustomerEntersPrice %>">
											<i class="fa fa-question-circle fa-lg"></i>
										</asp:Label>
									</td>
								</tr>
								<tr id="trCustomerEntersPricePrompt" runat="server">
									<td>Customer Enters Price Prompt:
									</td>
									<td>
										<asp:TextBox ID="txtCustomerEntersPricePrompt" runat="server" CssClass="singleNormal" />
										<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.tooltip.CustomerEntersPricePrompt %>">
											<i class="fa fa-question-circle fa-lg"></i>
										</asp:Label>
									</td>
								</tr>
								<tr>
									<td>MSRP:
									</td>
									<td>
										<asp:TextBox MaxLength="10" CssClass="singleShortest" ID="txtMSRP" runat="Server" />
										<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.tooltip.msrp %>">
											<i class="fa fa-question-circle fa-lg"></i>
										</asp:Label>
									</td>
								</tr>
								<tr>
									<td>Actual Cost:
									</td>
									<td>
										<asp:TextBox MaxLength="10" CssClass="singleShortest" ID="txtActualCost" runat="Server" />
										<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.tooltip.actualCost %>">
											<i class="fa fa-question-circle fa-lg"></i>
										</asp:Label>
									</td>
								</tr>
								<tr>
									<td>
										<span class="text-danger">*</span>Is Taxable:
									</td>
									<td>
										<asp:RadioButtonList ID="rblTaxable" CssClass="radio-button-align" runat="server" RepeatColumns="2" RepeatDirection="horizontal">
											<asp:ListItem Value="0" Text="No" />
											<asp:ListItem Value="1" Text="Yes" Selected="true" />
										</asp:RadioButtonList>
										<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.tooltip.IsTaxable %>">
											<i class="fa fa-question-circle fa-lg"></i>
										</asp:Label>
									</td>
								</tr>
								<tr>
									<td>
										<span class="text-danger">*</span>Shipping:
									</td>
									<td>
										<asp:RadioButtonList ID="rblFreeShipping" runat="server" ClientIDMode="Static">
											<asp:ListItem Value="0" Text="Has a cost" Selected="true" />
											<asp:ListItem Value="1" Text="Is free" />
											<asp:ListItem Value="2" Text="Is not required" />
										</asp:RadioButtonList>
										<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.tooltip.FreeShipping %>">
											<i class="fa fa-question-circle fa-lg"></i>
										</asp:Label>
									</td>
								</tr>
								<tr id="trShippingCost" runat="server" class="shipping-fields" style="display: none;">
									<td>Shipping Cost:
									</td>
									<td>
										<asp:Literal ID="ltShippingCost" runat="server" />
										<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.tooltip.ShippingCost %>">
											<i class="fa fa-question-circle fa-lg"></i>
										</asp:Label>
									</td>
								</tr>
								<tr id="trShipSeparately" runat="server" class="shipping-fields">
									<td>
										<span class="text-danger">*</span>Is Ship Separately:
									</td>
									<td>
										<asp:RadioButtonList ID="rblShipSeparately" CssClass="radio-button-align" runat="server" RepeatColumns="2" RepeatDirection="horizontal">
											<asp:ListItem Value="0" Text="No" Selected="true" />
											<asp:ListItem Value="1" Text="Yes" />
										</asp:RadioButtonList>
										<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.tooltip.ShipSeparately %>">
											<i class="fa fa-question-circle fa-lg"></i>
										</asp:Label>
									</td>
								</tr>
								<tr id="trISDownload" runat="server">
									<td>
										<span class="text-danger">*</span>Is Download:
									</td>
									<td>
										<asp:RadioButtonList ID="rblDownload" CssClass="radio-button-align" runat="server" RepeatColumns="2" RepeatDirection="horizontal">
											<asp:ListItem Value="0" Text="No" Selected="true" />
											<asp:ListItem Value="1" Text="Yes" />
										</asp:RadioButtonList>
										<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.tooltip.Download %>">
											<i class="fa fa-question-circle fa-lg"></i>
										</asp:Label>
									</td>
								</tr>
								<tr>
									<td>Download is Valid For Number Of Days:
									</td>
									<td>
										<asp:TextBox MaxLength="5" ID="txtValidForDays" runat="server" CssClass="singleShortest" />
									</td>
								</tr>
								<tr id="trDownloadLoc" runat="server">
									<td>Download Location:
									</td>
									<td>
										<asp:TextBox MaxLength="250" ID="txtDownloadLocation" runat="server" CssClass="singleLonger" />
										<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.tooltip.DownloadLocation %>">
											<i class="fa fa-question-circle fa-lg"></i>
										</asp:Label>
									</td>
								</tr>
								<tr id="trCondition" runat="server">
									<td>
										<span class="text-danger">*</span>Condition:
									</td>
									<td>
										<asp:RadioButtonList ID="rblCondition" CssClass="radio-button-align" runat="server" RepeatColumns="3" RepeatDirection="horizontal">
											<asp:ListItem Value="0" Text="New" Selected="true" />
											<asp:ListItem Value="1" Text="Used" />
											<asp:ListItem Value="2" Text="Refurbished" />
										</asp:RadioButtonList>
										<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.tooltip.Condition %>">
											<i class="fa fa-question-circle fa-lg"></i>
										</asp:Label>
									</td>
								</tr>
								<tr>
									<td>Weight:
									</td>
									<td>
										<asp:TextBox MaxLength="15" CssClass="singleShortest" ID="txtWeight" runat="Server" />
										<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.tooltip.Weight %>">
											<i class="fa fa-question-circle fa-lg"></i>
										</asp:Label>
									</td>
								</tr>
								<tr>
									<td>
										<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.DepthInfo %>" />
									</td>
									<td>
										<div>
											<asp:TextBox MaxLength="15" ID="txtWidth" runat="server" CssClass="text-sm" />
											<i class="fa fa-times"></i>
											<asp:TextBox MaxLength="15" ID="txtHeight" runat="server" CssClass="text-sm" />
											<i class="fa fa-times"></i>
											<asp:TextBox MaxLength="15" ID="txtDepth" runat="server" CssClass="text-sm" />
											<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.tooltip.Dimensions %>">
												<i class="fa fa-question-circle fa-lg"></i>
											</asp:Label>
										</div>
										<asp:RegularExpressionValidator ID="revWidth" ControlToValidate="txtWidth"
											Display="Dynamic" runat="server" CssClass="text-danger" ValidationGroup="Main" ValidationExpression="\d*\.?\d*"
											ErrorMessage="<%$ Tokens:StringResource, admin.common.WidthError %>" />
										<asp:RegularExpressionValidator ID="revHeight" ControlToValidate="txtHeight"
											Display="Dynamic" runat="server" CssClass="text-danger" ValidationGroup="Main" ValidationExpression="\d*\.?\d*"
											ErrorMessage="<%$ Tokens:StringResource, admin.common.HeightError %>" />
										<asp:RegularExpressionValidator ID="revDepth" ControlToValidate="txtDepth"
											Display="Dynamic" runat="server" CssClass="text-danger" ValidationGroup="Main" ValidationExpression="\d*\.?\d*"
											ErrorMessage="<%$ Tokens:StringResource, admin.common.DepthError %>" />
										<asp:CustomValidator runat="server" ID="cvrDimensions" ValidationGroup="Main" CssClass="text-danger" Display="Dynamic" ClientValidationFunction="Dimension_CustomValidator" ErrorMessage="<%$ Tokens:StringResource, admin.editproductvariant.DimensionsAllOrNone %>" />
									</td>
								</tr>
								<tr id="trCurrentInventory" runat="server">
									<td>Current Inventory:
									</td>
									<td>
										<asp:TextBox MaxLength="15" CssClass="singleShortest" ID="txtCurrentInventory" runat="Server" />
										<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.tooltip.CurrentInventory %>">
											<i class="fa fa-question-circle fa-lg"></i>
										</asp:Label>
									</td>
								</tr>
								<tr id="trManageInventory" runat="server">
									<td>Manage Inventory:
									</td>
									<td>
										<asp:Literal ID="ltManageInventory" runat="server" />
									</td>
								</tr>
							</table>
						</div>
					</div>
					<div class="tab-pane" id="images-tab">
						<div class="admin-row">
							<table class="table table-detail">
								<tr>
									<td>Image Filename Override:
									</td>
									<td>
										<asp:TextBox CssClass="singleNormal" ID="txtImageOverride" runat="Server" />
										<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.tooltip.ImageFilenameOverride %>">
											<i class="fa fa-question-circle fa-lg"></i>
										</asp:Label>
									</td>
								</tr>
								<tr>
									<td>Alt Text:
									</td>
									<td>
										<asp:TextBox CssClass="text-md" ID="txtSEAlt" runat="Server" />
									</td>
								</tr>
								<tr>
									<td>Icon:
									</td>
									<td>
										<asp:FileUpload CssClass="fileUpload" ID="fuIcon" runat="Server" />
										<div>
											<asp:Literal ID="ltIcon" runat="server" />
										</div>
									</td>
								</tr>
								<tr>
									<td>Medium:
									</td>
									<td>
										<asp:FileUpload CssClass="fileUpload" ID="fuMedium" runat="Server" />
										<div>
											<asp:Literal ID="ltMedium" runat="server" />
										</div>
									</td>
								</tr>
								<tr>
									<td>Large:
									</td>
									<td>
										<asp:FileUpload CssClass="fileUpload" ID="fuLarge" runat="Server" />
										<div>
											<asp:Literal ID="ltLarge" runat="server" />
										</div>
									</td>
								</tr>
							</table>
						</div>
					</div>
					<div class="tab-pane" id="description-tab">
						<div class="admin-row">
							<table class="table table-detail">
								<tr>
									<td>
										<div class="form-group">
											<asp:TextBox ID="txtDescriptionNoHtmlEditor" Rows="15" TextMode="MultiLine" runat="server" Visible="false" CssClass="form-control" />
										</div>
										<telerik:radeditor runat="server" id="radDescription" SkinID="RadEditorSettings" />
									</td>
								</tr>
							</table>
						</div>
					</div>
					<div class="tab-pane" id="productfeeds-tab">
						<div class="admin-row">
							<table class="table table-detail">
								<tr>
									<td>
										<asp:Literal ID="litProductFeed" runat="server" Text="<%$Tokens:StringResource, admin.product.ProductFeedDescription %>" />:
									</td>
								</tr>
								<tr>
									<td>
										<div class="form-group">
											<asp:TextBox CssClass="form-control multiExtension" ID="txtFroogle" runat="Server" TextMode="multiLine" />
										</div>
									</td>
								</tr>
							</table>
						</div>
					</div>
					<div class="tab-pane" id="attributes-tab">
						<div class="admin-row">
							<table class="table table-detail">
								<tr>
									<td style="width: 25%">
										<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.editproductvariant.Colors %>" />:
									</td>
									<td style="width: 75%">
										<asp:TextBox ID="txtColors" runat="server" CssClass="text-lg" />
										<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.tooltip.Colors %>">
											<i class="fa fa-question-circle fa-lg"></i>
										</asp:Label>
									</td>
								</tr>
								<tr>
									<td>
										<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.editproductvariant.ColorSkuModifiers %>" />:
									</td>
									<td>
										<asp:TextBox ID="txtColorSKUModifiers" runat="server" CssClass="text-lg" />
										<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.tooltip.ColorSkuMods %>">
											<i class="fa fa-question-circle fa-lg"></i>
										</asp:Label>
									</td>
								</tr>
								<tr>
									<td>
										<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.editproductvariant.Sizes %>" />:
									</td>
									<td>
										<asp:TextBox ID="txtSizes" runat="server" CssClass="text-lg" />
										<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.tooltip.sizes %>">
											<i class="fa fa-question-circle fa-lg"></i>
										</asp:Label>
									</td>
								</tr>
								<tr>
									<td>
										<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.editproductvariant.SizeSkuModifiers %>" />:
									</td>
									<td>
										<asp:TextBox ID="txtSizeSKUModifiers" runat="server" CssClass="text-lg" />
										<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.tooltip.sizeskumods %>">
											<i class="fa fa-question-circle fa-lg"></i>
										</asp:Label>
									</td>
								</tr>
							</table>
						</div>
						<div class="form-group" id="trColorSizeSummary" runat="server">
							<div class="admin-row">
								<div class="col-sm-3">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.editproductvariant.ColorSizeSummary %>" />:
								</div>
								<div class="col-sm-9" runat="server" id="colorSummarySection">
									<asp:Repeater ID="rptColors" runat="server">
										<HeaderTemplate>
											<table class="table">
												<tr>
													<th>
														<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.editproductvariant.Color %>" /></th>
													<th>
														<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.editproductvariant.ColorSkuModifier %>" /></th>
												</tr>
										</HeaderTemplate>
										<ItemTemplate>
											<tr>
												<td style="width: 50%;"><%# Eval("Item1")%></td>
												<td style="width: 50%;"><%# Eval("Item2")%></td>
											</tr>
										</ItemTemplate>
										<FooterTemplate>
											</table>
										</FooterTemplate>
									</asp:Repeater>
								</div>
							</div>
							<div class="admin-row">
								<div class="col-sm-3" runat="server" id="sizeSummarySection">
								</div>
								<div class="col-sm-9">
									<asp:Repeater ID="rptSizes" runat="server">
										<HeaderTemplate>
											<table class="table">
												<tr>
													<th>
														<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.editproductvariant.Size %>" /></th>
													<th>
														<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.editproductvariant.SizeSkuModifier %>" /></th>
												</tr>
										</HeaderTemplate>
										<ItemTemplate>
											<tr>
												<td style="width: 50%;"><%# Eval("Item1")%></td>
												<td style="width: 50%;"><%# Eval("Item2")%></td>
											</tr>
										</ItemTemplate>
										<FooterTemplate>
											</table>
										</FooterTemplate>
									</asp:Repeater>
								</div>
							</div>
						</div>
						<div style="clear: both;"></div>
					</div>
					<div class="tab-pane" id="recurring-tab">
						<div class="admin-row">
							<asp:HyperLink Text="Get help with recurring products" NavigateUrl="http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=10000manual&type=recurringvariant" Target="_blank" runat="server" ID="HyperLink1" />
						</div>
						<div class="admin-row" runat="server" id="trRecurring">
							<div class="col-sm-3">
								<span class="text-danger">*</span>Is Recurring:
							</div>
							<div class="col-sm-9">
								<asp:RadioButtonList ID="rblRecurring" runat="server" RepeatColumns="2" RepeatDirection="horizontal">
									<asp:ListItem Value="0" Text="No" Selected="true" />
									<asp:ListItem Value="1" Text="Yes" />
								</asp:RadioButtonList>
							</div>
						</div>
						<div class="admin-row" runat="server" id="trRecurringInterval">
							<div class="col-sm-3">
								Recurring Interval:
							</div>
							<div class="col-sm-9">
								<asp:TextBox MaxLength="50" ID="txtRecurringIntervalMultiplier" runat="server" CssClass="text-xs" />
							</div>
						</div>
						<div class="admin-row" runat="server" id="trRecurringType">
							<div class="col-sm-3">
								<span class="text-danger">*</span>Interval Type:
							</div>
							<div class="col-sm-9">
								<asp:DropDownList ID="ddRecurringIntervalType" CssClass="text-xs" runat="server" />
							</div>
						</div>
					</div>
					<asp:ValidationSummary ID="validationSummary" runat="Server" ValidationGroup="Main" CssClass="text-danger" DisplayMode="BulletList" ShowMessageBox="true" ShowSummary="false" />
					<div style="clear: both;"></div>
				</div>
				<div class="item-action-bar">
					<asp:Button ID="btnDeleteVariantBottom" runat="server" Text="Delete this Variant" OnClick="btnDeleteVariant_Click" CssClass="btn btn-danger" />
					<asp:Label ID="lblNoDeleteBottom" runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$Tokens:StringResource, admin.common.DeleteNAInfoRecurring %>" Visible="false">
						<i class="fa fa-question-circle fa-lg"></i>
					</asp:Label>
					<asp:HyperLink runat="server" ID="btnClose" CssClass="btn btn-default" NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>" Text="<%$Tokens:StringResource, admin.common.close %>" />
					<asp:Button ID="btnSaveAndClose" runat="server" CssClass="btn btn-default" OnClick="btnSaveAndClose_Click" ValidationGroup="Main" Text="<%$Tokens:StringResource, admin.common.saveandclose %>" NavigateUrl="products.aspx" />
					<asp:Button ID="btnSubmitBottom" runat="server" CssClass="btn btn-primary" OnClick="btnSubmit_Click" ValidationGroup="Main" />
				</div>
			</div>
		</asp:Panel>
	</div>
	<script>
		function DeleteImage(imgurl, name) {
			if (confirm('Are you sure you want to delete this image?')) {
				window.open('deleteimage.aspx?imgurl=' + imgurl + '&FormImageName=' + name, "Admin_ML", "height=250,width=440,top=10,left=20,status=no,toolbar=no,menubar=no,scrollbars=yes,location=no");
			}
		}

		//payflow days recurring
		$(document).ready(function () {
			var dayRangeIntervalValue = <%= (int)AspDotNetStorefrontCore.DateIntervalTypeEnum.NumberOfDays %>;
			var $trRecurringInterval = $("div[id$='trRecurringInterval']");
			var $ddRecurringIntervalType = $('select[id$=ddRecurringIntervalType]');

			$trRecurringInterval.hide();

			//show if set to days enum
			if($ddRecurringIntervalType.val() == dayRangeIntervalValue) {
				$trRecurringInterval.show();
			}
			$ddRecurringIntervalType.on('change', function () {
				if($(this).val() == dayRangeIntervalValue) {
					$trRecurringInterval.show();
				}
				else {
					//reset value to supported value of 1 if not a days interval
					//double-checked in variant.aspx.cs
					$("input[id$='txtRecurringIntervalMultiplier']").val("1");
					$trRecurringInterval.hide();
				}
			});
		});

	</script>
</asp:Content>