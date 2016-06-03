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
	public partial class FlagFilter : FilterControl
	{
		public string Label
		{ get; set; }

		public string FieldName
		{ get; set; }

		public bool Default
		{ get; set; }

		protected override void OnPreRender(EventArgs e)
		{
			Value.Visible = !String.IsNullOrEmpty(Label);
			LabelControl.Text = Label;

			var filterNames = GenerateFilterNames();
			Value.Attributes.Add("data-filter-name", filterNames.First());
			Value.Attributes.Add("data-filter-apply-empty-value", "true");
		}

		protected override void SetValues(IEnumerable<KeyValuePair<string, string>> values)
		{
			if(String.IsNullOrWhiteSpace(values.First().Value))
				Value.Checked = Default;
			else
				Value.Checked = StringComparer.OrdinalIgnoreCase.Equals(Boolean.TrueString, values.First().Value);
		}

		protected override FilterClause GetFilterClause(IEnumerable<string> parameterNames)
		{
			var selectedValueParameterName = parameterNames.First();

			return new FilterClause(
				String.IsNullOrWhiteSpace(FieldName)
					? null
					: String.Format(
						"({0} = {2} or {0} = @{1})",
						FieldName,
						selectedValueParameterName,
						Default
							? 1
							: 0),
			new[] { new ControlParameter(selectedValueParameterName, System.Data.DbType.Boolean, Value.UniqueID, "Checked") });
		}
	}
}
