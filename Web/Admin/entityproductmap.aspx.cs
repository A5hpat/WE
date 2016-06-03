// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI.WebControls;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class EntityProductMap : AspDotNetStorefront.Admin.AdminPageBase
	{
		string EntityType
		{ get { return Request.QueryString["entityType"] ?? String.Empty; } }

		int EntityId
		{
			get
			{
				int parsed;
				return Int32.TryParse(Request.QueryString["entityId"], out parsed)
					? parsed
					: 0;
			}
		}

		readonly string[] SupportedEntityTypes;
		EntitySpecs EntitySpecs;
		EntityHelper EntityHelper;

		public EntityProductMap()
		{
			SupportedEntityTypes = new[] 
			{ 
				"distributor",
				"section",
				"manufacturer",
				"category",
			};
		}

		protected override void OnInit(EventArgs e)
		{
			EntitySpecs = EntityDefinitions.LookupSpecs(EntityType);
			EntityHelper = new EntityHelper(0, EntitySpecs, 0);
			HeaderText.Text = String.Format("Products for {0}", EntityHelper.GetEntityBreadcrumb6(EntityId, ThisCustomer.LocaleSetting));

			base.OnInit(e);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
		}

		protected void Save_Click(object sender, EventArgs e)
		{
			Save();
		}

		protected void SaveAndClose_Click(object sender, EventArgs e)
		{
			if(Save())
				Response.Redirect(ReturnUrlTracker.GetReturnUrl());
		}

		bool Save()
		{
			var productMappingChanges = grdMap
				.Rows
				.Cast<GridViewRow>()
				.Select(row => new
				{
					productId = (int)grdMap.DataKeys[row.DataItemIndex].Values["ProductID"],
					currentMapping = (int)grdMap.DataKeys[row.DataItemIndex].Values["IsMapped"] == 1,
					desiredMapping = row.FindControl<CheckBox>("chkIsMapped").Checked
				})
				.Where(o => o.currentMapping != o.desiredMapping);

			using(var connection = new SqlConnection(DB.GetDBConn()))
			{
				connection.Open();

				foreach(var mapping in productMappingChanges)
					SetMap(connection, EntityType, EntityId, mapping.productId, mapping.desiredMapping);
			}

			AlertMessageControl.PushAlertMessage("Mappings successfully updated", AlertMessage.AlertType.Success);
			return true;
		}

		void SetMap(SqlConnection connection, string entityType, int entityId, int productId, bool setAsMapped)
		{
			var entityTableName = GetEntityTableName(entityType);

			if(entityType.ToLowerInvariant() == "manufacturer")
				DB.ExecuteSQL(String.Format("delete from {0} where ProductId = {1}", entityTableName, productId));

			var sql = setAsMapped
				? string.Format("insert into {0} (ProductID, {1}ID, DisplayOrder, CreatedOn, UpdatedOn) values(@productId, @entityId, 1, getdate(), getdate())", 
					entityTableName, 
					entityType)
				: string.Format("delete from {0} where {1}Id = @entityId and ProductId = @productId", 
					entityTableName, 
					entityType);

			using(var command = new SqlCommand(sql, connection))
			{
				command.Parameters.Add(new SqlParameter("EntityId", entityId));
				command.Parameters.Add(new SqlParameter("ProductId", productId));
				command.ExecuteNonQuery();
			}
		}

		string GetEntityTableName(string entityType)
		{
			if(!SupportedEntityTypes.Contains(entityType, StringComparer.OrdinalIgnoreCase))
				throw new ArgumentException("Invalid entity type specified.", "entityType");

			return String.Format("Product{0}", entityType.ToLower());
		}
	}
}
