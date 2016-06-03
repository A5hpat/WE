// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AspDotNetStorefront.Checkout;
using AspDotNetStorefront.Classes;
using AspDotNetStorefront.Controllers.Classes;
using AspDotNetStorefront.Filters;
using AspDotNetStorefront.Models;
using AspDotNetStorefront.Models.Converter;
using AspDotNetStorefront.Routing;
using AspDotNetStorefront.Validation.AddressValidator;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontCore.Validation;

namespace AspDotNetStorefront.Controllers
{
	[RequireCustomerRecordFilter]
	[SecureAccessFilter(forceHttps: true)]
	public class AddressController : Controller
	{
		readonly AddressControllerHelper ControllerHelper;
		readonly AddressHeaderProvider AddressHeaderProvider;
		readonly IAddressValidationProviderFactory AddressValidationProviderFactory;
		readonly AddressSelectListBuilder SelectListBuilder;
		readonly NoticeProvider NoticeProvider;
		readonly IPostalCodeLookupProvider PostalCodeLookupProvider;
		readonly AddressViewModelConverter AddressViewModelConverter;
		readonly bool AllowDifferentShipTo;
		readonly bool ShowCompanyField;
		readonly bool ShowNickName;

		public AddressController(IAddressValidationProviderFactory addressValidationProviderFactory,
			AddressControllerHelper helper,
			AddressHeaderProvider addressHeaderProvider,
			AddressSelectListBuilder selectListBuilder,
			NoticeProvider noticeProvider,
			IPostalCodeLookupProvider postalCodeLookupProvider,
			AddressViewModelConverter addressViewModelConverter)
		{
			AddressValidationProviderFactory = addressValidationProviderFactory;
			ControllerHelper = helper;
			AddressHeaderProvider = addressHeaderProvider;
			SelectListBuilder = selectListBuilder;
			NoticeProvider = noticeProvider;
			PostalCodeLookupProvider = postalCodeLookupProvider;
			AddressViewModelConverter = addressViewModelConverter;
			AllowDifferentShipTo = true;
			ShowCompanyField = true;
			ShowNickName = true;
		}

		[HttpGet]
		public ActionResult Index(string addressType)
		{
			var customer = HttpContext.GetCustomer();

			ViewBag.MetaTitle = "Your Address Book";

			AddressTypes addressTypeValue;
			if(!Enum.TryParse(
				value: addressType, 
				ignoreCase: true, 
				result: out addressTypeValue))
			{
				addressTypeValue = AddressTypes.Unknown;
			}

			var model = new AddressIndexViewModel(
				addresses: ControllerHelper
					.GetCustomerAddresses(customer)
					.Where(a => string.IsNullOrEmpty(a.OffsiteSource)),
				returnUrl: ControllerHelper.GetRedirectUrl(Url),
				allowDifferentShipTo: AllowDifferentShipTo,
				addressType: addressTypeValue);

			return View(model);
		}

		[HttpGet]
		public ActionResult Detail(int? addressId, string addressType, string returnUrl, bool makePrimary = false)
		{
			var customer = HttpContext.GetCustomer();

			var safeReturnUrl = Url.MakeSafeReturnUrl(returnUrl, Url.Action(ActionNames.Index));

			ViewBag.MetaTitle = "Your Address Book";

			var address = ControllerHelper.GetCustomerAddress(addressId, customer);

			AddressTypes addressTypeValue;
			if(!Enum.TryParse(addressType, out addressTypeValue))
				addressTypeValue = AddressTypes.Unknown;

			return View(new AddressDetailViewModel(
				address: address,
				residenceTypeOptions: SelectListBuilder.BuildResidenceTypeSelectList(address.ResidenceType),
				stateOptions: SelectListBuilder.BuildStateSelectList(address.Country, address.State),
				countryOptions: SelectListBuilder.BuildCountrySelectList(address.Country),
				showCompanyField: ShowCompanyField,
				showNickName: ShowNickName,
				showResidenceTypeField: addressTypeValue == AddressTypes.Shipping || !AllowDifferentShipTo,
				returnUrl: safeReturnUrl,
				header: AddressHeaderProvider.GetHeaderText(address.Id, addressTypeValue),
				showPostalCodeLookup: PostalCodeLookupProvider.IsEnabled(string.IsNullOrEmpty(address.Country)
					? AppLogic.GetCountryNameFromTwoLetterISOCode("US")
					: address.Country),
				addressType: addressTypeValue,
				makePrimary: makePrimary));
		}

		[HttpPost]
		public ActionResult Detail(AddressPostViewModel model, string addressType, string returnUrl, bool makePrimary = false)
		{
			var customer = HttpContext.GetCustomer();

			var safeReturnUrl = Url.MakeSafeReturnUrl(returnUrl, Url.Action(ActionNames.Index));

			if(model.Address.Id.HasValue)
			{
				var verificationAddress = new Address();
				verificationAddress.LoadFromDB(model.Address.Id.Value);

				if(verificationAddress.CustomerID != customer.CustomerID)
					throw new HttpException(404, null);
			}

			AddressTypes addressTypeValue;
			if(!Enum.TryParse(addressType, out addressTypeValue))
				addressTypeValue = AddressTypes.Unknown;

			if(!ModelState.IsValid)
				return View(ActionNames.Detail, new AddressDetailViewModel(
					address: model.Address,
					residenceTypeOptions: SelectListBuilder.BuildResidenceTypeSelectList(model.Address.ResidenceType),
					stateOptions: SelectListBuilder.BuildStateSelectList(model.Address.Country, model.Address.State),
					countryOptions: SelectListBuilder.BuildCountrySelectList(model.Address.Country),
					showCompanyField: ShowCompanyField,
					showNickName: ShowNickName,
					showResidenceTypeField: addressTypeValue == AddressTypes.Shipping || !AllowDifferentShipTo,
					showPostalCodeLookup: PostalCodeLookupProvider.IsEnabled(model.Address.Country),
					returnUrl: safeReturnUrl,
					header: AddressHeaderProvider.GetHeaderText(model.Address.Id, addressTypeValue),
					makePrimary: makePrimary));

			var addressValidationResults = AddressValidationProviderFactory
				.Create()
				.Select(v => v
					.Validate(
						address: AddressViewModelConverter.ConvertToAddress(model.Address, customer),
						addressType: addressTypeValue)
						)
				.ToArray();

			if(addressValidationResults
				.Where(v => v.Status == AddressValidationStatus.Failure)
				.Any())
			{
				foreach(var addressValidationResult in addressValidationResults
					.Where(v => v.Status == AddressValidationStatus.Failure))
					NoticeProvider.PushNotice(
						message: string.Concat("Address is not valid:", addressValidationResult.Message),
						type: NoticeType.Failure);

				var correctedAddress = addressValidationResults
					.Where(v => v.CorrectedAddresses != null)
					.SelectMany(v => v
						.CorrectedAddresses
						.Select(a => AddressViewModelConverter.ConvertToAddressViewModel(a, customer)))
					.FirstOrDefault()
					?? model.Address;

				//remove fields USPS could have modified so corrected values will display on front end
				ModelState.Remove("Address.Address1");
				ModelState.Remove("Address.Address2");
				ModelState.Remove("Address.City");
				ModelState.Remove("Address.State");
				ModelState.Remove("Address.Zip");

				return View(ActionNames.Detail, new AddressDetailViewModel(
					address: correctedAddress,
					residenceTypeOptions: SelectListBuilder.BuildResidenceTypeSelectList(model.Address.ResidenceType),
					stateOptions: SelectListBuilder.BuildStateSelectList(model.Address.Country, model.Address.State),
					countryOptions: SelectListBuilder.BuildCountrySelectList(model.Address.Country),
					showCompanyField: ShowCompanyField,
					showNickName: ShowNickName,
					showResidenceTypeField: addressTypeValue == AddressTypes.Shipping || !AllowDifferentShipTo,
					showPostalCodeLookup: PostalCodeLookupProvider.IsEnabled(correctedAddress.Country),
					returnUrl: safeReturnUrl,
					header: AddressHeaderProvider.GetHeaderText(correctedAddress.Id, addressTypeValue),
					makePrimary: makePrimary));
			}

			var updatedAddressId = ControllerHelper.UpdateAddress(model.Address, customer);

			if(makePrimary)
			{
				// Read the address back out of the database so that we can guarantee an address id before we try to make it a primary address
				var address = ControllerHelper.GetCustomerAddress(updatedAddressId, customer);
				MakePrimary(customer, addressTypeValue, address);
			}

			return Redirect(safeReturnUrl);
		}

		public ActionResult Delete(int addressId, string returnUrl)
		{
			var customer = HttpContext.GetCustomer();

			ControllerHelper.DeleteAddress(addressId, customer);

			var safeReturnUrl = Url.MakeSafeReturnUrl(returnUrl, Url.Action(ActionNames.Index));

			return Redirect(safeReturnUrl);
		}

		public ActionResult MakePrimaryAddress(int addressId, AddressTypes addressType, string returnUrl)
		{
			var customer = HttpContext.GetCustomer();
			var address = ControllerHelper.GetCustomerAddress(addressId, customer);

			var safeReturnUrl = Url.MakeSafeReturnUrl(returnUrl, Url.Action(ActionNames.Index));

			var verificationAddress = new Address();
			verificationAddress.LoadFromDB(address.Id.Value);
			if(verificationAddress.CustomerID != customer.CustomerID)
				throw new HttpException(404, null);

			MakePrimary(customer, addressType, address);

			return Redirect(safeReturnUrl);
		}

		void MakePrimary(Customer customer, AddressTypes addressType, AddressViewModel address)
		{
			if (!address.Id.HasValue)
				return;

			var customerId = DB.GetSqlN("select CustomerID as N from Address where AddressId = @addressId", new SqlParameter("@addressId", address.Id.Value));
			if (customer.CustomerID != customerId)
				return;

			//Change both if the site requires it
            // nal
			//if(!AppLogic.AppConfigBool("AllowShipToDifferentThanBillTo"))
			//	addressType = AddressTypes.Account;

			//If they do not have a primary address of the opposite type we might as well set that too.
			if((addressType == AddressTypes.Billing && customer.PrimaryShippingAddressID == 0)
				|| addressType == AddressTypes.Shipping && customer.PrimaryBillingAddressID == 0)
				addressType = AddressTypes.Account;

			switch(addressType)
			{
				case AddressTypes.Billing:
					ControllerHelper.UpdatePrimaryBilling(address, customer);
					if(!AllowDifferentShipTo)
						ControllerHelper.UpdatePrimaryShipping(address, customer);
					break;

				case AddressTypes.Shipping:
					//Shipping address may not be eligible to be a primary address.
					if(!ControllerHelper.CanBePrimaryShippingAddress(address))
						return;

					ControllerHelper.UpdatePrimaryShipping(address, customer);
					if(!AllowDifferentShipTo)
						ControllerHelper.UpdatePrimaryBilling(address, customer);
					break;

				default: // Make the address primary billing and shipping.
					//Shipping address may not be eligible to be a primary address.
					if(!ControllerHelper.CanBePrimaryShippingAddress(address))
						return;

					ControllerHelper.UpdatePrimaryBilling(address, customer);
					ControllerHelper.UpdatePrimaryShipping(address, customer);
					break;
			}

			ControllerHelper.UpdatePrimaryBillingEqualsShipping(customer);
		}

		public ActionResult PostalCodeLookup(string postalCode, string countryCode)
		{
			return Json(
				data: PostalCodeLookupProvider.Lookup(postalCode, countryCode),
				behavior: JsonRequestBehavior.AllowGet);
		}
	}
}
