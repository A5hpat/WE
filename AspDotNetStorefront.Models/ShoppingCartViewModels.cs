// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using AspDotNetStorefront.Validation.DataAttribute;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Models
{
	public class ShoppingCartViewModel
	{
		public IEnumerable<ShoppingCartItemViewModel> CartItems
		{ get; set; }

		public int MaximumCartItemsToDisplay
		{ get; set; }

		public bool UseMaximumCartItemBehavor
		{ get; set; }

		public string SubTotal
		{ get; set; }
	}

	public class ShoppingCartItemViewModel
	{
		public int Id
		{ get; set; }

		public int ProductId
		{ get; set; }

		public int VariantId
		{ get; set; }

		public int RecurringVariantId
		{ get; set; }

		public string ProductName
		{ get; set; }

		public string VariantName
		{ get; set; }

		public string ChosenColor
		{ get; set; }

		public string ChosenColorSkuModifier
		{ get; set; }

		public string ChosenSize
		{ get; set; }

		public string ChosenSizeSkuModifier
		{ get; set; }

		public string ProductSku
		{ get; set; }

		[DisplayFormat(DataFormatString = "{0:c}")]
		public decimal Price
		{ get; set; }

		public string PriceDisplay
		{ get; set; }

		public string SubTotalDisplay
		{ get; set; }

		public string VatDisplay
		{ get; set; }

		public string Notes
		{ get; set; }

		public string ProductUrl
		{ get; set; }

		public string ProductImageUrl
		{ get; set; }

		public string ProductImageAlternateText
		{ get; set; }

		[MinValue(0, ErrorMessage = "shoppingcart.quantity.invalid")]
		[Required(ErrorMessage = "shoppingcart.quantity.invalid")]
		public int Quantity
		{ get; set; }

		public bool ShowCartImages
		{ get; set; }

		public bool ShowRecurringDropDown
		{ get; set; }

		public bool LinkToProduct
		{ get; set; }

		public bool ShowSku
		{ get; set; }

		public string TextOptionLabel
		{ get; set; }

		public string TextOption
		{ get; set; }

		public string LineItemPromotionText
		{ get; set; }

		public SelectList RecurringIntervalOptions
		{ get; set; }

		public bool IsAKit
		{ get; set; }

		public List<KitCartItemViewModel> KitDetails
		{ get; set; }

		public bool ShowEditLink
		{ get; set; }

		public string EditUrl
		{ get; set; }

		public SelectList RestrictedQuantities
		{ get; set; }

	}

	public class KitCartItemViewModel
	{
		public string Name
		{ get; set; }

		public string TextOption
		{ get; set; }

		public bool IsImage
		{ get; set; }

	}

	public class AddToCartPostModel
	{
		public int ProductId
		{ get; set; }

		public int VariantId
		{ get; set; }

		public int? CartRecordId
		{ get; set; }

		public int Quantity
		{ get; set; }

		public decimal CustomerEnteredPrice
		{ get; set; }

		public string Color
		{ get; set; }

		public string Size
		{ get; set; }

		public string TextOption
		{ get; set; }

		public string UpsellProducts
		{ get; set; }

		public bool IsWishlist
		{ get; set; }

		public string ReturnUrl
		{ get; set; }
	}

	public class AddToCartViewModel : AddToCartPostModel
	{
		public readonly SelectList VariantOptions;
		public readonly bool ShowQuantity;
		public readonly SelectList RestrictedQuantities;
		public readonly bool CustomerEntersPrice;
		public readonly string CustomerEntersPricePrompt;
		public readonly SelectList ColorOptions;
		public readonly string ColorOptionPrompt;
		public readonly SelectList SizeOptions;
		public readonly string SizeOptionPrompt;
		public readonly string TextOptionPrompt;
		public readonly bool ShowTextOption;
		public readonly bool RequiresTextOption;
		public readonly int TextOptionMaxLength;
		public readonly bool IsCallToOrder;
		public readonly bool ShowBuyButton;
		public readonly bool ShowWishlistButton;
		public readonly PayPalAd PayPalAd;
		public readonly bool ShowBuySafeKicker;
		public readonly string BuySafeKickerType;
		public readonly bool ColorSelectorChangesImage;
		public readonly bool IsSimpleProduct;
		public readonly CartTypeEnum CartType;

		public AddToCartViewModel(
			bool showQuantity,
			SelectList restrictedQuantities,
			bool customerEntersPrice,
			string customerEntersPricePrompt,
			SelectList colorOptions,
			string colorOptionPrompt,
			SelectList sizeOptions,
			string sizeOptionPrompt,
			string textOptionPrompt,
			bool showTextOption,
			bool requiresTextOption,
			int textOptionMaxLength,
			bool isCallToOrder,
			bool showBuyButton,
			bool showWishlistButton,
			PayPalAd payPalAd,
			bool showBuySafeKicker,
			string buySafeKickerType,
			bool colorSelectorChangesImage,
			bool isSimpleProduct,
			CartTypeEnum cartType)
		{
			ShowQuantity = showQuantity;
			RestrictedQuantities = restrictedQuantities;
			CustomerEntersPrice = customerEntersPrice;
			CustomerEntersPricePrompt = customerEntersPricePrompt;
			ColorOptions = colorOptions;
			ColorOptionPrompt = colorOptionPrompt;
			SizeOptions = sizeOptions;
			SizeOptionPrompt = sizeOptionPrompt;
			TextOptionPrompt = textOptionPrompt;
			ShowTextOption = showTextOption;
			RequiresTextOption = requiresTextOption;
			TextOptionMaxLength = textOptionMaxLength;
			IsCallToOrder = isCallToOrder;
			ShowBuyButton = showBuyButton;
			ShowWishlistButton = showWishlistButton;
			PayPalAd = payPalAd;
			ShowBuySafeKicker = showBuySafeKicker;
			BuySafeKickerType = buySafeKickerType;
			ColorSelectorChangesImage = colorSelectorChangesImage;
			IsSimpleProduct = isSimpleProduct;
			CartType = cartType;
		}
	}

	public class BulkAddToCartPostModel
	{
		public IEnumerable<AddToCartPostModel> AddToCartItems
		{ get; set; }

		public string ReturnUrl
		{ get; set; }
	}

	public class AjaxKitDataViewModel
	{
		public string SummaryHtml
		{ get; set; }

		public Dictionary<string, string> ItemDisplayNames
		{ get; set; }
	}
}
