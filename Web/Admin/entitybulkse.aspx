<%@ Page Language="c#" Inherits="AspDotNetStorefrontAdmin.entityBulkSE" Codebehind="entitybulkse.aspx.cs" %>

<html>
<head runat="server">
	<title>Entity Bulk SE</title>
	<script src="Scripts/jquery.min.js" type="text/javascript"></script>
	<script src="Scripts/bootstrap.min.js" type="text/javascript"></script>
</head>
<body>
	<form runat="server">
		<aspdnsf:AlertMessage runat="server" ID="AlertMessageDisplay" />
		<asp:Panel runat="server" ID="MainBody" DefaultButton="btnSubmit">
			<div class="item-action-bar">
				<asp:Button runat="server" ID="btnSubmit" CssClass="btn btn-primary btn-sm" Text="<%$Tokens:StringResource, admin.entityBulkSE.SearchEngineUpdate %>" />
			</div>
			<asp:Literal ID="ltBody" runat="server" />
			<div class="item-action-bar">
				<asp:Button runat="server" CssClass="btn btn-primary btn-sm" Text="<%$Tokens:StringResource, admin.entityBulkSE.SearchEngineUpdate %>" />
			</div>
		</asp:Panel>
	</form>
</body>
</html>