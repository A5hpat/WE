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
	public class SkinPath : ITokenHandler
	{
		readonly string[] Tokens = { "skinpath" };

		readonly ISkinProvider SkinProvider;

		public SkinPath(ISkinProvider skinProvider)
		{
			SkinProvider = skinProvider;
		}

		public string RenderToken(TokenHandlerContext context)
		{
			if(!Tokens.Contains(context.Token, StringComparer.OrdinalIgnoreCase))
				return null;

			var virtualSkinPath = string.Format("~/skins/{0}", SkinProvider.GetSkinNameById(context.Customer.SkinID));

			return VirtualPathUtility
				.ToAbsolute(virtualSkinPath)
				.ToLower();
		}
	}
}
