// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class _InventoryControl : AspDotNetStorefront.Admin.AdminPageBase
	{
		string SelectedLocale;
		int StoreId;

		protected void Page_Load(object sender, EventArgs e)
		{
			Page.Form.DefaultButton = btnUpdate.UniqueID;

			SelectedLocale = LocaleSelector
				.GetSelectedLocale()
				.Name;
			
			if(!IsPostBack)
			{
				StoreId = Store.GetDefaultStore().StoreID;
				BindStores();
			}
			else
			{
				int storeId;
				if(!int.TryParse(StoreSelector.SelectedValue, out storeId))
					return;

				StoreId = storeId;
			}

			if(!Page.IsPostBack)
				LoadPageContent(SelectedLocale, StoreId);
		}
		void BindStores()
		{
			List<Store> storeList = Store.GetStoreList();
			StoreSelector.DataSource = storeList;
			StoreSelector.DataTextField = "Name";
			StoreSelector.DataValueField = "StoreID";
			StoreSelector.DataBind();

			StoreSelector.SelectedValue = StoreId.ToString();
			StoreSelector.Visible = storeList.Count > 1;
		}

		protected void StoreSelector_SelectedIndexChanged(object sender, EventArgs e)
		{
			var selectedStoreId = 1;
			if(!int.TryParse(StoreSelector.SelectedValue, out selectedStoreId))
				return;

			StoreId = selectedStoreId;
			LoadPageContent(SelectedLocale, StoreId);
		}

		protected void Page_PreRender(object sender, EventArgs e)
		{
			DataBind();
		}

		protected void btnUpdate_Click(object sender, EventArgs e)
		{
			UpdateStringResources(SelectedLocale, StoreId);
			UpdateAppConfigs(StoreId);
			LoadPageContent(SelectedLocale, StoreId);
			ctrlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.common.updated", ThisCustomer.LocaleSetting), AlertMessage.AlertType.Success);
		}

		protected void LocaleSelector_SelectedLocaleChanged(object sender, EventArgs e)
		{
			LoadPageContent(SelectedLocale, StoreId);
		}

		void LoadPageContent(string locale, int storeId)
		{
			LoadAppConfigs(storeId);
			LoadStringResources(locale, storeId);
		}

		void LoadAppConfigs(int storeId)
		{
			txtHideProductsWithLessThanThisInventoryLevel.Text = AppLogic.AppConfig("HideProductsWithLessThanThisInventoryLevel", storeId, true);
			txtOutOfStockThreshold.Text = AppLogic.AppConfig("OutOfStockThreshold", storeId, true);

			rblShowOutOfStockMessageOnEntityPages.SelectedIndex = AppLogic.AppConfigBool("DisplayOutOfStockOnEntityPages", storeId, true) == true ? 0 : 1;
			rblShowOutOfStockMessageOnProductPages.SelectedIndex = AppLogic.AppConfigBool("DisplayOutOfStockOnProductPages", storeId, true) == true ? 0 : 1;
			rblDisplayOutOfStockMessage.SelectedIndex = AppLogic.AppConfigBool("DisplayOutOfStockProducts", storeId, true) == true ? 0 : 1;
			rdbShowOutOfStockMessage.SelectedIndex = AppLogic.AppConfigBool("KitInventory.ShowOutOfStockMessage", storeId, true) == true ? 0 : 1;
			rblAllowSaleOfOutOfStock.SelectedIndex = AppLogic.AppConfigBool("KitInventory.AllowSaleOfOutOfStock", storeId, true) == true ? 0 : 1;
			rblProductPageOutOfStockRedirect.SelectedIndex = AppLogic.AppConfigBool("ProductPageOutOfStockRedirect", storeId, true) == true ? 0 : 1;
			rblLimitCartToQuantityOnHand.SelectedIndex = AppLogic.AppConfigBool("Inventory.LimitCartToQuantityOnHand", storeId, true) == true ? 0 : 1;
		}

		void UpdateAppConfigs(int storeId)
		{

			AppConfigManager.SetAppConfigValue("HideProductsWithLessThanThisInventoryLevel", txtHideProductsWithLessThanThisInventoryLevel.Text, storeId);
			AppConfigManager.SetAppConfigValue("OutOfStockThreshold", txtOutOfStockThreshold.Text, storeId);

			AppConfigManager.SetAppConfigValue("ProductPageOutOfStockRedirect", rblProductPageOutOfStockRedirect.SelectedValue.ToString(), storeId);

			AppConfigManager.SetAppConfigValue("DisplayOutOfStockProducts", rblDisplayOutOfStockMessage.SelectedValue.ToString(), storeId);
			AppConfigManager.SetAppConfigValue("DisplayOutOfStockOnProductPages", rblShowOutOfStockMessageOnProductPages.SelectedValue.ToString(), storeId);
			AppConfigManager.SetAppConfigValue("DisplayOutOfStockOnEntityPages", rblShowOutOfStockMessageOnEntityPages.SelectedValue.ToString(), storeId);
			
			AppConfigManager.SetAppConfigValue("Inventory.LimitCartToQuantityOnHand", rblLimitCartToQuantityOnHand.SelectedValue.ToString(), storeId);

			AppConfigManager.SetAppConfigValue("KitInventory.ShowOutOfStockMessage", rdbShowOutOfStockMessage.SelectedValue.ToString(), storeId);
			AppConfigManager.SetAppConfigValue("KitInventory.AllowSaleOfOutOfStock", rblAllowSaleOfOutOfStock.SelectedValue.ToString(), storeId);
		}

		void LoadStringResources(string locale, int storeId)
		{
			txtProductOutOfStockMessage.Text = AppLogic.GetString("OutOfStock.DisplayOutOfStockOnProductPage", locale, storeId);
			txtEntityOutOfStockMessage.Text = AppLogic.GetString("OutOfStock.DisplayOutOfStockOnEntityPage", locale, storeId);
			txtProductInStockMessage.Text = AppLogic.GetString("OutOfStock.DisplayInStockOnProductPage", locale, storeId);
			txtEntityInStockMessage.Text = AppLogic.GetString("OutOfStock.DisplayInStockOnEntityPage", locale, storeId);
			txtKitItemOutOfStockSellAnyway.Text = AppLogic.GetString("OutofStock.DisplaySellableOutOfStockOnKitPage", locale, storeId);
		}

		void UpdateStringResources(string locale, int storeId)
		{
			UpdateStringResource("OutofStock.DisplayOutOfStockOnProductPage", txtProductOutOfStockMessage.Text, locale, storeId);
			UpdateStringResource("OutofStock.DisplayOutOfStockOnEntityPage", txtEntityOutOfStockMessage.Text, locale, storeId);
			UpdateStringResource("OutofStock.DisplayInStockOnProductPage", txtProductInStockMessage.Text, locale, storeId);
			UpdateStringResource("OutofStock.DisplayInStockOnEntityPage", txtEntityInStockMessage.Text, locale, storeId);
			UpdateStringResource("OutofStock.DisplaySellableOutOfStockOnKitPage", txtKitItemOutOfStockSellAnyway.Text, locale, storeId);
		}

		private void UpdateStringResource(string name, string value, string locale, int storeId)
		{
			var stringResource = StringResourceManager.GetStringResource(storeId, locale, name);
			if(stringResource == null)
			{
				StringResource.Create(storeId, name, locale, value.Trim());
				
				// Reload string resources from DB. Necessary after creating a new string resource.
				StringResourceManager.LoadAllStrings(false);
			}
			else
				stringResource.Update(storeId, name, locale, value.Trim());
		}
	}
}
