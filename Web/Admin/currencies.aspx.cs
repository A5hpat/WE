// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class currencies : AspDotNetStorefront.Admin.AdminPageBase
	{
		#region Handlers
		protected void Page_Load(object sender, System.EventArgs e)
		{
			if (!Page.IsPostBack)
			{
				SetupLiveRatesDisplay();
				BindGrid();
				ToggleMultiCurrencyDisplay();
			}
		}

		protected void gMain_RowCommand(object sender, GridViewCommandEventArgs e)
		{
			if(e.CommandName == "DeleteItem")
			{
				try
				{
					DeleteCurrency(Localization.ParseNativeInt(e.CommandArgument.ToString()));
					ctlAlertMessage.PushAlertMessage("admin.currencies.deleted".StringResource(), AspDotNetStorefrontControls.AlertMessage.AlertType.Success);
					BindGrid();
				}
				catch(Exception ex)
				{
					ctlAlertMessage.PushAlertMessage(ex.Message, AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
				}
			}
			BindGrid();
			ToggleMultiCurrencyDisplay();	//The last row may have been deleted, in which case we need to show the header again
		}

		protected void btnConversionTest_Click(Object sender, EventArgs e)
		{
			decimal testAmount;
			if (Decimal.TryParse(txtTestAmount.Text.Trim(), out testAmount))
			{
				var result = Currency.Convert(testAmount, ddlSource.SelectedValue, ddlTarget.SelectedValue);
				txtTestResult.Text = result.ToString();
				txtTestResult.Visible = true;
			}
		}

		protected void btnGetLiveRates_Click(Object sender, EventArgs e)
		{
			AppConfigManager.SetAppConfigValue("Localization.CurrencyFeedUrl", txtLiveURL.Text.Trim());
			AppConfigManager.SetAppConfigValue("Localization.CurrencyFeedBaseRateCurrencyCode", txtBaseCurrencyCode.Text.Trim());
			AppConfigManager.SetAppConfigValue("Localization.CurrencyFeedXmlPackage", txtXmlPackage.Text.Trim());

			Currency.GetLiveRates();

			txtXmlDoc.Text = XmlCommon.PrettyPrintXml(Currency.m_LastRatesResponseXml);
			txtXmlTransform.Text = XmlCommon.PrettyPrintXml(Currency.m_LastRatesTransformedXml);
			pnlLiveRatesDebug.Visible = true;

			BindGrid();
		}

		protected void btnSaveCurrencies_Click(Object sender, EventArgs e)
		{
			var currencyId = String.Empty;
			var name = String.Empty;
			var code = String.Empty;
			var displayLocale = String.Empty;
			var displaySpec = String.Empty;
			var displayOrder = "1";
			var published = true;
			decimal exchangeRate = 1;
			var currencyUpdated = true;

			try
			{
				foreach(GridViewRow row in grdCurrencies.Rows)
				{
					if(row.RowType == DataControlRowType.DataRow)
					{
						Literal litCurrencyID = row.FindControl("litCurrencyID") as Literal;
						TextBox txtName = row.FindControl("txtName") as TextBox;
						TextBox txtCode = row.FindControl("txtCode") as TextBox;
						TextBox txtRate = row.FindControl("txtRate") as TextBox;
						TextBox txtDisplayFormat = row.FindControl("txtDisplayFormat") as TextBox;
						TextBox txtDisplaySpec = row.FindControl("txtDisplaySpec") as TextBox;
						TextBox txtDisplayOrder = row.FindControl("txtDisplayOrder") as TextBox;
						CheckBox cbxPublished = row.FindControl("cbxPublished") as CheckBox;

						if(litCurrencyID != null)
							currencyId = litCurrencyID.Text;

						if(txtName != null)
							name = txtName.Text.Trim();

						if(txtCode != null)
							code = txtCode.Text.Trim();

						if(txtRate != null)
							exchangeRate = Decimal.Parse(txtRate.Text.Trim());

						if(txtDisplayFormat != null)
							displayLocale = txtDisplayFormat.Text.Trim();

						if(txtDisplaySpec != null)
							displaySpec = txtDisplaySpec.Text.Trim();

						if(txtDisplayOrder != null)
							displayOrder = txtDisplayOrder.Text.Trim();

						if(cbxPublished != null)
							published = cbxPublished.Checked;

						//Locale reality check
						if(!ValidateDisplayLocale(displayLocale))
							return;

						if(currencyId.Length != 0)
						{
							DB.ExecuteSQL(String.Format("UPDATE Currency SET Name={0}, WasLiveRate=0, CurrencyCode={1}, ExchangeRate={2}, DisplayLocaleFormat={3}, DisplaySpec={4}, Published={5}, DisplayOrder={6}, LastUpdated=getdate() where CurrencyID={7}",
										DB.SQuote(name),
										DB.SQuote(code),
										Localization.DecimalStringForDB(exchangeRate),
										DB.SQuote(displayLocale),
										DB.SQuote(displaySpec),
										published ? "1" : "0",
										displayOrder,
										currencyId));
						}
					}
				}
				
				BindGrid();
				ToggleMultiCurrencyDisplay();	//May have added the first row, in which case hide the new table header
			}
			catch (Exception ex)
			{
				currencyUpdated = false;
				ctlAlertMessage.PushAlertMessage(ex.Message, AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
			}

			if(currencyUpdated)
			{
				Currency.FlushCache();
				ctlAlertMessage.PushAlertMessage("admin.currencies.updated".StringResource(), AspDotNetStorefrontControls.AlertMessage.AlertType.Success);
			}
		}

		protected void btnAddCurrency_Click(Object sender, EventArgs e)
		{
			Page.Validate("gAdd");
			if(Page.IsValid)
			{
				var name = txtNewName.Text.Trim();
				var code = txtNewCode.Text.Trim();
				var displayLocale = txtNewDisplayLocale.Text.Trim();
				var displaySpec = txtNewDisplaySpec.Text.Trim();
				var displayOrder = txtNewDisplayOrder.Text.Trim();
				var published = cbxNewPublished.Checked;
				decimal parsedRate;
				decimal exchangeRate = 0;

				if(decimal.TryParse(txtNewExchangeRate.Text.Trim(), out parsedRate))
					exchangeRate = parsedRate;

				//Locale reality check
				if(!ValidateDisplayLocale(displayLocale))
				{
					ctlAlertMessage.PushAlertMessage("admin.currencies.invalidLocale".StringResource(), AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
					return;
				}

				try
				{
					DB.ExecuteSQL(
						@"INSERT INTO Currency(
							Name,
							WasLiveRate,
							CurrencyCode,
							ExchangeRate,
							DisplayLocaleFormat,
							DisplaySpec,
							Published,	
							DisplayOrder,
							LastUpdated)
						VALUES(
							@name,
							0,
							@currencyCode,
							@exchangeRate,
							@displayLocaleFormat,
							@displaySpec,
							@published,
							@displayOrder,
							getDate())",
						new SqlParameter("@name", name),
						new SqlParameter("@currencyCode", code),
						new SqlParameter("@exchangeRate", Localization.DecimalStringForDB(exchangeRate)),
						new SqlParameter("@displayLocaleFormat", displayLocale),
						new SqlParameter("@displaySpec", displaySpec),
						new SqlParameter("@published", published ? "1" : "0"),
						new SqlParameter("@displayOrder", displayOrder)
					);		

					BindGrid();
					ToggleMultiCurrencyDisplay();   //May have added the first row, in which case hide the new table header
					ClearNewFields();
					ctlAlertMessage.PushAlertMessage("admin.currencies.created".StringResource(), AspDotNetStorefrontControls.AlertMessage.AlertType.Success);
				}
				catch(Exception ex)
				{
					ctlAlertMessage.PushAlertMessage(ex.Message, AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
				}				
			}
		}
		#endregion

		#region Display methods
		void ToggleMultiCurrencyDisplay()
		{
			int numCurrencies = DB.GetSqlN("SELECT COUNT(*) N FROM Currency");

			tblNewRowHeader.Visible = numCurrencies == 0;
			tblSaveButton.Visible = numCurrencies > 0;
			divInstalledCurrenciesHeader.Visible = numCurrencies > 0;
			pnlMultipleCurrencies.Visible = numCurrencies > 1;

			if(numCurrencies > 0)
				PopulateTestDropdowns();
		}

		void ClearNewFields()
		{
			txtNewName.Text = txtNewCode.Text = txtNewExchangeRate.Text = txtNewDisplayLocale.Text = txtNewDisplaySpec.Text = txtNewDisplayOrder.Text = String.Empty;
			cbxNewPublished.Checked = true;
		}

		void BindGrid()
		{
			using(var dbconn = new SqlConnection(DB.GetDBConn()))
			{
				dbconn.Open();
				using(var rs = DB.GetRS("SELECT CurrencyID, Name, CurrencyCode, ExchangeRate, DisplayLocaleFormat, Published, DisplayOrder, DisplaySpec, LastUpdated FROM Currency with (NOLOCK)", dbconn))
				{
					using(var dt = new DataTable())
					{
						dt.Load(rs);
						grdCurrencies.DataSource = dt;
						grdCurrencies.DataBind();
					}
				}
			}
		}

		void SetupLiveRatesDisplay()
		{
			var feedURL = AppLogic.AppConfig("Localization.CurrencyFeedUrl");

			txtLiveURL.Text = feedURL;
			litLiveURL.Text = String.Format(AppLogic.GetString("admin.currencies.CurrencyFeedUrl", ThisCustomer.LocaleSetting),
				(feedURL.Length != 0)
					? " (<a href=\"" + AppLogic.AppConfig("Localization.CurrencyFeedUrl") + "\" target=\"_blank\">test</a>)"
					: String.Empty);

			txtBaseCurrencyCode.Text = AppLogic.AppConfig("Localization.CurrencyFeedBaseRateCurrencyCode");
			txtXmlPackage.Text = AppLogic.AppConfig("Localization.CurrencyFeedXmlPackage");
		}

		void PopulateTestDropdowns()
		{
			ddlSource.Items.Clear();
			ddlTarget.Items.Clear();

			using(var dbconn = new SqlConnection(DB.GetDBConn()))
			{
				dbconn.Open();
				using(var rs = DB.GetRS("SELECT Name, CurrencyCode FROM Currency with (NOLOCK) WHERE Published = 1 ORDER BY DisplayOrder", dbconn))
				{
					while(rs.Read())
					{
						var currencyItem = new ListItem()
						{
							Text = DB.RSField(rs, "Name"),
							Value = DB.RSField(rs, "CurrencyCode")
						};

						ddlSource.Items.Add(currencyItem);
						ddlTarget.Items.Add(currencyItem);
					}
				}
			}
		}
		#endregion

		#region Helper methods
		bool ValidateDisplayLocale(string displayLocale)
		{
			try
			{
				CultureInfo validCulture = new CultureInfo(displayLocale);
				return true;
			}
			catch
			{
				ctlAlertMessage.PushAlertMessage(String.Format("Invalid Display Locale: {0}", displayLocale), AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
				return false;
			}
		}

		void DeleteCurrency(int currencyId)
		{
			DB.ExecuteSQL("DELETE FROM Currency WHERE CurrencyID=" + currencyId.ToString());
		}		
		#endregion
	}
}
