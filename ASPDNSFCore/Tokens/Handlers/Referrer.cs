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
	public class Referrer : ITokenHandler
	{
		readonly string[] Tokens = { "referrer" };

		readonly HttpServerUtilityBase Server;

		public Referrer(HttpServerUtilityBase server)
		{
			Server = server;
		}

		public string RenderToken(TokenHandlerContext context)
		{
			if(!Tokens.Contains(context.Token, StringComparer.OrdinalIgnoreCase))
				return null;

			return Server.HtmlEncode(CommonLogic.PageReferrer());
		}
	}
}
