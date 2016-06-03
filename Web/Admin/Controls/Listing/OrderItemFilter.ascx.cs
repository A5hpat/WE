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
	public partial class OrderItemFilter : FilterControl
	{
		public string Label
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
				Value.Text = values.First().Value;
		}

		protected override FilterClause GetFilterClause(IEnumerable<string> parameterNames)
		{
			var selectedValueParameterName = parameterNames.First();

			return new FilterClause(
				String.Format(
					"(@{0} is null or OrderNumber in (select OrderNumber from Orders_ShoppingCart where charindex(@{0}, OrderedProductName) > 0 or charindex(@{0}, OrderedProductVariantName) > 0 or charindex(@{0}, OrderedProductSku) > 0))",
					selectedValueParameterName),
				new[] { new ControlParameter(selectedValueParameterName, System.Data.DbType.String, Value.UniqueID, "Text") });
		}
	}
}
