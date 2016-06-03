// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AspDotNetStorefrontCore;
using AspDotNetStorefront.Routing;

namespace AspDotNetStorefront.Filters
{
	public class SkinIdRouteDataFilterAttribute : FilterAttribute, IActionFilter
	{
		public void OnActionExecuting(ActionExecutingContext filterContext)
		{
			if(filterContext.RouteData.Values.ContainsKey(RouteDataKeys.SkinId))
				return;

			var customer = Customer.Current;
			var skinName = SkinProvider.DefaultSkinName;
			var skinProvider = new SkinProvider();
			var skinId = AppLogic.GetStoreSkinID(AppLogic.StoreID());

			// Have to call GetPropertyValue once before you actually need it to initialize the PropertyValues collection
			if(HttpContext.Current.Profile != null)
				HttpContext.Current.Profile.GetPropertyValue("SkinID");

			// Skin querystring
			var skinNameQuerystring = CommonLogic.QueryStringCanBeDangerousContent("skin");
			if(!String.IsNullOrEmpty(skinNameQuerystring))
			{
				skinName = skinNameQuerystring;
				skinId = skinProvider.GetSkinIdByName(skinName);

				// Customer has a querystring so save this to the profile.
				if(HttpContext.Current.Profile != null)
					HttpContext.Current.Profile.SetPropertyValue("SkinID", skinId.ToString());
			}
			// SkinId querystring 
			else if(CommonLogic.QueryStringUSInt("skinid") > 0)
			{
				skinId = CommonLogic.QueryStringUSInt("skinid");

				// Customer has a querystring so save this to the profile.
				if(HttpContext.Current.Profile != null)
					HttpContext.Current.Profile.SetPropertyValue("SkinID", skinId.ToString());
			}
			// Check to see if we are previewing the skin
			else if(CommonLogic.QueryStringUSInt("previewskinid") > 0)
			{
				skinId = CommonLogic.QueryStringUSInt("previewskinid");

				//Customer has a preview querystring so save this to the profile.
				if(HttpContext.Current.Profile != null)
					HttpContext.Current.Profile.SetPropertyValue("PreviewSkinID", skinId.ToString());
			}
			// Use the preview profile value if we have one
			else if(HttpContext.Current.Profile != null
				&& HttpContext.Current.Profile.PropertyValues["PreviewSkinID"] != null
				&& CommonLogic.IsInteger(HttpContext.Current.Profile.GetPropertyValue("PreviewSkinID").ToString()))
			{
				int skinFromProfile = int.Parse(HttpContext.Current.Profile.GetPropertyValue("PreviewSkinID").ToString());
				if(skinFromProfile > 0)
				{
					skinId = skinFromProfile;
				}
			}
			// Pull the skinid from the current profile
			else if(HttpContext.Current.Profile != null && CommonLogic.IsInteger(HttpContext.Current.Profile.GetPropertyValue("SkinID").ToString()))
			{
				int skinFromProfile = int.Parse(HttpContext.Current.Profile.GetPropertyValue("SkinID").ToString());
				if(skinFromProfile > 0)
				{
					skinId = skinFromProfile;
				}
			}

			// Now save the skinID to the customer record.  This is not used OOB.
			if(customer.SkinID != skinId)
			{
				customer.SkinID = skinId;
				customer.UpdateCustomer(new SqlParameter[] { new SqlParameter("SkinID", skinId) });
			}

			filterContext.RouteData.Values.Add(RouteDataKeys.SkinId, skinId);
		}

		public void OnActionExecuted(ActionExecutedContext filterContext)
		{
		}
	}
}
