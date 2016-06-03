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
	public partial class VariantEntityFilter : FilterControl
	{
		const string FallbackEntityType = "Category";
		const string DefaultVariantIdColumnName = "VariantID";

		public string Label
		{ get; set; }

		public string EntityType
		{ get; set; }

		public string VariantIdColumnName
		{ get; set; }

		protected override void OnPreRender(EventArgs e)
		{
			ValueLabel.Visible = !String.IsNullOrEmpty(Label);
			ValueLabel.Text = Label;

			var filterNames = GenerateFilterNames();
			Value.Attributes.Add("data-filter-name", filterNames.First());
		}

		protected override FilterClause GetFilterClause(IEnumerable<string> parameterNames)
		{
			var valueParameterName = parameterNames.First();

			return new FilterClause(
				String.Format(
					@"(
						@{0} is null or 
						{1} in (
							select pv.VariantId
							from {2} e
								inner join Product{2} pe on e.{2}ID = pe.{2}ID
								inner join ProductVariant pv on pv.ProductID = pe.ProductID
							where charindex(@{0}, e.Name) > 0)
					)",
					valueParameterName,
					VariantIdColumnName ?? DefaultVariantIdColumnName,
					EntityType ?? FallbackEntityType),
				new[] { new ControlParameter(valueParameterName, System.Data.DbType.String, Value.UniqueID, "Text") });
		}

		protected void Value_DataBound(object sender, EventArgs e)
		{
			if(!IsPostBack)
				Value.Text = string.Empty;
		}

		protected override void SetValues(IEnumerable<KeyValuePair<string, string>> values)
		{
			if(String.IsNullOrWhiteSpace(values.First().Value))
				Value.Text = null;
			else
				Value.Text = values.First().Value;
		}
	}
}
