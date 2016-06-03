// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Web.UI.WebControls;

namespace AspDotNetStorefrontControls.Listing
{
	public class FilterControlContext
	{
		public readonly Parameter LocaleParameter;
		public readonly Parameter CurrentCustomerLocaleParameter;
		public readonly int Index;

		public FilterControlContext(Parameter localeParameter, Parameter currentCustomerLocaleParameter, int index)
		{
			LocaleParameter = localeParameter;
			CurrentCustomerLocaleParameter = currentCustomerLocaleParameter;
			Index = index;
		}
	}
}
