// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
namespace AspDotNetStorefrontControls
{
	public partial class LinkGroupLinks : System.Web.UI.UserControl
	{
		public string SelectedLink { get; set; }

		public Dictionary<string, string> Links { get; set; }

		protected override void OnPreRender(EventArgs e)
		{
			if(Links != null)
			{
				LinkGroup.DataSource = Links;
				LinkGroup.DataBind();
			}
			base.OnPreRender(e);
		}

	}
}
