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
	/// This class wraps <see cref="CacheDependencyEvaluator"/> with a generic accessor of
	/// <typeparamref name="TObject"/>. This building block is used to compose a strongly-typed
	/// cache accessor.
	/// </summary>
	/// <typeparam name="TObject">The type of the object to cache</typeparam>
	/// <typeparam name="TObjectContext">The type of the context needed to instantiate a new 
	/// <see cref="TObject"> and its dependencies"/></typeparam>
	public class CachedObjectProvider<TObject, TObjectContext>
	{
		public delegate string CacheKeyBuilder(TObjectContext context);
		public delegate TObject CacheObjectBuilder(TObjectContext context);
		public delegate IEnumerable<DependencyStateContext> CacheDependencyStateBuilder(TObjectContext context);

		readonly CacheDependencyEvaluator CacheDependencyEvaluator;
		readonly DependencyStateProvider DependencyStateProvider;
		readonly ICachedObjectContextBuilder<TObject, TObjectContext> CachedObjectContextBuilder;

		public CachedObjectProvider(CacheDependencyEvaluator cacheDependencyEvaluator, DependencyStateProvider dependencyStateProvider, ICachedObjectContextBuilder<TObject, TObjectContext> cachedObjectContextBuilder)
		{
			CacheDependencyEvaluator = cacheDependencyEvaluator;
			DependencyStateProvider = dependencyStateProvider;
			CachedObjectContextBuilder = cachedObjectContextBuilder;
		}

		public TObject Get(TObjectContext objectContext)
		{
			// Use the provided context to generate the context needed for the CacheDependencyEvaluator.
			var cachedObjectContext = CachedObjectContextBuilder.CreateContext();
			var key = cachedObjectContext.KeyBuilder(objectContext);

			return CacheDependencyEvaluator.GetOrUpdateCache(
				key,
				new Lazy<Tuple<TObject, IEnumerable<DependencyState>>>(
					() => new Tuple<TObject, IEnumerable<DependencyState>>(
						cachedObjectContext.ObjectBuilder(objectContext),
						cachedObjectContext.DependencyStateBuilder(objectContext)
							.Select(dependencyStateContext => DependencyStateProvider.GetState(dependencyStateContext))
							.ToArray())));
		}
	}
}
