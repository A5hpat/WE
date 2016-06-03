// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web;
using System.Web.Mvc;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Controllers
{
	public class WatermarkController : Controller
	{
		[HttpGet]
		public ActionResult Index(string imageUrl, string imageSize)
		{
			Response.Cache.SetCacheability(HttpCacheability.NoCache);

			var imagePath = GetImagePath(imageUrl);
			if(string.IsNullOrEmpty(imagePath))
				return new EmptyResult();

			using(var image = GetImage(
				imagePath: imagePath,
				copyrightImageUrl: GetCopyrightImageUrl(imageSize),
				copyrightText: GetCopyrightText()))
			{
				if(image == null)
					return new EmptyResult();

				var imageExtension = GetImageExtension(imagePath);
				var imageFormat = GetImageFormat(imageExtension);

				return File(
					fileContents: GetImageBytes(image, imageFormat),
					contentType: GetContentType(imageExtension));
			}
		}

		string GetImagePath(string imageUrl)
		{
			var imagePath = Request.MapPath(imageUrl);
			return CommonLogic.FileExists(imagePath)
				? imagePath
				: null;
		}

		string GetImageExtension(string imagePath)
		{
			return Path
				.GetExtension(imagePath)
				.ToLower();
		}

		string GetCopyrightImageUrl(string imageSize)
		{
			switch(imageSize.ToLower())
			{
				default:
				case "icon":
					return AppLogic
						.AppConfig("Watermark.CopyrightImage.Icon")
						.TrimStart('/');

				case "medium":
					return AppLogic
						.AppConfig("Watermark.CopyrightImage.Medium")
						.TrimStart('/');
				case "large":
					return AppLogic
						.AppConfig("Watermark.CopyrightImage.Large")
						.TrimStart('/');
			}
		}

		string GetCopyrightText()
		{
			var copyrightText = AppLogic.AppConfig("Watermark.CopyrightText");
			return !string.IsNullOrEmpty(copyrightText)
				? copyrightText
				: AppLogic.AppConfig("StoreName");
		}

		string GetContentType(string imageExtension)
		{
			switch(imageExtension)
			{
				default:
				case ".jpg":
				case ".jpeg":
					return "image/jpeg";

				case ".gif":
					return "image/gif";

				case ".png":
					return "image/png";

				case ".bmp":
					return "image/bmp";
			}
		}

		Image GetImage(string imagePath, string copyrightImageUrl, string copyrightText)
		{
			var image = CommonLogic.LoadImage(imagePath);

			// If copyright image and text are not specified, return the original image untouched.
			if(string.IsNullOrEmpty(copyrightImageUrl)
				&& string.IsNullOrEmpty(copyrightText))
				return image;

			// Favor the copyright image over text.
			if(!string.IsNullOrEmpty(copyrightImageUrl) && CommonLogic.FileExists(copyrightImageUrl))
				copyrightText = string.Empty;

			try
			{
				return CommonLogic.AddWatermark(image, copyrightText, copyrightImageUrl);
			}
			catch(Exception ex)
			{
				SysLog.LogException(ex, MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
				return null;
			}
		}

		ImageFormat GetImageFormat(string imageExtension)
		{
			switch(imageExtension)
			{
				default:
				case ".jpg":
				case ".jpeg":
					return ImageFormat.Jpeg;

				case ".gif":
					return ImageFormat.Gif;

				case ".png":
					return ImageFormat.Png;

				case ".bmp":
					return ImageFormat.Bmp;
			}
		}

		byte[] GetImageBytes(Image image, ImageFormat imageFormat)
		{
			using(var imageStream = new MemoryStream())
			{
				image.Save(
					stream: imageStream,
					format: imageFormat);

				return imageStream.ToArray();
			}
		}
	}
}
