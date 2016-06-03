// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;

using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontControls
{
	public partial class QuickAddCustomerLevel : BaseControl
	{
		#region Properties
		public int CustomerLevelId { get; private set; }
		public string CustomerLevelName { get; private set; }
		#endregion

		#region Event Handlers

		protected void Page_Load(Object sender, EventArgs e)
		{
			InitializeClientSideHelpers("CustomerLevel");
		}

		protected void btnSubmit_Click(Object sender, EventArgs e)
		{
			//only validates this button's validationgroup
			if(txtName.Text.Length > 0 && Page.IsValid)
				CreateCustomerLevel();
		}

		#endregion

		#region Protected Methods

		private void InitializeClientSideHelpers(string suffix)
		{
			rfvName.ValidationGroup = string.Format("QuickAdd{0}", suffix);
			rvDiscount.ValidationGroup = string.Format("QuickAdd{0}", suffix);
			btnSubmit.ValidationGroup = string.Format("QuickAdd{0}", suffix);
			pnlQuickAddFields.Attributes.Add("class", string.Format("QuickAdd{0} panel panel-default quick-add-field-panel", suffix));
			linkQuickAdd.Attributes.Add("class", string.Format("LinkAdd{0}", suffix));
			linkQuickClose.Attributes.Add("class", string.Format("LinkClose{0} btn btn-sm btn-default", suffix));
		}

		private void CreateCustomerLevel()
		{
			try
			{
				var newGuid = Guid.NewGuid();
				var discountAmount = decimal.Zero;
				decimal.TryParse(txtDiscountAmount.Text, out discountAmount);
				var discountType = ddlDiscountType.SelectedValue;

				using(var cn = new SqlConnection(DB.GetDBConn()))
				{
					cn.Open();
					using(var cmd = new SqlCommand(@"insert into CustomerLevel(CustomerLevelGUID,Name,LevelDiscountPercent,LevelDiscountAmount,LevelHasFreeShipping,
                                LevelAllowsQuantityDiscounts,LevelAllowsPO,LevelHasNoTax,LevelAllowsCoupons,LevelDiscountsApplyToExtendedPrices)
                                values(@newGUID,@name,@discountPercent,@discountAmount,@freeShipping,@allowQuantityDiscount,@allowPO,
                                @hasTax,@allowCoupons,@applyToExtendedPrices)", cn))
					{
						cmd.Parameters.Add(new SqlParameter("@newGuid", SqlDbType.UniqueIdentifier));
						cmd.Parameters.Add(new SqlParameter("@name", SqlDbType.NVarChar));
						cmd.Parameters.Add(new SqlParameter("@discountPercent", SqlDbType.Decimal));
						cmd.Parameters.Add(new SqlParameter("@discountAmount", SqlDbType.Decimal));
						cmd.Parameters.Add(new SqlParameter("@freeShipping", SqlDbType.TinyInt));
						cmd.Parameters.Add(new SqlParameter("@allowQuantityDiscount", SqlDbType.TinyInt));
						cmd.Parameters.Add(new SqlParameter("@allowPO", SqlDbType.TinyInt));
						cmd.Parameters.Add(new SqlParameter("@hasTax", SqlDbType.TinyInt));
						cmd.Parameters.Add(new SqlParameter("@allowCoupons", SqlDbType.TinyInt));
						cmd.Parameters.Add(new SqlParameter("@applyToExtendedPrices", SqlDbType.TinyInt));

						cmd.Parameters["@newGuid"].Value = newGuid;
						cmd.Parameters["@name"].Value = txtName.Text;
						cmd.Parameters["@discountPercent"].Value = (discountType.EqualsIgnoreCase("Fixed") ? decimal.Zero : discountAmount);
						cmd.Parameters["@discountAmount"].Value = (discountType.EqualsIgnoreCase("Fixed") ? discountAmount : decimal.Zero);
						cmd.Parameters["@freeShipping"].Value = 0;
						cmd.Parameters["@allowQuantityDiscount"].Value = 1;
						cmd.Parameters["@allowPO"].Value = 0;
						cmd.Parameters["@hasTax"].Value = 0;
						cmd.Parameters["@allowCoupons"].Value = 1;
						cmd.Parameters["@applyToExtendedPrices"].Value = 0;

						cmd.ExecuteNonQuery();
					}
				}

				using(var dbconn = new SqlConnection(DB.GetDBConn()))
				{
					dbconn.Open();
					using(var rs =
						DB.GetRS("select CustomerLevelId, Name from CustomerLevel with (NOLOCK) where Deleted=0 and CustomerLevelGUID=" +
								  DB.SQuote(newGuid.ToString()), dbconn))
					{
						rs.Read();

						CustomerLevelId = DB.RSFieldInt(rs, "CustomerLevelId");
						CustomerLevelName = DB.RSField(rs, "Name");
					}
				}
				txtName.Text = string.Empty;
				txtDiscountAmount.Text = string.Empty;
			}
			catch(Exception ex)
			{
				SysLog.LogException(ex, MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
			}
		}

		#endregion

	}
}
