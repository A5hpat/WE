// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Linq;
using System.Web;
using AspDotNetStorefront.Auth;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Application
{
	public class SessionTimeoutModule : IHttpModule
	{
		public void Init(HttpApplication context)
		{
			context.PostAuthenticateRequest += (sender, e) => PostAuthenticateRequest((HttpApplication)sender);
		}

		void PostAuthenticateRequest(HttpApplication application)
		{
			var context = application.Context;

			var customer = context
				.GetCustomer();

			// If the user is logged in or checking out and has sat idle too long, make them reauthenticate.
			var sessionTimeout = customer.IsAdminUser || customer.IsAdminSuperUser
				? AppLogic.AdminSessionTimeout()
				: AppLogic.SessionTimeout();

			var reauthRequired = customer.HasCustomerRecord
				&& customer.LastActivity < DateTime.Now - sessionTimeout;

			if(!reauthRequired)
			{
				// Don't update customer sessions if the request is just for certain page elements.  This protects against the possibility of AJAX'y content messing up the session timer
				var requestedResourceEndsWithIgnoredExtension = new[]
					{
						".png",
						".jpg",
						".gif",
						".js",
					}
					.Where(extension => context.Request.Url.AbsoluteUri.EndsWith(extension))
					.Any();

				if(!requestedResourceEndsWithIgnoredExtension)
					customer.ThisCustomerSession.UpdateCustomerSession(null, null);
			}
			else if(customer.IsRegistered)
			{
				var authenticationManager = context
					.Request
					.GetOwinContext()
					.Authentication;

				// Registered users have to sign back in
				authenticationManager.SignOut(AuthValues.CookiesAuthenticationType);
				authenticationManager.Challenge(AuthValues.CookiesAuthenticationType);
			}
			else
			{
				// Anons go here instead
				customer.EndAnonymousSession();

				var sessionTimeoutLandingPage = AppLogic.AppConfig("SessionTimeoutLandingPage");
				var redirectUrl = string.IsNullOrEmpty(sessionTimeoutLandingPage)
					? "~/"
					: sessionTimeoutLandingPage;
				context.Response.Redirect(redirectUrl, false);
				context.ApplicationInstance.CompleteRequest();
				return;
			}
		}

		public void Dispose()
		{
		}
	}
}
