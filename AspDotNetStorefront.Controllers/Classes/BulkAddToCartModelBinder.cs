// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using AspDotNetStorefront.Models;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Classes
{
	public class BulkAddToCartModelBinder : IModelBinder
	{
		public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
			if(bindingContext == null)
				throw new ArgumentNullException("bindingContext");

			var addToCartItems = new List<AddToCartPostModel>();

			var form = controllerContext.HttpContext.Request.Form;

			var returnUrl = form.Get("ReturnUrl");

			foreach(var inputName in form.AllKeys)
			{
				var inputNameInfo = inputName.Split('.');
				if(inputNameInfo.Length != 2)
					continue;

				var productIdentifier = inputNameInfo[0];
				var fieldName = inputNameInfo[1];

				var itemInfo = productIdentifier.Split('_');
				if(itemInfo.Length < 3)
					continue;

				int productId;
				if(!int.TryParse(itemInfo[1], out productId))
					continue;

				int variantId;
				if(!int.TryParse(itemInfo[2], out variantId))
					continue;

				int colorIndex = -1;
				int sizeIndex = -1;
				if(itemInfo.Length == 5)
				{
					if(!int.TryParse(itemInfo[3], out colorIndex))
						continue;

					if(!int.TryParse(itemInfo[4], out sizeIndex))
						continue;
				}

				if(StringComparer.OrdinalIgnoreCase.Equals(fieldName, "CustomerEnteredPrice"))
				{
					var price = 0.00M;
					if(!decimal.TryParse(form[inputName], out price))
						continue;

					if(price == 0)
						continue;

					addToCartItems.Add(new AddToCartPostModel
					{
						VariantId = variantId,
						ProductId = productId,
						Quantity = 1,
						CustomerEnteredPrice = price,
					});
				}
				else if(StringComparer.OrdinalIgnoreCase.Equals(fieldName, "Quantity"))
				{
					var quantity = 1;
					if(!int.TryParse(form[inputName], out quantity))
						continue;

					if(quantity == 0)
						continue;

					var chosenColor = string.Empty;
					var chosenSize = string.Empty;

					if(colorIndex > -1 || sizeIndex > -1)
					{
						using(SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
						{
							dbconn.Open();

							using(IDataReader rs = DB.GetRS(
								"select Colors, Sizes from productvariant where VariantID = @VariantId", 
								dbconn, 
								new[] { new SqlParameter("VariantId", variantId) }))
							{
								rs.Read();
								var defaultLocale = AspDotNetStorefrontCore.Localization.GetDefaultLocale();
								var colors = DB.RSFieldByLocale(rs, "Colors", defaultLocale).Split(',');
								if(colors.Length > colorIndex)
									chosenColor = colors[colorIndex].Trim();

								var sizes = DB.RSFieldByLocale(rs, "Sizes", defaultLocale).Split(',');
								if(sizes.Length > sizeIndex)
									chosenSize = sizes[sizeIndex].Trim();
							}
						}
					}

					addToCartItems.Add(new AddToCartPostModel
					{
						VariantId = variantId,
						ProductId = productId,
						Quantity = quantity,
						Color = chosenColor,
						Size = chosenSize
					});
				}
			}

			return new BulkAddToCartPostModel
			{
				AddToCartItems = addToCartItems,
				ReturnUrl = returnUrl
			};
		}
	}
}
