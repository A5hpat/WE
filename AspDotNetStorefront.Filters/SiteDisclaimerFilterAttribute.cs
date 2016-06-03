// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using AspDotNetStorefrontCore;
using AspDotNetStorefront.Routing;

namespace AspDotNetStorefront.Filters
{
	public class SiteDisclaimerFilterAttribute : FilterAttribute, IAuthorizationFilter
	{
		public void OnAuthorization(AuthorizationContext filterContext)
		{
			//Disclaimer isn't on or we're logging into admin
			if(!AppLogic.AppConfigBool("SiteDisclaimerRequired") 
				|| CommonLogic.QueryStringCanBeDangerousContent("ReturnURL").Contains(AppLogic.AppConfig("AdminDir"))
				|| HttpContext.Current.Request.RequestContext.RouteData.Values[RouteDataKeys.Controller].ToString().EqualsIgnoreCase(ControllerNames.SiteDisclaimer)
				|| HttpContext.Current.Request.RequestContext.RouteData.Values[RouteDataKeys.Controller].ToString().EqualsIgnoreCase(ControllerNames.Captcha))
				return;

			//Disclaimer is on and has been accepted
			if(CommonLogic.CookieCanBeDangerousContent("SiteDisclaimerAccepted", true).Length != 0)
				return;

			filterContext.Result = new RedirectToRouteResult(
				new RouteValueDictionary 
                { 
                    { RouteDataKeys.Controller, ControllerNames.SiteDisclaimer }, 
                    { RouteDataKeys.Action, ActionNames.Index },
 					{ RouteDataKeys.ReturnUrl, CommonLogic.QueryStringCanBeDangerousContent("ReturnURL") }
                });
		}
	}
}
