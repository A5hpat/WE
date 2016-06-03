<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.editfeed" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="feed.aspx.cs" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">

	<h1>
		<i class="fa fa-list-alt"></i>
		<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.feeds.ProductFeeds %>" />
	</h1>

	<aspdnsf:AlertMessage runat="server" ID="AlertMessage" />

	<div class="item-action-bar">
		<asp:Button ID="btnCancel_Top" runat="server" Text="<%$Tokens:StringResource, admin.common.cancel %>" CssClass="btn btn-default btn-sm" OnClick="btnCancel_OnClick" />
		<asp:Button ID="btnSubmit_Top" runat="server" Text="<%$Tokens:StringResource, admin.common.save %>" CssClass="btn btn-primary btn-sm" OnClick="btnSubmit_OnClick" />
		<asp:Button ID="btnExecFeed_Top" runat="server" Text="<%$Tokens:StringResource, admin.editfeed.buttonExecuteFeed %>" CssClass="btn btn-default btn-sm" OnClick="btnExecFeed_OnClick" />
	</div>

	<div id="white-ui-box">
		<table>
			<tr>
				<td>
					<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editfeed.store %>" />
				</td>
				<td>
					<asp:DropDownList ID="cboStore" runat="server" CausesValidation="true"></asp:DropDownList>
					<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$Tokens:StringResource, admin.editfeed.tooltip.imgStoreID %>">
						<i class="fa fa-question-circle fa-lg"></i>
					</asp:Label>
					<asp:CustomValidator ID="reqStore" runat="server" Display="Dynamic" OnServerValidate="ValidateStoreID" ErrorMessage="<%$Tokens:StringResource,admin.editfeed.msgselectstore %>"></asp:CustomValidator>
				</td>
			</tr>
			<tr>
				<td>
					<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editfeed.Feedname %>" />
				</td>
				<td>
					<asp:TextBox ID="txtFeedName" Columns="30" MaxLength="100" runat="server" CausesValidation="true"></asp:TextBox>
					<asp:RequiredFieldValidator ID="reqFeedName" ControlToValidate="txtFeedName" Display="Dynamic" EnableClientScript="true" ErrorMessage="<%$Tokens:StringResource,admin.editfeed.msgEnterFeedName %>" runat="server">!!</asp:RequiredFieldValidator>
				</td>
			</tr>
			<tr>
				<td>
					<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editfeed.XmlPackage %>" />
				</td>
				<td>
					<asp:DropDownList ID="XmlPackage" runat="server" CausesValidation="true">
						<asp:ListItem Text="Select a package" Value="" Selected="True"></asp:ListItem>
					</asp:DropDownList>
					<asp:CustomValidator ID="reqXmlPackage" runat="server" Display="Dynamic" OnServerValidate="ValidateXmlPackage" ErrorMessage="<%$Tokens:StringResource,admin.editfeed.tooltip.imgFtpUserName %>"></asp:CustomValidator>
				</td>
			</tr>
			<tr>
				<td>
					<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editfeed.AutoFTP %>" />
				</td>
				<td>
					<asp:RadioButtonList ID="CanAutoFtp" runat="server" RepeatDirection="Horizontal">
						<asp:ListItem Text="Yes" Value="1" Selected="True"></asp:ListItem>
						<asp:ListItem Text="No" Value="0"></asp:ListItem>
					</asp:RadioButtonList>
				</td>
			</tr>
			<tr>
				<td>
					<asp:Label ID="Label2" runat="server" Text="<%$Tokens:StringResource, admin.editfeed.FTPUsername %>" />
				</td>
				<td>
					<asp:TextBox ID="txtFtpUserName" Columns="30" MaxLength="100" runat="server"></asp:TextBox>
					<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$Tokens:StringResource, admin.editfeed.tooltip.imgFtpUserName %>">
						<i class="fa fa-question-circle fa-lg"></i>
					</asp:Label>
			</tr>
			<tr>
				<td>
					<asp:Label ID="Label3" runat="server" Text="<%$Tokens:StringResource, admin.editfeed.FTPPassword %>" />
				</td>
				<td>
					<asp:TextBox ID="txtFtpPwd" Columns="30" MaxLength="100" runat="server"></asp:TextBox>
					<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$Tokens:StringResource, admin.editfeed.tooltip.imgFtpPwd %>">
						<i class="fa fa-question-circle fa-lg"></i>
					</asp:Label>
			</tr>
			<tr>
				<td>
					<asp:Label ID="Label4" runat="server" Text="<%$Tokens:StringResource, admin.editfeed.FTPserver %>" />
				</td>
				<td>
					<asp:TextBox ID="txtFtpServer" Columns="30" MaxLength="100" runat="server" CausesValidation="true"></asp:TextBox>
					<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$Tokens:StringResource, admin.editfeed.tooltip.imgFtpServer %>">
						<i class="fa fa-question-circle fa-lg"></i>
					</asp:Label>
				</td>
			</tr>
			<tr>
				<td>
					<asp:Label ID="Label5" runat="server" Text="<%$Tokens:StringResource, admin.editfeed.FTPPort %>" />
				</td>
				<td>
					<asp:TextBox ID="txtFtpPort" Columns="30" MaxLength="5" Text="21" runat="server"></asp:TextBox>
					<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$Tokens:StringResource, admin.editfeed.tooltip.imgFtpPort %>">
						<i class="fa fa-question-circle fa-lg"></i>
					</asp:Label>
					<asp:CustomValidator ID="PortIsNumber" Display="Dynamic" OnServerValidate="ValidatePort" runat="server" ErrorMessage="<%$Tokens:StringResource,admin.editfeed.msgPortNumber %>"></asp:CustomValidator>
				</td>
			</tr>
			<tr>
				<td>
					<asp:Label ID="Label6" runat="server" Text="<%$Tokens:StringResource, admin.editfeed.FTPFilename %>" />
				</td>
				<td>
					<asp:TextBox ID="txtFtpFileName" Columns="30" MaxLength="1000" runat="server" CausesValidation="true"></asp:TextBox>
					<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$Tokens:StringResource, admin.editfeed.tooltip.imgFtpFileName %>">
						<i class="fa fa-question-circle fa-lg"></i>
					</asp:Label>
				</td>
			</tr>
		</table>
	</div>

	<div class="item-action-bar">
		<asp:Button ID="btnCancel_Bottom" runat="server" Text="<%$Tokens:StringResource, admin.common.cancel %>" CssClass="btn btn-default btn-sm" OnClick="btnCancel_OnClick" />
		<asp:Button ID="btnSubmit_Bottom" runat="server" Text="<%$Tokens:StringResource, admin.common.save %>" CssClass="btn btn-primary btn-sm" OnClick="btnSubmit_OnClick" />
		<asp:Button ID="btnExecFeed_Bottom" runat="server" Text="<%$Tokens:StringResource, admin.editfeed.buttonExecuteFeed %>" CssClass="btn btn-default btn-sm" OnClick="btnExecFeed_OnClick" />
	</div>

</asp:Content>
