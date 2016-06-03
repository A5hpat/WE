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
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class EntityBulkDisplayOrder : AspDotNetStorefront.Admin.AdminPageBase
	{
		protected string EntityType;
		protected int EntityId;
		EntitySpecs EntitySpecs;
		EntityHelper EntityHelper;
		protected string TopLevelString;
		protected string CurrentEntityName;

		protected void Page_Load(object sender, EventArgs e)
		{
			EntityId = CommonLogic.QueryStringNativeInt("EntityID");
			EntityType = string.IsNullOrEmpty(CommonLogic.QueryStringCanBeDangerousContent("EntityType"))
				? EntityTypes.Category.ToString()
				: CommonLogic.QueryStringCanBeDangerousContent("EntityType");
			EntitySpecs = EntityDefinitions.LookupSpecs(EntityType);
			EntityHelper = new EntityHelper(0, EntitySpecs, 0);

			if(IsPostBack)
				return;

			ddEntityType.SelectedValue = EntityType.ToLower();
			ddEntity.SelectedValue = EntityId.ToString();

			var currentEntity = new Entity(EntityId, EntityType);

			TopLevelString = BuildTopLevelString(EntityType);
			CurrentEntityName = GetCurrentEntityName(currentEntity);
			PopulateEntityDropDown(EntityType);
			SetupBreadcrumbData(currentEntity);

			DataBind();
		}

		void SetupBreadcrumbData(Entity entity)
		{
			var safetyCounter = 0;
			var currentEntity = entity;
			var breadcrumbEntities = new List<Entity>();
			while(safetyCounter < 10 && currentEntity.ParentID != 0)
			{
				// Move to the parent
				currentEntity = new Entity(currentEntity.ParentID, entity.EntityType);
				// Add the parent to the breadcrumb list
				breadcrumbEntities.Add(currentEntity);
				// Increment our safety counter
				safetyCounter++;
			}
			if(breadcrumbEntities.Any())
				BreadCrumb.DataSource = breadcrumbEntities;
		}

		string GetCurrentEntityName(Entity entity)
		{
			return entity != null && entity.Name != null
				? entity.LocaleName
				: string.Empty;
		}

		string BuildTopLevelString(string entityType)
		{
			return AppLogic.GetString(string.Format("admin.TopLevel{0}", entityType));
		}

		protected void PopulateEntityDropDown(string entityType)
		{
			ddEntity.Items.Clear();

			ddEntity.Items.Add(new ListItem(TopLevelString, "0"));
			foreach(ListItemClass listItem in EntityHelper.GetEntityArrayList(0, "", 0, ThisCustomer.LocaleSetting, false))
			{
				var entity = new Entity(listItem.Value, entityType);

				if(entity.NumChildren > 0)
					ddEntity.Items.Add(new ListItem(String.Format("{0} ({1})", HttpUtility.HtmlDecode(listItem.Item), entity.NumChildren), listItem.Value.ToString()));
			}
		}

		protected void grdDisplayOrder_DataBinding(object sender, EventArgs e)
		{
			using(var dbconn = new SqlConnection(DB.GetDBConn()))
			{
				var sql = string.Format(@"SELECT '{0}' AS EntityType, 
						e.{0}ID AS EntityId, 
						dbo.GetMlValue(Name, @localeSetting) AS Name, 
						e.DisplayOrder,
						(select COUNT(*) from {0} c where c.Parent{0}ID = e.{0}ID) as ChildCount
					FROM {0} e
					WHERE Parent{0}ID = @entityId 
					ORDER BY DisplayOrder, Name",
					EntityType);

				dbconn.Open();
				var parameters = new[]
				{
					new SqlParameter("entityId", EntityId),
					new SqlParameter("localeSetting", LocaleSetting)
				};
				using(var rs = DB.GetRS(sql, dbconn, parameters))
				using(var dt = new DataTable())
				{
					dt.Load(rs);
					grdDisplayOrder.DataSource = dt;
				}
			}
		}

		protected void ddEntityType_SelectedIndexChanged(object sender, EventArgs e)
		{
			// Redirect to this page with the new querystring paramters set
			Response.Redirect(string.Format(
				"entitybulkdisplayorder.aspx?entitytype={0}&entityid=0",
				ddEntityType.SelectedValue));
		}

		protected void UpdateDisplayOrder(object sender, EventArgs e)
		{
			try
			{
				foreach(GridViewRow row in grdDisplayOrder.Rows)
				{
					var entityId = grdDisplayOrder.DataKeys[row.DataItemIndex].Value;

					var txtDisplayOrder = (TextBox)row.FindControl("txtDisplayOrder");
					var litEntityId = (Literal)row.FindControl("litEntityId");

					int displayOrderVal;

					if(int.TryParse(txtDisplayOrder.Text, out displayOrderVal))
					{
						var parameters = new[]
						{
							new SqlParameter("@displayOrder", displayOrderVal),
							new SqlParameter("@entityId", entityId)
						};

						DB.ExecuteSQL(String.Format("UPDATE {0} SET DisplayOrder = @displayOrder WHERE {0}ID = @entityId", EntityType), parameters);
					}
				}

				AlertMessageDisplay.PushAlertMessage("admin.orderdetails.UpdateSuccessful".StringResource(), AlertMessage.AlertType.Success);
			}
			catch(Exception exception)
			{
				AlertMessageDisplay.PushAlertMessage(exception.Message, AlertMessage.AlertType.Error);
			}

			// Redirect to this page with the new querystring paramters set
			Response.Redirect(string.Format(
				"entitybulkdisplayorder.aspx?entitytype={0}&entityid={1}",
				ddEntityType.SelectedValue,
				ddEntity.SelectedValue));
		}

		protected void ddEntity_SelectedIndexChanged(object sender, EventArgs e)
		{
			// Redirect to this page with the new querystring paramters set
			Response.Redirect(string.Format(
				"entitybulkdisplayorder.aspx?entitytype={0}&entityid={1}",
				ddEntityType.SelectedValue,
				ddEntity.SelectedValue));
		}
	}
}
