// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	/// <summary>
	/// Provides basic attribute-based authorization for WebForms pages.
	/// THIS IS ONLY INTENDED TO BE USED IN ADMIN!
	/// </summary>
	public class LoggedAuthorizationModule : IHttpModule
	{
		public void Init(HttpApplication context)
		{
			// We need to check authorization in PostMapRequestHandler so that we have access to the page
			context.PostMapRequestHandler += (sender, e) => PostMapRequestHandler((HttpApplication)sender);
		}

		void PostMapRequestHandler(HttpApplication application)
		{
			// Check if this request is for a page with a LoggedAuthorizeAttribute on it
			var context = application.Context;

			var page = context.Handler as Page;
			if(page == null)
				return;

			var loggedAuthorizeAttribute = page
				.GetType()
				.GetCustomAttribute<LoggedAuthorizeAttribute>();

			if(loggedAuthorizeAttribute == null)
				return;

			// Once we have an attribute, we can validate the current user against it
			var allowedUsers = loggedAuthorizeAttribute.Users.ParseAsDelimitedList();
			var allowedRoles = loggedAuthorizeAttribute.Roles.ParseAsDelimitedList();
			var authorizationResult = GetAuthorizationResult(context, allowedUsers, allowedRoles);

			var customer = HttpContext.Current.GetCustomer();

			// Log success results
			if(authorizationResult == AuthorizationResult.Authorized && loggedAuthorizeAttribute.LogSuccess)
			{
				Security.LogEvent(
					"Access Success",
					string.Format(
						"URL: {0}",
						context.Request.Url),
					0,
					customer.CustomerID,
					customer.CurrentSessionID);
			}

			// Handle failure results
			if(authorizationResult != AuthorizationResult.Authorized)
			{
				if(loggedAuthorizeAttribute.LogFailed)
					Security.LogEvent(
						"Access Failed",
						string.Format(
							"URL: {0}; Reason: {1}",
							context.Request.Url,
							authorizationResult.ToString()),
						0,
						customer.CustomerID,
						customer.CurrentSessionID);

				// Notify the user and end the request
				// We're using NoticeProvider instead of AlertMessage because a 301 status code 
				// will cause a redirect to the front-end sign-in page, which only displays notices
				// and not alerts.
				var noticeProvider = DependencyResolver.Current.GetService<NoticeProvider>();
				noticeProvider.PushNotice(
					AppLogic.GetString("admin.common.Notification.UnAuthorized"),
					NoticeType.Failure);

				context.Response.StatusCode = (int)System.Net.HttpStatusCode.Unauthorized;
				context.ApplicationInstance.CompleteRequest();
			}
		}

		AuthorizationResult GetAuthorizationResult(HttpContext context, IEnumerable<string> allowedUsers, IEnumerable<string> allowedRoles)
		{
			var user = context.User;
			if(user == null || user.Identity == null || !user.Identity.IsAuthenticated)
				return AuthorizationResult.Unauthenticated;

			if(allowedUsers.Any() && !allowedUsers.Contains(user.Identity.Name, StringComparer.OrdinalIgnoreCase))
				return AuthorizationResult.NotInAllowedUsers;

			if(allowedRoles.Any() && !allowedRoles.Any(user.IsInRole))
				return AuthorizationResult.NotInAllowedRoles;

			return AuthorizationResult.Authorized;
		}

		enum AuthorizationResult
		{
			Unauthenticated,
			NotInAllowedUsers,
			NotInAllowedRoles,
			Authorized,
		}

		public void Dispose()
		{
		}
	}
}
