// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Caching;
using System.Web.Services.Protocols;
using System.Xml.Serialization;
using AspDotNetStorefrontCore.RateServiceWebReference;
using AspDotNetStorefrontCore.ShippingCalculation;

namespace AspDotNetStorefrontCore
{
	public partial class RTShipping
    {
        private RateRequest CreateRateRequest(Packages Shipment)
        {
            // Build the RateRequest
            RateRequest request = new RateRequest();
            
            //
            request.WebAuthenticationDetail = new WebAuthenticationDetail();
            request.WebAuthenticationDetail.UserCredential = new WebAuthenticationCredential();
            request.WebAuthenticationDetail.UserCredential.Key = FedexKey; // Replace "XXX" with the Key
            request.WebAuthenticationDetail.UserCredential.Password = FedexPassword; // Replace "XXX" with the Password
            //
            request.ClientDetail = new ClientDetail();
            request.ClientDetail.AccountNumber = FedexAccountNumber; // Replace "XXX" with client's account number
            request.ClientDetail.MeterNumber = FedexMeter; // Replace "XXX" with client's meter number
            //
            request.TransactionDetail = new TransactionDetail();
            request.TransactionDetail.CustomerTransactionId = ""; // This is a reference field for the customer.  Any value can be used and will be provided in the response.
            //
            request.Version = new VersionId(); // WSDL version information, value is automatically set from wsdl
            //
            request.ReturnTransitAndCommit = true;
            request.ReturnTransitAndCommitSpecified = true;
            //
            SetShipmentDetails(request);
            //
            SetOrigin(request);
            //
            SetDestination(request, Shipment);
            //
            SetPayment(request);
            //
            SetPackageLineItems(request, Shipment);

			bool includeSmartPost = AppLogic.AppConfigBool("RTShipping.FedEx.SmartPost.Enabled"); ;
			if (includeSmartPost)
			{
				SetSmartPostDetails(request, Shipment);
			}
            return request;
        }

		private SmartPostIndiciaType GetSmartPostIndiciaType(decimal weight)
		{
			SmartPostIndiciaType retType = SmartPostIndiciaType.PARCEL_SELECT;
			//0-1:PRESORTED STANDARD,1.01-69.99:PARCEL SELECT
			string indiciaString = AppLogic.AppConfig("RTShipping.FedEx.SmartPost.IndiciaWeights");
			string[] weightBreaks = indiciaString.Split(',');
			foreach (string weightBreak in weightBreaks)
			{
				string[] splitWeight = weightBreak.Split(':');
				if (splitWeight.Length == 2)
				{
					decimal weightLow = decimal.Zero;
					decimal weightHigh = decimal.Zero;
					string[] weightRanges = splitWeight[0].Split('-');
					if (weightRanges.Length == 2)
					{
						if (decimal.TryParse(weightRanges[0], out weightLow) &&
							decimal.TryParse(weightRanges[1], out weightHigh))
						{
							if (weight >= weightLow && weight <= weightHigh)
							{
								retType = (SmartPostIndiciaType)Enum.Parse(typeof(SmartPostIndiciaType), splitWeight[1]);
							}
						}
					}
				}
			}

			return retType;
		}

		private SmartPostAncillaryEndorsementType? GetSmartPostAncillaryEndorsement()
		{
			string endorsement = AppLogic.AppConfig("RTShipping.FedEx.SmartPost.AncillaryEndorsementType");
			switch (endorsement.ToUpper())
			{
				case "ADDRESS CORRECTION": return SmartPostAncillaryEndorsementType.ADDRESS_CORRECTION;
				case "CARRIER LEAVE IF NO RESPONSE": return SmartPostAncillaryEndorsementType.CARRIER_LEAVE_IF_NO_RESPONSE;
				case "CHANGE SERVICE": return SmartPostAncillaryEndorsementType.CHANGE_SERVICE;
				case "FORWARDING SERVICE": return SmartPostAncillaryEndorsementType.FORWARDING_SERVICE;
				case "RETURN SERVICE": return SmartPostAncillaryEndorsementType.RETURN_SERVICE;
			}
			return null;
		}

		private string GetSmartPostHubId()
		{
			#region HubIds
			//            5303 ATGA Atlanta
			//• 	5281 CHNC Charlotte
			//• 	5602 CIIL Chicago
			//• 	5929 COCA Chino
			//• 	5751 DLTX Dallas
			//• 	5802 DNCO Denver
			//• 	5481 DTMI Detroit
			//• 	5087 EDNJ Edison
			//• 	5431 GCOH Grove City
			//• 	5771 HOTX Houston
			//• 	5465 ININ Indianapolis
			//• 	5648 KCKS Kansas City
			//• 	5902 LACA Los Angeles
			//• 	5254 MAWV Martinsburg
			//• 	5379 METN Memphis
			//• 	5552 MPMN Minneapolis
			//• 	5531 NBWI New Berlin
			//• 	5110 NENY Newburgh
			//• 	5015 NOMA Northborough
			//• 	5327 ORFL Orlando
			//• 	5194 PHPA Philadelphia
			//• 	5854 PHAZ Phoenix
			//• 	5150 PTPA Pittsburgh
			//• 	5958 SACA Sacramento
			//• 	5843 SCUT Salt Lake City
			//• 	5983 SEWA Seattle
			//• 	5631 STMO St. Louis
			#endregion
			return AppLogic.AppConfig("RTShipping.FedEx.SmartPost.HubId");
		}

		private void SetSmartPostDetails(RateRequest request, Packages shipment)
		{
			request.RequestedShipment.SmartPostDetail = new SmartPostShipmentDetail()
			{
				Indicia = GetSmartPostIndiciaType(shipment.Weight),
				IndiciaSpecified = true,
				HubId = GetSmartPostHubId(),
				AncillaryEndorsementSpecified = false,
			};

			var ancillary = GetSmartPostAncillaryEndorsement();
			if (ancillary.HasValue)
			{
				request.RequestedShipment.SmartPostDetail.AncillaryEndorsement = ancillary.Value;
				request.RequestedShipment.SmartPostDetail.AncillaryEndorsementSpecified = true;				 
			}	
		}

        private void SetShipmentDetails(RateRequest request)
        {
            request.RequestedShipment = new RequestedShipment();
            request.RequestedShipment.ShipTimestamp = DateTime.Now; // Shipping date and time
            request.RequestedShipment.ShipTimestampSpecified = true;
            request.RequestedShipment.DropoffType = DropoffType.REGULAR_PICKUP; //Drop off types are BUSINESS_SERVICE_CENTER, DROP_BOX, REGULAR_PICKUP, REQUEST_COURIER, STATION
            request.RequestedShipment.DropoffTypeSpecified = true;
            request.RequestedShipment.PackagingType = PackagingType.YOUR_PACKAGING;
            request.RequestedShipment.PackagingTypeSpecified = true;
            //
            request.RequestedShipment.RateRequestTypes = new RateRequestType[2];
            request.RequestedShipment.RateRequestTypes[0] = RateRequestType.ACCOUNT;
            request.RequestedShipment.RateRequestTypes[1] = RateRequestType.LIST;
            request.RequestedShipment.PackageDetail = RequestedPackageDetailType.INDIVIDUAL_PACKAGES;
            request.RequestedShipment.PackageDetailSpecified = true;
        }

        private void SetOrigin(RateRequest request)
        {
            request.RequestedShipment.Shipper = new Party();
            request.RequestedShipment.Shipper.Address = new AspDotNetStorefrontCore.RateServiceWebReference.Address();
            request.RequestedShipment.Shipper.Address.StreetLines = new string[1] { OriginAddress };
            request.RequestedShipment.Shipper.Address.City = OriginCity;
            request.RequestedShipment.Shipper.Address.StateOrProvinceCode = OriginStateProvince;
            request.RequestedShipment.Shipper.Address.PostalCode = OriginZipPostalCode;
            request.RequestedShipment.Shipper.Address.CountryCode = OriginCountry;
        }

        private void SetDestination(RateRequest request, Packages Shipment)
        {
            request.RequestedShipment.Recipient = new Party();
            request.RequestedShipment.Recipient.Address = new AspDotNetStorefrontCore.RateServiceWebReference.Address();
            if (Shipment.DestinationStateProvince.Replace("-", "").Length != 0 && Shipment.DestinationStateProvince != "ZZ")
            {
                request.RequestedShipment.Recipient.Address.StreetLines = new string[1] { Shipment.DestinationAddress1 };
                request.RequestedShipment.Recipient.Address.City = Shipment.DestinationCity;
                request.RequestedShipment.Recipient.Address.StateOrProvinceCode = Shipment.DestinationStateProvince;
            }
            if (Shipment.DestinationCountryCode != String.Empty)
                request.RequestedShipment.Recipient.Address.CountryCode = Shipment.DestinationCountryCode;
            else
                request.RequestedShipment.Recipient.Address.CountryCode = OriginCountry;

            if (Shipment.DestinationCountryCode.Equals("US", StringComparison.InvariantCultureIgnoreCase) && Shipment.DestinationZipPostalCode.Length > 5)
                request.RequestedShipment.Recipient.Address.PostalCode = Shipment.DestinationZipPostalCode.Substring(0, 5);
            else
                request.RequestedShipment.Recipient.Address.PostalCode = Shipment.DestinationZipPostalCode;

			if (Shipment.DestinationResidenceType == ResidenceTypes.Residential)
			{
				request.RequestedShipment.Recipient.Address.Residential = true;
				request.RequestedShipment.Recipient.Address.ResidentialSpecified = true;
			}
        }

        private void SetPayment(RateRequest request)
        {
            request.RequestedShipment.ShippingChargesPayment = new Payment();
            request.RequestedShipment.ShippingChargesPayment.PaymentType = PaymentType.SENDER; // Payment options are RECIPIENT, SENDER, THIRD_PARTY
            request.RequestedShipment.ShippingChargesPayment.PaymentTypeSpecified = true;
            request.RequestedShipment.ShippingChargesPayment.Payor = new Payor();
            request.RequestedShipment.ShippingChargesPayment.Payor.AccountNumber = FedexAccountNumber; // Replace "XXX" with client's account number
            request.RequestedShipment.ShippingChargesPayment.Payor.CountryCode = OriginCountry;
        }

        private void SetPackageLineItems(RateRequest request, Packages Shipment)
        {

            // ------------------------------------------
            // Passing individual pieces rate request
            // ------------------------------------------
            request.RequestedShipment.RequestedPackageLineItems = new RequestedPackageLineItem[Shipment.Count];
            int packagecount = 0;
            foreach (Package package in Shipment)
            {
                //
                request.RequestedShipment.RequestedPackageLineItems[packagecount] = new RequestedPackageLineItem();
                request.RequestedShipment.RequestedPackageLineItems[packagecount].SequenceNumber = (packagecount + 1).ToString(); // package sequence number
                //
                request.RequestedShipment.RequestedPackageLineItems[packagecount].Weight = new Weight(); // package weight
                request.RequestedShipment.RequestedPackageLineItems[packagecount].Dimensions = new Dimensions(); // package dimensions

                if (AppLogic.AppConfig("RTShipping.WeightUnits").Trim().Equals("LBS", StringComparison.InvariantCultureIgnoreCase))
                {
                    request.RequestedShipment.RequestedPackageLineItems[packagecount].Weight.Units = WeightUnits.LB;
                    request.RequestedShipment.RequestedPackageLineItems[packagecount].Dimensions.Units = LinearUnits.IN;
                }
                else
                {
                    request.RequestedShipment.RequestedPackageLineItems[packagecount].Weight.Units = WeightUnits.KG;
                    request.RequestedShipment.RequestedPackageLineItems[packagecount].Dimensions.Units = LinearUnits.CM;
                }

                request.RequestedShipment.RequestedPackageLineItems[packagecount].Weight.Value = package.Weight;
                request.RequestedShipment.RequestedPackageLineItems[packagecount].Dimensions.Length = Math.Round(package.Length, 0, MidpointRounding.AwayFromZero).ToString();
                request.RequestedShipment.RequestedPackageLineItems[packagecount].Dimensions.Width = Math.Round(package.Width, 0, MidpointRounding.AwayFromZero).ToString();
                request.RequestedShipment.RequestedPackageLineItems[packagecount].Dimensions.Height = Math.Round(package.Height, 0, MidpointRounding.AwayFromZero).ToString();

                if (package.Insured)
                {
                    request.RequestedShipment.RequestedPackageLineItems[packagecount].InsuredValue = new Money(); // insured value
                    request.RequestedShipment.RequestedPackageLineItems[packagecount].InsuredValue.Amount = package.InsuredValue;
					request.RequestedShipment.RequestedPackageLineItems[packagecount].InsuredValue.AmountSpecified = true;
                    request.RequestedShipment.RequestedPackageLineItems[packagecount].InsuredValue.Currency = "USD";
                }
                packagecount += 1;
            }
            request.RequestedShipment.PackageCount = packagecount.ToString();
        }
		
		public string FedExGetCodeDescription(string code)
		{
			switch(code.Replace("_", ""))
			{
				case "PRIORITYOVERNIGHT":
					return "Priority";

				case "FEDEX2DAY":
					return "2nd Day";

				case "STANDARDOVERNIGHT":
					return "Standard Overnight";

				case "FIRSTOVERNIGHT":
					return "First Overnight";

				case "FEDEXEXPRESSSAVER":
					return "Express Saver";

				case "FEDEX1DAYFREIGHT":
					return "Overnight Freight";

				case "FEDEX2DAYFREIGHT":
					return "2nd Day Freight";

				case "FEDEX3DAYFREIGHT":
					return "Express Saver Freight";

				case "GROUNDHOMEDELIVERY":
					return "Home Delivery";

				case "FEDEXGROUND":
					return "Ground Service";

				case "INTERNATIONALPRIORITY":
					return "International Priority";

				case "INTERNATIONALECONOMY":
					return "International Economy";

				case "INTERNATIONALPRIORITYFREIGHT":
					return "International Priority Freight";

				case "SMARTPOST":
					return "Smart Post";

				default:
					return string.Empty;
			}
		}

		void FedExGetRates(Shipments shipments, out string rtShipRequest, out string rtShipResponse, decimal extraFee, decimal markupPercent, decimal shippingTaxRate, Cache cache, ref ShippingMethodCollection shippingMethods)
		{
			rtShipRequest = string.Empty;
			rtShipResponse = string.Empty;

			var maxWeight = AppLogic.AppConfigUSDecimal("RTShipping.Fedex.MaxWeight");
			if(maxWeight == 0)
				maxWeight = 150;

			if(ShipmentWeight > maxWeight)
			{
				shippingMethods.ErrorMsg = "FedEx " + AppLogic.AppConfig("RTShipping.CallForShippingPrompt");
				return;
			}

			foreach(var shipment in shipments)
			{
				HasFreeItems = shipments
					.SelectMany(packages => packages)
					.Where(package => package.IsFreeShipping)
					.Any();

				var rateRequest = CreateRateRequest(shipment);
				rtShipRequest = SerializeToString(rateRequest);

				string cacheKey = null;
				RateReply rateReply = null;
				if(cache != null)
				{
					// Hash the content
					var md5 = System.Security.Cryptography.MD5.Create();
					using(var hashDataStream = new MemoryStream())
					{
						using(var hashDataWriter = new StreamWriter(hashDataStream, Encoding.UTF8, 1024, true))
							hashDataWriter.Write(StripShipTimestampNode(rtShipRequest));

						hashDataStream.Flush();
						hashDataStream.Position = 0;

						cacheKey = string.Format("RTShipping.FedEx.{0}", 
							string.Join(
								"-",
								md5
									.ComputeHash(hashDataStream)
									.Select(data => data.ToString("x2"))));
					}

					// See if the cache contains the content
					var cachedResponse = cache.Get(cacheKey) as RateReply;
					if(cachedResponse != null)
						rateReply = cachedResponse;
				}

				try
				{
					if(rateReply == null)
					{
						var rateService = new RateService
						{
							Url = FedexServer,
						};

						rateReply = rateService.getRates(rateRequest);
						rtShipResponse = SerializeToString(rateReply);

						if(cache != null && cacheKey != null)
							cache.Insert(cacheKey, rateReply, null, DateTime.Now.AddMinutes(15), Cache.NoSlidingExpiration);
					}

					if(rateReply.RateReplyDetails == null
						|| (rateReply.HighestSeverity != NotificationSeverityType.SUCCESS
							&& rateReply.HighestSeverity != NotificationSeverityType.NOTE
							&& rateReply.HighestSeverity != NotificationSeverityType.WARNING))
					{
						rtShipResponse = "Error: Call Not Successful " + rateReply.Notifications[0].Message;
						return;
					}

					// Create a list of available services
					foreach(var rateReplyDetail in rateReply.RateReplyDetails)
					{
						var ratedShipmentDetail = rateReplyDetail.RatedShipmentDetails
							.Where(rsd => rsd.ShipmentRateDetail.RateType == ReturnedRateType.PAYOR_ACCOUNT_PACKAGE
								|| rsd.ShipmentRateDetail.RateType == ReturnedRateType.PAYOR_ACCOUNT_SHIPMENT
								|| rsd.ShipmentRateDetail.RateType == ReturnedRateType.RATED_ACCOUNT_PACKAGE
								|| rsd.ShipmentRateDetail.RateType == ReturnedRateType.RATED_ACCOUNT_SHIPMENT)
							.FirstOrDefault();

						var rateName = string.Format("FedEx {0}", FedExGetCodeDescription(rateReplyDetail.ServiceType.ToString()));

						if(!ShippingMethodIsAllowed(rateName, FedExName))
							continue;

						var totalCharges = ratedShipmentDetail.ShipmentRateDetail.TotalNetCharge.Amount;

						// Multiply the returned rate by the quantity in the package to avoid calling
						// more than necessary if there were multiple IsShipSeparately items
						// ordered.  If there weren't, Shipment.PackageCount is 1 and the rate is normal.
						totalCharges = totalCharges * shipment.PackageCount;

						if(markupPercent != 0)
							totalCharges = decimal.Round(totalCharges * (1 + (markupPercent / 100m)), 2, MidpointRounding.AwayFromZero);

						var vat = decimal.Round(totalCharges * shippingTaxRate, 2, MidpointRounding.AwayFromZero);

						if(!shippingMethods.MethodExists(rateName))
						{
							var shippingMethod = new ShippingMethod
							{
								Carrier = FedExName,
								Name = rateName,
								Freight = totalCharges,
								VatRate = vat,
								IsRealTime = true,
								Id = Shipping.GetShippingMethodID(rateName),
							};

							if(HasFreeItems)
								shippingMethod.FreeItemsRate = totalCharges;

							shippingMethods.Add(shippingMethod);
						}
						else
						{
							var shippingMethodIndex = shippingMethods.GetIndex(rateName);
							var shippingMethod = shippingMethods[shippingMethodIndex];

							shippingMethod.Freight += totalCharges;
							shippingMethod.VatRate += vat;

							if(HasFreeItems)
								shippingMethod.FreeItemsRate += totalCharges;

							shippingMethods[shippingMethodIndex] = shippingMethod;
						}
					}

					// Handling fee should only be added per shipping address not per package
					// let's just compute it here after we've gone through all the packages.
					// Also, since we can't be sure about the ordering of the method call here
					// and that the collection SM includes shipping methods from all possible carriers
					// we'll need to filter out the methods per this carrier to avoid side effects on the main collection
					foreach(var shippingMethod in shippingMethods.PerCarrier(FedExName))
						if(shippingMethod.Freight != 0) //Don't add the fee to free methods.
							shippingMethod.Freight += extraFee;
				}
				catch(SoapException exception)
				{
					rtShipResponse = "FedEx Error: " + exception.Detail.InnerXml;
				}
				catch(Exception exception)
				{
					while(exception.InnerException != null)
						exception = exception.InnerException;

					rtShipResponse = "FedEx Error: " + exception.Message;
				}
			}
		}

		string SerializeToString(object value)
		{
			if(value == null)
				return null;

			var serializer = new XmlSerializer(value.GetType());
			using(var writer = new StringWriter())
			{
				serializer.Serialize(writer, value);
				return writer.ToString();
			}
		}

		string StripShipTimestampNode(string rateRequestDoc)
		{
			return System.Text.RegularExpressions.Regex.Replace(rateRequestDoc, @"<ShipTimestamp.+</ShipTimestamp>", string.Empty);
		}
    }
}
