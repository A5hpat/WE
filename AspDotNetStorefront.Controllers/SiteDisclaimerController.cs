// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web.Mvc;
using AspDotNetStorefront.Models;
using AspDotNetStorefront.Routing;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Controllers
{
	public class SiteDisclaimerController : Controller
	{
		public ActionResult Index(string returnUrl)
		{
			var customer = HttpContext.GetCustomer();

			var siteDisclaimerViewModel = new SiteDisclaimerViewModel
			{
				DisclaimerText = new Topic(
					TopicID: Topic
						.GetTopicID("SiteDisclaimer", customer.LocaleSetting, customer.StoreID), 
					LocaleSetting: customer.LocaleSetting,
					SkinID: customer.SkinID,
					UseParser: new Parser()).Contents,
				ReturnUrl = returnUrl
			};

			return View(siteDisclaimerViewModel);
		}

		[HttpPost]
		public ActionResult Accept(string returnUrl)
		{
			AppLogic.SetCookie("SiteDisclaimerAccepted", new Guid().ToString(), new TimeSpan(1, 0, 0, 0));

			if(string.IsNullOrEmpty(returnUrl))
			{
				var siteDisclaimerAgreedPage = AppLogic.AppConfig("SiteDisclaimerAgreedPage");
				if (!string.IsNullOrWhiteSpace(siteDisclaimerAgreedPage))
				{
					if(!siteDisclaimerAgreedPage.StartsWith("/"))
						siteDisclaimerAgreedPage = string.Format("/{0}", siteDisclaimerAgreedPage);

					returnUrl = Url.MakeSafeReturnUrl(siteDisclaimerAgreedPage);
				}
			}

			if(string.IsNullOrEmpty(returnUrl))
				returnUrl = Url.Action(ActionNames.Index, ControllerNames.Home);

			return Redirect(returnUrl);
		}

		[HttpPost]
		public ActionResult Decline()
		{
			return Redirect(AppLogic.AppConfig("SiteDisclaimerNotAgreedURL"));
		}
	}
}
