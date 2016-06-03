// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Linq;
using System.Web.Mvc;
using AspDotNetStorefront.Routing;

namespace AspDotNetStorefrontCore.Tokens.Handlers
{
	public class SignInOutLink : ITokenHandler
	{
		readonly string[] Tokens = { "signinout_link", "signinoutlink" };

		readonly UrlHelper UrlHelper;

		public SignInOutLink(UrlHelper urlHelper)
		{
			UrlHelper = urlHelper;
		}

		public string RenderToken(TokenHandlerContext context)
		{
			if(!Tokens.Contains(context.Token, StringComparer.OrdinalIgnoreCase))
				return null;

			return AppLogic.ResolveUrl(
				context.Customer.IsRegistered
					? UrlHelper.Action(
						actionName: ActionNames.SignOut,
						controllerName: ControllerNames.Account)
					: UrlHelper.Action(
						actionName: ActionNames.SignIn,
						controllerName: ControllerNames.Account));
		}
	}
}
