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
	public class VatSelectList : ITokenHandler
	{
		readonly string[] Tokens = { "vatselectlist" };

		public string RenderToken(TokenHandlerContext context)
		{
			if(!Tokens.Contains(context.Token, StringComparer.OrdinalIgnoreCase))
				return null;

			if(!AppLogic.VATIsEnabled() || !AppLogic.AppConfigBool("VAT.AllowCustomerToChooseSetting"))
				return string.Empty;

			return AppLogic.GetVATSelectList(context.Customer);
		}
	}
}
