// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Linq;

namespace AspDotNetStorefrontCore.Tokens.Handlers
{
	public class CurrencyLocaleRobotsTag : ITokenHandler
	{
		readonly string[] Tokens = { "currency_locale_robots_tag" };

		public string RenderToken(TokenHandlerContext context)
		{
			if(!Tokens.Contains(context.Token, StringComparer.OrdinalIgnoreCase))
				return null;

			if(context.Customer.CurrencySetting == Localization.GetPrimaryCurrency()
				|| context.Customer.LocaleSetting == Localization.GetDefaultLocale())
				return string.Empty;

			return "<meta name=\"robots\" content=\"noindex,nofollow,noarchive\" />";
		}
	}
}
