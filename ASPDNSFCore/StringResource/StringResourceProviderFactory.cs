// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.StringResource
{
	public class StringResourceProviderFactory : IStringResourceProviderFactory
	{
		public IStringResourceProvider Create()
		{
			var showResourceKeysOnly = AppLogic.AppConfigBool("ShowStringResourceKeys");
			if(showResourceKeysOnly)
				return new KeyOnlyStringResourceProvider();

			var currentLocale = System.Web.HttpContext
				.Current
				.GetCustomer()
				.LocaleSetting;

			return new LocalizedStringResourceProvider(currentLocale);
		}
	}
}
