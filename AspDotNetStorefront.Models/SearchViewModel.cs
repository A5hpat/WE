// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AspDotNetStorefront.Models
{
	public class SearchViewModel
	{
		public string SearchTerm { get; set; }
		public string PageContent { get; set; }
		public string PageTitle { get; set; }
	}
}
