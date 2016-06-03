// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AspDotNetStorefront
{
	public static class ControllerContextExtensions
	{
		public static HtmlHelper GetHtmlHelper(this ControllerContext controllerContext)
		{
			return new HtmlHelper(
				new ViewContext(controllerContext, new WebFormView(controllerContext, "Placeholder"),
					new ViewDataDictionary(),
					new TempDataDictionary(),
					new StringWriter()),
				new ViewPage());
		}
	}
}
