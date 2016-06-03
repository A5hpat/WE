// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Linq;
using System.Web.Mvc;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Filters
{
	public class ReferrerCookieFilterAttribute : FilterAttribute, IActionFilter
	{
		public void OnActionExecuting(ActionExecutingContext filterContext)
		{
			// Set the referrer cookie if:
			//	- A referrer value is present
			//	- There is no referrer cookie already set
			//	- The referrer is not coming from internal web site

			var referrer = CommonLogic.PageReferrer();
			if(string.IsNullOrEmpty(referrer))
				return;

			var referrerCookieValue = CommonLogic.CookieCanBeDangerousContent(Customer.ro_ReferrerCookieName, true);
			if(!string.IsNullOrEmpty(referrerCookieValue))
				return;

			var liveServerUrl = AppLogic.LiveServer();
			var referrerIsIgnored = new[] 
				{
					"localhost",
					"192.168.",
					"10.",
					liveServerUrl,
				}
				.Where(s => referrer.IndexOf(s, StringComparison.OrdinalIgnoreCase) != -1)
				.Any();

			if(referrerIsIgnored)
				return;

			AppLogic.SetCookie(Customer.ro_ReferrerCookieName, referrer, TimeSpan.FromDays(365));
		}

		public void OnActionExecuted(ActionExecutedContext filterContext)
		{ }
	}
}
