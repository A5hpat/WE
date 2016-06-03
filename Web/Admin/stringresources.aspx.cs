// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class stringresourcepage : AspDotNetStorefront.Admin.AdminPageBase
	{
		protected const string DeleteCommand = "stringResource:delete";
		protected const string ClearLocalCommand = "stringResource:clearLocale";

		protected readonly LocaleSource LocaleSource;

		public stringresourcepage()
		{
			LocaleSource = new LocaleSource();
		}

		protected void gMain_RowCommand(object sender, GridViewCommandEventArgs e)
		{
			if(e.CommandName == DeleteCommand)
			{
				var stringResourceId = Localization.ParseNativeInt((string)e.CommandArgument);

				DB.ExecuteSQL(
					"delete from StringResource where StringResourceID = @id",
					new[] { new SqlParameter("@id", stringResourceId) });

				AlertMessage.PushAlertMessage("String resource deleted", AspDotNetStorefrontControls.AlertMessage.AlertType.Success);
			}
		}

		protected void ClearLocaleLink_Command(object sender, CommandEventArgs e)
		{
			if(e.CommandName == ClearLocalCommand)
			{
				var locale = (string)e.CommandArgument;

				DB.ExecuteSQL(
					"delete from StringResource where LocaleSetting = @locale",
					new[] { new SqlParameter("@locale", locale) });

				AlertMessage.PushAlertMessage("Locale cleared", AspDotNetStorefrontControls.AlertMessage.AlertType.Success);
			}
		}
	}
}
