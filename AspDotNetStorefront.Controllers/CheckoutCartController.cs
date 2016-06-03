// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AspDotNetStorefront.Caching.ObjectCaching;
using AspDotNetStorefront.Classes.Checkout;
using AspDotNetStorefront.Filters;
using AspDotNetStorefront.Models;
using AspDotNetStorefront.Routing;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Controllers
{
	[SecureAccessFilter(forceHttps: true)]
	public class CheckoutCartController : Controller
	{
		readonly NoticeProvider NoticeProvider;
		readonly ICachedShoppingCartProvider CachedShoppingCartProvider;
		readonly CartActionProvider CartActionProvider;

		public CheckoutCartController(
			NoticeProvider noticeProvider,
			ICachedShoppingCartProvider cachedShoppingCartProvider,
			CartActionProvider cartActionProvider)
		{
			NoticeProvider = noticeProvider;
			CachedShoppingCartProvider = cachedShoppingCartProvider;
			CartActionProvider = cartActionProvider;
		}

		[SecureAccessFilter(forceHttps: true)]
		public ActionResult CheckoutCart()
		{
			var customer = HttpContext.GetCustomer();
			var cart = CachedShoppingCartProvider.Get(customer, CartTypeEnum.ShoppingCart, AppLogic.StoreID());
			var cartViewModel = GetShoppingCartViewModel(cart, customer);
			return PartialView(ViewNames.CheckoutCartPartial, cartViewModel);
		}

		[HttpPost]
		[SecureAccessFilter(forceHttps: true)]
		public ActionResult CheckoutCart([Bind(Include = "CartItems")] ShoppingCartViewModel cartViewModel)
		{
			if(!ModelState.IsValid)
				return RedirectToAction(ActionNames.CheckoutCart);

			var customer = HttpContext.GetCustomer();
			var notices = new List<Notice>();

			// Make request, check status
			var response = UpdateCartFromViewModel(cartViewModel, customer, CartTypeEnum.ShoppingCart);
			switch(response.Status)
			{
				case CartActionStatus.Forbidden:
					throw new HttpException(403, "Forbidden");
				case CartActionStatus.RequiresLogin:
				case CartActionStatus.SessionTimeout:
					{
						foreach(var message in response.Messages)
							NoticeProvider.PushNotice(message.ConvertToNotice());
						return RedirectToAction(ActionNames.SignIn, ControllerNames.Account);
					}
				default:
					break;
			}

			// Push any returned notices
			if(response.Messages.Any())
				foreach(var message in response.Messages)
					NoticeProvider.PushNotice(message.ConvertToNotice());
			
			return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
		}

		ShoppingCartViewModel GetShoppingCartViewModel(ShoppingCart cart, Customer customer)
		{
			var viewCartItems = new List<ShoppingCartItemViewModel>();
			foreach(var cartItem in cart.CartItems)
			{
				var recurringVariants = ProductVariant.GetVariants(cartItem.ProductID, false)
					.Where(pc => pc.IsRecurring
						&& pc.Published
						&& !pc.Deleted);
				var recurringIntervalOptions = new SelectList(recurringVariants, "VariantID", "LocaleName", cartItem.VariantID.ToString());

				var promotionText = cartItem.ThisShoppingCart.DiscountResults
					.Where(dr => dr.DiscountedItems
						.Where(di => di.ShoppingCartRecordId == cartItem.ShoppingCartRecordID)
						.Any())
					.Select(dr => dr.Promotion.UsageText)
					.FirstOrDefault();

				viewCartItems.Add(new ShoppingCartItemViewModel
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
					Price = cartItem.Price,
					PriceDisplay = cartItem.RegularPriceRateDisplayFormat,
					Notes = cartItem.Notes,
					Quantity = cartItem.Quantity,
					ProductUrl = Url.BuildProductLink(
						id: cartItem.ProductID,
						seName: cartItem.SEName),
					ProductImageUrl = cartItem.ProductPicURL,
					ProductImageAlternateText = cartItem.SEAltText,
                    // nal
                    //ShowCartImages = AppLogic.AppConfigBool("ShowPicsInCart"),
                    ShowCartImages = true,
					ShowRecurringDropDown = recurringVariants.Any() && AppLogic.AppConfigBool("AllowRecurringFrequencyChangeInCart"),
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
					ShowEditLink = true,
                    // nal
						//(AppLogic.AppConfigBool("ShowEditButtonInCartForKitProducts") && cartItem.IsAKit)
						//|| (AppLogic.AppConfigBool("ShowEditButtonInCartForRegularProducts") && !cartItem.IsAKit),
					EditUrl = string.Format(
						"{0}?cartRecordId={1}",
						Url.BuildProductLink(
							id: cartItem.ProductID,
							seName: cartItem.SEName),
						cartItem.ShoppingCartRecordID),
					RestrictedQuantities = new SelectList(cartItem.RestrictedQuantities),
					ShowSku = !cartItem.IsSystem 
						&& cartItem.SKU.Length > 0
				});
			}

            // nal
			var maximumCartItemsToDisplay = 0;
			//var maximumCartItemsToDisplay = AppLogic.AppConfigNativeInt("MaximumNumberOfCartItemsToDisplay");

			var cartViewModel = new ShoppingCartViewModel
			{
				CartItems = viewCartItems,
				MaximumCartItemsToDisplay = maximumCartItemsToDisplay,
				UseMaximumCartItemBehavor = maximumCartItemsToDisplay > 0 && viewCartItems.Count() > maximumCartItemsToDisplay,
				SubTotal = Localization.CurrencyStringForDisplayWithExchangeRate(
					cart.SubTotal(false, false, true, true, true, true),
					customer.CurrencySetting)
			};

			return cartViewModel;
		}

		CartActionResponse UpdateCartFromViewModel(ShoppingCartViewModel cartViewModel, Customer customer, CartTypeEnum cartType)
		{
			if(cartViewModel.CartItems == null)
				return null;

			var cart = CachedShoppingCartProvider.Get(customer, cartType, AppLogic.StoreID());
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
				updatedCart: response.UpdatedCart ?? cart,
				status: CartActionStatus.Success,
				messages: messages);
		}
	}
}
