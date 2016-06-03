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
	public class BuySafeSeal : ITokenHandler
	{
		readonly string[] Tokens = { "buysafeseal" };

		public string RenderToken(TokenHandlerContext context)
		{
			if(!Tokens.Contains(context.Token, StringComparer.OrdinalIgnoreCase))
				return null;

			if(!AppLogic.GlobalConfigBool("BuySafe.Enabled")
				|| string.IsNullOrEmpty(AppLogic.GlobalConfig("BuySafe.Hash")))
				return string.Empty;

			return string.Format(@"
				<!-- BEGIN: buySAFE Guarantee Seal -->
				<script src=""{0}""></script>
				<span id=""BuySafeSealSpan""></span>
				<script type=""text/javascript"">
					buySAFE.Hash = {1};
					WriteBuySafeSeal('BuySafeSealSpan', 'GuaranteedSeal');
				</script>
				<!-- END: buySAFE Guarantee Seal -->",
				HttpUtility.HtmlAttributeEncode(AppLogic.GlobalConfig("BuySafe.RollOverJSLocation")),
				HttpUtility.JavaScriptStringEncode(AppLogic.GlobalConfig("BuySafe.Hash"), true));
		}
	}
}
