// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Web.Mvc;

namespace AspDotNetStorefront.Filters
{
	class TemporaryRedirectResult : RedirectResult
	{
		public TemporaryRedirectResult(string url)
			: base(url, false)
		{ }

		public override void ExecuteResult(ControllerContext context)
		{
			base.ExecuteResult(context);

			// Use a 307 instead of 302 to preserve POST's
			context.HttpContext.Response.StatusCode = 307;
		}
	}
}
