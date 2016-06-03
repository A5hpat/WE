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
using System.Text;

namespace AspDotNetStorefrontCore
{
    public class Product
    {
        #region Private Variables

        private int m_productid;
        private String m_productguid;
        private String m_name;
        private String m_summary;
        private String m_description;
        private String m_sekeywords;
        private String m_sedescription;
        private String m_misctext;
        private String m_froogledescription;
        private String m_setitle;
        private String m_sealttext;
        private String m_sizeoptionprompt;
        private String m_coloroptionprompt;
        private String m_textoptionprompt;
        private int m_producttypeid;
        private int m_taxclassid;
        private String m_sku;
        private String m_manufacturerpartnumber;
        private int m_salespromptid;
        private string m_salesprompt;
        private String m_xmlpackage;
        private int m_colwidth;
        private bool m_published;
        private bool m_wholesale;
        private bool m_requiresregistration;
        private int m_looks;
        private String m_notes;
        private int m_quantitydiscountid;
        private String m_relatedproducts;
        private String m_upsellproducts;
        private Decimal m_upsellproductdiscountpercentage;
        private String m_relateddocuments;
        private bool m_trackinventorybysizeandcolor;
        private bool m_trackinventorybysize;
        private bool m_trackinventorybycolor;
        private bool m_isakit;
        private bool m_showinproductbrowser;
        private bool m_showbuybutton;
        private String m_requiresproducts;
        private bool m_hidepriceuntilcart;
        private bool m_iscalltoorder;
        private bool m_excludefrompricefeeds;
        private bool m_requirestextoption;
        private int m_textoptionmaxlength;
        private String m_sename;
        private String m_extensiondata;
        private String m_extensiondata2;
        private String m_extensiondata3;
        private String m_extensiondata4;
        private String m_extensiondata5;
        private String m_imagefilenameoverride;
        private bool m_isimport;
        private bool m_issystem;
        private bool m_deleted;
        private DateTime m_createdon;
        private String m_warehouselocation;
        private int m_skinid;
        private String m_templatename;

        private List<ProductVariant> m_productvariants;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor for Product that will populate the properties from the database
        /// </summary>
        /// <param name="ProductID">The ID of the product to retrieve data for</param>
        public Product(int ProductID)
        {
            using (var conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();
                using (var rs = DB.GetRS("select * from dbo.Product with(NOLOCK) where ProductID=" + ProductID.ToString(), conn))
                {
                    if (rs.Read())
                        LoadFromRS(rs);
                }
            }
        }
        #endregion

        #region Public Properties

        public int ProductID { get { return m_productid; } set { m_productid = value; } }
        public String ProductGUID { get { return m_productguid; } set { m_productguid = value; } }
        public String Name { get { return m_name; } set { m_name = value; } }
        public String LocaleName { get { return LocalizedValue(m_name); } }
        public String Summary { get { return m_summary; } set { m_summary = value; } }
        public String Description { get { return m_description; } set { m_description = value; } }
        public String LocaleDescription { get { return LocalizedValue(m_description); } }
        public String SEKeywords { get { return m_sekeywords; } set { m_sekeywords = value; } }
        public String SEDescription { get { return m_sedescription; } set { m_sedescription = value; } }
        public String MiscText { get { return m_misctext; } set { m_misctext = value; } }
        public String FroogleDescription { get { return m_froogledescription; } set { m_froogledescription = value; } }
        public String SETitle { get { return m_setitle; } set { m_setitle = value; } }
        public String SEAltText { get { return m_sealttext; } set { m_sealttext = value; } }
        public String SizeOptionPrompt { get { return m_sizeoptionprompt; } set { m_sizeoptionprompt = value; } }
        public String ColorOptionPrompt { get { return m_coloroptionprompt; } set { m_coloroptionprompt = value; } }
        public String TextOptionPrompt { get { return m_textoptionprompt; } set { m_textoptionprompt = value; } }
        public int ProductTypeID { get { return m_producttypeid; } set { m_producttypeid = value; } }
        public int TaxClassID { get { return m_taxclassid; } set { m_taxclassid = value; } }
        public String SKU { get { return m_sku; } set { m_sku = value; } }
        public String ManufacturerPartNumber { get { return m_manufacturerpartnumber; } set { m_manufacturerpartnumber = value; } }
        public int SalesPromptID { get { return m_salespromptid; } set { m_salespromptid = value; } }
        public String SalesPrompt
        {
            get
            {
                if (String.IsNullOrEmpty(m_salesprompt))
                {
                    m_salesprompt = DB.GetSqlS("select Name as S from dbo.SalesPrompt with(NOLOCK) where SalesPromptID=" + m_salespromptid.ToString());
                }
                return m_salesprompt;
            }
            set
            {
                m_salesprompt = value;
            }
        }
        public String XmlPackage { get { return m_xmlpackage; } set { m_xmlpackage = value; } }
        public int ColWidth { get { return m_colwidth; } set { m_colwidth = value; } }
        public bool Published { get { return m_published; } set { m_published = value; } }
        public bool Wholesale { get { return m_wholesale; } set { m_wholesale = value; } }
        public bool RequiresRegistration { get { return m_requiresregistration; } set { m_requiresregistration = value; } }
        public int Looks { get { return m_looks; } set { m_looks = value; } }
        public String Notes { get { return m_notes; } set { m_notes = value; } }
        public int QuantityDiscountID { get { return m_quantitydiscountid; } set { m_quantitydiscountid = value; } }
        public String RelatedProducts { get { return m_relatedproducts; } set { m_relatedproducts = value; } }
        public String UpsellProducts { get { return m_upsellproducts; } set { m_upsellproducts = value; } }
        public Decimal UpsellProductDiscountPercentage { get { return m_upsellproductdiscountpercentage; } set { m_upsellproductdiscountpercentage = value; } }
        public String RelatedDocuments { get { return m_relateddocuments; } set { m_relateddocuments = value; } }
        public bool TrackInventoryBySizeAndColor { get { return m_trackinventorybysizeandcolor; } set { m_trackinventorybysizeandcolor = value; } }
        public bool TrackInventoryBySize { get { return m_trackinventorybysize; } set { m_trackinventorybysize = value; } }
        public bool TrackInventoryByColor { get { return m_trackinventorybycolor; } set { m_trackinventorybycolor = value; } }
        public bool IsAKit { get { return m_isakit; } set { m_isakit = value; } }
        public bool ShowInProductBrowser { get { return m_showinproductbrowser; } set { m_showinproductbrowser = value; } }
        public bool ShowBuyButton { get { return m_showbuybutton; } set { m_showbuybutton = value; } }
        public String RequiresProducts { get { return m_requiresproducts; } set { m_requiresproducts = value; } }
        public bool HidePriceUntilCart { get { return m_hidepriceuntilcart; } set { m_hidepriceuntilcart = value; } }
        public bool IsCalltoOrder { get { return m_iscalltoorder; } set { m_iscalltoorder = value; } }
        public bool ExcludeFromPriceFeeds { get { return m_excludefrompricefeeds; } set { m_excludefrompricefeeds = value; } }
        public bool RequiresTextOption { get { return m_requirestextoption; } set { m_requirestextoption = value; } }
        public int TextOptionMaxLength { get { return m_textoptionmaxlength; } set { m_textoptionmaxlength = value; } }
        public String SEName { get { return m_sename; } set { m_sename = value; } }
        public String ExtensionData { get { return m_extensiondata; } set { m_extensiondata = value; } }
        public String ExtensionData2 { get { return m_extensiondata2; } set { m_extensiondata2 = value; } }
        public String ExtensionData3 { get { return m_extensiondata3; } set { m_extensiondata3 = value; } }
        public String ExtensionData4 { get { return m_extensiondata4; } set { m_extensiondata4 = value; } }
        public String ExtensionData5 { get { return m_extensiondata5; } set { m_extensiondata5 = value; } }
        public String ImageFilenameOverride { get { return m_imagefilenameoverride; } set { m_imagefilenameoverride = value; } }
        public bool IsImport { get { return m_isimport; } set { m_isimport = value; } }
        public bool IsSystem { get { return m_issystem; } set { m_issystem = value; } }
        public bool Deleted { get { return m_deleted; } set { m_deleted = value; } }
        public DateTime CreatedOn { get { return m_createdon; } set { m_createdon = value; } }
        public String WarehouseLocation { get { return m_warehouselocation; } set { m_warehouselocation = value; } }
        public int SkinID { get { return m_skinid; } set { m_skinid = value; } }
        public String TemplateName { get { return m_templatename; } set { m_templatename = value; } }
        public List<ProductVariant> Variants { get { return m_productvariants; } set { m_productvariants = value; } }

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
        /// Uses current storeId via AppLogic.StoreID() if not included. Returns a bool indicating whether this Product is mapped to the requested Store. NOTE: Returns true if Product Filtering is disabled.
        /// </summary>
        public bool IsMappedToStore(int? storeId = null)
        {
            if (!AppLogic.GlobalConfigBool("AllowProductFiltering"))
                return true;

            var storeIdToCheck = storeId ?? AppLogic.StoreID();

            var countOfMappedProducts = DB.GetSqlN(
                string.Format(@"select count(*) as N from product p with (nolock)
					left join productstore ps with (nolock)
					on p.ProductID = ps.ProductID
					where p.productid = {0}
					and ps.StoreID = {1}",
                    this.ProductID,
                    storeIdToCheck
                    ));

            if (countOfMappedProducts > 0)
                return true;
            return false;
        }

        public void LoadFromRS(IDataReader rs)
        {
            m_productid = DB.RSFieldInt(rs, "ProductID");
            m_productguid = DB.RSFieldGUID(rs, "ProductGUID");
            m_name = DB.RSField(rs, "Name");
            m_summary = DB.RSField(rs, "Summary");
            m_description = DB.RSField(rs, "Description");
            m_sekeywords = DB.RSField(rs, "SEKeywords");
            m_sedescription = DB.RSField(rs, "SEDescription");
            m_misctext = DB.RSField(rs, "MiscText");
            m_froogledescription = DB.RSField(rs, "FroogleDescription");
            m_setitle = DB.RSField(rs, "SETitle");
            m_sealttext = DB.RSField(rs, "SEAltText");
            m_sizeoptionprompt = DB.RSField(rs, "SizeOptionPrompt");
            m_coloroptionprompt = DB.RSField(rs, "ColorOptionPrompt");
            m_textoptionprompt = DB.RSField(rs, "TextOptionPrompt");
            m_producttypeid = DB.RSFieldInt(rs, "ProductTypeID");
            m_taxclassid = DB.RSFieldInt(rs, "TaxClassID");
            m_sku = DB.RSField(rs, "SKU");
            m_manufacturerpartnumber = DB.RSField(rs, "ManufacturerPartNumber");
            m_salespromptid = DB.RSFieldInt(rs, "SalesPromptID");
            m_xmlpackage = DB.RSField(rs, "XmlPackage");
            m_colwidth = DB.RSFieldInt(rs, "ColWidth");
            m_published = DB.RSFieldBool(rs, "Published");
            m_wholesale = DB.RSFieldBool(rs, "Wholesale");
            m_requiresregistration = DB.RSFieldBool(rs, "RequiresRegistration");
            m_looks = DB.RSFieldInt(rs, "Looks");
            m_notes = DB.RSField(rs, "Notes");
            m_quantitydiscountid = DB.RSFieldInt(rs, "QuantityDiscountID");
            m_relatedproducts = DB.RSField(rs, "RelatedProducts");
            m_upsellproducts = DB.RSField(rs, "UpsellProducts");
            m_upsellproductdiscountpercentage = DB.RSFieldDecimal(rs, "UpsellProductDiscountPercentage");
            m_relateddocuments = DB.RSField(rs, "RelatedDocuments");
            m_trackinventorybysizeandcolor = DB.RSFieldBool(rs, "TrackInventoryBySizeAndColor");
            m_trackinventorybysize = DB.RSFieldBool(rs, "TrackInventoryBySize");
            m_trackinventorybycolor = DB.RSFieldBool(rs, "TrackInventoryByColor");
            m_isakit = DB.RSFieldBool(rs, "IsAKit");
            m_showinproductbrowser = DB.RSFieldBool(rs, "ShowInProductBrowser");
            m_showbuybutton = DB.RSFieldBool(rs, "ShowBuyButton");
            m_requiresproducts = DB.RSField(rs, "RequiresProducts");
            m_hidepriceuntilcart = DB.RSFieldBool(rs, "HidePriceUntilCart");
            m_iscalltoorder = DB.RSFieldBool(rs, "IsCalltoOrder");
            m_excludefrompricefeeds = DB.RSFieldBool(rs, "ExcludeFromPriceFeeds");
            m_requirestextoption = DB.RSFieldBool(rs, "RequiresTextOption");
            m_textoptionmaxlength = DB.RSFieldInt(rs, "TextOptionMaxLength");
            m_sename = DB.RSField(rs, "SEName");
            m_extensiondata = DB.RSField(rs, "ExtensionData");
            m_extensiondata2 = DB.RSField(rs, "ExtensionData2");
            m_extensiondata3 = DB.RSField(rs, "ExtensionData3");
            m_extensiondata4 = DB.RSField(rs, "ExtensionData4");
            m_extensiondata5 = DB.RSField(rs, "ExtensionData5");
            m_imagefilenameoverride = DB.RSField(rs, "ImageFilenameOverride");
            m_isimport = DB.RSFieldBool(rs, "IsImport");
            m_issystem = DB.RSFieldBool(rs, "IsSystem");
            m_deleted = DB.RSFieldBool(rs, "Deleted");
            m_createdon = DB.RSFieldDateTime(rs, "CreatedOn");
            m_warehouselocation = DB.RSField(rs, "WarehouseLocation");
            m_skinid = DB.RSFieldInt(rs, "SkinID");
            m_templatename = DB.RSField(rs, "TemplateName");

            //m_productvariants = GetVariants(true);
            m_productvariants = new List<ProductVariant>();
        }

        #endregion

        #region Static Methods

        public static String DisplayStockHint(string sProductID, string sVariantID, string page, string className, string renderAsElement)
        {
            Customer ThisCustomer = AppLogic.GetCurrentCustomer();

            StringBuilder results = new StringBuilder();

            InputValidator IV = new InputValidator("DisplayProductStockHint");
            int productID = IV.ValidateInt("ProductID", sProductID);
            int variantID = IV.ValidateInt("VariantID", sVariantID);

            bool trackInventoryBySizeAndColor = AppLogic.ProductTracksInventoryBySizeAndColor(productID);

            bool probablyOutOfStock = AppLogic.ProbablyOutOfStock(productID, variantID, trackInventoryBySizeAndColor, page);

            bool displayOutOfStockOnProductPages = AppLogic.AppConfigBool("DisplayOutOfStockOnProductPages");
            bool displayOutOfStockOnEntityPage = AppLogic.AppConfigBool("DisplayOutOfStockOnEntityPages");

            if (renderAsElement == string.Empty)
            {
                if (probablyOutOfStock)
                {
                    // the css is always 3 set and you can customized
                    // and create new one just pass as parameter
                    //(Default - StockHint for instock - out-stock-hint and outstock - in-stock-hint)
                    results.AppendLine();

                    // display Out of Stock
                    if (page.Equals("Product", StringComparison.InvariantCultureIgnoreCase) && displayOutOfStockOnProductPages)
                    {
                        results.Append("Out of stock");
                    }
                    else if (page.Equals("Entity", StringComparison.InvariantCultureIgnoreCase) && displayOutOfStockOnEntityPage)
                    {
                        results.Append("Out of stock");
                    }
                }
                else
                {
                    results.AppendLine();
                    // display "In Stock"
                    if (page.Equals("Product", StringComparison.InvariantCultureIgnoreCase) && displayOutOfStockOnProductPages)
                    {
                        results.Append("In stock");
                    }
                    else if (page.Equals("Entity", StringComparison.InvariantCultureIgnoreCase) && displayOutOfStockOnEntityPage)
                    {
                        results.Append("In stock");
                    }
                }
            }
            else
            {
                if (probablyOutOfStock)
                {
                    // the css is always 3 set and you can customized
                    // and create new one just pass as parameter
                    //(Default - StockHint for instock - out-stock-hint and outstock - in-stock-hint)

                    className = string.Format("{0} {1}", className, "out-stock-hint");
                    results.AppendLine();

                    // display Out of Stock
                    if (page.Equals("Product", StringComparison.InvariantCultureIgnoreCase) && displayOutOfStockOnProductPages)
                    {
                        results.AppendFormat("<{0} class=\"{1}\" >\n", renderAsElement, className);
                        results.Append("Out of stock");
                        results.AppendFormat("</{0}>\n", renderAsElement);
                    }
                    else if (page.Equals("Entity", StringComparison.InvariantCultureIgnoreCase) && displayOutOfStockOnEntityPage)
                    {
                        results.AppendFormat("<{0} class=\"{1}\" >\n", renderAsElement, className);
                        results.Append("Out of stock");
                        results.AppendFormat("</{0}>\n", renderAsElement);
                    }
                }
                else
                {
                    className = string.Format("{0} {1}", className, "in-stock-hint");
                    results.AppendLine();
                    // display "In Stock"
                    if (page.Equals("Product", StringComparison.InvariantCultureIgnoreCase) && displayOutOfStockOnProductPages)
                    {
                        results.AppendFormat("<{0} class=\"{1}\" >\n", renderAsElement, className);
                        results.Append("In stock");
                        results.AppendFormat("</{0}>\n", renderAsElement);
                    }
                    else if (page.Equals("Entity", StringComparison.InvariantCultureIgnoreCase) && displayOutOfStockOnEntityPage)
                    {
                        results.AppendFormat("<{0} class=\"{1}\" >\n", renderAsElement, className);
                        results.Append("In stock");
                        results.AppendFormat("</{0}>\n", renderAsElement);
                    }
                }
            }
            results.AppendLine();

            return results.ToString().Trim();
        }
        #endregion
    }
}
