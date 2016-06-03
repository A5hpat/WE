// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AspDotNetStorefront.Validation.DataAttribute;
using AspDotNetStorefrontCore.Validation;

namespace AspDotNetStorefront.Models
{
	public class AccountSignInViewModel
	{
		[HtmlAutocompleteType(HtmlAutocomplete.Email)]
		[Display(Name = "signin.emailaddress.label", Prompt = "signin.emailaddress.example")]
		[Required(ErrorMessage = "signin.emailaddress.required")]
		[RegularExpression(EmailAddressValidator.ValidationRegularExpression, ErrorMessage = "signin.emailaddress.format")]
		[DataType(DataType.EmailAddress, ErrorMessage = "signin.emailaddress.format")]
		[StringLength(100, ErrorMessage = "signin.emailaddress.length")]
		public string Email
		{ get; set; }

		[HtmlAutocompleteType(HtmlAutocomplete.CurrentPassword)]
		[Display(Name = "signin.password.label", Prompt = "signin.password.example")]
		[Required(ErrorMessage = "signin.password.required")]
		[DataType(DataType.Password)]
		[StringLength(50, ErrorMessage = "signin.password.length")]
		public string Password
		{ get; set; }

		[Display(Name = "signin.captcha.label")]
		[RequiredIfAppConfigTrue(appConfigName: "SecurityCodeRequiredOnStoreLogin", ErrorMessage = "account.captcha.required")]
		public string CaptchaCode
		{ get; set; }

		[Display(Name = "signin.rememberpassword.label")]
		[WhatsThis("account.rememberpassword.whats-this.content")]
		public bool PersistLogin
		{ get; set; }

		public readonly string ReturnUrl;
		public readonly bool DisplayCaptcha;
		public readonly bool PasswordResetAvailable;

		public AccountSignInViewModel()
		{ }

		public AccountSignInViewModel(string returnUrl, bool displayCaptcha, bool passwordResetAvailable)
		{
			ReturnUrl = returnUrl;
			DisplayCaptcha = displayCaptcha;
			PasswordResetAvailable = passwordResetAvailable;
		}

		public AccountSignInViewModel(AccountSignInViewModel source, string email = null, string password = null, string captchaCode = null, bool? persistLogin = null, string returnUrl = null, bool? displayCaptcha = null, bool? passwordResetAvailable = null)
		{
			Email = email ?? source.Email;
			Password = password ?? source.Password;
			CaptchaCode = captchaCode ?? source.CaptchaCode;
			PersistLogin = persistLogin ?? source.PersistLogin;
			ReturnUrl = returnUrl ?? source.ReturnUrl;
			DisplayCaptcha = displayCaptcha ?? source.DisplayCaptcha;
			PasswordResetAvailable = passwordResetAvailable ?? source.PasswordResetAvailable;
		}
	}

	public class AccountResetPasswordViewModel
	{
		[Display(Name = "signin.emailaddress.label", Prompt = "signin.emailaddress.example")]
		[Required(ErrorMessage = "signin.emailaddress.required")]
		[RegularExpression(EmailAddressValidator.ValidationRegularExpression, ErrorMessage = "signin.emailaddress.format")]
		[DataType(DataType.EmailAddress, ErrorMessage = "signin.emailaddress.format")]
		[StringLength(100, ErrorMessage = "signin.emailaddress.length")]
		public string Email
		{ get; set; }

		public readonly string ReturnUrl;

		public AccountResetPasswordViewModel()
		{ }

		public AccountResetPasswordViewModel(string returnUrl)
		{
			ReturnUrl = returnUrl;
		}

		public AccountResetPasswordViewModel(AccountResetPasswordViewModel source, string returnUrl = null)
		{
			Email = source.Email;
			ReturnUrl = returnUrl ?? source.ReturnUrl;
		}
	}

	public class AccountChangePasswordViewModel
	{
		[HtmlAutocompleteType(HtmlAutocomplete.Email)]
		[Display(Name = "signin.emailaddress.label", Prompt = "signin.emailaddress.example")]
		[Required(ErrorMessage = "signin.emailaddress.required")]
		[RegularExpression(EmailAddressValidator.ValidationRegularExpression, ErrorMessage = "signin.emailaddress.format")]
		[DataType(DataType.EmailAddress, ErrorMessage = "signin.emailaddress.format")]
		[StringLength(100, ErrorMessage = "signin.emailaddress.length")]
		public string Email
		{ get; set; }

		[HtmlAutocompleteType(HtmlAutocomplete.CurrentPassword)]
		[Display(Name = "signin.oldpassword.label", Prompt = "signin.password.example")]
		[Required(ErrorMessage = "signin.password.required")]
		[DataType(DataType.Password)]
		[StringLength(50, ErrorMessage = "signin.password.length")]
		public string OldPassword
		{ get; set; }

		[HtmlAutocompleteType(HtmlAutocomplete.NewPassword)]
		[Display(Name = "signin.newpassword.label", Prompt = "signin.newpassword.example")]
		[Required(ErrorMessage = "signin.password.required")]
		[DataType(DataType.Password)]
		[StringLength(50, ErrorMessage = "signin.password.length")]
		[StrongPasswordDisplay(weakDescription: "account.aspx.19", strongDescription: "account.strongPassword", emailPropertyName:"Email")]
		[PasswordRegularExpression(normalPasswordErrorMessage: "signin.newpassword.normalRegexFailure", strongPasswordErrorMessage: "signin.newpassword.strongRegexFailure", emailPropertyName: "Email", disableClientValidation: true)]
		public string NewPassword
		{ get; set; }

		[HtmlAutocompleteType(HtmlAutocomplete.Off)]
		[Display(Name = "signin.confirmpassword.label", Prompt = "signin.confirmpassword.example")]
		[Required(ErrorMessage = "signin.password.required")]
		[DataType(DataType.Password)]
		[StringLength(50, ErrorMessage = "signin.password.length")]
		[Compare("NewPassword", ErrorMessage = "signin.newpassword.mismatch")]
		public string NewPasswordConfirmation
		{ get; set; }

		public readonly string ReturnUrl;
		public readonly bool PasswordResetAvailable;

		public AccountChangePasswordViewModel()
		{ }

		public AccountChangePasswordViewModel(string returnUrl, bool passwordResetAvailable)
		{
			ReturnUrl = returnUrl;
			PasswordResetAvailable = passwordResetAvailable;
		}

		public AccountChangePasswordViewModel(AccountChangePasswordViewModel source, string returnUrl = null, bool? passwordResetAvailable = null)
		{
			Email = source.Email;
			OldPassword = source.OldPassword;
			NewPassword = source.NewPassword;
			NewPasswordConfirmation = source.NewPasswordConfirmation;
			ReturnUrl = returnUrl ?? source.ReturnUrl;
			PasswordResetAvailable = passwordResetAvailable ?? source.PasswordResetAvailable;
		}
	}

	public class AccountIndexViewModel
	{
		public readonly AccountAddressViewModel PrimaryBillingAddress;
		public readonly AccountAddressViewModel PrimaryShippingAddress;
		public readonly IEnumerable<AccountOrderViewModel> Orders;
		public readonly string PaymentMethodLastUsed;
		public readonly bool ShowWishListButton;
		public readonly bool ShowVatRegistrationId;
		public readonly bool ShowSaveCreditCardNumber;
		public readonly string SaveCreditCardNumberNote;
		public readonly bool ShowRecurringOrders;
		public readonly bool ShowWallets;
		public readonly string CustomerLevel;
		public readonly bool HasMicropayBalance;
		public readonly bool ShowMicropayLink;
		public readonly string MicropayLink;
		public readonly decimal MicropayBalance;
		public readonly string LocaleSetting;
		public readonly string CurrencySetting;
		public readonly bool RequireEmailConfirmation;
		public readonly bool DisplayOver13Selector;

		public AccountViewModel Account
		{ get; set; }

		public string CaptchaError
		{ get; set; }

		public AccountIndexViewModel(
			AccountViewModel account,
			AccountAddressViewModel primaryBillingAddress,
			AccountAddressViewModel primaryShippingAddress,
			IEnumerable<AccountOrderViewModel> orders,
			string paymentMethodLastUsed,
			bool showWishListButton,
			bool showVatRegistrationId,
			bool showSaveCreditCardNumber,
			string saveCreditCardNumberNote,
			bool showRecurringOrders,
			bool showWallets,
			string customerLevel,
			bool hasMicropayBalance,
			bool showMicropayLink,
			string micropayLink,
			decimal micropayBalance,
			string localeSetting,
			string currencySetting,
			bool requireEmailConfirmation,
			bool displayOver13Selector)
		{
			Account = account;
			PrimaryBillingAddress = primaryBillingAddress;
			PrimaryShippingAddress = primaryShippingAddress;
			Orders = orders;
			PaymentMethodLastUsed = paymentMethodLastUsed;
			ShowWishListButton = showWishListButton;
			ShowVatRegistrationId = showVatRegistrationId;
			ShowSaveCreditCardNumber = showSaveCreditCardNumber;
			SaveCreditCardNumberNote = saveCreditCardNumberNote;
			ShowRecurringOrders = showRecurringOrders;
			ShowWallets = showWallets;
			CustomerLevel = customerLevel;
			HasMicropayBalance = hasMicropayBalance;
			ShowMicropayLink = showMicropayLink;
			MicropayLink = micropayLink;
			MicropayBalance = micropayBalance;
			LocaleSetting = localeSetting;
			CurrencySetting = currencySetting;
			RequireEmailConfirmation = requireEmailConfirmation;
			DisplayOver13Selector = displayOver13Selector;
		}
	}

	public class AccountPostViewModel
	{
		public AccountViewModel Account
		{ get; set; }
	}

	public class AccountViewModel
	{
		[HtmlAutocompleteType(HtmlAutocomplete.GivenName)]
		[Display(Name = "account.firstname.label", Prompt = "account.firstname.example")]
		[Required(ErrorMessage = "account.firstname.required")]
		[StringLength(100, ErrorMessage = "account.firstname.length")]
		public string FirstName
		{ get; set; }

		[HtmlAutocompleteType(HtmlAutocomplete.FamilyName)]
		[Display(Name = "account.lastname.label", Prompt = "account.lastname.example")]
		[Required(ErrorMessage = "account.lastname.required")]
		[StringLength(100, ErrorMessage = "account.lastname.length")]
		public string LastName
		{ get; set; }

		[HtmlAutocompleteType(HtmlAutocomplete.Email)]
		[Display(Name = "account.emailaddress.label", Prompt = "account.emailaddress.example")]
		[Required(ErrorMessage = "account.emailaddress.required")]
		[RegularExpression(EmailAddressValidator.ValidationRegularExpression, ErrorMessage = "account.emailaddress.invalid")]
		[DataType(DataType.EmailAddress, ErrorMessage = "account.emailaddress.format")]
		[StringLength(100, ErrorMessage = "account.emailaddress.length")]
		public string Email
		{ get; set; }

		[HtmlAutocompleteType(HtmlAutocomplete.Off)]
		[Display(Name = "account.emailconfirmation.label", Prompt = "account.emailconfirmation.example")]
		[RequiredIfAppConfigTrue(appConfigName: "RequireEmailConfirmation", ErrorMessage = "account.emailconfirmation.required")]
		[CompareIfAppConfigTrue(appConfigName: "RequireEmailConfirmation", otherProperty: "Email", ErrorMessage = "account.emailconfirmation.invalid")]
		public string EmailConfirmation
		{ get; set; }

		[HtmlAutocompleteType(HtmlAutocomplete.NewPassword)]
		[Display(Name = "account.changepassword.label", Prompt = "account.changepassword.example")]
		[StrongPasswordDisplay(weakDescription: "account.aspx.19", strongDescription: "account.strongPassword")]
		[DataType(DataType.Password)]
		[StringLength(50, ErrorMessage = "account.changepassword.length")]
		[PasswordRegularExpression(normalPasswordErrorMessage: "account.changepassword.normalRegexFailure", strongPasswordErrorMessage: "account.changepassword.strongRegexFailure")]
		public virtual string Password
		{ get; set; }

		[HtmlAutocompleteType(HtmlAutocomplete.Off)]
		[Display(Name = "account.confirmchangepassword.label", Prompt = "account.confirmchangepassword.example")]
		[DataType(DataType.Password)]
		[StringLength(50, ErrorMessage = "account.confirmchangepassword.length")]
		[Compare("Password", ErrorMessage = "account.confirmchangepassword.mismatch")]
		public virtual string PasswordConfirmation
		{ get; set; }

		[HtmlAutocompleteType(HtmlAutocomplete.Telephone)]
		[Display(Name = "account.phone.label", Prompt = "account.phone.example")]
		[Required(ErrorMessage = "account.phone.required")]
		[WhatsThis("account.phone.whats-this.content")]
		[StringLength(25, ErrorMessage = "account.phone.length")]
		public string Phone
		{ get; set; }
		
		[RequiredIfAppConfigTrue("RequireOver13Checked")]
		[Display(Name = "account.over13.label")]
		public bool IsOver13
		{ get; set; }

		[Display(Name = "account.oktoemail.label", Description = "account.aspx.27")]
		public bool IsOkToEmail
		{ get; set; }

		[Display(Name = "account.storecc.label")]
		public bool SaveCreditCardNumber
		{ get; set; }

		[Display(Name = "account.vatid.label")]
		[StringLength(100, ErrorMessage = "account.vatid.length")]
		public string VatRegistrationId
		{ get; set; }

		[Display(Name = "signin.captcha.label")]
		[StringLength(6, ErrorMessage = "signin.captcha.length", MinimumLength = 6)]
		public virtual string CaptchaCode
		{ get; set; }
	}

	public class AccountAddressViewModel
	{
		public int Id
		{ get; set; }

		public string FirstName
		{ get; set; }

		public string LastName
		{ get; set; }

		public string Company
		{ get; set; }

		public string Address1
		{ get; set; }

		public string Address2
		{ get; set; }

		public string Suite
		{ get; set; }

		public string City
		{ get; set; }

		public string State
		{ get; set; }

		public string Zip
		{ get; set; }

		public string Country
		{ get; set; }

		public string Phone
		{ get; set; }
	}

	public class AccountOrderViewModel
	{
		public int OrderNumber
		{ get; set; }

		public DateTime OrderDate
		{ get; set; }

		public string PaymentStatus
		{ get; set; }

		public string TransactionStateNotificationType
		{ get; set; }

		public string TransactionState
		{ get; set; }

		public string ShippingStatus
		{ get; set; }

		public string OrderTotal
		{ get; set; }

		public string CustomerServiceNotes
		{ get; set; }

		public bool CanReorder
		{ get; set; }
	}
}
