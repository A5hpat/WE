// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontEventHandlers.Publishers
{
	class DatabaseExceptionPublisher : IExceptionPublisher
	{
		public void Publish(Exception exception, string errorCode)
		{
			SysLog.LogException(exception, MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
		}
	}
}
