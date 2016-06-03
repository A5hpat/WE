// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace AspDotNetStorefrontCore
{
	/// <summary>
	/// Summary description for XmlCommon.
	/// </summary>
	public class XmlCommon
	{
        public static String SerializeObject(Object pObject, System.Type objectType) 
		{
			try 
			{
				String XmlizedString = null;
				MemoryStream memoryStream = new MemoryStream();
				XmlSerializer xs = new XmlSerializer(objectType);
				XmlTextWriter XmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
				xs.Serialize(XmlTextWriter, pObject);
				memoryStream = (MemoryStream)XmlTextWriter.BaseStream;
				XmlizedString = CommonLogic.UTF8ByteArrayToString(memoryStream.ToArray());
				return XmlizedString;
			}
			catch (Exception ex)
			{
				return CommonLogic.GetExceptionDetail(ex,"\n");
			}
		}

		public static String FormatXml(XmlDocument inputXml)
		{
			StringWriter writer = new StringWriter();
			XmlTextWriter XmlWriter = new XmlTextWriter(writer);
			XmlWriter.Formatting = Formatting.Indented;
			XmlWriter.Indentation = 2;
			inputXml.WriteTo(XmlWriter);
			return writer.ToString();
		}

		public static string PrettyPrintXml(string xml)
		{
			if(string.IsNullOrEmpty(xml))
				return xml;

			var prettyXml = (new StringBuilder(xml))
				.Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>", "")
				.Replace("<?xml version=\"1.0\" encoding=\"UTF-8\"?>", "")
				.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", "")
				.Replace("<?xml version=\"1.0\" encoding=\"UTF-16\"?>", "")
				.ToString();

			var xmlDoc = new XmlDocument();
			try
			{
				xmlDoc.LoadXml(prettyXml);
			}
			catch (Exception ex)
			{
				SysLog.LogException(ex, MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
				return prettyXml;
			}

			try
			{
				using(var memoryStream = new MemoryStream())
				{
					var settings = new XmlWriterSettings();
					settings.Indent = true;

					using(var xmlWriter = XmlWriter.Create(memoryStream, settings))
					{
						xmlDoc.WriteContentTo(xmlWriter);
					}
					
					//rewind memoryStream
					memoryStream.Position = 0;

					using(var streamReader = new StreamReader(memoryStream))
					{
						prettyXml = streamReader.ReadToEnd();
					}
				}
			}
			catch(Exception ex)
			{
				SysLog.LogException(ex, MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
				return prettyXml;
			}

			return prettyXml;
		}

		// strips illegal Xml characters:
		public static String XmlEncode(String S)
		{
			if (S == null) 
			{
				return null; 
			}
            S = Regex.Replace(S, @"[^\u0009\u000A\u000D\u0020-\uD7FF\uE000-\uFFFD]", "", RegexOptions.Compiled);
			return XmlEncodeAsIs(S);
		}

		// leaves whatever data is there, and just XmlEncodes it:
		public static String XmlEncodeAsIs(String S)
		{
			if (S == null) 
			{
				return null; 
			}
            StringBuilder sTmp = new StringBuilder();
            using (StringWriter sw = new StringWriter())
            {
                using (XmlTextWriter xwr = new XmlTextWriter(sw))
                {
                    xwr.WriteString(S);
                    sTmp.Append(sw.ToString());
                    xwr.Flush();
                    xwr.Close();
                }
                sw.Close();
            }
            return sTmp.ToString();
        }

		// strips illegal Xml characters:
		public static String XmlEncodeAttribute(String S)
		{
			if (S == null) 
			{
				return null; 
			}
            S = Regex.Replace(S, @"[^\u0009\u000A\u000D\u0020-\uD7FF\uE000-\uFFFD]", "", RegexOptions.Compiled);
			return XmlEncodeAttributeAsIs(S);
		}

		// leaves whatever data is there, and just XmlEncodes it:
		public static String XmlEncodeAttributeAsIs(String S)
		{
            return XmlEncodeAsIs(S).Replace("\"","&quot;");
		}

		public static String XmlDecode(String S)
		{
			StringBuilder tmpS = new StringBuilder(S);
			String sTmp = tmpS.Replace("&quot;","\"").Replace("&apos;","'").Replace("&lt;","<").Replace("&gt;",">").Replace("&amp;","&").ToString();
			return sTmp;
		}

		// ----------------------------------------------------------------
		//
		// SIMPLE Xml FIELD ROUTINES
		//
		// ----------------------------------------------------------------

		public static String GetLocaleEntry(String S, String LocaleSetting, bool fallBack)
		{
            String tmpS = String.Empty;
            if (S.Length == 0)
            {
                return tmpS;
            }
            if (S.StartsWith("&lt;ml&gt;", StringComparison.InvariantCultureIgnoreCase))
            {
                S = XmlDecode(S);
            }
            if (S.StartsWith("<ml>", StringComparison.InvariantCultureIgnoreCase))
            {
                String WebConfigLocale = Localization.GetDefaultLocale();
                if (AppLogic.AppConfigBool("UseXmlDOMForLocaleExtraction"))
                {
                    try
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(S);
                        XmlNode node = doc.DocumentElement.SelectSingleNode("//locale[@name=\"" + LocaleSetting + "\"]");
                        if (fallBack && (node == null))
                        {
                            node = doc.DocumentElement.SelectSingleNode("//locale[@name=\"" + WebConfigLocale + "\"]");
                        }
                        if (node != null)
                        {
                            tmpS = node.InnerText.Trim();
                        }
                        if (tmpS.Length != 0)
                        {
                            tmpS = XmlCommon.XmlDecode(tmpS);
                        }
                    }
                    catch { }
                }
                else
                {
                    // for speed, we are using lightweight simple string token extraction here, not full Xml DOM for speed
                    // return what is between <locale name=\"en-US\">...</locale>, Xml Decoded properly.
                    // we have a good locale field formatted field, so try to get desired locale:
                    if (S.IndexOf("<locale name=\"" + LocaleSetting + "\">") != -1)
                    {
                        tmpS = CommonLogic.ExtractToken(S, "<locale name=\"" + LocaleSetting + "\">", "</locale>");
                    }
                    else if (fallBack && (S.IndexOf("<locale name=\"" + WebConfigLocale + "\">") != -1))
                    {
                        tmpS = CommonLogic.ExtractToken(S, "<locale name=\"" + WebConfigLocale + "\">", "</locale>");
                    }
                    else
                    {
                        tmpS = String.Empty;
                    }
                    if (tmpS.Length != 0)
                    {
                        tmpS = XmlCommon.XmlDecode(tmpS);
                    }
                }
            }
            else
            {
                tmpS = S; // for backwards compatibility...they have no locale info, so just return the field.
            }
            return tmpS;
		}

        public static bool NodeContainsAttribute(XmlNode n, String AttributeName)
        {
            return (n.Attributes[AttributeName] != null);
        }

		// assumes this "xmlnode" n has <ml>...</ml> markup on it!
		public static String GetLocaleEntry(XmlNode n, String LocaleSetting, bool fallBack)
		{
            String tmpS = String.Empty;
            if (n != null)
            {
                if (n.InnerText.StartsWith("&lt;ml&gt;", StringComparison.InvariantCultureIgnoreCase))
                {
                    return GetLocaleEntry(XmlDecode(n.InnerText), LocaleSetting, fallBack);
                }
                if (n.HasChildNodes && n.FirstChild.LocalName.Equals("ml", StringComparison.InvariantCultureIgnoreCase))
                {
                    String WebConfigLocale = Localization.GetDefaultLocale();
                    try
                    {
                        XmlNode node = n.SelectSingleNode("ml/locale[@name=\"" + LocaleSetting + "\"]");
                        if (fallBack && (node == null))
                        {
                            node = n.SelectSingleNode("ml/locale[@name=\"" + WebConfigLocale + "\"]");
                        }
                        if (node != null)
                        {
                            tmpS = node.InnerText.Trim();
                        }
                        if (tmpS.Length != 0)
                        {
                            tmpS = XmlCommon.XmlDecode(tmpS);
                        }
                    }
                    catch { }
                }
                else
                {
                    tmpS = n.InnerText.Trim(); // for backwards compatibility...they have no locale info, so just return the field.
                }
            }

            return tmpS;
        }
		
		public static String XmlField(XmlNode node, String fieldName)
		{
			String fieldVal = String.Empty;
			try
			{
				fieldVal = node.SelectSingleNode(@fieldName).InnerText.Trim();
			}
			catch {} // node might not be there
			return fieldVal;
		}

		public static String XmlFieldByLocale(XmlNode node, String fieldName, String LocaleSetting)
		{
			String fieldVal = String.Empty;
            XmlNode n = node.SelectSingleNode(@fieldName);
            
            if (n != null)
            {
                if (n.InnerText.StartsWith("&lt;ml&gt;", StringComparison.InvariantCultureIgnoreCase))
                {
                    fieldVal = GetLocaleEntry(XmlCommon.XmlDecode(n.InnerText.Trim()), LocaleSetting, true);
                }
                if (n.HasChildNodes && n.FirstChild.LocalName.Equals("ml", StringComparison.InvariantCultureIgnoreCase))
                {
                    fieldVal = GetLocaleEntry(n, LocaleSetting, true);
                }
                else
                {
                    fieldVal = n.InnerText.Trim();
                }
            }
            if (fieldVal.StartsWith("<ml>", StringComparison.InvariantCultureIgnoreCase))
            {
                fieldVal = GetLocaleEntry(fieldVal, LocaleSetting, true);
            }

			return fieldVal;
		}

		public static bool XmlFieldBool(XmlNode node, String fieldName)
		{
            String tmp = XmlField(node, fieldName);
			if( tmp.Equals("TRUE", StringComparison.InvariantCultureIgnoreCase) || 
                tmp.Equals("YES", StringComparison.InvariantCultureIgnoreCase) || 
                tmp.Equals("1", StringComparison.InvariantCultureIgnoreCase))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public static int XmlFieldUSInt(XmlNode node, String fieldName)
		{
			String tmpS = XmlField(node,fieldName);
			return Localization.ParseUSInt(tmpS);
		}

		public static int XmlFieldNativeInt(XmlNode node, String fieldName)
		{
			String tmpS = XmlField(node,fieldName);
			return Localization.ParseNativeInt(tmpS);
		}

		public static decimal XmlFieldNativeDecimal(XmlNode node, String fieldName)
		{
			String tmpS = XmlField(node,fieldName);
			return Localization.ParseNativeDecimal(tmpS);
		}

		public static DateTime XmlFieldNativeDateTime(XmlNode node, String fieldName)
		{
			String tmpS = XmlField(node,fieldName);
			return Localization.ParseNativeDateTime(tmpS);
		}

		// ----------------------------------------------------------------
		//
		// SIMPLE Xml ATTRIBUTE ROUTINES
		//
		// ----------------------------------------------------------------


        public static String XmlAttribute(XmlNode node, String AttributeName)
        {
            if (node == null)
            {
                return String.Empty;
            }

            String AttributeVal = String.Empty;
            try
            {
                if (node.Attributes != null && node.Attributes[AttributeName] != null)
                {
                    AttributeVal = node.Attributes[AttributeName].InnerText.Trim();
                }
            }
            catch { } // node might not be there
            return AttributeVal;
        }

		public static bool XmlAttributeBool(XmlNode node, String AttributeName)
		{
            String tmp = XmlAttribute(node, AttributeName);

			if( tmp.Equals("TRUE", StringComparison.InvariantCultureIgnoreCase) || 
                tmp.Equals("YES", StringComparison.InvariantCultureIgnoreCase) || 
                tmp.Equals("1", StringComparison.InvariantCultureIgnoreCase))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public static int XmlAttributeUSInt(XmlNode node, String AttributeName)
		{
			String tmpS = XmlAttribute(node,AttributeName);
			return Localization.ParseUSInt(tmpS);
		}

		public static decimal XmlAttributeUSDecimal(XmlNode node, String AttributeName)
		{
			String tmpS = XmlAttribute(node,AttributeName);
			return Localization.ParseUSDecimal(tmpS);
		}

		public static int XmlAttributeNativeInt(XmlNode node, String AttributeName)
		{
			String tmpS = XmlAttribute(node,AttributeName);
			return Localization.ParseNativeInt(tmpS);
		}

		public static decimal XmlAttributeNativeDecimal(XmlNode node, String AttributeName)
		{
			String tmpS = XmlAttribute(node,AttributeName);
			return Localization.ParseNativeDecimal(tmpS);
		}

		public static DateTime XmlAttributeNativeDateTime(XmlNode node, String AttributeName)
		{
			String tmpS = XmlAttribute(node,AttributeName);
			return Localization.ParseNativeDateTime(tmpS);
		}

        /// <summary>
        /// Send and capture data using GET
        /// </summary>
        /// <param name="Request">The Xml Request to be sent</param>
        /// <param name="Server">The server the request should be sent to</param>
        /// <returns>String</returns>
        public static String GETandReceiveData(String Request, String Server)
        {
            // check for cache hit:
            String CacheName = Server + Request;
            String s = (String)HttpContext.Current.Cache.Get(CacheName);
            if (s != null)
            {
                return s;
            }
            HttpWebRequest requestX = (HttpWebRequest)WebRequest.Create(Server + "?" + Request);
            HttpWebResponse response = (HttpWebResponse)requestX.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream());
            String result = sr.ReadToEnd();
            response.Close();
            sr.Close();
            sr.Dispose();

            // cache result. if there was no error in it!
            if (result.IndexOf("error:", StringComparison.InvariantCultureIgnoreCase) != -1)
            {
                try
                {
                    HttpContext.Current.Cache.Remove(CacheName);
                }
                catch { }
            }
            else
            {
                HttpContext.Current.Cache.Insert(CacheName, result, null, System.DateTime.Now.AddMinutes(15), TimeSpan.Zero);
            }

            return result;
        }
	}
}
