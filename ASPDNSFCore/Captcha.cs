// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json;

namespace AspDotNetStorefrontCore
{
	public class Captcha
	{
		public readonly string SecurityCode;
		readonly int Width;
		readonly int Height;
		readonly Random Random;

		Bitmap CaptchaImage;
		Color ImageForeColor;
		Color ImageBackColor;
		Color TextForeColor;
		Color TextBackColor;
		Color HorizontalColor;
		Color VerticalColor;

		public Bitmap Image
		{
			get { return CaptchaImage; }
		}

		/// <summary>
		/// Generate captcha image with a random security code
		/// </summary>
		/// <param name="securityCode">Text for image</param>
		/// <param name="width">Width of image</param>
		/// <param name="height">height of image</param>
		public Captcha(string existingSecurityCode, int width, int height)
		{
			Random = new Random();

			SecurityCode = existingSecurityCode 
				?? GenerateSecurityCode();

			Width = width;
			Height = height;

			InitializeColorProperties();
			GenerateColorCaptcha();
		}

		/// <summary>
		/// Generate a random security code
		/// </summary>
		/// <returns>An alpha-numeric string</returns>
		string GenerateSecurityCode()
		{
			var allowedCharacters = new Regex(
				pattern: AppLogic.AppConfig("Captcha.AllowedCharactersRegex").Length == 0
					? "@[0-9]"
					: AppLogic.AppConfig("Captcha.AllowedCharactersRegex"));

			var maxTries = 1000;
			var tries = 0;

			var randomCode = string.Empty;
			var random = new Random((int)DateTime.Now.Ticks);

			var codeLength = AppLogic.AppConfigNativeInt("Captcha.NumberOfCharacters");
			if(codeLength < 6)
				codeLength = 6;
			else if(codeLength > 20)
				codeLength = 20;

			var minAscii = 33;
			var maxAscii = AppLogic.AppConfigNativeInt("Captcha.MaxAsciiValue");
			if(maxAscii == 0)
				maxAscii = 126;

			do
			{
				try
				{
					var character = char.ConvertFromUtf32(random.Next(minAscii, maxAscii));
					if(!allowedCharacters.IsMatch(character))
						continue;

					randomCode += character;
					tries++;
				}
				catch { }
			}
			while(randomCode.Length < codeLength && tries < maxTries);

			if(string.IsNullOrEmpty(randomCode))
				throw new InvalidOperationException("A captcha code could not be created.");

			return randomCode;
		}

		/// <summary>
		/// Generate captcha image with colors and random noise
		/// </summary>
		void GenerateColorCaptcha()
		{
			using(var captchaImage = new Bitmap(Width, Height, PixelFormat.Format32bppArgb))
			using(var graphics = Graphics.FromImage(captchaImage))
			{
				graphics.SmoothingMode = SmoothingMode.AntiAlias;

				// Draw the hatched background
				var boundary = new Rectangle(0, 0, Width, Height);
				using(var backgroundHatchBrush = new HatchBrush(HatchStyle.SmallGrid, ImageForeColor, ImageBackColor))
					graphics.FillRectangle(backgroundHatchBrush, boundary);

				// Generate random horizontal alpha-fading colored lines for noise
				for(var horizontalIdx = 0; horizontalIdx < 6; horizontalIdx++)
				{
					var x1 = Random.Next(Width);
					var x2 = Random.Next(Width);
					var y = Random.Next(Height);

					if(x1 == x2)
						continue;

					using(var gradientBrush = new LinearGradientBrush(
						point1: new PointF(x1, y),
						point2: new PointF(x2, y),
						color1: Color.FromArgb(125, HorizontalColor),
						color2: Color.FromArgb(10, HorizontalColor)))
					using(var pen = new Pen(gradientBrush, 5))
						graphics.DrawLine(pen, new Point(x1, y), new Point(x2, y));
				}

				// Generate random vertical alpha-fading colored lines for noise
				for(int verticalIdx = 0; verticalIdx < 6; verticalIdx++)
				{
					var y1 = Random.Next(Height);
					var y2 = Random.Next(Height);
					var x = Random.Next(Width);

					if(y1 == y2)
						continue;

					using(var gradientBrush = new LinearGradientBrush(
						new PointF(x, y1),
						new PointF(x, y2),
						Color.FromArgb(125, VerticalColor),
						Color.FromArgb(10, VerticalColor)))
					using(var pen = new Pen(gradientBrush, 5))
						graphics.DrawLine(pen, new Point(x, y1), new Point(x, y2));
				}

				using(var path = new GraphicsPath())
				using(var format = new StringFormat())
				{
					// Format the text to draw on a single line
					format.Alignment = StringAlignment.Center;
					format.LineAlignment = StringAlignment.Center;
					format.FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.NoWrap | StringFormatFlags.NoClip;
					
					// Adjust the font size so it fits inside the image
					var fontSize = boundary.Height * 2; // Pick an arbitrarily oversized font
					var fontSizeAdjustment = Math.Min((int)(fontSize * -0.02), -1);	// Adjust down by 2% or 1em each iteration

					// Measure the string and reduce the font size incrementally until it fits
					do
					{
						float mismatchDelta;
						using(var sizeCheckPath = new GraphicsPath())
						{
							sizeCheckPath.AddString(
								SecurityCode,
								FontFamily.GenericSerif,
								(int)FontStyle.Bold,
								fontSize,
								new Point(
									boundary.Width / 2,
									boundary.Height / 2),
								format);

							var sizeCheckBounds = sizeCheckPath.GetBounds();
							mismatchDelta = Math.Max(
								sizeCheckBounds.Width - boundary.Width,
								sizeCheckBounds.Height - boundary.Height);
						}

						if(fontSize <= 1)
						{
							fontSize = 1;
							break;
						}

						if(mismatchDelta <= 1f)
							break;

						fontSize += fontSizeAdjustment;
					} while(true);

					// Add the string to the path at the correct size
					path.AddString(
						SecurityCode,
						FontFamily.GenericSerif,
						(int)FontStyle.Bold,
						fontSize,
						new Point(
							boundary.Width / 2,
							boundary.Height / 2),
						format);

					// Warp the string by pushing in the corners by different random amounts
					var distortionCoefficient = 0.17f;
					var warpPoints = new[]
					{
						new PointF(	// Top left
							(Random.Next(boundary.Width) * distortionCoefficient),
							(Random.Next(boundary.Height) * distortionCoefficient)),
						new PointF( // Bottom left
							boundary.Width - (Random.Next(boundary.Width) * distortionCoefficient),
							(Random.Next(boundary.Height) * distortionCoefficient)),
						new PointF( // Top right
							(Random.Next(boundary.Width) * distortionCoefficient),
							boundary.Height - (Random.Next(boundary.Height) * distortionCoefficient)),
						new PointF( // Bottom right
							boundary.Width - (Random.Next(boundary.Width) * distortionCoefficient),
							boundary.Height - (Random.Next(boundary.Height) * distortionCoefficient)),
					};

					path.Warp(warpPoints, boundary);

					// Draw the warped string
					using(var textHatchBrush = new SolidBrush(Color.Black))
						graphics.FillPath(textHatchBrush, path);
				}

				// Generate random beziers for noise
				using(var bezierPen = new Pen(Color.FromArgb(175, ImageForeColor), 0.25F))
					for(int i = 0; i < 5; i++)
						graphics.DrawBeziers(
							bezierPen,
							new[]
							{
								new PointF(Random.Next(Width),Random.Next(Height)),
								new PointF(Random.Next(Width),Random.Next(Height)),
								new PointF(Random.Next(Width),Random.Next(Height)),
								new PointF(Random.Next(Width),Random.Next(Height))
							});

				CaptchaImage = captchaImage.Clone(boundary, captchaImage.PixelFormat);
			}
		}

		/// <summary>
		/// Initialize Captcha2 properties
		/// </summary>
		void InitializeColorProperties()
		{
			ImageBackColor = Color.LightGray;
			ImageForeColor = Color.White;
			TextBackColor = Color.DarkGray;
			TextForeColor = Color.Black;
			HorizontalColor = Color.Blue;
			VerticalColor = Color.Blue;
		}
	}

	public class CaptchaStorageService
	{
		const string PersistedCookieName = "ASPDNSFCaptchaService";

		/// <summary>
		///  Saves the current Security Code previously generated and stores it in a secure cookie.
		/// </summary>
		/// <param name="httpContext">HttpContext of the current request.</param>
		/// <param name="securityCode">Security code to store in a secure cookie.</param>
		/// <param name="scope">Scope to narrow the area a security code is valid for. This is what allows different captcha codes per page.</param>
		public void StoreSecurityCode(HttpContextBase httpContext, string securityCode, string scope)
		{
			if(httpContext == null)
				throw new ArgumentNullException("httpContext", "Http context cannot be null.");

			if(httpContext.Request
				.Cookies
				.AllKeys
				.Contains(PersistedCookieName))
				httpContext.Response
					.AppendCookie(new HttpCookie(PersistedCookieName)
					{
						Expires = DateTime.Now.AddSeconds(-1)
					});

			httpContext.Response
				.Cookies
				.Add(new HttpCookie(PersistedCookieName)
				{
					Value = Security.MungeString(
						s: JsonConvert.SerializeObject(new CaptchaInfo
							{
								SecurityCode = securityCode,
								Scope = scope
						})),
					HttpOnly = true
				});
		}

		/// <summary>
		/// Retrieves the security code from a previously created cookie.
		/// </summary>
		/// <param name="httpContext">HttpContext of the current request.</param>
		/// <param name="scope">Scope to narrow the area a security code is valid for. This is what allows different captcha codes per page.</param>
		/// <returns>An unencrypted security code.</returns>
		public string RetrieveSecurityCode(HttpContextBase httpContext, string scope)
		{
			if(httpContext == null)
				throw new ArgumentNullException("httpContext", "Http context cannot be null.");

			if(!httpContext.Request
				.Cookies
				.AllKeys
				.Contains(PersistedCookieName))
				return null;

			try
			{
				var captchaInfo = JsonConvert
					.DeserializeObject<CaptchaInfo>(
						value: Security.UnmungeString(
							s: httpContext.Request.Cookies[PersistedCookieName].Value));

				return string.Equals(captchaInfo.Scope, scope, StringComparison.InvariantCultureIgnoreCase)
					? captchaInfo.SecurityCode
					: null;
			}
			catch (JsonException)
			{
				return null;
			}
		}

		public void ClearSecurityCode(HttpContextBase httpContext)
		{
			if(httpContext == null)
				throw new ArgumentNullException("httpContext", "Http context cannot be null.");

			if(httpContext.Request
				.Cookies
				.AllKeys
				.Contains(PersistedCookieName))
				httpContext.Response
					.AppendCookie(new HttpCookie(PersistedCookieName)
						{
							Expires = DateTime.Now.AddSeconds(-1)
						});
		}
	}

	public class CaptchaInfo
	{
		public string SecurityCode
		{ get; set; }

		public string Scope
		{ get; set; }
	}
}
