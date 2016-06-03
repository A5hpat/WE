// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontEventHandlers.Formatters;

namespace AspDotNetStorefrontEventHandlers.Publishers
{
	class EmailExceptionPublisher : IExceptionPublisher
	{
		readonly IExceptionFormatter ExceptionFormatter;

		public EmailExceptionPublisher(IExceptionFormatter exceptionFormatter)
		{
			ExceptionFormatter = exceptionFormatter;
		}

		public void Publish(Exception exception, string errorCode)
		{
			var toAddress = AppLogic.AppConfig("MailMe_ErrorToAddress");
			var toName = AppLogic.AppConfig("MailMe_ErrorToName");
			var fromAddress = AppLogic.AppConfig("MailMe_ErrorFromAddress");

			if(String.IsNullOrWhiteSpace(toAddress))
				toAddress = AppLogic.AppConfig("MailMe_ToAddress");

			if(String.IsNullOrWhiteSpace(toName))
				toName = AppLogic.AppConfig("MailMe_ToName");

			if(String.IsNullOrWhiteSpace(fromAddress))
				fromAddress = AppLogic.AppConfig("MailMe_FromAddress");

			if(String.IsNullOrWhiteSpace(fromAddress))
				fromAddress = AppLogic.AppConfig("MailMe_FromName");

			var errorMessage = ExceptionFormatter.Format(exception, errorCode);

			AppLogic.SendMail(
				subject: string.Format("Error on Site {0} (Error Code:{1})", AppLogic.AppConfig("StoreName"), errorCode),
				body: errorMessage,
				useHtml: false,
				fromAddress: fromAddress,
				fromName: toName,
				toAddress: toAddress,
				toName: toName,
				bccAddresses: String.Empty,
				replyToAddress: fromAddress);
		}
	}
}
