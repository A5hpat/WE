// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
namespace AspDotNetStorefront.Checkout
{
	public interface IPaymentMethodInfoProvider
	{
		PaymentMethodInfo GetPaymentMethodInfo(string paymentMethod, string gateway);
	}

	public class PaymentMethodInfo
	{
		public readonly string Name;
		public readonly string DisplayName;
		public readonly PaymentMethodLocation Location;
		public bool RequiresBillingSelection;

		public PaymentMethodInfo(string name, string displayName, PaymentMethodLocation location, bool requiresBillingSelection)
		{
			Name = name;
			DisplayName = displayName;
			Location = location;
			RequiresBillingSelection = requiresBillingSelection;
		}
	}

	public enum PaymentMethodLocation
	{
		Onsite,
		Offsite,
	}
}
