// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Data.SqlClient;

namespace AspDotNetStorefrontCore
{
	public class Entity
	{
		#region Private Variables

		private int m_id;
		private int m_parentid;
		private bool m_published;
		private bool m_deleted;
		private string m_entitytype;
		private string m_name;
		private string m_sekeywords;
		private string m_sedescription;
		private string m_setitle;
		private string m_xmlpackage;
		private string m_sename;
		private Address m_entityaddress;

		#endregion

		#region Constructors

		public Entity(int entityIdToLoad, string entityTypeToLoad)
		{
			m_entitytype = LoadEntityType(entityTypeToLoad);

			using(var conn = new SqlConnection(DB.GetDBConn()))
			{
				conn.Open();
				using(var rs = DB.GetRS(@"SELECT * FROM [dbo].[{0}] WITH (NOLOCK) WHERE {0}ID=".FormatWith(m_entitytype) + entityIdToLoad, conn))
				{
					if(rs.Read())
					{
						m_id = DB.RSFieldInt(rs, "{0}ID".FormatWith(m_entitytype));
						m_name = DB.RSField(rs, "Name");
						m_sekeywords = DB.RSField(rs, "SEKeywords");
						m_sedescription = DB.RSField(rs, "SEDescription");
						m_setitle = DB.RSField(rs, "SETitle");
						m_parentid = DB.RSFieldInt(rs, "Parent{0}ID".FormatWith(m_entitytype));
						m_xmlpackage = DB.RSField(rs, "XmlPackage");
						m_sename = DB.RSField(rs, "SEName");
						m_published = DB.RSFieldBool(rs, "Published");
						m_deleted = DB.RSFieldBool(rs, "Deleted");
						if(HasAddress)
						{
							m_entityaddress = new Address();

							m_entityaddress.Address1 = DB.RSField(rs, "Address1");
							m_entityaddress.Address2 = DB.RSField(rs, "Address2");
							m_entityaddress.Suite = DB.RSField(rs, "Suite");
							m_entityaddress.City = DB.RSField(rs, "City");
							m_entityaddress.State = DB.RSField(rs, "State");
							m_entityaddress.Zip = DB.RSField(rs, "ZipCode");
							m_entityaddress.Country = DB.RSField(rs, "Country");
							m_entityaddress.Phone = DB.RSField(rs, "Phone");
							m_entityaddress.Fax = DB.RSField(rs, "Fax");
							m_entityaddress.Url = DB.RSField(rs, "URL");
							m_entityaddress.EMail = DB.RSField(rs, "EMail");
						}
					}

					rs.Close();
					rs.Dispose();
				}
				conn.Close();
				conn.Dispose();
			}
		}

		#endregion

		#region Public Properties

		public string EntityType { get { return LoadEntityType(m_entitytype); } set { m_entitytype = value; } }
		public int ID { get { return m_id; } set { m_id = value; } }
		public string Name { get { return m_name; } set { m_name = value; } }
		public string LocaleName { get { return LocalizedValue(m_name); } }
		public string SEKeywords { get { return m_sekeywords; } set { m_sekeywords = value; } }
		public string SEDescription { get { return m_sedescription; } set { m_sedescription = value; } }
		public string SETitle { get { return m_setitle; } set { m_setitle = value; } }
		public int ParentID { get { return m_parentid; } set { m_parentid = value; } }
		public string XmlPackage { get { return m_xmlpackage; } set { m_xmlpackage = value; } }
		public string SEName { get { return m_sename; } set { m_sename = value; } }
		public bool HasAddress { get { return m_entitytype.EqualsIgnoreCase("Affiliate") || m_entitytype.EqualsIgnoreCase("Distributor") || m_entitytype.EqualsIgnoreCase("Manufacturer"); } }
		public int NumChildren { get { return DB.GetSqlN("SELECT COUNT(*) AS N FROM [dbo].[{0}] WITH (NOLOCK) WHERE Parent{0}ID=".FormatWith(EntityType) + m_id.ToString()); } }
		public bool Published { get { return m_published; } set { m_published = value; } }
		public bool Deleted { get { return m_deleted; } set { m_deleted = value; } }
		#endregion

		#region Public Methods

		public string LocalizedValue(string unlocalizedValue)
		{
			var customer = AppLogic.GetCurrentCustomer();
			var localizedValue = unlocalizedValue;

			if(customer != null)
				localizedValue = XmlCommon.GetLocaleEntry(unlocalizedValue, customer.LocaleSetting, true);
			else
				localizedValue = XmlCommon.GetLocaleEntry(unlocalizedValue, Localization.GetDefaultLocale(), true);

			return localizedValue;
		}

		#endregion

		#region Static Methods

		public static string LoadEntityType(string EntityToLoad)
		{
			var EntityType = "Category";

			switch(EntityToLoad.ToLowerInvariant())
			{
				case "manufacturer":
					EntityType = "Manufacturer";
					break;
				case "distributor":
					EntityType = "Distributor";
					break;
				case "section":
				case "department":
					EntityType = "Section";
					break;
				case "vector":
					EntityType = "Vector";
					break;
				case "genre":
					EntityType = "Genre";
					break;
				default:
					EntityType = "Category";
					break;
			}

			return EntityType;
		}

		#endregion
	}
}
