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
using System.Xml;

namespace AspDotNetStorefrontCore
{
	public enum GiftCardTypes : int
	{
		PhysicalGiftCard = 100,
		EMailGiftCard = 101,
		CertificateGiftCard = 102
	}

	public enum GiftCardUsageReasons : int
	{
		UsedByCustomer = 2,
		FundsAddedByAdmin = 3,
		FundsRemovedByAdmin = 4
	}

	/// <summary>
	/// </summary>
	public class GiftCard
	{
		private int m_GiftCardID = 0;
		private string m_GiftCardGUID = String.Empty;
		private string m_SerialNumber = String.Empty;
		private int m_OrderNumber = 0;
		private int m_PurchasedByCustomerID = 0;
		private int m_ShoppingCartRecID = 0;
		private int m_ProductID = 0;
		private int m_VariantID = 0;
		private decimal m_InitialAmount = System.Decimal.Zero;
		private decimal m_Balance = System.Decimal.Zero;
		private DateTime m_ExpirationDate = System.DateTime.MinValue;
		private int m_GiftCardTypeID = 101;
		private string m_EMailName = String.Empty;
		private string m_EMailTo = String.Empty;
		private string m_EMailMessage = String.Empty;
		private string m_ValidForCustomers = String.Empty;
		private string m_ValidForProducts = String.Empty;
		private string m_ValidForManufacturers = String.Empty;
		private string m_ValidForCategories = String.Empty;
		private string m_ValidForSections = String.Empty;
		private string m_ExtensionData = String.Empty;
		private DateTime m_CreatedOn = System.DateTime.MinValue;
		private bool m_DisabledByAdministrator = false;
		private GiftCardTransactions m_giftcardransactions = null;

		public GiftCard(int GiftCardID)
		{
			LoadFromDB(GiftCardID);

		}

		public GiftCard(string SerialNumber)
		{
			LoadFromDB(SerialNumber);

		}

		/// <summary>
		/// Updates GiftCard records to Order_ShoppingCart table
		/// </summary>
		/// <param name="OrderNumber">Order containing gift card</param>
		public static void SyncOrderNumber(int OrderNumber)
		{
			DB.ExecuteSQL(
				@"
                UPDATE GiftCard SET OrderNumber = @OrderNumber WHERE ShoppingCartRecID IN (
                SELECT ShoppingCartRecID FROM Orders_ShoppingCart WHERE OrderNumber = @OrderNumber)
                ",
				new SqlParameter[] { new SqlParameter("@OrderNumber", OrderNumber) })
				;
		}

		public static GiftCard CreateGiftCard(int PurchasedByCustomerID, string SerialNumber, object OrderNumber, int ShoppingCartRecID, object ProductID, object VariantID, object InitialAmount, object ExpirationDate, object Balance, object GiftCardTypeID, string EMailName, string EMailTo, string EMailMessage, string ValidForCustomers, string ValidForProducts, string ValidForManufacturers, string ValidForCategories, string ValidForSections, string ExtensionData)
		{
			var GiftCardID = 0;

			using(var cn = new SqlConnection(DB.GetDBConn()))
			{
				cn.Open();
				using(var cmd = new SqlCommand())
				{
					cmd.Connection = cn;
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.CommandText = "dbo.aspdnsf_CreateGiftCard";

					cmd.Parameters.Add(new SqlParameter("@SerialNumber", SqlDbType.NVarChar, 200));
					cmd.Parameters.Add(new SqlParameter("@PurchasedByCustomerID", SqlDbType.Int, 4));
					cmd.Parameters.Add(new SqlParameter("@OrderNumber", SqlDbType.Int, 4));
					cmd.Parameters.Add(new SqlParameter("@ShoppingCartRecID", SqlDbType.Int, 4));
					cmd.Parameters.Add(new SqlParameter("@ProductID", SqlDbType.Int, 4));
					cmd.Parameters.Add(new SqlParameter("@VariantID", SqlDbType.Int, 4));
					cmd.Parameters.Add(new SqlParameter("@InitialAmount", SqlDbType.Decimal, 8));
					cmd.Parameters.Add(new SqlParameter("@Balance", SqlDbType.Decimal, 8));
					cmd.Parameters.Add(new SqlParameter("@ExpirationDate", SqlDbType.DateTime, 8));
					cmd.Parameters.Add(new SqlParameter("@GiftCardTypeID", SqlDbType.Int, 4));
					cmd.Parameters.Add(new SqlParameter("@EMailName", SqlDbType.NVarChar, 200));
					cmd.Parameters.Add(new SqlParameter("@EMailTo", SqlDbType.NVarChar, 200));
					cmd.Parameters.Add(new SqlParameter("@EMailMessage", SqlDbType.NText));
					cmd.Parameters.Add(new SqlParameter("@ValidForCustomers", SqlDbType.NText));
					cmd.Parameters.Add(new SqlParameter("@ValidForProducts", SqlDbType.NText));
					cmd.Parameters.Add(new SqlParameter("@ValidForManufacturers", SqlDbType.NText));
					cmd.Parameters.Add(new SqlParameter("@ValidForCategories", SqlDbType.NText));
					cmd.Parameters.Add(new SqlParameter("@ValidForSections", SqlDbType.NText));
					cmd.Parameters.Add(new SqlParameter("@ExtensionData", SqlDbType.NText));
					cmd.Parameters.Add(new SqlParameter("@GiftCardID", SqlDbType.Int, 4)).Direction = ParameterDirection.Output;

					cmd.Parameters["@PurchasedByCustomerID"].Value = PurchasedByCustomerID;

					if(SerialNumber == null)
						cmd.Parameters["@SerialNumber"].Value = DBNull.Value;
					else
						cmd.Parameters["@SerialNumber"].Value = SerialNumber;

					if(OrderNumber == null)
						cmd.Parameters["@OrderNumber"].Value = DBNull.Value;
					else
						cmd.Parameters["@OrderNumber"].Value = OrderNumber;

					cmd.Parameters["@ShoppingCartRecID"].Value = ShoppingCartRecID;

					if(ProductID == null)
						cmd.Parameters["@ProductID"].Value = DBNull.Value;
					else
						cmd.Parameters["@ProductID"].Value = ProductID;

					if(VariantID == null)
						cmd.Parameters["@VariantID"].Value = DBNull.Value;
					else
						cmd.Parameters["@VariantID"].Value = VariantID;

					if(InitialAmount == null)
						cmd.Parameters["@InitialAmount"].Value = DBNull.Value;
					else
						cmd.Parameters["@InitialAmount"].Value = InitialAmount;

					if(Balance == null)
						cmd.Parameters["@Balance"].Value = DBNull.Value;
					else
						cmd.Parameters["@Balance"].Value = Balance;

					if(ExpirationDate == null || !CommonLogic.IsDate(ExpirationDate.ToString()))
						cmd.Parameters["@ExpirationDate"].Value = DBNull.Value;
					else
						cmd.Parameters["@ExpirationDate"].Value = DateTime.Parse(ExpirationDate.ToString());

					if(GiftCardTypeID == null)
						cmd.Parameters["@GiftCardTypeID"].Value = DBNull.Value;
					else
						cmd.Parameters["@GiftCardTypeID"].Value = GiftCardTypeID;

					if(EMailName == null)
						cmd.Parameters["@EMailName"].Value = DBNull.Value;
					else
						cmd.Parameters["@EMailName"].Value = EMailName;

					if(EMailTo == null) cmd.Parameters["@EMailTo"].Value = DBNull.Value;
					else
						cmd.Parameters["@EMailTo"].Value = EMailTo;

					if(EMailMessage == null)
						cmd.Parameters["@EMailMessage"].Value = DBNull.Value;
					else
						cmd.Parameters["@EMailMessage"].Value = EMailMessage;

					if(ValidForCustomers == null)
						cmd.Parameters["@ValidForCustomers"].Value = DBNull.Value;
					else
						cmd.Parameters["@ValidForCustomers"].Value = ValidForCustomers;

					if(ValidForProducts == null)
						cmd.Parameters["@ValidForProducts"].Value = DBNull.Value;
					else
						cmd.Parameters["@ValidForProducts"].Value = ValidForProducts;

					if(ValidForManufacturers == null)
						cmd.Parameters["@ValidForManufacturers"].Value = DBNull.Value;
					else
						cmd.Parameters["@ValidForManufacturers"].Value = ValidForManufacturers;

					if(ValidForCategories == null)
						cmd.Parameters["@ValidForCategories"].Value = DBNull.Value;
					else
						cmd.Parameters["@ValidForCategories"].Value = ValidForCategories;

					if(ValidForSections == null)
						cmd.Parameters["@ValidForSections"].Value = DBNull.Value;
					else
						cmd.Parameters["@ValidForSections"].Value = ValidForSections;

					if(ExtensionData == null)
						cmd.Parameters["@ExtensionData"].Value = DBNull.Value;
					else
						cmd.Parameters["@ExtensionData"].Value = ExtensionData;


					try
					{
						cmd.ExecuteNonQuery();
						GiftCardID = Int32.Parse(cmd.Parameters["@GiftCardID"].Value.ToString());
					}
					catch(Exception ex)
					{
						SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Error);
					}
				}
			}

			if(GiftCardID > 0)
				return new GiftCard(GiftCardID);

			return null;
		}

		public static string UpdateCard(int GiftCardID, string SerialNumber, object OrderNumber, object InitialAmount, object Balance, object DisabledByAdministrator, object ExpirationDate, string EMailName, string EMailTo, string EMailMessage, string ValidForCustomers, string ValidForProducts, string ValidForManufacturers, string ValidForCategories, string ValidForSections, string ExtensionData)
		{

			var err = string.Empty;

			using(var cn = new SqlConnection(DB.GetDBConn()))
			{
				cn.Open();
				using(var cmd = new SqlCommand())
				{
					cmd.Connection = cn;
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.CommandText = "dbo.aspdnsf_updGiftCard";

					cmd.Parameters.Add(new SqlParameter("@GiftCardID", SqlDbType.Int, 4));
					cmd.Parameters.Add(new SqlParameter("@SerialNumber", SqlDbType.NVarChar, 200));
					cmd.Parameters.Add(new SqlParameter("@OrderNumber", SqlDbType.Int, 4));
					cmd.Parameters.Add(new SqlParameter("@InitialAmount", SqlDbType.Decimal, 8));
					cmd.Parameters.Add(new SqlParameter("@Balance", SqlDbType.Decimal, 8));
					cmd.Parameters.Add(new SqlParameter("@ExpirationDate", SqlDbType.VarChar, 30));
					cmd.Parameters.Add(new SqlParameter("@EMailName", SqlDbType.NVarChar, 200));
					cmd.Parameters.Add(new SqlParameter("@EMailTo", SqlDbType.NVarChar, 200));
					cmd.Parameters.Add(new SqlParameter("@EMailMessage", SqlDbType.NText));
					cmd.Parameters.Add(new SqlParameter("@ValidForCustomers", SqlDbType.NText));
					cmd.Parameters.Add(new SqlParameter("@ValidForProducts", SqlDbType.NText));
					cmd.Parameters.Add(new SqlParameter("@ValidForManufacturers", SqlDbType.NText));
					cmd.Parameters.Add(new SqlParameter("@ValidForCategories", SqlDbType.NText));
					cmd.Parameters.Add(new SqlParameter("@ValidForSections", SqlDbType.NText));
					cmd.Parameters.Add(new SqlParameter("@DisabledByAdministrator", SqlDbType.TinyInt, 1));
					cmd.Parameters.Add(new SqlParameter("@ExtensionData", SqlDbType.NText));

					cmd.Parameters["@GiftCardID"].Value = GiftCardID;

					if(SerialNumber == null)
						cmd.Parameters["@SerialNumber"].Value = DBNull.Value;
					else
						cmd.Parameters["@SerialNumber"].Value = SerialNumber;

					if(OrderNumber == null)
						cmd.Parameters["@OrderNumber"].Value = DBNull.Value;
					else
						cmd.Parameters["@OrderNumber"].Value = OrderNumber;

					if(InitialAmount == null)
						cmd.Parameters["@InitialAmount"].Value = DBNull.Value;
					else
						cmd.Parameters["@InitialAmount"].Value = InitialAmount;

					if(Balance == null)
						cmd.Parameters["@Balance"].Value = DBNull.Value;
					else
						cmd.Parameters["@Balance"].Value = Balance;

					if(ExpirationDate == null)
						cmd.Parameters["@ExpirationDate"].Value = DBNull.Value;
					else
						cmd.Parameters["@ExpirationDate"].Value = ExpirationDate;

					if(EMailName == null)
						cmd.Parameters["@EMailName"].Value = DBNull.Value;
					else
						cmd.Parameters["@EMailName"].Value = EMailName;

					if(EMailTo == null)
						cmd.Parameters["@EMailTo"].Value = DBNull.Value;
					else
						cmd.Parameters["@EMailTo"].Value = EMailTo;

					if(EMailMessage == null)
						cmd.Parameters["@EMailMessage"].Value = DBNull.Value;
					else
						cmd.Parameters["@EMailMessage"].Value = EMailMessage;

					if(ValidForCustomers == null)
						cmd.Parameters["@ValidForCustomers"].Value = DBNull.Value;
					else
						cmd.Parameters["@ValidForCustomers"].Value = ValidForCustomers;

					if(ValidForProducts == null)
						cmd.Parameters["@ValidForProducts"].Value = DBNull.Value;
					else
						cmd.Parameters["@ValidForProducts"].Value = ValidForProducts;

					if(ValidForManufacturers == null)
						cmd.Parameters["@ValidForManufacturers"].Value = DBNull.Value;
					else
						cmd.Parameters["@ValidForManufacturers"].Value = ValidForManufacturers;

					if(ValidForCategories == null)
						cmd.Parameters["@ValidForCategories"].Value = DBNull.Value;
					else
						cmd.Parameters["@ValidForCategories"].Value = ValidForCategories;

					if(ValidForSections == null)
						cmd.Parameters["@ValidForSections"].Value = DBNull.Value;
					else
						cmd.Parameters["@ValidForSections"].Value = ValidForSections;

					if(DisabledByAdministrator == null)
						cmd.Parameters["@DisabledByAdministrator"].Value = DBNull.Value;
					else
						cmd.Parameters["@DisabledByAdministrator"].Value = DisabledByAdministrator;

					if(ExtensionData == null)
						cmd.Parameters["@ExtensionData"].Value = DBNull.Value;
					else
						cmd.Parameters["@ExtensionData"].Value = ExtensionData;

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

		public string UpdateCard(string SerialNumber, object OrderNumber, object InitialAmount, object Balance, object DisabledByAdministrator, object ExpirationDate, string EMailName, string EMailTo, string EMailMessage, string ValidForCustomers, string ValidForProducts, string ValidForManufacturers, string ValidForCategories, string ValidForSections, string ExtensionData)
		{

			var err = string.Empty;
			using(var cn = new SqlConnection(DB.GetDBConn()))
			{
				cn.Open();
				using(var cmd = new SqlCommand())
				{
					cmd.Connection = cn;
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.CommandText = "dbo.aspdnsf_updGiftCard";

					cmd.Parameters.Add(new SqlParameter("@GiftCardID", SqlDbType.Int, 4));
					cmd.Parameters.Add(new SqlParameter("@SerialNumber", SqlDbType.NVarChar, 200));
					cmd.Parameters.Add(new SqlParameter("@OrderNumber", SqlDbType.Int, 4));
					cmd.Parameters.Add(new SqlParameter("@InitialAmount", SqlDbType.Decimal, 8));
					cmd.Parameters.Add(new SqlParameter("@Balance", SqlDbType.Decimal, 8));
					cmd.Parameters.Add(new SqlParameter("@ExpirationDate", SqlDbType.VarChar, 30));
					cmd.Parameters.Add(new SqlParameter("@EMailName", SqlDbType.NVarChar, 200));
					cmd.Parameters.Add(new SqlParameter("@EMailTo", SqlDbType.NVarChar, 200));
					cmd.Parameters.Add(new SqlParameter("@EMailMessage", SqlDbType.NText));
					cmd.Parameters.Add(new SqlParameter("@ValidForCustomers", SqlDbType.NText));
					cmd.Parameters.Add(new SqlParameter("@ValidForProducts", SqlDbType.NText));
					cmd.Parameters.Add(new SqlParameter("@ValidForManufacturers", SqlDbType.NText));
					cmd.Parameters.Add(new SqlParameter("@ValidForCategories", SqlDbType.NText));
					cmd.Parameters.Add(new SqlParameter("@ValidForSections", SqlDbType.NText));
					cmd.Parameters.Add(new SqlParameter("@DisabledByAdministrator", SqlDbType.TinyInt, 1));
					cmd.Parameters.Add(new SqlParameter("@ExtensionData", SqlDbType.NText));

					cmd.Parameters["@GiftCardID"].Value = this.m_GiftCardID;

					if(SerialNumber == null) cmd.Parameters["@SerialNumber"].Value = DBNull.Value;
					else cmd.Parameters["@SerialNumber"].Value = SerialNumber;

					if(OrderNumber == null) cmd.Parameters["@OrderNumber"].Value = DBNull.Value;
					else cmd.Parameters["@OrderNumber"].Value = OrderNumber;

					if(InitialAmount == null) cmd.Parameters["@InitialAmount"].Value = DBNull.Value;
					else cmd.Parameters["@InitialAmount"].Value = InitialAmount;

					if(Balance == null) cmd.Parameters["@Balance"].Value = DBNull.Value;
					else cmd.Parameters["@Balance"].Value = Balance;

					if(ExpirationDate == null) cmd.Parameters["@ExpirationDate"].Value = DBNull.Value;
					else cmd.Parameters["@ExpirationDate"].Value = ExpirationDate;

					if(EMailName == null) cmd.Parameters["@EMailName"].Value = DBNull.Value;
					else cmd.Parameters["@EMailName"].Value = EMailName;

					if(EMailTo == null) cmd.Parameters["@EMailTo"].Value = DBNull.Value;
					else cmd.Parameters["@EMailTo"].Value = EMailTo;

					if(EMailMessage == null) cmd.Parameters["@EMailMessage"].Value = DBNull.Value;
					else cmd.Parameters["@EMailMessage"].Value = EMailMessage;

					if(ValidForCustomers == null) cmd.Parameters["@ValidForCustomers"].Value = DBNull.Value;
					else cmd.Parameters["@ValidForCustomers"].Value = ValidForCustomers;

					if(ValidForProducts == null) cmd.Parameters["@ValidForProducts"].Value = DBNull.Value;
					else cmd.Parameters["@ValidForProducts"].Value = ValidForProducts;

					if(ValidForManufacturers == null) cmd.Parameters["@ValidForManufacturers"].Value = DBNull.Value;
					else cmd.Parameters["@ValidForManufacturers"].Value = ValidForManufacturers;

					if(ValidForCategories == null) cmd.Parameters["@ValidForCategories"].Value = DBNull.Value;
					else cmd.Parameters["@ValidForCategories"].Value = ValidForCategories;

					if(ValidForSections == null) cmd.Parameters["@ValidForSections"].Value = DBNull.Value;
					else cmd.Parameters["@ValidForSections"].Value = ValidForSections;

					if(DisabledByAdministrator == null) cmd.Parameters["@DisabledByAdministrator"].Value = DBNull.Value;
					else cmd.Parameters["@DisabledByAdministrator"].Value = DisabledByAdministrator;

					if(ExtensionData == null) cmd.Parameters["@ExtensionData"].Value = DBNull.Value;
					else cmd.Parameters["@ExtensionData"].Value = ExtensionData;

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
			RefreshCard();
			return err;

		}

		public void SerializeGiftCard()
		{
			//physical gift cards cannot be serialized
			if(this.m_GiftCardTypeID != (int)GiftCardTypes.PhysicalGiftCard)
			{
				Customer c = new Customer(m_PurchasedByCustomerID);
				string SerialNumber = String.Empty;
				string GiftCardXml = String.Empty;
				XmlDocument xdoc = new XmlDocument();

				try
				{
					GiftCardXml = AppLogic.RunXmlPackage("giftcardassignment.xml.config", null, c, 1, String.Empty, String.Empty, false, true);
					xdoc.LoadXml(GiftCardXml);
					SerialNumber = xdoc.SelectSingleNode("//CardNumber").InnerText;
				}
				catch
				{
					SerialNumber = CommonLogic.GetNewGUID();
				}
				UpdateCard(SerialNumber, null, null, null, null, null, null, null, null, null, null, null, null, null, null);
			}
		}

		public bool AddTransaction(decimal amount, int UsedByCustomerID, int OrderNumber)
		{
			GiftCardUsageTransaction ut = GiftCardUsageTransaction.CreateTransaction(this.GiftCardID, GiftCardUsageReasons.UsedByCustomer, UsedByCustomerID, OrderNumber, amount, "");
			RefreshCard();
			m_giftcardransactions = null;
			return (ut != null);
		}

		public void LoadFromDB(int GiftCardID)
		{
			Clear();

			using(var dbconn = DB.dbConn())
			{
				dbconn.Open();
				using(var rs = DB.GetRS(string.Format("select GiftCard.* FROM GiftCard with (NOLOCK) where GiftCardID={0}", GiftCardID.ToString()), dbconn))
				{
					if(rs.Read())
					{
						m_GiftCardID = DB.RSFieldInt(rs, "GiftCardID");
						m_GiftCardGUID = DB.RSFieldGUID(rs, "GiftCardGUID");
						m_SerialNumber = DB.RSField(rs, "SerialNumber");
						m_OrderNumber = DB.RSFieldInt(rs, "OrderNumber");
						m_PurchasedByCustomerID = DB.RSFieldInt(rs, "PurchasedByCustomerID");
						m_ShoppingCartRecID = DB.RSFieldInt(rs, "ShoppingCartRecID");
						m_ProductID = DB.RSFieldInt(rs, "ProductID");
						m_VariantID = DB.RSFieldInt(rs, "VariantID");
						m_InitialAmount = DB.RSFieldDecimal(rs, "InitialAmount");
						m_Balance = DB.RSFieldDecimal(rs, "Balance");
						m_ExpirationDate = DB.RSFieldDateTime(rs, "ExpirationDate");
						m_EMailName = DB.RSField(rs, "EMailName");
						m_EMailTo = DB.RSField(rs, "EMailTo");
						m_EMailMessage = DB.RSField(rs, "EMailMessage");
						m_ValidForCustomers = DB.RSField(rs, "ValidForCustomers");
						m_ValidForProducts = DB.RSField(rs, "ValidForProducts");
						m_ValidForManufacturers = DB.RSField(rs, "ValidForManufacturers");
						m_ValidForCategories = DB.RSField(rs, "ValidForCategories");
						m_ValidForSections = DB.RSField(rs, "ValidForSections");
						m_ExtensionData = DB.RSField(rs, "ExtensionData");
						m_CreatedOn = DB.RSFieldDateTime(rs, "CreatedOn");
						m_GiftCardTypeID = DB.RSFieldInt(rs, "GiftCardTypeID");
						m_DisabledByAdministrator = DB.RSFieldBool(rs, "DisabledByAdministrator");
					}
					else
					{
						Clear();
					}
				}
			}
		}

		public void LoadFromDB(string SerialNumber)
		{
			Clear();

			using(var dbconn = DB.dbConn())
			{
				dbconn.Open();
				using(var rs = DB.GetRS(string.Format("select GiftCard.* FROM GiftCard with (NOLOCK) where SerialNumber={0}", DB.SQuote(SerialNumber)), dbconn))
				{
					if(rs.Read())
					{
						m_GiftCardID = DB.RSFieldInt(rs, "GiftCardID");
						m_GiftCardGUID = DB.RSFieldGUID(rs, "GiftCardGUID");
						m_SerialNumber = DB.RSField(rs, "SerialNumber");
						m_OrderNumber = DB.RSFieldInt(rs, "OrderNumber");
						m_PurchasedByCustomerID = DB.RSFieldInt(rs, "PurchasedByCustomerID");
						m_ShoppingCartRecID = DB.RSFieldInt(rs, "ShoppingCartRecID");
						m_ProductID = DB.RSFieldInt(rs, "ProductID");
						m_VariantID = DB.RSFieldInt(rs, "VariantID");
						m_InitialAmount = DB.RSFieldDecimal(rs, "InitialAmount");
						m_Balance = DB.RSFieldDecimal(rs, "Balance");
						m_ExpirationDate = DB.RSFieldDateTime(rs, "ExpirationDate");
						m_EMailName = DB.RSField(rs, "EMailName");
						m_EMailTo = DB.RSField(rs, "EMailTo");
						m_EMailMessage = DB.RSField(rs, "EMailMessage");
						m_ValidForCustomers = DB.RSField(rs, "ValidForCustomers");
						m_ValidForProducts = DB.RSField(rs, "ValidForProducts");
						m_ValidForManufacturers = DB.RSField(rs, "ValidForManufacturers");
						m_ValidForCategories = DB.RSField(rs, "ValidForCategories");
						m_ValidForSections = DB.RSField(rs, "ValidForSections");
						m_ExtensionData = DB.RSField(rs, "ExtensionData");
						m_CreatedOn = DB.RSFieldDateTime(rs, "CreatedOn");
						m_GiftCardTypeID = DB.RSFieldInt(rs, "GiftCardTypeID");
						m_DisabledByAdministrator = DB.RSFieldBool(rs, "DisabledByAdministrator");
					}
					else
					{
						Clear();
					}
				}
			}
		}

		public void SendGiftCardEmail()
		{
			if(this.GiftCardTypeID == (int)GiftCardTypes.EMailGiftCard)
			{
				Customer c = new Customer(this.PurchasedByCustomerID);
				AppLogic.SendMail(
					subject: String.Format("{0} sent you a {1} Gift Certificate!", c.FullName(), AppLogic.AppConfig("StoreName")),
					body: AppLogic.RunXmlPackage(AppLogic.AppConfig("XmlPackage.EmailGiftCardNotification"), null, c, 1, "", "GiftCardID=" + this.GiftCardID.ToString(), false, false),
					useHtml: true,
					fromAddress: AppLogic.AppConfig("MailMe_FromAddress"),
					fromName: AppLogic.AppConfig("MailMe_FromName"),
					toAddress: this.EMailTo,
					toName: this.EMailName,
					bccAddresses: "",
					server: AppLogic.MailServer());
			}
		}

		public void RefreshCard()
		{
			LoadFromDB(this.GiftCardID);
		}

		#region Public Properties
		public GiftCardTransactions GiftCardTransactions
		{
			get
			{
				if(m_giftcardransactions == null)
				{
					m_giftcardransactions = new GiftCardTransactions(m_GiftCardID);
				}
				return m_giftcardransactions;
			}
		}

		public void Clear()
		{
			m_GiftCardID = 0;
			m_GiftCardGUID = String.Empty;
			m_SerialNumber = String.Empty;
			m_OrderNumber = 0;
			m_PurchasedByCustomerID = 0;
			m_ShoppingCartRecID = 0;
			m_ProductID = 0;
			m_VariantID = 0;
			m_GiftCardTypeID = 101;
			m_InitialAmount = System.Decimal.Zero;
			m_Balance = System.Decimal.Zero;
			m_ExpirationDate = System.DateTime.MinValue;
			m_EMailName = String.Empty;
			m_EMailTo = String.Empty;
			m_EMailMessage = String.Empty;
			m_ValidForCustomers = String.Empty;
			m_ValidForProducts = String.Empty;
			m_ValidForManufacturers = String.Empty;
			m_ValidForCategories = String.Empty;
			m_ValidForSections = String.Empty;
			m_ExtensionData = String.Empty;
			m_CreatedOn = System.DateTime.MinValue;
			m_DisabledByAdministrator = false;
		}

		public int GiftCardID
		{
			get { return m_GiftCardID; }
		}
		public string SerialNumber
		{
			get { return m_SerialNumber; }
			set { m_SerialNumber = value; }
		}
		public int PurchasedByCustomerID
		{
			get { return m_PurchasedByCustomerID; }
			set { m_PurchasedByCustomerID = value; }
		}

		public decimal InitialAmount
		{
			get { return m_InitialAmount; }
			set { m_InitialAmount = value; }
		}

		public decimal Balance
		{
			get { return m_Balance; }
			set { m_Balance = value; }
		}

		public string EMailName
		{
			get { return m_EMailName; }
			set { m_EMailName = value; }
		}

		public string EMailTo
		{
			get { return m_EMailTo; }
			set { m_EMailTo = value; }
		}

		public string EMailMessage
		{
			get { return m_EMailMessage; }
			set { m_EMailMessage = value; }
		}

		public int GiftCardTypeID
		{
			get { return m_GiftCardTypeID; }
			set { m_GiftCardTypeID = value; }
		}
		#endregion

		public static string GenerateGiftCardKey(int customerId)
		{
			var ticks = DateTime.Now.Ticks.ToString();

			return string.Format(
				"{0}-{1}-{2}-{3}",
				customerId.ToString().ToUpperInvariant(),
				ticks.Substring(1, 5),
				ticks.Substring(6, 5),
				ticks.Substring(11, 5));
		}

		public static bool ProductIsGiftCard(int productId)
		{
			var physicalGiftCardProductTypeIds = AppLogic.AppConfig("GiftCard.PhysicalProductTypeIDs").Trim();
			var emailGiftCardProductTypeIds = AppLogic.AppConfig("GiftCard.EmailProductTypeIDs").Trim();
			var certificateGiftCardProductTypeIds = AppLogic.AppConfig("GiftCard.CertificateProductTypeIDs").Trim();

			var sql = @"
				select count(*) as N
				from Product
				where 
					ProductId = @productId
					and ProductTypeID in (
						select Items from dbo.Split(@physicalGiftCardProductTypeIds, ',')
						union all select Items from dbo.Split(@emailGiftCardProductTypeIds, ',')
						union all select Items from dbo.Split(@certificateGiftCardProductTypeIds, ','))";

			var count = DB.GetSqlN(
				sql,
				new SqlParameter("productId", productId),
				new SqlParameter("physicalGiftCardProductTypeIds", physicalGiftCardProductTypeIds),
				new SqlParameter("emailGiftCardProductTypeIds", emailGiftCardProductTypeIds),
				new SqlParameter("certificateGiftCardProductTypeIds", certificateGiftCardProductTypeIds));

			return count > 0;
		}

		public static bool ProductIsEmailGiftCard(int productId)
		{
			var emailGiftCardProductTypeIds = AppLogic.AppConfig("GiftCard.EmailProductTypeIDs").Trim();

			var sql = @"
				select count(*) as N
				from Product
				where 
					ProductId = @productId
					and ProductTypeID in (
						select Items from dbo.Split(@emailGiftCardProductTypeIds, ','))";

			var count = DB.GetSqlN(
				sql,
				new SqlParameter("productId", productId),
				new SqlParameter("emailGiftCardProductTypeIds", emailGiftCardProductTypeIds));

			return count > 0;
		}
	}

	public enum GiftCardCollectionFilterType : int
	{
		ShoppingCartID = 1,
		OrderNumber = 2,
		PurchasingCustomerID = 3,
		UsingCustomerID = 4
	}

	public class GiftCards : IEnumerable
	{
		public ArrayList m_GiftCards;

		public GiftCards(int ID, GiftCardCollectionFilterType IDType)
		{
			m_GiftCards = new ArrayList();
			string sql = "select GiftCardID FROM dbo.GiftCard ";
			switch(IDType)
			{
				case GiftCardCollectionFilterType.ShoppingCartID:
					sql += "WHERE ShoppingCartRecID = " + ID.ToString();
					break;
				case GiftCardCollectionFilterType.OrderNumber:
					sql += "WHERE OrderNumber = " + ID.ToString();
					break;
				case GiftCardCollectionFilterType.PurchasingCustomerID:
					sql += "WHERE PurchasedByCustomerID = " + ID.ToString();
					break;
				case GiftCardCollectionFilterType.UsingCustomerID:
					sql = "select distinct GiftCard.GiftCardID FROM dbo.GiftCard join dbo.GiftCardUsage on GiftCard.GiftCardID = GiftCardUsage.GiftCardID WHERE GiftCardUsage.UsedByCustomerID = " + ID.ToString() + " and GiftCard.Balance > 0 and GiftCard.DisabledByAdministrator = 0";
					break;
			}

			using(var dbconn = DB.dbConn())
			{
				dbconn.Open();
				using(var dr = DB.GetRS(sql, dbconn))
				{
					while(dr.Read())
					{
						this.Add(new GiftCard(dr.GetInt32(0)));
					}
				}
			}
		}

		public void Add(GiftCard giftcard)
		{
			m_GiftCards.Add(giftcard);
		}

		public IEnumerator GetEnumerator()
		{
			return new GiftCardsEnumerator(this);
		}
	}

	public class GiftCardsEnumerator : IEnumerator
	{
		private int position = -1;
		private GiftCards giftcards;

		public GiftCardsEnumerator(GiftCards giftcards)
		{
			this.giftcards = giftcards;
		}

		public bool MoveNext()
		{
			if(position < giftcards.m_GiftCards.Count - 1)
			{
				position++;
				return true;
			}
			else
			{
				return false;
			}
		}

		public void Reset()
		{
			position = -1;
		}

		public object Current
		{
			get
			{
				return giftcards.m_GiftCards[position];
			}
		}
	}

	public class GiftCardUsageTransaction
	{
		private int m_Giftcardusageid;
		private string m_Giftcardusageguid;
		private int m_Giftcardid;
		private GiftCardUsageReasons m_UsageType;
		private int m_Usedbycustomerid;
		private int m_Ordernumber;
		private decimal m_Amount;
		private string m_Extensiondata;
		private DateTime m_Createdon;

		GiftCardUsageTransaction(int GiftCardTransactionID)
		{

			using(var dbconn = DB.dbConn())
			{
				dbconn.Open();
				using(var dr = DB.GetRS("aspdnsf_getGiftCardUsage " + GiftCardTransactionID.ToString(), dbconn))
				{
					if(dr.Read())
					{
						m_Giftcardusageid = DB.RSFieldInt(dr, "GiftCardUsageID");
						m_Giftcardusageguid = DB.RSFieldGUID(dr, "GiftCardUsageGUID");
						m_Giftcardid = DB.RSFieldInt(dr, "GiftCardID");
						m_UsageType = (GiftCardUsageReasons)Enum.Parse(typeof(GiftCardUsageReasons), DB.RSFieldInt(dr, "UsageTypeID").ToString());
						m_Usedbycustomerid = DB.RSFieldInt(dr, "UsedByCustomerID");
						m_Ordernumber = DB.RSFieldInt(dr, "OrderNumber");
						m_Amount = DB.RSFieldDecimal(dr, "Amount");
						m_Extensiondata = DB.RSField(dr, "ExtensionData");
						m_Createdon = DB.RSFieldDateTime(dr, "CreatedOn");
					}
				}
			}
		}

		public GiftCardUsageTransaction(int GiftCardUsageID, string GiftCardUsageGUID, int GiftCardID, int UsageTypeID, int UsedByCustomerID, int OrderNumber, decimal Amount, string ExtensionData, DateTime CreatedOn)
		{
			m_Giftcardusageid = GiftCardUsageID;
			m_Giftcardusageguid = GiftCardUsageGUID;
			m_Giftcardid = GiftCardID;
			m_UsageType = (GiftCardUsageReasons)Enum.Parse(typeof(GiftCardUsageReasons), UsageTypeID.ToString());
			m_Usedbycustomerid = UsedByCustomerID;
			m_Ordernumber = OrderNumber;
			m_Amount = Amount;
			m_Extensiondata = ExtensionData;
			m_Createdon = CreatedOn;

		}

		public static GiftCardUsageTransaction CreateTransaction(int GiftCardID, GiftCardUsageReasons UsageReason, int UsedByCustomerID, int OrderNumber, decimal Amount, string ExtensionData)
		{
			var GiftCardUsageID = 0;

			using(var cn = new SqlConnection(DB.GetDBConn()))
			{
				cn.Open();
				using(var cmd = new SqlCommand())
				{
					cmd.Connection = cn;
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.CommandText = "dbo.aspdnsf_insGiftCardUsage";

					cmd.Parameters.Add(new SqlParameter("@GiftCardID", SqlDbType.Int, 4));
					cmd.Parameters.Add(new SqlParameter("@UsageTypeID", SqlDbType.Int, 4));
					cmd.Parameters.Add(new SqlParameter("@UsedByCustomerID", SqlDbType.Int, 4));
					cmd.Parameters.Add(new SqlParameter("@OrderNumber", SqlDbType.Int, 4));
					cmd.Parameters.Add(new SqlParameter("@Amount", SqlDbType.Decimal, 8));
					cmd.Parameters.Add(new SqlParameter("@ExtensionData", SqlDbType.NText));
					cmd.Parameters.Add(new SqlParameter("@GiftCardUsageID", SqlDbType.Int, 4)).Direction = ParameterDirection.Output;

					cmd.Parameters["@GiftCardID"].Value = GiftCardID;
					cmd.Parameters["@UsageTypeID"].Value = (int)UsageReason;
					cmd.Parameters["@UsedByCustomerID"].Value = UsedByCustomerID;
					cmd.Parameters["@OrderNumber"].Value = OrderNumber;
					cmd.Parameters["@Amount"].Value = Amount;
					cmd.Parameters["@ExtensionData"].Value = ExtensionData;

					try
					{
						cmd.ExecuteNonQuery();
						GiftCardUsageID = Int32.Parse(cmd.Parameters["@GiftCardUsageID"].Value.ToString());
					}
					catch(Exception ex)
					{
						SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Error);
					}
				}
			}

			if(GiftCardID > 0)
				return new GiftCardUsageTransaction(GiftCardUsageID);

			return null;
		}
	}

	public class GiftCardTransactions : IEnumerable
	{
		public ArrayList m_Transactions;
		public GiftCardTransactions(int GiftCardID)
		{
			m_Transactions = new ArrayList();

			using(var dbconn = DB.dbConn())
			{
				dbconn.Open();
				using(var dr = DB.GetRS("aspdnsf_getGiftCardUsage " + GiftCardID.ToString(), dbconn))
				{
					while(dr.Read())
					{
						this.Add(new GiftCardUsageTransaction(DB.RSFieldInt(dr, "GiftCardUsageID"), DB.RSFieldGUID(dr, "GiftCardUsageGUID"), DB.RSFieldInt(dr, "GiftCardID"), DB.RSFieldInt(dr, "UsageTypeID"), DB.RSFieldInt(dr, "UsedByCustomerID"), DB.RSFieldInt(dr, "OrderNumber"), DB.RSFieldDecimal(dr, "Amount"), DB.RSField(dr, "ExtensionData"), DB.RSFieldDateTime(dr, "CreatedOn")));
					}
				}
			}
		}

		public void Add(GiftCardUsageTransaction transaction)
		{
			m_Transactions.Add(transaction);
		}

		public IEnumerator GetEnumerator()
		{
			return new GiftCardTransactionsEnumerator(this);
		}
	}

	class GiftCardTransactionsEnumerator : IEnumerator
	{
		private int position = -1;
		private GiftCardTransactions transactions;

		public GiftCardTransactionsEnumerator(GiftCardTransactions transactions)
		{
			this.transactions = transactions;
		}

		public bool MoveNext()
		{
			if(position < transactions.m_Transactions.Count - 1)
			{
				position++;
				return true;
			}
			else
			{
				return false;
			}
		}

		public void Reset()
		{
			position = -1;
		}

		public object Current
		{
			get
			{
				return transactions.m_Transactions[position];
			}
		}
	}
}
