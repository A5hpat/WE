// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Optimization;

namespace AspDotNetStorefront.Optimization
{
	public static class AdnsfBundler
	{
		public static IHtmlString RenderStyleBundle(string bundlePath, string[] filePaths)
		{
			// Make sure file paths are app relative (~/).
			filePaths = filePaths.Select(path => path.StartsWith("~") 
				? path 
				: string.Format("~{0}", path)).ToArray();

			// Make sure the bundle path is app relative
			bundlePath = bundlePath.StartsWith("~") 
				? bundlePath 
				: string.Format("~{0}", bundlePath);

			// Add a hash of the files onto the path to ensure that the filepaths have not changed.
			bundlePath = string.Format("{0}{1}", bundlePath, GetBundleHashForFiles(filePaths));

			var bundleIsRegistered = BundleTable
				.Bundles
				.GetRegisteredBundles()
				.Where(bundle => bundle.Path == bundlePath)
				.Any();

			if(!bundleIsRegistered)
			{
				var bundle = new StyleBundle(bundlePath);
				bundle.Orderer = new AsProvidedOrderer();
				bundle.Include(filePaths);
				BundleTable.Bundles.Add(bundle);
			}

			return Styles.Render(bundlePath);
		}

		public static IHtmlString RenderScriptBundle(string bundlePath, string[] filePaths)
		{
			// Make sure file paths are relative (~/).
			filePaths = filePaths.Select(path => path.StartsWith("~") 
				? path 
				: string.Format("~{0}", path)).ToArray();

			// Make sure the bundle path is app relative
			bundlePath = bundlePath.StartsWith("~")
				? bundlePath
				: string.Format("~{0}", bundlePath);

			// Add a hash of the files onto the path to ensure that the filepaths have not changed.
			bundlePath = string.Format("{0}{1}", bundlePath, GetBundleHashForFiles(filePaths));

			var bundleIsRegistered = BundleTable
				.Bundles
				.GetRegisteredBundles()
				.Where(bundle => bundle.Path == bundlePath)
				.Any();

			if(!bundleIsRegistered)
			{
				var bundle = new ScriptBundle(bundlePath);
				bundle.Orderer = new AsProvidedOrderer();
				bundle.Include(filePaths);
				BundleTable.Bundles.Add(bundle);
			}

			return Scripts.Render(bundlePath);
		}

		static string GetBundleHashForFiles(IEnumerable<string> filePaths)
		{
			// Create a unique hash for this set of files
			var aggregatedPaths = filePaths.Aggregate((pathString, next) => pathString + next);
			var Md5 = MD5.Create();
			var encodedPaths = Encoding.UTF8.GetBytes(aggregatedPaths);
			var hash = Md5.ComputeHash(encodedPaths);
			var bundlePath = hash.Aggregate(string.Empty, (hashString, next) => string.Format("{0}{1:x2}", hashString, next));
			return bundlePath;
		}

		class AsProvidedOrderer : IBundleOrderer
		{
			public IEnumerable<BundleFile> OrderFiles(BundleContext context, IEnumerable<BundleFile> files)
			{
				return files;
			}
		}
	}
}
