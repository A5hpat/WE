<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin._RTShippingLocalPickup" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="rtshippinglocalpickup.aspx.cs" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<aspdnsf:ReturnUrlTracker runat="server" ID="ReturnUrlTracker" DefaultReturnUrl="shipping.aspx" />
	<h1>
		<i class="fa fa-anchor"></i>
		<asp:Label ID="lblrtshippinglocalpickupbreadcrumb" runat="server" />
	</h1>
	<div>
		<aspdnsf:AlertMessage runat="server" ID="ctrlAlertMessage" />
	</div>
	<div class="item-action-bar">
		<div class="col-list-action-bar">
			<asp:HyperLink runat="server" ID="btnCloseTop" CssClass="btn btn-default" NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>" Text="<%$Tokens:StringResource, admin.common.close %>" />
			<asp:Button ID="btnUpdateTop" runat="server" OnClick="btnUpdate_Click" CssClass="btn btn-primary" />
		</div>
	</div>
	<asp:Panel ID="pnlLocale" runat="server">
		<div class="item-action-bar clearfix">
			<div class="other-actions">
				<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.stringresources.Locale %>" AssociatedControlID="ddlLocale" />
				<asp:DropDownList ID="ddlLocale" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlLocale_SelectedIndexChanged" />
			</div>
		</div>
	</asp:Panel>
	<div class="white-ui-box">
		<div class="white-box-heading">
			<asp:Label ID="lblTitle" runat="server" Text="Label" />
		</div>
		<div class="row form-group">
			<div class="col-sm-3">
				<asp:Label ID="lblAllowLocalPickup" AssociatedControlID="lblTitle" runat="server" />
				<asp:Label runat="server" ID="imgAllowLocalPickup" CssClass="hover-help" data-toggle="tooltip">
					<i class="fa fa-question-circle fa-lg"></i>
				</asp:Label>
			</div>
			<div class="col-sm-6">
				<asp:CheckBox ID="cbxAllowLocalPickup" runat="server" />
			</div>
		</div>
		<div class="row form-group">
			<div class="col-sm-3">
				<asp:Label ID="lblRTShippingLocalPickupHandlingFee" runat="server" Text="Handling fee for local pickup" />
				<asp:Label runat="server" ID="imgRTShippingLocalPickupHandlingFee" CssClass="hover-help" data-toggle="tooltip">
					<i class="fa fa-question-circle fa-lg"></i>
				</asp:Label>
			</div>
			<div class="col-sm-6">
				<asp:TextBox ID="txtRTShippingLocalPickupHandlingFee" runat="server" />
			</div>
		</div>
	</div>
	<div class="white-ui-box">
		<div class="white-box-heading">
			<asp:Label ID="lblRestrictionsTitle" runat="server" Text="Label" />
		</div>
		<div class="row form-group">
			<div class="col-sm-3">
				<asp:Label ID="lblrestrictiontype" runat="server" />
				<asp:Label runat="server" ID="imgRestrictionType" CssClass="hover-help" data-toggle="tooltip">
					<i class="fa fa-question-circle fa-lg"></i>
				</asp:Label>
			</div>
			<div class="col-sm-6">
				<asp:RadioButtonList ID="rblRestrictionType" runat="server" AutoPostBack="true">
					<asp:ListItem ID="liUnrestricted" runat="server" Value="1" Text="Unrestricted" />
					<asp:ListItem ID="liState" runat="server" Value="2" Text="State" />
					<asp:ListItem ID="liZip" runat="server" Value="3" Text="Zip" />
					<asp:ListItem ID="liZone" runat="server" Value="4" Text="Zone" />
				</asp:RadioButtonList>
			</div>
		</div>
		<asp:Panel ID="pnlStateSelectContainer" Visible="false" runat="server">
			<div class="row form-group">
				<div class="col-sm-3">
					<asp:Label ID="lblRestrictionAllowedStates" runat="server" />
					<asp:Label runat="server" ID="imgRestrictionAllowedStates" CssClass="hover-help" data-toggle="tooltip">
						<i class="fa fa-question-circle fa-lg"></i>
					</asp:Label>
				</div>
				<div class="col-sm-6">
					<asp:Panel ID="pnlStateSelect" Visible="false" runat="server" >
						<asp:Literal ID="litNoStates" Visible="false" runat="server" Text='You do not have any states configured.  Please <a href="states.aspx">set some up</a> and then come back to this page.' />
					</asp:Panel>
				</div>
			</div>
		</asp:Panel>
		<asp:Panel ID="pnlZipSelect" Visible="false" runat="server">
			<div class="row form-group">
				<div class="col-sm-3">
					<asp:Label ID="lblRestrictionAllowedZips" runat="server" />
					<asp:Label runat="server" ID="imgRestrictionAllowedZips" CssClass="hover-help" data-toggle="tooltip">
						<i class="fa fa-question-circle fa-lg"></i>
					</asp:Label>
				</div>
				<div class="col-sm-6">
					<asp:TextBox ID="txtRestrictionAllowedZips" runat="server" />
				</div>
			</div>
		</asp:Panel>
		<asp:Panel ID="pnlZoneSelectContainer" Visible="false" runat="server">
			<div class="row form-group">
				<div class="col-sm-3">
					<asp:Label ID="lblRestrictionAllowedZones" runat="server" />
					<asp:Label runat="server" ID="imgRestrictionAllowedZones" CssClass="hover-help" data-toggle="tooltip">
						<i class="fa fa-question-circle fa-lg"></i>
					</asp:Label>
				</div>
				<div class="col-sm-6">
					<asp:Panel ID="pnlZoneSelect" Visible="false" runat="server">
						<asp:Literal ID="litNoZones" Visible="false" runat="server" Text='You do not have any zones configured.  Please <a href="shippingzones.aspx">set some up</a> and then come back to this page.' />
					</asp:Panel>
				</div>
			</div>
		</asp:Panel>
	</div>
	<div class="item-action-bar">
		<div class="col-list-action-bar">
			<asp:HyperLink runat="server" ID="btnClose" CssClass="btn btn-default" NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>" Text="<%$Tokens:StringResource, admin.common.close %>" />
			<asp:Button ID="btnUpdate" runat="server" OnClick="btnUpdate_Click" CssClass="btn btn-primary" />
		</div>
	</div>
</asp:Content>