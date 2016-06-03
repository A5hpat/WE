// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;

namespace AspDotNetStorefrontCore
{
	/// <summary>
	/// </summary>
	public static class Currency
	{
		static public String m_LastRatesResponseXml = String.Empty;
		static public String m_LastRatesTransformedXml = String.Empty;

		static private XmlDocument RatesDoc
		{
			get
			{
				XmlDocument d = (XmlDocument)HttpContext.Current.Cache.Get("CurrencyDoc");
				if(d == null)
				{
					d = DB.GetSqlXmlDoc("select * from Currency  with (NOLOCK)  order by Published desc, DisplayOrder,Name for xml auto", null);
					HttpContext.Current.Cache.Insert("CurrencyDoc", d, null, System.DateTime.Now.AddMinutes(AppLogic.AppConfigUSInt("Localization.CurrencyCacheMinutes")), TimeSpan.Zero);
				}
				return d;
			}
		}

		static public void GetLiveRates()
		{
			String PN = AppLogic.AppConfig("Localization.CurrencyFeedXmlPackage");
			if(PN.Length != 0)
			{
				try
				{
					var package = new XmlPackage(PN);
					m_LastRatesResponseXml = package.XmlDataDocument.InnerXml;
					m_LastRatesTransformedXml = package.TransformString();
					if(m_LastRatesTransformedXml.Length != 0)
					{
						// update master db table:
						XmlDocument d = new XmlDocument();
						d.LoadXml(m_LastRatesTransformedXml);
						foreach(XmlNode n in d.SelectNodes("//currency"))
						{
							String CurrencyCode = XmlCommon.XmlAttribute(n, "code");
							String rate = XmlCommon.XmlAttribute(n, "rate");
							DB.ExecuteSQL("update Currency set ExchangeRate=" + rate + ", WasLiveRate=1, LastUpdated=getdate() where CurrencyCode=" + DB.SQuote(CurrencyCode));
						}
					}

					FlushCache(); // flush anyway for safety
				}
				catch(Exception ex)
				{
					try
					{
						AppLogic.SendMail(subject: AppLogic.AppConfig("StoreName") + " Currency.GetLiveRates Failure", body: "Occurred at: " + Localization.ToNativeDateTimeString(System.DateTime.Now) + CommonLogic.GetExceptionDetail(ex, ""), useHtml: false, fromAddress: AppLogic.AppConfig("MailMe_FromAddress"), fromName: AppLogic.AppConfig("MailMe_FromName"), toAddress: AppLogic.AppConfig("MailMe_ToAddress"), toName: AppLogic.AppConfig("MailMe_ToName"), bccAddresses: String.Empty, server: AppLogic.MailServer());
					}
					catch { }
				}
			}
		}

		static public void FlushCache()
		{
			try
			{
				HttpContext.Current.Cache.Remove("CurrencyDoc");
			}
			catch { }
		}

		static public bool isValidCurrencyCode(String CurrencyCode)
		{
			XmlNode n = RatesDoc.SelectSingleNode("//Currency[@CurrencyCode='" + CurrencyCode + "']");
			return (n != null);
		}

		static public Decimal GetExchangeRate(String CurrencyCode)
		{
			if(CurrencyCode.Length == 0)
			{
				throw new ArgumentException("Invalid CurrencyCode (empty string)");
			}
			Decimal tmpS = System.Decimal.Zero;
			XmlNode n = RatesDoc.SelectSingleNode("//Currency[@CurrencyCode=" + CommonLogic.SQuote(CurrencyCode) + "]");
			if(n != null)
			{
				tmpS = XmlCommon.XmlAttributeUSDecimal(n, "ExchangeRate");
			}
			return tmpS;
		}

		static public String GetFeedReferenceCurrencyCode()
		{
			return AppLogic.AppConfig("Localization.CurrencyFeedBaseRateCurrencyCode");
		}

		static public String GetCurrencyCode(int CurrencyID)
		{
			String tmpS = String.Empty;
			XmlNode n = RatesDoc.SelectSingleNode("//Currency[@CurrencyID=" + CurrencyID.ToString() + "]");
			if(n != null)
			{
				tmpS = XmlCommon.XmlAttribute(n, "CurrencyCode");
			}
			return tmpS;
		}

		static public String GetName(int CurrencyID)
		{
			String tmpS = String.Empty;
			XmlNode n = RatesDoc.SelectSingleNode("//Currency[@CurrencyID=" + CurrencyID.ToString() + "]");
			if(n != null)
			{
				tmpS = XmlCommon.XmlAttribute(n, "Name");
			}
			return tmpS;
		}

		static public String GetDisplayLocaleFormat(String CurrencyCode)
		{
			if(CurrencyCode.Length == 0)
			{
				throw new ArgumentException("Invalid CurrencyCode (empty string)");
			}
			String tmpS = String.Empty;
			XmlNode n = RatesDoc.SelectSingleNode("//Currency[@CurrencyCode=" + CommonLogic.SQuote(CurrencyCode) + "]");
			if(n != null)
			{
				tmpS = XmlCommon.XmlAttribute(n, "DisplayLocaleFormat");
			}
			return tmpS;
		}

		static public String GetDisplaySpec(String CurrencyCode)
		{
			if(CurrencyCode.Length == 0)
			{
				throw new ArgumentException("Invalid CurrencyCode (empty string)");
			}
			String tmpS = String.Empty;
			XmlNode n = RatesDoc.SelectSingleNode("//Currency[@CurrencyCode=" + CommonLogic.SQuote(CurrencyCode) + "]");
			if(n != null)
			{
				tmpS = XmlCommon.XmlAttribute(n, "DisplaySpec");
			}
			return tmpS;
		}

		static public Decimal Convert(Decimal SourceValue, String SourceCurrencyCode, String TargetCurrencyCode)
		{
			Decimal result = SourceValue;
			if(SourceCurrencyCode == TargetCurrencyCode)
			{
				return SourceValue;
			}
			if(result != System.Decimal.Zero && SourceCurrencyCode.ToLower() != TargetCurrencyCode.ToLower())
			{
				result = ConvertToBaseCurrency(result, SourceCurrencyCode);
				result = ConvertFromBaseCurrency(result, TargetCurrencyCode);
			}
			return result;
		}

		static public Decimal ConvertToBaseCurrency(Decimal SourceValue, String SourceCurrencyCode)
		{
			Decimal result = SourceValue;
			if(result != System.Decimal.Zero && SourceCurrencyCode != GetFeedReferenceCurrencyCode())
			{
				Decimal ExchangeRate = GetExchangeRate(SourceCurrencyCode);
				if(ExchangeRate == System.Decimal.Zero)
				{
					throw new ArgumentException("Exchange Rate Not Found for Currency=" + SourceCurrencyCode);
				}
				result = result / ExchangeRate;
			}
			return result;
		}

		static public Decimal ConvertFromBaseCurrency(Decimal SourceValue, String TargetCurrencyCode)
		{
			Decimal result = SourceValue;
			if(result != System.Decimal.Zero && TargetCurrencyCode != GetFeedReferenceCurrencyCode())
			{
				Decimal ExchangeRate = GetExchangeRate(TargetCurrencyCode);
				if(ExchangeRate == System.Decimal.Zero)
				{
					throw new ArgumentException("Exchange Rate Not Found for Currency=" + TargetCurrencyCode);
				}
				result = result * ExchangeRate;
			}
			return result;
		}

		static public String GetDefaultCurrency()
		{
			return Localization.GetPrimaryCurrency();
		}

		static public String GetDefaultCurrencySymbol()
		{
			var currencyAbbreviation = Localization.GetPrimaryCurrency();
			string currencySymbol = String.Empty;
			foreach(CultureInfo cultureInfo in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
			{
				RegionInfo region = new RegionInfo(cultureInfo.LCID);
				if(region.ISOCurrencySymbol == currencyAbbreviation)
				{
					currencySymbol = region.CurrencySymbol;
				}
			}
			return currencySymbol;
		}

		static public String ValidateCurrencySetting(String Currency)
		{
			String tmp = Localization.GetPrimaryCurrency();
			if(isValidCurrencyCode(Currency))
			{
				tmp = Currency;
			}
			return Localization.CheckCurrencySettingForProperCase(tmp);
		}

		static public int NumPublishedCurrencies()
		{
			return RatesDoc.SelectNodes("//Currency[@Published = 1]").Count;
		}

		static public String GetSelectList(String SelectName, String OnChangeHandler, String CssClass, String SelectedCurrencyCode)
		{
			StringBuilder tmpS = new StringBuilder(4096);
			tmpS.Append("<select size=\"1\" id=\"" + SelectName + "\" name=\"" + SelectName + "\"");
			if(OnChangeHandler.Length != 0)
			{
				tmpS.Append(" onChange=\"" + OnChangeHandler + "\"");
			}
			if(CssClass.Length != 0)
			{
				tmpS.Append(" class=\"" + CssClass + "\"");
			}
			tmpS.Append(">");
			foreach(XmlNode n in RatesDoc.SelectNodes("//Currency[@Published = 1]"))
			{
				string cc = XmlCommon.XmlAttribute(n, "CurrencyCode");
				tmpS.Append("<option value=\"" + cc + "\" " + CommonLogic.IIF(SelectedCurrencyCode == cc, " selected ", "") + ">" + cc + " (" + XmlCommon.XmlAttribute(n, "Name") + ")</option>");
			}
			tmpS.Append("</select>");
			return tmpS.ToString();
		}

		static public ArrayList getCurrencyList()
		{
			ArrayList list = new ArrayList();

			foreach(XmlNode n in RatesDoc.SelectNodes("//Currency[@Published = 1]"))
			{
				int cID = XmlCommon.XmlAttributeNativeInt(n, "CurrencyID");
				string cc = XmlCommon.XmlAttribute(n, "CurrencyCode");
				string cn = XmlCommon.XmlAttribute(n, "Name");
				ListItemClass item = new ListItemClass();
				item.Item = cc + " (" + cn + ")";
				item.Value = cID;
				list.Add(item);
			}

			return list;
		}

		static public IEnumerable<CurrencyInfo> GetCurrencies()
		{
			return RatesDoc
				.SelectNodes("//Currency[@Published = 1]")
				.OfType<XmlNode>()
				.Select(node => new CurrencyInfo(
					currencyId: XmlCommon.XmlAttributeNativeInt(node, "CurrencyID"),
					currencyGuid: Guid.Parse(XmlCommon.XmlAttribute(node, "CurrencyGUID")),
					name: XmlCommon.XmlAttribute(node, "Name"),
					currencyCode: XmlCommon.XmlAttribute(node, "CurrencyCode"),
					exchangeRate: XmlCommon.XmlAttributeNativeDecimal(node, "ExchangeRate"),
					wasLiveRate: XmlCommon.XmlAttributeBool(node, "WasLiveRate"),
					displayLocaleFormat: XmlCommon.XmlAttribute(node, "DisplayLocaleFormat"),
					symbol: XmlCommon.XmlAttribute(node, "Symbol"),
					extensionData: XmlCommon.XmlAttribute(node, "ExtensionData"),
					published: XmlCommon.XmlAttributeBool(node, "Published"),
					displayOrder: XmlCommon.XmlAttributeNativeInt(node, "DisplayOrder"),
					displaySpec: XmlCommon.XmlAttribute(node, "DisplaySpec"),
					lastUpdated: XmlCommon.XmlAttributeNativeDateTime(node, "LastUpdated"),
					createdOn: XmlCommon.XmlAttributeNativeDateTime(node, "CreatedOn"),
					updatedOn: XmlCommon.XmlAttributeNativeDateTime(node, "UpdatedOn")));
		}
	}

	public class CurrencyInfo
	{
		public readonly int CurrencyId;
		public readonly Guid CurrencyGuid;
		public readonly string Name;
		public readonly string CurrencyCode;
		public readonly decimal? ExchangeRate;
		public readonly bool WasLiveRate;
		public readonly string DisplayLocaleFormat;
		public readonly string Symbol;
		public readonly string ExtensionData;
		public readonly bool Published;
		public readonly int DisplayOrder;
		public readonly string DisplaySpec;
		public readonly DateTime LastUpdated;
		public readonly DateTime CreatedOn;
		public readonly DateTime UpdatedOn;

		public CurrencyInfo(
			int currencyId,
			Guid currencyGuid,
			string name,
			string currencyCode,
			decimal? exchangeRate,
			bool wasLiveRate,
			string displayLocaleFormat,
			string symbol,
			string extensionData,
			bool published,
			int displayOrder,
			string displaySpec,
			DateTime lastUpdated,
			DateTime createdOn,
			DateTime updatedOn)
		{
			CurrencyId = currencyId;
			CurrencyGuid = currencyGuid;
			Name = name;
			CurrencyCode = currencyCode;
			ExchangeRate = exchangeRate;
			WasLiveRate = wasLiveRate;
			DisplayLocaleFormat = displayLocaleFormat;
			Symbol = symbol;
			ExtensionData = extensionData;
			Published = published;
			DisplayOrder = displayOrder;
			DisplaySpec = displaySpec;
			LastUpdated = lastUpdated;
			CreatedOn = createdOn;
			UpdatedOn = updatedOn;
		}
	}
}
