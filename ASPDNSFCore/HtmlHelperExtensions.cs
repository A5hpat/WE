// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Helpers
{
	public static class HtmlHelperExtensions
	{
		public static MvcHtmlString XmlPackage(this HtmlHelper helper, string packageName, object runtimeParameters = null)
		{
			var customer = HttpContext.Current.GetCustomer();
			var parser = new Parser();
			var runtimeParameterString = BuildRuntimeParameterString(runtimeParameters);
			var xmlpackage = new XmlPackage(
				packageName: packageName,
				customer: customer,
				additionalRuntimeParms: runtimeParameterString,
				htmlHelper: helper);

			var xmlpackageOutput = AppLogic.RunXmlPackage(xmlpackage, parser, customer, customer.SkinID, true, true);
			var htmlOutput = String.Format("<!--Xmlpackage '{0}' -->\n{1}\n<!--End of xmlpackage '{0}' -->", packageName, xmlpackageOutput);
			return MvcHtmlString.Create(htmlOutput);
		}

		static string BuildRuntimeParameterString(object runtimeParameters)
		{
			if(runtimeParameters == null)
				return String.Empty;

			var keyValuePairs = runtimeParameters
				.GetType()
				.GetProperties()
				.Select(property => String.Format("{0}={1}", property.Name, property.GetValue(runtimeParameters, null)));

			return string.Join("&", keyValuePairs);
		}

        public static MvcHtmlString StringResource(this HtmlHelper htmlHelper, string key, string locale = null)
        {
            if (locale == null)
                locale = HttpContext
                    .Current
                    .GetCustomer()
                    .LocaleSetting;

            return MvcHtmlString.Create(AppLogic.GetString(key, locale));
        }

        public static MvcHtmlString StringResourceFormat(this HtmlHelper htmlHelper, string key, params object[] formatItems)
		{
			return MvcHtmlString.Create(string.Format(AppLogic.GetString(key), formatItems));
		}

		public static MvcHtmlString MultilineText(this HtmlHelper htmlHelper, string textToDisplay)
		{
			return string.IsNullOrEmpty(textToDisplay)
				? MvcHtmlString.Empty
				: MvcHtmlString.Create(textToDisplay.Replace("\r\n", "<br/>"));
		}

		public static MvcHtmlString Topic(this HtmlHelper helper, string topicName)
		{
			var customer = HttpContext.Current.GetCustomer();
			var parser = new Parser();
			var topic = new Topic(topicName, customer.LocaleSetting, customer.SkinID, parser);

			return MvcHtmlString.Create(topic.Contents);
		}

		public static MvcHtmlString Token(this HtmlHelper helper, string tokenKey, object parameters = null)
		{

			var parameterDictionary = parameters == null
				? null
				: parameters
					.GetType()
					.GetProperties()
					.Select(property => new
					{
						name = property.Name,
						value = property.GetValue(parameters) ?? string.Empty,
					})
					.ToDictionary(
						o => o.name,
						o => o.value.ToString());

			var customer = HttpContext.Current.GetCustomer();
			var parser = new Parser();
			var tokenOutput = parser.GetTokenValue(tokenKey, parameterDictionary);
			return MvcHtmlString.Create(tokenOutput.ToString());
		}

		public static MvcHtmlString JavascriptString(this HtmlHelper helper, string value)
		{
			return MvcHtmlString.Create(HttpUtility.JavaScriptStringEncode(value, true));
		}

		public static MvcHtmlString JavascriptBool(this HtmlHelper helper, bool value)
		{
			return MvcHtmlString.Create(value ? "true" : "false");
		}

		// This is useful for html attributes such as checked="checked" or disabled="disabled". If you return null in a razor html attribute the attribute gets removed from the html completely.
		public static MvcHtmlString AttributeBool(this HtmlHelper helper, bool attributeEnabled, string trueValue, string falseValue = null)
		{
			if(attributeEnabled || falseValue != null)
				return MvcHtmlString.Create(attributeEnabled == true ? trueValue : falseValue);
			else
				return null;
		}

		//This is a generic way of looping through a collection and running EditorFor on each of them. This is built into the framework's EditorFor method, but the built in EditorFor doesn't work right if you try to specify an editor template with a collection.
		public static MvcHtmlString EditorForMany<TModel, TValue>(
			this HtmlHelper<TModel> html,
			Expression<Func<TModel, IEnumerable<TValue>>> expression,
			string templateName = null,
			string fieldName = null,
			int index = 0)
		{
			var outputString = new StringBuilder();

			// Get the items from ViewData
			var items = expression.Compile()(html.ViewData.Model);
			if(items == null)
				return new MvcHtmlString(string.Empty);

			var appliedFieldName = fieldName ?? ExpressionHelper.GetExpressionText(expression);

			foreach(var item in items)
			{
				// Wrap the item in an object
				var itemWrap = new { Item = item };

				// Get the actual item by accessing the "Item" property from our wrapper class
				var itemProperty = Expression.MakeMemberAccess(Expression.Constant(itemWrap), itemWrap.GetType().GetProperty("Item"));

				// Create a lambda expression for just one item
				var itemExpression = Expression.Lambda<Func<TModel, TValue>>(itemProperty, expression.Parameters);

				// Build the field name up so that model binding can work correctly.
				var itemFieldName = string.Format("{0}[{1}]", appliedFieldName, index++);
				var singleItemHtml = html.EditorFor(itemExpression, templateName, itemFieldName).ToString();
				outputString.AppendLine(singleItemHtml);
			}

			return new MvcHtmlString(outputString.ToString());
		}
	}
}
