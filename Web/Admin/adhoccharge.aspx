<%@ Page Language="c#" Inherits="AspDotNetStorefrontAdmin.adhoccharge" Title="<%$Tokens:StringResource, admin.title.adhoccharge %>" MasterPageFile="~/App_Templates/Admin_Default/Popup.master" Theme="Admin_Default" Codebehind="adhoccharge.aspx.cs" AutoEventWireup="True" %>
<asp:Content runat="server" ContentPlaceHolderID="head">
	<script type="text/javascript" src="scripts/formValidate.js"></script>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<asp:Literal ID="ltContent" runat="server" />
</asp:Content>