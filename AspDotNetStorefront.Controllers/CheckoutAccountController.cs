// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Web;
using System.Web.Mvc;
using AspDotNetStorefront.Auth;
using AspDotNetStorefront.Checkout;
using AspDotNetStorefront.Classes;
using AspDotNetStorefront.Filters;
using AspDotNetStorefront.Models;
using AspDotNetStorefrontCore;
using AspDotNetStorefront.Routing;
using AspDotNetStorefront.Controllers.Classes;

namespace AspDotNetStorefront.Controllers
{
	[SecureAccessFilter(forceHttps: true)]
	public class CheckoutAccountController : Controller
	{
		readonly AccountControllerHelper AccountControllerHelper;
		readonly CaptchaStorageService CaptchaStorageService;
		readonly ICheckoutAccountStatusProvider CheckoutAccountStatusProvider;
		readonly IClaimsIdentityProvider ClaimsIdentityProvider;
		readonly NoticeProvider NoticeProvider;
		readonly IPersistedCheckoutContextProvider PersistedCheckoutContextProvider;
		readonly SendWelcomeEmailProvider SendWelcomeEmailProvider;

		public CheckoutAccountController(
			AccountControllerHelper accountControllerHelper,
			CaptchaStorageService captchaStorageService,
			ICheckoutAccountStatusProvider checkoutAccountStatusProvider,
			IClaimsIdentityProvider claimsIdentityProvider,
			NoticeProvider noticeProvider,
			IPersistedCheckoutContextProvider persistedCheckoutContextProvider,
			SendWelcomeEmailProvider sendWelcomeEmailProvider)
		{
			AccountControllerHelper = accountControllerHelper;
			CaptchaStorageService = captchaStorageService;
			CheckoutAccountStatusProvider = checkoutAccountStatusProvider;
			ClaimsIdentityProvider = claimsIdentityProvider;
			NoticeProvider = noticeProvider;
			PersistedCheckoutContextProvider = persistedCheckoutContextProvider;
			SendWelcomeEmailProvider = sendWelcomeEmailProvider;
		}

		[HttpGet, ImportModelStateFromTempData]
		public ActionResult Account()
		{
			var customer = HttpContext.GetCustomer();
			var checkoutContext = PersistedCheckoutContextProvider.LoadCheckoutContext(customer);

			var email = customer.IsRegistered
				? customer.EMail
				: checkoutContext.Email ?? string.Empty;

			var checkoutAccountStatus = CheckoutAccountStatusProvider.GetCheckoutAccountStatus(customer, email);

			var model = new CheckoutAccountViewModel(
				passwordRequired: checkoutAccountStatus.RequireRegisteredCustomer,
				showCaptcha: AppLogic.AppConfigBool("SecurityCodeRequiredOnCheckout"),
				passwordResetAvailable: !string.IsNullOrEmpty(AppLogic.MailServer()) && AppLogic.MailServer() != AppLogic.ro_TBD)
			{ Email = checkoutAccountStatus.Email };

			if(checkoutAccountStatus.State == CheckoutAccountState.Registered)
				return PartialView(ViewNames.AccountRegisteredPartial, model);

			switch(checkoutAccountStatus.NextAction)
			{
				case CheckoutAccountAction.CanLogin:
					return PartialView(ViewNames.AccountLoginPartial, model);

				case CheckoutAccountAction.CanCreateAccount:
					return PartialView(ViewNames.AccountCreateAccountPartial, model);

				default:
					// In all other cases, the user is allowed to change their email at-will with no password prompt
					return PartialView(ViewNames.AccountCollectEmailPartial, model);
			}
		}

		[HttpPost, ExportModelStateToTempData]
		public ActionResult SetEmail(CheckoutAccountPostModel model)
		{
			var customer = HttpContext.GetCustomer();

			// Don't set the email if they are logged in.
			if(customer.IsRegistered)
				return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);

			var result = ValidateEmail(model.Email);
			if(result.State == ResultState.Error)
				ModelState.AddModelError("Email", "Please enter a valid email address");


            SaveEmail(model.Email, customer);

			return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
		}

		[HttpPost, ExportModelStateToTempData, ValidateAntiForgeryToken]
		public ActionResult SignIn(CheckoutAccountPostModel model)
		{
			var signedInCustomer = HttpContext.GetCustomer();

			// Don't let them sign in if they are logged in.
			if(signedInCustomer.IsRegistered)
				return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);

			if(string.IsNullOrWhiteSpace(model.Password))
			{
				ModelState.AddModelError("Password", "Password is required to login");
				return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
			}

			var showCaptchaOnLogin = AppLogic.AppConfigBool("SecurityCodeRequiredOnCheckout");
			if(showCaptchaOnLogin)
			{
				var securityCode = CaptchaStorageService.RetrieveSecurityCode(HttpContext, string.Concat(ControllerNames.Account, ActionNames.SignIn));
				if(!AccountControllerHelper.IsCaptchaValid(securityCode, model.Captcha))
				{
					CaptchaStorageService.ClearSecurityCode(HttpContext);

					ModelState.AddModelError(
						key: "Captcha",
						errorMessage: "The letters you entered did not match, please try again.");

					return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
				}
			}

			// Validate the email
			var emailResult = ValidateEmail(model.Email);
			if(emailResult.State == ResultState.Error)
			{
				ModelState.AddModelError("Email", emailResult.Message);
				return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
			}

			// Login
			var result = AccountControllerHelper.Login(
				signedInCustomer: signedInCustomer,
				profile: HttpContext.Profile,
				username: model.Email,
				password: model.Password,
				skinId: signedInCustomer.SkinID);

			if(result.State == AccountControllerHelper.ResultState.Error)
			{
				ModelState.AddModelError("Password", result.Message);
				return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
			}
			else if(result.State == AccountControllerHelper.ResultState.PasswordChangeRequired)
			{
				NoticeProvider.PushNotice(result.Message, NoticeType.Info);
				return RedirectToAction(
					actionName: ActionNames.ChangePassword,
					controllerName: ControllerNames.Account,
					routeValues: new
					{
						email = model.Email,
						returnUrl = Url.Action(ActionNames.Index, ControllerNames.Checkout),
					});
			}

			var targetCustomer = new Customer(model.Email);
			var identity = ClaimsIdentityProvider.CreateClaimsIdentity(targetCustomer);

			Request
				.GetOwinContext()
				.Authentication
				.SignIn(
					properties: new Microsoft.Owin.Security.AuthenticationProperties
					{
						IsPersistent = false
					},
					identities: identity);

			if(!string.IsNullOrEmpty(result.Message))
				NoticeProvider.PushNotice(result.Message, NoticeType.Info);

			// Clear the captcha so additional requests use a different security code.
			CaptchaStorageService.ClearSecurityCode(HttpContext);

			return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
		}

		[HttpPost, ExportModelStateToTempData, ValidateAntiForgeryToken]
		public ActionResult CreateAccount(CheckoutAccountPostModel model)
		{
			var customer = HttpContext.GetCustomer();

			// Don't create an account if they are logged in.
			if(customer.IsRegistered)
				return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);

			var showCaptchaOnCreateAccount = AppLogic.AppConfigBool("SecurityCodeRequiredOnCreateAccount");
			if(showCaptchaOnCreateAccount)
			{
				var securityCode = CaptchaStorageService.RetrieveSecurityCode(HttpContext, string.Concat(ControllerNames.Account, ActionNames.Create));
				if(!AccountControllerHelper.IsCaptchaValid(securityCode, model.Captcha))
				{
					CaptchaStorageService.ClearSecurityCode(HttpContext);

					ModelState.AddModelError(
						key: "Captcha",
						errorMessage: "The letters you entered did not match, please try again.");

					return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
				}
			}

			// Validate the email
			var emailResult = ValidateEmail(model.Email);
			if(emailResult.State == ResultState.Error)
			{
				ModelState.AddModelError("Email", emailResult.Message);
				return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
			}

			if(!Customer.NewEmailPassesDuplicationRules(model.Email, customer.CustomerID))
			{
				ModelState.AddModelError(
					key: "Email",
					errorMessage: "That EMail Address is Already Used By Another Customer");
				return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
			}

			// Validate the password
			if(string.IsNullOrEmpty(model.Password))
			{
				ModelState.AddModelError("Password", "Please enter a password");
				return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
			}

			switch(AccountControllerHelper.ValidateAccountPassword(customer, model.Password, model.Password)) //Intentionally passing in matching passwords
			{
				case AccountControllerHelper.PasswordValidationResult.NotStrong:
					ModelState.AddModelError("Password", "The new password you created is not a strong password. Please make sure that your password is at least 8 characters long and includes at least one upper case character, one lower case character, one number, and one \"symbol\" character (e.g. ?,&,#,$,%,etc).");
					return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);

				case AccountControllerHelper.PasswordValidationResult.DoesNotMeetMinimum:
					ModelState.AddModelError("Password", "The new password you created does not meet the minimum requirements. Please make sure that your password is at least 7 characters long and includes at least one letter and at least one number.");
					return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);

				case AccountControllerHelper.PasswordValidationResult.Ok:
					break;

				default:
					ModelState.AddModelError("Password", "There was a problem signing in. Please try again or contact customer service.");
					return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
			}

			// Create the account
			var password = new Password(model.Password);
			customer.UpdateCustomer(
				isRegistered: true,
				email: model.Email.ToLowerInvariant().Trim(),
				saltedAndHashedPassword: password.SaltedPassword,
				saltKey: password.Salt,
				storeCreditCardInDb: false
			);

			// Login
			var result = AccountControllerHelper.Login(
				signedInCustomer: customer,
				profile: HttpContext.Profile,
				username: model.Email,
				password: model.Password,
				skinId: customer.SkinID);

			switch(result.State)
			{
				case AccountControllerHelper.ResultState.Error:
					NoticeProvider.PushNotice(result.Message, NoticeType.Failure);
					return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);

				case AccountControllerHelper.ResultState.PasswordChangeRequired:
					NoticeProvider.PushNotice(result.Message, NoticeType.Info);
					return RedirectToAction(
						actionName: ActionNames.ChangePassword,
						controllerName: ControllerNames.Account,
						routeValues: new
						{
							email = model.Email,
							returnUrl = Url.Action(ActionNames.Index, ControllerNames.Checkout),
						});

				case AccountControllerHelper.ResultState.Success:
					break;

				default:
					ModelState.AddModelError("Password", "There was a problem signing in. Please try again or contact customer service.");
					return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
			}

			var targetCustomer = new Customer(model.Email);
			var identity = ClaimsIdentityProvider.CreateClaimsIdentity(targetCustomer);

			Request
				.GetOwinContext()
				.Authentication
				.SignIn(
					properties: new Microsoft.Owin.Security.AuthenticationProperties
					{
						IsPersistent = false
					},
					identities: identity);

			if(!string.IsNullOrEmpty(result.Message))
				NoticeProvider.PushNotice(result.Message, NoticeType.Info);
            // nal
			//if(AppLogic.AppConfigBool("SendWelcomeEmail"))
				SendWelcomeEmailProvider.SendWelcomeEmail(targetCustomer);

			// Clear the captcha so additional requests use a different security code.
			CaptchaStorageService.ClearSecurityCode(HttpContext);

			return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
		}

		Result ValidateEmail(string email)
		{
			if(!string.IsNullOrEmpty(email))
				email = email
					.ToLowerInvariant()
					.Trim();

			var emailValidator = new AspDotNetStorefrontCore.Validation.EmailAddressValidator();

			if(string.IsNullOrEmpty(email) || !emailValidator.IsValidEmailAddress(email))
			{
				return new Result(
					state: ResultState.Error,
					message: "Please enter a valid email address");
			}

			return new Result(
				state: ResultState.Success);
		}

		void SaveEmail(string email, Customer customer)
		{
			var checkoutContext = PersistedCheckoutContextProvider.LoadCheckoutContext(customer);
			PersistedCheckoutContextProvider.SaveCheckoutContext(customer, new PersistedCheckoutContext(
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
				email: email,
				selectedShippingMethodId: checkoutContext.SelectedShippingMethodId));

			var allowGuestCheckoutForEmail = Customer.NewEmailPassesDuplicationRules(
				email: email,
				customerId: customer.CustomerID);

			if(!allowGuestCheckoutForEmail)
				return;

			//At this point we have a valid guest email address so lets update the guest account with the email.
			customer.UpdateCustomer(
				email: email
			);

			return;
		}

		public class Result
		{
			public readonly ResultState State;
			public readonly string Message;

			public Result(ResultState state, string message = null)
			{
				State = state;
				Message = message;
			}
		}

		public enum ResultState
		{
			Success,
			Error,
		}
	}
}
