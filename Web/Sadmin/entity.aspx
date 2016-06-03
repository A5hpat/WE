<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.EntityEditor" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="entity.aspx.cs" %>

<%@ Register TagPrefix="AjaxToolkit" Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" %>
<%@ Register TagPrefix="telerik" Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" %>
<%@ Register TagPrefix="aspdnsf" TagName="EntityToStore" Src="controls/EntityToStoreMapper.ascx" %>
<%@ Register TagPrefix="QuickAdd" TagName="QuickAddQuantityDiscounts" Src="Controls/QuickAdd/QuickAddQuantityDiscounts.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="bodyContentPlaceholder" runat="server">
	<asp:Panel ID="pnlContent" runat="server" DefaultButton="btnSubmit" CssClass="wrapper">
		<div class="main-breadcrumb">
			<div>
				<asp:Literal ID="ltEntity" runat="server" />
			</div>
		</div>

		<h1 runat="server" id="header">
			<i class="fa fa-sitemap"></i>
			<asp:Literal ID="HeaderText" runat="server" />
		</h1>

		<aspdnsf:ReturnUrlTracker runat="server" ID="ReturnUrlTracker" DefaultReturnUrl="Entities.aspx" Blacklist="Product.aspx,EntityProductMap.aspx" />

		<aspdnsf:AlertMessage runat="server" ID="ctrlAlertMessage" />

		<div class="item-action-bar clearfix">
			<div class="other-actions">
				<asp:Panel ID="pnlLocale" runat="server" Visible='<%# LocaleSelector.HasMultipleLocales() %>'>
					<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.stringresources.Locale %>" AssociatedControlID="LocaleSelector" />
					<aspdnsf:LocaleSelector ID="LocaleSelector" runat="server" OnSelectedLocaleChanged="LocaleSelector_SelectedLocaleChanged" />
				</asp:Panel>
			</div>

			<asp:HyperLink runat="server"
				ID="CancelLinkTop"
				CssClass="btn btn-default"
				NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>"
				Text="<%$Tokens:StringResource, admin.common.close %>" />

			<asp:Button ID="btnClose" runat="server" CssClass="btn btn-default" OnClick="btnClose_Click" ValidationGroup="Main" Text="<%$Tokens:StringResource, admin.common.SaveAndClose %>" />
			<asp:Button ID="btnSubmit" runat="server" OnClick="btnSubmit_Click" CssClass="btn btn-primary" ValidationGroup="Main" Text="<%$Tokens:StringResource, admin.common.Save %>" />
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
				<li><a href="#extensiondata-tab" role="tab">
					<asp:Literal runat="server" ID="ltTabExtensionData" Text='<%$ Tokens:StringResource, admin.tabs.ExtensionData %>' /></a></li>
				<li runat="server" id="liStoreMappingTab"><a href="#storemapping-tab" role="tab">
					<asp:Literal runat="server" ID="ltTabStoreMapping" Text='<%$ Tokens:StringResource, admin.tabs.StoreMapping %>' /></a></li>
				<li><a href="#searchengine-tab" role="tab">
					<asp:Literal runat="server" ID="ltTabSearchEngine" Text='<%$ Tokens:StringResource, admin.tabs.SearchEngine %>' /></a></li>
				<li runat="server" id="liProductsTab"><a href="#products-tab" role="tab">
					<asp:Literal runat="server" ID="ltTabProducts" Text='<%$ Tokens:StringResource, admin.tabs.Products %>' /></a></li>
			</ul>
			<!-- Tab panes -->
			<div class="tab-content white-ui-box">
				<div class="tab-pane active" id="main-tab">
					<div class="admin-row">
						<table class="table table-detail">
							<tr>
								<td><span class="text-danger">*</span><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.common.Name %>" />:
								</td>
								<td>
									<asp:TextBox CssClass="text-md" ID="txtName" runat="Server"></asp:TextBox>
									<asp:RequiredFieldValidator ControlToValidate="txtName" ID="rfvName" ValidationGroup="Main"
										EnableClientScript="true" SetFocusOnError="true" runat="server" Display="Static">!!</asp:RequiredFieldValidator>
								</td>
							</tr>
							<tr>
								<td><span class="text-danger">*</span><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.currencies.Published %>" />:
								</td>
								<td>
									<asp:RadioButtonList ID="rblPublished" runat="server" RepeatColumns="2" RepeatDirection="horizontal">
										<asp:ListItem Value="0" Text="No"></asp:ListItem>
										<asp:ListItem Value="1" Text="Yes" Selected="true"></asp:ListItem>
									</asp:RadioButtonList>
								</td>
							</tr>
							<tr id="trBrowser" runat="server">
								<td><span class="text-danger">*</span><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.ShowProductBrowser %>" />:
								</td>
								<td>
									<asp:RadioButtonList ID="rblBrowser" runat="server" RepeatColumns="2" RepeatDirection="horizontal">
										<asp:ListItem Value="0" Text="No"></asp:ListItem>
										<asp:ListItem Value="1" Text="Yes" Selected="true"></asp:ListItem>
									</asp:RadioButtonList>
								</td>
							</tr>
							<tr id="trParent" runat="server">
								<td><span class="text-danger">*</span><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.Parent %>" />:
								</td>
								<td>
									<asp:DropDownList ID="ddParent" CssClass="parent-entity-dropdown" runat="Server">
									</asp:DropDownList>
								</td>
							</tr>
							<tr>
								<td><span class="text-danger">*</span><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.DisplayFormatXmlPackage %>" />:
								</td>
								<td>
									<aspdnsf:XmlPackageSelector runat="server" 
										ID="ddXmlPackage"
										Prefix="entity"
										Locations="RootAndSkin" />
									<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$Tokens:StringResource, admin.entityedit.tooltip.XMLPackage %>">
										<i class="fa fa-question-circle fa-lg"></i>
									</asp:Label>
								</td>
							</tr>
							<tr id="trSubEntities" runat="server">
								<td>
									<asp:Label runat="server" ID="subEntityDisplayOrderLabel" />
								</td>
								<td>
									<asp:HyperLink runat="server" ID="DisplayOrderLink" Text="<%$Tokens:StringResource, admin.newentities.editDisplayOrder %>" />
								</td>
							</tr>
							<tr>
								<td>
									<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.QuantityDiscountTable %>" />:
								</td>
								<td>
									<asp:DropDownList ID="ddDiscountTable" runat="Server" />
									<QuickAdd:QuickAddQuantityDiscounts ID="QuickAddQuantityDiscounts" runat="server" />
								</td>
							</tr>
							<tr>
								<td>
									<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.PageSize %>" />:
								</td>
								<td>
									<asp:TextBox MaxLength="2" ID="txtPageSize" runat="server" CssClass="single3chars"></asp:TextBox>
									<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$Tokens:StringResource, admin.entityedit.tooltip.PageSize %>">
												<i class="fa fa-question-circle fa-lg"></i>
									</asp:Label>
								</td>
							</tr>
							<tr>
								<td>
									<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.Columns %>" />:
								</td>
								<td>
									<asp:TextBox MaxLength="2" ID="txtColumn" runat="server" CssClass="single3chars"></asp:TextBox>
									<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$Tokens:StringResource, admin.entityedit.tooltip.ColumnWidth %>">
												<i class="fa fa-question-circle fa-lg"></i>
									</asp:Label>
								</td>
							</tr>
							<tr>
								<td>
									<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.OrderProductsByLooks %>" />:
								</td>
								<td>
									<asp:RadioButtonList ID="rblLooks" runat="server" RepeatColumns="2" RepeatDirection="horizontal">
										<asp:ListItem Value="0" Text="No"></asp:ListItem>
										<asp:ListItem Value="1" Text="Yes" Selected="true"></asp:ListItem>
									</asp:RadioButtonList>
								</td>
							</tr>
							<!-- Address -->
							<asp:PlaceHolder ID="phAddress" runat="Server">
								<tr>
									<td>
										<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.common.Address %>" />:
									</td>
									<td>
										<asp:TextBox CssClass="text-md" ID="txtAddress1" runat="Server"></asp:TextBox>
									</td>
								</tr>
								<tr>
									<td>
										<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.AptSuite %>" />:
									</td>
									<td>
										<asp:TextBox CssClass="singleShortest" ID="txtApt" runat="Server"></asp:TextBox>
									</td>
								</tr>
								<tr>
									<td>
										<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.Address2 %>" />:
									</td>
									<td>
										<asp:TextBox CssClass="text-md" ID="txtAddress2" runat="Server"></asp:TextBox>
									</td>
								</tr>
								<tr>
									<td>
										<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.City %>" />:
									</td>
									<td>
										<asp:TextBox CssClass="singleShorter" ID="txtCity" runat="Server"></asp:TextBox>
									</td>
								</tr>
								<tr>
									<td>
										<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.State %>" />:
									</td>
									<td>
										<asp:DropDownList ID="ddState" runat="server">
										</asp:DropDownList>
									</td>
								</tr>
								<tr>
									<td>
										<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.ZipCode %>" />:
									</td>
									<td>
										<asp:TextBox CssClass="singleShortest" ID="txtZip" runat="Server"></asp:TextBox>
									</td>
								</tr>
								<tr>
									<td>
										<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.common.Country %>" />:
									</td>
									<td>
										<asp:DropDownList ID="ddCountry" runat="server">
										</asp:DropDownList>
									</td>
								</tr>
								<tr>
									<td>
										<asp:Label Visible="false" ID="lblEmailRequired" runat="server" class="text-danger">*</asp:Label><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.common.E-Mail %>" />:
									</td>
									<td>
										<asp:TextBox ID="txtEmail" runat="server"></asp:TextBox>
										<aspdnsf:EmailValidator ErrorMessage="Invalid Email Address Format (Main Tab)" ID="valRegExValEmail"
											ControlToValidate="txtEmail" runat="server" ValidationGroup="Main"></aspdnsf:EmailValidator>
										<asp:RequiredFieldValidator Visible="false" ErrorMessage="Fill in E-Mail (Main Tab)" ControlToValidate="txtEmail"
											ID="rfvEmail" ValidationGroup="Main" SetFocusOnError="true" runat="server">!!</asp:RequiredFieldValidator>
									</td>
								</tr>
								<tr>
									<td>
										<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.Website %>" />:
									</td>
									<td>
										<asp:TextBox ID="txtURL" runat="server"></asp:TextBox>
										<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$Tokens:StringResource, admin.entityedit.tooltip.Website %>">
													<i class="fa fa-question-circle fa-lg"></i>
										</asp:Label>
									</td>
								</tr>
								<tr>
									<td>
										<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editaddress.Phone %>" />
									</td>
									<td>
										<asp:TextBox ID="txtPhone" runat="server"></asp:TextBox>
										<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$Tokens:StringResource, admin.entityedit.tooltip.Phone %>">
													<i class="fa fa-question-circle fa-lg"></i>
										</asp:Label>
									</td>
								</tr>
								<tr>
									<td>
										<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.Fax %>" />:
									</td>
									<td>
										<asp:TextBox ID="txtFax" runat="server"></asp:TextBox>
										<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$Tokens:StringResource, admin.entityedit.tooltip.Fax %>">
													<i class="fa fa-question-circle fa-lg"></i>
										</asp:Label>
									</td>
								</tr>
							</asp:PlaceHolder>
						</table>
					</div>
				</div>
				<div class="tab-pane" id="images-tab">
					<div class="admin-row">
						<table class="table table-detail">
							<tr>
								<td>
									<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.ImageFilenameOverride %>" />:
								</td>
								<td>
									<asp:TextBox CssClass="text-md" ID="txtImageOverride" runat="Server"></asp:TextBox>
								</td>
							</tr>
							<tr>
								<td>
									<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.common.Icon %>" />:
								</td>
								<td>
									<asp:FileUpload CssClass="fileUpload" ID="fuIcon" runat="Server" />
									<div>
										<asp:Literal ID="ltIcon" runat="server"></asp:Literal>
									</div>
								</td>
							</tr>
							<tr>
								<td>
									<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.Medium %>" />:
								</td>
								<td>
									<asp:FileUpload CssClass="fileUpload" ID="fuMedium" runat="Server" />
									<div>
										<asp:Literal ID="ltMedium" runat="server"></asp:Literal>
									</div>
								</td>
							</tr>
							<tr>
								<td>
									<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.Large %>" />:
								</td>
								<td>
									<asp:FileUpload CssClass="fileUpload" ID="fuLarge" runat="Server" />
									<div>
										<asp:Literal ID="ltLarge" runat="server"></asp:Literal>
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
									<telerik:RadEditor runat="server" ID="radSummary" SkinID="RadEditorSettings" />
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
									<telerik:RadEditor runat="server" ID="radDescription" SkinID="RadEditorSettings" />
								</td>
							</tr>
						</table>
					</div>
				</div>
				<div class="tab-pane" id="extensiondata-tab">
					<div class="admin-row">
						<table class="table table-detail">
							<tr>
								<td>
									<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.ExtensionData %>" />:
								</td>
							</tr>
							<tr>
								<td>
									<div class="form-group">
										<asp:TextBox class="form-control multiExtension" Rows="0" Columns="0" ID="txtExtensionData" runat="Server" TextMode="multiLine"></asp:TextBox>
									</div>
								</td>
							</tr>
							<tr id="Tr2" runat="server" visible="false">
								<td>
									<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.UseSkinTemplate %>" />:
								</td>
								<td>
									<asp:TextBox CssClass="singleShorter" ID="txtUseSkinTemplateFile" runat="Server"></asp:TextBox>
								</td>
							</tr>
							<tr id="Tr1" runat="server" visible="false">
								<td>
									<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.UseSkinID %>" />:
								</td>
								<td>
									<asp:TextBox CssClass="singleShorter" ID="txtUseSkinID" runat="Server"></asp:TextBox>
								</td>
							</tr>
						</table>
					</div>
				</div>
				<div class="tab-pane" id="storemapping-tab">
					<div class="admin-row">
						<aspdnsf:EntityToStore ID="etsMapper" EntityType="Category" runat="server" />
						<asp:Panel ID="pnlStoreMapNotSupported" runat="server" Visible="false">
							<strong style="display: block; padding: 10px 5px;">Store Mapping is not supported for this entity.</strong>
						</asp:Panel>
					</div>
				</div>
				<div class="tab-pane" id="searchengine-tab">
					<div class="admin-row">
						<table class="table table-detail">
							<tr>
								<td>
									<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.PageTitle %>" />:
								</td>
								<td>
									<asp:TextBox CssClass="singleAuto" ID="txtSETitle" runat="Server"></asp:TextBox>
								</td>
							</tr>
							<tr>
								<td>
									<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.Keywords %>" />:
								</td>
								<td>
									<asp:TextBox CssClass="singleAuto" ID="txtSEKeywords" runat="Server"></asp:TextBox>
								</td>
							</tr>
							<tr>
								<td>
									<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.common.Description %>" />:
								</td>
								<td>
									<asp:TextBox CssClass="singleAuto" ID="txtSEDescription" runat="Server"></asp:TextBox>
								</td>
							</tr>
							<tr id="Tr3" runat="server">
								<td>
									<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.AltText %>" />:
								</td>
								<td>
									<asp:TextBox CssClass="singleAuto" ID="txtSEAlt" runat="Server"></asp:TextBox>
								</td>
							</tr>
						</table>
					</div>
				</div>
				<div class="tab-pane" id="products-tab">
					<asp:PlaceHolder runat="Server" ID="phProducts">
						<div class="admin-row">
							<table class="table table-detail">
								<tr style="padding-top: 10px;">
									<td>
										<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entity.Products %>" />:
									</td>
									<td>
										<asp:HyperLink runat="server"
											ID="ProductMappingLink" 
											NavigateUrl='<%# String.Format("entityproductmap.aspx?EntityType={0}&EntityId={1}", EntityName, EntityId) %>' />
									</td>
								</tr>
								<tr style="padding-top: 10px;">
									<td>
										<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.BulkProducts %>" />:
									</td>
									<td>
										<asp:LinkButton ID="lnkBulkDisplayOrder" runat="server" OnClick="lnkBulkDisplayOrder_Click">Display Order</asp:LinkButton>&nbsp;|&nbsp;
										<asp:LinkButton ID="lnkBulkInventory" runat="server" OnClick="lnkBulkInventory_Click">Inventory</asp:LinkButton>&nbsp;|&nbsp;
										<asp:LinkButton ID="lnkBulkSEFields" runat="server" OnClick="lnkBulkSEFields_Click">SE Fields</asp:LinkButton>&nbsp;|&nbsp;
										<asp:LinkButton ID="lnkBulkPrices" runat="server" OnClick="lnkBulkPrices_Click">Prices</asp:LinkButton>&nbsp;|&nbsp;
										<asp:LinkButton ID="lnkBulkDownloadFiles" runat="server" OnClick="lnkBulkDownloadFiles_Click">Download Files</asp:LinkButton>&nbsp;|&nbsp;
										<asp:LinkButton ID="lnkBulkShippingMethods" runat="server" OnClick="lnkBulkShippingMethods_Click">Shipping Costs</asp:LinkButton>
									</td>
								</tr>
								<tr>
									<td colspan="2">
										<iframe name="bulkFrame" id="bulkFrame" runat="server" frameborder="0" scrolling="auto" marginheight="0" marginwidth="0" height="1000" style="width: 100%;"></iframe>
									</td>
								</tr>
							</table>
						</div>
					</asp:PlaceHolder>
				</div>

				<asp:ValidationSummary ID="validationSummary" runat="Server" ValidationGroup="Main" DisplayMode="BulletList" ShowMessageBox="true" ShowSummary="false" />
				<script type="text/javascript">
					function DeleteImage(imgurl, name) {
						if(confirm('Are you sure you want to delete this image?'))
							window.open('deleteimage.aspx?imgurl=' + imgurl + '&FormImageName=' + name, 'Admin_ML', 'height=250,width=440,top=10,left=20,status=no,toolbar=no,menubar=no,scrollbars=yes,location=no');
					}
				</script>
			</div>
		</div>
		<div class="item-action-bar clearfix">
			<asp:HyperLink runat="server"
				ID="CancelLinkBottom"
				CssClass="btn btn-default"
				NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>"
				Text="<%$Tokens:StringResource, admin.common.close %>" />

			<asp:Button ID="btnCloseBottom" runat="server" CssClass="btn btn-default" OnClick="btnClose_Click" ValidationGroup="Main" Text="<%$Tokens:StringResource, admin.common.SaveAndClose %>" />
			<asp:Button ID="btnSubmitBottom" runat="server" OnClick="btnSubmit_Click" CssClass="btn btn-primary" ValidationGroup="Main" Text="<%$Tokens:StringResource, admin.common.Save %>" />
		</div>
	</asp:Panel>
</asp:Content>
