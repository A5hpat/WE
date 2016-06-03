<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.importstringresourcefile2" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="importstringresourcefile2.aspx.cs" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<h1>
		<i class="fa fa-pencil-square-o"></i>
		<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.stringresources.VerifyFile %>" />
	</h1>

	<aspdnsf:AlertMessage runat="server" ID="AlertMessageControl" />

	<div class="white-ui-box">
		<asp:Literal runat="server" ID="ltProcessing" />
		<asp:Panel runat="server" ID="ActionsPanel" CssClass="form-group">
			<asp:Literal runat="server" ID="ltVerify" Text="<%$Tokens:StringResource, admin.importstringresourcefile2.Good %>" />
			<asp:LinkButton runat="Server" ID="btnProcessFile" OnClick="btnProcessFile_Click" CssClass="btn btn-primary" />
			<asp:HyperLink runat="server" ID="CancelLink" CssClass="btn btn-default" Text="<%$Tokens:StringResource, admin.importstringresourcefile2.CancelReload %>" />
		</asp:Panel>
		
		<asp:GridView runat="server" 
			ID="DataReportGrid" 
			CssClass="table table-fixed"
			GridLines="None"
			AutoGenerateColumns="false"
			ShowFooter="false"
			OnRowDataBound="DataReportGrid_RowDataBound"
			OnDataBinding="DataReportGrid_DataBinding">
			<Columns>
				<asp:BoundField 
					HeaderStyle-Width="5%"
					HeaderText="<%$ Tokens:StringResource, admin.importstringresourcefile2.Row %>"
					DataField="Index" />

				<asp:BoundField 
					HeaderStyle-Width="10%"
					HeaderText="<%$ Tokens:StringResource, admin.importstringresourcefile2.Status %>"
					DataField="Status" />

				<asp:BoundField 
					HeaderStyle-Width="20%"
					HeaderText="<%$ Tokens:StringResource, admin.common.Name %>"
					DataField="Name" />

				<asp:BoundField 
					HeaderStyle-Width="5%"
					HeaderText="<%$ Tokens:StringResource, admin.common.Locale %>"
					DataField="LocaleSetting" />

				<asp:BoundField 
					HeaderText="<%$ Tokens:StringResource, admin.importstringresourcefile2.StringValue %>"
					DataField="Value" />

				<asp:BoundField 
					HeaderStyle-Width="5%"
					HeaderText="<%$ Tokens:StringResource, admin.importstringresourcefile2.StoreID %>"
					DataField="StoreId" />
					<%--Visible="<%# AspDotNetStorefrontCore.Store.IsMultiStore %>"--%>
			</Columns>
		</asp:GridView>
		
		<asp:HyperLink runat="server" NavigateUrl='<%# AspDotNetStorefrontCore.AppLogic.AdminLinkUrl("StringResources.aspx") + "?filterlocale=" + ShowLocaleSetting %>' Text="<%$ Tokens:StringResource, admin.importstringresourcefile2.Back %>" />
	</div>

</asp:Content>
