// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web.UI;

namespace AspDotNetStorefrontControls.Listing
{
	public partial class DefaultFilterActions : UserControl, IGridLayoutControl
	{
		public virtual int GridColumns
		{ get { return 3; } }

		protected override void Render(HtmlTextWriter writer)
		{
			writer.WriteBeginTag("div");
			writer.WriteAttribute("class", String.Format("col-md-{0} pull-right filter-actions-container", GridColumns, 12 - GridColumns));
			writer.Write(HtmlTextWriter.TagRightChar);

			base.Render(writer);

			writer.WriteEndTag("div");
		}
	}
}
