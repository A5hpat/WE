<%@ Page Language="c#" Inherits="AspDotNetStorefrontAdmin.entityBulkInventory" Codebehind="entitybulkinventory.aspx.cs" %>

<html>
<head runat="server">
	<title>Entity Bulk Inventory</title>
	<script src="Scripts/jquery.min.js" type="text/javascript"></script>
	<script src="Scripts/bootstrap.min.js" type="text/javascript"></script>
</head>
<body>
	<form runat="server">
		<aspdnsf:AlertMessage runat="server" ID="AlertMessageDisplay" />
		<asp:Panel runat="server" ID="MainBody" DefaultButton="btnSubmit">
			<div class="item-action-bar">
				<asp:Button ID="btnSubmit" Text="Save Inventory" CssClass="btn btn-primary btn-sm" runat="server" OnClick="btnSubmit_Click" />
			</div>

			<asp:Literal ID="ltBody" runat="server" />
		
			<div class="item-action-bar">
				<asp:Button Text="Save Inventory" CssClass="btn btn-primary btn-sm" runat="server" OnClick="btnSubmit_Click" />
			</div>
		</asp:Panel>
	</form>
</body>
</html>