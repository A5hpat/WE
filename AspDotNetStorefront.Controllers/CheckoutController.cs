// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using AspDotNetStorefront.Caching.ObjectCaching;
using AspDotNetStorefront.Checkout;
using AspDotNetStorefront.Checkout.Engine;
using AspDotNetStorefront.Filters;
using AspDotNetStorefront.Models;
using AspDotNetStorefront.Models.Converter;
using AspDotNetStorefront.Routing;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways;
using AspDotNetStorefrontGateways.Processors;

namespace AspDotNetStorefront.Controllers
{
	[SecureAccessFilter(forceHttps: true)]
	[RequireCustomerRecordFilter]
	public class CheckoutController : Controller
	{
		const string ShowCheckoutStageErrors = "ShowCheckoutStageErrors";

		readonly AddressViewModelConverter AddressViewModelConverter;
		readonly IEnumerable<IAppliedPaymentMethodCleanup> AppliedPaymentMethodCleanupProviders;
		readonly ICachedShoppingCartProvider CachedShoppingCartProvider;
		readonly ICartContextProvider CartContextProvider;
		readonly ICheckoutConfigurationProvider CheckoutConfigurationProvider;
		readonly CheckoutEngine CheckoutEngine;
		readonly ICheckoutSelectionProvider CheckoutSelectionProvider;
		readonly NoticeProvider NoticeProvider;
		readonly IPaymentMethodInfoProvider PaymentMethodInfoProvider;
		readonly IPaymentOptionProvider PaymentOptionProvider;
		readonly IPersistedCheckoutContextProvider PersistedCheckoutContextProvider;

		public CheckoutController(
			AddressViewModelConverter addressViewModelConverter,
			IEnumerable<IAppliedPaymentMethodCleanup> appliedPaymentMethodCleanupProviders,
			ICachedShoppingCartProvider cachedShoppingCartProvider,
			ICartContextProvider cartContextProvider,
			ICheckoutConfigurationProvider checkoutConfigurationProvider,
			CheckoutEngine checkoutEngine,
			ICheckoutSelectionProvider checkoutSelectionProvider,
			NoticeProvider noticeProvider,
			IPaymentMethodInfoProvider paymentMethodInfoProvider,
			IPaymentOptionProvider paymentOptionProvider,
			IPersistedCheckoutContextProvider persistedCheckoutContextProvider)
		{
			AddressViewModelConverter = addressViewModelConverter;
			AppliedPaymentMethodCleanupProviders = appliedPaymentMethodCleanupProviders;
			CachedShoppingCartProvider = cachedShoppingCartProvider;
			CartContextProvider = cartContextProvider;
			CheckoutConfigurationProvider = checkoutConfigurationProvider;
			CheckoutEngine = checkoutEngine;
			CheckoutSelectionProvider = checkoutSelectionProvider;
			NoticeProvider = noticeProvider;
			PaymentMethodInfoProvider = paymentMethodInfoProvider;
			PaymentOptionProvider = paymentOptionProvider;
			PersistedCheckoutContextProvider = persistedCheckoutContextProvider;
		}

		[PageTypeFilter(PageTypes.Checkout)]
		[ImportModelStateFromTempData]
		public ActionResult Index(string errorMessage, string returnUrl, bool? showErrors)
		{
			PushErrorMessages(errorMessage);

			var defaultReturnUrl = "";
			if(string.IsNullOrEmpty(defaultReturnUrl))
				defaultReturnUrl = Url.Action(ActionNames.Index, ControllerNames.Home);

			var safeReturnUrl = Url.MakeSafeReturnUrl(returnUrl, defaultReturnUrl);

			// Get the current checkout state
			var customer = HttpContext.GetCustomer();

			// We need to validate that a (potentially previously) selected PM is still available and valid (as any number of site settings/configs/customerLevel values could have changed)
			if(!string.IsNullOrEmpty(customer.RequestedPaymentMethod)
					&& !PaymentOptionProvider.PaymentMethodSelectionIsValid(customer.RequestedPaymentMethod, customer))
				customer.UpdateCustomer(requestedPaymentMethod: string.Empty);

			var storeId = AppLogic.StoreID();
			var cart = CachedShoppingCartProvider.Get(customer, CartTypeEnum.ShoppingCart, AppLogic.StoreID());

			// Make sure there's a GiftCard record for every gift card product in the cart
			CreateGiftCards(customer, cart);

			var checkoutConfiguration = CheckoutConfigurationProvider.GetCheckoutConfiguration();

			var selectedPaymentMethod = PaymentMethodInfoProvider
				.GetPaymentMethodInfo(
					paymentMethod: customer.RequestedPaymentMethod,
					gateway: AppLogic.ActivePaymentGatewayCleaned());

			var persistedCheckoutContext = PersistedCheckoutContextProvider
				.LoadCheckoutContext(customer);

			var cartContext = CartContextProvider
				.LoadCartContext(
					customer: customer,
					configuration: checkoutConfiguration,
					persistedCheckoutContext: persistedCheckoutContext,
					selectedPaymentMethod: selectedPaymentMethod);

			var checkoutSelectionContext = CheckoutSelectionProvider
				.GetCheckoutSelection(
					customer: customer,
					persistedCheckoutContext: persistedCheckoutContext,
					selectedPaymentMethod: selectedPaymentMethod);

			var result = CheckoutEngine
				.EvaluateCheckout(
					customer: customer,
					configuration: checkoutConfiguration,
					persistedCheckoutContext: persistedCheckoutContext,
					checkoutSelectionContext: checkoutSelectionContext,
					storeId: storeId,
					cartContext: cartContext);

			var updated = CheckoutSelectionProvider.ApplyCheckoutSelections(customer, result.Selections);

			var action = GetActionForState(result.State);

			// Perform the resulting action
			switch(action)
			{
				case CheckoutAction.Error:
					if(result.State == CheckoutState.CustomerIsNotOver13)
						break;
					else if(result.State == CheckoutState.TermsAndConditionsRequired)
						break;
					else if(result.State == CheckoutState.ShippingAddressDoesNotMatchBillingAddress)
						NoticeProvider.PushNotice("This store can only ship to the billing address", NoticeType.Failure);
					else if(result.State == CheckoutState.MicroPayBalanceIsInsufficient)
						NoticeProvider.PushNotice(
							string.Format("Your MicroPay balance is insufficient for purchasing this order. <br />(<b>Your MicroPay Balance is {0}</b>)",
								Localization.CurrencyStringForDisplayWithExchangeRate(updated.Customer.MicroPayBalance, updated.Customer.CurrencySetting)),
							NoticeType.Failure);
					else if(result.State == CheckoutState.SubtotalDoesNotMeetMinimumAmount)
						NoticeProvider.PushNotice(
							string.Format("The minimum allowed order amount is {0} Please add additional items to your cart, or increase item quantities.",
								updated.Customer.CurrencyString(AppLogic.AppConfigNativeDecimal("CartMinOrderAmount"))),
							NoticeType.Failure);
					else if(result.State == CheckoutState.CartItemsLessThanMinimumItemCount)
						NoticeProvider.PushNotice(
							string.Format("Please make sure you have ordered at least {0} items - any {1} items from our site - before checking out.",
								AppLogic.AppConfigNativeInt("MinCartItemsBeforeCheckout"),
								AppLogic.AppConfigNativeInt("MinCartItemsBeforeCheckout")),
							NoticeType.Failure);
					else if(result.State == CheckoutState.CartItemsGreaterThanMaximumItemCount)
						NoticeProvider.PushNotice(
							string.Format("We allow a maximum of {0} items per order.  Please remove some products from your cart before beginning checkout.",
								AppLogic.AppConfigNativeInt("MaxCartItemsBeforeCheckout")),
							NoticeType.Failure);
					else if(result.State == CheckoutState.RecurringScheduleMismatchOnItems)
						NoticeProvider.PushNotice("Your cart has Auto-Ship items with conflicting shipment schedules. Only one Auto-Ship schedule is allowed per order.", NoticeType.Failure);
					else
						NoticeProvider.PushNotice(result.State.ToString(), NoticeType.Failure);
					break;

				case CheckoutAction.Empty:
					return View("EmptyCart");
			}

			return RenderIndexView(
				checkoutStageContext: result.CheckoutStageContext,
				persistedCheckoutContext: updated.PersistedCheckoutContext,
				selectedPaymentMethod: updated.SelectedPaymentMethod,
				customer: updated.Customer,
				termsAndConditionsAccepted: result.Selections.TermsAndConditionsAccepted,
				returnUrl: safeReturnUrl,
				showCheckoutStageErrors: showErrors ?? false);
		}

		ActionResult RenderIndexView(CheckoutStageContext checkoutStageContext, PersistedCheckoutContext persistedCheckoutContext, PaymentMethodInfo selectedPaymentMethod, Customer customer, bool termsAndConditionsAccepted, string returnUrl, bool showCheckoutStageErrors)
		{
			var cart = CachedShoppingCartProvider.Get(customer, CartTypeEnum.ShoppingCart, AppLogic.StoreID());

			// Build a model and render it
			var billingAddressViewModel = customer.PrimaryBillingAddress != null
				? AddressViewModelConverter.ConvertToAddressViewModel(customer.PrimaryBillingAddress, customer)
				: null;

			var shippingAddressViewModel = customer.PrimaryShippingAddress != null
				? AddressViewModelConverter.ConvertToAddressViewModel(customer.PrimaryShippingAddress, customer)
				: null;

			var cartPageAd = new PayPalAd(PayPalAd.TargetPage.Cart);
			var gatewayIsTwoCheckout = AppLogic.ActivePaymentGatewayCleaned() == Gateway.ro_GWTWOCHECKOUT 
				&& selectedPaymentMethod != null 
				&& selectedPaymentMethod.Name == AppLogic.ro_PMCreditCard;

			var shippingEnabled =
				checkoutStageContext.ShippingAddress.Disabled != true
				|| checkoutStageContext.ShippingMethod.Disabled != true;

			var shippingInfoRequired =
				checkoutStageContext.ShippingAddress.Required == true
				|| checkoutStageContext.ShippingMethod.Required == true;

			var displayBillingSection = checkoutStageContext.BillingAddress.Disabled != true;

            // nal
			//var allowShipToDifferentThanBillTo = AppLogic.AppConfigBool("AllowShipToDifferentThanBillTo");
			var allowShipToDifferentThanBillTo = true;

			//  Display the shipping section only if shipping is enabled on the site. If so, than if shipping can be different
			//  than billing, we need to show the form. If shipping and billing can be different or the current payment
			//  option doesn't care about billing, then show just the shipping section and that address will be used for
			//  both shipping and billing.
			var displayShippingSection = shippingEnabled && allowShipToDifferentThanBillTo;

			var checkoutIsOffsiteOnly = PaymentOptionProvider.CheckoutIsOffsiteOnly(customer, cart);
			var paymentMethodStageState = ConvertStageStatusToDisplayState(checkoutStageContext.PaymentMethod, showCheckoutStageErrors);

			var model = new CheckoutIndexViewModel(
				selectedPaymentMethod: selectedPaymentMethod,
				selectedBillingAddress: billingAddressViewModel,
				selectedShippingAddress: shippingAddressViewModel,
				checkoutButtonDisabled: checkoutStageContext.PlaceOrderButton.Fulfilled != true,
				showOver13Required: Over13Required(customer),
				showOkToEmail: AppLogic.AppConfigBool("Checkout.ShowOkToEmailOnCheckout"),
				showTermsAndConditions: AppLogic.AppConfigBool("RequireTermsAndConditionsAtCheckout"),
				displayGiftCardSetup: checkoutStageContext.GiftCardSetup.Required == true,
				showOrderOptions: cart.AllOrderOptions.Any(),
                // nal
				//showOrderNotes: !AppLogic.AppConfigBool("DisallowOrderNotes"),
				showOrderNotes: true,
				showRealTimeShippingInfo: AppLogic.AppConfigBool("RTShipping.DumpDebugXmlOnCheckout") && (customer.IsAdminUser || customer.IsAdminSuperUser),
				allowShipToDifferentThanBillTo: allowShipToDifferentThanBillTo,
				displayShippingSection: displayShippingSection,
				displayBillingSection: displayBillingSection,
				shippingInfoIsRequired: shippingInfoRequired,
				displayTwoCheckoutText: gatewayIsTwoCheckout,
				displayContinueOffsite: selectedPaymentMethod != null && selectedPaymentMethod.Location == PaymentMethodLocation.Offsite,
                // nal
				//showPromotions: AppLogic.AppConfigBool("Promotions.Enabled"),
                showPromotions: true,
                // nal
				//showGiftCards: AppLogic.AppConfigBool("GiftCards.Enabled"),
                showGiftCards: true,
				giftCardCoversTotal: cart.GiftCardCoversTotal(),
				checkoutIsOffsiteOnly: checkoutIsOffsiteOnly,
				pageTitle: "Secure Checkout",
				payPalBanner: cartPageAd.Show ? cartPageAd.ImageScript : null,
				accountStageState: ConvertStageStatusToDisplayState(checkoutStageContext.Account, showCheckoutStageErrors),
                // nal
				continueShoppingUrl: Url.Content(returnUrl),
				offsiteCheckoutError: checkoutIsOffsiteOnly && paymentMethodStageState == CheckoutStageDisplayState.Failing
					? "Please choose a payment method"
                    : string.Empty,
				paymentMethodStageState: paymentMethodStageState,
				billingAddressStageState: ConvertStageStatusToDisplayState(checkoutStageContext.BillingAddress, showCheckoutStageErrors),
				shippingAddressStageState: ConvertStageStatusToDisplayState(checkoutStageContext.ShippingAddress, showCheckoutStageErrors),
				shippingMethodStageState: ConvertStageStatusToDisplayState(checkoutStageContext.ShippingMethod, showCheckoutStageErrors),
				giftCardSetupStageState: ConvertStageStatusToDisplayState(checkoutStageContext.GiftCardSetup, showCheckoutStageErrors))
			{
				Over13Selected = persistedCheckoutContext.Over13Checked,
				OkToEmailSelected = customer.OKToEMail,
				TermsAndConditionsAccepted = termsAndConditionsAccepted
			};

			return View(model);
		}

		CheckoutStageDisplayState ConvertStageStatusToDisplayState(CheckoutStageStatus status, bool showCheckoutStageErrors)
		{
			if(status.Required == false)
				return CheckoutStageDisplayState.Disabled;

			if(status.Fulfilled == false && showCheckoutStageErrors)
				return CheckoutStageDisplayState.Failing;

			if(status.Available == true || status.Fulfilled == true)
				return CheckoutStageDisplayState.Passing;

			if(showCheckoutStageErrors)
				return CheckoutStageDisplayState.Failing;

			return CheckoutStageDisplayState.Unknown;
		}

		CheckoutAction GetActionForState(CheckoutState state)
		{
			switch(state)
			{
				case CheckoutState.SubtotalDoesNotMeetMinimumAmount:
				case CheckoutState.CartItemsLessThanMinimumItemCount:
				case CheckoutState.CartItemsGreaterThanMaximumItemCount:
				case CheckoutState.RecurringScheduleMismatchOnItems:
				case CheckoutState.ShippingAddressDoesNotMatchBillingAddress:
				case CheckoutState.MicroPayBalanceIsInsufficient:
				case CheckoutState.CustomerIsNotOver13:
				case CheckoutState.TermsAndConditionsRequired:
					return CheckoutAction.Error;

				case CheckoutState.ShoppingCartIsEmpty:
					return CheckoutAction.Empty;

				case CheckoutState.Valid:
					return CheckoutAction.Complete;

				default:
					return CheckoutAction.None;
			}
		}

		//This needs to be a post so that we can accept an AntiForgeryToken.
		[HttpPost, ValidateAntiForgeryToken, ExportModelStateToTempData]
		public ActionResult PlaceOrder(CheckoutIndexPostModel model)
		{
			// Get the current checkout state
			var customer = HttpContext.GetCustomer();
			var storeId = AppLogic.StoreID();

			var checkoutConfiguration = CheckoutConfigurationProvider.GetCheckoutConfiguration();

			var selectedPaymentMethod = PaymentMethodInfoProvider
				.GetPaymentMethodInfo(
					paymentMethod: customer.RequestedPaymentMethod,
					gateway: AppLogic.ActivePaymentGatewayCleaned());

			// update checkboxes
			UpdateOver13(model.Over13Selected, customer);
			UpdateOkToEmail(model.OkToEmailSelected, customer);
			UpdateTermsAndConditions(model.TermsAndConditionsAccepted, customer);

			var persistedCheckoutContext = PersistedCheckoutContextProvider
				.LoadCheckoutContext(customer);

			UpdateCustomerEmail(persistedCheckoutContext.Email, customer);

			var cartContext = CartContextProvider
				.LoadCartContext(
					customer: customer,
					configuration: checkoutConfiguration,
					persistedCheckoutContext: persistedCheckoutContext,
					selectedPaymentMethod: selectedPaymentMethod);

			var checkoutSelectionContext = CheckoutSelectionProvider
				.GetCheckoutSelection(
					customer: customer,
					persistedCheckoutContext: persistedCheckoutContext,
					selectedPaymentMethod: selectedPaymentMethod);

			var result = CheckoutEngine
				.EvaluateCheckout(
					customer: customer,
					configuration: checkoutConfiguration,
					persistedCheckoutContext: persistedCheckoutContext,
					checkoutSelectionContext: checkoutSelectionContext,
					storeId: storeId,
					cartContext: cartContext);

			var action = GetActionForState(result.State);

			var checkoutEngineUpdates = CheckoutSelectionProvider.ApplyCheckoutSelections(customer, result.Selections);
			customer = checkoutEngineUpdates.Customer;
			persistedCheckoutContext = checkoutEngineUpdates.PersistedCheckoutContext;
			selectedPaymentMethod = checkoutEngineUpdates.SelectedPaymentMethod;

			if(action != CheckoutAction.Complete)
				NoticeProvider.PushNotice("Please complete the required areas below before you place your order", NoticeType.Failure);

			// Only place the order if checkout is complete.
			if(action != CheckoutAction.Complete)
				return RedirectToAction(ActionNames.Index, ControllerNames.Checkout, new RouteValueDictionary { { RouteDataKeys.ShowErrors, true } });

			var billingAddress = new Address();
			billingAddress.LoadByCustomer(customer.CustomerID, customer.PrimaryBillingAddressID, AddressTypes.Billing);

			//Save customer context to the 'permanent' places it needs to go
			SaveCustomerContextToDB(selectedPaymentMethod, persistedCheckoutContext, customer, billingAddress);

			//Reload the customer so it's got the new CustomerSession values
			customer = new Customer(customer.CustomerID);
			var cart = CachedShoppingCartProvider.Get(customer, CartTypeEnum.ShoppingCart, AppLogic.StoreID());

			var orderNumber = 0;
			var gatewayToUse = AppLogic.ActivePaymentGatewayCleaned();
			var paymentMethod = selectedPaymentMethod == null
				? null
				: selectedPaymentMethod.Name;
			var giftCardOrder = cart.GiftCardCoversTotal();

			if(selectedPaymentMethod == null)
			{
				orderNumber = AppLogic.GetNextOrderNumber();
				var status = Gateway.MakeOrder(string.Empty, AppLogic.TransactionMode(), cart, orderNumber, string.Empty, string.Empty, string.Empty, string.Empty);

				if(status != AppLogic.ro_OK)
				{
					NoticeProvider.PushNotice(status, NoticeType.Failure);
					return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
				}
			}
			else if(paymentMethod == AppLogic.ro_PMCreditCard)
			{
				//2checkout has own 3dsecure
				if(gatewayToUse == Gateway.ro_GWTWOCHECKOUT)
					return RedirectToAction(ActionNames.TwoCheckout, ControllerNames.TwoCheckout);

				//Set up some special info for Braintree
				if(gatewayToUse == Gateway.ro_GWBRAINTREE && !giftCardOrder)
				{
					customer.ThisCustomerSession[AppLogic.Braintree3dSecureKey] = persistedCheckoutContext.Braintree.ThreeDSecureApproved.ToString();
					customer.ThisCustomerSession[AppLogic.BraintreeNonceKey] = persistedCheckoutContext.Braintree.Nonce.ToString();
					customer.ThisCustomerSession[AppLogic.BraintreePaymentMethod] = persistedCheckoutContext.Braintree.PaymentMethod;
				}

				var status = string.Empty;
				orderNumber = AppLogic.GetNextOrderNumber();

				if(Cardinal.EnabledForCheckout(cart.Total(true), billingAddress.CardType)
					&& gatewayToUse != Gateway.ro_GWBRAINTREE)  //Braintree has its own native 3dSecure support
				{
					if(Cardinal.PreChargeLookupAndStoreSession(
						customer,
						orderNumber,
						cart.Total(true),
						billingAddress.CardNumber,
						billingAddress.CardExpirationMonth,
						billingAddress.CardExpirationYear)
						&& gatewayToUse != Gateway.ro_GWBRAINTREE)
					{
						return RedirectToAction(ActionNames.ThreeDSecure, ControllerNames.ThreeDSecure);
					}
					else
					{
						// user not enrolled or cardinal gateway returned error, so process card normally, using already created order #:
						var eciFlag = Cardinal.GetECIFlag(billingAddress.CardType);
						status = Gateway.MakeOrder(string.Empty, AppLogic.TransactionMode(), cart, orderNumber, string.Empty, eciFlag, string.Empty, string.Empty);

						CleanupPaymentMethod(new AppliedPaymentMethodCleanupContext(
							customer: customer,
							orderNumber: orderNumber,
							status: status,
							paymentMethod: paymentMethod));

						if(status != AppLogic.ro_OK)
						{
							NoticeProvider.PushNotice(status, NoticeType.Failure);
							return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
						}
						DB.ExecuteSQL("update orders set CardinalLookupResult=" + DB.SQuote(customer.ThisCustomerSession["Cardinal.LookupResult"]) + " where OrderNumber=" + orderNumber.ToString());
					}
				}
				else
				{
					status = Gateway.MakeOrder(string.Empty, AppLogic.TransactionMode(), cart, orderNumber, string.Empty, string.Empty, string.Empty, string.Empty);

					CleanupPaymentMethod(new AppliedPaymentMethodCleanupContext(
						customer: customer,
						orderNumber: orderNumber,
						status: status,
						paymentMethod: paymentMethod,
						gateway: gatewayToUse));

					if(status == AppLogic.ro_3DSecure)
					{ // If credit card is enrolled in a 3D Secure service (Verified by Visa, etc.)
						return RedirectToAction(ActionNames.ThreeDSecure, ControllerNames.ThreeDSecure);
					}
					if(status != AppLogic.ro_OK)
					{
						NoticeProvider.PushNotice(status, NoticeType.Failure);
						return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
					}
				}
			}
			else if(paymentMethod == AppLogic.ro_PMPayPalExpress
				|| paymentMethod == AppLogic.ro_PMPayPalExpressMark)
			{
				if(persistedCheckoutContext.PayPalExpress == null || string.IsNullOrEmpty(persistedCheckoutContext.PayPalExpress.Token))
				{
					NoticeProvider.PushNotice("The PaypalExpress checkout token has expired, please re-login to your PayPal account or checkout using a different method of payment.", NoticeType.Failure);
					return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
				}

				orderNumber = AppLogic.GetNextOrderNumber();

				var effectiveBillingAddress = new Address();
				effectiveBillingAddress.LoadByCustomer(customer.CustomerID, customer.PrimaryBillingAddressID, AddressTypes.Billing);
				effectiveBillingAddress.PaymentMethodLastUsed = paymentMethod;
				effectiveBillingAddress.CardNumber = string.Empty;
				effectiveBillingAddress.CardType = string.Empty;
				effectiveBillingAddress.CardExpirationMonth = string.Empty;
				effectiveBillingAddress.CardExpirationYear = string.Empty;
				effectiveBillingAddress.CardName = string.Empty;
				effectiveBillingAddress.CardStartDate = string.Empty;
				effectiveBillingAddress.CardIssueNumber = string.Empty;
				effectiveBillingAddress.UpdateDB();

				var transactionContext = new Dictionary<string, string>
				{
					{ "TENDER", "P" }
				};

				gatewayToUse = PayPalController.GetAppropriateExpressType() == ExpressAPIType.PayFlowPro
					? Gateway.ro_GWPAYFLOWPRO
					: string.Empty;

				var status = Gateway.MakeOrder(
					gatewayToUse,
					AppLogic.TransactionMode(),
					cart,
					orderNumber,
					persistedCheckoutContext.PayPalExpress.Token,
					persistedCheckoutContext.PayPalExpress.PayerId,
					persistedCheckoutContext.PayPalExpress.Token,
					string.Empty,
					transactionContext);

				CleanupPaymentMethod(new AppliedPaymentMethodCleanupContext(
					customer: customer,
					orderNumber: orderNumber,
					status: status,
					paymentMethod: selectedPaymentMethod.Name,
					gateway: gatewayToUse));

				if(status != AppLogic.ro_OK)
				{
					NoticeProvider.PushNotice(status, NoticeType.Failure);
					return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
				}
			}
			else if(paymentMethod == AppLogic.ro_PMPayPalEmbeddedCheckout)
			{
				var returnUrl = Url.Action(
					actionName: ActionNames.Ok,
					controllerName: ControllerNames.PayPalPaymentsAdvanced,
					routeValues: null,
					protocol: Uri.UriSchemeHttps);

				var errorUrl = Url.Action(
					actionName: ActionNames.Error,
					controllerName: ControllerNames.PayPalPaymentsAdvanced,
					routeValues: null,
					protocol: Uri.UriSchemeHttps);

				var cancelUrl = Url.Action(
					actionName: ActionNames.Index,
					controllerName: ControllerNames.Checkout,
					routeValues: null,
					protocol: Uri.UriSchemeHttps);

				var notifyUrl = Url.Action(
					actionName: ActionNames.Index,
					controllerName: ControllerNames.PayPalNotifications,
					routeValues: null,
					protocol: Uri.UriSchemeHttps);

				var silentPostUrl = Url.Action(
					actionName: ActionNames.Ok,
					controllerName: ControllerNames.PayPalPaymentsAdvanced,
					routeValues: null,
					protocol: Uri.UriSchemeHttps);

				var shippingAddress = customer.PrimaryShippingAddress ?? new Address();

				var response = PayFlowProController.GetFramedHostedCheckout(
					cart: cart,
					ShippingAddress: shippingAddress,
					returnUrl: returnUrl,
					errorUrl: errorUrl,
					cancelUrl: cancelUrl,
					notifyUrl: notifyUrl,
					silentPostUrl: silentPostUrl);

				if(response.Result != 0)
					throw new Exception("PayPal Payments Advanced is not configured properly.");

				Session["PayPalEmbeddedCheckoutSecureToken"] = response.SecureToken;
				Session["PayPalEmbeddedCheckoutSecureTokenId"] = response.SecureTokenID;

				var redirectUrl = response.GetRedirectUrl();
				return Redirect(redirectUrl);
			}
			else if(paymentMethod == AppLogic.ro_PMAmazonPayments
				|| paymentMethod == AppLogic.ro_PMPurchaseOrder
				|| paymentMethod == AppLogic.ro_PMRequestQuote
				|| paymentMethod == AppLogic.ro_PMCheckByMail
				|| paymentMethod == AppLogic.ro_PMCOD
				|| paymentMethod == AppLogic.ro_PMMicropay)
			{
				orderNumber = AppLogic.GetNextOrderNumber();
				var status = Gateway.MakeOrder(string.Empty, AppLogic.TransactionMode(), cart, orderNumber, string.Empty, string.Empty, string.Empty, string.Empty);

				CleanupPaymentMethod(new AppliedPaymentMethodCleanupContext(
					customer: customer,
					orderNumber: orderNumber,
					status: status,
					paymentMethod: selectedPaymentMethod.Name));

				if(status != AppLogic.ro_OK)
				{
					NoticeProvider.PushNotice(status, NoticeType.Failure);
					return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
				}
			}

			return RedirectToAction(
				ActionNames.Confirmation,
				ControllerNames.CheckoutConfirmation,
				new
				{
					orderNumber = orderNumber,
					paymentMethod = paymentMethod
				});
		}

		bool Over13Required(Customer customer)
		{
            // nal
			//return AppLogic.AppConfigBool("RequireOver13Checked") && !customer.IsOver13;
			return !customer.IsOver13;
		}

		void UpdateCustomerEmail(string email, Customer customer)
		{
			// Only update the customer if we have an email from the checkout process and an email hasn't already
			//  been set on the customer.
			if(string.IsNullOrEmpty(email) || !string.IsNullOrEmpty(customer.EMail))
				return;

			customer.UpdateCustomer(email: email);
		}

		void UpdateOver13(bool? over13Selected, Customer customer)
		{
			if(!over13Selected.HasValue)
				return;

			var checkoutContext = PersistedCheckoutContextProvider.LoadCheckoutContext(customer);
			var updatedCheckoutContext = new PersistedCheckoutContext(
				creditCard: checkoutContext.CreditCard,
				payPalExpress: checkoutContext.PayPalExpress,
				purchaseOrder: checkoutContext.PurchaseOrder,
				braintree: checkoutContext.Braintree,
				amazonPayments: checkoutContext.AmazonPayments,
				termsAndConditionsAccepted: checkoutContext.TermsAndConditionsAccepted,
				over13Checked: over13Selected.Value,
				shippingEstimateDetails: checkoutContext.ShippingEstimateDetails,
				offsiteRequiresBillingAddressId: checkoutContext.OffsiteRequiresBillingAddressId,
				offsiteRequiresShippingAddressId: checkoutContext.OffsiteRequiresShippingAddressId,
				email: checkoutContext.Email,
				selectedShippingMethodId: checkoutContext.SelectedShippingMethodId);

			PersistedCheckoutContextProvider.SaveCheckoutContext(customer, updatedCheckoutContext);
		}

		void UpdateOkToEmail(bool? okToEmailSelected, Customer customer)
		{
			if(!okToEmailSelected.HasValue)
				return;

			customer.UpdateCustomer(okToEmail: okToEmailSelected.Value);
		}

		void UpdateTermsAndConditions(bool? termsAndConditionAccepted, Customer customer)
		{
			if(!termsAndConditionAccepted.HasValue)
				return;

			var checkoutContext = PersistedCheckoutContextProvider.LoadCheckoutContext(customer);
			var updatedCheckoutContext = new PersistedCheckoutContext(
				creditCard: checkoutContext.CreditCard,
				payPalExpress: checkoutContext.PayPalExpress,
				purchaseOrder: checkoutContext.PurchaseOrder,
				braintree: checkoutContext.Braintree,
				amazonPayments: checkoutContext.AmazonPayments,
				termsAndConditionsAccepted: termsAndConditionAccepted.Value,
				over13Checked: checkoutContext.Over13Checked,
				shippingEstimateDetails: checkoutContext.ShippingEstimateDetails,
				offsiteRequiresBillingAddressId: checkoutContext.OffsiteRequiresBillingAddressId,
				offsiteRequiresShippingAddressId: checkoutContext.OffsiteRequiresShippingAddressId,
				email: checkoutContext.Email,
				selectedShippingMethodId: checkoutContext.SelectedShippingMethodId);

			PersistedCheckoutContextProvider.SaveCheckoutContext(customer, updatedCheckoutContext);
		}

		void CreateGiftCards(Customer customer, ShoppingCart cart)
		{
			var giftCardProductTypeIds = string.Format("{0},{1},{2}", AppLogic.AppConfig("GiftCard.CertificateProductTypeIDs").Trim(','),
					AppLogic.AppConfig("GiftCard.EmailProductTypeIDs").Trim(','),
					AppLogic.AppConfig("GiftCard.PhysicalProductTypeIDs").Trim(','))
				.Split(',')
				.Select(int.Parse)
				.ToList();

			foreach(var item in cart.CartItems.Where(ci => giftCardProductTypeIds.Contains(ci.ProductTypeId)))
			{
				//Check the number of certificate records in the GiftCard table for this item.
				int numCards = DB.GetSqlN("SELECT COUNT(*) AS N FROM GiftCard WHERE ShoppingCartRecID = @ShoppingCartRecId",
					new SqlParameter[] { new SqlParameter("@ShoppingCartRecId", item.ShoppingCartRecordID) });

				if(numCards < item.Quantity)
				{
					//Add records if not enough
					for(int i = 1; i <= item.Quantity - numCards; i++)
						GiftCard.CreateGiftCard(PurchasedByCustomerID: customer.CustomerID,
							SerialNumber: null,
							OrderNumber: null,
							ShoppingCartRecID: item.ShoppingCartRecordID,
							ProductID: item.ProductID,
							VariantID: item.VariantID,
							InitialAmount: item.Price,
							ExpirationDate: null,
							Balance: item.Price,
							GiftCardTypeID: item.ProductTypeId,
							EMailName: null,
							EMailTo: null,
							EMailMessage: null,
							ValidForCustomers: null,
							ValidForProducts: null,
							ValidForManufacturers: null,
							ValidForCategories: null,
							ValidForSections: null,
							ExtensionData: null);
				}
				else if(numCards > item.Quantity)
				{
					//Delete records if there are too many.
					DB.ExecuteSQL(string.Format("DELETE FROM GiftCard WHERE GiftCardID IN (SELECT TOP {0} GiftCardID FROM GiftCard WHERE ShoppingCartRecID = {1} ORDER BY GiftCardID DESC)",
						numCards - item.Quantity,
						item.ShoppingCartRecordID));
				}
			}
		}

		void SaveCustomerContextToDB(PaymentMethodInfo paymentMethod, PersistedCheckoutContext context, Customer customer, Address billingAddress)
		{
			// Take info from the billing address and put it on the customer record if it's missing
			if(string.IsNullOrEmpty(customer.FirstName) || string.IsNullOrEmpty(customer.LastName))
				customer.UpdateCustomer(
					firstName: billingAddress.FirstName,
					lastName: billingAddress.LastName);

			if(string.IsNullOrEmpty(customer.Phone))
				customer.UpdateCustomer(phone: billingAddress.Phone);

			//Save the 'over 13' value if needed
            // nal
			//if(AppLogic.AppConfigBool("RequireOver13Checked") && context.Over13Checked)
			if(context.Over13Checked)
				customer.UpdateCustomer(over13Checked: true);

			//payment details for 2Checkout and braintree are not stored on the customer
			var activeGateway = AppLogic.ActivePaymentGatewayCleaned();
			var gatewayIsOffsite = activeGateway == Gateway.ro_GWBRAINTREE
					|| activeGateway == Gateway.ro_GWTWOCHECKOUT;

            if(paymentMethod == null
				|| (gatewayIsOffsite && paymentMethod.Name == AppLogic.ro_PMCreditCard))
				return;

			//Save payment details to the address
			if(paymentMethod.Name == AppLogic.ro_PMCreditCard)
			{
				billingAddress.PaymentMethodLastUsed = AppLogic.ro_PMCreditCard;
				billingAddress.CardName = context.CreditCard.Name;
				billingAddress.CardType = context.CreditCard.CardType;
				billingAddress.CardNumber = context.CreditCard.Number;
				billingAddress.CardExpirationMonth = context.CreditCard.ExpirationDate.Value.Month.ToString();
				billingAddress.CardExpirationYear = context.CreditCard.ExpirationDate.Value.Year.ToString();

				if(context.CreditCard.StartDate.HasValue)
					billingAddress.CardStartDate = string.Format("{0:00}{1:0000}", 
						context.CreditCard.StartDate.Value.Month,
						context.CreditCard.StartDate.Value.Year);

				billingAddress.CardIssueNumber = string.IsNullOrEmpty(context.CreditCard.IssueNumber)
					? string.Empty
					: context.CreditCard.IssueNumber;
				billingAddress.UpdateDB();

				//Stick the CVV in customer session so Gateway.MakeOrder can get to it (this is cleared out later)
				AppLogic.StoreCardExtraCodeInSession(customer, context.CreditCard.Cvv);
			}
			else if(paymentMethod.Name == AppLogic.ro_PMPurchaseOrder)
			{
				billingAddress.PONumber = context.PurchaseOrder.Number;
				if(!customer.MasterShouldWeStoreCreditCardInfo)
					billingAddress.ClearCCInfo();

				billingAddress.UpdateDB();
			}
			else if(paymentMethod.Name == AppLogic.ro_PMAmazonPayments)
			{
				billingAddress.CardNumber = context.AmazonPayments.AmazonOrderReferenceId;
				billingAddress.UpdateDB();
			}
		}

		void CleanupPaymentMethod(AppliedPaymentMethodCleanupContext context)
		{
			foreach(var provider in AppliedPaymentMethodCleanupProviders)
				provider.Cleanup(context);
		}

		void PushErrorMessages(string errorMessage)
		{
			if(string.IsNullOrEmpty(errorMessage))
				return;

			// Try for the ID first
			int errorId;
			if(int.TryParse(errorMessage, out errorId))
			{
				NoticeProvider.PushNotice(
						new ErrorMessage(errorId).Message,
						NoticeType.Failure);

				return;
			}

			// Sometimes we actually get the error message on the querystring
			NoticeProvider.PushNotice(
				new ErrorMessage(errorMessage).Message,
				NoticeType.Failure);
		}
	}
}
