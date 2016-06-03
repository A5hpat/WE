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

namespace AspDotNetStorefront.Checkout.Engine
{
	/// <summary>
	/// <see cref="CheckoutEngine"/> is a centralized class to validate a customer's state for 
	/// proceeding through checkout.
	/// </summary>
	public class CheckoutEngine
	{
		readonly IPaymentMethodInfoProvider PaymentMethodInfoProvider;
		readonly Guards Guards;
		readonly TransitionBuilder TransitionBuilder;

		readonly IReadOnlyDictionary<CheckoutState, IEnumerable<Transition>> StateMachine;

		public CheckoutEngine(Guards guards, IPaymentMethodInfoProvider paymentMethodInfoProvider, TransitionBuilder transitionBuilder)
		{
			PaymentMethodInfoProvider = paymentMethodInfoProvider;
			Guards = guards;
			TransitionBuilder = transitionBuilder;

			// This state machine defines the checkout validation workflow. The keys are states 
			// and the values are arrays of transitions out of that state. Transitions describe
			// the preconditions that must be met before we can move to a new state.

			// When the checkout engine runs, it looks up the current state (dictionary key) and
			// gets the transitions for that key (dictionary value). For each transition, it runs 
			// the guards. Each guard checks some state and simply returns true or false. The first
			// transition for which all guards return true will be taken. If there are any triggers
			// on that transition, they will be executed, then the current state will be set to
			// the transition's target state. Now we have a new current state and we'll run this 
			// again.

			// We keep looping until we end up in a state with no valid transitions (a "terminal
			// state") or we transition back to the same state we started from (basic infinite 
			// loop protection). In either case, we have a final state that we return to the
			// caller.
			StateMachine = new Dictionary<CheckoutState, IEnumerable<Transition>>
				{
					{
						CheckoutState.Start,
						new Transition[]
						{
							TransitionBuilder
								.Always()
								.TransitionTo(CheckoutState.ShoppingCart_Validating),
						}
					},
					{
						CheckoutState.ShoppingCart_Validating,
						new Transition[]
						{
							TransitionBuilder
								.If(Guards.ShoppingCartIsEmpty)
								.TransitionTo(CheckoutState.ShoppingCartIsEmpty),

							TransitionBuilder
								.If(Guards.SubtotalDoesNotMeetMinimumAmount)
								.TransitionTo(CheckoutState.SubtotalDoesNotMeetMinimumAmount),

							TransitionBuilder
								.If(Guards.CartItemsLessThanMinimumItemCount)
								.TransitionTo(CheckoutState.CartItemsLessThanMinimumItemCount),

							TransitionBuilder
								.If(Guards.CartItemsGreaterThanMaximumItemCount)
								.TransitionTo(CheckoutState.CartItemsGreaterThanMaximumItemCount),

							TransitionBuilder
								.If(Guards.RecurringScheduleMismatchOnItems)
								.TransitionTo(CheckoutState.RecurringScheduleMismatchOnItems),

							TransitionBuilder
								.Always()
								.TransitionTo(CheckoutState.Account_Validating),
						}
					},
					{
						CheckoutState.Account_Validating,
						new Transition[]
						{
							TransitionBuilder
								.If(Guards.CustomerAccountRequired)
								.TransitionTo(CheckoutState.CustomerAccountRequired)
								.Then(
									Triggers.UpdateAccount(CheckoutStageStatusExtensions.UpdateAvailable, false),
									Triggers.UpdateAccount(CheckoutStageStatusExtensions.UpdateFulfilled, false)),

							TransitionBuilder
								.Always()
								.TransitionTo(CheckoutState.PaymentMethod_Cleaning)
								.Then(
									Triggers.UpdateAccount(CheckoutStageStatusExtensions.UpdateAvailable, true),
									Triggers.UpdateAccount(CheckoutStageStatusExtensions.UpdateFulfilled, true)),
						}
					},
					{
						CheckoutState.PaymentMethod_Cleaning,
						new Transition[]
						{
							// If a payment method is selected but no details are entered, then deselect the payment method
							TransitionBuilder
								.If(
									Guards.PaymentMethodIsCreditCard,
									Guards.Not(Guards.PaymentMethodIsOffsite),
                                    Guards.CreditCardDetailsMissing)
								.TransitionTo(CheckoutState.PaymentMethod_Validating)
								.Then(Triggers.ClearSelectedPaymentMethod),

							TransitionBuilder
								.If(
									Guards.PaymentMethodIsPurchaseOrder,
									Guards.PurchaseOrderDetailsMissing)
								.TransitionTo(CheckoutState.PaymentMethod_Validating)
								.Then(Triggers.ClearSelectedPaymentMethod),

							TransitionBuilder
								.Always()
								.TransitionTo(CheckoutState.PaymentMethod_Validating),
						}
					},
					{
						CheckoutState.PaymentMethod_Validating,
						new Transition[]
						{
							TransitionBuilder
								.If(Guards.PaymentMethodRequired,
									Guards.Not(Guards.PaymentMethodPresent))
								.TransitionTo(CheckoutState.PaymentMethodRequired)
								.Then(Triggers.UpdatePaymentMethod(CheckoutStageStatusExtensions.UpdateFulfilled, false)),

							TransitionBuilder
								.If(Guards.PaymentMethodIsOffsite)
								.TransitionTo(CheckoutState.PaymentMethod_Valid),

							TransitionBuilder
								.If(Guards.PaymentMethodIsMicroPay)
								.TransitionTo(CheckoutState.PaymentMethod_ValidatingMicropayDetails),

							TransitionBuilder
								.If(Guards.PaymentMethodIsPayPalExpress)
								.TransitionTo(CheckoutState.PaymentMethod_ValidatingPayPalExpressDetails),

							TransitionBuilder
								.If(Guards.PaymentMethodIsAmazonPayments)
								.TransitionTo(CheckoutState.PaymentMethod_ValidatingAmazonPaymentsDetails),

							TransitionBuilder
								.If(Guards.PaymentMethodIsCreditCard)
								.TransitionTo(CheckoutState.PaymentMethod_ValidatingCreditCardDetails),

							TransitionBuilder
								.If(Guards.PaymentMethodIsPurchaseOrder)
								.TransitionTo(CheckoutState.PaymentMethod_ValidatingPurchaseOrderDetails),

							TransitionBuilder
								.If(Guards.Always)
								.TransitionTo(CheckoutState.PaymentMethod_Valid),
						}
					},
					{
						CheckoutState.PaymentMethod_ValidatingMicropayDetails,
						new Transition[]
						{
							TransitionBuilder
								.If(Guards.MicroPayBalanceIsInsufficient)
								.TransitionTo(CheckoutState.MicroPayBalanceIsInsufficient)
								.Then(Triggers.UpdatePaymentMethod(CheckoutStageStatusExtensions.UpdateFulfilled, false)),

							TransitionBuilder
								.If(Guards.Always)
								.TransitionTo(CheckoutState.PaymentMethod_Valid),
						}
					},
					{
						CheckoutState.PaymentMethod_ValidatingPayPalExpressDetails,
						new Transition[]
						{
							TransitionBuilder
								.If(Guards.Always)
								.TransitionTo(CheckoutState.PaymentMethod_Valid),
						}
					},
					{
						CheckoutState.PaymentMethod_ValidatingAmazonPaymentsDetails,
						new Transition[]
						{
							TransitionBuilder
								.If(Guards.AmazonPaymentsDetailsMissing)
								.TransitionTo(CheckoutState.AmazonPaymentsDetailsRequired)
								.Then(
									Triggers.UpdatePaymentMethod(CheckoutStageStatusExtensions.UpdateAvailable, false),
									Triggers.UpdatePaymentMethod(CheckoutStageStatusExtensions.UpdateFulfilled, false)),

							TransitionBuilder
								.If(Guards.Always)
								.TransitionTo(CheckoutState.PaymentMethod_Valid),
						}
					},
					{
						CheckoutState.PaymentMethod_ValidatingCreditCardDetails,
						new Transition[]
						{
							TransitionBuilder
								.If(Guards.CreditCardDetailsMissing)
								.TransitionTo(CheckoutState.CreditCardDetailsRequired)
								.Then(
									Triggers.UpdatePaymentMethod(CheckoutStageStatusExtensions.UpdateAvailable, false),
									Triggers.UpdatePaymentMethod(CheckoutStageStatusExtensions.UpdateFulfilled, false)),

							TransitionBuilder
								.If(Guards.Always)
								.TransitionTo(CheckoutState.PaymentMethod_Valid),
						}
					},
					{
						CheckoutState.PaymentMethod_ValidatingPurchaseOrderDetails,
						new Transition[]
						{
							TransitionBuilder
								.If(Guards.PurchaseOrderDetailsMissing)
								.TransitionTo(CheckoutState.PurchaseOrderDetailsRequired)
								.Then(
									Triggers.UpdatePaymentMethod(CheckoutStageStatusExtensions.UpdateAvailable, false),
									Triggers.UpdatePaymentMethod(CheckoutStageStatusExtensions.UpdateFulfilled, false)),

							TransitionBuilder
								.If(Guards.Always)
								.TransitionTo(CheckoutState.PaymentMethod_Valid),
						}
					},
					{
						CheckoutState.PaymentMethod_Valid,
						new Transition[]
						{
							TransitionBuilder
								.If(Guards.Always)
								.TransitionTo(CheckoutState.BillingAddress_Validating)
								.Then(Triggers.UpdatePaymentMethod(CheckoutStageStatusExtensions.UpdateFulfilled, true)),
						}
					},
					{
						CheckoutState.BillingAddress_Validating,
						new Transition[]
						{
							TransitionBuilder
								.If(Guards.BillingAddressRequired,
									Guards.Not(Guards.BillingAddressPresent))
								.TransitionTo(CheckoutState.BillingAddressRequired)
								.Then(Triggers.UpdateBillingAddress(CheckoutStageStatusExtensions.UpdateFulfilled, false)),

							TransitionBuilder
								.If(Guards.Always)
								.TransitionTo(CheckoutState.BillingAddress_Valid),
						}
					},
					{
						CheckoutState.BillingAddress_Valid,
						new Transition[]
						{
							TransitionBuilder
								.If(Guards.Always)
								.TransitionTo(CheckoutState.ShippingAddress_Validating)
								.Then(Triggers.UpdateBillingAddress(CheckoutStageStatusExtensions.UpdateFulfilled, true)),
						}
					},
					{
						CheckoutState.ShippingAddress_Validating,
						new Transition[]
						{
							TransitionBuilder
								.If(Guards.ShippingDisabled)
								.TransitionTo(CheckoutState.ShippingAddress_Valid)
								.Then(Triggers.UpdateShippingAddress(CheckoutStageStatusExtensions.UpdateDisabled, true)),

							TransitionBuilder
								.If(Guards.ShippingAddressRequired,
									Guards.Not(Guards.ShippingAddressPresent))
								.TransitionTo(CheckoutState.ShippingAddressRequired)
								.Then(Triggers.UpdateShippingAddress(CheckoutStageStatusExtensions.UpdateFulfilled, false)),

							TransitionBuilder
								.If(Guards.Always)
								.TransitionTo(CheckoutState.ShippingAddress_Valid),
						}
					},
					{
						CheckoutState.ShippingAddress_Valid,
						new Transition[]
						{
							TransitionBuilder
								.If(Guards.Always)
								.TransitionTo(CheckoutState.ShippingMethod_Validating)
								.Then(Triggers.UpdateShippingAddress(CheckoutStageStatusExtensions.UpdateFulfilled, true)),
						}
					},
					{
						CheckoutState.ShippingMethod_Validating,
						new Transition[]
						{
							TransitionBuilder
								.If(Guards.ShippingDisabled)
								.TransitionTo(CheckoutState.ShippingMethod_Valid)
								.Then(Triggers.UpdateShippingMethod(CheckoutStageStatusExtensions.UpdateDisabled, true)),

							TransitionBuilder
								.If(Guards.Not(Guards.ShippingMethodRequired))
								.TransitionTo(CheckoutState.ShippingMethod_Valid)
								.Then(Triggers.UpdateShippingMethod(CheckoutStageStatusExtensions.UpdateRequired, false)),

							TransitionBuilder
								.If(Guards.ShippingMethodRequired,
									Guards.Not(Guards.ShippingMethodPresent))
								.TransitionTo(CheckoutState.ShippingMethodRequired)
								.Then(Triggers.UpdateShippingMethod(CheckoutStageStatusExtensions.UpdateFulfilled, false)),

							TransitionBuilder
								.If(Guards.ShippingMethodPresent,
									Guards.Not(Guards.ShippingMethodIsValid))
								.TransitionTo(CheckoutState.ShippingMethodRequired)
								.Then(Triggers.UpdateShippingMethod(CheckoutStageStatusExtensions.UpdateFulfilled, false)),

							TransitionBuilder
								.Always()
								.TransitionTo(CheckoutState.ShippingMethod_Valid),
						}
					},
					{
						CheckoutState.ShippingMethod_Valid,
						new Transition[]
						{
							TransitionBuilder
								.Always()
								.TransitionTo(CheckoutState.GiftCardSetup_Validating)
								.Then(Triggers.UpdateShippingMethod(CheckoutStageStatusExtensions.UpdateFulfilled, true)),
						}
					},
					{
						CheckoutState.GiftCardSetup_Validating,
						new Transition[]
						{
							TransitionBuilder
								.If(Guards.CartContainsGiftCard,
									Guards.Not(Guards.GiftCardSetupComplete))
								.TransitionTo(CheckoutState.GiftCardRequiresSetup)
								.Then(Triggers.UpdateGiftCardSetup(CheckoutStageStatusExtensions.UpdateFulfilled, false)),

							TransitionBuilder
								.Always()
								.TransitionTo(CheckoutState.Checkout_Validating)
								.Then(Triggers.UpdateGiftCardSetup(CheckoutStageStatusExtensions.UpdateFulfilled, true)),
						}
					},
					{
						CheckoutState.Checkout_Validating,
						new Transition[]
						{
							TransitionBuilder
								.If(Guards.ShippingAddressMustMatchBillingAddress,
									Guards.ShippingAddressDoesNotMatchBillingAddress)
								.TransitionTo(CheckoutState.ShippingAddressDoesNotMatchBillingAddress),

							TransitionBuilder
								.If(Guards.RequireCustomerOver13,
									Guards.CustomerIsNotOver13)
								.TransitionTo(CheckoutState.CustomerIsNotOver13),

							TransitionBuilder
								.If(Guards.TermsAndConditionsRequired,
									Guards.TermsAndConditionsNotAccepted)
								.TransitionTo(CheckoutState.TermsAndConditionsRequired),

							TransitionBuilder
								.Always()
								.TransitionTo(CheckoutState.Valid),
						}
					},
					{
						CheckoutState.Valid,
						new Transition[]
						{ }
					},
				};
		}

		public CheckoutEvaluationResult EvaluateCheckout(CheckoutConfiguration configuration, PersistedCheckoutContext persistedCheckoutContext, CheckoutSelectionContext checkoutSelectionContext, Customer customer, int storeId, CartContext cartContext)
		{
			// Build the evaluation context
			var stateMachineContext = new StateMachineContext(
				customer: customer,
				storeId: storeId,
				shoppingCart: cartContext.Cart,
				configuration: configuration,
				selections: checkoutSelectionContext);

			// Run the state machine
			var stateEvaluationResult = EvaluateStateMachine(stateMachineContext);

			// Put together a result
			var checkoutEvaluationResult = new CheckoutEvaluationResult(
				state: stateEvaluationResult.State,
				checkoutStageContext: stateEvaluationResult.CheckoutStageContext,
				selections: stateEvaluationResult.Selections);

			return checkoutEvaluationResult;
		}

		StateEvaluationResult EvaluateStateMachine(StateMachineContext context)
		{
			// Set up the initial state
			var previousResult = new StateEvaluationResult(
				state: CheckoutState.Start,
				checkoutStageContext: new CheckoutStageContext(
					account: new CheckoutStageStatus(
						required: Guards.CustomerAccountRequired(context),
						available: null,
						fulfilled: null,
						disabled: null),
					paymentMethod: new CheckoutStageStatus(
						required: Guards.PaymentMethodRequired(context),
						available: Guards.PaymentMethodPresent(context),
						fulfilled: null,
						disabled: null),
					billingAddress: new CheckoutStageStatus(
						required: Guards.BillingAddressRequired(context),
						available: Guards.BillingAddressPresent(context),
						fulfilled: null,
						disabled: null),
					shippingAddress: new CheckoutStageStatus(
						required: Guards.ShippingAddressRequired(context),
						available: Guards.ShippingAddressPresent(context),
						fulfilled: null,
						disabled: Guards.ShippingDisabled(context)),
					shippingMethod: new CheckoutStageStatus(
						required: Guards.ShippingMethodRequired(context),
						available: Guards.ShippingMethodPresent(context),
						fulfilled: null,
						disabled: Guards.ShippingDisabled(context)),
					giftCardSetup: new CheckoutStageStatus(
						required: Guards.CartContainsGiftCard(context),
						available: null,
						fulfilled: Guards.GiftCardSetupComplete(context),
						disabled: null),
					placeOrderButton: new CheckoutStageStatus(
						required: null,
						available: null,
						fulfilled: true,
						disabled: null)),
				selections: context.Selections);

			// Loop until we find an exit condition
			while(true)
			{
				// Find a transition from the current state where all guards pass
				var passedTransition = StateMachine.ContainsKey(previousResult.State)
					? StateMachine[previousResult.State]
						.Where(transition => transition
							.Guards
							.Select(guard => guard(context))
							.All(guardResult => guardResult == true))
						.FirstOrDefault()
					: null;

				// If there's nothing to transition to, we've hit a terminal state and need to return
				if(passedTransition == null)
					return previousResult;

				// Build up the next state
				var currentResult = new StateEvaluationResult(
					state: passedTransition.Target,
					checkoutStageContext: previousResult.CheckoutStageContext,
					selections: previousResult.Selections);

				// Run any triggers on the target state
				foreach(var trigger in passedTransition.Triggers)
					currentResult = trigger(context, currentResult);

				// If we're just looping back to the same state or we've hit the final state, we're done
				if(currentResult.State == previousResult.State || currentResult.State == CheckoutState.Valid)
					return currentResult;

				// Set up the current state as the previous state and loop
				previousResult = currentResult;
			}
		}

		/// <summary>
		/// A <see cref="Trigger"/> is an effect that is executed when one state transitions to another.
		/// The checkout engine is designed to run without side effects, so triggers are not meant to
		/// change external state. Instead, we use them to track additional internal state;
		/// specifically, we update the boolean flags on the checkout stages.
		/// </summary>
		public static class Triggers
		{
			static Trigger UpdateStatusValue(Func<CheckoutStageContext, CheckoutStageContext> contextApplicator)
			{
				return (context, previousResult) => new StateEvaluationResult(
					state: previousResult.State,
					checkoutStageContext: contextApplicator(previousResult.CheckoutStageContext),
					selections: previousResult.Selections);
			}

			public static Trigger UpdatePaymentMethod(Func<CheckoutStageStatus, bool?, CheckoutStageStatus> valueApplicator, bool? value)
			{
				return UpdateStatusValue(context => context.UpdatePaymentMethod(valueApplicator(context.PaymentMethod, value)));
			}

			public static Trigger UpdateBillingAddress(Func<CheckoutStageStatus, bool?, CheckoutStageStatus> valueApplicator, bool? value)
			{
				return UpdateStatusValue(context => context.UpdateBillingAddress(valueApplicator(context.BillingAddress, value)));
			}

			public static Trigger UpdateShippingMethod(Func<CheckoutStageStatus, bool?, CheckoutStageStatus> valueApplicator, bool? value)
			{
				return UpdateStatusValue(context => context.UpdateShippingMethod(valueApplicator(context.ShippingMethod, value)));
			}

			public static Trigger UpdateShippingAddress(Func<CheckoutStageStatus, bool?, CheckoutStageStatus> valueApplicator, bool? value)
			{
				return UpdateStatusValue(context => context.UpdateShippingAddress(valueApplicator(context.ShippingAddress, value)));
			}

			public static Trigger UpdateGiftCardSetup(Func<CheckoutStageStatus, bool?, CheckoutStageStatus> valueApplicator, bool? value)
			{
				return UpdateStatusValue(context => context.UpdateGiftCardSetup(valueApplicator(context.GiftCardSetup, value)));
			}

			public static Trigger UpdateAccount(Func<CheckoutStageStatus, bool?, CheckoutStageStatus> valueApplicator, bool? value)
			{
				return UpdateStatusValue(context => context.UpdateAccount(valueApplicator(context.Account, value)));
			}

			public static StateEvaluationResult ClearSelectedPaymentMethod(StateMachineContext context, StateEvaluationResult previousResult)
			{
				// Copy everything but the selected payment method
				return new StateEvaluationResult(
					state: previousResult.State,
					checkoutStageContext: previousResult.CheckoutStageContext,
					selections: new CheckoutSelectionContext(
						selectedPaymentMethod: null,
						selectedBillingAddress: context.Selections.SelectedBillingAddress,
						selectedShippingAddress: context.Selections.SelectedShippingAddress,
						selectedShippingMethodId: context.Selections.SelectedShippingMethodId,
						creditCard: context.Selections.CreditCard,
						payPalExpress: context.Selections.PayPalExpress,
						amazonPayments: context.Selections.AmazonPayments,
						purchaseOrder: context.Selections.PurchaseOrder,
						braintree: context.Selections.Braintree,
						termsAndConditionsAccepted: context.Selections.TermsAndConditionsAccepted,
						over13Checked: context.Selections.Over13Checked,
						email: context.Selections.Email));
			}
		}

		public delegate StateEvaluationResult Trigger(StateMachineContext context, StateEvaluationResult previousResult);

		public class Transition
		{
			public readonly CheckoutState Target;
			public readonly IEnumerable<Guard> Guards;
			public readonly IEnumerable<Trigger> Triggers;

			public Transition(CheckoutState target, IEnumerable<Guard> guards = null, IEnumerable<Trigger> triggers = null)
			{
				Target = target;
				Guards = guards ?? Enumerable.Empty<Guard>();
				Triggers = triggers ?? Enumerable.Empty<Trigger>();
			}
		}

		public class StateMachineContext
		{
			public readonly Customer Customer;
			public readonly int StoreId;
			public readonly ShoppingCart ShoppingCart;
			public readonly CheckoutConfiguration Configuration;
			public readonly CheckoutSelectionContext Selections;

			public StateMachineContext(Customer customer, int storeId, ShoppingCart shoppingCart, CheckoutConfiguration configuration, CheckoutSelectionContext selections)
			{
				Customer = customer;
				StoreId = storeId;
				ShoppingCart = shoppingCart;
				Configuration = configuration;
				Selections = selections;
			}
		}

		public class StateEvaluationResult
		{
			public readonly CheckoutState State;
			public readonly CheckoutStageContext CheckoutStageContext;
			public readonly CheckoutSelectionContext Selections;

			public StateEvaluationResult(CheckoutState state, CheckoutStageContext checkoutStageContext, CheckoutSelectionContext selections)
			{
				State = state;
				CheckoutStageContext = checkoutStageContext;
				Selections = selections;
			}
		}
	}
}
