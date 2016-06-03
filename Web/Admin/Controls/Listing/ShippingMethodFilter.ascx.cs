// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
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
	public partial class ShippingMethodFilter : FilterControl
	{
		public string Label
		{ get; set; }

		public string UnspecifiedLabel
		{ get; set; }

		public string FieldName
		{ get; set; }

		public bool HideUnspecifiedItem
		{ get; set; }

		string SelectedValue
		{ get; set; }

		protected override void OnInit(EventArgs e)
		{
			ValueDataSource.SelectCommand = @"
				select distinct
					case when nullif(ShippingMethod, '') is null
					then
						'(No Shipping Method)'
					else
						coalesce(nullif(SUBSTRING(ShippingMethod,0,CHARINDEX('|',ShippingMethod)), ''), ShippingMethod)
					end
					As Display,
					case when nullif(SUBSTRING(ShippingMethod,0,CHARINDEX('|',ShippingMethod)), '') is not null
						then
							SUBSTRING(ShippingMethod,0,CHARINDEX('|',ShippingMethod)) + '|%'
						else
							ShippingMethod
						end
					As Value 
				from Orders
				where ShippingMethod is not null
				order by Display";

			ValueDataSource.SelectParameters.Add(FilterControlContext.LocaleParameter);
			ValueDataSource.SelectParameters.Add(FilterControlContext.CurrentCustomerLocaleParameter);

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
			SelectedValue = values.First().Value;
		}

		protected override FilterClause GetFilterClause(IEnumerable<string> parameterNames)
		{
			var selectedValueParameterName = parameterNames.First();

			return new FilterClause(
				String.IsNullOrWhiteSpace(FieldName)
					? null
					: String.Format("(@{0} is null or @{0} = '' or {1} like @{0})", selectedValueParameterName, FieldName),
				new[] { new ControlParameter(selectedValueParameterName, System.Data.DbType.String, Value.UniqueID, "SelectedValue") });
		}

		protected void Value_DataBound(object sender, EventArgs e)
		{
			if(!HideUnspecifiedItem)
				Value.Items.Insert(0, 
					new ListItem(UnspecifiedLabel ?? AppLogic.GetStringForDefaultLocale("admin.common.Unspecified"), String.Empty));

			if(String.IsNullOrWhiteSpace(SelectedValue))
				Value.SelectedIndex = -1;
			else
				Value.SelectFirstByValue(SelectedValue, StringComparer.OrdinalIgnoreCase);
		}
	}
}
