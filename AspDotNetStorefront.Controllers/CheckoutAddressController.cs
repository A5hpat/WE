// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Linq;
using System.Web.Mvc;
using AspDotNetStorefront.Checkout;
using AspDotNetStorefront.Controllers.Classes;
using AspDotNetStorefront.Models;
using AspDotNetStorefront.Validation.AddressValidator;
using AspDotNetStorefrontCore;
using AspDotNetStorefront.Routing;
using AspDotNetStorefront.Models.Converter;
using AspDotNetStorefront.Filters;

namespace AspDotNetStorefront.Controllers
{
	[SecureAccessFilter(forceHttps: true)]
	public class CheckoutAddressController : Controller
	{
		readonly NoticeProvider NoticeProvider;
		readonly IPersistedCheckoutContextProvider PersistedCheckoutContextProvider;
		readonly IAddressValidationProviderFactory AddressValidationProviderFactory;
		readonly AddressViewModelConverter AddressViewModelConverter;
		readonly AddressControllerHelper AddressControllerHelper;

		public CheckoutAddressController(
			NoticeProvider noticeProvider,
			IPersistedCheckoutContextProvider persistedCheckoutContextProvider,
			IAddressValidationProviderFactory addressValidationProviderFactory,
			AddressViewModelConverter addressViewModelConverter,
			AddressControllerHelper addressControllerHelper)
		{
			NoticeProvider = noticeProvider;
			PersistedCheckoutContextProvider = persistedCheckoutContextProvider;
			AddressValidationProviderFactory = addressValidationProviderFactory;
			AddressViewModelConverter = addressViewModelConverter;
			AddressControllerHelper = addressControllerHelper;
		}

		public ActionResult SelectAddress(AddressTypes addressType)
		{
			var customer = HttpContext.GetCustomer();
			var primaryAddressId = addressType == AddressTypes.Shipping
				? customer.PrimaryShippingAddressID
				: customer.PrimaryBillingAddressID;

			var checkoutContext = PersistedCheckoutContextProvider.LoadCheckoutContext(customer);

			var pageTitle = string.Empty;

			if(!AppLogic.AppConfigBool("AllowShipToDifferentThanBillTo") || addressType == AddressTypes.Account)
				pageTitle = "Choose your shipping and billing address";
			else if(addressType == AddressTypes.Shipping)
				pageTitle = "Choose a shipping address";
			else
				pageTitle = "Choose a billing address";

			var addresses = AddressControllerHelper.GetCustomerAddresses(customer);

			var model = new SelectAddressViewModel
			{
				SelectedAddressId = primaryAddressId,
				SelectedAddress = addresses
					.Where(address => address.Id == primaryAddressId)
					.FirstOrDefault(),
				AddressOptions = addresses,
				AddressType = addressType,
				PageTitle = pageTitle,
				AddressSelectionLocked = (addressType == AddressTypes.Billing && checkoutContext.OffsiteRequiresBillingAddressId.HasValue)
					|| (addressType == AddressTypes.Shipping && checkoutContext.OffsiteRequiresShippingAddressId.HasValue)
			};

			return PartialView(ViewNames.SelectAddressPartial, model);
		}

		[HttpPost]
		public ActionResult SelectAddress(SelectAddressViewModel model)
		{
			if(!(model.SelectedAddressId > 0))
			{
				NoticeProvider.PushNotice("Please select a valid address", NoticeType.Failure);
				return RedirectToAction(
					ActionNames.SelectAddress,
					ControllerNames.CheckoutAddress);
			}

			return RedirectToAction(
				ActionNames.MakePrimaryAddress,
				ControllerNames.Address,
				new
				{
					addressId = model.SelectedAddressId,
					addressType = model.AddressType,
					returnUrl = Url.Action(ActionNames.Index, ControllerNames.Checkout)
				});
		}
	}
}
