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
using AspDotNetStorefrontGateways.Processors;

namespace AspDotNetStorefrontAdmin
{
	public partial class recurringrefundcancel : AspDotNetStorefront.Admin.AdminPageBase
	{
		private int orderNumber
		{
			//defaults to 0
			get { return CommonLogic.QueryStringUSInt("OrderNumber"); }
		}

		protected void Page_Load(object sender, System.EventArgs e)
		{
			if(orderNumber == 0)
			{
				AlertMessageDisplayRefundStatus.PushAlertMessage("Order Number Required", AlertMessage.AlertType.Error);
				return;
			}

			if(IsPostBack)
			{
				pnlResults.Visible = true;
				pnlForm.Visible = false;
				ProcessRefund();
			}
			else
			{
				pnlForm.Visible = true;
				lblFormHeader.Text = String.Format("Are you sure you want to stop future billing and refund order {0}?", orderNumber);
				btnSubmit.PostBackUrl = AppLogic.AdminLinkUrl("recurringrefundcancel.aspx") + "?ordernumber=" + orderNumber.ToString();
			}
		}

		private void ProcessRefund()
		{
			var thisOrder = new Order(orderNumber, ThisCustomer.LocaleSetting);

			AlertMessageGeneralStatus.PushAlertMessage("CANCEL AUTO-BILL AND FULLY REFUND ORDER: " + orderNumber.ToString(), AlertMessage.AlertType.Info);

			String refundReason = Server.HtmlEncode(txtRefundReason.Text);
			String refundStatus = Gateway.OrderManagement_DoFullRefund(thisOrder, refundReason);

			if(refundStatus == AppLogic.ro_OK)
			{
				AlertMessageDisplayRefundStatus.PushAlertMessage("Refund Status = " + refundStatus, AlertMessage.AlertType.Success);
				CancelRecurringOrder(thisOrder);
			}
			else
				AlertMessageDisplayRefundStatus.PushAlertMessage("Refund Failed = " + refundStatus, AlertMessage.AlertType.Error);
		}

		private void CancelRecurringOrder(Order thisOrder)
		{
			var autoBillStatus = string.Empty;
			var recurringManager = new RecurringOrderMgr();

			if(thisOrder.ParentOrderNumber == 0)
				autoBillStatus = recurringManager.CancelRecurringOrder(orderNumber);
			else
				autoBillStatus = recurringManager.CancelRecurringOrder(thisOrder.ParentOrderNumber);


			if(autoBillStatus == AppLogic.ro_OK)
				AlertMessageDisplayAutoBillStatus.PushAlertMessage("Cancel Auto-Bill Status: " + autoBillStatus, AlertMessage.AlertType.Success);
			else
				AlertMessageDisplayAutoBillStatus.PushAlertMessage("Cancel Auto-Bill Failed: " + autoBillStatus, AlertMessage.AlertType.Error);
		}

	}
}
