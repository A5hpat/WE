<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.importstringresourcefile1" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="importstringresourcefile1.aspx.cs" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<h1>
		<i class="fa fa-pencil-square-o"></i>
		<asp:Literal runat="server" ID="litStage" />
	</h1>
	<aspdnsf:AlertMessage runat="server" ID="ctlAlertMessage" />

	<asp:Panel runat="server" ID="pnlUpload" DefaultButton="btnSubmit">
		<div class="white-ui-box">
			<div class="white-box-heading">
				<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.stringresources.UploadFile %>" />
			</div>
			<p>
				<asp:Literal runat="server" ID="litSelectFileInstructions" />
			</p>
			<div class="form-group">
				*<asp:Label runat="server" AssociatedControlID="fuMain" Text="<%$ Tokens:StringResource, admin.stringresources.File %>" />
				<asp:FileUpload runat="server" ID="fuMain" CssClass="fileUpload" />
				<asp:RequiredFieldValidator runat="server" ControlToValidate="fuMain" ErrorMessage="Required" CssClass="text-danger" />
			</div>
			<div class="form-group">
				<asp:CheckBox runat="server" ID="chkReplaceExisting" Text="<%$ Tokens:StringResource, admin.stringresources.ReplaceExisting %>" />
				<asp:CheckBox runat="server" ID="chkLeaveModified" Text="<%$ Tokens:StringResource, admin.stringresources.LeaveModified %>" />
			</div>
		</div>

		<div class="item-action-bar">
			<asp:HyperLink runat="server" ID="lnkBack1" Text="<%$ Tokens:StringResource,admin.common.cancel %>" CssClass="btn btn-default" />
			<asp:Button runat="server" ID="btnSubmit" CssClass="btn btn-primary" Text="<%$Tokens:StringResource, admin.common.Upload %>" OnClick="btnSubmit_Click" />
		</div>
	</asp:Panel>

	<asp:Panel runat="server" ID="pnlReload" DefaultButton="btnReload">
		<div class="white-ui-box">
			<div class="white-box-heading">
				<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.stringresources.ReloadFromServer %>" />
			</div>
			<div class="form-group">
				<asp:CheckBox runat="server" ID="chkReloadReplaceExisting" Text="<%$ Tokens:StringResource, admin.stringresources.ReplaceExisting %>" />
				<asp:CheckBox runat="server" ID="chkReloadLeaveModified" Text="<%$ Tokens:StringResource, admin.stringresources.LeaveModified %>" />
			</div>
		</div>
		<div class="item-action-bar">
			<asp:HyperLink runat="server" ID="lnkBack2" Text="<%$Tokens:StringResource, admin.common.cancel%>" CssClass="btn btn-default" />
			<asp:Button runat="server" ID="btnReload" CssClass="btn btn-primary" Text="<%$Tokens:StringResource, admin.importstringresourcefile1.ReloadReview %>" OnClick="btnReload_Click" />
		</div>
	</asp:Panel>
</asp:Content>
