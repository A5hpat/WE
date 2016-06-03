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
	public class CommandCheckBox : CheckBox
	{
		[Bindable(true)]
		[DefaultValue("")]
		[Themeable(false)]
		public string CommandArgument
		{
			get { return ((string)ViewState["CommandArgument"]) ?? String.Empty; }
			set { ViewState["CommandArgument"] = value; }
		}

		[DefaultValue("")]
		[Themeable(false)]
		public string CheckedCommandName
		{
			get { return ((string)ViewState["CheckedCommandName"]) ?? String.Empty; }
			set { ViewState["CheckedCommandName"] = value; }
		}

		[DefaultValue("")]
		[Themeable(false)]
		public string UncheckedCommandName
		{
			get { return ((string)ViewState["UncheckedCommandName"]) ?? String.Empty; }
			set { ViewState["UncheckedCommandName"] = value; }
		}

		public event CommandEventHandler Command;

		protected virtual void OnCommand(CommandEventArgs e)
		{
			if(Command != null)
				Command(this, e);

			// Always bubble command events
			RaiseBubbleEvent(this, e);
		}

		protected override void OnCheckedChanged(EventArgs e)
		{
			// Handle the check changed
			base.OnCheckedChanged(e);

			// Then generate a command event
			OnCommand(
				new CommandEventArgs(
					Checked
						? CheckedCommandName ?? String.Empty
						: UncheckedCommandName ?? String.Empty,
					CommandArgument));
		}
	}
}
