// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{
	public class SkinBase : Page
	{
		private string m_SectionTitle = string.Empty;
		private string m_TemplateName = string.Empty;
		private string m_SETitle = string.Empty;
		private string m_SEDescription = string.Empty;
		private string m_SEKeywords = string.Empty;
		private Parser m_Parser;

		/// <summary>
		/// Determines which masterpage skin to use to render the site to the browsing customer
		/// </summary>
		/// <returns>The name of the file that is the masterpage</returns>
		private string GetTemplateName()
		{
			return "template.master";
		}

		protected override void OnPreInit(EventArgs e)
		{
			if(HttpContext.Current != null)
			{
				//Have to call GetPropertyValue once before you actually need it to initialize the PropertyValues collection
				if(HttpContext.Current.Profile != null) 
					HttpContext.Current.Profile.GetPropertyValue("SkinID");
				
				//SkinId querystring overrides everything but mobile
				if(CommonLogic.QueryStringUSInt("skinid") > 0)
				{
					SkinID = CommonLogic.QueryStringUSInt("skinid");

					//Customer has a querystring so save this to the profile.
					if(HttpContext.Current.Profile != null)
						HttpContext.Current.Profile.SetPropertyValue("SkinID", SkinID.ToString());
				}
				//Check to see if we are previewing the skin
				else if(CommonLogic.QueryStringUSInt("previewskinid") > 0)
				{
					SkinID = CommonLogic.QueryStringUSInt("previewskinid");

					//Customer has a preview querystring so save this to the profile.
					if(HttpContext.Current.Profile != null)
						HttpContext.Current.Profile.SetPropertyValue("PreviewSkinID", SkinID.ToString());
				}
				//Use the preview profile value if we have one
				else if(HttpContext.Current.Profile != null
					&& HttpContext.Current.Profile.PropertyValues["PreviewSkinID"] != null
					&& CommonLogic.IsInteger(HttpContext.Current.Profile.GetPropertyValue("PreviewSkinID").ToString()))
				{
					int skinFromProfile = int.Parse(HttpContext.Current.Profile.GetPropertyValue("PreviewSkinID").ToString());
					if(skinFromProfile > 0)
					{
						SkinID = skinFromProfile;
					}
				}
				//Pull the skinid from the current profile
				else if(HttpContext.Current.Profile != null && CommonLogic.IsInteger(HttpContext.Current.Profile.GetPropertyValue("SkinID").ToString()))
				{
					int skinFromProfile = int.Parse(HttpContext.Current.Profile.GetPropertyValue("SkinID").ToString());
					if(skinFromProfile > 0)
					{
						SkinID = skinFromProfile;
					}
				}

				//Now save the skinID to the customer record.  This is not used OOB.
				if(ThisCustomer.SkinID != SkinID)
				{
					ThisCustomer.SkinID = SkinID;
					ThisCustomer.UpdateCustomer(new SqlParameter[] { new SqlParameter("SkinID", SkinID) });
				}

				if(CommonLogic.QueryStringUSInt("affiliateid") > 0)
				{
					var affiliateId = CommonLogic.QueryStringUSInt("affiliateid");
					var affiliate = new Affiliate(affiliateId);
					if(!AppLogic.GlobalConfigBool("AllowAffiliateFiltering") || affiliate.StoreID == AppLogic.StoreID())
						HttpContext.Current.Profile.SetPropertyValue("AffiliateID", affiliateId.ToString());
				}

				if(HttpContext.Current.Request.UrlReferrer != null && HttpContext.Current.Request.UrlReferrer.Authority != HttpContext.Current.Request.Url.Authority)
				{
					HttpContext.Current.Profile.SetPropertyValue("Referrer", HttpContext.Current.Request.UrlReferrer.ToString());
				}

				Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(Localization.GetDefaultLocale());
				Thread.CurrentThread.CurrentUICulture = new CultureInfo(ThisCustomer.LocaleSetting);

				m_TemplateName = GetTemplateName();

				m_Parser = new Parser();

				string SkinDirectory = string.Empty;
				string PageTheme = string.Empty;

				SkinDirectory = "Skin_" + SkinID.ToString();
				PageTheme = "Skin_" + SkinID.ToString();

				if(!m_TemplateName.EndsWith(".master", StringComparison.OrdinalIgnoreCase))
				{
					m_TemplateName = m_TemplateName + ".master";
				}

				MasterPageFile = "~/App_Templates/" + SkinDirectory + "/" + m_TemplateName;
				Theme = PageTheme;

				if(!CommonLogic.FileExists(MasterPageFile))
				{

					SkinID = AppLogic.DefaultSkinID();

					m_TemplateName = "template.master";
					SkinDirectory = "Skin_" + SkinID.ToString();
					PageTheme = "Skin_" + SkinID.ToString();

					MasterPageFile = "~/App_Templates/" + SkinDirectory + "/" + m_TemplateName;
					Theme = PageTheme;
				}
			}

			base.OnPreInit(e);
		}

		/// <summary>
		/// Responsible for replacing localized strings in child controls
		/// </summary>
		/// <param name="controls"></param>
		private void IterateControls(ControlCollection controls)
		{
			for(int i = 0; i < controls.Count; i++)
			{
				Control c = controls[i];
				if(c.ID != "PageContent")
				{
					ProcessControl(c, true);
				}
			}
		}

		private void ProcessControl(Control ctl, bool includeChildren)
		{
			IEditableTextControl e = ctl as IEditableTextControl;
			if(e != null)
			{
				e.Text = ReplaceTokens(e.Text);
			}
			else
			{
				ITextControl t = ctl as ITextControl;
				if(t != null)
				{
					t.Text = ReplaceTokens(t.Text);
				}
			}
			IValidator v = ctl as IValidator;
			if(v != null)
			{
				v.ErrorMessage = ReplaceTokens(v.ErrorMessage);
			}
			
			if(includeChildren && ctl.HasControls())
			{
				IterateControls(ctl.Controls);
			}
		}

		/// <summary>
		/// Method responsible for setting dynamic page properties, as well as replacing parser tokens in child controls
		/// </summary>
		/// <param name="writer"></param>
		protected override void Render(System.Web.UI.HtmlTextWriter writer)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");

			if(SETitle.Length == 0)
			{
				SETitle = AppLogic.AppConfig("SE_MetaTitle");
			}
			if(SEDescription.Length == 0)
			{
				SEDescription = AppLogic.AppConfig("SE_MetaDescription");
			}
			if(SEKeywords.Length == 0)
			{
				SEKeywords = AppLogic.AppConfig("SE_MetaKeywords");
			}

			if(AppLogic.GlobalConfigBool("ShowStoreHint"))
			{
				var stores = Store.GetStoreList();
				Store selectedStore = null;
				selectedStore = stores.FirstOrDefault(store => store.StoreID.Equals(AppLogic.StoreID()));
				if(selectedStore != null)
				{
					SETitle = "[{0}] - ".FormatWith(selectedStore.Name) + SETitle;
				}
			}

			//IterateControls(Controls);
			base.Render(writer);
		}
		
		private string ReplaceTokens(String s)
		{
			if(s.IndexOf("(!") == -1)
			{
				return s;
			}
			// process SKIN specific tokens here only:
			s = s.Replace("(!SECTION_TITLE!)", SectionTitle);
			if(SectionTitle.Length != 0)
			{
				s = s.Replace("(!SUPERSECTIONTITLE!)", "<div align=\"left\"><span class=\"SectionTitleText\">" + SectionTitle + "</span><small>&nbsp;</small></div>");
				if(AppLogic.IsAdminSite)
				{
					s = s.Replace("class=\"SectionTitleText\"", "class=\"breadCrumb3\"");
				}
			}
			else
			{
				s = s.Replace("(!SUPERSECTIONTITLE!)", "");
			}

			s = s.Replace("(!METATITLE!)", CommonLogic.IIF(CommonLogic.StringIsAlreadyHTMLEncoded(m_SETitle), m_SETitle, HttpContext.Current.Server.HtmlEncode(m_SETitle)));
			s = s.Replace("(!METADESCRIPTION!)", CommonLogic.IIF(CommonLogic.StringIsAlreadyHTMLEncoded(m_SEDescription), m_SEDescription, HttpContext.Current.Server.HtmlEncode(m_SEDescription)));
			s = s.Replace("(!METAKEYWORDS!)", CommonLogic.IIF(CommonLogic.StringIsAlreadyHTMLEncoded(m_SEKeywords), m_SEKeywords, HttpContext.Current.Server.HtmlEncode(m_SEKeywords)));
			s = GetParser.ReplaceTokens(s);
			return s;
		}

		protected virtual void RenderContents(System.Web.UI.HtmlTextWriter writer) { }

		private Customer m_ThisCustomer;
		public Customer ThisCustomer
		{
			get
			{
				if(m_ThisCustomer == null)
					m_ThisCustomer = HttpContext.Current.GetCustomer();

				return m_ThisCustomer;
			}
			set
			{
				m_ThisCustomer = value;
			}
		}

		public Parser GetParser
		{
			get
			{
				return m_Parser;
			}
		}

		/// <summary>
		/// Gets or sets the SectionTitle text
		/// </summary>
		public string SectionTitle
		{
			get
			{
				if(AppLogic.IsAdminSite)
				{
					return m_SectionTitle;
				}
				else
				{
					if(Master is MasterPageBase)
					{
						return (Master as MasterPageBase).SectionTitleText;
					}

					return string.Empty;
				}
			}
			set
			{
				if(AppLogic.IsAdminSite)
				{
					m_SectionTitle = value;
				}
				else
				{
					if(Master is MasterPageBase)
					{
						(Master as MasterPageBase).SectionTitleText = value;
					}
				}
			}
		}

		public string SETitle
		{
			get
			{
				if(AppLogic.IsAdminSite)
				{
					return m_SETitle;
				}
				else
				{
					if(CommonLogic.IsStringNullOrEmpty(Title))
					{
						return string.Empty;
					}

					return Title;
				}
			}
			set
			{
				if(AppLogic.IsAdminSite)
				{
					m_SETitle = value;
				}
				else
				{
					Title = value;
				}
			}
		}

		private const string META_KEYWORDS = "keywords";
		private const string META_DESCRIPTION = "description";

		public string SEKeywords
		{
			get
			{
				if(AppLogic.IsAdminSite)
				{
					return m_SEKeywords;
				}
				else
				{
					return GetMetaContent(META_KEYWORDS);
				}
			}
			set
			{
				if(AppLogic.IsAdminSite)
				{
					m_SEKeywords = value;
				}
				else
				{
					SetMetaContent(META_KEYWORDS, value);
				}
			}
		}

		public string SEDescription
		{
			get
			{
				if(AppLogic.IsAdminSite)
				{
					return m_SEDescription;
				}
				else
				{
					return GetMetaContent(META_DESCRIPTION);
				}
			}
			set
			{
				if(AppLogic.IsAdminSite)
				{
					m_SEDescription = value;
				}
				else
				{
					SetMetaContent(META_DESCRIPTION, value);
				}
			}
		}

		private int m_SkinID = 0;
		new public int SkinID
		{
			get
			{
				if(m_SkinID == 0)
					m_SkinID = AppLogic.GetStoreSkinID(AppLogic.StoreID());

				return m_SkinID;
			}
			set
			{
				m_SkinID = value;
			}
		}

		private string GetMetaContent(string name)
		{
			if(Header != null)
			{
				foreach(Control c in Header.Controls)
				{
					HtmlMeta meta = c as HtmlMeta;
					if(meta != null &&
						meta.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
					{
						return meta.Content;
					}
				}
			}

			return string.Empty;
		}

		private void SetMetaContent(string name, string value)
		{
			if(Header != null)
			{
				foreach(Control c in Header.Controls)
				{
					HtmlMeta meta = c as HtmlMeta;
					if(meta != null &&
						meta.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
					{
						meta.Content = value;
					}
				}
			}
		}

		/// <summary>
		/// Gets whether to require asp.net ajax script manager
		/// </summary>
		public virtual bool RequireScriptManager
		{
			get
			{
				// false by default since not all pages require this functionality
				return false;
			}
		}

		/// <summary>
		/// If overridden, provides access to the script manager to render script and services references
		/// </summary>
		/// <param name="scrptMgr"></param>
		public virtual void RegisterScriptAndServices(ScriptManager scrptMgr) { }
	}
}
