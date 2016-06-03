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
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;

namespace AspDotNetStorefrontCore
{
	/// <summary>
	/// Summary description for CommonLogic.
	/// </summary>
	public class CommonLogic
    {
        static public String[] SupportedImageTypes = { ".jpg", ".gif", ".png" };

        /// <summary>
        /// Gets the image extension for a given mime type
        /// </summary>
        /// <param name="mimeType"></param>
        /// <returns></returns>
        public static string ResolveExtensionFromMimeType(string mimeType)
        {
            string extension = string.Empty;

            switch (mimeType)
            {
                case "image/gif":
                    extension = ".gif";
                    break;
                case "image/png":
                case "image/x-png":
                    extension = ".png";
                    break;
                case "image/jpg":
                case "image/jpeg":
                case "image/pjpeg":
                    extension = ".jpg";
                    break;
            }

            return extension;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool IsStringNullOrEmpty(string value)
        {
            if (null == value) return true;

            return value.Length == 0 || value.Trim().Length == 0;
        }

        // this is good enough for what we need to use it for. it is not scientifically 100% valid for all cases!!
        static public bool StringIsAlreadyHTMLEncoded(String s)
        {
            return s.Contains("&nbsp;") || s.Contains("&quot;") || s.Contains("&amp;") || s.Contains("&lt;") || s.Contains("&gt;") || Regex.IsMatch(s, "&#[^;]+;", RegexOptions.Compiled);
        }

        /// <summary>
        /// Check String for invalid character
        /// </summary>
        /// <param name="s">The string to verify</param>
        /// <returns>Returns True if contains Invalid Character </returns>
        static public Boolean HasInvalidChar(String s)
        {
                              
            return !Regex.IsMatch(s,@"^[^<>`~!/@\#}$%:;)(_^{&*=|'+]+$");
                     
        }

        static public System.Drawing.Image LoadImage(String url)
        {
            string imgName = SafeMapPath(url);
            Bitmap bmp = new Bitmap(imgName);
            return bmp;
        }

        // can use either text copyright, or image copyright, or both:
        // imgPhoto is image (memory) on which to add copyright text/mark
        static public System.Drawing.Image AddWatermark(System.Drawing.Image imgPhoto, String CopyrightText, String CopyrightImageUrl)
        {
            int phWidth = imgPhoto.Width;
            int phHeight = imgPhoto.Height;
            float WatermarkOpacity = AppLogic.AppConfigUSSingle("Watermark.Opacity");

            //create a Bitmap the Size of the original photograph
            Bitmap bmPhoto = new Bitmap(phWidth, phHeight, PixelFormat.Format24bppRgb);

            bmPhoto.SetResolution(imgPhoto.HorizontalResolution, imgPhoto.VerticalResolution);

            //load the Bitmap into a Graphics object 
            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;
            grPhoto.SmoothingMode = SmoothingMode.HighQuality;
            grPhoto.PixelOffsetMode = PixelOffsetMode.HighQuality;
            grPhoto.CompositingQuality = CompositingQuality.HighQuality;
            //create a image object containing the watermark
            Image imgWatermark = null;
            int wmWidth = 0;
            int wmHeight = 0;		
            string imageUrl = SafeMapPath(CopyrightImageUrl).ToLower();
                
			if(FileExists(imageUrl))
			{
				if(imageUrl.IndexOf("controls") != -1)
				{
					imageUrl = imageUrl.Replace("\\controls", "");
				}
				imgWatermark = new Bitmap(imageUrl);
				wmWidth = imgWatermark.Width;
				wmHeight = imgWatermark.Height;             
			}

            //------------------------------------------------------------
            //Step #1 - Insert Copyright message
            //------------------------------------------------------------

            //Set the rendering quality for this Graphics object
            grPhoto.SmoothingMode = SmoothingMode.AntiAlias;

            //Draws the photo Image object at original size to the graphics object.
            grPhoto.DrawImage(
                imgPhoto,                               // Photo Image object
                new Rectangle(0, 0, phWidth, phHeight), // Rectangle structure
                0,                                      // x-coordinate of the portion of the source image to draw. 
                0,                                      // y-coordinate of the portion of the source image to draw. 
                phWidth,                                // Width of the portion of the source image to draw. 
                phHeight,                               // Height of the portion of the source image to draw. 
                GraphicsUnit.Pixel);                    // Units of measure 

            //-------------------------------------------------------
            //to maximize the size of the Copyright message we will 
            //test multiple Font sizes to determine the largest posible 
            //font we can use for the width of the Photograph
            //define an array of point sizes you would like to consider as possiblities
            //-------------------------------------------------------
            int[] sizes = new int[] { 16, 14, 12, 10, 8, 6, 4 };

            Font crFont = null;
            SizeF crSize = new SizeF();

            //Loop through the defined sizes checking the length of the Copyright string
            //If its length in pixles is less then the image width choose this Font size.
            for (int i = 0; i < 7; i++)
            {
                //set a Font object to Arial (i)pt, Bold
                crFont = new Font("arial", sizes[i], FontStyle.Bold);
                //Measure the Copyright string in this Font
                crSize = grPhoto.MeasureString(CopyrightText, crFont);

                if ((ushort)crSize.Width < (ushort)phWidth)
                    break;
            }

            //Since all photographs will have varying heights, determine a 
            //position 5% from the bottom of the image
            int OffsetPercentage = AppLogic.AppConfigUSInt("Watermark.OffsetFromBottomPercentage");
            if (OffsetPercentage == 0)
            {
                OffsetPercentage = 5;
            }
            int yPixlesFromBottom = (int)(phHeight * (OffsetPercentage / 100.0));

            //Now that we have a point size use the Copyrights string height 
            //to determine a y-coordinate to draw the string of the photograph
            float yPosFromBottom = ((phHeight - yPixlesFromBottom) - (crSize.Height / 2.0F));

            //Determine its x-coordinate by calculating the center of the width of the image
            float xCenterOfImg = (phWidth / 2);

            //Define the text layout by setting the text alignment to centered
            StringFormat StrFormat = new StringFormat();
            StrFormat.Alignment = StringAlignment.Center;

            int textOpacity = (int)(153 * WatermarkOpacity);
            //define a Brush which is semi trasparent black (Alpha set to 153)
            SolidBrush semiTransBrush2 = new SolidBrush(Color.FromArgb(textOpacity, 0, 0, 0));

            //Draw the Copyright string
            grPhoto.DrawString(CopyrightText,                 //string of text
                crFont,                                   //font
                semiTransBrush2,                           //Brush
                new PointF(xCenterOfImg + 1, yPosFromBottom + 1),  //Position
                StrFormat);

            //define a Brush which is semi trasparent white (Alpha set to 153)
            SolidBrush semiTransBrush = new SolidBrush(Color.FromArgb(textOpacity, 255, 255, 255));

            //Draw the Copyright string a second time to create a shadow effect
            //Make sure to move this text 1 pixel to the right and down 1 pixel
            grPhoto.DrawString(CopyrightText,                 //string of text
                crFont,                                   //font
                semiTransBrush,                           //Brush
                new PointF(xCenterOfImg, yPosFromBottom),  //Position
                StrFormat);                               //Text alignment

            //------------------------------------------------------------
            //Step #2 - Insert Watermark image
            //------------------------------------------------------------
            if (imgWatermark != null)
            {
                //Create a Bitmap based on the previously modified photograph Bitmap
                Bitmap bmWatermark = new Bitmap(bmPhoto);
                bmWatermark.SetResolution(imgPhoto.HorizontalResolution, imgPhoto.VerticalResolution);
                //Load this Bitmap into a new Graphic Object
                Graphics grWatermark = Graphics.FromImage(bmWatermark);

                //To achieve a transulcent watermark we will apply (2) color 
                //manipulations by defineing a ImageAttributes object and 
                //seting (2) of its properties.
                ImageAttributes imageAttributes = new ImageAttributes();

                //The first step in manipulating the watermark image is to replace 
                //the background color with one that is trasparent (Alpha=0, R=0, G=0, B=0)
                //to do this we will use a Colormap and use this to define a RemapTable
                ColorMap colorMap = new ColorMap();

                //Watermark image should be defined with a background of 100% Green this will
                //be the color we search for and replace with transparency
                colorMap.OldColor = Color.FromArgb(255, 0, 255, 0);
                colorMap.NewColor = Color.FromArgb(0, 0, 0, 0);

                ColorMap[] remapTable = { colorMap };

                imageAttributes.SetRemapTable(remapTable, ColorAdjustType.Bitmap);

                //The second color manipulation is used to change the opacity of the 
                //watermark.  This is done by applying a 5x5 matrix that contains the 
                //coordinates for the RGBA space.  By setting the 3rd row and 3rd column 
                //to 0.1f we achive a level of opacity
                float[][] colorMatrixElements = {   new float[] {1.0f,  0.0f,  0.0f,  0.0f, 0.0f},       
													new float[] {0.0f,  1.0f,  0.0f,  0.0f, 0.0f},        
													new float[] {0.0f,  0.0f,  1.0f,  0.0f, 0.0f},        
													new float[] {0.0f,  0.0f,  0.0f,  0.1f, 0.0f},        
													new float[] {0.0f,  0.0f,  0.0f,  0.0f, 1.0f}};
                
                if (WatermarkOpacity != 0.0F)
                {
                    colorMatrixElements[3][3] = WatermarkOpacity;
                }
                ColorMatrix wmColorMatrix = new ColorMatrix(colorMatrixElements);

                imageAttributes.SetColorMatrix(wmColorMatrix, ColorMatrixFlag.Default,
                    ColorAdjustType.Bitmap);

                //For this example we will place the watermark in center of the photograph.

                int xPosOfWm = ((phWidth - wmWidth) / 2);
                int yPosOfWm = ((phHeight - wmHeight) / 2);

                grWatermark.DrawImage(imgWatermark,
                    new Rectangle(xPosOfWm, yPosOfWm, wmWidth, wmHeight),  //Set the detination Position
                    0,                  // x-coordinate of the portion of the source image to draw. 
                    0,                  // y-coordinate of the portion of the source image to draw. 
                    wmWidth,            // Watermark Width
                    wmHeight,		    // Watermark Height
                    GraphicsUnit.Pixel, // Unit of measurment
                    imageAttributes);   //ImageAttributes Object
                bmPhoto = bmWatermark;
                grWatermark.Dispose();
            }
            grPhoto.Dispose();
            if (imgWatermark != null)
            {
                imgWatermark.Dispose();
            }
            return bmPhoto;
        }

        static public String UTF8ByteArrayToString(Byte[] characters)
        {

            UTF8Encoding encoding = new UTF8Encoding();
            String constructedString = encoding.GetString(characters);
            return constructedString;
        }

        static public bool IntegerIsInIntegerList(int SearchInt, String ListOfInts)
        {
            String MasterList = ListOfInts.Replace(" ", "").Trim();
            if (MasterList.Length == 0)
            {
                return false;
            }
            String target = SearchInt.ToString();
            foreach (string spat in MasterList.Split(','))
            {
                if (target == spat)
                {
                    return true;
                }
            }
            return false;
        }

        static public int IIF(bool condition, int a, int b)
        {
            int x = 0;
            if (condition)
            {
                x = a;
            }
            else
            {
                x = b;
            }
            return x;
        }

        static public bool IIF(bool condition, bool a, bool b)
        {
            bool x = false;
            if (condition)
            {
                x = a;
            }
            else
            {
                x = b;
            }
            return x;
        }

        static public decimal IIF(bool condition, decimal a, decimal b)
        {
            decimal x = 0;
            if (condition)
            {
                x = a;
            }
            else
            {
                x = b;
            }
            return x;
        }

        static public String IIF(bool condition, String a, String b)
        {
            String x = String.Empty;
            if (condition)
            {
                x = a;
            }
            else
            {
                x = b;
            }
            return x;
        }

		/// <summary>
		/// Accepts an object and if that object is null then the value passed to replacementValue will be returned.  If not null then the object value is returned.
		/// </summary>
		/// <param name="testValue">Value to test for null</param>
		/// <param name="replacementValue">Value returned if testValue is null</param>
		/// <returns></returns>
		static public T IsNull<T>(T testValue, T replacementValue)
		{
			if(testValue == null)
				return replacementValue;
			else
				return testValue;
		}

        public static int Min(int a, int b)
        {
            if (a < b)
            {
                return a;
            }
            return b;
        }

        public static String PageInvocation()
        {
            return HttpContext.Current.Request.RawUrl;
        }

        public static String PageReferrer()
        {
            try
            {
                if (HttpContext.Current.Request.UrlReferrer == null)
                {
                    return String.Empty;
                }
                else
                {
                    return HttpContext.Current.Request.UrlReferrer.ToString();
                }
            }
            catch
            { }
            return String.Empty;
        }

        static public String GetThisPageName(bool includePath)
        {
            String s = CommonLogic.ServerVariables("SCRIPT_NAME");
            if (!includePath)
            {
                int ix = s.LastIndexOf("/");
                if (ix != -1)
                {
                    s = s.Substring(ix + 1);
                }
            }
            return s;
        }

		static public string GetProductName()
		{
			var productAttribute = Assembly
				.GetExecutingAssembly()
				.GetCustomAttributes(typeof(AssemblyProductAttribute), false)
				.OfType<AssemblyProductAttribute>()
				.FirstOrDefault();

			return productAttribute != null
				? productAttribute.Product
				: string.Empty;
		}

		static public String GetVersion(bool IncludeDescription = true)
		{
			var productName = GetProductName();
			var assembly = Assembly.GetExecutingAssembly();

			if(IncludeDescription)
				return productName + " " + assembly.GetName().Version.ToString() + "/" + AppLogic.AppConfig("StoreVersion");
			else
				return assembly.GetName().Version.ToString() + "/" + AppLogic.AppConfig("StoreVersion");
		}

		public static String GetPhoneDisplayFormat(String PhoneNumber)
        {
            if (PhoneNumber.Length == 0)
            {
                return String.Empty;
            }
            if (PhoneNumber.Length != 11)
            {
                return PhoneNumber;
            }
            return "(" + PhoneNumber.Substring(1, 3) + ") " + PhoneNumber.Substring(4, 3) + "-" + PhoneNumber.Substring(7, 4);
        }

        public static bool IsNumber(string expression)
        {
            expression = expression.Trim();
            if (expression.Length == 0)
            {
                return false;
            }
            expression = expression.Replace(",", "").Replace(".", "").Replace("-", "");
            for (int i = 0; i < expression.Length; i++)
            {
                // only allow numeric digits
                if (!char.IsNumber(expression[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsInteger(string expression)
        {
            if (expression.Trim().Length == 0)
            {
                return false;
            }
            // leading - is ok
            expression = expression.Trim();
            int startIdx = 0;
            if (expression.StartsWith("-"))
            {
                startIdx = 1;
            }
            for (int i = startIdx; i < expression.Length; i++)
            {
                if (!char.IsNumber(expression[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsDate(string expression)
        {
            if (expression.Trim().Length == 0)
            {
                return false;
            }
            try
            {
                DateTime.Parse(expression);
                return true;
            }
            catch
            {
                return false;
            }
        }

        static public int GetRandomNumber(int lowerBound, int upperBound)
        {
            return new System.Random().Next(lowerBound, upperBound + 1);
        }

        static public String GetExceptionDetail(Exception ex, String LineSeparator)
        {
            String ExDetail = "Exception=" + ex.Message + LineSeparator;
            while (ex.InnerException != null)
            {
                ExDetail += ex.InnerException.Message + LineSeparator;
                ex = ex.InnerException;
            }
            return ExDetail;
        }

        static public String HighlightTerm(String InString, String Term)
        {
            int i = InString.ToUpperInvariant().IndexOf(Term.ToUpperInvariant());
            if (i != -1)
            {
                InString = InString.Substring(0, i) + "<b>" + InString.Substring(i, Term.Length) + "</b>" + InString.Substring(i + Term.Length, InString.Length - Term.Length - i);
            }
            return InString;
        }

        static public String Left(String s, int l)
        {
            if (s.Length <= l)
            {
                return s;
            }
            return s.Substring(0, l - 1);
        }

        // this really is never meant to be called with ridiculously  small l values (e.g. l < 10'ish)
        static public String Ellipses(String s, int l, bool BreakBetweenWords)
        {
            if (l < 1)
            {
                return String.Empty;
            }
            if (l >= s.Length)
            {
                return s;
            }
            String tmpS = Left(s, l - 2);
            if (BreakBetweenWords)
            {
                try
                {
                    tmpS = tmpS.Substring(0, tmpS.LastIndexOf(" "));
                }
                catch { }
            }
            tmpS += "...";
            return tmpS;
        }

        public static String AspHTTP(String url, int TimeoutSecs)
        {
            String result;
            try
            {
                WebResponse objResponse;
                WebRequest objRequest = System.Net.HttpWebRequest.Create(url);
                if (TimeoutSecs > 0)
                {
                    objRequest.Timeout = TimeoutSecs * 1000; // ms
                }
                else
                {
                    objRequest.Timeout = System.Threading.Timeout.Infinite;
                }
                objResponse = objRequest.GetResponse();
                using (StreamReader sr = new StreamReader(objResponse.GetResponseStream()))
                {
                    result = sr.ReadToEnd();
                    // Close and clean up the StreamReader
                    sr.Close();
                }
                objResponse.Close();
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }

        public static String ExtractBody(String ss)
        {
            try
            {
                int startAt;
                int stopAt;
                startAt = ss.IndexOf("<body");
                if (startAt == -1)
                {
                    startAt = ss.IndexOf("<BODY");
                }
                if (startAt == -1)
                {
                    startAt = ss.IndexOf("<Body");
                }
                startAt = ss.IndexOf(">", startAt);
                stopAt = ss.IndexOf("</body>");
                if (stopAt == -1)
                {
                    stopAt = ss.IndexOf("</BODY>");
                }
                if (stopAt == -1)
                {
                    stopAt = ss.IndexOf("</Body>");
                }
                if (startAt == -1)
                {
                    startAt = 1;
                }
                else
                {
                    startAt = startAt + 1;
                }
                if (stopAt == -1)
                {
                    stopAt = ss.Length;
                }
                return ss.Substring(startAt, stopAt - startAt);
            }
            catch
            {
                return String.Empty;
            }
        }

        public static void WriteFile(String fname, String contents, bool WriteFileInUTF8)
        {
            fname = SafeMapPath(fname);
            StreamWriter wr;
            if (WriteFileInUTF8)
            {
                wr = new StreamWriter(fname, false, System.Text.Encoding.UTF8, 4096);
            }
            else
            {
                wr = new StreamWriter(fname, false, System.Text.Encoding.ASCII, 4096);
            }
            wr.Write(contents);
            wr.Flush();
            wr.Close();
        }

        public static String ReadFile(String fname, bool ignoreErrors)
        {
            String contents;
            try
            {
                fname = SafeMapPath(fname);
                using (StreamReader rd = new StreamReader(fname))
                {
                    contents = rd.ReadToEnd();
                    rd.Close();
                    return contents;
                }
            }
            catch (Exception e)
            {
                if (ignoreErrors)
                    return String.Empty;
                else
                    throw e;
            }
        }

        public static String Capitalize(String s)
        {
            if (s.Length == 0)
            {
                return String.Empty;
            }
            else if (s.Length == 1)
            {
                return s.ToUpper(CultureInfo.InvariantCulture);
            }
            else
            {
                return s.Substring(0, 1).ToUpper(CultureInfo.InvariantCulture) + s.Substring(1, s.Length - 1).ToLowerInvariant();
            }
        }

        public static String ServerVariables(String paramName)
        {
            String tmpS = String.Empty;
            if (HttpContext.Current.Request.ServerVariables[paramName] != null)
            {
                try
                {
                    tmpS = HttpContext.Current.Request.ServerVariables[paramName].ToString();
                }
                catch
                {
                    tmpS = String.Empty;
                }
            }
            return tmpS;
        }

        /// <summary>
        /// Returns the customers' IPv4 or IPv6 Address. Works for proxies and clusters.
        /// </summary>
        public static string CustomerIpAddress()
        {
            // check request headers for client IP Address
            string ipAddr = ServerVariables("HTTP_X_FORWARDED_FOR").Trim(); // ssl cluster
            if (string.IsNullOrEmpty(ipAddr) || ipAddr.Equals("unknown", StringComparison.OrdinalIgnoreCase)) ipAddr = ServerVariables("HTTP_X_CLUSTER_CLIENT_IP").Trim(); // non-ssl cluster
            if (string.IsNullOrEmpty(ipAddr) || ipAddr.Equals("unknown", StringComparison.OrdinalIgnoreCase)) ipAddr = ServerVariables("HTTP_CLIENT_IP").Trim(); // proxy
            if (string.IsNullOrEmpty(ipAddr) || ipAddr.Equals("unknown", StringComparison.OrdinalIgnoreCase)) ipAddr = ServerVariables("REMOTE_ADDR").Trim(); // non-cluster

            // proxies can return a comma-separated list, so use the left-most (furthest downstream) one
            return ipAddr.Split(',')[0].Trim();
        }

		/// <summary>
		/// Determines whether request came from a secure (https) connection.
		/// </summary>
		/// <returns>
		/// <c>true</c> if request came from a secure (https) connection; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsSecureConnection()
		{
			return
				HttpContext.Current.Request.IsSecureConnection
				// Loadbalancers and other services that terminate SSL connections in front of the server
				// will set various headers indicating if the request is HTTPS
				|| StringComparer.OrdinalIgnoreCase.Equals(HttpContext.Current.Request.Headers["Cluster-Https"], "on")
				|| StringComparer.OrdinalIgnoreCase.Equals(HttpContext.Current.Request.Headers["X-Forwarded-Proto"], "https");
		}

        // can take virtual fname, or fully qualified path fname
        public static bool FileExists(String fname)
        {
            bool retVal = File.Exists(SafeMapPath(fname));
            if (!retVal && !fname.Contains(":") )
            {
                retVal = File.Exists(HttpContext.Current.Request.MapPath(fname));
            }
            return retVal;
        }

		// this is probably the implementation that Microsoft SHOULD have done!
		// use this helper function for ALL MapPath calls in the entire storefront for safety!
		public static string SafeMapPath(string fileName)
		{
			//checks for relative path
			if(string.IsNullOrEmpty(fileName) || Path.IsPathRooted(fileName))
				return fileName;
			try
			{
				//HttpContext.Current.Request threw exception in some cases in Integrated App Pool
				if(fileName.StartsWith("~"))
					return HostingEnvironment.MapPath(fileName) ?? fileName;
				else
					return HostingEnvironment.MapPath(
						string.Join(
							HostingEnvironment.ApplicationVirtualPath.EndsWith("/") || fileName.StartsWith("/")
								? string.Empty
								: "/",
							HostingEnvironment.ApplicationVirtualPath,
							fileName))
							?? fileName;
			}
			//Path is somewhere you're not allowed to access or is otherwise damaged
			catch(SecurityException ex)
			{
				throw new SecurityException("If you are running in Medium Trust you may have virtual directories defined that are not accessible at this trust level,\n " + ex.Message);
			}
			catch
			{
				//previous storefront behavior
				return fileName;
			}
		}

		public static String ExtractToken(String ss, String t1, String t2)
        {
            if (ss.Length == 0)
            {
                return String.Empty;
            }
            int i1 = ss.IndexOf(t1);
            int i2 = ss.IndexOf(t2, CommonLogic.IIF(i1 == -1, 0, i1));
            if (i1 == -1 || i2 == -1 || i1 >= i2 || (i2 - i1) <= 0)
            {
                return String.Empty;
            }
            return ss.Substring(i1, i2 - i1).Replace(t1, "");
        }

        static public String MakeSafeFilesystemName(String s)
        {
            String OKChars = "abcdefghijklmnopqrstuvwxyz1234567890_";
            s = s.ToLowerInvariant();
            StringBuilder tmpS = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                String tok = s.Substring(i, 1);
                if (OKChars.IndexOf(tok) != -1)
                {
                    tmpS.Append(tok);
                }
            }
            return tmpS.ToString();
        }

        static public Size GetImagePixelSize(String imgname)
        {
            try
            {
                //create instance of Bitmap class around specified image file
                // must use try/catch in case the image file is bogus
                using (Bitmap img = new Bitmap(SafeMapPath(imgname), false))
                {
                    return new Size(img.Width, img.Height);
                }
            }
            catch
            {
                try
                {
                    using (Bitmap img = new Bitmap(HttpContext.Current.Request.MapPath(imgname), false))
                    {
                        return new Size(img.Width, img.Height);
                    }
                }
                catch
                {
                    return new Size(0, 0);
                }
            }
        }

        static public String WrapString(String s, int ColWidth, String Separator)
        {
            StringBuilder tmpS = new StringBuilder(s.Length + 100);
            if (s.Length <= ColWidth || ColWidth == 0)
            {
                return s;
            }
            int start = 0;
            int length = Min(ColWidth, s.Length);
            while (start < s.Length)
            {
                if (tmpS.Length != 0)
                {
                    tmpS.Append(Separator);
                }
                tmpS.Append(s.Substring(start, length));
                start += ColWidth;
                length = Min(ColWidth, s.Length - start);
            }
            return tmpS.ToString();
        }

        public static String GetNewGUID()
        {
            return System.Guid.NewGuid().ToString().ToUpperInvariant();
        }

        // this version is NOT to be used to squote db sql stuff!
        public static String SQuote(String s)
        {
            return "'" + s.Replace("'", "''") + "'";
        }

        // ----------------------------------------------------------------
        //
        // PARAMS SUPPORT ROUTINES Uses Request.Params[]
        //
        // ----------------------------------------------------------------

        public static String ParamsCanBeDangerousContent(String paramName)
        {
            String tmpS = String.Empty;
            if (HttpContext.Current.Request.Params[paramName] != null)
            {
                try
                {
                    tmpS = HttpContext.Current.Request.Params[paramName];
                }
                catch
                {
                    tmpS = String.Empty;
                }
            }
            return tmpS;
        }

        // ----------------------------------------------------------------
        //
        // FORM SUPPORT ROUTINES
        //
        // ----------------------------------------------------------------
        public static String FormCanBeDangerousContent(String paramName)
        {
            String tmpS = String.Empty;
            if (HttpContext.Current.Request.Form[paramName] != null)
            {
                try
                {
                    tmpS = HttpContext.Current.Request.Form[paramName].ToString();
                }
                catch
                {
                    tmpS = String.Empty;
                }
            }
            return tmpS;
        }

        public static bool FormBool(String paramName)
        {
            String tmpS = FormCanBeDangerousContent(paramName).ToUpperInvariant();
            return (tmpS == "TRUE" || tmpS == "YES" || tmpS == "1");
        }

        public static int FormUSInt(String paramName)
        {
            String tmpS = FormCanBeDangerousContent(paramName);
            return Localization.ParseUSInt(tmpS);
        }

        public static decimal FormUSDecimal(String paramName)
        {
            String tmpS = FormCanBeDangerousContent(paramName);
            return Localization.ParseUSDecimal(tmpS);
        }

        public static int FormNativeInt(String paramName)
        {
            String tmpS = FormCanBeDangerousContent(paramName);
            return Localization.ParseNativeInt(tmpS);
        }

        public static decimal FormNativeDecimal(String paramName)
        {
            String tmpS = FormCanBeDangerousContent(paramName);
            return Localization.ParseNativeDecimal(tmpS);
        }

        // ----------------------------------------------------------------
        //
        // QUERYSTRING SUPPORT ROUTINES
        //
        // ----------------------------------------------------------------
		public static String QueryStringCanBeDangerousContent(string parameterName)
		{
			try
			{
				return (HttpContext.Current.Request.QueryString[parameterName]
					?? HttpContext.Current.Request.RequestContext.RouteData.Values[parameterName]
					?? string.Empty)
					.ToString();
			}
			catch
			{
				return string.Empty;
			}
		}

        public static bool QueryStringBool(String paramName)
        {
            String tmpS = QueryStringCanBeDangerousContent(paramName).ToUpperInvariant();
            return (tmpS == "TRUE" || tmpS == "YES" || tmpS == "1");
        }

        public static int QueryStringUSInt(String paramName)
        {
            String tmpS = QueryStringCanBeDangerousContent(paramName);
            return Localization.ParseUSInt(tmpS);
        }

        public static int QueryStringNativeInt(String paramName)
        {
            String tmpS = QueryStringCanBeDangerousContent(paramName);
            return Localization.ParseNativeInt(tmpS);
        }

        // ----------------------------------------------------------------
        //
        // SESSION SUPPORT ROUTINES --  These routines are all deprecated in favor of using the CustomerSession object in the Customer object
        //
        // ----------------------------------------------------------------

        public static String Application(String paramName)
        {
            String tmpS = String.Empty;
            if (System.Web.Configuration.WebConfigurationManager.AppSettings[paramName] != null)
            {
                try
                {
                    tmpS = System.Web.Configuration.WebConfigurationManager.AppSettings[paramName];
                }
                catch
                {
                    tmpS = String.Empty;
                }
            }
            return tmpS;
        }

        public static bool ApplicationBool(String paramName)
        {
            String tmpS = Application(paramName).ToUpperInvariant();
            return (tmpS == "TRUE" || tmpS == "YES" || tmpS == "1");
        }

        public static int ApplicationUSInt(String paramName)
        {
            String tmpS = Application(paramName);
            return Localization.ParseUSInt(tmpS);
        }

        // ----------------------------------------------------------------
        //
        // COOKIE SUPPORT ROUTINES
        //
        // ----------------------------------------------------------------

        public static String CookieCanBeDangerousContent(String paramName, bool decode)
        {
            if (HttpContext.Current.Request.Cookies[paramName] == null)
            {
                return String.Empty;
            }
            try
            {
                String tmp = HttpContext.Current.Request.Cookies[paramName].Value.ToString();
                if (decode)
                {
                    tmp = HttpContext.Current.Server.UrlDecode(tmp);
                }
                return tmp;
            }
            catch
            {
                return String.Empty;
            }
        }

        public static int CookieUSInt(String paramName)
        {
            String tmpS = CookieCanBeDangerousContent(paramName, true);
            return Localization.ParseUSInt(tmpS);
        }

        public static string UnzipBase64DataToString(String s)
        {
            string result = string.Empty;

            // So far this is only used for the Cybersource gateway
            // Cybersource requires unzipping base 64 encoded data to save as transaction proof for Streamline 3D Secure transactions.

            try
            {
                byte[] rawBytes = Convert.FromBase64String(s);

                // ZLIB format, first 2 bytes is header, last 4 bytes is checksum, throw those out
                byte[] decodedBytes = new byte[(rawBytes.Length - 6)];
                System.Buffer.BlockCopy(rawBytes, 2, decodedBytes, 0, decodedBytes.Length);

                MemoryStream ms = new MemoryStream(decodedBytes, false);
                ms.Position = 0;
                Stream zipStream = new DeflateStream(ms, CompressionMode.Decompress, false);
                StreamReader SR = new StreamReader(zipStream);
                result = SR.ReadToEnd();
            }
            catch { }

            return result;
        }

        public static string UrlFriendlyEntityName(string n)
        {
            var entityName = (n ?? "").Trim().ToLower().Replace("&", "").Replace(".", "").Replace("/", "").Replace(" ", "-");

            while (entityName.IndexOf("--") > -1)
                entityName = entityName.Replace("--", "-");

            if (entityName.IndexOf("-") == 0)
                entityName = entityName.Substring(1);

            return entityName;
        }

        /// <summary>
        /// Utility function to check if a string value is included within the commadelimited string to match with
        /// </summary>
        /// <param name="stringValue">the string to look for</param>
        /// <param name="commaDelimitedList">the comma delimited string to search at</param>
        /// <returns></returns>
        public static bool StringInCommaDelimitedStringList(string stringValue, string commaDelimitedList)
        {
            try
            {
                if (CommonLogic.IsStringNullOrEmpty(stringValue) ||
                    CommonLogic.IsStringNullOrEmpty(commaDelimitedList))
                {
                    return false;
                }

                string[] individualValues = commaDelimitedList.Split(',');

                foreach (string valueToMatch in individualValues)
                {
                    if (valueToMatch.ToUpperInvariant().Trim().Equals(stringValue.ToUpperInvariant().Trim()))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }

            return false;
        }
    }
}
