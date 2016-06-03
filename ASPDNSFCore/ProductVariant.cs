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

namespace AspDotNetStorefrontCore
{
	public class ProductVariant
    {
        #region Private Variables

        private int m_variantid;
        private String m_variantguid;
        private bool m_isdefault;
        private String m_name;
        private String m_description;
        private String m_sekeywords;
        private String m_sedescription;
        private String m_sealttext;
        private String m_colors;
        private String m_colorskumodifiers;
        private String m_sizes;
        private String m_sizeskumodifiers;
        private String m_froogledescription;
        private int m_productid;
        private String m_skusuffix;
        private String m_manufacturerpartnumber;
        private Decimal m_price;
        private Decimal m_saleprice;
        private Decimal m_weight;
        private Decimal m_msrp;
        private Decimal m_cost;
        private int m_points;
        private String m_dimensions;
        private int m_inventory;
        private int m_displayorder;
        private String m_notes;
        private bool m_istaxable;
        private bool m_isshipseparately;
        private bool m_isdownload;
        private String m_downloadlocation;
		private int m_downloadValidDays;
        private int m_freeshipping;
        private bool m_published;
        private bool m_wholesale;
        private bool m_issecureattachment;
        private bool m_isrecurring;
        private int m_recurringinterval;
        private int m_recurringintervaltype;
        private int m_rewardpoints;
        private String m_sename;
        private String m_restrictedquantities;
        private int m_minimumquantity;
        private String m_extensiondata;
        private String m_extensiondata2;
        private String m_extensiondata3;
        private String m_extensiondata4;
        private String m_extensiondata5;
        private String m_imagefilenameoverride;
        private bool m_isimport;
        private bool m_deleted;
        private DateTime m_createdon;
        private bool m_customerentersprice;
        private String m_customerenterspriceprompt;
        private int m_condition;
		
        #endregion

        #region Constructors

        /// <summary>
        /// Constructor for ProductVariant that will populate the properties from the database
        /// </summary>
        /// <param name="ProductVariantID">The ID of the ProductVariant to retrieve data for</param>
        public ProductVariant(int ProductVariantID)
        {
            using (var conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();
                using (var rs = DB.GetRS("select * from dbo.ProductVariant with(NOLOCK) where VariantID = " + ProductVariantID.ToString(), conn))
                {
                    if (rs.Read())
                        Load(rs);
                }
            }
        }

        /// <summary>
        /// Constructor for ProductVariant that will populate the properties from the database
        /// </summary>
        /// <param name="rs"></param>
        public ProductVariant(IDataReader rs)
        {
            Load(rs);
        }

        #endregion

        #region Public Properties

        public int VariantID { get { return m_variantid; } set { m_variantid = value; } }
        public String VariantGUID { get { return m_variantguid; } set { m_variantguid = value; } }
        public bool IsDefault { get { return m_isdefault; } set { m_isdefault = value; } }
        public String Name { get { return m_name; } set { m_name = value; } }
        public String LocaleName { get { return LocalizedValue(m_name); } }
        public String Description { get { return m_description; } set { m_description = value; } }
        public String LocaleDescription { get { return LocalizedValue(m_description); } }
        public String SEKeywords { get { return m_sekeywords; } set { m_sekeywords = value; } }
        public String SEDescription { get { return m_sedescription; } set { m_sedescription = value; } }
        public String SEAltText { get { return m_sealttext; } set { m_sealttext = value; } }
        public String Colors { get { return m_colors; } set { m_colors = value; } }
        public String ColorSKUModifiers { get { return m_colorskumodifiers; } set { m_colorskumodifiers = value; } }
        public String Sizes { get { return m_sizes; } set { m_sizes = value; } }
        public String SizeSKUModifiers { get { return m_sizeskumodifiers; } set { m_sizeskumodifiers = value; } }
        public String FroogleDescription { get { return m_froogledescription; } set { m_froogledescription = value; } }
        public int ProductID { get { return m_productid; } set { m_productid = value; } }
        public String SKUSuffix { get { return m_skusuffix; } set { m_skusuffix = value; } }
        public String ManufacturerPartNumber { get { return m_manufacturerpartnumber; } set { m_manufacturerpartnumber = value; } }
        public Decimal Price { get { return m_price; } set { m_price = value; } }
        public Decimal SalePrice { get { return m_saleprice; } set { m_saleprice = value; } }
        public Decimal Weight { get { return m_weight; } set { m_weight = value; } }
        public Decimal MSRP { get { return m_msrp; } set { m_msrp = value; } }
        public Decimal Cost { get { return m_cost; } set { m_cost = value; } }
        public int Points { get { return m_points; } set { m_points = value; } }
        public String Dimensions { get { return m_dimensions; } set { m_dimensions = value; } }
        public int Inventory { get { return m_inventory; } set { m_inventory = value; } }
        public int DisplayOrder { get { return m_displayorder; } set { m_displayorder = value; } }
        public String Notes { get { return m_notes; } set { m_notes = value; } }
        public bool IsTaxable { get { return m_istaxable; } set { m_istaxable = value; } }
        public bool IsShipSeparately { get { return m_isshipseparately; } set { m_isshipseparately = value; } }
        public bool IsDownload { get { return m_isdownload; } set { m_isdownload = value; } }
        public String DownloadLocation { get { return m_downloadlocation; } set { m_downloadlocation = value; } }
		public int DownloadValidDays { get { return m_downloadValidDays; } set { m_downloadValidDays = value; } }
        public int FreeShipping { get { return m_freeshipping; } set { m_freeshipping = value; } }
        public bool Published { get { return m_published; } set { m_published = value; } }
        public bool Wholesale { get { return m_wholesale; } set { m_wholesale = value; } }
        public bool IsSecureAttachment { get { return m_issecureattachment; } set { m_issecureattachment = value; } }
        public bool IsRecurring { get { return m_isrecurring; } set { m_isrecurring = value; } }
        public int RecurringInterval { get { return m_recurringinterval; } set { m_recurringinterval = value; } }
        public int RecurringIntervalType { get { return m_recurringintervaltype; } set { m_recurringintervaltype = value; } }
        public int RewardPoints { get { return m_rewardpoints; } set { m_rewardpoints = value; } }
        public String SEName { get { return m_sename; } set { m_sename = value; } }
        public String RestrictedQuantities { get { return m_restrictedquantities; } set { m_restrictedquantities = value; } }
        public int MinimumQuantity { get { return m_minimumquantity; } set { m_minimumquantity = value; } }
        public String ExtensionData { get { return m_extensiondata; } set { m_extensiondata = value; } }
        public String ExtensionData2 { get { return m_extensiondata2; } set { m_extensiondata2 = value; } }
        public String ExtensionData3 { get { return m_extensiondata3; } set { m_extensiondata3 = value; } }
        public String ExtensionData4 { get { return m_extensiondata4; } set { m_extensiondata4 = value; } }
        public String ExtensionData5 { get { return m_extensiondata5; } set { m_extensiondata5 = value; } }
        public String ImageFilenameOverride { get { return m_imagefilenameoverride; } set { m_imagefilenameoverride = value; } }
        public bool IsImport { get { return m_isimport; } set { m_isimport = value; } }
        public bool Deleted { get { return m_deleted; } set { m_deleted = value; } }
        public DateTime CreatedOn { get { return m_createdon; } set { m_createdon = value; } }
        public bool CustomerEntersPrice { get { return m_customerentersprice; } set { m_customerentersprice = value; } }
        public String CustomerEntersPricePrompt { get { return m_customerenterspriceprompt; } set { m_customerenterspriceprompt = value; } }
        public int Condition { get { return m_condition; } set { m_condition = value; } }
        public bool IsFirstVariantAdded { get { return (DB.GetSqlN("select count(VariantID) as N from ProductVariant  with (NOLOCK)  where ProductID=" + m_productid.ToString() + " and Deleted=0") == 0); } }

        #endregion

        #region Public Methods

        /// <summary>
        /// Determines a localized value for the current locale for any product property when multiple locales are being used
        /// </summary>
        /// <param name="val">The unlocalized string value to localize</param>
        /// <returns>A localized string based on the current locale</returns>
        public String LocalizedValue(String val)
        {
            Customer ThisCustomer = AppLogic.GetCurrentCustomer();

            String LocalValue = val;

            if (ThisCustomer != null)
            {
                LocalValue = XmlCommon.GetLocaleEntry(val, ThisCustomer.LocaleSetting, true);
            }
            else
            {
                LocalValue = XmlCommon.GetLocaleEntry(val, Localization.GetDefaultLocale(), true);
            }

            return LocalValue;
        }

        /// <summary>
        /// Loads data from the database and populates the properties of the variant
        /// </summary>
        /// <param name="rs">IDataReader containing variant data</param>
        public void Load(IDataReader rs)
        {
            m_variantid = DB.RSFieldInt(rs, "VariantID");
            m_variantguid = DB.RSFieldGUID(rs, "VariantGUID");
            m_isdefault = DB.RSFieldBool(rs, "IsDefault");
            m_name = DB.RSField(rs, "Name");
            m_description = DB.RSField(rs, "Description");
            m_sekeywords = DB.RSField(rs, "SEKeywords");
            m_sedescription = DB.RSField(rs, "SEDescription");
            m_sealttext = DB.RSField(rs, "SEAltText");
            m_colors = DB.RSField(rs, "Colors");
            m_colorskumodifiers = DB.RSField(rs, "ColorSKUModifiers");
            m_sizes = DB.RSField(rs, "Sizes");
            m_sizeskumodifiers = DB.RSField(rs, "SizeSKUModifiers");
            m_froogledescription = DB.RSField(rs, "FroogleDescription");
            m_productid = DB.RSFieldInt(rs, "ProductID");
            m_skusuffix = DB.RSField(rs, "SKUSuffix");
            m_manufacturerpartnumber = DB.RSField(rs, "ManufacturerPartNumber");
            m_price = DB.RSFieldDecimal(rs, "Price");
            m_saleprice = DB.RSFieldDecimal(rs, "SalePrice");
            m_weight = DB.RSFieldDecimal(rs, "Weight");
            m_msrp = DB.RSFieldDecimal(rs, "MSRP");
            m_cost = DB.RSFieldDecimal(rs, "Cost");
            m_points = DB.RSFieldInt(rs, "Points");
            m_dimensions = DB.RSField(rs, "Dimensions");
            m_inventory = DB.RSFieldInt(rs, "Inventory");
            m_displayorder = DB.RSFieldInt(rs, "DisplayOrder");
            m_notes = DB.RSField(rs, "Notes");
            m_istaxable = DB.RSFieldBool(rs, "IsTaxable");
            m_isshipseparately = DB.RSFieldBool(rs, "IsShipSeparately");
            m_isdownload = DB.RSFieldBool(rs, "IsDownload");
            m_downloadlocation = DB.RSField(rs, "DownloadLocation");
			m_downloadValidDays = DB.RSFieldInt(rs, "DownloadValidDays");
            m_freeshipping = DB.RSFieldTinyInt(rs, "FreeShipping");
            m_published = DB.RSFieldBool(rs, "Published");
            m_wholesale = DB.RSFieldBool(rs, "Wholesale");
            m_issecureattachment = DB.RSFieldBool(rs, "IsSecureAttachment");
            m_isrecurring = DB.RSFieldBool(rs, "IsRecurring");
            m_recurringinterval = DB.RSFieldInt(rs, "RecurringInterval");
            m_recurringintervaltype = DB.RSFieldInt(rs, "RecurringIntervalType");
            m_rewardpoints = DB.RSFieldInt(rs, "RewardPoints");
            m_sename = DB.RSField(rs, "SEName");
            m_restrictedquantities = DB.RSField(rs, "RestrictedQuantities");
            m_minimumquantity = DB.RSFieldInt(rs, "MinimumQuantity");
            m_extensiondata = DB.RSField(rs, "ExtensionData");
            m_extensiondata2 = DB.RSField(rs, "ExtensionData2");
            m_extensiondata3 = DB.RSField(rs, "ExtensionData3");
            m_extensiondata4 = DB.RSField(rs, "ExtensionData4");
            m_extensiondata5 = DB.RSField(rs, "ExtensionData5");
            m_imagefilenameoverride = DB.RSField(rs, "ImageFilenameOverride");
            m_isimport = DB.RSFieldBool(rs, "IsImport");
            m_deleted = DB.RSFieldBool(rs, "Deleted");
            m_createdon = DB.RSFieldDateTime(rs, "CreatedOn");
            m_customerentersprice = DB.RSFieldBool(rs, "CustomerEntersPrice");
            m_customerenterspriceprompt = DB.RSField(rs, "CustomerEntersPricePrompt");
            m_condition = DB.RSFieldTinyInt(rs, "Condition");
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Retrieves a list of all variants for a specific product with the option to include soft deleted variants
        /// </summary>
        /// <param name="ProductID">The ID of the product to retrieve variants for</param>
        /// <param name="includeDeleted">Boolean value indicating whether soft deleted variants should be included</param>
        /// <returns>A ProductVariant List of variants for a product</returns>
        public static List<ProductVariant> GetVariants(int ProductID, bool includeDeleted)
        {
			var variants = new List<ProductVariant>();

			var sql = "select * from dbo.ProductVariant with(NOLOCK) where ProductID=" + ProductID.ToString() + CommonLogic.IIF(includeDeleted, string.Empty, " and Deleted=0");

            using (var conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (var rs = DB.GetRS(sql, conn))
                {

                    while (rs.Read())
                    {
						var pv = new ProductVariant(rs);

                        variants.Add(pv);
                    }

                    rs.Close();
                    rs.Dispose();
                }

                conn.Close();
                conn.Dispose();
            }

            return variants;
        }

        #endregion
    }

	public class InventoryItem
	{
		public int VariantId { get; private set; }
		public string Size { get; private set; }
		public string Color { get; private set; }
		public string WarehouseLocation { get; private set; }
		public string FullSku { get; private set; }
		public string VendorId { get; private set; }
		public decimal WeightDelta { get; private set; }
		public string GTIN { get; private set; }
		public int Inventory { get; private set; }

		public InventoryItem(string size, string color, int productId, int variantId)
		{
			string warehouseLocation = string.Empty;
			string fullSku = string.Empty;
			string vendorId = string.Empty;
			decimal weightDelta = decimal.Zero;
			string gtin = string.Empty;

			int inventory = AppLogic.GetInventory(productId, variantId, size, color, true, true, true, out warehouseLocation, out fullSku, out vendorId, out weightDelta, out gtin);

			VariantId = variantId;
			Size = size;
			Color = color;
			WarehouseLocation = warehouseLocation;
			FullSku = fullSku;
			VendorId = vendorId;
			WeightDelta = weightDelta;
			GTIN = gtin;
			Inventory = inventory;
		}
	}
}
