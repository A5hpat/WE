// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Web.UI;

namespace AspDotNetStorefrontControls.Config
{
	public abstract class ConfigEditorControl : UserControl
	{
		protected int StoreId
		{
			get { return (int?)ViewState["StoreId"] ?? 0; }
			set { ViewState["StoreId"] = value; }
		}

		protected bool Exists
		{
			get { return (bool?)ViewState["Exists"] ?? false; }
			set { ViewState["Exists"] = value; }
		}

		public string DefaultValue
		{
			get { return (string)ViewState["DefaultValue"]; }
			set { ViewState["DefaultValue"] = value; }
		}

		public abstract void SetValue(int storeId, string value, bool exists, IEnumerable<string> allowedValues, string defaultValue);
		public abstract Tuple<int, string, bool> GetValue();		// Returns StoreId, value, exists
	}
}
