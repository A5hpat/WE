// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Linq;
using System.Web.Mvc;
using AspDotNetStorefront.Models;
using AspDotNetStorefrontCore;
using AspDotNetStorefront.Routing;
using AspDotNetStorefront.Filters;
using AspDotNetStorefront.Caching.ObjectCaching;

namespace AspDotNetStorefront.Controllers
{
	[SecureAccessFilter(forceHttps: true)]
	public class CheckoutGiftCardEntryController : Controller
	{
		readonly ICachedShoppingCartProvider CachedShoppingCartProvider;

		public CheckoutGiftCardEntryController(ICachedShoppingCartProvider cachedShoppingCartProvider)
		{
			CachedShoppingCartProvider = cachedShoppingCartProvider;
		}

		[ChildActionOnly, ImportModelStateFromTempData]
		public ActionResult AddGiftCard()
		{
			var customer = HttpContext.GetCustomer();
			var cart = CachedShoppingCartProvider.Get(customer, CartTypeEnum.ShoppingCart, AppLogic.StoreID());
			
			var model = new GiftCardViewModel
			{
				Code = cart.Coupon.CouponCode
			};

			return PartialView(ViewNames.AddGiftCardPartial, model);
		}

		[HttpPost, ExportModelStateToTempData]
		public ActionResult AddGiftCard(GiftCardViewModel model)
		{
			if(!ModelState.IsValid)
				return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);

			var customer = HttpContext.GetCustomer();

			var coupons = Coupons.LoadCoupons(model.Code, customer.StoreID);
			if (!coupons.Any())
			{
				ModelState.AddModelError("Code", "Gift card not found.");
				return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
			}

			var cart = CachedShoppingCartProvider.Get(customer, CartTypeEnum.ShoppingCart, AppLogic.StoreID());
			cart.SetCoupon(model.Code, true);

			//If the gift card covers the whole order, do some setup that Gateway.MakeOrder requires
			if(cart.GiftCardCoversTotal())
				customer.UpdateCustomer(requestedPaymentMethod: AppLogic.ro_PMCreditCard);

			return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
		}

		public ActionResult RemoveGiftCard()
		{
			var customer = HttpContext.GetCustomer();
			var cart = CachedShoppingCartProvider.Get(customer, CartTypeEnum.ShoppingCart, AppLogic.StoreID());
			cart.SetCoupon(string.Empty, true);

			return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
		}
	}
}
