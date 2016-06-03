// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Text;
using System.Web.Mvc;
using AspDotNetStorefront.Routing;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways.Processors;

namespace AspDotNetStorefrontAdmin
{
	/// <summary>
	/// Summary description for customer_history.
	/// </summary>
	public partial class customer_history : AspDotNetStorefront.Admin.AdminPageBase
	{

		private Customer TargetCustomer;
		bool authorized = true;

		protected void Page_Load(object sender, System.EventArgs e)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");
			int targetCustomerId = CommonLogic.QueryStringUSInt("CustomerID");

			if (targetCustomerId == 0)
			{
				Response.Redirect(AppLogic.AdminLinkUrl("customers.aspx"));
			}

			TargetCustomer = new Customer(targetCustomerId, true);
			if (TargetCustomer.IsAdminSuperUser && !ThisCustomer.IsAdminSuperUser)
			{
				authorized = false;
			}
			RenderHtml();
		}

		private void RenderHtml()
		{
			StringBuilder writer = new StringBuilder();
			if (authorized)
			{
				//If there is a DeleteID remove it from the cart
				int DeleteRecurringOrderNumber = CommonLogic.QueryStringUSInt("DeleteID");
				String DeleteRecurringOrderResult = String.Empty;
				if (DeleteRecurringOrderNumber != 0)
				{
					Order originalOrder = new Order(DeleteRecurringOrderNumber);
					RecurringOrderMgr rmgr = new RecurringOrderMgr();

					ExpressAPIType expressApiType = PayPalController.GetAppropriateExpressType();

					if (originalOrder.PaymentMethod == AppLogic.ro_PMPayPalExpress && expressApiType == ExpressAPIType.PayPalExpress)
						DeleteRecurringOrderResult = rmgr.CancelPPECRecurringOrder(DeleteRecurringOrderNumber, false);
					else
						DeleteRecurringOrderResult = rmgr.CancelRecurringOrder(DeleteRecurringOrderNumber);
				}

				//If there is a FullRefundID refund it
				int FullRefundID = CommonLogic.QueryStringUSInt("FullRefundID");
				String FullRefundResult = String.Empty;
				if (FullRefundID != 0)
				{
					RecurringOrderMgr rmgr = new RecurringOrderMgr();
					FullRefundResult = rmgr.ProcessAutoBillFullRefund(FullRefundID);
				}

				//If there is a PartialRefundID refund it
				int PartialRefundID = CommonLogic.QueryStringUSInt("PartialRefundID");
				String PartialRefundResult = String.Empty;
				if (PartialRefundID != 0)
				{
					RecurringOrderMgr rmgr = new RecurringOrderMgr();
					PartialRefundResult = rmgr.ProcessAutoBillPartialRefund(PartialRefundID);
				}

				//If there is a retrypaymentid, retry it
				int RetryPaymentID = CommonLogic.QueryStringUSInt("retrypaymentid");
				String RetryPaymentResult = String.Empty;
				if (RetryPaymentID != 0)
				{
					RecurringOrderMgr rmgr = new RecurringOrderMgr();
					RetryPaymentResult = rmgr.ProcessAutoBillRetryPayment(RetryPaymentID);
				}

				//If there is a restartid, restart it
				int RestartPaymentID = CommonLogic.QueryStringUSInt("restartid");
				String RestartPaymentResult = String.Empty;
				if (RestartPaymentID != 0)
				{
					RecurringOrderMgr rmgr = new RecurringOrderMgr();
					RestartPaymentResult = rmgr.ProcessAutoBillRestartPayment(RestartPaymentID);
				}

				if (AppLogic.AppConfigBool("AuditLog.Enabled"))
				{
					writer.Append("<p><a href=\"" + AppLogic.AdminLinkUrl("auditlog.aspx") + "?CustomerID=" + TargetCustomer.CustomerID.ToString() + "\">View Customer Activity Log</a></p>\n");
				}

				if (ShoppingCart.NumItems(TargetCustomer.CustomerID, CartTypeEnum.RecurringCart) != 0)
				{

					writer.Append("<p align=\"left\"><b>" + AppLogic.GetString("admin.common.CstMsg9", SkinID, LocaleSetting) + "</b></p>\n");

					// build JS code to show/hide address update block:
					StringBuilder tmpS = new StringBuilder(4096);
					tmpS.Append("<script type=\"text/javascript\">\n");
					tmpS.Append("function toggleLayer(DivID)\n");
					tmpS.Append("{\n");
					tmpS.Append("	var elem;\n");
					tmpS.Append("	var vis;\n");
					tmpS.Append("	if(document.getElementById)\n");
					tmpS.Append("	{\n");
					tmpS.Append("		// standards\n");
					tmpS.Append("		elem = document.getElementById(DivID);\n");
					tmpS.Append("	}\n");
					tmpS.Append("	else if(document.all)\n");
					tmpS.Append("	{\n");
					tmpS.Append("		// old msie versions\n");
					tmpS.Append("		elem = document.all[DivID];\n");
					tmpS.Append("	}\n");
					tmpS.Append("	else if(document.layers)\n");
					tmpS.Append("	{\n");
					tmpS.Append("		// nn4\n");
					tmpS.Append("		elem = document.layers[DivID];\n");
					tmpS.Append("	}\n");
					tmpS.Append("	vis = elem.style;\n");
					tmpS.Append("	if(vis.display == '' && elem.offsetWidth != undefined && elem.offsetHeight != undefined)\n");
					tmpS.Append("	{\n");
					tmpS.Append("		vis.display = (elem.offsetWidth != 0 && elem.offsetHeight != 0) ? 'block' : 'none';\n");
					tmpS.Append("	}\n");
					tmpS.Append("	vis.display = (vis.display == '' || vis.display == 'block') ? 'none' : 'block' ;\n");
					tmpS.Append("}\n");
					tmpS.Append("</script>\n");
					tmpS.Append("\n");
					tmpS.Append("<style type=\"text/css\">\n");
					tmpS.Append("	.addressBlockDiv { margin: 0px 20px 0px 20px;  display: none;}\n");
					tmpS.Append("</style>\n");
					writer.Append(tmpS.ToString());

					var parser = new Parser();

					using (var dbconn = DB.dbConn())
					{
						dbconn.Open();
						using (var rsr = DB.GetRS("Select distinct OriginalRecurringOrderNumber from ShoppingCart   with (NOLOCK)  where CartType=" + ((int)CartTypeEnum.RecurringCart).ToString() + " and CustomerID=" + TargetCustomer.CustomerID.ToString() + " order by OriginalRecurringOrderNumber desc", dbconn))
						{
							while (rsr.Read())
							{
								bool ShowCancelButton = true;
								bool ShowRetryButton = false;
								bool ShowRestartButton = false;
								String GatewayStatus = String.Empty;

								RecurringOrderMgr rmgr1 = new RecurringOrderMgr();
								rmgr1.ProcessAutoBillGetAdminButtons(DB.RSFieldInt(rsr, "OriginalRecurringOrderNumber"), out ShowCancelButton, out ShowRetryButton, out ShowRestartButton, out GatewayStatus);

								if (DeleteRecurringOrderNumber == DB.RSFieldInt(rsr, "OriginalRecurringOrderNumber"))
								{
									writer.Append("<table class=\"table\">\n");
									writer.Append("<tr><td>\n");
									writer.Append("<span class=\"h4\">" + AppLogic.GetString("admin.cst_history.StopBillingResult", SkinID, LocaleSetting) + " " + DeleteRecurringOrderResult + "</span>\n");
									writer.Append("</td></tr>\n");
									writer.Append("</table>\n");
								}

								if (FullRefundID == DB.RSFieldInt(rsr, "OriginalRecurringOrderNumber"))
								{
									writer.Append("<table class=\"table\">\n");
									writer.Append("<tr><td>\n");
									writer.Append("<span class=\"h4\">" + AppLogic.GetString("admin.cst_history.FullRefundResult", SkinID, LocaleSetting) + " " + FullRefundResult + "</span>\n");
									writer.Append("</td></tr>\n");
									writer.Append("</table>\n");
								}

								if (PartialRefundID == DB.RSFieldInt(rsr, "OriginalRecurringOrderNumber"))
								{
									writer.Append("<table class=\"table\">\n");
									writer.Append("<tr><td>\n");
									writer.Append("<span class=\"h4\">" + AppLogic.GetString("admin.cst_history.PartialRefundResult", SkinID, LocaleSetting) + " " + PartialRefundResult + "</span>\n");
									writer.Append("</td></tr>\n");
									writer.Append("</table>\n");
								}

								if (RetryPaymentID == DB.RSFieldInt(rsr, "OriginalRecurringOrderNumber"))
								{
									writer.Append("<table class=\"table\">\n");
									writer.Append("<tr><td>\n");
									writer.Append("<span class=\"h4\">" + AppLogic.GetString("admin.cst_history.RetryPaymentResult", SkinID, LocaleSetting) + " " + RetryPaymentResult + "</span>\n");
									writer.Append("</td></tr>\n");
									writer.Append("</table>\n");
								}

								if (RestartPaymentID == DB.RSFieldInt(rsr, "OriginalRecurringOrderNumber"))
								{
									writer.Append("<table class=\"table\">\n");
									writer.Append("<tr><td>\n");
									writer.Append("<span class=\"h4\">" + AppLogic.GetString("admin.cst_history.RestartPaymentResult", SkinID, LocaleSetting) + " " + RestartPaymentResult + "</span>\n");
									writer.Append("</td></tr>\n");
									writer.Append("</table>\n");
								}

								writer.Append(AppLogic.GetRecurringCart(parser, TargetCustomer, DB.RSFieldInt(rsr, "OriginalRecurringOrderNumber"), SkinID, false, ShowCancelButton, ShowRetryButton, ShowRestartButton, GatewayStatus));
							}
						}
					}
				}

				writer.Append("<div class=\"admin-module\">\n");
				writer.Append("<h3>" + AppLogic.GetString("admin.orderframe.OrderHistory", SkinID, LocaleSetting) + "</h3>\n");

				int N = 0;

				writer.Append("<table class=\"table\">\n");
				writer.Append("<tr>\n");
				writer.Append("<td><b>" + AppLogic.GetString("admin.common.OrderNumber", SkinID, LocaleSetting) + "</b></td>\n");
				writer.Append("<td><b>" + AppLogic.GetString("admin.common.OrderDate", SkinID, LocaleSetting) + "</b></td>\n");
				writer.Append("<td><b>" + AppLogic.GetString("admin.cst_history.PaymentStatus", SkinID, LocaleSetting) + "</b></td>\n");
				writer.Append("<td><b>" + AppLogic.GetString("admin.common.ShippingStatus", SkinID, LocaleSetting) + "</b></td>\n");
				writer.Append("<td><b>" + AppLogic.GetString("admin.common.OrderTotal", SkinID, LocaleSetting) + "</b></td>\n");
				if (AppLogic.AppConfigBool("ShowCustomerServiceNotesInReceipts"))
				{
					writer.Append("<td><b>" + AppLogic.GetString("admin.cst_history.CustomerServiceNotes", SkinID, LocaleSetting) + "</b></td>\n");
				}
				writer.Append("</tr>\n");

				using (var dbconn = DB.dbConn())
				{
					dbconn.Open();
					using (var rs = DB.GetRS("Select '' Failed, PaymentGateway, PaymentMethod, ShippedOn, ShippedVIA, ShippingTrackingNumber, OrderNumber, OrderDate, OrderTotal, cast(CustomerServiceNotes as nvarchar(4000)) CustomerServiceNotes, TransactionState, DownloadEMailSentOn, CustomerID, RecurringSubscriptionID from orders  with (NOLOCK)  where CustomerID=" + TargetCustomer.CustomerID.ToString()
					+ " union select 'Failed' Failed, PaymentGateway, PaymentMethod, null ShippedOn, null ShippedVIA, null ShippingTrackingNumber, OrderNumber, OrderDate, null OrderTotal, cast(TransactionResult as nvarchar(4000)) CustomerServiceNotes, null TransactionState, null DownloadEMailSentOn, CustomerID, RecurringSubscriptionID  from FailedTransaction  with (NOLOCK)  where CustomerID=" + TargetCustomer.CustomerID.ToString()
					+ " order by OrderDate desc", dbconn))
					{
						while (rs.Read())
						{
							String PaymentStatus = String.Empty;
							if (DB.RSField(rs, "PaymentMethod").Length != 0)
							{
								PaymentStatus = AppLogic.GetString("admin.order.PaymentMethod", SkinID, LocaleSetting) + " " + DB.RSField(rs, "PaymentMethod") + "<br/>";
							}
							else
							{
								PaymentStatus = AppLogic.GetString("admin.order.PaymentMethod", SkinID, LocaleSetting) + " " + CommonLogic.IIF(DB.RSField(rs, "CardNumber").StartsWith(AppLogic.ro_PMPayPal, StringComparison.InvariantCultureIgnoreCase), AppLogic.ro_PMPayPal, "Credit Card") + "<br/>";
							}

							if (DB.RSField(rs, "RecurringSubscriptionID").Length > 0 && DB.RSField(rs, "PaymentGateway") == AspDotNetStorefrontGateways.Gateway.ro_GWPAYFLOWPRO)
							{ // include link to recurringgatewaydetails.aspx for live gateway status 
								PaymentStatus += "Subscription ID: <a href=\"" + AppLogic.AdminLinkUrl("recurringgatewaydetails.aspx") + "?RecurringSubscriptionID=" + DB.RSField(rs, "RecurringSubscriptionID") + "\">" + DB.RSField(rs, "RecurringSubscriptionID") + "</a><br/>";
							}

							String ShippingStatus = String.Empty;
							if (AppLogic.OrderHasShippableComponents(DB.RSFieldInt(rs, "OrderNumber")))
							{
								if (DB.RSFieldDateTime(rs, "ShippedOn") != System.DateTime.MinValue)
								{
									ShippingStatus = "Shipped";
									if (DB.RSField(rs, "ShippedVIA").Length != 0)
									{
										ShippingStatus += " via " + DB.RSField(rs, "ShippedVIA");
									}
									ShippingStatus += " on " + Localization.ToThreadCultureShortDateString(DB.RSFieldDateTime(rs, "ShippedOn")) + ".";
									if (DB.RSField(rs, "ShippingTrackingNumber").Length != 0)
									{
										ShippingStatus += " " + AppLogic.GetString("admin.orderframe.TrackingNumber", SkinID, LocaleSetting) + " ";

										String TrackURL = Shipping.GetTrackingURL(DB.RSField(rs, "ShippingTrackingNumber"));
										if (TrackURL.Length != 0)
										{
											ShippingStatus += "<a href=\"" + TrackURL + "\" target=\"_blank\">" + DB.RSField(rs, "ShippingTrackingNumber") + "</a>";
										}
										else
										{
											ShippingStatus += DB.RSField(rs, "ShippingTrackingNumber");
										}
									}
								}
								else
								{
									ShippingStatus = AppLogic.GetString("admin.cst_history.NotYetShipped", SkinID, LocaleSetting);
								}
							}
							if (AppLogic.OrderHasDownloadComponents(DB.RSFieldInt(rs, "OrderNumber"), true))
							{
								if (DB.RSField(rs, "TransactionState") == AppLogic.ro_TXStateCaptured && DB.RSFieldDateTime(rs, "DownloadEMailSentOn") != System.DateTime.MinValue)
								{
									if (ShippingStatus.Length != 0)
									{
										ShippingStatus += "<hr size=\"1\"/>";
									}
								}
								else
								{
									if (ShippingStatus.Length == 0)
									{
										ShippingStatus += AppLogic.GetString("admin.cst_history.DownloadListPendingPayment", SkinID, LocaleSetting);
									}
								}
							}
							writer.Append("<tr>\n");
							writer.Append("<td>");
							writer.Append("<a href=\"" + AppLogic.AdminLinkUrl("order.aspx") + "?ordernumber=" + DB.RSFieldInt(rs, "OrderNumber").ToString() + "\">" + DB.RSFieldInt(rs, "OrderNumber").ToString() + "</a>");
							writer.Append("<br/><br/>");
							if (string.IsNullOrEmpty(DB.RSField(rs, "Failed")))
							{
								var urlHelper = DependencyResolver.Current.GetService<UrlHelper>();
								writer.AppendFormat(@"<a href=""{0}"" target=""_blank"">{1}</a>",
									urlHelper.Action(
										actionName: ActionNames.Index,
										controllerName: ControllerNames.Receipt,
										routeValues: new
											{
												OrderNumber = DB.RSFieldInt(rs, "OrderNumber")
											}),
									AppLogic.GetString("admin.cst_history.PrintableReceipt", SkinID, LocaleSetting));
							}
							else
							{
								writer.Append("<font color=\"red\">" + DB.RSField(rs, "Failed") + "</font>");
							}
							writer.Append("</td>");
							writer.Append("<td>" + Localization.ToNativeDateTimeString(DB.RSFieldDateTime(rs, "OrderDate")));
							writer.Append("</td>");
							writer.Append("<td>" + PaymentStatus + "&nbsp;" + "</td>");
							writer.Append("<td>" + ShippingStatus + "&nbsp;" + "</td>");
							writer.Append("<td>" + ThisCustomer.CurrencyString(DB.RSFieldDecimal(rs, "OrderTotal")) + "</td>");
							if (AppLogic.AppConfigBool("ShowCustomerServiceNotesInReceipts"))
							{
								if (DB.RSField(rs, "CustomerServiceNotes").Length > 110)
								{
									writer.Append("<td><textarea READONLY rows=\"10\" cols=\"50\">" + DB.RSField(rs, "CustomerServiceNotes") + "</textarea></td>");
								}
								else
								{
									writer.Append("<td>" + CommonLogic.IIF(DB.RSField(rs, "CustomerServiceNotes").Length == 0, "None", DB.RSField(rs, "CustomerServiceNotes")) + "</td>");
								}
							}
							else
							{
								writer.Append("&nbsp;");
							}
							writer.Append("</tr>\n");
							N++;
						}
					}
				}
				writer.Append("</table>\n");
				if (N == 0)
				{
					writer.Append("<p align=\"left\">" + AppLogic.GetString("admin.common.NoOrdersFound", SkinID, LocaleSetting) + "</p>\n");
				}
			}
			ltContent.Text = writer.ToString();
		}
	}
}
