// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Payment.Wallet
{
	public interface IWalletProvider
	{
		bool WalletsAreEnabled();

		long GetProfileId(Customer customer);

		PaymentProfile GetPaymentProfile(Customer customer, long paymentProfileId);

		IEnumerable<PaymentProfile> GetPaymentProfiles(Customer customer);

		long CreateProfile(Customer customer);

		void CreatePaymentProfile(Customer customer, Address billingAddress, string cardType, string number, string cvv, DateTime expirationDate);

		void DeletePaymentProfile(Customer customer, long paymentProfileId);

	}

	public class PaymentProfile
	{
		public readonly int CustomerId;
		public readonly string Email;
		public readonly long ProfileId;
		public readonly long PaymentProfileId;
		public readonly string CreditCardNumberMasked;
		public readonly string CardType;
		public readonly string ExpirationMonth;
		public readonly string ExpirationYear;
		public readonly int AddressId;

		public PaymentProfile(int customerId, string email, long profileId, long paymentProfileId, string creditCardNumberMasked, string cardType, string expirationMonth, string expirationYear, int addressId = 0)
		{
			Email = email;
			CustomerId = customerId;
			ProfileId = profileId;
			PaymentProfileId = paymentProfileId;
			CreditCardNumberMasked = creditCardNumberMasked;
			CardType = cardType;
			ExpirationMonth = expirationMonth;
			ExpirationYear = expirationYear;
			AddressId = addressId;
		}
	}

	[Serializable]
	public class WalletException : Exception
	{
		public WalletException(string message)
			: base(message)
		{ }
	}
}
