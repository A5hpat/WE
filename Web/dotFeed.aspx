<%@ Page Language="C#" AutoEventWireup="false" EnableTheming="false" %>

<%@ Import Namespace="System.Data.SqlClient" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Reflection" %>

<html>
<head runat="server" visible="false">

	<script runat="server">
		interface IAdnsfAdapter
		{
			IDbAdapter GetDBAdapter();
			IXmlPackageAdapter GetXmlPackageAdapter(string xmlPackagePath);
			IAppConfigAdapter GetAppConfigAdapter();
		}

		interface IDbAdapter
		{
			int GetSqlN(string query);
			string GetSqlS(string query);
			string QuoteSql(string query);
			void ExecuteSql(string query);
		}

		interface IXmlPackageAdapter
		{
			string TransformString();
		}

		interface IAppConfigAdapter
		{
			void CreateAppConfig(string name, string description, string value, string group, bool superOnly);
		}

		class AdnsfV10Adapter : IAdnsfAdapter
		{
			readonly Type DbType;
			readonly Type XmlPackageType;
			readonly Type AppConfigType;

			public AdnsfV10Adapter()
			{
				DbType = LoadType("DB");
				XmlPackageType = LoadType("XmlPackage");
				AppConfigType = LoadType("AppConfig");
			}

			public IDbAdapter GetDBAdapter()
			{
				return new DbAdapter(DbType);
			}

			public IXmlPackageAdapter GetXmlPackageAdapter(string xmlPackagePath)
			{
				return new XmlPackageAdapter(XmlPackageType, xmlPackagePath);
			}

			public IAppConfigAdapter GetAppConfigAdapter()
			{
				return new AppConfigAdapter(AppConfigType);
			}

			Type LoadType(string typeName)
			{
				foreach(Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
					if(assembly.FullName.Contains("AspDotNetStorefrontCore"))
						return assembly.GetType(string.Format("AspDotNetStorefrontCore.{0}", typeName), false, true);

				return null;
			}

			class DbAdapter : IDbAdapter
			{
				readonly Type DbType;

				public DbAdapter(Type dbType)
				{
					DbType = dbType;
				}

				public int GetSqlN(string query)
				{
					MethodInfo method = DbType.GetMethod(
						"GetSqlN",
						BindingFlags.Static | BindingFlags.Public,
						null,
						new Type[] { typeof(string), typeof(SqlParameter[]) },
						null);

					return (int)method.Invoke(null, new object[] { query, null });
				}

				public string GetSqlS(string query)
				{
					MethodInfo method = DbType.GetMethod(
						"GetSqlS",
						BindingFlags.Static | BindingFlags.Public,
						null,
						new Type[] { typeof(string), typeof(SqlParameter[]) },
						null);

					object result = method.Invoke(null, new object[] { query, null });
					return result == null
						? null
						: result.ToString();
				}

				public string QuoteSql(string query)
				{
					MethodInfo method = DbType.GetMethod(
						"SQuote",
						BindingFlags.Static | BindingFlags.Public,
						null,
						new Type[] { typeof(string) },
						null);

					object result = method.Invoke(null, new object[] { query });
					return result == null
						? null
						: result.ToString();
				}

				public void ExecuteSql(string query)
				{
					MethodInfo method = DbType.GetMethod(
						"ExecuteSQL",
						BindingFlags.Static | BindingFlags.Public,
						null,
						new Type[] { typeof(string) },
						null);

					method.Invoke(null, new object[] { query });
				}
			}

			class XmlPackageAdapter : IXmlPackageAdapter
			{
				readonly object XmlPackageInstance;
				readonly Type XmlPackageType;

				public XmlPackageAdapter(Type xmlPackageType, string xmlPackagePath)
				{
					XmlPackageType = xmlPackageType;
					XmlPackageInstance = Activator.CreateInstance(
						xmlPackageType,
						BindingFlags.CreateInstance
							| BindingFlags.Public
							| BindingFlags.Instance
							| BindingFlags.OptionalParamBinding,
						null,
						new object[]
						{
							xmlPackagePath,		// string packageName, 
							Type.Missing,		// Customer customer = null, 
							Type.Missing,		// int? skinId = null, 
							Type.Missing,		// string userQuery = null, 
							Type.Missing,		// string additionalRuntimeParms = null, 
							Type.Missing,		// string onlyRunNamedQuery = null, 
							Type.Missing,		// bool useExtensions = true, 
							Type.Missing,		// HtmlHelper htmlHelper = null
						},
						System.Globalization.CultureInfo.CurrentCulture);
				}

				public string TransformString()
				{
					return XmlPackageType
						.GetMethod("TransformString")
						.Invoke(XmlPackageInstance, null)
						.ToString();
				}
			}

			class AppConfigAdapter : IAppConfigAdapter
			{
				readonly Type AppConfigType;

				public AppConfigAdapter(Type appConfigType)
				{
					AppConfigType = appConfigType;
				}

				public void CreateAppConfig(string name, string description, string value, string group, bool superOnly)
				{
					MethodInfo method = AppConfigType.GetMethod(
						"Create",
						BindingFlags.Static | BindingFlags.Public,
						null,
						new Type[] { typeof(string), typeof(string), typeof(string), typeof(string), typeof(bool) },
						null);

					method.Invoke(null, new object[] { name, description, value, group, superOnly });
				}
			}
		}

		class AdnsfLegacyAdapter : IAdnsfAdapter
		{
			readonly Type DbType;
			readonly Type XmlPackageType;
			readonly Type AppConfigType;

			public AdnsfLegacyAdapter()
			{
				DbType = LoadType("DB");
				XmlPackageType = LoadType("XmlPackage2");
				AppConfigType = LoadType("AppConfig");
			}

			public IDbAdapter GetDBAdapter()
			{
				return new DbAdapter(DbType);
			}

			public IXmlPackageAdapter GetXmlPackageAdapter(string xmlPackagePath)
			{
				return new XmlPackageAdapter(XmlPackageType, xmlPackagePath);
			}

			public IAppConfigAdapter GetAppConfigAdapter()
			{
				return new AppConfigAdapter(AppConfigType);
			}

			public Type LoadType(string className)
			{
				foreach(Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
					if(assembly.FullName.Contains("AspDotNetStorefrontCore"))
					{
						Type desiredType = assembly.GetType(string.Format("AspDotNetStorefrontCore.{0}", className), false, true);
						if(desiredType != null)
							return desiredType;
					}
					else if(assembly.FullName.Contains("AspDotNetStorefrontCommon"))
					{
						Type desiredType = assembly.GetType(string.Format("AspDotNetStorefrontCommon.{0}", className), false, true);
						if(desiredType != null)
							return desiredType;
					}

				return null;
			}

			class DbAdapter : IDbAdapter
			{
				readonly Type DbType;

				public DbAdapter(Type dbType)
				{
					DbType = dbType;
				}

				public int GetSqlN(string query)
				{
					MethodInfo method = DbType.GetMethod(
						"GetSqlN",
						BindingFlags.Static | BindingFlags.Public,
						null,
						new Type[] { typeof(string) },
						null);

					return (int)method.Invoke(null, new object[] { query });
				}

				public string GetSqlS(string query)
				{
					MethodInfo method = DbType.GetMethod(
						"GetSqlS",
						BindingFlags.Static | BindingFlags.Public,
						null,
						new Type[] { typeof(string) },
						null);

					object result = method.Invoke(null, new object[] { query });
					return result == null
						? null
						: result.ToString();
				}

				public string QuoteSql(string query)
				{
					MethodInfo method = DbType.GetMethod(
						"SQuote",
						BindingFlags.Static | BindingFlags.Public,
						null,
						new Type[] { typeof(string) },
						null);

					object result = method.Invoke(null, new object[] { query });
					return result == null
						? null
						: result.ToString();
				}

				public void ExecuteSql(string query)
				{
					MethodInfo method = DbType.GetMethod(
						"ExecuteSQL",
						BindingFlags.Static | BindingFlags.Public,
						null,
						new Type[] { typeof(string) },
						null);

					method.Invoke(null, new object[] { query });
				}
			}

			class XmlPackageAdapter : IXmlPackageAdapter
			{
				readonly Type XmlPackageType;
				readonly object XmlPackageInstance;

				public XmlPackageAdapter(Type xmlPackageType, string xmlPackagePath)
				{
					XmlPackageType = xmlPackageType;
					XmlPackageInstance = Activator.CreateInstance(
						xmlPackageType,
						new object[] { xmlPackagePath });
				}

				public string TransformString()
				{
					MethodInfo method = XmlPackageType.GetMethod("TransformString");
					return method.Invoke(XmlPackageInstance, null).ToString();
				}
			}

			class AppConfigAdapter : IAppConfigAdapter
			{
				readonly Type AppConfigType;

				public AppConfigAdapter(Type appConfigType)
				{
					AppConfigType = appConfigType;
				}

				public void CreateAppConfig(string name, string description, string value, string group, bool superOnly)
				{
					MethodInfo method = AppConfigType.GetMethod(
						"Create",
						BindingFlags.Static | BindingFlags.Public,
						null,
						new Type[] { typeof(string), typeof(string), typeof(string), typeof(string), typeof(bool) },
						null);

					method.Invoke(null, new object[] { name, description, value, group, superOnly });
				}
			}
		}

		const string DotFeedPublicKey = "<RSAKeyValue><Modulus>2792gV8Hyld7hYNdouEcEfaKquKEZzPMv6iFJIYm0Va4XbXecTEHXKY/sdv03+lxANRc9EbZ0unJHNrSfTDkeRDCgbokce7Yzc0IIOVMHgjwLoVrCjFyWW0mXteBKm65Rqvjm2FGqjOCcPHoBG3G01sKw40aaQB+FmjNfD8OSvE=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
		const string AccessKeyKey = "DotFeed.AccessKey";
		readonly TimeSpan AuthWindow = new TimeSpan(0, 1, 0);

		string _ConfiguredAccessKey;
		string ConfiguredAccessKey
		{
			get { return _ConfiguredAccessKey; }
			set { _ConfiguredAccessKey = value; }
		}

		string _RawAuthDateParam;
		string RawAuthDateParam
		{
			get { return _RawAuthDateParam; }
			set { _RawAuthDateParam = value; }
		}

		DateTime _AuthDate;
		DateTime AuthDate
		{
			get { return _AuthDate; }
			set { _AuthDate = value; }
		}

		byte[] _ExtendedAuthToken;
		byte[] ExtendedAuthToken
		{
			get { return _ExtendedAuthToken; }
			set { _ExtendedAuthToken = value; }
		}

		IAdnsfAdapter Adnsf;

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			DateTime requestDate = DateTime.Now;

			Version adnsfVersion = GetAdnsfVersion();
			if(adnsfVersion == null)
				Adnsf = new AdnsfLegacyAdapter();
			else
				Adnsf = new AdnsfV10Adapter();

			Response.Clear();

			VerifyConfiguration();
			VerifyParameters();

			string contents;
			using(StreamReader reader = new StreamReader(Request.InputStream))
			{
				contents = reader.ReadToEnd();
				Request.InputStream.Position = 0;

				VerifyAuthentication(requestDate, contents);

				string result = ProcessXmlPackage();
				Response.Write(result);
			}

			Response.End();
		}

		Version GetAdnsfVersion()
		{
			if(HttpContext.Current == null
				|| HttpContext.Current.Items["AdnsfVersion"] == null)
				return null;

			return HttpContext.Current.Items["AdnsfVersion"] as Version;
		}

		void VerifyConfiguration()
		{
			IDbAdapter dbAdapter = Adnsf.GetDBAdapter();
			IAppConfigAdapter appConfigAdapter = Adnsf.GetAppConfigAdapter();

			if(!AppConfigExists(AccessKeyKey))
			{
				appConfigAdapter.CreateAppConfig(AccessKeyKey, "The key you provide to DotFeed to allow it to access your site data.", string.Empty, "DOTFEED", false);

				if(StoreID() != null)
				{
					string sql = string.Format("UPDATE AppConfig SET StoreID = 0 WHERE Name = {0}", dbAdapter.QuoteSql(AccessKeyKey));
					dbAdapter.ExecuteSql(sql);
				}
			}

			string configuredAccessKeyValue = (GetAppConfig(AccessKeyKey) ?? string.Empty).Trim();

			if(string.IsNullOrEmpty(configuredAccessKeyValue))
			{
				ReturnErrorMessage("DotFeed has not been enabled on this store");
				Response.End();
			}

			ConfiguredAccessKey = configuredAccessKeyValue;
		}

		public bool AppConfigExists(string key)
		{
			IDbAdapter dbAdapter = Adnsf.GetDBAdapter();
			Nullable<int> storeId = StoreID();

			string sql;
			if(storeId != null)
				sql = string.Format(
					"select count(*) as N from AppConfig where Name ='{0}' and (StoreId = 0 or StoreId = {1})",
					key,
					storeId);
			else
				sql = string.Format(
					"select count(*) as N from AppConfig where Name = '{0}'",
					key);

			return dbAdapter.GetSqlN(sql) > 0;
		}

		public string GetAppConfig(string key)
		{
			IDbAdapter dbAdapter = Adnsf.GetDBAdapter();
			Nullable<int> storeId = StoreID();

			string sql;
			if(storeId != null)
				sql = string.Format("select top 1 ConfigValue as S from AppConfig where Name ='{0}' and (StoreId = 0 or StoreId = {1}) order by StoreID DESC", key, storeId);
			else
				sql = string.Format("select top 1 ConfigValue as S from AppConfig where Name = '{0}'", key, storeId);

			return dbAdapter.GetSqlS(sql);
		}

		public static int? StoreID()
		{
			if(HttpContext.Current == null
				|| HttpContext.Current.Items["StoreId"] == null)
				return null;

			return Convert.ToInt32(HttpContext.Current.Items["StoreId"]);
		}

		void VerifyParameters()
		{
			if(Request.QueryString.Count == 0)
			{
				ReturnErrorMessage("DotFeed Request Acknowledged. See DotFeed Panel for more information and validation.");
				Response.End();
			}

			if(Request.QueryString["AccessKey"] != ConfiguredAccessKey)
			{
				ReturnErrorMessage("Invalid access key provided");
				Response.End();
			}

			if(Request.RequestType.ToUpperInvariant() != "POST")
			{
				ReturnErrorMessage("This page only accepts HTTP POST");
				Response.End();
			}

			DateTime authDateValue;
			if(!DateTime.TryParse(Request.QueryString["AuthDate"], out authDateValue))
			{
				ReturnErrorMessage("AuthDate is not a valid DateTime");
				Response.End();
			}

			RawAuthDateParam = Request.Params["AuthDate"];
			AuthDate = authDateValue;

			try
			{
				ExtendedAuthToken = Convert.FromBase64String((Request.QueryString["ExtendedAuthToken"] ?? string.Empty).Replace(" ", "+"));
			}
			catch(Exception)
			{
				ReturnErrorMessage("ExtendedAuthToken is not a valid base64 encoded string");
				Response.End();
			}

			if(Request.ContentLength == 0)
			{
				ReturnErrorMessage("No XmlPackage provided");
				Response.End();
			}
		}

		void VerifyAuthentication(DateTime requestDate, string requestContents)
		{
			// Make sure authDate is within window
			if(requestDate - AuthDate > AuthWindow || requestDate - AuthDate < -AuthWindow)
			{
				ReturnErrorMessage("The request authorization window has expired");
				Response.End();
			}

			// Verify signature
			string source = ConfiguredAccessKey + RawAuthDateParam + requestContents;
			byte[] data = Encoding.Unicode.GetBytes(source);

			System.Security.Cryptography.RSACryptoServiceProvider rsa = new System.Security.Cryptography.RSACryptoServiceProvider();
			rsa.FromXmlString(DotFeedPublicKey);

			bool verified = rsa.VerifyData(data, "SHA1", ExtendedAuthToken);

			if(!verified)
			{
				ReturnErrorMessage("The provided ExtendedAuthToken is invalid");
				Response.End();
			}
		}

		string ProcessXmlPackage()
		{
			string xmlPackagePath = null;
			string result;

			try
			{
				xmlPackagePath = SaveXmlPackage();
				IXmlPackageAdapter xmlPackageAdapter = Adnsf.GetXmlPackageAdapter(xmlPackagePath);
				result = xmlPackageAdapter.TransformString();
			}
			catch(Exception exception)
			{
				result = null;
				ReturnErrorMessage("An exception occurred: " + exception.ToString());
				Response.End();

			}
			finally
			{
				if(xmlPackagePath != null)
					CleanUpXmlPackage(xmlPackagePath);
			}

			return result;
		}

		string SaveXmlPackage()
		{
			string xmlPackagePath;

			do
			{
				string filename = string.Format("DotFeed-{0}.xml.config", Guid.NewGuid());
				xmlPackagePath = Server.MapPath("~/images/" + filename);
			} while(File.Exists(xmlPackagePath));

			FileInfo file = new FileInfo(xmlPackagePath);

			using(FileStream fileStream = file.Create())
			{
				byte[] buffer = new byte[32768];
				int read;
				while((read = Request.InputStream.Read(buffer, 0, buffer.Length)) > 0)
					fileStream.Write(buffer, 0, read);

				fileStream.Close();
			}

			return xmlPackagePath;
		}

		void CleanUpXmlPackage(string xmlPackagePath)
		{
			File.Delete(xmlPackagePath);
		}

		void ReturnErrorMessage(string message)
		{
			Response.Write(@"<error><message>" + Server.HtmlEncode(message) + @"</message></error>");
		}
	</script>
</head>
<body />
</html>
