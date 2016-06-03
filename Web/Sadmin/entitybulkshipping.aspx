<%@ Page Language="c#" Inherits="AspDotNetStorefrontAdmin.entityBulkShipping" MasterPageFile="~/App_Templates/Admin_Default/Popup.master" Theme="Admin_Default" Codebehind="entitybulkshipping.aspx.cs" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<aspdnsf:AlertMessage runat="server" ID="AlertMessageDisplay" />
	<asp:Panel runat="server" ID="MainBody" DefaultButton="btnSubmit">
		<div class="item-action-bar">
			<asp:Button runat="server" ID="btnSubmit" CssClass="btn btn-primary btn-sm" Text="<%$Tokens:StringResource, admin.entityBulkShipping.ShippingUpdate %>" />
		</div>
		<asp:Literal ID="ltBody" runat="server" />
		<div class="item-action-bar">
			<asp:Button runat="server" CssClass="btn btn-primary btn-sm" Text="<%$Tokens:StringResource, admin.entityBulkShipping.ShippingUpdate %>" />
		</div>
	</asp:Panel>
</asp:Content>
