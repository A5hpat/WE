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
	public class FilterClause
	{
		public readonly string WhereClauseSegment;
		public readonly IEnumerable<Parameter> Parameters;

		public FilterClause(string whereClauseSegment, IEnumerable<Parameter> parameters)
		{
			WhereClauseSegment = whereClauseSegment ?? String.Empty;
			Parameters = parameters ?? Enumerable.Empty<Parameter>();
		}
	}
}
