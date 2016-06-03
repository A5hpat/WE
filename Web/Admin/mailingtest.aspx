<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.MailingTest" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="mailingtest.aspx.cs" %>

<%@ Register TagPrefix="aspdnsf" TagName="StoreSelector" Src="controls/StoreSelector.ascx" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="AjaxToolkit" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">	
	<h1>
		<i class="fa fa-envelope"></i>
		<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.title.mailingtest %>" />
	</h1>
	<div>
		<aspdnsf:AlertMessage runat="server" ID="ctrlAlertMessage" />
	</div>
	<div class="list-action-bar">
		<div class="other-actions">
			<aspdnsf:StoreSelector runat="server" ID="ssOne" AutoPostBack="true" OnSelectedIndexChanged="ssOne_SelectedIndexChanged"
				SelectMode="SingleDropDown" ShowDefaultForAllStores="true" />
		</div>
		<asp:Button runat="server" ID="btnSendTestReceiptTop" CssClass="btn btn-default"
			Text="<%$Tokens:StringResource, admin.mailingtest.SendTestReceipt %>" ValidationGroup="MainAdvanced" 
			OnClick="btnSendTestReceipt_Click" />
		<asp:Button runat="server" ID="btnSendNewOrderNotificationTop" CssClass="btn btn-default"
			Text="<%$Tokens:StringResource, admin.mailingtest.SendTestOrderNotification %>" ValidationGroup="MainAdvanced"
			OnClick="btnSendNewOrderNotification_Click" />
		<asp:Button runat="server" ID="btnSendTestShippedTop" CssClass="btn btn-default"
			Text="<%$Tokens:StringResource, admin.mailingtest.SendTestShippedEmail %>" ValidationGroup="MainAdvanced"
			OnClick="btnSendTestShipped_Click" />
		<asp:Button runat="server" ID="btnSendAllTop" CssClass="btn btn-default"
			Text="<%$Tokens:StringResource, admin.mailingtest.TestAll %>" ValidationGroup="MainAdvanced"
			OnClick="btnSendAll_Click" />
		<asp:Button runat="server" ID="btnUpdateAppConfigsTop" CssClass="btn btn-primary" 
			Text="<%$Tokens:StringResource, admin.common.Save %>" ValidationGroup="MainAdvanced"
			OnClick="btnUpdateAppConfigs_Click" />
	</div>
	<div class="white-ui-box">
		<div class="row form-group">
			<div class="col-sm-4">
				<span class="text-danger">*</span><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.mailingtest.MailServerDNS %>" />:
			</div>
			<div class="col-sm-8">

				<asp:TextBox Width="350" CssClass="singleNormal" ID="txtMailMe_Server" runat="server"></asp:TextBox>
				<asp:RequiredFieldValidator ControlToValidate="txtMailMe_Server" ErrorMessage="<%$Tokens:StringResource, admin.mailingtest.EnterDNS %>"
					ID="RequiredFieldValidator3" ValidationGroup="MainAdvanced" EnableClientScript="true"
					SetFocusOnError="true" runat="server" Display="Dynamic" CssClass="text-danger" />
			</div>
		</div>
		<div class="row form-group">
			<div class="col-sm-4">
				<label>
					<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.mailingtest.MailServerUsername %>" />:</label>
			</div>
			<div class="col-sm-8">
				<asp:TextBox Width="350" CssClass="singleNormal" ID="txtMailServerUser" runat="server"></asp:TextBox>
			</div>
		</div>
		<div class="row form-group">
			<div class="col-sm-4">
				<label>
					<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.mailingtest.MailServerPassword %>" />:</label>
			</div>
			<div class="col-sm-8">
				<asp:TextBox Width="350" CssClass="singleNormal" ID="txtMailServerPwd" runat="server"></asp:TextBox>
			</div>
		</div>
		<div class="row form-group">
			<div class="col-sm-4">
				<span class="text-danger">*</span><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.mailingtest.MailServerTCP %>" />:
			</div>
			<div class="col-sm-8">
				<asp:TextBox Width="350" CssClass="singleNormal" ID="txtMailServerPort" runat="server"></asp:TextBox>
				<asp:RequiredFieldValidator ControlToValidate="txtMailServerPort" ErrorMessage="<%$Tokens:StringResource, admin.mailingtest.EnterTCP %>"
					ID="RequiredFieldValidator6" ValidationGroup="MainAdvanced" EnableClientScript="true"
					SetFocusOnError="true" runat="server" Display="Dynamic" CssClass="text-danger" />
			</div>
		</div>
		<div class="row form-group">
			<div class="col-sm-4">
				<label>
					<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.mailingtest.MailServerRequiresSSL %>" />:</label>
			</div>
			<div class="col-sm-8">
				<asp:RadioButtonList ID="rblMailServerSSL" runat="server" RepeatColumns="2"
					RepeatDirection="horizontal">
					<asp:ListItem Value="1" Text="Yes"></asp:ListItem>
					<asp:ListItem Value="0" Text="No" Selected="true"></asp:ListItem>
				</asp:RadioButtonList>
			</div>
		</div>
		<div class="row form-group">
			<div class="col-sm-4">
				<span class="text-danger">*</span><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.mailingtest.ReceiptEmailSendsFrom %>" />:
			</div>
			<div class="col-sm-8">
				<asp:TextBox Width="350" CssClass="singleNormal" ID="txtReceiptFrom" runat="server"></asp:TextBox>
				<asp:RequiredFieldValidator ControlToValidate="txtReceiptFrom" ErrorMessage="<%$Tokens:StringResource, admin.mailingtest.EnterEmail %>"
					ID="RequiredFieldValidator4" ValidationGroup="MainAdvanced" EnableClientScript="true"
					SetFocusOnError="true" runat="server" Display="Dynamic" CssClass="text-danger"></asp:RequiredFieldValidator>
			</div>
		</div>
		<div class="row form-group">
			<div class="col-sm-4">
				<label>
					<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.mailingtest.ReceiptEmailSendsFromName %>" />:</label>
			</div>
			<div class="col-sm-8">
				<asp:TextBox Width="350" CssClass="singleNormal" ID="txtReceiptFromName"
					runat="server"></asp:TextBox>
			</div>
		</div>
		<div class="row form-group">
			<div class="col-sm-4">
				<span class="text-danger">*</span><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.mailingtest.ReceiptEmailWithXML %>" />:
			</div>
			<div class="col-sm-8">
				<aspdnsf:XmlPackageSelector runat="server" 
					ID="ddXmlPackageReceipt"
					Prefix="notification"
					Locations="RootAndSkin"
					Width="350" />
			</div>
		</div>
		<div class="row form-group">
			<div class="col-sm-4">
				<span class="text-danger">*</span><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.mailingtest.SendReceiptEmail %>" />:
			</div>
			<div class="col-sm-8">
				<asp:RadioButtonList ID="rblSendReceipts" runat="server" RepeatColumns="2"
					RepeatDirection="horizontal">
					<asp:ListItem Value="true" Text="<%$Tokens:StringResource, admin.common.Yes %>" Selected="true" />
					<asp:ListItem Value="false" Text="<%$Tokens:StringResource, admin.common.No %>" />
				</asp:RadioButtonList>
			</div>
		</div>
		<div class="row form-group">
			<div class="col-sm-4">
				<span class="text-danger">*</span><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.mailingtest.NewOrderSendToEmail %>" />:
			</div>
			<div class="col-sm-8">
				<asp:TextBox Width="350" CssClass="singleNormal" ID="txtOrderNotificationTo"
					runat="server"></asp:TextBox>
				<asp:RequiredFieldValidator ControlToValidate="txtOrderNotificationTo" ErrorMessage="<%$Tokens:StringResource, admin.mailingtest.EnterAdminEmail %>"
					ID="RequiredFieldValidator5" ValidationGroup="MainAdvanced" EnableClientScript="true"
					SetFocusOnError="true" runat="server" Display="Static" CssClass="text-danger"></asp:RequiredFieldValidator>
			</div>
		</div>
		<div class="row form-group">
			<div class="col-sm-4">
				<label>
					<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.mailingtest.NewOrderSendToName %>" />:</label>
			</div>
			<div class="col-sm-8">
				<asp:TextBox Width="350" CssClass="singleNormal" ID="txtOrderNotificationToName"
					runat="server"></asp:TextBox>
			</div>
		</div>
		<div class="row form-group">
			<div class="col-sm-4">
				<label>
					<span class="text-danger">*</span><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.mailingtest.NewOrderSendFromEmail %>" />:</label>
			</div>
			<div class="col-sm-8">
				<asp:TextBox Width="350" CssClass="singleNormal" ID="txtOrderNotificationFrom"
					runat="server"></asp:TextBox>
				<asp:RequiredFieldValidator ControlToValidate="txtOrderNotificationFrom"
					ErrorMessage="<%$Tokens:StringResource, admin.mailingtest.EnterAdminEmailFrom %>"
					ID="RequiredFieldValidator9" ValidationGroup="MainAdvanced" EnableClientScript="true"
					SetFocusOnError="true" runat="server" Display="Static" CssClass="text-danger"></asp:RequiredFieldValidator>
			</div>
		</div>
		<div class="row form-group">
			<div class="col-sm-4">
				<label>
					<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.mailingtest.NewOrderSendFromName %>" />:</label>
			</div>
			<div class="col-sm-8">
				<asp:TextBox Width="350" CssClass="singleNormal" ID="txtOrderNotificationFromName"
					runat="server"></asp:TextBox>
			</div>
		</div>
		<div class="row form-group">
			<div class="col-sm-4">
				<label>
					<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.mailingtest.SendNewOrderWithXML %>" />:</label>
			</div>
			<div class="col-sm-8">
				<aspdnsf:XmlPackageSelector runat="server" 
					ID="ddXmlPackageOrderNotifications"
					Prefix="notification"
					Locations="RootAndSkin"
					Width="350" />
			</div>
		</div>
		<div class="row form-group">
			<div class="col-sm-4">
				<span class="text-danger">*</span><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.mailingtest.SendNewOrderNotifications %>" />:
			</div>
			<div class="col-sm-8">
				<asp:RadioButtonList ID="rblSendOrderNotifications" runat="server" RepeatColumns="2"
					RepeatDirection="horizontal">
					<asp:ListItem Value="false" Text="<%$Tokens:StringResource, admin.common.Yes %>" Selected="true" />
					<asp:ListItem Value="true" Text="<%$Tokens:StringResource, admin.common.No %>" />
				</asp:RadioButtonList>
			</div>
		</div>
		<div class="row form-group">
			<div class="col-sm-4">
				<span class="text-danger">*</span><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.mailingtest.ShippedEmailsWithXML %>" />:
			</div>
			<div class="col-sm-8">
				<aspdnsf:XmlPackageSelector runat="server" 
					ID="ddXmlPackageShipped"
					Prefix="notification"
					Locations="RootAndSkin"
					Width="350" />
			</div>
		</div>
		<div class="row form-group">
			<div class="col-sm-4">
				<span class="text-danger">*</span><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.mailingtest.SendShippedEmails %>" />:
			</div>
			<div class="col-sm-8">
				<asp:RadioButtonList ID="rblSendShippedNotifications" runat="server" RepeatColumns="2"
					RepeatDirection="horizontal">
					<asp:ListItem Value="true" Text="<%$Tokens:StringResource, admin.common.Yes %>" Selected="true" />
					<asp:ListItem Value="false" Text="<%$Tokens:StringResource, admin.common.No %>" />
				</asp:RadioButtonList>
			</div>
		</div>
	</div>
	<div class="list-action-bar">		
		<asp:Button runat="server" ID="btnSendTestReceipt" CssClass="btn btn-default"
			Text="<%$Tokens:StringResource, admin.mailingtest.SendTestReceipt %>" ValidationGroup="MainAdvanced" 
			OnClick="btnSendTestReceipt_Click" />
		<asp:Button runat="server" ID="btnSendNewOrderNotification" CssClass="btn btn-default"
			Text="<%$Tokens:StringResource, admin.mailingtest.SendTestOrderNotification %>" ValidationGroup="MainAdvanced"
			OnClick="btnSendNewOrderNotification_Click" />
		<asp:Button runat="server" ID="btnSendTestShipped" CssClass="btn btn-default"
			Text="<%$Tokens:StringResource, admin.mailingtest.SendTestShippedEmail %>" ValidationGroup="MainAdvanced"
			OnClick="btnSendTestShipped_Click" />
		<asp:Button runat="server" ID="btnSendAll" CssClass="btn btn-default"
			Text="<%$Tokens:StringResource, admin.mailingtest.TestAll %>" ValidationGroup="MainAdvanced"
			OnClick="btnSendAll_Click" />
		<asp:Button runat="server" ID="btnUpdateAppConfigs" CssClass="btn btn-primary" 
			Text="<%$Tokens:StringResource, admin.common.Save %>" ValidationGroup="MainAdvanced"
			OnClick="btnUpdateAppConfigs_Click" />
	</div>
	<asp:ValidationSummary ID="validationSummaryAdvanced" ValidationGroup="MainAdvanced" DisplayMode="BulletList" ShowMessageBox="true" ShowSummary="false" runat="server" />
</asp:Content>