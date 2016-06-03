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
using System.Text;
using System.Web;
using System.Web.Mvc;
using AspDotNetStorefront.Checkout;
using AspDotNetStorefront.Classes;
using AspDotNetStorefront.Filters;
using AspDotNetStorefront.Models;
using AspDotNetStorefront.Models.Converter;
using AspDotNetStorefront.Routing;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontCore.Validation;
using AspDotNetStorefrontGateways;
using AspDotNetStorefrontGateways.Processors;

namespace AspDotNetStorefront.Controllers
{
	[RequireCustomerRecordFilter]
	[Authorize]
	[SecureAccessFilter(forceHttps: true)]
	public class RecurringOrderController : Controller
	{
		readonly AddressHeaderProvider AddressHeaderProvider;
		readonly RecurringOrderControllerHelper ControllerHelper;
		readonly AddressSelectListBuilder AddressSelectListBuilder;
		readonly CreditCardSelectListBuilder CreditCardSelectListBuilder;
		readonly NoticeProvider NoticeProvider;
		readonly IPostalCodeLookupProvider PostalCodeLookupProvider;
		readonly AddressViewModelConverter AddressViewModelConverter;
		readonly bool ShowCompanyField;
		readonly bool ShowNickName;

		public RecurringOrderController(
			AddressHeaderProvider addressHeaderProvider,
			AddressSelectListBuilder addressSelectListBuilder,
			CreditCardSelectListBuilder creditCardSelectListBuilder, 
			NoticeProvider noticeProvider, 
			IPostalCodeLookupProvider postalCodeLookupProvider,
			AddressViewModelConverter addressViewModelConverter)
		{
			AddressHeaderProvider = addressHeaderProvider;
            ControllerHelper = new RecurringOrderControllerHelper(creditCardSelectListBuilder);
			AddressSelectListBuilder = addressSelectListBuilder;
			CreditCardSelectListBuilder = creditCardSelectListBuilder;
			NoticeProvider = noticeProvider;
			PostalCodeLookupProvider = postalCodeLookupProvider;
			AddressViewModelConverter = addressViewModelConverter;
			ShowCompanyField = AppLogic.AppConfigBool("Address.CollectCompany");
			ShowNickName = AppLogic.AppConfigBool("Address.CollectNickName");
		}

		public ActionResult Index()
		{
			var customer = HttpContext.GetCustomer();
			var model = ControllerHelper.BuildRecurringOrderIndexViewModel(customer);

			return View(model);
		}

		public ActionResult Edit(int recurringOrderId)
		{
			var customer = HttpContext.GetCustomer();

			var billingAddress = AddressViewModelConverter.ConvertToAddressViewModel(
				customer.PrimaryBillingAddress,
				customer);

			var address = new AddressDetailViewModel(
				address: billingAddress,
				residenceTypeOptions: AddressSelectListBuilder.BuildResidenceTypeSelectList(billingAddress.ResidenceType.ToString()),
				stateOptions: AddressSelectListBuilder.BuildStateSelectList(billingAddress.Country, billingAddress.State),
				countryOptions: AddressSelectListBuilder.BuildCountrySelectList(billingAddress.Country),
				showCompanyField: ShowCompanyField,
				showNickName: ShowNickName,
				showResidenceTypeField: true,
				showPostalCodeLookup: PostalCodeLookupProvider.IsEnabled(billingAddress.Country),
				returnUrl: string.Empty,
				header: AddressHeaderProvider.GetHeaderText(billingAddress.Id, AddressTypes.Billing));

			return View(ControllerHelper.BuildRecurringOrderEditViewModel(
				recurringOrderId: recurringOrderId,
				address: address,
				creditCard: new CreditCardViewModel(),
				customer: customer));
		}

		[HttpPost]
		public ActionResult Edit(RecurringOrderPostViewModel model)
		{
			var customer = HttpContext.GetCustomer();

			if (!ModelState.IsValid)
				return View(ActionNames.Edit, ControllerHelper.BuildRecurringOrderEditViewModel(
					recurringOrderId: model.RecurringOrderId,
					address: new AddressDetailViewModel(
						address: model.Address,
						residenceTypeOptions: AddressSelectListBuilder.BuildResidenceTypeSelectList(model.Address.ResidenceType.ToString()),
						stateOptions: AddressSelectListBuilder.BuildStateSelectList(model.Address.Country, model.Address.State),
						countryOptions: AddressSelectListBuilder.BuildCountrySelectList(model.Address.Country),
						showCompanyField: ShowCompanyField,
						showNickName: ShowNickName,
						showResidenceTypeField: true,
						showPostalCodeLookup: PostalCodeLookupProvider.IsEnabled(model.Address.Country),
						returnUrl: string.Empty,
						header: AddressHeaderProvider.GetHeaderText(model.Address.Id, AddressTypes.Billing)),
					creditCard: model.CreditCard,
					customer: customer));

			if(!customer.Owns.RecurringOrder(model.RecurringOrderId))
				throw new HttpException(403, "Forbidden");

			var result = ControllerHelper.UpdateRecurringOrder(
				recurringOrderId: model.RecurringOrderId,
				address: AddressViewModelConverter.ConvertToAddress(model.Address, customer),
				creditCard: model.CreditCard,
				customer: customer);

			switch(result.Status)
			{
				case RecurringOrderActionStatus.Failure:
					NoticeProvider.PushNotice(result.Message, NoticeType.Failure);
					break;

				default:
				case RecurringOrderActionStatus.Success:
					break;
			}

			return RedirectToAction(ActionNames.Index);
		}

		[HttpPost]
		public ActionResult ChangeCountry(RecurringOrderPostViewModel model)
		{
			var customer = HttpContext.GetCustomer();

			if((model.Address.Id.HasValue
				&& !Customer.OwnsThisAddress(customer.CustomerID, (int)model.Address.Id))
				|| !customer.Owns.RecurringOrder(model.RecurringOrderId))
				throw new HttpException(403, "Forbidden");

			var address = new AddressDetailViewModel(
				address: model.Address,
				residenceTypeOptions: AddressSelectListBuilder.BuildResidenceTypeSelectList(model.Address.ResidenceType.ToString()),
				stateOptions: AddressSelectListBuilder.BuildStateSelectList(model.Address.Country, model.Address.State),
				countryOptions: AddressSelectListBuilder.BuildCountrySelectList(model.Address.Country),
				showCompanyField: ShowCompanyField,
				showNickName: ShowNickName,
				showResidenceTypeField: true,
				showPostalCodeLookup: PostalCodeLookupProvider.IsEnabled(model.Address.Country),
				returnUrl: string.Empty,
				header: AddressHeaderProvider.GetHeaderText(model.Address.Id, AddressTypes.Billing));

			return View(ActionNames.Edit,
					ControllerHelper.BuildRecurringOrderEditViewModel(
						recurringOrderId: model.RecurringOrderId,
						address: address,
						creditCard: new CreditCardViewModel(),
						customer: customer));
		}

		public ActionResult Delete(int recurringOrderId)
		{
			var customer = HttpContext.GetCustomer();

			var order = new Order(recurringOrderId);
			if(order == null)
				return HttpNotFound();

			if(!customer.IsAdminUser && customer.CustomerID != order.CustomerID)
				throw new HttpException(403, "Forbidden");

			var result = ControllerHelper.DeleteRecurringOrder(order);
			switch(result.Status)
			{
				case RecurringOrderActionStatus.Failure:
					NoticeProvider.PushNotice(result.Message, NoticeType.Failure);
					break;

				default:
				case RecurringOrderActionStatus.Success:
					break;
			}

			return RedirectToAction(ActionNames.Index);
		}

		public ActionResult AuthorizeNetSilentPost(FormCollection collection)
		{
			var subscriptionId = collection["x_subscription_id"] ?? string.Empty;
			var responseCode = collection["x_response_code"] ?? string.Empty;
			var responseReason = collection["x_response_reason_text"] ?? string.Empty;
			var transactionId = collection["x_trans_id"] ?? string.Empty;
			var amount = collection["x_amount"] ?? string.Empty;
			var transactionDate = DateTime.Now;

			if(string.IsNullOrEmpty(subscriptionId))
				return Content(string.Empty);

			var originalOrderId = AppLogic.GetOriginalRecurringOrderNumberFromSubscriptionID(subscriptionId);
			var status = string.Empty;

			if(originalOrderId == 0)
			{
				status = "Silent Post: No Original Order Found";
				if(!string.IsNullOrEmpty(transactionId))
					status += ", PNREF=" + transactionId;

				DB.ExecuteSQL(@"INSERT INTO FailedTransaction(CustomerID, 
															OrderNumber, 
															IPAddress, 
															OrderDate, 
															PaymentGateway, 
															PaymentMethod, 
															TransactionCommand, 
															TransactionResult, 
															CustomerEMailed, 
															RecurringSubscriptionID) 
								VALUES(0, 0, @ipAddress, @transactionDate, @gateway, @paymentMethod, @command, @status, 0, @subscriptionId)",
							new SqlParameter[]
								{
									new SqlParameter("@ipAddress", "0.0.0.0"),
									new SqlParameter("@transactionDate", transactionDate),
									new SqlParameter("@gateway", "AUTHORIZENET"),
									new SqlParameter("@paymentMethod", AppLogic.TransactionTypeEnum.RECURRING_AUTO.ToString()),
									new SqlParameter("@command", AppLogic.ro_NotApplicable),
									new SqlParameter("@status", status),
									new SqlParameter("@subscriptionId", subscriptionId),
								}
							);
			}
			else
			{
				if(responseCode.Equals("1")) // Approved
				{
					var newOrderNumber = 0;
					var manager = new RecurringOrderMgr();
					status = manager.ProcessAutoBillApproved(originalOrderId, transactionId, transactionDate, out newOrderNumber);
				}
				else
				{
					var manager = new RecurringOrderMgr();
					status = manager.ProcessAutoBillDeclined(originalOrderId, transactionId, transactionDate, subscriptionId, responseReason);
				}

				if(status.Equals(AppLogic.ro_OK, StringComparison.InvariantCultureIgnoreCase))
				{
					var customerId = Order.GetOrderCustomerID(originalOrderId);
					var customer = new Customer(customerId, true);

					if(!string.IsNullOrEmpty(transactionId))
						status += ", PNREF=" + transactionId;

					DB.ExecuteSQL(@"INSERT INTO FailedTransaction(CustomerID, 
																	OrderNumber, 
																	IPAddress, 
																	OrderDate, 
																	PaymentGateway, 
																	PaymentMethod, 
																	TransactionCommand, 
																	TransactionResult, 
																	CustomerEMailed, 
																	RecurringSubscriptionID) 
									VALUES(@customerId, @orderId, @ipAddress, @transactionDate, @gateway, @paymentMethod, @command, @status, 0, @subscriptionId)",
								new SqlParameter[]
									{
										new SqlParameter("@customerId", customer.CustomerID),
										new SqlParameter("@orderId", originalOrderId),
										new SqlParameter("@ipAddress", "0.0.0.0"),
										new SqlParameter("@transactionDate", transactionDate),
										new SqlParameter("@gateway", "AUTHORIZENET"),
										new SqlParameter("@paymentMethod", AppLogic.TransactionTypeEnum.RECURRING_AUTO.ToString()),
										new SqlParameter("@command", AppLogic.ro_NotApplicable),
										new SqlParameter("@status", status),
										new SqlParameter("@subscriptionId", subscriptionId),
									}
								);

				}
			}

			return Content(AppLogic.ro_OK);
		}

		class RecurringOrderControllerHelper
		{
			readonly CreditCardSelectListBuilder CreditCardSelectListBuilder;
			readonly UrlHelper Url;

			public RecurringOrderControllerHelper(CreditCardSelectListBuilder creditCardSelectListBuilder)
			{
				CreditCardSelectListBuilder = creditCardSelectListBuilder;
				Url = DependencyResolver.Current.GetService<UrlHelper>();
			}

			public RecurringOrderIndexViewModel BuildRecurringOrderIndexViewModel(Customer customer)
			{
				return new RecurringOrderIndexViewModel(
					linkToProduct: AppLogic.AppConfigBool("LinkToProductPageInCart"),
					recurringOrders: BuildRecurringOrderViewModels(customer));
			}
	
			IEnumerable<RecurringOrderViewModel> BuildRecurringOrderViewModels(Customer customer)
			{
				var parameters = new[]
					{
						new SqlParameter("@cartType", (int)CartTypeEnum.RecurringCart),
						new SqlParameter("@customerId", customer.CustomerID),
						new SqlParameter("@storeId", AppLogic.StoreID())
					};

				var query = @"
					SELECT DISTINCT OriginalRecurringOrderNumber 
					FROM ShoppingCart (NOLOCK) 
					WHERE CartType = @cartType
					AND CustomerID = @customerId 
					AND StoreID = @storeId
					ORDER BY OriginalRecurringOrderNumber DESC";

				using(var connection = new SqlConnection(DB.GetDBConn()))
				{
					connection.Open();
					using(var reader = DB.GetRS(query, parameters, connection))
						while(reader.Read())
							yield return BuildRecurringOrderViewModel(
								recurringOrderId: DB.RSFieldInt(reader, "OriginalRecurringOrderNumber"),
								customer: customer);
				}
			}

			RecurringOrderViewModel BuildRecurringOrderViewModel(int recurringOrderId, Customer customer)
			{
				var shoppingCart = new ShoppingCart(customer.SkinID, customer, CartTypeEnum.RecurringCart, recurringOrderId, false);
				var cartItem = (CartItem)shoppingCart.CartItems[0];
				var recurringOrder = new Order(recurringOrderId);

				var isPPECorder = recurringOrder.PaymentMethod == AppLogic.ro_PMPayPalExpress;
				
				var allowUpdate = !isPPECorder
					&& recurringOrder.PaymentGateway != Gateway.ro_GWPAYPAL
					&& !AppLogic.IsAdminSite;

				var cartItems = shoppingCart
					.CartItems
					.Where(ci => ci.OriginalRecurringOrderNumber == recurringOrderId)
					.Select(ci => new RecurringOrderCartItemViewModel(
						productId: ci.ProductID,
						productName: BuildCartItemProductName(ci, customer),
						productLink: Url.BuildProductLink(ci.ProductID, ci.SEName),
						sku: string.IsNullOrEmpty(ci.SKU)
							? "--"
							: ci.SKU,
						chosenColor: string.IsNullOrEmpty(ci.ChosenColor)
							? "--"
							: ci.ChosenColor,
						chosenSize: string.IsNullOrEmpty(ci.ChosenSize)
							? "--"
							: ci.ChosenSize,
						quantity: ci.Quantity,
						price: customer.CurrencyString(ci.Price),
						nextRecurringShipDate: ci.NextRecurringShipDate,
						isSystem: ci.IsSystem));

				return new RecurringOrderViewModel(
					recurringOrderId: recurringOrderId,
					recurringSubscriptionId: cartItem.RecurringSubscriptionID,
					recurringIndex: cartItem.RecurringIndex,
					createdOn: cartItem.CreatedOn,
					allowUpdate: allowUpdate,
					cartItems: cartItems);
			}

			public RecurringOrderEditViewModel BuildRecurringOrderEditViewModel(int recurringOrderId, AddressDetailViewModel address, CreditCardViewModel creditCard, Customer customer)
			{
				var originalOrder = new Order(recurringOrderId);

				if(customer.MasterShouldWeStoreCreditCardInfo
					&& customer.PrimaryBillingAddress != null)
				{
					creditCard.Name = customer.PrimaryBillingAddress.CardName;
					creditCard.Number = customer.PrimaryBillingAddress.CardNumber;
					creditCard.CardType = customer.PrimaryBillingAddress.CardType;
					creditCard.ExpirationMonth = int.Parse(customer.PrimaryBillingAddress.CardExpirationMonth);
					creditCard.ExpirationYear = int.Parse(customer.PrimaryBillingAddress.CardExpirationYear);
					creditCard.CardStartMonth = !string.IsNullOrEmpty(customer.PrimaryBillingAddress.CardStartDate)
						? int.Parse(customer.PrimaryBillingAddress.CardStartDate.Substring(0, 2))
						: 0;
					creditCard.CardStartYear = !string.IsNullOrEmpty(customer.PrimaryBillingAddress.CardStartDate)
						? int.Parse(customer.PrimaryBillingAddress.CardStartDate.Substring(2, 4))
						: 0;
					creditCard.CardIssueNumber = customer.PrimaryBillingAddress.CardIssueNumber;
				}

				return new RecurringOrderEditViewModel(
					recurringOrderId: recurringOrderId,
					requiresCreditCardForm: originalOrder.PaymentMethod == AppLogic.ro_PMCreditCard,
					address: address,
					creditCard: creditCard,
					cardTypeOptions: CreditCardSelectListBuilder.BuildCreditCardTypeSelectList(),
					expirationMonthOptions: CreditCardSelectListBuilder.BuildExpirationMonthSelectList(),
					expirationYearOptions: CreditCardSelectListBuilder.BuildExpirationYearSelectList());
			}

			string BuildCartItemProductName(CartItem cartItem, Customer customer)
			{
				var productName = new StringBuilder(AppLogic.MakeProperObjectName(cartItem.ProductName, cartItem.VariantName, customer.LocaleSetting));
				if(cartItem.TextOption.Length != 0)
				{
					if(cartItem.TextOption.IndexOf("\n") != -1)
						productName.AppendFormat("{0}{1}",
							"Customization Text:",
							XmlCommon
								.GetLocaleEntry(cartItem.TextOption, customer.LocaleSetting, true)
								.Replace("\n", ""));
					else
						productName.AppendFormat(" ({0} {1}) ",
							"Customization Text:",
							XmlCommon.GetLocaleEntry(cartItem.TextOption, customer.LocaleSetting, true));
				}

				return productName.ToString();
			}

			public UpdateRecurringOrderResult UpdateRecurringOrder(int recurringOrderId, Address address, CreditCardViewModel creditCard, Customer customer)
			{
				if(creditCard != null && customer.MasterShouldWeStoreCreditCardInfo)
				{
					address.PaymentMethodLastUsed = AppLogic.ro_PMCreditCard;
					address.CardName = creditCard.Name;
					address.CardType = creditCard.CardType;
					address.CardNumber = creditCard.Number;
					address.CardExpirationMonth = creditCard.ExpirationMonth.ToString();
					address.CardExpirationYear = creditCard.ExpirationYear.ToString();
					address.CardIssueNumber = creditCard.CardIssueNumber;
					address.CardStartDate = string.Format("{0:00}{1:0000}",
							creditCard.CardStartMonth,
							creditCard.CardStartYear);
				}

				address.UpdateDB();

				var recurringOrderManager = new RecurringOrderMgr();
				var result = recurringOrderManager
					.ProcessAutoBillAddressUpdate(recurringOrderId, address);

				if(result == AppLogic.ro_OK)
					return new UpdateRecurringOrderResult();
				else
					return new UpdateRecurringOrderResult(RecurringOrderActionStatus.Failure, result);
			}

			public DeleteRecurringOrderResult DeleteRecurringOrder(Order order)
			{
				var recurringOrderManager = new RecurringOrderMgr();
				var expressApiType = PayPalController.GetAppropriateExpressType();

				var result = string.Empty;

				if(order.PaymentMethod == AppLogic.ro_PMPayPalExpress && expressApiType == ExpressAPIType.PayPalExpress)
					result = recurringOrderManager.CancelPPECRecurringOrder(order.OrderNumber, false);
				else
					result = recurringOrderManager.CancelRecurringOrder(order.OrderNumber);

				if(result == AppLogic.ro_OK)
					return new DeleteRecurringOrderResult();
				else
					return new DeleteRecurringOrderResult(RecurringOrderActionStatus.Failure, result);
			}
		}

		class UpdateRecurringOrderResult
		{
			public readonly RecurringOrderActionStatus Status;
			public readonly string Message;

			public UpdateRecurringOrderResult(RecurringOrderActionStatus status = RecurringOrderActionStatus.Success, string message = null)
			{
				Status = status;
				Message = message;
			}
		}

		class DeleteRecurringOrderResult
		{
			public readonly RecurringOrderActionStatus Status;
			public readonly string Message;

			public DeleteRecurringOrderResult(RecurringOrderActionStatus status = RecurringOrderActionStatus.Success, string message = null)
			{
				Status = status;
				Message = message;
			}
		}

		enum RecurringOrderActionStatus
		{
			Success,
			Failure,
		}
	}
}
