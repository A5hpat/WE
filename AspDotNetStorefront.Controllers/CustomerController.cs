// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web.Mvc;
using AspDotNetStorefront.Filters;
using AspDotNetStorefront.Helpers;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Controllers
{
	[RequireCustomerRecordFilter]
	public class CustomerController : Controller
	{
		readonly NoticeProvider NoticeProvider;

		public CustomerController(NoticeProvider noticeProvider)
		{
			NoticeProvider = noticeProvider;
		}

		[HttpGet]
        public ActionResult SetLocale(string localeSetting, string returnUrl)
        {
			var customer = HttpContext.GetCustomer();
			var safeReturnUrl = Url.MakeSafeReturnUrl(returnUrl);

			SetCustomerLocale(customer, localeSetting);

			return new RedirectResult(safeReturnUrl);
        }

		[HttpGet]
		public ActionResult SetCurrency(string currencySetting, string returnUrl)
		{
			var customer = HttpContext.GetCustomer();
			var safeReturnUrl = Url.MakeSafeReturnUrl(returnUrl);

			SetCustomerCurrency(customer, currencySetting);

			return new RedirectResult(safeReturnUrl);
		}

		[HttpGet]
		public ActionResult SetVatSetting(string vatSetting, string vatRegistrationId, string returnUrl)
		{
			var customer = HttpContext.GetCustomer();
			var safeReturnUrl = Url.MakeSafeReturnUrl(returnUrl);

			SetCustomerVatSetting(customer, Customer.ValidateVATSetting(vatSetting), vatRegistrationId);

			return new RedirectResult(safeReturnUrl);
		}

		void SetCustomerLocale(Customer customer, string localeSetting)
		{
			if(string.IsNullOrEmpty(localeSetting))
				localeSetting = Localization.GetDefaultLocale();

			localeSetting = Localization.CheckLocaleSettingForProperCase(localeSetting);
			
			customer.LocaleSetting = localeSetting;

			//Now switch the currency to the locale's default
			SetCustomerCurrency(customer, AppLogic.GetLocaleDefaultCurrency(localeSetting));
		}

		void SetCustomerCurrency(Customer customer, string currencySetting)
		{
			if(string.IsNullOrEmpty(currencySetting))
				currencySetting = AppLogic.GetLocaleDefaultCurrency(customer.LocaleSetting);

			currencySetting = Localization.CheckCurrencySettingForProperCase(currencySetting);

			customer.CurrencySetting = currencySetting;
		}

		void SetCustomerVatSetting(Customer customer, string vatSetting, string vatRegistrationId)
		{
			var vatSettingRaw = (VATSettingEnum)int.Parse(vatSetting);
			customer.VATSettingRAW = vatSettingRaw;
			
			if(!string.IsNullOrEmpty(vatRegistrationId))
            {
				if(AppLogic.VATRegistrationIDIsValid(customer, vatRegistrationId))
					customer.SetVATRegistrationID(vatRegistrationId);
				else
					NoticeProvider.PushNotice("Unable to set the VAT Registration Id.  Contact the site administrator for details.", NoticeType.Warning);
			}			
		}
	}
}
