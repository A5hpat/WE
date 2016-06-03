// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AspDotNetStorefront.Filters;
using AspDotNetStorefront.Models;
using AspDotNetStorefrontCore;
using AspDotNetStorefront.Routing;

namespace AspDotNetStorefront.Controllers
{
	[RequireCustomerRecordFilter]
	[Authorize]
	public class DownloadsController : Controller
	{
		readonly NoticeProvider NoticeProvider;

		readonly bool ShowRelatedProducts;
		readonly bool StreamFiles;
		readonly int InventoryFilterLevel;

		public DownloadsController(NoticeProvider noticeProvider)
		{
			NoticeProvider = noticeProvider;

			ShowRelatedProducts = AppLogic.AppConfigBool("Download.ShowRelatedProducts");
			StreamFiles = AppLogic.AppConfigBool("Download.StreamFile");
			InventoryFilterLevel = AppLogic.AppConfigNativeInt("HideProductsWithLessThanThisInventoryLevel");
		}

		public ActionResult Index()
		{
			var customer = HttpContext.GetCustomer();

			var downloadItems = LoadDownloadItemsForCustomer(customer.CustomerID)
				.ToLookup(download => download.Status);
			
			var relatedProducts = ShowRelatedProducts
				? LoadRelatedProductsForCustomer(customer)
					.Distinct(new MemberEqualityComparer<DownloadsRelatedProductViewModel, int>(download => download.ProductId))
					.OrderBy(o => Guid.NewGuid())
					.Take(4)
				: Enumerable.Empty<DownloadsRelatedProductViewModel>();

			var model = new DownloadsViewModel(
				streamFiles: StreamFiles,
				available: downloadItems[DownloadItem.DownloadItemStatus.Available],
				pending: downloadItems[DownloadItem.DownloadItemStatus.Pending],
				expired: downloadItems[DownloadItem.DownloadItemStatus.Expired],
				relatedProducts: relatedProducts);

			return View(model);
		}

		public ActionResult Download(int shoppingCartRecordId)
		{
			var customer = HttpContext.GetCustomer();

			var customerOwnsDownload = LoadDownloadItemsForCustomer(customer.CustomerID)
				.Where(download => download.ShoppingCartRecordId == shoppingCartRecordId)
				.Any();

			if(!customerOwnsDownload)
				throw new HttpException(403, "Forbidden");

			var downloadItem = new DownloadItem();
			downloadItem.Load(shoppingCartRecordId);

			var filepath = CommonLogic.SafeMapPath("~/" + downloadItem.DownloadLocation);
			var filename = Path.GetFileName(filepath);

			try
			{
				if(!System.IO.File.Exists(filepath))
					throw new FileNotFoundException("Could not download the file because it does not exist", filepath);

				return File(filepath, downloadItem.ContentType);
			}
			catch(Exception exception)
			{
				SysLog.LogException(exception, MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
				NoticeProvider.PushNotice("There was a problem with your download file.  Please contact customer service.", NoticeType.Failure);
				
				return RedirectToAction(ActionNames.Index);
			}
		}

		IEnumerable<DownloadViewModel> LoadDownloadItemsForCustomer(int customerId)
		{
			using(var connection = new SqlConnection(DB.GetDBConn()))
			using(var command = new SqlCommand())
			{
				command.Connection = connection;
				command.CommandText = "SELECT DISTINCT ShoppingCartRecId FROM Orders_ShoppingCart(NOLOCK) WHERE IsDownload = 1 AND CustomerId = @CustomerId";
				command.Parameters.AddWithValue("@CustomerId", customerId);

				connection.Open();
				using(var reader = command.ExecuteReader())
					while(reader.Read())
					{
						var shoppingCartRecordId = DB.RSFieldInt(reader, "ShoppingCartRecId");

						var downloadItem = new DownloadItem();
						downloadItem.Load(shoppingCartRecordId);

						yield return new DownloadViewModel(
							shoppingCartRecordId: downloadItem.ShoppingCartRecordId,
							name: downloadItem.DownloadName,
							category: downloadItem.DownloadCategory,
							purchasedOn: downloadItem.PurchasedOn,
							expiresOn: downloadItem.ExpiresOn != DateTime.MinValue
								? downloadItem.ExpiresOn
								: (DateTime?)null,
							downloadLocation: StreamFiles && !Uri.IsWellFormedUriString(downloadItem.DownloadLocation, UriKind.Absolute)
								? Url.Action(ActionNames.Download, new { shoppingCartRecordId = shoppingCartRecordId })
								: downloadItem.DownloadLocation,
							status: downloadItem.Status);
					}
			}
		}

		IEnumerable<DownloadsRelatedProductViewModel> LoadRelatedProductsForCustomer(Customer customer)
		{
			using(var connection = new SqlConnection(DB.GetDBConn()))
			using(var command = new SqlCommand())
			{
				command.Connection = connection;
				command.CommandText = "select ProductID from Orders_ShoppingCart where IsDownload = 1 and CustomerId = @customerId";
				command.Parameters.AddWithValue("customerId", customer.CustomerID);

				connection.Open();
				using(var reader = command.ExecuteReader())
					while(reader.Read())
					{
						var downloadedProductId = DB.RSFieldInt(reader, "ProductID");
						var relatedProducts = LoadRelatedProductsForProduct(customer, downloadedProductId);

						foreach(var product in relatedProducts)
							yield return product;
					}
			}
		}

		IEnumerable<DownloadsRelatedProductViewModel> LoadRelatedProductsForProduct(Customer customer, int productId)
		{
			using(var connection = new SqlConnection(DB.GetDBConn()))
			using(var command = new SqlCommand())
			{
				command.Connection = connection;
				command.CommandText = "EXEC aspdnsf_GetCustomersRelatedProducts @customerGuid, @productId, @customerLevelId, @inventoryFilterLevel, @affiliateId, @storeId";
				command.Parameters.AddWithValue("customerGuid", customer.CustomerGUID);
				command.Parameters.AddWithValue("productId", productId);
				command.Parameters.AddWithValue("customerLevelId", customer.CustomerLevelID);
				command.Parameters.AddWithValue("inventoryFilterLevel", InventoryFilterLevel);
				command.Parameters.AddWithValue("affiliateId", customer.AffiliateID);
				command.Parameters.AddWithValue("storeId", customer.StoreID);

				connection.Open();
				using(var reader = command.ExecuteReader())
					while(reader.Read())
					{
						var loadedProductId = DB.RSFieldInt(reader, "ProductID");
						var loadedProduct = new Product(loadedProductId);
						yield return new DownloadsRelatedProductViewModel(
							productId: loadedProduct.ProductID,
							name: loadedProduct.Name,
							url: Url.BuildProductLink(loadedProduct.ProductID, loadedProduct.SEName),
							imageUrl: AppLogic.LookupImage("Product", loadedProduct.ProductID, "icon", customer.SkinID, customer.LocaleSetting));
					}
			}
		}
	}
}
