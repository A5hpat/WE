// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
namespace AspDotNetStorefront.Caching.ObjectCaching
{
	/// <summary>
	/// Bundles up all of the information needed to instantiate a new object and its dependencies and store it in the cache.
	/// </summary>
	/// <typeparam name="TObject">The type of the object to cache</typeparam>
	/// <typeparam name="TObjectContext">The context needed to instantiate a new <see cref="TObject"> and its dependencies"/></typeparam>
	public class CachedObjectContext<TObject, TObjectContext>
	{
		public readonly CachedObjectProvider<TObject, TObjectContext>.CacheKeyBuilder KeyBuilder;
		public readonly CachedObjectProvider<TObject, TObjectContext>.CacheObjectBuilder ObjectBuilder;
		public readonly CachedObjectProvider<TObject, TObjectContext>.CacheDependencyStateBuilder DependencyStateBuilder;

		public CachedObjectContext(
			CachedObjectProvider<TObject, TObjectContext>.CacheKeyBuilder keyBuilder,
			CachedObjectProvider<TObject, TObjectContext>.CacheObjectBuilder objectBuilder,
			CachedObjectProvider<TObject, TObjectContext>.CacheDependencyStateBuilder dependencyStateBuilder)
		{
			KeyBuilder = keyBuilder;
			ObjectBuilder = objectBuilder;
			DependencyStateBuilder = dependencyStateBuilder;
		}
	}

	/// <summary>
	/// Builds <see cref="CachedObjectContext{TObject, TObjectContext}" /> instances.
	/// </summary>
	/// <typeparam name="TObject">The type of the object to cache</typeparam>
	/// <typeparam name="TObjectContext">The context needed to instantiate a new <see cref="TObject"> and its dependencies"/></typeparam>
	public interface ICachedObjectContextBuilder<TObject, TObjectContext>
	{
		CachedObjectContext<TObject, TObjectContext> CreateContext();
	}
}
