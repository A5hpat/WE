<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.ProductType" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="producttype.aspx.cs" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<aspdnsf:ReturnUrlTracker runat="server" ID="ReturnUrlTracker" DefaultReturnUrl="producttypes.aspx" />
	<div class="admin-module">
	<aspdnsf:AlertMessage ID="ctlAlertMessage" runat="server" />
		<h1>
			<i class="fa fa-archive"></i>
			<asp:Literal ID="litHeader" runat="server" />
		</h1>	
		<div class="item-action-bar">
			<asp:HyperLink runat="server" ID="btnCloseTop" CssClass="btn btn-default" NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>" Text="<%$Tokens:StringResource, admin.common.close %>" />
			<asp:Button ID="btnSaveAndCloseTop" runat="server" CssClass="btn btn-default" OnClick="btnSaveAndClose_Click" ValidationGroup="gAdd" Text="<%$Tokens:StringResource, admin.common.SaveAndClose %>" />
			<asp:Button ID="btnSubmitTop" runat="server" CssClass="btn btn-primary" OnClick="btnSubmit_Click" ValidationGroup="gAdd" Text="<%$Tokens:StringResource, admin.common.Save %>" />
		</div>

		<div class="admin-row">
			<div id="divEditState" class="white-ui-box">
				<div class="row">
					<div class="col-md-4">
						<div class="form-inline">
							<asp:Label AssociatedControlID="litTypeId" for="litTypeId" runat="server" Text="<%$Tokens:StringResource, admin.common.ID %>" />:
							<asp:Literal ID="litTypeId" runat="server" />
						</div>
						<div class="form-group">
							<span class="text-danger">*</span><asp:Label AssociatedControlID="txtTypeName" for="txtTypeName" runat="server" Text="Product Type Name" />:
							<asp:TextBox ID="txtTypeName" CssClass="form-control" runat="server" />
							<asp:RequiredFieldValidator ErrorMessage="Fill in Name!" CssClass="text-danger" ControlToValidate="txtTypeName" ID="RequiredFieldValidator9" ValidationGroup="gAdd" SetFocusOnError="true" runat="server"></asp:RequiredFieldValidator>
						</div>
						<div class="form-group">
							<span class="text-danger">*</span><asp:Label AssociatedControlID="txtDisplayOrder" for="txtDisplayOrder" runat="server" Text="<%$Tokens:StringResource, admin.Common.DisplayOrder %>" />
							<asp:TextBox ID="txtDisplayOrder" CssClass="form-control" runat="server" />
							<asp:RequiredFieldValidator ErrorMessage="Fill in Display Order!" CssClass="text-danger" ControlToValidate="txtDisplayOrder" ID="RequiredFieldValidator2" ValidationGroup="gAdd" SetFocusOnError="true" runat="server"></asp:RequiredFieldValidator>
						</div>
					</div>
				</div>
			</div>

			<div class="item-action-bar">
				<asp:HyperLink runat="server" ID="btnClose" CssClass="btn btn-default" NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>" Text="<%$Tokens:StringResource, admin.common.close %>" />
				<asp:Button ID="btnSaveAndClose" runat="server" CssClass="btn btn-default" OnClick="btnSaveAndClose_Click" ValidationGroup="gAdd" Text="<%$Tokens:StringResource, admin.common.SaveAndClose %>" />
				<asp:Button ID="btnSubmit" runat="server" CssClass="btn btn-primary" OnClick="btnSubmit_Click" ValidationGroup="gAdd" Text="<%$Tokens:StringResource, admin.common.Save %>" />
			</div>
		</div>
	</div>
</asp:Content>
