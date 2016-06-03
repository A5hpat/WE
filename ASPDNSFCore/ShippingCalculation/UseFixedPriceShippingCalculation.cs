// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Data;

namespace AspDotNetStorefrontCore.ShippingCalculation
{
	public class UseFixedPriceShippingCalculation : ShippingCalculation
	{
		protected override bool UsesZones
		{ get { return false; } }

		protected override decimal CalculateFreight(ShippingCalculationContext context, int shippingMethodId, IDataReader reader)
		{
			return DB.RSFieldDecimal(reader, "FixedRate");
		}
	}
}
