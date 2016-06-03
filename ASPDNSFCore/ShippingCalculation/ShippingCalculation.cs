// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace AspDotNetStorefrontCore.ShippingCalculation
{
	public abstract class ShippingCalculation : IShippingCalculation
	{
		protected abstract bool UsesZones
		{ get; }

		public ShippingMethodCollection GetShippingMethods(ShippingCalculationContext context)
		{
			var availableShippingMethods = new ShippingMethodCollection();
			var shipSql = GenerateShippingMethodsQuery(context, UsesZones);

			using(var connection = new SqlConnection(DB.GetDBConn()))
			{
				connection.Open();
				using(var reader = DB.GetRS(shipSql, connection))
					while(reader.Read())
					{
						var thisMethod = new ShippingMethod
						{
							Id = DB.RSFieldInt(reader, "ShippingMethodID"),
							Name = DB.RSFieldByLocale(reader, "Name", context.Customer.LocaleSetting),
							IsFree = context.ShippingIsFreeIfIncludedInFreeList && Shipping.ShippingMethodIsInFreeList(DB.RSFieldInt(reader, "ShippingMethodID")),
							ImageFileName = DB.RSField(reader, "ImageFileName"),
						};

						if(thisMethod.IsFree)
						{
							thisMethod.ShippingIsFree = true;
							thisMethod.Freight = 0m;
						}
						else
						{
							var freight = CalculateFreight(context, thisMethod.Id, reader);

							if(freight > 0m && context.HandlingExtraFee > 0m)
								freight += context.HandlingExtraFee;

							if(freight < 0)
								freight = 0;

							thisMethod.Freight = freight;
						}

						if(!(context.ExcludeZeroFreightCosts == true && (thisMethod.Freight == 0m && !thisMethod.IsFree)))
							availableShippingMethods.Add(thisMethod);
					}
			}

			return availableShippingMethods;
		}

		protected abstract decimal CalculateFreight(ShippingCalculationContext context, int shippingMethodId, IDataReader reader);

		/// <summary>
		/// Generates the shipping method query per store
		/// </summary>
		/// <param name="context">Calculation context</param>
		/// <param name="includeZone">Whether to include zone mapping</param>
		protected string GenerateShippingMethodsQuery(ShippingCalculationContext context, bool includeZone)
		{
			var shippingMethodToStateMapIsEmpty = Shipping.ShippingMethodToStateMapIsEmpty();
			var shippingMethodToCountryMapIsEmpty = Shipping.ShippingMethodToCountryMapIsEmpty();
			var customerStateID = AppLogic.GetStateID(context.ShippingAddress.State);
			var customerCountryID = AppLogic.GetCountryID(context.ShippingAddress.Country);

			if(customerCountryID < 1 && context.ShippingAddress.Country.Length == 2)
				customerCountryID = AppLogic.GetCountryIDFromTwoLetterISOCode(context.ShippingAddress.Country);

			if(customerCountryID < 1 && context.ShippingAddress.Country.Length == 3)
				customerCountryID = AppLogic.GetCountryIDFromThreeLetterISOCode(context.ShippingAddress.Country);

			var shipsql = new StringBuilder(1024);
			if(context.StoreId == Shipping.DONT_FILTER_PER_STORE)
				shipsql.AppendFormat(
					"select sm.* from ShippingMethod sm with(nolock) where sm.IsRTShipping = 0");
			else
				shipsql.AppendFormat(
					@"select sm.* from ShippingMethod sm 
					inner join ShippingMethodStore sms on sms.ShippingMethodId = sm.ShippingMethodId 
					where sm.IsRTShipping = 0 and sms.StoreId = {0}", context.StoreId);

			if(!shippingMethodToStateMapIsEmpty && customerStateID <= 0)
				shipsql.Append(" and sm.ShippingMethodID not in (select ShippingMethodID from ShippingMethodToStateMap with (NOLOCK))");

			if(!shippingMethodToStateMapIsEmpty && customerStateID > 0)
				shipsql.Append(" and sm.ShippingMethodID in (select ShippingMethodID from ShippingMethodToStateMap with (NOLOCK) where StateID = " + customerStateID.ToString() + ")");

			if(!shippingMethodToCountryMapIsEmpty)
				shipsql.Append(" and sm.ShippingMethodID in (select ShippingMethodID from ShippingMethodToCountryMap with (NOLOCK) where CountryID = " + customerCountryID.ToString() + ")");

			// most of the shipping methods honor the state and country mapping
			// except for the zip zone mappings which only a handful or them use
			if(includeZone && !Shipping.ShippingMethodToZoneMapIsEmpty())
				shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToZoneMap with (NOLOCK) where ShippingZoneID = " + Shipping.ZoneLookup(context.ShippingAddress.Zip).ToString() + ")");

			shipsql.Append(" order by Displayorder");

			return shipsql.ToString();
		}
	}
}
