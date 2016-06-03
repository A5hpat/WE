<%@ Page Language="C#" AutoEventWireup="true" Inherits="Admin_AvalaraConnectionTest" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="avalaraconnectiontest.aspx.cs" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<aspdnsf:ReturnUrlTracker runat="server" ID="ReturnUrlTracker" DefaultReturnUrl="AppConfigs.aspx" />

	<div class="item-action-bar">
		<asp:HyperLink runat="server"
			ID="btnCloseTop" 
			CssClass="btn btn-default"
			NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>"
			Text="Back to Settings" />
	</div>

	<aspdnsf:AlertMessage runat="server" ID="ctlAlertMessage" />
</asp:Content>