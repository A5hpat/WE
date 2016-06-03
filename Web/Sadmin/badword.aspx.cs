// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class BadWord : AspDotNetStorefront.Admin.AdminPageBase
	{
		protected void gMain_RowCommand(object sender, GridViewCommandEventArgs e)
		{
			if(e.CommandName == "DeleteItem")
			{
				gMain.EditIndex = -1;
				int badWordId = Localization.ParseNativeInt(e.CommandArgument.ToString());
				DeleteBadWord(badWordId);
			}
		}

		private void DeleteBadWord(int badWordId)
		{
			AppLogic.BadWordTable.Remove(badWordId);
			AlertMessage.PushAlertMessage("Bad word removed.", AspDotNetStorefrontControls.AlertMessage.AlertType.Success);
		}

		protected void btnSubmit_Click(object sender, EventArgs e)
		{
			if(String.IsNullOrWhiteSpace(txtWord.Text))
			{
				AlertMessage.PushAlertMessage(AppLogic.GetString("admin.BadWord.EntryRequired", SkinID, LocaleSetting), AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
				return;
			}

			// see if this name is already there:
			var word = txtWord.Text.Trim();
			if(!String.IsNullOrEmpty(AppLogic.badWord(word).Word))
			{
				AlertMessage.PushAlertMessage(AppLogic.GetString("admin.BadWord.ExistingBadWord", SkinID, LocaleSetting), AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
				return;
			}

			try
			{
				AppLogic.BadWordTable.Add(word, LocaleSetting);
				txtWord.Text = String.Empty;
				AlertMessage.PushAlertMessage("Bad word added.", AspDotNetStorefrontControls.AlertMessage.AlertType.Success);
			}
			catch(Exception ex)
			{
				AlertMessage.PushAlertMessage(ex.Message, AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
			}
		}
	}
}
