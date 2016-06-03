// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Web.Mvc;
using AspDotNetStorefront.Filters;
using AspDotNetStorefront.Models;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Controllers
{
    public class BlankController : Controller
    {
        [PageTypeFilter(PageTypes.Home)]
        public ActionResult Index()
        {
            var customer = HttpContext.GetCustomer();

            var homePageAd = new PayPalAd(PayPalAd.TargetPage.Home);

            var homeViewModel = new HomeViewModel
            {
                PayPalAd = homePageAd.Show
                    ? homePageAd.ImageScript
                    : string.Empty,
                PageTitle = string.Format(
                    "Welcome to {0}",
                    AppLogic.AppConfig("StoreName")),
            };

            return View(homeViewModel);
        }
    }
}
