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
using System.Web;
using System.Web.Mvc;
using AspDotNetStorefront.Controllers.Classes;
using AspDotNetStorefront.Models;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Classes.Checkout
{
	public static class TypeConversions
	{
		public static AddressViewModel ConvertToAddressViewModel(this Address address, Customer customer)
		{
			return new AddressViewModel
			{
				Id = address.AddressID > 0
					? address.AddressID
					: (int?)null,
				NickName = address.NickName,
				FirstName = address.FirstName,
				LastName = address.LastName,
				Company = address.Company,
				Address1 = address.Address1,
				Address2 = address.Address2,
				Suite = address.Suite,
				City = address.City,
				State = address.State,
				Zip = address.Zip,
				Country = address.Country,
				Phone = address.Phone,
				ResidenceType = address.ResidenceType.ToString(),
				IsPrimaryBillingAddress = address.AddressID == customer.PrimaryBillingAddressID,
				IsPrimaryShippingAddress = address.AddressID == customer.PrimaryShippingAddressID,
				OffsiteSource = address.OffsiteSource
			};
		}

		public static Address ConvertToAddress(this AddressViewModel addressViewModel, Customer customer)
		{
			return new Address
			{
				CustomerID = customer.CustomerID,
				AddressID = addressViewModel.Id ?? -1,
				NickName = addressViewModel.NickName ?? string.Empty,
				FirstName = addressViewModel.FirstName ?? string.Empty,
				LastName = addressViewModel.LastName ?? string.Empty,
				Company = addressViewModel.Company ?? string.Empty,
				Address1 = addressViewModel.Address1 ?? string.Empty,
				Address2 = addressViewModel.Address2 ?? string.Empty,
				Suite = addressViewModel.Suite ?? string.Empty,
				City = addressViewModel.City ?? string.Empty,
				State = addressViewModel.State ?? string.Empty,
				Zip = addressViewModel.Zip ?? string.Empty,
				Country = addressViewModel.Country ?? string.Empty,
				Phone = addressViewModel.Phone ?? string.Empty,
				ResidenceType = (ResidenceTypes)Enum.Parse(typeof(ResidenceTypes), addressViewModel.ResidenceType ?? ResidenceTypes.Residential.ToString())
			};
		}

		public static AddToCartContext ConvertToAddToCartContext(this AddToCartPostModel addToCartPostModel, Customer customer, CartTypeEnum cartType)
		{
			return new AddToCartContext()
			{
				Customer = customer,
				CartType = cartType,

				ShoppingCartRecId = addToCartPostModel.CartRecordId,
				Quantity = addToCartPostModel.Quantity,

				ProductId = addToCartPostModel.ProductId,
				VariantId = addToCartPostModel.VariantId,
				UpsellProducts = addToCartPostModel.UpsellProducts,
				IsWishlist = addToCartPostModel.IsWishlist,

				CustomerEnteredPrice = addToCartPostModel.CustomerEnteredPrice,
				Color = addToCartPostModel.Color,
				Size = addToCartPostModel.Size,
				TextOption = addToCartPostModel.TextOption,

				TemporaryImageNameStub = string.Empty,
				KitData = null,
				Composition = null
			};
		}

		public static AddToCartContext ConvertToAddToCartContext(this KitAddToCartPostModel kitAddToCartPostModel, Customer customer, CartTypeEnum cartType)
		{
			// Start with default kit data with no customer selections
			var kitData = KitProductData.Find(
				productId: kitAddToCartPostModel.ProductId,
				cartRecId: kitAddToCartPostModel.CartRecordId ?? 0,
				thisCustomer: customer);

			// Move uploaded files to their permanent location.
			kitData.TempFileStub = kitAddToCartPostModel.TemporaryImageNameStub;
			if(kitData.HasFileUploadGroup)
				kitData.MoveAllTempImagesToOrdered(); // This also updates the text option field for file uploads.

			return new AddToCartContext()
			{
				Customer = customer,
				CartType = cartType,

				ShoppingCartRecId = kitAddToCartPostModel.CartRecordId,
				Quantity = kitAddToCartPostModel.Quantity,

				ProductId = kitAddToCartPostModel.ProductId,
				VariantId = kitAddToCartPostModel.VariantId,
				UpsellProducts = kitAddToCartPostModel.UpsellProducts,
				IsWishlist = kitAddToCartPostModel.IsWishlist,

				CustomerEnteredPrice = 0,
				Color = string.Empty,
				Size = string.Empty,
				TextOption = string.Empty,

				TemporaryImageNameStub = kitAddToCartPostModel.TemporaryImageNameStub,
				KitData = kitData,
				Composition =
					BuildCompositionFromModel(
						model: kitAddToCartPostModel,
						kitData: kitData,
						customerId: customer.CustomerID)
			};
		}

		public static IEnumerable<KitItemData> GetSelectedItems(this KitAddToCartPostModel model, KitProductData kitData)
		{
			var selectedItems = new List<KitItemData>();
			foreach(var group in kitData.Groups)
			{
				// If the group is readonly add it to the list regardless of selections
				if(group.IsReadOnly)
				{
					selectedItems.AddRange(group.Items);
					continue;
				}

				var groupSelection = model.KitGroups
					.Where(g => g.Id == group.Id)
					.FirstOrDefault();

				// If this group was not selected move to the next
				if(groupSelection == null)
					continue;

				// For single select items use the SelectedItemId property from the model.
				if((group.SelectionControl == KitGroupData.SINGLE_SELECT_DROPDOWN_LIST || group.SelectionControl == KitGroupData.SINGLE_SELECT_RADIO_LIST)
					&& groupSelection.SelectedItemId > 0)
				{
					var selectedItem = group.Items
						.Where(i => i.Id == groupSelection.SelectedItemId)
						.FirstOrDefault();

					if(selectedItem != null)
						selectedItems.Add(selectedItem);
					continue;
				}

				if(groupSelection.Items == null)
					continue;

				// Otherwise we'll loop through the items to get the individual selections
				foreach(var item in group.Items)
				{
					var itemSelection = groupSelection.Items
							.Where(i => i.Id == item.Id)
							.FirstOrDefault();

					// If this item was not selected move to the next
					if(itemSelection == null)
						continue;

					// If it's a text option and there's content in the text option field add it to the selected items.
					if((group.SelectionControl == KitGroupData.TEXT_OPTION || group.SelectionControl == KitGroupData.TEXT_AREA || group.SelectionControl == KitGroupData.FILE_OPTION)
						&& !string.IsNullOrWhiteSpace(itemSelection.TextOption))
					{
						item.TextOption = itemSelection.TextOption;
						selectedItems.Add(item);
						continue;
					}
					else if(group.SelectionControl == KitGroupData.MULTI_SELECT_CHECKBOX_LIST
						&& itemSelection.IsSelected)
					{
						selectedItems.Add(item);
						continue;
					}
				}
			}

			return selectedItems;
		}

		public static NoticeType ConvertToNoticeType(this CartActionMessageType messageType)
		{
			switch(messageType)
			{
				case CartActionMessageType.Info:
					return NoticeType.Info;
				case CartActionMessageType.Failure:
					return NoticeType.Failure;
				default:
					return NoticeType.Success;
			}
		}

		public static Notice ConvertToNotice(this CartActionMessage message)
		{
			return new Notice(
				message: message.MessageText,
				type: message.MessageType.ConvertToNoticeType());
		}

		public static AjaxNoticeType ConvertToAjaxNoticeType(this CartActionMessageType messageType)
		{
			switch(messageType)
			{
				case CartActionMessageType.Info:
					return AjaxNoticeType.info;
				case CartActionMessageType.Failure:
					return AjaxNoticeType.failure;
				default:
					return AjaxNoticeType.success;
			}
		}

		public static AjaxNotice ConvertToAjaxNotice(this CartActionMessage message)
		{
			return new AjaxNotice(
				message: message.MessageText,
				type: message.MessageType.ConvertToAjaxNoticeType());
		}

		static KitComposition BuildCompositionFromModel(KitAddToCartPostModel model, KitProductData kitData, int customerId)
		{

			// Build up the kit selections from the model
			var selectedItems = model.GetSelectedItems(kitData);

			// Validate Kit options
			if(kitData.HasRequiredGroups)
			{
				foreach(var requiredGroup in kitData.Groups.Where(g => g.IsRequired))
				{
					var requirementIsMet = requiredGroup
						.Items
						.Intersect(selectedItems)
						.Any();

					if(!requirementIsMet)
						return null;
				}
			}

			// Now build the composition
			var composition = new KitComposition(0);

			foreach(var selectedItem in selectedItems)
			{
				KitCartItem kcItem = new KitCartItem();
				kcItem.ProductID = model.ProductId;
				kcItem.VariantID = model.VariantId;
				kcItem.KitGroupID = selectedItem.Group.Id;
				kcItem.KitItemID = selectedItem.Id;
				kcItem.CustomerID = customerId;
				kcItem.TextOption = selectedItem.TextOption;
				int qty = 1;
				if(selectedItem.HasMappedVariant &&
					selectedItem.InventoryQuantityDelta > 1)
				{
					qty = selectedItem.InventoryQuantityDelta;
				}

				kcItem.Quantity = qty;

				composition.Compositions.Add(kcItem);
			}

			return composition;
		}

	}

	public class ModelBuilder
	{
		readonly CartActionProvider CartActionProvider;
		readonly UrlHelper Url;
		readonly RestrictedQuantityProvider RestrictedQuantityProvider;

		public ModelBuilder(
			CartActionProvider cartActionProvider, 
			UrlHelper urlHelper,
			RestrictedQuantityProvider restrictedQuantityProvider)
		{
			CartActionProvider = cartActionProvider;
			Url = urlHelper;
			RestrictedQuantityProvider = restrictedQuantityProvider;
		}

		public AddToCartViewModel BuildAddToCartViewModel(
			UrlHelper urlHelper,
			ProductVariant variant,
			Product product,
			CartItem cartItem,
			Customer customer,
			bool showWishlistButton,
			bool colorSelectorChangesImage,
			int defaultQuantity = 1,
			string selectedSize = "",
			string selectedColor = "")
		{
			// Build our options and prompts
			var customerEntersPricePrompt = XmlCommon.GetLocaleEntry(variant.CustomerEntersPricePrompt, customer.LocaleSetting, true);
			customerEntersPricePrompt = !string.IsNullOrEmpty(customerEntersPricePrompt)
					? customerEntersPricePrompt
					: "Price";

			var textOptionPrompt = XmlCommon.GetLocaleEntry(product.TextOptionPrompt, customer.LocaleSetting, true);
			textOptionPrompt = !string.IsNullOrEmpty(textOptionPrompt)
					? textOptionPrompt
					: "Customization Text";

			var sizes = XmlCommon.GetLocaleEntry(variant.Sizes, Localization.GetDefaultLocale(), true);
			var displaySizes = XmlCommon.GetLocaleEntry(variant.Sizes, customer.LocaleSetting, true);
			var sizeOptionPrompt = XmlCommon.GetLocaleEntry(product.SizeOptionPrompt, customer.LocaleSetting, true);
			sizeOptionPrompt = !string.IsNullOrEmpty(sizeOptionPrompt)
				? sizeOptionPrompt
				: "Size";

			var colors = XmlCommon.GetLocaleEntry(variant.Colors, Localization.GetDefaultLocale(), true);
			var displayColors = XmlCommon.GetLocaleEntry(variant.Colors, customer.LocaleSetting, true);
			var colorOptionPrompt = XmlCommon.GetLocaleEntry(product.ColorOptionPrompt, customer.LocaleSetting, true);
			colorOptionPrompt = !string.IsNullOrEmpty(colorOptionPrompt)
				? colorOptionPrompt
				: "Color";

			var quantity = defaultQuantity;
			if(cartItem != null && cartItem.Quantity > 0)
				quantity = cartItem.Quantity;
			else if(variant.MinimumQuantity > 0)
				quantity = variant.MinimumQuantity;
			else if(!string.IsNullOrEmpty(AppLogic.AppConfig("DefaultAddToCartQuantity")))
				quantity = AppLogic.AppConfigUSInt("DefaultAddToCartQuantity");

			selectedSize = cartItem != null
					? AppLogic.CleanSizeColorOption(cartItem.ChosenSize)
					: AppLogic.CleanSizeColorOption(selectedSize); ;

			selectedColor = cartItem != null
					? AppLogic.CleanSizeColorOption(cartItem.ChosenColor)
					: AppLogic.CleanSizeColorOption(selectedColor);

			// If this is a single variant product, setup the PayPal ad.
			PayPalAd payPalAd = null;
			if(AppLogic.GetNextVariant(product.ProductID, variant.VariantID) == variant.VariantID)
				payPalAd = new PayPalAd(PayPalAd.TargetPage.Product);

			var variantCount = DB.GetSqlN(
				sql: "SELECT COUNT(*) AS N FROM ProductVariant WHERE Deleted = 0 and published = 1 and ProductID = @productId",
				parameters: new SqlParameter("productId", product.ProductID));

			// Now build the model
			var model = new AddToCartViewModel(
				showQuantity: (!variant.CustomerEntersPrice
                // nal
					//&& AppLogic.AppConfigBool("ShowQuantityOnProductPage")
					&& !product.IsAKit)
                    // nal
					//|| !AppLogic.AppConfigBool("HideKitQuantity")
					&& product.IsAKit,
				restrictedQuantities: RestrictedQuantityProvider.BuildRestrictedQuantityList(variant.RestrictedQuantities),
				customerEntersPrice: variant.CustomerEntersPrice,
				customerEntersPricePrompt: customerEntersPricePrompt,
				colorOptions: BuildOptionList(colors, displayColors, customer, product.TaxClassID, colorOptionPrompt),
				colorOptionPrompt: colorOptionPrompt,
				sizeOptions: BuildOptionList(sizes, displaySizes, customer, product.TaxClassID, sizeOptionPrompt),
				sizeOptionPrompt: sizeOptionPrompt,
				requiresTextOption: product.RequiresTextOption,
				showTextOption: product.RequiresTextOption
					|| !string.IsNullOrEmpty(XmlCommon.GetLocaleEntry(product.TextOptionPrompt, customer.LocaleSetting, true)),
				textOptionPrompt: textOptionPrompt,
				textOptionMaxLength: product.TextOptionMaxLength == 0
					? 50
					: product.TextOptionMaxLength,
				isCallToOrder: product.IsCalltoOrder,
                // nal
                //showBuyButton: AppLogic.AppConfigBool("ShowBuyButtons")
                showBuyButton: product.ShowBuyButton
					&& !AppLogic.HideForWholesaleSite(customer.CustomerLevelID),
                // nal
				//showWishlistButton: showWishlistButton && AppLogic.AppConfigBool("ShowWishButtons"),
				showWishlistButton: showWishlistButton,
				payPalAd: payPalAd,
				showBuySafeKicker: (!AppLogic.AppConfigBool("BuySafe.DisableAddToCartKicker")
					&& AppLogic.GlobalConfigBool("BuySafe.Enabled")
					&& AppLogic.GlobalConfig("BuySafe.Hash").Length != 0),
				buySafeKickerType: AppLogic.AppConfig("BuySafe.KickerType"),
				colorSelectorChangesImage: colorSelectorChangesImage,
				isSimpleProduct: !product.IsAKit
					&& variantCount == 1
					&& string.IsNullOrEmpty(displaySizes)
					&& string.IsNullOrEmpty(displayColors)
					&& !product.RequiresTextOption
					&& !variant.CustomerEntersPrice,
				cartType: cartItem != null
					? cartItem.CartType
					: CartTypeEnum.ShoppingCart) // if no current cart item, this won't be used, so it doesn't matter what it's set to
			{
				ProductId = variant.ProductID,
				VariantId = variant.VariantID,
				CartRecordId = cartItem != null
					? cartItem.ShoppingCartRecordID
					: 0,
				Quantity = quantity,
				CustomerEnteredPrice = (cartItem != null && variant.CustomerEntersPrice)
					? cartItem.Price
					: 0.00M,
				TextOption = cartItem != null
					? cartItem.TextOption
					: String.Empty,
				Color = selectedColor,
				Size = selectedSize,
				UpsellProducts = null,
				IsWishlist = false,
				ReturnUrl = urlHelper.MakeSafeReturnUrl(HttpContext.Current.Request.RawUrl)
			};

			return model;
		}

		SelectList BuildOptionList(string values, string texts, Customer customer, int taxClassId, string prompt)
		{
			return new SelectList(
				items: ParseSizeColorValues(values, texts, customer, taxClassId, prompt),
				dataValueField: "Value",
				dataTextField: "Text");
		}

		IEnumerable<VariantAttribute> ParseSizeColorValues(string values, string texts, Customer customer, int taxClassId, string prompt)
		{
			if(string.IsNullOrEmpty(values))
				return Enumerable.Empty<VariantAttribute>();

			var options = values.ParseAsDelimitedList()
				.Zip(texts.ParseAsDelimitedList(),
					(left, right) => new VariantAttribute()
					{
						Value = left,
						Text = FormatOptionForDisplay(right, customer, taxClassId)
					}).ToList();

			if(!AppLogic.AppConfigBool("AutoSelectFirstSizeColorOption"))
				options.Insert(0, new VariantAttribute()
				{
					Value = string.Empty,
					Text = prompt
				});

			return options.AsEnumerable();
		}

		string FormatOptionForDisplay(string option, Customer customer, int taxClassId)
		{
			if(option.Contains("["))
			{
				var name = option.Substring(0, option.IndexOf("[")).Trim();
				var priceDelta = AppLogic.GetColorAndSizePriceDelta("", option, taxClassId, customer, true, true);
				option = string.Format(
					"{0} [{1}{2}]",
					name,
					priceDelta > 0
						? "+"
						: string.Empty,
					customer.CurrencyString(priceDelta));

			}
			return option;
		}

	}
}
