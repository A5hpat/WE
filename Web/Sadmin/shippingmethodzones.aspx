<%@ Page Language="c#" Inherits="AspDotNetStorefrontAdmin.ShippingMethodZones" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="shippingmethodzones.aspx.cs" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<aspdnsf:ReturnUrlTracker runat="server" ID="ReturnUrlTracker" DefaultReturnUrl="shipping.aspx" />
	<h1>
		<i class="fa fa-anchor"></i>
		Allowed Shipping Zones
	</h1>

	<div class="item-action-bar">
		<asp:HyperLink runat="server" ID="btnCloseTop" CssClass="btn btn-default" NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>" Text="<%$Tokens:StringResource, admin.common.close %>" />
		<asp:Button runat="server" CssClass="btn btn-primary btn-sm" Text="Save" OnClick="btnSave_Click" />
		<asp:Button runat="server" CssClass="btn btn-default btn-sm" Text="Allow All" OnClick="btnAllowAll_Click" />
		<asp:Button runat="server" CssClass="btn btn-default btn-sm" Text="Clear All" OnClick="btnClearAll_Click" />
	</div>

	<aspdnsf:AlertMessage runat="server" ID="AlertMessage" />

	<div class="white-ui-box">
		<p>Check the Shipping Zones that you want to <b>ALLOW</b> for this shipping method.<p>
		<asp:Literal ID="ltContent" runat="server" />
	</div>

	<div class="item-action-bar">
		<asp:HyperLink runat="server" ID="btnClose" CssClass="btn btn-default" NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>" Text="<%$Tokens:StringResource, admin.common.close %>" />
		<asp:Button runat="server" CssClass="btn btn-primary btn-sm" Text="Save" OnClick="btnSave_Click" />
		<asp:Button runat="server" CssClass="btn btn-default btn-sm" Text="Allow All" OnClick="btnAllowAll_Click" />
		<asp:Button runat="server" CssClass="btn btn-default btn-sm" Text="Clear All" OnClick="btnClearAll_Click" />
	</div>
</asp:Content>

