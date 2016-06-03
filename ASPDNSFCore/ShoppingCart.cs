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
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using AspDotNetStorefront.Promotions;
using AspDotNetStorefront.Routing;

namespace AspDotNetStorefrontCore
{
	public enum CartTypeEnum
	{
		ShoppingCart = 0,
		WishCart = 1,
		RecurringCart = 2,
	}

	public enum DateIntervalTypeEnum
	{
		Unknown = 0,
		Day = 1,				// Intended to be used with an interval number
		Week = 2,				// Intended to be used with an interval number
		Month = 3,				// Intended to be used with an interval number
		Year = 4,				// Intended to be used with an interval number
		Weekly = -1,			// Used by the PayflowPRO recurring API
		BiWeekly = -2,			// Used by the PayflowPRO recurring API
		EveryFourWeeks = -4,	// Used by the PayflowPRO recurring API
		Monthly = -5,			// Used by the PayflowPRO recurring API
		Quarterly = -6,			// Used by the PayflowPRO recurring API
		SemiYearly = -7,		// Used by the PayflowPRO recurring API
		Yearly = -8,			// Used by the PayflowPRO recurring API
		NumberOfDays = -9,		// Used by the PayflowPRO recurring API - Used with interval number (frequency)
	}

	public class OrderOption
	{
		public readonly int ID;
		public readonly Guid UniqueID;
		public readonly string Name;
		public readonly string Description;
		public readonly decimal Cost;
		public readonly decimal TaxRate;
		public readonly int DisplayOrder;
		public readonly int TaxClassID;
		public readonly string ImageUrl;

		public OrderOption(
			int id,
			Guid uniqueId,
			string name,
			string description,
			decimal cost,
			decimal taxRate,
			int displayOrder,
			int taxClassId,
			string imageUrl)
		{
			ID = id;
			UniqueID = uniqueId;
			Name = name;
			Description = description;
			Cost = cost;
			TaxRate = taxRate;
			DisplayOrder = displayOrder;
			TaxClassID = taxClassId;
			ImageUrl = imageUrl;
		}
	}

	public partial class ShoppingCart
	{
		readonly UrlHelper Url;
		readonly bool OnlyLoadRecurringItemsThatAreDue;
		readonly CartActionProvider CartActionProvider;

		// Comma delimited string
		string CustomerSelectedOrderOptions;

		public bool VatIsInclusive
		{ get { return AppLogic.AppConfigBool("VAT.Enabled") && ThisCustomer.VATSettingReconciled == VATSettingEnum.ShowPricesInclusiveOfVAT; } }

		public IList<IDiscountResult> DiscountResults
		{ get; private set; }

		/// <summary>
		/// Gets or sets the type of the cart.
		/// </summary>
		/// <value>The type of the cart.</value>
		public CartTypeEnum CartType
		{ get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether shipping is free.
		/// </summary>
		/// <value><c>true</c> if [shipping is free]; otherwise, <c>false</c>.</value>
		public bool ShippingIsFree
		{ get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether Promotions are allowed. Hides/Shows promo box during checkout
		/// </summary>
		/// <value><c>true</c> if [promotions allowed]; otherwise, <c>false</c>.</value>
		public bool PromotionsEnabled
		{ get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether cart allows shipping method selection.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if [cart allows shipping method selection]; otherwise, <c>false</c>.
		/// </value>
		public bool CartAllowsShippingMethodSelection
		{ get; set; }

		/// <summary>
		/// Gets the order notes.
		/// </summary>
		/// <value>The order notes.</value>
		public string OrderNotes
		{ get; private set; }

		/// <summary>
		/// Gets the finalization data.
		/// </summary>
		/// <value>The finalization data.</value>
		public string FinalizationData
		{ get; private set; }

		/// <summary>
		/// Contains all the order options from db and add-ins if availalbe 
		/// </summary>
		public IEnumerable<OrderOption> AllOrderOptions
		{ get; private set; }

		/// <summary>
		// The selected order options
		/// </summary>
		public IEnumerable<OrderOption> OrderOptions
		{ get; private set; }

		public InventoryTrimmedReason TrimmedReason
		{ get; private set; }

		/// <summary>
		/// Gets a value indicating whether recurring schedule has conflict.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if [recurring schedule conflict]; otherwise, <c>false</c>.
		/// </value>
		public bool RecurringScheduleConflict
		{ get; private set; }

		/// <summary>
		/// Gets a value indicating whether shopping cart contains recurring auto ship.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if [contains recurring auto ship]; otherwise, <c>false</c>.
		/// </value>
		public bool ContainsRecurringAutoShip
		{ get; private set; }

		/// <summary>
		/// Gets the free shipping reason.
		/// </summary>
		/// <value>The free shipping reason.</value>
		public Shipping.FreeShippingReasonEnum FreeShippingReason
		{ get; private set; }

		/// <summary>
		/// Gets or sets the cart items.
		/// </summary>
		/// <value>The cart items.</value>
		public CartItemCollection CartItems
		{ get; private set; }

		/// <summary>
		/// Gets this customer.
		/// </summary>
		/// <value>The this customer.</value>
		public Customer ThisCustomer
		{ get; private set; }

		/// <summary>
		/// Gets the skinID.
		/// </summary>
		/// <value>The skin ID.</value>
		public int SkinID
		{ get; private set; }

		/// <summary>
		/// Gets the original recurring order number.
		/// </summary>
		/// <value>The original recurring order number.</value>
		public int OriginalRecurringOrderNumber
		{ get; private set; }

		/// <summary>
		/// Gets or sets the coupon.
		/// </summary>
		/// <value>The coupon.</value>
		public CouponObject Coupon
		{ get; set; }

		/// <summary>
		/// Gets a value indicating whether coupon is valid.
		/// </summary>
		/// <value><c>true</c> if [coupon is valid]; otherwise, <c>false</c>.</value>
		public bool CouponIsValid
		{ get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ShoppingCart"/> class.
		/// </summary>
		/// <param name="skinId">The skinID.</param>
		/// <param name="thisCustomer">The customer.</param>
		/// <param name="cartType">Type of the cart.</param>
		/// <param name="originalRecurringOrderNumber">The original recurring order number.</param>
		/// <param name="onlyLoadRecurringItemsThatAreDue">if set to <c>true</c> [only load recurring items that are due].</param>
		/// <param name="storeId">The StoreID on the original order.</param>
		public ShoppingCart(int skinId, Customer thisCustomer, CartTypeEnum cartType, int originalRecurringOrderNumber, bool onlyLoadRecurringItemsThatAreDue, int? storeId = null)
		{
			Url = DependencyResolver.Current.GetService<UrlHelper>();
			CartActionProvider = DependencyResolver.Current.GetService<CartActionProvider>();

			DiscountResults = new List<IDiscountResult>();
			CartItems = new CartItemCollection();
			AllOrderOptions = new List<OrderOption>();
			OrderOptions = new List<OrderOption>();
			FreeShippingReason = Shipping.FreeShippingReasonEnum.DoesNotQualify;
			CartAllowsShippingMethodSelection = true;

			SkinID = skinId;
			ThisCustomer = thisCustomer;
			CartType = cartType;
			OriginalRecurringOrderNumber = originalRecurringOrderNumber;
			OnlyLoadRecurringItemsThatAreDue = onlyLoadRecurringItemsThatAreDue;

			if(AppLogic.GetCurrentPageType() != PageTypes.Checkout)
			{
				// if NOT on the cart page itself, remove any quantity 0 items from the cart:
				// the cart page itself MAY NEED 0 quantity items with an existing ShoppingCartRecID, which is why we are not cleaning those on the cart page itself
				DB.ExecuteSQL(
					"delete from ShoppingCart where CustomerID = @customerId and Quantity = 0",
					new SqlParameter("customerId", thisCustomer.CustomerID));

				DB.ExecuteSQL(
					"delete from KitCart where CustomerID = @customerId and ShoppingCartRecID != 0 and ShoppingCartRecID not in (select ShoppingCartRecID from ShoppingCart with(NOLOCK) where CustomerID = @customerId)",
					new SqlParameter("customerId", thisCustomer.CustomerID));
			}

			if(AppLogic.AppConfigBool("Promotions.Enabled"))
				if(thisCustomer.CustomerID == 0 || AppLogic.CustomerLevelAllowsCoupons(ThisCustomer.CustomerLevelID))
					PromotionsEnabled = true;

			LoadFromDB(cartType, storeId ?? AppLogic.StoreID());

			if(!AppLogic.AppConfigBool("GiftCards.Enabled") && Coupon.CouponCode.Length != 0)
				DB.ExecuteSQL("update Customer set CouponCode=NULL where CustomerID=" + thisCustomer.CustomerID.ToString());

			var subtotal = SubTotal(
				includeDiscount: false,
				onlyIncludeTaxableItems: false,
				includeDownloadItems: true,
				includeFreeShippingItems: true);

			CouponIsValid = Coupons.CheckIfCouponIsValidForOrder(thisCustomer, Coupon, subtotal) == AppLogic.ro_OK;
			AnalyzeCartForFreeShippingConditions(FirstItemShippingAddressID());
			
			// Need to calculate the discount then reload in case we've added an item to the cart in our promo calculation (fixes UI being one step behind the DB)
			RecalculateCartDiscount();
			LoadFromDB(cartType, storeId ?? AppLogic.StoreID());
		}

		[Obsolete("This method is Deprecated, and has been replaced with the new AspDotNetStorefrontCore.CartItemManagementProvider class. Please update any calling code accordingly.")]
		public static void Age(int customerId, int numberOfDaysToKeep, CartTypeEnum cartType)
		{
			var customer = new Customer(customerId);
			var cartActionProvider = DependencyResolver.Current.GetService<CartActionProvider>();

			cartActionProvider.RemoveItemsBeyondMaxAge(customer, cartType, numberOfDaysToKeep);
		}

		[Obsolete("This method is Deprecated, and has been replaced with the new AspDotNetStorefrontCore.CartItemManagementProvider class. Please update any calling code accordingly.")]
		public static void ClearDeletedAndUnPublishedProducts(int customerId)
		{
			var customer = new Customer(customerId);
			var cartActionProvider = DependencyResolver.Current.GetService<CartActionProvider>();
			cartActionProvider.ClearDeletedAndUnPublishedProducts(customer);
		}

		public static bool CartIsEmpty(int customerId, CartTypeEnum cartType)
		{
			return DB.GetSqlN(
				"select count(*) as N from ShoppingCart with(nolock) where CartType = @cartType and CustomerID = @customerId",
				new SqlParameter("cartType", (int)cartType),
				new SqlParameter("customerId", customerId)) == 0;
		}

		/// <summary>
		/// Total quantity of CartItems.
		/// </summary>
		/// <param name="customerId">The CustomerID.</param>
		/// <param name="cartType">Type of the cart.</param>
		/// <returns>Returns the total quantity of the cart.</returns>
		public static int NumItems(int customerId, CartTypeEnum cartType)
		{
			if(customerId == 0)
				return 0;

			// Only query items not are available (Published and Deleted = 0)
			var query =
				@"select sum(sc.Quantity) N
				from ShoppingCart sc with(nolock)
				inner join Product p with(nolock) on p.productid = sc.productid
				inner join ProductVariant pv with(nolock) on sc.variantid = pv.variantid
				inner join (
						select distinct a.ProductID, a.StoreID 
						from ShoppingCart a with(nolock) 
						left join ProductStore b with(nolock) on a.ProductID = b.ProductID 
						where 
							@allowProductFiltering = 0 
							or b.StoreID = a.StoreID
					) productstore on sc.ProductID = productstore.ProductID and sc.StoreID = productstore.StoreID
				where 
					sc.CustomerID = @customerId
					and sc.CartType = @cartType
					and p.Published = 1 
					and pv.Published = 1 
					and p.Deleted = 0 
					and pv.Deleted = 0
					and (
						@allowShoppingcartFiltering = 0 
						or sc.StoreID = @storeId
					)";

			return DB.GetSqlN(
				query,
				new SqlParameter("allowProductFiltering", AppLogic.GlobalConfigBool("AllowProductFiltering")),
				new SqlParameter("customerId", customerId),
				new SqlParameter("cartType", (int)cartType),
				new SqlParameter("allowShoppingcartFiltering", AppLogic.GlobalConfigBool("AllowShoppingcartFiltering")),
				new SqlParameter("storeId", AppLogic.StoreID()));
		}

		[Obsolete("This method is Deprecated, and has been replaced with the new AspDotNetStorefrontCore.CartItemManagementProvider class. Please update any calling code accordingly.")]
		public bool CheckInventory(int customerId, out InventoryTrimmedReason reason)
		{
			reason = CartActionProvider.ValidateCartQuantitiesAgainstInventory(ThisCustomer, CartType);
			RecalculateCartDiscount();
			LoadFromDB(CartType, AppLogic.StoreID());
			return reason != InventoryTrimmedReason.None;
		}

		/// <summary>
		///  For now, this is only to synchronize the cart productsku base on the product sku,variant skusuffice and inventory Vendorfullsku
		/// </summary>
		void Synchronize()
		{
			if(!AppLogic.AppConfigBool("UseSKUForProductImageName"))
				return;

			// Execute the SP that will update the productSKU of the shopping cart.
			// Scenarios happen when admin user update the sku of the product, the image base on the sku will not take  effect on the store site(unless he/she add an item to the cart) cause
			// its getting the Productsku from the shoppingCart table which is not yet been updated yet. That why we need to run this SP to synchronize the cart productsku
			DB.ExecuteSQL("dbo.aspdnsf_SynchronizeCart @customerId , @cartType",
				new SqlParameter("customerId", ThisCustomer.CustomerID),
				new SqlParameter("cartType", (int)CartType));
		}

		void LoadOrderOptions()
		{
			if(CartType != CartTypeEnum.ShoppingCart)
				return;

			AllOrderOptions = LoadAllOrderOptions();
			OrderOptions = FilterSelectedOrderOptions(AllOrderOptions);
		}

		IEnumerable<OrderOption> LoadAllOrderOptions()
		{
			var orderOptions = new List<OrderOption>();

			using(var connection = new SqlConnection(DB.GetDBConn()))
			using(var command = connection.CreateCommand())
			{
				command.CommandText = @"
					select distinct
						OrderOption.*
					from
						OrderOption with(nolock)
						left join OrderOptionStore with (nolock) on OrderOption.OrderOptionID = OrderOptionStore.OrderOptionID
					where
						@allowOrderOptionFiltering = 0
						or OrderOptionStore.StoreID = @storeId
					order by 
						OrderOption.DisplayOrder";
				command.Parameters.AddWithValue("allowOrderOptionFiltering", AppLogic.GlobalConfigBool("AllowOrderOptionFiltering"));
				command.Parameters.AddWithValue("storeId", AppLogic.StoreID());

				connection.Open();
				using(var reader = command.ExecuteReader())
					while(reader.Read())
					{
						var imgUrl = AppLogic.LookupImage(
							"OrderOption",
							reader.FieldInt("OrderOptionID"),
							"icon",
							ThisCustomer.SkinID,
							ThisCustomer.LocaleSetting);

						if(imgUrl.StartsWith("../"))
							imgUrl = imgUrl.Replace("../", string.Empty);

						var option = new OrderOption(
							id: reader.FieldInt("OrderOptionID"),
							name: reader.FieldByLocale("Name", ThisCustomer.LocaleSetting),
							description: reader.FieldByLocale("Description", ThisCustomer.LocaleSetting),
							displayOrder: reader.FieldInt("DisplayOrder"),
							uniqueId: reader.FieldGuid("OrderOptionGuid"),
							taxClassId: reader.FieldInt("TaxClassID"),
							cost: reader.FieldDecimal("Cost"),
							taxRate: Prices.OrderOptionVAT(ThisCustomer, reader.FieldDecimal("Cost"), DB.RSFieldInt(reader, "TaxClassID")),
							imageUrl: imgUrl);

						orderOptions.Add(option);
					}
			}

			return orderOptions;
		}

		/// <summary>
		/// Determines which order options are selected by the customer
		/// </summary>
		IEnumerable<OrderOption> FilterSelectedOrderOptions(IEnumerable<OrderOption> allOptions)
		{
			return allOptions
				.Join(
					(CustomerSelectedOrderOptions ?? string.Empty).ParseAsDelimitedList<Guid>(),
					orderOption => orderOption.UniqueID,
					orderOptionGuid => orderOptionGuid,
					(orderOption, orderOptionGuid) => orderOption)
				.Distinct();
		}

		void LoadFromDB(CartTypeEnum cartType, int storeId)
		{
			RecurringScheduleConflict = false;
			ContainsRecurringAutoShip = false;

			Synchronize();

			if(cartType == CartTypeEnum.ShoppingCart && ThisCustomer.CustomerID != 0 && ThisCustomer.PrimaryShippingAddressID != 0)
				// Force all empty or invalid AddressID records in the cart to the primary shipping address.
				DB.ExecuteSQL(@"
					update ShoppingCart 
					set ShippingAddressID = @shippingAddressId 
					where 
						CartType = @cartType 
						and CustomerID = @customerId 
						and (
							ShippingAddressID = 0 
							or ShippingAddressID not in (
								select AddressID from Address with(nolock) where CustomerID = @customerId))",
					new SqlParameter("shippingAddressId", ThisCustomer.PrimaryShippingAddressID),
					new SqlParameter("cartType", (object)CartTypeEnum.ShoppingCart),
					new SqlParameter("customerId", ThisCustomer.CustomerID));

			// We don't support checking out with with multiple recurring (auto-ship) items with differing shipment schedules.
			var recurringVariantsCommaSeparatedList = AppLogic.GetRecurringVariantsList();
			if(!string.IsNullOrEmpty(recurringVariantsCommaSeparatedList))
			{
				var recurringScheduleCount = DB.GetSqlN(@"
					SELECT COUNT(DISTINCT RecurringIntervalType) AS N
					FROM ShoppingCart
					WHERE CartType = @cartType
						AND CustomerID = @customerId
						AND VariantID IN (SELECT Items FROM dbo.Split(@variantIds, ','))
						AND StoreID = @storeId",
					new SqlParameter("cartType", (int)cartType),
					new SqlParameter("customerId", ThisCustomer.CustomerID),
					new SqlParameter("variantIds", recurringVariantsCommaSeparatedList),
					new SqlParameter("storeId", storeId));

				if(recurringScheduleCount > 1)
					RecurringScheduleConflict = true;

				if(recurringScheduleCount > 0)
					ContainsRecurringAutoShip = true;
			}

			CartItems.Clear();
			CartItems.m_couponlist = new List<CouponObject>();

			var customerLevelDiscountPercent = AppLogic.GetCustomerLevelDiscountPercent(ThisCustomer.CustomerLevelID);
			var recurringVariantsList = recurringVariantsCommaSeparatedList.ParseAsDelimitedList<int>();
			var couponSet = false;

			OrderNotes = string.Empty;
			FinalizationData = string.Empty;

			var firstIteration = true;
			using(var connection = new SqlConnection(DB.GetDBConn()))
			using(var command = connection.CreateCommand())
			{
				command.CommandText = "dbo.aspdnsf_GetShoppingCart @cartType, @customerId, @originalRecurringOrderNumber, @onlyLoadRecurringItemsThatAreDue, @storeId";
				command.Parameters.AddWithValue("cartType", (int)cartType);
				command.Parameters.AddWithValue("customerId", ThisCustomer.CustomerID);
				command.Parameters.AddWithValue("originalRecurringOrderNumber", OriginalRecurringOrderNumber);
				command.Parameters.AddWithValue("onlyLoadRecurringItemsThatAreDue", OnlyLoadRecurringItemsThatAreDue);
				command.Parameters.AddWithValue("storeId", storeId);

				connection.Open();
				using(var reader = command.ExecuteReader())
					while(reader.Read())
					{
						var newItem = new CartItem(this, ThisCustomer);
						newItem.CreatedOn = DB.RSFieldDateTime(reader, "CreatedOn");
						newItem.NextRecurringShipDate = DB.RSFieldDateTime(reader, "NextRecurringShipDate");
						newItem.RecurringIndex = DB.RSFieldInt(reader, "RecurringIndex");
						newItem.OriginalRecurringOrderNumber = DB.RSFieldInt(reader, "OriginalRecurringOrderNumber");
						newItem.RecurringSubscriptionID = DB.RSField(reader, "RecurringSubscriptionID");
						newItem.ShoppingCartRecordID = DB.RSFieldInt(reader, "ShoppingCartRecID");
						newItem.CartType = (CartTypeEnum)DB.RSFieldInt(reader, "CartType");
						newItem.ProductID = DB.RSFieldInt(reader, "ProductID");
						newItem.VariantID = DB.RSFieldInt(reader, "VariantID");
						newItem.IsSystem = DB.RSFieldBool(reader, "IsSystem");
						newItem.IsAKit = DB.RSFieldBool(reader, "IsAKit");
						newItem.ProductName = DB.RSFieldByLocale(reader, "ProductName", ThisCustomer.LocaleSetting);
						newItem.VariantName = DB.RSFieldByLocale(reader, "VariantName", ThisCustomer.LocaleSetting);
						newItem.SKU = DB.RSField(reader, "ProductSKU");
						newItem.ManufacturerPartNumber = DB.RSField(reader, "ManufacturerPartNumber");
						newItem.Quantity = DB.RSFieldInt(reader, "Quantity");
						newItem.RequiresCount = DB.RSFieldInt(reader, "RequiresCount");
						newItem.ChosenColor = DB.RSFieldByLocale(reader, "ChosenColor", ThisCustomer.LocaleSetting);
						newItem.ChosenColorSKUModifier = DB.RSField(reader, "ChosenColorSKUModifier");
						newItem.ChosenSize = DB.RSFieldByLocale(reader, "ChosenSize", ThisCustomer.LocaleSetting);
						newItem.ChosenSizeSKUModifier = DB.RSField(reader, "ChosenSizeSKUModifier");
						newItem.TextOption = DB.RSField(reader, "TextOption");
						newItem.ShippingMethodID = DB.RSFieldInt(reader, "ShippingMethodID");
						newItem.ShippingMethod = DB.RSFieldByLocale(reader, "ShippingMethod", ThisCustomer.LocaleSetting);
						newItem.ProductTypeId = DB.RSFieldInt(reader, "ProductTypeId");
						newItem.SEName = DB.RSField(reader, "SEName");
						newItem.ImageFileNameOverride = DB.RSField(reader, "ImageFileNameOverride");

						newItem.TextOptionPrompt = DB.RSFieldByLocale(reader, "TextOptionPrompt", ThisCustomer.LocaleSetting);
						if(newItem.TextOptionPrompt.Length == 0)
							newItem.TextOptionPrompt = "Customization Text";

						newItem.SizeOptionPrompt = DB.RSFieldByLocale(reader, "SizeOptionPrompt", ThisCustomer.LocaleSetting);
						if(newItem.SizeOptionPrompt.Length == 0)
							newItem.SizeOptionPrompt = "Size";

						newItem.ColorOptionPrompt = DB.RSFieldByLocale(reader, "ColorOptionPrompt", ThisCustomer.LocaleSetting);
						if(newItem.ColorOptionPrompt.Length == 0)
							newItem.ColorOptionPrompt = "Color";

						newItem.CustomerEntersPricePrompt = DB.RSFieldByLocale(reader, "CustomerEntersPricePrompt", ThisCustomer.LocaleSetting);
						if(newItem.CustomerEntersPricePrompt.Length == 0)
							newItem.CustomerEntersPricePrompt = "Enter Donation Amount:";

						newItem.Weight = DB.RSFieldDecimal(reader, "ProductWeight");
						newItem.Dimensions = DB.RSField(reader, "ProductDimensions");
						newItem.Notes = DB.RSField(reader, "Notes");

						int minimumQuantity;
						newItem.RestrictedQuantities = AppLogic.GetRestrictedQuantities(DB.RSFieldInt(reader, "VariantID"), out minimumQuantity)
							.ParseAsDelimitedList<int>()
							.ToList();

						newItem.MinimumQuantity = minimumQuantity;

						newItem.ExtensionData = DB.RSField(reader, "ExtensionData");
						newItem.IsUpsell = DB.RSFieldBool(reader, "IsUpSell");
						newItem.OrderShippingDetail = string.Empty;

						newItem.Price = DB.RSFieldDecimal(reader, "ProductPrice");
						newItem.CustomerEntersPrice = DB.RSFieldBool(reader, "CustomerEntersPrice");

						if(!newItem.CustomerEntersPrice && cartType != CartTypeEnum.RecurringCart && !newItem.IsUpsell)
						{
							var newItemPrice = newItem.Price;
							if(!newItem.IsAKit)
							{
								var isOnSale = false;
								newItemPrice = AppLogic.DetermineLevelPrice(newItem.VariantID, ThisCustomer.CustomerLevelID, out isOnSale);
								newItemPrice += AppLogic.GetColorAndSizePriceDelta(DB.RSField(reader, "ChosenColor"), DB.RSField(reader, "ChosenSize"), DB.RSFieldInt(reader, "TaxClassID"), ThisCustomer, true, false);
							}
							else
							{
								newItemPrice = DB.RSFieldDecimal(reader, "ProductPrice");
								if(customerLevelDiscountPercent != 0.0M)
									newItemPrice = AppLogic.GetKitTotalPrice(ThisCustomer.CustomerID, ThisCustomer.CustomerLevelID, newItem.ProductID, newItem.VariantID, newItem.ShoppingCartRecordID);
							}

							if(newItemPrice < decimal.Zero)
								newItemPrice = decimal.Zero; // never know what people will put in the modifiers :)

							if(newItemPrice != newItem.Price)
							{
								newItem.Price = newItemPrice;
								// remember to update the actual db record now!
								DB.ExecuteSQL("update shoppingcart set ProductPrice=" + Localization.CurrencyStringForDBWithoutExchangeRate(newItem.Price) + " where ShoppingCartRecID=" + newItem.ShoppingCartRecordID.ToString());
							}
						}

						var quantityDiscountId = QuantityDiscount.LookupProductQuantityDiscountID(DB.RSFieldInt(reader, "ProductID"));
						newItem.QuantityDiscountID = quantityDiscountId;
						newItem.QuantityDiscountName = quantityDiscountId > 0
							? QuantityDiscount.GetQuantityDiscountName(quantityDiscountId, ThisCustomer.LocaleSetting)
							: string.Empty;
						newItem.QuantityDiscountPercent = 0.0M;
						newItem.IsTaxable = DB.RSFieldBool(reader, "IsTaxable");
						newItem.TaxClassID = DB.RSFieldInt(reader, "TaxClassID");
						newItem.TaxRate = DB.RSFieldDecimal(reader, "TaxRate");
						newItem.IsShipSeparately = DB.RSFieldBool(reader, "IsShipSeparately");
						newItem.IsDownload = DB.RSFieldBool(reader, "IsDownload");
						newItem.DownloadLocation = DB.RSField(reader, "DownloadLocation");

						newItem.FreeShipping = DB.RSFieldTinyInt(reader, "FreeShipping") == 1;
						newItem.Shippable = DB.RSFieldTinyInt(reader, "FreeShipping") != 2;

						newItem.DistributorID = DB.RSFieldInt(reader, "DistributorID");
						newItem.IsRecurring = recurringVariantsList.Contains(newItem.VariantID);

						newItem.RecurringInterval = DB.RSFieldInt(reader, "RecurringInterval");
						if(newItem.RecurringInterval == 0)
							newItem.RecurringInterval = 1; // for backwards compatability

						newItem.RecurringIntervalType = (DateIntervalTypeEnum)DB.RSFieldInt(reader, "RecurringIntervalType");
						if(newItem.RecurringIntervalType == DateIntervalTypeEnum.Unknown)
							newItem.RecurringIntervalType = DateIntervalTypeEnum.Monthly; // for backwards compatibility

						// If the CartType = Recurring then use the ShoppingCart AddressIDs recorded at the order rather than the Customer Address IDs
						if(newItem.CartType == CartTypeEnum.RecurringCart)
						{
							newItem.BillingAddressID = DB.RSFieldInt(reader, "ShoppingCartBillingAddressID");
							newItem.ShippingAddressID = DB.RSFieldInt(reader, "ShoppingCartShippingAddressID");
						}
						else
						{
							newItem.BillingAddressID = DB.RSFieldInt(reader, "CustomerBillingAddressID");
							newItem.ShippingAddressID = DB.RSFieldInt(reader, "ShoppingCartShippingAddressID");
						}
						newItem.IsGift = DB.RSFieldBool(reader, "IsGift");

						CartItems.Add(newItem);

						if(firstIteration)
						{
							firstIteration = false;

							// clear the list of coupons
							CartItems.CouponList.Clear();

							// Note that AppLogic.CustomerLevelAllowsCoupons only applies to promotions
							//  and coupons have been repurposed to only be used for Gift Cards.
							Coupon = Coupons.GetCoupon(ThisCustomer, ThisCustomer.StoreID);
							couponSet = Coupon.CouponSet;
							if(couponSet)
								CartItems.CouponList.Add(Coupon);

							if(cartType == CartTypeEnum.RecurringCart)
							{
								var address = new Address();
								address.LoadByCustomer(ThisCustomer.CustomerID, ThisCustomer.PrimaryBillingAddressID, AddressTypes.Billing);
							}

							OrderNotes = DB.RSField(reader, "OrderNotes");
							FinalizationData = DB.RSField(reader, "FinalizationData");
							CustomerSelectedOrderOptions = reader.Field("OrderOptions");

							LoadOrderOptions();
						}
					}
			}

			foreach(var cartItem in CartItems)
				cartItem.ComputeRates();

			if(!couponSet)
			{
				// clear the list of coupons
				CartItems.CouponList.Clear();

				// create an "empty" coupon, but still set the code:
				Coupon = new CouponObject(
					couponCode: ThisCustomer.CouponCode,
					couponType: CouponTypeEnum.OrderCoupon);
			}
		}

		public void ClearCoupon()
		{
			DB.ExecuteSQL(
				"update Customer set CouponCode = null where CustomerID = @customerId",
				new SqlParameter("customerId", ThisCustomer.CustomerID));

			Coupon = new CouponObject(
				couponType: CouponTypeEnum.OrderCoupon);

			ThisCustomer.CouponCode = string.Empty;
		}

		public void SetCoupon(string couponCode, bool updateCartObject)
		{
			if(string.IsNullOrEmpty(couponCode))
			{
				ClearCoupon();
				return;
			}

			couponCode = couponCode.ToUpperInvariant();
			DB.ExecuteSQL(
				"update Customer set CouponCode = @couponCode where CustomerID = @customerId",
				new SqlParameter("couponCode", couponCode),
				new SqlParameter("customerId", ThisCustomer.CustomerID));

			ThisCustomer.CouponCode = couponCode;

			if(!updateCartObject)
				return;

			var giftCard = new GiftCard(couponCode);
			if(Coupon.CouponType != CouponTypeEnum.GiftCard && giftCard.SerialNumber.Length == 0)
				return;

			var newCoupon = Coupons
				.LoadCoupons(couponCode, AppLogic.StoreID())
				.FirstOrDefault();

			if(newCoupon != null)
			{
				Coupon = newCoupon;
				CouponIsValid = true;
				CartItems.CouponList.Add(newCoupon);
			}
		}

		/// <summary>
		/// Works off currently active CartItems list (which may be a subset of the whole cart on multi-ship orders!)
		/// </summary>
		public decimal WeightTotal()
		{
			var usingRealTimeRates = Shipping.GetActiveShippingCalculationID() == Shipping.ShippingCalculationEnum.UseRealTimeRates;

			var sum = 0m;
			foreach(var cartItem in CartItems)
			{
				// Don't include items that do not require shipping except when FreeShippingAllowsRateSelection = true
				if(!cartItem.IsDownload && cartItem.Shippable && !(cartItem.IsShipSeparately && cartItem.FreeShipping))
				{
					var itemWeight = cartItem.Weight;

					// Adjust weight to be non-zero for RT shipping only:
					if(usingRealTimeRates && itemWeight == 0)
						itemWeight = AppLogic.AppConfigUSDecimal("RTShipping.DefaultItemWeight");

					sum += cartItem.Quantity * itemWeight;
				}
			}

			// Take the largest of the calculated weight, configured minimum order weight, or 0.5.
			sum = Math.Max(sum,
				Math.Max(
					AppLogic.AppConfigUSDecimal("MinOrderWeight"),
					0.5m));

			// RTShipping has an optional PackageExtraWeight factor to account for dunnage
			if(usingRealTimeRates)
				sum += AppLogic.AppConfigUSDecimal("RTShipping.PackageExtraWeight");

			return sum;
		}

		public decimal ShippingTotal(bool includeDiscount, bool includeTax)
		{
			return Prices.ShippingTotal(includeDiscount, includeTax, CartItems, ThisCustomer, OrderOptions);
		}

		public decimal TaxTotal()
		{
			return Prices.TaxTotal(ThisCustomer, CartItems, ShippingTotal(true, false), OrderOptions);
		}

		public decimal Total(bool includeDiscount, Boolean roundBeforeTotaling = true)
		{
			return Prices.Total(includeDiscount, CartItems, ThisCustomer, OrderOptions, true, roundBeforeTotaling);
		}

		public bool GiftCardCoversTotal()
		{
			return Coupon.DiscountAmount >= Total(true);
		}

		/// <summary>
		/// Displays the recurring cart item.
		/// </summary>
		/// <param name="originalRecurringOrderNumber">The original recurring order number.</param>
		/// <param name="skinId">The skinID.</param>
		/// <param name="showCancelButton">if set to <c>true</c> [show cancel button].</param>
		/// <param name="showRetryButton">if set to <c>true</c> [show retry button].</param>
		/// <param name="showRestartButton">if set to <c>true</c> [show restart button].</param>
		/// <param name="gatewayStatus">The gateway status.</param>
		/// <param name="localeSetting">The locale setting.</param>
		/// <param name="parser">Use parser.</param>
		/// <returns></returns>
		public string DisplayRecurring(int originalRecurringOrderNumber, int skinId, bool showCancelButton, bool showRetryButton, bool showRestartButton, string gatewayStatus, string localeSetting, Parser parser)
		{
			if(IsEmpty())
				return new Topic("EmptyRecurringListText", ThisCustomer.LocaleSetting, skinId, parser)
					.Contents;

			var firstCartItem = CartItems[0];
			var originalOrder = new Order(originalRecurringOrderNumber);
			var isPayPalExpressCheckoutOrder = (originalOrder.PaymentMethod == AppLogic.ro_PMPayPalExpress);

			var builder = new StringBuilder(50000);
			builder.Append("<div class=\"recurring-wrap\">");

			if(firstCartItem.RecurringSubscriptionID.Length != 0)
				builder.Append(string.Format("<div class=\"recurring-header\">Original Recurring Order Number: {0}</div> <div class=\"recurring-id\">SubscriptionID: {1}</div> <div class=\"recurring-index\">RecurringIndex={2}</div><div class=\"recurring-created-on\">Created On {3}</div>", originalRecurringOrderNumber, firstCartItem.RecurringSubscriptionID, firstCartItem.RecurringIndex, Localization.ToThreadCultureShortDateString(firstCartItem.CreatedOn)));
			else
				builder.Append(string.Format("<div class=\"recurring-header\">Original Recurring Order Number: {0}</div> <div class=\"recurring-id\">RecurringIndex: {1}</div> <div class=\"recurring-created-on\">Created On {2}</div>", originalRecurringOrderNumber, firstCartItem.RecurringIndex, Localization.ToThreadCultureShortDateString(firstCartItem.CreatedOn)));

			builder.Append("<div class=\"recurring-buttons\">");
			if(showCancelButton)
			{
				var deletePage = AppLogic.IsAdminSite
					? "customer_history.aspx"
					: Url.Action(ActionNames.Delete, ControllerNames.RecurringOrder);

				builder.Append(" <input type=\"button\" class=\"btn btn-default stop-button\" value=\"Stop Future Billing\"");
				builder.AppendFormat(" onClick=\"if(confirm('{0}'))", "Are you sure you want to stop future billing & shipment for this item?");
				builder.AppendFormat(" {{self.location='{0}?deleteid={1}';}}\">", deletePage, firstCartItem.OriginalRecurringOrderNumber);
			}

			if(!isPayPalExpressCheckoutOrder && originalOrder.PaymentGateway != "PAYPAL" && !AppLogic.IsAdminSite)
				builder.Append(" <input type=\"button\" class=\"btn btn-default update-billing-button\" name=\"button" + originalRecurringOrderNumber.ToString() + "\" value=\"Update Billing Info\" onclick=\"javascript:toggleLayer('addressBlock" + originalRecurringOrderNumber.ToString() + "');\" />\n");

			builder.Append("</div>");

			if(!isPayPalExpressCheckoutOrder)
			{
				builder.Append("<div id=\"addressBlock" + originalRecurringOrderNumber.ToString() + "\" class=\"addressBlockDiv\">\n");
				var billingAddress = new Address();
				billingAddress.LoadByCustomer(ThisCustomer.CustomerID, AddressTypes.Billing);
				builder.Append(string.Format("<iframe src=\"editaddressrecurring.aspx?addressid={0}&originalrecurringordernumber={1}\" name=\"addressFrame{1}\" id=\"addressFrame{1}\" frameborder=\"0\" height=\"410\" scrolling=\"auto\" width=\"100%\" marginheight=\"0\" marginwidth=\"0\"></iframe>"
					, billingAddress.AddressID.ToString()
					, originalRecurringOrderNumber));

				builder.Append("</div>\n");
			}

			builder.Append("<table class=\"table table-striped order-table\">");
			builder.Append("<tr class=\"table-header\">");
			builder.Append("<th>");
			builder.Append("Product");
			builder.Append("</th>");
			builder.Append("<th>");
			builder.Append("SKU");
			builder.Append("</th>");
			builder.Append("<th>");
			builder.Append("COLOR");
			builder.Append("</th>");
			builder.Append("<th>");
			builder.Append("SIZE");
			builder.Append("</th>");
			builder.Append("<th>");
			builder.Append("Quantity");
			builder.Append("</th>");
			builder.Append("<th>");
			builder.Append("Price");
			builder.Append("</th>");
			builder.Append("<th>");
			builder.Append("Next Ship Date");
			builder.Append("</th>");
			builder.Append("</tr>");

			var showLinkBack = AppLogic.AppConfigBool("LinkToProductPageInCart");

			foreach(var cartItem in CartItems)
			{
				if(cartItem.OriginalRecurringOrderNumber == originalRecurringOrderNumber)
				{
					builder.Append("<tr class=\"table-row\">");
					builder.Append("<td>");

					if(showLinkBack && !cartItem.IsSystem)
						builder.Append("<a href=\"" + Url.BuildProductLink(cartItem.ProductID) + "\">");

					builder.Append(AppLogic.MakeProperObjectName(cartItem.ProductName, cartItem.VariantName, ThisCustomer.LocaleSetting));
					if(cartItem.TextOption.Length != 0)
					{
						if(cartItem.TextOption.IndexOf("\n") != -1)
						{
							builder.Append("");
							builder.Append("Customization Text:");
							builder.Append("");
							builder.Append(XmlCommon.GetLocaleEntry(cartItem.TextOption, ThisCustomer.LocaleSetting, true).Replace("\n", ""));
						}
						else
						{
							builder.Append(" (Customization Text: " + XmlCommon.GetLocaleEntry(cartItem.TextOption, ThisCustomer.LocaleSetting, true) + ") ");
						}
					}

					if(showLinkBack)
						builder.Append("</a>");

					if(cartItem.IsAKit)
					{
						using(var connection = new SqlConnection(DB.GetDBConn()))
						using(var command = connection.CreateCommand())
						{
							command.CommandText = "select kitItem.Name, kitcart.quantity from kitcart   with (NOLOCK)  inner join kititem   with (NOLOCK)  on kitcart.kititemid=kitItem.kititemid where ShoppingCartrecid=" + cartItem.ShoppingCartRecordID.ToString();

							connection.Open();
							using(var reader = command.ExecuteReader())
								while(reader.Read())
									builder.AppendFormat(" - ({0}) {1}",
										DB.RSFieldInt(reader, "Quantity"),
										DB.RSFieldByLocale(reader, "Name", ThisCustomer.LocaleSetting));
						}
					}
					builder.Append("</td>");
					builder.Append("<td>");
					builder.Append(cartItem.SKU);
					builder.Append("</td>");
					builder.Append("<td>");
					builder.Append(cartItem.ChosenColor.Length == 0
						? "--"
						: cartItem.ChosenColor);
					builder.Append("</td>");
					builder.Append("<td>");
					builder.Append(cartItem.ChosenSize.Length == 0
						? "--"
						: cartItem.ChosenSize);
					builder.Append("</td>");
					builder.Append("<td>");
					builder.Append(cartItem.Quantity);
					builder.Append("</td>");
					builder.Append("<td>");
					var extendedPrice = cartItem.Price * cartItem.Quantity;
					var quantityDiscountPercentage = 0.0M;
					var quantityDiscountType = QuantityDiscount.QuantityDiscountType.Percentage;
					if(QuantityDiscount.CustomerLevelAllowsQuantityDiscounts(ThisCustomer.CustomerLevelID))
					{
						quantityDiscountPercentage = QuantityDiscount.GetQuantityDiscountTablePercentageForLineItem(cartItem, out quantityDiscountType);
						if(quantityDiscountPercentage != 0.0M)
						{
							if(quantityDiscountType == QuantityDiscount.QuantityDiscountType.FixedAmount)
							{
								if(Currency.GetDefaultCurrency() == ThisCustomer.CurrencySetting)
								{
									extendedPrice = (extendedPrice - quantityDiscountPercentage);
								}
								else
								{
									quantityDiscountPercentage = decimal.Round(Currency.Convert(quantityDiscountPercentage, Localization.StoreCurrency(), ThisCustomer.CurrencySetting), 2, MidpointRounding.AwayFromZero);
									extendedPrice = (extendedPrice - quantityDiscountPercentage);
								}
							}
							else
							{
								extendedPrice = extendedPrice * ((100.0M - quantityDiscountPercentage) / 100.0M);
							}
						}
					}
					builder.Append(ThisCustomer.CurrencyString(extendedPrice));
					if(QuantityDiscount.CustomerLevelAllowsQuantityDiscounts(ThisCustomer.CustomerLevelID))
					{
						if(quantityDiscountPercentage != 0.0M)
						{
							builder.Append(" <span>(");
							builder.Append(Localization.CurrencyStringForDBWithoutExchangeRate(quantityDiscountPercentage));
							builder.Append(" ");
							builder.Append("Change This Item");
							builder.Append(")</span>");
						}
					}
					builder.Append("</td>");
					builder.Append("<td>");
					builder.Append(Localization.ToThreadCultureShortDateString(cartItem.NextRecurringShipDate));
					builder.Append("</td>");
					builder.Append("  </tr>");
				}
			}

			builder.Append("  </td>");
			builder.Append("  </tr>");
			builder.Append("</table>");
			builder.Append("</div>");

			return builder.ToString();
		}

		[Obsolete("This method is Deprecated, and has been replaced with the new AspDotNetStorefrontCore.CartItemManagementProvider class. Please update any calling code accordingly.")]
		public int AddItemToCart(int productId, int variantId, int quantity)
		{
			return AddItem(
				ThisCustomer,
				ThisCustomer.PrimaryShippingAddressID,
				productId,
				variantId,
				quantity,
				string.Empty,
				string.Empty,
				string.Empty,
				string.Empty,
				string.Empty,
				CartType,
				false,
				false,
				0,
				null,
				true);
		}

		[Obsolete("This method is Deprecated, and has been replaced with the new AspDotNetStorefrontCore.CartItemManagementProvider class. Please update any calling code accordingly.")]
		public int AddItem(Customer customer, int shippingAddressId, int productId, int variantId, int quantity, string chosenColor, string chosenColorSkuModifier, string chosenSize, string chosenSizeSkuModifier, string textOption, CartTypeEnum cartType, bool updateCartObject, bool isRequired, decimal customerEnteredPrice, KitComposition preferredComposition = null, bool isAGift = false)
		{
			CartActionProvider.AddItemToCart(
				new AddToCartContext()
				{
					Customer = ThisCustomer,
					CartType = CartType,
					ProductId = productId,
					VariantId = variantId,
					Quantity = quantity,
					CustomerEnteredPrice = customerEnteredPrice,
					Color = chosenColor,
					Size = chosenSize,
					TextOption = textOption,
					IsWishlist = cartType == CartTypeEnum.WishCart
				});

			if(updateCartObject)
			{
				CartItems.Clear();
				LoadFromDB(cartType, AppLogic.StoreID());
			}

			var newRecordId = DB.GetSqlN(
				"SELECT ISNULL(ShoppingCartRecID,0) 'N' FROM ShoppingCart(NOLOCK) WHERE ProductId=@ProductId AND VariantID=@VariantId AND CartType=@CartType AND CustomerId=@CustomerId;",
				new SqlParameter("@ProductId", productId),
				new SqlParameter("@VariantId", variantId),
				new SqlParameter("@CartType", (int)cartType),
				new SqlParameter("@CustomerId", ThisCustomer.CustomerID));

			if(!isAGift)
				RecalculateCartDiscount();

			return newRecordId;
		}

		[Obsolete("This method is deprecated, and has been replaced with the new CartActionProvider.ProcessKitComposition method. Please update any calling code accordingly.")]
		public void ProcessKitComposition(KitComposition preferredComposition, int productId, int variantId, int shoppingCartRecID)
		{
			CartActionProvider.ProcessKitComposition(preferredComposition, productId, variantId, shoppingCartRecID, ThisCustomer);
		}

		[Obsolete("This method is Deprecated, and has been replaced with the new AspDotNetStorefrontCore.CartItemManagementProvider class. Please update any calling code accordingly.")]
		public void SetItemQuantity(int shoppingCartRecordId, int quantity)
		{
			CartActionProvider.UpdateItemQuantityInCart(new UpdateQuantityContext()
			{
				Customer = ThisCustomer,
				CartType = CartType,
				ShoppingCartRecId = shoppingCartRecordId,
				Quantity = quantity
			});

			var affectedCartItems = CartItems
				.Where(cartItem => cartItem.ShoppingCartRecordID == shoppingCartRecordId);

			foreach(var cartItem in affectedCartItems)
				cartItem.Quantity = quantity;

			RecalculateCartDiscount();
		}

		[Obsolete("This method is Deprecated, and has been replaced with the new AspDotNetStorefrontCore.CartItemManagementProvider class. Please update any calling code accordingly.")]
		public void RemoveItem(int shoppingCartRecordId)
		{
			CartActionProvider.RemoveItemFromCart(ThisCustomer, CartType, shoppingCartRecordId);

			LoadFromDB(CartType, AppLogic.StoreID());

			var cartItemCount = CartItems.Count;

			for(int i = 0; i < cartItemCount; i++)
				if(CartItems[i].ShoppingCartRecordID == shoppingCartRecordId)
				{
					CartItems.RemoveAt(i);
					break;
				}

			if(CartItems.Count == 0)
				OrderOptions = Enumerable.Empty<OrderOption>();

			RecalculateCartDiscount();
		}

		public bool IsEmpty()
		{
			return !CartItems.Any();
		}

		public void ClearContents()
		{
			DB.ExecuteSQL(@"
				delete from KitCart 
				where 
					CartType = @cartType 
					and (
						ShoppingCartRecID = 0 
						or ShoppingCartRecID in (
							select ShoppingCartRecID 
							from ShoppingCart 
							where
								CustomerID = @customerId
								and CartType = @cartType))",
				new SqlParameter("cartType", (int)CartType),
				new SqlParameter("customerId", ThisCustomer.CustomerID));

			DB.ExecuteSQL(
				"delete from ShoppingCart where CartType = @cartType and CustomerID = @customerId",
				new SqlParameter("cartType", (int)CartType),
				new SqlParameter("customerId", ThisCustomer.CustomerID));

			CartItems.Clear();

			RecalculateCartDiscount();
		}

		public bool HasCoupon()
		{
			return !string.IsNullOrEmpty(Coupon.CouponCode) || !string.IsNullOrEmpty(ThisCustomer.CouponCode);
		}

		public string GetOptionsList()
		{
			var entries = OrderOptions
				.Select(orderOption => string.Format(
					"{0}|{1}|{2}|{3}|{4}|{5}|{6}",
					orderOption.ID,
					orderOption.UniqueID,
					orderOption.Name,
					ThisCustomer.CurrencyString(orderOption.Cost),
					ThisCustomer.CurrencyString(orderOption.TaxRate),
					orderOption.ImageUrl,
					orderOption.TaxClassID));

			return string.Join("^", entries);
		}

		/// <summary>
		/// Returns true if this cart has any items which are download items.
		/// </summary>
		public bool HasDownloadComponents()
		{
			return CartItems.HasDownloadComponents;
		}

		/// <summary>
		/// returns true if this order has ONLY download items
		/// </summary>
		public bool IsAllDownloadComponents()
		{
			return CartItems.IsAllDownloadComponents;
		}

		/// <summary>
		/// Returns true if this order has all items that do not require shipping or are marked as free shipping
		/// </summary>
		public bool IsAllFreeShippingComponents()
		{
			return CartItems.IsAllFreeShippingComponents;
		}

		/// <summary>
		/// Returns true if this order has ONLY items that don't require shipping
		/// </summary>
		public bool NoShippingRequiredComponents()
		{
			return Shipping.NoShippingRequiredComponents(CartItems);
		}

		/// <summary>
		/// Returns true if this order has ONLY system items
		/// </summary>
		public bool IsAllSystemComponents()
		{
			return CartItems.IsAllSystemComponents;
		}

		/// <summary>
		/// Returns the quantity of the specified Product ID in the shopping cart.
		/// </summary>
		public int Count(int productId)
		{
			return CartItems
				.Where(ci => ci.ProductID == productId)
				.Sum(p => p.Quantity);
		}

		public bool IsAllEmailGiftCards()
		{
			return CartItems.IsAllEmailGiftCards;
		}

		public bool HasGiftCards()
		{
			return CartItems.ContainsGiftCard;
		}

		/// <summary>
		/// Determines whether the shopping cart contains recurring items.
		/// </summary>
		public bool ContainsRecurring()
		{
			return CartItems.ContainsRecurring;
		}

		/// <summary>
		/// Returns the shopping cart's first item.
		/// </summary>
		public CartItem FirstItem()
		{
			return CartItems.FirstItem();
		}

		/// <summary>
		/// Returns the shopping cart's first item ShippingAddressID.
		/// </summary>
		public int FirstItemShippingAddressID()
		{
			return CartItems.FirstItemShippingAddressID();
		}

		/// <summary>
		/// Returns true if the shopping cart has multiple shipping addresses.
		/// </summary>
		public bool HasMultipleShippingAddresses()
		{
			return CartItems.HasMultipleShippingAddresses;
		}

		/// <summary>
		/// Sets the FreeShipping member variables
		/// </summary>
		/// <param name="addressId">Shipping AddressID to qualify for free shipping</param>
		public void AnalyzeCartForFreeShippingConditions(int addressId)
		{
			// Assume following tests will all fail
			FreeShippingReason = Shipping.FreeShippingReasonEnum.DoesNotQualify;

			var shippingCalculationId = Shipping.GetActiveShippingCalculationID();
			if(shippingCalculationId == Shipping.ShippingCalculationEnum.AllOrdersHaveFreeShipping)
				FreeShippingReason = Shipping.FreeShippingReasonEnum.AllOrdersHaveFreeShipping;

			if(IsAllDownloadComponents())
				FreeShippingReason = Shipping.FreeShippingReasonEnum.AllDownloadItems;

			if(IsAllFreeShippingComponents())
				FreeShippingReason = Shipping.FreeShippingReasonEnum.AllFreeShippingItems;

			// Test if free shipping threshold is met to the specified address
			var freeShippingThreshold = AppLogic.AppConfigUSDecimal("FreeShippingThreshold");
			var shippingMethodIdIfFreeShippingIsOn = AppLogic.AppConfig("ShippingMethodIDIfFreeShippingIsOn");
			if(freeShippingThreshold != 0 && !string.IsNullOrEmpty(shippingMethodIdIfFreeShippingIsOn))
			{
				// Check State and Country for valid FreeShippingMethods (Zones, aka intra-country areas, are not checked, as this is not supported)
				var shipToAddress = new Address();
				shipToAddress.LoadFromDB(addressId);

				foreach(var freeShippingMethdID in AppLogic.AppConfig("ShippingMethodIDIfFreeShippingIsOn").ParseAsDelimitedList<int>())
				{
					if(Shipping.ShippingMethodIsValid(freeShippingMethdID, shipToAddress.State, shipToAddress.Country)
						|| shippingCalculationId == Shipping.ShippingCalculationEnum.UseRealTimeRates)
					{
						var subtotal = SubTotal(
							includeDiscount: true,
							onlyIncludeTaxableItems: false,
							includeDownloadItems: true,
							includeFreeShippingItems: true,
							includeSystemItems: true,
							useCustomerCurrencySetting: true);

						if(subtotal >= freeShippingThreshold)
							FreeShippingReason = Shipping.FreeShippingReasonEnum.ExceedsFreeShippingThreshold;

						break;
					}
				}
			}

			if(ThisCustomer.CustomerLevelID != 0 && AppLogic.CustomerLevelHasFreeShipping(ThisCustomer.CustomerLevelID))
				FreeShippingReason = Shipping.FreeShippingReasonEnum.CustomerLevelHasFreeShipping;

			if(HasCoupon() && CouponIsValid && Coupon.DiscountIncludesFreeShipping)
				FreeShippingReason = Shipping.FreeShippingReasonEnum.CouponHasFreeShipping;

			// Set flags
			ShippingIsFree = FreeShippingReason != Shipping.FreeShippingReasonEnum.DoesNotQualify;
			CartAllowsShippingMethodSelection = (FreeShippingReason != Shipping.FreeShippingReasonEnum.AllDownloadItems
												&& !(FreeShippingReason == Shipping.FreeShippingReasonEnum.AllFreeShippingItems && !AppLogic.AppConfigBool("FreeShippingAllowsRateSelection")));
		}

		public string GetFreeShippingReason()
		{
			if(FreeShippingReason == Shipping.FreeShippingReasonEnum.AllDownloadItems)
				return "All Download Items";

			if(FreeShippingReason == Shipping.FreeShippingReasonEnum.AllFreeShippingItems)
				return "All Free Shipping Items";

			if(FreeShippingReason == Shipping.FreeShippingReasonEnum.CouponHasFreeShipping)
				return "Promotion Has Free Shipping";

			if(FreeShippingReason == Shipping.FreeShippingReasonEnum.AllOrdersHaveFreeShipping)
				return "All Orders Have Free {0} Shipping";

			if(FreeShippingReason == Shipping.FreeShippingReasonEnum.CustomerLevelHasFreeShipping)
				return string.Format("{0} Customers Get Free Shipping", ThisCustomer.CustomerLevelName);

			if(FreeShippingReason == Shipping.FreeShippingReasonEnum.ExceedsFreeShippingThreshold)
				return AppLogic.GetString("checkoutshipping.aspx.24", SkinID, ThisCustomer.LocaleSetting);

			return string.Empty;
		}

		public bool HasRecurringComponents()
		{
			return CartItems
				.Where(cartItem => cartItem.IsRecurring)
				.Any();
		}

		public bool MeetsMinimumOrderAmount(decimal minimumOrderAmount)
		{
			if(minimumOrderAmount <= 0 || IsEmpty() || IsAllSystemComponents())
				return true;

			var subtotal = SubTotal(
				includeDiscount: false,
				onlyIncludeTaxableItems: false,
				includeDownloadItems: true,
				includeFreeShippingItems: true,
				includeSystemItems: false,
				useCustomerCurrencySetting: false,
				forShippingAddressId: ThisCustomer.PrimaryShippingAddressID,
				excludeTax: true);

			return subtotal >= minimumOrderAmount;
		}

		public bool MeetsMinimumOrderQuantity(decimal minimumOrderQuantity)
		{
			if(minimumOrderQuantity <= 0 || IsEmpty() || IsAllSystemComponents())
				return true;

			var nonSystemItemQuantity = CartItems
				.Where(cartItem => !cartItem.IsSystem)
				.Select(cartItem => cartItem.Quantity)
				.Sum();

			return nonSystemItemQuantity >= minimumOrderQuantity;
		}

		/// <summary>
		/// Returns true if the cart contains more line items than are allowed
		/// </summary>
		/// <param name="maximumOrderQuantity">The maximum number of non-system line items allowed in the cart.</param>
		public bool ExceedsMaximumOrderQuantity(decimal maximumOrderQuantity)
		{
			if(IsEmpty() || IsAllSystemComponents())
				return false;

			var nonSystemItemCount = CartItems
				.Where(cartItem => !cartItem.IsSystem)
				.Count();

			return nonSystemItemCount > maximumOrderQuantity;
		}

		public decimal GetGiftCardTotal()
		{
			var discount = decimal.Round(Currency.Convert(Coupon.DiscountAmount, Localization.StoreCurrency(), ThisCustomer.CurrencySetting), 2, MidpointRounding.AwayFromZero);
			var totalRate = Prices.Total(true, CartItems, ThisCustomer, OrderOptions, true);
			return totalRate - discount < 0
				? totalRate
				: discount;
		}

		public decimal SubTotal(bool includeDiscount, bool onlyIncludeTaxableItems, bool includeDownloadItems, bool includeFreeShippingItems, bool includeSystemItems = true, bool useCustomerCurrencySetting = false, int forShippingAddressId = 0, bool excludeTax = false, bool includeShippingNotRequiredItems = true)
		{
			return Prices.SubTotal(includeDiscount, onlyIncludeTaxableItems, includeDownloadItems, includeFreeShippingItems, includeSystemItems, useCustomerCurrencySetting, forShippingAddressId, excludeTax, CartItems, ThisCustomer, OrderOptions, OriginalRecurringOrderNumber, OnlyLoadRecurringItemsThatAreDue, includeShippingNotRequiredItems);
		}

		public void ApplyShippingRules()
		{
			if(AppLogic.AppConfigBool("SurpressCartShippingRulesCode"))
				return;

			if(!AppLogic.AppConfigBool("AllowShipToDifferentThanBillTo") || AppLogic.AppConfigBool("SkipShippingOnCheckout"))
				DB.ExecuteSQL(@"
					update ShoppingCart 
					set ShippingAddressID = @primaryShippingAddressID 
					where CartType = @cartType and CustomerID = @customerId",
					new SqlParameter("primaryShippingAddressID", ThisCustomer.PrimaryShippingAddressID),
					new SqlParameter("cartType", (object)CartTypeEnum.ShoppingCart),
					new SqlParameter("customerId", ThisCustomer.CustomerID));

			LoadFromDB(CartType, AppLogic.StoreID());
		}

		[Obsolete("This method is Deprecated, and has been replaced with the new AspDotNetStorefrontCore.CartItemManagementProvider class. Please update any calling code accordingly.")]
		public void ConsolidateCartItems()
		{
			CartActionProvider.ConsolidateCartItems(ThisCustomer, CartType);
			LoadFromDB(CartType, AppLogic.StoreID());
		}

		public void RecalculateCartDiscount()
		{
			if(!AppLogic.AppConfigBool("Promotions.Enabled"))
				return;

			DiscountResults = new List<IDiscountResult>();

			// Clear out the cart gift items if the only thing in the cart are gift items
			if(CartItems.Any() && CartItems.All(cartItem => cartItem.IsGift))
				ClearContents();

			if(!IsEmpty())
			{
				var ruleContext = PromotionManager.CreateRuleContext(this);
				if(AppLogic.CustomerLevelAllowsCoupons(ruleContext.CustomerLevel))
				{
					PromotionManager.AutoAssignPromotions(ThisCustomer.CustomerID, ruleContext);
					PromotionManager.PrioritizePromotions(ruleContext);
					DiscountResults = PromotionManager.GetDiscountResultList(ruleContext);
				}
			}
			else if(CartType != CartTypeEnum.WishCart) // We only want to clear promos if this cart type is not a wishlist
			{
				// Clean up promos if the user has nothing in their cart.
				if(PromotionManager.GetPromotionUsagesByCustomer(ThisCustomer.CustomerID).Any())
					PromotionManager.ClearAllPromotionUsages(ThisCustomer.CustomerID);

				// Reset auto assigned anytime the cart is emptied out so that users won't have auto assigned only happen once per session.
				PromotionManager.ResetAutoAssignedPromotions(ThisCustomer.CustomerID);
			}
		}

		public string GetInventoryTrimmedUserMessage(InventoryTrimmedReason reason)
		{
			if(reason == InventoryTrimmedReason.RestrictedQuantities)
				return "Some of your item quantities were reduced because they were not allowed. Please check the product page for allowed quantities.";

			if(reason == InventoryTrimmedReason.MinimumQuantities)
				return "Some item quantities were increased to the minimum allowed level.";

			if(reason == InventoryTrimmedReason.InventoryLevels)
				return "Some of your item quantities were reduced as they exceeded stock on hand.";

			if(reason == InventoryTrimmedReason.NoLongerAvailable)
				return "Some of your items were removed as they were no longer available.";

			return string.Empty;
		}

		public void SetItemCartType(int shoppingCartRecordId, CartTypeEnum cartType)
		{
			// Note: This is only used for MoveAllToCart within the Wishlist and SHOULD NOT be called from anywhere else as it does not follow standard Add/Remove logic
			var cartItem = CartItems.Find(item => item.ShoppingCartRecordID == shoppingCartRecordId);

			if(cartItem != null)
				DB.ExecuteSQL(
					"UPDATE ShoppingCart SET CartType = @CartType WHERE ShoppingCartRecID = @ShoppingCartRecID",
					new SqlParameter[]
					{
						new SqlParameter("CartType", (int)cartType),
						new SqlParameter("ShoppingCartRecID", shoppingCartRecordId)
					});
		}
	}

	public enum InventoryTrimmedReason
	{
		None,
		RestrictedQuantities,
		InventoryLevels,
		MinimumQuantities,
		NoLongerAvailable
	}
}
