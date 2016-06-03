// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AspDotNetStorefront.Auth;
using AspDotNetStorefront.Checkout;
using AspDotNetStorefront.Filters;
using AspDotNetStorefront.Models;
using AspDotNetStorefront.Routing;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways;

namespace AspDotNetStorefront.Controllers
{
	[Authorize]
	[SecureAccessFilter(forceHttps: true)]
	public class CheckoutConfirmationController : Controller
	{
		readonly ICheckoutAccountStatusProvider CheckoutAccountStatusProvider;
		readonly NoticeProvider NoticeProvider;
		readonly IPersistedCheckoutContextProvider PersistedCheckoutContextProvider;

		public CheckoutConfirmationController(
			ICheckoutAccountStatusProvider checkoutAccountStatusProvider,
			NoticeProvider noticeProvider,
			IPersistedCheckoutContextProvider persistedCheckoutContextProvider)
		{
			CheckoutAccountStatusProvider = checkoutAccountStatusProvider;
			NoticeProvider = noticeProvider;
			PersistedCheckoutContextProvider = persistedCheckoutContextProvider;
		}

		[PageTypeFilter(PageTypes.OrderConfirmation)]
		public ActionResult Confirmation(int orderNumber)
		{
			var customer = HttpContext.GetCustomer();
			var checkoutContext = PersistedCheckoutContextProvider.LoadCheckoutContext(customer);
			var order = new Order(orderNumber, customer.LocaleSetting);

			//Missing info
			if(customer.CustomerID == 0 || orderNumber == 0)
			{
				NoticeProvider.PushNotice("Invalid Customer ID or Invalid Order Number", NoticeType.Failure);
				return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
			}

			//No such order
			if(order.IsEmpty)
			{
				NoticeProvider.PushNotice("No order could be found in the database...Please contact us for more information.", NoticeType.Failure);
				return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
			}

			//Wrong customer
			if(customer.CustomerID != order.CustomerID)
			{
				return RedirectToAction(ActionNames.Detail, ControllerNames.Topic, new { @name = "ordernotfound" });
			}

			if(customer.ThisCustomerSession["3DSecure.LookupResult"].Length > 0)
			{
				var sqlParams = new SqlParameter[]
				{
					new SqlParameter("@LookupResult", customer.ThisCustomerSession["3DSecure.LookupResult"]),
					new SqlParameter("@OrderNumber", orderNumber)
				};

				DB.ExecuteSQL("UPDATE Orders SET CardinalLookupResult = @LookupResult WHERE OrderNumber = @OrderNumber", sqlParams);
			}

			//Order cleanup
			if(!order.AlreadyConfirmed)
			{
				ViewBag.OrderAlreadyConfirmed = false; // Adding a variable to the viewbag so that xmlpackages can tell the order has not yet been confirmed
				var paymentMethod = AppLogic.CleanPaymentMethod(order.PaymentMethod);

				DB.ExecuteSQL("update Customer set OrderOptions=NULL, OrderNotes=NULL, FinalizationData=NULL where CustomerID=" + customer.CustomerID.ToString());

				//New order notification
				AppLogic.SendOrderEMail(customer, orderNumber, false, paymentMethod, true);

				//Low inventory notification
				if(AppLogic.AppConfigBool("SendLowStockWarnings") && order.TransactionIsCaptured()) //If delayed capture, we'll check this when the order is captured
				{
					List<int> purchasedVariants = new List<int>();
					foreach(CartItem ci in order.CartItems)
					{
						purchasedVariants.Add(ci.VariantID);
					}

					AppLogic.LowInventoryWarning(purchasedVariants);
				}

				//Handle impersonation
				var impersonationValue = customer.ThisCustomerSession[AppLogic.ImpersonationSessionKey];
				if(!string.IsNullOrEmpty(impersonationValue))
				{
					int impersonatorId = 0;

					if(int.TryParse(impersonationValue, out impersonatorId))
					{
						var impersonator = new Customer(impersonatorId);
						var impersonationSql = "UPDATE Orders SET Notes = Notes + @ImpersonationNote WHERE OrderNumber = @OrderNumber";
						var impersonationSqlParams = new SqlParameter[]
						{
							new SqlParameter("@OrderNumber", orderNumber),
							new SqlParameter("@ImpersonationNote", string.Format("This order was placed for the customer by {0}", impersonator.EMail))
						};

						DB.ExecuteSQL(impersonationSql, impersonationSqlParams);
						customer.ThisCustomerSession.ClearVal(AppLogic.ImpersonationSessionKey);
					}

				}

				//Braintree cleanup
				if(order.PaymentGateway == Gateway.ro_GWBRAINTREE)
				{
					//Clear out some session values we don't need anymore
					customer.ThisCustomerSession.ClearVal(AppLogic.Braintree3dSecureKey);
					customer.ThisCustomerSession.ClearVal(AppLogic.BraintreeNonceKey);
					customer.ThisCustomerSession.ClearVal(AppLogic.BraintreePaymentMethod);
				}

				//Make sure we don't do this again
				DB.ExecuteSQL("UPDATE Orders SET AlreadyConfirmed = 1 WHERE OrderNumber = @OrderNumber", new SqlParameter[] { new SqlParameter("@OrderNumber", orderNumber) });
			}

			//Build the return model
			var body = string.Empty;
			var googleTrackingCode = new Topic("GoogleTrackingCode").Contents;
			var generalTrackingCode = new Topic("ConfirmationTracking").Contents;
			var showGeneralTrackingCode = !string.IsNullOrEmpty(generalTrackingCode) 
				&& !order.AlreadyConfirmed;
			var showGoogleTrackingCode = AppLogic.AppConfigBool("IncludeGoogleTrackingCode") 
				&& !order.AlreadyConfirmed;
			var showGoogleTrustedStores = AppLogic.AppConfigBool("GoogleTrustedStoreEnabled") 
				&& !string.IsNullOrEmpty(AppLogic.AppConfig("GoogleTrustedStoreID"))
				&& !order.AlreadyConfirmed;

			var xmlPackage = "page.orderconfirmation.xml.config";

			if(string.IsNullOrEmpty(xmlPackage))
				xmlPackage = "page.orderconfirmation.xml.config";

			body = AppLogic.RunXmlPackage(xmlPackage, new Parser(), customer, customer.SkinID, String.Empty, "OrderNumber=" + orderNumber.ToString(), true, true);

			if(showGoogleTrackingCode)
			{
				if(!string.IsNullOrEmpty(googleTrackingCode))
					googleTrackingCode = googleTrackingCode
						.Replace("(!ORDERTOTAL!)", Localization.CurrencyStringForGatewayWithoutExchangeRate(order.Total()))
						.Replace("(!ORDERNUMBER!)", orderNumber.ToString())
						.Replace("(!CUSTOMERID!)", customer.CustomerID.ToString());
			}

			if(showGeneralTrackingCode)
				generalTrackingCode = generalTrackingCode
					.Replace("(!ORDERTOTAL!)", Localization.CurrencyStringForGatewayWithoutExchangeRate(order.Total()))
					.Replace("(!ORDERNUMBER!)", orderNumber.ToString())
					.Replace("(!CUSTOMERID!)", customer.CustomerID.ToString());

			var model = new OrderConfirmationViewModel(
				orderNumber: orderNumber,
				body: body,
				googleTrackingCode: googleTrackingCode,
				generalTrackingCode: generalTrackingCode,
				showGoogleTrackingCode: showGoogleTrackingCode,
				showGeneralTrackingCode: showGeneralTrackingCode,
				showGoogleTrustedStores: showGoogleTrustedStores,
				addPayPalIntegratedCheckoutScript: AppLogic.AppConfigBool("PayPal.Express.UseIntegratedCheckout")
					&& !order.AlreadyConfirmed,
				addBuySafeScript: AppLogic.GlobalConfigBool("BuySafe.Enabled")
					&& !string.IsNullOrEmpty(AppLogic.GlobalConfig("BuySafe.Hash"))
					&& !order.AlreadyConfirmed);

			//Get rid of old data - do this at the very end so we have all the info we need for order processing and building the model above
			ClearSensitiveOrderData(customer);

			if(!customer.IsRegistered || AppLogic.AppConfigBool("ForceSignoutOnOrderCompletion"))
				ClearCustomerSession(customer);

			return View(model);
		}

		public ActionResult GoogleTrustedStores(int orderNumber)
		{
			if(!AppLogic.AppConfigBool("GoogleTrustedStoreEnabled") || string.IsNullOrEmpty(AppLogic.AppConfig("GoogleTrustedStoreID")))
				return Content(string.Empty);

			var customer = HttpContext.GetCustomer();
			var order = new Order(orderNumber, customer.LocaleSetting);

			var productSearchStoreId = AppLogic.AppConfig("GoogleTrustedStoreProductSearchID");
			var country = AppLogic.AppConfig("GoogleTrustedStoreCountry");
			var language = AppLogic.AppConfig("GoogleTrustedStoreLanguage");

			var model = new GoogleTrustedStoresViewModel(
				orderNumber: orderNumber,
				domain: AppLogic.AppConfig("LiveServer"),
				email: !string.IsNullOrEmpty(customer.EMail)
					? customer.EMail
					: "anonymous@anonymous.com",
				countryCode: AppLogic.GetCountryTwoLetterISOCode(order.ShippingAddress.m_Country),
				shipDate: (System.DateTime.Now.AddDays(AppLogic.AppConfigUSInt("GoogleTrustedStoreShippingLeadTime"))).ToString("yyyy-MM-dd"),
				deliveryDate: (System.DateTime.Now.AddDays(AppLogic.AppConfigUSInt("GoogleTrustedStoreDeliveryLeadTime"))).ToString("yyyy-MM-dd"),
				currency: AppLogic.AppConfig("Localization.StoreCurrency"),
				total: Math.Round(order.Total(), 2),
				discounts: Math.Round((order.SubTotal() - order.SubTotal()), 2),
				shippingTotal: Math.Round(order.ShippingTotal(), 2),
				taxTotal: Math.Round(order.TaxTotal(), 2),
				hasDigital: order.HasDownloadComponents(false)
					? "Y"
					: "N",
				cartItems: order
					.CartItems
					.Select(ci => new GoogleTrustedStoresCartItemViewModel(productName: ci.ProductName,
						productSearchId: string.Format("{0}-{1}-{2}-{3}", ci.ProductID, ci.VariantID, AppLogic.CleanSizeColorOption(ci.ChosenSize), AppLogic.CleanSizeColorOption(ci.ChosenColor)),
						productSearchStoreId: productSearchStoreId,
						country: country,
						language: language,
						price: Math.Round(ci.Price, 2),
						quantity: ci.Quantity))
					.ToList());

			return PartialView(ViewNames.GoogleTrustedStoresPartial, model);
		}

		public ActionResult BuySafeGuarantee(int orderNumber)
		{
			if(!AppLogic.GlobalConfigBool("BuySafe.Enabled") || string.IsNullOrEmpty(AppLogic.GlobalConfig("BuySafe.Hash")))
				return Content(string.Empty);


			var customer = HttpContext.GetCustomer();
			var order = new Order(orderNumber, customer.LocaleSetting);

			var model = new BuySafeGuaranteeViewModel(
				orderNumber: orderNumber,
				jsLocation: AppLogic.GlobalConfig("BuySafe.RollOverJSLocation"),
				hash: AppLogic.GlobalConfig("BuySafe.Hash"),
				email: order.EMail,
				total: order.Total());

			return PartialView(ViewNames.BuySafeGuaranteePartial, model);
		}

		void ClearSensitiveOrderData(Customer customer)
		{
			Address billingAddress = new Address();

			//Clear anything that should not be stored except for immediate usage:
			billingAddress.LoadByCustomer(customer.CustomerID, customer.PrimaryBillingAddressID, AddressTypes.Billing);
			billingAddress.PONumber = String.Empty;
			if(!customer.MasterShouldWeStoreCreditCardInfo)
			{
				billingAddress.ClearCCInfo();
			}
			billingAddress.UpdateDB();

			//Clear out the payment method so it isn't automatically set on the next checkout
			customer.UpdateCustomer(requestedPaymentMethod: string.Empty);

			//Clear session data
			PersistedCheckoutContextProvider.ClearCheckoutContext(customer);
			AppLogic.ClearCardExtraCodeInSession(customer);
		}

		void ClearCustomerSession(Customer customer)
		{
			if(AppLogic.AppConfigBool("SiteDisclaimerRequired"))
				HttpContext.Profile.SetPropertyValue("SiteDisclaimerAccepted", string.Empty);

			Session.Clear();
			Session.Abandon();

			Request
				.GetOwinContext()
				.Authentication
				.SignOut(AuthValues.CookiesAuthenticationType);

			customer.Logout();
		}
	}
}
