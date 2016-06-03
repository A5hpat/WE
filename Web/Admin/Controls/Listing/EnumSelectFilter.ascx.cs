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
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontControls.Listing
{
	public partial class EnumSelectFilter : FilterControl
	{
		[TypeConverter(typeof(TypeTypeConverter))]
		public Type EnumType
		{ get; set; }

		public string Label
		{ get; set; }

		public string UnspecifiedLabel
		{ get; set; }

		public string FieldName
		{ get; set; }

		protected override void OnInit(EventArgs e)
		{
			if(!EnumType.IsEnum)
				throw new InvalidOperationException("EnumType must be an enum");

			if(!IsPostBack)
			{
				var names = Enum.GetNames(EnumType)
					.Select(name => new ListItem(name))
					.ToArray();

				Value.Items.Add(new ListItem(UnspecifiedLabel ?? AppLogic.GetStringForDefaultLocale("admin.common.Unspecified"), String.Empty));
				Value.Items.AddRange(names);
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
					: String.Format("(@{0} is null or @{0} = '' or {1} = @{0})", selectedValueParameterName, FieldName),
				new[] { new ControlParameter(selectedValueParameterName, System.Data.DbType.String, Value.UniqueID, "SelectedValue") });
		}
	}
}
