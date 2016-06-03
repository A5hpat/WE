// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;

namespace AspDotNetStorefront.Checkout.Engine
{
	public class TransitionBuilder
	{
		readonly Guards Guards;

		public TransitionBuilder(Guards guards)
		{
			Guards = guards;
		}

		// These methods are gateways into TransitionBuilder<T> and its extension methods
		public TransitionBuilder<Guard> If(params Guard[] guards)
		{
			return new TransitionBuilder<Guard>(guards: guards);
		}

		public TransitionBuilder<Guard> Always()
		{
			return new TransitionBuilder<Guard>(guards: new Guard[] { Guards.Always });
		}
	}

	public class TransitionBuilder<T>
	{
		public readonly CheckoutState? Target;
		public readonly IEnumerable<Guard> Guards;
		public readonly IEnumerable<CheckoutEngine.Trigger> Triggers;

		public TransitionBuilder(CheckoutState? target = null, IEnumerable<Guard> guards = null, IEnumerable<CheckoutEngine.Trigger> triggers = null)
		{
			Target = target;
			Guards = guards;
			Triggers = triggers;
		}

		public static implicit operator CheckoutEngine.Transition(TransitionBuilder<T> builder)
		{
			return new CheckoutEngine.Transition(
				target: builder.Target.Value,
				guards: builder.Guards,
				triggers: builder.Triggers);
		}
	}

	static class TransitionBuilderExtensions
	{
		public static TransitionBuilder<CheckoutState> TransitionTo(this TransitionBuilder<Guard> builder, CheckoutState target)
		{
			return new TransitionBuilder<CheckoutState>(
				target: target,
				guards: builder.Guards);
		}

		public static TransitionBuilder<CheckoutEngine.Trigger> Then(this TransitionBuilder<CheckoutState> builder, params CheckoutEngine.Trigger[] triggers)
		{
			return new TransitionBuilder<CheckoutEngine.Trigger>(
				target: builder.Target,
				guards: builder.Guards,
				triggers: triggers);
		}
	}
}
