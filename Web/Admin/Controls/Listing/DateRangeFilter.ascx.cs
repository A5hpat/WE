// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.UI.WebControls;

namespace AspDotNetStorefrontControls.Listing
{
	public partial class DateRangeFilter : FilterControl
	{
		public string StartLabel
		{ get; set; }

		public string EndLabel
		{ get; set; }

		public string FieldName
		{ get; set; }

		public override int GridColumns
		{ get { return 6; } }

		protected override void OnPreRender(EventArgs e)
		{
			StartValueLabel.Text = StartLabel ?? "Start Date";
			EndValueLabel.Text = EndLabel ?? "End Date";

			var filterNames = GenerateFilterNames();
			StartValue.DateInput.Attributes.Add("data-filter-name", filterNames.First());
			EndValue.DateInput.Attributes.Add("data-filter-name", filterNames.Skip(1).First());

			StartValue.Culture = Thread.CurrentThread.CurrentUICulture;
			EndValue.Culture = Thread.CurrentThread.CurrentUICulture;
		}

		protected override void SetValues(IEnumerable<KeyValuePair<string, string>> values)
		{
			DateTime startValueDate;
			if(DateTime.TryParse(values.First().Value, out startValueDate))
				StartValue.SelectedDate = startValueDate;
			else
				StartValue.SelectedDate = null;

			DateTime endValueDate;
			if(DateTime.TryParse(values.Skip(1).First().Value, out endValueDate))
				EndValue.SelectedDate = endValueDate;
			else
				EndValue.SelectedDate = null;
		}

		protected override FilterClause GetFilterClause(IEnumerable<string> parameterNames)
		{
			var startValueParameterName = parameterNames.First();
			var endValueParameterName = parameterNames.Skip(1).First();

			return new FilterClause(
				String.IsNullOrWhiteSpace(FieldName)
					? null
					: String.Format(
						@"(
							(@{0} is null and @{1} is null) or
							(@{0} is not null and @{1} is null and {2} >= @{0}) or
							(@{0} is null and @{1} is not null and {2} <= dateadd(s, -1, dateadd(d,1,@{1}))) or
							({2} >= @{0} and {2} <= dateadd(d,1,@{1}))
						)",
						startValueParameterName,
						endValueParameterName,
						FieldName),
				new[] 
					{
						new ControlParameter(startValueParameterName, System.Data.DbType.DateTime, StartValue.UniqueID, "SelectedDate"),
						new ControlParameter(endValueParameterName, System.Data.DbType.DateTime, EndValue.UniqueID, "SelectedDate") 
					});
		}
	}
}
