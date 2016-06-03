<%@ Page Language="c#" Inherits="AspDotNetStorefrontAdmin.paypalreauthorder" MasterPageFile="~/App_Templates/Admin_Default/Popup.master" Theme="Admin_Default" Title="<%$Tokens:StringResource, admin.title.paypalreauthorder %>" Codebehind="paypalreauthorder.aspx.cs" %>

<asp:Content runat="server" ContentPlaceHolderID="head">
	<script type="text/javascript" src="scripts/formValidate.js"></script>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<asp:Literal ID="ltContent" runat="server" />
</asp:Content>
