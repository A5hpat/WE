// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using AspDotNetStorefront.Routing;

namespace AspDotNetStorefrontCore
{
	public static class UrlHelperExtensions
	{
		public static string BuildProductLink(this UrlHelper url, int id, string seName = null, IDictionary<string, object> additionalRouteValues = null)
		{
			var searchEngineProvider = DependencyResolver.Current.GetService<ISearchEngineNameProvider>();

			var verifiedSeName = string.IsNullOrEmpty(seName)
				? searchEngineProvider.EnsureDatabaseObjectSeName("Product", id)
				: searchEngineProvider.GenerateSeName(seName);

			var routeValues = new RouteValueDictionary
				{
					{ RouteDataKeys.Id, id },
					{ RouteDataKeys.SearchEngineName, verifiedSeName }
				};

			var mergedRouteValues = MergeRouteValues(routeValues, additionalRouteValues);

			return url.Action(
				actionName: ActionNames.Detail,
				controllerName: ControllerNames.Product,
				routeValues: new RouteValueDictionary(mergedRouteValues));
		}

		public static string BuildEntityLink(this UrlHelper url, string type, int id, string seName, IDictionary<string, object> additionalRouteValues = null)
		{
			var searchEngineProvider = DependencyResolver.Current.GetService<ISearchEngineNameProvider>();

			var verifiedSeName = string.IsNullOrEmpty(seName)
				? searchEngineProvider.EnsureDatabaseObjectSeName(type, id)
				: searchEngineProvider.GenerateSeName(seName);

			var routeValues = new RouteValueDictionary
				{
					{ RouteDataKeys.EntityType, type },
					{ RouteDataKeys.Id, id },
					{ RouteDataKeys.SearchEngineName, verifiedSeName }
				};

			var mergedRouteValues = MergeRouteValues(routeValues, additionalRouteValues);
			
			return url.Action(
				actionName: ActionNames.Detail,
				controllerName: ControllerNames.Entity,
				routeValues: new RouteValueDictionary(mergedRouteValues));
		}

		public static string BuildTopicLink(this UrlHelper url, string name, bool? disableTemplate = null, IDictionary<string, object> additionalRouteValues = null)
		{
			var routeValues = new RouteValueDictionary
				{ { RouteDataKeys.Name, name } };

			if(disableTemplate != null)
				routeValues.Add(RouteDataKeys.DisableTemplate, disableTemplate);

			var mergedRouteValues = MergeRouteValues(routeValues, additionalRouteValues);

			return url.Action(
				actionName: ActionNames.Detail,
				controllerName: ControllerNames.Topic,
				routeValues: new RouteValueDictionary(mergedRouteValues));
		}

		public static string BuildCheckoutLink(this UrlHelper url, int? errorId = null, IDictionary<string, object> additionalRouteValues = null)
		{
			var routeValues = new RouteValueDictionary
				{
					{ RouteDataKeys.ErrorMessage, errorId },
				};

			var mergedRouteValues = MergeRouteValues(routeValues, additionalRouteValues);

			return url.Action(
				actionName: ActionNames.Index,
				controllerName: ControllerNames.Checkout,
				routeValues: new RouteValueDictionary(mergedRouteValues));
		}

		static IDictionary<string, object> MergeRouteValues(IDictionary<string, object> baseRouteValues, IDictionary<string, object> additionalRouteValues)
		{
			if(additionalRouteValues == null)
				return baseRouteValues;

			// Union baseRouteValues into additionalRouteValues, allowing additionalRouteValues to override anything in baseRouteValues
			return additionalRouteValues
				.Union(baseRouteValues, new KeyEqualityComparer<string, object>(StringComparer.OrdinalIgnoreCase))
				.ToDictionary(
					kvp => kvp.Key,
					kvp => kvp.Value);
		}

		public static string SkinUrl(this UrlHelper url, string path)
		{
			return url.Content(AppRelativeSkinUrl(url, path));
		}

		public static string AppRelativeSkinUrl(this UrlHelper url, string path)
		{
			var skinId = (int)url.RequestContext.RouteData.Values[RouteDataKeys.SkinId];
			var skinProvider = new SkinProvider();
			var baseSkinUrl = string.Format("~/skins/{0}/", skinProvider.GetSkinNameById(skinId));
			var virtualPath = VirtualPathUtility.Combine(baseSkinUrl, path);
			return virtualPath;
		}

		public static string MakeSafeReturnUrl(this UrlHelper url, string returnUrl, string defaultUrl = "~/")
		{
			Uri effectiveReturnUri;
			if(string.IsNullOrEmpty(returnUrl) || !Uri.TryCreate(returnUrl, UriKind.Relative, out effectiveReturnUri))
				effectiveReturnUri = new Uri(defaultUrl, UriKind.Relative);

			return effectiveReturnUri.ToString();
		}

		public static string ActionForStore(this UrlHelper url, string actionName, string controllerName, int storeId, RouteValueDictionary routeValues = null, string scheme = "http")
		{
			// Get the target store's base Url
			var storeBaseUrl = GetStoreBaseUrl(url, scheme, storeId);

			// Get the action from the current store
			var actionAndQueryString = url.Action(actionName, controllerName, routeValues);

			// Remove the virtual directory from the action link on the current store so when we build up our link to the other store it does not include the virtual directory of the current store.
			var currentStoreVirtualDirectory = url.RequestContext.HttpContext.Request.ApplicationPath;
			if(currentStoreVirtualDirectory != "/"
				&& actionAndQueryString.StartsWithIgnoreCase(currentStoreVirtualDirectory))
				actionAndQueryString = actionAndQueryString.Substring(currentStoreVirtualDirectory.Length, actionAndQueryString.Length - currentStoreVirtualDirectory.Length);

			return string.Format("{0}{1}", storeBaseUrl, actionAndQueryString);
		}

		public static string AdminLinkForStore(this UrlHelper url, string adminPage, int storeId, string scheme = "http")
		{
			// Get the target store's base Url
			var storeBaseUrl = GetStoreBaseUrl(url, scheme, storeId)
				.TrimEnd('/');

			var storeAdminDirectory = AppLogic.AppConfig("AdminDir", storeId, true)
				.Replace("/", string.Empty);

			return string.Format("{0}/{1}/{2}", storeBaseUrl, storeAdminDirectory, adminPage);
		}

		static string GetStoreBaseUrl(UrlHelper url, string scheme, int storeId)
		{
			// Is the site running in development, staging, or live?
			var currentEnvironment = Store.GetEnvironment(currentDomain: url.RequestContext.HttpContext.Request.Url.Host);

			// Get the domain for the requested store based on the environment
			var storeDomain = Store.GetStoreDomainByEnvironment(currentEnvironment, storeId);

			// Get the port for the requested store based on the environment
			var storePort = scheme.EqualsIgnoreCase("https")
				? string.Empty
				: Store.GetStorePortByEnvironment(currentEnvironment, storeId);
			
			if(!string.IsNullOrEmpty(storePort))
				storePort = string.Format(":{0}", storePort);

			// Get the application path or virtual directory for the requested store based on the environment
			var virtualDirectory = Store.GetStoreDirectoryByEnvironment(currentEnvironment, storeId);

			// Build up and return the root store url for the requested store
			return string.Format("{0}://{1}{2}{3}", scheme, storeDomain, storePort, virtualDirectory);
		}
	}
}
