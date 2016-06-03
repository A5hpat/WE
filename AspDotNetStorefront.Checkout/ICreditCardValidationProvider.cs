// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Linq;

namespace AspDotNetStorefront.Checkout
{
	public interface ICreditCardValidationProvider
	{
		CreditCardValidationResult ValidateCreditCard(CreditCardValidationConfiguration configuration, CreditCardValidationContext context);
	}

	public class CreditCardValidationConfiguration
	{
		public readonly bool ValidateCreditCardNumber;
		public readonly bool ShowCardStartDateFields;
		public readonly bool CardExtraCodeIsOptional;

		public CreditCardValidationConfiguration(bool validateCreditCardNumber, bool showCardStartDateFields, bool cardExtraCodeIsOptional)
		{
			ValidateCreditCardNumber = validateCreditCardNumber;
			ShowCardStartDateFields = showCardStartDateFields;
			CardExtraCodeIsOptional = cardExtraCodeIsOptional;
		}
	}

	public class CreditCardValidationContext
	{
		public readonly string CardType;
		public readonly string Number;
		public readonly string IssueNumber;
		public readonly DateTime? ExpirationDate;
		public readonly DateTime? StartDate;
		public readonly string Cvv;

		public CreditCardValidationContext(string cardType, string number, string issueNumber, DateTime? expirationDate, DateTime? startDate, string cvv)
		{
			CardType = cardType;
			Number = number;
			IssueNumber = issueNumber;
			ExpirationDate = expirationDate;
			StartDate = startDate;
			Cvv = cvv;
		}
	}

	public class CreditCardValidationResult
	{
		public readonly bool Valid;
		public readonly ILookup<CreditCardValidationField, string> FieldErrors;

		public CreditCardValidationResult(bool valid, ILookup<CreditCardValidationField, string> fieldErrors)
		{
			Valid = valid;
			FieldErrors = fieldErrors;
		}
	}

	public enum CreditCardValidationField
	{
		CardType,
		Number,
		IssueNumber,
		ExpirationDate,
		StartDate,
		Cvv,
	}
}
