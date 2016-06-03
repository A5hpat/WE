// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Text;
using System.Xml;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Validation.AddressValidator
{
	public class UspsAddressValidationProvider : IAddressValidationProvider
	{
		public AddressValidationResult Validate(Address address, AddressTypes addressType = AddressTypes.Unknown)
		{
			if(address.Country != "United States") // USPS can't verify non-US addresses
				return new AddressValidationResult();

			var result = AppLogic.ro_OK;
			var correctedAddress = new Address();
			correctedAddress.LoadFromDB(address.AddressID);

			var requestDoc = new StringBuilder();
			requestDoc.AppendFormat("API=Verify&Xml=<AddressValidateRequest USERID=\"{0}\">", AppLogic.AppConfig("VerifyAddressesProvider.USPS.UserID"));
			requestDoc.Append("<Address ID=\"0\">");
			requestDoc.AppendFormat("<Address1>{0}</Address1>", address.Address2);
			requestDoc.AppendFormat("<Address2>{0}</Address2>", address.Address1);
			requestDoc.AppendFormat("<City>{0}</City>", address.City);
			requestDoc.AppendFormat("<State>{0}</State>", address.State);
			requestDoc.AppendFormat("<Zip5>{0}</Zip5>", address.Zip);
			requestDoc.Append("<Zip4></Zip4>");
			requestDoc.Append("</Address>");
			requestDoc.Append("</AddressValidateRequest>");
			;

			// Send request & capture response
			// Possible USPS server values:
			//  http://production.shippingapis.com/shippingapi.dll 
			//  http://testing.shippingapis.com/ShippingAPITest.dll
			var received = XmlCommon.GETandReceiveData(requestDoc.ToString(), AppLogic.AppConfig("VerifyAddressesProvider.USPS.Server"));

			var response = new XmlDocument();
			try
			{
				response.LoadXml(received);
			}
			catch
			{
				return new AddressValidationResult(); // we don't want to bug the customer if the server did not respond.
			}

			// Check for error response from server
			var errors = response.GetElementsByTagName("Error");
			if(errors.Count > 0) // Error has occurred
			{
				var error = response.GetElementsByTagName("Error");
				var errorMessage = error.Item(0);

				result = AppLogic.GetString("uspsValidate" + errorMessage["Number"].InnerText, address.SkinID, address.LocaleSetting);
				if(result == "uspsValidate" + errorMessage["Number"].InnerText)
				{
					// Use the USPS Error Description for error messages we don't have String values for.
					result = string.Format("Address Verification Error: {0}", errorMessage["Description"].InnerText);
				}

				return new AddressValidationResult(
					status: AddressValidationStatus.Failure,
					message: result);
			}

			var addresses = response.GetElementsByTagName("Address");
			if(addresses.Count > 0)
			{
				var uspsAddress = addresses.Item(0);
				// our address1 is their address2
				correctedAddress.Address1 = uspsAddress["Address2"].InnerText;
				correctedAddress.Address2 = uspsAddress["Address1"] != null ? uspsAddress["Address1"].InnerText : String.Empty;
				correctedAddress.City = uspsAddress["City"].InnerText;
				correctedAddress.State = uspsAddress["State"].InnerText;
				correctedAddress.Zip = uspsAddress["Zip5"].InnerText;
				// add Zip+4 if it exists
				correctedAddress.Zip += uspsAddress["Zip4"].InnerText.Length == 4 ? "-" + uspsAddress["Zip4"].InnerText : String.Empty;
				correctedAddress.FirstName = correctedAddress.FirstName.ToUpperInvariant();
				correctedAddress.LastName = correctedAddress.LastName.ToUpperInvariant();
				correctedAddress.Company = correctedAddress.Company.ToUpperInvariant();
				result = "Your shipping address has been formatted and validated according to US Postal Service standards. Please verify that it is correct.";

				// Does the resulting address matches the entered address?
				bool IsMatch = true;
				if(correctedAddress.Address1 != address.Address1)
					IsMatch = false;
				else if(correctedAddress.Address2 != address.Address2)
					IsMatch = false;
				else if(correctedAddress.City != address.City)
					IsMatch = false;
				else if(correctedAddress.State != address.State)
					IsMatch = false;
				//USPS is USA only - no check for country
				else if(correctedAddress.Zip != address.Zip)
					IsMatch = false;

				// If the resulting address matches the entered address, then return ro_OK
				if(IsMatch)
					return new AddressValidationResult();
			}
			else
			{ // unknown response from server
				return new AddressValidationResult(); // we don't want to bug the customer if we don't recognize the response.
			}

			return new AddressValidationResult(
				status: AddressValidationStatus.Failure,
				message: result,
				correctedAddresses: new[]
					{
						correctedAddress
					});
		}
	}
}
