<%@ Page Language="C#" Inherits="AspDotNetStorefrontAdmin.importProductsFromExcel" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="importproductsfromexcel.aspx.cs" %>
<%@ Register TagPrefix="aspdnsf" TagName="LinkGroupImportExport" Src="Controls/LinkGroupImportExport.ascx" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<div id="container">
		<h1>
			<i class="fa fa-cube"></i>
			<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.ProductImportExport %>" />
			<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.breadcrumbseparator %>" />
			<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.productloadfromexcel %>" />
		</h1>
		<aspdnsf:AlertMessage runat="server" ID="AlertMessage" />
		<div class="alert alert-warning">
			<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.BackupDBBeforeImports %>" />
		</div>
		<div class="item-action-bar">
			<aspdnsf:LinkGroupImportExport runat="server" ID="LinkGroupImportExport" SelectedLink="importproductsfromexcel.aspx" />
			<asp:Button runat="server" Text="<%$Tokens:StringResource, admin.common.Import %>" OnClick="btnUpload_Click" CssClass="btn btn-primary" />
		</div>
		<div class="white-ui-box">
			<div id="divMain" runat="server" class="form-group">
				<p>
					<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.importProductsFromExcel.ExcelInfo %>" />
				</p>
				<asp:FileUpload ID="fuFile" runat="server" CssClass="fileUpload" />
			</div>
		</div>

		<div runat="server" id="divReview" class="white-ui-box">
			<div class="white-box-heading">
				<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.ReviewUpload %>" />
			</div>

			<div class="wrapper">
				<p><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.ReviewLogStatus %>" /></p>
				<p><asp:Literal ID="ltResults" runat="server"></asp:Literal></p>
				<div class="form-group">
					<asp:Button ID="btnAccept" runat="server" Text="<%$Tokens:StringResource, admin.common.ClickHereToAcceptImportUC %>" CssClass="btn btn-primary" OnClick="btnAccept_Click" />
					<asp:Button ID="btnUndo" runat="server" Text="<%$Tokens:StringResource, admin.common.ClickHereToUndoImportUC %>" CssClass="btn btn-danger" OnClick="btnUndo_Click" />
				</div>
			</div>

		</div>

		<div class="item-action-bar">
			<aspdnsf:LinkGroupImportExport runat="server" SelectedLink="importproductsfromexcel.aspx" />
			<asp:Button ID="btnUpload" runat="server" Text="<%$Tokens:StringResource, admin.common.Import %>" OnClick="btnUpload_Click" CssClass="btn btn-primary" />
		</div>		
	</div>
</asp:Content>
