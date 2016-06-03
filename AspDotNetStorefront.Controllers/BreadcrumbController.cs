// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AspDotNetStorefront.Models;
using AspDotNetStorefrontCore;
using AspDotNetStorefront.Routing;

namespace AspDotNetStorefront.Controllers
{
	public class BreadcrumbController : Controller
	{
		public ActionResult Detail(string pageTitle = null, IEnumerable<PathInfoViewModel> pathInfo = null)
		{
			var breadcrumbViewModel = new BreadcrumbViewModel(
				pageTitle: String.IsNullOrEmpty(pageTitle)
					? String.Empty
					: pageTitle,
				pathInfos: pathInfo ?? Enumerable.Empty<PathInfoViewModel>(),
				breadcrumbSeparator: "/");

			return PartialView(ViewNames.DetailPartial, breadcrumbViewModel);
		}
	}
}
