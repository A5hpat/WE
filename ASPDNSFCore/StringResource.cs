// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using AspDotNetStorefrontExcelWrapper;

namespace AspDotNetStorefrontCore
{
	public class StringResource
	{
		#region private variables

		private int m_Stringresourceid = -1;
		private string m_Name = string.Empty;
		private string m_Localesetting = string.Empty;
		private string m_Configvalue = string.Empty;
		private bool m_Modified = false;

		#endregion

		#region contructors

		public StringResource()
		{
			StoreId = 1; // force default
		}

		public StringResource(int StringResourceID) : this()
		{
			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(var dr = DB.GetRS("aspdnsf_getStringresource " + StringResourceID.ToString(), con))
				{
					if(dr.Read())
					{
						m_Stringresourceid = DB.RSFieldInt(dr, "StringResourceID");
						m_Name = DB.RSField(dr, "Name");
						m_Localesetting = DB.RSField(dr, "LocaleSetting");
						m_Configvalue = DB.RSField(dr, "ConfigValue");
						m_Modified = DB.RSFieldBool(dr, "Modified");
						StoreId = dr.FieldInt("StoreId");
					}
				}
			}
		}

		public StringResource(int StringResourceID, string Name, string LocaleSetting, string ConfigValue, bool Modified) : this()
		{
			m_Stringresourceid = StringResourceID;
			m_Name = Name;
			m_Localesetting = LocaleSetting;
			m_Configvalue = ConfigValue;
			m_Modified = Modified;

		}

		#endregion

		#region static methods

		/// <summary>
		/// Creates a new StringResource record and returns a StringResource Object.  If the StringResource record cannot be created the returned StringResource object has the error in it's ConfigValue parameter
		/// </summary>
		/// <param name="Name"></param>
		/// <param name="LocaleSetting"></param>
		/// <param name="ConfigValue"></param>
		/// <returns></returns>
		public static StringResource Create(int storeId, string Name, string LocaleSetting, string ConfigValue)
		{
			var StringResourceID = -1;
			var err = string.Empty;

			using(var cn = new SqlConnection(DB.GetDBConn()))
			{
				cn.Open();
				using(var cmd = new SqlCommand())
				{
					cmd.Connection = cn;
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.CommandText = "dbo.aspdnsf_insStringresource";

					cmd.Parameters.Add(new SqlParameter("@StoreId", SqlDbType.Int, 4));
					cmd.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 200));
					cmd.Parameters.Add(new SqlParameter("@LocaleSetting", SqlDbType.NVarChar, 20));
					cmd.Parameters.Add(new SqlParameter("@ConfigValue", SqlDbType.NVarChar, 4000));
					cmd.Parameters.Add(new SqlParameter("@StringResourceID", SqlDbType.Int, 4)).Direction = ParameterDirection.Output;

					cmd.Parameters["@StoreId"].Value = storeId;
					cmd.Parameters["@Name"].Value = Name;
					cmd.Parameters["@LocaleSetting"].Value = LocaleSetting;
					cmd.Parameters["@ConfigValue"].Value = ConfigValue;

					try
					{
						cmd.ExecuteNonQuery();
						StringResourceID = Int32.Parse(cmd.Parameters["@StringResourceID"].Value.ToString());
					}
					catch(Exception ex)
					{
						err = ex.Message;
					}
				}
			}

			if(StringResourceID > 0)
			{
				StringResource sr = new StringResource(StringResourceID);
				return sr;
			}
			else
			{
				return new StringResource(-1, "", "", err, false);
			}

		}

		public static string Update(int storeId, int StringResourceID, string Name, string LocaleSetting, string ConfigValue)
		{
			var err = string.Empty;

			using(var cn = new SqlConnection(DB.GetDBConn()))
			{
				cn.Open();
				using(var cmd = new SqlCommand())
				{
					cmd.Connection = cn;
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.CommandText = "dbo.aspdnsf_updStringresource";

					cmd.Parameters.Add(new SqlParameter("@StoreId", SqlDbType.Int, 4));
					cmd.Parameters.Add(new SqlParameter("@StringResourceID", SqlDbType.Int, 4));
					cmd.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 200));
					cmd.Parameters.Add(new SqlParameter("@LocaleSetting", SqlDbType.NVarChar, 20));
					cmd.Parameters.Add(new SqlParameter("@ConfigValue", SqlDbType.NVarChar, 4000));

					cmd.Parameters["@StoreId"].Value = storeId;
					cmd.Parameters["@StringResourceID"].Value = StringResourceID;

					if(Name == null) cmd.Parameters["@Name"].Value = DBNull.Value;
					else cmd.Parameters["@Name"].Value = Name;

					if(LocaleSetting == null) cmd.Parameters["@LocaleSetting"].Value = DBNull.Value;
					else cmd.Parameters["@LocaleSetting"].Value = LocaleSetting;

					if(ConfigValue == null) cmd.Parameters["@ConfigValue"].Value = DBNull.Value;
					else cmd.Parameters["@ConfigValue"].Value = ConfigValue;

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

		#endregion

		#region Public Methods

		public string Update(string Name, string LocaleSetting, string ConfigValue)
		{
			return Update(this.StoreId, Name, LocaleSetting, ConfigValue);
		}

		public string Update(int storeId, string Name, string LocaleSetting, string ConfigValue)
		{
			string err = String.Empty;
			try
			{
				err = Update(storeId, this.m_Stringresourceid, Name, LocaleSetting, ConfigValue);
				if(err == "")
				{
					this.StoreId = storeId;
					m_Name = CommonLogic.IIF(Name != null, Name, m_Name);
					m_Configvalue = CommonLogic.IIF(LocaleSetting != null, ConfigValue, m_Configvalue);
					m_Localesetting = CommonLogic.IIF(LocaleSetting != null, LocaleSetting, m_Localesetting);
					m_Modified = true;
				}
			}
			catch(Exception ex)
			{
				err = ex.Message;
			}

			return err;

		}

		#endregion

		#region public properties

		public int StringResourceID
		{
			get { return m_Stringresourceid; }
		}

		public string Name
		{
			get { return m_Name; }
		}

		public string LocaleSetting
		{
			get { return m_Localesetting; }
		}

		public string ConfigValue
		{
			get { return m_Configvalue; }
		}

		private int m_storeid;
		public int StoreId
		{
			get { return m_storeid; }
			set { m_storeid = value; }
		}

		private StringResources m_owner;
		public StringResources Owner
		{
			get { return m_owner; }
			set { m_owner = value; }
		}

		#endregion
	}

	public class StringResources : IEnumerable, IEnumerable<StringResource>
	{
		private Dictionary<string, StringResource> data = new Dictionary<string, StringResource>();

		public void Add(StringResource str)
		{
			string key = string.Format("{0}_{1}", str.LocaleSetting.ToLowerInvariant(), str.Name.ToLowerInvariant());
			if(data.ContainsKey(key))
			{
				// if it's already existing, remove the previous one so that we can overwriteit
				data.Remove(key);
			}
			data.Add(key, str);

			str.Owner = this;
		}

		public StringResource this[string localesetting, string name]
		{
			get
			{
				string key = string.Format("{0}_{1}", localesetting.ToLowerInvariant(), name.ToLowerInvariant());

				StringResource str = null;
				// perform checking first before directly trying to get the value
				// so as not to throw error if key is not existing
				if(data.ContainsKey(key))
				{
					str = data[key];
				}

				// allow for null values which means it's not present in this collection
				return str;
			}
		}

		public StringResource this[int id]
		{
			get
			{
				var found = data.Values.FirstOrDefault(config => config.StringResourceID == id);
				return found;
			}
		}

		public int Count
		{
			get { return data.Count; }
		}

		/// <summary>
		/// Override the default behavior to enumerate through the values instead of the keys
		/// </summary>
		/// <returns></returns>
		public IEnumerator<StringResource> GetEnumerator()
		{
			return data.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}

	/// <summary>
	/// Imports a .csv or .xlsx file into the StringResources table.
	/// </summary>
	public class StringResourceImporter
	{
		[Flags]
		public enum ImportOption
		{
			Default = 0,
			LeaveModified = 1,
			OverWrite = 2
		}

		/// <summary>
		/// Load the indicated spreadsheets and return a DataTable with the status of each line. Does NOT actually update the StringResources table.
		/// </summary>
		/// <param name="localeSetting">The .NET locale code, e.g. "en-US"</param>
		/// <param name="paths">The filesystem paths to the spreadsheets to import</param>
		/// <param name="options">Import options flags</param>
		/// <returns>A DataTable of the imported data with the status of each line.</returns>
		public DataTable Validate(string localeSetting, IEnumerable<string> paths, ImportOption options)
		{
			return Import(localeSetting, paths, options, true);
		}

		/// <summary>
		/// Import the indicated spreadsheets into the StringResources table.
		/// </summary>
		/// <param name="localeSetting"></param>
		/// <param name="paths"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public DataTable Import(string localeSetting, IEnumerable<string> paths, ImportOption options)
		{
			return Import(localeSetting, paths, options, false);
		}

		DataTable Import(string localeSetting, IEnumerable<string> paths, ImportOption options, bool dryRun)
		{
			// Convert spreadsheets to a single DataTable
			var importData = ConvertToDataTable(paths);

			// Validate and, if dryRun = false, import the DataTable
			var result = LoadDataTable(localeSetting, importData, options, dryRun);

			return result;
		}

		public DataTable BuildEmptyStringResourceDataTable()
		{
			var keyColumn = new DataColumn("A");
			var valueColumn = new DataColumn("B");
			var storeColumn = new DataColumn("C");
			var primaryKey = new[]
			{
				keyColumn,
				storeColumn
			};

			var dataTable = new DataTable
			{
				Columns = {
						keyColumn,
						valueColumn,
						storeColumn,
					}
			};
			dataTable.PrimaryKey = primaryKey;
			return dataTable;
		}

		DataTable ConvertToDataTable(IEnumerable<string> paths)
		{
			var dataTable = BuildEmptyStringResourceDataTable();

			// Merge each file into a single DataTable
			foreach(var path in paths)
			{
				var convertedDataTable = String.Equals(Path.GetExtension(path), ".csv", StringComparison.OrdinalIgnoreCase)
					? ConvertCsvToDataTable(path)
					: ConvertXlsxToDataTable(path);

				dataTable.Merge(convertedDataTable, false, MissingSchemaAction.Ignore);
			}

			return dataTable;
		}

		DataTable ConvertCsvToDataTable(string path)
		{
			using(var stream = File.Open(path, FileMode.Open, FileAccess.Read))
			using(var reader = new StreamReader(stream))
				return CsvParser.Parse(reader, false);
		}

		DataTable ConvertXlsxToDataTable(string path)
		{
			// Parse XLSX to XML
			var legacyXmlDoc = new ExcelToXml(path).LoadSheet("Sheet1", "C", 5000, "A");
			var nodeReader = new XmlNodeReader(legacyXmlDoc);
			nodeReader.MoveToContent();
			var doc = XDocument.Load(nodeReader);
			var rows = doc
				.Element("excel")
				.Element("sheet")
				.Elements("row");

			// Load XML into a DataTable
			var dataTable = BuildEmptyStringResourceDataTable();

			foreach(var row in rows)
				dataTable.Rows.Add(
					(string)row.Elements("col").Where(xe => (string)xe.Attribute("id") == "A").FirstOrDefault(),
					(string)row.Elements("col").Where(xe => (string)xe.Attribute("id") == "B").FirstOrDefault(),
					(string)row.Elements("col").Where(xe => (string)xe.Attribute("id") == "C").FirstOrDefault());

			return dataTable;
		}

		DataTable LoadDataTable(string localeSetting, DataTable importData, ImportOption options, bool dryRun)
		{
			var importResult = new DataTable
			{
				Columns =
						{
							new DataColumn("Index", typeof(int)),
							new DataColumn("Status", typeof(string)),
							new DataColumn("Name", typeof(string)),
							new DataColumn("Value", typeof(string)),
							new DataColumn("LocaleSetting", typeof(string)),
							new DataColumn("StoreId", typeof(int)),
						},
			};

			using(var connection = DB.dbConn())
			{
				connection.Open();

				var index = 0;
				foreach(DataRow row in importData.Rows)
				{
					var name = row.Field<string>("A");
					var value = row.Field<string>("B");

					int storeId;
					if(!int.TryParse(row.Field<string>("C"), out storeId))
						storeId = AppLogic.StoreID();

					var status = LoadLine(name, value, localeSetting, storeId, options, dryRun, connection);

					importResult.Rows.Add(
						index,
						status,
						name,
						value,
						localeSetting,
						storeId);

					index++;
				}
			}

			return importResult;
		}

		string LoadLine(string name, string configValue, string localeSetting, int storeId, ImportOption options, bool preview, SqlConnection connection)
		{
			if(String.IsNullOrEmpty(name))
				return AppLogic.ro_OK;

			try
			{
				var existing = false;
				var modified = false;
				var resourceGuid = string.Empty;

				// Check if the string resource exists and what its status is
				using(var command = new SqlCommand())
				{
					command.Connection = connection;
					command.CommandText =
						@"select Name, Modified, StringResourceGuid 
						from StringResource 
						where Name = @name AND LocaleSetting = @localeSetting and StoreId = @storeId";

					command.Parameters.AddRange(new[]
						{
							new SqlParameter("@name", name),
							new SqlParameter("@localeSetting", localeSetting),
							new SqlParameter("@storeId", storeId),
						});

					using(var reader = command.ExecuteReader())
						if(reader.Read())
						{
							existing = true;
							modified = DB.RSFieldTinyInt(reader, "Modified") > 0;
							resourceGuid = DB.RSFieldGUID(reader, "StringResourceGuid");
						}
				}

				// Execute behavior based on the status of the string resource
				if(!existing)
				{
					if(!preview)
						InsertStringResource(name, localeSetting, configValue, storeId);

					return AppLogic.ro_OK;
				}

				if(modified && ((options & ImportOption.LeaveModified) == ImportOption.LeaveModified))
					return "Not Imported (was already modified)";

				if((options & ImportOption.OverWrite) == ImportOption.OverWrite)
				{
					if(!preview)
					{
						DeleteStringResource(resourceGuid);
						InsertStringResource(name, localeSetting, configValue, storeId);
					}

					return AppLogic.ro_OK;
				}

				return "Not Imported (existing duplicate)";
			}
			catch(Exception exception)
			{
				return CommonLogic.GetExceptionDetail(exception, "<br/>");
			}
		}

		void DeleteStringResource(string stringResourceGuid)
		{
			DB.ExecuteSQL(
				@"delete StringResource where StringResourceGuid = @stringResourceGuid",
				new[] { new SqlParameter("@stringResourceGuid", stringResourceGuid) });
		}

		void InsertStringResource(string name, string localeSetting, string configValue, int storeId)
		{
			DB.ExecuteSQL(
				@"insert into StringResource(StringResourceGUID, Name, LocaleSetting, ConfigValue, StoreID) 
					values(newid(), @name, @localeSetting, @configValue, @storeId)",
				new[]
					{
						new SqlParameter("@name", name),
						new SqlParameter("@localeSetting", localeSetting),
						new SqlParameter("@configValue", configValue),
						new SqlParameter("@storeId", storeId),
					});
		}
	}

	public static class StringResourceManager
	{
		private static Dictionary<int, StringResources> storeStrings = new Dictionary<int, StringResources>();

		private static StringResources EnsureStoreStringResourcesExists(int storeId)
		{
			return EnsureStoreStringResourcesExists(storeStrings, storeId);
		}

		private static StringResources EnsureStoreStringResourcesExists(Dictionary<int, StringResources> data, int storeId)
		{
			if(!data.ContainsKey(storeId))
			{
				data.Add(storeId, new StringResources());
			}

			return data[storeId];
		}

		/// <summary>
		/// Thread safe lock object
		/// </summary>
		private static object syncLock = new object();

		public static void LoadAllStrings(bool tryToReload)
		{
			lock (syncLock)
			{
				// temporary data holder upon loading strings
				var data = new Dictionary<int, StringResources>();

				LoadAllStringsFromDB(data);

				if(HasAnyStrings() == false && tryToReload)
				{
					// no content from db
					var locales = GetLocales();

					// get all available locales and check if there's an uploaded excel file for that locale
					// and then populate the db from the extracted excel file per locale
					foreach(var locale in locales)
					{
						if(tryToReload && HasNoStringResourceInDB(locale))
						{
							// there doesn't seem to be any string resources for this locale in the db table, so try to load them from the 
							// excel spreadsheet for this locale:
							LoadStringResourceSpreadsheets(locale);
						}
					}

					// re-read from DB
					LoadAllStringsFromDB(data);
				}

				// clear the main repository
				storeStrings.Clear();

				// push the read data into the main repository                
				foreach(var storeId in data.Keys)
				{
					var strings = data[storeId];
					storeStrings.Add(storeId, strings);
				}
			}
		}

		private static void LoadAllStringsFromDB(Dictionary<int, StringResources> data)
		{
			Action<IDataReader> readAction = (rs) =>
			{
				while(rs.Read())
				{
					var str = new StringResource(DB.RSFieldInt(rs, "StringResourceID"), DB.RSField(rs, "Name"), DB.RSField(rs, "LocaleSetting"), DB.RSField(rs, "ConfigValue"), DB.RSFieldBool(rs, "Modified"));
					str.StoreId = rs.FieldInt("StoreId");

					var strings = EnsureStoreStringResourcesExists(data, str.StoreId);
					strings.Add(str);
				}
			};


			DB.UseDataReader("aspdnsf_getStringresource", readAction);
		}

		public static List<string> GetLocales()
		{
			var locales = new List<string>();
			Action<IDataReader> readAction = (rs) =>
			{
				while(rs.Read())
				{
					locales.Add(rs.Field("Name"));
				}
			};

			string query = "select * from LocaleSetting  with (NOLOCK)  order by displayorder,description";
			DB.UseDataReader(query, readAction);

			return locales;
		}

		/// <summary>
		/// Determines if a given locale contains any string resource files in the
		/// [appRoot]/StringResources folder
		/// </summary>
		/// <param name="locale">The locale to look for string resource files for</param>
		/// <returns>Returns true if ANY string resource files exist for a given locale.
		/// Else, returns false.</returns>
		public static bool CheckStringResourceExcelFileExists(string locale)
		{
			return GetStringResourceFilesForLocale(locale).Any();
		}

		/// <summary>
		/// Returns a string array of filenames containing all string resource excel files for a given locale
		/// </summary>
		/// <param name="locale">Locale to retrieve string resource files for</param>
		/// <returns>A collection of paths to the string resource files for the given locale.</returns>
		public static IEnumerable<string> GetStringResourceFilesForLocale(string locale)
		{
			return Directory.GetFiles(CommonLogic.SafeMapPath("~/stringresources"), "*." + locale + ".csv", SearchOption.TopDirectoryOnly)
				.Concat(Directory.GetFiles(CommonLogic.SafeMapPath("~/stringresources"), "*." + locale + ".xls", SearchOption.TopDirectoryOnly));
		}

		static public void LoadStringResourceSpreadsheets(string localeSetting)
		{
			if(!CheckStringResourceExcelFileExists(localeSetting))
				return;

			// Find all string resource excel files in <appRoot>/StringResources pertaining to the specified locale
			var files = GetStringResourceFilesForLocale(localeSetting);

			try
			{
				var importer = new StringResourceImporter();
				importer.Import(localeSetting, files, StringResourceImporter.ImportOption.Default);
			}
			catch(Exception ex)
			{
				SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Alert);
			}
		}

		public static bool HasAnyStrings()
		{
			return storeStrings.Values.Any(strings => strings.Count > 0);
		}

		private static bool HasNoStringResourceInDB(string locale)
		{
			var count = DB.GetSqlN(String.Format("select count(*) as N from StringResource  with (NOLOCK)  where LocaleSetting={0}", locale.DBQuote()));
			return count == 0;
		}

		public static StringResource GetStringResource(int storeId, string localeSetting, string name)
		{
			var strings = EnsureStoreStringResourcesExists(storeId);
			return strings[localeSetting, name];
		}
	}
}
