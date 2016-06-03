// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Linq;
using System.Web.Mvc;
using AspDotNetStorefront.Caching.ObjectCaching;
using AspDotNetStorefront.Models;
using AspDotNetStorefrontCore;
using AspDotNetStorefront.Routing;

namespace AspDotNetStorefront.Controllers
{
	public class UserLinksController : Controller
	{
		readonly ICachedShoppingCartProvider CachedShoppingCartProvider;

		public UserLinksController(ICachedShoppingCartProvider cachedShoppingCartProvider)
		{
			CachedShoppingCartProvider = cachedShoppingCartProvider;
		}

		[ChildActionOnly]
		public ActionResult Index()
		{
			var customer = HttpContext.GetCustomer();
			return PartialView(ViewNames.UserLinksPartial, new UserLinksViewModel {
				UserIsRegistered = customer.IsRegistered,
				UserFirstName = customer.FirstName,
				Email = customer.EMail,
				UserLastName = customer.LastName,
				MinicartEnabled = AppLogic.AppConfigBool("Minicart.Enabled"),
				MiniwishlistEnabled = AppLogic.AppConfigBool("ShowWishButtons"),
				CheckoutInProgress = AppLogic.GetCurrentPageType() == PageTypes.Checkout,
				CartHasItems = CachedShoppingCartProvider.Get(customer, CartTypeEnum.ShoppingCart, AppLogic.StoreID())
					.CartItems
						.Any()
			});
		}

		public ActionResult LoginLinks()
		{
			var customer = HttpContext.GetCustomer();

			return PartialView(
				ViewNames.LoginLinksPartial,
				new LoginLinksViewModel(
					userIsRegistered: customer.IsRegistered));
		}
	}
}
