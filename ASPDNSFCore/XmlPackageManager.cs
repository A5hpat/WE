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
	public class XmlPackageManager
	{
		public IEnumerable<XmlPackageInfo> GetXmlPackageInfosByPrefix(HttpContextBase httpContext, XmlPackageLocation searchLocations, string prefix = null, IEnumerable<int> skinIds = null)
		{
			var fileSearchPattern = String.IsNullOrEmpty(prefix)
				? "*.xml.config"
				: String.Format("{0}.*.xml.config", prefix);

			var xmlPackageInfos = new[]
					{
						XmlPackageLocation.Root,
						XmlPackageLocation.Skin,
						XmlPackageLocation.Admin,
					}
				.Where(location => (searchLocations & location) == location)		// Filter down the possible locations to those provided in searchLocations
				.SelectMany(location => location == XmlPackageLocation.Skin			// Add a skin location for each provided skin ID
					? (skinIds ?? Enumerable.Empty<int>())
						.Select(skinId => new
							{
								location,
								skinId = (int?)skinId,
							})
						.ToArray()
					: new[] 
						{ 
							new
								{ 
									location, 
									skinId = (int?)null,
								}
						})
				.Select(spec => new
					{
						spec.location,
						spec.skinId,
						filePaths = Directory.GetFiles(
							searchPattern: fileSearchPattern,
							path: httpContext.Server.MapPath(
								GetXmlPackagePath(spec.location, spec.skinId))),
					})
				.SelectMany(xmlPackages => xmlPackages
					.filePaths
					.Select(filePath => new XmlPackageInfo(
						location: xmlPackages.location,
						path: filePath,
						displayName: GetXmlPackageDisplayName(filePath),
						skinId: xmlPackages.skinId)))
				.OrderBy(info => info.DisplayName)
				.ThenBy(info => info.SkinId)
				.ToArray();

			return xmlPackageInfos;
		}
		readonly SkinProvider SkinProvider;

		public XmlPackageManager()
		{
			SkinProvider = new SkinProvider();
		}
		
		string GetXmlPackageDisplayName(string packageName)
		{
			var xmlpackage = new XmlPackage(
				packageName: packageName,
				customer: HttpContext.Current.GetCustomer(),
				onlyRunNamedQuery: "QueryThatDoesNotExistSoThisNewsUpFaster");

			return xmlpackage.DisplayName;
		}

		string GetXmlPackagePath(XmlPackageLocation location, int? skinId)
		{
			switch(location)
			{
				case XmlPackageLocation.Root:
					return "~/XmlPackages/";

				case XmlPackageLocation.Skin:
					if(skinId != null)
						return String.Format("~/Skins/{0}/XmlPackages/", SkinProvider.GetSkinNameById(skinId.Value));
					else
						throw new InvalidOperationException("A skinId must be provided when getting the location of a skin");

				case XmlPackageLocation.Admin:
					return String.Format("~/{0}/XmlPackages/", AppLogic.GetAdminDir());

				default:
					throw new NotSupportedException(String.Format("The XmlPackageLocation {0} is not supported", location));
			}
		}
	}

	[Flags]
	public enum XmlPackageLocation
	{
		Skin = 1,
		Root = 2,
		Admin = 4,
	}

	public class XmlPackageInfo
	{
		public string Name
		{ get { return _Name; } }

		public string DisplayName
		{ get { return _DisplayName; } }

		public XmlPackageLocation Location
		{ get { return _Location; } }

		public int? SkinId
		{ get { return _SkinId; } }

		readonly string _Name;
		readonly string _DisplayName;
		readonly string _Path;
		readonly XmlPackageLocation _Location;
		readonly int? _SkinId;

		public XmlPackageInfo(XmlPackageLocation location, string path, string displayName, int? skinId)
		{
			if(path == null)
				throw new ArgumentNullException("path");

			_Name = System.IO.Path.GetFileName(path);
			_DisplayName = String.IsNullOrWhiteSpace(displayName)
				? _Name
				: displayName;
			_Path = path;
			_Location = location;
			_SkinId = skinId;
		}
	}
}
