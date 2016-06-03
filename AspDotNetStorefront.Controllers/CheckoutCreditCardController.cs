// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Linq;
using System.Web.Mvc;
using AspDotNetStorefront.Checkout;
using AspDotNetStorefront.Classes;
using AspDotNetStorefront.Filters;
using AspDotNetStorefront.Models;
using AspDotNetStorefront.Payment.Wallet;
using AspDotNetStorefront.Routing;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways;

namespace AspDotNetStorefront.Controllers
{
	[SecureAccessFilter(forceHttps: true)]
	public class CheckoutCreditCardController : Controller
	{
		readonly CreditCardTypeProvider CreditCardTypeProvider;
		readonly ICreditCardValidationProvider CreditCardValidationProvider;
		readonly NoticeProvider NoticeProvider;
		readonly IPaymentOptionProvider PaymentOptionProvider;
		readonly IPersistedCheckoutContextProvider PersistedCheckoutContextProvider;
		readonly IWalletProvider WalletProvider;

		public CheckoutCreditCardController(
			CreditCardTypeProvider creditCardTypeProvider,
			ICreditCardValidationProvider creditCardValidationProvider,
			NoticeProvider noticeProvider,
			IPaymentOptionProvider paymentOptionProvider,
			IPersistedCheckoutContextProvider persistedCheckoutContextProvider,
			IWalletProvider walletProvider)
		{
			CreditCardTypeProvider = creditCardTypeProvider;
			CreditCardValidationProvider = creditCardValidationProvider;
			NoticeProvider = noticeProvider;
			PaymentOptionProvider = paymentOptionProvider;
			PersistedCheckoutContextProvider = persistedCheckoutContextProvider;
			WalletProvider = walletProvider;
		}

		[PageTypeFilter(PageTypes.Checkout)]
		[HttpGet]
		[ImportModelStateFromTempData]
		public ActionResult CreditCard()
		{
			var customer = HttpContext.GetCustomer();

			if(!PaymentOptionProvider.PaymentMethodSelectionIsValid(AppLogic.ro_PMCreditCard, customer))
			{
				NoticeProvider.PushNotice(
					message: "Invalid payment method!  Please choose another.",
					type: NoticeType.Failure);
				return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
			}

			//Decide which form to display
			if(AppLogic.ActivePaymentGatewayCleaned() == Gateway.ro_GWBRAINTREE)
			{
				var processor = GatewayLoader.GetProcessor(Gateway.ro_GWBRAINTREE);

				var clientToken = processor.ObtainBraintreeToken();

				if(string.IsNullOrEmpty(clientToken))
				{
					NoticeProvider.PushNotice("Our credit card processor is currently excperiencing difficulties.  Please try another payment method or contact us for assistance.", NoticeType.Failure);
                    return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
				}

				var braintreeModel = new BraintreeViewModel(token: clientToken,
					scriptUrl: AppLogic.AppConfig("Braintree.ScriptUrl"));

				return View(ViewNames.BraintreeCreditCard, braintreeModel);
			}
			else
			{
				var ccModel = BuildCheckoutCreditCardViewModel(customer);
				return View(ViewNames.CreditCard, ccModel);
			}
		}

		[HttpPost]
		[ExportModelStateToTempData]
		public ActionResult CreditCard(CheckoutCreditCardViewModel model)
		{
			// Convert model fields into validatable values
			var customer = HttpContext.GetCustomer();
			var checkoutContext = PersistedCheckoutContextProvider.LoadCheckoutContext(customer);

			var number = !string.IsNullOrEmpty(model.Number) && model.Number.StartsWith("•") && checkoutContext.CreditCard != null
				? checkoutContext.CreditCard.Number
				: model.Number.Replace(" ", "");
			var issueNumber = !string.IsNullOrEmpty(model.IssueNumber) && model.IssueNumber.StartsWith("•") && checkoutContext.CreditCard != null
				? checkoutContext.CreditCard.IssueNumber
				: model.IssueNumber;
			var expirationDate = ParseMonthYearString(model.ExpirationDate);
			var startDate = ParseMonthYearString(model.StartDate);
			var cvv = !string.IsNullOrEmpty(model.Cvv) && model.Cvv.StartsWith("•") && checkoutContext.CreditCard != null
				? checkoutContext.CreditCard.Cvv
				: model.Cvv;

			// Run server-side credit card validation
			var validationConfiguration = new CreditCardValidationConfiguration(
				validateCreditCardNumber: AppLogic.AppConfigBool("ValidateCreditCardNumbers"),
				showCardStartDateFields: AppLogic.AppConfigBool("ShowCardStartDateFields"),
				cardExtraCodeIsOptional: AppLogic.AppConfigBool("CardExtraCodeIsOptional"));

			var validationContext = new CreditCardValidationContext(
				cardType: model.CardType,
				number: number,
				issueNumber: issueNumber,
				expirationDate: expirationDate,
				startDate: startDate,
				cvv: cvv);

			var validationResult = CreditCardValidationProvider.ValidateCreditCard(validationConfiguration, validationContext);

			// Update the ModelState with any validation issues
			if(!validationResult.Valid)
				foreach(var field in validationResult.FieldErrors)
					foreach(var error in field)
						// This assumes that the model properties and the credit card validation field enum names match perfectly
						ModelState.AddModelError(field.Key.ToString(), error);

			// Use POST redirect GET if there are any issues
			if(!ModelState.IsValid)
				return RedirectToAction(ActionNames.CreditCard, ControllerNames.CheckoutCreditCard);

			// Save the validated credit card details into the persisted checkout state
			var updatedCheckoutContext = new PersistedCheckoutContext(
				creditCard: new CreditCardDetails(
					name: model.Name,
					number: number,
					issueNumber: issueNumber,
					cardType: model.CardType,
					expirationDate: expirationDate,
					startDate: startDate,
					cvv: cvv),
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
				selectedShippingMethodId: checkoutContext.SelectedShippingMethodId);

			PersistedCheckoutContextProvider.SaveCheckoutContext(customer, updatedCheckoutContext);

			// Save the StoreCCInDB setting if it was shown to the customer & their choice changed
			var siteIsStoringCCs = AppLogic.AppConfigBool("StoreCCInDB");

            if(siteIsStoringCCs && model.SaveCreditCardNumber != customer.StoreCCInDB)
				customer.UpdateCustomer(storeCreditCardInDb: siteIsStoringCCs && model.SaveCreditCardNumber);

			// Update the customer record
			if(customer.RequestedPaymentMethod != AppLogic.ro_PMCreditCard)
				customer.UpdateCustomer(requestedPaymentMethod: AppLogic.ro_PMCreditCard);

			try
			{
				if(WalletProvider.WalletsAreEnabled() && model.SaveToWallet)
					WalletProvider.CreatePaymentProfile(
						customer: customer,
						billingAddress: customer.PrimaryBillingAddress,
						cardType: model.CardType,
						number: number,
						cvv: cvv,
						expirationDate: expirationDate.Value);
			}
			catch(WalletException walletException)
			{
				ModelState.AddModelError("SaveToWallet", walletException.Message);
				return RedirectToAction(ActionNames.CreditCard, ControllerNames.CheckoutCreditCard);
			}

			return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
		}

		public ActionResult CreditCardDetail()
		{
			var customer = HttpContext.GetCustomer();

			var showCardStartDateFields = AppLogic.AppConfigBool("ShowCardStartDateFields");

			var walletsAreEnabled = customer.IsRegistered && WalletProvider.WalletsAreEnabled();
			var displayWalletCards = walletsAreEnabled && WalletProvider.GetPaymentProfiles(customer).Any();

			string name = customer.FullName(),
				number = null,
				cardType = null,
				expirationDate = null,
				startDate = null;

			var checkoutContext = PersistedCheckoutContextProvider.LoadCheckoutContext(customer);
			if(checkoutContext.CreditCard != null)
			{
				name = checkoutContext.CreditCard.Name ?? customer.FullName();

				number = GetCreditCardNumberForDisplay(checkoutContext.CreditCard.Number);

				cardType = checkoutContext.CreditCard.CardType;

				expirationDate = GetCreditCardDateForDisplay(checkoutContext.CreditCard.ExpirationDate);

				startDate = GetCreditCardDateForDisplay(checkoutContext.CreditCard.StartDate);
			}

			var skinProvider = new SkinProvider();
			return PartialView(ViewNames.CreditCardDetailPartial, new CheckoutCreditCardViewModel
			{
				Name = name,
				Number = number,
				CardType = cardType,
				ExpirationDate = expirationDate,
				StartDate = startDate,
				ShowStartDate = showCardStartDateFields,
				WalletsAreEnabled = walletsAreEnabled,
				DisplayWalletCards = displayWalletCards,
				LastFour = (!string.IsNullOrEmpty(number) && number.Length > 4)
					? number.Substring(number.Length - 4)
					: null,
				CardImage = !string.IsNullOrEmpty(cardType)
					? DisplayTools.GetCardImage(
						imagePath: Url.SkinUrl("images/"),
						cardName: cardType)
					: null
			});
		}

		string GetCreditCardNumberForDisplay(string cardNumber)
		{
			return !string.IsNullOrEmpty(cardNumber)
				? string.Format(
					"•••• •••• •••• {0}",
					cardNumber.Substring(
						Math.Max(0, cardNumber.Length - 4),
						Math.Min(4, cardNumber.Length)))
				: null;
		}

		string GetCreditCardDateForDisplay(DateTime? date)
		{
			return date != null
				? string.Format(
					"{0} / {1}",
					date.Value.Month,
					date.Value.ToString("yy"))
				: null;
		}

		/// <summary>
		/// Converts a mm/yy formatted string to a DateTime?
		/// </summary>
		/// <param name="monthYear">A string formatted as mm/yy or mm/yyyy</param>
		/// <returns>A DateTime if the value could be parsed, or null otherwise</returns>
		DateTime? ParseMonthYearString(string monthYear)
		{
			if(string.IsNullOrEmpty(monthYear))
				return null;

			var splitValues = monthYear
				.Split(
					new[] { '/', '-' },
					StringSplitOptions.RemoveEmptyEntries)
				.Select(value => value.Trim())
				.ToArray();

			if(splitValues.Length != 2)
				return null;

			int month;
			if(!int.TryParse(splitValues[0], out month))
				return null;

			int year;
			if(!int.TryParse(splitValues[1], out year))
				return null;

			// If it's a two digit year, assume it's this century.
			// Probable Y2.1K bug.
			var yearOffset = year < 100
				? 2000
				: 0;

			try
			{
				return new DateTime(yearOffset + year, month, 1);
			}
			catch
			{
				return null;
			}
		}

		CheckoutCreditCardViewModel BuildCheckoutCreditCardViewModel(Customer customer)
		{
			var walletsAreEnabled = customer.IsRegistered && WalletProvider.WalletsAreEnabled();
			var displayWalletCards = walletsAreEnabled && WalletProvider.GetPaymentProfiles(customer).Any();

			var creditCardTypeListItems = CreditCardTypeProvider
				.GetAcceptedCreditCardTypes()
				.Select(creditCardType => new SelectListItem
				{
					Text = creditCardType,
					Value = creditCardType.ToUpper(),
				});
			
			var showIssueNumber = CreditCardTypeProvider
				.GetAcceptedCreditCardTypes()
				.Intersect(
					CreditCardTypeProvider.GetCardTypesRequiringIssueNumber(),
					StringComparer.OrdinalIgnoreCase)
				.Any();


			string name = customer.FullName(),
				number = null,
				cardType = null,
				issueNumber = null,
				expirationDate = null,
				startDate = null,
				cvv = null;

			var checkoutContext = PersistedCheckoutContextProvider.LoadCheckoutContext(customer);
			if(checkoutContext.CreditCard != null)
			{
				name = checkoutContext.CreditCard.Name ?? customer.FullName();

				number = GetCreditCardNumberForDisplay(checkoutContext.CreditCard.Number);

				cardType = checkoutContext.CreditCard.CardType;

				issueNumber = !string.IsNullOrEmpty(checkoutContext.CreditCard.IssueNumber)
					? "••••"
					: null;

				expirationDate = GetCreditCardDateForDisplay(checkoutContext.CreditCard.ExpirationDate);

				startDate = GetCreditCardDateForDisplay(checkoutContext.CreditCard.StartDate);

				cvv = !string.IsNullOrEmpty(checkoutContext.CreditCard.Cvv)
					? "•••"
					: null;
			}

			return new CheckoutCreditCardViewModel
			{
				Name = name,
				Number = number,
				CardType = cardType,
				IssueNumber = issueNumber,
				ExpirationDate = expirationDate,
				StartDate = startDate,
				Cvv = cvv,
				CardTypes = creditCardTypeListItems,
				ShowStartDate = AppLogic.AppConfigBool("ShowCardStartDateFields"),
				ShowIssueNumber = showIssueNumber,
				ShowSaveCreditCardNumber = AppLogic.AppConfigBool("StoreCCInDB"),
				SaveCreditCardNumber = customer.StoreCCInDB,
				ValidateCreditCardNumber = AppLogic.AppConfigBool("ValidateCreditCardNumbers"),
				WalletsAreEnabled = walletsAreEnabled,
				DisplayWalletCards = displayWalletCards,
			};
		}
	}
}
