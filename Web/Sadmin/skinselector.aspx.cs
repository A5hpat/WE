// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using AspDotNetStorefront.Routing;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class skinselector : AspDotNetStorefront.Admin.AdminPageBase
	{
		ISkinProvider SkinProvider = new SkinProvider();
		Store selectedStore;
		Skin selectedStoreSkin;

		protected void Page_Load(object sender, EventArgs e)
		{
			if(!IsPostBack)
			{
				selectedStore = Store.GetDefaultStore();
				BindStores();
			}
			else
			{
				int storeId;
				if(!int.TryParse(StoreSelector.SelectedValue, out storeId))
					return;

				selectedStore = Store.GetStoreById(storeId);
			}
			SetSelectedSkin(selectedStore.SkinID);
			BindSkins();
		}

		void SetSelectedSkin(int skinId)
		{
			selectedStoreSkin = SkinProvider.GetSkinById(skinId);
			SelectedStoreSkinName.Value = selectedStoreSkin.Name;
		}

		void BindStores()
		{
			List<Store> storeList = Store.GetStoreList();
			StoreSelector.DataSource = storeList;
			StoreSelector.DataTextField = "Name";
			StoreSelector.DataValueField = "StoreID";
			StoreSelector.DataBind();

			StoreSelector.SelectedValue = selectedStore.StoreID.ToString();
		}

		private void BindSkins()
		{
			var allSkins = SkinProvider.GetSkins();

			//exclude mobile skins from the options
			var skins = allSkins.Where(s => !s.IsMobile);

			SkinNavigationList.DataSource = skins;
			SkinNavigationList.DataBind();

			SkinInfo.DataSource = skins;
			SkinInfo.DataBind();
		}

		protected void SkinNavigation_ItemDataBound(object sender, RepeaterItemEventArgs e)
		{
			if(e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
				return;

			var skin = e.Item.DataItem as Skin;
			if(skin == null)
				return;

			HtmlImage skinImage = e.Item.FindControl("SkinImageIcon") as HtmlImage;
			if(skinImage == null)
				return;

			if(String.IsNullOrEmpty(skin.PreviewUrl))
				skinImage.Visible = false;

			Literal skinDisplayName = e.Item.FindControl("DisplayName") as Literal;
			if(skinDisplayName == null)
				return;

			if(String.IsNullOrEmpty(skin.DisplayName))
			{
				skinDisplayName.Text = skin.Name;
			}

			Label CurrentSkinIndicator = e.Item.FindControl("currentSkinIndicator") as Label;
			if(CurrentSkinIndicator == null)
				return;

			CurrentSkinIndicator.ToolTip = String.Format(AppLogic.GetString("admin.SkinManagement.SkinAppliedTitle", Localization.GetDefaultLocale()), selectedStore.Name);
			CurrentSkinIndicator.Visible = selectedStoreSkin.Name == skin.Name;

		}

		protected void SetSkin_Click(object sender, CommandEventArgs e)
		{
			var skinNameToSet = e.CommandArgument as String;
			if(String.IsNullOrEmpty(skinNameToSet))
				return;

			var skin = SkinProvider.GetSkinByName(skinNameToSet);
			DB.ExecuteSQL(
				"UPDATE Profile SET PropertyValueString = @skinId WHERE PropertyName = 'SkinID' AND StoreID = @storeId", 
				new SqlParameter("skinId", skin.Id), 
				new SqlParameter("storeId", selectedStore.StoreID));

			selectedStore.SkinID = skin.Id;
			selectedStore.Save();
			SetSelectedSkin(skin.Id);
			BindSkins();
			ctrlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.SkinSelector.SkinSaved", ThisCustomer.LocaleSetting), AlertMessage.AlertType.Success);
		}

		protected void SkinInfo_ItemDataBound(object sender, RepeaterItemEventArgs e)
		{
			if(e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
				return;

			var skin = e.Item.DataItem as Skin;
			if(skin == null)
				return;

			HtmlImage skinImage = e.Item.FindControl("SkinImage") as HtmlImage;
			if(skinImage == null)
				return;

			Panel noPreview = e.Item.FindControl("NoPreviewAvailable") as Panel;
			if(noPreview == null)
				return;

			if(String.IsNullOrEmpty(skin.PreviewUrl))
			{
				skinImage.Visible = false;
				noPreview.Visible = true;
			}

			Literal skinDisplayName = e.Item.FindControl("DisplayName") as Literal;
			if(skinDisplayName == null)
				return;

			if(String.IsNullOrEmpty(skin.DisplayName))
			{
				skinDisplayName.Text = skin.Name;
			}

			HtmlGenericControl skinDescription = e.Item.FindControl("SkinDescriptionContainer") as HtmlGenericControl;
			if(skinDescription == null)
				return;

			skinDescription.Visible = String.IsNullOrEmpty(skin.Description) ? false : true;

			HyperLink previewLink = e.Item.FindControl("PreviewSkin") as HyperLink;
			if(previewLink == null)
				return;

			string previewUrl = BuildPreviewLink(skin.Id);

			if(!string.IsNullOrEmpty(previewUrl))
				previewLink.NavigateUrl = previewUrl;
			else
				DisablePreviewButton(previewLink);

			HyperLink previewLinkBottom = e.Item.FindControl("PreviewSkinBottom") as HyperLink;
			if(previewLinkBottom == null)
				return;

			string previewLinkBottomUrl = BuildPreviewLink(skin.Id);

			if(!string.IsNullOrEmpty(previewUrl))
				previewLinkBottom.NavigateUrl = previewLinkBottomUrl;
			else
				DisablePreviewButton(previewLinkBottom);

			Button setSkinButton = e.Item.FindControl("SetSkin") as Button;
			if(setSkinButton == null)
				return;

			if(selectedStoreSkin.Name == skin.Name)
				setSkinButton.Enabled = false;

			Button setSkinButtonBottom = e.Item.FindControl("SetSkinBottom") as Button;
			if(setSkinButtonBottom == null)
				return;

			if(selectedStoreSkin.Name == skin.Name)
				setSkinButtonBottom.Enabled = false;

		}

		protected void StoreSelector_SelectedIndexChanged(object sender, EventArgs e)
		{
			var selectedStoreId = 1;
			if(!int.TryParse(StoreSelector.SelectedValue, out selectedStoreId))
				return;

			selectedStore = Store.GetStoreById(selectedStoreId);
			selectedStoreSkin = SkinProvider.GetSkinById(selectedStore.SkinID);
			SelectedStoreSkinName.Value = selectedStoreSkin.Name;
		}

		protected string BuildPreviewLink(int skinId)
		{
			int storeId;
			if(!int.TryParse(StoreSelector.SelectedValue, out storeId))
				return string.Empty;

			var urlHelper = DependencyResolver.Current.GetService<UrlHelper>();
			return urlHelper.ActionForStore(ActionNames.Index, ControllerNames.Home, storeId, new System.Web.Routing.RouteValueDictionary
				{
					{  "previewSkinId", skinId }
				});
		}

		private void DisablePreviewButton(HyperLink previewLink)
		{
			previewLink.Attributes.Add("disabled", "disabled");
			previewLink.ToolTip = AppLogic.GetString("Admin.SkinSelector.Previewunavailable", ThisCustomer.LocaleSetting);
		}
	}
}
