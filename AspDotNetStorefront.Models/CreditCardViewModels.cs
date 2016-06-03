// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using AspDotNetStorefront.Validation.DataAttribute;

namespace AspDotNetStorefront.Models
{
	public class CreditCardViewModel
	{
		[Required(ErrorMessage = "address.cs.24")]
		[Display(Name = "address.cs.23")]
		public string Name
		{ get; set; }

		[Required(ErrorMessage = "address.cs.27")]
		[Display(Name = "address.cs.25")]
		public string Number
		{ get; set; }

		[Display(Name = "address.cs.31")]
		public string CardType
		{ get; set; }

		[Display(Name = "address.cs.33")]
		public int ExpirationMonth
		{ get; set; }
		
		public int ExpirationYear
		{ get; set; }

		[Display(Name = "address.cs.59")]
		public int CardStartMonth
		{ get; set; }
		
		public int CardStartYear
		{ get; set; }

		[Display(Name = "address.cs.61")]
		public string CardIssueNumber
		{ get; set; }
	}

	public class CheckoutCreditCardViewModel
	{
		[Display(Name = "creditCardDetails.cardName.label", Prompt = "creditCardDetails.cardName.example")]
		[Required(ErrorMessage = "creditCardDetails.cardName.required")]
		[HtmlAutocompleteType(HtmlAutocomplete.CcName)]
		[StringLength(100, ErrorMessage = "creditCardDetails.cardName.length")]
		public string Name
		{ get; set; }

		[Display(Name = "creditCardDetails.cardNumber.label", Prompt = "creditCardDetails.cardNumber.example")]
		[Required(ErrorMessage = "creditCardDetails.cardNumber.required")]
		[HtmlAutocompleteType(HtmlAutocomplete.CcNumber)]
		[StringLength(300, ErrorMessage = "creditCardDetails.cardNumber.length")]
		public string Number
		{ get; set; }

		public string LastFour
		{ get; set; }

		public string CardImage
		{ get; set; }

		[Display(Name = "creditCardDetails.cardType.label", Prompt = "creditCardDetails.cardType.example")]
		[Required(ErrorMessage = "creditCardDetails.cardType.required")]
		[HtmlAutocompleteType(HtmlAutocomplete.CcType)]
		[StringLength(25, ErrorMessage = "creditCardDetails.cardType.length")]
		public string CardType
		{ get; set; }

		[Display(Name = "creditCardDetails.cardIssueNumber.label", Prompt = "creditCardDetails.cardIssueNumber.example")]
		[StringLength(25, ErrorMessage = "creditCardDetails.cardIssueNumber.length")]
		public string IssueNumber
		{ get; set; }

		[Display(Name = "creditCardDetails.cardExpirationDate.label", Prompt = "creditCardDetails.cardExpirationDate.example")]
		[RegularExpression(@"\d{1,2}\s*[/-]\s*\d{2,4}", ErrorMessage = "creditCardDetails.cardExpirationDate.invalid")]
		[Required(ErrorMessage = "creditCardDetails.cardExpirationDate.required")]
		[HtmlAutocompleteType(HtmlAutocomplete.CcExp)]
		[CreditCardFutureExpirationDate(ErrorMessage = "creditCardDetails.cardExpirationDate.inThePast")]
		[StringLength(10, ErrorMessage = "creditCardDetails.cardExpirationDate.length")]
		public string ExpirationDate
		{ get; set; }

		[Display(Name = "creditCardDetails.cardStartDate.label", Prompt = "creditCardDetails.cardStartDate.example")]
		[RegularExpression(@"\d{1,2}\s*[/-]\s*\d{2,4}", ErrorMessage = "creditCardDetails.cardStartDate.invalid")]
		[StringLength(10, ErrorMessage = "creditCardDetails.cardStartDate.length")]
		public string StartDate
		{ get; set; }

		[Display(Name = "creditCardDetails.cardCvc.label", Prompt = "creditCardDetails.cardCvc.example")]
		[RequiredIfAppConfigFalse("CardExtraCodeIsOptional", ErrorMessage = "creditCardDetails.cardCvc.required")]
		[HtmlAutocompleteType(HtmlAutocomplete.CcCsc)]
		[StringLength(10, ErrorMessage = "creditCardDetails.cardCvc.length")]
		public string Cvv
		{ get; set; }

		public bool ShowStartDate
		{ get; set; }

		public bool ShowIssueNumber
		{ get; set; }

		public bool ShowSaveCreditCardNumber
		{ get; set; }

		[Display(Name = "checkout.storecc.label")]
		public bool SaveCreditCardNumber
		{ get; set; }

		public bool WalletsAreEnabled
		{ get; set; }

		public bool DisplayWalletCards
		{ get; set; }

		[Display(Name= "checkout.wallet.save")]
		public bool SaveToWallet
		{ get; set; }

		public IEnumerable<SelectListItem> CardTypes
		{ get; set; }

		public bool ValidateCreditCardNumber
		{ get; set; }
	}
}
