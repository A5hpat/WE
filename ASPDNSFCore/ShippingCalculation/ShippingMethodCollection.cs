// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;

namespace AspDotNetStorefrontCore.ShippingCalculation
{
	public class ShippingMethodCollection : List<ShippingMethod>
	{
		public string ErrorMsg
		{ get; set; }

		public ShippingMethodCollection()
		{
			ErrorMsg = string.Empty;
		}

		public ShippingMethodCollection(IEnumerable<ShippingMethod> collection)
			: base(collection)
		{
			ErrorMsg = string.Empty;
		}

		public bool MethodExists(string name)
		{
			return ToArray()
				.Select(method => method.Name)
				.Contains(name, StringComparer.OrdinalIgnoreCase);
		}

		public int GetIndex(string name)
		{
			return FindIndex(method => StringComparer.OrdinalIgnoreCase.Equals(method.Name, name));
		}

		public IEnumerable<ShippingMethod> PerCarrier(string carrier)
		{
			return this
				.Where(method => StringComparer.OrdinalIgnoreCase.Equals(method.Carrier, carrier));
		}
	}
}

