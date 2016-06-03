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
	public class InventoryTrimmedPreCheckoutRule : IPreCheckoutRule
	{
		readonly ICachedShoppingCartProvider CachedShoppingCartProvider;
		readonly NoticeProvider NoticeProvider;
		readonly CartActionProvider CartActionProvider;

		public InventoryTrimmedPreCheckoutRule(ICachedShoppingCartProvider cachedShoppingCartProvider, NoticeProvider noticeProvider, CartActionProvider cartActionProvider)
		{
			CachedShoppingCartProvider = cachedShoppingCartProvider;
			NoticeProvider = noticeProvider;
			CartActionProvider = cartActionProvider;
		}

		public CartContext Apply(PreCheckoutRuleContext preCheckoutRuleContext)
		{
			InventoryTrimmedReason inventoryTrimmedReason =
				CartActionProvider
				.ValidateCartQuantitiesAgainstInventory(preCheckoutRuleContext.Customer, preCheckoutRuleContext.CartContext.Cart.CartType);

			//Display inventory adjustment notice if necessary
			var cartTrimmedMessage = preCheckoutRuleContext.CartContext.Cart.GetInventoryTrimmedUserMessage(inventoryTrimmedReason);

			if(!string.IsNullOrEmpty(cartTrimmedMessage))
				NoticeProvider.PushNotice(cartTrimmedMessage, NoticeType.Info);

			return new CartContext(
				cartContext: preCheckoutRuleContext.CartContext,
				cart: CachedShoppingCartProvider.Get(preCheckoutRuleContext.Customer, CartTypeEnum.ShoppingCart, AppLogic.StoreID()));
		}
	}
}
