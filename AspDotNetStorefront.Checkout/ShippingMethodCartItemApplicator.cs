// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data.SqlClient;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontCore.ShippingCalculation;

namespace AspDotNetStorefront.Checkout
{
	public class ShippingMethodCartItemApplicator : IShippingMethodCartItemApplicator
	{
		public void UpdateCartItemsShippingMethod(Customer customer, ShoppingCart cart, ShippingMethod shippingMethod)
		{
			int? databaseShippingMethodId;
			string databaseShippingMethodName;
			if(shippingMethod == null)
			{
				databaseShippingMethodId = null;
				databaseShippingMethodName = null;
			}
			else if(cart.ShippingIsFree && !AppLogic.AppConfigBool("FreeShippingAllowsRateSelection"))
			{
				databaseShippingMethodId = shippingMethod.Id;
				databaseShippingMethodName = string.Format(
					"{0} : {1}",
                    "FREE",
					cart.GetFreeShippingReason());
			}
			else
			{
				databaseShippingMethodId = shippingMethod.Id;
				databaseShippingMethodName = Shipping.GetActiveShippingCalculationID() != Shipping.ShippingCalculationEnum.UseRealTimeRates
					? Shipping.GetShippingMethodDisplayName(shippingMethod.Id, null)
					: Shipping.GetFormattedRealTimeShippingMethodForDatabase(shippingMethod.Name, shippingMethod.Freight, shippingMethod.VatRate);
			}

			DB.ExecuteSQL(@"
				update 
					dbo.ShoppingCart 
				set 
					ShippingMethodID = @shippingMethodId, 
					ShippingMethod = @shippingMethod 
				where 
					CustomerID = @customerID
					and CartType = @cartType
					and StoreID = @storeId 
					and (
						ISNULL(ShippingMethodId, 0) != @shippingMethodId 
						or ISNULL(ShippingMethod, '') != @shippingMethod
					) and (
						FreeShipping = 0
						or @freeShippingAllowsRateSelection = 1
					)",

				new SqlParameter("storeId", AppLogic.StoreID()),
				new SqlParameter("shippingMethodId", (object)databaseShippingMethodId ?? DBNull.Value),
				new SqlParameter("shippingMethod", (object)databaseShippingMethodName ?? DBNull.Value),
				new SqlParameter("customerID", customer.CustomerID),
				new SqlParameter("cartType", CartTypeEnum.ShoppingCart),
				new SqlParameter("freeShippingAllowsRateSelection", AppLogic.AppConfigBool("FreeShippingAllowsRateSelection")));
		}
	}
}
