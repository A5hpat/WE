// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Web.Mvc;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Models
{
	public class KitAddToCartPostModel
	{
		public int ProductId
		{ get; set; }

		public int VariantId
		{ get; set; }

		public int? CartRecordId
		{ get; set; }

		public int Quantity
		{ get; set; }

		public string UpsellProducts
		{ get; set; }

		public bool IsWishlist
		{ get; set; }

		public string ReturnUrl
		{ get; set; }

		public IEnumerable<KitGroupViewModel> KitGroups
		{ get; set; }

		public string TemporaryImageNameStub
		{ get; set; }
	}

	public class KitAddToCartViewModel : KitAddToCartPostModel
	{
		public readonly string RegularBasePrice;
		public readonly string BasePrice;
		public readonly string CustomizedPrice;
		public readonly string CustomerLevelPrice;
		public readonly bool IsUnorderable;
		public readonly bool IsCallToOrder;
		public readonly bool ShowRegularBasePrice;
		public readonly bool ShowBasePrice;
		public readonly bool ShowCustomerLevelPrice;
		public readonly bool ShowWishlistButton;
		public readonly bool ShowBuyButton;
		public readonly bool ShowQuantity;
		public readonly bool HidePriceUntilCart;
		public readonly SelectList RestrictedQuantities;
		public readonly PayPalAd PayPalAd;
		public readonly bool ShowSchemaOrgPrice;
		public readonly string SchemaBasePrice;
		public readonly string IsoThreeLetterCurrency;
		public readonly string SchemaProductUrl;
		public readonly string SchemaOfferUrl;
		public readonly CartTypeEnum CartType;

		public KitAddToCartViewModel(
			string regularBasePrice,
			string basePrice,
			string customizedPrice,
			string customerLevelPrice,
			bool isUnorderable,
			bool isCallToOrder,
			bool showRegularBasePrice,
			bool showBasePrice,
			bool showCustomerLevelPrice,
			bool showWishlistButton,
			bool showBuyButton,
			bool showQuantity,
			bool hidePriceUntilCart,
			SelectList restrictedQuantities,
			PayPalAd payPalAd,
			bool showSchemaOrgPrice,
			string schemaBasePrice,
			string isoThreeLetterCurrency,
			string schemaProductUrl,
			string schemaOfferUrl,
			CartTypeEnum cartType)
		{
			RegularBasePrice = regularBasePrice;
			BasePrice = basePrice;
			CustomizedPrice = customizedPrice;
			CustomerLevelPrice = customerLevelPrice;
			IsUnorderable = isUnorderable;
			IsCallToOrder = isCallToOrder;
			ShowRegularBasePrice = showRegularBasePrice;
			ShowBasePrice = showBasePrice;
			ShowCustomerLevelPrice = showCustomerLevelPrice;
			ShowWishlistButton = showWishlistButton;
			ShowBuyButton = showBuyButton;
			ShowQuantity = showQuantity;
			RestrictedQuantities = restrictedQuantities;
			PayPalAd = payPalAd;
			HidePriceUntilCart = hidePriceUntilCart;
			ShowSchemaOrgPrice = showSchemaOrgPrice;
			SchemaBasePrice = schemaBasePrice;
			IsoThreeLetterCurrency = isoThreeLetterCurrency;
			SchemaProductUrl = schemaProductUrl;
			SchemaOfferUrl = schemaOfferUrl;
			CartType = cartType;
		}
	}

	public class KitGroupViewModel
	{
		public int Id
		{ get; set; }

		public string Name
		{ get; set; }

		public string Description
		{ get; set; }

		public string Summary
		{ get; set; }

		public string ImageUrl
		{ get; set; }

		public int KitGroupType
		{ get; set; }

		public int SelectedItemId
		{ get; set; }

		public bool IsRequired
		{ get; set; }

		public bool IsReadOnly
		{ get; set; }

		public IEnumerable<KitItemViewModel> Items
		{ get; set; }
	}

	public class KitItemViewModel
	{
		public int Id
		{ get; set; }

		public bool IsSelected
		{ get; set; }

		public string TextOption
		{ get; set; }

		public string Name
		{ get; set; }

		public string NameDisplay
		{ get; set; }

		public string Description
		{ get; set; }

		public bool IsDefault
		{ get; set; }

		public decimal PriceDelta
		{ get; set; }

		public string PriceDeltaDisplay
		{ get; set; }

		public decimal WeightDelta
		{ get; set; }

		public int DisplayOrder
		{ get; set; }

		public string ImageUrl
		{ get; set; }

		public string OutOfStockMessage
		{ get; set; }
	}
}
