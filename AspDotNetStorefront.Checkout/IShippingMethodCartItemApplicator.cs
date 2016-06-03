// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using AspDotNetStorefrontCore;
using AspDotNetStorefrontCore.ShippingCalculation;

namespace AspDotNetStorefront.Checkout
{
	public interface IShippingMethodCartItemApplicator
	{
		void UpdateCartItemsShippingMethod(Customer customer, ShoppingCart cart, ShippingMethod shippingMethod);
	}
}
