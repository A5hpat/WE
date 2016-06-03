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
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontControls.Listing
{
	public partial class StoreFilter : FilterControl
	{
		public string Label
		{ get; set; }

		public string UnspecifiedLabel
		{ get; set; }

		public string FieldName
		{ get; set; }

		string SelectedValue
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
			SelectedValue = values.First().Value;
		}

		protected override FilterClause GetFilterClause(IEnumerable<string> parameterNames)
		{
			var selectedValueParameterName = parameterNames.First();

			return new FilterClause(
				String.IsNullOrWhiteSpace(FieldName)
					? null
					: String.Format("(@{0} is null or @{0} = '' or {1} = @{0})", selectedValueParameterName, FieldName),
				new[] { new ControlParameter(selectedValueParameterName, System.Data.DbType.String, Value.UniqueID, "SelectedValue") });
		}

		protected void Value_DataBound(object sender, EventArgs e)
		{
			Value.Items.Insert(0, new ListItem(UnspecifiedLabel ?? AppLogic.GetStringForDefaultLocale("admin.common.Unspecified"), String.Empty));

			if(String.IsNullOrWhiteSpace(SelectedValue))
				Value.SelectedIndex = -1;
			else
				Value.SelectFirstByValue(SelectedValue, StringComparer.OrdinalIgnoreCase);
		}
	}
}
