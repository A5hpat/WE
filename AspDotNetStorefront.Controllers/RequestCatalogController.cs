// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Text;
using System.Web.Mvc;
using AspDotNetStorefront.Checkout;
using AspDotNetStorefront.Classes;
using AspDotNetStorefront.Filters;
using AspDotNetStorefront.Models;
using AspDotNetStorefront.Models.Converter;
using AspDotNetStorefrontCore;
using AspDotNetStorefront.Routing;
using AspDotNetStorefrontCore.Validation;

namespace AspDotNetStorefront.Controllers
{
	[SecureAccessFilter(forceHttps: true)]
	public class RequestCatalogController : Controller
	{
		readonly AddressHeaderProvider AddressHeaderProvider;
		readonly AddressSelectListBuilder AddressSelectListBuilder;
		readonly NoticeProvider NoticeProvider;
		readonly IPostalCodeLookupProvider PostalCodeLookupProvider;
		readonly AddressViewModelConverter AddressViewModelConverter;
		readonly bool ShowCompanyField;
		readonly bool ShowNickName;

		public RequestCatalogController(
			AddressHeaderProvider addressHeaderProvider,
			AddressSelectListBuilder addressSelectListBuilder,
			NoticeProvider noticeProvider,
			IPostalCodeLookupProvider postalCodeLookupProvider,
			AddressViewModelConverter addressViewModelConverter)
		{
			AddressHeaderProvider = addressHeaderProvider;
            AddressSelectListBuilder = addressSelectListBuilder;
			NoticeProvider = noticeProvider;
			PostalCodeLookupProvider = postalCodeLookupProvider;
			AddressViewModelConverter = addressViewModelConverter;
			ShowCompanyField = AppLogic.AppConfigBool("Address.CollectCompany");
			ShowNickName = AppLogic.AppConfigBool("Address.CollectNickName");
		}

		public ActionResult Index()
		{
			var customer = HttpContext.GetCustomer();

			var shippingAddress = AddressViewModelConverter.ConvertToAddressViewModel(
				customer.PrimaryShippingAddress,
				customer);

			return View(new AddressDetailViewModel(
				address: shippingAddress,
				residenceTypeOptions: AddressSelectListBuilder.BuildResidenceTypeSelectList(shippingAddress.ResidenceType.ToString()),
				stateOptions: AddressSelectListBuilder.BuildStateSelectList(shippingAddress.Country, shippingAddress.State),
				countryOptions: AddressSelectListBuilder.BuildCountrySelectList(shippingAddress.Country),
				showCompanyField: ShowCompanyField,
				showNickName: ShowNickName,
				showResidenceTypeField: true,
				showPostalCodeLookup: PostalCodeLookupProvider.IsEnabled(shippingAddress.Country),
				returnUrl: string.Empty,
				header: AddressHeaderProvider.GetHeaderText(shippingAddress.Id, AddressTypes.Shipping)));
		}

		[HttpPost]
		public ActionResult Index(AddressPostViewModel model)
		{
			var customer = HttpContext.GetCustomer();

			if(ModelState.IsValid)
			{
				SendCatalogEmailTo(model.Address, customer);

				return RedirectToAction(ActionNames.Detail, ControllerNames.Topic, new { name = "RequestCatalogSuccessful" });
			}

			return View(new AddressDetailViewModel(
				address: model.Address,
				residenceTypeOptions: AddressSelectListBuilder.BuildResidenceTypeSelectList(model.Address.ResidenceType.ToString()),
				stateOptions: AddressSelectListBuilder.BuildStateSelectList(model.Address.Country, model.Address.State),
				countryOptions: AddressSelectListBuilder.BuildCountrySelectList(model.Address.Country),
				showCompanyField: ShowCompanyField,
				showNickName: ShowNickName,
				showResidenceTypeField: true,
				showPostalCodeLookup: PostalCodeLookupProvider.IsEnabled(model.Address.Country),
				returnUrl: string.Empty,
				header: AddressHeaderProvider.GetHeaderText(model.Address.Id, AddressTypes.Shipping)));
		}

		public ActionResult ChangeCountry(AddressPostViewModel model)
		{
			var customer = HttpContext.GetCustomer();

			return View(ActionNames.Index, new AddressDetailViewModel(
				address: model.Address,
				residenceTypeOptions: AddressSelectListBuilder.BuildResidenceTypeSelectList(model.Address.ResidenceType.ToString()),
				stateOptions: AddressSelectListBuilder.BuildStateSelectList(model.Address.Country, model.Address.State),
				countryOptions: AddressSelectListBuilder.BuildCountrySelectList(model.Address.Country),
				showCompanyField: ShowCompanyField,
				showNickName: ShowNickName,
				showResidenceTypeField: true,
				showPostalCodeLookup: PostalCodeLookupProvider.IsEnabled(model.Address.Country),
				returnUrl: string.Empty,
				header: AddressHeaderProvider.GetHeaderText(model.Address.Id, AddressTypes.Shipping)));
		}

		void SendCatalogEmailTo(AddressViewModel address, Customer customer)
		{
			var body = new StringBuilder();
			body.AppendLine("CATALOG REQUEST:");
			body.AppendLine(string.Format("Customer Name: {0} {1}", address.FirstName, address.LastName));
			body.AppendLine(string.Format("Company: {0}", address.Company));
			body.AppendLine(string.Format("Residence Type: {0}", address.ResidenceType));
			body.AppendLine(string.Format("Address1: {0}", address.Address1));
			body.AppendLine(string.Format("Address2: {0}", address.Address2));
			body.AppendLine(string.Format("Suite: {0}", address.Suite));
			body.AppendLine(string.Format("City: {0}", address.City));
			body.AppendLine(string.Format("State: {0}", address.State));
			body.AppendLine(string.Format("Country: {0}", address.Country));
			body.AppendLine(string.Format("ZIP: {0}", address.Zip));

			AppLogic.SendMail(
				subject: "CATALOG REQUEST:",
				body: body.ToString(),
				useHtml: false,
				fromAddress: AppLogic.AppConfig("MailMe_FromAddress"),
				fromName: AppLogic.AppConfig("MailMe_FromName"),
				toAddress: AppLogic.AppConfig("MailMe_ToAddress"),
				toName: AppLogic.AppConfig("MailMe_ToName"),
				bccAddresses: string.Empty,
				server: AppLogic.MailServer());
		}
	}
}
