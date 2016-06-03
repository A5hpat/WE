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
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Xml;
using AspDotNetStorefrontCore.ShippingCalculation;

namespace AspDotNetStorefrontCore
{
	public partial class RTShipping
	{
		private string m_upsLogin;          // username, password, license
		private string m_upsServer;         // UPS server
		private string m_upsUsername;
		private string m_upsPassword;
		private string m_upsLicense;

		private string m_uspsLogin;         // username, password
		private string m_uspsServer;            // USPS server
		private string m_uspsUsername;
		private string m_uspsPassword;

		private string m_FedexAccountNumber;
		private string m_FedexServer;
		private string m_FedexMeter;            // Returned from Fedex after subscription
		private string m_FedexKey;
		private string m_FedexPassword;

		private string m_OriginAddress;
		private string m_OriginAddress2;
		private string m_OriginCity;
		private string m_OriginStateProvince;
		private string m_OriginZipPostalCode;
		private string m_OriginCountry;
		private string m_DestinationAddress;
		private string m_DestinationAddress2;
		private string m_DestinationCity;
		private string m_DestinationStateProvince;
		private string m_DestinationZipPostalCode;
		private string m_DestinationCountry;
		private ResidenceTypes m_DestinationResidenceType;

		private Decimal m_ShipmentWeight;
		private decimal m_ShipmentValue;
		private Decimal m_Length;   // Length of the package in inches
		private Decimal m_Width;    // Width of the package in inches
		private Decimal m_Height;   // Height of the package in inches

		private bool m_TestMode;

		private ArrayList ratesValues;
		private ArrayList ratesText;

		public const string CanadaPostName = "Canada Post";
		public const string UpsName = "UPS";
		public const string FedExName = "FEDEX";
		public const string UspsName = "USPS";
		public const string AustraliaPostName = "Australia Post";

		public const string UpsIdentifier = "UPS";
		public const string Ups2Identifier = "UPS2";
		public const string UspsIdentifier = "USPS";
		public const string FedExIdentifier = "FEDEX";
		public const string DhlIdentifier = "DHL";
		public const string CanadaPostIdentifier = "CANADAPOST";
		public const string AustraliaPostIdentifier = "AUSPOST";

		//will use for determining the cost of free shipping items to include or omit from a
		//returned rate if AppConfig.FreeShippingAllowsRateSelection is true
		Boolean HasFreeItems;
		int PackageQuantity;

		static private bool IsDomesticCountryCode(String CountryCode)
		{
			CountryCode = CountryCode.Trim().ToUpperInvariant();
			String[] DomesticCountries = { "US", "PR", "VI", "AS", "GU", "MP", "PW", "MH" };
			foreach(String s in DomesticCountries)
			{
				if(s == CountryCode)
				{
					return true;
				}
			}
			return false;
		}

		static private String MapPickupType(String s)
		{
			s = s.Trim().ToLowerInvariant();
			if(s == "upsdailypickup")
			{
				return "01";
			}
			if(s == "upscustomercounter")
			{
				return "03";
			}
			if(s == "upsonetimepickup")
			{
				return "06";
			}
			if(s == "upsoncallair")
			{
				return "07";
			}
			if(s == "upssuggestedretailrates")
			{
				return "11";
			}
			if(s == "upslettercenter")
			{
				return "19";
			}
			if(s == "upsairservicecenter")
			{
				return "20";
			}
			return "03"; // find some default
		}

		public string UPSLogin  // UPS Login infomration, "Username,Password,License" Please note: The login information is case sensitive
		{
			get { return m_upsLogin; }
			set
			{
				m_upsLogin = value;
				string[] arrUpsLogin = m_upsLogin.Split(',');
				try
				{
					m_upsUsername = arrUpsLogin[0].Trim();
					m_upsPassword = arrUpsLogin[1].Trim();
					m_upsLicense = arrUpsLogin[2].Trim();
				}
				catch { }
			}
		}

		/// FedEx login information, "Username,Password"
		/// 
		public string FedexKey
		{
			get { return this.m_FedexKey; }
			set { this.m_FedexKey = value; }
		}

		public string FedexPassword
		{
			get { return this.m_FedexPassword; }
			set { this.m_FedexPassword = value; }
		}
		public string FedexAccountNumber
		{
			get { return this.m_FedexAccountNumber; }
			set { this.m_FedexAccountNumber = value; }
		}

		/// FedEx Meter Number provided by FedEx after subscription
		public string FedexMeter
		{
			get { return this.m_FedexMeter; }
			set { this.m_FedexMeter = value; }
		}

		/// URL To FedEx server
		public string FedexServer
		{
			get { return this.m_FedexServer; }
			set { this.m_FedexServer = value; }
		}


		public string UPSServer // URL To ups server, either test or live
		{
			get { return m_upsServer; }
			set { m_upsServer = value.Trim(); }
		}

		public string UPSUsername   // URL To ups server, either test or live
		{
			get { return m_upsUsername; }
			set { m_upsUsername = value.Trim(); }
		}

		public string UPSPassword   // URL To ups server, either test or live
		{
			get { return m_upsPassword; }
			set { m_upsPassword = value.Trim(); }
		}

		public string UPSLicense    // URL To ups server, either test or live
		{
			get { return m_upsLicense; }
			set { m_upsLicense = value.Trim(); }
		}

		public string USPSLogin // USPS Login information, "Username,Password" Please note: The login information is case sensitive
		{
			get { return m_uspsLogin; }
			set
			{
				m_uspsLogin = value.Trim();
				string[] arrUSPSLogin = m_uspsLogin.Split(',');
				try
				{
					m_uspsUsername = arrUSPSLogin[0].Trim();
					m_uspsPassword = arrUSPSLogin[1].Trim();
				}
				catch { }
			}
		}

		public string USPSServer    // URL To usps server, either test or live
		{
			get { return m_uspsServer; }
			set { m_uspsServer = value.Trim(); }
		}

		public string USPSUsername  // URL To usps server, either test or live
		{
			get { return m_uspsUsername; }
			set { m_uspsUsername = value.Trim(); }
		}

		public string USPSPassword  // URL To usps server, either test or live
		{
			get { return m_uspsPassword; }
			set { m_uspsPassword = value.Trim(); }
		}

		public string DestinationAddress    // Shipment destination street address
		{
			get { return m_DestinationAddress; }
			set { m_DestinationAddress = value; }
		}

		public string DestinationAddress2   // Shipment destination street address continued
		{
			get { return m_DestinationAddress2; }
			set { m_DestinationAddress2 = value; }
		}

		public string DestinationCity   // Shipment destination city
		{
			get { return m_DestinationCity; }
			set { m_DestinationCity = value; }
		}

		public string DestinationStateProvince  // Shipment destination State or Province
		{
			get
			{
				if(m_DestinationStateProvince == "-" || m_DestinationStateProvince == "--" || m_DestinationStateProvince == "ZZ")
				{
					return String.Empty;
				}
				else
				{
					return m_DestinationStateProvince;
				}
			}
			set { m_DestinationStateProvince = value; }
		}

		public string DestinationZipPostalCode  // Shipment Destination Zip or Postal Code
		{
			get { return m_DestinationZipPostalCode; }
			set { m_DestinationZipPostalCode = value; }
		}

		public string DestinationCountry    // Shipment Destination Country
		{
			get { return m_DestinationCountry; }
			set { m_DestinationCountry = value; }
		}

		public ResidenceTypes DestinationResidenceType  // Shipment Destination ResidenceType
		{
			get { return m_DestinationResidenceType; }
			set { m_DestinationResidenceType = value; }
		}

		public string OriginAddress // Shipment origin street address
		{
			get { return m_OriginAddress; }
			set { m_OriginAddress = value; }
		}

		public string OriginAddress2    // Shipment origin street address continued
		{
			get { return m_OriginAddress2; }
			set { m_OriginAddress2 = value; }
		}

		public string OriginCity    // Shipment origin city
		{
			get { return m_OriginCity; }
			set { m_OriginCity = value; }
		}

		public string OriginStateProvince   // Shipment origin State or Province
		{
			get { return m_OriginStateProvince; }
			set { m_OriginStateProvince = value; }
		}

		public string OriginZipPostalCode   // Shipment Origin Zip or Postal Code
		{
			get { return m_OriginZipPostalCode; }
			set { m_OriginZipPostalCode = value; }
		}

		public string OriginCountry // Shipment Origin Country
		{
			get { return m_OriginCountry; }
			set { m_OriginCountry = value; }
		}

		public Decimal ShipmentWeight   // Shipment shipmentWeight
		{
			get { return m_ShipmentWeight; }
			set { m_ShipmentWeight = value; }
		}

		public decimal ShipmentValue    //  Shipment value
		{
			get { return m_ShipmentValue; }
			set { m_ShipmentValue = value; }
		}

		public bool TestMode    // Boolean value to set entire class into test mode. Only test servers will be used if applicable
		{
			get { return m_TestMode; }
			set { m_TestMode = value; }
		}

		public Decimal Length   // Single value representing the lenght of the package in inches
		{
			get { return m_Length; }
			set { m_Length = value; }
		}

		public Decimal Width    // Single value representing the width of the package in inches
		{
			get { return m_Width; }
			set { m_Width = value; }
		}

		public Decimal Height   // Single value representing the height of the package in inches
		{
			get { return m_Height; }
			set { m_Height = value; }
		}

		public RTShipping()
		{
			UPSLogin = AppLogic.AppConfig("RTShipping.UPS.Username") + "," + AppLogic.AppConfig("RTShipping.UPS.Password") + "," + AppLogic.AppConfig("RTShipping.UPS.License");
			UPSServer = AppLogic.AppConfig("RTShipping.UPS.Server");
			UPSUsername = AppLogic.AppConfig("RTShipping.UPS.Username");
			UPSPassword = AppLogic.AppConfig("RTShipping.UPS.Password");
			UPSLicense = AppLogic.AppConfig("RTShipping.UPS.License");

			USPSServer = AppLogic.AppConfig("RTShipping.USPS.Server");
			USPSLogin = AppLogic.AppConfig("RTShipping.USPS.Username") + "," + AppLogic.AppConfig("RTShipping.USPS.Password");
			USPSUsername = AppLogic.AppConfig("RTShipping.USPS.Username");
			USPSPassword = AppLogic.AppConfig("RTShipping.USPS.Password");

			FedexAccountNumber = AppLogic.AppConfig("RTShipping.FEDEX.AccountNumber");
			FedexKey = AppLogic.AppConfig("RTShipping.FEDEX.Key");
			FedexPassword = AppLogic.AppConfig("RTShipping.FEDEX.Password");
			FedexServer = AppLogic.AppConfig("RTShipping.FEDEX.Server");
			FedexMeter = AppLogic.AppConfig("RTShipping.FEDEX.Meter");

			OriginAddress = AppLogic.AppConfig("RTShipping.OriginAddress");
			OriginAddress2 = AppLogic.AppConfig("RTShipping.OriginAddress2");
			OriginCity = AppLogic.AppConfig("RTShipping.OriginCity");
			OriginStateProvince = AppLogic.AppConfig("RTShipping.OriginState");
			OriginZipPostalCode = AppLogic.AppConfig("RTShipping.OriginZip");
			OriginCountry = AppLogic.AppConfig("RTShipping.OriginCountry");

			if(OriginCountry.Equals("US", StringComparison.InvariantCultureIgnoreCase))
			{
				try
				{
					OriginZipPostalCode = OriginZipPostalCode.Substring(0, 5);
				}
				catch
				{
					throw new Exception("The RTShipping.OriginZip AppConfig parameter is invalid, please update this value.");
				}
			}

			m_DestinationAddress = string.Empty;
			m_DestinationAddress2 = string.Empty;
			m_DestinationCity = string.Empty;
			m_DestinationStateProvince = string.Empty;
			m_DestinationZipPostalCode = string.Empty;
			m_DestinationCountry = string.Empty;
			m_DestinationResidenceType = ResidenceTypes.Unknown;

			m_ShipmentWeight = 0.0M;

			m_ShipmentValue = System.Decimal.Zero;

			m_TestMode = false;

			ratesValues = new ArrayList();
			ratesText = new ArrayList();
		}

		/// <summary>
		/// Main method which retrieves rates. Returns a dropdown list, radio button list, or multiline select box
		/// </summary>
		/// <param name="allShipments">The Shipments object which contains the shipments to be rated when RTShipping.MultiDistributorCalculation is true</param>
		/// <param name="carriers">The carriers to get rates from: UPS, USPS, FedEx, DHL. Use a comma separated list</param>
		/// <param name="shippingTaxRate">The tax rate for shipping to display in the list of rate options, send 0 to to add no tax</param>
		/// <param name="extraFee"></param>
		/// <param name="markupPercent"></param>
		/// <param name="realtimeShipRequest"></param>
		/// <param name="realtimeShipResponse"></param>
		/// <param name="shipmentValue"></param>
		/// <returns>System.String</returns>
		public object GetRates(Shipments allShipments, string carriers, decimal shippingTaxRate, out string realtimeShipRequest, out string realtimeShipResponse, decimal extraFee, decimal markupPercent, decimal shipmentValue)
		{
			// Get all carriers to retrieve rates for
			var carriersNames = carriers
				.Split(',')
				.Select(carriersName => carriersName
					.Trim()
					.ToUpperInvariant());

			// Parallel queries can lose access to HttpContext.Current, so we'll capture the cache and pass it in.
			var cache = HttpContext.Current.Cache;
			var allRequestsAndResponses = carriersNames
				.AsParallel()
				.SelectMany(carrierName => GetAllShippingRequestsAndResponses(
					allShipments,
					carrierName,
					shippingTaxRate,
					extraFee,
					markupPercent,
					shipmentValue,
					cache))
				.Aggregate(
					new
					{
						request = string.Empty,
						response = string.Empty,
						errors = string.Empty,
						shippingMethods = new ShippingMethodCollection(),
					},
					(all, carrier) => new
					{
						request = all.request + carrier.Item1,
						response = all.response + carrier.Item2,
						errors = all.errors + carrier.Item3,
                        shippingMethods = new ShippingMethodCollection(all.shippingMethods.Concat(carrier.Item4)),
					});

			realtimeShipRequest = allRequestsAndResponses.request;
			realtimeShipResponse = allRequestsAndResponses.response;
			var shippingMethods = allRequestsAndResponses.shippingMethods;

			//Make sure we bubble up any errors
			shippingMethods.ErrorMsg = allRequestsAndResponses.errors;

			// optionally sort rates by cost
			var shippingMethodSorter = AppLogic.AppConfigBool("RTShipping.SortByRate")
				? (IComparer<ShippingMethod>)new ShippingCostsSorter()
				: new CarrierSorter();

			shippingMethods.Sort(shippingMethodSorter);

			//Apply attributes from the database to the shipping methods returned.
			var customer = HttpContext.Current.GetCustomer();
			string sql = "select Name, DisplayName, ImageFileName from ShippingMethod";
			using(var connection = new SqlConnection(DB.GetDBConn()))
			{
				connection.Open();
				using(var reader = DB.GetRS(sql, connection))
					while(reader.Read())
					{
						var displayName = DB.RSFieldByLocale(reader, "DisplayName", customer.LocaleSetting);
						var ImageFileName = DB.RSFieldByLocale(reader, "ImageFileName", customer.LocaleSetting);
						var methodName = DB.RSField(reader, "Name");

						foreach(ShippingMethod method in shippingMethods)
						{
							if(method.Name == methodName)
							{
								method.DisplayName = displayName;
								method.ImageFileName = ImageFileName;
							}
						}
					}
			}
			
			return shippingMethods;
		}

		IEnumerable<Tuple<string, string, string, ShippingMethodCollection>> GetAllShippingRequestsAndResponses(Shipments allShipments, string carrierName, decimal shippingTaxRate, decimal extraFee, decimal markupPercent, decimal shipmentValue, Cache cache)
		{
			switch(carrierName)
			{
				case UpsIdentifier:
					{
						string shippingRequest, shippingResponse;
						var shippingMethods = new ShippingMethodCollection();
						UPSGetRates(allShipments, out shippingRequest, out shippingResponse, extraFee, markupPercent, shippingTaxRate, cache, ref shippingMethods);

						yield return Tuple.Create(
							"<UPSRequest>" + shippingRequest.Replace("<?xml version=\"1.0\"?>", "") + "</UPSRequest>",
							"<UPSResponse>" + shippingResponse.Replace("<?xml version=\"1.0\"?>", "") + "</UPSResponse>",
								shippingMethods.ErrorMsg,
							shippingMethods);

						break;
					}

				case Ups2Identifier:
					{
						string shippingRequest, shippingResponse;
						var shippingMethods = new ShippingMethodCollection();
						UPS2GetRates(allShipments, out shippingRequest, out shippingResponse, markupPercent, shipmentValue, shippingTaxRate, ref shippingMethods);

						yield return Tuple.Create(
							"<UPS2Request>" + shippingRequest.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", "") + "</UPS2Request>",
							"<UPS2Response>" + shippingResponse.Replace("<?xml version=\"1.0\"?>", "") + "</UPS2Response>",
								shippingMethods.ErrorMsg,
							shippingMethods);

						break;
					}

				case UspsIdentifier:
					{
						var domesticShipments = new Shipments();
						var internationalShipments = new Shipments();

						foreach(Packages shipment in allShipments)
						{
							if(IsDomesticCountryCode(shipment.DestinationCountryCode))
							{
								domesticShipments.AddPackages(shipment);
								domesticShipments.HasFreeItems = shipment
									.Cast<Package>()
									.Where(package => package.IsFreeShipping)
									.Any();
							}
							else
							{
								internationalShipments.AddPackages(shipment);
								internationalShipments.HasFreeItems = shipment
									.Cast<Package>()
									.Where(package => package.IsFreeShipping)
									.Any();
							}
						}

						if(domesticShipments.Count > 0)
						{
							string shippingRequest, shippingResponse;
							var shippingMethods = new ShippingMethodCollection();
							USPSGetRates(domesticShipments, out shippingRequest, out shippingResponse, extraFee, markupPercent, shippingTaxRate, cache, ref shippingMethods);

							yield return Tuple.Create(
								"<USPSRequest>" + shippingRequest.Replace("API=RateV4&Xml=", "") + "</USPSRequest>",
								"<USPSResponse>" + shippingResponse.Replace("<?xml version=\"1.0\"?>", "") + "</USPSResponse>",
								shippingMethods.ErrorMsg,
								shippingMethods);
						}

						if(internationalShipments.Count > 0)
						{
							string shippingRequest, shippingResponse;
							var shippingMethods = new ShippingMethodCollection();
							USPSIntlGetRates(internationalShipments, out shippingRequest, out shippingResponse, extraFee, markupPercent, shippingTaxRate, cache, ref shippingMethods);

							yield return Tuple.Create(
								"<USPSRequest>" + shippingRequest.Replace("API=RateV4&Xml=", "") + "</USPSRequest>",
								"<USPSResponse>" + shippingResponse.Replace("<?xml version=\"1.0\"?>", "") + "</USPSResponse>",
								shippingMethods.ErrorMsg,
								shippingMethods);
						}

						break;
					}

				case FedExIdentifier:
					{
						string shippingRequest, shippingResponse;
						var shippingMethods = new ShippingMethodCollection();
						if(allShipments.Count > 0)
						{
							FedExGetRates(allShipments, out shippingRequest, out shippingResponse, extraFee, markupPercent, shippingTaxRate, cache, ref shippingMethods);

							yield return Tuple.Create(
								"<FedExRequest>" + shippingRequest.Replace("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>", "") + "</FedExRequest>",
								"<FedExResponse>" + shippingResponse.Replace("<?xml version=\"1.0\" encoding=\"UTF-8\"?>", "") + "</FedExResponse>",
								shippingMethods.ErrorMsg,
								shippingMethods);
						}

						break;
					}

				case DhlIdentifier:
					{
						var internationalShipments = new Shipments();
						foreach(Packages shipment in allShipments)
						{
							if(!IsDomesticCountryCode(shipment.DestinationCountryCode))
							{
								internationalShipments.AddPackages(shipment);
								internationalShipments.HasFreeItems = shipment
									.Cast<Package>()
									.Where(package => package.IsFreeShipping)
									.Any();
							}
						}

						if(internationalShipments.Count > 0)
						{
							string shippingRequest, shippingResponse;
							var shippingMethods = new ShippingMethodCollection();
							DHLIntlGetRates(internationalShipments, out shippingRequest, out shippingResponse, extraFee, markupPercent, shippingTaxRate, ref shippingMethods);

							yield return Tuple.Create(
								"<DHLRequest>" + shippingRequest + "</DHLRequest>",
								"<DHLResponse>" + shippingResponse + "</DHLResponse>",
								shippingMethods.ErrorMsg,
								shippingMethods);
						}

						break;
					}

				case CanadaPostIdentifier:
					{
						string shippingRequest, shippingResponse;
						var shippingMethods = new ShippingMethodCollection();
						CanadaPostGetRates(allShipments, out shippingRequest, out shippingResponse, extraFee, markupPercent, shippingTaxRate, ref shippingMethods);

						yield return Tuple.Create(
							"<CanadaPostRequest>" + shippingRequest.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", "") + "</CanadaPostRequest>",
							shippingResponse.Contains("]>")
								? "<CanadaPostResponse>" + shippingResponse.Substring(shippingResponse.IndexOf("]>") + 2) + "</CanadaPostResponse>"
								: string.Empty,
							shippingMethods.ErrorMsg,
							shippingMethods);

						break;
					}

				case AustraliaPostIdentifier:
					{
						string shippingRequest, shippingResponse;
						var shippingMethods = new ShippingMethodCollection();
						AusPostGetRates(allShipments, out shippingRequest, out shippingResponse, extraFee, markupPercent, shippingTaxRate, ref shippingMethods);

						yield return Tuple.Create(
							"<AusPostRequest><![CDATA[" + shippingRequest + "]]></AusPostRequest>",
							"<AusPostResponse><![CDATA[" + shippingResponse + "]]></AusPostResponse>",
							shippingMethods.ErrorMsg,
							shippingMethods);

						break;
					}
			}
		}

		private void UPSGetRates(Shipments AllShipments, out string RTShipRequest, out string RTShipResponse, decimal ExtraFee, Decimal MarkupPercent, decimal ShippingTaxRate, Cache cache, ref ShippingMethodCollection shippingMethods)  // Private method to retrieve UPS rates
		{
			RTShipRequest = String.Empty;
			RTShipResponse = String.Empty;

			HasFreeItems = false;
			PackageQuantity = 1;

			// check all required info
			if(m_upsLogin == string.Empty || m_upsUsername == string.Empty || m_upsPassword == string.Empty || m_upsLicense == string.Empty)
			{
				shippingMethods.ErrorMsg = "Error: You must provide UPS login information";
				return;
			}

			foreach(Packages ps in AllShipments)
			{
				if(ps.DestinationStateProvince == "AE")
				{
					shippingMethods.ErrorMsg = "UPS Does not ship to APO Boxes";
					return;
				}
			}

			// Check for test mode
			if(m_TestMode)
			{
				m_upsServer = AppLogic.AppConfig("RTShipping.UPS.TestServer");
			}

			// Check server setting
			if(m_upsServer == string.Empty)
			{
				shippingMethods.ErrorMsg = "Error: You must provide the UPS Server";
				return;
			}

			// Check for m_ShipmentWeight
			if(m_ShipmentWeight == 0.0M)
			{
				shippingMethods.ErrorMsg = "Error: Shipment Weight must be great than 0 " + Localization.WeightUnits() + ".";
				return;
			}

			Decimal maxWeight = AppLogic.AppConfigUSDecimal("RTShipping.UPS.MaxWeight");
			if(maxWeight == 0)
			{
				maxWeight = 150;
			}

			if(m_ShipmentWeight > maxWeight)
			{
				shippingMethods.ErrorMsg = "UPS " + AppLogic.AppConfig("RTShipping.CallForShippingPrompt");
				return;
			}

			// Set the access request Xml
			String accessRequest = String.Empty;
			StringBuilder shipmentRequest = new StringBuilder(1024);

			Boolean MultiDistributorEnabled = AppLogic.AppConfigBool("RTShipping.MultiDistributorCalculation") && AllShipments.HasDistributorItems;

			HasFreeItems = false;
			PackageQuantity = 1;

			foreach(Packages Shipment in AllShipments)
			{
				shipmentRequest = new StringBuilder(1024);

				accessRequest = string.Format("<?xml version=\"1.0\"?><AccessRequest xml:lang=\"en-us\"><AccessLicenseNumber>{0}</AccessLicenseNumber><UserId>{1}</UserId><Password>{2}</Password></AccessRequest>", this.m_upsLicense, this.m_upsUsername, this.m_upsPassword);

				// Set the rate request Xml

				shipmentRequest.Append("<?xml version=\"1.0\"?>");
				shipmentRequest.Append("<RatingServiceSelectionRequest xml:lang=\"en-US\">");
				shipmentRequest.Append("<Request>");
				shipmentRequest.Append("<RequestAction>Rate</RequestAction>");
				shipmentRequest.Append("<RequestOption>Shop</RequestOption>");
				shipmentRequest.Append("<TransactionReference>");
				shipmentRequest.Append("<CustomerContext>Rating and Service</CustomerContext>");
				shipmentRequest.Append("<XpciVersion>1.0001</XpciVersion>");
				shipmentRequest.Append("</TransactionReference>");
				shipmentRequest.Append("</Request>");
				shipmentRequest.Append("<PickupType>");
				shipmentRequest.Append("<Code>");
				shipmentRequest.Append(MapPickupType(Shipment.PickupType));
				shipmentRequest.Append("</Code>");
				shipmentRequest.Append("</PickupType>");
				//Add proper elements to support SuggestedRetailRates
				if(AppLogic.AppConfig("RTShipping.UPS.UPSPickupType").Equals("UPSSUGGESTEDRETAILRATES", StringComparison.InvariantCultureIgnoreCase))
				{
					shipmentRequest.Append("<CustomerClassification>");
					shipmentRequest.Append("<Code>04</Code>");
					shipmentRequest.Append("</CustomerClassification>");
				}
				shipmentRequest.Append("<Shipment>");
				shipmentRequest.Append("<Shipper>");
				shipmentRequest.Append("<Address>");
				shipmentRequest.Append("<City>");
				if(MultiDistributorEnabled)
					shipmentRequest.Append(CommonLogic.IIF(Shipment.OriginCity == "", m_OriginCity.ToUpperInvariant(), Shipment.OriginCity.ToUpperInvariant()));
				else
					shipmentRequest.Append(m_OriginCity.ToUpperInvariant());
				shipmentRequest.Append("</City>");
				shipmentRequest.Append("<StateProvinceCode>");
				if(MultiDistributorEnabled)
					shipmentRequest.Append(CommonLogic.IIF(Shipment.OriginStateProvince == "", m_OriginStateProvince.ToUpperInvariant(), Shipment.OriginStateProvince.ToUpperInvariant()));
				else
					shipmentRequest.Append(m_OriginStateProvince.ToUpperInvariant());
				shipmentRequest.Append("</StateProvinceCode>");
				shipmentRequest.Append("<PostalCode>");
				if(!MultiDistributorEnabled || Shipment.OriginZipPostalCode == "")
					shipmentRequest.Append(m_OriginZipPostalCode);
				else
					shipmentRequest.Append(Shipment.OriginZipPostalCode);
				shipmentRequest.Append("</PostalCode>");
				shipmentRequest.Append("<CountryCode>");
				if(MultiDistributorEnabled)
					shipmentRequest.Append(CommonLogic.IIF(Shipment.OriginCountryCode == "", m_OriginCountry.ToUpperInvariant(), Shipment.OriginCountryCode.ToUpperInvariant()));
				else
					shipmentRequest.Append(m_OriginCountry.ToUpperInvariant());
				shipmentRequest.Append("</CountryCode>");
				shipmentRequest.Append("</Address>");
				shipmentRequest.Append("</Shipper>");
				shipmentRequest.Append("<ShipTo>");
				shipmentRequest.Append("<Address>");
				shipmentRequest.Append("<City>");
				shipmentRequest.Append(Shipment.DestinationCity.ToUpperInvariant());
				shipmentRequest.Append("</City>");
				shipmentRequest.Append("<StateProvinceCode>");
				shipmentRequest.Append(Shipment.DestinationStateProvince.ToUpperInvariant());
				shipmentRequest.Append("</StateProvinceCode>");
				shipmentRequest.Append("<PostalCode>");
				shipmentRequest.Append(Shipment.DestinationZipPostalCode);
				shipmentRequest.Append("</PostalCode>");
				shipmentRequest.Append("<CountryCode>");
				shipmentRequest.Append(Shipment.DestinationCountryCode.ToUpperInvariant());
				shipmentRequest.Append("</CountryCode>");
				shipmentRequest.Append(CommonLogic.IIF(Shipment.DestinationResidenceType == ResidenceTypes.Commercial, "", "<ResidentialAddressIndicator/>"));
				shipmentRequest.Append("</Address>");
				shipmentRequest.Append("</ShipTo>");
				shipmentRequest.Append("<ShipmentWeight>");
				shipmentRequest.Append("<UnitOfMeasurement>");
				shipmentRequest.Append("<Code>");
				shipmentRequest.Append(AppLogic.AppConfig("RTShipping.WeightUnits").Trim().ToUpperInvariant());
				shipmentRequest.Append("</Code>");
				shipmentRequest.Append("</UnitOfMeasurement>");
				shipmentRequest.Append("<Weight>");
				shipmentRequest.Append(Localization.DecimalStringForDB(Shipment.Weight));
				shipmentRequest.Append("</Weight>");
				shipmentRequest.Append("</ShipmentWeight>");


				// loop through the packages
				foreach(Package p in Shipment)
				{
					//can do this because any Shipment that has free items will only have 1 Package p
					if(p.IsFreeShipping)
					{
						HasFreeItems = true;
					}

					//can do this because any Shipment that has IsShipSeparately items will only have 1 Package p
					//sanity check to make sure we don't call the server 100 times for each carrier if 100 of an
					//IsShipSeparately item is ordered.  ShoppingCart.cs will add 1 Package p with a quantity
					//if the item IsShipSeparately
					if(p.IsShipSeparately)
					{
						PackageQuantity = p.Quantity;
					}

					//Check for invalid weights and assign a new value if necessary
					if(p.Weight < AppLogic.AppConfigUSDecimal("UPS.MinimumPackageWeight"))
					{
						p.Weight = AppLogic.AppConfigUSDecimal("UPS.MinimumPackageWeight");
					}

					shipmentRequest.Append("<Package>");
					shipmentRequest.Append("<PackagingType>");
					shipmentRequest.Append("<Code>02</Code>");
					shipmentRequest.Append("</PackagingType>");
					shipmentRequest.Append("<Dimensions>");
					shipmentRequest.Append("<UnitOfMeasurement>");

					if(AppLogic.AppConfig("RTShipping.WeightUnits").Trim().Equals("LBS", StringComparison.InvariantCultureIgnoreCase))
						shipmentRequest.Append("<Code>IN</Code>");
					else
						shipmentRequest.Append("<Code>CM</Code>");

					shipmentRequest.Append("</UnitOfMeasurement>");
					shipmentRequest.Append("<Length>");
					shipmentRequest.Append(p.Length.ToString());
					shipmentRequest.Append("</Length>");
					shipmentRequest.Append("<Width>");
					shipmentRequest.Append(p.Width.ToString());
					shipmentRequest.Append("</Width>");
					shipmentRequest.Append("<Height>");
					shipmentRequest.Append(p.Height.ToString());
					shipmentRequest.Append("</Height>");
					shipmentRequest.Append("</Dimensions>");
					shipmentRequest.Append("<Description>");
					shipmentRequest.Append(p.PackageId.ToString());
					shipmentRequest.Append("</Description>");
					shipmentRequest.Append("<PackageWeight>");
					shipmentRequest.Append("<UnitOfMeasure>");
					shipmentRequest.Append("<Code>");
					shipmentRequest.Append(AppLogic.AppConfig("RTShipping.WeightUnits").Trim().ToUpperInvariant());
					shipmentRequest.Append("</Code>");
					shipmentRequest.Append("</UnitOfMeasure>");
					shipmentRequest.Append("<Weight>");
					shipmentRequest.Append(Localization.DecimalStringForDB(p.Weight));
					shipmentRequest.Append("</Weight>");
					shipmentRequest.Append("</PackageWeight>");
					shipmentRequest.Append("<OversizePackage />");

					if(p.Insured && (p.InsuredValue != 0))
					{
						shipmentRequest.Append("<PackageServiceOptions>");
						shipmentRequest.Append("<InsuredValue>");
						shipmentRequest.Append("<CurrencyCode>USD</CurrencyCode>");
						shipmentRequest.Append("<MonetaryValue>");
						shipmentRequest.Append(Localization.CurrencyStringForDBWithoutExchangeRate(p.InsuredValue));
						shipmentRequest.Append("</MonetaryValue>");
						shipmentRequest.Append("</InsuredValue>");
						shipmentRequest.Append("</PackageServiceOptions>");
					}

					shipmentRequest.Append("</Package>");
				}

				shipmentRequest.Append("<ShipmentServiceOptions/></Shipment></RatingServiceSelectionRequest>");

				// Concat the requests
				String fullUPSRequest = accessRequest + shipmentRequest.ToString();

				RTShipRequest = fullUPSRequest;

				// Send request & capture response
				var result = MakeCachedRequest(cache, m_upsServer, HttpMethod.Post, fullUPSRequest);

				RTShipResponse = result;

				// Load Xml into a XmlDocument object
				XmlDocument UPSResponse = new XmlDocument();
				try
				{
					UPSResponse.LoadXml(result);
				}
				catch
				{
					shippingMethods.ErrorMsg = "Error: UPS Gateway Did Not Respond";
					return;
				}

				// Get Response code: 0 = Fail, 1 = Success
				XmlNodeList UPSResponseCode = UPSResponse.GetElementsByTagName("ResponseStatusCode");

				if(UPSResponseCode[0].InnerText == "1") // Success
				{
					// Loop through elements & get rates
					XmlNodeList ratedShipments = UPSResponse.GetElementsByTagName("RatedShipment");
					string tempService = string.Empty;
					Decimal tempRate = 0.0M;
					for(int i = 0; i < ratedShipments.Count; i++)
					{
						XmlNode shipmentX = ratedShipments.Item(i);
						tempService = UPSServiceCodeDescription(shipmentX["Service"]["Code"].InnerText);

						if(ShippingMethodIsAllowed(tempService, UpsName))
						{
							tempRate = Localization.ParseUSDecimal(shipmentX["TotalCharges"]["MonetaryValue"].InnerText);

							//multiply the returned rate by the quantity in the package to avoid calling
							//UPS more than necessary if there were multiple IsShipSeparately items
							//ordered.  If there weren't, PackageQuantity is 1 and the rate is normal
							tempRate = tempRate * PackageQuantity;

							if(MarkupPercent != System.Decimal.Zero)
							{
								tempRate = Decimal.Round(tempRate * (1.00M + (MarkupPercent / 100.0M)), 2, MidpointRounding.AwayFromZero);
							}

							decimal vat = Decimal.Round(tempRate * ShippingTaxRate, 2, MidpointRounding.AwayFromZero);
							tempService = StripHtmlAndRemoveSpecialCharacters(HttpUtility.HtmlDecode(tempService));
							if(!shippingMethods.MethodExists(tempService))
							{
								var s_method = new ShippingMethod();

								s_method.Carrier = UpsName;
								s_method.Name = tempService;
								s_method.Freight = tempRate;
								s_method.VatRate = vat;
								s_method.IsRealTime = true;
								s_method.Id = Shipping.GetShippingMethodID(s_method.Name);
								if(HasFreeItems)
								{
									s_method.FreeItemsRate = tempRate;
								}
								shippingMethods.Add(s_method);
							}
							else
							{
								int IndexOf = shippingMethods.GetIndex(tempService);
								ShippingMethod s_method = shippingMethods[IndexOf];

								s_method.Freight += tempRate;
								s_method.VatRate += vat;
								if(HasFreeItems)
								{
									s_method.FreeItemsRate += tempRate;
								}
								shippingMethods[IndexOf] = s_method;
							}
						}
					}
				}
				else // Error
				{
					XmlNodeList UPSError = UPSResponse.GetElementsByTagName("ErrorDescription");
					shippingMethods.ErrorMsg = "UPS Error: " + UPSError[0].InnerText;
					UPSError = null;
					return;
				}

				// Some clean up
				UPSResponseCode = null;
				UPSResponse = null;
				HasFreeItems = false;
				PackageQuantity = 1;
			}

			// Handling fee should only be added per shipping address not per package
			// let's just compute it here after we've gone through all the packages.
			// Also, since we can't be sure about the ordering of the method call here
			// and that the collection SM includes shipping methods from all possible carriers
			// we'll need to filter out the methods per this carrier to avoid side effects on the main collection
			foreach(ShippingMethod shipMethod in shippingMethods.PerCarrier(UpsName))
			{
				if(shipMethod.Freight != System.Decimal.Zero) //Don't add the fee to free methods.
					shipMethod.Freight += ExtraFee;
			}
		}

		private void USPSIntlGetRates(Shipments AllShipments, out string RTShipRequest, out string RTShipResponse, decimal ExtraFee, Decimal MarkupPercent, decimal ShippingTaxRate, Cache cache, ref ShippingMethodCollection shippingMethods) // Retrieves International rates for USPS
		{
			RTShipRequest = String.Empty;
			RTShipResponse = String.Empty;

			foreach(Packages Shipment in AllShipments)
			{

				// check all required info
				if(Shipment.DestinationCountryCode.Equals("US", StringComparison.InvariantCultureIgnoreCase))
				{
					shippingMethods.ErrorMsg = "Error: Calling USPS International but shipping to US country";
					return; // error
				}

				if(m_uspsLogin == string.Empty || m_uspsUsername == string.Empty)
				{
					shippingMethods.ErrorMsg = "Error: You must provide USPS login information";
					return;
				}

				// Check server setting
				if(m_uspsServer == string.Empty)
				{
					shippingMethods.ErrorMsg = "Error: You must provide the USPS server";
					return;
				}

				// Check for test mode
				if(m_TestMode)
				{
					m_uspsServer = AppLogic.AppConfig("RTShipping.USPS.TestServer");
				}

				// Check for m_ShipmentWeight
				if(ShipmentWeight == 0.0M)
				{
					shippingMethods.ErrorMsg = "Error: Shipment Weight must be greater than 0 " + Localization.WeightUnits() + ".";
					return;
				}

				Decimal maxWeight = AppLogic.AppConfigUSDecimal("RTShipping.USPS.MaxWeight");
				if(maxWeight == 0)
				{
					maxWeight = 70;
				}

				if(ShipmentWeight > maxWeight)
				{
					shippingMethods.ErrorMsg = "USPS " + AppLogic.AppConfig("RTShipping.CallForShippingPrompt");
					return;
				}


				HasFreeItems = false;
				PackageQuantity = 1;

				// Create the Xml request (International)
				StringBuilder USPSRequest = new StringBuilder(1024);
				USPSRequest.Append("API=IntlRateV2&Xml=");

				StringBuilder uspsReqLoop = new StringBuilder(1024);
				uspsReqLoop.Append("<IntlRateV2Request USERID=\"{0}\">");
				uspsReqLoop.Append("<Revision>2</Revision>");

				foreach(Package p in Shipment)
				{
					USPSWeight w = USPSGetWeight(p.Weight);

					//can do this because any Shipment that has free items will only have 1 Package p
					if(p.IsFreeShipping)
					{
						HasFreeItems = true;
					}

					//can do this because any Shipment that has IsShipSeparately items will only have 1 Package p
					//sanity check to make sure we don't call the server 100 times for each carrier if 100 of an
					//IsShipSeparately item is ordered.  ShoppingCart.cs will add 1 Package p with a quantity
					//if the item IsShipSeparately
					if(p.IsShipSeparately)
					{
						PackageQuantity = p.Quantity;
					}

					uspsReqLoop.Append("<Package ID=\"" + p.PackageId + "\">");
					uspsReqLoop.Append("<Pounds>" + w.pounds + "</Pounds>");
					uspsReqLoop.Append("<Ounces>" + w.ounces + "</Ounces>");
					uspsReqLoop.Append("<Machinable>True</Machinable>");
					uspsReqLoop.Append("<MailType>Package</MailType>");
					uspsReqLoop.Append("<ValueOfContents>" + p.InsuredValue + "</ValueOfContents>");

					uspsReqLoop.Append("<Country>");
					uspsReqLoop.Append(AppLogic.GetCountryName(Shipment.DestinationCountryCode));
					uspsReqLoop.Append("</Country>");

					uspsReqLoop.Append("<Container>RECTANGULAR</Container>");
					uspsReqLoop.Append(USPSGetSize(p.Length, p.Width, p.Height));

					uspsReqLoop.Append("<OriginZip>");
					uspsReqLoop.Append(OriginZipPostalCode);
					uspsReqLoop.Append("</OriginZip>");

					uspsReqLoop.Append("</Package>");
				}
				USPSRequest.Append(uspsReqLoop);
				USPSRequest.Append("</IntlRateV2Request>");

				// Replace login info
				String USPSRequest2 = string.Format(USPSRequest.ToString(), USPSUsername, USPSPassword);
				RTShipRequest += USPSRequest2;

				// Send request & capture response
				var result = MakeCachedRequest(cache, USPSServer, HttpMethod.Get, USPSRequest2);
				RTShipResponse += result;

				// Load Xml into a XmlDocument object
				XmlDocument USPSResponse = new XmlDocument();
				try
				{
					USPSResponse.LoadXml(result);
				}
				catch
				{
					shippingMethods.ErrorMsg = "Error: USPS Gateway Did Not Respond";
					return;
				}

				// Check for error
				XmlNodeList USPSErrors = USPSResponse.GetElementsByTagName("Error");
				if(USPSErrors.Count > 0) // Error has occurred
				{
					XmlNodeList USPSError = USPSResponse.GetElementsByTagName("Error");
					XmlNode USPSErrorMessage = USPSError.Item(0);
					ratesText.Add("USPS Error: " + USPSErrorMessage["Description"].InnerText);
					ratesValues.Add("USPS Error: " + USPSErrorMessage["Description"].InnerText);
					USPSError = null;
					return;
				}
				else
				{
					XmlNodeList nodesPackages = USPSResponse.GetElementsByTagName("Package");
					foreach(XmlNode nodePackage in nodesPackages)
					{
						XmlNodeList nodesServices = nodePackage.SelectNodes("Service");
						foreach(XmlNode nodeService in nodesServices)
						{
							try
							{
								string rateName = nodeService.SelectSingleNode("SvcDescription").InnerText;
								if(ShippingMethodIsAllowed("U.S. Postal " + rateName, "USPS") && rateName.IndexOf("Envelope") == -1 && rateName.IndexOf(" Document") == -1 && rateName.IndexOf("Letter") == -1)
								{
									decimal totalCharges = Localization.ParseUSDecimal(nodeService.SelectSingleNode("Postage").InnerText);

									//multiply the returned rate by the quantity in the package to avoid calling
									//more than necessary if there were multiple IsShipSeparately items
									//ordered.  If there weren't, PackageQuantity is 1 and the rate is normal
									totalCharges = totalCharges * PackageQuantity;

									if(MarkupPercent != System.Decimal.Zero)
									{
										totalCharges = Decimal.Round(totalCharges * (1.00M + (MarkupPercent / 100.0M)), 2, MidpointRounding.AwayFromZero);
									}

									decimal vat = Decimal.Round(totalCharges * ShippingTaxRate, 2, MidpointRounding.AwayFromZero);
									rateName = StripHtmlAndRemoveSpecialCharacters(HttpUtility.HtmlDecode(rateName).Replace("<sup>&reg;</sup>", ""));
									if(!shippingMethods.MethodExists(rateName))
									{
										ShippingMethod s_method = new ShippingMethod();
										s_method.Carrier = "USPS";
										s_method.Name = rateName;
										s_method.Freight = totalCharges;
										s_method.VatRate = vat;
										s_method.IsRealTime = true;
										s_method.Id = Shipping.GetShippingMethodID(s_method.Name);
										if(HasFreeItems)
											s_method.FreeItemsRate = totalCharges;
										shippingMethods.Add(s_method);
									}
									else
									{
										int IndexOf = shippingMethods.GetIndex(rateName);
										ShippingMethod s_method = shippingMethods[IndexOf];

										s_method.Freight += totalCharges;
										s_method.VatRate += vat;

										if(HasFreeItems)
										{
											s_method.FreeItemsRate += totalCharges;
										}

										shippingMethods[IndexOf] = s_method;
									}
								}
							}
							catch(Exception ex)
							{
								SysLog.LogMessage("There was an error getting rate info for a USPS shipping service.", ex.Message, MessageTypeEnum.Informational, MessageSeverityEnum.Error);
							}
						}
					}

					// Clean up
					USPSResponse = null;
				}

				//cleanup
				HasFreeItems = false;
				PackageQuantity = 1;
			}

			// Handling fee should only be added per shipping address not per package
			// let's just compute it here after we've gone through all the packages.
			// Also, since we can't be sure about the ordering of the method call here
			// and that the collection SM includes shipping methods from all possible carriers
			// we'll need to filter out the methods per this carrier to avoid side effects on the main collection
			foreach(ShippingMethod shipMethod in shippingMethods.PerCarrier("USPS"))
			{
				if(shipMethod.Freight != System.Decimal.Zero) //Don't add the fee to free methods.
					shipMethod.Freight += ExtraFee;
			}
		}

		private void USPSGetRates(Shipments AllShipments, out string RTShipRequest, out string RTShipResponse, decimal ExtraFee, Decimal MarkupPercent, decimal ShippingTaxRate, Cache cache, ref ShippingMethodCollection shippingMethods) // Retrieves rates for USPS
		{
			RTShipRequest = String.Empty;
			RTShipResponse = String.Empty;



			// check all required info
			if(USPSLogin == string.Empty || USPSUsername == string.Empty)
			{
				shippingMethods.ErrorMsg = "Error: You must provide USPS login information";
				return;
			}

			// Check server setting
			if(USPSServer == string.Empty)
			{
				shippingMethods.ErrorMsg = "Error: You must provide the USPS server";
				return;
			}

			// Check for test mode
			if(TestMode)
			{
				shippingMethods.ErrorMsg = "Error: Test Mode not supported for USPS";
				return;
			}

			// Check for shipmentWeight
			if(ShipmentWeight == 0.0M)
			{
				shippingMethods.ErrorMsg = "Error: Shipment Shipment Weight must be greater than 0 " + Localization.WeightUnits() + ".";
				return;
			}

			Decimal maxWeight = AppLogic.AppConfigUSDecimal("RTShipping.USPS.MaxWeight");
			if(maxWeight == 0)
				maxWeight = 70;

			if(ShipmentWeight > maxWeight)
			{
				shippingMethods.ErrorMsg = "USPS " + AppLogic.AppConfig("RTShipping.CallForShippingPrompt");
				return;
			}
			foreach(Packages Shipment in AllShipments)
			{
				// Create the Xml request (Domestinc)
				// 0 = Usename
				// 1 = Password
				// 2 = Service name
				// 3 = origin zip
				// 4 = dest zip
				// 5 = pounds
				// 6 = ounces (always 0)
				// 7 = Machinable? Always false
				StringBuilder USPSRequest = new StringBuilder(1024);
				USPSRequest.Append("API=RateV4&Xml=");

				String[] USPSServices = AppLogic.AppConfig("RTShipping.USPS.Services").Split(',');

				HasFreeItems = false;
				PackageQuantity = 1;

				StringBuilder uspsReqLoop = new StringBuilder(1024);
				uspsReqLoop.Append("<RateV4Request USERID=\"{0}\">");
				uspsReqLoop.Append("<Revision />");
				foreach(Package p in Shipment)
				{
					USPSWeight w = USPSGetWeight(p.Weight);

					//can do this because any Shipment that has free items will only have 1 Package p
					if(p.IsFreeShipping)
					{
						HasFreeItems = true;
					}

					//can do this because any Shipment that has IsShipSeparately items will only have 1 Package p
					//sanity check to make sure we don't call the server 100 times for each carrier if 100 of an
					//IsShipSeparately item is ordered.  ShoppingCart.cs will add 1 Package p with a quantity
					//if the item IsShipSeparately
					if(p.IsShipSeparately)
					{
						PackageQuantity = p.Quantity;
					}

					for(int srvcs = 0; srvcs < USPSServices.Length; srvcs++)
					{

						uspsReqLoop.Append("<Package ID=\"");
						uspsReqLoop.Append(p.PackageId.ToString());
						uspsReqLoop.Append("-");
						uspsReqLoop.Append(srvcs.ToString());
						uspsReqLoop.Append("\">");
						uspsReqLoop.Append("<Service>");
						uspsReqLoop.Append(USPSServices[srvcs].ToString());
						uspsReqLoop.Append("</Service>");
						uspsReqLoop.Append("<ZipOrigination>");
						uspsReqLoop.Append(OriginZipPostalCode);
						uspsReqLoop.Append("</ZipOrigination>");
						uspsReqLoop.Append("<ZipDestination>");
						if(Shipment.DestinationCountryCode.Equals("US", StringComparison.InvariantCultureIgnoreCase) && Shipment.DestinationZipPostalCode.Length > 5)
						{
							uspsReqLoop.Append(Shipment.DestinationZipPostalCode.Substring(0, 5));
						}
						else
						{
							uspsReqLoop.Append(Shipment.DestinationZipPostalCode);
						}
						uspsReqLoop.Append("</ZipDestination>");
						uspsReqLoop.Append("<Pounds>");
						uspsReqLoop.Append(w.pounds.ToString());
						uspsReqLoop.Append("</Pounds>");
						uspsReqLoop.Append("<Ounces>");
						uspsReqLoop.Append(w.ounces.ToString());
						uspsReqLoop.Append("</Ounces>");
						uspsReqLoop.Append("<Container/>");
						uspsReqLoop.Append(USPSGetSize(p.Length, p.Width, p.Height));
						uspsReqLoop.Append("<Machinable>False</Machinable>");
						uspsReqLoop.Append("</Package>");
					}
				}
				USPSRequest.Append(uspsReqLoop);
				USPSRequest.Append("</RateV4Request>");

				// Replace login info
				String USPSRequest2 = String.Format(USPSRequest.ToString(), USPSUsername, USPSPassword);
				RTShipRequest += USPSRequest2;

				// Send request & capture response
				var result = MakeCachedRequest(cache, USPSServer, HttpMethod.Get, USPSRequest2);
				RTShipResponse += result;

				// Load Xml into a XmlDocument object
				XmlDocument USPSResponse = new XmlDocument();
				try
				{
					USPSResponse.LoadXml(result);
				}
				catch
				{
					shippingMethods.ErrorMsg = "Error: USPS Gateway Did Not Respond";
					return;
				}

				string tempService = string.Empty;
				Decimal tempRate = 0.0M;

				XmlNodeList USPSPackage = USPSResponse.GetElementsByTagName("Postage");

				for(int i = 0; i < USPSPackage.Count; i++)
				{
					XmlNode USPSPostage = USPSPackage.Item(i);
					tempService = USPSPostage["MailService"].InnerText;
					if(ShippingMethodIsAllowed("U.S. Postal " + tempService, UspsName))
					{
						tempRate = Localization.ParseUSDecimal(USPSPostage["Rate"].InnerText);

						//multiply the returned rate by the quantity in the package to avoid calling
						//more than necessary if there were multiple IsShipSeparately items
						//ordered.  If there weren't, PackageQuantity is 1 and the rate is normal
						tempRate = tempRate * PackageQuantity;

						if(MarkupPercent != System.Decimal.Zero)
						{
							tempRate = Decimal.Round(tempRate * (1.00M + (MarkupPercent / 100.0M)), 2, MidpointRounding.AwayFromZero);
						}

						//strip out html encoded characters sent back from USPS
						tempService = StripHtmlAndRemoveSpecialCharacters(HttpUtility.HtmlDecode(tempService));

						decimal vat = Decimal.Round(tempRate * ShippingTaxRate, 2, MidpointRounding.AwayFromZero);

						if(!shippingMethods.MethodExists(tempService))
						{
							var s_method = new ShippingMethod();

							s_method.Carrier = UspsName;
							s_method.Name = tempService;
							s_method.Freight = tempRate;
							s_method.VatRate = vat;
							s_method.IsRealTime = true;
							s_method.Id = Shipping.GetShippingMethodID(s_method.Name);

							if(HasFreeItems)
							{
								s_method.FreeItemsRate = tempRate;
							}

							shippingMethods.Add(s_method);
						}
						else
						{
							int IndexOf = shippingMethods.GetIndex(tempService);
							ShippingMethod s_method = shippingMethods[IndexOf];

							s_method.Freight += tempRate;
							s_method.VatRate += vat;

							if(HasFreeItems)
							{
								s_method.FreeItemsRate += tempRate;
							}

							shippingMethods[IndexOf] = s_method;
						}
					}
					USPSPostage = null;
				}

				USPSPackage = null;
				HasFreeItems = false;
				PackageQuantity = 1;
			}

			// Handling fee should only be added per shipping address not per package
			// let's just compute it here after we've gone through all the packages.
			// Also, since we can't be sure about the ordering of the method call here
			// and that the collection SM includes shipping methods from all possible carriers
			// we'll need to filter out the methods per this carrier to avoid side effects on the main collection
			foreach(ShippingMethod shipMethod in shippingMethods.PerCarrier(UspsName))
			{
				if(shipMethod.Freight != System.Decimal.Zero) //Don't add the fee to free methods.
					shipMethod.Freight += ExtraFee;
			}
		}
		
		private string USPSGetSize(Decimal length, Decimal width, Decimal height)
		{
			string Output = string.Empty;
			Decimal girth = 0;

			if(length >= width && length >= height)
				girth = (height + width) * 2;
			else if(width >= length && width >= height)
				girth = (length + height) * 2;
			else
				girth = (length + width) * 2;

			if(length > 12 || width > 12 || height > 12)
			{
				Output = "<Size>Large</Size>";
				Output += "<Width>" + width.ToString() + "</Width>";
				Output += "<Length>" + length.ToString() + "</Length>";
				Output += "<Height>" + height.ToString() + "</Height>";
				Output += "<Girth>" + girth.ToString() + "</Girth>";
			}
			else
			{

				Output = "<Size>Regular</Size>";
				Output += "<Width>" + width.ToString() + "</Width>";
				Output += "<Length>" + length.ToString() + "</Length>";
				Output += "<Height>" + height.ToString() + "</Height>";
				Output += "<Girth>" + girth.ToString() + "</Girth>";
			}
			return Output;
		}





		/// <summary>
		/// Convert the input number to the textual description of the Service Code
		/// </summary>
		/// <param name="code">The Service Code number to be converted</param>
		/// <returns></returns>
		private string UPSServiceCodeDescription(string code)
		{
			string result = string.Empty;
			switch(code)
			{
				case "01":
					result = "UPS Next Day Air";
					break;
				case "02":
					result = "UPS 2nd Day Air";
					break;
				case "03":
					result = "UPS Ground";
					break;
				case "07":
					result = "UPS Worldwide Express";
					break;
				case "08":
					result = "UPS Worldwide Expedited";
					break;
				case "11":
					result = "UPS Standard";
					break;
				case "12":
					result = "UPS 3-Day Select";
					break;
				case "13":
					result = "UPS Next Day Air Saver";
					break;
				case "14":
					result = "UPS Next Day Air Early AM";
					break;
				case "54":
					result = "UPS Worldwide Express Plus";
					break;
				case "59":
					result = "UPS 2nd Day Air AM";
					break;
				case "65":
					result = "UPS Express Saver";
					break;
			}

			return result;
		}


		/// <summary>
		/// Convert the decimal weight passed in to pounds and ounces
		/// </summary>
		/// <param name="weight">The decimal weight to be convert (in pounds only)</param>
		/// <returns></returns>
		USPSWeight USPSGetWeight(Decimal weight)
		{
			Decimal pounds = 0;
			Decimal ounces = 0;

			pounds = Convert.ToInt32(weight - weight % 1);
			decimal tempWeight = (decimal)weight * 16;
			ounces = Convert.ToInt32(Math.Ceiling((Decimal)tempWeight - (Decimal)pounds * 16.0M));

			USPSWeight w = new USPSWeight();
			w.pounds = Localization.ParseUSInt(pounds.ToString());
			w.ounces = Localization.ParseUSInt(ounces.ToString());

			return w;
		}

		public static bool ShippingMethodIsAllowed(String MethodName, String Carrier)
		{
			string SuperName = StripHtmlAndRemoveSpecialCharacters(HttpUtility.HtmlDecode(MethodName).Replace("<sup>&reg;</sup>", "").Replace("U.S. Postal", "").ToUpperInvariant());
			if(MethodName.Length == 0)
			{
				return true; // not sure how this could happen, but...
			}
			MethodName = MethodName.ToUpperInvariant();
			String tmpS = AppLogic.AppConfig("RTShipping.ShippingMethodsToPrevent").ToUpperInvariant();
			if(tmpS.Length == 0)
			{
				// nothing is prevented
				return true;
			}
			// only allow this method if does not match (exactly) any of the prevented methods:
			foreach(String s in tmpS.Split(','))
			{
				if(s.Trim() == MethodName || s.Trim() == (Carrier.ToUpperInvariant() + " " + MethodName).Trim())
				{
					// restrict on match:
					return false;
				}

				//Lets add some brains for those USPS Intl methods as well.
				if(Carrier.Contains(UspsName))
				{
					SuperName = SuperName.Replace("U.S. POSTAL", "").Trim();

					if(s.Trim() == SuperName || s.Trim() == (Carrier.ToUpperInvariant() + " " + SuperName).Trim() || s.Trim() == ("U.S. POSTAL " + SuperName))
					{
						// restrict on match:
						return false;
					}
				}
			}
			return true;
		}
		public static string StripHtmlAndRemoveSpecialCharacters(string value)
		{
			string returnValue;

			//RS 1/11
			//strip the html from the string
			returnValue = AppLogic.StripHtml(value);

			//for shipping values, there are a number of special characters we want to remove
			//if these are left in the shipping names, they will not match when compared to RTShipping.ShippingMethodsToPrevent
			returnValue = returnValue.Replace("&reg;", "");
			returnValue = returnValue.Replace("&copy;", "");

			return returnValue;
		}

		string MakeCachedRequest(Cache cache, string url, HttpMethod method, string content)
		{
			string cacheKey = null;
			if(cache != null)
			{
				// Hash the content
				var md5 = System.Security.Cryptography.MD5.Create();
				using(var hashDataStream = new MemoryStream())
				{
					using(var hashDataWriter = new StreamWriter(hashDataStream, Encoding.UTF8, 1024, true))
					{
						hashDataWriter.Write(url);
						hashDataWriter.Write(content);
					}

					hashDataStream.Flush();
					hashDataStream.Position = 0;

					cacheKey = string.Format("RTShipping.HTTP.{0}",
						string.Join(
							"-",
							md5
								.ComputeHash(hashDataStream)
								.Select(data => data.ToString("x2"))));
				}

				// See if the cache contains the content
				var cachedResponse = cache.Get(cacheKey) as string;
				if(cachedResponse != null)
					return cachedResponse;
			}

			// Make the request and get the response
			string response;
			if(method == HttpMethod.Post)
				response = MakePostRequest(url, content);
			else if(method == HttpMethod.Get)
				response = MakeGetRequest(url, content);
			else
				throw new NotSupportedException(string.Format("Cannot cache {0} requests", method));

			// Update the cache with the response for 15 minutes
			if(cache != null && cacheKey != null)
				cache.Insert(cacheKey, response, null, DateTime.Now.AddMinutes(15), Cache.NoSlidingExpiration);

			return response;
		}

		string MakePostRequest(string url, string content)
		{
			var request = (HttpWebRequest)WebRequest.Create(url);
			request.Method = "POST";
			request.ContentType = "application/x-www-form-urlencoded";

			var contentData = Encoding.ASCII.GetBytes(content);
			using(var requestStream = request.GetRequestStream())
				requestStream.Write(contentData, 0, contentData.Length);

			using(var response = request.GetResponse())
			using(var responseStream = response.GetResponseStream())
			using(var responseStreamReader = new StreamReader(responseStream))
				return responseStreamReader.ReadToEnd();
		}

		string MakeGetRequest(string url, string content)
		{
			var request = (HttpWebRequest)WebRequest.Create(url + "?" + content);

			using(var response = (HttpWebResponse)request.GetResponse())
			using(var responseStream = response.GetResponseStream())
			using(var responseStreamReader = new StreamReader(responseStream))
				return responseStreamReader.ReadToEnd();
		}

		public enum ResultType  // Enum ResultType: The available return types of the shipment rating(s)
		{
			Unknown = 0,
			PlainText = 1,  // ResultType.PlainText: Specifies the resulting output to be plain text with &lt;BR&gt; tags to separate them
			SingleDropDownList = 2, // ResultType.SingleDropDownList: Specifies the resulting output to be a single line drop down list
			MultiDropDownList = 3,  // ResultType.MultiDropDownList: Specifies the resulting output to be a multi-line combo-box
			RadioButtonList = 4,    // ResultType.RadioButtonList: Specifies the resulting output to be a list of radio buttons with labels
			RawDelimited = 5,   // ResultType.RawDelimited: Specifes the resulting output to be a delimited string. Rates are delimited with a pipe character (|), rate names &amp; prices are delimited with a comma (,)
			DropDownListControl = 6,    // ResultType.DropDownListControl: Specifes the resulting output to be a System.Web.UI.WebControls.DropDownList control.
			CollectionList = 8  // ResultType.CollectionList: Returns the ShippingMethod Collection
		}
		
		public class PickupTypes
		{
			/// <summary>
			/// Specifies the pickup type as: Daily Pickup
			/// </summary>
			public static string UPSDailyPickup
			{
				get { return "01"; }
			}
			/// <summary>
			/// Specifies the pickup type as: Customer Counter
			/// </summary>
			public static string UPSCustomerCounter
			{
				get { return "03"; }
			}
			/// <summary>
			/// Specifies the pickup type as: One time pickup
			/// </summary>
			public static string UPSOneTimePickup
			{
				get { return "06"; }
			}
			/// <summary>
			/// Specifies the pickup type as: On Call Air
			/// </summary>
			public static string UPSOnCallAir
			{
				get { return "07"; }
			}
			/// <summary>
			/// Specifies the pickup type as: Suggested retail rates
			/// </summary>
			public static string UPSSuggestedRetailRates
			{
				get { return "11"; }
			}
			/// <summary>
			/// Specifies the pickup type as: Letter center
			/// </summary>
			public static string UPSLetterCenter
			{
				get { return "19"; }
			}
			/// <summary>
			/// Specifies the pickup type as: Air service center
			/// </summary>
			public static string UPSAirServiceCenter
			{
				get { return "20"; }
			}
		}


		public struct USPSWeight    // Struct USPSWeight: Used to hold shipment weight in pounds and ounces
		{
			public int pounds;  // USPSWeight.pounds: Holds shipment weight in pounds
			public int ounces;  // USPSWeight.pounds: Holds shipment weight in remaining ounces
		}

		/// <summary>
		/// Provides for ability to sort rate list string by rate.
		/// </summary>
		public class ShippingCostsSorter : IComparer<ShippingMethod>
		{
			// Compare cost of shipping.
			public int Compare(ShippingMethod x, ShippingMethod y)
			{
				return x.Freight.CompareTo(y.Freight);
			}
		}

		public class CarrierSorter : IComparer<ShippingMethod>
		{
			public int Compare(ShippingMethod x, ShippingMethod y)
			{
				return x.Carrier.CompareTo(y.Carrier);
			}
		}
	}
}
