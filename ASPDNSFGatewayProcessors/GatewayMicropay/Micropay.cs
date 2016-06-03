// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontGateways.Processors
{
	public class Micropay : GatewayProcessor
	{
		public override string CaptureOrder(Order order)
		{
			var result = AppLogic.ro_OK;
			var useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
			var transactionId = order.AuthorizationPNREF;
			var orderTotal = order.OrderBalance;
			var transactionState = order.TransactionState;
			var customerId = order.CustomerID;
			var mpBalance = AppLogic.GetMicroPayBalance(customerId);

			var transactionCommand = new StringBuilder(4096);
			transactionCommand.Append("x_type=CAPTURE");
			transactionCommand.Append("&x_test_request=" + CommonLogic.IIF(useLiveTransactions, "FALSE", "TRUE"));
			transactionCommand.Append("&x_customer_ip=" + CommonLogic.CustomerIpAddress());
			transactionCommand.Append("&x_trans_id=" + transactionId);
			transactionCommand.Append("&x_amount=" + Localization.CurrencyStringForGatewayWithoutExchangeRate(orderTotal));
			order.CaptureTXCommand = transactionCommand.ToString();

			if(transactionId.Length == 0)
			{
				result = "Invalid or Empty Transaction ID";
			}
			else
			{
				var rawResponseString = string.Empty;
				if(orderTotal > mpBalance)
				{
					rawResponseString = "INSUFFICIENT FUNDS";
					result = rawResponseString;
				}
				else
				{
					// withdrawl the funds:
					DB.ExecuteSQL(string.Format("update customer set MicroPayBalance={0} where CustomerID={1}", Localization.CurrencyStringForDBWithoutExchangeRate(mpBalance - orderTotal), customerId.ToString()));
					rawResponseString = "MICROPAY GATEWAY SAID OK";
				}

				order.CaptureTXResult = rawResponseString;
			}

			return result;
		}

		public override string VoidOrder(int orderNumber)
		{
			var result = AppLogic.ro_OK;

			DB.ExecuteSQL("update orders set VoidTXCommand=NULL, VoidTXResult=NULL where OrderNumber=" + orderNumber.ToString());

			var useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
			var orderTotal = System.Decimal.Zero;
			var transactionState = string.Empty;
			var transactionId = string.Empty;
			var customerId = 0;

			using(var connection = DB.dbConn())
			{
				connection.Open();
				using(var reader = DB.GetRS(string.Format("select AuthorizationPNREF,OrderTotal,CustomerID,TransactionState from orders  with (NOLOCK)  where OrderNumber={0}", orderNumber), connection))
				{
					if(reader.Read())
					{
						transactionId = DB.RSField(reader, "AuthorizationPNREF");
						orderTotal = DB.RSFieldDecimal(reader, "OrderTotal");
						customerId = DB.RSFieldInt(reader, "CustomerID");
						transactionState = DB.RSField(reader, "TransactionState");
					}
				}
			}

			var mpBalance = AppLogic.GetMicroPayBalance(customerId);

			var transactionCommand = new StringBuilder(4096);
			transactionCommand.Append("x_type=VOID");
			transactionCommand.Append("&x_test_request=" + CommonLogic.IIF(useLiveTransactions, "FALSE", "TRUE"));
			transactionCommand.Append("&x_customer_ip=" + CommonLogic.CustomerIpAddress());
			transactionCommand.Append("&x_trans_id=" + transactionId);

			DB.ExecuteSQL("update orders set VoidTXCommand=" + DB.SQuote(transactionCommand.ToString()) + " where OrderNumber=" + orderNumber.ToString());

			if(transactionId.Length == 0)
			{
				result = "Invalid or Empty Transaction ID";
			}
			else
			{
				var rawResponseString = string.Empty;
				if(transactionState == AppLogic.ro_TXStateCaptured)
				{
					// restore their balance if it was captured!
					DB.ExecuteSQL(string.Format("update customer set MicroPayBalance={0} where CustomerID={1}", Localization.CurrencyStringForDBWithoutExchangeRate(mpBalance + orderTotal), customerId.ToString()));
					rawResponseString = string.Format("MicroPay Balance {0} => {1}", Localization.CurrencyStringForDBWithoutExchangeRate(mpBalance), Localization.CurrencyStringForDBWithoutExchangeRate(orderTotal + mpBalance));
				}
				else
				{
					rawResponseString = "MICROPAY GATEWAY SAID NO VOID ACTION NEEDED (WAS NOT IN CAPTURED STATE)";
				}
				DB.ExecuteSQL("update orders set VoidTXResult=" + DB.SQuote(rawResponseString) + " where OrderNumber=" + orderNumber.ToString());
			}

			return result;
		}

		public override string RefundOrder(int originalOrderNumber, int newOrderNumber, decimal refundAmount, string refundReason, Address billingAddress)
		{
			// if RefundAmount == 0.0M, then then ENTIRE order amount will be refunded!
			var result = AppLogic.ro_OK;

			DB.ExecuteSQL("update orders set RefundTXCommand=NULL, RefundTXResult=NULL where OrderNumber=" + originalOrderNumber.ToString());
			var useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
			var transactionId = string.Empty;
			var orderTotal = System.Decimal.Zero;
			var transactionState = string.Empty;
			var customerId = 0;

			using(var connection = DB.dbConn())
			{
				connection.Open();
				using(var reader = DB.GetRS("select AuthorizationPNREF,OrderTotal,TransactionState,CustomerID from orders   with (NOLOCK)  where OrderNumber=" + originalOrderNumber.ToString(), connection))
				{
					if(reader.Read())
					{
						transactionId = DB.RSField(reader, "AuthorizationPNREF");
						orderTotal = DB.RSFieldDecimal(reader, "OrderTotal");
						transactionState = DB.RSField(reader, "TransactionState");
						customerId = DB.RSFieldInt(reader, "CustomerID");
					}
				}
			}

			var mpBalance = AppLogic.GetMicroPayBalance(customerId);

			var transactionCommand = new StringBuilder(4096);
			transactionCommand.Append("x_type=CREDIT");
			transactionCommand.Append("&x_test_request=" + CommonLogic.IIF(useLiveTransactions, "FALSE", "TRUE"));
			transactionCommand.Append("&x_trans_id=" + transactionId);
			if(refundAmount == System.Decimal.Zero)
			{
				transactionCommand.Append("&x_amount=" + Localization.CurrencyStringForGatewayWithoutExchangeRate(orderTotal));
			}
			else
			{
				transactionCommand.Append("&x_amount=" + Localization.CurrencyStringForGatewayWithoutExchangeRate(refundAmount));
			}
			transactionCommand.Append("&x_customer_ip=" + CommonLogic.CustomerIpAddress());

			DB.ExecuteSQL("update orders set RefundTXCommand=" + DB.SQuote(transactionCommand.ToString()) + " where OrderNumber=" + originalOrderNumber.ToString());

			if(transactionId.Length == 0)
			{
				result = "Invalid or Empty Transaction ID";
			}
			else
			{
				var rawResponseString = string.Empty;
				if(transactionState == AppLogic.ro_TXStateCaptured)
				{
					// restore their balance if it was captured!
					DB.ExecuteSQL(string.Format("update customer set MicroPayBalance={0} where CustomerID={1}", Localization.CurrencyStringForDBWithoutExchangeRate(mpBalance + CommonLogic.IIF(refundAmount == System.Decimal.Zero, orderTotal, refundAmount)), customerId.ToString()));
					rawResponseString = string.Format("MicroPay Balance {0} => {1}", Localization.CurrencyStringForDBWithoutExchangeRate(mpBalance), Localization.CurrencyStringForDBWithoutExchangeRate(orderTotal + mpBalance));
				}
				else
				{
					rawResponseString = "MICROPAY GATEWAY SAID NO REFUND ACTION NEEDED (WAS NOT IN CAPTURED STATE)";
				}
				DB.ExecuteSQL("update orders set RefundTXResult=" + DB.SQuote(rawResponseString) + " where OrderNumber=" + originalOrderNumber.ToString());

			}
			return result;
		}

		public override string ProcessCard(int orderNumber, int customerId, decimal orderTotal, bool useLiveTransactions, TransactionModeEnum transactionMode, Address billingAddress, string cardExtraCode, Address shippingAddress, string cavv, string eci, string xid, out string avsResult, out string authorizationResult, out string authorizationCode, out string authorizationTransactionId, out string transactionCommand, out string transactionResponse)
		{
			return 
				avsResult = 
				authorizationResult = 
				authorizationCode = 
				authorizationTransactionId = 
				transactionCommand = 
				transactionResponse = string.Empty;
		}
	}
}
