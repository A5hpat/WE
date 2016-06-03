// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Diagnostics;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Caching.ObjectCaching.Dependency
{
	/// <summary>
	/// Handles dependencies on AppConfigs that change in any way.
	/// </summary>
	public class AppConfigDependencyStateManager : IDependencyStateManager
	{
		readonly AppConfigProvider AppConfigProvider;
		readonly HashProvider HashProvider;

		public AppConfigDependencyStateManager(AppConfigProvider appConfigProvider, HashProvider hashProvider)
		{
			AppConfigProvider = appConfigProvider;
			HashProvider = hashProvider;
		}

		public DependencyState GetState(DependencyStateContext context)
		{
			if(!(context is AppConfigDependencyStateContext))
				return null;

			// Get the requested AppConfig
			var appConfigContext = (AppConfigDependencyStateContext)context;
			var appConfig = AppConfigProvider.GetAppConfig(appConfigContext.Name, appConfigContext.StoreId, cascadeToDefault: true);

			// There are many cases to consider: creation, deletion, and modification; and default 
			// store fallbacks in all three cases.

			// If the requested AppConfig doesn't exist, create a state that doesn't include the value
			if(appConfig == null)
				return new DependencyState(
					context: context,
					state: HashProvider.Hash(new object[]
						{
							appConfigContext.Name,
							appConfigContext.StoreId,
						}));

			// Create a state that includes both the requested and actual values.
			return new DependencyState(
				context: context,
				state: HashProvider.Hash(new object[]
					{
						appConfigContext.Name,		// The name of the AppConfig
						appConfigContext.StoreId,	// The requested store ID
						appConfig.StoreId,			// The actual store ID, in case the requested
													// store ID used the default store's value
						appConfig.UpdatedOn,		// The timestamp of the last change
					}));
		}

		public bool? HasStateChanged(DependencyState establishedState)
		{
			if(!(establishedState.Context is AppConfigDependencyStateContext))
				return null;

			var currentState = GetState(establishedState.Context);
			var stateChanged = currentState.State != establishedState.State;

			if(stateChanged && AppConfigProvider.GetAppConfigValue<bool>("ObjectCacheDebuggingEnabled"))
				Debug.WriteLine(string.Format(
					"ObjCache - AppConfig Changed - {0} @ Store {1}", 
					((AppConfigDependencyStateContext)establishedState.Context).Name,
					((AppConfigDependencyStateContext)establishedState.Context).StoreId));

			return stateChanged;
		}
	}

	[DebuggerDisplay("AppConfig: {Name}@{StoreId}")]
	public class AppConfigDependencyStateContext : DependencyStateContext
	{
		public readonly string Name;
		public readonly int StoreId;

		public AppConfigDependencyStateContext(string name, int storeId)
		{
			Name = name;
			StoreId = storeId;
		}
	}
}
