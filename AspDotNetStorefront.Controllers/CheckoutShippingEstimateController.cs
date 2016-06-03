// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Web.Mvc;
using AspDotNetStorefront.Checkout;
using AspDotNetStorefront.Classes;
using AspDotNetStorefront.Filters;
using AspDotNetStorefront.Models;
using AspDotNetStorefrontCore;
using AspDotNetStorefront.Routing;

namespace AspDotNetStorefront.Controllers
{
	[SecureAccessFilter(forceHttps: true)]
	public class CheckoutShippingEstimateController : Controller
	{
		readonly IPersistedCheckoutContextProvider PersistedCheckoutContextProvider;
		readonly AddressSelectListBuilder AddressSelectListBuilder;

		public CheckoutShippingEstimateController(
			IPersistedCheckoutContextProvider persistedCheckoutContextProvider,
			AddressSelectListBuilder addressSelectListBuilder)
		{
			PersistedCheckoutContextProvider = persistedCheckoutContextProvider;
			AddressSelectListBuilder = addressSelectListBuilder;
		}

		[HttpGet, ImportModelStateFromTempData]
		public ActionResult ShippingEstimate(bool methodsWereReturned = false)
		{
			var customer = HttpContext.GetCustomer();
            // nal
			//var showShippingEstimator = AppLogic.AppConfigBool("ShowShippingEstimate")
			var showShippingEstimator = customer.PrimaryShippingAddressID == 0;

			if(!showShippingEstimator)
				return Content(string.Empty);

			var checkoutContext = PersistedCheckoutContextProvider.LoadCheckoutContext(customer);

			var showNoRates = false;

			// We've entered an address and we did not get any rates back so lets dispay a generic error.
			if(!methodsWereReturned && checkoutContext.ShippingEstimateDetails != null)
				showNoRates = true;

			return PartialView(ViewNames.ShippingEstimatePartial, BuildViewModel(checkoutContext.ShippingEstimateDetails, showNoRates));
		}

		[HttpPost, ExportModelStateToTempData]
		public ActionResult ShippingEstimate(ShippingEstimateViewModel model)
		{
			if(!ModelState.IsValid)
				return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);

			// Add the estimate partial address to the checkout context so that we can use that later to display rates if there is no customer address
			var customer = HttpContext.GetCustomer();
			var checkoutContext = PersistedCheckoutContextProvider.LoadCheckoutContext(customer);
			var shippingEstimateDetails = new ShippingEstimateDetails(
				country: model.Country,
				city: model.City,
				state: model.State,
				postalCode: model.PostalCode);

			var updatedCheckoutContext = new PersistedCheckoutContext(
				creditCard: checkoutContext.CreditCard,
				payPalExpress: checkoutContext.PayPalExpress,
				purchaseOrder: checkoutContext.PurchaseOrder,
				braintree: checkoutContext.Braintree,
				termsAndConditionsAccepted: checkoutContext.TermsAndConditionsAccepted,
				over13Checked: checkoutContext.Over13Checked,
				amazonPayments: checkoutContext.AmazonPayments,
				shippingEstimateDetails: shippingEstimateDetails,
				offsiteRequiresBillingAddressId: checkoutContext.OffsiteRequiresBillingAddressId,
				offsiteRequiresShippingAddressId: checkoutContext.OffsiteRequiresShippingAddressId,
				email: checkoutContext.Email,
				selectedShippingMethodId: checkoutContext.SelectedShippingMethodId);

			PersistedCheckoutContextProvider.SaveCheckoutContext(customer, updatedCheckoutContext);

			return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
		}

		ShippingEstimateViewModel BuildViewModel(ShippingEstimateDetails shippingEstimateDetails, bool showNoRates)
		{
			if(shippingEstimateDetails == null)
				shippingEstimateDetails = new ShippingEstimateDetails(
					country: null,
					city: null,
					state: null,
					postalCode: null);

			return new ShippingEstimateViewModel
			{
				Country = shippingEstimateDetails.Country,
				Countries = AddressSelectListBuilder.BuildCountrySelectList(shippingEstimateDetails.Country),
				City = shippingEstimateDetails.City,
				State = shippingEstimateDetails.State,
				States = AddressSelectListBuilder.BuildStateSelectList(shippingEstimateDetails.Country, shippingEstimateDetails.State),
				PostalCode = shippingEstimateDetails.PostalCode,
				ShowNoRates = showNoRates,
				ShippingCalculationRequiresCityAndState = Shipping.GetActiveShippingCalculationID() != Shipping.ShippingCalculationEnum.UseRealTimeRates
			};
		}
	}
}
