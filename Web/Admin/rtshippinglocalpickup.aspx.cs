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
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	/// <summary>
	/// Summary description for inventory
	/// </summary>
	public partial class _RTShippingLocalPickup : AspDotNetStorefront.Admin.AdminPageBase
	{

		protected void Page_Load(object sender, EventArgs e)
		{

			Response.CacheControl = "private";
			Response.Expires = -1;
			Response.AddHeader("pragma", "no-cache");

			Page.Form.DefaultButton = btnUpdate.UniqueID;

			if(!Page.IsPostBack)
			{
				LoadLocaleContent();
				InitializePageContent();
			}

			InitializePanels();
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			btnClose.DataBind();
			btnCloseTop.DataBind();
		}

		/// <summary>
		/// Update the appconfig and stringresource 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void btnUpdate_Click(object sender, EventArgs e)
		{
			UpdateAppConfig();
			UpdateRestrictions();
			InitializePageContent();
			ctrlAlertMessage.PushAlertMessage(AppLogic.GetString("In-Store Pickup Updated", ThisCustomer.LocaleSetting), AlertMessage.AlertType.Success);
		}

		/// <summary>
		/// Change the locale
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void ddlLocale_SelectedIndexChanged(object sender, EventArgs e)
		{
			InitializePageContent();
		}

		/// <summary>
		/// Get all the localesetting
		/// </summary>
		private void LoadLocaleContent()
		{
			if(!Page.IsPostBack)
			{
				ddlLocale.Items.Clear();
				//Populate the dropdowlist for localesetting
				using(SqlConnection conn = new SqlConnection(DB.GetDBConn()))
				{
					conn.Open();
					using(IDataReader localeReader = DB.GetRS("SELECT Name, Description FROM LocaleSetting  with (NOLOCK)  ORDER BY DisplayOrder,Name", conn))
					{
						while(localeReader.Read())
						{
							ddlLocale.Items.Add(new ListItem(DB.RSField(localeReader, "Description"), DB.RSField(localeReader, "Name")));
						}
					}
				}
				if(ddlLocale.Items.Count < 2)//If only have 1 locale dont show the dropdown
				{
					pnlLocale.Visible = false;
				}
				else
				{
					ddlLocale.SelectedValue = LocaleSetting;
				}
			}
		}

		/// <summary>
		/// Set the initial value
		/// </summary>
		private void InitializePageContent()
		{
			string locale = Localization.GetDefaultLocale();
			// for multilocale
			if(ddlLocale.SelectedValue != null)
			{
				locale = ddlLocale.SelectedValue;
			}

			AspDotNetStorefrontCore.AppConfig allowLocalPickup = AppLogic.GetAppConfigRouted("RTShipping.AllowLocalPickup");
			if(allowLocalPickup != null)
				imgAllowLocalPickup.ToolTip = allowLocalPickup.Description;

			AspDotNetStorefrontCore.AppConfig localPickupCost = AppLogic.GetAppConfigRouted("RTShipping.LocalPickupCost");
			if(localPickupCost != null)
				imgRTShippingLocalPickupHandlingFee.ToolTip = localPickupCost.Description;

			AspDotNetStorefrontCore.AppConfig localPickupRestrictionType = AppLogic.GetAppConfigRouted("RTShipping.LocalPickupRestrictionType");
			if(localPickupRestrictionType != null)
				imgRestrictionType.ToolTip = localPickupRestrictionType.Description;

			AspDotNetStorefrontCore.AppConfig localPickupRestrictionStates = AppLogic.GetAppConfigRouted("RTShipping.LocalPickupRestrictionStates");
			if(localPickupRestrictionStates != null)
				imgRestrictionAllowedStates.ToolTip = localPickupRestrictionStates.Description;

			AspDotNetStorefrontCore.AppConfig localPickupRestrictionZips = AppLogic.GetAppConfigRouted("RTShipping.LocalPickupRestrictionZips");
			if(localPickupRestrictionZips != null)
				imgRestrictionAllowedZips.ToolTip = localPickupRestrictionZips.Description;

			AspDotNetStorefrontCore.AppConfig localPickupRestrictionZones = AppLogic.GetAppConfigRouted("RTShipping.LocalPickupRestrictionZones");
			if(localPickupRestrictionZones != null)
				imgRestrictionAllowedZones.ToolTip = localPickupRestrictionZones.Description;

			//Set initial value, this is from stringresource
			lblrtshippinglocalpickupbreadcrumb.Text = AppLogic.GetString("RTShipping.LocalPickup.Breadcrumb", SkinID, locale);

			lblAllowLocalPickup.Text = AppLogic.GetString("RTShipping.CheckBox.AllowLocalPickup", SkinID, locale);

			lblrestrictiontype.Text = AppLogic.GetString("RTShipping.LocalPickup.RestrictionTypeLabel", SkinID, locale);

			liUnrestricted.Text = AppLogic.GetString("RTShipping.LocalPickup.RestrictionType.Unrestricted", SkinID, locale);
			liState.Text = AppLogic.GetString("RTShipping.LocalPickup.RestrictionType.State", SkinID, locale);
			liZip.Text = AppLogic.GetString("RTShipping.LocalPickup.RestrictionType.Zip", SkinID, locale);
			liZone.Text = AppLogic.GetString("RTShipping.LocalPickup.RestrictionType.Zone", SkinID, locale);

			lblRTShippingLocalPickupHandlingFee.Text = AppLogic.GetString("RTShipping.LocalPickup.HandlingFeeLabel", SkinID, locale);
			txtRTShippingLocalPickupHandlingFee.Text = AppLogic.AppConfig("RTShipping.LocalPickupCost");

			btnUpdate.Text = AppLogic.GetString("admin.common.save", SkinID, locale);
			btnUpdateTop.Text = AppLogic.GetString("admin.common.save", SkinID, locale);
			lblTitle.Text = AppLogic.GetString("RTShipping.LocalPickup.TitleMessage", SkinID, locale);
			lblRestrictionsTitle.Text = AppLogic.GetString("RTShipping.LocalPickup.RestrictionsMessage", SkinID, locale);
			lblRestrictionAllowedZones.Text = AppLogic.GetString("RTShipping.LocalPickup.Restriction.Zones", SkinID, LocaleSetting);
			lblRestrictionAllowedZips.Text = AppLogic.GetString("RTShipping.LocalPickup.Restriction.Zips", SkinID, LocaleSetting);
			lblRestrictionAllowedStates.Text = AppLogic.GetString("RTShipping.LocalPickup.Restriction.States", SkinID, LocaleSetting);

			InitializeSelectedValue();
		}

		/// <summary>
		/// Get the initial selected value
		/// </summary>
		private void InitializeSelectedValue()
		{
			if(AppLogic.AppConfigBool("RTShipping.AllowLocalPickup"))
			{
				cbxAllowLocalPickup.Checked = true;
			}

			String sRestrictionType = AppLogic.AppConfig("RTShipping.LocalPickupRestrictionType");

			if(sRestrictionType.Equals("state", StringComparison.InvariantCultureIgnoreCase))
			{
				liState.Selected = true;
			}
			else if(sRestrictionType.Equals("zip", StringComparison.InvariantCultureIgnoreCase))
			{
				liZip.Selected = true;

				String allowedzips = AppLogic.AppConfig("RTShipping.LocalPickupRestrictionZips").Trim();

				txtRestrictionAllowedZips.Text = HttpUtility.HtmlEncode(allowedzips);
			}
			else if(sRestrictionType.Equals("zone", StringComparison.InvariantCultureIgnoreCase))
			{
				liZone.Selected = true;
			}
			else
			{
				liUnrestricted.Selected = true;
			}
		}

		/// <summary>
		/// Update appconfig
		/// </summary>
		private void UpdateAppConfig()
		{
			//Update the appconfigs through checkbox
			AppConfigManager.SetAppConfigValue("RTShipping.AllowLocalPickup", cbxAllowLocalPickup.Checked.ToString(), AppLogic.StoreID());

			//Update the appconfig through text box
			AppConfigManager.SetAppConfigValue(
				"RTShipping.LocalPickupCost",
				txtRTShippingLocalPickupHandlingFee.Text.Trim().Length == 0
					? "0.00"
					: txtRTShippingLocalPickupHandlingFee.Text, 
				AppLogic.StoreID());

			//Update the appconfig through radio button
				if(liState.Selected)
					AppConfigManager.SetAppConfigValue("RTShipping.LocalPickupRestrictionType", "state", AppLogic.StoreID());
				else if(liZip.Selected)
					AppConfigManager.SetAppConfigValue("RTShipping.LocalPickupRestrictionType", "zip", AppLogic.StoreID());
				else if(liZone.Selected)
					AppConfigManager.SetAppConfigValue("RTShipping.LocalPickupRestrictionType", "zone", AppLogic.StoreID());
				else
					AppConfigManager.SetAppConfigValue("RTShipping.LocalPickupRestrictionType", "unrestricted", AppLogic.StoreID());
		}

		/// <summary>
		/// Update the restrictions
		/// </summary>
		private void UpdateRestrictions()
		{
			// Unrestricted
			// Do nothing...restrictions are ignored

			// States
			if(liState.Selected)
			{
				String allowedstateids = String.Empty;
				foreach(Control ctrl in pnlStateSelect.Controls)
				{
					string Type = ctrl.GetType().ToString();
					if(Type == "System.Web.UI.WebControls.CheckBox")
					{
						CheckBox cb = (CheckBox)ctrl;
						if(cb.Checked)
						{
							allowedstateids += cb.ID.ToString().Remove(0, 4) + ",";
						}
					}
				}

				AppConfigManager.SetAppConfigValue(
					"RTShipping.LocalPickupRestrictionStates", 
					allowedstateids.TrimEnd(',').Trim(), 
					AppLogic.StoreID());
			}

			// Zones
			if(liZone.Selected)
			{
				var allowedZoneIds = new List<string>();
				String shippingZoneId = String.Empty;

				using(SqlConnection dbconn = DB.dbConn())
				{
					dbconn.Open();

					using(IDataReader rs = DB.GetRS("select ShippingZoneID from ShippingZone with (NOLOCK)", dbconn))
					{
						while(rs.Read())
						{
							shippingZoneId = DB.RSFieldInt(rs, "ShippingZoneID").ToString();
							String cbxId = HttpContext.Current.Request.Form.AllKeys.FirstOrDefault(x => x.EndsWith(String.Format(CultureInfo.InvariantCulture, "ckb_{0}", shippingZoneId)));
							if(CommonLogic.FormCanBeDangerousContent(cbxId).Equals("on", StringComparison.InvariantCultureIgnoreCase))
							{
								allowedZoneIds.Add(shippingZoneId);
							}
						}
					}
				}

				AppConfigManager.SetAppConfigValue(
					"RTShipping.LocalPickupRestrictionZones",
					string.Join(",", allowedZoneIds),
					AppLogic.StoreID());
			}

			// Zips
			if(liZip.Selected)
			{
				AppConfigManager.SetAppConfigValue(
					"RTShipping.LocalPickupRestrictionZips",
					HttpUtility.HtmlEncode(txtRestrictionAllowedZips.Text).Trim().TrimEnd(','),
					AppLogic.StoreID());
			}
		}

		/// <summary>
		/// Initialize Panels for restrictions based on restriction type setting
		/// </summary>
		private void InitializePanels()
		{
			// If unrestricted...no panels are visible
			pnlStateSelect.Visible = false;
			pnlStateSelectContainer.Visible = false;
			pnlZipSelect.Visible = false;
			pnlZoneSelect.Visible = false;
			pnlZoneSelectContainer.Visible = false;
			// If state...get states from database
			if(liState.Selected)
			{
				pnlStateSelect.Visible = true;
				pnlStateSelectContainer.Visible = true;

				int stateCount = DB.GetSqlN("SELECT COUNT(*) AS N FROM State WITH (NOLOCK)");

				if (stateCount > 0)
				{
					String[] allowedstateids = AppLogic.AppConfig("RTShipping.LocalpickupRestrictionStates").Trim().Split(',');
					using(SqlConnection dbconn = DB.dbConn())
					{
						dbconn.Open();
						using(IDataReader rs = DB.GetRS("select * from State with (NOLOCK)", dbconn))
						{
							while(rs.Read())
							{
								CheckBox ckb = new CheckBox();
								ckb.Text = DB.RSField(rs, "Name") + "<br/>";
								ckb.ID = "ckb_" + DB.RSFieldInt(rs, "StateID").ToString();

								foreach(String allowedstateid in allowedstateids)
								{
									if(allowedstateid.Length > 0)
									{
										if(Int32.Parse(allowedstateid) == (DB.RSFieldInt(rs, "StateID")))
										{
											ckb.Checked = true;
										}
									}
								}
								pnlStateSelect.Controls.Add(ckb);
							}
						}
					}
				}
				else
				{
					litNoStates.Visible = true;
				}
			}

			// If zone...get zones from database
			if(liZone.Selected)
			{
				pnlZoneSelectContainer.Visible = true;
				pnlZoneSelect.Visible = true;

				int cntZones = DB.GetSqlN("select count(*) as N from ShippingZone with (NOLOCK) where deleted <> 1");
				String shippingZoneId = String.Empty;
				if(cntZones > 0)
				{
					String[] allowedZoneIds = AppLogic.AppConfig("RTShipping.LocalpickupRestrictionZones").Trim().Split(',');

					using(SqlConnection dbconn = DB.dbConn())
					{
						dbconn.Open();
						using(IDataReader rs = DB.GetRS("select * from ShippingZone with (NOLOCK) where deleted <> 1", dbconn))
							while(rs.Read())
							{
								shippingZoneId = DB.RSFieldInt(rs, "ShippingZoneID").ToString();

								CheckBox ckb = new CheckBox();
								ckb.Text = String.Format(CultureInfo.InvariantCulture, "{0}<br/>", DB.RSField(rs, "Name"));
								ckb.ID = String.Format(CultureInfo.InvariantCulture, "ckb_{0}", shippingZoneId);
								ckb.CssClass = String.Format(CultureInfo.InvariantCulture, "ckb_{0}", shippingZoneId);
								ckb.Checked = allowedZoneIds.Any(s => s.Equals(shippingZoneId, StringComparison.Ordinal));

								pnlZoneSelect.Controls.Add(ckb);
							}
					}
				}
				else
				{
					litNoZones.Visible = true;
				}
			}

			// If zip...populate text box with comma separated zips
			if(liZip.Selected)
			{
				pnlZipSelect.Visible = true;
			}
		}
	}
}
