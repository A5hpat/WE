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
	public partial class stringresourceeditor : AspDotNetStorefront.Admin.AdminPageBase
	{
		int StringId = 0;		

		protected void Page_Load(object sender, EventArgs e)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");

			StringId = CommonLogic.QueryStringNativeInt("stringid");

			if(StringId != 0)
			{
				Editing = true;
			}

			if (!Page.IsPostBack)
			{
				LoadLocales();
				LoadStores();
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
				Title = HeaderText.Text = "admin.editstringresource.New".StringResource();
			}
			else
			{
				Title = HeaderText.Text = "admin.editstringresource.Edit".StringResource();

				string sql = "SELECT * FROM StringResource WITH (NOLOCK) WHERE StringResourceID = @StringResourceID";
				List<SqlParameter> sqlParams = new List<SqlParameter> { new SqlParameter("@StringResourceID", StringId.ToString()) };

				using (SqlConnection dbconn = DB.dbConn())
				{
					dbconn.Open();
					using (IDataReader rs = DB.GetRS(sql, sqlParams.ToArray(), dbconn))
					{
						if (rs.Read())
						{
							litStringId.Text = DB.RSFieldInt(rs, "StringResourceID").ToString();
							txtStringName.Text = DB.RSField(rs, "Name");
							txtValue.Text = DB.RSField(rs, "ConfigValue");
							ddLocales.SelectedValue = DB.RSField(rs, "LocaleSetting");
							ddStores.SelectedValue = DB.RSFieldInt(rs, "StoreID").ToString();
						}
					}
				}
			}
		}

		private void LoadLocales()
		{
			ddLocales.Items.Clear();

			using(SqlConnection conn = new SqlConnection(DB.GetDBConn()))
			{
				conn.Open();
				using(IDataReader localeReader = DB.GetRS("SELECT Name, Description FROM LocaleSetting  with (NOLOCK)  ORDER BY DisplayOrder,Name", conn))
				{
					while(localeReader.Read())
					{
						ddLocales.Items.Add(new ListItem(DB.RSField(localeReader, "Description"), DB.RSField(localeReader, "Name")));
					}
				}
			}
		}

		private void LoadStores()
		{
			List<Store> stores = Store.GetStoreList();

			foreach (Store store in stores)
			{
				ddStores.Items.Add(new ListItem(store.Name, store.StoreID.ToString()));
			}
		}

		private bool SaveString()
		{
			string stringName = txtStringName.Text.Trim();
			string stringValue = txtValue.Text.Trim();
			string stringLocale = ddLocales.SelectedValue;
			string stringStore = ddStores.SelectedValue;

			bool saved = true;
			StringBuilder stringSql = new StringBuilder();
			var stringParams = new List<SqlParameter> {new SqlParameter("@Name", stringName),
														new SqlParameter("@ConfigValue", stringValue),
														new SqlParameter("@LocaleSetting", stringLocale),
														new SqlParameter("@StoreID", stringStore)
														};

			if (Editing)
			{
				int stringId = int.Parse(litStringId.Text);
				stringParams.Add(new SqlParameter("@StringResourceID", stringId));

				stringSql.Append("UPDATE StringResource SET Name = @Name,");
				stringSql.Append("ConfigValue = @ConfigValue,");
				stringSql.Append("LocaleSetting = @LocaleSetting,");
				stringSql.Append("StoreID = @StoreID,");
				stringSql.Append("Modified = 1");
				stringSql.Append(" WHERE StringResourceID = @StringResourceID");
			}
			else
			{
				stringSql.Append("INSERT INTO StringResource (Name, ConfigValue, LocaleSetting, StoreID, CreatedOn)");
				stringSql.Append("VALUES (@Name, @ConfigValue, @LocaleSetting, @StoreID, getDate())");
			}

			try
			{
				DB.ExecuteSQL(stringSql.ToString(), stringParams.ToArray());
				ctlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.orderdetails.UpdateSuccessful", ThisCustomer.LocaleSetting), AlertMessage.AlertType.Success);
			}
			catch (Exception ex)
			{
				ctlAlertMessage.PushAlertMessage(ex.Message, AlertMessage.AlertType.Error);
				saved = false;
			}

			if (!Editing)
			{
				//Added a new string - get the ID so we can reload the form
				StringId = DB.GetSqlN("SELECT TOP 1 StringResourceID N FROM StringResource ORDER BY StringResourceID DESC");
			}

			return saved;
		}

		protected void btnSubmit_Click(object sender, EventArgs e)
		{
			if(SaveString())
				Response.Redirect(String.Format("stringresource.aspx?stringid={0}", StringId));
		}

		protected void btnSaveAndClose_Click(object sender, EventArgs e)
		{
			if(SaveString())
				Response.Redirect(ReturnUrlTracker.GetReturnUrl());
		}
	}
}
