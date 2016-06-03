// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AspDotNetStorefront.Checkout;
using AspDotNetStorefront.Classes;
using AspDotNetStorefront.Filters;
using AspDotNetStorefront.Models;
using AspDotNetStorefront.Payment.Wallet;
using AspDotNetStorefrontCore;
using AspDotNetStorefront.Routing;
using GatewayAuthorizeNet;

namespace AspDotNetStorefront.Controllers
{
	[Authorize]
	[RequireCustomerRecordFilter]
	[SecureAccessFilter(forceHttps: true)]
	public class WalletController : Controller
	{
		const string SelectListValueField = "Value";
		const string SelectListDataField = "Text";
		
		readonly IWalletProvider WalletProvider;
		readonly IPersistedCheckoutContextProvider PersistedCheckoutContextProvider;
		readonly NoticeProvider NoticeProvider;
		readonly ISkinProvider SkinProvider;

		public WalletController(
			IWalletProvider walletProvider,
			IPersistedCheckoutContextProvider persistedCheckoutContextProvider,
			NoticeProvider noticeProvider,
			ISkinProvider skinProvider)
		{
			WalletProvider = walletProvider;
			PersistedCheckoutContextProvider = persistedCheckoutContextProvider;
			NoticeProvider = noticeProvider;
			SkinProvider = skinProvider;
		}

		[ChildActionOnly]
		[ImportModelStateFromTempData]
		public ActionResult SelectWallet()
		{
			var customer = HttpContext.GetCustomer();
			var paymentProfiles = WalletProvider.GetPaymentProfiles(customer);

			return PartialView(new WalletSelectViewModel(
				walletPaymentTypes: paymentProfiles
					.Select(profile => new WalletPaymentType
						{
							PaymentProfileId = profile.PaymentProfileId,
							CardType = profile.CardType,
							CardNumber = profile.CreditCardNumberMasked,
							ExpirationMonth = profile.ExpirationMonth,
							ExpirationYear = profile.ExpirationYear,
							CardImage = Url.Content(string.Format("~/skins/{0}/Images/{1}",
								SkinProvider.GetSkinNameById(customer.SkinID),
								DisplayTools.GetCardImage(string.Empty, profile.CardType)))
						})));
		}

		[HttpPost]
		[ExportModelStateToTempData]
		public ActionResult SelectWallet(WalletSelectViewModel model)
		{
			if(!ModelState.IsValid)
				return RedirectToAction(ActionNames.CreditCard, ControllerNames.CheckoutCreditCard);

			var customer = HttpContext.GetCustomer();

			if(!customer.Owns.Wallet(model.SelectedPaymentProfileId))
				throw new HttpException(403, "Forbidden");

			customer.UpdateCustomer(requestedPaymentMethod: AppLogic.ro_PMCreditCard);
			customer.ThisCustomerSession["ActivePaymentProfileId"] = model.SelectedPaymentProfileId.ToString();

			var paymentProfile = WalletProvider.GetPaymentProfile(customer, model.SelectedPaymentProfileId);
			var checkoutContext = PersistedCheckoutContextProvider.LoadCheckoutContext(customer);

			var expirationDate = new DateTime(
				year: int.Parse(paymentProfile.ExpirationYear),
				month: int.Parse(paymentProfile.ExpirationMonth),
				day: DateTime.Now.Day);

			PersistedCheckoutContextProvider.SaveCheckoutContext(
				customer: customer,
				checkoutContext: new PersistedCheckoutContext(
					creditCard: new CreditCardDetails(
						name: string.Format("{0} {1}", customer.FirstName, customer.LastName),
						number: paymentProfile.CreditCardNumberMasked,
						issueNumber: null,
						cardType: paymentProfile.CardType,
						expirationDate: expirationDate,
						startDate: null,
						cvv: string.Empty),
					payPalExpress: checkoutContext.PayPalExpress,
					purchaseOrder: checkoutContext.PurchaseOrder,
					braintree: checkoutContext.Braintree,
					amazonPayments: null,
					termsAndConditionsAccepted: checkoutContext.TermsAndConditionsAccepted,
					over13Checked: checkoutContext.Over13Checked,
					shippingEstimateDetails: checkoutContext.ShippingEstimateDetails,
					offsiteRequiresBillingAddressId: null,
					offsiteRequiresShippingAddressId: null,
					email: checkoutContext.Email,
					selectedShippingMethodId: checkoutContext.SelectedShippingMethodId));

			return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
		}

		[HttpGet]
		public ActionResult Index()
		{
			if(!WalletProvider.WalletsAreEnabled())
			{
				NoticeProvider.PushNotice("Wallets are not enabled.", NoticeType.Warning);
				return RedirectToAction(ActionNames.Index, ControllerNames.Account);
			}

			var customer = HttpContext.GetCustomer();

			if(!CustomerHasAvailableAddresses(customer))
			{
				NoticeProvider.PushNotice(
					message: "You must have a billing address configured before you can add a credit card to your account.",
					type: NoticeType.Warning);

				return RedirectToAction(ActionNames.Index, ControllerNames.Account);
			}

			return View(new WalletIndexViewModel(
				paymentTypes: WalletProvider
					.GetPaymentProfiles(customer)
					.Select(p => new WalletPaymentType
					{
						CardType = p.CardType,
						CardImage = DisplayTools.GetCardImage(
							imagePath: VirtualPathUtility.ToAbsolute(string.Format("~/skins/{0}/images/", SkinProvider.GetSkinNameById(customer.SkinID).ToLower())),
							cardName: p.CardType),
						CardNumber = p.CreditCardNumberMasked,
						ExpirationMonth = p.ExpirationMonth,
						ExpirationYear = p.ExpirationYear,
						PaymentProfileId = p.PaymentProfileId
					})));
		}

		[HttpGet]
		[SecureAccessFilter(forceHttps: true)]
		public ActionResult Edit(long? id)
		{
			if(!WalletProvider.WalletsAreEnabled())
			{
				NoticeProvider.PushNotice(
					message: "Wallets are not enabled.",
					type: NoticeType.Warning);

				return RedirectToAction(ActionNames.Index, ControllerNames.Account);
			}

			var customer = HttpContext.GetCustomer();
			if(!CustomerHasAvailableAddresses(customer))
			{
				NoticeProvider.PushNotice(
					message: "You must have a billing address configured before you can add a credit card to your account.",
					type: NoticeType.Warning);

				return RedirectToAction(ActionNames.Index, ControllerNames.Account);
			}

			var profileId = WalletProvider.GetProfileId(customer);
			if(profileId <= 0)
				return View(BuildWalletEditViewModel(customer));

			if(!id.HasValue)
				return View(BuildWalletEditViewModel(customer));

			var paymentProfile = WalletProvider.GetPaymentProfile(customer, id.Value);
			if(paymentProfile == null)
				return View(BuildWalletEditViewModel(customer));

			return View(BuildWalletEditViewModel(customer, new WalletPaymentType
				{
					PaymentProfileId = id.Value,
					BillingAddressId = paymentProfile.AddressId,
					CardNumber = paymentProfile.CreditCardNumberMasked,
					ExpirationMonth = paymentProfile.ExpirationMonth,
					ExpirationYear = paymentProfile.ExpirationYear
				}));
		}
		
		[HttpPost]
		[SecureAccessFilter(forceHttps: true)]
		public ActionResult Edit(WalletPaymentType model)
		{
			if(!ModelState.IsValid)
				return View(BuildWalletEditViewModel(HttpContext.GetCustomer()));

			var customer = HttpContext.GetCustomer();
			if(model.PaymentProfileId < 0
				&& !customer.Owns.Wallet(model.PaymentProfileId))
				throw new HttpException(403, "Forbidden");

			if(!WalletProvider.WalletsAreEnabled())
			{
				NoticeProvider.PushNotice(
					message: "Wallets are not enabled.",
					type: NoticeType.Warning);

				return RedirectToAction(ActionNames.Index, ControllerNames.Account);
			}

			string errorMessage, errorCode;
			var paymentProfileId = ProcessTools
				.SaveProfileAndPaymentProfile(
					customerId: customer.CustomerID,
					email: customer.EMail,
					storeName: AppLogic.AppConfig("StoreName"),
					paymentProfileId: model.PaymentProfileId,
					addressId: model.BillingAddressId,
					cardNumber: model.CardNumber,
					cardCode: model.CardSecurityCode,
					expMonth: model.ExpirationMonth,
					expYear: model.ExpirationYear,
					errorMessage: out errorMessage,
					errorCode: out errorCode);

			if(paymentProfileId <= 0)
			{
				var message = string.Empty;

				if(errorCode == "E00039")
					message = "This card has already been saved to your wallet. Please edit the existing card.";
				else
					message = String.Format("{0} {1}",
                        "This card has already been saved to your wallet. Please edit the existing card.",
						errorMessage);

				NoticeProvider.PushNotice(message, NoticeType.Failure);

				return View(BuildWalletEditViewModel(HttpContext.GetCustomer()));
			}

			if(model.MakePrimary)
			{
				var address = new Address();
				address.LoadFromDB(model.BillingAddressId);
				address.MakeCustomersPrimaryAddress(AddressTypes.Billing);
				DataUtility.SetPrimaryPaymentProfile(customer.CustomerID, paymentProfileId);
			}

			return RedirectToAction(ActionNames.Index, ControllerNames.Wallet);
		}

		[HttpGet]
		public ActionResult Delete(long id)
		{
			var customer = HttpContext.GetCustomer();
			if(!customer.Owns.Wallet(id))
				throw new HttpException(403, "Forbidden");

			if(!WalletProvider.WalletsAreEnabled())
			{
				NoticeProvider.PushNotice(
					message: "Wallets are not enabled.",
					type: NoticeType.Warning);

				return RedirectToAction(ActionNames.Index, ControllerNames.Account);
			}

			WalletProvider.DeletePaymentProfile(customer, id);

			return RedirectToAction(ActionNames.Index, ControllerNames.Wallet);
		}

		WalletEditViewModel BuildWalletEditViewModel(Customer customer, WalletPaymentType model = null)
		{
			var walletEditViewModel = new WalletEditViewModel(
				dateExpirationMonthOptions: BuildMonthOptionsSelectList(),
				dateExpirationYearOptions: BuildYearOptionsSelectList(),
				billingAddressOptions: BuildBillingAddressesSelectList(customer.CustomerID));

			if (model != null)
			{
				walletEditViewModel.BillingAddressId = model.BillingAddressId;
				walletEditViewModel.CardImage = model.CardImage;
				walletEditViewModel.CardNumber = model.CardNumber;
				walletEditViewModel.CardSecurityCode = model.CardSecurityCode;
				walletEditViewModel.CardType = model.CardType;
				walletEditViewModel.ExpirationMonth = model.ExpirationMonth;
				walletEditViewModel.ExpirationYear = model.ExpirationYear;
				walletEditViewModel.MakePrimary = model.MakePrimary;
				walletEditViewModel.PaymentProfileId = model.PaymentProfileId;
			}

			return walletEditViewModel;
		}

		SelectList BuildMonthOptionsSelectList(string selectedValue = null)
		{
			return new SelectList(
				items: Enumerable.Range(1, 12)
					.Select(month => new SelectListItem
						{
							Text = month.ToString(),
							Value = month.ToString()
						}),
				dataValueField: SelectListValueField,
				dataTextField: SelectListDataField,
				selectedValue: selectedValue);
		}

		SelectList BuildYearOptionsSelectList(string selectedValue = null)
		{
			return new SelectList(
				items: Enumerable.Range(DateTime.Now.Date.Year, 12)
					.Select(month => new SelectListItem
						{
							Text = month.ToString("D2"),
							Value = month.ToString("D2")
						}),
				dataValueField: SelectListValueField,
				dataTextField: SelectListDataField,
				selectedValue: selectedValue);
		}

		SelectList BuildBillingAddressesSelectList(int customerId, string selectedValue = null)
		{
			var selectListItems = new List<SelectListItem>();

			using(var connection = new SqlConnection(DB.GetDBConn()))
			{
				connection.Open();

				var query = @"select AddressID, Address1, City, State, Zip, Country
							from Address
							where CustomerID = @customerId
								and (OffsiteSource = '' or OffsiteSource is null)
								and Deleted = 0";

				using(var reader = DB.GetRS(query, connection, new SqlParameter("@customerId", customerId)))
					while(reader.Read())
						selectListItems.Add(new SelectListItem
							{
								Value = reader["AddressID"].ToString(),
								Text = string.Format("{0}, {1}, {2} {3} {4}",
									reader["Address1"],
									reader["City"],
									reader["State"],
									reader["Zip"],
									reader["Country"])
							});
			}

			return new SelectList(
				items: selectListItems,
				dataValueField: SelectListValueField,
				dataTextField: SelectListDataField,
				selectedValue: selectedValue);
		}

		bool CustomerHasAvailableAddresses(Customer customer)
		{
			var addressCount = DB.GetSqlN(
				@"select count(*) as N 
					from Address 
					where CustomerId = @customerId
						and (OffsiteSource = '' or OffsiteSource is null)
						and Deleted = 0",
				new SqlParameter("@customerId", customer.CustomerID));

			return addressCount > 0;
		}
	}
}
