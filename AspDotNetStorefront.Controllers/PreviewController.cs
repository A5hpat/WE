// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web.Mvc;
using AspDotNetStorefront.Models;
using AspDotNetStorefrontCore;
using AspDotNetStorefront.Routing;

namespace AspDotNetStorefront.Controllers
{
	public class PreviewController : Controller
	{
		[ChildActionOnly]
		public ActionResult PreviewBar()
		{
			var profile = HttpContext.Profile;

			//return early if we are not previewing
			if(profile == null 
				|| profile.PropertyValues["PreviewSkinID"] == null 
				|| !CommonLogic.IsInteger(profile.GetPropertyValue("PreviewSkinID").ToString()))
				return Content(String.Empty);

			var customer = HttpContext.GetCustomer();

			var previewSkinId = int.Parse(HttpContext.Profile.GetPropertyValue("PreviewSkinID").ToString());
			var previewSkin = new SkinProvider().GetSkinById(previewSkinId);
			var previewSkinName = !String.IsNullOrEmpty(previewSkin.DisplayName) 
				? previewSkin.DisplayName 
				: previewSkin.Name;

			var previewBarViewModel = new PreviewBarViewModel
			{
				PreviewText = String.Format("You are previewing the <span class=\"preview - skin - name\">{0}</span> skin.  Click the button to the right to end the preview.", previewSkinName),
				PreviewBarCssPath = "~/App_Templates/Admin_Default/previewstyles.css"
			};

			return PartialView(ViewNames.PreviewBarPartial, previewBarViewModel);
		}

		public ActionResult EndPreview()
		{
			//remove the profile Value
			HttpContext.Profile.SetPropertyValue("PreviewSkinID", "");

			//redirect to the homepage without the querystring
			return RedirectToAction(ActionNames.Index, ControllerNames.Home);
		}

	}
}
