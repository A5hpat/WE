// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace AspDotNetStorefront.Models
{
	public class WalletSelectViewModel
	{
		[Range(1, long.MaxValue, ErrorMessage = "checkout.wallet.required")]
		[Required(ErrorMessage = "checkout.wallet.required")]
		public long SelectedPaymentProfileId
		{ get; set; }

		public readonly IEnumerable<WalletPaymentType> WalletPaymentTypes;

		public WalletSelectViewModel()
			: this(null)
		{ }

		public WalletSelectViewModel(IEnumerable<WalletPaymentType> walletPaymentTypes)
		{
			WalletPaymentTypes = walletPaymentTypes ?? Enumerable.Empty<WalletPaymentType>();
		}
	}

	public class WalletIndexViewModel
	{
		public readonly IEnumerable<WalletPaymentType> PaymentTypes;

		public WalletIndexViewModel(IEnumerable<WalletPaymentType> paymentTypes)
		{
			PaymentTypes = paymentTypes;
		}
	}

	public class WalletEditViewModel : WalletPaymentType
	{
		public readonly SelectList DateExpirationMonthOptions;
		public readonly SelectList DateExpirationYearOptions;
		public readonly SelectList BillingAddressOptions;

		public WalletEditViewModel(
			SelectList dateExpirationMonthOptions,
			SelectList dateExpirationYearOptions,
			SelectList billingAddressOptions)
		{
			DateExpirationMonthOptions = dateExpirationMonthOptions;
			DateExpirationYearOptions = dateExpirationYearOptions;
			BillingAddressOptions = billingAddressOptions;
		}
	}

	public class WalletPaymentType
	{
		public long PaymentProfileId
		{ get; set; }

		public string CardType
		{ get; set; }

		public string CardImage
		{ get; set; }

		[Display(Name = "account.aspx.97")]
		public int BillingAddressId
		{ get; set; }

		[Display(Name = "account.aspx.99")]
		public bool MakePrimary
		{ get; set; }

		[Display(Name = "account.aspx.93", Prompt = "creditCardDetails.cardNumber.example")]
		[StringLength(100, ErrorMessage = "creditCardDetails.cardNumber.length")]
		[Required(ErrorMessage = "address.cs.27")]
		public string CardNumber
		{ get; set; }

		[Display(Name = "account.aspx.96")]
		[Required(ErrorMessage = "address.cs.30")]
		[StringLength(25, ErrorMessage = "creditCardDetails.cardIssueNumber.length")]
		public string CardSecurityCode
		{ get; set; }

		[Display(Name = "address.cs.34")]
		[Required(ErrorMessage = "address.cs.20")]
		public string ExpirationMonth
		{ get; set; }

		[Display(Name = "address.cs.35")]
		[Required(ErrorMessage = "address.cs.21")]
		public string ExpirationYear
		{ get; set; }
	}
}
