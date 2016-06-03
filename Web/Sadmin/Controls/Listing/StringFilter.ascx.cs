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
	public partial class StringFilter : FilterControl
	{
		public string Label
		{ get; set; }

		public string FieldName
		{ get; set; }

		protected override void OnPreRender(EventArgs e)
		{
			ValueLabel.Visible = !String.IsNullOrEmpty(Label);
			ValueLabel.Text = Label;

			var filterNames = GenerateFilterNames();
			Value.Attributes.Add("data-filter-name", filterNames.First());
		}

		protected override void SetValues(IEnumerable<KeyValuePair<string, string>> values)
		{
			if(String.IsNullOrWhiteSpace(values.First().Value))
				Value.Text = null;
			else
				Value.Text = values.First().Value.Trim();
		}

		protected override FilterClause GetFilterClause(IEnumerable<string> parameterNames)
		{
			var selectedValueParameterName = parameterNames.First();

			return new FilterClause(
				String.IsNullOrWhiteSpace(FieldName)
					? null
					: String.Format("(@{0} is null or charindex(@{0}, {1}) > 0)", selectedValueParameterName, FieldName),
				new[] { new ControlParameter(selectedValueParameterName, System.Data.DbType.String, Value.UniqueID, "Text") });
		}
	}
}
