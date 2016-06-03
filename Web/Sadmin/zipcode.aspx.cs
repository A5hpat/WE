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
using System.Text;
using System.Web.UI.WebControls;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	/// <summary>
	/// Summary description for taxzips.
	/// </summary>
	public partial class zipcode : AspDotNetStorefront.Admin.AdminPageBase
	{
		string ZipCode = String.Empty;
		
		protected void Page_Load(object sender, EventArgs e)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");

			ZipCode = CommonLogic.QueryStringCanBeDangerousContent("ZipCode");

			if(!String.IsNullOrEmpty(ZipCode))
			{
				Editing = true;
			}

			if(!Page.IsPostBack)
			{
				BuildCountryDropdown();
				PopulateForm(Editing);
				BindGrid();
			}
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			btnClose.DataBind();
			btnCloseTop.DataBind();
		}

		private void PopulateForm(bool editing)
		{
			if(!editing)
			{
				litHeader.Text = "admin.common.Add".StringResource() + " " + "admin.entityEdit.ZipCode".StringResource();
			}
			else
			{
				litHeader.Text = "admin.common.Edit".StringResource() + " " + "admin.entityEdit.ZipCode".StringResource();
				txtZipCode.Text = ZipCode;

				string sql = "select distinct z.CountryID from ZipTaxRate z, Country c with (NOLOCK) where z.CountryId = c.CountryId AND z.ZipCode=" + DB.SQuote(ZipCode);

				using(SqlConnection dbconn = DB.dbConn())
				{
					dbconn.Open();
					using(IDataReader rs = DB.GetRS(sql, dbconn))
					{
						if(rs.Read())
						{
							ddlCountry.SelectedValue = DB.RSFieldInt(rs, "CountryID").ToString();
						}
					}
				}
			}
			ddlCountry.Enabled = !Editing;
		}

		protected void BuildCountryDropdown()
		{
			string sSql = "select Name, CountryID, DisplayOrder from country  with (NOLOCK)  where Published = 1 and PostalCodeRequired=1 order by DisplayOrder,Name";
			string selectedValue = String.IsNullOrEmpty(ddlCountry.SelectedValue) ? String.Empty : ddlCountry.SelectedValue;
			using(DataTable dt = new DataTable())
			{
				using(SqlConnection conn = new SqlConnection(DB.GetDBConn()))
				{
					conn.Open();

					using(IDataReader rs = DB.GetRS(sSql, conn))
					{
						dt.Load(rs);
					}
				}
				ddlCountry.DataSource = dt;
				ddlCountry.DataTextField = "Name";
				ddlCountry.DataValueField = "CountryID";
				ddlCountry.DataBind();
				if(String.IsNullOrEmpty(selectedValue))
					ddlCountry.SelectedValue = selectedValue;
			}
		}

		private void BindGrid()
		{
			using(SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
			{
				StringBuilder sql = new StringBuilder(string.Format("SELECT TaxClassID, dbo.GetMlValue(Name, '{0}') as Name FROM TaxClass ORDER BY TaxClassID", Localization.GetDefaultLocale()));

				dbconn.Open();
				using(IDataReader rs = DB.GetRS(sql.ToString(), dbconn))
				{
					using(DataTable dt = new DataTable())
					{
						dt.Load(rs);
						grdTaxRates.DataSource = dt;
						grdTaxRates.DataBind();
					}
				}
			}
		}
		
		protected void grdTaxRates_OnRowDataBound(object sender, GridViewRowEventArgs e)
		{
			TextBox txtTaxRate = e.Row.FindControl("txtTaxRate") as TextBox;
			Literal litTaxClassId = e.Row.FindControl("litTaxClassId") as Literal;

			if(txtTaxRate != null && litTaxClassId != null && !String.IsNullOrEmpty(ZipCode))
			{
				int taxClassId = 0;

				if(int.TryParse(litTaxClassId.Text, out taxClassId))
				{
					decimal taxClassRate = DB.GetSqlNDecimal(String.Format("SELECT TaxRate N FROM ZipTaxRate WHERE ZipCode = {0} AND TaxClassID = {1}", DB.SQuote(ZipCode), taxClassId));
					txtTaxRate.Text = taxClassRate.ToString();
				}
			}
		}

		protected void grdTaxRates_OnPageIndexChanging(object sender, GridViewPageEventArgs e)
		{
			grdTaxRates.PageIndex = e.NewPageIndex;
			BindGrid();
		}

		protected void btnSave_Click(object sender, EventArgs e)
		{
			if(SaveZipTaxes())
			{
				ctlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.zipcode.Updated", ThisCustomer.LocaleSetting), AlertMessage.AlertType.Success);
				Response.Redirect(String.Format("zipcode.aspx?zipcode={0}", ZipCode));
			}				
		}

		protected void btnSaveAndClose_Click(object sender, EventArgs e)
		{
			if(SaveZipTaxes())
			{
				ctlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.zipcode.Updated", ThisCustomer.LocaleSetting), AlertMessage.AlertType.Success);
				Response.Redirect(ReturnUrlTracker.GetReturnUrl());
			}
		}

		private bool SaveZipTaxes()
		{
			var zipCode = txtZipCode.Text.Trim();
			
			int countryId;
			int.TryParse(ddlCountry.SelectedValue, out countryId);

			var validCountryAndZip = AppLogic.ValidatePostalCode(zipCode, countryId);
			if(!validCountryAndZip)
			{
				Customer cust = Context.GetCustomer();
				ctlAlertMessage.PushAlertMessage(AppLogic.GetCountryPostalErrorMessage(countryId, cust.SkinID, cust.LocaleSetting), AlertMessage.AlertType.Danger);
				return false;
			}

			if(!Editing && CountryZipDuplicateExists(zipCode, countryId.ToString()))
			{
				ctlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.zipcode.ComboExists", ThisCustomer.LocaleSetting), AlertMessage.AlertType.Danger);
				return false;
			}

			foreach(GridViewRow row in grdTaxRates.Rows)
			{
				var txtTaxRate = (TextBox)row.FindControl("txtTaxRate");
				var litTaxClassId = (Literal)row.FindControl("litTaxClassId");

				int taxClassId;
				int.TryParse(litTaxClassId.Text, out taxClassId);

				decimal taxClassRate;
				decimal.TryParse(txtTaxRate.Text, out taxClassRate);

				try
				{
					SaveTaxRate(countryId, zipCode, taxClassId, taxClassRate);
				}
				catch(Exception exception)
				{
					ctlAlertMessage.PushAlertMessage(exception.Message, AlertMessage.AlertType.Error);
					return false;
				}
			}

			ZipCode = zipCode; //for btnSave redirect to zipcode edit page
			return true;
		}

		private bool CountryZipDuplicateExists(string zipCode, string countryId)
		{
			List<SqlParameter> dupeCheckParams = new List<SqlParameter> { new SqlParameter("@ZipCode", zipCode),
																			new SqlParameter("@CountryID", countryId) };

			bool dupeExists = false;
			string dupeCheck = "SELECT COUNT(*) AS N FROM ZipTaxRate WHERE ZipCode = @ZipCode AND CountryID = @CountryID";
			dupeExists = DB.GetSqlN(dupeCheck, dupeCheckParams.ToArray()) > 0;

			return dupeExists;
		}

		public void SaveTaxRate(int countryId, string zipCode, int taxClassId, decimal rate)
		{
			if(rate == 0)
			{
				var deleteParameters = new SqlParameter[]
				{
					new SqlParameter("@countryId", countryId),
					new SqlParameter("@zipCode", zipCode),
					new SqlParameter("@taxClassId", taxClassId),
				};
				DB.ExecuteSQL("delete from ZipTaxRate where CountryID=@countryId and ZipCode=@zipCode and TaxClassID=@taxClassId", deleteParameters);
			}
			else
			{
				var selectParameters = new SqlParameter[]
				{
					new SqlParameter("@countryId", countryId),
					new SqlParameter("@zipCode", zipCode),
					new SqlParameter("@taxClassId", taxClassId),
				};

				var updateParameters = new SqlParameter[]
				{
					new SqlParameter("@countryId", countryId),
					new SqlParameter("@zipCode", zipCode),
					new SqlParameter("@taxClassId", taxClassId),
					new SqlParameter("@rate", rate)
				};

				if(DB.GetSqlN("select count(*) N from ZipTaxRate where CountryID=@countryId and ZipCode=@zipCode and TaxClassID=@taxClassId", selectParameters) == 0)
					DB.ExecuteSQL("insert ZipTaxRate (CountryID, ZipCode, TaxClassID, TaxRate) values (@countryId, @zipCode, @taxClassId, @rate)", updateParameters);
				else
					DB.ExecuteSQL("update ZipTaxRate set TaxRate = @rate where CountryID=@countryId and ZipCode=@zipCode and TaxClassID=@taxClassId", updateParameters);
			}
		}
	}
}
