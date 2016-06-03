// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using AspDotNetStorefront.Caching.ObjectCaching.Dependency;

namespace AspDotNetStorefront.Caching.ObjectCaching
{
	/// <summary>
	/// The <see cref="CacheDependencyEvaluator"/> is a layer over a <see cref="CacheProvider"/>
	/// and the <see cref="DependencyStateProvider"/> used to evaluate the dependencies of its
	/// entries. Any access to the cache needs to check dependencies, so a client will use
	/// this class rather than a <see cref="CacheProvider"/> directly.
	/// </summary>
	public class CacheDependencyEvaluator
	{
		readonly CacheProvider CacheProvider;
		readonly DependencyStateProvider DependencyStateProvider;

		public CacheDependencyEvaluator(CacheProvider cacheProvider, DependencyStateProvider dependencyStateProvider)
		{
			CacheProvider = cacheProvider;
			DependencyStateProvider = dependencyStateProvider;
		}

		/// <summary>
		/// Attempts to retrieve an item from the cache, checking its dependencies and re-generating the
		/// item if they've been invalidated.
		/// </summary>
		/// <typeparam name="T">The type of the item to return.</typeparam>
		/// <param name="key">The key of the item in the cache.</param>
		/// <param name="setter">The context needed to create the item in the cache if it doesn't exist.</param>
		/// <returns>An instance of the requested type.</returns>
		public T GetOrUpdateCache<T>(string key, Lazy<Tuple<T, IEnumerable<DependencyState>>> setter)
		{
			var existingEntry = CacheProvider.Get(key);
			if(existingEntry != null)
			{
				// Validate dependencies
				var invalidated = existingEntry
					.DependencyStates
					.Where(state => DependencyStateProvider.HasStateChanged(state))
					.Any();

				if(!invalidated)
					return (T)existingEntry.Value;
			}

			var newEntry = CacheProvider.Set(
				key: key,
				value: setter.Value.Item1,
				dependencyStates: setter.Value.Item2);

			return (T)newEntry.Value;
		}
	}
}
