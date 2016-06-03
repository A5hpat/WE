// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways;
using AspDotNetStorefrontGateways.Processors;
using AspDotNetStorefrontControls;

namespace AspDotNetStorefrontAdmin
{
	/// <summary>
	/// Summary description for recurringgatewaydetails.
	/// </summary>
	public partial class recurringgatewaydetails : AspDotNetStorefront.Admin.AdminPageBase
	{
		protected void Page_Load(object sender, System.EventArgs e)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");

			Customer ThisCustomer = Context.GetCustomer();

			pnlOrderNumberInput.Visible = false;
			pnlSubscriptionIdInput.Visible = false;
			pnlResults.Visible = false;

			if(!ThisCustomer.IsAdminUser) // safety check
			{
				ctrlAlertMessage.PushAlertMessage("PERMISSION DENIED", AlertMessage.AlertType.Error);
			}
			else
			{
				String RecurringSubscriptionID = CommonLogic.QueryStringCanBeDangerousContent("RecurringSubscriptionID");
				int OriginalRecurringOrderNumber = CommonLogic.QueryStringUSInt("OriginalRecurringOrderNumber");

				if(RecurringSubscriptionID.Length == 0 && OriginalRecurringOrderNumber > 0)
				{
					RecurringSubscriptionID = AppLogic.GetRecurringSubscriptionIDFromOrder(OriginalRecurringOrderNumber);
				}
				if(OriginalRecurringOrderNumber == 0 && RecurringSubscriptionID.Length != 0)
				{
					OriginalRecurringOrderNumber = AppLogic.GetOriginalRecurringOrderNumberFromSubscriptionID(RecurringSubscriptionID);
				}

				if(OriginalRecurringOrderNumber == 0 || RecurringSubscriptionID.Length == 0)
				{
					ctrlAlertMessage.PushAlertMessage("Need Original Order Number or Subscription ID", AlertMessage.AlertType.Error);

					pnlOrderNumberInput.Visible = true;
					pnlSubscriptionIdInput.Visible = true;
				}
				else if(OriginalRecurringOrderNumber > 0 && RecurringSubscriptionID.Length == 0)
				{
					ctrlAlertMessage.PushAlertMessage("Subscription ID was not found for Order Number", AlertMessage.AlertType.Error);
					pnlOrderNumberInput.Visible = true;
					pnlSubscriptionIdInput.Visible = true;
				}
				else
				{
					pnlResults.Visible = true;
					String GW = AppLogic.ActivePaymentGatewayCleaned();

					if(GW == Gateway.ro_GWPAYFLOWPRO)
					{
						txtResults.Text = PayFlowProController.RecurringBillingInquiryDisplay(RecurringSubscriptionID);
					}
					else
					{
						ctrlAlertMessage.PushAlertMessage("Gateway " + GW + " not supported.", AlertMessage.AlertType.Error);
					}
				}
			}
			Page.Form.DefaultFocus = txtOrderNumber.ClientID;
		}

		protected void btnOrderNumber_Click(object sender, EventArgs e)
		{
			Response.Redirect(AppLogic.AdminLinkUrl("recurringgatewaydetails.aspx") + "?OriginalRecurringOrderNumber=" + txtOrderNumber.Text);
		}
		protected void btnSubscriptionID_Click(object sender, EventArgs e)
		{
			Response.Redirect(AppLogic.AdminLinkUrl("recurringgatewaydetails.aspx") + "?RecurringSubscriptionID=" + txtSubscriptionID.Text);
		}
	}
}
