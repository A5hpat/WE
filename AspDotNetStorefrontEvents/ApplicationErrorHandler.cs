// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Web;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontEventHandlers.Formatters;
using AspDotNetStorefrontEventHandlers.Publishers;

namespace AspDotNetStorefrontEventHandlers
{
	/// <summary>
	/// Implements methods to intercept application errors and log them appropriately
	/// </summary>
	class ApplicationErrorHandler : IHttpModule
	{
		/// <summary>
		/// Required method responsible for loading the module
		/// </summary>
		public void Init(HttpApplication application)
		{
			application.Error += new EventHandler(Application_Error);
		}

		/// <summary>
		/// Fires whenever an unhandled exception in the application occurs
		/// </summary>
		public void Application_Error(Object sender, EventArgs e)
		{
			// Do not let a database error or something of that nature throw the site into an endless exception loop
			try
			{
				if(!AppLogic.AppIsStarted)
					return;

				AppLogic.Custom_Application_Error(sender, e);

				if(HttpContext.Current.Server.GetLastError() != null)
				{
					var exceptionHandler = new ExceptionHandler(InitializeExceptionPublishers());
					exceptionHandler.HandleException(HttpContext.Current.Server.GetLastError());
				}
			}
			catch { }
		}

		IEnumerable<IExceptionPublisher> InitializeExceptionPublishers()
		{
			var exceptionFormatter = new TextExceptionFormatter();
			var configuredPublishers = AppLogic.AppConfig("System.LoggingLocation");

			if(CommonLogic.StringInCommaDelimitedStringList("file", configuredPublishers))
				yield return new FileBasedExceptionPublisher(exceptionFormatter);

			if(CommonLogic.StringInCommaDelimitedStringList("email", configuredPublishers))
				yield return new EmailExceptionPublisher(exceptionFormatter);

			if(CommonLogic.StringInCommaDelimitedStringList("eventLog", configuredPublishers))
				yield return new EventLogExceptionPublisher(exceptionFormatter);

			if(CommonLogic.StringInCommaDelimitedStringList("database", configuredPublishers))
				yield return new DatabaseExceptionPublisher();
		}

		public void Dispose()
		{ }
	}
}
