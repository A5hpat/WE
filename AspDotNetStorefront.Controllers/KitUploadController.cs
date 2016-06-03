// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data.SqlClient;
using System.Web.Mvc;
using AspDotNetStorefront.Models;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Controllers
{
	public class KitUploadController : Controller
	{
		[HttpGet]
		public ActionResult Detail(int id, string stub)
		{
			var model = BuildKitUploadViewModel(id, stub);
			if(model == null)
				return HttpNotFound();

			return View(model);
		}

		[HttpPost]
		public ActionResult Detail(KitUploadPostModel model)
		{
			if(!ModelState.IsValid)
				return View(model);

			var kitItemInfo = GetKitItemInfo(model.ItemId, model.TemporaryFileStub);
			if(kitItemInfo == null)
				return HttpNotFound();

			if(kitItemInfo.KitItem == null)
				return HttpNotFound();

			kitItemInfo.KitItem.UploadCustomerImage(model.FileUpload);

			var viewModel = BuildKitUploadViewModel(model.ItemId, model.TemporaryFileStub);
			viewModel.ImageUrl = string.Format("{0}?nocache={1}", kitItemInfo.KitItem.CustomerUploadedImagePath, DateTime.Now.Ticks);
			viewModel.UploadSuccessful = true;
			return View(viewModel);
		}

		KitUploadViewModel BuildKitUploadViewModel(int itemId, string stub)
		{
			var itemInfo = GetKitItemInfo(itemId, stub);
			if(itemInfo == null)
				return null;

			var item = itemInfo.KitItem;
			if(item == null)
				return null;

			var customer = HttpContext.GetCustomer();

			var priceDelta = item.PriceDelta;

			var product = new Product(itemInfo.ProductId);

			var showTaxInclusive = AppLogic.AppConfigBool("Vat.Enabled") && customer.VATSettingReconciled == VATSettingEnum.ShowPricesInclusiveOfVAT;
			if(showTaxInclusive)
			{
				var taxRate = customer.TaxRate(product.TaxClassID);
				var taxMultiplier = 1M + (taxRate / 100M);
				priceDelta *= taxMultiplier;
			}

			var priceDeltaDisplay = Localization.CurrencyStringForDisplayWithExchangeRate(item.PriceDelta, customer.CurrencySetting);

			var defaultVariant = new ProductVariant(AppLogic.GetDefaultProductVariant(itemInfo.ProductId));

			if(AppLogic.AppConfigBool("VAT.Enabled") && defaultVariant.IsTaxable)
			{
				var vatSuffix = customer.VATSettingReconciled == VATSettingEnum.ShowPricesInclusiveOfVAT
					? "inc vat" // Vat inclusive
                    : "Excluding VAT"; // Vat exclusive

				priceDeltaDisplay = string.Format("{0} {1}", priceDeltaDisplay, vatSuffix);
			}

			return new KitUploadViewModel
			{
				ProductId = itemInfo.ProductId,
				GroupId = itemInfo.GroupId,
				ItemId = itemId,
				Item = new KitItemViewModel
				{
					Name = item.Name,
					NameDisplay = item.PriceDelta > 0
								? string.Format("{0} [{1} {2}]", item.Name, "Add", priceDeltaDisplay)
								: item.Name,
					Description = item.Description,
					IsDefault = item.IsDefault,
					PriceDelta = item.PriceDelta,
					PriceDeltaDisplay = priceDeltaDisplay,
					WeightDelta = item.WeightDelta,
					DisplayOrder = item.DisplayOrder,
					Id = item.Id,
					IsSelected = item.IsSelected,
					TextOption = item.TextOption,
				},
				TemporaryFileStub = stub
			};
		}

		KitItemInfo GetKitItemInfo(int itemId, string stub)
		{
			int productId = 0;
			int groupId = 0;
			using(var connection = new SqlConnection(DB.GetDBConn()))
			using(var command = new SqlCommand())
			{
				command.Connection = connection;
				command.CommandText = @"select kg.ProductID, kg.KitGroupID from KitItem ki
										left join KitGroup kg on kg.KitGroupID = ki.KitGroupID
										where KitItemId = @KitItemId";

				command.Parameters.AddWithValue("KitItemId", itemId);
				connection.Open();
				using(var reader = command.ExecuteReader())
				{
					while(reader.Read())
					{
						productId = reader.FieldInt("ProductID");
						groupId = reader.FieldInt("kitgroupID");
					}
				}
			}

			var kitData = KitProductData.Find(productId, Customer.Current);

			if(kitData == null)
				return null;

			// Put the temporary file stub on the kit data object so that later it can be saved with the right filename.
			kitData.TempFileStub = stub;

			var kitGroup = kitData.GetGroup(groupId);
			if(kitGroup == null)
				return null;

			return new KitItemInfo
			{
				KitItem = kitGroup.GetItem(itemId),
				ProductId = productId,
				GroupId = groupId
			};
		}

		class KitItemInfo
		{
			public KitItemData KitItem
			{ get; set; }

			public int ProductId
			{ get; set; }

			public int GroupId
			{ get; set; }
		}
	}
}
