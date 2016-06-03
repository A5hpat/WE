// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;

namespace XsltObjects
{
	/// <summary>
	/// Contains the loaded extension configurations.
	/// </summary>
	public class ExtensionConfiguration
	{
		public readonly IReadOnlyDictionary<string, Extension> Extensions;
		public readonly string DefaultExtension;

		public ExtensionConfiguration(IReadOnlyDictionary<string, Extension> extensions, string defaultExtension)
		{
			Extensions = extensions;
			DefaultExtension = defaultExtension;
		}
	}
}
