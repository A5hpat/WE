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
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace AspDotNetStorefrontCore
{
	public struct AddressInfo
    {
        public String m_NickName;
        public String m_FirstName;
        public String m_LastName;
        public String m_Company;
        public ResidenceTypes m_ResidenceType;
        public String m_Address1;
        public String m_Address2;
        public String m_Suite;
        public String m_City;
        public String m_State;
        public String m_Zip;
        public String m_Country;
        public String m_Phone;
        public String m_EMail;
    }

    public class OrderTransaction
    {
        public int OrderTransactionID { get; protected set; }
        public int OrderNumber { get; protected set; }
        public String TransactionType { get; protected set; }
        public String TransactionCommand { get; protected set; }
        public String TransactionResult { get; protected set; }
        public String PNREF { get; protected set; }
        public String Code { get; protected set; }
        public String PaymentMethod { get; protected set; }
        public String PaymentGateway { get; protected set; }
        public decimal Amount { get; protected set; }

        public OrderTransaction(int orderTransactionID, int orderNumber, String transactionType, String transactionCommand, String transactionResult, String pnref, String code, String paymentMethod, String paymentGateway, decimal amount)
        {
            this.OrderTransactionID = orderTransactionID;
            this.OrderNumber = orderNumber;
            this.TransactionType = transactionType;
            this.TransactionCommand = transactionCommand;
            this.TransactionResult = transactionResult;
            this.PNREF = pnref;
            this.Code = code;
            this.PaymentMethod = paymentMethod;
            this.PaymentGateway = paymentGateway;
            this.Amount = amount;
        }

        public static int LookupOrderNumber(String transactionType, String transactionCommand, String transactionResult, String pnref, String code, String paymentMethod, String paymentGateway)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("select ordernumber as N from ordertransaction where 1=1 ");
            if (transactionType != null)
                sql.Append(" and transactiontype = " + DB.SQuote(transactionType));
            if (transactionCommand != null)
                sql.Append(" and transactionCommand = " + DB.SQuote(transactionCommand));
            if (transactionResult != null)
                sql.Append(" and transactionResult = " + DB.SQuote(transactionResult));
            if (pnref != null)
                sql.Append(" and pnref = " + DB.SQuote(pnref));
            if (code != null)
                sql.Append(" and code = " + DB.SQuote(code));
            if (paymentMethod != null)
                sql.Append(" and paymentMethod = " + DB.SQuote(paymentMethod));
            if (paymentGateway != null)
                sql.Append(" and paymentGateway = " + DB.SQuote(paymentGateway));

            sql.Append(" order by ordernumber desc");

            return DB.GetSqlN(sql.ToString());
        }
    }

    public class OrderTransactionCollection
    {
        private int m_OrderNumber;
        public List<OrderTransaction> Transactions { get; protected set; }

        public OrderTransactionCollection(int orderNumber)
        {
            m_OrderNumber = orderNumber;
            LoadFromDB(orderNumber);
        }

        private void LoadFromDB(int orderNumber)
        {
            Transactions = new List<OrderTransaction>();

            using (var con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (var rs = DB.GetRS("select * from OrderTransaction where orderNumber = {0}".FormatWith(orderNumber), con))
                {
                    while (rs.Read())
                    {
                        Transactions.Add(new OrderTransaction(
                            DB.RSFieldInt(rs, "OrderTransactionID"),
                            DB.RSFieldInt(rs, "orderNumber"),
                            DB.RSField(rs, "TransactionType"),
                            DB.RSField(rs, "TransactionCommand"),
                            DB.RSField(rs, "TransactionResult"),
                            DB.RSField(rs, "PNREF"),
                            DB.RSField(rs, "Code"),
                            DB.RSField(rs, "PaymentMethod"),
                            DB.RSField(rs, "PaymentGateway"),
                            DB.RSFieldDecimal(rs, "Amount")
                        ));
                    }
                }
            }
        }

        public void AddTransaction(String transactionType, String transactionCommand, String transactionResult, String pnref, String code, String paymentMethod, String paymentGateway, decimal amount)
        {
            string insertStatement = "INSERT INTO [OrderTransaction]([orderNumber],[TransactionType],[TransactionCommand],[TransactionResult],[PNREF],[Code],[PaymentMethod],[PaymentGateway],[Amount]) VALUES ({0},{1},{2},{3},{4},{5},{6},{7},{8})";
			DB.ExecuteSQL(insertStatement.FormatWith(m_OrderNumber, CheckNullAndQuote(transactionType), CheckNullAndQuote(transactionCommand), CheckNullAndQuote(transactionResult), CheckNullAndQuote(pnref), CheckNullAndQuote(code), CheckNullAndQuote(paymentMethod), CheckNullAndQuote(paymentGateway), CheckNullAndQuote(Localization.CurrencyStringForDBWithoutExchangeRate(amount))));
            LoadFromDB(m_OrderNumber);
        }

        private string CheckNullAndQuote(string dbString)
        {
            if (dbString == null)
                return "NULL";

            return DB.SQuote(dbString);
        }
    }

    public class Order
    {
        private int m_CustomerID;
        private int m_OrderNumber;
        private bool m_IsNew;
        private bool m_IsEmpty;
        private Decimal m_OrderWeight;
        private String m_TransactionState;
        private AppLogic.TransactionTypeEnum m_TransactionType; // just int here to avoid circular ref to gateways dll
        private DateTime m_AuthorizedOn;
        private DateTime m_CapturedOn;
        private DateTime m_VoidedOn;
        private DateTime m_RefundedOn;
        private DateTime m_FraudedOn;
        private DateTime m_EditedOn;
        private String m_PaymentGateway;
        private String m_PaymentMethod;
        private String m_OrderNotes;
        private String m_FinalizationData;
        private String m_OrderOptions;
        private String m_LocaleSetting;
        private String m_LastIPAddress;
        private String m_ViewInLocaleSetting;
        private DateTime m_ReceiptEMailSentOn;
        private DateTime m_ShippedOn;
        private DateTime m_DownloadEMailSentOn;
        private DateTime m_DistributorEMailSentOn;
        private String m_CustomerServiceNotes;
        private int m_ParentOrderNumber;
        private int m_RelatedOrderNumber;
        private String m_ChildOrderNumbers;
        private bool m_AlreadyConfirmed;
        private String m_RecurringSubscriptionID;
        private int m_ShippingMethodID;
        private String m_ShippingMethod;
        private CouponObject m_Coupon = new CouponObject();
        private CartItemCollection m_CartItems = new CartItemCollection();
        private AddressInfo m_ShippingAddress;
        private Decimal m_ShippingTotal;
        private AddressInfo m_BillingAddress;
        private DateTime m_OrderDate;
        private Decimal m_Total;
        private Decimal m_SubTotal;
        private Decimal m_TaxTotal;
        private String m_CardNumber;
        private String m_CardName;
        private String m_CardExpirationMonth;
        private String m_CardExpirationYear;
        private String m_Last4;
        private Decimal m_MaxMindFraudScore;
        private String m_MaxMindDetails; // xml fragment
        private int m_AffiliateID;
        private String m_EMail;
        private int m_SkinID;
        private int m_LevelID;
        private String m_LevelName;
        private decimal m_LevelDiscountAmount;
        private Decimal m_LevelDiscountPercent;
        private bool m_LevelHasFreeShipping;
        private bool m_LevelAllowsQuantityDiscounts;
        private bool m_LevelHasNoTax;
        private bool m_LevelAllowsCoupons;
        private bool m_LevelDiscountsApplyToExtendedPrices;
        private string m_CaptureTXResult;
        private string m_CaptureTXCommand;
        private string m_RefundTXResult;
        private string m_RefundTXCommand;
        private string m_AuthorizationPNREF;
        private string m_AuthorizationCode;
        private string m_OrdersCCSaltField;
        private string m_ReceiptHtml = string.Empty;

        public Order(int OrderNumber) : this(OrderNumber, Localization.GetDefaultLocale()) { }

        public Order(int OrderNumber, String ViewInLocaleSetting)
        {
            m_OrderNumber = OrderNumber;
            m_ViewInLocaleSetting = ViewInLocaleSetting;
            LoadFromDB();
        }

        public static string UpdateOrder(int ordernumber, SqlParameter[] spa)
        {
			var err = string.Empty;

			using(var cn = new SqlConnection(DB.GetDBConn()))
			{
				cn.Open();
				using(var cmd = new SqlCommand())
				{
					cmd.Connection = cn;
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.CommandText = "dbo.aspdnsf_updOrders";

					var sqlparam = new SqlParameter("@OrderNUmber", SqlDbType.Int, 4);
					sqlparam.Value = ordernumber;
					cmd.Parameters.Add(sqlparam);
					foreach(var sp in spa)
					{
						cmd.Parameters.Add(sp);
					}
					try
					{
						cmd.ExecuteNonQuery();
					}
					catch(Exception ex)
					{
						err = ex.Message;
					}
				}
			}
            return err;

        }

        public static int GetOrderStoreID(int OrderNumber)
        {
			var StoreId = 0;
            using (var con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (var rs = DB.GetRS("SELECT StoreID FROM Orders with (NOLOCK) WHERE OrderNumber ='" + OrderNumber.ToString() + "';", con))
                {
                    if (rs.Read())
                    {
                        StoreId = DB.RSFieldInt(rs, "StoreId");

                    }
                }
            }
            return StoreId;
        }

        public string UpdateOrder(SqlParameter[] spa)
        {
            return Order.UpdateOrder(m_OrderNumber, spa);
        }

        private void LoadFromDB()
        {
            m_IsEmpty = true;

            m_CartItems = new CartItemCollection();
            m_Coupon = new CouponObject();
			var i = 0;
            using (var con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (var rs = DB.GetRS("dbo.aspdnsf_getOrder " + m_OrderNumber.ToString(), con))
                {
                    while (rs.Read())
                    {
                        m_SkinID = DB.RSFieldInt(rs, "SkinID");
                        if (m_SkinID == 0)
                        {
							m_SkinID = AppLogic.GetStoreSkinID(AppLogic.StoreID());
						}
                        m_IsEmpty = false;

                        if (i == 0)
                        {

                            if (m_ViewInLocaleSetting == null)
                            {
                                m_ViewInLocaleSetting = DB.RSField(rs, "LocaleSetting");
                            }

                            if (m_ViewInLocaleSetting.Length == 0)
                            {
                                m_ViewInLocaleSetting = DB.RSField(rs, "LocaleSetting");
                            }

                            m_MaxMindFraudScore = DB.RSFieldDecimal(rs, "MaxMindFraudScore");
                            m_MaxMindDetails = DB.RSField(rs, "MaxMindDetails");

                            m_Total = DB.RSFieldDecimal(rs, "OrderTotal");
                            m_SubTotal = DB.RSFieldDecimal(rs, "OrderSubtotal");
                            m_TaxTotal = DB.RSFieldDecimal(rs, "OrderTax");
                            m_ShippingTotal = DB.RSFieldDecimal(rs, "OrderShippingCosts");
                            m_OrderDate = DB.RSFieldDateTime(rs, "OrderDate");
                            m_PaymentGateway = DB.RSField(rs, "PaymentGateway");
                            m_CardNumber = DB.RSField(rs, "CardNumber");
                            m_CardName = DB.RSField(rs, "CardName");
                            m_CardExpirationMonth = DB.RSField(rs, "CardExpirationMonth");
                            m_CardExpirationYear = DB.RSField(rs, "CardExpirationYear");
							m_Last4 = DB.RSField(rs, "Last4");
                            m_ParentOrderNumber = DB.RSFieldInt(rs, "ParentOrderNumber");
                            m_RelatedOrderNumber = DB.RSFieldInt(rs, "RelatedOrderNumber");
                            m_AffiliateID = DB.RSFieldInt(rs, "AffiliateID");
                            m_CustomerID = DB.RSFieldInt(rs, "CustomerID");
                            m_EMail = DB.RSField(rs, "EMail");

                            m_OrderWeight = DB.RSFieldDecimal(rs, "OrderWeight");
                            m_TransactionState = DB.RSField(rs, "TransactionState");
                            m_TransactionType = (AppLogic.TransactionTypeEnum)DB.RSFieldInt(rs, "TransactionType");

                            m_AuthorizedOn = DB.RSFieldDateTime(rs, "AuthorizedOn");
                            m_CapturedOn = DB.RSFieldDateTime(rs, "CapturedOn");
                            m_VoidedOn = DB.RSFieldDateTime(rs, "VoidedOn");
                            m_RefundedOn = DB.RSFieldDateTime(rs, "RefundedOn");
                            m_FraudedOn = DB.RSFieldDateTime(rs, "FraudedOn");
                            m_EditedOn = DB.RSFieldDateTime(rs, "EditedOn");

                            m_AlreadyConfirmed = DB.RSFieldBool(rs, "AlreadyConfirmed");

                            m_PaymentMethod = DB.RSField(rs, "PaymentMethod");
                            m_OrderNotes = DB.RSField(rs, "OrderNotes");
                            m_FinalizationData = DB.RSField(rs, "FinalizationData");
                            m_OrderOptions = DB.RSField(rs, "OrderOptions");
                            m_LocaleSetting = Localization.CheckLocaleSettingForProperCase(DB.RSField(rs, "LocaleSetting"));
                            m_LastIPAddress = DB.RSField(rs, "LastIPAddress");
                            m_ReceiptEMailSentOn = DB.RSFieldDateTime(rs, "ReceiptEMailSentOn");
                            m_ShippedOn = DB.RSFieldDateTime(rs, "ShippedOn");
                            m_DownloadEMailSentOn = DB.RSFieldDateTime(rs, "DownloadEMailSentOn");
                            m_DistributorEMailSentOn = DB.RSFieldDateTime(rs, "DistributorEMailSentOn");
                            m_RecurringSubscriptionID = DB.RSField(rs, "RecurringSubscriptionID");
                            m_CustomerServiceNotes = DB.RSField(rs, "CustomerServiceNotes");

							m_Coupon = new CouponObject(
								couponCode: DB.RSField(rs, "CouponCode"),
								description: DB.RSField(rs, "CouponDescription"),
								discountAmount: DB.RSFieldDecimal(rs, "CouponDiscountAmount"),
								discountPercent: DB.RSFieldDecimal(rs, "CouponDiscountPercent"),
								discountIncludesFreeShipping: DB.RSFieldBool(rs, "CouponIncludesFreeShipping"),
								couponType: (CouponTypeEnum)DB.RSFieldInt(rs, "CouponType"));

                            m_ShippingAddress.m_NickName = String.Empty; // TBD DB.RSField(rs,"ShippingNickName");
                            m_ShippingAddress.m_FirstName = DB.RSField(rs, "ShippingFirstName");
                            m_ShippingAddress.m_LastName = DB.RSField(rs, "ShippingLastName");
                            m_ShippingAddress.m_Company = DB.RSField(rs, "ShippingCompany");
                            m_ShippingAddress.m_ResidenceType = (ResidenceTypes)DB.RSFieldInt(rs, "ShippingResidenceType");
                            m_ShippingAddress.m_Address1 = DB.RSField(rs, "ShippingAddress1");
                            m_ShippingAddress.m_Address2 = DB.RSField(rs, "ShippingAddress2");
                            m_ShippingAddress.m_Suite = DB.RSField(rs, "ShippingSuite");
                            m_ShippingAddress.m_City = DB.RSField(rs, "ShippingCity");
                            m_ShippingAddress.m_State = DB.RSField(rs, "ShippingState");
                            m_ShippingAddress.m_Zip = DB.RSField(rs, "ShippingZip");
                            m_ShippingAddress.m_Country = DB.RSField(rs, "ShippingCountry");
                            m_ShippingAddress.m_Phone = DB.RSField(rs, "ShippingPhone");
                            m_ShippingAddress.m_EMail = DB.RSField(rs, "EMail");

                            m_BillingAddress.m_NickName = String.Empty; // TBD DB.RSField(rs,"BillingNickName");
                            m_BillingAddress.m_FirstName = DB.RSField(rs, "BillingFirstName");
                            m_BillingAddress.m_LastName = DB.RSField(rs, "BillingLastName");
                            m_BillingAddress.m_Company = DB.RSField(rs, "BillingCompany");
                            m_BillingAddress.m_ResidenceType = ResidenceTypes.Unknown;
                            m_BillingAddress.m_Address1 = DB.RSField(rs, "BillingAddress1");
                            m_BillingAddress.m_Address2 = DB.RSField(rs, "BillingAddress2");
                            m_BillingAddress.m_Suite = DB.RSField(rs, "BillingSuite");
                            m_BillingAddress.m_City = DB.RSField(rs, "BillingCity");
                            m_BillingAddress.m_State = DB.RSField(rs, "BillingState");
                            m_BillingAddress.m_Zip = DB.RSField(rs, "BillingZip");
                            m_BillingAddress.m_Country = DB.RSField(rs, "BillingCountry");
                            m_BillingAddress.m_Phone = DB.RSField(rs, "BillingPhone");
                            m_BillingAddress.m_EMail = DB.RSField(rs, "EMail");

                            m_ShippingMethodID = DB.RSFieldInt(rs, "ShippingMethodID");
                            m_ShippingMethod = DB.RSFieldByLocale(rs, "ShippingMethod", ViewInLocaleSetting);
                            if (m_ShippingMethod.Length == 0)
                            {
                                m_ShippingMethod = Shipping.GetShippingMethodDisplayName(m_ShippingMethodID, ViewInLocaleSetting); // for old order compatibility
                            }
                            m_LevelID = DB.RSFieldInt(rs, "LevelID");
                            m_LevelName = DB.RSField(rs, "LevelName");
                            m_LevelDiscountAmount = DB.RSFieldDecimal(rs, "LevelDiscountAmount");
                            m_LevelDiscountPercent = DB.RSFieldDecimal(rs, "LevelDiscountPercent");
                            m_LevelHasFreeShipping = DB.RSFieldBool(rs, "LevelHasFreeShipping");
                            m_LevelAllowsQuantityDiscounts = DB.RSFieldBool(rs, "LevelAllowsQuantityDiscounts");
                            m_LevelHasNoTax = DB.RSFieldBool(rs, "LevelHasNoTax");
                            m_LevelAllowsCoupons = DB.RSFieldBool(rs, "LevelAllowsCoupons");
                            m_LevelDiscountsApplyToExtendedPrices = DB.RSFieldBool(rs, "LevelDiscountsApplyToExtendedPrices");


                            m_OrdersCCSaltField = rs[AppLogic.AppConfig("OrdersCCSaltField")].ToString();
                            m_CaptureTXResult = DB.RSField(rs, "CaptureTXResult");
                            m_CaptureTXCommand = DB.RSField(rs, "CaptureTXCommand");
                            m_RefundTXResult = DB.RSField(rs, "RefundTXResult");
                            m_RefundTXCommand = DB.RSField(rs, "RefundTXCommand");
                            m_AuthorizationPNREF = DB.RSField(rs, "AuthorizationPNREF");
                            m_AuthorizationCode = DB.RSField(rs, "AuthorizationCode");
                            m_ReceiptHtml = DB.RSField(rs, "ReceiptHtml");
                        }

                        m_IsNew = DB.RSFieldBool(rs, "IsNew");
						var newItem = new CartItem();
                        newItem.ShoppingCartRecordID = DB.RSFieldInt(rs, "ShoppingCartRecID");
                        newItem.ProductID = DB.RSFieldInt(rs, "ProductID");
                        newItem.VariantID = DB.RSFieldInt(rs, "VariantID");
                        newItem.ProductName = DB.RSFieldByLocale(rs, "OrderedProductName", ViewInLocaleSetting);
                        newItem.VariantName = DB.RSFieldByLocale(rs, "OrderedProductVariantName", ViewInLocaleSetting);
                        newItem.SKU = DB.RSField(rs, "OrderedProductSKU");
                        newItem.Quantity = DB.RSFieldInt(rs, "Quantity");

                        newItem.ShippingMethodID = DB.RSFieldInt(rs, "ShippingMethodID");
                        newItem.ShippingMethod = DB.RSFieldByLocale(rs, "ShippingMethod", ViewInLocaleSetting);
                        if (newItem.ShippingMethod.Length == 0)
                        {
							newItem.ShippingMethod = Shipping.GetShippingMethodDisplayName(newItem.ShippingMethodID, ViewInLocaleSetting); // for old order compatibility
                        }

                        newItem.ChosenColor = DB.RSFieldByLocale(rs, "ChosenColor", ViewInLocaleSetting);
                        newItem.ChosenColorSKUModifier = DB.RSField(rs, "ChosenColorSKUModifier");
                        newItem.ChosenSize = DB.RSFieldByLocale(rs, "ChosenSize", ViewInLocaleSetting);
                        newItem.ChosenSizeSKUModifier = DB.RSField(rs, "ChosenSizeSKUModifier");
                        newItem.TextOption = DB.RSField(rs, "TextOption");
                        newItem.SizeOptionPrompt = DB.RSFieldByLocale(rs, "SizeOptionPrompt", ViewInLocaleSetting);
                        newItem.ColorOptionPrompt = DB.RSFieldByLocale(rs, "ColorOptionPrompt", ViewInLocaleSetting);
                        newItem.TextOptionPrompt = DB.RSFieldByLocale(rs, "TextOptionPrompt", ViewInLocaleSetting);
                        newItem.CustomerEntersPricePrompt = DB.RSFieldByLocale(rs, "CustomerEntersPricePrompt", ViewInLocaleSetting);
                        if (newItem.SizeOptionPrompt.Length == 0)
                        {
                            newItem.SizeOptionPrompt = "Size";
                        }
                        if (newItem.ColorOptionPrompt.Length == 0)
                        {
                            newItem.ColorOptionPrompt = "Color";
                        }
                        if (newItem.TextOptionPrompt.Length == 0)
                        {
                            newItem.TextOptionPrompt = "Customization Text:";
                        }
                        if (newItem.CustomerEntersPricePrompt.Length == 0)
                        {
                            newItem.CustomerEntersPricePrompt = "Enter Donation Amount:";
                        }
                        newItem.ManufacturerPartNumber = DB.RSField(rs, "OrderedProductManufacturerPartNumber");
                        newItem.Weight = DB.RSFieldDecimal(rs, "OrderedProductWeight");
                        newItem.Price = DB.RSFieldDecimal(rs, "OrderedProductPrice");
                        newItem.CustomerEntersPrice = DB.RSFieldBool(rs, "CustomerEntersPrice");
                        newItem.QuantityDiscountID = DB.RSFieldInt(rs, "OrderedProductQuantityDiscountID");
                        newItem.QuantityDiscountName = DB.RSFieldByLocale(rs, "OrderedProductQuantityDiscountName", ViewInLocaleSetting);
                        newItem.QuantityDiscountPercent = DB.RSFieldDecimal(rs, "OrderedProductQuantityDiscountPercent");
                        newItem.IsTaxable = DB.RSFieldBool(rs, "IsTaxable");
                        newItem.TaxClassID = DB.RSFieldInt(rs, "TaxClassID");
                        newItem.TaxRate = DB.RSFieldDecimal(rs, "TaxRate");
                        newItem.IsShipSeparately = DB.RSFieldBool(rs, "IsShipSeparately");
                        newItem.IsDownload = DB.RSFieldBool(rs, "IsDownload");
                        newItem.DownloadLocation = DB.RSField(rs, "DownloadLocation");

                        newItem.FreeShipping = DB.RSFieldTinyInt(rs, "FreeShipping") == 1;
                        newItem.Shippable = DB.RSFieldTinyInt(rs, "FreeShipping") != 2;

                        newItem.DistributorID = DB.RSFieldInt(rs, "DistributorID");
                        newItem.Notes = DB.RSField(rs, "Notes");
                        newItem.OrderShippingDetail = DB.RSField(rs, "ShippingDetail");
                        newItem.IsAKit = DB.RSFieldBool(rs, "IsAKit");
                        newItem.ShippingMethodID = DB.RSFieldInt(rs, "CartItemShippingMethodID");
                        newItem.ShippingMethod = DB.RSField(rs, "CartItemShippingMethod");

						var detailXml = DB.RSField(rs, "ShippingDetail");
                        newItem.ShippingAddressID = DB.RSFieldInt(rs, "ShippingAddressID");
                        if (detailXml.Length != 0)
                        {
                            newItem.ShippingDetail = new Address();
                            newItem.ShippingDetail.AsXml = detailXml;
                        }
                        m_CartItems.Add(newItem);

                        i = i + 1;
                    }
                  
                    if (m_ViewInLocaleSetting == null || m_ViewInLocaleSetting.Length == 0)
                    {
                        m_ViewInLocaleSetting = m_LocaleSetting;
                    }
                }
            }

            // get child order #'s, if any:
            m_ChildOrderNumbers = string.Empty;

            using (var con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (var rs = DB.GetRS(string.Format("select OrderNumber from orders with (NOLOCK) where ParentOrderNumber={0}", OrderNumber.ToString()), con))
                {
                    while (rs.Read())
                    {
                        if (m_ChildOrderNumbers.Length != 0)
                        {
                            m_ChildOrderNumbers += ",";
                        }
                        m_ChildOrderNumbers += DB.RSFieldInt(rs, "OrderNumber").ToString();
                    }
                }
            }
        }

        public CouponObject GetCoupon()
        {
            return m_Coupon;
        }

        public Decimal SubTotal()
        {
            return m_SubTotal;
        }

        public Decimal TaxTotal()
        {
            return m_TaxTotal;
        }

        public Decimal ShippingTotal()
        {
            return m_ShippingTotal;
        }

        public Decimal Total()
        {
            return m_Total;
        }

        public string GetDistributorNotificationPackageToUse(int ForDistributorID)
        {
			var PackageName = string.Empty;

            using (var con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (var rs = DB.GetRS("select NotificationXmlPackage from Distributor  with (NOLOCK)  where DistributorID=" + ForDistributorID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        PackageName = DB.RSField(rs, "NotificationXmlPackage");
                    }
                }
            }

            if (PackageName.Length == 0)
            {
                PackageName = AppLogic.AppConfig("XmlPackage.DefaultDistributorNotification");
            }
            if (PackageName.Length == 0)
            {
                PackageName = "notification.distributor.xml.config";
            }
            return PackageName;
        }

        public String DistributorNotification(int ForDistributorID)
        {            
            String PackageName = GetDistributorNotificationPackageToUse(ForDistributorID);                
            return AppLogic.RunXmlPackage(PackageName, null, null, AppLogic.GetStoreSkinID(GetOrderStoreID(OrderNumber)), string.Empty, "ordernumber=" + OrderNumber.ToString() + "&distributorid=" + ForDistributorID.ToString(), false, false);
        }

        public String ShippedNotification()
        {
            String PackageName = AppLogic.AppConfig("XmlPackage.OrderShipped");
            if (PackageName.Length != 0)
            {
                return AppLogic.RunXmlPackage(PackageName, null, null, AppLogic.GetStoreSkinID(GetOrderStoreID(OrderNumber)), string.Empty, "ordernumber=" + OrderNumber.ToString(), false, false);
            }
            return string.Empty;
        }

        public String AdminNotification()
        {
            String PackageName = AppLogic.AppConfig("XmlPackage.NewOrderAdminNotification");
            if (PackageName.Length != 0)
            {
                return AppLogic.RunXmlPackage(PackageName, null, null, AppLogic.GetStoreSkinID(GetOrderStoreID(OrderNumber)), string.Empty, "ordernumber=" + OrderNumber.ToString(), false, false);
            }
            return string.Empty;
        }

        // returns true if this order has any items which are download items:
        public bool HasDownloadComponents(bool RequireDownloadLocationAlso)
        {
            if (AppLogic.OrderHasDownloadComponents(m_OrderNumber, RequireDownloadLocationAlso))
            {
                return true;
            }
            foreach (CartItem c in m_CartItems)
            {
                if (c.IsDownload)
                {
                    if (RequireDownloadLocationAlso)
                    {
                        if (c.DownloadLocation.Length != 0)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
                // this item is not a download item, or is download but doesn't have a downloadlocation, so move to next item
            }
            return false;
        }

        // returns true if this order has any items which are download items:
        public bool HasRecurringComponents()
        {
            return m_CartItems.ContainsRecurring;
        }

		public bool HasKitItems()
		{
			return m_CartItems
				.Where(c => AppLogic.IsAKit(c.ProductID))
				.Any();
		}

		// returns true if this order has any items which are drop ship items:
		public bool HasDistributorComponents()
        {
            foreach (CartItem c in m_CartItems)
            {
                if (c.DistributorID != 0)
                {
                    return true;
                }
            }
            return false;
        }

        // returns true if this order has ONLY download items:
        public bool IsAllDownloadComponents()
        {
            foreach (CartItem c in m_CartItems)
            {
                if (!c.IsDownload)
                {
                    return false;
                }
            }
            return true;
        }

        public bool isAllDistributorComponents()
        {
            foreach (CartItem c in m_CartItems)
            {
                if (c.DistributorID == 0)
                {
                    return false;
                }
            }
            return true;
        }

        public bool HasMultipleShippingAddresses()
        {
            return m_CartItems.HasMultipleShippingAddresses;
        }
		
        private void SaveReceipt(String receiptText)
        {
            DB.ExecuteSQL("update Orders set ReceiptHtml=" + DB.SQuote(receiptText) + " where OrderNumber=" + m_OrderNumber.ToString());
        }

        public String Receipt(Customer ViewingCustomer, bool ShowOnlineLink)
        {
            return Receipt(ViewingCustomer, ShowOnlineLink, false);
        }

        /// <summary>
        /// Generate order receipt report.
        /// </summary>
        /// <param name="ViewingCustomer">Current customer info who is login.</param>
        /// <param name="ShowOnlineLink"> If receipt is sent through email value should be 'true' to show online link.</param>
        /// <param name="disallowCacheing">Forces the receipt to be regenerated rather than pulling from the database.</param>
        /// <returns>Generated receipt report.</returns>
        public String Receipt(Customer ViewingCustomer, bool ShowOnlineLink, bool disallowCacheing)
        {
            if (!CustomerCanViewRecipt(ViewingCustomer))
                return string.Empty;

            if (disallowCacheing || ReceiptEMailSentOn == DateTime.MinValue || String.IsNullOrEmpty(m_ReceiptHtml))
            {
                String PackageName = AppLogic.AppConfig("XmlPackage.OrderReceipt");
                string runtimeParams = string.Format("ordernumber={0}&ShowOnlineLink={1}", OrderNumber.ToString(), ShowOnlineLink);
                String result = AppLogic.RunXmlPackage(PackageName, null, ViewingCustomer, AppLogic.GetStoreSkinID(GetOrderStoreID(OrderNumber)), String.Empty, runtimeParams, false, true);
                if (!disallowCacheing && !result.Contains(new Topic("InvalidRequest", ViewingCustomer.LocaleSetting, 1).Contents))
                    SaveReceipt(result);
                return result;
            }
            return m_ReceiptHtml;
        }

        private Boolean CustomerCanViewRecipt(Customer ViewingCustomer)
        {
            return ViewingCustomer.CustomerID == this.CustomerID || ViewingCustomer.IsAdminUser;
        }
		
        public int CustomerID
        {
            get
            {
                return m_CustomerID;
            }
        }

        public int ShippingMethodID
        {
            get
            {
                return m_ShippingMethodID;
            }
        }

        public Decimal OrderWeight
        {
            get
            {
                return m_OrderWeight;
            }
        }

        public String LocaleSetting
        {
            get
            {
                return m_LocaleSetting;
            }
        }

        public String LastIPAddress
        {
            get
            {
                return m_LastIPAddress;
            }
        }

        public String ViewInLocaleSetting
        {
            get
            {
                if (m_ViewInLocaleSetting == null || m_ViewInLocaleSetting.Length == 0)
                {
                    m_ViewInLocaleSetting = m_LocaleSetting;
                }
                return m_ViewInLocaleSetting;
            }
        }

        public int OrderNumber
        {
            get
            {
                return m_OrderNumber;
            }
        }

        public int ParentOrderNumber
        {
            get
            {
                return m_ParentOrderNumber;
            }
        }

        public int RelatedOrderNumber
        {
            get
            {
                return m_RelatedOrderNumber;
            }
            set
            {
                m_RelatedOrderNumber = value;
                DB.ExecuteSQL("update Orders set RelatedOrderNumber=" + m_RelatedOrderNumber.ToString() + " where OrderNumber=" + m_OrderNumber.ToString());
            }
        }

        public int SkinID
        {
            get
            {
                return m_SkinID;
            }
        }

        public String PaymentMethod
        {
            get
            {
                return m_PaymentMethod;
            }
        }

        public String EMail
        {
            get
            {
                return m_EMail;
            }
        }

        public String PaymentGateway
        {
            get
            {
                return m_PaymentGateway;
            }
        }

        public Decimal MaxMindFraudScore
        {
            get
            {
                return m_MaxMindFraudScore;
            }
        }

        public CartItemCollection CartItems
        {
            get
            {
                return m_CartItems;
            }
            set
            {
                m_CartItems = value;
            }
        }

        public String OrderOptions
        {
            get
            {
                return m_OrderOptions;
            }
        }

        public DateTime ReceiptEMailSentOn
        {
            get
            {
                return m_ReceiptEMailSentOn;
            }
        }

        public DateTime ShippedOn
        {
            get
            {
                return m_ShippedOn;
            }
        }
        public DateTime DistributorEMailSentOn
        {
            get
            {
                return m_DistributorEMailSentOn;
            }
        }

        public DateTime AuthorizedOn
        {
            get
            {
                return m_AuthorizedOn;
            }
        }

        public DateTime CapturedOn
        {
            get
            {
                return m_CapturedOn;
            }
        }

        public DateTime VoidedOn
        {
            get
            {
                return m_VoidedOn;
            }
        }

        public DateTime RefundedOn
        {
            get
            {
                return m_RefundedOn;
            }
        }

        public DateTime FraudedOn
        {
            get
            {
                return m_FraudedOn;
            }
        }

        public DateTime EditedOn
        {
            get
            {
                return m_EditedOn;
            }
            set
            {
                m_EditedOn = value;
                DB.ExecuteSQL("update Orders set EditedOn=" + DB.SQuote(Localization.DateStringForDB(m_EditedOn)) + " where OrderNumber=" + m_OrderNumber.ToString());
            }
        }

        public AddressInfo ShippingAddress
        {
            get
            {
                return m_ShippingAddress;
            }
        }

        public string CustomerServiceNotes
        {
            get { return m_CustomerServiceNotes; }
        }

        public string FinalizationData
        {
            get { return m_FinalizationData; }
        }

        public string OrderNotes
        {
            get { return m_OrderNotes; }
        }

        public AddressInfo BillingAddress
        {
            get
            {
                return m_BillingAddress;
            }
        }

        public String ShippingMethod
        {
            get
            {
                return m_ShippingMethod;
            }
        }

        public String ChildOrderNumbers
        {
            get
            {
                return m_ChildOrderNumbers;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return m_IsEmpty;
            }
        }

        public DateTime OrderDate
        {
            get
            {
                return m_OrderDate;
            }
        }
		
        static public void MarkOrderAsShipped(int orderNumber, string shippedVia, string shippingTrackingNumber, DateTime shippedOn, bool isRecurring, bool disableEmail)
        {
            DB.ExecuteSQL("UPDATE Orders SET IsNew = 0, ReadyToShip = 1, ShippedVIA = @shippedVia, ShippingTrackingNumber = @trackingNumber, ShippedOn = @shippedOn WHERE OrderNumber = @orderNumber",
				new SqlParameter("@shippedVia", shippedVia),
				new SqlParameter("@trackingNumber", shippingTrackingNumber),
				new SqlParameter("@shippedOn", shippedOn),
				new SqlParameter("@orderNumber", orderNumber));

            if (!disableEmail)
            {
                var okToSend = false;
                var orderStoreId = GetOrderStoreID(orderNumber);

                var mailserver = AppLogic.AppConfig("MailMe_Server", orderStoreId, true);
                if (isRecurring)
                {
                    if (AppLogic.AppConfigBool("Recurring.SendShippedEMailToCustomer", orderStoreId, true) && mailserver.Length != 0 && mailserver != AppLogic.ro_TBD)
                        okToSend = true;
                }
                else
                {
                    if (AppLogic.AppConfigBool("SendShippedEMailToCustomer", orderStoreId, true) && mailserver.Length != 0 && mailserver != AppLogic.ro_TBD)
                        okToSend = true;
                }
                if (okToSend)
                {
                    try
					{
						// try to send "shipped on" EMail
						var order = new Order(orderNumber, null);
                        var subjectLine = string.Format("{0} Order Shipped", AppLogic.AppConfig("StoreName", orderStoreId, true));

                        if (isRecurring)
                            subjectLine += "(RECURRING AUTO-SHIP)";

                        var body = order.ShippedNotification();
                        if (mailserver.Length != 0 && 
                            mailserver.Equals(AppLogic.ro_TBD, StringComparison.InvariantCultureIgnoreCase) == false)
                        {
                            AppLogic.SendMail(subject: subjectLine, 
								body: body + AppLogic.AppConfig("MailFooter", orderStoreId, true), 
								useHtml: true, 
								fromAddress: AppLogic.AppConfig("ReceiptEMailFrom", orderStoreId, true), 
								fromName: AppLogic.AppConfig("ReceiptEMailFromName", orderStoreId, true), 
								toAddress: order.EMail, 
								toName: order.EMail, 
								bccAddresses: string.Empty, 
								replyToAddress: AppLogic.AppConfig("ReceiptEMailFrom", orderStoreId, true));
                        }
                    }
                    catch (Exception exception)
					{
						SysLog.LogException(exception, MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
					}
                }
            }
        }

        public bool TransactionIsCaptured()
        {
            return m_TransactionState == AppLogic.ro_TXStateCaptured;
        }

        public string CaptureTXResult
        {
            get { return m_CaptureTXResult; }
            set
            {
                SqlParameter sp1 = new SqlParameter("@CaptureTXResult", SqlDbType.NText, 1000000000);
                sp1.Value = value;
                SqlParameter[] spa = { sp1 };
                string retval = this.UpdateOrder(spa);
                if (retval == string.Empty)
                {
                    m_CaptureTXResult = value;
                }

            }
        }

        public string CaptureTXCommand
        {
            get { return m_CaptureTXCommand; }
            set
            {
                SqlParameter sp1 = new SqlParameter("@CaptureTXCommand", SqlDbType.NText, 1000000000);
                sp1.Value = value;
                SqlParameter[] spa = { sp1 };
                string retval = this.UpdateOrder(spa);
                if (retval == string.Empty)
                {
                    m_CaptureTXCommand = value;
                }

            }
        }

        public string RefundTXResult
        {
            get { return m_RefundTXResult; }
            set
            {
                SqlParameter sp1 = new SqlParameter("@RefundTXResult", SqlDbType.NText, 1000000000);
                sp1.Value = value;
                SqlParameter[] spa = { sp1 };
                string retval = this.UpdateOrder(spa);
                if (retval == string.Empty)
                {
                    m_RefundTXResult = value;
                }

            }
        }

        public string RefundTXCommand
        {
            get { return m_RefundTXCommand; }
            set
            {
                SqlParameter sp1 = new SqlParameter("@RefundTXCommand", SqlDbType.NText, 1000000000);
                sp1.Value = value;
                SqlParameter[] spa = { sp1 };
                string retval = this.UpdateOrder(spa);
                if (retval == string.Empty)
                {
                    m_RefundTXCommand = value;
                }

            }
        }
        
        public string AuthorizationPNREF
        {
            get { return m_AuthorizationPNREF; }
            set
            {
                SqlParameter sp1 = new SqlParameter("@AuthorizationPNREF", SqlDbType.NVarChar, 100);
                sp1.Value = value;
                SqlParameter[] spa = { sp1 };
                string retval = this.UpdateOrder(spa);
                if (retval == string.Empty)
                {
                    m_AuthorizationPNREF = value;
                }

            }
        }

        public string CardExpirationMonth
        {
            get { return m_CardExpirationMonth; }
            set
            {
                SqlParameter sp1 = new SqlParameter("@CardExpirationMonth", SqlDbType.NVarChar, 10);
                sp1.Value = value;
                SqlParameter[] spa = { sp1 };
                string retval = this.UpdateOrder(spa);
                if (retval == string.Empty)
                {
                    m_CardExpirationMonth = value;
                }

            }
        }

        public string CardExpirationYear
        {
            get { return m_CardExpirationYear; }
            set
            {
                SqlParameter sp1 = new SqlParameter("@CardExpirationYear", SqlDbType.NVarChar, 10);
                sp1.Value = value;
                SqlParameter[] spa = { sp1 };
                string retval = this.UpdateOrder(spa);
                if (retval == string.Empty)
                {
                    m_CardExpirationYear = value;
                }

            }
        }

        public string AuthorizationCode
        {
            get { return m_AuthorizationCode; }
            set
            {
                SqlParameter sp1 = new SqlParameter("@AuthorizationCode", SqlDbType.NVarChar, 100);
                sp1.Value = value;
                SqlParameter[] spa = { sp1 };
                string retval = this.UpdateOrder(spa);
                if (retval == string.Empty)
                {
                    m_AuthorizationCode = value;
                }

            }
        }

        public string CardNumber
        {
            get { return m_CardNumber; }
            set
            {
                SqlParameter sp1 = new SqlParameter("@CardNumber", SqlDbType.NText, 1000000000);
                sp1.Value = value;
                SqlParameter[] spa = { sp1 };
                string retval = this.UpdateOrder(spa);
                if (retval == string.Empty)
                {
                    m_CardNumber = value;
                }

            }
        }

        public string TransactionState
        {
            get { return m_TransactionState; }
            set
            {
                SqlParameter sp1 = new SqlParameter("@TransactionState", SqlDbType.NVarChar, 20);
                sp1.Value = value;
                SqlParameter[] spa = { sp1 };
                string retval = this.UpdateOrder(spa);
                if (retval == string.Empty)
                {
                    m_TransactionState = value;
                }

            }
        }

        public AppLogic.TransactionTypeEnum TransactionType
        {
            get { return m_TransactionType; }
        }
        
        public string Last4
        {
            get { return m_Last4; }
            set
            {
                SqlParameter sp1 = new SqlParameter("@Last4", SqlDbType.NVarChar, 4);
                sp1.Value = value;
                SqlParameter[] spa = { sp1 };
                string retval = this.UpdateOrder(spa);
                if (retval == string.Empty)
                {
                    m_Last4 = value;
                }

            }
        }
		
        public String RecurringSubscriptionID
        {
            get
            {
                return m_RecurringSubscriptionID;
            }
        }

        public decimal OrderBalance
        {
            get
            {
                decimal t = Total();
                return t - CommonLogic.IIF(this.GetCoupon().CouponType == CouponTypeEnum.GiftCard, CommonLogic.IIF(t < this.GetCoupon().DiscountAmount, t, this.GetCoupon().DiscountAmount), 0);
            }
        }

        public bool AlreadyConfirmed
        {
            get
            {
                return m_AlreadyConfirmed;
            }
        }
        
        public int AffiliateID
        {
            get
            {
                return m_AffiliateID;
            }
        }

        public static void DeleteAnyOrderDownloadFiles(int OrderNumber)
        {
            using (var con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (var rs = DB.GetRS("Select CustomerID from Orders  with (NOLOCK)  where OrderNumber=" + OrderNumber.ToString(), con))
                {
                    if (rs.Read())
                    {
						var CustomerID = DB.RSFieldInt(rs, "CustomerID");
						var ThisOrderDownloadDir = CommonLogic.SafeMapPath(CommonLogic.IIF(AppLogic.IsAdminSite, "~/", "") + "orderdownloads/" + OrderNumber.ToString() + "_" + CustomerID.ToString());
                        if (Directory.Exists(ThisOrderDownloadDir))
                        {
                            Directory.Delete(ThisOrderDownloadDir, true);
                        }
                    }
                }
            }
        }

        public static int GetOrderCustomerID(int OrderNumber)
        {
			var tmp = 0;

            using (var con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (var rs = DB.GetRS("Select CustomerID from Orders  with (NOLOCK)  where OrderNumber=" + OrderNumber.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldInt(rs, "CustomerID");
                    }
                }
            }

            return tmp;
        }

        public bool ContainsGiftCard()
        {
            return m_CartItems.ContainsGiftCard;
        }

        public static void MarkOrderAsFraud(int OrderNumber, bool SetFraudStateOn)
        {
			var CouponType = 0;
			var CouponCode = string.Empty;
			var CustomerID = 0;

            using (var con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (var rs = DB.GetRS("Select CustomerID, CouponType, CouponCode from Orders  with (NOLOCK)  where OrderNumber=" + OrderNumber.ToString(), con))
                {
                    if (rs.Read())
                    {
                        CustomerID = DB.RSFieldInt(rs, "CustomerID");
                        CouponType = DB.RSFieldInt(rs, "CouponType");
                        CouponCode = DB.RSField(rs, "CouponCode");
                    }
                }
            }

            // force this customer to be logged out:
            DB.ExecuteSQL("update Customer set CustomerGUID=newid() where CustomerID=" + CustomerID.ToString());
            
            // make sure inventory was restored. safe to call repeatedly. proc protects against deducting twice
            DB.ExecuteSQL("aspdnsf_AdjustInventory " + OrderNumber.ToString() + "," + CommonLogic.IIF(SetFraudStateOn,"1","-1"));
            DB.ExecuteSQL("aspdnsf_MarkOrderAsFraud " + OrderNumber.ToString() + "," + CommonLogic.IIF(SetFraudStateOn, "1", "0"));
            if (SetFraudStateOn)
            {
                Order.DeleteAnyOrderDownloadFiles(OrderNumber);

                //Invalidate GiftCards ordered on this order
                GiftCards GCs = new GiftCards(OrderNumber, GiftCardCollectionFilterType.OrderNumber);
                foreach (GiftCard gc in GCs)
                {
                    gc.GiftCardTransactions.Add(GiftCardUsageTransaction.CreateTransaction(gc.GiftCardID, GiftCardUsageReasons.FundsRemovedByAdmin, 0, 0, gc.Balance, ""));
                    gc.UpdateCard(null, null, null, null, 1, null, null, null, null, null, null, null, null, null, null);
                }

                //remove any balance remianing on the coupon used in paying for the order
                if ((CouponTypeEnum)CouponType == CouponTypeEnum.GiftCard)
                {
                    GiftCard gc = new GiftCard(CouponCode);
                    if (gc.GiftCardID != 0)
                    {
                        gc.UpdateCard(null, null, null, null, 1, null, null, null, null, null, null, null, null, null, null);
                    }
                }

                // lock this customer's account:
                Customer.LockAccount(CustomerID,true);
            }
            else
            {
                //Restore GiftCard 
                GiftCards GCs = new GiftCards(OrderNumber, GiftCardCollectionFilterType.OrderNumber);
                foreach (GiftCard gc in GCs)
                {
                    gc.GiftCardTransactions.Add(GiftCardUsageTransaction.CreateTransaction(gc.GiftCardID, GiftCardUsageReasons.FundsAddedByAdmin, 0, 0, gc.InitialAmount, ""));
                    gc.UpdateCard(null, null, null, null, 0, null, null, null, null, null, null, null, null, null, null);
                }

                //remove any balance remianing on the coupon used in paying for the order
                if ((CouponTypeEnum)CouponType == CouponTypeEnum.GiftCard)
                {
                    GiftCard gc = new GiftCard(CouponCode);
                    if (gc.GiftCardID != 0)
                    {
                        gc.UpdateCard(null, null, null, null, 0, null, null, null, null, null, null, null, null, null, null);
                    }
                }

                // unlock this customer's account:
                Customer.LockAccount(CustomerID, false);

            }

        }

        public static string StaticGetSaltKey(int OrderNumber)
        {
			var tmp = string.Empty;

            using (var con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (var rs = DB.GetRS("select " + AppLogic.AppConfig("OrdersCCSaltField") + " from Orders  with (NOLOCK)  where OrderNumber=" + OrderNumber.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmp = rs[AppLogic.AppConfig("OrdersCCSaltField")].ToString();
                    }
                }
            }

            return tmp;
        }

		static public bool BuildReOrder(Dictionary<string, EntityHelper> entityHelpers, Customer customer, int orderNumber, out string status)
		{
			status = AppLogic.ro_OK;

			var order = new Order(orderNumber, customer.LocaleSetting);
			if(order.IsEmpty)
			{
				status = "Order Not Found";
				return false;
			}

			if(order.HasRecurringComponents())
			{
				status = "Orders that contain recurring auto-ship items can not be re-ordered. The next recurring shipment will be processed by the store administrator when the next recurring order date arrives.";
				return false;
			}

			if(order.HasKitItems())
			{
				status = "Order contains kits";
				return false;
			}
			
			var reorderCart = new ShoppingCart(order.SkinID, customer, CartTypeEnum.ShoppingCart, 0, false);
			if(AppLogic.AppConfigBool("Reorder.ClearCartBeforeAddingReorderItems"))
				reorderCart.ClearContents();

			try
			{
				var cartActionProvider = DependencyResolver.Current.GetService<CartActionProvider>();
				foreach(var cartItem in order.CartItems)
				{
					var product = new Product(cartItem.ProductID);
					var variant = new ProductVariant(cartItem.VariantID);

					// Check for cart items that cannot be reordered.
					if(product.Deleted
						|| !product.Published
						|| variant.Deleted
						|| !variant.Published
						|| cartItem.IsRecurring)
						continue;

					cartActionProvider.AddItemToCart(
						new AddToCartContext()
						{
							Customer = customer,
							CartType = CartTypeEnum.ShoppingCart,
							Quantity = cartItem.Quantity,
							ProductId = cartItem.ProductID,
							VariantId = cartItem.VariantID,
							IsWishlist = false,
							Color = cartItem.ChosenColor,
							Size = cartItem.ChosenSize,
							TextOption = cartItem.TextOption
						});
				}
			}
			catch(Exception ex)
			{
				status = CommonLogic.GetExceptionDetail(ex, string.Empty);
				return false;
			}

			return true;
		}

		public static bool OrderExists(int ordernumber)
		{
			return DB.GetSqlN("select OrderNumber N from dbo.orders where OrderNumber = " + ordernumber.ToString()) > 0;
		}

        public bool HasBeenEdited
        {
            get { return !EditedOn.Equals(System.DateTime.MinValue); }
        }

        public string RegenerateReceipt(Customer ViewingCustomer)
        {
            String Status = AppLogic.ro_OK;
            String PackageName = AppLogic.AppConfig("XmlPackage.OrderReceipt");
                string runtimeParams = string.Format("ordernumber={0}&ShowOnlineLink={1}", OrderNumber.ToString(), false);
                String result = AppLogic.RunXmlPackage(PackageName, null, ViewingCustomer, AppLogic.GetStoreSkinID(GetOrderStoreID(OrderNumber)), string.Empty, runtimeParams, false, true);
                if (!result.Contains(new Topic("InvalidRequest", ViewingCustomer.LocaleSetting, 1).Contents))
                    SaveReceipt(result);
            return Status;
        }
    }
}
