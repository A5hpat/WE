<%@ Page Language="c#" Inherits="AspDotNetStorefrontAdmin.ShippingMethodCountries" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="shippingmethodcountries.aspx.cs" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<aspdnsf:ReturnUrlTracker runat="server" ID="ReturnUrlTracker" DefaultReturnUrl="shipping.aspx" />
	<h1>
		<i class="fa fa-anchor"></i>
		Allowed Shipping Countries
	</h1>

	<div class="item-action-bar">
		<asp:HyperLink runat="server" ID="btnCloseTop" CssClass="btn btn-default" NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>" Text="<%$Tokens:StringResource, admin.common.close %>" />
		<asp:Button runat="server" CssClass="btn btn-primary" Text="Save" OnClick="btnSave_Click" />
		<asp:Button runat="server" CssClass="btn btn-default" Text="Allow All" OnClick="btnAllowAll_Click" />
		<asp:Button runat="server" CssClass="btn btn-default" Text="Clear All" OnClick="btnClearAll_Click" />
	</div>

	<aspdnsf:AlertMessage runat="server" ID="AlertMessage" />

	<div class="white-ui-box">
		<p>Check the Countries that you want to <b>ALLOW</b> for this shipping method.<p>
		<asp:Literal ID="ltContent" runat="server" />
	</div>

	<div class="item-action-bar">
		<asp:HyperLink runat="server" ID="btnClose" CssClass="btn btn-default" NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>" Text="<%$Tokens:StringResource, admin.common.close %>" />
		<asp:Button runat="server" CssClass="btn btn-primary" Text="Save" OnClick="btnSave_Click" />
		<asp:Button runat="server" CssClass="btn btn-default" Text="Allow All" OnClick="btnAllowAll_Click" />
		<asp:Button runat="server" CssClass="btn btn-default" Text="Clear All" OnClick="btnClearAll_Click" />
	</div>
</asp:Content>