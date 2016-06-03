// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.IO;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontEventHandlers.Formatters;

namespace AspDotNetStorefrontEventHandlers.Publishers
{
	class FileBasedExceptionPublisher : IExceptionPublisher
	{
		readonly IExceptionFormatter ExceptionFormatter;

		public FileBasedExceptionPublisher(IExceptionFormatter exceptionFormatter)
		{
			ExceptionFormatter = exceptionFormatter;
		}

		public void Publish(Exception exception, string errorCode)
		{
			var dumpDirectory = CommonLogic.SafeMapPath("~/images/errors");

			if(!Directory.Exists(dumpDirectory))
				Directory.CreateDirectory(dumpDirectory);

			var filename = string.Format("{0}_{1}.txt", DateTime.Now.ToString("MM-dd-yyy_hhmmss"), errorCode);
			var filePath = CommonLogic.SafeMapPath(Path.Combine(dumpDirectory, filename));

			var errorMessage = ExceptionFormatter.Format(exception, errorCode);

			File.WriteAllText(filePath, errorMessage);
		}
	}
}
