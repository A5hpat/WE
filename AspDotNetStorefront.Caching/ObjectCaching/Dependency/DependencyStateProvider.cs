// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AspDotNetStorefront.Caching.ObjectCaching.Dependency
{
	/// <summary>
	/// A wrapper for all <see cref="IDependencyStateManager"/> instances.
	/// </summary>
	public class DependencyStateProvider
	{
		readonly IEnumerable<IDependencyStateManager> Managers;

		public DependencyStateProvider(IEnumerable<IDependencyStateManager> managers)
		{
			Managers = managers ?? Enumerable.Empty<IDependencyStateManager>();
		}

		public DependencyState GetState(DependencyStateContext context)
		{
			// Delegate down to all managers, returning the first non-null result
			return Managers
				.Select(manager => manager.GetState(context))
				.Where(state => state != null)
				.FirstOrDefault();
		}

		public bool HasStateChanged(DependencyState establishedState)
		{
			// Delegate down to all managers, returning the first non-null result
			return Managers
				.Select(manager => manager.HasStateChanged(establishedState))
				.Where(stateChanged => stateChanged != null)
				.FirstOrDefault()
				?? true;
		}
	}

	[DebuggerDisplay("{Context} : {State}")]
	public class DependencyState
	{
		public readonly DependencyStateContext Context;
		public readonly long State;

		public DependencyState(DependencyStateContext context, long state)
		{
			Context = context;
			State = state;
		}
	}

	public abstract class DependencyStateContext
	{
	}

	public interface IDependencyStateManager
	{
		DependencyState GetState(DependencyStateContext context);

		bool? HasStateChanged(DependencyState establishedState);
	}
}
