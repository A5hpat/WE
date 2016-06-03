<%@ Page Language="C#" Inherits="AspDotNetStorefrontAdmin.importProductPricing" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="importproductpricing.aspx.cs" %>
<%@ Register TagPrefix="aspdnsf" TagName="LinkGroupImportExport" Src="Controls/LinkGroupImportExport.ascx" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<h1>
		<i class="fa fa-tag"></i>
		<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.ProductImportExport %>" />
		<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.breadcrumbseparator %>" />
		<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.priceimport %>" />
	</h1>
	<aspdnsf:AlertMessage runat="server" ID="AlertMessage" />
	
	<asp:Panel runat="server" DefaultButton="btnUpload">
		<div class="item-action-bar">
			<aspdnsf:LinkGroupImportExport runat="server" ID="LinkGroupImportExport1" SelectedLink="importproductpricing.aspx" />
			<asp:Button runat="server" Text="<%$Tokens:StringResource, admin.common.Import %>" OnClick="btnUpload_Click" CssClass="btn btn-primary" />
		</div>
		<div class="white-ui-box">
			<div id="divMain" runat="server">
				<div class="form-group">
					<span><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.importProductPricing.FileToImport %>" />:</span>
					<asp:FileUpload CssClass="fileUpload" ID="fuFile" runat="server" />
				</div>
			</div>
		</div>
		<div class="item-action-bar">
			<aspdnsf:LinkGroupImportExport runat="server" ID="LinkGroupImportExport" SelectedLink="importproductpricing.aspx" />
			<asp:Button ID="btnUpload" runat="server" Text="<%$Tokens:StringResource, admin.common.Import %>" OnClick="btnUpload_Click" CssClass="btn btn-primary" />
		</div>
	</asp:Panel>

</asp:Content>
