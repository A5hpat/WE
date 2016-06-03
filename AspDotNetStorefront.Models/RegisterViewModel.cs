// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.ComponentModel.DataAnnotations;
using AspDotNetStorefrontCore.Validation;

namespace AspDotNetStorefront.Models
{
	public class RegisterViewModel
	{
		[Display(Name = "signin.aspx.10")]
		[Required(ErrorMessage = "signin.aspx.3")]
		[RegularExpression(EmailAddressValidator.ValidationRegularExpression, ErrorMessage = "account.emailaddress.invalid")]
		[DataType(DataType.EmailAddress, ErrorMessage = "account.emailaddress.format")]
		[StringLength(100)]
		public string Email
		{ get; set; }

		public bool OkToEmail
		{ get; set; }

		public readonly bool ShowOkToEmail;

		public RegisterViewModel(bool okToEmail, bool showOkToEmail)
		{
			OkToEmail = okToEmail;
			ShowOkToEmail = showOkToEmail;
		}
	}
}
