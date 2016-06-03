// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web.Mvc;
using AspDotNetStorefront.Models;
using AspDotNetStorefrontCore;
using AspDotNetStorefront;
using System.Web;

namespace AdnsfMvc.Controllers
{
	public class XmlPackageController : Controller
	{
		public ActionResult Detail(string name, bool? disableTemplate = null)
		{
			if(string.IsNullOrEmpty(name))
				throw new HttpException(404, null);

			var customer = HttpContext.GetCustomer();
			var xmlPackage = new XmlPackage(
				packageName: name,
				customer: customer,
				htmlHelper: ControllerContext.GetHtmlHelper());

			// Make sure the engine is allowed to render this package
			if(!xmlPackage.AllowEngine)
				throw new Exception("This XmlPackage is not allowed to be run from the engine. Set the package element's allowengine attribute to true to enable this package to run.");

			var packageOutput = AppLogic.RunXmlPackage(xmlPackage, null, customer, customer.SkinID, true, false);

			if(disableTemplate ?? false)
				return Content(
					packageOutput,
					string.IsNullOrEmpty(xmlPackage.ContentType)
						? "text/html"
						: xmlPackage.ContentType);
			else
				return View(new XmlPackageViewModel
				{
					Name = xmlPackage.PackageName,
					MetaTitle = xmlPackage.SETitle,
					MetaDescription = xmlPackage.SEDescription,
					MetaKeywords = xmlPackage.SEKeywords,
					PageTitle = xmlPackage.SectionTitle,
					PageContent = packageOutput,
				});
		}
	}
}
