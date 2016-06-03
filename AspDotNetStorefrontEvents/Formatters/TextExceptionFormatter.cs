// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Specialized;
using System.Text;
using System.Web;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontEventHandlers.Formatters
{
	class TextExceptionFormatter : IExceptionFormatter
	{
		public string Format(Exception exception, string errorCode)
		{
			if(HttpContext.Current == null)
				return String.Empty;

			var request = HttpContext.Current.Request;
			var customer = HttpContext.Current.GetCustomer();
			var builder = new StringBuilder();

			AppendGroup(builder, "General");
			Append(builder, "Date", DateTime.Now.ToString("G"));
			Append(builder, "Url", request.Url.ToString());

			if(request.UrlReferrer != null)
				Append(builder, "Referrer", request.UrlReferrer.ToString());

			if(customer.IsRegistered)
			{
				AppendGroup(builder, "Customer");
				Append(builder, "Customer ID", customer.CustomerID.ToString());
				Append(builder, "Email", customer.EMail);
				Append(builder, "Phone", customer.Phone);
			}

			AppendGroup(builder, "Exception");
			Append(builder, "ErrorCode", errorCode);
			AppendException(builder, exception);

			AppendGroup(builder, "Query String");
			AppendCollection(builder, request.QueryString);

			AppendGroup(builder, "Cookies");
			foreach(string cookie in request.Cookies.AllKeys)
				Append(builder, cookie, request.Cookies[cookie].Value);

			AppendGroup(builder, "Form");
			AppendCollection(builder, request.Form);

			AppendGroup(builder, "Server Variables");
			AppendCollection(builder, request.ServerVariables);

			return builder.ToString();
		}

		void Append(StringBuilder builder, string name, string value)
		{
			builder.AppendFormat("\n{0,-30}{1}", name + ":", value);
		}

		void AppendGroup(StringBuilder builder, string groupName)
		{
			builder.AppendFormat("\n\n----------- {0} -----------", groupName);
		}

		void AppendCollection(StringBuilder builder, NameValueCollection collection)
		{
			foreach(string name in collection.AllKeys)
				Append(builder, name, collection[name]);
		}

		void AppendException(StringBuilder builder, Exception error)
		{
			Append(builder, "Type", error.GetType().ToString());

			if(error.Message != null)
				Append(builder, "Message", error.Message);

			if(error.Source != null)
				Append(builder, "Source", error.Source);

			if(error.TargetSite != null)
				Append(builder, "TargetSite", error.TargetSite.ToString());

			if(error.StackTrace != null)
				Append(builder, "StackTrace", error.StackTrace);

			if(error.InnerException != null)
			{
				AppendGroup(builder, "Inner Exception");
				AppendException(builder, error.InnerException);
			}
		}
	}
}
