<%@ Page Language="C#" Inherits="AspDotNetStorefrontAdmin.exportProductPricing" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="exportproductpricing.aspx.cs" %>
<%@ Register TagPrefix="aspdnsf" TagName="LinkGroupImportExport" Src="Controls/LinkGroupImportExport.ascx" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<h1>
		<i class="fa fa-tag"></i>
		<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.ProductImportExport %>" />
		<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.breadcrumbseparator %>" />
		<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.priceexport %>" />
	</h1>
	<div class="item-action-bar">
		<aspdnsf:LinkGroupImportExport runat="server" SelectedLink="exportproductpricing.aspx" />
		<asp:Button runat="server" CssClass="btn btn-primary" Text="<%$Tokens:StringResource, admin.common.Export %>" OnClick="btnUpload_Click" />
	</div>
	<div class="white-ui-box">
		<div class="white-box-heading">
			<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.exportProductPricing.ExportPrices %>" />:
		</div>
		<div class="row">
			<div class="col-md-4">
				<div class="form-group">
					<asp:Label runat="server" AssociatedControlID="ddCategory" Text="<%$Tokens:StringResource, admin.exportProductPricing.FilterCategory %>" />:
					<asp:DropDownList ID="ddCategory" CssClass="form-control" runat="server" />
				</div>
				<div class="form-group">
					<asp:Label runat="server" AssociatedControlID="ddSection" Text="<%$Tokens:StringResource, admin.exportProductPricing.FilterSection %>" />:
					<asp:DropDownList ID="ddSection" runat="server" CssClass="form-control" />
				</div>
				<div class="form-group">
					<asp:Label runat="server" AssociatedControlID="ddManufacturer" Text="<%$Tokens:StringResource, admin.exportProductPricing.FilterManufacturer %>" />:
					<asp:DropDownList ID="ddManufacturer" runat="server" CssClass="form-control" />
				</div>
				<div class="form-group">
					<asp:Panel id="pnlDistributor" runat="server">
						<asp:Label runat="server" AssociatedControlID="ddDistributor" Text="<%$Tokens:StringResource, admin.exportProductPricing.FilterDistributor %>" />:
						<asp:DropDownList ID="ddDistributor" runat="server" CssClass="form-control" />
					</asp:Panel>
				</div>
				<div class="form-group">
					<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.exportProductPricing.SelectFormat %>" />:
					<asp:RadioButtonList ID="rblExport" runat="server">
						<asp:ListItem Value="xls" Text="<%$Tokens:StringResource, admin.exportProductPricing.Excel %>" Selected="true"></asp:ListItem>
						<asp:ListItem Value="xml" Text="<%$Tokens:StringResource, admin.exportProductPricing.XML %>"></asp:ListItem>
						<asp:ListItem Value="csv" Text="<%$Tokens:StringResource, admin.exportProductPricing.CSV %>"></asp:ListItem>
					</asp:RadioButtonList>
				</div>
			</div>
		</div>
	</div>
	<div class="item-action-bar">
		<aspdnsf:LinkGroupImportExport runat="server" ID="LinkGroupImportExport" SelectedLink="exportproductpricing.aspx" />
		<asp:Button ID="btnUpload" runat="server" CssClass="btn btn-primary" Text="<%$Tokens:StringResource, admin.common.Export %>" OnClick="btnUpload_Click" />
	</div>

</asp:Content>
