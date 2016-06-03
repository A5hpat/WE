// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways.Processors;
using GatewayAuthorizeNet;
using GatewayAuthorizeNet.AuthorizeNetApi;

namespace AspDotNetStorefront.Payment.Wallet
{
	public class AuthorizeNetWalletProvider : IWalletProvider
	{
		public bool WalletsAreEnabled()
		{
			return new AuthorizeNet().IsCimEnabled;
		}

		public long GetProfileId(Customer customer)
		{
			return DataUtility.GetProfileId(customer.CustomerID);
		}

		public PaymentProfile GetPaymentProfile(Customer customer, long paymentProfileId)
		{
			var paymentProfileWrapper = DataUtility
				.GetPaymentProfileWrapper(customer.CustomerID, customer.EMail, paymentProfileId);

			return new PaymentProfile(
				customerId: (int)paymentProfileWrapper.CustomerId,
				email: customer.EMail,
				profileId: paymentProfileWrapper.ProfileId,
				paymentProfileId: paymentProfileId,
				creditCardNumberMasked: paymentProfileWrapper.CreditCardNumberMasked,
				cardType: paymentProfileWrapper.CardType,
				expirationMonth: paymentProfileWrapper.ExpirationMonth,
				expirationYear: paymentProfileWrapper.ExpirationYear);
		}

		public IEnumerable<PaymentProfile> GetPaymentProfiles(Customer customer)
		{
			return GatewayAuthorizeNet.DataUtility
				.GetPaymentProfiles(customer.CustomerID, customer.EMail)
				.Select(paymentProfileWrapper => GetPaymentProfile(customer, paymentProfileWrapper.ProfileId));
		}

		public long CreateProfile(Customer customer)
		{
			string errorMessage;
			var profileManager = ProfileManager.CreateProfile(
					customerId: customer.CustomerID,
					email: customer.EMail,
					errorMessage: out errorMessage);

			if(!string.IsNullOrEmpty(errorMessage))
				throw new WalletException(errorMessage);

			DataUtility.SaveProfileId(customer.CustomerID, profileManager.ProfileId);

			return profileManager.ProfileId;
		}

		public void CreatePaymentProfile(Customer customer, Address billingAddress, string cardType, string number, string cvv, DateTime expirationDate)
		{
			var profileId = GetProfileId(customer);
			if(profileId == 0)
				profileId = CreateProfile(customer);

			var profileManager = new ProfileManager(
				customerId: customer.CustomerID,
				email: customer.EMail,
				profileId: profileId);

			var address = new CustomerAddressType
			{
				address = billingAddress.Address1,
				city = billingAddress.City,
				company = billingAddress.Company,
				country = billingAddress.Country,
				faxNumber = billingAddress.Fax,
				firstName = billingAddress.FirstName,
				lastName = billingAddress.LastName,
				phoneNumber = billingAddress.Phone,
				state = billingAddress.State,
				zip = billingAddress.Zip
			};

			var paymentProfile = profileManager.CreatePaymentProfile(
				address: address,
				creditCardNumber: number,
				cardCode: cvv,
				expMonth: expirationDate.Month,
				expYear: expirationDate.Year);

			if(!string.IsNullOrEmpty(paymentProfile.ErrorCode))
				throw new WalletException(paymentProfile.ErrorMessage);

			DataUtility.SavePaymentProfile(
				customerid: customer.CustomerID,
				addressId: billingAddress.AddressID,
				paymentProfileId: paymentProfile.PaymentProfileId,
				expirationMonth: expirationDate.Month.ToString(),
				expirationYear: expirationDate.Year.ToString(),
				cardType: cardType);
		}

		public void DeletePaymentProfile(Customer customer, long paymentProfileId)
		{
			var profileId = GetProfileId(customer);

			var profileManager = new ProfileManager(
				customerId: customer.CustomerID,
				email: customer.EMail,
				profileId: profileId);

			if(profileManager == null)
				return;

			profileManager.DeletePaymentProfile(paymentProfileId);
			DataUtility.DeletePaymentProfile(customer.CustomerID, paymentProfileId);
		}
	}
}
