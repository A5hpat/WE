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
	public class SkinName : ITokenHandler
	{
		readonly string[] Tokens = { "skinname" };

		readonly ISkinProvider SkinProvider;

		public SkinName(ISkinProvider skinProvider)
		{
			SkinProvider = skinProvider;
		}

		public string RenderToken(TokenHandlerContext context)
		{
			if(!Tokens.Contains(context.Token, StringComparer.OrdinalIgnoreCase))
				return null;

			return SkinProvider.GetSkinNameById(context.Customer.SkinID);
		}
	}
}
