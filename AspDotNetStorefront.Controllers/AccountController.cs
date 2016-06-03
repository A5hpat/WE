// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Web.Mvc;
using AspDotNetStorefront.Auth;
using AspDotNetStorefront.Classes;
using AspDotNetStorefront.Filters;
using AspDotNetStorefront.Models;
using AspDotNetStorefrontCore;
using AspDotNetStorefront.Routing;
using AspDotNetStorefront.Controllers.Classes;

namespace AspDotNetStorefront.Controllers
{
	[SecureAccessFilter(forceHttps: true)]
	public class AccountController : Controller
	{
		readonly AccountSettings Settings;
		readonly AccountControllerHelper ControllerHelper;
		readonly NoticeProvider NoticeProvider;
		readonly IClaimsIdentityProvider ClaimsIdentityProvider;
		readonly CaptchaStorageService CaptchaStorageService;
		readonly SendWelcomeEmailProvider SendWelcomeEmailProvider;
		readonly AppConfigProvider AppConfigProvider;

		public AccountController(NoticeProvider noticeProvider, 
			IClaimsIdentityProvider claimsIdentityProvider, 
			CaptchaStorageService captchaStorageService,
			SendWelcomeEmailProvider sendWelcomeEmailProvider,
			AppConfigProvider appConfigProvider)
		{
			Settings = new AccountSettings();
			ControllerHelper = new AccountControllerHelper(Settings);
			NoticeProvider = noticeProvider;
			ClaimsIdentityProvider = claimsIdentityProvider;
			CaptchaStorageService = captchaStorageService;
			SendWelcomeEmailProvider = sendWelcomeEmailProvider;
			AppConfigProvider = appConfigProvider;
		}

		[HttpGet]
		[RequireCustomerRegistrationFilter(RequiredRegistrationStatus.Registered, ControllerNames.Account, ActionNames.SignIn)]
		[ImportModelStateFromTempData]
		public ActionResult Index()
		{
			var customer = HttpContext.GetCustomer();

			var account = new AccountViewModel
			{
				FirstName = customer.FirstName,
				LastName = customer.LastName,
				Email = customer.EMail,
				EmailConfirmation = customer.EMail,
				Phone = customer.Phone,
				IsOkToEmail = customer.OKToEMail,
				IsOver13 = customer.IsOver13,
				VatRegistrationId = customer.VATRegistrationID,
				SaveCreditCardNumber = customer.StoreCCInDB
			};

			var model = ControllerHelper.BuildAccountIndexViewModel(account, customer, Url);

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[RequireCustomerRegistrationFilter(RequiredRegistrationStatus.Registered, ControllerNames.Account, ActionNames.SignIn)]
		[ExportModelStateToTempData]
		public ActionResult Index(AccountPostViewModel model)
		{
			var customer = HttpContext.GetCustomer();

			if(!ModelState.IsValid)
				return RedirectToAction(ActionNames.Index);

			if(!Customer.NewEmailPassesDuplicationRules(model.Account.Email, customer.CustomerID))
			{
				ModelState.AddModelError("Account.Email", "That EMail Address is Already Used By Another Customer");
				return RedirectToAction(ActionNames.Index);
			}

			// The account editor only updates the password if one was specified or if the customer has not yet registered.
			if(!customer.IsRegistered || !string.IsNullOrEmpty(model.Account.Password))
			{
				switch(ControllerHelper.ValidateAccountPassword(customer, model.Account.Password, model.Account.PasswordConfirmation))
				{
					case AccountControllerHelper.PasswordValidationResult.DoesNotMatch:
						ModelState.AddModelError("Account.PasswordConfirmation", "The new passwords do not match!");
						return RedirectToAction(ActionNames.Index);

					case AccountControllerHelper.PasswordValidationResult.NotStrong:
						ModelState.AddModelError("Account.Password", "The new password you created is not a strong password. Please make sure that your password is at least 8 characters long and includes at least one upper case character, one lower case character, one number, and one \"symbol\" character (e.g. ?,&,#,$,%,etc).");
						return RedirectToAction(ActionNames.Index);

					case AccountControllerHelper.PasswordValidationResult.SameAsCurrent:
						ModelState.AddModelError("Account.Password", "The new password cannot be the same as the old password.");
						return RedirectToAction(ActionNames.Index);

					case AccountControllerHelper.PasswordValidationResult.SameAsPrevious:
						ModelState.AddModelError("Account.Password", string.Format("The new password has been previously used.  Please select a password that has not been used in {0} previous uses.", Settings.NumberOfPreviouslyUsedPasswords));
						return RedirectToAction(ActionNames.Index);

					default:
					case AccountControllerHelper.PasswordValidationResult.Ok:
						break;
				}
			}

			var vatRegistationValidationResult = ControllerHelper.ValidateVatRegistrationId(model.Account, customer);
			if(!vatRegistationValidationResult.Ok)
			{
				NoticeProvider.PushNotice(
					AppLogic.GetString(
						vatRegistationValidationResult.Message
						?? "account.aspx.91"),
					NoticeType.Failure);

				return RedirectToAction(ActionNames.Index);
			}

			ControllerHelper.UpdateAccount(model.Account, customer);
			NoticeProvider.PushNotice("Your account has been updated.", NoticeType.Success);
			return RedirectToAction(ActionNames.Index);
		}

		[HttpGet]
		[ImportModelStateFromTempData]
		public ActionResult Create()
		{
			var customer = HttpContext.GetCustomer();

			// We will allow registered customers to create new accounts if they end up on the page but we won't
			// prepopulate and fields so its clear they're creating a new account. Otherwise, we'll try and fill in
			// whatever fields we might have fromt he current customer record.
			var account = !customer.IsRegistered
				? new AccountCreateViewModel
				{
					FirstName = customer.FirstName,
					LastName = customer.LastName,
					Email = customer.EMail,
					Phone = customer.Phone,
					IsOkToEmail = customer.OKToEMail,
					IsOver13 = customer.IsOver13,
					VatRegistrationId = customer.VATRegistrationID,
					SaveCreditCardNumber = customer.StoreCCInDB
				}
				: new AccountCreateViewModel();

			return View(new AccountCreateIndexViewModel(
				displayCaptcha: Settings.RequireCaptchaOnCreateAccount,
				requireEmailConfirmation: AppConfigProvider.GetAppConfigValue<bool>("RequireEmailConfirmation"),
				displayOver13Selector: AppConfigProvider.GetAppConfigValue<bool>("RequireOver13Checked"))
			{
				Account = account,
				PrimaryBillingAddress = new AccountAddressViewModel(),
				PrimaryShippingAddress = new AccountAddressViewModel()
			});
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[ExportModelStateToTempData]
		public ActionResult Create(AccountCreatePostModel model)
		{
			var customer = HttpContext.GetCustomer();

			if(!ModelState.IsValid)
				return RedirectToAction(ActionNames.Create);

			if(!Customer.NewEmailPassesDuplicationRules(model.Account.Email, customer.CustomerID))
			{
				ModelState.AddModelError(
					key: "Account.Email",
					errorMessage: "That EMail Address is Already Used By Another Customer");
				return RedirectToAction(ActionNames.Create);
			}

			switch(ControllerHelper.ValidateAccountPassword(customer, model.Account.Password, model.Account.PasswordConfirmation))
			{
				case AccountControllerHelper.PasswordValidationResult.DoesNotMatch:
					ModelState.AddModelError(
						key: "Account.PasswordConfirmation",
						errorMessage: "The new passwords do not match!");
					return RedirectToAction(ActionNames.Create);

				case AccountControllerHelper.PasswordValidationResult.DoesNotMeetMinimum:
					ModelState.AddModelError(
						key: "Account.Password",
						errorMessage: "The new password you created does not meet the minimum requirements. Please make sure that your password is at least 7 characters long and includes at least one letter and at least one number.");
					return RedirectToAction(ActionNames.Create);

				case AccountControllerHelper.PasswordValidationResult.NotStrong:
					ModelState.AddModelError(
						key: "Account.Password",
						errorMessage: "The new password you created is not a strong password. Please make sure that your password is at least 8 characters long and includes at least one upper case character, one lower case character, one number, and one \"symbol\" character (e.g. ?,&,#,$,%,etc).");

                    return RedirectToAction(ActionNames.Create);
			}

			if(AppConfigProvider.GetAppConfigValue<bool>("RequireOver13Checked") && !model.Account.IsOver13)
			{
				ModelState.AddModelError(
					key: "Account.IsOver13",
					errorMessage: "You Must Be Over 18 To Purchase or have Parental Consent");
				return RedirectToAction(ActionNames.Create);
			}

			if(Settings.RequireCaptchaOnCreateAccount)
			{
				var securityCode = CaptchaStorageService.RetrieveSecurityCode(HttpContext, string.Concat(ControllerNames.Account, ActionNames.Create));
				if(!ControllerHelper.IsCaptchaValid(securityCode, model.Account.CaptchaCode))
				{
					CaptchaStorageService.ClearSecurityCode(HttpContext);
					ModelState.AddModelError(
						key: "Account.CaptchaCode",
						errorMessage: "The letters you entered did not match, please try again.");

					return RedirectToAction(ActionNames.Create);
				}
			}

			var registeredCustomer = ControllerHelper.CreateAccount(model.Account, customer);

			ControllerHelper.Login(
				signedInCustomer: registeredCustomer,
				profile: HttpContext.Profile,
				username: model.Account.Email,
				password: model.Account.Password,
				skinId: registeredCustomer.SkinID,
				registering: true);

			Request
				.GetOwinContext()
				.Authentication
				.SignOut();

			Request
				.GetOwinContext()
				.Authentication
				.SignIn(
					properties: new Microsoft.Owin.Security.AuthenticationProperties
					{
						IsPersistent = true
					},
					identities: ClaimsIdentityProvider
						.CreateClaimsIdentity(registeredCustomer));

			// Clear the captcha so additional requests use a different security code.
			CaptchaStorageService.ClearSecurityCode(HttpContext);

			if(AppConfigProvider.GetAppConfigValue<bool>("SendWelcomeEmail"))
				SendWelcomeEmailProvider.SendWelcomeEmail(registeredCustomer);

			NoticeProvider.PushNotice("You have successfully created a new account", NoticeType.Success);
			return RedirectToAction(ActionNames.Index);
		}

		[AllowInMaintenanceMode]
		[PageTypeFilter(PageTypes.Signin)]
		public ActionResult SignIn(int? errorMessage = null, string returnUrl = null)
		{
			var queryStringErrorMessage = ControllerHelper.GetQueryStringErrorMessage(errorMessage);
			if(!String.IsNullOrEmpty(queryStringErrorMessage))
				NoticeProvider.PushNotice(queryStringErrorMessage, NoticeType.Failure);

			return View(new AccountSignInViewModel(
				returnUrl: returnUrl,
				displayCaptcha: Settings.RequireCaptchaOnLogin,
				passwordResetAvailable: ControllerHelper.IsPasswordResetAvailable()));
		}

		[HttpPost]
		[AllowInMaintenanceMode]
		[PageTypeFilter(PageTypes.Signin)]
		public ActionResult SignIn(AccountSignInViewModel model, string returnUrl = null, int? errorMessage = null)
		{
			if(!ModelState.IsValid)
			{
				return View(new AccountSignInViewModel(
					source: model,
					captchaCode: string.Empty,
					returnUrl: returnUrl,
					displayCaptcha: Settings.RequireCaptchaOnLogin,
					passwordResetAvailable: ControllerHelper.IsPasswordResetAvailable()));
			}

			var signedInCustomer = HttpContext.GetCustomer();

			if(Settings.RequireCaptchaOnLogin
				&& !ControllerHelper.IsCaptchaValid(
					requiredSecurityCode: CaptchaStorageService.RetrieveSecurityCode(HttpContext, string.Concat(ControllerNames.Account, ActionNames.SignIn)),
					securityCode: model.CaptchaCode))
			{
				CaptchaStorageService.ClearSecurityCode(HttpContext);

				ModelState.AddModelError(
					key: "CaptchaCode",
					errorMessage: "The letters you entered did not match, please try again.");


                return View(new AccountSignInViewModel(
					source: model,
					captchaCode: string.Empty,
					returnUrl: returnUrl,
					displayCaptcha: Settings.RequireCaptchaOnLogin,
					passwordResetAvailable: ControllerHelper.IsPasswordResetAvailable()));
			}

			// Login
			var result = ControllerHelper.Login(
				signedInCustomer: signedInCustomer,
				profile: HttpContext.Profile,
				username: model.Email,
				password: model.Password,
				skinId: signedInCustomer.SkinID);

			if(result.State == AccountControllerHelper.ResultState.Error)
			{
				NoticeProvider.PushNotice(result.Message, NoticeType.Failure);

				return View(new AccountSignInViewModel(
					source: model,
					captchaCode: string.Empty,
					returnUrl: returnUrl,
					displayCaptcha: Settings.RequireCaptchaOnLogin,
					passwordResetAvailable: ControllerHelper.IsPasswordResetAvailable()));
			}
			else if(result.State == AccountControllerHelper.ResultState.PasswordChangeRequired)
			{
				NoticeProvider.PushNotice(result.Message, NoticeType.Info);
				return RedirectToAction(
					actionName: ActionNames.ChangePassword,
					routeValues: new
					{
						email = model.Email,
						returnUrl = returnUrl
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
						IsPersistent = model.PersistLogin
					},
					identities: identity);

			if(!String.IsNullOrEmpty(result.Message))
				NoticeProvider.PushNotice(result.Message, NoticeType.Info);

			// Clear the captcha so additional requests use a different security code.
			CaptchaStorageService.ClearSecurityCode(HttpContext);

			var safeReturnUrl = Url.MakeSafeReturnUrl(returnUrl);

			return Redirect(safeReturnUrl);
		}

		public ActionResult ChangePassword(string email = null, string returnUrl = null, int? errorMessage = null)
		{
			var queryStringErrorMessage = ControllerHelper.GetQueryStringErrorMessage(errorMessage);
			if(!String.IsNullOrEmpty(queryStringErrorMessage))
				NoticeProvider.PushNotice(queryStringErrorMessage, NoticeType.Failure);

			return View(new AccountChangePasswordViewModel(
				returnUrl: returnUrl,
				passwordResetAvailable: ControllerHelper.IsPasswordResetAvailable())
			{
				Email = email
			});
		}

		[HttpPost, ValidateAntiForgeryToken]
		public ActionResult ChangePassword(AccountChangePasswordViewModel model, string returnUrl = null)
		{
			if(!ModelState.IsValid)
				return View(new AccountChangePasswordViewModel(
					source: model,
					returnUrl: returnUrl,
					passwordResetAvailable: ControllerHelper.IsPasswordResetAvailable()));

			var signedInCustomer = HttpContext.GetCustomer();

			var result = ControllerHelper.ChangePassword(
				signedInCustomer: signedInCustomer,
				username: model.Email,
				oldPassword: model.OldPassword,
				newPassword: model.NewPassword,
				newPasswordConfirmation: model.NewPassword,
				skinId: signedInCustomer.SkinID);

			if(result.State == AccountControllerHelper.ResultState.Error)
			{
				NoticeProvider.PushNotice(result.Message, NoticeType.Failure);
				return View(new AccountChangePasswordViewModel(
					source: model,
					returnUrl: returnUrl,
					passwordResetAvailable: ControllerHelper.IsPasswordResetAvailable()));
			}

			var targetCustomer = new Customer(model.Email);
			var identity = ClaimsIdentityProvider.CreateClaimsIdentity(targetCustomer);

			Request
				.GetOwinContext()
				.Authentication
				.SignIn(identity);

			if(!string.IsNullOrEmpty(result.Message))
				NoticeProvider.PushNotice(result.Message, NoticeType.Info);

			var safeReturnUrl = Url.MakeSafeReturnUrl(returnUrl);

			return Redirect(safeReturnUrl);
		}

		public ActionResult SignOut()
		{
			var signedInCustomer = HttpContext.GetCustomer();
			if(signedInCustomer.IsAdminUser)
				Security.LogEvent("Store Logout Success", "", signedInCustomer.CustomerID, signedInCustomer.CustomerID, signedInCustomer.CurrentSessionID);

			signedInCustomer.Logout();

			Request
				.GetOwinContext()
				.Authentication
				.SignOut(AuthValues.CookiesAuthenticationType);

			return RedirectToAction(ActionNames.Index, ControllerNames.Home);
		}

		[HttpPost, ValidateAntiForgeryToken]
		public ActionResult ResetPassword(AccountResetPasswordViewModel model, string returnUrl = null)
		{
			if(!ModelState.IsValid)
				return View(
					viewName: "signIn",
					model: new AccountSignInViewModel(
						returnUrl: returnUrl,
						displayCaptcha: Settings.RequireCaptchaOnLogin,
						passwordResetAvailable: ControllerHelper.IsPasswordResetAvailable()));

			var signedInCustomer = HttpContext.GetCustomer();

			var result = ControllerHelper.RequestNewPassword(
				signedInCustomer: signedInCustomer,
				email: model.Email,
				skinId: signedInCustomer.SkinID);

			if(result.State == AccountControllerHelper.ResultState.Error)
				NoticeProvider.PushNotice(result.Message, NoticeType.Failure);
			else
				NoticeProvider.PushNotice(result.Message, NoticeType.Success);

			return View(
				viewName: "signIn",
				model: new AccountSignInViewModel(
					returnUrl: returnUrl,
						displayCaptcha: Settings.RequireCaptchaOnLogin,
						passwordResetAvailable: ControllerHelper.IsPasswordResetAvailable()));
		}

		[Authorize]
		public ActionResult Reorder(int orderId)
		{
			var customer = HttpContext.GetCustomer();

			var order = new Order(orderId);
			if(order == null)
				return HttpNotFound();

			if(!customer.IsAdminUser && customer.CustomerID != order.CustomerID)
				return HttpNotFound();

			string result;
			if(!Order.BuildReOrder(null, customer, order.OrderNumber, out result))
			{
				NoticeProvider.PushNotice(result, NoticeType.Failure);
				return RedirectToAction(ActionNames.Index);
			}

			return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
		}
	}
}
