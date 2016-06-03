// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using AspDotNetStorefront.Routing;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontGateways.Processors
{
	public static class PayPalController
	{
		public const String BN = "AspDotNet" + "Storefront" + "_Cart"; // Do not change this line or your paypal website calls may not work!
		private const String API_VER = "98";
		private const Int16 ECFaultRedirectErrorCode = 10486;

		// ProcessPaypal() is used for Express Checkout and PayPal payments.
		// Credit Card processing via Website Payments Pro is handled by ProcessCard(),
		// just like other credit card gateways.
		static public String ProcessPaypal(int OrderNumber, int CustomerID, Decimal OrderTotal, bool useLiveTransactions, String TransactionMode, Address UseBillingAddress, Address UseShippingAddress, String CAVV, String ECI, String XID, out String AVSResult, out String AuthorizationResult, out String AuthorizationCode, out String AuthorizationTransID, out String TransactionCommandOut, out String TransactionResponse)
		{
			String result = AppLogic.ro_OK;

			AuthorizationCode = String.Empty;
			AuthorizationResult = String.Empty;
			AuthorizationTransID = String.Empty;
			if(!String.IsNullOrEmpty(XID))
			{
				AuthorizationTransID = CommonLogic.IIF(TransactionMode == AppLogic.ro_TXModeAuthOnly, "AUTH=", "CAPTURE=") + XID;
			}
			AVSResult = String.Empty;
			TransactionCommandOut = String.Empty;
			TransactionResponse = String.Empty;
			return result;
		}

		static public String GetTransactionState(String PaymentStatus, String PendingReason)
		{
			String result = String.Empty;

			switch(PaymentStatus.ToLowerInvariant())
			{
				case "pending":
					switch(PendingReason.ToLowerInvariant())
					{
						case "unilateral":
							result = AppLogic.ro_TXStateCaptured;
							break;
						case "authorization":
							result = AppLogic.ro_TXStateAuthorized;
							break;
						default:
							result = AppLogic.ro_TXStatePending;
							break;
					}
					break;
				case "processed":
				case "completed":
				case "canceled_reversal":
					result = AppLogic.ro_TXStateCaptured;
					break;
				case "denied":
				case "expired":
				case "failed":
				case "voided":
					result = AppLogic.ro_TXStateVoided;
					break;
				case "refunded":
				case "reversed":
					result = AppLogic.ro_TXStateRefunded;
					break;
				default:
					result = AppLogic.ro_TXStateUnknown;
					break;
			}
			return result;
		}

		static public void SetECFaultRedirect(Customer customer, string faultRedirect)
		{
			customer.ThisCustomerSession["EC_FaultRedirect"] = faultRedirect;
		}

		static public ExpressAPIType GetAppropriateExpressType()
		{
			var APIUserName = AppLogic.AppConfig("PayPal.API.Username");
			var APIPassword = AppLogic.AppConfig("PayPal.API.Password");
			var APISigniture = AppLogic.AppConfig("PayPal.API.Signature");
			var APIAcceleratedBoardingEmail = AppLogic.AppConfig("PayPal.API.AcceleratedBoardingEmailAddress");

			// Even if the active gateway is PAYFLOWPRO, if the PayPal.API 
			// credentials are filled in we want to use the PayPal API.
			if(APIAcceleratedBoardingEmail.Length != 0)
				return ExpressAPIType.PayPalAcceleratedBording;
			else if(APIUserName.Length != 0 && APIPassword.Length != 0 && APISigniture.Length != 0)
				return ExpressAPIType.PayPalExpress;
			else if(AppLogic.ActivePaymentGatewayCleaned() == Gateway.ro_GWPAYFLOWPRO)
				return ExpressAPIType.PayFlowPro;
			else
				return ExpressAPIType.NoValidAPIType;
		}

		public static string StartEC(ShoppingCart cart, bool boolBypassOrderReview, IDictionary<string, string> checkoutOptions)
		{
			var payPalRefund = new PayPalAPISoapBinding();
			var payPalBinding = new PayPalAPIAASoapBinding();
			var redirectUrl = new StringBuilder();
			var ecOrderTotal = new BasicAmountType();
			var request = new SetExpressCheckoutReq();
			var requestType = new SetExpressCheckoutRequestType();
			var requestDetails = new SetExpressCheckoutRequestDetailsType();
			var response = new SetExpressCheckoutResponseType();
			var result = string.Empty;
			var urlHelper = DependencyResolver.Current.GetService<UrlHelper>();

			//Express checkout
			GetPaypalRequirements(out payPalRefund, out payPalBinding);

			ecOrderTotal.Value = Localization.CurrencyStringForGatewayWithoutExchangeRate(cart.Total(true));

			if(cart.HasRecurringComponents() && AppLogic.AppConfigBool("Recurring.UseGatewayInternalBilling"))
			{
				//Have to send extra details on the SetExpressCheckoutReq or the token will be invalid for creating a recurring profile later
				var recurringAgreement = new BillingAgreementDetailsType();
				var recurringAgreementList = new List<BillingAgreementDetailsType>();

				recurringAgreement.BillingType = BillingCodeType.RecurringPayments;
				recurringAgreement.BillingAgreementDescription = "Recurring order created on " + System.DateTime.Now.ToShortDateString() + " from " + AppLogic.AppConfig("StoreName");
				recurringAgreementList.Add(recurringAgreement);
				requestDetails.BillingAgreementDetails = recurringAgreementList.ToArray();
			}

			request.SetExpressCheckoutRequest = requestType;
			requestType.SetExpressCheckoutRequestDetails = requestDetails;

			ecOrderTotal.currencyID = (CurrencyCodeType)Enum.Parse(typeof(CurrencyCodeType), AppLogic.AppConfig("Localization.StoreCurrency"), true);
			requestDetails.OrderTotal = ecOrderTotal;

			if(AppLogic.AppConfigBool("PayPal.RequireConfirmedAddress"))
				requestDetails.ReqConfirmShipping = "1";
			else
				requestDetails.ReqConfirmShipping = "0";

			requestDetails.ReturnURL = string.Format("{0}{1}",
				AppLogic.GetStoreHTTPLocation(
					useSsl: true, 
					includeScriptLocation: true, 
					noVirtualNoSlash: true),
				urlHelper.Action(
					actionName: ActionNames.PayPalExpressReturn,
					controllerName: ControllerNames.PayPalExpress));
			if(boolBypassOrderReview)
				requestDetails.ReturnURL = string.Format("{0}?BypassOrderReview=true", requestDetails.ReturnURL);

			requestDetails.CancelURL = string.Format("{0}{1}",
				AppLogic.GetStoreHTTPLocation(
					useSsl: true, 
					includeScriptLocation: true, 
					noVirtualNoSlash: true),
				urlHelper.Action(
					actionName: ActionNames.Index,
					controllerName: ControllerNames.Checkout));
			requestDetails.LocaleCode = AppLogic.AppConfig("PayPal.DefaultLocaleCode");
			requestDetails.PaymentAction = PaymentActionCodeType.Authorization;

			if(AppLogic.TransactionModeIsAuthCapture() || AppLogic.AppConfigBool("PayPal.ForceCapture") || PayPalController.GetAppropriateExpressType() == ExpressAPIType.PayPalAcceleratedBording)
				requestDetails.PaymentAction = PaymentActionCodeType.Sale;

			requestDetails.SolutionType = SolutionTypeType.Sole;
			requestDetails.PaymentActionSpecified = true;
			requestType.Version = API_VER;

			if(!string.IsNullOrWhiteSpace(AppLogic.AppConfig("PayPal.Express.PageStyle")))
				requestDetails.PageStyle = AppLogic.AppConfig("PayPal.Express.PageStyle").Trim();

			if(!string.IsNullOrWhiteSpace(AppLogic.AppConfig("PayPal.Express.HeaderImage")))
				requestDetails.cppheaderimage = AppLogic.AppConfig("PayPal.Express.HeaderImage").Trim();

			if(!string.IsNullOrWhiteSpace(AppLogic.AppConfig("PayPal.Express.HeaderBackColor")))
				requestDetails.cppheaderbackcolor = AppLogic.AppConfig("PayPal.Express.HeaderBackColor").Trim();

			if(!string.IsNullOrWhiteSpace(AppLogic.AppConfig("PayPal.Express.HeaderBorderColor")))
				requestDetails.cppheaderbordercolor = AppLogic.AppConfig("PayPal.Express.HeaderBorderColor").Trim();

			if(!string.IsNullOrWhiteSpace(AppLogic.AppConfig("PayPal.Express.PayFlowColor")))
				requestDetails.cpppayflowcolor = AppLogic.AppConfig("PayPal.Express.PayFlowColor").Trim();

			if(checkoutOptions != null && checkoutOptions.ContainsKey("UserSelectedFundingSource") && checkoutOptions["UserSelectedFundingSource"] == "BML")
			{
				var fundingSourceDetails = new FundingSourceDetailsType();

				fundingSourceDetails.AllowPushFunding = "0";
				fundingSourceDetails.UserSelectedFundingSource = UserSelectedFundingSourceType.BML;
				fundingSourceDetails.UserSelectedFundingSourceSpecified = true;
				requestDetails.FundingSourceDetails = fundingSourceDetails;
			}

			try
			{
				response = payPalBinding.SetExpressCheckout(request);

				if(response.Ack.ToString().StartsWith("success", StringComparison.InvariantCultureIgnoreCase))
				{
					result = AppLogic.ro_OK;
				}
				else
				{
					if(response.Errors != null)
					{
						bool first = true;
						for(int ix = 0; ix < response.Errors.Length; ix++)
						{
							if(!first)
							{
								result += ", ";
							}
							result += "Error: [" + response.Errors[ix].ErrorCode + "] " + response.Errors[ix].LongMessage;
							first = false;
						}
					}
				}

			}
			catch(Exception)
			{
				result = "Failed to start PayPal Express Checkout! Please try another payment method.";
			}

			if(result == AppLogic.ro_OK)
			{
				var useIntegratedCheckout = AppLogic.AppConfigBool("PayPal.Express.UseIntegratedCheckout");

				if(AppLogic.AppConfigBool("UseLiveTransactions") == true)
					redirectUrl.Append(useIntegratedCheckout 
						? AppLogic.AppConfig("PayPal.Express.IntegratedCheckout.LiveURL") 
						: AppLogic.AppConfig("PayPal.Express.LiveURL"));
				else
					redirectUrl.Append(useIntegratedCheckout 
						? AppLogic.AppConfig("PayPal.Express.IntegratedCheckout.SandboxURL") 
						: AppLogic.AppConfig("PayPal.Express.SandboxURL"));

				redirectUrl.Append(useIntegratedCheckout 
					? "?token=" 
					: "?cmd=_express-checkout&token=");

				redirectUrl.Append(response.Token);

				if(boolBypassOrderReview)
					redirectUrl.Append("&useraction=commit");

				// Set active payment method to PayPalExpress
				DB.ExecuteSQL(string.Format("UPDATE Address SET PaymentMethodLastUsed={0} WHERE AddressID={1}",
					DB.SQuote(AppLogic.ro_PMPayPalExpress), 
					cart.ThisCustomer.PrimaryBillingAddressID));
				
				SetECFaultRedirect(cart.ThisCustomer, redirectUrl.ToString());
			}
			else
			{
				var error = new ErrorMessage(HttpUtility.HtmlEncode(result));
				redirectUrl.Append(urlHelper.BuildCheckoutLink(error.MessageId));
			}

			return redirectUrl.ToString();
		}

		public static string GetECDetails(string payPalToken, int customerId)
		{
			var payPalRefund = new PayPalAPISoapBinding();
            var payPalBinding = new PayPalAPIAASoapBinding();
			var payerId = string.Empty;
			var addressStatus = string.Empty;
			var request = new GetExpressCheckoutDetailsReq();
			var requestType = new GetExpressCheckoutDetailsRequestType();
			var response = new GetExpressCheckoutDetailsResponseType();
			var responseType = new GetExpressCheckoutDetailsResponseDetailsType();
			
			GetPaypalRequirements(out payPalRefund, out payPalBinding);

			request.GetExpressCheckoutDetailsRequest = requestType;
			response.GetExpressCheckoutDetailsResponseDetails = responseType;
			requestType.Token = payPalToken;
			requestType.Version = API_VER;

			response = payPalBinding.GetExpressCheckoutDetails(request);

			var payerInfo = response.GetExpressCheckoutDetailsResponseDetails.PayerInfo;

			payerId = payerInfo.PayerID;
			if(string.IsNullOrEmpty(payerId))   // If we don't have a PayerID the transaction must be aborted.
				return string.Empty;

			addressStatus = payerInfo.Address.AddressStatus.ToString();

			//Is address AVS Confirmed or Unconfirmed?
			var requireConfirmedAddress = AppLogic.AppConfigBool("PayPal.Express.AVSRequireConfirmedAddress");
			if(requireConfirmedAddress && !addressStatus.Equals("Confirmed", StringComparison.OrdinalIgnoreCase))
				return "AVSFAILED";
			
			var customer = new Customer(customerId, true);
			customer.UpdateCustomer(
				email: customer.IsRegistered
					? null
					: payerInfo.Payer,
				firstName: payerInfo.PayerName.FirstName,
				lastName: payerInfo.PayerName.LastName,
				phone: payerInfo.Address.Phone != null
					? payerInfo.Address.Phone
					: string.Empty,
				okToEmail: false
				);

			//Use the address from PayPal
			var payPalAddress = Address.FindOrCreateOffSiteAddress(
				customerId: customerId,
				city: payerInfo.Address.CityName,
				stateAbbreviation: AppLogic.GetStateAbbreviation(payerInfo.Address.StateOrProvince, payerInfo.Address.CountryName),
				postalCode: payerInfo.Address.PostalCode,
				countryName: payerInfo.Address.CountryName,
				offSiteSource: AppLogic.ro_PMPayPalExpress,
				firstName: payerInfo.PayerName.FirstName,
				lastName: payerInfo.PayerName.LastName,
				address1: payerInfo.Address.Street1,
				address2: payerInfo.Address.Street2,
				phone: payerInfo.Address.Phone != null
					? payerInfo.Address.Phone
					: null
				);
				
			customer.SetPrimaryAddress(payPalAddress.AddressID, AddressTypes.Billing);
			customer.SetPrimaryAddress(payPalAddress.AddressID, AddressTypes.Shipping);

			return payerId;
		}

		public static String ProcessEC(ShoppingCart cart, decimal OrderTotal, int OrderNumber, String PayPalToken, String PayerID, String TransactionMode, out String AuthorizationResult, out String AuthorizationTransID)
		{
			PayPalAPISoapBinding IPayPalRefund;
			PayPalAPIAASoapBinding IPayPal;
			PayPalController.GetPaypalRequirements(out IPayPalRefund, out IPayPal);
			String result = String.Empty;

			AuthorizationResult = String.Empty;
			AuthorizationTransID = String.Empty;

			DoExpressCheckoutPaymentReq ECRequest = new DoExpressCheckoutPaymentReq();
			DoExpressCheckoutPaymentRequestType varECRequest = new DoExpressCheckoutPaymentRequestType();
			DoExpressCheckoutPaymentRequestDetailsType varECRequestDetails = new DoExpressCheckoutPaymentRequestDetailsType();
			DoExpressCheckoutPaymentResponseType ECResponse = new DoExpressCheckoutPaymentResponseType();
			DoExpressCheckoutPaymentResponseDetailsType varECResponse = new DoExpressCheckoutPaymentResponseDetailsType();

			ECRequest.DoExpressCheckoutPaymentRequest = varECRequest;
			varECRequest.DoExpressCheckoutPaymentRequestDetails = varECRequestDetails;
			ECResponse.DoExpressCheckoutPaymentResponseDetails = varECResponse;

			varECRequestDetails.Token = PayPalToken;
			varECRequestDetails.PayerID = PayerID;

			varECRequestDetails.PaymentAction = PaymentActionCodeType.Authorization;
			if(TransactionMode == AppLogic.ro_TXModeAuthCapture || AppLogic.AppConfigBool("PayPal.ForceCapture") || PayPalController.GetAppropriateExpressType() == ExpressAPIType.PayPalAcceleratedBording)
			{
				varECRequestDetails.PaymentAction = PaymentActionCodeType.Sale;
			}
			varECRequestDetails.PaymentActionSpecified = true;

			PaymentDetailsType ECPaymentDetails = new PaymentDetailsType();
			BasicAmountType ECPaymentOrderTotal = new BasicAmountType();
			ECPaymentOrderTotal.Value = Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal);
			ECPaymentOrderTotal.currencyID =
				(CurrencyCodeType)Enum.Parse(typeof(CurrencyCodeType), AppLogic.AppConfig("Localization.StoreCurrency"), true);
			ECPaymentDetails.InvoiceID = OrderNumber.ToString();
			ECPaymentDetails.Custom = cart.ThisCustomer.CustomerID.ToString();
			ECPaymentDetails.ButtonSource = PayPalController.BN + "_EC_US";
			ECPaymentDetails.NotifyURL = AppLogic.GetStoreHTTPLocation(true) + AppLogic.AppConfig("PayPal.NotificationURL");

			varECRequest.Version = API_VER;

			ECPaymentDetails.OrderTotal = ECPaymentOrderTotal;

			List<PaymentDetailsType> ECPaymentDetailsList = new List<PaymentDetailsType>();

			ECPaymentDetailsList.Add(ECPaymentDetails);

			varECRequestDetails.PaymentDetails = ECPaymentDetailsList.ToArray();

			ECResponse = IPayPal.DoExpressCheckoutPayment(ECRequest);

			if(ECResponse.Ack.ToString().StartsWith("success", StringComparison.InvariantCultureIgnoreCase))
			{
				AuthorizationTransID = CommonLogic.IIF(varECRequestDetails.PaymentAction == PaymentActionCodeType.Sale, "CAPTURE=", "AUTH=") + ECResponse.DoExpressCheckoutPaymentResponseDetails.PaymentInfo[0].TransactionID;
				result = AppLogic.ro_OK;
				AuthorizationResult = ECResponse.Ack.ToString();
				SetECFaultRedirect(cart.ThisCustomer, string.Empty);
			}
			else
			{
				if(ECResponse.Errors != null)
				{
					if(ECResponse.Errors.Any(e => e.ErrorCode == ECFaultRedirectErrorCode.ToString()))
					{
						// Express Checkout failed funding recovery feature returns a 10486 which signifies that 
						//  the user should be redirected back to paypal.
						result =
							AuthorizationResult = AppLogic.ro_PMPayPalExpressRedirect;
					}
					else
					{
						result =
							AuthorizationResult = string.Join(",", ECResponse.Errors.Select(e => e.LongMessage).ToArray());
					}
				}
			}

			return result;
		}

		public static String MakeECRecurringProfile(ShoppingCart cart, int orderNumber, String payPalToken, String payerID, DateTime nextRecurringShipDate)
		{
			PayPalAPISoapBinding IPayPalRefund;
			PayPalAPIAASoapBinding IPayPal;
			PayPalController.GetPaypalRequirements(out IPayPalRefund, out IPayPal);
			String result = String.Empty;

			CreateRecurringPaymentsProfileReq ECRecurringRequest = new CreateRecurringPaymentsProfileReq();
			CreateRecurringPaymentsProfileRequestType varECRequest = new CreateRecurringPaymentsProfileRequestType();
			CreateRecurringPaymentsProfileRequestDetailsType varECRequestDetails = new CreateRecurringPaymentsProfileRequestDetailsType();
			CreateRecurringPaymentsProfileResponseType ECRecurringResponse = new CreateRecurringPaymentsProfileResponseType();

			//Re-Use the Internal Gateway Recurring Billing logic for calculating how much of the order is recurring
			ShoppingCart cartRecur = new ShoppingCart(cart.ThisCustomer.SkinID, cart.ThisCustomer, CartTypeEnum.RecurringCart, orderNumber, false);
			Decimal CartTotalRecur = Decimal.Round(cartRecur.Total(true), 2, MidpointRounding.AwayFromZero);
			Decimal RecurringAmount = CartTotalRecur - CommonLogic.IIF(cartRecur.Coupon.CouponType == CouponTypeEnum.GiftCard, CommonLogic.IIF(CartTotalRecur < cartRecur.Coupon.DiscountAmount, CartTotalRecur, cartRecur.Coupon.DiscountAmount), 0);

			DateIntervalTypeEnum ecRecurringIntervalType = cartRecur.CartItems[0].RecurringIntervalType;    //We currently only support 1 interval per recurring order, so grabbing the first as a default should be safe
			int ecRecurringInterval = cartRecur.CartItems[0].RecurringInterval;

			BasicAmountType ecRecurringAmount = new BasicAmountType();
			ecRecurringAmount.currencyID = (CurrencyCodeType)Enum.Parse(typeof(CurrencyCodeType), AppLogic.AppConfig("Localization.StoreCurrency"), true);
			ecRecurringAmount.Value = RecurringAmount.ToString();

			BillingPeriodDetailsType varECSchedulePaymentDetails = GetECRecurringPeriodDetails(ecRecurringIntervalType, ecRecurringInterval);
			varECSchedulePaymentDetails.Amount = ecRecurringAmount;
			varECSchedulePaymentDetails.TotalBillingCyclesSpecified = false;

			ScheduleDetailsType varECSchedule = new ScheduleDetailsType();
			//Need a better description, but it must match the one sent in StartEC
			varECSchedule.Description = "Recurring order created on " + System.DateTime.Now.ToShortDateString() + " from " + AppLogic.AppConfig("StoreName");
			varECSchedule.MaxFailedPayments = 0;    //Cancel the order if a recurrence fails
			varECSchedule.MaxFailedPaymentsSpecified = true;
			varECSchedule.AutoBillOutstandingAmount = AutoBillType.NoAutoBill;
			varECSchedule.AutoBillOutstandingAmountSpecified = true;
			varECSchedule.PaymentPeriod = varECSchedulePaymentDetails;

			RecurringPaymentsProfileDetailsType varECProfileDetails = new RecurringPaymentsProfileDetailsType();
			varECProfileDetails.SubscriberName = cart.ThisCustomer.FirstName + " " + cart.ThisCustomer.LastName;
			varECProfileDetails.BillingStartDate = nextRecurringShipDate;

			varECRequestDetails.ScheduleDetails = varECSchedule;
			varECRequestDetails.Token = payPalToken;
			varECRequestDetails.RecurringPaymentsProfileDetails = varECProfileDetails;

			if(cart.IsAllDownloadComponents())
			{
				PaymentDetailsItemType varECPaymentDetails = new PaymentDetailsItemType();
				varECPaymentDetails.ItemCategory = ItemCategoryType.Digital;
				varECPaymentDetails.ItemCategorySpecified = true;

				List<PaymentDetailsItemType> ECPaymentDetailsList = new List<PaymentDetailsItemType>();

				ECPaymentDetailsList.Add(varECPaymentDetails);

				varECRequestDetails.PaymentDetailsItem = ECPaymentDetailsList.ToArray();
			}

			varECRequest.Version = API_VER;
			varECRequest.CreateRecurringPaymentsProfileRequestDetails = varECRequestDetails;

			ECRecurringRequest.CreateRecurringPaymentsProfileRequest = varECRequest;

			ECRecurringResponse = IPayPal.CreateRecurringPaymentsProfile(ECRecurringRequest);

			if(ECRecurringResponse.Ack.ToString().StartsWith("success", StringComparison.InvariantCultureIgnoreCase))
			{
				result = AppLogic.ro_OK;
			}
			else
			{
				if(ECRecurringResponse.Errors != null)
				{
					bool first = true;
					for(int ix = 0; ix < ECRecurringResponse.Errors.Length; ix++)
					{
						if(!first)
						{
							result += ", ";
						}
						result += ECRecurringResponse.Errors[ix].LongMessage;
						first = false;
					}
				}
			}

			//Log the transaction
			OrderTransactionCollection ecRecurringOrderTransaction = new OrderTransactionCollection(orderNumber);
			ecRecurringOrderTransaction.AddTransaction("PayPal Express Checkout Recurring Profile Creation",
				ECRecurringRequest.ToString(),
				result,
				payerID,    //PNREF = payerID
				(ECRecurringResponse.CreateRecurringPaymentsProfileResponseDetails.ProfileID == null ? "No ProfileID provided" : ECRecurringResponse.CreateRecurringPaymentsProfileResponseDetails.ProfileID),    //Code = ProfileID
				AppLogic.ro_PMPayPalExpress,
				null,
				RecurringAmount);

			return result;
		}

		public static String CancelECRecurringProfile(int OriginalOrderNumber)
		{
			PayPalAPISoapBinding IPayPalRefund;
			PayPalAPIAASoapBinding IPayPal;
			PayPalController.GetPaypalRequirements(out IPayPalRefund, out IPayPal);
			String profileID = String.Empty;
			String result = String.Empty;

			profileID = GetPPECProfileID(OriginalOrderNumber);

			if(profileID != String.Empty)
			{
				ManageRecurringPaymentsProfileStatusReq ECRecurringCancelRequest = new ManageRecurringPaymentsProfileStatusReq();
				ManageRecurringPaymentsProfileStatusRequestType varECRecurringRequest = new ManageRecurringPaymentsProfileStatusRequestType();
				ManageRecurringPaymentsProfileStatusRequestDetailsType varECRecurringRequestDetails = new ManageRecurringPaymentsProfileStatusRequestDetailsType();
				ManageRecurringPaymentsProfileStatusResponseType varECRecurringResponse = new ManageRecurringPaymentsProfileStatusResponseType();

				varECRecurringRequestDetails.Action = StatusChangeActionType.Cancel;
				varECRecurringRequestDetails.ProfileID = profileID;

				varECRecurringRequest.ManageRecurringPaymentsProfileStatusRequestDetails = varECRecurringRequestDetails;
				varECRecurringRequest.Version = API_VER;

				ECRecurringCancelRequest.ManageRecurringPaymentsProfileStatusRequest = varECRecurringRequest;

				varECRecurringResponse = IPayPal.ManageRecurringPaymentsProfileStatus(ECRecurringCancelRequest);

				if(varECRecurringResponse.Ack.ToString().StartsWith("success", StringComparison.InvariantCultureIgnoreCase))
				{
					result = AppLogic.ro_OK;
				}
				else
				{
					if(varECRecurringResponse.Errors != null)
					{
						bool first = true;
						for(int ix = 0; ix < varECRecurringResponse.Errors.Length; ix++)
						{
							if(!first)
							{
								result += ", ";
							}
							result += varECRecurringResponse.Errors[ix].LongMessage;
							first = false;
						}
					}
				}
			}
			else
			{
				result = "No matching ProfileID found for that order number";

				SysLog.LogMessage("An attempt was made to cancel a PayPal express recurring order with no matching ProfileID",
					"Original order ID was: " + OriginalOrderNumber.ToString(),
					MessageTypeEnum.Informational,
					MessageSeverityEnum.Alert);
			}

			return result;
		}

		public static void GetPaypalRequirements(out PayPalAPISoapBinding IPayPalRefund, out PayPalAPIAASoapBinding IPayPal)
		{
			IPayPal = new PayPalAPIAASoapBinding();
			IPayPalRefund = new PayPalAPISoapBinding();

			if(AppLogic.AppConfigBool("UseLiveTransactions"))
			{
				IPayPal.Url = AppLogic.AppConfig("PayPal.API.LiveURL");
			}
			else
			{
				IPayPal.Url = AppLogic.AppConfig("PayPal.API.TestURL");
			}
			IPayPalRefund.Url = IPayPal.Url;

			IPayPal.UserAgent = HttpContext.Current.Request.UserAgent;
			IPayPalRefund.UserAgent = IPayPal.UserAgent;

			UserIdPasswordType PayPalUser = new UserIdPasswordType();
			if(PayPalController.GetAppropriateExpressType() == ExpressAPIType.PayPalAcceleratedBording)
			{
				PayPalUser.Subject = AppLogic.AppConfig("PayPal.API.AcceleratedBoardingEmailAddress");
			}
			else
			{
				PayPalUser.Username = AppLogic.AppConfig("PayPal.API.Username");
				PayPalUser.Password = AppLogic.AppConfig("PayPal.API.Password");
				PayPalUser.Signature = AppLogic.AppConfig("PayPal.API.Signature");

				//Subject should be the Sellers e-mail address (if you are using 3-part API calls) with the correct account permissions. You also have 
				//set up permissions for this e-mail address for the "type" of transaction you want to allow.
				//This access changes are made in the Sandbox.
				//The name of the entity on behalf of which this profile is issuing calls
				//This is for Third-Party access
				// You have to set up Virtual Terminals and complete the Billing Agreement in the Sandbox before you can make Direct Payments
				PayPalUser.Subject = AppLogic.AppConfig("PayPal.API.MerchantEMailAddress");
			}



			CustomSecurityHeaderType CSecHeaderType = new CustomSecurityHeaderType();
			CSecHeaderType.Credentials = PayPalUser;
			CSecHeaderType.MustUnderstand = true;

			IPayPal.RequesterCredentials = CSecHeaderType;
			IPayPalRefund.RequesterCredentials = CSecHeaderType;
		}

		public static BillingPeriodDetailsType GetECRecurringPeriodDetails(DateIntervalTypeEnum ecRecurringIntervalType, int ecRecurringInterval)
		{
			BillingPeriodDetailsType ecBillingPeriod = new BillingPeriodDetailsType();

			switch(ecRecurringIntervalType)
			{
				case DateIntervalTypeEnum.Day:
					ecBillingPeriod.BillingFrequency = ecRecurringInterval;
					ecBillingPeriod.BillingPeriod = BillingPeriodType.Day;
					break;
				case DateIntervalTypeEnum.Week:
					ecBillingPeriod.BillingFrequency = ecRecurringInterval;
					ecBillingPeriod.BillingPeriod = BillingPeriodType.Week;
					break;
				case DateIntervalTypeEnum.Month:
					ecBillingPeriod.BillingFrequency = ecRecurringInterval;
					ecBillingPeriod.BillingPeriod = BillingPeriodType.Month;
					break;
				case DateIntervalTypeEnum.Year:
					ecBillingPeriod.BillingFrequency = ecRecurringInterval;
					ecBillingPeriod.BillingPeriod = BillingPeriodType.Year;
					break;
				case DateIntervalTypeEnum.Weekly:
					ecBillingPeriod.BillingFrequency = 1;
					ecBillingPeriod.BillingPeriod = BillingPeriodType.Week;
					break;
				case DateIntervalTypeEnum.BiWeekly:
					ecBillingPeriod.BillingFrequency = 1;
					ecBillingPeriod.BillingPeriod = BillingPeriodType.SemiMonth;
					break;
				case DateIntervalTypeEnum.EveryFourWeeks:
					ecBillingPeriod.BillingFrequency = 4;
					ecBillingPeriod.BillingPeriod = BillingPeriodType.Week;
					break;
				case DateIntervalTypeEnum.Monthly:
					ecBillingPeriod.BillingFrequency = 1;
					ecBillingPeriod.BillingPeriod = BillingPeriodType.Month;
					break;
				case DateIntervalTypeEnum.Quarterly:
					ecBillingPeriod.BillingFrequency = 3;
					ecBillingPeriod.BillingPeriod = BillingPeriodType.Month;
					break;
				case DateIntervalTypeEnum.SemiYearly:
					ecBillingPeriod.BillingFrequency = 6;
					ecBillingPeriod.BillingPeriod = BillingPeriodType.Month;
					break;
				case DateIntervalTypeEnum.Yearly:
					ecBillingPeriod.BillingFrequency = 1;
					ecBillingPeriod.BillingPeriod = BillingPeriodType.Year;
					break;
				default:    //Default to monthly like we do elsewhere
					ecBillingPeriod.BillingFrequency = ecRecurringInterval;
					ecBillingPeriod.BillingPeriod = BillingPeriodType.Month;
					break;
			}

			return ecBillingPeriod;
		}

		public static string GetPPECProfileID(int OriginalOrderNumber)
		{
			String profileID = string.Empty;

			String profileIDSql = "SELECT TOP 1 Code FROM OrderTransaction WHERE OrderNumber = @OrderNumber AND TransactionCommand = @TransactionCommand AND PaymentMethod = @PaymentMethod";

			SqlParameter[] profileIDParams = { new SqlParameter("@OrderNumber", OriginalOrderNumber),
                                                 new SqlParameter("@TransactionCommand", "CreateRecurringPaymentsProfileReq"),
                                                 new SqlParameter("@PaymentMethod", AppLogic.ro_PMPayPalExpress) };

			using(SqlConnection conn = new SqlConnection(DB.GetDBConn()))
			{
				conn.Open();
				using(IDataReader rs = DB.GetRS(profileIDSql, profileIDParams, conn))
				{
					while(rs.Read())
					{
						profileID = DB.RSField(rs, "Code");
					}
				}
			}

			return profileID;
		}
	}

	public enum ExpressAPIType
	{
		PayFlowPro,
		PayPalExpress,
		PayPalAcceleratedBording,
		NoValidAPIType
	}
}
