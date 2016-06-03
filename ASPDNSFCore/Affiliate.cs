// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;

namespace AspDotNetStorefrontCore
{
	public class Affiliate
	{
		#region private variables

		private int m_Affiliateid = 0;
		private string m_Email;
		private string m_Firstname;
		private string m_Lastname;

		#endregion

		#region Constructors

		public Affiliate(int AffiliateID)
		{
			using(var dbconn = DB.dbConn())
			{
				dbconn.Open();
				using(var dr = DB.GetRS("aspdnsf_getAffiliate " + AffiliateID.ToString(), dbconn))
				{
					if(dr.Read())
					{
						m_Affiliateid = DB.RSFieldInt(dr, "AffiliateID");
						m_Email = DB.RSField(dr, "EMail");
						m_Firstname = DB.RSField(dr, "FirstName");
						m_Lastname = DB.RSField(dr, "LastName");
						StoreID = DB.RSFieldInt(dr, "StoreID");
					}
				}
			}
		}

		#endregion

		#region Static Methods

		public static string Update(int AffiliteID, string EMail, string Password, string Notes, object IsOnline, string FirstName, string LastName, string Name, string Company, string Address1, string Address2, string Suite, string City, string State, string Zip, string Country, string Phone, string WebSiteName, string WebSiteDescription, string URL, object DefaultSkinID, object ParentAffiliateID, object DisplayOrder, string ExtensionData, string SEName, string SETitle, string SEAltText, string SEKeywords, string SEDescription, object Published, object Wholesale, object Deleted, object SaltKey)
		{
			string err = string.Empty;

			using(var cn = new SqlConnection(DB.GetDBConn()))
			{
				cn.Open();
				using(var cmd = new SqlCommand())
				{
					cmd.Connection = cn;
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.CommandText = "dbo.aspdnsf_updAffiliate";

					cmd.Parameters.Add(new SqlParameter("@AffiliateID", SqlDbType.Int, 4));
					cmd.Parameters.Add(new SqlParameter("@EMail", SqlDbType.NVarChar, 200));
					cmd.Parameters.Add(new SqlParameter("@Password", SqlDbType.NVarChar, 500));
					cmd.Parameters.Add(new SqlParameter("@Notes", SqlDbType.Text));
					cmd.Parameters.Add(new SqlParameter("@IsOnline", SqlDbType.TinyInt, 1));
					cmd.Parameters.Add(new SqlParameter("@FirstName", SqlDbType.NVarChar, 200));
					cmd.Parameters.Add(new SqlParameter("@LastName", SqlDbType.NVarChar, 200));
					cmd.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 200));
					cmd.Parameters.Add(new SqlParameter("@Company", SqlDbType.NVarChar, 200));
					cmd.Parameters.Add(new SqlParameter("@Address1", SqlDbType.NVarChar, 200));
					cmd.Parameters.Add(new SqlParameter("@Address2", SqlDbType.NVarChar, 200));
					cmd.Parameters.Add(new SqlParameter("@Suite", SqlDbType.NVarChar, 100));
					cmd.Parameters.Add(new SqlParameter("@City", SqlDbType.NVarChar, 200));
					cmd.Parameters.Add(new SqlParameter("@State", SqlDbType.NVarChar, 200));
					cmd.Parameters.Add(new SqlParameter("@Zip", SqlDbType.NVarChar, 20));
					cmd.Parameters.Add(new SqlParameter("@Country", SqlDbType.NVarChar, 200));
					cmd.Parameters.Add(new SqlParameter("@Phone", SqlDbType.NVarChar, 50));
					cmd.Parameters.Add(new SqlParameter("@WebSiteName", SqlDbType.NVarChar, 200));
					cmd.Parameters.Add(new SqlParameter("@WebSiteDescription", SqlDbType.NText));
					cmd.Parameters.Add(new SqlParameter("@URL", SqlDbType.NText));
					cmd.Parameters.Add(new SqlParameter("@DefaultSkinID", SqlDbType.Int, 4));
					cmd.Parameters.Add(new SqlParameter("@ParentAffiliateID", SqlDbType.Int, 4));
					cmd.Parameters.Add(new SqlParameter("@DisplayOrder", SqlDbType.Int, 4));
					cmd.Parameters.Add(new SqlParameter("@ExtensionData", SqlDbType.NText));
					cmd.Parameters.Add(new SqlParameter("@SEName", SqlDbType.NVarChar, 200));
					cmd.Parameters.Add(new SqlParameter("@SETitle", SqlDbType.NText));
					cmd.Parameters.Add(new SqlParameter("@SEAltText", SqlDbType.NText));
					cmd.Parameters.Add(new SqlParameter("@SEKeywords", SqlDbType.NText));
					cmd.Parameters.Add(new SqlParameter("@SEDescription", SqlDbType.NText));
					cmd.Parameters.Add(new SqlParameter("@Published", SqlDbType.TinyInt, 1));
					cmd.Parameters.Add(new SqlParameter("@Wholesale", SqlDbType.TinyInt, 1));
					cmd.Parameters.Add(new SqlParameter("@Deleted", SqlDbType.TinyInt, 1));
					cmd.Parameters.Add(new SqlParameter("@SaltKey", SqlDbType.Int, 4));

					cmd.Parameters["@AffiliateID"].Value = AffiliteID;

					if(EMail == null) cmd.Parameters["@EMail"].Value = DBNull.Value;
					else cmd.Parameters["@EMail"].Value = EMail;

					if(Password == null) cmd.Parameters["@Password"].Value = DBNull.Value;
					else cmd.Parameters["@Password"].Value = Password;

					if(Notes == null) cmd.Parameters["@Notes"].Value = DBNull.Value;
					else cmd.Parameters["@Notes"].Value = Notes;

					if(IsOnline == null) cmd.Parameters["@IsOnline"].Value = DBNull.Value;
					else cmd.Parameters["@IsOnline"].Value = IsOnline;

					if(FirstName == null) cmd.Parameters["@FirstName"].Value = DBNull.Value;
					else cmd.Parameters["@FirstName"].Value = FirstName;

					if(LastName == null) cmd.Parameters["@LastName"].Value = DBNull.Value;
					else cmd.Parameters["@LastName"].Value = LastName;

					if(Name == null) cmd.Parameters["@Name"].Value = DBNull.Value;
					else cmd.Parameters["@Name"].Value = Name;

					if(Company == null) cmd.Parameters["@Company"].Value = DBNull.Value;
					else cmd.Parameters["@Company"].Value = Company;

					if(Address1 == null) cmd.Parameters["@Address1"].Value = DBNull.Value;
					else cmd.Parameters["@Address1"].Value = Address1;

					if(Address2 == null) cmd.Parameters["@Address2"].Value = DBNull.Value;
					else cmd.Parameters["@Address2"].Value = Address2;

					if(Suite == null) cmd.Parameters["@Suite"].Value = DBNull.Value;
					else cmd.Parameters["@Suite"].Value = Suite;

					if(City == null) cmd.Parameters["@City"].Value = DBNull.Value;
					else cmd.Parameters["@City"].Value = City;

					if(State == null) cmd.Parameters["@State"].Value = DBNull.Value;
					else cmd.Parameters["@State"].Value = State;

					if(Zip == null) cmd.Parameters["@Zip"].Value = DBNull.Value;
					else cmd.Parameters["@Zip"].Value = Zip;

					if(Country == null) cmd.Parameters["@Country"].Value = DBNull.Value;
					else cmd.Parameters["@Country"].Value = Country;

					if(Phone == null) cmd.Parameters["@Phone"].Value = DBNull.Value;
					else cmd.Parameters["@Phone"].Value = Phone;

					if(WebSiteName == null) cmd.Parameters["@WebSiteName"].Value = DBNull.Value;
					else cmd.Parameters["@WebSiteName"].Value = WebSiteName;

					if(WebSiteDescription == null) cmd.Parameters["@WebSiteDescription"].Value = DBNull.Value;
					else cmd.Parameters["@WebSiteDescription"].Value = WebSiteDescription;

					if(URL == null) cmd.Parameters["@URL"].Value = DBNull.Value;
					else cmd.Parameters["@URL"].Value = URL;

					if(DefaultSkinID == null) cmd.Parameters["@DefaultSkinID"].Value = DBNull.Value;
					else cmd.Parameters["@DefaultSkinID"].Value = DefaultSkinID;

					if(ParentAffiliateID == null) cmd.Parameters["@ParentAffiliateID"].Value = DBNull.Value;
					else cmd.Parameters["@ParentAffiliateID"].Value = ParentAffiliateID;

					if(DisplayOrder == null) cmd.Parameters["@DisplayOrder"].Value = DBNull.Value;
					else cmd.Parameters["@DisplayOrder"].Value = DisplayOrder;

					if(ExtensionData == null) cmd.Parameters["@ExtensionData"].Value = DBNull.Value;
					else cmd.Parameters["@ExtensionData"].Value = ExtensionData;

					if(SEName == null) cmd.Parameters["@SEName"].Value = DBNull.Value;
					else cmd.Parameters["@SEName"].Value = SEName;

					if(SETitle == null) cmd.Parameters["@SETitle"].Value = DBNull.Value;
					else cmd.Parameters["@SETitle"].Value = SETitle;

					if(SEAltText == null) cmd.Parameters["@SEAltText"].Value = DBNull.Value;
					else cmd.Parameters["@SEAltText"].Value = SEAltText;

					if(SEKeywords == null) cmd.Parameters["@SEKeywords"].Value = DBNull.Value;
					else cmd.Parameters["@SEKeywords"].Value = SEKeywords;

					if(SEDescription == null) cmd.Parameters["@SEDescription"].Value = DBNull.Value;
					else cmd.Parameters["@SEDescription"].Value = SEDescription;

					if(Published == null) cmd.Parameters["@Published"].Value = DBNull.Value;
					else cmd.Parameters["@Published"].Value = Published;

					if(Wholesale == null) cmd.Parameters["@Wholesale"].Value = DBNull.Value;
					else cmd.Parameters["@Wholesale"].Value = Wholesale;

					if(Deleted == null) cmd.Parameters["@Deleted"].Value = DBNull.Value;
					else cmd.Parameters["@Deleted"].Value = Deleted;

					if(SaltKey == null) cmd.Parameters["@SaltKey"].Value = DBNull.Value;
					else cmd.Parameters["@SaltKey"].Value = SaltKey;

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

		public static bool EmailInUse(string email, int ExcludeAffiliateID)
		{
			string query = string.Format("select count(*) N from dbo.affiliate a with (nolock) inner join (select a.AffiliateID from Affiliate a with (nolock) left join AffiliateStore b with (nolock) on a.AffiliateID = b.AffiliateID where ({0} = 0 or StoreID = {1})) " +
				"B ON A.AffiliateID = B.AffiliateID where email = {2} and A.AffiliateID <> {3}", CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowAffiliateFiltering") == true, 1, 0), AppLogic.StoreID(), DB.SQuote(email), ExcludeAffiliateID);
			return (DB.GetSqlN(query) > 0);
		}

		#endregion

		#region Public Methods

		public string Update(string EMail, string Password, string Notes, object IsOnline, string FirstName, string LastName, string Name, string Company, string Address1, string Address2, string Suite, string City, string State, string Zip, string Country, string Phone, string WebSiteName, string WebSiteDescription, string URL, object TrackingOnly, object DefaultSkinID, object ParentAffiliateID, object DisplayOrder, string ExtensionData, string SEName, string SETitle, string SEAltText, string SEKeywords, string SEDescription, object Published, object Wholesale, object Deleted, object SaltKey)
		{
			string err = String.Empty;
			err = Update(m_Affiliateid, EMail, Password, Notes, IsOnline, FirstName, LastName, Name, Company, Address1, Address2, Suite, City, State, Zip, Country, Phone, WebSiteName, WebSiteDescription, URL, DefaultSkinID, ParentAffiliateID, DisplayOrder, ExtensionData, SEName, SETitle, SEAltText, SEKeywords, SEDescription, Published, Wholesale, Deleted, SaltKey);
			LoadFromDB(m_Affiliateid);
			return err;
		}

		#endregion

		#region Private Methods

		private void LoadFromDB(int AffiliateID)
		{
			using(var cn = new SqlConnection(DB.GetDBConn()))
			{
				cn.Open();
				using(var cmd = new SqlCommand())
				{
					cmd.Connection = cn;
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.CommandText = "dbo.aspdnsf_getAffiliate";

					cmd.Parameters.Add(new SqlParameter("@AffiliateID", SqlDbType.Int, 4));
					cmd.Parameters["@AffiliateID"].Value = AffiliateID;

					using(var dr = cmd.ExecuteReader())
					{
						if(dr.Read())
						{
							m_Affiliateid = DB.RSFieldInt(dr, "AffiliateID");
							m_Email = DB.RSField(dr, "EMail");
							m_Firstname = DB.RSField(dr, "FirstName");
							m_Lastname = DB.RSField(dr, "LastName");
							StoreID = DB.RSFieldInt(dr, "StoreID");
						}
					}
				}
			}
		}
		#endregion

		#region Public Properties
		public int AffiliateID
		{
			get { return m_Affiliateid; }
		}

		public string EMail
		{
			get { return m_Email; }
		}

		public string FirstName
		{
			get { return m_Firstname; }
		}

		public string LastName
		{
			get { return m_Lastname; }
		}

		private int m_storeid;

		public int StoreID
		{
			get { return m_storeid; }
			set { m_storeid = value; }
		}

		#endregion
	}
}
