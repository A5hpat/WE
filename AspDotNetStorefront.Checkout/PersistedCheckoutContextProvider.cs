// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using AspDotNetStorefrontCore;
using Newtonsoft.Json;

namespace AspDotNetStorefront.Checkout
{
	public class PersistedCheckoutContextProvider : IPersistedCheckoutContextProvider
	{
		const string CheckoutContextSessionKey = "CheckoutContext";

		public PersistedCheckoutContext LoadCheckoutContext(Customer customer)
		{
			var encryptedCheckoutContext = customer.ThisCustomerSession.Session(CheckoutContextSessionKey);
			if(string.IsNullOrEmpty(encryptedCheckoutContext))
				return new PersistedCheckoutContext(creditCard: null,
					payPalExpress: null,
					purchaseOrder: null,
					braintree: null,
					amazonPayments: null,
					termsAndConditionsAccepted: false,
					over13Checked: false,
					shippingEstimateDetails: null,
					offsiteRequiresBillingAddressId: null,
					offsiteRequiresShippingAddressId: null,
					email: null,
					selectedShippingMethodId: null);

			var serializedCheckoutContext = Security.UnmungeString(encryptedCheckoutContext);
			if(string.IsNullOrEmpty(serializedCheckoutContext))
				return new PersistedCheckoutContext(creditCard: null,
					payPalExpress: null,
					purchaseOrder: null,
					braintree: null,
					amazonPayments: null,
					termsAndConditionsAccepted: false,
					over13Checked: false,
					shippingEstimateDetails: null,
					offsiteRequiresBillingAddressId: null,
					offsiteRequiresShippingAddressId: null,
					email: null,
					selectedShippingMethodId: null);

			var checkoutContext = JsonConvert.DeserializeObject<PersistedCheckoutContext>(serializedCheckoutContext);
			return checkoutContext;
		}

		public void SaveCheckoutContext(Customer customer, PersistedCheckoutContext checkoutContext)
		{
			var serializedCheckoutContext = JsonConvert.SerializeObject(checkoutContext
				?? new PersistedCheckoutContext(creditCard: null,
					payPalExpress: null,
					purchaseOrder: null,
					braintree: null,
					amazonPayments: null,
					termsAndConditionsAccepted: false,
					over13Checked: false,
					shippingEstimateDetails: null,
					offsiteRequiresBillingAddressId: null,
					offsiteRequiresShippingAddressId: null,
					email: null,
					selectedShippingMethodId: null));

			var encryptedCheckoutContext = Security.MungeString(serializedCheckoutContext);

			customer.ThisCustomerSession.SetVal(CheckoutContextSessionKey, encryptedCheckoutContext);
		}

		public void ClearCheckoutContext(Customer customer)
		{
			customer.ThisCustomerSession.ClearVal(CheckoutContextSessionKey);
		}
	}
}
