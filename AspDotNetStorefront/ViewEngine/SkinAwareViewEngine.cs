// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Web.Mvc;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.ViewEngine
{
	public class SkinAwareViewEngine : RazorViewEngine
	{
		readonly ISkinProvider SkinProvider;

		public SkinAwareViewEngine()
		{
			SkinProvider = new SkinProvider();

			AreaViewLocationFormats = new string[] {
				"~/Areas/{2}/Skins/(!SkinName!)/Views/{1}/{0}.cshtml",
				"~/Areas/{2}/Skins/(!SkinName!)/Views/{1}/{0}.vbhtml",
				"~/Areas/{2}/Skins/(!SkinName!)/Views/Shared/{0}.cshtml",
				"~/Areas/{2}/Skins/(!SkinName!)/Views/Shared/{0}.vbhtml",

				"~/Areas/{2}/Views/{1}/{0}.cshtml",
				"~/Areas/{2}/Views/{1}/{0}.vbhtml",
				"~/Areas/{2}/Views/Shared/{0}.cshtml",
				"~/Areas/{2}/Views/Shared/{0}.vbhtml",
			};

			AreaMasterLocationFormats = new string[] {
				"~/Areas/{2}/Skins/(!SkinName!)/Views/{1}/{0}.cshtml",
				"~/Areas/{2}/Skins/(!SkinName!)/Views/{1}/{0}.vbhtml",
				"~/Areas/{2}/Skins/(!SkinName!)/Views/Shared/{0}.cshtml",
				"~/Areas/{2}/Skins/(!SkinName!)/Views/Shared/{0}.vbhtml",

				"~/Areas/{2}/Views/{1}/{0}.cshtml",
				"~/Areas/{2}/Views/{1}/{0}.vbhtml",
				"~/Areas/{2}/Views/Shared/{0}.cshtml",
				"~/Areas/{2}/Views/Shared/{0}.vbhtml",
			};

			AreaPartialViewLocationFormats = new string[] {
				"~/Areas/{2}/Skins/(!SkinName!)/Views/{1}/{0}.cshtml",
				"~/Areas/{2}/Skins/(!SkinName!)/Views/{1}/{0}.vbhtml",
				"~/Areas/{2}/Skins/(!SkinName!)/Views/Shared/{0}.cshtml",
				"~/Areas/{2}/Skins/(!SkinName!)/Views/Shared/{0}.vbhtml",

				"~/Areas/{2}/Views/{1}/{0}.cshtml",
				"~/Areas/{2}/Views/{1}/{0}.vbhtml",
				"~/Areas/{2}/Views/Shared/{0}.cshtml",
				"~/Areas/{2}/Views/Shared/{0}.vbhtml",
			};

			ViewLocationFormats = new string[] {
				"~/Skins/(!SkinName!)/Views/{1}/{0}.cshtml",
				"~/Skins/(!SkinName!)/Views/{1}/{0}.vbhtml",
				"~/Skins/(!SkinName!)/Views/Shared/{0}.cshtml",
				"~/Skins/(!SkinName!)/Views/Shared/{0}.vbhtml",

				"~/Views/{1}/{0}.cshtml",
				"~/Views/{1}/{0}.vbhtml",
				"~/Views/Shared/{0}.cshtml",
				"~/Views/Shared/{0}.vbhtml",
			};

			MasterLocationFormats = new string[] {
				"~/Skins/(!SkinName!)/Views/{1}/{0}.cshtml",
				"~/Skins/(!SkinName!)/Views/{1}/{0}.vbhtml",
				"~/Skins/(!SkinName!)/Views/Shared/{0}.cshtml",
				"~/Skins/(!SkinName!)/Views/Shared/{0}.vbhtml",

				"~/Views/{1}/{0}.cshtml",
				"~/Views/{1}/{0}.vbhtml",
				"~/Views/Shared/{0}.cshtml",
				"~/Views/Shared/{0}.vbhtml",
			};

			PartialViewLocationFormats = new string[] {
				"~/Skins/(!SkinName!)/Views/{1}/{0}.cshtml",
				"~/Skins/(!SkinName!)/Views/{1}/{0}.vbhtml",
				"~/Skins/(!SkinName!)/Views/Shared/{0}.cshtml",
				"~/Skins/(!SkinName!)/Views/Shared/{0}.vbhtml",

				"~/Views/{1}/{0}.cshtml",
				"~/Views/{1}/{0}.vbhtml",
				"~/Views/Shared/{0}.cshtml",
				"~/Views/Shared/{0}.vbhtml",
			};
		}

		protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath)
		{
			var skinId = (int)controllerContext.RouteData.Values["SkinId"];
			var skinName = SkinProvider.GetSkinNameById(skinId);
			return base.CreateView(controllerContext, viewPath.Replace("(!SkinName!)", skinName), masterPath.Replace("(!SkinName!)", skinName));
		}

		protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath)
		{
			var skinId = (int)controllerContext.RouteData.Values["SkinId"];
			var skinName = SkinProvider.GetSkinNameById(skinId);
			return base.CreatePartialView(controllerContext, partialPath.Replace("(!SkinName!)", skinName));
		}

		protected override bool FileExists(ControllerContext controllerContext, string virtualPath)
		{
			var skinId = (int)controllerContext.RouteData.Values["SkinId"];
			var skinName = SkinProvider.GetSkinNameById(skinId);
			return base.FileExists(controllerContext, virtualPath.Replace("(!SkinName!)", skinName));
		}
	}
}
