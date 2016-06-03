// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;

namespace AspDotNetStorefront.Models
{
	public class OrderConfirmationViewModel
	{
		public readonly int OrderNumber;
		public readonly string Body;
		public readonly string GoogleTrackingCode;
		public readonly string GeneralTrackingCode;
        public readonly bool ShowGeneralTrackingCode;
        public readonly bool ShowGoogleTrackingCode;
		public readonly bool ShowGoogleTrustedStores;
        public readonly bool AddPayPalIntegratedCheckoutScript;
		public readonly bool AddBuySafeScript;

		public OrderConfirmationViewModel(
			int orderNumber,
			string body,
			string googleTrackingCode,
			string generalTrackingCode,
            bool showGoogleTrackingCode,
            bool showGeneralTrackingCode,
			bool showGoogleTrustedStores,
            bool addPayPalIntegratedCheckoutScript,
			bool addBuySafeScript)
		{
			OrderNumber = orderNumber;
			Body = body;
			GoogleTrackingCode = googleTrackingCode;
			GeneralTrackingCode = generalTrackingCode;
			ShowGoogleTrackingCode = showGoogleTrackingCode;
			ShowGeneralTrackingCode = showGeneralTrackingCode;
			ShowGoogleTrustedStores = showGoogleTrustedStores;
			AddPayPalIntegratedCheckoutScript = addPayPalIntegratedCheckoutScript;
			AddBuySafeScript = addBuySafeScript;
        }
	}

	public class GoogleTrustedStoresViewModel
	{
		public readonly int OrderNumber;
		public readonly string Domain;
		public readonly string Email;
		public readonly string CountryCode;
		public readonly string Currency;
		public readonly string ShipDate;
		public readonly string DeliverDate;
		public readonly string HasDigital;
		public readonly decimal Total;
		public readonly decimal Discounts;
		public readonly decimal ShippingTotal;
		public readonly decimal TaxTotal;
		public readonly IEnumerable<GoogleTrustedStoresCartItemViewModel> CartItems;

		public GoogleTrustedStoresViewModel(
			int orderNumber,
			string domain,
			string email,
			string countryCode,
			string currency,
			string shipDate,
			string deliveryDate,
			string hasDigital,
            decimal total,
			decimal discounts,
			decimal shippingTotal,
			decimal taxTotal,
			List<GoogleTrustedStoresCartItemViewModel> cartItems)
		{
			OrderNumber = orderNumber;
			Domain = domain;
			Email = email;
			CountryCode = countryCode;
			Currency = currency;
			ShipDate = shipDate;
			DeliverDate = deliveryDate;
			HasDigital = hasDigital;
			Total = total;
			Discounts = discounts;
			ShippingTotal = shippingTotal;
			TaxTotal = taxTotal;
			CartItems = cartItems;
		}
    }

	public class GoogleTrustedStoresCartItemViewModel
	{
		public readonly string ProductName;
		public readonly string ProductSearchId;
		public readonly string ProductSearchStoreId;
		public readonly string Country;
		public readonly string Language;
		public readonly decimal Price;
		public readonly int Quantity;

		public GoogleTrustedStoresCartItemViewModel(
			string productName,
			string productSearchId,
			string productSearchStoreId,
			string country,
			string language,
			decimal price,
			int quantity)
		{
			ProductName = productName;
			ProductSearchId = productSearchId;
			ProductSearchStoreId = productSearchStoreId;
			Country = country;
			Language = language;
			Price = price;
			Quantity = quantity;
		}
	}

	public class BuySafeGuaranteeViewModel
	{
		public readonly int OrderNumber;
		public readonly string JSLocation;
		public readonly string Hash;
		public readonly string Email;
		public readonly decimal Total;

		public BuySafeGuaranteeViewModel(
			int orderNumber,
			string jsLocation,
			string hash,
			string email,
			decimal total)
		{
			OrderNumber = orderNumber;
			JSLocation = jsLocation;
			Hash = hash;
			Email = email;
			Total = total;
		}
	}

	public class ReceiptViewModel
	{
		public readonly string Body;

		public ReceiptViewModel(string body)
		{
			Body = body;
		}
	}
}
