<%@ Page Language="c#" Inherits="AspDotNetStorefrontAdmin.rating" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="rating.aspx.cs" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<aspdnsf:ReturnUrlTracker runat="server" ID="ReturnUrlTracker" DefaultReturnUrl="ratings.aspx" />
	<div class="admin-module">
		<h1>
			<i class="fa fa-thumbs-up"></i>
			<asp:Literal ID="litManageRatingTitle" runat="server" Text="<%$Tokens:StringResource, admin.menu.Rating %>" />
		</h1>
		<aspdnsf:AlertMessage ID="ctlAlertMessage" runat="server" />
		<div class="item-action-bar">
			<asp:HyperLink ID="btnCloseTop" runat="server" CssClass="btn btn-default" NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>" Text="<%$Tokens:StringResource, admin.common.close %>" />
			<asp:Button ID="btnSaveAndCloseTop" runat="server" CssClass="btn btn-default" OnClick="btnSaveAndClose_Click" ValidationGroup="gAdd" Text="<%$Tokens:StringResource, admin.common.SaveAndClose %>" />
			<asp:Button ID="btnSubmitTop" runat="server" CssClass="btn btn-primary" OnClick="btnSubmit_Click" ValidationGroup="gAdd" Text="<%$Tokens:StringResource, admin.common.Save %>" />
		</div>

		<div class="admin-row">
			<div id="divEditRating" runat="server" class="white-ui-box">
				<div class="row">
					<div class="col-md-4">
						<div class="form-inline">
							<asp:Label AssociatedControlID="litRatingId" runat="server" Text="<%$Tokens:StringResource, admin.common.ID %>" />:
							<asp:Literal ID="litRatingId" runat="server" />
						</div>
						<div class="form-group">
							<span class="text-danger">*</span><asp:Label AssociatedControlID="txtRatingComment" runat="server" Text="<%$ Tokens:StringResource, admin.manageratings.Comments %>" />:
							<asp:TextBox ID="txtRatingComment" runat="server" TextMode="MultiLine" Rows="4" CssClass="form-control" ValidationGroup="gAdd"></asp:TextBox>
							<asp:RequiredFieldValidator Display="Dynamic" ErrorMessage="Fill in a comment!" ControlToValidate="txtRatingComment" ID="RequiredFieldValidator2" ValidationGroup="gAdd" SetFocusOnError="true" runat="server" />
						</div>
						<div class="form-group">
							<span class="text-danger">*</span><asp:Label AssociatedControlID="txtRating" runat="server" Text="<%$ Tokens:StringResource, admin.manageratings.Rating %>" />:
							<asp:TextBox ID="txtRating" runat="server" CssClass="form-control" ValidationGroup="gAdd"></asp:TextBox>
							<asp:RequiredFieldValidator Display="Dynamic" ErrorMessage="Fill in a rating!" ControlToValidate="txtRating" ID="RequiredFieldValidator1" ValidationGroup="gAdd" SetFocusOnError="true" runat="server" />
							<asp:RangeValidator Display="Dynamic" ID="RangeValidator1" ControlToValidate="txtRating" MaximumValue="5" MinimumValue="1" Type="Integer" ErrorMessage="Please enter a number between 1 and 5!" ValidationGroup="gAdd" SetFocusOnError="true" runat="server" />
						</div>
						<div class="form-group">
							<asp:Label AssociatedControlID="cbxHasBadWords" runat="server" Text="<%$ Tokens:StringResource, admin.manageratings.HasBadWords %>" />:
							<asp:CheckBox runat="server" ID="cbxHasBadWords" />
						</div>
					</div>
				</div>
			</div>
			<asp:ValidationSummary ValidationGroup="gAdd" ID="validationSummary" runat="server" EnableClientScript="true" ShowMessageBox="false" ShowSummary="false" Enabled="true" />

			<div class="item-action-bar">
				<asp:HyperLink ID="btnClose" runat="server" CssClass="btn btn-default" NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>" Text="<%$Tokens:StringResource, admin.common.close %>" />
				<asp:Button ID="btnSaveAndClose" runat="server" CssClass="btn btn-default" OnClick="btnSaveAndClose_Click" ValidationGroup="gAdd" Text="<%$Tokens:StringResource, admin.common.SaveAndClose %>" />
				<asp:Button ID="btnSubmit" runat="server" CssClass="btn btn-primary" OnClick="btnSubmit_Click" ValidationGroup="gAdd" Text="<%$Tokens:StringResource, admin.common.Save %>" />
			</div>
		</div>
	</div>
</asp:Content>
