// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class Variants : AspDotNetStorefront.Admin.AdminPageBase
	{
		public const string DeleteVariantCommand = "variant:delete";
		public const string UndeleteVariantCommand = "variant:undelete";
		public const string SetDefaultVariantCommand = "variant:set-default";
		public const string CloneVariantCommand = "variant:clone";

		protected int ProductId;

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			ProductId = CommonLogic.QueryStringNativeInt("productid");

			if(!IsPostBack)
			{
				AppLogic.EnsureProductHasADefaultVariantSet(ProductId);
			}			

			if(ProductId > 0)
			{
				lnkProduct.NavigateUrl = String.Format("product.aspx?productid={0}", ProductId);
				lnkProduct.Text = AppLogic.GetProductName(ProductId, Localization.GetDefaultLocale()) + " > ";
			}
		}

		protected void DispatchGridCommand(object sender, GridViewCommandEventArgs e)
		{
			if(e.CommandName == DeleteVariantCommand)
				DeleteVariantCommandHandler(sender, e, deleted: true, successStringResource: "admin.common.HasBeenDeleted");
			else if(e.CommandName == UndeleteVariantCommand)
				DeleteVariantCommandHandler(sender, e, deleted: false, successStringResource: "admin.common.HasBeenRestored");
			else if(e.CommandName == SetDefaultVariantCommand)
				SetDefaultVariantCommandHandler(sender, e);
			else if(e.CommandName == CloneVariantCommand)
				CloneVariantCommandHandler(sender, e);
			AppLogic.EnsureProductHasADefaultVariantSet(ProductId);
		}

		protected void btnAdd_Click(object sender, EventArgs e)
		{
			Response.Redirect(String.Format("{0}?ProductID={1}", AppLogic.AdminLinkUrl("variant.aspx"), ProductId));
		}

		protected void btnUpdate_Click(object sender, EventArgs e)
		{
			if(UpdateDisplayOrder())
				AlertMessageDisplay.PushAlertMessage(AppLogic.GetString("admin.product.variantgrid.UpdatedOrderAndDefault", ThisCustomer.LocaleSetting), AlertMessage.AlertType.Success);
			else
				AlertMessageDisplay.PushAlertMessage(AppLogic.GetString("admin.variant.VariantNotUpdated", ThisCustomer.LocaleSetting), AlertMessage.AlertType.Error);
		}

		void DeleteVariantCommandHandler(object sender, CommandEventArgs e, bool deleted, string successStringResource)
		{
			int variantId;
			if(!Int32.TryParse((string)e.CommandArgument, out variantId))
				return;

			if(SetDeletedFlag(variantId, deleted))
			{
				AlertMessageDisplay.PushAlertMessage(
					BuildAlertMessage(variantId, successStringResource),
					AlertMessage.AlertType.Success);

				FilteredListing.Rebind();
			}
		}

		void SetDefaultVariantCommandHandler(object sender, CommandEventArgs e)
		{
			int variantId;
			if(!Int32.TryParse((string)e.CommandArgument, out variantId))
				return;

			if(SetDefaultVariant(variantId))
			{
				AlertMessageDisplay.PushAlertMessage(
					String.Format("Variant {0} set as default variant", variantId),
					AlertMessage.AlertType.Success);

				FilteredListing.Rebind();
			}
		}

		void CloneVariantCommandHandler(object sender, CommandEventArgs e)
		{
			int variantId;
			if(!Int32.TryParse((string)e.CommandArgument, out variantId))
				return;

			if(CloneVariant(variantId))
			{
				AlertMessageDisplay.PushAlertMessage(
					AppLogic.GetString("admin.entityProductVariantsOverview.VariantCloned", ThisCustomer.SkinID, ThisCustomer.LocaleSetting),
					AlertMessage.AlertType.Success);

				FilteredListing.Rebind();
			}
		}

		bool UpdateDisplayOrder()
		{
			if(Grid.Rows.Count == 0)
				return false;

			foreach(GridViewRow row in Grid.Rows)
			{
				var variantIdField = row.FindControl<HiddenField>("VariantID");
				if(variantIdField == null)
					return false;

				int variantId;
				if(!Int32.TryParse(variantIdField.Value, out variantId))
					return false;

				var txtDisplayOrder = row.FindControl<TextBox>("txtDisplayOrder");
				if(txtDisplayOrder == null)
					return false;

				int displayOrder;
				if(!Int32.TryParse(txtDisplayOrder.Text, out displayOrder))
					return false;

				SetDisplayOrder(variantId, displayOrder);
			}

			return true;
		}

		string BuildAlertMessage(int variantId, string stringResourceName)
		{
			var locale = AppLogic.GetCurrentCustomer().LocaleSetting;
			return String.Format(
				"{0} {1} {2}",
				AppLogic.GetString("admin.common.Variant", locale),
				variantId,
				AppLogic.GetString(stringResourceName, locale));
		}

		#region Variant Data Access Methods

		bool SetDeletedFlag(int variantId, bool deleted)
		{
			try
			{
				DB.ExecuteSQL(
					"update dbo.ProductVariant set Deleted = @deleted where VariantID = @variantId",
					new[]
					{
						new SqlParameter("deleted", deleted),
						new SqlParameter("variantId", variantId),
					});

				return true;
			}
			catch(Exception ex)
			{
				SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Error);
				return false;
			}
		}

		void SetDisplayOrder(int variantId, int displayOrder)
		{
			DB.ExecuteSQL(
				"update ProductVariant set DisplayOrder = @displayOrder where VariantID = @variantId",
				new[]
					{
						new SqlParameter("displayOrder", displayOrder),
						new SqlParameter("variantId", variantId),
					});
		}

		bool SetDefaultVariant(int variantId)
		{
			try
			{
				//reset
				DB.ExecuteSQL(
					"update ProductVariant set IsDefault = 0 where ProductID = @productId",
					new[]
						{
							new SqlParameter("productId", ProductId),
						});

				//update
				DB.ExecuteSQL(
					"update ProductVariant set IsDefault = 1 where VariantID = @variantId",
					new[]
						{
							new SqlParameter("variantId", variantId),
						});

				return true;
			}
			catch(Exception ex)
			{
				SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Error);
				return false;
			}
		}

		bool CloneVariant(int variantId)
		{
			try
			{
				DB.ExecuteSQL(
					"aspdnsf_CloneVariant @variantId",
					new SqlParameter("@variantId", variantId));

				return true;
			}
			catch(Exception ex)
			{
				SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Error);
				return false;
			}
		}

		#endregion

	}
}
