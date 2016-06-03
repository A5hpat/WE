// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Linq;
using System.Web.Mvc;
using AspDotNetStorefront.Routing;
using AspDotNetStorefrontCore;

public partial class Admin_Controls_StoreNavigator : System.Web.UI.UserControl
{
	public object ActionNamesstring { get; private set; }

	protected void Page_Load(object sender, EventArgs e)
    {
		BindStores();
    }

	private void BindStores()
	{
		var urlHelper = DependencyResolver.Current.GetService<UrlHelper>();
		storeList.DataSource = Store
			.GetStoreList()
			.Select(s => new
				{
					Name = s.Name,
					Url = urlHelper.ActionForStore(ActionNames.Index, ControllerNames.Home, s.StoreID)
				})
			.Where(s => Uri.IsWellFormedUriString(s.Url, UriKind.RelativeOrAbsolute));

		storeList.DataBind();
	}
}
