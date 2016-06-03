// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class Admin_setupFTS : AspDotNetStorefront.Admin.AdminPageBase
	{
		ftsddl FTSHelper = new ftsddl();

		protected string JSwarnProceed = string.Empty;
		protected string JSwarnInLocation = string.Empty;
		protected string JSwarnInDefaultLocation = string.Empty;
		protected string JSwarnDefineName = string.Empty;
		protected string JSwarnDefineNamePath = string.Empty;
		protected string JSwarnSelectCatalog = string.Empty;
		protected string JSwarnReuseCatalog = string.Empty;
		protected string JSwarnDefineSingleCatalog = string.Empty;
		protected string JSwarnUninstallFTS = string.Empty;
		protected string JSwarnOptimizeFTS = string.Empty;
		protected string JSwarnSelectRadioButton = string.Empty;


		protected void Page_Load(object sender, EventArgs e)
		{
			radioCreate.Attributes.Add("onclick", "CreateNew();");
			radioReuse.Attributes.Add("onclick", "Reuse();");
			lstCatalogNames.Attributes.Add("disabled", "true");

			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");

			loadLocaleStrings();

			if(!IsPostBack)
			{
				ShowForm();
			}
			else
			{
				if(!FullTextProductIndexExists() && btnInstallFTS.Visible)
				{
					InstallFTS();
				}
			}

			if(!btnInstallFTS.Visible)
				AlertMessageFTSIsEnabledAndWorking.PushAlertMessage("setupFTS.FullTextSearchIsInstalled".StringResource(), AlertMessage.AlertType.Success);
		}

		private void ShowForm()
		{
			try
			{
				if(FullTextProductIndexExists())
				{
					ShowControlsForWhenFtsIsInstalled();
					return;
				}

				btnUninstallFTS.Visible = false;
				btnOptimize.Visible = false;

				if(FullTextEngineIsSupported())
				{
					if(FtsCatalogsAvailable() || FullTextSearchIsEnabled())
					{
						DisplayCatalogsIfTheyExist();
					}
					else
					{
						ctlAlertMessageFTSDisabled.PushAlertMessage("setupFTS.FtsIsDisabled".StringResource(), AlertMessage.AlertType.Info);
						ShowControlsForWhenNoFtsCatalogsExist();
					}
				}
				else
				{
					ctlAlertMessageFTSSetup.PushAlertMessage("setupFTS.WarningMSFullTextIsNotInstalled".StringResource(), AlertMessage.AlertType.Warning);
					HideInstallationControls();
				}
			}
			catch(Exception ex)
			{
				string error = ex.Message;
				ctlAlertMessage1.PushAlertMessage(error, AlertMessage.AlertType.Error);
			}
			return;
		}

		private void InstallFTS()
		{
			bool enableFtsSucceeded = false;
			string query = string.Empty;

			using(var connection = new SqlConnection())
			{
				connection.ConnectionString = DB.GetDBConn();
				connection.Open();

				using(var command = connection.CreateCommand())
				{
					SqlTransaction transaction;

					transaction = connection.BeginTransaction();

					command.Connection = connection;
					command.Transaction = transaction;

					try
					{
						if(!FullTextSearchIsEnabled())
						{
							DB.ExecuteSQL(FTSHelper.queryEnableFTS);
							enableFtsSucceeded = true;
						}

						string newCatalogName = txtNewCatalogName.Text.Trim();
						string newCatalogPath = txtNewCatalogPath.Text.Trim();

						if(!String.IsNullOrWhiteSpace(newCatalogName) && CommonLogic.HasInvalidChar(newCatalogName))
						{
							//HasInvalid returns true on empty string
							Exception invalid = new Exception("setupFTS.InvalidCharacterOnNewCatalogName".StringResource());
							throw invalid;
						}


						string language = ddlLanguage.SelectedValue.Trim();

						if(!LanguageIsSupported(language))
						{
							Exception invalid = new Exception("setupFTS.CurrentCollationSettingDoesNotSupport".StringResource() + " " + language);
							throw invalid;
						}


						if(radioCreate.Checked.Equals(true) && radioReuse.Checked.Equals(false))
						{
							if(!String.IsNullOrWhiteSpace(newCatalogName)
							&& !String.IsNullOrWhiteSpace(newCatalogPath))
							{
								query = query + FTSHelper.queryCreateFullTextCatalog + newCatalogName.Replace(" ", "_") + " IN PATH " + newCatalogPath;
								DB.ExecuteSQL(query);
								query = string.Empty;
								query = query + FTSHelper.queryCreateFullTextIndexOnProduct + newCatalogName + FTSHelper.queryWithChangeTrackingAuto;
								DB.ExecuteSQL(query);
								query = string.Empty;
								query = query + FTSHelper.queryCreateTableNoiseWords;
								command.CommandText = query;
								command.ExecuteNonQuery();
								query = string.Empty;
							}
							else if(!String.IsNullOrWhiteSpace(newCatalogName)
								 && String.IsNullOrWhiteSpace(newCatalogPath))
							{
								query = query + FTSHelper.queryCreateFullTextCatalog + newCatalogName.Replace(" ", "_");
								DB.ExecuteSQL(query);
								query = string.Empty;
								query = query + FTSHelper.queryCreateFullTextIndexOnProduct + newCatalogName.Replace(" ", "_") + FTSHelper.queryWithChangeTrackingAuto;
								DB.ExecuteSQL(query);
								query = string.Empty;
								query = query + FTSHelper.queryCreateTableNoiseWords;
								command.CommandText = query;
								command.ExecuteNonQuery();
								query = string.Empty;
							}
							else if(String.IsNullOrWhiteSpace(newCatalogName)
								&& !String.IsNullOrWhiteSpace(newCatalogPath))
							{
								throw new Exception(JSwarnDefineName);
							}
							else
							{
								throw new Exception(JSwarnDefineNamePath);
							}
						}
						else if(radioCreate.Checked.Equals(false) && radioReuse.Checked.Equals(true))
						{
							if(String.IsNullOrWhiteSpace(newCatalogName) && String.IsNullOrWhiteSpace(newCatalogPath))
							{
								if(lstCatalogNames.SelectedValue.Trim().Equals(string.Empty) == false)
								{
									query = query + FTSHelper.queryAlterFullTextCatalog + lstCatalogNames.SelectedValue.Trim().Replace(" ", "_") + " REORGANIZE";
									DB.ExecuteSQL(query);
									query = string.Empty;
									query = query + FTSHelper.queryCreateFullTextIndexOnProduct + lstCatalogNames.SelectedValue.Trim().Replace(" ", "_") + FTSHelper.queryWithChangeTrackingAuto;
									DB.ExecuteSQL(query);
									query = string.Empty;
									query = query + FTSHelper.queryCreateTableNoiseWords;
									command.CommandText = query;
									command.ExecuteNonQuery();
									query = string.Empty;
								}
								else
								{
									throw new Exception(JSwarnSelectCatalog);
								}
							}
							else
							{
								throw new Exception(JSwarnDefineSingleCatalog);
							}
						}

						InsertLanguageSpecificNoiseWords(query, command, language);
						query = string.Empty;

						query = query + FTSHelper.queryCreateFunctionGetValidSearchString;
						command.CommandText = query;
						command.ExecuteNonQuery();
						query = string.Empty;

						query = query + FTSHelper.queryCreateFunctionKeyWordSearch;
						command.CommandText = query;
						command.ExecuteNonQuery();
						query = string.Empty;

						transaction.Commit();

						Response.Redirect(AppLogic.AdminLinkUrl("setupfts.aspx"));
					}
					catch(System.Threading.ThreadAbortException)
					{
						throw;
					}
					catch(Exception ex)
					{
						btnInstallFTS.Enabled = false;
						ctlAlertMessage1.PushAlertMessage(ex.Message, AlertMessage.AlertType.Error);

						try
						{
							transaction.Rollback();

							if(enableFtsSucceeded)
							{
								DB.ExecuteSQL(FTSHelper.queryDisableFTSUninstall);
							}

							if(FullTextProductIndexExists())
							{
								DB.ExecuteSQL(FTSHelper.sqlDropFTIndex);
							}
						}
						catch(Exception ex2)
						{
							ctlAlertMessage2.PushAlertMessage(ex2.Message, AlertMessage.AlertType.Error);
						}
					}
				}
			}
		}

		private string InsertLanguageSpecificNoiseWords(string query, SqlCommand command, string language)
		{
			if(language.Equals("Chinese-Simplified"))
			{
				query = query + FTSHelper.Chinese_Simplified();
				command.CommandText = query;
				command.ExecuteNonQuery();
				query = string.Empty;
			}
			else if(language.Equals("Chinese-Traditional"))
			{
				query = query + FTSHelper.Chinese_Traditional();
				command.CommandText = query;
				command.ExecuteNonQuery();
				query = string.Empty;
			}
			else if(language.Equals("Danish"))
			{
				query = query + FTSHelper.Danish();
				command.CommandText = query;
				command.ExecuteNonQuery();
				query = string.Empty;
			}
			else if(language.Equals("Dutch"))
			{
				query = query + FTSHelper.Dutch();
				command.CommandText = query;
				command.ExecuteNonQuery();
				query = string.Empty;
			}
			else if(language.Equals("English-International"))
			{
				query = query + FTSHelper.English_International();
				command.CommandText = query;
				command.ExecuteNonQuery();
				query = string.Empty;
			}
			else if(language.Equals("English-US"))
			{
				query = query + FTSHelper.English_US();
				command.CommandText = query;
				command.ExecuteNonQuery();
				query = string.Empty;
			}
			else if(language.Equals("French"))
			{
				query = query + FTSHelper.French();
				command.CommandText = query;
				command.ExecuteNonQuery();
				query = string.Empty;
			}
			else if(language.Equals("German"))
			{
				query = query + FTSHelper.German();
				command.CommandText = query;
				command.ExecuteNonQuery();
				query = string.Empty;
			}
			else if(language.Equals("Italian"))
			{
				query = query + FTSHelper.Italian();
				command.CommandText = query;
				command.ExecuteNonQuery();
				query = string.Empty;
			}
			else if(language.Equals("Japanese"))
			{
				query = query + FTSHelper.Japanese();
				command.CommandText = query;
				command.ExecuteNonQuery();
				query = string.Empty;
			}
			else if(language.Equals("Korean"))
			{
				query = query + FTSHelper.Korean();
				command.CommandText = query;
				command.ExecuteNonQuery();
				query = string.Empty;
			}
			else if(language.Equals("Neutral"))
			{
				query = query + FTSHelper.Neutral();
				command.CommandText = query;
				command.ExecuteNonQuery();
				query = string.Empty;
			}
			else if(language.Equals("Polish"))
			{
				query = query + FTSHelper.Polish();
				command.CommandText = query;
				command.ExecuteNonQuery();
				query = string.Empty;
			}
			else if(language.Equals("Portuguese"))
			{
				query = query + FTSHelper.Portugese();
				command.CommandText = query;
				command.ExecuteNonQuery();
				query = string.Empty;
			}
			else if(language.Equals("Portuguese(Brazil)"))
			{
				query = query + FTSHelper.Portugese_Brazil();
				command.CommandText = query;
				command.ExecuteNonQuery();
				query = string.Empty;
			}
			else if(language.Equals("Russian"))
			{
				query = query + FTSHelper.Russian();
				command.CommandText = query;
				command.ExecuteNonQuery();
				query = string.Empty;
			}
			else if(language.Equals("Spanish"))
			{
				query = query + FTSHelper.Spanish();
				command.CommandText = query;
				command.ExecuteNonQuery();
				query = string.Empty;
			}
			else if(language.Equals("Swedish"))
			{
				query = query + FTSHelper.Swedish();
				command.CommandText = query;
				command.ExecuteNonQuery();
				query = string.Empty;
			}
			else if(language.Equals("Thai"))
			{
				query = query + FTSHelper.Thailand();
				command.CommandText = query;
				command.ExecuteNonQuery();
				query = string.Empty;
			}
			else if(language.Equals("Turkish"))
			{
				query = query + FTSHelper.Turkish();
				command.CommandText = query;
				command.ExecuteNonQuery();
				query = string.Empty;
			}
			return query;
		}

		private static bool LanguageIsSupported(string language)
		{
			using(SqlConnection con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				string collationquery = "SELECT databasepropertyex(db_name(),'collation') AS collation_name";
				using(IDataReader rscoll = DB.GetRS(collationquery, con))
				{
					if(rscoll.Read())
					{
						if(DB.RSField(rscoll, "collation_name") == "SQL_Latin1_General_CP1_CI_AS")
						{
							if(language.Equals("Chinese-Simplified") || language.Equals("Chinese-Traditional") | language.Equals("Russian") || language.Equals("Japanese"))
							{
								return false;
							}
						}
					}
				}
			}
			return true;
		}

		private bool FullTextSearchIsEnabled()
		{
			using(SqlConnection conn = DB.dbConn())
			{
				conn.Open();
				using(IDataReader rs = DB.GetRS(FTSHelper.sqlCheckFTSEnabled, conn))
				{
					if(rs.Read())
					{
						if(rs.GetInt32(0) < 1)
							return false;
						else
							return true;
					}
				}
			}
			return false;
		}

		private void HideInstallationControls()
		{
			btnInstallFTS.Enabled = false;
			lblCatalogList.Visible = false;
			lstCatalogNames.Visible = false;
			lblNewCatalogName.Visible = false;
			txtNewCatalogName.Visible = false;
			lblNewCatalogPath.Visible = false;
			txtNewCatalogPath.Visible = false;
			lblRadioCreate.Visible = false;
			radioCreate.Visible = false;
			lblRadioReuse.Visible = false;
			radioReuse.Visible = false;
			btnOptimize.Visible = false;
			btnUninstallFTS.Visible = false;
		}

		private void ShowControlsForWhenFtsIsInstalled()
		{
			hyperNoiseWord.Visible = true;
			btnInstallFTS.Visible = false;
			lblLanguage.Visible = false;
			ddlLanguage.Visible = false;
			lblRadioCreate.Visible = false;
			radioCreate.Visible = false;
			lblRadioReuse.Visible = false;
			radioReuse.Visible = false;
			lblCatalogList.Visible = false;
			lstCatalogNames.Visible = false;
			lblNewCatalogName.Visible = false;
			txtNewCatalogName.Visible = false;
			lblNewCatalogPath.Visible = false;
			txtNewCatalogPath.Visible = false;
		}

		private bool FullTextProductIndexExists()
		{
			using(SqlConnection conn = DB.dbConn())
			{
				conn.Open();
				using(IDataReader rs = DB.GetRS(FTSHelper.sqlCheckFTIndex, conn))
				{
					if(rs.Read())
					{
						if(rs.GetInt32(0) > 0)
						{
							//FTS is installed
							return true;
						}
					}
				}
			}
			return false;
		}

		private bool FullTextEngineIsSupported()
		{
			using(SqlConnection conn = DB.dbConn())
			{
				conn.Open();
				using(IDataReader rs = DB.GetRS(FTSHelper.sqlCheckFTSEngine, conn))
				{
					if(rs.Read())
					{
						if(rs.GetInt32(0) == 1)
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		private void loadLocaleStrings()
		{
			lblIntro.Text = AppLogic.GetString("setupFTS.aspx.2", SkinID, LocaleSetting);
			lblLanguage.Text = AppLogic.GetString("setupFTS.aspx.5", SkinID, LocaleSetting);
			lblNewCatalogName.Text = AppLogic.GetString("setupFTS.aspx.8", SkinID, LocaleSetting);
			lblNewCatalogPath.Text = AppLogic.GetString("setupFTS.aspx.9", SkinID, LocaleSetting);
			lblCatalogList.Text = AppLogic.GetString("setupFTS.aspx.10", SkinID, LocaleSetting);
			btnInstallFTS.Text = AppLogic.GetString("setupFTS.aspx.11", SkinID, LocaleSetting);
			btnUninstallFTS.Text = AppLogic.GetString("setupFTS.aspx.21", SkinID, LocaleSetting);
			btnOptimize.Text = AppLogic.GetString("setupFTS.aspx.22", SkinID, LocaleSetting);
			hyperNoiseWord.Text = AppLogic.GetString("setupFTS.aspx.28", SkinID, LocaleSetting);

			JSwarnProceed = AppLogic.GetString("setupFTS.aspx.12", SkinID, LocaleSetting) + " ";
			JSwarnInLocation = " " + AppLogic.GetString("setupFTS.aspx.13", SkinID, LocaleSetting) + " ";
			JSwarnInDefaultLocation = " " + AppLogic.GetString("setupFTS.aspx.14", SkinID, LocaleSetting);
			JSwarnDefineName = AppLogic.GetString("setupFTS.aspx.15", SkinID, LocaleSetting);
			JSwarnDefineNamePath = AppLogic.GetString("setupFTS.aspx.16", SkinID, LocaleSetting);
			JSwarnSelectCatalog = AppLogic.GetString("setupFTS.aspx.17", SkinID, LocaleSetting);
			JSwarnReuseCatalog = AppLogic.GetString("setupFTS.aspx.18", SkinID, LocaleSetting) + " ";
			JSwarnDefineSingleCatalog = AppLogic.GetString("setupFTS.aspx.19", SkinID, LocaleSetting);
			JSwarnUninstallFTS = AppLogic.GetString("setupFTS.aspx.23", SkinID, LocaleSetting);
			JSwarnOptimizeFTS = AppLogic.GetString("setupFTS.aspx.24", SkinID, LocaleSetting);
			JSwarnSelectRadioButton = AppLogic.GetString("admin.setup.selectRadioButton", SkinID, LocaleSetting);
		}

		private void DisplayCatalogsIfTheyExist()
		{
			try
			{
				if(FtsCatalogsExist())
				{
					AddFtsCatalogsToList();
					ShowFtsCatalogListControls();
				}
			}
			catch(Exception ex)
			{
				throw ex;
			}
		}

		private bool FtsCatalogsExist()
		{
			int numberOfFTSCatalogs = 0;
			using(SqlConnection conn = new SqlConnection(DB.GetDBConn()))
			{
				conn.Open();
				using(IDataReader rs = DB.GetRS(FTSHelper.sqlSearchFTCatalogs, conn))
				{
					if(rs.Read())
					{
						numberOfFTSCatalogs = rs.GetInt32(0);
						if(numberOfFTSCatalogs > 0)
							return true;
					}
				}
			}
			return false;
		}

		private void ShowControlsForWhenNoFtsCatalogsExist()
		{
			lblCatalogList.Visible = false;
			lstCatalogNames.Visible = false;
			radioReuse.Enabled = false;
			txtNewCatalogName.Enabled = true;
			txtNewCatalogPath.Enabled = true;
			radioCreate.Checked = true;
		}

		private void ShowFtsCatalogListControls()
		{
			lblCatalogList.Visible = true;
			lstCatalogNames.Visible = true;
			radioReuse.Enabled = true;
		}

		private bool FtsCatalogsAvailable()
		{
			using(SqlConnection conn2 = new SqlConnection(DB.GetDBConn()))
			{
				conn2.Open();
				using(IDataReader rs = DB.GetRS(FTSHelper.sqlAvailFTCatalogs, conn2))
				{
					if(rs.Read())
					{
						return true;
					}
				}
			}
			return false;
		}

		private void AddFtsCatalogsToList()
		{
			using(SqlConnection conn2 = new SqlConnection(DB.GetDBConn()))
			{
				conn2.Open();
				using(IDataReader rs = DB.GetRS(FTSHelper.sqlAvailFTCatalogs, conn2))
				{
					while(rs.Read())
					{
						lstCatalogNames.Items.Add(DB.RSField(rs, "Name").Replace("_", " "));
					}
				}
			}
		}

		private string GetCatalogName()
		{
			string catalogName = String.Empty;
			using(SqlConnection conn = DB.dbConn())
			{
				conn.Open();
				using(IDataReader rs = DB.GetRS(FTSHelper.sqlGetFTCatalogName, conn))
				{
					if(rs.Read())
					{
						catalogName = rs.GetString(0);
					}
				}
			}
			return catalogName;
		}

		private void UninstallFTS()
		{
			string query = string.Empty;
			string catalogName = GetCatalogName();

			using(var connection = new SqlConnection())
			{
				connection.ConnectionString = DB.GetDBConn();
				connection.Open();

				using(var command = connection.CreateCommand())
				{
					SqlTransaction transaction;

					transaction = connection.BeginTransaction();

					command.Connection = connection;
					command.Transaction = transaction;

					try
					{
						DB.ExecuteSQL(FTSHelper.queryDisableFTSUninstall);

						DB.ExecuteSQL(FTSHelper.sqlDropFTIndex);

						query = query + FTSHelper.queryDropNoiseWords;
						command.CommandText = query;
						command.ExecuteNonQuery();
						query = string.Empty;

						query = query + FTSHelper.queryDropFunctionGetValidSearchString;
						command.CommandText = query;
						command.ExecuteNonQuery();
						query = string.Empty;

						query = query + FTSHelper.queryDropFunctionKeyWordSearch;
						command.CommandText = query;
						command.ExecuteNonQuery();
						query = string.Empty;

						transaction.Commit();

						Response.Redirect(AppLogic.AdminLinkUrl("setupfts.aspx"));
					}
					catch(System.Threading.ThreadAbortException)
					{
						throw;
					}
					catch(Exception ex)
					{
						string error = ex.Message;
						if(string.IsNullOrEmpty(error))
							ctlAlertMessage1.PushAlertMessage(error, AlertMessage.AlertType.Error);

						try
						{
							transaction.Rollback();

							DB.ExecuteSQL(FTSHelper.queryEnableFTS);

							if(!FullTextProductIndexExists())
							{
								query = string.Empty;
								query = query + FTSHelper.queryCreateFullTextIndexOnProduct + catalogName + FTSHelper.queryWithChangeTrackingAuto;
								DB.ExecuteSQL(query);
								query = string.Empty;
							}
						}
						catch(Exception ex2)
						{
							ctlAlertMessage2.PushAlertMessage(ex2.Message, AlertMessage.AlertType.Error);
						}
					}
				}
			}
		}

		protected void btnUninstallFTS_Click(object sender, EventArgs e)
		{
			if(FullTextProductIndexExists())
			{
				UninstallFTS();
			}
		}

		protected void btnOptimize_Click(object sender, EventArgs e)
		{
			string query = string.Empty;
			string catalogName = string.Empty;

			using(SqlConnection conn = DB.dbConn())
			{
				conn.Open();
				using(IDataReader reader7 = DB.GetRS(FTSHelper.sqlGetFTCatalogName, conn))
				{
					if(reader7.Read())
					{
						catalogName = reader7.GetString(0);
					}
				}
			}

			query = query + FTSHelper.queryAlterFullTextCatalog + catalogName.Trim() + " REORGANIZE";
			try
			{
				DB.ExecuteSQL(query);
			}
			catch(Exception ex)
			{
				ctlAlertMessage2.PushAlertMessage(ex.Message, AlertMessage.AlertType.Error);
				return;
			}
			query = string.Empty;
			ctlAlertMessage2.PushAlertMessage("Optimize Completed", AlertMessage.AlertType.Success);
		}
	}
}
