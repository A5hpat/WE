// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Diagnostics;
using System.Linq;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Caching.ObjectCaching.Dependency
{
	/// <summary>
	/// Handles dependencies on other cache entires.
	/// </summary>
	public class CacheEntryDependencyStateManager : IDependencyStateManager
	{
		readonly AppConfigProvider AppConfigProvider;
		readonly CacheProvider CacheProvider;

		public CacheEntryDependencyStateManager(AppConfigProvider appConfigProvider, CacheProvider cacheProvider)
		{
			AppConfigProvider = appConfigProvider;
			CacheProvider = cacheProvider;
		}

		public DependencyState GetState(DependencyStateContext context)
		{
			if(!(context is CacheEntryDependencyStateContext))
				return null;

			// Attempt to get the requested entry from the cache
			var cacheContext = (CacheEntryDependencyStateContext)context;
			var entry = CacheProvider.Check(cacheContext.Key);

			// If the entry doesn't exist, return a "null" state.
			if(entry == null)
				return new DependencyState(
					context: context,
					state: 0);

			// Create a state that is the hash of the matching keys' states.
			var hash = entry
				.DependencyStates
				.Where(state => !(state.Context is CacheEntryDependencyStateContext))
				.Aggregate(0L, (agg, state) => agg ^ state.State);

			return new DependencyState(
				context: context,
				state: hash);
		}

		public bool? HasStateChanged(DependencyState establishedState)
		{
			if(!(establishedState.Context is CacheEntryDependencyStateContext))
				return null;

			var currentState = GetState(establishedState.Context);
			var stateChanged = currentState.State != establishedState.State;

			if(stateChanged && AppConfigProvider.GetAppConfigValue<bool>("ObjectCacheDebuggingEnabled"))
				Debug.WriteLine(string.Format(
					"ObjCache - Cache Entry Changed - Key {0}",
					((CacheEntryDependencyStateContext)establishedState.Context).Key));

			return stateChanged;
		}
	}

	[DebuggerDisplay("Cache: {Key}")]
	public class CacheEntryDependencyStateContext : DependencyStateContext
	{
		public readonly string Key;

		public CacheEntryDependencyStateContext(string key)
		{
			Key = key;
		}
	}
}
