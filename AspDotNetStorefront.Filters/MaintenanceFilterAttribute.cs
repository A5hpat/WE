// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using AspDotNetStorefront.Routing;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Filters
{
	public class MaintenanceFilterAttribute : FilterAttribute, IAuthorizationFilter
	{
		public void OnAuthorization(AuthorizationContext filterContext)
		{
			if(!AppLogic.AppConfigBool("DisplayMaintenancePage.Enable"))
				return;

			var allowedOnAction = 
				filterContext
					.ActionDescriptor
					.GetCustomAttributes(typeof(AllowInMaintenanceModeAttribute), true)
					.Any();

			var allowedOnController =
				filterContext
					.ActionDescriptor
					.ControllerDescriptor
					.GetCustomAttributes(typeof(AllowInMaintenanceModeAttribute), true)
					.Any();

			if(allowedOnAction || allowedOnController)
				return;

			var customer = filterContext
				.HttpContext
				.GetCustomer();

			if(customer.IsAdminSuperUser || customer.IsAdminUser)
				return;

			//avoid "Child actions are not allowed to perform redirect actions." error
			if(filterContext.IsChildAction)
				return;

			filterContext.Result = new RedirectToRouteResult(
				new RouteValueDictionary
				{
					{ RouteDataKeys.Controller, ControllerNames.Maintenance },
					{ RouteDataKeys.Action, ActionNames.Index }
				});
		}
	}

	[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
	public sealed class AllowInMaintenanceModeAttribute : Attribute
	{ }
}
