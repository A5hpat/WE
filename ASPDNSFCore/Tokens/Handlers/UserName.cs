// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AspDotNetStorefront.Routing;

namespace AspDotNetStorefrontCore.Tokens.Handlers
{
	public class UserName : ITokenHandler
	{
		readonly string[] Tokens = { "username", "user_name" };

		readonly UrlHelper UrlHelper;

		public UserName(UrlHelper urlHelper)
		{
			UrlHelper = urlHelper;
		}

		public string RenderToken(TokenHandlerContext context)
		{
			if(!Tokens.Contains(context.Token, StringComparer.OrdinalIgnoreCase))
				return null;

			if(!context.Customer.IsRegistered)
				return string.Empty;

			if(AppLogic.IsAdminSite)
				return context.Customer.FullName();

			var accountLink = UrlHelper.Action(
				actionName: ActionNames.Index,
				controllerName: ControllerNames.Account);

			return string.Format(
				"{0} <a class='username' href='{1}'>{2}</a>{3}",
                "Welcome, ",
				HttpUtility.HtmlAttributeEncode(accountLink),
				HttpUtility.HtmlEncode(context.Customer.FullName()),
				context.Customer.CustomerLevelID != 0
					? string.Format(" ({0})", HttpUtility.HtmlEncode(context.Customer.CustomerLevelName))
					: string.Empty);
		}
	}
}
