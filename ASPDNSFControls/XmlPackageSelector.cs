// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontControls
{
	public class XmlPackageSelector : DropDownList
	{
		// ASP.Net typeconverters aren't executed for primitive types like enums, and the default enum converter can't handle combining flags.
		// Instead, we're stuck with a stupid proxy enum.
		public enum XmlPackageLocationProxy
		{
			Root = XmlPackageLocation.Root,
			Skin = XmlPackageLocation.Skin,
			Admin = XmlPackageLocation.Admin,
			RootAndSkin = XmlPackageLocation.Root | XmlPackageLocation.Skin,
			RootAndAdmin = XmlPackageLocation.Skin | XmlPackageLocation.Admin,
			SkinAndAdmin = XmlPackageLocation.Root | XmlPackageLocation.Admin,
			All = XmlPackageLocation.Root | XmlPackageLocation.Skin | XmlPackageLocation.Admin,
		}

		public string Prefix { get; set; }
		public XmlPackageLocationProxy Locations { get; set; }

		readonly XmlPackageManager XmlPackageManager;
		readonly ISkinProvider SkinProvider;

		public XmlPackageSelector()
		{
			XmlPackageManager = new XmlPackageManager();
			SkinProvider = new SkinProvider();
		}

		protected override void OnInit(EventArgs e)
		{
			var xmlPackageListItems = BuildXmlPackageListItems().ToArray();
			Items.AddRange(xmlPackageListItems);

			base.OnInit(e);
		}

		IEnumerable<ListItem> BuildXmlPackageListItems()
		{
			// Get available skins
			var allSkinIds = SkinProvider
				.GetSkins()
				.Where(skin => !skin.IsMobile)
				.Select(skin => skin.Id);

			// Load root and skin XmlPackages
			var availableXmlPackages = XmlPackageManager
				.GetXmlPackageInfosByPrefix(
					httpContext: new HttpContextWrapper(HttpContext.Current),
					searchLocations: (XmlPackageLocation)Locations,
					prefix: Prefix,
					skinIds: allSkinIds);

			// Group XmlPackages together and denote skin-specific ones
			var xmlPackageListItems = availableXmlPackages
				.GroupBy(info => info.Name)
				.Select(packages => new
					{
						name = packages.Key,
						displayName =
							packages.All(x => x.Location == XmlPackageLocation.Admin)										// Display all admin packages
							|| packages.Any(x => x.Location == XmlPackageLocation.Root)										// Display all packages that are in root only or overridden in a skin
							|| packages.Where(x => x.SkinId.HasValue).Select(x => x.SkinId.Value).SequenceEqual(allSkinIds)	// Display all packages that are in all skins
							? packages.First().DisplayName
							: string.Format(																				// Display skin-specific skins with a note on which skins
								"{0} ({1} skin(s) only)", 
								packages.First().DisplayName, 
								string.Join(", ", packages
									.Where(x => x.SkinId.HasValue)
									.Select(x => string.Format("\"{0}\"", SkinProvider.GetSkinById(x.SkinId.Value).Name))))
					})
				.Select(listing => new ListItem(listing.displayName, listing.name));

			return xmlPackageListItems;
		}
	}
}
