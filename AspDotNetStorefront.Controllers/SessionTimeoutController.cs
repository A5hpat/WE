// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Web.Mvc;
using AspDotNetStorefront.Models;
using AspDotNetStorefrontCore;
using AspDotNetStorefront.Routing;

namespace AspDotNetStorefront.Controllers
{
	public class SessionTimeoutController : Controller
	{
		[ChildActionOnly]
		public ActionResult _AddTimer()
		{
			var customer = HttpContext.GetCustomer();
			var isCheckoutPage = AppLogic.GetCurrentPageType() == PageTypes.Checkout;

			//Only add the timer to the page if the customer is logged in or they're in checkout
			if(!isCheckoutPage && !customer.IsRegistered)
				return Content(string.Empty);

			var sessionTimeout = customer.IsAdminUser || customer.IsAdminSuperUser
				? AppLogic.AdminSessionTimeout()
				: AppLogic.SessionTimeout();

			var enabled = AppLogic.AppConfigBool("SessionTimeoutWarning.Enabled");

			var model = new AddTimerViewModel(
				enabled: enabled,
				sessionTimeout: sessionTimeout,
				refreshUrl: Url.Action(ActionNames.RefreshSession));
			
			return View(model);
		}

		public ActionResult RefreshSession()
		{
			return Json(new
				{
					success = true,
				});
		}
	}
}
