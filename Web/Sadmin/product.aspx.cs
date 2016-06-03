// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class ProductEditor : AspDotNetStorefront.Admin.AdminPageBase
	{
		protected int ProductId;

		readonly XmlPackageManager XmlPackageManager;
		readonly ISearchEngineNameProvider SearchEngineNameProvider;
		int EntityId;
		string EntityName;
		EntityHelper Entity;
		EntitySpecs EntitySpecs;
		int CurrentSkinID = 1;
		bool UseHtmlEditor;
		SkinProvider SkinProvider;

		string selectedLocale;
		String Locale
		{
			get
			{
				if (String.IsNullOrEmpty(selectedLocale))
					selectedLocale = LocaleSelector.GetSelectedLocale().Name;

				return selectedLocale;
			}
		}

		public ProductEditor()
		{
			XmlPackageManager = new XmlPackageManager();
			SearchEngineNameProvider = DependencyResolver.Current.GetService<ISearchEngineNameProvider>();
			SkinProvider = new SkinProvider();
		}

		protected override void OnPreRender(EventArgs e)
		{
			if(QuickAddManufacturer.EntityName != null)
				LoadManufacturers(QuickAddManufacturer.EntityId);

			if(QuickAddDistributor.EntityName != null)
				LoadDistributors(QuickAddDistributor.EntityId);

			if(QuickAddQuantityDiscounts.QuantityDiscountName != null)
				LoadDiscountTables(QuickAddQuantityDiscounts.QuantityDiscountId);

			if(QuickAddCustomerLevel.CustomerLevelName != null)
				LoadCustomerLevels(QuickAddCustomerLevel.CustomerLevelId);

			DataBind();

			base.OnPreRender(e);
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");

			if(AppLogic.MaxProductsExceeded())
			{
				ctrlAlertMessage.PushAlertMessage("WARNING: Maximum number of allowed products exceeded. To add additional products, please delete some products or upgrade AspDotNetStoreFront", AlertMessage.AlertType.Danger);
			}

			if(!IsPostBack)
			{
				ViewState.Add("ProductEditID", 0);
			}

			// Determine HTML editor configuration
			UseHtmlEditor = !AppLogic.AppConfigBool("TurnOffHtmlEditorInAdminSite");
			radSummary.Visible = radDescription.Visible = UseHtmlEditor;
			txtSummaryNoHtmlEditor.Visible = txtDescriptionNoHtmlEditor.Visible =  !UseHtmlEditor;

			ProductId = CommonLogic.QueryStringNativeInt("productid");
			if((int?)ViewState["ProductEditID"] > 0)
				ProductId = (int)ViewState["ProductEditID"];

			EntityId = CommonLogic.QueryStringNativeInt("EntityFilterID") == 0
				? CommonLogic.QueryStringNativeInt("EntityID")
				: CommonLogic.QueryStringNativeInt("EntityFilterID");

			EntityName = CommonLogic.QueryStringCanBeDangerousContent("EntityName").ToUpperInvariant();
			switch(EntityName)
			{
				case "SECTION":
					Entity = AppLogic.SectionStoreEntityHelper[0];
					break;
				case "MANUFACTURER":
					Entity = AppLogic.ManufacturerStoreEntityHelper[0];
					break;
				case "DISTRIBUTOR":
					Entity = AppLogic.DistributorStoreEntityHelper[0];
					break;
				case "LIBRARY":
					Entity = AppLogic.LibraryStoreEntityHelper[0];
					break;
				default:
					Entity = AppLogic.CategoryStoreEntityHelper[0];
					EntityName = "Category";
					break;
			}
			EntitySpecs = EntityDefinitions.LookupSpecs(EntityName);

			etsMapper.ObjectID = ProductId;
			if(!IsPostBack)
			{
				liStoreMappingTab.Visible = etsMapper.StoreCount > 1;
				btnDeleteAll.Attributes.Add("onclick", "return confirm('Confirm Delete');");
				LoadContent();
			}

			//Hide the kit option if the product had recurring variants, so they can't make a recurring kit product
			List<ProductVariant> variantsToCheck = new List<ProductVariant>(ProductVariant.GetVariants(ProductId, false));
			trKit.Visible = variantsToCheck.Count(pv => pv.IsRecurring == true) < 1;
		}

		protected void LoadContent()
		{
			ddDistributor.Items.Clear();
			ddDiscountTable.Items.Clear();
			ddManufacturer.Items.Clear();
			ddOnSalePrompt.Items.Clear();
			ddType.Items.Clear();
			ddTaxClass.Items.Clear();

			bool ProductTracksInventoryBySizeAndColor = AppLogic.ProductTracksInventoryBySizeAndColor(ProductId);
			bool IsAKit = AppLogic.IsAKit(ProductId);
			Editing = false;

			if(IsAKit)
			{
				ProductTracksInventoryBySizeAndColor = false;
			}

			using(var dbconn = new SqlConnection(DB.GetDBConn()))
			{
				dbconn.Open();
				using(var rs = DB.GetRS("select * from Product   with (NOLOCK)  where ProductID=" + ProductId.ToString(), dbconn))
				{
					if(rs.Read())
					{
						Editing = true;
						if(!DB.RSFieldBool(rs, "Published"))
						{
							lblPublished.CssClass = "text-danger";
							lblPublished.Font.Bold = true;
						}
						else
						{
							lblPublished.CssClass = "";
							lblPublished.Font.Bold = false;
						}
					}

					Title = HeaderText.Text = Editing ? "admin.common.editproduct".StringResource() : "admin.products.AddNew".StringResource();


					ViewState.Add("ProductEdit", Editing);

					//load Product Types
					ddType.Items.Add(new ListItem(" - Select -", "0"));

					string query = string.Empty;
					query = "select * from ProductType   with (NOLOCK)  order by DisplayOrder,Name";

					using(var conn = new SqlConnection(DB.GetDBConn()))
					{
						conn.Open();

						using(var rsst = DB.GetRS(query, conn))
						{
							while(rsst.Read())
							{
								ddType.Items.Add(new ListItem(DB.RSFieldByLocale(rsst, "Name", Locale), DB.RSFieldInt(rsst, "ProductTypeID").ToString()));
							}
						}
					}

					LoadManufacturers();
					LoadDistributors();
					LoadDiscountTables();

					//load Tax Class
					if(CommonLogic.IsStringNullOrEmpty(AppLogic.AppConfig("Admin_DefaultTaxClassID")))
					{
						ddTaxClass.Items.Add(new ListItem("None", "0"));
					}

					using(var conn = new SqlConnection(DB.GetDBConn()))
					{
						conn.Open();
						using(var rsst = DB.GetRS("select * from TaxClass   with (NOLOCK)  order by DisplayOrder,Name", conn))
						{
							while(rsst.Read())
							{
								ddTaxClass.Items.Add(new ListItem(DB.RSFieldByLocale(rsst, "Name", Locale), DB.RSFieldInt(rsst, "TaxClassID").ToString()));
							}
						}

					}

					if(ddTaxClass.Items.FindByValue(AppLogic.AppConfig("Admin_DefaultTaxClassID")) != null)
					{
						ddTaxClass.SelectedValue = AppLogic.AppConfig("Admin_DefaultTaxClassID");
					}

					//On Sale prompt
					ddOnSalePrompt.Items.Add(new ListItem(" - Select -", "0"));

					using(var conn = new SqlConnection(DB.GetDBConn()))
					{
						conn.Open();
						using(var rsst = DB.GetRS("select * from salesprompt   with (NOLOCK)  where deleted=0", conn))
						{
							while(rsst.Read())
							{
								ddOnSalePrompt.Items.Add(new ListItem(DB.RSFieldByLocale(rsst, "Name", Locale), DB.RSFieldInt(rsst, "SalesPromptID").ToString()));
							}
						}
					}

					//MAPPINGS
					cblAffiliates.Items.Clear();
					cblGenres.Items.Clear();
					cblVectors.Items.Clear();

					//items.clear handled in methods
					LoadCategories();
					LoadDepartments();
					LoadCustomerLevels();

					EntityHelper eTemp = new EntityHelper(0, EntityDefinitions.LookupSpecs("Section"), false, 0);
					ArrayList al = eTemp.GetEntityArrayList(0, "", 0, Locale, false);
					eTemp = new EntityHelper(EntityDefinitions.readonly_AffiliateEntitySpecs, 0);
					al = eTemp.GetEntityArrayList(0, "", 0, Locale, false);
					for(int i = 0; i < al.Count; i++)
					{
						ListItemClass lic = (ListItemClass)al[i];
						string value = lic.Value.ToString();
						string name = lic.Item;

						cblAffiliates.Items.Add(new ListItem(name, value));
					}

					eTemp = new EntityHelper(0, EntityDefinitions.LookupSpecs("Genre"), false, 0);
					al = eTemp.GetEntityArrayList(0, "", 0, Locale, false);
					for(int i = 0; i < al.Count; i++)
					{
						ListItemClass lic = (ListItemClass)al[i];
						string value = lic.Value.ToString();
						string name = lic.Item;

						cblGenres.Items.Add(new ListItem(name, value));
					}

					eTemp = new EntityHelper(0, EntityDefinitions.LookupSpecs("Vector"), false, 0);
					al = eTemp.GetEntityArrayList(0, "", 0, Locale, false);
					for(int i = 0; i < al.Count; i++)
					{
						ListItemClass lic = (ListItemClass)al[i];
						string value = lic.Value.ToString();
						string name = lic.Item;

						cblVectors.Items.Add(new ListItem(name, value));
					}

					if(Editing)
					{

						this.phAllVariants.Visible = false;
						this.phAddVariant.Visible = false;

						SetVariantButtons();

						ltStatus.Text = "Editing <em>" + AppLogic.GetProductName(ProductId, Locale) + "</em> (" + ProductId.ToString() + ")";
						txtName.Text = XmlCommon.GetLocaleEntry(DB.RSField(rs, "Name"), Locale, false);

						if(!DB.RSFieldBool(rs, "Published"))
						{
							rblPublished.BackColor = System.Drawing.Color.LightYellow;
						}
						else
						{
							rblPublished.BackColor = System.Drawing.Color.Empty;
						}
						rblPublished.SelectedIndex = (DB.RSFieldBool(rs, "Published") ? 1 : 0);

						rblFeatured.SelectedIndex = (DB.RSFieldBool(rs, "IsFeatured") ? 1 : 0);

						//match Type
						foreach(ListItem li in ddType.Items)
						{
							if(li.Value.Equals(DB.RSFieldInt(rs, "ProductTypeID").ToString()))
							{
								ddType.SelectedValue = DB.RSFieldInt(rs, "ProductTypeID").ToString();
							}
						}

						//match Manufacturer
						foreach(ListItem li in ddManufacturer.Items)
						{
							if(li.Value.Equals(AppLogic.GetProductManufacturerID(ProductId).ToString()))
							{
								ddManufacturer.SelectedValue = AppLogic.GetProductManufacturerID(ProductId).ToString();
							}
						}

						//match Distributor
						foreach(ListItem li in ddDistributor.Items)
						{
							if(li.Value.Equals(AppLogic.GetProductDistributorID(ProductId).ToString()))
							{
								ddDistributor.SelectedValue = AppLogic.GetProductDistributorID(ProductId).ToString();
							}
						}

						//match XmlPackage
						ddXmlPackage.SelectFirstByValue(DB.RSField(rs, "XmlPackage"), StringComparer.OrdinalIgnoreCase);

						//match Discount Table
						ddDiscountTable.ClearSelection();
						foreach(ListItem li in ddDiscountTable.Items)
						{
							if(li.Value.Equals(DB.RSFieldInt(rs, "QuantityDiscountID").ToString()))
							{
								ddDiscountTable.SelectedValue = DB.RSFieldInt(rs, "QuantityDiscountID").ToString();
							}
						}

						//match Tax Class
						ddTaxClass.ClearSelection();
						foreach(ListItem li in ddTaxClass.Items)
						{
							if(li.Value.Equals(DB.RSFieldInt(rs, "TaxClassID").ToString()))
							{
								ddTaxClass.SelectedValue = DB.RSFieldInt(rs, "TaxClassID").ToString();
							}
						}

						//match On Sale
						ddOnSalePrompt.ClearSelection();
						foreach(ListItem li in ddOnSalePrompt.Items)
						{
							if(li.Value.Equals(DB.RSFieldInt(rs, "SalesPromptID").ToString()))
							{
								ddOnSalePrompt.SelectedValue = DB.RSFieldInt(rs, "SalesPromptID").ToString();
							}
						}

						txtSKU.Text = DB.RSField(rs, "SKU");
						txtManufacturerPartNumber.Text = XmlCommon.XmlDecode(DB.RSField(rs, "ManufacturerPartNumber"));

						rblShowBuyButton.SelectedIndex = (DB.RSFieldBool(rs, "ShowBuyButton") ? 1 : 0);
						rblIsCallToOrder.SelectedIndex = (DB.RSFieldBool(rs, "IsCallToOrder") ? 1 : 0);
						rblHidePriceUntilCart.SelectedIndex = (DB.RSFieldBool(rs, "HidePriceUntilCart") ? 1 : 0);
						rblExcludeFroogle.SelectedIndex = (DB.RSFieldBool(rs, "ExcludeFromPriceFeeds") ? 1 : 0);

						rblIsKit.SelectedIndex = IsAKit ? 1 : 0;
						ltKit.Text = IsAKit
									 ? string.Format("<a class=\"btn btn-sm btn-primary\" href=\"kit.aspx?productid={0}&entityName={1}&entityFilterID={2}\">Edit Kit</a>", DB.RSFieldInt(rs, "ProductID"), EntityName, EntityId)
									 : String.Empty;

						if(IsAKit)
						{
							trInventory1.Visible = false;
							trInventory2.Visible = false;
							trInventory3.Visible = false;
						}
						else
						{
							trInventory1.Visible = true;
							trInventory2.Visible = true;
							trInventory3.Visible = true;

							rblTrackSizeColor.SelectedIndex = CommonLogic.IIF(DB.RSFieldBool(rs, "TrackInventoryBySizeAndColor"), 1, 0);
							txtColorOption.Text = XmlCommon.GetLocaleEntry(DB.RSField(rs, "ColorOptionPrompt"), Locale, false);
							txtSizeOption.Text = XmlCommon.GetLocaleEntry(DB.RSField(rs, "SizeOptionPrompt"), Locale, false);
						}

						rblRequiresTextField.SelectedIndex = CommonLogic.IIF(DB.RSFieldBool(rs, "RequiresTextOption"), 1, 0);
						txtTextFieldPrompt.Text = XmlCommon.GetLocaleEntry(DB.RSField(rs, "TextOptionPrompt"), Locale, false);
						txtTextOptionMax.Text = DB.RSFieldInt(rs, "TextOptionMaxLength").ToString();

						rblRequiresRegistrationToView.SelectedIndex = CommonLogic.IIF(DB.RSFieldBool(rs, "RequiresRegistration"), 1, 0);

						txtColumn.Text = CommonLogic.IIF(Editing, DB.RSFieldInt(rs, "ColWidth").ToString(), AppLogic.AppConfig("Default_" + EntitySpecs.m_EntityName + "ColWidth"));

						//DESCRIPTION
						if(UseHtmlEditor)
						{
							radDescription.Content = XmlCommon.GetLocaleEntry(DB.RSField(rs, "Description"), Locale, false);
						}
						else
						{
							txtDescriptionNoHtmlEditor.Text = XmlCommon.GetLocaleEntry(DB.RSField(rs, "Description"), Locale, false);
						}

						//SUMMARY
						if(UseHtmlEditor)
						{
							radSummary.Content = XmlCommon.GetLocaleEntry(DB.RSField(rs, "Summary"), Locale, false);
						}
						else
						{
							txtSummaryNoHtmlEditor.Text = XmlCommon.GetLocaleEntry(DB.RSField(rs, "Summary"), Locale, false);
						}

						//FROOGLE
						txtFroogle.Text = XmlCommon.GetLocaleEntry(DB.RSField(rs, "FroogleDescription"), Locale, false);

						//EXTENSION DATA
						txtExtensionData1.Text = Server.HtmlDecode(DB.RSField(rs, "ExtensionData"));
						txtExtensionData2.Text = Server.HtmlDecode(DB.RSField(rs, "ExtensionData2"));
						txtExtensionData3.Text = Server.HtmlDecode(DB.RSField(rs, "ExtensionData3"));
						txtExtensionData4.Text = Server.HtmlDecode(DB.RSField(rs, "ExtensionData4"));
						txtExtensionData5.Text = Server.HtmlDecode(DB.RSField(rs, "ExtensionData5"));

						//MISC TEXT
						txtMiscText.Text = Server.HtmlDecode(DB.RSField(rs, "MiscText"));

						//MAPPINGS
						pnlMapCategories.Visible = false;
						pnlMapDepartments.Visible = false;
						pnlMapAffiliates.Visible = false;
						pnlMapGenres.Visible = false;
						pnlMapVectors.Visible = false;
						var alE = new ArrayList();
						//Category Mapping
						if(cblCategory.Items.Count > 0)
						{
							pnlMapCategories.Visible = true;

							alE = EntityHelper.GetProductEntityList(ProductId, EntityDefinitions.readonly_CategoryEntitySpecs.m_EntityName);
							foreach(ListItem li in cblCategory.Items)
							{
								if(alE.IndexOf(Localization.ParseNativeInt(li.Value)) > -1)
								{
									li.Selected = true;
								}
							}
						}
						//Section Mapping
						if(cblDepartment.Items.Count > 0)
						{
							pnlMapDepartments.Visible = true;

							alE = EntityHelper.GetProductEntityList(ProductId, EntityDefinitions.readonly_SectionEntitySpecs.m_EntityName);
							foreach(ListItem li in cblDepartment.Items)
							{
								if(alE.IndexOf(Localization.ParseNativeInt(li.Value)) > -1)
								{
									li.Selected = true;
								}
							}
						}

						//Affiliate Mapping
						if(cblAffiliates.Items.Count > 0)
						{
							pnlMapAffiliates.Visible = true;

							alE = EntityHelper.GetProductEntityList(ProductId, EntityDefinitions.readonly_AffiliateEntitySpecs.m_EntityName);
							foreach(ListItem li in cblAffiliates.Items)
							{
								if(alE.IndexOf(Localization.ParseNativeInt(li.Value)) > -1)
								{
									li.Selected = true;
								}
							}
						}


						//Customer Level Mapping
						if(cblCustomerLevels.Items.Count > 0)
						{
							pnlMapCustomerLevels.Visible = true;

							alE = EntityHelper.GetProductEntityList(ProductId, EntityDefinitions.readonly_CustomerLevelEntitySpecs.m_EntityName);
							foreach(ListItem li in cblCustomerLevels.Items)
							{
								if(alE.IndexOf(Localization.ParseNativeInt(li.Value)) > -1)
								{
									li.Selected = true;
								}
							}
						}

						//Genre Mapping
						if(cblGenres.Items.Count > 0)
						{
							pnlMapGenres.Visible = true;

							alE = EntityHelper.GetProductEntityList(ProductId, EntityDefinitions.readonly_GenreEntitySpecs.m_EntityName);
							foreach(ListItem li in cblGenres.Items)
							{
								if(alE.IndexOf(Localization.ParseNativeInt(li.Value)) > -1)
								{
									li.Selected = true;
								}
							}
						}
						//Vector Mapping
						if(cblVectors.Items.Count > 0)
						{
							pnlMapVectors.Visible = true;

							alE = EntityHelper.GetProductEntityList(ProductId, EntityDefinitions.readonly_VectorEntitySpecs.m_EntityName);
							foreach(ListItem li in cblVectors.Items)
							{
								if(alE.IndexOf(Localization.ParseNativeInt(li.Value)) > -1)
								{
									li.Selected = true;
								}
							}
						}


						//RELATED and REQUIRED
						txtRelatedProducts.Text = DB.RSField(rs, "RelatedProducts");
						txtUpsellProducts.Text = DB.RSField(rs, "UpsellProducts");
						txtUpsellProductsDiscount.Text = Localization.FormatDecimal2Places(DB.RSFieldDecimal(rs, "UpsellProductDiscountPercentage"));
						txtRequiresProducts.Text = DB.RSField(rs, "RequiresProducts");

						//SEARCH ENGINE
						txtSETitle.Text = XmlCommon.GetLocaleEntry(DB.RSField(rs, "SETitle"), Locale, false);
						txtSEKeywords.Text = XmlCommon.GetLocaleEntry(DB.RSField(rs, "SEKeywords"), Locale, false);
						txtSEDescription.Text = XmlCommon.GetLocaleEntry(DB.RSField(rs, "SEDescription"), Locale, false);
						txtSEAlt.Text = XmlCommon.GetLocaleEntry(DB.RSField(rs, "SEAltText"), Locale, false);

						// BEGIN IMAGES 
						txtImageOverride.Text = CommonLogic.IIF(Editing, DB.RSField(rs, "ImageFilenameOverride"), "");
						bool disableupload = (Editing && DB.RSField(rs, "ImageFilenameOverride") != "");

						var defaultSkinName = SkinProvider.GetSkinNameById(Store.GetDefaultStore().SkinID);

						if(EntitySpecs.m_HasIconPic)
						{
							fuIcon.Enabled = !disableupload;
							String Image1URL = AppLogic.LookupImage("Product", ProductId, "icon", CurrentSkinID, Locale);
							if(Image1URL.Length == 0)
							{
								Image1URL = AppLogic.NoPictureImageURL(false, CurrentSkinID, Locale);
							}
							if(!CommonLogic.FileExists(Image1URL))
							{
								Image1URL = AppLogic.LocateImageURL(string.Format("Skins/{0}/images/nopictureicon.gif", defaultSkinName), Locale);
							}
							if(Image1URL.Length != 0)
							{
								ltIcon.Text = "";
								if(Image1URL.IndexOf("nopicture") == -1)
								{
									ltIcon.Text = ("<a href=\"javascript:void(0);\" onClick=\"DeleteImage('" + Image1URL + "','Pic1');\">Click here</a> to delete the current image <br/>\n");
								}
								ltIcon.Text += "<img align=\"absmiddle\" style=\"margin-top: 3px; margin-bottom: 5px;\" id=\"Pic1\" name=\"Pic1\" border=\"0\" src=\"" + Image1URL + "?" + CommonLogic.GetRandomNumber(1, 1000000).ToString() + "\" />\n";
								if(AppLogic.GetProductsDefaultVariantID(ProductId) != 0)
								{
									ltIcon.Text += ("&nbsp;&nbsp;<a href=\"javascript:;\" onclick=\"window.open('multiimagemanager.aspx?size=icon&productid=" + ProductId.ToString() + "','Images','height=450, width=780,  scrollbars=yes, resizable=yes, toolbar=no, status=yes, location=no, directories=no, menubar=no, alwaysRaised=yes');\">Icon Multi-Image Manager</a>");
								}
							}
						}
						if(EntitySpecs.m_HasMediumPic)
						{
							fuMedium.Enabled = !disableupload;
							String Image2URL = AppLogic.LookupImage("Product", ProductId, "medium", CurrentSkinID, Locale);
							if(Image2URL.Length == 0)
							{
								Image2URL = AppLogic.NoPictureImageURL(false, CurrentSkinID, Locale);
							}
							if(!CommonLogic.FileExists(Image2URL))
							{
								Image2URL = AppLogic.LocateImageURL(String.Format("Skins/{0}/images/nopicture.gif", defaultSkinName), Locale);
							}
							if(Image2URL.Length != 0)
							{
								ltMedium.Text = "";
								if(Image2URL.IndexOf("nopicture") == -1)
								{
									ltMedium.Text = ("<a href=\"javascript:void(0);\" onClick=\"DeleteImage('" + Image2URL + "','Pic2');\">Click here</a> to delete the current image <br/>\n");
								}
								ltMedium.Text += "<img align=\"absmiddle\" style=\"margin-top: 3px; margin-bottom: 5px;\" id=\"Pic2\" name=\"Pic2\" border=\"0\" src=\"" + Image2URL + "?" + CommonLogic.GetRandomNumber(1, 1000000).ToString() + "\" />\n";
								if(AppLogic.GetProductsDefaultVariantID(ProductId) != 0)
								{
									ltMedium.Text += ("&nbsp;&nbsp;<a href=\"javascript:;\" onclick=\"window.open('multiimagemanager.aspx?size=medium&productid=" + ProductId.ToString() + "','Images','height=550, width=780,  scrollbars=yes, resizable=yes, toolbar=no, status=yes, location=no, directories=no, menubar=no, alwaysRaised=yes');\">Medium Multi-Image Manager</a>");
								}
							}
						}
						if(EntitySpecs.m_HasLargePic)
						{
							fuLarge.Enabled = !disableupload;
							String Image3URL = AppLogic.LookupImage("Product", ProductId, "large", CurrentSkinID, Locale);
							if(Image3URL.Length == 0)
							{
								Image3URL = AppLogic.NoPictureImageURL(false, CurrentSkinID, Locale);
							}
							if(!CommonLogic.FileExists(Image3URL))
							{
								Image3URL = AppLogic.LocateImageURL(String.Format("Skins/{0}/images/nopicture.gif", defaultSkinName), Locale);
							}
							if(Image3URL.Length != 0)
							{
								ltLarge.Text = "";
								if(Image3URL.IndexOf("nopicture") == -1)
								{
									ltLarge.Text = ("<a href=\"javascript:void(0);\" onClick=\"DeleteImage('" + Image3URL + "','Pic3');\">Click here</a> to delete the current image <br/>\n");
								}
								ltLarge.Text += "<img align=\"absmiddle\" style=\"margin-top: 3px; margin-bottom: 5px;\" id=\"Pic3\" name=\"Pic3\" border=\"0\" src=\"" + Image3URL + "?" + CommonLogic.GetRandomNumber(1, 1000000).ToString() + "\" />\n";
								if(AppLogic.GetProductsDefaultVariantID(ProductId) != 0)
								{
									ltLarge.Text += ("&nbsp;&nbsp;<a href=\"javascript:;\" onclick=\"window.open('multiimagemanager.aspx?size=large&productid=" + ProductId.ToString() + "','Images','height=650, width=780,  scrollbars=yes, resizable=yes, toolbar=no, status=yes, location=no, directories=no, menubar=no, alwaysRaised=yes');\">Large Multi-Image Manager</a>");
								}
							}
						}
						// END IMAGES   

						//VARIANTS
					}
					else
					{
						this.phAllVariants.Visible = false;
						this.phAddVariant.Visible = true;
						ltStatus.Text = "Adding Product";

						txtColumn.Text = AppLogic.AppConfig("Default_" + EntitySpecs.m_EntityName + "ColWidth");

						//match Type
						foreach(ListItem li in ddType.Items)
						{
							if(li.Value.Equals(AppLogic.AppConfigUSInt("Admin_DefaultProductTypeID").ToString()))
							{
								ddType.SelectedValue = AppLogic.AppConfigUSInt("Admin_DefaultProductTypeID").ToString();
							}
						}

						//match On Sale
						foreach(ListItem li in ddOnSalePrompt.Items)
						{
							if(li.Value.Equals(AppLogic.AppConfigUSInt("Admin_DefaultSalesPromptID").ToString()))
							{
								ddOnSalePrompt.SelectedValue = AppLogic.AppConfigUSInt("Admin_DefaultSalesPromptID").ToString();
							}
						}

						//EXTENSION DATA
						txtExtensionData1.Text = CommonLogic.IIF(Editing, DB.RSField(rs, "ExtensionData"), "");
						txtExtensionData2.Text = CommonLogic.IIF(Editing, DB.RSField(rs, "ExtensionData2"), "");
						txtExtensionData3.Text = CommonLogic.IIF(Editing, DB.RSField(rs, "ExtensionData3"), "");
						txtExtensionData4.Text = CommonLogic.IIF(Editing, DB.RSField(rs, "ExtensionData4"), "");
						txtExtensionData5.Text = CommonLogic.IIF(Editing, DB.RSField(rs, "ExtensionData5"), "");

						//MISCTEXT
						txtMiscText.Text = CommonLogic.IIF(Editing, DB.RSField(rs, "MiscText"), "");

						//VARIANT
						trInventory.Visible = false;
						if(!ProductTracksInventoryBySizeAndColor)
						{
							trInventory.Visible = true;
						}

						//set defaults
						rblExcludeFroogle.SelectedIndex = 0;
						rblHidePriceUntilCart.SelectedIndex = 0;
						rblIsCallToOrder.SelectedIndex = 0;
						rblIsKit.SelectedIndex = 0;
						rblPublished.SelectedIndex = 1;
						rblFeatured.SelectedIndex = 0;
						rblRequiresRegistrationToView.SelectedIndex = 0;
						rblRequiresTextField.SelectedIndex = 0;
						rblShowBuyButton.SelectedIndex = 1;
						rblTrackSizeColor.SelectedIndex = 0;

						//MAPPINGS
						if(EntityId > 0)
						{
							//match Manufacturer
							if(EntityName.Equals("MANUFACTURER", StringComparison.InvariantCultureIgnoreCase))
							{
								ddManufacturer.ClearSelection();
								foreach(ListItem li in ddManufacturer.Items)
								{
									if(li.Value.Equals(EntityId.ToString()))
									{
										ddManufacturer.SelectedValue = EntityId.ToString();
									}
								}
							}

							//match Distributor
							if(EntityName.Equals("DISTRIBUTOR", StringComparison.InvariantCultureIgnoreCase))
							{
								ddDistributor.ClearSelection();
								foreach(ListItem li in ddDistributor.Items)
								{
									if(li.Value.Equals(EntityId.ToString()))
									{
										ddDistributor.SelectedValue = EntityId.ToString();
									}
								}
							}

							//Category
							if(EntityName.Equals("CATEGORY", StringComparison.InvariantCultureIgnoreCase))
							{
								foreach(ListItem li in cblCategory.Items)
								{
									if(li.Value.Equals(EntityId.ToString()))
									{
										li.Selected = true;
									}
								}
							}

							//Section
							if(EntityName.Equals("SECTION", StringComparison.InvariantCultureIgnoreCase))
							{
								foreach(ListItem li in cblDepartment.Items)
								{
									if(li.Value.Equals(EntityId.ToString()))
									{
										li.Selected = true;
									}
								}
							}

							//Affiliate
							if(EntityName.Equals("AFFILIATE", StringComparison.InvariantCultureIgnoreCase))
							{
								foreach(ListItem li in cblAffiliates.Items)
								{
									if(li.Value.Equals(EntityId.ToString()))
									{
										li.Selected = true;
									}
								}
							}
						}
					}

					ltScript2.Text = ("<script type=\"text/javascript\">\n");
					ltScript2.Text += ("function DeleteImage(imgurl,name)\n");
					ltScript2.Text += ("{\n");
					ltScript2.Text += ("if(confirm('Are you sure you want to delete this image?')){\n");
					ltScript2.Text += ("window.open('deleteimage.aspx?imgurl=' + imgurl + '&FormImageName=' + name,\"Admin_ML\",\"height=250,width=440,top=10,left=20,status=no,toolbar=no,menubar=no,scrollbars=yes,location=no\")\n");
					ltScript2.Text += ("}}\n");
					ltScript2.Text += ("</script>\n");
				}
			}
		}

		private void LoadDistributors()
		{
			LoadDistributors(0);
		}

		private void LoadDistributors(int distributorId)
		{
			ddDistributor.Items.Clear();
			ddDistributor.Items.Add(new ListItem(" - Select -", "0"));

			using(SqlConnection conn = new SqlConnection(DB.GetDBConn()))
			{
				conn.Open();
				using(IDataReader rsst = DB.GetRS("select Name, DistributorID from Distributor   with (NOLOCK)  where Deleted=0 order by DisplayOrder,Name", conn))
				{
					while(rsst.Read())
					{
						ddDistributor.Items.Add(new ListItem(DB.RSFieldByLocale(rsst, "Name", Locale), DB.RSFieldInt(rsst, "DistributorID").ToString()));
					}
				}
			}
			if(distributorId > 0)
				ddDistributor.SelectedValue = distributorId.ToString();
		}


		private void LoadManufacturers()
		{
			LoadManufacturers(0);
		}

		private void LoadManufacturers(int manufacturerId)
		{
			ddManufacturer.Items.Clear();
			ddManufacturer.Items.Add(new ListItem(" - Select -", "0"));
			string sql = "select Name, ManufacturerID from Manufacturer  with (NOLOCK)  where deleted=0 order by DisplayOrder,Name";

			using(SqlConnection conn = new SqlConnection(DB.GetDBConn()))
			{
				conn.Open();
				using(IDataReader rsst = DB.GetRS(sql, conn))
				{
					while(rsst.Read())
					{
						ddManufacturer.Items.Add(new ListItem(DB.RSFieldByLocale(rsst, "Name", Locale), DB.RSFieldInt(rsst, "ManufacturerID").ToString()));
					}
				}
			}

			if(manufacturerId > 0)
				ddManufacturer.SelectedValue = manufacturerId.ToString();
		}

		private void LoadDiscountTables()
		{
			LoadDiscountTables(0);
		}

		private void LoadDiscountTables(int quantityDiscountId)
		{
			ddDiscountTable.Items.Clear();

			ddDiscountTable.Items.Add(new ListItem("None", "0"));

			using(SqlConnection conn = new SqlConnection(DB.GetDBConn()))
			{
				conn.Open();
				using(IDataReader rsst = DB.GetRS("select Name, QuantityDiscountID from QuantityDiscount   with (NOLOCK)  order by DisplayOrder,Name", conn))
				{
					while(rsst.Read())
					{
						ddDiscountTable.Items.Add(new ListItem(DB.RSFieldByLocale(rsst, "Name", Locale), DB.RSFieldInt(rsst, "QuantityDiscountID").ToString()));
					}
				}
			}

			if(quantityDiscountId > 0)
				ddDiscountTable.SelectedValue = quantityDiscountId.ToString();
		}

		private void LoadCategories()
		{
			LoadCategories(0);
		}

		private void LoadCategories(int categoryId)
		{
			cblCategory.Items.Clear();


			EntityHelper eTemp = new EntityHelper(0, EntityDefinitions.LookupSpecs("Category"), false, 0);
			var categoryEntities = eTemp.GetEntityArrayList(0, "", 0, Locale, false).Cast<ListItemClass>();
			foreach(var listItem in categoryEntities)
			{
				string value = listItem.Value.ToString();
				string name = listItem.Item;
				ListItem checkBoxItem = new ListItem(name, value);

				if(value == categoryId.ToString())
					checkBoxItem.Selected = true;

				cblCategory.Items.Add(checkBoxItem);

			}

			ArrayList alE = EntityHelper.GetProductEntityList(ProductId, EntityDefinitions.readonly_CategoryEntitySpecs.m_EntityName);
			foreach(ListItem li in cblCategory.Items)
			{
				if(alE.IndexOf(Localization.ParseNativeInt(li.Value)) > -1)
				{
					li.Selected = true;
				}
			}
		}

		private void LoadDepartments()
		{
			LoadDepartments(0);
		}

		private void LoadDepartments(int departmentId)
		{
			cblDepartment.Items.Clear();


			EntityHelper eTemp = new EntityHelper(0, EntityDefinitions.LookupSpecs("Section"), false, 0);
			var categoryEntities = eTemp.GetEntityArrayList(0, "", 0, Locale, false).Cast<ListItemClass>();
			foreach(var listItem in categoryEntities)
			{
				string value = listItem.Value.ToString();
				string name = listItem.Item;
				ListItem checkBoxItem = new ListItem(name, value);

				if(value == departmentId.ToString())
					checkBoxItem.Selected = true;

				cblDepartment.Items.Add(checkBoxItem);
			}

			ArrayList alE = EntityHelper.GetProductEntityList(ProductId, EntityDefinitions.readonly_SectionEntitySpecs.m_EntityName);
			foreach(ListItem li in cblDepartment.Items)
			{
				if(alE.IndexOf(Localization.ParseNativeInt(li.Value)) > -1)
				{
					li.Selected = true;
				}
			}
		}

		private void LoadCustomerLevels()
		{
			LoadCustomerLevels(0);
		}
		private void LoadCustomerLevels(int customerLevelId)
		{
			cblCustomerLevels.Items.Clear();

			EntityHelper eTemp = new EntityHelper(EntityDefinitions.readonly_CustomerLevelEntitySpecs, 0);
			var custLevelEntities = eTemp.GetEntityArrayList(0, "", 0, Locale, false).Cast<ListItemClass>();
			foreach(var listItem in custLevelEntities)
			{
				string value = listItem.Value.ToString();
				string name = listItem.Item;
				ListItem checkBoxItem = new ListItem(name, value);

				if(value == customerLevelId.ToString())
					checkBoxItem.Selected = true;

				cblCustomerLevels.Items.Add(checkBoxItem);
			}

			ArrayList alE = EntityHelper.GetProductEntityList(ProductId, EntityDefinitions.readonly_CustomerLevelEntitySpecs.m_EntityName);
			foreach(ListItem li in cblCustomerLevels.Items)
			{
				if(alE.IndexOf(Localization.ParseNativeInt(li.Value)) > -1)
				{
					li.Selected = true;
				}
			}
		}


		protected void LocaleSelector_SelectedLocaleChanged(Object sender, EventArgs e)
		{
			LoadContent();
		}

		protected void btnSubmit_Click(object sender, EventArgs e)
		{
			if(!UpdateProduct())
				return;

			etsMapper.Save();
			LoadContent();
		}

		protected void btnSaveAndClose_Click(object sender, EventArgs e)
		{
			if(!UpdateProduct())
				return;

			etsMapper.Save();
			Response.Redirect(ReturnUrlTracker.GetReturnUrl());
		}

		protected bool UpdateProduct()
		{
			if(!Page.IsValid)
				return false;

			var editing = Localization.ParseBoolean(ViewState["ProductEdit"].ToString());

			var productParameters = new[]
				{
					new SqlParameter(
						"Name", 
						AppLogic.FormLocaleXml("Name", txtName.Text, Locale, "Product", ProductId)),

					new SqlParameter(
						"SEName", 
						SearchEngineNameProvider.GenerateSeName(AppLogic.GetFormsDefaultLocale("Name", txtName.Text, Locale, "Product", ProductId))),

					new SqlParameter("ImageFilenameOverride", txtImageOverride.Text),

					new SqlParameter(
						"ProductTypeID", 
						Localization.ParseNativeInt(ddType.SelectedValue)),

					new SqlParameter(
						"Summary", 
						AppLogic.FormLocaleXml(
							"Summary", 
							UseHtmlEditor 
								? radSummary.Content 
								: txtSummaryNoHtmlEditor.Text.Trim(), 
							Locale, 
							"Product", 
							ProductId)),

					new SqlParameter(
						"Description", 
						AppLogic.FormLocaleXml(
							"Description", 
							UseHtmlEditor 
								? radDescription.Content 
								: txtDescriptionNoHtmlEditor.Text.Trim(), 
							Locale, 
							"Product", 
							ProductId)),

					new SqlParameter("ExtensionData", txtExtensionData1.Text),
					new SqlParameter("ExtensionData2", txtExtensionData2.Text),
					new SqlParameter("ExtensionData3", txtExtensionData3.Text),
					new SqlParameter("ExtensionData4", txtExtensionData4.Text),
					new SqlParameter("ExtensionData5", txtExtensionData5.Text),

					new SqlParameter(
						"ColorOptionPrompt", 
						AppLogic.FormLocaleXml("ColorOptionPrompt", txtColorOption.Text, Locale, "Product", ProductId)),

					new SqlParameter(
						"SizeOptionPrompt", 
						AppLogic.FormLocaleXml("SizeOptionPrompt", txtSizeOption.Text, Locale, "Product", ProductId)),

					new SqlParameter("RequiresTextOption", rblRequiresTextField.SelectedValue == "1"),

					new SqlParameter(
						"TextOptionPrompt", 
						AppLogic.FormLocaleXml("TextOptionPrompt", txtTextFieldPrompt.Text, Locale, "Product", ProductId)),

					new SqlParameter(
						"TextOptionMaxLength", 
						String.IsNullOrWhiteSpace(txtTextOptionMax.Text) 
							? DBNull.Value : 
							(object)Localization.ParseNativeInt(txtTextOptionMax.Text)),

					new SqlParameter(
						"FroogleDescription", 
						AppLogic.FormLocaleXml("FroogleDescription", txtFroogle.Text, Locale, "Product", ProductId)),
					
					new SqlParameter(
						"RelatedProducts", 
						String.IsNullOrWhiteSpace(txtRelatedProducts.Text) 
						? DBNull.Value 
						: (object)txtRelatedProducts.Text.Trim().Replace(" ", "")),

					new SqlParameter(
						"UpsellProducts", 
						String.IsNullOrWhiteSpace(txtUpsellProducts.Text) 
						? DBNull.Value 
						: (object)txtUpsellProducts.Text.Trim().Replace(" ", "")),

					new SqlParameter(
						"UpsellProductDiscountPercentage", 
						Localization.DecimalStringForDB(Localization.ParseNativeDecimal(txtUpsellProductsDiscount.Text))),

					new SqlParameter(
						"RequiresProducts", 
						String.IsNullOrWhiteSpace(txtRequiresProducts.Text)
							? DBNull.Value
							: (object)txtRequiresProducts.Text.Trim().Replace(" ", "")),

					new SqlParameter(
						"SEKeywords", 
						String.IsNullOrWhiteSpace(txtSEKeywords.Text)
							? DBNull.Value
							: (object)AppLogic.FormLocaleXml("SEKeywords", txtSEKeywords.Text, Locale, "Product", ProductId)),

					new SqlParameter(
						"SEDescription", 
						String.IsNullOrWhiteSpace(txtSEDescription.Text)
							? DBNull.Value 
							: (object)AppLogic.FormLocaleXml("SEDescription", txtSEDescription.Text, Locale, "Product", ProductId)),

					new SqlParameter(
						"SETitle", 
						String.IsNullOrWhiteSpace(txtSETitle.Text)
							? DBNull.Value 
							: (object)AppLogic.FormLocaleXml("SETitle", txtSETitle.Text, Locale, "Product", ProductId)),

					new SqlParameter(
						"SEAltText", 
						String.IsNullOrWhiteSpace(txtSEAlt.Text)
							? DBNull.Value 
							: (object)AppLogic.FormLocaleXml("SEAltText", txtSEAlt.Text, Locale, "Product", ProductId)),

					new SqlParameter(
						"SKU", 
						String.IsNullOrWhiteSpace(txtSKU.Text)
							? DBNull.Value 
							: (object)txtSKU.Text),
					
					new SqlParameter(
						"ColWidth", 
						String.IsNullOrWhiteSpace(txtColumn.Text) 
							? AppLogic.AppConfigNativeInt("Default_ProductColWidth") 
							: (object)Localization.ParseNativeInt(txtColumn.Text)),
					
					new SqlParameter(
						"XmlPackage", 
						String.IsNullOrWhiteSpace(ddXmlPackage.SelectedValue)
							? rblIsKit.SelectedValue == "1"
								? AppLogic.ro_DefaultProductKitXmlPackage	// force a default!
								: AppLogic.ro_DefaultProductXmlPackage		// force a default!
							: ddXmlPackage.SelectedValue.ToLowerInvariant()),
					
					new SqlParameter("ManufacturerPartNumber", txtManufacturerPartNumber.Text),
					new SqlParameter("SalesPromptID", Localization.ParseNativeInt(ddOnSalePrompt.SelectedValue)),
					new SqlParameter("Published", rblPublished.SelectedValue == "1"),
					new SqlParameter("IsFeatured", rblFeatured.SelectedValue == "1"),
					new SqlParameter("ShowBuyButton", rblShowBuyButton.SelectedValue == "1"),
					new SqlParameter("IsCallToOrder", rblIsCallToOrder.SelectedValue == "1"),
					new SqlParameter("HidePriceUntilCart", rblHidePriceUntilCart.SelectedValue == "1"),
					new SqlParameter("ExcludeFromPriceFeeds", rblExcludeFroogle.SelectedValue == "1"),
					new SqlParameter("IsAKit", rblIsKit.SelectedValue == "1"),
					
					new SqlParameter(
						"TrackInventoryBySizeAndColor",						// We will use this one value to set TrackInventoryBySizeAndColor, TrackInventoryBySize, and TrackInventoryByColor
						rblIsKit.SelectedValue == "1"
							? false											// Cannot track inventory by size and color on kits
							: rblTrackSizeColor.SelectedValue == "1"),
					
					new SqlParameter("RequiresRegistration", rblRequiresRegistrationToView.SelectedValue == "1"),
					
					new SqlParameter(
						"MiscText", 
						String.IsNullOrWhiteSpace(txtMiscText.Text)
							? DBNull.Value
							: (object) txtMiscText.Text),
					
					new SqlParameter(
						"QuantityDiscountID", 
						(object)Localization.ParseNativeInt(ddDiscountTable.SelectedValue)),
					
					new SqlParameter(
						"TaxClassID",
						ddTaxClass.SelectedValue == "0"
							? AppLogic.AppConfigNativeInt("Admin_DefaultTaxClassID")
							: (object)Localization.ParseNativeInt(ddTaxClass.SelectedValue)),
				};

			if(!editing)
			{
				var sql = @"
					insert into product(
						ProductGUID,
						Name,
						SEName,
						ImageFilenameOverride,
						ProductTypeID,
						Summary,
						Description,
						ExtensionData,
						ExtensionData2,
						ExtensionData3,
						ExtensionData4,
						ExtensionData5,
						ColorOptionPrompt,
						SizeOptionPrompt,
						RequiresTextOption,
						TextOptionPrompt,
						TextOptionMaxLength,
						FroogleDescription,
						RelatedProducts,
						UpsellProducts,
						UpsellProductDiscountPercentage,
						RequiresProducts,
						SEKeywords,
						SEDescription,
						SETitle,
						SEAltText,
						SKU,
						ColWidth,
						XmlPackage,
						ManufacturerPartNumber,
						SalesPromptID,
						Published,
						IsFeatured,
						ShowBuyButton,
						IsCallToOrder,
						HidePriceUntilCart,
						ExcludeFromPriceFeeds,
						IsAKit,
						TrackInventoryBySizeAndColor,
						TrackInventoryBySize,
						TrackInventoryByColor,
						RequiresRegistration,
						MiscText,
						QuantityDiscountID,
						TaxClassID
					) values (
						@ProductGUID,
						@Name,
						@SEName,
						@ImageFilenameOverride,
						@ProductTypeID,
						@Summary,
						@Description,
						@ExtensionData,
						@ExtensionData2,
						@ExtensionData3,
						@ExtensionData4,
						@ExtensionData5,
						@ColorOptionPrompt,
						@SizeOptionPrompt,
						@RequiresTextOption,
						@TextOptionPrompt,
						@TextOptionMaxLength,
						@FroogleDescription,
						@RelatedProducts,
						@UpsellProducts,
						@UpsellProductDiscountPercentage,
						@RequiresProducts,
						@SEKeywords,
						@SEDescription,
						@SETitle,
						@SEAltText,
						@SKU,
						@ColWidth,
						@XmlPackage,
						@ManufacturerPartNumber,
						@SalesPromptID,
						@Published,
						@IsFeatured,
						@ShowBuyButton,
						@IsCallToOrder,
						@HidePriceUntilCart,
						@ExcludeFromPriceFeeds,
						@IsAKit,
						@TrackInventoryBySizeAndColor,
						@TrackInventoryBySizeAndColor,
						@TrackInventoryBySizeAndColor,
						@RequiresRegistration,
						@MiscText,
						@QuantityDiscountID,
						@TaxClassID
					)";

				var productGuid = DB.GetNewGUID();

				using(var connection = new SqlConnection(DB.GetDBConn()))
				{
					connection.Open();

					DB.ExecuteSQL(
						sql,
						connection,
						productParameters
							.Concat(new[] { new SqlParameter("ProductGUID", productGuid) })
							.ToArray());

					using(var reader = DB.GetRS("select ProductID from product with (NOLOCK) where deleted=0 and ProductGUID=" + DB.SQuote(productGuid), connection))
					{
						reader.Read();
						ProductId = DB.RSFieldInt(reader, "ProductID");
						ViewState.Add("ProductEdit", true);
						ViewState.Add("ProductEditID", ProductId);
						etsMapper.ObjectID = ProductId;
					}
				}

				// Create default variant
				var variantInsertSql = @"
					insert into ProductVariant (
						VariantGUID,
						Name,
						IsDefault,
						ProductID,
						Price,
						SalePrice,
						Weight,
						Dimensions,
						Inventory,
						Published,
						Colors,
						ColorSKUModifiers,
						Sizes,
						SizeSKUModifiers
					) values (
						@NewGUID,
						@VariantName,
						1,
						@ProductID,
						isnull(@Price, 0),
						@SalePrice,
						@Weight,
						@Dimensions,
						isnull(@Inventory, 10000),
						1,
						@Colors,
						@ColorSkuModifiers,
						@Sizes,
						@SizesSKUModifiers
					)";

				var variantParameters = new[]
					{
						new SqlParameter("@NewGUID", new Guid(DB.GetNewGUID())),
						new SqlParameter("@VariantName", ""),
						new SqlParameter("@ProductID", ProductId),
						new SqlParameter("@Dimensions", String.Format("{0}x{1}x{2}", txtWidth.Text.Trim(), txtHeight.Text.Trim(), txtDepth.Text.Trim())),
						new SqlParameter("@Colors", txtColors.Text),
						new SqlParameter("@ColorSkuModifiers", txtColorSKUModifiers.Text),
						new SqlParameter("@Sizes", txtSizes.Text),
						new SqlParameter("@SizesSKUModifiers", txtSizeSKUModifiers.Text),
						
						new SqlParameter(
							"@Price",
							String.IsNullOrWhiteSpace(txtPrice.Text)
								? DBNull.Value
								: (object)Localization.ParseNativeDecimal(txtPrice.Text)),

						new SqlParameter(
							"@SalePrice",
							String.IsNullOrWhiteSpace(txtSalePrice.Text)
								? DBNull.Value
								: (object)Localization.ParseNativeDecimal(txtSalePrice.Text)),


						new SqlParameter(
							"@Weight",
							String.IsNullOrWhiteSpace(txtWeight.Text)
								? DBNull.Value
								: (object)Localization.ParseNativeDecimal(txtWeight.Text)),

						new SqlParameter(
							"@Inventory",
							String.IsNullOrWhiteSpace(txtInventory.Text)
								? AppLogic.AppConfigNativeInt("Admin_DefaultInventory")
								: (object)Localization.ParseNativeInt(txtInventory.Text)),
					};

				DB.ExecuteSQL(variantInsertSql, variantParameters);

				ctrlAlertMessage.PushAlertMessage("A new product and default variant were added.", AlertMessage.AlertType.Success);
			}
			else
			{
				var sql = @"
					update
						product 
					set
						Name = @Name,
						SEName = @SEName,
						ImageFilenameOverride = @ImageFilenameOverride,
						ProductTypeID = @ProductTypeID,
						Summary = @Summary,
						Description = @Description,
						ExtensionData = @ExtensionData,
						ExtensionData2 = @ExtensionData2,
						ExtensionData3 = @ExtensionData3,
						ExtensionData4 = @ExtensionData4,
						ExtensionData5 = @ExtensionData5,
						ColorOptionPrompt = @ColorOptionPrompt,
						SizeOptionPrompt = @SizeOptionPrompt,
						RequiresTextOption = @RequiresTextOption,
						TextOptionPrompt = @TextOptionPrompt,
						TextOptionMaxLength = @TextOptionMaxLength,
						FroogleDescription = @FroogleDescription,
						RelatedProducts = @RelatedProducts,
						UpsellProducts = @UpsellProducts,
						UpsellProductDiscountPercentage = @UpsellProductDiscountPercentage,
						RequiresProducts = @RequiresProducts,
						SEKeywords = @SEKeywords,
						SEDescription = @SEDescription,
						SETitle = @SETitle,
						SEAltText = @SEAltText,
						SKU = @SKU,
						ColWidth = @ColWidth,
						XmlPackage = @XmlPackage,
						ManufacturerPartNumber = @ManufacturerPartNumber,
						SalesPromptID = @SalesPromptID,
						Published = @Published,
						IsFeatured = @IsFeatured,
						ShowBuyButton = @ShowBuyButton,
						IsCallToOrder = @IsCallToOrder,
						HidePriceUntilCart = @HidePriceUntilCart,
						ExcludeFromPriceFeeds = @ExcludeFromPriceFeeds,
						IsAKit = @IsAKit,
						TrackInventoryBySizeAndColor = @TrackInventoryBySizeAndColor,
						TrackInventoryBySize = @TrackInventoryBySizeAndColor,
						TrackInventoryByColor = @TrackInventoryBySizeAndColor,
						RequiresRegistration = @RequiresRegistration,
						MiscText = @MiscText,
						QuantityDiscountID = @QuantityDiscountID,
						TaxClassID = @TaxClassID
					where 
						ProductID = @ProductID";

				DB.ExecuteSQL(
					sql,
					productParameters
						.Concat(new[] { new SqlParameter("ProductID", (object)ProductId) })
						.ToArray());

				ViewState.Add("ProductEdit", true);
				ViewState.Add("ProductEditID", ProductId);

				ctrlAlertMessage.PushAlertMessage("Product Updated.", AlertMessage.AlertType.Success);
			}

			// UPDATE 1:1 ENTITY MAPPINGS:
			{
				String[] Entities = { "Manufacturer", "Distributor" };
				foreach(String en in Entities)
				{
					int NewID = 0;
					if(en.Equals("Manufacturer"))
					{
						NewID = Localization.ParseNativeInt(ddManufacturer.SelectedValue);
					}
					else
					{
						NewID = Localization.ParseNativeInt(ddDistributor.SelectedValue);
					}
					if(NewID == 0)
					{
						// no mapping (should not be allowed by form validator, but...)
						DB.ExecuteSQL("delete from Product" + en + " where ProductID=" + ProductId.ToString());
					}
					else
					{
						int OldID = CommonLogic.IIF(en == "Manufacturer", AppLogic.GetProductManufacturerID(ProductId), AppLogic.GetProductDistributorID(ProductId));
						if(OldID == 0)
						{
							// create default mapping:
							DB.ExecuteSQL(String.Format("insert into Product{0}(ProductID,{1}ID,DisplayOrder) values({2},{3},1)", en, en, ProductId.ToString(), NewID.ToString()));
						}
						else if(OldID != NewID)
						{
							// update existing mapping:
							DB.ExecuteSQL(String.Format("update Product{0} set {1}ID={2} where {3}ID={4} and ProductID={5}", en, en, NewID.ToString(), en, OldID.ToString(), ProductId.ToString()));
						}
					}
				}
			}

			// UPDATE 1:N ENTITY MAPPINGS:
			{
				String[] Entities2 = { "Category", "Section", "Affiliate", "CustomerLevel", "Genre", "Vector" };

				foreach(String en in Entities2)
				{
					String EnMap = "";
					if(en.Equals("Category"))
					{
						foreach(ListItem li in cblCategory.Items)
						{
							if(li.Selected)
							{
								EnMap += "," + li.Value;
							}
						}
					}
					else if(en.Equals("Section"))
					{
						foreach(ListItem li in cblDepartment.Items)
						{
							if(li.Selected)
							{
								EnMap += "," + li.Value;
							}
						}
					}
					else if(en.Equals("Affiliate"))
					{
						foreach(ListItem li in cblAffiliates.Items)
						{
							if(li.Selected)
							{
								EnMap += "," + li.Value;
							}
						}
					}
					else if(en.Equals("CustomerLevel"))
					{
						foreach(ListItem li in cblCustomerLevels.Items)
						{
							if(li.Selected)
							{
								EnMap += "," + li.Value;
							}
						}
					}
					else if(en.Equals("Genre"))
					{
						foreach(ListItem li in cblGenres.Items)
						{
							if(li.Selected)
							{
								EnMap += "," + li.Value;
							}
						}
					}
					else
					{
						foreach(ListItem li in cblVectors.Items)
						{
							if(li.Selected)
							{
								EnMap += "," + li.Value;
							}
						}
					}

					if(EnMap.Length > 1)
					{
						EnMap = EnMap.Substring(1);// Remove the leading ,
					}

					if(EnMap.Length == 0)
					{
						// no mappings
						DB.ExecuteSQL(String.Format("delete from Product{0} where ProductID={1}", en, ProductId.ToString()));
					}
					else
					{
						// remove any mappings not current anymore:
						DB.ExecuteSQL(String.Format("delete from Product{0} where ProductID={1} and {2}ID not in ({3})", en, ProductId.ToString(), en, EnMap));
						// add new default mappings:
						String[] EnMapArray = EnMap.Split(',');
						foreach(String EntityID in EnMapArray)
						{
							try
							{
								DB.ExecuteSQL(String.Format("insert Product{0}(ProductID,{1}ID,DisplayOrder) values({2},{3},1)", en, en, ProductId.ToString(), EntityID));
							}
							catch { }
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
			String FN = txtImageOverride.Text.Trim();
			if(AppLogic.AppConfigBool("UseSKUForProductImageName"))
				FN = txtSKU.Text.Trim();
			if(FN.Length == 0)
				FN = ProductId.ToString();

			String Image1 = String.Empty;
			String TempImage1 = String.Empty;
			HttpPostedFile Image1File = fuIcon.PostedFile;
			if(Image1File != null && Image1File.ContentLength != 0)
			{
				// delete any current image file first
				try
				{
					foreach(String ss in CommonLogic.SupportedImageTypes)
					{
						if(FN.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase) || FN.EndsWith(".gif", StringComparison.InvariantCultureIgnoreCase) || FN.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase))
						{
							System.IO.File.Delete(AppLogic.GetImagePath("Product", "icon", true) + FN);
						}
						else
						{
							System.IO.File.Delete(AppLogic.GetImagePath("Product", "icon", true) + FN + ss);
						}
					}
				}
				catch
				{ }

				switch(Image1File.ContentType)
				{
					case "image/gif":
						TempImage1 = AppLogic.GetImagePath("Product", "icon", true) + "tmp_" + FN + ".gif";
						Image1 = AppLogic.GetImagePath("Product", "icon", true) + FN + ".gif";
						Image1File.SaveAs(TempImage1);
						ImageResize.ResizeEntityOrObject("Product", TempImage1, Image1, "icon", "image/gif");
						ImageResize.DisposeOfTempImage(TempImage1);

						break;
					case "image/x-png":
					case "image/png":
						TempImage1 = AppLogic.GetImagePath("Product", "icon", true) + "tmp_" + FN + ".png";
						Image1 = AppLogic.GetImagePath("Product", "icon", true) + FN + ".png";
						Image1File.SaveAs(TempImage1);
						ImageResize.ResizeEntityOrObject("Product", TempImage1, Image1, "icon", "image/png");

						ImageResize.DisposeOfTempImage(TempImage1);
						break;
					case "image/jpg":
					case "image/jpeg":
					case "image/pjpeg":
						TempImage1 = AppLogic.GetImagePath("Product", "icon", true) + "tmp_" + FN + ".jpg";
						Image1 = AppLogic.GetImagePath("Product", "icon", true) + FN + ".jpg";
						Image1File.SaveAs(TempImage1);
						ImageResize.ResizeEntityOrObject("Product", TempImage1, Image1, "icon", "image/jpeg");
						ImageResize.DisposeOfTempImage(TempImage1);

						break;
				}
			}

			String Image2 = String.Empty;
			String TempImage2 = String.Empty;
			HttpPostedFile Image2File = fuMedium.PostedFile;
			if(Image2File != null && Image2File.ContentLength != 0)
			{
				// delete any current image file first
				try
				{
					foreach(String ss in CommonLogic.SupportedImageTypes)
					{
						System.IO.File.Delete(AppLogic.GetImagePath("Product", "medium", true) + FN + ss);
					}
				}
				catch
				{ }

				switch(Image2File.ContentType)
				{
					case "image/gif":

						TempImage2 = AppLogic.GetImagePath("Product", "medium", true) + "tmp_" + FN + ".gif";
						Image2 = AppLogic.GetImagePath("Product", "medium", true) + FN + ".gif";
						Image2File.SaveAs(TempImage2);
						ImageResize.ResizeEntityOrObject("Product", TempImage2, Image2, "medium", "image/gif");
						ImageResize.DisposeOfTempImage(TempImage2);

						break;
					case "image/x-png":
					case "image/png":

						TempImage2 = AppLogic.GetImagePath("Product", "medium", true) + "tmp_" + FN + ".png";
						Image2 = AppLogic.GetImagePath("Product", "medium", true) + FN + ".png";
						Image2File.SaveAs(TempImage2);
						ImageResize.ResizeEntityOrObject("Product", TempImage2, Image2, "medium", "image/png");
						ImageResize.DisposeOfTempImage(TempImage2);

						break;
					case "image/jpg":
					case "image/jpeg":
					case "image/pjpeg":

						TempImage2 = AppLogic.GetImagePath("Product", "medium", true) + "tmp_" + FN + ".jpg";
						Image2 = AppLogic.GetImagePath("Product", "medium", true) + FN + ".jpg";
						Image2File.SaveAs(TempImage2);
						ImageResize.ResizeEntityOrObject("Product", TempImage2, Image2, "medium", "image/jpeg");
						ImageResize.DisposeOfTempImage(TempImage2);

						break;
				}
			}

			String Image3 = String.Empty;
			String TempImage3 = String.Empty;
			HttpPostedFile Image3File = fuLarge.PostedFile;
			if(Image3File != null && Image3File.ContentLength != 0)
			{
				// delete any current image file first
				try
				{
					foreach(String ss in CommonLogic.SupportedImageTypes)
					{
						System.IO.File.Delete(AppLogic.GetImagePath("Product", "large", true) + FN + ss);
					}
				}
				catch
				{ }

				switch(Image3File.ContentType)
				{
					case "image/gif":

						TempImage3 = AppLogic.GetImagePath("Product", "large", true) + "tmp_" + FN + ".gif";
						Image3 = AppLogic.GetImagePath("Product", "large", true) + FN + ".gif";
						Image3File.SaveAs(TempImage3);
						ImageResize.ResizeEntityOrObject("Product", TempImage3, Image3, "large", "image/gif");
						ImageResize.CreateOthersFromLarge("Product", TempImage3, FN, "image/gif");

						break;
					case "image/x-png":
					case "image/png":

						TempImage3 = AppLogic.GetImagePath("Product", "large", true) + "tmp_" + FN + ".png";
						Image3 = AppLogic.GetImagePath("Product", "large", true) + FN + ".png";
						Image3File.SaveAs(TempImage3);
						ImageResize.ResizeEntityOrObject("Product", TempImage3, Image3, "large", "image/png");
						ImageResize.CreateOthersFromLarge("Product", TempImage3, FN, "image/png");

						break;
					case "image/jpg":
					case "image/jpeg":
					case "image/pjpeg":

						TempImage3 = AppLogic.GetImagePath("Product", "large", true) + "tmp_" + FN + ".jpg";
						Image3 = AppLogic.GetImagePath("Product", "large", true) + FN + ".jpg";
						Image3File.SaveAs(TempImage3);
						ImageResize.ResizeEntityOrObject("Product", TempImage3, Image3, "large", "image/jpeg");
						ImageResize.CreateOthersFromLarge("Product", TempImage3, FN, "image/jpeg");

						break;
				}
				ImageResize.DisposeOfTempImage(TempImage3);
			}
		}

		protected void SetVariantButtons()
		{
			ltVariantsLinks.Text = string.Format("<a target=\"entityBody\" href=\"variants.aspx?ProductID={0}\">Show/Edit/Add Variants</a>", ProductId);
		}

		protected void btnDeleteAll_Click(object sender, EventArgs e)
		{
			DB.ExecuteSQL("delete from KitCart where VariantID in (select VariantID from ProductVariant where ProductID=" + ProductId.ToString() + ")");
			DB.ExecuteSQL("delete from ShoppingCart where VariantID in (select VariantID from ProductVariant where ProductID=" + ProductId.ToString() + ")");
			DB.ExecuteSQL("delete from ProductVariant where ProductID=" + ProductId.ToString());

			LoadContent();
			ctrlAlertMessage.PushAlertMessage("All variants have been deleted", AlertMessage.AlertType.Success);
		}

		protected void NameLength_ServerValidate(object source, ServerValidateEventArgs args)
		{
			var nameFieldLength = DB.GetSqlN(
				@"select CHARACTER_MAXIMUM_LENGTH [N]
				from INFORMATION_SCHEMA.COLUMNS
				where TABLE_SCHEMA = 'dbo' and TABLE_NAME = 'Product' and COLUMN_NAME = 'Name'");

			// Always accept on a MAX length
			if(nameFieldLength == -1)
			{
				args.IsValid = true;
				return;
			}

			// Make sure the generated data is less than the field length
			var fullMlData = AppLogic.FormLocaleXml("Name", args.Value, Locale, "Product", ProductId);
			args.IsValid = fullMlData.Length <= nameFieldLength;

			if(!args.IsValid)
				ClientScript.RegisterStartupScript(this.GetType(), "Name Length Violation Popup", String.Format("alert('- {0}');", NameLengthValidator.ErrorMessage), true);
		}
	}
}
