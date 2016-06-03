// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.IO;
using System.Linq;
using System.Xml;

namespace AspDotNetStorefrontCore
{
	public class XmlPackageUrlResolver : XmlUrlResolver
	{
		readonly XmlPackagePathHelper PathHelper;
		readonly string SkinName;

		public XmlPackageUrlResolver(XmlPackagePathHelper pathHelper, string skinName)
		{
			PathHelper = pathHelper;
			SkinName = skinName;
		}

		public override Uri ResolveUri(Uri baseUri, string relativeUri)
		{
			var paths = PathHelper
				.GenerateXmlPackageSearchPaths(SkinName)
				.ToArray();

			var newRelativeUri = paths
				.Select(folder => Path.Combine(folder, relativeUri))
				.Where(path => File.Exists(path))
				.FirstOrDefault();

			if(newRelativeUri == null)
				throw new FileNotFoundException(
					string.Format(
						"Could not find the file {0} in any of the following locations:{1}{2}",
						relativeUri,
						Environment.NewLine,
						string.Join(
							Environment.NewLine,
							paths)));

			return base.ResolveUri(baseUri, newRelativeUri);
		}
	}
}
