// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using System.Xml;
using System.Xml.XPath;
using AspDotNetStorefront.Routing;

namespace AspDotNetStorefrontCore
{
	public class InputValidator
	{
		private String m_RoutineName = String.Empty;

		public InputValidator(String RoutineName)
		{
			m_RoutineName = RoutineName;
		}

		private void ReportError(String ParamName, String ParamValue)
		{
			ArgumentException ex = new ArgumentException("Error Calling XSLTExtension Function " + m_RoutineName + ": Invalid value specified for " + ParamName + " (" + CommonLogic.IIF(ParamValue == null, "null", ParamValue) + ")");
			SysLog.LogException(ex, MessageTypeEnum.XmlPackageException, MessageSeverityEnum.Error);
			throw ex;
		}

		public String ValidateString(String ParamName, String ParamValue)
		{
			if(ParamValue == null)
			{
				ReportError(ParamName, ParamValue);
			}
			return ParamValue;
		}

		public int ValidateInt(String ParamName, String ParamValue)
		{
			if(ParamValue == null ||
				!CommonLogic.IsInteger(ParamValue))
			{
				ReportError(ParamName, ParamValue);
			}
			return Int32.Parse(ParamValue);
		}

		public Decimal ValidateDecimal(String ParamName, String ParamValue)
		{
			if(ParamValue == null ||
				!CommonLogic.IsNumber(ParamValue))
			{
				ReportError(ParamName, ParamValue);
			}
			return Localization.ParseDBDecimal(ParamValue);
		}

		public Double ValidateDouble(String ParamName, String ParamValue)
		{
			if(ParamValue == null ||
				!CommonLogic.IsNumber(ParamValue))
			{
				ReportError(ParamName, ParamValue);
			}
			return Localization.ParseDBDouble(ParamValue);
		}

		public bool ValidateBool(String ParamName, String ParamValue)
		{
			if(ParamValue == null)
			{
				ReportError(ParamName, ParamValue);
			}

			if(ParamValue.Equals("TRUE", StringComparison.InvariantCultureIgnoreCase) ||
				ParamValue.Equals("YES", StringComparison.InvariantCultureIgnoreCase) ||
				ParamValue.Equals("1", StringComparison.InvariantCultureIgnoreCase))
			{
				return true;
			}

			return false;
		}

		public DateTime ValidateDateTime(String ParamName, String ParamValue)
		{
			DateTime dt = DateTime.MinValue;
			if(ParamValue == null)
			{
				ReportError(ParamName, ParamValue);
			}
			try
			{
				dt = Localization.ParseDBDateTime(ParamValue);
			}
			catch
			{
				ReportError(ParamName, ParamValue);
			}
			return dt;
		}
	}

	/// <summary>
	/// Summary description for XSLTExtensions.
	/// </summary>
	public class XSLTExtensionBase
	{
		/// <summary>
		/// Javascript items
		/// </summary>
		protected static class Scripts
		{
			public static string PopupImage(Size picSize)
			{
				var urlHelper = DependencyResolver.Current.GetService<UrlHelper>();
                string _script =
				#region PopupImg Script
				 @"
					<script type='text/javascript'>
						function popupimg(url)
						{{
							window.open(
								'{0}?{1}=' + document.getElementById('popupImageURL').value,'LargerImage{2}',
								'toolbar=no,location=no,directories=no,status=no, menubar=no,scrollbars={3}, resizable={3},copyhistory=no,width={4},height={5},left=0,top=0');
							return (true);
						}}
					</script>
				";
				#endregion

				return string.Format(_script,
					new object[]
					{
						urlHelper.Action(ActionNames.PopUp, ControllerNames.Image),
						RouteDataKeys.ImagePath,
						CommonLogic.GetRandomNumber(1, 100000),
						AppLogic.AppConfigBool("ResizableLargeImagePopup")? "yes": "no",
						picSize.Width,
						picSize.Height,
					});
			}
		}

		protected bool m_VATEnabled = false;
		protected bool m_VATOn = false;
		protected Customer m_ThisCustomer;
		protected Dictionary<string, EntityHelper> m_EntityHelpers;
		public HtmlHelper HtmlHelper { get; set; }
		readonly SkinProvider SkinProvider;
		readonly UrlHelper Url;

		public XSLTExtensionBase(Customer cust, int SkinID)
		{
			Url = DependencyResolver.Current.GetService<UrlHelper>();
			SkinProvider = new SkinProvider();

			m_ThisCustomer = cust;
			if(m_ThisCustomer == null)
			{
				try
				{
					m_ThisCustomer = HttpContext.Current.GetCustomer();
				}
				catch
				{
				}
				if(m_ThisCustomer == null)
				{
					m_ThisCustomer = new Customer(true);
				}
			}
			m_VATEnabled = AppLogic.AppConfigBool("VAT.Enabled");
			m_VATOn = (m_VATEnabled && m_ThisCustomer.VATSettingReconciled == VATSettingEnum.ShowPricesInclusiveOfVAT);
		}

		//Public Methods
		public Customer ThisCustomer
		{
			get { return m_ThisCustomer; }
		}

		public virtual string SetTrace(string sTraceName)
		{
			InputValidator IV = new InputValidator("SetTrace");
			String TraceName = IV.ValidateString("TraceName", sTraceName);
			if(!HttpContext.Current.Items.Contains("XmlPackageTracePoint"))
			{
				HttpContext.Current.Items.Add("XmlPackageTracePoint", TraceName);
			}
			else
			{
				HttpContext.Current.Items["XmlPackageTracePoint"] = TraceName;
			}
			return String.Empty;
		}

		public virtual String GetRootEntityContextOfPage(String sEntityName)
		{
			InputValidator IV = new InputValidator("GetRootEntityContextOfPage");
			String EntityName = IV.ValidateString("VariantID", sEntityName);

			EntityHelper catHelper = AppLogic.LookupHelper(EntityName, 0);
			int ProductIDQS = CommonLogic.QueryStringUSInt("ProductID");
			int EntityIDQS = CommonLogic.QueryStringUSInt(EntityName + "ID");
			if(CommonLogic.QueryStringCanBeDangerousContent("EntityID").Length != 0)
			{
				EntityIDQS = CommonLogic.QueryStringUSInt("EntityID");
			}
			int EntityIDCookie = CommonLogic.IIF(CommonLogic.CookieCanBeDangerousContent("LastViewedEntityName", true) == EntityName, CommonLogic.CookieUSInt("LastViewedEntityInstanceID"), 0);
			int EntityIDActual = 0;
			int EntityIDRoot = 0;
			if(ProductIDQS != 0)
			{
				// we have a product context, did they get there from a Entity/subcat page or not. cookie is set if so.
				if(EntityIDCookie != 0)
				{
					EntityIDActual = EntityIDCookie;
				}
				else
				{
					EntityIDActual = AppLogic.GetFirstProductEntityID(catHelper, ProductIDQS, false);
				}
			}
			else
			{
				if(EntityIDQS != 0)
				{
					EntityIDActual = EntityIDQS;
				}
			}
			EntityIDRoot = catHelper.GetRootEntity(EntityIDActual);
			return EntityIDRoot.ToString();
		}

		public virtual string RemoteUrl(string sURL)
		{
			InputValidator IV = new InputValidator("RemoteUrl");
			String URL = IV.ValidateString("URL", sURL);
			return CommonLogic.AspHTTP(URL, 30);
		}

		public string StripHtml(String sTheString)
		{
			InputValidator IV = new InputValidator("StripHtml");
			String TheString = IV.ValidateString("TheString", sTheString);
			return AppLogic.StripHtml(TheString);
		}

		public virtual string PagingControl(string sBaseURL, String sPageNum, String sNumPages)
		{
			InputValidator IV = new InputValidator("PagingControl");
			String BaseURL = IV.ValidateString("BaseURL", sBaseURL);
			int PageNum = IV.ValidateInt("PageNum", sPageNum);
			int NumPages = IV.ValidateInt("NumPage", sNumPages);

			if(AppLogic.AppConfigBool("Paging.ShowAllPageLinks"))
				return Paging.GetAllPagesOldFormat(BaseURL, PageNum, NumPages, ThisCustomer);
			else
				return Paging.GetPagedPages(BaseURL, PageNum, NumPages, ThisCustomer);
		}

		public virtual string SkinID()
		{
			return ThisCustomer.SkinID.ToString();
		}

		public virtual string SkinName()
		{
			return SkinProvider.GetSkinNameById(ThisCustomer.SkinID);
		}

		public virtual void SendMail(String sSubject, String sBody, String sUseHtml, String sToAddress)
		{
			InputValidator IV = new InputValidator("SendMail");
			String Subject = IV.ValidateString("Subject", sSubject);
			String Body = IV.ValidateString("Body", sBody);
			bool UseHtml = IV.ValidateBool("UseHtml", sUseHtml);
			String ToAddress = IV.ValidateString("ToAddress", sToAddress);
			String Srv = AppLogic.MailServer().Trim();
			if(Srv.Length != 0 &&
				Srv != AppLogic.ro_TBD)
			{
				AppLogic.SendMail(subject: Subject, body: Body, useHtml: UseHtml, fromAddress: AppLogic.AppConfig("MailMe_FromAddress"), fromName: AppLogic.AppConfig("MailMe_FromAddress"), toAddress: ToAddress, toName: ToAddress, bccAddresses: String.Empty, replyToAddress: AppLogic.AppConfig("MailMe_FromAddress"));
			}
		}

		public virtual string CustomerID()
		{
			return ThisCustomer.CustomerID.ToString();
		}

		public virtual string User_Name()
		{
			var result = string.Empty;
			if(!ThisCustomer.IsRegistered)
			{
				result = string.Empty;
			}
			else
			{
				if(AppLogic.IsAdminSite)
				{
					result = ThisCustomer.FullName();
				}
				else
				{
					var accountLink = Url.Action(
						actionName: ActionNames.Index,
						controllerName: ControllerNames.Account);

					result = "Welcome,  <a class=\"username\" href=\"" + accountLink + "\">" + ThisCustomer.FullName() + "</a>" + CommonLogic.IIF(ThisCustomer.CustomerLevelID != 0, "(" + ThisCustomer.CustomerLevelName + ")", "");
				}
			}
			return result;
		}

		public virtual string User_Menu_Name()
		{
			string result = String.Empty;
			if(!ThisCustomer.IsRegistered)
			{
				result = "My Account";
			}
			else
			{
				result = ThisCustomer.FullName();
			}
			return result;
		}

		public virtual string Store_Version(String sNotUsed)
		{
			return StoreVersion(sNotUsed);
		}

		public virtual string StoreVersion(String sNotUsed)
		{
			return CommonLogic.GetVersion();
		}

		public virtual string OnLiveServer(String sNotUsed)
		{
			return AppLogic.OnLiveServer().ToString().ToLowerInvariant();
		}

		//The following functions are for backward compatibility with Parser functions only and 
		//should not be used in XmlPackage transforms because the output invalid XML when using the IncludeATag
		//Newer fucntions that produce well formed output are below
		public virtual string ManufacturerLink(String sManufacturerID, String sSEName, String sIncludeATag)
		{
			InputValidator IV = new InputValidator("ManufacturerLink");
			int ManufacturerID = IV.ValidateInt("ManufacturerID", sManufacturerID);
			String SEName = IV.ValidateString("SEName", sSEName);
			bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
			string result = String.Empty;
			result = Url.BuildEntityLink(EntityTypes.Manufacturer, ManufacturerID, SEName);
			if(IncludeATag)
			{
				result = "<a href=\"" + result + "\" />";
			}
			return result;
		}

		public virtual string CategoryLink(String sCategoryID, String sSEName, String sIncludeATag)
		{
			InputValidator IV = new InputValidator("CategoryLink");
			int CategoryID = IV.ValidateInt("CategoryID", sCategoryID);
			String SEName = IV.ValidateString("SEName", sSEName);
			bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
			string result = String.Empty;
			result = Url.BuildEntityLink(EntityTypes.Category, CategoryID, SEName);
			if(IncludeATag)
			{
				result = "<a href=\"" + result + "\" />";
			}
			return result;
		}

		public virtual string SectionLink(String sSectionID, String sSEName, String sIncludeATag)
		{
			InputValidator IV = new InputValidator("SectionLink");
			int SectionID = IV.ValidateInt("SectionID", sSectionID);
			String SEName = IV.ValidateString("SEName", sSEName);
			bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
			string result = String.Empty;
			result = Url.BuildEntityLink(EntityTypes.Section, SectionID, SEName);
			if(IncludeATag)
			{
				result = "<a href=\"" + result + "\" />";
			}
			return result;
		}

		public virtual string ProductLink(String sProductID, String sSEName, String sIncludeATag)
		{
			InputValidator IV = new InputValidator("ProductLink");
			int ProductID = IV.ValidateInt("ProductID", sProductID);
			String SEName = IV.ValidateString("SEName", sSEName);
			bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
			string result = String.Empty;
			result = Url.BuildProductLink(ProductID, SEName);
			if(IncludeATag)
			{
				result = "<a href=\"" + result + "\" />";
			}
			return result;
		}

		public virtual string ProductandCategoryLink(String sProductID, String sSEName, String sCategoryID, String sIncludeATag)
		{
			InputValidator IV = new InputValidator("ProductandCategoryLink");
			int ProductID = IV.ValidateInt("ProductID", sProductID);
			String SEName = IV.ValidateString("SEName", sSEName);
			bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
			string result = String.Empty;
			result = Url.BuildProductLink(ProductID, SEName);
			if(IncludeATag)
			{
				result = "<a href=\"" + result + "\">";
			}
			return result;
		}

		public virtual string ProductandSectionLink(String sProductID, String sSEName, String sSectionID, String sIncludeATag)
		{
			InputValidator IV = new InputValidator("ProductandSectionLink");
			int ProductID = IV.ValidateInt("ProductID", sProductID);
			String SEName = IV.ValidateString("SEName", sSEName);
			bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
			string result = String.Empty;
			result = Url.BuildProductLink(ProductID, SEName);
			if(IncludeATag)
			{
				result = "<a href=\"" + result + "\">";
			}
			return result;
		}

		public virtual string ProductandManufacturerLink(String sProductID, String sSEName, String sManufacturerID, String sIncludeATag)
		{
			InputValidator IV = new InputValidator("ProductandManufacturerLink");
			int ProductID = IV.ValidateInt("ProductID", sProductID);
			String SEName = IV.ValidateString("SEName", sSEName);
			bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
			string result = String.Empty;
			result = Url.BuildProductLink(ProductID, SEName);
			if(IncludeATag)
			{
				result = "<a href=\"" + result + "\">";
			}
			return result;
		}

		public virtual string EntityLink(String sEntityID, String sSEName, String sEntityName, String sIncludeATag)
		{
			InputValidator IV = new InputValidator("EntityLink");
			String SEName = IV.ValidateString("SEName", sSEName);
			String EntityName = IV.ValidateString("EntityName", sEntityName);
			int EntityID = IV.ValidateInt("EntityID", sEntityID);
			bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
			string result = String.Empty;
			result = Url.BuildEntityLink(EntityName, EntityID, SEName);
			if(IncludeATag)
			{
				result = "<a href=\"" + result + "\">";
			}
			return result;
		}

		public virtual string ObjectLink(String sObjectID, String sSEName, String sObjectName, String sIncludeATag)
		{
			InputValidator IV = new InputValidator("ObjectLink");
			int ObjectID = IV.ValidateInt("ObjectID", sObjectID);
			String ObjectName = IV.ValidateString("ObjectName", sObjectName);
			String SEName = IV.ValidateString("SEName", sSEName);
			bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);

			string result;
			if(AppLogic.ro_SupportedEntities.Contains(ObjectName, StringComparer.OrdinalIgnoreCase))
				result = Url.BuildEntityLink(ObjectName, ObjectID, SEName);
			else if(StringComparer.OrdinalIgnoreCase.Equals(ObjectName, "product"))
				result = Url.BuildProductLink(ObjectID, SEName);
			else
				result = string.Empty;

			if(IncludeATag)
			{
				result = "<a href=\"" + result + "\">";
			}
			return result;
		}

		public virtual string TopicLink(String sTopicName, String sIncludeATag)
		{
			InputValidator IV = new InputValidator("TopicLink");
			String TopicName = IV.ValidateString("TopicName", sTopicName);
			bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
			string result = String.Empty;
			result = Url.BuildTopicLink(TopicName);
			if(IncludeATag)
			{
				result = "<a href=\"" + result + "\">";
			}
			return result;
		}

		public virtual string ProductandEntityLink(String sProductID, String sSEName, String sEntityID, String sEntityName, String sIncludeATag)
		{
			InputValidator IV = new InputValidator("ProductandEntityLink");
			int ProductID = IV.ValidateInt("ProductID", sProductID);
			String SEName = IV.ValidateString("SEName", sSEName);
			bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
			string result = String.Empty;
			result = Url.BuildProductLink(ProductID, SEName);
			if(IncludeATag)
			{
				result = "<a href=\"" + result + "\">";
			}
			return result;
		}

		/// <summary>
		/// Creates a link to the Distributor page.
		/// </summary>
		/// <param name="DistributorID">ID of the Distributor</param>
		/// <param name="SEName">The Search Engine name of the distributor</param>
		/// <param name="IncludeATag">Flag to create an achor tag</param>
		/// <param name="TagInnerText">The innertext of the anchor tag</param>
		/// <returns>Returns an SE encoded page name</returns>
		public string DistributorLink(string sDistributorID, String sSEName, String sIncludeATag, string sTagInnerText)
		{
			InputValidator IV = new InputValidator("DistributorLink");
			int DistributorID = IV.ValidateInt("DistributorID", sDistributorID);
			String SEName = IV.ValidateString("SEName", sSEName);
			bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
			String TagInnerText = IV.ValidateString("TagInnerText", sTagInnerText);
			string result = String.Empty;
			result = Url.BuildEntityLink(EntityTypes.Distributor, DistributorID, SEName);
			if(IncludeATag)
			{
				result = "<a href=\"" + result + "\" />" + TagInnerText + "</a>";
			}
			return result;
		}

		/// <summary>
		/// Creates a link to the Genre page.
		/// </summary>
		/// <param name="GenreID">ID of the Genre</param>
		/// <param name="SEName">The Search Engine name of the Genre</param>
		/// <param name="IncludeATag">Flag to create an achor tag</param>
		/// <param name="TagInnerText">The innertext of the anchor tag</param>
		/// <returns>Returns an SE encoded page name</returns>
		public string GenreLink(string sGenreID, String sSEName, String sIncludeATag, string sTagInnerText)
		{
			InputValidator IV = new InputValidator("GenreLink");
			int GenreID = IV.ValidateInt("GenreID", sGenreID);
			String SEName = IV.ValidateString("SEName", sSEName);
			bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
			String TagInnerText = IV.ValidateString("TagInnerText", sTagInnerText);
			string result = String.Empty;
			result = Url.BuildEntityLink(EntityTypes.Genre, GenreID, SEName);
			if(IncludeATag)
			{
				result = "<a href=\"" + result + "\" />" + TagInnerText + "</a>";
			}
			return result;
		}

		/// <summary>
		/// Creates a link to the Vector page.
		/// </summary>
		/// <param name="VectorID">ID of the Vector</param>
		/// <param name="SEName">The Search Engine name of the Show</param>
		/// <param name="IncludeATag">Flag to create an achor tag</param>
		/// <param name="TagInnerText">The innertext of the anchor tag</param>
		/// <returns>Returns an SE encoded page name</returns>
		public string VectorLink(string sVectorID, String sSEName, String sIncludeATag, string sTagInnerText)
		{
			InputValidator IV = new InputValidator("ShowLink");
			int VectorID = IV.ValidateInt("VectorID", sVectorID);
			String SEName = IV.ValidateString("SEName", sSEName);
			bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
			String TagInnerText = IV.ValidateString("TagInnerText", sTagInnerText);
			string result = String.Empty;
			result = Url.BuildEntityLink(EntityTypes.Vector, VectorID, SEName);
			if(IncludeATag)
			{
				result = "<a href=\"" + result + "\">" + TagInnerText + "</a>";
			}
			return result;
		}

		public virtual string ManufacturerLink(String sManufacturerID, String sSEName, String sIncludeATag, String sTagInnerText)
		{
			return ManufacturerLink(sManufacturerID, sSEName, sIncludeATag, sTagInnerText, "0");
		}

		public virtual string ManufacturerLink(String sManufacturerID, String sSEName, String sIncludeATag, String sTagInnerText, String sFullURL)
		{
			InputValidator IV = new InputValidator("ManufacturerLink");
			int ManufacturerID = IV.ValidateInt("ManufacturerID", sManufacturerID);
			String SEName = IV.ValidateString("SEName", sSEName);
			bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
			String TagInnerText = IV.ValidateString("TagInnerText", sTagInnerText);
			string result = String.Empty;
			result = Url.BuildEntityLink(EntityTypes.Manufacturer, ManufacturerID, SEName);
			bool FullURL = IV.ValidateBool("FullURL", sFullURL);

			if(FullURL)
			{
				result = string.Format("{0}://{1}{2}", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Authority, result);
			}

			if(IncludeATag)
			{
				result = "<a href=\"" + result + "\">" + TagInnerText + "</a>";
			}
			return result;
		}

		public virtual string CategoryLink(string sCategoryID, String sSEName, String sIncludeATag, string sTagInnerText)
		{
			InputValidator IV = new InputValidator("CategoryLink");
			int CategoryID = IV.ValidateInt("CategoryID", sCategoryID);
			String SEName = IV.ValidateString("SEName", sSEName);
			bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
			String TagInnerText = IV.ValidateString("TagInnerText", sTagInnerText);
			string result = String.Empty;
			result = Url.BuildEntityLink(EntityTypes.Category, CategoryID, SEName);
			if(IncludeATag)
			{
				result = "<a href=\"" + result + "\">" + TagInnerText + "</a>";
			}
			return result;
		}

		public virtual string SectionLink(string sSectionID, String sSEName, String sIncludeATag, string sTagInnerText)
		{
			InputValidator IV = new InputValidator("SectionLink");
			int SectionID = IV.ValidateInt("SectionID", sSectionID);
			String SEName = IV.ValidateString("SEName", sSEName);
			bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
			String TagInnerText = IV.ValidateString("TagInnerText", sTagInnerText);
			string result = String.Empty;
			result = Url.BuildEntityLink(EntityTypes.Section, SectionID, SEName);
			if(IncludeATag)
			{
				result = "<a href=\"" + result + "\">" + TagInnerText + "</a>";
			}
			return result;
		}

		public virtual string ProductLink(String sProductID, String sSEName, String sIncludeATag, String sTagInnerText)
		{
			return ProductLink(sProductID, sSEName, sIncludeATag, sTagInnerText, "0");
		}

		public virtual string ProductLink(String sProductID, String sSEName, String sIncludeATag, String sTagInnerText, String sJustProductPage)
		{
			return ProductLink(sProductID, sSEName, sIncludeATag, sTagInnerText, sJustProductPage, "0");
		}

		public virtual string ProductLink(String sProductID, String sSEName, String sIncludeATag, String sTagInnerText, String sJustProductPage, String sFullURL)
		{
			InputValidator IV = new InputValidator("ProductLink");
			int ProductID = IV.ValidateInt("ProductID", sProductID);
			String SEName = IV.ValidateString("SEName", sSEName);
			bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
			String TagInnerText = IV.ValidateString("TagInnerText", sTagInnerText);
			bool JustProductPage = IV.ValidateBool("JustProductPage", sJustProductPage);
			bool FullURL = IV.ValidateBool("FullURL", sFullURL);

			string result = String.Empty;
			result = Url.BuildProductLink(ProductID, SEName);

			if(FullURL)
			{
				result = string.Format("{0}://{1}{2}", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Authority, result);
			}
			else if(JustProductPage)
			{
				result = result.TrimStart('/');
				result = result.Substring(result.IndexOf('/') + 1);
			}

			if(IncludeATag)
			{
				result = "<a href=\"" + result + "\">" + TagInnerText + "</a>";
			}
			return result;
		}

		public virtual string BuildRoute(string action, string controller)
		{
			return BuildRoute(action, controller, null);
		}

		//The optional Xml node passed into this method - with msxsl:node-set() - needs to be formatted like this:
		//
		//<xsl:variable name="OrderInfo" >
		//	<param name="Name1" value="Value1" />
		//	<param name="Name2" value="Value2" />
		//</xsl:variable>
		public virtual string BuildRoute(string action, string controller, XPathNodeIterator routeValues)
		{
			return BuildRoute(action, controller, routeValues, "false");
		}

		public virtual string BuildRoute(string action, string controller, XPathNodeIterator routeValues, string isAbsolute)
		{
			var protocol = "http";
			var httpContext = HttpContext.Current;
			if(httpContext != null)
				protocol = httpContext.Request.Url.Scheme;

			return BuildRoute(action, controller, routeValues, isAbsolute, protocol);
		}

		public virtual string BuildRoute(string action, string controller, XPathNodeIterator routeValues, string isAbsolute, string protocol)
		{
			var inputValidator = new InputValidator("BuildRoute");
			var validatedAction = inputValidator.ValidateString("Action", action);
			var validatedController = inputValidator.ValidateString("Controller", controller);
			var valuesDictionary = ExtractRouteValuesFromXPathNodeIterator(routeValues);
			var validatedIsAbsolute = inputValidator.ValidateBool("IsAbsolute", isAbsolute);
			var validatedProtocol = inputValidator.ValidateString("Protocol", protocol);

			// Return a relative url if that's what was asked for
			if(!validatedIsAbsolute)
				return Url.Action(
					actionName: validatedAction,
					controllerName: validatedController,
					routeValues: valuesDictionary);

			return Url.Action(
				actionName: validatedAction,
				controllerName: validatedController,
				routeValues: valuesDictionary,
				protocol: validatedProtocol);
		}

		public virtual string BuildRouteForStore(string action, string controller, string storeId)
		{
			return BuildRouteForStore(action, controller, storeId, null, "http");
		}

		public virtual string BuildRouteForStore(string action, string controller, string storeId, XPathNodeIterator routeValues, string protocol)
		{
			var inputValidator = new InputValidator("BuildRoute");
			var validatedAction = inputValidator.ValidateString("Action", action);
			var validatedController = inputValidator.ValidateString("Controller", controller);
			var valuesDictionary = ExtractRouteValuesFromXPathNodeIterator(routeValues);
			var validatedStoreId = inputValidator.ValidateInt("StoreId", storeId);
			var validatedProtocol = inputValidator.ValidateString("Protocol", protocol);

			return Url.ActionForStore(
				actionName: validatedAction,
				controllerName: validatedController,
				routeValues: valuesDictionary,
				storeId: validatedStoreId,
				scheme: validatedProtocol);
		}

		public virtual string Action(string action, string controller)
		{
			return Action(action, controller, null);
		}

		public virtual string Action(string action, string controller, XPathNodeIterator routeValues)
		{
			var inputValidator = new InputValidator("Action");
			var validatedAction = inputValidator.ValidateString("Action", action);
			var validatedController = inputValidator.ValidateString("Controller", controller);
			var valuesDictionary = ExtractRouteValuesFromXPathNodeIterator(routeValues);

			var actionHtml = HtmlHelper.Action(
				actionName: validatedAction,
				controllerName: validatedController,
				routeValues: valuesDictionary);

			return actionHtml.ToString();
		}

		RouteValueDictionary ExtractRouteValuesFromXPathNodeIterator(XPathNodeIterator routeValues)
		{
			//Was an Xml node passed in at all?
			if(routeValues == null)
				return null;

			//Is there any data in it?
			if(!routeValues.MoveNext())
				return null;

			//Move into the child nodes
			if((!routeValues.Current.MoveToChild("param", string.Empty)))
				return null;

			var valuesDictionary = new RouteValueDictionary();

			//Now add the values
			do
			{
				if(routeValues.Current.NodeType != XPathNodeType.Element)
					continue;

				var value = routeValues.Current.GetAttribute("value", string.Empty);
				var parsedValue = ParseRouteDataValue(value);
				valuesDictionary.Add(routeValues.Current.GetAttribute("name", string.Empty), parsedValue);
			}
			while(routeValues.Current.MoveToNext("param", string.Empty));

			return valuesDictionary;
		}

		object ParseRouteDataValue(string value)
		{
			int intValue;
			if(value == null)
				return null;
			else if(StringComparer.OrdinalIgnoreCase.Equals(value, bool.TrueString))
				return true;
			else if(StringComparer.OrdinalIgnoreCase.Equals(value, bool.FalseString))
				return false;
			else if(int.TryParse(value, out intValue))
				return intValue;
			else
				return value;
		}

		/// <summary>
		/// overload method that calls xmlpackage that displays related products. sets parameter value of enclosedTab to true
		/// </summary>
		/// <param name="sProductID">the product id to look for related products</param>
		/// <returns>returns string html to be rendered</returns>
		public virtual string RelatedProducts(string sProductID)
		{
			return RelatedProducts(sProductID, true.ToString());
		}

		/// <summary>
		/// calls xml package that displays related products
		/// </summary>
		/// <param name="sProductID">the product id to look for related products</param>
		/// <param name="sEncloseInTab">set to true if not to be displayed in a tabUI</param>
		/// <returns>returns string html to be rendered</returns>
		public virtual string RelatedProducts(string sProductID, string sEncloseInTab)
		{
			InputValidator IV = new InputValidator("RelatedProducts");
			bool encloseInTab = IV.ValidateBool("EncloseInTab", sEncloseInTab);
			string CustomerViewID = String.Empty;

			bool renderXMLPackage = (ThisCustomer.IsRegistered || HttpContext.Current.Items["OriginalSessionID"] != null);
			if(renderXMLPackage)
			{
				if(ThisCustomer.IsRegistered)
				{
					CustomerViewID = ThisCustomer.CustomerID.ToString();
				}
				else
				{
					CustomerViewID = HttpContext.Current.Items["OriginalSessionID"].ToString();
				}
				string runtimeParams = string.Format("ProductID={0}&EncloseInTab={1}&customerGuid={2}", sProductID, encloseInTab.ToString().ToLowerInvariant(), CustomerViewID);
				return AppLogic.RunXmlPackage("relatedproducts.xml.config", null, ThisCustomer, Convert.ToInt32(SkinID()), "", runtimeParams, false, false);
			}
			else
			{
				return string.Empty;
			}
		}

		public virtual string ProductandCategoryLink(string sProductID, String sSEName, string sCategoryID, String sIncludeATag, string sTagInnerText)
		{
			InputValidator IV = new InputValidator("ProductandCategoryLink");
			int ProductID = IV.ValidateInt("ProductID", sProductID);
			String SEName = IV.ValidateString("SEName", sSEName);
			bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
			String TagInnerText = IV.ValidateString("TagInnerText", sTagInnerText);
			string result = String.Empty;
			result = Url.BuildProductLink(ProductID, SEName);
			if(IncludeATag)
			{
				result = "<a href=\"" + result + "\">" + TagInnerText + "</a>";
			}
			return result;
		}

		public virtual string ProductandSectionLink(string sProductID, String sSEName, string sSectionID, String sIncludeATag, string sTagInnerText)
		{
			InputValidator IV = new InputValidator("ProductandSectionLink");
			int SectionID = IV.ValidateInt("SectionID", sSectionID);
			int ProductID = IV.ValidateInt("ProductID", sProductID);
			String SEName = IV.ValidateString("SEName", sSEName);
			bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
			String TagInnerText = IV.ValidateString("TagInnerText", sTagInnerText);
			string result = String.Empty;
			result = Url.BuildProductLink(ProductID, SEName);
			if(IncludeATag)
			{
				result = "<a href=\"" + result + "\">" + TagInnerText + "</a>";
			}
			return result;
		}

		public virtual string ProductandManufacturerLink(string sProductID, String sSEName, string sManufacturerID, String sIncludeATag, string sTagInnerText)
		{
			InputValidator IV = new InputValidator("ProductandManufacturerLink");
			int ProductID = IV.ValidateInt("ProductID", sProductID);
			String SEName = IV.ValidateString("SEName", sSEName);
			bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
			String TagInnerText = IV.ValidateString("TagInnerText", sTagInnerText);
			string result = String.Empty;
			result = Url.BuildProductLink(ProductID, SEName);
			if(IncludeATag)
			{
				result = "<a href=\"" + result + "\">" + TagInnerText + "</a>";
			}
			return result;
		}

		public virtual string ProductProperName(string sProductID, string sVariantID)
		{
			InputValidator IV = new InputValidator("ProductProperName");
			int ProductID = IV.ValidateInt("ProductID", sProductID);
			int VariantID = IV.ValidateInt("VariantID", sVariantID);
			string result = String.Empty;
			result = AppLogic.MakeProperProductName(ProductID, VariantID, ThisCustomer.LocaleSetting);
			return result;
		}

		public virtual string EntityLink(string sEntityID, String sSEName, String sEntityName, String sIncludeATag, string sTagInnerText)
		{
			InputValidator IV = new InputValidator("EntityLink");
			int EntityID = IV.ValidateInt("EntityID", sEntityID);
			String SEName = IV.ValidateString("SEName", sSEName);
			String EntityName = IV.ValidateString("EntityName", sEntityName);
			bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
			String TagInnerText = IV.ValidateString("TagInnerText", sTagInnerText);
			string result = String.Empty;
			result = Url.BuildEntityLink(EntityName, EntityID, SEName);
			if(IncludeATag)
			{
				result = "<a href=\"" + result + "\">" + TagInnerText + "</a>";
			}
			return result;
		}

		public virtual string ObjectLink(string sObjectID, String sSEName, String sObjectName, String sIncludeATag, string sTagInnerText)
		{
			InputValidator IV = new InputValidator("ObjectLink");
			String ObjectName = IV.ValidateString("ObjectName", sObjectName);
			int ObjectID = IV.ValidateInt("ObjectID", sObjectID);
			String SEName = IV.ValidateString("SEName", sSEName);
			bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
			String TagInnerText = IV.ValidateString("TagInnerText", sTagInnerText);

			string result;
			if(AppLogic.ro_SupportedEntities.Contains(ObjectName, StringComparer.OrdinalIgnoreCase))
				result = Url.BuildEntityLink(ObjectName, ObjectID, SEName);
			else if(StringComparer.OrdinalIgnoreCase.Equals(ObjectName, "product"))
				result = Url.BuildProductLink(ObjectID, SEName);
			else
				result = string.Empty;

			if(IncludeATag)
			{
				result = "<a href=\"" + result + "\">" + TagInnerText + "</a>";
			}
			return result;
		}

		public virtual string ProductandEntityLink(String sProductID, String sSEName, String sEntityID, String sEntityName, String sIncludeATag, String sTagInnerText)
		{
			return ProductandEntityLink(sProductID, sSEName, sEntityID, sEntityName, sIncludeATag, sTagInnerText, "0");
		}

		public virtual string ProductandEntityLink(String sProductID, String sSEName, String sEntityID, String sEntityName, String sIncludeATag, String sTagInnerText, String sJustProductPage)
		{
			return ProductandEntityLink(sProductID, sSEName, sEntityID, sEntityName, sIncludeATag, sTagInnerText, sJustProductPage, "0");
		}

		public virtual string ProductandEntityLink(String sProductID, String sSEName, String sEntityID, String sEntityName, String sIncludeATag, String sTagInnerText, String sJustProductPage, String sFullURL)
		{
			InputValidator IV = new InputValidator("ProductandEntityLink");
			int ProductID = IV.ValidateInt("ProductID", sProductID);
			String SEName = IV.ValidateString("SEName", sSEName);
			bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
			String TagInnerText = IV.ValidateString("TagInnerText", sTagInnerText);
			bool JustProductPage = IV.ValidateBool("JustProductPage", sJustProductPage);
			bool FullURL = IV.ValidateBool("FullURL", sFullURL);

			string result = String.Empty;
			result = Url.BuildProductLink(ProductID, SEName);

			if(FullURL)
			{
				result = string.Format("{0}://{1}{2}", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Authority, result);
			}
			else if(JustProductPage)
			{
				result = result.TrimStart('/');
				result = result.Substring(result.IndexOf('/') + 1);
			}

			if(IncludeATag)
			{
				result = "<a href=\"" + result + "\">" + TagInnerText + "</a>";
			}

			return result;
		}

		public virtual string ApplicationRelativeUrl(string url)
		{
			return VirtualPathUtility.ToAbsolute(url);
		}

        public virtual string Topic(String sTopicName, string sTopicID)
        {
            return Topic(sTopicName, sTopicID, AppLogic.StoreID());
        }

        public virtual string Topic(String sTopicName, string sTopicID, int StoreId)
        {
            InputValidator IV = new InputValidator("Topic");
            String TopicName = IV.ValidateString("TopicName", sTopicName);
            int TopicID = IV.ValidateInt("TopicID", sTopicID);
            String LCL = ThisCustomer.LocaleSetting;
            string result = String.Empty;
            if (TopicID != 0)
            {
                Topic t = new Topic(TopicID, LCL, ThisCustomer.SkinID, null, StoreId);
                result = t.Contents;
            }

            if (TopicName.Length != 0)
            {
                Topic t = new Topic(TopicName, LCL, ThisCustomer.SkinID, null, StoreId);
                result = t.Contents;
            }
            Parser p = new Parser();
            result = p.ReplaceTokens(result);
            return result;
        }

        public virtual string ReceiptTopic(String sTopicName, int OrderNumber)
        {
            int StoreID = Order.GetOrderStoreID(OrderNumber);
            InputValidator IV = new InputValidator("Topic");
            String TopicName = IV.ValidateString("TopicName", sTopicName);
            String LCL = ThisCustomer.LocaleSetting;
            string result = String.Empty;
            if (TopicName.Length != 0)
            {
                Topic t = new Topic(sTopicName, LCL, ThisCustomer.SkinID, null, StoreID);
                result = t.Contents;
                Parser p = new Parser();
                result = p.ReplaceTokens(result);
            }

            return result;
        }

        public virtual string Topic(String sTopicName)
        {
            return Topic(sTopicName, AppLogic.StoreID());
        }

        public virtual string Topic(String sTopicName, int StoreID)
        {
            InputValidator IV = new InputValidator("Topic");
            String TopicName = IV.ValidateString("TopicName", sTopicName);
            String LCL = ThisCustomer.LocaleSetting;
            string result = String.Empty;
            if (TopicName.Length != 0)
            {
                Topic t = new Topic(TopicName, LCL, ThisCustomer.SkinID, null, StoreID);
                result = t.Contents;
                Parser p = new Parser();
                result = p.ReplaceTokens(result);
            }
            return result;
        }

        public virtual string AppConfig(String sAppConfigName)
        {
            InputValidator IV = new InputValidator("AppConfig");
            String AppConfigName = IV.ValidateString("AppConfigName", sAppConfigName);
            string result = String.Empty;
            if (AppConfigName.Length != 0)
            {
                result = AppLogic.AppConfig(AppConfigName);
            }
            return result;
        }

        public virtual string AppConfig(string storeID, String sAppConfigName)
        {
            InputValidator IV = new InputValidator("AppConfig");
            String AppConfigName = IV.ValidateString("AppConfigName", sAppConfigName);
            string result = String.Empty;
            if (AppConfigName.Length != 0)
            {
                result = AppLogic.AppConfig(AppConfigName, Localization.ParseUSInt(storeID), true);
            }
            return result;
        }

        public virtual string AppConfigBool(String sAppConfigName)
        {
            InputValidator IV = new InputValidator("AppConfigBool");
            String AppConfigName = IV.ValidateString("AppConfigName", sAppConfigName);
            return AppLogic.AppConfigBool(AppConfigName).ToString().ToLowerInvariant();
        }

        public Boolean EvalBool(string sEvalString)
        {
            InputValidator IV = new InputValidator("EvalBool");
            String EvalString = IV.ValidateString("EvalString", sEvalString);

            if (EvalString.Equals("TRUE", StringComparison.InvariantCultureIgnoreCase) ||
                EvalString.Equals("YES", StringComparison.InvariantCultureIgnoreCase) ||
                EvalString.Equals("1", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public virtual string StringResource(String sStringResourceName)
        {
            InputValidator IV = new InputValidator("StringResource");
            String StringResourceName = IV.ValidateString("StringResourceName", sStringResourceName);
            // undocumented diagnostic mode:
            if (AppLogic.AppConfigBool("ShowStringResourceKeys"))
            {
                return StringResourceName;
            }
            string result = String.Empty;
            if (StringResourceName.Length != 0)
            {
                result = AppLogic.GetString(StringResourceName, ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
            }
            return result;
        }

        public virtual string StringResource(String sStringResourceName, String sLocaleSetting)
        {
            InputValidator IV = new InputValidator("StringResource");
            String StringResourceName = IV.ValidateString("StringResourceName", sStringResourceName);
            String LocaleSetting = IV.ValidateString("LocaleSetting", sLocaleSetting);
            // undocumented diagnostic mode:
            if (AppLogic.AppConfigBool("ShowStringResourceKeys"))
            {
                return StringResourceName;
            }
            string result = String.Empty;
            if (StringResourceName.Length != 0)
            {
                result = AppLogic.GetString(StringResourceName, ThisCustomer.SkinID, LocaleSetting);
            }
            return result;
        }

        //uses a delimited list of params to format a StringResource that has format tags in it
        public virtual string StrFormatStringresource(string sStringResourceName, string sFormatParams, string sDelimiter)
        {
            InputValidator IV = new InputValidator("StrFormatStringresource");
            String StringResourceName = IV.ValidateString("StringResourceName", sStringResourceName);
            String FormatParams = IV.ValidateString("FormatParams", sFormatParams);
            String Delimiter = IV.ValidateString("Delimiter", sDelimiter);
            char[] delim = Delimiter.ToCharArray();
            string[] rParams = FormatParams.Split(delim);
            return String.Format(StringResource(StringResourceName), rParams);
        }

		[Obsolete("deprecated (10.0) no longer used")]
		public virtual string SearchBox()
		{
			return string.Empty;
		}

		[Obsolete("deprecated (10.0) no longer used")]
		public virtual string HelpBox()
        {
	        return string.Empty;
        }

        public virtual string AddtoCartForm(string sProductID, string sVariantID, string sColorChangeProductImage)
        {
			return AddtoCartForm(sProductID, sVariantID, sColorChangeProductImage, AppLogic.AppConfigBool("ShowWishButtons").ToString());
        }

		public virtual string AddtoCartForm(string sProductID, string sVariantID, string sColorChangeProductImage, string sShowWishListButton)
		{
			return AddtoCartForm(sProductID, sVariantID, sColorChangeProductImage, sShowWishListButton, string.Empty, string.Empty);
		}

		public virtual string AddtoCartForm(string sProductId, string sVariantId, string sColorChangeProductImage, string sShowWishListButton, string selectedSize, string selectedColor)
        {
			var inputValidator = new InputValidator("AddtoCartForm");
			var productId = inputValidator.ValidateInt("ProductID", sProductId);
			var variantId = inputValidator.ValidateInt("VariantID", sVariantId);
			var colorSelectorChangesImage = inputValidator.ValidateBool("ColorChangeProductImage", sColorChangeProductImage);
			var showWishlistButton = inputValidator.ValidateBool("ShowWishListButton", sShowWishListButton);

			if(variantId == 0)
				variantId = AppLogic.GetDefaultProductVariant(productId);

			if(productId == 0 || variantId == 0)
				throw new Exception("Please specify a valid product id");

			if(HtmlHelper == null)
				throw new Exception("An HtmlHelper is required for this method. Make sure to specify one when you call the RunXmlPackage method");

			var addToCartHtml = HtmlHelper.Action(
				actionName: ActionNames.AddToCartForm,
				controllerName: ControllerNames.ShoppingCart,
				routeValues: new
				{
					productId = productId,
					variantId = variantId,
					showWishlistButton = showWishlistButton,
					selectedSize = selectedSize,
					selectedColor = selectedColor,
					colorSelectorChangesImage = colorSelectorChangesImage
				});

			return addToCartHtml.ToString();
		}

		public virtual string SimpleAddToCartForm(string sProductId, string sShowWishListButton)
		{
			var inputValidator = new InputValidator("AddtoCartForm");
			var productId = inputValidator.ValidateInt("ProductID", sProductId);
			var showWishlistButton = inputValidator.ValidateBool("ShowWishListButton", sShowWishListButton);

			if(HtmlHelper == null)
				throw new Exception("An HtmlHelper is required for this method. Make sure to specify one when you call the RunXmlPackage method");

			if(productId == 0)
				throw new Exception("Please specify a valid product id");

			var addToCartHtml = HtmlHelper.Action(
				actionName: ActionNames.SimpleAddToCartForm,
				controllerName: ControllerNames.ShoppingCart,
				routeValues: new
				{
					productId = productId,
					showWishlistButton = showWishlistButton
				});

			return addToCartHtml.ToString();
		}

		//Deprecated methods,  Use the methods that accept image Alt Text
		public virtual string LookupImage(string sID, string sEntityOrObjectName, string sDesiredSize, string sIncludeATag)
		{
			var IV = new InputValidator("LookupImage");
			var ID = IV.ValidateInt("ID", sID);
			var EntityOrObjectName = IV.ValidateString("EntityOrObjectName", sEntityOrObjectName);
			var DesiredSize = IV.ValidateString("DesiredSize", sDesiredSize);
			var result = string.Empty;

			var sku = string.Empty;
			var IFO = string.Empty;
			var AltText = string.Empty;

			if(EntityOrObjectName.Equals("PRODUCT", StringComparison.InvariantCultureIgnoreCase))
			{
				using(var con = new SqlConnection(DB.GetDBConn()))
				{
					con.Open();
					var query = string.Format("select SKU, ImageFilenameOverride, SEAltText from product where productid = {0}", sID);
					using(var dr = DB.GetRS(query, con))
					{
						if(dr.Read())
						{
							sku = DB.RSField(dr, "SKU");
							IFO = DB.RSField(dr, "ImageFilenameOverride");
							AltText = DB.RSFieldByLocale(dr, "SEAltText", ThisCustomer.LocaleSetting);
						}
					}
				}

				if(DesiredSize.Equals("MEDIUM", StringComparison.InvariantCultureIgnoreCase))
				{
					StringBuilder tmpS = new StringBuilder(4096);
					tmpS.Append(@"<div class=""image-wrap medium-image-wrap"">");
					String ProductPicture = String.Empty;
					ProductPicture = AppLogic.LookupImage("Product", ID, "medium", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
					String LargePic = AppLogic.LookupImage("Product", ID, "large", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
					bool HasLargePic = !LargePic.Contains("nopicture");
					String LargePicForPopup = LargePic;

					var hasProductPicture = ProductPicture.IndexOf("nopicture") == -1;

					// setup multi-image gallery:
					ProductImageGallery ImgGal = null;
					var ImgGalCacheName = string.Format("ImgGal_{0}_{1}_{2}", ID, ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
					if(AppLogic.CachingOn)
					{
						ImgGal = (ProductImageGallery)HttpContext.Current.Cache.Get(ImgGalCacheName);
					}
					if(ImgGal == null)
					{
						ImgGal = new ProductImageGallery(ID, ThisCustomer.SkinID, ThisCustomer.LocaleSetting, sku);
					}
					if(AppLogic.CachingOn)
					{
						HttpContext.Current.Cache.Insert(ImgGalCacheName, ImgGal, null, DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
					}
					tmpS.Append(ImgGal.ImgDHTML);

					if(HasLargePic && !ImgGal.HasSomeLarge)
					{
						Size size = CommonLogic.GetImagePixelSize(LargePic);
						tmpS.Append(Scripts.PopupImage(size));
					}
					tmpS.AppendFormat(@"<div class=""medium-image-wrap"" id=""divProductPic{0}"">{1}", ID, Environment.NewLine);
					if(ImgGal.ImgDHTML.Length == 0 || !ImgGal.HasSomeLarge)
					{
						if(HasLargePic)
						{
							tmpS.AppendFormat(
								@"<img id=""ProductPic{0}"" name=""ProductPic{0}"" class=""product-image large-image img-responsive""{1} alt=""{2}"" src=""{3}"" />",
								ID,
								hasProductPicture
									? string.Format(
										@" onClick=""{0}""",
										ImgGal.HasSomeLarge
											? string.Format("popuplarge_{0}()", ID)
											: string.Format("popupimg('{0}')", LargePicForPopup))
									: string.Empty,
								AltText.Replace("\"", "&quot;"),
								ProductPicture);
						}
						else
						{
							tmpS.AppendFormat(
								@"<img id=""ProductPic{0}"" name=""ProductPic{0}"" class=""product-image large-image img-responsive"" src=""{1}"" alt=""{2}"" />",
								ID,
								ProductPicture,
								AltText.Replace("\"", "&quot;"));
						}
					}
					else
					{
						if(ImgGal.HasSomeLarge)
						{
							tmpS.AppendFormat(
								@"<img id=""ProductPic{0}"" name=""ProductPic{0}"" class=""{1}""{2} alt=""{3}"" src=""{4}"" />",
								ID,
								hasProductPicture
									? "actionelement product-image large-image img-responsive"
									: "product-image large-image img-responsive",
								hasProductPicture
									? string.Format(@" onClick=""popuplarge_{0}()""", ID)
									: string.Empty,
								AltText.Replace("\"", "&quot;"),
								ProductPicture);
						}
						else
						{
							tmpS.AppendFormat(
								@"<img id=""ProductPic{0}"" name=""ProductPic{0}"" class=""product-image large-image img-responsive"" src=""{1}"" alt=""{2}"" />",
								ID,
								ProductPicture,
								AltText.Replace("\"", "&quot;"));
						}
					}
					tmpS.Append("</div>");
					tmpS.Append(@"<div class=""image-controls"">");
					if(ImgGal.ImgGalIcons.Length != 0)
					{
						tmpS.Append(@"<div class=""image-icons"">");
						tmpS.Append(ImgGal.ImgGalIcons);
						tmpS.Append("</div>");
					}

					if(ImgGal.HasSomeLarge && hasProductPicture)
					{
						tmpS.AppendFormat(
							@"<div class=""view-larger-wrap""><a href=""javascript:void(0);"" onClick=""popuplarge_{0}()"">{1}</a></div>",
							ID,
							"showproduct.aspx.43".StringResource());
					}
					else if(HasLargePic && hasProductPicture)
					{
						tmpS.AppendFormat(
							@"<div class=""view-larger-wrap""><a href=""javascript:void(0);"" onClick=""{0}"">{1}</a></div>",
							ImgGal.HasSomeLarge
								? string.Format("popuplarge_{0}()", ID)
								: string.Format("popupimg('{0}')", LargePicForPopup),
							"showproduct.aspx.43".StringResource());
					}
					tmpS.Append("</div>");
					tmpS.Append("</div>");
					result = tmpS.ToString();
				}
				else
				{
					result = AppLogic.LookupImage("Product", ID, IFO, sku, DesiredSize.ToLowerInvariant(), ThisCustomer.SkinID, ThisCustomer.LocaleSetting);

					// we must ALWAYS return an image here back to Xsl (this is a little different than the prior version logic, where large did not have a "no picture" returned!)
					if(result.Length == 0)
					{
						result = AppLogic.NoPictureImageURL(DesiredSize.Equals("icon", StringComparison.InvariantCultureIgnoreCase), ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
					}
					if(result.Length != 0)
					{
						result = string.Format(
							@"<img id=""ProductPic{0}"" name=""ProductPic{0}"" class=""actionelement"" src=""{1}"" alt=""{2}"" />",
							ID,
							result,
							AltText.Replace("\"", "&quot;"));
					}
				}
			}

			else if(EntityOrObjectName.Equals("DOCUMENT", StringComparison.InvariantCultureIgnoreCase))
			{
				result = string.Empty; // not supported yet
			}
			else
			{
				// a category, section, or manufacturer, etc...
				using(var con = new SqlConnection(DB.GetDBConn()))
				{
					con.Open();
					var query = string.Format("select ImageFilenameOverride, SEAltText from {0} where {0}ID = {1}", EntityOrObjectName, sID);
					using(var dr = DB.GetRS(query, con))
					{
						if(dr.Read())
						{
							IFO = DB.RSField(dr, "ImageFilenameOverride");
							AltText = DB.RSField(dr, "SEAltText");
						}
					}
				}
				result = AppLogic.LookupImage(EntityOrObjectName, ID, IFO, "", DesiredSize, ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
				result = string.Format(
					@"<img id=""EntityPic{0}"" class=""actionelement"" src=""{1}"" alt=""{2}"" />",
					ID,
					result,
					AltText.Replace("\"", "&quot;"));
			}
			return result;
		}

		public virtual string LookupImage(String sEntityOrObjectName, string sID, String sImageFileNameOverride, String sSKU, String sImgSize)
		{
			InputValidator IV = new InputValidator("LookupImage");
			String ImageFileNameOverride = IV.ValidateString("ImageFileNameOverride", sImageFileNameOverride);
			String EntityOrObjectName = IV.ValidateString("EntityOrObjectName", sEntityOrObjectName);
			String SKU = IV.ValidateString("SKU", sSKU);
			String ImgSize = IV.ValidateString("ImgSize", sImgSize);
			int ID = IV.ValidateInt("ID", sID);
			String EONU = EntityOrObjectName.ToUpperInvariant();
			String seName = AppLogic.GetEntitySEName(ID, (AppLogic.EntityType)Enum.Parse(typeof(AppLogic.EntityType), EONU, true), ThisCustomer.LocaleSetting);

			String FN = ImageFileNameOverride;
			if(FN.Length == 0 && EONU == "PRODUCT" &&
				AppLogic.AppConfigBool("UseSKUForProductImageName"))
			{
				FN = SKU;
			}
			if(FN.Length == 0)
			{
				FN = ID.ToString();
			}
			String Image1 = String.Empty;
			String Image1URL = String.Empty;
			if((FN.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase) || FN.EndsWith(".gif", StringComparison.InvariantCultureIgnoreCase) || FN.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase)))
			{
				Image1 = AppLogic.GetImagePath(EntityOrObjectName, ImgSize, true) + FN;
				Image1URL = AppLogic.GetImagePath(EntityOrObjectName, ImgSize, false) + FN;
			}
			else
			{
				Image1 = AppLogic.GetImagePath(EntityOrObjectName, ImgSize, true) + FN + ".jpg";
				Image1URL = AppLogic.GetImagePath(EntityOrObjectName, ImgSize, false) + FN + ".jpg";
				if(!CommonLogic.FileExists(Image1))
				{
					Image1 = AppLogic.GetImagePath(EntityOrObjectName, ImgSize, true) + FN + ".gif";
					Image1URL = AppLogic.GetImagePath(EntityOrObjectName, ImgSize, false) + FN + ".gif";
				}
				if(!CommonLogic.FileExists(Image1))
				{
					Image1 = AppLogic.GetImagePath(EntityOrObjectName, ImgSize, true) + FN + ".png";
					Image1URL = AppLogic.GetImagePath(EntityOrObjectName, ImgSize, false) + FN + ".png";
				}
			}
			if(!CommonLogic.FileExists(Image1))
			{
				Image1 = String.Empty;
				Image1URL = String.Empty;
			}

			if(Image1URL.Length == 0 &&
				(ImgSize == "icon" || ImgSize == "medium"))
			{
				Image1URL = AppLogic.NoPictureImageURL(ImgSize == "icon", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
				if(Image1URL.Length != 0)
				{
					Image1 = CommonLogic.SafeMapPath(Image1URL);
				}
			}

			return "<img id=\"" + EONU + "Pic" + ID.ToString() + "\" name=\"" + CommonLogic.IIF(AppLogic.AppConfigBool("NameImagesBySEName") && !String.IsNullOrEmpty(seName), seName, EONU.ToString() + ID.ToString()) + "\" src=\"" + Image1URL + "\">";
		}

		public virtual string LookupProductImage(string sProductID, String sImageFileNameOverride, String sSKU, String sDesiredSize, String sIncludeATag)
		{

			InputValidator IV = new InputValidator("LookupProductImage");
			int ProductID = IV.ValidateInt("ProductID", sProductID);
			String DesiredSize = IV.ValidateString("DesiredSize", sDesiredSize);
			String ImageFileNameOverride = IV.ValidateString("ImageFileNameOverride", sImageFileNameOverride);
			String SKU = IV.ValidateString("SKU", sSKU);
			bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
			string result = String.Empty;
			string seName = AppLogic.GetProductSEName(ProductID, ThisCustomer.LocaleSetting);

			if(DesiredSize.Equals("ICON", StringComparison.InvariantCultureIgnoreCase))
			{
				result = AppLogic.LookupImage("Product", ProductID, ImageFileNameOverride, SKU, "icon", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
				result = string.Format(
					@"<img id=""ProductPic{0}"" name=""{1}"" class=""product-image icon-image img-responsive"" src=""{2}"" />",
					ProductID,
					AppLogic.AppConfigBool("NameImagesBySEName") && !String.IsNullOrEmpty(seName)
						? seName
						: string.Format("ProductPic{0}", ProductID),
					result);

				if(IncludeATag)
				{
					result = ProductLink(ProductID.ToString(), "", "TRUE", result);
				}
			}
			else
			{
				StringBuilder tmpS = new StringBuilder(4096);
				tmpS.Append(@"<div class=""image-wrap product-image-wrap"">");
				String ProductPicture = String.Empty;
				ProductPicture = AppLogic.LookupImage("Product", ProductID, ImageFileNameOverride, SKU, "medium", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
				String LargePic = AppLogic.LookupImage("Product", ProductID, ImageFileNameOverride, SKU, "large", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
				bool HasLargePic = !LargePic.Contains("nopicture");
				String LargePicForPopup = LargePic;

				var hasProductPicture = ProductPicture.IndexOf("nopicture") == -1;

				// setup multi-image gallery:
				ProductImageGallery ImgGal = null;
				var ImgGalCacheName = string.Format("ImgGal_{0}_{1}_{2}", ProductID, ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
				if(AppLogic.CachingOn)
				{
					ImgGal = (ProductImageGallery)HttpContext.Current.Cache.Get(ImgGalCacheName);
				}
				if(ImgGal == null)
				{
					ImgGal = new ProductImageGallery(ProductID, ThisCustomer.SkinID, ThisCustomer.LocaleSetting, SKU);
				}
				if(AppLogic.CachingOn)
				{
					HttpContext.Current.Cache.Insert(ImgGalCacheName, ImgGal, null, DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
				}
				tmpS.Append(ImgGal.ImgDHTML);

				if(HasLargePic && hasProductPicture)
				{
					Size size = CommonLogic.GetImagePixelSize(LargePic);
					tmpS.Append(Scripts.PopupImage(size));
				}

				tmpS.AppendFormat(@"<div id=""divProductPicZ{0}"" style=""display:none"">{1}", ProductID, Environment.NewLine);
				tmpS.AppendFormat("</div>{0}", Environment.NewLine);
				tmpS.AppendFormat(@"<div class=""medium-image-wrap"" id=""divProductPic{0}"">{1}", ProductID, Environment.NewLine);
				
				if(HasLargePic)
				{
					tmpS.AppendFormat(
						@"<img id=""ProductPic{0}"" name=""{1}"" class=""product-image img-responsive {2}""{3} alt=""{4}"" src=""{5}"" />",
						ProductID,
						AppLogic.AppConfigBool("NameImagesBySEName") && !String.IsNullOrEmpty(seName)
							? seName
							: string.Format("ProductPic{0}", ProductID),
						hasProductPicture
							? "medium-image medium-image-cursor"
							: "medium-image",
						hasProductPicture
							? string.Format(
								@" onClick=""{0}""",
								ImgGal.HasSomeLarge
									? string.Format("popuplarge_{0}()", sProductID)
									: string.Format("popupimg('{0}')", LargePicForPopup))
							: string.Empty,
                        "Click here to view larger image",
						ProductPicture);
				}
				else
				{
					tmpS.AppendFormat(
						@"<img id=""ProductPic{0}"" name=""{1}"" class=""product-image img-responsive {2}"" src=""{3}"" />",
						ProductID,
						AppLogic.AppConfigBool("NameImagesBySEName") && !String.IsNullOrEmpty(seName)
							? seName
							: string.Format("ProductPic{0}", ProductID),
						hasProductPicture
							? "medium-image medium-image-cursor"
							: "medium-image",
						ProductPicture);
				}
				tmpS.Append("</div>");
				tmpS.Append(@"<div class=""image-controls"">");
				if(ImgGal.ImgGalIcons.Length != 0)
				{
					tmpS.Append(@"<div class=""image-icons"">");
					tmpS.Append(ImgGal.ImgGalIcons);
					tmpS.Append("</div>");
				}
				else if(HasLargePic && hasProductPicture)
				{
					tmpS.AppendFormat(
						@"<div class=""view-larger-wrap""><a href=""javascript:void(0);"" onClick=""{0}"">{1}</a></div>",
						ImgGal.HasSomeLarge
							? string.Format("popuplarge_{0}()", sProductID)
							: string.Format("popupimg('{0}')", LargePicForPopup),
						"showproduct.aspx.43".StringResource());
				}
				tmpS.Append("</div>");
				tmpS.Append("</div>");
				result = tmpS.ToString();
			}
			return result;
		}

		public virtual string LookupEntityImage(String sID, String sEntityName, String sDesiredSize, String sIncludeATag)
		{
			InputValidator IV = new InputValidator("LookupEntityImage");
			int ID = IV.ValidateInt("ID", sID);
			String EntityName = IV.ValidateString("EntityName", sEntityName);
			String DesiredSize = IV.ValidateString("DesiredSize", sDesiredSize);
			string result = String.Empty;
			string seName = AppLogic.GetEntitySEName(ID, EntityName, ThisCustomer.LocaleSetting);

			if(EntityName.Equals("DOCUMENT", StringComparison.InvariantCultureIgnoreCase))
			{
				result = String.Empty; // not supported yet
			}
			else
			{
				// a category, section, or manufacturer, etc...
				result = AppLogic.LookupImage(EntityName, ID, DesiredSize, ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
				result = "<img id=\"EntityPic" + ID.ToString() + "\"  name=\"" + CommonLogic.IIF(AppLogic.AppConfigBool("NameImagesBySEName") && !String.IsNullOrEmpty(seName), seName, "EntityPic" + ID.ToString()) + "\" class=\"actionelement\" src=\"" + result + "\">";
			}
			return result;
		}
		//end deprecated image lookup methods

		public virtual string LookupImage(String sEntityOrObjectName, string sID, String sImageFileNameOverride, String sSKU, String sImgSize, string sAltText)
		{
			InputValidator IV = new InputValidator("LookupImage");
			String ImageFileNameOverride = IV.ValidateString("ImageFileNameOverride", sImageFileNameOverride);
			String EntityOrObjectName = IV.ValidateString("EntityOrObjectName", sEntityOrObjectName);
			String SKU = IV.ValidateString("SKU", sSKU);
			String ImgSize = IV.ValidateString("ImgSize", sImgSize);
			int ID = IV.ValidateInt("ID", sID);
			String EONU = EntityOrObjectName.ToUpperInvariant();
			String AltText = IV.ValidateString("AltText", sAltText);

			String FN = ImageFileNameOverride;
			if(FN.Length == 0 && EONU == "PRODUCT" &&
				AppLogic.AppConfigBool("UseSKUForProductImageName"))
			{
				FN = SKU;
			}
			if(FN.Length == 0)
			{
				FN = ID.ToString();
			}
			String Image1 = String.Empty;
			String Image1URL = String.Empty;
			if((FN.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase) || FN.EndsWith(".gif", StringComparison.InvariantCultureIgnoreCase) || FN.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase)))
			{
				Image1 = AppLogic.GetImagePath(EntityOrObjectName, ImgSize, true) + FN;
				Image1URL = AppLogic.GetImagePath(EntityOrObjectName, ImgSize, false) + FN;
			}
			else
			{
				Image1 = AppLogic.GetImagePath(EntityOrObjectName, ImgSize, true) + FN + ".jpg";
				Image1URL = AppLogic.GetImagePath(EntityOrObjectName, ImgSize, false) + FN + ".jpg";
				if(!CommonLogic.FileExists(Image1))
				{
					Image1 = AppLogic.GetImagePath(EntityOrObjectName, ImgSize, true) + FN + ".gif";
					Image1URL = AppLogic.GetImagePath(EntityOrObjectName, ImgSize, false) + FN + ".gif";
				}
				if(!CommonLogic.FileExists(Image1))
				{
					Image1 = AppLogic.GetImagePath(EntityOrObjectName, ImgSize, true) + FN + ".png";
					Image1URL = AppLogic.GetImagePath(EntityOrObjectName, ImgSize, false) + FN + ".png";
				}
			}
			if(!CommonLogic.FileExists(Image1))
			{
				Image1 = String.Empty;
				Image1URL = String.Empty;
			}

			if(Image1URL.Length == 0 &&
				(ImgSize == "icon" || ImgSize == "medium"))
			{
				Image1URL = AppLogic.NoPictureImageURL(ImgSize == "icon", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
				if(Image1URL.Length != 0)
				{
					Image1 = CommonLogic.SafeMapPath(Image1URL);
				}
			}

			return "<img id=\"" + EONU + "Pic" + ID.ToString() + "\" name=\"" + EONU + "Pic" + ID.ToString() + "\" src=\"" + Image1URL + "\" alt=" + AltText.Replace("\"", "&quot;") + "\" />";
		}

		public virtual string LookupProductImage(string sProductID, string sImageFileNameOverride, string sSKU, string sDesiredSize, string sIncludeATag, string sAltText)
		{
			var validator = new InputValidator("LookupProductImage");
			var productId = validator.ValidateInt("ProductID", sProductID);
			var desiredSize = validator.ValidateString("DesiredSize", sDesiredSize);
			var imageFilenameOverride = validator.ValidateString("ImageFileNameOverride", sImageFileNameOverride);
			var sku = validator.ValidateString("SKU", sSKU);
			var includeATag = validator.ValidateBool("IncludeATag", sIncludeATag);
			var altText = validator.ValidateString("AltText", sAltText);
			var result = string.Empty;
			var seName = AppLogic.GetProductSEName(productId, ThisCustomer.LocaleSetting);

			if(desiredSize.Equals("ICON", StringComparison.InvariantCultureIgnoreCase))
			{
				result = AppLogic.LookupImage("Product", productId, imageFilenameOverride, sku, "icon", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
				result = string.Format(
					@"<img id=""ProductPic{0}"" name=""{1}"" class=""product-image icon-image img-responsive"" src=""{2}"" alt=""{3}"" />",
					productId,
					AppLogic.AppConfigBool("NameImagesBySEName") && !string.IsNullOrEmpty(seName)
						? seName
						: string.Format("ProductPic{0}", productId),
					result,
					altText.Replace("\"", "&quot;"));

				if(includeATag)
					result = ProductLink(productId.ToString(), "", "TRUE", result);
			}
			else
			{
				var resultBuilder = new StringBuilder(4096);
				resultBuilder.Append(@"<div class=""image-wrap product-image-wrap"">");

				var mediumPicture =  AppLogic.LookupImage("Product", productId, imageFilenameOverride, sku, "medium", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
				var largePicture = AppLogic.LookupImage("Product", productId, imageFilenameOverride, sku, "large", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);

				var hasMediumPicture = !mediumPicture.Contains("nopicture");
				var hasLargePicture = !largePicture.Contains("nopicture");

				// setup multi-image gallery:
				ProductImageGallery imageGallery = null;
				var imageGalleryCacheName = string.Format("ImgGal_{0}_{1}_{2}", productId, ThisCustomer.SkinID, ThisCustomer.LocaleSetting);

				if(AppLogic.CachingOn)
					imageGallery = (ProductImageGallery)HttpContext.Current.Cache.Get(imageGalleryCacheName);

				if(imageGallery == null)
					imageGallery = new ProductImageGallery(productId, ThisCustomer.SkinID, ThisCustomer.LocaleSetting, sku);

				if(AppLogic.CachingOn)
					HttpContext.Current.Cache.Insert(imageGalleryCacheName, imageGallery, null, DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);

				resultBuilder.Append(imageGallery.ImgDHTML);

				if(hasLargePicture)
					resultBuilder.Append(Scripts.PopupImage(CommonLogic.GetImagePixelSize(largePicture)));

				resultBuilder.AppendFormat(@"<div id=""divProductPicZ{0}"" style=""display:none"">{1}", productId, Environment.NewLine);
				resultBuilder.AppendFormat("</div>{0}", Environment.NewLine);
				resultBuilder.AppendFormat(@"<div class=""medium-image-wrap"" id=""divProductPic{0}"">{1}", productId, Environment.NewLine);

				if(hasLargePicture)
				{
					resultBuilder.AppendFormat(
						@"<img id=""ProductPic{0}"" name=""{1}"" class=""product-image img-responsive {2}""{3} title=""{4}"" src=""{5}"" alt=""{6}"" />",
						productId,
						AppLogic.AppConfigBool("NameImagesBySEName") && !string.IsNullOrEmpty(seName)
							? seName
							: string.Format("ProductPic{0}", productId),
						hasMediumPicture
							? "medium-image medium-image-cursor"
							: "medium-image",
						hasMediumPicture
							? string.Format(
							@" onClick=""{0}""",
						imageGallery.HasSomeLarge
								? string.Format("popuplarge_{0}()", sProductID)
								: string.Format("popupimg('{0}')", largePicture))
							: string.Empty,
						hasMediumPicture
							? "Click here to view larger image"
                            : string.Empty,
						mediumPicture,
						altText.Replace("\"", "&quot;"));
					
					resultBuilder.AppendFormat(@"<input type=""hidden"" id=""popupImageURL"" value=""{0}"" />", Url.Encode(largePicture));
				}
				else
				{
					resultBuilder.AppendFormat(
						@"<img id=""ProductPic{0}"" name=""{1}"" class=""product-image img-responsive medium-image"" src=""{2}"" alt=""{3}"" />",
						productId,
						AppLogic.AppConfigBool("NameImagesBySEName") && !String.IsNullOrEmpty(seName)
							? seName
							: string.Format("ProductPic{0}", productId),
						mediumPicture,
						altText.Replace("\"", "&quot;"));

					if(hasMediumPicture)
					{
						resultBuilder.AppendFormat(@"<input type=""hidden"" id=""popupImageURL"" value=""{0}"" />", Url.Encode(mediumPicture));
					}
				}

				resultBuilder.AppendFormat("</div>{0}", Environment.NewLine);
				resultBuilder.Append(@"<div class=""image-controls"">");

				if(!string.IsNullOrEmpty(imageGallery.ImgGalIcons))
				{
					resultBuilder.Append(@"<div class=""image-icons"">");
					resultBuilder.Append(imageGallery.ImgGalIcons);
					resultBuilder.Append("</div>");
				}

				if(hasLargePicture)
				{
					resultBuilder.AppendFormat(
						@"<div class=""pop-large-wrap""><a href=""javascript:void(0);"" class=""pop-large-link"" onClick=""{0}"">{1}</a></div>",
						imageGallery.HasSomeLarge
							? string.Format("popuplarge_{0}()", sProductID)
							: string.Format("popupimg('{0}')", largePicture),
						"showproduct.aspx.43".StringResource());
				}

				resultBuilder.Append("</div>");
				resultBuilder.Append("</div>");
				result = resultBuilder.ToString();
			}
			return result;
		}

		public virtual string LookupVariantImage(string sProductID, string sVariantID, string sImageFileNameOverride, string sSKU, string sDesiredSize, string sIncludeATag, string sAltText)
		{
			var validator = new InputValidator("LookupVariantImage");
			var variantId = validator.ValidateInt("VariantID", sVariantID);
			var size = validator.ValidateString("DesiredSize", sDesiredSize);
			var imageFilenameOverride = validator.ValidateString("ImageFileNameOverride", sImageFileNameOverride);
			var sku = validator.ValidateString("SKU", sSKU);
			var altText = validator.ValidateString("AltText", sAltText);
			var result = string.Empty;
			var seName = AppLogic.GetVariantSEName(variantId, ThisCustomer.LocaleSetting);

			if(size.Equals("ICON", StringComparison.InvariantCultureIgnoreCase))
			{
				result = string.Format(
					@"<img id=""ProductPic{0}"" name=""{1}"" src=""{2}"" alt=""{3}"" />",
					variantId,
					AppLogic.AppConfigBool("NameImagesBySEName") && !string.IsNullOrEmpty(seName)
						? seName
						: string.Format("ProductPic{0}", variantId),
					AppLogic.LookupImage("VARIANT", variantId, imageFilenameOverride, sku, "icon", ThisCustomer.SkinID, ThisCustomer.LocaleSetting),
					altText.Replace("\"", "&quot;"));
			}
			else
			{
				var returnValue = new StringBuilder(4096);
				var picture = AppLogic.LookupImage("VARIANT", variantId, imageFilenameOverride, sku, "medium", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
				var largePicture = AppLogic.LookupImage("VARIANT", variantId, imageFilenameOverride, sku, "large", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
				var hasLargePicture = !largePicture.Contains("nopicture");
				var largePictureForPopup = largePicture;
				var hasProductPicture = !picture.Contains("nopicture");
				var scriptPopup = string.Empty;
				var urlHelper = DependencyResolver.Current.GetService<UrlHelper>();

				returnValue.Append(@"<div align=""center"">");

				if(hasLargePicture && hasProductPicture)
				{
					var s = CommonLogic.GetImagePixelSize(largePicture);
					scriptPopup = string.Format("window.open('{0}','LargerImage{1}','toolbar=no,location=no,directories=no,status=no, menubar=no,scrollbars={2}, resizable={2},copyhistory=no,width={3},height={4},left=0,top=0');",
						urlHelper.Action(ActionNames.PopUp, ControllerNames.Image, new RouteValueDictionary { { RouteDataKeys.ImagePath, largePictureForPopup } }),
						CommonLogic.GetRandomNumber(1, 100000),
						AppLogic.AppConfigBool("ResizableLargeImagePopup") ? "yes" : "no",
						s.Width,
						s.Height);
				}

				if(hasLargePicture && hasProductPicture)
				{
					returnValue.AppendFormat(
						@"<img id=""ProductPic{0}"" name=""{1}""{2} title=""{3}""{4} src=""{5}"" alt=""{6}"" />",
						variantId,
						AppLogic.AppConfigBool("NameImagesBySEName") && !String.IsNullOrEmpty(seName)
							? seName
							: string.Format("ProductPic{0}", variantId),
						hasProductPicture
							? @" class=""actionelement"""
							: string.Empty,
						"showproduct.aspx.19".StringResource(),
						hasProductPicture
							? string.Format(@"onClick=""{0}""", scriptPopup)
							: string.Empty,
						picture,
						altText.Replace("\"", "&quot;"));
				}
				else
				{
					returnValue.AppendFormat(
						@"<img id=""ProductPic{0}"" name=""{1}"" src=""{2}"" alt=""{3}"" />",
						variantId,
						AppLogic.AppConfigBool("NameImagesBySEName") && !String.IsNullOrEmpty(seName)
							? seName
							: string.Format("ProductPic{0}", variantId),
						picture,
						altText.Replace("\"", "&quot;"));
				}
				if(hasLargePicture && hasProductPicture)
				{
					returnValue.AppendFormat(@"<img src=""{0}"" width=""1"" height=""4"" />", AppLogic.LocateImageURL("images/spacer.gif"));
					returnValue.AppendFormat(
						@"<div class=""pop-large-wrap""><a href=""javascript:void(0);"" class=""pop-large-link"" onClick=""{0}""><img src=""{1}"" title=""{2}""></a></div>",
						scriptPopup,
						AppLogic.LocateImageURL(string.Format("Skins/{0}/images/showlarger.gif", SkinProvider.GetSkinNameById(ThisCustomer.SkinID))),
						"showproduct.aspx.19".StringResource());
				}
				returnValue.Append("</div>");
				result = returnValue.ToString();
			}
			return result;
		}

		public virtual string LookupEntityImage(String sID, String sEntityName, String sDesiredSize, String sIncludeATag, string sAltText)
		{
			InputValidator IV = new InputValidator("LookupEntityImage");
			int ID = IV.ValidateInt("ID", sID);
			String EntityName = IV.ValidateString("EntityName", sEntityName);
			String DesiredSize = IV.ValidateString("DesiredSize", sDesiredSize);
			String AltText = IV.ValidateString("AltText", sAltText);
			string result = String.Empty;
			string seName = AppLogic.GetEntitySEName(ID, EntityName, ThisCustomer.LocaleSetting);

			if(EntityName.Equals("DOCUMENT", StringComparison.InvariantCultureIgnoreCase))
			{
				result = String.Empty; // not supported yet
			}
			else
			{
				// a category, section, or manufacturer, etc...
				result = AppLogic.LookupImage(EntityName, ID, DesiredSize, ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
				result = "<img id=\"EntityPic" + ID.ToString() + "\" name=\"" + CommonLogic.IIF(AppLogic.AppConfigBool("NameImagesBySEName") && !String.IsNullOrEmpty(seName), seName, "EntityPic" + ID.ToString()) + "\" class=\"actionelement\" src=\"" + result + "\" alt=\"" + AltText.Replace("\"", "&quot;") + "\" />";
			}
			return result;
		}

		public virtual String GetStoreHTTPLocation(String sTryToUseSSL)
		{
			InputValidator IV = new InputValidator("GetStoreHTTPLocation");
			bool TryToUseSSL = IV.ValidateBool("TryToUseSSL", sTryToUseSSL);
			return AppLogic.GetStoreHTTPLocation(TryToUseSSL);
		}

		public virtual string ImageUrl(String sID, String sEntityOrObjectName, String sDesiredSize, String sFullUrl)
		{
			InputValidator IV = new InputValidator("ImageUrl");
			int ID = IV.ValidateInt("ID", sID);
			String EntityOrObjectName = IV.ValidateString("EntityOrObjectName", sEntityOrObjectName);
			bool FullUrl = IV.ValidateBool("FullUrl", sFullUrl);
			String DesiredSize = IV.ValidateString("DesiredSize", sDesiredSize);
			string result = String.Empty;
			string ImgPath = String.Empty;
			string sURL = CommonLogic.IIF(FullUrl, AppLogic.GetStoreHTTPLocation(false), "");
			sURL = sURL.Replace(AppLogic.AdminDir() + "/", "");
			if(sURL.EndsWith("/"))
				sURL = sURL.Substring(0, sURL.Length - 1);
			ImgPath = AppLogic.LookupImage(EntityOrObjectName, ID, DesiredSize, ThisCustomer.SkinID, ThisCustomer.LocaleSetting).Replace("..", "");
            if (AppLogic.IsCDNImagePath(ImgPath))
                result = ImgPath;
            else
                result = sURL + CommonLogic.IIF(ImgPath.StartsWith("/"), "", "/") + ImgPath;

            return result;
		}

		public virtual string ProductImageUrl(String sProductID, String sImageFileNameOverride, String sSKU, String sDesiredSize, String sFullUrl)
		{
			InputValidator IV = new InputValidator("ProductImageUrl");
			int ProductID = IV.ValidateInt("ProductID", sProductID);
			String ImageFileNameOverride = IV.ValidateString("ImageFileNameOverride", sImageFileNameOverride);
			bool FullUrl = IV.ValidateBool("FullUrl", sFullUrl);
			String DesiredSize = IV.ValidateString("DesiredSize", sDesiredSize);
			String SKU = IV.ValidateString("SKU", sSKU);
			string result = String.Empty;
			string ImgPath = String.Empty;
			string sURL = CommonLogic.IIF(FullUrl, AppLogic.GetStoreHTTPLocation(false), "");
			sURL = sURL.Replace(AppLogic.AdminDir() + "/", "");

			if(!sURL.EndsWith("/"))
			{
				sURL += "/";
			}
            ImgPath = AppLogic.LookupImage("Product", ProductID, ImageFileNameOverride, SKU, DesiredSize, ThisCustomer.SkinID, ThisCustomer.LocaleSetting).Replace("..", "");
            if (AppLogic.IsCDNImagePath(ImgPath))
                result = ImgPath;
            else
            {

                if (FullUrl && ImgPath.StartsWithIgnoreCase(HttpContext.Current.Request.ApplicationPath))
                {
                    ImgPath = ImgPath.Substring(HttpContext.Current.Request.ApplicationPath.Length);
                }

                ImgPath = ImgPath.TrimStart('/');
                result = sURL + ImgPath;
            }
            return result;
        }

        public bool Owns(String sProductID)
        {
            InputValidator IV = new InputValidator("Owns");
            int ProductID = IV.ValidateInt("ProductID", sProductID);
            return AppLogic.Owns(ProductID, ThisCustomer.CustomerID);
        }

        public bool Owns(String sProductID, String sCustomerID)
        {
            InputValidator IV = new InputValidator("Owns");
            int ProductID = IV.ValidateInt("ProductID", sProductID);
            int CustomerID = IV.ValidateInt("CustomerID", sCustomerID);
            return AppLogic.Owns(ProductID, CustomerID);
        }

		[Obsolete("Deprecated (10.0)")]
		public virtual string ProductNavLinks(string sProductID, string sCategoryID, string sSectionID)
        {
			return string.Empty;
        }

		[Obsolete("Deprecated (10.0)")]
		public virtual string ProductNavLinks(string sProductID, string sCategoryID, string sSectionID, string sUseGraphics)
        {
			return string.Empty;
		}

		[Obsolete("Deprecated (10.0)")]
		public virtual string ProductNavLinks(string sProductID, string sCategoryID, string sSectionID, string sUseGraphics, string sIncludeUpLink)
        {
			return string.Empty;
		}

		[Obsolete("Deprecated (10.0)")]
		public virtual string ProductNavLinks(string sProductID, string sEntityID, string sEntityName, string sEntitySEName, string sSortByLooks, string sUseGraphics, string sIncludeUpLink)
        {
			return string.Empty;
		}

		public virtual string EmailProductToFriend(string sProductId, string sCategoryId)
		{
			return EmailProductToFriend(sProductId);
		}

		public virtual string EmailProductToFriend(string sProductId)
		{
			var IV = new InputValidator("EmailProductToFriend");
			var productId = IV.ValidateInt("ProductID", sProductId);

			if(!AppLogic.AppConfigBool("ShowEMailProductToFriend"))
				return string.Empty;

			return string.Format(@"
				<div class='email-a-friend-wrap'>
					<a href='{0}' class='email-a-friend-link'>
						{2}
					</a>
				</div>",
				DependencyResolver.Current.GetService<UrlHelper>()
					.Action(
						actionName: ActionNames.EmailProduct,
						controllerName: ControllerNames.Product,
						routeValues: new
						{
							@id = productId
						}),
				productId,
                "Email this product to a friend");
		}

		public virtual string ProductRatings(String sProductID, String sCategoryID, String sSectionID, String sManufacturerID, String sIncludeBRBefore)
		{
			bool showRating = AppLogic.AppConfigBool("RatingsEnabled");
			if(!showRating)
			{
				return string.Empty;
			}

			return ProductRatings(sProductID, sCategoryID, sSectionID, sManufacturerID, sIncludeBRBefore, true.ToString());
		}

		public virtual string ProductRatings(String sProductID, String sCategoryID, String sSectionID, String sManufacturerID, String sIncludeBRBefore, String sEncloseInTab)
		{
			InputValidator IV = new InputValidator("ProductRatings");
			int ProductID = IV.ValidateInt("ProductID", sProductID);
			int CategoryID = IV.ValidateInt("CategoryID", sCategoryID);
			int SectionID = IV.ValidateInt("SectionID", sSectionID);
			int ManufacturerID = IV.ValidateInt("ManufacturerID", sManufacturerID);
			bool IncludeBRBefore = IV.ValidateBool("IncludeBRBefore", sIncludeBRBefore);
			bool encloseInTab = IV.ValidateBool("EncloseInTab", sEncloseInTab);
			string result = String.Empty;

			String tmpS = Ratings.Display(ThisCustomer, ProductID, CategoryID, SectionID, ManufacturerID, ThisCustomer.SkinID, encloseInTab);
			if(IncludeBRBefore && tmpS.Length != 0 && encloseInTab)
			{
				result = "<div class=\"clear\"></div><hr size=\"1\"/>" + tmpS;
			}
			else
			{
				result = tmpS;
			}
			return result;
		}

		public virtual string ProductEntityList(String sProductID, string sEntityName)
		{
			InputValidator IV = new InputValidator("ProductEntityList");
			int ProductID = IV.ValidateInt("ProductID", sProductID);
			String EntityName = IV.ValidateString("EntityName", sEntityName);
			StringBuilder results = new StringBuilder("");

			EntityHelper eh = AppLogic.LookupHelper(EntityName.ToLowerInvariant(), 0);
			String Entities = eh.GetObjectEntities(ProductID, false);
			if(Entities.Length != 0)
			{
				String[] EntityIDs = Entities.Split(',');
				bool firstEntity = true;
				foreach(String s in EntityIDs)
				{
					if(!firstEntity)
					{
						results.Append(", ");
					}

					string str = eh.GetEntityName(Localization.ParseUSInt(s), ThisCustomer.LocaleSetting).Trim();
					if(!str.Equals(string.Empty))
					{
						results.Append("<a href=\"" + Url.BuildEntityLink(EntityName, Localization.ParseUSInt(s), string.Empty) + "\">" + str + "</a>");
						firstEntity = false;
					}
					else
					{
						firstEntity = true;
					}

				}
			}
			else
			{
				results.Append("");
			}
			return results.ToString();
		}

		// uses the DisplaySpec if provided for output display
		// else uses DisplayLocale if provided
		// input string expected in SQL Locale format
		public virtual string FormatCurrency(string sCurrencyValue)
		{
			InputValidator IV = new InputValidator("FormatCurrency");
			String CurrencyValue = IV.ValidateString("CurrencyValue", sCurrencyValue);
			return FormatCurrencyHelper(Localization.ParseDBDecimal(CurrencyValue));
		}

		// uses the DisplaySpec if provided for output display
		// else uses DisplayLocale if provided
		// input string expected in SQL Locale format
		public virtual string FormatCurrency(string sCurrencyValue, String sTargetCurrency)
		{
			InputValidator IV = new InputValidator("FormatCurrency");
			String CurrencyValue = IV.ValidateString("CurrencyValue", sCurrencyValue);
			String TargetCurrency = IV.ValidateString("TargetCurrency", sTargetCurrency);
			return FormatCurrencyHelper(Localization.ParseDBDecimal(CurrencyValue), TargetCurrency);
		}

		// internal helper function only!
		protected virtual String FormatCurrencyHelper(Decimal Amt)
		{
			return Localization.CurrencyStringForDisplayWithExchangeRate(Amt, ThisCustomer.CurrencySetting);
		}

		// internal helper function only!
		protected virtual String FormatCurrencyHelper(Decimal Amt, String TargetCurrency)
		{
			if(TargetCurrency == null ||
				TargetCurrency.Length == 0)
			{
				TargetCurrency = ThisCustomer.CurrencySetting;
			}
			return Localization.CurrencyStringForDisplayWithExchangeRate(Amt, TargetCurrency);
		}

		[Obsolete("deprecated (10.0) no longer used")]
		public virtual string GetSpecialsBoxExpandedRandom(String sCategoryID, String sShowPics, String sIncludeFrame, String sTeaser)
		{
			return string.Empty;
		}

		[Obsolete("deprecated (10.0) no longer used")]
		public virtual string GetSpecialsBoxExpanded(String sCategoryID, String sShowNum, String sShowPics, String sIncludeFrame, String sTeaser)
		{
			return string.Empty;
		}

		public virtual string GetNewsBoxExpanded(String sShowCopy, String sShowNum, String sIncludeFrame, String sTeaser)
		{
			InputValidator IV = new InputValidator("GetNewsBoxExpanded");
			bool ShowCopy = IV.ValidateBool("ShowCopy", sShowCopy);
			int ShowNum = IV.ValidateInt("ShowNum", sShowNum);
			bool IncludeFrame = IV.ValidateBool("IncludeFrame", sIncludeFrame);
			String Teaser = IV.ValidateString("Teaser", sTeaser);
			string result = String.Empty;

			result = AppLogic.GetNewsBoxExpanded(AppLogic.GetCurrentPageType() != PageTypes.News, ShowCopy, ShowNum, IncludeFrame, AppLogic.CachingOn, Teaser, ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
			return result;
		}

		[Obsolete("deprecated (7.0) in favor of the HtmlDecode method")]
		public virtual string Decode(string sHtmlContent)
		{
			InputValidator IV = new InputValidator("Decode");
			String HtmlContent = IV.ValidateString("HtmlContent", sHtmlContent);
			return HttpContext.Current.Server.HtmlDecode(HtmlContent);
		}

		public virtual string HtmlDecode(string sHtmlContent)
		{
			InputValidator IV = new InputValidator("Decode");
			String HtmlContent = IV.ValidateString("HtmlContent", sHtmlContent);
			return HttpContext.Current.Server.HtmlDecode(HtmlContent);
		}

		public virtual string HtmlEncode(string sHtmlContent)
		{
			InputValidator IV = new InputValidator("Decode");
			String HtmlContent = IV.ValidateString("HtmlContent", sHtmlContent);
			return HttpContext.Current.Server.HtmlEncode(HtmlContent);
		}

		public virtual string ShowUpsellProducts(String sProductID)
		{
			InputValidator IV = new InputValidator("ShowUpsellProducts");
			int ProductID = IV.ValidateInt("ProductID", sProductID);
			string result = String.Empty;
			String UpsellXmlPackage = AppLogic.AppConfig("XmlPackage.UpsellProductPackage");
			if(UpsellXmlPackage.Length != 0)
			{
                result = AppLogic.RunXmlPackage(UpsellXmlPackage, new Parser(), ThisCustomer, ThisCustomer.SkinID, String.Empty, "ProductID=" + ProductID.ToString(), true, true);
			}
			else
			{
				try
				{
					// people type weird things in the upsell box field, so ignore any "issues"...no other good solution at the moment:                    
					result = AppLogic.GetUpsellProductsBoxExpanded(ProductID, 100, true, String.Empty, AppLogic.AppConfig("RelatedProductsFormat").Equals("GRID", StringComparison.InvariantCultureIgnoreCase), ThisCustomer.SkinID, ThisCustomer);
				}
				catch
				{
					result = string.Empty;
				}
			}
			return result;
		}

		public virtual string Decrypt(string sEncryptedData)
		{
			InputValidator IV = new InputValidator("Decrypt");
			string EncryptedData = IV.ValidateString("EncryptedData", sEncryptedData);
			if(EncryptedData.Length == 0)
			{
				return "";
			}
			return Security.UnmungeString(EncryptedData);
		}

		public virtual string EncryptString(string sString2Encrypt)
		{
			InputValidator IV = new InputValidator("EncryptString");
			string String2Encrypt = IV.ValidateString("String2Encrypt", sString2Encrypt);
			return Security.MungeString(String2Encrypt);
		}

		public virtual string Decrypt(string sEncryptedData, String sSaltKey)
		{
			InputValidator IV = new InputValidator("Decrypt");
			string EncryptedData = IV.ValidateString("EncryptedData", sEncryptedData);
			string SaltKey = IV.ValidateString("SaltKey", sSaltKey);
			if(EncryptedData.Length == 0)
			{
				return "";
			}
			return Security.UnmungeString(EncryptedData, SaltKey);
		}

		public virtual string DecryptCCNumber(string sCardNumberCrypt, string sOrderNumber)
		{
			InputValidator IV = new InputValidator("DecryptCCNumber");
			String CardNumberCrypt = IV.ValidateString("CardNumberCrypt", sCardNumberCrypt);
			int OrderNumber = IV.ValidateInt("OrderNumber", sOrderNumber);
			String CardNumber = Security.UnmungeString(CardNumberCrypt, Order.StaticGetSaltKey(OrderNumber));
			if(CardNumber.StartsWith(Security.ro_DecryptFailedPrefix, StringComparison.InvariantCultureIgnoreCase))
			{
				CardNumber = CardNumberCrypt;
			}
			return CardNumber;
		}

		public virtual string EncryptString(string sString2Encrypt, string sSaltKey)
		{
			InputValidator IV = new InputValidator("EncryptString");
			string String2Encrypt = IV.ValidateString("String2Encrypt", sString2Encrypt);
			string SaltKey = IV.ValidateString("SaltKey", sSaltKey);
			return Security.MungeString(String2Encrypt, SaltKey);
		}

		public virtual string XmlPackage(string sPackageName)
		{
			var inputValidatator = new InputValidator("XmlPackage");
			var packageName = inputValidatator.ValidateString("PackageName", sPackageName);

			if(string.IsNullOrEmpty(packageName))
				throw new ArgumentException("Please specify an XmlPackage name");

			var package = new XmlPackage(
				packageName: packageName,
				customer: ThisCustomer,
				userQuery: string.Empty,
				additionalRuntimeParms: string.Empty,
				htmlHelper: HtmlHelper);

			return AppLogic.RunXmlPackage(
				xmlPackage: package,
				UseParser: new Parser(),
				ThisCustomer: ThisCustomer,
				SkinID: ThisCustomer.SkinID,
				ReplaceTokens: true,
				WriteExceptionMessage: true);
		}

		/// <summary>
		/// XmlPackage overload which allows a package to be loaded with specified runtime parameters.
		/// </summary>
		/// <param name="PackageName">The name of the package to load. The package name must include the xml.config extension.</param>
		/// <param name="AdditionalRuntimeParms">Querystring containing additional parameters that will be passed to the package as runtime values.</param>
		/// <returns>results of executing the specified package</returns>
		public virtual string XmlPackage(string sPackageName, string sAdditionalRuntimeParms)
		{
			var inputValidatator = new InputValidator("XmlPackage");
			var packageName = inputValidatator.ValidateString("PackageName", sPackageName);
			var additionalRuntimeParms = inputValidatator.ValidateString("AdditionalRuntimeParms", sAdditionalRuntimeParms);

			if(string.IsNullOrEmpty(packageName))
				throw new ArgumentException("Please specify an XmlPackage name");

			var package = new XmlPackage(
				packageName: packageName,
				customer: ThisCustomer,
				userQuery: string.Empty,
				additionalRuntimeParms: additionalRuntimeParms,
				htmlHelper: HtmlHelper);

			return AppLogic.RunXmlPackage(
				xmlPackage: package,
				UseParser: new Parser(),
				ThisCustomer: ThisCustomer,
				SkinID: ThisCustomer.SkinID,
				ReplaceTokens: true,
				WriteExceptionMessage: true);
		}

		public virtual string ImageGallery(String sProductID, string sImageFileNameOverride, string sSKU)
		{
			InputValidator IV = new InputValidator("ImageGallery");
			int ProductID = IV.ValidateInt("ProductID", sProductID);
			string SKU = IV.ValidateString("SKU", sSKU);
			StringBuilder results = new StringBuilder("");

			// setup multi-image gallery:
			ProductImageGallery ImgGal = null;
			String ImgGalCacheName = "ImgGal_" + ProductID.ToString() + "_" + ThisCustomer.SkinID.ToString() + "_" + ThisCustomer.LocaleSetting;
			if(AppLogic.CachingOn)
			{
				ImgGal = (ProductImageGallery)HttpContext.Current.Cache.Get(ImgGalCacheName);
			}
			if(ImgGal == null)
			{
				ImgGal = new ProductImageGallery(ProductID, ThisCustomer.SkinID, ThisCustomer.LocaleSetting, SKU);
			}
			if(AppLogic.CachingOn)
			{
				HttpContext.Current.Cache.Insert(ImgGalCacheName, ImgGal, null, DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
			}
			results.Append(ImgGal.ImgDHTML);

			if(ImgGal.ImgGalIcons.Length != 0)
			{
				results.Append(ImgGal.ImgGalIcons);
			}
			return results.ToString();
		}

		public virtual string ShowQuantityDiscountTable(String sProductID)
		{
			InputValidator IV = new InputValidator("ShowQuantityDiscountTable");
			int ProductID = IV.ValidateInt("ProductID", sProductID);
			StringBuilder results = new StringBuilder("");
			bool CustomerLevelAllowsQuantityDiscounts = QuantityDiscount.CustomerLevelAllowsQuantityDiscounts(ThisCustomer.CustomerLevelID);
			int ActiveDIDID = QuantityDiscount.LookupProductQuantityDiscountID(ProductID);
			bool ActiveDID = (ActiveDIDID != 0);
			if(!CustomerLevelAllowsQuantityDiscounts)
			{
				ActiveDID = false;
			}
			if(ActiveDID)
			{
				results.Append("<div class=\"quantity-discount-wrap\">");
				bool ShowInLine = AppLogic.AppConfigBool("ShowQuantityDiscountTablesInline");
				results.Append("<span class=\"quantity-discount-header\">This product qualifies for quantity discount pricing.</span>");

				string Key = (Guid.NewGuid()).ToString().Substring(0, 4);
				string discountTableHTML = QuantityDiscount.GetQuantityDiscountDisplayTable(ActiveDIDID, ThisCustomer.SkinID);
				discountTableHTML = discountTableHTML.Replace("\r\n", "");

				if(!ShowInLine)
				{
					results.Append(string.Format("(<a id=\"lnkQuantityDiscount_{0}", Key) + "\" href=\"javascript:void(0);\" class=\"quantity-discount-link\" >what's this?</a>)");
					results.Append("<script type=\"text/javascript\" language=\"Javascript\" src=\"" + AppLogic.ResolveUrl("~/scripts/tooltip.js") + "\" ></script>\n");
					results.Append("<script type=\"text/javascript\" language=\"Javascript\">\n");
					results.AppendFormat("    $window_addLoad(function(){{ new ToolTip('lnkQuantityDiscount_{0}', 'discount-table-tooltip', '{1}'); }});\n", Key, discountTableHTML);
					results.Append("</script>\n");
				}
				else
				{
					results.Append("<div class=\"quantity-discount-table-wrap\">");
					results.Append(QuantityDiscount.GetQuantityDiscountDisplayTable(ActiveDIDID, ThisCustomer.SkinID));
					results.Append("</div>");
				}
				results.Append("</div>");
			}
			return results.ToString();
		}

		public virtual string GetProductDiscountID(String sProductID)
		{
			InputValidator IV = new InputValidator("GetProductDiscountID");
			int ProductID = IV.ValidateInt("ProductID", sProductID);
			return QuantityDiscount.LookupProductQuantityDiscountID(ProductID).ToString();
		}

		/// <summary>
		/// Display 'out of stock' or 'in stock' message
		/// </summary>
		/// <param name="productId">The product id</param>
		/// <param name="pages">Entity or Product</param>
		/// <returns></returns>
		public string DisplayProductStockHint(string productId, string pages)
		{
			return DisplayProductStockHint(productId, AppLogic.OUT_OF_STOCK_ALL_VARIANTS.ToString(), pages);
		}

		/// <summary>
		/// Display 'out of stock' or 'in stock' message 
		/// </summary>
		/// <param name="sProductID">The product id</param>
		/// <param name="sVariantID">The variant id</param>
		/// <param name="page">Entity or Product</param>
		/// <returns>HTML message</returns>
		public virtual string DisplayProductStockHint(string sProductID, string sVariantID, string page)
		{
			return DisplayProductStockHint(sProductID, sVariantID, page, "stock-hint");
		}

		/// <summary>
		/// Display 'out of stock' or 'in stock' message 
		/// </summary>
		/// <param name="sProductID">The product id</param>
		/// <param name="sVariantID">The variant id</param>
		/// <param name="page">Entity or Product</param>
		/// <param name="className">css class name</param>
		/// <returns>HTML message</returns>
		public virtual string DisplayProductStockHint(string sProductID, string sVariantID, string page, string className)
		{
			return DisplayProductStockHint(sProductID, sVariantID, page, className, "div");
		}

		/// <summary>
		/// Display 'out of stock' or 'in stock' message 
		/// </summary>
		/// <param name="sProductID">The product id</param>
		/// <param name="sVariantID">The variant id</param>
		/// <param name="className">css class name</param>
		/// <param name="renderAsElement">Span or div</param>
		/// <returns>HTML stock message</returns>
		public virtual string DisplayProductStockHint(string sProductID, string sVariantID, string page, string className, string renderAsElement)
		{
			return Product.DisplayStockHint(sProductID, sVariantID, page, className, renderAsElement);
		}

		public virtual string ShowInventoryTable(String sProductID)
		{
			InputValidator IV = new InputValidator("ShowInventoryTable");
			int ProductID = IV.ValidateInt("ProductID", sProductID);
			StringBuilder results = new StringBuilder("");
			if(AppLogic.AppConfigBool("ShowInventoryTable"))
			{
				results.Append(AppLogic.GetInventoryTable(ProductID, AppLogic.GetDefaultProductVariant(ProductID), ThisCustomer.IsAdminUser, ThisCustomer.SkinID, true, false));
			}
			return results.ToString();
		}

		public virtual string ShowInventoryTable(String sProductID, String sVariantID)
		{
			InputValidator IV = new InputValidator("ShowInventoryTable");
			int ProductID = IV.ValidateInt("ProductID", sProductID);
			int VariantID = IV.ValidateInt("VariantID", sVariantID);
			StringBuilder results = new StringBuilder("");
			if(AppLogic.AppConfigBool("ShowInventoryTable"))
			{
				results.Append(AppLogic.GetInventoryTable(ProductID, VariantID, ThisCustomer.IsAdminUser, ThisCustomer.SkinID, true, false));
			}
			return results.ToString();
		}

		//this version of GetVariantPrice is used only for backward compatibility.  New packages should use one of the methods that accept TaxClassID
		public virtual string GetVariantPrice(String sVariantID, String sHidePriceUntilCart, string sPrice, string sSalePrice, string sExtPrice, String sPoints, string sSalesPromptName)
		{
			return GetVariantPrice(sVariantID, sHidePriceUntilCart, sPrice, sSalePrice, sExtPrice, sPoints, sSalesPromptName, "True", "1", "0.00");
		}

		public virtual string GetVariantPrice(String sVariantID, String sHidePriceUntilCart, string sPrice, string sSalePrice, string sExtPrice, String sPoints, string sSalesPromptName, string sTaxClassID)
		{
			return GetVariantPrice(sVariantID, sHidePriceUntilCart, sPrice, sSalePrice, sExtPrice, sPoints, sSalesPromptName, "True", sTaxClassID, "0.00");
		}

		public virtual string GetVariantPrice(String sVariantID, String sHidePriceUntilCart, string sPrice, string sSalePrice, string sExtPrice, String sPoints, string sSalesPromptName, String sShowpricelabel, string sTaxClassID)
		{
			return GetVariantPrice(sVariantID, sHidePriceUntilCart, sPrice, sSalePrice, sExtPrice, sPoints, sSalesPromptName, sShowpricelabel, sTaxClassID, "0.00");
		}

		public virtual string GetVariantPrice(String sVariantID, String sHidePriceUntilCart, string sPrice, string sSalePrice, string sExtPrice, String sPoints, string sSalesPromptName, String sShowpricelabel, string sTaxClassID, String sChosenAttributesPriceDelta)
		{
			return GetVariantPrice(sVariantID, sHidePriceUntilCart, sPrice, sSalePrice, sExtPrice, sPoints, sSalesPromptName, sShowpricelabel, sTaxClassID, sChosenAttributesPriceDelta, "true");
		}

		public virtual string GetVariantPrice(String sVariantID, String sHidePriceUntilCart, string sPrice, string sSalePrice, string sExtPrice, String sPoints, string sSalesPromptName, String sShowpricelabel, string sTaxClassID, String sChosenAttributesPriceDelta, String sIncludeHTMLMarkup)
		{
			// validate paramters
			InputValidator IV = new InputValidator("GetVariantPrice");
			int variantID = IV.ValidateInt("VariantID", sVariantID);
			bool hidePriceUntilCart = IV.ValidateBool("HidePriceUntilCart", sHidePriceUntilCart);
			decimal regularPrice = IV.ValidateDecimal("Price", sPrice);
			decimal salePrice = IV.ValidateDecimal("SalePrice", sSalePrice);
			decimal extPrice = IV.ValidateDecimal("ExtPrice", sExtPrice);
			int points = IV.ValidateInt("Points", sPoints);
			string salesPromptName = IV.ValidateString("SalesPromptName", sSalesPromptName);
			bool showPriceLabel = IV.ValidateBool("Showpricelabel", sShowpricelabel);
			int taxClassID = IV.ValidateInt("TaxClassID", sTaxClassID);
			decimal attributesPriceDelta = IV.ValidateDecimal("AttributesPriceDelta", sChosenAttributesPriceDelta);
			bool includeHTMLMarkup = IV.ValidateBool("IncludeHTMLMarkup", sIncludeHTMLMarkup);
			decimal discountedPrice = System.Decimal.Zero;
			decimal schemaPrice = 0;

			// instantiate return variable
			StringBuilder results = new StringBuilder(1024);
			results.Append("<div class=\"price-wrap\">");
			// short-circuit this procedure if the price will be hidden
			if(hidePriceUntilCart)
			{
				return string.Empty;
			}

			string taxSuffix = string.Empty;

			bool taxable = false;

			if(m_VATOn)
			{
				taxable = Prices.IsTaxable(variantID);

				// get suffix to display after pricing             
				if(m_VATEnabled)
				{
					// put another validation stuff here if the product variant is taxable or not 
					// set tax suffix to an empty string when the item is non-taxable regardless if the VAT Setting is either
					// Inclusive or Exclusive
					if(m_VATOn && !taxable)
					{
						taxSuffix = String.Empty;
					}
					else if(m_VATOn && taxable)
					{
						taxSuffix = "inc vat";
					}
					else if(!m_VATOn && taxable)
					{
						taxSuffix = "Excluding VAT";
					}
				}
			}

			Decimal origRegularPrice = regularPrice;

			// add inclusive tax, convert all pricing to ThisCustomer's currency, and round
			regularPrice = Prices.VariantPrice(ThisCustomer, variantID, origRegularPrice, salePrice, extPrice, attributesPriceDelta, false, taxClassID);
			discountedPrice = Prices.VariantPrice(ThisCustomer, variantID, origRegularPrice, salePrice, extPrice, attributesPriceDelta, true, taxClassID);

			// format pricing
			string regularPriceFormatted = Localization.CurrencyStringForDisplayWithExchangeRate(regularPrice, ThisCustomer.CurrencySetting);
			string discountedPriceFormatted = Localization.CurrencyStringForDisplayWithExchangeRate(discountedPrice, ThisCustomer.CurrencySetting);

			// get pricing labels
			string genericPriceLabel = string.Empty;
			string regularPriceLabel = string.Empty;
			string salePriceLabel = string.Empty;
			string customerLevelName = string.Empty;

			if(showPriceLabel)
			{
				genericPriceLabel = "Price:";
				regularPriceLabel = "Regular Price:";
				salePriceLabel = salesPromptName + ":";
				customerLevelName = ThisCustomer.CustomerLevelName;
			}

			// format micropay points
			string pointsFormatted = string.Empty;
			if(AppLogic.AppConfigBool("MicroPay.ShowPointsWithPrices"))
			{
				pointsFormatted = "(" + points.ToString() + " Points)";
			}

			// create results string
			// handle non-discounted customerLevel cases
			if(ThisCustomer.CustomerLevelID == 0 || (ThisCustomer.LevelDiscountPct == 0 && extPrice == 0))
			{
				if(AppLogic.HideForWholesaleSite(ThisCustomer.CustomerLevelID))  // wholesale site with default customerLevel
				{

				}
				else  // show Level 0 Pricing
				{
					if(salePrice == 0 || ThisCustomer.CustomerLevelID > 0)
					{
						if(includeHTMLMarkup)
						{
							results.Append("<div class=\"variant-price\"><span>" + genericPriceLabel + "</span> " + regularPriceFormatted + "</div>");
						}
						else
						{
							results.Append(genericPriceLabel + regularPriceFormatted);
						}
						schemaPrice = regularPrice;
					}
					else if(includeHTMLMarkup)
					{
						results.Append("<div class=\"price regular-price\"><span>" + regularPriceLabel + "</span> " + regularPriceFormatted + "</div>");
						results.Append("<div class=\"price sale-price\"><span>" + salePriceLabel + "</span> " + discountedPriceFormatted + "</div>");
						schemaPrice = discountedPrice;
					}
					else
					{
						results.Append(" " + regularPriceLabel + regularPriceFormatted + " " + salePriceLabel + discountedPriceFormatted);
						schemaPrice = discountedPrice;
					}

					results.Append(" ");

					results.Append(taxSuffix);
				}
			}

			// handle discounted customerLevels
			else
			{
				results.Append("<div class=\"price regular-price\">" + regularPriceLabel + " " + regularPriceFormatted + "</div>");
				results.Append("<div class=\"price level-price\">" + customerLevelName + " " + regularPriceLabel + " " + discountedPriceFormatted + "</div>");
				schemaPrice = discountedPrice;
				results.Append(pointsFormatted);
				results.Append(taxSuffix);
			}

			if(schemaPrice > 0)
			{
				var storeDefaultCultureInfo = CultureInfo.GetCultureInfo(Localization.GetDefaultLocale());
				var formattedSchemaPrice = String.Format(storeDefaultCultureInfo, "{0:C}", schemaPrice);
				var schemaRegionInfo = new RegionInfo(storeDefaultCultureInfo.Name);

				results.AppendFormat("<meta itemprop=\"price\" content=\"{0}\"/>", formattedSchemaPrice);
				results.AppendFormat("<meta itemprop=\"priceCurrency\" content=\"{0}\"/>", schemaRegionInfo.ISOCurrencySymbol);
			}

			results.Append("</div>");
			return results.ToString();
		}

		public virtual decimal GetVariantPriceDecimal(String sVariantID, string sPrice, string sSalePrice, string sExtPrice, String sPoints, string sTaxClassID, string sChosenAttributesPriceDelta, string sConvertForCustomerCurrency, string sUseAnonymousCustomer)
		{
			// validate paramters
			InputValidator IV = new InputValidator("GetVariantPrice");
			int variantID = IV.ValidateInt("VariantID", sVariantID);
			decimal regularPrice = IV.ValidateDecimal("Price", sPrice);
			decimal salePrice = IV.ValidateDecimal("SalePrice", sSalePrice);
			decimal extPrice = IV.ValidateDecimal("ExtPrice", sExtPrice);
			int taxClassID = IV.ValidateInt("TaxClassID", sTaxClassID);
			decimal attributesPriceDelta = IV.ValidateDecimal("AttributesPriceDelta", sChosenAttributesPriceDelta);
			bool convertForCustomerCurrency = IV.ValidateBool("ConvertForCustomerCurrency", sConvertForCustomerCurrency);
			bool useAnonymousCustomer = IV.ValidateBool("UseAnonymousCustomer", sUseAnonymousCustomer);

			Customer customerToUse = ThisCustomer;
			if(useAnonymousCustomer)
				customerToUse = new Customer(true);

			decimal discountedPrice = System.Decimal.Zero;
			Decimal origRegularPrice = regularPrice;

			// add inclusive tax, convert all pricing to ThisCustomer's currency, and round
			regularPrice = Prices.VariantPrice(customerToUse, variantID, origRegularPrice, salePrice, extPrice, attributesPriceDelta, false, taxClassID);
			discountedPrice = Prices.VariantPrice(customerToUse, variantID, origRegularPrice, salePrice, extPrice, attributesPriceDelta, true, taxClassID);

			// format pricing
			if(convertForCustomerCurrency)
			{
				regularPrice = Currency.Convert(regularPrice, Localization.GetPrimaryCurrency(), customerToUse.CurrencySetting);
				discountedPrice = Currency.Convert(discountedPrice, Localization.GetPrimaryCurrency(), customerToUse.CurrencySetting);
			}

			// create results string
			// handle non-discounted customerLevel cases
			if(customerToUse.CustomerLevelID == 0 || (customerToUse.LevelDiscountPct == 0 && extPrice == 0))
			{
				if(salePrice == 0 || customerToUse.CustomerLevelID > 0)
				{
					return regularPrice;
				}
				else
				{
					return discountedPrice;
				}
			}
			// handle discounted customerLevels
			else
			{
				return discountedPrice;
			}
		}

		public virtual string GetUpsellVariantPrice(String sVariantID, String sHidePriceUntilCart, string sPrice, string sSalePrice, string sExtPrice, String sPoints, string sSalesPromptName, String sShowpricelabel, string sTaxClassID, string decUpSelldiscountPct)
		{
			return Prices.GetUpsellVariantPrice(ThisCustomer, sVariantID, sHidePriceUntilCart, sPrice, sSalePrice, sExtPrice, sPoints, sSalesPromptName, sShowpricelabel, sTaxClassID, decUpSelldiscountPct);
		}

		public virtual string GetUpsellVariantPrice(String sVariantID, String sHidePriceUntilCart, string sPrice, string sSalePrice, string sExtPrice, String sPoints, string sSalesPromptName, String sShowpricelabel, string sTaxClassID, string decUpSelldiscountPct, string sProductID)
		{
			return Prices.GetUpsellVariantPrice(ThisCustomer, sVariantID, sHidePriceUntilCart, sPrice, sSalePrice, sExtPrice, sPoints, sSalesPromptName, sShowpricelabel, sTaxClassID, decUpSelldiscountPct, sProductID);
		}

		[Obsolete("deprecated (10.0) no longer used")]
		public virtual string GetCartPrice(string intProductID, string intQuantity, string decProductPrice, string intTaxClassID)
		{
			return Prices.GetCartPrice(ThisCustomer, intProductID, intQuantity, decProductPrice, intTaxClassID);
		}

		public virtual Decimal SubTotal()
		{
			ShoppingCart cart = new ShoppingCart(Convert.ToInt32(SkinID()), ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);
			return Prices.SubTotal(true, false, true, true, true, true, cart.CartItems, cart.ThisCustomer, cart.OrderOptions);
		}

		public virtual Decimal SubTotal(bool includeDiscounts)
		{
			ShoppingCart cart = new ShoppingCart(Convert.ToInt32(SkinID()), ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);
			return Prices.SubTotal(includeDiscounts, false, true, true, true, true, cart.CartItems, cart.ThisCustomer, cart.OrderOptions);
		}

		public virtual Decimal SubTotal(bool includeDiscounts, bool onlyIncludeTaxableItems)
		{
			ShoppingCart cart = new ShoppingCart(Convert.ToInt32(SkinID()), ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);
			return Prices.SubTotal(includeDiscounts, onlyIncludeTaxableItems, true, true, true, true, cart.CartItems, cart.ThisCustomer, cart.OrderOptions);
		}

		public virtual Decimal SubTotal(bool includeDiscounts, bool onlyIncludeTaxableItems, bool includeDownloadItems)
		{
			ShoppingCart cart = new ShoppingCart(Convert.ToInt32(SkinID()), ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);
			return Prices.SubTotal(includeDiscounts, onlyIncludeTaxableItems, includeDownloadItems, true, true, true, cart.CartItems, cart.ThisCustomer, cart.OrderOptions);
		}

		public virtual Decimal SubTotal(bool includeDiscounts, bool onlyIncludeTaxableItems, bool includeDownloadItems, bool includeFreeShippingItems)
		{
			ShoppingCart cart = new ShoppingCart(Convert.ToInt32(SkinID()), ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);
			return Prices.SubTotal(includeDiscounts, onlyIncludeTaxableItems, includeDownloadItems, includeFreeShippingItems, true, true, cart.CartItems, cart.ThisCustomer, cart.OrderOptions);
		}

		public virtual Decimal SubTotal(bool includeDiscounts, bool onlyIncludeTaxableItems, bool includeDownloadItems, bool includeFreeShippingItems, bool includeSystemItems)
		{
			ShoppingCart cart = new ShoppingCart(Convert.ToInt32(SkinID()), ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);
			return Prices.SubTotal(includeDiscounts, onlyIncludeTaxableItems, includeDownloadItems, includeFreeShippingItems, includeSystemItems, true, cart.CartItems, cart.ThisCustomer, cart.OrderOptions);
		}

		[Obsolete("deprecated (10.0) no longer used")]
		public virtual string CartSubTotal()
		{
			ShoppingCart cart = new ShoppingCart(Convert.ToInt32(SkinID()), ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);
			return "Subtotal: " + Localization.CurrencyStringForDisplayWithoutExchangeRate(Prices.SubTotal(true, false, true, true, true, true, cart.CartItems, cart.ThisCustomer, cart.OrderOptions), ThisCustomer.CurrencySetting);
		}

		public virtual string EntityPageHeaderDescription(string sEntityName, String sEntityID)
		{
			InputValidator IV = new InputValidator("EntityPageHeaderDescription");
			String EntityName = IV.ValidateString("EntityName", sEntityName);
			int EntityID = IV.ValidateInt("EntityID", sEntityID);
			StringBuilder results = new StringBuilder("");

			string EntityInstancePicture = AppLogic.LookupImage(EntityName, EntityID, "medium", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);

			// boolean flag indicating a supported entity
			bool entitySupported = false;

			// loop through each of the supported entities
			foreach(String eName in AppLogic.ro_SupportedEntities)
			{
				// if EntityName is equal to a supported entity
				if(eName.EqualsIgnoreCase(EntityName))
				{
					// set EntityName to the name of a supported entity
					EntityName = eName;

					// flip flag indicating a supported entity
					entitySupported = true;
					break;
				}
			}

			// if a supported entity was not passed in, just return an empty string
			if(!entitySupported)
			{
				return String.Empty;
			}

			// end safety check

			string EntityInstanceDescription = DB.GetSqlS("select Description as S from dbo.{0} with(NOLOCK) where {0}ID=".FormatWith(EntityName) + EntityID.ToString());
			bool hasImage = EntityInstanceDescription.Length != 0 && EntityInstancePicture.IndexOf("nopicture") == -1;
			
			if(AppLogic.ReplaceImageURLFromAssetMgr)
			{
				EntityInstanceDescription = EntityInstanceDescription.Replace("../images", "images");
			}

			if(AppLogic.AppConfigBool("UseParserOnEntityDescriptions"))
			{
                Parser p = new Parser();
				EntityInstanceDescription = p.ReplaceTokens(EntityInstanceDescription);
			}

			if(AppLogic.AppConfigBool("Force" + EntityName + "HeaderDisplay") ||
				EntityInstanceDescription.Length != 0)
			{
				results.Append("<div class=\"row entity-description-row\">\n");
				if(hasImage)
				{
					results.Append("<div class=\"col-sm-4 entity-image-wrap\"><img class=\"grid-item-image img-responsive product-image\" src=\"" + EntityInstancePicture + "\"></div>");
				}
				results.Append(hasImage ? "<div class=\"col-sm-8 entity-description-wrap\">" : "<div class=\"col-sm-12 entity-description-wrap\">");
				results.Append("<div class=\"grid-column-inner\">");
				results.Append(EntityInstanceDescription);
				results.Append("</div>\n");
				results.Append("</div>\n");
				results.Append("</div>\n");
			}

			return results.ToString();
		}

		public virtual string GetMLValue(XPathNodeIterator MLContent)
		{
			return GetMLValue(MLContent, ThisCustomer.LocaleSetting);
		}

		public virtual string GetMLValue(XPathNodeIterator MLContent, string sLocale)
		{
			InputValidator IV = new InputValidator("GetMLValue");
			String Locale = IV.ValidateString("Locale", sLocale);
			if(Locale.Length == 0)
			{
				Locale = ThisCustomer.LocaleSetting;
			}
			return GetMLValue(MLContent, Locale, "FALSE");
		}

		public virtual string GetMLValue(XPathNodeIterator MLContent, string sLocale, String sXMLEncodeOutput)
		{
			InputValidator IV = new InputValidator("GetMLValue");
			String Locale = IV.ValidateString("Locale", sLocale);
			bool XMLEncodeOutput = IV.ValidateBool("XMLEncodeOutput", sXMLEncodeOutput);
			XPathNavigator xpn;

			if(MLContent.MoveNext())
				xpn = MLContent.Current;
			else
				return string.Empty;

			try
			{
				xpn.MoveToFirstChild();
				if(xpn.NodeType ==
					XPathNodeType.Text)
				{
					if(XMLEncodeOutput)
					{
						return xpn.OuterXml.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("'", "&apos;").Replace("\"", "&quot;").Trim();
					}
					else
					{
						return HttpContext.Current.Server.HtmlDecode(xpn.OuterXml).Trim();
					}
				}
				else
				{
					XPathNavigator n = xpn.SelectSingleNode("./locale[@name='" + Locale + "']");
					if(n == null)
					{
						if(Locale != Localization.GetDefaultLocale())
						{
							n = xpn.SelectSingleNode("./locale[@name='" + Localization.GetDefaultLocale() + "']");
							if(n == null)
							{
								return "";
							}
							else
							{
								if(XMLEncodeOutput)
								{
									return n.InnerXml.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("'", "&apos;").Replace("\"", "&quot;").Trim();
								}
								else
								{
									return HttpContext.Current.Server.HtmlDecode(n.InnerXml).Trim();
								}
							}
						}
						return "";
					}
					else
					{
						if(XMLEncodeOutput)
						{
							return n.InnerXml.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("'", "&apos;").Replace("\"", "&quot;").Trim();
						}
						else
						{
							return HttpContext.Current.Server.HtmlDecode(n.InnerXml).Trim();
						}
					}
				}
			}
			catch(Exception ex)
			{
				return "MLData Error: " + ex.Message;
			}
		}

		public virtual string Ellipses(string sContent, String sReturnLength, String sBreakBetweenWords)
		{
			InputValidator IV = new InputValidator("Ellipses");
			String Content = IV.ValidateString("Content", sContent);
			int ReturnLength = IV.ValidateInt("sReturnLength", sReturnLength);
			bool BreakBetweenWords = IV.ValidateBool("BreakBetweenWords", sBreakBetweenWords);
			return CommonLogic.Ellipses(Content, ReturnLength, BreakBetweenWords);
		}

		public virtual string SizeColorQtyOption(String sProductID, String sVariantID, string sColors, string sSizes)
		{
			return SizeColorQtyOption(sProductID, sVariantID, sColors, sSizes, "Color", "Size");
		}

		public virtual string SizeColorQtyOption(String sProductID, String sVariantID, string sColors, string sSizes, string sColorPrompt, string sSizePrompt)
		{
			return SizeColorQtyOption(sProductID, sVariantID, sColors, sSizes, "Color", "Size", "");
		}

		public virtual string SizeColorQtyOption(String sProductID, String sVariantID, string sColors, string sSizes, string sColorPrompt, string sSizePrompt, string sRestrictedQuantities)
		{
			return SizeColorQtyOption(sProductID, sVariantID, sColors, sSizes, "Color", "Size", "", "0", "", "1");
		}

		public virtual string SizeColorQtyOption(string sProductId, string sVariantId, string sColors, string sSizes, string sColorPrompt, string sSizePrompt, string sRestrictedQuantities, string boolCustomerEntersPrice, string sCustomerEntersPricePrompt, string intTaxClassID)
		{
			var inputValidator = new InputValidator("SizeColorQtyOption");
			var productId = inputValidator.ValidateInt("ProductID", sProductId);
			var variantId = inputValidator.ValidateInt("VariantID", sVariantId);
			var colors = inputValidator.ValidateString("Colors", sColors);
			var sizes = inputValidator.ValidateString("Sizes", sSizes);

			if(variantId == 0)
				variantId = AppLogic.GetDefaultProductVariant(productId);

			if(productId == 0 || variantId == 0)
				return string.Empty;

			sColorPrompt = !string.IsNullOrEmpty(sColorPrompt)
				? sColorPrompt
				: "Color";

			sSizePrompt = !string.IsNullOrEmpty(sSizePrompt)
				? sSizePrompt
				: "Size";

			if(HtmlHelper == null)
				throw new Exception("An HtmlHelper is required for this method. Make sure to specify one when you call the RunXmlPackage method");
			
			var addToCartHtml = HtmlHelper.Action(
				actionName: ActionNames.BulkAddToCartForm,
				controllerName: ControllerNames.ShoppingCart,
				routeValues: new
				{
					productId = productId,
					variantId = variantId
		});

			return addToCartHtml.ToString();
		}

		public virtual string GetSizePriceDelta(string sSizes)
		{
			InputValidator IV = new InputValidator("SizePriceDelta");
			String Sizes = IV.ValidateString("Sizes", sSizes);
			int TaxClassID = IV.ValidateInt("TaxClassID", "1");

			StringBuilder results = new StringBuilder("");
			String[] SizesSplit = Sizes.Split(',');


			for(int i = SizesSplit.GetLowerBound(0); i <= SizesSplit.GetUpperBound(0); i++)
			{
				string SizeString = SizesSplit[i].Trim();
				if(SizeString.IndexOf("[") != -1)
				{
					decimal SizePriceDelta = AppLogic.GetColorAndSizePriceDelta("", SizesSplit[i].Trim(), TaxClassID, ThisCustomer, true, true);
					SizeString = SizeString.Substring(0, SizesSplit[i].IndexOf("[")).Trim() + " [" + AppLogic.AppConfig("AjaxPricingPrompt") + CommonLogic.IIF(SizePriceDelta > 0, "+", "") + ThisCustomer.CurrencyString(SizePriceDelta) + "]";
				}
				results.Append(SizeString);
			}


			return results.ToString();

		}

		public virtual string GetColorPriceDelta(string sColors)
		{
			InputValidator IV = new InputValidator("ColorPriceDelta");
			String Colors = IV.ValidateString("Colors", sColors);
			int TaxClassID = IV.ValidateInt("TaxClassID", "1");

			StringBuilder results = new StringBuilder("");
			String[] ColorsSplit = Colors.Split(',');

			for(int i = ColorsSplit.GetLowerBound(0); i <= ColorsSplit.GetUpperBound(0); i++)
			{
				string ColorString = ColorsSplit[i].Trim();
				if(ColorString.IndexOf("[") != -1)
				{
					decimal ColorPriceDelta = AppLogic.GetColorAndSizePriceDelta(ColorsSplit[i].Trim(), "", TaxClassID, ThisCustomer, true, true);
					ColorString = ColorString.Substring(0, ColorsSplit[i].IndexOf("[")).Trim() + " [" + AppLogic.AppConfig("AjaxPricingPrompt") + CommonLogic.IIF(ColorPriceDelta > 0, "+", "") + ThisCustomer.CurrencyString(ColorPriceDelta) + "]";
				}
				results.Append(ColorString);
			}


			return results.ToString();

		}

		public virtual string ReplaceNewLineWithBR(string sContent)
		{
			InputValidator IV = new InputValidator("ReplaceNewLineWithBR");
			String Content = IV.ValidateString("Content", sContent);
			return Content.Replace("\n", "");
		}

		public virtual string GetOrderReceiptCCNumber(string sLast4, string sCardType, string sCardExpirationMonth, string sCardExpirationYear)
		{
			InputValidator IV = new InputValidator("GetOrderReceiptCCNumber");
			String Last4 = IV.ValidateString("Last4", sLast4);
			String CardType = IV.ValidateString("CardType", sCardType);
			StringBuilder results = new StringBuilder("");

			if(CardType.StartsWith("paypal", StringComparison.InvariantCultureIgnoreCase))
			{
				results.Append("<tr><td align=\"left\" width=\"20%\">Card Number:</td><td colspan=\"3\" width=\"80%\" align=\"left\">Not Available</td></tr>");
				results.Append("<tr><td align=\"left\" width=\"20%\">Card Expiration:</td><td colspan=\"3\" width=\"80%\" align=\"left\">Not Available</td></tr>");
			}
			else
			{
				results.Append("<tr><td align=\"left\" width=\"20%\">Card Number:</td><td colspan=\"3\" width=\"80%\" align=\"left\">****" + Last4 + "</td></tr>");
			}
			return results.ToString();
		}

		public virtual string DisplayOrderOptions(string sOrderOptions, string sViewInLocaleSetting, String sUseFullPathToImages)
		{
			InputValidator IV = new InputValidator("DisplayOrderOptions");
			String OrderOptions = IV.ValidateString("OrderOptions", sOrderOptions);
			String ViewInLocaleSetting = IV.ValidateString("ViewInLocaleSetting", sViewInLocaleSetting);
			bool UseFullPathToImages = IV.ValidateBool("sUseFullPathToImages", sUseFullPathToImages);
			StringBuilder results = new StringBuilder("");
			if(OrderOptions.Length != 0)
			{
				results.Append("<div align=\"center\" width=\"100%\">");

				results.Append("<table cellpadding=\"2\" cellspacing=\"0\" border=\"0\">");
				results.Append("<tr>");
				results.Append("<td align=\"left\"><span class=\"OrderOptionsRowHeader\">You Have Selected The Following Order Options:</span></td>");
				results.Append("<td align=\"center\"><span class=\"OrderOptionsRowHeader\">Cost</span></td>");
				results.Append("</tr>");
				foreach(String s in OrderOptions.Split('^'))
				{
					String[] flds = s.Split('|');
					results.Append("<tr>");
					results.Append("<td align=\"left\">");
					String ImgUrl = AppLogic.LookupImage("OrderOption", Localization.ParseUSInt(flds[0]), "icon", ThisCustomer.SkinID, ViewInLocaleSetting);
                    if (UseFullPathToImages && !AppLogic.IsCDNImagePath(ImgUrl))
                    {
                        if (ImgUrl.StartsWith("../"))
                        {
                            ImgUrl = ImgUrl.Replace("../", "");
                        }
                        ImgUrl = AppLogic.GetStoreHTTPLocation(true) + ImgUrl;
                    }
                    if (ImgUrl.Length != 0 &&
						ImgUrl.IndexOf("nopicture") == -1)
					{
						results.Append("<img src=\"" + ImgUrl + "\" align=\"absmiddle\">");
					}
					results.Append("<span class=\"OrderOptionsName\">" + flds[1] + "</span></td>");
					string VAT = "";
					if(m_VATOn)
					{
						VAT = "VAT: 0.00";
					}
					if(flds.Length > 3)
					{
						VAT = "VAT: " + flds[3];
					}
					results.Append("<td width=\"150\" align=\"center\">");
					results.Append("<span class=\"OrderOptionsPrice\">");
					results.Append(flds[2]);
					if(AppLogic.AppConfigBool("VAT.Enabled"))
					{
						results.Append("");
						results.Append(VAT);
					}
					results.Append("</span>");
					results.Append("</td>");
					ImgUrl = AppLogic.LocateImageURL("Skins/" + SkinProvider.GetSkinNameById(ThisCustomer.SkinID) + "/images/selected.gif");
                    if (UseFullPathToImages && !AppLogic.IsCDNImagePath(ImgUrl))
                    {
                        if (ImgUrl.StartsWith("../"))
                        {
                            ImgUrl = ImgUrl.Replace("../", "");
                        }
                        ImgUrl = AppLogic.GetStoreHTTPLocation(true) + ImgUrl;
                    }
                    results.Append("</tr>");
				}
				results.Append("</table>");
				results.Append("</div>");
			}

			return results.ToString();
		}

		public virtual string ToUpper(string sStrValue)
		{
			InputValidator IV = new InputValidator("ToUpper");
			String StrValue = IV.ValidateString("StrValue", sStrValue);
			return StrValue.ToUpperInvariant();
		}

		public virtual string ToLower(string sStrValue)
		{
			InputValidator IV = new InputValidator("ToLower");
			String StrValue = IV.ValidateString("StrValue", sStrValue);
			return StrValue.ToLowerInvariant();
		}

		public virtual string OrderShippingCalculation(string sPaymentMethod, string sShippingMethod, string sShippingTotal, String sShippingCalculationID, String sShipAddresses, String sIsAllDownloadComponents, String sIsAllFreeShippingComponents, String sIsAllSystemComponents)
		{
			InputValidator IV = new InputValidator("OrderShippingCalculation");
			String PaymentMethod = IV.ValidateString("PaymentMethod", sPaymentMethod);
			String ShippingMethod = IV.ValidateString("ShippingMethod", sShippingMethod);
			Decimal ShippingTotal = IV.ValidateDecimal("ShippingTotal", sShippingTotal);

			//this will handle if sShippingCalculationID is null or empty. For shippingcalculationid that is not set

			if(CommonLogic.IsStringNullOrEmpty(sShippingCalculationID))
			{
				sShippingCalculationID = ((int)Shipping.GetActiveShippingCalculationID()).ToString();
			}
			int ShippingCalculationID = IV.ValidateInt("ShippingCalculationID", sShippingCalculationID);
			int ShipAddresses = IV.ValidateInt("ShipAddresses", sShipAddresses);
			bool IsAllDownloadComponents = IV.ValidateBool("IsAllDownloadComponents", sIsAllDownloadComponents);
			bool IsAllFreeShippingComponents = IV.ValidateBool("sIsAllFreeShippingComponents", sIsAllFreeShippingComponents);
			bool IsAllSystemComponents = IV.ValidateBool("IsAllSystemComponents", sIsAllSystemComponents);

			StringBuilder results = new StringBuilder("");

			if(!AppLogic.AppConfigBool("SkipShippingOnCheckout"))
			{
				results.Append("<tr>");
				String ShowShipText = CommonLogic.IIF(ShipAddresses > 1, String.Empty, ShippingMethod);
				// strip out RT shipping cost, if any:
				if(IsAllDownloadComponents || IsAllSystemComponents)
				{
					ShowShipText = "FREE SHIPPING";
				}
				else if(IsAllFreeShippingComponents && !AppLogic.AppConfigBool("FreeShippingAllowsRateSelection"))
				{
					ShowShipText = "FREE SHIPPING";
				}
				else if(ShowShipText.IndexOf("|") != -1)
				{
					String[] ss2 = ShowShipText.Split('|');
					try
					{
						ShowShipText = ss2[0].Trim();
					}
					catch
					{
					}
				}
				if(ShippingCalculationID == 4)
				{
					ShowShipText = "FREE SHIPPING";
				}

				if(ShowShipText.Length != 0)
				{
					results.Append("<td align=\"right\" valign=\"top\" class=\"OrderSummaryLabel\">Shipping (" + ShowShipText + "):</td>");
				}
				else
				{
					results.Append("<td align=\"right\" valign=\"top\" class=\"OrderSummaryLabel\">Shipping:</td>");
				}
				string st = "Will Quote";
				if(PaymentMethod.Equals("REQUEST QUOTE", StringComparison.InvariantCultureIgnoreCase) == false)
				{
					//in case free shipping allows rate selection and they choose the free one
					if(ShippingTotal == 0.0M)
					{
						ShowShipText = "FREE SHIPPING";
					}
					st = FormatCurrencyHelper(ShippingTotal, ThisCustomer.CurrencySetting);
				}
				results.Append("<td align=\"right\" valign=\"top\" class=\"OrderSummaryValue\">" + st + "</td>");
				results.Append("</tr>");
			}
			return results.ToString();
		}

		public virtual string GetMultiVariantPayPalAd(string sProductId)
		{
			int productId = 0;
			int.TryParse(sProductId, out productId);

			int variantId = AppLogic.GetDefaultProductVariant(productId);

			if(AppLogic.GetNextVariant(productId, variantId) != variantId) // multi variant product
			{
				PayPalAd productPageAd = new PayPalAd(PayPalAd.TargetPage.Product);
				if(productPageAd.Show)
					return productPageAd.ImageScript;
			}
			return string.Empty;
		}

		public bool FileExists(String sFNOrUrl)
		{
			InputValidator IV = new InputValidator("FileExists");
			String FNOrUrl = IV.ValidateString("FNOrUrl", sFNOrUrl);
			// Name can be relative URL or physical file path!
			return CommonLogic.FileExists(CommonLogic.SafeMapPath(FNOrUrl));
		}

		// returns Payment Method cleaned (no spaces, or weird chars, all uppercased)
		public virtual string CleanPaymentMethod(String sPM)
		{
			InputValidator IV = new InputValidator("CleanPaymentMethod");
			String PM = IV.ValidateString("PM", sPM);
			return AppLogic.CleanPaymentMethod(PM);
		}

		// returns Payment Gateway cleaned (no spaces, or weird chars, all uppercased)
		public virtual string CleanPaymentGateway(String sGW)
		{
			InputValidator IV = new InputValidator("CleanPaymentGateway");
			String GW = IV.ValidateString("GW", sGW);
			return AppLogic.CleanPaymentGateway(GW);
		}

		// returns lowercase of string, invariant culture
		public virtual string StrToLower(String sS)
		{
			InputValidator IV = new InputValidator("StrToLower");
			String S = IV.ValidateString("S", sS);
			return S.ToLower(CultureInfo.InvariantCulture);
		}

		// returns uppercase of string, invariant culture
		public virtual string StrToUpper(String sS)
		{
			InputValidator IV = new InputValidator("StrToUpper");
			String S = IV.ValidateString("S", sS);
			return S.ToUpper(CultureInfo.InvariantCulture);
		}

		// returns capitalize of string, invariant culture
		public virtual string StrCapitalize(String sS)
		{
			InputValidator IV = new InputValidator("StrCapitalize");
			String S = IV.ValidateString("S", sS);
			return CommonLogic.Capitalize(S);
		}

		// returns trim of string
		public virtual string StrTrim(String sS)
		{
			InputValidator IV = new InputValidator("StrTrim");
			String S = IV.ValidateString("S", sS);
			return S.Trim();
		}

		// returns trim start of string
		public virtual string StrTrimStart(String sS)
		{
			InputValidator IV = new InputValidator("StrTrimStart");
			String S = IV.ValidateString("S", sS);
			return S.TrimStart();
		}

		// returns trim end of string
		public virtual string StrTrimEnd(String sS)
		{
			InputValidator IV = new InputValidator("StrTrimEnd");
			String S = IV.ValidateString("S", sS);
			return S.TrimEnd();
		}

		// returns string replace
		public virtual string StrReplace(String sS, String sOldValue, String sNewValue)
		{
			InputValidator IV = new InputValidator("StrReplace");
			String S = IV.ValidateString("S", sS);
			String OldValue = IV.ValidateString("OldValue", sOldValue);
			String NewValue = IV.ValidateString("NewValue", sNewValue);
			return S.Replace(OldValue, NewValue);
		}

		// returns string in "plural" form (this is almost impossible to do)!
		public virtual string StrMakePlural(String sS)
		{
			InputValidator IV = new InputValidator("StrMakePlural");
			String S = IV.ValidateString("S", sS);
			return S;
		}

		// returns string in "singular" form (this is almost impossible to do)!
		public virtual string StrMakeSingular(String sS)
		{
			InputValidator IV = new InputValidator("StrMakeSingular");
			String S = IV.ValidateString("S", sS);
			return S;
		}

		// returns a string in "multiline" form (preserves CR/LF's as <br/>'s)
		public virtual string StrMultiline(string textToDisplay)
		{
			InputValidator IV = new InputValidator("strMultiline");
			string validatedText = IV.ValidateString("textToDisplay", textToDisplay);
			return validatedText.Replace("\r\n", "<br/>");
		}

		//splits a string and puts it inside tags using the specified TagName, e.g. <TagName>value1</TagName><TagName>value2</TagName>..., the valuex will be XML Encoded
		public virtual string SplitString(string sS, string sDelimiter, string sTagName)
		{
			InputValidator IV = new InputValidator("SplitString");
			String S = IV.ValidateString("S", sS);
			String Delimiter = IV.ValidateString("Delimiter", sDelimiter);
			String TagName = IV.ValidateString("TagName", sTagName);
			if(S.Trim().Length == 0)
			{
				return "";
			}

			string tagStart = string.Empty;
			string tagEnd = string.Empty;
			if(TagName.Trim().Length > 0)
			{
				tagStart = "<" + TagName.Trim() + ">";
				tagEnd = "</" + TagName.Trim() + ">";
			}
			StringBuilder tmpS = new StringBuilder();
			char[] delim = Delimiter.ToCharArray();
			foreach(string sv in S.Split(delim))
			{
				tmpS.Append(tagStart + XmlCommon.XmlEncode(sv) + tagEnd);
			}
			return tmpS.ToString();
		}

		public virtual string InStr(string strSource, string strFind)
		{
			return strSource.IndexOf(strFind).ToString();
		}

		public virtual string GiftCardKey()
		{
			return GiftCard.GenerateGiftCardKey(ThisCustomer.CustomerID);
		}

		public virtual string MicroPayBalance()
		{
			Decimal mpd = AppLogic.GetMicroPayBalance(ThisCustomer.CustomerID);
			return FormatCurrencyHelper(mpd, ThisCustomer.CurrencySetting);
		}

		public virtual string StrFormat(string sSrcString, string sFormatParams, string sDelimiter)
		{
			InputValidator IV = new InputValidator("StrFormat");
			String SrcString = IV.ValidateString("SrcString", sSrcString);
			String FormatParams = IV.ValidateString("FormatParams", sFormatParams);
			String Delimiter = IV.ValidateString("Delimiter", sDelimiter);
			return String.Format(SrcString, FormatParams);
		}

		public virtual string ReadFile(string sFName)
		{
			InputValidator IV = new InputValidator("ReadFile");
			String FName = IV.ValidateString("FName", sFName);
			return CommonLogic.ReadFile(FName, true);
		}

		public virtual string LocateImageURL(string sImgUrl)
		{
			InputValidator IV = new InputValidator("LocateImageURL");
			String ImgUrl = IV.ValidateString("ImgUrl", sImgUrl);
			return AppLogic.LocateImageURL(ImgUrl);
		}

		public virtual string LocateImageURL(string sImgUrl, string sLocale)
		{
			InputValidator IV = new InputValidator("LocateImageURL");
			String ImgUrl = IV.ValidateString("ImgUrl", sImgUrl);
			String Locale = IV.ValidateString("Locale", sLocale);
			return AppLogic.LocateImageURL(ImgUrl, Locale);
		}

		public virtual string GetNativeShortDateString(String sDateTimeString)
		{
			InputValidator IV = new InputValidator("GetNativeShortDateString");
			DateTime dt = IV.ValidateDateTime("DateTimeString", sDateTimeString);
			return Localization.ToThreadCultureShortDateString(dt);
		}

		public virtual string GetLocaleShortDateString(String sDateTimeString)
		{
			return Localization.ParseLocaleDateTime(sDateTimeString, ThisCustomer.LocaleSetting).ToShortDateString();
		}

		public virtual string GetRatingStarsImage(String sRating)
		{
			InputValidator IV = new InputValidator("GetRatingStarsImage");
			Decimal Rating = IV.ValidateDecimal("Rating", sRating);
			return Ratings.BuildStarImages(Rating, ThisCustomer.SkinID);
		}

		public virtual string DisplayAddressString(String sAddressID)
		{
			InputValidator IV = new InputValidator("DisplayAddressString");
			int AddressID = IV.ValidateInt("AddressID", sAddressID);
			StringBuilder results = new StringBuilder("");
			Address adr = new Address();
			adr.LoadFromDB(AddressID);
			results.Append("<div style=\"margin-left: 10px;\">");
			results.Append(adr.DisplayHTML(false));
			results.Append("</div>");
			return results.ToString();
		}

		/// <summary>
		/// Parses incoming date string DateString and reformats it to a new date string according to the FormatString parameter
		/// </summary>
		/// <param name="strDate"></param>
		/// <param name="strFmt"></param>
		/// <returns></returns>
		public virtual string FormatDate(string strDate, string sSourceLocale, string strFmt)
		{
			InputValidator IV = new InputValidator("FormatDate");
			string SourceLocale = IV.ValidateString("SourceLocale", sSourceLocale);
			string sFormat = IV.ValidateString("FormatString", strFmt);
			try
			{
				DateTime dt = Localization.ParseLocaleDateTime(strDate, SourceLocale);
				return dt.ToString(sFormat);
			}
			catch(Exception ex)
			{
				return ex.Message;
			}
		}

		public virtual string GetLocalizedShortDate(string strDate, string sSourceLocale, string sTargetLocale)
		{
			if(string.IsNullOrEmpty(sTargetLocale))
				sTargetLocale = Localization.GetDefaultLocale();

			InputValidator IV = new InputValidator("FormatDate");
			string SourceLocale = IV.ValidateString("SourceLocale", sSourceLocale);
			string TargetLocale = IV.ValidateString("TargetLocale", sTargetLocale);
			try
			{
				DateTime dt = Localization.ParseLocaleDateTime(strDate, SourceLocale);
				CultureInfo ci = new CultureInfo(TargetLocale);

				return dt.ToString("d", ci);
			}
			catch(Exception ex)
			{
				return ex.Message;
			}
		}

		// returns:
		// if Date1 < Date2 => -1
		// if Date1 = Date2 => 0
		// if Date1 > Date2 -> 1
		// if Date1 or Date2 is empty string, they will be set to "now"
		public virtual String DateCompare(String sDate1, String sDate2)
		{
			InputValidator IV = new InputValidator("DateCompare");
			String Date1 = IV.ValidateString("Date1", sDate1);
			String Date2 = IV.ValidateString("Date2", sDate2);
			try
			{
				DateTime d1 = DateTime.Now;
				DateTime d2 = DateTime.Now;
				if(!Date1.Trim().Length.Equals(0))
				{
					d1 = Localization.ParseNativeDateTime(Date1);
				}
				if(!Date2.Trim().Length.Equals(0))
				{
					d2 = Localization.ParseNativeDateTime(Date2);
				}
				if(d1 < d2)
				{
					return "-1";
				}
				if(d1.Equals(d2))
				{
					return "0";
				}
				if(d1 > d2)
				{
					return "1";
				}
			}
			catch
			{
			}
			return "0";
		}

		public virtual string FormatDecimal(string sDecimalValue, string intFixPlaces)
		{
			InputValidator IV = new InputValidator("FormatDecimal");
			Decimal DecimalValue = IV.ValidateDecimal("DecimalValue", sDecimalValue);
			DecimalValue = Localization.ParseDBDecimal(sDecimalValue);
			int FixPlaces = IV.ValidateInt("intFixPlaces", intFixPlaces);
			DecimalValue = Decimal.Round(DecimalValue, FixPlaces, MidpointRounding.AwayFromZero);
			return DecimalValue.ToString(new CultureInfo(ThisCustomer.LocaleSetting));
		}

		public virtual string GetSplitString(string sStringToSplit, string sSplitChar, string intReturnIndex)
		{
			InputValidator IV = new InputValidator("GetSplitString");
			String StringToSplit = IV.ValidateString("StringToSplit", sStringToSplit);
			String SplitChar = IV.ValidateString("SplitChar", sSplitChar);
			int ReturnIndex = IV.ValidateInt("ReturnIndex", intReturnIndex);
			if(SplitChar.Length == 0)
			{
				SplitChar = ",";
			}
			String[] s = StringToSplit.Split(SplitChar.ToCharArray());
			String tmp = String.Empty;
			try
			{
				tmp = s[ReturnIndex];
			}
			catch
			{
			}
			return tmp;
		}

		public virtual string ConvertToBase64(string input)
		{
			if(input == null)
			{
				input = String.Empty;
			}
			byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(input);
			string base64Encoded = Convert.ToBase64String(bytes);
			return base64Encoded;
		}

		public virtual Boolean IsEmailGiftCard(int ProductID)
		{
			if(GiftCard.ProductIsEmailGiftCard(ProductID))
			{
				return true;
			}
			return false;
		}

		public virtual string GetNewKitItemOptions(String sProductID, string sPrice, string sSalePrice, string sExtendedPrice, String sHidePriceUntilCart, string sColors, String sShoppingCartRecID, String intTaxClassID)
		{
			return string.Empty;
		}

		/// <summary>
		/// Returns the content of CSS file based on skin ID
		/// </summary>
		/// <param name="skinId">Skin ID</param>
		/// <returns></returns>
		public virtual string GetReceiptCss(int skinId)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append("\n<style type=\"text/css\">\n");
			builder.Append(CommonLogic.ReadFile(String.Format("~/Skins/{0}/css/receipt.css", SkinProvider.GetSkinNameById(skinId)), true).ToString());
			builder.Append("\n</style>\n");
			return builder.ToString();
		}

		public IXPathNavigable XmlPackageAsXml(string packageName)
		{
			return XmlPackageAsXml(packageName, string.Empty);
		}

		public IXPathNavigable XmlPackageAsXml(string packageName, string runtimeParams)
		{
			XmlDocument doc = new XmlDocument();
			try
			{
				string result = AppLogic.RunXmlPackage(packageName, null, ThisCustomer, ThisCustomer.SkinID, string.Empty, runtimeParams, false, false);
				doc.LoadXml(result);
				return doc;
			}
			catch(Exception e)
			{
				String errorXML = String.Format("<error>{0}</error>", HtmlEncode(e.Message));
				doc.LoadXml(errorXML);
				return doc;
			}
		}

		public IXPathNavigable XmlStringAsXml(string xml)
		{
			return XmlStringAsXml(xml, string.Empty);
		}

		public IXPathNavigable XmlStringAsXml(string xml, string xpath)
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xml);

			if(!string.IsNullOrEmpty(xpath))
			{
				return doc.SelectSingleNode(xpath);
			}
			else
			{
				return doc;
			}
		}

		public IXPathNavigable SelectElementsFromIDDelimitedString(string delimitedString, string delimiter, XPathNodeIterator selection, string element)
		{
			XmlDocument doc = new XmlDocument();
			XmlNode rootNode = doc.CreateNode(XmlNodeType.Element, "root", string.Empty);

			string[] values = delimitedString.Split(delimiter.ToCharArray());

			// let's perform linear searching
			while(selection.MoveNext())
			{
				XmlNode itemNode = (selection.Current as IHasXmlNode).GetNode();
				foreach(string value in values)
				{
					if(itemNode[element].InnerText == value)
					{
						XmlNode copyNode = doc.ImportNode(itemNode, true);
						rootNode.AppendChild(copyNode);
						break;
					}
				}
			}

			return rootNode;
		}

		public IXPathNavigable CreateXmlFromDelimitedString(string delimitedString, string delimiter, string rootName, string elementName)
		{
			XmlDocument doc = new XmlDocument();

			XmlNode rootNode = doc.CreateNode(XmlNodeType.Element, rootName, string.Empty);
			doc.AppendChild(rootNode);

			string[] values = delimitedString.Split(delimiter.ToCharArray());

			foreach(string value in values)
			{
				XmlNode elementNode = doc.CreateNode(XmlNodeType.Element, elementName, string.Empty);
				elementNode.InnerText = value;

				rootNode.AppendChild(elementNode);
			}

			return rootNode;
		}

		public IXPathNavigable OrderOptionsAsXml(string strOrderOptions)
		{
			InputValidator validator = new InputValidator("OrderOptionsAsXml");
			string orderOptions = validator.ValidateString("OrderOptions", strOrderOptions);

			XmlDocument doc = new XmlDocument();
			XmlNode orderOptionNodes = doc.CreateNode(XmlNodeType.Element, "OrderOptions", string.Empty);

			if(!string.IsNullOrEmpty(orderOptions))
			{
				string[] orderOptionDelimitedValues = orderOptions.Split('^');
				foreach(string orderOptionDelimitedValue in orderOptionDelimitedValues)
				{
					string[] orderOptionValues = orderOptionDelimitedValue.Split('|');
					if(orderOptionValues != null && orderOptionValues.Length > 0)
					{
						int id = int.Parse(orderOptionValues[0]);
						string name = orderOptionValues[1];

						// NOTE :
						//  These fields are already Currency Formatted!!!
						string priceFormatted = orderOptionValues[2];

						bool withVat = orderOptionValues.Length >= 3;
						string vat = string.Empty;
						if(withVat)
						{
							vat = orderOptionValues[3];
						}

						XmlNode orderOptionNode = doc.CreateNode(XmlNodeType.Element, "OrderOption", string.Empty);

						// the details
						XmlNode idNode = doc.CreateNode(XmlNodeType.Element, "ID", string.Empty);
						XmlNode nameNode = doc.CreateNode(XmlNodeType.Element, "ProductName", string.Empty);
						XmlNode priceNode = doc.CreateNode(XmlNodeType.Element, "Price", string.Empty);
						XmlNode vatNode = doc.CreateNode(XmlNodeType.Element, "VAT", string.Empty);
						XmlNode imageUrlNode = doc.CreateNode(XmlNodeType.Element, "ImageUrl", string.Empty);

						idNode.InnerText = id.ToString();
						nameNode.InnerXml = name; // NOTE: this value may be localized, make sure to call GetMLValue on the xml package!!!
						priceNode.InnerText = priceFormatted;
						vatNode.InnerText = vat;

						// get the image info
						string imgUrl = AppLogic.LookupImage("OrderOption", id, "icon", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                        if (!AppLogic.IsCDNImagePath(imgUrl))
                        {
                            if (imgUrl.StartsWith("../"))
                            {
                                imgUrl = imgUrl.Replace("../", "");
                            }
                            imgUrl = AppLogic.GetStoreHTTPLocation(true) + imgUrl;
                        }
                        imageUrlNode.InnerText = imgUrl;

						orderOptionNode.AppendChild(idNode);
						orderOptionNode.AppendChild(nameNode);
						orderOptionNode.AppendChild(priceNode);
						orderOptionNode.AppendChild(vatNode);
						orderOptionNode.AppendChild(imageUrlNode);

						orderOptionNodes.AppendChild(orderOptionNode);
					}
				}
			}

			doc.AppendChild(orderOptionNodes);

			return doc;
		}

		public string CleanShippingMethod(string shippingMethod)
		{
			string cleanedShippingMethod = shippingMethod;

			// in case of RT Shipping
			if(shippingMethod.IndexOf('|') > -1)
			{
				string[] split = shippingMethod.Split('|');
				if(split.Length > 0)
				{
					cleanedShippingMethod = split[0];
				}
			}

			return cleanedShippingMethod;
		}

		public bool IsStringEmpty(string stringValue)
		{
			return string.IsNullOrEmpty(stringValue) || stringValue.Trim().Length == 0;
		}

		/// <summary>
		/// Gets the relative image path from the Skins/{SkinName} directory for the specified image
		/// </summary>
		/// <param name="fileName">The image file name</param>
		/// <returns>The relative path from the themes/image directory</returns>
		public string SkinImage(string fileName)
		{
			return AppLogic.SkinImage(fileName);
		}

		public string ProcessXsltExtensionArgs(params object[] args)
		{
			return args[0].ToString();
		}

		public string ProcessXsltExtension(object args)
		{
			return args.ToString();
		}

		public string ProcessXsltExtensionX(IXPathNavigable navi)
		{
			var nav = navi.CreateNavigator();
			return nav.ToString();
		}

		public string GetCurrentProtocol()
		{
			string protocol = "http";
			if(HttpContext.Current.Request.IsSecureConnection)
			{
				protocol = "https";
			}
			return protocol;
		}

		#region PRODUCT HELPER METHODS

		/// <summary>
		/// Breaks up the dimensions value and returns a single desired dimension
		/// </summary>
		/// <param name="Dimensions"></param>
		/// <param name="DesiredDimension"></param>
		/// <returns></returns>
		public string RetrieveDimension(string Dimensions, string DesiredDimension)
		{
			return AppLogic.RetrieveProductDimension(Dimensions, DesiredDimension);
		}

		/// <summary>
		/// Validates the weight text is either N|.N|N.N
		/// </summary>
		/// <param name="weightValue"></param>
		/// <returns></returns>
		public string ValidateWeight(string weightValue)
		{
			string trimmedWeight = weightValue.ToLower().Trim();
			string pattern = "^((\\+?\\d+)|(.\\+?\\d+)|(\\+?\\d+.\\+?\\d+))$";
			Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
			Match isMatch = rgx.Match(trimmedWeight);

			if(isMatch.Success)
			{
				return trimmedWeight;
			}
			else
			{
				return string.Empty;
			}
		}

		/// <summary>
		/// validates and formats GTIN
		/// </summary>
		/// <param name="GTINValue"></param>
		/// <returns></returns>
		public string ValidateGTIN(string GTINValue)
		{
			string trimmedGTIN = GTINValue.ToLower().Trim();
			string pattern = "^(\\+?\\d+)$";
			Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
			Match isMatch = rgx.Match(trimmedGTIN);

			if(isMatch.Success)
			{
				switch(trimmedGTIN.Length)
				{
					case 8:
						return string.Format("gtin8|{0}", trimmedGTIN);
					case 12:
						return string.Format("gtin13|0{0}", trimmedGTIN);
					case 13:
						return string.Format("gtin13|{0}", trimmedGTIN);
					case 14:
						return string.Format("gtin14|{0}", trimmedGTIN);
					default:
						return string.Empty;
				}
			}
			else
			{
				return string.Empty;
			}
		}

		/// <summary>
		/// Returns the product condition text by it's value
		/// </summary>
		/// <param name="conditionValue"></param>
		/// <returns></returns>
		public string RetrieveProductConditionText(string conditionValue)
		{
			int conditionInt = 0;
			bool isNumber = int.TryParse(conditionValue, out conditionInt);
			if(isNumber)
			{
				return Enum.GetName(typeof(ProductCondition), conditionInt);
			}
			else
			{
				return Enum.GetName(typeof(ProductCondition), 0);
			}
		}

		/// <summary>
		/// Declares options for a product condition
		/// </summary>
		public enum ProductCondition
		{
			New = 0,
			Used = 1,
			Refurbished = 2
		}

		/// <summary>
		/// Gets the in stock/out of stock text
		/// </summary>
		/// <param name="productId"></param>
		/// <param name="variantId"></param>
		/// <param name="page"></param>
		/// <returns></returns>
		public string GetStockStatusText(string productId, string variantId, string page)
		{
			string stockText = string.Empty;
			int productInt = 0;
			int variantInt = 0;

			if(int.TryParse(productId, out productInt) && productInt > 0)
			{
				if(int.TryParse(variantId, out variantInt) && variantInt > 0)
				{
					bool trackInventoryBySizeAndColor = AppLogic.ProductTracksInventoryBySizeAndColor(productInt);
					bool outOfStock = AppLogic.ProbablyOutOfStock(productInt, variantInt, trackInventoryBySizeAndColor, page);
					if(outOfStock)
					{
						stockText = "OutOfStock|Out Of Stock";
					}
					else
					{
						stockText = "InStock|In Stock";
					}
				}
			}
			return stockText;
		}

		#endregion

	}

}
