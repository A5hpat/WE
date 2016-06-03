// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AspDotNetStorefrontControls.Listing
{
	class QueryBuilder
	{
		public const string DisplayLocaleParameterName = "_locale";
		public const string CurrentCustomerLocaleParameterName = "_currentCustomerLocale";
		public const string StartingRowIndexParameterName = "_startingRowIndex";
		public const string PageSizeParameterName = "_pageSize";

		public ParameterizedDataSourceQuery BuildDataSourceQuery(Templates templates, QueryContext queryContext, IEnumerable<FilterClause> filterClauses)
		{
			var sql = BuildSql(templates, queryContext, filterClauses);
			var parameters = BuildDataSourceParameters(queryContext,filterClauses);

			return new ParameterizedDataSourceQuery(sql, parameters);
		}

		public ParameterizedSqlQuery BuildFilterWhereClause(QueryContext queryContext, IEnumerable<FilterClause> filterClauses, Control dataSourceControl)
		{
			var sql = BuildFilterWhereSql(filterClauses);
			var parameters = BuildSqlParameters(queryContext, filterClauses, dataSourceControl);

			return new ParameterizedSqlQuery(sql, parameters);
		}

		IEnumerable<Parameter> BuildDataSourceParameters(QueryContext queryContext, IEnumerable<FilterClause> filterClauses)
		{
			return filterClauses
				.SelectMany(segment => segment.Parameters)
				.Concat(new[] 
					{ 
						new Parameter(DisplayLocaleParameterName, DbType.String, queryContext.DisplayLocale),
						new Parameter(CurrentCustomerLocaleParameterName, DbType.String, queryContext.CurrentCustomerLocale),
						new Parameter(StartingRowIndexParameterName, DbType.Int64, queryContext.PageRowStartIndex.ToString()),
						new Parameter(PageSizeParameterName, DbType.Int64, queryContext.PageSize.ToString()),
					})
				.ToArray();
		}

		IEnumerable<SqlParameter> BuildSqlParameters(QueryContext queryContext, IEnumerable<FilterClause> filterClauses, Control dataSourceControl)
		{
			var parameterEvaluateMethod = typeof(Parameter).GetMethod("Evaluate", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			var parameterGetValueMethod = typeof(Parameter).GetMethod("GetValue", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, new[] { typeof(object), typeof(bool) }, null);

			return filterClauses
				.SelectMany(segment => segment.Parameters
					.Select(parameter => 
					{
						var value = parameterEvaluateMethod.Invoke(parameter, new object[] { System.Web.HttpContext.Current, dataSourceControl });
						value = parameterGetValueMethod.Invoke(parameter, new object[] { value, false});

						return new
						{
							Name = parameter.Name,
							DbType = parameter.GetDatabaseType(),
							Value = value,
						};
					})
					.Select(o => new SqlParameter
						{
							ParameterName = o.Name,
							DbType = o.DbType,
							Value = o.Value == null
								? DBNull.Value
								: o.Value
						}))
				.Concat(new[] 
					{ 
						new SqlParameter(DisplayLocaleParameterName, queryContext.DisplayLocale),
						new SqlParameter(CurrentCustomerLocaleParameterName, queryContext.CurrentCustomerLocale),
					})
				.ToArray();
		}

		string BuildSql(Templates templates, QueryContext queryContext, IEnumerable<FilterClause> filterClauses)
		{
			var selectSql = BuildPagingSelectSql(templates, queryContext, filterClauses);

			return String.Format(
				templates.QueryTemplate,
				selectSql);
		}

		string BuildPagingSelectSql(Templates templates, QueryContext queryContext, IEnumerable<FilterClause> filterClauses)
		{
			var sortSelectSql = BuildSortSelectSql(templates, queryContext);
			var filterWhereSql = BuildFilterWhereSql(filterClauses);

			return String.Format(
				queryContext.SqlQuery,
				sortSelectSql,
				filterWhereSql);
		}

		string BuildSortSelectSql(Templates templates, QueryContext queryContext)
		{
			var sortExpression = queryContext.SortExpression ?? "(select null)";

			return String.Format(
				templates.RowSortTemplate,
				String.Format(
					sortExpression.Contains("{0}")
						? sortExpression
						: String.Format("{0} {{0}}", sortExpression),
					queryContext.SortDirection == System.Web.UI.WebControls.SortDirection.Descending 
						? "desc" 
						: "asc"));
		}

		string BuildFilterWhereSql(IEnumerable<FilterClause> filterClauses)
		{
			return String.Join(
				" and ",
				filterClauses
					.Where(clause => clause != null)
					.Select(clause => clause.WhereClauseSegment)
					.Where(segment => !String.IsNullOrWhiteSpace(segment))
					.DefaultIfEmpty("1=1"));
		}

		public class Templates
		{
			public readonly string QueryTemplate;
			public readonly string RowSortTemplate;

			public Templates(string queryTemplate, string rowSortTemplate)
			{
				QueryTemplate = queryTemplate;
				RowSortTemplate = rowSortTemplate;
			}
		}

		public class QueryContext
		{
			public readonly string SqlQuery;
			public readonly string SortExpression;
			public readonly SortDirection SortDirection;
			public readonly long PageRowStartIndex;
			public readonly long PageSize;
			public readonly string DisplayLocale;
			public readonly string CurrentCustomerLocale;

			public QueryContext(string sqlQuery, string sortExpression, SortDirection sortDirection, long pageRowStartIndex, long pageSize, string displayLocale, string currentCustomerLocale)
			{
				SqlQuery = sqlQuery;
				SortExpression = sortExpression;
				SortDirection = sortDirection;
				PageRowStartIndex = pageRowStartIndex;
				PageSize = pageSize;
				DisplayLocale = displayLocale;
				CurrentCustomerLocale = currentCustomerLocale;
			}
		}
	}
}
