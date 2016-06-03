// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class StoreMappings : AspDotNetStorefront.Admin.AdminPageBase
	{
		// This query is templated to allow a dynamic number of store columns.
		const string SqlTemplate =
			@"select {{0}}
				Entities.EntityID [EntityId],
				dbo.GetMlValue(Entities.EntityName, @_locale) [EntityName],
				@entityType as [EntityType],
				-- Generated mapping bit columns
				{0}
			from 
				(
								select ProductID [EntityID], Name [EntityName], 'Product' [EntityType] from Product where @entityType = 'Product' and Deleted = 0
					union all	select NewsID [EntityID], News.Headline [EntityName], 'News' [EntityType] from News where @entityType = 'News' and Deleted = 0
					union all	select CouponID [EntityID], CouponCode [EntityName], 'Coupon' [EntityType] from Coupon where @entityType = 'Coupon' and Deleted = 0
					union all	select Id [EntityID], Name [EntityName], 'Promotion' [EntityType] from Promotions where @entityType = 'Promotion'
					union all	select OrderOptionID [EntityID], Name [EntityName], 'OrderOption' [EntityType] from OrderOption where @entityType = 'OrderOption'
					union all	select GiftCardID [EntityID], SerialNumber [EntityName], 'GiftCard' [EntityType] from GiftCard where @entityType = 'GiftCard'
					union all	select ShippingMethodID [EntityID], Name [EntityName], 'ShippingMethod' [EntityType] from ShippingMethod where @entityType = 'ShippingMethod'
					union all	select EntityID, Name [EntityName], EntityType from Entities where EntityType = @entityType
				) Entities
				left join (
					select 
						Pivoted.*
					from (
						select
							EntityType,
							EntityId,
							StoreId
						from
							dbo.StoreMappingView
						where
							EntityType = @entityType
						) as Mapping
						pivot (
							count(StoreId)
							for StoreId in (
								-- Generated store indexed columns
								{1}
							)
						) Pivoted
				) as Mappings on 
					Mappings.EntityID = Entities.EntityID
					and Mappings.EntityType = Entities.EntityType
			where 
				case
					when @storeId is null then 1
					{2}
					else 0
				end = 1
				and {{1}}";

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			var stores = Store.GetStores(false);
			var maxStoreId = stores.Max(store => store.StoreID);

			// A pivot requires a full numeric range to work correctly. We generate an index column for each possible ID between 1 and the max ID, then we only generate the mapping
			// columns for those ID's that actually exist.
			var storeValues = Enumerable
				.Range(1, maxStoreId)		// Full numeric range
				.Select(index => new
					{
						index,
						store = stores.FirstOrDefault(store => store.StoreID == index)						// Will be null if the store with the ID doesn't exist
					})
				.Select(o => new
					{
						mappingColumn = o.store == null
							? null
							: String.Format("isnull(cast(Mappings.[{0}] as bit), 0) [{0}]", o.index),		// Generate a column indicating if a mapping exists for only those ID's that are published, non-deleted stores
						indexColumn = String.Format("[{0}]", o.index),										// Generate a column for the full range of indicies
						filterColumn = String.Format("when @storeId = {0} then Mappings.[{0}]", o.index),	// Generate a column to filter on Store ID
						columnTemplate = o.store == null													// Generate a GridView column with the same criteria as a mapping column
							? null
							: new TemplateField
								{
									HeaderText = o.store.Name,
									ItemTemplate = new StoreMappingTemplate(o.index, o.store.StoreID)
								}
					})
				.ToArray();

			// Populate the query template with stores
			FilteredListing.SqlQuery = String.Format(
				SqlTemplate,
				String.Join(",\r\n", storeValues.Select(o => o.mappingColumn).Where(s => s != null)),
				String.Join(",", storeValues.Select(o => o.indexColumn).Where(s => s != null)),
				String.Join(Environment.NewLine, storeValues.Select(o => o.filterColumn).Where(s => s != null)));

			// Create columns in the grid for each store
			FilteredListing.ListingTemplateInstantiated += (sender, eventArgs) =>
				{
					foreach(var column in storeValues.Select(o => o.columnTemplate).Where(template => template != null))
						ListingGrid.Columns.Add(column);
				};
		}

		// This template cotnains all of the logic for displaying and updating a store entity mapping
		class StoreMappingTemplate : ITemplate
		{
			readonly int StoreIndex;
			readonly int StoreId;

			public StoreMappingTemplate(int storeIndex, int storeId)
			{
				StoreIndex = storeIndex;
				StoreId = storeId;
			}

			public void InstantiateIn(Control container)
			{
				var checkBox = new CommandCheckBox
				{
					CssClass = "js-selectable",
					CheckedCommandName = "storemapping:map",
					UncheckedCommandName = "storemapping:unmap",
				};
				checkBox.DataBinding += ControlDataBinding;
				checkBox.Command += ControlCommand;
				container.Controls.Add(checkBox);
			}

			void ControlDataBinding(object sender, EventArgs e)
			{
				var checkBox = (CommandCheckBox)sender;
				var container = (GridViewRow)checkBox.NamingContainer;
				checkBox.Checked = (bool)DataBinder.Eval(container.DataItem, StoreIndex.ToString());
				checkBox.CommandArgument = StoreId.ToString();
			}

			void ControlCommand(object sender, CommandEventArgs e)
			{
				var checkBox = (CommandCheckBox)sender;
				var gridViewRow = (GridViewRow)checkBox.NamingContainer;
				var gridView = (GridView)gridViewRow.NamingContainer;

				var dataKey = gridView.DataKeys[gridViewRow.RowIndex];
				var entityId = (int)dataKey.Values["EntityID"];
				var entityType = (string)dataKey.Values["EntityType"];

				var storeId = Int32.Parse(checkBox.CommandArgument);
				var mapped = checkBox.Checked;

				using(var connection = new SqlConnection(DB.GetDBConn()))
				{
					connection.Open();
					SetMap(connection, entityType, entityId, storeId, mapped);
				}
			}

			void SetMap(SqlConnection connection, string entityType, int entityId, int storeId, bool mapped)
			{
				using(var command = new SqlCommand("dbo.aspdnsf_Map", connection))
				{
					command.CommandType = CommandType.StoredProcedure;
					command.Parameters.AddRange(new[]
					{
						new SqlParameter("@entityType", entityType),
						new SqlParameter("@entityId", entityId),
						new SqlParameter("@storeId", storeId),
						new SqlParameter("@mapped", mapped),
					});

					command.ExecuteNonQuery();
				}
			}
		}

		protected void btnSave_Click(Object sender, EventArgs e)
		{
			AlertMessage.PushAlertMessage("Mappings updated", AspDotNetStorefrontControls.AlertMessage.AlertType.Success);
		}
	}
}
