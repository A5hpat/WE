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
using AspDotNetStorefrontEventHandlers.Publishers;

namespace AspDotNetStorefrontEventHandlers
{
	class ExceptionHandler
	{
		readonly IEnumerable<IExceptionPublisher> ExceptionPublishers;

		public ExceptionHandler(IEnumerable<IExceptionPublisher> exceptionPublishers)
		{
			ExceptionPublishers = exceptionPublishers;
		}

		public void HandleException(Exception exception)
		{
			if(!AppLogic.AppConfigBool("System.LoggingEnabled"))
				return;

			// make sure this is a server error before proceeding
			if(exception is HttpException && ((HttpException)exception).GetHttpCode() != 500)
				return;

			// generate a new error code for reference
			var errorCode = Guid
				.NewGuid()
				.ToString("N")
				.Substring(0, 7)
				.ToUpper();

			// if an error occurs be silent about it
			// since these are the ones supposed to send the notification!
			foreach(var exceptionPublisher in ExceptionPublishers)
			{
				try
				{
					exceptionPublisher.Publish(exception, errorCode);
				}
				catch { }
			}
		}
	}
}
