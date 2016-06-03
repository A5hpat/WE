// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Filters;
using AspDotNetStorefront.Auth;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Filters
{
	public class CustomerRoleVerificationFilterAttribute : FilterAttribute, IAuthenticationFilter
	{
		readonly IClaimsIdentityProvider ClaimsIdentityProvider;

		public CustomerRoleVerificationFilterAttribute(IClaimsIdentityProvider claimsIdentityProvider)
		{
			ClaimsIdentityProvider = claimsIdentityProvider;
		}

		public void OnAuthentication(AuthenticationContext filterContext)
		{
			var customer = filterContext
				.HttpContext
				.GetCustomer();

			var principal = filterContext
				.HttpContext
				.GetOwinContext()
				.Authentication
				.User;

			var identity = ClaimsIdentityProvider.CreateClaimsIdentity(customer);

			if(customer.IsAdminUser != principal.IsInRole("Admin")
				|| customer.IsAdminSuperUser != principal.IsInRole("SuperAdmin"))
				filterContext
					.HttpContext
					.GetOwinContext()
					.Authentication
					.SignIn(identity);
		}

		public void OnAuthenticationChallenge(AuthenticationChallengeContext filterContext)
		{ }
	}
}
