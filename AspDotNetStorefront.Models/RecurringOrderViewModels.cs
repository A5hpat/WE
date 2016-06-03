// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace AspDotNetStorefront.Models
{
	public class RecurringOrderIndexViewModel
	{
		public readonly bool LinkToProduct;
		public readonly IEnumerable<RecurringOrderViewModel> RecurringOrders;

		public RecurringOrderIndexViewModel(bool linkToProduct, IEnumerable<RecurringOrderViewModel> recurringOrders)
		{
			LinkToProduct = linkToProduct;
			RecurringOrders = recurringOrders;
		}
	}

	public class RecurringOrderEditViewModel
	{
		public readonly int RecurringOrderId;
		public readonly bool RequiresCreditCardForm;
		public readonly AddressDetailViewModel Address;
		public readonly CreditCardViewModel CreditCard;
		public readonly SelectList CardTypeOptions;
		public readonly SelectList ExpirationMonthOptions;
		public readonly SelectList ExpirationYearOptions;

		public RecurringOrderEditViewModel(int recurringOrderId, bool requiresCreditCardForm, AddressDetailViewModel address, CreditCardViewModel creditCard, SelectList cardTypeOptions, SelectList expirationMonthOptions, SelectList expirationYearOptions)
		{
			RecurringOrderId = recurringOrderId;
			RequiresCreditCardForm = requiresCreditCardForm;
			Address = address;
			CreditCard = creditCard;
			CardTypeOptions = cardTypeOptions;
			ExpirationMonthOptions = expirationMonthOptions;
			ExpirationYearOptions = expirationYearOptions;
		}
	}

	public class RecurringOrderPostViewModel
	{
		public int RecurringOrderId
		{ get; set; }

		public AddressViewModel Address
		{ get; set; }

		public CreditCardViewModel CreditCard
		{ get; set; }
	}

	public class RecurringOrderViewModel
	{
		public readonly int RecurringOrderId;
		public readonly string RecurringSubscriptionId;
		public readonly int RecurringIndex;
		public readonly DateTime CreatedOn;
		public readonly bool AllowUpdate;
		public readonly IEnumerable<RecurringOrderCartItemViewModel> CartItems;

		public RecurringOrderViewModel(int recurringOrderId, string recurringSubscriptionId, int recurringIndex, DateTime createdOn, bool allowUpdate, IEnumerable<RecurringOrderCartItemViewModel> cartItems)
		{
			RecurringOrderId = recurringOrderId;
			RecurringSubscriptionId = recurringSubscriptionId;
			RecurringIndex = recurringIndex;
			CreatedOn = createdOn;
			AllowUpdate = allowUpdate;
			CartItems = cartItems;
		}
	}

	public class RecurringOrderCartItemViewModel
	{
		public readonly int ProductId;
		public readonly string ProductName;
		public readonly string ProductLink;
		public readonly string Sku;
		public readonly string ChosenColor;
		public readonly string ChosenSize;
		public readonly int Quantity;
		public readonly string Price;
		public readonly DateTime NextRecurringShipDate;
		public readonly bool IsSystem;

		public RecurringOrderCartItemViewModel(int productId, string productName, string productLink, string sku, string chosenColor, string chosenSize, int quantity, string price, DateTime nextRecurringShipDate, bool isSystem)
		{
			ProductId = productId;
			ProductName = productName;
			ProductLink = productLink;
			Sku = sku;
			ChosenColor = chosenColor;
			ChosenSize = chosenSize;
			Quantity = quantity;
			Price = price;
			NextRecurringShipDate = nextRecurringShipDate;
			IsSystem = isSystem;
		}
	}
}
