// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Web.Mvc;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Controllers
{
	public class RssController : Controller
	{
		public ActionResult Index(string channel)
		{
			if(string.IsNullOrEmpty(channel) 
				|| !CommonLogic.FileExists(string.Format("xmlpackages/rss.{0}.xml.config", channel)))
				channel = "unknown";
			
			var xmlPackage = new XmlPackage(string.Format("rss.{0}", channel));
			var customer = HttpContext.GetCustomer();
			var packageOutput = AppLogic.RunXmlPackage(xmlPackage, null, customer, customer.SkinID, true, false);

			Response.ContentType = "text/xml";
			return Content(packageOutput);
		}
	}
}
