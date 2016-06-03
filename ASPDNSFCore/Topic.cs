// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;

namespace AspDotNetStorefrontCore
{
	/// <summary>
	/// Handles creating, modifying, and updating topics.
	/// Topics are free form content blocks that can be displayed as pages 
	/// or inserted into existing pages. Access /topic/name to view topic as page.
	/// </summary>
	public class Topic
	{
		private String m_TopicName = String.Empty;
		private int m_TopicID = 0;
		private String m_LocaleSetting = String.Empty;
		private int m_SkinID = 1;
		private int m_StoreID = 0;
		private int m_DisplayOrder = 1;
		private String m_Contents = String.Empty;
		private String m_ContentsRAW = String.Empty;
		private String m_SectionTitle = String.Empty;
		private String m_SETitle = String.Empty;
		private String m_SEKeywords = String.Empty;
		private String m_SEDescription = String.Empty;
		private String m_Password = String.Empty;
		private bool m_FromDB = false;
		private bool m_RequiresDisclaimer = false;
		private bool m_HtmlOk = true;
		private bool m_ShowInSiteMap = true;
		private String m_FN = String.Empty;
		private String m_MasterLocale = String.Empty;
		private readonly Hashtable m_CommandHashtable;
		private readonly Parser m_UseParser = null;
		private bool m_HasChildren = false;
		private readonly bool m_ChildMappedToSelf = false;
		private bool m_IsMapped = false;

		readonly SkinProvider SkinProvider;

		public Topic()
		{
			SkinProvider = new SkinProvider();
		}

		public Topic(int TopicID, String LocaleSetting, int SkinID)
			: this(TopicID, LocaleSetting, SkinID, null)
		{ }

		public Topic(int TopicID, String LocaleSetting, int SkinID, Parser UseParser)
			: this(TopicID, LocaleSetting, SkinID, UseParser, AppLogic.StoreID())
		{ }

		public Topic(int TopicID, String LocaleSetting, int SkinID, Parser UseParser, int StoreID)
			: this()
		{
			m_LocaleSetting = LocaleSetting;
			m_TopicID = TopicID;
			m_SkinID = SkinID;
			m_TopicName = String.Empty; //GetName(TopicID,Localization.GetWebConfigLocale());
			m_CommandHashtable = new Hashtable();
			m_UseParser = UseParser;
			m_StoreID = StoreID;
			LoadFromDB(StoreID);
		}

		public Topic(String TopicName)
			: this(TopicName, Localization.GetDefaultLocale(), 1, null, AppLogic.StoreID())
		{ }

		public Topic(String TopicName, int SkinID)
			: this(TopicName, Localization.GetDefaultLocale(), SkinID, null, AppLogic.StoreID())
		{ }

		public Topic(String TopicName, String LocaleSetting, int SkinID)
			: this(TopicName, LocaleSetting, SkinID, null)
		{ }

		public Topic(String TopicName, String LocaleSetting, int SkinID, Parser UseParser)
			: this(TopicName, LocaleSetting, SkinID, UseParser, AppLogic.StoreID())
		{ }

		public Topic(String TopicName, String LocaleSetting, int SkinID, Parser UseParser, int StoreID)
			: this()
		{
			m_TopicName = TopicName.Trim();
			m_LocaleSetting = LocaleSetting;
			m_SkinID = SkinID;
			m_TopicID = Topic.GetTopicID(TopicName, Localization.GetDefaultLocale(), StoreID); // always find topics by MASTER locale name!
			m_CommandHashtable = new Hashtable();
			m_UseParser = UseParser;
			m_StoreID = StoreID;
			LoadFromDB(StoreID);
		}

		private Customer thisCustomer;
		public Customer ThisCustomer
		{
			get
			{
				if(thisCustomer == null)
					thisCustomer = AppLogic.GetCurrentCustomer();

				return thisCustomer;
			}
		}

		public String Contents
		{
			get
			{
				return m_Contents;
			}
			set { m_Contents = value; }
		}

		public String ContentsRAW
		{
			get
			{
				return m_ContentsRAW;
			}
			set { m_ContentsRAW = value; }
		}

		public bool FromDB
		{
			get
			{
				return m_FromDB;
			}
		}

		public bool RequiresDisclaimer
		{
			get
			{
				return m_RequiresDisclaimer;
			}
			set { m_RequiresDisclaimer = value; }
		}

		public String TopicName
		{
			get
			{
				return m_TopicName;
			}
			set { m_TopicName = value; }
		}

		public String SectionTitle
		{
			get
			{
				return m_SectionTitle;
			}
			set { m_SectionTitle = value; }
		}

		public String Password
		{
			get
			{
				return m_Password;
			}
			set { m_Password = value; }
		}

		public int TopicID
		{
			get
			{
				return m_TopicID;
			}
		}

		public int DisplayOrder
		{
			get { return m_DisplayOrder; }
			set { m_DisplayOrder = value; }
		}

		public int SkinID
		{
			get
			{
				return m_SkinID;
			}
			set { m_SkinID = value; }
		}

		public String LocaleSetting
		{
			get { return m_LocaleSetting; }
			set { m_LocaleSetting = value; }
		}

		public String SETitle
		{
			get
			{
				return m_SETitle;
			}
			set { m_SETitle = value; }
		}

		public String SEKeywords
		{
			get
			{
				return m_SEKeywords;
			}
			set { m_SEKeywords = value; }
		}

		public String SEDescription
		{
			get
			{
				return m_SEDescription;
			}
			set { m_SEDescription = value; }
		}

		public bool HTMLOk
		{
			get { return m_HtmlOk; }
			set { m_HtmlOk = value; }
		}

		public bool ShowInSiteMap
		{
			get { return m_ShowInSiteMap; }
			set { m_ShowInSiteMap = value; }
		}

		// Find the specified topic content. note, we try to find content even if it doesn't exactly match the input specs, by doing an ordered lookup in various areas
		// we want to show SOME topic content if it is at all possible, even if the language is not right, etc...
		// Note: site id only used for file based topic _contents
		// Search Order is (yes, other orderings are possible, but this is the one we chose, where ANY db topic match overrides file content):
		// the other option would be to match on locales in the order of DB/File (Customer Locale), DB/File (Store Locale), DB/File (Null locale)
		// DB (customer locale)
		// DB (store locale)
		// DB (null locale)
		// File (customer locale)
		// File (store locale)
		// File (null locale)
		void LoadFromDB(int StoreID)
		{
			m_FromDB = false;
			m_DisplayOrder = 1;
			m_SkinID = ThisCustomer.SkinID;
			m_StoreID = StoreID;
			m_LocaleSetting = CommonLogic.IIF(m_LocaleSetting.Length > 0, m_LocaleSetting, Localization.GetDefaultLocale());
			m_Contents = String.Empty;
			m_ContentsRAW = String.Empty;
			m_SectionTitle = String.Empty;
			m_RequiresDisclaimer = false;
			m_ShowInSiteMap = true;
			m_Password = String.Empty;
			m_SETitle = m_TopicName;
			m_SEKeywords = String.Empty;
			m_SEDescription = String.Empty;
			m_FN = String.Empty;
			m_MasterLocale = m_LocaleSetting;
			m_HasChildren = false;

			if(m_TopicID == 0)
			{
				m_TopicID = Topic.GetTopicID(m_TopicName, CommonLogic.IIF(AppLogic.IsAdminSite, m_MasterLocale, m_LocaleSetting), AppLogic.StoreID());
			}

			if(m_TopicID != 0)
			{
				var sql = string.Format("SELECT * from Topic with (NOLOCK) where Deleted=0 and Published=1 and TopicID={0} and (SkinID IS NULL or SkinID=0 or SkinID={1}) order by DisplayOrder, Name ASC", m_TopicID.ToString(), m_SkinID.ToString());

				using(var con = new SqlConnection(DB.GetDBConn()))
				{
					con.Open();
					using(var rs = DB.GetRS(sql, con))
					{
						if(rs.Read())
						{
							m_FromDB = true;
							m_TopicID = DB.RSFieldInt(rs, "TopicID");
							m_TopicName = DB.RSField(rs, "Name");
							m_Contents = DB.RSFieldByLocale(rs, "Description", m_LocaleSetting);
							m_Password = DB.RSField(rs, "Password");
							m_RequiresDisclaimer = DB.RSFieldBool(rs, "RequiresDisclaimer");
							m_DisplayOrder = DB.RSFieldInt(rs, "DisplayOrder");
							m_ShowInSiteMap = DB.RSFieldBool(rs, "ShowInSiteMap");
							m_SkinID = DB.RSFieldInt(rs, "SkinID");
							if(m_Contents.Length != 0)
							{
								m_ContentsRAW = m_Contents;
								m_SectionTitle = DB.RSFieldByLocale(rs, "Title", m_LocaleSetting);
								m_SETitle = DB.RSFieldByLocale(rs, "SETitle", m_LocaleSetting);
								m_SEKeywords = DB.RSFieldByLocale(rs, "SEKeywords", m_LocaleSetting);
								m_SEDescription = DB.RSFieldByLocale(rs, "SEDescription", m_LocaleSetting);
							}
							else // nothing found, try master locale:
							{
								m_Contents = DB.RSFieldByLocale(rs, "Description", m_MasterLocale);
								m_ContentsRAW = m_Contents;
								m_SectionTitle = DB.RSFieldByLocale(rs, "Title", m_MasterLocale);
								m_SETitle = DB.RSFieldByLocale(rs, "SETitle", m_MasterLocale);
								m_SEKeywords = DB.RSFieldByLocale(rs, "SEKeywords", m_MasterLocale);
								m_SEDescription = DB.RSFieldByLocale(rs, "SEDescription", m_MasterLocale);
							}

							// if an html tag is present, extract just the body of the content
							if(m_Contents.IndexOf("<html", StringComparison.InvariantCultureIgnoreCase) != -1)
							{
								m_Contents = CommonLogic.ExtractBody(m_ContentsRAW);
							}
						}
					}
				}
			}

			if(!m_FromDB) // did not find anything in db, try file based topic content (in skins folder as topicname.htm)
			{
				string appdir = HttpContext.Current.Request.PhysicalApplicationPath;

				List<string> possibleFileNames = new List<string> {
                    Path.Combine(appdir, string.Format("Skins\\{0}\\Topics\\{1}.{2}.htm", SkinProvider.GetSkinNameById(SkinID), m_TopicName, m_LocaleSetting)),   //Skin specific, localized
                    Path.Combine(appdir, String.Format("Skins\\{0}\\Topics\\{1}.htm", SkinProvider.GetSkinNameById(SkinID), m_TopicName)),                        //Skin specific, unlocalized
                    Path.Combine(appdir, string.Format("Topics\\{0}.{1}.htm", m_TopicName, m_LocaleSetting)),                                               //Root folder, localized
                    Path.Combine(appdir, string.Format("Topics\\{0}.htm", m_TopicName)),                                                                    //Root folder, unlocalized
                    Path.Combine(appdir, string.Format("Skins\\{0}\\Topics\\{1}.{2}.html", SkinProvider.GetSkinNameById(SkinID), m_TopicName, m_LocaleSetting)),  //Skin specific, localized HTML
                    Path.Combine(appdir, String.Format("Skins\\{0}\\Topics\\{1}.html", SkinProvider.GetSkinNameById(SkinID), m_TopicName)),                       //Skin specific, unlocalized HTML
                    Path.Combine(appdir, string.Format("Topics\\{0}.{1}.html", m_TopicName, m_LocaleSetting)),                                              //Root folder, localized HTML
                    Path.Combine(appdir, string.Format("Topics\\{0}.html", m_TopicName))                                                                    //Root folder, unlocalized HTML
                };

				foreach(string fileNametoCheck in possibleFileNames)
				{
					m_FN = CommonLogic.SafeMapPath(fileNametoCheck);

					if(CommonLogic.FileExists(m_FN))
						break;
				}

				if(m_FN.Length != 0 && CommonLogic.FileExists(m_FN))
				{
					m_Contents = CommonLogic.ReadFile(m_FN, true);
					m_ContentsRAW = m_Contents;
					m_SectionTitle = CommonLogic.ExtractToken(m_ContentsRAW, "<title>", "</title>");
					m_Contents = CommonLogic.ExtractBody(m_Contents);

					// Throw a helpful error if the topic file is not formatted properly
					if(m_Contents.Length == 0 && m_ContentsRAW.Length > 0)
						throw new Exception(@"Make sure to format your topic file like a normal html document.
							For Example:
							<!DOCTYPE html>
							<html>
								<head>
									<title>Your title</title>
								</head>
								<body>
									Your content here
								</body>
							</html>");

					// try old token formats first, for backwards compatibility:
					m_SETitle = CommonLogic.ExtractToken(m_ContentsRAW, "<PAGETITLE>", "</PAGETITLE>");
					m_SEKeywords = CommonLogic.ExtractToken(m_ContentsRAW, "<PAGEKEYWORDS>", "</PAGEKEYWORDS>");
					m_SEDescription = CommonLogic.ExtractToken(m_ContentsRAW, "<PAGEDESCRIPTION>", "</PAGEDESCRIPTION>");

					// if regular HTML tokens found, try to parse it out in regular HTML syntax meta tag format and they take precedence over the old tokens (above):
					String t = Regex.Match(m_ContentsRAW, @"(?<=<title[^\>]*>).*?(?=</title>)", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture).Value;
					if(t.Length != 0)
					{
						m_SETitle = t;
					}

					String MK = String.Empty;
					String MV = String.Empty;
					foreach(Match metamatch in Regex.Matches(m_ContentsRAW, @"<meta\s*(?:(?:\b(\w|-)+\b\s*(?:=\s*(?:""[^""]*""|'[^']*'|[^""'<> ]+)\s*)?)*)/?\s*>", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture))
					{
						MK = String.Empty;
						MV = String.Empty;
						// Loop through the attribute/value pairs inside the tag
						foreach(Match submatch in Regex.Matches(metamatch.Value.ToString(), @"(?<name>\b(\w|-)+\b)\s*=\s*(""(?<value>[^""]*)""|'(?<value>[^']*)'|(?<value>[^""'<> ]+)\s*)+", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture))
						{
							if("http-equiv".Equals(submatch.Groups[1].ToString(), StringComparison.InvariantCultureIgnoreCase))
							{
								MV = submatch.Groups[2].ToString();
							}
							if(("name".Equals(submatch.Groups[1].ToString(), StringComparison.InvariantCultureIgnoreCase)) &&
								MK == String.Empty) // if it's already set, HTTP-EQUIV takes precedence
							{
								MV = submatch.Groups[2].ToString();
							}
							if("content".Equals(submatch.Groups[1].ToString(), StringComparison.InvariantCultureIgnoreCase))
							{
								MV = submatch.Groups[2].ToString();
							}
						}
						switch(MK.ToLowerInvariant())
						{
							case "description":
								m_SEDescription = MV;
								break;
							case "keywords":
							case "keyword":
								m_SEKeywords = MV;
								break;
						}
					}
				}
			}

			if(m_SETitle.Length == 0)
			{
				m_SETitle = m_SectionTitle;
			}

			if(AppLogic.ReplaceImageURLFromAssetMgr)
			{
				while(m_Contents.IndexOf("../images") != -1)
				{
					m_Contents = m_Contents.Replace("../images", "images");
				}
			}
			if(m_UseParser != null)
			{
				m_Contents = m_UseParser.ReplaceTokens(m_Contents);
			}
			else
			{
				if(SkinID > 0)
				{
					m_Contents = m_Contents.Replace("(!SKINID!)", SkinID.ToString());
				}
			}
		}

		#region Static Methods

		public static String GetTitle(String TopicName, String LocaleSetting, int StoreID)
		{
			int tID = GetTopicID(TopicName, LocaleSetting, StoreID);

			return GetTitle(tID, LocaleSetting);
		}

		public static string GetTitle(int TopicID, string LocaleSetting)
		{
			var tmpS = string.Empty;

			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(var rs = DB.GetRS(string.Format("select Title from Topic with (NOLOCK) where Deleted=0 and TopicID={0}", TopicID.ToString()), con))
					if(rs.Read())
						tmpS = DB.RSFieldByLocale(rs, "Title", LocaleSetting);
			}

			return tmpS;
		}

		public static int GetTopicID(string TopicName, string LocaleSetting, int StoreID)
		{
			if(string.IsNullOrEmpty(LocaleSetting))
			{
				LocaleSetting = Localization.GetDefaultLocale();
			}
			var localeMatch = BuildLocaleSearchString(LocaleSetting, TopicName);
			var tmp = 0;
			var topicSQL = "select TopicID from Topic where (StoreID={1} or 0={0}) AND Deleted=0 AND Published=1 and (lower(Name)={2} OR Name like '%' + {3} + '%') order by storeid";
			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(var rs = DB.GetRS(string.Format(topicSQL
																	, CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowTopicFiltering") == true, 1, 0)
																	, StoreID
																	, DB.SQuote(TopicName.ToLowerInvariant())
																	, DB.SQuote(localeMatch))
												, con))
				{
					if(rs.Read())
					{
						tmp = DB.RSFieldInt(rs, "TopicID");
					}
				}
			}

			if(tmp == 0)
			{
				StoreID = 0;
				using(var con = new SqlConnection(DB.GetDBConn()))
				{
					con.Open();
					using(var rs = DB.GetRS(string.Format(topicSQL
																	, CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowTopicFiltering") == true, 1, 0)
																	, StoreID
																	, DB.SQuote(TopicName.ToLowerInvariant())
																	, DB.SQuote(localeMatch))
													, con))
					{
						if(rs.Read())
						{
							tmp = DB.RSFieldInt(rs, "TopicID");
						}
					}
				}
			}
			return tmp;
		}

		private static string BuildLocaleSearchString(string LocaleSetting, string TopicName)
		{
			return "<locale name=\"" + LocaleSetting + "\">" + TopicName + "</locale>";
		}

		#endregion
	}
}
