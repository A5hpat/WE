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
using System.Linq;
using System.Web;

namespace AspDotNetStorefrontCore
{
	public enum StoreEnvironment
    {
        Production = 0,
        Staging = 1,
        Development = 2,
        Unknown
    }

    [Serializable]
    public class Store
    {
		public List<StoreEntityMap> EntityMaps
		{ get; private set; }

		public int StoreID
		{ get; set; }

		public Guid StoreGuid
		{ get; set; }

		/// <summary>
		/// The URI for the production instance of this site
		/// </summary>
		public string ProductionURI
		{ get; set; }

		/// <summary>
		/// The directory path for the production instance of this site
		/// </summary>
		public string ProductionDirectoryPath
		{ get; set; }

		/// <summary>
		/// The Port for the production instance of this site
		/// </summary>
		public string ProductionPort
		{ get; set; }

		/// <summary>
		/// The URI for the staging instance of this site
		/// </summary>
		public string StagingURI
		{ get; set; }

		/// <summary>
		/// The directory path for the staging instance of this site
		/// </summary>
		public string StagingDirectoryPath
		{ get; set; }

		/// <summary>
		/// The Port for the staging instance of this site
		/// </summary>
		public string StagingPort
		{ get; set; }

		/// <summary>
		/// The URI for the development instance of this site
		/// </summary>
		public string DevelopmentURI
		{ get; set; }

		/// <summary>
		/// The directory path for the development instance of this site
		/// </summary>
		public string DevelopmentDirectoryPath
		{ get; set; }

		/// <summary>
		/// The Port for the development instance of this site
		/// </summary>
		public string DevelopmentPort
		{ get; set; }

		/// <summary>
		/// Name of the Store
		/// </summary>
		public string Name
		{ get; set; }

		/// <summary>
		/// Description of the store
		/// </summary>
		public string Description
		{ get; set; }

		/// <summary>
		/// Flag indicating whether or not the site is accessible to customers
		/// </summary>
		public bool Published
		{ get; set; }

		/// <summary>
		/// Flag indicating weather store has been delete
		/// Note: This is a soft delete, nuke for a hard delete
		/// </summary>
		public bool Deleted
		{ get; set; }

		/// <summary>
		/// Inidcator that site is default in the event another
		/// store cannot be parsed from the URL
		/// </summary>
		public bool IsDefault
		{ get; set; }

		/// <summary>
		/// The skin to be used by the store
		/// </summary>
		public int SkinID
		{ get; set; }

		/// <summary>
		/// The date the store was created
		/// </summary>
		public DateTime CreatedOn
		{ get; set; }

		[NonSerialized]
		SqlCommand _cmdSaveList;

		SqlCommand sqlUpdateList
		{
			get
			{
				if(_cmdSaveList != null)
					return _cmdSaveList;
				string SQL =
 @"
IF @GUID IS NULL 
BEGIN
	INSERT Store(StoreGUID, ProductionURI, ProductionDirectoryPath, ProductionPort, StagingURI, StagingDirectoryPath, StagingPort, DevelopmentURI, DevelopmentDirectoryPath, DevelopmentPort, Name, Description, IsDefault, Published, SkinID, CreatedOn)
	VALUES (NewID(), @ProductionURI, @ProductionDirectoryPath, @ProductionPort, @StagingURI, @StagingDirectoryPath, @StagingPort, @DevelopmentURI, @DevelopmentDirectoryPath, @DevelopmentPort, @Name, @Description, @IsDefault, @Published, @SkinID, Getdate())
END
ELSE
BEGIN
	UPDATE Store 
		SET 
        ProductionURI = ISNULL(@ProductionURI, ProductionURI), 
        ProductionDirectoryPath = ISNULL(@ProductionDirectoryPath, ProductionDirectoryPath), 
		ProductionPort = ISNULL(@ProductionPort, ProductionPort),
        StagingURI = ISNULL(@StagingURI, StagingURI), 
        StagingDirectoryPath = ISNULL(@StagingDirectoryPath, StagingDirectoryPath), 
		StagingPort = ISNULL(@StagingPort, StagingPort),
        DevelopmentURI = ISNULL(@DevelopmentURI, DevelopmentURI), 
        DevelopmentDirectoryPath = ISNULL(@DevelopmentDirectoryPath, DevelopmentDirectoryPath), 
		DevelopmentPort = ISNULL(@DevelopmentPort, DevelopmentPort),
		[Name] = ISNULL(@Name, [Name]),
		Description = ISNULL(@Description, Description),
        Deleted = ISNULL(@Deleted, Deleted),
        IsDefault = ISNULL(@IsDefault, IsDefault), 
        Published = ISNULL(@Published, Published), 
		SkinID = ISNULL(@SkinID, SkinID)
	WHERE
		StoreGUID = @GUID
END
"
;
				_cmdSaveList = new SqlCommand(SQL);
				_cmdSaveList.Parameters.Add(new SqlParameter("@GUID", DBNull.Value));
				_cmdSaveList.Parameters.Add(new SqlParameter("@Name", DBNull.Value));
				_cmdSaveList.Parameters.Add(new SqlParameter("@ProductionURI", DBNull.Value));
				_cmdSaveList.Parameters.Add(new SqlParameter("@ProductionDirectoryPath", DBNull.Value));
				_cmdSaveList.Parameters.Add(new SqlParameter("@ProductionPort", DBNull.Value));
				_cmdSaveList.Parameters.Add(new SqlParameter("@StagingURI", DBNull.Value));
				_cmdSaveList.Parameters.Add(new SqlParameter("@StagingDirectoryPath", DBNull.Value));
				_cmdSaveList.Parameters.Add(new SqlParameter("@StagingPort", DBNull.Value));
				_cmdSaveList.Parameters.Add(new SqlParameter("@DevelopmentURI", DBNull.Value));
				_cmdSaveList.Parameters.Add(new SqlParameter("@DevelopmentDirectoryPath", DBNull.Value));
				_cmdSaveList.Parameters.Add(new SqlParameter("@DevelopmentPort", DBNull.Value));
				_cmdSaveList.Parameters.Add(new SqlParameter("@Description", DBNull.Value));
				_cmdSaveList.Parameters.Add(new SqlParameter("@Published", DBNull.Value));
				_cmdSaveList.Parameters.Add(new SqlParameter("@IsDefault", DBNull.Value));
				_cmdSaveList.Parameters.Add(new SqlParameter("@SkinID", DBNull.Value));
				_cmdSaveList.Parameters.Add(new SqlParameter("@Deleted", DBNull.Value));
				return _cmdSaveList;
			}
		}

		public Store()
        {
            EntityMaps = new List<StoreEntityMap>();
        }

        /// <summary>
        /// Commits any changes to the object to the database
        /// </summary>
        public void Save()
        {
            using (var xCon = new SqlConnection(DB.GetDBConn()))
            {
                if (StoreGuid.ToString() != new Guid().ToString())
                    sqlUpdateList.Parameters["@GUID"].Value = StoreGuid;
                sqlUpdateList.Parameters["@Name"].Value = Name;
                sqlUpdateList.Parameters["@Description"].Value = Description == null ? " " : Description;
                if (ProductionURI != null)
                    sqlUpdateList.Parameters["@ProductionURI"].Value = ProductionURI;
				if(ProductionDirectoryPath != null)
					sqlUpdateList.Parameters["@ProductionDirectoryPath"].Value = ProductionDirectoryPath;
				if(ProductionPort != null)
					sqlUpdateList.Parameters["@ProductionPort"].Value = ProductionPort;
				if(StagingURI != null)
					sqlUpdateList.Parameters["@StagingURI"].Value = StagingURI;
				if(StagingDirectoryPath != null)
					sqlUpdateList.Parameters["@StagingDirectoryPath"].Value = StagingDirectoryPath;
				if(StagingPort != null)
					sqlUpdateList.Parameters["@StagingPort"].Value = StagingPort;
				if (DevelopmentURI != null)
                    sqlUpdateList.Parameters["@DevelopmentURI"].Value = DevelopmentURI;
				if(DevelopmentDirectoryPath != null)
					sqlUpdateList.Parameters["@DevelopmentDirectoryPath"].Value = DevelopmentDirectoryPath;
				if(DevelopmentPort != null)
					sqlUpdateList.Parameters["@DevelopmentPort"].Value = DevelopmentPort;
				sqlUpdateList.Parameters["@IsDefault"].Value = IsDefault ? 1 : 0;
                sqlUpdateList.Parameters["@Published"].Value = Published ? 1 : 0;
                sqlUpdateList.Parameters["@Deleted"].Value = Deleted ? 1 : 0;
                sqlUpdateList.Parameters["@SkinID"].Value = SkinID;
                xCon.Open();
                sqlUpdateList.Connection = xCon;
                sqlUpdateList.ExecuteNonQuery();
            }
        }

        public static int StoreCount
        {
            get
            {
                if (_lstStores == null)
                {
                    return DB.GetSqlN("SELECT COUNT(*) N FROM Store WHERE Deleted = 0");
                }
                else
                {
                    return _lstStores.Count;
                }
            }
        }

        public static bool IsMultiStore
        {
            get { return Store.StoreCount > 1; }
        }

        public static void resetStoreCache()
        {
            GetStoreList(true);
            CacheMappings();
        }

        private static List<Store> _lstStores;

        public static String GetStoreName(int StoreId)
        {
            Store st = GetStoreList().FirstOrDefault(s => s.StoreID == StoreId);
            if (st != null)
	        {
		        return st.Name;
	        }
            return string.Empty;
        }

        /// <summary>
        /// Retrieves all stores
        /// </summary>
        public static List<Store> GetStoreList()
        {
            return GetStoreList(false);
        }

        /// <summary>
        /// Retrieves all stores
        /// </summary>
        /// <param name="refresh">Whether or not to refresh the cache</param>
        /// <returns></returns>
        public static List<Store> GetStoreList(bool refresh)
        {
            if (_lstStores != null && !refresh)
            {
                return _lstStores;
            }

            _lstStores = GetStores(false);

            return _lstStores;

        }

        /// <summary>
        /// Returns a generic list for the stores
        /// </summary>
        /// <param name="all"></param>
        /// <returns></returns>
        public static List<Store> GetStores(bool all)
        {
            var allStores = new List<Store>();

            var sql = @"SELECT StoreID, 
							StoreGUID, 
							Name, 
							ProductionURI, 
							ProductionDirectoryPath, 
							ProductionPort,
							StagingURI, 
							StagingDirectoryPath,
							StagingPort, 
							DevelopmentURI, 
							DevelopmentDirectoryPath, 
							DevelopmentPort,
							Description, 
							Published, 
							Deleted, 
							SkinID, 
							IsDefault, 
							CreatedOn 
						FROM Store";

            if (!all)
            {
                sql += " WHERE Deleted = 0 and Published = 1";
            }

            using (var xCon = new SqlConnection(DB.GetDBConn()))
            {
                xCon.Open();
				using(var xRdr = DB.GetRS(sql, xCon))
				{
					while(xRdr.Read())
					{
						Store xObj = new Store();
						xObj.Initialize(xRdr);

						allStores.Add(xObj);
					}
				}
            }

            return allStores;
        }

		/// <summary>
		/// Retrieves a store by its id
		/// </summary>
		/// <param name="id">the store id</param>
		/// <returns></returns>
		public static Store GetStoreById(int id)
		{
			return GetStores(true).Find(s => s.StoreID == id);
		}

		/// <summary>
		/// Gets the application's default store
		/// </summary>
		/// <returns></returns>
		public static Store GetDefaultStore()
		{
			return GetStores(false).Find(s => s.IsDefault == true);
		}

		/// <summary>
		/// Returns the environment (Development, Staging, Live) for the current site base on urls in the store table.
		/// </summary>
		public static StoreEnvironment GetEnvironment(string currentDomain)
		{
			var currentStore = GetStoreById(AppLogic.StoreID());

			if(StringComparer.OrdinalIgnoreCase.Equals(currentStore.ProductionURI, currentDomain))
				return StoreEnvironment.Production;
			else if(StringComparer.OrdinalIgnoreCase.Equals(currentStore.StagingURI, currentDomain))
				return StoreEnvironment.Staging;
			else if(StringComparer.OrdinalIgnoreCase.Equals(currentStore.DevelopmentURI, currentDomain))
				return StoreEnvironment.Development;
			else
				return StoreEnvironment.Unknown;
		}

		/// <summary>
		/// Gets the domain from the store table for a given store and environment (Development, Staging, Live)
		/// </summary>
		public static string GetStoreDomainByEnvironment(StoreEnvironment environment, int storeId)
		{
			var store = GetStoreById(storeId);

			switch(environment)
			{
				case StoreEnvironment.Development:
					return store.DevelopmentURI;
				case StoreEnvironment.Staging:
					return store.StagingURI;
			}

			return store.ProductionURI;
		}

		/// <summary>
		/// Gets the directory path a.k.a. virtual directory from the store table for a given store and environment (Development, Staging, Live)
		/// </summary>
		public static string GetStoreDirectoryByEnvironment(StoreEnvironment environment, int storeId)
		{
			var store = GetStoreById(storeId);

			switch(environment)
			{
				case StoreEnvironment.Development:
					return store.DevelopmentDirectoryPath;
				case StoreEnvironment.Staging:
					return store.StagingDirectoryPath;
			}

			return store.ProductionDirectoryPath;
		}

		/// <summary>
		/// Gets the port from the store table for a given store and environment (Development, Staging, Production)
		/// </summary>
		public static string GetStorePortByEnvironment(StoreEnvironment environment, int storeId)
		{
			var store = GetStoreById(storeId);

			switch(environment)
			{
				case StoreEnvironment.Development:
					return store.DevelopmentPort;
				case StoreEnvironment.Staging:
					return store.StagingPort;
			}

			return store.ProductionPort;
		}

		/// <summary>
		/// Initializes the members of this store based on the datareader values
		/// </summary>
		/// <param name="xRdr"></param>
		private void Initialize(IDataReader xRdr)
		{
			StoreID = xRdr.FieldInt("StoreID");
			Name = xRdr.Field("Name");
			StoreGuid = xRdr.FieldGuid("StoreGUID");
			ProductionURI = xRdr.Field("ProductionURI");
			ProductionDirectoryPath = xRdr.Field("ProductionDirectoryPath");
			ProductionPort = xRdr.Field("ProductionPort");
			DevelopmentURI = xRdr.Field("DevelopmentURI");
			DevelopmentDirectoryPath = xRdr.Field("DevelopmentDirectoryPath");
			DevelopmentPort = xRdr.Field("DevelopmentPort");
			StagingURI = xRdr.Field("StagingURI");
			StagingDirectoryPath = xRdr.Field("StagingDirectoryPath");
			StagingPort = xRdr.Field("StagingPort");
			Description = xRdr.Field("Description");
			Published = ((byte)xRdr["Published"]) == 1;
			SkinID = (int)xRdr["SkinID"];
			IsDefault = (byte)xRdr["IsDefault"] == 1;
			CreatedOn = xRdr.FieldDateTime("CreatedOn");
			Deleted = xRdr.FieldBool("Deleted");
		}


		/// <summary>
		/// Switches setting on published flag
		/// </summary>
		public void PublishSwitch()
        {
            string SQL = @"
                UPDATE Store SET Published = @Published WHERE StoreGUID = @GUID
            ";
            SqlParameter[] xParams = new SqlParameter[]
            {
                new SqlParameter("@Published", Published ? 0 : 1),
                new SqlParameter("@GUID", StoreGuid)
            };
            DB.ExecuteSQL(SQL, xParams);
            Store.GetStoreList(true);
        }
        /// <summary>
        /// Sets the delete flag to true. This also will remove all store related mappings.
        /// (Logical delete)
        /// </summary>
        public void DeleteStore()
        {
            // removes mappings first then
            NukeMappings();

            string SQL = @"
                UPDATE Store SET Deleted = 1 WHERE StoreGUID = @GUID
            ";
            SqlParameter[] xParams = new SqlParameter[]
            {
                new SqlParameter("@GUID", StoreGuid)
            };
            DB.ExecuteSQL(SQL, xParams);
        }

        /// <summary>
        /// Sets the delete flag to false
        /// (Logical delete)
        /// </summary>
        public void UnDeleteStore()
        {
            string SQL = @"
                UPDATE Store SET Deleted = 0 WHERE StoreGUID = @GUID
            ";
            SqlParameter[] xParams = new SqlParameter[]
            {
                new SqlParameter("@GUID", StoreGuid)
            };
            DB.ExecuteSQL(SQL, xParams);
        }

        /// <summary>
        /// Sets this store as default turning the default flag off for all other stores
        /// </summary>
        public void SetDefault()
        {
            var makeDefSql = "[aspdnsf_MakeStoreDefault] @StoreID = {0}".FormatWith(this.StoreID);
            DB.ExecuteSQL(makeDefSql);

            // update the context cache
            var ctx = System.Web.HttpContext.Current;
            if (ctx != null)
            {
                ctx.Items["DefaultStoreId"] = this.StoreID;
            }
        }

        /// <summary>
        /// Removes all store mappings
        /// </summary>
        private void NukeMappings()
        {
            // first nuke all mappings
            var nukeMapSql = "[aspdnsf_NukeStoreMappings] @StoreID = {0}".FormatWith(this.StoreID);
            DB.ExecuteSQL(nukeMapSql);
        }

        /// <summary>
        /// Overrides the settings of this store instance and copies the settings from the passed store instance.
        /// </summary>
        /// <param name="other"></param>
        public void CopyFrom(Store other)
        {
            NukeMappings();

            // clone all mappings from the other store
            var cloneMapSql = "[aspdnsf_CloneStoreMappings] @FromStoreID = {0}, @ToStoreID = {1}".FormatWith(other.StoreID, this.StoreID);
            DB.ExecuteSQL(cloneMapSql);
            
            // reset cached entity mappings
            CacheEntityMappings();

            // now overwrite the following attributes            
            ProductionURI = other.ProductionURI;
			ProductionDirectoryPath = other.ProductionDirectoryPath;
			ProductionPort = other.ProductionPort;
			DevelopmentURI = other.DevelopmentURI;
			DevelopmentDirectoryPath = other.DevelopmentDirectoryPath;
			DevelopmentPort = other.DevelopmentPort;
            StagingURI = other.StagingURI;
			StagingDirectoryPath = other.StagingDirectoryPath;
			StagingPort = other.StagingPort;
            Description = other.Description;
            Published = other.Published;
            SkinID = other.SkinID;

            Save();
        }

        /// <summary>
        /// Creates a duplicate copy of the store
        /// </summary>
        public Store CloneStore()
        {
            Store newClonedStore = null;

            var newName = "{0} - Clone".FormatWith(this.Name);

            try
            {
                var newStoreID = DB.ExecuteStoredProcInt("aspdnsf_CloneStore",
                                new SqlParameter[] { 
                                    new SqlParameter("StoreID", SqlDbType.Int) { Value=this.StoreID, Direction= ParameterDirection.Input},
                                    new SqlParameter("NewStoreName", SqlDbType.NVarChar) { Size = 400, Value=newName, Direction= ParameterDirection.Input},
                                    new SqlParameter("NewStoreID", SqlDbType.Int) { Size = 400, Direction= ParameterDirection.Output}
                                });

                Action<IDataReader> readAction = (rs) =>
                {
                    if (rs.Read())
                    {
                        newClonedStore = new Store();
                        newClonedStore.Initialize(rs);
                    }
                };

                var sql = "SELECT * FROM Store WITH (NOLOCK) WHERE StoreID = {0}".FormatWith(newStoreID);
                DB.UseDataReader(sql, readAction);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Unable to clone store: ".FormatWith(this.Name), ex);
            }

            return newClonedStore;
        }

        private void CacheEntityMappings()
        {
            EntityMaps.Clear();

            // cache per entity map
            CacheEntityMappings("Product");
            CacheEntityMappings("Manufacturer");
            CacheEntityMappings("Category");
            CacheEntityMappings("Section");
            CacheEntityMappings("Coupon");
            CacheEntityMappings("OrderOption");
            CacheEntityMappings("GiftCard");
            CacheEntityMappings("ShippingMethod");
            CacheEntityMappings("Affiliate");
            CacheEntityMappings("Topic");
            CacheEntityMappings("News");
        }

        private void CacheEntityMappings(string entityType)
        {
            var entityMap = new StoreEntityMap() { StoreID = this.StoreID, EntityType = entityType };
            entityMap.MappedObjects = MappedObjectCollection.GetObjects(this.StoreID, entityType);

            EntityMaps.Add(entityMap);
        }

        private static object sl = new object();

        public static void CacheMappings()
        {
            lock (sl)
            {
                List<Store> stores = Store.GetStoreList();

                foreach (Store sto in stores)
                {
                    sto.CacheEntityMappings();
                }
            }
        }
    }

    public class StoreEntityMap
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
        private MappedObjectCollection m_mappedobjects;
        public MappedObjectCollection MappedObjects 
        {
            get { return m_mappedobjects; }
            set { m_mappedobjects = value; }
        }
    }
}
