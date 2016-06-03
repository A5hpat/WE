// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using AspDotNetStorefront.Caching.ObjectCaching.Dependency;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontCore.ShippingCalculation;

namespace AspDotNetStorefront.Caching.ObjectCaching.ObjectProvider
{
	/// <summary>
	/// Convenience class that wraps <see cref="CachedObjectProvider"/> to provide a cached <see cref="ShippingMethodCollection"/>
	/// </summary>
	public class CachedShippingMethodCollectionProvider : ICachedShippingMethodCollectionProvider
	{
		readonly CachedObjectProvider<ShippingMethodCollection, ShippingMethodCollectionCacheContext> CachedObjectProvider;

		public CachedShippingMethodCollectionProvider(CachedObjectProvider<ShippingMethodCollection, ShippingMethodCollectionCacheContext> cachedObjectProvider)
		{
			CachedObjectProvider = cachedObjectProvider;
		}

		public ShippingMethodCollection Get(Customer customer, Address address, CartItemCollection cartItems, int storeId)
		{
			return CachedObjectProvider.Get(new ShippingMethodCollectionCacheContext(
				customer: customer,
				address: address,
				cartItems: cartItems,
				storeId: storeId));
		}
	}

	/// <summary>
	/// The context needed to generate a <see cref="ShippingMethodCollection"/> when its 
	/// dependencies are invalidated.
	/// </summary>
	public class ShippingMethodCollectionCacheContext
	{
		public readonly Customer Customer;
		public readonly Address Address;
		public readonly CartItemCollection CartItems;
		public readonly int StoreId;

		public ShippingMethodCollectionCacheContext(Customer customer, Address address, CartItemCollection cartItems, int storeId)
		{
			Customer = customer;
			Address = address;
			CartItems = cartItems;
			StoreId = storeId;
		}
	}

	/// <summary>
	/// The behavior needed to generate a new <see cref="ShippingMethodCollection"/> when its
	/// dependencies are invalidated.
	/// </summary>
	public class ShippingMethodCollectionCachedObjectContextBuilder : ICachedObjectContextBuilder<ShippingMethodCollection, ShippingMethodCollectionCacheContext>
	{
		public const string KeyTemplate = "{0}:ShippingMethodCollection";

		readonly IShippingMethodCollectionProvider ShippingMethodCollectionProvider;
		readonly AppConfigValueConverter AppConfigValueConverter;

		public ShippingMethodCollectionCachedObjectContextBuilder(IShippingMethodCollectionProvider shippingMethodCollectionProvider, AppConfigValueConverter appConfigValueConverter)
		{
			ShippingMethodCollectionProvider = shippingMethodCollectionProvider;
			AppConfigValueConverter = appConfigValueConverter;
		}

		public CachedObjectContext<ShippingMethodCollection, ShippingMethodCollectionCacheContext> CreateContext()
		{
			return new CachedObjectContext<ShippingMethodCollection, ShippingMethodCollectionCacheContext>(
				keyBuilder: KeyBuilder,
				objectBuilder: ObjectBuilder,
				dependencyStateBuilder: DependencyStateBuilder);
		}

		string KeyBuilder(ShippingMethodCollectionCacheContext objectContext)
		{
			return string.Format(KeyTemplate, objectContext.Customer.ThisCustomerSession.SessionID);
		}

		ShippingMethodCollection ObjectBuilder(ShippingMethodCollectionCacheContext objectContext)
		{
			return ShippingMethodCollectionProvider.GetShippingMethods(objectContext.Customer, objectContext.Address, objectContext.CartItems, objectContext.StoreId);
		}

		IEnumerable<DependencyStateContext> DependencyStateBuilder(ShippingMethodCollectionCacheContext objectContext)
		{
			return new DependencyStateContext[]
				{
					new AppConfigValueDependencyStateContext("CacheShippingMethods", objectContext.StoreId, appConfigValue => AppConfigValueConverter.ConvertAppConfigValueToTypedValue<bool>(appConfigValue)),

					new AppConfigDependencyStateContext("DefaultShippingCalculationID", objectContext.StoreId),
					new AppConfigDependencyStateContext("FreeShippingThreshold", objectContext.StoreId),
					new AppConfigDependencyStateContext("ShippingMethodIDIfFreeShippingIsOn", objectContext.StoreId),
					new AppConfigDependencyStateContext("FilterOutShippingMethodsThatHave0Cost", objectContext.StoreId),
					new AppConfigDependencyStateContext("ShippingTaxClassID", objectContext.StoreId),
					new AppConfigDependencyStateContext("FreeShippingAllowsRateSelection", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.ActiveCarrier", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.DomesticCarriers", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.InternationalCarriers", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.Insured", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.DefaultItemWeight", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.OriginAddress", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.OriginAddress2", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.OriginCity", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.OriginState", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.OriginZip", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.OriginCountry", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.UPS.UserName", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.UPS.Password", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.UPS.License", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.UPS.Server", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.UPS.TestServer", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.UPS.UPSPickupType", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.UPS.AccountNumber", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.UPS.AddressTypeBehavior", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.UPS.DeliveryConfirmation", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.UPS.GetNegotiatedRates", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.UPS.Services", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.UPS.MaxWeight", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.USPS.UserName", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.USPS.Server", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.USPS.TestServer", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.USPS.MaxWeight", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.USPS.Services", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.FedEx.AccountNumber", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.FedEx.Meter", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.WeightUnits", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.MarkupPercent", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.DumpDebugXmlOnCheckout", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.CallForShippingPrompt", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.FedEx.MaxWeight", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.ShowErrors", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.DHL.AccountNumber", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.DHL.APISystemID", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.DHL.APISystemPassword", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.DHL.Server", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.DHL.TestServer", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.ShippingMethodsToPrevent", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.PackageExtraWeight", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.UseTestRates", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.MultiDistributorCalculation", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.CanadaPost.DefaultPackageSize", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.CanadaPost.Language", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.CanadaPost.MaxWeight", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.CanadaPost.MerchantID", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.CanadaPost.Server", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.CanadaPost.ServerPort", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.DHL.ShipInDays", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.DHLIntl.BillingAccountNbr", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.DHLIntl.BillingParty", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.DHLIntl.Dutiable", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.DHLIntl.DutyPayment", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.DHLIntl.DutyPaymentAccountNbr", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.DHLIntl.MaxWeight", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.DHLIntl.Overrides", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.DHLIntl.Packaging", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.DHLIntl.Services", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.DHLIntl.ShippingKey", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.AusPost.DefaultPackageSize", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.AusPost.DomesticServices", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.AusPost.IntlServices", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.AusPost.MaxWeight", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.SortByRate", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.AllowLocalPickup", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.LocalPickupCost", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.LocalPickupRestrictionType", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.LocalPickupRestrictionStates", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.LocalPickupRestrictionZips", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.LocalPickupRestrictionZones", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.FedEx.Key", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.FedEx.Password", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.FedEx.Server", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.FedEx.SmartPost.IndiciaWeights", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.FedEx.SmartPost.AncillaryEndorsementType", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.FedEx.SmartPost.HubId", objectContext.StoreId),
					new AppConfigDependencyStateContext("RTShipping.FedEx.SmartPost.Enabled", objectContext.StoreId),

					new CheckoutShippingSelectionDependencyStateContext(objectContext.Customer.CustomerID),

					new QueryDependencyStateContext("select UpdatedOn from ShippingByProduct"),
					new QueryDependencyStateContext(
						"select UpdatedOn from ShippingByTotal where StoreId = @storeId",
						new Dictionary<string, object> { {"storeId", objectContext.StoreId } }),
					new QueryDependencyStateContext(
						"select UpdatedOn from ShippingByTotalByPercent where StoreId = @storeId",
						new Dictionary<string, object> { {"storeId", objectContext.StoreId } }),
					new QueryDependencyStateContext(
						"select UpdatedOn from ShippingByWeight where StoreId = @storeId",
						new Dictionary<string, object> { {"storeId", objectContext.StoreId } }),
					new QueryDependencyStateContext(
						"select UpdatedOn from ShippingCalculationStore where StoreId = @storeId",
						new Dictionary<string, object> { {"storeId", objectContext.StoreId } }),
					new QueryDependencyStateContext(
						"select m.UpdatedOn from ShippingMethod m inner join ShippingMethodStore s on m.ShippingMethodID = s.ShippingMethodId where s.StoreId = @storeId",
						new Dictionary<string, object> { {"storeId", objectContext.StoreId } }),
					new QueryDependencyStateContext(
						"select UpdatedOn from ShippingMethodToCountryMap where StoreId = @storeId",
						new Dictionary<string, object> { {"storeId", objectContext.StoreId } }),
					new QueryDependencyStateContext(
						"select UpdatedOn from ShippingMethodToStateMap where StoreId = @storeId",
						new Dictionary<string, object> { {"storeId", objectContext.StoreId } }),
					new QueryDependencyStateContext(
						"select UpdatedOn from ShippingMethodToZoneMap where StoreId = @storeId",
						new Dictionary<string, object> { {"storeId", objectContext.StoreId } }),
					new QueryDependencyStateContext(
						"select UpdatedOn from ShippingTotalByZone where StoreId = @storeId",
						new Dictionary<string, object> { {"storeId", objectContext.StoreId } }),
					new QueryDependencyStateContext("select UpdatedOn from ShippingWeightByZone"),
					new QueryDependencyStateContext("select UpdatedOn from ShippingZone"),

					new CacheEntryDependencyStateContext(string.Format(ShoppingCartCachedObjectContextBuilder.KeyTemplate, objectContext.Customer.ThisCustomerSession.SessionID, "ShoppingCart")),
				};
		}
	}
}
