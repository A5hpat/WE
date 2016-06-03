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
using System.Web;

namespace AspDotNetStorefrontCore
{
	/// <summary>
	/// Class containing the individual cart line item
	/// </summary>
	public class CartItem
	{
		#region Variable Declarations

		private DateTime m_CreatedOn;
		private DateTime m_NextRecurringShipDate;
		private int m_RecurringIndex;
		private int m_OriginalRecurringOrderNumber;
		private String m_RecurringSubscriptionID;
		private int m_ShoppingCartRecordID;
		private CartTypeEnum m_CartType;
		private int m_ProductID;
		private int m_VariantID;
		private bool m_IsSystem;
		private bool m_IsAKit;
		private String m_ProductName;
		private String m_VariantName;
		private String m_SKU;
		private String m_ManufacturerPartNumber;
		private int m_Quantity;
		private int m_QuantityDiscountID;
		private String m_QuantityDiscountName;
		private List<int> m_RestrictedQuantities;
		private int m_MinimumQuantity;
		private Decimal m_QuantityDiscountPercent;
		private String m_ChosenColor;
		private String m_ChosenColorSKUModifier;
		private String m_ChosenSize;
		private String m_ChosenSizeSKUModifier;
		private String m_TextOption;
		private String m_TextOptionPrompt;
		private String m_SizeOptionPrompt;
		private String m_ColorOptionPrompt;
		private String m_CustomerEntersPricePrompt;
		private Decimal m_Weight;
		private String m_Dimensions;
		private Decimal m_Price; // of one item! multiply by quantity to get this item subtotal
		private bool m_CustomerEntersPrice;
		private bool m_IsTaxable;
		private int m_TaxClassID;
		private Decimal m_TaxRate;
		private bool m_IsShipSeparately;
		private bool m_IsDownload;
		private String m_DownloadLocation;
		private bool m_FreeShipping;
		private bool m_Shippable;
		private int m_DistributorID;
		private bool m_IsRecurring;
		private int m_RecurringInterval;
		private DateIntervalTypeEnum m_RecurringIntervalType;
		private int m_ShippingAddressID;
		private int m_ShippingMethodID;
		private String m_ShippingMethod;
		private int m_BillingAddressID;
		private Address m_ShippingDetail;
		private String m_Notes;
		private String m_ExtensionData;
		private bool m_IsUpsell;
		private String m_OrderShippingDetail; // only used on ORDER items!
		private int m_RequiresCount;
		private int m_ProductTypeId;
		private String m_SEName;
		private String m_SEAltText;
		private String m_ImageFileNameOverride;
		private Customer m_ThisCustomer;
		private ShoppingCart m_ThisShoppingCart;
		private bool m_IsGift;

		// computed fields
		private decimal m_computedRegularPrice = System.Decimal.Zero;
		private decimal m_computedVatRate = System.Decimal.Zero;

		#endregion

		#region Constructors

		/// <summary>
		/// Default Constructor for CartItem
		/// </summary>
		public CartItem()
		{
			m_RestrictedQuantities = new List<int>();
		}

		/// <summary>
		/// Default Constructor for CartItem
		/// </summary>
		public CartItem(ShoppingCart cart, Customer thisCustomer)
			: this()
		{
			m_ThisShoppingCart = cart;
			m_ThisCustomer = thisCustomer;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the CreatedOn
		/// </summary>
		public DateTime CreatedOn
		{
			get { return m_CreatedOn; }
			set { m_CreatedOn = value; }
		}

		/// <summary>
		/// Gets or sets the NextREcurringShipDate
		/// </summary>
		public DateTime NextRecurringShipDate
		{
			get { return m_NextRecurringShipDate; }
			set { m_NextRecurringShipDate = value; }
		}

		/// <summary>
		/// Gets or sets the RecurringIndex
		/// </summary>
		public int RecurringIndex
		{
			get { return m_RecurringIndex; }
			set { m_RecurringIndex = value; }
		}

		/// <summary>
		/// Gets or sets the OriginalRecurringOrderNumber
		/// </summary>
		public int OriginalRecurringOrderNumber
		{
			get { return m_OriginalRecurringOrderNumber; }
			set { m_OriginalRecurringOrderNumber = value; }
		}

		/// <summary>
		/// Gets or sets the RecurringSubscriptionID
		/// </summary>
		public String RecurringSubscriptionID
		{
			get { return m_RecurringSubscriptionID; }
			set { m_RecurringSubscriptionID = value; }
		}

		/// <summary>
		/// Gets or sets the ShoppingCartRecordID
		/// </summary>
		public int ShoppingCartRecordID
		{
			get { return m_ShoppingCartRecordID; }
			set { m_ShoppingCartRecordID = value; }
		}

		/// <summary>
		/// Gets or sets the CartType
		/// </summary>
		public CartTypeEnum CartType
		{
			get { return m_CartType; }
			set { m_CartType = value; }
		}

		/// <summary>
		/// Gets or sets the ProductID
		/// </summary>
		public int ProductID
		{
			get { return m_ProductID; }
			set { m_ProductID = value; }
		}

		/// <summary>
		/// Gets or sets the VariantID
		/// </summary>
		public int VariantID
		{
			get { return m_VariantID; }
			set { m_VariantID = value; }
		}

		/// <summary>
		/// Gets or sets the IsSystem
		/// </summary>
		public bool IsSystem
		{
			get { return m_IsSystem; }
			set { m_IsSystem = value; }
		}

		/// <summary>
		/// Gets or sets the IsAKit
		/// </summary>
		public bool IsAKit
		{
			get { return m_IsAKit; }
			set { m_IsAKit = value; }
		}

		/// <summary>
		/// Gets or sets the ProductName
		/// </summary>
		public String ProductName
		{
			get { return m_ProductName; }
			set { m_ProductName = value; }
		}

		/// <summary>
		/// Gets or sets the VariantName
		/// </summary>
		public String VariantName
		{
			get { return m_VariantName; }
			set { m_VariantName = value; }
		}

		/// <summary>
		/// Gets or sets the SKU
		/// </summary>
		public String SKU
		{
			get { return m_SKU; }
			set { m_SKU = value; }
		}

		/// <summary>
		/// Gets or sets the ManufacturerPartNumber
		/// </summary>
		public String ManufacturerPartNumber
		{
			get { return m_ManufacturerPartNumber; }
			set { m_ManufacturerPartNumber = value; }
		}

		/// <summary>
		/// Gets or sets the Quantity
		/// </summary>
		public int Quantity
		{
			get { return m_Quantity; }
			set { m_Quantity = value; }
		}

		/// <summary>
		/// Gets or sets the QuantityDiscountID 
		/// </summary>
		public int QuantityDiscountID
		{
			get { return m_QuantityDiscountID; }
			set { m_QuantityDiscountID = value; }
		}

		/// <summary>
		/// Gets or sets the QuantityDiscountName
		/// </summary>
		public String QuantityDiscountName
		{
			get { return m_QuantityDiscountName; }
			set { m_QuantityDiscountName = value; }
		}

		/// <summary>
		/// Gets or sets the RestrictedQuantities
		/// </summary>
		public List<int> RestrictedQuantities
		{
			get { return m_RestrictedQuantities; }
			set { m_RestrictedQuantities = value; }
		}

		/// <summary>
		/// Gets or sets the MinimumQuantity
		/// </summary>
		public int MinimumQuantity
		{
			get { return m_MinimumQuantity; }
			set { m_MinimumQuantity = value; }
		}

		/// <summary>
		/// Gets or sets the QuantityDiscountPercent
		/// </summary>
		public Decimal QuantityDiscountPercent
		{
			get { return m_QuantityDiscountPercent; }
			set { m_QuantityDiscountPercent = value; }
		}

		/// <summary>
		/// Gets or sets the ChosenColor
		/// </summary>
		public String ChosenColor
		{
			get { return m_ChosenColor; }
			set { m_ChosenColor = value; }
		}

		/// <summary>
		/// Gets or sets the ChosenColorSKUModifier
		/// </summary>
		public String ChosenColorSKUModifier
		{
			get { return m_ChosenColorSKUModifier; }
			set { m_ChosenColorSKUModifier = value; }
		}

		/// <summary>
		/// Gets or sets the ChosenSize
		/// </summary>
		public String ChosenSize
		{
			get { return m_ChosenSize; }
			set { m_ChosenSize = value; }
		}

		/// <summary>
		/// Gets or sets the ChosenSizeSKUModifier
		/// </summary>
		public String ChosenSizeSKUModifier
		{
			get { return m_ChosenSizeSKUModifier; }
			set { m_ChosenSizeSKUModifier = value; }
		}

		/// <summary>
		/// Gets or sets the TextOption
		/// </summary>
		public String TextOption
		{
			get { return m_TextOption; }
			set { m_TextOption = value; }
		}

		/// <summary>
		/// Gets or sets the TextOptionPrompt
		/// </summary>
		public String TextOptionPrompt
		{
			get { return m_TextOptionPrompt; }
			set { m_TextOptionPrompt = value; }
		}

		/// <summary>
		/// Gets or sets the SizeOptionPrompt
		/// </summary>
		public String SizeOptionPrompt
		{
			get { return m_SizeOptionPrompt; }
			set { m_SizeOptionPrompt = value; }
		}

		/// <summary>
		/// Gets or sets the ColorOptionPrompt
		/// </summary>
		public String ColorOptionPrompt
		{
			get { return m_ColorOptionPrompt; }
			set { m_ColorOptionPrompt = value; }
		}

		/// <summary>
		/// Gets or sets the CustomerEntersPricePrompt
		/// </summary>
		public String CustomerEntersPricePrompt
		{
			get { return m_CustomerEntersPricePrompt; }
			set { m_CustomerEntersPricePrompt = value; }
		}

		/// <summary>
		/// Gets or sets the Weight
		/// </summary>
		public Decimal Weight
		{
			get { return m_Weight; }
			set { m_Weight = value; }
		}

		/// <summary>
		/// Gets or sets the Dimensions
		/// </summary>
		public String Dimensions
		{
			get { return m_Dimensions; }
			set { m_Dimensions = value; }
		}

		/// <summary>
		/// Gets or sets the Price
		/// </summary>
		public Decimal Price
		{
			get { return m_Price; }
			set { m_Price = value; }
		}

		/// <summary>
		/// Gets or sets the CustomerEntersPrice
		/// </summary>
		public bool CustomerEntersPrice
		{
			get { return m_CustomerEntersPrice; }
			set { m_CustomerEntersPrice = value; }
		}

		/// <summary>
		/// Gets or sets the IsTaxable
		/// </summary>
		public bool IsTaxable
		{
			get { return m_IsTaxable; }
			set { m_IsTaxable = value; }
		}

		/// <summary>
		/// Gets or sets the TaxClassID
		/// </summary>
		public int TaxClassID
		{
			get { return m_TaxClassID; }
			set { m_TaxClassID = value; }
		}

		/// <summary>
		/// Gets or sets the TaxRate
		/// </summary>
		public Decimal TaxRate
		{
			get { return m_TaxRate; }
			set { m_TaxRate = value; }
		}

		/// <summary>
		/// Gets or sets the IsShipSeparately
		/// </summary>
		public bool IsShipSeparately
		{
			get { return m_IsShipSeparately; }
			set { m_IsShipSeparately = value; }
		}

		/// <summary>
		/// Gets or sets the IsDownload
		/// </summary>
		public bool IsDownload
		{
			get { return m_IsDownload; }
			set { m_IsDownload = value; }
		}

		/// <summary>
		/// Gets or sets the DownloadLocation
		/// </summary>
		public String DownloadLocation
		{
			get { return m_DownloadLocation; }
			set { m_DownloadLocation = value; }
		}

		/// <summary>
		/// Gets or sets the FreeShipping
		/// </summary>
		public bool FreeShipping
		{
			get { return m_FreeShipping; }
			set { m_FreeShipping = value; }
		}

		/// <summary>
		/// Gets or sets the Shippable
		/// </summary>
		public bool Shippable
		{
			get { return m_Shippable; }
			set { m_Shippable = value; }
		}

		/// <summary>
		/// Gets or sets the DistributorID 
		/// </summary>
		public int DistributorID
		{
			get { return m_DistributorID; }
			set { m_DistributorID = value; }
		}

		/// <summary>
		/// Gets or sets the IsRecurring
		/// </summary>
		public bool IsRecurring
		{
			get { return m_IsRecurring; }
			set { m_IsRecurring = value; }
		}

		/// <summary>
		/// Gets or sets the RecurringInterval
		/// </summary>
		public int RecurringInterval
		{
			get { return m_RecurringInterval; }
			set { m_RecurringInterval = value; }
		}

		/// <summary>
		/// Gets or sets the RecurringIntervalType
		/// </summary>
		public DateIntervalTypeEnum RecurringIntervalType
		{
			get { return m_RecurringIntervalType; }
			set { m_RecurringIntervalType = value; }
		}

		/// <summary>
		/// Gets or sets the ShippingAddressID
		/// </summary>
		public int ShippingAddressID
		{
			get { return m_ShippingAddressID; }
			set { m_ShippingAddressID = value; }
		}

		/// <summary>
		/// Gets or sets the ShippingMethodID
		/// </summary>
		public int ShippingMethodID
		{
			get { return m_ShippingMethodID; }
			set { m_ShippingMethodID = value; }
		}

		/// <summary>
		/// Gets or sets the ShippingMethod
		/// </summary>
		public String ShippingMethod
		{
			get { return m_ShippingMethod; }
			set { m_ShippingMethod = value; }
		}

		/// <summary>
		/// Gets or sets the BillingAddressID
		/// </summary>
		public int BillingAddressID
		{
			get { return m_BillingAddressID; }
			set { m_BillingAddressID = value; }
		}

		/// <summary>
		/// Gets or sets the ShippingDetail
		/// </summary>
		public Address ShippingDetail
		{
			get { return m_ShippingDetail; }
			set { m_ShippingDetail = value; }
		}

		/// <summary>
		/// Gets or sets the Notes
		/// </summary>
		public String Notes
		{
			get { return m_Notes; }
			set { m_Notes = value; }
		}

		/// <summary>
		/// Gets or sets the ExtensionData
		/// </summary>
		public String ExtensionData
		{
			get { return m_ExtensionData; }
			set { m_ExtensionData = value; }
		}

		/// <summary>
		/// Gets or sets the IsUpsell
		/// </summary>
		public bool IsUpsell
		{
			get { return m_IsUpsell; }
			set { m_IsUpsell = value; }
		}

		/// <summary>
		/// Gets or sets the OrderShippingDetail
		/// </summary>
		public String OrderShippingDetail
		{
			get { return m_OrderShippingDetail; }
			set { m_OrderShippingDetail = value; }
		}

		/// <summary>
		/// Gets or sets the RequiresCount
		/// </summary>
		public int RequiresCount
		{
			get { return m_RequiresCount; }
			set { m_RequiresCount = value; }
		}

		/// <summary>
		/// Gets or sets the ProductTypeId
		/// </summary>
		public int ProductTypeId
		{
			get { return m_ProductTypeId; }
			set { m_ProductTypeId = value; }
		}

		/// <summary>
		/// Gets or sets the SEName
		/// </summary>
		public String SEName
		{
			get { return m_SEName; }
			set { m_SEName = value; }
		}

		/// <summary>
		/// Gets or sets the SEAltText
		/// </summary>
		public String SEAltText
		{
			get { return m_SEAltText; }
			set { m_SEAltText = value; }
		}

		/// <summary>
		/// Gets or sets the ImageFileNameOverride
		/// </summary>
		public String ImageFileNameOverride
		{
			get { return m_ImageFileNameOverride; }
			set { m_ImageFileNameOverride = value; }
		}

		/// <summary>
		/// Gets or sets the ThisCustomer
		/// </summary>
		public Customer ThisCustomer
		{
			get { return m_ThisCustomer; }
			set { m_ThisCustomer = value; }
		}

		/// <summary>
		/// Gets or sets the ThisShoppingCart
		/// </summary>
		public ShoppingCart ThisShoppingCart
		{
			get { return m_ThisShoppingCart; }
			set { m_ThisShoppingCart = value; }
		}

		/// <summary>
		/// Gets or sets the IsGift
		/// </summary>
		public bool IsGift
		{
			get { return m_IsGift; }
			set { m_IsGift = value; }
		}

		/// <summary>
		/// Gets the ProductPicURL
		/// </summary>
		public string ProductPicURL
		{
			get { return GetLineItemProductPicURL(); }
		}

		/// <summary>
		/// 
		/// </summary>
		public KitComposition KitComposition
		{
			get { return KitComposition.FromCart(ThisCustomer, CartType, ShoppingCartRecordID); }
		}

		/// <summary>
		/// 
		/// </summary>
		public string TextOptionDisplayFormat
		{
			get
			{
				return HttpContext.Current.Server.HtmlDecode(TextOption);
			}
		}

		public decimal RegularPrice
		{
			get
			{
				return m_computedRegularPrice;
			}
		}

		/// <summary>
		/// Gets the Formatted Regular Price for Display
		/// </summary>
		public string RegularPriceRateDisplayFormat
		{
			get
			{
				decimal displayPrice = this.RegularPrice;

				if(AppLogic.AppConfigBool("VAT.Enabled") && m_ThisShoppingCart.VatIsInclusive && AppLogic.AppConfigBool("VAT.RoundPerItem"))
					displayPrice = (Price + Prices.GetVATPrice(Price, 1, ThisCustomer, TaxClassID)) * Quantity;

				string vatDetails = string.Empty;

				if(AppLogic.AppConfigBool("VAT.Enabled"))
				{
					if(ThisShoppingCart.VatIsInclusive)
					{
						if(this.IsTaxable)
						{
							vatDetails = "inc vat";
						}
					}
					else
					{
						if(this.IsTaxable)
						{
							vatDetails = "Excluding VAT";
						}
					}
				}

				string priceText = Localization.CurrencyStringForDisplayWithExchangeRate(displayPrice, ThisCustomer.CurrencySetting);

				return string.Format("{0} {1}", priceText, vatDetails);
			}
		}

		/// <summary>
		/// Gets the Vat Rate
		/// </summary>
		public decimal VatRate
		{
			get
			{
				return m_computedVatRate;
			}
		}

		/// <summary>
		/// Gets the Formatted VAT rate
		/// </summary>
		public string VatRateDisplayFormat
		{
			get
			{
				string vatText = string.Empty;
				if(AppLogic.AppConfigBool("VAT.Enabled"))
				{
					if(this.m_IsTaxable)
					{

						vatText = Localization.CurrencyStringForDisplayWithExchangeRate(VatRate, ThisCustomer.CurrencySetting);
					}
					else
					{
						vatText = "HasNoTax";
					}
				}

				return vatText;
			}
		}
		#endregion

		#region Methods
		#region ComputeRates
		/// <summary>
		/// Computes the price and tax rates
		/// </summary>
		public void ComputeRates()
		{
			ComputePriceRate();
			ComputeVat();
		}

		#endregion

		#region ComputePriceRate

		void ComputePriceRate()
		{
			m_computedRegularPrice = Prices.LineItemPrice(
				cartItem: this,
				coupons: Enumerable.Empty<CouponObject>(),
				customer: ThisCustomer,
				includeDiscount: false);
		}

		#endregion

		#region ComputeVat
		/// <summary>
		/// Computes the VAT
		/// </summary>
		private void ComputeVat()
		{
			m_computedVatRate = Prices.LineItemVAT(this, ThisShoppingCart.CartItems.CouponList, ThisCustomer);
		}

		#endregion


		/// <summary>
		/// Increments the Quantity of this CartItem
		/// </summary>
		/// <param name="quantity"></param>
		public void IncrementQuantity(int quantity)
		{

			SqlParameter[] spa = {DB.CreateSQLParameter("@ProductID", SqlDbType.Int, 4, m_ProductID, ParameterDirection.Input),
								  DB.CreateSQLParameter("@VariantID", SqlDbType.Int, 4, m_VariantID, ParameterDirection.Input),
								  DB.CreateSQLParameter("@ShoppingCartRecID", SqlDbType.Int, 4, m_ShoppingCartRecordID, ParameterDirection.Input),
								  DB.CreateSQLParameter("@Quantity", SqlDbType.Int, 4, quantity, ParameterDirection.Input),
								  DB.CreateSQLParameter("@NewQuantity", SqlDbType.Int, 4, null, ParameterDirection.Output)
								 };
			this.m_Quantity = DB.ExecuteStoredProcInt("dbo.aspdnsf_UpdateCartItemQuantity", spa, null);
		}

		/// <summary>
		/// Gets the icon image for the cart item
		/// </summary>
		/// <returns></returns>
		private string GetLineItemProductPicURL()
		{
			var pictureUrl = AppLogic.LookupImage("Variant", VariantID, "icon", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);

			if(string.IsNullOrEmpty(pictureUrl) || pictureUrl.Contains("nopicture"))
			{
				if(!string.IsNullOrEmpty(ChosenColor))  //Try to get the color-specific image
					pictureUrl = AppLogic.LookupProductImageByNumberAndColor(ProductID,
						ThisCustomer.SkinID,
						m_ImageFileNameOverride,
						SKU,
						ThisCustomer.LocaleSetting,
						1,
						AppLogic.RemoveAttributePriceModifier(ChosenColor),
						"icon");

				if(string.IsNullOrEmpty(pictureUrl) || pictureUrl.Contains("nopicture"))    //If we still have nothing, fall back to the default product image
					pictureUrl = AppLogic.LookupImage("Product",
						ProductID,
						m_ImageFileNameOverride,
						SKU,
						"icon",
						ThisCustomer.SkinID,
						ThisCustomer.LocaleSetting);
			}

			return pictureUrl;
		}

		public Boolean MatchesComposition(CartItem ci)
		{
			if(ci.ProductID != ProductID || VariantID != ci.VariantID)
				return false;

			if(CustomerEntersPrice || ci.CustomerEntersPrice)
				return false;

			if(!ChosenColor.Equals(ci.ChosenColor))
				return false;

			if(!ChosenSize.Equals(ci.ChosenSize))
				return false;

			if(!TextOption.Equals(ci.TextOption))
				return false;

			if(ShippingAddressID != ci.ShippingAddressID)
				return false;

			if((IsAKit || ci.IsAKit) && !KitComposition.Matches(ci.KitComposition))
				return false;

			if(Price != ci.Price)
				return false;

			if(CartType != ci.CartType)
				return false;

			return true;
		}

		#endregion
	}

	/// <summary>
	/// Collection class for the CartItems
	/// </summary>
	public class CartItemCollection : List<CartItem>
	{
		#region Internal Variables

		internal List<CouponObject> m_couponlist;

		#endregion

		#region Constructors

		public CartItemCollection()
		{
			m_couponlist = new List<CouponObject>();
		}

		public CartItemCollection(IEnumerable<CartItem> collection)
			: base(collection)
		{
			m_couponlist = new List<CouponObject>();
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets or sets a list of <c>CouponObject</c> that belong to this <c>CartItemCollection</c>
		/// </summary>
		public List<CouponObject> CouponList
		{
			get { return m_couponlist; }
			set { m_couponlist = value; }
		}

		/// <summary>
		///  Gets whether or not the collection of shopping cart items have any discount results.
		/// </summary>
		public Boolean HasDiscountResults
		{
			get
			{
				var firstCartItem = this.FirstItem();

				return firstCartItem != null && firstCartItem.ThisShoppingCart != null
					&& firstCartItem.ThisShoppingCart.DiscountResults != null
					&& firstCartItem.ThisShoppingCart.DiscountResults.Count > 0;
			}
		}

		/// <summary>
		///  Gets the list of discount results for the collection of shopping cart items.
		/// </summary>
		public IList<AspDotNetStorefront.Promotions.IDiscountResult> DiscountResults
		{
			get
			{
				if(!HasDiscountResults)
					return new List<AspDotNetStorefront.Promotions.IDiscountResult>();

				return this.FirstItem().ThisShoppingCart.DiscountResults;
			}
		}

		/// <summary>
		/// Gets a value indicating whether all <c>CartItem</c> in the <c>CartItemCollection</c> are email <c>GiftCard</c>
		/// </summary>
		public bool IsAllEmailGiftCards
		{
			get { return this.IsAllEmailGiftCards(); }
		}

		/// <summary>
		/// Gets a value indicating whether any <c>CartItem</c> in the <c>CartItemCollection</c> is a download <c>CartItem</c>
		/// </summary>
		public bool HasDownloadComponents
		{
			get { return this.Exists(ci => ci.IsDownload); }
		}

		/// <summary>
		/// Gets a value indicating whether every <c>CartItem</c> in the <c>CartItemCollection</c> is a download <c>CartItem</c>
		/// </summary>
		public bool IsAllDownloadComponents
		{
			get { return this.IsAllDownloadComponents(); }
		}

		/// <summary>
		/// Gets a value indicating whether every <c>CartItem</c> in the <c>CartItemCollection</c> has free shipping
		/// </summary>
		public bool IsAllFreeShippingComponents
		{
			get { return this.IsAllFreeShippingComponents(); }
		}

		/// <summary>
		/// Gets a value indicating whether the <c>CartItemCollection</c> contains only system <c>CartItem</c>
		/// </summary>
		public bool IsAllSystemComponents
		{
			get { return this.IsAllSystemComponents(); }
		}

		/// <summary>
		/// Gets a value indicating whether any <c>CartItem</c> in the <c>CartItemCollection</c> is a <c>GiftCard</c>
		/// </summary>
		public bool ContainsGiftCard
		{
			get
			{
				// CartItemCollection inherits from List, so this.Count in C# doesn't directly
				// translate to LINQ in VB.  Use .Where(Func predicate).Count() instead...
				return this.Where(ci => GiftCard.ProductIsGiftCard(ci.ProductID)).Count() > 0;
			}
		}

		/// <summary>
		/// Gets a value indicating whether any <c>CartItem</c> in the <c>CartItemCollection</c> is recurring
		/// </summary>
		public bool ContainsRecurring
		{
			get
			{
				// CartItemCollection inherits from List, so this.Count in C# doesn't directly
				// translate to LINQ in VB.  Use .Where(Func predicate).Count() instead...
				return this.Where(ci => ci.IsRecurring).Count() > 0;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the <c>CartItemCollection</c> contains <c>CartItem</c> that will ship to different addresses
		/// </summary>
		public bool HasMultipleShippingAddresses
		{
			get { return this.Select(ci => ci.ShippingAddressID).Distinct().Count() > 1; }
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Determines the first <c>CartItem</c> in the <c>CartItemCollection</c>
		/// </summary>
		/// <returns>The first <c>CartItem</c> found in the <c>CartItemCollection</c></returns>
		public CartItem FirstItem()
		{
			// shouldn't call on an empty cart                              
			return this.FirstOrDefault();
		}

		/// <summary>
		/// Determines the shipping address id of the first <c>CartItem</c> in the <c>CartItemCollection</c>
		/// </summary>
		/// <returns>The shipping address id of the first <c>CartItem</c> in the <c>CartItemCollection</c></returns>
		public int FirstItemShippingAddressID()
		{
			return this.Where(sa => sa != null)
				.Select(sa => sa.ShippingAddressID)
				.DefaultIfEmpty(0)
				.First();
		}

		#endregion
	}

	public static class CartItemEnumerableExtensions
	{
		/// <summary>
		/// Gets a value indicating if any <c>CartItem</c> in the <c>IEnumerable<CartItem></c> has a distributor
		/// </summary>
		public static bool HasDistributorComponents(this IEnumerable<CartItem> cartItems)
		{
			return cartItems
				.Where(cartItem => cartItem.DistributorID > 0)
				.Any();
		}

		/// <summary>
		/// Gets a value indicating whether every <c>CartItem</c> in the <c>IEnumerable<CartItem></c> is a download <c>CartItem</c>
		/// </summary>
		public static bool IsAllDownloadComponents(this IEnumerable<CartItem> cartItems)
		{
			return cartItems
				.All(cartItem => cartItem.IsDownload);
		}

		/// <summary>
		/// Gets a value indicating whether the <c>IEnumerable<CartItem></c> contains only system <c>CartItem</c>
		/// </summary>
		public static bool IsAllSystemComponents(this IEnumerable<CartItem> cartItems)
		{
			return cartItems
				.All(cartItem => cartItem.IsSystem);
		}

		/// <summary>
		/// Gets a value indicating whether every <c>CartItem</c> in the <c>IEnumerable<CartItem></c> has free shipping
		/// </summary>
		public static bool IsAllFreeShippingComponents(this IEnumerable<CartItem> cartItems)
		{
			return Shipping.IsAllFreeShippingComponents(cartItems);
		}

		/// <summary>
		/// Gets a value indicating whether the <c>IEnumerable<CartItem></c> contains only <c>CartItem</c> that never require shipping
		/// </summary>
		public static bool NoShippingRequiredComponents(this IEnumerable<CartItem> cartItems)
		{
			return Shipping.NoShippingRequiredComponents(cartItems);
		}

		/// <summary>
		/// Gets a value indicating whether all <c>CartItem</c> in the <c>IEnumerable<CartItem></c> are email <c>GiftCard</c>
		/// </summary>
		public static bool IsAllEmailGiftCards(this IEnumerable<CartItem> cartItems)
		{
			return cartItems
				.All(ci => GiftCard.ProductIsEmailGiftCard(ci.ProductID));
		}
	}
}
