// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Linq;
using System.Web.UI.WebControls;

namespace AspDotNetStorefrontControls.Listing
{
	public class CommandCheckBoxField : CheckBoxField
	{
		public string CheckedCommandName
		{
			get { return (string)ViewState["CheckedCommandName"] ?? String.Empty; }
			set { ViewState["CheckedCommandName"] = value; }
		}

		public string UncheckedCommandName
		{
			get { return (string)ViewState["UncheckedCommandName"] ?? String.Empty; }
			set { ViewState["UncheckedCommandName"] = value; }
		}

		public string CommandArgument
		{
			get { return (string)ViewState["CommandArgument"] ?? String.Empty; }
			set { ViewState["CommandArgument"] = value; }
		}

		public CommandCheckBoxField()
		{
		}

		public override void InitializeCell(DataControlFieldCell cell, DataControlCellType cellType, DataControlRowState rowState, int rowIndex)
		{
			base.InitializeCell(cell, cellType, rowState, rowIndex);
			var checkbox = cell.Controls
				.OfType<CommandCheckBox>()
				.FirstOrDefault();

			if(checkbox == null)
				return;
			
			checkbox.CheckedCommandName = CheckedCommandName;
			checkbox.UncheckedCommandName = UncheckedCommandName;
			checkbox.CommandArgument = CommandArgument;
		}
	}
}
