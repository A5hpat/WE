<%@ Page Language="c#" Inherits="AspDotNetStorefrontAdmin.changeencryptkey"
	EnableEventValidation="false" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" CodeBehind="changeencryptkey.aspx.cs" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<h1>
		<i class="fa fa-wrench"></i>
		<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.title.changeencryptkey %>" />
	</h1>

	<div class="alert alert-danger">
		<asp:Label ID="Label6" runat="server" Text="<%$Tokens:StringResource, admin.changeencryptkey.Backup %>" />
	</div>

	<asp:UpdatePanel runat="server">
		<ContentTemplate>
			<aspdnsf:AlertMessage ID="ctlAlertMessage" runat="server" />
		</ContentTemplate>
	</asp:UpdatePanel>

	<asp:Panel runat="server" CssClass="white-ui-box" DefaultButton="btnUpdateEncryptKey">
		<p>
			<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.changeencryptkey.Recommendation %>" />
		</p>
		<p>
			<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.changeencryptkey.StoringCreditCards %>" />
			<asp:Label ID="StoringCC" runat="server" Font-Bold="True" />
		</p>
		<p>
			<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.changeencryptkey.RequireCreditCardStorage %>" />
			<asp:Label ID="RecurringProducts" runat="server" Font-Bold="True" />
		</p>
		
		<p>
			<h4>
				<asp:Literal ID="ltChangeEncryptKey" runat="server" Text="<%$ Tokens:StringResource, admin.changeencrypt.ChangeEncryptKey %>" />
				<asp:RadioButtonList ID="rblChangeEncryptKey" runat="server" OnSelectedIndexChanged="rblChangeEncryptKey_OnSelectedIndexChanged" AutoPostBack="true">
					<asp:ListItem Value="true" Text="<%$Tokens:StringResource, admin.common.yes %>" />
					<asp:ListItem Value="false" Text="<%$Tokens:StringResource, admin.common.no %>" Selected="True" />
				</asp:RadioButtonList>
			</h4>
			<asp:Panel ID="pnlChangeEncryptKeyMaster" runat="server" Visible="false">
				<strong>
					<asp:Label ID="Label5" runat="server" Text="<%$Tokens:StringResource, admin.changeencryptkey.BePatient %>" /></strong><br />
				<br />

				<asp:Panel runat="server" ID="pnlEncryptKey">
					<div>
						<asp:Label ID="Label1" runat="server" Text="<%$Tokens:StringResource, admin.changeencryptkey.Explanation %>" />
					</div>

					<div class="row">
						<div class="col-md-2">
							<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.changeencrypt.PrimaryEncryptKey %>" />
						</div>
						<div class="col-md-2">
							<asp:TextBox ID="txtPrimaryEncryptKey" runat="server" TextMode="Password" AutoCompleteType="Disabled" Width="317px" MaxLength="50" />
						</div>
						<div class="col-md-8">
						</div>
					</div>

					<div class="row">
						<div class="col-md-2">
							<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.changeencrypt.PrimaryEncryptKey.Confirm %>" />
						</div>
						<div class="col-md-10">
							<asp:TextBox ID="txtPrimaryEncryptKeyConfirm" runat="server" TextMode="Password" AutoCompleteType="Disabled" Width="317px" MaxLength="50" />
							<asp:RequiredFieldValidator runat="server"
								ControlToValidate="txtPrimaryEncryptKeyConfirm"
								ErrorMessage="<%$Tokens:StringResource, admin.changeencrypt.PrimaryEncryptKey.Confirm.Required %>"
								Display="Dynamic" EnableClientScript="true" CssClass="error-just-color" />

							<asp:CompareValidator runat="server"
								ControlToCompare="txtPrimaryEncryptKey"
								ControlToValidate="txtPrimaryEncryptKeyConfirm"
								ErrorMessage="<%$Tokens:StringResource, admin.changeencrypt.PrimaryEncryptKey.DoesntMatch %>"
								Display="Dynamic" Operator="Equal" EnableClientScript="true" CssClass="error-just-color" />
						</div>
					</div>

					<div class="row">
						<div class="col-md-2">
							<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.changeencrypt.SecondaryEncryptKey %>" />
						</div>
						<div class="col-md-2">
							<asp:TextBox ID="txtSecondaryEncryptKey" runat="server" TextMode="Password" AutoCompleteType="Disabled" Width="317px" MaxLength="50" />
						</div>
						<div class="col-md-8">
						</div>
					</div>

					<div class="row">
						<div class="col-md-2">
							<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.changeencrypt.SecondaryEncryptKey.Confirm %>" />
						</div>
						<div class="col-md-10">
							<asp:TextBox ID="txtSecondaryEncryptKeyConfirm" runat="server" TextMode="Password" AutoCompleteType="Disabled" Width="317px" MaxLength="50" />
							<asp:CompareValidator runat="server"
								ControlToCompare="txtSecondaryEncryptKey"
								ControlToValidate="txtSecondaryEncryptKeyConfirm"
								ErrorMessage="<%$Tokens:StringResource, admin.changeencrypt.SecondaryEncryptKey.DoesntMatch %>"
								Display="Dynamic" Operator="Equal" EnableClientScript="true" CssClass="error-just-color" />
						</div>
					</div>
				</asp:Panel>
			</asp:Panel>
		</p>
		<p>
			<h4>
				<asp:Literal ID="ltChangeSetMachineKey" runat="server" Text="<%$ Tokens:StringResource, admin.changeencrypt.SetChangeMachineKey %>" />
				<asp:RadioButtonList ID="rblChangeMachineKey" runat="server" OnSelectedIndexChanged="rblChangeMachineKey_OnSelectedIndexChanged" AutoPostBack="true">
					<asp:ListItem Value="true" Text="<%$Tokens:StringResource, admin.common.yes %>" />
					<asp:ListItem Value="false" Text="<%$Tokens:StringResource, admin.common.no %>" Selected="True" />
				</asp:RadioButtonList>
			</h4>
			<asp:Panel ID="pnlChangeSetMachineKey" runat="server" Visible="false">
				<asp:Literal ID="ltMachineKeyGeneration" runat="server" Text="<%$ Tokens:StringResource,admin.changeencrypt.MachineKeyGeneration %>" />
				<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.changeencrypt.tooltip.imgMachineKeyGeneration %>">
							<i class="fa fa-question-circle fa-lg"></i>
				</asp:Label>
				<br />
				<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.changeencryptkey.MachineKeyLogout %>" />
				<asp:RadioButtonList ID="rblMachineKeyGenType" runat="server" AutoPostBack="true"
					OnSelectedIndexChanged="rblMachineKeyGenType_OnSelectedIndexChanged">
					<asp:ListItem Value="auto" Text="<%$ Tokens:StringResource, admin.changeencrypt.Auto %>" />
					<asp:ListItem Value="manual" Text="<%$ Tokens:StringResource, admin.changeencrypt.Manual %>" />
				</asp:RadioButtonList>
				<asp:Panel runat="server" ID="pnlMachineKey" Visible="false">
					<br />
					<asp:Label ID="lblMachineKeyLength" runat="server" Text="<%$Tokens:StringResource, admin.changeencryptkey.MachineKeyDescription %>" />
					<br />
					<br />
					<asp:Label ID="lblEnterValidationKey" runat="server" Text="<%$Tokens:StringResource, admin.changeencrypt.NewValidationKey %>" />
					<asp:TextBox ID="txtValidationKey" runat="server" Width="317px" MaxLength="64" />
					<br />
					<asp:Label ID="lblEnterDecryptKey" runat="server" Text="<%$Tokens:StringResource, admin.changeencrypt.NewDecryptKey %>" />
					<asp:TextBox ID="txtDecryptKey" runat="server" Width="317px" MaxLength="32" />
				</asp:Panel>
			</asp:Panel>
		</p>
	</asp:Panel>

	<div id="pnlUpdateEncryptKey" runat="server" class="item-action-bar">
		<asp:Button ID="btnUpdateEncryptKey" runat="server" CssClass="btn btn-primary" OnClientClick="if(Page_ClientValidate()) { showAdminLoadingOverlay(); }" OnClick="btnUpdateEncryptKey_Click" Text="<%$Tokens:StringResource, admin.changeencrypt.UpdateEncryptKey %>" />
	</div>
</asp:Content>
