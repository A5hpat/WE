// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace AspDotNetStorefrontCore
{
	[Serializable()]
	public class GlobalConfig
	{
		public Guid GUID
		{ get; set; }

		public int ID
		{ get; set; }

		public string Name
		{ get; set; }

		public string Description
		{ get; set; }

		public string ConfigValue
		{ get; set; }

		public string ValueType
		{ get; set; }

		public string GroupName
		{ get; set; }

		public bool SuperOnly
		{ get; set; }

		public DateTime CreatedOn
		{ get; set; }

		public IEnumerable<string> AllowableValues
		{ get; set; }

		public GlobalConfig(int id, Guid guid, string name, string description, string configValue, string groupName, bool superOnly, string configType, DateTime createdOn, string enumValues)
		{
			ID = id;
			GUID = guid;
			Name = name;
			Description = description;
			ConfigValue = configValue;
			GroupName = groupName;
			ValueType = configType;
			SuperOnly = superOnly;
			CreatedOn = createdOn;
			AllowableValues = enumValues
				.ParseAsDelimitedList()
				.ToArray();
		}

		public void Save()
		{
			string sql = @"
				IF EXISTS (SELECT * FROM GlobalConfig WHERE GlobalConfigGuid = @guid)
				BEGIN
					UPDATE GlobalConfig SET 
						[Name] = @name,
						Description = @description,
						ConfigValue = @configValue,
						GroupName = @groupName,
						SuperOnly = @superOnly,
						EnumValues = @enumValues
					WHERE [GlobalConfigID] = @id
				END
				ELSE
				BEGIN
					INSERT INTO GlobalConfig
						(GlobalConfigGUID, [Name], [Description], [ConfigValue], [ValueType] ,[GroupName], [SuperOnly], [EnumValues])
						VALUES
						(NewID(), @name, @description, @configValue, @valueType, @groupName, @superOnly, @enumValues)
				END";

			var parameters = new[] 
			{
				new SqlParameter("@name", Name),
				new SqlParameter("@description", Description),
				new SqlParameter("@configValue", ConfigValue.ToString()),
				new SqlParameter("@valueType", ValueType),
				new SqlParameter("@enumValues", AllowableValues.ToDelimitedString()),
				new SqlParameter("@groupName", GroupName),
				new SqlParameter("@superOnly", SuperOnly ? 1 : 0),
				new SqlParameter("@guid", GUID),
				new SqlParameter("@id", ID)
			};

			DB.ExecuteSQL(sql, parameters);
		}

		public static GlobalConfig GetGlobalConfig(string name)
		{
			using(var connection = new SqlConnection(DB.GetDBConn()))
			using(var command = new SqlCommand())
			{
				command.Connection = connection;
				command.CommandText = "select * from GlobalConfig with (nolock) where Hidden = 0 and [Name] = @name";
				command.Parameters.AddWithValue("@name", name);

				connection.Open();

				using(var reader = command.ExecuteReader())
				{
					if(!reader.Read())
						return null;

					return new GlobalConfig(
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

		public static bool CreateGlobalConfig(string name, string group, string description, string configValue, string valueType)
		{
			if(GlobalConfig.GetGlobalConfig(name) != null)
				return false;

			var sql = @"
				INSERT GlobalConfig([Name], [GroupName], [Description], [ConfigValue], [ValueType], [IsMultiStore]) 
				VALUES(@name, @group, @description, @configValue, @valueType, 'false')";

			var parameters = new[]
			{
				new SqlParameter("@name", name),
				new SqlParameter("@group", group.ToUpper()),
				new SqlParameter("@description", description),
				new SqlParameter("@configValue", configValue.ToString()),
				new SqlParameter("@valueType", valueType),
			};

			DB.ExecuteSQL(sql, parameters);

			return true;
		}
	}
}
