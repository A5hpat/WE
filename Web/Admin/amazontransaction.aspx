<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.amazontransaction" Title="<%$Tokens:StringResource, admin.title.amazontransaction %>" MasterPageFile="~/App_Templates/Admin_Default/Popup.master" Theme="Admin_Default" Codebehind="amazontransaction.aspx.cs" %>

<asp:Content runat="server" ContentPlaceHolderID="head">
	<script type="text/javascript" src="scripts/formValidate.js"></script>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<asp:Literal ID="ltContent" runat="server" />
</asp:Content>