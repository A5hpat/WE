// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using AspDotNetStorefront.Caching.ObjectCaching;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Checkout.PreCheckoutRule
{
	public class PaymentMethodPreCheckoutRule : IPreCheckoutRule
	{
		readonly ICachedShoppingCartProvider CachedShoppingCartProvider;

		public PaymentMethodPreCheckoutRule(ICachedShoppingCartProvider cachedShoppingCartProvider)
		{
			CachedShoppingCartProvider = cachedShoppingCartProvider;
		}

		public CartContext Apply(PreCheckoutRuleContext preCheckoutRuleContext)
		{
			var customer = preCheckoutRuleContext
				.Customer;

			// Ensure the payment method on the billing address matches the selected payment method
			if(customer.PrimaryBillingAddressID != 0
				&& customer.PrimaryBillingAddress.PaymentMethodLastUsed != customer.RequestedPaymentMethod)
			{
				customer.PrimaryBillingAddress.PaymentMethodLastUsed = customer.RequestedPaymentMethod;
				customer.PrimaryBillingAddress.UpdateDB();
			}

			return new CartContext(
				cartContext: preCheckoutRuleContext.CartContext,
				cart: CachedShoppingCartProvider.Get(customer, CartTypeEnum.ShoppingCart, AppLogic.StoreID()));
		}
	}
}
