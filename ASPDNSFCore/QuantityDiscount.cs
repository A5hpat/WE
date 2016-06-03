// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using System.Web;

namespace AspDotNetStorefrontCore
{
	public class QuantityDiscount
	{
		public enum QuantityDiscountType
		{
			None = -1,
			Percentage = 0,
			FixedAmount = 1
		}

		static public string GetQuantityDiscountName(int QuantityDiscountID, string LocaleSetting)
		{
			var tmpS = string.Empty;

			using (var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using (var rs = DB.GetRS("Select Name from QuantityDiscount   with (NOLOCK)  where QuantityDiscountID=" + QuantityDiscountID.ToString(), con))
				{
					if (rs.Read())
					{
						tmpS = DB.RSFieldByLocale(rs, "Name", LocaleSetting);
					}
				}
			}

			return tmpS;
		}

		// don't return any quotes, single quotes, or carraige returns in this string!
		static public String GetQuantityDiscountDisplayTable(int DID, int SkinID)
		{
			String CacheName = "GetQuantityDiscountDisplayTable_" + DID.ToString() + "_" + SkinID.ToString();
			if (AppLogic.CachingOn)
			{
				String CacheData = (String)HttpContext.Current.Cache.Get(CacheName);
				if (CacheData != null)
				{
					if (CommonLogic.ApplicationBool("DumpSQL"))
					{
						HttpContext.Current.Response.Write("Cache Hit Found!");
					}
					return CacheData;
				}
			}
			Customer ThisCustomer = HttpContext.Current.GetCustomer();
			bool fixedDiscount = isFixedQuantityDiscount(DID);
			StringBuilder tmpS = new StringBuilder(10000);
			String sql = "select * from dbo.QuantityDiscountTable  with (NOLOCK)  where QuantityDiscountID=" + DID.ToString() + " order by LowQuantity";

            tmpS.Append("<table class=\"table table-striped quantity-discount-table\">");
            tmpS.Append("<tr class=\"table-header\"><th>Quantity</th><th>" + CommonLogic.IIF(fixedDiscount, "Per Item ", "") + "Discount </th></tr>");
			using (var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using (var rs = DB.GetRS(sql, con))
				{
					while (rs.Read())
					{
                        tmpS.Append("<tr class=\"table-row\">");
						tmpS.Append("<td class=\"quantity-cell\">");
						tmpS.Append(DB.RSFieldInt(rs, "LowQuantity").ToString() + CommonLogic.IIF(DB.RSFieldInt(rs, "HighQuantity") > 9999, "+", "-" + DB.RSFieldInt(rs, "HighQuantity").ToString()));
						tmpS.Append("</td>");
						tmpS.Append("<td class=\"discount-cell\">");
						if (fixedDiscount)
						{
							tmpS.Append(Localization.CurrencyStringForDisplayWithExchangeRate(DB.RSFieldDecimal(rs, "DiscountPercent"), ThisCustomer.CurrencySetting));
						}
						else
						{
							tmpS.Append(DB.RSFieldDecimal(rs, "DiscountPercent").ToString("N" + AppLogic.AppConfigNativeInt("QuantityDiscount.PercentDecimalPlaces")) + "%");
						}
						tmpS.Append("</td>");
						tmpS.Append("</tr>");
					}
				}
			}

			tmpS.Append("</table>");

			if (AppLogic.CachingOn)
			{
				HttpContext.Current.Cache.Insert(CacheName, tmpS.ToString(), null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
			}
			return tmpS.ToString();
		}

		/// <summary>
		/// Returns the Quantity Discount Percent for the specified product and quantity
		/// </summary>
		/// <param name="productId">The product ID to evaluate</param>
		/// <param name="quantity">The quantity value to evaluate</param>
		/// <returns></returns>
		static private Decimal GetQuantityDiscountTablePercentage(int customerId, int productId, int quantity, out QuantityDiscountType discountType)
		{
			discountType = QuantityDiscountType.Percentage;

			if(productId == 0)
				return 0M;

			var customer = new Customer(customerId);
			if(customer.CustomerLevelID > 0
				&& !CustomerLevelAllowsQuantityDiscounts(customer.CustomerLevelID))
				return 0M;

			var retVal = 0M;
			var query = "select dbo.GetQtyDiscount(@productid, @qty, 0) Pct, dbo.GetQtyDiscount(@productid, @qty, 1) Amt";
			var productIdParam = new []
				{
					DB.CreateSQLParameter("@productid", SqlDbType.Int, 4, productId, ParameterDirection.Input),
					DB.CreateSQLParameter("@qty", SqlDbType.Int, 4, quantity, ParameterDirection.Input)
				};

			using (var connection = new SqlConnection(DB.GetDBConn()))
			{
				connection.Open();

				using (var reader = DB.GetRS(query, connection, productIdParam))
				{
					if(!reader.Read())
						return retVal;

					var percentage = DB.RSFieldDecimal(reader, "Pct");
					var amount = DB.RSFieldDecimal(reader, "Amt");
					if (amount > decimal.Zero)
					{
						discountType = QuantityDiscountType.FixedAmount;
						retVal = amount;
					}
					else
					{
						discountType = QuantityDiscountType.Percentage;
						retVal = percentage;
					}
				}
			}

			return retVal;
		}

		static public Decimal GetQuantityDiscountTablePercentageWithoutCartAwareness(int customerId, int ProductID, int Quantity, out QuantityDiscountType DiscountType)
		{
			return GetQuantityDiscountTablePercentage(customerId, ProductID, Quantity, out DiscountType);
		}

		static public Decimal GetQuantityDiscountTablePercentageForLineItem(CartItem item, out QuantityDiscountType DiscountType)
		{
			if (!AppLogic.AppConfigBool("QuantityDiscount.CombineQuantityByProduct"))
				return GetQuantityDiscountTablePercentage(item.ThisCustomer.CustomerID, item.ProductID, item.Quantity, out DiscountType);

			if (item.ThisShoppingCart.CartItems.Count < 1)
				throw new ArgumentException("cart items must be greater than 0.");

			int quan = 0;
			foreach (CartItem c in item.ThisShoppingCart.CartItems)
			{
				if (c.ProductID == item.ProductID)
				{
					quan += c.Quantity;
				}
			}

			return GetQuantityDiscountTablePercentage(item.ThisCustomer.CustomerID, item.ProductID, quan, out DiscountType);
		}

		/// <summary>
		/// Returns the Quantity Discount ID for the specified Product
		/// </summary>
		/// <param name="ProductID">The productID to find the QuantityDiscountID for</param>
		/// <returns>Quantity Discount ID</returns>
		static public int LookupProductQuantityDiscountID(int ProductID)
		{

			var DID = 0;
			SqlParameter[] spa = { DB.CreateSQLParameter("@productid", SqlDbType.Int, 4, ProductID, ParameterDirection.Input) };
			
			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(var rs = DB.GetRS("select dbo.GetQtyDiscountID(@productid) DID", spa, con))
					if(rs.Read())
						DID = DB.RSFieldInt(rs, "DID");
			}

			if(DID == -1)
				SysLog.LogMessage(
					message: AppLogic.GetString("admin.entity.invalidRelationship.Message"),
					details: string.Format("ProductID {0} is mapped to a Product Group that, through one or more parent/child Product Group mappings, is creating a circular reference (For example: ProdGroup A -> is child of -> ProdGroup B -> is child of -> ProdGroup A).", ProductID),
					messageType: MessageTypeEnum.GeneralException,
					messageSeverity: MessageSeverityEnum.Error);

			return Math.Max(DID, 0);
		}

		static public bool isFixedQuantityDiscount(int quantityDiscountID)
		{
			var tmp = false;
			if (quantityDiscountID != 0)
			{
				using (var con = new SqlConnection(DB.GetDBConn()))
				{
					con.Open();
					using (var rs = DB.GetRS("select DiscountType from  QuantityDiscount  with (NOLOCK)  where QuantityDiscountID=" + quantityDiscountID, con))
					{
						if (rs.Read())
						{
							tmp = DB.RSFieldBool(rs, "DiscountType");
						}
					}
				}
			}
			return tmp;
		}

		static public bool CustomerLevelAllowsQuantityDiscounts(int customerLevelId)
		{
			using(var connection = new SqlConnection(DB.GetDBConn()))
			{
				connection.Open();
				return CustomerLevelAllowsQuantityDiscounts(connection, customerLevelId);
			}
		}

		static public bool CustomerLevelAllowsQuantityDiscounts(SqlConnection connection, int customerLevelId)
		{
			if(customerLevelId == 0)
				return true;

			using(var command = connection.CreateCommand())
			{
				command.CommandText = "select LevelAllowsQuantityDiscounts from CustomerLevel  with(nolock) where CustomerLevelID = @customerLevelId";
				command.Parameters.AddWithValue("customerLevelId", customerLevelId);

				using(var reader = command.ExecuteReader())
					if(reader.Read())
						return DB.RSFieldBool(reader, "LevelAllowsQuantityDiscounts");
			}

			return false;
		}
	}
}
