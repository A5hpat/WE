// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Xml.Serialization;

namespace AspDotNetStorefrontCore
{
	public enum VATSettingEnum
    {
        UnknownValue = 0,
        ShowPricesInclusiveOfVAT = 1,
        ShowPricesExclusiveOfVAT = 2
    }

    /// <summary>
    /// Customer object info is rebuilt off of session["CustomerID"]!!!! This is created in AppLogic.SessionStart or other places
    /// this is the ONLY data stored in session and is server farm safe
    /// </summary>
    [Serializable]
    public class Customer : IIdentity
    {
        // referrer is handled specially (to get the very first one!)
        /// <summary>
        /// The ReadOnly String for getting the string "Referrer";
        /// </summary>
        public static readonly String ro_ReferrerCookieName = "Referrer";

        #region Private Variables

        private int m_CustomerID;
        private bool m_HasCustomerRecord;
        private String m_CustomerGUID = String.Empty;
        private int m_CustomerLevelID;
        private String m_CustomerLevelName;
        private int m_AffiliateID;
        private String m_LocaleSetting;
        private String m_CurrencySetting;
        private VATSettingEnum m_VATSetting;
        private String m_VATRegistrationID;
        private String m_Phone;
        private String m_EMail;
        private bool m_OKToEMail;
        private String m_Password; // salted and hashed (on retrieval from the db)
        private int m_SaltKey;
        private bool m_IsRegistered;
        private bool m_IsAdminUser;
        private bool m_IsAdminSuperUser;
        private decimal m_MicroPayBalance;
        private String m_FirstName;
        private String m_LastName;
        private String m_Notes;
        private String m_LastIPAddress;
        private readonly bool m_SuppressCookies;
        private int m_PrimaryBillingAddressID;
        private int m_PrimaryShippingAddressID;
        private int m_SkinID;
        private decimal m_LevelDiscountPct;
        private bool m_DiscountExtendedPrices;
        private string m_Roles = string.Empty;
        private int m_CurrentSessionID;
        private bool m_StoreCCInDB;
        private DateTime m_LockedUntil;
        private bool m_AdminCanViewCC;
        private DateTime m_LastActivity;
        private DateTime m_PwdChanged;
        private byte m_BadLoginCount;
        private bool m_Active;
        private bool m_Over13;
        private bool m_PwdChgRequired;
        private String m_CouponCode;
        private CustomerSession m_CustomerSession;
        private Address m_PrimaryBillingAddress;
        private Address m_PrimaryShippingAddress;
        private string m_RequestedPaymentMethod;
        private DateTime m_CreatedOn;
        readonly SqlTransaction m_DBTrans = null;

        private bool m_DefaultCustLevel_DiscountExtendedPrices;
        private decimal m_DefaultCustLevel_LevelDiscountPct;
        private string m_DefaultCustLevel_CustomerLevelName;
        private int m_DefaultCustLevel_CustomerLevelID = 0;
        // nal m_DefaultCustLevel_CustomerLevelID
        //{
        //    get
        //    {
        //        if (AppLogic.AppConfigUSInt("DefaultCustomerLevelID") > 0)
        //        {
        //            return AppLogic.AppConfigUSInt("DefaultCustomerLevelID");
        //        }
        //        else
        //        {
        //            return 0;
        //        }
        //    }
        //}

        private int m_FailedTransactionCount = -1;
        #endregion

        /// <summary>
        /// The ReadOnly String for getting the string "LocaleSetting";
        /// </summary>
        public static readonly String ro_LocaleSettingCookieName = "LocaleSetting";
        /// <summary>
        /// The ReadOnly String for getting the string "CurrencySetting";
        /// </summary>
        public static readonly String ro_CurrencySettingCookieName = "CurrencySetting";
        /// <summary>
        /// The ReadOnly String for getting the string "AffiliateID";
        /// </summary>
        public static readonly String ro_AffiliateCookieName = "AffiliateID";
        /// <summary>
        /// The ReadOnly String for getting the string "VATSettingID";
        /// </summary>
        public static readonly String ro_VATSettingCookieName = "VATSettingID";

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Customer"/> class.
        /// </summary>
        /// <param name="CustomerID">The customerID.</param>
        public Customer(int CustomerID)
            : this(CustomerID, false)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Customer"/> class.
        /// </summary>
        /// <param name="SuppressCookies">if set to <c>true</c> [suppress cookies].</param>
        public Customer(bool SuppressCookies)
            : this(0, SuppressCookies)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Customer"/> class.
        /// </summary>
        /// <param name="CustomerID">The customerID.</param>
        /// <param name="SuppressCookies">if set to <c>true</c> [suppress cookies].</param>
        public Customer(int CustomerID, bool SuppressCookies)
            : this(CustomerID, SuppressCookies, false)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Customer"/> class.
        /// </summary>
        /// <param name="CustomerID">The customerID.</param>
        /// <param name="SuppressCookies">if set to <c>true</c> [suppress cookies].</param>
        /// <param name="UseDBDataOnly">if set to <c>true</c> [use DB data only].</param>
        public Customer(int CustomerID, bool SuppressCookies, bool UseDBDataOnly)
            : this(null, CustomerID, SuppressCookies, UseDBDataOnly)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Customer"/> class.
        /// </summary>
        /// <param name="CustomerGUID">The customer GUID.</param>
        public Customer(Guid CustomerGUID)
            : this(CustomerGUID, false)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Customer"/> class.
        /// </summary>
        /// <param name="CustomerGUID">The customer GUID.</param>
        /// <param name="SuppressCookies">if set to <c>true</c> [suppress cookies].</param>
        public Customer(Guid CustomerGUID, bool SuppressCookies)
            : this(CustomerGUID, SuppressCookies, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Customer"/> class.
        /// </summary>
        /// <param name="CustomerGUID">The customer GUID.</param>
        /// <param name="SuppressCookies">if set to <c>true</c> [suppress cookies].</param>
        /// <param name="UseDBDataOnly">if set to <c>true</c> [use DB data only].</param>
        public Customer(Guid CustomerGUID, bool SuppressCookies, bool UseDBDataOnly)
            : this(null, CustomerGUID, SuppressCookies, UseDBDataOnly)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Customer"/> class.
        /// </summary>
        /// <param name="email">The email.</param>
        public Customer(string email)
            : this(email, false)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Customer"/> class.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="SuppressCookies">if set to <c>true</c> [suppress cookies].</param>
        public Customer(string email, bool SuppressCookies)
            : this(email, SuppressCookies, false)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Customer"/> class.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="SuppressCookies">if set to <c>true</c> [suppress cookies].</param>
        /// <param name="UseDBDataOnly">if set to <c>true</c> [use DB data only].</param>
        public Customer(string email, bool SuppressCookies, bool UseDBDataOnly)
            : this(null, email, SuppressCookies, UseDBDataOnly)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Customer"/> class.
        /// </summary>
        /// <param name="DBTrans">The DataBase trans.</param>
        /// <param name="CustomerID">The customer ID.</param>
        /// <param name="SuppressCookies">if set to <c>true</c> [suppress cookies].</param>
        public Customer(SqlTransaction DBTrans, int CustomerID, bool SuppressCookies)
            : this(DBTrans, CustomerID, SuppressCookies, false)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Customer"/> class.
        /// </summary>
        /// <param name="DBTrans">The DataBase trans.</param>
        /// <param name="CustomerID">The customer ID.</param>
        /// <param name="SuppressCookies">if set to <c>true</c> [suppress cookies].</param>
        /// <param name="UseDBDataOnly">if set to <c>true</c> [use DB data only].</param>
        public Customer(SqlTransaction DBTrans, int CustomerID, bool SuppressCookies, bool UseDBDataOnly)
        {
            m_DBTrans = DBTrans;
            m_SuppressCookies = SuppressCookies;
            Init(CustomerID);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Customer"/> class.
        /// </summary>
        /// <param name="DBTrans">The DataBase trans.</param>
        /// <param name="CustomerGUID">The customer GUID.</param>
        /// <param name="SuppressCookies">if set to <c>true</c> [suppress cookies].</param>
        /// <param name="UseDBDataOnly">if set to <c>true</c> [use DB data only].</param>
        public Customer(SqlTransaction DBTrans, Guid CustomerGUID, bool SuppressCookies, bool UseDBDataOnly)
        {
            m_DBTrans = DBTrans;
            m_SuppressCookies = SuppressCookies;
            Init(CustomerGUID);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Customer"/> class.
        /// </summary>
        /// <param name="DBTrans">The Database transaction.</param>
        /// <param name="email">The email.</param>
        /// <param name="SuppressCookies">if set to <c>true</c> [suppress cookies].</param>
        /// <param name="UseDBDataOnly">if set to <c>true</c> [use DB data only].</param>
        public Customer(SqlTransaction DBTrans, string email, bool SuppressCookies, bool UseDBDataOnly)
            : this(DBTrans, email, SuppressCookies, UseDBDataOnly, false)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="Customer"/> class.
        /// </summary>
        /// <param name="DBTrans">The Database transaction.</param>
        /// <param name="email">The email.</param>
        /// <param name="SuppressCookies">if set to <c>true</c> [suppress cookies].</param>
        /// <param name="UseDBDataOnly">if set to <c>true</c> [use DB data only].</param>
        public Customer(SqlTransaction DBTrans, string email, bool SuppressCookies, bool UseDBDataOnly, bool AdminOnly)
        {
            m_DBTrans = DBTrans;
            m_SuppressCookies = SuppressCookies;
            Init(email, AdminOnly);
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Sets the primary shipping address for shopping cart.
        /// </summary>
        /// <param name="CustomerID">The customer ID.</param>
        /// <param name="OldPrimaryShippingAddressID">The old primary shipping address ID.</param>
        /// <param name="NewPrimaryShippingAddressID">The new primary shipping address ID.</param>
        static public void SetPrimaryShippingAddressForShoppingCart(int CustomerID, int OldPrimaryShippingAddressID, int NewPrimaryShippingAddressID)
        {
            DB.ExecuteSQL("update shoppingcart set ShippingAddressID=" + NewPrimaryShippingAddressID.ToString() + " where ShippingAddressID=" + OldPrimaryShippingAddressID.ToString() + " and CustomerID=" + CustomerID.ToString() + " and CartType in (" + ((int)CartTypeEnum.ShoppingCart).ToString() + "," + ((int)CartTypeEnum.RecurringCart).ToString() + ")");
        }

		/// <summary>
		/// Creates the customer record.
		/// </summary>
		/// <param name="email">The email.</param>
		/// <param name="password">The password.</param>
		/// <param name="skinId">The skin ID.</param>
		/// <param name="affiliateId">The affiliate ID.</param>
		/// <param name="referrer">The referrer.</param>
		/// <param name="isAdmin">The is admin.</param>
		/// <param name="lastIpAddress">The last IP address.</param>
		/// <param name="localeSetting">The locale setting.</param>
		/// <param name="over13Checked">Is over13 checked.</param>
		/// <param name="currencySetting">The currency setting.</param>
		/// <param name="vatSetting">The VAT setting.</param>
		/// <param name="vatRegistrationId">The VAT registration ID.</param>
		/// <param name="customerLevelId">The customer level ID.</param>
		/// <returns></returns>
		static public int CreateCustomerRecord(string email, string password, object skinId, object affiliateId, string referrer, object isAdmin, string lastIpAddress, string localeSetting, object over13Checked, string currencySetting, object vatSetting, string vatRegistrationId, object customerLevelId, int? storeId = null)
		{
			using(var connection = new SqlConnection(DB.GetDBConn()))
			using(var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = "dbo.aspdnsf_insCustomer";

				command.Parameters.AddWithValue("@Email", (object)email ?? DBNull.Value);
				command.Parameters.AddWithValue("@Password", (object)password ?? DBNull.Value);
				command.Parameters.AddWithValue("@SkinID", skinId ?? DBNull.Value);
				command.Parameters.AddWithValue("@AffiliateID", affiliateId ?? DBNull.Value);
				command.Parameters.AddWithValue("@CustomerLevelID", customerLevelId ?? 0);
				command.Parameters.AddWithValue("@Referrer", (object)referrer ?? DBNull.Value);
				command.Parameters.AddWithValue("@IsAdmin", isAdmin ?? DBNull.Value);
				command.Parameters.AddWithValue("@LastIPAddress", (object)lastIpAddress ?? DBNull.Value);
				command.Parameters.AddWithValue("@LocaleSetting", (object)localeSetting ?? DBNull.Value);
				command.Parameters.AddWithValue("@Over13Checked", over13Checked ?? DBNull.Value);
				command.Parameters.AddWithValue("@CurrencySetting", (object)currencySetting ?? DBNull.Value);
				command.Parameters.AddWithValue("@VATSetting", vatSetting ?? DBNull.Value);
				command.Parameters.AddWithValue("@VATRegistrationID", vatRegistrationId ?? string.Empty);
				command.Parameters.AddWithValue("@StoreID", storeId ?? AppLogic.StoreID());

				command.Parameters.Add(
					new SqlParameter("@CustomerID", SqlDbType.Int, 4)
					{ Direction = ParameterDirection.Output });

				try
				{
					connection.Open();
					command.ExecuteNonQuery();
					return (int)command.Parameters["@CustomerID"].Value;
				}
				catch
				{
					return 0;
				}
			}
		}

		public static bool EmailInUse(string email, int customerId, bool excludeUnregisteredUsers)
		{
			if(string.IsNullOrEmpty(email))
				return false;

			var matchingEmailCount = DB.GetSqlN(@"
				select count(*) as N 
				from Customer with(nolock) 
				where 
					EMail = @email 
					and CustomerID != @customerId 
					and (@excludeUnregisteredUsers = 0 or IsRegistered = 1)
					and (@allowCustomerFiltering = 0 or StoreID = @storeId)",
				new SqlParameter("@email", email),
				new SqlParameter("@customerId", customerId),
				new SqlParameter("@excludeUnregisteredUsers", excludeUnregisteredUsers),
				new SqlParameter("@allowCustomerFiltering", AppLogic.GlobalConfigBool("AllowCustomerFiltering")),
				new SqlParameter("@storeId", AppLogic.StoreID()));

			return matchingEmailCount > 0;
		}

		public static bool NewEmailPassesDuplicationRules(string email, int customerId)
		{
			var guestCheckoutType = AppLogic.AppConfig("GuestCheckout");
			var guestCheckoutTypeAllowsDuplicateEmails = new[]
				{
					GuestCheckoutType.AllowRegisteredCustomers.ToString(),
					GuestCheckoutType.PasswordNeverRequestedAtCheckout.ToString(),
				}
				.Contains(guestCheckoutType, StringComparer.OrdinalIgnoreCase);

			if(guestCheckoutTypeAllowsDuplicateEmails)
				return true;

			return !EmailInUse(email, customerId, true);
		}

        /// <summary>
        /// Updates the customer static.
        /// </summary>
        /// <param name="CustomerID">The customerID.</param>
        /// <param name="spa">The sql parameter.</param>
        /// <returns></returns>
        public static string UpdateCustomerStatic(int CustomerID, SqlParameter[] spa)
        {

            string err = String.Empty;
            try
            {
                SqlParameter sp = DB.CreateSQLParameter("@CustomerID", SqlDbType.Int, 4, CustomerID, ParameterDirection.Input);
                spa = DB.CreateSQLParameterArray(spa, sp);

                DB.ExecuteStoredProcInt("dbo.aspdnsf_updCustomer", spa);
            }
            catch (Exception ex)
            {
                err = ex.Message;
            }
            return err;
        }

        /// <summary>
        /// Determines whether [has at least one address] [the specified customer ID].
        /// </summary>
        /// <param name="CustomerID">The customer ID.</param>
        /// <returns>
        /// 	<c>true</c> if [has at least one address] [the specified customer ID]; otherwise, <c>false</c>.
        /// </returns>
        static public bool HasAtLeastOneAddress(int CustomerID)
        {
            return (DB.GetSqlN("select count(AddressID) as N from Address  with (NOLOCK)  where CustomerID=" + CustomerID.ToString()) > 0);
        }

        /// <summary>
        /// Gets the CustomerID from E mail.
        /// </summary>
        /// <param name="EMail">The E mail.</param>
        /// <returns>Returns the CustomerID from the EMail supplied.</returns>
        static public int GetIDFromEMail(String EMail)
        {
            int tmpS = 0;

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("Select CustomerID from customer   with (NOLOCK)  where deleted=0 and EMail=" + DB.SQuote(EMail.ToLowerInvariant()), dbconn))
                {
                    if (rs.Read())
                    {
                        tmpS = DB.RSFieldInt(rs, "CustomerID");
                    }
                    return tmpS;
                }
            }
        }

        /// <summary>
        /// Check if the User is a SuperUser.
        /// </summary>
        /// <param name="CustomerID">The customerID.</param>
        /// <returns>Returns TRUE if the user is a SuperUser otherwise FALSE.</returns>
        static public bool StaticIsAdminSuperUser(int CustomerID)
        {
            if (CustomerID == 0)
            {
                return false;
            }
            bool tmp = false;

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("select IsAdmin from Customer  with (NOLOCK)  where CustomerID=" + CustomerID.ToString(), dbconn))
                {
                    if (rs.Read())
                    {
                        tmp = ((DB.RSFieldTinyInt(rs, "IsAdmin") & 2) != 0);
                    }
                    return tmp;
                }
            }
        }

        /// <summary>
        /// Gets the name of the customer level.
        /// </summary>
        /// <param name="CustomerLevelID">The customerlevelID.</param>
        /// <param name="LocaleSetting">The locale setting.</param>
        /// <returns>Returns the CustomerLevelName</returns>
        static public String GetCustomerLevelName(int CustomerLevelID, String LocaleSetting)
        {
            String tmpS = String.Empty;
            if (CustomerLevelID != 0)
            {
                using (SqlConnection dbconn = DB.dbConn())
                {
                    dbconn.Open();
                    using (IDataReader rs = DB.GetRS("Select Name from CustomerLevel  with (NOLOCK)  where CustomerLevelID=" + CustomerLevelID.ToString(), dbconn))
                    {
                        if (rs.Read())
                        {
                            tmpS = DB.RSFieldByLocale(rs, "Name", LocaleSetting);
                        }
                    }
                }
            }
            return tmpS;
        }

        /// <summary>
        /// Gets the customer primary shipping address ID.
        /// </summary>
        /// <param name="CustomerID">The customer ID.</param>
        /// <returns>Returns the customer primary shipping address ID.</returns>
        static public int GetCustomerPrimaryShippingAddressID(int CustomerID)
        {
            int tmp = 0;
            if (CustomerID != 0)
            {
                using (SqlConnection dbconn = DB.dbConn())
                {
                    dbconn.Open();
                    using (IDataReader rs = DB.GetRS("Select ShippingAddressID from Customer  with (NOLOCK)  where CustomerID=" + CustomerID.ToString(), dbconn))
                    {
                        if (rs.Read())
                        {
                            tmp = DB.RSFieldInt(rs, "ShippingAddressID");
                        }
                    }
                }
            }
            return tmp;
        }

        /// <summary>
        // returns true if this address belongs to this customer
        /// </summary>
        /// <param name="CustomerID">The customerID.</param>
        /// <param name="AddressID">The addressID.</param>
        /// <returns>Returns TRUE if this address belongs to this customer otherwise FALSE.</returns>
        static public bool OwnsThisAddress(int CustomerID, int AddressID)
        {
            return (DB.GetSqlN("select count(*) as N from Address  with (NOLOCK)  where CustomerID=" + CustomerID.ToString() + " and AddressID=" + AddressID.ToString()) > 0);
        }

		/// <summary>
		/// Create an anonymous customer record from profile data.
		/// </summary>
		/// <param name="CustomerID">The created record's CustomerID.</param>
		/// <param name="CustomerGUID">The created record's CustomerGUID.</param>
		public static void MakeAnonCustomerRecord(out int customerId, out string customerGuid)
		{
			var profileKeyPrefix = AppLogic.IsAdminSite
				? "Admin"
				: string.Empty;

			var affiliateProfileValue = HttpContext.Current.Profile.GetPropertyValue(profileKeyPrefix + ro_AffiliateCookieName);
			var affiliateID = CommonLogic.IsInteger(affiliateProfileValue.ToString())
				? int.Parse(affiliateProfileValue.ToString())
				: 0;

			var localeProfileValue = HttpContext.Current.Profile.GetPropertyValue(profileKeyPrefix + ro_LocaleSettingCookieName);
			var localeSetting = Localization.ValidateLocaleSetting(localeProfileValue.ToString());
			if(String.IsNullOrEmpty(localeSetting) || AppLogic.IsAdminSite)
				localeSetting = Localization.GetDefaultLocale();

			var currencyProfileValue = HttpContext.Current.Profile.GetPropertyValue(profileKeyPrefix + ro_CurrencySettingCookieName);
			var currencySetting = Currency.ValidateCurrencySetting(currencyProfileValue.ToString());
			if(currencySetting.Length == 0 || AppLogic.IsAdminSite)
				currencySetting = Localization.GetStoreDisplayCurrency(AppLogic.StoreID());

			var vatSettingProfileValue = HttpContext.Current.Profile.GetPropertyValue(profileKeyPrefix + ro_VATSettingCookieName);
			var vatSetting = CommonLogic.IsInteger(vatSettingProfileValue.ToString())
				? int.Parse(vatSettingProfileValue.ToString())
				: 0;

			if(vatSetting == 0 || AppLogic.IsAdminSite)
				vatSetting = AppLogic.AppConfigUSInt("VAT.DefaultSetting");

			var referrerProfileValue = HttpContext.Current.Profile.GetPropertyValue(ro_ReferrerCookieName);

			customerId = CreateCustomerRecord(
				email: null,
				password: null,
				skinId: null,
				affiliateId: affiliateID,
				referrer: referrerProfileValue.ToString(),
				isAdmin: null,
				lastIpAddress: CommonLogic.CustomerIpAddress(),
				localeSetting: localeSetting,
				over13Checked: null,
				currencySetting: currencySetting,
				vatSetting: vatSetting,
				vatRegistrationId: null,
				customerLevelId: null);

			customerGuid = GetCustomerGuid(customerId);

			AppLogic
				.eventHandler("CreateCustomer")
				.CallEvent(String.Format("&CreateCustomer=true&CreatedCustomerID={0}", customerId));
		}

		/// <summary>
		/// Gets the CustomerGUID for the specified CustomerID.
		/// </summary>
		/// <param name="customerId">The customerID.</param>
		/// <returns>Returns the CustomerGUID for the provided customer ID</returns>
		static string GetCustomerGuid(int customerId)
		{
			using(var connection = new SqlConnection(DB.GetDBConn()))
			using(var command = new SqlCommand())
			{
				command.Connection = connection;
				command.CommandText = "select CustomerGUID from Customer with (NOLOCK) where CustomerID = @customerId";
				command.Parameters.AddWithValue("customerId", customerId);

				connection.Open();
				using(var reader = command.ExecuteReader())
					return reader.Read()
						? DB.RSFieldGUID(reader, "CustomerGUID")
						: string.Empty;
			}
		}

        /// <summary>
        /// Locks the account.
        /// </summary>
        /// <param name="CustomerID">The customerID.</param>
        /// <param name="LockIt">if set to <c>true</c> [lock it].</param>
        public static void LockAccount(int CustomerID, bool LockIt)
        {
            if (LockIt)
            {
                DB.ExecuteSQL("update customer set Active=0 where CustomerID=" + CustomerID.ToString());
            }
            else
            {
                DB.ExecuteSQL("update customer set Active=1 where CustomerID=" + CustomerID.ToString());
            }
        }

        /// <summary>
        /// Validates the VAT setting.
        /// </summary>
        /// <param name="VATSettingID">The VAT setting ID.</param>
        /// <returns>Returns the validated VAT setting.</returns>
        public static String ValidateVATSetting(String VATSettingID)
        {
            if (!AppLogic.AppConfigBool("VAT.AllowCustomerToChooseSetting"))
            {
                return AppLogic.AppConfigUSInt("VAT.DefaultSetting").ToString();
            }
            if (VATSettingID == "1" || VATSettingID == "2")
            {
                return VATSettingID;
            }
            return AppLogic.AppConfigUSInt("VAT.DefaultSetting").ToString();
        }

        /// <summary>
        /// Creates the new anonymous customer object.
        /// </summary>
        /// <returns>Returns the new anonymous customer object.</returns>
        public static Customer CreateNewAnonCustomerObject()
        {
            int CustomerID = 0;
            String CustomerGUID = String.Empty;
            Customer.MakeAnonCustomerRecord(out CustomerID, out CustomerGUID);
            Customer NewCustomer = new Customer(CustomerID, true);
            return NewCustomer;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Initilizes the customer fields.
        /// </summary>
        /// <param name="rs">The sql reader.</param>
        private void Init(IDataReader rs)
        {
            m_CustomerID = 0;
            m_AffiliateID = 0;
            m_LocaleSetting = Localization.GetDefaultLocale();
            m_CurrencySetting = Localization.GetPrimaryCurrency();
            m_VATSetting = (VATSettingEnum)AppLogic.AppConfigUSInt("VAT.DefaultSetting");
            m_VATRegistrationID = String.Empty;
            m_LastIPAddress = CommonLogic.CustomerIpAddress();
            m_CustomerLevelID = 0;
            m_CustomerLevelName = String.Empty;
            m_HasCustomerRecord = false;
            m_Phone = String.Empty;
            m_EMail = String.Empty;
            m_Notes = String.Empty;
            m_OKToEMail = false;
            m_CouponCode = String.Empty;
            m_Password = String.Empty;
            m_SaltKey = Security.ro_SaltKeyIsInvalid;
            m_CreatedOn = System.DateTime.MinValue;
            m_IsRegistered = false;
            m_IsAdminUser = false;
            m_IsAdminSuperUser = false;
            m_FirstName = String.Empty;
            m_LastName = String.Empty;
            m_MicroPayBalance = System.Decimal.Zero;
            m_PrimaryBillingAddressID = 0;
            m_PrimaryShippingAddressID = 0;
            m_CurrentSessionID = 0;
            m_StoreCCInDB = false;
            m_LockedUntil = DateTime.MinValue;
            m_AdminCanViewCC = false;
            m_PwdChanged = DateTime.MinValue;
            m_BadLoginCount = 0;
            m_Active = true;
            m_Roles = String.Empty;
			m_SkinID = AppLogic.GetStoreSkinID(AppLogic.StoreID());

            if (rs != null && rs.Read())
            {
                m_HasCustomerRecord = true;
                m_CustomerID = DB.RSFieldInt(rs, "CustomerID");
                m_AffiliateID = DB.RSFieldInt(rs, "AffiliateID");
                m_CurrencySetting = DB.RSField(rs, "CurrencySetting");
                m_LocaleSetting = DB.RSField(rs, "LocaleSetting");
                m_VATSetting = (VATSettingEnum)DB.RSFieldInt(rs, "VATSetting");
                m_VATRegistrationID = DB.RSField(rs, "VATRegistrationID");
                m_CreatedOn = DB.RSFieldDateTime(rs, "CreatedOn");
                m_Notes = DB.RSField(rs, "Notes");
                m_Phone = DB.RSField(rs, "Phone");
                m_EMail = DB.RSField(rs, "EMail");
                m_Over13 = DB.RSFieldBool(rs, "Over13Checked");
                m_OKToEMail = DB.RSFieldBool(rs, "OKToEMail");
                m_CouponCode = DB.RSField(rs, "CouponCode");
                m_Password = DB.RSField(rs, "Password"); // retreive hashed and salted pwd
                m_SaltKey = DB.RSFieldInt(rs, "SaltKey");
                m_IsRegistered = DB.RSFieldBool(rs, "IsRegistered");
                m_FirstName = DB.RSField(rs, "FirstName");
                m_LastName = DB.RSField(rs, "LastName");
                m_IsAdminUser = DB.RSFieldBool(rs, "IsAdmin");
                m_IsAdminSuperUser = DB.RSFieldBool(rs, "IsSuperAdmin");
                m_CustomerLevelID = DB.RSFieldInt(rs, "CustomerLevelID");
                m_LevelDiscountPct = DB.RSFieldDecimal(rs, "LevelDiscountPercent");
                m_DiscountExtendedPrices = DB.RSFieldBool(rs, "LevelDiscountsApplyToExtendedPrices");
                m_CustomerGUID = DB.RSFieldGUID(rs, "CustomerGUID");
                m_MicroPayBalance = DB.RSFieldDecimal(rs, "MicroPayBalance");
                m_PrimaryBillingAddressID = DB.RSFieldInt(rs, "BillingAddressID");
                m_PrimaryShippingAddressID = DB.RSFieldInt(rs, "ShippingAddressID");
                m_CustomerLevelName = GetCustomerLevelName(m_CustomerLevelID, m_LocaleSetting);
                m_CurrentSessionID = DB.RSFieldInt(rs, "CustomerSessionID");
                m_StoreCCInDB = DB.RSFieldBool(rs, "StoreCCInDB");
                m_LockedUntil = DB.RSFieldDateTime(rs, "LockedUntil");
                m_AdminCanViewCC = DB.RSFieldBool(rs, "AdminCanViewCC");
                m_LastActivity = DB.RSFieldDateTime(rs, "LastActivity");
                m_PwdChanged = DB.RSFieldDateTime(rs, "PwdChanged");
                m_BadLoginCount = DB.RSFieldByte(rs, "BadLoginCount");
                m_Active = DB.RSFieldBool(rs, "Active");
                m_PwdChgRequired = DB.RSFieldBool(rs, "PwdChangeRequired");
                m_RequestedPaymentMethod = DB.RSField(rs, "RequestedPaymentMethod");
                StoreID = DB.RSFieldInt(rs, "StoreID");
                StoreName = DB.RSField(rs, "StoreName");
                m_LastIPAddress = DB.RSField(rs, "LastIPAddress");

                //Get Roles
                if (IsRegistered)
                {
                    m_Roles += "Free";
                }

                // Admins and super users rule!
                if (IsAdminUser)
                {
                    m_Roles += CommonLogic.IIF(m_Roles.Length == 0, "", ",") + "Admin";
                }
                if (IsAdminSuperUser)
                {
                    m_Roles += CommonLogic.IIF(m_Roles.Length == 0, "", ",") + "SuperAdmin";
                }

                if (IsRegistered)
                {
                    m_Roles += CommonLogic.IIF(m_Roles.Length == 0, "", ",") + "Registered";
                }
                else
                {
                    m_Roles += CommonLogic.IIF(m_Roles.Length == 0, "", ",") + "NotRegistered";
                }

                if (AppLogic.IPIsRestricted(LastIPAddress))
                {
                    m_Roles += CommonLogic.IIF(m_Roles.Length == 0, "", ",") + "IPRestricted";
                }

                if (CustomerLevelID != 0)
                {
                    m_Roles += CommonLogic.IIF(m_Roles.Length == 0, "", ",") + CustomerLevelName.Trim();
                }

                // only set dynamic product roles if NOT running via WSI:
                if (m_DBTrans == null)
                {
                    string sql = "select Name Role from CustomerLevel  with (NOLOCK)  where CustomerLevelID=" + this.CustomerLevelID.ToString();
                    sql += " UNION ";
                    sql += "select OrderedProductSKU Role from orders_ShoppingCart os  with (NOLOCK)  join Orders o  with (NOLOCK)  on o.OrderNumber= os.OrderNumber where o.CustomerID=" + this.CustomerID.ToString() + " and TransactionState=" + DB.SQuote(AppLogic.ro_TXStateCaptured);

                    Hashtable ht = new Hashtable();

                    SqlConnection con = null;
                    IDataReader dr = null;
                    try
                    {
                        string query = sql;
                        if (m_DBTrans != null)
                        {
                            // if a transaction was passed, we should use the transaction objects connection
                            dr = DB.GetRS(query, m_DBTrans);
                        }
                        else
                        {
                            // otherwise create it
                            con = new SqlConnection(DB.GetDBConn());
                            con.Open();
                            dr = DB.GetRS(query, con);
                        }

                        using (dr)
                        {
                            while (dr.Read())
                            {
                                string role = DB.RSField(dr, "Role").Trim();
                                if (role != "" && !ht.ContainsKey(role))
                                {
                                    m_Roles += CommonLogic.IIF(m_Roles.Length == 0, "", ",") + role;
                                    ht.Add(role, "true");
                                }
                            }
                        }
                    }
                    catch { throw; }
                    finally
                    {
                        // we can't dispose of the connection if it's part of a transaction
                        if (con != null && m_DBTrans == null)
                        {
                            // here it's safe to dispose since we created the connection ourself
                            con.Dispose();
                        }

                        // make sure we won't reference this again in code
                        dr = null;
                        con = null;
                    }


                }
            }
            
            if (AppLogic.IsAdminSite)
            {
                m_SkinID = 1; // forced!!
            }

            this.EnsurePublishedCurrency(m_CurrencySetting);
            SetDefaultCustomerLevelData();
        }

        private void EnsurePublishedCurrency(string CurrentCurrency)
        {
            ArrayList publishedCurrencies = Currency.getCurrencyList();
            foreach (ListItemClass item in publishedCurrencies)
                if (CurrentCurrency.Equals(Currency.GetCurrencyCode(item.Value), StringComparison.InvariantCultureIgnoreCase))
                    return;

            String DefaultCurrency = AppLogic.GetLocaleDefaultCurrency(this.LocaleSetting);
			
			//Only hit the DB when it makes sense to
			if (!String.IsNullOrEmpty(DefaultCurrency) && DefaultCurrency != CurrentCurrency)
			{
				this.UpdateCustomer(new SqlParameter[] { new SqlParameter("CurrencySetting", DefaultCurrency) });
				m_CurrencySetting = DefaultCurrency;
			}
        }

        private void SetDefaultCustomerLevelData()
        {
            // nal replace select *
            string sql = "SELECT CustomerLevelID, CustomerLevelGUID, Name, LevelDiscountPercent, LevelDiscountAmount, LevelHasFreeShipping, LevelAllowsQuantityDiscounts, LevelHasNoTax, LevelAllowsCoupons, LevelDiscountsApplyToExtendedPrices, LevelAllowsPO, DisplayOrder, ParentCustomerLevelID, SEName, ExtensionData, Deleted, CreatedOn, SkinID, TemplateName, UpdatedOn FROM CustomerLevel where CustomerLevelID = @CustomerLevelID";
            SqlParameter[] spa = { DB.CreateSQLParameter("@CustomerLevelID", SqlDbType.Int, 4, AppLogic.AppConfigUSInt("DefaultCustomerLevelID"), ParameterDirection.Input) };

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader dr = DB.GetRS(sql, spa, dbconn))
                {
                    if (dr.Read())
                    {
                        m_DefaultCustLevel_DiscountExtendedPrices = DB.RSFieldBool(dr, "LevelDiscountsApplyToExtendedPrices");
                        m_DefaultCustLevel_LevelDiscountPct = DB.RSFieldDecimal(dr, "LevelDiscountPercent");
                        m_DefaultCustLevel_CustomerLevelName = DB.RSFieldByLocale(dr, "Name", Localization.GetDefaultLocale());
                    }
                }
            }
        }

        /// <summary>
        /// Initilizes the specified customer ID.
        /// </summary>
        /// <param name="CustomerID">The customer ID.</param>
        private void Init(int CustomerID)
        {
            String sql = "exec aspdnsf_GetCustomerByID " + CustomerID.ToString();

            SqlConnection con = null;
            IDataReader rs = null;
            try
            {
                string query = sql;
                if (m_DBTrans != null)
                {
                    // if a transaction was passed, we should use the transaction objects connection
                    rs = DB.GetRS(query, m_DBTrans);
                }
                else
                {
                    // otherwise create it
                    con = new SqlConnection(DB.GetDBConn());
                    con.Open();
                    rs = DB.GetRS(query, con);
                }

                using (rs)
                {
                    Init(rs);
                }
            }
            catch { throw; }
            finally
            {
                // we can't dispose of the connection if it's part of a transaction
                if (con != null && m_DBTrans == null)
                {
                    // here it's safe to dispose since we created the connection ourself
                    con.Dispose();
                }

                // make sure we won't reference this again in code
                rs = null;
                con = null;
            }
        }

        /// <summary>
        /// Initilizes the Customer by the specified customer GUID.
        /// </summary>
        /// <param name="CustomerGuid">The customer GUID.</param>
        private void Init(Guid CustomerGuid)
        {
            String sql = "exec dbo.aspdnsf_GetCustomerByGUID " + DB.SQuote(CustomerGuid.ToString());

            SqlConnection con = null;
            IDataReader rs = null;
            try
            {
                string query = sql;
                if (m_DBTrans != null)
                {
                    // if a transaction was passed, we should use the transaction objects connection
                    rs = DB.GetRS(query, m_DBTrans);
                }
                else
                {
                    // otherwise create it
                    con = new SqlConnection(DB.GetDBConn());
                    con.Open();
                    rs = DB.GetRS(query, con);
                }

                using (rs)
                {
                    Init(rs);
                }
            }
            catch { throw; }
            finally
            {
                // we can't dispose of the connection if it's part of a transaction
                if (con != null && m_DBTrans == null)
                {
                    // here it's safe to dispose since we created the connection ourself
                    con.Dispose();
                }

                // make sure we won't reference this again in code
                rs = null;
                con = null;
            }


        }

        /// <summary>
        /// Initilizes the Customer by the specified customer GUID.
        /// </summary>
        /// <param name="email">The email.</param>
        private void Init(string email, bool adminOnly)
        {
            String sql = string.Format("exec dbo.aspdnsf_GetCustomerByEmail {0},{1},{2},{3}", DB.SQuote(email), CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowCustomerFiltering") == true, 1, 0), AppLogic.StoreID(), CommonLogic.IIF(adminOnly, 1, 0));

            SqlConnection con = null;
            IDataReader rs = null;
            try
            {
                string query = sql;
                if (m_DBTrans != null)
                {
                    // if a transaction was passed, we should use the transaction objects connection
                    rs = DB.GetRS(query, m_DBTrans);
                }
                else
                {
                    // otherwise create it
                    con = new SqlConnection(DB.GetDBConn());
                    con.Open();
                    rs = DB.GetRS(query, con);
                }

                using (rs)
                {
                    Init(rs);
                }
            }
            catch { throw; }
            finally
            {
                // we can't dispose of the connection if it's part of a transaction
                if (con != null && m_DBTrans == null)
                {
                    // here it's safe to dispose since we created the connection ourself
                    con.Dispose();
                }

                // make sure we won't reference this again in code
                rs = null;
                con = null;
            }


        }


        /// <summary>
        /// Refreshes this instance.
        /// </summary>
        private void refresh()
        {
            String sql = "exec aspdnsf_GetCustomerByID " + m_CustomerID.ToString();

            SqlConnection con = null;
            IDataReader rs = null;
            try
            {
                string query = sql;
                if (m_DBTrans != null)
                {
                    // if a transaction was passed, we should use the transaction objects connection
                    rs = DB.GetRS(query, m_DBTrans);
                }
                else
                {
                    // otherwise create it
                    con = new SqlConnection(DB.GetDBConn());
                    con.Open();
                    rs = DB.GetRS(query, con);
                }

                using (rs)
                {
                    Init(rs);
                }
            }
            catch { throw; }
            finally
            {
                // we can't dispose of the connection if it's part of a transaction
                if (con != null && m_DBTrans == null)
                {
                    // here it's safe to dispose since we created the connection ourself
                    con.Dispose();
                }

                // make sure we won't reference this again in code
                rs = null;
                con = null;
            }


        }

        #endregion


        #region Public Methods

        /// <summary>
        /// Updates the customer.
        /// </summary>
        /// <param name="spa">The sql parameter.</param>
        /// <returns></returns>
        public string UpdateCustomer(SqlParameter[] spa)
        {
            return Customer.UpdateCustomerStatic(m_CustomerID, spa);
        }

		public string UpdateCustomer(
			int? storeId = null,
			int? customerLevelId = null,
			string email = null,
			string saltedAndHashedPassword = null,
			int? saltKey = null,
			string gender = null,
			string firstName = null,
			string lastName = null,
			string notes = null,
			int? skinId = null,
			string phone = null,
			int? affiliateId = null,
			string referrer = null,
			string couponCode = null,
			bool? okToEmail = null,
			int? isAdmin = null,
			bool? billingEqualsShipping = null,
			string lastIpAddress = null,
			string orderNotes = null,
			string rtShipRequest = null,
			string rtShipResponse = null,
			string orderOptions = null,
			string localeSetting = null,
			decimal? microPayBalance = null,
			int? recurringShippingMethodId = null,
			string recurringShippingMethod = null,
			int? billingAddressId = null,
			int? shippingAddressId = null,
			string extensionData = null,
			string finalizationData = null,
			bool? deleted = null,
			bool? over13Checked = null,
			string currencySetting = null,
			int? vatSetting = null,
			string vatRegistrationId = null,
			bool? storeCreditCardInDb = null,
			bool? isRegistered = null,
			DateTime? lockedUntil = null,
			bool? adminCanViewCreditCard = null,
			int? badLogin = null,
			bool? active = null,
			bool? passwordChangeRequired = null,
			string registerDate = null,
			string requestedPaymentMethod = null,
			bool clearSavedCCNumbers = false)
		{
			return UpdateCustomer(
				customerLevelId,
				email,
				saltedAndHashedPassword,
				saltKey,
				gender,
				firstName,
				lastName,
				notes,
				skinId,
				phone,
				affiliateId,
				referrer,
				couponCode,
				okToEmail,
				isAdmin,
				billingEqualsShipping,
				lastIpAddress,
				orderNotes,
				rtShipRequest,
				rtShipResponse,
				orderOptions,
				localeSetting,
				microPayBalance,
				recurringShippingMethodId,
				recurringShippingMethod,
				billingAddressId,
				shippingAddressId,
				extensionData,
				finalizationData,
				deleted,
				over13Checked,
				currencySetting,
				vatSetting,
				vatRegistrationId,
				storeCreditCardInDb,
				isRegistered,
				lockedUntil,
				adminCanViewCreditCard,
				badLogin,
				active,
				passwordChangeRequired,
				registerDate,
				storeId,
				requestedPaymentMethod,
				clearSavedCCNumbers);
		}

		/// <summary>
		/// Updates the customer.
		/// </summary>
		/// <param name="CustomerLevelID">The customer level ID.</param>
		/// <param name="EMail">The E mail.</param>
		/// <param name="SaltedAndHashedPassword">The salted and hashed password.</param>
		/// <param name="SaltKey">The salt key.</param>
		/// <param name="Gender">The gender.</param>
		/// <param name="FirstName">The first name.</param>
		/// <param name="LastName">The last name.</param>
		/// <param name="Notes">The notes.</param>
		/// <param name="SkinID">The skinID.</param>
		/// <param name="Phone">The phone.</param>
		/// <param name="AffiliateID">The affiliate ID.</param>
		/// <param name="Referrer">The referrer.</param>
		/// <param name="CouponCode">The coupon code.</param>
		/// <param name="OkToEmail">Is ok to email.</param>
		/// <param name="IsAdmin">The is admin.</param>
		/// <param name="BillingEqualsShipping">Is billing equals shipping.</param>
		/// <param name="LastIPAddress">The last IP address.</param>
		/// <param name="OrderNotes">The order notes.</param>
		/// <param name="RTShipRequest">The RTship request.</param>
		/// <param name="RTShipResponse">The RTship response.</param>
		/// <param name="OrderOptions">The order options.</param>
		/// <param name="LocaleSetting">The locale setting.</param>
		/// <param name="MicroPayBalance">The micropay balance.</param>
		/// <param name="RecurringShippingMethodID">The recurring shipping method ID.</param>
		/// <param name="RecurringShippingMethod">The recurring shipping method.</param>
		/// <param name="BillingAddressID">The billing address ID.</param>
		/// <param name="ShippingAddressID">The shipping address ID.</param>
		/// <param name="ExtensionData">The extension data.</param>
		/// <param name="FinalizationData">The finalization data.</param>
		/// <param name="Deleted">Is deleted.</param>
		/// <param name="Over13Checked">Is over13 checked.</param>
		/// <param name="CurrencySetting">The currency setting.</param>
		/// <param name="VATSetting">The VAT setting.</param>
		/// <param name="VATRegistrationID">The VAT registration ID.</param>
		/// <param name="StoreCCInDB">Store Credit Card in DataBase.</param>
		/// <param name="IsRegistered">Is registered.</param>
		/// <param name="LockedUntil">Is locked until.</param>
		/// <param name="AdminCanViewCC">The admin can view CC.</param>
		/// <param name="BadLogin">Only pass value -1, 0, 1:  -1 clears bad login count, zero does nothing, 1 increments the count</param>
		/// <param name="Active">The active.</param>
		/// <param name="PwdChangeRequired">The password change required.</param>
		/// <param name="RegisterDate">The register date.</param>
		/// <param name="RequestedPaymentMethod">The currently selected payment method.</param>
		/// <param name="ClearSavedCCNumbers">If true, stored credit card numbers will be erased.</param>
		/// <returns></returns>
		public string UpdateCustomer(
			object CustomerLevelID,
			string EMail,
			string SaltedAndHashedPassword,
			object SaltKey,
			string Gender,
			string FirstName,
			string LastName,
			string Notes,
			object SkinID,
			string Phone,
			object AffiliateID,
			string Referrer,
			string CouponCode,
			object OkToEmail,
			object IsAdmin,
			object BillingEqualsShipping,
			string LastIPAddress,
			string OrderNotes,
			string RTShipRequest,
			string RTShipResponse,
			string OrderOptions,
			string LocaleSetting,
			object MicroPayBalance,
			object RecurringShippingMethodID,
			string RecurringShippingMethod,
			object BillingAddressID,
			object ShippingAddressID,
			string ExtensionData,
			string FinalizationData,
			object Deleted,
			object Over13Checked,
			string CurrencySetting,
			object VATSetting,
			string VATRegistrationID,
			object StoreCCInDB,
			object IsRegistered,
			object LockedUntil,
			object AdminCanViewCC,
			object BadLogin,
			object Active,
			object PwdChangeRequired,
			object RegisterDate,
			object StoreId,
			string RequestedPaymentMethod = null,
			bool ClearSavedCCNumbers = false)
		{
			AppLogic
				.eventHandler("UpdateCustomer")
				.CallEvent("&UpdateCustomer=true&UpdatedCustomerID=" + CustomerID.ToString());

			if(SaltKey != null && (int)SaltKey == Security.ro_SaltKeyIsInvalid)
				SaltKey = null;

			if(BadLogin != null && (Convert.ToInt16(BadLogin) < -1 || Convert.ToInt16(BadLogin) > 1))
				BadLogin = null;

			if(Active != null && (Convert.ToInt16(Active) != 0 && Convert.ToInt16(Active) != 1))
				Active = null;

			using(var connection = new SqlConnection(DB.GetDBConn()))
			using(var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = "dbo.aspdnsf_updCustomer";

				command.Parameters.AddWithValue("@CustomerID", m_CustomerID);
				command.Parameters.AddWithValue("@RegisterDate", RegisterDate ?? DBNull.Value);
				command.Parameters.AddWithValue("@CustomerLevelID", CustomerLevelID ?? DBNull.Value);
				command.Parameters.AddWithValue("@Email", (object)EMail ?? DBNull.Value);
				command.Parameters.AddWithValue("@Password", (object)SaltedAndHashedPassword ?? DBNull.Value);
				command.Parameters.AddWithValue("@SaltKey", SaltKey ?? DBNull.Value);
				command.Parameters.AddWithValue("@Gender", (object)Gender ?? DBNull.Value);
				command.Parameters.AddWithValue("@FirstName", (object)FirstName ?? DBNull.Value);
				command.Parameters.AddWithValue("@LastName", (object)LastName ?? DBNull.Value);
				command.Parameters.AddWithValue("@Notes", (object)Notes ?? DBNull.Value);
				command.Parameters.AddWithValue("@SkinID", SkinID ?? DBNull.Value);
				command.Parameters.AddWithValue("@Phone", (object)Phone ?? DBNull.Value);
				command.Parameters.AddWithValue("@AffiliateID", AffiliateID ?? DBNull.Value);
				command.Parameters.AddWithValue("@Referrer", (object)Referrer ?? DBNull.Value);
				command.Parameters.AddWithValue("@CouponCode", (object)CouponCode ?? DBNull.Value);
				command.Parameters.AddWithValue("@OkToEmail", OkToEmail ?? DBNull.Value);
				command.Parameters.AddWithValue("@IsAdmin", IsAdmin ?? DBNull.Value);
				command.Parameters.AddWithValue("@BillingEqualsShipping", BillingEqualsShipping ?? DBNull.Value);
				command.Parameters.AddWithValue("@LastIPAddress", (object)LastIPAddress ?? DBNull.Value);
				command.Parameters.AddWithValue("@OrderNotes", (object)OrderNotes ?? DBNull.Value);
				command.Parameters.AddWithValue("@RTShipRequest", (object)RTShipRequest ?? DBNull.Value);
				command.Parameters.AddWithValue("@RTShipResponse", (object)RTShipResponse ?? DBNull.Value);
				command.Parameters.AddWithValue("@OrderOptions", (object)OrderOptions ?? DBNull.Value);
				command.Parameters.AddWithValue("@LocaleSetting", (object)LocaleSetting ?? DBNull.Value);
				command.Parameters.AddWithValue("@MicroPayBalance", MicroPayBalance ?? DBNull.Value);
				command.Parameters.AddWithValue("@RecurringShippingMethodID", RecurringShippingMethodID ?? DBNull.Value);
				command.Parameters.AddWithValue("@RecurringShippingMethod", (object)RecurringShippingMethod ?? DBNull.Value);
				command.Parameters.AddWithValue("@BillingAddressID", BillingAddressID ?? DBNull.Value);
				command.Parameters.AddWithValue("@ShippingAddressID", ShippingAddressID ?? DBNull.Value);
				command.Parameters.AddWithValue("@ExtensionData", (object)ExtensionData ?? DBNull.Value);
				command.Parameters.AddWithValue("@FinalizationData", (object)FinalizationData ?? DBNull.Value);
				command.Parameters.AddWithValue("@Deleted", Deleted ?? DBNull.Value);
				command.Parameters.AddWithValue("@Over13Checked", Over13Checked ?? DBNull.Value);
				command.Parameters.AddWithValue("@CurrencySetting", (object)CurrencySetting ?? DBNull.Value);
				command.Parameters.AddWithValue("@VATSetting", VATSetting ?? DBNull.Value);
				command.Parameters.AddWithValue("@VATRegistrationID", (object)VATRegistrationID ?? DBNull.Value);
				command.Parameters.AddWithValue("@StoreCCInDB", StoreCCInDB ?? DBNull.Value);
				command.Parameters.AddWithValue("@IsRegistered", IsRegistered ?? DBNull.Value);
				command.Parameters.AddWithValue("@LockedUntil", LockedUntil ?? DBNull.Value);
				command.Parameters.AddWithValue("@AdminCanViewCC", AdminCanViewCC ?? DBNull.Value);
				command.Parameters.AddWithValue("@BadLogin", BadLogin ?? DBNull.Value);
				command.Parameters.AddWithValue("@Active", Active ?? DBNull.Value);
				command.Parameters.AddWithValue("@PwdChangeRequired", PwdChangeRequired ?? DBNull.Value);
				command.Parameters.AddWithValue("@StoreID", (object)StoreId ?? DBNull.Value);
				command.Parameters.AddWithValue("@RequestedPaymentMethod", (object)RequestedPaymentMethod ?? DBNull.Value);
				command.Parameters.AddWithValue("@ClearSavedCCNumbers", ClearSavedCCNumbers);

				connection.Open();
				try
				{
					command.ExecuteNonQuery();
					refresh();
					return string.Empty;
				}
				catch(Exception exception)
				{
					return exception.Message;
				}
			}
		}

        /// <summary>
        /// Check if this customer owns the address
        /// </summary>
        /// <param name="AddressID">The address ID.</param>
        /// <returns>Returns TRUE if this customer owns the address otherwise FALSE.</returns>
        public bool OwnsThisAddress(int AddressID)
        {
            return Customer.OwnsThisAddress(CustomerID, AddressID);
        }

        /// <summary>
        /// Gets the Customer's Full Name.
        /// </summary>
        /// <returns>Returns teh Customer's Full Name.</returns>
        public String FullName()
        {
            return (m_FirstName + " " + m_LastName).Trim();
        }

        /// <summary>
        /// Determines whether [has at least one address].
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [has at least one address]; otherwise, <c>false</c>.
        /// </returns>
        public bool HasAtLeastOneAddress()
        {
            return Customer.HasAtLeastOneAddress(m_CustomerID);
        }

        /// <summary>
        /// Sets the VAT registration ID.
        /// </summary>
        /// <param name="VATRegistrationID">The VAT registration ID.</param>
        public void SetVATRegistrationID(String VATRegistrationID)
        {
            if (m_HasCustomerRecord)
            {
                DB.ExecuteSQL("update customer set VATRegistrationID=" + DB.SQuote(VATRegistrationID) + " where CustomerID=" + m_CustomerID.ToString());
            }
            m_VATRegistrationID = VATRegistrationID;
        }

        /// <summary>
        /// Checks the password to make sure it doesn't match any of the last x passwords used where x is the value in the AppConfig NumPreviouslyUsedPwds
        /// </summary>
        /// <param name="NewClearTextPassword"></param>
        /// <returns>True is the password has been used, false otherwise</returns>
        public bool PwdPreviouslyUsed(string NewClearTextPassword)
        {
            bool pwdused = false;

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader dr = DB.GetRS("select top " + AppLogic.AppConfigNativeInt("NumPreviouslyUsedPwds").ToString() + " OldPwd, SaltKey from dbo.passwordlog where customerid = " + CustomerID.ToString() + " order by ChangeDt desc", dbconn))
                {
                    while (dr.Read())
                    {
                        Password p = new Password(NewClearTextPassword, DB.RSFieldInt(dr, "SaltKey"));
                        if (p.SaltedPassword == DB.RSField(dr, "OldPwd"))
                        {
                            pwdused = true;
                        }
                    }
                    return pwdused;
                }
            }
        }

        /// <summary>
        /// Validates the password and returns true if it matches the current password
        /// </summary>
        /// <param name="TestPwd"></param>
        /// <returns></returns>
        public bool CheckLogin(string TestPwd)
        {
            Password pwd = new Password(TestPwd, m_SaltKey);
            if (m_Password == pwd.SaltedPassword)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Clears out the customer session and clears all customer cookies
        /// </summary>
        public void Logout()
        {
            ThisCustomerSession.Clear();
            ClearAllCustomerProfile();
        }

		public void EndAnonymousSession()
		{
			//Nuke all the cookies
			for(int i = 0; i < HttpContext.Current.Request.Cookies.Count; i++)
			{
				String cookie = HttpContext.Current.Request.Cookies.Keys[i];
				HttpContext.Current.Response.Cookies[cookie].Expires = System.DateTime.Now.AddDays(-1);
			}

			//Clear CustomerSession table
			ThisCustomerSession.Clear();

			//Nuke Profile records
			DB.ExecuteSQL(String.Format("DELETE FROM Profile WHERE CustomerID = {0}", CustomerID));
		}

        /// <summary>
        /// Removes all cookies created by the store
        /// </summary>
        static public void ClearAllCustomerProfile()
        {
            if (HttpContext.Current.Profile != null)
            {
                HttpContext.Current.Profile.SetPropertyValue("SiteDisclaimerAccepted", String.Empty);
            }          

            String[] ClearCookieNames = {"StatsView","ViewStatsSelectedIndex","SelectedChartsView","CompareStatsBy","ChartType", 
                                          "YearCompareSelectedYear1","YearCompareSelectedYear2","MonthCompareSelectedYear1","MonthCompareSelectedYear2", 
                                          "MonthCompareSelectedMonth1","MonthCompareSelectedMonth2","WeekCompareSelectedYear1","WeekCompareSelectedYear2", 
                                          "WeekCompareSelectedMonth1","WeekCompareSelectedMonth2","WeekCompareSelectedWeek1","WeekCompareSelectedWeek2", 
                                          "CategoryFilterID","SectionFilterID","ManufacturerFilterID","DistributorFilterID","GenreFilterID","VectorFilterID", 
                                          "Master","SkinID","Toolbars","AffiliateID","VATSettingID","LocaleSetting","CurrencySetting","LastViewedEntityName", 
                                          "LastViewedEntityInstanceID","LastViewedEntityInstanceName","LATAffiliateID","SiteDisclaimerAccepted", 
                                          "AdminVATSettingID","AdminLocaleSetting","AdminCurrencySetting","Referrer" }; 
            foreach (String s in ClearCookieNames)
            {
                if (HttpContext.Current.Profile.GetPropertyValue(s).ToString() != string.Empty)
                {
                    HttpContext.Current.Profile.SetPropertyValue(s, string.Empty);
                }
            }
        }

        /// <summary>
        /// Returns the tax rate for the specified tax class for the Customer's curent shipping address, if the TaxCalcMode AppConfig is set to "billing" the rate is for the billing address
        /// </summary>
        /// <param name="TaxClassID">The item tax class ID</param>
        /// <returns></returns>
        public Decimal TaxRate(int TaxClassID)
        {
            if ("billing".Equals(AppLogic.AppConfig("TaxCalcMode"), StringComparison.InvariantCultureIgnoreCase))
            {
                return TaxRate(PrimaryBillingAddress, TaxClassID);
            }
            else
            {
                return TaxRate(PrimaryShippingAddress, TaxClassID);
            }
        }

        /// <summary>
        /// Returns the tax rate for the specified tax class and address
        /// </summary>
        /// <param name="useAddress">The customer address to calculate taxes for</param>
        /// <param name="TaxClassID">The item tax class id</param>
        /// <returns></returns>
        public Decimal TaxRate(Address useAddress, int TaxClassID)
        {
            if (LevelHasNoTax || IsVatExempt())
                return 0;
            else
                return Prices.TaxRate(useAddress, TaxClassID, this);
        }

        /// <summary>
        /// Clears any failed transactions in the failed transactions table linked to the provided customer id
        /// </summary>
        public static void ClearFailedTransactions(int CustomerID)
        {
            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();
                DB.ExecuteSQL("exec [dbo].[aspdnsf_delFailedTransactionsByCustomer] @CustomerID=" + DB.SQuote(CustomerID.ToString()), conn);
            }
        }

        public void SetPrimaryAddress(int addressID, AddressTypes addrType)
        {
            if (!OwnsThisAddress(addressID))
            {
                return;
            }
            else
            {
                string addressField = String.Empty;

                if (addrType == AddressTypes.Billing)
                {
                    addressField = "BillingAddressID";
                    m_PrimaryBillingAddressID = addressID;
                    m_PrimaryBillingAddress = new Address(addressID);
                }
                else
                {
                    addressField = "ShippingAddressID";
                    m_PrimaryShippingAddressID = addressID;
                    m_PrimaryShippingAddress = new Address(addressID);
                }

                string sql = String.Format("UPDATE Customer SET {0} = {1} WHERE CustomerID = {2}", addressField, addressID.ToString(), m_CustomerID.ToString());
    
                using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
                {
                    conn.Open();
                    DB.ExecuteSQL(sql, conn);
                }
            }
        }

		public Owns Owns
		{
			get { return new Owns(m_CustomerID); }
		}
        #endregion


        #region Public Properties
        // uses customer's current currency setting to control output display format:
        // exchange rate WILL be applied on input amt
        // input amt is assumed to be in the store's PRIMARY CURRENCY!
        public String CurrencyString(decimal amt)
        {
            return Localization.CurrencyStringForDisplayWithExchangeRate(amt, CurrencySetting);
        }

        /// <summary>
        /// Gets the customerID.
        /// </summary>
        /// <value>The customerID.</value>
        public int CustomerID
        {
            get
            {
                return m_CustomerID;
            }
        }

        /// <summary>
        /// Gets the date the data was created on.
        /// </summary>
        /// <value>The date the data was created on.</value>
        public DateTime CreatedOn
        {
            get
            {
                return m_CreatedOn;
            }
        }

        /// <summary>
        /// Gets or sets the VAT setting RAW.
        /// </summary>
        /// <value>The VAT setting RAW.</value>
        public VATSettingEnum VATSettingRAW
        {
            get
            {
                VATSettingEnum vat = (VATSettingEnum)AppLogic.AppConfigUSInt("VAT.DefaultSetting");

                if (m_HasCustomerRecord)
                {
                    return m_VATSetting;
                }
                else if (HttpContext.Current.Profile != null)
                {
                    if (HttpContext.Current.Profile.GetPropertyValue(ro_VATSettingCookieName).ToString() != "")
                    {
                        string value = HttpContext.Current.Profile.GetPropertyValue(ro_VATSettingCookieName).ToString();
                        int iVal = 0;
                        if (int.TryParse(value, out iVal))
                        {
                            vat = (VATSettingEnum)iVal;
                        }
                    }
                }


                return vat;
            }
            set
            {
                if (m_HasCustomerRecord && m_VATSetting != value)
                {
                    SqlParameter sp1 = DB.CreateSQLParameter("@VATSetting", SqlDbType.Int, 4, value, ParameterDirection.Input);
                    SqlParameter[] spa = { sp1 };
                    string retval = this.UpdateCustomer(spa);
                    if (retval == string.Empty)
                    {
                        m_VATSetting = value;
                    }
                }
                if (!AppLogic.IsAdminSite)
                {
                    // the call could have been made on the BeginRequest and specified at the querystring
                    // at that point, the Profile is null.
                    // The only way to use a querystring to set this profile's property is 
                    // to go through the VAT controller
                    // if the call is coming from the BeginRequest, we shouldn't set this property yet
                    // and let the code in the VAT controller set the current customer's property
                    // at that point on the page_load of the page, the Profile is already instantiated
                    if (HttpContext.Current.Profile != null)
                    {
                        HttpContext.Current.Profile.SetPropertyValue(Customer.ro_VATSettingCookieName, ((int)value).ToString());
                    }
                }
            }
        }

        /// <summary>
        /// this is the one to USE for the customer, after we have checked all store appconfigs, and default values
        /// the VATSettingRAW is the customer's raw setting (what is in their db record!)
        /// this property gives the actual VATSetting value to use when displaying any data.
        /// </summary>
        /// <value>The VAT setting reconciled.</value>
        public VATSettingEnum VATSettingReconciled
        {
            get
            {
                VATSettingEnum xvat = (VATSettingEnum)AppLogic.AppConfigUSInt("VAT.DefaultSetting");
                if (AppLogic.AppConfigBool("VAT.AllowCustomerToChooseSetting"))
                {
                    xvat = VATSettingRAW;
                }
                return xvat;
            }
        }



        /// <summary>
        /// Gets the VAT registration ID.
        /// </summary>
        /// <value>The VAT registration ID.</value>
        public String VATRegistrationID
        {
            get
            {
                return m_VATRegistrationID;
            }
        }

        /// <summary>
        /// Gets or sets the primary billing address ID.
        /// </summary>
        /// <value>The primary billing address ID.</value>
        public int PrimaryBillingAddressID
        {
            get
            {
                return m_PrimaryBillingAddressID;
            }
            set
            {
                m_PrimaryBillingAddressID = value;
                m_PrimaryBillingAddress = new Address(m_CustomerID, AddressTypes.Billing);
                m_PrimaryBillingAddress.LoadFromDB(m_PrimaryBillingAddressID);
            }
        }

        /// <summary>
        /// Gets or sets the primary shipping address ID.
        /// </summary>
        /// <value>The primary shipping address ID.</value>
        public int PrimaryShippingAddressID
        {
            get
            {
                return m_PrimaryShippingAddressID;
            }

            set
            {
                m_PrimaryShippingAddressID = value;
                m_PrimaryShippingAddress = new Address(m_CustomerID, AddressTypes.Shipping);
                m_PrimaryShippingAddress.LoadFromDB(m_PrimaryShippingAddressID);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is over13.
        /// </summary>
        /// <value><c>true</c> if this instance is over13; otherwise, <c>false</c>.</value>
        public bool IsOver13
        {
            get { return m_Over13; }
        }

        public int SkinID
        {
            get
            {
                return m_SkinID;
            }
            set
            {
                m_SkinID = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is admin user.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is admin user; otherwise, <c>false</c>.
        /// </value>
        public bool IsAdminUser
        {
            get
            {
                return m_IsAdminUser;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is admin super user.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is admin super user; otherwise, <c>false</c>.
        /// </value>
        public bool IsAdminSuperUser
        {
            get
            {
                return m_IsAdminSuperUser;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [OK to E mail].
        /// </summary>
        /// <value><c>true</c> if [OK to E mail]; otherwise, <c>false</c>.</value>
        public bool OKToEMail
        {
            get
            {
                return m_OKToEMail;
            }
            set
            {
                m_OKToEMail = value;
            }
        }

        /// <summary>
        /// Gets or sets the coupon code.
        /// </summary>
        /// <value>The coupon code.</value>
        public String CouponCode
        {
            get
            {
                return m_CouponCode;
            }
            set
            {
                m_CouponCode = value;
            }
        }

        /// <summary>
        /// Gets the last IP address.
        /// </summary>
        /// <value>The last IP address.</value>
        public String LastIPAddress
        {
            get
            {
                return m_LastIPAddress;
            }
        }

        /// <summary>
        /// Gets or sets the phone.
        /// </summary>
        /// <value>The phone.</value>
        public String Phone
        {
            get
            {
                return m_Phone;
            }
            set
            {
                m_Phone = value;
            }
        }

        /// <summary>
        /// Gets or sets the affiliate ID.
        /// </summary>
        /// <value>The affiliate ID.</value>
        public int AffiliateID
        {
            get
            {
                if (m_HasCustomerRecord || AppLogic.IsAdminSite)
                {
                    return m_AffiliateID;
                }
                else if (HttpContext.Current.Profile != null)
                {
                    if (HttpContext.Current.Profile.GetPropertyValue(ro_AffiliateCookieName).ToString() != "") 
                          { 
                          return int.Parse(HttpContext.Current.Profile.GetPropertyValue(ro_AffiliateCookieName).ToString()); 
                          } 
                          else 
                          { 
                          return 0; 
                          }          
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                if (m_HasCustomerRecord && m_AffiliateID != value)
                {
                    SqlParameter sp1 = DB.CreateSQLParameter("@AffiliateID", SqlDbType.Int, 4, value, ParameterDirection.Input);
                    SqlParameter[] spa = { sp1 };
                    string retval = this.UpdateCustomer(spa);
                    if (retval == string.Empty)
                    {
                        m_AffiliateID = value;
                    }
                }
                if (!AppLogic.IsAdminSite)
                {
                    if (HttpContext.Current.Profile != null)
                    {
                        HttpContext.Current.Profile.SetPropertyValue(Customer.ro_AffiliateCookieName, value.ToString());                    
                    }
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is registered.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is registered; otherwise, <c>false</c>.
        /// </value>
        public bool IsRegistered
        {
            get
            {
                return m_IsRegistered;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has customer record.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has customer record; otherwise, <c>false</c>.
        /// </value>
        public bool HasCustomerRecord
        {
            get
            {
                return m_HasCustomerRecord;
            }
        }

        private int m_storeid;
        private string m_storename;

        public int StoreID
        {
            get { return m_storeid; }
            set { m_storeid = value; }
        }

        public string StoreName
        {
            get { return m_storename; }
            set { m_storename = value; }
        }

        // returns true if the customer has any active recurring billing orders:
        /// Determines whether [has active recurring orders] [the specified count only those without subscription I ds].
        /// </summary>
        /// <param name="CountOnlyThoseWithoutSubscriptionIDs">if set to <c>true</c> [count only those without subscription I ds].</param>
        /// <returns>
        /// 	<c>true</c> if [has active recurring orders] [the specified count only those without subscription I ds]; otherwise, <c>false</c>.
        /// </returns>
        public bool HasActiveRecurringOrders(bool CountOnlyThoseWithoutSubscriptionIDs)
        {
            if (CountOnlyThoseWithoutSubscriptionIDs)
            {
                return (DB.GetSqlN("select count(*) as N from ShoppingCart  with (NOLOCK)  where RecurringSubscriptionID='' and CartType=" + ((int)CartTypeEnum.RecurringCart).ToString() + " and CustomerID=" + CustomerID.ToString()) > 0);
            }
            else
            {
                return (DB.GetSqlN("select count(*) as N from ShoppingCart  with (NOLOCK)  where CartType=" + ((int)CartTypeEnum.RecurringCart).ToString() + " and CustomerID=" + CustomerID.ToString()) > 0);
            }
        }



        /// <summary>
        /// Inserts all products that has been viewed
        /// </summary>
        /// <param name="ProductID">The product id of the recently viewed product</param>
        public void LogProductView(int productID)
        {
            if (m_IsRegistered ||                 
                HttpContext.Current.Items["OriginalSessionID"] != null)
            {
                string viewingCustomerID = "";
                if (m_IsRegistered)
                {
                    viewingCustomerID = CustomerID.ToString();
                }
                else
                {
                    viewingCustomerID = HttpContext.Current.Items["OriginalSessionID"].ToString();
                }

                if (productID != 0)
                {
                    using (SqlConnection cn = new SqlConnection(DB.GetDBConn()))
                    {
                        cn.Open();
                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.Connection = cn;
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandText = "dbo.aspdnsf_insProductView";


                            SqlParameter paramCustomerGuid = new SqlParameter("@CustomerViewID", SqlDbType.NVarChar);
                            SqlParameter paramProductId = new SqlParameter("@ProductID", SqlDbType.Int);
                            SqlParameter paramViewDate = new SqlParameter("@ViewDate", SqlDbType.DateTime);

                            paramCustomerGuid.Value = viewingCustomerID;
                            paramProductId.Value = productID;
                            paramViewDate.Value = DateTime.Now;

                            cmd.Parameters.Add(paramCustomerGuid);
                            cmd.Parameters.Add(paramProductId);
                            cmd.Parameters.Add(paramViewDate);

                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Replaces the product view of the unknown customer
        /// </summary>               
        public void ReplaceProductViewFromAnonymous()
        {
            if (HttpContext.Current.Items["OriginalSessionID"] != null)
            {                
                DB.ExecuteSQL(string.Format("aspdnsf_updProductView {0}, {1}", DB.SQuote(HttpContext.Current.Items["OriginalSessionID"].ToString()), DB.SQuote(this.CustomerID.ToString())));
            }
        }

        /// <summary>
        /// Gets the salt key.
        /// </summary>
        /// <value>The salt key.</value>
        public int SaltKey
        {
            get { return m_SaltKey; }
        }

        /// <summary>
        /// Gets the name of the customer level.
        /// </summary>
        /// <value>The name of the customer level.</value>
        public String CustomerLevelName
        {
            get
            {
                if ((!HasCustomerRecord || m_CustomerLevelID == 0) && !AppLogic.IsAdminSite && AppLogic.AppConfigUSInt("DefaultCustomerLevelID") > 0)
                {
                    if (m_DefaultCustLevel_CustomerLevelName == null)
                        SetDefaultCustomerLevelData();
                    if (m_DefaultCustLevel_CustomerLevelName == null)
                        return m_CustomerLevelName;
                    return m_DefaultCustLevel_CustomerLevelName;
                }
                else
                {
                    return m_CustomerLevelName;
                }
            }
        }

        /// <summary>
        /// Gets or sets the E mail.
        /// </summary>
        /// <value>The E mail.</value>
        public String EMail
        {
            get
            {
                return m_EMail;
            }
            set
            {
                m_EMail = value;
            }
        }

        /// <summary>
        /// Gets the customer GUID.
        /// </summary>
        /// <value>The customer GUID.</value>
        public String CustomerGUID
        {
            get
            {
                return m_CustomerGUID;
            }
        }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        /// <value>The first name.</value>
        public String FirstName
        {
            get
            {
                return m_FirstName;
            }
            set
            {
                m_FirstName = value;
            }
        }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        /// <value>The last name.</value>
        public String LastName
        {
            get
            {
                return m_LastName;
            }
            set
            {
                m_LastName = value;
            }
        }

        /// <summary>
        /// Gets the customer level ID.
        /// </summary>
        /// <value>The customer level ID.</value>
        public int CustomerLevelID
        {
            get
            {
                //JH - broader default customer level ID support for Multi-Store - default customer level will not set the customer level on that customer, just use it for the session.
                if ((!HasCustomerRecord || m_CustomerLevelID == 0) && !AppLogic.IsAdminSite && AppLogic.AppConfigUSInt("DefaultCustomerLevelID") > 0)
                {
                    return m_DefaultCustLevel_CustomerLevelID;
                }
                else
                {
                    return m_CustomerLevelID;
                }
            }
        }

        /// <summary>
        /// Gets or sets the locale setting.
        /// </summary>
        /// <value>The locale setting.</value>
        public String LocaleSetting
        {
            get
            {
                if (AppLogic.IsAdminSite)
                {
                    return Localization.GetDefaultLocale();
                }

                if (m_HasCustomerRecord && m_LocaleSetting.Trim() != "")
                {
                    return m_LocaleSetting;
                }
                else if (HttpContext.Current.Profile != null)
                {
                    if (HttpContext.Current.Profile.GetPropertyValue(ro_LocaleSettingCookieName).ToString() != "") 
                      { 
                      return HttpContext.Current.Profile.GetPropertyValue(ro_LocaleSettingCookieName).ToString(); 
                      } 
                      else 
                      { 
                      return Localization.GetDefaultLocale(); 
                      } 
                }
                else
                {
                    return Localization.GetDefaultLocale();
                }
            }
            set
            {
				if(value != "" && value != null && value != m_LocaleSetting)
                {
					if(m_HasCustomerRecord)
                    {
                        SqlParameter sp1 = DB.CreateSQLParameter("@LocaleSetting", SqlDbType.NVarChar, 10, value, ParameterDirection.Input);
                        SqlParameter[] spa = { sp1 };
                        string retval = this.UpdateCustomer(spa);
                        if (retval == string.Empty)
                        {
                            m_LocaleSetting = value;
                        }
                    }
                    if (!AppLogic.IsAdminSite)
                    {
						// the call could have been made on the BeginRequest and specified at the querystring
						// at that point, the Profile is null.
						// The only way to use a querystring to set this profile's property is 
						// to go through the Locale controller
						// if the call is coming from the BeginRequest, we shouldn't set this property yet
						// and let the code in the Locale controller set the current customer's property
						// at that point on the page_load of the page, the Profile is already instantiated
						if(HttpContext.Current.Profile != null)
                        {
                            HttpContext.Current.Profile.SetPropertyValue(Customer.ro_LocaleSettingCookieName, value);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the notes.
        /// </summary>
        /// <value>The notes.</value>
        public String Notes
        {
            get
            {
                return m_Notes;
            }
        }

        /// <summary>
        /// Gets or sets the currency setting.
        /// </summary>
        /// <value>The currency setting.</value>
        public String CurrencySetting
        {
            get
            {
                if (AppLogic.IsAdminSite)
                {
                    return Localization.GetPrimaryCurrency();
                }

                if (m_HasCustomerRecord && m_CurrencySetting.Trim() != "")
                {
                    return m_CurrencySetting;
                }
                else if (HttpContext.Current.Profile != null)
                {
                    if (HttpContext.Current.Profile.GetPropertyValue(ro_CurrencySettingCookieName).ToString() != "") 
                      { 
                      return HttpContext.Current.Profile.GetPropertyValue(ro_CurrencySettingCookieName).ToString(); 
                      } 
                      else 
                      { 
                      return Localization.GetPrimaryCurrency(false); 
                      } 
                }
                else
                {
                    return Localization.GetPrimaryCurrency(false);
                }
            }
            set
            {
                if (value != "" && value != null && value != m_CurrencySetting)
                {
                    if (m_HasCustomerRecord)
                    {
                        SqlParameter sp1 = DB.CreateSQLParameter("@CurrencySetting", SqlDbType.NVarChar, 10, value, ParameterDirection.Input);
                        SqlParameter[] spa = { sp1 };
                        string retval = this.UpdateCustomer(spa);
                        if (retval == string.Empty)
                        {
                            m_CurrencySetting = value;
                        }
                    }
                    if (!AppLogic.IsAdminSite)
                    {
						// the call could have been made on the BeginRequest and specified at the querystring
						// at that point, the Profile is null.
						// The only way to use a querystring to set this profile's property is 
						// to go through the Currency controller
						// if the call is coming from the BeginRequest, we shouldn't set this property yet
						// and let the code in the Currency controller set the current customer's property
						// at that point on the page_load of the page, the Profile is already instantiated
						if(HttpContext.Current.Profile != null)
                        {
                            HttpContext.Current.Profile.SetPropertyValue(Customer.ro_CurrencySettingCookieName, value);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the micropay balance.
        /// </summary>
        /// <value>The micropay balance.</value>
        public decimal MicroPayBalance
        {
            get
            {
                return m_MicroPayBalance;
            }
        }

        /// <summary>
        /// Gets a value indicating whether [discount extended prices].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [discount extended prices]; otherwise, <c>false</c>.
        /// </value>
        public bool DiscountExtendedPrices
        {
            get
            {
                if ((!HasCustomerRecord || m_CustomerLevelID == 0) && !AppLogic.IsAdminSite && AppLogic.AppConfigUSInt("DefaultCustomerLevelID") > 0)
                {
                    return m_DefaultCustLevel_DiscountExtendedPrices;
                }
                else
                {
                    return m_DiscountExtendedPrices;
                }
            }
        }

        /// <summary>
        /// Gets the level discount percentage.
        /// </summary>
        /// <value>The level discount percentage.</value>
        public decimal LevelDiscountPct
        {
            get
            {
                if ((!HasCustomerRecord || m_CustomerLevelID == 0) && !AppLogic.IsAdminSite && AppLogic.AppConfigUSInt("DefaultCustomerLevelID") > 0)
                {
                    return m_DefaultCustLevel_LevelDiscountPct;
                }
                else
                {
                    return m_LevelDiscountPct;
                }
            }
        }

        /// <summary>
        /// Gets the roles.
        /// </summary>
        /// <value>The roles.</value>
        public string Roles
        {
            get { return m_Roles; }
        }

        // low level flag set by customer preference.
        // this is NOT the final determination of whether CC info should be stored for this customer.
        /// <summary>
        /// Gets a value indicating whether [store CC in DB].
        /// </summary>
        /// <value><c>true</c> if [store CC in DB]; otherwise, <c>false</c>.</value>
        public bool StoreCCInDB
        {
            get { return m_StoreCCInDB; }
        }

        // this method is the MASTER routine which should be used to determine if CC info is being stored for a customer.
        // this method takes into account all store appconfig settings, and recurring billing considerations
        /// <summary>
        /// Gets a value indicating whether [master should we store credit card info].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [master should we store credit card info]; otherwise, <c>false</c>.
        /// </value>
        public bool MasterShouldWeStoreCreditCardInfo
        {
            get
            {
                bool SaveCC = false;
                bool HasRecurring = HasActiveRecurringOrders(true);
                bool UseGatewayInternalBilling = AppLogic.AppConfigBool("Recurring.UseGatewayInternalBilling");
                if (!HasRecurring)
                {
                    if (AppLogic.StoreCCInDB())
                    {
                        SaveCC = m_StoreCCInDB; //Use the customer's preference
                    }
                    else
                    {
                        SaveCC = false;  //Don't store the card number, period
                    }
                }
                else
                {
                    //Customer has recurring orders, so lets see if we need the card numbet or not
                    if (!UseGatewayInternalBilling) //We aren't using the gateway's recurring order feature so the card number MUST be stored
                    {
                        SaveCC = true;
                    }
                    else if (UseGatewayInternalBilling && AppLogic.StoreCCInDB()) //Card number is not required, but admin allows it to be stored
                    {
                        SaveCC = m_StoreCCInDB; //Use the customer's settings here
                    }
                    else //WCard number is not required and the admin does NOT allow storing cards
                    {
                        SaveCC = false; //Don't store the card number, period
                    }
                }
                return SaveCC;
            }
        }

        /// <summary>
        /// Gets the datetime of lockeduntil.
        /// </summary>
        /// <value>The locked until.</value>
        public DateTime LockedUntil
        {
            get { return m_LockedUntil; }
        }

        /// <summary>
        /// Gets a value indicating whether [admin can view CC].
        /// </summary>
        /// <value><c>true</c> if [admin can view CC]; otherwise, <c>false</c>.</value>
        public bool AdminCanViewCC
        {
            get { return m_AdminCanViewCC; }
        }

        /// <summary>
        /// Gets the current session ID.
        /// </summary>
        /// <value>The current session ID.</value>
        public int CurrentSessionID
        {
            get { return m_CurrentSessionID; }
        }

        /// <summary>
        /// Gets the last activity date.
        /// </summary>
        /// <value>The last activity date.</value>
        public DateTime LastActivity
        {
            get { return m_LastActivity; }
        }

        /// <summary>
        /// Gets the password changed date.
        /// </summary>
        /// <value>The password changed date.</value>
        public DateTime PwdChanged
        {
            get { return m_PwdChanged; }
        }

        /// <summary>
        /// Gets the bad login count.
        /// </summary>
        /// <value>The bad login count.</value>
        public byte BadLoginCount
        {
            get { return m_BadLoginCount; }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Customer"/> is active.
        /// </summary>
        /// <value><c>true</c> if active; otherwise, <c>false</c>.</value>
        public bool Active
        {
            get { return m_Active; }
        }

        /// <summary>
        /// Gets a value indicating whether [PWD change required].
        /// </summary>
        /// <value><c>true</c> if [PWD change required]; otherwise, <c>false</c>.</value>
        public bool PwdChangeRequired
        {
            get { return m_PwdChgRequired; }
        }

        /// <summary>
        /// Gets the this customer session.
        /// </summary>
        /// <value>The this customer session.</value>
        public CustomerSession ThisCustomerSession
        {
            get
            {
                if (!HasCustomerRecord)
                {
                    m_CustomerSession = new CustomerSession();
                }
                else if (m_CurrentSessionID == -1)
                {
                    m_CustomerSession = CustomerSession.CreateCustomerSession(m_CustomerID, "", "", m_LastIPAddress);
                    m_CurrentSessionID = m_CustomerSession.SessionID;
                }
                else
                {
                    if (m_CustomerSession == null)
                    {
                        m_CustomerSession = new CustomerSession(m_CurrentSessionID, false);
                        m_CurrentSessionID = m_CustomerSession.SessionID;
                    }
                }
                return m_CustomerSession;
            }
        }

        /// <summary>
        /// Gets the primary billing address.
        /// </summary>
        /// <value>The primary billing address.</value>
        public Address PrimaryBillingAddress
        {
            get
            {
                if (m_PrimaryBillingAddress == null)
                {
                    m_PrimaryBillingAddress = new Address(m_CustomerID, AddressTypes.Billing);
                    m_PrimaryBillingAddress.LoadFromDB(m_PrimaryBillingAddressID);
                }
                return m_PrimaryBillingAddress;
            }
        }

        /// <summary>
        /// Gets the primary shipping address.
        /// </summary>
        /// <value>The primary shipping address.</value>
        public Address PrimaryShippingAddress
        {
            get
            {
                if (m_PrimaryShippingAddress == null)
                {
                    m_PrimaryShippingAddress = new Address(m_CustomerID, AddressTypes.Shipping);
                    m_PrimaryShippingAddress.LoadFromDB(m_PrimaryShippingAddressID);
                }
                return m_PrimaryShippingAddress;
            }
        }

        /// <summary>
        /// Gets the requested payment method.
        /// </summary>
        /// <value>The requested payment method.</value>
        public string RequestedPaymentMethod
        {
            get { return m_RequestedPaymentMethod; }
        }

        /// <summary>
        /// Returns the number of failed transactions this customer has attempted to process
        /// </summary>
        public int FailedTransactionCount
        {
			get
			{
				if (m_FailedTransactionCount == -1)
				{
					m_FailedTransactionCount = DB.GetSqlN(String.Format("[dbo].[aspdnsf_getFailedTransactionCount] {0}", m_CustomerID));
				}
				return m_FailedTransactionCount;
			}
        }

        #endregion
		
        #region IIdentity Members
        /// <summary>
        /// Gets a value that indicates whether the user has been authenticated.
        /// </summary>
        /// <value></value>
        /// <returns>true if the user was authenticated; otherwise, false.
        /// </returns>
        [XmlIgnore]
        public bool IsAuthenticated
        {
            get
            {
                return ((this.CustomerGUID != null) && (!this.CustomerGUID.Equals("")));
            }
        }

        /// <summary>
        /// Gets the name of the current user.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The name of the user on whose behalf the code is running.
        /// </returns>
        [XmlIgnore]
        public string Name
        {
            get
            {
                return this.CustomerGUID;
            }
        }

        /// <summary>
        /// Gets the type of authentication used.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The type of authentication used to identify the user.
        /// </returns>
        [XmlIgnore]
        public string AuthenticationType
        {
            get
            {
                return "Forms";
            }
        }
        #endregion

        /// <summary>
        /// Gets the current customer.
        /// </summary>
        /// <value>The current customer.</value>
        public static Customer Current
        {
            get
            {
				return HttpContext.Current.GetCustomer();
            }
        }

        public bool LevelHasNoTax
        {
            get { return AppLogic.CustomerLevelHasNoTax(CustomerLevelID); }
        }

        internal Boolean IsVatExempt()
        {
            if (!AppLogic.AppConfigBool("VAT.Enabled"))
                return false;

            return !String.IsNullOrEmpty(this.VATRegistrationID);
        }
    }

	public class Owns
	{
		int customerId;

		public Owns(int customerId)
		{
			this.customerId = customerId;
		}

		public bool RecurringOrder(int recurringOrderId)
		{
			return (DB.GetSqlN(
				"SELECT COUNT(*) N FROM Orders(NOLOCK) WHERE OrderNumber=@OrderNumber AND CustomerId=@CustomerId",
				new SqlParameter[]
				{
					new SqlParameter("@OrderNumber", recurringOrderId),
					new SqlParameter("@CustomerId", customerId)
				}
				) > 0);
		}

		public bool ShoppingCartRecord(int shoppingCartRecordId)
		{
			return (DB.GetSqlN(
				"SELECT COUNT(*) N FROM ShoppingCart(NOLOCK) WHERE ShoppingCartRecId=@ShoppingCartRecId AND CustomerId=@CustomerId",
				new SqlParameter[]
				{
					new SqlParameter("@ShoppingCartRecId", shoppingCartRecordId),
					new SqlParameter("@CustomerId", customerId)
				}
				) > 0);
		}

		public bool Wallet(long paymentProfileId)
		{
			return (DB.GetSqlN(
				"SELECT COUNT(*) N FROM CIM_AddressPaymentProfileMap(NOLOCK) WHERE AuthorizeNetProfileId = @AuthorizeNetProfileId AND CustomerId = @CustomerId",
				new SqlParameter[]
				{
					new SqlParameter("@AuthorizeNetProfileId", paymentProfileId),
					new SqlParameter("@CustomerId", customerId)
				}
				) > 0);
		}
	}
}
