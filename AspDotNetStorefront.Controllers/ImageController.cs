// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web.Mvc;
using AspDotNetStorefront.Models;
using System.Web;

namespace AspDotNetStorefront.Controllers
{
	public class ImageController : Controller
	{
		public ActionResult PopUp(string imagePath)
		{

			if(!imagePath.StartsWith("/"))
				imagePath = string.Format("/{0}", imagePath);

			// Validate the imagePath parameter
			Uri imageUri;
			if(!Uri.TryCreate(imagePath, UriKind.Relative, out imageUri))
				throw new HttpException(404, null);

			return View(new PopUpImageViewModel(imageUri.ToString()));
		}
	}
}
