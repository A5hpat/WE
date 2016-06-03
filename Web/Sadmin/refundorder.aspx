<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.refundorder" Codebehind="refundorder.aspx.cs" %>
<%@ Import Namespace="AspDotNetStorefrontCore" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<script src="Scripts/jquery.min.js" type="text/javascript"></script>
	<script src="Scripts/bootstrap.min.js" type="text/javascript"></script>
	<title></title>
</head>

<body class="body-tag main-content-wrapper">
	<form runat="server">
	<div class="container-fluid">
		<h1>
			<i class="fa fa-undo"></i>
			<asp:Label ID="lblHeader" runat="server" Text="<%$Tokens:StringResource, admin.title.refundorder %>" />
		</h1>
		<div class="white-ui-box">
			<aspdnsf:AlertMessage runat="server" ID="ctrlAlertMessage" />

			<div runat="server" Visible='<%# !Order.IsEmpty && !IsOrderRefunded(Order) %>'>
				<div>
					<asp:Label ID="lblForce" runat="server" Text="<%$Tokens:StringResource, admin.refund.forcelabel %>" Visible='<%# CommonLogic.QueryStringBool("Force") %>' />
					<div class="row admin-row">
						<div class="col-sm-4">
							<asp:Label runat="server" AssociatedControlID="lblOrderNumber" Text="<%$Tokens:StringResource, admin.refund.FullyRefundOrder %>" />
						</div>
						<div class="col-sm-8">
							<asp:Label ID="lblOrderNumber" runat="server" Text='<%# Order.OrderNumber %>' />
						</div>
					</div>
					<div>
						<div class="row admin-row">
							<div class="col-sm-4">
								<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.refund.Reason%>" />
							</div>
							<div class="col-sm-8">
								<asp:TextBox ID="txtReason" runat="server" type="text" class="text-lg" MaxLength="100" />
							</div>
						</div>
					</div>
				</div>
			</div>
		</div>

		<div class="item-action-bar">
			<div class="col-list-action-bar">
				<asp:Button ID="btnRefund" runat="server"
					CssClass="btn btn-primary"
					Text="<%$Tokens:StringResource, admin.refund.ProcessRefund %>"
					Visible='<%# !Order.IsEmpty && !IsOrderRefunded(Order) %>'
					OnClick="btnRefund_Click" />

				<asp:Button ID="btnCancel" runat="server"
					CssClass="btn btn-default"
					Text='<%# Order.IsEmpty || IsOrderRefunded(Order)
								? AspDotNetStorefrontCore.AppLogic.GetString("admin.common.close", ThisCustomer.LocaleSetting)
								: AspDotNetStorefrontCore.AppLogic.GetString("admin.common.cancel", ThisCustomer.LocaleSetting)%>'
					OnClientClick="return refreshParent();" />
			</div>
		</div>
	</div>

	<script type="text/javascript">
		function refreshParent() {
			var orderIsEmpty = <%= Order.IsEmpty.ToString().ToLower() %>;
			if (!orderIsEmpty)
				window.opener.location.href = '<%=string.Format("{0}?ordernumber={1}", AspDotNetStorefrontCore.AppLogic.AdminLinkUrl("order.aspx"), Order.OrderNumber)%>';

			self.close();
			return false;
		}
	</script>
	</form>
</body>
</html>