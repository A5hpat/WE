// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;
using Telerik.Web.UI;
using System.Linq;

namespace AspDotNetStorefrontAdmin.Controls
{
    public partial class StoreEdit : BaseUserControl<Store>
    {
        private string m_headertext;
        private string m_popuptargetcontrolid;
        private bool m_clonemode;
		private Customer _customer;

        public string HeaderText
        {
            get { return m_headertext; }
            set { m_headertext = value; }
        }

        public string PopupTargetControlID
        {
            get { return m_popuptargetcontrolid; }
            set { m_popuptargetcontrolid = value; }
        }

        public bool VisibleOnPageLoad
        {
            get
            {
                return string.IsNullOrEmpty(pnlEditStore.Style["display"]);
            }
            set
            {
                pnlEditStore.Style["display"] = value ? string.Empty : "none";
            }
        }

        public bool CloneMode
        {
            get { return m_clonemode; }
            set { m_clonemode = value; }
        }

		protected void Page_Load(object sender, EventArgs e)
		{
			_customer = Context.GetCustomer();
        }

        public override void DataBind()
        {
            base.DataBind();

			cmbSkinID.Items.Clear();
			ISkinProvider skinProvider = new SkinProvider();
			var allSkins = skinProvider.GetSkins();
			//exclude mobile skins from the options
			var skins = allSkins.Where(s => !s.IsMobile);

			foreach(var skin in skins)
			{
				var displayName = String.IsNullOrEmpty(skin.DisplayName) ? skin.Name : skin.DisplayName;
				cmbSkinID.Items.Add(new ListItem(displayName, skin.Id.ToString()));
			}

            var store = this.Datasource;

			if (cmbSkinID.Items.FindByValue(store.SkinID.ToString()) != null)
				cmbSkinID.SelectedValue = store.SkinID.ToString();

            phRegisterWithBuySafe.Visible = cbxBuySafe.Checked = store.StoreID < 1;

            extEditStorePanel.TargetControlID = this.PopupTargetControlID;          

			if(this.CloneMode)
            {
                txtStoreName.Text = "{0} - Clone".FormatWith(Datasource.Name);
            }
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

        public override bool UpdateChanges()
        {
            var store = this.Datasource;
			if(this.CloneMode)
            {   // clone first the details
                store = store.CloneStore();
            }
            store.Name = txtStoreName.Text;
            store.Description = txtDescription.Text;
            store.ProductionURI = txtProductionURI.Text;
            store.ProductionDirectoryPath = FormatDirectoryPath(txtProductionDirectoryPath.Text);
			store.ProductionPort = string.Empty;
			store.StagingURI = txtStagingURI.Text;
			store.StagingDirectoryPath = FormatDirectoryPath(txtStagingDirectoryPath.Text);
			store.StagingPort = string.Empty;
            store.DevelopmentURI = txtDevURI.Text;
            store.DevelopmentDirectoryPath = FormatDirectoryPath(txtDevelopmentDirectoryPath.Text);
			store.DevelopmentPort = txtDevelopmentPort.Text;
            store.Published = chkPublished.Checked;
            store.SkinID = cmbSkinID.SelectedValue.ToNativeInt();
            if (store.StoreID < 1 && cbxBuySafe.Checked)
                AspDotNetStorefrontBuySafe.BuySafeController.RegisterStore(store);
            store.Save();
            OnUpdatedChanges(EventArgs.Empty);
			CachelessStore.resetStoreCache();
            return true;
        }

		string FormatDirectoryPath(string directoryPath)
		{
			var plainPath = directoryPath.Trim().Trim('/');
			if(string.IsNullOrEmpty(plainPath))
				return string.Empty;

			return string.Format("/{0}", plainPath);
		}

        public string GetPopupCommandScript()
        {            
            return "$find('{0}').show();return false;".FormatWith(extEditStorePanel.ClientID);
        }

        protected void cmdSave_Click(object sender, EventArgs e)
        {
			if(txtStoreName.Text.Trim().Length == 0 && IsPostBack == true)
            {
                SetError(AppLogic.GetString("admin.stores.EnterName", _customer.LocaleSetting));
				return;
            }

			try
			{
				if(!String.IsNullOrEmpty(txtProductionURI.Text))
				{
                    Uri url = new Uri(String.Format("http://{0}", txtProductionURI.Text.Trim().Replace("http://", String.Empty).Replace("https://", String.Empty)));
					if(url.AbsolutePath != "/")
					{      
                        SetError(AppLogic.GetString("admin.stores.ProductionOnlyHostURL", _customer.LocaleSetting));						
						return;
					}

					txtProductionURI.Text = url.Host;
				}
			}
			catch
			{
                SetError(AppLogic.GetString("admin.stores.ProductionOnlyHostURL", _customer.LocaleSetting));
				return;
			}

			try
			{
				if(!String.IsNullOrEmpty(txtStagingURI.Text))
				{
                    Uri url = new Uri(String.Format("http://{0}", txtStagingURI.Text.Trim().Replace("http://", String.Empty).Replace("https://", String.Empty)));
					if(url.AbsolutePath != "/")
					{
                        SetError(AppLogic.GetString("admin.stores.StagingOnlyHostURL", _customer.LocaleSetting));
						return;
					}

					txtStagingURI.Text = url.Host;
				}
			}
			catch
			{
                SetError(AppLogic.GetString("admin.stores.StagingOnlyHostURL", _customer.LocaleSetting));
				return;
			}

			try
			{
				if(!String.IsNullOrEmpty(txtDevURI.Text))
				{
                    Uri url = new Uri(String.Format("http://{0}", txtDevURI.Text.Trim().Replace("http://", String.Empty).Replace("https://", String.Empty)));
					if(url.AbsolutePath != "/")
					{
                        SetError(AppLogic.GetString("admin.stores.DevOnlyHostURL", _customer.LocaleSetting));
						return;
					}

					txtDevURI.Text = url.Host;
				}
			}
			catch
			{
                SetError(AppLogic.GetString("admin.stores.DevOnlyHostURL", _customer.LocaleSetting));
				return;
			}
			
			if(txtDevelopmentPort.Text.Equals("80"))
				txtDevelopmentPort.Text = string.Empty;

			if(!string.IsNullOrEmpty(txtDevelopmentPort.Text))
			{
				int port;
				if(!int.TryParse(txtDevelopmentPort.Text, out port) || port < 0)
				{
					SetError(AppLogic.GetString("admin.storecontrol.port.developmenterror", _customer.LocaleSetting));
					return;
				}
			}

			UpdateChanges();
        }

        /// <summary>
        /// Sets an error message. Used here because the new alertmessage control is not working with modal popups as expected.
        /// </summary>
        /// <param name="message"></param>
        private void SetError(string message)
        {
            lblError.Text = message;
            divError.Visible = true;
            extEditStorePanel.Show();
        }
    }
}
