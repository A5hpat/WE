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
using AspDotNetStorefrontAdmin;
using AspDotNetStorefrontCore;

public partial class BulkEditPrices : AspDotNetStorefront.Admin.AdminPageBase
{
	protected void Page_Load(object sender, System.EventArgs e)
	{
		Response.CacheControl = "private";
		Response.Expires = 0;
		Response.AddHeader("pragma", "no-cache");
	}

	protected void btnSave_Click(Object sender, EventArgs e)
	{
		Save();
		AlertMessage.PushAlertMessage(AppLogic.GetString("admin.bulkeditprices.pricesupdated", ThisCustomer.LocaleSetting), AspDotNetStorefrontControls.AlertMessage.AlertType.Success);
	}

	protected void btnBulkSaveSalesPrice_Click(Object sender, EventArgs e)
	{
		double discount;
		if(!double.TryParse(txtSalesDiscountPercentage.Text, out discount))
		{
			AlertMessage.PushAlertMessage(AppLogic.GetString("admin.bulkeditprices.salespricesnotupdated", ThisCustomer.LocaleSetting), AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
			return;
		}

		BulkSaveSalesPrice(discount);
		AlertMessage.PushAlertMessage(AppLogic.GetString("admin.bulkeditprices.salespricesupdated", ThisCustomer.LocaleSetting), AspDotNetStorefrontControls.AlertMessage.AlertType.Success);
	}

	protected void btnBulkClearSalesPrice_Click(Object sender, EventArgs e)
	{
		BulkClearSalesPrice();
		AlertMessage.PushAlertMessage(AppLogic.GetString("admin.bulkeditprices.salespricesupdated", ThisCustomer.LocaleSetting), AspDotNetStorefrontControls.AlertMessage.AlertType.Success);
	}

	void Save()
	{
		using(var connection = new SqlConnection(DB.GetDBConn()))
		{
			connection.Open();

			var items = repeatMap.Items.Cast<RepeaterItem>().Where(item => item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem);
			foreach(var item in items)
			{
				var variantId = int.Parse((item.FindControl("lblVariantId") as Label).Text);
				var variantName = (item.FindControl("txtVariantName") as TextBox).Text;
				var variantSKUSuffix = (item.FindControl("txtVariantSKUSuffix") as TextBox).Text;
				var price = decimal.Parse((item.FindControl("txtPrice") as TextBox).Text);
				var inventory = int.Parse((item.FindControl("txtInventory") as TextBox).Text);
				var published = (item.FindControl("chkPublished") as CheckBox).Checked;
				var where = FilteredListing.GetFilterWhereClause();

				decimal? salePrice = null;
				decimal result;
				if(decimal.TryParse((item.FindControl("txtSalePrice") as TextBox).Text, out result))
					salePrice = result;

				using(var command = new SqlCommand(@"
					update ProductVariant set
						Name = dbo.SetMlValue(Name, @Name, @_locale),
						SKUSuffix = @SKUSuffix,
						Price = @Price,
						SalePrice = @SalePrice,
						Inventory = @Inventory,
						Published = @published
					where VariantID = @VariantID", connection))
				{
					command.Parameters.Add(new SqlParameter("VariantID", variantId));
					command.Parameters.Add(new SqlParameter("Name", variantName));
					command.Parameters.Add(new SqlParameter("SKUSuffix", variantSKUSuffix));
					command.Parameters.Add(new SqlParameter("Price", price));
					command.Parameters.Add(new SqlParameter("SalePrice", (object)salePrice ?? DBNull.Value));
					command.Parameters.Add(new SqlParameter("Inventory", inventory));
					command.Parameters.Add(new SqlParameter("Published", published));
					command.Parameters.Add(new SqlParameter("_locale", where.Parameters.Where(p => p.ParameterName == "_locale").FirstOrDefault().Value));

					DB.ExecuteSQL(command);
				}
			}
		}
	}

	void BulkSaveSalesPrice(double discount)
	{
		using(var connection = new SqlConnection(DB.GetDBConn()))
		{
			connection.Open();

			var whereClause = FilteredListing.GetFilterWhereClause();
			var setClause = ("Price * @discount");
			var command = new SqlCommand(BuildBatchUpdateCommandString(setClause, whereClause.Sql), connection);
			command.Parameters.Add(new SqlParameter("discount", (100 - discount) / 100));
			command.Parameters.AddRange(whereClause.Parameters.ToArray());

			DB.ExecuteSQL(command);
		}
	}

	void BulkClearSalesPrice()
	{
		using(var connection = new SqlConnection(DB.GetDBConn()))
		{
			connection.Open();

			var whereClause = FilteredListing.GetFilterWhereClause();
			var setClause = "null";
			var command = new SqlCommand(BuildBatchUpdateCommandString(setClause, whereClause.Sql), connection);
			command.Parameters.AddRange(whereClause.Parameters.ToArray());

			DB.ExecuteSQL(command);
		}
	}

	string BuildBatchUpdateCommandString(string setValueClause, string whereClause)
	{
		return String.Format(@"
			declare @_localeId int,
			@_currentCustomerLocaleId int

			select @_localeId = LocaleSettingID 
			from LocaleSetting 
			where Name = @_locale

			select @_currentCustomerLocaleId = LocaleSettingID 
			from LocaleSetting 
			where Name = @_currentCustomerLocale

			update ProductVariant 
			set SalePrice = {0}
			where VariantId in (
				select v.VariantId 
				from ProductVariant v inner join Product p on p.ProductID = v.ProductID 
						left join (
							select ObjectId, LocalizedName
							from dbo.LocalizedObjectName
							where ObjectType = 'productvariant' and LocaleId = @_localeId
							) as SelectedVariantLocalization on v.VariantId = SelectedVariantLocalization.ObjectId
						left join (
							select ObjectId, LocalizedName
							from dbo.LocalizedObjectName
							where ObjectType = 'productvariant' and LocaleId = @_currentCustomerLocaleId
							) as DefaultVariantLocalization on v.VariantId = DefaultVariantLocalization.ObjectId
						left join (
							select ObjectId, LocalizedName
							from dbo.LocalizedObjectName
							where ObjectType = 'productvariant' and LocaleId is null
							) as UnspecifiedVariantLocalization on v.VariantId = UnspecifiedVariantLocalization.ObjectId 
						left join (
							select ObjectId, LocalizedName
							from dbo.LocalizedObjectName
							where ObjectType = 'product' and LocaleId = @_localeId
							) as SelectedProductLocalization on p.ProductId = SelectedProductLocalization.ObjectId
						left join (
							select ObjectId, LocalizedName
							from dbo.LocalizedObjectName
							where ObjectType = 'product' and LocaleId = @_currentCustomerLocaleId
							) as DefaultProductLocalization on p.ProductId = DefaultProductLocalization.ObjectId
						left join (
							select ObjectId, LocalizedName
							from dbo.LocalizedObjectName
							where ObjectType = 'product' and LocaleId is null
							) as UnspecifiedProductLocalization on p.ProductId = UnspecifiedProductLocalization.ObjectId 
				where {1})",
			setValueClause,
			whereClause);
	}
}
