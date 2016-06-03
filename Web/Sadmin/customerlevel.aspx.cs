// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class editcustomerlevel : AspDotNetStorefront.Admin.AdminPageBase
	{
		int CustomerLevelID = 0;

		/// <summary>
		/// Page Load
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Page_Load(object sender, System.EventArgs e)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");

			if(CommonLogic.QueryStringCanBeDangerousContent("CustomerLevelID").Length != 0 && CommonLogic.QueryStringCanBeDangerousContent("CustomerLevelID") != "0")
			{
				Editing = true;
				CustomerLevelID = Localization.ParseUSInt(CommonLogic.QueryStringCanBeDangerousContent("CustomerLevelID"));
			}
			else if(ViewState["CustomerLevelID"] != null)
			{
				Editing = true;
				CustomerLevelID = (int)ViewState["CustomerLevelID"];
			}
			else
			{
				Editing = false;
			}

			if(!IsPostBack)
				SetPageValues();
		}

		protected override void OnPreRender(EventArgs e)
		{
			DataBind();

			base.OnPreRender(e);
		}

		/// <summary>
		/// Saves New Customer Level
		/// </summary>
		/// <returns></returns>
		private int SaveNewCustomerLevel()
		{
			int customerLevelID = 0;
			StringBuilder sql = new StringBuilder();

			try
			{
				String NewGUID = DB.GetNewGUID();

				sql.Append("INSERT INTO CUSTOMERLEVEL(");
				sql.Append("CustomerLevelGUID");
				sql.Append(",Name");
				sql.Append(",LevelDiscountPercent");
				sql.Append(",LevelDiscountAmount");
				sql.Append(",LevelHasFreeShipping");
				sql.Append(",LevelAllowsQuantityDiscounts");
				sql.Append(",LevelAllowsPO");
				sql.Append(",LevelHasNoTax");
				sql.Append(",LevelAllowsCoupons");
				sql.Append(",LevelDiscountsApplyToExtendedPrices) ");
				sql.Append("VALUES(");
				sql.AppendFormat("{0},", DB.SQuote(NewGUID));
				sql.AppendFormat("{0},", DB.SQuote(NameLocaleField.GetTextFromFields()));
				sql.AppendFormat("{0},", string.IsNullOrEmpty(txtLevelDiscountPercent.Text.Trim()) ? "0" : Localization.DecimalStringForDB(Localization.ParseUSDecimal(txtLevelDiscountPercent.Text.Trim())));
				sql.AppendFormat("{0},", string.IsNullOrEmpty(txtLevelDiscountAmount.Text.Trim()) ? "0" : Localization.CurrencyStringForDBWithoutExchangeRate(Localization.ParseUSDecimal(txtLevelDiscountAmount.Text.Trim())));
				sql.AppendFormat("{0},", LevelHasFreeShipping.SelectedValue);
				sql.AppendFormat("{0},", LevelAllowsQuantityDiscounts.SelectedValue);
				sql.AppendFormat("{0},", LevelAllowsPO.SelectedValue);
				sql.AppendFormat("{0},", LevelHasNoTax.SelectedValue);
				sql.AppendFormat("{0},", LevelAllowsCoupons.SelectedValue);
				sql.AppendFormat("{0})", LevelDiscountsApplyToExtendedPrices.SelectedValue);
				DB.ExecuteSQL(sql.ToString());

				using(var dbconn = DB.dbConn())
				{
					dbconn.Open();
					using(var rs = DB.GetRS("SELECT CustomerLevelID FROM CustomerLevel WITH (NOLOCK) WHERE deleted=0 AND CustomerLevelGUID=" + DB.SQuote(NewGUID), dbconn))
					{
						rs.Read();
						customerLevelID = DB.RSFieldInt(rs, "CustomerLevelID");
						if(customerLevelID > 0)
						{
							Editing = true;
						}
					}
				}
			}
			catch
			{
				customerLevelID = 0;
			}

			return customerLevelID;
		}

		/// <summary>
		/// Updates Existing Customer Level
		/// </summary>
		/// <returns></returns>
		private bool UpdateExistingCustomerLevel()
		{
			bool saved = false;

			try
			{
				StringBuilder sql = new StringBuilder();
				sql.Append("UPDATE CustomerLevel SET ");
				sql.AppendFormat("name={0},", DB.SQuote(NameLocaleField.GetTextFromFields()));
				sql.AppendFormat("LevelDiscountPercent={0},", string.IsNullOrEmpty(txtLevelDiscountPercent.Text.Trim()) ? "0" : Localization.DecimalStringForDB(Localization.ParseUSDecimal(txtLevelDiscountPercent.Text.Trim())));
				sql.AppendFormat("LevelDiscountAmount={0},", string.IsNullOrEmpty(txtLevelDiscountAmount.Text.Trim()) ? "0" : Localization.CurrencyStringForDBWithoutExchangeRate(Localization.ParseUSDecimal(txtLevelDiscountAmount.Text.Trim())));
				sql.AppendFormat("LevelHasFreeShipping={0},", LevelHasFreeShipping.SelectedValue);
				sql.AppendFormat("LevelAllowsQuantityDiscounts={0},", LevelAllowsQuantityDiscounts.SelectedValue);
				sql.AppendFormat("LevelAllowsPO={0},", LevelAllowsPO.SelectedValue);
				sql.AppendFormat("LevelHasNoTax={0},", LevelHasNoTax.SelectedValue);
				sql.AppendFormat("LevelAllowsCoupons={0},", LevelAllowsCoupons.SelectedValue);
				sql.AppendFormat("LevelDiscountsApplyToExtendedPrices={0} ", LevelDiscountsApplyToExtendedPrices.SelectedValue);
				sql.AppendFormat("WHERE CustomerLevelID={0}", CustomerLevelID.ToString());
				DB.ExecuteSQL(sql.ToString());
				Editing = true;
				saved = true;
			}
			catch
			{
				saved = false;
			}

			return saved;
		}

		/// <summary>
		/// Handles clicking of the save button
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void btnSave_Click(object sender, EventArgs e)
		{
			if(Editing)
			{
				//Update
				if(UpdateExistingCustomerLevel())
				{
					ctrlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.customer.level.UpdateSuccessful", SkinID, ThisCustomer.LocaleSetting), AlertMessage.AlertType.Success);
				}
				else
				{
					ctrlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.customer.level.UpdateFailed", SkinID, ThisCustomer.LocaleSetting), AlertMessage.AlertType.Error);
				}
			}
			else
			{
				var temporaryCustomerLevelID = SaveNewCustomerLevel();

				//Insert
				if(temporaryCustomerLevelID > 0)
				{
					CustomerLevelID = temporaryCustomerLevelID;
					ViewState["CustomerLevelID"] = CustomerLevelID;
					ctrlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.customer.level.SaveNewSuccessful", SkinID, ThisCustomer.LocaleSetting), AlertMessage.AlertType.Success);
				}
				else
				{
					ctrlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.customer.level.SaveNewFailed", SkinID, ThisCustomer.LocaleSetting), AlertMessage.AlertType.Error);
				}
			}

			SetPageValues();
			NameLocaleField.BindData();
		}

		/// <summary>
		/// Handles clicking of the save and close button
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void btnSaveAndClose_Click(object sender, EventArgs e)
		{
			if(Editing)
			{
				//Update
				if(UpdateExistingCustomerLevel())
				{
					ctrlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.customer.level.UpdateSuccessful", SkinID, ThisCustomer.LocaleSetting), AlertMessage.AlertType.Success);
					Response.Redirect(ReturnUrlTracker.GetReturnUrl());
				}
				else
				{
					ctrlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.customer.level.UpdateFailed", SkinID, ThisCustomer.LocaleSetting), AlertMessage.AlertType.Error);
				}
			}
			else
			{
				//Insert
				if(SaveNewCustomerLevel() != 0)
				{
					ctrlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.customer.level.SaveNewSuccessful", SkinID, ThisCustomer.LocaleSetting), AlertMessage.AlertType.Success);
					Response.Redirect(ReturnUrlTracker.GetReturnUrl());
				}
				else
				{
					ctrlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.customer.level.SaveNewFailed", SkinID, ThisCustomer.LocaleSetting), AlertMessage.AlertType.Error);
				}
			}
		}

		/// <summary>
		/// Handles clicking of the reset button
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void btnReset_Click(object sender, EventArgs e)
		{
			if(Editing)
			{
				//Set form back to the values for the current customer level id
				Response.Redirect(String.Format("customerlevel.aspx?CustomerLevelID={0}", CustomerLevelID));
			}
			else
			{
				//Set form back to defaults
				Response.Redirect("customerlevel.aspx");
			}
		}

		/// <summary>
		/// Sets the fields and radio buttons on the page
		/// </summary>
		private void SetPageValues()
		{
			using(var dbconn = DB.dbConn())
			{
				dbconn.Open();
				using(var rs = DB.GetRS("select * from CustomerLevel with (NOLOCK) where CustomerLevelID=" + CustomerLevelID.ToString(), dbconn))
				{
					if(rs.Read())
						Editing = true;

					ctrlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.editCustomerLevel.Warning", SkinID, ThisCustomer.LocaleSetting), AspDotNetStorefrontControls.AlertMessage.AlertType.Warning);
					if(Editing)
					{
						lblHeader.Text = String.Format(AppLogic.GetString("admin.editCustomerLevel.EditingCLevel", SkinID, ThisCustomer.LocaleSetting), DB.RSFieldByLocale(rs, "Name", ThisCustomer.LocaleSetting), DB.RSFieldInt(rs, "CustomerLevelID"));
						NameLocaleField.Text = DB.RSField(rs, "Name");
						txtLevelDiscountPercent.Text = Localization.CurrencyStringForDBWithoutExchangeRate(DB.RSFieldDecimal(rs, "LevelDiscountPercent"));
						txtLevelDiscountAmount.Text = Localization.CurrencyStringForDBWithoutExchangeRate(DB.RSFieldDecimal(rs, "LevelDiscountAmount"));
						LevelHasFreeShipping.SelectedIndex = (DB.RSFieldBool(rs, "LevelHasFreeShipping") ? 0 : 1);
						LevelAllowsQuantityDiscounts.SelectedIndex = (DB.RSFieldBool(rs, "LevelAllowsQuantityDiscounts") ? 0 : 1);
						LevelAllowsPO.SelectedIndex = (DB.RSFieldBool(rs, "LevelAllowsPO") ? 0 : 1);
						LevelHasNoTax.SelectedIndex = (DB.RSFieldBool(rs, "LevelHasNoTax") ? 0 : 1);
						LevelAllowsCoupons.SelectedIndex = (DB.RSFieldBool(rs, "LevelAllowsCoupons") ? 0 : 1);
						LevelDiscountsApplyToExtendedPrices.SelectedIndex = (DB.RSFieldBool(rs, "LevelDiscountsApplyToExtendedPrices") ? 0 : 1);
					}
					else
					{
						lblHeader.Text = AppLogic.GetString("admin.editCustomerLevel.AddingCLevel", SkinID, ThisCustomer.LocaleSetting);
						NameLocaleField.Text = string.Empty;
						txtLevelDiscountPercent.Text = string.Empty;
						txtLevelDiscountAmount.Text = string.Empty;
						LevelHasFreeShipping.SelectedIndex = 1;
						LevelAllowsQuantityDiscounts.SelectedIndex = 1;
						LevelAllowsPO.SelectedIndex = 1;
						LevelHasNoTax.SelectedIndex = 1;
						LevelAllowsCoupons.SelectedIndex = 1;
						LevelDiscountsApplyToExtendedPrices.SelectedIndex = 1;
					}
				}
			}
		}
	}
}
