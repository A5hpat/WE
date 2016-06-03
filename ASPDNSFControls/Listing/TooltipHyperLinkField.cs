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
	public class TooltipHyperLinkField : HyperLinkField
	{
		public string ToolTip
		{
			get { return (string)ViewState["ToolTip"] ?? String.Empty; }
			set { ViewState["ToolTip"] = value; }
		}

		public override void InitializeCell(DataControlFieldCell cell, DataControlCellType cellType, DataControlRowState rowState, int rowIndex)
		{
			base.InitializeCell(cell, cellType, rowState, rowIndex);
			var hyperlink = cell.Controls
				.OfType<HyperLink>()
				.FirstOrDefault();
			
			if(hyperlink == null)
				return;
			
			hyperlink.ToolTip = ToolTip;
		}
	}
}
