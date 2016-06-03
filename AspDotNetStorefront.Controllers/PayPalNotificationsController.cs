// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;
using AspDotNetStorefront.Filters;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways;
using AspDotNetStorefrontGateways.Processors;

namespace AspDotNetStorefront.Controllers
{
	[SecureAccessFilter(forceHttps: true)]
	public class PayPalNotificationsController : Controller
	{
		public ActionResult Index(FormCollection collection)
		{
			if(!PostIsValid())
				return Content(string.Empty);

			var paymentStatus = collection["payment_status"] ?? string.Empty;
			var transactionId = collection["txn_id"] ?? string.Empty;
			var custom = collection["custom"] ?? string.Empty;
			var invoice = collection["invoice"] ?? string.Empty;
			var pendingReason = collection["pending_reason"] ?? string.Empty;
			var parentTransactionId = collection["parent_txn_id"] ?? string.Empty;
			var transactionType = collection["txn_type"] ?? string.Empty;
			var payerId = collection["payer_id"] ?? string.Empty;
			var profileId = collection["recurring_payment_id"] ?? string.Empty;
			var subscriptionId = collection["subscr_id"] ?? string.Empty;
			var paymentTotal = CommonLogic.FormNativeDecimal("mc_gross");

			//Invalid customer or missing info
			var customerId = Localization.ParseNativeInt(custom);
			if(customerId == 0)
				return Content(string.Empty);

			//Recurring notification
			if(transactionType.ToLowerInvariant().Contains("recurring")
				|| transactionType.ToLowerInvariant().Contains("subscr_cancel"))
			{
				HandlePayPalExpressCheckoutRecurringNotification(transactionType, payerId, profileId, subscriptionId);
			}

			// Normal notification
			var transactionState = PayPalController.GetTransactionState(paymentStatus, pendingReason);
			var existingOrderNumber = Localization.ParseNativeInt(invoice);

			if(existingOrderNumber > 0 && !Order.OrderExists(existingOrderNumber))
				existingOrderNumber = 0;

			if(existingOrderNumber == 0)    //Was it a PayPal Express order?
				existingOrderNumber = DB.GetSqlN(
					string.Format("SELECT MIN(OrderNumber) N FROM Orders WHERE (PaymentMethod = '{0}') AND CHARINDEX({1}, AuthorizationPNREF) > 0",
					AppLogic.ro_PMPayPalExpress,
					string.IsNullOrEmpty(parentTransactionId)
						? DB.SQuote(transactionId)
						: DB.SQuote(parentTransactionId)));

			if(existingOrderNumber == 0) //Last try - look up by paypal payments advanced checkout transaction
			{
				if(!string.IsNullOrEmpty(parentTransactionId))
					existingOrderNumber = OrderTransaction.LookupOrderNumber(null, null, null, null, parentTransactionId, null, null);
				else if(!string.IsNullOrEmpty(transactionId))
					existingOrderNumber = OrderTransaction.LookupOrderNumber(null, null, null, null, transactionId, null, null);
			}
			
			if(existingOrderNumber == 0)
				return Content(string.Empty);

			if(transactionState == AppLogic.ro_TXStateVoided)
			{
				VoidPPOrder(existingOrderNumber);
			}
			else if(transactionState == AppLogic.ro_TXStateCaptured)
			{
				CapturePPOrder(existingOrderNumber, transactionId, paymentTotal);
			}
			else if(transactionState == AppLogic.ro_TXStateRefunded)
			{
				RefundPPOrder(existingOrderNumber, transactionId, paymentTotal);
			}
			else if(transactionState == AppLogic.ro_TXStatePending)
			{
				DB.ExecuteSQL(string.Format("UPDATE Orders SET CapturedOn = NULL, TransactionState = {0} WHERE OrderNumber = {1}", DB.SQuote(AppLogic.ro_TXStatePending), existingOrderNumber));
			}

			OrderTransactionCollection transactions = new OrderTransactionCollection(existingOrderNumber);
			transactions.AddTransaction(transactionState, null, null, null, transactionId, AppLogic.ro_PMPayPalExpress + " IPN", null, paymentTotal);

			return Content(string.Empty);
		}

		bool PostIsValid()
		{
			//Validate the post by querying PayPal
			var param = Request.BinaryRead(Request.ContentLength);
			var verifyUrl = AppLogic.AppConfigBool("UseLiveTransactions")
				? AppLogic.AppConfig("PayPal.LiveServer")
				: AppLogic.AppConfig("PayPal.TestServer");

			var formString = Encoding.ASCII.GetString(param);
			formString += "&cmd=_notify-validate";

			var data = Encoding.ASCII.GetBytes(formString);

			var webRequest = (HttpWebRequest)WebRequest.Create(verifyUrl);
			webRequest.Method = "POST";
			webRequest.ContentType = "application/x-www-form-urlencoded";
			webRequest.ContentLength = data.Length;

			var reqStream = webRequest.GetRequestStream();
			reqStream.Write(data, 0, data.Length);
			reqStream.Close();

			var rawResponse = string.Empty;
			try
			{
				var webResponse = webRequest.GetResponse();
				var sr = new StreamReader(webResponse.GetResponseStream());
				rawResponse = sr.ReadToEnd();
				sr.Close();
				webResponse.Close();
			}
			catch(Exception exception)
			{
				SysLog.LogException(exception, MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
			}

			return rawResponse.Equals("VERIFIED", StringComparison.InvariantCultureIgnoreCase);
		}

		void HandlePayPalExpressCheckoutRecurringNotification(string transactionType, string payerId, string profileId, string subscriptionId)
		{
			switch(transactionType.ToLowerInvariant())
			{
				case "recurring_payment":
					CreateNewPPECRecurrence(payerId, profileId, subscriptionId);
					break;
				case "recurring_payment_expired":
				case "subscr_cancel":
				case "recurring_payment_profile_cancel":
					CancelPPECRecurringSubscription(payerId, profileId, subscriptionId);
					break;
				default:
					break;
			}
		}

		void CreateNewPPECRecurrence(string payerId, string profileId, string subscriptionId)
		{
			var originalOrderNumber = GetPPECOriginalOrderNumber(profileId, subscriptionId);

			if(originalOrderNumber != 0)
			{
				var manager = new RecurringOrderMgr();
				manager.ProcessPPECRecurringOrder(originalOrderNumber);
			}
			else
			{
				SysLog.LogMessage("A recurring payment notification came from PayPal Express that did not match an existing recurring order.",
					string.Format("PayerID = {0}, ProfileID = {1}", payerId, profileId),
					MessageTypeEnum.Informational,
					MessageSeverityEnum.Alert);
			}
		}

		void CancelPPECRecurringSubscription(string payerId, string profileId, string subscriptionId)
		{
			var originalOrderNumber = GetPPECOriginalOrderNumber(profileId, subscriptionId);

			if(originalOrderNumber != 0)
			{
				// Cancelling through the API triggers a notification to this page.  Make sure we don't try to cancel the same order repeatedly.
				if(PPECRecurringOrderIsStillActive(originalOrderNumber))
				{
					var manager = new RecurringOrderMgr();
					manager.CancelPPECRecurringOrder(originalOrderNumber, true);
				}
			}
			else
			{
				SysLog.LogMessage("A recurring payment cancellation notification came from PayPal Express that did not match an existing recurring order.",
					string.Format("PayerID = {0}, ProfileID = {1}", payerId, profileId),
					MessageTypeEnum.Informational,
					MessageSeverityEnum.Alert);
			}
		}

		int GetPPECOriginalOrderNumber(string profileId, string subscriptionId)
		{
			var originalOrderNumber = 0;

			if(string.IsNullOrEmpty(profileId))
				profileId = subscriptionId;

			var orderNumberSql = "SELECT OrderNumber AS N FROM OrderTransaction WHERE Code = @profileId";
			SqlParameter[] orderNumberParams = { new SqlParameter("@profileId", profileId) };

			using(var connection = new SqlConnection(DB.GetDBConn()))
			{
				originalOrderNumber = DB.GetSqlN(orderNumberSql, orderNumberParams);
			}

			return originalOrderNumber;
		}

		bool PPECRecurringOrderIsStillActive(int originalOrderNumber)
		{
			var orderToCheck = new Order(originalOrderNumber);
			var customerToCheck = new Customer(orderToCheck.CustomerID);
			var cartToCheck = new ShoppingCart(customerToCheck.SkinID, customerToCheck, CartTypeEnum.RecurringCart, originalOrderNumber, false);

			return cartToCheck.CartItems.Any();
		}

		void CapturePPOrder(int orderNumber, string transactionId, decimal orderTotal)
		{
			using(var connection = DB.dbConn())
			{
				connection.Open();
				using(var reader = DB.GetRS(string.Format("SELECT PaymentMethod, CapturedOn, TransactionState FROM Orders WITH (NOLOCK) WHERE OrderNumber = {0}", orderNumber), connection))
				{
					if(reader.Read())
					{
						var paymentMethod = AppLogic.CleanPaymentMethod(DB.RSField(reader, "PaymentMethod"));
						if(DB.RSFieldDateTime(reader, "CapturedOn") == DateTime.MinValue
							&& (DB.RSField(reader, "TransactionState") == AppLogic.ro_TXStateAuthorized
								|| DB.RSField(reader, "TransactionState") == AppLogic.ro_TXStatePending))
						{
							DB.ExecuteSQL(string.Format("UPDATE Orders SET OrderTotal = {0} WHERE OrderNumber = {1}",
								Localization.CurrencyStringForDBWithoutExchangeRate(orderTotal),
								orderNumber));

							var order = new Order(orderNumber, Localization.GetDefaultLocale());
							order.CaptureTXCommand = "Instant Payment Notification";
							order.CaptureTXResult = AppLogic.ro_OK;
							order.AuthorizationPNREF = order.AuthorizationPNREF + "|CAPTURE=" + transactionId;

							Gateway.ProcessOrderAsCaptured(orderNumber);
						}
					}
				}
			}
		}

		void RefundPPOrder(int orderNumber, string transactionid, decimal refundAmount)
		{
			var order = new Order(orderNumber, Localization.GetDefaultLocale());
			var customer = new Customer(order.CustomerID);
			var orderTotal = 0.0M;
			var couponType = 0;
			var couponCode = string.Empty;
			var couponDiscountAmount = 0.0M;

			if(refundAmount < 0)
				refundAmount = (decimal)(-1.0) * refundAmount;

			using(var connection = DB.dbConn())
			{
				connection.Open();
				using(var reader = DB.GetRS(string.Format(@"SELECT RefundedOn, 
																	CapturedOn, 
																	TransactionState, 
																	OrderTotal, 
																	CouponType, 
																	CouponCode, 
																	CouponDiscountAmount 
																FROM Orders WITH (NOLOCK) 
																WHERE OrderNumber = {0}",
														orderNumber),
											connection))
				{
					if(reader.Read())
					{
						if(DB.RSFieldDateTime(reader, "RefundedOn") == DateTime.MinValue
							&& DB.RSFieldDateTime(reader, "CapturedOn") != DateTime.MinValue
							&& DB.RSField(reader, "TransactionState") == AppLogic.ro_TXStateCaptured)
						{
							orderTotal = DB.RSFieldDecimal(reader, "OrderTotal");
							couponType = DB.RSFieldInt(reader, "CouponType");
							couponCode = DB.RSField(reader, "CouponCode");
							couponDiscountAmount = DB.RSFieldDecimal(reader, "CouponDiscountAmount");

							DB.ExecuteSQL(string.Format(@"UPDATE Orders 
														SET RefundTXCommand = 'Instant Payment Notification', 
															RefundTXResult = {0}, 
															AuthorizationPNREF = AuthorizationPNREF + {1}
														WHERE OrderNumber = {2}",
															DB.SQuote(AppLogic.ro_OK),
															DB.SQuote("|REFUND=" + transactionid),
															orderNumber));

							// was this a full refund
							// we can only properly handle IPN's for refunds of the full order amount
							if(refundAmount == decimal.Zero || refundAmount == orderTotal)
							{
								// make sure inventory was restored. safe to call repeatedly. proc protects against deducting twice
								DB.ExecuteSQL(string.Format("aspdnsf_AdjustInventory {0}, 1 ", orderNumber));
								Gateway.DecrementMicropayProductsInOrder(orderNumber);

								// update transactionstate
								DB.ExecuteSQL(string.Format(@"UPDATE Orders 
															SET RefundReason = 'PayPal IPN Refund', 
																TransactionState = {0}, 
																RefundedOn = GETDATE(), 
																IsNew = 0 
															WHERE OrderNumber = {1}",
															DB.SQuote(AppLogic.ro_TXStateRefunded),
															orderNumber));

								//Invalidate GiftCards ordered on this order
								HandleGiftCardsOnCancelledOrder(orderNumber, couponType, couponCode, couponDiscountAmount);
							}
						}
					}
				}
			}
		}

		void VoidPPOrder(int orderNumber)
		{
			var order = new Order(orderNumber, Localization.GetDefaultLocale());
			var customer = new Customer(order.CustomerID);

			using(var connection = DB.dbConn())
			{
				connection.Open();
				using(var reader = DB.GetRS(string.Format(@"SELECT VoidedOn, 
																OrderTotal, 
																CouponType,
																CouponCode,
																CouponDiscountAmount
															FROM Orders WITH (NOLOCK) 
															WHERE OrderNumber = {0}",
														orderNumber),
											connection))
				{
					if(reader.Read())
					{
						if(DB.RSFieldDateTime(reader, "VoidedOn") == DateTime.MinValue)
						{
							var orderTotal = DB.RSFieldDecimal(reader, "OrderTotal");
							var couponType = DB.RSFieldInt(reader, "CouponType");
							var couponCode = DB.RSField(reader, "CouponCode");
							var couponDiscountAmount = DB.RSFieldDecimal(reader, "CouponDiscountAmount");

							// make sure inventory was restored. safe to call repeatedly. proc protects against deducting twice
							DB.ExecuteSQL(string.Format("aspdnsf_AdjustInventory {0}, 1 ", orderNumber));
							Gateway.DecrementMicropayProductsInOrder(orderNumber);

							// update transactionstate
							DB.ExecuteSQL("update Orders set VoidTXCommand='Instant Payment Notification', VoidTXResult=" + DB.SQuote(AppLogic.ro_OK) + ", TransactionState=" + DB.SQuote(AppLogic.ro_TXStateVoided) + ", VoidedOn=getdate(), IsNew=0 where ordernumber=" + orderNumber.ToString());

							//Invalidate GiftCards ordered on this order
							HandleGiftCardsOnCancelledOrder(orderNumber, couponType, couponCode, couponDiscountAmount);
						}
					}
				}
			}
		}

		void HandleGiftCardsOnCancelledOrder(int orderNumber, int couponType, string couponCode, decimal couponDiscountAmount)
		{
			var giftCards = new GiftCards(orderNumber, GiftCardCollectionFilterType.OrderNumber);

			foreach(GiftCard card in giftCards)
			{
				card
					.GiftCardTransactions
						.Add(GiftCardUsageTransaction
							.CreateTransaction(card.GiftCardID,
												GiftCardUsageReasons.FundsRemovedByAdmin,
												0,
												0,
												card.Balance,
												string.Empty));
				card.UpdateCard(null, null, null, null, 1, null, null, null, null, null, null, null, null, null, null);
			}

			//Restore Amount to gift card used in paying for the order
			if((CouponTypeEnum)couponType == CouponTypeEnum.GiftCard)
			{
				var giftCard = new GiftCard(couponCode);
				if(giftCard.GiftCardID != 0)
				{
					giftCard
						.GiftCardTransactions
							.Add(GiftCardUsageTransaction
								.CreateTransaction(giftCard.GiftCardID,
													GiftCardUsageReasons.FundsAddedByAdmin,
													0,
													0,
													couponDiscountAmount,
													string.Empty));
				}
			}
		}
	}
}
