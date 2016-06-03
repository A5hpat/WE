// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Data.SqlClient;
using System.Web.Mvc;
using AspDotNetStorefront.Filters;
using AspDotNetStorefrontCore;
using AspDotNetStorefront.Routing;
using AspDotNetStorefrontGateways.Processors;

namespace AspDotNetStorefront.Controllers
{
	[SecureAccessFilter(forceHttps: true)]
	public class PayPalPaymentsAdvancedController : Controller
	{
		public ActionResult Ok()
		{
			var customer = HttpContext.GetCustomer();
			var formString = Request.Form.ToString();

			var postData = string.IsNullOrEmpty(formString)
				? Request.QueryString.ToString()
				: formString;

			//Successful order callback - make the order and send to confirmation
			if(!string.IsNullOrEmpty(postData))
			{
				var parameterDictionary = PayFlowProController.GetParameterStringAsDictionary(postData, true);
				var processor = new PayPalEmbeddedCheckoutCallBackProcessor(parameterDictionary, customer);

				var redirectRoute = processor.ProcessCallBack();
				return new RedirectResult(redirectRoute);
			}

			//Customer ended up here after the order was created - send them to confirmation
			var orderNumber = DB.GetSqlN(
				"select MAX(OrderNumber) N from dbo.orders where CustomerID = @customerId",
				new SqlParameter("customerId", customer.CustomerID));

			var confirmationUrl = Url.Action(
				ActionNames.Confirmation,
				ControllerNames.CheckoutConfirmation,
				new
				{
					orderNumber = orderNumber
				});

			return Redirect(confirmationUrl);
		}

		public ActionResult Error(string respmsg)
		{
			var error = new ErrorMessage(respmsg ?? "There was an error processing your payment. Please contact customer support.");
			
			var checkoutUrl = Url.BuildCheckoutLink(error.MessageId);

			return Redirect(checkoutUrl);
		}
	}
}
