// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class ShippingMethod : AspDotNetStorefront.Admin.AdminPageBase
	{
		bool ShippingMethodStoreFilteringEnabled = AppLogic.GlobalConfigBool("AllowShippingFiltering");
		protected int ShippingMethodID { get; set; }
		public int StoreFilter { get; set; }
		private Guid ShippingMethodGuid { get; set; }
		public string SelectedImageFileName { get; set; }

		public bool IsRealTime
		{
			get
			{
				return (bool?)ViewState["IsRealTimeShipping"] ?? false;
			}
			set
			{
				ViewState["IsRealTimeShipping"] = value;
			}
		}

		protected void Page_Load(object sender, System.EventArgs e)
		{
			StoreFilter = Request.QueryStringNativeInt("StoreId");

			//get the id from the querystring
			int tempMethodId = 0;
			if(CommonLogic.QueryStringCanBeDangerousContent("ShippingMethodID").Length != 0
				&& CommonLogic.QueryStringCanBeDangerousContent("ShippingMethodID") != "0"
				&& int.TryParse(CommonLogic.QueryStringCanBeDangerousContent("ShippingMethodID"), out tempMethodId))
			{
				Editing = true;
				ShippingMethodID = tempMethodId;
			}
			//otherwise get it from the viewstate
			else if(ViewState["ShippingMethodID"] != null)
			{
				Editing = true;
				ShippingMethodID = (int)ViewState["ShippingMethodID"];
			}

			if(!IsPostBack)
				BindPage();

			//bind the visible attributes on the frontend
			plcNavButtonsTop.Visible = Editing && !IsRealTime;
			plcNavButtons.Visible = Editing && !IsRealTime;
			btnDeleteTop.Visible = Editing;
			btnDelete.Visible = Editing;
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			btnClose.DataBind();
			btnCloseTop.DataBind();
		}

		private void BindPage()
		{
			using(SqlConnection dbconn = DB.dbConn())
			{
				dbconn.Open();
				using(IDataReader rs = DB.GetRS("select Name, DisplayName, IsRTShipping, ImageFileName from ShippingMethod with (NOLOCK) where ShippingMethodID=" + ShippingMethodID.ToString(), dbconn))
				{
					if(rs.Read())
					{
						Editing = true;
					}

					if(DB.RSFieldBool(rs, "IsRTShipping"))
					{
						DisplayNameLocaleField.Text = DB.RSField(rs, "DisplayName");
						IsRealTime = true;
					}
					else
					{
						DisplayNameRow.Visible = false;
					}

					NameLocaleField.Text = DB.RSField(rs, "Name");
					SelectedImageFileName = DB.RSField(rs, "ImageFileName");
					hdnImageFileName.Value = SelectedImageFileName;
				}
			}

			// Databind the image file name repeater
			ImageFileNameList.DataSource = Shipping.GetShippingIconFileNames();

			if(ShippingMethodStoreFilteringEnabled)
			{
				StoreMappingPanel.Visible = true;
				MappedStores.DataSource = Store.GetStoreList();
				MappedStores.DataValueField = "StoreID";
				MappedStores.DataTextField = "Name";
				MappedStores.DataBind();

				//wire up existing mappings
				if(Editing)
				{
					//bind the store mapper
					var sqlParameters = new SqlParameter[]{
						new SqlParameter{
							ParameterName = "ShippingMethodID",
							Value = ShippingMethodID
						}
					};
					var sql = "SELECT StoreId FROM ShippingMethodStore WITH (NOLOCK) WHERE ShippingMethodId = @ShippingMethodID";

					using(SqlConnection conn = new SqlConnection(DB.GetDBConn()))
					{
						conn.Open();

						using(IDataReader rsMap = DB.GetRS(sql, sqlParameters, conn))
						{
							while(rsMap.Read())
							{
								var storeId = rsMap.FieldInt("StoreId");
								foreach(ListItem item in MappedStores.Items)
									if(item.Value == storeId.ToString())
										item.Selected = true;
							}
						}
					}
				}
			}

			ImageFileNameList.DataBind();
		}

		SqlCommand CreateUpdateCommand()
		{
			var updateCommand = new SqlCommand(
				@"IF NOT EXISTS(SELECT ShippingMethodGUID FROM ShippingMethod WHERE ShippingMethodID = @ID)
				BEGIN
					INSERT INTO ShippingMethod(ShippingMethodGUID, [Name], DisplayName, ImageFileName) 
					VALUES (@GUID, @Name, @DisplayName, @ImageFileName)
					SELECT @ID = ShippingMethodID FROM ShippingMethod WHERE ShippingMethodGUID = @GUID
				END
				ELSE
				BEGIN
					DECLARE @ISRtShipping tinyint
					SELECT @IsRtShipping  = IsRTShipping FROM ShippingMethod WHERE ShippingMethodID = @ID
					UPDATE ShippingMethod SET
					[Name] = CASE WHEN @IsRtShipping = 1 Then [Name] ELSE @Name END,
					DisplayName = @DisplayName,
					ImageFileName = @ImageFileName
					WHERE 
					ShippingMethodID = @ID
				END"
			);

			updateCommand.Parameters.AddRange(new SqlParameter[] {
				new SqlParameter("@GUID", ShippingMethodGuid),
				new SqlParameter("@Name", NameLocaleField.GetTextFromFields()),
				new SqlParameter("@DisplayName", DisplayNameLocaleField.GetTextFromFields()),
				new SqlParameter("@ImageFileName", SqlDbType.Text)
				{
					Value = !string.IsNullOrEmpty(hdnImageFileName.Value)
						? (object)hdnImageFileName.Value
						: DBNull.Value
				},
				new SqlParameter("@ID", SqlDbType.Int)
				{
					Value = DBNull.Value,
					Direction = ParameterDirection.InputOutput
				}
			});

			return updateCommand;
		}

		protected bool SaveShippingMethod()
		{
			bool saved = true;

			try
			{
				if(!Editing)
				{
					// ok to add:
					ShippingMethodGuid = new Guid(DB.GetNewGUID());
					using(var updateCommand = CreateUpdateCommand())
					{
						DB.ExecuteSQL(updateCommand);
						ShippingMethodID = (int)updateCommand.Parameters["@ID"].Value;
					}
					ViewState["ShippingMethodID"] = ShippingMethodID;
				}
				else
				{
					// ok to update:
					using(var updateCommand = CreateUpdateCommand())
					{
						updateCommand.Parameters["@ID"].Value = ShippingMethodID;
						DB.ExecuteSQL(updateCommand);
					}
				}

				// for the store mapping
				if(ShippingMethodStoreFilteringEnabled)
				{
					DB.ExecuteSQL("DELETE ShippingMethodStore WHERE ShippingMethodId = @shippingMethodId", new[]
					{
						new SqlParameter("@shippingMethodId", ShippingMethodID)
					});

					foreach(ListItem item in MappedStores.Items)
					{
						if(item.Selected)
						{
							DB.ExecuteSQL("INSERT INTO ShippingMethodStore(StoreId, ShippingMethodId) Values(@storeId, @shippingMethodId)", new[]
							{
								new SqlParameter("@storeId", item.Value),
								new SqlParameter("@shippingMethodId", ShippingMethodID),
							});
						}
					}
				}

				BindPage();
				NameLocaleField.BindData();
				DisplayNameLocaleField.BindData();
				AlertMessage.PushAlertMessage("Updated", AspDotNetStorefrontControls.AlertMessage.AlertType.Success);
			}
			catch(Exception exception)
			{
				AlertMessage.PushAlertMessage(exception.Message, AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
				saved = false;
			}

			return saved;
		}

		protected void SubmitButton_Click(object sender, EventArgs e)
		{
			if(SaveShippingMethod())
				Response.Redirect(String.Format("shippingmethod.aspx?ShippingMethodID={0}", ShippingMethodID));
		}

		protected void btnSaveAndClose_Click(object sender, EventArgs e)
		{
			if(SaveShippingMethod())
				Response.Redirect(ReturnUrlTracker.GetReturnUrl());
		}

		protected void Delete_Click(object sender, EventArgs e)
		{
			var sqlParams = new[] {
				new SqlParameter("@ShippingMethodId", ShippingMethodID),
			};

			DB.ExecuteSQL(@"delete from ShippingByTotal where ShippingMethodID = @ShippingMethodId
							delete from ShippingByWeight where ShippingMethodID = @ShippingMethodId
							delete from ShippingWeightByZone where ShippingMethodID = @ShippingMethodId
							delete from ShippingTotalByZone where ShippingMethodID = @ShippingMethodId
							delete from ShippingMethod where ShippingMethodID = @ShippingMethodId
							delete from ShippingMethodToStateMap where ShippingMethodID = @ShippingMethodId
							delete from ShippingMethodToCountryMap where ShippingMethodID = @ShippingMethodId
							delete from ShippingMethodToZoneMap where ShippingMethodID = @ShippingMethodId
							delete from ShippingMethodStore where ShippingMethodID = @ShippingMethodId
							update shoppingcart set ShippingMethodID=0, ShippingMethod=NULL where ShippingMethodID = @ShippingMethodId", sqlParams);

			if(IsRealTime)
				Response.Redirect(AppLogic.AdminLinkUrl("shippingmethodsrealtime.aspx"));

			Response.Redirect(AppLogic.AdminLinkUrl("shippingmethods.aspx"));
		}
	}
}
