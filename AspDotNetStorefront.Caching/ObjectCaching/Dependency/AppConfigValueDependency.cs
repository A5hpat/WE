// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Diagnostics;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Caching.ObjectCaching.Dependency
{
	/// <summary>
	/// Handles dependencies on an AppConfig values meeting a specified predicate.
	/// </summary>
	public class AppConfigValueDependencyStateManager : IDependencyStateManager
	{
		readonly AppConfigProvider AppConfigProvider;
		readonly HashProvider HashProvider;

		public AppConfigValueDependencyStateManager(AppConfigProvider appConfigProvider, HashProvider hashProvider)
		{
			AppConfigProvider = appConfigProvider;
			HashProvider = hashProvider;
		}

		public DependencyState GetState(DependencyStateContext context)
		{
			if(!(context is AppConfigValueDependencyStateContext))
				return null;

			// Get the requested AppConfig
			var appConfigValueContext = (AppConfigValueDependencyStateContext)context;
			var appConfig = AppConfigProvider.GetAppConfig(appConfigValueContext.Name, appConfigValueContext.StoreId, cascadeToDefault: true);

			// If the AppConfig doesn't exist or it doesn't match predicate, return a 
			// state that will be different every time. The required behavior is that any AppConfig
			// that doesn't match the predicate should cause the dependecy to fail.
			if(appConfig == null || !appConfigValueContext.Predicate(appConfig.ConfigValue))
			{
				// Generate a random long
				var rng = new Random();
				var rngBuffer = new byte[8];
				rng.NextBytes(rngBuffer);
				var failedHash = BitConverter.ToInt64(rngBuffer, 0);

				// Return that as the state
				return new DependencyState(
					context: context,
					state: failedHash);
			}

			// If the AppConfig exists and matches the predicate, return a state that tracks any changes in the AppConfig.
			return new DependencyState(
				context: context,
				state: HashProvider.Hash(new object[]
					{
						appConfigValueContext.Name,
						appConfigValueContext.StoreId,
						appConfig.StoreId,
						appConfig.UpdatedOn.Ticks,
					}));
		}

		public bool? HasStateChanged(DependencyState establishedState)
		{
			if(!(establishedState.Context is AppConfigValueDependencyStateContext))
				return null;

			var currentState = GetState(establishedState.Context);
			var stateChanged = currentState.State != establishedState.State;

			if(stateChanged && AppConfigProvider.GetAppConfigValue<bool>("ObjectCacheDebuggingEnabled"))
				Debug.WriteLine(string.Format(
					"ObjCache - AppConfig Value Predicate Changed - {0} @ Store {1}",
					((AppConfigValueDependencyStateContext)establishedState.Context).Name,
					((AppConfigValueDependencyStateContext)establishedState.Context).StoreId));

			return stateChanged;
		}
	}

	[DebuggerDisplay("AppConfig: {Name}@{StoreId}")]
	public class AppConfigValueDependencyStateContext : DependencyStateContext
	{
		public readonly string Name;
		public readonly int StoreId;
		public readonly Predicate<string> Predicate;

		public AppConfigValueDependencyStateContext(string name, int storeId, Predicate<string> predicate)
		{
			Name = name;
			StoreId = storeId;
			Predicate = predicate;
		}
	}
}
