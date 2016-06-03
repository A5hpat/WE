<%@ Page Language="c#" Inherits="AspDotNetStorefrontAdmin.recurringrefundcancel" MasterPageFile="~/App_Templates/Admin_Default/PopUp.master" Codebehind="recurringrefundcancel.aspx.cs" %>

<asp:Content ContentPlaceHolderID="bodyContentPlaceholder" runat="server">

	<aspdnsf:AlertMessage ID="AlertMessageGeneralStatus" runat="server" />
	<aspdnsf:AlertMessage ID="AlertMessageDisplayRefundStatus" runat="server" />
	<aspdnsf:AlertMessage ID="AlertMessageDisplayAutoBillStatus" runat="server" />

	<asp:Panel ID="pnlResults" runat="server" CssClass="white-ui-box" Visible="false">
		<div class="white-ui-box">
			<asp:Literal ID="ltReloadScript" runat="server" />
			<asp:Button CssClass="btn btn-default" OnClientClick="javascript:self.close();" Text="Close" runat="server" />
			<br />
		</div>
	</asp:Panel>
	<asp:Panel ID="pnlForm" runat="server" CssClass="white-ui-box" Visible="false">
		<div class="white-box-heading">
			<asp:Label ID="lblFormHeader" runat="server" />
		</div>
		<div class="form-group">
			<asp:Label Text="Reason" AssociatedControlID="txtRefundReason" runat="server" />
			<asp:TextBox ID="txtRefundReason" CssClass="text-md" MaxLength="100" runat="server" />
			<asp:Button ID="btnCancel" CssClass="btn btn-default" Text="Cancel" OnClientClick="javascript:self.close();" runat="server" />
			<asp:Button ID="btnSubmit" CssClass="btn btn-primary" Text="Submit" runat="server" />
		</div>
	</asp:Panel>
</asp:Content>
