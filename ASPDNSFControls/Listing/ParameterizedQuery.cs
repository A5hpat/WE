// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;

namespace AspDotNetStorefrontControls.Listing
{
	public abstract class ParameterizedQuery<TParameter>
	{
		public readonly string Sql;
		public readonly IEnumerable<TParameter> Parameters;

		public ParameterizedQuery(string sql, IEnumerable<TParameter> parameters)
		{
			Sql = sql;
			Parameters = parameters;
		}
	}

	public class ParameterizedDataSourceQuery : ParameterizedQuery<System.Web.UI.WebControls.Parameter>
	{
		public ParameterizedDataSourceQuery(string sql, IEnumerable<System.Web.UI.WebControls.Parameter> parameters)
			: base(sql, parameters)
		{ }
	}

	public class ParameterizedSqlQuery : ParameterizedQuery<System.Data.SqlClient.SqlParameter>
	{
		public ParameterizedSqlQuery(string sql, IEnumerable<System.Data.SqlClient.SqlParameter> parameters)
			: base(sql, parameters)
		{ }
	}
}
