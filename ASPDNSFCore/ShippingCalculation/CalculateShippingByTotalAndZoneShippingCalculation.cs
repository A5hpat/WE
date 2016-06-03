// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;

namespace AspDotNetStorefrontCore.ShippingCalculation
{
	public class CalculateShippingByTotalAndZoneShippingCalculation : ShippingCalculation
	{
		protected override bool UsesZones
		{ get { return true; } }

		protected override decimal CalculateFreight(ShippingCalculationContext context, int shippingMethodId, IDataReader reader)
		{
			var zoneID = Shipping.ZoneLookup(context.ShippingAddress.Zip);
			return Shipping.GetShipByTotalAndZoneCharge(
				shippingMethodId,
				context.CartSubtotal(
						true, // includeDiscount
						false, // onlyIncludeTaxableItems
						false, // includeDownloadItems
						false, // includeFreeShippingItems
						true, // includeSystemItems
						false, // useCustomerCurrencySetting
						0, // forShippingAddressId
						true, // excludeTax
						false) // includeShippingNotRequiredItems
				,
				zoneID);
		}
	}
}
