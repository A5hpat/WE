<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.TopicEditor" MaintainScrollPositionOnPostback="false" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="topic.aspx.cs" %>

<%@ Register TagPrefix="telerik" Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" %>
<%@ Register TagPrefix="aspdnsf" TagName="TopicToStore" Src="Controls/StoreSelector.ascx" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">

	<h1>
		<i class="fa fa-file-code-o"></i>
		<asp:Literal runat="server" ID="HeaderText" />
	</h1>

	<aspdnsf:ReturnUrlTracker runat="server" ID="ReturnUrlTracker" DefaultReturnUrl="Topics.aspx" />
	<aspdnsf:AlertMessage runat="server" ID="AlertMessageDisplay" />

	<asp:Panel runat="server" DefaultButton="btnSave">
		<div class="item-action-bar clearfix">
			<div class="other-actions">
				<asp:Panel ID="pnlLocale" runat="server" Visible='<%# LocaleSelector.HasMultipleLocales() %>'>
					<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.stringresources.Locale %>" AssociatedControlID="LocaleSelector" />
					<aspdnsf:LocaleSelector ID="LocaleSelector" runat="server" OnSelectedLocaleChanged="LocaleSelector_SelectedLocaleChanged" />
				</asp:Panel>
			</div>

			<asp:Button runat="server" 
				CssClass="btn btn-danger js-confirm-prompt" 
				Text='<%$ Tokens:StringResource, admin.topic.deletebutton %>'
				Visible="<%# TopicId != null %>" 
				OnClick="btnDelete_Click"
				data-confirmation-prompt="Are you sure you want to delete this topic?"
				CausesValidation="false" />

			<asp:HyperLink runat="server"
				CssClass="btn btn-default"
				NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>"
				Text="<%$Tokens:StringResource, admin.common.close %>" />

			<asp:Button runat="server" CssClass="btn btn-default js-save-and-close" OnClick="btnSaveAndClose_Click" Text="<%$ Tokens: StringResource, admin.common.SaveAndClose %>" />
			<asp:Button runat="server" ID="btnSave" CssClass="btn btn-primary js-save" OnClick="btnSave_Click" Text='<%$ Tokens:StringResource, admin.Common.Save %>' />
		</div>

		<div class="white-ui-box">
			<div class="tab-pane active" id="main-tab">
				<asp:Panel ID="pnlTopicEditor" runat="server">
					<div class="topic-edit">
						<div class="row form-group">
							<div class="col-sm-3">
								<span class="text-danger">*</span><asp:Literal runat="server" Text='<%$ Tokens:StringResource, admin.topic.name %>' />
							</div>
							<div class="col-sm-3">
								<div>
									<asp:TextBox runat="server" 
										ID="txtTopicName" 
										CssClass="form-control" />
								</div>
								<asp:RequiredFieldValidator runat="server" 
									CssClass="text-danger" 
									ControlToValidate="txtTopicName" 
									EnableClientScript="true" 
									Display="Dynamic" 
									ErrorMessage="<%$ Tokens:StringResource, admin.common.FillOutNamePrompt %>" />
							</div>
						</div>
						<div class="row form-group">
							<div class="col-sm-3">
								<span class="text-danger">*</span><asp:Literal runat="server" Text='<%$ Tokens:StringResource, admin.topic.title %>' />
							</div>
							<div class="col-sm-3">
								<div>
									<asp:TextBox runat="server" 
										ID="txtTopicTitle" 
										CssClass="form-control" />
								</div>
								<asp:RequiredFieldValidator runat="server" 
									CssClass="text-danger" 
									ControlToValidate="txtTopicTitle" 
									EnableClientScript="true" 
									Display="Dynamic" 
									ErrorMessage="<%$ Tokens:StringResource, admin.common.TopicPageTitlePrompt %>" />
							</div>
						</div>
						<div class="row form-group">
							<div class="col-sm-3">
								<label>
									<asp:Literal runat="server" Text='<%$ Tokens:StringResource, admin.topic.description %>' /></label>
							</div>
							<div class="col-sm-9">
								<asp:TextBox ID="txtDescriptionNoHtmlEditor" TextMode="MultiLine" runat="server" Visible="false" CssClass="form-control" />
								<telerik:RadEditor runat="server" ID="radDescription" SkinID="RadEditorSettings" />
							</div>
						</div>
						<div class="row form-group">
							<div class="col-sm-3">
								<label>
									<asp:Literal runat="server" Text='<%$ Tokens:StringResource, admin.topic.publishedlabel %>' />
								</label>
							</div>
							<div class="col-sm-3">
								<asp:CheckBox ID="chkPublished" runat="server" />
							</div>
						</div>
						<div runat="server" id="trCopyToStore" visible="false" class="row form-group">
							<div class="col-sm-3">
								<label>
									<asp:Literal runat="server" Text='<%$ Tokens:StringResource, admin.topic.copytostore %>' />
								</label>
							</div>
							<div class="col-sm-3">
								<asp:DropDownList runat="server" ID="ddCopyToStore" CssClass="form-control" />
							</div>
							<div class="col-sm-6">
								<asp:Button runat="server" ID="btnCopyToStore" CssClass="btn btn-default btn-sm" Text="<%$ Tokens:StringResource, admin.common.Submit %>" OnClick="btnCopyToStore_Click" />
							</div>
						</div>
						<div class="row form-group">
							<div class="col-sm-3">
								<label>
									<asp:Literal runat="server" Text='<%$ Tokens:StringResource, admin.topic.displayorder %>' /></label>
							</div>
							<div class="col-sm-2">
								<asp:TextBox ID="txtDspOrdr" runat="server" CssClass="form-control" />
							</div>
						</div>
						<div class="row form-group">
							<div class="col-sm-3">
								<label>
									<asp:Literal runat="server" Text='<%$ Tokens:StringResource, admin.topic.setitle %>' /></label>
							</div>
							<div class="col-sm-3">
								<asp:TextBox runat="server" ID="ltSETitle" CssClass="form-control" />
							</div>
						</div>
						<div class="row form-group">
							<div class="col-sm-3">
								<label>
									<asp:Literal runat="server" Text='<%$ Tokens:StringResource, admin.topic.sekeywords %>' /></label>
							</div>
							<div class="col-sm-9">
								<asp:TextBox runat="server" ID="ltSEKeywords" CssClass="form-control" />
							</div>
						</div>
						<div class="row form-group">
							<div class="col-sm-3">
								<label>
									<asp:Literal runat="server" Text='<%$ Tokens:StringResource, admin.topic.sedescription %>' /></label>
							</div>
							<div class="col-sm-9">
								<asp:TextBox runat="server" ID="ltSEDescription" CssClass="form-control" />
							</div>
						</div>
						<div class="row form-group">
							<div class="col-sm-3">
								<label>
									<asp:Literal runat="server" Text='<%$ Tokens:StringResource, admin.topic.password %>' /></label>
							</div>
							<div class="col-sm-3">
								<asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" />
							</div>
							<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="Only required if you want to protect this topic content by requiring a password to be entered.">
								<i class="fa fa-question-circle fa-lg"></i>
							</asp:Label>
						</div>
						<div class="row form-group">
							<div class="col-sm-3">
								<label>
									<asp:Literal runat="server" Text='<%$ Tokens:StringResource, admin.topic.disclaimer %>' /></label>
							</div>
							<div class="col-sm-3">
								<asp:RadioButtonList ID="rbDisclaimer" runat="server" RepeatDirection="horizontal" CssClass="horizontal-radio-helper">
									<asp:ListItem Value="0" Selected="true" Text='<%$ Tokens:StringResource, admin.common.No %>' />
									<asp:ListItem Value="1" Text='<%$ Tokens:StringResource, admin.common.Yes %>' />
								</asp:RadioButtonList>
							</div>
						</div>
						<div class="row form-group">
							<div class="col-sm-3">
								<label>
									<asp:Literal runat="server" Text='<%$ Tokens:StringResource, admin.topic.sitemap %>' /></label>
							</div>
							<div class="col-sm-9">
								<asp:RadioButtonList ID="rbPublish" runat="server" RepeatDirection="horizontal" CssClass="horizontal-radio-helper">
									<asp:ListItem Value="0" Selected="true" Text='<%$ Tokens:StringResource, admin.common.No %>' />
									<asp:ListItem Value="1" Text='<%$ Tokens:StringResource, admin.common.Yes %>' />
								</asp:RadioButtonList>
							</div>
						</div>
						<asp:Panel runat="server" ID="storeMapperWrapper" Visible="false" class="row form-group">
							<div class="col-sm-3">
								<label>
									<asp:Literal runat="server" Text='<%$ Tokens:StringResource, admin.topic.mapstores %>' />
								</label>
							</div>
							<div class="col-sm-3">
								<aspdnsf:TopicToStore runat="server" ID="TopicStoreMapper" ShowText="false" SelectMode="SingleDropDown" ShowDefaultForAllStores="true" />
							</div>
						</asp:Panel>
						<div class="form-group">
							<label>
								<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.topic.isfrequent %>" /></label>
							<asp:CheckBox ID="chkIsFrequent" Checked="true" runat="server" />
						</div>
					</div>
				</asp:Panel>
				<div class="item-action-bar">
					<asp:Button runat="server" 
						CssClass="btn btn-danger js-confirm-prompt" 
						Text='<%$ Tokens:StringResource, admin.topic.deletebutton %>'
						Visible="<%# TopicId != null %>" 
						OnClick="btnDelete_Click"
						data-confirmation-prompt="Are you sure you want to delete this topic?"
						CausesValidation="false" />

					<asp:HyperLink runat="server"
						CssClass="btn btn-default"
						NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>"
						Text="<%$Tokens:StringResource, admin.common.close %>" />

					<asp:Button runat="server" CssClass="btn btn-default js-save-and-close" OnClick="btnSaveAndClose_Click" Text="<%$ Tokens: StringResource, admin.common.SaveAndClose %>" />
					<asp:Button runat="server" CssClass="btn btn-primary js-save" OnClick="btnSave_Click" Text='<%$ Tokens:StringResource, admin.Common.Save %>' />
				</div>
			</div>
		</div>
	</asp:Panel>

</asp:Content>
