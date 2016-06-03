// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.ComponentModel.DataAnnotations;
using AspDotNetStorefront.Validation.DataAttribute;
using AspDotNetStorefrontCore.Validation;

namespace AspDotNetStorefront.Models
{
	public class CheckoutAccountPostModel
	{
		[Display(Name = "checkout.email.label")]
		[RegularExpression(EmailAddressValidator.ValidationRegularExpression, ErrorMessage = "account.emailaddress.invalid")]
		[DataType(DataType.EmailAddress, ErrorMessage = "account.emailaddress.format")]
		[StringLength(100, ErrorMessage = "account.emailaddress.length")]
		[Required(ErrorMessage = "account.emailaddress.required")]
		public string Email
		{ get; set; }

		[Display(Name = "checkout.password.label")]
		[DataType(DataType.Password)]
		[StringLength(50, ErrorMessage = "account.password.length")]
		[StrongPasswordDisplay(weakDescription: "account.aspx.19", strongDescription: "account.strongPassword")]
		public string Password
		{ get; set; }

		[Display(Name = "account.captcha.label")]
		[RequiredIfAppConfigTrue(appConfigName: "SecurityCodeRequiredOnCheckout", ErrorMessage = "account.captcha.required")]
		public string Captcha
		{ get; set; }
	}

	public class CheckoutAccountViewModel : CheckoutAccountPostModel
	{
		public readonly bool PasswordRequired;
		public readonly bool ShowCaptcha;
		public readonly bool PasswordResetAvailable;

		public CheckoutAccountViewModel(bool passwordRequired, bool showCaptcha, bool passwordResetAvailable)
		{
			PasswordRequired = passwordRequired;
			ShowCaptcha = showCaptcha;
			PasswordResetAvailable = passwordResetAvailable;
		}
	}
}
