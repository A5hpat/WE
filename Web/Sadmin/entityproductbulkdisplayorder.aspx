<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.EntityProductBulkDisplayOrder" Theme="Admin_Default" Codebehind="entityproductbulkdisplayorder.aspx.cs" %>

<html>
<head runat="server">
	<title>Entity Product Bulk Display Order</title>
	<script src="Scripts/jquery.min.js" type="text/javascript"></script>
	<script src="Scripts/bootstrap.min.js" type="text/javascript"></script>
</head>

<body>
	<form runat="server">

	<aspdnsf:AlertMessage runat="server" ID="AlertMessage" />

	<asp:Panel runat="server" Visible='<%#EntityCount > 0 %>' DefaultButton="btnSubmit">
		<div class="item-action-bar">
			<asp:Button ID="btnSubmit" runat="server" Text="Save Display Order" CssClass="btn btn-primary btn-sm" OnClick="btnSubmit_Click" />
		</div>

		<asp:Literal ID="RenderContainer" runat="server" />

		<div class="item-action-bar">
			<asp:Button runat="server" Text="Save Display Order" CssClass="btn btn-primary btn-sm" OnClick="btnSubmit_Click" />
		</div>
	</asp:Panel>
	</form>
</body>
</html>
