// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web.Mvc;
using AspDotNetStorefront.Models;
using AspDotNetStorefrontCore;
using AspDotNetStorefront.Routing;

namespace AspDotNetStorefront.Controllers
{
	public class MetaTagController : Controller
	{
		public ActionResult Detail(string metaTitle, string metaDescription, string metaKeywords)
		{
			var metaTagViewModel = new MetaTagViewModel
			{
                // nal
				MetaTitle = "Wedding Enhancements",
				MetaDescription = "SE_MetaDescription ",
				MetaKeywords = "SE_MetaKeywords"
            };

			if(!String.IsNullOrEmpty(metaTitle))
				metaTagViewModel.MetaTitle = metaTitle;

			if(!String.IsNullOrEmpty(metaDescription))
				metaTagViewModel.MetaDescription = metaDescription;

			if(!String.IsNullOrEmpty(metaKeywords))
				metaTagViewModel.MetaKeywords = metaKeywords;

			return PartialView(ViewNames.DetailPartial, metaTagViewModel);
		}
	}
}
