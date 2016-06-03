// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Hosting;
using AspDotNetStorefrontCore.ShippingCalculation;

namespace AspDotNetStorefrontCore
{
	/// <summary>
	/// Summary description for Shipping.
	/// </summary>
	public class Shipping
    {
        // this MUST match the table defs in ShippingCalculation table
        public enum ShippingCalculationEnum
        {
            Unknown = 0,
            CalculateShippingByWeight = 1,
            CalculateShippingByTotal = 2,
            UseFixedPrice = 3,
            AllOrdersHaveFreeShipping = 4,
            UseFixedPercentageOfTotal = 5,
            UseIndividualItemShippingCosts = 6,
            UseRealTimeRates = 7,
            CalculateShippingByWeightAndZone = 8,
            CalculateShippingByTotalAndZone = 9,
            CalculateShippingByTotalByPercent = 10,
        }

        public enum FreeShippingReasonEnum
        {
            DoesNotQualify = 0,
            AllOrdersHaveFreeShipping = 1,
            AllDownloadItems = 2,
            ExceedsFreeShippingThreshold = 3,
            CustomerLevelHasFreeShipping = 4,
            CouponHasFreeShipping = 5,
            AllFreeShippingItems = 6
        }

		/// <summary>
		/// Flag used to disable shipping filtering per store
		/// </summary>
		public const int DONT_FILTER_PER_STORE = -1;

        public static decimal GetVariantShippingCost(int VariantID, int ShippingMethodID)
        {
			var tmp = Decimal.Zero;

            using (var con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (var rs = DB.GetRS("select ShippingCost from ShippingByProduct  with (NOLOCK)  where VariantID=" + VariantID.ToString() + " and ShippingMethodID=" + ShippingMethodID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldDecimal(rs, "ShippingCost");
                    }
                }
            }
            return tmp;
        }

        static public bool ShippingMethodIsInFreeList(int ShippingMethodID)
        {
            return CommonLogic.IntegerIsInIntegerList(ShippingMethodID, AppLogic.AppConfig("ShippingMethodIDIfFreeShippingIsOn"));
        }

        static public String GetFreeShippingMethodIDs()
        {
            return AppLogic.AppConfig("ShippingMethodIDIfFreeShippingIsOn").Trim();
        }

        static public bool ShippingMethodIsValid(int ShippingMethodID, String StateAbbrev, String CountryName)
        {
            // helper for shoppingcart class for efficiency:
            return ShippingMethodIsAllowedForState(ShippingMethodID, AppLogic.GetStateID(StateAbbrev)) && ShippingMethodIsAllowedForCountry(ShippingMethodID, AppLogic.GetCountryID(CountryName));
        }

        static public bool ShippingMethodToStateMapIsEmpty()
        {
            return (DB.GetSqlN("select count(*) as N from ShippingMethodToStateMap  with (NOLOCK)") == 0);
        }

        static public bool ShippingMethodToCountryMapIsEmpty()
        {
            return (DB.GetSqlN("select count(*) as N from ShippingMethodToCountryMap  with (NOLOCK)") == 0);
        }

        static public bool ShippingMethodToZoneMapIsEmpty()
        {
            return (DB.GetSqlN("select count(*) as N from ShippingMethodToZoneMap  with (NOLOCK)") == 0);
        }

        static public bool ShippingMethodIsAllowedForState(int ShippingMethodID, int StateID)
        {
            if (ShippingMethodToStateMapIsEmpty() || GetActiveShippingCalculationID() == ShippingCalculationEnum.UseRealTimeRates)
            {
                return true;
            }
            return (DB.GetSqlN("select count(*) as N from ShippingMethodToStateMap  with (NOLOCK)  where ShippingMethodID=" + ShippingMethodID.ToString() + " and StateID=" + StateID.ToString()) != 0);
        }

        static public bool ShippingMethodIsAllowedForCountry(int ShippingMethodID, int CountryID)
        {
            if (ShippingMethodToCountryMapIsEmpty() || GetActiveShippingCalculationID() == ShippingCalculationEnum.UseRealTimeRates)
            {
                return true;
            }
            return (DB.GetSqlN("select count(*) as N from ShippingMethodToCountryMap  with (NOLOCK)  where ShippingMethodID=" + ShippingMethodID.ToString() + " and CountryID=" + CountryID.ToString()) != 0);
        }

        public static int ZoneLookup(String zip)
        {
            int ZipCodePrefixLength = AppLogic.AppConfigNativeInt("ZipCodePrefixLength");

            if (ZipCodePrefixLength < 0)
                ZipCodePrefixLength = 3;

            if (ZipCodePrefixLength > 5)
                ZipCodePrefixLength = 5;

            zip = zip.Trim().PadRight(5, '0');
            String ZipSubStr = zip.Substring(0, ZipCodePrefixLength);
            int ZipSubStrInt = 0;
            try
            {
                ZipSubStrInt = Localization.ParseUSInt(ZipSubStr);
            }
            catch
            {
                return AppLogic.AppConfigUSInt("ZoneIDForNoMatch"); // something bad as input zip
            }
            int ZoneID = 0;
            
            using (var dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (var rs = DB.GetRS("select * from ShippingZone with (NOLOCK)", dbconn))
                {
                    while (rs.Read())
                    {
                        String[] thisZipList = Regex.Replace(DB.RSField(rs, "ZipCodes"), "\\s+", "", RegexOptions.Compiled).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (String s in thisZipList)
                        {
                            // is it a single 3 digit prefix, or a range:
                            if (s.IndexOf("-") ==
                                -1)
                            {
                                String s2 = s.Substring(0, ZipCodePrefixLength);
                                // single item:
                                int LowPrefix = 0;
                                try
                                {
                                    if (CommonLogic.IsInteger(s2))
                                    {
                                        LowPrefix = Localization.ParseUSInt(s2);
                                    }
                                }
                                catch
                                {
                                }
                                if (LowPrefix == ZipSubStrInt)
                                {
                                    ZoneID = DB.RSFieldInt(rs, "ShippingZoneID");
                                    break;
                                }
                            }
                            else
                            {
                                // range:
                                String[] s2 = s.Split('-');
                                int LowPrefix = 0;
                                int HighPrefix = 0;
                                try
                                {
                                    String s2a;
                                    s2a = s2[0].Substring(0, ZipCodePrefixLength);
                                    String s2b;
                                    s2b = s2[1].Substring(0, ZipCodePrefixLength);
                                    if (CommonLogic.IsInteger(s2a))
                                    {
                                        LowPrefix = Localization.ParseUSInt(s2a);
                                    }
                                    if (CommonLogic.IsInteger(s2b))
                                    {
                                        HighPrefix = Localization.ParseUSInt(s2b);
                                    }
                                }
                                catch
                                {
                                }
                                if (LowPrefix <= ZipSubStrInt &&
                                    ZipSubStrInt <= HighPrefix)
                                {
                                    ZoneID = DB.RSFieldInt(rs, "ShippingZoneID");
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            if (ZoneID == 0)
            {
                ZoneID = AppLogic.AppConfigUSInt("ZoneIDForNoMatch");
            }
            return ZoneID;
        }

		public static IShippingCalculation GetActiveShippingCalculation(int? storeId = null)
		{
			switch(GetActiveShippingCalculationID(storeId))
			{
				case ShippingCalculationEnum.CalculateShippingByWeight:
					return new CalculateShippingByWeightShippingCalculation();

				case ShippingCalculationEnum.CalculateShippingByTotal:
					return new CalculateShippingByTotalShippingCalculation();

				case ShippingCalculationEnum.CalculateShippingByTotalByPercent:
					return new CalculateShippingByTotalByPercentShippingCalculation();

				case ShippingCalculationEnum.UseFixedPrice:
					return new UseFixedPriceShippingCalculation();

				case ShippingCalculationEnum.AllOrdersHaveFreeShipping:
					return new AllOrdersHaveFreeShippingShippingCalculation();

				case ShippingCalculationEnum.UseFixedPercentageOfTotal:
					return new UseFixedPercentageOfTotalShippingCalculation();

				case ShippingCalculationEnum.UseIndividualItemShippingCosts:
					return new UseIndividualItemShippingCostsShippingCalculation();

				case ShippingCalculationEnum.CalculateShippingByWeightAndZone:
					return new CalculateShippingByWeightAndZoneShippingCalculation();

				case ShippingCalculationEnum.CalculateShippingByTotalAndZone:
					return new CalculateShippingByTotalAndZoneShippingCalculation();

				case ShippingCalculationEnum.UseRealTimeRates:
					return new UseRealTimeRatesShippingCalculation();

				default:
					return null;
			}
		}

		/// <summary>
		/// Gets the active shiping calculation, multi-store aware
		/// </summary>
		public static ShippingCalculationEnum GetActiveShippingCalculationID(int? storeId = null)
		{
			var storeShippingCalculationId = GetActiveStoreShippingCalculationID(storeId ?? AppLogic.StoreID());
			if(storeShippingCalculationId != ShippingCalculationEnum.Unknown)
				return storeShippingCalculationId;

			var defaultStoreShippingCalculationId = GetActiveStoreShippingCalculationID(AppLogic.DefaultStoreID());
			if(defaultStoreShippingCalculationId != ShippingCalculationEnum.Unknown)
				return defaultStoreShippingCalculationId;

			var globalShippingCalculationId = (ShippingCalculationEnum)AppLogic.AppConfigUSInt("DefaultShippingCalculationID");
			return globalShippingCalculationId;
		}

		/// <summary>
		/// Gets the active shipping calculation per store
		/// </summary>
		static ShippingCalculationEnum GetActiveStoreShippingCalculationID(int storeId)
		{
			return (ShippingCalculationEnum)DB.GetSqlN(
				"SELECT ShippingCalculationID N FROM ShippingCalculationStore WITH (NOLOCK) WHERE StoreId = @storeId",
				new SqlParameter("@storeId", storeId));
		}

		public static string GetShippingMethodDisplayName(int shippingMethodId, string localeSetting)
		{
			if(shippingMethodId <= 0)
				return string.Empty;

			using(var connection = new SqlConnection(DB.GetDBConn()))
			using(var command = connection.CreateCommand())
			{
				command.CommandText = "select Name, DisplayName from ShippingMethod with(nolock) where ShippingMethodID = @shippingMethodId";
				command.Parameters.AddWithValue("@shippingMethodId", shippingMethodId);

				connection.Open();
				using(var reader = command.ExecuteReader())
					if(reader.Read())
					{
						var displayName = DB.RSFieldByLocale(reader, "DisplayName", localeSetting);
						return string.IsNullOrEmpty(displayName)
							? DB.RSFieldByLocale(reader, "Name", localeSetting)
							: displayName;
					}
			}

			return string.Empty;
		}

		static public int GetShippingMethodID(string name)
		{
			using(var connection = new SqlConnection(DB.GetDBConn()))
			using(var command = connection.CreateCommand())
			{
				command.CommandText = "select ShippingMethodID from ShippingMethod with(nolock) where Name = @name";
				command.Parameters.AddWithValue("name", name);
				connection.Open();

				var result = command.ExecuteScalar();
				if(result is int)
					return (int)result;
			}

			return 0;
		}

        static public decimal GetShipByTotalCharge(int ShippingMethodID, String RowGUID)
        {
			var tmp = Decimal.Zero;

            using (var con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (var rs = DB.GetRS("Select ShippingCharge from ShippingByTotal  with (NOLOCK)  where RowGUID=" + DB.SQuote(RowGUID) + " and ShippingMethodID=" + ShippingMethodID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldDecimal(rs, "ShippingCharge");
                    }
                }
            }

            return tmp;
        }

        static public decimal GetShipByTotalByPercentCharge(int ShippingMethodID, String RowGUID, out Decimal MinimumCharge, out Decimal SurCharge)
        {
			var tmp = Decimal.Zero;
            MinimumCharge = Decimal.Zero;
            SurCharge = Decimal.Zero;

            using (var con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (var rs = DB.GetRS("Select PercentOfTotal,MinimumCharge,SurCharge from ShippingByTotalByPercent  with (NOLOCK)  where RowGUID=" + DB.SQuote(RowGUID) + " and ShippingMethodID=" + ShippingMethodID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldDecimal(rs, "PercentOfTotal");
                        MinimumCharge = DB.RSFieldDecimal(rs, "MinimumCharge");
                        SurCharge = DB.RSFieldDecimal(rs, "SurCharge");
                    }
                }
            }

            return tmp;
        }

        static public decimal GetShipByFixedPercentageCharge(int ShippingMethodID, decimal SubTotal)
        {
			var tmp = Decimal.Zero;

            using (var con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (var rs = DB.GetRS("Select * from ShippingMethod   with (NOLOCK)  where ShippingMethodID=" + ShippingMethodID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmp = (decimal)DB.RSFieldDecimal(rs, "FixedPercentOfTotal");
                    }
                }
            }

            return (tmp / 100.0M) * SubTotal;
        }

        static public decimal GetShipByItemCharge(int ShippingMethodID, CartItemCollection cartItems)
        {
            decimal tmp = System.Decimal.Zero;
            foreach (CartItem c in cartItems)
            {
                if (!c.IsDownload)
                {
                    int Q = c.Quantity;
                    decimal PR = Shipping.GetVariantShippingCost(c.VariantID, ShippingMethodID) * Q;
                    tmp += PR;
                }
            }
            return tmp;
        }

        static public decimal GetShipByTotalCharge(int ShippingMethodID, decimal SubTotal)
        {
			var tmp = 0.0M;

            using (var con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (var rs = DB.GetRS("select * from ShippingByTotal  with (NOLOCK)  where LowValue<=" + Localization.CurrencyStringForDBWithoutExchangeRate(SubTotal) + " and HighValue>=" + Localization.CurrencyStringForDBWithoutExchangeRate(SubTotal) + " and ShippingMethodID=" + ShippingMethodID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldDecimal(rs, "ShippingCharge");
                    }
                }
            }

            return tmp;
        }

        static public decimal GetShipByTotalByPercentCharge(int ShippingMethodID, decimal SubTotal)
        {
			var tmp = Decimal.Zero;
			var MinimumCharge = Decimal.Zero;
			var SurCharge = Decimal.Zero;

            using (var con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (var rs = DB.GetRS("select * from ShippingByTotalByPercent  with (NOLOCK)  where LowValue<=" + Localization.CurrencyStringForDBWithoutExchangeRate(SubTotal) + " and HighValue>=" + Localization.CurrencyStringForDBWithoutExchangeRate(SubTotal) + " and ShippingMethodID=" + ShippingMethodID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldDecimal(rs, "PercentOfTotal");
                        MinimumCharge = DB.RSFieldDecimal(rs, "MinimumCharge");
                        SurCharge = DB.RSFieldDecimal(rs, "SurCharge");
                    }
                }
            }

            tmp = (SubTotal * (tmp / 100.0M)) + SurCharge;
            if (tmp < MinimumCharge)
            {
                tmp = MinimumCharge;
            }
            return tmp;
        }

        static public decimal GetShipByWeightCharge(int ShippingMethodID, string RowGUID)
        {
			var tmp = Decimal.Zero;

            using (var con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (var rs = DB.GetRS("Select ShippingCharge from ShippingByWeight  with (NOLOCK)  where RowGUID=" + DB.SQuote(RowGUID) + " and ShippingMethodID=" + ShippingMethodID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldDecimal(rs, "ShippingCharge");
                    }
                }
            }

            return tmp;
        }

        static public decimal GetShipByWeightCharge(int ShippingMethodID, Decimal WeightTotal)
        {
			var tmp = 0.0M;

            using (var con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (var rs = DB.GetRS("select * from ShippingByWeight  with (NOLOCK)  where LowValue<=" + Localization.DecimalStringForDB(WeightTotal) + " and HighValue>=" + Localization.DecimalStringForDB(WeightTotal) + " and ShippingMethodID=" + ShippingMethodID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldDecimal(rs, "ShippingCharge");
                    }
                }
            }

            return tmp;
        }

        static public decimal GetShipByWeightAndZoneCharge(int ShippingZoneID, int ShippingMethodID, String RowGUID)
        {
			var tmp = Decimal.Zero;

            using (var con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (var rs = DB.GetRS("Select ShippingCharge from ShippingWeightByZone  with (NOLOCK)  where ShippingMethodID=" + ShippingMethodID.ToString() + " and RowGUID=" + DB.SQuote(RowGUID) + " and ShippingZoneID=" + ShippingZoneID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldDecimal(rs, "ShippingCharge");
                    }
                }
            }

            return tmp;
        }

        static public decimal GetShipByTotalAndZoneCharge(int ShippingZoneID, int ShippingMethodID, String RowGUID)
        {
			var tmp = Decimal.Zero;

            using (var con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (var rs = DB.GetRS("Select ShippingCharge from ShippingTotalByZone  with (NOLOCK)  where ShippingMethodID=" + ShippingMethodID.ToString() + " and RowGUID=" + DB.SQuote(RowGUID) + " and ShippingZoneID=" + ShippingZoneID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldDecimal(rs, "ShippingCharge");
                    }
                }
            }

            return tmp;
        }

        static public decimal GetShipByWeightAndZoneCharge(int ShippingMethodID, Decimal WeightTotal, int ShippingZoneID)
        {
			var tmp = -1.0M;

            using (var con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (var rs = DB.GetRS("Select ShippingCharge from ShippingWeightByZone  with (NOLOCK)  where LowValue<=" + Localization.DecimalStringForDB(WeightTotal) + " and HighValue>=" + Localization.DecimalStringForDB(WeightTotal) + " and ShippingMethodID=" + ShippingMethodID.ToString() + " and ShippingZoneID=" + ShippingZoneID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldDecimal(rs, "ShippingCharge");
                    }
                }
            }

            return tmp;
        }

        static public decimal GetShipByTotalAndZoneCharge(int ShippingMethodID, decimal OrderTotal, int ShippingZoneID)
        {
			var tmp = -1.0M;

            using (var con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (var rs = DB.GetRS("Select ShippingCharge from ShippingTotalByZone  with (NOLOCK)  where LowValue<=" + Localization.CurrencyStringForDBWithoutExchangeRate(OrderTotal) + " and HighValue>=" + Localization.CurrencyStringForDBWithoutExchangeRate(OrderTotal) + " and ShippingMethodID=" + ShippingMethodID.ToString() + " and ShippingZoneID=" + ShippingZoneID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldDecimal(rs, "ShippingCharge");
                    }
                }
            }

            return tmp;
        }

        static public String GetTrackingURL(String ShippingTrackingNumber)
        {
            // Trim tracking number, get rid of spaces and hyphens.
            ShippingTrackingNumber = ShippingTrackingNumber.Replace(" ", "").Replace("-", "").Trim();

            if (ShippingTrackingNumber.Length == 0)
            {
                return "";
            }

            // Check for a match on the ShippingTrackingNumber

            String[] CarrierList = AppLogic.AppConfig("ShippingTrackingCarriers").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            String match = String.Empty;

            foreach (String Carrier in CarrierList)
            {
                match = Regex.Match(ShippingTrackingNumber, AppLogic.AppConfig("ShippingTrackingRegex." + Carrier.Trim()), RegexOptions.Compiled).Value;

                if (match.Length != 0)
                {
                    return String.Format(AppLogic.AppConfig("ShippingTrackingURL." + Carrier.Trim()), ShippingTrackingNumber);
                }
            }

            return "";
        }

        static public List<int> GetDistinctShippingAddressIDs(CartItemCollection cic)
        {
            List<int> addressIDs = new List<int>();
            //String tmpS = ",";
            foreach (CartItem c in cic)
            {
                if (!addressIDs.Contains(c.ShippingAddressID))
                {
                    addressIDs.Add(c.ShippingAddressID);
                }
            }
            return addressIDs;
        }

        static public bool NoShippingRequiredComponents(IEnumerable<CartItem> cartItems)
        {
            return !cartItems
                .Where(ci => ci.Shippable)
                .Any();
        }

        static public bool IsAllFreeShippingComponents(IEnumerable<CartItem> cartItems)
        {
            return cartItems
                .All(ci => ci.FreeShipping || !ci.Shippable || ci.IsDownload || GiftCard.ProductIsEmailGiftCard(ci.ProductID));
        }

        static public string GetFormattedRealTimeShippingMethodForDatabase(string name, decimal freight, decimal vat)
        {
            var usNumberFormat = new System.Globalization.CultureInfo("en-US").NumberFormat;
            return GetFormattedRealTimeShippingMethodForDatabase(name, freight.ToString("0.00", usNumberFormat), vat.ToString("0.00", usNumberFormat));
        }

        static public string GetFormattedRealTimeShippingMethodForDatabase(string name, string freight, string vat)
        {      
            return string.Format("{0}|{1}|{2}", name, freight, vat);
        }

		static public IEnumerable<string> GetShippingIconFileNames()
		{
			var path = HostingEnvironment.MapPath("~/Images/shipping");
			if(!Directory.Exists(path))
				return Enumerable.Empty<string>();

			return Directory.EnumerateFiles(path, "*.*")
				.Where(s => s.EndsWith(".png") || s.EndsWith(".jpg") || s.EndsWith(".gif") || s.EndsWith(".jpeg"))
				.Select(file => Path.GetFileName(file))
				.ToList();
		}
	}

	public class Shipments : List<Packages>
	{
		public bool HasDistributorItems
		{ get; set; }

		public bool HasFreeItems
		{ get; set; }

		public bool IsInternational
		{
			get
			{
				return this
					.Where(packages => packages.DestinationCountry != "US" || packages.DestinationCountry != "United States")
					.Any();
			}
		}

		public void AddPackages(Packages packages)
		{
			Add(packages);
		}
	}

	public class Packages : List<Package>
	{
		public string PickupType
		{ get; set; }

		public string OriginAddress1
		{ get; set; }

		public string OriginAddress2
		{ get; set; }

		public string OriginCity
		{ get; set; }

		string _OriginStateProvince;
		public string OriginStateProvince
		{
			get
			{
				if(_OriginStateProvince == "-" 
					|| _OriginStateProvince == "--" 
					|| _OriginStateProvince == "ZZ")
					return string.Empty;

				return _OriginStateProvince;
			}

			set { _OriginStateProvince = value; }
		}

		public string OriginZipPostalCode
		{ get; set; }

		public string OriginCountryCode
		{ get; set; }

		public string DestinationAddress1
		{ get; set; }

		public string DestinationAddress2
		{ get; set; }

		public string DestinationCity
		{ get; set; }

		string _DestinationStateProvince;
		public string DestinationStateProvince
		{
			get
			{
				if(_DestinationStateProvince == "-" 
					|| _DestinationStateProvince == "--" 
					|| _DestinationStateProvince == "ZZ")
					return string.Empty;

				return _DestinationStateProvince;
			}

			set { _DestinationStateProvince = value; }
		}

		public string DestinationZipPostalCode
		{ get; set; }

		public string DestinationCountry
		{ get; set; }

		public string DestinationCountryCode
		{ get; set; }

		public ResidenceTypes DestinationResidenceType
		{ get; set; }

		public decimal Weight
		{
			get { return this.Sum(package => package.Weight); }
		}

		public int PackageCount
		{
			get
			{
				return this
					.Where(package => package.IsShipSeparately)
					.Select(package => package.Quantity)
					.DefaultIfEmpty(1)
					.Sum();
			}
		}

		public Packages()
		{
			OriginAddress1 = string.Empty;
			OriginAddress2 = string.Empty;
			OriginCity = string.Empty;
			OriginStateProvince = string.Empty;
			OriginZipPostalCode = string.Empty;
			OriginCountryCode = string.Empty;

			DestinationAddress1 = string.Empty;
			DestinationAddress2 = string.Empty;
			DestinationCity = string.Empty;
			DestinationStateProvince = string.Empty;
			DestinationZipPostalCode = string.Empty;
			DestinationCountry = string.Empty;
			DestinationCountryCode = string.Empty;
		}

		public void AddPackage(Package package)
		{
			Add(package);
		}
	}

    public class Package	// Data class which holds information about a single package
    {
        public Package()
        {
        }

        public Package(CartItem fromCartItem)
        {
            //JH fix from 5004 notes
            Quantity = fromCartItem.Quantity;
            IsShipSeparately = fromCartItem.IsShipSeparately;
            //end JH
            DimensionsWHD = fromCartItem.Dimensions.ToLowerInvariant();
            Weight = fromCartItem.Weight != 0.0M ?
                fromCartItem.Weight:
                AppLogic.AppConfigUSDecimal("RTShipping.DefaultItemWeight")
                ;
            if (Weight == 0.0M)
            {
                Weight = 0.5M;
            }
            Weight += AppLogic.AppConfigUSDecimal("RTShipping.PackageExtraWeight");
            Insured = AppLogic.AppConfigBool("RTShipping.Insured");
            InsuredValue = fromCartItem.Price;
            IsFreeShipping = fromCartItem.FreeShipping;
        }

        #region Properties

        private int m_quantity;
        public int Quantity
        {
            get { return m_quantity; }
            set { m_quantity = value; }
        }

        private Boolean m_isshipseparately;
        public Boolean IsShipSeparately
        {
            get { return m_isshipseparately; }
            set { m_isshipseparately = value; }
        }

        private Boolean m_isfreeshipping;
        public Boolean IsFreeShipping
        {
            get { return m_isfreeshipping; }
            set { m_isfreeshipping = value; }
        }

        private Decimal m_insuredvalue;
        public Decimal InsuredValue
        {
            get { return m_insuredvalue; }
            set { m_insuredvalue = value; }
        }

        private int m_packageid;
        public int PackageId
        {
            get { return m_packageid; }
            set { m_packageid = value; }
        }

        private bool m_insured;
        public bool Insured
        {
            get { return m_insured; }
            set { m_insured = value; }
        }

        private Decimal m_width;
        public Decimal Width
        {
            get { return m_width; }
            set { m_width = value; }
        }

        private decimal m_weight;
        public Decimal Weight
        {
            get { return m_weight; }
            set { m_weight = value; }
        }

        private Decimal m_height;
        public Decimal Height
        {
            get { return m_height; }
            set { m_height = value; }
        }

        private Decimal m_length;
        public Decimal Length
        {
            get { return m_length; }
            set { m_length = value; }
        }

        /// <summary>
        /// Gets or sets package dimensions in format of N.NN x N.NN x N.NN.
        /// This is Height x Length x Width
        /// </summary>
        public string DimensionsWHD
        {
            get
            {
                return string.Format("{0}x{1}x{2}",
                    Width,
                    Height,
                    Length);
            }
            set
            {
                string[] dd = value.Split('x');
                try
                {
                    Width = Localization.ParseUSDecimal(dd[0].Trim());
                    Height = Localization.ParseUSDecimal(dd[1].Trim());
                    Length = Localization.ParseUSDecimal(dd[2].Trim());
                }
                catch
                {
					Width = 0;
					Height = 0;
					Length = 0;
                }
            }
        }

        #endregion
    }
}
