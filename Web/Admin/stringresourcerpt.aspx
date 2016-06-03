<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.stringresourcerpt" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="stringresourcerpt.aspx.cs" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<h1>
		<i class="fa fa-pencil-square-o"></i>
		<asp:Label ID="ReportLabel" runat="server" Text="Missing Strings" />
	</h1>
	<div>
		<aspdnsf:AlertMessage runat="server" ID="ctrlAlertMessage" />
	</div>
	<div class="row admin-row">
		<div class="col-sm-12">
			<asp:Literal ID="ltLocale" runat="server" />
		</div>
	</div>
	<div class="item-action-bar" id="topButtonPanel" runat="server" visible="false">
		<div class="col-list-action-bar">
			<asp:Button ID="btnExportExcel" runat="server" CssClass="btn btn-default" Text="<%$Tokens:StringResource, admin.common.ExportToExcel %>" OnClick="btnExportExcel_Click" />
			<asp:Button ID="btnSubmit" runat="server" CssClass="btn btn-primary" Text="<%$Tokens:StringResource, admin.common.save %>" OnClick="btnSubmit_Click" />
		</div>
	</div>
	<div class="white-ui-box">
		<asp:Literal ID="ltData" runat="server" />
	</div>
	<div class="item-action-bar" id="bottomButtonPanel" runat="server" visible="false">
		<div class="col-list-action-bar">
			<asp:Button ID="btnExportExcelBottom" runat="server" CssClass="btn btn-default" Text="<%$Tokens:StringResource, admin.common.ExportToExcel %>" OnClick="btnExportExcel_Click" />
			<asp:Button ID="btnSubmitBottom" runat="server" CssClass="btn btn-primary" Text="<%$Tokens:StringResource, admin.common.save %>" OnClick="btnSubmit_Click" />
		</div>
	</div>
</asp:Content>