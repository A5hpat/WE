<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.NewsEditor" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="newseditor.aspx.cs" %>

<%@ Register TagPrefix="aspdnsf" TagName="EntityToStore" Src="Controls/EntityToStoreMapper.ascx" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<aspdnsf:ReturnUrlTracker runat="server" ID="ReturnUrlTracker" DefaultReturnUrl="news.aspx" />
	<div class="admin-module">
		<h1>
			<i class="fa fa-newspaper-o"></i>
			<asp:Literal ID="HeaderText" runat="server" />
		</h1>
		
		<aspdnsf:AlertMessage ID="ctlAlertMessage" runat="server" />	

		<div class="item-action-bar">
			<asp:Panel runat="server" ID="pnlLocale" Visible="<%# LocaleSelector.HasMultipleLocales() %>" CssClass="other-actions" >
				<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.stringresources.Locale %>" AssociatedControlID="LocaleSelector" />
				<aspdnsf:LocaleSelector runat="server" ID="LocaleSelector" OnSelectedLocaleChanged="LocaleSelector_SelectedLocaleChanged" />
			</asp:Panel>
			<asp:HyperLink runat="server" ID="btnCloseTop" CssClass="btn btn-default" NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>" Text="<%$Tokens:StringResource, admin.common.close %>" />
			<asp:Button ID="btnSaveAndCloseTop" runat="server" CssClass="btn btn-default" OnClick="btnSaveAndClose_Click" ValidationGroup="gAdd" Text="<%$Tokens:StringResource, admin.common.SaveAndClose %>" />
			<asp:Button ID="btnSubmitTop" runat="server" CssClass="btn btn-primary" OnClick="btnSubmit_Click" ValidationGroup="gAdd" Text="<%$Tokens:StringResource, admin.common.Save %>" />
		</div>

		<div class="admin-row">
			<div id="divEditNews" class="white-ui-box">
				<div class="row">
					<div class="col-md-4">
						<div class="form-inline">
							<asp:Label AssociatedControlID="litNewsId" for="litNewsId" runat="server" Text="<%$Tokens:StringResource, admin.common.ID %>" />:
							<asp:Literal ID="litNewsId" runat="server" />
						</div>
						<div class="form-group">
							<span class="text-danger">*</span><asp:Label AssociatedControlID="txtHeadline" for="txtHeadline" runat="server" Text="<%$Tokens:StringResource, admin.editnews.Headline %>" />
							<asp:TextBox ID="txtHeadline" CssClass="form-control" runat="server" />
							<asp:RequiredFieldValidator ErrorMessage="<%$Tokens:StringResource, admin.news.HeadlineRequired %>" CssClass="text-danger" ControlToValidate="txtHeadline" ID="RequiredFieldValidator9" ValidationGroup="gAdd" SetFocusOnError="true" runat="server" />
						</div>
						<div class="form-group">
							<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editnews.NewsCopy %>" />
							<asp:TextBox ID="txtCopy" runat="server" Visible="false" TextMode="MultiLine" Rows="10" CssClass="text-multiline form-control" />
							<telerik:RadEditor id="radCopy" runat="server" SkinID="RadEditorSettings" />
						</div>
						<div class="form-group">
							<span class="text-danger">*</span><asp:Label AssociatedControlID="txtDate" for="txtDate" runat="server" Text="<%$Tokens:StringResource, admin.editnews.ExpirationDate %>" />
							<telerik:RadDatePicker ID="txtDate" runat="server" Style="z-index: 150000;" MaxDate="9999-12-31" MinDate="0001-01-01">
								<Calendar UseColumnHeadersAsSelectors="False" UseRowHeadersAsSelectors="False" ViewSelectorText="x" runat="server" />
							</telerik:RadDatePicker>
							<asp:RequiredFieldValidator ErrorMessage="<%$Tokens:StringResource, admin.news.ExpirationRequired %>" CssClass="text-danger" ControlToValidate="txtDate" ID="RequiredFieldValidator1" ValidationGroup="gAdd" SetFocusOnError="true" runat="server" />
						</div>
						<div class="form-inline">
							<span class="text-danger">*</span><asp:Label AssociatedControlID="cbxPublished" for="cbxPublished" runat="server" Text="<%$Tokens:StringResource, admin.Common.Published %>" />:
							<asp:CheckBox ID="cbxPublished" Checked="true" runat="server" />
						</div>
						<div runat="server" visible="false" id="divStoreMapping" class="form-group">
							<span class="text-danger">*</span><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.topic.mapstores %>" />
							<aspdnsf:EntityToStore runat="server" ID="StoresMapping" EntityType="News" ShowText="false" />
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