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
	public class CurrencySelectList : ITokenHandler
	{
		readonly string[] Tokens = { "currencyselectlist" };

		public string RenderToken(TokenHandlerContext context)
		{
			if(!Tokens.Contains(context.Token, StringComparer.OrdinalIgnoreCase))
				return null;

			if(Currency.NumPublishedCurrencies() < 2)
				return string.Empty;

			return AppLogic.GetCurrencySelectList(context.Customer);
		}
	}
}
