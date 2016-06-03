// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Web;
using System.Web.Mvc;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Filters
{
	public class CustomerQueryStringFilterAttribute : FilterAttribute, IActionFilter
	{
		public void OnActionExecuting(ActionExecutingContext filterContext)
		{
			var customer = filterContext
				.HttpContext
				.GetCustomer();

			if(customer == null)
				return;

			if(!string.IsNullOrEmpty(CommonLogic.QueryStringCanBeDangerousContent("AffiliateID")))
				customer.AffiliateID = CommonLogic.QueryStringNativeInt("AffiliateID");

			if(!string.IsNullOrEmpty(CommonLogic.QueryStringCanBeDangerousContent("AffID")))
				customer.AffiliateID = CommonLogic.QueryStringNativeInt("AffID");

			var localeSetting = CommonLogic.QueryStringCanBeDangerousContent("LocaleSetting");
			if(!string.IsNullOrEmpty(localeSetting))
				customer.LocaleSetting = Localization.CheckLocaleSettingForProperCase(localeSetting);

			var currencySetting = CommonLogic.QueryStringCanBeDangerousContent("CurrencySetting");
			if(!string.IsNullOrEmpty(currencySetting))
				customer.CurrencySetting = Localization.CheckCurrencySettingForProperCase(currencySetting);

			if(!string.IsNullOrEmpty(CommonLogic.QueryStringCanBeDangerousContent("VATSetting")))
				customer.VATSettingRAW = (VATSettingEnum)CommonLogic.QueryStringNativeInt("VATSetting");
		}

		public void OnActionExecuted(ActionExecutedContext filterContext)
		{ }
	}
}
