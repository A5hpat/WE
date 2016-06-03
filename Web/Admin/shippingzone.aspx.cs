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
using System.Text.RegularExpressions;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
    /// <summary>
    /// Summary description for editshippingzone
    /// </summary>
    public partial class shippingzone : AspDotNetStorefront.Admin.AdminPageBase
    {
		private const int START_RANGE = 0;
		private const int END_RANGE = 1;
        int ShippingZoneID = 0;

		protected void Page_Load(object sender, System.EventArgs e)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");

			if(CommonLogic.QueryStringCanBeDangerousContent("ShippingZoneID").Length != 0 && CommonLogic.QueryStringCanBeDangerousContent("ShippingZoneID") != "0")
			{
				Editing = true;
				ShippingZoneID = Localization.ParseUSInt(CommonLogic.QueryStringCanBeDangerousContent("ShippingZoneID"));
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
			if(!editing)
			{
				litHeader.Text = AppLogic.GetString("admin.editshippingzone.AddNewShippingZone", ThisCustomer.LocaleSetting);
				divZoneId.Visible = false;
			}
			else
			{
				litHeader.Text = AppLogic.GetString("admin.editshippingzone.EditingShippingZone", ThisCustomer.LocaleSetting);

				string sql = "SELECT * FROM ShippingZone WITH (NOLOCK) WHERE Deleted = 0 AND ShippingZoneID = @ShippingZoneID";
				List<SqlParameter> sqlParams = new List<SqlParameter> { new SqlParameter("@ShippingZoneID", ShippingZoneID.ToString()) };

				using(SqlConnection dbconn = DB.dbConn())
				{
					dbconn.Open();
					using(IDataReader rs = DB.GetRS(sql, sqlParams.ToArray(), dbconn))
					{
						if(rs.Read())
						{
							litZoneId.Text = DB.RSFieldInt(rs, "ShippingZoneID").ToString();
							txtZoneName.Text = DB.RSField(rs, "Name");
							txtZipCodes.Text = DB.RSField(rs, "ZipCodes");
						}
					}
				}
			}
		}		

		protected void btnSubmit_Click(object sender, EventArgs e)
		{
			if(SaveZone())
				Response.Redirect(String.Format("shippingzone.aspx?ShippingZoneID={0}", ShippingZoneID));
			
		}

		protected void btnSaveAndClose_Click(object sender, EventArgs e)
		{
			if (SaveZone())
				Response.Redirect(ReturnUrlTracker.GetReturnUrl());
		}

		private bool SaveZone()
		{
			bool saved = false;
			string zipSql = String.Empty;
			string zoneName = txtZoneName.Text.Trim();
			string zipCodes = CleanZipCodes(txtZipCodes.Text.TrimEnd(',').Trim());
			bool zipsAreValid = ValidateZipCodes(zipCodes);

			if (zipsAreValid)
			{
				List<SqlParameter> zipParams = new List<SqlParameter> { new SqlParameter("@ZipCodes", zipCodes),
																		new SqlParameter("@Name", zoneName) };

				if (Editing)
				{
					int zoneId = int.Parse(litZoneId.Text);

					zipParams.Add(new SqlParameter("@ShippingZoneID", zoneId));

					zipSql = "UPDATE ShippingZone SET Name = @Name, ZipCodes = @ZipCodes WHERE ShippingZoneID = @ShippingZoneID";
				}
				else
				{					
					zipSql = "INSERT INTO ShippingZone (Name, ZipCodes) VALUES (@Name, @ZipCodes)";
				}

				try
				{
					DB.ExecuteSQL(zipSql, zipParams.ToArray());
					ctrlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.orderdetails.UpdateSuccessful", ThisCustomer.LocaleSetting), AlertMessage.AlertType.Success);
					saved = true;
				}
				catch(Exception ex)
				{
					ctrlAlertMessage.PushAlertMessage(ex.Message, AlertMessage.AlertType.Error);
				}

				if(!Editing)
				{
					//Added a new zone - get the ID so we can reload the form
					ShippingZoneID = DB.GetSqlN("SELECT TOP 1 ShippingZoneID N FROM ShippingZone ORDER BY ShippingZoneID DESC");
				}
			}
			else
			{
				ctrlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.editshippingzone.EnterZipCodes", ThisCustomer.LocaleSetting), AlertMessage.AlertType.Error);
			}

			return saved;
		}	

        private string CleanZipCodes(string zipCodes)
        {
            return Regex.Replace(zipCodes, "\\s+", "", RegexOptions.Compiled);
        }

        private bool ValidateZipCodes(string zipCodes)
        {
            if (zipCodes.Length > 0)
            {
                string[] commaZipCodesSplit = Regex.Split(zipCodes, ",", RegexOptions.IgnorePatternWhitespace);

                foreach (string zipCode in commaZipCodesSplit)
                {
                    //Returns true if the Special Character (-) do not occur or occur 1 time
                    bool isHyphenated = zipCode.IndexOf('-') >= 0;

                    if (isHyphenated)
                    {
                        bool hyphenOcurrenceIsOnlyOnce = zipCode.IndexOf("-") == zipCode.LastIndexOf("-");
                        if (hyphenOcurrenceIsOnlyOnce)
                        {
                            string[] hypenZipCodesSplit = Regex.Split(zipCode, "-", RegexOptions.IgnorePatternWhitespace);

                            if (!ValidateZipCode(hypenZipCodesSplit[START_RANGE]) ||
                                !ValidateZipCode(hypenZipCodesSplit[END_RANGE]))
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!ValidateZipCode(zipCode))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        private bool ValidateZipCode(string zipCode)
        {
            if (zipCode.Length < 5)
            {
                zipCode = zipCode.PadRight(5, '0');
            }

            return CommonLogic.IsInteger(zipCode);
        }
    }
}
