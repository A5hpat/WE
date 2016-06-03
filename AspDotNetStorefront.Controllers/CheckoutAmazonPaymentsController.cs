// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Web.Mvc;
using AspDotNetStorefront.Checkout;
using AspDotNetStorefront.Filters;
using AspDotNetStorefront.Models;
using AspDotNetStorefront.Routing;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways.Processors;

namespace AspDotNetStorefront.Controllers
{
	[SecureAccessFilter(forceHttps: true)]
	public class CheckoutAmazonPaymentsController : Controller
	{
		readonly AmazonPaymentsApiProvider AmazonPaymentsApiProvider;
		readonly NoticeProvider NoticeProvider;
		readonly IPaymentOptionProvider PaymentOptionProvider;
		readonly IPersistedCheckoutContextProvider PersistedCheckoutContextProvider;

		public CheckoutAmazonPaymentsController(
			AmazonPaymentsApiProvider amazonPaymentsApiProvider,
			NoticeProvider noticeProvider, 
			IPaymentOptionProvider paymentOptionProvider,
			IPersistedCheckoutContextProvider persistedCheckoutContextProvider)
		{
			AmazonPaymentsApiProvider = amazonPaymentsApiProvider;
			NoticeProvider = noticeProvider;
			PaymentOptionProvider = paymentOptionProvider;
			PersistedCheckoutContextProvider = persistedCheckoutContextProvider;
		}

		[PageTypeFilter(PageTypes.Checkout)]
		public ActionResult AmazonPayments(bool clearSession = false)
		{
			var customer = HttpContext.GetCustomer();

			if(!PaymentOptionProvider.PaymentMethodSelectionIsValid(AppLogic.ro_PMAmazonPayments, customer))
			{
				NoticeProvider.PushNotice(
					message: "Invalid payment method!  Please choose another.",
					type: NoticeType.Failure);
				return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
			}

			var model = new AmazonPaymentsViewModel(
				clientId: AmazonPaymentsApiProvider.Configuration.ClientId,
				merchantId: AmazonPaymentsApiProvider.Configuration.MerchantId,
				scriptUrl: AmazonPaymentsApiProvider.Configuration.ScriptUrl);

			if(clearSession)
			{
				var checkoutContext = PersistedCheckoutContextProvider.LoadCheckoutContext(customer);
				var updatedCheckoutContext = new PersistedCheckoutContext(
					creditCard: checkoutContext.CreditCard,
					payPalExpress: checkoutContext.PayPalExpress,
					purchaseOrder: checkoutContext.PurchaseOrder,
					braintree: checkoutContext.Braintree,
					amazonPayments: null,
					termsAndConditionsAccepted: checkoutContext.TermsAndConditionsAccepted,
					over13Checked: checkoutContext.Over13Checked,
					shippingEstimateDetails: checkoutContext.ShippingEstimateDetails,
					offsiteRequiresBillingAddressId: null,
					offsiteRequiresShippingAddressId: null,
					email: checkoutContext.Email,
					selectedShippingMethodId: checkoutContext.SelectedShippingMethodId);

				PersistedCheckoutContextProvider.SaveCheckoutContext(customer, updatedCheckoutContext);
				customer.UpdateCustomer(requestedPaymentMethod: string.Empty);
				return Redirect(Url.Action(ActionNames.Index, ControllerNames.Checkout));
			}

			return View(model);
		}

		public ActionResult AmazonPaymentsDetail()
		{
			return PartialView(ViewNames.AmazonPaymentsDetailPartial);
		}

		public ActionResult AmazonPaymentsCallback(string session, string access_token, string token_type, string expires_in, string scope)
		{
			// Get an email back from amazon and update the checkout context with it if we don't already have an email on the checkout context.
			var customer = HttpContext.GetCustomer();
			var checkoutContext = PersistedCheckoutContextProvider.LoadCheckoutContext(customer);
			if(string.IsNullOrEmpty(checkoutContext.Email))
			{
				if(string.IsNullOrEmpty(access_token))
					return View(ViewNames.AmazonPayments, new { clearSession = true });

				var userProfile = AmazonPaymentsApiProvider.GetUserProfile(access_token);
				if(userProfile != null && !string.IsNullOrEmpty(userProfile.Email))
				{
					var updatedCheckoutContext = new PersistedCheckoutContext(
						creditCard: checkoutContext.CreditCard,
						payPalExpress: checkoutContext.PayPalExpress,
						purchaseOrder: checkoutContext.PurchaseOrder,
						braintree: checkoutContext.Braintree,
						amazonPayments: checkoutContext.AmazonPayments,
						termsAndConditionsAccepted: checkoutContext.TermsAndConditionsAccepted,
						over13Checked: checkoutContext.Over13Checked,
						shippingEstimateDetails: checkoutContext.ShippingEstimateDetails,
						offsiteRequiresBillingAddressId: checkoutContext.OffsiteRequiresBillingAddressId,
						offsiteRequiresShippingAddressId: checkoutContext.OffsiteRequiresShippingAddressId,
						email: userProfile.Email,
						selectedShippingMethodId: checkoutContext.SelectedShippingMethodId);

					PersistedCheckoutContextProvider.SaveCheckoutContext(customer, updatedCheckoutContext);
				}
			}

			var model = new AmazonPaymentsViewModel(
				clientId: AmazonPaymentsApiProvider.Configuration.ClientId,
				merchantId: AmazonPaymentsApiProvider.Configuration.MerchantId,
				scriptUrl: AmazonPaymentsApiProvider.Configuration.ScriptUrl);

			model.CheckoutStep = AmazonPaymentsCheckoutStep.SelectAddress;

			return View(ViewNames.AmazonPayments, model);
		}

		public ActionResult AmazonPaymentsComplete(AmazonPaymentsViewModel model)
		{
			var customer = HttpContext.GetCustomer();

			var orderDetails = AmazonPaymentsApiProvider
				.GetOrderDetails(model.AmazonOrderReferenceId)
				.GetOrderReferenceDetailsResult
				.OrderReferenceDetails;

			var shippingAddress = orderDetails
				.Destination
				.PhysicalDestination;

			var city = shippingAddress.City;
			var countryCode = shippingAddress.CountryCode;
			var countryName = AppLogic.GetCountryNameFromTwoLetterISOCode(countryCode);
			var stateName = shippingAddress.StateOrRegion ?? string.Empty;
			var stateAbbreviation = AppLogic.GetStateAbbreviation(stateName, countryName);
			var postalCode = shippingAddress.PostalCode;

			var amazonAddress = Address.FindOrCreateOffSiteAddress(
				customerId: customer.CustomerID,
				city: city,
				stateAbbreviation: string.IsNullOrEmpty(stateAbbreviation)
					? stateName
					: stateAbbreviation,
				postalCode: postalCode,
				countryName: string.IsNullOrEmpty(countryName)
					? countryCode
					: countryName,
				offSiteSource: AppLogic.ro_PMAmazonPayments);

			customer.SetPrimaryAddress(amazonAddress.AddressID, AddressTypes.Billing);
			customer.SetPrimaryAddress(amazonAddress.AddressID, AddressTypes.Shipping);

			var checkoutContext = PersistedCheckoutContextProvider.LoadCheckoutContext(customer);
			var updatedCheckoutContext = new PersistedCheckoutContext(
				creditCard: checkoutContext.CreditCard,
				payPalExpress: checkoutContext.PayPalExpress,
				purchaseOrder: checkoutContext.PurchaseOrder,
				braintree: checkoutContext.Braintree,
				amazonPayments: new AmazonPaymentsDetails(model.AmazonOrderReferenceId),
				termsAndConditionsAccepted: checkoutContext.TermsAndConditionsAccepted,
				over13Checked: checkoutContext.Over13Checked,
				shippingEstimateDetails: checkoutContext.ShippingEstimateDetails,
				offsiteRequiresBillingAddressId: amazonAddress.AddressID,
				offsiteRequiresShippingAddressId: amazonAddress.AddressID,
				email: checkoutContext.Email,
				selectedShippingMethodId: checkoutContext.SelectedShippingMethodId);

			PersistedCheckoutContextProvider.SaveCheckoutContext(customer, updatedCheckoutContext);
			customer.UpdateCustomer(requestedPaymentMethod: AppLogic.ro_PMAmazonPayments);

			return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
		}
	}
}
