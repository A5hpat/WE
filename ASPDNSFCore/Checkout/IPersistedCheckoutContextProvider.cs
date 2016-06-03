// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Checkout
{
	public interface IPersistedCheckoutContextProvider
	{
		void ClearCheckoutContext(Customer customer);

		PersistedCheckoutContext LoadCheckoutContext(Customer customer);

		void SaveCheckoutContext(Customer customer, PersistedCheckoutContext checkoutContext);
	}

	public class PersistedCheckoutContext
	{
		public readonly CreditCardDetails CreditCard;
		public readonly PayPalExpressDetails PayPalExpress;
		public readonly PurchaseOrderDetails PurchaseOrder;
		public readonly BraintreeDetails Braintree;
		public readonly AmazonPaymentsDetails AmazonPayments;
		public readonly bool TermsAndConditionsAccepted;
		public readonly bool Over13Checked;
		public readonly ShippingEstimateDetails ShippingEstimateDetails;
		public readonly int? OffsiteRequiresBillingAddressId;
		public readonly int? OffsiteRequiresShippingAddressId;
		public readonly string Email;
		public readonly int? SelectedShippingMethodId;

		public PersistedCheckoutContext(CreditCardDetails creditCard,
			PayPalExpressDetails payPalExpress,
			PurchaseOrderDetails purchaseOrder,
			BraintreeDetails braintree,
			AmazonPaymentsDetails amazonPayments,
			bool termsAndConditionsAccepted,
			bool over13Checked,
			ShippingEstimateDetails shippingEstimateDetails,
			int? offsiteRequiresBillingAddressId,
			int? offsiteRequiresShippingAddressId,
			string email,
			int? selectedShippingMethodId)
		{
			CreditCard = creditCard;
			PayPalExpress = payPalExpress;
			PurchaseOrder = purchaseOrder;
			Braintree = braintree;
			AmazonPayments = amazonPayments;
			TermsAndConditionsAccepted = termsAndConditionsAccepted;
			Over13Checked = over13Checked;
			ShippingEstimateDetails = shippingEstimateDetails;
			OffsiteRequiresBillingAddressId = offsiteRequiresBillingAddressId;
			OffsiteRequiresShippingAddressId = offsiteRequiresShippingAddressId;
			Email = email;
			SelectedShippingMethodId = selectedShippingMethodId;
		}
	}

	public class CreditCardDetails
	{
		public readonly string Name;
		public readonly string Number;
		public readonly string IssueNumber;
		public readonly string CardType;
		public readonly DateTime? ExpirationDate;
		public readonly DateTime? StartDate;
		public readonly string Cvv;

		public CreditCardDetails(string name, string number, string issueNumber, string cardType, DateTime? expirationDate, DateTime? startDate, string cvv)
		{
			Name = name;
			Number = number;
			IssueNumber = issueNumber;
			CardType = cardType;
			ExpirationDate = expirationDate;
			StartDate = startDate;
			Cvv = cvv;
		}
	}

	public class PayPalExpressDetails
	{
		public readonly string Token;
		public readonly string PayerId;

		public PayPalExpressDetails(string token, string payerId)
		{
			Token = token;
			PayerId = payerId;
		}
	}

	public class PurchaseOrderDetails
	{
		public readonly string Number;

		public PurchaseOrderDetails(string number)
		{
			Number = number;
		}
	}

	public class BraintreeDetails
	{
		public readonly string Nonce;
		public readonly string Token;
		public readonly string PaymentMethod;
		public readonly bool ThreeDSecureApproved;

		public BraintreeDetails(string nonce, string token, string paymentMethod, bool threeDSecureApproved)
		{
			Nonce = nonce;
			Token = token;
			PaymentMethod = paymentMethod;
			ThreeDSecureApproved = threeDSecureApproved;
		}
	}

	public class AmazonPaymentsDetails
	{
		public readonly string AmazonOrderReferenceId;

		public AmazonPaymentsDetails(string amazonOrderReferenceId)
		{
			AmazonOrderReferenceId = amazonOrderReferenceId;
		}
	}

	public class ShippingEstimateDetails
	{
		public readonly string Country;
		public readonly string City;
		public readonly string State;
		public readonly string PostalCode;

		public ShippingEstimateDetails(
			string country,
			string city,
			string state,
			string postalCode)
		{
			Country = country;
			City = city;
			State = state;
			PostalCode = postalCode;
		}
	}
}
