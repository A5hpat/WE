<%@ Page Language="c#" Inherits="AspDotNetStorefrontAdmin.auditlog" EnableEventValidation="false" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="auditlog.aspx.cs" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<span>
		<asp:Label ID="lblCustomer" runat="server" Font-Bold="true"></asp:Label>
	</span>

	<h1>
		<i class="fa fa-asterisk"></i>
		<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.title.auditlog %>" />
	</h1>

	<div class="white-ui-box">
		<asp:GridView
			ID="GridView1" runat="server"
			CssClass="table"
			AllowPaging="True"
			PageSize="15"
			CellPadding="0"
			GridLines="None"
			ShowFooter="True"
			OnPageIndexChanging="GridView1_PageIndexChanging">
			<FooterStyle CssClass="gridFooter" />
			<RowStyle CssClass="table-row2" />
			<PagerStyle CssClass="gridPager" HorizontalAlign="Left" />
			<HeaderStyle CssClass="gridHeader" />
			<AlternatingRowStyle CssClass="table-alternatingrow2" />
		</asp:GridView>
	</div>
</asp:Content>

