// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
namespace AspDotNetStorefrontCore.ShippingCalculation
{
	public class AllOrdersHaveFreeShippingShippingCalculation : IShippingCalculation
	{
		public ShippingMethodCollection GetShippingMethods(ShippingCalculationContext context)
		{
			return new ShippingMethodCollection(new[]
			{
				new ShippingMethod
				{
					Name = string.Format("All Orders Have Free {0} Shipping", string.Empty),
				}
			});
		}
	}
}
