// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using AspDotNetStorefront.Caching.ObjectCaching;
using AspDotNetStorefront.Models;
using AspDotNetStorefrontCore;
using AspDotNetStorefront.Routing;
using AspDotNetStorefront.Filters;

namespace AspDotNetStorefront.Controllers
{
	[SecureAccessFilter(forceHttps: true)]
	public class CheckoutOrderOptionController : Controller
	{
		readonly ICachedShoppingCartProvider CachedShoppingCartProvider;

		public CheckoutOrderOptionController(ICachedShoppingCartProvider cachedShoppingCartProvider)
		{
			CachedShoppingCartProvider = cachedShoppingCartProvider;
		}

		[ChildActionOnly]
		public ActionResult OrderOption()
		{
			var customer = HttpContext.GetCustomer();
			var cart = CachedShoppingCartProvider.Get(customer, CartTypeEnum.ShoppingCart, AppLogic.StoreID());

			var options = cart
				.AllOrderOptions
				.Select(option => new OrderOptionItemViewModel
				{
					OrderOptionGuid = option.UniqueID,
					Id = option.ID,
					Name = option.Name,
					Description = option.Description,
					Cost = Localization.CurrencyStringForDisplayWithExchangeRate(option.Cost, customer.CurrencySetting),
					Checked = cart.OrderOptions.Select(orderOption => orderOption.ID).Contains(option.ID),
					TaxClassId = option.TaxClassID,
					ImageUrl = option.ImageUrl,
					HasImage = !string.IsNullOrEmpty(option.ImageUrl) && !option.ImageUrl.Contains("nopictureicon")
				});

			var model = new OrderOptionViewModel
			{
				Options = options
			};

			return PartialView(ViewNames.OrderOptionPartial, model);
		}

		[HttpPost]
		public ActionResult OrderOption(OrderOptionViewModel model)
		{
			var customer = HttpContext.GetCustomer();

			//Save the new selections
			var saveGuids = model.Options.Where(opt => opt.Checked).Select(o => o.OrderOptionGuid);
			var saveFormat = string.Join(",", saveGuids);

			string updateSql = "UPDATE Customer SET OrderOptions = @OrderOptionString WHERE CustomerId = @CustomerId";
			DB.ExecuteSQL(updateSql, new[] {
						new SqlParameter("@OrderOptionString", saveFormat),
						new SqlParameter("@CustomerId", customer.CustomerID) });

			return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
		}
	}
}
