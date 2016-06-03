// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Web.Mvc;
using AspDotNetStorefront.Filters;

namespace AspDotNetStorefront.Controllers
{
	[AllowInMaintenanceMode]
	public class MaintenanceController : Controller
	{
		public ActionResult Index()
		{
			return View();
		}
	}
}
