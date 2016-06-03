// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.ServiceModel;
using System.Xml;
using AspDotNetStorefrontWSI;

namespace AspDotNetStorefront
{
	[ServiceBehavior(
		Name = "AspDotNetStorefrontImportWebService",
		IncludeExceptionDetailInFaults = true)]
	public class Ipx : IIpx
	{
		public string DoIt(string email, string password, string document)
		{
			return DoItUsernamePwd(email, password, document);
		}

		public string DoItUsernamePwd(string email, string password, string document)
		{
			var wsi = new WSI();
			if(!wsi.AuthenticateRequest(email, password))
				throw new ArgumentException("Authentication Failed");

			try
			{
				wsi.LoadFromString(document);
				wsi.DoIt();

				return wsi
					.GetResults()
					.InnerXml;
			}
			catch(XmlException)
			{
				throw new ArgumentException("Invalid Import XmlDocument");
			}
			catch
			{
				return wsi.GetResultsAsString();
			}
		}
	}
}
