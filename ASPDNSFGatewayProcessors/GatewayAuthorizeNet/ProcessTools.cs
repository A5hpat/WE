// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;

namespace GatewayAuthorizeNet
{
	public class ProcessTools
	{
		private const string OK = "OK";
		private const string ERROR = "ERROR";

		public static string ProcessCard(int orderNumber, int customerId, decimal orderTotal, long paymentProfileId, string transactionMode,
			bool useLiveTransactions, out string AVSResult, out string AuthorizationResult,
			out string AuthorizationCode, out string AuthorizationTransID, out string TransactionCommandOut, out string TransactionResponse)
		{
			AVSResult = string.Empty;
			AuthorizationResult = string.Empty;
			AuthorizationCode = string.Empty;
			AuthorizationTransID = string.Empty;
			TransactionCommandOut = string.Empty;
			TransactionResponse = string.Empty;

			var status = ERROR;

			var paymentProcessor = new PaymentProcessor();
			CIMResponse processStatus = null;
			long profileId = DataUtility.GetProfileId(customerId);
			if(profileId > 0)
			{
				if(paymentProfileId > 0)
				{
					if(transactionMode.ToUpperInvariant() == "AUTH")
						processStatus = paymentProcessor.Authorize(profileId, paymentProfileId, orderNumber, orderTotal);
					else if(transactionMode.ToUpperInvariant() == "AUTHCAPTURE")
						processStatus = paymentProcessor.AuthCapture(profileId, paymentProfileId, orderNumber, orderTotal);
				}
			}
			if(processStatus != null)
			{
				if(processStatus.Success)
				{
					status = OK;
					AVSResult = processStatus.AvsCode;
					AuthorizationResult = processStatus.AuthMessage;
					AuthorizationCode = processStatus.AuthCode;
					AuthorizationTransID = processStatus.TransactionId;
				}
				else
				{
					status = processStatus.AuthMessage;

					if(string.IsNullOrEmpty(status))
						status = "There was an error processing your payment. Please try again, choose a different payment method, or contact customer support.";
				}
			}
			return status;
		}


		public static long SaveProfileAndPaymentProfile(int customerId, string email, string storeName, long paymentProfileId, int addressId, string cardNumber, string cardCode, string expMonth, string expYear, out string errorMessage, out string errorCode)
		{
			errorMessage = string.Empty;
			errorCode = string.Empty;
			long profileId = DataUtility.GetProfileId(customerId);
			ProfileManager profileMgr = profileId > 0 ? new ProfileManager(customerId, email, profileId) : null;

			// Create profile if needed
			if(profileMgr == null 
				|| !profileMgr.UpdateProfile(email, ProfileManager.GetProfileDescription(storeName)))
			{
				//Clear out the profile id in case the auth.net account has changed and we need to create
				//a new profile for a customer that already has a profile id
				paymentProfileId = 0;
				profileMgr = ProfileManager.CreateProfile(customerId, email, out errorMessage);

				if(profileMgr == null)
					return 0;

				profileId = profileMgr.ProfileId;

				if(profileId <= 0)
					return 0;

				DataUtility.SaveProfileId(customerId, profileId);
			}

			AuthorizeNetApi.CustomerAddressType cimAddress = DataUtility.GetCustomerAddressFromAddress(addressId);

			if(paymentProfileId <= 0)
			{
				// create new payment profile
				var paymentProfileResponse = profileMgr.CreatePaymentProfile(cimAddress, cardNumber.Trim(), cardCode.Trim(),
					int.Parse(expMonth), int.Parse(expYear));
				if(paymentProfileResponse == null)
				{
					errorMessage = "Null payment profile response.";
					return 0;
				}

				if(!paymentProfileResponse.Success)
				{
					errorMessage = paymentProfileResponse.ErrorMessage;
					errorCode = paymentProfileResponse.ErrorCode;
					return 0;
				}

				paymentProfileId = paymentProfileResponse.PaymentProfileId;
			}
			else
			{
				// update profile
				var response = profileMgr.UpdatePaymentProfile(paymentProfileId, cimAddress, cardNumber.Trim(), cardCode.Trim(),
					int.Parse(expMonth), int.Parse(expYear));

				errorCode = response.ErrorCode;
				errorMessage = response.ErrorMessage;
			}

			if(paymentProfileId > 0)
			{
				string cardType = DetectCreditCardType(cardNumber.Trim());
				DataUtility.SavePaymentProfile(customerId,
					addressId, paymentProfileId,
					expMonth, expYear,
					cardType != null ? cardType : string.Empty);
			}

			return paymentProfileId;
		}

		public static string DetectCreditCardType(string cardNo)
		{
			cardNo = cardNo.Replace(" ", "").Replace("-", "");
			if(cardNo.Length < 6)
				return null;

			int cardNum = 0;
			int.TryParse(cardNo.Substring(0, 2), out cardNum);
			switch(cardNum)
			{
				case 34:
				case 37:
					return "AMEX";
				case 36:
					return "Diners Club";
				case 38:
					return "Carte Blanche";
			}

			if(cardNum >= 51 && cardNum <= 55)
				return "MasterCard";

			cardNum = 0;
			int.TryParse(cardNo.Substring(0, 4), out cardNum);
			switch(cardNum)
			{
				case 2014:
				case 2149:
					return "enRoute";
				case 2131:
				case 1800:
					return "JCB";
				case 6011:
					return "Discover";
			}

			cardNum = 0;
			int.TryParse(cardNo.Substring(0, 3), out cardNum);
			if(cardNum >= 300 && cardNum <= 305)
				return "Diners Club";

			cardNum = 0;
			int.TryParse(cardNo.Substring(0, 1), out cardNum);
			switch(cardNum)
			{
				case 3:
					return "JCB";
				case 4:
					return "Visa";
			}

			return string.Empty;
		}
	}
}
