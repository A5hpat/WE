// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Checkout
{
	public interface ICartContextProvider
	{
		CartContext LoadCartContext(CheckoutConfiguration configuration, Customer customer, PersistedCheckoutContext persistedCheckoutContext, PaymentMethodInfo selectedPaymentMethod);
	}

	public class CartContext
	{
		public readonly ShoppingCart Cart;

		public CartContext(ShoppingCart cart)
		{
			Cart = cart;
		}

		public CartContext(CartContext cartContext, ShoppingCart cart = null)
		{
			Cart = cart ?? cartContext.Cart;
		}
	}
}
