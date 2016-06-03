// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using AspDotNetStorefrontCore;
using AspDotNetStorefrontCore.ShippingCalculation;

namespace AspDotNetStorefront.Caching.ObjectCaching
{
	public interface ICachedShippingMethodCollectionProvider
	{
		ShippingMethodCollection Get(Customer customer, Address address, CartItemCollection cartItems, int storeId);
	}
}
