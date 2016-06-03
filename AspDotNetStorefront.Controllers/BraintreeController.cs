// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Web.Mvc;
using AspDotNetStorefront.Checkout;
using AspDotNetStorefront.Filters;
using AspDotNetStorefrontCore;
using AspDotNetStorefront.Routing;
using AspDotNetStorefrontGateways;

namespace AspDotNetStorefront.Controllers
{
	public class BraintreeController : Controller
	{
		readonly NoticeProvider NoticeProvider;
		readonly IPaymentOptionProvider PaymentOptionProvider;
		readonly IPersistedCheckoutContextProvider PersistedCheckoutContextProvider;

		public BraintreeController(
			NoticeProvider noticeProvider,
			IPaymentOptionProvider paymentOptionProvider,
			IPersistedCheckoutContextProvider persistedCheckoutContextProvider)
		{
			NoticeProvider = noticeProvider;
			PaymentOptionProvider = paymentOptionProvider;
			PersistedCheckoutContextProvider = persistedCheckoutContextProvider;
		}

		[HttpPost]
		[ExportModelStateToTempData]
		public ActionResult BraintreeCreditCard(FormCollection collection)
		{
			var customer = HttpContext.GetCustomer();
			var checkoutContext = PersistedCheckoutContextProvider.LoadCheckoutContext(customer);

			var updatedCheckoutContext = new PersistedCheckoutContext(
				creditCard: new CreditCardDetails(
					name: customer.Name,
					number: null,
					issueNumber: null,
					cardType: collection["braintreeCardType"],
					expirationDate: null,
					startDate: null,
					cvv: null),
				payPalExpress: checkoutContext.PayPalExpress,
				purchaseOrder: checkoutContext.PurchaseOrder,
				braintree: new BraintreeDetails(
					nonce: collection["braintreeNonce"], 
					token: collection["braintreeToken"], 
					paymentMethod: Gateway.BraintreeCreditCardKey,	//This is the Braintree payment method, not ours	
                    threeDSecureApproved: false),
				amazonPayments: checkoutContext.AmazonPayments,
				termsAndConditionsAccepted: checkoutContext.TermsAndConditionsAccepted,
				over13Checked: checkoutContext.Over13Checked,
				shippingEstimateDetails: checkoutContext.ShippingEstimateDetails,
				offsiteRequiresBillingAddressId: null,
				offsiteRequiresShippingAddressId: null,
				email: checkoutContext.Email,
				selectedShippingMethodId: checkoutContext.SelectedShippingMethodId);

			PersistedCheckoutContextProvider.SaveCheckoutContext(customer, updatedCheckoutContext);

			customer.UpdateCustomer(requestedPaymentMethod: AppLogic.ro_PMCreditCard);

			return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
		}
	}
}
