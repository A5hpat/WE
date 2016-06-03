// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Checkout
{
	public interface IPaymentOptionProvider
	{
		bool CheckoutIsOffsiteOnly(Customer customer, ShoppingCart cart);

		PaymentOption GetCustomerSelectedPaymentOption(IEnumerable<PaymentOption> paymentOptions, Customer customer);

		IEnumerable<PaymentOption> GetPaymentOptions(Customer customer, ShoppingCart cart);

		bool PaymentMethodSelectionIsValid(string selectedPaymentMethod, Customer customer);
	}

	public class PaymentOption
	{
		public readonly PaymentMethodInfo Info;
		public readonly bool Available;
		public readonly int DisplayOrder;
		public readonly string SelectionImage;
		public readonly string EditUrl;
		public readonly bool IsOffsiteForDisplay;
		public readonly bool IsEditable;
		public readonly PaymentScriptInfo PaymentScriptProvider;

		public PaymentOption(PaymentMethodInfo info, bool available, int displayOrder, string selectionImage, string editUrl, bool isOffsiteForDisplay, bool isEditable, PaymentScriptInfo paymentScriptProvider = null)
		{
			Info = info;
			Available = available;
			DisplayOrder = displayOrder;
			SelectionImage = selectionImage;
			EditUrl = editUrl;
			IsOffsiteForDisplay = isOffsiteForDisplay;
			IsEditable = isEditable;
			PaymentScriptProvider = paymentScriptProvider;
		}
	}

	public class PaymentScriptInfo
	{
		public readonly IEnumerable<IPaymentScript> PaymentScripts;
		public readonly string ScriptTarget;

		public PaymentScriptInfo(IEnumerable<IPaymentScript> paymentScripts, string scriptTarget)
		{
			PaymentScripts = paymentScripts ?? Enumerable.Empty<IPaymentScript>();
			ScriptTarget = scriptTarget;
		}
	}

	public interface IPaymentScript
	{
		string Render();
	}
}
