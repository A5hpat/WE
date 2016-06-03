// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AspDotNetStorefrontControls.Listing
{
	public interface IGridLayoutControl
	{
		int GridColumns { get; }
	}

	public abstract class FilterControl : UserControl, IGridLayoutControl
	{
		[TypeConverter(typeof(StringArrayConverter))]
		public virtual string[] QueryStringNames
		{ get; set; }

		[TypeConverter(typeof(StringArrayConverter))]
		public virtual string[] SqlParameterNames
		{ get; set; }

		public virtual int GridColumns
		{ get { return 3; } }

		protected FilterControlContext FilterControlContext
		{ get; private set; }

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			ApplyQueryStringValues();
		}

		protected override void Render(HtmlTextWriter writer)
		{
			writer.WriteBeginTag("div");
			writer.WriteAttribute("class", String.Format("col-md-{0}", GridColumns));
			writer.Write(HtmlTextWriter.TagRightChar);

			base.Render(writer);

			writer.WriteEndTag("div");
		}

		public void SetContext(FilterControlContext filterControlContext)
		{
			FilterControlContext = filterControlContext;
		}

		protected virtual void ApplyQueryStringValues()
		{
			// Merge the "&key=" together with the "&key" query string params (because asp.net handles them differently).
			var queryParams = Request.QueryString.AllKeys
				.Where(key => key != null)
				.Select(key => new KeyValuePair<string, string>(key, Request.QueryString[key]))
				.Concat(
					(Request.QueryString[null] ?? String.Empty)
						.Split(',')
						.Select(key => new KeyValuePair<string, string>(key, String.Empty)));

			// Find the query string params that match filter names
			var filterParams = GenerateFilterNames()
				.Select(filterName => queryParams
					.Where(kvp => kvp.Key == filterName)
					.DefaultIfEmpty(new KeyValuePair<string, string>(filterName, null))
					.FirstOrDefault());

			SetValues(filterParams);
		}

		protected IEnumerable<string> GenerateFilterNames()
		{
			var queryStringNames = (QueryStringNames ?? Enumerable.Empty<string>())
				.Where(name => name != null)
				.Where(name => !String.IsNullOrWhiteSpace(name));

			foreach(var name in queryStringNames)
				yield return name;

			for(ulong innerIndex = 0; innerIndex < UInt64.MaxValue; innerIndex++)
				yield return String.Format("filter.{0}.{1}", FilterControlContext.Index, innerIndex);
		}

		protected abstract void SetValues(IEnumerable<KeyValuePair<string, string>> values);

		public virtual FilterClause GetFilterClause()
		{
			var parameterNames = GetParameterNames();
			return GetFilterClause(parameterNames);
		}

		protected abstract FilterClause GetFilterClause(IEnumerable<string> parameterNames);

		protected virtual IEnumerable<string> GetParameterNames()
		{
			foreach(var name in SqlParameterNames ?? Enumerable.Empty<String>())
				yield return name;

			// Generate as many auto-generated parameter names as needed.
			// Technically infinite, but the VS debugger doesn't like iterating infinte enumerables, so cap it at 100.
			for(var i = 0; i < 100; i++)
				yield return String.Format("P_{0}_{1}", FilterControlContext.Index, i);
		}
	}
}
