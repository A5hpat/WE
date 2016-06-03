// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways;

namespace AspDotNetStorefrontAdmin
{
	public partial class refundorder : AspDotNetStorefront.Admin.AdminPageBase
	{
		protected Order Order { get; set; }

		protected void Page_Load(object sender, EventArgs e)
		{
			Order = new Order(CommonLogic.QueryStringUSInt("OrderNumber"), ThisCustomer.LocaleSetting);

			if(Order.IsEmpty)
				ctrlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.refund.InvalidOrderNumber", SkinID, LocaleSetting), AlertMessage.AlertType.Error);
			else if(IsOrderRefunded(Order))
				ctrlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.refund.OrderWasRefunded", SkinID, LocaleSetting), AlertMessage.AlertType.Success);
			
            DataBind();
		}

		protected void btnRefund_Click(object sender, EventArgs e)
		{
			var result = ProcessRefund(
				order: Order,
				reason: txtReason.Text,
				forceRefund: CommonLogic.QueryStringBool("Force"),
				localeSetting: LocaleSetting);

			if(result == AppLogic.ro_OK)
				ctrlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.refund.OrderWasRefunded", SkinID, LocaleSetting), AlertMessage.AlertType.Success);
			else
				ctrlAlertMessage.PushAlertMessage(AppLogic.GetString(result, SkinID, LocaleSetting), AlertMessage.AlertType.Info);

			Order = new Order(CommonLogic.QueryStringUSInt("OrderNumber"), ThisCustomer.LocaleSetting);
			DataBind();
		}

		protected bool IsOrderRefunded(Order order)
		{
			return Order.TransactionState == AspDotNetStorefrontCore.AppLogic.ro_TXStateRefunded || Order.RefundedOn != DateTime.MinValue;
		}

		string ProcessRefund(Order order, string reason, bool forceRefund, string localeSetting)
		{
			return forceRefund
				? Gateway.OrderManagement_DoForceFullRefund(order)
				: Gateway.OrderManagement_DoFullRefund(order, reason);
		}
	}
}
