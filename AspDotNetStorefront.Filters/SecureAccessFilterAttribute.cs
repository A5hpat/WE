// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Web.Mvc;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Filters
{
	public class SecureAccessFilterAttribute : ActionFilterAttribute
	{
		readonly bool ForceHttps;

		public SecureAccessFilterAttribute(bool forceHttps = false)
		{
			ForceHttps = forceHttps;
		}

		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			if(filterContext.IsChildAction 
				|| filterContext.HttpContext.Request.IsAjaxRequest()
				|| !AppLogic.UseSSL() 
				|| !AppLogic.OnLiveServer())
				return;

			var requireHttps = ForceHttps || AppLogic.AppConfigBool("AlwaysUseHTTPS");

			// Decide if we should be forcing HTTP or HTTPS
			var redirectToHttps = requireHttps
				&& !CommonLogic.IsSecureConnection();

			var redirectToHttp = !requireHttps
				&& AppLogic.AppConfigBool("GoNonSecureAgain")
				&& CommonLogic.IsSecureConnection()
				&& filterContext.HttpContext.Request.HttpMethod == "GET";

			// Write out the HSTS header if the always use https setting is on
			if(AppLogic.AppConfigBool("AlwaysUseHTTPS"))
				WriteHstsHeader(filterContext.HttpContext);

			// Continue the current request if no action is needed
			if(!redirectToHttps && !redirectToHttp)
				return;

			// Build the redirect URL
			var currentUrl = filterContext.HttpContext.Request.Url;
			var uriBuilder = new UriBuilder(currentUrl);

			// Use the default port for whatever scheme is selected
			uriBuilder.Port = -1;

			// Use HTTPS if required, HTTP otherwise
			uriBuilder.Scheme = redirectToHttps
				? Uri.UriSchemeHttps
				: Uri.UriSchemeHttp;

			// Redirect to the new URL
			filterContext.Result = new TemporaryRedirectResult(uriBuilder.Uri.ToString());
		}

		void WriteHstsHeader(HttpContextBase context)
		{
			var hstsHeaderValue = AppLogic.AppConfig("HstsHeader");
			if(!string.IsNullOrEmpty(hstsHeaderValue))
				context.Response.Headers.Add("Strict-Transport-Security", hstsHeaderValue);
		}
	}
}
