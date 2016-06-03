// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Linq;
using AspDotNetStorefront.Caching.ObjectCaching;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Checkout.PreCheckoutRule
{
	public class ShippingSelectionPreCheckoutRule : IPreCheckoutRule
	{
		readonly ICachedShippingMethodCollectionProvider CachedShippingMethodCollectionProvider;
		readonly ICachedShoppingCartProvider CachedShoppingCartProvider;
		readonly IShippingMethodCartItemApplicator ShippingMethodCartItemApplicator;
		readonly IEffectiveShippingAddressProvider EffectiveShippingAddressProvider;
		readonly bool FreeShippingAllowsRateSelection;

		public ShippingSelectionPreCheckoutRule(ICachedShippingMethodCollectionProvider cachedShippingMethodCollectionProvider, ICachedShoppingCartProvider cachedShoppingCartProvider, IShippingMethodCartItemApplicator shippingMethodCartItemApplicator, IEffectiveShippingAddressProvider effectiveShippingAddressProvider)
		{
			CachedShippingMethodCollectionProvider = cachedShippingMethodCollectionProvider;
			CachedShoppingCartProvider = cachedShoppingCartProvider;
			ShippingMethodCartItemApplicator = shippingMethodCartItemApplicator;
			FreeShippingAllowsRateSelection = AppLogic.AppConfigBool("FreeShippingAllowsRateSelection");
			EffectiveShippingAddressProvider = effectiveShippingAddressProvider;
		}

		public CartContext Apply(PreCheckoutRuleContext context)
		{
			var shippingAddress = EffectiveShippingAddressProvider.GetEffectiveShippingAddress(context.Customer);

			var availableShippingMethods = CachedShippingMethodCollectionProvider
				.Get(context.Customer, shippingAddress, context.CartContext.Cart.CartItems, AppLogic.StoreID());

			// If there are no available shipping methods, the customer hasn't selected one, or the customer selected an invalid one, selectedShipping will be null.
			var selectedShippingMethod = availableShippingMethods
				.Where(shippingMethod => shippingMethod.Id == context.PersistedCheckoutContext.SelectedShippingMethodId)
				.FirstOrDefault();

			// No matter what shipping method was selected but the customer, if there is only one available shipping method, set the customers selection to it.
			if(availableShippingMethods.Count() == 1)
				selectedShippingMethod = availableShippingMethods.First();

			// Update all cart items to the updated selection. If the selection is null, then it clears the selection from the cart items and the customer has to select a new one.
			ShippingMethodCartItemApplicator.UpdateCartItemsShippingMethod(
				context.Customer,
				context.CartContext.Cart,
				selectedShippingMethod);

			return new CartContext(
				cartContext: context.CartContext,
				cart: CachedShoppingCartProvider.Get(context.Customer, CartTypeEnum.ShoppingCart, AppLogic.StoreID()));
		}
	}
}
