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
using AspDotNetStorefrontCore;
using AspDotNetStorefrontControls;

namespace AspDotNetStorefrontAdmin
{
	/// <summary>
	/// Summary description for countries.
	/// </summary>
	public partial class country : AspDotNetStorefront.Admin.AdminPageBase
	{
		int CountryId = 0;

		protected void Page_Load(object sender, EventArgs e)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");

			CountryId = CommonLogic.QueryStringNativeInt("CountryId");

			if (CountryId != 0)
			{
				Editing = true;
			}

			if (!Page.IsPostBack)
			{
				PopulateForm(Editing);
				BindGrid();
			}
		}

		protected override void OnPreRender(EventArgs e)
		{
			DataBind();

			base.OnPreRender(e);
		}

		private void PopulateForm(bool editing)
		{
			if (!editing)
			{
				litHeader.Text = "admin.common.Add".StringResource() + " " + "admin.common.Country".StringResource();
			}
			else
			{
				litHeader.Text = "admin.common.Edit".StringResource() + " " + "admin.common.Country".StringResource();

				var sql = "SELECT * FROM Country WITH (NOLOCK) WHERE CountryID = @CountryID";
				var sqlParams = new List<SqlParameter> { new SqlParameter("@CountryID", CountryId.ToString()) };

				using (var dbconn = DB.dbConn())
				{
					dbconn.Open();
					using (var rs = DB.GetRS(sql, sqlParams.ToArray(), dbconn))
					{
						if (rs.Read())
						{
							litCountryId.Text = DB.RSFieldInt(rs, "CountryID").ToString();
							cbxPublished.Checked = DB.RSFieldBool(rs, "Published");
							txtCountryName.Text = DB.RSField(rs, "Name");
							txtTwoLetterISOCode.Text = DB.RSField(rs, "TwoLetterISOCode");
							txtThreeLetterISOCode.Text = DB.RSField(rs, "ThreeLetterISOCode");
							txtNumericISOCode.Text = DB.RSField(rs, "NumericISOCode");
							cbxPostalCodeRequired.Checked = DB.RSFieldBool(rs, "PostalCodeRequired");
							txtPostalCodeRegEx.Text = DB.RSField(rs, "PostalCodeRegEx").ToString();
							txtPostalCodeExample.Text = DB.RSField(rs, "PostalCodeExample").ToString();
							txtDisplayOrder.Text = DB.RSFieldInt(rs, "DisplayOrder").ToString();
						}
					}
				}
			}
		}

		private void BindGrid()
		{
			using (var dbconn = new SqlConnection(DB.GetDBConn()))
			{
				var sql = new StringBuilder("SELECT TaxClassID, Name FROM TaxClass ORDER BY TaxClassID");

				dbconn.Open();
				using (var rs = DB.GetRS(sql.ToString(), dbconn))
				{
					using (var dt = new DataTable())
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

			if (txtTaxRate != null && litTaxClassId != null)
			{
				int taxClassId = 0;

				if (int.TryParse(litTaxClassId.Text, out taxClassId))
				{
					decimal taxClassRate = DB.GetSqlNDecimal(String.Format("SELECT TaxRate N FROM CountryTaxRate WHERE CountryID = {0} AND TaxClassID = {1}", CountryId, taxClassId));
					txtTaxRate.Text = taxClassRate.ToString();
				}
			}
		}

		protected void grdTaxRates_OnPageIndexChanging(object sender, GridViewPageEventArgs e)
		{
			grdTaxRates.PageIndex = e.NewPageIndex;
			BindGrid();
		}

		private bool SaveCountry()
		{
			bool published = cbxPublished.Checked;
			string countryName = txtCountryName.Text.Trim();
			string twoLetterISOCode = txtTwoLetterISOCode.Text.Trim();
			string threeLetterISOCode = txtThreeLetterISOCode.Text.Trim();
			string numericISOCode = txtNumericISOCode.Text.Trim();
			bool postalCodeRequired = cbxPostalCodeRequired.Checked;
			string postalCodeRegEx = txtPostalCodeRegEx.Text.Trim();
			string postalCodeExample = txtPostalCodeExample.Text.Trim();
			int	displayOrder = 1; //default
			int.TryParse(txtDisplayOrder.Text.Trim(), out displayOrder);

			bool saved = true;
			string countrySql = String.Empty;
			var countryParams = new List<SqlParameter> {new SqlParameter("@Published", published),
														new SqlParameter("@Name", countryName),
														new SqlParameter("@TwoLetterISOCode", twoLetterISOCode),
														new SqlParameter("@ThreeLetterISOCode", threeLetterISOCode),
														new SqlParameter("@NumericISOCode", numericISOCode),
														new SqlParameter("@PostalCodeRequired", postalCodeRequired),
														new SqlParameter("@PostalCodeRegEx", postalCodeRegEx),
														new SqlParameter("@PostalCodeExample", postalCodeExample),
														new SqlParameter("@DisplayOrder", displayOrder)
														};

			if (Editing)
			{
				int countryId = int.Parse(litCountryId.Text);
				countryParams.Add(new SqlParameter("@CountryID", countryId));

				countrySql = "UPDATE Country SET Published = @Published,";
				countrySql += "Name = @Name,";
				countrySql += "TwoLetterISOCode = @TwoLetterISOCode,";
				countrySql += "ThreeLetterISOCode = @ThreeLetterISOCode,";
				countrySql += "NumericISOCode = @NumericISOCode,";
				countrySql += "PostalCodeRequired = @PostalCodeRequired,";
				countrySql += "PostalCodeRegEx = @PostalCodeRegEx,";
				countrySql += "PostalCodeExample = @PostalCodeExample,";
				countrySql += "DisplayOrder = @DisplayOrder";
				countrySql += " WHERE CountryID = @CountryID";
			}
			else
			{
				countrySql = "INSERT INTO Country (Published, Name, TwoLetterISOCode, ThreeLetterISOCode, NumericISOCode, PostalCodeRequired, PostalCodeRegEx, PostalCodeExample, DisplayOrder)";
				countrySql += "           VALUES (@Published, @Name, @TwoLetterISOCode, @ThreeLetterISOCode, @NumericISOCode, @PostalCodeRequired, @PostalCodeRegEx, @PostalCodeExample, @DisplayOrder)";
			}

			try
			{
				DB.ExecuteSQL(countrySql, countryParams.ToArray());
				ctlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.orderdetails.UpdateSuccessful", ThisCustomer.LocaleSetting), AlertMessage.AlertType.Success);
			}
			catch (Exception ex)
			{
				ctlAlertMessage.PushAlertMessage(ex.Message, AlertMessage.AlertType.Error);
				saved = false;
			}

			if (!Editing)
			{
				//Added a new country - get the ID so we can reload the form & save tax rates
				CountryId = DB.GetSqlN("SELECT TOP 1 CountryID N FROM Country ORDER BY CountryID DESC");
			}

			saved = SaveTaxes();

			return saved;
		}

		private bool SaveTaxes()
		{
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
					SaveTaxRate(CountryId, taxClassId, taxClassRate);
				}
				catch(Exception exception)
				{
					ctlAlertMessage.PushAlertMessage(exception.Message, AlertMessage.AlertType.Error);
					return false;
				}
			}

			return true;
		}

		protected void btnSubmit_Click(object sender, EventArgs e)
		{
			if (SaveCountry())
				Response.Redirect(String.Format("country.aspx?countryid={0}", CountryId));
		}

		protected void btnClose_Click(object sender, EventArgs e)
		{
			if (SaveCountry())
				Response.Redirect(ReturnUrlTracker.GetReturnUrl());
		}

		public void SaveTaxRate(int countryId, int taxClassId, decimal rate)
		{
			if(rate == 0)
			{
				var deleteParameters = new SqlParameter[]
				{
					new SqlParameter("@countryId", countryId),
					new SqlParameter("@taxClassId", taxClassId),
				};
				DB.ExecuteSQL("delete from CountryTaxRate where CountryID=@countryId and TaxClassID=@taxClassId", deleteParameters);
			}
			else
			{
				var selectParameters = new SqlParameter[]
				{
					new SqlParameter("@countryId", countryId),
					new SqlParameter("@taxClassId", taxClassId),
				};

				var updateParameters = new SqlParameter[]
				{
					new SqlParameter("@countryId", countryId),
					new SqlParameter("@taxClassId", taxClassId),
					new SqlParameter("@rate", rate)
				};

				if(DB.GetSqlN("select count(*) N from CountryTaxRate where CountryID=@countryId and TaxClassID=@taxClassId", selectParameters) == 0)
					DB.ExecuteSQL("insert CountryTaxRate (CountryID, TaxClassID, TaxRate) values (@countryId, @taxClassId, @rate)", updateParameters);
				else
					DB.ExecuteSQL("update CountryTaxRate set TaxRate=@rate where CountryID=@countryId and TaxClassID=@taxClassId", updateParameters);
			}
		}
	}
}


