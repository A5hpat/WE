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
	/// Contains configuration for a given extension.
	/// </summary>
	public class Extension
	{
		public readonly string Name;
		public readonly string Type;
		public readonly IReadOnlyDictionary<string, string> Attributes;

		public Extension(string name, string type, IReadOnlyDictionary<string, string> attributes)
		{
			Name = name;
			Type = type;
			Attributes = attributes;
		}
	}
}
