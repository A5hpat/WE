// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways;
using AspDotNetStorefrontGateways.Processors;

namespace AspDotNetStorefrontAdmin
{
	public partial class Variant : AspDotNetStorefront.Admin.AdminPageBase
	{
		private int productId;
		private bool ProductTracksInventoryBySizeAndColor;
		private Shipping.ShippingCalculationEnum ShipCalcID = Shipping.ShippingCalculationEnum.Unknown;
		private int skinID = 1;
		private bool UseSpecialRecurringIntervals = false;
		private int variantId;
		protected bool useHtmlEditor;
		private GatewayProcessor _gwActual;
		SkinProvider SkinProvider;

		string selectedLocale;
		String Locale
		{
			get
			{
				if(String.IsNullOrEmpty(selectedLocale))
					selectedLocale = LocaleSelector.GetSelectedLocale().Name;

				return selectedLocale;
			}
		}

		public Variant()
		{
			SkinProvider = new SkinProvider();
		}

		//this is assumed not to be null.
		private GatewayProcessor GWActual
		{
			get
			{
				if(_gwActual == null)
				{
					_gwActual = GatewayLoader.GetProcessor(AppLogic.ActivePaymentGatewayCleaned());
					if(_gwActual == null)
						_gwActual = GatewayLoader.GetProcessor("Manual");
				}
				return _gwActual;
			}
		}
		private List<DateIntervalTypeEnum> RecurringIntervals
		{
			get
			{
				return GWActual.GetAllowedRecurringIntervals();
			}
		}
		private DateIntervalTypeEnum GetSelectedRecurringInterval()
		{
			int selectedInt;
			if(!int.TryParse(ddRecurringIntervalType.SelectedValue, out selectedInt))
				return DateIntervalTypeEnum.Unknown;

			return (DateIntervalTypeEnum)selectedInt;
		}

		protected void DetermineKitItemMapppingVisibility()
		{
			//vID = 0 when new variant created
			if(variantId > 0 && KitProductData.GetCountOfProductsThatHasKitItemsMappedToVariant(variantId) > 0)
			{
				pnlUpdateMappedKitItem.Visible = true;
			}
		}

		protected string BackLink;

		protected void Page_Load(object sender, EventArgs e)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");

			if(!IsPostBack)
			{
				ViewState.Add("VariantEditID", 0);
			}

			SkinProvider = new SkinProvider();

			lblNoDeleteTop.Visible = false;
			lblNoDeleteBottom.Visible = false;

			// Determine HTML editor configuration
			useHtmlEditor = !AppLogic.AppConfigBool("TurnOffHtmlEditorInAdminSite");
			radDescription.Visible = useHtmlEditor;
			txtDescriptionNoHtmlEditor.Visible = !useHtmlEditor;

			UseSpecialRecurringIntervals = AppLogic.UseSpecialRecurringIntervals();
			ShipCalcID = Shipping.GetActiveShippingCalculationID();
			variantId = CommonLogic.QueryStringNativeInt("VariantID");
			productId = CommonLogic.QueryStringNativeInt("ProductID");

			if(productId > 0)
			{
				BackLink = String.Format("variants.aspx?productid={0}", productId);
				lnkManageVariants.NavigateUrl = BackLink;

				ltProduct.Text = "<a href=\"" + AppLogic.AdminLinkUrl("product.aspx") + "?productid=" + productId + "\">" + AppLogic.GetProductName(productId, ThisCustomer.LocaleSetting) + " (" + productId + ")</a>&nbsp;";
			}

			if(Localization.ParseNativeInt(ViewState["VariantEditID"].ToString()) > 0)
			{
				variantId = Localization.ParseNativeInt(ViewState["VariantEditID"].ToString());
			}

			if(!IsPostBack)
			{
				if(ThisCustomer.ThisCustomerSession.Session("entityUserLocale").Length == 0)
				{
					ThisCustomer.ThisCustomerSession.SetVal("entityUserLocale", Localization.GetDefaultLocale());
				}

				ddRecurringIntervalType.Items.Clear();
				//Only display PayFlowPro's supported intervals
				if(UseSpecialRecurringIntervals)
				{
					ddRecurringIntervalType.Items.Add(
							new ListItem(DateIntervalTypeEnum.NumberOfDays.ToString(),
										((int)DateIntervalTypeEnum.NumberOfDays).ToString()));
					ddRecurringIntervalType.Items.Add(
							new ListItem(DateIntervalTypeEnum.Weekly.ToString(),
										  ((int)DateIntervalTypeEnum.Weekly).ToString()));
					ddRecurringIntervalType.Items.Add(
							new ListItem(DateIntervalTypeEnum.BiWeekly.ToString(),
										  ((int)DateIntervalTypeEnum.BiWeekly).ToString()));
					ddRecurringIntervalType.Items.Add(
							new ListItem(DateIntervalTypeEnum.Monthly.ToString(),
										  ((int)DateIntervalTypeEnum.Monthly).ToString()));
					ddRecurringIntervalType.Items.Add(
							new ListItem(DateIntervalTypeEnum.EveryFourWeeks.ToString(),
										  ((int)DateIntervalTypeEnum.EveryFourWeeks).ToString()));
					ddRecurringIntervalType.Items.Add(
							new ListItem(DateIntervalTypeEnum.Quarterly.ToString(),
										  ((int)DateIntervalTypeEnum.Quarterly).ToString()));
					ddRecurringIntervalType.Items.Add(
							new ListItem(DateIntervalTypeEnum.SemiYearly.ToString(),
										  ((int)DateIntervalTypeEnum.SemiYearly).ToString()));
					ddRecurringIntervalType.Items.Add(
							new ListItem(DateIntervalTypeEnum.Yearly.ToString(),
										  ((int)DateIntervalTypeEnum.Yearly).ToString()));
				}
				else
				{
					foreach(DateIntervalTypeEnum intType in GWActual.GetAllowedRecurringIntervals())
						ddRecurringIntervalType.Items.Add(new ListItem(intType.ToString(), ((int)intType).ToString()));
				}

				Product kitCheck = new Product(productId);
				if(kitCheck.IsAKit)    //Hide recurring options for kits
				{
					liRecurringTab.Visible = false;
				}

				LoadContent();
			}
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			btnClose.DataBind();
			btnCloseTop.DataBind();
		}

		protected void LoadContent()
		{
			bool Editing = false;

			using(SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
			{
				dbconn.Open();
				using(IDataReader rs = DB.GetRS("select PV.*,P.ColorOptionPrompt,P.SizeOptionPrompt from Product P  with (NOLOCK)  , productvariant PV   with (NOLOCK)  where PV.ProductID=P.ProductID and VariantID=" + variantId.ToString(), dbconn))
				{
					if(rs.Read())
					{
						Editing = true;
						if(DB.RSFieldBool(rs, "Deleted"))
						{
							btnDeleteVariant.Text = btnDeleteVariantBottom.Text = "UnDelete this Variant";
						}
						else
						{
							btnDeleteVariant.Text = btnDeleteVariantBottom.Text = "Delete this Variant";
						}

						if(0 < DB.GetSqlN("select count(*) N from ShoppingCart  with (NOLOCK)  where VariantID=" + variantId.ToString() + " and CartType=" + ((int)CartTypeEnum.RecurringCart).ToString()))
						{
							btnDeleteVariant.Text = btnDeleteVariantBottom.Text = AppLogic.GetString("admin.common.DeleteNA", skinID, ThisCustomer.LocaleSetting);
							lblNoDeleteTop.Visible = true;
							lblNoDeleteBottom.Visible = true;
							btnDeleteVariant.Enabled = false;
						}
						else
						{
							lblNoDeleteTop.Visible = false;
							lblNoDeleteBottom.Visible = false;
							btnDeleteVariant.Enabled = true;
						}
					}

					ViewState.Add("VariantEdit", Editing);

					bool IsAKit = AppLogic.IsAKit(productId);

					String ColorOptionPrompt = DB.RSFieldByLocale(rs, "ColorOptionPrompt", Locale);
					String SizeOptionPrompt = DB.RSFieldByLocale(rs, "SizeOptionPrompt", Locale);
					if(ColorOptionPrompt.Length == 0)
					{
						ColorOptionPrompt =
								AppLogic.GetString("AppConfig.ColorOptionPrompt", skinID, ThisCustomer.LocaleSetting);
					}
					if(SizeOptionPrompt.Length == 0)
					{
						SizeOptionPrompt = AppLogic.GetString("AppConfig.SizeOptionPrompt", skinID, ThisCustomer.LocaleSetting);
					}

					//SHIPPING
					trShippingCost.Visible = false;
					trShipSeparately.Visible = false;
					if(ShipCalcID == Shipping.ShippingCalculationEnum.UseIndividualItemShippingCosts)
					{
						trShippingCost.Visible = true;
						int counter = 0;
						string temp = "";
						temp += ("<table class=\"table\">");
						using(SqlConnection conn = new SqlConnection(DB.GetDBConn()))
						{
							conn.Open();
							using(IDataReader rs3 = DB.GetRS("select * from ShippingMethod  with (NOLOCK)  where IsRTShipping=0 order by DisplayOrder", conn))
							{
								while(rs3.Read())
								{
									temp += ("<tr>");
									temp += ("<td>");
									temp += (DB.RSFieldByLocale(rs3, "Name", ThisCustomer.LocaleSetting) + ": ");
									temp += ("</td>");
									temp += ("<td>");
									temp += ("<input type=\"text\" class=\"text-sm\" maxLength=\"10\" size=\"10\" name=\"ShippingCost_" + DB.RSFieldInt(rs3, "ShippingMethodID") + "\" value=\"" + CommonLogic.IIF(Editing, Localization.CurrencyStringForDBWithoutExchangeRate(Shipping.GetVariantShippingCost(variantId, DB.RSFieldInt(rs3, "ShippingMethodID"))), "") + "\">  (in format x.xx)\n");
									temp += ("<input type=\"hidden\" name=\"ShippingCost_" + DB.RSFieldInt(rs3, "ShippingMethodID") + "_vldt\" value=\"[number][invalidalert=Please enter a valid dollar amount, e.g. 10.00 without the leading $ sign!]\">\n");
									temp += ("</td>");
									temp += ("</tr>");
									counter++;
								}

							}
						}

						if(counter == 0)
						{
							temp += ("<tr><td>No Shipping Methods Found.  Click <a href=\"shippingmethods.aspx\" >here</a> to set some up.</td></tr>");
						}
						temp += ("</table>");
						ltShippingCost.Text = temp;
						rblShipSeparately.SelectedIndex = 0;
					}
					else
					{
						trShipSeparately.Visible = true;
						rblShipSeparately.SelectedIndex = (DB.RSFieldBool(rs, "IsShipSeparately") ? 1 : 0);
					}

					//INVENTORY
					ProductTracksInventoryBySizeAndColor = AppLogic.ProductTracksInventoryBySizeAndColor(productId);

					var defaultSkinName = SkinProvider.GetSkinNameById(Store.GetDefaultStore().SkinID);

					if(Editing)
					{
						btnDeleteVariant.Visible = btnDeleteVariantBottom.Visible = true;
						ltStatus.Text = "Editing Variant";
						btnSubmit.Text = btnSubmitBottom.Text = "Save";
						btnSubmit.CssClass = btnSubmitBottom.CssClass = "btn btn-primary";

						txtName.Text = XmlCommon.GetLocaleEntry(DB.RSField(rs, "Name"), Locale, false);

						txtSKU.Text = DB.RSField(rs, "SKUSuffix");
						txtManufacturePartNumber.Text = DB.RSField(rs, "ManufacturerPartNumber");
						txtGTIN.Text = DB.RSField(rs, "GTIN");
						if(!DB.RSFieldBool(rs, "Published"))
						{
							rblPublished.BackColor = Color.LightYellow;
						}
						rblPublished.SelectedIndex = (DB.RSFieldBool(rs, "Published") ? 1 : 0);

						rblRecurring.SelectedIndex = (DB.RSFieldBool(rs, "IsRecurring") ? 1 : 0);

						//force multiplier to 1 if interval isn't days. Only days supports multipliers other than 1
						ddRecurringIntervalType.SelectedValue = DB.RSFieldInt(rs, "RecurringIntervalType").ToString();
						if(GetSelectedRecurringInterval() != DateIntervalTypeEnum.NumberOfDays && txtRecurringIntervalMultiplier.Text != "1")
							txtRecurringIntervalMultiplier.Text = "1";
						else
							txtRecurringIntervalMultiplier.Text = DB.RSFieldInt(rs, "RecurringInterval").ToString();

						//DESCRIPTION
						if(useHtmlEditor)
						{
							radDescription.Content = DB.RSFieldByLocale(rs, "Description", Locale);
						}
						else
						{
							txtDescriptionNoHtmlEditor.Text = XmlCommon.GetLocaleEntry(DB.RSField(rs, "Description"), Locale, false);
						}

						//FROOGLE
						txtFroogle.Text = XmlCommon.GetLocaleEntry(DB.RSField(rs, "FroogleDescription"), Locale, false);

						txtRestrictedQuantities.Text = DB.RSField(rs, "RestrictedQuantities");
						txtMinimumQuantity.Text = CommonLogic.IIF(DB.RSFieldInt(rs, "MinimumQuantity") != 0, DB.RSFieldInt(rs, "MinimumQuantity").ToString(), "");

						int NumCustomerLevels = DB.GetSqlN("select count(*) as N from dbo.CustomerLevel cl  with (NOLOCK)  join dbo.ProductCustomerLevel pcl  with (NOLOCK)  on cl.CustomerLevelID = pcl.CustomerLevelID where pcl.ProductID = " + productId.ToString() + " and cl.deleted = 0");
						//PRICE
						txtPrice.Text = Localization.ParseNativeCurrency(DB.RSFieldDecimal(rs, "Price").ToString()).ToString();
						if(NumCustomerLevels > 0)
						{
							ltExtendedPricing.Text = "<a href=\"EditExtendedPrices.aspx?ProductID=" + productId.ToString() + "&VariantID=" + variantId.ToString() + "\">" + "admin.defineextendedprices".StringResource() + "</a>";
						}
						txtSalePrice.Text = CommonLogic.IIF(DB.RSFieldDecimal(rs, "SalePrice") != Decimal.Zero, Localization.ParseNativeCurrency(DB.RSFieldDecimal(rs, "SalePrice").ToString()).ToString(), "");

						trCustomerEntersPrice1.Visible = false;
						trCustomerEntersPricePrompt.Visible = false;
						if(IsAKit)
						{
							rblCustomerEntersPrice.SelectedIndex = 0;
							txtCustomerEntersPricePrompt.Text = "";
						}
						else
						{
							trCustomerEntersPrice1.Visible = true;
							trCustomerEntersPricePrompt.Visible = true;
							rblCustomerEntersPrice.SelectedIndex = (DB.RSFieldBool(rs, "CustomerEntersPrice") ? 1 : 0);
							txtCustomerEntersPricePrompt.Text = XmlCommon.GetLocaleEntry(DB.RSField(rs, "CustomerEntersPricePrompt"), Locale, false);
						}
						txtMSRP.Text = CommonLogic.IIF(DB.RSFieldDecimal(rs, "MSRP") != Decimal.Zero, Localization.ParseNativeCurrency(DB.RSFieldDecimal(rs, "MSRP").ToString()).ToString(), "");
						txtActualCost.Text = CommonLogic.IIF(DB.RSFieldDecimal(rs, "Cost") != Decimal.Zero, Localization.ParseNativeCurrency(DB.RSFieldDecimal(rs, "Cost").ToString()).ToString(), "");

						rblTaxable.SelectedIndex = (DB.RSFieldBool(rs, "IsTaxable") ? 1 : 0);

						rblFreeShipping.SelectedIndex = (DB.RSFieldTinyInt(rs, "FreeShipping"));

						rblDownload.SelectedIndex = (DB.RSFieldBool(rs, "IsDownload") ? 1 : 0);
						rblCondition.SelectedIndex = (DB.RSFieldTinyInt(rs, "Condition"));
						txtDownloadLocation.Text = DB.RSField(rs, "DownloadLocation");
						txtValidForDays.Text = DB.RSFieldInt(rs, "DownloadValidDays") > 0 ? DB.RSFieldInt(rs, "DownloadValidDays").ToString() : string.Empty;
						txtWeight.Text = Localization.ParseNativeDecimal(DB.RSFieldDecimal(rs, "Weight").ToString()).ToString();
						txtWidth.Text = AppLogic.RetrieveProductDimension(DB.RSField(rs, "Dimensions"), "width");
						txtHeight.Text = AppLogic.RetrieveProductDimension(DB.RSField(rs, "Dimensions"), "height");
						txtDepth.Text = AppLogic.RetrieveProductDimension(DB.RSField(rs, "Dimensions"), "depth");

						//INVENTORY
						trCurrentInventory.Visible = false;
						trManageInventory.Visible = false;
						if(ProductTracksInventoryBySizeAndColor)
						{
							trManageInventory.Visible = true;
							ltManageInventory.Text = ("<a href=\"variantsizecolorinventory.aspx?productid=" + productId.ToString() + "&variantid=" + variantId.ToString() + "\">Click Here</a>\n");
						}
						else
						{
							trCurrentInventory.Visible = true;
							CurrentInventory = DB.RSFieldInt(rs, "Inventory");
						}

						// BEGIN IMAGES 
						txtImageOverride.Text = CommonLogic.IIF(Editing, DB.RSField(rs, "ImageFilenameOverride"), "");
						txtSEAlt.Text = CommonLogic.IIF(Editing, DB.RSField(rs, "SEAltText"), "");


						String Image1URL = AppLogic.LookupImage("Variant", variantId, "icon", skinID, ThisCustomer.LocaleSetting);
						if(Image1URL.Length == 0)
						{
							Image1URL = AppLogic.NoPictureImageURL(false, skinID, ThisCustomer.LocaleSetting);
						}
						if(!CommonLogic.FileExists(Image1URL))
						{
							Image1URL = AppLogic.LocateImageURL(string.Format("Skins/{0}/images/nopictureicon.gif", defaultSkinName), ThisCustomer.LocaleSetting);
						}
						if(Image1URL.Length != 0)
						{
							ltIcon.Text = "";
							if(Image1URL.IndexOf("nopicture") ==
								 -1)
							{
								ltIcon.Text = ("<a href=\"javascript:void(0);\" onClick=\"DeleteImage('" + Image1URL + "','Pic1');\">Click here</a> to delete the current image <br/>\n");
							}
							ltIcon.Text += "<img align=\"absmiddle\" style=\"margin-top: 3px; margin-bottom: 5px;\" id=\"Pic1\" name=\"Pic1\" border=\"0\" src=\"" + Image1URL + "?" + CommonLogic.GetRandomNumber(1, 1000000).ToString() + "\" />\n";
						}
						String Image2URL = AppLogic.LookupImage("Variant", variantId, "medium", skinID, ThisCustomer.LocaleSetting);
						if(Image2URL.Length == 0)
						{
							Image2URL = AppLogic.NoPictureImageURL(false, skinID, ThisCustomer.LocaleSetting);
						}
						if(!CommonLogic.FileExists(Image2URL))
						{
							Image2URL = AppLogic.LocateImageURL(String.Format("Skins/{0}/images/nopicture.gif", defaultSkinName), ThisCustomer.LocaleSetting);
						}
						if(Image2URL.Length != 0)
						{
							ltMedium.Text = "";
							if(Image2URL.IndexOf("nopicture") ==
								 -1)
							{
								ltMedium.Text = ("<a href=\"javascript:void(0);\" onClick=\"DeleteImage('" + Image2URL + "','Pic2');\">Click here</a> to delete the current image <br/>\n");
							}
							ltMedium.Text += "<img align=\"absmiddle\" style=\"margin-top: 3px; margin-bottom: 5px;\" id=\"Pic2\" name=\"Pic2\" border=\"0\" src=\"" + Image2URL + "?" + CommonLogic.GetRandomNumber(1, 1000000).ToString() + "\" />\n";
						}
						String Image3URL = AppLogic.LookupImage("Variant", variantId, "large", skinID, ThisCustomer.LocaleSetting);
						if(Image3URL.Length == 0)
						{
							Image3URL = AppLogic.NoPictureImageURL(false, skinID, ThisCustomer.LocaleSetting);
						}
						if(!CommonLogic.FileExists(Image3URL))
						{
							Image3URL = AppLogic.LocateImageURL(String.Format("Skins/{0}/images/nopicture.gif", defaultSkinName), ThisCustomer.LocaleSetting);
						}
						if(Image3URL.Length != 0)
						{
							ltLarge.Text = "";
							if(Image3URL.IndexOf("nopicture") ==
								 -1)
							{
								ltLarge.Text = ("<a href=\"javascript:void(0);\" onClick=\"DeleteImage('" + Image3URL + "','Pic3');\">Click here</a> to delete the current image <br/>\n");
							}
							ltLarge.Text += "<img align=\"absmiddle\" style=\"margin-top: 3px; margin-bottom: 5px;\" id=\"Pic3\" name=\"Pic3\" border=\"0\" src=\"" + Image3URL + "?" + CommonLogic.GetRandomNumber(1, 1000000).ToString() + "\" />\n";
						}
						// END IMAGES   

						// COLORS & SIZES:
						VariantColors = XmlCommon.GetLocaleEntry(Server.HtmlDecode(DB.RSField(rs, "Colors")), Locale, false);
						txtColorSKUModifiers.Text = XmlCommon.GetLocaleEntry(DB.RSField(rs, "ColorSKUModifiers"), Locale, false);
						VariantSizes = XmlCommon.GetLocaleEntry(Server.HtmlDecode(DB.RSField(rs, "Sizes")), Locale, false);
						txtSizeSKUModifiers.Text = XmlCommon.GetLocaleEntry(DB.RSField(rs, "SizeSKUModifiers"), Locale, false);

						trColorSizeSummary.Visible = false;
						// size/color tables for display purposes only:
						if(Editing && (DB.RSField(rs, "Colors").Length != 0 || DB.RSField(rs, "ColorSKUModifiers").Length != 0 || DB.RSField(rs, "Sizes").Length != 0 || DB.RSField(rs, "SizeSKUModifiers").Length != 0))
						{
							var lstColors = new List<Tuple<string, string>>();
							var lstSizes = new List<Tuple<string, string>>();

							//decode special characters for color & size attribute to be displayed on a summary table
							String[] Colors = Server.HtmlDecode(DB.RSFieldByLocale(rs, "Colors", Locale)).Split(',');
							String[] Sizes = Server.HtmlDecode(DB.RSFieldByLocale(rs, "Sizes", Locale)).Split(',');
							String[] ColorSKUModifiers = DB.RSFieldByLocale(rs, "ColorSKUModifiers", Locale).Split(',');
							String[] SizeSKUModifiers = DB.RSFieldByLocale(rs, "SizeSKUModifiers", Locale).Split(',');

							for(int i = Colors.GetLowerBound(0); i <= Colors.GetUpperBound(0); i++)
							{
								Colors[i] = Colors[i].Trim();
							}

							for(int i = Sizes.GetLowerBound(0); i <= Sizes.GetUpperBound(0); i++)
							{
								Sizes[i] = Sizes[i].Trim();
							}

							for(int i = ColorSKUModifiers.GetLowerBound(0); i <= ColorSKUModifiers.GetUpperBound(0); i++)
							{
								ColorSKUModifiers[i] = ColorSKUModifiers[i].Trim();
							}

							for(int i = SizeSKUModifiers.GetLowerBound(0); i <= SizeSKUModifiers.GetUpperBound(0); i++)
							{
								SizeSKUModifiers[i] = SizeSKUModifiers[i].Trim();
							}

							int ColorCols = Colors.GetUpperBound(0);
							int SizeCols = Sizes.GetUpperBound(0);
							ColorCols = Math.Max(ColorCols, ColorSKUModifiers.GetUpperBound(0));
							SizeCols = Math.Max(SizeCols, SizeSKUModifiers.GetUpperBound(0));

							if(ColorCols > 0 || ((ColorCols == 0) && (!string.IsNullOrEmpty(Colors[0]) | !string.IsNullOrEmpty(ColorSKUModifiers[0]))))
							{
								colorSummarySection.Visible = true;
								for(int i = 0; i <= ColorCols; i++)
								{
									string thisColor = string.Empty;
									string thisColorModifier = string.Empty;

									try
									{
										thisColor = Colors[i].Length == 0 ? string.Empty : Server.HtmlEncode(Colors[i]);
									}
									catch
									{
										thisColor = string.Empty;
									}
									try
									{
										thisColorModifier = ColorSKUModifiers[i].Length == 0 ? string.Empty : Server.HtmlEncode(ColorSKUModifiers[i]);
									}
									catch
									{
										thisColorModifier = string.Empty;
									}

									var tplColors = new Tuple<string, string>(Server.HtmlEncode(thisColor), Server.HtmlEncode(thisColorModifier));
									lstColors.Add(tplColors);
								}
							}
							else
							{
								colorSummarySection.Visible = false;
							}

							if(SizeCols > 0 || ((SizeCols == 0) && (!string.IsNullOrEmpty(Sizes[0]) | !string.IsNullOrEmpty(SizeSKUModifiers[0]))))
							{
								sizeSummarySection.Visible = true;
								for(int i = 0; i <= SizeCols; i++)
								{
									string thisSize = string.Empty;
									string thisSizeModifier = string.Empty;

									try
									{
										thisSize = Sizes[i].Length == 0 ? string.Empty : Server.HtmlEncode(Sizes[i]);
									}
									catch
									{
										thisSize = string.Empty;
									}
									try
									{
										thisSizeModifier = SizeSKUModifiers[i].Length == 0 ? string.Empty : Server.HtmlEncode(SizeSKUModifiers[i]);
									}
									catch
									{
										thisSizeModifier = string.Empty;
									}

									var tplSizes = new Tuple<string, string>(Server.HtmlEncode(thisSize), Server.HtmlEncode(thisSizeModifier));
									lstSizes.Add(tplSizes);
								}
							}
							else
							{
								sizeSummarySection.Visible = false;
							}

							rptColors.DataSource = lstColors;
							rptColors.DataBind();
							rptSizes.DataSource = lstSizes;
							rptSizes.DataBind();

							trColorSizeSummary.Visible = true;
						}
					}
					else
					{
						btnDeleteVariant.Visible = btnDeleteVariantBottom.Visible = false;
						ltStatus.Text = "Adding Variant";
						btnSubmit.Text = btnSubmitBottom.Text = "Add New";
						btnSubmit.CssClass = btnSubmitBottom.CssClass = "btn btn-action";

						//INVENTORY
						trCurrentInventory.Visible = false;
						trManageInventory.Visible = false;
						if(ProductTracksInventoryBySizeAndColor)
						{
							trManageInventory.Visible = true;
							ltManageInventory.Text = ("<a href=\"variantsizecolorinventory.aspx?productid=" + productId.ToString() +
													   "&variantid=" + variantId.ToString() + "\">Click Here</a>\n");
						}
						else
						{
							trCurrentInventory.Visible = true;
						}

						//set defaults
						rblCustomerEntersPrice.SelectedIndex = 0;
						rblDownload.SelectedIndex = 0;
						rblCondition.SelectedIndex = 0;
						rblFreeShipping.SelectedIndex = 0;
						rblPublished.SelectedIndex = 1;
						rblRecurring.SelectedIndex = 0;
						ddRecurringIntervalType.SelectedIndex = 2;
						rblShipSeparately.SelectedIndex = 0;
						rblTaxable.SelectedIndex = 1;
					}

				}
			}

			DetermineKitItemMapppingVisibility();
		}

		#region formProperties

		private int CurrentInventory
		{
			get
			{
				try
				{
					return int.Parse(txtCurrentInventory.Text);
				}
				catch
				{
					return 0;
				}
			}
			set
			{
				txtCurrentInventory.Text = value.ToString();
			}
		}

		#endregion


		protected void LocaleSelector_SelectedLocaleChanged(object sender, EventArgs e)
		{
			LoadContent();
		}

		protected void btnSubmit_Click(object sender, EventArgs e)
		{
			UpdateVariant();
		}

		protected void btnSaveAndClose_Click(object sender, EventArgs e)
		{
			UpdateVariant();
			Response.Redirect(ReturnUrlTracker.GetReturnUrl());
		}

		protected Boolean RecurringIntervalIsValid()
		{
			if(rblRecurring.SelectedValue == "0")
				return true;

			//only Days supports a recurring interval multiplier other than 1
			if(GetSelectedRecurringInterval() != DateIntervalTypeEnum.NumberOfDays && txtRecurringIntervalMultiplier.Text != "1")
			{
				var payflowError = new StringBuilder();
				payflowError.Append("PayFlowPro only supports a recurring interval other than 1 for Days interval.");
				ctrlAlertMessage.PushAlertMessage(payflowError.ToString(), AlertMessage.AlertType.Danger);
				return false;
			}

			bool intervalmatches = RecurringIntervals.Contains(GetSelectedRecurringInterval());
			IntervalValidator iv = GWActual.GetIntervalValidator(GetSelectedRecurringInterval());
			ValidatorResult vr = iv.Validate(txtRecurringIntervalMultiplier.Text);

			if(intervalmatches && vr.IsValid)
				return true;

			var error = new StringBuilder();
			if(!intervalmatches)
				error.Append("The " + GWActual.DisplayName(ThisCustomer.LocaleSetting) + " gateway does not support the selected interval. Please choose a supported interval.<br />");
			if(!vr.IsValid)
				error.Append("There was an error with the recurring interval that you entered: " + vr.Message);

			ctrlAlertMessage.PushAlertMessage(error.ToString(), AlertMessage.AlertType.Danger);

			return false;
		}

		protected Boolean UpdateVariant()
		{
			if(!RecurringIntervalIsValid())
				return false;

			bool Editing = Localization.ParseBoolean(ViewState["VariantEdit"].ToString());
			StringBuilder sql = new StringBuilder();

			decimal Price = Decimal.Zero;
			decimal SalePrice = Decimal.Zero;
			decimal MSRP = Decimal.Zero;
			decimal Cost = Decimal.Zero;
			int Points = 0;
			int MinimumQuantity = 0;
			if(txtPrice.Text.Length != 0)
			{
				Price = Localization.ParseNativeDecimal(txtPrice.Text);
			}
			if(txtSalePrice.Text.Length != 0)
			{
				SalePrice = Localization.ParseNativeDecimal(txtSalePrice.Text);
			}

			if(txtMSRP.Text.Length != 0)
			{
				MSRP = Localization.ParseNativeDecimal(txtMSRP.Text);
			}
			if(txtActualCost.Text.Length != 0)
			{
				Cost = Localization.ParseNativeDecimal(txtActualCost.Text);
			}

			if(txtMinimumQuantity.Text.Length != 0)
			{
				MinimumQuantity = Localization.ParseNativeInt(txtMinimumQuantity.Text);
			}

			bool IsFirstVariantAdded = (DB.GetSqlN("select count(VariantID) as N from ProductVariant  with (NOLOCK)  where ProductID=" + productId.ToString() + " and Deleted=0") == 0);

			if(!Editing)
			{
				// ok to ADD them:
				String NewGUID = DB.GetNewGUID();
				sql.Append("insert into productvariant(VariantGUID,ProductID,Name,ImageFilenameOverride,SEAltText,IsDefault,Description,RestrictedQuantities,FroogleDescription,Price,SalePrice,MSRP,Cost,Points,MinimumQuantity,SKUSuffix,ManufacturerPartNumber,GTIN,Weight,Dimensions,Inventory,Published,CustomerEntersPrice,CustomerEntersPricePrompt,IsRecurring,RecurringInterval,RecurringIntervalType,Colors,ColorSKUModifiers,Sizes,SizeSKUModifiers,IsTaxable,IsShipSeparately,IsDownload,FreeShipping,DownloadLocation,DownloadValidDays,Condition) values(");
				sql.Append(DB.SQuote(NewGUID) + ",");
				sql.Append(productId.ToString() + ",");
				sql.Append(DB.SQuote(AppLogic.FormLocaleXmlVariant("Name", txtName.Text, Locale, variantId)) + ",");
				sql.Append(DB.SQuote(txtImageOverride.Text) + ",");
				if(txtSEAlt.Text.Length != 0)
				{
					sql.Append(DB.SQuote(txtSEAlt.Text) + ",");
				}
				else
				{
					sql.Append(DB.SQuote(txtName.Text) + ",");
				}
				if(IsFirstVariantAdded)
				{
					sql.Append("1,"); // IsDefault=1
				}
				else
				{
					sql.Append("0,"); // IsDefault=0
				}
				string temp = string.Empty;
				if(useHtmlEditor)
				{
					temp = AppLogic.FormLocaleXmlVariant("Description", radDescription.Content, Locale, variantId);
				}
				else
				{
					temp = AppLogic.FormLocaleXmlEditorVariant("Description", txtDescriptionNoHtmlEditor.Text.Trim(), Locale, variantId);
				}
				if(temp.Length != 0)
				{
					sql.Append(DB.SQuote(temp) + ",");
				}
				else
				{
					sql.Append("NULL,");
				}
				if(txtRestrictedQuantities.Text.Length != 0)
				{
					sql.Append(DB.SQuote(txtRestrictedQuantities.Text) + ",");
				}
				else
				{
					sql.Append("NULL,");
				}
				temp =
						AppLogic.FormLocaleXmlVariant("FroogleDescription", txtFroogle.Text, Locale, variantId);
				if(temp.Length != 0)
				{
					sql.Append(DB.SQuote(temp) + ",");
				}
				else
				{
					sql.Append("NULL,");
				}
				sql.Append(Localization.DecimalStringForDB(Price) + ",");
				sql.Append(CommonLogic.IIF(SalePrice != Decimal.Zero, Localization.DecimalStringForDB(SalePrice), "NULL") + ",");
				sql.Append(CommonLogic.IIF(MSRP != Decimal.Zero, Localization.DecimalStringForDB(MSRP), "NULL") + ",");
				sql.Append(CommonLogic.IIF(Cost != Decimal.Zero, Localization.DecimalStringForDB(Cost), "NULL") + ",");
				sql.Append(Localization.IntStringForDB(Points) + ",");
				sql.Append(CommonLogic.IIF(MinimumQuantity != 0, Localization.IntStringForDB(MinimumQuantity), "NULL") + ",");

				if(txtSKU.Text.Length != 0)
					sql.Append(DB.SQuote(txtSKU.Text) + ",");
				else
					sql.Append("NULL,");

				if(txtManufacturePartNumber.Text.Length != 0)
					sql.Append(DB.SQuote(txtManufacturePartNumber.Text) + ",");
				else
					sql.Append("NULL,");

				if(txtGTIN.Text.Length != 0)
					sql.Append(DB.SQuote(txtGTIN.Text) + ",");
				else
					sql.Append("NULL,");

				decimal Weight = Localization.ParseNativeDecimal(txtWeight.Text);
				sql.Append(CommonLogic.IIF(Weight != 0.0M, Localization.DecimalStringForDB(Weight), "NULL") + ",");
				if(txtWidth.Text.Length != 0 && txtHeight.Text.Length != 0 && txtDepth.Text.Length != 0)
				{
					sql.Append(DB.SQuote(string.Format("{0}x{1}x{2}", Convert.ToDecimal(txtWidth.Text.Trim()), Convert.ToDecimal(txtHeight.Text.Trim()), Convert.ToDecimal(txtDepth.Text.Trim()))) + ",");
				}
				else
				{
					sql.Append("NULL,");
				}
				sql.Append(CurrentInventory.ToString() + ",");
				sql.Append(rblPublished.SelectedValue + ",");
				sql.Append(rblCustomerEntersPrice.SelectedValue + ",");
				sql.Append(DB.SQuote(AppLogic.FormLocaleXmlVariant("CustomerEntersPricePrompt", txtCustomerEntersPricePrompt.Text, Locale, variantId)) + ",");
				sql.Append(rblRecurring.SelectedValue + ",");
				sql.Append(Localization.ParseNativeInt(txtRecurringIntervalMultiplier.Text).ToString() + ",");
				sql.Append(ddRecurringIntervalType.SelectedValue + ",");
				sql.Append(DB.SQuote(AppLogic.FormLocaleXmlVariant("Colors", VariantColors, Locale, variantId).Replace(", ", ",").Replace(" ,", ",").Replace("'", "").Trim()) + ",");
				sql.Append(DB.SQuote(txtColorSKUModifiers.Text.Replace(", ", ",").Replace(" ,", ",").Trim()) + ",");
				sql.Append(DB.SQuote(AppLogic.FormLocaleXmlVariant("Sizes", VariantSizes, Locale, variantId).Replace(", ", ",").Replace(" ,", ",").Replace("'", "").Trim()) + ",");
				sql.Append(DB.SQuote(txtSizeSKUModifiers.Text.Replace(", ", ",").Replace(" ,", ",").Trim()) + ",");
				sql.Append(rblTaxable.SelectedValue + ",");
				sql.Append(rblShipSeparately.SelectedValue + ",");
				sql.Append(rblDownload.SelectedValue + ",");
				sql.Append(rblFreeShipping.SelectedValue + ",");
				String DLoc = txtDownloadLocation.Text;
				if(DLoc.StartsWith("/"))
				{
					DLoc = DLoc.Substring(1, DLoc.Length - 1); // remove leading / char!
				}
				sql.Append(DB.SQuote(DLoc) + ",");

				int validDays = 0;
				sql.Append((int.TryParse(txtValidForDays.Text, out validDays) ? validDays.ToString() : "NULL") + ",");
				sql.Append(rblCondition.SelectedValue);
				sql.Append(")");
				try
				{
					DB.ExecuteSQL(sql.ToString());
				}
				catch(Exception ex)
				{
					throw new ArgumentException("Error in EditVariant(.RunSql), Msg=[" + CommonLogic.GetExceptionDetail(ex, String.Empty) + "], Sql=[" + sql.ToString() + "]");
				}

				//Get variantID for editing
				using(SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
				{
					dbconn.Open();
					using(IDataReader rs = DB.GetRS("select VariantID from productvariant with (NOLOCK) where deleted=0 and VariantGUID=" + DB.SQuote(NewGUID), dbconn))
					{
						rs.Read();
						variantId = DB.RSFieldInt(rs, "VariantID");
						ViewState.Add("VariantEdit", true);
						ViewState.Add("VariantEditID", variantId);

					}
				}
				ctrlAlertMessage.PushAlertMessage("Variant Added.", AlertMessage.AlertType.Success);
			}
			else
			{
				// ok to update:
				sql.Append("update productvariant set ");
				sql.Append("ProductID=" + productId.ToString() + ",");
				sql.Append("Name=" + DB.SQuote(AppLogic.FormLocaleXmlVariant("Name", txtName.Text, Locale, variantId)) + ",");
				sql.Append("ImageFilenameOverride=" + DB.SQuote(txtImageOverride.Text) + ",");
				sql.Append("SEAltText=" + DB.SQuote(txtSEAlt.Text) + ",");
				string temp = string.Empty;
				if(useHtmlEditor)
				{
					temp = AppLogic.FormLocaleXmlVariant("Description", radDescription.Content, Locale, variantId);
				}
				else
				{
					temp = AppLogic.FormLocaleXmlVariant("Description", txtDescriptionNoHtmlEditor.Text.Trim(), Locale, variantId);
				}
				if(temp.Length != 0)
				{
					sql.Append("Description=" + DB.SQuote(temp) + ",");
				}
				else
				{
					sql.Append("Description=NULL,");
				}
				if(txtRestrictedQuantities.Text.Length != 0)
				{
					sql.Append("RestrictedQuantities=" + DB.SQuote(txtRestrictedQuantities.Text) + ",");
				}
				else
				{
					sql.Append("RestrictedQuantities=NULL,");
				}
				temp =
						AppLogic.FormLocaleXmlVariant("FroogleDescription", txtFroogle.Text, Locale, variantId);
				if(temp.Length != 0)
				{
					sql.Append("FroogleDescription=" + DB.SQuote(temp) + ",");
				}
				else
				{
					sql.Append("FroogleDescription=NULL,");
				}
				sql.Append("Price=" + Localization.DecimalStringForDB(Price) + ",");
				sql.Append("SalePrice=" + CommonLogic.IIF(SalePrice != Decimal.Zero, Localization.DecimalStringForDB(SalePrice), "NULL") + ",");
				sql.Append("MSRP=" + CommonLogic.IIF(MSRP != Decimal.Zero, Localization.DecimalStringForDB(MSRP), "NULL") + ",");
				sql.Append("Cost=" + CommonLogic.IIF(Cost != Decimal.Zero, Localization.DecimalStringForDB(Cost), "NULL") + ",");
				sql.Append("Points=" + Localization.IntStringForDB(Points) + ",");
				sql.Append("MinimumQuantity=" + CommonLogic.IIF(MinimumQuantity != 0, Localization.IntStringForDB(MinimumQuantity), "NULL") + ",");

				if(txtSKU.Text.Length != 0)
					sql.Append("SKUSuffix=" + DB.SQuote(txtSKU.Text) + ",");
				else
					sql.Append("SKUSuffix=NULL,");

				if(txtManufacturePartNumber.Text.Length != 0)
					sql.Append("ManufacturerPartNumber=" + DB.SQuote(txtManufacturePartNumber.Text) + ",");
				else
					sql.Append("ManufacturerPartNumber=NULL,");

				if(txtGTIN.Text.Length != 0)
					sql.Append("GTIN=" + DB.SQuote(txtGTIN.Text) + ",");
				else
					sql.Append("GTIN=NULL,");



				decimal Weight = Localization.ParseNativeDecimal(txtWeight.Text);
				sql.Append("Weight=" +
							CommonLogic.IIF(Weight != 0.0M, Localization.DecimalStringForDB(Weight), "NULL") + ",");
				if(txtWidth.Text.Length != 0 && txtHeight.Text.Length != 0 && txtDepth.Text.Length != 0)
				{
					sql.Append("Dimensions=" + DB.SQuote(string.Format("{0}x{1}x{2}", Convert.ToDecimal(txtWidth.Text.Trim()), Convert.ToDecimal(txtHeight.Text.Trim()), Convert.ToDecimal(txtDepth.Text.Trim()))) + ",");
				}
				else
				{
					sql.Append("Dimensions=NULL,");
				}
				sql.Append("Inventory=" + CurrentInventory.ToString() + ",");
				sql.Append("Published=" + rblPublished.SelectedValue + ",");
				sql.Append("CustomerEntersPrice=" + rblCustomerEntersPrice.SelectedValue + ",");
				sql.Append("CustomerEntersPricePrompt=" + DB.SQuote(AppLogic.FormLocaleXmlVariant("CustomerEntersPricePrompt", txtCustomerEntersPricePrompt.Text, Locale, variantId)) + ",");
				sql.Append("IsRecurring=" + rblRecurring.SelectedValue + ",");
				sql.Append("RecurringInterval=" + Localization.ParseNativeInt(txtRecurringIntervalMultiplier.Text).ToString() + ",");
				sql.Append("RecurringIntervalType=" + ddRecurringIntervalType.SelectedValue + ",");
				sql.Append("Colors=" + DB.SQuote(AppLogic.FormLocaleXmlVariant("Colors",
					XmlCommon.GetLocaleEntry(VariantColors, Locale, false),
					Locale, variantId).Replace(", ", ",").Replace(" ,", ",").Replace("'", "").Trim()) + ",");
				sql.Append("ColorSKUModifiers=" + DB.SQuote(XmlCommon.GetLocaleEntry(txtColorSKUModifiers.Text, Locale, false).Replace(", ", ",").Replace(" ,", ",").Trim()) + ",");
				sql.Append("Sizes=" + DB.SQuote(AppLogic.FormLocaleXmlVariant("Sizes",
					XmlCommon.GetLocaleEntry(VariantSizes, Locale, false),
					Locale, variantId).Replace(", ", ",").Replace(" ,", ",").Replace("'", "").Trim()) + ",");
				sql.Append("SizeSKUModifiers=" + DB.SQuote(XmlCommon.GetLocaleEntry(txtSizeSKUModifiers.Text, Locale, false).Replace(", ", ",").Replace(" ,", ",").Trim()) + ",");
				sql.Append("IsTaxable=" + rblTaxable.SelectedValue + ",");
				sql.Append("IsShipSeparately=" + rblShipSeparately.SelectedValue + ",");
				sql.Append("IsDownload=" + rblDownload.SelectedValue + ",");
				sql.Append("FreeShipping=" + rblFreeShipping.SelectedValue + ",");
				String DLoc = txtDownloadLocation.Text;
				if(DLoc.StartsWith("/"))
				{
					DLoc = DLoc.Substring(1, DLoc.Length - 1); // remove leading / char!
				}
				sql.Append("DownloadLocation=" + DB.SQuote(DLoc) + ",");

				int validDays = 0;
				sql.Append(string.Format("DownloadValidDays={0},", (int.TryParse(txtValidForDays.Text, out validDays) ? validDays.ToString() : "NULL")));

				sql.Append("Condition=" + rblCondition.SelectedValue);
				sql.Append(" where VariantID=" + variantId.ToString());
				try
				{
					DB.ExecuteSQL(sql.ToString());
				}
				catch(Exception ex)
				{
					throw new ArgumentException("Error in EditVariant(.RunSql), Msg=[" + CommonLogic.GetExceptionDetail(ex, String.Empty) + "], Sql=[" + sql.ToString() + "]");
				}

				ViewState.Add("VariantEdit", true);
				ViewState.Add("VariantEditID", variantId);

				ctrlAlertMessage.PushAlertMessage("Variant Updated.", AlertMessage.AlertType.Success);
			}

			// handle shipping costs uploaded (if any):
			if(ShipCalcID == Shipping.ShippingCalculationEnum.UseIndividualItemShippingCosts)
			{
				DB.ExecuteSQL("delete from ShippingByProduct where VariantID=" + variantId.ToString());

				using(SqlConnection conn = new SqlConnection(DB.GetDBConn()))
				{
					conn.Open();
					using(IDataReader rs3 = DB.GetRS("select * from ShippingMethod  with (NOLOCK)  where IsRTShipping=0 order by DisplayOrder", conn))
					{
						while(rs3.Read())
						{
							String FldName = "ShippingCost_" + DB.RSFieldInt(rs3, "ShippingMethodID");
							decimal ShippingCost = CommonLogic.FormUSDecimal(FldName);
							DB.ExecuteSQL("insert ShippingByProduct(VariantID,ShippingMethodID,ShippingCost) values(" + variantId.ToString() + "," + DB.RSFieldInt(rs3, "ShippingMethodID").ToString() + "," + Localization.CurrencyStringForDBWithoutExchangeRate(ShippingCost) + ")");
						}
					}
				}
			}

			//Upload Images
			HandleImageSubmits();

			LoadContent();
			return true;
		}

		public void HandleImageSubmits()
		{
			// handle image uploaded:
			String FN = CommonLogic.FormCanBeDangerousContent("ImageFilenameOverride").Trim();
			if(FN.Length == 0)
			{
				FN = variantId.ToString();
			}

			String Image1 = String.Empty;
			String TempImage1 = String.Empty;
			HttpPostedFile Image1File = fuIcon.PostedFile;
			if(Image1File != null &&
				 Image1File.ContentLength != 0)
			{
				// delete any current image file first
				try
				{
					if(FN.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase) ||
						 FN.EndsWith(".gif", StringComparison.InvariantCultureIgnoreCase) ||
						 FN.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase))
					{
						File.Delete(AppLogic.GetImagePath("Product", "icon", true) + FN);
					}
					else
					{
						File.Delete(AppLogic.GetImagePath("Variant", "icon", true) + FN + ".jpg");
						File.Delete(AppLogic.GetImagePath("Variant", "icon", true) + FN + ".gif");
						File.Delete(AppLogic.GetImagePath("Variant", "icon", true) + FN + ".png");
					}
				}
				catch { }

				switch(Image1File.ContentType)
				{
					case "image/gif":

						TempImage1 = AppLogic.GetImagePath("Variant", "icon", true) + "tmp_" + FN + ".gif";
						Image1 = AppLogic.GetImagePath("Variant", "icon", true) + FN + ".gif";
						Image1File.SaveAs(TempImage1);
						ImageResize.ResizeEntityOrObject("Variant", TempImage1, Image1, "icon", "image/gif");

						break;
					case "image/x-png":
					case "image/png":

						TempImage1 = AppLogic.GetImagePath("Variant", "icon", true) + "tmp_" + FN + ".png";
						Image1 = AppLogic.GetImagePath("Variant", "icon", true) + FN + ".png";
						Image1File.SaveAs(TempImage1);
						ImageResize.ResizeEntityOrObject("Variant", TempImage1, Image1, "icon", "image/png");

						break;
					case "image/jpg":
					case "image/jpeg":
					case "image/pjpeg":

						TempImage1 = AppLogic.GetImagePath("Variant", "icon", true) + "tmp_" + FN + ".jpg";
						Image1 = AppLogic.GetImagePath("Variant", "icon", true) + FN + ".jpg";
						Image1File.SaveAs(TempImage1);
						ImageResize.ResizeEntityOrObject("Variant", TempImage1, Image1, "icon", "image/jpeg");

						break;
				}
				ImageResize.DisposeOfTempImage(TempImage1);
			}

			String Image2 = String.Empty;
			String TempImage2 = String.Empty;
			HttpPostedFile Image2File = fuMedium.PostedFile;
			if(Image2File != null &&
				 Image2File.ContentLength != 0)
			{
				// delete any current image file first
				try
				{
					File.Delete(AppLogic.GetImagePath("Variant", "medium", true) + FN + ".jpg");
					File.Delete(AppLogic.GetImagePath("Variant", "medium", true) + FN + ".gif");
					File.Delete(AppLogic.GetImagePath("Variant", "medium", true) + FN + ".png");
				}
				catch { }

				switch(Image2File.ContentType)
				{
					case "image/gif":

						TempImage2 = AppLogic.GetImagePath("Variant", "medium", true) + "tmp_" + FN + ".gif";
						Image2 = AppLogic.GetImagePath("Variant", "medium", true) + FN + ".gif";
						Image2File.SaveAs(TempImage2);
						ImageResize.ResizeEntityOrObject("Variant", TempImage2, Image2, "medium", "image/gif");

						break;
					case "image/x-png":
					case "image/png":

						TempImage2 = AppLogic.GetImagePath("Variant", "medium", true) + "tmp_" + FN + ".png";
						Image2 = AppLogic.GetImagePath("Variant", "medium", true) + FN + ".png";
						Image2File.SaveAs(TempImage2);
						ImageResize.ResizeEntityOrObject("Variant", TempImage2, Image2, "medium", "image/png");

						break;
					case "image/jpg":
					case "image/jpeg":
					case "image/pjpeg":

						TempImage2 = AppLogic.GetImagePath("Variant", "medium", true) + "tmp_" + FN + ".jpg";
						Image2 = AppLogic.GetImagePath("Variant", "medium", true) + FN + ".jpg";
						Image2File.SaveAs(TempImage2);
						ImageResize.ResizeEntityOrObject("Variant", TempImage2, Image2, "medium", "image/jpeg");

						break;
				}
				ImageResize.DisposeOfTempImage(TempImage2);
			}

			String Image3 = String.Empty;
			String TempImage3 = String.Empty;
			HttpPostedFile Image3File = fuLarge.PostedFile;
			if(Image3File != null &&
				 Image3File.ContentLength != 0)
			{
				// delete any current image file first
				try
				{
					File.Delete(AppLogic.GetImagePath("Variant", "large", true) + variantId.ToString() + ".jpg");
					File.Delete(AppLogic.GetImagePath("Variant", "large", true) + variantId.ToString() + ".gif");
					File.Delete(AppLogic.GetImagePath("Variant", "large", true) + variantId.ToString() + ".png");
				}
				catch { }

				switch(Image3File.ContentType)
				{
					case "image/gif":

						TempImage3 = AppLogic.GetImagePath("Variant", "large", true) + "tmp_" + FN + ".gif";
						Image3 = AppLogic.GetImagePath("Variant", "large", true) + FN + ".gif";
						Image3File.SaveAs(TempImage3);
						ImageResize.ResizeEntityOrObject("Variant", TempImage3, Image3, "large", "image/gif");
						ImageResize.CreateOthersFromLarge("Variant", TempImage3, FN, "image/gif");

						break;
					case "image/x-png":
					case "image/png":

						TempImage3 = AppLogic.GetImagePath("Variant", "large", true) + "tmp_" + FN + ".png";
						Image3 = AppLogic.GetImagePath("Variant", "large", true) + FN + ".png";
						Image3File.SaveAs(TempImage3);
						ImageResize.ResizeEntityOrObject("Variant", TempImage3, Image3, "large", "image/png");
						ImageResize.CreateOthersFromLarge("Variant", TempImage3, FN, "image/png");

						break;
					case "image/jpg":
					case "image/jpeg":
					case "image/pjpeg":

						TempImage3 = AppLogic.GetImagePath("Variant", "large", true) + "tmp_" + FN + ".jpg";
						Image3 = AppLogic.GetImagePath("Variant", "large", true) + FN + ".jpg";
						Image3File.SaveAs(TempImage3);
						ImageResize.ResizeEntityOrObject("Variant", TempImage3, Image3, "large", "image/jpeg");
						ImageResize.CreateOthersFromLarge("Variant", TempImage3, FN, "image/jpeg");

						break;
				}
			}
		}

		protected void btnDeleteVariant_Click(object sender, EventArgs e)
		{
			string sql = "if (select count(*) from dbo.ProductVariant where ProductID = " + productId.ToString() +
						 " and VariantID <> " + variantId.ToString() + " and deleted = 0 ) = 0 \n";
			sql += "select 'This is the only Variant for this product and cannot be deleted' \n";
			sql += "else begin \n";
			sql +=
					"update dbo.ProductVariant set Deleted = case deleted when 1 then 0 else 1 end, isdefault = 0 where VariantID = " +
					variantId.ToString() + "\n";
			sql += " if exists (select * from dbo.ProductVariant where ProductID = " + productId.ToString() +
				   " and published = 1 and deleted = 0 and isdefault = 1 ) declare @t tinyint \n";
			sql +=
					" else update dbo.ProductVariant set isdefault = 1 where VariantID = (select top 1 VariantID from dbo.ProductVariant where ProductID = " +
					productId.ToString() + " and published = 1 and deleted = 0)\n";
			sql += "select '' \n";
			sql += "end";


			string err = string.Empty;

			using(SqlConnection conn = new SqlConnection(DB.GetDBConn()))
			{
				conn.Open();
				using(IDataReader dr = DB.GetRS(sql, conn))
				{
					dr.Read();
					err = dr.GetString(0);
				}
			}

			ctrlAlertMessage.PushAlertMessage(err, AlertMessage.AlertType.Danger);
			LoadContent();
		}

		#region "field Properties"

		private System.Text.RegularExpressions.Regex rxForbiddenCharacters =
			new System.Text.RegularExpressions.Regex("[&#<>#@]");

		/// <summary>
		/// txtColor field with prohibited characters removed
		/// </summary>
		private string VariantColors
		{
			get
			{
				return rxForbiddenCharacters.Replace(txtColors.Text, " ");
			}
			set
			{
				txtColors.Text = rxForbiddenCharacters.Replace(value, " ");
			}
		}
		/// <summary>
		/// txtSizes field with prohibited characters removed
		/// </summary>
		private string VariantSizes
		{
			get
			{
				return rxForbiddenCharacters.Replace(txtSizes.Text, " ");
			}
			set
			{
				txtSizes.Text = rxForbiddenCharacters.Replace(value, " ");
			}
		}

		#endregion
	}
}
