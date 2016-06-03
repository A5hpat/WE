// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
namespace AspDotNetStorefront.Checkout.Engine
{
	public class CheckoutEvaluationResult
	{
		public readonly CheckoutState State;
		public readonly CheckoutStageContext CheckoutStageContext;
		public readonly CheckoutSelectionContext Selections;

		public CheckoutEvaluationResult(
			CheckoutState state,
			CheckoutStageContext checkoutStageContext,
			CheckoutSelectionContext selections)
		{
			State = state;
			CheckoutStageContext = checkoutStageContext;
			Selections = selections;
		}
	}
}
