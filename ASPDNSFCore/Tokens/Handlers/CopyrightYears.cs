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
	public class CopyrightYears : ITokenHandler
	{
		readonly string[] Tokens = { "copyrightyears" };

		public string RenderToken(TokenHandlerContext context)
		{
			if(!Tokens.Contains(context.Token, StringComparer.OrdinalIgnoreCase))
				return null;

			return string.Format(
				"{0}{1}",
				string.IsNullOrEmpty(AppLogic.AppConfig("StartingCopyrightYear"))
					? string.Empty
					: string.Format("{0}-", AppLogic.AppConfig("StartingCopyrightYear")),
				DateTime.Now.Year);
		}
	}
}
