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
	public class CartPrompt : ITokenHandler
	{
		readonly string[] Tokens = { "cartprompt" };

		public string RenderToken(TokenHandlerContext context)
		{
			if(!Tokens.Contains(context.Token, StringComparer.OrdinalIgnoreCase))
				return null;

			return "Shopping Cart";
		}
	}
}
