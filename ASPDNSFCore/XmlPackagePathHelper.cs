// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace AspDotNetStorefrontCore
{
	public class XmlPackagePathHelper
	{
		public IEnumerable<string> GenerateXmlPackageNameVariations(string requestedFileName, Customer customer)
		{
			if(!requestedFileName.EndsWith(XmlPackage.XmlPackageExtension, StringComparison.OrdinalIgnoreCase))
				requestedFileName += XmlPackage.XmlPackageExtension;

			// Build a list of localized filenames to try
			var customerLocale = customer != null
				? customer.LocaleSetting
				: Localization.GetDefaultLocale();

			return new[]
				{
					requestedFileName.Replace(
						XmlPackage.XmlPackageExtension, 
						string.Format(".{0}{1}", customerLocale, XmlPackage.XmlPackageExtension)),

					requestedFileName.Replace(
						XmlPackage.XmlPackageExtension, 
						string.Format(".{0}{1}", Localization.GetDefaultLocale(), XmlPackage.XmlPackageExtension)),

					requestedFileName,
				}
				.Distinct(StringComparer.OrdinalIgnoreCase);
		}

		public IEnumerable<string> GenerateXmlPackageSearchPaths(string currentSkinName)
		{
			// Search various folder/filename combinations for the package
			var physicalRootPath = HttpContext.Current.Request.PhysicalApplicationPath;
			var physicalAdminPath = Path.Combine(physicalRootPath, AppLogic.AppConfig("AdminDir"));

			var folders = new[] {
				new {
					path = Path.Combine(physicalRootPath, string.Format("Skins\\{0}\\XmlPackages", currentSkinName)),
					isAdmin = false },
				new {
					path = Path.Combine(physicalRootPath, "XmlPackages"),
					isAdmin = false },
				new {
					path = Path.Combine(physicalRootPath, "EntityHelper"),
					isAdmin = false },
				new {
					path = Path.Combine(physicalAdminPath, "XmlPackages"),
					isAdmin = true },
				new {
					path = Path.Combine(physicalAdminPath, "EntityHelper"),
					isAdmin = true }};

			return folders
				.Where(folder => !folder.isAdmin || folder.isAdmin == AppLogic.IsAdminSite)
				.Select(folder => folder.path);
		}
	}
}
