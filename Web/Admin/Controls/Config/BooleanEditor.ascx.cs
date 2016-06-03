// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;

namespace AspDotNetStorefrontControls.Config
{
	public partial class BooleanEditor : ConfigEditorControl
	{
		protected string Value
		{ get; set; }

		public override void SetValue(int storeId, string value, bool exists, IEnumerable<string> allowedValues, string defaultValue)
		{
			StoreId = storeId;
			Value = value;
			Exists = exists;
			DefaultValue = defaultValue;
		}

		public override Tuple<int, string, bool> GetValue()
		{
			return Tuple.Create(
				StoreId,
				String.IsNullOrEmpty(ValueEditor.SelectedValue)
					? null
					: ValueEditor.SelectedValue,
				Exists);
		}

		public override void DataBind()
		{
			base.DataBind();

			foreach(ListItem item in ValueEditor.Items)
				item.Selected = item.Value.Equals(Value, StringComparison.OrdinalIgnoreCase);
		}
	}
}
