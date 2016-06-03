// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Checkout.AppliedPaymentMethodCleanup
{
	public class PayPalExpressAppliedPaymentMethodCleanup : IAppliedPaymentMethodCleanup
	{
		readonly IPersistedCheckoutContextProvider PersistedCheckoutContextProvider;

		public PayPalExpressAppliedPaymentMethodCleanup(IPersistedCheckoutContextProvider persistedCheckoutContextProvider)
		{
			PersistedCheckoutContextProvider = persistedCheckoutContextProvider;
		}

		public void Cleanup(AppliedPaymentMethodCleanupContext context)
		{
			// Only finalize the process if this is a PayPal Express order.
			if(context.PaymentMethod != AppLogic.ro_PMPayPalExpress)
				return;

			// Currently, PayPal Express only needs to clean up the checkout context if there's an error.
			//  Successful orders will trigger the checkout process to clear the context.
			if(context.Status == AppLogic.ro_OK)
				return;

			var checkoutContext = PersistedCheckoutContextProvider.LoadCheckoutContext(context.Customer);
			var updatedCheckoutContext = new PersistedCheckoutContext(
				creditCard: checkoutContext.CreditCard,
				payPalExpress: null,
				purchaseOrder: checkoutContext.PurchaseOrder,
				braintree: checkoutContext.Braintree,
				amazonPayments: checkoutContext.AmazonPayments,
				termsAndConditionsAccepted: checkoutContext.TermsAndConditionsAccepted,
				over13Checked: checkoutContext.Over13Checked,
				shippingEstimateDetails: checkoutContext.ShippingEstimateDetails,
				offsiteRequiresBillingAddressId: checkoutContext.OffsiteRequiresBillingAddressId,
				offsiteRequiresShippingAddressId: checkoutContext.OffsiteRequiresShippingAddressId,
				email: checkoutContext.Email,
				selectedShippingMethodId: checkoutContext.SelectedShippingMethodId);

			PersistedCheckoutContextProvider.SaveCheckoutContext(context.Customer, updatedCheckoutContext);

			DB.ExecuteSQL(
				sql: "update Customer set RequestedPaymentMethod = null where CustomerId = @customerId",
				parameters: new System.Data.SqlClient.SqlParameter("@customerId", context.Customer.CustomerID));
		}
	}
}
