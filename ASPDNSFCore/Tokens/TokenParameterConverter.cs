// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;

namespace AspDotNetStorefrontCore.Tokens
{
	public class TokenParameterConverter
	{
		readonly IDictionary<string, string> EmptyDictionary = new Dictionary<string, string>();

		public IDictionary<string, string> ConvertPositionalToNamedParameters(IEnumerable<string> positionalParameters, IEnumerable<string> parameterNames, bool? mapExtraParametersToWildcards = null)
		{
			if(positionalParameters == null || parameterNames == null)
				return EmptyDictionary;

			var effectiveParameterNames = mapExtraParametersToWildcards == true
				? ExtendWithWildcardNames(parameterNames)
				: parameterNames;

			// Merge the names and parameter values together
			return positionalParameters
				.Zip(
					effectiveParameterNames,
					(parameter, name) => new { parameter, name })
				.ToDictionary(
					o => o.name,
					o => o.parameter,
					StringComparer.OrdinalIgnoreCase);
		}

		IEnumerable<string> ExtendWithWildcardNames(IEnumerable<string> parameterNames)
		{
			var enumerator = parameterNames.GetEnumerator();

			// Iterate through all the names
			while(enumerator.MoveNext())
				yield return enumerator.Current;

			// Create a new wildcard name for each extra parameter
			for(var i = 0; i < int.MaxValue; i++)
				yield return string.Format("*{0}", i);
		}
	}
}
