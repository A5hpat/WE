<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.SystemLog" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="systemlog.aspx.cs" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<div class="admin-module">
		<aspdnsf:AlertMessage ID="ctlAlertMessage" runat="server" />
		<h1>
			<i class="fa fa-exclamation-triangle"></i>
			<asp:Literal ID="litHeader" runat="server" Text="<%$ Tokens:StringResource, admin.title.systemlog %>" />
		</h1>

		<div class="admin-row">
			<div class="white-ui-box">
				<div class="row">
					<div class="col-md-12">
						<div class="form-inline">
							<asp:Label AssociatedControlID="litLogId" for="litLogId" runat="server" Text="<%$Tokens:StringResource, admin.common.ID %>" />:
							<asp:Literal ID="litLogId" runat="server" />
						</div>
						<div class="form-inline">
							<asp:Label AssociatedControlID="litLogCreatedOn" for="litLogCreatedOn" runat="server" Text="<%$Tokens:StringResource, admin.systemlog.aspx.6 %>" />:
							<asp:Literal ID="litLogCreatedOn" runat="server" />
						</div>
						<div>
							<asp:Label AssociatedControlID="litLogMessage" for="litLogMessage" runat="server" Text="<%$ Tokens:StringResource, admin.systemlog.Message %>" />
						</div>
						<div style="margin-left:20px;">
							<asp:Literal ID="litLogMessage" runat="server" />
						</div>
						<div>
							<asp:Label AssociatedControlID="litLogDetails" for="litLogDetails" runat="server" Text="<%$Tokens:StringResource, admin.systemlog.InnerException %>" />
						</div>
						<div style="margin-left:20px;">
							<asp:Literal ID="litLogDetails" runat="server" />
						</div>
					</div>
			</div>
		</div>

		<div class="item-action-bar">
			<a href="systemlogs.aspx" class="btn btn-default btn-sm">
				<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.Back %>" />
			</a>
		</div>
	</div>
	</div>
</asp:Content>
