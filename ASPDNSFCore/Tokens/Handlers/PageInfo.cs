// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Linq;
using System.Web;

namespace AspDotNetStorefrontCore.Tokens.Handlers
{
	public class PageInfo : ITokenHandler
	{
		readonly string[] Tokens = { "pageinfo" };

		readonly HttpServerUtilityBase Server;

		public PageInfo(HttpServerUtilityBase server)
		{
			Server = server;
		}

		public string RenderToken(TokenHandlerContext context)
		{
			if(!Tokens.Contains(context.Token, StringComparer.OrdinalIgnoreCase))
				return null;

			return string.Format(@"
				<!--
					PAGE INVOCATION: {0}
					PAGE REFERRER: {1}
					STORE LOCALE: {2}
					STORE CURRENCY: {3}
					CUSTOMER ID: {4}
					AFFILIATE ID: {5}
					CUSTOMER LOCALE: {6}
					CURRENCY SETTING: {7}
					CACHE MENUS: {8}
				-->",
				Server.HtmlEncode(CommonLogic.PageInvocation()),
				Server.HtmlEncode(CommonLogic.PageReferrer()),
				Localization.GetDefaultLocale(),
				Localization.GetPrimaryCurrency(),
				context.Customer.CustomerID,
				context.Customer.AffiliateID,
				context.Customer.LocaleSetting,
				context.Customer.CurrencySetting,
				AppLogic.AppConfigBool("CacheMenus"));
		}
	}
}
