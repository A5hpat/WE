// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AspDotNetStorefrontControls
{
	public class AlertMessageContainer : WebControl, INamingContainer
	{
		protected override HtmlTextWriterTag TagKey
		{ get { return HtmlTextWriterTag.Div; } }

		public string Message
		{ get { return _Message; } }

		readonly string _Message;

		public AlertMessageContainer(string message)
		{
			_Message = message;
		}
	}
}
