// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI;
using System.Xml;
using AspDotNetStorefront.Routing;

namespace AspDotNetStorefrontCore
{
	public delegate void ApplicationStartRoutine();

	public partial class AppLogic
	{
		public const string IsAdminSiteKey = "IsAdminSite";

		public static int NumProductsInDB = 0; // set to # of products in the db on Application_Start. Not updated thereafter
		public static bool CachingOn = false;  // set to true in Application_Start if AppConfig:CacheMenus=true
		public static IReadOnlyDictionary<string, GlobalConfig> GlobalConfigTable;
		public static AspdnsfEventHandlers EventHandlerTable;//LOADED in application_start of the respective web project
		public static BadWords BadWordTable;
		public static CountryTaxRates CountryTaxRatesTable; // LOADED in application_start of the respective web project
		public static StateTaxRates StateTaxRatesTable; // LOADED in application_start of the respective web project
		public static ZipTaxRates ZipTaxRatesTable; // LOADED in application_start of the respective web project
		public static Hashtable ImageFilenameCache = new Hashtable(); // Caching is ALWAYS on for images, cache of category/section/product/etc image filenames from LookupImage. Added on first need
		public static Dictionary<int, EntityHelper> CategoryStoreEntityHelper;
		public static Dictionary<int, EntityHelper> SectionStoreEntityHelper;
		public static Dictionary<int, EntityHelper> ManufacturerStoreEntityHelper;
		public static Dictionary<int, EntityHelper> DistributorStoreEntityHelper;
		public static Dictionary<int, EntityHelper> GenreStoreEntityHelper;
		public static Dictionary<int, EntityHelper> VectorStoreEntityHelper;
		public static Dictionary<int, EntityHelper> LibraryStoreEntityHelper;

		public static ApplicationStartRoutine m_RestartApp;

		public static readonly String[] ro_SupportedEntities =
		{
			EntityTypes.Category,
			EntityTypes.Section,
			EntityTypes.Manufacturer,
			EntityTypes.Distributor,
			EntityTypes.Genre,
			EntityTypes.Vector,
		};

		// if you want ../images to be replaced with images so images resolve on the store side, applies to any HTML field in the db.
		public static bool ReplaceImageURLFromAssetMgr = false;
		public static bool AppIsStarted = false;
		public static AspNetHostingPermissionLevel TrustLevel;
		public static XmlDocument Customerlevels;
		public static int MicropayProductID = 0;
		public static int AdHocProductID = 0;
		public static int AdHocVariantID = 0;

		public const String ro_DefaultProductXmlPackage = "product.variantsinrightbar.xml.config";
		public const String ro_DefaultProductKitXmlPackage = "product.kitproduct.xml.config";
		public const String ro_DefaultEntityXmlPackage = "entity.grid.xml.config";

		public const String ro_NotApplicable = "N/A";

		const string CardExtraCodeSessionKey = "CardExtraCode";

		public enum TransactionTypeEnum
		{
			CHARGE = 1,
			CREDIT = 2,
			RECURRING_AUTO = 3
		}

		public enum EntityType
		{
			Unknown = 0,
			Category = 1,
			Section = 2,
			Manufacturer = 3,
			Distributor = 4,
			Library = 5,
			Genre = 6,
			Vector = 7,
			Affiliate = 8,
			CustomerLevel = 9,
			Product = 10
		}

		public const string ro_CCNotStoredString = "Not Stored";
		public const string ro_TXModeAuthCapture = "AUTH CAPTURE";
		public const string ro_TXModeAuthOnly = "AUTH";
		public const string ro_TXStateAuthorized = "AUTHORIZED";
		public const string ro_TXStateCaptured = "CAPTURED";
		public const string ro_TXStateVoided = "VOIDED";
		public const string ro_TXStateForceVoided = "FORCE VOIDED";
		public const string ro_TXStateRefunded = "REFUNDED";
		public const string ro_TXStateFraud = "FRAUD";
		public const string ro_TXStateUnknown = "UNKNOWN"; // possible, but not used
		public const string ro_TXStatePending = "PENDING"; // possible, but not used
		public const string ro_OK = "OK";
		public const string ro_TBD = "TBD";
		public const string ro_3DSecure = "3Denrollee";
		public const string ro_PMMicropay = "MICROPAY";
		public const string ro_PMCreditCard = "CREDITCARD";
		public const string ro_PMRequestQuote = "REQUESTQUOTE";
		public const string ro_PMCOD = "COD";
		public const string ro_PMPurchaseOrder = "PURCHASEORDER";
		public const string ro_PMPayPal = "PAYPAL";
		public const string ro_PMPayPalExpress = "PAYPALEXPRESS";
		public const string ro_PMPayPalCredit = "PAYPALCREDIT";
		public const string ro_PMPayPalExpressRedirect = "PAYPALEXPRESSDOREDIRECT";
		public const string ro_PMPayPalExpressMark = "PAYPALEXPRESSMARK"; // used for checkout flow only, order is stored as ro_PMPayPalExpress
		public const string ro_PMAmazonPayments = "AMAZONPAYMENTS";
		public const string ro_PMCheckByMail = "CHECKBYMAIL";
		public const string ro_PMPayPalEmbeddedCheckout = "PAYPALPAYMENTSADVANCED";
		public const string ro_PMBypassGateway = "BYPASSGATEWAY";

		public static readonly string[] ro_OffsitePayMentMethods =
		{
			ro_PMPayPalExpress,
			ro_PMPayPalExpressMark,
			ro_PMAmazonPayments
		};

		public static readonly string[] ro_PMsWhichAreSetToPendingForInitialTXState = {
																							   ro_PMCheckByMail, ro_PMCOD,
																							   ro_PMPurchaseOrder,
																							   ro_PMRequestQuote
																					   };

		public static bool isPendingPM(string PM)
		{
			bool setAsPending = false;
			foreach(String s in ro_PMsWhichAreSetToPendingForInitialTXState)
			{
				if(PM == s)
				{
					setAsPending = true;
					break;
				}
			}
			return setAsPending;
		}

		private const int NONE_FOUND = 0;

		public const int OUT_OF_STOCK_ALL_VARIANTS = -1;

		static readonly SkinProvider SkinProvider;
		public const string ImpersonationSessionKey = "ImpersonatingAdminId";
		public const string Braintree3dSecureKey = "Braintree.3DSApproved";
		public const string BraintreeNonceKey = "Braintree.Nonce";
		public const string BraintreePaymentMethod = "Braintree.PaymentMethod";

		static UrlHelper Url
		{ get { return DependencyResolver.Current.GetService<UrlHelper>(); } }

		static AppLogic()
		{
			SkinProvider = new SkinProvider();
		}

		public static bool HideForWholesaleSite(int CustomerLevelId)
		{
			bool isSomeSortOfDefault = CustomerLevelId == 0 || CustomerLevelId == AppConfigUSInt("DefaultCustomerLevelID");
			return AppConfigBool("WholesaleOnlySite") && isSomeSortOfDefault;
		}

		public static bool ThereAreRecurringOrdersThatNeedCCStorage()
		{
			return !AppConfigBool("Recurring.UseGatewayInternalBilling") && (DB.GetSqlN("select count(*) as N from ShoppingCart  with (NOLOCK)  where RecurringSubscriptionID='' and CartType=" + ((int)CartTypeEnum.RecurringCart).ToString()) > 0);
		}

		public static bool ThereAreRecurringGatewayAutoBillOrders()
		{
			return AppConfigBool("Recurring.UseGatewayInternalBilling") && (DB.GetSqlN("select count(*) as N from ShoppingCart  with (NOLOCK)  where RecurringSubscriptionID<>'' and CartType=" + ((int)CartTypeEnum.RecurringCart).ToString()) > 0);
		}

		public static string SkipJackEmail(int CustomerID)
		{
			string Email = "";
			using(var dbconn = DB.dbConn())
			{
				dbconn.Open();
				using(var rs = DB.GetRS("SELECT Email FROM CUSTOMER WHERE CustomerID='" + CustomerID.ToString() + "';", dbconn))
				{
					if(rs.Read())
					{
						Email = DB.RSField(rs, "Email");
					}
				}
			}
			return Email;
		}

		static public String GetCurrentEntityTemplateName(String EntityName, int UseThisEntityID)
		{
			EntityHelper helper = LookupHelper(EntityName, 0);
			int eID = CommonLogic.QueryStringUSInt(helper.GetEntitySpecs.m_EntityName + "ID");
			if(UseThisEntityID != 0)
				eID = UseThisEntityID;

			XmlNode n = helper.m_TblMgr.SetContext(eID);
			var templateName = XmlCommon.XmlField(n, "TemplateName");

			while(templateName.Length == 0 && (n = helper.m_TblMgr.MoveParent(n)) != null)
			{
				templateName = XmlCommon.XmlField(n, "TemplateName");
			}

			if(templateName.Length != 0)
			{
				if(templateName.EndsWith(".cshtml", StringComparison.InvariantCultureIgnoreCase))
					templateName = templateName.Replace(".cshtml", String.Empty);

				if(templateName.EndsWith(".vbhtml", StringComparison.InvariantCultureIgnoreCase))
					templateName = templateName.Replace(".vbhtml", String.Empty);
			}
			return templateName;
		}

		static public void LoadEntityHelpers()
		{
			CategoryStoreEntityHelper = new Dictionary<int, EntityHelper>();
			SectionStoreEntityHelper = new Dictionary<int, EntityHelper>();
			ManufacturerStoreEntityHelper = new Dictionary<int, EntityHelper>();
			DistributorStoreEntityHelper = new Dictionary<int, EntityHelper>();
			GenreStoreEntityHelper = new Dictionary<int, EntityHelper>();
			VectorStoreEntityHelper = new Dictionary<int, EntityHelper>();
			LibraryStoreEntityHelper = new Dictionary<int, EntityHelper>();

			CategoryStoreEntityHelper[0] = new EntityHelper(0, EntityDefinitions.LookupSpecs("Category"), true, 0);
			SectionStoreEntityHelper[0] = new EntityHelper(0, EntityDefinitions.LookupSpecs("Section"), true, 0);
			ManufacturerStoreEntityHelper[0] = new EntityHelper(0, EntityDefinitions.LookupSpecs("Manufacturer"), true, 0);
			DistributorStoreEntityHelper[0] = new EntityHelper(0, EntityDefinitions.LookupSpecs("Distributor"), true, 0);
			GenreStoreEntityHelper[0] = new EntityHelper(0, EntityDefinitions.LookupSpecs("Genre"), true, 0);
			VectorStoreEntityHelper[0] = new EntityHelper(0, EntityDefinitions.LookupSpecs("Vector"), true, 0);
			LibraryStoreEntityHelper[0] = new EntityHelper(0, EntityDefinitions.LookupSpecs("Library"), true, 0);

			if(GlobalConfigBool("AllowEntityFiltering"))
			{
				foreach(Store s in Store.GetStoreList())
				{
					CategoryStoreEntityHelper[s.StoreID] = new EntityHelper(0, EntityDefinitions.LookupSpecs("Category"), true, s.StoreID);
					SectionStoreEntityHelper[s.StoreID] = new EntityHelper(0, EntityDefinitions.LookupSpecs("Section"), true, s.StoreID);
					ManufacturerStoreEntityHelper[s.StoreID] = new EntityHelper(0, EntityDefinitions.LookupSpecs("Manufacturer"), true, s.StoreID);
					DistributorStoreEntityHelper[s.StoreID] = new EntityHelper(0, EntityDefinitions.LookupSpecs("Distributor"), true, s.StoreID);
					GenreStoreEntityHelper[s.StoreID] = new EntityHelper(0, EntityDefinitions.LookupSpecs("Genre"), true, s.StoreID);
					VectorStoreEntityHelper[s.StoreID] = new EntityHelper(0, EntityDefinitions.LookupSpecs("Vector"), true, s.StoreID);
					LibraryStoreEntityHelper[s.StoreID] = new EntityHelper(0, EntityDefinitions.LookupSpecs("Library"), true, s.StoreID);
				}
			}
		}

		static public string GetCurrentPageID()
		{
			var routeData = HttpContext.Current.Request.RequestContext.RouteData.Values;
			if(routeData == null || !routeData.ContainsKey(RouteDataKeys.Id))
				return string.Empty;

			return (routeData[RouteDataKeys.Id] as string) ?? string.Empty;
		}

		static public string GetCurrentPageType()
		{
			var routeData = HttpContext.Current.Request.RequestContext.RouteData.Values;
			if(routeData == null || !routeData.ContainsKey(RouteDataKeys.PageType))
				return string.Empty;

			return (routeData[RouteDataKeys.PageType] as string) ?? string.Empty;
		}

		public static String ActivePaymentGatewayRAW()
		{
			return AppConfig("PaymentGateway");
		}

		public static String ActivePaymentGatewayCleaned()
		{
			return CleanPaymentGateway(ActivePaymentGatewayRAW());
		}

		public static bool UseSpecialRecurringIntervals()
		{
			bool UseSpecial = AppConfigBool("Recurring.UseGatewayInternalBilling");
			if(UseSpecial)
			{
				String GWCleaned = ActivePaymentGatewayCleaned();
				if(!(GWCleaned == "PAYFLOWPRO"))
				{
					UseSpecial = false;
				}
			}
			return UseSpecial;
		}

		public static bool StoreCCInDB()
		{
			return AppConfigBool("StoreCCInDB");
		}

		public static int DefaultSkinID()
		{
            // nal 
            return 2;
			//return GetStoreSkinID(StoreID());
		}

		public static String LiveServer()
		{
			return AppConfig("LiveServer");
		}

		public static String MailServer()
		{
			return AppConfig("MailMe_Server");
		}

		public static String AdminDir()
		{
			return AppConfig("AdminDir");
		}

		public static bool UseSSL()
		{
			return AppConfigBool("UseSSL");
		}

		public static void NukeCustomer(int CustomerID, bool BanTheirIPAddress)
		{
			eventHandler("NukeCustomer").CallEvent("&NukeCustomer=true");
			if(BanTheirIPAddress)
			{
				using(var con = new SqlConnection(DB.GetDBConn()))
				{
					con.Open();
					using(var rs = DB.GetRS("Select LastIPAddress from Customer where CustomerID=" + CustomerID.ToString(), con))
					{
						if(rs.Read())
						{
							// ignore duplicates:
							if(DB.RSField(rs, "LastIPAddress").Length != 0)
							{
								DB.ExecuteSQL("insert RestrictedIP(IPAddress) values(" + DB.SQuote(DB.RSField(rs, "LastIPAddress")) + ")");
							}
						}
					}
				}
			}

			// remove any download folders also:
			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(var rso = DB.GetRS("Select ordernumber from orders   with (NOLOCK)  where CustomerID=" + CustomerID.ToString(), con))
				{
					while(rso.Read())
					{
						if(!DB.RSFieldDateTime(rso, "DownloadEMailSentOn").Equals(System.DateTime.MinValue))
						{
							var dirname = DB.RSFieldInt(rso, "OrderNumber").ToString() + "_" + CustomerID.ToString().ToString();
							try
							{
								Directory.Delete(CommonLogic.SafeMapPath("~/orderdownloads/" + dirname), true);
							}
							catch { }
						}
					}
				}
			}

			DB.ExecuteSQL("delete from promotionusage where CustomerID=" + CustomerID.ToString());
			DB.ExecuteSQL("delete from orders_kitcart where CustomerID=" + CustomerID.ToString());
			DB.ExecuteSQL("delete from orders_ShoppingCart where CustomerID=" + CustomerID.ToString());
			DB.ExecuteSQL("delete from orders where CustomerID=" + CustomerID.ToString());
			DB.ExecuteSQL("delete from ShoppingCart where CustomerID=" + CustomerID.ToString());
			DB.ExecuteSQL("delete from failedtransaction where CustomerID=" + CustomerID.ToString());
			DB.ExecuteSQL("delete from kitcart where CustomerID=" + CustomerID.ToString());
			DB.ExecuteSQL("delete from ratingcommenthelpfulness where RatingCustomerID=" + CustomerID.ToString() + " or VotingCustomerID=" + CustomerID.ToString());
			DB.ExecuteSQL("delete from rating where CustomerID=" + CustomerID.ToString());
			DB.ExecuteSQL("delete from CIM_AddressPaymentProfileMap where CustomerID=" + CustomerID.ToString());
			DB.ExecuteSQL("delete from Address where CustomerID=" + CustomerID.ToString());
			DB.ExecuteSQL("delete from customer where CustomerID=" + CustomerID.ToString());
		}

		public static bool VATRegistrationIDIsValid(Customer customer, string regID)
		{
			var country = customer.PrimaryBillingAddress.Country;

			country = DB.GetSqlS(string.Format("SELECT TwoLetterISOCode S FROM Country WHERE Name = {0}", DB.SQuote(country)));

			return VATRegistrationIDIsValid(country, regID);
		}

		/// <summary>
		/// Checks the VAT registration ID
		/// </summary>
		/// <param name="country">The full country name as specified in the Country table</param>
		/// <param name="regId">Customer VAT registration ID</param>
		/// <returns></returns>
		/// 
		public static bool VATRegistrationIDIsValid(string country, string regId)
		{
			var vatService = new VATCheck.checkVatService();
			var valid = false;
			var sName = string.Empty;
			var sAddress = string.Empty;

			if(!string.IsNullOrEmpty(AppConfig("VAT.VATCheckServiceURL")))
				vatService.Url = AppConfig("VAT.VATCheckServiceURL");

			regId = regId.Replace(" ", "").Trim();

			if(country.Length != 2)
			{
				country = DB.GetSqlS("select TwoLetterISOCode S FROM DBO.COUNTRY WHERE Name = " + DB.SQuote(country));
			}

			try
			{
				vatService.checkVat(ref country, ref regId, out valid, out sName, out sAddress);
			}
			catch(Exception exception)
			{
				SysLog.LogException(exception, MessageTypeEnum.GeneralException, MessageSeverityEnum.Alert);
			}

			return valid;
		}

		public static void CheckForScriptTag(String s)
		{
			if(s.Replace(" ", "").IndexOf("<script", StringComparison.InvariantCultureIgnoreCase) != -1)
			{
				throw new ArgumentException("SECURITY EXCEPTION");
			}
		}

		public static bool IsAdminSite
		{
			get
			{
				if(HttpContext.Current == null)
					return false;

				return (bool?)HttpContext.Current.Items[IsAdminSiteKey] == true;
			}
		}

		// This is expensive to call, do not call it unless you absolutely have to:
		public static AspNetHostingPermissionLevel DetermineTrustLevel()
		{
			foreach(AspNetHostingPermissionLevel trustLevel in
					new AspNetHostingPermissionLevel[] {
				AspNetHostingPermissionLevel.Unrestricted,
				AspNetHostingPermissionLevel.High,
				AspNetHostingPermissionLevel.Medium,
				AspNetHostingPermissionLevel.Low,
				AspNetHostingPermissionLevel.Minimal
			})
			{
				try
				{
					new AspNetHostingPermission(trustLevel).Demand();
				}
				catch(System.Security.SecurityException)
				{
					continue;
				}

				return trustLevel;
			}

			return AspNetHostingPermissionLevel.None;
		}

		public static bool IPIsRestricted(String IPAddress)
		{
			if(IPAddress.Length == 0)
			{
				return false;
			}
			return (DB.GetSqlN("select count(*) as N from RestrictedIP where IPAddress=" + DB.SQuote(IPAddress)) > 0);
		}

		public static bool ExceedsFailedTransactionsThreshold(Customer ThisCustomer)
		{
			int MaxAllowed = AppConfigUSInt("IPAddress.MaxFailedTransactions");
			if(MaxAllowed == 0)
			{
				MaxAllowed = 5;
			}
			int NumFailedTransactions = DB.GetSqlN("select count(*) as N from FailedTransaction where orderdate>dateadd(hh,-1,getdate()) and IPAddress=" + DB.SQuote(ThisCustomer.LastIPAddress));
			return NumFailedTransactions > MaxAllowed;
		}

		// Prevent hacking of the querystring by hacks trying to force their own payment method in here (e.g. PayPal)
		// Throw a hard error if something looks invalid
		public static void ValidatePM(string paymentMethod)
		{
			paymentMethod = CleanPaymentMethod(paymentMethod);

			if(paymentMethod == ro_PMPayPalExpressMark)
				paymentMethod = ro_PMPayPalExpress;

			if(!string.IsNullOrEmpty(paymentMethod))
			{
				if(AppConfig("PaymentMethods")
					.ParseAsDelimitedList()
					.Select(pm => CleanPaymentMethod(pm))
					.Contains(paymentMethod))
					return;

				if(MicropayIsEnabled() && paymentMethod == ro_PMMicropay)
					return;

				if(paymentMethod.IndexOf("BYPASS") != -1)
					return;

				if(paymentMethod == ro_PMPayPalExpress && ActivePaymentGatewayCleaned() == "PAYPALPRO")
					return;
			}

			throw new ArgumentException("SECURITY EXCEPTION");
		}

		public static bool VATIsEnabled()
		{
			return AppConfigBool("VAT.Enabled");
		}

		public static string GetLocaleDefaultCurrency(string LocaleSetting)
		{
			int tmp = 0;

            //using(var con = new SqlConnection(DB.GetDBConn()))
            //{
            //	con.Open();
            //	using(var rs = DB.GetRS("select DefaultCurrencyID from LocaleSetting  with (NOLOCK)  where Name=" + DB.SQuote(LocaleSetting), con))
            //	{
            //		if(rs.Read())
            //		{
            //			tmp = DB.RSFieldInt(rs, "DefaultCurrencyID");
            //		}
            //	}
            //}
            // nal
            tmp = 1;

			if(tmp == 0)
				return Localization.GetPrimaryCurrency();
			else
				return Currency.GetCurrencyCode(tmp);
		}

		// just removes all <....> markeup from the text string. this is brute force, and may or may not give
		// the right aesthetic result to the text. it just brute force removes the markeup tags
		public static string StripHtml(String s)
		{
			return Regex.Replace(s, @"<(.|\n)*?>", string.Empty, RegexOptions.Compiled);
		}

		// input CardNumber can be in plain text or encrypted, doesn't matter:
		public static String SafeDisplayCardNumber(String CardNumber, String Table, int TableID)
		{
			if(CardNumber == null || CardNumber.Length == 0)
			{
				return String.Empty;
			}
			String SaltKey = String.Empty;
			if(Table == "Address")
			{
				SaltKey = Address.StaticGetSaltKey(TableID);
			}
			else
			{
				SaltKey = Order.StaticGetSaltKey(TableID);
			}
			String CardNumberDecrypt = Security.UnmungeString(CardNumber, SaltKey);
			if(CardNumberDecrypt.StartsWith(Security.ro_DecryptFailedPrefix, StringComparison.InvariantCultureIgnoreCase))
			{
				CardNumberDecrypt = CardNumber;
			}
			if(CardNumberDecrypt == ro_CCNotStoredString)
			{
				return String.Empty;
			}
			if(CardNumberDecrypt.Length > 4)
			{
				return "****" + CardNumberDecrypt.Substring(CardNumberDecrypt.Length - 4, 4);
			}
			else
			{
				return String.Empty;
			}
		}

		// input CardExtraCode can be in plain text or encrypted, doesn't matter:
		public static String SafeDisplayCardExtraCode(String CardExtraCode)
		{
			if(CardExtraCode == null || CardExtraCode.Length == 0)
			{
				return String.Empty;
			}
			String CardExtraCodeDecrypt = Security.UnmungeString(CardExtraCode);
			if(CardExtraCodeDecrypt.StartsWith(Security.ro_DecryptFailedPrefix, StringComparison.InvariantCultureIgnoreCase))
			{
				CardExtraCodeDecrypt = CardExtraCode;
			}
			return "*".PadLeft(CardExtraCodeDecrypt.Length, '*');
		}

		// returns empty string, or decrypted card extra code from appropriate session state location
		public static String GetCardExtraCodeFromSession(Customer customer)
		{
			return Security.UnmungeString(customer.ThisCustomerSession[CardExtraCodeSessionKey]);
		}

		// stores cardextracode in appropriate session, encrypted
		public static void StoreCardExtraCodeInSession(Customer customer, String cardExtraCode)
		{
			customer.ThisCustomerSession[CardExtraCodeSessionKey] = Security.MungeString(cardExtraCode);
		}

		public static void ClearCardExtraCodeInSession(Customer customer)
		{
			customer.ThisCustomerSession.ClearVal(CardExtraCodeSessionKey);
		}

		// input CardNumber can be in plain text or encrypted, doesn't matter:
		public static String AdminViewCardNumber(String CardNumber, String Table, int TableID)
		{
			if(CardNumber.Length == 0 || CardNumber == ro_CCNotStoredString)
			{
				return CardNumber;
			}
			String SaltKey = String.Empty;
			if(Table == "Address")
			{
				SaltKey = Address.StaticGetSaltKey(TableID);
			}
			else
			{
				SaltKey = Order.StaticGetSaltKey(TableID);
			}

			String CardNumberDecrypt = Security.UnmungeString(CardNumber, SaltKey);
			if(CardNumberDecrypt.StartsWith(Security.ro_DecryptFailedPrefix, StringComparison.InvariantCultureIgnoreCase))
			{
				CardNumberDecrypt = CardNumber;
			}
			if(IsAdminSite)
			{
				return CardNumberDecrypt;
			}
			else
			{
				return SafeDisplayCardNumber(CardNumber, Table, TableID);
			}
		}

		// input CardNumber can be in plain text or encrypted, doesn't matter:
		public static String SafeDisplayCardNumberLast4(String CardNumber, String Table, int TableID)
		{
			String SaltKey = String.Empty;
			if(Table == "Address")
			{
				SaltKey = Address.StaticGetSaltKey(TableID);
			}
			else
			{
				SaltKey = Order.StaticGetSaltKey(TableID);
			}
			String CardNumberDecrypt = Security.UnmungeString(CardNumber, SaltKey);
			if(CardNumberDecrypt.StartsWith(Security.ro_DecryptFailedPrefix, StringComparison.InvariantCultureIgnoreCase))
			{
				CardNumberDecrypt = CardNumber;
			}
			if(CardNumberDecrypt == ro_CCNotStoredString)
			{
				return String.Empty;
			}
			if(CardNumberDecrypt.Length >= 4)
			{
				return CardNumberDecrypt.Substring(CardNumberDecrypt.Length - 4, 4);
			}
			else
			{
				return String.Empty;
			}
		}

		public static bool VariantAllowsCustomerPricing(int VariantID)
		{
			bool tmp = false;

			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(var rs = DB.GetRS("select CustomerEntersPrice from ProductVariant  with (NOLOCK)  where VariantID=" + VariantID.ToString(), con))
				{
					if(rs.Read())
					{
						tmp = DB.RSFieldBool(rs, "CustomerEntersPrice");
					}
				}
			}

			return tmp;
		}

		public static string ExportProductList(int categoryid, int sectionid, int manufacturerid, int distributorid, int genreid, int vectorid)
		{
			var returnXml = string.Empty;

			using(var dbconn = new SqlConnection(DB.GetDBConn()))
			{
				dbconn.Open();
				using(var cmd = new SqlCommand())
				{
					cmd.Connection = dbconn;
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.CommandText = "aspdnsf_ExportProductList";

					cmd.Parameters.Add(new SqlParameter("@categoryID", SqlDbType.Int));
					cmd.Parameters.Add(new SqlParameter("@sectionID", SqlDbType.Int));
					cmd.Parameters.Add(new SqlParameter("@manufacturerID", SqlDbType.Int));
					cmd.Parameters.Add(new SqlParameter("@distributorID", SqlDbType.Int));
					cmd.Parameters.Add(new SqlParameter("@genreID", SqlDbType.Int));
					cmd.Parameters.Add(new SqlParameter("@vectorID", SqlDbType.Int));

					cmd.Parameters["@categoryID"].Value = categoryid;
					cmd.Parameters["@sectionID"].Value = sectionid;
					cmd.Parameters["@manufacturerID"].Value = manufacturerid;
					cmd.Parameters["@distributorID"].Value = distributorid;
					cmd.Parameters["@genreID"].Value = genreid;
					cmd.Parameters["@vectorID"].Value = vectorid;

					using(SqlDataReader dr = cmd.ExecuteReader())
					{
						return DB.GetENLocaleXml(dr, "root", "product");
					}
				}
			}
		}

		public static string ImportProductList(string productImport)
		{
			if(string.IsNullOrEmpty(productImport))
				return "No data to import";

			try
			{
				DB.ExecuteStoredProcInt("aspdnsf_ImportProductPricing_XML",
					new[]
					{
						new SqlParameter("@document", SqlDbType.NText)
						{
							Value = System.Xml.Linq.XDocument
								.Parse(productImport)
								.Root
								.ToString()
						}
					});

				return string.Empty;
			}
			catch(Exception ex)
			{
				return ex.Message;
			}

		}

		// returns PM in all uppercase with only primary chars included
		static public String CleanPaymentMethod(String PM)
		{
			return PM.Replace(" ", String.Empty).Replace(".", String.Empty).Replace("(", String.Empty).Replace(")", String.Empty).Trim().ToUpperInvariant();
		}

		// returns PM in all uppercase with only primary chars included
		static public String CleanPaymentGateway(String GW)
		{
			return GW.Replace(" ", String.Empty).Replace(".", String.Empty).Replace("(", String.Empty).Replace(")", String.Empty).Trim().ToUpperInvariant();
		}

		static public EntityHelper LookupHelper(System.Collections.Generic.Dictionary<string, EntityHelper> EntityHelpers, String EntityName)
		{
			String en = EntityName.Substring(0, 1).ToUpperInvariant() + EntityName.Substring(1, EntityName.Length - 1).ToLowerInvariant();
			EntityHelper h = null;
			if(EntityHelpers == null)
			{
				h = new EntityHelper(EntityDefinitions.LookupSpecs(en), 0);
			}
			else
			{
				if(!EntityHelpers.ContainsKey(en))
				{
					h = new EntityHelper(EntityDefinitions.LookupSpecs(en), 0);
				}
				else
				{
					h = (EntityHelper)EntityHelpers[en];
				}
			}
			return h;
		}

		static public EntityHelper LookupHelper(String EntityName, int StoreID)
		{
			EntityHelper h = null;
			Dictionary<int, EntityHelper> genericEntityDictionary;

			switch(EntityName.ToUpperInvariant())
			{
				case "CATEGORY":
					genericEntityDictionary = CategoryStoreEntityHelper;
					break;
				case "SECTION":
					genericEntityDictionary = SectionStoreEntityHelper;
					break;
				case "MANUFACTURER":
					genericEntityDictionary = ManufacturerStoreEntityHelper;
					break;
				case "DISTRIBUTOR":
					genericEntityDictionary = DistributorStoreEntityHelper;
					break;
				case "GENRE":
					genericEntityDictionary = GenreStoreEntityHelper;
					break;
				case "VECTOR":
					genericEntityDictionary = VectorStoreEntityHelper;
					break;
				case "LIBRARY":
					genericEntityDictionary = LibraryStoreEntityHelper;
					break;
				default:
					h = new EntityHelper(EntityDefinitions.LookupSpecs(EntityName), StoreID);
					return h;
			}
			if(genericEntityDictionary.ContainsKey(StoreID))
			{
				h = genericEntityDictionary[StoreID];
			}
			else
			{
				h = genericEntityDictionary[0];
			}
			return h;

		}

		public static String RunXmlPackage(String XmlPackageName, Parser UseParser, Customer ThisCustomer, int SkinID, String RunTimeQuery, String RunTimeParams, bool ReplaceTokens, bool WriteExceptionMessage, HtmlHelper htmlHelper = null)
		{
			try
			{
				String pn = XmlPackageName;
				if(!pn.EndsWith(".xml.config", StringComparison.InvariantCultureIgnoreCase))
				{
					pn += ".xml.config";
				}
				var package = new XmlPackage(
					packageName: XmlPackageName,
					customer: ThisCustomer,
					userQuery: RunTimeQuery,
					additionalRuntimeParms: RunTimeParams,
					htmlHelper: htmlHelper);

				return RunXmlPackage(package, UseParser, ThisCustomer, SkinID, ReplaceTokens, WriteExceptionMessage);
			}
			catch(Exception ex)
			{
				return CommonLogic.GetExceptionDetail(ex, "");
			}
		}

		public static String RunXmlPackage(XmlPackage xmlPackage, Parser UseParser, Customer ThisCustomer, int SkinID, bool ReplaceTokens, bool WriteExceptionMessage)
		{
			StringBuilder tmpS = new StringBuilder(10000);

			if(xmlPackage != null)
			{
				String XmlPackageName = xmlPackage.PackageName;
				if(CommonLogic.ApplicationBool("DumpSQL") && !xmlPackage.PackageName.Equals("page.menu.xml.config"))
				{
					tmpS.Append("<p><b>XmlPackage: " + XmlPackageName + "</b></p>");
				}

				String s = xmlPackage.TransformString();
				if(ReplaceTokens && xmlPackage.RequiresParser)
				{
					if(UseParser == null)
					{
						UseParser = new Parser();
					}
					tmpS.Append(UseParser.ReplaceTokens(s));
				}
				else
				{
					tmpS.Append(s);
				}

				if((AppConfigBool("XmlPackage.DumpTransform") || xmlPackage.IsDebug) && !xmlPackage.PackageName.EqualsIgnoreCase("page.menu.xml.config"))
				{
					tmpS.Append("<div>");
					tmpS.Append("<h4>" + xmlPackage.PackageName + "</h4>");
					tmpS.Append("<textarea readonly style=\"width:100%; height:500px;\">" + XmlCommon.XmlEncode(XmlCommon.PrettyPrintXml(xmlPackage.PackageDocument.InnerXml)) + "</textarea>");
					tmpS.Append("</div>");

					tmpS.Append("<div>");
					tmpS.Append("<h4>" + XmlPackageName + "_store.runtime.xml</h4>");
					tmpS.Append("<textarea readonly style=\"width:100%; height:500px;\">" + XmlCommon.XmlEncode(CommonLogic.ReadFile(CommonLogic.IIF(IsAdminSite, "~/{0}".FormatWith(AdminDir()), "~/") + "images/" + XmlPackageName + "_" + CommonLogic.IIF(IsAdminSite, "admin", "store") + ".runtime.xml", true)) + "</textarea>");
					tmpS.Append("</div>");

					tmpS.Append("<div>");
					tmpS.Append("<h4>" + XmlPackageName + "_store.runtime.sql</h4>");
					tmpS.Append("<textarea readonly style=\"width:100%; height:500px;\">" + XmlCommon.XmlEncode(CommonLogic.ReadFile(CommonLogic.IIF(IsAdminSite, "~/{0}".FormatWith(AdminDir()), "~/") + "images/" + XmlPackageName + "_" + CommonLogic.IIF(IsAdminSite, "admin", "store") + ".runtime.sql", true)) + "</textarea>");
					tmpS.Append("</div>");

					tmpS.Append("<div>");
					tmpS.Append("<h4>" + XmlPackageName + "_store.xfrm.xml</h4>");
					tmpS.Append("<textarea readonly style=\"width:100%; height:500px;\">" + XmlCommon.XmlEncode(CommonLogic.ReadFile(CommonLogic.IIF(IsAdminSite, "~/{0}".FormatWith(AdminDir()), "~/") + "images/" + XmlPackageName + "_" + CommonLogic.IIF(IsAdminSite, "admin", "store") + ".xfrm.xml", true)) + "</textarea>");
					tmpS.Append("</div>");
				}
			}
			else
			{
				tmpS.Append("XmlPackage parameter is null");
			}

			return tmpS.ToString();
		}

		public static void EnsureProductHasADefaultVariantSet(int ProductID)
		{
			if(DB.GetSqlN(string.Format("SELECT COUNT(VariantID) AS N FROM ProductVariant WHERE Deleted=0 AND ProductID={0} AND IsDefault=1", ProductID.ToString())) == 0)
			{
				// force a default variant, none was specified!
				DB.ExecuteSQL(string.Format("UPDATE ProductVariant SET IsDefault=1 WHERE Deleted=0 AND ProductID={0} AND VariantID IN (SELECT TOP 1 VariantID FROM ProductVariant WHERE Deleted=0 AND Published=(SELECT CASE WHEN (SELECT COUNT(VariantID) FROM ProductVariant WHERE Deleted=0 AND Published=1 AND ProductID={0}) >= 1 THEN 1 ELSE 0 END) AND ProductID={0} ORDER BY DisplayOrder,Name)", ProductID.ToString()));
			}
			DB.ExecuteSQL(string.Format("UPDATE ProductVariant SET IsDefault=0 WHERE Deleted=1 AND ProductID={0}", ProductID.ToString()));
		}

		public static string GetCountrySelectList(string currentLocaleSetting)
		{
			if(NumLocaleSettingsInstalled() < 2)
				return string.Empty;

			var optionlist = new StringBuilder();
			var returnValue = new StringBuilder(4096);
			returnValue.Append("<!-- COUNTRY SELECT LIST -->\n");

			returnValue.Append(@"<select size='1' 
				onChange='self.location=document.getElementById(""CountrySelectList"").value' 
				id='CountrySelectList' 
				name='CountrySelectList' 
				class='CountrySelectList form-control form-control-inline'>");

			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(var rs = DB.GetRS("SELECT Name, Description FROM LocaleSetting WITH (NOLOCK) ORDER BY Displayorder, Description", con))
				{
					while(rs.Read())
					{
						var localeName = DB.RSField(rs, "Name");

						var localeLink = Url.Action(
							actionName: ActionNames.SetLocale,
							controllerName: ControllerNames.Customer,
							routeValues: new RouteValueDictionary
								{
									{ RouteDataKeys.ReturnUrl, HttpContext.Current.Request.Url.PathAndQuery },
									{ RouteDataKeys.LocaleSetting, localeName }
								});

						optionlist.AppendFormat(
							"<option value='{0}'{1}>{2}</option>",
							HttpUtility.HtmlAttributeEncode(localeLink),
							currentLocaleSetting == localeName
								? " selected "
								: string.Empty,
							HttpUtility.HtmlAttributeEncode(DB.RSField(rs, "Description")));
					}
				}
			}

			returnValue.Append(optionlist.ToString());
			returnValue.Append("</select>");
			returnValue.Append("<!-- END COUNTRY SELECT LIST -->\n");

			return returnValue.ToString();
		}

		public static string GetCurrencySelectList(Customer customer)
		{
			if(Currency.NumPublishedCurrencies() == 0)
				return string.Empty;

			var currencyLink = Url.Action(
				actionName: ActionNames.SetCurrency,
				controllerName: ControllerNames.Customer,
				routeValues: new RouteValueDictionary
					{
						{ RouteDataKeys.ReturnUrl, HttpContext.Current.Request.Url.PathAndQuery }
					});

			return Currency.GetSelectList("CurrencySelectList",
				string.Format("self.location='{0}&{1}=' + document.getElementById('CurrencySelectList').value",
					currencyLink,
					RouteDataKeys.CurrencySetting),
				"CurrencySelectList form-control form-control-inline",
				customer.CurrencySetting);
		}

		public static string GetVATSelectList(Customer customer)
		{
			if(!VATIsEnabled())
			{
				return string.Empty;
			}

			var selectName = "VATSelectList";
			var returnValue = new StringBuilder(4096);
			returnValue.AppendFormat(@"<select size='1' 
				onChange='self.location=document.getElementById(""VATSelectList"").value' 
				id='{0}' 
				name='{0}' 
				class='{0} form-control form-control-inline'>", selectName);

			var vatInclusiveLink = Url.Action(
				actionName: ActionNames.SetVatSetting,
				controllerName: ControllerNames.Customer,
				routeValues: new RouteValueDictionary
					{
						{ RouteDataKeys.ReturnUrl, HttpContext.Current.Request.Url.PathAndQuery },
						{ RouteDataKeys.VATSetting, ((int)VATSettingEnum.ShowPricesInclusiveOfVAT) }
					});

			returnValue.AppendFormat(
							"<option value='{0}'{1}>{2}</option>",
							HttpUtility.HtmlAttributeEncode(vatInclusiveLink),
							customer.VATSettingRAW == VATSettingEnum.ShowPricesInclusiveOfVAT
								? " selected "
								: string.Empty,
							GetString("setvatsetting.aspx.3"));

			var vatExclusiveLink = Url.Action(
				actionName: ActionNames.SetVatSetting,
				controllerName: ControllerNames.Customer,
				routeValues: new RouteValueDictionary
					{
						{ RouteDataKeys.ReturnUrl, HttpContext.Current.Request.Url.PathAndQuery },
						{ RouteDataKeys.VATSetting, ((int)VATSettingEnum.ShowPricesExclusiveOfVAT) }
					});

			returnValue.AppendFormat(
							"<option value='{0}'{1}>{2}</option>",
							HttpUtility.HtmlAttributeEncode(vatExclusiveLink),
							customer.VATSettingRAW == VATSettingEnum.ShowPricesExclusiveOfVAT
								? " selected "
								: string.Empty,
							GetString("setvatsetting.aspx.4"));

			returnValue.Append("</select>");

			return returnValue.ToString();
		}

		public static TimeSpan SessionTimeout()
		{
			var configuredSessionTimeout = TimeSpan.FromMinutes(AppConfigUSInt("SessionTimeoutInMinutes"));
			var limitedSessionTimeout = LimitSessionTimeout(configuredSessionTimeout);

			return limitedSessionTimeout;
		}

		public static TimeSpan AdminSessionTimeout()
		{
			var configuredSessionTimeout = TimeSpan.FromMinutes(AppConfigUSInt("AdminSessionTimeoutInMinutes"));
			var limitedSessionTimeout = LimitSessionTimeout(configuredSessionTimeout);

			return limitedSessionTimeout;
		}

		static TimeSpan LimitSessionTimeout(TimeSpan sessionTimeout)
		{
			if(sessionTimeout.TotalMinutes < 1)
				return TimeSpan.FromMinutes(15);

			if(sessionTimeout.TotalMinutes < 2)
				return TimeSpan.FromMinutes(2);

			if(sessionTimeout.TotalMinutes > 99999)
				return TimeSpan.FromMinutes(99999);

			return sessionTimeout;
		}

		public static int CacheDurationMinutes()
		{
			int ST = AppConfigUSInt("CacheDurationMinutes");
			if(ST == 0)
			{
				ST = 20;
			}
			return ST;
		}

		public static String NoPictureImageURL(bool icon, int SkinID, String LocaleSetting)
		{
			return LocateImageURL("~/skins/" + SkinProvider.GetSkinNameById(SkinID).ToLower() + "/images/nopicture" + CommonLogic.IIF(icon, "icon", String.Empty) + ".gif", LocaleSetting);
		}

		// given an input image string like /skins/{SkinName}/images/shoppingcart.gif
		// tries to resolve it to the proper locale by:
		// /skins/{SkinName}/images/shoppingcart.LocaleSetting.gif first
		// /skins/{SkinName}/images/shoppingcart.WebConfigLocale.gif second
		// /skins/{SkinName}/images/shoppingcart.gif last
		public static String LocateImageURL(String ImageName, String LocaleSetting)
		{
			String CacheName = "LocateImageURL_" + ImageName + "_" + LocaleSetting;
			if(CachingOn)
			{
				String s = (String)HttpContext.Current.Cache.Get(CacheName);
				if(s != null)
				{
					if(CommonLogic.ApplicationBool("DumpSQL"))
					{
						HttpContext.Current.Response.Write("Cache Hit Found!\n");
					}
					if(!CommonLogic.FileExists(ResolveUrl(s)) && !s.Contains("~"))
						s = "~/" + s;
					return ResolveUrl(s);
				}
			}
			int i = ImageName.LastIndexOf(".");
			String url = String.Empty;
			if(i == -1)
			{
				url = ImageName; // no extension??
			}
			else
			{
				String Extension = ImageName.Substring(i);
				url = ImageName.Substring(0, i) + "." + LocaleSetting + Extension;
				if(!CommonLogic.FileExists(url))
				{
					url = ImageName.Substring(0, i) + "." + Localization.GetDefaultLocale() + Extension;
					url = ResolveUrl(url);
				}
				if(!CommonLogic.FileExists(url))
				{
					url = ImageName.Substring(0, i) + Extension;
					url = ResolveUrl(url);
				}
			}
			if(CachingOn)
			{
				HttpContext.Current.Cache.Insert(CacheName, url, null, System.DateTime.Now.AddMinutes(CacheDurationMinutes()), TimeSpan.Zero);
			}
			if(!CommonLogic.FileExists(url) && !url.Contains("~"))
				url = "~/" + url;
			return ResolveUrl(url);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ImageName">Image filename with or without the extension</param>
		/// <param name="ImageType">e.g. Category, Section, Product</param>
		/// <param name="LocaleSetting">Viewing Locale</param>
		/// <returns>full path to the image</returns>
		public static String LocateImageURL(String ImageName, String ImageType, String ImgSize, String LocaleSetting)
		{
			try
			{
				ImageName = ImageName.Trim();
				string WebConfigLocale = "." + Localization.GetDefaultLocale();
				string IPath = GetImagePath(ImageType, ImgSize, true);
				if(LocaleSetting.Trim() != String.Empty)
				{
					LocaleSetting = "." + LocaleSetting;
				}
				bool UseCache = !IsAdminSite;

				//Used for ImageFilenameOverride
				if(ImageName.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase) || ImageName.EndsWith(".gif", StringComparison.InvariantCultureIgnoreCase) || ImageName.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase))
				{
					String[] imagepaths = { IPath + ImageName.Replace(".", LocaleSetting + "."), IPath + ImageName.Replace(".", WebConfigLocale + "."), IPath + ImageName };
					foreach(string ImagePath in imagepaths)
					{
						if(UseCache && ImageFilenameCache.ContainsKey(ImagePath) && ((String)ImageFilenameCache[ImagePath]).Length > 1)
						{
							return (String)ImageFilenameCache[ImagePath];
						}
						else if(File.Exists(ImagePath))
						{
							if(UseCache)
							{
								ImageFilenameCache[ImagePath] = GetImagePath(ImageType, ImgSize, false) + ImageName;
								return (String)ImageFilenameCache[ImagePath];
							}
							else
							{
								return GetImagePath(ImageType, ImgSize, false) + ImageName;
							}
						}
						if(UseCache && (ImageFilenameCache[ImagePath] == null || (String)ImageFilenameCache[ImagePath] == String.Empty)) ImageFilenameCache[ImagePath] = "0";
					}
					return String.Empty;
				}
				else //all other image name formats (i.e. productid, sku)
				{
					String[] imageext = { ".jpg", ".gif", ".png" };
					foreach(string ext in imageext)
					{
						String[] locales = { LocaleSetting, WebConfigLocale, String.Empty };
						foreach(string locale in locales)
						{
							string ImagePath = IPath + ImageName + locale + ext;
							if(UseCache && ImageFilenameCache.ContainsKey(ImagePath) && ((String)ImageFilenameCache[ImagePath]).Length > 1)
							{
								return (String)ImageFilenameCache[ImagePath];
							}
							else if(File.Exists(ImagePath))
							{
								if(UseCache)
								{
									ImageFilenameCache[ImagePath] = GetImagePath(ImageType, ImgSize, false) + ImageName + locale + ext;
									return (String)ImageFilenameCache[ImagePath];
								}
								else
								{
									return GetImagePath(ImageType, ImgSize, false) + ImageName + locale + ext;
								}
							}
							if(UseCache && (ImageFilenameCache[ImagePath] == null || (String)ImageFilenameCache[ImagePath] == String.Empty)) ImageFilenameCache[ImagePath] = "0";
						}
					}
					return String.Empty;
				}
			}
			catch
			{
				return String.Empty;
			}

		}

		public static String LocateImageURL(String ImageName)
		{
			return LocateImageURL(ImageName, Thread.CurrentThread.CurrentUICulture.Name);
		}

		public static String GetLocaleEntryFields(String fieldVal, String baseFormFieldName, bool useTextArea, bool htmlEncodeIt, bool isRequired, String requiredFieldMissingPrompt, int maxLength, int displaySize, int displayRows, int displayCols, bool HTMLOk)
		{
			String MasterLocale = Localization.GetDefaultLocale();
			StringBuilder tmpS = new StringBuilder(4096);
			String ThisLocale = String.Empty;

			if(displayRows == 0)
			{
				displayRows = 5;
			}
			if(displayCols == 0)
			{
				displayCols = 80;
			}

			if(NumLocaleSettingsInstalled() < 2)
			{
				// for only 1 locale, just store things directly for speed:
				ThisLocale = MasterLocale;
				String FormFieldName = baseFormFieldName;
				String ThisLocaleValue = fieldVal;
				if(fieldVal.StartsWith("<ml>", StringComparison.InvariantCultureIgnoreCase) || fieldVal.StartsWith("&lt;ml&gt;", StringComparison.InvariantCultureIgnoreCase))
				{
					ThisLocaleValue = XmlCommon.GetLocaleEntry(fieldVal, ThisLocale, false);
				}
				if(htmlEncodeIt)
				{
					ThisLocaleValue = HttpContext.Current.Server.HtmlEncode(ThisLocaleValue);
				}
				if(useTextArea)
				{
					tmpS.Append("<div id=\"id" + FormFieldName + "\" style=\"height: 1%;\">");
					tmpS.Append("<textarea class=\"text-md text-multiline\" cols=\"" + displayCols + "\" rows=\"" + displayRows.ToString() + "\" id=\"" + FormFieldName + "\" name=\"" + FormFieldName + "\">" + ThisLocaleValue + "</textarea>\n");
					tmpS.Append("</div>");
				}
				else
				{
					tmpS.Append("<input class=\"text-md\" type=\"text\" maxLength=\"" + maxLength + "\" size=\"" + displaySize + "\" id=\"" + FormFieldName + "\" name=\"" + FormFieldName + "\" value=\"" + ThisLocaleValue + "\">");
				}
				if(isRequired)
				{
					tmpS.Append("<input type=\"hidden\" name=\"" + FormFieldName + "_vldt\" value=\"[req][blankalert=" + requiredFieldMissingPrompt + " (" + ThisLocale + ")]\">\n");
				}
			}
			else
			{
				var locales = new List<string>();
				using(var con = new SqlConnection(DB.GetDBConn()))
				{
					con.Open();
					using(var rs = DB.GetRS("select Name from LocaleSetting   with (NOLOCK)  order by displayorder,description", con))
					{
						while(rs.Read())
						{
							locales.Add(DB.RSField(rs, "Name"));
						}
					}
				}

				if(useTextArea)
					tmpS.Append("<div class=\"tab-container2\" id=\"" + baseFormFieldName + "Div\">\n");
				else
					tmpS.Append("<div class=\"tab-container\" id=\"" + baseFormFieldName + "Div\">\n");

				var i = 1;
				tmpS.Append("<div class=\"tab-panes\">\n");
				tmpS.Append("<table class=\"table\">\n");
				tmpS.Append("<tbody>\n");
				foreach(var locale in locales)
				{
					tmpS.Append("<tr>\n");
					ThisLocale = locale;
					var FormFieldName = baseFormFieldName + "_" + ThisLocale.Replace("-", "_");
					var ThisLocaleValue = XmlCommon.GetLocaleEntry(fieldVal, ThisLocale, false);
					if(htmlEncodeIt)
						ThisLocaleValue = HttpContext.Current.Server.HtmlEncode(ThisLocaleValue);

					if(useTextArea)
					{
						tmpS.Append("<td>" + locale + "</td><td>");
						tmpS.Append("<div id=\"id" + FormFieldName + "\" style=\"height: 1%;\">");
						tmpS.Append(locale + ": <textarea class=\"text-multiline text-lg\" rows=\"" + displayRows.ToString() + "\" cols=\"" + displayCols.ToString() + "\" id=\"" + FormFieldName + "\" name=\"" + FormFieldName + "\">" + ThisLocaleValue + "</textarea>\n");
						tmpS.Append("</div>");
						tmpS.Append("</td>");
					}
					else
					{
						tmpS.Append("<td>" + locale + "</td><td> <input type=\"text\" class=\"text-md\" maxLength=\"" + maxLength + "\" size=\"" + displaySize + "\" id=\"" + FormFieldName + "\" name=\"" + FormFieldName + "\" value=\"" + ThisLocaleValue + "\">");
					}
					if(isRequired && ThisLocale == MasterLocale)
					{
						tmpS.Append("<span class=\"text-danger\">*</span><input type=\"hidden\" name=\"" + FormFieldName + "_vldt\" value=\"[req][blankalert=" + requiredFieldMissingPrompt + " (" + ThisLocale + ")]\">\n");
					}

					tmpS.Append("\n");
					tmpS.Append("</tr>\n");
					i++;
				}
				tmpS.Append("</tbody>\n");
				tmpS.Append("</table>\n");
				tmpS.Append("</div>\n");
				tmpS.Append("</div>\n");
			}

			return tmpS.ToString();
		}

		public static void UpdateNumLocaleSettingsInstalled()
		{
			String CacheName = "NumLocaleSettingsInstalled";
			int N = DB.GetSqlN("select count(*) as N from LocaleSetting with (NOLOCK)");
			HttpContext.Current.Cache.Insert(CacheName, N.ToString(), null, System.DateTime.Now.AddMinutes(CacheDurationMinutes()), TimeSpan.Zero);
		}

		public static int NumLocaleSettingsInstalled()
		{
			int N = 0; // can't ever be 0 really ;)
			String CacheName = "NumLocaleSettingsInstalled";
			if(CachingOn)
			{
				String s = (String)HttpContext.Current.Cache.Get(CacheName);
				if(s != null)
				{
					if(CommonLogic.ApplicationBool("DumpSQL"))
					{
						HttpContext.Current.Response.Write("Cache Hit Found!\n");
					}
					N = Localization.ParseUSInt(s);
				}
			}
			if(N == 0)
			{
				N = DB.GetSqlN("select count(*) as N from LocaleSetting with (NOLOCK)");
			}
			if(N == 0)
			{
				N = 1;
			}
			if(CachingOn)
			{
				HttpContext.Current.Cache.Insert(CacheName, N.ToString(), null, System.DateTime.Now.AddMinutes(CacheDurationMinutes()), TimeSpan.Zero);
			}

			return N;
		}

		public static string FormLocaleXml(string baseFormFieldName)
		{
			if(NumLocaleSettingsInstalled() > 1)
			{
				var tmpS = new StringBuilder(4096);
				tmpS.Append("<ml>");

				using(var con = new SqlConnection(DB.GetDBConn()))
				{
					con.Open();
					using(var rs = DB.GetRS("select * from LocaleSetting   with (NOLOCK)  order by displayorder,description", con))
					{
						while(rs.Read())
						{
							var ThisLocale = DB.RSField(rs, "Name");
							var FormFieldName = baseFormFieldName + "_" + ThisLocale.Replace("-", "_");
							var FormFieldVal = CommonLogic.FormCanBeDangerousContent(FormFieldName);
							if(FormFieldVal.Length != 0)
							{
								tmpS.Append("<locale name=\"" + ThisLocale + "\">");
								tmpS.Append(XmlCommon.XmlEncode(FormFieldVal));
								tmpS.Append("</locale>");
							}
						}
					}
				}

				tmpS.Append("</ml>");
				return tmpS.ToString();
			}
			else
			{
				return CommonLogic.FormCanBeDangerousContent(baseFormFieldName);
			}
		}

		public static string FormLocaleXml(string fieldLocaleVal, string locale)
		{
			if(NumLocaleSettingsInstalled() <= 1)
				return fieldLocaleVal;

			var mlDataBuilder = new StringBuilder(4096);
			var allLocaleNames = Localization
				.GetLocales()
				.AsEnumerable()
				.Select(row => row.Field<string>("Name"));

			mlDataBuilder.Append("<ml>");
			foreach(var iteratedLocaleName in allLocaleNames)
			{
				mlDataBuilder.AppendFormat("<locale name=\"{0}\">", iteratedLocaleName);

				if(iteratedLocaleName == locale)
					mlDataBuilder.Append(XmlCommon.XmlEncode(fieldLocaleVal));

				mlDataBuilder.Append("</locale>");
			}
			mlDataBuilder.Append("</ml>");

			return mlDataBuilder.ToString();
		}

		public static String FormLocaleXml(string sqlName, string formValue, string locale, EntitySpecs eSpecs, int eID)
		{
			if(NumLocaleSettingsInstalled() > 1)
			{
				//gets the current DB value
				string sqlNameValue = DB.GetSqlSAllLocales("SELECT " + sqlName + " AS S FROM " + eSpecs.m_EntityName + " WHERE " + eSpecs.m_EntityName + "ID=" + eID.ToString());

				StringBuilder tmpS = new StringBuilder(4096);
				tmpS.Append("<ml>");
				XmlNodeList nl = Localization.LocalesDoc.SelectNodes("//Locales");
				foreach(XmlNode xn in nl)
				{
					String thisLocale = xn.Attributes["Name"].InnerText;
					string localeEntry = XmlCommon.GetLocaleEntry(sqlNameValue, thisLocale, false);
					if(thisLocale.Equals(locale, StringComparison.InvariantCultureIgnoreCase))
					{
						tmpS.Append("<locale name=\"" + thisLocale + "\">");
						tmpS.Append(XmlCommon.XmlEncode(formValue));
						tmpS.Append("</locale>");
					}
					else
					{
						tmpS.Append("<locale name=\"" + thisLocale + "\">");
						tmpS.Append(XmlCommon.XmlEncode(localeEntry));
						tmpS.Append("</locale>");
					}
				}
				tmpS.Append("</ml>");
				return tmpS.ToString();
			}
			else
			{
				return formValue;
			}
		}

		public static string FormLocaleXml(string sqlName, string formValue, string locale, string table, int eID)
		{
			if(NumLocaleSettingsInstalled() <= 1)
				return formValue;

			var mlDataBuilder = new StringBuilder(4096);
			var currentMlData = DB.GetSqlSAllLocales(String.Format("SELECT {0} AS S FROM {1} WHERE {1}ID = {2}", sqlName, table, eID));
			var allLocaleNames = Localization
				.GetLocales()
				.AsEnumerable()
				.Select(row => row.Field<string>("Name"));

			mlDataBuilder.Append("<ml>");
			foreach(var iteratedLocaleName in allLocaleNames)
			{
				var iteratedLocaleEntry = XmlCommon.GetLocaleEntry(currentMlData, iteratedLocaleName, false);

				mlDataBuilder.AppendFormat("<locale name=\"{0}\">", iteratedLocaleName);

				if(iteratedLocaleName.Equals(locale, StringComparison.OrdinalIgnoreCase))
					mlDataBuilder.Append(XmlCommon.XmlEncode(formValue));
				else if(iteratedLocaleEntry.Length > 0)
					mlDataBuilder.Append(XmlCommon.XmlEncode(iteratedLocaleEntry));

				mlDataBuilder.Append("</locale>");
			}
			mlDataBuilder.Append("</ml>");

			return mlDataBuilder.ToString();
		}

		public static String FormLocaleXmlVariant(string sqlName, string formValue, string locale, int eID)
		{
			if(NumLocaleSettingsInstalled() > 1)
			{
				//gets the current DB value
				string sqlNameValue = DB.GetSqlSAllLocales("SELECT " + sqlName + " AS S FROM ProductVariant WHERE VariantID=" + eID.ToString());

				StringBuilder tmpS = new StringBuilder(4096);
				tmpS.Append("<ml>");
				XmlNodeList nl = Localization.LocalesDoc.SelectNodes("//Locales");
				foreach(XmlNode xn in nl)
				{
					String thisLocale = xn.Attributes["Name"].InnerText;
					string localeEntry = XmlCommon.GetLocaleEntry(sqlNameValue, thisLocale, false);
					if(thisLocale.Equals(locale, StringComparison.InvariantCultureIgnoreCase))
					{
						tmpS.Append("<locale name=\"" + thisLocale + "\">");
						tmpS.Append(XmlCommon.XmlEncode(formValue));
						tmpS.Append("</locale>");
					}
					else
					{
						tmpS.Append("<locale name=\"" + thisLocale + "\">");
						if(localeEntry.Length == 0)
						{
							XmlCommon.XmlEncode(formValue);
						}
						else
						{
							tmpS.Append(XmlCommon.XmlEncode(localeEntry));
						}
						tmpS.Append("</locale>");
					}
				}
				tmpS.Append("</ml>");
				return tmpS.ToString();
			}
			else
			{
				return formValue;
			}
		}

		public static String FormLocaleXmlEditor(string sqlName, string formName, string locale, EntitySpecs eSpecs, int eID)
		{
			if(NumLocaleSettingsInstalled() > 1)
			{
				//gets the current DB value
				string sqlNameValue = DB.GetSqlSAllLocales("SELECT " + sqlName + " AS S FROM " + eSpecs.m_EntityName + " WHERE " + eSpecs.m_EntityName + "ID=" + eID.ToString());

				StringBuilder tmpS = new StringBuilder(4096);
				tmpS.Append("<ml>");
				XmlNodeList nl = Localization.LocalesDoc.SelectNodes("//Locales");
				foreach(XmlNode xn in nl)
				{
					String thisLocale = xn.Attributes["Name"].InnerText;
					string localeEntry = XmlCommon.GetLocaleEntry(sqlNameValue, thisLocale, false);

					if(thisLocale.Equals(locale, StringComparison.InvariantCultureIgnoreCase))
					{
						tmpS.Append("<locale name=\"" + thisLocale + "\">");
						tmpS.Append(XmlCommon.XmlEncode(CommonLogic.FormCanBeDangerousContent(formName)));
						tmpS.Append("</locale>");
					}
					else
					{
						tmpS.Append("<locale name=\"" + thisLocale + "\">");
						tmpS.Append(XmlCommon.XmlEncode(localeEntry));
						tmpS.Append("</locale>");
					}
				}
				tmpS.Append("</ml>");
				return tmpS.ToString();
			}
			else
			{
				return CommonLogic.FormCanBeDangerousContent(formName);
			}
		}

		static public string FormLocaleXmlEditor(string sqlName, string formName, string locale, string table, int eID)
		{
			return FormLocaleXml(
				sqlName: sqlName,
				formValue: CommonLogic.FormCanBeDangerousContent(formName),
				locale: locale,
				table: table,
				eID: eID);
		}

		public static String FormLocaleXmlEditorVariant(string sqlName, string formName, string locale, int eID)
		{
			if(NumLocaleSettingsInstalled() > 1)
			{
				//gets the current DB value
				string sqlNameValue = DB.GetSqlSAllLocales("SELECT " + sqlName + " AS S FROM ProductVariant WHERE VariantID=" + eID.ToString());

				StringBuilder tmpS = new StringBuilder(4096);
				tmpS.Append("<ml>");
				XmlNodeList nl = Localization.LocalesDoc.SelectNodes("//Locales");
				foreach(XmlNode xn in nl)
				{
					String thisLocale = xn.Attributes["Name"].InnerText;
					string localeEntry = XmlCommon.GetLocaleEntry(sqlNameValue, thisLocale, false);

					if(thisLocale.Equals(locale, StringComparison.InvariantCultureIgnoreCase))
					{
						tmpS.Append("<locale name=\"" + thisLocale + "\">");
						tmpS.Append(XmlCommon.XmlEncode(CommonLogic.FormCanBeDangerousContent(formName)));
						tmpS.Append("</locale>");
					}
					else
					{
						tmpS.Append("<locale name=\"" + thisLocale + "\">");
						tmpS.Append(XmlCommon.XmlEncode(localeEntry));
						tmpS.Append("</locale>");
					}
				}
				tmpS.Append("</ml>");
				return tmpS.ToString();
			}
			else
			{
				return CommonLogic.FormCanBeDangerousContent(formName);
			}
		}

		public static String GetFormsDefaultLocale(string sqlName, string formValue, string locale, EntitySpecs eSpecs, int eID)
		{
			if(NumLocaleSettingsInstalled() > 1)
			{
				//gets the current DB value
				string sqlNameValue = DB.GetSqlSAllLocales("SELECT " + sqlName + " AS S FROM " + eSpecs.m_EntityName + " WHERE " + eSpecs.m_EntityName + "ID=" + eID.ToString());

				XmlNodeList nl = Localization.LocalesDoc.SelectNodes("//Locales");
				foreach(XmlNode xn in nl)
				{
					String thisLocale = xn.Attributes["Name"].InnerText;
					string localeEntry = XmlCommon.GetLocaleEntry(sqlNameValue, thisLocale, false);

					if(thisLocale.Equals(Localization.GetDefaultLocale(), StringComparison.InvariantCultureIgnoreCase))
					{
						if(thisLocale.Equals(locale, StringComparison.InvariantCultureIgnoreCase))
						{
							return formValue;
						}
						else
						{
							return localeEntry;
						}
					}
				}
				return formValue;
			}
			else
			{
				return formValue;
			}
		}

		public static string GetFormsDefaultLocale(string sqlName, string formValue, string locale, string table, int eID)
		{
			if(NumLocaleSettingsInstalled() > 1)
			{
				//gets the current DB value
				var sqlNameValue = DB.GetSqlSAllLocales("SELECT " + sqlName + " AS S FROM " + table + " WHERE " + table + "ID=" + eID.ToString());

				using(var con = new SqlConnection(DB.GetDBConn()))
				{
					con.Open();
					using(var rs = DB.GetRS("select * from LocaleSetting   with (NOLOCK)  order by displayorder,description", con))
					{
						while(rs.Read())
						{
							var thisLocale = DB.RSField(rs, "Name");
							var localeEntry = XmlCommon.GetLocaleEntry(sqlNameValue, thisLocale, false);

							if(thisLocale.Equals(Localization.GetDefaultLocale(), StringComparison.InvariantCultureIgnoreCase))
							{
								if(thisLocale.Equals(locale, StringComparison.InvariantCultureIgnoreCase))
									return formValue;
								else
									return localeEntry;
							}
						}
					}
				}

				return formValue;
			}
			else
			{
				return formValue;
			}
		}

		// if an attribute (size/color) has a price modifier, this removes that and just returns the
		// base color/size portion
		public static String RemoveAttributePriceModifier(String s)
		{
			char[] splitchars = { '[', ']' };
			String[] x = s.Split(splitchars);
			String tmp = x[0];
			return tmp.Trim();
		}

		/// <summary>
		/// Transfers customer related data from the previous anonymous record to the new authenticated
		///  customer record as a result of logging on. If the appconfig ClearOldCartOnSignin is true,
		///  data will be deleted from the authenticated customer record. If the appconfig
		///  PreserveActiveCartOnSignin is true, data will be moved from the anonymous record to the
		///  authenticated record.
		/// </summary>
		/// <param name="anonymousCustomerId">Customer id of the anonymous record that was first established when the customer was cookied.</param>
		/// <param name="authenticatedCustomerId">Customer id of the authenticated record for the current customer.</param>
		public static void ExecuteSigninLogic(Customer anonymousCustomer, Customer authenticatedCustomer)
		{
			var coupon = string.Empty;

			var referrer = HttpContext
				.Current
				.Profile
				.GetPropertyValue("Referrer")
				.ToString();

			var skinId = HttpContext
				.Current
				.Profile
				.GetPropertyValue("SkinID")
				.ToString();

			if(anonymousCustomer != null)
			{
				using(var connection = new SqlConnection(DB.GetDBConn()))
				{
					connection.Open();
					using(var reader = DB.GetRS("select CouponCode from Customer with (nolock) where CustomerId = @anonymousCustomerId",
						connection,
						new SqlParameter("anonymousCustomerId", anonymousCustomer.CustomerID)))
					{
						if(reader.Read())
						{
							coupon = DB.RSField(reader, "CouponCode");
						}
					}
				}

				if(!string.IsNullOrEmpty(coupon))
					authenticatedCustomer.UpdateCustomer(couponCode: coupon);

				if(!string.IsNullOrWhiteSpace(referrer))
					authenticatedCustomer.UpdateCustomer(referrer: referrer);
			}

			if(AppConfigBool("ClearOldCartOnSignin"))
			{
				DB.ExecuteSQL("delete from ShoppingCart where CartType = @cartType and CustomerId = @authenticatedCustomerId",
					new SqlParameter("authenticatedCustomerId", authenticatedCustomer.CustomerID),
					new SqlParameter(
						parameterName: "cartType",
						value: CartTypeEnum.ShoppingCart));

				DB.ExecuteSQL("delete from KitCart where CartType = @cartType and CustomerId = @authenticatedCustomerId",
					new SqlParameter("authenticatedCustomerId", authenticatedCustomer.CustomerID),
					new SqlParameter(
						parameterName: "cartType",
						value: CartTypeEnum.ShoppingCart));

				DB.ExecuteSQL("delete from ShoppingCart where CartType = @cartType and CustomerId = @authenticatedCustomerId",
					new SqlParameter("authenticatedCustomerId", authenticatedCustomer.CustomerID),
					new SqlParameter(
						parameterName: "cartType",
						value: CartTypeEnum.WishCart));

				DB.ExecuteSQL("delete from KitCart where CartType = @cartType and CustomerId = @authenticatedCustomerId",
					new SqlParameter("authenticatedCustomerId", authenticatedCustomer.CustomerID),
					new SqlParameter(
						parameterName: "cartType",
						value: CartTypeEnum.WishCart));

				PromotionManager.ClearAllPromotionUsages(authenticatedCustomer.CustomerID);
			}

			if(anonymousCustomer != null && anonymousCustomer.CustomerID > 0 && AppConfigBool("PreserveActiveCartOnSignin"))
			{
				if(ShoppingCart.NumItems(anonymousCustomer.CustomerID, CartTypeEnum.ShoppingCart) > 0)
				{
					DB.ExecuteSQL("update ShoppingCart set CustomerID = @authenticatedCustomerId, BillingAddressId = @billingAddressId, ShippingAddressId = @shippingAddressId where CartType = @cartType and CustomerId = @anonymousCustomerId",
						new SqlParameter("anonymousCustomerId", anonymousCustomer.CustomerID),
						new SqlParameter("authenticatedCustomerId", authenticatedCustomer.CustomerID),
						new SqlParameter("billingAddressId", authenticatedCustomer.PrimaryBillingAddressID),
						new SqlParameter("shippingAddressId", authenticatedCustomer.PrimaryShippingAddressID),
						new SqlParameter(
							parameterName: "cartType",
							value: CartTypeEnum.ShoppingCart));

					DB.ExecuteSQL("update KitCart set CustomerId = @authenticatedCustomerId where CartType = @cartType and CustomerId = @anonymousCustomerId",
						new SqlParameter("anonymousCustomerId", anonymousCustomer.CustomerID),
						new SqlParameter("authenticatedCustomerId", authenticatedCustomer.CustomerID),
						new SqlParameter(
							parameterName: "cartType",
							value: CartTypeEnum.ShoppingCart));

					PromotionManager.TransferPromotionsOnUserLogin(anonymousCustomer.CustomerID, authenticatedCustomer.CustomerID);
				}

				if(ShoppingCart.NumItems(anonymousCustomer.CustomerID, CartTypeEnum.WishCart) > 0)
				{
					DB.ExecuteSQL("update ShoppingCart set CustomerID = @authenticatedCustomerId where CartType = @cartType and CustomerId = @anonymousCustomerId",
						new SqlParameter("anonymousCustomerId", anonymousCustomer.CustomerID),
						new SqlParameter("authenticatedCustomerId", authenticatedCustomer.CustomerID),
						new SqlParameter(
							parameterName: "cartType",
							value: CartTypeEnum.WishCart));

					DB.ExecuteSQL("update KitCart set CustomerID = @authenticatedCustomerId where CartType = @cartType and CustomerId = @anonymousCustomerId",
						new SqlParameter("anonymousCustomerId", anonymousCustomer.CustomerID),
						new SqlParameter("authenticatedCustomerId", authenticatedCustomer.CustomerID),
						new SqlParameter(
							parameterName: "cartType",
							value: CartTypeEnum.WishCart)); ;
				}
			}

			if(anonymousCustomer != null && anonymousCustomer.CustomerID > 0 && AppConfigBool("PreserveActiveAddressOnSignin"))
			{
				DB.ExecuteSQL("update Address set CustomerId = @authenticatedCustomerId where CustomerId = @anonymousCustomerId",
					new SqlParameter("anonymousCustomerId", anonymousCustomer.CustomerID),
					new SqlParameter("authenticatedCustomerId", authenticatedCustomer.CustomerID));
			}

			var authenticatedCustomerShoppingCart = new ShoppingCart(authenticatedCustomer.SkinID, authenticatedCustomer, CartTypeEnum.ShoppingCart, 0, false);
			authenticatedCustomerShoppingCart.ApplyShippingRules();

			if(anonymousCustomer != null)
			{
				authenticatedCustomer.UpdateCustomer(
					requestedPaymentMethod: anonymousCustomer.RequestedPaymentMethod);

				authenticatedCustomer.ThisCustomerSession["checkoutcontext"] =
					anonymousCustomer.ThisCustomerSession["checkoutcontext"];

				//If the customer signed in through checkout there will be an email address on the anon record, so clear that out
				if(anonymousCustomer != null && !anonymousCustomer.IsRegistered)
					anonymousCustomer.UpdateCustomer(email: string.Empty);
			}

			Customer.ClearAllCustomerProfile();

			switch(AppConfig("Signin.SkinMaster").ToLower())
			{
				case "session":
					HttpContext.Current.Profile.SetPropertyValue("SkinID", skinId.ToString());
					int sessionSkinID;
					if(int.TryParse(skinId, out sessionSkinID)
						&& authenticatedCustomer.SkinID != sessionSkinID)
					{
						authenticatedCustomer.SkinID = sessionSkinID;
						authenticatedCustomer.UpdateCustomer(new SqlParameter[]
							{
								new SqlParameter("SkinID", sessionSkinID)
							});
					}
					break;

				case "default":
					HttpContext.Current.Profile.SetPropertyValue("SkinID", DefaultSkinID().ToString());
					if(authenticatedCustomer.SkinID != DefaultSkinID())
					{
						authenticatedCustomer.SkinID = DefaultSkinID();
						authenticatedCustomer.UpdateCustomer(new SqlParameter[]
							{
								new SqlParameter("SkinID", DefaultSkinID())
							});
					}
					break;
			}
        }

		// examines the specified option string, which should correspond to a size or color option in the product variant,
		// and returns JUST the main option text, removing any cost delta specifiers
		public static String CleanSizeColorOption(String s)
		{
			String tmp = s;
			int i = s.IndexOf("[");
			if(i > 0)
			{
				tmp = s.Substring(0, i).Trim();
			}
			return tmp.Trim();
		}

		public static void SendDistributorNotifications(Order ord)
		{
			var MailServer = AppLogic.MailServer();

			if(MailServer.Length != 0 &&
				false == MailServer.Equals(ro_TBD, StringComparison.InvariantCultureIgnoreCase))
			{
				var DistCC = AppConfig("DistributorEMailCC").Trim();
				var SubjectReceipt = AppConfig("StoreName") + " - Distributor Notification, Order #" + ord.OrderNumber.ToString();
				var sql = "select distinct DistributorID,EMail from Distributor  with (NOLOCK)  where DistributorID in (select distinct DistributorID from Orders_ShoppingCart where OrderNumber=" + ord.OrderNumber.ToString() + " and (DistributorID IS NOT NULL and DistributorID <> 0))";

				using(var con = new SqlConnection(DB.GetDBConn()))
				{
					con.Open();
					using(var rs = DB.GetRS(sql, con))
					{
						while(rs.Read())
						{
							if(DB.RSFieldInt(rs, "DistributorID") != 0)
							{
								var EM = DB.RSField(rs, "EMail").Trim();
								EM = EM.Replace(";", "|").Replace(",", "|").Replace(" ", "|");
								var first = true;
								foreach(var emx in EM.Split('|'))
								{
									if(emx.Length != 0)
									{
										try
										{
											SendMail(subject: SubjectReceipt, body: ord.DistributorNotification(DB.RSFieldInt(rs, "DistributorID")) + AppConfig("MailFooter"), useHtml: true, fromAddress: AppConfig("ReceiptEMailFrom"), fromName: AppConfig("ReceiptEMailFromName"), toAddress: emx, toName: emx, bccAddresses: CommonLogic.IIF(DistCC.Length != 0, DistCC, String.Empty), replyToAddress: AppConfig("ReceiptEMailFrom"));
											if(first)
											{
												DB.ExecuteSQL("update Orders set DistributorEMailSentOn=getdate() where OrderNumber=" + ord.OrderNumber.ToString());
											}
											first = false;
										}
										catch { }
									}
								}
							}
						}
					}
				}
			}
		}

		public static string GetAllDistributorNotifications(Order ord)
		{
			var tmpS = new StringBuilder(10000);
			var SubjectReceipt = AppConfig("StoreName") + " - Distributor Notification, Order #" + ord.OrderNumber.ToString();
			var sql = "select distinct Name, DistributorID,EMail from Distributor  with (NOLOCK)  where DistributorID in (select distinct DistributorID from Orders_ShoppingCart where OrderNumber=" + ord.OrderNumber.ToString() + " and (DistributorID IS NOT NULL and DistributorID <> 0))";

			bool first = true;

			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(var rs = DB.GetRS(sql, con))
				{
					while(rs.Read())
					{
						if(DB.RSFieldInt(rs, "DistributorID") != 0)
						{
							if(!first)
							{
								tmpS.Append("<hr/>");
							}
							var EM = DB.RSField(rs, "EMail").Trim();
							tmpS.Append("<p><b>Distributor Name: " + DB.RSField(rs, "Name") + "</b></p>");
							tmpS.Append("<p><b>Distributor ID: " + DB.RSFieldInt(rs, "DistributorID").ToString() + "</b></p>");
							tmpS.Append("<p><b>Distributor E-Mail: " + EM + "</b></p>");
							tmpS.Append("<p><b>Distributor XmlPackage: " + ord.GetDistributorNotificationPackageToUse(DB.RSFieldInt(rs, "DistributorID")) + "</b></p>");
							tmpS.Append("<p><b>Notification E-Mail Subject: " + SubjectReceipt + "</b></p>");
							tmpS.Append("<p><b>Notification E-Mail Body:</b></p>");
							tmpS.Append("<div>");
							tmpS.Append(ord.DistributorNotification(DB.RSFieldInt(rs, "DistributorID")) + AppConfig("MailFooter"));
							tmpS.Append("</div>");
							first = false;
						}
					}
				}
			}

			return tmpS.ToString();
		}

		public static void SendOrderEMail(Customer activeCustomer, int orderNumber, bool isRecurring, String paymentMethod, bool notifyStoreAdmin)
		{
			Order ord = new Order(orderNumber, activeCustomer.LocaleSetting);
			bool UseLiveTransactions = AppConfigBool("UseLiveTransactions");
			String StoreName = AppConfig("StoreName");
			String MailServer = AppLogic.MailServer();

			String OrdPM = CleanPaymentMethod(ord.PaymentMethod);
			String SubjectReceipt = String.Empty;
			if(UseLiveTransactions)
			{
				SubjectReceipt = String.Format(GetString("common.cs.1", ord.SkinID, ord.LocaleSetting), StoreName);
			}
			else
			{
				SubjectReceipt = String.Format(GetString("common.cs.2", ord.SkinID, ord.LocaleSetting), StoreName);
			}
			if(OrdPM == ro_PMRequestQuote)
			{
				SubjectReceipt += GetString("common.cs.3", ord.SkinID, ord.LocaleSetting);
			}
			String SubjectNotification = String.Empty;
			if(UseLiveTransactions)
			{
				SubjectNotification = String.Format(GetString("common.cs.4", ord.SkinID, ord.LocaleSetting), StoreName);
			}
			else
			{
				SubjectNotification = String.Format(GetString("common.cs.5", ord.SkinID, ord.LocaleSetting), StoreName);
			}
			if(OrdPM == ro_PMRequestQuote)
			{
				SubjectNotification += GetString("common.cs.3", ord.SkinID, ord.LocaleSetting);
			}

			if(isRecurring)
			{
				SubjectReceipt += GetString("common.cs.6", ord.SkinID, ord.LocaleSetting);
			}

			if(notifyStoreAdmin)
			{
				// send E-Mail notice to store admin:
				if(ord.ReceiptEMailSentOn.Equals(System.DateTime.MinValue))
				{
					try
					{
						if(AppConfig("GotOrderEMailTo").Length != 0 && !AppConfigBool("TurnOffStoreAdminEMailNotifications"))
						{
							String SendToList = AppConfig("GotOrderEMailTo").Replace(",", ";");
							if(SendToList.IndexOf(';') != -1)
							{
								foreach(String s in SendToList.Split(';'))
								{
									SendMail(subject: SubjectNotification, body: ord.AdminNotification() + AppConfig("MailFooter"), useHtml: true, fromAddress: AppConfig("GotOrderEMailFrom"), fromName: AppConfig("GotOrderEMailFromName"), toAddress: s.Trim(), toName: s.Trim(), bccAddresses: String.Empty, server: AppLogic.MailServer());
								}
							}
							else
							{
								SendMail(subject: SubjectNotification, body: ord.AdminNotification() + AppConfig("MailFooter"), useHtml: true, fromAddress: AppConfig("GotOrderEMailFrom"), fromName: AppConfig("GotOrderEMailFromName"), toAddress: SendToList, toName: SendToList, bccAddresses: String.Empty, server: AppLogic.MailServer());
							}
						}
					}
					catch { }
				}
				// send SMS notice to store admin:
				if(ord.ReceiptEMailSentOn.Equals(System.DateTime.MinValue))
				{
					// SEND CELL MESSAGE NOTIFICATION:
					try
					{
						SMS.Send(ord, AppConfig("ReceiptEMailFrom"), MailServer, activeCustomer);
					}
					catch { }
				}
			}

			//  now send customer e-mails:
			bool OKToSend = false;
			if(ord.BillingAddress.m_EMail != "")
			{
				if(isRecurring)
				{
					if(AppConfigBool("Recurring.SendOrderEMailToCustomer") && MailServer.Length != 0 && MailServer != ro_TBD)
					{
						OKToSend = true;
					}
				}
				else
				{
					if(AppConfigBool("SendOrderEMailToCustomer") && MailServer.Length != 0 && MailServer != ro_TBD)
					{
						OKToSend = true;
					}
				}
			}
			if(OKToSend)
			{
				try
				{
					// NOTE: we changed this to ALWAYS send the receipt:
					if(ord.ReceiptEMailSentOn.Equals(System.DateTime.MinValue))
					{
						try
						{
							SendMail(subject: SubjectReceipt, body: ord.Receipt(activeCustomer, true) + AppConfig("MailFooter"), useHtml: true, fromAddress: AppConfig("ReceiptEMailFrom"), fromName: AppConfig("ReceiptEMailFromName"), toAddress: ord.BillingAddress.m_EMail, toName: ord.BillingAddress.m_EMail, bccAddresses: String.Empty, replyToAddress: AppConfig("ReceiptEMailFrom"));
							DB.ExecuteSQL("update Orders set ReceiptEMailSentOn=getdate() where OrderNumber=" + ord.OrderNumber.ToString());
						}
						catch { }
					}
				}
				catch { }
			}
			bool DelayTheDropShipNotification = AppConfigBool("DelayedDropShipNotifications");
			if(!DelayTheDropShipNotification && (AppConfigBool("MaxMind.Enabled") && ord.MaxMindFraudScore >= AppConfigNativeDecimal("MaxMind.DelayDropShipThreshold")))
			{
				DelayTheDropShipNotification = true; // delay it anyway if maxmind fraud score is too high!
			}
			if(!DelayTheDropShipNotification && ord.TransactionIsCaptured() && ord.DistributorEMailSentOn.Equals(System.DateTime.MinValue) && ord.HasDistributorComponents())
			{
				SendDistributorNotifications(new Order(ord.OrderNumber, ord.ViewInLocaleSetting)); // must reload order object to make this call work!
			}
		}

		public static String GetRecurringCart(Parser UseParser, Customer ThisCustomer, int OriginalRecurringOrderNumber, int SkinID, bool OnlyLoadRecurringItemsThatAreDue, bool ShowCancelButton, bool ShowRetryButton, bool ShowRestartButton, String GatewayStatus)
		{
			ShoppingCart cart = new ShoppingCart(SkinID, ThisCustomer, CartTypeEnum.RecurringCart, OriginalRecurringOrderNumber, OnlyLoadRecurringItemsThatAreDue);
			StringBuilder tmpS = new StringBuilder(10000);

			tmpS.Append(cart.DisplayRecurring(OriginalRecurringOrderNumber, SkinID, ShowCancelButton, ShowRetryButton, ShowRestartButton, GatewayStatus, ThisCustomer.LocaleSetting, UseParser));

			return tmpS.ToString();
		}

		public static string GetRecurringSubscriptionIDFromOrder(int OrderNumber)
		{
			var RecurringSubscriptionID = string.Empty;

			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(var rs = DB.GetRS("Select RecurringSubscriptionID from Orders  with (NOLOCK)  where OrderNumber=" + OrderNumber.ToString(), con))
				{
					if(rs.Read())
					{
						RecurringSubscriptionID = DB.RSField(rs, "RecurringSubscriptionID");
					}
				}
			}

			return RecurringSubscriptionID;
		}

		public static int GetOriginalRecurringOrderNumberFromSubscriptionID(string RecurringSubscriptionID)
		{
			int OrderNumber = 0;

			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(var rs = DB.GetRS("Select top 1 OriginalRecurringOrderNumber from ShoppingCart  with (NOLOCK)  where RecurringSubscriptionID=" + DB.SQuote(RecurringSubscriptionID), con))
				{
					if(rs.Read())
					{
						OrderNumber = DB.RSFieldInt(rs, "OriginalRecurringOrderNumber");
					}
				}
			}

			return OrderNumber;
		}

		static public string GetRecurringVariantsList()
		{
			using(var connection = new SqlConnection(DB.GetDBConn()))
			{
				connection.Open();
				return GetRecurringVariantsList(connection);
			}
		}

		static public string GetRecurringVariantsList(SqlConnection connection)
		{
			var cacheKey = "GetRecurringVariantsList";
			if(CachingOn)
			{
				var cachedValue = HttpContext.Current.Cache.Get(cacheKey) as string;
				if(cachedValue != null)
				{
					if(CommonLogic.ApplicationBool("DumpSQL"))
						HttpContext.Current.Response.Write("Cache Hit Found!\n");

					return cachedValue;
				}
			}

			var variantIds = new List<int>();
			using(var command = connection.CreateCommand())
			{
				command.CommandText = "select VariantID from ProductVariant with(nolock) where IsRecurring = 1 and Deleted = 0";

				using(var reader = command.ExecuteReader())
					while(reader.Read())
						variantIds.Add(reader.FieldInt("VariantID"));
			}

			var variantIdCommaSeparatedList = string.Join(",", variantIds);

			if(CachingOn)
				HttpContext.Current.Cache.Insert(cacheKey, variantIdCommaSeparatedList, null, DateTime.Now.AddMinutes(CacheDurationMinutes()), TimeSpan.Zero);

			return variantIdCommaSeparatedList;
		}

		public static bool IsValidAffiliate(int AffiliateID)
		{
			string query = string.Format("Select count(a.AffiliateID) as N from Affiliate a with (NOLOCK) inner join(select a.AffiliateID from Affiliate a with (nolock) left join AffiliateStore b with (nolock) on a.AffiliateID = b.AffiliateID " +
							"where ({0}= 0 or StoreID = {1})) b on a.AffiliateID = b.AffiliateID where Deleted=0 and a.AffiliateID = {2}", CommonLogic.IIF(GlobalConfigBool("AllowAffiliateFiltering") == true, 1, 0), StoreID(), AffiliateID);
			return (DB.GetSqlN(query) != 0);
		}

		public static string GetProductSEName(int productID, string localeSetting)
		{
			var tmpS = string.Empty;
			SqlParameter[] spa = { DB.CreateSQLParameter("@ProductID", SqlDbType.Int, 4, productID, ParameterDirection.Input) };

			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(var rs = DB.GetRS("select SEName from Product  with (NOLOCK)  where ProductID = @ProductID", spa, con))
				{
					if(rs.Read())
					{
						tmpS = DB.RSFieldByLocale(rs, "SEName", localeSetting);
					}
				}
			}

			return tmpS;
		}

		public static string GetVariantSEName(int variantID, string localeSetting)
		{
			var tmpS = string.Empty;
			SqlParameter[] spa = { DB.CreateSQLParameter("@VariantID", SqlDbType.Int, 4, variantID, ParameterDirection.Input) };

			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(var rs = DB.GetRS("select SEName from ProductVariant  with (NOLOCK)  where VariantID = @VariantID", spa, con))
				{
					if(rs.Read())
					{
						tmpS = DB.RSFieldByLocale(rs, "SEName", localeSetting);
					}
				}
			}

			return tmpS;
		}

		public static string GetEntitySEName(int EntityID, string entityType, string localeSetting)
		{
			return GetEntitySEName(EntityID, (EntityType)Enum.Parse(typeof(EntityType), entityType, true), localeSetting);
		}

		public static string GetEntitySEName(int EntityID, EntityType entityType, string localeSetting)
		{
			var tmpS = string.Empty;
			SqlParameter[] spa =
			{
				new SqlParameter("@EntityID", EntityID)
			};

			if(entityType == EntityType.Affiliate || entityType == EntityType.CustomerLevel || entityType == EntityType.Unknown)
			{
				throw new ArgumentException("Unsupported EntityType", "entityType");
			}

			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(var rs = DB.GetRS("select SEName from " + entityType.ToString() + " with (NOLOCK) where " + entityType.ToString() + "ID = @EntityID", spa, con))
				{
					if(rs.Read())
					{
						tmpS = DB.RSFieldByLocale(rs, "SEName", localeSetting);
					}
				}
			}

			return tmpS;
		}

		static public int GetNextOrderNumber()
		{
			var NewGUID = CommonLogic.GetNewGUID();
			DB.ExecuteSQL("insert into OrderNumbers(OrderNumberGUID) values(" + DB.SQuote(NewGUID) + ")");
			int tmp = 0;

			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(var rs = DB.GetRS("Select OrderNumber from OrderNumbers  with (NOLOCK)  where OrderNumberGUID=" + DB.SQuote(NewGUID), con))
				{
					if(rs.Read())
					{
						tmp = DB.RSFieldInt(rs, "ordernumber");
					}
				}
			}

			return tmp;
		}

		static public String TransactionMode()
		{
			String tmpS = AppConfig("TransactionMode").Trim().ToUpperInvariant();
			if(tmpS.Length == 0)
			{
				tmpS = ro_TXModeAuthOnly; // forcefully set SOME default!
			}
			return tmpS;
		}

		static public bool TransactionModeIsAuthCapture()
		{
			return TransactionMode() != ro_TXModeAuthOnly;
		}

		public static bool TransactionModeIsAuthOnly()
		{
			return !TransactionModeIsAuthCapture();
		}

		/// <summary>
		/// Gets the default store
		/// </summary>
		/// <returns></returns>
		public static Store GetDefaultStore()
		{
			Store defStore = null;

			try
			{
				var stores = Store.GetStoreList();
				defStore = stores.FirstOrDefault(store => store.IsDefault);
			}
			catch(Exception ex)
			{
				var logEx = new Exception("Default store not found", ex);
				SysLog.LogException(logEx, MessageTypeEnum.GeneralException, MessageSeverityEnum.Message);
			}

			return defStore;
		}

		public static bool MicropayIsEnabled()
		{
			return AppConfig("PaymentMethods").IndexOf(ro_PMMicropay, StringComparison.InvariantCultureIgnoreCase) != -1;
		}

		/// <summary>
		/// Gets the Theme aware skin image directory
		/// </summary>
		/// <returns></returns>
		public static string SkinImageDir()
		{
			var handler = HttpContext.Current.Handler;
			if(handler != null && handler is Page)
			{
				if(IsAdminSite)
					return "App_Themes/{0}/images".FormatWith((handler as Page).Theme);

				return "Skins/{0}/images".FormatWith((handler as Page).Theme);
			}
			else
			{
				Customer ThisCustomer = HttpContext.Current.GetCustomer();
				if(IsAdminSite)
					return "App_Themes/Skin_{0}/images".FormatWith(ThisCustomer.SkinID.ToString());

				return "Skins/{0}/images".FormatWith(SkinProvider.GetSkinNameById(ThisCustomer.SkinID));
			}
		}

		/// <summary>
		/// Gets the relative image path from the Skins/{SkinName} directory for the specified image
		/// </summary>
		/// <param name="fileName">The image file name</param>
		/// <returns>The relative path from the themes/image directory</returns>
		public static string SkinImage(string fileName)
		{
			return LocateImageURL(ResolveUrl("~/{0}/{1}".FormatWith(SkinImageDir(), fileName)));
		}

		/// <summary>
		/// Determines if  product is out of stock base on the inventory and OutOfStockThreshold appconfig
		/// </summary>
		/// <param name="productId">The product id</param>
		/// <param name="variantId">The variant id</param>
		/// <param name="TrackInventoryBySizeAndColor"></param>
		/// <returns>True or False</returns>
		public static bool ProbablyOutOfStock(int productId, int variantId, bool trackInventoryBySizeAndColor)
		{
			return ProbablyOutOfStock(productId, variantId, trackInventoryBySizeAndColor, "Product");
		}

		/// <summary>
		/// Determines if  product is out of stock base on the inventory and OutOfStockThreshold appconfig
		/// </summary>
		/// <param name="productId">The product id</param>
		/// <param name="variantId">The variant id</param>
		/// <param name="trackInventoryBySizeAndColor">Determine if inventroy is track by sizes and color</param>
		/// <param name="page"></param>
		/// <returns>True or False</returns>
		public static bool ProbablyOutOfStock(int productId, int variantId, bool trackInventoryBySizeAndColor, string page)
		{
			if(IsAKit(productId))
			{
				KitProductData kit = KitProductData.Find(productId, Customer.Current);
				return !kit.HasStock;
			}
			else
			{
				return DetermineOutOfStock(productId, variantId, trackInventoryBySizeAndColor, page);
			}
		}

		public static bool DetermineOutOfStock(int productId, int variantId, bool trackInventoryBySizeAndColor)
		{
			return DetermineOutOfStock(productId, variantId, trackInventoryBySizeAndColor, "Product");
		}

		public static bool DetermineOutOfStock(int productId, int variantId, bool trackInventoryBySizeAndColor, string page)
		{
			//If OutOfStockThreshold is greater than inventory it will be consider out of stock
			return (AppConfigNativeInt("OutOfStockThreshold") > DeterminePurchasableQuantity(productId, variantId, trackInventoryBySizeAndColor, page));
		}

		public static int DeterminePurchasableQuantity(int productId, int variantId, bool trackInventoryBySizeAndColor, string page)
		{
			int inventoryLevel = 0;

			if(trackInventoryBySizeAndColor)//this will query the inventory base on total quantity of the attribute for variant
			{
				if(page.Equals("Entity", StringComparison.InvariantCultureIgnoreCase) || variantId == OUT_OF_STOCK_ALL_VARIANTS)
				{
					using(var conn = new SqlConnection(DB.GetDBConn()))
					{
						var st = new StringBuilder();
						st.Append("SELECT SUM(Inventory.Quan) AS TotalQuantity ");
						st.Append("FROM Inventory with (NOLOCK) INNER JOIN ProductVariant ON Inventory.VariantID = ProductVariant.VariantID INNER JOIN ");
						st.Append("Product ON ProductVariant.ProductID = Product.ProductID WHERE Product.ProductID =" + productId.ToString());
						conn.Open();
						using(var bySizesAndColorReader = DB.GetRS(st.ToString(), conn))
						{
							if(bySizesAndColorReader.Read())
							{
								inventoryLevel = DB.RSFieldInt(bySizesAndColorReader, "TotalQuantity");
							}
						}
					}
				}
				else
				{
					using(var conn = new SqlConnection(DB.GetDBConn()))
					{
						conn.Open();
						using(var bySizesAndColorReader = DB.GetRS("SELECT SUM(Quan) as TotalQuantity from Inventory  WITH (NOLOCK)  WHERE VariantID=" + variantId.ToString(), conn))
						{
							if(bySizesAndColorReader.Read())
							{
								inventoryLevel = DB.RSFieldInt(bySizesAndColorReader, "TotalQuantity");
							}
						}
					}
				}
			}
			else if(page.Equals("Entity", StringComparison.InvariantCultureIgnoreCase) || variantId == OUT_OF_STOCK_ALL_VARIANTS)//this will query the total inventory of variants base on product id
			{
				using(var conn = new SqlConnection(DB.GetDBConn()))
				{
					conn.Open();
					using(var allVariantOfProductReader = DB.GetRS("SELECT SUM(Inventory) as Total from ProductVariant  WITH (NOLOCK)  WHERE ProductId =" + productId.ToString(), conn))
					{
						if(allVariantOfProductReader.Read())
						{
							inventoryLevel = DB.RSFieldInt(allVariantOfProductReader, "Total");
						}
					}
				}

			}
			else //this will query the inventory per variant
			{
				using(var conn = new SqlConnection(DB.GetDBConn()))
				{
					conn.Open();
					using(var inventoryReader = DB.GetRS("SELECT Inventory FROM ProductVariant  WITH (NOLOCK)  WHERE VariantID=" + variantId.ToString(), conn))
					{
						if(inventoryReader.Read())
						{
							inventoryLevel = DB.RSFieldInt(inventoryReader, "Inventory");
						}
					}
				}
			}

			return inventoryLevel;
		}

		/// <summary>
		/// Gets the inventory info.
		/// </summary>
		/// <param name="ProductID">The Product id</param>
		/// <param name="VariantID">The variant id</param>
		/// <param name="ShowActualValues">If true will show the actual value eg. 100 , if false 'yes' or 'no' depending on the stringresource</param>
		/// <param name="SkinID">The skin id</param>
		/// <param name="IncludeFrame">If include border on inventory table</param>
		/// <param name="ForEdit">This is use by admin page to edit the inventory</param>
		/// <returns>HTML of inventory info</returns>
		static public string GetInventoryTable(int ProductID, int VariantID, bool ShowActualValues, int SkinID, bool IncludeFrame, bool ForEdit)
		{
			if(ForEdit)
			{
				ShowActualValues = true;
			}

			var tmpS = new StringBuilder(10000);
			tmpS.Append("<div class=\"inventory-table-wrap\">");
			var ProductTracksInventoryBySizeAndColor = AppLogic.ProductTracksInventoryBySizeAndColor(ProductID);
			if(ProductTracksInventoryBySizeAndColor)
			{
				var ProductTracksInventoryBySize = AppLogic.ProductTracksInventoryBySize(ProductID);
				var ProductTracksInventoryByColor = AppLogic.ProductTracksInventoryByColor(ProductID);

				using(var con = new SqlConnection(DB.GetDBConn()))
				{
					con.Open();
					using(var rs = DB.GetRS("select * from productvariant   with (NOLOCK)  where VariantID=" + VariantID.ToString(), con))
					{
						rs.Read();
						var ThisCustomer = HttpContext.Current.GetCustomer();
						var SizesDisplay = DB.RSFieldByLocale(rs, "Sizes", ThisCustomer.LocaleSetting).Trim();
						var ColorsDisplay = DB.RSFieldByLocale(rs, "Colors", ThisCustomer.LocaleSetting).Trim();

						var Sizes = DB.RSFieldByLocale(rs, "Sizes", Localization.GetDefaultLocale());
						var Colors = DB.RSFieldByLocale(rs, "Colors", Localization.GetDefaultLocale());

						if(SizesDisplay.Length == 0)
						{
							SizesDisplay = Sizes;
						}

						if(ColorsDisplay.Length == 0)
						{
							ColorsDisplay = Colors;
						}

						if(!ProductTracksInventoryBySize)
						{
							Sizes = String.Empty;
						}
						if(!ProductTracksInventoryByColor)
						{
							Colors = String.Empty;
						}

						String[] ColorsSplit = Colors.Split(',');
						String[] SizesSplit = Sizes.Split(',');

						String[] ColorsDisplaySplit = ColorsDisplay.Split(',');
						String[] SizesDisplaySplit = SizesDisplay.Split(',');

						tmpS.Append("<table class=\"table table-striped inventory-table\">\n");
						tmpS.Append("<tr class=\"table-header\">\n");
						tmpS.Append("<th>");
						if(!ForEdit)
						{
							tmpS.Append(GetString("common.cs.83", SkinID, Thread.CurrentThread.CurrentUICulture.Name));
						}
						tmpS.Append("</th>\n");
						for(int i = SizesSplit.GetLowerBound(0); i <= SizesSplit.GetUpperBound(0); i++)
						{
							tmpS.Append("<th>" + CleanSizeColorOption(SizesDisplaySplit[i]) + "</th>\n");
						}
						tmpS.Append("</tr>\n");
						int FormFieldID = 1000; // arbitrary number
						for(int i = ColorsSplit.GetLowerBound(0); i <= ColorsSplit.GetUpperBound(0); i++)
						{
							tmpS.Append("<tr class=\"table-row\">\n");
							tmpS.Append("<td>" + CleanSizeColorOption(ColorsDisplaySplit[i]) + "</td>\n");
							for(int j = SizesSplit.GetLowerBound(0); j <= SizesSplit.GetUpperBound(0); j++)
							{
								tmpS.Append("<td>");
								int iVal = GetInventory(ProductID, VariantID, CleanSizeColorOption(SizesSplit[j]), CleanSizeColorOption(ColorsSplit[i]), ProductTracksInventoryBySizeAndColor, ProductTracksInventoryByColor, ProductTracksInventoryBySize);
								if(ForEdit)
								{
									String fldName = "sizecolor|" + ProductID.ToString() + "|" + VariantID.ToString() + "|" + CleanSizeColorOption(SizesSplit[j]) + "|" + CleanSizeColorOption(ColorsSplit[i]);
									if(IsAdminSite)
									{
										tmpS.Append("<input type=\"text\" class=\"form-control\" id=\"" + fldName + "\" name=\"" + fldName + "\" value=\"" + iVal.ToString() + "\">");
									}
									else
									{
										tmpS.Append("<input type=\"text\" id=\"" + fldName + "\" name=\"" + fldName + "\" class=\"form-control\" value=\"" + iVal.ToString() + "\">");
									}
								}
								else
								{
									if(ShowActualValues)
									{
										tmpS.Append(iVal);
									}
									else
									{
										tmpS.Append(CommonLogic.IIF(iVal > 0, GetString("common.cs.28", SkinID, Thread.CurrentThread.CurrentUICulture.Name), GetString("common.cs.29", SkinID, Thread.CurrentThread.CurrentUICulture.Name)));
									}
								}
								FormFieldID++;
								tmpS.Append("</td>\n");
							}
							tmpS.Append("</tr>\n");
						}

						tmpS.Append("</table>\n");
					}
				}
			}
			else
			{
				using(var con = new SqlConnection(DB.GetDBConn()))
				{
					con.Open();
					using(var rs = DB.GetRS("Select Inventory from ProductVariant  with (NOLOCK)  where VariantID=" + VariantID.ToString(), con))
					{
						if(rs.Read())
						{
							int iVal = DB.RSFieldInt(rs, "Inventory");
							if(ForEdit)
							{
								var fldName = "simple|" + ProductID.ToString() + "|" + VariantID.ToString() + "||"; // size and color are blank here to make all fields have 5 parts
								if(IsAdminSite)
								{
									tmpS.Append("<input type=\"text\" class=\"form-control\" id=\"" + fldName + "\" name=\"" + fldName + "\" value=\"" + iVal.ToString() + "\">");
								}
								else
								{
									tmpS.Append("<input type=\"text\" id=\"" + fldName + "\" name=\"" + fldName + "\" class=\"form-control\" value=\"" + iVal.ToString() + "\">");
								}
							}
							else
							{
								bool displayOutOfStockProductOnProductPage = AppConfigBool("DisplayOutOfStockProducts") && AppConfigBool("DisplayOutOfStockOnProductPages");

								//This will use the stringresource for out stock and instock that is set in inventory.aspx
								if(displayOutOfStockProductOnProductPage)
								{
									//define the stock message and css that will be use
									if(ProbablyOutOfStock(ProductID, VariantID, false))
									{
										tmpS.Append("<span class='stock-hint out-stock-hint'>\n");
										tmpS.Append(GetString("OutofStock.DisplayOutOfStockOnProductPage", SkinID, Thread.CurrentThread.CurrentUICulture.Name));
									}
									else
									{
										string messageInStockOnProductPage = GetString("OutofStock.DisplayInStockOnProductPage", SkinID, Thread.CurrentThread.CurrentUICulture.Name);

										//We will use span to set the css
										tmpS.Append("<span class='stock-hint in-stock-hint'>\n");

										if(messageInStockOnProductPage != string.Empty)
										{
											if(ShowActualValues)
											{
												tmpS.Append(messageInStockOnProductPage);
												tmpS.Append(":" + iVal.ToString());
											}
											else
											{
												tmpS.Append(messageInStockOnProductPage);
											}
										}
									}

									tmpS.Append("</span>");
								}
								else
								{
									bool inStock = iVal > 0;
									string stockClass = inStock ? "in-stock-hint" : "out-stock-hint";
									tmpS.Append(String.Format("<span class='stock-hint {0}'>{1}", stockClass, GetString("showproduct.aspx.25", SkinID, Thread.CurrentThread.CurrentUICulture.Name)));
									tmpS.Append("<span class='stock-hint-value'>");
									if(ShowActualValues)// Actual value eg. 100
										tmpS.Append(iVal.ToString());
									else // value eg. Yes or No 
										tmpS.Append(CommonLogic.IIF(iVal > 0, GetString("common.cs.28", SkinID, Thread.CurrentThread.CurrentUICulture.Name), GetString("common.cs.29", SkinID, Thread.CurrentThread.CurrentUICulture.Name)));
									tmpS.Append("</span></span>");
								}
							}
						}
					}
				}
			}
			tmpS.Append("</div>");
			return tmpS.ToString();
		}

		public static int GetProductManufacturerID(int ProductID)
		{
			var tmp = 0;
			if(ProductID != 0)
			{
				using(var con = new SqlConnection(DB.GetDBConn()))
				{
					con.Open();
					using(var rs = DB.GetRS("select ManufacturerID from ProductManufacturer  with (NOLOCK)  where ProductID=" + ProductID.ToString(), con))
					{
						if(rs.Read())
						{
							tmp = DB.RSFieldInt(rs, "ManufacturerID");
						}
					}
				}
			}
			return tmp;
		}

		public static int GetProductDistributorID(int ProductID)
		{
			var tmp = 0;
			if(ProductID != 0)
			{
				using(var con = new SqlConnection(DB.GetDBConn()))
				{
					con.Open();
					using(var rs = DB.GetRS("select DistributorID from ProductDistributor  with (NOLOCK)  where ProductID=" + ProductID.ToString(), con))
					{
						if(rs.Read())
						{
							tmp = DB.RSFieldInt(rs, "DistributorID");
						}
					}
				}
			}
			return tmp;
		}

		public static int GetProductsDefaultVariantID(int ProductID)
		{
			var tmp = 0;
			if(ProductID != 0)
			{
				using(var con = new SqlConnection(DB.GetDBConn()))
				{
					con.Open();
					using(var rs = DB.GetRS("select VariantID from ProductVariant  with (NOLOCK)  where Deleted=0 and Published=1 and IsDefault=1 and ProductID=" + ProductID.ToString(), con))
					{
						if(rs.Read())
						{
							tmp = DB.RSFieldInt(rs, "VariantID");
						}
					}
				}
			}
			return tmp;
		}

		// mod start
		// -- new kit format -- //
		public static void ProcessKitComposition(Customer ThisCustomer, KitComposition composition)
		{
			foreach(KitCartItem item in composition.Compositions)
			{
				string textOptionOrImagePath = item.TextOption;
				if(item.ImageFile != null)
				{
					string extension = string.Empty;
					switch(item.ImageFile.ContentType)
					{
						case "image/gif":
							extension = "gif";
							break;
						case "image/png":
						case "image/x-png":
							extension = "png";
							break;
						case "image/jpg":
						case "image/jpeg":
						case "image/pjpeg":
							extension = "jpg";
							break;
					}

					if(extension != string.Empty)
					{
						string saveFileName = string.Format("{0}_{1}_{2}_{3}.{4}", ThisCustomer.CustomerID, item.ProductID, item.KitItemID, DateTime.Now.ToString("yyyyMMddHHmmss"), extension);
						string saveImagePath = GetImagePath("Orders", string.Empty, false) + saveFileName;
						string saveImageFullPath = GetImagePath("Orders", string.Empty, true) + saveFileName;

						textOptionOrImagePath = saveImagePath;

						try
						{
							item.ImageFile.SaveAs(saveImageFullPath);
						}
						catch { }
					}
					else
					{
						textOptionOrImagePath = string.Empty;
					}
				}

				CreateKitItem(ThisCustomer, composition.CartID, item.ProductID, item.VariantID, item.KitGroupID, item.KitItemID, textOptionOrImagePath, item.Quantity);
			}
		}

		public static void ClearKitItems(Customer ThisCustomer, int ProductID, int VariantID, int CartRecID)
		{
			string clearKitItemsCommand =
				string.Format("DELETE FROM KitCart WHERE CustomerID = {0} AND ProductID = {1} AND VariantID = {2} AND ShoppingCartRecID = {3}", ThisCustomer.CustomerID, ProductID, VariantID, CartRecID);

			DB.ExecuteSQL(clearKitItemsCommand);
		}

		public static void CreateKitItem(Customer ThisCustomer,
			int CartRecID,
			int ProductID,
			int VariantID,
			int KitGroupID,
			int KitItemID,
			string textOption,
			int quantity)
		{
			var KitGroupTypeID = 0;
			var InventoryVariantID = 0;
			var InventoryVariantColor = String.Empty;
			var InventoryVariantSize = String.Empty;

			var sql = string.Format("select kg.KitGroupTypeID, ki.InventoryVariantID, isNull(ki.InventoryVariantColor,'') InventoryVariantColor, isNull(ki.InventoryVariantSize,'') InventoryVariantSize from KitItem ki with (NOLOCK) inner join KitGroup kg with (NOLOCK) on kg.KitGroupID=ki.KitGroupID where ki.KitItemID={0}", KitItemID);

			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(var reader = DB.GetRS(sql, con))
				{
					while(reader.Read())
					{
						KitGroupTypeID = DB.RSFieldInt(reader, "KitGroupTypeID");
						InventoryVariantID = DB.RSFieldInt(reader, "InventoryVariantID");
						InventoryVariantColor = DB.RSField(reader, "InventoryVariantColor");
						InventoryVariantSize = DB.RSField(reader, "InventoryVariantSize");
					}
				}
			}

			string createKitItemCommand =
			string.Format(
				"INSERT INTO KitCart(CustomerID,ProductID,VariantID,ShoppingCartRecID,KitGroupID,KitGroupTypeID,KitItemID,CartType,InventoryVariantID, InventoryVariantColor, InventoryVariantSize, TextOption, Quantity) VALUES({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12})",
				ThisCustomer.CustomerID,
				ProductID,
				VariantID,
				CartRecID,
				KitGroupID,
				KitGroupTypeID,
				KitItemID,
				((int)CartTypeEnum.ShoppingCart),
				InventoryVariantID,
				DB.SQuote(InventoryVariantColor),
				DB.SQuote(InventoryVariantSize),
				DB.SQuote(textOption),
				quantity);

			DB.ExecuteSQL(createKitItemCommand);
		}
		// -- new kit format -- //
		// mod end

		public static decimal KitPriceDelta(int CustomerID, int ProductID, int ShoppingCartRecID)
		{
			return KitPriceDelta(CustomerID, ProductID, ShoppingCartRecID, Localization.StoreCurrency());
		}

		public static decimal KitPriceDelta(int CustomerID, int ProductID, int ShoppingCartRecID, string ForCurrency)
		{
			return Prices.KitPriceDelta(CustomerID, ProductID, ShoppingCartRecID, ForCurrency);
		}

		public static decimal KitWeightDelta(int CustomerID, int ProductID, int ShoppingCartRecID)
		{
			var tmp = Decimal.Zero;
			if(CustomerID != 0)
			{
				using(var con = new SqlConnection(DB.GetDBConn()))
				{
					con.Open();
					using(var rs = DB.GetRS("select sum(quantity*weightdelta) as WT from kitcart   with (NOLOCK)  inner join kititem   with (NOLOCK)  on kitcart.kititemid=kitItem.kititemid where customerid=" + CustomerID.ToString() + " and productid=" + ProductID.ToString() + " and ShoppingCartrecid=" + ShoppingCartRecID.ToString(), con))
					{
						if(rs.Read())
						{
							tmp = DB.RSFieldDecimal(rs, "WT");
						}
					}
				}
			}
			return tmp;
		}

		public static decimal GetColorAndSizePriceDelta(String ChosenColor, String ChosenSize, int TaxClassID, Customer ThisCustomer, bool WithDiscount, bool WithVAT)
		{
			return Prices.GetColorAndSizePriceDelta(ChosenColor, ChosenSize, TaxClassID, ThisCustomer, WithDiscount, WithVAT);
		}

		public static decimal GetKitTotalPrice(int CustomerID, int CustomerLevelID, int ProductID, int VariantID, int ShoppingCartRecID)
		{
			return Prices.GetKitTotalPrice(CustomerID, CustomerLevelID, ProductID, VariantID, ShoppingCartRecID);
		}

		public static decimal GetKitTotalWeight(int CustomerID, int CustomerLevelID, int ProductID, int VariantID, int ShoppingCartRecID)
		{
			var tmp = Decimal.Zero;

			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(var rs = DB.GetRS("SELECT Product.*, ProductVariant.Price, ProductVariant.SalePrice, ProductVariant.Weight FROM Product   with (NOLOCK)  inner join productvariant   with (NOLOCK)  on product.productid=productvariant.productid where ProductVariant.VariantID=" + VariantID.ToString(), con))
				{
					if(rs.Read())
					{
						var KitWeight = DB.RSFieldDecimal(rs, "Weight");
						var KitWeightDelta = AppLogic.KitWeightDelta(CustomerID, ProductID, ShoppingCartRecID);
						tmp = KitWeight + KitWeightDelta;
					}
				}
			}

			return tmp;
		}

		public static bool IsAKit(int ProductID)
		{
			var tmpS = false;

			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(var rs = DB.GetRS("select IsAKit from Product   with (NOLOCK)  where ProductID=" + ProductID.ToString(), con))
				{
					if(rs.Read())
					{
						tmpS = DB.RSFieldBool(rs, "IsAKit");
					}
				}
			}

			return tmpS;
		}

		public static int GetStoreSkinID(int StoreID)
		{
            // nal
            //var skinProvider = new SkinProvider();
            //var skinId = 0;
            //using(var con = new SqlConnection(DB.GetDBConn()))
            //{
            //	con.Open();
            //	using(var rs = DB.GetRS("SELECT SkinID FROM Store with (NOLOCK) WHERE StoreID='" + StoreID.ToString() + "';", con))
            //	{
            //		if(rs.Read())
            //		{
            //			skinId = DB.RSFieldInt(rs, "SkinID");
            //		}
            //	}
            //}

            //if(!skinProvider.SkinIdIsValid(skinId))
            //	skinId = skinProvider.GetSkinIdByName(SkinProvider.DefaultSkinName);

            //return skinId;
            return 1;
		}

		/// <summary>
		/// Use noVirtualNoSlash for UrlHelper MVC
		/// </summary>
		public static string GetStoreHTTPLocation(bool useSsl, bool includeScriptLocation = true, bool noVirtualNoSlash = false)
		{
			var applyHttps = useSsl && UseSSL() && OnLiveServer();

			var scheme = applyHttps
				? Uri.UriSchemeHttps
				: Uri.UriSchemeHttp;

			var host = HttpContext.Current.Request.Url.Host;

			var port = HttpContext.Current.Request.Url.IsDefaultPort
				? -1
				: HttpContext.Current.Request.Url.Port;

			//for MVC urlHelper
			if(noVirtualNoSlash)
				return new UriBuilder(scheme, host, port)
					.Uri
					.ToString()
					.TrimEnd('/');

			var siteRoot = VirtualPathUtility.ToAbsolute("~/");
			return new UriBuilder(scheme, host, port, siteRoot)
				.Uri
				.ToString();
		}

		public static bool ProductTracksInventoryBySizeAndColor(int ProductID)
		{
			var tmp = false;

			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(var rs = DB.GetRS("select TrackInventoryBySizeAndColor from Product   with (NOLOCK)  where ProductID=" + ProductID.ToString(), con))
				{
					if(rs.Read())
					{
						tmp = DB.RSFieldBool(rs, "TrackInventoryBySizeAndColor");
					}
				}
			}

			return tmp;
		}

		public static bool ProductTracksInventoryBySize(int ProductID)
		{
			var tmp = false;

			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(var rs = DB.GetRS("select TrackInventoryBySize from Product   with (NOLOCK)  where ProductID=" + ProductID.ToString(), con))
				{
					if(rs.Read())
					{
						tmp = DB.RSFieldBool(rs, "TrackInventoryBySize");
					}
				}
			}

			return tmp;
		}

		public static bool ProductTracksInventoryByColor(int ProductID)
		{
			var tmp = false;

			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(var rs = DB.GetRS("select TrackInventoryByColor from Product   with (NOLOCK)  where ProductID=" + ProductID.ToString(), con))
				{
					if(rs.Read())
					{
						tmp = DB.RSFieldBool(rs, "TrackInventoryByColor");
					}
				}
			}

			return tmp;
		}

		public static int GetInventory(int productId, int variantId, string chosenSize, string chosenColor)
		{
			var trackInventoryBySizeAndColor = false;
			var trackInventoryBySize = false;
			var trackInventoryByColor = false;
			SqlParameter[] spa = { DB.CreateSQLParameter("@ProductID", SqlDbType.Int, 4, productId, ParameterDirection.Input) };

			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(var dr = DB.GetRS("select TrackInventoryBySizeAndColor, TrackInventoryBySize, TrackInventoryByColor from dbo.Product where ProductID = @ProductID", spa, con))
				{
					var b = dr.Read();
					if(b)
					{
						trackInventoryBySizeAndColor = DB.RSFieldBool(dr, "TrackInventoryBySizeAndColor");
						trackInventoryBySize = DB.RSFieldBool(dr, "TrackInventoryBySize");
						trackInventoryByColor = DB.RSFieldBool(dr, "TrackInventoryByColor");
					}
				}
			}

			return GetInventory(productId, variantId, chosenSize, chosenColor, trackInventoryBySizeAndColor, trackInventoryByColor, trackInventoryBySize);
		}

		public static int GetInventory(int productID, int variantID, string chosenSize, string chosenColor, bool trackInventoryBySizeAndColor, bool trackInventoryByColor, bool tracksInventoryBySize)
		{
			string warehouseLocation = string.Empty;
			string fullSku = string.Empty;

			return GetInventory(productID, variantID, chosenSize, chosenColor, trackInventoryBySizeAndColor, trackInventoryByColor, tracksInventoryBySize, out warehouseLocation, out fullSku);
		}

		public static int GetInventory(int productID, int variantID, string chosenSize, string chosenColor, bool trackInventoryBySizeAndColor, bool trackInventoryByColor, bool tracksInventoryBySize, out string warehouseLocation, out string fullSku)
		{
			string vendorId = string.Empty;
			decimal weightDelta = decimal.Zero;
			string gtin = string.Empty;

			return GetInventory(productID, variantID, chosenSize, chosenColor, trackInventoryBySizeAndColor, trackInventoryByColor, tracksInventoryBySize, out warehouseLocation, out fullSku, out vendorId, out weightDelta, out gtin);
		}

		public static int GetInventory(int productID, int variantID, string chosenSize, string chosenColor, bool trackInventoryBySizeAndColor, bool trackInventoryByColor, bool tracksInventoryBySize, out string warehouseLocation, out string fullSku, out string vendorId, out decimal weightDelta, out string gtin)
		{
			var inventory = 0;
			warehouseLocation = string.Empty;
			fullSku = string.Empty;
			vendorId = string.Empty;
			weightDelta = decimal.Zero;
			gtin = string.Empty;

			if(!trackInventoryBySizeAndColor)
			{
				using(var con = new SqlConnection(DB.GetDBConn()))
				{
					con.Open();
					using(var rs = DB.GetRS("Select Inventory from ProductVariant with (NOLOCK)  where VariantId = @VariantId", new SqlParameter[] { new SqlParameter("@VariantId", variantID) }, con))
					{
						if(rs.Read())
						{
							inventory = DB.RSFieldInt(rs, "Inventory");
						}
					}
				}
			}
			else
			{
				var size = tracksInventoryBySize ? CleanSizeColorOption(chosenSize).ToLower() : string.Empty;
				var color = trackInventoryByColor ? CleanSizeColorOption(chosenColor).ToLower() : string.Empty;

				using(var con = new SqlConnection(DB.GetDBConn()))
				{
					con.Open();
					using(var rs = DB.GetRS(@"select Quan, WarehouseLocation, VendorFullSKU, VendorID, WeightDelta, GTIN 
														from Inventory with (NOLOCK) 
														where VariantID = @VariantId 
														and lower([size]) = @Size 
														and lower(color) = @Color", new SqlParameter[] { new SqlParameter("@VariantId", variantID), new SqlParameter("@Size", size), new SqlParameter("@Color", color) }, con))
					{
						if(rs.Read())
						{
							inventory = DB.RSFieldInt(rs, "Quan");
							warehouseLocation = DB.RSField(rs, "WarehouseLocation");
							fullSku = DB.RSField(rs, "VendorFullSKU");
							vendorId = DB.RSField(rs, "VendorID");
							weightDelta = DB.RSFieldDecimal(rs, "WeightDelta");
							gtin = DB.RSField(rs, "GTIN");
						}
					}
				}
			}
			return inventory;
		}

		public static bool OrderHasDownloadComponents(int OrderNumber, bool RequireDownloadLocationAlso)
		{
			return DB.GetSqlN("select count(*) as N from orders_ShoppingCart where IsDownload=1 " + CommonLogic.IIF(RequireDownloadLocationAlso, " and DownloadLocation is not null and datalength(DownloadLocation) > 0", "") + " and OrderNumber=" + OrderNumber.ToString()) != 0;
		}

		public static bool OrderHasShippableComponents(int OrderNumber)
		{
			return DB.GetSqlN("select count(*) as N from orders_ShoppingCart   with (NOLOCK)  where IsDownload=0 and FreeShipping!=2 and OrderNumber=" + OrderNumber.ToString()) != 0;
		}

		public static Decimal GetUpsellProductPrice(int SourceProductID, int UpsellProductID, int CustomerLevelID)
		{
			return Prices.GetUpsellProductPrice(SourceProductID, UpsellProductID, CustomerLevelID);
		}

		public static String GetUpsellProductsBoxExpanded(int forProductID, int showNum, bool showPics, String teaser, bool gridFormat, int SkinID, Customer ThisCustomer)
		{
			String s = RunXmlPackage("upsellproducts.xml.config", new Parser(), ThisCustomer, SkinID, "", "Productid=" + forProductID.ToString(), false, false);
			return s;
		}

		public static String GetNewsBoxExpanded(bool LinkHeadline, bool ShowCopy, int showNum, bool IncludeFrame, bool useCache, String teaser, int SkinID, String LocaleSetting)
		{
			var CacheName = "GetNewsBoxExpanded_" + showNum.ToString() + "_" + teaser + "_" + SkinID.ToString() + "_" + LocaleSetting;
			if(CachingOn && useCache)
			{
				var cachedData = (string)HttpContext.Current.Cache.Get(CacheName);
				if(cachedData != null)
				{
					if(CommonLogic.ApplicationBool("DumpSQL"))
					{
						HttpContext.Current.Response.Write("Cache Hit Found!\n");
					}
					return cachedData;
				}
			}

			var tmpS = new StringBuilder(10000);

			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				string query = string.Format("select COUNT(*) AS N from News a with (NOLOCK) inner join (Select distinct a.NewsID FROM News a with (nolock) left join NewsStore b with (NOLOCK) on a.NewsID = b.NewsID WHERE ({0} = 0 or StoreID = {1})) b on a.NewsID = b.NewsID where ExpiresOn>getdate() and Deleted=0 and Published=1 ; " +
											 "select a.* from News a with (NOLOCK) inner join (SELECT a.NewsID FROM News a with (nolock) left join NewsStore b with (NOLOCK) on a.NewsID = b.NewsID WHERE ({0} = 0 or StoreID = {1})) b on a.NewsID = b.NewsID where ExpiresOn>getdate() and Deleted=0 and Published=1 order by a.CreatedON desc", CommonLogic.IIF(GlobalConfigBool("AllowNewsFiltering") == true, 1, 0), StoreID());
				using(var rs = DB.GetRS(query, con))
				{
					if(rs.Read() && DB.RSFieldInt(rs, "N") > 0)
					{
						if(rs.NextResult())
						{
							var newsIndexLink = Url.Action(
								actionName: ActionNames.Index,
								controllerName: ControllerNames.News);

							if(IncludeFrame)
							{
								tmpS.Append("<table width=\"100%\" cellpadding=\"2\" cellspacing=\"0\">\n");
								tmpS.Append("<tr><td align=\"left\" valign=\"top\">\n");
								tmpS.Append("<a href=\"" + newsIndexLink + "\"><img src=\"" + LocateImageURL("Skins/skin_" + SkinProvider.GetSkinNameById(SkinID) + "/images/newsexpanded.gif") + "\" border=\"0\" /></a>");
								tmpS.Append("<table width=\"100%\" cellpadding=\"4\" cellspacing=\"0\" border=\"0\" class=\"box-frame\">\n");
								tmpS.Append("<tr><td align=\"left\" valign=\"top\">\n");
							}

							tmpS.Append("<p><b>" + teaser + "</b></p>\n");


							tmpS.Append("<table width=\"100%\" cellpadding=\"2\" cellspacing=\"0\" border=\"0\">\n");
							int i = 1;

							while(rs.Read())
							{
								if(i > showNum)
								{
									tmpS.Append("<tr><td colspan=\"2\"><hr class=\"news-token-hr\" /><a href=\"" + newsIndexLink + "\">more...</a></td></tr>");
									break;
								}
								if(i > 1)
								{
									tmpS.Append("<tr><td colspan=\"2\"><hr class=\"news-token-hr\" /></td></tr>");
								}
								tmpS.Append("<tr>");
								tmpS.Append("<td width=\"15%\" align=\"left\" valign=\"top\">\n");
								tmpS.Append("<b>" + Localization.ToThreadCultureShortDateString(DB.RSFieldDateTime(rs, "CreatedOn")) + "</b>");
								tmpS.Append("</td>");
								tmpS.Append("<td align=\"left\" valign=\"top\">\n");
								String Hdl = DB.RSFieldByLocale(rs, "Headline", LocaleSetting);
								if(Hdl.Length == 0)
								{
									Hdl = CommonLogic.Ellipses(DB.RSFieldByLocale(rs, "NewsCopy", LocaleSetting), 50, true);
								}
								tmpS.Append("<div align=\"left\">");
								if(LinkHeadline)
								{
									var newsArticleLink = Url.Action(
										actionName: ActionNames.Detail,
										controllerName: ControllerNames.News,
										routeValues: new RouteValueDictionary
										{
											{  RouteDataKeys.Id, DB.RSFieldInt(rs, "NewsID") }
										});

									tmpS.Append("<a href=\"" + newsArticleLink + "\">");
								}
								tmpS.Append("<b>");
								tmpS.Append(Hdl);
								tmpS.Append("</b>");
								if(LinkHeadline)
								{
									tmpS.Append("</a>");
								}
								tmpS.Append("</div>");
								if(ShowCopy)
								{
									tmpS.Append("<div align=\"left\">" + HttpContext.Current.Server.HtmlDecode(DB.RSFieldByLocale(rs, "NewsCopy", LocaleSetting)) + "</div>");
								}
								tmpS.Append("</td>");
								tmpS.Append("</tr>");
								i++;
							}

							tmpS.Append("</table>\n");

							if(IncludeFrame)
							{
								tmpS.Append("</td></tr>\n");
								tmpS.Append("</table>\n");
								tmpS.Append("</td></tr>\n");
								tmpS.Append("</table>\n");
							}
						}
					}
				}
			}

			if(CachingOn && useCache)
			{
				HttpContext.Current.Cache.Insert(CacheName, tmpS.ToString(), null, System.DateTime.Now.AddMinutes(CacheDurationMinutes()), TimeSpan.Zero);
			}
			return tmpS.ToString();
		}

		public static decimal GetCustomerLevelDiscountPercent(int customerLevelId)
		{
			using(var connection = new SqlConnection(DB.GetDBConn()))
			{
				connection.Open();
				return GetCustomerLevelDiscountPercent(connection, customerLevelId);
			}
		}

		public static decimal GetCustomerLevelDiscountPercent(SqlConnection connection, int customerLevelId)
		{
			if(customerLevelId == 0)
				return 0;

			using(var command = connection.CreateCommand())
			{
				command.CommandText = "select LevelDiscountPercent from CustomerLevel with(nolock) where CustomerLevelID = @customerLevelId";
				command.Parameters.AddWithValue("customerLevelId", customerLevelId);

				using(var reader = command.ExecuteReader())
					if(reader.Read())
						return reader.FieldDecimal("LevelDiscountPercent");

			}

			return 0;
		}

		public static bool CustomerLevelHasNoTax(int CustomerLevelID)
		{
			if(CustomerLevelID == 0)
			{
				// consumers always have tax:
				return false;
			}
			var tmpS = false;

			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(var rs = DB.GetRS("Select LevelHasNoTax from CustomerLevel   with (NOLOCK)  where CustomerLevelID=" + CustomerLevelID.ToString(), con))
				{
					if(rs.Read())
					{
						tmpS = DB.RSFieldBool(rs, "LevelHasNoTax");
					}
				}
			}

			return tmpS;

		}

		public static bool CustomerLevelHasFreeShipping(int CustomerLevelID)
		{
			if(CustomerLevelID == 0)
			{
				// consumers always have shipping, unless overridden by other parameters:
				return false;
			}
			var tmpS = false;

			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(var rs = DB.GetRS("Select LevelHasFreeShipping from CustomerLevel   with (NOLOCK)  where CustomerLevelID=" + CustomerLevelID.ToString(), con))
				{
					if(rs.Read())
					{
						tmpS = DB.RSFieldBool(rs, "LevelHasFreeShipping");
					}
				}
			}

			return tmpS;

		}

		public static bool CustomerLevelAllowsCoupons(int customerLevelId)
		{
			using(var connection = new SqlConnection(DB.GetDBConn()))
			{
				connection.Open();
				return CustomerLevelAllowsCoupons(connection, customerLevelId);
			}
		}

		public static bool CustomerLevelAllowsCoupons(SqlConnection connection, int customerLevelId)
		{
			if(customerLevelId == 0)
				return true;

			using(var command = connection.CreateCommand())
			{
				command.CommandText = "select LevelAllowsCoupons from CustomerLevel with(nolock) where CustomerLevelID = @customerLevelId";
				command.Parameters.AddWithValue("customerLevelId", customerLevelId);

				using(var reader = command.ExecuteReader())
					if(reader.Read())
						return DB.RSFieldBool(reader, "LevelAllowsCoupons");
			}

			return false;
		}

		public static bool CustomerLevelAllowsPO(int customerLevelID)
		{
			if(!AppLogic.AppConfig("PaymentMethods").Split(',')
					.Where(pm => AppLogic.CleanPaymentMethod(pm) == AppLogic.ro_PMPurchaseOrder)
					.Any())
				return false;

			if(customerLevelID == 0) // consumers cannot use PO's, unless overridden by other parameters:
				return AppLogic.AppConfigBool("CustomerLevel0AllowsPOs");

			using(var connection = new SqlConnection(DB.GetDBConn()))
			using(var command = new SqlCommand("SELECT COUNT(*) FROM CustomerLevel(NOLOCK) WHERE CustomerLevelID=@CustomerLevelID AND LevelAllowsPO=1", connection))
			{
				connection.Open();
				command.Parameters.AddWithValue("@CustomerLevelID", customerLevelID);
				var result = (int)command.ExecuteScalar();

				return result == 1;
			}
		}

		public static decimal GetVariantExtendedPrice(int VariantID, int CustomerLevelID)
		{
			var pr = Decimal.Zero;
			if(CustomerLevelID != 0)
			{
				using(var con = new SqlConnection(DB.GetDBConn()))
				{
					con.Open();
					using(var rs = DB.GetRS("select Price from ExtendedPrice  with (NOLOCK)  where VariantID=" + VariantID.ToString() + " and CustomerLevelID=" + CustomerLevelID.ToString() + " and VariantID in (select VariantID from ProductVariant where ProductID in (select ProductID from ProductCustomerLevel where CustomerLevelID=" + CustomerLevelID.ToString() + "))", con))
					{
						if(rs.Read())
						{
							pr = DB.RSFieldDecimal(rs, "Price");
						}
					}
				}
			}
			return pr;
		}

		public static decimal GetVariantPrice(int VariantID)
		{
			return Prices.GetVariantPrice(VariantID);
		}

		public static decimal GetVariantSalePrice(int VariantID)
		{
			return Prices.GetVariantSalePrice(VariantID);
		}

		public static decimal DetermineLevelPrice(int VariantID, int CustomerLevelID, out bool IsOnSale)
		{
			return Prices.DetermineLevelPrice(VariantID, CustomerLevelID, out IsOnSale);
		}

		public static String MakeProperPhoneFormat(String PhoneNumber)
		{
			return PhoneNumber;
		}

		public static bool ProductHasBeenDeleted(int ProductID)
		{
			return (DB.GetSqlN("Select count(ProductID) as N from Product   with (NOLOCK)  where Deleted=0 and Published=1 and ProductID=" + ProductID.ToString()) == 0);
		}

		public static bool VariantHasBeenDeleted(int VariantID)
		{
			return (DB.GetSqlN("Select count(VariantID) as N from ProductVariant   with (NOLOCK)  where Deleted=0 and Published=1 and VariantID=" + VariantID.ToString()) == 0);
		}

		public static String GetAdminDir()
		{
			String AdminDir = AppLogic.AdminDir();
			if(AdminDir.Length == 0)
			{
				AdminDir = "admin";
			}
			if(AdminDir.EndsWith("/"))
			{
				AdminDir = AdminDir.Substring(0, AdminDir.Length - 1);
			}
			return AdminDir;
		}

		public static String GetAdminHTTPLocation(bool TryToUseSSL)
		{
			return GetStoreHTTPLocation(TryToUseSSL);
		}

		public static bool OnLiveServer()
		{
			return (CommonLogic.ServerVariables("HTTP_HOST").IndexOf(LiveServer(), StringComparison.InvariantCultureIgnoreCase) != -1);
		}

		public static string GetProductEntityMappings(int ProductID, string EntityName)
		{
			var tmpS = new StringBuilder(512);
			var separator = string.Empty;
			var sql = string.Format("select * from product{0} with (NOLOCK) where ProductID={1} order by displayorder", EntityName, ProductID.ToString());
			var idxfield = EntityName + "ID";

			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(var rs = DB.GetRS(sql, con))
				{
					while(rs.Read())
					{
						tmpS.Append(separator);
						tmpS.Append(DB.RSFieldInt(rs, idxfield).ToString());
						separator = ",";
					}
				}
			}

			return tmpS.ToString();
		}

		// returns the "next" variant in this product, after the specified variant
		// "next" is defined as either the product that is next higher display order, or same display order and next highest alphabetical order
		// is circular also (i.e. if last, return first)
		public static int GetNextVariant(int ProductID, int VariantID)
		{
			var sql = "SELECT VariantID from ProductVariant where ProductID=" + ProductID.ToString() + " and Deleted=0 order by DisplayOrder,Name";

			var id = NONE_FOUND;

			var variantIds = new LinkedList<int>();
			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(var rs = DB.GetRS(sql, con))
				{
					while(rs.Read())
					{
						variantIds.AddLast(DB.RSFieldInt(rs, "VariantID"));
					}
				}
			}

			var foundNode = variantIds.Find(VariantID);
			if(null != foundNode)
			{
				// if last return first
				if(foundNode.Next == null)
				{
					id = variantIds.First.Value;
				}
				else
				{
					id = foundNode.Next.Value;
				}
			}

			return id;
		}

		public static string GetVariantSKUSuffix(int VariantID)
		{
			var tmpS = string.Empty;

			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(var rs = DB.GetRS("Select * from ProductVariant   with (NOLOCK)  where VariantID=" + VariantID.ToString(), con))
				{
					if(rs.Read())
					{
						tmpS = DB.RSField(rs, "SKUSuffix");
					}
				}
			}

			return tmpS;
		}

		// ONLY a helper function to GetDefaultProductVariant now!
		public static int x_GetFirstProductVariant(int ProductID)
		{
			var tmp = 0;

			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(var rs = DB.GetRS("select top 1 VariantID from ProductVariant   with (NOLOCK)  where deleted=0 and published=1 and productid=" + ProductID.ToString() + " order by DisplayOrder,Name", con))
				{
					if(rs.Read())
					{
						tmp = DB.RSFieldInt(rs, "VariantID");
					}
				}
			}

			return tmp;
		}

		public static int GetDefaultProductVariant(int ProductID)
		{
			return GetDefaultProductVariant(ProductID, true);
		}

		public static int GetDefaultProductVariant(int ProductID, bool PublishedOnly)
		{
			var tmp = 0;

			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(var rs = DB.GetRS("select top 1 VariantID from ProductVariant   with (NOLOCK)  where deleted=0 " + CommonLogic.IIF(PublishedOnly, " and published=1", String.Empty) + " and IsDefault=1 and ProductID=" + ProductID.ToString(), con))
				{
					if(rs.Read())
					{
						tmp = DB.RSFieldInt(rs, "VariantID");
					}
				}
			}

			if(tmp == 0)
			{
				tmp = x_GetFirstProductVariant(ProductID);
			}

			return tmp;
		}

		public static int GetVariantProductID(int VariantID)
		{
			var tmp = 0;

			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(var rs = DB.GetRS("select productid from productvariant   with (NOLOCK)  where variantid=" + VariantID.ToString(), con))
				{
					if(rs.Read())
					{
						tmp = DB.RSFieldInt(rs, "ProductID");
					}
				}
			}

			return tmp;
		}

		/// <summary>
		/// Converts a State/Province/County Name to its Abbreviation. If no match is found in the given country, the input is returned.
		/// </summary>
		public static string GetStateAbbreviation(string stateName, string countryName)
		{
			if(string.IsNullOrEmpty(stateName) || string.IsNullOrEmpty(countryName))
				return string.Empty;

			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(var rs = DB.GetRS(
						"select Abbreviation from dbo.State s with (NOLOCK) " +
						"inner join dbo.Country c on c.CountryID = s.CountryID " +
						"where s.Name = " + DB.SQuote(stateName) + " and c.Name = " + DB.SQuote(countryName), con))
				{
					while(rs.Read())
					{
						return DB.RSField(rs, "Abbreviation");
					}
				}
			}

			return stateName;
		}

		public static int GetStateID(String StateAbbreviation)
		{
			var tmp = 0;

			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(var rs = DB.GetRS("Select * from state with (NOLOCK)  order by DisplayOrder,Name", con))
				{
					while(rs.Read())
					{
						if(StateAbbreviation.Equals(DB.RSField(rs, "Abbreviation"), StringComparison.InvariantCultureIgnoreCase))
						{
							tmp = DB.RSFieldInt(rs, "StateID");
							break;
						}
					}
				}
			}

			return tmp;
		}

		public static string GetCountryNameFromTwoLetterISOCode(string CountryTwoLetterISOCode)
		{
			var tmp = string.Empty;

			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(var rs = DB.GetRS("Select * from Country with (NOLOCK)  order by DisplayOrder,Name", con))
				{
					while(rs.Read())
					{
						if(CountryTwoLetterISOCode.Equals(DB.RSField(rs, "TwoLetterISOCode"), StringComparison.InvariantCultureIgnoreCase))
						{
							tmp = DB.RSField(rs, "Name");
							break;
						}
					}
				}
			}

			return tmp;
		}

		public static Int32 GetCountryIDFromTwoLetterISOCode(string CountryTwoLetterISOCode)
		{
			if(CountryTwoLetterISOCode.Length != 2)
			{
				return 0;
			}

			var tmp = 0;

			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(var rs = DB.GetRS("Select * from Country with (NOLOCK)  order by DisplayOrder,Name", con))
				{
					while(rs.Read())
					{
						if(CountryTwoLetterISOCode.Equals(DB.RSField(rs, "TwoLetterISOCode"), StringComparison.InvariantCultureIgnoreCase))
						{
							tmp = DB.RSFieldInt(rs, "CountryID");
							break;
						}
					}
				}
			}

			return tmp;
		}

		public static Int32 GetCountryIDFromThreeLetterISOCode(string CountryThreeLetterISOCode)
		{
			if(CountryThreeLetterISOCode.Length != 3)
			{
				return 0;
			}

			var tmp = 0;

			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(var rs = DB.GetRS("Select * from Country with (NOLOCK)  order by DisplayOrder,Name", con))
				{
					while(rs.Read())
					{
						if(CountryThreeLetterISOCode.Equals(DB.RSField(rs, "ThreeLetterISOCode"), StringComparison.InvariantCultureIgnoreCase))
						{
							tmp = DB.RSFieldInt(rs, "CountryID");
							break;
						}
					}
				}
			}

			return tmp;
		}

		public static string GetCountryTwoLetterISOCode(string CountryName)
		{
			var tmp = "US";

			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(var rs = DB.GetRS("Select * from Country with (NOLOCK)  order by DisplayOrder,Name", con))
				{
					while(rs.Read())
					{
						if(CountryName.Equals(DB.RSField(rs, "Name"), StringComparison.InvariantCultureIgnoreCase))
						{
							tmp = DB.RSField(rs, "TwoLetterISOCode");
							break;
						}
					}
				}
			}

			return tmp;
		}

		public static int GetCountryID(string countryName)
		{
			return DB.GetSqlN(
				sql: "select top 1 CountryId as N from Country with (nolock) where Name = @countryName order by DisplayOrder,Name",
				parameters: new SqlParameter("countryName", countryName));
		}

		/// <summary>
		/// Check the inputed Zip Code and match the value from Country PostalCodeRegEx Regular Expression
		/// </summary>
		/// <param name="ZipCode">ZipCode entered by user</param>
		/// <param name="CountryID">The country chosen</param>
		/// <returns>returns the validated postal code for each specific country</returns>
		public static Boolean ValidatePostalCode(String ZipCode, int CountryID)
		{
			if(GetCountryPostalCodeRequired(CountryID))
			{
				string ZipRegEx = GetCountryPostalCodeRegEx(CountryID);
				if(!CommonLogic.IsStringNullOrEmpty(ZipRegEx))
				{
					Regex regExpValue = new Regex(ZipRegEx);
					Match regExpMatch = regExpValue.Match(ZipCode);

					return regExpMatch.Success;
				}
			}
			return true;
		}

		public static string GetCountryPostalCodeRegEx(int CountryID)
		{
			var tmp = string.Empty;

			using(var conn = new SqlConnection(DB.GetDBConn()))
			{
				conn.Open();
				using(var rs = DB.GetRS(String.Format("Select * from Country with (NOLOCK) where CountryID={0}", CountryID), conn))
				{
					if(rs.Read())
					{
						tmp = DB.RSField(rs, "PostalCodeRegex");
					}
				}
			}

			return tmp;
		}

		public static bool GetCountryPostalCodeRequired(int CountryID)
		{
			var tmp = false;

			using(var conn = new SqlConnection(DB.GetDBConn()))
			{
				conn.Open();
				using(var rs = DB.GetRS(String.Format("Select * from Country with (NOLOCK) where CountryID={0}", CountryID), conn))
				{
					if(rs.Read())
					{
						tmp = DB.RSFieldBool(rs, "PostalCodeRequired");
					}
				}
			}

			return tmp;
		}

		public static string GetCountryPostalExample(int CountryID)
		{
			var tmp = string.Empty;
			using(var conn = new SqlConnection(DB.GetDBConn()))
			{
				conn.Open();
				using(var rs = DB.GetRS(String.Format("Select * from Country with (NOLOCK) where CountryID={0}", CountryID), conn))
				{
					if(rs.Read())
					{
						tmp = DB.RSField(rs, "PostalCodeExample");
					}
				}
			}

			return tmp;
		}

		/// <summary>
		/// Gets the error message whenever country zip does not match the specified PostalCodeRegex
		/// </summary>
		/// <param name="CountryID">The countryid of whom to get the PostalCodeRegex</param>
		/// <param name="skinID">The currently used skinID</param>
		/// <param name="LocaleSetting">The currently used Locale Setting</param>
		/// <returns>returns string</returns>
		public static String GetCountryPostalErrorMessage(int CountryID, int skinID, String LocaleSetting)
		{
			String tmp = "";

			tmp = GetCountryPostalExample(CountryID);

			return String.Format("{0} {1}", GetString("admin.common.postalcodeerrormessage", skinID, LocaleSetting), tmp);
		}

		public static bool GetCountryIsInternational(string countryName)
		{
			return countryName.ToLower() != "united states";
		}

		public static String GetFirstProductEntity(EntityHelper EntityHelper, int ProductID, bool ForProductBrowser, String LocaleSetting)
		{
			String tmpS = EntityHelper.GetObjectEntities(ProductID, ForProductBrowser);
			if(tmpS.Length == 0)
			{
				return String.Empty;
			}
			String[] ss = tmpS.Split(',');
			String result = String.Empty;
			try
			{
				result = EntityHelper.GetEntityName(Localization.ParseUSInt(ss[0]), LocaleSetting);
			}
			catch { }
			return result;
		}

		public static int GetFirstProductEntityID(EntityHelper EntityHelper, int ProductID, bool ForProductBrowser)
		{
			String tmpS = EntityHelper.GetObjectEntities(ProductID, ForProductBrowser);
			if(tmpS.Length == 0)
			{
				return 0;
			}
			String[] ss = tmpS.Split(',');
			int result = 0;
			try
			{
				result = Localization.ParseUSInt(ss[0]);
			}
			catch { }
			return result;
		}

		public static string GetProductName(int ProductID, string LocaleSetting)
		{
			var tmpS = string.Empty;
			if(ProductID != 0)
			{
				using(var con = new SqlConnection(DB.GetDBConn()))
				{
					con.Open();
					using(var rs = DB.GetRS("select Name from product   with (NOLOCK)  where productid=" + ProductID.ToString(), con))
					{
						if(rs.Read())
						{
							tmpS = DB.RSFieldByLocale(rs, "Name", LocaleSetting);
						}
					}
				}
			}
			return tmpS;
		}

		public static string GetRequiresProducts(int productId)
		{
			if(productId == 0)
				return string.Empty;

			return DB.GetSqlS(
				"select top 1 RequiresProducts S from product with(nolock) where ProductID = @productId",
				new SqlParameter("productId", productId));
		}

		public static string GetVariantName(int VariantID, string LocaleSetting)
		{
			var tmpS = string.Empty;
			if(VariantID != 0)
			{
				using(var con = new SqlConnection(DB.GetDBConn()))
				{
					con.Open();
					using(var rs = DB.GetRS("select Name from productvariant   with (NOLOCK)  where VariantID=" + VariantID.ToString(), con))
					{
						if(rs.Read())
						{
							tmpS = DB.RSFieldByLocale(rs, "Name", LocaleSetting);
						}
					}
				}
			}
			return tmpS;
		}

		public static string GetRestrictedQuantities(int VariantID, out int MinimumQuantity)
		{
			MinimumQuantity = 0;
			var tmpS = string.Empty;

			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(var rs = DB.GetRS("select RestrictedQuantities,MinimumQuantity from productvariant  with (NOLOCK)  where VariantID=" + VariantID.ToString(), con))
				{
					if(rs.Read())
					{
						tmpS = DB.RSField(rs, "RestrictedQuantities");
						MinimumQuantity = DB.RSFieldInt(rs, "MinimumQuantity");
					}
				}
			}

			return tmpS;
		}

		public static string GetProductSKU(int ProductID)
		{
			var tmpS = string.Empty;

			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(var rs = DB.GetRS("select SKU from product   with (NOLOCK)  where productid=" + ProductID.ToString(), con))
				{
					if(rs.Read())
					{
						tmpS = DB.RSField(rs, "SKU");
					}
				}
			}

			return tmpS;
		}

		public static String GetImagePath(String EntityOrObjectName, String Size, bool fullPath)
		{
			string pth = string.Empty;
			string pthPrefix = string.Empty;

			pth = pthPrefix + "~/images/" + EntityOrObjectName;

			if(Size.Length != 0)
			{
				pth += "/" + Size.ToLowerInvariant();
			}
			pth += "/";
			//Now have a _full_ url pth which will take into account any virtual directory mappings
			if(fullPath)
			{
				pth = CommonLogic.SafeMapPath(pth); //AppConfig("StoreFilesPath");
			}
			else
			{
				pth = ResolveUrl(pth);  // resolve tilde to application root
			}
			return pth;
		}

		public static string LookupImage(string EntityOrObjectName, int ID, string ImgSize, int SkinID, string LocaleSetting)
		{
			var useWatermarks = (AppConfigBool("Watermark.Enabled") && !IsAdminSite) && (EntityOrObjectName.Equals("VARIANT", StringComparison.InvariantCultureIgnoreCase) || EntityOrObjectName.Equals("PRODUCT", StringComparison.InvariantCultureIgnoreCase));
			var fileName = string.Empty;

			try
			{
				// using exception block because not all "entities or objects" support this feature, 
				// and this is easiest way to code it so it works for all of them:

				var TableName = EntityOrObjectName.Replace("]", "");
				if(TableName.Equals("VARIANT", StringComparison.InvariantCultureIgnoreCase))
				{
					TableName = "PRODUCTVARIANT";
				}

				if(EntityCanHaveImage(TableName))
				{
					using(var con = new SqlConnection(DB.GetDBConn()))
					{
						con.Open();
						using(var rs = DB.GetRS(string.Format("select ImageFilenameOverride from {0} with (NOLOCK) where {1}ID={2}", "[" + TableName + "]", EntityOrObjectName, ID.ToString()), con))
						{
							if(rs.Read())
							{
								fileName = DB.RSField(rs, "ImageFilenameOverride");
							}
						}
					}
				}
			}
			catch { }
			if(fileName.Length == 0 && EntityOrObjectName.Equals("Product", StringComparison.InvariantCultureIgnoreCase) && AppConfigBool("UseSKUForProductImageName"))
			{
				using(var con = new SqlConnection(DB.GetDBConn()))
				{
					con.Open();
					using(var rs = DB.GetRS("select SKU from product   with (NOLOCK)  where productid=" + ID.ToString(), con))
					{
						if(rs.Read())
						{
							var SKU = DB.RSField(rs, "SKU").Trim();
							if(SKU.Length != 0)
							{
								fileName = SKU;
							}
						}
					}
				}
			}
			if(fileName.Length == 0)
			{
				fileName = ID.ToString();
			}

            string Image1URL = AppLogic.GetCDNImagePath(fileName, ImgSize);

            if (!AppLogic.IsCDNImagePath(Image1URL))
                Image1URL = LocateImageURL(fileName, EntityOrObjectName, ImgSize, LocaleSetting);

            if (Image1URL.Length == 0)
			{
				Image1URL = NoPictureImageURL(ImgSize.Equals("icon", StringComparison.InvariantCultureIgnoreCase), SkinID, LocaleSetting);
			}
			else if(useWatermarks)
			{
				Image1URL = GetWatermarkUrl(ImgSize, Image1URL);
			}

			return Image1URL;
		}

		private static bool EntityCanHaveImage(string tableName)
		{
			if(tableName.ToLower() == "orderoption")
				return false;

			return true;
		}

		public static string LookupImage(string EntityOrObjectName, int ID, string ImageFileNameOverride, string SKU, string ImgSize, int SkinID, string LocaleSetting)
		{
			bool useWatermarks = (AppConfigBool("Watermark.Enabled") && !IsAdminSite) && (EntityOrObjectName.Equals("VARIANT", StringComparison.InvariantCultureIgnoreCase) || EntityOrObjectName.Equals("PRODUCT", StringComparison.InvariantCultureIgnoreCase));
			string fileName = ImageFileNameOverride;
            string Image1URL = AppLogic.GetCDNImagePath(ImageFileNameOverride, ImgSize);

            if (!AppLogic.IsCDNImagePath(Image1URL))
            {

                if (fileName.Length == 0 && EntityOrObjectName.Equals("PRODUCT", StringComparison.InvariantCultureIgnoreCase) && AppConfigBool("UseSKUForProductImageName") && SKU.Trim().Length > 0)
                {
                    fileName = SKU;
                }
                if (fileName.Length == 0)
                {
                    fileName = ID.ToString();
                }
                Image1URL = LocateImageURL(fileName, EntityOrObjectName, ImgSize, LocaleSetting).Replace("//", "/");
            }

			if(Image1URL.Length == 0)
			{
				if(ImgSize.Equals("icon", StringComparison.InvariantCultureIgnoreCase) || ImgSize.Equals("medium", StringComparison.InvariantCultureIgnoreCase) || ImgSize.Equals("large", StringComparison.InvariantCultureIgnoreCase))
				{
					Image1URL = NoPictureImageURL(ImgSize.Equals("icon", StringComparison.InvariantCultureIgnoreCase), SkinID, LocaleSetting);
				}
			}
			else if(useWatermarks)
			{
				Image1URL = GetWatermarkUrl(ImgSize, Image1URL);
			}

			return Image1URL;
		}

        private static string GetCDNImagePath(string fileName, string imgSize)
        {
            var cdn = AppLogic.AppConfig("nal-CDN");
            var result = string.Empty;

            if (!string.IsNullOrWhiteSpace(cdn) && !string.IsNullOrWhiteSpace(fileName))
            {
                if (cdn.EndsWith("/"))
                    cdn = cdn.Substring(0, cdn.Length - 1);

                result = string.Format("{0}/{1}/{2}", cdn, imgSize, fileName);
            }

            if (!AppLogic.URLExists(result))
            {
                result = string.Empty;
            }

            return result;
        }

        public static bool IsCDNImagePath(string url)
        {
            var cdn = AppLogic.AppConfig("nal-CDN");
            var result = false;

            if (!string.IsNullOrWhiteSpace(cdn) && !string.IsNullOrWhiteSpace(url))
                result = url.ToLower().StartsWith(cdn.ToLower());

            return result;

        }

        public static bool URLExists(string url)
        {
            var result = true;

            var webRequest = WebRequest.Create(url);
            webRequest.Timeout = 1200;
            webRequest.Method = "HEAD";

            try
            {
                webRequest.GetResponse();
            }
            catch
            {
                result = false;
            }

            return result;

        }

        // Color string MUST be in master store LocaleSetting!
        public static string LookupProductImageByNumberAndColor(int ProductID, int SkinID, string LocaleSetting, int ImageNumber, string Color, string ImgSize)
		{
			var useWatermarks = AppConfigBool("Watermark.Enabled") && !IsAdminSite;
			var fileName = ProductID.ToString();
			var EntityOrObjectName = "Product";
			var SafeColor = string.Empty;

			var idx = Color.IndexOf("[");
			if(idx != -1)
			{
				SafeColor = CommonLogic.MakeSafeFilesystemName(Color.Substring(0, idx));
			}
			else
			{
				SafeColor = CommonLogic.MakeSafeFilesystemName(Color);
			}

			if(EntityOrObjectName.Equals("PRODUCT", StringComparison.InvariantCultureIgnoreCase) &&
				AppConfigBool("UseSKUForProductImageName"))
			{
				using(var con = new SqlConnection(DB.GetDBConn()))
				{
					con.Open();
					using(var rs = DB.GetRS("select SKU from product   with (NOLOCK)  where productid=" + ProductID.ToString(), con))
					{
						if(rs.Read())
						{
							var SKU = DB.RSField(rs, "SKU").Trim();
							if(SKU.Length != 0)
							{
								fileName = SKU;
							}
						}
					}
				}
			}
			var Image1 = GetImagePath(EntityOrObjectName, ImgSize, true) + fileName + "_" + ImageNumber.ToString() + "_" + SafeColor + ".jpg";
			var Image1URL = GetImagePath(EntityOrObjectName, ImgSize, false) + fileName + "_" + ImageNumber.ToString() + "_" + SafeColor + ".jpg";
			if(!CommonLogic.FileExists(Image1))
			{
				Image1 = GetImagePath(EntityOrObjectName, ImgSize, true) + fileName + "_" + ImageNumber.ToString() + "_" + SafeColor + ".gif";
				Image1URL = GetImagePath(EntityOrObjectName, ImgSize, false) + fileName + "_" + ImageNumber.ToString() + "_" + SafeColor + ".gif";
			}
			if(!CommonLogic.FileExists(Image1))
			{
				Image1 = GetImagePath(EntityOrObjectName, ImgSize, true) + fileName + "_" + ImageNumber.ToString() + "_" + SafeColor + ".png";
				Image1URL = GetImagePath(EntityOrObjectName, ImgSize, false) + fileName + "_" + ImageNumber.ToString() + "_" + SafeColor + ".png";
			}
			if(!CommonLogic.FileExists(Image1))
			{
				Image1URL = string.Empty;
			}

			if(Image1URL.Length == 0)
			{
				Image1URL = NoPictureImageURL(ImgSize.Equals("icon", StringComparison.InvariantCultureIgnoreCase), SkinID, LocaleSetting);
			}
			else if(useWatermarks)
			{
				Image1URL = GetWatermarkUrl(ImgSize, Image1URL);
			}

			return Image1URL;
		}

		public static string LookupProductImageByNumberAndColor(int ProductID, int SkinID, string SKU, string LocaleSetting, int ImageNumber, string Color, string ImgSize)
		{
			bool useWatermarks = AppConfigBool("Watermark.Enabled") && !IsAdminSite;
			string fileName = ProductID.ToString();
			string EntityOrObjectName = "Product";
			string SafeColor = CommonLogic.MakeSafeFilesystemName(Color);

			if(EntityOrObjectName.Equals("PRODUCT", StringComparison.InvariantCultureIgnoreCase) &&
				AppConfigBool("UseSKUForProductImageName") &&
				SKU.Trim().Length > 0)
			{
				fileName = SKU.Trim();
			}

			fileName = fileName + "_" + ImageNumber.ToString() + "_" + SafeColor;

			string Image1URL = LocateImageURL(fileName, "PRODUCT", ImgSize, LocaleSetting);

			if(Image1URL.Length == 0)
			{
				Image1URL = NoPictureImageURL(ImgSize.Equals("icon", StringComparison.InvariantCultureIgnoreCase), SkinID, LocaleSetting);
			}
			else if(useWatermarks)
			{
				Image1URL = GetWatermarkUrl(ImgSize, Image1URL);
			}

			return Image1URL;
		}

		public static string LookupProductImageByNumberAndColor(int ProductID, int SkinID, string ImageFileNameOverride, string SKU, string LocaleSetting, int ImageNumber, string Color, string ImgSize)
		{
			bool useWatermarks = AppConfigBool("Watermark.Enabled") && !IsAdminSite;
			string fileName = string.Empty;

			if(ImageFileNameOverride.Trim().Length > 0)
			{
				fileName = ImageFileNameOverride.Substring(0, ImageFileNameOverride.IndexOf("."));
			}
			string EntityOrObjectName = "Product";
			string SafeColor = CommonLogic.MakeSafeFilesystemName(Color);

			if(EntityOrObjectName.Equals("PRODUCT", StringComparison.InvariantCultureIgnoreCase) &&
				AppConfigBool("UseSKUForProductImageName") &&
				SKU.Trim().Length > 0)
			{
				fileName = SKU.Trim();
			}
			if(fileName.Length == 0)
			{
				fileName = ProductID.ToString();
			}

			fileName = fileName + "_" + ImageNumber.ToString() + "_" + SafeColor;
			string Image1URL = LocateImageURL(fileName, "PRODUCT", ImgSize, LocaleSetting);
			if(Image1URL.Length == 0)
			{
				Image1URL = NoPictureImageURL(ImgSize.Equals("icon", StringComparison.InvariantCultureIgnoreCase), SkinID, LocaleSetting);
			}
			else if(useWatermarks)
			{
				Image1URL = GetWatermarkUrl(ImgSize, Image1URL);
			}

			return Image1URL;
		}

		static string GetWatermarkUrl(string imageSize, string originalImageUrl)
		{
			if(string.IsNullOrEmpty(imageSize)
				&& string.IsNullOrEmpty(originalImageUrl))
				return originalImageUrl;

			if(imageSize.Equals("icon", StringComparison.InvariantCultureIgnoreCase)
				&& !AppConfigBool("Watermark.Icons.Enabled"))
				return originalImageUrl;

			return Url.Action(
				actionName: ActionNames.Index,
				controllerName: ControllerNames.Watermark,
				routeValues: new RouteValueDictionary
				{
					{ RouteDataKeys.ImageSize, imageSize },
					{ RouteDataKeys.ImageUrl, originalImageUrl }
				});
		}

		public static void SetCookie(String cookieName, String cookieVal, TimeSpan ts)
		{
			try
			{
				HttpCookie cookie = new HttpCookie(cookieName);
				cookie.Value = HttpContext.Current.Server.UrlEncode(cookieVal);
				DateTime dt = DateTime.Now;
				cookie.Expires = dt.Add(ts);
				if(OnLiveServer())
				{
					cookie.Domain = LiveServer();
				}
				HttpContext.Current.Response.Cookies.Add(cookie);
			}
			catch
			{ }
		}

		public static String MakeProperObjectName(String pname, String vname, String LocaleSetting)
		{
			vname = XmlCommon.GetLocaleEntry(vname, LocaleSetting, true);
			pname = XmlCommon.GetLocaleEntry(pname, LocaleSetting, true);
			if(vname.Trim().Length == 0 || pname == vname)
			{
				return pname.Trim();
			}
			else
			{
				return String.Format("{0}-{1}", pname.Trim(), vname.Trim());
			}
		}

		public static String MakeProperProductName(int ProductID, int VariantID, String LocaleSetting)
		{
			return MakeProperObjectName(GetProductName(ProductID, LocaleSetting), GetVariantName(VariantID, LocaleSetting), LocaleSetting);
		}

		public static String MakeProperProductSKU(String pSKU, String vSKU, String colorMod, String sizeMod)
		{
			return pSKU + vSKU + colorMod + sizeMod;
		}

		static public void SendMail(string subject = null, string body = null, bool useHtml = false, string fromAddress = null, string fromName = null, string toAddress = null, string toName = null, string bccAddresses = null, string replyToAddress = null, string server = null)
		{
			if(server == null)
				server = MailServer();

			if(string.IsNullOrWhiteSpace(server)
				|| server.Equals(ro_TBD, StringComparison.InvariantCultureIgnoreCase)
				|| server.Equals("MAIL.YOURDOMAIN.COM", StringComparison.InvariantCultureIgnoreCase))
			{
				if(IsAdminSite)
					throw new ArgumentException("Invalid Mail Server: " + server + "" + GetString("admin.splash.aspx.security.MailServer", Customer.Current.SkinID, Customer.Current.LocaleSetting));

				return;
			}

			if(string.IsNullOrWhiteSpace(fromAddress))
				fromAddress = AppConfig("MailMe_FromAddress");

			if(fromName == null)
				fromName = AppConfig("MailMe_FromName");

			if(string.IsNullOrWhiteSpace(toAddress))
				toAddress = AppConfig("MailMe_ToAddress");

			if(toName == null)
				toName = AppConfig("MailMe_ToName");

			if(toAddress.Contains(",") || toAddress.Contains(";"))
			{
				//Ensure that the TO: address string is only a single address
				var indexOfComma = toAddress.IndexOf(',');
				var indexOfSemicolon = toAddress.IndexOf(';');
				if(indexOfComma != -1 || indexOfSemicolon != -1)
				{
					var multipleAddresses = toAddress;

					if(indexOfSemicolon != -1)
						toAddress = multipleAddresses.Substring(0, indexOfSemicolon);

					//Commas win if there happen to be both, it's what we used to say was supported
					if(indexOfComma != -1)
						toAddress = multipleAddresses.Substring(0, indexOfComma);

					SysLog.LogMessage("Email was configured to go to multiple addresses, which is not supported.",
						string.Format("Instead of going to {0}, the email was sent to only {1}.", multipleAddresses, toAddress),
						MessageTypeEnum.Informational,
						MessageSeverityEnum.Alert);
				}
			}

			using(var message = new MailMessage(
				from: new MailAddress(fromAddress, fromName),
				to: new MailAddress(toAddress, toName)))
			{
				if(!string.IsNullOrWhiteSpace(replyToAddress))
					message.ReplyToList.Add(new MailAddress(replyToAddress));

				message.Subject = subject;
				message.Body = body;
				message.IsBodyHtml = useHtml;

				if(!string.IsNullOrWhiteSpace(bccAddresses))
					foreach(string s in bccAddresses.Split(new char[] { ',', ';' }))
						if(s.Trim().Length > 0)
							message.Bcc.Add(new MailAddress(s.Trim()));

				var credentials = string.IsNullOrWhiteSpace(AppConfig("MailMe_User"))
					? CredentialCache.DefaultNetworkCredentials
					: new NetworkCredential(
						userName: AppConfig("MailMe_User"),
						password: AppConfig("MailMe_Pwd"));

				var client = new SmtpClient(server)
				{
					UseDefaultCredentials = credentials != CredentialCache.DefaultNetworkCredentials,
					Credentials = credentials,
					EnableSsl = AppConfigBool("MailMe_UseSSL"),
					Port = AppConfigNativeInt("MailMe_Port")
				};

				try
				{
					client.Send(message);
				}
				catch(Exception ex)
				{
					if(IsAdminSite)
						throw new ArgumentException(string.Format("Mail Error occurred - {0}", CommonLogic.GetExceptionDetail(ex, "")));
				}
			}
		}

		static public string GetCountryName(string CountryTwoLetterISOCode)
		{
			var tmp = "United Countries"; // default to US just in case

			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(var rs = DB.GetRS("select * from country   with (NOLOCK)  where upper(TwoLetterISOCode)=" + DB.SQuote(CountryTwoLetterISOCode.ToUpperInvariant()), con))
				{
					if(rs.Read())
					{
						tmp = DB.RSField(rs, "name");
					}
				}
			}

			return tmp;
		}

		/// <summary>
		/// //Returns true if the Customer has previously purchased this product.
		/// </summary>
		public static bool Owns(int ProductID, int CustomerID)
		{
			int nCount = 0;

			//VIP users have total access
			if(HttpContext.Current.User.IsInRole("VIP"))
			{
				return true;
			}

			if(ProductID != 0)
			{
				nCount = DB.GetSqlN(String.Format("select top 1 os.productid as N from dbo.orders_ShoppingCart os  with (NOLOCK)  join dbo.orders o  with (NOLOCK)  on o.ordernumber = os.ordernumber  where o.CustomerID={0} and os.ProductID={1} and o.TransactionState={2}", CustomerID, ProductID, DB.SQuote(ro_TXStateCaptured)));
				if(nCount != 0)
				{
					return true;
				}
			}
			return false;
		}

		public static int GetMicroPayProductID()
		{
			var result = 0;

			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(var rs = DB.GetRS("SELECT ProductID FROM Product WITH (NOLOCK) WHERE Deleted = 0 and Published = 1 and Sku ='MICROPAY'", con))
				{
					if(rs.Read())
					{
						result = DB.RSFieldInt(rs, "ProductID");
					}
				}
			}

			return result;
		}

		public static int GetAdHocProductID()
		{
			var result = 0;

			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(var rs = DB.GetRS("select ProductID from Product          with (NOLOCK)  where deleted=0 and SKU='ADHOCCHARGE'", con))
				{
					if(rs.Read())
					{
						result = DB.RSFieldInt(rs, "ProductID");
					}
				}
			}

			return result;
		}

		public static decimal GetMicroPayBalance(int CustomerID)
		{
			var result = Decimal.Zero;

			if(CustomerID != 0)
			{
				using(var con = new SqlConnection(DB.GetDBConn()))
				{
					con.Open();
					using(var rs = DB.GetRS(String.Format("select MicroPayBalance from Customer   with (NOLOCK)  where CustomerID={0}", CustomerID), con))
					{
						if(rs.Read())
						{
							result = DB.RSFieldDecimal(rs, "MicroPayBalance");
						}
					}
				}
			}
			return result;
		}

		static String readonly_WCLocale = String.Empty; // for speed

		/// <summary>
		/// Returns the string resource for the specified key using the default web.config locale and default store skin ID
		/// This should ONLY be used in instances where multi-lingual support is NEVER needed
		/// </summary>
		/// <param name="key">The name (key) of the string to return</param>
		/// <returns></returns>
		public static string GetStringForDefaultLocale(string key)
		{
			return GetString(key, readonly_WCLocale);
		}

		/// <summary>
		/// Determines the runtime StringResource for <paramref name="key"/>.
		/// </summary>
		/// <param name="key">Key of the string resource</param>
		/// <param name="LocaleSetting">The language setting from which do derive the string</param>
		/// <returns>The StringResource from the appropriate language for the key specified</returns>
		public static string GetString(string key, string localeSetting = null, int? storeId = null)
		{
			// Undocumented diagnostic mode
			if(AppConfigBool("ShowStringResourceKeys"))
				return key;

			// Try the current store and requested locale
			var effectiveStoreId = storeId.HasValue 
				? storeId.Value 
				: StoreID();

			if(string.IsNullOrEmpty(localeSetting))
				localeSetting = Customer.Current.LocaleSetting;

			var stringResource = StringResourceManager.GetStringResource(effectiveStoreId, localeSetting, key);
			if(stringResource != null)
				return stringResource.ConfigValue;

			// Try the current store and web.config locale
			if(string.IsNullOrEmpty(readonly_WCLocale))
				readonly_WCLocale = Localization.GetDefaultLocale();

			stringResource = StringResourceManager.GetStringResource(effectiveStoreId, readonly_WCLocale, key);
			if(stringResource != null)
				return stringResource.ConfigValue;

			// Try the default store and requested locale
			var defaultStoreId = DefaultStoreID();

			stringResource = StringResourceManager.GetStringResource(defaultStoreId, localeSetting, key);
			if(stringResource != null)
				return stringResource.ConfigValue;

			// Try the default store and web.config locale
			stringResource = StringResourceManager.GetStringResource(defaultStoreId, readonly_WCLocale, key);
			if(stringResource != null)
				return stringResource.ConfigValue;

			// Not found
			return key;
		}

		/// <summary>
		/// Returns the runtime StringResource for 'key'
		/// </summary>
		/// <param name="key">Key of the string resource</param>
		/// <param name="SkinID">ID of the skin for which to return</param>
		/// <param name="LocaleSetting">The language setting from which do derive the string</param>
		/// <returns></returns>
		public static string GetString(string key, int skinId, string localeSetting)
		{
			return GetString(key, localeSetting);
		}

		// ----------------------------------------------------------------
		//
		// APPCONFIG SUPPORT ROUTINES
		//
		// ----------------------------------------------------------------
		public static void SetGlobalConfig(String Name, String ConfigValue)
		{
			AspDotNetStorefrontCore.GlobalConfig c = AspDotNetStorefrontCore.GlobalConfig.GetGlobalConfig(Name);
			if(c == null)
				return;
			c.ConfigValue = ConfigValue;
			c.Save();
		}

		public static string GlobalConfig(string paramName)
		{
			if(!GlobalConfigTable.ContainsKey(paramName))
			{
				SysLog.LogMessage(string.Format(GetStringForDefaultLocale("admin.globalconfig.DoesNotExist"), paramName),
					string.Empty,
					MessageTypeEnum.GeneralException,
					MessageSeverityEnum.Error);

				return string.Empty;
			}

			return GlobalConfigTable[paramName].ConfigValue;
		}

		public static bool GlobalConfigBool(string paramName)
		{
			if(!GlobalConfigTable.ContainsKey(paramName))
			{
				SysLog.LogMessage(string.Format(GetStringForDefaultLocale("admin.globalconfig.DoesNotExist"), paramName),
					string.Empty,
					MessageTypeEnum.GeneralException,
					MessageSeverityEnum.Error);

				return false;
			}

			return Localization.ParseBoolean(GlobalConfigTable[paramName].ConfigValue);
		}

		/// <summary>
		/// Gets the AppConfig value
		/// </summary>
		/// <param name="paramName"></param>
		/// <returns></returns>
		public static string AppConfig(string paramName)
		{
			var config = GetAppConfigRouted(paramName);

			return config == null
				? string.Empty
				: config.ConfigValue;
		}

		public static AppConfig GetAppConfigRouted(string paramName, int? storeId = null)
		{
			return AppConfigManager.GetAppConfig(paramName, storeId ?? StoreID(), true);
		}

		///// <summary>
		///// Used with StoreID parameter. Load appconfig depending on StoreID parameter.
		///// </summary>
		///// <param name="storeID">Store ID to load an appconfig value</param>
		///// <param name="paramName">Appconfig name</param>
		///// <param name="cascadeToDefault">If the app config for this store is not found the default will be returned.</param>
		///// <returns>Appconfig value</returns>
		public static string AppConfig(string name, int storeId, bool cascadeToDefault)
		{
			return AppConfigManager.GetAppConfigValue(name, storeId, cascadeToDefault);
		}

		//public static bool AppConfigBool(int StoreId, String paramName, bool cascadeToDefault)
		public static bool AppConfigBool(string paramName, int storeId, bool cascadeToDefault)
		{
			var tmp = AppConfig(paramName, storeId, cascadeToDefault);
			if(tmp.Equals("TRUE", StringComparison.InvariantCultureIgnoreCase) ||
				tmp.Equals("YES", StringComparison.InvariantCultureIgnoreCase) ||
				tmp.Equals("1", StringComparison.InvariantCultureIgnoreCase))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public static bool AppConfigBool(string paramName)
		{
			var tmp = AppConfig(paramName);
			if(tmp.Equals("TRUE", StringComparison.InvariantCultureIgnoreCase) ||
				tmp.Equals("YES", StringComparison.InvariantCultureIgnoreCase) ||
				tmp.Equals("1", StringComparison.InvariantCultureIgnoreCase))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public static int AppConfigUSInt(string paramName)
		{
			return Localization.ParseUSInt(AppConfig(paramName));
		}

		public static Single AppConfigUSSingle(string paramName)
		{
			return Localization.ParseUSSingle(AppConfig(paramName));
		}

		public static double AppConfigUSDouble(string paramName)
		{
			return Localization.ParseUSDouble(AppConfig(paramName));
		}

		public static decimal AppConfigUSDecimal(string paramName)
		{
			return Localization.ParseUSDecimal(AppConfig(paramName));
		}
		public static int AppConfigNativeInt(string paramName)
		{
			return Localization.ParseNativeInt(AppConfig(paramName));
		}

		public static decimal AppConfigNativeDecimal(string paramName)
		{
			return Localization.ParseNativeDecimal(AppConfig(paramName));
		}

		public static double AppConfigNativeDouble(string paramName)
		{
			return Localization.ParseNativeDouble(AppConfig(paramName));
		}

		//-----------------------------------------
		//EventHandler Support Routine
		//
		//------------------------------------------
		public static AspdnsfEventHandler eventHandler(String eventName)
		{
			if(EventHandlerTable[eventName] != null)
			{
				return EventHandlerTable[eventName];
			}

			//Returning Dummy object with Active as False to avoid null returns
			AspdnsfEventHandler dummyHandler = new AspdnsfEventHandler(0, "", "", "", false, false);
			return dummyHandler;
		}

		public static BadWord badWord(String word)
		{
			if(BadWordTable[word] != null)
			{
				return BadWordTable[word];
			}
			else
			{   //Returning Dummy object with Active as False to avoid null returns
				BadWord dummy = new BadWord(0, "", "", DateTime.Now);
				return dummy;
			}
		}

		public static void AuditLogInsert(int CustomerID, int UpdatedCustomerID, int OrderNumber, string Description, string Details, string PagePath, string AuditGroup)
		{
			if(UpdatedCustomerID == 0 && OrderNumber != 0)
			{
				UpdatedCustomerID = Order.GetOrderCustomerID(OrderNumber);
			}

			if(CustomerID == 0)
			{
				try
				{
					if(HttpContext.Current.User != null)
					{
						Customer AuditCustomer = HttpContext.Current.GetCustomer();
						if(AuditCustomer != null)
						{
							CustomerID = AuditCustomer.CustomerID;
						}
					}
				}
				catch { }
			}

			if(Description.Length > 100) Description = CommonLogic.Ellipses(Description, 100, false);
			if(Details.Length > 1000) Details = CommonLogic.Ellipses(Details, 1000, true);
			if(PagePath.Length > 200) PagePath = PagePath.Substring(0, 200);
			if(AuditGroup.Length > 30) AuditGroup = AuditGroup.Substring(0, 30);
			DB.ExecuteSQL("insert into AuditLog(CustomerID,UpdatedCustomerID,OrderNumber,"
				+ "Description,Details,PagePath,AuditGroup)"
				+ " values(" + CustomerID.ToString() + "," + UpdatedCustomerID.ToString() + "," + OrderNumber.ToString()
				+ "," + DB.SQuote(Description) + "," + DB.SQuote(Details) + "," + DB.SQuote(PagePath) + "," + DB.SQuote(AuditGroup) + ")");
		}

		/// <summary>
		/// Determine the current website id.
		/// </summary>
		/// <returns>Store id.</returns>
		public static int StoreID()
		{
			// force default
			int sID = 1;
			if(HttpContext.Current != null &&
				HttpContext.Current.Items["StoreId"] != null)
			{
				return Convert.ToInt32(HttpContext.Current.Items["StoreId"]);
			}
			return sID;
		}

		/// <summary>
		/// Gets the default store id
		/// </summary>
		/// <returns>Store id.</returns>
		public static int DefaultStoreID()
		{
			// force default
			int defStoreId = 1;
			if(HttpContext.Current != null &&
				HttpContext.Current.Items["DefaultStoreId"] != null)
			{
				return Convert.ToInt32(HttpContext.Current.Items["DefaultStoreId"]);
			}
			else
			{
				var defStore = GetDefaultStore();
				if(defStore != null)
				{
					defStoreId = defStore.StoreID;
				}
			}

			return defStoreId;
		}

		/// <summary>
		/// Determine the store id where the order is created.
		/// </summary>
		/// <param name="orderNumber">Order number to be queried.</param>
		/// <returns>Store ID of the order.</returns>
		public static int GetOrdersStoreID(int orderNumber)
		{
			var id = 1;
			using(var conn = new SqlConnection(DB.GetDBConn()))
			{
				conn.Open();
				id = DB.GetSqlN(string.Format("Select StoreID as N from Orders with (NOLOCK) where OrderNumber={0}", orderNumber), conn);
			}

			return id;
		}

		/// <summary>
		/// ML/Express feature that limits the number of products
		/// </summary>
		/// <returns>returns true if total count of published and non deleted products exceeded the specified limit</returns>
		public static bool MaxProductsExceeded()
		{
			return false;
		}

		/// <summary>
		/// ML/Express feature that limits the number of entities
		/// </summary>
		/// <returns>returns true if total count of published and non deleted entities exceeded the specified limit</returns>
		public static bool MaxEntitiesExceeded()
		{
			return false;
		}

		/// <summary>
		/// Returns a site relative HTTP path from a partial path starting out with a ~.
		/// Same syntax that ASP.Net internally supports but this method can be used
		/// outside of the Page framework.
		///
		/// Works like Control.ResolveUrl including support for ~ syntax
		/// but returns an absolute URL.
		/// </summary>
		/// <param name="originalUrl">Any Url including those starting with ~</param>
		/// <returns>relative url</returns>
		public static string ResolveUrl(string originalUrl)
		{
			if(originalUrl == null) return null;

			// *** Absolute path - just return
			if(originalUrl.IndexOf("://") != -1) return originalUrl;

			// *** Fix up image path for ~ root app dir directory
			if(originalUrl.StartsWith("~"))
			{
				string newUrl = "";
				if(HttpContext.Current != null)
				{
					newUrl = HttpContext.Current.Request.ApplicationPath + originalUrl.Substring(1).Replace("//", "/");
				}
				else
				{
					// *** Not context: assume current directory is the base directory
					throw new ArgumentException("Invalid URL: Relative URL not allowed.");
				}

				// *** Just to be sure fix up any double slashes
				return newUrl.Replace("//", "/");
			}

			return originalUrl;
		}

		/// <summary>
		/// Formats fully qualified page URLs for use on the admin site since masterpages can be upredictable with relative urls
		/// </summary>
		/// <param name="pageName">page name to generate the link to</param>
		/// <returns>fully qualified URL to the supplied page name</returns>
		public static String AdminLinkUrl(String pageName)
		{
			return AdminLinkUrl(pageName, false);
		}

		public static String AdminLinkUrl(String pageName, bool AllowSub)
		{
			if(pageName.ToLowerInvariant().Contains("http"))
			{
				//fully qualified URL.  Nothing to do.
				return pageName;
			}
			else
			{
				String caller = HttpContext.Current.Request.FilePath;
				String url = caller.Remove(caller.LastIndexOf("/"));

				// don't replace "/" because we're digging into a subdirectory
				if(AllowSub)
				{
					return url + "/" + pageName;
				}
				return url + "/" + pageName.Replace("/", "");
			}
		}

		#region CUSTOMGLOBALLOGIC
		public static void Custom_ApplicationStart_Logic(Object sender, EventArgs e)
		{
			// put any custom application start logic you need here...
			// do not change this routine unless you know exactly what you are doing
		}
		public static void Custom_ApplicationEnd_Logic(Object sender, EventArgs e)
		{
			// put any custom application end logic you need here...
			// do not change this routine unless you know exactly what you are doing
		}
		public static void Custom_SessionStart_Logic(Object sender, EventArgs e)
		{
			// put any custom session start logic you need here...
			// do not change this routine unless you know exactly what you are doing
		}
		public static void Custom_SessionEnd_Logic(Object sender, EventArgs e)
		{
			// put any custom session end logic you need here...
			// do not change this routine unless you know exactly what you are doing
		}
		public static void Custom_Application_Error(Object sender, EventArgs e)
		{
			// put any custom application error logic you need here...
			// do not change this routine unless you know exactly what you are doing
		}
		public static void Custom_Application_EndRequest_Logic(Object sender, EventArgs e)
		{
			// put any custom application end request logic you need here...
			// do not change this routine unless you know exactly what you are doing
		}
		public static bool Custom_Application_BeginRequest_Logic(Object sender, EventArgs e)
		{
			// put any custom application begin request logic you need here...
			// return TRUE if you do NOT want our UrlRewriter to fire
			// return FALSE if you do want our UrlRewriter to fire and handle this event
			// do not change this routine unless you know exactly what you are doing
			return true;
		}
		#endregion

		public static bool CurrentRequestIsLicensableHandler()
		{
			var currentContext = HttpContext.Current;
			if (currentContext == null)
				return false;

			var currentHandler = currentContext.CurrentHandler;
			return currentHandler is Page
				|| currentHandler is MvcHandler;
		}

        /// <summary>
        /// Checks if the current request is a callback
        /// </summary>
        /// <returns></returns>
        public static bool CurrentRequestIsCallBack()
		{
			HttpContext ctx = HttpContext.Current;
			if(ctx != null)
			{
				return ctx.CurrentHandler is Page &&
						(ctx.CurrentHandler as Page).IsCallback;
			}

			return false;
		}

		/// <summary>
		/// Checks the collection for the name value pair
		/// </summary>
		/// <param name="values"></param>
		/// <param name="name"></param>
		/// <param name="compareWith"></param>
		/// <returns></returns>
		public static bool ContainsValue(NameValueCollection values, string name, string compareWith)
		{
			foreach(string key in values.AllKeys)
			{
				if(key.EqualsIgnoreCase(name))
				{
					string value = values[key];
					return !string.IsNullOrEmpty(value) && value.EqualsIgnoreCase(compareWith);
				}
			}

			return false;
		}

		/// <summary>
		/// Checks the current request if it's an async postback for asp.net ajax Update Panel
		/// </summary>
		/// <returns></returns>
		public static bool CurrentRequestIsAsyncPostBack()
		{
			HttpContext ctx = HttpContext.Current;
			if(ctx != null)
			{
				return ContainsValue(ctx.Request.Headers, "X-MicrosoftAjax", "Delta=true") &&
						ContainsValue(ctx.Request.Form, "__ASYNCPOST", "true");
			}

			return false;
		}

		/// <summary>
		/// Used for aspx page that do not inherit SkinBase.
		/// Example are those gateways that requires external redirection.
		/// </summary>
		/// <returns>Current customer making the order.</returns>
		public static Customer GetCurrentCustomer()
		{
			return Customer.Current;
		}

		public static void LowInventoryWarning(List<int> variantIds)
		{
			bool sendWarning = false;
			int warningThreshold = AppConfigNativeInt("SendLowStockWarningsThreshold");
			string lowProducts = string.Empty;

			foreach(int vId in variantIds)
			{
				ProductVariant varToCheck = new ProductVariant(vId);
				if(varToCheck.Inventory < warningThreshold)
				{
					Product tempProduct = new Product(varToCheck.ProductID);
					lowProducts += Environment.NewLine + tempProduct.LocaleName + " (VariantID: " + varToCheck.VariantID.ToString() + ")";
					sendWarning = true;
				}
			}

			if(sendWarning)
			{
				try //Don't not display the orderconfirmation page just because email sending failed
				{
					SendMail(subject: AppConfig("StoreName") + " " + GetStringForDefaultLocale("admin.LowStockWarningTitle"),
						body: GetStringForDefaultLocale("admin.LowStockWarningBody") + lowProducts,
						useHtml: false,
						fromAddress: AppConfig("GotOrderEmailFrom"),
						fromName: AppConfig("GotOrderEmailFromName"),
						toAddress: AppConfig("GotOrderEmailTo"),
						toName: AppConfig("MailMe_ToName"),
						bccAddresses: string.Empty,
						replyToAddress: AppConfig("MailMe_Server"));
				}
				catch(Exception ex)
				{
					var logEx = new Exception("Low stock warning couldn't be sent", ex);
					SysLog.LogException(logEx, MessageTypeEnum.GeneralException, MessageSeverityEnum.Message);
				}
			}
		}

		/// <summary>
		/// Breaks up the dimensions value and returns a single desired dimension
		/// </summary>
		/// <param name="Dimensions"></param>
		/// <param name="DesiredDimension"></param>
		/// <returns></returns>
		public static string RetrieveProductDimension(string Dimensions, string DesiredDimension)
		{
			string trimmedDimensions = Dimensions.ToLower().Trim();
			string pattern = "^(\\d*\\.?\\d*x\\d*\\.?\\d*x\\d*\\.?\\d*)$";
			Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
			Match isMatch = rgx.Match(trimmedDimensions);

			if(isMatch.Success)
			{
				string[] splitDimensions = trimmedDimensions.Split('x');

				switch(DesiredDimension.ToLower())
				{
					case "width":
						return splitDimensions[0].Trim();
					case "height":
						return splitDimensions[1].Trim();
					case "depth":
						return splitDimensions[2].Trim();
					default:
						return string.Empty;
				}
			}
			else
			{
				return string.Empty;
			}
		}

		public static bool ShoppingCartRecIdIsValid(int shoppingCartRecId)
		{
			var sqlCommand = new SqlCommand("SELECT COUNT(*) FROM ShoppingCart WHERE ShoppingCartRecId=@ShoppingCartRecId");
			sqlCommand.Parameters.Add(new SqlParameter("ShoppingCartRecId", shoppingCartRecId));
			return DB.Scalar<int>
				.ExecuteScalar(sqlCommand) > 0;
		}

		public static bool UserAgentIsKnownBot()
		{
			var regex = new Regex(AppConfig("BotUserAgentRegEx"));
			return regex.IsMatch(HttpContext.Current.Request.UserAgent.ToLower());
		}
	}

	public static class AppStartLogger
	{
		public static void WriteLine(String Message)
		{
			// don't let logging crash anything itself!
			try
			{
				FileStream fs = new FileStream(CommonLogic.SafeMapPath("images/appstart.log"), FileMode.Append, FileAccess.Write, FileShare.Read | FileShare.Delete);
				StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
				sw.AutoFlush = true;
				sw.WriteLine("{0:G}: {1}\r\n", DateTime.Now, Message);
				sw.Close();
				fs.Close();
			}
			catch { }
		}

		public static void ResetLog()
		{
			// don't let logging crash anything itself!
			try
			{
				File.Delete(CommonLogic.SafeMapPath("images/appstart.log"));
			}
			catch { }
		}
	}
}
