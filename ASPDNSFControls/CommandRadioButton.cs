// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AspDotNetStorefrontControls
{
	public class CommandRadioButton : RadioButton
	{
		[Bindable(true)]
		[DefaultValue("")]
		[Themeable(false)]
		public string CommandArgument
		{
			get { return ((string)ViewState["CommandArgument"]) ?? string.Empty; }
			set { ViewState["CommandArgument"] = value; }
		}

		[DefaultValue("")]
		[Themeable(false)]
		public string CommandName
		{
			get { return ((string)ViewState["CheckedCommandName"]) ?? string.Empty; }
			set { ViewState["CheckedCommandName"] = value; }
		}

		public event CommandEventHandler Command;

		protected virtual void OnCommand(CommandEventArgs e)
		{
			if(Command != null)
				Command(this, e);

			RaiseBubbleEvent(this, e);
		}

		protected override void OnCheckedChanged(EventArgs e)
		{
			// Handle the check changed
			base.OnCheckedChanged(e);

			if(!Checked)
				return;

			// Then generate a command event
			OnCommand(
				new CommandEventArgs(
					CommandName ?? string.Empty,
					CommandArgument));
		}
	}
}
