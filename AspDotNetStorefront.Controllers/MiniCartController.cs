// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using AspDotNetStorefront.Caching.ObjectCaching;
using AspDotNetStorefront.Classes.Checkout;
using AspDotNetStorefront.Filters;
using AspDotNetStorefront.Models;
using AspDotNetStorefrontCore;
using AspDotNetStorefront.Routing;

namespace AspDotNetStorefront.Controllers
{
	public partial class MinicartController : Controller
	{
		readonly NoticeProvider NoticeProvider;
		readonly CartActionProvider CartActionProvider;
		readonly ICachedShoppingCartProvider CachedShoppingCartProvider;

		public MinicartController(NoticeProvider noticeProvider, CartActionProvider cartActionProvider, ICachedShoppingCartProvider cachedShoppingCartProvider)
		{
			NoticeProvider = noticeProvider;
			CartActionProvider = cartActionProvider;
			CachedShoppingCartProvider = cachedShoppingCartProvider;
		}

		public ActionResult Index(CartTypeEnum cartType)
		{
			// Do not show this minicart if it is disabled
			var enabled = cartType == CartTypeEnum.WishCart
				? AppLogic.AppConfigBool("ShowWishButtons")
				: AppLogic.AppConfigBool("Minicart.Enabled") 
					&& AppLogic.GetCurrentPageType() != PageTypes.Checkout;
			if(!enabled)
				return Content(string.Empty);

			var customer = HttpContext.GetCustomer();
			var cart = CachedShoppingCartProvider.Get(customer, cartType, AppLogic.StoreID());

			return PartialView(
				cartType == CartTypeEnum.ShoppingCart
					? ViewNames.MiniCartPartial
					: ViewNames.MiniWishPartial,
				GetShoppingCartViewModel(cart, customer));
		}

		public ActionResult MinicartLink(CartTypeEnum cartType)
		{
			var customer = HttpContext.GetCustomer();
			var cart = CachedShoppingCartProvider.Get(customer, cartType, AppLogic.StoreID());

			return PartialView(
				cartType == CartTypeEnum.ShoppingCart
					? ViewNames.MiniCartLinkPartial
					: ViewNames.MiniWishLinkPartial,
				GetShoppingCartViewModel(cart, customer));
		}

		public ActionResult MinicartResources()
		{
			return PartialView(ViewNames.MiniCartResourcesPartial, new MinicartResourcesViewModel
			{
				MinicartEnabled = AppLogic.AppConfigBool("Minicart.Enabled"),
				MiniwishlistEnabled = AppLogic.AppConfigBool("ShowWishButtons"),
				CheckoutInProgress = AppLogic.GetCurrentPageType() == PageTypes.Checkout
			});
		}

		[HttpPost]
		public ActionResult UpdateMinicart([Bind(Include = "CartItems")] MinicartViewModel cartViewModel, CartTypeEnum cartType)
		{
			if(!ModelState.IsValid)
				return RedirectToAction(ActionNames.Index, ControllerNames.Minicart);

			var customer = HttpContext.GetCustomer();

			// Make request, check status
			var response = UpdateCartFromViewModel(cartViewModel, customer, cartType);
			switch(response.Status)
			{
				case CartActionStatus.Forbidden:
				case CartActionStatus.RequiresLogin:
				case CartActionStatus.SessionTimeout:
					foreach(var message in response.Messages)
						NoticeProvider.PushNotice(message.ConvertToNotice());
					return Json(
						data: new AjaxAddToCartData(
							status: response.Status.ToString(),
							messages: null,
							minicartData: GetMinicartData(customer, response.UpdatedCart, null)),
						behavior: JsonRequestBehavior.AllowGet);
				default:
					break;
			}
			
			// Handle non-ajax calls
			if(!Request.IsAjaxRequest())
			{
				foreach(var message in response.Messages)
					NoticeProvider.PushNotice(message.ConvertToNotice());
				return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
			}

			// The view should reflect the real cart not the posted values in the modelstate.
			ModelState.Clear();
			return Json(
				GetMinicartData(
					customer: customer,
					cart: response.UpdatedCart,
					messages: response.Messages.Select(
						n => n.ConvertToAjaxNotice())),
				JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public ActionResult DeleteMiniCartItem(int id, CartTypeEnum cartType)
		{
			var customer = HttpContext.GetCustomer();

			// Make request, check status
			var response = CartActionProvider.RemoveItemFromCart(customer, cartType, id);
			switch(response.Status)
			{
				case CartActionStatus.Forbidden:
				case CartActionStatus.RequiresLogin:
				case CartActionStatus.SessionTimeout:
					foreach(var message in response.Messages)
						NoticeProvider.PushNotice(message.ConvertToNotice());
					return Json(
						data: new AjaxAddToCartData(
							status: response.Status.ToString(),
							messages: null,
							minicartData: GetMinicartData(customer, response.UpdatedCart, null)),
						behavior: JsonRequestBehavior.AllowGet);
				default:
					break;
			}
			
			// Handle non-ajax calls
			if(!Request.IsAjaxRequest())
			{
				foreach(var message in response.Messages)
					NoticeProvider.PushNotice(message.ConvertToNotice());
				return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
			}

			// The view should reflect the real cart not the posted values in the modelstate.
			ModelState.Clear();
			return Json(
				GetMinicartData(
					customer: customer,
					cart: response.UpdatedCart,
					messages: response.Messages.Select(
						n => n.ConvertToAjaxNotice())),
				JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public ActionResult MoveAllToCart([Bind(Include = "CartItems")] MinicartViewModel cartViewModel)
		{
			// update the wishlist qty's first (in case they were changed on the form but not saved yet)
			UpdateMinicart(cartViewModel, CartTypeEnum.WishCart);

			// now move all items to cart
			var customer = HttpContext.GetCustomer();
			var wish = CachedShoppingCartProvider.Get(customer, CartTypeEnum.WishCart, AppLogic.StoreID());

			foreach(var item in wish.CartItems.ToArray())
				wish.SetItemCartType(item.ShoppingCartRecordID, CartTypeEnum.ShoppingCart);

			CartActionProvider.ConsolidateCartItems(customer, CartTypeEnum.ShoppingCart);

			// rebuild wishlist (should be now empty) to send in response
			wish = CachedShoppingCartProvider.Get(customer, CartTypeEnum.WishCart, AppLogic.StoreID());

			// Pull cart (to reflect any changes) to send in response
			var cart = CachedShoppingCartProvider.Get(customer, CartTypeEnum.ShoppingCart, AppLogic.StoreID());

			ModelState.Clear();

			return Json(new AjaxDualMiniData(
				GetMinicartData(customer, cart),
				GetMinicartData(customer, wish)),
				JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		[RequireCustomerRecordFilter]
		public ActionResult AjaxAddToCart(AddToCartPostModel model)
		{
			if(!ModelState.IsValid)
				return RedirectToAction(ActionNames.Index, ControllerNames.Minicart);

			var customer = HttpContext.GetCustomer();
			var cartType = model.IsWishlist
				? CartTypeEnum.WishCart
				: CartTypeEnum.ShoppingCart;

			// Make request, check status
			var response = CartActionProvider.AddItemToCart(
				model.ConvertToAddToCartContext(
					customer,
					cartType));

			switch(response.Status)
			{
				case CartActionStatus.Forbidden:
				case CartActionStatus.RequiresLogin:
				case CartActionStatus.SessionTimeout:
					if(response.Messages.Any())
						foreach(var message in response.Messages)
							NoticeProvider.PushNotice(message.ConvertToNotice());
					return Json(
						data: new AjaxAddToCartData(
							status: response.Status.ToString(),
							messages: null,
							minicartData: GetMinicartData(customer, response.UpdatedCart, null)),
						behavior: JsonRequestBehavior.AllowGet);
				default:
					break;
			}
			
			// Handle non-ajax calls
			if(!Request.IsAjaxRequest())
			{
				foreach(var message in response.Messages)
					NoticeProvider.PushNotice(message.ConvertToNotice());
				return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
			}

			ModelState.Clear();

			// The view should reflect the real cart not the posted values in the modelstate.
			var notices = response.Messages
				.Select(n => n.ConvertToAjaxNotice());
			// Pull cart now (to reflect any possible changes)
			var cart = response.UpdatedCart;

			return Json(
					data: new AjaxAddToCartData(
						status: response.Status.ToString(),
						messages: notices,
						minicartData: GetMinicartData(customer, cart, notices)),
					behavior: JsonRequestBehavior.AllowGet);

		}

		[HttpPost]
		[RequireCustomerRecordFilter]
		public ActionResult AjaxAddKitToCart(KitAddToCartPostModel model)
		{
			if(!ModelState.IsValid)
				return RedirectToAction(ActionNames.Index, ControllerNames.Minicart);

			var customer = HttpContext.GetCustomer();
			var cartType = model.IsWishlist
				? CartTypeEnum.WishCart
				: CartTypeEnum.ShoppingCart;
			var notices = new List<AjaxNotice>();

			// Make request, check status
			var response = CartActionProvider.AddItemToCart(
				model.ConvertToAddToCartContext(
					customer,
					cartType));

			switch(response.Status)
			{
				case CartActionStatus.Forbidden:
				case CartActionStatus.RequiresLogin:
				case CartActionStatus.SessionTimeout:
					if(response.Messages.Any())
						foreach(var message in response.Messages)
							NoticeProvider.PushNotice(message.ConvertToNotice());
					return Json(
						data: new AjaxAddToCartData(
							status: response.Status.ToString(),
							messages: notices.AsEnumerable(),
							minicartData: GetMinicartData(customer, response.UpdatedCart, notices)),
						behavior: JsonRequestBehavior.AllowGet);
				default:
					break;
			}

			// Add any returned notices
			if(response.Messages.Any())
				notices.AddRange(
					response.Messages.Select(
						n => n.ConvertToAjaxNotice()));

			// Pull cart now (to reflect any possible changes)
			var cart = response.UpdatedCart;

			// Handle non-ajax calls
			if(!Request.IsAjaxRequest())
			{
				foreach(var message in response.Messages)
					NoticeProvider.PushNotice(message.ConvertToNotice());
				return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
			}

			// The view should reflect the real cart not the posted values in the modelstate.
			ModelState.Clear();
			return Json(
					data: new AjaxAddToCartData(
						status: response.Status.ToString(),
						messages: notices.AsEnumerable(),
						minicartData: GetMinicartData(customer, cart, notices)),
					behavior: JsonRequestBehavior.AllowGet);
		}

		AjaxAddToCartData GetAddToCartData(CartActionResponse response)
		{
			switch(response.Status)
			{
				case CartActionStatus.NotAllowed:
					return new AjaxAddToCartData(
							status: AjaxNoticeType.failure.ToString()
						);
				case CartActionStatus.RequiresLogin:
					return new AjaxAddToCartData(
						status: AjaxNoticeType.failure.ToString(),
						messages: new[]
						{
							new AjaxNotice(
								message: "In order to create a wishlist, you will need to register now and create an account on the site.",
								type: AjaxNoticeType.failure)
						});
				case CartActionStatus.ValidationErrors:
					return new AjaxAddToCartData(
						status: AjaxNoticeType.failure.ToString(),
						messages: response.Messages
							.Select(n => n.ConvertToAjaxNotice())
					);
				default:
					return new AjaxAddToCartData(
							status: AjaxNoticeType.failure.ToString(),
							messages: new[]
							{
								new AjaxNotice(
									message: "An error occurred attempting to add an item to the cart.",
									type: AjaxNoticeType.failure)
							}
						);
			}
		}

		MinicartViewModel GetShoppingCartViewModel(ShoppingCart cart, Customer customer)
		{
			var viewCartItems = new List<MinicartItemViewModel>();
			var showImages = AppLogic.AppConfigBool("Minicart.ShowImages");
			var allowQuantityUpdate = AppLogic.AppConfigBool("Minicart.QuantityUpdate.Enabled");

			foreach(var cartItem in cart.CartItems)
			{
				var recurringVariants = ProductVariant.GetVariants(cartItem.ProductID, false).Where(pc => pc.IsRecurring == true);
				var recurringIntervalOptions = new SelectList(recurringVariants, "VariantID", "LocaleName", cartItem.VariantID.ToString());

				var promotionText = cartItem.ThisShoppingCart.DiscountResults
					.Where(dr => dr.DiscountedItems
						.Where(di => di.ShoppingCartRecordId == cartItem.ShoppingCartRecordID)
						.Any())
					.Select(dr => dr.Promotion.UsageText)
					.FirstOrDefault();

				viewCartItems.Add(new MinicartItemViewModel
				{
					Id = cartItem.ShoppingCartRecordID,
					ProductId = cartItem.ProductID,
					VariantId = cartItem.VariantID,
					RecurringVariantId = cartItem.VariantID,    //Have to set this here for the dropdown's SelectedValue to be right
					ProductName = cartItem.ProductName,
					VariantName = cartItem.VariantName,
					ChosenColor = AppLogic.CleanSizeColorOption(cartItem.ChosenColor),
					ChosenColorSkuModifier = cartItem.ChosenColorSKUModifier,
					ChosenSize = AppLogic.CleanSizeColorOption(cartItem.ChosenSize),
					ChosenSizeSkuModifier = cartItem.ChosenColorSKUModifier,
					ProductSku = cartItem.SKU,
					Notes = cartItem.Notes,
					Quantity = cartItem.Quantity,
					ProductUrl = Url.BuildProductLink(
						id: cartItem.ProductID,
						seName: cartItem.SEName),
					ProductImageUrl = cartItem.ProductPicURL,
					ProductImageAlternateText = cartItem.SEAltText,
					ShowImage = showImages,
					RecurringIntervalOptions = recurringIntervalOptions,
                    // nal
					//LinkToProduct = AppLogic.AppConfigBool("LinkToProductPageInCart"),
                    LinkToProduct = true,
					TextOption = cartItem.TextOptionDisplayFormat,
					TextOptionLabel = cartItem.TextOptionPrompt,
					LineItemPromotionText = promotionText,
					SubTotalDisplay = cartItem.RegularPriceRateDisplayFormat,
					VatDisplay = cartItem.VatRateDisplayFormat,
					IsAKit = cartItem.IsAKit,
					KitDetails = cartItem.KitComposition.Compositions
						.Select(comp => new KitCartItemViewModel
						{
							Name = comp.Name,
							IsImage = comp.ContentIsImage,
							TextOption = comp.TextOption
						})
						.ToList(),
					AllowQuantityUpdate = allowQuantityUpdate,
					ShowEditLink = AppLogic.AppConfigBool("ShowEditButtonInCartForKitProducts") && cartItem.IsAKit
						? true
						: AppLogic.AppConfigBool("ShowEditButtonInCartForRegularProducts") && !cartItem.IsAKit
							? true
							: false,
					EditUrl = string.Format(
						"{0}?cartRecordId={1}",
						Url.BuildProductLink(
							id: cartItem.ProductID,
							seName: cartItem.SEName),
						cartItem.ShoppingCartRecordID),
					RestrictedQuantities = new SelectList(cartItem.RestrictedQuantities),
					ShowSku = !cartItem.IsSystem && AppLogic.AppConfigBool("Minicart.ShowSku")
				});
			}

			var maximumCartItemsToDisplay = AppLogic.AppConfigNativeInt("MaximumNumberOfCartItemsToDisplay");

			var orderTotal = cart.Total(true);
			var subtotal = cart.SubTotal(
				includeDiscount: false,
				onlyIncludeTaxableItems: false,
				includeDownloadItems: true,
				includeFreeShippingItems: true,
				includeSystemItems: true,
				useCustomerCurrencySetting: true);

			var taxTotal = cart.TaxTotal();
			var shippingTotal = cart.ShippingTotal(
				includeDiscount: true,
				includeTax: true);
			var displayTotal = orderTotal - taxTotal - shippingTotal;
			var discountTotal = Math.Round(displayTotal - subtotal, 2);

			var cartViewModel = new MinicartViewModel
			{
				CartItems = viewCartItems,
				MaximumCartItemsToDisplay = maximumCartItemsToDisplay,
				UseMaximumCartItemBehavor = maximumCartItemsToDisplay > 0 && viewCartItems.Count() > maximumCartItemsToDisplay,
				SubTotal = Localization.CurrencyStringForDisplayWithExchangeRate(subtotal, customer.CurrencySetting),
				Total = Localization.CurrencyStringForDisplayWithExchangeRate(displayTotal, customer.CurrencySetting),
				Discount = discountTotal != 0
					? Localization.CurrencyStringForDisplayWithExchangeRate(
						discountTotal,
						customer.CurrencySetting)
					: null,
				ItemCount = viewCartItems.Sum(item => item.Quantity),
				OtherMiniCount = cart.CartType == CartTypeEnum.ShoppingCart
					? CachedShoppingCartProvider.Get(customer, CartTypeEnum.WishCart, AppLogic.StoreID()).CartItems.Count()
					: CachedShoppingCartProvider.Get(customer, CartTypeEnum.ShoppingCart, AppLogic.StoreID()).CartItems.Count(),
				AllowQuantityUpdate = allowQuantityUpdate,
				MinicartEnabled = AppLogic.AppConfigBool("Minicart.Enabled") && AppLogic.GetCurrentPageType() != PageTypes.Checkout,
				MiniwishEnabled = AppLogic.AppConfigBool("ShowWishButtons") && AppLogic.GetCurrentPageType() != PageTypes.Checkout
			};

			return cartViewModel;
		}

		CartActionResponse UpdateCartFromViewModel(MinicartViewModel cartViewModel, Customer customer, CartTypeEnum cartType)
		{
			if(cartViewModel.CartItems == null)
				return null;
			
			var messages = new List<CartActionMessage>();
			CartActionResponse response = null;

			foreach(var cartItem in cartViewModel.CartItems)
			{
				// Is this a Recurring Interval (Variant) change only?
				if(cartItem.RecurringVariantId != 0
					&& cartItem.VariantId != cartItem.RecurringVariantId
					&& AppLogic.AppConfigBool("AllowRecurringFrequencyChangeInCart")) // Update Recurring Interval (Variant)
					response = CartActionProvider.ReplaceRecurringIntervalVariantInCart(
						customer: customer,
						cartType: cartType,
						shoppingCartRecId: cartItem.Id, 
						oldVariantId: cartItem.VariantId,
						newVariantId: cartItem.RecurringVariantId);
				else // Perform regular Qty Update
					response = CartActionProvider.UpdateItemQuantityInCart(new UpdateQuantityContext()
					{
						Customer = customer,
						CartType = cartType,
						ShoppingCartRecId = cartItem.Id,
						Quantity = cartItem.Quantity
					});

				if(response.Status != CartActionStatus.Success)
					return response;

				messages.AddRange(response.Messages.ToList());
			}

			return new CartActionResponse(
				updatedCart: response.UpdatedCart
					?? CachedShoppingCartProvider.Get(customer, cartType, AppLogic.StoreID()),
				status: CartActionStatus.Success,
				messages: messages);
		}

		MinicartAjaxData GetMinicartData(Customer customer, ShoppingCart cart, IEnumerable<AjaxNotice> messages = null)
		{
			var newCart = cart;
			var inventoryTrimmedReason = CartActionProvider.ValidateCartQuantitiesAgainstInventory(customer, cart.CartType);
			if(inventoryTrimmedReason != InventoryTrimmedReason.None)
			{
				//If inventory was trimmed, we need to update the cart object so the cart items are current
				newCart = CachedShoppingCartProvider.Get(customer, cart.CartType, AppLogic.StoreID());

				messages = new[] {
					new AjaxNotice(
						message: newCart.GetInventoryTrimmedUserMessage(inventoryTrimmedReason),
						type: AjaxNoticeType.info)
				};
			}

			var viewName = newCart.CartType == CartTypeEnum.ShoppingCart
				? "_MiniCartContents"
				: "_MiniWishContents";

			var cartViewModel = GetShoppingCartViewModel(newCart, customer);

			var orderTotal = newCart.Total(true);

			var subtotal = newCart.SubTotal(
				includeDiscount: false,
				onlyIncludeTaxableItems: false,
				includeDownloadItems: true,
				includeFreeShippingItems: true,
				includeSystemItems: true,
				useCustomerCurrencySetting: true);

			var taxTotal = newCart.TaxTotal();

			var shippingTotal = newCart.ShippingTotal(
				includeDiscount: true,
				includeTax: true);

			var displayTotal = orderTotal - taxTotal - shippingTotal;

			var discountTotal = Math.Round(displayTotal - subtotal, 2);

			var miniCartData = new MinicartAjaxData(
				cartCount: newCart.CartItems.Sum(item => item.Quantity),
				minicartContentsHtml: ControllerContext.GetHtmlHelper().Partial(viewName, cartViewModel).ToString(),
				messages: messages ?? Enumerable.Empty<AjaxNotice>(),
				subTotal: Localization.CurrencyStringForDisplayWithExchangeRate(
					subtotal,
					customer.CurrencySetting),
				discount: discountTotal != 0
					? Localization.CurrencyStringForDisplayWithExchangeRate(
						discountTotal,
						customer.CurrencySetting)
					: null,
				total: Localization.CurrencyStringForDisplayWithExchangeRate(
					displayTotal,
					customer.CurrencySetting)
			);

			return miniCartData;
		}

		AddToCartPostModel BuildAddToCartPostModelFromItem(CartItem cartItem)
		{
			return new AddToCartPostModel
			{
				ProductId = cartItem.ProductID,
				VariantId = cartItem.VariantID,
				Color = cartItem.ChosenColor,
				Size = cartItem.ChosenSize,
				TextOption = cartItem.TextOption,
				Quantity = cartItem.Quantity,
				IsWishlist = false
			};
		}

		public class MinicartAjaxData
		{
			public readonly int CartCount;
			public readonly string SubTotal;
			public readonly string Discount;
			public readonly string Total;
			public readonly string MinicartContentsHtml;
			public readonly IEnumerable<AjaxNotice> Messages;

			public MinicartAjaxData(
				int cartCount, 
				string subTotal,
				string discount, 
				string total,
				string minicartContentsHtml, 
				IEnumerable<AjaxNotice> messages = null)
			{
				CartCount = cartCount;
				SubTotal = subTotal;
				Discount = discount;
				Total = total;
				MinicartContentsHtml = minicartContentsHtml;
				Messages = messages;
			}
		}

		public class AjaxAddToCartData
		{
			public readonly string Status;
			public readonly IEnumerable<AjaxNotice> Messages;
			public readonly MinicartAjaxData MinicartData;

			public AjaxAddToCartData(string status, IEnumerable<AjaxNotice> messages = null, MinicartAjaxData minicartData = null)
			{
				Status = status;
				Messages = messages;
				MinicartData = minicartData;
			}
		}

		public class AjaxDualMiniData
		{
			public readonly MinicartAjaxData Cart;
			public readonly MinicartAjaxData Wish;

			public AjaxDualMiniData(MinicartAjaxData cart, MinicartAjaxData wish)
			{
				Cart = cart;
				Wish = wish;
			}
		}

	}
}
