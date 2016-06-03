// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using AspDotNetStorefrontCore;
using System.Web.Mvc;
using System.Web.Routing;
using AspDotNetStorefront.Routing;

namespace AspDotNetStorefront.Filters
{
	public class RequireCustomerRegistrationFilterAttribute : FilterAttribute, IActionFilter
	{
		readonly RequiredRegistrationStatus RequiredRegistrationStatus;
		readonly string RedirectController;
		readonly string RedirectAction;

		public RequireCustomerRegistrationFilterAttribute(RequiredRegistrationStatus requiredRegistrationStatus, string redirectController, string redirectAction)
		{
			RequiredRegistrationStatus = requiredRegistrationStatus;
			RedirectController = redirectController;
			RedirectAction = redirectAction;
		}

		public void OnActionExecuting(ActionExecutingContext filterContext)
		{
			var customer = filterContext.HttpContext.GetCustomer();

			switch(RequiredRegistrationStatus)
			{
				case RequiredRegistrationStatus.Guest:
					if(!customer.IsRegistered)
						return;
					break;

				case RequiredRegistrationStatus.Registered:
					if(customer.IsRegistered)
						return;
					break;
			}

			var returnUrl = filterContext.HttpContext.Request.Url.PathAndQuery;

			filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary
			{
				{ RouteDataKeys.Controller, RedirectController },
				{ RouteDataKeys.Action, RedirectAction },
				{ RouteDataKeys.ReturnUrl, returnUrl }
			});
		}

		public void OnActionExecuted(ActionExecutedContext filterContext)
		{ }
	}

	public enum RequiredRegistrationStatus
	{
		Guest,
		Registered,
	}
}
