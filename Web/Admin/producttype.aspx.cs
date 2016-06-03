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
	public partial class ProductType : AspDotNetStorefront.Admin.AdminPageBase
	{
		int TypeId = 0;
		
		protected void Page_Load(object sender, EventArgs e)
		{
			TypeId = CommonLogic.QueryStringNativeInt("typeid");

			if(TypeId != 0)
			{
				Editing = true;
			}

			if(!Page.IsPostBack)
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
			if(!editing)
			{
				litHeader.Text = "Create Product Type";
			}
			else
			{
				litHeader.Text = "Edit Product Type";

				string sql = "SELECT * FROM ProductType WITH (NOLOCK) WHERE ProductTypeID = @ProductTypeID";
				List<SqlParameter> sqlParams = new List<SqlParameter> { new SqlParameter("@ProductTypeID", TypeId.ToString()) };

				using(SqlConnection dbconn = DB.dbConn())
				{
					dbconn.Open();
					using(IDataReader rs = DB.GetRS(sql, sqlParams.ToArray(), dbconn))
					{
						if(rs.Read())
						{
							litTypeId.Text = DB.RSFieldInt(rs, "ProductTypeID").ToString();
							txtTypeName.Text = DB.RSField(rs, "Name");
							txtDisplayOrder.Text = DB.RSFieldInt(rs, "DisplayOrder").ToString();
						}
					}
				}
			}
		}

		protected void btnSubmit_Click(object sender, EventArgs e)
		{
			if(SaveType())
				Response.Redirect(String.Format("producttype.aspx?typeid={0}", TypeId));
		}

		protected void btnSaveAndClose_Click(object sender, EventArgs e)
		{
			if(SaveType())
				Response.Redirect(ReturnUrlTracker.GetReturnUrl());
		}

		private bool SaveType()
		{
			bool saved = true;
			string saveSql = String.Empty;
			string typeName = txtTypeName.Text.Trim();
			int displayOrder = 1;

			int.TryParse(txtDisplayOrder.Text.Trim(), out displayOrder);

			List<SqlParameter> saveParams = new List<SqlParameter> { new SqlParameter("@Name", typeName),
																	new SqlParameter("@DisplayOrder", displayOrder) };

			if(Editing)
			{
				int typeId = int.Parse(litTypeId.Text);

				saveParams.Add(new SqlParameter("@ProductTypeID", typeId));

				saveSql = "UPDATE ProductType SET Name = @Name, DisplayOrder = @DisplayOrder WHERE ProductTypeID = @ProductTypeID";
			}
			else
			{
				saveSql = "INSERT INTO ProductType (Name, DisplayOrder) VALUES (@Name, @DisplayOrder)";
			}

			try
			{
				DB.ExecuteSQL(saveSql, saveParams.ToArray());
				ctlAlertMessage.PushAlertMessage("admin.editproducttype.UpdateSuccessful".StringResource(), AlertMessage.AlertType.Success);
			}
			catch(Exception ex)
			{
				ctlAlertMessage.PushAlertMessage(ex.Message, AlertMessage.AlertType.Error);
				saved = false;
			}

			if(!Editing)
			{
				//Added a new type - get the ID so we can reload the form
				TypeId = DB.GetSqlN("SELECT TOP 1 ProductTypeID N FROM ProductType ORDER BY ProductTypeID DESC");
			}

			return saved;
		}
	}
}
