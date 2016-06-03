// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Checkout
{
	public interface IAppliedPaymentMethodCleanup
	{
		void Cleanup(AppliedPaymentMethodCleanupContext context);
	}

	public class AppliedPaymentMethodCleanupContext
	{
		public readonly Customer Customer;
		public readonly int OrderNumber;
		public readonly string Status;
		public readonly string PaymentMethod;
		public readonly string Gateway;

		public AppliedPaymentMethodCleanupContext(Customer customer, int orderNumber, string status, string paymentMethod, string gateway = null)
		{
			Customer = customer;
			OrderNumber = orderNumber;
			Status = status;
			PaymentMethod = paymentMethod;
			Gateway = gateway;
		}
	}
}
