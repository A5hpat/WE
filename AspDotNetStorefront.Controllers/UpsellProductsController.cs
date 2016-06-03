// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using AspDotNetStorefront.Caching.ObjectCaching;
using AspDotNetStorefront.Models;
using AspDotNetStorefront.Routing;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Controllers
{
	public class UpsellProductsController : Controller
	{
		readonly ICachedShoppingCartProvider CachedShoppingCartProvider;

		public UpsellProductsController(ICachedShoppingCartProvider cachedShoppingCartProvider)
		{
			CachedShoppingCartProvider = cachedShoppingCartProvider;
		}

		[HttpGet]
		public ActionResult ShoppingCartUpsells()
		{
			if(!AppLogic.AppConfigBool("ShowUpsellProductsOnCartPage"))
				return Content(string.Empty);

			var customer = HttpContext.GetCustomer();

			var upsellProducts = new List<UpsellProductViewModel>();
			using(var connection = new SqlConnection(DB.GetDBConn()))
			{
				using(var command = connection.CreateCommand())
				{
					command.CommandText = @"exec aspdnsf_GetUpsellProductsForCart @customerId, @customerlevelId, @invFilter, @storeID, @upsellProductsLimitNumberOnCart, @filterProduct";
					command.Parameters.AddWithValue("customerId", customer.CustomerID);
					command.Parameters.AddWithValue("customerlevelId", customer.CustomerLevelID);
					command.Parameters.AddWithValue("invFilter", AppLogic.AppConfigNativeInt("HideProductsWithLessThanThisInventoryLevel"));
					command.Parameters.AddWithValue("storeID", AppLogic.StoreID());
					command.Parameters.AddWithValue("upsellProductsLimitNumberOnCart", AppLogic.AppConfigNativeInt("UpsellProductsLimitNumberOnCart"));
					command.Parameters.AddWithValue("filterProduct", AppLogic.GlobalConfigBool("AllowProductFiltering"));
					connection.Open();

					using(var reader = command.ExecuteReader())
					{
						while(reader.Read())
						{
							var productId = DB.RSFieldInt(reader, "ProductId");
							var name = XmlCommon.GetLocaleEntry(DB.RSField(reader, "Name"), customer.LocaleSetting, true);
							var seAltText = DB.RSField(reader, "SeAltText");
							var seName = DB.RSField(reader, "SeName");
							var price = DB.RSFieldDecimal(reader, "Price");
							var salePrice = DB.RSFieldDecimal(reader, "SalePrice");
							var extendedPrice = DB.RSFieldDecimal(reader, "ExtendedPrice");
							var taxClassId = DB.RSFieldInt(reader, "TaxClassId");
							var upsellDiscountMultiplier = DB.RSFieldDecimal(reader, "UpsellDiscMultiplier");


							var upsellProduct = new UpsellProductViewModel
							{
								ProductId = productId,
								Name = name,
								AltText = !string.IsNullOrEmpty(seAltText)
									? seAltText
									: name,
								ProductLink = Url.BuildProductLink(productId, seName),
								ImageUrl = AppLogic.LookupImage(
									EntityOrObjectName: "product",
									ID: productId,
									ImgSize: "icon",
									SkinID: customer.SkinID,
									LocaleSetting: customer.LocaleSetting),
								DisplayPrice = Localization.CurrencyStringForDisplayWithExchangeRate(
									amt: Prices.GetUpsellPrice(
										customer: customer, 
										price: price,
										salePrice: salePrice, 
										extendedPrice: extendedPrice, 
										taxClassId: taxClassId, 
										upsellDiscountPercent: upsellDiscountMultiplier),
									TargetCurrencyCode: customer.CurrencySetting),
								Selected = false
							};
							upsellProducts.Add(upsellProduct);
						}
					}
				}
			}

			if(!upsellProducts.Any())
				return Content(string.Empty);

			var model = new UpsellProductsViewModel
			{
				UpsellProducts = upsellProducts
			};

			return PartialView(model: model, viewName: ViewNames.ShoppingCartUpsellsPartial);
		}

		[HttpPost]
		public ActionResult ShoppingCartUpsells(UpsellProductsViewModel model)
		{
			var customer = HttpContext.GetCustomer();
			var cart = CachedShoppingCartProvider.Get(customer, CartTypeEnum.ShoppingCart, AppLogic.StoreID());

			foreach(var upsellItem in model.UpsellProducts.Where(item => item.Selected))
			{
				if(upsellItem.ProductId == 0)
					continue;

				var variantId = AppLogic.GetProductsDefaultVariantID(upsellItem.ProductId);
				if(variantId == 0)
					continue;

				var newCartRecord = cart.AddItem(
					customer: customer, 
					shippingAddressId: customer.PrimaryShippingAddressID, 
					productId: upsellItem.ProductId, 
					variantId: variantId, 
					quantity: 1, 
					chosenColor: string.Empty, 
					chosenColorSkuModifier: string.Empty, 
					chosenSize: string.Empty, 
					chosenSizeSkuModifier: string.Empty, 
					textOption: string.Empty, 
					cartType: CartTypeEnum.ShoppingCart,
					updateCartObject: true,
					isRequired: false, 
					customerEnteredPrice: decimal.Zero);

				var price = AppLogic.GetUpsellProductPrice(0, upsellItem.ProductId, customer.CustomerLevelID);
				DB.ExecuteSQL(
					"update shoppingcart set IsUpsell=1, ProductPrice= @price where ShoppingCartRecID = @cartRecID",
					new SqlParameter("price", price),
					new SqlParameter("cartRecID", newCartRecord));
			}

			return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
		}
	}
}
