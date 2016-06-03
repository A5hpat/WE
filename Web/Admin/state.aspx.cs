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
	public partial class _State : AspDotNetStorefront.Admin.AdminPageBase
	{
		int StateId = 0;
		
		protected void Page_Load(object sender, EventArgs e)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");

			StateId = CommonLogic.QueryStringNativeInt("StateId");

			if (StateId != 0)
			{
				Editing = true;
			}

			if(!Page.IsPostBack)
			{
				PopulateForm(Editing);
				LoadCountries();
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
				litHeader.Text = AppLogic.GetString("admin.AddState", ThisCustomer.LocaleSetting);
			}
			else
			{
				litHeader.Text = AppLogic.GetString("admin.EditState", ThisCustomer.LocaleSetting);

				string sql = "SELECT * FROM State WITH (NOLOCK) WHERE StateID = @StateID";
				List<SqlParameter> sqlParams = new List<SqlParameter> { new SqlParameter("@StateID", StateId.ToString()) };

				using(SqlConnection dbconn = DB.dbConn())
				{
					dbconn.Open();
					using(IDataReader rs = DB.GetRS(sql, sqlParams.ToArray(), dbconn))
					{
						if(rs.Read())
						{
							litStateId.Text = DB.RSFieldInt(rs, "StateID").ToString();
							txtStateName.Text = DB.RSField(rs, "Name");
							txtStateAbbreviation.Text = DB.RSField(rs, "Abbreviation");
							txtDisplayOrder.Text = DB.RSFieldInt(rs, "DisplayOrder").ToString();
							cbxPublished.Checked = DB.RSFieldBool(rs, "Published");
						}
					}
				}
			}
		}

		private void LoadCountries()
		{
			using(SqlConnection con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(IDataReader rsst = DB.GetRS("SELECT CountryID, Name FROM Country WITH (NOLOCK) WHERE Published = 1 ORDER BY DisplayOrder,Name", con))
				{
					while(rsst.Read())
					{
						ddCountry.Items.Add(new ListItem(DB.RSField(rsst, "Name"), DB.RSFieldInt(rsst, "CountryID").ToString()));
					}
				}
			}
		}

		private void BindGrid()
		{
			using(SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
			{
				StringBuilder sql = new StringBuilder("SELECT TaxClassID, Name FROM TaxClass ORDER BY TaxClassID");

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

			if (txtTaxRate != null && litTaxClassId != null)
			{
				int taxClassId = 0;

				if (int.TryParse(litTaxClassId.Text, out taxClassId))
				{
					decimal taxClassRate = DB.GetSqlNDecimal(String.Format("SELECT TaxRate N FROM StateTaxRate WHERE StateID = {0} AND TaxClassID = {1}", StateId, taxClassId));
					txtTaxRate.Text = taxClassRate.ToString();
				}
			}
		}

		protected void grdTaxRates_OnPageIndexChanging(object sender, GridViewPageEventArgs e)
		{
			grdTaxRates.PageIndex = e.NewPageIndex;
			BindGrid();
		}

		protected void btnSubmit_Click(object sender, EventArgs e)
		{
			if(SaveState())
				Response.Redirect(String.Format("state.aspx?stateid={0}", StateId));
		}

		protected void btnSaveAndClose_Click(object sender, EventArgs e)
		{
			if(SaveState())
				Response.Redirect(ReturnUrlTracker.GetReturnUrl());
		}

		private bool SaveState()
		{
			bool saved = true;
			string stateSql = String.Empty;
			string stateName = txtStateName.Text.Trim();
			string stateAbbrev = txtStateAbbreviation.Text.Trim();
			bool published = cbxPublished.Checked;
			int countryId = int.Parse(ddCountry.SelectedValue);
			int displayOrder = 1;

			int.TryParse(txtDisplayOrder.Text.Trim(), out displayOrder);

			List<SqlParameter> stateParams = new List<SqlParameter> { new SqlParameter("@Name", stateName),
																	new SqlParameter("@Abbreviation", stateAbbrev),
																	new SqlParameter("@DisplayOrder", displayOrder),
																	new SqlParameter("@Published", published),
																	new SqlParameter("@CountryID", countryId) };

			if(Editing)
			{
				int stateId = int.Parse(litStateId.Text);

				stateParams.Add(new SqlParameter("@StateID", stateId));

				stateSql = "UPDATE State SET Name = @Name, Abbreviation = @Abbreviation, Published = @Published, DisplayOrder = @DisplayOrder, CountryID = @CountryID WHERE StateID = @StateID";
			}
			else
			{
				stateSql = "INSERT INTO State (Name, Abbreviation, Published, DisplayOrder, CountryID) VALUES (@Name, @Abbreviation, @Published, @DisplayOrder, @CountryID)";
			}

			try
			{
				DB.ExecuteSQL(stateSql, stateParams.ToArray());
				ctlAlertMessage.PushAlertMessage("admin.orderdetails.UpdateSuccessful".StringResource(), AlertMessage.AlertType.Success);
			}
			catch(Exception ex)
			{
				ctlAlertMessage.PushAlertMessage(ex.Message, AlertMessage.AlertType.Error);
				saved = false;
			}

			if(!Editing)
			{
				//Added a new state - get the ID so we can reload the form & save tax rates
				StateId = DB.GetSqlN("SELECT TOP 1 StateID N FROM State ORDER BY StateID DESC");
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
					SaveTaxRate(StateId, taxClassId, taxClassRate);
				}
				catch(Exception exception)
				{
					ctlAlertMessage.PushAlertMessage(exception.Message, AlertMessage.AlertType.Error);
					return false;
				}
			}

			return true;
		}

		public void SaveTaxRate(int stateId, int taxClassId, decimal rate)
		{
			if(rate == 0)
			{
				var deleteParameters = new SqlParameter[]
				{
					new SqlParameter("@stateId", stateId),
					new SqlParameter("@taxClassId", taxClassId),
				};
				DB.ExecuteSQL("delete from StateTaxRate where StateID=@stateId and TaxClassID=@taxClassId", deleteParameters);
			}
			else
			{
				var selectParameters = new SqlParameter[]
				{
					new SqlParameter("@stateId", stateId),
					new SqlParameter("@taxClassId", taxClassId),
				};

				var updateParameters = new SqlParameter[]
				{
					new SqlParameter("@stateId", stateId),
					new SqlParameter("@taxClassId", taxClassId),
					new SqlParameter("@rate", rate)
				};

				if(DB.GetSqlN("select count(*) N from StateTaxRate where StateID=@stateId and TaxClassID=@taxClassId", selectParameters) == 0)
					DB.ExecuteSQL("insert StateTaxRate (StateID, TaxClassID, TaxRate) values (@stateId, @taxClassId, @rate)", updateParameters);
				else
					DB.ExecuteSQL("update StateTaxRate set TaxRate=@rate where StateID=@stateId and TaxClassID=@taxClassId", updateParameters);
			}
		}
	}
}
