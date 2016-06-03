// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Filters
{
	/// <summary>
	/// The ContentSecurityPolicyFilter class enables CSP releated headers to be written to every
	/// response originating from the application.
	/// </summary>
	/// <remarks>
	/// In an effort to protect merchants and customers against click jacking/ui redress attacks,
	/// we've created this filter to output the necessary headers to direct browsers to block rendering
	/// the site in an iframe.
	/// 
	/// To Disable this filter, set the app config parameter ContentSecurityPolicy.Enabled to false.
	/// 
	/// https://developer.mozilla.org/en-US/docs/Web/HTTP/X-Frame-Options
	/// https://developer.mozilla.org/en-US/docs/Web/Security/CSP/CSP_policy_directives#frame-ancestors
	/// </remarks>
	public class ContentSecurityPolicyFilterAttribute : FilterAttribute, IActionFilter
	{
		public readonly ContentSecurityPolicy ContentSecurityPolicy;

		public ContentSecurityPolicyFilterAttribute()
		{
			ContentSecurityPolicy = new ContentSecurityPolicy();
		}

		public void OnActionExecuting(ActionExecutingContext filterContext)
		{ }

		public void OnActionExecuted(ActionExecutedContext filterContext)
		{
			ContentSecurityPolicy.Enforce(filterContext.HttpContext.Response);
		}
	}

	public class ContentSecurityPolicy
	{
		readonly IDictionary<string, string> Headers;

		public ContentSecurityPolicy()
		{
			Headers = new Dictionary<string, string>()
			{
				{ "X-Frame-Options", AppLogic.AppConfig("ContentSecurityPolicy.X-Frame-Options") },
                { "Content-Security-Policy", AppLogic.AppConfig("ContentSecurityPolicy.Content-Security-Policy") },
				{ "X-Content-Security-Policy", AppLogic.AppConfig("ContentSecurityPolicy.X-Content-Security-Policy") },
			};
		}

		public void Enforce(HttpResponseBase response)
		{
			if(!AppLogic.AppConfigBool("ContentSecurityPolicy.Enabled"))
				return;

			foreach(var header in Headers)
				if(!response.Headers.AllKeys.Contains(header.Key))
					response.AddHeader(header.Key, header.Value);
		}
	}
}
