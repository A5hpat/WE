// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI.WebControls;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontCore.Validation;

namespace AspDotNetStorefrontAdmin
{

	public partial class affiliate : AspDotNetStorefront.Admin.AdminPageBase
	{
		protected int affiliateID;

		protected void Page_Load(object sender, EventArgs e)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");

			affiliateID = CommonLogic.QueryStringNativeInt("affiliateId");

			//Check querystring, if zero check db, if DB is also zero then no parent
			var parentAffiliateId = CommonLogic.QueryStringNativeInt("parentAffiliateId");
			if(parentAffiliateId == 0)
				parentAffiliateId = GetAffiliatesParentId(affiliateID);

			ResetPasswordError.Visible = false;
			ResetPasswordOk.Visible = false;

			if (!IsPostBack)
			{
				ResetPasswordLink.Attributes.Add("onClick", "javascript: return confirm('Are you sure you want to reset the password for this affiliate?');");
				ResetPasswordError.Text = AppLogic.GetString("cst_account_process.aspx.2", 1, Localization.GetDefaultLocale());
				ResetPasswordOk.Text = AppLogic.GetString("cst_account_process.aspx.3", 1, Localization.GetDefaultLocale());
				ddParent.Items.Clear();
				ddParent.Items.Add(new ListItem("--NONE--", "0"));
				string affRaw = AppLogic.RunXmlPackage("AffiliateArrayList.xml.config", null, null, 1, "", "", false, false);
				affRaw = affRaw.Replace("&gt;", " -> ").Replace("\n", "").Replace("\r", "");
				string[] affList = affRaw.Split('~');
				foreach (string aff in affList)
				{
					string[] affItem = aff.Split('|');
					if (affItem.Length > 1)
						ddParent.Items.Add(new ListItem(HttpUtility.HtmlDecode(affItem[1].ToString()), affItem[0].ToString().Replace(" ", "")));
				}
				if (parentAffiliateId > 0 && ddParent.Items.FindByValue(parentAffiliateId.ToString()) != null)
					ddParent.SelectedValue = parentAffiliateId.ToString();

				ddState.Items.Clear();
				ddState.Items.Add(new ListItem("SELECT ONE", "0"));

				using (var dbconn = DB.dbConn())
				{
					dbconn.Open();
					using (var rs = DB.GetRS("select * from state   with (NOLOCK)  order by DisplayOrder,Name", dbconn))
					{
						while (rs.Read())
						{
                            ddState.Items.Add(new ListItem(HttpUtility.HtmlEncode(DB.RSField(rs, "Name")), HttpUtility.HtmlEncode(DB.RSField(rs, "Abbreviation"))));
						}
					}
				}

				ddCountry.Items.Clear();
				ddCountry.Items.Add(new ListItem("SELECT ONE", "0"));

				using (var dbconn = DB.dbConn())
				{
					dbconn.Open();
					using (var rs = DB.GetRS("select * from country   with (NOLOCK)  order by DisplayOrder,Name", dbconn))
					{
						while (rs.Read())
						{
							ddCountry.Items.Add(new ListItem(DB.RSField(rs, "Name"), DB.RSField(rs, "Name")));
						}
					}
				}


				ViewState["EditingAffiliate"] = false;
				ViewState["EditingAffiliateID"] = "0";

				reqValPassword.ErrorMessage = AppLogic.GetString("createaccount.aspx.20", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);

				if (affiliateID > 0)
				{
					ResetPasswordRow.Visible = true;
					CreatePasswordRow.Visible = false;
					ViewState["EditingAffiliate"] = true;
					ViewState["EditingAffiliateID"] = affiliateID;

					etsMapper.ObjectID = Localization.ParseNativeInt(ViewState["EditingAffiliateID"].ToString());
					etsMapper.DataBind();
					litStoreMapper.Visible = etsMapper.StoreCount > 1;
					litStoreMapperHdr.Visible = etsMapper.StoreCount > 1;

					getAffiliateDetails();

					Title = HeaderText.Text = "admin.Affiliate.Edit".StringResource();
				}
				else
				{
					ResetPasswordRow.Visible = false;
					CreatePasswordRow.Visible = true;
					Title = HeaderText.Text = "admin.Affiliate.Create".StringResource();

				}
			}
		}

		protected override void OnPreRender(EventArgs e)
		{
			DataBind();
			base.OnPreRender(e);
		}

		protected void getAffiliateDetails()
		{

			using (var dbconn = DB.dbConn())
			{
				dbconn.Open();
				using (var rs = DB.GetRS("select * from Affiliate   with (NOLOCK)  where AffiliateID=" + ViewState["EditingAffiliateID"].ToString(), dbconn))
				{
					if (!rs.Read())
					{
						ctrlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.common.UnableToRetrieveData", LocaleSetting), AlertMessage.AlertType.Error);
						return;
					}

					//editing affiliate
                    txtNickName.Text = DB.RSField(rs, "Name");
					txtFirstName.Text = DB.RSField(rs, "FirstName");
                    txtLastName.Text = DB.RSField(rs, "LastName");
                    
					ddCountry.ClearSelection();
					var parentAffiliateId = DB.RSField(rs, "ParentAffiliateID");
					if (ddParent.Items.FindByValue(parentAffiliateId.ToString()) != null)
						ddParent.SelectedValue = parentAffiliateId;

                    txtEmail.Text = DB.RSField(rs, "EMail");
                    txtCompany.Text = DB.RSField(rs, "Company");
                    txtAddress1.Text = DB.RSField(rs, "Address1");
                    txtAddress2.Text = DB.RSField(rs, "Address2");
                    txtSuite.Text = DB.RSField(rs, "Suite");
                    txtCity.Text = DB.RSField(rs, "City");

					ddState.ClearSelection();
					try
					{
						foreach (ListItem i in ddState.Items)
						{
							if (i.Value.Equals(DB.RSField(rs, "State")))
							{
								ddState.ClearSelection();
								ddState.SelectedValue = DB.RSField(rs, "State");
								break;
							}
						}
					}
					catch { }

					txtZip.Text = DB.RSField(rs, "Zip");

					ddCountry.ClearSelection();
					try
					{
						foreach (ListItem i in ddCountry.Items)
						{
							if (i.Value.Equals(DB.RSField(rs, "Country")))
							{
								ddCountry.ClearSelection();
								ddCountry.SelectedValue = DB.RSField(rs, "Country");
								break;
							}
						}
					}
					catch { }

                    txtPhone.Text = DB.RSField(rs, "Phone");

					//WEBSITE INFORMATION
                    txtWebName.Text = DB.RSField(rs, "WebSiteName");
                    txtWebDescription.Text = DB.RSField(rs, "WebSiteDescription");
                    txtWebURL.Text = DB.RSField(rs, "URL");
				}
			}
		}

		protected void btnSubmit_Click(object sender, EventArgs e)
		{
			if (Page.IsValid)
			{
                if(UpdateInformation())
                {
				    etsMapper.ObjectID = Localization.ParseNativeInt(ViewState["EditingAffiliateID"].ToString());
				    etsMapper.Save();
                }
			}
		}

		protected void btnSaveAndClose_Click(object sender, EventArgs e)
		{
			if(Page.IsValid)
			{
				if(UpdateInformation())
                {
                    etsMapper.ObjectID = Localization.ParseNativeInt(ViewState["EditingAffiliateID"].ToString());
				    etsMapper.Save();
                    Response.Redirect(ReturnUrlTracker.GetReturnUrl());
                }				
			}			
		}

		bool UpdateInformation()
		{
			bool Editing = Localization.ParseBoolean(ViewState["EditingAffiliate"].ToString());
			int AffiliateID = Localization.ParseNativeInt(ViewState["EditingAffiliateID"].ToString());

			StringBuilder sql = new StringBuilder();
			String Name = txtNickName.Text;
			if (Name.Length == 0)
			{
				if (txtFirstName.Text.Length != 0)
				{
					Name = (txtFirstName.Text + " " + txtLastName.Text).Trim();
				}
				else
				{
					Name = txtLastName.Text;
				}
			}
			int ParID = Localization.ParseNativeInt(ddParent.SelectedValue);
			if (ParID == AffiliateID)  // prevent case which causes endless recursion
			{
				ParID = 0;
			}

			if (txtEmail.Text.Trim().Length > 0 && !(new EmailAddressValidator()).IsValidEmailAddress(txtEmail.Text.Trim()))
			{
				ctrlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.editAffiliates.InvalidEmailFormat", LocaleSetting), AlertMessage.AlertType.Error);
				return false;
			}

			if (txtEmail.Text.Trim().Length > 0 && Affiliate.EmailInUse(txtEmail.Text.Trim(), AffiliateID))
			{
				ctrlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.editAffiliates.TakenEmailAddress", LocaleSetting), AlertMessage.AlertType.Error);
				return false;
			}
        
			if (!Editing)
			{

				// ok to add them:
				String NewGUID = DB.GetNewGUID();
				sql.Append("insert Affiliate(AffiliateGUID,EMail,Password,SaltKey,IsOnline,FirstName,LastName,[Name],ParentAffiliateID,[Company],Address1,Address2,Suite,City,State,Zip,Country,Phone,WebSiteName,WebSiteDescription,URL) values(");
				sql.Append(CommonLogic.SQuote(NewGUID) + ",");
				sql.Append(CommonLogic.SQuote(CommonLogic.Left(txtEmail.Text.Trim(), 100)) + ",");

				Password p = new Password(CommonLogic.IsNull(ViewState["affpwd"], "").ToString());
				sql.Append(CommonLogic.SQuote(p.SaltedPassword) + ",");
				sql.Append(p.Salt.ToString() + ",");

				if (txtWebURL.Text.Length != 0)
				{
					sql.Append("1,");
				}
				else
				{
					sql.Append("0,");
				}
				sql.Append(CommonLogic.SQuote(CommonLogic.Left(txtFirstName.Text, 100)) + ",");
				sql.Append(CommonLogic.SQuote(CommonLogic.Left(txtLastName.Text, 100)) + ",");
				sql.Append(CommonLogic.SQuote(CommonLogic.Left(Name, 100)) + ",");
				sql.Append(ParID.ToString() + ",");
				sql.Append(CommonLogic.SQuote(CommonLogic.Left(txtCompany.Text, 100)) + ",");
				if (txtAddress1.Text.Length != 0)
				{
					sql.Append(CommonLogic.SQuote(txtAddress1.Text.Replace("\x0D\x0A", "")) + ",");
				}
				else
				{
					sql.Append("NULL,");
				}
				if (txtAddress2.Text.Length != 0)
				{
					sql.Append(CommonLogic.SQuote(txtAddress2.Text.Replace("\x0D\x0A", "")) + ",");
				}
				else
				{
					sql.Append("NULL,");
				}
				if (txtSuite.Text.Length != 0)
				{
					sql.Append(CommonLogic.SQuote(txtSuite.Text) + ",");
				}
				else
				{
					sql.Append("NULL,");
				}
				if (txtCity.Text.Length != 0)
				{
					sql.Append(CommonLogic.SQuote(txtCity.Text) + ",");
				}
				else
				{
					sql.Append("NULL,");
				}
				if (ddState.SelectedValue != "0")
				{
					sql.Append(CommonLogic.SQuote(ddState.SelectedValue) + ",");
				}
				else
				{
					sql.Append("NULL,");
				}
				if (txtZip.Text.Length != 0)
				{
					sql.Append(CommonLogic.SQuote(txtZip.Text) + ",");
				}
				else
				{
					sql.Append("NULL,");
				}
				if (ddCountry.SelectedValue != "0")
				{
					sql.Append(CommonLogic.SQuote(ddCountry.SelectedValue) + ",");
				}
				else
				{
					sql.Append("NULL,");
				}
				if (txtPhone.Text.Length != 0)
				{
					sql.Append(CommonLogic.SQuote(txtPhone.Text) + ",");
				}
				else
				{
					sql.Append("NULL,");
				}
				if (txtWebName.Text.Length != 0)
				{
					sql.Append(CommonLogic.SQuote(txtWebName.Text) + ",");
				}
				else
				{
					sql.Append("NULL,");
				}
				if (txtWebDescription.Text.Length != 0)
				{
					sql.Append(CommonLogic.SQuote(txtWebDescription.Text) + ",");
				}
				else
				{
					sql.Append("NULL,");
				}
				if (txtWebURL.Text.Length != 0)
				{
					String theUrl = CommonLogic.Left(txtWebURL.Text, 80);
					if (theUrl.IndexOf("http://") == -1 && theUrl.IndexOf("https://") == -1 && theUrl.Length != 0)
					{
						theUrl = "http://" + theUrl;
					}
					if (theUrl.Length == 0)
					{
						sql.Append("NULL");
					}
					else
					{
						sql.Append(CommonLogic.SQuote(theUrl) + " ");
					}
				}
				else
				{
					sql.Append("NULL");
				}

				sql.Append(")");

				DB.ExecuteSQL(sql.ToString());

				ctrlAlertMessage.PushAlertMessage("Affiliate added.", AlertMessage.AlertType.Success);

				ResetPasswordRow.Visible = true;
				CreatePasswordRow.Visible = false;

				using (var dbconn = DB.dbConn())
				{
					dbconn.Open();
					using (var rs = DB.GetRS("select AffiliateID from Affiliate where deleted=0 and AffiliateGUID=" + CommonLogic.SQuote(NewGUID), dbconn))
					{
						rs.Read();
						AffiliateID = DB.RSFieldInt(rs, "AffiliateID");
						ViewState["EditingAffiliate"] = true;
						ViewState["EditingAffiliateID"] = AffiliateID.ToString();
					}
				}

				getAffiliateDetails();
			}
			else
			{
				// ok to update:
				sql.Append("update Affiliate set ");
				sql.Append("EMail=" + CommonLogic.SQuote(CommonLogic.Left(txtEmail.Text, 100)) + ",");
				sql.Append("IsOnline=" + CommonLogic.IIF(txtWebURL.Text.Length == 0, "0", "1") + ",");
				sql.Append("FirstName=" + CommonLogic.SQuote(CommonLogic.Left(txtFirstName.Text, 100)) + ",");
				sql.Append("LastName=" + CommonLogic.SQuote(CommonLogic.Left(txtLastName.Text, 100)) + ",");
				sql.Append("Name=" + CommonLogic.SQuote(CommonLogic.Left(Name, 100)) + ",");
				sql.Append("ParentAffiliateID=" + ParID + ",");
				if (txtCompany.Text.Length != 0)
				{
					sql.Append("Company=" + CommonLogic.SQuote(txtCompany.Text) + ",");
				}
				else
				{
					sql.Append("Company=NULL,");
				}
				if (txtAddress1.Text.Length != 0)
				{
					sql.Append("Address1=" + CommonLogic.SQuote(txtAddress1.Text.Replace("\x0D\x0A", "")) + ",");
				}
				else
				{
					sql.Append("Address1=NULL,");
				}
				if (txtAddress2.Text.Length != 0)
				{
					sql.Append("Address2=" + CommonLogic.SQuote(txtAddress2.Text.Replace("\x0D\x0A", "")) + ",");
				}
				else
				{
					sql.Append("Address2=NULL,");
				}
				if (txtSuite.Text.Length != 0)
				{
					sql.Append("Suite=" + CommonLogic.SQuote(txtSuite.Text) + ",");
				}
				else
				{
					sql.Append("Suite=NULL,");
				}
				if (txtCity.Text.Length != 0)
				{
					sql.Append("City=" + CommonLogic.SQuote(txtCity.Text) + ",");
				}
				else
				{
					sql.Append("City=NULL,");
				}
				if (ddState.SelectedValue != "0")
				{
					sql.Append("State=" + CommonLogic.SQuote(ddState.SelectedValue) + ",");
				}
				else
				{
					sql.Append("State=NULL,");
				}
				if (txtZip.Text.Length != 0)
				{
					sql.Append("Zip=" + CommonLogic.SQuote(txtZip.Text) + ",");
				}
				else
				{
					sql.Append("Zip=NULL,");
				}
				if (ddCountry.SelectedValue != "0")
				{
					sql.Append("Country=" + CommonLogic.SQuote(ddCountry.SelectedValue) + ",");
				}
				else
				{
					sql.Append("Country=NULL,");
				}
				if (txtPhone.Text.Length != 0)
				{
					sql.Append("Phone=" + CommonLogic.SQuote(AppLogic.MakeProperPhoneFormat(txtPhone.Text)) + ",");
				}
				else
				{
					sql.Append("Phone=NULL,");
				}
				if (txtWebName.Text.Length != 0)
				{
					sql.Append("WebSiteName=" + CommonLogic.SQuote(txtWebName.Text) + ",");
				}
				else
				{
					sql.Append("WebSiteName=NULL,");
				}
				if (txtWebDescription.Text.Length != 0)
				{
					sql.Append("WebSiteDescription=" + CommonLogic.SQuote(txtWebDescription.Text) + ",");
				}
				else
				{
					sql.Append("WebSiteDescription=NULL,");
				}
				if (txtWebURL.Text.Length != 0)
				{
					String theUrl2 = CommonLogic.Left(txtWebURL.Text, 80);
					if(theUrl2.IndexOf("http://") == -1 && theUrl2.IndexOf("https://") == -1 && theUrl2.Length != 0)
					{
						theUrl2 = "http://" + theUrl2;
					}
					if (theUrl2.Length != 0)
					{
						sql.Append("URL=" + CommonLogic.SQuote(theUrl2) + " ");
					}
					else
					{
						sql.Append("URL=NULL");
					}
				}
				else
				{
					sql.Append("URL=NULL");
				}

				sql.Append(" where AffiliateID=" + AffiliateID.ToString());
				DB.ExecuteSQL(sql.ToString());

				ctrlAlertMessage.PushAlertMessage("Affiliate updated.", AlertMessage.AlertType.Success);

				getAffiliateDetails();
			}
            return true;
		}

		public void ValidatePassword(object source, ServerValidateEventArgs args)
		{
			SetPasswordFields();
			string pwd1 = ViewState["affpwd"].ToString();
			string pwd2 = ViewState["affpwd2"].ToString();

			if (pwd1.Length == 0)
			{
				args.IsValid = true;
			}
			else if (pwd1.Trim().Length == 0)
			{
				args.IsValid = false;
				valPassword.ErrorMessage = AppLogic.GetString("account.aspx.74", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
			}
			else if (pwd1 == pwd2)
			{
				try
				{
                    valPassword.ErrorMessage = AppLogic.GetString("account.aspx.7", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    // nal UseStrongPwd
     //               if (AppLogic.AppConfigBool("UseStrongPwd"))
					//{

					//	if (Regex.IsMatch(pwd1, AppLogic.AppConfig("StrongPasswordValidator"), RegexOptions.Compiled))
					//	{
					//		args.IsValid = true;
					//	}
					//	else
					//	{
					//		args.IsValid = false;
					//		valPassword.ErrorMessage = AppLogic.GetString("account.aspx.69", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
					//	}
					//}
					//else
					//{
						if(Regex.IsMatch(pwd1, AppLogic.AppConfig("PasswordValidator"), RegexOptions.Compiled))
						{
							args.IsValid = true;
						}
						else
						{
							args.IsValid = false;
							valPassword.ErrorMessage = AppLogic.GetString("signin.newpassword.normalRegexFailure", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
						}
                    //}
				}
				catch
				{
					AppLogic.SendMail(AppLogic.GetString("admin.editAffiliates.InvalidPasswordValidation", SkinID, LocaleSetting), "", false, AppLogic.AppConfig("MailMe_ToAddress"), AppLogic.AppConfig("MailMe_ToAddress"), AppLogic.AppConfig("MailMe_ToAddress"), AppLogic.AppConfig("MailMe_ToAddress"), "", "", AppLogic.MailServer());
					throw new Exception(AppLogic.GetString("admin.editAffiliates.InvalidPasswordExpression", SkinID, LocaleSetting));
				}
			}
			else
			{
				args.IsValid = false;
				valPassword.ErrorMessage = AppLogic.GetString("createaccount.aspx.80", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
			}

			if (!args.IsValid)
			{
				ViewState["affpwd"] = "";
				ViewState["affpwd2"] = "";
			}


		}

		private void SetPasswordFields()
		{
			if (ViewState["affpwd"] == null)
			{
				ViewState["affpwd"] = "";
			}
			if (AffPassword.Text.Trim() != "")
			{
				ViewState["affpwd"] = AffPassword.Text;
				reqValPassword.Enabled = false;
			}

			if (ViewState["affpwd2"] == null)
			{
				ViewState["affpwd2"] = "";
			}
			if (AffPassword2.Text != "")
			{
				ViewState["affpwd2"] = AffPassword2.Text;
			}
		}


		protected void ResetPasswordLink_Click(object sender, EventArgs e)
		{
			if (AppLogic.MailServer().Length == 0 || AppLogic.MailServer() == AppLogic.ro_TBD)
			{
				ResetPasswordError.Visible = true;
			}
			else
			{
				Password p = new Password(AspDotNetStorefrontEncrypt.Encrypt.CreateRandomStrongPassword(8));
				Affiliate a = new Affiliate(affiliateID);
				try
				{
					String Subject = AppLogic.AppConfig("StoreName") + " - " + AppLogic.AppConfig("AppConfig.AffiliateProgramName") + " - " + AppLogic.GetString("cst_account_process.aspx.1", 1, Localization.GetDefaultLocale());
					String Body = AppLogic.RunXmlPackage("notification.lostpassword.xml.config", null, null, 1, "", "thisaffiliateid=" + a.AffiliateID.ToString() + "&newpwd=" + p.ClearPassword, false, false);
					AppLogic.SendMail(Subject, Body, true, AppLogic.AppConfig("MailMe_FromAddress"), AppLogic.AppConfig("MailMe_FromName"), a.EMail, (a.FirstName + " " + a.LastName).Trim(), "", "", AppLogic.MailServer());
					ResetPasswordOk.Visible = true;
					a.Update(null, p.SaltedPassword, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, p.Salt);
				}
				catch
				{
					ResetPasswordOk.Visible = false;
					ResetPasswordError.Visible = true;
				}
			}

		}

		/// <summary>
		/// Gets affiliates parent id, return zero if one doesn't exist
		/// </summary>
		/// <param name="childAffiliateID"></param>
		/// <returns></returns>
		int GetAffiliatesParentId(int? childAffiliateID)
		{			
			if(childAffiliateID == null || childAffiliateID == 0)
				return 0;

			return DB.GetSqlN("SELECT ParentAffiliateId AS N FROM Affiliate WHERE AffiliateId = @AffiliateId", 
				new SqlParameter[] { new SqlParameter("AffiliateId", childAffiliateID) });
		}
	}
}
