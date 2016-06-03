// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.SessionState;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Xsl;
using AspDotNetStorefront.XmlPackageSchema;

namespace AspDotNetStorefrontCore
{
	public class XmlPackage
	{
		public const string XmlPackageExtension = ".xml.config";

		const string ReadonlyDefaultUserQueryName = "UserQuery";
		const string FolderPath = "~/images/";

		readonly bool IncludesEntityHelper;
		readonly bool HasAspdnsfNameSpace;
		readonly int SkinId;
		readonly string AdditionalRuntimeParms;
		readonly string UserSpecifiedQuery;
		readonly string TransformSource;
		readonly string[] ServerVariablesList;
		readonly string LocaleSetting;
		readonly package Package;
		readonly XmlDocument SystemData;
		readonly XslCompiledTransform Transform;
		readonly XsltArgumentList TransformArgumentList;
		readonly Hashtable RuntimeQueries;
		readonly Hashtable RuntimeParameters;
		readonly SkinProvider SkinProvider;
		readonly Customer CurrentCustomer;
		string SqlDebug;
		string FinalResult;

		public readonly string PackageUrl;
		public readonly string PackageName;
		public readonly string DisplayName;
		public readonly string ContentType;
		public readonly bool AllowEngine;
		public readonly bool IsDebug;
		public readonly XmlDocument XmlDataDocument;
		public bool RequiresParser;

		public string SETitle
		{ get; private set; }

		public string SEKeywords
		{ get; private set; }

		public string SEDescription
		{ get; private set; }

		public string SENoScript
		{ get; private set; }

		public string SectionTitle
		{ get; private set; }

		public XmlDocument PackageDocument
		{
			get
			{
				if(Package == null)
					return null;

				var document = new XmlDocument();
				var nav = document.CreateNavigator();
				using(var writer = nav.AppendChild())
					new XmlSerializer(typeof(package))
						.Serialize(writer, Package);

				return document;
			}
		}

		public XmlPackage(string packageName, Customer customer = null, int? skinId = null, string userQuery = null, string additionalRuntimeParms = null, string onlyRunNamedQuery = null, bool useExtensions = true, HtmlHelper htmlHelper = null)
		{
			XmlDataDocument = new XmlDocument();
			SystemData = new XmlDocument();
			TransformArgumentList = new XsltArgumentList();
			RuntimeQueries = new Hashtable();
			RuntimeParameters = new Hashtable();
			SkinProvider = new SkinProvider();

			FinalResult =
				SectionTitle =
				DisplayName =
				SETitle =
				SEKeywords =
				SEDescription =
				SENoScript =
				SqlDebug =
				AdditionalRuntimeParms =
				UserSpecifiedQuery =
				LocaleSetting =
				PackageUrl =
				TransformSource = string.Empty;

			ServerVariablesList = new[] {
					"HTTP_HOST",
					"HTTP_USER_AGENT",
					"AUTH_TYPE",
					"AUTH_USER",
					"AUTH_PASSWORD",
					"HTTPS",
					"LOCAL_ADDR",
					"PATH_INFO",
					"PATH_TRANSLATED",
					"SCRIPT_NAME",
					"SERVER_NAME",
					"SERVER_PORT_SECURE",
					"HTTP_CLUSTER_HTTPS"
				};

			ContentType = "text/html";

			PackageName = packageName;

			CurrentCustomer = customer;

			if(CurrentCustomer == null && HttpContext.Current.User != null)
				CurrentCustomer = HttpContext.Current.GetCustomer();

			if(CurrentCustomer == null)
			{
				CurrentCustomer = new Customer(true);
				LocaleSetting = Localization.GetDefaultLocale();
			}
			else
			{
				LocaleSetting = CurrentCustomer.LocaleSetting;
			}

			SkinId = skinId ?? (CurrentCustomer != null
				? CurrentCustomer.SkinID
				: 1);

			UserSpecifiedQuery = (userQuery ?? string.Empty).Trim();
			AdditionalRuntimeParms = (additionalRuntimeParms ?? string.Empty).Trim();
			
			if(useExtensions)
			{
				if(htmlHelper == null)
					htmlHelper = DependencyResolver.Current.GetService<HtmlHelper>();

				TransformArgumentList.AddExtensionObject(
					"urn:aspdnsf",
					new XSLTExtensions(CurrentCustomer, SkinId)
					{
						HtmlHelper = htmlHelper
					});

				var objExtConfig = XsltObjects.ExtensionConfigurationHandler.GetExtensionConfiguration("xsltobjects");
				if(objExtConfig != null)
					foreach(var ext in objExtConfig.Extensions.Values)
						TransformArgumentList.AddExtensionObject(
							namespaceUri: ext.Attributes["namespace"],
							extension: ExtensionObjects.CreateExtension(ext.Type, CurrentCustomer, SkinId, null));
			}

			if(!string.IsNullOrEmpty(UserSpecifiedQuery))
				AddRunTimeQuery(ReadonlyDefaultUserQueryName, UserSpecifiedQuery);

			if(AdditionalRuntimeParms.Length != 0)
				foreach(var runtimeParam in AdditionalRuntimeParms.Split('&'))
				{
					var runtimeParamParts = runtimeParam
						.Split(new[] { '=' })
						.Where(p => !string.IsNullOrEmpty(p))
						.Take(2)
						.ToArray();

					if(runtimeParamParts.Count() != 2)
						continue;

					AddRunTimeParam(
						parameterName: runtimeParamParts[0],
						parameterValue: HttpContext.Current.Server.UrlDecode(runtimeParamParts[1]));
				}

			//Load the package (from cache first) and create a Transform using the packageTransform
			//then get the data based on the queries in the package and any user queries
			var packageCacheName = string.Format("{0}_{1}_{2}_package", packageName, SkinId, LocaleSetting);
			Package = HttpContext.Current.Cache.Get(packageCacheName) as package;
			var pathHelper = new XmlPackagePathHelper();

			if(Package == null)
			{
				var packageNames = pathHelper.GenerateXmlPackageNameVariations(packageName, CurrentCustomer);

				if(Path.IsPathRooted(packageName))
				{
					// If we have a fully specified path, just try to find a localized or non-localized version of the path.
					PackageUrl = packageNames
						.Where(path => CommonLogic.FileExists(path))
						.FirstOrDefault()
						?? string.Empty;
				}
				else
				{
					// If the path isn't rooted, search through all the XmlPackage paths.
					PackageUrl = pathHelper
						.GenerateXmlPackageSearchPaths(SkinProvider.GetSkinNameById(CurrentCustomer.SkinID))
						.SelectMany(
							folder => packageNames,
							(folder, fileName) => Path.Combine(folder, fileName))
						.Where(path => File.Exists(path))
						.FirstOrDefault()
						?? string.Empty;
				}

				TransformSource = CommonLogic.SafeMapPath(PackageUrl);
				if(CommonLogic.FileExists(TransformSource))
				{
					try
					{
						using(var memoryStream = File.OpenRead(TransformSource))
							Package = ImportObjectFromStream<package>(new Type[0], memoryStream);

						HttpContext.Current.Cache.Insert(packageCacheName, Package, new CacheDependency(TransformSource));
					}
					catch(Exception ex)
					{
						throw new ArgumentException(string.Format("Error in XmlPackage(.Load), Package=[{0}]", packageName), ex);
					}
				}
				else
				{
					throw new FileNotFoundException(string.Format("The {0} xmlpackage was not found.", packageName), TransformSource);
				}
			}

			//Assign package property variables
			RequiresParser = Package.RequiresParserSpecified && Package.RequiresParser;

			if(Package.debugSpecified && Package.debug || AppLogic.AppConfigBool("XmlPackage.DumpTransform"))
			{
				Transform = new XslCompiledTransform(true);
				IsDebug = true;
			}

			if(Package.includeentityhelperSpecified && Package.includeentityhelper)
				IncludesEntityHelper = true;

			if(Package.allowengineSpecified && Package.allowengine)
				AllowEngine = true;

			if(!string.IsNullOrEmpty(Package.displayname))
				DisplayName = Package.displayname;

			if(!string.IsNullOrEmpty(Package.contenttype))
				ContentType = Package.contenttype;

			if(Package.runtime != null)
			{
				foreach(var runtime in Package.runtime)
				{
					switch(runtime.paramtype)
					{
						case QueryParamType.appconfig:
							AddRunTimeParam(
								runtime.paramname,
								AppLogic.AppConfig(runtime.requestparamname));
							break;

						case QueryParamType.request:
							AddRunTimeParam(
								runtime.paramname,
								CommonLogic.ParamsCanBeDangerousContent(runtime.requestparamname));
							break;
					}
				}
			}

			var transform = Package
				.PackageTransform
				.Any
				.FirstOrDefault();

			//Load transform
			if(transform == null)
				throw new Exception("The PackageTransform element must contain an xsl:stylesheet node");

			if(transform != null && transform.HasAttribute("xmlns:aspdnsf"))
				HasAspdnsfNameSpace = true;

			var resolver = new XmlPackageUrlResolver(pathHelper, SkinProvider.GetSkinNameById(SkinId))
			{
				Credentials = CredentialCache.DefaultCredentials
			};

			if(IsDebug)
			{
				var transformDumpFilename = string.Format("{0}_{1}.runtime.xsl", PackageName, AppLogic.IsAdminSite ? "admin" : "store");
				var transformDumpPath = CommonLogic.SafeMapPath(Path.Combine(FolderPath, transformDumpFilename));
				using(var writer = File.CreateText(transformDumpPath))
					writer.Write(XmlCommon.PrettyPrintXml(transform.OuterXml));

				try
				{
					Transform.Load(transformDumpPath, XsltSettings.TrustedXslt, resolver);
				}
				catch(SecurityException)
				{
					// If it failed it must be in Medium trust so turn off debugging in the tranform itself.
					Transform = new XslCompiledTransform(false);
					Transform.Load(transformDumpPath, XsltSettings.TrustedXslt, resolver);
				}
			}
			else
			{
				var transformCacheName = string.Format("{0}_{1}_{2}_transform", packageName, SkinId, LocaleSetting);
				Transform = HttpContext.Current.Cache.Get(transformCacheName) as XslCompiledTransform;
				if(Transform == null)
				{
					Transform = new XslCompiledTransform(false);
					Transform.Load(transform, XsltSettings.TrustedXslt, resolver);
					HttpContext.Current.Cache.Insert(
						transformCacheName,
						Transform,
						new CacheDependency(
							null,
							new[] { packageCacheName }));
				}
			}

			var currentCulture = Thread.CurrentThread.CurrentCulture;
			var currentUiCulture = Thread.CurrentThread.CurrentUICulture;
			Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(Localization.GetSqlServerLocale());
			Thread.CurrentThread.CurrentUICulture = new CultureInfo(Localization.GetSqlServerLocale());

			XmlDataDocument.AppendChild(XmlDataDocument.CreateElement("root"));

			InitializeSystemData();
			GetSqlData(onlyRunNamedQuery);
			Thread.CurrentThread.CurrentCulture = currentCulture;
			Thread.CurrentThread.CurrentUICulture = currentUiCulture;

			GetWebData();

			//Add EntityHelper data to the Xml DataDocument
			if(IncludesEntityHelper)
			{
				var entityHelperRootNode = XmlDataDocument
					.DocumentElement
					.AppendChild(XmlDataDocument.CreateNode(XmlNodeType.Element, "EntityHelpers", string.Empty));

				foreach(var entity in AppLogic.ro_SupportedEntities)
				{
					var storeID = AppLogic.IsAdminSite
						? 0
						: AppLogic.StoreID();

					var node = entityHelperRootNode.AppendChild(XmlDataDocument.CreateNode(XmlNodeType.Element, entity, string.Empty));
					foreach(XmlNode entityHelper in AppLogic.LookupHelper(entity, storeID).m_TblMgr.XmlDoc.SelectNodes("/root/Entity"))
						node.AppendChild(XmlDataDocument.ImportNode(entityHelper, true));
				}
			}

			//Add Search Engine Settings
			ProcessSESettings(Package.SearchEngineSettings);
			ProcessAfterActions(Package.PostProcessing);
		}

		void InitializeSystemData()
		{
			var primaryCurrency = Localization.GetPrimaryCurrency();
			var primaryCurrencyDisplayLocaleFormat = Currency.GetDisplayLocaleFormat(primaryCurrency);
			var webConfigLocale = Localization.GetDefaultLocale();

			if(XmlDataDocument == null || XmlDataDocument.DocumentElement == null)
				return;

			AddRuntimeParams(webConfigLocale, primaryCurrency, primaryCurrencyDisplayLocaleFormat, CurrentCustomer);

			var systemData = CreateSystemData(webConfigLocale, primaryCurrency, primaryCurrencyDisplayLocaleFormat, SkinId, PackageName, CurrentCustomer);
			var systemDataSerializer = new XmlSerializer(typeof(SystemData));
			var systemDataNavigator = SystemData.CreateNavigator();
			using(var writer = systemDataNavigator.AppendChild())
			{
				writer.WriteWhitespace(""); //Satisfy error: WriteStartDocument cannot be called on writers created with ConformanceLevel.Fragment
				systemDataSerializer.Serialize(writer, systemData);
			}

			//Import SystemData into DataDocument
			XmlDataDocument.DocumentElement.AppendChild(XmlDataDocument.ImportNode(SystemData.DocumentElement, true));
			XmlDataDocument.DocumentElement.AppendChild(CreateNameValueNode(XmlDataDocument, "QueryString", HttpContext.Current.Request.QueryString));
			XmlDataDocument.DocumentElement.AppendChild(CreateNameValueNode(XmlDataDocument, "Form", HttpContext.Current.Request.Form));
			XmlDataDocument.DocumentElement.AppendChild(CreateNameValueNode(XmlDataDocument, "Session", HttpContext.Current.Session));
			XmlDataDocument.DocumentElement.AppendChild(CreateNameValueNode(XmlDataDocument, "Cookies", HttpContext.Current.Request.Cookies));
			XmlDataDocument.DocumentElement.AppendChild(CreateNameValueNode(XmlDataDocument, "ServerVariables", HttpContext.Current.Request.ServerVariables, ServerVariablesList));
			XmlDataDocument.DocumentElement.AppendChild(CreateNameValueNode(XmlDataDocument, "Runtime", RuntimeParameters));
		}

		XmlNode CreateNameValueNode(XmlDocument document, string nodeName, Hashtable nvc)
		{
			var rootNode = document.CreateNode(XmlNodeType.Element, nodeName, string.Empty);
			var en = nvc.GetEnumerator();
			while(en.MoveNext())
			{
				try
				{
					var name = en.Key as string;
					var values = en.Value as string;
					if(!string.IsNullOrEmpty(name))
					{
						var node = document.CreateNode(XmlNodeType.Element, name, string.Empty);
						if(!string.IsNullOrEmpty(values))
							node.InnerText = values;

						rootNode.AppendChild(node);
					}
				}
				catch
				{ }
			}

			return rootNode;
		}

		XmlNode CreateNameValueNode(XmlDocument document, string nodeName, NameValueCollection nvc)
		{
			var rootNode = document.CreateNode(XmlNodeType.Element, nodeName, string.Empty);

			for(var i = 0; i < nvc.Count; i++)
			{
				try
				{
					var name = nvc.GetKey(i);
					if(string.IsNullOrEmpty(name))
						continue;

					// ASP.NET web controls often use $ in the name, which is invalid in XML element names.
					if(name.Contains("$"))
						continue;

					var values = nvc.Get(name);
					var node = document.CreateNode(XmlNodeType.Element, name.ToLowerInvariant(), string.Empty);
					node.InnerText = values;
					rootNode.AppendChild(node);
				}
				catch
				{ }
			}

			return rootNode;
		}

		XmlNode CreateNameValueNode(XmlDocument document, string nodeName, NameValueCollection nvc, string[] keys)
		{
			var rootNode = document.CreateNode(XmlNodeType.Element, nodeName, string.Empty);

			for(var i = 0; i < keys.Length; i++)
			{
				try
				{
					var name = keys[i];
					var values = nvc.Get(name);
					var node = document.CreateNode(XmlNodeType.Element, name, string.Empty);
					node.InnerText = values;
					rootNode.AppendChild(node);
				}
				catch
				{ }
			}

			return rootNode;
		}

		XmlNode CreateNameValueNode(XmlDocument document, string nodeName, HttpSessionState sessionState)
		{
			var rootNode = document.CreateNode(XmlNodeType.Element, nodeName, string.Empty);

			if(sessionState == null)
				return rootNode;

			for(var i = 0; i < sessionState.Count; i++)
			{
				try
				{
					var name = sessionState.Keys[i].ToLowerInvariant();
					var value = sessionState[i] as string;
					var node = document.CreateNode(XmlNodeType.Element, name, string.Empty);
					if(!string.IsNullOrEmpty(value))
						node.InnerText = value;

					rootNode.AppendChild(node);
				}
				catch
				{ }
			}

			return rootNode;
		}

		XmlNode CreateNameValueNode(XmlDocument document, string nodeName, HttpCookieCollection nvc)
		{
			var rootNode = document.CreateNode(XmlNodeType.Element, nodeName, string.Empty);
			var cookieBlackList = new[]
			{
				".ASPXANONYMOUS",
				"aspxauth",
				"asp.net_sessionid",
				"__RequestVerificationToken",
				".AspNet.ApplicationCookie"
			};

			for(var i = 0; i < nvc.Count; i++)
			{
				try
				{
					var name = nvc.GetKey(i);

					if(!string.IsNullOrEmpty(name)
						&& !cookieBlackList.Contains(name, StringComparer.InvariantCultureIgnoreCase))
					{
						var cookie = nvc.Get(name);
						var node = document.CreateNode(XmlNodeType.Element, name, string.Empty);

						if(cookie != null)
							node.InnerText = CommonLogic.CookieCanBeDangerousContent(name, true);

						rootNode.AppendChild(node);
					}
				}
				catch
				{ }
			}

			return rootNode;
		}

		void GetSqlData(string onlyRunNamedQuery)
		{
			if(Package == null
				|| Package.query == null
				|| Package.query.Length == 0)
				return;

			var queries = string.IsNullOrEmpty(onlyRunNamedQuery)
				? new List<query>(Package.query)
				: Package
					.query
					.Where(q => onlyRunNamedQuery.Equals(q.name, StringComparison.InvariantCultureIgnoreCase));

			foreach(var query in queries)
			{
				var node = ProcessQuery(query);

				if(node == null)
					continue;

				// Import result xml into DataDocument
				foreach(XmlNode childNode in node.ChildNodes)
					XmlDataDocument.DocumentElement.AppendChild(XmlDataDocument.ImportNode(childNode, true));
			}
		}

		XmlNode ProcessQuery(query sqlQuery)
		{
			//Evaluate runif attribute
			if(!string.IsNullOrEmpty(sqlQuery.runif))
			{
				var runIfParam = sqlQuery.runif;
				if(string.IsNullOrEmpty(CommonLogic.ParamsCanBeDangerousContent(runIfParam))
					&& string.IsNullOrEmpty(AppLogic.AppConfig(runIfParam))
					&& !RuntimeParameters.Contains(runIfParam))
					return null;
			}

			var queryName = sqlQuery.name;
			var rowElementName = string.IsNullOrEmpty(sqlQuery.rowElementName)
				? queryName + "row"
				: sqlQuery.rowElementName;
			var sql = sqlQuery.sql.Value;

			if(sqlQuery.querystringreplace != null)
				foreach(var queryStringReplaceNode in sqlQuery.querystringreplace)
					sql = ProcessReplaceParam(sql, queryStringReplaceNode);

			var connectionString = string.IsNullOrEmpty(sqlQuery.connectionStringName)
				? DB.GetDBConn()
				: ConfigurationManager.ConnectionStrings[sqlQuery.connectionStringName].ConnectionString;

			XmlNode sqlResult = null;
			using(var connection = new SqlConnection(connectionString))
			using(var cmd = new SqlCommand(sql, connection))
			{
				if(sqlQuery.queryparam != null)
					foreach(var pn in sqlQuery.queryparam)
						cmd.Parameters.Add(CreateParameter(pn));

				if(IsDebug)
				{
					var debugParamsDeclare = new StringBuilder();
					var debugParamsValues = new StringBuilder();
					SqlDebug += "/************************************  SQL Statement and parameters for query " + sqlQuery.name + "  ************************************/" + Environment.NewLine + Environment.NewLine;
					foreach(SqlParameter sqlParameter in cmd.Parameters)
					{
						debugParamsDeclare.AppendFormat("declare {0} {1}{2}", sqlParameter.ParameterName, sqlParameter.SqlDbType, Environment.NewLine);
						var shouldQuoteValues = new[]
							{
								SqlDbType.VarChar,
								SqlDbType.Char,
								SqlDbType.DateTime,
								SqlDbType.SmallDateTime,
								SqlDbType.NChar,
								SqlDbType.NVarChar,
								SqlDbType.Text,
								SqlDbType.NText,
								SqlDbType.UniqueIdentifier,
								SqlDbType.Bit
							}.Contains(sqlParameter.SqlDbType);

						if(shouldQuoteValues)
							debugParamsValues.AppendFormat("set {0} = '{1}'{2}", sqlParameter.ParameterName, sqlParameter.Value, Environment.NewLine);
						else
							debugParamsValues.AppendFormat("set {0} = {1}{2}", sqlParameter.ParameterName, sqlParameter.Value, Environment.NewLine);
					}

					SqlDebug += debugParamsDeclare + Environment.NewLine + debugParamsValues + Environment.NewLine + Environment.NewLine;
					SqlDebug += cmd.CommandText.Trim() + Environment.NewLine + Environment.NewLine;
				}


				// Allow override of default commandTimeout length in seconds
				// Can only 'increase' the default amount
				if(sqlQuery.commandTimeoutSpecified && sqlQuery.commandTimeout > cmd.CommandTimeout)
					cmd.CommandTimeout = sqlQuery.commandTimeout;

				connection.Open();
				if(!string.IsNullOrEmpty(sqlQuery.retType)
					&& "xml".Equals(sqlQuery.retType, StringComparison.InvariantCultureIgnoreCase))
				{
					var doc = new XmlDocument();
					using(var docWriter = doc.CreateNavigator().AppendChild())
					using(var sqlReader = cmd.ExecuteXmlReader())
					{
						docWriter.WriteStartDocument();
						docWriter.WriteStartElement("root");
						docWriter.WriteStartElement(queryName);

						sqlReader.MoveToContent();
						while(!sqlReader.EOF)
							docWriter.WriteNode(sqlReader, true);

						docWriter.WriteEndElement();
						docWriter.WriteEndElement();
						docWriter.WriteEndDocument();
					}

					sqlResult = doc.DocumentElement;
				}
				else
				{
					using(var sqlReader = cmd.ExecuteReader())
						sqlResult = GetXml(sqlReader, queryName, rowElementName, (sql.IndexOf("aspdnsf_PageQuery", StringComparison.InvariantCultureIgnoreCase) > -1));
				}
			}

			if(sqlResult != null
				&& sqlQuery.querytransform != null
				&& sqlQuery.querytransform.Any != null
				&& sqlQuery.querytransform.Any.Length > 0)
			{
				sqlResult.AppendChild(SystemData);
				return QueryTransform(sqlQuery.querytransform, sqlResult);
			}
			else if(sqlResult != null)
				return sqlResult;
			else
				return null;
		}

		XmlNode QueryTransform(querytransform queryTransform, XmlNode data)
		{
			if(queryTransform == null
				|| queryTransform.Any == null
				|| queryTransform.Any.Length == 0)
				throw new ArgumentNullException("queryTransform");

			if(data == null)
				throw new ArgumentNullException("data");

			var xsl = new XslCompiledTransform();
			xsl.Load(queryTransform.Any[0]);

			var document = new XmlDocument();
			using(var xmlWriter = document.CreateNavigator().AppendChild())
				xsl.Transform(data, xmlWriter);

			return document.DocumentElement;
		}

		string ProcessReplaceParam(string input, querystringreplace node)
		{
			if(string.IsNullOrEmpty(input))
				throw new ArgumentException("Missing input statement", "input");

			if(node == null)
				throw new ArgumentNullException("node");

			if(string.IsNullOrEmpty(node.replaceTag)
				|| string.IsNullOrEmpty(node.replaceparamname))
				throw new ArgumentNullException("node");

			var replaceParamName = node.replaceparamname;
			var replaceValue = string.Empty;

			switch(node.replacetype)
			{
				case QueryParamType.request:
					var value = CommonLogic.ParamsCanBeDangerousContent(replaceParamName);
					if(!string.IsNullOrEmpty(value))
						replaceValue = value;
					break;

				case QueryParamType.appconfig:
					replaceValue = AppLogic.AppConfig(replaceParamName);
					break;

				case QueryParamType.webconfig:
					replaceValue = CommonLogic.Application(replaceParamName);
					break;

				case QueryParamType.runtime:
					replaceValue = GetRuntimeParamValue(replaceParamName);
					break;

				case QueryParamType.system:
					if(SystemData != null)
					{
						var result = SystemData.SelectSingleNode("/System/" + replaceParamName);
						if(result != null)
							replaceValue = result.InnerText;
					}
					break;

			}

			if(string.IsNullOrEmpty(replaceValue))
				replaceValue = node.defvalue ?? string.Empty;

			if(!string.IsNullOrEmpty(node.validationpattern))
			{
				if(!Regex.IsMatch(replaceValue, node.validationpattern, RegexOptions.Compiled | RegexOptions.IgnoreCase))
					throw new Exception(string.Format("var Replace parameter {0} failed validation", node.replaceTag));

				return input.Replace(node.replaceTag, replaceValue);
			}
			else
				return input.Replace(node.replaceTag, replaceValue);
		}

		private SqlParameter CreateParameter(queryparam param)
		{
			var requestParameterName = param.requestparamname;
			var parameterValue = GetRawParameterValue(
				parameterType: param.paramtype,
				parameterName: requestParameterName);

			var defaultValue = param.defvalue;
			if(string.IsNullOrWhiteSpace(parameterValue))
				parameterValue = defaultValue;

			try
			{
				var validationPattern = param.validationpattern;
				if(!string.IsNullOrEmpty(validationPattern))
					if(!Regex.IsMatch(parameterValue, validationPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase))
						throw new Exception("Query parameter failed validation: paramvalue=" + parameterValue + "; validationpattern=" + validationPattern);

				var sqlParameterName = param.paramname;
				var sqlTypeMapping = GetSqlTypeMapping(param.sqlDataType);

				// If no mapping was found, we create the param but leave out the value and type
				if(sqlTypeMapping == null)
					return new SqlParameter
					{
						ParameterName = param.paramname
					};

				object sqlParameterValue;
				if(parameterValue.Equals("null", StringComparison.InvariantCultureIgnoreCase))
				{
					sqlParameterValue = DBNull.Value;
				}
				else if(sqlTypeMapping.Item2 == null)
				{
					// If no .net type is given, that means pass the value through as a string without any conversion
					sqlParameterValue = parameterValue;
				}
				else
				{
					// If there's a .net type, we fire up the converter and convert to the target type
					var typeConverter = System.ComponentModel.TypeDescriptor.GetConverter(sqlTypeMapping.Item2);
					sqlParameterValue = typeConverter.ConvertFrom(parameterValue);
				}

				return new SqlParameter(sqlParameterName, sqlTypeMapping.Item1)
				{
					Value = parameterValue.Equals("null", StringComparison.InvariantCultureIgnoreCase)
						? DBNull.Value
						: sqlParameterValue,
				};
			}
			catch(Exception exception)
			{
				throw new Exception("Invalid parameter specification (" + exception.Message + ")");
			}
		}

		string GetRawParameterValue(QueryParamType parameterType, string parameterName)
		{
			switch(parameterType)
			{
				case QueryParamType.request:
					if(HttpContext.Current.Request.RequestContext.RouteData.Values.ContainsKey(parameterName))
						return HttpContext.Current.Request.RequestContext.RouteData.Values[parameterName].ToString();

					if(HttpContext.Current.Request.RequestContext.RouteData.DataTokens.ContainsKey(parameterName))
						return HttpContext.Current.Request.RequestContext.RouteData.DataTokens[parameterName].ToString();

					if(!string.IsNullOrEmpty(CommonLogic.QueryStringCanBeDangerousContent(parameterName)))
						return CommonLogic.QueryStringCanBeDangerousContent(parameterName);

					if(!string.IsNullOrEmpty(CommonLogic.FormCanBeDangerousContent(parameterName)))
						return CommonLogic.FormCanBeDangerousContent(parameterName);

					if(!string.IsNullOrEmpty(CommonLogic.CookieCanBeDangerousContent(parameterName, true)))
						return CommonLogic.CookieCanBeDangerousContent(parameterName, true);

					if(!string.IsNullOrEmpty(CommonLogic.ServerVariables(parameterName)))
						return CommonLogic.ServerVariables(parameterName);

					return null;

				case QueryParamType.webconfig:
					return CommonLogic.Application(parameterName);

				case QueryParamType.appconfig:
					return AppLogic.AppConfig(parameterName);

				case QueryParamType.runtime:
					return GetRuntimeParamValue(parameterName);

				case QueryParamType.system:
					return SystemData.SelectSingleNode("/System/" + parameterName).InnerText;

				case QueryParamType.form:
					try
					{
						return XmlDataDocument.SelectSingleNode("/root/Form/" + parameterName).InnerText;
					}
					catch(Exception exception)
					{
						SysLog.LogMessage(
							String.Format("Error finding form key {0} for XmlPackage {1}.", parameterName, PackageName),
							exception.Message,
							MessageTypeEnum.XmlPackageException,
							MessageSeverityEnum.Message);

						return null;
					}

				case QueryParamType.xpath:
					var parameterNode = XmlDataDocument.SelectSingleNode(parameterName);
					return parameterNode.InnerText;

				default:
					return null;
			}
		}

		Tuple<SqlDbType, Type> GetSqlTypeMapping(SqlDataType sqlDataType)
		{
			// Map the SqlDbType string to the SqlDbType enum and corresponding .net type.
			// A null for the .net type means no conversion is needed, just pass the string through.
			var sqlTypeMappings = new Dictionary<SqlDataType, Tuple<SqlDbType, Type>>()
			{
				{ SqlDataType.bigint, Tuple.Create(SqlDbType.BigInt, typeof(long)) },
				{ SqlDataType.bit, Tuple.Create(SqlDbType.Bit, typeof(bool)) },
				{ SqlDataType.@char, Tuple.Create(SqlDbType.Char, (Type)null) },
				{ SqlDataType.datetime, Tuple.Create(SqlDbType.DateTime, typeof(DateTime)) },
				{ SqlDataType.@decimal, Tuple.Create(SqlDbType.Decimal, typeof(decimal)) },
				{ SqlDataType.@float, Tuple.Create(SqlDbType.Float, typeof(double)) },
				{ SqlDataType.@int, Tuple.Create(SqlDbType.Int, typeof(int)) },
				{ SqlDataType.money, Tuple.Create(SqlDbType.Money, typeof(decimal)) },
				{ SqlDataType.nchar, Tuple.Create(SqlDbType.NChar, (Type)null) },
				{ SqlDataType.ntext, Tuple.Create(SqlDbType.NText, (Type)null) },
				{ SqlDataType.nvarchar, Tuple.Create(SqlDbType.NVarChar, (Type)null) },
				{ SqlDataType.real, Tuple.Create(SqlDbType.Real, typeof(float)) },
				{ SqlDataType.smalldatetime, Tuple.Create(SqlDbType.SmallDateTime, typeof(DateTime)) },
				{ SqlDataType.smallint, Tuple.Create(SqlDbType.SmallInt, typeof(short)) },
				{ SqlDataType.smallmoney, Tuple.Create(SqlDbType.SmallMoney, typeof(decimal)) },
				{ SqlDataType.text, Tuple.Create(SqlDbType.Text, (Type)null) },
				{ SqlDataType.tinyint, Tuple.Create(SqlDbType.TinyInt, typeof(byte)) },
				{ SqlDataType.uniqueidentifier, Tuple.Create(SqlDbType.UniqueIdentifier, typeof(Guid)) },
				{ SqlDataType.varchar, Tuple.Create(SqlDbType.VarChar, (Type)null) },
			};

			if(!sqlTypeMappings.ContainsKey(sqlDataType))
				return null;

			return sqlTypeMappings[sqlDataType];
		}

		XmlNode GetWebData(webquery webQuery)
		{
			//Process RunIfAttribute
			if(!string.IsNullOrEmpty(webQuery.runif)
				&& string.IsNullOrEmpty(CommonLogic.ParamsCanBeDangerousContent(webQuery.runif))
				&& string.IsNullOrEmpty(AppLogic.AppConfig(webQuery.runif)))
				return null;

			var requestMethod = "get";
			if(!string.IsNullOrEmpty(webQuery.method))
				requestMethod = webQuery.method;

			var name = webQuery.name;
			var resultType = webQuery.RetType;
			var url = webQuery.url;

			if(string.IsNullOrEmpty(url))
				return null;

			var timeout = webQuery.timeoutSpecified
				? webQuery.timeout
				: 30;

			if(webQuery.querystringreplace != null)
				foreach(querystringreplace replacement in webQuery.querystringreplace)
					url = ProcessReplaceParam(url, replacement);

			var postData = string.Empty;
			if("post".Equals(requestMethod, StringComparison.InvariantCultureIgnoreCase)
				&& webQuery.postdata != null)
			{
				var parameterType = webQuery.postdata.paramtype;
				var parameterName = webQuery.postdata.paramname;
				switch(parameterType)
				{
					case QueryParamType.request:
						postData = CommonLogic.ParamsCanBeDangerousContent(parameterName);
						break;
					case QueryParamType.appconfig:
						postData = AppLogic.AppConfig(parameterName);
						break;
					case QueryParamType.webconfig:
						postData = CommonLogic.Application(parameterName);
						break;
					case QueryParamType.runtime:
						postData = GetRuntimeParamValue(parameterName);
						break;
					case QueryParamType.system:
						if(SystemData != null)
						{
							var result = SystemData.SelectSingleNode("/System/" + parameterName);
							if(result != null)
								postData = result.InnerText;
						}
						break;
				}
			}

			XmlNode xmlWebData;
			if("xml".Equals(resultType, StringComparison.InvariantCultureIgnoreCase))
			{
				xmlWebData = AspHttpXmlReader(name, url, timeout, postData);
			}
			else
			{
				xmlWebData = AspHttpStringReader(name, url, timeout, postData);
			}

			if("xml".Equals(resultType, StringComparison.InvariantCultureIgnoreCase)
				&& webQuery.querytransform != null
				&& webQuery.querytransform.Any != null
				&& webQuery.querytransform.Any.Any())
			{
				var xdoc = new XmlDocument();
				var rootNode = xdoc.CreateNode(XmlNodeType.Element, "root", string.Empty);
				rootNode.AppendChild(xdoc.ImportNode(xmlWebData, true));
				rootNode.AppendChild(xdoc.ImportNode(SystemData, true));
				return QueryTransform(webQuery.querytransform, xdoc);
			}
			else
				return xmlWebData;
		}

		void GetWebData()
		{
			// Check for web queries to process
			if(Package == null || Package.webquery == null || !Package.webquery.Any())
				return;

			foreach(var webQuery in Package.webquery)
			{
				XmlNode node;

				try
				{
					node = GetWebData(webQuery);
				}
				catch(Exception ex)
				{
					//Original Behavior, return CDATA with Exception::Message
					var errorNode = XmlDataDocument.CreateNode(XmlNodeType.Element, webQuery.name, string.Empty);
					errorNode.InnerXml = string.Format("<![CDATA[{0}]]>", ex.Message);
					XmlDataDocument.DocumentElement.AppendChild(errorNode);
					return;
				}

				//Import result xml into DataDocument
				using(var reader = new XmlNodeReader(node))
				using(var writer = XmlDataDocument.DocumentElement.CreateNavigator().AppendChild())
				{
					reader.MoveToContent();
					while(!reader.EOF)
						writer.WriteNode(reader, true);
				}
			}
		}

		void AddRunTimeQuery(string queryName, string userSpecifiedQuery)
		{
			if(RuntimeQueries.Contains(queryName))
				RuntimeQueries.Remove(queryName);

			RuntimeQueries.Add(queryName, userSpecifiedQuery);
		}

		void AddRunTimeParam(string parameterName, string parameterValue)
		{
			if(RuntimeParameters.Contains(parameterName))
				RuntimeParameters.Remove(parameterName);

			RuntimeParameters.Add(parameterName, parameterValue);
		}

		string GetRuntimeParamValue(string findParamName)
		{
			var enumerator = RuntimeParameters.GetEnumerator();
			while(enumerator.MoveNext())
			{
				var paramName = enumerator.Key.ToString();
				var paramValue = enumerator.Value.ToString();

				if(paramName.Equals(findParamName, StringComparison.InvariantCultureIgnoreCase))
					return paramValue;
			}

			return string.Empty;
		}

		string ProcessSearchEngineSettingNode(XmlNode node, string actionType)
		{
			switch(actionType)
			{
				case "xpath":
					var xPathResult = XmlDataDocument.SelectSingleNode(node.InnerText);
					if(xPathResult == null)
						return string.Empty;

					return xPathResult.InnerText;

				case "transform":
					using(var stringWriter = new StringWriter())
					{
						var transform = new XslCompiledTransform();
						transform.Load(node);
						transform.Transform(XmlDataDocument, TransformArgumentList, stringWriter);
						stringWriter.Flush();

						return stringWriter.ToString();
					}

				case "text":
					return node.InnerText.Replace("\r\n", "").Trim();

				default:
					return string.Empty;
			}
		}

		void ProcessSESettings(SearchEngineSettings searchEngineSettings)
		{
			if(searchEngineSettings == null)
				return;

			//Process SectionTitle Node
			if(searchEngineSettings.SectionTitle != null)
			{
				var actionType = searchEngineSettings.SectionTitle.actionType;
				if(string.IsNullOrEmpty(actionType))
					throw new Exception("actionType attribute not specified for SectionTitle element");

				var node = searchEngineSettings.SectionTitle.Any[0];
				SectionTitle = ProcessSearchEngineSettingNode(node, actionType.ToLowerInvariant());
			}

			//Process SETitle Node
			if(searchEngineSettings.SETitle != null)
			{
				var actionType = searchEngineSettings.SETitle.actionType;
				if(string.IsNullOrEmpty(actionType))
					throw new Exception("actionType attribute not specified for SETitle element");

				var node = searchEngineSettings.SETitle.Any[0];
				SETitle = ProcessSearchEngineSettingNode(node, actionType.ToLowerInvariant());
			}

			//Process SEKeywords Node
			if(searchEngineSettings.SEKeywords != null)
			{
				var actionType = searchEngineSettings.SEKeywords.actionType;
				if(string.IsNullOrEmpty(actionType))
					throw new Exception("actionType attribute not specified for SEKeywords element");

				var node = searchEngineSettings.SEKeywords.Any[0];
				SEKeywords = ProcessSearchEngineSettingNode(node, actionType.ToLowerInvariant());
			}

			//Process SEDescription Node
			if(searchEngineSettings.SEDescription != null)
			{
				var actionType = searchEngineSettings.SEDescription.actionType;
				if(string.IsNullOrEmpty(actionType))
					throw new Exception("actionType attribute not specified for SEDescription element");

				var node = searchEngineSettings.SEDescription.Any[0];
				SEDescription = ProcessSearchEngineSettingNode(node, actionType.ToLowerInvariant());
			}

			//Process SENoScript Node
			if(searchEngineSettings.SENoScript != null)
			{
				var actionType = searchEngineSettings.SENoScript.actionType;
				if(string.IsNullOrEmpty(actionType))
					throw new Exception("actionType attribute not specified for SENoScript element");

				var node = searchEngineSettings.SENoScript.Any[0];
				SENoScript = ProcessSearchEngineSettingNode(node, actionType.ToLowerInvariant());
			}
		}

		void ProcessAfterActions(PostProcessing postProcessing)
		{
			if(postProcessing == null)
				return;

			if(postProcessing.queryafter != null)
				foreach(var node in postProcessing.queryafter)
					ProcessSQLAfterActions(node);

			if(postProcessing.webqueryafter != null)
				foreach(var node in postProcessing.webqueryafter)
					ProcessWebQueryAfterActions(node);

			if(postProcessing.setcookie != null)
				foreach(var node in postProcessing.setcookie)
					ProcessCookieAfterActions(node);
		}

		void ProcessSQLAfterActions(queryafter node)
		{
			//Check RunIf attribute
			if(node.runif != null && node.runif.Any())
			{
				queryafterRunif runIf = node.runif[0];
				var runIfParameter = runIf.paramsource;
				switch(runIf.paramtype)
				{
					case queryafterRunifParamtype.request:
					case queryafterRunifParamtype.appconfig:
						if(string.IsNullOrEmpty(CommonLogic.ParamsCanBeDangerousContent(runIfParameter))
							&& string.IsNullOrEmpty(AppLogic.AppConfig(runIfParameter)))
							return;
						break;

					case queryafterRunifParamtype.xpath:
						if(XmlDataDocument.SelectSingleNode(runIfParameter) == null)
							return;
						break;

					default:
						return;
				}
			}

			var sql = node.sql as string;
			if(node.querystringreplace != null)
				foreach(querystringreplace qsr in node.querystringreplace)
					sql = ProcessReplaceParam(sql, qsr);


			using(var connection = new SqlConnection(DB.GetDBConn()))
			using(var command = new SqlCommand(sql, connection))
			{
				if(node.queryparam != null)
					foreach(var pn in node.queryparam)
						command.Parameters.Add(CreateParameter(pn));

				connection.Open();
				command.ExecuteNonQuery();
			}
		}

		void ProcessWebQueryAfterActions(webqueryafter node)
		{
			//Check RunIf attribute
			if(node.runif != null && node.runif.Any())
			{
				webqueryafterRunif runif = node.runif[0];
				var runifparam = runif.paramsource;
				switch(runif.paramtype)
				{
					case webqueryafterRunifParamtype.request:
					case webqueryafterRunifParamtype.appconfig:
						if(string.IsNullOrEmpty(CommonLogic.ParamsCanBeDangerousContent(runifparam))
							&& string.IsNullOrEmpty(AppLogic.AppConfig(runifparam)))
							return;
						break;

					case webqueryafterRunifParamtype.xpath:
						if(XmlDataDocument.SelectSingleNode(runifparam) == null)
							return;
						break;

					default:
						return;
				}
			}

			var url = node.url as string;
			if(string.IsNullOrEmpty(url))
				return;

			if(node.querystringreplace != null)
				foreach(var qsr in node.querystringreplace)
					url = ProcessReplaceParam(url, qsr);

			AspHttp(url, 30, string.Empty);
		}

		void ProcessCookieAfterActions(setcookie node)
		{
			string cookieValue = string.Empty;
			switch(node.valuetype)
			{
				case setcookieValuetype.request:
					cookieValue = CommonLogic.ParamsCanBeDangerousContent(node.cookiesource);
					break;

				case setcookieValuetype.appconfig:
					cookieValue = AppLogic.AppConfig(node.cookiesource);
					break;

				case setcookieValuetype.webconfig:
					cookieValue = CommonLogic.Application(node.cookiesource);
					break;

				case setcookieValuetype.xpath:
					var xpathResult = XmlDataDocument.SelectSingleNode(node.cookiesource);
					if(xpathResult != null)
						cookieValue = xpathResult.InnerText;
					break;
			}

			var cookieName = node.cookiename;
			var cookie = HttpContext.Current.Request.Cookies.Get(cookieName)
				?? new HttpCookie(cookieName);

			cookie.Value = cookieValue;
			if(node.expiresSpecified && node.expires > 0)
				cookie.Expires = DateTime.Now.AddDays(node.expires);

			HttpContext.Current.Response.Cookies.Set(cookie);
		}

		T ImportObjectFromStream<T>(Type[] extraTypes, Stream stream)
		{
			var serializer = new XmlSerializer(typeof(T), extraTypes);
			return (T)serializer.Deserialize(stream);
		}

		string AspHttp(string url, int timeoutInSeconds, string postData)
		{
			try
			{
				using(var response = _AspHTTP(url, timeoutInSeconds, postData))
				{
					var stream = response.GetResponseStream();
					if(stream == null)
						return string.Empty;

					using(var responseStream = new StreamReader(stream))
						return responseStream.ReadToEnd();
				}
			}
			catch(Exception exception)
			{
				return exception.Message;
			}
		}

		XmlNode AspHttpStringReader(string rootNodeName, string url, int timeoutInSeconds, string postData)
		{
			var webData = AspHttp(url, timeoutInSeconds, postData);

			var document = new XmlDocument();
			document.LoadXml(string.Format("<{0}><![CDATA[{1}]]></{0}>", rootNodeName, webData));

			return document.DocumentElement;
		}

		XmlNode AspHttpXmlReader(string rootNodeName, string url, int timeoutInSeconds, string postData)
		{
			var document = new XmlDocument();
			using(var writer = document.CreateNavigator().AppendChild())
			{
				writer.WriteStartDocument();
				writer.WriteStartElement(rootNodeName);

				using(var response = _AspHTTP(url, timeoutInSeconds, postData))
				{
					var stream = response.GetResponseStream();
					if(stream != null)
					{
						using(var reader = XmlReader.Create(stream))
						{
							reader.MoveToContent();
							while(!reader.EOF)
								writer.WriteNode(reader, true);
						}
					}
				}

				writer.WriteEndElement();
				writer.WriteEndDocument();
			}

			return document.DocumentElement;
		}

		WebResponse _AspHTTP(string url, int timeoutInSeconds, string postData)
		{
			var request = WebRequest.Create(url);
			request.Timeout = timeoutInSeconds > 0
				? timeoutInSeconds * 1000
				: Timeout.Infinite;

			if(!string.IsNullOrEmpty(postData))
			{
				request.ContentType = "application/x-www-form-urlencoded";
				request.Method = "POST";

				var byteArray = Encoding.UTF8.GetBytes(postData);
				request.ContentLength = byteArray.Length;

				using(var requestStream = request.GetRequestStream())
					requestStream.Write(byteArray, 0, byteArray.Length);
			}

			return request.GetResponse();
		}

		XmlNode GetXml(IDataReader reader, string rootElement, string rowElement, bool isPagingProc)
		{
			var document = new XmlDocument();
			using(var writer = document.CreateNavigator().AppendChild())
			{
				writer.WriteStartDocument();
				writer.WriteStartElement("root");

				if(!string.IsNullOrEmpty(rootElement))
					writer.WriteStartElement(rootElement);

				while(reader.Read())
				{
					if(!string.IsNullOrEmpty(rowElement))
						writer.WriteStartElement(rowElement);
					else
						writer.WriteStartElement("row");

					for(var i = 0; i < reader.FieldCount; i++)
					{
						var elementName = reader.GetName(i).Replace(" ", "_");
						if(reader.IsDBNull(i))
						{
							writer.WriteElementString(elementName, null);
						}
						else if(reader.GetFieldType(i) == typeof(DateTime))
						{
							var value = Localization.ParseLocaleDateTime(
								reader.GetDateTime(i).ToString(),
								Thread.CurrentThread.CurrentUICulture.Name).ToString();
							writer.WriteElementString(elementName, value);
						}
						else
						{
							var value = Convert.ToString(reader.GetValue(i));
							if(value.StartsWith("<ml>", StringComparison.InvariantCultureIgnoreCase))
							{
								writer.WriteStartElement(elementName);

								using(var xmlTextReader = new XmlTextReader(new StringReader(value)))
									while(xmlTextReader.Read())
										writer.WriteNode(xmlTextReader, true);

								writer.WriteEndElement();
							}
							else
							{
								writer.WriteElementString(elementName, value);
							}
						}
					}

					//End Row
					writer.WriteEndElement();
				}

				if(!string.IsNullOrEmpty(rootElement))
					writer.WriteEndElement();

				var resultSet = 1;
				while(reader.NextResult())
				{
					resultSet++;
					if(isPagingProc)
					{
						if(!string.IsNullOrEmpty(rootElement))
							writer.WriteStartElement(rootElement + "Paging");
						else
							writer.WriteStartElement("Paging");
					}
					else if(!string.IsNullOrEmpty(rootElement))
						writer.WriteStartElement(string.Format("{0}{1}", rootElement, resultSet));

					while(reader.Read())
					{
						if(!isPagingProc)
							if(!string.IsNullOrEmpty(rowElement))
								writer.WriteStartElement(rowElement);
							else
								writer.WriteStartElement("row");

						for(var i = 0; i < reader.FieldCount; i++)
						{
							var elementName = reader.GetName(i).Replace(" ", "_");
							if(reader.IsDBNull(i))
							{
								writer.WriteElementString(elementName, null);
							}
							else if(reader.GetFieldType(i) == typeof(DateTime))
							{
								var value = Localization.ParseLocaleDateTime(reader.GetDateTime(i).ToString(), Thread.CurrentThread.CurrentUICulture.Name).ToString();
								writer.WriteElementString(elementName, value);
							}
							else
							{
								var value = Convert.ToString(reader.GetValue(i));
								if(value.StartsWith("<ml>", StringComparison.InvariantCultureIgnoreCase))
								{
									writer.WriteStartElement(elementName);

									using(var r = new XmlTextReader(new StringReader(value)))
										while(r.Read())
											writer.WriteNode(r, true);

									writer.WriteEndElement();
								}
								else
								{
									writer.WriteElementString(elementName, value);
								}
							}
						}

						if(!isPagingProc)
							writer.WriteEndElement();
					}

					if(isPagingProc || !string.IsNullOrEmpty(rootElement))
						writer.WriteEndElement();
				}

				writer.WriteEndElement();
				writer.WriteEndDocument();
			}

			return document.DocumentElement;
		}

		void AddRuntimeParams(string webConfigLocale, string primaryCurrency, string primaryCurrencyDisplayLocaleFormat, Customer customer)
		{
			var appliedCultureInfo = customer == null
				? Thread.CurrentThread.CurrentCulture
				: CultureInfo.InvariantCulture;

			AddRunTimeParam("DefaultVATSetting", AppLogic.AppConfigUSInt("VAT.DefaultSetting").ToString(appliedCultureInfo));
			AddRunTimeParam("CustomerVATSetting", ((int)CurrentCustomer.VATSettingRAW).ToString(appliedCultureInfo));
			AddRunTimeParam("UseVATSetting", ((int)CurrentCustomer.VATSettingReconciled).ToString(appliedCultureInfo));
			AddRunTimeParam("CustomerLevelID", CurrentCustomer.CustomerLevelID.ToString(appliedCultureInfo));
			AddRunTimeParam("CustomerLevelName", CurrentCustomer.CustomerLevelName);
			AddRunTimeParam("CustomerFirstName", CurrentCustomer.FirstName);
			AddRunTimeParam("CustomerLastName", CurrentCustomer.LastName);
			AddRunTimeParam("CustomerFullName", string.Format("{0} {1}", CurrentCustomer.FirstName, CurrentCustomer.LastName).Trim());

			AddRunTimeParam("CustomerRoles", CurrentCustomer.Roles);
			AddRunTimeParam("IsAdminUser", CurrentCustomer.IsAdminUser.ToString());
			AddRunTimeParam("IsSuperUser", CurrentCustomer.IsAdminSuperUser.ToString());
			AddRunTimeParam("VAT.Enabled", AppLogic.AppConfigBool("VAT.Enabled").ToString());
			AddRunTimeParam("VAT.AllowCustomerToChooseSetting", AppLogic.AppConfigBool("VAT.AllowCustomerToChooseSetting").ToString());
			AddRunTimeParam("LocaleSetting", CurrentCustomer.LocaleSetting);
			AddRunTimeParam("CurrencySetting", CurrentCustomer.CurrencySetting);
			AddRunTimeParam("CurrencyDisplayLocaleFormat", Currency.GetDisplayLocaleFormat(CurrentCustomer.CurrencySetting));
			AddRunTimeParam("IPAddress", CurrentCustomer.LastIPAddress);
			AddRunTimeParam("AffiliateID", CurrentCustomer.AffiliateID.ToString(appliedCultureInfo));

			//Runtime Parmas
			AddRunTimeParam("IsAdminSite", AppLogic.IsAdminSite.ToString());
			AddRunTimeParam("IsAdminSiteInt", CommonLogic.IIF(AppLogic.IsAdminSite, "1", "0"));
			AddRunTimeParam("WebConfigLocaleSetting", webConfigLocale);
			AddRunTimeParam("SqlServerLocaleSetting", Localization.GetSqlServerLocale());
			AddRunTimeParam("PrimaryCurrency", primaryCurrency);
			AddRunTimeParam("PrimaryCurrencyDisplayLocaleFormat", primaryCurrencyDisplayLocaleFormat);
			AddRunTimeParam("Date", Localization.ToThreadCultureShortDateString(DateTime.Now));
			AddRunTimeParam("Time", DateTime.Now.ToShortTimeString());
			AddRunTimeParam("SkinId", SkinId.ToString());
			AddRunTimeParam("QueryStringRAW", XmlCommon.XmlEncode(HttpContext.Current.Request.QueryString.ToString()));
			AddRunTimeParam("XmlPackageName", PackageName);
			AddRunTimeParam("PageName", CommonLogic.GetThisPageName(false));
			AddRunTimeParam("FullPageName", CommonLogic.GetThisPageName(true));
			var storeUrl = AppLogic
				.GetStoreHTTPLocation(true)
				.ToLowerInvariant()
				.Replace(AppLogic.AdminDir().ToLowerInvariant() + "/", "");
			AddRunTimeParam("StoreUrl", storeUrl);
			var customerIsRegistered = CurrentCustomer == null
				? bool.FalseString.ToLowerInvariant()
				: CurrentCustomer.IsRegistered.ToString().ToLowerInvariant();
			AddRunTimeParam("CustomerIsRegistered", customerIsRegistered);
			AddRunTimeParam("StoreID", AppLogic.StoreID().ToString());
			AddRunTimeParam("FilterProduct", AppLogic.GlobalConfigBool("AllowProductFiltering").ToString());
			AddRunTimeParam("FilterEntity", AppLogic.GlobalConfigBool("AllowEntityFiltering").ToString());
			AddRunTimeParam("FilterTopic", AppLogic.GlobalConfigBool("AllowTopicFiltering").ToString());
			AddRunTimeParam("FilterNews", AppLogic.GlobalConfigBool("AllowNewsFiltering").ToString());
			AddRunTimeParam("PageType", AppLogic.GetCurrentPageType());
			AddRunTimeParam("PageID", AppLogic.GetCurrentPageID());
		}

		SystemData CreateSystemData(string webConfigLocale, string primaryCurrency, string primaryCurrencyDisplayLocaleFormat, Int32 skinId, string xmlPackageName)
		{
			var defaultVatSetting = AppLogic.AppConfigUSInt("VAT.DefaultSetting");
			var now = DateTime.Now;

			var systemData = new SystemData
			{
				DefaultVATSetting = defaultVatSetting,
				CustomerVATSetting = defaultVatSetting,
				UseVATSetting = defaultVatSetting,
				CustomerLevelID = 0,
				IsAdminUser = false,
				IsSuperUser = false,

				VAT_Enabled = AppLogic.AppConfigBool("VAT.Enabled"),
				VAT_AllowCustomerToChooseSetting = AppLogic.AppConfigBool("VAT.AllowCustomerToChooseSetting"),

				LocaleSetting = webConfigLocale,
				CurrencySetting = primaryCurrency,
				IPAddress = string.Empty,
				AffiliateID = 0,

				IsAdminSite = AppLogic.IsAdminSite,
				IsAdminSiteInt = AppLogic.IsAdminSite ? 1 : 0,
				WebConfigLocaleSetting = webConfigLocale,
				SqlServerLocaleSetting = Localization.GetSqlServerLocale(),
				PrimaryCurrency = primaryCurrency,
				PrimaryCurrencyDisplayLocaleFormat = primaryCurrencyDisplayLocaleFormat,

				Date = now,
				Time = now,
				SkinID = skinId,

				QueryStringRAW = HttpContext.Current.Request.QueryString.ToString(),
				XmlPackageName = xmlPackageName,

				PageName = CommonLogic.GetThisPageName(false),
				FullPageName = CommonLogic.GetThisPageName(true),

				StoreUrl = AppLogic.GetStoreHTTPLocation(true).ToLowerInvariant().Replace(AppLogic.AdminDir().ToLowerInvariant() + "/", ""),

				CurrentDateTime = now,

				CustomerIsRegistered = false,
				StoreID = AppLogic.StoreID(),
				FilterProduct = AppLogic.GlobalConfigBool("AllowProductFiltering"),
				FilterEntity = AppLogic.GlobalConfigBool("AllowEntityFiltering"),
				FilterTopic = AppLogic.GlobalConfigBool("AllowTopicFiltering"),
				FilterNews = AppLogic.GlobalConfigBool("AllowNewsFiltering"),
			};

			if(HttpContext.Current.Items.Contains("RequestedPage"))
				systemData.RequestedPage = HttpContext.Current.Items["RequestedPage"] as string;

			if(HttpContext.Current.Items.Contains("RequestedQuerystring"))
				systemData.RequestedQuerystring = HttpContext.Current.Items["RequestedQuerystring"] as string;

			if(HttpContext.Current.Items.Contains("AdnsfVersion") && HttpContext.Current.Items["AdnsfVersion"] != null)
			{
				var adnsfVersion = (Version)HttpContext.Current.Items["AdnsfVersion"];
				systemData.AdnsfVersion = adnsfVersion.ToString();
				systemData.AdnsfVersionMajor = adnsfVersion.Major;
				systemData.AdnsfVersionMinor = adnsfVersion.Minor;
				systemData.AdnsfVersionRevision = adnsfVersion.Revision;
				systemData.AdnsfVersionBuild = adnsfVersion.Build;
			}

			return systemData;
		}

		SystemData CreateSystemData(string webConfigLocale, string primaryCurrency, string primaryCurrencyDisplayLocaleFormat, Int32 skinId, string xmlPackageName, Customer customer)
		{
			var systemData = CreateSystemData(webConfigLocale, primaryCurrency, primaryCurrencyDisplayLocaleFormat, skinId, xmlPackageName);

			if(customer == null)
				return systemData;

			systemData.CustomerID = customer.CustomerID;
			systemData.CustomerVATSetting = (int)customer.VATSettingRAW;
			systemData.UseVATSetting = (int)customer.VATSettingReconciled;
			systemData.CustomerLevelID = customer.CustomerLevelID;
			systemData.CustomerLevelName = customer.CustomerLevelName;
			systemData.CustomerFirstName = customer.FirstName;
			systemData.CustomerLastName = customer.LastName;
			systemData.CustomerFullName = string.Format("{0} {1}", customer.FirstName, customer.LastName).Trim();

			systemData.CustomerRoles = CurrentCustomer.Roles;
			systemData.IsAdminUser = customer.IsAdminUser;
			systemData.IsSuperUser = customer.IsAdminSuperUser;

			systemData.VAT_Enabled = AppLogic.AppConfigBool("VAT.Enabled");
			systemData.VAT_AllowCustomerToChooseSetting = AppLogic.AppConfigBool("VAT.AllowCustomerToChooseSetting");
			systemData.LocaleSetting = customer.LocaleSetting;
			systemData.CurrencySetting = customer.CurrencySetting;

			systemData.CurrencyDisplayLocaleFormat = Currency.GetDisplayLocaleFormat(customer.CurrencySetting);

			IPAddress address;
			if(IPAddress.TryParse(customer.LastIPAddress, out address))
				systemData.IPAddress = address.ToString();

			systemData.AffiliateID = customer.AffiliateID;
			systemData.CustomerIsRegistered = customer.IsRegistered;

			return systemData;
		}

		public string TransformString()
		{
			if(AppLogic.AppConfigBool("XmlPackage.DumpTransform") || IsDebug)
			{
				try // don't let logging crash the site
				{
					var siteSpecifier = AppLogic.IsAdminSite
						? "admin"
						: "store";

					var xslFile = string.Format("{0}_{1}.runtime.xml", PackageName, siteSpecifier);
					var xslFilePath = CommonLogic.SafeMapPath(Path.Combine(FolderPath, xslFile));
					File.WriteAllText(xslFilePath, XmlCommon.PrettyPrintXml(XmlDataDocument.InnerXml));

					var sqlFile = string.Format("{0}_{1}.runtime.sql", PackageName, siteSpecifier);
					var sqlFilePath = CommonLogic.SafeMapPath(Path.Combine(FolderPath, sqlFile));
					File.WriteAllText(sqlFilePath, SqlDebug);
				}
				catch
				{ }
			}

			using(var memoryStream = new MemoryStream())
			{
				try
				{
					if(HasAspdnsfNameSpace)
					{
						Transform.Transform(XmlDataDocument, TransformArgumentList, memoryStream);
						RequiresParser = true;
					}
					else
					{
						Transform.Transform(XmlDataDocument, null, memoryStream);
					}
				}
				catch(Exception exception)
				{
					throw new Exception(
						string.Format("Error has occurred on XML package: {0}", PackageName),
						exception);
				}

				memoryStream.Position = 0;
				using(var reader = new StreamReader(memoryStream, Transform.OutputSettings.Encoding))
					FinalResult = reader.ReadToEnd();
			}

			if(AppLogic.AppConfigBool("XmlPackage.DumpTransform") || IsDebug)
			{
				// don't let logging crash the site!
				try
				{
					var siteSpecifier = AppLogic.IsAdminSite
						? "admin"
						: "store";

					var xslFile = string.Format("{0}_{1}.xfrm.xml", PackageName, siteSpecifier);
					var xslPath = CommonLogic.SafeMapPath(Path.Combine(FolderPath, xslFile));
					File.WriteAllText(xslPath, XmlCommon.PrettyPrintXml(FinalResult));
				}
				catch
				{ }
			}

			return FinalResult;
		}
	}
}
