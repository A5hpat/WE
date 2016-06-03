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
using System.Text;
using System.Web.Profile;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class monthlymaintenance : AspDotNetStorefront.Admin.AdminPageBase
	{
		string Status
		{
			get { return (string)Session["Status"]; }
			set { Session["Status"] = value; }
		}
		
		DateTime StartTime
		{
			get { return Session["StartTime"] == null ? new DateTime(0) : (DateTime)Session["StartTime"]; }
			set { Session["StartTime"] = value; }

		}
		
		DateTime EndTime
		{
			get { return Session["EndTime"] == null ? new DateTime(0) : (DateTime)Session["EndTime"]; }
			set { Session["EndTime"] = value; }
		}
		
		TimeSpan RunningTime
		{
			get
			{
				if (StartTime == new DateTime(0))
					new TimeSpan(0);
				if (EndTime != new DateTime(0))
					return new TimeSpan(EndTime.Ticks - StartTime.Ticks);
				else
					return new TimeSpan(DateTime.Now.Ticks - StartTime.Ticks);
			}
		}
		
		string FormattedRunningTime
		{
			get
			{
				if (RunningTime == TimeSpan.Zero)
					return string.Empty;
				else
					return string.Format("{2}:{1}:{0}",
						(RunningTime.Seconds % 60).ToString("0#"),
						(RunningTime.Minutes % 60).ToString("0#"),
						(RunningTime.Hours).ToString("#0")
						);
			}
		}
		
		bool ControlsEnabled
		{
			set
			{
				btnRunMaintenance.Enabled = value;
				btnRunMaintenanceBottom.Enabled = value;
				ClearAllShoppingCarts.Enabled = value;
				ClearAllWishLists.Enabled = value;
				EraseOrderCreditCards.Enabled = value;
				EraseProfileLog.Enabled = value;
				ClearProductViewsOlderThan.Enabled = value;
				PurgeAnonUsers.Enabled = value;
				EraseAddressCreditCards.Enabled = value;
				PurgeDeletedRecords.Enabled = value;
				TuneIndexes.Enabled = value;
				CleanupSecurityLog.Enabled = value;
				SaveSettings.Enabled = value;
			}
		}

		int GetIndex(DropDownList list, String value)
		{
			int i = 0;
			foreach (ListItem ix in list.Items)
			{
				if (ix.Text.Equals(value, StringComparison.InvariantCultureIgnoreCase))
				{
					break;
				}
				i++;
			}
			return i;
		}
		
		static int MaintenanceTimeout
		{
			get
			{
				if (AppLogic.AppConfig("MonthlyMaintenanceTimeout") != string.Empty)
				{
					return int.Parse(AppLogic.AppConfig("MonthlyMaintenanceTimeout"));
				}
				else
				{
					return 120;
				}
			}
		}

		Action<SqlParameter[]> RunMaintenance = (paramset) =>
		{
			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				using(var cmdMaintenance = new SqlCommand("dbo.aspdnsf_MonthlyMaintenance"))
				{
					cmdMaintenance.CommandType = CommandType.StoredProcedure;
					cmdMaintenance.Parameters.AddRange(paramset);
					con.Open();
					cmdMaintenance.Connection = con;
					cmdMaintenance.CommandTimeout = MaintenanceTimeout * 1000;
					cmdMaintenance.ExecuteNonQuery();
				}
			}
		};

		protected void Page_Load(object sender, EventArgs e)
		{
			Server.ScriptTimeout = 10000; // these could run quite a long time!
			if (!IsPostBack)
			{
				divRunning.Attributes.CssStyle["visibility"] = "hidden";
				
				var SavedSettings = AppLogic.AppConfig("System.SavedMonthlyMaintenance", 0, true);
				if (SavedSettings.Length != 0)
				{
					foreach (String s in SavedSettings.Split(','))
					{
						if (s.Trim().Length != 0)
						{
							var token = s.Trim().Split('=');
							var ParmName = token[0].ToUpper(CultureInfo.InvariantCulture).Trim();
							var ParmValue = token[1].ToUpper(CultureInfo.InvariantCulture).Trim();
							switch (ParmName)
							{
								case "PURGEANONUSERS":
									PurgeAnonUsers.Checked = (ParmValue == "TRUE");
									break;
								case "PURGEDELETEDRECORDS":
									PurgeDeletedRecords.Checked = (ParmValue == "TRUE");
									break;
								case "CLEARALLSHOPPINGCARTS":
									ClearAllShoppingCarts.SelectedValue = ParmValue;
									break;
								case "CLEARALLWISHLISTS":
									ClearAllWishLists.SelectedValue = ParmValue;
									break;
								case "ERASEORDERCREDITCARDS":
									EraseOrderCreditCards.SelectedValue = ParmValue;
									break;
								case "ERASEADDRESSCREDITCARDS":
									EraseAddressCreditCards.Checked = (ParmValue == "TRUE");
									break;
								case "CLEARPRODUCTVIEWSOLDERTHAN":
									ClearProductViewsOlderThan.SelectedValue = ParmValue;
									break;
								case "CLEANUPLOCALIZATIONDATA":
									CleanupLocalizationData.Checked = (ParmValue == "TRUE");
									break;
								case "TUNEINDEXES":
									TuneIndexes.Checked = (ParmValue == "TRUE");
									break;
								case "UPDATESTATISTICS":
									UpdateStatistics.Checked = (ParmValue == "TRUE");
									break;
								case "CLEARPROFILES":
									EraseProfileLog.SelectedValue = ParmValue;
									break;
								case "CLEARRTSHIPPING":
									dlClearRTShippingData.SelectedValue = ParmValue;
									break;
								case "CLEARSEARCH":
									dlClearSearchData.SelectedValue = ParmValue;
									break;
								case "CLEANUPSECURITYLOG":
									CleanupSecurityLog.Checked = (ParmValue == "TRUE");
									break;
								case "SAVESETTINGS":
									SaveSettings.Checked = (ParmValue == "TRUE");
									break;
							}
						}
					}
				}
			}

			if(Status != null && Status == "Completed")
				Response.Redirect(System.IO.Path.GetFileName(Request.PhysicalPath));

			Page.Form.DefaultButton = btnRunMaintenance.UniqueID;
		}

		protected void ShowTime_Tick(object sender, EventArgs e)
		{
			if (Status != "Complete")
			{
				lblStatus.Text = FormattedRunningTime;
			}
			else
			{
				lblStatus.Text = AppLogic.GetString("admin.monthlymaintenance.Done", SkinID, LocaleSetting);
				tmrMain.Enabled = false;
			}
		}

		protected void MaintenanceCallback(IAsyncResult res)
		{
			if (res.IsCompleted)
			{
				Status = "Complete";
				EndTime = DateTime.Now;
				Session.Remove("EndTime");
				Session.Remove("StartTime");
			}
		}
		
		protected void RunMaintenance_Click(object sender, EventArgs e)
		{
			SqlParameter[] spa = 
			{
				DB.CreateSQLParameter("@PurgeAnonCustomers", SqlDbType.TinyInt, 1, CommonLogic.IIF(PurgeAnonUsers.Checked, 1, 0), ParameterDirection.Input),
				DB.CreateSQLParameter("@CleanShoppingCartsOlderThan", SqlDbType.SmallInt, 2, Convert.ToInt16(ClearAllShoppingCarts.SelectedValue), ParameterDirection.Input),
				DB.CreateSQLParameter("@CleanWishListsOlderThan", SqlDbType.SmallInt, 2, Convert.ToInt16(ClearAllWishLists.SelectedValue), ParameterDirection.Input),
				DB.CreateSQLParameter("@EraseCCFromAddresses", SqlDbType.TinyInt, 1, CommonLogic.IIF(EraseAddressCreditCards.Checked, 1, 0), ParameterDirection.Input),
				DB.CreateSQLParameter("@ClearProductViewsOrderThan", SqlDbType.SmallInt, 2, Convert.ToInt16(ClearProductViewsOlderThan.SelectedValue), ParameterDirection.Input),
				DB.CreateSQLParameter("@EraseCCFromOrdersOlderThan", SqlDbType.SmallInt, 2, Convert.ToInt16(EraseOrderCreditCards.SelectedValue), ParameterDirection.Input),
				DB.CreateSQLParameter("@DefragIndexes", SqlDbType.TinyInt, 1, CommonLogic.IIF(TuneIndexes.Checked, 1, 0), ParameterDirection.Input),
				DB.CreateSQLParameter("@UpdateStats", SqlDbType.TinyInt, 1, CommonLogic.IIF(UpdateStatistics.Checked, 1, 0), ParameterDirection.Input),
				DB.CreateSQLParameter("@PurgeDeletedRecords", SqlDbType.TinyInt, 1, CommonLogic.IIF(PurgeDeletedRecords.Checked, 1, 0), ParameterDirection.Input),
				DB.CreateSQLParameter("@RemoveRTShippingDataOlderThan", SqlDbType.SmallInt, 2, Convert.ToInt16(dlClearRTShippingData.SelectedValue), ParameterDirection.Input),
				DB.CreateSQLParameter("@ClearSearchLogOlderThan", SqlDbType.SmallInt, 2, Convert.ToInt16(dlClearSearchData.SelectedValue), ParameterDirection.Input),
				DB.CreateSQLParameter("@CleanOrphanedLocalizedNames", SqlDbType.TinyInt, 2, CleanupLocalizationData.Checked ? 1 : 0, ParameterDirection.Input),
				DB.CreateSQLParameter("@CleanupSecurityLog", SqlDbType.TinyInt, 2, CleanupSecurityLog.Checked ? 1 : 0, ParameterDirection.Input)
			};

			if (Status == null || Status != "Processing")
			{
				Status = "Processing";
				StartTime = DateTime.Now;
				Session.Remove("EndTime");

				RunMaintenance.BeginInvoke(spa, new AsyncCallback(MaintenanceCallback), null);
			}

			if (EraseProfileLog.SelectedIndex != 0)
			{
				using (var con = new SqlConnection(DB.GetDBConn()))
				{
					con.Open();

					var str = "select Date = dateadd(dd,-" + EraseProfileLog.SelectedValue.ToString() + ",GETDATE())";
					if (EraseProfileLog.SelectedIndex == 1)
					{
						str = "select Date = dateadd(dd,-1,GETDATE())";
					}
					else
					{
						str = "select Date = dateadd(dd,-" + EraseProfileLog.SelectedValue.ToString() + ",GETDATE())";
					}

					using (var rs = DB.GetRS(str, con))
					{
						if (rs.Read())
						{
							ProfileManager.DeleteInactiveProfiles(ProfileAuthenticationOption.All, DB.RSFieldDateTime(rs, "Date"));
						}
					}
				}
			}

			if (SaveSettings.Checked)
			{
				var tmpS = new StringBuilder(1024);
				tmpS.Append("PurgeAnonUsers=");
				tmpS.Append(PurgeAnonUsers.Checked);
				tmpS.Append(",");
				tmpS.Append("PurgeDeletedRecords=");
				tmpS.Append(PurgeDeletedRecords.Checked);
				tmpS.Append(",");
				tmpS.Append("ClearAllShoppingCarts=");
				tmpS.Append(ClearAllShoppingCarts.Items[ClearAllShoppingCarts.SelectedIndex].Value);
				tmpS.Append(",");
				tmpS.Append("ClearAllWishLists=");
				tmpS.Append(ClearAllWishLists.Items[ClearAllWishLists.SelectedIndex].Value);
				tmpS.Append(",");
				tmpS.Append("EraseOrderCreditCards=");
				tmpS.Append(EraseOrderCreditCards.Items[EraseOrderCreditCards.SelectedIndex].Value);
				tmpS.Append(",");
				tmpS.Append("EraseAddressCreditCards=");
				tmpS.Append(EraseAddressCreditCards.Checked);
				tmpS.Append(",");
				tmpS.Append("ClearProductViewsOlderThan=");
				tmpS.Append(ClearProductViewsOlderThan.Items[ClearProductViewsOlderThan.SelectedIndex].Value);
				tmpS.Append(",");
				tmpS.Append("TuneIndexes=");
				tmpS.Append(TuneIndexes.Checked);
				tmpS.Append(",");
				tmpS.Append("UpdateStatistics=");
				tmpS.Append(UpdateStatistics.Checked);
				tmpS.Append(",");
				tmpS.Append("CleanUpLocalizationData=");
				tmpS.Append(CleanupLocalizationData.Checked);
				tmpS.Append(",");
				tmpS.Append("ClearProfiles=");
				tmpS.Append(EraseProfileLog.Items[EraseProfileLog.SelectedIndex].Value);
				tmpS.Append(",");
				tmpS.Append("ClearRTShipping=");
				tmpS.Append(dlClearRTShippingData.Items[dlClearRTShippingData.SelectedIndex].Value);
				tmpS.Append(",");
				tmpS.Append("ClearSearch=");
				tmpS.Append(dlClearSearchData.Items[dlClearSearchData.SelectedIndex].Value);
				tmpS.Append(",");
				tmpS.Append("CleanUpSecurityLog=");
				tmpS.Append(CleanupSecurityLog.Checked);
				tmpS.Append(",");
				tmpS.Append("SaveSettings=");
				tmpS.Append(SaveSettings.Checked);
				AppConfigManager.SetAppConfigValue("System.SavedMonthlyMaintenance", tmpS.ToString());
			}

			//Schedule the next maintenance alert
			AppConfigManager.SetAppConfigValue("NextMaintenanceDate", DateTime.Now.AddMonths(1).ToString());

			Response.Redirect(System.IO.Path.GetFileName(Request.PhysicalPath));
		}
	}
}
