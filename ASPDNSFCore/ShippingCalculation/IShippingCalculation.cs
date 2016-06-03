// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
namespace AspDotNetStorefrontCore.ShippingCalculation
{
	public interface IShippingCalculation
	{
		ShippingMethodCollection GetShippingMethods(ShippingCalculationContext context);
	}

	public delegate decimal CartSubtotalDelegate(bool includeDiscount, bool onlyIncludeTaxableItems, bool includeDownloadItems, bool includeFreeShippingItems, bool includeSystemItems, bool useCustomerCurrencySetting, int forShippingAddressId, bool excludeTax, bool includeShippingNotRequiredItems);

	public class ShippingCalculationContext
	{
		public readonly Customer Customer;
		public readonly Address ShippingAddress;
		public readonly CartItemCollection CartItems;
		public readonly decimal TaxRate;
		public readonly bool ShippingIsFreeIfIncludedInFreeList;
		public readonly decimal HandlingExtraFee;
		public readonly bool ExcludeZeroFreightCosts;
		public readonly int StoreId;
		public readonly decimal Weight;
		public readonly CartSubtotalDelegate CartSubtotal;

		public ShippingCalculationContext(
			Customer customer,
			Address shippingAddress,
			CartItemCollection cartItems,
			decimal taxRate,
			bool shippingIsFreeIfIncludedInFreeList,
			decimal handlingExtraFee,
			bool excludeZeroFreightCosts,
			int storeId,
			decimal weight,
			CartSubtotalDelegate cartSubtotal)
		{
			Customer = customer;
			ShippingAddress = shippingAddress;
			CartItems = cartItems;
			TaxRate = taxRate;
			ShippingIsFreeIfIncludedInFreeList = shippingIsFreeIfIncludedInFreeList;
			HandlingExtraFee = handlingExtraFee;
			ExcludeZeroFreightCosts = excludeZeroFreightCosts;
			StoreId = storeId;
			Weight = weight;
			CartSubtotal = cartSubtotal;
		}

		public ShippingCalculationContext(
			ShippingCalculationContext source,
			Customer customer = null,
			Address shippingAddress = null,
			CartItemCollection cartItems = null,
			decimal? taxRate = null,
			bool? shippingIsFreeIfIncludedInFreeList = null,
			decimal? handlingExtraFee = null,
			bool? excludeZeroFreightCosts = null,
			int? storeId = null,
			decimal? weight = null,
			CartSubtotalDelegate cartSubtotal = null)
		{
			Customer = customer ?? source.Customer;
			ShippingAddress = shippingAddress ?? source.ShippingAddress;
			CartItems = cartItems ?? source.CartItems;
			TaxRate = taxRate ?? source.TaxRate;
			ShippingIsFreeIfIncludedInFreeList = shippingIsFreeIfIncludedInFreeList ?? source.ShippingIsFreeIfIncludedInFreeList;
			HandlingExtraFee = handlingExtraFee ?? source.HandlingExtraFee;
			ExcludeZeroFreightCosts = excludeZeroFreightCosts ?? source.ExcludeZeroFreightCosts;
			StoreId = storeId ?? source.StoreId;
			Weight = weight ?? source.Weight;
			CartSubtotal = cartSubtotal ?? source.CartSubtotal;
		}
	}
}
