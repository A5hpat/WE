// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Linq;
using System.Web.Mvc;
using AspDotNetStorefront.Caching.ObjectCaching;
using AspDotNetStorefront.Checkout;
using AspDotNetStorefront.Filters;
using AspDotNetStorefront.Models;
using AspDotNetStorefrontCore;
using AspDotNetStorefront.Routing;
using AspDotNetStorefrontCore.ShippingCalculation;

namespace AspDotNetStorefront.Controllers
{
	[SecureAccessFilter(forceHttps: true)]
	public class CheckoutShippingMethodController : Controller
	{
		readonly ICachedShippingMethodCollectionProvider CachedShippingMethodCollectionProvider;
		readonly ICachedShoppingCartProvider CachedShoppingCartProvider;
		readonly IEffectiveShippingAddressProvider EffectiveShippingAddressProvider;
		readonly NoticeProvider NoticeProvider;
		readonly IPersistedCheckoutContextProvider PersistedCheckoutContextProvider;
		readonly IShippingMethodCartItemApplicator ShippingMethodCartItemApplicator;

		public CheckoutShippingMethodController(
			ICachedShippingMethodCollectionProvider cachedShippingMethodCollectionProvider,
			ICachedShoppingCartProvider cachedShoppingCartProvider,
			IEffectiveShippingAddressProvider effectiveShippingAddressProvider,
			NoticeProvider noticeProvider,
			IPersistedCheckoutContextProvider persistedCheckoutContextProvider,
			IShippingMethodCartItemApplicator shippingMethodCartItemApplicator)
		{
			CachedShippingMethodCollectionProvider = cachedShippingMethodCollectionProvider;
			CachedShoppingCartProvider = cachedShoppingCartProvider;
			EffectiveShippingAddressProvider = effectiveShippingAddressProvider;
			NoticeProvider = noticeProvider;
			PersistedCheckoutContextProvider = persistedCheckoutContextProvider;
			ShippingMethodCartItemApplicator = shippingMethodCartItemApplicator;
		}

		[HttpGet, ImportModelStateFromTempData]
		public ActionResult ShippingMethod()
		{
			var customer = HttpContext.GetCustomer();
			var storeId = AppLogic.StoreID();
			var cart = CachedShoppingCartProvider.Get(customer, CartTypeEnum.ShoppingCart, storeId);
			var shippingAddress = EffectiveShippingAddressProvider.GetEffectiveShippingAddress(customer);
			var checkoutContext = PersistedCheckoutContextProvider.LoadCheckoutContext(customer);

			var shippingMethodModels = CachedShippingMethodCollectionProvider
				.Get(customer, shippingAddress, cart.CartItems, storeId)
				.Select(shippingMethod => new ShippingMethodRenderModel(
					id: shippingMethod.Id,
					name: shippingMethod.GetNameForDisplay(),
					rateDisplay: GetShippingMethodRateDisplay(shippingMethod, customer, cart),
					imageFileName: shippingMethod.ImageFileName));

			var selectedShippingMethodModel = shippingMethodModels
				.Where(shippingMethod => shippingMethod.Id == checkoutContext.SelectedShippingMethodId)
				.FirstOrDefault();

			var model = new SelectShippingMethodViewModel
			{
				RenderModel = new SelectShippingMethodRenderModel(
					shippingMethods: shippingMethodModels,
					selectedShippingMethod: selectedShippingMethodModel,
                    // nal
					//showShippingIcons: AppLogic.AppConfigBool("ShowShippingIcons"),
                    showShippingIcons: true,
					cartIsAllFreeShipping: !AppLogic.AppConfigBool("FreeShippingAllowsRateSelection") && cart.IsAllFreeShippingComponents(),
                    // nal
					//numberOfMethodsToShow: AppLogic.AppConfigNativeInt("NumberOfShippingMethodsToDisplay")),
                numberOfMethodsToShow: 0),
				SelectedShippingMethodId = checkoutContext.SelectedShippingMethodId,
			};

			return PartialView(ViewNames.ShippingMethodPartial, model);
		}

		[HttpPost]
		public ActionResult ShippingMethod(SelectShippingMethodViewModel model)
		{
			var customer = HttpContext.GetCustomer();
			var storeId = AppLogic.StoreID();

			if(!model.SelectedShippingMethodId.HasValue)
			{
				NoticeProvider.PushNotice("Please select a shipping method", NoticeType.Failure);
				return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
			}

			var cart = CachedShoppingCartProvider.Get(customer, CartTypeEnum.ShoppingCart, storeId);
			var shippingAddress = EffectiveShippingAddressProvider.GetEffectiveShippingAddress(customer);
			var checkoutContext = PersistedCheckoutContextProvider.LoadCheckoutContext(customer);

			var shippingMethods = CachedShippingMethodCollectionProvider.Get(customer, shippingAddress, cart.CartItems, storeId);

			var selectedShippingMethod = shippingMethods
				.Where(shippingMethod => shippingMethod.Id == model.SelectedShippingMethodId)
				.FirstOrDefault();

			if(selectedShippingMethod == null)
			{
				NoticeProvider.PushNotice("Please select a shipping method", NoticeType.Failure);
				return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
			}

			SetShippingMethod(selectedShippingMethod, cart, customer);
			return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
		}

		void SetShippingMethod(ShippingMethod shippingMethod, ShoppingCart cart, Customer customer)
		{
			int shippingMethodId;
			string shippingMethodNameForDatabase;

			if(cart.ShippingIsFree 
				&& !AppLogic.AppConfigBool("FreeShippingAllowsRateSelection")
				&& !AppLogic
					  .AppConfig("ShippingMethodIDIfFreeShippingIsOn")
					  .ParseAsDelimitedList<int>()
					  .Contains(shippingMethod.Id))
			{
				shippingMethodId = 0;
				shippingMethodNameForDatabase = string.Format(
					"{0} : {1}",
                    "FREE",
					cart.GetFreeShippingReason());
			}
			else
			{
				shippingMethodId = shippingMethod.Id;
				shippingMethodNameForDatabase = Shipping.GetActiveShippingCalculationID() != Shipping.ShippingCalculationEnum.UseRealTimeRates
					? Shipping.GetShippingMethodDisplayName(shippingMethod.Id, null)
					: Shipping.GetFormattedRealTimeShippingMethodForDatabase(shippingMethod.Name, shippingMethod.Freight, shippingMethod.VatRate);
			}

			// Update the persisted checkout context
			var checkoutContext = PersistedCheckoutContextProvider.LoadCheckoutContext(customer);
			PersistedCheckoutContextProvider.SaveCheckoutContext(
				customer,
				new PersistedCheckoutContext(
					creditCard: checkoutContext.CreditCard,
					payPalExpress: checkoutContext.PayPalExpress,
					purchaseOrder: checkoutContext.PurchaseOrder,
					braintree: checkoutContext.Braintree,
					amazonPayments: checkoutContext.AmazonPayments,
					termsAndConditionsAccepted: checkoutContext.TermsAndConditionsAccepted,
					over13Checked: checkoutContext.Over13Checked,
					shippingEstimateDetails: checkoutContext.ShippingEstimateDetails,
					offsiteRequiresBillingAddressId: checkoutContext.OffsiteRequiresBillingAddressId,
					offsiteRequiresShippingAddressId: checkoutContext.OffsiteRequiresShippingAddressId,
					email: checkoutContext.Email,
					selectedShippingMethodId: shippingMethodId));

			// Update the database for legacy cases
			ShippingMethodCartItemApplicator.UpdateCartItemsShippingMethod(customer, cart, shippingMethod);
		}

		string GetShippingMethodRateDisplay(ShippingMethod shippingMethod, Customer customer, ShoppingCart cart)
		{
			var freightDisplayText = string.Empty;

			if(!string.IsNullOrEmpty(customer.CurrencySetting))
			{
				// Add the VAT rate to the price
				var calculatedFreight = AppLogic.AppConfigBool("VAT.Enabled") && customer.VATSettingReconciled == VATSettingEnum.ShowPricesInclusiveOfVAT
					? shippingMethod.Freight + (shippingMethod.Freight * (Prices.TaxRate(customer, AppLogic.AppConfigNativeInt("ShippingTaxClassID")) / 100))
					: shippingMethod.Freight;

				freightDisplayText = Localization.CurrencyStringForDisplayWithExchangeRate(calculatedFreight, customer.CurrencySetting);

				if(shippingMethod.ShippingIsFree && Shipping.ShippingMethodIsInFreeList(shippingMethod.Id))
					freightDisplayText = string.Format("({0})", "FREE");

				if(cart.FreeShippingReason == Shipping.FreeShippingReasonEnum.AllDownloadItems || cart.FreeShippingReason == Shipping.FreeShippingReasonEnum.AllOrdersHaveFreeShipping)
					freightDisplayText = string.Empty;

				freightDisplayText += AddVatDetailsIfApplicable(customer.VATSettingReconciled);
			}

			return freightDisplayText;
		}

		string AddVatDetailsIfApplicable(VATSettingEnum vatSetting)
		{
			if(!AppLogic.AppConfigBool("VAT.Enabled"))
				return string.Empty;

			if(vatSetting == VATSettingEnum.ShowPricesInclusiveOfVAT)
				return string.Format(" {0}", "inc vat");

			return string.Format(" {0}", "Excluding VAT");
		}
	}
}
