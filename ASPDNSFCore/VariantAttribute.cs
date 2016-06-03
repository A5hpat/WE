// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------

namespace AspDotNetStorefrontCore
{
	public enum VariantAttributeType
	{
		Size,
		Color,
	}

	public class VariantAttribute
	{
		public string Value { get; set; }
		public string Text { get; set; }
	}
}
