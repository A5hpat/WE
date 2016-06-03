// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Data.SqlClient;
using System.Web;
using System.Web.Mvc;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Filters
{
	public class UpdateCustomerLastIpAddressFilterAttribute : FilterAttribute, IActionFilter
	{
		public void OnActionExecuting(ActionExecutingContext filterContext)
		{
			var customerGuid = filterContext
				.HttpContext
				.Request
				.GetOwinContext()
				.Authentication
				.User
				.GetCustomerGuid();

			if(customerGuid == null)
				return;

			var ipAddress = CommonLogic.CustomerIpAddress();
			DB.ExecuteSQL(
				"update Customer set LastIPAddress = @lastIpAddress where CustomerGUID = @customerGuid and LastIPAddress != @lastIpAddress",
				new[] {
					new SqlParameter("lastIpAddress", ipAddress),
					new SqlParameter("customerGuid", customerGuid),
				});
		}

		public void OnActionExecuted(ActionExecutedContext filterContext)
		{ }
	}
}
