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
using AspDotNetStorefront.Routing;

namespace AspDotNetStorefront.Controllers
{
	public class ReceiptController : Controller
	{
		[Authorize]
		[PageTypeFilter(PageTypes.Checkout)]
		public ActionResult Index(int orderNumber)
		{
			var customer = HttpContext.GetCustomer();
			var order = new Order(orderNumber, customer.LocaleSetting);

			// Does the order exist?
			if (order.IsEmpty)
				return RedirectToAction(ActionNames.Detail, ControllerNames.Topic, new { @name = "ordernotfound" });

			// If currently logged in user is not the one who owns the order, and this is not an admin user who is logged in, reject the view
			if(customer.CustomerID != order.CustomerID && !customer.IsAdminUser)
				return RedirectToAction(ActionNames.Detail, ControllerNames.Topic, new { @name = "ordernotfound" });

			// Determine if customer is allowed to view orders from other store.
			if(!customer.IsAdminUser 
				&& AppLogic.StoreID() != AppLogic.GetOrdersStoreID(orderNumber) 
				&& AppLogic.GlobalConfigBool("AllowCustomerFiltering"))
				return RedirectToAction(ActionNames.Detail, ControllerNames.Topic, new { @name = "ordernotfound" });

			var model = new ReceiptViewModel(body: order.Receipt(customer, false));

			return View(model);
		}
	}
}
