// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Xml;
using System.Xml.Xsl;

namespace AspDotNetStorefrontCore
{
	/// <summary>
	/// Summary description for DB.
	/// </summary>
	public class DB
	{
		static CultureInfo SqlServerCulture = new CultureInfo(CommonLogic.Application("DBSQLServerLocaleSetting"));

		public DB() { }

		static private String _activeDBConn = SetDBConn();

		static private String SetDBConn()
		{
			String s = ConfigurationManager.ConnectionStrings["DBConn"].ConnectionString;
			return s;
		}

		static public String GetDBConn()
		{
			return _activeDBConn;
		}

		static public SqlConnection dbConn()
		{
			return new SqlConnection(DB.GetDBConn());
		}

		static public string GetTableIdentityField(string TableName)
		{
			if(TableName.Length == 0)
				return string.Empty;

			var tmpS = string.Empty;

			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(var rs = DB.GetRS("select name from syscolumns with (NOLOCK) where id = object_id(" + DB.SQuote(TableName) + ") and colstat & 1 = 1", con))
				{
					if(rs.Read())
					{
						tmpS = DB.RSField(rs, "name");
					}
				}
			}

			return tmpS;
		}

		static public string GetTableColumnDataType(string TableName, string ColumnName)
		{
			if(TableName.Length == 0 || ColumnName.Length == 0)
			{
				return string.Empty;
			}
			var tmpS = string.Empty;

			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(var rs = DB.GetRS("select st.name from syscolumns sc with (NOLOCK) join systypes st with (NOLOCK) on sc.xtype = st.xtype where id = object_id(" + DB.SQuote(TableName) + ") and sc.name = " + DB.SQuote(ColumnName) + "", con))
				{
					if(rs.Read())
					{
						tmpS = DB.RSField(rs, "name");
					}
				}
			}

			return tmpS;
		}

		public static String SQuote(String s)
		{
			int len = s.Length + 25;
			StringBuilder tmpS = new StringBuilder(len); // hopefully only one alloc
			tmpS.Append("N'");
			tmpS.Append(s.Replace("'", "''"));
			tmpS.Append("'");
			return tmpS.ToString();
		}

		public static String SQuoteNotUnicode(String s)
		{
			int len = s.Length + 25;
			StringBuilder tmpS = new StringBuilder(len); // hopefully only one alloc
			tmpS.Append("'");
			tmpS.Append(s.Replace("'", "''"));
			tmpS.Append("'");
			return tmpS.ToString();
		}

		public static String DateQuote(String s)
		{
			int len = s.Length + 25;
			StringBuilder tmpS = new StringBuilder(len); // hopefully only one alloc
			tmpS.Append("'");
			tmpS.Append(s.Replace("'", "''"));
			tmpS.Append("'");
			return tmpS.ToString();
		}

		public static String DateQuote(DateTime dt)
		{
			return DateQuote(Localization.ToDBDateTimeString(dt));
		}

		/// <summary>
		/// Provides a controlled construct for for which to use a DataContext object
		/// </summary>
		/// <param name="action"></param>
		public static void UseDataReader(string query, Action<IDataReader> action)
		{
			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();

				using(var rs = DB.GetRS(query, con))
				{
					action(rs);
				}
			}
		}

		/// <summary>
		/// Provides a controlled construct for for which to use a DataContext object
		/// </summary>
		/// <param name="command">SQL command to execute</param>
		/// <param name="action"></param>
		public static void UseDataReader(SqlCommand command, Action<IDataReader> action)
		{
			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				command.Connection = con;
				con.Open();
				using(var rs = command.ExecuteReader())
				{
					action(rs);
				}
			}
		}

		public static void UseDataReader(string query, SqlParameter[] parameters, Action<IDataReader> action)
		{
			using(var con = new SqlConnection(DB.GetDBConn()))
			using(var command = new SqlCommand(query, con))
			{
				command.Parameters.AddRange(parameters);
				con.Open();
				using(var rs = command.ExecuteReader())
					action(rs);
			}
		}

		static public IDataReader GetRS(string sql, SqlConnection connection, params SqlParameter[] parameters)
		{
			if(CommonLogic.ApplicationBool("DumpSQL"))
				HttpContext.Current.Response.Write("SQL=" + sql + "\n");

			using(var command = new SqlCommand(sql, connection))
			{
				if(parameters != null)
					command.Parameters.AddRange(parameters);

				return command.ExecuteReader();
			}
		}

		static public IDataReader GetRS(String sql, SqlParameter[] parameters, SqlConnection connection)
		{
			return GetRS(sql, connection, parameters);
		}

		static public IDataReader GetRS(String sql, SqlTransaction transaction, params SqlParameter[] parameters)
		{
			if(transaction == null)
			{
				// we can't use the other overloads for this
				// since one is obsolete and the other one requires an 
				// SqlConnection object, creating one here, there's no way to tell
				// when we can/should dispose it.
				throw new ArgumentNullException("Transaction cannot be null!!");
			}

			if(CommonLogic.ApplicationBool("DumpSQL"))
				HttpContext.Current.Response.Write("SQL=" + sql + "\n");

			using(var command = new SqlCommand(sql, transaction.Connection, transaction))
			{
				if(parameters != null)
					command.Parameters.AddRange(parameters);

				return command.ExecuteReader();
			}
		}

		public static SqlDataReader ExecuteStoredProcReader(String StoredProcName, SqlParameter[] spa, SqlConnection dbconn)
		{
			using(SqlCommand dbCommand = new SqlCommand(StoredProcName, dbconn))
			{
				dbCommand.CommandType = CommandType.StoredProcedure;

				foreach(SqlParameter sp in spa)
				{ dbCommand.Parameters.Add(sp); }

				SqlDataReader dbReader;
				dbReader = dbCommand.ExecuteReader();
				// Always call Read before accessing data.
				return dbReader;
			}

		}

		public static int ExecuteStoredProcInt(String StoredProcName, SqlParameter[] spa)
		{
			bool Flag = false;
			string RetValName = String.Empty;
			SqlConnection dbconn = new SqlConnection();
			dbconn.ConnectionString = GetDBConn();
			dbconn.Open();

			SqlCommand dbCommand = new SqlCommand(StoredProcName, dbconn);
			dbCommand.CommandType = CommandType.StoredProcedure;

			foreach(SqlParameter sp in spa)
			{
				if(sp.Direction == ParameterDirection.Output)
				{
					Flag = true; // There is one output parameter to read value from, return this value
					RetValName = sp.ParameterName;
				}
				dbCommand.Parameters.Add(sp);
			}

			try
			{
				int retval = dbCommand.ExecuteNonQuery();
				if(Flag)
					retval = Int32.Parse(dbCommand.Parameters[RetValName].Value.ToString());

				return retval;
			}
			catch(Exception ex)
			{
				throw (ex);
			}
			finally
			{
				dbCommand.Dispose();
				dbconn.Close();
				dbconn.Dispose();
			}
		}

		public static int ExecuteStoredProcInt(String StoredProcName, SqlParameter[] spa, SqlTransaction trans)
		{
			bool Flag = false;
			string RetValName = String.Empty;

			if(trans == null)
			{
				return ExecuteStoredProcInt(StoredProcName, spa);
			}

			SqlCommand dbCommand = new SqlCommand(StoredProcName, trans.Connection, trans);
			dbCommand.CommandType = CommandType.StoredProcedure;

			foreach(SqlParameter sp in spa)
			{
				if(sp.Direction == ParameterDirection.Output)
				{
					Flag = true; // There is one output parameter to read value from, return this value
					RetValName = sp.ParameterName;
				}
				dbCommand.Parameters.Add(sp);
			}

			try
			{
				int retval = dbCommand.ExecuteNonQuery();
				if(Flag)
				{
					retval = Int32.Parse(dbCommand.Parameters[RetValName].Value.ToString());
				}
				dbCommand.Dispose();

				return retval;
			}
			catch(Exception ex)
			{
				dbCommand.Dispose();
				throw (ex);
			}

		}

		public static SqlParameter SetValueDecimal(SqlParameter sparam, object value)
		{
			if(value == null)
			{
				sparam.Value = DBNull.Value;
			}
			else
			{
				sparam.Value = (decimal)value;
			}

			return sparam;
		}

		public static SqlParameter SetValueBool(SqlParameter sparam, object value)
		{
			if(value == null)
			{
				sparam.Value = DBNull.Value;
			}
			else
			{
				sparam.Value = (bool)value;
			}

			return sparam;
		}

		public static SqlParameter SetValueSmallInt(SqlParameter sparam, object value)
		{
			if(value == null)
			{
				sparam.Value = DBNull.Value;
			}
			else
			{
				sparam.Value = (Int16)value;
			}

			return sparam;
		}

		public static SqlParameter SetValueTinyInt(SqlParameter sparam, object value)
		{
			if(value == null)
			{
				sparam.Value = DBNull.Value;
			}
			else
			{
				sparam.Value = Convert.ToByte(Convert.ToInt32(value));
			}

			return sparam;
		}

		public static SqlParameter SetValueInt(SqlParameter sparam, object value)
		{
			if(value == null)
			{
				sparam.Value = DBNull.Value;
			}
			else
			{
				sparam.Value = (int)value;
			}

			return sparam;
		}

		public static SqlParameter SetValueBigInt(SqlParameter sparam, object value)
		{
			if(value == null)
			{
				sparam.Value = DBNull.Value;
			}
			else
			{
				sparam.Value = (Int64)value;
			}

			return sparam;
		}

		public static SqlParameter SetValueDateTime(SqlParameter sparam, object value)
		{
			if(value == null)
			{
				sparam.Value = DBNull.Value;
			}
			else
			{
				sparam.Value = (DateTime)value;
			}
			return sparam;
		}

		public static SqlParameter SetValueGUID(SqlParameter sparam, object value)
		{
			if(value == null)
			{
				sparam.Value = DBNull.Value;
			}
			else
			{
				sparam.Value = (Guid)value;
			}
			return sparam;
		}

		public static SqlParameter SetValue(SqlParameter sparam, object value)
		{
			if(value == null)
			{
				sparam.Value = DBNull.Value;
			}
			else
			{
				sparam.Value = value;
			}
			return sparam;
		}

		public static SqlParameter CreateSQLParameter(String ParameterName, SqlDbType ParamterType, int ParamterLength, object Value, ParameterDirection Direction)
		{
			SqlParameter sq = new SqlParameter(ParameterName, ParamterType, ParamterLength);
			sq.Direction = Direction;

			switch(ParamterType)
			{
				case SqlDbType.Decimal:
				case SqlDbType.Money:
				case SqlDbType.Float:
				case SqlDbType.Real:
				case SqlDbType.SmallMoney:
					sq = SetValueDecimal(sq, Value);
					break;
				case SqlDbType.Int:
					sq = SetValueInt(sq, Value);
					break;
				case SqlDbType.BigInt:
					sq = SetValueBigInt(sq, Value);
					break;
				case SqlDbType.TinyInt:
					sq = SetValueTinyInt(sq, Value);
					break;
				case SqlDbType.NVarChar:
				case SqlDbType.NChar:
					sq = SetValue(sq, Value);
					break;
				case SqlDbType.VarChar:
				case SqlDbType.Char:
					sq = SetValue(sq, Value);
					break;
				case SqlDbType.NText:
				case SqlDbType.Text:
					sq = SetValue(sq, Value);
					break;
				case SqlDbType.Xml:
					sq = SetValue(sq, Value);
					break;
				case SqlDbType.Bit:
					sq = SetValueBool(sq, Value);
					break;
				case SqlDbType.DateTime:
				case SqlDbType.SmallDateTime:
					sq = SetValueDateTime(sq, Value);
					break;
				case SqlDbType.SmallInt:
					sq = SetValueSmallInt(sq, Value);
					break;
				case SqlDbType.UniqueIdentifier:
					sq = SetValueGUID(sq, Value);
					break;


			}
			return sq;
		}

		public static SqlParameter[] CreateSQLParameterArray(SqlParameter[] spa, SqlParameter sp)
		{
			Array.Resize(ref spa, spa.Length + 1);
			spa[spa.Length - 1] = sp;

			return spa;
		}

		static public void ExecuteSQL(string sql, params SqlParameter[] parameters)
		{
			using(var connection = new SqlConnection(DB.GetDBConn()))
			{
				connection.Open();
				ExecuteSQL(sql, connection, parameters);
			}
		}

		static public void ExecuteSQL(string sql, SqlConnection connection, params SqlParameter[] parameters)
		{
			if(CommonLogic.ApplicationBool("DumpSQL"))
				HttpContext.Current.Response.Write("SQL=" + sql + "\n");

			using(var command = new SqlCommand(sql, connection))
			{
				if(parameters != null)
					command.Parameters.AddRange(parameters);

				command.ExecuteNonQuery();
			}
		}

		static public void ExecuteSQL(string sql, SqlTransaction transaction, params SqlParameter[] parameters)
		{
			if(transaction == null)
			{
				ExecuteSQL(sql, parameters);
				return;
			}

			if(CommonLogic.ApplicationBool("DumpSQL"))
				HttpContext.Current.Response.Write("SQL=" + sql + "\n");

			using(var command = new SqlCommand(sql, transaction.Connection, transaction))
			{
				if(parameters != null)
					command.Parameters.AddRange(parameters);

				command.ExecuteNonQuery();
			}
		}

		public static void ExecuteSQL(SqlCommand command)
		{
			if(CommonLogic.ApplicationBool("DumpSQL"))
				HttpContext.Current.Response.Write("SQL=" + command.CommandText + "\n");

			using(var connection = new SqlConnection(DB.GetDBConn()))
			{
				command.Connection = connection;
				connection.Open();
				command.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// Daemon for handling scalar SQL Queries
		/// </summary>
		/// <typeparam name="returnType">The type of value to return from the query</typeparam>
		public static class Scalar<returnType>
		{
			/// <summary>
			/// Executes a SQL Command and returns is Scalar Value
			/// </summary>
			/// <param name="cmd">the SQLCommand to execute</param>
			public static returnType ExecuteScalar(SqlCommand cmd)
			{
				using(var conn = new SqlConnection(GetDBConn()))
				{
					conn.Open();
					cmd.Connection = conn;
					return (returnType)cmd.ExecuteScalar();
				}
			}

		}

		// NOTE FOR DB ACCESSOR FUNCTIONS: AdminSite try/catch block is needed until
		// we convert to the new admin page styles. Our "old" db accessors handled empty
		// recordset conditions, so we need to preserve that for the admin site to add 
		// new products/categories/etc...
		//
		// We do not use try/catch on the store site for speed

		// ----------------------------------------------------------------
		//
		// SIMPLE ROW FIELD ROUTINES
		//
		// ----------------------------------------------------------------

		public static String RowField(DataRow row, String fieldname)
		{
			if(AppLogic.IsAdminSite)
			{
				try
				{
					if(Convert.IsDBNull(row[fieldname]))
					{
						return String.Empty;
					}
					return Convert.ToString(row[fieldname]);
				}
				catch
				{
					return String.Empty;
				}
			}
			else
			{
				if(Convert.IsDBNull(row[fieldname]))
				{
					return String.Empty;
				}
				return Convert.ToString(row[fieldname]);
			}
		}

		public static String RowFieldByLocale(DataRow row, String fieldname, String LocaleSetting)
		{
			String tmpS = String.Empty;
			if(AppLogic.IsAdminSite)
			{
				try
				{
					if(Convert.IsDBNull(row[fieldname]))
					{
						tmpS = String.Empty;
					}
					else
					{
						tmpS = Convert.ToString(row[fieldname]);
					}
				}
				catch
				{
					tmpS = String.Empty;
				}
			}
			else
			{
				if(Convert.IsDBNull(row[fieldname]))
				{
					tmpS = String.Empty;
				}
				else
				{
					tmpS = Convert.ToString(row[fieldname]);
				}
			}

			return XmlCommon.GetLocaleEntry(tmpS, LocaleSetting, true);
		}

		public static bool RowFieldBool(DataRow row, String fieldname)
		{
			if(AppLogic.IsAdminSite)
			{
				try
				{
					if(Convert.IsDBNull(row[fieldname]))
					{
						return false;
					}

					String s = row[fieldname].ToString();

					return (s.Equals("TRUE", StringComparison.InvariantCultureIgnoreCase) ||
							s.Equals("YES", StringComparison.InvariantCultureIgnoreCase) ||
							s.Equals("1", StringComparison.InvariantCultureIgnoreCase));
				}
				catch
				{
					return false;
				}
			}
			else
			{
				if(Convert.IsDBNull(row[fieldname]))
				{
					return false;
				}

				String s = row[fieldname].ToString();

				return (s.Equals("TRUE", StringComparison.InvariantCultureIgnoreCase) ||
						s.Equals("YES", StringComparison.InvariantCultureIgnoreCase) ||
						s.Equals("1", StringComparison.InvariantCultureIgnoreCase));
			}
		}

		public static String RowFieldGUID(DataRow row, String fieldname)
		{
			if(AppLogic.IsAdminSite)
			{
				try
				{
					if(Convert.IsDBNull(row[fieldname]))
					{
						return String.Empty;
					}
					return Convert.ToString(row[fieldname]);
				}
				catch
				{
					return String.Empty;
				}
			}
			else
			{
				if(Convert.IsDBNull(row[fieldname]))
				{
					return String.Empty;
				}
				return Convert.ToString(row[fieldname]);
			}
		}

		public static int RowFieldInt(DataRow row, String fieldname)
		{
			if(AppLogic.IsAdminSite)
			{
				try
				{
					if(Convert.IsDBNull(row[fieldname]))
					{
						return 0;
					}
					return Convert.ToInt32(row[fieldname]);
				}
				catch
				{
					return 0;
				}
			}
			else
			{
				if(Convert.IsDBNull(row[fieldname]))
				{
					return 0;
				}
				return Convert.ToInt32(row[fieldname]);
			}
		}

		public static Decimal RowFieldDecimal(DataRow row, String fieldname)
		{
			if(AppLogic.IsAdminSite)
			{
				try
				{
					if(Convert.IsDBNull(row[fieldname]))
					{
						return System.Decimal.Zero;
					}
					return Convert.ToDecimal(row[fieldname]);
				}
				catch
				{
					return System.Decimal.Zero;
				}
			}
			else
			{
				if(Convert.IsDBNull(row[fieldname]))
				{
					return System.Decimal.Zero;
				}
				return Convert.ToDecimal(row[fieldname]);
			}
		}

		// ----------------------------------------------------------------
		//
		// SIMPLE RS FIELD ROUTINES
		//
		// ----------------------------------------------------------------

		public static String RSField(IDataReader rs, String fieldname)
		{
			if(AppLogic.IsAdminSite)
			{
				try
				{
					int idx = rs.GetOrdinal(fieldname);
					if(rs.IsDBNull(idx))
					{
						return String.Empty;
					}
					return rs.GetString(idx);
				}
				catch
				{
					return String.Empty;
				}
			}
			else
			{
				int idx = rs.GetOrdinal(fieldname);
				if(rs.IsDBNull(idx))
				{
					return String.Empty;
				}
				return rs.GetString(idx);
			}
		}

		public static String RSFieldByLocale(IDataReader rs, String fieldname, String LocaleSetting)
		{
			String tmpS = String.Empty;
			if(AppLogic.IsAdminSite)
			{
				try
				{
					int idx = rs.GetOrdinal(fieldname);
					if(rs.IsDBNull(idx))
					{
						tmpS = String.Empty;
					}
					else
					{
						tmpS = rs.GetString(idx);
					}
				}
				catch
				{
					tmpS = String.Empty;
				}
			}
			else
			{
				int idx = rs.GetOrdinal(fieldname);
				if(rs.IsDBNull(idx))
				{
					tmpS = String.Empty;
				}
				else
				{
					tmpS = rs.GetString(idx);
				}
			}
			if(LocaleSetting == Localization.ALL_LOCALES)
			{
				return tmpS;
			}
			else
			{
				return XmlCommon.GetLocaleEntry(tmpS, LocaleSetting, true);
			}
		}

		public static bool RSFieldBool(IDataReader rs, String fieldname)
		{
			if(AppLogic.IsAdminSite)
			{
				try
				{
					int idx = rs.GetOrdinal(fieldname);
					if(rs.IsDBNull(idx))
					{
						return false;
					}

					String s = rs[fieldname].ToString();

					return (s.Equals("TRUE", StringComparison.InvariantCultureIgnoreCase) ||
							s.Equals("YES", StringComparison.InvariantCultureIgnoreCase) ||
							s.Equals("1", StringComparison.InvariantCultureIgnoreCase));
				}
				catch
				{
					return false;
				}
			}
			else
			{
				int idx = rs.GetOrdinal(fieldname);
				if(rs.IsDBNull(idx))
				{
					return false;
				}

				String s = rs[fieldname].ToString();

				return (s.Equals("TRUE", StringComparison.InvariantCultureIgnoreCase) ||
						s.Equals("YES", StringComparison.InvariantCultureIgnoreCase) ||
						s.Equals("1", StringComparison.InvariantCultureIgnoreCase));
			}
		}

		public static String RSFieldGUID(IDataReader rs, String fieldname)
		{
			if(AppLogic.IsAdminSite)
			{
				try
				{
					int idx = rs.GetOrdinal(fieldname);
					if(rs.IsDBNull(idx))
					{
						return String.Empty;
					}
					return rs.GetGuid(idx).ToString();
				}
				catch
				{
					return String.Empty;
				}
			}
			else
			{
				int idx = rs.GetOrdinal(fieldname);
				if(rs.IsDBNull(idx))
				{
					return String.Empty;
				}
				return rs.GetGuid(idx).ToString();
			}
		}

		public static Guid RSFieldGUID2(IDataReader rs, String fieldname)
		{
			if(AppLogic.IsAdminSite)
			{
				try
				{
					int idx = rs.GetOrdinal(fieldname);
					if(rs.IsDBNull(idx))
					{
						return new Guid("00000000000000000000000000000000");
					}
					return rs.GetGuid(idx);
				}
				catch
				{
					return new Guid("00000000000000000000000000000000");
				}
			}
			else
			{
				int idx = rs.GetOrdinal(fieldname);
				if(rs.IsDBNull(idx))
				{
					return new Guid("00000000000000000000000000000000");
				}
				return rs.GetGuid(idx);
			}
		}

		public static Byte RSFieldByte(IDataReader rs, String fieldname)
		{
			if(AppLogic.IsAdminSite)
			{
				try
				{
					int idx = rs.GetOrdinal(fieldname);
					if(rs.IsDBNull(idx))
					{
						return 0;
					}
					return rs.GetByte(idx);
				}
				catch
				{
					return 0;
				}
			}
			else
			{
				int idx = rs.GetOrdinal(fieldname);
				if(rs.IsDBNull(idx))
				{
					return 0;
				}
				return rs.GetByte(idx);
			}
		}

		public static int RSFieldInt(IDataReader rs, String fieldname)
		{
			if(AppLogic.IsAdminSite)
			{
				try
				{
					int idx = rs.GetOrdinal(fieldname);
					if(rs.IsDBNull(idx))
					{
						return 0;
					}
					return rs.GetInt32(idx);
				}
				catch
				{
					return 0;
				}
			}
			else
			{
				int idx = rs.GetOrdinal(fieldname);
				if(rs.IsDBNull(idx))
				{
					return 0;
				}
				return rs.GetInt32(idx);
			}
		}

		public static int RSFieldTinyInt(IDataReader rs, String fieldname)
		{
			if(AppLogic.IsAdminSite)
			{
				try
				{
					int idx = rs.GetOrdinal(fieldname);
					if(rs.IsDBNull(idx))
					{
						return 0;
					}
					return Localization.ParseNativeInt(rs[idx].ToString());
				}
				catch
				{
					return 0;
				}
			}
			else
			{
				int idx = rs.GetOrdinal(fieldname);
				if(rs.IsDBNull(idx))
				{
					return 0;
				}
				return Localization.ParseNativeInt(rs[idx].ToString());
			}
		}

		public static long RSFieldLong(IDataReader rs, String fieldname)
		{
			if(AppLogic.IsAdminSite)
			{
				try
				{
					int idx = rs.GetOrdinal(fieldname);
					if(rs.IsDBNull(idx))
					{
						return 0;
					}
					return rs.GetInt64(idx);
				}
				catch
				{
					return 0;
				}
			}
			else
			{
				int idx = rs.GetOrdinal(fieldname);
				if(rs.IsDBNull(idx))
				{
					return 0;
				}
				return rs.GetInt64(idx);
			}
		}

		public static Decimal RSFieldDecimal(IDataReader rs, String fieldname)
		{
			if(AppLogic.IsAdminSite)
			{
				try
				{
					int idx = rs.GetOrdinal(fieldname);
					if(rs.IsDBNull(idx))
					{
						return System.Decimal.Zero;
					}
					return rs.GetDecimal(idx);
				}
				catch
				{
					return System.Decimal.Zero;
				}
			}
			else
			{
				int idx = rs.GetOrdinal(fieldname);
				if(rs.IsDBNull(idx))
				{
					return System.Decimal.Zero;
				}
				return rs.GetDecimal(idx);
			}
		}

		public static DateTime RSFieldDateTime(IDataReader rs, String fieldname)
		{
			if(AppLogic.IsAdminSite)
			{
				try
				{
					int idx = rs.GetOrdinal(fieldname);
					if(rs.IsDBNull(idx))
					{
						return System.DateTime.MinValue;
					}
					return Convert.ToDateTime(rs[idx], SqlServerCulture);

				}
				catch
				{
					return System.DateTime.MinValue;
				}
			}
			else
			{
				int idx = rs.GetOrdinal(fieldname);
				if(rs.IsDBNull(idx))
				{
					return System.DateTime.MinValue;
				}
				return Convert.ToDateTime(rs[idx], SqlServerCulture);

			}
		}

		public static string GetNewGUID()
		{
			return Guid.NewGuid().ToString();
		}

		static public int GetSqlN(string sql, params SqlParameter[] parameters)
		{
			using(var connection = new SqlConnection(DB.GetDBConn()))
			{
				connection.Open();
				return GetSqlN(sql, connection, parameters);
			}
		}

		static public int GetSqlN(string sql, SqlConnection connection, params SqlParameter[] parameters)
		{
			using(var reader = DB.GetRS(sql, parameters, connection))
				if(reader.Read())
					return DB.RSFieldInt(reader, "N");

			return 0;
		}

		static public int GetSqlN(string sql, SqlTransaction transaction, params SqlParameter[] parameters)
		{
			if(transaction == null)
				return GetSqlN(sql, parameters);

			using(var reader = DB.GetRS(sql, transaction, parameters))
				if(reader.Read())
					return DB.RSFieldInt(reader, "N");

			return 0;
		}

		static public long GetSqlNLong(string sql, params SqlParameter[] parameters)
		{
			using(var connection = new SqlConnection(GetDBConn()))
			{
				connection.Open();
				using(var reader = DB.GetRS(sql, connection, parameters))
					if(reader.Read())
						return DB.RSFieldLong(reader, "N");
			}

			return 0;
		}

		static public decimal GetSqlNDecimal(string sql, params SqlParameter[] parameters)
		{
			using(var connection = new SqlConnection(GetDBConn()))
			{
				connection.Open();
				using(var reader = GetRS(sql, connection, parameters))
					return reader.Read()
						? RSFieldDecimal(reader, "N")
						: 0;
			}
		}

		static public void ExecuteLongTimeSQL(String Sql, int TimeoutSecs)
		{
			if(CommonLogic.ApplicationBool("DumpSQL"))
			{
				HttpContext.Current.Response.Write("SQL=" + Sql + "\n");
			}
			var dbconn = new SqlConnection();
			dbconn.ConnectionString = DB.GetDBConn();
			dbconn.Open();
			var cmd = new SqlCommand(Sql, dbconn);
			cmd.CommandTimeout = TimeoutSecs;
			try
			{
				cmd.ExecuteNonQuery();
			}
			catch(Exception ex)
			{
				throw (ex);
			}
			finally
			{
				cmd.Dispose();
				dbconn.Close();
				dbconn.Dispose();
			}
		}

		static public string GetSqlS(string sql, params SqlParameter[] parameters)
		{
			using(var connection = new SqlConnection(DB.GetDBConn()))
			using(var command = new SqlCommand())
			{
				command.Connection = connection;
				command.CommandText = sql;

				if(parameters != null)
					command.Parameters.AddRange(parameters.ToArray());

				connection.Open();

				using(var reader = command.ExecuteReader())
					if(reader.Read())
						return RSFieldByLocale(reader, "S", Thread.CurrentThread.CurrentUICulture.Name);
			}

			return string.Empty;
		}

		static public decimal GetSqlNDecimal(String Sql, SqlConnection dbconn)
		{
			decimal N = System.Decimal.Zero;

			using(IDataReader rs = DB.GetRS(Sql, dbconn))
			{
				if(rs.Read())
				{
					N = DB.RSFieldDecimal(rs, "N");
				}
			}

			return N;
		}

		static public string GetSqlSAllLocales(string Sql)
		{
			var S = string.Empty;

			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(var rs = DB.GetRS(Sql, con))
				{
					if(rs.Read())
					{
						S = DB.RSField(rs, "S");
						if(S.Equals(DBNull.Value))
						{
							S = string.Empty;
						}
					}
				}
			}

			return S;
		}

		static public XmlDocument GetSqlXmlDoc(string Sql, string xslTranformFile)
		{
			if(Sql.ToUpper(CultureInfo.InvariantCulture).IndexOf("FOR XML") < 1)
			{
				Sql += " FOR XML AUTO";
			}
			var s = new StringBuilder(4096);

			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(var rs = DB.GetRS(Sql, con))
				{
					while(rs.Read())
					{
						s.Append(rs.GetString(0));
					}
				}
			}

			var xdoc = new XmlDocument();
			xdoc.LoadXml("<root>" + s.ToString() + "</root>");
			if(xslTranformFile != null && xslTranformFile.Trim() != "")
			{
				var xsl = new XslCompiledTransform();
				xsl.Load(xslTranformFile);
				var tw = new StringWriter();
				xsl.Transform(xdoc, null, tw);
				xdoc.LoadXml(tw.ToString());
			}
			return xdoc;
		}

		static public int GetXml(IDataReader dr, string rootEl, string rowEl, bool IsPagingProc, ref StringBuilder Xml)
		{
			int rows = 0;
			if(rootEl.Length != 0)
			{
				Xml.Append("<");
				Xml.Append(rootEl);
				Xml.Append(">");
			}
			while(dr.Read())
			{
				++rows;
				if(rowEl.Length == 0)
				{
					Xml.Append("<row>");
				}
				else
				{
					Xml.Append("<");
					Xml.Append(rowEl);
					Xml.Append(">");
				}
				for(int i = 0; i < dr.FieldCount; i++)
				{
					string elname = dr.GetName(i).Replace(" ", "_");
					if(dr.IsDBNull(i))
					{
						Xml.Append("<");
						Xml.Append(elname);
						Xml.Append("/>");
					}
					else
					{
						if(Convert.ToString(dr.GetValue(i)).StartsWith("<ml>", StringComparison.InvariantCultureIgnoreCase))
						{
							Xml.Append("<");
							Xml.Append(elname);
							Xml.Append(">");
							Xml.Append(Convert.ToString(dr.GetValue(i)));
							Xml.Append("</");
							Xml.Append(elname);
							Xml.Append(">");

						}
						else
						{
							if(dr.GetFieldType(i).Equals(DbType.DateTime))
							{
								Xml.Append("<");
								Xml.Append(elname);
								Xml.Append(">");
								Xml.Append(Localization.ParseLocaleDateTime(dr.GetString(i), Thread.CurrentThread.CurrentUICulture.Name));
								Xml.Append("</");
								Xml.Append(elname);
								Xml.Append(">");
							}
							else
							{
								Xml.Append("<");
								Xml.Append(elname);
								Xml.Append(">");
								Xml.Append(XmlCommon.XmlEncodeAsIs(Convert.ToString(dr.GetValue(i))));
								Xml.Append("</");
								Xml.Append(elname);
								Xml.Append(">");

							}
						}
					}
				}
				if(rowEl.Length == 0)
				{
					Xml.Append("</row>");
				}
				else
				{
					Xml.Append("</");
					Xml.Append(rowEl);
					Xml.Append(">");
				}

			}
			if(rootEl.Length != 0)
			{
				Xml.Append("</");
				Xml.Append(rootEl);
				Xml.Append(">");
			}
			int ResultSet = 1;
			while(dr.NextResult())
			{
				ResultSet++;
				if(IsPagingProc)
				{
					if(rootEl.Length != 0)
					{
						Xml.Append("<");
						Xml.Append(rootEl);
						Xml.Append("Paging>");
					}
					else
					{
						Xml.Append("<Paging>");
					}
				}
				else
				{
					if(rootEl.Length != 0)
					{
						Xml.Append("<");
						Xml.Append(rootEl);
						Xml.Append(ResultSet.ToString());
						Xml.Append(">");
					}
				}
				while(dr.Read())
				{
					if(!IsPagingProc)
					{
						if(rowEl.Length == 0)
						{
							Xml.Append("<row>");
						}
						else
						{
							Xml.Append("<");
							Xml.Append(rowEl);
							Xml.Append(">");
						}
					}
					for(int i = 0; i < dr.FieldCount; i++)
					{
						string elname = dr.GetName(i).Replace(" ", "_");
						if(dr.IsDBNull(i))
						{
							Xml.Append("<");
							Xml.Append(elname);
							Xml.Append("/>");
						}
						else
						{
							if(System.Convert.ToString(dr.GetValue(i)).StartsWith("<ml>", StringComparison.InvariantCultureIgnoreCase))
							{
								Xml.Append("<");
								Xml.Append(elname);
								Xml.Append(">");
								Xml.Append(System.Convert.ToString(dr.GetValue(i)));
								Xml.Append("</");
								Xml.Append(elname);
								Xml.Append(">");
							}
							else
							{
								Xml.Append("<");
								Xml.Append(elname);
								Xml.Append(">");
								Xml.Append(XmlCommon.XmlEncodeAsIs(System.Convert.ToString(dr.GetValue(i))));
								Xml.Append("</");
								Xml.Append(elname);
								Xml.Append(">");
							}
						}
					}
					if(!IsPagingProc)
					{
						if(rowEl.Length == 0)
						{
							Xml.Append("</row>");
						}
						else
						{
							Xml.Append("</");
							Xml.Append(rowEl);
							Xml.Append(">");
						}
					}
				}
				if(IsPagingProc)
				{
					if(rootEl.Length != 0)
					{
						Xml.Append("</");
						Xml.Append(rootEl);
						Xml.Append("Paging>");
					}
					else
					{
						Xml.Append("</Paging>");
					}
				}
				else
				{
					if(rootEl.Length != 0)
					{
						Xml.Append("</");
						Xml.Append(rootEl);
						Xml.Append(ResultSet.ToString());
						Xml.Append(">");
					}
				}
			}
			dr.Close();
			return rows;
		}


		// assumes a 2nd result set is the PAGING INFO back from the aspdnsf_PageQuery proc!!!
		// looks for aspdnsf_PageQuery in the Sql input to determine this!
		static public int GetXml(string Sql, string rootEl, string rowEl, ref string Xml)
		{
			using(var dbconn = new SqlConnection(DB.GetDBConn()))
			{
				dbconn.Open();
				int i = GetXml(Sql, rootEl, rowEl, ref Xml, dbconn);
				return i;
			}
		}


		// assumes a 2nd result set is the PAGING INFO back from the aspdnsf_PageQuery proc!!!
		// looks for aspdnsf_PageQuery in the Sql input to determine this!
		static public int GetXml(string Sql, string rootEl, string rowEl, ref string Xml, SqlConnection dbconn)
		{
			var IsPagingProc = (Sql.IndexOf("aspdnsf_PageQuery") != -1);
			var s = new StringBuilder(4096);

			var rows = 0;
			if(rootEl.Length != 0)
			{
				s.Append("<");
				s.Append(rootEl);
				s.Append(">");
			}

			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(var rs = DB.GetRS(Sql, con))
				{
					while(rs.Read())
					{
						++rows;
						if(rowEl.Length == 0)
						{
							s.Append("<row>");
						}
						else
						{
							s.Append("<");
							s.Append(rowEl);
							s.Append(">");
						}
						for(int i = 0; i < rs.FieldCount; i++)
						{
							string elname = rs.GetName(i).Replace(" ", "_");
							if(rs.IsDBNull(i))
							{
								s.Append("<");
								s.Append(elname);
								s.Append("/>");
							}
							else
							{
								if(Convert.ToString(rs.GetValue(i)).StartsWith("<ml>", StringComparison.InvariantCultureIgnoreCase))
								{
									s.Append("<");
									s.Append(elname);
									s.Append(">");
									s.Append(Convert.ToString(rs.GetValue(i)));
									s.Append("</");
									s.Append(elname);
									s.Append(">");
								}
								else
								{
									s.Append("<");
									s.Append(elname);
									s.Append(">");
									s.Append(Convert.ToString(rs.GetValue(i)).Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("'", "&apos;").Replace("\"", "&quot;"));
									s.Append("</");
									s.Append(elname);
									s.Append(">");
								}
							}
						}
						if(rowEl.Length == 0)
						{
							s.Append("</row>");
						}
						else
						{
							s.Append("</");
							s.Append(rowEl);
							s.Append(">");
						}

					}
					if(rootEl.Length != 0)
					{
						s.Append("</");
						s.Append(rootEl);
						s.Append(">");
					}
					int ResultSet = 1;
					while(rs.NextResult())
					{
						ResultSet++;
						if(IsPagingProc)
						{
							if(rootEl.Length != 0)
							{
								s.Append("<");
								s.Append(rootEl);
								s.Append("Paging>");
							}
							else
							{
								s.Append("<Paging>");
							}
						}
						else
						{
							if(rootEl.Length != 0)
							{
								s.Append("<");
								s.Append(rootEl);
								s.Append(ResultSet.ToString());
								s.Append(">");
							}
						}
						while(rs.Read())
						{
							if(!IsPagingProc)
							{
								if(rowEl.Length == 0)
								{
									s.Append("<row>");
								}
								else
								{
									s.Append("<");
									s.Append(rowEl);
									s.Append(">");
								}
							}
							for(int i = 0; i < rs.FieldCount; i++)
							{
								string elname = rs.GetName(i).Replace(" ", "_");
								if(rs.IsDBNull(i))
								{
									s.Append("<");
									s.Append(elname);
									s.Append("/>");
								}
								else
								{
									if(Convert.ToString(rs.GetValue(i)).StartsWith("<ml>", StringComparison.InvariantCultureIgnoreCase))
									{
										s.Append("<");
										s.Append(elname);
										s.Append(">");
										s.Append(Convert.ToString(rs.GetValue(i)));
										s.Append("</");
										s.Append(elname);
										s.Append(">");
									}
									else
									{
										s.Append("<");
										s.Append(elname);
										s.Append(">");
										s.Append(Convert.ToString(rs.GetValue(i)).Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("'", "&apos;").Replace("\"", "&quot;"));
										s.Append("</");
										s.Append(elname);
										s.Append(">");
									}
								}
							}
							if(!IsPagingProc)
							{
								if(rowEl.Length == 0)
								{
									s.Append("</row>");
								}
								else
								{
									s.Append("</");
									s.Append(rowEl);
									s.Append(">");
								}
							}
						}
						if(IsPagingProc)
						{
							if(rootEl.Length != 0)
							{
								s.Append("</");
								s.Append(rootEl);
								s.Append("Paging>");

							}
							else
							{
								s.Append("</Paging>");
							}
						}
						else
						{
							if(rootEl.Length != 0)
							{
								s.Append("</");
								s.Append(rootEl);
								s.Append(ResultSet.ToString());
								s.Append(">");
							}
						}
					}
				}
			}

			Xml = s.ToString();
			return rows;
		}

		static public string GetENLocaleXml(SqlDataReader reader, string rootElement, string rowElement)
		{
			var xml = new StringBuilder(1024);
			
			xml.AppendFormat("<{0}>", rootElement);

			while(reader.Read())
			{
				xml.AppendFormat("<{0}>", rowElement);

				for(var i = 0; i < reader.FieldCount; i++)
				{
					var elementName = reader.GetName(i).Replace(" ", "_");

					if(reader.IsDBNull(i))
					{
						xml.AppendFormat("<{0}>", elementName);
					}
					else
					{
						if(Convert.ToString(reader.GetValue(i)).StartsWith("<ml>", StringComparison.InvariantCultureIgnoreCase))
						{
							xml.AppendFormat("<{0}>{1}</{0}>", elementName, Convert.ToString(reader.GetValue(i)));
						}
						else
						{
							var fieldValue = string.Empty;
							var fieldDataType = reader.GetDataTypeName(i);
							if((fieldDataType.Equals("decimal", StringComparison.InvariantCultureIgnoreCase) 
									|| fieldDataType.Equals("money", StringComparison.InvariantCultureIgnoreCase))
									&& CommonLogic.Application("DBSQLServerLocaleSetting") != "en-US")
							{
								xml.AppendFormat("<{0}>{1}</{0}>", elementName, Localization.ParseLocaleDecimal(reader.GetDecimal(i).ToString(), "en-US").ToString());
							}
							else if(reader.GetDataTypeName(i).Equals("datetime", StringComparison.InvariantCultureIgnoreCase) &&
								CommonLogic.Application("DBSQLServerLocaleSetting") != "en-US")
							{
								xml.AppendFormat("<{0}>{1}</{0}>", elementName, Localization.ParseLocaleDateTime(reader.GetDateTime(i).ToString(), "en-US").ToString());
							}
							else
							{
								xml.AppendFormat("<{0}>{1}</{0}>", elementName, Convert.ToString(reader.GetValue(i))
									.Replace("&", "&amp;")
									.Replace("<", "&lt;")
									.Replace(">", "&gt;")
									.Replace("'", "&apos;")
									.Replace("\"", "&quot;"));
							}
						}
					}
				}

				xml.AppendFormat("</{0}>", rowElement);
			}

			xml.AppendFormat("</{0}>", rootElement);

			reader.Close();
			return xml.ToString();
		}
}

	// currently only supported for SQL Server:
	public class DBTransaction
	{
		private ArrayList sqlCommands = new ArrayList(10);

		public DBTransaction() { }

		public void AddCommand(String Sql)
		{
			sqlCommands.Add(Sql);
		}

		// returns true if no errors, or false if ANY Exception is found:
		public bool Commit()
		{
			using(var conn = new SqlConnection())
			{
				conn.ConnectionString = DB.GetDBConn();
				conn.Open();
				using(var trans = conn.BeginTransaction())
				{
					try
					{

						foreach(string s in sqlCommands)
						{
							var comm = new SqlCommand(s, conn);
							comm.Transaction = trans;
							comm.ExecuteNonQuery();
						}
						trans.Commit();
						return true;
					}
					catch
					{
						trans.Rollback();
						return false;
					}
				}
			}
		}

	}

	public enum DateRangeType
	{
		UseDatesAbove,
		Today,
		Yesterday,
		ThisWeek,
		LastWeek,
		ThisMonth,
		LastMonth,
		ThisYear,
		LastYear
	}
}
