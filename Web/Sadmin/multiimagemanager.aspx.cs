// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	/// <summary>
	/// Summary description for productimagemgr
	/// </summary>
	public partial class MultiImageManager : AspDotNetStorefront.Admin.AdminPageBase
	{
		public int ProductID;
		public int VariantID;
		public string TheSize;
		private int currentSkinID = 1;
		public string ProductName;

		protected void Page_Load(object sender, System.EventArgs e)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");

			ProductID = CommonLogic.QueryStringUSInt("ProductID");
			VariantID = CommonLogic.QueryStringUSInt("VariantID");
			TheSize = CommonLogic.QueryStringCanBeDangerousContent("Size");

			if(TheSize.Length == 0)
			{
				TheSize = "medium";
			}
			if(VariantID == 0)
			{
				VariantID = AppLogic.GetDefaultProductVariant(ProductID);
			}

			if(CommonLogic.FormBool("IsSubmit"))
			{
				var FN = ProductID.ToString();
				if(AppLogic.AppConfigBool("UseSKUForProductImageName"))
				{
					using(var dbconn = new SqlConnection(DB.GetDBConn()))
					{
						dbconn.Open();
						using(var rs = DB.GetRS("select SKU from product   with (NOLOCK)  where productid=" + ProductID.ToString(), dbconn))
						{

							if(rs.Read())
							{
								var SKU = DB.RSField(rs, "SKU").Trim();
								if(SKU.Length != 0)
								{
									FN = SKU;
								}
							}
						}

					}

				}
				try
				{
					for(int i = 0; i <= Request.Form.Count - 1; i++)
					{
						String FieldName = Request.Form.Keys[i];
						if(FieldName.IndexOf("Key_") != -1)
						{
							String KeyVal = CommonLogic.FormCanBeDangerousContent(FieldName);
							// this field should be processed
							String[] KeyValSplit = KeyVal.Split('|');
							int TheFieldID = Localization.ParseUSInt(KeyValSplit[0]);
							int TheProductID = Localization.ParseUSInt(KeyValSplit[1]);
							int TheVariantID = Localization.ParseUSInt(KeyValSplit[2]);
							String ImageNumber = AppLogic.CleanSizeColorOption(KeyValSplit[3]);
							String Color = AppLogic.CleanSizeColorOption(HttpContext.Current.Server.UrlDecode(KeyValSplit[4]));
							String SafeColor = CommonLogic.MakeSafeFilesystemName(Color);
							bool DeleteIt = (CommonLogic.FormCanBeDangerousContent("Delete_" + TheFieldID.ToString()).Length != 0);
							if(DeleteIt)
							{
								System.IO.File.Delete(AppLogic.GetImagePath("Product", TheSize, true) + FN + "_" + ImageNumber.ToLowerInvariant() + "_" + SafeColor + ".jpg");
								System.IO.File.Delete(AppLogic.GetImagePath("Product", TheSize, true) + FN + "_" + ImageNumber.ToLowerInvariant() + "_" + SafeColor + ".gif");
								System.IO.File.Delete(AppLogic.GetImagePath("Product", TheSize, true) + FN + "_" + ImageNumber.ToLowerInvariant() + "_" + SafeColor + ".png");
								System.IO.File.Delete(AppLogic.GetImagePath("Product", "micro", true) + FN + "_" + ImageNumber.ToLowerInvariant() + ".jpg");

							}

							String Image2 = String.Empty;
							String TempImage2 = String.Empty;
							String ContentType = String.Empty;
							HttpPostedFile Image2File = Request.Files["Image" + TheFieldID.ToString()];
							if(Image2File.ContentLength != 0)
							{
								// delete any current image file first
								try
								{
									System.IO.File.Delete(AppLogic.GetImagePath("Product", TheSize, true) + FN + "_" + ImageNumber.ToLowerInvariant() + "_" + SafeColor + ".jpg");
									System.IO.File.Delete(AppLogic.GetImagePath("Product", TheSize, true) + FN + "_" + ImageNumber.ToLowerInvariant() + "_" + SafeColor + ".gif");
									System.IO.File.Delete(AppLogic.GetImagePath("Product", TheSize, true) + FN + "_" + ImageNumber.ToLowerInvariant() + "_" + SafeColor + ".png");
								}
								catch
								{ }

								String s = Image2File.ContentType;
								switch(Image2File.ContentType)
								{
									case "image/gif":
										TempImage2 = AppLogic.GetImagePath("Product", TheSize, true) + "tmp_" + FN + "_" + ImageNumber.ToLowerInvariant() + "_" + SafeColor + ".gif";
										Image2 = AppLogic.GetImagePath("Product", TheSize, true) + FN + "_" + ImageNumber.ToLowerInvariant() + "_" + SafeColor + ".gif";
										Image2File.SaveAs(TempImage2);
										ImageResize.ResizeEntityOrObject("Product", TempImage2, Image2, TheSize, "image/gif");
										ContentType = "image/gif";
										break;
									case "image/x-png":
									case "image/png":
										TempImage2 = AppLogic.GetImagePath("Product", TheSize, true) + "tmp_" + FN + "_" + ImageNumber.ToLowerInvariant() + "_" + SafeColor + ".png";
										Image2 = AppLogic.GetImagePath("Product", TheSize, true) + FN + "_" + ImageNumber.ToLowerInvariant() + "_" + SafeColor + ".png";
										Image2File.SaveAs(TempImage2);
										ImageResize.ResizeEntityOrObject("Product", TempImage2, Image2, TheSize, "image/png");
										ContentType = "image/png";
										break;
									case "image/jpg":
									case "image/jpeg":
									case "image/pjpeg":
										TempImage2 = AppLogic.GetImagePath("Product", TheSize, true) + "tmp_" + FN + "_" + ImageNumber.ToLowerInvariant() + "_" + SafeColor + ".jpg";
										Image2 = AppLogic.GetImagePath("Product", TheSize, true) + FN + "_" + ImageNumber.ToLowerInvariant() + "_" + SafeColor + ".jpg";
										Image2File.SaveAs(TempImage2);
										ImageResize.ResizeEntityOrObject("Product", TempImage2, Image2, TheSize, "image/jpeg");
										ContentType = "image/jpeg";
										break;
								}


								// lets try and create the other multi images if using the large multi image manager
								if(TheSize == "large")
								{
									ImageResize.MakeOtherMultis(FN, ImageNumber, SafeColor, TempImage2, ContentType);
								}
								else if(AppLogic.AppConfigBool("MultiMakesMicros") && TheSize == "medium" && SafeColor == "")
								{
									// lets create micro images if using the medium multi image manager
									// since the medium icons are what show on the product pages
									ImageResize.MakeMicroPic(FN, TempImage2, ImageNumber);
								}

								// delete the temp image
								ImageResize.DisposeOfTempImage(TempImage2);

							}
						}
					}
					ctrlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.common.ItemUpdated", SkinID, LocaleSetting), AlertMessage.AlertType.Success);
				}
				catch(Exception ex)
				{
					ctrlAlertMessage.PushAlertMessage(CommonLogic.GetExceptionDetail(ex, "<br/>"), AlertMessage.AlertType.Error);
				}
				String variantColors = String.Empty;

				using(var dbconn = new SqlConnection(DB.GetDBConn()))
				{
					dbconn.Open();
					using(var rsColors = DB.GetRS("select Colors from productvariant   with (NOLOCK)  where VariantID=" + VariantID.ToString(), dbconn))
					{
						if(rsColors.Read())
						{
							variantColors = DB.RSFieldByLocale(rsColors, "Colors", Localization.GetDefaultLocale());
						}
					}
				}
			}
			this.LoadData();
		}

		protected void LoadData()
		{
			string temp = "";

			using(var dbconn = new SqlConnection(DB.GetDBConn()))
			{
				dbconn.Open();
				using(var rs = DB.GetRS("select * from productvariant   with (NOLOCK)  where VariantID=" + VariantID.ToString(), dbconn))
				{
					if(!rs.Read())
					{
						Response.Redirect("splash.aspx"); // should not happen, but...
					}

					ProductName = AppLogic.GetProductName(ProductID, ThisCustomer.LocaleSetting);
					String ProductSKU = AppLogic.GetProductSKU(ProductID);
					String VariantName = AppLogic.GetVariantName(VariantID, ThisCustomer.LocaleSetting);
					String VariantSKU = AppLogic.GetVariantSKUSuffix(VariantID);

					String ImageNumbers = "1,2,3,4,5,6,7,8,9,10";
					String Colors = "," + DB.RSFieldByLocale(rs, "Colors", Localization.GetDefaultLocale()); // add an "empty" color to the first entry, to allow an image to be specified for "no color selected"
					String[] ColorsSplit = Colors.Split(',');
					String[] ImageNumbersSplit = ImageNumbers.Split(',');

					temp += ("<table class=\"table\" \">\n");
					temp += ("<tr>\n");
					temp += ("<td valign=\"middle\" align=\"right\"><b>" + AppLogic.GetString("admin.multiimage.ColorImage", SkinID, LocaleSetting) + "</b></td>\n");
					for(int i = ImageNumbersSplit.GetLowerBound(0); i <= ImageNumbersSplit.GetUpperBound(0); i++)
					{
						temp += ("<td valign=\"middle\" align=\"center\"><b>" + AppLogic.CleanSizeColorOption(ImageNumbersSplit[i]) + "</b></td>\n");
					}
					temp += ("</tr>\n");
					int FormFieldID = 1000; // arbitrary number
					bool first = true;
					for(int i = ColorsSplit.GetLowerBound(0); i <= ColorsSplit.GetUpperBound(0); i++)
					{
						if(ColorsSplit[i].Length == 0 && !first)
						{
							continue;
						}
						temp += ("<tr>\n");
						temp += ("<td valign=\"middle\" align=\"right\"><b>" + CommonLogic.IIF(ColorsSplit[i].Length == 0, AppLogic.GetString("admin.multiimage.NoColorSelected", SkinID, LocaleSetting), AppLogic.CleanSizeColorOption(ColorsSplit[i])) + "</b></td>\n");
						for(int j = ImageNumbersSplit.GetLowerBound(0); j <= ImageNumbersSplit.GetUpperBound(0); j++)
						{
							temp += ("<td valign=\"bottom\" align=\"center\" bgcolor=\"#EEEEEE\">");
							int ImgWidth = AppLogic.AppConfigNativeInt("Admin.MultiGalleryImageWidth");
							temp += ("<img " + CommonLogic.IIF(ImgWidth != 0, "width=\"" + ImgWidth.ToString() + "\"", "") + " src=\"" + AppLogic.LookupProductImageByNumberAndColor(ProductID, currentSkinID, ThisCustomer.LocaleSetting, Localization.ParseUSInt(ImageNumbersSplit[j]), AppLogic.CleanSizeColorOption(ColorsSplit[i]), TheSize) + "\"><br/>");
							temp += ("<input style=\"font-size: 9px;\" type=\"file\" name=\"Image" + FormFieldID.ToString() + "\" size=\"24\" value=\"\"><br/>\n");
							temp += ("<input type=\"checkbox\" name=\"Delete_" + FormFieldID.ToString() + "\"> <small>" + AppLogic.GetString("admin.common.delete", SkinID, LocaleSetting) + "</small>");
							String sColorValue = HttpContext.Current.Server.UrlEncode(AppLogic.CleanSizeColorOption(ColorsSplit[i]));
							temp += ("<input type=\"hidden\" name=\"Key_" + FormFieldID.ToString() + "\" value=\"" + FormFieldID.ToString() + "|" + ProductID.ToString() + "|" + VariantID.ToString() + "|" + AppLogic.CleanSizeColorOption(ImageNumbersSplit[j]) + "|" + sColorValue + "\">");
							FormFieldID++;
							temp += ("</td>\n");
						}
						temp += ("</tr>\n");
						first = false;
					}

					temp += ("</table>\n");
				}
			}
			ltContent.Text = temp;
		}

	}
}
