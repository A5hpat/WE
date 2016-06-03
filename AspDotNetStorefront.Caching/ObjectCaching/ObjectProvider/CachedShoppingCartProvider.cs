// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using AspDotNetStorefront.Caching.ObjectCaching.Dependency;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Caching.ObjectCaching.ObjectProvider
{
	/// <summary>
	/// Convenience class that wraps <see cref="CachedObjectProvider"/> to provide a cached <see cref="ShoppingCart"/>
	/// </summary>
	public class CachedShoppingCartProvider : ICachedShoppingCartProvider
	{
		readonly CachedObjectProvider<ShoppingCart, ShoppingCartCacheContext> CachedObjectProvider;

		public CachedShoppingCartProvider(CachedObjectProvider<ShoppingCart, ShoppingCartCacheContext> cachedObjectProvider)
		{
			CachedObjectProvider = cachedObjectProvider;
		}

		public ShoppingCart Get(Customer customer, CartTypeEnum cartType, int storeId)
		{
			return CachedObjectProvider.Get(new ShoppingCartCacheContext(
				customer: customer,
				cartType: cartType,
				storeId: storeId));
		}
	}

	/// <summary>
	/// The context needed to generate a <see cref="ShoppingCart"/> when its dependencies are 
	/// invalidated.
	/// </summary>
	public class ShoppingCartCacheContext
	{
		public readonly Customer Customer;
		public readonly CartTypeEnum CartType;
		public readonly int StoreId;

		public ShoppingCartCacheContext(Customer customer, CartTypeEnum cartType, int storeId)
		{
			Customer = customer;
			CartType = cartType;
			StoreId = storeId;
		}
	}

	public class ShoppingCartCachedObjectContextBuilder : ICachedObjectContextBuilder<ShoppingCart, ShoppingCartCacheContext>
	{
		public const string KeyTemplate = "{0}:{1}";

		readonly AppConfigValueConverter AppConfigValueConverter;

		public ShoppingCartCachedObjectContextBuilder(AppConfigValueConverter appConfigValueConverter)
		{
			AppConfigValueConverter = appConfigValueConverter;
		}

		public CachedObjectContext<ShoppingCart, ShoppingCartCacheContext> CreateContext()
		{
			return new CachedObjectContext<ShoppingCart, ShoppingCartCacheContext>(
				keyBuilder: KeyBuilder,
				objectBuilder: ObjectBuilder,
				dependencyStateBuilder: DependencyStateBuilder);
		}

		string KeyBuilder(ShoppingCartCacheContext objectContext)
		{
			return string.Format(KeyTemplate, objectContext.Customer.ThisCustomerSession.SessionID, objectContext.CartType);
		}

		ShoppingCart ObjectBuilder(ShoppingCartCacheContext objectContext)
		{
			return new ShoppingCart(objectContext.Customer.SkinID, objectContext.Customer, objectContext.CartType, 0, false);
		}

		IEnumerable<DependencyStateContext> DependencyStateBuilder(ShoppingCartCacheContext objectContext)
		{
			return new DependencyStateContext[]
				{
					new AppConfigValueDependencyStateContext("CacheShoppingCarts", objectContext.StoreId, appConfigValue => AppConfigValueConverter.ConvertAppConfigValueToTypedValue<bool>(appConfigValue)),

					new AppConfigDependencyStateContext("AllowShipToDifferentThanBillTo", objectContext.StoreId),
					new AppConfigDependencyStateContext("AutoSelectFirstSizeColorOption", objectContext.StoreId),
					new AppConfigDependencyStateContext("DefaultAddToCartQuantity", objectContext.StoreId),
					new AppConfigDependencyStateContext("FilterOutShippingMethodsThatHave0Cost", objectContext.StoreId),
					new AppConfigDependencyStateContext("FreeShippingAllowsRateSelection", objectContext.StoreId),
					new AppConfigDependencyStateContext("FreeShippingThreshold", objectContext.StoreId),
					new AppConfigDependencyStateContext("GiftCards.Enabled", objectContext.StoreId),
					new AppConfigDependencyStateContext("MinOrderWeight", objectContext.StoreId),
					new AppConfigDependencyStateContext("Promotions.Enabled", objectContext.StoreId),
					new AppConfigDependencyStateContext("ShippingHandlingExtraFee", objectContext.StoreId),
					new AppConfigDependencyStateContext("ShippingTaxClassID", objectContext.StoreId),
					new AppConfigDependencyStateContext("VAT.Enabled", objectContext.StoreId),

					new CheckoutShippingSelectionDependencyStateContext(objectContext.Customer.CustomerID),

					new QueryDependencyStateContext(
						"select UpdatedOn from ShoppingCart where CustomerId = @customerId",
						new Dictionary<string, object> { { "customerId", objectContext.Customer.CustomerID } }),
					new QueryDependencyStateContext(
						"select UpdatedOn from PromotionUsage where CustomerId = @customerId and OrderId is null",
						new Dictionary<string, object> { { "customerId", objectContext.Customer.CustomerID } }),
					new QueryDependencyStateContext(
						"select UpdatedOn from Promotions where Id in (select PromotionId from PromotionUsage where CustomerId = @customerId and OrderId is null)",
						new Dictionary<string, object> { { "customerId", objectContext.Customer.CustomerID } }),
					new QueryDependencyStateContext(
						"select UpdatedOn from Customer where CustomerId = @customerId",
						new Dictionary<string, object> { { "customerId", objectContext.Customer.CustomerID } }),

					new QueryDependencyStateContext("select UpdatedOn from GiftCard"),
					new QueryDependencyStateContext("select UpdatedOn from OrderOption"),
					new QueryDependencyStateContext("select UpdatedOn from OrderOptionStore"),

					new CacheEntryDependencyStateContext(string.Format(ShippingMethodCollectionCachedObjectContextBuilder.KeyTemplate, objectContext.Customer.ThisCustomerSession.SessionID)),
				};
		}
	}
}
