// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Web.Mvc;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Filters
{
	public class IpBlacklistFilterAttribute : FilterAttribute, IAuthorizationFilter
	{
		public void OnAuthorization(AuthorizationContext filterContext)
		{
			if(!AppLogic.AppConfigBool("IPAddress.RefuseRestrictedIPsFromSite"))
				return;

			var customerIpAddress = CommonLogic.CustomerIpAddress();

			if(!AppLogic.IPIsRestricted(customerIpAddress))
				return;

			filterContext.Result = new RedirectResult("refused.htm", false);
		}
	}
}
