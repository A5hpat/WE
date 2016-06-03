<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.affiliate" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="affiliate.aspx.cs" %>

<%@ Register TagPrefix="aspdnsf" TagName="EntityToStore" Src="controls/EntityToStoreMapper.ascx" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<aspdnsf:ReturnUrlTracker runat="server" ID="ReturnUrlTracker" DefaultReturnUrl="AppConfigs.aspx" />
	<div id="container">
		<h1>
			<i class="fa fa-retweet"></i>
			<asp:Label runat="server" ID="HeaderText" />
		</h1>
		<aspdnsf:AlertMessage runat="server" ID="ctrlAlertMessage" />
		<div class="list-action-bar">
			<asp:HyperLink runat="server"
				CssClass="btn btn-default"
				NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>"
				Text="<%$Tokens:StringResource, admin.common.close %>" />

			<asp:Button runat="server"
				ID="btnSaveAndClose"
				CssClass="btn btn-default"
				ValidationGroup="signup"
				OnClick="btnSaveAndClose_Click"
				Text="<%$Tokens:StringResource, admin.common.SaveAndClose %>" />

			<asp:Button runat="server"
				ID="btnSubmit"
				CssClass="btn btn-primary"
				ValidationGroup="signup"
				OnClick="btnSubmit_Click"
				Text="<%$Tokens:StringResource, admin.common.Save %>" />
		</div>
		<asp:Panel ID="pnlAffiliateDetails" runat="server" DefaultButton="btnSubmit">
			<div class="white-ui-box" >
				<div class="row">
					<div class="col-md-4">
						<div class="form-group">
							<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editAffiliates.NickName %>" />
							<asp:TextBox ID="txtNickName" CssClass="form-control" runat="server" />
						</div>
						<div class="form-group">
							<span class="text-danger">*</span><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editAffiliates.FirstName %>" />:
							<asp:TextBox ID="txtFirstName" CssClass="form-control" runat="server" />
							<asp:RequiredFieldValidator ValidationGroup="signup" ErrorMessage="<%$Tokens:StringResource, admin.editAffiliates.ErrorMessageFirstName %>"
								ControlToValidate="txtFirstName" EnableClientScript="false" ID="RequiredFieldValidator4" SetFocusOnError="true" runat="server" Display="dynamic"
								CssClass="text-danger" />
						</div>
						<div class="form-group">
							<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editAffiliates.LastName %>" />
							<asp:TextBox ID="txtLastName" CssClass="form-control" runat="server" />
						</div>
						<div class="form-group">
							<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editAffiliates.ParentAffiliate %>" />
							<asp:DropDownList ID="ddParent" CssClass="form-control" runat="server" />
						</div>
						<div class="form-group">
							<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editaddress.Email %>" />
							<asp:TextBox ID="txtEmail" CssClass="form-control" runat="server" />
							<asp:RequiredFieldValidator ID="valReqEmail" runat="server" Display="dynamic" EnableClientScript="false" ValidationGroup="signup" ErrorMessage="<%$Tokens:StringResource, admin.editAffiliates.EnterEmailAddress %>" ControlToValidate="txtEmail" SetFocusOnError="true" Enabled="false" />
						</div>
						<div runat="server" id="ResetPasswordRow" class="form-group">
							<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.cst_account.ResetPassword %>" />
							<asp:LinkButton ID="ResetPasswordLink" runat="server" OnClick="ResetPasswordLink_Click" Text="<%$Tokens:StringResource, admin.common.ResetPassword %>"></asp:LinkButton>
							<asp:Label ID="ResetPasswordError" runat="server" Text="Label" Visible="False" CssClass="text-danger"></asp:Label>
							<asp:Label ID="ResetPasswordOk" runat="server" Text="Label" Visible="False" CssClass="text-success"></asp:Label>
						</div>
						<asp:Panel ID="CreatePasswordRow" runat="server">
							<div class="form-group">
								<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.topic.password %>" />
								<asp:TextBox ID="AffPassword" TextMode="Password" CssClass="form-control" runat="server" ValidationGroup="signup" CausesValidation="true" />
								<asp:RequiredFieldValidator ID="reqValPassword" CssClass="text-danger" ControlToValidate="AffPassword" runat="server" Display="Dynamic" EnableClientScript="false" ValidationGroup="signup" Enabled="false"></asp:RequiredFieldValidator>
								<asp:CustomValidator ID="valPassword" runat="server" CssClass="text-danger" Display="Dynamic" EnableClientScript="false" ValidationGroup="signup" SetFocusOnError="true" OnServerValidate="ValidatePassword"></asp:CustomValidator>
							</div>
							<div class="form-group">
								<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editAffiliates.RepeatPassword %>" />
								<asp:TextBox ID="AffPassword2" TextMode="Password" CssClass="form-control" runat="server" ValidationGroup="signup" CausesValidation="true" />
							</div>
						</asp:Panel>
						<div class="form-group">
							<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.order.Company %>" />
							<asp:TextBox ID="txtCompany" CssClass="form-control" runat="server" />
						</div>
						<div class="form-group">
							<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editAffiliates.Address1 %>" />
							<asp:TextBox ID="txtAddress1" runat="server" CssClass="form-control" />
						</div>
						<div class="form-group">
							<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editAffiliates.Address2 %>" />
							<asp:TextBox ID="txtAddress2" runat="server" CssClass="form-control" />
						</div>
						<div class="form-group">
							<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editaddress.Suite %>" />
							<asp:TextBox ID="txtSuite" runat="server" CssClass="form-control" />
						</div>
						<div class="form-group">
							<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editAffiliates.City %>" />
							<asp:TextBox ID="txtCity" runat="server" CssClass="form-control" />
						</div>
						<div class="form-group">
							<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editaddress.State %>" />
							<asp:DropDownList ID="ddState" runat="server" CssClass="form-control" />
						</div>
						<div class="form-group">
							<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editAffiliates.Zip %>" />
							<asp:TextBox ID="txtZip" runat="server" CssClass="form-control" />
							<asp:RegularExpressionValidator Display="Dynamic" ValidationGroup="signup" CssClass="text-danger" ControlToValidate="txtZip" ID="revTxtZip" ValidationExpression="^[\s\S]{0,10}$" runat="server" ErrorMessage="Maximum 10 characters allowed" />
						</div>
						<div class="form-group">
							<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editaddress.Country %>" />
							<asp:DropDownList ID="ddCountry" runat="server" CssClass="form-control" />
						</div>
						<div class="form-group">
							<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editaddress.Phone %>" />
							<asp:TextBox ID="txtPhone" runat="server" CssClass="form-control" />
						</div>
						<div class="form-group">
							<asp:Panel ID="litStoreMapperHdr" runat="server" Visible="false">
								<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.topic.mapstores %>" />
							</asp:Panel>
						</div>
						<div class="form-group">
							<asp:Panel ID="litStoreMapper" runat="server">
								<aspdnsf:EntityToStore ID="etsMapper" runat="server"
									EntityType="Affiliate"
									Text="" />
							</asp:Panel>
						</div>
					</div>
				</div>
			</div>

			<div class="white-ui-box">
				<div class="row">
					<div class="white-box-heading">
						<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editAffiliates.WebSiteInformation %>" />
						<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editAffiliates.WebSiteNotification %>" />
					</div>

					<div class="col-md-4">
						<div class="form-group">
							<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editAffiliates.WebSiteName %>" />
							<asp:TextBox ID="txtWebName" runat="server" CssClass="form-control" />
						</div>
						<div class="form-group">
							<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editAffiliates.WebSiteDescription %>" />
							<asp:TextBox ID="txtWebDescription" runat="server" CssClass="form-control" />
						</div>
						<div class="form-group">
							<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editAffiliates.WebSiteUrl %>" />
							<asp:TextBox ID="txtWebURL" runat="server" CssClass="form-control" />
							<asp:RegularExpressionValidator runat="server" ID="valURL" ControlToValidate="txtWebURL" Display="Dynamic" ValidationGroup="signup" SetFocusOnError="true" CssClass="text-danger" ErrorMessage="Invalid URL format"
								ValidationExpression="((([A-Za-z]{3,9}:(?:\/\/)?)(?:[\-;:&=\+\$,\w]+@)?[A-Za-z0-9\.\-]+|(?:www\.|[\-;:&=\+\$,\w]+@)[A-Za-z0-9\.\-]+)((?:\/[\+~%\/\.\w\-_]*)?\??(?:[\-\+=&;%@\.\w_]*)#?(?:[\.\!\/\\\w]*))?)" />
						</div>
					</div>
				</div>
			</div>

		</asp:Panel>
		<div class="list-action-bar">
			<asp:HyperLink ID="hlAffiliates1" runat="server" CssClass="btn btn-default" NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>">Close</asp:HyperLink>
			<asp:Button ValidationGroup="signup" ID="btnSaveAndCloseBottom" Text="<%$Tokens:StringResource, admin.common.SaveAndClose %>" runat="server" CssClass="btn btn-default" OnClick="btnSaveAndClose_Click" />
			<asp:Button ValidationGroup="signup" ID="btnSubmitBottom" Text="<%$Tokens:StringResource, admin.common.Save %>" runat="server" CssClass="btn btn-primary" OnClick="btnSubmit_Click" />
		</div>
	</div>
</asp:Content>
