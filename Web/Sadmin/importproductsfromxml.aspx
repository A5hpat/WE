<%@ Page Language="C#" Inherits="AspDotNetStorefrontAdmin.importProductsFromXML" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="importproductsfromxml.aspx.cs" %>
<%@ Register TagPrefix="aspdnsf" TagName="LinkGroupImportExport" Src="Controls/LinkGroupImportExport.ascx" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">

	<h1>
		<i class="fa fa-cube"></i>
		<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.ProductImportExport %>" />
		<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.breadcrumbseparator %>" />
		<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.productloadfromxml %>" />
	</h1>
	
	<aspdnsf:AlertMessage runat="server" ID="AlertMessage" />

	<div class="alert alert-warning">
		<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.BackupDBBeforeImports %>" />
	</div>
	<div class="item-action-bar">
		<aspdnsf:LinkGroupImportExport runat="server" SelectedLink="importproductsfromxml.aspx" />
		<asp:Button ID="Button1" runat="server" Text="<%$Tokens:StringResource, admin.common.Import %>" OnClick="btnUpload_Click" CssClass="btn btn-primary" />
	</div>
	<div class="white-ui-box">
		<div>
			<div id="divMain" runat="server">
				<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.importProductsFromXML.XMLInfo %>" />
				<asp:FileUpload ID="fuFile" runat="server" CssClass="fileUpload" />
				<br />
				<br />
			</div>
		</div>
	</div>
	<div class="white-ui-box" runat="server" id="divReview">
		<div class="white-box-heading">
			<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.ReviewUpload %>" />
		</div>
		<div>
			<p>
				<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.ReviewLogStatus %>" />
			</p>
			<div class="form-group">
				<asp:Literal ID="ltResults" runat="server"></asp:Literal>
			</div>
			<div class="form-group">
				<asp:Button ID="btnAccept" runat="server" Text="<%$Tokens:StringResource, admin.common.ClickHereToAcceptImportUC %>" CssClass="btn btn-primary" OnClick="btnAccept_Click" />
				<asp:Button ID="btnUndo" runat="server" Text="<%$Tokens:StringResource, admin.common.ClickHereToUndoImportUC %>" CssClass="btn btn-danger" OnClick="btnUndo_Click" />
			</div>
		</div>
	</div>

	<div class="item-action-bar">
		<aspdnsf:LinkGroupImportExport runat="server" ID="LinkGroupImportExport" SelectedLink="importproductsfromxml.aspx" />
		<asp:Button ID="btnUpload" runat="server" Text="<%$Tokens:StringResource, admin.common.Import %>" OnClick="btnUpload_Click" CssClass="btn btn-primary" />
	</div>

</asp:Content>
