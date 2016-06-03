<%@ Page Language="c#" Inherits="AspDotNetStorefrontAdmin.entityBulkPrices" Codebehind="entitybulkprices.aspx.cs" %>

<html>
<head runat="server">
	<title>Entity Bulk Prices</title>
</head>
<body>
	<form runat="server">
	<aspdnsf:AlertMessage runat="server" ID="AlertMessageDisplay" />
	<asp:Panel runat="server" ID="MainBody" DefaultButton="btnSubmit">
		<div class="item-action-bar">
			<asp:Button runat="server" ID="btnSubmit" CssClass="btn btn-primary btn-sm" Text="<%$Tokens:StringResource, admin.entityBulkPrices.PricesUpdate %>" />
		</div>
		<asp:Literal ID="ltBody" runat="server" />
		<div class="item-action-bar">
			<asp:Button runat="server" CssClass="btn btn-primary btn-sm" Text="<%$Tokens:StringResource, admin.entityBulkPrices.PricesUpdate %>" />
		</div>
	</asp:Panel>
	</form>
</body>
</html>
