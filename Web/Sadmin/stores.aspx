<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.Stores" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="stores.aspx.cs" %>
<%@ Register TagPrefix="aspdnsf" TagName="StoreControl" Src="controls/StoreControl.ascx" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<h1>
		<i class="fa fa-building-o"></i>
		<asp:Label ID="lblHeader" runat="server" Text="<%$Tokens:StringResource, admin.title.stores %>" />
	</h1>
	<aspdnsf:StoreControl ID="scMain" runat="server" />
</asp:Content>
