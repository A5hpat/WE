// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using System.Web;
using System.Xml;

namespace AspDotNetStorefrontCore
{
	[FlagsAttribute]
    public enum AddressTypes : int
    {
        Unknown = 0,
        Billing = 1,
        Shipping = 2,
        Account = 4
    }

    public enum ResidenceTypes : int
    {
        Unknown = 0,
        Residential = 1,
        Commercial = 2
    }

    /// <summary>
    /// Summary description for Address.
    /// </summary>
    public class Address
    {
        private int m_SkinID = 1; // caller must set this if required to be non "1"
        private String m_LocaleSetting = Thread.CurrentThread.CurrentUICulture.Name;
        private int m_CustomerID = 0;
        private int m_AddressID = 0;
        private string m_AddressGuid;

        private AddressTypes m_AddressType = AddressTypes.Unknown;
        private ResidenceTypes m_ResidenceType = ResidenceTypes.Unknown;

        private String m_NickName = String.Empty;
        private String m_FirstName = String.Empty;
        private String m_LastName = String.Empty;
        private String m_Company = String.Empty;
        private String m_Address1 = String.Empty;
        private String m_Address2 = String.Empty;
        private String m_Suite = String.Empty;
        private String m_City = String.Empty;
        private String m_State = String.Empty;
        private String m_Zip = String.Empty;
        private String m_Country = String.Empty;
        private String m_Phone = String.Empty;
        private String m_Fax = String.Empty;
        private String m_Url = String.Empty;
        private String m_EMail = String.Empty;
        private int m_CountryID = 0;
        private int m_StateID = 0;

        private String m_PaymentMethodLastUsed = String.Empty;
        private String m_CardType = String.Empty;
        private String m_CardNumber = String.Empty;
        private String m_CardName = String.Empty;
        private String m_CardExpirationMonth = String.Empty;
        private String m_CardExpirationYear = String.Empty;
        private String m_CardStartDate = String.Empty; // used in UK/EU
        private String m_CardIssueNumber = String.Empty; // used in UK/EU

        private String m_PONumber = String.Empty;

        private DateTime m_ShippingDate = DateTime.MinValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="Address"/> class.
        /// </summary>
        /// <param name="CustomerID">The customer ID.</param>
        /// <param name="AddressType">Type of the address.</param>
        public Address(int CustomerID = 0, AddressTypes AddressType = AddressTypes.Unknown)
        {
            m_CustomerID = CustomerID;
            m_AddressType = AddressType;
        }

        /// <summary>
        /// Gets or sets the skin ID.
        /// </summary>
        /// <value>The skin ID.</value>
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
        /// Gets or sets the locale setting.
        /// </summary>
        /// <value>The locale setting.</value>
        public String LocaleSetting
        {
            get
            {
                return m_LocaleSetting;
            }
            set
            {
                m_LocaleSetting = value;
            }
        }

        /// <summary>
        /// Gets or sets the customer ID.
        /// </summary>
        /// <value>The customer ID.</value>
        public int CustomerID
        {
            get
            {
                return m_CustomerID;
            }
            set
            {
                m_CustomerID = value;
            }
        }

        /// <summary>
        /// Gets or sets the address ID.
        /// </summary>
        /// <value>The address ID.</value>
        public int AddressID
        {
            get
            {
                return m_AddressID;
            }
            set
            {
                m_AddressID = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the nick.
        /// </summary>
        /// <value>The name of the nick.</value>
        public String NickName
        {
            get
            {
                return m_NickName;
            }
            set
            {
                m_NickName = value.Trim();
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
                m_FirstName = value.Trim();
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
                m_LastName = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the company.
        /// </summary>
        /// <value>The company.</value>
        public String Company
        {
            get
            {
                return m_Company;
            }
            set
            {
                m_Company = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the first address.
        /// </summary>
        /// <value>The first address.</value>
        public String Address1
        {
            get
            {
                return m_Address1;
            }
            set
            {
                m_Address1 = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the second address.
        /// </summary>
        /// <value>The second address.</value>
        public String Address2
        {
            get
            {
                return m_Address2;
            }
            set
            {
                m_Address2 = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the suite.
        /// </summary>
        /// <value>The suite.</value>
        public String Suite
        {
            get
            {
                return m_Suite;
            }
            set
            {
                m_Suite = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        /// <value>The city.</value>
        public String City
        {
            get
            {
                return m_City;
            }
            set
            {
                m_City = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>The state.</value>
        public String State
        {
            get
            {
                return m_State;
            }
            set
            {
                m_State = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the zip.
        /// </summary>
        /// <value>The zip.</value>
        public String Zip
        {
            get
            {
                return m_Zip;
            }
            set
            {
                m_Zip = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the country.
        /// </summary>
        /// <value>The country.</value>
        public String Country
        {
            get
            {
                return m_Country;
            }
            set
            {
                m_Country = value.Trim();
                if (m_Country.Length == 2 && AppLogic.GetCountryIDFromTwoLetterISOCode(m_Country) > 0)
                    m_CountryID = AppLogic.GetCountryIDFromTwoLetterISOCode(m_Country);
                else if (m_Country.Length == 3 && AppLogic.GetCountryIDFromThreeLetterISOCode(m_Country) > 0)
                    m_CountryID = AppLogic.GetCountryIDFromThreeLetterISOCode(m_Country);
            }
        }

        /// <summary>
        /// Gets the country ID.
        /// </summary>
        /// <value>The country ID.</value>
        public int CountryID
        {
            get { return m_CountryID; }
        }

        /// <summary>
        /// Gets the state ID.
        /// </summary>
        /// <value>The state ID.</value>
        public int StateID
        {
            get { return m_StateID; }
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
                m_Phone = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the fax.
        /// </summary>
        /// <value>The Fax.</value>
        public String Fax
        {
            get
            {
                return m_Fax;
            }
            set
            {
                m_Fax = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the Url.
        /// </summary>
        /// <value>The phone.</value>
        public String Url
        {
            get
            {
                return m_Url;
            }
            set
            {
                m_Url = value.Trim();
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
                m_EMail = value.Trim();
            }
        }

		public string OffsiteSource
		{ get; set; }

		/// <summary>
		/// Gets or sets the type of the address.
		/// </summary>
		/// <value>The type of the address.</value>
		public AddressTypes AddressType
        {
            get
            {
                return (m_AddressType);
            }
            set
            {
                m_AddressType = value;
            }
        }

        /// <summary>
        /// Gets or sets the type of the residence.
        /// </summary>
        /// <value>The type of the residence.</value>
        public ResidenceTypes ResidenceType
        {
            get
            {
                return (m_ResidenceType);
            }
            set
            {
                m_ResidenceType = value;
            }
        }

        /// <summary>
        /// Gets or sets the payment method last used.
        /// </summary>
        /// <value>The payment method last used.</value>
        public String PaymentMethodLastUsed
        {
            get
            {
                return m_PaymentMethodLastUsed;
            }
            set
            {
                m_PaymentMethodLastUsed = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the type of the card.
        /// </summary>
        /// <value>The type of the card.</value>
        public String CardType
        {
            get
            {
                return m_CardType;
            }
            set
            {
                m_CardType = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the name of the card.
        /// </summary>
        /// <value>The name of the card.</value>
        public String CardName
        {
            get
            {
                if (m_CardName.Length == 0)
                {
                    m_CardName = String.Format("{0} {1}", m_FirstName, m_LastName).Trim();
                }
                return m_CardName;
            }
            set
            {
                m_CardName = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the card number.
        /// </summary>
        /// <value>The card number.</value>
        public String CardNumber
        {
            get
            {
                return Security.UnmungeString(m_CardNumber, this.GetSaltKey());
            }
            set
            {
                m_CardNumber = Security.MungeString(value.Trim(), this.GetSaltKey());
            }
        }

        /// <summary>
        /// Gets or sets the card expiration month.
        /// </summary>
        /// <value>The card expiration month.</value>
        public String CardExpirationMonth
        {
            get
            {
                return m_CardExpirationMonth;
            }
            set
            {
                m_CardExpirationMonth = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the card expiration year.
        /// </summary>
        /// <value>The card expiration year.</value>
        public String CardExpirationYear
        {
            get
            {
                return m_CardExpirationYear;
            }
            set
            {
                m_CardExpirationYear = value.Trim();
            }
        }

        // must be in format MMYY
        /// <summary>
        /// Gets or sets the card start date.
        /// </summary>
        /// <value>The card start date.</value>
        public String CardStartDate
        {
            get
            {
                return m_CardStartDate;
            }
            set
            {
                m_CardStartDate = value.Trim().Replace(" ", "").Replace("/", "").Replace("\\", "");
            }
        }

        /// <summary>
        /// Gets or sets the card issue number.
        /// </summary>
        /// <value>The card issue number.</value>
        public String CardIssueNumber
        {
            get
            {
                return m_CardIssueNumber;
            }
            set
            {
                m_CardIssueNumber = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the PO number.
        /// </summary>
        /// <value>The PO number.</value>
        public String PONumber
        {
            get
            {
                return m_PONumber;
            }
            set
            {
                m_PONumber = (value ?? string.Empty).Trim();
            }
        }

        public String DisplayPaymentMethodInfo(Customer ViewingCustomer, String PaymentMethod)
        {
            String PMCleaned = AppLogic.CleanPaymentMethod(PaymentMethod);
            if (PMCleaned == AppLogic.ro_PMMicropay)
            {
                return String.Format("MicroPay - {0}", ViewingCustomer.CurrencyString(AppLogic.GetMicroPayBalance(CustomerID)));
            }
            if (PMCleaned == AppLogic.ro_PMCreditCard)
            {
                return String.Format("{0} - {1}: {2} {3}/{4}", "Credit Card", m_CardType, AppLogic.SafeDisplayCardNumber(CardNumber, "Address", m_AddressID), CardExpirationMonth, CardExpirationYear);
            }
            return String.Empty;
        }

        /// <summary>
        /// Clears the Credit Card info.
        /// </summary>
        public void ClearCCInfo()
        {
            CardNumber = "1111111111111111";
            CardType = "111111111111";
            CardExpirationMonth = "11";
            CardExpirationYear = "1111";
            CardName = "111111111111111111111";
            CardStartDate = "1111111";
            CardIssueNumber = "11111111";

            CardNumber = String.Empty;
            CardType = String.Empty;
            CardExpirationMonth = String.Empty;
            CardExpirationYear = String.Empty;
            CardName = String.Empty;
            CardStartDate = String.Empty;
            CardIssueNumber = String.Empty;
        }

        /// <summary>
        /// Gets or sets as XML.
        /// </summary>
        /// <value>As XML.</value>
        public string AsXml
        {
            get
            {
                XmlDocument doc = new XmlDocument();
                XmlElement root = doc.CreateElement("Detail");
                doc.AppendChild(root);
                XmlElement address = doc.CreateElement("Address");
                root.AppendChild(address);

                XmlElement node = doc.CreateElement("AddressID");
                node.InnerText = m_AddressID.ToString();
                address.AppendChild(node);

                node = doc.CreateElement("NickName");
                node.InnerText = m_NickName;
                address.AppendChild(node);

                node = doc.CreateElement("FirstName");
                node.InnerText = m_FirstName;
                address.AppendChild(node);

                node = doc.CreateElement("LastName");
                node.InnerText = m_LastName;
                address.AppendChild(node);

                node = doc.CreateElement("Company");
                node.InnerText = m_Company;
                address.AppendChild(node);

                node = doc.CreateElement("ResidenceType");
                node.InnerText = ((int)m_ResidenceType).ToString();
                address.AppendChild(node);

                node = doc.CreateElement("Address1");
                node.InnerText = m_Address1;
                address.AppendChild(node);

                node = doc.CreateElement("Address2");
                node.InnerText = m_Address2;
                address.AppendChild(node);

                node = doc.CreateElement("Suite");
                node.InnerText = m_Suite;
                address.AppendChild(node);

                node = doc.CreateElement("City");
                node.InnerText = m_City;
                address.AppendChild(node);

                node = doc.CreateElement("State");
                node.InnerText = m_State;
                address.AppendChild(node);

                node = doc.CreateElement("Zip");
                node.InnerText = m_Zip;
                address.AppendChild(node);

                node = doc.CreateElement("Country");
                node.InnerText = m_Country;
                address.AppendChild(node);

                node = doc.CreateElement("Phone");
                node.InnerText = m_Phone;
                address.AppendChild(node);

                node = doc.CreateElement("EMail");
                node.InnerText = m_EMail;
                address.AppendChild(node);

                XmlElement shipping = doc.CreateElement("Shipping");
                root.AppendChild(shipping);

                node = doc.CreateElement("CustomerID");
                node.InnerText = CustomerID.ToString();
                shipping.AppendChild(node);

                node = doc.CreateElement("Date");
                node.InnerText = m_ShippingDate.ToString("s"); //(ISO 8601 sortable)
                shipping.AppendChild(node);

                return doc.OuterXml;
            }
            set
            {
                Clear();

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(value);

                XmlNode node = doc.SelectSingleNode("//AddressID");
                if (node != null)
                {
                    AddressID = Int32.Parse(node.InnerText);
                }

                node = doc.SelectSingleNode("//NickName");
                if (node != null)
                {
                    NickName = node.InnerText;
                }

                node = doc.SelectSingleNode("//FirstName");
                if (node != null)
                {
                    FirstName = node.InnerText;
                }

                node = doc.SelectSingleNode("//LastName");
                if (node != null)
                {
                    LastName = node.InnerText;
                }

                node = doc.SelectSingleNode("//Company");
                if (node != null)
                {
                    Company = node.InnerText;
                }

                node = doc.SelectSingleNode("//Address1");
                if (node != null)
                {
                    Address1 = node.InnerText;
                }

                node = doc.SelectSingleNode("//Address2");
                if (node != null)
                {
                    Address2 = node.InnerText;
                }

                node = doc.SelectSingleNode("//Suite");
                if (node != null)
                {
                    Suite = node.InnerText;
                }

                node = doc.SelectSingleNode("//City");
                if (node != null)
                {
                    City = node.InnerText;
                }

                node = doc.SelectSingleNode("//State");
                if (node != null)
                {
                    State = node.InnerText;
                }

                node = doc.SelectSingleNode("//Zip");
                if (node != null)
                {
                    Zip = node.InnerText;
                }

                node = doc.SelectSingleNode("//Country");
                if (node != null)
                {
                    Country = node.InnerText;
                }

                node = doc.SelectSingleNode("//Phone");
                if (node != null)
                {
                    Phone = node.InnerText;
                }

                node = doc.SelectSingleNode("//EMail");
                if (node != null)
                {
                    EMail = node.InnerText;
                }

                node = doc.SelectSingleNode("//CustomerID");
                if (node != null)
                {
                    CustomerID = Int32.Parse(node.InnerText);
                }

            }
        }

        private String GetSaltKey()
        {
            String KY = AppLogic.AppConfig("AddressCCSaltField").Trim();
            String tmp = String.Empty;
            if (KY.Equals("ADDRESSID", StringComparison.InvariantCultureIgnoreCase))
            {
                tmp = m_AddressID.ToString();
            }
            else if (KY.Equals("CUSTOMERID", StringComparison.InvariantCultureIgnoreCase))
            {
                tmp = m_CustomerID.ToString();
            }
            else if (KY.Equals("AddressGUID", StringComparison.InvariantCultureIgnoreCase))
            {
                tmp = m_AddressGuid;
            }
            return tmp;
        }

        /// <summary>
        /// Creates an array of Address sql parameters that can be used by String.Format to build SQL statements.
        /// </summary>
        /// <returns>object[]</returns>
		SqlParameter[] BuildAddressParameters()
		{
			return new[]
				{
					new SqlParameter("addressId", m_AddressID),
					new SqlParameter("customerId", CustomerID),
					new SqlParameter("nickName", NickName),
					new SqlParameter("firstName", m_FirstName),
					new SqlParameter("lastName", m_LastName),
					new SqlParameter("company", m_Company),
					new SqlParameter("address1", m_Address1),
					new SqlParameter("address2", m_Address2),
					new SqlParameter("suite", m_Suite),
					new SqlParameter("city", m_City),
					new SqlParameter("state", m_State),
					new SqlParameter("zip", m_Zip),
					new SqlParameter("country", m_Country),
					new SqlParameter("phone", m_Phone),
					new SqlParameter("paymentMethodLastUsed", PaymentMethodLastUsed),
					new SqlParameter("cardType", CardType),
					new SqlParameter("cardNumber", !string.IsNullOrEmpty(CardNumber) 
						&& CardNumber != AppLogic.ro_CCNotStoredString
						? Security.MungeString(CardNumber, GetSaltKey())
						: string.Empty),
					new SqlParameter("cardName", CardName),
					new SqlParameter("cardExpirationMonth", CardExpirationMonth),
					new SqlParameter("cardExpirationYear", CardExpirationYear),
					new SqlParameter("cardStartDate", CardStartDate),
					new SqlParameter("cardIssueNumber", Security.MungeString(CardIssueNumber)),
					new SqlParameter("poNumber", PONumber),
					new SqlParameter("email", EMail),
					new SqlParameter("residenceType", ((int)m_ResidenceType).ToString()),
					new SqlParameter("offsiteSource", OffsiteSource??string.Empty)
				};
		}

		public void Clear()
		{
			m_CustomerID =
				m_AddressID = 0;

			m_AddressType = AddressTypes.Unknown;
			m_ResidenceType = ResidenceTypes.Unknown;

			m_NickName =
				m_FirstName =
				m_LastName =
				m_Company =
				m_Address1 =
				m_Address2 =
				m_Suite =
				m_City =
				m_State =
				m_Zip =
				m_Country =
				m_Phone =
				m_Fax =
				m_Url =
				m_PaymentMethodLastUsed =
				m_CardType =
				m_CardNumber =
				m_CardName =
				m_CardExpirationMonth =
				m_CardExpirationYear =
				m_CardStartDate =
				m_CardIssueNumber =
				m_PONumber =
				m_EMail =
				OffsiteSource = string.Empty;
		}

        /// <summary>
        /// Adds an address to the Address Table associated with a passed CustomerID
        /// </summary>
        public void InsertDB(int aCustomerID)
        {
            CustomerID = aCustomerID;
            InsertDB();
        }

        /// <summary>
        /// Adds an address to the Address Table
        /// </summary>
        public void InsertDB()
        {
            var AddressGUID = CommonLogic.GetNewGUID();
            var sql = String.Format("insert into Address(AddressGUID,CustomerID) values({0},{1})", DB.SQuote(AddressGUID), CustomerID);
            DB.ExecuteSQL(sql);

            using (var dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (var rs = DB.GetRS(String.Format("select AddressID from Address with (NOLOCK) where AddressGUID={0}", DB.SQuote(AddressGUID)), dbconn))
                {
                    if (rs.Read())
                    {
                        m_AddressID = DB.RSFieldInt(rs, "AddressID");
                    }
                }
            }
            UpdateDB();
        }

		/// <summary>
		/// Updates the address on Address Table.
		/// </summary>
		public void UpdateDB()
		{
			DB.ExecuteSQL(
				sql: @"update Address set 
					CustomerID = @customerId,
					NickName = @nickName,
					FirstName = @firstName,
					LastName = @lastName,
					Company = @company,
					Address1 = @address1,
					Address2 = @address2,
					Suite = @suite,
					City = @city,
					State = @state,
					Zip = @zip,
					Country = @country,
					Phone = @phone,
					PaymentMethodLastUsed = @paymentMethodLastUsed,
					CardType = @cardType,
					CardNumber = @cardNumber,
					CardName = @cardName,
					CardExpirationMonth = @cardExpirationMonth,
					CardExpirationYear = @cardExpirationYear,
					CardStartDate = @cardStartDate,
					CardIssueNumber = @cardIssueNumber,
					PONumber = @poNumber,
					EMail = @email,
					ResidenceType = @residenceType,
					OffsiteSource = @offsiteSource 
					where AddressID = @addressId",
				parameters: BuildAddressParameters());
		}

        /// <summary>
        /// Deletes an address from the Address Table
        /// </summary>
        static public void DeleteFromDB(int DeleteAddressID, int CustomerID)
        {
            if (DeleteAddressID != 0)
            {

                if (CustomerID == 0)
                {
                    var addr = new Address();
                    addr.LoadFromDB(DeleteAddressID);
                    CustomerID = addr.CustomerID;
                }

                var thisCustomer = new Customer(CustomerID);

                // PABP required
                var sql = String.Format("update Address Set CardNumber=" + DB.SQuote("1111111111111111") + " where AddressID={0}", DeleteAddressID.ToString());
                DB.ExecuteSQL(sql);
                sql = string.Format("update Address Set CardNumber=NULL where AddressID={0}", DeleteAddressID.ToString());
                DB.ExecuteSQL(sql);
                // end PABP mod

                // Delete any CIM mappings
                sql = string.Format("delete from CIM_AddressPaymentProfileMap where AddressID={0}", DeleteAddressID.ToString());
                DB.ExecuteSQL(sql);

                sql = string.Format("delete from Address where AddressID={0}", DeleteAddressID.ToString());
                DB.ExecuteSQL(sql);

                var AlternateBillingAddressID = 0;
                // try to find ANY other customer address (that has credit card info) to use in place of the one being deleted, if required:

                using (var dbconn = DB.dbConn())
                {
                    dbconn.Open();
                    using (var rs = DB.GetRS(String.Format("select top 1 AddressID from Address with (NOLOCK) where CardNumber IS NOT NULL and CustomerID={0}", thisCustomer.CustomerID.ToString()), dbconn))
                    {
                        if (rs.Read())
                        {
                            AlternateBillingAddressID = DB.RSFieldInt(rs, "AddressID");
                        }
                    }
                }

                int AlternateShippingAddressID = 0;

                // try to find ANY other customer address (that does not have credit card info) to use in place of the one being deleted, if required:
                using (var dbconn = DB.dbConn())
                {
                    dbconn.Open();
                    using (var rs = DB.GetRS(String.Format("select top 1 AddressID from Address with (NOLOCK) where CardNumber IS NULL and CustomerID={0}", thisCustomer.CustomerID.ToString()), dbconn))
                    {
                        if (rs.Read())
                        {
                            AlternateShippingAddressID = DB.RSFieldInt(rs, "AddressID");
                        }
                    }
                }

                int BackupAddressID = 0;
                // try to find ANY other customer address as further backup, if required:
                using (var dbconn = DB.dbConn())
                {
                    dbconn.Open();
                    using (var rs = DB.GetRS(String.Format("select top 1 AddressID from Address with (NOLOCK) where CustomerID={0}", thisCustomer.CustomerID.ToString()), dbconn))
                    {
                        if (rs.Read())
                        {
                            BackupAddressID = DB.RSFieldInt(rs, "AddressID");
                        }
                    }
                }

                if (AlternateBillingAddressID == 0)
                {
                    AlternateBillingAddressID = BackupAddressID;
                }

                if (AlternateShippingAddressID == 0)
                {
                    AlternateShippingAddressID = BackupAddressID;
                }

                // now try to prevent invalid conditions
                if (thisCustomer.PrimaryBillingAddressID == DeleteAddressID)
                {
                    DB.ExecuteSQL(String.Format("update Customer set BillingAddressID={0} where CustomerID={1}", AlternateBillingAddressID.ToString(), thisCustomer.CustomerID.ToString()));
                }
                if (thisCustomer.PrimaryShippingAddressID == DeleteAddressID)
                {
                    DB.ExecuteSQL(String.Format("update Customer set ShippingAddressID={0} where CustomerID={1}", AlternateShippingAddressID.ToString(), thisCustomer.CustomerID.ToString()));
                }

                // update any cart shipping addresses (all types, regular cart, wish list and recurring cart) that match the one being deleted:
                sql = String.Format("update ShoppingCart set ShippingAddressID={0} where ShippingAddressID={1}", AlternateShippingAddressID.ToString(), DeleteAddressID.ToString());
                DB.ExecuteSQL(sql);
            }
        }

		/// <summary>
		/// Makes the customers primary address.
		/// </summary>
		/// <param name="addressType">Type of a address.</param>
		public void MakeCustomersPrimaryAddress(AddressTypes addressType)
		{
			//An address could be both Type Shipping and Billing save both to Customer if so.
			var currentPrimaryShippingAddressID = 0;

			var fieldUpdates = new List<string>();
			if((addressType & AddressTypes.Billing) != 0)
				fieldUpdates.Add("BillingAddressID = @addressId");

			if((addressType & AddressTypes.Shipping) != 0)
			{
				currentPrimaryShippingAddressID = Customer.GetCustomerPrimaryShippingAddressID(CustomerID);
				fieldUpdates.Add("ShippingAddressID = @addressId");
			}

			DB.ExecuteSQL(
				string.Format("update Customer set {0} where CustomerID = @customerId", string.Join(", ", fieldUpdates)),
				new SqlParameter("customerId", CustomerID),
				new SqlParameter("addressId", m_AddressID));

			if((addressType & AddressTypes.Shipping) != 0)
				Customer.SetPrimaryShippingAddressForShoppingCart(CustomerID, currentPrimaryShippingAddressID, AddressID);
		}

		/// <summary>
		/// Gets the salt key.
		/// </summary>
		/// <param name="AddressID">The address ID.</param>
		/// <returns>Returns the salt key</returns>
		public static string StaticGetSaltKey(int AddressID)
        {
            var tmp = string.Empty;
            using (var dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (var rs = DB.GetRS("select " + AppLogic.AppConfig("AddressCCSaltField") + " from Address  with (NOLOCK)  where AddressID=" + AddressID.ToString(), dbconn))
                {
                    if (rs.Read())
                    {
                        tmp = rs[AppLogic.AppConfig("AddressCCSaltField")].ToString();
                    }
                }
            }
            return tmp;
        }

		/// <summary>
		/// Loads the Address record using the AddressID to select it.
		/// </summary>
		/// <param name="addressId"></param>
		public void LoadFromDB(int addressId)
		{
			Clear();
			using(var connection = DB.dbConn())
			{
				connection.Open();
                // nal removed select *
				using(var reader = DB.GetRS("SELECT a.AddressID, a.AddressGUID, a.CustomerID, a.NickName, a.FirstName, a.LastName, a.Company, a.Address1, a.Address2, a.Suite, a.City, a.STATE, a.Zip, a.Country, a.ResidenceType, a.Phone, a.Email, a.PaymentMethodLastUsed, a.CardType, a.CardName, a.CardNumber, a.CardExpirationMonth, a.CardExpirationYear, a.CardStartDate, a.CardIssueNumber, a.PONumber, a.ExtensionData, a.Deleted, a.CreatedOn, a.Crypt, a.UpdatedOn, a.OffsiteSource, c.CountryID, s.StateID FROM Address a WITH(NOLOCK) LEFT OUTER JOIN dbo.Country c WITH(NOLOCK) ON a.country = c.NAME LEFT OUTER JOIN dbo.STATE s WITH(NOLOCK) ON a.STATE = s.Abbreviation AND s.countryid = c.countryid where AddressID=@addressId", new [] { new SqlParameter("addressId", addressId) }, connection))
				{
					if(reader.Read())
					{
						m_AddressID = DB.RSFieldInt(reader, "AddressID");
						m_AddressGuid = DB.RSFieldGUID(reader, "AddressGuid");
						CustomerID = DB.RSFieldInt(reader, "CustomerID");
						m_NickName = DB.RSField(reader, "NickName");
						m_FirstName = DB.RSField(reader, "FirstName");
						m_LastName = DB.RSField(reader, "LastName");
						m_Company = DB.RSField(reader, "Company");
						m_Address1 = DB.RSField(reader, "Address1");
						m_Address2 = DB.RSField(reader, "Address2");
						m_Suite = DB.RSField(reader, "Suite");
						m_City = DB.RSField(reader, "City");
						m_State = DB.RSField(reader, "State");
						m_Zip = DB.RSField(reader, "Zip");
						m_Country = DB.RSField(reader, "Country");
						m_CountryID = DB.RSFieldInt(reader, "CountryID");
						m_StateID = DB.RSFieldInt(reader, "StateID");
						m_Phone = DB.RSField(reader, "Phone");
						m_ResidenceType = (ResidenceTypes)DB.RSFieldInt(reader, "ResidenceType");
						PaymentMethodLastUsed = DB.RSField(reader, "PaymentMethodLastUsed");
						CardType = DB.RSField(reader, "CardType");

						CardNumber = DB.RSField(reader, "CardNumber");
						if(CardNumber.Length != 0 && CardNumber != AppLogic.ro_CCNotStoredString)
						{
							CardNumber = Security.UnmungeString(CardNumber, this.GetSaltKey());
							if(CardNumber.StartsWith(Security.ro_DecryptFailedPrefix, StringComparison.InvariantCultureIgnoreCase))
								CardNumber = DB.RSField(reader, "CardNumber");
						}

						CardName = DB.RSField(reader, "CardName");
						CardExpirationMonth = DB.RSField(reader, "CardExpirationMonth");
						CardExpirationYear = DB.RSField(reader, "CardExpirationYear");
						CardStartDate = DB.RSField(reader, "CardStartDate");
						CardIssueNumber = Security.UnmungeString(DB.RSField(reader, "CardIssueNumber"));
						PONumber = DB.RSField(reader, "PONumber");
						EMail = DB.RSField(reader, "EMail");
						OffsiteSource = DB.RSField(reader, "OffsiteSource");
					}
					else
					{
						Clear();
					}
				}
			}
		}

		/// <summary>
		/// Loads the customer.
		/// </summary>
		/// <param name="CustomerID">The customer ID.</param>
		/// <param name="aAddressType">Type of a address.</param>
		public void LoadByCustomer(int CustomerID, AddressTypes aAddressType)
        {
            int AddressID = 0;
            using (var dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (var rs = DB.GetRS(String.Format("select ShippingAddressID,BillingAddressID from Customer with (NOLOCK) where CustomerID={0}", CustomerID.ToString()), dbconn))
                {
                    if (rs.Read())
                    {
                        if (aAddressType == AddressTypes.Billing)
                        {
                            AddressID = DB.RSFieldInt(rs, "BillingAddressID");
                        }
                        else
                        {
                            AddressID = DB.RSFieldInt(rs, "ShippingAddressID");
                        }
                    }
                }
            }

            LoadByCustomer(CustomerID, AddressID, aAddressType);
        }

        /// <summary>
        /// Loads the customer.
        /// </summary>
        /// <param name="CustomerID">The customer ID.</param>
        /// <param name="AddressID">The address ID.</param>
        /// <param name="aAddressType">Type of a address.</param>
        public void LoadByCustomer(int CustomerID, int AddressID, AddressTypes aAddressType)
        {
            m_AddressID = AddressID;
            if (m_AddressID != 0)
            {
                LoadFromDB(m_AddressID);
                m_AddressType = aAddressType;
            }
            else
            {
                Clear();
            }
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return DisplayHTML(false);
        }

        /// <summary>
        /// Displays the string.
        /// </summary>
        /// <param name="Checkout">if set to <c>true</c> [checkout].</param>
        /// <param name="IncludePhone">if set to <c>true</c> [include phone].</param>
        /// <param name="Separator">The separator.</param>
        /// <returns>Returns the display string.</returns>
        public string DisplayHTML(bool IncludePhone)
        {
            StringBuilder tmpS = new StringBuilder(1000);

            tmpS.Append(String.Format("<div>{0} {1}</div>", FirstName, LastName));
            tmpS.Append(String.IsNullOrEmpty(m_Company) ? "" : String.Format("<div>{0}</div>", Company));
            tmpS.Append(String.IsNullOrEmpty(m_Address1) ? "" : String.Format("<div>{0}</div>", Address1));
            tmpS.Append(String.IsNullOrEmpty(m_Address2) ? "" : String.Format("<div>{0}</div>", Address2));
            tmpS.Append(String.IsNullOrEmpty(m_Suite) ? "" : String.Format("<div>{0}</div>", Suite));
            tmpS.Append(String.Format("<div>{0}, {1} {2}</div>", City, State, Zip));
            tmpS.Append(String.IsNullOrEmpty(m_Country) ? "" : String.Format("<div>{0}</div>", Country));
            if (IncludePhone)
            {
                tmpS.Append(String.IsNullOrEmpty(m_Phone) ? "" : String.Format("<div>{0}</div>", Phone));
            }

            return tmpS.ToString();
        }

		public static Address FindOrCreateOffSiteAddress(int customerId, 
			string city, 
			string stateAbbreviation, 
			string postalCode, 
			string countryName, 
			string offSiteSource,
			string firstName = null,
			string lastName = null,
            string address1 = null,
			string address2 = null,
			string phone = null)
		{
			var addressId = DB.GetSqlN(
				sql: "select AddressId as N from Address where CustomerId=@customerId and City=@city and State=@stateAbbreviation and Zip=@postalCode and Country=@countryName and OffsiteSource = @offsiteSource",
				parameters: new[]
					{
						new SqlParameter("customerId", customerId),
						new SqlParameter("city", city),
						new SqlParameter("stateAbbreviation", stateAbbreviation),
						new SqlParameter("postalCode", postalCode),
						new SqlParameter("countryName", countryName),
						new SqlParameter("offsiteSource", offSiteSource),
					});

			var address = new Address(CustomerID: customerId, AddressType: AddressTypes.Shipping);
			if(addressId > 0)
			{
				address.LoadFromDB(addressId);

				// The matching Amazon address will be reused but we need to clear the fields Amazon doesn't
				// make available until after the order is placed. When we get the full details, we'll put all
				// available values back on the address. If we didn't do this, changes on Amazon will be displayed
				// incorrectly during checkout.
				if(offSiteSource == AppLogic.ro_PMAmazonPayments)
				{
					address.NickName =
						address.Address1 =
						address.Address2 =
						address.Phone =
						address.NickName = string.Empty;
				}

				address.ClearCCInfo();    //Just in case
				address.UpdateDB();
			}
			else
			{
				address.FirstName = firstName != null
					? firstName
					: string.Empty;

				address.LastName = lastName != null
					? lastName
					: string.Empty;

				address.Address1 = address1 != null
					? address1
					: string.Empty;

				address.Address2 = address2 != null
					? address2
					: string.Empty;

				address.Phone = phone != null
					? phone
					: string.Empty;

				address.City = city;
				address.State = stateAbbreviation;
				address.Zip = postalCode;
				address.Country = countryName;
				address.OffsiteSource = offSiteSource;
				address.PaymentMethodLastUsed = offSiteSource;
				address.InsertDB();
			}

			return address;
		}

		public static void ReleaseOffsiteAddress(Customer customer, Address offsiteAddress)
		{
			// Releasing an address means removing the offsite source data, thus enabling visibility in the address editor
			// and management of the address by the customer.

			// Find a previously saved address matching the offsite address before releasing it.
			var localAddressId = DB.GetSqlN(
				@"select AddressId as N from Address 
					where CustomerId = @customerId
					and OffsiteSource = ''
					and NickName = @nickName
					and FirstName = @firstName
					and LastName = @lastName
					and Address1 = @address1
					and Address2 = @address2
					and City = @city
					and State = @state
					and Zip = @zip
					and Country = @country
					and Phone = @phone",
				new SqlParameter("customerId", customer.CustomerID),
				new SqlParameter("nickName", offsiteAddress.NickName),
				new SqlParameter("firstName", offsiteAddress.FirstName),
				new SqlParameter("lastName", offsiteAddress.LastName),
				new SqlParameter("address1", offsiteAddress.Address1),
				new SqlParameter("address2", offsiteAddress.Address2),
				new SqlParameter("city", offsiteAddress.City),
				new SqlParameter("state", offsiteAddress.State),
				new SqlParameter("zip", offsiteAddress.Zip),
				new SqlParameter("country", offsiteAddress.Country),
				new SqlParameter("phone", offsiteAddress.Phone));

			// If no local address matching the offsite address is found, then release the offsite address...
			if(localAddressId == 0)
			{
				offsiteAddress.CardNumber = string.Empty; // Clear amazon reference id.
				offsiteAddress.OffsiteSource = string.Empty;
				offsiteAddress.UpdateDB();
			}
			// ...otherwise delete it.
			else
			{
				DeleteFromDB(offsiteAddress.AddressID, customer.CustomerID);
			}
		}

		public static void CleanupAbandonAddresses(Customer customer, string offsiteSource)
		{
			// We could have unused offsite addresses from switching during checkout.
			// Cleanup all addresses for this customer that have not been released.
			DB.ExecuteSQL(
				"delete from Address where OffsiteSource = @offsiteSource and CustomerId = @customerId",
				new SqlParameter("offsiteSource", offsiteSource),
				new SqlParameter("customerId", customer.CustomerID));
		}

		/// <summary>
		/// Inputs the card HTML.
		/// </summary>
		/// <param name="ThisCustomer">The this customer.</param>
		/// <param name="Validate">if set to <c>true</c> [validate].</param>
		/// <param name="CheckForTerms">if set to <c>true</c> [check for terms].</param>
		/// <returns></returns>
		public String InputCardHTML(Customer customer, bool validate)
        {
            var tmpS = new StringBuilder(1000);

            tmpS.Append("<div class='form card-form'>");
            // Credit Card fields

            if (CardExpirationMonth == "")
            {
                CardExpirationMonth = CommonLogic.ParamsCanBeDangerousContent("CardExpirationMonth");
            }

            if (CardExpirationYear == "")
            {
                CardExpirationYear = CommonLogic.ParamsCanBeDangerousContent("CardExpirationYear");
            }

            if (CardType == "")
            {
                CardType = CommonLogic.ParamsCanBeDangerousContent("CardType");
            }

            if (CardName == "")
            {
                CardName = CommonLogic.ParamsCanBeDangerousContent("CardName");
            }

            if (CardNumber == "")
            {
                CardNumber = CommonLogic.ParamsCanBeDangerousContent("CardNumber");
            }

            if (CardExpirationMonth == "")
            {
                CardExpirationMonth = CommonLogic.ParamsCanBeDangerousContent("CardExpirationMonth");
            }

            if (customer.MasterShouldWeStoreCreditCardInfo)
            {
                tmpS.Append("      <div class='form-group'>");
                tmpS.Append("       <label>*Name On Card:</label>\n");
                tmpS.Append("        <input type=\"text\" name=\"CardName\" id=\"CardName\" class=\"form-control\" maxlength=\"100\" value=\"" + HttpContext.Current.Server.HtmlEncode(CardName.Trim()) + "\">\n");
                if (validate)
                {
                    tmpS.Append("        <input type=\"hidden\" name=\"CardName_vldt\" value=\"[req][blankalert=Please enter the name on the credit card]\">\n");
                }
                tmpS.Append("      </div>");
                tmpS.Append("      <div class='form-group'>");
                tmpS.Append("        <label>*Credit Card Number:</label>");
                tmpS.Append("        <input type=\"text\" autocomplete=\"off\" name=\"CardNumber\" id=\"CardNumber\" class=\"form-control\" maxlength=\"20\" value=\"" + AppLogic.SafeDisplayCardNumber(CardNumber, "Address", m_AddressID) + "\"> (no spaces)\n");
                if (validate)
                {
                    tmpS.Append("        <input type=\"hidden\" name=\"CardNumber_vldt\" value=\"[req][len=8][blankalert=Please enter credit card number with no spaces][invalidalert=Please enter a valid credit card number]\">\n");
                }
                tmpS.Append("      </div>");
                tmpS.Append("      <div class='form-group'>");
                tmpS.Append("        <label>*Card Type:</label>");
                tmpS.Append("        <select class=\"form-control\" name=\"CardType\" id=\"CardType\">");
                tmpS.Append("				<option value=\"\">CARD TYPE");

                using (var dbconn = DB.dbConn())
                {
                    dbconn.Open();
                    using (var rsCard = DB.GetRS("select * from creditcardtype  with (NOLOCK)  where Accepted=1 order by CardType", dbconn))
                    {
                        while (rsCard.Read())
                        {
                            tmpS.Append("<option value=\"" + DB.RSField(rsCard, "CardType") + "\" " + CommonLogic.IIF(CardType == DB.RSField(rsCard, "CardType"), " selected ", "") + ">" + DB.RSField(rsCard, "CardType") + "</option>\n");
                        }
                    }
                }

                tmpS.Append("              </select>\n");
                tmpS.Append("      </div>");

                tmpS.Append("      <div class='form-group'>");
                tmpS.Append("        <label>*Expiration Date:</label>");
                tmpS.Append("        <select class=\"form-control\" name=\"CardExpirationMonth\" id=\"CardExpirationMonth\">");
                tmpS.Append("<option value=\"\">MONTH");
                for (int i = 1; i <= 12; i++)
                {
					tmpS.AppendFormat("<option value=\"{0}\" {1}>{0}</option>",
						i.ToString().PadLeft(2, '0'),
						CardExpirationMonth.PadLeft(2, '0') == i.ToString().PadLeft(2, '0')
							? "selected "
							: string.Empty);
                }
                tmpS.Append("</select>    <select class=\"form-control\" name=\"CardExpirationYear\" id=\"CardExpirationYear\">");
                tmpS.Append("<option value=\"\" SELECTED>YEAR");
                for (int y = 0; y <= 10; y++)
                {
                    tmpS.Append("<option value=\"" + (System.DateTime.Now.Year + y).ToString() + "\" " + CommonLogic.IIF(CardExpirationYear == (System.DateTime.Now.Year + y).ToString(), " selected ", "") + ">" + (System.DateTime.Now.Year + y).ToString() + "</option>");
                }
                tmpS.Append("</select>");
                tmpS.Append("      </div>");


                // nal ShowCardStartDateFields
                // If TRUE, credit card forms will show the Card Start Date, and Card Issue Number fields. Used for UK/EU storefronts.
      //          if (AppLogic.AppConfigBool("ShowCardStartDateFields"))
      //          {
      //              tmpS.Append("      <div class='form-group'>");
      //              tmpS.Append("        <label>*Card Start Date:</label>");
      //              String CardStartDateMonth = String.Empty;
      //              try
      //              {
						//CardStartDateMonth = CardStartDate.Substring(0, 2);
      //              }
      //              catch { }
      //              String CardStartDateYear = String.Empty;
      //              try
      //              {
      //                  CardStartDateYear = CardStartDate.Substring(2, 2);
      //              }
      //              catch { }

      //              tmpS.Append("        <select class=\"form-control\" name=\"CardStartDateMonth\" id=\"CardStartDateMonth\">");
      //              tmpS.Append("<option value=\"\">" + AppLogic.GetString("address.cs.34", m_SkinID, m_LocaleSetting));
      //              for (int i = 1; i <= 12; i++)
      //              {
      //                  tmpS.Append("<option value=\"" + i.ToString().PadLeft(2, '0') + "\" " + CommonLogic.IIF(CardStartDateMonth == i.ToString().PadLeft(2, '0'), " selected ", "") + ">" + i.ToString().PadLeft(2, '0') + "</option>");
      //              }
      //              tmpS.Append("</select>    <select class=\"form-control\" name=\"CardStartDateYear\" id=\"CardStartDateYear\">");
      //              tmpS.Append("<option value=\"\" SELECTED>YEAR");
      //              for (int y = -4; y <= 10; y++)
      //              {
						//tmpS.AppendFormat("<option value=\"{0}\" {1}>{2}</option>",
						//	(DateTime.Now.Year + y).ToString().Substring(2, 2),
						//	CardStartDateYear == (DateTime.Now.Year + y).ToString().Substring(2, 2)
						//		? "selected"
						//		: string.Empty,
						//	(DateTime.Now.Year + y).ToString());
      //              }
      //              tmpS.Append("</select>");
      //              tmpS.Append("      </div>");


      //              tmpS.Append("      <div class='form-group'>");
      //              tmpS.Append("        <label>Card Issue Number:</label>");
      //              tmpS.Append("        <input type=\"text\" autocomplete=\"off\" name=\"CardIssueNumber\" id=\"CardIssueNumber\" class=\"form-control\" maxlength=\"2\" value=\"" + CardIssueNumber + "\"> (if shown, Switch/Solo only)\n");
      //              tmpS.Append("      </div>");
      //          }
            }
            tmpS.Append("</div>");
            return tmpS.ToString();
        }
    }

    public class Addresses : ArrayList
    {
        public Addresses() { }

        public new Address this[int index]
        {
            get
            {
                return (Address)base[index];
            }
            set
            {
                base[index] = value;
            }
        }

        /// <summary>
        /// Loads the customer.
        /// </summary>
        /// <param name="CustomerID">The customer ID.</param>
        public void LoadCustomer(int CustomerID)
        {
            var sql = string.Format("select AddressID from Address with (NOLOCK) where CustomerID={0}", CustomerID.ToString());

            using (var dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (var rs = DB.GetRS(sql, dbconn))
                {
                    while (rs.Read())
                    {
                        int AddressID = DB.RSFieldInt(rs, "AddressID");
                        Address newAddress = new Address();
                        newAddress.LoadFromDB(AddressID);
                        Add(newAddress);
                    }
                }
            }
        }
    }
}
