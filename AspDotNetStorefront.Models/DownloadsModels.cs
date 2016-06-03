// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Models
{
	public class DownloadsViewModel
	{
		public readonly bool StreamFiles;
		public readonly IEnumerable<DownloadViewModel> Available;
		public readonly IEnumerable<DownloadViewModel> Pending;
		public readonly IEnumerable<DownloadViewModel> Expired;
		public readonly IEnumerable<DownloadsRelatedProductViewModel> RelatedProducts;

		public DownloadsViewModel(
			bool streamFiles,
			IEnumerable<DownloadViewModel> available, 
			IEnumerable<DownloadViewModel> pending, 
			IEnumerable<DownloadViewModel> expired, 
			IEnumerable<DownloadsRelatedProductViewModel> relatedProducts)
		{
			StreamFiles = streamFiles;
			Available = available;
			Pending = pending;
			Expired = expired;
			RelatedProducts = relatedProducts;
		}
	}

	public class DownloadViewModel
	{
		public readonly int ShoppingCartRecordId;
		public readonly string Name;
		public readonly string Category;
		public readonly DateTime PurchasedOn;
		public readonly DateTime? ExpiresOn;
		public readonly string DownloadLocation;
		public readonly DownloadItem.DownloadItemStatus Status;

		public DownloadViewModel(int shoppingCartRecordId, string name, string category, DateTime purchasedOn, DateTime? expiresOn, string downloadLocation, DownloadItem.DownloadItemStatus status)
		{
			ShoppingCartRecordId = shoppingCartRecordId;
			Name = name;
			Category = category;
			PurchasedOn = purchasedOn;
			ExpiresOn = expiresOn;
			DownloadLocation = downloadLocation;
			Status = status;
		}
	}

	public class DownloadsRelatedProductViewModel
	{
		public readonly int ProductId;
		public readonly string Name;
		public readonly string Url;
		public readonly string ImageUrl;

		public DownloadsRelatedProductViewModel(int productId, string name, string url, string imageUrl)
		{
			ProductId = productId;
			Name = name;
			Url = url;
			ImageUrl = imageUrl;
		}
	}
}
