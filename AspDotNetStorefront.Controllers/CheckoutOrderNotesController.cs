// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Web.Mvc;
using AspDotNetStorefront.Caching.ObjectCaching;
using AspDotNetStorefront.Models;
using AspDotNetStorefrontCore;
using AspDotNetStorefront.Routing;
using AspDotNetStorefront.Filters;

namespace AspDotNetStorefront.Controllers
{
	[SecureAccessFilter(forceHttps: true)]
	public class CheckoutOrderNotesController : Controller
	{
		readonly ICachedShoppingCartProvider CachedShoppingCartProvider;

		public CheckoutOrderNotesController(ICachedShoppingCartProvider cachedShoppingCartProvider)
		{
			CachedShoppingCartProvider = cachedShoppingCartProvider;
		}

		[ChildActionOnly]
		public ActionResult OrderNotes()
		{
			var customer = HttpContext.GetCustomer();
			var cart = CachedShoppingCartProvider.Get(customer, CartTypeEnum.ShoppingCart, AppLogic.StoreID());

			var model = new OrderNotesViewModel
			{
				OrderNotes = cart.OrderNotes
			};

			return PartialView(ViewNames.OrderNotesPartial, model);
		}

		[HttpPost]
		public ActionResult OrderNotes(OrderNotesViewModel model)
		{
			var customer = HttpContext.GetCustomer();

			if(model.OrderNotes == null)
				model.OrderNotes = string.Empty;

			customer.UpdateCustomer(orderNotes: model.OrderNotes.Trim());

			return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
		}
	}
}
