// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace AspDotNetStorefrontCore
{
	public class ImageResize
	{
		static Hashtable SplitConfig(string configValue)
		{
			char[] trimChars = { ' ', ';' };

			var imgAppConfigParam = AppLogic.AppConfig(configValue).TrimEnd(trimChars);
			var mapEntries = new Hashtable();

			// Split the input string
			var arEntries = imgAppConfigParam.Split(';');
			for(var i = 0; i < arEntries.Length; ++i)
			{
				// Split this entry in to key and value
				var arPieces = arEntries[i].Split(':');

				try
				{
					if(arPieces.Length > 1)
					{
						// Add this key/value pair (trimmed and lowercase) to our map
						mapEntries[arPieces[0].Trim().ToLower()] = arPieces[1].Trim().ToLower();
					}
				}
				catch { }
			}

			return mapEntries;
		}

		static Hashtable SplitConfig(string objectName, string size)
		{
			char[] trimChars = { ' ', ';' };
			var imgAppConfigParam = AppLogic.AppConfig(objectName + "Img_" + size).TrimEnd(trimChars);
			var mapEntries = new Hashtable();

			// Split the input string
			var arEntries = imgAppConfigParam.Split(';');
			for(var i = 0; i < arEntries.Length; ++i)
			{
				// Split this entry in to key and value
				var arPieces = arEntries[i].Split(':');

				try
				{
					if(arPieces.Length > 1)
					{
						// Add this key/value pair (trimmed and lowercase) to our map
						mapEntries[arPieces[0].Trim().ToLower()] = arPieces[1].Trim().ToLower();
					}
				}
				catch { }
			}

			return mapEntries;
		}

		static Hashtable SplitConfig(string objectName, string size, string properties)
		{
			char[] trimChars = { ' ', ';', ',' };
			var imgParams = properties.TrimEnd(trimChars);
			var mapEntries = new Hashtable();

			// Split the input string
			var arEntries = imgParams.Split(';');
			for(var i = 0; i < arEntries.Length; ++i)
			{
				// Split this entry in to key and value
				var arPieces = arEntries[i].Split(':');

				try
				{
					if(arPieces.Length > 1)
					{
						// Add this key/value pair (trimmed and lowercase) to our map
						mapEntries[arPieces[0].Trim().ToLower()] = arPieces[1].Trim().ToLower();
					}
				}
				catch { }
			}

			return mapEntries;
		}

		public static void ResizeEntityOrObject(string objectName, string tempFileName, string fileName, string size, string content)
		{
			Hashtable spltConfig = new Hashtable();
			using(var imageToResize = Image.FromFile(tempFileName, true))
			{
				OrientImage(imageToResize);

				switch(objectName.ToUpperInvariant())
				{
					case "CATEGORY":
						spltConfig = SplitConfig("Category", size);
						break;
					case "MANUFACTURER":
						spltConfig = SplitConfig("Manufacturer", size);
						break;
					case "SECTION":
						spltConfig = SplitConfig("Section", size);
						break;
					case "DISTRIBUTOR":
						spltConfig = SplitConfig("Distributor", size);
						break;
					case "PRODUCT":
						spltConfig = SplitConfig("Product", size);
						break;
					case "VARIANT":
						spltConfig = SplitConfig("Variant", size);
						break;
					default:
						return;
				}

				ResizePhoto(spltConfig, imageToResize, fileName, tempFileName, size, content);
			}
		}

		public static void ResizeEntityOrObject(string objectName, string tempFileName, string fileName, string eoID, string size, string content, string imageParameters, bool useAppConfigs)
		{
			var splitConfigParams = new Hashtable();
			var splitConfigAppConfigs = new Hashtable();
			var imageToResize = Image.FromFile(tempFileName, true);

			OrientImage(imageToResize);

			splitConfigParams = SplitConfig(objectName.ToUpperInvariant(), size, imageParameters);

			if(useAppConfigs)
				splitConfigAppConfigs = SplitConfig(objectName, size);

			ResizePhoto(splitConfigParams, splitConfigAppConfigs, imageToResize, fileName, tempFileName, size, content);

			eoID = eoID.Replace(".", "").Replace("jpg", "").Replace("jpeg", "").Replace("gif", "").Replace("png", "");

			if(size.Equals("large", StringComparison.InvariantCultureIgnoreCase))
				CreateOthersFromLarge(objectName, tempFileName, eoID, content);

			if(size.Equals("medium", StringComparison.InvariantCultureIgnoreCase))
				MakeMicroPic(eoID, tempFileName, imageParameters, size);
		}

		private static Image OrientImage(Image image)
		{
			// Sometimes images can upload with the improper orientation when the image is taller than it is wide
			// To fix this we can read the image's Exchangeable Image File Format (EXIF) data and handle orientation
			foreach(var prop in image.PropertyItems)
			{
				if((prop.Id == 0x0112 || prop.Id == 274))
				{
					var value = (int)prop.Value[0];
					switch(value)
					{
						case 6: // indicates the top of the image is pointing left
							image.RotateFlip(RotateFlipType.Rotate90FlipNone);
							break;
						case 8: // indicates the top of the image is pointing right
							image.RotateFlip(RotateFlipType.Rotate270FlipNone);
							break;
						case 3: // indicates the top of the image is pointing down
							image.RotateFlip(RotateFlipType.Rotate180FlipNone);
							break;
					}
				}
			}
			return image;
		}

		static void ResizePhoto(Hashtable splitConfigParams, Hashtable splitConfigAppConfigs, Image originalPhoto, string fileName, string tempFilename, string size, string content)
		{
			if(splitConfigAppConfigs.ContainsKey("resize")
				&& splitConfigAppConfigs["resize"].ToString() == "false")
			{
				originalPhoto.Save(fileName);
				return;
			}

			var resizedWidth = AppLogic.AppConfigNativeInt("DefaultWidth_" + size);
			var resizedHeight = AppLogic.AppConfigNativeInt("DefaultHeight_" + size);
			var resizedQuality = AppLogic.AppConfigNativeInt("DefaultQuality");

			var stretchMe = AppLogic.AppConfig("DefaultStretch");
			var cropMe = AppLogic.AppConfig("DefaultCrop");
			var cropV = AppLogic.AppConfig("DefaultCropVertical");
			var cropH = AppLogic.AppConfig("DefaultCropHorizontal");
			var fillColor = AppLogic.AppConfig("DefaultFillColor");

			var sourceWidth = originalPhoto.Width;
			var sourceHeight = originalPhoto.Height;
			var sourceX = 0;
			var sourceY = 0;
			// we will extend 2 pixels on all sides to avoid the border bug
			// we could use InterpolationMode.NearestNeighbor instead of
			// InterpolationMode.HighQualityBicubic but not without sacrificing quality
			var destX = -2;
			var destY = -2;

			var nPercent = 0f;
			var nPercentW = 0f;
			var nPercentH = 0f;

			var destWidth = 0;
			var destHeight = 0;

			if(splitConfigAppConfigs.Count > 0)
			{
				if(splitConfigAppConfigs.ContainsKey("width")
					&& CommonLogic.IsInteger(splitConfigAppConfigs["width"].ToString()))
					resizedWidth = int.Parse(splitConfigAppConfigs["width"].ToString());

				if(splitConfigAppConfigs.ContainsKey("height")
					&& CommonLogic.IsInteger(splitConfigAppConfigs["height"].ToString()))
					resizedHeight = int.Parse(splitConfigAppConfigs["height"].ToString());

				if(splitConfigAppConfigs.ContainsKey("quality")
					&& CommonLogic.IsInteger(splitConfigAppConfigs["quality"].ToString()))
					resizedQuality = int.Parse(splitConfigAppConfigs["quality"].ToString());

				if(splitConfigAppConfigs.ContainsKey("stretch"))
					stretchMe = splitConfigAppConfigs["stretch"].ToString();

				if(splitConfigAppConfigs.ContainsKey("fill"))
					fillColor = splitConfigAppConfigs["fill"].ToString();

				if(splitConfigAppConfigs.ContainsKey("crop"))
				{
					cropMe = splitConfigAppConfigs["crop"].ToString();
					if(cropMe == "true")
					{
						if(splitConfigAppConfigs.ContainsKey("cropv"))
						{
							cropV = splitConfigAppConfigs["cropv"].ToString();
						}
						if(splitConfigAppConfigs.ContainsKey("croph"))
						{
							cropH = splitConfigAppConfigs["croph"].ToString();
						}
					}
				}
			}

			//check for params passed in through WSI
			if(splitConfigParams.ContainsKey("width")
				&& CommonLogic.IsInteger(splitConfigParams["width"].ToString()))
				resizedWidth = int.Parse(splitConfigParams["width"].ToString());

			if(splitConfigParams.ContainsKey("height")
				&& CommonLogic.IsInteger(splitConfigParams["height"].ToString()))
				resizedHeight = int.Parse(splitConfigParams["height"].ToString());

			if(splitConfigParams.ContainsKey("quality")
				&& CommonLogic.IsInteger(splitConfigParams["quality"].ToString()))
				resizedQuality = int.Parse(splitConfigParams["quality"].ToString());

			if(splitConfigParams.ContainsKey("stretch"))
				stretchMe = splitConfigParams["stretch"].ToString();

			if(splitConfigParams.ContainsKey("fill"))
				fillColor = splitConfigParams["fill"].ToString();

			if(splitConfigParams.ContainsKey("crop"))
			{
				cropMe = splitConfigParams["crop"].ToString();

				if(cropMe == "true")
				{
					if(splitConfigParams.ContainsKey("cropv"))
						cropV = splitConfigParams["cropv"].ToString();

					if(splitConfigParams.ContainsKey("croph"))
						cropH = splitConfigParams["croph"].ToString();
				}
			}

			if(resizedWidth < 1 || resizedHeight < 1)
			{
				resizedWidth = originalPhoto.Width;
				resizedHeight = originalPhoto.Height;
			}

			if(cropMe == "true")
			{
				var AnchorUpDown = cropV;
				var AnchorLeftRight = cropH;
				nPercentW = ((float)resizedWidth / (float)sourceWidth);
				nPercentH = ((float)resizedHeight / (float)sourceHeight);

				if(nPercentH < nPercentW)
				{
					nPercent = nPercentW;
					switch(AnchorUpDown)
					{
						case "top":
							destY = -2;
							break;
						case "bottom":
							destY = (int)(resizedHeight - (sourceHeight * nPercent));
							break;
						case "center":
						default:
							destY = (int)((resizedHeight - (sourceHeight * nPercent)) / 2) - 2;
							break;
					}
				}
				else
				{
					nPercent = nPercentH;
					switch(AnchorLeftRight.ToUpper())
					{
						case "left":
							destX = 0;
							break;
						case "right":
							destX = (int)(resizedWidth - (sourceWidth * nPercent));
							break;
						case "middle":
						default:
							destX = (int)((resizedWidth - (sourceWidth * nPercent)) / 2) - 2;
							break;
					}
				}
			}
			else
			{
				nPercentW = ((float)resizedWidth / (float)sourceWidth);
				nPercentH = ((float)resizedHeight / (float)sourceHeight);

				if(nPercentH < nPercentW)
				{
					nPercent = nPercentH;
					destX = (int)((resizedWidth - (sourceWidth * nPercent)) / 2);
				}
				else
				{
					nPercent = nPercentW;
					destY = (int)((resizedHeight - (sourceHeight * nPercent)) / 2) - 2;
				}
			}

			// let's account for the extra pixels we left to avoid the borderbug here
			// some distortion will occur...but it should be unnoticeable
			if(stretchMe == "false" && (originalPhoto.Width < resizedWidth && originalPhoto.Height < resizedHeight))
			{
				destWidth = originalPhoto.Width;
				destHeight = originalPhoto.Height;
				destX = (int)((resizedWidth / 2) - (originalPhoto.Width / 2));
				destY = (int)((resizedHeight / 2) - (originalPhoto.Height / 2));
			}
			else
			{
				destWidth = (int)Math.Ceiling(sourceWidth * nPercent) + 4;
				destHeight = (int)Math.Ceiling(sourceHeight * nPercent) + 4;
			}

			SavePhoto(resizedWidth, resizedHeight, destHeight, destWidth, destX, destY, sourceHeight, sourceWidth, sourceX, sourceY, originalPhoto, fileName, fillColor, resizedQuality, content);
		}

		static void ResizePhoto(Hashtable configValues, Image originalPhoto, string filename, string tempFilename, string size, string content)
		{
			var resizeMe = AppLogic.AppConfigBool("UseImageResize");

			if(configValues.ContainsKey("resize") && configValues["resize"].ToString() == "false")
				resizeMe = false;
			else if(configValues.ContainsKey("resize") && configValues["resize"].ToString() == "true")
				resizeMe = true;

			if(!resizeMe)
			{
				originalPhoto.Save(filename);
				return;
			}

			var resizedWidth = AppLogic.AppConfigNativeInt("DefaultWidth_" + size);
			var resizedHeight = AppLogic.AppConfigNativeInt("DefaultHeight_" + size);
			var resizedQuality = AppLogic.AppConfigNativeInt("DefaultQuality");

			var stretchMe = AppLogic.AppConfig("DefaultStretch");
			var cropMe = AppLogic.AppConfig("DefaultCrop");
			var cropV = AppLogic.AppConfig("DefaultCropVertical");
			var cropH = AppLogic.AppConfig("DefaultCropHorizontal");
			var fillColor = AppLogic.AppConfig("DefaultFillColor");

			var sourceWidth = originalPhoto.Width;
			var sourceHeight = originalPhoto.Height;
			var sourceX = 0;
			var sourceY = 0;
			// we will extend 2 pixels on all sides to avoid the border bug
			// we could use InterpolationMode.NearestNeighbor instead of
			// InterpolationMode.HighQualityBicubic but not without sacrificing quality
			var destX = -2;
			var destY = -2;

			var nPercent = 0f;
			var nPercentW = 0f;
			var nPercentH = 0f;

			var destWidth = 0;
			var destHeight = 0;

			if(configValues.ContainsKey("width")
				&& CommonLogic.IsInteger(configValues["width"].ToString()))
				resizedWidth = Int32.Parse(configValues["width"].ToString());

			if(configValues.ContainsKey("height")
				&& CommonLogic.IsInteger(configValues["height"].ToString()))
				resizedHeight = Int32.Parse(configValues["height"].ToString());

			if(configValues.ContainsKey("quality")
				&& CommonLogic.IsInteger(configValues["quality"].ToString()))
				resizedQuality = Int32.Parse(configValues["quality"].ToString());

			if(configValues.ContainsKey("stretch"))
				stretchMe = configValues["stretch"].ToString();

			if(configValues.ContainsKey("fill"))
				fillColor = configValues["fill"].ToString();

			if(configValues.ContainsKey("crop"))
			{
				cropMe = configValues["crop"].ToString();

				if(cropMe == "true")
				{
					if(configValues.ContainsKey("cropv"))
						cropV = configValues["cropv"].ToString();

					if(configValues.ContainsKey("croph"))
						cropH = configValues["croph"].ToString();
				}
			}

			if(resizedWidth < 1 || resizedHeight < 1)
			{
				resizedWidth = originalPhoto.Width;
				resizedHeight = originalPhoto.Height;
			}

			if(cropMe == "true")
			{
				var AnchorUpDown = cropV;
				var AnchorLeftRight = cropH;
				nPercentW = ((float)resizedWidth / (float)sourceWidth);
				nPercentH = ((float)resizedHeight / (float)sourceHeight);

				if(nPercentH < nPercentW)
				{
					nPercent = nPercentW;
					switch(AnchorUpDown)
					{
						case "top":
							destY = -2;
							break;
						case "bottom":
							destY = (int)(resizedHeight - (sourceHeight * nPercent));
							break;
						case "center":
						default:
							destY = (int)((resizedHeight - (sourceHeight * nPercent)) / 2) - 2;
							break;
					}
				}
				else
				{
					nPercent = nPercentH;
					switch(AnchorLeftRight.ToUpper())
					{
						case "left":
							destX = 0;
							break;
						case "right":
							destX = (int)(resizedWidth - (sourceWidth * nPercent));
							break;
						case "middle":
						default:
							destX = (int)((resizedWidth - (sourceWidth * nPercent)) / 2) - 2;
							break;
					}
				}
			}
			else
			{
				nPercentW = ((float)resizedWidth / (float)sourceWidth);
				nPercentH = ((float)resizedHeight / (float)sourceHeight);

				if(nPercentH < nPercentW)
				{
					nPercent = nPercentH;
					destX = (int)((resizedWidth - (sourceWidth * nPercent)) / 2);
				}
				else
				{
					nPercent = nPercentW;
					destY = (int)((resizedHeight - (sourceHeight * nPercent)) / 2) - 2;
				}
			}
			// let's account for the extra pixels we left to avoid the borderbug here
			// some distortion will occur...but it should be unnoticeable
			if(stretchMe == "false" && (originalPhoto.Width < resizedWidth && originalPhoto.Height < resizedHeight))
			{
				destWidth = originalPhoto.Width;
				destHeight = originalPhoto.Height;
				destX = (int)((resizedWidth / 2) - (originalPhoto.Width / 2));
				destY = (int)((resizedHeight / 2) - (originalPhoto.Height / 2));
			}
			else
			{
				destWidth = (int)Math.Ceiling(sourceWidth * nPercent) + 4;
				destHeight = (int)Math.Ceiling(sourceHeight * nPercent) + 4;
			}

			SavePhoto(resizedWidth, resizedHeight, destHeight, destWidth, destX, destY, sourceHeight, sourceWidth, sourceX, sourceY, originalPhoto, filename, fillColor, resizedQuality, content);
		}

		private static void SavePhoto(int resizedWidth,
			int resizedHeight,
			int destHeight,
			int destWidth,
			int destX,
			int destY,
			int sourceHeight,
			int sourceWidth,
			int sourceX,
			int sourceY,
			Image originalPhoto,
			string imageFilename,
			string fillColor,
			int resizedQuality,
			string content)
		{
			using(var resizedPhoto = new Bitmap(resizedWidth, resizedHeight, PixelFormat.Format24bppRgb))
			{
				resizedPhoto.SetResolution(originalPhoto.HorizontalResolution, originalPhoto.VerticalResolution);

				using(var grPhoto = Graphics.FromImage(resizedPhoto))
				{
					var clearColor = new Color();

					try
					{
						clearColor = ColorTranslator.FromHtml(fillColor);
					}
					catch
					{
						clearColor = Color.White;
					}

					grPhoto.Clear(clearColor);

					if(resizedQuality > 100 || resizedQuality < 1)
						resizedQuality = 100;

					// Encoder parameter for image quality 
					var qualityParam = new EncoderParameter(Encoder.Quality, resizedQuality);
					var encoderParams = new EncoderParameters(1);
					encoderParams.Param[0] = qualityParam;

					if(content == "image/gif")
						content = "image/jpeg";

					// Image codec 
					var imgCodec = GetEncoderInfo(content);

					grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;

					grPhoto.DrawImage(originalPhoto,
						new Rectangle(destX, destY, destWidth, destHeight),
						new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
						GraphicsUnit.Pixel);

					try
					{
						resizedPhoto.Save(imageFilename, imgCodec, encoderParams);
					}
					catch
					{
						throw new Exception("You do not have proper permissions set.  You must ensure that the ASPNET and NETWORK SERVICE users have write/modify permissions on the images directory and it's sub-directories.");
					}
				}
			}
		}

		public static void DisposeOfTempImage(string tmpImg_fName)
		{
			try
			{
				System.IO.File.Delete(tmpImg_fName);
			}
			catch { }
		}

		public static void CreateOthersFromLarge(string objectName, string tempImage, string filename, string content)
		{
			var imgExt = ".jpg";
			var largeCreates = false;
			var largeOverwrites = false;
			var globalCreate = (AppLogic.AppConfigBool("LargeCreatesOthers"));
			var globalOverwrite = (AppLogic.AppConfigBool("LargeOverwritesOthers"));
			var localCreate = string.Empty;
			var localOverwrite = string.Empty;
			var configValues = SplitConfig(objectName, "large");

			if(configValues.ContainsKey("largecreates"))
				localCreate = configValues["largecreates"].ToString();

			if(configValues.ContainsKey("largeoverwrites"))
				localOverwrite = configValues["largeoverwrites"].ToString();

			if(localCreate == "false")
				largeCreates = false;
			else if(localCreate == "true")
				largeCreates = true;
			else
				largeCreates = globalCreate;

			if(localOverwrite == "false")
				largeOverwrites = false;
			else if(localOverwrite == "true")
				largeOverwrites = true;
			else
				largeOverwrites = globalOverwrite;

			switch(content)
			{
				case "image/png":
					imgExt = ".png";
					break;
				case "image/jpeg":
				default:
					imgExt = ".jpg";
					break;
			}

			filename = filename.Replace(".", "");
			if(largeCreates)
			{
				var microImage = AppLogic.GetImagePath(objectName, "micro", true) + filename + imgExt;
				var iconImage = AppLogic.GetImagePath(objectName, "icon", true) + filename + imgExt;
				var mediumImage = AppLogic.GetImagePath(objectName, "medium", true) + filename + imgExt;
				if(largeOverwrites)
				{
					// delete any smaller image files first
					try
					{
						foreach(var imageType in CommonLogic.SupportedImageTypes)
						{
							System.IO.File.Delete(AppLogic.GetImagePath(objectName, "icon", true) + filename + imageType);
							System.IO.File.Delete(AppLogic.GetImagePath(objectName, "medium", true) + filename + imageType);
						}
					}
					catch
					{ }

					ResizeEntityOrObject(objectName, tempImage, iconImage, "icon", content);
					ResizeEntityOrObject(objectName, tempImage, mediumImage, "medium", content);

					if(objectName.Equals("PRODUCT", StringComparison.InvariantCultureIgnoreCase))
					{
						foreach(string ss in CommonLogic.SupportedImageTypes)
						{
							System.IO.File.Delete(AppLogic.GetImagePath(objectName, "micro", true) + filename + ss);
						}
						ResizeEntityOrObject(objectName, tempImage, microImage.Replace("_.", "."), "micro", content);
					}
				}
				else
				{
					var iconExists = false;
					var mediumExists = false;
					foreach(string imageType in CommonLogic.SupportedImageTypes)
					{
						if(CommonLogic.FileExists(AppLogic.GetImagePath(objectName, "icon", true) + filename + imageType))
						{
							iconExists = true;
						}
						if(CommonLogic.FileExists(AppLogic.GetImagePath(objectName, "medium", true) + filename + imageType))
						{
							mediumExists = true;
						}
					}

					if(!iconExists)
						ResizeEntityOrObject(objectName, tempImage, iconImage, "icon", content);

					if(!mediumExists)
						ResizeEntityOrObject(objectName, tempImage, mediumImage, "medium", content);

					if(objectName.Equals("PRODUCT", StringComparison.InvariantCultureIgnoreCase))
					{
						var microExists = false;

						foreach(string imageType in CommonLogic.SupportedImageTypes)
						{
							if(CommonLogic.FileExists(AppLogic.GetImagePath(objectName, "micro", true) + filename + imageType))
								microExists = true;

						}

						if(!microExists)
							ResizeEntityOrObject(objectName, tempImage, microImage.Replace("_.", "."), "micro", content);
					}
				}
			}
		}

		private static ImageCodecInfo GetEncoderInfo(string resizeMimeType)
		{
			// Get image codecs for all image formats 
			var codecs = ImageCodecInfo.GetImageEncoders();

			// Find the correct image codec 
			for(var i = 0; i < codecs.Length; i++)
				if(codecs[i].MimeType == resizeMimeType)
					return codecs[i];

			return null;
		}

		static void ResizeForMicro(string tempFilename, string filename, int microWidth, int microHeight)
		{
			var resizedWidth = microWidth;
			var resizedHeight = microHeight;

			using(var origPhoto = Image.FromFile(CommonLogic.SafeMapPath(tempFilename)))
			{
				var sourceWidth = origPhoto.Width;
				var sourceHeight = origPhoto.Height;
				var sourceX = 0;
				var sourceY = 0;
				var destX = -2;
				var destY = -2;

				var nPercent = 0f;
				var nPercentW = ((float)resizedWidth / (float)sourceWidth);
				var nPercentH = ((float)resizedHeight / (float)sourceHeight);

				var destWidth = 0;
				var destHeight = 0;

				if(nPercentH < nPercentW)
				{
					nPercent = nPercentW;
					destY = (int)((resizedHeight - (sourceHeight * nPercent)) / 2) - 2;
				}
				else
				{
					nPercent = nPercentH;
					destX = (int)((resizedWidth - (sourceWidth * nPercent)) / 2) - 2;
				}

				destWidth = (int)Math.Ceiling(sourceWidth * nPercent) + 4;
				destHeight = (int)Math.Ceiling(sourceHeight * nPercent) + 4;

				using(var resizedPhoto = new Bitmap(resizedWidth, resizedHeight, PixelFormat.Format24bppRgb))
				using(var grPhoto = Graphics.FromImage(resizedPhoto))
				{
					grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;
					grPhoto.DrawImage(origPhoto,
						new Rectangle(destX, destY, destWidth, destHeight),
						new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
						GraphicsUnit.Pixel);

					resizedPhoto.Save(filename);
				}
			}
		}

		public static void MakeMicroPic(string fileName, string tempImage, string imageNumber)
		{
			var microWidth = AppLogic.AppConfigNativeInt("DefaultWidth_micro");
			var microHeight = AppLogic.AppConfigNativeInt("DefaultHeight_micro");

			var newImageName = AppLogic.GetImagePath("Product", "micro", true) + fileName + "_" + imageNumber.ToLowerInvariant() + ".jpg";

			ResizeForMicro(tempImage, newImageName, microWidth, microHeight);
		}

		static void MakeMicroPic(string fileName, string tempImage, string microParams, string imageSize)
		{
			fileName = fileName.Replace("_.", ".");
			var spltConfig = new Hashtable();

			var microWidth = AppLogic.AppConfigNativeInt("DefaultWidth_micro");
			var microHeight = AppLogic.AppConfigNativeInt("DefaultHeight_micro");

			var microMime = ".jpg";

			spltConfig = SplitConfig(microParams);

			if(spltConfig.ContainsKey("width"))
				microWidth = int.Parse(spltConfig["width"].ToString());

			if(spltConfig.ContainsKey("height"))
				microHeight = int.Parse(spltConfig["height"].ToString());

			if(spltConfig.ContainsKey("mime"))
				microMime = spltConfig["mime"].ToString();

			if(microHeight < 1)
				microHeight = 40;

			if(microWidth < 1)
				microWidth = 40;

			switch(microMime)
			{
				case "png":
					microMime = ".png";
					break;
				case "jpg":
				default:
					microMime = ".jpg";
					break;
			}

			if(imageSize == "large")
			{
				//check for large creates others and large overwrites others to create the micro image if desired
				var largeCreates = false;
				var largeOverwrites = false;
				var globalCreate = (AppLogic.AppConfigBool("LargeCreatesOthers"));
				var globalOverwrite = (AppLogic.AppConfigBool("LargeOverwritesOthers"));
				var localCreate = string.Empty;
				var localOverwrite = string.Empty;
				var configValues = SplitConfig("Micro", "large", microParams);

				if(configValues.ContainsKey("largecreates"))
					localCreate = configValues["largecreates"].ToString();

				if(configValues.ContainsKey("largeoverwrites"))
					localOverwrite = configValues["largeoverwrites"].ToString();

				if(localCreate == "false")
					largeCreates = false;
				else if(localCreate == "true")
					largeCreates = true;
				else
					largeCreates = globalCreate;

				if(localOverwrite == "false")
					largeOverwrites = false;
				else if(localOverwrite == "true")
					largeOverwrites = true;
				else
					largeOverwrites = globalOverwrite;

				if(largeCreates)
				{
					if(largeOverwrites)
					{
						try
						{
							foreach(string ss in CommonLogic.SupportedImageTypes)
							{
								System.IO.File.Delete(AppLogic.GetImagePath("Product", "micro", true) + fileName + ss);
							}
						}
						catch { }

						var newImageName = AppLogic.GetImagePath("Product", "micro", true) + fileName + microMime;

						ResizeForMicro(tempImage, newImageName, microWidth, microHeight);

					}
					else
					{
						var microExists = false;
						foreach(var imageType in CommonLogic.SupportedImageTypes)
						{
							if(CommonLogic.FileExists(AppLogic.GetImagePath("Product", "micro", true) + fileName + imageType))
								microExists = true;
						}

						if(!microExists)
						{
							var newImageName = AppLogic.GetImagePath("Product", "micro", true) + fileName + microMime;

							ResizeForMicro(tempImage, newImageName, microWidth, microHeight);
						}
					}
				}
			}
			else if(AppLogic.AppConfigBool("MultiMakesMicros") && imageSize == "medium")
			{
				// lets create micro images if using the medium multi image manager
				// since the medium icons are what show on the product pages
				var newImageName = AppLogic.GetImagePath("Product", "micro", true) + fileName + microMime;

				ResizeForMicro(tempImage, newImageName, microWidth, microHeight);
			}
		}

		public static void MakeOtherMultis(string filename, string imageId, string color, string tempImage, string content)
		{
			var imgExt = ".jpg";
			var largeCreates = false;
			var largeOverwrites = false;
			var globalCreate = (AppLogic.AppConfigBool("LargeCreatesOthers"));
			var globalOverwrite = (AppLogic.AppConfigBool("LargeOverwritesOthers"));
			var localCreate = String.Empty;
			var localOverwrite = String.Empty;
			var configValues = SplitConfig("Product", "large");

			if(configValues.ContainsKey("largecreates"))
				localCreate = configValues["largecreates"].ToString();

			if(configValues.ContainsKey("largeoverwrites"))
				localOverwrite = configValues["largeoverwrites"].ToString();

			if(localCreate == "false")
				largeCreates = false;
			else if(localCreate == "true")
				largeCreates = true;
			else
				largeCreates = globalCreate;

			if(localOverwrite == "false")
				largeOverwrites = false;
			else if(localOverwrite == "true")
				largeOverwrites = true;
			else
				largeOverwrites = globalOverwrite;

			switch(content)
			{
				case "image/png":
					imgExt = ".png";
					break;
				case "image/jpeg":
				default:
					imgExt = ".jpg";
					break;
			}

			if(!largeCreates)
				return;

			var makeMicros = AppLogic.AppConfigBool("MultiMakesMicros");
			var sizesToMake = new List<ProductImageSize>();
			sizesToMake.Add(ProductImageSize.icon);
			sizesToMake.Add(ProductImageSize.medium);

			if(makeMicros)
				sizesToMake.Add(ProductImageSize.micro);

			foreach(var size in sizesToMake)
			{
				var imagepath = AppLogic.GetImagePath("Product", size.ToString(), true) + filename + "_" + imageId.ToLowerInvariant() + "_" + color + imgExt;
				if(largeOverwrites)
				{
					// delete any smaller image files first
					foreach(var imageType in CommonLogic.SupportedImageTypes)
					{
						try
						{
							System.IO.File.Delete(AppLogic.GetImagePath("Product", size.ToString(), true) + filename + "_" + imageId.ToLowerInvariant() + "_" + color + imageType);
						}
						catch { }
					}
					ResizeEntityOrObject("Product", tempImage, imagepath, size.ToString(), content);
				}
				else
				{
					var imageExists = false;
					foreach(var imageType in CommonLogic.SupportedImageTypes)
					{
						if(CommonLogic.FileExists(AppLogic.GetImagePath("Product", size.ToString(), true) + filename + imageType))
							imageExists = true;
					}
					if(!imageExists)
						ResizeEntityOrObject("Product", tempImage, imagepath, size.ToString(), content);
				}
			}
		}
	}
}
