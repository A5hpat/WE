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

namespace AspDotNetStorefrontCore.ShippingCalculation
{
	public class UseRealTimeRatesShippingCalculation : IShippingCalculation
	{
		public ShippingMethodCollection GetShippingMethods(ShippingCalculationContext context)
		{
			var methods = GetRates(new RealTimeRateCalculationContext(
					shipmentAddress: context.ShippingAddress,
					shippingHandlingExtraFee: AppLogic.AppConfigUSDecimal("ShippingHandlingExtraFee"),
					cartItems: context.CartItems,
					shipmentValue: context.CartSubtotal(
						true, // includeDiscount
						false, // onlyIncludeTaxableItems
						false, // includeDownloadItems
						true, // includeFreeShippingItems
						false, // includeSystemItems
						false, // useCustomerCurrencySetting
						0, // forShippingAddressId
						true, // excludeTax
						false) // includeShippingNotRequiredItems
						,
					shippingTaxRate: context.TaxRate,
					customerId: context.Customer.CustomerID));

			var availableShippingMethods = new ShippingMethodCollection();
			if(methods.Any())
				availableShippingMethods.AddRange(GetRealTimeShippingMethods(context, methods));
			else if(methods.ErrorMsg.Contains(AppLogic.AppConfig("RTShipping.CallForShippingPrompt")))
				availableShippingMethods.AddRange(GetCallForShippingMethods());

			return availableShippingMethods;
		}

		IEnumerable<ShippingMethod> GetRealTimeShippingMethods(ShippingCalculationContext context, IEnumerable<ShippingMethod> methods)
		{
			foreach(var method in methods)
			{
				method.IsFree = method.IsFree && context.ShippingIsFreeIfIncludedInFreeList;
				method.IsRealTime = true;

				if(context.ShippingIsFreeIfIncludedInFreeList && Shipping.GetFreeShippingMethodIDs().ParseAsDelimitedList<int>().Contains(method.Id))
				{
					method.Freight = 0.0M;
					method.ShippingIsFree = true;
				}

				if(context.ExcludeZeroFreightCosts && method.Freight == decimal.Zero)
					continue;

				// Add it to the db
				var countOfMatchingMethods = DB.GetSqlN(
					"select count(*) N from ShippingMethod with (NOLOCK) where IsRTShipping = 1 and Name = @methodName",
					new SqlParameter("methodName", method.Name));

				if(countOfMatchingMethods == 0)
				{
					var defaultCarrierIcon = GetDefaultCarrierIcon(method.Carrier);
					method.ImageFileName = defaultCarrierIcon;
					DB.ExecuteSQL(
						"insert ShippingMethod(Name, IsRTShipping, ImageFileName) values(@MethodName, 1, @ImageFileName)",
						new SqlParameter("MethodName", method.Name),
						new SqlParameter("ImageFileName", defaultCarrierIcon));
				}

				yield return method;
			}
		}

		IEnumerable<ShippingMethod> GetCallForShippingMethods()
		{
			yield return new ShippingMethod
			{
				Name = AppLogic.AppConfig("RTShipping.CallForShippingPrompt"),
				IsRealTime = true,
			};
		}

		string GetDefaultCarrierIcon(string carrier)
		{
			switch(carrier)
			{
				case RTShipping.UpsName:
					return "ups.png";

				case RTShipping.UspsName:
					return "usps.png";

				case RTShipping.FedExName:
					return "fedex.png";

				case RTShipping.AustraliaPostName:
					return "australiapost.png";

				case RTShipping.CanadaPostName:
					return "canadapost.png";

				default:
					return "default.png";
			}
		}

		Shipments BuildShipments(RealTimeRateCalculationContext context)
		{
			var useDistributors = context.CartItems.HasDistributorComponents() && AppLogic.AppConfigBool("RTShipping.MultiDistributorCalculation");
			var freeShippingAllowsRateSelection = AppLogic.AppConfigBool("FreeShippingAllowsRateSelection");

			var shipments = new Shipments
			{
				HasDistributorItems = useDistributors,
			};

			// Get ship separately cart items 
			var packageId = 1;
			foreach(var cartItem in context.CartItems.Where(ci => ci.IsShipSeparately))
			{
				// Only calculate rates for products that require shipping
				if(cartItem.IsDownload
					|| cartItem.IsSystem
					|| !cartItem.Shippable
					|| GiftCard.ProductIsEmailGiftCard(cartItem.ProductID)
					|| (cartItem.FreeShipping && !freeShippingAllowsRateSelection))
				{
					continue;
				}

				var packages = new Packages();
				ApplyDestinationForAddress(packages, context.ShipmentAddress);

				if(useDistributors && cartItem.DistributorID > 0)
					ApplyOriginForDistributor(packages, cartItem.DistributorID);

				packages.AddPackage(new Package(cartItem)
				{
					PackageId = packageId++,
				});

				shipments.Add(packages);
			}

			// Now get all itmes that do not ship separately, but group them into shipments by distributor.
			// Note that distributor ID 0 will be all of the items without a distributor.
			var maxDistributorId = useDistributors
				? DB.GetSqlN("select max(DistributorID) N from ProductDistributor")
				: 0;

			for(int distributorId = 0; distributorId <= maxDistributorId; distributorId++)
			{
				var remainingItemsWeight = 0m;
				var remainingItemsInsuranceValue = 0m;
				foreach(var cartItem in context.CartItems.Where(ci => !ci.IsShipSeparately))
				{
					// Only calculate rates for products that require shipping
					if(cartItem.IsDownload
						|| cartItem.IsSystem
						|| !cartItem.Shippable
						|| GiftCard.ProductIsEmailGiftCard(cartItem.ProductID)
						|| (cartItem.FreeShipping && !freeShippingAllowsRateSelection))
					{
						continue;
					}

					if(useDistributors && cartItem.DistributorID != distributorId)
						continue;

					var weight = cartItem.Weight;
					if(weight == 0m)
						weight = AppLogic.AppConfigUSDecimal("RTShipping.DefaultItemWeight");

					if(weight == 0m)
						weight = 0.5m; // must have SOMETHING to use!

					remainingItemsWeight += (weight * cartItem.Quantity);
					remainingItemsInsuranceValue += (cartItem.Price * cartItem.Quantity);
				}

				if(remainingItemsWeight == 0m)
					continue;

				var package = new Package
				{
					PackageId = packageId++,
					Weight = remainingItemsWeight + AppLogic.AppConfigUSDecimal("RTShipping.PackageExtraWeight"),
					Insured = AppLogic.AppConfigBool("RTShipping.Insured"),
					InsuredValue = remainingItemsInsuranceValue,
				};

				var packages = new Packages();
				ApplyDestinationForAddress(packages, context.ShipmentAddress);

				if(useDistributors && distributorId != 0)
					ApplyOriginForDistributor(packages, distributorId);

				packages.AddPackage(package);
				shipments.Add(packages);
			}

			return shipments;
		}

		/// <summary>
		/// Get shipping method rates.
		/// </summary>
		ShippingMethodCollection GetRates(RealTimeRateCalculationContext context)
		{
			// Build shipments
			var shipments = BuildShipments(context);

			// Set shipment info
			var realTimeShipping = new RTShipping
			{
				ShipmentWeight = GetHeaviestPackageWeightTotal(context.CartItems),
				DestinationResidenceType = context.ShipmentAddress.ResidenceType,
				ShipmentValue = context.ShipmentValue,
			};

			// Get carriers
			var carriers = shipments.Any()
				? shipments.IsInternational
					? AppLogic.AppConfig("RTShipping.ActiveCarrier")
					: AppLogic.AppConfig("RTshipping.DomesticCarriers")
				: AppLogic.AppConfig("RTShipping.ActiveCarrier");

			// Get results
			string rtShipRequest, rtShipResponse;
			var shippingMethods = (ShippingMethodCollection)realTimeShipping.GetRates(
				shipments,
				carriers,
				context.ShippingTaxRate,
				out rtShipRequest,
				out rtShipResponse,
				context.ShippingHandlingExtraFee,
				AppLogic.AppConfigUSDecimal("RTShipping.MarkupPercent"),
				realTimeShipping.ShipmentValue);

			DB.ExecuteSQL(
				"update customer set RTShipRequest = @rtShipRequest, RTShipResponse = @rtShipResponse where CustomerID = @customerId",
				new SqlParameter("rtShipRequest", rtShipRequest),
				new SqlParameter("rtShipResponse", rtShipResponse),
				new SqlParameter("customerId", context.CustomerId));

			return shippingMethods;
		}

		void ApplyDestinationForAddress(Packages packages, Address address)
		{
			packages.DestinationCity = address.City;
			packages.DestinationStateProvince = address.State.Length == 2
				? address.State
				: "--";
			packages.DestinationZipPostalCode = address.Zip;
			packages.DestinationCountry = address.Country;
			packages.DestinationCountryCode = address.Country.Length == 2   // off site support
				? address.Country
				: AppLogic.GetCountryTwoLetterISOCode(address.Country);
			packages.DestinationResidenceType = address.ResidenceType;
			packages.PickupType = string.IsNullOrEmpty(AppLogic.AppConfig("RTShipping.UPS.UPSPickupType"))
				? RTShipping.PickupTypes.UPSCustomerCounter
				: AppLogic.AppConfig("RTShipping.UPS.UPSPickupType");
		}

		void ApplyOriginForDistributor(Packages packages, int distributorId)
		{
			using(var connection = new SqlConnection(DB.GetDBConn()))
			using(var command = connection.CreateCommand())
			{
				command.CommandText = "select top 1 Address1, Address2, City, State, Country, ZipCode from Distributor with(nolock) where DistributorID = @distributorId and Deleted = 0 and Published = 1";
				command.Parameters.AddWithValue("distributorId", distributorId);

				connection.Open();
				using(var reader = command.ExecuteReader())
					if(reader.Read())
					{
						packages.OriginAddress1 = DB.RSField(reader, "Address1");
						packages.OriginAddress2 = DB.RSField(reader, "Address2");
						packages.OriginCity = DB.RSField(reader, "City");
						packages.OriginStateProvince = DB.RSField(reader, "State");
						packages.OriginCountryCode = AppLogic.GetCountryTwoLetterISOCode(DB.RSField(reader, "Country"));
						packages.OriginZipPostalCode = DB.RSField(reader, "ZipCode");
					}
			}
		}

		/// <summary>
		/// Gets the heaviest package weight total.
		/// </summary>
		/// <returns>Returns the heaviest package weight total.</returns>
		decimal GetHeaviestPackageWeightTotal(IEnumerable<CartItem> cartItems)
		{
			var defaultWeight = AppLogic.AppConfigUSDecimal("RTShipping.DefaultItemWeight");
			var minOrderWeight = AppLogic.AppConfigUSDecimal("MinOrderWeight");

			// check non-ship separately items:
			var sum = cartItems
				.Where(ci => !ci.IsDownload && (!ci.FreeShipping || (ci.FreeShipping && AppLogic.AppConfigBool("FreeShippingAllowsRateSelection"))) && !ci.IsShipSeparately && ci.Shippable)
				.Sum(ci => CommonLogic.IIF(ci.Weight == decimal.Zero, ci.Quantity * defaultWeight, ci.Quantity * ci.Weight));

			// now check ship separately items, and find heaviest package:
			var maxWeightShipSeparately = cartItems
				.Where(ci => !ci.IsDownload && !ci.FreeShipping && ci.IsShipSeparately && ci.Shippable)
				.Select(ci => ci.Weight)
				.DefaultIfEmpty(0)
				.Max();

			var heaviest = Math.Max(sum, maxWeightShipSeparately);

			return heaviest == 0
				? minOrderWeight == 0
					? 0.5m
					: minOrderWeight
				: heaviest;
		}

		public class RealTimeRateCalculationContext
		{
			public readonly decimal ShippingHandlingExtraFee;
			public readonly decimal ShipmentValue;
			public readonly decimal ShippingTaxRate;
			public readonly int CustomerId;
			public readonly Address ShipmentAddress;
			public readonly IEnumerable<CartItem> CartItems;

			public RealTimeRateCalculationContext(
				decimal shippingHandlingExtraFee,
				decimal shipmentValue,
				decimal shippingTaxRate,
				int customerId,
				Address shipmentAddress,
				IEnumerable<CartItem> cartItems)
			{
				ShippingHandlingExtraFee = shippingHandlingExtraFee;
				ShipmentValue = shipmentValue;
				ShippingTaxRate = shippingTaxRate;
				CustomerId = customerId;
				ShipmentAddress = shipmentAddress;
				CartItems = cartItems;
			}
		}
	}
}
