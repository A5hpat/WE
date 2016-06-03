// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;
using Telerik.Web.UI;

namespace AspDotNetStorefrontControls
{
	public partial class StoreControl : BaseUserControl<List<Store>>
	{

		public StoreControl()
		{
		}

		private List<Store> lstStores
		{
			get;
			set;
		}

		protected override void OnInit(EventArgs e)
		{
			BindData();
			InitializeAddNewStore();

			base.OnInit(e);
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			if(lstStores == null)
			{
				lstStores = Store.GetStoreList();
			}
		}

		private void BindData()
		{
			BindData(false);
		}

		private void BindData(bool refresh)
		{
			lstStores = Store.GetStores(true);

			Datasource = lstStores;

			grdStores.DataSource = lstStores;
			grdStores.DataBind();
		}

		/// <summary>
		/// Telerik compatible extension function for each individual bound item to get the actual type bound
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="item"></param>
		/// <returns></returns>
		protected T DataItemAs<T>(GridItem item) where T : class
		{
			return item.DataItem as T;
		}

		protected void grdStores_ItemCommand(object source, GridCommandEventArgs e)
		{
			bool requiresRestartCache = false;
			bool requiresRebind = false;

			switch(e.CommandName)
			{
				case "CopyStore":
					var cboCopystoreFrom = e.Item.FindControl<DropDownList>("cboCopystoreFrom");
					var sourceStoreId = cboCopystoreFrom.SelectedValue.ToNativeInt();

					var destinationStoreId = e.CommandArgument.ToString().ToNativeInt();

					CopyFromStore(sourceStoreId, destinationStoreId);

					requiresRestartCache = true;
					requiresRebind = true;
					break;
			}

			// requires restart cache
			if(requiresRestartCache)
			{
				RestartCache();
			}

			if(requiresRebind)
			{
				BindData(true);
			}
		}

		private void RestartCache()
		{
			AppLogic.m_RestartApp();
		}

		private void DeleteToggle(int storeId)
		{
			var store = Datasource.Find(s => s.StoreID == storeId);
			if(store != null)
			{
				if(store.Deleted == false)
				{
					store.DeleteStore();
				}
				else
				{
					store.UnDeleteStore();
				}
			}
		}

		private void PublishToggle(int storeId)
		{
			var store = Datasource.Find(s => s.StoreID == storeId);
			if(store != null)
			{
				store.PublishSwitch();
			}
		}

		/// <summary>
		/// Copies and overwrites the settings of the source store to the destination store
		/// </summary>
		/// <param name="sourceStoreId"></param>
		/// <param name="destinationStoreId"></param>
		private void CopyFromStore(int sourceStoreId, int destinationStoreId)
		{
			var from_Store = Datasource.Find(store => store.StoreID == sourceStoreId);
			var to_Store = Datasource.Find(store => store.StoreID == destinationStoreId);

			if(from_Store != null &&
				to_Store != null)
			{
				to_Store.CopyFrom(from_Store);
			}
		}

		protected void grdStores_SortCommand(object source, GridSortCommandEventArgs e)
		{
		}

		protected void grdStores_ItemCreated(object sender, GridItemEventArgs e)
		{
			if(e.Item.ItemType == GridItemType.Item ||
				e.Item.ItemType == GridItemType.AlternatingItem)
			{
				var chkDefault = e.Item.FindControl<DataCheckBox>("chkDefault");
				chkDefault.CheckedChanged += new EventHandler(chkDefault_CheckedChanged);

				var ctrlEditStore = e.Item.FindControl<AspDotNetStorefrontAdmin.Controls.StoreEdit>("ctrlEditStore");
				if(ctrlEditStore != null)
				{
					var btnEditStore = e.Item.FindControl<LinkButton>("btnEditStore");
					btnEditStore.OnClientClick = ctrlEditStore.GetPopupCommandScript();

					// attach the event handler that will notify us whenever the content has been updated
					ctrlEditStore.UpdatedChanges += new EventHandler(ctrlStore_UpdatedChanges);
				}

				var ctrlEditStoreUsingButton = e.Item.FindControl<AspDotNetStorefrontAdmin.Controls.StoreEdit>("ctrlEditStoreUsingButton");
				if(ctrlEditStoreUsingButton != null)
				{
					var btnEditStore = e.Item.FindControl<LinkButton>("btnEditStore");
					btnEditStore.OnClientClick = ctrlEditStoreUsingButton.GetPopupCommandScript();

					// attach the event handler that will notify us whenever the content has been updated
					ctrlEditStoreUsingButton.UpdatedChanges += new EventHandler(ctrlStore_UpdatedChanges);
				}
			}
		}

		private void chkDefault_CheckedChanged(object sender, EventArgs e)
		{
			var chkDefault = sender as DataCheckBox;
			int storeId = chkDefault.Data.ToString().ToNativeInt();
			var store = Datasource.Find(s => s.StoreID == storeId);
			if(store != null)
			{
				store.SetDefault();
				RestartCache();
				BindData(true);
			}
		}

		protected void cbxPublish_CheckedChanged(object sender, EventArgs e)
		{
			bool requiresRestartCache = false;
			bool requiresRebind = false;
			var cbxPublish = sender as DataCheckBox;
			int pstoreId = cbxPublish.Data.ToString().ToNativeInt();

			PublishToggle(pstoreId);
			requiresRestartCache = true;
			requiresRebind = true;

			// requires restart cache
			if(requiresRestartCache)
			{
				RestartCache();
			}

			if(requiresRebind)
			{
				BindData(true);
			}
		}

		protected void cbxDelete_CheckedChanged(object sender, EventArgs e)
		{
			bool requiresRestartCache = false;
			bool requiresRebind = false;
			var cbxDelete = sender as DataCheckBox;
			int storeId = cbxDelete.Data.ToString().ToNativeInt();

			DeleteToggle(storeId);
			requiresRestartCache = true;
			requiresRebind = true;

			// requires restart cache
			if(requiresRestartCache)
			{
				RestartCache();
			}

			if(requiresRebind)
			{
				BindData(true);
			}
		}

		protected void grdStores_ItemDataBound(object sender, GridItemEventArgs e)
		{
			if(e.Item.ItemType == GridItemType.Item ||
				e.Item.ItemType == GridItemType.AlternatingItem)
			{
				var currentStore = e.Item.DataItem as Store;

				var cboCopystoreFrom = e.Item.FindControl<DropDownList>("cboCopystoreFrom");
				if(cboCopystoreFrom != null)
				{
					var otherStores = this.Datasource.Except(new Store[] { currentStore });

					foreach(var otherStore in otherStores)
					{
						var text = "({0}) {1}".FormatWith(otherStore.StoreID, otherStore.Name);
						cboCopystoreFrom.Items.Add(new ListItem(text, otherStore.StoreID.ToString()));
					}
				}

				var cbxDelete = e.Item.FindControl<DataCheckBox>("cbxDelete");				

				if(cbxDelete != null)
				{
					PrepareRadDeleteConfirm(cbxDelete, currentStore);
				}
			}
		}			

		protected string HTTPFy(string url)
		{
			return "http://" + url;
		}

		protected void PrepareRadDeleteConfirm(DataCheckBox cbxDelete, Store boundStore)
		{
			if(boundStore.Deleted)
			{
				cbxDelete.CssClass = "undelete-box";
			}
			else
			{
				cbxDelete.CssClass = "delete-box";
			}
		}

		protected string DeleteText(Store boundStore)
		{
			var text = "Delete";
			if(boundStore.Deleted)
			{
				text = "Un-Delete";
			}

			return text;
		}

		protected string PublishText(Store boundStore)
		{
			var text = "Un-Publish";
			if(boundStore.Published == false)
			{
				text = "Publish";
			}

			return text;
		}

		private void InitializeAddNewStore()
		{
			var newStore = new Store();
			newStore.Deleted = false;
			newStore.Description = string.Empty;
			newStore.DevelopmentURI = string.Empty;
			newStore.StagingURI = string.Empty;
			newStore.ProductionURI = string.Empty;
			newStore.IsDefault = false;
			newStore.Published = true;
			newStore.CreatedOn = DateTime.Now;
			newStore.Name = string.Empty;
			newStore.SkinID = 1;
			ctrlAddStore.Datasource = newStore;
			ctrlAddStore.ThisCustomer = ThisCustomer;
			ctrlAddStore.VisibleOnPageLoad = false;
			ctrlAddStore.DataBind();
		}

		#region "Edit Box Handlers"

		protected void ctrlStore_UpdatedChanges(object sender, EventArgs e)
		{
			// re-initialize add
			InitializeAddNewStore();

			BindData(true);
		}

		#endregion
	}
}
