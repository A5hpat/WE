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
	/// <summary>
	/// Retreives Collection of products matching intersection of all criteria.
	/// Products are placed in DataSet Tables[0].
	/// The Default instance of object returns ALL products with page number 1, page size 0 (no paging), default variant only, in product name sort order
	/// published only defaults to yes for store side, no for admin site
	/// To add additional constraints, add additional filter calls before doing the "LoadFromDB" call
	/// use DB. field accessor routines to get product fields in a typesafe manner.
	/// generally, a filter id of "0" means to "not" filter by that criteria (i.e. that disables that filter)
	/// </summary>
	public class ProductCollection : IDisposable
	{
		private DataSet m_Products;
		private int m_CategoryID = 0;
		private int m_SectionID = 0;
		private int m_ManufacturerID = 0;
		private int m_DistributorID = 0;
		private int m_GenreID = 0;
		private int m_VectorID = 0;
		private int m_LocaleID = 0;
		private int m_AffiliateID = 0;
		private int m_CustomerLevelID = 0;
		private int m_ProductTypeID = 0;
		private bool m_PublishedOnly = true;
		private bool m_OnSaleOnly = false;
		private String m_SearchMatch = String.Empty; // used like search. do NOT include % wildcards, those are added by this class
		private bool m_SearchDescriptionAndSummaryFields = false;
		private bool m_ReturnAllVariants = false;
		private bool m_ExcludeKits = false;
		private bool m_ExcludeSystemProducts = true;
		private int m_PageNum = 1;
		private int m_PageSize = 0;
		private int m_NumProducts = 0; // set AFTER LoadFromDB call is made (may equal PageSize if paging is on), so you cannot use NumProducts to necessarily determine the total # of products with paging enabled
		private decimal m_MinPrice = System.Decimal.Zero;
		private decimal m_MaxPrice = System.Decimal.Zero;

		private string m_SearchIndex = "";
		private string m_SortBy = "Name";
		private string m_SortOrder = "ASC";

		private int m_InventoryFilter = 0;

		public ProductCollection()
			: this(String.Empty, 0)
		{ }

		public ProductCollection(String EntityName, int EntityFilterID)
		{
			m_PublishedOnly = !AppLogic.IsAdminSite;
			AddEntityFilter(EntityName, EntityFilterID);
		}
		
		public void AddEntityFilter(String EntityName, int EntityFilterID)
		{
			switch(EntityName.Trim().ToUpperInvariant())
			{
				case "CATEGORY":
					m_CategoryID = EntityFilterID;
					break;
				case "SECTION":
					m_SectionID = EntityFilterID;
					break;
				case "MANUFACTURER":
					m_ManufacturerID = EntityFilterID;
					break;
				case "DISTRIBUTOR":
					m_DistributorID = EntityFilterID;
					break;
				case "GENRE":
					m_GenreID = EntityFilterID;
					break;
				case "VECTOR":
					m_VectorID = EntityFilterID;
					break;
				case "AFFILIATE":
					m_AffiliateID = EntityFilterID;
					break;
				case "CUSTOMERLEVEL":
					m_CustomerLevelID = EntityFilterID;
					break;
			}
		}

		public DataSet LoadFromDB()
		{

			using(var dbconn = new SqlConnection())
			{
				dbconn.ConnectionString = DB.GetDBConn();
				dbconn.Open();

				using(var cmd = new SqlCommand())
				{
					cmd.Connection = dbconn;
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.CommandText = "aspdnsf_GetProducts";

					if(CategoryID != 0)
					{
						cmd.Parameters.Add(new SqlParameter("@CategoryID", SqlDbType.Int));
						cmd.Parameters["@CategoryID"].Value = m_CategoryID;
					}

					if(SectionID != 0)
					{
						cmd.Parameters.Add(new SqlParameter("@SectionID", SqlDbType.Int));
						cmd.Parameters["@SectionID"].Value = m_SectionID;
					}

					if(ManufacturerID != 0)
					{
						cmd.Parameters.Add(new SqlParameter("@ManufacturerID", SqlDbType.Int));
						cmd.Parameters["@ManufacturerID"].Value = m_ManufacturerID;
					}

					if(DistributorID != 0)
					{
						cmd.Parameters.Add(new SqlParameter("@distributorID", SqlDbType.Int));
						cmd.Parameters["@distributorID"].Value = m_DistributorID;
					}

					if(GenreID != 0)
					{
						cmd.Parameters.Add(new SqlParameter("@genreID", SqlDbType.Int));
						cmd.Parameters["@genreID"].Value = m_GenreID;
					}

					if(VectorID != 0)
					{
						cmd.Parameters.Add(new SqlParameter("@vectorID", SqlDbType.Int));
						cmd.Parameters["@vectorID"].Value = m_VectorID;
					}

					if(!AppLogic.IsAdminSite && AppLogic.AppConfigBool("FilterProductsByLocaleSetting"))
					{
						if(m_LocaleID != 0)
						{
							cmd.Parameters.Add(new SqlParameter("@LocaleID", SqlDbType.Int));
							cmd.Parameters["@LocaleID"].Value = m_LocaleID;
						}
					}

					if(AppLogic.IsAdminSite)
					{
						cmd.Parameters.Add(new SqlParameter("@InventoryFilter", SqlDbType.Int));
						cmd.Parameters["@InventoryFilter"].Value = -1;
					}
					else if(AppLogic.AppConfigUSInt("MinimumInventoryFilter") > m_InventoryFilter)
					{
						cmd.Parameters.Add(new SqlParameter("@InventoryFilter", SqlDbType.Int));
						cmd.Parameters["@InventoryFilter"].Value = AppLogic.AppConfigUSInt("MinimumInventoryFilter");
					}
					else
					{
						cmd.Parameters.Add(new SqlParameter("@InventoryFilter", SqlDbType.Int));
						cmd.Parameters["@InventoryFilter"].Value = m_InventoryFilter;
					}

					// leave null for no filtering. 0 passed in this case will filter to customer level 0!!
					// This is the only param that has this special treatment
					if(!AppLogic.IsAdminSite && AppLogic.AppConfigBool("FilterProductsByCustomerLevel"))
					{

						// we are filtering normally:
						cmd.Parameters.Add(new SqlParameter("@CustomerLevelID", SqlDbType.Int));
						cmd.Parameters["@CustomerLevelID"].Value = m_CustomerLevelID;
					}

					if(!AppLogic.IsAdminSite && AffiliateID != 0 && AppLogic.AppConfigBool("FilterProductsByAffiliate"))
					{
						cmd.Parameters.Add(new SqlParameter("@AffiliateID", SqlDbType.Int));
						cmd.Parameters["@AffiliateID"].Value = m_AffiliateID;
					}

					if(ProductTypeID != 0)
					{
						cmd.Parameters.Add(new SqlParameter("@ProductTypeID", SqlDbType.Int));
						cmd.Parameters["@ProductTypeID"].Value = m_ProductTypeID;
					}

					cmd.Parameters.Add(new SqlParameter("@ViewType", SqlDbType.Int));
					cmd.Parameters["@ViewType"].Value = CommonLogic.IIF(m_ReturnAllVariants || m_SearchMatch.Length != 0, 0, 1);

					cmd.Parameters.Add(new SqlParameter("@ExcludeKits", SqlDbType.Int));
					cmd.Parameters["@ExcludeKits"].Value = CommonLogic.IIF(m_ExcludeKits, 1, 0);

					cmd.Parameters.Add(new SqlParameter("@ExcludeSysProds", SqlDbType.Int));
					cmd.Parameters["@ExcludeSysProds"].Value = CommonLogic.IIF(m_ExcludeSystemProducts, 1, 0);

					if(PageSize != 0)
					{
						if(PageNum == 0)
						{
							PageNum = 1;
						}
						cmd.Parameters.Add(new SqlParameter("@pagesize", SqlDbType.Int));
						cmd.Parameters["@pagesize"].Value = m_PageSize;

						cmd.Parameters.Add(new SqlParameter("@pagenum", SqlDbType.Int));
						cmd.Parameters["@pagenum"].Value = m_PageNum;
					}
					else
					{
						// no paging
						cmd.Parameters.Add(new SqlParameter("@pagesize", SqlDbType.Int));
						cmd.Parameters["@pagesize"].Value = 9999999;

						cmd.Parameters.Add(new SqlParameter("@pagenum", SqlDbType.Int));
						cmd.Parameters["@pagenum"].Value = m_PageNum;
					}

					cmd.Parameters.Add(new SqlParameter("@StatsFirst", SqlDbType.Int));
					cmd.Parameters["@StatsFirst"].Value = 0;

					if(SearchMatch.Length != 0)
					{
						cmd.Parameters.Add(new SqlParameter("@SearchStr", SqlDbType.NVarChar));
						cmd.Parameters["@SearchStr"].Value = SearchMatch;
						cmd.Parameters.Add(new SqlParameter("@ExtSearch", SqlDbType.Int));
						cmd.Parameters["@ExtSearch"].Value = CommonLogic.IIF(m_SearchDescriptionAndSummaryFields, 1, 0);
					}

					cmd.Parameters.Add(new SqlParameter("@PublishedOnly", SqlDbType.Int));
					cmd.Parameters["@PublishedOnly"].Value = CommonLogic.IIF(m_PublishedOnly, 1, 0);

					cmd.Parameters.Add(new SqlParameter("@OnSaleOnly", SqlDbType.Int));
					cmd.Parameters["@OnSaleOnly"].Value = CommonLogic.IIF(m_OnSaleOnly, 1, 0);

					m_Products = new DataSet();
					SqlDataAdapter da = new SqlDataAdapter(cmd);
					da.Fill(m_Products, "Table");

					DataRow PagingStatsRow = m_Products.Tables[1].Rows[0];

					m_NumProducts = m_Products.Tables[0].Rows.Count;
				}
			}
			return m_Products;
		}
		
		public void Dispose()
		{
			if(m_Products != null)
			{
				m_Products.Dispose();
				m_Products = null;
			}
		}

		public int CategoryID
		{
			get
			{
				return m_CategoryID;
			}
			set
			{
				m_CategoryID = value;
			}
		}

		public int PageSize
		{
			get
			{
				return m_PageSize;
			}
			set
			{
				m_PageSize = value;
			}
		}

		// CAUTION: if paging is on this is probably NOT the total # of products matching the filter, it's the # returned in the requested page
		public int NumProducts
		{
			get
			{
				return m_NumProducts;
			}
			set
			{
				m_NumProducts = value;
			}
		}

		public int PageNum
		{
			get
			{
				return m_PageNum;
			}
			set
			{
				m_PageNum = value;
			}
		}

		public bool ReturnAllVariants
		{
			get
			{
				return m_ReturnAllVariants;
			}
			set
			{
				m_ReturnAllVariants = value;
			}
		}

		public bool PublishedOnly
		{
			get
			{
				return m_PublishedOnly;
			}
			set
			{
				m_PublishedOnly = value;
			}
		}

		public bool SearchDescriptionAndSummaryFields
		{
			get
			{
				return m_SearchDescriptionAndSummaryFields;
			}
			set
			{
				m_SearchDescriptionAndSummaryFields = value;
			}
		}

		public int SectionID
		{
			get
			{
				return m_SectionID;
			}
			set
			{
				m_SectionID = value;
			}
		}

		public int ProductTypeID
		{
			get
			{
				return m_ProductTypeID;
			}
			set
			{
				m_ProductTypeID = value;
			}
		}

		public int DistributorID
		{
			get
			{
				return m_DistributorID;
			}
			set
			{
				m_DistributorID = value;
			}
		}

		public int GenreID
		{
			get
			{
				return m_GenreID;
			}
			set
			{
				m_GenreID = value;
			}
		}

		public int VectorID
		{
			get
			{
				return m_VectorID;
			}
			set
			{
				m_VectorID = value;
			}
		}

		public int ManufacturerID
		{
			get
			{
				return m_ManufacturerID;
			}
			set
			{
				m_ManufacturerID = value;
			}
		}

		public int AffiliateID
		{
			get
			{
				return m_AffiliateID;
			}
			set
			{
				m_AffiliateID = value;
			}
		}

		public String SearchMatch
		{
			get
			{
				return m_SearchMatch;
			}
			set
			{
				m_SearchMatch = value;
			}
		}
	}
}
