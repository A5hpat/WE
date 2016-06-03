// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using AspDotNetStorefront.Routing;
using Microsoft.Owin.Security.Cookies;

namespace AspDotNetStorefront.Auth
{
	public class AppRelativeCookieAuthenticationProvider : CookieAuthenticationProvider
	{
		public override void ApplyRedirect(CookieApplyRedirectContext context)
		{
			// Get the current sign-in and sign-out paths per request. They may have changed since appstart 
			// due to configuration.
			var urlHelper = new UrlHelper(context.OwinContext.Get<RequestContext>(typeof(RequestContext).ToString()));

			var currentSignInPath = urlHelper.Action(ActionNames.SignIn, ControllerNames.Account);
			var currentSignOutPath = urlHelper.Action(ActionNames.SignOut, ControllerNames.Account);

			// Convert the paths to be non-virtual, but still relative to the app root as the cookie auth 
			// handler will prepend any virtual directory.
			var appRelativeSignInPath = VirtualPathUtility
				.ToAppRelative(currentSignInPath)
				.TrimStart('~');

			var appRelativeSignOutPath = VirtualPathUtility
				.ToAppRelative(currentSignOutPath)
				.TrimStart('~');

			// Replace the old paths in the redirect URL with the updated, app-relative ones. This will fix 
			// the duplicated virtual directory if it exists.
			var redirectUri = context.RedirectUri
				.Replace(context.Options.LoginPath.ToString(), appRelativeSignInPath)
				.Replace(context.Options.LogoutPath.ToString(), appRelativeSignOutPath);

			// Invoke the normal base redirect logic with the updated URL.
			base.ApplyRedirect(new CookieApplyRedirectContext(
				context.OwinContext,
				context.Options,
				redirectUri));
		}
	}
}
