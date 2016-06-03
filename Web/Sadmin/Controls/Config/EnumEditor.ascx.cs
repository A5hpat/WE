// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontControls.Config
{
	public partial class EnumEditor : ConfigEditorControl
	{
		protected string Value
		{ get; set; }

		public override void SetValue(int storeId, string value, bool exists, IEnumerable<string> allowedValues, string defaultValue)
		{
			StoreId = storeId;
			Value = value;
			Exists = exists;
			DefaultValue = defaultValue;

			if(!Exists)
				ValueEditor.Items.Add(new ListItem(String.Format("(Default) {0}", defaultValue), String.Empty));

			ValueEditor.DataSource = allowedValues;
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

			ValueEditor.SelectFirstByValue(Value, StringComparer.OrdinalIgnoreCase);
		}
	}
}
