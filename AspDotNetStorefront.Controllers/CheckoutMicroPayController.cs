// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Web.Mvc;
using AspDotNetStorefront.Caching.ObjectCaching;
using AspDotNetStorefront.Checkout;
using AspDotNetStorefront.Filters;
using AspDotNetStorefront.Models;
using AspDotNetStorefront.Routing;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Controllers
{
	[SecureAccessFilter(forceHttps: true)]
	public class CheckoutMicroPayController : Controller
	{
		readonly NoticeProvider NoticeProvider;
		readonly IPaymentOptionProvider PaymentOptionProvider;
		readonly ICachedShoppingCartProvider CachedShoppingCartProvider;

		public CheckoutMicroPayController(
			NoticeProvider noticeProvider,
			IPaymentOptionProvider paymentOptionProvider,
			ICachedShoppingCartProvider cachedShoppingCartProvider)
		{
			NoticeProvider = noticeProvider;
			PaymentOptionProvider = paymentOptionProvider;
			CachedShoppingCartProvider = cachedShoppingCartProvider;
		}

		[HttpGet]
		public ActionResult MicroPayDetail()
		{
			var customer = HttpContext.GetCustomer();

			if(!PaymentOptionProvider.PaymentMethodSelectionIsValid(AppLogic.ro_PMMicropay, customer))
			{
				NoticeProvider.PushNotice(
					message: "Invalid payment method!  Please choose another.",
					type: NoticeType.Failure);
				return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
			}

			var cart = CachedShoppingCartProvider.Get(customer, CartTypeEnum.ShoppingCart, AppLogic.StoreID());

			var model = new MicroPayViewModel
			{
				Balance = Localization.CurrencyStringForDisplayWithExchangeRate(customer.MicroPayBalance, customer.CurrencySetting),
				BalanceIsSufficient = customer.MicroPayBalance >= cart.Total(true)
			};

			return PartialView(ViewNames.MicroPayPartial, model);
		}
	}
}
