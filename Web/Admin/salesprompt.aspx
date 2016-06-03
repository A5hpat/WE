<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.salesprompt" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="salesprompt.aspx.cs" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<aspdnsf:ReturnUrlTracker runat="server" ID="ReturnUrlTracker" DefaultReturnUrl="SalesPrompts.aspx" />

	<div class="admin-module">
		<h1>
			<i class="fa fa-pencil-square-o"></i>
			<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.title.editsalesprompt %>" />
		</h1>

		<aspdnsf:AlertMessage ID="ctlAlertMessage" runat="server" />

		<div class="item-action-bar">
			<div class="other-actions">
				<asp:Panel runat="server" Visible='<%# LocaleSelector.HasMultipleLocales() %>'>
					<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.stringresources.Locale %>" AssociatedControlID="LocaleSelector" />
					<aspdnsf:LocaleSelector runat="server" ID="LocaleSelector" OnSelectedLocaleChanged="LocaleSelector_SelectedLocaleChanged" />
				</asp:Panel>
			</div>

			<asp:HyperLink runat="server" CssClass="btn btn-default" NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>" Text="<%$Tokens:StringResource, admin.common.close %>" />
			<asp:Button runat="server" CssClass="btn btn-default" OnClick="btnSaveAndClose_Click" Text="<%$Tokens:StringResource, admin.common.SaveAndClose %>" />
			<asp:Button runat="server" CssClass="btn btn-primary" OnClick="btnSubmit_Click" Text="<%$Tokens:StringResource, admin.common.Save %>" />
		</div>

		<div class="white-ui-box">
			<div class="admin-row">
				<div class="row">
					<div class="col-md-4">
						<div class="form-group">
							<span class="text-danger">*</span>
							<asp:Label runat="server"
								AssociatedControlID="NameTextbox"
								Text="Sales Prompt:" />:
							<asp:TextBox runat="server"
								ID="NameTextBox"
								CssClass="form-control" />
							<asp:RequiredFieldValidator runat="server"
								CssClass="text-danger"
								ControlToValidate="NameTextBox"
								ErrorMessage="<%$Tokens:StringResource, admin.common.FillinName %>"
								SetFocusOnError="true" />
							<asp:CustomValidator runat="server"
								CssClass="text-danger"
								ControlToValidate="NameTextBox"
								ErrorMessage="That name is already used by another Sales Prompt"
								OnServerValidate="ValidateUniqueName" />
								
						</div>
					</div>
				</div>
			</div>
		</div>

		<div class="item-action-bar">
			<asp:HyperLink runat="server" CssClass="btn btn-default" NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>" Text="<%$Tokens:StringResource, admin.common.close %>" />
			<asp:Button runat="server" CssClass="btn btn-default" OnClick="btnSaveAndClose_Click" Text="<%$Tokens:StringResource, admin.common.SaveAndClose %>" />
			<asp:Button runat="server" CssClass="btn btn-primary" OnClick="btnSubmit_Click" Text="<%$Tokens:StringResource, admin.common.Save %>" />
		</div>
	</div>

</asp:Content>
