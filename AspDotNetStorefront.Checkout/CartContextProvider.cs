// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using AspDotNetStorefront.Caching.ObjectCaching;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Checkout
{
	public class CartContextProvider : ICartContextProvider
	{
		readonly IEnumerable<IPreCheckoutRule> PreCheckoutRules;
		readonly ICachedShoppingCartProvider CachedShoppingCartProvider;

		public CartContextProvider(IEnumerable<IPreCheckoutRule> checkoutRules, ICachedShoppingCartProvider cachedShoppingCartProvider)
		{
			PreCheckoutRules = checkoutRules;
			CachedShoppingCartProvider = cachedShoppingCartProvider;
		}

		public CartContext LoadCartContext(CheckoutConfiguration configuration, Customer customer, PersistedCheckoutContext persistedCheckoutContext, PaymentMethodInfo selectedPaymentMethod)
		{
			var cartContext = new CartContext(
				cart: CachedShoppingCartProvider.Get(customer, CartTypeEnum.ShoppingCart, AppLogic.StoreID()));

			var preRuleCheckoutContext = new PreCheckoutRuleContext(
				checkoutConfiguration: configuration,
				customer: customer,
				persistedCheckoutContext: persistedCheckoutContext,
				cartContext: cartContext,
				paymentMethodInfo: selectedPaymentMethod);

			foreach(var rule in PreCheckoutRules)
				cartContext = rule.Apply(preRuleCheckoutContext);

			return cartContext;
		}
	}
}
