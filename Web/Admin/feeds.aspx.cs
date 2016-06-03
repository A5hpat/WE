// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using AspDotNetStorefrontAdmin;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class Feeds : AspDotNetStorefront.Admin.AdminPageBase
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");

			if(IsPostBack)
				return;

			InitializePageData();
		}

		protected void rptrFeeds_ItemDataBound(object sender, System.Web.UI.WebControls.RepeaterItemEventArgs e)
		{
			if(e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
				return;
			
			Button btnDelete = (Button)e.Item.FindControl("btnDeleteFeed");
			btnDelete.Attributes.Add("onClick", "javascript: return confirm('" + AppLogic.GetString("admin.feeds.msgconfirmdeletion", SkinID, LocaleSetting) + "')");
		}
		
		protected void rptrFeeds_ItemCommand(object source, System.Web.UI.WebControls.RepeaterCommandEventArgs e)
		{
			switch(e.CommandName)
			{
				case "execute":
					String[] splitArgs = e.CommandArgument.ToString().Split(':');
					if(splitArgs.Length != 2)
						return;

					ExecuteFeed(Convert.ToInt32(splitArgs[0]), Convert.ToInt32(splitArgs[1]));
					break;

				case "delete":
					int FeedID = Convert.ToInt32(e.CommandArgument);
					DeleteFeed(FeedID);
					break;
			}
		}

		private void InitializePageData()
		{
			using(var dbconn = new SqlConnection(DB.GetDBConn()))
			{
				dbconn.Open();
				using(var dr = DB.GetRS("aspdnsf_GetFeed", dbconn))
				{
					rptrFeeds.DataSource = dr;
					rptrFeeds.DataBind();

				}
			}
		}

		private void ExecuteFeed(int FeedID, int StoreID)
		{
			Customer ThisCustomer = Context.GetCustomer();
			Feed f = new Feed(FeedID);
			String RuntimeParams = String.Empty;
			RuntimeParams += String.Format("SID={0}&", StoreID);
			string result = f.ExecuteFeed(ThisCustomer, RuntimeParams);
			if(!string.IsNullOrWhiteSpace(result))
				AlertMessage.PushAlertMessage(result, AspDotNetStorefrontControls.AlertMessage.AlertType.Error);

			InitializePageData();
		}

		private void DeleteFeed(int FeedID)
		{
			var result = Feed.DeleteFeed(FeedID);
			if(!string.IsNullOrWhiteSpace(result))
				AlertMessage.PushAlertMessage(result, AspDotNetStorefrontControls.AlertMessage.AlertType.Error);

			InitializePageData();
		}
	}
}
