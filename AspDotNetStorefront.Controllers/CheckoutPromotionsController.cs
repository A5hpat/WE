// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Linq;
using System.Web.Mvc;
using AspDotNetStorefront.Caching.ObjectCaching;
using AspDotNetStorefront.Filters;
using AspDotNetStorefront.Models;
using AspDotNetStorefrontCore;
using AspDotNetStorefront.Routing;

namespace AspDotNetStorefront.Controllers
{
	[SecureAccessFilter(forceHttps: true)]
	public class CheckoutPromotionsController : Controller
	{
		readonly ICachedShoppingCartProvider CachedShoppingCartProvider;

		public CheckoutPromotionsController(ICachedShoppingCartProvider cachedShoppingCartProvider)
		{
			CachedShoppingCartProvider = cachedShoppingCartProvider;
		}

		[ChildActionOnly, ImportModelStateFromTempData]
		public ActionResult AddPromoCode()
		{
			var customer = HttpContext.GetCustomer();
			var cart = CachedShoppingCartProvider.Get(customer, CartTypeEnum.ShoppingCart, AppLogic.StoreID());

			var promotions = !cart.PromotionsEnabled
				? Enumerable.Empty<PromotionViewModel>()
				: cart
					.DiscountResults
					.Where(discountResult => discountResult != null)
					.Select(discountResult => new PromotionViewModel(
						code: discountResult.Promotion.Code,
						description: discountResult.Promotion.Description));

			var model = new PromotionsViewModel(
				enteredPromoCode: string.Empty,
				promotions: promotions);

			return PartialView(ViewNames.AddPromoCodePartial, model);
		}

		[HttpPost, ExportModelStateToTempData]
		public ActionResult AddPromoCode(PromotionsViewModel model)
		{
			if(!ModelState.IsValid)
				return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);

			var promoCode = model.EnteredCode.ToLower();
			var customer = HttpContext.GetCustomer();
			var cart = CachedShoppingCartProvider.Get(customer, CartTypeEnum.ShoppingCart, AppLogic.StoreID());

			var validationResults = PromotionManager.ValidatePromotion(promoCode, PromotionManager.CreateRuleContext(cart));
			var validationFailureMessages = validationResults
				.Where(validationResult => !validationResult.IsValid)
				.SelectMany(validationResult => validationResult.Reasons)
				.Select(reason => new
				{
					message = AppLogic.GetString(reason.MessageKey),
					context = reason.ContextItems,
				})
				.Select(o => o.context == null
					? o.message
					: o.context.Aggregate(
						o.message,
						(message, item) => message.Replace(string.Format("{{{0}}}", item.Key), item.Value.ToString())));

			if(validationFailureMessages.Any())
			{
				foreach(var failureMessage in validationFailureMessages)

					ModelState.AddModelError("EnteredCode", failureMessage);

				return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
			}

			PromotionManager.AssignPromotion(customer.CustomerID, promoCode);

			return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
		}

		public ActionResult RemovePromo(string code)
		{
			if(!string.IsNullOrEmpty(code))
			{
				var customer = HttpContext.GetCustomer();
				PromotionManager.ClearPromotionUsages(customerId: customer.CustomerID, promotionCode: code, removeAutoAssigned: true);
			}

			return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
		}
	}
}
