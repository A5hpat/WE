// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Xml.Linq;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontCore.Validation;

namespace AspDotNetStorefront.Validation
{
	public class UspsPostalCodeLookupProvider : IPostalCodeLookupProvider
	{
		readonly AppConfigProvider AppConfigProvider;

		public UspsPostalCodeLookupProvider(AppConfigProvider appConfigProvider)
		{
			AppConfigProvider = appConfigProvider;
		}

		public bool IsEnabled(string country)
		{
			return AppConfigProvider.GetAppConfigValue<bool>("Address.UsePostalCodeLookupService")
				&& !AppLogic.GetCountryIsInternational(country);
		}

		public PostalCodeLookupResult Lookup(string postalCode, string country)
		{
			if(string.IsNullOrWhiteSpace(postalCode))
				return null;

			if(string.IsNullOrWhiteSpace(country))
				return null;

			if(AppLogic.GetCountryIsInternational(country))
				return null;

			try
			{
				var serviceUrl = AppConfigProvider.GetAppConfigValue("Address.PostalCodeLookupService.UspsServiceUrl");
				var userId = AppConfigProvider.GetAppConfigValue("Address.PostalCodeLookupService.UspsUserId");

				var requestDoc = new XDocument(
					new XElement("CityStateLookupRequest",
						new XAttribute("USERID", userId),
						new XElement("ZipCode",
							new XAttribute("ID", 0),
							new XElement("Zip5", postalCode.Substring(0, Math.Min(5, postalCode.Length))))));

				var rdstring = requestDoc.ToString();

				var url = string.Format(
					"{0}?API=CityStateLookup&XML={1}",
					serviceUrl,
					HttpUtility.UrlEncode(requestDoc.ToString()));

				using(var client = new HttpClient())
				{
					var response = client
						.GetAsync(url)
						.Result
						.EnsureSuccessStatusCode()
						.Content
						.ReadAsStringAsync()
						.Result;

					var responseDoc = XDocument.Parse(response);

					var resultElement = responseDoc
						.Elements("CityStateLookupResponse")
						.Elements("ZipCode")
						.FirstOrDefault();

					if(resultElement == null)
						return null;

					if(resultElement.Element("Error") != null)
						return null;

					return new PostalCodeLookupResult(
						city: CultureInfo.InvariantCulture.TextInfo.ToTitleCase(((string)resultElement.Element("City")).ToLower()),
						state: (string)resultElement.Element("State"),
						country: "United States");
				}
			}
			catch(Exception exception)
			{
				SysLog.LogException(exception, MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
				return null;
			}
		}
	}
}
