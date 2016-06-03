// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

namespace AspDotNetStorefrontControls.Listing
{
	public partial class IntegerFilter : FilterControl
	{
		public string Label
		{ get; set; }

		protected string Value
		{ get; set; }

		protected string ValueFilterName
		{ get; set; }

		public string FieldName
		{ get; set; }

		protected override void OnInit(EventArgs e)
		{
			ValueFilterName = GenerateFilterNames().First();
		}

		protected override void OnPreRender(EventArgs e)
		{
			DataBindChildren();
			
			base.OnPreRender(e);
		}

		protected override void SetValues(IEnumerable<KeyValuePair<string, string>> values)
		{
			var value = values.First().Value;

			Value = String.IsNullOrWhiteSpace(value)
				? null
				: value;
		}

		protected override FilterClause GetFilterClause(IEnumerable<string> parameterNames)
		{
			var selectedValueParameterName = parameterNames.First();

			return new FilterClause(
				String.IsNullOrWhiteSpace(FieldName)
					? null
					: String.Format("(@{0} is null or {1} = @{0})", selectedValueParameterName, FieldName),
				new[] { 
					new QueryStringParameter(selectedValueParameterName, System.Data.DbType.Int32, ValueFilterName),
				});
		}
	}
}
