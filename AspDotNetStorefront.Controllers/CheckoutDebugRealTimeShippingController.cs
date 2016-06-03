// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data.SqlClient;
using System.Text;
using System.Web.Mvc;
using AspDotNetStorefront.Caching.ObjectCaching;
using AspDotNetStorefront.Filters;
using AspDotNetStorefront.Models;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Controllers
{
	[SecureAccessFilter(forceHttps: true)]
	public class CheckoutDebugRealTimeShippingController : Controller
	{
		readonly ICachedShoppingCartProvider CachedShoppingCartProvider;

		public CheckoutDebugRealTimeShippingController(ICachedShoppingCartProvider cachedShoppingCartProvider)
		{
			CachedShoppingCartProvider = cachedShoppingCartProvider;
		}

		[HttpGet]
		public ActionResult DebugRealTimeShipping()
		{
			if(!AppLogic.AppConfigBool("RTShipping.DumpDebugXmlOnCheckout"))
				return Content(string.Empty);

			var customer = HttpContext.GetCustomer();
			var cart = CachedShoppingCartProvider.Get(customer, CartTypeEnum.ShoppingCart, AppLogic.StoreID());

			if(Shipping.GetActiveShippingCalculationID() != Shipping.ShippingCalculationEnum.UseRealTimeRates)
				return Content(string.Empty);

			var model = BuildModel(customer);

			if(ControllerContext.IsChildAction)
				return PartialView(model);

			return View(model);
		}

		DebugRealTimeShippingViewModel BuildModel(Customer customer)
		{
			var requestInfo = new StringBuilder();
			var responseInfo = new StringBuilder();

			using(var connection = new SqlConnection(DB.GetDBConn()))
			{
				connection.Open();
				using(var rs = DB.GetRS(
					"Select RTShipRequest, RTShipResponse from customer  with (NOLOCK)  where CustomerID = @CustomerId",
					new[] { new SqlParameter("CustomerId", customer.CustomerID) },
					connection))
				{
					if(rs.Read())
					{
						requestInfo.Append("<DebugRealTimeShippingRequest>");
						requestInfo.Append(DB.RSField(rs, "RTShipRequest"));
						requestInfo.Append("</DebugRealTimeShippingRequest>");

						responseInfo.Append("<DebugRealTimeShippingResponse>");
						responseInfo.Append(DB.RSField(rs, "RTShipResponse"));
						responseInfo.Append("</DebugRealTimeShippingResponse>");
					}
				}
			}

			return new DebugRealTimeShippingViewModel
			{
				RequestInfo = XmlCommon.PrettyPrintXml(ReplaceAccountInfo(requestInfo.ToString())),
				ResponseInfo = XmlCommon.PrettyPrintXml(ReplaceAccountInfo(responseInfo.ToString()))
			};
		}

		string ReplaceAccountInfo(string requestString)
		{
			var replaceString = "REPLACED";

			var carriers = string
				.Join(",",
					AppLogic.AppConfig("RTShipping.ActiveCarrier").Trim(),
					AppLogic.AppConfig("RTShipping.DomesticCarriers").Trim(),
					AppLogic.AppConfig("RTShipping.InternationalCarriers").Trim())
				.ParseAsDelimitedList(',');

			foreach(var carrier in carriers)
			{
				if(carrier.Equals(RTShipping.UpsIdentifier, StringComparison.OrdinalIgnoreCase))
				{
					requestString = requestString.Replace(AppLogic.AppConfig("RTShipping.UPS.License"), replaceString);
					requestString = requestString.Replace(AppLogic.AppConfig("RTShipping.UPS.Password"), replaceString);
				}
				else if(carrier.Equals(RTShipping.Ups2Identifier, StringComparison.OrdinalIgnoreCase))
				{
					requestString = requestString.Replace(AppLogic.AppConfig("RTShipping.UPS.License"), replaceString);
					requestString = requestString.Replace(AppLogic.AppConfig("RTShipping.UPS.Password"), replaceString);
					requestString = requestString.Replace(AppLogic.AppConfig("RTShipping.UPS.AccountNumber"), replaceString);
				}
				else if(carrier.Equals(RTShipping.UspsIdentifier, StringComparison.OrdinalIgnoreCase))
				{
					requestString = requestString.Replace(AppLogic.AppConfig("RTShipping.USPS.UserName"), replaceString);
				}
				else if(carrier.Equals(RTShipping.FedExIdentifier, StringComparison.OrdinalIgnoreCase))
				{
					requestString = requestString.Replace(AppLogic.AppConfig("RTShipping.FedEx.AccountNumber"), replaceString);
					requestString = requestString.Replace(AppLogic.AppConfig("RTSHIPPING.FedEx.Password"), replaceString);
					requestString = requestString.Replace(AppLogic.AppConfig("RTSHIPPING.FedEx.Key"), replaceString);
				}
				else if(carrier.Equals(RTShipping.DhlIdentifier, StringComparison.OrdinalIgnoreCase))
				{
					requestString = requestString.Replace(AppLogic.AppConfig("RTShipping.DHL.AccountNumber"), replaceString);
					requestString = requestString.Replace(AppLogic.AppConfig("RTShipping.DHL.APISystemPassword"), replaceString);
					requestString = requestString.Replace(AppLogic.AppConfig("RTShipping.DHL.APISystemID"), replaceString);
					requestString = requestString.Replace(AppLogic.AppConfig("RTShipping.DHLIntl.BillingAccountNbr"), replaceString);
				}
				else if(carrier.Equals(RTShipping.CanadaPostIdentifier, StringComparison.OrdinalIgnoreCase))
					requestString = requestString.Replace(AppLogic.AppConfig("RTShipping.CanadaPost.MerchantID"), replaceString);
			}

			return requestString;
		}
	}
}
