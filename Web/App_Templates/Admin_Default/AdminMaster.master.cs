// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web.Mvc;
using AspDotNetStorefront.Admin;
using AspDotNetStorefrontCore;
using AspDotNetStorefront.Routing;

namespace AspDotNetStorefrontAdmin
{
	public partial class NewAdmin_App_Templates_AdminMaster : System.Web.UI.MasterPage
	{
        #region eventHandlers

        /// <summary>
        /// Default page load event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
			var urlHelper = DependencyResolver.Current.GetService<UrlHelper>();
			SignoutLink.HRef = urlHelper.Action(ActionNames.SignOut, ControllerNames.Account);

			String menuPath = AppLogic.AdminLinkUrl("Controls/VerticalMenu.ascx", true);
			Menu.Controls.Add(LoadControl(menuPath));

			String configurationMenuPath = AppLogic.AdminLinkUrl("Controls/ConfigurationMenu.ascx", true);
			ConfigurationMenu.Controls.Add(LoadControl(configurationMenuPath));

			String manualSearchPath = AppLogic.AdminLinkUrl("Controls/ManualSearch.ascx", true);
			ManualSearch.Controls.Add(LoadControl(manualSearchPath));

			String storeNavigatorPath = AppLogic.AdminLinkUrl("Controls/StoreNavigator.ascx", true);
			StoreNavigator.Controls.Add(LoadControl(storeNavigatorPath));
        }

        /// <summary>
        /// Handles search functionality within the admin page header
        /// </summary>
        protected void search_OnClick(object sender, EventArgs e)
        {
            Response.Redirect("search.aspx?searchterm=" + txtSearch.Text);
        }
        #endregion
    }
}
