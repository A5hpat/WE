// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;
using System.Web.Routing;
using AspDotNetStorefront.License.Concrete;
using AspDotNetStorefront.Routing;
using AspDotNetStorefrontCore;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace AspDotNetStorefront.Controllers
{
	[RoutePrefix("siteinfo")]
	public class SiteInfoController : ApiController
	{
		const string KeyId = "B7752547-4544-4A27-9B6C-20055EC50EB9";

		const string SiteInfoAssembliesRoute = "SiteInfoAssembliesRoute";
		const string SiteInfoAuditRoute = "SiteInfoAuditRoute";
		const string SiteInfoFilesRoute = "SiteInfoFilesRoute";
		const string SiteInfoGatewayRoute = "SiteInfoGatewayRoute";
		const string SiteInfoLicensingRoute = "SiteInfoLicensingRoute";
		const string SiteInfoLocalizationRoute = "SiteInfoLocalizationRoute";
		const string SiteInfoRoutingRoute = "SiteInfoRoutingRoute";
		const string SiteInfoShippingRoute = "SiteInfoShippingRoute";
		const string SiteInfoStoresRoute = "SiteInfoStoresRoute";
		const string SiteInfoSystemRoute = "SiteInfoSystemRoute";

		readonly IRoutingConfigurationProvider RoutingConfigurationProvider;
		readonly IEnumerable<string> ExcludedFileInfoPaths;

		public SiteInfoController(IRoutingConfigurationProvider routingConfigurationProvider)
		{
			RoutingConfigurationProvider = routingConfigurationProvider;
			ExcludedFileInfoPaths = new[]
			{
				"images",
				"radspell",
				"_sgbak",
				"aspnet_client",
				"descriptions",
				"download",
				"radcontrols",
				"orderdownloads",
			};
		}

		[Route]
		public IHttpActionResult GetIndex()
		{
			var payload = new
			{
				nonce = Guid.NewGuid().ToString(),
				timestamp = DateTimeOffset.Now,
				resources = new
				{
					assemblies = Url.Link(SiteInfoAssembliesRoute, null),
					audit = Url.Link(SiteInfoAuditRoute, null),
					files = Url.Link(SiteInfoFilesRoute, null),
					gateway = Url.Link(SiteInfoGatewayRoute, null),
					licensing = Url.Link(SiteInfoLicensingRoute, null),
					localization = Url.Link(SiteInfoLocalizationRoute, null),
					routing = Url.Link(SiteInfoRoutingRoute, null),
					shipping = Url.Link(SiteInfoShippingRoute, null),
					store = Url.Link(SiteInfoStoresRoute, null),
					system = Url.Link(SiteInfoSystemRoute, null),
				},
			};

			return Jwe(payload, KeyId);
		}

		[Route("assemblies", Name = SiteInfoAssembliesRoute)]
		public IHttpActionResult GetAssemblies()
		{
			var payload = new
			{
				nonce = Guid.NewGuid().ToString(),
				timestamp = DateTimeOffset.Now,
				assemblies = AppDomain
					.CurrentDomain
					.GetAssemblies()
					.Select(assembly => assembly.GetName())
					.Select(assembly => new
					{
						fullName = assembly.FullName,
						name = assembly.Name,
						version = assembly.Version,
						architecture = assembly.ProcessorArchitecture.ToString(),
					}),
			};

			return Jwe(payload, KeyId);
		}

		[Route("audit", Name = SiteInfoAuditRoute)]
		public IHttpActionResult GetAudit()
		{
			var payload = new
			{
				nonce = Guid.NewGuid().ToString(),
				timestamp = DateTimeOffset.Now,
				audit = new
				{
					issues = Security
						.GetAuditIssues()
						.Select(issue => issue.Message),
				},
			};

			return Jwe(payload, KeyId);
		}

		[Route("files", Name = SiteInfoFilesRoute)]
		public IHttpActionResult GetFiles()
		{
			var payload = new
			{
				nonce = Guid.NewGuid().ToString(),
				timestamp = DateTimeOffset.Now,
				files = BuildDirectoryNode(new DirectoryInfo(CommonLogic.SafeMapPath("~/"))),
			};

			return Jwe(
				payload,
				KeyId,
				new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
		}

		[Route("gateway", Name = SiteInfoGatewayRoute)]
		public IHttpActionResult GetGateway()
		{
			var payload = new
			{
				nonce = Guid.NewGuid().ToString(),
				timestamp = DateTimeOffset.Now,
				gateway = new
				{
					name = AppLogic.ActivePaymentGatewayRAW(),
					liveTransactions = AppLogic.AppConfigBool("UseLiveTransactions"),
					transactionMode = AppLogic.AppConfig("TransactionMode"),
					paymentMethods = AppLogic
						.AppConfig("PaymentMethods")
						.ParseAsDelimitedList(),
					transactionCurrency = Localization.GetTransactionCurrency(),
					micropayEnabled = AppLogic.MicropayIsEnabled(),
					cardinalEnabled = AppLogic.AppConfigBool("CardinalCommerce.Centinel.Enabled"),
					saveCreditCards = AppLogic.StoreCCInDB(),
					useGatewayRecurringBilling = AppLogic.AppConfigBool("Recurring.UseGatewayInternalBilling"),
				},
			};

			return Jwe(payload, KeyId);
		}

		[Route("licensing", Name = SiteInfoLicensingRoute)]
		public IHttpActionResult GetLicensing()
		{
			var licenseValidationResult = LicenseController
				.Current
				.GetLicenseValidationResult();

			var payload = new
			{
				nonce = Guid.NewGuid().ToString(),
				timestamp = DateTimeOffset.Now,
				status = licenseValidationResult.LicenseStatus.ToString(),
				exception = licenseValidationResult.LicenseException,
				licenses = licenseValidationResult
					.Licenses
					.Select(license => new
					{
						identifier = license.Identifier,
						createdDate = license.CreatedDate,
						filename = license.Filename,
						policies = license
							.Policies
							.OfType<ADNSFPolicy>()
							.Select(policy => new
							{
								resourceIdentifier = policy.ResourceIdentifier,
								hosts = policy
									.RuleSet
									.Rules
									.OfType<HostRule>()
									.Select(hostRule => new
									{
										domains = hostRule.Domains,
									}),
								licenses = policy
									.RuleSet
									.Rules
									.OfType<LicenseRule>()
									.Select(licenseRule => new
									{
										requiredFileName = licenseRule.RequiredFileName,
									}),
								timePeriods = policy
									.RuleSet
									.Rules
									.OfType<TimePeriodRule>()
									.Select(timePeriodRule => new
									{
										beginDate = timePeriodRule.BeginDate,
										endDate = timePeriodRule.EndDate,
									}),
								environment = policy
									.RuleSet
									.Rules
									.OfType<EnvironmentRule>()
									.Select(environmentRule => new
									{
										beginDate = environmentRule.MachineName,
										osVersion = environmentRule.OSVersion,
										platformVersion = environmentRule.PlatformVersion,
										processorCount = environmentRule.ProcessorCount,
									}),
								limits = policy
									.RuleSet
									.Rules
									.OfType<ADNSFLimitsRule>()
									.Select(adnsfLimitsRule => new
									{
										categoryLimit = adnsfLimitsRule.CategoryLimit,
										customerLimit = adnsfLimitsRule.CustomerLimit,
										orderLimit = adnsfLimitsRule.OrderLimit,
										productLimit = adnsfLimitsRule.ProductLimit,
										sectionLimit = adnsfLimitsRule.SectionLimit,
									}),
							})
					}),
				permissions = new
				{
					domains = licenseValidationResult
						.PermissionSet
						.PermissionsOfType<DomainPermission>()
						.SelectMany(domainPermission => domainPermission.Domains),
					tokens = licenseValidationResult
						.PermissionSet
						.PermissionsOfType<TokenPermission>()
						.SelectMany(tokenPermission => tokenPermission.Tokens),
					identifiers = licenseValidationResult
						.PermissionSet
						.PermissionsOfType<IdentifierPermission>()
						.SelectMany(identifierPermission => identifierPermission.Identifiers),
				},
			};

			return Jwe(payload, KeyId);
		}

		[Route("localization", Name = SiteInfoLocalizationRoute)]
		public IHttpActionResult GetLocalization()
		{
			var payload = new
			{
				nonce = Guid.NewGuid().ToString(),
				timestamp = DateTimeOffset.Now,
				configuration = new
				{
					webConfigLocale = Localization.GetDefaultLocale(),
					sqlServerLocale = Localization.GetSqlServerLocale(),
					currencyCode = AppLogic.AppConfig("Localization.StoreCurrency"),
					numericCurrencyCode = AppLogic.AppConfig("Localization.StoreCurrencyNumericCode"),
				},
				locales = Localization
					.GetLocales()
					.Rows
					.Cast<DataRow>()
					.Select(row => new
					{
						id = row.Field<int>("LocaleSettingID"),
						name = row.Field<string>("Name"),
						description = row.Field<string>("Description"),
						defaultCurrencyId = row.Field<int>("DefaultCurrencyID"),
					}),

				currencies = Currency.GetCurrencies(),
			};

			return Jwe(payload, KeyId);
		}

		[Route("routing", Name = SiteInfoRoutingRoute)]
		public IHttpActionResult GetRouting()
		{
			var payload = new
			{
				nonce = Guid.NewGuid().ToString(),
				timestamp = DateTimeOffset.Now,
				routing = new
				{
					configuration = new
					{
						legacyRouteGenerationEnabled = RoutingConfigurationProvider
							.GetRoutingConfiguration()
							.LegacyRouteGenerationEnabled,
						legacyRouteRecognitionEnabled = RoutingConfigurationProvider
							.GetRoutingConfiguration()
							.LegacyRouteRecognitionEnabled,
						legacyRoutes301RedirectEnabled = RoutingConfigurationProvider
							.GetRoutingConfiguration()
							.LegacyRoutes301RedirectEnabled,
						seNameOnlyRoutesEnabled = RoutingConfigurationProvider
							.GetRoutingConfiguration()
							.SeNameOnlyRoutesEnabled,
					},
					routes = RouteTable
						.Routes
						.OfType<Route>()
						.Select(route => new
						{
							template = route.Url,
							defaults = route.Defaults == null || !route.Defaults.Any()
								? null
								: route
									.Defaults
									.ToDictionary(
										@default => @default.Key,
										@default => @default.Value == null
											? "{null}"
											: @default.Value == RouteParameter.Optional
												? "{optional}"
												: @default.Value.ToString()),
							constraints = route.Constraints == null || !route.Constraints.Any()
								? null
								: route
									.Constraints
									.ToDictionary(
										constraint => constraint.Key,
										constraint => constraint.Value == null
											? "{null}"
											: constraint.Value.ToString()),
							handler = route.RouteHandler.GetType().Name,
						}),
				},
			};

			return Jwe(payload, KeyId, serializerSettings: new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
		}

		[Route("shipping", Name = SiteInfoShippingRoute)]
		public IHttpActionResult GetShipping()
		{
			var payload = new
			{
				nonce = Guid.NewGuid().ToString(),
				timestamp = DateTimeOffset.Now,
				shipping = new
				{
					calculation = DB.GetSqlS("select Name as S from dbo.ShippingCalculation where Selected = 1"),
					origin = new
					{
						state = AppLogic.AppConfig("RTShipping.OriginState"),
						zip = AppLogic.AppConfig("RTShipping.OriginZip"),
						country = AppLogic.AppConfig("RTShipping.OriginCountry"),
					},
					freeShipping = new
					{
						threshold = AppLogic.AppConfigNativeDecimal("FreeShippingThreshold"),
						shippingMethod = AppLogic.AppConfig("ShippingMethodIDIfFreeShippingIsOn"),
						shippingRateSelectionAllowed = AppLogic.AppConfigBool("FreeShippingAllowsRateSelection"),
					},
				},
			};

			return Jwe(payload, KeyId);
		}

		[Route("stores", Name = SiteInfoStoresRoute)]
		public IHttpActionResult GetStore()
		{
			var payload = new
			{
				nonce = Guid.NewGuid().ToString(),
				timestamp = DateTimeOffset.Now,
				stores = new
				{
					configuration = new
					{
						allowProductFiltering = AppLogic.GlobalConfigBool("AllowProductFiltering"),
						allowEntityFiltering = AppLogic.GlobalConfigBool("AllowEntityFiltering"),
						allowCustomerFiltering = AppLogic.GlobalConfigBool("AllowCustomerFiltering"),
						allowNewsFiltering = AppLogic.GlobalConfigBool("AllowNewsFiltering"),
						allowTopicFiltering = AppLogic.GlobalConfigBool("AllowTopicFiltering"),
						allowOrderOptionFiltering = AppLogic.GlobalConfigBool("AllowOrderOptionFiltering"),
						allowPromoFiltering = AppLogic.GlobalConfigBool("AllowPromotionFiltering"),
						allowShoppingcartFiltering = AppLogic.GlobalConfigBool("AllowShoppingcartFiltering"),
						allowAffiliateFiltering = AppLogic.GlobalConfigBool("AllowAffiliateFiltering"),
						allowShippingFiltering = AppLogic.GlobalConfigBool("AllowShippingFiltering"),
						allowGiftCardFiltering = AppLogic.GlobalConfigBool("AllowGiftCardFiltering"),
					},
					stores = Store
						.GetStoreList()
						.Where(store => !store.Deleted)
						.Select(store => new
						{
							storeId = store.StoreID,
							name = store.Name,
							published = store.Published,
							skinId = store.SkinID,
							developmentUri = store.DevelopmentURI,
							stagingUri = store.StagingURI,
							productionUri = store.ProductionURI,
						})
				},
			};

			return Jwe(payload, KeyId);
		}

		[Route("system", Name = SiteInfoSystemRoute)]
		public IHttpActionResult GetSystem()
		{
			var payload = new
			{
				nonce = Guid.NewGuid().ToString(),
				timestamp = DateTimeOffset.Now,
				system = new
				{
					productName = CommonLogic.GetProductName(),
					dllVersion = Assembly.GetAssembly(typeof(Global)).GetName().Version,
					appConfigVersion = AppLogic.AppConfig("StoreVersion"),
					onLiveServer = AppLogic.OnLiveServer(),
					sslEnabled = AppLogic.UseSSL(),
					sslInUse = CommonLogic.IsSecureConnection(),
					adminDirectory = AppLogic.AppConfig("AdminDir"),
					menuCachingEnabled = AppLogic.AppConfigBool("CacheMenus"),
					trustLevel = AppLogic.TrustLevel.ToString(),
					executionMode = Environment.Is64BitProcess
						? "x64"
						: "x86",
					os = new
					{
						platform = Environment.OSVersion.Platform.ToString(),
						version = Environment.OSVersion.Version,
					},
					dotNetVersion = Environment.Version,
					installedFrameworkVersion = GetInstalledFrameworkVersion()
				},
			};

			return Jwe(payload, KeyId);
		}

		FileNode BuildDirectoryNode(DirectoryInfo directoryInfo)
		{
			return new FileNode
			{
				name = directoryInfo.Name,
				files = directoryInfo
					.GetFiles()
					.Select(file => new FileNode
					{
						name = file.Name,
						hash = GetFileContentHash(file),
					}),
				directories = directoryInfo
					.GetDirectories()
					.Where(directory => !ExcludedFileInfoPaths.Contains(directory.Name, StringComparer.OrdinalIgnoreCase))
					.Select(BuildDirectoryNode)
			};
		}

		string GetInstalledFrameworkVersion()
		{
			try
			{
				using(var registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full\\"))
				{
					var releaseKey = Convert.ToInt32(registryKey.GetValue("Release"));

					if(releaseKey >= 393295)
						return "4.6 or later";
					if((releaseKey >= 379893))
						return "4.5.2 or later";
					if((releaseKey >= 378675))
						return "4.5.1 or later";
					if((releaseKey >= 378389))
						return "4.5 or later";
				}
			}
			catch { }

			return "Unknown";
		}

		class FileNode
		{
			public string name;
			public string hash;
			public IEnumerable<FileNode> files;
			public IEnumerable<FileNode> directories;
		}

		string GetFileContentHash(FileInfo fileInfo)
		{
			try
			{
				using(var fileStream = fileInfo.OpenRead())
					return MD5
						.Create()
						.ComputeHash(fileStream)
						.ToString(string.Empty);
			}
			catch
			{
				return null;
			}
		}

		IHttpActionResult Jwe(object payload, string keyId, JsonSerializerSettings serializerSettings = null, Encoding encoding = null)
		{
			try
			{
				return Json(
					new
					{
						keyId = KeyId,
						payload = Jose.JWT.Encode(
							payload,
							LoadPublicKey(keyId),
							Jose.JweAlgorithm.RSA_OAEP,
							Jose.JweEncryption.A256GCM),
					},
					serializerSettings ?? new JsonSerializerSettings(),
					encoding ?? Encoding.UTF8);
			}
			catch(Exception exception)
			{
				return Json(
					new
					{
						stringed = exception.ToString(),
						exception = exception,
					});
			}
		}

		RSACryptoServiceProvider LoadPublicKey(string keyId)
		{
			var keyResourceAssembly = Assembly.Load("ASPDNSFApplication");
			var keyResourceName = string.Format("ASPDNSFApplication.Resources.SiteInfo.{0}.pubkey", keyId);

			byte[] key;
			using(var keyStream = keyResourceAssembly.GetManifestResourceStream(keyResourceName))
			using(var keyCopyStream = new MemoryStream((int)keyStream.Length))
			{
				keyStream.CopyTo(keyCopyStream);
				keyCopyStream.Flush();
				key = keyCopyStream.ToArray();
			}

			var rsa = new RSACryptoServiceProvider();
			rsa.ImportCspBlob(key);

			return rsa;
		}
	}
}
