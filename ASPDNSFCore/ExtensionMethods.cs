// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AspDotNetStorefrontCore
{
	/// <summary>
	/// Helper class containing our extension methods
	/// </summary>
	public static class ExtensionMethods
    {
        #region String Extensions

        private const StringComparison STRING_COMPARISON_RULE = StringComparison.InvariantCultureIgnoreCase;

        /// <summary>
        /// Extension method to do case-insensitive string comparison
        /// </summary>
        /// <param name="str"></param>
        /// <param name="equalTo"></param>
        /// <returns></returns>
        public static bool EqualsIgnoreCase(this string str, string equalTo)
        {
            return str.Equals(equalTo, STRING_COMPARISON_RULE);
        }


        /// <summary>
        /// Extension method to determine whether the beginning of the string matches the specified string in a case-insensitive manner
        /// </summary>
        /// <param name="str"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool StartsWithIgnoreCase(this string str, string value)
        {
            return str.StartsWith(value, STRING_COMPARISON_RULE);
        }

        /// <summary>
        /// Extension method to determine if whether the string contains the specified value in a case-insensitive manner
        /// </summary>
        /// <param name="str"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool ContainsIgnoreCase(this string str, string value)
        {
            return str.IndexOf(value, STRING_COMPARISON_RULE) != -1;
        }

        /// <summary>
        /// Extension method to parse the specified string into a native integer
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int ToNativeInt(this string str)
        {
            return Localization.ParseNativeInt(str);
        }

        /// <summary>
        /// Extension method to parse the specified string into native decimal
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static decimal ToNativeDecimal(this string str)
        {
            return Localization.ParseNativeDecimal(str);
        }

        public static bool ToBool(this string str)
        {
            if (str.Equals("TRUE", StringComparison.InvariantCultureIgnoreCase) ||
                str.Equals("YES", StringComparison.InvariantCultureIgnoreCase) ||
                str.Equals("1", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Extension method to safe-quote the string to use for db queries
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string DBQuote(this string str)
        {
            return DB.SQuote(str);
        }

		public static IEnumerable<string> ParseAsDelimitedList(this string value, char delimiter = ',')
		{
			if(value == null)
				return Enumerable.Empty<string>();

			return value
				.Split(
					new[] { delimiter }, 
					StringSplitOptions.RemoveEmptyEntries)
				.Select(token => token.Trim())
				.Where(token => !string.IsNullOrEmpty(token));
		}

		public static IEnumerable<T> ParseAsDelimitedList<T>(this string value, char delimiter = ',')
		{
			var converter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(T));
			return value
				.ParseAsDelimitedList(delimiter)
				.Select(token => (T)converter.ConvertFrom(token));
		}

		public static string ToDelimitedString<T>(this IEnumerable<T> values, char delimiter = ',')
		{
			var converter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(T));
			return string.Join(
				separator: delimiter.ToString(), 
				values: values
					.Where(value => value != null)
					.Select(value => converter.ConvertToString(value))
					.Select(value => value.Trim())
					.Where(value => !string.IsNullOrEmpty(value)));
		}

        /// <summary>
        /// String.Formats a given string
        /// </summary>
        /// <param name="str"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string FormatWith(this string str, params object[] args)
        {
            return string.Format(str, args);
        }

        /// <summary>
        /// Looks up the value of a string resource with the current customer's SkinID and LocaleSetting
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string StringResource(this string str)
        {
            return AppLogic.GetString(str, Customer.Current.SkinID, Customer.Current.LocaleSetting);
        }

		public static string JoinTo(this string start, string delimiter, params string[] segments)
		{
			return string.Join(
				delimiter ?? string.Empty,
				new[] { start }
					.Concat(segments)
					.Where(s => !string.IsNullOrEmpty(s)));
		}

		/// <summary>
		/// Removes quotes or another character from around a string. Much like Trim(), but only one instance of a character.
		/// </summary>
		/// <param name="input">a string to be unwrapped</param>
		/// <param name="wraps">a list of characters which might wrap this string to be removed.</param>
		/// <returns>The unwrapped string without the given characters around it</returns>
		public static string Unwrap(this string input, params char[] wraps)
		{
			foreach(var wrap in wraps)
				if(input.StartsWith(wrap.ToString()) && input.EndsWith(wrap.ToString()))
					return input.Substring(1, input.Length - 2);

			return input;
		}

		#endregion

		#region Control Extensions

		/// <summary>
		/// Extension method for data-binding operations on a Repeater control to strongly type the current binded item
		/// Sample usage:  Text='&lt;%# Container.DataItemAs&lt;KitItemData&gt;().Name %&gt;'
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="item"></param>
		/// <returns></returns>
		public static T DataItemAs<T>(this RepeaterItem item) where T : class
        {
            return item.DataItem as T;
        }

        /// <summary>
        /// Extension method for data-binding operations on a DataList control to strongly type the current binded item
        /// Sample usage:  Text='&lt;%# Container.DataItemAs&lt;KitItemData&gt;().Name %&gt;'
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public static T DataItemAs<T>(this DataListItem item) where T : class
        {
            return item.DataItem as T;
        }

        /// <summary>
        /// Extension method to automatically define the expected type of the found control by specifying it's generic type
        /// Sample usage: FindControl&lt;TextBox&gt;("txtValue");
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="container"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static T FindControl<T>(this Control container, string id) where T : class
        {
            return container.FindControl(id) as T;
        }

		/// <summary>
		/// Extension method to navigate a control tree and return all controls matching the given ID.
		/// </summary>
		/// <param name="control">The root control to search from.</param>
		/// <param name="id">The ID of the controls to search for.</param>
		/// <returns>A collection of controls matching the provided ID.</returns>
		public static IEnumerable<Control> FindControlRecursive(this Control control, string id, int? maxDepth = null)
		{
			if(maxDepth.HasValue && maxDepth <= 0)
				yield break;

			var result = control.FindControl(id);
			if(result != null)
				yield return result;

			foreach(Control childControl in control.Controls)
				foreach(var match in FindControlRecursive(childControl, id, maxDepth.HasValue ? maxDepth - 1 : null))
					yield return match;
		}

        /// <summary>
        /// Extension method to assign css class for a asp.net control based on a condition
        /// </summary>
        /// <param name="container"></param>
        /// <param name="condition"></param>
        /// <param name="className"></param>
        /// <returns></returns>
        public static string CssClassIf(this Control container, bool condition, string className)
        {
            if (condition == true)
            {
                return className;
            }

            return string.Empty;
        }
		
        /// <summary>
        /// Extension method to assign css class for a asp.net control based on a condition
        /// </summary>
        /// <param name="container"></param>
        /// <param name="condition"></param>
        /// <param name="className"></param>
        /// <returns></returns>
        public static string CssClassIfInvalid(this Control container, IValidable item, string propertyName, string errorStyle)
        {
            if (!item.IsValid())
            {
                // find the matching property
                ValidationError valError = item.ValidationErrors().Find(vError => vError.PropertyName.EqualsIgnoreCase(propertyName));
                if (valError != null)
                {
                    return errorStyle;
                }
            }

            return string.Empty;
        }

		public static TListControl AddItems<TListControl>(this TListControl listControl, IEnumerable<ListItem> items)
			where TListControl : ListControl
		{
			if(listControl == null)
				return listControl;

			foreach(var item in items ?? Enumerable.Empty<ListItem>())
				listControl.Items.Add(item);

			return listControl;
		}

		public static TListControl SelectFirstByText<TListControl>(this TListControl listControl, string text, IEqualityComparer<string> equalityComparer = null)
			where TListControl : ListControl
		{
			equalityComparer = equalityComparer ?? EqualityComparer<string>.Default;
			return SelectFirst(listControl, item => equalityComparer.Equals(item.Text, text));
		}

		public static TListControl SelectFirstByValue<TListControl>(this TListControl listControl, string value, IEqualityComparer<string> equalityComparer = null)
			where TListControl : ListControl
		{
			equalityComparer = equalityComparer ?? EqualityComparer<string>.Default;
			return SelectFirst(listControl, item => equalityComparer.Equals(item.Value, value));
		}

		public static TListControl SelectFirst<TListControl>(this TListControl listControl, Func<ListItem, bool> selectionPredicate)
			where TListControl : ListControl
		{
			if(listControl == null)
				return listControl;

			// Find zero or one matching selected items. Ensure all other items are unselected.
			var selectionFound = false;
			foreach(ListItem item in listControl.Items)
				if(!selectionFound)
					selectionFound
						= item.Selected
						= selectionPredicate(item);
				else
					item.Selected = false;

			return listControl;
		}
		
		#endregion

        #region HttpRequest Extension Methods

        /// <summary>
        /// Extension method to get the querystring value as integer from the HttpRequest object
        /// </summary>
        /// <param name="req"></param>
        /// <param name="paramName"></param>
        /// <returns></returns>
        public static int QueryStringNativeInt(this HttpRequest req, string paramName)
        {
            return CommonLogic.QueryStringNativeInt(paramName);
        }

        #endregion

        #region IDataReader Extension Methods

        public static string Field(this IDataReader rs, string fieldName)
        {
            return DB.RSField(rs, fieldName);
        }

        public static int FieldInt(this IDataReader rs, string fieldName)
        {
            return DB.RSFieldInt(rs, fieldName);
        }

        public static bool FieldBool(this IDataReader rs, string fieldName)
        {
            return DB.RSFieldBool(rs, fieldName);
        }

        public static string FieldByLocale(this IDataReader rs, string fieldName, string locale)
        {
            return DB.RSFieldByLocale(rs, fieldName, locale);
        }

        public static DateTime FieldDateTime(this IDataReader rs, string fieldName)
        {
            return DB.RSFieldDateTime(rs, fieldName);
        }

        public static Decimal FieldDecimal(this IDataReader rs, string fieldName)
        {
            return DB.RSFieldDecimal(rs, fieldName);
        }

        public static Guid FieldGuid(this IDataReader rs, string fieldName)
        {
            return DB.RSFieldGUID2(rs, fieldName);
        }

        #endregion

		#region DbParameter Extensions

		public static T Value<T>(this System.Data.Common.DbParameter dbParameter)
		{
			if(dbParameter == null)
				throw new ArgumentNullException("dbParameter");

			var value = dbParameter.Value;

			if(value == null || value is DBNull)
				return default(T);

			return (T)value;
		}

		#endregion
	}

	public static class ByteEnumerableExtensions
	{
		public static string ToString(this IEnumerable<byte> data, string delimiter, bool uppercase = false)
		{
			if(data == null)
				return string.Empty;

			var formatString = uppercase
				? "x2"
				: "X2";

			return string.Join(
				delimiter,
				data.Select(b => b.ToString(formatString)));
		}
	}

	public static class HttpContextExtensions
	{
		const string CustomerItemKeyFormat = "adnsf:requestCustomer-{0}";

		public static Customer GetCustomer(this HttpContext httpContext)
		{
			return GetCustomer(new HttpContextWrapper(httpContext));
		}

		public static Customer GetCustomer(this HttpContextBase httpContext)
		{
			if(httpContext == null)
				throw new ArgumentNullException("httpContext");

			Guid? customerGuid;
			if(httpContext.User != null)
			{
				if(!(httpContext.User is System.Security.Claims.ClaimsPrincipal))
					throw new InvalidOperationException("Principal must be a ClaimsPrincipal");

				var claimsPrincipal = (System.Security.Claims.ClaimsPrincipal)httpContext.User;
				customerGuid = claimsPrincipal.GetCustomerGuid();
			}
			else
				customerGuid = null;
			
			var customerItemKey = string.Format(CustomerItemKeyFormat, customerGuid);
			if(httpContext.Items.Keys.OfType<string>().Contains(customerItemKey))
			{
				var cachedCustomer = (Customer)httpContext.Items[customerItemKey];
				if(cachedCustomer != null)
					return cachedCustomer;
			}

			var isAdminSite = AppLogic.IsAdminSite;
			var newCustomer = customerGuid == null
				? new Customer(isAdminSite)
				: new Customer(customerGuid.Value, isAdminSite);

			httpContext.Items[customerItemKey] = newCustomer;
			
			return newCustomer;
		}
	}

	public static class ClaimsPrincipalExtensions
	{
		public static Guid? GetCustomerGuid(this System.Security.Claims.ClaimsPrincipal claimsPrincipal)
		{
			// Parse customer GUID out of the identity's claims.
			var customerGuidClaim = claimsPrincipal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
			if(customerGuidClaim == null)
				return null;
				
			if(String.IsNullOrEmpty(customerGuidClaim.Value))
				return null;

			Guid parsedCustomerGuid;
			if(!Guid.TryParse(customerGuidClaim.Value, out parsedCustomerGuid))
				return null;
			
			return parsedCustomerGuid;
		}
	}
}
