// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.ComponentModel.DataAnnotations;
using AspDotNetStorefront.Validation.DataAttribute;

namespace AspDotNetStorefront.Models
{
	public class AccountCreateViewModel : AccountViewModel
	{
		[Required(ErrorMessage = "signin.password.required")]
		[StrongPasswordDisplay(weakDescription:"account.aspx.19", strongDescription: "account.strongPassword")]
		[Display(Name = "account.password.label", Prompt = "account.changepassword.example")]
		public override string Password
		{ get; set; }

		[Required(ErrorMessage = "signin.passwordconfirmation.required")]
		public override string PasswordConfirmation
		{ get; set; }

		[RequiredIfAppConfigTrue(appConfigName: "SecurityCodeRequiredOnCreateAccount", ErrorMessage = "account.captcha.required")]
		public override string CaptchaCode
		{ get; set; }
	}

	public class AccountCreatePostModel
	{
		public AccountCreateViewModel Account
		{ get; set; }

		public AccountAddressViewModel PrimaryBillingAddress
		{ get; set; }

		public AccountAddressViewModel PrimaryShippingAddress
		{ get; set; }
	}
	
	public class AccountCreateIndexViewModel : AccountCreatePostModel
	{
		public readonly bool DisplayCaptcha;
		public readonly bool RequireEmailConfirmation;
		public readonly bool DisplayOver13Selector;

		public AccountCreateIndexViewModel(bool displayCaptcha, bool requireEmailConfirmation, bool displayOver13Selector)
		{
			DisplayCaptcha = displayCaptcha;
			RequireEmailConfirmation = requireEmailConfirmation;
			DisplayOver13Selector = displayOver13Selector;
		}
	}
}
