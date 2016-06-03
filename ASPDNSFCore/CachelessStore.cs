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
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontCore
{
	// CachelessStore is a temporary copy of the same class in core. We didn't want to modify the application dll or the core dll for admin patches. Because of the insanely slow store entity mapping
	//  cachine that was going on, this particular class mimics the original but without the caching model. This class is used the the EntityObjectMap control in Admin\Controls. In our next point rev, this will be resolved in core and this file will go away.
	[Serializable]
    public class CachelessStore
    {
        private Guid _StoreGuid;
        private static List<CachelessStore> _lstStores;

        public int StoreID { get; set; }

        public object StoreGuid
        {
            get { return _StoreGuid; }
            set
            {
                if (value.GetType() == typeof(Guid))
                    _StoreGuid = (Guid)value;
                else if (value.GetType() == typeof(string))
                    _StoreGuid = new Guid((string)value);
            }
        }

        public string ProductionURI { get; set; }

        public string StagingURI { get; set; }

        public string DevelopmentURI { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public bool Published { get; set; }

        public bool Deleted { get; set; }

        public bool IsDefault { get; set; }

        public int SkinID { get; set; }

        public DateTime CreatedOn { get; set; }

        public static void resetStoreCache()
        {
			_lstStores = null;
        }

        public static List<CachelessStore> GetStoreList()
        {
            return GetStoreList(false);
        }

        public static List<CachelessStore> GetStoreList(Boolean refresh)
        {
            if (_lstStores == null || refresh)
                _lstStores = GetStores(false);
            
            return _lstStores;
        }

        public static List<CachelessStore> GetStores(Boolean all)
        {
            var stores = new List<CachelessStore>();

            var sql = "SELECT [StoreID],[StoreGUID], [Name], [ProductionURI], [StagingURI], [DevelopmentURI], [Description], [Published], [Deleted], [SkinID], [IsDefault], [CreatedOn] FROM Store";

            if (!all)
                sql += " WHERE Deleted = 0 and Published = 1";

            using (var connection = new SqlConnection(DB.GetDBConn()))
            {
                connection.Open();
				using(var reader = DB.GetRS(sql, connection))
				{
					while(reader.Read())
					{
						CachelessStore store = new CachelessStore();
						store.Initialize(reader);
						stores.Add(store);
					}
				}
            }

            return stores;
        }

        public bool IsMapped(String entityType, Int32 id)
        {
            MappedObject map = GetMapping(entityType, id);

            return (map != null && map.IsMapped);
        }

        public MappedObject GetMapping(String entityType, Int32 id)
        {
            entityType = entityType.ToLowerInvariant();

            MappedObject map = null;

            using (var connection = new SqlConnection(DB.GetDBConn()))
            {
                connection.Open();

				using(var reader = DB.GetRS("dbo.aspdnsf_GetMappedObject @StoreId = {0}, @EntityType = {1}, @EntityID = {2}".FormatWith(this.StoreID, entityType.DBQuote(), id), connection))
				{
					while(reader.Read())
					{
						map = new MappedObject();
						map.ID = reader.FieldInt("ID");
						map.StoreID = this.StoreID;
						map.Type = reader.Field("EntityType");
						map.Name = reader.Field("Name");
						map.IsMapped = reader.FieldBool("Mapped");
					}
				}
            }

            return map;
        }

        public void UpdateMapping(string entityType, int entityID, bool isMapped)
        {
            var map = GetMapping(entityType, entityID);
            if (map == null)
            {
                map = new MappedObject();
                map.Type = entityType;
                map.StoreID = StoreID;
                map.ID = entityID;
            }

            map.IsMapped = isMapped;
            map.Save();
        }

        private void Initialize(IDataReader reader)
        {
            StoreID = reader.FieldInt("StoreID");
            Name = reader.Field("Name");
            StoreGuid = reader.FieldGuid("StoreGUID");
            ProductionURI = reader.Field("ProductionURI");
            DevelopmentURI = reader.Field("DevelopmentURI");
            StagingURI = reader.Field("StagingURI");
            Description = reader.Field("Description");
            Published = ((byte)reader["Published"]) == 1;
            SkinID = (int)reader["SkinID"];
            IsDefault = (byte)reader["IsDefault"] == 1;
            CreatedOn = reader.FieldDateTime("CreatedOn");
            Deleted = reader.FieldBool("Deleted");
        }
    }
}
