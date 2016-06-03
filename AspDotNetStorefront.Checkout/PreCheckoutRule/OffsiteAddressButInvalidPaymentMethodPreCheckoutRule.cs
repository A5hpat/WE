// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Linq;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Checkout.PreCheckoutRule
{
	public class OffsiteAddressButInvalidPaymentMethodPreCheckoutRule : IPreCheckoutRule
	{
		public CartContext Apply(PreCheckoutRuleContext preCheckoutRuleContext)
		{
			var customer = preCheckoutRuleContext.Customer;

			var persistedCheckout = preCheckoutRuleContext
				.PersistedCheckoutContext;

			if(!AppLogic.ro_OffsitePayMentMethods.Contains(customer.PrimaryBillingAddress.OffsiteSource))
				return preCheckoutRuleContext.CartContext;

			if(preCheckoutRuleContext.PaymentMethodInfo == null
				|| AppLogic.ro_OffsitePayMentMethods.Contains(preCheckoutRuleContext.PaymentMethodInfo.Name))
			{
				customer.PrimaryBillingAddressID = persistedCheckout.OffsiteRequiresBillingAddressId ?? 0;
				customer.PrimaryShippingAddressID = persistedCheckout.OffsiteRequiresShippingAddressId ?? 0;
			}
			else if(!AppLogic.ro_OffsitePayMentMethods.Contains(preCheckoutRuleContext.PaymentMethodInfo.Name))
			{
				if(AppLogic.ro_OffsitePayMentMethods.Contains(customer.PrimaryBillingAddress.OffsiteSource))
					customer.PrimaryBillingAddressID = 0;

				if(AppLogic.ro_OffsitePayMentMethods.Contains(customer.PrimaryShippingAddress.OffsiteSource))
					customer.PrimaryShippingAddressID = 0;
			}

			return new CartContext(
				cartContext: preCheckoutRuleContext.CartContext,
				cart: new ShoppingCart(customer.SkinID, customer, CartTypeEnum.ShoppingCart, 0, false));
		}
	}
}
