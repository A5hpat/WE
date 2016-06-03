// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data.SqlClient;
using System.Linq;
using AspDotNetStorefrontCore;
using GWBraintree = Braintree;

namespace AspDotNetStorefrontGateways.Processors
{
    public class Braintree : GatewayProcessor
	{
		readonly string BN = "AspDotNetStorefront_BT";  // Do not change this line or your paypal website calls may not work!

		public override string CaptureOrder(Order order)
		{
			var result = AppLogic.ro_OK;
			var transactionId = LookupBraintreeTransactionId(order.OrderNumber);

			if(string.IsNullOrEmpty(transactionId))
				return "No stored Braintree transaction ID found for this order!";

			var gateway = CreateBraintreeGateway();

			var transactionResult = gateway.Transaction.SubmitForSettlement(transactionId);

			if(!transactionResult.IsSuccess())
			{
				result = string.Join(",", transactionResult.Errors.DeepAll().Select(e => e.Message));
			}

			return result;
		}

		public override string VoidOrder(int orderNumber)
		{
			var result = AppLogic.ro_OK;
			var transactionId = LookupBraintreeTransactionId(orderNumber);

			if(string.IsNullOrEmpty(transactionId))
				return "No stored Braintree transaction ID found for this order!";

			var gateway = CreateBraintreeGateway();

			var transactionResult = gateway.Transaction.Void(transactionId);

			if(!transactionResult.IsSuccess())
			{
				result = string.Join(",", transactionResult.Errors.DeepAll().Select(e => e.Message));
			}

			return result;
		}

		public override string RefundOrder(int originalOrderNumber, int orderNumber, decimal refundAmount, string refundReason, Address useBillingAddress)
		{
			var result = AppLogic.ro_OK;
			var transactionId = LookupBraintreeTransactionId(originalOrderNumber);

			if(string.IsNullOrEmpty(transactionId))
				return "No stored Braintree transaction ID found for this order!";

			var gateway = CreateBraintreeGateway();

			var transactionResult = gateway.Transaction.Refund(transactionId, refundAmount);

			if(!transactionResult.IsSuccess())
			{
				result = string.Join(",", transactionResult.Errors.DeepAll().Select(e => e.Message));
			}

			return result;
		}

		public override string ProcessCard(int orderNumber, int customerID, Decimal orderTotal, bool useLiveTransactions, TransactionModeEnum transactionMode, Address useBillingAddress, string cardExtraCode, Address useShippingAddress, string cavv, string eci, string xid, out string avsResult, out string authorizationResult, out string authorizationCode, out string authorizationTransID, out string transactionCommandOut, out string transactionResponse)
		{
			var session = new CustomerSession(customerID);

			string result = AppLogic.ro_OK;
			authorizationCode =
				authorizationResult =
				authorizationTransID =
				avsResult =
				transactionCommandOut =
				transactionResponse = string.Empty;

			var threeDSecureEnabled = AppLogic.AppConfigBool("Braintree.3dSecureEnabled");

			//3dSecure is enabled and this customer hasn't yet been authed - send them to the 3dSecure form
			if(threeDSecureEnabled 
				&& session[AppLogic.Braintree3dSecureKey].EqualsIgnoreCase("false")
				&& session[AppLogic.BraintreePaymentMethod].EqualsIgnoreCase("creditcard"))
			{
				session["3DSecure.OrderNumber"] = orderNumber.ToString();
				return AppLogic.ro_3DSecure;
			}

			var nonce = session[AppLogic.BraintreeNonceKey];

			var gateway = CreateBraintreeGateway();

			var transactionRequest = new GWBraintree.TransactionRequest
			{
				OrderId = orderNumber.ToString(),
				Amount = Math.Round(orderTotal, 2), //They only accept up to 2 decimal places
				PaymentMethodNonce = nonce,
				Options = new GWBraintree.TransactionOptionsRequest
				{
					SubmitForSettlement = AppLogic.TransactionModeIsAuthCapture(),
					ThreeDSecure = (threeDSecureEnabled && session[AppLogic.Braintree3dSecureKey] == "true")	//Don't add this if the card wasn't enrolled
					? new GWBraintree.TransactionOptionsThreeDSecureRequest()
						{
							Required = threeDSecureEnabled
						}
					: null
				},
				//Add address info in case the merchant has turned on AVS settings in their Braintree account
				BillingAddress = new GWBraintree.AddressRequest
				{
					FirstName = useBillingAddress.FirstName,
					LastName = useBillingAddress.LastName,
					StreetAddress = useBillingAddress.Address1,
					ExtendedAddress = useBillingAddress.Address2,
					Locality = useBillingAddress.City,
					Region = useBillingAddress.State,
					PostalCode = useBillingAddress.Zip,
					CountryName = useBillingAddress.Country
				},
				Channel = BN
			};

			var transactionResult = gateway.Transaction.Sale(transactionRequest);

			if(transactionResult.IsSuccess())
			{
				authorizationTransID = transactionResult.Target.Id;
				authorizationCode = transactionResult.Target.ProcessorAuthorizationCode ?? string.Empty;
				authorizationResult = transactionResult.Target.ProcessorResponseText ?? string.Empty;
				avsResult = transactionResult.Target.AvsPostalCodeResponseCode ?? string.Empty;

				//We don't have any card details yet - fill some of that in with what Braintree gives us back.
				if(transactionResult.Target.PaymentInstrumentType == GWBraintree.PaymentInstrumentType.CREDIT_CARD)
				{
					useBillingAddress.CardNumber = transactionResult.Target.CreditCard.LastFour;
					useBillingAddress.CardType = transactionResult.Target.CreditCard.CardType.ToString().ToUpperInvariant();
					useBillingAddress.CardExpirationMonth = transactionResult.Target.CreditCard.ExpirationMonth;
					useBillingAddress.CardExpirationYear = transactionResult.Target.CreditCard.ExpirationYear;
					useBillingAddress.UpdateDB();
				}
			}
			else
			{
				result = transactionResult.Message;
			}

			return result;
		}

		public override bool SupportsPostProcessingEdits()
		{
			return false;
		}

		public override bool SupportsAdHocOrders()
		{
			return true;
		}

		public override string ObtainBraintreeToken()
		{
			try
			{
				return CreateBraintreeGateway().ClientToken.generate();
			}
			catch(Exception exception)
			{
				SysLog.LogMessage("Connection to Braintree service failed.", exception.Message, MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
                return string.Empty;
			}
		}

		GWBraintree.BraintreeGateway CreateBraintreeGateway()
		{
			return new GWBraintree.BraintreeGateway
			{
				Environment = AppLogic.AppConfigBool("UseLiveTransactions")
					? GWBraintree.Environment.PRODUCTION
					: GWBraintree.Environment.SANDBOX,
				MerchantId = AppLogic.AppConfig("Braintree.MerchantId"),
				PublicKey = AppLogic.AppConfig("Braintree.PublicKey"),
				PrivateKey = AppLogic.AppConfig("Braintree.PrivateKey")
			};
		}

		string LookupBraintreeTransactionId(int orderNumber)
		{
			return DB.GetSqlS(
				"SELECT AuthorizationPNREF AS S FROM Orders with (NOLOCK) WHERE OrderNumber = @OrderNumber",
				new SqlParameter("@OrderNumber", orderNumber));
		}
	}
}
