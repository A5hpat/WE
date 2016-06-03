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
	public class BongoExtend : ITokenHandler
	{
		readonly string[] Tokens = { "bongoextend" };

		public string RenderToken(TokenHandlerContext context)
		{
			if(!Tokens.Contains(context.Token, StringComparer.OrdinalIgnoreCase))
				return null;

			if(!AppLogic.AppConfigBool("Bongo.Extend.Enabled")
				|| string.IsNullOrEmpty(AppLogic.AppConfig("Bongo.Extend.Script")))
				return string.Empty;

			return string.Format(
				"<script type=\"text/javascript\" src=\"{0}\"></script>",
				AppLogic.AppConfig("Bongo.Extend.Script"));
		}
	}
}
