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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using AspDotNetStorefront.Caching.ObjectCaching;
using AspDotNetStorefront.Checkout;

namespace AspDotNetStorefrontCore
{
	/// <summary>
	/// Class responsible for all pricing calculations.  All new routines
	/// and modifications to existing routines related to pricing should
	/// be made in this class to maintain rounding consistency.
	/// </summary>
	public partial class Prices
	{
		#region Vat

		/// <summary>
		/// Calculates display price for a given item considering sale price, extended
		/// price, customer level, attributes with price modifiers, and VAT for
		/// product and entity pages.
		/// </summary>
		/// <param name="ThisCustomer">Customer object for customer level and discount data retrieval</param>
		/// <param name="variantID">int value indicating the variant id of the item being priced</param>
		/// <param name="regularPrice">Decimal value representing the regular price</param>
		/// <param name="salePrice">Decimal value representing the sale price</param>
		/// <param name="extPrice">Decimal value representing the extended price</param>
		/// <param name="attributesPriceDelta">Decimal value representing any additional price for attributes</param>
		/// <param name="returnDiscountedPrice">Boolean value indicating whether to calculate the regular price or the discounted (sale, extended, customer level) price</param>
		/// <param name="taxClassID">int value represending the tax class id of the current item</param>
		/// <returns>Decimal price, rounded to 2 decimal places</returns>
		public static Decimal VariantPrice(Customer ThisCustomer, int variantID, decimal regularPrice, decimal salePrice, decimal extPrice, decimal attributesPriceDelta, Boolean returnDiscountedPrice, int taxClassID)
		{
			// is VAT enabled and supported
			Boolean VATEnabled = AppLogic.AppConfigBool("VAT.Enabled");

			// is the item taxable
			Boolean taxable = IsTaxable(variantID);

			// set the regular price
			Decimal price = regularPrice + attributesPriceDelta;

			// if trying to get the discounted (sale, customer level discounted, or extended) price
			if(returnDiscountedPrice)
			{
				// is sale price defined and does customer not belong to a customer level
				if(salePrice > System.Decimal.Zero && ThisCustomer.CustomerLevelID == 0)
				{
					price = salePrice + attributesPriceDelta;
				}

				// if this customer is a member of a customer level, determine customer level prices
				if(ThisCustomer.CustomerLevelID > 0)
				{
					// determine multiplier for customer level discount
					decimal customerLevelMultiplier = 1 - (ThisCustomer.LevelDiscountPct / 100);

					// defaults to regular price with applied discount
					decimal customerLevelPrice = (regularPrice + attributesPriceDelta) * customerLevelMultiplier;

					// if extended pricing is defined (non-zero), use it instead
					if(extPrice > System.Decimal.Zero)
					{
						// do customerLevel discounts apply to extended pricing?
						if(ThisCustomer.DiscountExtendedPrices)
						{
							// yes, use extended pricing and apply discount
							customerLevelPrice = (extPrice + attributesPriceDelta) * customerLevelMultiplier;
						}
						else
						{
							// no, just use extended price with no additional discount
							customerLevelPrice = extPrice + attributesPriceDelta;
						}
					}

					// set the return price to the customer level discounted price
					price = customerLevelPrice;
				}
			}

			// if the item is taxable, VAT is enabled, and viewing prices inclusive of VAT
			if(taxable && VATEnabled && ThisCustomer.VATSettingReconciled == VATSettingEnum.ShowPricesInclusiveOfVAT)
			{
				// add the VAT rate to the price
				price = price + (price * (TaxRate(ThisCustomer, taxClassID) / 100));
			}

			// return the price, rounded to 2 decimal places
			return price;
		}

		/// <summary>
		/// Determines the VAT amount for a line item in the shopping cart
		/// </summary>
		/// <param name="ci">CartItem to retrieve VAT for</param>
		/// <param name="ThisCustomer">Customer object</param>
		/// <returns>The VAT amount for a line item for display in the shopping cart</returns>
		public static Decimal LineItemVAT(CartItem ci, List<CouponObject> cList, Customer ThisCustomer)
		{
			Decimal VAT = 0.0M;
			Decimal price = ci.Price;

			// if there is a quantity discount
			if(ci.QuantityDiscountID > 0)
			{
				// get product level quantity discount price
				price = GetQuantityDiscount(ci, ThisCustomer);
			}

			if(ci.ThisShoppingCart.DiscountResults != null)
			{
				foreach(var discountResult in ci.ThisShoppingCart.DiscountResults)
				{
					foreach(var discountedItem in discountResult.DiscountedItems.Where(di => di.ShoppingCartRecordId == ci.ShoppingCartRecordID))
					{
						price += ((discountedItem.Quantity != 0) ? discountedItem.DiscountAmount / discountedItem.Quantity : 0);
						if(price < 0)
							price = 0;
					}
				}
			}

			VAT = GetVATPrice(price, ci.Quantity, ThisCustomer, ci.TaxClassID);

			return VAT;
		}

		public static Decimal GetVATPrice(Decimal price, Customer customer, Int32 taxClassId)
		{
			return GetVATPrice(price, 1, customer, taxClassId);
		}

		public static Decimal GetVATPrice(Decimal price, Int32 quantity, Customer customer, Int32 taxClassId)
		{
			if(AppLogic.AppConfigBool("VAT.RoundPerItem"))
				// Multiply the price by the tax, then round to 2 decimals, then multiply by the quantity to get the VAT amount
				return Math.Round(price * (TaxRate(customer, taxClassId) / 100), 2, MidpointRounding.AwayFromZero) * quantity;
			else
				// Multiply the price by the tax, then multiply by the quantity, then round to 2 decimals to get the VAT amount
				return price * (TaxRate(customer, taxClassId) / 100) * quantity;
		}

		#endregion

		#region Tax

		/// <summary>
		/// Determines if a Variant has been set as taxable
		/// </summary>
		/// <param name="VariantID">The VariantID of the item to check</param>
		/// <returns>True if the item is taxable, else false</returns>
		public static bool IsTaxable(int VariantID)
		{
			var taxable = false;

			// establish connection to the database
			using(var conn = new SqlConnection(DB.GetDBConn()))
			{
				// open the connection
				conn.Open();

				// create the data reader to retireve the information from the ProductVariant table
				using(var reader = DB.GetRS("SELECT IsTaxable FROM dbo.ProductVariant with(NOLOCK) WHERE VariantID=" + VariantID.ToString(), conn))
				{
					// determine if the item is taxable by checking the IsTaxable field
					taxable = reader.Read() && DB.RSFieldTinyInt(reader, "IsTaxable") == 1;
				}
			}

			// return true for taxable, false for non-taxable
			return taxable;
		}

		/// <summary>
		/// Returns the tax rate for the specified tax class for the Customer's curent shipping address, if the TaxCalcMode AppConfig is set to "billing" the rate is for the billing address
		/// </summary>
		/// <param name="TaxClassID">The item tax class ID</param>
		/// <returns>The tax rate for the item</returns>
		public static Decimal TaxRate(Customer ThisCustomer, int TaxClassID)
		{
			// if the TaxCalcMode AppConfig parameter is set to billing
			if("billing".Equals(AppLogic.AppConfig("TaxCalcMode"), StringComparison.InvariantCultureIgnoreCase))
			{
				// use the billing address to get the tax rate
				return TaxRate(ThisCustomer.PrimaryBillingAddress, TaxClassID, ThisCustomer);
			}
			else // TaxCalcMode is shipping
			{
				// use the shipping address to get the tax rate
				return TaxRate(ThisCustomer.PrimaryShippingAddress, TaxClassID, ThisCustomer);
			}
		}

		/// <summary>
		/// Determines the appropriate tax rate to be charged.  If a tax AddIn is present/installed, it will be
		/// used to calculate the tax rate rather than the in-store logic.
		/// </summary>
		/// <param name="useAddress">The customer address to calculate taxes for</param>
		/// <param name="TaxClassID">The item tax class id</param>
		/// <param name="ThisCustomer">The customer being taxed</param>
		/// <returns></returns>
		public static Decimal TaxRate(Address useAddress, int taxClassID, Customer thisCustomer)
		{
			Decimal rate = System.Decimal.Zero;

			//sets default vat country id
			int countryID = AppLogic.AppConfigBool("VAT.Enabled") ? AppLogic.AppConfigUSInt("VAT.CountryID") : 0;
			int stateID = 0;
			string zipCode = String.Empty;

			if(useAddress.CountryID > 0)
			{
				countryID = useAddress.CountryID;
			}
			if(useAddress.StateID > 0)
			{
				stateID = useAddress.StateID;
			}
			if(useAddress.Zip.Trim().Length != 0)
			{
				zipCode = useAddress.Zip.Trim();
			}

			rate = TaxRate(countryID, stateID, zipCode, taxClassID, thisCustomer);

			return rate;
		}

		/// <summary>
		/// Determines the appropriate tax rate based on the Country, State, and ZipCode.  Should never be called directly from outside
		/// the Prices class unless the intention is to bypass any Tax Add-Ins.
		/// </summary>
		/// <param name="CountryID">CountryID of the country where the tax is calulated, set to -1 for no country tax</param>
		/// <param name="StateID">StateID of the state where the tax is calulated, set to -1 for no country tax</param>
		/// <param name="ZipCode">Postal Code of the region where the tax is calulated, set to empty string for no zip code tax</param>
		/// <param name="TaxClassID">The product tax class</param>
		/// <returns>Decimal rate</returns>
		private static Decimal TaxRate(int CountryID, int StateID, string ZipCode, int TaxClassID, Customer ThisCustomer)
		{
			if(ThisCustomer.LevelHasNoTax || ThisCustomer.IsVatExempt())
			{
				return 0;
			}

			Decimal rate = System.Decimal.Zero;

			rate += AppLogic.CountryTaxRatesTable.GetTaxRate(CountryID, TaxClassID);
			rate += AppLogic.StateTaxRatesTable.GetTaxRate(StateID, TaxClassID);
			rate += AppLogic.ZipTaxRatesTable.GetTaxRate(ZipCode, TaxClassID, CountryID);

			return rate;
		}

		/// <summary>
		/// Calculates the total amount of tax to be charged
		/// </summary>
		/// <param name="ThisCustomer">Customer object </param>
		/// <param name="cartItems">A CartItemCollection collection of items in the shopping cart</param>
		/// <param name="shipCost">The total cost of shipping</param>
		/// <param name="orderOptions">A collection of OrderOption</param>
		/// <returns></returns>
		public static Decimal TaxTotal(Customer thisCustomer, CartItemCollection cartItems, Decimal shipCost, IEnumerable<OrderOption> orderOptions)
		{
			if(AppLogic.AppConfigBool("VAT.Enabled") && thisCustomer.VATSettingReconciled == VATSettingEnum.ShowPricesInclusiveOfVAT)
				return 0.00M;

			try
			{
				// Apply Avalara if enabled
				if(AppLogic.AppConfigBool("AvalaraTax.Enabled"))
				{
					AvaTax avaTax = new AvaTax();
					return avaTax.GetTaxRate(thisCustomer, cartItems, orderOptions);
				}
			}
			catch(Exception Ex)
			{
				SysLog.LogException(Ex, MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
			}

			/**********************************************************************/
			//Determine internal tax rates if there are no AddIn interfaces and Avalara is disabled or there were exceptions using either.
			var taxAmount = Decimal.Zero;

			// Promotions: Look for promotions in the shopping cart and find any order level discounts
			//  and save them for discounting line items before calculating taxes.
			var totalDiscount = 0.00M;
			if(cartItems.HasDiscountResults)
				totalDiscount = -cartItems.DiscountResults.Sum(dr => dr.OrderTotal);

			var taxClassIds = new List<Int32>();
			using(var conn = new SqlConnection(DB.GetDBConn()))
			{
				conn.Open();
				using(var rs = DB.GetRS("select TaxClassID from TaxClass Order By DisplayOrder", conn))
				{
					while(rs.Read())
					{
						taxClassIds.Add(DB.RSFieldInt(rs, "TaxClassID"));
					}
				}
			}

			// item tax
			foreach(var taxClassId in taxClassIds)
			{
				Decimal lineItemTotal = System.Decimal.Zero;

				foreach(CartItem ci in cartItems.Where(c => c.IsTaxable && c.TaxClassID == taxClassId))
				{
					lineItemTotal += LineItemPrice(ci, cartItems.CouponList, thisCustomer);

					if(totalDiscount > 0)
					{
						if(totalDiscount > lineItemTotal)
						{
							totalDiscount -= lineItemTotal;
							lineItemTotal = 0;
						}
						else
						{
							lineItemTotal -= totalDiscount;
							totalDiscount = 0;
						}
					}
				}

				if(AppLogic.AppConfigBool("VAT.Enabled") && thisCustomer.VATSettingReconciled != VATSettingEnum.ShowPricesInclusiveOfVAT)
					taxAmount += GetVATPrice(lineItemTotal, thisCustomer, taxClassId);
				else if(!AppLogic.AppConfigBool("VAT.Enabled"))
					taxAmount += lineItemTotal * (TaxRate(thisCustomer, taxClassId) / 100);
			}

			//shipping tax
			List<int> shipAddresses = Shipping.GetDistinctShippingAddressIDs(cartItems);
			if(shipAddresses.Count() == 1)
			{
				taxAmount += ShippingTax(shipCost, shipAddresses.First(), thisCustomer);
			}
			else
			{
				foreach(int addr in shipAddresses)
				{
					IEnumerable<CartItem> addrCart = cartItems.Where(c => c.ShippingAddressID == addr);
					CartItemCollection tmpcic = new CartItemCollection(addrCart);
					shipCost = ShippingTotal(true, false, tmpcic, thisCustomer, orderOptions);
					taxAmount += ShippingTax(shipCost, addr, thisCustomer);
				}
			}

			// order option tax
			taxAmount += orderOptions.Sum(oo => oo.TaxRate);

			return taxAmount;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="shipCost"></param>
		/// <param name="addressID"></param>
		/// <returns></returns>
		public static decimal ShippingTax(Decimal shipCost, int addressID, Customer thisCustomer)
		{
			Address addr = new Address();
			addr.LoadFromDB(addressID);

			return ShippingTax(shipCost, addr, thisCustomer);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public static decimal ShippingTax(Decimal shipCost, Address shipAddr, Customer thisCustomer)
		{
			Decimal tax = System.Decimal.Zero;

			tax = shipCost * (TaxRate(shipAddr, AppLogic.AppConfigUSInt("ShippingTaxClassID"), thisCustomer) / 100.0M);

			return tax;
		}

		public static CartItemCollection RemoveTaxRelatedCartItems(CartItemCollection Collection)
		{
			if(AppLogic.AppConfigBool("AvalaraTax.Enabled"))
				Collection.RemoveAll(c => c.SKU.ToLower() == "shipping" || c.ProductID == 0);

			return Collection;
		}

		#endregion

		#region Order Option

		/// <summary>
		/// Returns the VAT amount for an order option
		/// </summary>
		/// <param name="ci">CartItem to retrieve VAT for</param>
		/// <param name="ThisCustomer">Customer object</param>
		/// <returns></returns>
		public static Decimal OrderOptionVAT(Customer ThisCustomer, Decimal optionCost, int TaxClassID)
		{
			Decimal VAT = System.Decimal.Zero;

			VAT = (TaxRate(ThisCustomer, TaxClassID) * optionCost) / 100;

			return VAT;
		}

		/// <summary>
		/// Calculates the subtotal of all order options selected
		/// </summary>
		/// <returns>Decimal optiontotal, rounded to 2 decimal places</returns>
		public static Decimal OrderOptionTotal(Customer ThisCustomer, IEnumerable<OrderOption> OrderOptions)
		{
			Decimal optiontotal = System.Decimal.Zero;

			bool VATEnabled = AppLogic.AppConfigBool("VAT.Enabled");
			bool VATOn = (VATEnabled && ThisCustomer.VATSettingReconciled == VATSettingEnum.ShowPricesInclusiveOfVAT);

			if(OrderOptions.Count() > 0)
			{
				// cost and tax was already computed upon cart.LoadOrderOptions
				if(VATOn)
				{
					optiontotal = OrderOptions.Sum(opt => opt.Cost + opt.TaxRate);
				}
				else
				{
					optiontotal = OrderOptions.Sum(opt => opt.Cost);
				}
			}

			return optiontotal;
		}

		#endregion

		#region Line Item Price

		/// <summary>
		/// Calculates the final line item price for display in the shopping cart,
		/// considering quantity discounts, customer level percent discounts, product
		/// level coupon discounts, sale prices, extended prices, and VAT.
		/// </summary>
		/// <returns>Decimal price, rounded to 2 decimal places</returns>
		public static Decimal LineItemPrice(CartItem cartItem, IEnumerable<CouponObject> coupons, Customer customer, Boolean includeDiscount = true, Boolean includeTax = true)
		{
			var vatEnabled = AppLogic.AppConfigBool("VAT.Enabled");
			var price = cartItem.Price;

			// if retrieving the discounted price apply quantity discounts and coupons
			if(includeDiscount)
			{
				if(cartItem.QuantityDiscountID > 0)
					price = GetQuantityDiscount(cartItem, customer);

				// Promotions: If there are any promotions assigned to the cart, we need to look at each discount and see if there are any line item level
				//  discounts and decrement the price by the amount.
				if(cartItem.ThisShoppingCart.DiscountResults != null)
				{
					foreach(var discountResult in cartItem.ThisShoppingCart.DiscountResults)
					{
						foreach(var discountedItem in discountResult.DiscountedItems.Where(di => di.ShoppingCartRecordId == cartItem.ShoppingCartRecordID))
						{
							price += ((discountedItem.Quantity != 0) ? discountedItem.DiscountAmount / discountedItem.Quantity : 0);
							if(price < 0)
								price = 0;
						}
					}
				}
			}

			// If the item is taxable and we're either not excluding the tax, or vat is enabled and the tax is inclusive.
			if(cartItem.IsTaxable && (includeTax || (vatEnabled && customer.VATSettingReconciled == VATSettingEnum.ShowPricesInclusiveOfVAT)))
			{
				if(vatEnabled && customer.VATSettingReconciled == VATSettingEnum.ShowPricesInclusiveOfVAT)
				{
					if(AppLogic.AppConfigBool("VAT.RoundPerItem"))
						price = Math.Round(price + (price * (TaxRate(customer, cartItem.TaxClassID) / 100)), 4, MidpointRounding.AwayFromZero) * cartItem.Quantity;
					else
						price = (price + (price * (TaxRate(customer, cartItem.TaxClassID) / 100))) * cartItem.Quantity;
				}
				else
				{
					// taxes aren't shown per line item, just multiply the price by the quantity
					price = price * cartItem.Quantity;
				}
			}
			else
				price = price * cartItem.Quantity;

			return price;
		}

		#endregion

		#region SubTotal

		public static Decimal SubTotal(bool includeDiscount, bool onlyIncludeTaxableItems, bool includeDownloadItems, bool includeFreeShippingItems, bool includeSystemItems, bool UseCustomerCurrencySetting, CartItemCollection cic, Customer ThisCustomer, IEnumerable<OrderOption> OrderOptions)
		{
			return SubTotal(includeDiscount, onlyIncludeTaxableItems, includeDownloadItems, includeFreeShippingItems, includeSystemItems, UseCustomerCurrencySetting, 0, false, cic, ThisCustomer, OrderOptions);
		}

		public static Decimal SubTotal(bool includeDiscount, bool onlyIncludeTaxableItems, bool includeDownloadItems, bool includeFreeShippingItems, bool includeSystemItems, bool UseCustomerCurrencySetting, int ForShippingAddressID, bool ExcludeTax, CartItemCollection cic, Customer ThisCustomer, IEnumerable<OrderOption> OrderOptions)
		{
			return SubTotal(includeDiscount, onlyIncludeTaxableItems, includeDownloadItems, includeFreeShippingItems, includeSystemItems, UseCustomerCurrencySetting, ForShippingAddressID, ExcludeTax, cic, ThisCustomer, OrderOptions, 0, false);
		}

		public static Decimal SubTotal(bool includeDiscount, bool onlyIncludeTaxableItems, bool includeDownloadItems, bool includeFreeShippingItems, bool includeSystemItems, bool UseCustomerCurrencySetting, int ForShippingAddressID, bool excludeTax, CartItemCollection cic, Customer ThisCustomer, IEnumerable<OrderOption> OrderOptions, int OriginalRecurringOrderNumber, bool OnlyLoadRecurringItemsThatAreDue)
		{
			return SubTotal(includeDiscount, onlyIncludeTaxableItems, includeDownloadItems, includeFreeShippingItems, includeSystemItems, UseCustomerCurrencySetting, ForShippingAddressID, excludeTax, cic, ThisCustomer, OrderOptions, OriginalRecurringOrderNumber, OnlyLoadRecurringItemsThatAreDue, true);
		}

		public static Decimal SubTotal(bool includeDiscount, bool onlyIncludeTaxableItems, bool includeDownloadItems, bool includeFreeShippingItems, bool includeSystemItems, bool UseCustomerCurrencySetting, int ForShippingAddressID, bool excludeTax, CartItemCollection cic, Customer ThisCustomer, IEnumerable<OrderOption> OrderOptions, int OriginalRecurringOrderNumber, bool OnlyLoadRecurringItemsThatAreDue, bool includeShippingNotRequiredItems)
		{
			Decimal subTotal = Decimal.Zero;
			IEnumerable<CartItem> tmpList = cic;

			if(onlyIncludeTaxableItems)
				tmpList = tmpList.Where(ci => ci.IsTaxable);

			if(!includeDownloadItems)
				tmpList = tmpList.Where(ci => !ci.IsDownload);

			if(!includeFreeShippingItems)
				tmpList = tmpList.Where(ci => !ci.FreeShipping);

			if(!includeShippingNotRequiredItems)
				tmpList = tmpList.Where(ci => ci.Shippable);

			foreach(CartItem ci in tmpList)
				subTotal += LineItemPrice(ci, cic.CouponList, ThisCustomer, includeDiscount, excludeTax);

			if(includeDiscount)
				subTotal = GetOrderCustomerLevelDiscount(subTotal, ThisCustomer);

			subTotal += OrderOptionTotal(ThisCustomer, OrderOptions);

			return subTotal;
		}

		#endregion

		#region Shipping

		/// <summary>
		/// Calculates the total to be charged for shipping
		/// </summary>
		/// <returns>Decimal shipping</returns>
		public static decimal ShippingTotal(bool includeDiscount, bool includeTax, CartItemCollection cartItems, Customer customer, IEnumerable<OrderOption> orderOptions)
		{
			var effectiveShippingAddressProvider = (IEffectiveShippingAddressProvider)System.Web.Mvc.DependencyResolver.Current.GetService(typeof(IEffectiveShippingAddressProvider));
			var effectiveShippingAddress = effectiveShippingAddressProvider.GetEffectiveShippingAddress(customer);

			if(includeTax)
			{
				var vatOn = AppLogic.AppConfigBool("VAT.Enabled")
					&& customer.VATSettingReconciled == VATSettingEnum.ShowPricesInclusiveOfVAT;

				var taxBilling = AppLogic
					.AppConfig("TaxCalcMode")
					.Equals("billing", StringComparison.InvariantCultureIgnoreCase);

				if(vatOn && taxBilling && customer.PrimaryBillingAddressID > 0)
					effectiveShippingAddress = customer.PrimaryBillingAddress;
			}

			return ShippingTotalForAddress(cartItems, customer, effectiveShippingAddress, includeTax);
		}

		public static decimal ShippingTotalForAddress(CartItemCollection cartItems, Customer customer, Address address, bool includeTax)
		{
			if(cartItems == null || !cartItems.Any())
				return 0;

			var persistedCheckoutContextProvider = (IPersistedCheckoutContextProvider)System.Web.Mvc.DependencyResolver.Current.GetService(typeof(IPersistedCheckoutContextProvider));
			var persistedCheckoutContext = persistedCheckoutContextProvider.LoadCheckoutContext(customer);
			if(persistedCheckoutContext.SelectedShippingMethodId == null)
				return 0;

			var cachedShippingMethodsProvider = (ICachedShippingMethodCollectionProvider)System.Web.Mvc.DependencyResolver.Current.GetService(typeof(ICachedShippingMethodCollectionProvider));
			var shippingMethods = cachedShippingMethodsProvider.Get(customer, address, cartItems, AppLogic.StoreID());
			
			var shippingCost = shippingMethods
				.Where(shippingMethod => shippingMethod.Id == persistedCheckoutContext.SelectedShippingMethodId.Value)
				.Select(shippingMethod => shippingMethod.Freight)
				.FirstOrDefault();

			var vatOn = AppLogic.AppConfigBool("VAT.Enabled")
				&& customer.VATSettingReconciled == VATSettingEnum.ShowPricesInclusiveOfVAT;

			if(vatOn && includeTax)
				shippingCost += ShippingTax(shippingCost, address, customer);

			// Add COD fee for non-zero shipping cost COD orders
			if(shippingCost > 0 && AppLogic.CleanPaymentMethod(customer.RequestedPaymentMethod) == AppLogic.ro_PMCOD)
				shippingCost += AppLogic.AppConfigNativeDecimal("CODHandlingExtraFee");

			// Never return a negative amount
			return Math.Max(shippingCost, 0);
		}

		#endregion

		#region Total

		public static Decimal Total(bool includeDiscount, CartItemCollection cic, Customer ThisCustomer, IEnumerable<OrderOption> OrderOptions, Boolean IncludeTax, Boolean roundBeforeTotaling = true)
		{
			Decimal orderTotal = System.Decimal.Zero;

			bool VATEnabled = AppLogic.AppConfigBool("VAT.Enabled");
			bool VATOn = (VATEnabled && ThisCustomer.VATSettingReconciled == VATSettingEnum.ShowPricesInclusiveOfVAT);
			Decimal sTotal = SubTotal(includeDiscount, false, true, true, true, false, 0, false, cic, ThisCustomer, OrderOptions);
			Decimal shTotal = ShippingTotal(includeDiscount, VATOn, cic, ThisCustomer, OrderOptions);
			Decimal tTotal = 0;

			if(IncludeTax)
				tTotal = Prices.TaxTotal(ThisCustomer, cic, shTotal, OrderOptions);

			RemoveTaxRelatedCartItems(cic);

			// Promotions: Line Item and Shipping discounts happen in the individual calculations so all that's left is to apply order level discounts.
			var orderDiscount = 0.00M;
			if(includeDiscount && cic.HasDiscountResults)
			{
				orderDiscount = cic.DiscountResults.Sum(dr => dr.OrderTotal);

				if (roundBeforeTotaling)
					orderDiscount = Math.Round(orderDiscount, 2, MidpointRounding.AwayFromZero);
			}

			orderTotal = sTotal + orderDiscount;

			// Promotions: Because multiple promotions can be applied, it's possible to get a negative value, which should be caught and zeroed out.
			if(orderTotal < 0)
				orderTotal = 0;

			// Shipping and Tax can never be discounted so it's added after discounts.
			orderTotal += roundBeforeTotaling
				? Math.Round(shTotal, 2, MidpointRounding.AwayFromZero)
				: shTotal;

			orderTotal += roundBeforeTotaling
				? Math.Round(tTotal, 2, MidpointRounding.AwayFromZero)
				: tTotal;

			return orderTotal;
		}

		#endregion

		#region Customer Level Discount

		/// <summary>
		/// Calculates a customer level discount for an order
		/// </summary>
		/// <returns>Decimal total after discount</returns>
		public static Decimal GetOrderCustomerLevelDiscount(Decimal st, Customer ThisCustomer)
		{
			using(var conn = new SqlConnection(DB.GetDBConn()))
			{
				conn.Open();

				using(var rs = DB.GetRS("select LevelDiscountAmount from dbo.CustomerLevel with(NOLOCK) where CustomerLevelID = @CustomerLevelId", new SqlParameter[] { new SqlParameter("CustomerLevelId", ThisCustomer.CustomerLevelID) }, conn))
				{
					if(rs.Read())
					{
						st = st - DB.RSFieldDecimal(rs, "LevelDiscountAmount");
					}
				}
			}

			return st;
		}

		#endregion

		#region GetQuantityDiscount

		/// <summary>
		/// 
		/// </summary>
		/// <param name="productId"></param>
		/// <param name="quantity"></param>
		/// <param name="priceRate"></param>
		/// <param name="fixedPriceDID"></param>
		/// <param name="discountPercent"></param>
		/// <param name="ThisCustomer"></param>
		/// <returns></returns>
		public static decimal GetQuantityDiscount(int productId, int quantity, Decimal priceRate, QuantityDiscount.QuantityDiscountType fixedPriceDID, Decimal discountPercent, Customer ThisCustomer)
		{
			Decimal DIDPercent = 0.0M;
			Decimal DiscountedItemPrice = priceRate;


			if(fixedPriceDID != QuantityDiscount.QuantityDiscountType.None)
			{
				// Make sure customer isn't in a customer level that disallows quantity discounts
				if(QuantityDiscount.CustomerLevelAllowsQuantityDiscounts(ThisCustomer.CustomerLevelID))
				{
					if(DIDPercent != 0.0M)
					{
						if(fixedPriceDID == QuantityDiscount.QuantityDiscountType.FixedAmount)
						{
							if(Currency.GetDefaultCurrency() == ThisCustomer.CurrencySetting)
							{
								DiscountedItemPrice = (priceRate - DIDPercent);
							}
						}
						else
						{
							DiscountedItemPrice = priceRate * ((100.0M - DIDPercent) / 100.0M);
						}
					}
				}
			}

			return DiscountedItemPrice;
			//if (fixedPriceDID != QuantityDiscount.QuantityDiscountType.None)
			//{
			//    if (fixedPriceDID == QuantityDiscount.QuantityDiscountType.FixedAmount)
			//    {
			//        string storeCurrency = Localization.StoreCurrency();
			//        bool customerCurrencyIsDefault = storeCurrency == ThisCustomer.CurrencySetting;
			//        if (customerCurrencyIsDefault)
			//        {
			//            priceRate = priceRate - discountPercent;
			//        }
			//        else
			//        {
			//            discountPercent = Currency.Convert(discountPercent, storeCurrency, ThisCustomer.CurrencySetting);
			//            // round
			//            discountPercent = Decimal.Round(discountPercent, 2, MidpointRounding.AwayFromZero);

			//            priceRate = ((priceRate - discountPercent) * (decimal)quantity);
			//        }
			//    }
			//    else
			//    {
			//        priceRate = ((100.0M - discountPercent) / 100.0M) * priceRate;
			//    }
			//}

			//return priceRate;
		}

		/// <summary>
		/// Applies a quantity discount to a line item total
		/// </summary>
		/// <param name="ci"></param>
		/// <param name="ThisCustomer"></param>
		/// <returns>Decimal total after discount</returns>
		public static Decimal GetQuantityDiscount(CartItem ci, Customer ThisCustomer)
		{
			Decimal DIDPercent = 0.0M;
			Decimal DiscountedItemPrice = ci.Price;
			QuantityDiscount.QuantityDiscountType fixedPriceDID = QuantityDiscount.QuantityDiscountType.Percentage;

			// Make sure customer isn't in a customer level that disallows quantity discounts
			if(QuantityDiscount.CustomerLevelAllowsQuantityDiscounts(ThisCustomer.CustomerLevelID))
			{
				DIDPercent = QuantityDiscount.GetQuantityDiscountTablePercentageForLineItem(ci, out fixedPriceDID);
				if(DIDPercent != 0.0M)
				{
					if(fixedPriceDID == QuantityDiscount.QuantityDiscountType.FixedAmount)
					{
						if(Currency.GetDefaultCurrency() == ThisCustomer.CurrencySetting)
						{
							DiscountedItemPrice = (ci.Price - DIDPercent);
						}
					}
					else
					{
						DiscountedItemPrice = ci.Price * ((100.0M - DIDPercent) / 100.0M);
					}
				}
			}

			// A failsafe check to insure that a flat rate discount never discounts an item into the negative.
			if(DiscountedItemPrice < 0)
				DiscountedItemPrice = 0;

			return DiscountedItemPrice;
		}

		#endregion

		#region Product Pricing

		/// <summary>
		/// Calculates the price, with discounts if they exist, for upsell items
		/// </summary>
		/// <param name="SourceProductID">The ProductID of the product that has the upsell product</param>
		/// <param name="UpsellProductID">The ProductID of the upsell product</param>
		/// <param name="CustomerLevelID">The CustomerLevelID of the current customer</param>
		/// <returns>Decimal price, rounded to 2 decimal places</returns>
		static public Decimal GetUpsellProductPrice(int SourceProductID, int UpsellProductID, int CustomerLevelID)
		{
			Decimal UpsellProductDiscountPercentage = 0.0M;

			if(SourceProductID == 0)
			{
				string sql = "select top 1 UpsellProductDiscountPercentage N From dbo.product with(NOLOCK) where deleted = 0 and published = 1 and charindex(','+@UpsellProductID+',', ','+convert(nvarchar(4000), upsellproducts)+',') > 0 order by productid";
				SqlParameter[] spa = { DB.CreateSQLParameter("@UpsellProductID", SqlDbType.VarChar, 10, UpsellProductID.ToString(), ParameterDirection.Input) };
				UpsellProductDiscountPercentage = DB.GetSqlNDecimal(sql, spa);
			}
			else
			{
				using(var con = new SqlConnection(DB.GetDBConn()))
				{
					con.Open();
					using(var rs = DB.GetRS("Select UpsellProducts,UpsellProductDiscountPercentage from dbo.Product with (NOLOCK) where ProductID=" + SourceProductID.ToString(), con))
					{
						if(rs.Read())
						{
							UpsellProductDiscountPercentage = DB.RSFieldDecimal(rs, "UpsellProductDiscountPercentage");
						}
					}
				}
			}
			bool IsOnSale = true; // don't care really
			Decimal PR = AppLogic.DetermineLevelPrice(AppLogic.GetProductsDefaultVariantID(UpsellProductID), CustomerLevelID, out IsOnSale);
			if(UpsellProductDiscountPercentage != 0.0M)
			{
				PR = PR * (Decimal)(1 - (UpsellProductDiscountPercentage / 100.0M));
			}

			// return the price, rounded to 2 decimal places
			return PR;
			//return PR;
		}

		/// <summary>
		/// Used for backward compatibility with AppLogic.DetermineLevelPrice
		/// </summary>
		/// <param name="VariantID"></param>
		/// <param name="CustomerLevelID"></param>
		/// <param name="IsOnSale"></param>
		/// <returns></returns>
		static public decimal DetermineLevelPrice(int VariantID, int CustomerLevelID, out bool IsOnSale)
		{
			// the way the site is written, this should NOT be called with CustomerLevelID=0 but, you never know
			// if that's the case, return the sale price if any, and if not, the regular price instead:
			decimal pr = System.Decimal.Zero;
			IsOnSale = false;
			if(CustomerLevelID == 0)
			{
				using(var con = new SqlConnection(DB.GetDBConn()))
				{
					con.Open();
					using(var rs = DB.GetRS(String.Format("select Price, SalePrice from productvariant with (NOLOCK) where VariantID = {0}", VariantID), con))
					{
						if(rs.Read())
						{
							if(DB.RSFieldDecimal(rs, "SalePrice") != System.Decimal.Zero)
							{
								pr = DB.RSFieldDecimal(rs, "SalePrice");
								IsOnSale = true;
							}
							else
							{
								pr = DB.RSFieldDecimal(rs, "Price");
							}
						}
						else
						{
							// well, this is bad, we can't return 0, and we don't have ANY valid price to return...stop the web page!
							throw (new ApplicationException("Invalid Variant Price Structure, VariantID=" + VariantID.ToString()));
						}
					}
				}
			}
			else
			{
				// ok, now for the hard part (e.g. the fun)
				// determine the actual price for this thing, considering everything involved!
				// If we have an extended price, get that first!
				var ExtendedPriceFound = false;

				using(var con = new SqlConnection(DB.GetDBConn()))
				{
					con.Open();
					using(var rs = DB.GetRS("select Price from ExtendedPrice  with (NOLOCK)  where VariantID=" + VariantID.ToString() + " and CustomerLevelID=" + CustomerLevelID.ToString() + " and VariantID in (select VariantID from ProductVariant where ProductID in (select ProductID from ProductCustomerLevel where CustomerLevelID=" + CustomerLevelID.ToString() + "))", con))
					{
						if(rs.Read())
						{
							pr = DB.RSFieldDecimal(rs, "Price");
							ExtendedPriceFound = true;
						}
					}
				}

				if(!ExtendedPriceFound)
				{
					pr = GetVariantPrice(VariantID);
				}

				// now get the "level" info:
				using(var con = new SqlConnection(DB.GetDBConn()))
				{
					con.Open();
					using(var rs = DB.GetRS("select * from CustomerLevel   with (NOLOCK)  where CustomerLevelID=" + CustomerLevelID.ToString(), con))
					{
						if(rs.Read())
						{
							var DiscountPercent = DB.RSFieldDecimal(rs, "LevelDiscountPercent");
							var LevelDiscountsApplyToExtendedPrices = DB.RSFieldBool(rs, "LevelDiscountsApplyToExtendedPrices");
							rs.Close();
							if(DiscountPercent != 0.0M)
							{
								if(!ExtendedPriceFound || (ExtendedPriceFound && LevelDiscountsApplyToExtendedPrices))
								{
									pr = pr * (decimal)(1.00M - (DiscountPercent / 100.0M));
								}
							}
						}
					}
				}
			}

			// WEBOPIUS, was 2
			// return the price, rounded to 4 decimal places
			return Decimal.Round(pr, 4, MidpointRounding.AwayFromZero);
		}

		/// <summary>
		/// Used for backward compatibility with AppLogic.GetVariantPrice
		/// </summary>
		/// <param name="VariantID"></param>
		/// <returns></returns>
		static public decimal GetVariantPrice(int VariantID)
		{
			var pr = Decimal.Zero;

			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(var rs = DB.GetRS("select Price from productvariant   with (NOLOCK)  where VariantID=" + VariantID.ToString(), con))
				{
					if(rs.Read())
					{
						pr = DB.RSFieldDecimal(rs, "Price");
					}
				}
			}

			return pr;
		}

		/// <summary>
		/// Used for backward compatibility with AppLogic.GetKitTotalPrice
		/// </summary>
		/// <param name="CustomerID"></param>
		/// <param name="CustomerLevelID"></param>
		/// <param name="ProductID"></param>
		/// <param name="VariantID"></param>
		/// <param name="ShoppingCartRecID"></param>
		/// <returns></returns>
		static public decimal GetKitTotalPrice(int CustomerID, int CustomerLevelID, int ProductID, int VariantID, int ShoppingCartRecID)
		{
			var ThisCustomer = HttpContext.Current.GetCustomer();
			var tmp = Decimal.Zero;

			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(var rs = DB.GetRS("SELECT Product.*, ProductVariant.Price, ProductVariant.SalePrice FROM Product   with (NOLOCK)  inner join productvariant   with (NOLOCK)  on product.productid=productvariant.productid where ProductVariant.VariantID=" + VariantID.ToString(), con))
				{
					if(rs.Read())
					{
						var BasePrice = Decimal.Zero;
						if(DB.RSFieldDecimal(rs, "SalePrice") != System.Decimal.Zero && CustomerLevelID == 0)
						{
							BasePrice = DB.RSFieldDecimal(rs, "SalePrice");
						}
						else
						{
							bool isonsale = false; // not used
							BasePrice = AppLogic.DetermineLevelPrice(VariantID, CustomerLevelID, out isonsale);
						}
						var KitPriceDelta = AppLogic.KitPriceDelta(CustomerID, ProductID, ShoppingCartRecID);
						tmp = BasePrice + (KitPriceDelta * (1 - (ThisCustomer.LevelDiscountPct / 100)));
					}
				}
			}

			return tmp;
		}

		/// <summary>
		/// Used for backward compatibility with AppLogic.KitPriceDelta
		/// </summary>
		/// <param name="CustomerID"></param>
		/// <param name="ProductID"></param>
		/// <param name="ShoppingCartRecID"></param>
		/// <param name="ForCurrency"></param>
		/// <returns></returns>
		static public decimal KitPriceDelta(int CustomerID, int ProductID, int ShoppingCartRecID, string ForCurrency)
		{
			var tmp = Decimal.Zero;
			if(CustomerID != 0)
			{
				var sql = string.Empty;
				if(ForCurrency == Localization.StoreCurrency())
				{
					sql = "select sum(pricedelta) as PR from kitcart   with (NOLOCK)  inner join kititem   with (NOLOCK)  on kitcart.kititemid=kitItem.kititemid where customerid=" + CustomerID.ToString() + " and productid=" + ProductID.ToString() + " and ShoppingCartrecid=" + ShoppingCartRecID.ToString();
				}
				else
				{
					sql = "select sum(round(pricedelta*dbo.ExchangeRate(" + DB.SQuote(ForCurrency) + "), 2)) as PR from kitcart   with (NOLOCK)  inner join kititem   with (NOLOCK)  on kitcart.kititemid=kitItem.kititemid where customerid=" + CustomerID.ToString() + " and productid=" + ProductID.ToString() + " and ShoppingCartrecid=" + ShoppingCartRecID.ToString();
				}

				using(var con = new SqlConnection(DB.GetDBConn()))
				{
					con.Open();
					using(var rs = DB.GetRS(sql, con))
					{
						if(rs.Read())
						{
							tmp = DB.RSFieldDecimal(rs, "PR");
						}
					}
				}
			}
			return tmp;
		}

		/// <summary>
		/// Used for backward compatibility with AppLogic.GetVariantSalePrice
		/// </summary>
		/// <param name="VariantID"></param>
		/// <returns></returns>
		static public decimal GetVariantSalePrice(int VariantID)
		{
			var pr = Decimal.Zero;

			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(var rs = DB.GetRS("select SalePrice from productvariant   with (NOLOCK)  where VariantID=" + VariantID.ToString(), con))
				{
					if(rs.Read())
					{
						pr = DB.RSFieldDecimal(rs, "SalePrice");
					}
				}
			}

			return pr;
		}

		/// <summary>
		/// Used for backwards compatibility with XSLTExtensionBase.GetUpsellVariantPrice
		/// </summary>
		/// <param name="ThisCustomer"></param>
		/// <param name="sVariantID"></param>
		/// <param name="sHidePriceUntilCart"></param>
		/// <param name="sPrice"></param>
		/// <param name="sSalePrice"></param>
		/// <param name="sExtPrice"></param>
		/// <param name="sPoints"></param>
		/// <param name="sSalesPromptName"></param>
		/// <param name="sShowpricelabel"></param>
		/// <param name="sTaxClassID"></param>
		/// <param name="decUpSelldiscountPct"></param>
		/// <returns></returns>
		public static string GetUpsellVariantPrice(Customer ThisCustomer, String sVariantID, String sHidePriceUntilCart, string sPrice, string sSalePrice, string sExtPrice, String sPoints, string sSalesPromptName, String sShowpricelabel, string sTaxClassID, string decUpSelldiscountPct)
		{
			return GetUpsellVariantPrice(ThisCustomer, sVariantID, sHidePriceUntilCart, sPrice, sSalePrice, sExtPrice, sPoints, sSalesPromptName, sShowpricelabel, sTaxClassID, decUpSelldiscountPct, "0");
		}
		public static string GetUpsellVariantPrice(Customer ThisCustomer, String sVariantID, String sHidePriceUntilCart, string sPrice, string sSalePrice, string sExtPrice, String sPoints, string sSalesPromptName, String sShowpricelabel, string sTaxClassID, string decUpSelldiscountPct, string sProductId)
		{
			var inputValidator = new InputValidator("GetVariantPrice");
			var hidePriceUntilCart = inputValidator.ValidateBool("HidePriceUntilCart", sHidePriceUntilCart);
			var price = inputValidator.ValidateDecimal("Price", sPrice);
			var salePrice = inputValidator.ValidateDecimal("SalePrice", sSalePrice);
			var extPrice = inputValidator.ValidateDecimal("ExtPrice", sExtPrice);
			var taxClassId = inputValidator.ValidateInt("TaxClassID", sTaxClassID);
			var upsellDiscountPercentage = inputValidator.ValidateDecimal("UpSelldiscountPct", decUpSelldiscountPct);

			var results = new StringBuilder(1024);

			if(hidePriceUntilCart
				|| AppLogic.HideForWholesaleSite(ThisCustomer.CustomerLevelID))
				return string.Empty;

			var upsellPrice = GetUpsellPrice(ThisCustomer, price, salePrice, extPrice, taxClassId, upsellDiscountPercentage);

			if(upsellPrice <= 0)
				return string.Empty;

			var storeDefaultCultureInfo = CultureInfo.GetCultureInfo(Localization.GetDefaultLocale());
			var formattedSchemaPrice = string.Format(storeDefaultCultureInfo, "{0:C}", upsellPrice);
			var schemaRegionInfo = new RegionInfo(storeDefaultCultureInfo.Name);
			var xsltExtensionBase = new XSLTExtensionBase(ThisCustomer, AppLogic.GetStoreSkinID(AppLogic.StoreID()));

			results.AppendFormat("<span itemprop=\"offers\" itemscope itemtype=\"{0}://schema.org/Offer\">{1}", HttpContext.Current.Request.Url.Scheme, Environment.NewLine);

			var productId = 0;
			var variantId = 0;
			if(int.TryParse(sProductId, out productId) 
				&& int.TryParse(sVariantID, out variantId) 
				&& productId > 0 
				&& variantId > 0)
			{
				string stockStatusText = xsltExtensionBase.GetStockStatusText(sProductId, sVariantID, "Product");
				if(stockStatusText.Length > 0)
				{
					results.AppendFormat("<link itemprop=\"availability\" href=\"{0}://schema.org/{1}\"/>{2}", HttpContext.Current.Request.Url.Scheme, stockStatusText.Split('|')[0], Environment.NewLine);
				}
			}

			results.AppendFormat("<meta itemprop=\"price\" content=\"{0}\"/>{1}", formattedSchemaPrice, Environment.NewLine);
			results.AppendFormat("<meta itemprop=\"priceCurrency\" content=\"{0}\"/>{1}", schemaRegionInfo.ISOCurrencySymbol, Environment.NewLine);
			results.Append("</span>");

			results.Append(Localization.CurrencyStringForDisplayWithExchangeRate(upsellPrice, ThisCustomer.CurrencySetting));

			return results.ToString();
		}

		public static decimal GetUpsellPrice(Customer customer, decimal price, decimal salePrice, decimal extendedPrice, int taxClassId, decimal upsellDiscountPercent)
		{
			var vatEnabled = AppLogic.AppConfigBool("VAT.Enabled") && customer.VATSettingReconciled == VATSettingEnum.ShowPricesInclusiveOfVAT;

			var vatPrice = price;
			var vatSalePrice = salePrice;

			var taxRate = TaxRate(customer, taxClassId);
			var taxMultiplier = 1.0M + (taxRate / 100.00M);
			if(vatEnabled)
			{
				vatPrice = taxMultiplier * price;
				vatSalePrice = taxMultiplier * salePrice;
			}

			decimal variantPrice;
			
			if(customer.CustomerLevelID == 0 || (customer.LevelDiscountPct == 0.0M && extendedPrice == 0.0M))
			{
				// show consumer pricing (e.g. level 0):
				variantPrice = vatPrice;
				if(salePrice != decimal.Zero)
					variantPrice = vatSalePrice;
			}
			else
			{
				variantPrice = extendedPrice == 0.0M 
					? price 
					: extendedPrice;

				if(customer.LevelDiscountPct != 0.0M && (extendedPrice == 0.0M || (extendedPrice > 0.0M && customer.DiscountExtendedPrices)))
					variantPrice = variantPrice * (1.00M - (customer.LevelDiscountPct / 100.0M)) * (vatEnabled ? taxMultiplier : 1.0M);
			}
			
			return upsellDiscountPercent * variantPrice;
		}


		/// <summary>
		/// Used for backwards compatibility with XSLTExtensionBase.GetCartPrice
		/// </summary>
		/// <param name="ThisCustomer"></param>
		/// <param name="intProductID"></param>
		/// <param name="intQuantity"></param>
		/// <param name="decProductPrice"></param>
		/// <param name="intTaxClassID"></param>
		/// <returns></returns>
		[Obsolete("deprecated (10.0) no longer used")]
		public static string GetCartPrice(Customer ThisCustomer, string intProductID, string intQuantity, string decProductPrice, string intTaxClassID)
		{
			InputValidator IV = new InputValidator("GetCartPrice");
			int ProductID = IV.ValidateInt("ProductID", intProductID);
			int TaxClassID = IV.ValidateInt("TaxClassID", intTaxClassID);
			int Quantity = IV.ValidateInt("Quantity", intQuantity);
			Decimal Price = IV.ValidateDecimal("Price", decProductPrice);

			if(Currency.GetDefaultCurrency() != ThisCustomer.CurrencySetting)
			{
				Price = Decimal.Round(Currency.Convert(Price, Localization.StoreCurrency(), ThisCustomer.CurrencySetting), 2, MidpointRounding.AwayFromZero);
			}

			decimal TaxRate = 0.0M;
			bool VATEnabled = AppLogic.AppConfigBool("VAT.Enabled");
			bool VATOn = (VATEnabled && ThisCustomer.VATSettingReconciled == VATSettingEnum.ShowPricesInclusiveOfVAT);
			if(VATEnabled)
			{
				TaxRate = Prices.TaxRate(ThisCustomer, TaxClassID);
			}

			int ProductQty = DB.GetSqlN("select sum(quantity) N from shoppingcart where carttype = 0 and customerid = " + ThisCustomer.CustomerID.ToString() + " and productid = " + intProductID);

			int Q = Quantity;
			decimal PR = Price * (decimal)Q;
			Decimal DIDPercent = 0.0M;
			QuantityDiscount.QuantityDiscountType fixedPriceDID = QuantityDiscount.QuantityDiscountType.Percentage;
			if(QuantityDiscount.CustomerLevelAllowsQuantityDiscounts(ThisCustomer.CustomerLevelID))
			{
				DIDPercent = QuantityDiscount.GetQuantityDiscountTablePercentageWithoutCartAwareness(ThisCustomer.CustomerID, ProductID, ProductQty, out fixedPriceDID);
				if(DIDPercent != 0.0M)
				{
					if(fixedPriceDID == QuantityDiscount.QuantityDiscountType.FixedAmount)
					{
						if(Currency.GetDefaultCurrency() == ThisCustomer.CurrencySetting)
						{
							PR = (Price - DIDPercent) * (Decimal)Q;
						}
						else
						{
							DIDPercent = Decimal.Round(Currency.Convert(DIDPercent, Localization.StoreCurrency(), ThisCustomer.CurrencySetting), 2, MidpointRounding.AwayFromZero);
							PR = (Price - DIDPercent) * (Decimal)Q;
						}
					}
					else
					{
						PR = PR * ((100.0M - DIDPercent) / 100.0M);
					}
				}
			}
			decimal VAT = PR * (TaxRate / 100.0M);
			if(VATOn)
			{
				PR += VAT;
			}
			StringBuilder results = new StringBuilder();
			results.Append(Localization.CurrencyStringForDisplayWithoutExchangeRate(PR, ThisCustomer.CurrencySetting));
			if(VATEnabled)
			{
				results.Append("&nbsp;");
				if(VATOn)
				{
					results.Append("inc vat");
				}
				else
				{
					results.Append("Excluding VAT");
				}
			}

			return results.ToString();
		}

		/// <summary>
		/// Used for backwards compatibility with AppLogic.GetColorAndSizePriceDelta
		/// </summary>
		/// <param name="ChosenColor"></param>
		/// <param name="ChosenSize"></param>
		/// <param name="TaxClassID"></param>
		/// <param name="ThisCustomer"></param>
		/// <param name="WithDiscount"></param>
		/// <param name="WithVAT"></param>
		/// <returns></returns>
		static public decimal GetColorAndSizePriceDelta(string ChosenColor, string ChosenSize, int TaxClassID, Customer ThisCustomer, bool WithDiscount, bool WithVAT)
		{
			var VATEnabled = AppLogic.AppConfigBool("VAT.Enabled");
			var VATOn = (VATEnabled && ThisCustomer.VATSettingReconciled == VATSettingEnum.ShowPricesInclusiveOfVAT);

			var CustLevelDiscountPct = 1.0M;
			var price = Decimal.Zero;
			var ColorPriceModifier = string.Empty;
			var SizePriceModifier = string.Empty;
			if(ThisCustomer.CustomerLevelID > 0 && WithDiscount)
			{
				var LevelDiscountPercent = Decimal.Zero;
				using(var dbconn = new SqlConnection(DB.GetDBConn()))
				{
					dbconn.Open();
					var sSql = string.Format("select LevelDiscountPercent from CustomerLevel with (NOLOCK) where CustomerLevelID={0}", ThisCustomer.CustomerLevelID);
					using(var rs = DB.GetRS(sSql, dbconn))
					{
						if(rs.Read())
						{
							LevelDiscountPercent = DB.RSFieldDecimal(rs, "LevelDiscountPercent");
						}
					}
				}

				if(LevelDiscountPercent != Decimal.Zero)
				{
					CustLevelDiscountPct -= LevelDiscountPercent / 100.0M;
				}
			}
			if(TaxClassID != AppLogic.AppConfigNativeInt("ShippingTaxClassId") && ChosenColor != null && ChosenSize != null)
			{
				if(ChosenColor.IndexOf("[") != -1)
				{
					int i1 = ChosenColor.IndexOf("[");
					int i2 = ChosenColor.IndexOf("]");
					if(i1 != -1 && i2 != -1)
					{
						ColorPriceModifier = ChosenColor.Substring(i1 + 1, i2 - i1 - 1);
					}
				}
				if(ChosenSize.IndexOf("[") != -1)
				{
					int i1 = ChosenSize.IndexOf("[");
					int i2 = ChosenSize.IndexOf("]");
					if(i1 != -1 && i2 != -1)
					{
						SizePriceModifier = ChosenSize.Substring(i1 + 1, i2 - i1 - 1);
					}
				}

				if(ColorPriceModifier.Length != 0)
				{
					//Modifier is 1.23 -- never 1,23 -- force en-US. Comma format handled elsewhere
					price += Localization.ParseLocaleDecimal(ColorPriceModifier, "en-US");
				}
				if(SizePriceModifier.Length != 0)
				{
					//Modifier is 1.23 -- never 1,23 -- force en-US. Comma format handled elsewhere
					price += Localization.ParseLocaleDecimal(SizePriceModifier, "en-US");
				}
			}
			if(VATOn && WithVAT)
			{
				decimal TaxRate = 0.0M;

				TaxRate = Prices.TaxRate(ThisCustomer, TaxClassID);

				Decimal TaxMultiplier = (1.0M + (TaxRate / 100.00M));
				price = TaxMultiplier * price;
			}
			return price * CustLevelDiscountPct;
		}

		#endregion
	}
}
