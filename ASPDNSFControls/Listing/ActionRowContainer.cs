// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AspDotNetStorefrontControls.Listing
{
	public class ActionRowContainer : WebControl, INamingContainer
	{
		protected override HtmlTextWriterTag TagKey
		{ get { return HtmlTextWriterTag.Div; } }

		public bool LocaleSelectionEnabled
		{ get { return _LocaleSelectionEnabled; } }

		public bool ActionBarEnabled
		{ get { return _ActionBarEnabled; } }

		public string DisplayLocale
		{ get { return _DisplayLocale; } }

		readonly bool _LocaleSelectionEnabled;
		readonly bool _ActionBarEnabled;
		readonly string _DisplayLocale;

		public ActionRowContainer(bool localeSelectionEnabled, bool actionBarEnabled, string displayLocale)
		{
			_LocaleSelectionEnabled = localeSelectionEnabled;
			_ActionBarEnabled = actionBarEnabled;
			_DisplayLocale = displayLocale;
		}
	}
}
