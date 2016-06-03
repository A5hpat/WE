// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using AspDotNetStorefront.Caching.ObjectCaching.Dependency;

namespace AspDotNetStorefront.Caching.ObjectCaching
{
	/// <summary>
	/// <see cref="CacheProvider"/> is the core of ADNSF object caching. A singleton instance (managed by the DI container)
	/// provides a persistent data store (scope to the application lifetime) and accessors that validate 
	/// dependencies and invoke builders as appropriate.
	/// </summary>
	public class CacheProvider
	{
		/// <summary>
		/// How frequently to evaluate entries to check if they're expired.
		/// </summary>
		readonly TimeSpan ExpiryEvaluationInterval = TimeSpan.FromSeconds(15);

		/// <summary>
		/// How long an entry is allowed to remain in the cache before expiring.
		/// </summary>
		readonly TimeSpan EntryExpirationTimeout = TimeSpan.FromMinutes(15);

		readonly ConcurrentDictionary<string, CacheEntry> Cache;
		readonly IEqualityComparer<CacheEntry> CacheEntryComparer;
		readonly Timer CacheExpiryTimer;

		public CacheProvider()
		{
			var keyComparer = StringComparer.OrdinalIgnoreCase;
			Cache = new ConcurrentDictionary<string, CacheEntry>(keyComparer);
			CacheEntryComparer = new CacheEntryEqualityComparer(keyComparer);

			CacheExpiryTimer = new Timer(ExpiryEvaluationInterval.TotalMilliseconds);
			CacheExpiryTimer.Elapsed += CacheExpiryTimer_Elapsed;
			CacheExpiryTimer.AutoReset = false;											// Pause the timer while executing the handler to prevent 
																						// simultaneuous execution.
			CacheExpiryTimer.Start();
		}

		/// <summary>
		/// Executed every time the CacheExpiryTimer elapses.
		/// </summary>
		void CacheExpiryTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			try
			{
				// Iterate the entries and remove any that haven't been used in a while
				var expiredEntries = Cache
					.Values
					.Where(entry => DateTime.Now - entry.Accessed > EntryExpirationTimeout)
					.Select(entry => entry.Key)
					.ToArray();

				CacheEntry satisfyOutParameter;
				foreach(var key in expiredEntries)
					Cache.TryRemove(key, out satisfyOutParameter);
			}
			finally
			{
				// No matter what happens, restart the timer
				CacheExpiryTimer.Start();
			}
		}

		/// <summary>
		/// Retrieves a <see cref="CacheEntry"/> from the cache and updates its last accessed timestamp.
		/// </summary>
		/// <param name="key">The key of the <see cref="CacheEntry"/> to return.</param>
		/// <returns>A <see cref="CacheEntry"/> if the key is found, null otherwise.</returns>
		public CacheEntry Get(string key)
		{
			// If the entry doesn't exist, return null
			CacheEntry existingEntry;
			if(!Cache.TryGetValue(key, out existingEntry))
				return null;

			// If it does exist, update access timestamp and return the entry
			return Cache[key] = new CacheEntry(existingEntry, accessed: DateTime.Now);
		}

		/// <summary>
		/// Retrieves a <see cref="CacheEntry"/> from the cache without updating its last accessed timestamp.
		/// </summary>
		/// <param name="key">The key of the <see cref="CacheEntry"/> to return.</param>
		/// <returns>A <see cref="CacheEntry"/> if the key is found, null otherwise.</returns>
		public CacheEntry Check(string key)
		{
			// If the entry doesn't exist, return null
			CacheEntry existingEntry;
			if(!Cache.TryGetValue(key, out existingEntry))
				return null;

			// If it does exist, just return the entry
			return existingEntry;
		}

		/// <summary>
		/// Stores a new entry in the cache.
		/// </summary>
		/// <param name="key">The key of the new entry.</param>
		/// <param name="value">The value to store.</param>
		/// <param name="dependencyStates">The dependency states of the new entry.</param>
		/// <returns>A <see cref="CacheEntry"/> for the new entry.</returns>
		public CacheEntry Set(string key, object value, IEnumerable<DependencyState> dependencyStates)
		{
			// Create a new entry and set it, overwriting any current entry that may have been at that key
			var newEntry = new CacheEntry(key, value, dependencyStates, DateTime.Now, DateTime.Now);
			return Cache[key] = newEntry;
		}

		/// <summary>
		/// Compares two CacheEntries, determining if they're equal based on their key and last-accessed timestamp.
		/// </summary>
		class CacheEntryEqualityComparer : EqualityComparer<CacheEntry>
		{
			readonly IEqualityComparer<string> KeyComparer;

			public CacheEntryEqualityComparer(IEqualityComparer<string> keyComparer)
			{
				KeyComparer = keyComparer;
			}

			public override bool Equals(CacheEntry x, CacheEntry y)
			{
				// Check an instance compared to itself as well as null compared to null.
				if(ReferenceEquals(x, y))
					return true;

				// Check an instance compared to null
				if(ReferenceEquals(x, null) || ReferenceEquals(y, null))
					return false;

				// Check and instance compared to an instance
				return KeyComparer.Equals(x.Key, y.Key) && x.Accessed == y.Accessed;
			}

			public override int GetHashCode(CacheEntry obj)
			{
				// Nulls are always zero
				if(object.Equals(obj, null))
					return 0;

				return KeyComparer.GetHashCode(obj.Key) ^ obj.Accessed.GetHashCode();
			}
		}
	}

	/// <summary>
	/// Tracks the key, value, dependencies, and cache expiry timestamps of a cached item.
	/// </summary>
	public class CacheEntry
	{
		public readonly string Key;
		public readonly object Value;
		public readonly IEnumerable<DependencyState> DependencyStates;
		public readonly DateTime Accessed;
		public readonly DateTime Updated;

		public CacheEntry(string key, object value, IEnumerable<DependencyState> dependencyStates, DateTime accessed, DateTime updated)
		{
			Key = key;
			Value = value;
			DependencyStates = dependencyStates;
			Accessed = accessed;
			Updated = updated;
		}

		public CacheEntry(CacheEntry source, string key = null, object value = null, IEnumerable<DependencyState> dependencyStates = null, DateTime? accessed = null, DateTime? updated = null)
		{
			Key = key ?? source.Key;
			Value = value ?? source.Value;
			DependencyStates = dependencyStates ?? source.DependencyStates;
			Accessed = accessed ?? source.Accessed;
			Updated = updated  ?? source.Updated;
		}
	}
}
