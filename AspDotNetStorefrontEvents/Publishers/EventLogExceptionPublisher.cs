// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Diagnostics;
using AspDotNetStorefrontEventHandlers.Formatters;

namespace AspDotNetStorefrontEventHandlers.Publishers
{
	class EventLogExceptionPublisher : IExceptionPublisher
	{
		readonly IExceptionFormatter ExceptionFormatter;

		public EventLogExceptionPublisher(IExceptionFormatter exceptionFormatter)
		{
			ExceptionFormatter = exceptionFormatter;
		}

		public void Publish(Exception exception, string errorCode)
		{
			var eventLog = new EventLog
			{
				Source = "Application"
			};

			var errorMessage = ExceptionFormatter.Format(exception, errorCode);

			eventLog.WriteEntry(errorMessage, EventLogEntryType.Error);
		}
	}
}
