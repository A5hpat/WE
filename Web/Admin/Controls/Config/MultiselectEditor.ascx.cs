// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

namespace AspDotNetStorefrontControls.Config
{
	public partial class MultiselectEditor : ConfigEditorControl
	{
		protected string Value
		{ get; set; }

		protected IEnumerable<string> AllowedValues
		{ get; set; }

		public override void SetValue(int storeId, string value, bool exists, IEnumerable<string> allowedValues, string defaultValue)
		{
			StoreId = storeId;
			Value = value;
			Exists = exists;
			DefaultValue = defaultValue;

			ValueEditor.DataSource = allowedValues;
		}

		public override Tuple<int, string, bool> GetValue()
		{
			var selectedItemString = String.Join(
				",",
				ValueEditor.Items
					.OfType<ListItem>()
					.Where(item => item.Selected)
					.Select(item => item.Value));

			return Tuple.Create(
				StoreId,
				String.IsNullOrEmpty(selectedItemString)
					? null
					: selectedItemString,
				Exists);
		}

		public override void DataBind()
		{
			base.DataBind();

			var selectedItems = Value
				.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(s => s.Trim());

			foreach(ListItem item in ValueEditor.Items)
				item.Selected = selectedItems.Contains(item.Value, StringComparer.OrdinalIgnoreCase);
		}
	}
}
