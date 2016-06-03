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
	/// <summary>
	/// Generic database object class
	/// </summary>
	[Serializable]
	public class DatabaseObject
	{
		private int m_id;
		/// <summary>
		/// Gets or sets the ID
		/// </summary>
		public int ID
		{
			get { return m_id; }
			set { m_id = value; }
		}

		private string m_name;
		/// <summary>
		/// Gets or sets the Name
		/// </summary>
		public string Name
		{
			set { m_name = value; }
		}

		private string m_type;
		/// <summary>
		/// Gets or sets the Type
		/// </summary>
		public string Type
		{
			get { return m_type; }
			set { m_type = value; }
		}
	}

	/// <summary>
	/// Mapped object class
	/// </summary>
	public class MappedObject : DatabaseObject
    {

        private int m_storeid;
        /// <summary>
        /// Gets or sets the StoreID
        /// </summary>
        public int StoreID
        {
            get { return m_storeid; }
            set { m_storeid = value; }
        }

        private bool m_ismapped;

        /// <summary>
        /// Gets or sets if this object is mapped
        /// </summary>
        public bool IsMapped
        {
            get { return m_ismapped; }
            set { m_ismapped = value; }
        }

        /// <summary>
        /// Gets the mapped object
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="type"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static MappedObject Find(int storeId, string type, int id)
        {
            MappedObject map = null;

            Action<IDataReader> readAction = (rs) =>
            {
                if (rs.Read())
                {
                    map = new MappedObject();
                    map.ID = rs.FieldInt("ID");
                    map.StoreID = storeId;
                    map.Type = rs.Field("EntityType");
                    map.Name = rs.Field("Name");
                    map.IsMapped = rs.FieldBool("Mapped");
                }
            };

            string findQuery = "dbo.aspdnsf_GetMappedObject @StoreId = {0}, @EntityType = {1}, @EntityID = {2}".FormatWith(storeId, type.DBQuote(), id);

            DB.UseDataReader(findQuery, readAction);

            return map;
        }

        /// <summary>
        /// Save mapping for this entity
        /// </summary>
        public void Save()
        {
            var cmd = new SqlCommand("aspdnsf_SaveMap");
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@StoreId", SqlDbType.Int) { Value = this.StoreID });
            cmd.Parameters.Add(new SqlParameter("@EntityType", SqlDbType.NVarChar, 30) { Value = this.Type });

            var pEntityId = new SqlParameter("@EntityId", SqlDbType.Int) { Value = this.ID };
            var pMap = new SqlParameter("@Map", SqlDbType.Bit) { Value = this.IsMapped };
            cmd.Parameters.Add(pEntityId);
            cmd.Parameters.Add(pMap);

            using (var con = DB.dbConn())
            {
                con.Open();
                using (cmd)
                {
                    cmd.Connection = con;
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }

    /// <summary>
    /// Generic paging information struct
    /// </summary>
    public struct PagingInfo
    {
        private int m_totalcount;
        public int TotalCount
        {
            get { return m_totalcount; }
            set { m_totalcount = value; }
        }
        private int m_pagesize;
        public int PageSize
        {
            get { return m_pagesize; }
            set { m_pagesize = value; }
        }
        private int m_currentpage;
        public int CurrentPage
        {
            get { return m_currentpage; }
            set { m_currentpage = value; }
        }
        private int m_totalpages;
        public int TotalPages
        {
            get { return m_totalpages; }
            set { m_totalpages = value; }
        }
        private int m_startindex;
        public int StartIndex
        {
            get { return m_startindex; }
            set { m_startindex = value; }
        }
        private int m_endindex;
        public int EndIndex 
        {
            get { return m_endindex; }
            set { m_endindex = value; }
        }
    }   

    /// <summary>
    /// Represents a collection of MappedObjects
    /// </summary>
    public class MappedObjectCollection : List<MappedObject>
    {
        private int m_storeid;
        public int StoreID
        {
            get { return m_storeid; }
            set { m_storeid = value; }
        }
        private string m_entitytype;
        public string EntityType
        {
            get { return m_entitytype; }
            set { m_entitytype = value; }
        }
        private PagingInfo m_pageinfo;
        public PagingInfo PageInfo
        {
            get { return m_pageinfo; }
            set { m_pageinfo = value; }
        }

        public static MappedObjectCollection GetObjects(int storeId, string entityType)
        {
            return GetObjects(storeId, entityType, string.Empty, -1, -1);
        }

        /// <summary>
        /// Gets the mapped objects
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="entityType"></param>
        /// <param name="searchFilter"></param>
        /// <param name="pageSize"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public static MappedObjectCollection GetObjects(int storeId,
            string entityType,
            string searchFilter,
            int pageSize,
            int page)
        {
            var mapping = new MappedObjectCollection();
            mapping.StoreID = storeId;
            mapping.EntityType = entityType;

			using(var cmd = new SqlCommand("aspdnsf_GetMappedObjects"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add(new SqlParameter("@StoreId", SqlDbType.Int) { Value = storeId });
				cmd.Parameters.Add(new SqlParameter("@EntityType", SqlDbType.NVarChar, 30) { Value = entityType });

				// nullable parameters
				var pSearchFilter = new SqlParameter("@SearchFilter", SqlDbType.NVarChar, 30) { Value = DBNull.Value };
				var pPageSize = new SqlParameter("@PageSize", SqlDbType.Int) { Value = DBNull.Value };
				var pPage = new SqlParameter("@Page", SqlDbType.Int) { Value = DBNull.Value };

				if(!string.IsNullOrEmpty(searchFilter))
				{
					pSearchFilter.Value = searchFilter;
				}

				if(pageSize > 0 && page > 0)
				{
					pPageSize.Value = pageSize;
					pPage.Value = page;
				}

				cmd.Parameters.Add(pSearchFilter);
				cmd.Parameters.Add(pPageSize);
				cmd.Parameters.Add(pPage);

				Action<IDataReader> readAction = (rs) =>
				{
				// first result set is the paging data
				if(rs.Read())
					{
						var nfo = new PagingInfo();
						nfo.TotalCount = rs.FieldInt("TotalCount");
						nfo.PageSize = rs.FieldInt("PageSize");
						nfo.CurrentPage = rs.FieldInt("CurrentPage");
						nfo.TotalPages = rs.FieldInt("TotalPages");
						nfo.StartIndex = rs.FieldInt("StartIndex");
						nfo.EndIndex = rs.FieldInt("EndIndex");

						mapping.PageInfo = nfo;
					}

					rs.NextResult();

				// next is the actual result set
				while(rs.Read())
					{
						var map = new MappedObject();
						map.ID = rs.FieldInt("ID");
						map.StoreID = storeId;
						map.Type = rs.Field("EntityType");
						map.Name = rs.Field("Name");
						map.IsMapped = rs.FieldBool("Mapped");
						mapping.Add(map);
					}
				};

				DB.UseDataReader(cmd, readAction);
			}
            return mapping;
        }
    }
}
