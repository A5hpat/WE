// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class EntityEditor : AspDotNetStorefront.Admin.AdminPageBase
	{
		private bool EntityEditing
		{
			get { return (bool?)ViewState["EntityEdit"] ?? false; }
			set { ViewState["EntityEdit"] = value; }
		}

		protected int EntityId;
		protected string EntityName;
		EntityHelper EntityHelper;
		EntitySpecs EntitySpecs;
		bool UseHtmlEditor;
		SkinProvider SkinProvider;

		readonly ISearchEngineNameProvider SearchEngineNameProvider;

		public EntityEditor()
		{
			SearchEngineNameProvider = DependencyResolver.Current.GetService<ISearchEngineNameProvider>();
			SkinProvider = new SkinProvider();
		}

		protected override void OnInit(EventArgs e)
		{
			EntityId = CommonLogic.QueryStringNativeInt("EntityID");
			EntityName = CommonLogic.QueryStringCanBeDangerousContent("EntityName");
			EntitySpecs = EntityDefinitions.LookupSpecs(EntityName);
			EntityHelper = new EntityHelper(0, EntitySpecs, 0);
			UseHtmlEditor = !AppLogic.AppConfigBool("TurnOffHtmlEditorInAdminSite");

			base.OnInit(e);
		}

		protected void Page_Load(Object sender, EventArgs e)
		{
			if(AppLogic.MaxEntitiesExceeded())
				ctrlAlertMessage.PushAlertMessage("Maximum number of allowed entities exceeded. To add additional entities, please delete some entities or upgrade to AspDotNetStoreFront ML", AlertMessage.AlertType.Error);

			// Determine HTML editor configuration
			radSummary.Visible = radDescription.Visible = UseHtmlEditor;
			txtDescriptionNoHtmlEditor.Visible = txtSummaryNoHtmlEditor.Visible = !UseHtmlEditor;

			Tr2.Visible = AppLogic.AppConfigBool("TemplateSwitching.Enabled");

			etsMapper.ObjectID = EntityId;
			etsMapper.EntityType = EntityName;
			if(!IsPostBack)
			{
				etsMapper.DataBind();
				liStoreMappingTab.Visible = etsMapper.StoreCount > 1;

				if(!(EntityName.ToLowerInvariant() == "category" || EntityName.ToLowerInvariant() == "section" || EntityName.ToLowerInvariant() == "manufacturer"))
				{
					etsMapper.Visible = false;
					pnlStoreMapNotSupported.Visible = true;
				}
				
				LoadContent();
				liProductsTab.Visible = EntityId > 0;
			}
		}

		protected override void OnPreRender(EventArgs e)
		{
			DataBind();
			CancelLinkTop.DataBind();
			CancelLinkBottom.DataBind();
			ProductMappingLink.DataBind();

			if(QuickAddQuantityDiscounts.QuantityDiscountName != null)
				LoadDiscountTables(QuickAddQuantityDiscounts.QuantityDiscountId);

			base.OnPreRender(e);
		}

		protected void LocaleSelector_SelectedLocaleChanged(Object sender, EventArgs e)
		{
			if(bulkFrame.Attributes["src"] != null)
			{
				bulkFrame.Attributes["src"] = Regex.Replace(
					input: bulkFrame.Attributes["src"],
					pattern: "&locale\\.selection=[a-z]{2}-[A-Z]{2}",
					replacement: string.Format("&locale.selection={0}", LocaleSelector.GetSelectedLocale().Name));
			}

			LoadContent();
		}

		protected void btnSubmit_Click(Object sender, EventArgs e)
		{
			UpdateEntity();

			etsMapper.ObjectID = EntityId;
			etsMapper.Save();
		}

		protected void btnClose_Click(Object sender, EventArgs e)
		{
			UpdateEntity();

			etsMapper.ObjectID = EntityId;
			etsMapper.Save();

			Response.Redirect(ReturnUrlTracker.GetReturnUrl());
		}

		protected void lnkBulkDisplayOrder_Click(Object sender, EventArgs e)
		{
			SetBulkFrameSource("entityproductbulkdisplayorder.aspx");
		}

		protected void lnkBulkInventory_Click(Object sender, EventArgs e)
		{
			SetBulkFrameSource("entitybulkinventory.aspx");
		}

		protected void lnkBulkSEFields_Click(Object sender, EventArgs e)
		{
			SetBulkFrameSource("entitybulkse.aspx");
		}

		protected void lnkBulkPrices_Click(Object sender, EventArgs e)
		{
			SetBulkFrameSource("entitybulkprices.aspx");
		}

		protected void lnkBulkShippingMethods_Click(Object sender, EventArgs e)
		{
			SetBulkFrameSource("entitybulkshipping.aspx");
		}

		protected void lnkBulkDownloadFiles_Click(Object sender, EventArgs e)
		{
			SetBulkFrameSource("entitybulkdownloadfiles.aspx");
		}

		void SetBulkFrameSource(string page)
		{
			var locale = LocaleSelector
				.GetSelectedLocale()
				.Name;

			bulkFrame.Attributes.Add("src", String.Format("{0}?EntityName={1}&EntityId={2}&locale.selection={3}", page, EntityName, EntityId, locale));
		}

		void LoadContent()
		{
			ddCountry.Items.Clear();
			ddParent.Items.Clear();
			ddState.Items.Clear();

			var skinId = 1;

			var locale = LocaleSelector
				.GetSelectedLocale()
				.Name;

			using(var connection = new SqlConnection(DB.GetDBConn()))
			{
				connection.Open();

				var entitySql = String.Format("select *, (select count(*) from {0} with(nolock) where Parent{0}ID = e.{0}ID) as ChildEntityCount from {0} e with(nolock) where {0}ID = {1}", EntitySpecs.m_EntityName, EntityId);
				using(var entityReader = DB.GetRS(entitySql, connection))
				{
					if(entityReader.Read())
						EntityEditing = true;

					rfvName.ErrorMessage = String.Format("Please enter the {0} name (Main Tab)", AppLogic.GetString("AppConfig." + EntitySpecs.m_EntityName + "PromptSingular", skinId, ThisCustomer.LocaleSetting).ToLowerInvariant());

					trParent.Visible = false;
					trSubEntities.Visible = false;
					if(EntitySpecs.m_HasParentChildRelationship)
					{
						// Load the parent dropdown
						trParent.Visible = true;
						ddParent.Items.Add(new ListItem("No Parent", "0"));
						foreach(ListItemClass lic in EntityHelper.GetEntityArrayList(0, "", 0, ThisCustomer.LocaleSetting, false))
							ddParent.Items.Add(new ListItem(String.Format("{0} (Id: {1})", HttpUtility.HtmlDecode(lic.Item), lic.Value), lic.Value.ToString()));

						// Setup the display order link
						subEntityDisplayOrderLabel.Text = AppLogic.GetString(string.Format("admin.sub{0}displayorderlabel", EntityName));
						trSubEntities.Visible = DB.RSFieldInt(entityReader, "ChildEntityCount") > 0;
						DisplayOrderLink.NavigateUrl = string.Format("entitybulkdisplayorder.aspx?entitytype={0}&entityid={1}", EntityName, EntityId);
					}

					LoadDiscountTables(0);

					trBrowser.Visible = false; // We're removing the availablity of this unused feature and will remove related schema and assets in a future version.

					// Address
					phAddress.Visible = false;
					if(EntitySpecs.m_HasAddress)
					{
						if(EntityName.ToUpperInvariant().Equals("DISTRIBUTOR"))
						{
							lblEmailRequired.Visible = true;
							rfvEmail.Visible = true;
						}

						phAddress.Visible = true;

						ddState.Items.Add(new ListItem("SELECT ONE", "0"));

						using(var innerConnection = new SqlConnection(DB.GetDBConn()))
						{
							innerConnection.Open();

							using(var reader = DB.GetRS("select * from state   with (NOLOCK)  order by DisplayOrder,Name", innerConnection))
								while(reader.Read())
									ddState.Items.Add(new ListItem(DB.RSField(reader, "Name"), DB.RSField(reader, "Abbreviation")));

							using(var reader = DB.GetRS("select * from country with (NOLOCK) order by DisplayOrder,Name", innerConnection))
								while(reader.Read())
									ddCountry.Items.Add(new ListItem(DB.RSField(reader, "Name"), DB.RSField(reader, "Name")));
						}
					}

					var defaultSkinName = SkinProvider.GetSkinNameById(Store.GetDefaultStore().SkinID);

					if(EntityEditing)
					{
						liProductsTab.Visible = EntityId > 0;

						//SET Product Buttons
						SetProductButtons();

						ltEntity.Text = EntityHelper.GetEntityBreadcrumb6(EntityId, ThisCustomer.LocaleSetting);

						var localeName = XmlCommon.GetLocaleEntry(DB.RSField(entityReader, "Name"), locale, false);
						Title
							= HeaderText.Text
							= String.Format(
								"{0} {1}",
								"admin.common.edit".StringResource(),
								String.Format("admin.common.{0}", EntityName).StringResource());

						header.Visible = !String.IsNullOrEmpty(localeName);
						txtName.Text = localeName;

						if(!DB.RSFieldBool(entityReader, "Published"))
							rblPublished.BackColor = Color.LightYellow;

						rblPublished.SelectedIndex = (DB.RSFieldBool(entityReader, "Published") ? 1 : 0);

						if(EntitySpecs.m_EntityName == "Category" || EntitySpecs.m_EntityName == "Section")
							rblBrowser.SelectedIndex = (DB.RSFieldBool(entityReader, "ShowIn" + EntitySpecs.m_ObjectName + "Browser") ? 1 : 0);

						ddParent.SelectFirstByValue(DB.RSFieldInt(entityReader, "Parent" + EntitySpecs.m_EntityName + "ID").ToString());
						ddXmlPackage.SelectFirstByValue(DB.RSField(entityReader, "XmlPackage"), StringComparer.OrdinalIgnoreCase);
						ddDiscountTable.SelectFirstByValue(DB.RSFieldInt(entityReader, "QuantityDiscountID").ToString());

						txtPageSize.Text = EntityEditing
							? DB.RSFieldInt(entityReader, "PageSize").ToString()
							: AppLogic.AppConfig("Default_" + EntitySpecs.m_EntityName + "PageSize");

						txtColumn.Text = EntityEditing
							? DB.RSFieldInt(entityReader, "ColWidth").ToString()
							: AppLogic.AppConfig("Default_" + EntitySpecs.m_EntityName + "ColWidth");

						rblLooks.SelectedIndex = DB.RSFieldBool(entityReader, "SortByLooks")
							? 1
							: 0;

						if(EntitySpecs.m_HasAddress)
						{
							txtAddress1.Text = EntityEditing
								? DB.RSField(entityReader, "Address1")
								: String.Empty;

							txtApt.Text = EntityEditing
								? DB.RSField(entityReader, "Suite")
								: String.Empty;

							txtAddress2.Text = EntityEditing
								? DB.RSField(entityReader, "Address2")
								: String.Empty;

							txtCity.Text = EntityEditing
								? DB.RSField(entityReader, "City")
								: String.Empty;

							ddState.SelectFirstByValue(DB.RSField(entityReader, "State"));

							txtZip.Text = EntityEditing
								? DB.RSField(entityReader, "ZipCode")
								: String.Empty;

							ddCountry.SelectFirstByValue(DB.RSField(entityReader, "Country"));

							txtURL.Text = EntityEditing
								? DB.RSField(entityReader, "URL")
								: String.Empty;

							txtEmail.Text = EntityEditing
								? DB.RSField(entityReader, "EMail")
								: CommonLogic.QueryStringCanBeDangerousContent("EMail");

							txtPhone.Text = EntityEditing
								? CommonLogic.GetPhoneDisplayFormat(DB.RSField(entityReader, "Phone"))
								: String.Empty;

							txtFax.Text = EntityEditing
								? CommonLogic.GetPhoneDisplayFormat(DB.RSField(entityReader, "Fax"))
								: String.Empty;
						}

						// Begin images 
						txtImageOverride.Text = EntityEditing
							? DB.RSField(entityReader, "ImageFilenameOverride")
							: String.Empty;

						var disableupload = EntityEditing && DB.RSField(entityReader, "ImageFilenameOverride") != "";

						if(EntitySpecs.m_HasIconPic)
						{
							fuIcon.Enabled = !disableupload;
							var imageUrl = AppLogic.LookupImage(EntitySpecs.m_EntityName, EntityId, "icon", skinId, ThisCustomer.LocaleSetting);

							if(imageUrl.Length == 0)
								imageUrl = AppLogic.NoPictureImageURL(false, skinId, ThisCustomer.LocaleSetting);
							
							if(!CommonLogic.FileExists(imageUrl))
								imageUrl = AppLogic.LocateImageURL(string.Format("Skins/{0}/images/nopictureicon.gif", defaultSkinName), ThisCustomer.LocaleSetting);

							if(imageUrl.Length != 0)
							{
								ltIcon.Text = "";
								if(imageUrl.IndexOf("nopicture") == -1)
									ltIcon.Text = "<a href=\"javascript:void(0);\" onClick=\"DeleteImage('" + imageUrl + "','Pic1');\">Click here</a> to delete the current image <br/>\n";

								ltIcon.Text += "<img style=\"margin-top: 3px; margin-bottom: 5px;\" id=\"Pic1\" name=\"Pic1\" border=\"0\" src=\"" + imageUrl + "?" + CommonLogic.GetRandomNumber(1, 1000000).ToString() + "\" />\n";
							}
						}

						if(EntitySpecs.m_HasMediumPic)
						{
							fuMedium.Enabled = !disableupload;
							var imageUrl = AppLogic.LookupImage(EntitySpecs.m_EntityName, EntityId, "medium", skinId, ThisCustomer.LocaleSetting);

							if(imageUrl.Length == 0)
								imageUrl = AppLogic.NoPictureImageURL(false, skinId, ThisCustomer.LocaleSetting);

							if(!CommonLogic.FileExists(imageUrl))
								imageUrl = AppLogic.LocateImageURL(string.Format("Skins/{0}/images/nopicture.gif", defaultSkinName), ThisCustomer.LocaleSetting);

							if(imageUrl.Length != 0)
							{
								ltMedium.Text = "";
								if(imageUrl.IndexOf("nopicture") == -1)
									ltMedium.Text = "<a href=\"javascript:void(0);\" onClick=\"DeleteImage('" + imageUrl + "','Pic2');\">Click here</a> to delete the current image <br/>\n";

								ltMedium.Text += "<img style=\"margin-top: 3px; margin-bottom: 5px;\" id=\"Pic2\" name=\"Pic2\" border=\"0\" src=\"" + imageUrl + "?" + CommonLogic.GetRandomNumber(1, 1000000).ToString() + "\" />\n";
							}
						}

						if(EntitySpecs.m_HasLargePic)
						{
							fuLarge.Enabled = !disableupload;
							var imageUrl = AppLogic.LookupImage(EntitySpecs.m_EntityName, EntityId, "large", skinId, ThisCustomer.LocaleSetting);

							if(imageUrl.Length == 0)
								imageUrl = AppLogic.NoPictureImageURL(false, skinId, ThisCustomer.LocaleSetting);

							if(!CommonLogic.FileExists(imageUrl))
								imageUrl = AppLogic.LocateImageURL(string.Format("Skins/{0}/images/nopicture.gif", defaultSkinName), ThisCustomer.LocaleSetting);

							if(imageUrl.Length != 0)
							{
								ltLarge.Text = "";
								if(imageUrl.IndexOf("nopicture") == -1)
									ltLarge.Text = "<a href=\"javascript:void(0);\" onClick=\"DeleteImage('" + imageUrl + "','Pic3');\">Click here</a> to delete the current image <br/>\n";

								ltLarge.Text += "<img style=\"margin-top: 3px; margin-bottom: 5px;\" id=\"Pic3\" name=\"Pic3\" border=\"0\" src=\"" + imageUrl + "?" + CommonLogic.GetRandomNumber(1, 1000000).ToString() + "\" />\n";
							}
						}
						// End images

						if(UseHtmlEditor)
						{
							radDescription.Content = DB.RSFieldByLocale(entityReader, "Description", locale);
						}
						else
						{
							txtDescriptionNoHtmlEditor.Text = XmlCommon.GetLocaleEntry(DB.RSField(entityReader, "Description"), locale, false);
						}

						if(UseHtmlEditor)
						{
							radSummary.Content = DB.RSFieldByLocale(entityReader, "Summary", locale);
						}
						else
						{
							txtSummaryNoHtmlEditor.Text = XmlCommon.GetLocaleEntry(DB.RSField(entityReader, "Summary"), locale, false);
						}

						txtExtensionData.Text = EntityEditing
							? DB.RSField(entityReader, "ExtensionData")
							: String.Empty;

						txtUseSkinTemplateFile.Text = EntityEditing
							? DB.RSField(entityReader, "TemplateName")
							: String.Empty;

						txtUseSkinID.Text =
							(EntityEditing
								? DB.RSFieldInt(entityReader, "SkinID")
								: 0)
							.ToString();

						if(txtUseSkinID.Text == "0")
							txtUseSkinID.Text = String.Empty;

						txtSETitle.Text = XmlCommon.GetLocaleEntry(DB.RSField(entityReader, "SETitle"), locale, false);
						txtSEKeywords.Text = XmlCommon.GetLocaleEntry(DB.RSField(entityReader, "SEKeywords"), locale, false);
						txtSEDescription.Text = XmlCommon.GetLocaleEntry(DB.RSField(entityReader, "SEDescription"), locale, false);
						txtSEAlt.Text = XmlCommon.GetLocaleEntry(DB.RSField(entityReader, "SEAltText"), locale, false);
					}
					else
					{
						btnSubmit.Enabled
							= btnSubmitBottom.Enabled
							= !AppLogic.MaxEntitiesExceeded();

						Title
							= HeaderText.Text
							= String.Format(
								"{0} {1}",
								"admin.common.addnew".StringResource(),
								String.Format("admin.common.{0}", EntityName).StringResource());

						txtPageSize.Text = AppLogic.AppConfig("Default_" + EntitySpecs.m_EntityName + "PageSize");
						txtColumn.Text = AppLogic.AppConfig("Default_" + EntitySpecs.m_EntityName + "ColWidth");

						var parentID = CommonLogic.QueryStringNativeInt("entityparent");
						if(parentID > 0)
						{
							ddParent.SelectedValue = parentID.ToString();
							ltEntity.Text = EntityHelper.GetEntityBreadcrumb6(parentID, ThisCustomer.LocaleSetting);
						}
						else
						{
							ltEntity.Text = "Root Level";
						}


						txtExtensionData.Text = EntityEditing
							? DB.RSField(entityReader, "ExtensionData")
							: String.Empty;
					}
				}
			}
		}

		void LoadDiscountTables(int quantityDiscountId)
		{
			ddDiscountTable.Items.Clear();
			ddDiscountTable.Items.Add(new ListItem("None", "0"));

			using(var conn = new SqlConnection(DB.GetDBConn()))
			{
				conn.Open();
				using(var reader = DB.GetRS("select Name, QuantityDiscountID from QuantityDiscount   with (NOLOCK)  order by DisplayOrder,Name", conn))
					while(reader.Read())
						ddDiscountTable.Items.Add(new ListItem(DB.RSFieldByLocale(reader, "Name", ThisCustomer.LocaleSetting), DB.RSFieldInt(reader, "QuantityDiscountID").ToString()));
			}

			if(quantityDiscountId > 0)
				ddDiscountTable.SelectedValue = quantityDiscountId.ToString();
		}

		void UpdateEntity()
		{
			var sql = new StringBuilder(2500);
			var parentId = Localization.ParseNativeInt(ddParent.SelectedValue);
			if(parentId == EntityId) // prevent case which causes endless recursion
				parentId = 0;

			var locale = LocaleSelector
				.GetSelectedLocale()
				.Name;

			try
			{
				if(!EntityEditing)
				{
					var newGuid = DB.GetNewGUID();
					sql.AppendFormat(
						@"insert into {0} (
							{0}GUID, 
							Name,
							SEName,
							{1}
							TemplateName,
							SkinID,
							ImageFilenameOverride,
							{2}
							Summary,
							Description,
							ExtensionData,
							SEKeywords,
							SEDescription,
							SETitle,
							SEAltText,
							Published,
							{3}
							PageSize,
							ColWidth,
							XmlPackage,
							SortByLooks,
							QuantityDiscountID)",
						EntitySpecs.m_EntityName,
						EntitySpecs.m_HasAddress
							? "Address1,Address2,Suite,City,State,ZipCode,Country,Phone,FAX,URL,EMail,"
							: String.Empty,
						EntitySpecs.m_HasParentChildRelationship
							? String.Format("Parent{0}ID,", EntitySpecs.m_EntityName)
							: String.Empty,

						EntitySpecs.m_EntityName == "Category" || EntitySpecs.m_EntityName == "Section"
							? String.Format("ShowIn{0}Browser,", EntitySpecs.m_ObjectName)
							: String.Empty);

					sql.Append("values(");
					sql.AppendFormat("{0},", DB.SQuote(newGuid));
					sql.AppendFormat("{0},", DB.SQuote(AppLogic.FormLocaleXml("Name", txtName.Text, locale, EntitySpecs, EntityId)));
					sql.AppendFormat("{0},", DB.SQuote(SearchEngineNameProvider.GenerateSeName(AppLogic.GetFormsDefaultLocale("Name", txtName.Text, locale, EntitySpecs, EntityId))));

					if(EntitySpecs.m_HasAddress)
					{
						if(txtAddress1.Text.Length != 0)
							sql.Append(DB.SQuote(txtAddress1.Text) + ",");
						else
							sql.Append("NULL,");

						if(txtAddress2.Text.Length != 0)
							sql.Append(DB.SQuote(txtAddress2.Text) + ",");
						else
							sql.Append("NULL,");

						if(txtApt.Text.Length != 0)
							sql.Append(DB.SQuote(txtApt.Text) + ",");
						else
							sql.Append("NULL,");

						if(txtCity.Text.Length != 0)
							sql.Append(DB.SQuote(txtCity.Text) + ",");
						else
							sql.Append("NULL,");

						if(!ddState.SelectedValue.Equals("0"))
							sql.Append(DB.SQuote(ddState.SelectedValue) + ",");
						else
							sql.Append("NULL,");

						if(txtZip.Text.Length != 0)
							sql.Append(DB.SQuote(txtZip.Text) + ",");
						else
							sql.Append("NULL,");

						if(!ddCountry.SelectedValue.Equals("0"))
							sql.Append(DB.SQuote(ddCountry.SelectedValue) + ",");
						else
							sql.Append("NULL,");

						if(txtPhone.Text.Length != 0)
							sql.Append(DB.SQuote(AppLogic.MakeProperPhoneFormat(txtPhone.Text)) + ",");
						else
							sql.Append("NULL,");

						if(txtFax.Text.Length != 0)
							sql.Append(DB.SQuote(AppLogic.MakeProperPhoneFormat(txtFax.Text)) + ",");
						else
							sql.Append("NULL,");

						if(txtURL.Text.Length != 0)
						{
							var url = CommonLogic.Left(txtURL.Text, 80);
							if(url.IndexOf("http://") == -1 && url.Length != 0)
								url = "http://" + url;

							if(url.Length == 0)
								sql.Append("NULL,");
							else
								sql.Append(DB.SQuote(url) + ",");
						}
						else
							sql.Append("NULL,");

						if(txtEmail.Text.Length != 0)
							sql.Append(DB.SQuote(CommonLogic.Left(txtEmail.Text, 100)) + ",");
						else
							sql.Append("NULL,");
					}

					sql.Append(DB.SQuote(txtUseSkinTemplateFile.Text) + ",");
					sql.AppendFormat("{0},", Localization.ParseUSInt(txtUseSkinID.Text));
					sql.Append(DB.SQuote(txtImageOverride.Text) + ",");

					if(EntitySpecs.m_HasParentChildRelationship)
						sql.Append(parentId.ToString() + ",");

					if(UseHtmlEditor)
					{
						sql.Append(DB.SQuote(AppLogic.FormLocaleXml("Summary", radSummary.Content, locale, EntitySpecs, EntityId)) + ",");
						sql.Append(DB.SQuote(AppLogic.FormLocaleXml("Description", radDescription.Content, locale, EntitySpecs, EntityId)) + ",");
					}
					else
					{
						sql.Append(DB.SQuote(AppLogic.FormLocaleXmlEditor("Summary", txtSummaryNoHtmlEditor.Text.Trim(), locale, EntitySpecs, EntityId)) + ",");
						sql.Append(DB.SQuote(AppLogic.FormLocaleXmlEditor("Description", txtDescriptionNoHtmlEditor.Text.Trim(), locale, EntitySpecs, EntityId)) + ",");
					}

					sql.Append(DB.SQuote(txtExtensionData.Text) + ",");

					if(txtSEKeywords.Text.Length != 0)
						sql.Append(DB.SQuote(AppLogic.FormLocaleXml("SEKeywords", txtSEKeywords.Text, locale, EntitySpecs, EntityId)) + ",");
					else
						sql.Append("NULL,");

					if(txtSEDescription.Text.Length != 0)
						sql.Append(DB.SQuote(AppLogic.FormLocaleXml("SEDescription", txtSEDescription.Text, locale, EntitySpecs, EntityId)) + ",");
					else
						sql.Append("NULL,");

					if(txtSETitle.Text.Length != 0)
						sql.Append(DB.SQuote(AppLogic.FormLocaleXml("SETitle", txtSETitle.Text, locale, EntitySpecs, EntityId)) + ",");
					else
						sql.Append("NULL,");

					if(txtSEAlt.Text.Length != 0)
						sql.Append(DB.SQuote(AppLogic.FormLocaleXml("SEAltText", txtSEAlt.Text, locale, EntitySpecs, EntityId)) + ",");
					else
						sql.Append(DB.SQuote(AppLogic.FormLocaleXml("SEAltText", txtName.Text, locale, EntitySpecs, EntityId)) + ",");

					sql.Append(Localization.ParseNativeInt(rblPublished.SelectedValue) + ",");
					if(EntitySpecs.m_EntityName == "Category" || EntitySpecs.m_EntityName == "Section")
						sql.Append(Localization.ParseNativeInt(rblBrowser.SelectedValue) + ",");

					sql.AppendFormat(
						"{0},",
						txtPageSize.Text.Length == 0
							? AppLogic.AppConfigUSInt("Default_" + EntitySpecs.m_EntityName + "PageSize").ToString()
							: txtPageSize.Text);

					sql.AppendFormat(
						"{0},",
						txtColumn.Text.Length == 0
							? AppLogic.AppConfigUSInt("Default_" + EntitySpecs.m_EntityName + "ColWidth").ToString()
							: txtColumn.Text);

					sql.AppendFormat(
						"{0},",
						String.IsNullOrEmpty(ddXmlPackage.SelectedValue)
							? DB.SQuote(AppLogic.ro_DefaultEntityXmlPackage)	// force a default!
							: DB.SQuote(ddXmlPackage.SelectedValue.ToLower()));

					sql.Append(rblLooks.SelectedValue + ",");
					sql.Append(ddDiscountTable.SelectedValue);
					sql.Append(")");

					DB.ExecuteSQL(sql.ToString());

					using(var connection = new SqlConnection(DB.GetDBConn()))
					{
						connection.Open();
						using(var dataReader = DB.GetRS("select " + EntitySpecs.m_EntityName + "ID from " + EntitySpecs.m_EntityName + " with (NOLOCK) where Deleted=0 and " + EntitySpecs.m_EntityName + "GUID=" + DB.SQuote(newGuid), connection))
						{
							dataReader.Read();
							EntityId = DB.RSFieldInt(dataReader, EntitySpecs.m_EntityName + "ID");
						}
					}

					HandleImageSubmits();
					var entityDisplayName = String.Format("admin.common.{0}", EntityName).StringResource().ToLower();
					ctrlAlertMessage.PushAlertMessage(String.Format("The {0} was created successfully", entityDisplayName), AlertMessage.AlertType.Success);
					Response.Redirect(String.Format("entity.aspx?entityid={0}&entityname={1}", EntityId, EntityName));
				}
				else
				{
					// ok to update:
					sql.Append("update " + EntitySpecs.m_EntityName + " set ");
					sql.Append("Name=" + DB.SQuote(AppLogic.FormLocaleXml("Name", txtName.Text, locale, EntitySpecs, EntityId)) + ",");
					sql.Append("SEName=" + DB.SQuote(SearchEngineNameProvider.GenerateSeName(AppLogic.GetFormsDefaultLocale("Name", txtName.Text, locale, EntitySpecs, EntityId))) + ",");

					if(EntitySpecs.m_HasAddress)
					{
						if(txtAddress1.Text.Length != 0)
							sql.Append("Address1=" + DB.SQuote(txtAddress1.Text) + ",");
						else
							sql.Append("Address1=NULL,");

						if(txtAddress2.Text.Length != 0)
							sql.Append("Address2=" + DB.SQuote(txtAddress2.Text) + ",");
						else
							sql.Append("Address2=NULL,");

						if(txtApt.Text.Length != 0)
							sql.Append("Suite=" + DB.SQuote(txtApt.Text) + ",");
						else
							sql.Append("Suite=NULL,");

						if(txtCity.Text.Length != 0)
							sql.Append("City=" + DB.SQuote(txtCity.Text) + ",");
						else
							sql.Append("City=NULL,");

						if(!ddState.SelectedValue.Equals("0"))
							sql.Append("State=" + DB.SQuote(ddState.SelectedValue) + ",");
						else
							sql.Append("State=NULL,");

						if(txtZip.Text.Length != 0)
							sql.Append("ZipCode=" + DB.SQuote(txtZip.Text) + ",");
						else
							sql.Append("ZipCode=NULL,");

						if(!ddCountry.SelectedValue.Equals("0"))
							sql.Append("Country=" + DB.SQuote(ddCountry.SelectedValue) + ",");
						else
							sql.Append("Country=NULL,");

						if(txtPhone.Text.Length != 0)
							sql.Append("Phone=" + DB.SQuote(AppLogic.MakeProperPhoneFormat(txtPhone.Text)) + ",");
						else
							sql.Append("Phone=NULL,");

						if(txtFax.Text.Length != 0)
							sql.Append("FAX=" + DB.SQuote(AppLogic.MakeProperPhoneFormat(txtFax.Text)) + ",");
						else
							sql.Append("FAX=NULL,");

						if(txtURL.Text.Length != 0)
						{
							var url = CommonLogic.Left(txtURL.Text, 80);
							if(url.IndexOf("http://") == -1 && url.Length != 0)
								url = "http://" + url;

							if(url.Length != 0)
								sql.Append("URL=" + DB.SQuote(url) + ",");
							else
								sql.Append("URL=NULL,");
						}
						else
							sql.Append("URL=NULL,");

						if(txtEmail.Text.Length != 0)
							sql.Append("EMail=" + DB.SQuote(CommonLogic.Left(txtEmail.Text, 100)) + ",");
						else
							sql.Append("EMail=NULL,");
					}

					sql.Append("TemplateName=" + DB.SQuote(txtUseSkinTemplateFile.Text) + ",");
					sql.AppendFormat("SkinID={0},", Localization.ParseUSInt(txtUseSkinID.Text));
					sql.Append("ImageFilenameOverride=" + DB.SQuote(txtImageOverride.Text) + ",");
					if(EntitySpecs.m_HasParentChildRelationship)
						sql.Append("Parent" + EntitySpecs.m_EntityName + "ID=" + parentId.ToString() + ",");

					if(UseHtmlEditor)
					{
						sql.Append("Summary=" + DB.SQuote(AppLogic.FormLocaleXml("Summary", radSummary.Content, locale, EntitySpecs, EntityId)) + ",");
						sql.Append("Description=" + DB.SQuote(AppLogic.FormLocaleXml("Description", radDescription.Content, locale, EntitySpecs, EntityId)) + ",");
					}
					else
					{
						sql.Append("Summary=" + DB.SQuote(AppLogic.FormLocaleXml("Summary", txtSummaryNoHtmlEditor.Text.Trim(), locale, EntitySpecs, EntityId)) + ",");
						sql.Append("Description=" + DB.SQuote(AppLogic.FormLocaleXml("Description", txtDescriptionNoHtmlEditor.Text.Trim(), locale, EntitySpecs, EntityId)) + ",");
					}
					sql.Append("ExtensionData=" + DB.SQuote(txtExtensionData.Text) + ",");

					sql.Append("SEKeywords=" + DB.SQuote(AppLogic.FormLocaleXml("SEKeywords", txtSEKeywords.Text, locale, EntitySpecs, EntityId)) + ",");
					sql.Append("SEDescription=" + DB.SQuote(AppLogic.FormLocaleXml("SEDescription", txtSEDescription.Text, locale, EntitySpecs, EntityId)) + ",");
					sql.Append("SETitle=" + DB.SQuote(AppLogic.FormLocaleXml("SETitle", txtSETitle.Text, locale, EntitySpecs, EntityId)) + ",");
					sql.Append("SEAltText=" + DB.SQuote(AppLogic.FormLocaleXml("SEAltText", txtSEAlt.Text, locale, EntitySpecs, EntityId)) + ",");

					sql.Append("Published=" + rblPublished.SelectedValue + ",");
					if(EntitySpecs.m_EntityName == "Category" || EntitySpecs.m_EntityName == "Section")
						sql.Append("ShowIn" + EntitySpecs.m_ObjectName + "Browser=" + Localization.ParseNativeInt(rblBrowser.SelectedValue) + ",");

					sql.AppendFormat(
						"PageSize={0},",
						txtPageSize.Text.Length == 0
							? AppLogic.AppConfig("Default_" + EntitySpecs.m_EntityName + "PageSize")
							: txtPageSize.Text);

					sql.AppendFormat(
						"ColWidth={0},",
						txtColumn.Text.Length == 0
							? AppLogic.AppConfig("Default_" + EntitySpecs.m_EntityName + "ColWidth")
							: txtColumn.Text);

					sql.AppendFormat(
						"XmlPackage={0},",
						String.IsNullOrEmpty(ddXmlPackage.SelectedValue)
							? AppLogic.ro_DefaultEntityXmlPackage
							: DB.SQuote(ddXmlPackage.SelectedValue.ToLower()));

					sql.Append("SortByLooks=" + rblLooks.SelectedValue + ",");
					sql.Append("QuantityDiscountID=" + ddDiscountTable.SelectedValue);
					sql.Append(" where " + EntitySpecs.m_EntityName + "ID=" + EntityId.ToString());

					DB.ExecuteSQL(sql.ToString());
					var entityDisplayName = String.Format("admin.common.{0}", EntityName).StringResource().ToLower();
					ctrlAlertMessage.PushAlertMessage(String.Format("The {0} was updated successfully", entityDisplayName), AlertMessage.AlertType.Success);
					EntityEditing = true;

					HandleImageSubmits();

					//refresh the static entityhelper
					switch(EntitySpecs.m_EntityName.ToUpperInvariant())
					{
						case "CATEGORY":
							AppLogic.CategoryStoreEntityHelper[0] = new EntityHelper(0, EntityDefinitions.LookupSpecs("Category"), true, 0);
							break;
						case "SECTION":
							AppLogic.SectionStoreEntityHelper[0] = new EntityHelper(0, EntityDefinitions.LookupSpecs("Section"), true, 0);
							break;
						case "MANUFACTURER":
							AppLogic.ManufacturerStoreEntityHelper[0] = new EntityHelper(0, EntityDefinitions.LookupSpecs("Manufacturer"), true, 0);
							break;
						case "DISTRIBUTOR":
							AppLogic.DistributorStoreEntityHelper[0] = new EntityHelper(0, EntityDefinitions.LookupSpecs("Distributor"), true, 0);
							break;
						case "GENRE":
							AppLogic.GenreStoreEntityHelper[0] = new EntityHelper(0, EntityDefinitions.LookupSpecs("Genre"), true, 0);
							break;
						case "VECTOR":
							AppLogic.VectorStoreEntityHelper[0] = new EntityHelper(0, EntityDefinitions.LookupSpecs("VECTOR"), true, 0);
							break;
						case "LIBRARY":
							AppLogic.LibraryStoreEntityHelper[0] = new EntityHelper(0, EntityDefinitions.LookupSpecs("Library"), true, 0);
							break;
					}

					LoadContent();
				}
			}
			catch(System.Threading.ThreadAbortException)
			{
				// Intentionally caused by Response.Redirect()
				throw;
			}
			catch(Exception ex)
			{
				ctrlAlertMessage.PushAlertMessage(String.Format("Error updating {0}: {1}", txtName.Text, ex), AlertMessage.AlertType.Error);
			}
		}

		void HandleImageSubmits()
		{
			var filename = txtImageOverride.Text.Trim();
			if(filename.Length == 0)
				filename = EntityId.ToString();

			if(EntitySpecs.m_HasIconPic && fuIcon.PostedFile != null && fuIcon.PostedFile.ContentLength != 0)
				HandleImage(filename, "icon", fuIcon.PostedFile);

			if(EntitySpecs.m_HasMediumPic && fuMedium.PostedFile != null && fuMedium.PostedFile.ContentLength != 0)
				HandleImage(filename, "medium", fuMedium.PostedFile);

			if(EntitySpecs.m_HasLargePic && fuLarge.PostedFile != null && fuLarge.PostedFile.ContentLength != 0)
				HandleImage(filename, "large", fuLarge.PostedFile);
		}

		void HandleImage(string filename, string imageSize, HttpPostedFile imageFile)
		{
			string targetMimeType, targetExtension;
			switch(imageFile.ContentType)
			{
				case "image/gif":
					targetExtension = ".gif";
					targetMimeType = "image/gif";
					break;

				case "image/x-png":
				case "image/png":
					targetExtension = ".png";
					targetMimeType = "image/png";
					break;

				case "image/jpg":
				case "image/jpeg":
				case "image/pjpeg":
					targetExtension = ".jpg";
					targetMimeType = "image/jpeg";
					break;

				default:
					ctrlAlertMessage.PushAlertMessage(String.Format("The type of the {0} image is not supported. Please upload a .jpg, .gif, or .png image.", imageSize), AlertMessage.AlertType.Error);
					return;
			}

			// Delete any current image file first
			try
			{
				foreach(var imageType in CommonLogic.SupportedImageTypes)
					File.Delete(String.Format("{0}{1}{2}", AppLogic.GetImagePath(EntitySpecs.m_EntityName, imageSize, true), filename, imageType));
			}
			catch { }

			var tempImagePath = String.Format("{0}tmp_{1}{2}", AppLogic.GetImagePath(EntitySpecs.m_EntityName, imageSize, true), filename, targetExtension);
			var imagePath = String.Format("{0}{1}{2}", AppLogic.GetImagePath(EntitySpecs.m_EntityName, imageSize, true), filename, targetExtension);
			imageFile.SaveAs(tempImagePath);
			ImageResize.ResizeEntityOrObject(EntitySpecs.m_EntityName, tempImagePath, imagePath, imageSize, targetMimeType);

			if(imageSize == "large")
				ImageResize.CreateOthersFromLarge(EntitySpecs.m_EntityName, tempImagePath, filename, targetMimeType);

			ImageResize.DisposeOfTempImage(tempImagePath);
		}

		void SetProductButtons()
		{
			var entityDisplayName = String.Format("admin.common.{0}", EntityName).StringResource();
			ProductMappingLink.Text = String.Format("{0} for {1}", EntitySpecs.m_ObjectNamePlural, entityDisplayName);

			if(Shipping.GetActiveShippingCalculationID() != Shipping.ShippingCalculationEnum.UseIndividualItemShippingCosts)
				lnkBulkShippingMethods.Visible = false;
		}
	}
}
