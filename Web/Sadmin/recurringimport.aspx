<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.recurringimport" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="recurringimport.aspx.cs" %>
<%@ Register Src="Controls/LinkGroupRecurring.ascx" TagPrefix="aspdnsf" TagName="LinkGroupRecurring" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<h1>
		<i class="fa fa-table"></i>
		<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.orderrecurringimport %>" />
	</h1>
	<aspdnsf:AlertMessage runat="server" ID="ctrlAlertMessage" />
	<div id="divUnsupportedWarning" visible="false" runat="server" class="alert alert-warning">
		<asp:Literal runat="server" ID="litUnsupportedWarning" />
	</div>
	<div class="item-action-bar">
		<aspdnsf:LinkGroupRecurring runat="server" ID="LinkGroupRecurring" SelectedLink="recurringimport.aspx" />
		<asp:Button ID="btnGetGatewayStatus" CssClass="btn btn-primary" runat="server" Text="<%$Tokens:StringResource, admin.recurringimport.GetTodaysStatusFile %>" OnClick="btnGetGatewayStatus_Click" />
	</div>

	<asp:Panel ID="pnlMain" runat="server" DefaultButton="btnProcessFile">
		
		<asp:Panel runat="server" Visible="false" ID="LastRunPanel" class="alert alert-info">
			<asp:Label ID="lblLastRun" runat="server" />
		</asp:Panel>
		
		<div class="white-ui-box">
			<div class="white-box-heading">
				<asp:Label ID="PastePromptLabel" AssociatedControlID="txtInputFile" runat="server" Text="<%$Tokens:StringResource, admin.recurringimport.PasteGatewayAutobill %>" />
			</div>
			<div class="form-group">
				<asp:TextBox ID="txtInputFile" runat="server" Height="300px" CssClass="text-lg" TextMode="MultiLine" />
			</div>

			<div class="item-action-bar">
				<asp:Button ID="btnProcessFile" CssClass="btn btn-action" runat="server" Text="<%$Tokens:StringResource, admin.recurringimport.ProcessRecords %>" OnClick="btnProcessFile_Click" Visible="True" />
			</div>
		</div>
		<div class="white-ui-box">
			<div class="white-box-heading">
				<asp:Label runat="server" AssociatedControlID="txtResults" Text="<%$Tokens:StringResource, admin.recurringimport.ProcessingResultsWillGoHere %>" />
			</div>

			<div class="form-group">
				<asp:TextBox ID="txtResults" runat="server" Height="300px" CssClass="text-lg" TextMode="MultiLine" Visible="False" />
			</div>
		</div>
	</asp:Panel>

</asp:Content>
