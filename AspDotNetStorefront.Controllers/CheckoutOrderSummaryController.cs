// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web.Mvc;
using AspDotNetStorefront.Caching.ObjectCaching;
using AspDotNetStorefront.Models;
using AspDotNetStorefrontCore;
using AspDotNetStorefront.Routing;
using AspDotNetStorefront.Filters;

namespace AspDotNetStorefront.Controllers
{
	[SecureAccessFilter(forceHttps: true)]
	public class CheckoutOrderSummaryController : Controller
	{
		readonly ICachedShoppingCartProvider CachedShoppingCartProvider;

		public CheckoutOrderSummaryController(ICachedShoppingCartProvider cachedShoppingCartProvider)
		{
			CachedShoppingCartProvider = cachedShoppingCartProvider;
		}

		[ChildActionOnly]
		public ActionResult OrderSummary()
		{
			return PartialView(ViewNames.OrderSummaryPartial, BuildOrderSummaryViewModel());
		}

		OrderSummaryViewModel BuildOrderSummaryViewModel()
		{
			var customer = HttpContext.GetCustomer();
			var cart = CachedShoppingCartProvider.Get(customer, CartTypeEnum.ShoppingCart, AppLogic.StoreID());

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

			// Calculate lineitem and order level coupon and promotional discounts.
			var discountTotal = cart.Total(
				includeDiscount: true,
				roundBeforeTotaling: false) - taxTotal - shippingTotal - subtotal;

			var orderTotal = cart.Total(includeDiscount: true);

			// Calculate the discount for gift card.
			var giftCardDiscount = Decimal.Zero;
			if(cart.Coupon.CouponType == CouponTypeEnum.GiftCard)
				giftCardDiscount = decimal.Round(Currency.Convert(cart.Coupon.DiscountAmount, Localization.StoreCurrency(), customer.CurrencySetting), 2, MidpointRounding.AwayFromZero);

			// Ensure gift card discounts do not create a negative order total.
			orderTotal -= giftCardDiscount;
			if(orderTotal < 0)
				orderTotal = 0;

			var vatEnabled = AppLogic.AppConfigBool("VAT.Enabled");
			var shippingVatCaption = string.Empty;
			if(vatEnabled)
			{
				shippingVatCaption = "Excluding VAT";
				if(cart.VatIsInclusive)
					shippingVatCaption = "inc vat";
			}

			var currencySetting = customer.CurrencySetting;

			return new OrderSummaryViewModel
			{
				SubTotal = Localization.CurrencyStringForDisplayWithExchangeRate(subtotal, currencySetting),
				DiscountTotal = Localization.CurrencyStringForDisplayWithExchangeRate(discountTotal, currencySetting),
				ShippingTotal = Localization.CurrencyStringForDisplayWithExchangeRate(shippingTotal, currencySetting),
				ShippingVatCaption = string.Format("({0})", shippingVatCaption),
				TaxTotal = Localization.CurrencyStringForDisplayWithExchangeRate(taxTotal, currencySetting),
				HasGiftCardDiscountTotal = giftCardDiscount != 0,
				GiftCardDiscountTotal = Localization.CurrencyStringForDisplayWithExchangeRate(giftCardDiscount, currencySetting),
				Total = Localization.CurrencyStringForDisplayWithExchangeRate(orderTotal, currencySetting),
				HasDiscount = discountTotal != 0,
				ShowVatLabels = vatEnabled,
				ShowTax = !cart.VatIsInclusive
			};
		}
	}
}
