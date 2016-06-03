// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.StringResource
{
	public class LocalizedStringResourceProvider : IStringResourceProvider
	{
		readonly string Locale;

		public LocalizedStringResourceProvider(string locale)
		{
			Locale = locale;
		}

		public string GetString(string key)
		{
			if(string.IsNullOrWhiteSpace(key))
				return null;

			return AppLogic.GetString(key, Locale);
		}
	}
}
