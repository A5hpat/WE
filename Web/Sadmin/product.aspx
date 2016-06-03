<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.ProductEditor" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="product.aspx.cs" %>

<%@ Register TagPrefix="AjaxToolkit" Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" %>
<%@ Register TagPrefix="telerik" Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" %>
<%@ Register TagPrefix="aspdnsf" TagName="EntityToStore" Src="controls/EntityToStoreMapper.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="CSVHelper" Src="controls/CSVHelper.ascx" %>
<%@ Register TagPrefix="QuickAdd" TagName="QuickAddEntity" Src="Controls/QuickAdd/QuickAddEntity.ascx" %>
<%@ Register TagPrefix="QuickAdd" TagName="QuickAddQuantityDiscounts" Src="Controls/QuickAdd/QuickAddQuantityDiscounts.ascx" %>
<%@ Register TagPrefix="QuickAdd" TagName="QuickAddCustomerLevel" Src="Controls/QuickAdd/QuickAddCustomerLevel.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="bodyContentPlaceholder" runat="server">
	<aspdnsf:ReturnUrlTracker runat="server" ID="ReturnUrlTracker" DefaultReturnUrl="products.aspx" Blacklist="kit.aspx,variants.aspx,variantsizecolorinventory.aspx" />
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
		<asp:Panel ID="pnlContent" runat="server">
			<div class="main-breadcrumb">
				<asp:Literal ID="ltStatus" runat="server" />
			</div>
			<h1>
				<i class="fa fa-cube f-x3"></i>
				<asp:Literal runat="server" ID="HeaderText" />
			</h1>
			<div>
				<aspdnsf:AlertMessage runat="server" ID="ctrlAlertMessage" />
			</div>
			<div class="item-action-bar clearfix">
				<div class="other-actions">
					<asp:Panel ID="Panel1" runat="server" Visible='<%# LocaleSelector.HasMultipleLocales() %>'>
						<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.stringresources.Locale %>" AssociatedControlID="LocaleSelector" />
						<aspdnsf:LocaleSelector ID="LocaleSelector" runat="server" OnSelectedLocaleChanged="LocaleSelector_SelectedLocaleChanged" />
					</asp:Panel>
				</div>

				<asp:HyperLink runat="server"
					CssClass="btn btn-default"
					Visible='<%# Editing %>'
					NavigateUrl='<%# String.Format("variants.aspx?productid={0}", ProductId) %>'
					Text="Manage Variants" />
				
				<asp:HyperLink runat="server"
					CssClass="btn btn-default"
					NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>"
					Text="<%$Tokens:StringResource, admin.common.close %>" />

				<asp:Button runat="server"
					CssClass="btn btn-default"
					ValidationGroup="Main"
					OnClick="btnSaveAndClose_Click"
					Text="<%$Tokens:StringResource, admin.common.saveandclose %>" />

				<asp:Button runat="server"
					CssClass="btn btn-primary"
					ValidationGroup="Main"
					OnClick="btnSubmit_Click"
					Text="<%$Tokens:StringResource, admin.common.save %>" />
			</div>
			<div class="tabcontainer" style="clear: left;">
				<asp:HiddenField ID="hdnTabId" Value="0" runat="server" EnableViewState="true" />
				<!-- Nav tabs -->
				<ul class="nav nav-tabs" role="tablist">
					<li class="active"><a href="#main-tab" role="tab">
						<asp:Literal runat="server" ID="ltTabMain" Text='<%$ Tokens:StringResource, admin.tabs.Main %>' /></a></li>
					<li><a href="#images-tab" role="tab">
						<asp:Literal runat="server" ID="ltTabImages" Text='<%$ Tokens:StringResource, admin.tabs.Images %>' /></a></li>
					<li><a href="#summary-tab" role="tab">
						<asp:Literal runat="server" ID="ltTabSummary" Text='<%$ Tokens:StringResource, admin.tabs.Summary %>' /></a></li>
					<li><a href="#description-tab" role="tab">
						<asp:Literal runat="server" ID="ltTabDescription" Text='<%$ Tokens:StringResource, admin.tabs.Description %>' /></a></li>
					<li><a href="#searchengine-tab" role="tab">
						<asp:Literal runat="server" ID="ltTabSearchEngine" Text='<%$ Tokens:StringResource, admin.tabs.SearchEngine %>' /></a></li>
					<li><a href="#productfeeds-tab" role="tab">
						<asp:Literal runat="server" ID="ltTabProductFeeds" Text='<%$ Tokens:StringResource, admin.tabs.ProductFeeds %>' /></a></li>
					<li><a href="#mappings-tab" role="tab">
						<asp:Literal runat="server" ID="ltTabMappings" Text='<%$ Tokens:StringResource, admin.tabs.Mappings %>' /></a></li>
					<li runat="server" id="liStoreMappingTab"><a href="#storemapping-tab" role="tab">
						<asp:Literal runat="server" ID="ltTabStoreMapping" Text='<%$ Tokens:StringResource, admin.tabs.StoreMapping %>' /></a></li>
					<li><a href="#options-tab" role="tab">
						<asp:Literal runat="server" ID="ltTabOptions" Text='<%$ Tokens:StringResource, admin.tabs.Options %>' /></a></li>
					<li><a href="#extensiondata-tab" role="tab">
						<asp:Literal runat="server" ID="ltTabExtensionData" Text='<%$ Tokens:StringResource, admin.tabs.ExtensionData %>' /></a></li>
				</ul>
				<!-- Tab panes -->
				<div class="tab-content white-ui-box">
					<div class="tab-pane active" id="main-tab">
						<div class="admin-row">
							<table class="table table-detail">
								<tr>
									<td style="width: 30%;">
										<span class="text-danger">*</span>Name:
									</td>
									<td>
										<asp:TextBox CssClass="text-lg" ID="txtName" runat="Server" />
										<asp:RequiredFieldValidator ControlToValidate="txtName" CssClass="text-danger" ErrorMessage="Please enter the Product Name (Main Tab)"
											ID="rfvName" ValidationGroup="Main" EnableClientScript="true" SetFocusOnError="true"
											runat="server" Display="Static">!!</asp:RequiredFieldValidator>
										<asp:CustomValidator runat="server"
											ID="NameLengthValidator"
											ValidationGroup="Main" 
											ControlToValidate="txtName"
											OnServerValidate="NameLength_ServerValidate"
											ErrorMessage="The product name is too long (Main Tab)"
											CssClass="text-danger" 
											SetFocusOnError="true"
											Text="!!"
											Display="Static" />
									</td>
								</tr>
								<tr>
									<td>
										<span class="text-danger">*</span>Product Type:
									</td>
									<td>
										<asp:DropDownList ID="ddType" CssClass="comboBox" runat="server" />
										<asp:RequiredFieldValidator ControlToValidate="ddType" CssClass="text-danger" InitialValue="0" ErrorMessage="Please select a product type (Main Tab)"
											ID="RequiredFieldValidator3" ValidationGroup="Main" EnableClientScript="true"
											SetFocusOnError="true" runat="server" Display="Static">!!</asp:RequiredFieldValidator>
									</td>
								</tr>
								<tr>
									<td>
										<span class="text-danger">*</span>Manufacturer:
									</td>
									<td>
										<asp:DropDownList CssClass="comboBox" ID="ddManufacturer" runat="server" />
										<asp:RequiredFieldValidator ControlToValidate="ddManufacturer" CssClass="text-danger" InitialValue="0" ErrorMessage="Please select a manufacturer (Main Tab)"
											ID="RequiredFieldValidator1" ValidationGroup="Main" EnableClientScript="true"
											SetFocusOnError="true" runat="server" Display="Static">!!</asp:RequiredFieldValidator>
										<QuickAdd:QuickAddEntity ID="QuickAddManufacturer" EntityType="Manufacturer" runat="server" />										
									</td>
								</tr>
								<tr runat="server" id="trDistributor">
									<td>Distributor:
									</td>
									<td>
										<asp:DropDownList CssClass="comboBox" ID="ddDistributor" runat="server" />
										<QuickAdd:QuickAddEntity ID="QuickAddDistributor" EntityType="Distributor" runat="server" />
									</td>
								</tr>
								<tr>
									<td>SKU:
									</td>
									<td>
										<asp:TextBox MaxLength="50" ID="txtSKU" runat="server" CssClass="text-sm" />
									</td>
								</tr>
								<tr>
									<td>Manufacturer Part&nbsp;#:
									</td>
									<td>
										<asp:TextBox MaxLength="50" ID="txtManufacturerPartNumber" runat="server" CssClass="text-sm" />
									</td>
								</tr>

								<tr>
									<td>
										<span class="text-danger">*</span><asp:Label ID="lblPublished" runat="server">Published:</asp:Label>
									</td>
									<td>
										<asp:RadioButtonList ID="rblPublished" runat="server" RepeatColumns="2" RepeatDirection="horizontal">
											<asp:ListItem Value="0" Text="No" />
											<asp:ListItem Value="1" Text="Yes" Selected="true" />
										</asp:RadioButtonList>
									</td>
								</tr>
								<tr>
									<td>
										<span class="text-danger">*</span><asp:Label ID="lblIsFeatured" runat="server">Featured:</asp:Label>
									</td>
									<td>
										<asp:RadioButtonList ID="rblFeatured" runat="server" RepeatColumns="2" RepeatDirection="horizontal" CssClass="horizontal-radio-helper">
											<asp:ListItem Value="0" Text="No" />
											<asp:ListItem Value="1" Text="Yes" Selected="true" />
										</asp:RadioButtonList>
										<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.entityeditproduct.ToolTip.Featured %>">
											<i class="fa fa-question-circle fa-lg"></i>
										</asp:Label>
									</td>
								</tr>
								<tr>
									<td>
										<span class="text-danger">*</span>Page Display:
									</td>
									<td>
										<aspdnsf:XmlPackageSelector runat="server" 
											ID="ddXmlPackage"
											Prefix="product"
											Locations="RootAndSkin" 
											CssClass="comboBox" />
										<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.entityeditproduct.ToolTip.XMLPackage %>">
											<i class="fa fa-question-circle fa-lg"></i>
										</asp:Label>
									</td>
								</tr>
								<tr>
									<td>Column Width:
									</td>
									<td>
										<asp:TextBox MaxLength="2" ID="txtColumn" runat="server" CssClass="text-2" />
										<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.entityeditproduct.ToolTip.ColumnWidth %>">
											<i class="fa fa-question-circle fa-lg"></i>
										</asp:Label>
									</td>
								</tr>
								<tr>
									<td>Tax Class:
									</td>
									<td>
										<asp:DropDownList CssClass="comboBox" ID="ddTaxClass" runat="Server" />
									</td>
								</tr>
								<tr>
									<td>Quantity Discount Table:
									</td>
									<td>
										<asp:DropDownList CssClass="comboBox" ID="ddDiscountTable" runat="Server" />
										<QuickAdd:QuickAddQuantityDiscounts ID="QuickAddQuantityDiscounts" runat="server" />
									</td>
								</tr>
								<tr>
									<td>
										<span class="text-danger">*</span>Show Buy Button:
									</td>
									<td>
										<asp:RadioButtonList ID="rblShowBuyButton" runat="server" RepeatColumns="2" RepeatDirection="horizontal" RepeatLayout="Flow">
											<asp:ListItem Value="0" Text="No" />
											<asp:ListItem Value="1" Text="Yes" Selected="True" />
										</asp:RadioButtonList>
										<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.entityeditproduct.ToolTip.ShowBuyButton %>">
											<i class="fa fa-question-circle fa-lg"></i>
										</asp:Label>
									</td>
								</tr>
								<tr>
									<td>
										<span class="text-danger">*</span>Requires Registration To View:
									</td>
									<td>
										<asp:RadioButtonList ID="rblRequiresRegistrationToView" runat="server" RepeatColumns="2" RepeatDirection="horizontal" RepeatLayout="Flow">
											<asp:ListItem Value="0" Text="No" />
											<asp:ListItem Value="1" Text="Yes" Selected="True" />
										</asp:RadioButtonList>
									</td>
								</tr>
								<tr>
									<td>
										<span class="text-danger">*</span>Is Call to Order:
									</td>
									<td>
										<asp:RadioButtonList ID="rblIsCallToOrder" runat="server" RepeatColumns="2" RepeatDirection="horizontal" RepeatLayout="Flow">
											<asp:ListItem Value="0" Text="No" />
											<asp:ListItem Value="1" Text="Yes" Selected="True" />
										</asp:RadioButtonList>
										<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.entityeditproduct.ToolTip.CallToOrder %>">
											<i class="fa fa-question-circle fa-lg"></i>
										</asp:Label>
									</td>
								</tr>
								<tr>
									<td>
										<span class="text-danger">*</span>Hide Price Until Cart:
									</td>
									<td>
										<asp:RadioButtonList ID="rblHidePriceUntilCart" runat="server" RepeatColumns="2" RepeatDirection="horizontal" RepeatLayout="Flow">
											<asp:ListItem Value="0" Text="No" />
											<asp:ListItem Value="1" Text="Yes" Selected="True" />
										</asp:RadioButtonList>
										<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.entityeditproduct.ToolTip.HidePriceUntilCart %>">
											<i class="fa fa-question-circle fa-lg"></i>
										</asp:Label>
									</td>
								</tr>
								<tr>
									<td>
										<span class="text-danger">*</span>Exclude from Product Feeds:
									</td>
									<td>
										<asp:RadioButtonList ID="rblExcludeFroogle" runat="server" RepeatColumns="2" RepeatDirection="horizontal" RepeatLayout="Flow">
											<asp:ListItem Value="0" Text="No" Selected="True" />
											<asp:ListItem Value="1" Text="Yes" />
										</asp:RadioButtonList>
									</td>
								</tr>
								<tr id="trKit" runat="server">
									<td>
										<span class="text-danger">*</span>Is a Kit:
									</td>
									<td>
										<asp:RadioButtonList ID="rblIsKit" runat="server" RepeatColumns="2" RepeatDirection="horizontal" RepeatLayout="Flow">
											<asp:ListItem Value="0" Text="No" />
											<asp:ListItem Value="1" Text="Yes" Selected="True" />
										</asp:RadioButtonList>
										<asp:Literal ID="ltKit" runat="server" />
									</td>
								</tr>
								<tr runat="server" id="trInventory1">
									<td>
										<span class="text-danger">*</span>Track Inventory By Size and Color:
									</td>
									<td>
										<asp:RadioButtonList ID="rblTrackSizeColor" runat="server" RepeatColumns="2" RepeatDirection="horizontal" RepeatLayout="Flow">
											<asp:ListItem Value="0" Text="No" />
											<asp:ListItem Value="1" Text="Yes" Selected="True" />
										</asp:RadioButtonList>
										<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.entityeditproduct.ToolTip.TrackInventoryBySizeAndColor %>">
											<i class="fa fa-question-circle fa-lg"></i>
										</asp:Label>
									</td>
								</tr>
								<tr runat="server" id="trInventory2">
									<td>Color Option Prompt:
									</td>
									<td>
										<asp:TextBox MaxLength="100" ID="txtColorOption" runat="server" CssClass="text-md" />
									</td>
								</tr>
								<tr runat="server" id="trInventory3">
									<td>Size Option Prompt:
									</td>
									<td>
										<asp:TextBox MaxLength="100" ID="txtSizeOption" runat="server" CssClass="text-md" />
									</td>
								</tr>
								<tr>
									<td>
										<span class="text-danger">*</span>Requires Text Field:
									</td>
									<td>
										<div>
											<asp:RadioButtonList ID="rblRequiresTextField" runat="server" RepeatColumns="2" RepeatDirection="horizontal" RepeatLayout="Flow">
												<asp:ListItem Value="0" Text="No" />
												<asp:ListItem Value="1" Text="Yes" Selected="True" />
											</asp:RadioButtonList>
											<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.entityeditproduct.ToolTip.RequiresTextField %>">
												<i class="fa fa-question-circle fa-lg"></i>
											</asp:Label>
										</div>
										<div>
											Field Prompt:
											<asp:TextBox MaxLength="100" ID="txtTextFieldPrompt" runat="server" CssClass="text-md" />
										</div>
										<div>
											Max Length:
											<asp:TextBox MaxLength="3" ID="txtTextOptionMax" runat="server" CssClass="text-2" />
										</div>
									</td>
								</tr>
							</table>
						</div>
						<div class="admin-row">
							<asp:PlaceHolder runat="Server" ID="phAddVariant">
								<div class="wrapperExtraTop">
									<h2>
										<asp:Literal runat="server" ID="litDefaultVariantTitle" Text='<%$ Tokens:StringResource, admin.product.DefaultVariant %>' /></h2>
									<table class="table table-detail">
										<tr>
											<td style="width: 30%;">Price:
											</td>
											<td>
												<asp:TextBox MaxLength="10" ID="txtPrice" runat="server" CssClass="singleShortest" />
											</td>
										</tr>
										<tr>
											<td>Sale Price:
											</td>
											<td>
												<asp:TextBox MaxLength="10" ID="txtSalePrice" runat="server" CssClass="singleShortest" />
											</td>
										</tr>
										<tr>
											<td>Weight:
											</td>
											<td>
												<asp:TextBox MaxLength="10" ID="txtWeight" runat="server" CssClass="single3chars" />
												<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.entityeditproduct.ToolTip.Weight %>">
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
										<tr id="trInventory" runat="server">
											<td>Current Inventory:
											</td>
											<td>
												<asp:TextBox MaxLength="10" ID="txtInventory" runat="server" CssClass="singleShorter" />
											</td>
										</tr>
										<tr>
											<td>Colors:
											</td>
											<td>
												<asp:TextBox ID="txtColors" runat="server" CssClass="singleNormal" />
												<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.ToolTip.Colors %>">
													<i class="fa fa-question-circle fa-lg"></i>
												</asp:Label>
											</td>
										</tr>
										<tr>
											<td>Color SKU Modifiers:
											</td>
											<td>
												<asp:TextBox ID="txtColorSKUModifiers" runat="server" CssClass="singleNormal" />
												<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.ToolTip.ColorSkuMods %>">
													<i class="fa fa-question-circle fa-lg"></i>
												</asp:Label>
											</td>
										</tr>
										<tr>
											<td>Sizes:
											</td>
											<td>
												<asp:TextBox ID="txtSizes" runat="server" CssClass="singleNormal" />
												<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.ToolTip.Sizes %>">
													<i class="fa fa-question-circle fa-lg"></i>
												</asp:Label>
											</td>
										</tr>
										<tr>
											<td>Size SKU Modifiers:
											</td>
											<td>
												<asp:TextBox ID="txtSizeSKUModifiers" runat="server" CssClass="singleNormal" />
												<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.ToolTip.SizeSkuMods %>">
													<i class="fa fa-question-circle fa-lg"></i>
												</asp:Label>
											</td>
										</tr>
									</table>
								</div>
							</asp:PlaceHolder>
							<asp:PlaceHolder ID="phAllVariants" runat="server">
								<div class="wrapperExtraTop">
									<table class="table table-detail">
										<tr>
											<td>Action:
											</td>
											<td>
												<asp:Literal ID="ltVariantsLinks" runat="server" />
												|
												<asp:LinkButton ID="btnDeleteAll" runat="server" Text="Delete All Variants" OnClick="btnDeleteAll_Click" />
											</td>
										</tr>
									</table>
								</div>
							</asp:PlaceHolder>
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
					<div class="tab-pane" id="summary-tab">
						<div class="admin-row">
							<table class="table table-detail">
								<tr>
									<td>
										<div class="form-group">
											<asp:TextBox ID="txtSummaryNoHtmlEditor" Rows="15" TextMode="MultiLine" runat="server" Visible="false" CssClass="form-control" />
										</div>
										<telerik:radeditor id="radSummary" runat="server" editable="true" SkinID="RadEditorSettings" />
										<br />
										<asp:Literal ID="ltSummaryAuto" runat="server" />
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
										<telerik:radeditor id="radDescription" runat="server" editable="true" SkinID="RadEditorSettings" />
									</td>
								</tr>
							</table>
						</div>
					</div>
					<div class="tab-pane" id="searchengine-tab">
						<div class="admin-row">
							<table class="table table-detail">
								<tr>
									<td>Page Title:
									</td>
									<td>
										<asp:TextBox CssClass="text-md" ID="txtSETitle" runat="Server" Style="width: 600px" />
									</td>
								</tr>
								<tr>
									<td>Keywords:
									</td>
									<td>
										<asp:TextBox CssClass="text-md" ID="txtSEKeywords" runat="Server" Style="width: 600px" />
									</td>
								</tr>
								<tr>
									<td>Description:
									</td>
									<td>
										<asp:TextBox CssClass="text-md" ID="txtSEDescription" runat="Server" Style="width: 600px" />
									</td>
								</tr>
								<tr id="Tr1" runat="server">
									<td>Alt Text:
									</td>
									<td>
										<asp:TextBox CssClass="text-md" ID="txtSEAlt" runat="Server" Style="width: 600px" />
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
					<div class="tab-pane" id="mappings-tab">
						<div class="admin-row">
							<asp:Panel ID="pnlMapCategories" runat="server" CssClass="mappings-alignment">
								<h2><i class="fa fa-sitemap"></i> Categories:</h2>
								<asp:CheckBoxList ID="cblCategory" runat="server" CellPadding="0" CellSpacing="0" RepeatColumns="1" RepeatDirection="Vertical" RepeatLayout="Flow" />
							</asp:Panel>
							<asp:Panel ID="pnlMapDepartments" runat="server" CssClass="mappings-alignment">
								<h2><i class="fa fa-sitemap"></i> Departments:</h2>
								<asp:CheckBoxList ID="cblDepartment" runat="server" CellPadding="0" CellSpacing="0" RepeatColumns="1" RepeatDirection="Vertical" RepeatLayout="Flow" />
							</asp:Panel>
							<asp:Panel ID="pnlMapAffiliates" runat="server" CssClass="mappings-alignment">
								<h2><i class="fa fa-retweet"></i> Affiliates:</h2>
								<asp:CheckBoxList ID="cblAffiliates" runat="server" CellPadding="0" CellSpacing="0" RepeatColumns="1" RepeatDirection="Vertical" RepeatLayout="Flow" />
							</asp:Panel>
							<asp:Panel ID="pnlMapCustomerLevels" runat="server" CssClass="mappings-alignment">
								<h2><i class="fa fa-list-ol"></i> Customer Levels:</h2>
								<asp:CheckBoxList ID="cblCustomerLevels" runat="server" CellPadding="0" CellSpacing="0" RepeatColumns="1" RepeatDirection="Vertical" RepeatLayout="Flow" />
								<QuickAdd:QuickAddCustomerLevel ID="QuickAddCustomerLevel" runat="server" />
							</asp:Panel>
							<asp:Panel ID="pnlMapGenres" runat="server" CssClass="mappings-alignment-hidden">
								<h2><i class="fa fa-sitemap"></i> Genres:</h2>
								<asp:CheckBoxList ID="cblGenres" runat="server" CellPadding="0" CellSpacing="0" RepeatColumns="1" RepeatDirection="Vertical" RepeatLayout="Flow" />
							</asp:Panel>
							<asp:Panel ID="pnlMapVectors" runat="server" CssClass="mappings-alignment-hidden">
								<h2><i class="fa fa-sitemap"></i> Vectors:</h2>
								<asp:CheckBoxList ID="cblVectors" runat="server" CellPadding="0" CellSpacing="0" RepeatColumns="1" RepeatDirection="Vertical" RepeatLayout="Flow" />
							</asp:Panel>
							<div style="clear: both;"></div>
						</div>
					</div>
					<div class="tab-pane" id="storemapping-tab">
						<div class="admin-row">
							<aspdnsf:EntityToStore ID="etsMapper" EntityType="Product" runat="server" />
						</div>
					</div>
					<div class="tab-pane" id="options-tab">
						<div class="form-group">
							<div class="row">
								<div class="col-sm-2">
									Related Products:
								</div>
								<div class="col-sm-3">
									<asp:TextBox CssClass="text-md" ID="txtRelatedProducts" runat="Server" />
									<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.entityeditproduct.ToolTip.RelatedProducts %>">
									<i class="fa fa-question-circle fa-lg"></i>
									</asp:Label>
									<br />
									<asp:RegularExpressionValidator ID="valRelatedProducts" ControlToValidate="txtRelatedProducts" Display="Dynamic" runat="server" CssClass="text-danger" ValidationGroup="Main" ValidationExpression="[0-9,]*" ErrorMessage="Related Products must be comma separated product IDs" />
								</div>
								<div class="col-sm-7">
									<div class="panel-group">
										<div class="panel panel-default">
											<div class="panel-heading" id="headingOne">
												<h4 class="panel-title">
													<a class="collapsed" data-toggle="collapse" data-parent="#accordion" href="#collapseOne" aria-expanded="false" aria-controls="collapseOne">
														<asp:Literal Text="Related Products Helper" runat="server" />
														<asp:Label runat="server" CssClass="hover-help pull-right" data-toggle="tooltip" ToolTip="The Related Products Helper allows you to easily search for, add, and remove related product IDs. Click here to show or hide the helper.">
													<i class="fa fa-question-circle fa-lg"></i>
														</asp:Label>
													</a>
												</h4>
											</div>
											<div id="collapseOne" class="panel-collapse collapse" role="tabpanel" aria-labelledby="headingOne">
												<div class="panel-body">
													<div class="row">
														<div class="col-sm-12">
															<div class="form-group">
																<aspdnsf:CSVHelper ID="relatedHelper" runat="server" CSVTextBoxID="txtRelatedProducts" UniqueJSID="Related" CSVSearchButtonText="<%$Tokens:StringResource, admin.common.Search%>" />
															</div>
														</div>
													</div>
												</div>
											</div>
										</div>
									</div>
								</div>
							</div>
							<div class="row">
								<div class="col-sm-2">
									Upsell Products:
								</div>
								<div class="col-sm-3">
									<asp:TextBox CssClass="text-md" ID="txtUpsellProducts" runat="Server" />
									<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.entityeditproduct.ToolTip.UpsellProducts %>">
										<i class="fa fa-question-circle fa-lg"></i>
									</asp:Label>
									<br />
									<asp:RegularExpressionValidator ID="valUpsellProducts" ControlToValidate="txtUpsellProducts"
										Display="Dynamic" CssClass="text-danger" runat="SERVER" ValidationGroup="Main" ValidationExpression="[0-9,]*"
										ErrorMessage="Upsell Products must be comma seperated product IDs" />
								</div>
								<div class="col-sm-7">
									<div class="panel-group">
										<div class="panel panel-default">
											<div class="panel-heading" id="headingTwo">
												<h4 class="panel-title">
													<a class="collapsed" data-toggle="collapse" data-parent="#accordion" href="#collapseTwo" aria-expanded="false" aria-controls="collapseTwo">
														<asp:Literal Text="Upsell Products Helper" runat="server" />
														<asp:Label runat="server" CssClass="hover-help pull-right" data-toggle="tooltip" ToolTip="The Upsell Products Helper allows you to easily search for, add, and remove upsell product IDs. Click here to show or hide the helper.">
													<i class="fa fa-question-circle fa-lg"></i>
														</asp:Label>
													</a>
												</h4>
											</div>
											<div id="collapseTwo" class="panel-collapse collapse" role="tabpanel" aria-labelledby="headingTwo">
												<div class="panel-body">
													<div class="row">
														<div class="col-sm-12">
															<div class="form-group">
																<aspdnsf:CSVHelper ID="upsellHelper" runat="server" CSVTextBoxID="txtUpsellProducts" UniqueJSID="Upsell" CSVSearchButtonText="<%$Tokens:StringResource, admin.common.Search%>" />
															</div>
														</div>
													</div>
												</div>
											</div>
										</div>
									</div>
								</div>
							</div>
							<div class="row">
								<div class="col-sm-2">
									Required Products:
								</div>
								<div class="col-sm-3">
									<asp:TextBox CssClass="text-md" ID="txtRequiresProducts" runat="Server" />
									<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.entityeditproduct.ToolTip.RequiresProducts %>">
									<i class="fa fa-question-circle fa-lg"></i>
									</asp:Label>
									<br />
									<asp:RegularExpressionValidator ID="valRequiredProducts" ControlToValidate="txtRequiresProducts"
										Display="Dynamic" runat="SERVER" CssClass="text-danger" ValidationGroup="Main" ValidationExpression="[0-9,]*"
										ErrorMessage="Required Products must be comma seperated product IDs" />
								</div>
								<div class="col-sm-7">
									<div class="panel-group">
										<div class="panel panel-default">
											<div class="panel-heading" id="headingThree">
												<h4 class="panel-title">
													<a class="collapsed" data-toggle="collapse" data-parent="#accordion" href="#collapseThree" aria-expanded="false" aria-controls="collapseThree">
														<asp:Literal Text="Required Products Helper" runat="server" />
														<asp:Label runat="server" CssClass="hover-help pull-right" data-toggle="tooltip" ToolTip="The Required Products Helper allows you to easily search for, add, and remove required product IDs. Click here to show or hide the helper.">
															<i class="fa fa-question-circle fa-lg"></i>
														</asp:Label>
													</a>
												</h4>
											</div>
											<div id="collapseThree" class="panel-collapse collapse" role="tabpanel" aria-labelledby="headingThree">
												<div class="panel-body">
													<div class="row">
														<div class="col-sm-12">
															<div class="form-group">
																<aspdnsf:CSVHelper ID="requiresHelper" runat="server" CSVTextBoxID="txtRequiresProducts" UniqueJSID="Requires" CSVSearchButtonText="<%$Tokens:StringResource, admin.common.Search%>" />
															</div>
														</div>
													</div>
												</div>
											</div>
										</div>
									</div>
								</div>
							</div>
							<div class="row">
								<div class="col-sm-2">
									Upsell Product Discount Percent:
								</div>
								<div class="col-sm-3">
									<asp:TextBox MaxLength="5" CssClass="text-md" ID="txtUpsellProductsDiscount" runat="Server" />
									<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.entityeditproduct.ToolTip.UpsellProductDiscount %>">
												<i class="fa fa-question-circle fa-lg"></i>
									</asp:Label>
								</div>
								<div class="col-sm-7">
								</div>
							</div>
						</div>
						<div class="form-group">
							<div class="row">
								<div class="col-sm-2">
									<span class="text-danger">*</span>'On Sale' Prompt:
								</div>
								<div class="col-sm-3">
									<asp:DropDownList ID="ddOnSalePrompt" runat="server" />
									<br />
									<asp:RequiredFieldValidator ControlToValidate="ddOnSalePrompt" InitialValue="0" ErrorMessage="Please select an 'On Sale' prompt"
										ID="RequiredFieldValidator4" CssClass="text-danger" ValidationGroup="Main" EnableClientScript="true"
										SetFocusOnError="true" runat="server" Display="Dynamic" />
								</div>
								<div class="col-sm-7">
								</div>
							</div>
						</div>
						<div style="clear: both;"></div>
						<script type="text/javascript">
							jQuery(document).ready(function () {
								$('.CSVHelpers .head').click(function () {
									$(this).next().toggle('fast');
									return false;
								}).next().hide();
							});
						</script>
					</div>
					<div class="tab-pane" id="extensiondata-tab">
						<div class="form-group">
							<div class="admin-row">
								<div class="col-sm-2">
									<asp:Label ID="lblExtensionData1" runat="server" AssociatedControlID="txtExtensionData1">Custom Field 1:</asp:Label>
								</div>
								<div class="col-sm-10">
									<div class="col-sm-7">
										<asp:TextBox ID="txtExtensionData1" runat="Server" CssClass="form-control multiExtension" TextMode="multiLine" CausesValidation="false" />
									</div>
								</div>
							</div>
							<div class="admin-row">
								<div class="col-sm-2">
									<asp:Label ID="lblExtensionData2" runat="server" AssociatedControlID="txtExtensionData2">Custom Field 2:</asp:Label>
								</div>
								<div class="col-sm-10">
									<div class="col-sm-7">
										<asp:TextBox ID="txtExtensionData2" runat="Server" CssClass="form-control multiExtension" TextMode="multiLine" CausesValidation="false" />
									</div>
								</div>
							</div>
							<div class="admin-row">
								<div class="col-sm-2">
									<asp:Label ID="lblExtensionData3" runat="server" AssociatedControlID="txtExtensionData3">Custom Field 3:</asp:Label>
								</div>
								<div class="col-sm-10">
									<div class="col-sm-7">
										<asp:TextBox ID="txtExtensionData3" runat="Server" CssClass="form-control multiExtension" TextMode="multiLine" CausesValidation="false" />
									</div>
								</div>
							</div>
							<div class="admin-row">
								<div class="col-sm-2">
									<asp:Label ID="lblExtensionData4" runat="server" AssociatedControlID="txtExtensionData4">Custom Field 4:</asp:Label>
								</div>
								<div class="col-sm-10">
									<div class="col-sm-7">
										<asp:TextBox ID="txtExtensionData4" runat="Server" CssClass="form-control multiExtension" TextMode="multiLine" CausesValidation="false" />
									</div>
								</div>
							</div>
							<div class="admin-row">
								<div class="col-sm-2">
									<asp:Label ID="lblExtensionData5" runat="server" AssociatedControlID="txtExtensionData5">Custom Field 5:</asp:Label>
								</div>
								<div class="col-sm-10">
									<div class="col-sm-7">
										<asp:TextBox ID="txtExtensionData5" runat="Server" CssClass="form-control multiExtension" TextMode="multiLine" CausesValidation="false" />
									</div>
								</div>
							</div>
							<div class="admin-row">
								<div class="col-sm-2">
									<asp:Label ID="lblMiscText" runat="server" AssociatedControlID="txtMiscText">Misc Text:</asp:Label>
								</div>
								<div class="col-sm-10">
									<div class="col-sm-7">
										<asp:TextBox CssClass="form-control multiExtension" ID="txtMiscText" runat="Server" TextMode="multiLine" CausesValidation="false" />
									</div>
								</div>
							</div>
						</div>
						<div style="clear: both;"></div>
					</div>
					<asp:ValidationSummary ID="validationSummary" runat="Server" ValidationGroup="Main" DisplayMode="BulletList" ShowMessageBox="true" ShowSummary="false" />
					<asp:Literal ID="ltScript" runat="server" />
					<asp:Literal ID="ltScript2" runat="server" />
					<asp:Literal ID="ltScript3" runat="server" />
				</div>
			</div>
			<div class="item-action-bar">
				<asp:HyperLink runat="server"
					CssClass="btn btn-default"
					Visible='<%# Editing %>'
					NavigateUrl='<%# String.Format("variants.aspx?productid={0}", ProductId) %>'
					Text="Manage Variants" />

				<asp:HyperLink runat="server"
					CssClass="btn btn-default"
					NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>"
					Text="<%$Tokens:StringResource, admin.common.close %>" />

				<asp:Button runat="server"
					CssClass="btn btn-default"
					ValidationGroup="Main"
					OnClick="btnSaveAndClose_Click"
					Text="<%$Tokens:StringResource, admin.common.saveandclose %>" />

				<asp:Button runat="server"
					CssClass="btn btn-primary"
					ValidationGroup="Main"
					OnClick="btnSubmit_Click"
					Text="<%$Tokens:StringResource, admin.common.save %>" />
			</div>
		</asp:Panel>
	</div>
</asp:Content>
