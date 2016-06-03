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
using System.Text;
using System.Web;
using System.Web.Mvc;
using AspDotNetStorefront.Routing;

namespace AspDotNetStorefrontCore
{
	/// <summary>
	/// Summary description for ProductImageGallery.
	/// </summary>
	public class ProductImageGallery
	{

		private int m_ProductID;
		private int m_VariantID;
		private int m_SkinID;
		private String m_LocaleSetting;
		private String m_ProductSKU;
		private int m_MaxImageIndex; // will be 0 if empty
		private String m_Colors;
		private String[] m_ColorsSplit;
		private String m_ImageNumbers = "1,2,3,4,5,6,7,8,9,10";
		private String[] m_ImageNumbersSplit;
		private String m_ImgGalIcons;
		private String m_ImgDHTML;
		private String[,] m_ImageUrlsicon;
		private String[,] m_ImageUrlsmedium;
		private String[,] m_ImageUrlslarge;
		private bool m_HasSomeLarge;
		readonly SkinProvider SkinProvider;

		public ProductImageGallery()
		{
			SkinProvider = new SkinProvider();
		}

		public ProductImageGallery(int ProductID, int SkinID, String LocaleSetting, string SKU)
			: this()
		{
			m_ProductID = ProductID;
			m_VariantID = AppLogic.GetDefaultProductVariant(m_ProductID);
			m_SkinID = SkinID;
			m_LocaleSetting = LocaleSetting;
			m_MaxImageIndex = 0;
			m_Colors = String.Empty;
			m_ImgGalIcons = String.Empty;
			m_ImgDHTML = String.Empty;
			m_HasSomeLarge = false;
			m_ProductSKU = SKU;
			LoadFromDB();
		}

		public String ImgDHTML
		{
			get
			{
				return m_ImgDHTML;
			}
			set
			{
				m_ImgDHTML = value;
			}
		}

		public String ImgGalIcons
		{
			get
			{
				return m_ImgGalIcons;
			}
			set
			{
				m_ImgGalIcons = value;
			}
		}

		public bool HasSomeLarge
		{
			get
			{
				return m_HasSomeLarge;
			}
		}

		public bool IsEmpty()
		{
			return m_MaxImageIndex == 0;
		}

		public void LoadFromDB()
		{
			var suffix = "_" + m_ProductID.ToString();
			m_ImageNumbersSplit = m_ImageNumbers.Split(',');
			var m_WatermarksEnabled = AppLogic.AppConfigBool("Watermark.Enabled");
			var urlHelper = DependencyResolver.Current.GetService<UrlHelper>();

			m_ColorsSplit = new String[1] { "" };
			if(m_Colors == string.Empty)
			{
				using(var dbconn = new SqlConnection(DB.GetDBConn()))
				{
					dbconn.Open();
					using(var rs = DB.GetRS("select Colors from productvariant   with (NOLOCK)  where VariantID=" + m_VariantID.ToString(), dbconn))
					{
						if(rs.Read())
						{
							m_Colors = DB.RSFieldByLocale(rs, "Colors", Localization.GetDefaultLocale()); // remember to add "empty" color to front, for no color selected
							if(m_Colors.Length != 0)
							{
								m_ColorsSplit = ("," + m_Colors).Split(',');
							}
						}
					}

				}

			}
			else
			{
				m_ColorsSplit = ("," + m_Colors).Split(',');
			}
			if(m_Colors.Length != 0)
			{
				for(int i = m_ColorsSplit.GetLowerBound(0); i <= m_ColorsSplit.GetUpperBound(0); i++)
				{
					String s2 = AppLogic.RemoveAttributePriceModifier(m_ColorsSplit[i]);
					m_ColorsSplit[i] = CommonLogic.MakeSafeFilesystemName(s2);
				}
			}

			if(AppLogic.AppConfigBool("MultiImage.UseProductIconPics"))
			{
				m_ImageUrlsicon = new String[m_ImageNumbersSplit.Length, m_ColorsSplit.Length];
				for(int x = m_ImageNumbersSplit.GetLowerBound(0); x <= m_ImageNumbersSplit.GetUpperBound(0); x++)
				{
					int ImgIdx = Localization.ParseUSInt(m_ImageNumbersSplit[x]);
					for(int i = m_ColorsSplit.GetLowerBound(0); i <= m_ColorsSplit.GetUpperBound(0); i++)
					{
						String Url = string.Empty;
						if(m_ProductSKU == string.Empty)
						{
							Url = AppLogic.LookupProductImageByNumberAndColor(m_ProductID, m_SkinID, m_LocaleSetting, ImgIdx, AppLogic.RemoveAttributePriceModifier(m_ColorsSplit[i]), "icon");
						}
						else
						{
							Url = AppLogic.LookupProductImageByNumberAndColor(m_ProductID, m_SkinID, m_ProductSKU, m_LocaleSetting, ImgIdx, AppLogic.RemoveAttributePriceModifier(m_ColorsSplit[i]), "icon");
						}
						if(m_WatermarksEnabled && Url.Length != 0 && Url.IndexOf("nopicture") == -1)
						{
							if (Url.StartsWith("/"))
							{
								m_ImageUrlsicon[x, i] = Url.Substring(HttpContext.Current.Request.ApplicationPath.Length);
							}
							else
							{
								m_ImageUrlsicon[x, i] = Url.Substring(HttpContext.Current.Request.ApplicationPath.Length - 1);
							}
							
							if(m_ImageUrlsicon[x, i].StartsWith("/"))
							{
								m_ImageUrlsicon[x, i] = m_ImageUrlsicon[x, i].TrimStart('/');
							}
						}
						else
						{
							m_ImageUrlsicon[x, i] = Url;
						}
					}
				}
				for(int x = m_ImageNumbersSplit.GetLowerBound(0); x <= m_ImageNumbersSplit.GetUpperBound(0); x++)
				{
					int ImgIdx = Localization.ParseUSInt(m_ImageNumbersSplit[x]);
					if(m_ImageUrlsicon[x, 0].IndexOf("nopicture") == -1)
					{
						m_MaxImageIndex = ImgIdx;
					}
				}
			}

			m_ImageUrlsmedium = new String[m_ImageNumbersSplit.Length, m_ColorsSplit.Length];
			for(int j = m_ImageNumbersSplit.GetLowerBound(0); j <= m_ImageNumbersSplit.GetUpperBound(0); j++)
			{
				int ImgIdx = Localization.ParseUSInt(m_ImageNumbersSplit[j]);
				for(int i = m_ColorsSplit.GetLowerBound(0); i <= m_ColorsSplit.GetUpperBound(0); i++)
				{
					if(m_ProductSKU == string.Empty)
					{
						m_ImageUrlsmedium[j, i] = AppLogic.LookupProductImageByNumberAndColor(m_ProductID, m_SkinID, m_LocaleSetting, ImgIdx, AppLogic.RemoveAttributePriceModifier(m_ColorsSplit[i]), "medium");
					}
					else
					{
						m_ImageUrlsmedium[j, i] = AppLogic.LookupProductImageByNumberAndColor(m_ProductID, m_SkinID, m_ProductSKU, m_LocaleSetting, ImgIdx, AppLogic.RemoveAttributePriceModifier(m_ColorsSplit[i]), "medium");
					}
				}
			}
			for(int j = m_ImageNumbersSplit.GetLowerBound(0); j <= m_ImageNumbersSplit.GetUpperBound(0); j++)
			{
				int ImgIdx = Localization.ParseUSInt(m_ImageNumbersSplit[j]);
				if(m_ImageUrlsmedium[j, 0].IndexOf("nopicture") == -1)
				{
					m_MaxImageIndex = ImgIdx;
				}
			}

			m_ImageUrlslarge = new String[m_ImageNumbersSplit.Length, m_ColorsSplit.Length];
			for(int j = m_ImageNumbersSplit.GetLowerBound(0); j <= m_ImageNumbersSplit.GetUpperBound(0); j++)
			{
				int ImgIdx = Localization.ParseUSInt(m_ImageNumbersSplit[j]);
				for(int i = m_ColorsSplit.GetLowerBound(0); i <= m_ColorsSplit.GetUpperBound(0); i++)
				{
					String Url = string.Empty;
					if(m_ProductSKU == string.Empty)
					{
						Url = AppLogic.LookupProductImageByNumberAndColor(m_ProductID, m_SkinID, m_LocaleSetting, ImgIdx, AppLogic.RemoveAttributePriceModifier(m_ColorsSplit[i]), "large");
					}
					else
					{
						Url = AppLogic.LookupProductImageByNumberAndColor(m_ProductID, m_SkinID, m_ProductSKU, m_LocaleSetting, ImgIdx, AppLogic.RemoveAttributePriceModifier(m_ColorsSplit[i]), "large");
					}					

					if(m_WatermarksEnabled && Url.Length != 0 && Url.IndexOf("nopicture") == -1)
					{
						if(Url.StartsWith("/"))
						{
							m_ImageUrlslarge[j, i] = Url.Substring(HttpContext.Current.Request.ApplicationPath.Length);
						}
						else
						{
							m_ImageUrlslarge[j, i] = Url.Substring(HttpContext.Current.Request.ApplicationPath.Length - 1);
						}

						if(m_ImageUrlslarge[j, i].StartsWith("/"))
						{
							m_ImageUrlslarge[j, i] = m_ImageUrlslarge[j, i].TrimStart('/');
						}

						m_HasSomeLarge = true;
					}
					else if(Url.Length == 0 || Url.IndexOf("nopicture") != -1)
					{
						m_ImageUrlslarge[j, i] = String.Empty;
					}
					else
					{
						m_HasSomeLarge = true;
						m_ImageUrlslarge[j, i] = Url;
					}					
				}
			}

			if(!IsEmpty())
			{
				StringBuilder tmpS = new StringBuilder(4096);
				tmpS.Append("<script type=\"text/javascript\">\n");
				tmpS.Append("var ProductPicIndex" + suffix + " = 1;\n");
				tmpS.Append("var ProductColor" + suffix + " = '';\n");
				tmpS.Append("var boardpics" + suffix + " = new Array();\n");
				tmpS.Append("var boardpicslg" + suffix + " = new Array();\n");
				tmpS.Append("var boardpicslgwidth" + suffix + " = new Array();\n");
				tmpS.Append("var boardpicslgheight" + suffix + " = new Array();\n");
				
				for(int i = 1; i <= m_MaxImageIndex; i++)
				{
					foreach(String c in m_ColorsSplit)
					{
						String MdUrl = ImageUrl(i, c, "medium").ToLowerInvariant();
						String MdWatermarkedUrl = MdUrl;

						if(m_WatermarksEnabled)
						{
							if(MdUrl.Length > 0)
							{
								string[] split = MdUrl.Split('/');
								string lastPart = split.Last();
								MdUrl = AppLogic.LocateImageURL(lastPart, "PRODUCT", "medium", "");
							} 
						}

						tmpS.Append("boardpics" + suffix + "['" + i.ToString() + "," + c + "'] = '" + MdWatermarkedUrl + "';\n");

						String LgUrl = ImageUrl(i, c, "large").ToLowerInvariant();
						String LgWatermarkedUrl = LgUrl;

						if(m_WatermarksEnabled)
						{
							if(LgUrl.Length > 0)
							{
								string[] split = LgUrl.Split('/');
								string lastPart = split.Last();
								LgUrl = AppLogic.LocateImageURL(lastPart, "PRODUCT", "large", "");
							}
						}

							tmpS.Append("boardpicslg" + suffix + "['" + i.ToString() + "," + c + "'] = '" + LgWatermarkedUrl + "';\n");

						if(LgUrl.Length > 0)
						{
							System.Drawing.Size lgsz = CommonLogic.GetImagePixelSize(LgUrl);
							tmpS.Append("boardpicslgwidth" + suffix + "['" + i.ToString() + "," + c + "'] = '" + lgsz.Width.ToString() + "';\n");
							tmpS.Append("boardpicslgheight" + suffix + "['" + i.ToString() + "," + c + "'] = '" + lgsz.Height.ToString() + "';\n");
						}
					}
				}

				tmpS.Append("function changecolorimg" + suffix + "()\n");
				tmpS.Append("{\n");
				tmpS.Append("	var scidx = ProductPicIndex" + suffix + " + ',' + ProductColor" + suffix + ".toLowerCase();\n");
				
				tmpS.Append("	document.ProductPic" + m_ProductID.ToString() + ".src=boardpics" + suffix + "[scidx];\n");

				tmpS.Append("}\n");

				tmpS.Append("function popuplarge" + suffix + "()\n");
				tmpS.Append("{\n");
				tmpS.Append("	var scidx = ProductPicIndex" + suffix + " + ',' + ProductColor" + suffix + ".toLowerCase();\n");
				tmpS.Append("	var LargeSrc = encodeURIComponent(boardpicslg" + suffix + "[scidx]);\n");
				tmpS.Append("if(boardpicslg" + suffix + "[scidx] != '')\n");
				tmpS.Append("{\n");
				var popupUrl = urlHelper.Action(ActionNames.PopUp, ControllerNames.Image);
				tmpS.Append("	window.open('" + popupUrl + "?" + RouteDataKeys.ImagePath + "=' + LargeSrc,'LargerImage" + CommonLogic.GetRandomNumber(1, 100000) + "','toolbar=no,location=no,directories=no,status=no,menubar=no,scrollbars=" + CommonLogic.IIF(AppLogic.AppConfigBool("ResizableLargeImagePopup"), "yes", "no") + ",resizable=" + CommonLogic.IIF(AppLogic.AppConfigBool("ResizableLargeImagePopup"), "yes", "no") + ",copyhistory=no,width=' + boardpicslgwidth" + suffix + "[scidx] + ',height=' + boardpicslgheight" + suffix + "[scidx] + ',left=0,top=0');\n");
				tmpS.Append("}\n");
				tmpS.Append("else\n");
				tmpS.Append("{\n");
				tmpS.Append("	alert('There is no large image available for this picture');\n");
				tmpS.Append("}\n");
				tmpS.Append("}\n");

				tmpS.Append("function setcolorpicidx" + suffix + "(idx)\n");
				tmpS.Append("{\n");
				tmpS.Append("	ProductPicIndex" + suffix + " = idx;\n");
				tmpS.Append("	changecolorimg" + suffix + "();\n");
				tmpS.Append("}\n");
				
				tmpS.Append("function setActive(element)\n");
				tmpS.Append("{\n");
				tmpS.Append("	adnsf$('li.page-link').removeClass('active');\n");
				tmpS.Append("	adnsf$(element).parent().addClass('active');\n");
				tmpS.Append("}\n");

				tmpS.Append("function cleansizecoloroption" + suffix + "(theVal)\n");
				tmpS.Append("{\n");
				tmpS.Append("   if(theVal.indexOf('[') != -1){theVal = theVal.substring(0, theVal.indexOf('['))}");
				tmpS.Append("	theVal = theVal.replace(/[\\W]/g,\"\");\n");
				tmpS.Append("	theVal = theVal.toLowerCase();\n");
				tmpS.Append("	return theVal;\n");
				tmpS.Append("}\n");

				tmpS.Append("function setcolorpic" + suffix + "(color)\n");
				tmpS.Append("{\n");

				tmpS.Append("	while(color != unescape(color))\n");
				tmpS.Append("	{\n");
				tmpS.Append("		color = unescape(color);\n");
				tmpS.Append("	}\n");

				tmpS.Append("	if(color == '-,-' || color == '-')\n");
				tmpS.Append("	{\n");
				tmpS.Append("		color = '';\n");
				tmpS.Append("	}\n");

				tmpS.Append("	if(color != '' && color.indexOf(',') != -1)\n");
				tmpS.Append("	{\n");

				tmpS.Append("		color = color.substring(0,color.indexOf(',')).replace(new RegExp(\"'\", 'gi'), '');\n"); // remove sku from color select value

				tmpS.Append("	}\n");
				tmpS.Append("	if(color != '' && color.indexOf('[') != -1)\n");
				tmpS.Append("	{\n");

				tmpS.Append("	    color = color.substring(0,color.indexOf('[')).replace(new RegExp(\"'\", 'gi'), '');\n");
				tmpS.Append("		color = color.replace(/[\\s]+$/g,\"\");\n");

				tmpS.Append("	}\n");
				tmpS.Append("	ProductColor" + suffix + " = cleansizecoloroption" + suffix + "(color);\n");

				tmpS.Append("	changecolorimg" + suffix + "();\n");
				tmpS.Append("	return (true);\n");
				tmpS.Append("}\n");

				tmpS.Append("</script>\n");
				m_ImgDHTML = tmpS.ToString();

				bool useMicros = AppLogic.AppConfigBool("UseImagesForMultiNav");

				bool microAction = CommonLogic.IIF(AppLogic.AppConfigBool("UseRolloverForMultiNav"), true, false);

				if(m_MaxImageIndex > 1)
				{
					tmpS.Remove(0, tmpS.Length);

					if(!AppLogic.AppConfigBool("MultiImage.UseProductIconPics") && !useMicros)
					{
						tmpS.Append("<ul class=\"pagination image-paging\">");
						for(int i = 1; i <= m_MaxImageIndex; i++)
						{
							if(i == 1)
								tmpS.Append("<li class=\"page-link active\">");
							else
								tmpS.Append("<li class=\"page-link\">");

							tmpS.Append(string.Format("<a href=\"javascript:void(0);\" onclick='setcolorpicidx{0}({1});setActive(this);' class=\"page-number\">{1}</a>", suffix, i));
							tmpS.Append("</li>");
						}
						tmpS.Append("</ul>");
					}
					else
					{
						tmpS.Append("<div class=\"product-gallery-items\">");
						for(int i = 1; i <= m_MaxImageIndex; i++)
						{
							tmpS.Append("<div class=\"product-gallery-item\">");
							tmpS.Append("	<div class=\"gallery-item-inner\">");

							var imageUrl = GetImageUrl(
								size: AppLogic.AppConfigBool("MultiImage.UseProductIconPics") ? "icon" : "micro", 
								identifier: AppLogic.AppConfigBool("UseSKUForProductImageName") ? m_ProductSKU : m_ProductID.ToString(),
								index: i);

							// if not using rollover to change the images
							if(!microAction && imageUrl.Length > 0)
							{
								var strImageTag = string.Format("<img class='product-gallery-image' onclick='setcolorpicidx{0}({1});' alt='Show Picture {1}' src='{2}' border='0' />",
									suffix,
									i,
									imageUrl
								);
								tmpS.Append(strImageTag);
							}
							else if(imageUrl.Length > 0)
							{
								var strImageTag = string.Format("<img class='product-gallery-image' onMouseOver='setcolorpicidx{0}({1});' alt='Show Picture {1}' src='{2}' border='0' />",
									suffix,
									i,
									imageUrl
								);
								tmpS.Append(strImageTag);
							}
							tmpS.Append("	</div>");
							tmpS.Append("</div>");
						}
						tmpS.Append("</div>");
					}

					m_ImgGalIcons = tmpS.ToString();
				}
			}
		}

		string GetImageUrl(string size, string identifier, int index)
		{
			var locale = HttpContext.Current.GetCustomer().LocaleSetting;

			var imageUrl = AppLogic.LocateImageURL(string.Format("{0}_{1}.gif", identifier, index), "product", size, locale);
			if(!CommonLogic.FileExists(imageUrl))
				imageUrl = AppLogic.LocateImageURL(string.Format("{0}_{1}_.gif", identifier, index), "product", size, locale);
			if(!CommonLogic.FileExists(imageUrl))
				imageUrl = AppLogic.LocateImageURL(string.Format("{0}_{1}.jpg", identifier, index), "product", size, locale);
			if(!CommonLogic.FileExists(imageUrl))
				imageUrl = AppLogic.LocateImageURL(string.Format("{0}_{1}_.jpg", identifier, index), "product", size, locale);
			if(!CommonLogic.FileExists(imageUrl))
				imageUrl = AppLogic.LocateImageURL(string.Format("{0}_{1}.png", identifier, index), "product", size, locale);
			if(!CommonLogic.FileExists(imageUrl))
				imageUrl = AppLogic.LocateImageURL(string.Format("{0}_{1}_.png", identifier, index), "product", size, locale);
			if(!CommonLogic.FileExists(imageUrl))
			{
				if(StringComparer.OrdinalIgnoreCase.Equals(size, "large") || StringComparer.OrdinalIgnoreCase.Equals(size, "medium"))
					imageUrl = AppLogic.LocateImageURL(string.Format("skins/{0}/images/nopicture.gif", SkinProvider.GetSkinNameById(m_SkinID)));
				else
					imageUrl = AppLogic.LocateImageURL(string.Format("skins/{0}/images/nopicture{1}.gif", SkinProvider.GetSkinNameById(m_SkinID), size));
			}
			return imageUrl;
		}

		public int MaxImageIndex
		{
			get
			{
				return m_MaxImageIndex;
			}
		}

		private int GetColorIndex(String Color)
		{
			int i = 0;
			foreach(String s in m_ColorsSplit)
			{
				if(s == Color)
				{
					return i;
				}
				i++;
			}
			return 0;
		}

		public String ImageUrl(int Index, String Color, String ImgSize)
		{
			String s = ImgSize.ToLower(CultureInfo.InstalledUICulture);
			try
			{
				if(s == "icon")
				{
					return String.Empty;
				}
				else if(s == "medium")
				{
					return m_ImageUrlsmedium[Index - 1, GetColorIndex(Color)].Replace("//", "/");
				}
				else if(s == "large")
				{
					return m_ImageUrlslarge[Index - 1, GetColorIndex(Color)].Replace("//", "/"); ;
				}
				return String.Empty;
			}
			catch
			{
				return String.Empty;
			}
		}

	}

	public enum ProductImageSize
	{
		micro,
		icon,
		medium,
		large
	}
}
