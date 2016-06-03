// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Routing;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Routing.Constraints
{
	public class IsEntitySeName : IRouteConstraint
	{
		readonly string EntityTypeRouteDataKey;
		readonly string IdRouteDataKey;

		public IsEntitySeName(string entityTypeRouteDataKey, string idRouteDataKey)
		{
			EntityTypeRouteDataKey = entityTypeRouteDataKey;
			IdRouteDataKey = idRouteDataKey;
		}

		public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
		{
			if(!values.ContainsKey(EntityTypeRouteDataKey))
				return false;

			var entityType = values[EntityTypeRouteDataKey] as string;
			if(string.IsNullOrEmpty(entityType))
				return false;

			if(!AppLogic.ro_SupportedEntities.Contains(entityType, StringComparer.OrdinalIgnoreCase))
				return false;

			if(!values.ContainsKey(parameterName))
				return false;

			var seName = values[parameterName] as string;
			if(string.IsNullOrEmpty(seName))
				return false;

			using(var connection = new SqlConnection(DB.GetDBConn()))
			using(var command = new SqlCommand())
			{
				command.Connection = connection;
				command.CommandText = "select top 1 EntityID from Entities where EntityType = @entityType and SEName = @seName";
				command.Parameters.AddWithValue("entityType", entityType);
				command.Parameters.AddWithValue("seName", seName);

				connection.Open();
				var id = command.ExecuteScalar();

				if(id is DBNull)
					return false;

				if(!values.ContainsKey(IdRouteDataKey))
					values.Add(IdRouteDataKey, id);

				return true;
			}
		}
	}
}
