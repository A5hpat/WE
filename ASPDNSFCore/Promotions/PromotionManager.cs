// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Xml.Serialization;
using AspDotNetStorefront.Checkout;
using AspDotNetStorefront.Promotions;
using PromotionsData = AspDotNetStorefront.Promotions.Data;

namespace AspDotNetStorefrontCore
{
	public static class PromotionManager
	{
		static PromotionsData.Promotion GetPromotionById(int promotionId)
		{
			return PromotionsData.DataContextProvider
				.Current
				.Promotions
				.FirstOrDefault(p => p.Id == promotionId);
		}

		static PromotionsData.Promotion GetPromotionByCode(string promotionCode, Boolean activeOnly)
		{
			return PromotionsData.DataContextProvider
				.Current
				.Promotions
				.Where(p => !activeOnly || (activeOnly && p.Active))
				.Where(p => p.Code.ToUpper() == promotionCode.ToUpper())
				.FirstOrDefault();
		}

		public static PromotionController CreatePromotionController()
		{
			var promotionController = new PromotionController();
			promotionController.OnLookupData += new PromotionController.LookupDataDelegate(PromotionController_OnLookupData);
			return promotionController;
		}

		static IDataLookupResult PromotionController_OnLookupData(IDataLookupContext dataLookupContext)
		{
			var lookupResult = new SimpleDataLookupResult();

			switch(dataLookupContext.LookupType)
			{
				case LookupType.TotalPromotionUses:
					var promotionUsages = PromotionsData.DataContextProvider
						.Current
						.PromotionUsages
						.Where(pu => pu.Complete);

					if(dataLookupContext.CustomerId > 0)
						promotionUsages = promotionUsages
							.Where(pu => pu.CustomerId == dataLookupContext.CustomerId);

					if(dataLookupContext.PromotionId > 0)
						promotionUsages = promotionUsages
							.Where(pu => pu.PromotionId == dataLookupContext.PromotionId);

					promotionUsages = DateFilterPromotionUsage(dataLookupContext, promotionUsages);
					lookupResult.Int32Result = promotionUsages.Count();
					break;

				case LookupType.TotalOrders:
					var totalOrders = (IQueryable<PromotionsData.Order>)PromotionsData.DataContextProvider
						.Current
						.Orders;

					if(dataLookupContext.CustomerId > 0)
						totalOrders = totalOrders
							.Where(o => o.CustomerID == dataLookupContext.CustomerId);

					totalOrders = DateFilterOrders(dataLookupContext, totalOrders);
					lookupResult.Int32Result = totalOrders.Count();
					break;

				case LookupType.TotalOrderAmount:
					var totalOrderAmount = (IQueryable<PromotionsData.Order>)PromotionsData.DataContextProvider.Current.Orders;
					if(dataLookupContext.CustomerId > 0)
						totalOrderAmount = totalOrderAmount
							.Where(o => o.CustomerID == dataLookupContext.CustomerId);

					totalOrderAmount = DateFilterOrders(dataLookupContext, totalOrderAmount);
					lookupResult.DecimalResult = totalOrderAmount.Any()
						? totalOrderAmount.Sum(o => o.OrderTotal)
						: 0.00M;
					break;

				case LookupType.TotalProductOrdered:
					var totalProducts = (IQueryable<PromotionsData.Orders_ShoppingCart>)PromotionsData.DataContextProvider
						.Current
						.Orders_ShoppingCarts
						.Where(w => !dataLookupContext.ProductIds.Any() || dataLookupContext.ProductIds.Contains(w.ProductID));

					if(dataLookupContext.CustomerId > 0)
						totalProducts = totalProducts
							.Where(os => os.CustomerID == dataLookupContext.CustomerId);

					totalProducts = DateFilterOrders_ShoppingCart(dataLookupContext, totalProducts);

					lookupResult.Int32Result = totalProducts.Any()
						? totalProducts.Sum(s => s.Quantity)
						: 0;

					lookupResult.StringResult = String.Join(", ", PromotionsData.DataContextProvider
						.Current
						.Products
						.Where(p => dataLookupContext.ProductIds.Contains(p.ProductID))
						.Select(p => p.Name).ToArray());
					break;

				case LookupType.TotalProductOrderedAmount:
					var totalProductAmount = (IQueryable<PromotionsData.Orders_ShoppingCart>)PromotionsData.DataContextProvider
						.Current
						.Orders_ShoppingCarts
						.Where(w => !dataLookupContext.ProductIds.Any() || dataLookupContext.ProductIds.Contains(w.ProductID));

					if(dataLookupContext.CustomerId > 0)
						totalProductAmount = totalProductAmount.Where(os => os.CustomerID == dataLookupContext.CustomerId);

					totalProductAmount = DateFilterOrders_ShoppingCart(dataLookupContext, totalProductAmount)
						.Where(tpa => tpa.OrderedProductPrice != null);

					lookupResult.DecimalResult = totalProductAmount.Any()
						? (decimal)totalProductAmount.Sum(os => os.OrderedProductPrice)
						: 0.00M;

					lookupResult.StringResult = String.Join(", ", PromotionsData.DataContextProvider
						.Current
						.Products
						.Where(p => dataLookupContext.ProductIds.Contains(p.ProductID)).Select(p => p.Name)
						.ToArray());
					break;

				case LookupType.LastPromotionUsage:
					var lastPromotionUsages = (IQueryable<PromotionsData.PromotionUsage>)PromotionsData.DataContextProvider
						.Current
						.PromotionUsages;

					if(dataLookupContext.CustomerId > 0)
						lastPromotionUsages = lastPromotionUsages
							.Where(pu => pu.CustomerId == dataLookupContext.CustomerId);

					if(dataLookupContext.PromotionId > 0)
						lastPromotionUsages = lastPromotionUsages
							.Where(pu => pu.PromotionId == dataLookupContext.PromotionId);

					lastPromotionUsages = DateFilterPromotionUsage(dataLookupContext, lastPromotionUsages);

					var lastPromotionUsage = lastPromotionUsages
						.OrderByDescending(pu => pu.DateApplied)
						.FirstOrDefault();

					if(lastPromotionUsage != null)
						lookupResult.DateTimeResult = lastPromotionUsage.DateApplied.Value;
					else
						lookupResult.DateTimeResult = DateTime.MinValue;
					break;
			}

			return lookupResult;
		}

		static IQueryable<PromotionsData.PromotionUsage> DateFilterPromotionUsage(IDataLookupContext dataLookupContext, IQueryable<PromotionsData.PromotionUsage> promotionUsages)
		{
			var retVal = promotionUsages
				.Where(pu => pu.DateApplied.HasValue);

			var startDate = CalculateQueryDate(dataLookupContext.StartDateType, dataLookupContext.CustomStartDate, dataLookupContext.PromotionId, dataLookupContext.CustomerId);
			var endDate = CalculateQueryDate(dataLookupContext.EndDateType, dataLookupContext.CustomEndDate, dataLookupContext.PromotionId, dataLookupContext.CustomerId);

			if(startDate != DateTime.MinValue && endDate != DateTime.MinValue)
				retVal = retVal.Where(pu => startDate <= pu.DateApplied.Value && pu.DateApplied.Value <= endDate);

			return retVal;
		}

		static IQueryable<PromotionsData.Order> DateFilterOrders(IDataLookupContext dataLookupContext, IQueryable<PromotionsData.Order> orders)
		{
			var retVal = orders;
			var startDate = CalculateQueryDate(dataLookupContext.StartDateType, dataLookupContext.CustomStartDate, dataLookupContext.PromotionId, dataLookupContext.CustomerId);
			var endDate = CalculateQueryDate(dataLookupContext.EndDateType, dataLookupContext.CustomEndDate, dataLookupContext.PromotionId, dataLookupContext.CustomerId);

			if(startDate != DateTime.MinValue && endDate != DateTime.MinValue)
				retVal = retVal.Where(o => startDate <= o.OrderDate && o.OrderDate <= endDate);

			return retVal;
		}

		static IQueryable<PromotionsData.Orders_ShoppingCart> DateFilterOrders_ShoppingCart(IDataLookupContext dataLookupContext, IQueryable<PromotionsData.Orders_ShoppingCart> orders_ShoppingCart)
		{
			var retVal = orders_ShoppingCart;
			var startDate = CalculateQueryDate(dataLookupContext.StartDateType, dataLookupContext.CustomStartDate, dataLookupContext.PromotionId, dataLookupContext.CustomerId);
			var endDate = CalculateQueryDate(dataLookupContext.EndDateType, dataLookupContext.CustomEndDate, dataLookupContext.PromotionId, dataLookupContext.CustomerId);

			if(startDate != DateTime.MinValue && endDate != DateTime.MinValue)
				retVal = retVal.Where(o => startDate <= o.CreatedOn && o.CreatedOn <= endDate);

			return retVal;
		}

		static DateTime CalculateQueryDate(DateType dateType, DateTime customDate, int promotionId, int customerId)
		{
			switch(dateType)
			{
				case DateType.CustomDate:
					return customDate;

				case DateType.LastPromotionUsage:
					return PromotionController_OnLookupData(new SimpleDataLookupContext
						{
							CustomerId = customerId,
							PromotionId = promotionId,
							LookupType = LookupType.LastPromotionUsage,
						}).DateTimeResult;

				case DateType.CurrentDate:
					return DateTime.Now;

				default:
				case DateType.Unspecified:
					return DateTime.MinValue;
			}
		}

		public static IDiscountContext CreateDiscountContext(IDiscountContext discountContext, IEnumerable<DiscountableItem> discountableItems)
		{
			return new SimpleDiscountContext(discountContext)
				{
					DiscountableItems = discountableItems,
				};
		}

		public static IDiscountContext CreateDiscountContext(IRuleContext ruleContext, IEnumerable<DiscountableItem> discountableItems = null)
		{
			return new SimpleDiscountContext
				{
					LineItemTotal = ruleContext.SubTotal,
					ShippingTotal = ruleContext.ShippingTotal,
					ShippingTaxTotal = 0,
					OrderTaxTotal = 0,
					OrderTotal = ruleContext.SubTotal,
					DiscountableItems = discountableItems,
					BillingAddressId = ruleContext.BillingAddressId,
					CustomerId = ruleContext.CustomerId,
					CustomerLevelId = ruleContext.CustomerLevel,
					ShippingAddressId = ruleContext.ShippingAddressId,
					StoreId = ruleContext.StoreId
				};
		}

		public static IEnumerable<IPromotionValidationResult> ValidatePromotion(string promotionCode, IRuleContext ruleContext)
		{
			var promotion = GetPromotionByCode(promotionCode, false);
			if(promotion == null)
				return new[]
					{
						new SimplePromotionValidationResult(false, "Promotion.Reason.DoesNotExist")
					};
			else if(!promotion.Active)
				return new[]
					{
						new SimplePromotionValidationResult(false, "Promotion.Reason.InactivePromotion")
					};

			ruleContext.PromotionId = promotion.Id;

			return CreatePromotionController()
				.ValidatePromotion(promotion, ruleContext, AppLogic.CustomerLevelAllowsCoupons(ruleContext.CustomerLevel));
		}

		public static void AssignPromotion(int customerId, string promotionCode)
		{
			var promotion = GetPromotionByCode(promotionCode, true);
			if(promotion == null)
				return;

			AssignPromotion(customerId, promotion.Id);
		}

		public static void AssignPromotion(int customerId, int promotionId)
		{
			RemoveIgnoredPromotion(customerId, promotionId);

			if(GetPromotionUsagesByCustomer(customerId)
				.Any(pu => pu.PromotionId == promotionId))
				return;

			var context = PromotionsData.DataContextProvider.Current;
			context
				.PromotionUsages
				.InsertOnSubmit(new PromotionsData.PromotionUsage
					{
						PromotionId = promotionId,
						CustomerId = customerId,
					});

			context
				.SubmitChanges();
		}

		public static IQueryable<PromotionsData.PromotionUsage> GetPromotionUsagesByCustomer(int customerId)
		{
			return PromotionsData.DataContextProvider
				.Current
				.PromotionUsages
				.Where(pu => pu.CustomerId == customerId && pu.Complete == false);
		}

		public static IQueryable<PromotionsData.Promotion> GetAssignedPromotions(int customerId)
		{
			return GetPromotionUsagesByCustomer(customerId)
				.Select(pu => GetPromotionById(pu.PromotionId));
		}

		public static IDiscountResult GetPromotionDiscount(IRuleContext ruleContext, out IList<IDiscountResult> discountResults)
		{
			var promotionController = CreatePromotionController();

			discountResults = new List<IDiscountResult>();

			var promotionUsages = GetPromotionUsagesByCustomer(ruleContext.CustomerId);
			var AllPromotionDiscounts = new Dictionary<IPromotionUsage, IPromotionDiscount>();

			//Need to loop all promos and all discount types in those promos so we can build up a list of all the discounts on this order
			foreach(var promotionUsage in promotionUsages)
			{
				foreach(var promoDiscount in promotionUsage.Promotion.PromotionDiscounts)
				{
					//We need to add only one item per promo usage but we want to order by the non-shipping option when it has shipping plus another discount type on one promo
					//this only works becuase we restrict promos to shipping plus one other type of discount.
					if(promotionUsage.Promotion.PromotionDiscounts.Count == 1 || promoDiscount.SequenceNumber != (int)PromotionDiscountBase.PromotionSequence.Shipping)
						AllPromotionDiscounts.Add(promotionUsage, promoDiscount);
				}
			}

			//Sort the discounts, this is incase we need to deal with line item -vs- order level coupon priority
			var sortedPromotionDiscounts = AllPromotionDiscounts
				.ToArray()
				.OrderBy(apd => apd.Key.Id)
				.OrderBy(apd => apd.Value.SequenceNumber);

			var discountContext = CreateDiscountContext(ruleContext);

			foreach(var discountPair in sortedPromotionDiscounts)
			{
				var promotionRuleContext = CreateRuleContext(ruleContext, discountPair.Key.PromotionId);
				var discountableItems = GetDiscountableItems(promotionRuleContext, discountPair.Key.PromotionId);
				var promotionDiscountContext = CreateDiscountContext(discountContext, discountableItems);
				var discountResult = promotionController
					.ApplyPromotion(discountPair.Key, promotionRuleContext, promotionDiscountContext, () => new SimpleDiscountResult(), CreatePromotionController());

				if(discountResult != null)
					discountResults.Add(discountResult);
			}

			return promotionController
				.CombineDiscounts(discountResults, delegate () { return new SimpleDiscountResult(); });
		}

		public static void PrioritizePromotions(IRuleContext ruleContext)
		{
			if(!IsValidCartType(ruleContext))
				return;

			RemoveDuplicatePromotionUsages(ruleContext.CustomerId);

			var assignedPromotions = GetAssignedPromotions(ruleContext.CustomerId)
				.ToList();  // get all assigned promotions

			if(assignedPromotions.Any())
			{
				var priority = 0m;

				if(assignedPromotions.Where(w => w.Priority != 0).Any())
					priority = assignedPromotions.Where(w => w.Priority != 0).Min(m => m.Priority); // get lowest priority

				foreach(var p in assignedPromotions.Where(w => w.Priority == 0 || w.Priority == priority))
				{
					if(PromotionManager.ValidatePromotion(p.Code, ruleContext).All(vr => vr.IsValid))
						PromotionManager.AssignPromotion(ruleContext.CustomerId, p.Code);
					else
						PromotionManager.ClearPromotionUsages(ruleContext.CustomerId, p.Code, false);
				}

				foreach(var p in assignedPromotions.Where(w => w.Priority > priority))
				{
					PromotionManager.ClearPromotionUsages(ruleContext.CustomerId, p.Code, false);
				}
			}
		}

		public static void ClearPromotionUsages(int customerId, string promotionCode, bool removeAutoAssigned)
		{
			var db = PromotionsData.DataContextProvider.Current;

			var promotionUsages = 
				from pu in db.PromotionUsages
				join p in db.Promotions on pu.PromotionId equals p.Id
				join pli in db.PromotionLineItems on pu.Id equals pli.PromotionUsageId into pui
				from pli in pui.DefaultIfEmpty()
				join sc in db.ShoppingCarts on pli.shoppingCartRecordId equals sc.ShoppingCartRecID into sci
				from sc in sci.DefaultIfEmpty()
				where pu.CustomerId == customerId && !pu.Complete

				select new
				{
					promoUsage = pu,
					promo = p,
					promoLineItem = pli,
					scItem = sc
				};

			if(!string.IsNullOrEmpty(promotionCode))
				promotionUsages = promotionUsages
					.Where(puli => puli.promo.Code == promotionCode);

			var giftCarts = promotionUsages
				.Where(puli => puli.promoLineItem.isAGift).Select(puli => puli.scItem).Where(sc => sc != null);

			db.ShoppingCarts
				.DeleteAllOnSubmit(giftCarts);

			db.PromotionUsages
				.DeleteAllOnSubmit(promotionUsages.Select(puli => puli.promoUsage));

			//Only add auto assigned promos to the removed list if they are intentionally removed by user
			if(removeAutoAssigned)
				foreach(var promoUsage in promotionUsages.Where(puli => puli.promo.AutoAssigned))
					PromotionManager.IgnorePromotion(promoUsage.promoUsage.CustomerId, promoUsage.promo.Id);

			db.SubmitChanges();
		}

		public static void ClearAllPromotionUsages(int customerId)
		{
			ClearPromotionUsages(customerId, null, false);
		}

		public static void AutoAssignPromotions(int customerId, IRuleContext ruleContext)
		{
			var assignedPromotions = GetAssignedPromotions(customerId)
				.ToArray()
				.Where(p => p.AutoAssigned);

			var assignablePromotions = GetAssignablePromotions(customerId, ruleContext)
				.ToArray();

			foreach(var promotionToAssign in assignablePromotions)
				AssignPromotion(customerId, promotionToAssign.Id);

			var promotionsToUnassign = assignedPromotions
				.Except(assignablePromotions);

			foreach(var promotionToUnassign in promotionsToUnassign)
				ClearPromotionUsages(customerId, promotionToUnassign.Code, false);
		}

		public static IEnumerable<PromotionsData.Promotion> GetAssignablePromotions(int customerId, IRuleContext ruleContext)
		{
			// If the cart type isn't valid, skip all promotion checks and return an empty collection.
			if(!IsValidCartType(ruleContext))
				return Enumerable.Empty<PromotionsData.Promotion>();

			var promotionController = CreatePromotionController();

			// Determine all active, autoassigned promotions 
			var promotionsToAssign = PromotionsData.DataContextProvider
				.Current
				.Promotions
				.Where(p => p.Active
					&& p.AutoAssigned
					&& !GetIgnoredAutoAssignedPromotions(customerId).Contains(p.Id));

			var autoAssignPromotions = new List<PromotionsData.Promotion>();

			// Now see if they are valid and available for the current customer.
			foreach(var promotion in promotionsToAssign)
				if(promotionController.ValidatePromotion(promotion, ruleContext, true).All(vr => vr.IsValid))
					if(IsGiftProductDiscountValid(promotion, ruleContext, CreateDiscountContext(ruleContext, GetDiscountableItems(ruleContext, promotion.Id))))
						autoAssignPromotions.Add(promotion);

			return autoAssignPromotions;
		}

		static bool IsGiftProductDiscountValid(PromotionsData.Promotion promotion, IRuleContext ruleContext, IDiscountContext discountContext)
		{
			// If the store isn't concerned with quantity on hand, all 
			if(!AppLogic.AppConfigBool("Inventory.LimitCartToQuantityOnHand")
				&& Store.StoreCount == 1)
				return true;

			var giftDiscounts = promotion
				.PromotionDiscounts
				.OfType<GiftProductPromotionDiscount>();

			// If this promotion doesn't contain gift product discounts, return it as valid.
			if(!giftDiscounts.Any())
				return true;

			// If there's inventory for all gifted products then return the promotion as valid.
			return giftDiscounts.All(giftDiscount =>
			{
				// Determine the quantity of gifted products.
				var quantityToGift = giftDiscount.MatchQuantities
					? discountContext.DiscountableItems.Sum(s => s.Quantity)
					: 1;

				// this promotion
				return giftDiscount
					.GiftProductIds
					.All(giftProductId =>
					{
						return AppLogic.GetInventory(
							productId: giftProductId, 
							variantId: AppLogic.GetDefaultProductVariant(giftProductId),
							chosenSize: string.Empty,
							chosenColor: string.Empty) >= AppLogic.AppConfigNativeInt("HideProductsWithLessThanThisInventoryLevel")
							&& new Product(giftProductId).IsMappedToStore();
					});
			});
		}

		public static List<Int32> GetNonDiscountableShoppingCartIds(IRuleContext ruleContext)
		{
			var nonDiscountableIds = new List<int>();
			var promotionLineItems = PromotionsData.DataContextProvider.Current
				.PromotionLineItems
				.Where(pli => ruleContext.ShoppingCartItems
					.Select(sci => sci.ShoppingCartRecordId)
					.Contains(pli.shoppingCartRecordId) 
					&& pli.isAGift);

			if(promotionLineItems.Any())
				nonDiscountableIds
					.AddRange(promotionLineItems.Select(pli => pli.shoppingCartRecordId)
					.ToList());

			return nonDiscountableIds;
		}

		public static List<DiscountableItem> GetDiscountableItems(IRuleContext ruleContext, int promoId)
		{
			var discountableItems = new List<DiscountableItem>();
			var nonDiscountableIds = GetNonDiscountableShoppingCartIds(ruleContext);
			var promotion = PromotionsData.DataContextProvider.Current
				.Promotions
				.FirstOrDefault(p => p.Active && p.Id == promoId);

			ruleContext.PromotionId = promotion.Id;

			foreach(var pd in promotion
				.PromotionDiscounts
				.Where(p => p.GetType() == typeof(OrderItemPromotionDiscount) || p.GetType() == typeof(GiftProductPromotionDiscount) || p.GetType() == typeof(OrderPromotionDiscount)))
			{
				var categoryIdRule = new CategoryPromotionRule();
				var sectionIdRule = new SectionPromotionRule();
				var manufacturerIdRule = new ManufacturerPromotionRule();
				var productIdRule = new ProductIdPromotionRule();

				if(promotion.PromotionRules.Where(pr => pr.GetType() == typeof(CategoryPromotionRule)).Count() > 0)
					categoryIdRule = promotion.PromotionRules.Where(pr => pr.GetType() == typeof(CategoryPromotionRule)).First() as CategoryPromotionRule;

				if(promotion.PromotionRules.Where(pr => pr.GetType() == typeof(SectionPromotionRule)).Count() > 0)
					sectionIdRule = promotion.PromotionRules.Where(pr => pr.GetType() == typeof(SectionPromotionRule)).First() as SectionPromotionRule;

				if(promotion.PromotionRules.Where(pr => pr.GetType() == typeof(ManufacturerPromotionRule)).Count() > 0)
					manufacturerIdRule = promotion.PromotionRules.Where(pr => pr.GetType() == typeof(ManufacturerPromotionRule)).First() as ManufacturerPromotionRule;

				if(promotion.PromotionRules.Where(pr => pr.GetType() == typeof(ProductIdPromotionRule)).Count() > 0)
					productIdRule = promotion.PromotionRules.Where(pr => pr.GetType() == typeof(ProductIdPromotionRule)).First() as ProductIdPromotionRule;

				foreach(var cartItem in ruleContext.ShoppingCartItems)
				{
					var qualifies = true;
					if(categoryIdRule.CategoryIds != null && categoryIdRule.CategoryIds.Count() > 0 && cartItem.CategoryIds != null && cartItem.CategoryIds.Intersect(categoryIdRule.CategoryIds).Count() == 0)
						qualifies = false;

					if(sectionIdRule.SectionIds != null && sectionIdRule.SectionIds.Count() > 0 && cartItem.SectionIds != null && cartItem.SectionIds.Intersect(sectionIdRule.SectionIds).Count() == 0)
						qualifies = false;

					if(manufacturerIdRule.ManufacturerIds != null && manufacturerIdRule.ManufacturerIds.Count() > 0 && cartItem.ManufacturerIds != null && cartItem.ManufacturerIds.Intersect(manufacturerIdRule.ManufacturerIds).Count() == 0)
						qualifies = false;

					if(productIdRule.ProductIds != null && !productIdRule.ProductIds.Contains(cartItem.ProductId))
						qualifies = false;

					if(nonDiscountableIds.Contains(cartItem.ShoppingCartRecordId))
						qualifies = false;

					if(qualifies)
					{
						if(pd.GetType() == typeof(OrderItemPromotionDiscount) || pd.GetType() == typeof(GiftProductPromotionDiscount))
						{
							var discountableItem = new DiscountableItem();
							discountableItem.CartPrice = cartItem.CartPrice;
							discountableItem.ProductId = cartItem.ProductId;
							discountableItem.Quantity = cartItem.Quantity;
							discountableItem.ShoppingCartRecordId = cartItem.ShoppingCartRecordId;
							discountableItem.Sku = cartItem.Sku;
							discountableItem.Subtotal = cartItem.Subtotal;
							discountableItem.VariantId = cartItem.VariantId;
							discountableItems.Add(discountableItem);
						}
					}
				}
			}

			return discountableItems;
		}

		public static IRuleContext CreateRuleContext(ShoppingCart cart)
		{
			var excludeConfig = AppLogic.AppConfig("Promotions.ExcludeStates");
			var filterByStore = false;
			var allowPromoFiltering = GlobalConfig.GetGlobalConfig("AllowPromotionFiltering");
			if(allowPromoFiltering != null)
				bool.TryParse(allowPromoFiltering.ConfigValue, out filterByStore);

			var cartItems = cart.CartItems
				.Where(ci => !ci.IsGift)
				.Select(ci =>
					new ShoppingCartItem
					{
						CartPrice = ci.Price,
						CategoryIds = Array.ConvertAll<string, int>(AppLogic.GetProductEntityMappings(ci.ProductID, "CATEGORY").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries), i => Convert.ToInt32(i.ToString())),
						ManufacturerIds = Array.ConvertAll<string, int>(AppLogic.GetProductEntityMappings(ci.ProductID, "MANUFACTURER").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries), i => Convert.ToInt32(i.ToString())),
						ProductId = ci.ProductID,
						Quantity = ci.Quantity,
						SectionIds = Array.ConvertAll<string, int>(AppLogic.GetProductEntityMappings(ci.ProductID, "SECTION").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries), i => Convert.ToInt32(i.ToString())),
						ShoppingCartRecordId = ci.ShoppingCartRecordID,
						Sku = ci.SKU,
						Name = ci.ProductName,
						Subtotal = ci.Price * ci.Quantity,
						VariantId = ci.VariantID,
						IsGift = ci.IsGift
					}
				);

			//To check is Promotion applied Before Quantity Discount.
			var includeDiscounts = AppLogic.AppConfigBool("Promotions.ApplyDiscountsBeforePromoApplied");

			//Get the checkout context so we can use the shipping method the customer chose
			var persistedCheckoutContextProvider = (IPersistedCheckoutContextProvider)System.Web.Mvc.DependencyResolver.Current.GetService(typeof(IPersistedCheckoutContextProvider));
			var persistedCheckoutContext = persistedCheckoutContextProvider.LoadCheckoutContext(cart.ThisCustomer);

			var shippingMethodId = persistedCheckoutContext.SelectedShippingMethodId != null
				? (int)persistedCheckoutContext.SelectedShippingMethodId
				: 0;
			
			var ruleContext = new SimpleRuleContext
				{
					ShoppingCartItems = cartItems,
					StoreId = AppLogic.StoreID(),
					CustomerId = cart.ThisCustomer.CustomerID,
					IsRegistered = cart.ThisCustomer.IsRegistered,
					BillingAddressId = cart.ThisCustomer.PrimaryBillingAddressID,
					ShippingAddressId = cart.ThisCustomer.PrimaryShippingAddressID,
					EmailAddress = cart.ThisCustomer.EMail,
					CustomerLevel = cart.ThisCustomer.CustomerLevelID,
					ShippingMethodId = shippingMethodId,
					State = cart.ThisCustomer.PrimaryShippingAddress.State,
					ZipCode = cart.ThisCustomer.PrimaryShippingAddress.Zip,
					CountryCode = cart.ThisCustomer.PrimaryShippingAddress.Country,
					ShippingTotal = cart.ShippingTotal(true, true),
					SubTotal = cart.SubTotal(includeDiscounts, false, true, true),
					ExcludeStates = excludeConfig.Length > 0 ? excludeConfig.Split(',') : null,
					FilterByStore = filterByStore,
					AddItemToCart = (int productId, int variantId, int quantity) => {
							var cartActionProvider = DependencyResolver.Current.GetService<CartActionProvider>();
							return cartActionProvider.PromotionAdd(cart.ThisCustomer, cart.CartType, productId, variantId, quantity);
						},
					CartType = (int?)cart.CartType
				};

			return ruleContext;
		}

		public static IRuleContext CreateRuleContext(IRuleContext ruleContext, int promotionId)
		{
			return new SimpleRuleContext(ruleContext)
				{
					PromotionId = promotionId,
					AddItemToCart = ruleContext.AddItemToCart
				};
		}

		public static IList<IDiscountResult> GetDiscountResultList(IRuleContext ruleContext)
		{
			var discountResults = (IList<IDiscountResult>)new List<IDiscountResult>();

			if(!IsValidCartType(ruleContext))
				return discountResults;

			GetPromotionDiscount(ruleContext, out discountResults);

			return discountResults;
		}

		public static void FinalizePromotionsOnOrderComplete(ShoppingCart cart, int orderNumber)
		{
			foreach(var result in cart.DiscountResults)
			{
				var promotionUsage = PromotionsData.DataContextProvider
					.Current
					.PromotionUsages
					.FirstOrDefault(pu => pu.PromotionId == result.Promotion.Id && pu.Complete == false && pu.CustomerId == cart.ThisCustomer.CustomerID);

				if(promotionUsage == null)
					continue;

				promotionUsage.OrderId = orderNumber;
				promotionUsage.DateApplied = DateTime.Now;
				promotionUsage.LineItemDiscountAmount = result.LineItemTotal;
				promotionUsage.ShippingDiscountAmount = result.ShippingTotal;
				promotionUsage.OrderDiscountAmount = result.OrderTotal;
				promotionUsage.DiscountAmount = result.TotalDiscount;
				promotionUsage.Complete = true;

				PromotionsData.DataContextProvider.Current.SubmitChanges();
			}

			//Make sure we clear out the auto assigned list.
			PromotionManager.ResetAutoAssignedPromotions(cart.ThisCustomer.CustomerID);
		}

		public static void TransferPromotionsOnUserLogin(int currentCustomerId, int newCustomerId)
		{
			var db = PromotionsData.DataContextProvider.Current;
			var promotionUsages = db
				.PromotionUsages
				.Where(pu => pu.CustomerId == currentCustomerId);

			foreach(var promoUsage in promotionUsages)
				promoUsage.CustomerId = newCustomerId;

			db.SubmitChanges();
		}

		public static bool IsValidCartType(IRuleContext ruleContext)
		{
			return (ruleContext.CartType != null
				&& ruleContext.CartType == (Int32)CartTypeEnum.ShoppingCart);
		}

		public static void RemoveDuplicatePromotionUsages(int CustomerId)
		{
			var db = PromotionsData.DataContextProvider.Current;

			//Grab a list of all promos for this customer that are applied to the current cart (IE not complete)
			var promoUsages = 
				from pu in db.PromotionUsages
				join p in db.Promotions on pu.PromotionId equals p.Id
				join pli in db.PromotionLineItems on pu.Id equals pli.PromotionUsageId into pui
				from pli in pui.DefaultIfEmpty()
				join sc in db.ShoppingCarts on pli.shoppingCartRecordId equals sc.ShoppingCartRecID into sci
				from sc in sci.DefaultIfEmpty()
				where pu.CustomerId == CustomerId && !pu.Complete

				select new
					{
						promoUsage = pu
						,
						promo = p
						,
						promoLineItem = pli
						,
						scItem = sc
					};

			//Get all the promousages that are duplicates (should only have on promousage per promo on a customers cart, can have multiple complete but should only have one un-complete per promo)                      
			var groupedPromoUsages =
				from pu in promoUsages
				group pu by new { pu.promoUsage.Complete, pu.promoUsage.CustomerId, pu.promoUsage.PromotionId } into pug
				where pug.Count() > 1

				select new
					{
						pug.Key.CustomerId
						,
						pug.Key.PromotionId
						,
						pug.Key.Complete
					};

			if(!groupedPromoUsages.Any())
				return;

			//reduce our list of promo usages down to just those that have duplicates
			promoUsages =
				from pu in promoUsages
				join pug in groupedPromoUsages on new { pu.promoUsage.CustomerId, pu.promoUsage.PromotionId, pu.promoUsage.Complete } equals new { pug.CustomerId, pug.PromotionId, pug.Complete }
				select pu;

			foreach(var promoGroup in groupedPromoUsages)
			{
				//we will keep the first dupe we find, doesn't matter which
				var promoToSave = promoUsages.FirstOrDefault(pu => pu.promoUsage.PromotionId == promoGroup.PromotionId);

				//select all the duplicate records
				var promoUsageToDelete = promoUsages.Where(pu => pu.promoUsage.PromotionId == promoGroup.PromotionId && pu.promoUsage.Id != promoToSave.promoUsage.Id);
				//delete any duplicate free gift items
				var giftCarts = promoUsageToDelete.Where(pu => pu.promoLineItem.isAGift).Select(pu => pu.scItem).Where(sc => sc != null);
				db.ShoppingCarts.DeleteAllOnSubmit(giftCarts);
				//delete the duplicate promo usages
				db.PromotionUsages.DeleteAllOnSubmit(promoUsageToDelete.Select(pu => pu.promoUsage));

			}

			db.SubmitChanges();
		}

		/// <summary>
		/// This method clears out any unused promotions for an order.  Unused promotions are an artifact of promotions that include
		/// shipping discounts.  They stay applied in order to support switching of shipping methods without the promotion falling 
		/// off in the checkout process.
		/// </summary>
		/// <param name="orderNumber"></param>
		public static void RemoveUnusedPromotionsForOrder(int orderNumber)
		{
			var db = PromotionsData.DataContextProvider.Current;

			var promoUsages =
				from pu in db.PromotionUsages
				where pu.OrderId == orderNumber && pu.Complete && pu.DiscountAmount == decimal.Zero
				select pu;

			db.PromotionUsages.DeleteAllOnSubmit(promoUsages);
			db.SubmitChanges();
		}

		public static void ResetAutoAssignedPromotions(int customerId)
		{
			new Customer(customerId, true)
				.ThisCustomerSession["RemovedAutoAssignPromotionIdList"] = string.Empty;
		}

		public static List<int> GetIgnoredAutoAssignedPromotions(int customerId)
		{
			var ignoredPromotions = new Customer(customerId, true)
				.ThisCustomerSession["RemovedAutoAssignPromotionIdList"];

			var serializer = new XmlSerializer(typeof(List<int>));
			if(!string.IsNullOrEmpty(ignoredPromotions))
				using(var reader = new StringReader(ignoredPromotions))
					return (serializer.Deserialize(reader) as List<int>) ?? new List<int>();
			else
				return new List<int>();
		}

		public static void IgnorePromotion(int customerId, int promotionId)
		{
			var serializer = new XmlSerializer(typeof(List<int>));
			var builder = new StringBuilder();
			var promotions = GetIgnoredAutoAssignedPromotions(customerId);
			promotions.Add(promotionId);

			using(var writer = new StringWriter(builder))
				serializer.Serialize(writer, promotions);

			new Customer(customerId, true)
				.ThisCustomerSession["RemovedAutoAssignPromotionIdList"] = builder.ToString();
		}

		public static void RemoveIgnoredPromotion(int customerId, int promotionId)
		{
			var serializer = new XmlSerializer(typeof(List<int>));
			var builder = new StringBuilder();
			var promotions = GetIgnoredAutoAssignedPromotions(customerId);
			if(promotions.Contains(promotionId))
				promotions.Remove(promotionId);

			using(var writer = new StringWriter(builder))
				serializer.Serialize(writer, promotions);

			new Customer(customerId, true)
				.ThisCustomerSession["RemovedAutoAssignPromotionIdList"] = builder.ToString();
		}
	}
}
