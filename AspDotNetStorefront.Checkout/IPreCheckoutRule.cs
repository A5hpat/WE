// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Checkout
{
	// Pre-Checkout Rules modify the customer's checkout state before handing it to the Checkout 
	// Engine. They normalize state and ensure rules are enforced before the Checkout Engine runs,
	// ensuring that the checkout engine doesn't need to modify external state to validate 
	// the customer's checkout.

	public interface IPreCheckoutRule
	{
		CartContext Apply(PreCheckoutRuleContext preCheckoutRuleContext);
	}

	public class PreCheckoutRuleContext
	{
		public readonly CheckoutConfiguration CheckoutConfiguration;
		public readonly Customer Customer;
		public readonly PersistedCheckoutContext PersistedCheckoutContext;
		public readonly CartContext CartContext;
		public readonly PaymentMethodInfo PaymentMethodInfo;

		public PreCheckoutRuleContext(CheckoutConfiguration checkoutConfiguration, Customer customer, PersistedCheckoutContext persistedCheckoutContext, CartContext cartContext, PaymentMethodInfo paymentMethodInfo)
		{
			CheckoutConfiguration = checkoutConfiguration;
			Customer = customer;
			PersistedCheckoutContext = persistedCheckoutContext;
			CartContext = cartContext;
			PaymentMethodInfo = paymentMethodInfo;
		}
	}
}
