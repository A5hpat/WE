// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace AspDotNetStorefrontControls.Config
{
	public partial class StringEditor : ConfigEditorControl
	{
		protected const int MultilineThreshold = 75;

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
				String.IsNullOrEmpty(ValueEditor.Text)
					? null
					: ValueEditor.Text,
				Exists);
		}
	}
}
