// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Drawing.Imaging;
using System.IO;
using System.Web.Mvc;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Controllers
{
	public class CaptchaController : Controller
	{
		readonly CaptchaStorageService CaptchaStorageService;

		public CaptchaController(CaptchaStorageService captchaStorageService)
		{
			CaptchaStorageService = captchaStorageService;
		}

		[HttpGet]
		public ActionResult Index(string scope)
		{
			var existingSecurityCode = CaptchaStorageService
				.RetrieveSecurityCode(ControllerContext.HttpContext, scope);

			var captchaService = new Captcha(existingSecurityCode, 120, 31);
			CaptchaStorageService
				.StoreSecurityCode(ControllerContext.HttpContext, captchaService.SecurityCode, scope);

			using(var stream = new MemoryStream())
			{
				captchaService
					.Image
					.Save(stream, ImageFormat.Jpeg);

				return File(stream.ToArray(), "image/jpeg");
			}
		}
	}
}
