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

namespace AspDotNetStorefrontCore
{
	public enum CouponTypeEnum
	{
		OrderCoupon = 0,
		GiftCard = 2
	}

	public class CouponObject
	{
		public readonly string CouponCode;
		public readonly string Description;
		public readonly DateTime StartDate;
		public readonly DateTime ExpirationDate;
		public readonly decimal DiscountPercent;
		public readonly decimal DiscountAmount;
		public readonly bool DiscountIncludesFreeShipping;
		public readonly CouponTypeEnum CouponType;
		public readonly bool Deleted;
		public readonly bool CouponSet;

		public CouponObject(
			string couponCode = null,
			string description = null,
			DateTime? startDate = null,
			DateTime? expirationDate = null,
			decimal? discountPercent = null,
			decimal? discountAmount = null,
			bool? discountIncludesFreeShipping = null,
			CouponTypeEnum? couponType = null,
			bool? deleted = null,
			bool? couponSet = null)
		{
			CouponCode = couponCode ?? string.Empty;
			Description = description ?? string.Empty;
			ExpirationDate = expirationDate ?? DateTime.MinValue;
			DiscountPercent = discountPercent ?? 0;
			DiscountAmount = discountAmount ?? 0;
			DiscountIncludesFreeShipping = discountIncludesFreeShipping ?? false;
			StartDate = startDate ?? DateTime.MaxValue;
			CouponType = couponType ?? CouponTypeEnum.GiftCard;
			Deleted = deleted ?? false;
			CouponSet = couponSet ?? false;
		}
	}

	public class Coupons
	{
		public static CouponObject GetCoupon(Customer customer, int storeId)
		{
			using(var connection = new SqlConnection(DB.GetDBConn()))
			{
				connection.Open();
				return GetCoupon(connection, customer, storeId);
			}
		}

		public static CouponObject GetCoupon(SqlConnection connection, Customer customer, int storeId)
		{
			return LoadCoupons(connection, customer.CouponCode, storeId)
				.FirstOrDefault()
				?? new CouponObject(couponSet: false);
		}

		public static IEnumerable<CouponObject> LoadCoupons(string couponCode, int storeId)
		{
			using(var connection = new SqlConnection(DB.GetDBConn()))
			{
				connection.Open();
				return LoadCoupons(connection, couponCode, storeId);
			}
		}

		public static IEnumerable<CouponObject> LoadCoupons(SqlConnection connection, string couponCode, int storeId)
		{
			var storeFilteringEnabled = AppLogic.GlobalConfigBool("AllowGiftCardFiltering");
			var couponList = new List<CouponObject>();

			using(var command = connection.CreateCommand())
			{
				command.CommandText = @"
					select 
						SerialNumber, 
						StartDate, 
						ExpirationDate, 
						Balance 
					from 
						GiftCard with(nolock) 
					where
						(
							@storeFilteringEnabled = 0 
							or GiftCardID in (select GiftCardID from GiftCardStore where StoreID = @storeId)
						)
						and StartDate <= getdate() 
						and ExpirationDate >= getdate() 
						and DisabledByAdministrator = 0 
						and Balance > 0 
						and SerialNumber = @serialNumber";
				command.Parameters.AddWithValue("serialNumber", couponCode);
				command.Parameters.AddWithValue("storeFilteringEnabled", storeFilteringEnabled);
				command.Parameters.AddWithValue("storeId", storeId);

				using(var reader = command.ExecuteReader())
					while(reader.Read())
						couponList.Add(new CouponObject(
							couponSet: true,
							couponCode: DB.RSField(reader, "SerialNumber"),
							couponType: CouponTypeEnum.GiftCard,
							startDate: DB.RSFieldDateTime(reader, "StartDate"),
							expirationDate: DB.RSFieldDateTime(reader, "ExpirationDate"),
							discountAmount: DB.RSFieldDecimal(reader, "Balance")));

				return couponList;
			}
		}

		/// <summary>
		/// Determines if a coupon is valid for an order based on customer, customer level, coupon parameters, products, and order
		/// </summary>
		/// <param name="customer">Customer object representing the customer making the purchase</param>
		/// <param name="coupon">Coupon object representing the coupon and all of its settings</param>
		/// <param name="subTotalBeforeDiscounts">Subtotal of items in the shopping cart before any discounts have been applied</param>
		/// <returns>String 'AppLogic.ro_OK' if coupon is valid or there is no coupon, else returns reason why coupon is not valid</returns>
		public static string CheckIfCouponIsValidForOrder(Customer customer, CouponObject coupon, decimal subTotalBeforeDiscounts)
		{
			if(string.IsNullOrEmpty(coupon.CouponCode))
				return "You have entered an invalid gift card";

			if(coupon.ExpirationDate == DateTime.MinValue)
				return "You have entered an invalid gift card";

			if(coupon.Deleted)
				return "You have entered an invalid gift card";

			if(coupon.StartDate > DateTime.Now)
				return "You have entered an invalid gift card";

			if(coupon.ExpirationDate < DateTime.Now)
				return "That promotion has expired";

			return AppLogic.ro_OK;
		}
	}
}
