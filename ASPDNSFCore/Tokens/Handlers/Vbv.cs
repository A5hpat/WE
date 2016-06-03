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
	public class Vbv : ITokenHandler
	{
		readonly string[] Tokens = { "vbv" };

		readonly SkinProvider SkinProvider;

		public Vbv(SkinProvider skinProvider)
		{
			SkinProvider = skinProvider;
		}

		public string RenderToken(TokenHandlerContext context)
		{
			if(!Tokens.Contains(context.Token, StringComparer.OrdinalIgnoreCase))
				return null;

			if(!AppLogic.AppConfigBool("CardinalCommerce.Centinel.Enabled"))
				return string.Empty;

			return string.Format(
				@"<img src=""{0}"" border=""0"" alt=""{1}"">",
				HttpUtility.HtmlAttributeEncode(
					AppLogic.LocateImageURL(string.Format(
						"Skins/{0}/images/vbv.jpg",
						SkinProvider.GetSkinNameById(context.Customer.SkinID)))),
                "Store protected with Verified By Visa/MasterCard Secure Initiatives");
		}
	}
}
