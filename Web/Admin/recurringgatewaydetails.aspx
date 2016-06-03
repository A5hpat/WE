<%@ Page Language="c#" Inherits="AspDotNetStorefrontAdmin.recurringgatewaydetails" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="recurringgatewaydetails.aspx.cs" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<h1>
		<i class="fa fa-table"></i>
		Recurring Subscription Gateway Details:
	</h1>
	<aspdnsf:AlertMessage runat="server" ID="ctrlAlertMessage" />

	<div class="white-ui-box">
		<div class="row">
			<asp:Panel ID="pnlOrderNumberInput" runat="server" DefaultButton="btnOrderNumber">
				<div class="form-group">
					<asp:Label runat="server" AssociatedControlID="txtOrderNumber" Text="Original Order Number" />
					<asp:TextBox ID="txtOrderNumber" runat="server" />
					<asp:Button ID="btnOrderNumber" runat="server" CssClass="btn btn-action" Text="Submit" OnClick="btnOrderNumber_Click" />
				</div>
			</asp:Panel>

			<asp:Panel ID="pnlSubscriptionIdInput" runat="server" Width="100%" DefaultButton="btnSubscriptionID">
				<div class="form-group">
					<asp:Label runat="server" AssociatedControlID="txtSubscriptionID" Text="Subscription ID" />
					<asp:TextBox ID="txtSubscriptionID" runat="server"></asp:TextBox>
					<asp:Button ID="btnSubscriptionID" runat="server" CssClass="btn btn-action" Text="Submit" OnClick="btnSubscriptionID_Click" />
				</div>
			</asp:Panel>

			<asp:Panel ID="pnlResults" CssClass="col-md-8" runat="server">
				<div class="form-group">
					<asp:Label ID="lblResults" Text="Results from Gateway:" runat="server" />
					<asp:TextBox ID="txtResults" TextMode="MultiLine" Rows="10" CssClass="form-control" runat="server" />
				</div>
			</asp:Panel>
		</div>
	</div>
</asp:Content>
