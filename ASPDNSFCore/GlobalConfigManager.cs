// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Data.SqlClient;

namespace AspDotNetStorefrontCore
{
	public class GlobalConfigManager
	{
		public IEnumerable<GlobalConfig> ReadGlobalConfigsFromDb()
		{
			using(var connection = new SqlConnection(DB.GetDBConn()))
			using(var command = new SqlCommand())
			{
				command.Connection = connection;
				command.CommandText = "select * from GlobalConfig with(nolock) where Hidden = 0";

				connection.Open();
				using(var reader = command.ExecuteReader())
					while(reader.Read())
						yield return new GlobalConfig(
							reader.FieldInt("GlobalConfigID"),
							reader.FieldGuid("GlobalConfigGUID"),
							reader.Field("Name"),
							reader.Field("Description"),
							reader.Field("ConfigValue"),
							reader.Field("GroupName"),
							reader.FieldBool("SuperOnly"),
							reader.Field("ValueType"),
							reader.FieldDateTime("CreatedOn"),
							reader.Field("EnumValues"));
			}
		}
	}
}
