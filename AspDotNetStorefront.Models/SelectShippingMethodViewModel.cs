// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;

namespace AspDotNetStorefront.Models
{
	public class SelectShippingMethodViewModel
	{
		public SelectShippingMethodRenderModel RenderModel
		{ get; set; }

		public int? SelectedShippingMethodId
		{ get; set; }
	}

	public class SelectShippingMethodRenderModel
	{
		public readonly IEnumerable<ShippingMethodRenderModel> ShippingMethods;
		public readonly ShippingMethodRenderModel SelectedShippingMethod;
		public readonly bool ShowShippingIcons;
		public readonly bool CartIsAllFreeShipping;
		public int NumberOfMethodsToShow;

		public SelectShippingMethodRenderModel(IEnumerable<ShippingMethodRenderModel> shippingMethods, 
			ShippingMethodRenderModel selectedShippingMethod, 
			bool showShippingIcons, 
			bool cartIsAllFreeShipping,
			int numberOfMethodsToShow)
		{
			ShippingMethods = shippingMethods ?? Enumerable.Empty<ShippingMethodRenderModel>();
			SelectedShippingMethod = selectedShippingMethod;
			ShowShippingIcons = showShippingIcons;
			CartIsAllFreeShipping = cartIsAllFreeShipping;
			NumberOfMethodsToShow = numberOfMethodsToShow;
		}
	}

	public class ShippingMethodRenderModel
	{
		public readonly int Id;
		public readonly string Name;
		public readonly string RateDisplay;
		public readonly string ImageFileName;

		public ShippingMethodRenderModel(int id, string name, string rateDisplay, string imageFileName)
		{
			Id = id;
			Name = name;
			RateDisplay = rateDisplay;
			ImageFileName = imageFileName;
		}
	}
}
