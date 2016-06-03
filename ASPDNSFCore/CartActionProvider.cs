// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using AspDotNetStorefront.Caching.ObjectCaching;

namespace AspDotNetStorefrontCore
{
	public class CartActionProvider
	{
		readonly UrlHelper Url;
		readonly ICachedShoppingCartProvider CachedShoppingCartProvider;
		
		public CartActionProvider(UrlHelper urlHelper, ICachedShoppingCartProvider cachedShoppingCartProvider)
		{
			Url = urlHelper;
			CachedShoppingCartProvider = cachedShoppingCartProvider;
		}

		#region Add To Cart methods

		public CartActionResponse AddItemToCart(AddToCartContext context)
		{
			CartActionResponse response;

			if(context.ShoppingCartRecId.HasValue)
			{
				// If there is a RecId - this "Add" call came in as an "Update call"
				// No need to "Force" the record to exist, but remove "faulty" RecId if it doesn't; to allow a normal "Add"
				if(ValidateItemExists((int)context.ShoppingCartRecId, context.Customer, context.CartType) != null)
					context.ShoppingCartRecId = null;
			}

			response = ValidateCustomer(context.ShoppingCartRecId ?? 0, context.Customer, context.ShoppingCartRecId.HasValue);
			if(response != null)
				return response;

			if(AppLogic.HideForWholesaleSite(context.Customer.CustomerLevelID))
				return new CartActionResponse(
					updatedCart: CachedShoppingCartProvider.Get(context.Customer, context.CartType, AppLogic.StoreID()),
					status: CartActionStatus.NotAllowed,
					messages: null);

			if(!context.Customer.IsRegistered
				&& AppLogic.AppConfigBool("DisallowAnonCustomerToCreateWishlist")
				&& context.IsWishlist)
				return new CartActionResponse(
					updatedCart: CachedShoppingCartProvider.Get(context.Customer, context.CartType, AppLogic.StoreID()),
					status: CartActionStatus.RequiresLogin,
					messages: new[]
						{
							new CartActionMessage(
								messageType: CartActionMessageType.Failure,
								messageText: "You must login to perform this action.")
						});

			// Validate Ids (productId, variantId)
			var productIdContext = ValidateProductIds(context);
			if(productIdContext.Response != null)
				return productIdContext.Response;

			var product = new Product(productIdContext.ProductId);
			var variant = new ProductVariant(productIdContext.VariantId);

			// Check if Product requires registration
			if(!context.Customer.IsRegistered
				&& product.RequiresRegistration)
				return new CartActionResponse(
					updatedCart: CachedShoppingCartProvider.Get(context.Customer, context.CartType, AppLogic.StoreID()),
					status: CartActionStatus.RequiresLogin,
					messages: new[]
						{
							new CartActionMessage(
								messageType: CartActionMessageType.Failure,
								messageText: "You must login to perform this action.")
						});

			// If product filtering is enabled, ensure product is mapped to current store
			if(!product.IsMappedToStore())
				return new CartActionResponse(
					updatedCart: CachedShoppingCartProvider.Get(context.Customer, context.CartType, AppLogic.StoreID()),
					status: CartActionStatus.ValidationErrors,
					messages: new[]
					{
						new CartActionMessage(
							messageType: CartActionMessageType.Failure,
							messageText: "The specified product is not available")
					});
			
			string chosenColor,
				chosenColorSKUModifier,
				chosenSize,
				chosenSizeSKUModifier,
				textOption;
			decimal customerEnteredPrice;

			if(context.Composition == null)
			{
				// Regular Product, so we need to validate context selections (size, color, textOption, customerEntersPrice)
				var selectionContext = ValidatedRequestedItemToAdd(context, product, variant);
				if(selectionContext.Response != null)
					return selectionContext.Response;

				customerEnteredPrice = selectionContext.CustomerEnteredPrice;
				chosenColor = selectionContext.ChosenColor;
				chosenColorSKUModifier = selectionContext.ChosenColorSKUModifier;
				chosenSize = selectionContext.ChosenSize;
				chosenSizeSKUModifier = selectionContext.ChosenSizeSKUModifier;
				textOption = selectionContext.TextOption;
			}
			else
			{
				// Kit Product, so set these to "empty values"
				customerEnteredPrice = 0;
				chosenColor = string.Empty;
				chosenColorSKUModifier = string.Empty;
				chosenSize = string.Empty;
				chosenSizeSKUModifier = string.Empty;
				textOption = string.Empty;
			}

			// Validate quantity and inventory checks
			var quantity = Math.Max(1, context.Quantity);

			response = ValidateQuantityAndInventory(
				context: context,
				product: product,
				variant: variant,
				chosenSize: chosenSize,
				chosenColor: chosenColor,
				quantity: quantity);
			if(response != null)
				return response;

			// All validation has passed. We can now perform Add or Update on the Product, accordingly

			var cart = CachedShoppingCartProvider.Get(context.Customer, context.CartType, AppLogic.StoreID());

			if(context.ShoppingCartRecId.HasValue
				&& context.ShoppingCartRecId > 0
				&& ValidateItemExists((int)context.ShoppingCartRecId, context.Customer, context.CartType) == null) // This is an Update (could be qty, size/color/textOption, or kitComposition)
			{
				// Handle possible KitComposition changes
				if(context.Composition != null)
				{
					AppLogic.ClearKitItems(cart.ThisCustomer, context.KitData.Id, context.KitData.VariantId, context.KitData.ShoppingCartRecordId);
					ProcessKitComposition(context.Composition, context.KitData.Id, context.KitData.VariantId, context.KitData.ShoppingCartRecordId, cart.ThisCustomer);
				}

				// Handle possible size, color, textOption changes
				UpdateCartRecordSelections((int)context.ShoppingCartRecId, chosenColor, chosenSize, textOption);

				// Handle possible Qty Increment or Decrement
				var cartItem = cart.CartItems.Where(ci => ci.ShoppingCartRecordID == context.ShoppingCartRecId).FirstOrDefault();
				var previousQuantity = cartItem.Quantity;
				var adjustedQty = quantity - previousQuantity;

				if(adjustedQty > 0)
				{
					// Increment primary product
					cartItem.IncrementQuantity(adjustedQty);
					// Increment Required products
					AddRequiredProductsToCart(cart, product.RequiresProducts, quantity);
					// Increment Upsell products
					AddUpsellProductsToCart(cart, context.UpsellProducts, quantity, quantity);
				}
				else if(adjustedQty < 0) // We are removing from the original quantity
					RemoveItemFromCart(cart, (int)context.ShoppingCartRecId, -adjustedQty);
			}
			else // This is a normal AddToCart
			{
				// Add the primary record
				CreateSingleCartRecord(
						customer: cart.ThisCustomer,
						cartType: cart.CartType,
						shippingAddressId: context.Customer.PrimaryShippingAddressID,
						productId: productIdContext.ProductId,
						variantId: productIdContext.VariantId,
						quantity: quantity,
						chosenColor: chosenColor,
						chosenColorSkuModifier: chosenColorSKUModifier,
						chosenSize: chosenSize,
						chosenSizeSkuModifier: chosenSizeSKUModifier,
						textOption: textOption,
						isRequired: false,
						customerEnteredPrice: customerEnteredPrice,
						preferredComposition: context.Composition);

				// Add Required products
				AddRequiredProductsToCart(cart, product.RequiresProducts, quantity);

				// Add upsell products
				AddUpsellProductsToCart(cart, context.UpsellProducts, quantity, productIdContext.ProductId);
			}

			// Now consolidate, call Event, and return
			ConsolidateCartItems(cart.ThisCustomer, cart.CartType);

			AppLogic.eventHandler("AddToCart").CallEvent(
				string.Format(
					@"&AddToCart=true
					&VariantID={0}
					&ProductID={1}
					&ChosenColor={2}
					&ChosenSize={3}",
					productIdContext.VariantId,
					productIdContext.ProductId,
					chosenColor,
					chosenSize));

			return new CartActionResponse(
					updatedCart: CachedShoppingCartProvider.Get(context.Customer, context.CartType, AppLogic.StoreID()),
					status: CartActionStatus.Success,
					messages: null);

		}
		
		void AddUpsellProductsToCart(ShoppingCart cart, string upsellProducts, int qtyToAdd, int parentProductId)
		{
			if(string.IsNullOrEmpty(upsellProducts))
				return;

			foreach(var upsellId in upsellProducts.ParseAsDelimitedList())
			{
				var upsellProductId = 0;

				upsellProductId = Localization.ParseUSInt(upsellId);
				if(upsellProductId == 0)
					continue;

				var upsellVariantId = AppLogic.GetProductsDefaultVariantID(upsellProductId);
				var upsellVariant = new ProductVariant(upsellVariantId);

				if(upsellVariantId == 0)
					continue;

				var size = string.Empty;
				var sizeSKUModifiers = string.Empty;
				var color = string.Empty;
				var colorSKUModifiers = string.Empty;

				// If there is only one size than use it
				if(!string.IsNullOrWhiteSpace(upsellVariant.Sizes) && !upsellVariant.Sizes.Contains(","))
				{
					size = upsellVariant.Sizes;
					if(!string.IsNullOrWhiteSpace(upsellVariant.SizeSKUModifiers) && upsellVariant.SizeSKUModifiers.IndexOf(',') == -1)
						sizeSKUModifiers = upsellVariant.SizeSKUModifiers;
				}

				// If there is only one color than use it
				if(!string.IsNullOrWhiteSpace(upsellVariant.Colors) && upsellVariant.Colors.IndexOf(',') == -1)
				{
					color = upsellVariant.Colors;
					if(!string.IsNullOrWhiteSpace(upsellVariant.ColorSKUModifiers) && upsellVariant.ColorSKUModifiers.IndexOf(',') == -1)
						colorSKUModifiers = upsellVariant.ColorSKUModifiers;
				}

				var upsellCartRecordId = CreateSingleCartRecord(cart.ThisCustomer, cart.CartType, cart.ThisCustomer.PrimaryShippingAddressID, upsellProductId, upsellVariantId, qtyToAdd, color, colorSKUModifiers, size, sizeSKUModifiers, string.Empty, false, 0);

				var upsellPrice = AppLogic.GetUpsellProductPrice(parentProductId, upsellProductId, cart.ThisCustomer.CustomerLevelID);

				var updateSql = @"UPDATE ShoppingCart 
					SET IsUpsell = 1, 
					ProductPrice = @UpsellPrice 
					WHERE ShoppingCartRecId = @ShoppingCartRecordId";

				var parameters = new[]
				{
					new SqlParameter("@UpsellPrice", Localization.CurrencyStringForDBWithoutExchangeRate(upsellPrice)),
					new SqlParameter("@ShoppingCartRecordId", upsellCartRecordId)
				};

				DB.ExecuteSQL(updateSql, parameters);
			}
		}

		void AddRequiredProductsToCart(ShoppingCart cart, string requiredProducts, int qtyToAdd)
		{
			// Don't add required products if this is a recurring cart
			if(cart.CartType == CartTypeEnum.RecurringCart || string.IsNullOrEmpty(requiredProducts))
				return;

			foreach(var productId in requiredProducts.ParseAsDelimitedList())
				CreateSingleCartRecord(
					customer: cart.ThisCustomer,
					cartType: cart.CartType,
					shippingAddressId: cart.ThisCustomer.PrimaryShippingAddressID,
					productId: Convert.ToInt32(productId),
					variantId: AppLogic.GetDefaultProductVariant(Convert.ToInt32(productId), true),
					quantity: qtyToAdd,
					chosenColor: string.Empty,
					chosenColorSkuModifier: string.Empty,
					chosenSize: string.Empty,
					chosenSizeSkuModifier: string.Empty,
					textOption: string.Empty,
					isRequired: true,
					customerEnteredPrice: 0);
		}

		int CreateSingleCartRecord(Customer customer, CartTypeEnum cartType, int shippingAddressId, int productId, int variantId, int quantity, string chosenColor, string chosenColorSkuModifier, string chosenSize, string chosenSizeSkuModifier, string textOption, bool isRequired, decimal customerEnteredPrice, KitComposition preferredComposition = null, bool isAGift = false)
		{
			int newRecordId;
			using(var connection = new SqlConnection(DB.GetDBConn()))
			using(var command = connection.CreateCommand())
			{
				command.CommandText = @"
					SELECT
						pv.Colors,
						pv.Sizes,
						p.TaxClassID
					FROM
						Product p(nolock)
						JOIN ProductVariant pv(nolock) ON pv.ProductId = p.ProductId
					WHERE
						pv.VariantID = @VariantId
						AND p.ProductID = @ProductId";

				command.Parameters.AddWithValue("@VariantId", variantId);
				command.Parameters.AddWithValue("@ProductId", productId);
				connection.Open();

				using(var reader = command.ExecuteReader())
					if(reader.Read())
					{
						// check for color & size price modifiers:
						var priceMod = AppLogic.GetColorAndSizePriceDelta(chosenColor, chosenSize, DB.RSFieldInt(reader, "TaxClassID"), customer, false, false);
						var isKit2 = preferredComposition != null
							? 1
							: 0;

						newRecordId = DB.ExecuteStoredProcInt(
							"dbo.aspdnsf_AddItemToCart",
							new[]
							{
								DB.CreateSQLParameter("@CustomerID", SqlDbType.Int, 4, customer.CustomerID, ParameterDirection.Input),
								DB.CreateSQLParameter("@ProductID", SqlDbType.Int, 4, productId, ParameterDirection.Input),
								DB.CreateSQLParameter("@VariantID", SqlDbType.Int, 4, variantId, ParameterDirection.Input),
								DB.CreateSQLParameter("@Quantity", SqlDbType.Int, 4, quantity, ParameterDirection.Input),
								DB.CreateSQLParameter("@ShippingAddressID", SqlDbType.Int, 4, shippingAddressId, ParameterDirection.Input),
								DB.CreateSQLParameter("@BillingAddressID", SqlDbType.Int, 4, customer.PrimaryBillingAddressID, ParameterDirection.Input),
								DB.CreateSQLParameter("@ChosenColor", SqlDbType.NVarChar, 200, chosenColor, ParameterDirection.Input),
								DB.CreateSQLParameter("@ChosenColorSKUModifier", SqlDbType.NVarChar, 200, chosenColorSkuModifier, ParameterDirection.Input),
								DB.CreateSQLParameter("@ChosenSize", SqlDbType.NVarChar, 200, chosenSize, ParameterDirection.Input),
								DB.CreateSQLParameter("@ChosenSizeSKUModifier", SqlDbType.NVarChar, 200, chosenSizeSkuModifier, ParameterDirection.Input),
								DB.CreateSQLParameter("@CleanColorOption", SqlDbType.NVarChar, 200, AppLogic.CleanSizeColorOption(chosenColor), ParameterDirection.Input),
								DB.CreateSQLParameter("@CleanSizeOption", SqlDbType.NVarChar, 200, AppLogic.CleanSizeColorOption(chosenSize), ParameterDirection.Input),
								DB.CreateSQLParameter("@ColorAndSizePriceDelta", SqlDbType.Money, 8, priceMod, ParameterDirection.Input),
								DB.CreateSQLParameter("@TextOption", SqlDbType.NText, 2000000000, textOption, ParameterDirection.Input),
								DB.CreateSQLParameter("@CartType", SqlDbType.Int, 4, ((int)cartType), ParameterDirection.Input),
								DB.CreateSQLParameter("@CustomerEnteredPrice", SqlDbType.Money, 4, customerEnteredPrice, ParameterDirection.Input),
								DB.CreateSQLParameter("@CustomerLevelID", SqlDbType.Int, 4, customer.CustomerLevelID, ParameterDirection.Input),
								DB.CreateSQLParameter("@RequiresCount", SqlDbType.Int, 4, 0, ParameterDirection.Input),
								DB.CreateSQLParameter("@IsKit2", SqlDbType.TinyInt, 4, isKit2, ParameterDirection.Input),
								DB.CreateSQLParameter("@NewShoppingCartRecID", SqlDbType.Int, 4, null, ParameterDirection.Output),
								DB.CreateSQLParameter("@StoreID", SqlDbType.Int, 4, AppLogic.StoreID(), ParameterDirection.Input),
								DB.CreateSQLParameter("@IsAGift", SqlDbType.Bit, 1, isAGift, ParameterDirection.Input)
							});

						if(AppLogic.IsAKit(productId))
							ProcessKitComposition(preferredComposition, productId, variantId, newRecordId, customer);
					}
					else
						newRecordId = 0;
			}

			return newRecordId;
		}

		public void ProcessKitComposition(KitComposition preferredComposition, int productId, int variantId, int shoppingCartRecID, Customer customer)
		{
			if(preferredComposition != null)
			{
				preferredComposition.CartID = shoppingCartRecID;
				AppLogic.ProcessKitComposition(customer, preferredComposition);
			}

			var kitPrice = AppLogic.GetKitTotalPrice(customer.CustomerID, customer.CustomerLevelID, productId, variantId, shoppingCartRecID);
			DB.ExecuteSQL(
				"update ShoppingCart set ProductPrice = @kitPrice where ShoppingCartRecID = @shoppingCartRecID",
				new SqlParameter("kitPrice", kitPrice),
				new SqlParameter("shoppingCartRecID", shoppingCartRecID));

			var kitWeight = AppLogic.GetKitTotalWeight(customer.CustomerID, customer.CustomerLevelID, productId, variantId, shoppingCartRecID);
			DB.ExecuteSQL(
				"update ShoppingCart set ProductWeight = @kitWeight where ShoppingCartRecID = @shoppingCartRecID",
				new SqlParameter("kitWeight", kitWeight),
				new SqlParameter("shoppingCartRecID", shoppingCartRecID));
		}

		#endregion

		#region Update Cart methods

		public CartActionResponse UpdateItemQuantityInCart(UpdateQuantityContext context)
		{
			CartActionResponse response;

			// Do we have an actual cart record?
			response = ValidateItemExists(context.ShoppingCartRecId, context.Customer, context.CartType);
			if(response != null)
				return response;

			// Validate customer, session, and (optionally) cart item ownership
			response = ValidateCustomer(context.ShoppingCartRecId, context.Customer, true);
			if(response != null)
				return response;

			// Validation has passed, now determine what action to take

			var cart = CachedShoppingCartProvider.Get(context.Customer, context.CartType, AppLogic.StoreID());


			var previousQuantity = cart.CartItems
				.FirstOrDefault(ci => ci.ShoppingCartRecordID == context.ShoppingCartRecId).Quantity;

			if(context.Quantity == 0) // This Update qty call is really a "Remove All" request. Handle appropriately
				return RemoveItemFromCart(cart, context.ShoppingCartRecId, previousQuantity);

			else if(context.Quantity < previousQuantity) // This Update qty call is really a "Remove Some" request. Handle appropriately
				return RemoveItemFromCart(cart, context.ShoppingCartRecId, previousQuantity - context.Quantity);

			else if(context.Quantity > previousQuantity) // This Update Qty call is really an "Add More" request. Handle appropriately
			{
				var cartItem = cart.CartItems.Where(ci => ci.ShoppingCartRecordID == context.ShoppingCartRecId).FirstOrDefault();
				var isKit = ItemIsKit(context.ShoppingCartRecId);

				return 
					AddItemToCart(
						new AddToCartContext()
						{
							Customer = context.Customer,
							CartType = context.CartType,
							ShoppingCartRecId = cartItem.ShoppingCartRecordID,
							Quantity = context.Quantity,
							ProductId = cartItem.ProductID,
							VariantId = cartItem.VariantID,
							UpsellProducts = 
								string.Join(
									",",
									FindUpsellsInCart(cart, cartItem.ShoppingCartRecordID)
										.Select(u => u.ProductID)),
							IsWishlist = context.CartType == CartTypeEnum.WishCart,
							CustomerEnteredPrice = 0,
							Color = cartItem.ChosenColor,
							Size = cartItem.ChosenSize,
							TextOption = cartItem.TextOption,
							KitData = isKit
								? KitProductData.Find(
									productId: cartItem.ProductID,
									cartRecId: cartItem.ShoppingCartRecordID,
									thisCustomer: context.Customer)
								: null,
							Composition = isKit
								? KitComposition.FromCart(
									thisCustomer: context.Customer,
									cartType: context.CartType,
									cartId: cartItem.ShoppingCartRecordID)
								: null
						});
			}
			else
			{
				// We have been asked to change the quantity, but have been given the same qty amount as the current value. Return silently
				return new CartActionResponse(
					updatedCart: CachedShoppingCartProvider.Get(context.Customer, context.CartType, AppLogic.StoreID()),
					status: CartActionStatus.Success,
					messages: null);
			}
		}

		void UpdateCartRecordSelections(int shoppingCartRecId, string chosenColor, string chosenSize, string textOption)
		{
			DB.ExecuteSQL(
				@"UPDATE ShoppingCart 
				SET ChosenColor = @ChosenColor, 
					ChosenSize = @ChosenSize, 
					TextOption = @TextOption
				WHERE ShoppingCartRecID = @ShoppingCartRecId;",
				new SqlParameter("@ShoppingCartRecId", shoppingCartRecId),
				new SqlParameter("@ChosenColor", chosenColor),
				new SqlParameter("@ChosenSize", chosenSize),
				new SqlParameter("@TextOption", textOption));
		}

		#endregion

		#region Remove From Cart methods

		public CartActionResponse RemoveItemFromCart(Customer customer, CartTypeEnum cartType, int shoppingCartRecId)
		{
			var cart = CachedShoppingCartProvider.Get(customer, cartType, AppLogic.StoreID());

			return RemoveItemFromCart(
				cart: cart,
				shoppingCartRecId: shoppingCartRecId,
				quantityToRemove: GetItemQtyInCart(shoppingCartRecId));
		}

		CartActionResponse RemoveItemFromCart(ShoppingCart cart, int shoppingCartRecId, int quantityToRemove, bool suppressEventHandler = false)
		{
			CartActionResponse response;

			// Do we have an actual cart record?
			response = ValidateItemExists(shoppingCartRecId, cart.ThisCustomer, cart.CartType);
			if(response != null)
				return response;

			// Validate customer, session, and cart item ownership
			response = ValidateCustomer(shoppingCartRecId, cart.ThisCustomer, true);
			if(response != null)
				return response;

			var cartItemToRemove = cart.CartItems.Where(c => c.ShoppingCartRecordID == shoppingCartRecId).FirstOrDefault();
			if(cartItemToRemove == null)
				return new CartActionResponse(
					updatedCart: CachedShoppingCartProvider.Get(cart.ThisCustomer, cart.CartType, AppLogic.StoreID()),
					status: CartActionStatus.Success);

			if(quantityToRemove > cartItemToRemove.Quantity)
				quantityToRemove = cartItemToRemove.Quantity;

			// If all or part of requested qty to remove is required by any other item(s), we cannot remove
			response = AreItemsToRemoveRequired(cart.ThisCustomer, cart, cartItemToRemove, quantityToRemove);
			if(response != null)
				return response;
			
			var messages = new List<CartActionMessage>();

			// Check for upsells, adjust pricing
			var product = new Product(cartItemToRemove.ProductID);
			if(product.UpsellProductDiscountPercentage > 0)
				messages.AddRange(
					ResetUpsellProductPrices(
						cart: cart,
						parentShoppingCartRecId: cartItemToRemove.ShoppingCartRecordID,
						totalQty: cartItemToRemove.Quantity,
						qtyToReset: quantityToRemove));

			// All checks have passed, we can remove the requested product
			if(cartItemToRemove.Quantity == quantityToRemove)
				DeleteSingleCartRecord(
					cart: cart,
					shoppingCartRecId: cartItemToRemove.ShoppingCartRecordID,
					isKit: ItemIsKit(shoppingCartRecId),
					productId: cartItemToRemove.ProductID,
					variantId: cartItemToRemove.VariantID,
					suppressEventHandler: suppressEventHandler);
			else
				DecrementCartRecordQuantity(
					cart: cart,
					shoppingCartRecId: cartItemToRemove.ShoppingCartRecordID,
					newQuantity: cartItemToRemove.Quantity - quantityToRemove);

			// If other products in cart were required by this product, remove them now
			messages.AddRange(
				RemoveOrphanedRequiredProductRecords(
					cart: cart,
					parentProductId: cartItemToRemove.ProductID,
					parentProductName: cartItemToRemove.ProductName,
					qtyToRemove: quantityToRemove,
					suppressEventHandler: suppressEventHandler));

			// Consolidate any remaining items
			ConsolidateCartItems(cart.ThisCustomer, cart.CartType);

			// Send response
			return new CartActionResponse(
					updatedCart: CachedShoppingCartProvider.Get(cart.ThisCustomer, cart.CartType, AppLogic.StoreID()),
					status: CartActionStatus.Success,
					messages: messages.AsEnumerable());
		}

		void DeleteSingleCartRecord(ShoppingCart cart, int shoppingCartRecId, bool isKit, int productId, int variantId, bool suppressEventHandler = false)
		{
			using(var connection = new SqlConnection(DB.GetDBConn()))
			{
				connection.Open();

				DB.ExecuteSQL(
					"DELETE FROM ShoppingCart WHERE ShoppingCartRecID = @ShoppingCartRecordId AND CustomerID = @CustomerId;",
					connection,
					new SqlParameter("@ShoppingCartRecordId", shoppingCartRecId),
					new SqlParameter("@CustomerId", cart.ThisCustomer.CustomerID));

				if(isKit)
					DB.ExecuteSQL(
						"DELETE FROM KitCart WHERE ShoppingCartRecID = @ShoppingCartRecordId AND CustomerID = @CustomerId;",
						connection,
						new SqlParameter("@ShoppingCartRecordId", shoppingCartRecId),
						new SqlParameter("@CustomerId", cart.ThisCustomer.CustomerID));

				// If cart is now empty - reset Customer.OrderOptions
				if(GetCartItemCount(cart) == 0)
					DB.ExecuteSQL("UPDATE Customer SET OrderOptions = null WHERE CustomerId = @CustomerId",
						new SqlParameter("@CustomerId", cart.ThisCustomer.CustomerID));
			}

			if(!suppressEventHandler)
				AppLogic
					.eventHandler("RemoveFromCart")
					.CallEvent(string.Format("&RemoveFromCart=true&VariantID={0}&ProductID={1}", variantId, productId));
		}

		void DecrementCartRecordQuantity(ShoppingCart cart, int shoppingCartRecId, int newQuantity)
		{
			// Enforce this Item's Minimum Quantity
			var cartItem = cart.CartItems.Where(ci => ci.ShoppingCartRecordID == shoppingCartRecId).FirstOrDefault();
			var quantity = cartItem.MinimumQuantity > newQuantity
				? cartItem.MinimumQuantity
				: newQuantity;

			using(SqlConnection connection = new SqlConnection(DB.GetDBConn()))
			{
				connection.Open();
				DB.ExecuteSQL(
					"UPDATE ShoppingCart SET Quantity = @Quantity WHERE ShoppingCartRecID = @ShoppingCartRecID;",
					connection,
					new SqlParameter("@ShoppingCartRecID", shoppingCartRecId),
					new SqlParameter("@Quantity", quantity));
			}
		}

		ICollection<CartActionMessage> RemoveOrphanedRequiredProductRecords(ShoppingCart cart, int parentProductId, string parentProductName, int qtyToRemove, bool suppressEventHandler = false)
		{
			var messages = new List<CartActionMessage>();

			using(var connection = new SqlConnection(DB.GetDBConn()))
			using(var command = connection.CreateCommand())
			{
				command.CommandText = @"
					SELECT RequiresProducts 
						FROM Product(NOLOCK) 
						WHERE ProductId = @ProductId";
				command.Parameters.AddWithValue("@ProductId", parentProductId);
				connection.Open();
				using(var reader = command.ExecuteReader())
					while(reader.Read())
					{
						var requiresProducts = DB.RSField(reader, "RequiresProducts").ParseAsDelimitedList();

						var requiresProductsInCart = cart.CartItems.Where(ci => requiresProducts.Any(rp => rp == ci.ProductID.ToString()) && ci.VariantID == AppLogic.GetProductsDefaultVariantID(ci.ProductID));

						foreach(var requireProduct in requiresProductsInCart.ToList())
						{
							RemoveItemFromCart(cart, requireProduct.ShoppingCartRecordID, qtyToRemove, suppressEventHandler);

							messages.Add(new CartActionMessage(
								CartActionMessageType.Info,
								string.Format(
                                    "{0} has also been removed as it was required by {1}.",
									requireProduct.ProductName,
									parentProductName)));
						}
					}
			}

			return messages;
		}

		ICollection<CartActionMessage> ResetUpsellProductPrices(ShoppingCart cart, int parentShoppingCartRecId, int totalQty, int qtyToReset)
		{
			var messages = new List<CartActionMessage>();
			
			var upsellProductsInCart = FindUpsellsInCart(cart, parentShoppingCartRecId);

			if(!upsellProductsInCart.Any())
				return messages;

			if(totalQty > qtyToReset)
			{
				// We are only resetting some of the Upsell prices, not all of them. So We can remove the ones that need price reset, then re-add them (as regular products, not discounted upsells). This will reset their price while retaining the remain Upsells at the discount.
				foreach(var upsellProduct in upsellProductsInCart)
				{
					RemoveItemFromCart(cart, upsellProduct.ShoppingCartRecordID, qtyToReset, true);

					var upsellVariantId = AppLogic.GetProductsDefaultVariantID(upsellProduct.ProductID);
					var upsellVariant = new ProductVariant(upsellVariantId);

					if(upsellVariantId == 0)
						continue;

					var size = string.Empty;
					var sizeSKUModifiers = string.Empty;
					var color = string.Empty;
					var colorSKUModifiers = string.Empty;

					// If there is only one size than use it
					if(!string.IsNullOrWhiteSpace(upsellVariant.Sizes) && !upsellVariant.Sizes.Contains(","))
					{
						size = upsellVariant.Sizes;
						if(!string.IsNullOrWhiteSpace(upsellVariant.SizeSKUModifiers) && !upsellVariant.SizeSKUModifiers.Contains(","))
							sizeSKUModifiers = upsellVariant.SizeSKUModifiers;
					}

					// If there is only one color than use it
					if(!string.IsNullOrWhiteSpace(upsellVariant.Colors) && !upsellVariant.Colors.Contains(","))
					{
						color = upsellVariant.Colors;
						if(!string.IsNullOrWhiteSpace(upsellVariant.ColorSKUModifiers) && !upsellVariant.ColorSKUModifiers.Contains(","))
							colorSKUModifiers = upsellVariant.ColorSKUModifiers;
					}

					CreateSingleCartRecord(cart.ThisCustomer, cart.CartType, cart.ThisCustomer.PrimaryShippingAddressID, upsellProduct.ProductID, upsellProduct.VariantID, qtyToReset, color, colorSKUModifiers, size, sizeSKUModifiers, string.Empty, false, 0);
				}
			}
			else
			{
				// We are resetting price for ALL of the Upsells, with no changes to qty; so we can perform this directly in the ShoppingCart table.
				foreach(var upsellProduct in upsellProductsInCart)
				{
					bool isOnSale;
					var productPrice = AppLogic.DetermineLevelPrice(upsellProduct.VariantID, cart.ThisCustomer.CustomerLevelID, out isOnSale);

					using(var connection = new SqlConnection(DB.GetDBConn()))
					{
						connection.Open();

						DB.ExecuteSQL(
							"UPDATE ShoppingCart SET ProductPrice = @ProductPrice WHERE ShoppingCartRecID = @ShoppingCartRecId",
							connection,
							new SqlParameter("@ProductPrice", productPrice),
							new SqlParameter("@ShoppingCartRecId", upsellProduct.ShoppingCartRecordID));
					}
				}
			}

			messages.Add(
				new CartActionMessage(
					CartActionMessageType.Info,
                    "Some Upsell product prices were updated because of removing this item."));

			return messages;
		}

		void RemoveOrphanedUpsellProductRecords(ShoppingCart cart, int parentShoppingCartRecId, int qtyToRemove, bool suppressEventHandler = false)
		{
			var upsellProductsInCart = FindUpsellsInCart(cart, parentShoppingCartRecId);

			if(upsellProductsInCart.Any())
				foreach(var upsellProduct in upsellProductsInCart)
					RemoveItemFromCart(cart, upsellProduct.ShoppingCartRecordID, qtyToRemove, suppressEventHandler);
		}
		
		#endregion

		#region Misc CartAction methods

		public void ConsolidateCartItems(Customer customer, CartTypeEnum cartType)
		{
			var cart = CachedShoppingCartProvider.Get(customer, cartType, AppLogic.StoreID());

			ConsolidateCartItems(cart, 0);
		}

		public CartActionResponse ReplaceRecurringIntervalVariantInCart(Customer customer, CartTypeEnum cartType, int shoppingCartRecId, int oldVariantId, int newVariantId)
		{
			var cart = CachedShoppingCartProvider.Get(customer, cartType, AppLogic.StoreID());
			
			var cartItem = cart.CartItems.Where(ci => ci.ShoppingCartRecordID == shoppingCartRecId).FirstOrDefault();
			if(cartItem == null)
				return new CartActionResponse(
					updatedCart: cart,
					status: CartActionStatus.ValidationErrors,
					messages: new[] {
						new CartActionMessage(CartActionMessageType.Failure, "Item not found.") });

			// Add the same item with the new variant
			CreateSingleCartRecord(
				customer: cart.ThisCustomer,
				cartType: cart.CartType,
				shippingAddressId: cart.ThisCustomer.PrimaryShippingAddressID,
				productId: cartItem.ProductID,
				variantId: newVariantId,
				quantity: cartItem.Quantity,
				chosenColor: cartItem.ChosenColor,
				chosenColorSkuModifier: cartItem.ChosenColorSKUModifier,
				chosenSize: cartItem.ChosenSize,
				chosenSizeSkuModifier: cartItem.ChosenSizeSKUModifier,
				textOption: cartItem.TextOption,
				isRequired: false,
				customerEnteredPrice: 0);

			// Now Remove the old Variant
			DeleteSingleCartRecord(cart, shoppingCartRecId, false, cartItem.ProductID, oldVariantId, true);

			return new CartActionResponse(
					updatedCart: CachedShoppingCartProvider.Get(customer, cartType, AppLogic.StoreID()),
					status: CartActionStatus.Success,
					messages: null);
		}

		public int PromotionAdd(Customer customer, CartTypeEnum cartType, int productId, int variantId, int quantity)
		{
			var product = new Product(productId);
			if(!product.IsMappedToStore())
				return 0;

			return CreateSingleCartRecord(
				customer: customer,
				cartType: cartType,
				shippingAddressId: customer.PrimaryShippingAddressID,
				productId: productId,
				variantId: variantId,
				quantity: quantity,
				chosenColor: string.Empty,
				chosenColorSkuModifier: string.Empty,
				chosenSize: string.Empty,
				chosenSizeSkuModifier: string.Empty,
				textOption: string.Empty,
				isRequired: false,
				customerEnteredPrice: 0);
		}

		public void RemoveItemsBeyondMaxAge(Customer customer, CartTypeEnum cartType, int numberOfDaysToKeep)
		{

			if(numberOfDaysToKeep <= 0 || customer.CustomerID == 0 || cartType == CartTypeEnum.RecurringCart)
				return;

			var cart = CachedShoppingCartProvider.Get(customer, cartType, AppLogic.StoreID());

			foreach(var cartItem in cart.CartItems)
				if(cartItem.CreatedOn < DateTime.Now.AddDays(-numberOfDaysToKeep))
					RemoveItemFromCart(cart, cartItem.ShoppingCartRecordID, cartItem.Quantity);
		}

		public void ClearDeletedAndUnPublishedProducts(Customer customer)
		{
			DB.ExecuteSQL(@"
				DELETE KitCart
				FROM KitCart kc(NOLOCK)
				INNER JOIN Product p(NOLOCK) ON p.ProductID = kc.ProductID
				INNER JOIN ProductVariant pv(NOLOCK) ON pv.VariantID = kc.VariantID
				WHERE 
					kc.CustomerID = @CustomerId 
					AND (
						p.Published = 0 
						OR pv.Published = 0 
						OR p.Deleted = 1 
						OR pv.Deleted = 1)",
				new SqlParameter("@CustomerId", customer.CustomerID));

			DB.ExecuteSQL(@"
				DELETE ShoppingCart
				FROM ShoppingCart sc(NOLOCK)
				INNER JOIN Product p(NOLOCK) ON p.ProductID = sc.ProductID
				INNER JOIN ProductVariant pv(NOLOCK) ON pv.VariantID = sc.VariantID
				WHERE 
					sc.CustomerID = @CustomerId 
					AND (
						p.Published = 0 
						OR pv.Published = 0 
						OR p.Deleted = 1 
						OR pv.Deleted = 1)",
				new SqlParameter("@CustomerId", customer.CustomerID));
		}

		public InventoryTrimmedReason ValidateCartQuantitiesAgainstInventory(Customer customer, CartTypeEnum cartType)
		{
			var cart = CachedShoppingCartProvider.Get(customer, cartType, AppLogic.StoreID());

			if(EnforceInventoryLevels(cart))
				return InventoryTrimmedReason.InventoryLevels;

			if(EnforceRestrictedQuantities(cart))
				return InventoryTrimmedReason.RestrictedQuantities;

			if(EnforceMinimumQuantities(cart))
				return InventoryTrimmedReason.MinimumQuantities;

			if(RemoveInvalidMicropayItems(cart))
				return InventoryTrimmedReason.NoLongerAvailable;

			if(RemoveItemsNotMappedToStore(cart))
				return InventoryTrimmedReason.NoLongerAvailable;

			return InventoryTrimmedReason.None;
		}

		#endregion

		#region Private Helper methods
		
		CartTypeEnum GetCartType(int shoppingCartRecId)
		{
			return
				(CartTypeEnum)DB.GetSqlN(
					"SELECT CartType N FROM ShoppingCart(NOLOCK) WHERE ShoppingCartRecId = @ShoppingCartRecId",
					new SqlParameter("@ShoppingCartRecId", shoppingCartRecId));
		}

		void ConsolidateCartItems(ShoppingCart cart, int round)
		{
			if(AppLogic.AppConfigBool("SurpressCartMergeCode"))
				return;

			if(!cart.CartItems.Any())
				return;

			if(round > cart.CartItems.Count)
			{
				SysLog.LogMessage("Failed to consolidate cart.", "Failed to consolidate cart.", MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
				return;
			}

			var didMerge = false;
			var sourceItemIndex = 0;

			foreach(var sourceItem in cart.CartItems)
			{
				var mergeItem = cart.CartItems.FirstOrDefault(c => c.ShoppingCartRecordID != sourceItem.ShoppingCartRecordID && c.MatchesComposition(sourceItem));
				if(mergeItem == null)
					continue;

				sourceItemIndex = cart.CartItems.FindIndex(ci => ci.ShoppingCartRecordID == sourceItem.ShoppingCartRecordID);
				mergeItem.IncrementQuantity(sourceItem.Quantity);
				DeleteSingleCartRecord(cart, sourceItem.ShoppingCartRecordID, false, sourceItem.ProductID, sourceItem.VariantID);
				didMerge = true;
				break;
			}

			if(didMerge)
			{
				cart.CartItems.RemoveAt(sourceItemIndex);
				ConsolidateCartItems(cart, round + 1);
			}
		}

		string GetVariantAttributePrompt(string productOptionPrompt, string localeSetting, VariantAttributeType attributeType)
		{
			var prompt = XmlCommon.GetLocaleEntry(productOptionPrompt, localeSetting, true);
			return !string.IsNullOrEmpty(prompt)
				? prompt
				: AppLogic.GetString(string.Format("AppConfig.{0}OptionPrompt", attributeType), localeSetting);
		}

		string GetTextOptionPrompt(Product product, string locale)
		{
			var prompt = XmlCommon.GetLocaleEntry(product.TextOptionPrompt, locale, true);
			return !string.IsNullOrEmpty(prompt)
				? prompt
				: "Customization Text";
		}

		string GetCustomerEntersPricePrompt(string customerEntersPricePrompt, string locale)
		{
			var prompt = XmlCommon.GetLocaleEntry(customerEntersPricePrompt, locale, true);
			return !string.IsNullOrEmpty(prompt)
				? prompt
				: "Price";
		}

		bool EnforceInventoryLevels(ShoppingCart cart)
		{
			if(!AppLogic.AppConfigBool("Inventory.LimitCartToQuantityOnHand"))
				return false;

			var inventoryTrimmed = false;

			// Ensure that the quantity in the cart does not exceed the quantity on hand
			using(var connection = new SqlConnection(DB.GetDBConn()))
			{
				using(var command = connection.CreateCommand())
				{
					command.CommandText = @"
					SELECT 
						sc.ShoppingCartRecID, 
						sc.ProductID, 
						sc.VariantID,
						sc.ChosenSize,
						sc.ChosenColor,
						sc.Quantity,
						p.TrackInventoryBySizeAndColor,
						p.TrackInventoryBySize,
						p.TrackInventoryByColor
					FROM 
						dbo.ShoppingCart sc(NOLOCK) 
						JOIN dbo.Product p(NOLOCK) ON sc.ProductID = p.ProductID
					WHERE 
						sc.CartType = @CartType
						AND sc.CustomerID = @CustomerId";

					command.Parameters.AddWithValue("@CartType", CartTypeEnum.ShoppingCart);
					command.Parameters.AddWithValue("@CustomerId", cart.ThisCustomer.CustomerID);

					connection.Open();
					using(var reader = command.ExecuteReader())
						while(reader.Read())
						{
							var inventoryOnHand = AppLogic.GetInventory(
								DB.RSFieldInt(reader, "ProductID"),
								DB.RSFieldInt(reader, "VariantID"),
								DB.RSField(reader, "ChosenSize"),
								DB.RSField(reader, "ChosenColor"),
								DB.RSFieldBool(reader, "TrackInventoryBySizeAndColor"),
								DB.RSFieldBool(reader, "TrackInventoryByColor"),
								DB.RSFieldBool(reader, "TrackInventoryBySize"));

							var shoppingCartRecordId = DB.RSFieldInt(reader, "ShoppingCartRecID");
							var quantity = DB.RSFieldInt(reader, "Quantity");

							if(quantity > inventoryOnHand)
							{
								inventoryTrimmed = true;
								if(inventoryOnHand <= 0)
									RemoveItemFromCart(cart, shoppingCartRecordId, quantity);
								else
									UpdateItemQuantityInCart(new UpdateQuantityContext()
									{
										Customer = cart.ThisCustomer,
										CartType = cart.CartType,
										ShoppingCartRecId = shoppingCartRecordId,
										Quantity = inventoryOnHand
									});
							}
						}
				}

				// We need to check again with items grouped together since they can be individual line items in the cart.
				// This is primarily to handle Kit items.
				bool inventoryTrimmedInIteration;
				do
				{
					inventoryTrimmedInIteration = false;

					using(var command = connection.CreateCommand())
					{
						command.CommandText = @"
							SELECT 
								MAX(sc.ShoppingCartRecID) 'ShoppingCartRecID',
								sc.ProductID, 
								sc.VariantID,
								sc.ChosenSize,
								sc.ChosenColor,
								SUM(sc.Quantity) 'Quantity',
								p.TrackInventoryBySizeAndColor,
								p.TrackInventoryBySize,
								p.TrackInventoryByColor
							FROM 
								ShoppingCart sc(NOLOCK) 
								JOIN Product p(NOLOCK) ON sc.ProductID = p.ProductID
							WHERE 
								sc.CartType = @CartType
								AND sc.CustomerID = @CustomerId
							GROUP BY 
								sc.ProductID,
								sc.VariantID,
								sc.ChosenSize,
								sc.ChosenColor,
								p.TrackInventoryBySizeAndColor,
								p.TrackInventoryBySize,
								p.TrackInventoryByColor";

						command.Parameters.AddWithValue("@CartType", (object)CartTypeEnum.ShoppingCart);
						command.Parameters.AddWithValue("@CustomerId", cart.ThisCustomer.CustomerID);

						using(var reader = command.ExecuteReader())
							while(reader.Read())
							{
								var inventoryOnHand = AppLogic.GetInventory(
									DB.RSFieldInt(reader, "ProductID"),
									DB.RSFieldInt(reader, "VariantID"),
									DB.RSField(reader, "ChosenSize"),
									DB.RSField(reader, "ChosenColor"),
									DB.RSFieldBool(reader, "TrackInventoryBySizeAndColor"),
									DB.RSFieldBool(reader, "TrackInventoryByColor"),
									DB.RSFieldBool(reader, "TrackInventoryBySize"));

								var shoppingCartRecordId = DB.RSFieldInt(reader, "ShoppingCartRecID");
								var quantity = DB.RSFieldInt(reader, "Quantity");


								if(quantity > inventoryOnHand)
								{
									inventoryTrimmed = true;
									inventoryTrimmedInIteration = true;

									var quantityOverage = quantity - inventoryOnHand;
									var mostRecentQuantity = DB.GetSqlN(
										"SELECT Quantity 'N' FROM ShoppingCart(NOLOCK) WHERE ShoppingCartRecID = @ShoppingCartRecID",
										new SqlParameter("@ShoppingCartRecID", shoppingCartRecordId));

									if(quantityOverage >= mostRecentQuantity)
										DB.ExecuteSQL(
											"DELETE FROM ShoppingCart WHERE ShoppingCartRecID = @ShoppingCartRecID",
											new SqlParameter("@ShoppingCartRecID", shoppingCartRecordId));
									else
										DB.ExecuteSQL(
											"UPDATE ShoppingCart SET Quantity = @Quantity WHERE ShoppingCartRecID = @ShoppingCartRecID",
											new SqlParameter("@ShoppingCartRecID", shoppingCartRecordId),
											new SqlParameter("@Quantity", mostRecentQuantity - quantityOverage));
								}
							}
					}

					// Keep looping through until we have a pass that does not trim any items.
				} while(inventoryTrimmedInIteration);
			}

			return inventoryTrimmed;
		}

		bool EnforceRestrictedQuantities(ShoppingCart cart)
		{
			var quantityAdjustments = cart.CartItems
				.Where(cartItem => cartItem.RestrictedQuantities.Any())
				.Where(cartItem => !cartItem.RestrictedQuantities.Contains(cartItem.Quantity))
				.Select(cartItem => new
				{
					cartItem.ShoppingCartRecordID,
					newQuantity = cartItem          // Find the closest restricted quantity that is less than the customer's desired quantity
						.RestrictedQuantities       // Default to 0 if there is none
						.Where(restrictedQuantity => restrictedQuantity < cartItem.Quantity)
						.DefaultIfEmpty(0)
						.Max(),
				})
				.ToArray();

			foreach(var quantityAdjustment in quantityAdjustments)
				UpdateItemQuantityInCart(new UpdateQuantityContext()
				{
					Customer = cart.ThisCustomer,
					CartType = cart.CartType,
					ShoppingCartRecId = quantityAdjustment.ShoppingCartRecordID,
					Quantity = quantityAdjustment.newQuantity
				});

			return quantityAdjustments.Any();
		}

		bool EnforceMinimumQuantities(ShoppingCart cart)
		{
			var inventoryAdjusted = false;

			using(var connection = new SqlConnection(DB.GetDBConn()))
			using(var command = connection.CreateCommand())
			{
				command.CommandText = @"
					SELECT
						sc.ShoppingCartRecID,
						sc.Quantity,
						pv.MinimumQuantity
					FROM
						ShoppingCart sc(NOLOCK)
						LEFT OUTER JOIN ProductVariant pv(NOLOCK) ON sc.VariantID = pv.VariantID
					WHERE
						pv.MinimumQuantity > 0
						AND sc.Quantity < pv.MinimumQuantity
						AND sc.CartType = @CartType
						AND sc.CustomerID = @CustomerId";

				command.Parameters.AddWithValue("@CartType", (int)CartTypeEnum.ShoppingCart);
				command.Parameters.AddWithValue("@CustomerId", cart.ThisCustomer.CustomerID);

				connection.Open();
				using(var reader = command.ExecuteReader())
					while(reader.Read())
					{
						var shoppingCartRecordId = DB.RSFieldInt(reader, "ShoppingCartRecID");
						var minimumQuantity = DB.RSFieldInt(reader, "MinimumQuantity");

						UpdateItemQuantityInCart(new UpdateQuantityContext()
						{
							Customer = cart.ThisCustomer,
							CartType = cart.CartType,
							ShoppingCartRecId = shoppingCartRecordId,
							Quantity = minimumQuantity
						});

						inventoryAdjusted = true;
					}
			}

			return inventoryAdjusted;
		}

		bool RemoveInvalidMicropayItems(ShoppingCart cart)
		{
			var inventoryAdjusted = false;

			if(AppLogic.MicropayIsEnabled() == false && AppLogic.GetCurrentPageType() != PageTypes.Checkout)
				foreach(var cartItem in cart.CartItems.ToArray().Where(ci => ci.ProductID == AppLogic.MicropayProductID))
				{
					RemoveItemFromCart(cart, cartItem.ShoppingCartRecordID, cartItem.Quantity);
					cart.CartItems.Remove(cartItem);
					inventoryAdjusted = true;
				}

			return inventoryAdjusted;
		}

		bool RemoveItemsNotMappedToStore(ShoppingCart cart)
		{
			var inventoryAdjusted = false;
			
			foreach(var cartItem in cart.CartItems.ToArray())
			{
				var product = new Product(cartItem.ProductID);
				if(!product.IsMappedToStore())
				{
					RemoveItemFromCart(cart, cartItem.ShoppingCartRecordID, cartItem.Quantity);
					cart.CartItems.Remove(cartItem);
					inventoryAdjusted = true;
				}
			}

			return inventoryAdjusted;
		}

		string GetValidatedAttribute(string chosenAttribute, string commaSeparatedValidAttributes, int depth)
		{
			if(string.IsNullOrEmpty(chosenAttribute))
				return string.Empty;

			if(chosenAttribute.Contains("["))
				chosenAttribute = chosenAttribute.Substring(0, chosenAttribute.IndexOf("["));
			chosenAttribute = chosenAttribute.Trim().ToLowerInvariant();

			foreach(string variantAttribute in commaSeparatedValidAttributes.ParseAsDelimitedList())
			{
				string testAttribute;
				if(variantAttribute.IndexOf("[") != -1)
					testAttribute = variantAttribute.Substring(0, variantAttribute.IndexOf("["));
				else
					testAttribute = variantAttribute;
				testAttribute = testAttribute.Trim().ToLowerInvariant();

				if(chosenAttribute == testAttribute)
					return variantAttribute;
			}

			if(chosenAttribute != WebUtility.HtmlDecode(chosenAttribute) && depth < 3)
				return GetValidatedAttribute(WebUtility.HtmlDecode(chosenAttribute), commaSeparatedValidAttributes, depth + 1);

			return string.Empty;
		}

		int GetCartItemCount(ShoppingCart cart)
		{
			return DB.GetSqlN(
				"SELECT COUNT(*) 'N' FROM ShoppingCart(NOLOCK) WHERE CustomerId = @CustomerId AND CartType = @CartType;",
				new SqlParameter("@CustomerId", cart.ThisCustomer.CustomerID),
				new SqlParameter("@CartType", (int)cart.CartType));
		}

		IEnumerable<CartItem> FindUpsellsInCart(ShoppingCart cart, int parentShoppingCartRecId)
		{
			var upsellRecIds = new List<int>();

			using(var connection = new SqlConnection(DB.GetDBConn()))
			using(var command = connection.CreateCommand())
			{
				command.CommandText= @"
					SELECT 
						ShoppingCartRecID
					FROM ShoppingCart(NOLOCK)
						WHERE CustomerID = @CustomerId 
						AND CartType = @CartType
						AND CONVERT(VARCHAR, ProductID) IN 
							(SELECT Items FROM 
								Split((SELECT
									ISNULL(p.UpsellProducts, '')
								FROM
									ShoppingCart sc(NOLOCK)
									LEFT JOIN Product p(NOLOCK) ON sc.ProductID = p.ProductID
								WHERE
									sc.ShoppingCartRecID = @ShoppingCartRecId), ','))";

				command.Parameters.AddWithValue("@CustomerId", cart.ThisCustomer.CustomerID);
				command.Parameters.AddWithValue("@CartType", (int)cart.CartType);
				command.Parameters.AddWithValue("@ShoppingCartRecId", parentShoppingCartRecId);
				connection.Open();

				using(SqlDataReader reader = command.ExecuteReader())
				{
					while(reader.Read())
						upsellRecIds.Add(DB.RSFieldInt(reader, "ShoppingCartRecId"));
				}
			}

			return cart.CartItems.Where(ci => upsellRecIds.Contains(ci.ShoppingCartRecordID));
		}

		string GetProductName(int productId)
		{
			return DB.GetSqlS(
				"SELECT ISNULL(Name, '') 'S' FROM Product(NOLOCK) WHERE ProductId = @ProductId;",
				new SqlParameter("@ProductId", productId));
		}

		bool AttributeChoiceIsValid(string attribute, string attributeList)
		{
			// Strip out localization using the default locale
			var localizedAttributeList = XmlCommon.GetLocaleEntry(attributeList, Localization.GetDefaultLocale(), true);

			// No attributes return valid
			if(string.IsNullOrEmpty(attribute) && string.IsNullOrEmpty(localizedAttributeList))
				return true;

			// One or the other is empty, but not both return invalid
			if(string.IsNullOrEmpty(attribute) || string.IsNullOrEmpty(localizedAttributeList))
				return false;

			// Remove the price modifier
			if(attribute.Contains("["))
				attribute = attribute.Substring(0, attribute.IndexOf("["));

			// Loop through looking ofr a match
			foreach(var variantAttribute in localizedAttributeList.ParseAsDelimitedList())
			{
				string testAttribute;
				if(variantAttribute.Contains("["))
					testAttribute = variantAttribute.Substring(0, variantAttribute.IndexOf("["));
				else
					testAttribute = variantAttribute;

				testAttribute = testAttribute.Trim();

				if(attribute.Equals(testAttribute, StringComparison.OrdinalIgnoreCase))
					return true;
			}

			return false;
		}

		string GetOptionSkuModifier(string option, string optionList, string skuModifierList, string localeSetting)
		{
			if(string.IsNullOrEmpty(option) || string.IsNullOrEmpty(optionList) || string.IsNullOrEmpty(skuModifierList))
				return string.Empty;

			optionList = XmlCommon.GetLocaleEntry(optionList, localeSetting, true);
			var options = optionList.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

			option = AppLogic.CleanSizeColorOption(option);

			return optionList.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
				.Zip(skuModifierList.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries),
					(left, right) => new { Option = AppLogic.CleanSizeColorOption(left), SkuModifier = right })
				.Where(skuModifierLookup => skuModifierLookup.Option.Equals(option, StringComparison.OrdinalIgnoreCase)).FirstOrDefault().SkuModifier;
		}

		int GetItemQtyInCart(int shoppingCartRecId)
		{
			return DB.GetSqlN("SELECT ISNULL(Quantity, 0) 'N' FROM ShoppingCart(NOLOCK) WHERE ShoppingCartRecID = @ShoppingCartRecID;",
				new SqlParameter("@ShoppingCartRecId", shoppingCartRecId));
		}

		bool ItemIsKit(int shoppingCartRecId)
		{
			return DB.GetSqlN(
				"SELECT COUNT(*) 'N' FROM ShoppingCart(NOLOCK) WHERE ShoppingCartRecID = @ShoppingCartRecID AND IsKit2 = 1;",
				new SqlParameter("@ShoppingCartRecID", shoppingCartRecId)) == 1;
		}

		CartActionResponse ValidateItemExists(int shoppingCartRecId, Customer customer, CartTypeEnum cartType)
		{
			var itemRowCount = DB.GetSqlN(
				"SELECT COUNT(*) 'N' FROM ShoppingCart(NOLOCK) WHERE ShoppingCartRecID = @ShoppingCartRecID;",
				new SqlParameter("@ShoppingCartRecID", shoppingCartRecId));

			if(itemRowCount == 0)
			{
				return new CartActionResponse(
					updatedCart: CachedShoppingCartProvider.Get(customer, cartType, AppLogic.StoreID()),
					status: CartActionStatus.Success,
					messages: null);
			}

			return null;
		}

		CartActionResponse ValidateCustomer(int shoppingCartRecId, Customer customer, bool enforceOwnership)
		{
			// Ensure we have a customer record
			if(customer == null) // Should never be the case as we have Mvc Filters in place to ensure Cart actions have customer record
				return CreateFailureResponse(
					updatedCart: CachedShoppingCartProvider.Get(customer, GetCartType(shoppingCartRecId), AppLogic.StoreID()),
					status: CartActionStatus.ValidationErrors,
					messageText: "Customer record was not found. Could not remove item.");

			// Ensure customer session is valid
			var sessionRowCount = DB.GetSqlN(
				"SELECT COUNT(*) 'N' FROM CustomerSession(NOLOCK) WHERE CustomerSessionID = @CustomerSessionID AND CustomerID = @CustomerID AND LoggedOut IS NULL AND ExpiresOn < GETDATE();",
				new SqlParameter("@CustomerSessionID", customer.CurrentSessionID),
				new SqlParameter("@CustomerID", customer.CustomerID));

			if(sessionRowCount > 0)
				return new CartActionResponse(
					updatedCart: CachedShoppingCartProvider.Get(customer, GetCartType(shoppingCartRecId), AppLogic.StoreID()),
					status: CartActionStatus.SessionTimeout,
					messages: new[]
						{
							new CartActionMessage(
								messageType: CartActionMessageType.Failure,
								messageText: "Your session has timed out. Please login again.")
						});

			// If requested, ensure this customer owns this cart record
			if(enforceOwnership && !customer.Owns.ShoppingCartRecord(shoppingCartRecId))
				return new CartActionResponse(
					updatedCart: CachedShoppingCartProvider.Get(customer, GetCartType(shoppingCartRecId), AppLogic.StoreID()),
					status: CartActionStatus.Forbidden,
					messages: null);

			return null;
		}

		CartActionResponse AreItemsToRemoveRequired(Customer customer, ShoppingCart cart, CartItem cartItemToRemove, int qtyToRemove)
		{
			var totalNumberOfProductsInCartThatRequireThisProduct = 0;

			// If all or part of requested qty to remove of this product is required by any other product(s), DO NOT allow removal
			using(var connection = new SqlConnection(DB.GetDBConn()))
			using(var command = connection.CreateCommand())
			{
				command.CommandText = @"
					SELECT p.RequiresProducts, sc.Quantity 
						FROM Product p(NOLOCK) 
							LEFT JOIN ShoppingCart sc(NOLOCK) ON p.ProductID = sc.ProductId 
						WHERE sc.CustomerId = @CustomerId 
						AND CartType = @CartType";

				command.Parameters.AddWithValue("@CustomerId", customer.CustomerID);
				command.Parameters.AddWithValue("@CartType", (int)cart.CartType);
				connection.Open();

				using(var reader = command.ExecuteReader())
					while(reader.Read())
						if(DB.RSField(reader, "RequiresProducts").ParseAsDelimitedList().Any(r => r == cartItemToRemove.ProductID.ToString()))
						{
							totalNumberOfProductsInCartThatRequireThisProduct += DB.RSFieldInt(reader, "Quantity");
						}
			}

			if(cartItemToRemove.Quantity - qtyToRemove < totalNumberOfProductsInCartThatRequireThisProduct)
				return CreateFailureResponse(
					updatedCart: CachedShoppingCartProvider.Get(customer, cart.CartType, AppLogic.StoreID()),
					status: CartActionStatus.NotAllowed,
					messageText: string.Format(
                        "You can not remove this item because it is required by another item in your {0}.",
						cart.CartType == CartTypeEnum.ShoppingCart
							? "shopping cart"
							: "wish list"));

			// Requested qty to remove is not required, no response yet
			return null;
		}

		ProductIdContext ValidateProductIds(AddToCartContext context)
		{
			var productIdContext = new ProductIdContext();

			productIdContext.ProductId = context.ProductId;
			productIdContext.VariantId = context.VariantId;

			if(productIdContext.ProductId == 0)
				productIdContext.ProductId = AppLogic.GetVariantProductID(productIdContext.VariantId);

			if(productIdContext.VariantId == 0)
				productIdContext.VariantId = AppLogic.GetDefaultProductVariant(productIdContext.ProductId);

			if(productIdContext.VariantId == 0 || productIdContext.ProductId == 0)
				productIdContext.Response = CreateFailureResponse(
					updatedCart: CachedShoppingCartProvider.Get(context.Customer, context.CartType, AppLogic.StoreID()),
					status: CartActionStatus.ValidationErrors,
					messageText: "Cannot add to cart. Please provide valid Product and Variant Ids.");
			else
				productIdContext.Response = null;

			return productIdContext;
		}

		SelectionContext ValidatedRequestedItemToAdd(AddToCartContext context, Product product, ProductVariant variant)
		{
			var selectionContext = new SelectionContext();

			selectionContext.CustomerEnteredPrice = context.CustomerEnteredPrice;
			selectionContext.TextOption = context.TextOption ?? string.Empty;
			selectionContext.ChosenSize = context.Size ?? string.Empty;
			selectionContext.ChosenColor = context.Color ?? string.Empty;

			// Customer enters price
			if(!AppLogic.VariantAllowsCustomerPricing(variant.VariantID))
				selectionContext.CustomerEnteredPrice = 0;

			var messages = new List<CartActionMessage>();

			if(selectionContext.CustomerEnteredPrice < 0)
			{
				var customerEntersPricePrompt = GetCustomerEntersPricePrompt(variant.CustomerEntersPricePrompt, context.Customer.LocaleSetting);
				messages.Add(new CartActionMessage(
					CartActionMessageType.Failure,
					string.Format("Please enter a {0} greater than zero", customerEntersPricePrompt.ToLower())));
			}

			if(Currency.GetDefaultCurrency() != context.Customer.CurrencySetting && selectionContext.CustomerEnteredPrice != 0)
				selectionContext.CustomerEnteredPrice = Currency.Convert(selectionContext.CustomerEnteredPrice, context.Customer.CurrencySetting, Localization.StoreCurrency());

			// Text Option
			var textOptionPrompt = GetTextOptionPrompt(product, context.Customer.LocaleSetting);

			if(product.RequiresTextOption && string.IsNullOrWhiteSpace(selectionContext.TextOption))
				messages.Add(new CartActionMessage(
					CartActionMessageType.Failure,
					string.Format("Please enter your {0}.", textOptionPrompt)));

			if(selectionContext.TextOption != null && product.TextOptionMaxLength > 0 && selectionContext.TextOption.Length > product.TextOptionMaxLength)
				messages.Add(new CartActionMessage(
					CartActionMessageType.Failure,
					string.Format("{0} must be less than or equal to {1} characters.", textOptionPrompt, product.TextOptionMaxLength)));

			// Size
			var sizePrompt = GetVariantAttributePrompt(product.SizeOptionPrompt, context.Customer.LocaleSetting, VariantAttributeType.Size);
			if(!AttributeChoiceIsValid(selectionContext.ChosenSize, variant.Sizes))
				messages.Add(new CartActionMessage(
					CartActionMessageType.Failure,
					string.Format("Please select a {0}.", sizePrompt.ToLower())));

			selectionContext.ChosenSizeSKUModifier = GetOptionSkuModifier(selectionContext.ChosenSize, variant.Sizes, variant.SizeSKUModifiers, context.Customer.LocaleSetting);

			// Color
			var colorPrompt = GetVariantAttributePrompt(product.ColorOptionPrompt, context.Customer.LocaleSetting, VariantAttributeType.Color);
			if(!AttributeChoiceIsValid(selectionContext.ChosenColor, variant.Colors))
				messages.Add(new CartActionMessage(
					CartActionMessageType.Failure,
					string.Format("Please select a {0}.", colorPrompt.ToLower())));

			selectionContext.ChosenColorSKUModifier = GetOptionSkuModifier(selectionContext.ChosenColor, variant.Colors, variant.ColorSKUModifiers, context.Customer.LocaleSetting);

			if(messages.Any())
				selectionContext.Response = new CartActionResponse(
					updatedCart: CachedShoppingCartProvider.Get(context.Customer, context.CartType, AppLogic.StoreID()),
					status: CartActionStatus.ValidationErrors,
					messages: messages.AsEnumerable());
			else
				selectionContext.Response = null;

			return selectionContext;
		}

		CartActionResponse ValidateQuantityAndInventory(
			AddToCartContext context,
			Product product,
			ProductVariant variant,
			string chosenSize,
			string chosenColor,
			int quantity)
		{
			// Inventory
			var inventoryIsLimited = AppLogic.AppConfigBool("Inventory.LimitCartToQuantityOnHand");
			var sizePrompt = GetVariantAttributePrompt(product.SizeOptionPrompt, context.Customer.LocaleSetting, VariantAttributeType.Size);
			var colorPrompt = GetVariantAttributePrompt(product.ColorOptionPrompt, context.Customer.LocaleSetting, VariantAttributeType.Color);
			
			var messages = new List<CartActionMessage>();

			if(inventoryIsLimited)
			{
				var inventory = AppLogic.GetInventory(product.ProductID, variant.VariantID, chosenSize, chosenColor);
				if(product.TrackInventoryBySizeAndColor && inventory == 0)
				{
					messages.Add(new CartActionMessage(
						CartActionMessageType.Failure,
						string.Format("{1}, {3} is currently out of stock. Please select another {0}/{2} combination.", sizePrompt, chosenSize, colorPrompt, chosenColor)));
				}
				else if(inventory == 0)
				{
					messages.Add(new CartActionMessage(
						CartActionMessageType.Failure,
                        "Sorry this item is out of stock"));
				}
				else if(inventory < quantity)
				{
					messages.Add(new CartActionMessage(
						CartActionMessageType.Failure,
                        "Your quantity exceeds our inventory on hand. Please reduce your quantity."));
				}
			}

			// Make sure that quantity meets to minimum requirements
			if(quantity < variant.MinimumQuantity)
			{
				messages.Add(new CartActionMessage(
					CartActionMessageType.Failure,
					string.Format("The minimum allowed quantity for this product is {0}", variant.MinimumQuantity)));
			}

			if(messages.Any())
				return new CartActionResponse(
					updatedCart: CachedShoppingCartProvider.Get(context.Customer, context.CartType, AppLogic.StoreID()),
					status: CartActionStatus.ValidationErrors,
					messages: messages.AsEnumerable());

			return null;
		}

		CartActionResponse CreateFailureResponse(ShoppingCart updatedCart, CartActionStatus status, string messageText)
		{
			return new CartActionResponse(
				updatedCart: updatedCart,
				status: status,
				messages: new[]
				{
					new CartActionMessage(
						CartActionMessageType.Failure,
						messageText)
				});
		}

		#endregion
	}

	public class AddToCartContext
	{
		public Customer Customer { get; set; }
		public CartTypeEnum CartType { get; set; }

		public int? ShoppingCartRecId { get; set; }
		public int Quantity { get; set; }

		public int ProductId { get; set; }
		public int VariantId { get; set; }
		public string UpsellProducts { get; set; }
		public bool IsWishlist { get; set; }

		public decimal CustomerEnteredPrice { get; set; }
		public string Color { get; set; }
		public string Size { get; set; }
		public string TextOption { get; set; }

		public string TemporaryImageNameStub { get; set; }
		public KitProductData KitData { get; set; }
		public KitComposition Composition { get; set; }
	}

	public class UpdateQuantityContext
	{
		public Customer Customer { get; set; }
		public CartTypeEnum CartType { get; set; }

		public int ShoppingCartRecId { get; set; }
		public int Quantity { get; set; }
	}

	public enum CartActionMessageType
	{
		Info,
		Failure,
		Success
	}

	public class CartActionMessage
	{
		public CartActionMessageType MessageType { get; set; }
		public string MessageText { get; set; }

		public CartActionMessage(CartActionMessageType messageType, string messageText)
		{
			MessageType = messageType;
			MessageText = messageText;
		}
	}

	public enum CartActionStatus
	{
		RequiresLogin,
		SessionTimeout,
		Forbidden,
		NotAllowed,
		ValidationErrors,
		Success
	}

	/// <summary>
	/// Response object returned by <seealso cref="CartActionProvider"/> that contains information detailing the results of an attempted Cart Action (add, edit, delete, etc.).
	/// <para/>The UpdatedCart member returns a cart object that reflects any changes made during the Provider call. IMPORTANT: Be sure to reference this cart object after any <seealso cref="CartActionProvider"/> calls to ensure you have an updated cart object.
	/// <para/>The Status member returns a <seealso cref="CartActionStatus"/> indicating the Result of the attempted Action.
	///	<para/>The Messages member returns an IEnumerable&lt;string&gt; of messages that resulted from the attempted Action.
	/// </summary>
	public class CartActionResponse
	{
		public readonly ShoppingCart UpdatedCart;
		public readonly CartActionStatus Status;
		public readonly IEnumerable<CartActionMessage> Messages;

		public CartActionResponse(ShoppingCart updatedCart, CartActionStatus status, IEnumerable<CartActionMessage> messages = null)
		{
			UpdatedCart = updatedCart;
			Status = status;
			Messages = messages == null
				? Enumerable.Empty<CartActionMessage>()
				: messages;
		}
	}

	class SelectionContext
	{
		public CartActionResponse Response { get; set; }
		public decimal CustomerEnteredPrice { get; set; }
		public string TextOption { get; set; }
		public string ChosenColor { get; set; }
		public string ChosenColorSKUModifier { get; set; }
		public string ChosenSize { get; set; }
		public string ChosenSizeSKUModifier { get; set; }
	}

	class ProductIdContext
	{
		public CartActionResponse Response { get; set; }
		public int ProductId { get; set; }
		public int VariantId { get; set; }
	}
}
