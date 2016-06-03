// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Linq;
using System.Web.Mvc;

namespace AspDotNetStorefront.Controllers.Classes
{
	public class RestrictedQuantityProvider
	{
		public SelectList BuildRestrictedQuantityList(string values)
		{
			if(string.IsNullOrEmpty(values))
				return new SelectList(items: Enumerable.Empty<object>());

			return new SelectList(
				items: values
					.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
					.Select(value => new
					{
						Value = value,
						Text = value
					}),
				dataValueField: "Value",
				dataTextField: "Text");
		}
	}
}
