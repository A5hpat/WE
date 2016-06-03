// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Web;
using System.Web.Mvc;
using AspDotNetStorefront.Auth;
using AspDotNetStorefrontCore;
using Microsoft.Owin.Security;

namespace AspDotNetStorefront.Filters
{
	public class RequireCustomerRecordFilterAttribute : FilterAttribute, IActionFilter
	{
		// Injected
		public IClaimsIdentityProvider ClaimsIdentityProvider
		{ private get; set; }

		public void OnActionExecuting(ActionExecutingContext filterContext)
		{
			var customer = filterContext.HttpContext.GetCustomer();
			if(customer.HasCustomerRecord)
				return;

			int newCustomerId;
			string newCustomerGuid;
			Customer.MakeAnonCustomerRecord(out newCustomerId, out newCustomerGuid);

			var newCustomer = new Customer(newCustomerId, true);
			newCustomer.ThisCustomerSession.UpdateCustomerSession(null, null);

			var persistSignIn = AppLogic.AppConfigBool("Anonymous.PersistCookie");
			var identity = ClaimsIdentityProvider.CreateClaimsIdentity(
				name: newCustomerGuid,
				identifier: newCustomerGuid,
				authenticationType: AuthValues.CookiesAuthenticationType,
				identityProvider: AuthValues.CookiesAuthenticationIdentityProvider);

			filterContext
				.HttpContext
				.GetOwinContext()
				.Authentication
				.SignIn(
					properties: new AuthenticationProperties { IsPersistent = persistSignIn },
					identities: identity);

			filterContext.Result = new TemporaryRedirectResult(filterContext.HttpContext.Request.RawUrl);
		}

		public void OnActionExecuted(ActionExecutedContext filterContext)
		{ }
	}
}
