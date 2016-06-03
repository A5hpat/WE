<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.OrderOption" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="orderoption.aspx.cs" %>

<%@ Register TagPrefix="aspdnsf" TagName="EntityToStore" Src="Controls/EntityToStoreMapper.ascx" %>
<%@ Register TagPrefix="telerik" Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<aspdnsf:ReturnUrlTracker runat="server" ID="ReturnUrlTracker" DefaultReturnUrl="orderoptions.aspx" />
	<div class="admin-module">
		<h1>
			<i class="fa fa-list"></i>
			<asp:Literal ID="litHeader" runat="server" />
		</h1>
		<aspdnsf:AlertMessage ID="ctlAlertMessage" runat="server" />
		<div class="item-action-bar">
			<div class="other-actions">
				<asp:Panel ID="pnlLocale" runat="server" Visible='<%# LocaleSelector.HasMultipleLocales() %>'>
					<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.stringresources.Locale %>" AssociatedControlID="LocaleSelector" />
					<aspdnsf:LocaleSelector ID="LocaleSelector" runat="server" OnSelectedLocaleChanged="LocaleSelector_SelectedLocaleChanged" />
				</asp:Panel>
			</div>
			<asp:HyperLink ID="btnCloseTop" runat="server" CssClass="btn btn-default" NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>" Text="<%$Tokens:StringResource, admin.common.close %>" />
			<asp:Button ID="btnSaveAndCloseTop" runat="server" CssClass="btn btn-default" OnClick="btnSaveAndClose_Click" ValidationGroup="gAdd" Text="<%$Tokens:StringResource, admin.common.SaveAndClose %>" />
			<asp:Button ID="btnSubmitTop" runat="server" CssClass="btn btn-primary" OnClick="btnSubmit_Click" ValidationGroup="gAdd" Text="<%$Tokens:StringResource, admin.common.Save %>" />
		</div>
		<div class="admin-row">
			<div id="divEditOrderOption" class="white-ui-box">
				<div class="row">
					<div class="col-md-4">
						<div class="form-inline">
							<asp:Label AssociatedControlID="litOrderOptionId" for="litOrderOptionId" runat="server" Text="<%$Tokens:StringResource, admin.common.ID %>" />:
							<asp:Literal ID="litOrderOptionId" runat="server" />
						</div>
						<div class="form-group">
							<asp:Label AssociatedControlID="txtName" for="txtName" runat="server" Text="<%$Tokens:StringResource, admin.orderOption.OptionName %>" />:
							<asp:TextBox ID="txtName" CssClass="form-control" runat="server" />
							<asp:RequiredFieldValidator Display="Dynamic" CssClass="text-danger" ErrorMessage="Required!" ControlToValidate="txtName" ID="RequiredFieldValidator9" ValidationGroup="gAdd" SetFocusOnError="true" runat="server" />
						</div>
						<div class="form-group">
							<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.common.Description %>" />
							<asp:TextBox ID="txtDescription" runat="server" Visible="false" TextMode="MultiLine" Rows="10" />
							<telerik:RadEditor id="radCopy" runat="server" SkinID="RadEditorSettings" />
						</div>
						<div class="form-group">
							<span class="text-danger">*</span><asp:Label AssociatedControlID="txtCost" for="txtCost" runat="server" Text="<%$Tokens:StringResource, admin.common.Cost %>" />:
							<asp:TextBox ID="txtCost" CssClass="form-control" runat="server" />
							<asp:RequiredFieldValidator Display="Dynamic" ErrorMessage="Required!" CssClass="text-danger" ControlToValidate="txtCost" ID="RequiredFieldValidator1" ValidationGroup="gAdd" SetFocusOnError="true" runat="server" />
							<asp:CompareValidator Display="Dynamic" runat="server" CssClass="text-danger" ValidationGroup="gAdd" Operator="DataTypeCheck" Type="Double" ErrorMessage="Please enter a decimal value!" ControlToValidate="txtCost" />
						</div>
						<div class="form-group">
							<asp:Label AssociatedControlID="ddlTaxClass" for="txtCost" runat="server" Text="<%$Tokens:StringResource, admin.orderOption.TaxClass %>" />:
							<asp:DropDownList ID="ddlTaxClass" CssClass="form-control" runat="server" />
						</div>
						<div class="form-group">
							<span class="text-danger">*</span><asp:Label AssociatedControlID="txtDisplayOrder" for="txtDisplayOrder" runat="server" Text="<%$Tokens:StringResource, admin.Common.DisplayOrder %>" />
							<asp:TextBox ID="txtDisplayOrder" CssClass="form-control" runat="server" />
							<asp:RequiredFieldValidator Display="Dynamic" CssClass="text-danger" ErrorMessage="Fill in Display Order!" ControlToValidate="txtDisplayOrder" ID="RequiredFieldValidator2" ValidationGroup="gAdd" SetFocusOnError="true" runat="server" />
						</div>
						<div runat="server" id="divStoreMapping" class="form-group">
							<span class="text-danger">*</span><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.topic.mapstores %>" />
							<aspdnsf:EntityToStore runat="server" ID="StoresMapping" EntityType="OrderOption" ShowText="false" />
						</div>
						<div class="form-group">
							<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.Icon %>" />:
							<asp:FileUpload ID="fuIcon" runat="Server" />
							<div>
								<asp:Literal ID="ltIcon" runat="server" />
							</div>
						</div>
					</div>
				</div>
			</div>
			<div class="item-action-bar">
				<asp:HyperLink ID="btnClose" runat="server" CssClass="btn btn-default" NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>" Text="<%$Tokens:StringResource, admin.common.close %>" />
				<asp:Button ID="btnSaveAndClose" runat="server" CssClass="btn btn-default" OnClick="btnSaveAndClose_Click" ValidationGroup="gAdd" Text="<%$Tokens:StringResource, admin.common.SaveAndClose %>" />
				<asp:Button ID="btnSubmit" runat="server" CssClass="btn btn-primary" OnClick="btnSubmit_Click" ValidationGroup="gAdd" Text="<%$Tokens:StringResource, admin.common.Save %>" />
			</div>
		</div>
	</div>
</asp:Content>