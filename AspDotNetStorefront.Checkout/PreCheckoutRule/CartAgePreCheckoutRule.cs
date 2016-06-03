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
	public class CartAgePreCheckoutRule : IPreCheckoutRule
	{
		readonly ICachedShoppingCartProvider CachedShoppingCartProvider;
		readonly CartActionProvider CartActionProvider;

		public CartAgePreCheckoutRule(ICachedShoppingCartProvider cachedShoppingCartProvider, CartActionProvider cartActionProvider)
		{
			CachedShoppingCartProvider = cachedShoppingCartProvider;
			CartActionProvider = cartActionProvider;
		}

		public CartContext Apply(PreCheckoutRuleContext preCheckoutRuleContext)
		{
			// Age the cart
			var ageCartDays = preCheckoutRuleContext.CheckoutConfiguration.AgeCartDays == 0
				? 7
				: preCheckoutRuleContext.CheckoutConfiguration.AgeCartDays;

			CartActionProvider.RemoveItemsBeyondMaxAge(preCheckoutRuleContext.Customer, preCheckoutRuleContext.CartContext.Cart.CartType, ageCartDays);
			CartActionProvider.ClearDeletedAndUnPublishedProducts(preCheckoutRuleContext.Customer);

			return new CartContext(
				cartContext: preCheckoutRuleContext.CartContext,
				cart: CachedShoppingCartProvider.Get(preCheckoutRuleContext.Customer, CartTypeEnum.ShoppingCart, AppLogic.StoreID()));
		}
	}
}
