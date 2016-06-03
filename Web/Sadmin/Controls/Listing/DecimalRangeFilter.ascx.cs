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
	public partial class DecimalRangeFilter : FilterControl
	{
		public string StartLabel
		{ get; set; }

		protected string StartValue
		{ get; set; }

		protected string StartValueFilterName
		{ get; set; }

		public string EndLabel
		{ get; set; }

		protected string EndValue
		{ get; set; }

		protected string EndValueFilterName
		{ get; set; }
		
		public string FieldName
		{ get; set; }

		public override int GridColumns
		{ get { return 6; } }

		protected override void OnInit(EventArgs e)
		{
			var filterNames = GenerateFilterNames();
			StartValueFilterName = filterNames.First();
			EndValueFilterName = filterNames.Skip(1).First();
		}

		protected override void OnPreRender(EventArgs e)
		{
			DataBindChildren();

			base.OnPreRender(e);
		}

		protected override void SetValues(IEnumerable<KeyValuePair<string, string>> values)
		{
			var startValue = values.First().Value;
			var endValue = values.Skip(1).First().Value;

			StartValue = String.IsNullOrWhiteSpace(startValue)
				? null
				: startValue;

			EndValue = String.IsNullOrWhiteSpace(endValue)
				? null
				: endValue;
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
							(@{0} is null and @{1} is not null and {2} <= @{1}) or
							({2} >= @{0} and {2} <= @{1})
						)",
						startValueParameterName,
						endValueParameterName,
						FieldName),
				new[] 
					{
						new QueryStringParameter(startValueParameterName, System.Data.DbType.Decimal, StartValueFilterName),
						new QueryStringParameter(endValueParameterName, System.Data.DbType.Decimal, EndValueFilterName),
					});
		}
	}
}
