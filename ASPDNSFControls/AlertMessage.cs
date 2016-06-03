// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AspDotNetStorefrontControls
{
	public class AlertMessage : CompositeControl
	{
		const string AlertMessageTemplatePath = "Controls/AlertMessageTemplate.ascx";

		public enum AlertType
		{
			Info, //default
			Error,
			Success,
			Warning,
			Danger
		}

		public void PushAlertMessage(string message, AlertType type)
		{
			var alertMessageQueue = (Queue<Tuple<string, AlertType>>)Page.Session["AlertMessageQueue"];

			if(alertMessageQueue == null)
				Page.Session["AlertMessageQueue"]
					= alertMessageQueue
					= new Queue<Tuple<string, AlertType>>();

			alertMessageQueue.Enqueue(Tuple.Create(message, type));
		}
		
		protected override void OnPreRender(EventArgs e)
		{
			var alertMessageQueue = (Queue<Tuple<string, AlertType>>)Page.Session["AlertMessageQueue"];
			var template = Page.LoadTemplate(AlertMessageTemplatePath);

			while(!Context.Response.IsRequestBeingRedirected && alertMessageQueue != null && alertMessageQueue.Any())
			{
				var alert = alertMessageQueue.Dequeue();
				if(String.IsNullOrEmpty(alert.Item1))
					continue;

				var container = new AlertMessageContainer(alert.Item1)
					{
						CssClass = GetAlertTypeCssClass(alert.Item2),
					};

				Controls.Add(container);
				template.InstantiateIn(container);
			}

			base.OnPreRender(e);
		}

		string GetAlertTypeCssClass(AlertType alertType)
		{
			switch(alertType)
			{
				case AlertType.Error: return "alert alert-danger";
				case AlertType.Success: return "alert alert-success";
				case AlertType.Info: return "alert alert-info";
				case AlertType.Warning: return "alert alert-warning";
				case AlertType.Danger: return "alert alert-danger";
				default: return String.Empty;
			}
		}
	}
}
