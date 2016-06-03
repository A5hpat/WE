// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data.SqlClient;
using System.Text;
using System.Web.Mvc;
using AspDotNetStorefront.Caching.ObjectCaching;
using AspDotNetStorefront.Checkout;
using AspDotNetStorefront.Filters;
using AspDotNetStorefront.Models;
using AspDotNetStorefrontCore;
using AspDotNetStorefront.Routing;
using AspDotNetStorefrontGateways;

namespace AspDotNetStorefront.Controllers
{
	public class ThreeDSecureController : Controller
	{
		readonly ICachedShoppingCartProvider CachedShoppingCartProvider;
		readonly NoticeProvider NoticeProvider;
		readonly IPersistedCheckoutContextProvider PersistedCheckoutContextProvider;

		public ThreeDSecureController(
			ICachedShoppingCartProvider cachedShoppingCartProvider,
			NoticeProvider noticeProvider,
			IPersistedCheckoutContextProvider persistedCheckoutContextProvider)
		{
			CachedShoppingCartProvider = cachedShoppingCartProvider;
			NoticeProvider = noticeProvider;
			PersistedCheckoutContextProvider = persistedCheckoutContextProvider;
		}

		[HttpGet, ImportModelStateFromTempData]
		public ActionResult BraintreeThreeDSecureFail()
		{
			var customer = HttpContext.GetCustomer();
			var context = PersistedCheckoutContextProvider.LoadCheckoutContext(customer);

			var updatedCheckoutContext = new PersistedCheckoutContext(
				creditCard: context.CreditCard,
				payPalExpress: context.PayPalExpress,
				purchaseOrder: context.PurchaseOrder,
				braintree: new BraintreeDetails(
					nonce: context.Braintree.Nonce,
                    token: context.Braintree.Token,
					paymentMethod: context.Braintree.PaymentMethod,
					threeDSecureApproved: false),
				amazonPayments: context.AmazonPayments,
				termsAndConditionsAccepted: context.TermsAndConditionsAccepted,
				over13Checked: context.Over13Checked,
				shippingEstimateDetails: context.ShippingEstimateDetails,
				offsiteRequiresBillingAddressId: null,
				offsiteRequiresShippingAddressId: null,
				email: context.Email,
				selectedShippingMethodId: context.SelectedShippingMethodId);

			PersistedCheckoutContextProvider.SaveCheckoutContext(customer, updatedCheckoutContext);

			NoticeProvider.PushNotice("3dSecure verification failed, this order cannot be accepted.", NoticeType.Failure);

			return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
		}

		[HttpGet, ImportModelStateFromTempData]
		public ActionResult BraintreeThreeDSecurePass(string nonce)
		{
			var customer = HttpContext.GetCustomer();
			var context = PersistedCheckoutContextProvider.LoadCheckoutContext(customer);
			var cart = CachedShoppingCartProvider.Get(customer, CartTypeEnum.ShoppingCart, AppLogic.StoreID());
			var orderNumber = customer.ThisCustomerSession.SessionUSInt("3Dsecure.OrderNumber");

			var updatedCheckoutContext = new PersistedCheckoutContext(
				creditCard: context.CreditCard,
				payPalExpress: context.PayPalExpress,
				purchaseOrder: context.PurchaseOrder,
				braintree: new BraintreeDetails(
					nonce: nonce,   //We got a new nonce after the 3dSecure request
					token: context.Braintree.Token,
					paymentMethod: context.Braintree.PaymentMethod,
					threeDSecureApproved: true),
				amazonPayments: context.AmazonPayments,
				termsAndConditionsAccepted: context.TermsAndConditionsAccepted,
				over13Checked: context.Over13Checked,
				shippingEstimateDetails: context.ShippingEstimateDetails,
				offsiteRequiresBillingAddressId: null,
				offsiteRequiresShippingAddressId: null,
				email: context.Email,
				selectedShippingMethodId: context.SelectedShippingMethodId);

			PersistedCheckoutContextProvider.SaveCheckoutContext(customer, updatedCheckoutContext);

			customer.ThisCustomerSession[AppLogic.Braintree3dSecureKey] = "true";
			customer.ThisCustomerSession[AppLogic.BraintreeNonceKey] = nonce;
			customer.ThisCustomerSession[AppLogic.BraintreePaymentMethod] = context.Braintree.PaymentMethod;

			var status = Gateway.MakeOrder(string.Empty, AppLogic.TransactionMode(), cart, orderNumber, string.Empty, string.Empty, string.Empty, string.Empty);
			ClearThreeDSecureSessionInfo(customer);

			if(status == AppLogic.ro_OK)
			{
				return RedirectToAction(
					ActionNames.Confirmation,
					ControllerNames.CheckoutConfirmation,
					new { @orderNumber = orderNumber });
			}

			NoticeProvider.PushNotice(string.Format("Unknown Result. Message={0}. Please retry your credit card.", status), NoticeType.Failure);
			return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
		}

		[PageTypeFilter(PageTypes.Checkout)]
		[HttpGet, ImportModelStateFromTempData]
		public ActionResult ThreeDSecure()
		{			
			//Braintree has its own 3dSecure form
			if(AppLogic.ActivePaymentGatewayCleaned() == Gateway.ro_GWBRAINTREE)
			{
				var customer = HttpContext.GetCustomer();
				var context = PersistedCheckoutContextProvider.LoadCheckoutContext(customer);
				var cart = new ShoppingCart(customer.SkinID, customer, CartTypeEnum.ShoppingCart, 0, false);

				var braintreeModel = new BraintreeThreeDSecureViewModel(
					nonce: context.Braintree.Nonce,
					scriptUrl: AppLogic.AppConfig("Braintree.ScriptUrl"),
					token: context.Braintree.Token,
					total: cart.Total(true).ToString());

				return View(ViewNames.BraintreeThreeDSecureForm, braintreeModel);
			}
			else
			{
				var threeDSecureModel = new ThreeDSecureFrameViewModel
				{
					FrameUrl = Url.Action(ActionNames.ThreeDSecureForm, ControllerNames.ThreeDSecure, null, this.Request.Url.Scheme)
				};

				return View(threeDSecureModel);
			}
		}

		[PageTypeFilter(PageTypes.Checkout)]
		[HttpGet, ImportModelStateFromTempData]
		public ActionResult ThreeDSecureForm()
		{
			var customer = HttpContext.GetCustomer();
			var useCardinal = AppLogic.AppConfigBool("CardinalCommerce.Centinel.Enabled");

			var model = new ThreeDSecureViewModel
			{
				ACSUrl = useCardinal
					? customer.ThisCustomerSession["Cardinal.ACSURL"]
					: customer.ThisCustomerSession["3Dsecure.ACSURL"],
				PaReq = useCardinal
					? customer.ThisCustomerSession["Cardinal.Payload"]
					: customer.ThisCustomerSession["3Dsecure.paReq"],
				MD = useCardinal
					? "None"
					: customer.ThisCustomerSession["3DSecure.MD"],
				TermUrl = Url.Action(ActionNames.ThreeDSecureReturn, ControllerNames.ThreeDSecure, null, this.Request.Url.Scheme)
			};

			return View(model);
		}

		[HttpPost]
		public ActionResult ThreeDSecureReturn()
		{
			var customer = HttpContext.GetCustomer();
			var useCardinal = AppLogic.AppConfigBool("CardinalCommerce.Centinel.Enabled");

			if(ShoppingCart.CartIsEmpty(customer.CustomerID, CartTypeEnum.ShoppingCart))
			{
				ClearThreeDSecureSessionInfo(customer);
				return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
			}

			if(useCardinal)
				return Redirect(ProcessCardinalReturn(customer));
			else
				return Redirect(ProcessNativeThreeDSecureReturn(customer));
		}

		string ProcessCardinalReturn(Customer customer)
		{
			var cart = CachedShoppingCartProvider.Get(customer, CartTypeEnum.ShoppingCart, AppLogic.StoreID());
			var payload = customer.ThisCustomerSession["Cardinal.Payload"];
			var paRes = CommonLogic.FormCanBeDangerousContent("PaRes")
				.Replace(" ", "")
				.Replace("\r", "")
				.Replace("\n", "");
			var transactionId = customer.ThisCustomerSession["Cardinal.TransactionID"];
			var orderNumber = customer.ThisCustomerSession.SessionUSInt("Cardinal.OrderNumber");

			if(orderNumber == 0
				|| string.IsNullOrEmpty(payload)
				|| string.IsNullOrEmpty(transactionId))
			{
				NoticeProvider.PushNotice("Bank verification was incomplete or canceled. Please retry credit card entry", NoticeType.Failure);
				ClearThreeDSecureSessionInfo(customer);
				return Url.Action(ActionNames.Index, ControllerNames.Checkout);
			}

			var cardinalAuthenticateResult = string.Empty;
			var paResStatus = string.Empty;
			var signatureVerification = string.Empty;
			var errorNumber = string.Empty;
			var errorDescription = string.Empty;

			var AuthResult = Cardinal.PreChargeAuthenticate(orderNumber, 
				paRes,
				transactionId, 
				out paResStatus, 
				out signatureVerification, 
				out errorNumber, 
				out errorDescription, 
				out cardinalAuthenticateResult);

			customer.ThisCustomerSession["Cardinal.AuthenticateResult"] = cardinalAuthenticateResult;
			
			if(((paResStatus == "Y" || paResStatus == "A") && signatureVerification == "Y") //Great success
				|| (paResStatus == "U" && errorNumber == "0"))	//Signature verification failed but Cardinal says to take it anyway
			{
				var cardExtraCode = CommonLogic.ExtractToken(customer.ThisCustomerSession["Cardinal.AuthenticateResult"], "<Cavv>", "</Cavv>");
				var eciFlag = CommonLogic.ExtractToken(customer.ThisCustomerSession["Cardinal.AuthenticateResult"], "<EciFlag>", "</EciFlag>");
				var XID = CommonLogic.ExtractToken(customer.ThisCustomerSession["Cardinal.AuthenticateResult"], "<Xid>", "</Xid>");

				var billingAddress = new Address();
				billingAddress.LoadByCustomer(customer.CustomerID, customer.PrimaryBillingAddressID, AddressTypes.Billing);

				var status = Gateway.MakeOrder(string.Empty, AppLogic.TransactionMode(), cart, orderNumber, cardExtraCode, eciFlag, XID, string.Empty);

				if(status != AppLogic.ro_OK)
				{
					NoticeProvider.PushNotice(status, NoticeType.Failure);
					ClearThreeDSecureSessionInfo(customer);
					return Url.Action(ActionNames.Index, ControllerNames.Checkout);
				}

				DB.ExecuteSQL(string.Format("UPDATE Orders SET CardinalLookupResult = {0}, CardinalAuthenticateResult = {1} WHERE OrderNumber= {2}",
					DB.SQuote(customer.ThisCustomerSession["Cardinal.LookupResult"]),
					DB.SQuote(customer.ThisCustomerSession["Cardinal.AuthenticateResult"]),
					orderNumber));

				return Url.Action(
					ActionNames.Confirmation,
					ControllerNames.CheckoutConfirmation,
					new { @orderNumber = orderNumber });
			}

			//If we made it this far, either something failed or Authorization or Signature Verification didn't pass on Cardinal's end
			NoticeProvider.PushNotice("We were unable to verify your credit card. Please retry your credit card or choose a different payment type.", NoticeType.Failure);
			ClearThreeDSecureSessionInfo(customer);
			return Url.Action(ActionNames.Index, ControllerNames.Checkout);
		}

		string ProcessNativeThreeDSecureReturn(Customer customer)
		{
			var paReq = customer.ThisCustomerSession["3Dsecure.paReq"];
			var paRes = CommonLogic.FormCanBeDangerousContent("PaRes")
				.Replace(" ", "")
				.Replace("\r", "")
				.Replace("\n", "");
			var merchantData = CommonLogic.FormCanBeDangerousContent("MD");
			var transactionId = customer.ThisCustomerSession["3Dsecure.XID"];
			var orderNumber = customer.ThisCustomerSession.SessionUSInt("3Dsecure.OrderNumber");

			if(!string.IsNullOrEmpty(paRes))
				customer.ThisCustomerSession["3Dsecure.PaRes"] = paRes;

			if(merchantData != customer.ThisCustomerSession["3Dsecure.MD"]
				|| orderNumber == 0
				|| string.IsNullOrEmpty(paReq)
				|| string.IsNullOrEmpty(transactionId))
			{
				NoticeProvider.PushNotice("Session Expired. Please retry credit card entry", NoticeType.Failure);
				ClearThreeDSecureSessionInfo(customer);
				return Url.Action(ActionNames.Index, ControllerNames.Checkout);
			}

			var cart = CachedShoppingCartProvider.Get(customer, CartTypeEnum.ShoppingCart, AppLogic.StoreID());
			var status = Gateway.MakeOrder(string.Empty, AppLogic.TransactionMode(), cart, orderNumber, string.Empty, string.Empty, string.Empty, string.Empty);

			// The session may have changed in MakeOrder, so get the latest values from the DB
			CustomerSession cSession = new CustomerSession(customer.CustomerID);

			if(status == AppLogic.ro_OK)
			{
				if(!string.IsNullOrEmpty(cSession["3DSecure.LookupResult"]))
				{
					// the data in this session variable will be encoded, so decode it before saving to the database
					var decodedBytes = Convert.FromBase64String(cSession["3DSecure.LookupResult"]);
					var lookupResult = Encoding.UTF8.GetString(decodedBytes);

					DB.ExecuteSQL("UPDATE Orders SET CardinalLookupResult = @CardinalLookupResult WHERE OrderNumber = @OrderNumber",
						new SqlParameter[] {
							new SqlParameter("@CardinalLookupResult", lookupResult),
							new SqlParameter("@OrderNumber", orderNumber) });

					cSession["3DSecure.LookupResult"] = string.Empty;
				}

				ClearThreeDSecureSessionInfo(customer);
				return Url.Action(
					ActionNames.Confirmation,
					ControllerNames.CheckoutConfirmation,
					new { @orderNumber = orderNumber });
			}

			NoticeProvider.PushNotice(string.Format("Unknown Result. Message={0}. Please retry your credit card.", status), NoticeType.Failure);
			ClearThreeDSecureSessionInfo(customer);
			return Url.Action(ActionNames.Index, ControllerNames.Checkout);
		}

		void ClearThreeDSecureSessionInfo(Customer customer)
		{
			customer.ThisCustomerSession["3DSecure.CustomerID"] = string.Empty;
			customer.ThisCustomerSession["3DSecure.OrderNumber"] = string.Empty;
			customer.ThisCustomerSession["3DSecure.ACSUrl"] = string.Empty;
			customer.ThisCustomerSession["3DSecure.paReq"] = string.Empty;
			customer.ThisCustomerSession["3DSecure.XID"] = string.Empty;
			customer.ThisCustomerSession["3DSecure.MD"] = string.Empty;
			customer.ThisCustomerSession["3Dsecure.PaRes"] = string.Empty;
			customer.ThisCustomerSession["Cardinal.LookupResult"] = string.Empty;
			customer.ThisCustomerSession["Cardinal.AuthenticateResult"] = string.Empty;
			customer.ThisCustomerSession["Cardinal.ACSUrl"] = string.Empty;
			customer.ThisCustomerSession["Cardinal.Payload"] = string.Empty;
			customer.ThisCustomerSession["Cardinal.TransactionID"] = string.Empty;
			customer.ThisCustomerSession["Cardinal.OrderNumber"] = string.Empty;
			customer.ThisCustomerSession["Cardinal.LookupResult"] = string.Empty;
		}
    }
}
