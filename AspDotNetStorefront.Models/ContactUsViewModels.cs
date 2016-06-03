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
	public class ContactUsViewModel
	{
		[Display(Name = "ContactUs.Name.Label", Prompt = "ContactUs.Name.Example")]
		[Required(ErrorMessage = "ContactUs.Name.Required")]
		[HtmlAutocompleteType(HtmlAutocomplete.Name)]
		public string From { get; set; }

		[Display(Name = "ContactUs.EmailAddress.Label", Prompt = "ContactUs.EmailAddress.Example")]
		[Required(ErrorMessage = "ContactUs.EmailAddress.Required")]
		[RegularExpression(EmailAddressValidator.ValidationRegularExpression, ErrorMessage = "ContactUs.EmailAddress.Format")]
		[DataType(DataType.EmailAddress, ErrorMessage = "ContactUs.EmailAddress.Format")]
		[HtmlAutocompleteType(HtmlAutocomplete.Email)]
		public string Email { get; set; }

		[Display(Name = "ContactUs.PhoneNumber.Label", Prompt = "ContactUs.PhoneNumber.Example")]
		[Required(ErrorMessage = "ContactUs.PhoneNumber.Required")]
		[DataType(DataType.PhoneNumber, ErrorMessage = "ContactUs.PhoneNumber.Format")]
		[HtmlAutocompleteType(HtmlAutocomplete.Telephone)]
		public string Phone { get; set; }

		[Display(Name = "ContactUs.Subject.Label", Prompt = "ContactUs.Subject.Example")]
		[Required(ErrorMessage = "ContactUs.Subject.Required")]
		public string Subject { get; set; }

		[Display(Name = "ContactUs.Message.Label", Prompt = "ContactUs.Message.Example")]
		[Required(ErrorMessage = "ContactUs.Message.Required")]
		[DataType(DataType.MultilineText)]
		public string Message { get; set; }

		[Display(Name = "ContactUs.Captcha.Label")]
		[RequiredIfAppConfigTrue("ContactUs.UseCaptcha", ErrorMessage = "ContactUs.Captcha.Required")]
		public string CaptchaCode { get; set; }
	}

	public class ContactUsRenderModel : ContactUsViewModel
	{
		public readonly string PageHeader;
		public readonly bool UseCaptcha;

		public ContactUsRenderModel(string pageHeader, bool useCaptcha)
		{
			PageHeader = pageHeader;
			UseCaptcha = useCaptcha;
		}
	}
}
