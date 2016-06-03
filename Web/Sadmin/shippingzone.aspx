<%@ Page Language="c#" Inherits="AspDotNetStorefrontAdmin.shippingzone"
	MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="shippingzone.aspx.cs" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<aspdnsf:ReturnUrlTracker runat="server" ID="ReturnUrlTracker" DefaultReturnUrl="shipping.aspx" />
	<div class="admin-module">
		<h1>
			<i class="fa fa-anchor"></i>
			<asp:Literal ID="litHeader" runat="server" />
		</h1>
		<div>
			<aspdnsf:AlertMessage runat="server" ID="ctrlAlertMessage" />
		</div>
		<div class="item-action-bar">
			<asp:HyperLink runat="server" ID="btnCloseTop" CssClass="btn btn-default" NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>" Text="<%$Tokens:StringResource, admin.common.close %>" />
			<asp:Button ID="btnSaveAndCloseTop" runat="server" CssClass="btn btn-default" OnClick="btnSaveAndClose_Click" ValidationGroup="Main" Text="<%$Tokens:StringResource, admin.common.SaveAndClose %>" />
			<asp:Button ID="btnSubmitTop" runat="server" CssClass="btn btn-primary" OnClick="btnSubmit_Click" ValidationGroup="Main" Text="<%$Tokens:StringResource, admin.common.Save %>" />
		</div>

		<div class="admin-row">
			<div id="divEditZone" class="white-ui-box">
				<div class="row">
					<div class="col-md-4">
						<div class="form-inline" runat="server" id="divZoneId">
							<asp:Label AssociatedControlID="litZoneId" for="litZoneId" runat="server" Text="<%$Tokens:StringResource, admin.common.ID %>" />:
							<asp:Literal ID="litZoneId" runat="server" />
						</div>
						<div class="form-group">
							<asp:Label AssociatedControlID="txtZoneName" for="txtZoneName" runat="server" Text="<%$Tokens:StringResource, admin.editshippingzone.ZoneName %>" />:
							<asp:TextBox ID="txtZoneName" CssClass="form-control" runat="server" />
							<asp:RequiredFieldValidator ErrorMessage="Zone Name Is Required" CssClass="text-danger" ControlToValidate="txtZoneName" Display="Dynamic" ID="rfvZoneName" ValidationGroup="Main" SetFocusOnError="true" runat="server" />
						</div>
						<div class="form-group">
							<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.editshippingzone.EnterTarget %>">
								<i class="fa fa-question-circle fa-lg"></i>
							</asp:Label>
							<asp:Label AssociatedControlID="txtZipCodes" for="txtZipCodes" runat="server" Text="<%$Tokens:StringResource, admin.editshippingzone.ZipCodes %>" />:
							<asp:TextBox TextMode="MultiLine" Rows="2" ID="txtZipCodes" CssClass="form-control" runat="server" />
							<asp:RequiredFieldValidator ErrorMessage="Zip Codes Is Required" CssClass="text-danger" ControlToValidate="txtZipCodes" Display="Dynamic" ID="rfvZipCodes" ValidationGroup="Main" SetFocusOnError="true" runat="server" />
						</div>
					</div>
				</div>
			</div>
			<div class="item-action-bar">
				<asp:HyperLink runat="server" ID="btnClose" CssClass="btn btn-default" NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>" Text="<%$Tokens:StringResource, admin.common.close %>" />
				<asp:Button ID="btnSaveAndClose" runat="server" CssClass="btn btn-default" OnClick="btnSaveAndClose_Click" ValidationGroup="Main" Text="<%$Tokens:StringResource, admin.common.SaveAndClose %>" />
				<asp:Button ID="btnSubmit" runat="server" CssClass="btn btn-primary" OnClick="btnSubmit_Click" ValidationGroup="Main" Text="<%$Tokens:StringResource, admin.common.Save %>" />
			</div>
		</div>
	</div>
	<asp:Literal ID="ltContent" runat="server" />
</asp:Content>
