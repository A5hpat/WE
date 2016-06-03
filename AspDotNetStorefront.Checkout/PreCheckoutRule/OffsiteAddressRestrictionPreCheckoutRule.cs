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
	public class OffsiteAddressRestrictionPreCheckoutRule : IPreCheckoutRule
	{
		readonly NoticeProvider NoticeProvider;
		readonly ICachedShoppingCartProvider CachedShoppingCartProvider;

		public OffsiteAddressRestrictionPreCheckoutRule(NoticeProvider noticeProvider, ICachedShoppingCartProvider cachedShoppingCartProvider)
		{
			NoticeProvider = noticeProvider;
			CachedShoppingCartProvider = cachedShoppingCartProvider;
		}

		public CartContext Apply(PreCheckoutRuleContext preCheckoutRuleContext)
		{
			var persistedCheckout = preCheckoutRuleContext
				.PersistedCheckoutContext;

			var customer = preCheckoutRuleContext
				.Customer;

			// If an offsite payment method has flagged billing or shipping as required, any changes by the customer will be reverted.
			if(persistedCheckout.OffsiteRequiresBillingAddressId.HasValue
				&& customer.PrimaryBillingAddressID != persistedCheckout.OffsiteRequiresBillingAddressId)
			{
				customer.UpdateCustomer(
					billingAddressId: persistedCheckout.OffsiteRequiresBillingAddressId.Value);
				NoticeProvider.PushNotice("Your billing address has been reverted back to the required address for your payment selection.", NoticeType.Warning);
			}

			if(persistedCheckout.OffsiteRequiresShippingAddressId.HasValue
				&& customer.PrimaryShippingAddressID != persistedCheckout.OffsiteRequiresShippingAddressId)
			{
				customer.UpdateCustomer(
					shippingAddressId: persistedCheckout.OffsiteRequiresShippingAddressId.Value);
				NoticeProvider.PushNotice("Your shipping address has been reverted back to the required address for your payment selection.", NoticeType.Warning);
			}

			return new CartContext(
				cartContext: preCheckoutRuleContext.CartContext,
				cart: CachedShoppingCartProvider.Get(customer, CartTypeEnum.ShoppingCart, AppLogic.StoreID()));
		}
	}
}
