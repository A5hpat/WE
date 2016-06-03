// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontControls
{
	public partial class QuickAddEntity : BaseControl
	{
		#region Properties
		public string EntityType { get; set; }
		public int EntityId { get; private set; }
		public string EntityName { get; private set; }
		#endregion

		#region Fields
		private EntityHelper entityHelper;
		private EntitySpecs entitySpecs;
		#endregion

		readonly ISearchEngineNameProvider SearchEngineNameProvider;

		public QuickAddEntity()
		{
			SearchEngineNameProvider = DependencyResolver.Current.GetService<ISearchEngineNameProvider>();
		}

		#region Event Handlers

		protected void Page_Load(Object sender, EventArgs e)
		{

			InitializeEntityTypes();

			if(EntityType.ToUpperInvariant() == "CATEGORY")
				LoadParents();
			else
				pnlParent.Visible = false;
		}

		protected void btnSubmit_Click(Object sender, EventArgs e)
		{
			//only validates this button's validationgroup
			if(txtName.Text.Length > 0 && Page.IsValid)
				CreateEntity();
		}

		#endregion

		#region Protected Methods
		private void InitializeEntityTypes()
		{
			entitySpecs = EntityDefinitions.LookupSpecs(EntityType);

			switch(EntityType.ToUpperInvariant())
			{
				case "MANUFACTURER":
					InitializeClientSideHelpers("Manufacturer");
					entityHelper = AppLogic.ManufacturerStoreEntityHelper[0];
					break;
				case "DISTRIBUTOR":
					ShowEmailFields();
					InitializeClientSideHelpers("Distributor");
					entityHelper = AppLogic.DistributorStoreEntityHelper[0];
					break;
				default:
					InitializeClientSideHelpers("Category");
					entityHelper = AppLogic.CategoryStoreEntityHelper[0];
					break;
			}

		}

		private void ShowEmailFields()
		{
			lblEntityEmail.Visible = txtEmail.Visible = ltEmailAsterisk.Visible = rfvEmail.Visible = valRegExValEmail.Visible = true;
		}

		private void InitializeClientSideHelpers(string suffix)
		{
			ltPreEntity.Text = string.Format("{0} Quick Add", suffix);

			rfvName.ValidationGroup = string.Format("QuickAdd{0}", suffix);
			rfvEmail.ValidationGroup = string.Format("QuickAdd{0}", suffix);
			valRegExValEmail.ValidationGroup = string.Format("QuickAdd{0}", suffix);
			btnSubmit.ValidationGroup = string.Format("QuickAdd{0}", suffix);

			pnlQuickAddFields.Attributes.Add("class", string.Format("QuickAdd{0} panel panel-default quick-add-field-panel", suffix));
			linkQuickAdd.Attributes.Add("class", string.Format("LinkAdd{0}", suffix));
			linkQuickClose.Attributes.Add("class", string.Format("LinkClose{0} btn btn-sm btn-default", suffix));

			lblEntityName.Text = string.Format("{0} Name:", suffix);
			lblEntityEmail.Text = string.Format("{0} Email:", suffix);
			ltQuickAddScript.Text = string.Format(@"<script type='text/javascript'>
                                                    $('.LinkAdd{0}').click(function(event) {{
                                                        $('.QuickAdd{0}').show();
                                                    }});
                                                   $('.LinkClose{0}').click(function(event) {{
                                                        $('.QuickAdd{0}').hide();
                                                    }});
                                            </script>", suffix);
		}

		private void LoadParents()
		{
			if(Page.IsPostBack)
				return;

			pnlParent.Visible = true;
			ddParent.Items.Add(new ListItem("-- No --", "0"));

			ArrayList al = entityHelper.GetEntityArrayList(0, "", 0, ThisCustomer.LocaleSetting, false);
			for(int i = 0; i < al.Count; i++)
			{
				ListItemClass lic = (ListItemClass)al[i];
				string value = lic.Value.ToString();
				string name = HttpUtility.HtmlDecode(lic.Item);

				ddParent.Items.Add(new ListItem(name, value));
			}
		}

		private void CreateEntity()
		{
			try
			{
				bool isDistributor = entitySpecs.m_EntityName == "Distributor";

				int parentId = Localization.ParseNativeInt(ddParent.SelectedValue);
				Guid newGuid = Guid.NewGuid();
				string entityName = txtName.Text;
				string entityEmail = isDistributor && txtEmail != null
									? txtEmail.Text
									: String.Empty;

				bool isSectionOrCategory = (entitySpecs.m_EntityName == "Category" || entitySpecs.m_EntityName == "Section");

				string insertSQL = string.Format(@"insert into {0}({0}GUID,Name,SEName,TemplateName,SkinID,ImageFilenameOverride,{1} Summary,Description,
                                        SEAltText,Published,{2} PageSize,ColWidth,XmlPackage,SortByLooks,QuantityDiscountID {5}) 
                                        values(@newGuid, @name, @seName, @templateName, @skinId, @imageFileNameOverride, {3} @summary, @description,
                                        @seAltText,@published,{4} @pageSize,@colWidth,@xmlPackage,@sortByLooks,@quantityDiscountId {6})",
									entitySpecs.m_EntityName,
									(entitySpecs.m_HasParentChildRelationship ? string.Format("Parent{0}ID,", entitySpecs.m_EntityName) : string.Empty),
									(isSectionOrCategory ? string.Format("ShowIn{0}Browser,", entitySpecs.m_ObjectName) : string.Empty),
									(entitySpecs.m_HasParentChildRelationship ? "@parentEntityId," : string.Empty),
									(isSectionOrCategory ? "@showInEntityBrowser," : string.Empty),
									(isDistributor ? ", email" : string.Empty),
									(isDistributor ? ", @email" : string.Empty));

				using(var cn = new SqlConnection(DB.GetDBConn()))
				{
					cn.Open();
					using(var cmd = new SqlCommand(insertSQL, cn))
					{
						cmd.Parameters.Add(new SqlParameter("@newGuid", SqlDbType.UniqueIdentifier));
						cmd.Parameters["@newGuid"].Value = newGuid;

						if(isDistributor)
						{
							cmd.Parameters.Add(new SqlParameter("@email", SqlDbType.NVarChar, 100));
							cmd.Parameters["@email"].Value = entityEmail;
						}

						cmd.Parameters.Add(new SqlParameter("@name", SqlDbType.NVarChar, 400));
						cmd.Parameters["@name"].Value = entityName;

						cmd.Parameters.Add(new SqlParameter("@seName", SqlDbType.NVarChar, 100));
						cmd.Parameters["@sename"].Value = SearchEngineNameProvider.GenerateSeName(entityName);

						cmd.Parameters.Add(new SqlParameter("@templateName", string.Empty));

						cmd.Parameters.Add(new SqlParameter("@skinId", SqlDbType.Int));
						cmd.Parameters["@skinId"].Value = 0;

						cmd.Parameters.Add(new SqlParameter("@ImageFilenameOverride", string.Empty));

						if(entitySpecs.m_HasParentChildRelationship)
						{
							cmd.Parameters.Add(new SqlParameter("@parentEntityId", SqlDbType.Int));
							cmd.Parameters["@parentEntityId"].Value = parentId;
						}

						cmd.Parameters.Add(new SqlParameter("@summary", string.Empty));
						cmd.Parameters.Add(new SqlParameter("@description", string.Empty));

						cmd.Parameters.Add(new SqlParameter("@seAltText", SqlDbType.NText));
						cmd.Parameters["@seAltText"].Value = entityName;

						cmd.Parameters.Add(new SqlParameter("@published", SqlDbType.Int));
						cmd.Parameters["@published"].Value = 1;

						if(isSectionOrCategory)
						{
							cmd.Parameters.Add(new SqlParameter("@showInEntityBrowser", SqlDbType.Int));
							cmd.Parameters["@showInEntityBrowser"].Value = 1;
						}

						cmd.Parameters.Add(new SqlParameter("@pageSize", SqlDbType.Int));
						cmd.Parameters["@pageSize"].Value = 20;

						cmd.Parameters.Add(new SqlParameter("@colWidth", SqlDbType.Int));
						cmd.Parameters["@colWidth"].Value = 4;

						cmd.Parameters.Add(new SqlParameter("@xmlPackage", "entity.gridwithprices.xml.config"));

						cmd.Parameters.Add(new SqlParameter("@sortByLooks", SqlDbType.Int));
						cmd.Parameters["@sortByLooks"].Value = 0;

						cmd.Parameters.Add(new SqlParameter("@quantityDiscountId", SqlDbType.Int));
						cmd.Parameters["@quantityDiscountId"].Value = 0;

						cmd.ExecuteNonQuery();
					}
				}

				using(var dbconn = new SqlConnection(DB.GetDBConn()))
				{
					dbconn.Open();
					using(var rs =
						DB.GetRS("select " + entitySpecs.m_EntityName + "ID, Name from " + entitySpecs.m_EntityName + " with (NOLOCK) where Deleted=0 and " + entitySpecs.m_EntityName + "GUID=" +
								  DB.SQuote(newGuid.ToString()), dbconn))
					{
						rs.Read();

						EntityId = DB.RSFieldInt(rs, entitySpecs.m_EntityName + "ID");
						EntityName = DB.RSField(rs, "Name");
					}
				}
				//reset cache
				AppConfigManager.SetAppConfigValue("CacheMenus", "False");
				AppLogic.m_RestartApp();
				AppConfigManager.SetAppConfigValue("CacheMenus", "True");
				//reset cache
				txtName.Text = string.Empty;
				txtEmail.Text = string.Empty;
			}
			catch(Exception ex)
			{
				SysLog.LogException(ex, MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
			}
		}

		#endregion

	}
}
