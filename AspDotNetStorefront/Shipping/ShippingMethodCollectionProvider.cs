// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontCore.ShippingCalculation;

namespace AspDotNetStorefront.Shipping
{
	/// <summary>
	/// This class returns the currently available shipping methods for the provided customer's cart.
	/// </summary>
	public class ShippingMethodCollectionProvider : IShippingMethodCollectionProvider
	{
		public ShippingMethodCollection GetShippingMethods(Customer customer, Address address, CartItemCollection cartItems, int storeId)
		{
			if(!cartItems.Any())
				return new ShippingMethodCollection();

			if(cartItems.IsAllDownloadComponents())
				return new ShippingMethodCollection();

			if(cartItems.IsAllSystemComponents())
				return new ShippingMethodCollection();

			if(cartItems.NoShippingRequiredComponents())
				return new ShippingMethodCollection();

			if(cartItems.IsAllEmailGiftCards())
				return new ShippingMethodCollection();

			if(!AppLogic.AppConfigBool("FreeShippingAllowsRateSelection") && cartItems.IsAllFreeShippingComponents())
				return new ShippingMethodCollection();

			var shippingCalculation = AspDotNetStorefrontCore.Shipping.GetActiveShippingCalculation();
			if(shippingCalculation == null)
				return new ShippingMethodCollection();

			var returnInStorePickupRate = (shippingCalculation.GetType() == typeof(UseRealTimeRatesShippingCalculation))    //In-Store Pickup is only allowed with realtime rates
				&& AppLogic.AppConfigBool("RTShipping.AllowLocalPickup");   

			var appliedStoreId = AppLogic.GlobalConfigBool("AllowShippingFiltering")
				? storeId
				: AspDotNetStorefrontCore.Shipping.DONT_FILTER_PER_STORE;

			var cart = cartItems.First().ThisShoppingCart;
			var shippingCalculationContext = new ShippingCalculationContext(
				customer: customer,
				cartItems: cartItems,
				shippingAddress: address,
				taxRate: cart.VatIsInclusive
						? customer.TaxRate(AppLogic.AppConfigUSInt("ShippingTaxClassID")) / 100m
						: 0,
				excludeZeroFreightCosts: AppLogic.AppConfigBool("FilterOutShippingMethodsThatHave0Cost"),
				shippingIsFreeIfIncludedInFreeList: cart.ShippingIsFree,
				handlingExtraFee: AppLogic.AppConfigUSDecimal("ShippingHandlingExtraFee"),
				storeId: appliedStoreId,
				weight: cart.WeightTotal(),
				cartSubtotal: cart.SubTotal);

			var availableShippingMethods = shippingCalculation.GetShippingMethods(shippingCalculationContext);

			if(returnInStorePickupRate && LocalPickupAvailable(address))
				availableShippingMethods.Add(new ShippingMethod
				{
					Name = "In-Store Pickup",
					Freight = AppLogic.AppConfigNativeDecimal("RTShipping.LocalPickupCost"),
					ShippingIsFree = AppLogic.AppConfigNativeDecimal("RTShipping.LocalPickupCost") == 0,
				});

			if(!availableShippingMethods.Any() && appliedStoreId != AspDotNetStorefrontCore.Shipping.DONT_FILTER_PER_STORE)
			{
				// no shipping methods found, let's try and fallback to the default store
				// (if we're not already in it)
				// when we're on a multi-store scenario

				var stores = Store.GetStoreList();
				var defaultStore = Store.GetDefaultStore();
				if(stores.Count > 1 && defaultStore != null && defaultStore.StoreID != appliedStoreId)
					availableShippingMethods = shippingCalculation.GetShippingMethods(new ShippingCalculationContext(
						source: shippingCalculationContext,
						storeId: defaultStore.StoreID));

				// log if we still didn't find any shipping methods
				if(!availableShippingMethods.Any())
				{
					var message = string.Format("No Shipping method found for StoreId: {0}", appliedStoreId);
					var logDetails = customer.IsRegistered
						? string.Format(@"
							StoreId: {0}
							CalculationMode: {1}
							CustomerId: {2}
							Address: 
								{3}", appliedStoreId, AspDotNetStorefrontCore.Shipping.GetActiveShippingCalculationID(), customer.CustomerID, address.DisplayHTML(true))
						: string.Empty;

					SysLog.LogMessage(message, logDetails, MessageTypeEnum.Informational, MessageSeverityEnum.Alert);
				}
			}

			//Filter out non-free shipping methods if necessary
			if(cart.ShippingIsFree
				&& !AppLogic.AppConfigBool("FreeShippingAllowsRateSelection"))
            {
				var freeShippingMethodIds = AppLogic.AppConfig("ShippingMethodIDIfFreeShippingIsOn").ParseAsDelimitedList<int>();

				if(freeShippingMethodIds.Any())
					availableShippingMethods.RemoveAll(sm => !freeShippingMethodIds.Contains(sm.Id));
            }

			//Apply shipping promotion discounts if any
			var shippingDiscounts = cart.DiscountResults.SelectMany(p => p.Promotion.PromotionDiscounts.OfType<Promotions.ShippingPromotionDiscount>());
			if(shippingDiscounts.Any())
				foreach(var shippingMethod in availableShippingMethods)
					ApplyShippingPromotion(shippingMethod, shippingDiscounts);

			return availableShippingMethods;
		}

		void ApplyShippingPromotion(ShippingMethod shippingMethod, IEnumerable<Promotions.ShippingPromotionDiscount> shippingDiscounts)
		{
			var discountIncludedText = "(including promo discounts)";
			//Have to assign some kind of order to make sure this is calculated the same every time
			shippingDiscounts = shippingDiscounts.OrderBy(d => d.DiscountType);

			foreach(var discount in shippingDiscounts)
			{
				if(!discount.ShippingMethodIds.Contains(shippingMethod.Id))
					continue;
				
				if(discount.DiscountType == Promotions.DiscountType.Fixed)
					shippingMethod.Freight = shippingMethod.Freight - discount.DiscountAmount;
				else if(discount.DiscountType == Promotions.DiscountType.Percentage)
					shippingMethod.Freight = shippingMethod.Freight - (shippingMethod.Freight * discount.DiscountAmount);

				if(!string.IsNullOrEmpty(shippingMethod.DisplayName))
					shippingMethod.DisplayName += discountIncludedText;
				else
					shippingMethod.Name += discountIncludedText;

				//Safety check
				if(shippingMethod.Freight < 0)
					shippingMethod.Freight = 0;
			}
		}

		bool LocalPickupAvailable(Address address)
		{
			if(!AppLogic.AppConfigBool("RTShipping.AllowLocalPickup"))
				return false;

			var restrictiontype = AppLogic.AppConfig("RTShipping.LocalPickupRestrictionType").Trim();
			if(StringComparer.OrdinalIgnoreCase.Equals(restrictiontype, "zone"))
			{
				var zoneForZip = AspDotNetStorefrontCore.Shipping.ZoneLookup(address.Zip);
				return AppLogic.AppConfig("RTShipping.LocalPickupRestrictionZones")
					.ParseAsDelimitedList<int>()
					.Contains(zoneForZip);
			}

			if(StringComparer.OrdinalIgnoreCase.Equals(restrictiontype, "zip"))
			{
				return AppLogic.AppConfig("RTShipping.LocalPickupRestrictionZips")
					.ParseAsDelimitedList()
					.Contains(address.Zip, StringComparer.InvariantCultureIgnoreCase);
			}

			if(StringComparer.OrdinalIgnoreCase.Equals(restrictiontype, "state"))
			{
				var allowedStates = AppLogic.AppConfig("RTShipping.LocalpickupRestrictionStates")
					.Trim();

				var matchingStateCount = DB.GetSqlN(
					@"select count(*) as N from State with(nolock) where Abbreviation in (select Items from dbo.Split(@allowedStates, ',')) and StateID = @addressStateId",
					new SqlParameter("allowedStates", allowedStates),
					new SqlParameter("addressStateId", address.StateID));

				return matchingStateCount > 0;
			}

			return true;
		}
	}
}
