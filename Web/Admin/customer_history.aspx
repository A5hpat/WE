<%@ Page Language="C#" Inherits="AspDotNetStorefrontAdmin.customer_history" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="customer_history.aspx.cs" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<h1>
		<i class="fa fa-user"></i>
		<asp:Literal ID="customerHistoryTitle" runat="server" Text="<%$ Tokens:StringResource, admin.title.cst_history %>" />
	</h1>
	<div class="white-ui-box customer-history">
		<asp:Literal ID="ltContent" runat="server" />
	</div>
</asp:Content>
