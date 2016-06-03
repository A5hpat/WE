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
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class rating : AspDotNetStorefront.Admin.AdminPageBase
	{
		int RatingId = 0;
		
		protected void Page_Load(object sender, EventArgs e)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");

			RatingId = CommonLogic.QueryStringNativeInt("ratingid");

			if(RatingId != 0)
			{
				Editing = true;
			}

			if (!Page.IsPostBack)
			{
				PopulateForm(Editing);
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
			if (!editing)
			{
				ctlAlertMessage.PushAlertMessage("No rating found!", AlertMessage.AlertType.Warning);
				divEditRating.Visible = false;
			}
			else
			{
				string sql = "SELECT * FROM Rating WITH (NOLOCK) WHERE RatingID = @RatingID";
				List<SqlParameter> sqlParams = new List<SqlParameter> { new SqlParameter("@RatingID", RatingId.ToString()) };

				using (SqlConnection dbconn = DB.dbConn())
				{
					dbconn.Open();
					using (IDataReader rs = DB.GetRS(sql, sqlParams.ToArray(), dbconn))
					{
						if (rs.Read())
						{
							litRatingId.Text = RatingId.ToString();
							txtRatingComment.Text = DB.RSField(rs, "Comments");
							txtRating.Text = DB.RSFieldInt(rs, "Rating").ToString();
							cbxHasBadWords.Checked = DB.RSFieldTinyInt(rs, "IsFilthy").ToString() == "0" ? false : true;
						}
					}
				}
			}
		}

		private bool SaveRating()
		{
			bool saved = true; 
			string ratingComment = txtRatingComment.Text.Trim();
			int ratingValue = int.Parse(txtRating.Text.Trim());
			bool isFilthy = cbxHasBadWords.Checked;

			String stringSql = String.Format("exec aspdnsf_updProductRating {0}, {1}, {2}, {3}", RatingId, ratingValue, DB.SQuote(ratingComment), isFilthy);

			try
			{
				DB.ExecuteSQL(stringSql);
				ctlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.orderdetails.UpdateSuccessful", ThisCustomer.LocaleSetting), AlertMessage.AlertType.Success);
			}
			catch (Exception ex)
			{
				ctlAlertMessage.PushAlertMessage(ex.Message, AlertMessage.AlertType.Error);
				saved = false;
			}

			return saved;
		}

		protected void btnSubmit_Click(object sender, EventArgs e)
		{
			if(SaveRating())
				Response.Redirect(String.Format("rating.aspx?ratingid={0}", RatingId));
		}

		protected void btnSaveAndClose_Click(object sender, EventArgs e)
		{
			if(SaveRating())
				Response.Redirect("ratings.aspx");
		}
	}
}
