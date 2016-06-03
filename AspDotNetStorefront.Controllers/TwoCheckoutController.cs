// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Web.Mvc;
using AspDotNetStorefront.Caching.ObjectCaching;
using AspDotNetStorefront.Filters;
using AspDotNetStorefront.Models;
using AspDotNetStorefrontCore;
using AspDotNetStorefront.Routing;
using AspDotNetStorefrontGateways;

namespace AspDotNetStorefront.Controllers
{
	public class TwoCheckoutController : Controller
	{
		readonly ICachedShoppingCartProvider CachedShoppingCartProvider;
		readonly NoticeProvider NoticeProvider;

		public TwoCheckoutController(
			ICachedShoppingCartProvider cachedShoppingCartProvider,
			NoticeProvider noticeProvider)
		{
			CachedShoppingCartProvider = cachedShoppingCartProvider;
			NoticeProvider = noticeProvider;
		}

		[PageTypeFilter(PageTypes.Checkout)]
		[HttpGet, ImportModelStateFromTempData]
		public ActionResult TwoCheckout()
		{
			var customer = HttpContext.GetCustomer();
			var cart = CachedShoppingCartProvider.Get(customer, CartTypeEnum.ShoppingCart, AppLogic.StoreID());
			var returnUrl = Url.Action(ActionNames.TwoCheckoutReturn, ControllerNames.TwoCheckout, null, this.Request.Url.Scheme);

			var model = new TwoCheckoutViewModel
			{
				LiveServerUrl = AppLogic.AppConfig("2CHECKOUT_Live_Server"),
				ReturnUrl = returnUrl,
				Login = AppLogic.AppConfig("2CHECKOUT_VendorID"),
				Total = Localization.CurrencyStringForGatewayWithoutExchangeRate(cart.Total(true)),
				InvoiceNumber = CommonLogic.GetNewGUID(),
				Email = customer.EMail,
				BillingAddress = customer.PrimaryBillingAddress,
				UseLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions")
			};

			return View(model);
		}

		[HttpPost, RequireCustomerRecordFilter]
		public ActionResult TwoCheckoutReturn(string x_2checked)    //This is their form field name, not mine
		{
			//http://help.2checkout.com/articles/FAQ/What-Authorize-net-parameters-does-2checkout-support/
			//Y = success.  K = Pending
			if(x_2checked != "Y" && x_2checked != "K")
			{
				NoticeProvider.PushNotice("Billing was canceled or not approved. Please retry your order.", NoticeType.Failure);
				return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
			}

			var customer = HttpContext.GetCustomer();
			var cart = CachedShoppingCartProvider.Get(customer, CartTypeEnum.ShoppingCart, AppLogic.StoreID());
			var orderNumber = AppLogic.GetNextOrderNumber();

			var status = Gateway.MakeOrder(string.Empty, AppLogic.ro_TXModeAuthCapture, cart, orderNumber, string.Empty, string.Empty, string.Empty, string.Empty);

			if(status == AppLogic.ro_OK)
			{
				var confirmationUrl = Url.Action(
					ActionNames.Confirmation,
					ControllerNames.CheckoutConfirmation,
					new
					{
						orderNumber = orderNumber
					});

				return Redirect(confirmationUrl);
			}
			else
			{
				NoticeProvider.PushNotice(status, NoticeType.Failure);
				return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
			}
		}
	}
}
