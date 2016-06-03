<%@ Page Language="c#" Inherits="AspDotNetStorefrontAdmin.stringresourceeditor" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="stringresource.aspx.cs" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<aspdnsf:ReturnUrlTracker runat="server" ID="ReturnUrlTracker" DefaultReturnUrl="stringresources.aspx" />
	<div class="admin-module">
		<h1>
			<i class="fa fa-pencil-square-o"></i>
			<asp:Literal ID="HeaderText" runat="server" />
		</h1>
		<aspdnsf:AlertMessage ID="ctlAlertMessage" runat="server" />
		<div class="item-action-bar">
			<asp:HyperLink runat="server" ID="btnCloseTop" CssClass="btn btn-default" NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>" Text="<%$Tokens:StringResource, admin.common.close %>" />
			<asp:Button ID="btnSaveAndCloseTop" runat="server" CssClass="btn btn-default" OnClick="btnSaveAndClose_Click" ValidationGroup="gAdd" Text="<%$Tokens:StringResource, admin.common.SaveAndClose %>" />
			<asp:Button ID="btnSubmitTop" runat="server" CssClass="btn btn-primary" OnClick="btnSubmit_Click" ValidationGroup="gAdd" Text="<%$Tokens:StringResource, admin.common.Save %>" />
		</div>

		<div class="admin-row">
			<div id="divEditCountry" class="white-ui-box">
				<div class="row">
					<div class="col-md-4">
						<div class="form-inline">
							<asp:Label AssociatedControlID="litStringId" runat="server" Text="<%$Tokens:StringResource, admin.common.ID %>" />:
							<asp:Literal ID="litStringId" runat="server" />
						</div>
						<div class="form-group">
							<span class="text-danger">*</span><asp:Label AssociatedControlID="txtStringName" runat="server" Text="<%$Tokens:StringResource, admin.common.name %>" />:
							<asp:TextBox ID="txtStringName" runat="server" CssClass="form-control" ValidationGroup="gAdd"></asp:TextBox>
							<asp:RequiredFieldValidator ErrorMessage="<%$Tokens:StringResource, admin.common.FillinName %>" CssClass="text-danger" ControlToValidate="txtStringName" ID="RequiredFieldValidator2" ValidationGroup="gAdd" SetFocusOnError="true" runat="server"></asp:RequiredFieldValidator>
						</div>
						<div class="form-group">
							<asp:Label AssociatedControlID="txtValue" runat="server" Text="<%$Tokens:StringResource, admin.common.Value %>" />:
							<asp:TextBox ID="txtValue" TextMode="MultiLine" Rows="4" runat="server" CssClass="form-control" ValidationGroup="gAdd"></asp:TextBox>
						</div>
						<div class="form-group">
							<span class="text-danger">*</span><asp:Label AssociatedControlID="ddLocales" runat="server" Text="<%$Tokens:StringResource, admin.stringresources.Locale %>" />
							<asp:DropDownList ID="ddLocales" CssClass="form-control" runat="server" AutoPostBack="false" />
						</div>
						<div class="form-group">
							<span class="text-danger">*</span><asp:Label AssociatedControlID="ddStores" runat="server" Text="Store" />:
							<asp:DropDownList ID="ddStores" CssClass="form-control" runat="server" AutoPostBack="false" />
						</div>
					</div>
				</div>
			</div>
			<asp:ValidationSummary ValidationGroup="gAdd" ID="validationSummary" runat="server" EnableClientScript="true" ShowMessageBox="false" ShowSummary="false" Enabled="true" />

			<div class="item-action-bar">
				<asp:HyperLink runat="server" ID="btnClose" CssClass="btn btn-default" NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>" Text="<%$Tokens:StringResource, admin.common.close %>" />
				<asp:Button ID="btnSaveAndClose" runat="server" CssClass="btn btn-default" OnClick="btnSaveAndClose_Click" ValidationGroup="gAdd" Text="<%$Tokens:StringResource, admin.common.SaveAndClose %>" />
				<asp:Button ID="btnSubmit" runat="server" CssClass="btn btn-primary" OnClick="btnSubmit_Click" ValidationGroup="gAdd" Text="<%$Tokens:StringResource, admin.common.Save %>" />
			</div>
		</div>
	</div>
</asp:Content>