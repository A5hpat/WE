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
using System.Web.Mvc;

namespace AspDotNetStorefrontCore
{
	/// <summary>
	/// An immutable representation of an AppConfig database record.
	/// </summary>
	public class AppConfig
	{
		public readonly int AppConfigId;
		public readonly Guid AppConfigGuid;
		public readonly int StoreId;
		public readonly string Name;
		public readonly string GroupName;
		public readonly string Description;
		public readonly string ConfigValue;
		public readonly string ValueType;
		public readonly IEnumerable<string> AllowableValues;
		public readonly bool Hidden;
		public readonly bool SuperOnly;
		public readonly DateTime CreatedOn;
		public readonly DateTime UpdatedOn;

		public AppConfig(
			int appConfigId,
			Guid appConfigGuid,
			int storeId,
			string name,
			string groupName,
			string description,
			string configValue,
			string valueType,
			IEnumerable<string> allowableValues,
			bool hidden,
			bool superOnly,
			DateTime createdOn,
			DateTime updatedOn)
		{
			AppConfigId = appConfigId;
			AppConfigGuid = appConfigGuid;
			StoreId = storeId;
			Name = name;
			GroupName = groupName;
			Description = description;
			ConfigValue = configValue;
			ValueType = valueType;
			AllowableValues = allowableValues ?? Enumerable.Empty<string>();
			Hidden = hidden;
			SuperOnly = superOnly;
			CreatedOn = createdOn;
			UpdatedOn = updatedOn;
		}
	}

	/// <summary>
	/// Loads AppConfigs from the database into a dictionary.
	/// </summary>
	public class AppConfigLoader
	{
		/// <summary>
		/// Loads all AppConfigs for all stores into a read-only dictionary. It guarantees an entry exists for the provided <paramref name="defaultStoreId"/>.
		/// </summary>
		/// <param name="defaultStoreId">The default store ID</param>
		/// <returns>A read-only dictionary with a key for each store, with each entry being a read-only dictionary with a key for each AppConfig name.</returns>
		public IReadOnlyDictionary<int, IReadOnlyDictionary<string, AppConfig>> LoadAppConfigs(int defaultStoreId)
		{
			var storeAppConfigs = new Dictionary<int, IReadOnlyDictionary<string, AppConfig>>();

			using(var connection = new SqlConnection(DB.GetDBConn()))
			using(var command = connection.CreateCommand())
			{
				command.CommandText = "select AppConfigID, AppConfigGUID, StoreID, Name, GroupName, Description, ConfigValue, ValueType, AllowableValues, Hidden, SuperOnly, CreatedOn, UpdatedOn from AppConfig";

				connection.Open();
				using(var reader = command.ExecuteReader())
					while(reader.Read())
					{
						var storeId = reader.FieldInt("StoreID");
						var name = reader.Field("Name");

						var appConfig = new AppConfig(
							appConfigId: reader.FieldInt("AppConfigID"),
							appConfigGuid: reader.FieldGuid("AppConfigGUID"),
							storeId: storeId,
							name: name,
							groupName: reader.Field("GroupName"),
							description: reader.Field("Description"),
							configValue: reader.Field("ConfigValue"),
							valueType: reader.Field("ValueType"),
							allowableValues: reader.Field("AllowableValues").ParseAsDelimitedList(),
							hidden: reader.FieldBool("Hidden"),
							superOnly: reader.FieldBool("SuperOnly"),
							createdOn: reader.FieldDateTime("CreatedOn"),
							updatedOn: reader.FieldDateTime("UpdatedOn"));

						if(!storeAppConfigs.ContainsKey(storeId))
							storeAppConfigs[storeId] = new Dictionary<string, AppConfig>(1024, StringComparer.OrdinalIgnoreCase);

						// Cast the store's AppConfig dictionary back to a mutable dictionary so we can update it.
						var storeDictionary = (Dictionary<string, AppConfig>)storeAppConfigs[storeId];
						storeDictionary[name] = appConfig;
					}
			}

			if(!storeAppConfigs.ContainsKey(defaultStoreId))
				storeAppConfigs[defaultStoreId] = new Dictionary<string, AppConfig>();

			return storeAppConfigs;
		}
	}

	/// <summary>
	/// Maintains a persistent in-memory copy of all AppConfigs and provides access to those AppConfigs.
	/// </summary>
	public class AppConfigCache
	{
		public const int DefaultStoreId = 0;

		public event EventHandler CacheReset;

		readonly AppConfigLoader AppConfigLoader;

		// Mutable
		IReadOnlyDictionary<int, IReadOnlyDictionary<string, AppConfig>> Cache;

		public AppConfigCache(AppConfigLoader appConfigLoader)
		{
			AppConfigLoader = appConfigLoader;
			Cache = new Dictionary<int, IReadOnlyDictionary<string, AppConfig>>();
		}

		/// <summary>
		/// Reloads the AppConfig cache from the database.
		/// </summary>
		public void ResetCache()
		{
			Cache = AppConfigLoader.LoadAppConfigs(DefaultStoreId);

			if(CacheReset != null)
				CacheReset(this, EventArgs.Empty);
		}

		/// <summary>
		/// Gets all AppConfigs.
		/// </summary>
		/// <param name="storeId">The store to retrieve all AppConfigs for. Pass null to indicate the default store.</param>
		/// <param name="cascadeToDefaultStore">Pass true to also include all AppConfigs for the default store that were not 
		/// defined for the specified store. Pass false or leave null to only get AppConfigs defined for the specified store.</param>
		/// <returns>A collection of all AppConfigs for the specified store.</returns>
		public IEnumerable<AppConfig> GetAppConfigs(int? storeId = null, bool? cascadeToDefaultStore = null)
		{
			var storeAppConfigs = Cache[storeId ?? DefaultStoreId].Values;
			if((storeId ?? DefaultStoreId) == DefaultStoreId || cascadeToDefaultStore != true)
				return storeAppConfigs;

			return storeAppConfigs.Union(Cache[DefaultStoreId].Values, new AppConfigNameEqualityComparer());
		}

		/// <summary>
		/// Gets an AppConfig by name.
		/// </summary>
		/// <param name="name">The name of the AppConfig to get.</param>
		/// <param name="storeId">The store ID to get the AppConfig for.</param>
		/// <param name="cascadeToDefaultStore">Pass true to try to get the requested AppConfig from the default store if it's
		/// not found in the specified store. Leave null or pass false to only get an AppConfig defined for the specified store.</param>
		/// <returns>An AppConfig that matches the requested name, or null if none was found.</returns>
		public AppConfig GetAppConfig(string name, int? storeId = null, bool? cascadeToDefaultStore = null)
		{
			if(Cache.ContainsKey(storeId ?? DefaultStoreId) && Cache[storeId ?? DefaultStoreId].ContainsKey(name))
				return Cache[storeId ?? DefaultStoreId][name];

			if(storeId != DefaultStoreId && (cascadeToDefaultStore ?? false))
				return GetAppConfig(name, DefaultStoreId, false);

			return null;
		}

		/// <summary>
		/// Gets an AppConfig by ID.
		/// </summary>
		/// <param name="appConfigId">The ID of the AppConfig to get.</param>
		/// <returns>An AppConfig that matches the requested ID, or null if none was found.</returns>
		public AppConfig GetAppConfig(int appConfigId)
		{
			return Cache
				.Values
				.SelectMany(storeCache => storeCache
					.Values
					.Where(appConfig => appConfig.AppConfigId == appConfigId))
				.FirstOrDefault();
		}

		class AppConfigNameEqualityComparer : EqualityComparer<AppConfig>
		{
			readonly IEqualityComparer<string> NameComparer;

			public AppConfigNameEqualityComparer(IEqualityComparer<string> nameComparer = null)
			{
				NameComparer = nameComparer ?? StringComparer.OrdinalIgnoreCase;
			}

			public override bool Equals(AppConfig x, AppConfig y)
			{
				if(ReferenceEquals(x, y))
					return true;

				if(ReferenceEquals(x, null) || ReferenceEquals(y, null))
					return false;

				return NameComparer.Equals(x.Name, y.Name);
			}

			public override int GetHashCode(AppConfig obj)
			{
				if(obj == null)
					return NameComparer.GetHashCode(null);

				return NameComparer.GetHashCode(obj.Name);
			}
		}
	}

	public class AppConfigProvider
	{
		readonly AppConfigCache AppConfigCache;

		public AppConfigProvider(AppConfigCache appConfigCache)
		{
			AppConfigCache = appConfigCache;
		}

		/// <summary>
		/// Gets all AppConfigs.
		/// </summary>
		/// <param name="storeId">The store to retrieve all AppConfigs for. Pass null to indicate the default store.</param>
		/// <param name="cascadeToDefaultStore">Pass true to also include all AppConfigs for the default store that were not 
		/// defined for the specified store. Pass false or leave null to only get AppConfigs defined for the specified store.</param>
		/// <returns>A collection of all AppConfigs for the specified store.</returns>
		public IEnumerable<AppConfig> GetAppConfigs(int? storeId = null, bool? cascadeToDefault = null)
		{
			return AppConfigCache.GetAppConfigs(storeId, cascadeToDefault);
		}

		/// <summary>
		/// Gets an AppConfig by ID.
		/// </summary>
		/// <param name="appConfigId">The ID of the AppConfig to get.</param>
		/// <returns>An AppConfig that matches the requested ID, or null if none was found.</returns>
		public AppConfig GetAppConfig(int appConfigId)
		{
			return AppConfigCache.GetAppConfig(appConfigId);
		}

		/// <summary>
		/// Gets an AppConfig by name.
		/// </summary>
		/// <param name="name">The name of the AppConfig to get.</param>
		/// <param name="storeId">The store ID to get the AppConfig for. Pass null to use the default store.</param>
		/// <param name="cascadeToDefaultStore">Pass true to try to get the requested AppConfig from the default store if it's
		/// not found in the specified store. Leave null or pass false to only get an AppConfig defined for the specified store.</param>
		/// <returns>An AppConfig that matches the requested name, or null if none was found.</returns>
		public AppConfig GetAppConfig(string name, int? storeId = null, bool? cascadeToDefault = null)
		{
			return AppConfigCache.GetAppConfig(name, storeId, cascadeToDefault);
		}

		/// <summary>
		/// Gets an AppConfig value by name.
		/// </summary>
		/// <param name="name">The name of the AppConfig to get.</param>
		/// <param name="storeId">The store ID to get the AppConfig for. Pass null to use the default store.</param>
		/// <param name="cascadeToDefaultStore">Pass true to try to get the requested AppConfig from the default store if it's
		/// not found in the specified store. Leave null or pass false to only get an AppConfig defined for the specified store.</param>
		/// <returns>The string value from the AppConfig that matches the requested name, or an emptry string if none was found.</returns>
		public string GetAppConfigValue(string name, int? storeId = null, bool? cascadeToDefault = null)
		{
			var appConfig = AppConfigCache.GetAppConfig(name, storeId, cascadeToDefault ?? false);
			if(appConfig == null)
				return string.Empty;

			return appConfig.ConfigValue;
		}

		/// <summary>
		/// Sets the value of the specified AppConfig and updates the AppConfig cache. Will create the AppConfig if it doesn't exist. Will create a store-specific AppConfig if a store ID is provided and the AppConfig is 
		/// already defined for the default store.
		/// </summary>
		/// <param name="name">The name of the AppConfig to set.</param>
		/// <param name="value">The value to set on the AppConfig.</param>
		/// <param name="storeId">The store ID to get the AppConfig to set. Pass null to use the default store.</param>
		/// <param name="dontDupValues">A list of values that will result in only an update, never a create.</param>
		/// <returns>True if the value was set successfully, false otherwise.</returns>
		public bool SetAppConfigValue(string name, string value, int? storeId = null, IEnumerable<string> dontDupValues = null, bool? suppressSecurityLog = null)
		{
			try
			{
				var existingConfig = AppConfigCache.GetAppConfig(name, storeId);
				if(existingConfig != null)
					return UpdateAppConfig(existingConfig.AppConfigId, configValue: value, suppressSecurityLog: suppressSecurityLog);

				if((dontDupValues ?? Enumerable.Empty<string>()).Contains(value))
					return false;

				if(storeId != null && DuplicateAppConfigForStore(name, value, storeId.Value))
					return true;

				return AddAppConfig(
					name: name,
					description: string.Empty,
					configValue: value,
					valueType: null,
					allowableValues: null,
					groupName: null,
					superOnly: false,
					storeId: storeId ?? AppConfigCache.DefaultStoreId,
					suppressAuditLog: suppressSecurityLog);
			}
			catch(Exception e)
			{
				SysLog.LogException(
					e,
					MessageTypeEnum.GeneralException,
					MessageSeverityEnum.Error);

				return false;
			}
		}

		bool DuplicateAppConfigForStore(string name, string value, int storeId)
		{
			var defaultAppConfig = AppConfigCache.GetAppConfig(name, null);
			if(defaultAppConfig == null)
				return false;

			var isBoolean = StringComparer.OrdinalIgnoreCase.Equals(defaultAppConfig.ValueType, "boolean");
			var isMultiselect = StringComparer.OrdinalIgnoreCase.Equals(defaultAppConfig.ValueType, "multiselect");
			var isSameValue = isMultiselect
				? StringComparer.OrdinalIgnoreCase.Equals(
					value.Replace(" ", "").Trim(),
					defaultAppConfig.ConfigValue.Replace(" ", "").Trim())
				: StringComparer.OrdinalIgnoreCase.Equals(
					value,
					defaultAppConfig.ConfigValue);

			if(!isBoolean && isSameValue)
				return true;

			return AddAppConfig(
				name: defaultAppConfig.Name,
				description: defaultAppConfig.Description,
				configValue: value,
				valueType: defaultAppConfig.ValueType,
				allowableValues: defaultAppConfig.AllowableValues.ToDelimitedString(),
				groupName: defaultAppConfig.GroupName,
				superOnly: defaultAppConfig.SuperOnly,
				storeId: storeId);
		}

		/// <summary>
		/// Checks if an AppConfig exists for the given name and store.
		/// </summary>
		/// <param name="name">The name of the AppConfig to check for.</param>
		/// <param name="storeId">The store ID to check. Pass null to use the default store.</param>
		/// <param name="cascadeToDefaultStore">Pass true to check both the indicated store and the default store. Pass false or null to 
		/// check only the indicated store.</param>
		/// <returns>True if the AppConfig exists, false otherwise.</returns>
		public bool AppConfigExists(string name, int? storeId = null, bool? cascadeToDefaultStore = null)
		{
			return AppConfigCache.GetAppConfig(name, storeId, cascadeToDefaultStore) != null;
		}

		/// <summary>
		/// Creates a new AppConfig and reloads the AppConfig cache.
		/// </summary>
		/// <returns>True if the AppConfig was created, false otherwise.</returns>
		public bool AddAppConfig(string name, string description, string configValue, string valueType, string allowableValues, string groupName, bool superOnly, int storeId, bool? suppressAuditLog = null)
		{
			if(configValue != null)
				configValue = configValue.Trim();

			if((name ?? string.Empty).Trim().Length == 0)
				return false;

			if((groupName ?? string.Empty).Trim().Length == 0)
				groupName = "Custom";

			if(string.IsNullOrEmpty(valueType))
				valueType = "string";

			using(var connection = new SqlConnection(DB.GetDBConn()))
			using(var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = "dbo.aspdnsf_insAppconfig";
				command.Parameters.AddWithValue("Name", name);
				command.Parameters.AddWithValue("Description", description);
				command.Parameters.AddWithValue("ConfigValue", configValue);
				command.Parameters.AddWithValue("GroupName", groupName);
				command.Parameters.AddWithValue("SuperOnly", superOnly);
				command.Parameters.AddWithValue("StoreId", storeId);
				command.Parameters.AddWithValue("ValueType", valueType);
				command.Parameters.AddWithValue("AllowableValues", allowableValues);

				command.Parameters.Add(new SqlParameter("@AppConfigID", SqlDbType.Int, 4)).Direction = ParameterDirection.Output;

				connection.Open();
				command.ExecuteNonQuery();

				if(command.Parameters["@AppConfigID"].Value == null)
					return false;
			}

			if(superOnly && suppressAuditLog != true)
			{
				var customer = HttpContext.Current.GetCustomer();

				Security.LogEvent(
					"Setting Created Success",
					"Parameter Name: " + name,
					0,
					customer.CustomerID,
					customer.CurrentSessionID);
			}

			AppConfigCache.ResetCache();
			return true;
		}

		/// <summary>
		/// Updates an existing AppConfig and reloads the AppConfig cache.
		/// </summary>
		/// <returns>True if the AppConfig was updated, false otherwise.</returns>
		public bool UpdateAppConfig(int appConfigId, string description = null, string configValue = null, string groupName = null, bool? superOnly = null, int? storeId = null, string valueType = null, string allowableValues = null, bool? suppressSecurityLog = null)
		{
			if(configValue != null)
				configValue = configValue.Trim();

			using(var connection = new SqlConnection(DB.GetDBConn()))
			using(var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = "dbo.aspdnsf_updAppconfig";
				command.Parameters.AddWithValue("AppConfigID", appConfigId);
				command.Parameters.AddWithValue("Description", description);
				command.Parameters.AddWithValue("ConfigValue", configValue);
				command.Parameters.AddWithValue("GroupName", groupName);
				command.Parameters.AddWithValue("SuperOnly", superOnly);
				command.Parameters.AddWithValue("StoreId", storeId);
				command.Parameters.AddWithValue("ValueType", valueType);
				command.Parameters.AddWithValue("AllowableValues", allowableValues);

				connection.Open();
				try
				{
					command.ExecuteNonQuery();
				}
				catch
				{
					return false;
				}
			}

			var appConfig = AppConfigCache.GetAppConfig(appConfigId);
			if(appConfig.SuperOnly && suppressSecurityLog != true)
			{
				var customer = HttpContext.Current.GetCustomer();

				Security.LogEvent(
					"Setting Updated Success",
					"Parameter Changed: " + appConfig.Name,
					0,
					customer.CustomerID,
					customer.CurrentSessionID);
			}

			AppConfigCache.ResetCache();
			return true;
		}
	}

	/// <summary>
	/// Compatibility class to maintain legacy access to AppConfigs.
	/// </summary>
	public static class AppConfigManager
	{
		readonly static AppConfigProvider AppConfigProvider;

		static AppConfigManager()
		{
			AppConfigProvider = DependencyResolver.Current.GetService<AppConfigProvider>();
		}

		public static IEnumerable<AppConfig> GetAppConfigs(int? storeId = null, bool? cascadeToDefault = null)
		{
			return AppConfigProvider.GetAppConfigs(storeId, cascadeToDefault);
		}

		public static AppConfig GetAppConfig(int appConfigId)
		{
			return AppConfigProvider.GetAppConfig(appConfigId);
		}

		public static AppConfig GetAppConfig(string name, int? storeId = null, bool? cascadeToDefault = null)
		{
			return AppConfigProvider.GetAppConfig(name, storeId, cascadeToDefault);
		}

		public static string GetAppConfigValue(string name, int? storeId = null, bool? cascadeToDefault = null)
		{
			return AppConfigProvider.GetAppConfigValue(name, storeId, cascadeToDefault ?? false);
		}

		public static bool SetAppConfigValue(string name, string value, int? storeId = null, IEnumerable<string> dontDupValues = null)
		{
			return AppConfigProvider.SetAppConfigValue(name, value, storeId, dontDupValues);
		}

		public static bool AppConfigExists(string name, int? storeId = null, bool? cascadeToDefaultStore = null)
		{
			return AppConfigProvider.AppConfigExists(name, storeId, cascadeToDefaultStore);
		}

		public static bool AddAppConfig(string name, string description, string configValue, string valueType, string allowableValues, string groupName, bool superOnly, int storeId)
		{
			return AppConfigProvider.AddAppConfig(name, description, configValue, valueType, allowableValues, groupName, superOnly, storeId);
		}
	}

	public enum AppConfigType
	{
		@string,
		boolean,
		integer,
		@decimal,
		@double,
		@enum,
		multiselect,
		invoke
	}

	public static class AppConfigProviderExtensions
	{
		static readonly AppConfigValueConverter AppConfigValueConverter;

		static AppConfigProviderExtensions()
		{
			AppConfigValueConverter = DependencyResolver.Current.GetService<AppConfigValueConverter>();
		}

		public static T GetAppConfigValue<T>(this AppConfigProvider appConfigProvider, string name, int? storeId = null, bool? cascadeToDefault = null)
		{
			var value = appConfigProvider.GetAppConfigValue(name, storeId, cascadeToDefault);
			return AppConfigValueConverter.ConvertAppConfigValueToTypedValue<T>(value);
		}
	}

}
