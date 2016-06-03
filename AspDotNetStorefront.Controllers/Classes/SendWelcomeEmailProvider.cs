// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Controllers.Classes
{
	public class SendWelcomeEmailProvider
	{
		public void SendWelcomeEmail(Customer customer)
		{
			try
			{
				string body = AppLogic.RunXmlPackage(
					XmlPackageName: "notification.newmemberwelcome.xml.config",
					UseParser: null,
					ThisCustomer: customer,
					SkinID: customer.SkinID,
					RunTimeQuery: string.Empty,
					RunTimeParams: string.Format("fullname={0}", customer.FullName()),
					ReplaceTokens: false,
					WriteExceptionMessage: false);

				AppLogic.SendMail(
					subject: "Thank you for registering",
					body: body,
					useHtml: true,
					fromAddress: AppLogic.AppConfig("MailMe_FromAddress"),
					fromName: AppLogic.AppConfig("MailMe_FromName"),
					toAddress: customer.EMail,
					toName: customer.FullName(),
					bccAddresses: string.Empty,
					server: AppLogic.MailServer());
			}
			catch(Exception exception)
			{
				SysLog.LogException(exception, MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
			}
		}
	}
}
