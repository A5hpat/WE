// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;

namespace AspDotNetStorefront.Models
{
	public class BreadcrumbViewModel
	{
		public readonly string PageTitle;
		public readonly IEnumerable<PathInfoViewModel> PathInfos;
		public readonly string BreadcrumbSeparator;

		public BreadcrumbViewModel(string pageTitle, IEnumerable<PathInfoViewModel> pathInfos, string breadcrumbSeparator)
		{
			PageTitle = pageTitle;
			PathInfos = pathInfos;
			BreadcrumbSeparator = breadcrumbSeparator;
		}
	}

	public class PathInfoViewModel
	{
		public readonly string Display;
		public readonly string Url;

		public PathInfoViewModel(string display, string url)
		{
			Display = display;
			Url = url;
		}
	}
}
