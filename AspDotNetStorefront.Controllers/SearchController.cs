// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data.SqlClient;
using System.Web.Mvc;
using AspDotNetStorefront.Filters;
using AspDotNetStorefront.Models;
using AspDotNetStorefrontCore;
using AspDotNetStorefront.Routing;

namespace AspDotNetStorefront.Controllers
{
	[PageTypeFilter(PageTypes.Search)]
	public class SearchController : Controller
	{
		public ActionResult Index(string searchTerm = null)
		{

			var customer = HttpContext.GetCustomer();

			var pageContent = String.Empty;
            // nal MinSearchStringLength
            //var minimumSearchTermLength = AppLogic.AppConfigNativeInt("MinSearchStringLength");
            var minimumSearchTermLength = 3;
			if(searchTerm != null && minimumSearchTermLength <= searchTerm.Length)
                // nal GetSearchResultsHtml
                //pageContent = GetSearchResultsHtml(searchTerm, "page.search.xml.config", customer);
                pageContent = GetSearchResultsHtml(searchTerm, "guidednavigationsearch.grid.xml.config", customer);
			else
				pageContent = String.Format("Please enter at least {0} characters in the Search field.", minimumSearchTermLength);

			var searchViewModel = new SearchViewModel
			{
				SearchTerm = searchTerm,
				PageContent = pageContent,
				PageTitle = "Search"
            };

			return View(searchViewModel);
		}

		public ActionResult AdvancedSearch(string searchTerm = null)
		{
			var customer = HttpContext.GetCustomer();

            // nal
            //var searchXmlPackageName = String.IsNullOrEmpty(AppLogic.AppConfig("XmlPackage.SearchAdvPage"))
            var searchXmlPackageName = "page.searchadv.xml.config";

			var pageContent = GetSearchResultsHtml(searchTerm, searchXmlPackageName, customer);

			var searchViewModel = new SearchViewModel
			{
				SearchTerm = searchTerm,
				PageContent = pageContent,
				PageTitle = "Search"
            };

			return View(ActionNames.Index, searchViewModel);
		}

		string GetSearchResultsHtml(string searchTerm, string xmlpackageName, Customer customer)
		{
			if(!string.IsNullOrWhiteSpace(searchTerm)
				&& AppLogic.AppConfigBool("Search_LogSearches"))
				DB.ExecuteSQL("insert into SearchLog(SearchTerm,CustomerID,LocaleSetting) values(@SearchTerm,@CustomerID,@Locale)", new SqlParameter[] {
					new SqlParameter("@SearchTerm",  CommonLogic.Ellipses(searchTerm, 97, true)),
					new SqlParameter("@CustomerID", customer.CustomerID),
					new SqlParameter("@Locale", customer.LocaleSetting)});

			var searchResultHtml = AppLogic.RunXmlPackage(
				XmlPackageName: xmlpackageName,
				UseParser: null,
				ThisCustomer: customer,
				SkinID: customer.SkinID,
				RunTimeQuery: string.Empty,
				RunTimeParams: string.Format("SearchTerm={0}", searchTerm),
				ReplaceTokens: true,
				WriteExceptionMessage: false);

			return searchResultHtml;
		}
	}
}
