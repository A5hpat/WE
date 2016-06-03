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
	public partial class PromotionFilter : FilterControl
	{
		public string Label
		{ get; set; }

		public string UnspecifiedLabel
		{ get; set; }

		public string FieldName
		{ get; set; }

		public string PromotionFieldName
		{ get; set; }

		protected override void OnInit(EventArgs e)
		{
			if(!Page.IsPostBack)
			{
				Value.DataSource = ValueDataSource;
				Value.DataBind();
			}

			base.OnInit(e);
		}

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
				Value.SelectedIndex = -1;
			else
				Value.SelectFirstByValue(values.First().Value, StringComparer.OrdinalIgnoreCase);
		}

		protected override FilterClause GetFilterClause(IEnumerable<string> parameterNames)
		{
			var selectedValueParameterName = parameterNames.First();

			return new FilterClause(
				String.IsNullOrWhiteSpace(FieldName)
					? null
					: String.Format(
						"(@{0} is null or {1} in (select {2} from PromotionUsage where promotionid = @{0}))", 
						selectedValueParameterName, 
						FieldName, 
						PromotionFieldName),
				new[] { new ControlParameter(selectedValueParameterName, System.Data.DbType.Int32, Value.UniqueID, "SelectedValue") });
		}

		protected void Value_DataBound(object sender, EventArgs e)
		{
			if (!IsPostBack)
				Value.Items.Insert(0, new ListItem(UnspecifiedLabel ?? AppLogic.GetStringForDefaultLocale("admin.common.Unspecified"), String.Empty));
		}
	}
}
