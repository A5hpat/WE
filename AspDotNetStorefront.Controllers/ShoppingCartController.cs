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
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using AspDotNetStorefront.Caching.ObjectCaching;
using AspDotNetStorefront.Classes;
using AspDotNetStorefront.Classes.Checkout;
using AspDotNetStorefront.Controllers.Classes;
using AspDotNetStorefront.Filters;
using AspDotNetStorefront.Models;
using AspDotNetStorefront.Routing;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Controllers
{
	public partial class ShoppingCartController : Controller
	{
		readonly NoticeProvider NoticeProvider;
		readonly CartActionProvider CartActionProvider;
		readonly ICachedShoppingCartProvider CachedShoppingCartProvider;
		readonly UrlHelper UrlHelper;
		readonly ModelBuilder ModelBuilder;
		readonly RestrictedQuantityProvider RestrictedQuantityProvider;


		public ShoppingCartController(
			NoticeProvider noticeProvider,
			CartActionProvider cartActionProvider,
			ICachedShoppingCartProvider cachedShoppingCartProvider,
			UrlHelper urlHelper,
			ModelBuilder modelBuilder,
			RestrictedQuantityProvider restrictedQuantityProvider)
		{
			NoticeProvider = noticeProvider;
			CartActionProvider = cartActionProvider;
			CachedShoppingCartProvider = cachedShoppingCartProvider;
			UrlHelper = urlHelper;
			ModelBuilder = modelBuilder;
			RestrictedQuantityProvider = restrictedQuantityProvider;
		}

		[HttpGet]
		public ActionResult AddToCartForm(
			int? productId = 0,
			int? variantId = 0,
			int? cartRecordId = null,
			bool? showWishlistButton = null,
			string selectedSize = null,
			string selectedColor = null,
			bool? colorSelectorChangesImage = null)
		{
			if(variantId == 0)
				variantId = AppLogic.GetDefaultProductVariant(productId ?? 0);

			if(!productId.HasValue || productId.Value == 0)
				productId = AppLogic.GetVariantProductID(variantId ?? 0);

			if(colorSelectorChangesImage == null)
				colorSelectorChangesImage = false;

			var customer = HttpContext.GetCustomer();
			var variant = new ProductVariant(variantId ?? 0);
			var product = new Product(productId.Value);

			// Make sure we should show the add to cart form
			if(product.ProductID == 0 
				|| !product.Published 
				|| product.Deleted
				|| variant.VariantID == 0
				|| !variant.Published
				|| variant.Deleted)
				return Content("The specified product is not available");

			var cartItem = GetCartItem(cartRecordId, variantId ?? 0);

			var model = ModelBuilder.BuildAddToCartViewModel(
				urlHelper: UrlHelper,
				variant: variant,
				product: product,
				cartItem: cartItem,
				customer: customer,
				showWishlistButton: showWishlistButton ?? true,
				colorSelectorChangesImage: colorSelectorChangesImage.Value,
				selectedSize: selectedSize ?? string.Empty,
				selectedColor: selectedColor ?? string.Empty);

			return PartialView(ViewNames.AddToCartFormPartial, model);
		}

		[HttpGet]
		public ActionResult SimpleAddToCartForm(
			int productId,
			bool? showWishlistButton = null)
		{
			var variantId = AppLogic.GetDefaultProductVariant(productId);

			if(productId == 0 || variantId == 0)
				throw new Exception("Make sure to specify a product or variant id");

			var customer = HttpContext.GetCustomer();
			var variant = new ProductVariant(variantId);
			var product = new Product(productId);

			var model = ModelBuilder.BuildAddToCartViewModel(
				urlHelper: UrlHelper,
				variant: variant,
				product: product,
				cartItem: null,
				customer: customer,
				showWishlistButton: showWishlistButton ?? true,
				colorSelectorChangesImage: false,
				selectedSize: string.Empty,
				selectedColor: string.Empty);

			return PartialView(ViewNames.SimpleAddToCartFormPartial, model);
		}

		CartItem GetCartItem(int? cartRecordId, int variantId)
		{
			if(cartRecordId == null)
				return null;

			var cartItem = new CartItem();

			using(SqlConnection connection = new SqlConnection(DB.GetDBConn()))
			{
				var sql = @"select ShoppingCartRecID, TextOption, Quantity, ChosenSize, ChosenColor, ProductPrice, CartType 
							from ShoppingCart 
							where ShoppingCartRecID = @CartRecId and VariantID = @VariantId";

				var parameters = new[] {
					new SqlParameter("CartRecId", cartRecordId),
					new SqlParameter("VariantId", variantId),
				};

				connection.Open();

				using(IDataReader rscart = DB.GetRS(sql, parameters, connection))
				{
					if(rscart.Read())
					{
						cartItem.ShoppingCartRecordID = DB.RSFieldInt(rscart, "ShoppingCartRecID");
						cartItem.TextOption = DB.RSField(rscart, "TextOption");
						cartItem.Quantity = DB.RSFieldInt(rscart, "Quantity");
						cartItem.ChosenSize = DB.RSFieldByLocale(rscart, "ChosenSize", Localization.GetDefaultLocale()) ?? string.Empty;
						cartItem.ChosenColor = DB.RSFieldByLocale(rscart, "ChosenColor", Localization.GetDefaultLocale()) ?? string.Empty;
						cartItem.Price = DB.RSFieldDecimal(rscart, "ProductPrice");
						cartItem.CartType = (CartTypeEnum)DB.RSFieldInt(rscart, "CartType");
						return cartItem;
					}
				}
			}
			return null;
		}

		[HttpGet]
		[RequireCustomerRecordFilter]
		public ActionResult AddToCart(
			int productId = 0,
			int variantId = 0,
			int? cartRecordId = null,
			int quantity = 1,
			decimal customerEnteredPrice = 0,
			string color = "",
			string size = "",
			string textOption = "",
			string upsellProducts = "",
			bool isWishlist = false,
			string returnUrl = "")
		{
			if(variantId == 0)
				variantId = AppLogic.GetDefaultProductVariant(productId);

			if(productId == 0)
				productId = AppLogic.GetVariantProductID(variantId);

			if(productId == 0 || variantId == 0)
				throw new Exception("Make sure to specify a product or variant id");

			var customer = HttpContext.GetCustomer();
			var variant = new ProductVariant(variantId);
			var product = new Product(productId);

			var model = new AddToCartPostModel
			{
				ProductId = productId,
				VariantId = variantId,
				CartRecordId = cartRecordId,
				Quantity = quantity,
				CustomerEnteredPrice = customerEnteredPrice,
				Color = color,
				Size = size,
				TextOption = textOption,
				UpsellProducts = upsellProducts,
				IsWishlist = isWishlist,
				ReturnUrl = returnUrl
			};

			return AddToCart(model);
		}

		[HttpPost]
		[RequireCustomerRecordFilter]
		public ActionResult AddToCart(AddToCartPostModel model)
		{
			if(!ModelState.IsValid)
				return RedirectToAction(ActionNames.Index, ControllerNames.Minicart);

			var customer = HttpContext.GetCustomer();
			var notices = new List<Notice>();

			var cartType = model.IsWishlist
				? CartTypeEnum.WishCart
				: CartTypeEnum.ShoppingCart;

			// Use the provided return URL if there is one, otherwise try to use the referrer
			string unverifiedReturnUrl;
			if(!string.IsNullOrWhiteSpace(model.ReturnUrl))
				unverifiedReturnUrl = model.ReturnUrl;
			else if(Request.UrlReferrer != null)
				unverifiedReturnUrl = Request.UrlReferrer.ToString();
			else
				unverifiedReturnUrl = null;

			// Ensure we have a safe return URL
			var returnUrl = UrlHelper.MakeSafeReturnUrl(
				returnUrl: unverifiedReturnUrl,
				defaultUrl: cartType == CartTypeEnum.WishCart
					? Url.Action(ActionNames.Index, ControllerNames.Wishlist)
					: Url.Action(ActionNames.Index, ControllerNames.Checkout));

			// Make request, check status
			var response = CartActionProvider.AddItemToCart(model.ConvertToAddToCartContext(
				customer,
				model.IsWishlist
					? CartTypeEnum.WishCart
					: CartTypeEnum.ShoppingCart));
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
				case CartActionStatus.ValidationErrors:
					{
						foreach(var message in response.Messages)
							NoticeProvider.PushNotice(message.ConvertToNotice());
						return Redirect(returnUrl);
					}
				default:
					break;
			}

			// Add any returned notices
			if(response.Messages.Any())
				foreach(var message in response.Messages)
					NoticeProvider.PushNotice(message.ConvertToNotice());
			else
				NoticeProvider.PushNotice("Item successfully added to cart.", NoticeType.Success);

			// Redirect accordingly
			if(AppLogic.AppConfig("AddToCartAction").Equals("STAY", StringComparison.OrdinalIgnoreCase))
				return Redirect(returnUrl);
			
			return RedirectToAction(
				ActionNames.Index,
				ControllerNames.Checkout,
				new { returnUrl });

		}

		[HttpGet]
		public ActionResult BulkAddToCartForm(int productId = 0, int variantId = 0, int? cartRecordId = null, bool showWishlistButton = true)
		{
			if(variantId == 0)
				variantId = AppLogic.GetDefaultProductVariant(productId);

			if(productId == 0)
				productId = AppLogic.GetVariantProductID(variantId);

			if(productId == 0 || variantId == 0)
				throw new Exception("Make sure to specify a product or variant id");

			var customer = HttpContext.GetCustomer();
			var variant = new ProductVariant(variantId);
			var product = new Product(productId);

			if(product.IsCalltoOrder)
				return PartialView(ViewNames.CallToOrderPartial);

			var cartItem = GetCartItem(cartRecordId, variantId);

			var model = ModelBuilder.BuildAddToCartViewModel(
				urlHelper: UrlHelper,
				variant: variant,
				product: product,
				cartItem: cartItem,
				customer: customer,
				showWishlistButton: showWishlistButton,
				colorSelectorChangesImage: false,
				defaultQuantity: 0);

			//Setup a prefix on the html form fields
			ViewData.TemplateInfo.HtmlFieldPrefix = string.Format("AddToCartItems_{0}_{1}", productId, variantId);

			return PartialView(ViewNames.BulkAddToCartForm, model);
		}

		[HttpPost]
		[RequireCustomerRecordFilter]
		public ActionResult BulkAddToCart([ModelBinder(typeof(BulkAddToCartModelBinder))]BulkAddToCartPostModel model)
		{
			if(!ModelState.IsValid)
				return RedirectToAction(ActionNames.Index, ControllerNames.Minicart);

			if(!model.AddToCartItems.Any())
				return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);

			var customer = HttpContext.GetCustomer();
			var notices = new List<Notice>();
			var cartType = model.AddToCartItems.First().IsWishlist
				? CartTypeEnum.WishCart
				: CartTypeEnum.ShoppingCart;

			// Use the provided return URL if there is one, otherwise try to use the referrer
			string unverifiedReturnUrl;
			if(!string.IsNullOrWhiteSpace(model.ReturnUrl))
				unverifiedReturnUrl = model.ReturnUrl;
			else if(Request.UrlReferrer != null)
				unverifiedReturnUrl = Request.UrlReferrer.ToString();
			else
				unverifiedReturnUrl = null;

			// Ensure we have a safe return URL
			var returnUrl = UrlHelper.MakeSafeReturnUrl(
				returnUrl: unverifiedReturnUrl,
				defaultUrl: cartType == CartTypeEnum.WishCart
					? Url.Action(ActionNames.Index, ControllerNames.Wishlist)
					: Url.Action(ActionNames.Index, ControllerNames.Checkout));

			// Iterate the submitted items
			foreach(var addToCartItem in model.AddToCartItems)
			{
				// Make the request, check status
				var response = CartActionProvider.AddItemToCart(addToCartItem.ConvertToAddToCartContext(
					customer,
					model.AddToCartItems.First().IsWishlist
						? CartTypeEnum.WishCart
						: CartTypeEnum.ShoppingCart));
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
					case CartActionStatus.ValidationErrors:
						{
							foreach(var message in response.Messages)
								NoticeProvider.PushNotice(message.ConvertToNotice());
							return Redirect(returnUrl);
						}
					default:
						break;
				}

				// Add any returned notices
				if(response.Messages.Any())
					foreach(var message in response.Messages)
						NoticeProvider.PushNotice(message.ConvertToNotice());
			}

			if(!notices.Any())
				NoticeProvider.PushNotice("Item successfully added to cart.", NoticeType.Success);

			// Redirect accordingly
			if(AppLogic.AppConfig("AddToCartAction").Equals("STAY", StringComparison.OrdinalIgnoreCase))
				return Redirect(returnUrl);

			return RedirectToAction(
				ActionNames.Index,
				ControllerNames.Checkout,
				new { returnUrl });

		}

		[HttpGet]
		[ChildActionOnly]
		public ActionResult KitAddToCartForm(int? productId = null, int? variantId = null, int? cartRecordId = null)
		{
			var product = new Product(productId ?? 0);

			if(variantId == null || variantId == 0)
				variantId = AppLogic.GetDefaultProductVariant(product.ProductID);

			var variant = new ProductVariant(variantId.Value);

			var customer = HttpContext.GetCustomer();

			if(!product.IsAKit)
				throw new Exception(string.Format("product {0} is not a kit and so we cannot show the kit add to cart form", product.ProductID));

			var kitData = KitProductData.Find(
				productId: product.ProductID,
				cartRecId: cartRecordId.HasValue
					? cartRecordId.Value
					: 0,
				thisCustomer: customer);

			var quantity = GetKitQuantity(cartRecordId, variant.MinimumQuantity);

			if(kitData == null)
				return Content("Details for this kit have not been setup");

			var kitAddToCartViewModel = BuildDefaultKitAddToCartViewModel(kitData, product, variant, quantity, customer, cartRecordId);

			return PartialView(ViewNames.KitAddToCartFormPartial, kitAddToCartViewModel);
		}

		KitAddToCartViewModel BuildDefaultKitAddToCartViewModel(
			KitProductData kitData,
			Product product,
			ProductVariant variant,
			int quantity,
			Customer customer,
			int? cartRecordId)
		{
			var cartType = cartRecordId == null
				? CartTypeEnum.ShoppingCart // we won't use this, so what it's set to doesn't matter
				: CachedShoppingCartProvider.Get(customer, CartTypeEnum.ShoppingCart, AppLogic.StoreID())
				.CartItems
				.Where(c => c.ShoppingCartRecordID == cartRecordId)
				.Any()
				? CartTypeEnum.ShoppingCart
				: CartTypeEnum.WishCart;

			var isUnorderable = false;
			if(!AppLogic.AppConfigBool("KitInventory.AllowSaleOfOutOfStock")
				&& (!kitData.HasStock || kitData.HasRequiredOrReadOnlyButUnOrderableKitGroup))
			{
				isUnorderable = true;
			}

			var isCallToOrder = product.IsCalltoOrder;

			kitData.ComputePrices(quantity);

			var kitGroups = new List<KitGroupViewModel>();
			foreach(var group in kitData.Groups)
			{
				var kitItems = new List<KitItemViewModel>();
				foreach(var item in group.Items)
				{
					var priceDeltaDisplay = FormatCurrencyDisplay(item.PriceDelta, customer, variant.IsTaxable);
					var showOutOfStock = item.HasMappedVariant
						&& !item.VariantHasStock
						&& !AppLogic.AppConfigBool("KitInventory.AllowSaleOfOutOfStock")
						&& AppLogic.AppConfigBool("KitInventory.ShowOutOfStockMessage");

					var kitItemRelativePriceDeltaDisplayText = KitItemRelativePriceDeltaDisplayText(item, customer.CurrencySetting, variant.IsTaxable, kitData);

					kitItems.Add(new KitItemViewModel
					{
						Name = item.Name,
						NameDisplay = !string.IsNullOrEmpty(kitItemRelativePriceDeltaDisplayText)
							? string.Format("{0} [{1}]", item.Name, kitItemRelativePriceDeltaDisplayText)
							: item.Name,
						Description = item.Description,
						IsDefault = item.IsDefault,
						PriceDelta = item.PriceDelta,
						PriceDeltaDisplay = priceDeltaDisplay,
						WeightDelta = item.WeightDelta,
						DisplayOrder = item.DisplayOrder,
						Id = item.Id,
						IsSelected = item.IsSelected,
						TextOption = item.TextOption,
						ImageUrl = item.HasImage
							? item.ImagePath
							: string.Empty,
						OutOfStockMessage = showOutOfStock
							? "Out of stock"
                            : string.Empty
					});
				}

				var selectedItemId = group.FirstSelectedItem != null
					? group.FirstSelectedItem.Id
					: 0;

				var groupViewModel = new KitGroupViewModel
				{
					Id = group.Id,
					Name = group.Name,
					Description = group.Description,
					Summary = group.Summary,
					ImageUrl = group.HasImage
							? group.ImagePath
							: string.Empty,
					KitGroupType = group.SelectionControl,
					Items = kitItems,
					SelectedItemId = selectedItemId,
					IsRequired = group.IsRequired,
					IsReadOnly = group.IsReadOnly
				};

				kitGroups.Add(groupViewModel);
			}

			var regularBasePrice = kitData.Price;
			var basePrice = kitData.BasePrice + kitData.ReadOnlyItemsSum;
			var customizedPrice = kitData.CustomizedPrice;
			var customerLevelPrice = kitData.LevelPrice;

			PayPalAd payPalAd = null;
			// If this is a single variant product, setup the PayPal ad.
			if(AppLogic.GetNextVariant(product.ProductID, variant.VariantID) == variant.VariantID)
				payPalAd = new PayPalAd(PayPalAd.TargetPage.Product);

			// Values need for the Schema.Org tags              
			var storeDefaultCultureInfo = CultureInfo.GetCultureInfo(Localization.GetDefaultLocale());
			var formattedSchemaPrice = string.Format(storeDefaultCultureInfo, "{0:C}", kitData.BasePrice);
			var schemaRegionInfo = new RegionInfo(storeDefaultCultureInfo.Name);

			// Build the view model
			return new KitAddToCartViewModel(
				isUnorderable: isUnorderable,
				isCallToOrder: isCallToOrder,
				regularBasePrice: FormatCurrencyDisplay(regularBasePrice, customer, variant.IsTaxable),
				basePrice: FormatCurrencyDisplay(basePrice, customer, variant.IsTaxable),
				customizedPrice: FormatCurrencyDisplay(customizedPrice, customer, variant.IsTaxable),
				customerLevelPrice: FormatCurrencyDisplay(customerLevelPrice, customer, variant.IsTaxable),
				showRegularBasePrice: kitData.IsDiscounted,
				showBasePrice: !AppLogic.AppConfigBool("HideKitPrice"),
				showCustomerLevelPrice: kitData.HasCustomerLevelPricing,
				showQuantity: AppLogic.AppConfigBool("ShowQuantityOnProductPage") &&
					!AppLogic.AppConfigBool("HideKitQuantity"),
				showBuyButton: AppLogic.AppConfigBool("ShowBuyButtons")
					&& product.ShowBuyButton
					&& !AppLogic.HideForWholesaleSite(customer.CustomerLevelID),
				showWishlistButton: AppLogic.AppConfigBool("ShowWishButtons"),
				hidePriceUntilCart: product.HidePriceUntilCart,
				restrictedQuantities: RestrictedQuantityProvider.BuildRestrictedQuantityList(variant.RestrictedQuantities),
				payPalAd: payPalAd,
				showSchemaOrgPrice: kitData.BasePrice > 0 && !AppLogic.AppConfigBool("HideKitPrice"),
				schemaBasePrice: formattedSchemaPrice,
				isoThreeLetterCurrency: schemaRegionInfo.ISOCurrencySymbol,
				schemaProductUrl: string.Format("{0}://schema.org/Product", Request.Url.Scheme),
				schemaOfferUrl: string.Format("{0}://schema.org/Offer", Request.Url.Scheme),
				cartType: cartType)
			{
				ProductId = product.ProductID,
				VariantId = variant.VariantID,
				CartRecordId = cartRecordId,
				Quantity = quantity,
				KitGroups = kitGroups,

				UpsellProducts = null,
				IsWishlist = false,
				ReturnUrl = UrlHelper.MakeSafeReturnUrl(Request.RawUrl),
				TemporaryImageNameStub = Guid.NewGuid().ToString()
			};
		}

		int GetKitQuantity(int? cartRecordId, int minimumQuantity)
		{
			if(cartRecordId.HasValue)
				return GetQuantityFromCart(cartRecordId.Value);

			if(minimumQuantity > 0)
				return minimumQuantity;

			return AppLogic.AppConfigUSInt("DefaultAddToCartQuantity") > 0
				? AppLogic.AppConfigUSInt("DefaultAddToCartQuantity")
				: 1;
		}

		[HttpPost]
		[RequireCustomerRecordFilter]
		public ActionResult KitAddToCart(KitAddToCartPostModel model)
		{
			if(!ModelState.IsValid)
				return RedirectToAction(ActionNames.Index, ControllerNames.Minicart);

			var customer = HttpContext.GetCustomer();
			var notices = new List<Notice>();

			var cartType = model.IsWishlist
				? CartTypeEnum.WishCart
				: CartTypeEnum.ShoppingCart;

			// Use the provided return URL if there is one, otherwise try to use the referrer
			string unverifiedReturnUrl;
			if(!string.IsNullOrWhiteSpace(model.ReturnUrl))
				unverifiedReturnUrl = model.ReturnUrl;
			else if(Request.UrlReferrer != null)
				unverifiedReturnUrl = Request.UrlReferrer.ToString();
			else
				unverifiedReturnUrl = null;

			// Ensure we have a safe return URL
			var returnUrl = UrlHelper.MakeSafeReturnUrl(
				returnUrl: unverifiedReturnUrl,
				defaultUrl: cartType == CartTypeEnum.WishCart
					? Url.Action(ActionNames.Index, ControllerNames.Wishlist)
					: Url.Action(ActionNames.Index, ControllerNames.Checkout));

			// Make request, check status
			var response = CartActionProvider.AddItemToCart(
				model.ConvertToAddToCartContext(
					customer,
					cartType));

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
				case CartActionStatus.ValidationErrors:
					{
						foreach(var message in response.Messages)
							NoticeProvider.PushNotice(message.ConvertToNotice());
						return Redirect(returnUrl);
					}
				default:
					break;
			}

			// Add any returned notices
			if(response.Messages.Any())
				foreach(var message in response.Messages)
					NoticeProvider.PushNotice(message.ConvertToNotice());
			else
				NoticeProvider.PushNotice("Item successfully added to cart.", NoticeType.Success);

			// Redirect accordingly
			if(AppLogic.AppConfig("AddToCartAction").Equals("STAY", StringComparison.OrdinalIgnoreCase))
				return Redirect(returnUrl);

			return RedirectToAction(
				ActionNames.Index,
				ControllerNames.Checkout,
				new { returnUrl });

		}

		string FormatCurrencyDisplay(decimal amount, Customer customer, bool isTaxable)
		{
			var displayText = Localization.CurrencyStringForDisplayWithExchangeRate(amount, customer.CurrencySetting);

			if(!AppLogic.AppConfigBool("VAT.Enabled") || !isTaxable)
				return displayText;

			var vatSuffix = customer.VATSettingReconciled == VATSettingEnum.ShowPricesInclusiveOfVAT
				? "inc vat" // Vat inclusive
                : "Excluding VAT"; // Vat exclusive

			displayText = string.Format("{0} {1}", displayText, vatSuffix);
			return displayText;
		}

		int GetQuantityFromCart(int shoppingCartRecId)
		{
			using(var connection = new SqlConnection(DB.GetDBConn()))
			using(var command = connection.CreateCommand())
			{
				command.CommandText = "SELECT Quantity FROM ShoppingCart WHERE ShoppingCartRecId = @CartRecId";
				command.Parameters.AddWithValue("CartRecId", shoppingCartRecId);
				connection.Open();
				using(var reader = command.ExecuteReader())
				{
					if(reader.Read())
						return reader.FieldInt("Quantity");
				}
				return 1;
			}
		}

		[HttpPost]
		public ActionResult AjaxGetKitData(KitAddToCartPostModel model)
		{
			var product = new Product(model.ProductId);

			if(model.VariantId == 0)
				model.VariantId = AppLogic.GetDefaultProductVariant(model.ProductId);

			var variant = new ProductVariant(model.VariantId);

			var customer = HttpContext.GetCustomer();
			var cartType = model.IsWishlist
				? CartTypeEnum.WishCart
				: CartTypeEnum.ShoppingCart;

			if(!product.IsAKit)
				return Content(string.Empty);

			KitProductData kitData = KitProductData.Find(
				productId: product.ProductID,
				cartRecId: model.CartRecordId.HasValue
					? model.CartRecordId.Value
					: 0,
				thisCustomer: customer);

			// Manipulate the default selections to reflect the selections made so far
			var selectedItems = model.GetSelectedItems(kitData);
			foreach(var item in kitData.Groups.SelectMany(g => g.Items))
			{
				if(selectedItems.Contains(item))
					item.IsSelected = true;
				else
					item.IsSelected = false;
			}

			var quantity = Math.Max(model.Quantity, 1);

			var kitAddToCartViewModel = BuildDefaultKitAddToCartViewModel(kitData, product, variant, quantity, customer, model.CartRecordId);

			var itemDisplayNames = kitAddToCartViewModel.KitGroups
				.SelectMany(group => group.Items)
				.ToDictionary(item => item.Id.ToString(), item => item.NameDisplay);

			return Json(
				new AjaxKitDataViewModel
				{
					SummaryHtml = ControllerContext.GetHtmlHelper().Partial(
						partialViewName: ViewNames.KitSummaryPartial,
						model: kitAddToCartViewModel).ToString(),
					ItemDisplayNames = itemDisplayNames
				});
		}

		public string KitItemRelativePriceDeltaDisplayText(KitItemData item, string currencySetting, bool isTaxable, KitProductData kitData)
		{
			if(item.IsSelected)
				return null;

			var deltaText = item.RelativePriceDeltaIsAdd
				? "Add"
                : "Subtract";

			var customer = HttpContext.GetCustomer();

			var deltaDisplayFormat = string.Empty;
			var vatSuffix = customer.VATSettingReconciled == VATSettingEnum.ShowPricesInclusiveOfVAT
				? "inc vat" // Vat inclusive
                : "Excluding VAT"; // Vat exclusive

			var priceDelta = item.PriceDelta;

			if(item.Group.SelectionControl == KitGroupData.SINGLE_SELECT_DROPDOWN_LIST
				|| item.Group.SelectionControl == KitGroupData.SINGLE_SELECT_RADIO_LIST)
			{
				priceDelta = item.RelativePriceDelta;
			}
			
			if(priceDelta == 0)
				return null;

			if(kitData.ShowTaxInclusive)
			{
				var taxMultiplier = 1M + (kitData.TaxRate / 100M);
				priceDelta *= taxMultiplier;
			}

			deltaDisplayFormat = Localization.CurrencyStringForDisplayWithExchangeRate(Math.Abs(priceDelta), currencySetting);

			var displayText = string.Format("{0} {1}", deltaText, deltaDisplayFormat);
			if(AppLogic.AppConfigBool("VAT.Enabled") && isTaxable)
				displayText = string.Format("{0} {1}", displayText, vatSuffix);

			return displayText;
		}

		[HttpGet]
		public ActionResult DeleteItem(int id, string returnUrl)
		{
			var customer = HttpContext.GetCustomer();
			var notices = new List<Notice>();

			// Make request, check status
			var response = CartActionProvider.RemoveItemFromCart(customer, CartTypeEnum.ShoppingCart, id);

			foreach(var message in response.Messages)
				NoticeProvider.PushNotice(message.ConvertToNotice());

			switch(response.Status)
			{
				case CartActionStatus.Forbidden:
					throw new HttpException(403, "Forbidden");
				case CartActionStatus.RequiresLogin:
				case CartActionStatus.SessionTimeout:
					{
						return RedirectToAction(ActionNames.SignIn, ControllerNames.Account);
					}
				default:
					break;
			}

			return Redirect(
				UrlHelper.MakeSafeReturnUrl(returnUrl));
		}
	}
}
