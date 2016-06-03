// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;

namespace AspDotNetStorefrontCore
{
	/// <summary>
	/// Summary description for Cardinal.
	/// </summary>
	public class Cardinal
	{
		public Cardinal() { }

		static public bool EnabledForCheckout(decimal cartTotal, string cardType)
		{
			var cardinalAllowed = AppLogic.AppConfigBool("CardinalCommerce.Centinel.Enabled")
				&& !(cartTotal == decimal.Zero
					&& AppLogic.AppConfigBool("SkipPaymentEntryOnZeroDollarCheckout"));

			return cardinalAllowed
				&& (cardType.Trim().Equals("VISA", StringComparison.InvariantCultureIgnoreCase)
					|| cardType.Trim().Equals("MASTERCARD", StringComparison.InvariantCultureIgnoreCase)
					|| cardType.Trim().Equals("JCB", StringComparison.InvariantCultureIgnoreCase));
		}

		static public bool PreChargeLookupAndStoreSession(Customer customer, int orderNumber, decimal cartTotal, string cardNumber, string expMonth, string expYear)
		{
			// use cardinal pre-auth fraud screening:
			var ACSUrl = string.Empty;
			var Payload = string.Empty;
			var TransactionID = string.Empty;
			var CardinalLookupResult = string.Empty;

			if(PreChargeLookup(cardNumber,
				Localization.ParseUSInt(expYear),
				Localization.ParseUSInt(expMonth),
				orderNumber,
				cartTotal,
				string.Empty,
				out ACSUrl,
				out Payload,
				out TransactionID,
				out CardinalLookupResult))
			{
				// redirect to intermediary page which gets card password from user:
				customer.ThisCustomerSession["Cardinal.LookupResult"] = CardinalLookupResult;
				customer.ThisCustomerSession["Cardinal.ACSUrl"] = ACSUrl;
				customer.ThisCustomerSession["Cardinal.Payload"] = Payload;
				customer.ThisCustomerSession["Cardinal.TransactionID"] = TransactionID;
				customer.ThisCustomerSession["Cardinal.OrderNumber"] = orderNumber.ToString();

				return true;
			}
			else
			{
				customer.ThisCustomerSession["Cardinal.LookupResult"] = CardinalLookupResult;
			}

			return false;
		}

		static public string GetECIFlag(string cardType)
		{
			// set the ECIFlag for an 'N' Enrollment response, so the merchant receives Liability Shift Protection
			if(cardType.Trim().Equals("VISA", StringComparison.InvariantCultureIgnoreCase))
			{
				return "06";  // Visa Card Issuer Liability
			}
			else if(cardType.Trim().Equals("JCB", StringComparison.InvariantCultureIgnoreCase))
			{
				return "07";  // Indicates Merchant Liability 
			}
			else
			{
				return "01";  // MasterCard Merchant Liability for non-enrolled card (rules differ between MC and Visa in the regard)
			}
		}

		// returns true if no errors, and card is enrolled:
		static public bool PreChargeLookup(string cardNumber,
			int cardExpirationYear,
			int cardExpirationMonth,
			int orderNumber,
			decimal orderTotal,
			string orderDescription,
			out string acsUrl,
			out string payload,
			out string transactionId,
			out string cardinalLookupResult)
		{
			var ccRequest = new CardinalCommerce.CentinelRequest();
			var ccResponse = new CardinalCommerce.CentinelResponse();
			var numAttempts = AppLogic.AppConfigUSInt("CardinalCommerce.Centinel.NumRetries");
			var callSucceeded = false;

			payload = string.Empty;
			acsUrl = String.Empty;
			transactionId = String.Empty;

			// ==================================================================================
			// Construct the cmpi_lookup message
			// ==================================================================================

			ccRequest.add("MsgType", AppLogic.AppConfig("CardinalCommerce.Centinel.MsgType.Lookup"));
			ccRequest.add("Version", "1.7");
			ccRequest.add("ProcessorId", AppLogic.AppConfig("CardinalCommerce.Centinel.ProcessorID"));
			ccRequest.add("MerchantId", AppLogic.AppConfig("CardinalCommerce.Centinel.MerchantID"));
			ccRequest.add("TransactionPwd", AppLogic.AppConfig("CardinalCommerce.Centinel.TransactionPwd"));
			ccRequest.add("TransactionType", "C"); //C = Credit Card / Debit Card Authentication.
			ccRequest.add("Amount", Localization.CurrencyStringForGatewayWithoutExchangeRate(orderTotal).Replace(",", "").Replace(".", ""));
			ccRequest.add("CurrencyCode", Localization.StoreCurrencyNumericCode());
			ccRequest.add("CardNumber", cardNumber);
			ccRequest.add("CardExpMonth", cardExpirationMonth.ToString().PadLeft(2, '0'));
			ccRequest.add("CardExpYear", cardExpirationYear.ToString().PadLeft(4, '0'));
			ccRequest.add("OrderNumber", orderNumber.ToString());

			// Optional fields
			ccRequest.add("OrderDescription", orderDescription);
			ccRequest.add("UserAgent", CommonLogic.ServerVariables("HTTP_USER_AGENT"));
			ccRequest.add("Recurring", "N");

			if(numAttempts == 0)
				numAttempts = 1;

			for(int i = 1; i <= numAttempts; i++)
			{
				callSucceeded = true;
				try
				{
					var URL = AppLogic.AppConfigBool("CardinalCommerce.Centinel.IsLive")
						? AppLogic.AppConfig("CardinalCommerce.Centinel.TransactionUrl.Live")
						: AppLogic.AppConfig("CardinalCommerce.Centinel.TransactionUrl.Test");

					ccResponse = ccRequest.sendHTTP(URL, AppLogic.AppConfigUSInt("CardinalCommerce.Centinel.MapsTimeout"));
				}
				catch
				{
					callSucceeded = false;
				}
				if(callSucceeded)
				{
					break;
				}
			}
			
			if(callSucceeded)
			{
				var errorNo = ccResponse.getValue("ErrorNo");
				var enrolled = ccResponse.getValue("Enrolled");
				payload = ccResponse.getValue("Payload");
				acsUrl = ccResponse.getValue("ACSUrl");
				transactionId = ccResponse.getValue("TransactionId");

				cardinalLookupResult = ccResponse.getUnparsedResponse();

				ccRequest = null;
				ccResponse = null;

				//======================================================================================
				// Assert that there was no error code returned and the Cardholder is enrolled in the
				// Payment Authentication Program prior to starting the Authentication process.
				//======================================================================================

				if(errorNo == "0" && enrolled == "Y")
				{
					return true;
				}
				return false;
			}
			ccRequest = null;
			ccResponse = null;
			cardinalLookupResult = "Cardinal Lookup Call Failed";
			return false;
		}

		static public string PreChargeAuthenticate(int orderNumber, 
			string paRes,
			string transactionId, 
			out string paResStatus, 
			out string signatureVerification, 
			out string errorNumber, 
			out string errorDescription, 
			out string cardinalAuthenticateResult)
		{
			var ccRequest = new CardinalCommerce.CentinelRequest();
			var ccResponse = new CardinalCommerce.CentinelResponse();
			var numAttempts = AppLogic.AppConfigUSInt("CardinalCommerce.Centinel.NumRetries");
			var callSucceeded = false;

			errorNumber = string.Empty;
			errorDescription = string.Empty;
			paResStatus = string.Empty;
			signatureVerification = string.Empty;


			if(paRes.Length == 0 || transactionId.Length == 0)
			{
				cardinalAuthenticateResult = "Insufficient Input Data";
				return "Error Invoking Cardinal Authentication - Session May Have Expired";
			}
			else
			{

				// ==================================================================================
				// Construct the cmpi_authenticate message
				// ==================================================================================

				ccRequest.add("MsgType", AppLogic.AppConfig("CardinalCommerce.Centinel.MsgType.Authenticate")); //cmpi_authenticate
				ccRequest.add("Version", "1.7");
				ccRequest.add("ProcessorId", AppLogic.AppConfig("CardinalCommerce.Centinel.ProcessorID"));
				ccRequest.add("MerchantId", AppLogic.AppConfig("CardinalCommerce.Centinel.MerchantID"));
				ccRequest.add("TransactionType", "C");
				ccRequest.add("TransactionPwd", AppLogic.AppConfig("CardinalCommerce.Centinel.TransactionPwd"));
				ccRequest.add("TransactionId", transactionId);
				ccRequest.add("PAResPayload", HttpContext.Current.Server.HtmlEncode(paRes));

				if(numAttempts == 0)
					numAttempts = 1;

				for(int i = 1; i <= numAttempts; i++)
				{
					callSucceeded = true;
					try
					{
						var URL = AppLogic.AppConfigBool("CardinalCommerce.Centinel.IsLive")
							? AppLogic.AppConfig("CardinalCommerce.Centinel.TransactionUrl.Live")
							: AppLogic.AppConfig("CardinalCommerce.Centinel.TransactionUrl.Test");

						ccResponse = ccRequest.sendHTTP(URL, AppLogic.AppConfigUSInt("CardinalCommerce.Centinel.MapsTimeout"));
					}
					catch
					{
						callSucceeded = false;
					}
					if(callSucceeded)
					{
						break;
					}
				}

				if(callSucceeded)
				{
					errorNumber = ccResponse.getValue("ErrorNo");
					errorDescription = ccResponse.getValue("ErrorDesc");
					paResStatus = ccResponse.getValue("PAResStatus");
					signatureVerification = ccResponse.getValue("SignatureVerification");

					cardinalAuthenticateResult = ccResponse.getUnparsedResponse();
					var response = ccResponse.getUnparsedResponse();
					return response;
				}
				
				cardinalAuthenticateResult = "Cardinal Authenticate Call Failed";
				return "Error Invoking Cardinal Authentication - Please Retry Credit Card Entry";
			}
		}
	}
}
