// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Caching.ObjectCaching.Dependency
{
	/// <summary>
	/// Handles dependencies on the last time the rows in a query were updated.
	/// </summary>
	public class QueryDependencyStateManager : IDependencyStateManager
	{
		readonly AppConfigProvider AppConfigProvider;
		readonly HashProvider HashProvider;

		public QueryDependencyStateManager(AppConfigProvider appConfigProvider, HashProvider hashProvider)
		{
			AppConfigProvider = appConfigProvider;
			HashProvider = hashProvider;
		}

		public DependencyState GetState(DependencyStateContext context)
		{
			if(!(context is QueryDependencyStateContext))
				return null;

			var queryContext = (QueryDependencyStateContext)context;
			var updatedOnValues = new List<object>();

			// Connect to the database and run the query
			using(var connection = new SqlConnection(DB.GetDBConn()))
			using(var command = connection.CreateCommand())
			{
				command.CommandText = queryContext.Query;

				var sqlParameters = queryContext
					.Parameters
					.Select(kvp => new SqlParameter(kvp.Key, kvp.Value))
					.ToArray();

				command.Parameters.AddRange(sqlParameters);

				connection.Open();
				using(var reader = command.ExecuteReader())
					while(reader.Read())
					{
						// Read out all results as nullable DateTime's and store them in a list
						var updatedOn = reader.GetFieldValue<DateTime?>(0);
						if(updatedOn != null)
							updatedOnValues.Add(updatedOn.Value);
					}
			}

			// Return a state that is the result of hashing all of the returned DateTimes together.
			return new DependencyState(
				context: context,
				state: HashProvider.Hash(updatedOnValues));
		}

		public bool? HasStateChanged(DependencyState establishedState)
		{
			if(!(establishedState.Context is QueryDependencyStateContext))
				return null;

			var currentState = GetState((QueryDependencyStateContext)establishedState.Context);
			var stateChanged = currentState.State != establishedState.State;

			if(stateChanged && AppConfigProvider.GetAppConfigValue<bool>("ObjectCacheDebuggingEnabled"))
				Debug.WriteLine(string.Format(
					"ObjCache - Query Changed - {0}",
					((QueryDependencyStateContext)establishedState.Context).Query));

			return stateChanged;
		}
	}

	[DebuggerDisplay("Query: {Query}")]
	public class QueryDependencyStateContext : DependencyStateContext
	{
		public readonly string Query;
		public Dictionary<string, object> Parameters;

		public QueryDependencyStateContext(string query, Dictionary<string, object> parameters = null)
		{
			Query = query;
			Parameters = parameters ?? new Dictionary<string, object>();
		}
	}
}
