// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data.SqlClient;
using System.Web;
using System.Web.Routing;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Routing.Constraints
{
	public class IsProductSeName : IRouteConstraint
	{
		readonly string IdRouteDataKey;

		public IsProductSeName(string idRouteDataKey)
		{
			IdRouteDataKey = idRouteDataKey;
		}

		public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
		{
			if(!values.ContainsKey(parameterName))
				return false;

			var seName = values[parameterName] as string;
			if(String.IsNullOrEmpty(seName))
				return false;

			using(var connection = new SqlConnection(DB.GetDBConn()))
			using(var command = new SqlCommand())
			{
				command.Connection = connection;
				command.CommandText = "select top 1 ProductID from Product where SEName = @seName";
				command.Parameters.AddWithValue("seName", seName);

				connection.Open();
				var productId = command.ExecuteScalar();

				if(productId is DBNull)
					return false;

				if(!values.ContainsKey(IdRouteDataKey))
					values.Add(IdRouteDataKey, productId);

				return true;
			}
		}
	}
}
