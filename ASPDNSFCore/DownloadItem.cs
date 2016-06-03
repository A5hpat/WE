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
using System.IO;

namespace AspDotNetStorefrontCore
{

	/// <summary>
	/// Download item
	/// </summary>
	public class DownloadItem
	{
		public enum DownloadItemStatus
		{
			Pending,
			Available,
			Expired
		}
		public int ShoppingCartRecordId { get; private set; }
		public int OrderNumber { get; private set; }
		public int CustomerId { get; private set; }
		public string DownloadName { get; private set; }
		public string DownloadLocation { get; private set; }
		public string DownloadCategory { get; private set; }
		public DownloadItemStatus Status { get; private set; }
		public DateTime PurchasedOn { get; private set; }
		public DateTime ReleasedOn { get; private set; }
		public DateTime ExpiresOn { get; private set; }
		public int ValidDays { get; private set; }
		public string ContentType { get; private set; }

		public void Load(int shoppingCartRecordId)
		{
			var sqlParams = new List<SqlParameter>();
			sqlParams.Add(DB.CreateSQLParameter("@ShoppingCartRecID", SqlDbType.Int, 4, shoppingCartRecordId, ParameterDirection.Input));

			using (var dbconn = DB.dbConn())
			{
				dbconn.Open();
				using (var dr = DB.GetRS("SELECT osc.OrderNumber, osc.CustomerId, osc.OrderedProductVariantName, osc.OrderedProductName, osc.DownloadStatus, osc.DownloadLocation, osc.DownloadValidDays, osc.DownloadCategory, osc.DownloadReleasedOn, osc.CreatedOn FROM Orders_ShoppingCart osc(NOLOCK) LEFT JOIN Orders o(NOLOCK) ON osc.OrderNumber = o.OrderNumber WHERE o.TransactionState NOT IN('REFUNDED', 'VOIDED') AND ShoppingCartRecID = @ShoppingCartRecID", sqlParams.ToArray(), dbconn))
				{
					if (dr.Read())
					{
						ShoppingCartRecordId = shoppingCartRecordId;
						OrderNumber = DB.RSFieldInt(dr, "OrderNumber");
						CustomerId = DB.RSFieldInt(dr, "CustomerId");
                        DownloadName = DB.RSFieldByLocale(dr, "OrderedProductVariantName", Localization.GetDefaultLocale()).Length > 0 ? string.Format("{0} - {1}", DB.RSFieldByLocale(dr, "OrderedProductName", Localization.GetDefaultLocale()), DB.RSFieldByLocale(dr, "OrderedProductVariantName", Localization.GetDefaultLocale())) : DB.RSFieldByLocale(dr, "OrderedProductName", Localization.GetDefaultLocale());
						DownloadLocation = DB.RSField(dr, "DownloadLocation") ?? string.Empty;
						DownloadCategory = DB.RSField(dr, "DownloadCategory") ?? string.Empty;
						Status = (DownloadItemStatus)DB.RSFieldInt(dr, "DownloadStatus");
						ValidDays = DB.RSFieldInt(dr, "DownloadValidDays");
						PurchasedOn = DB.RSFieldDateTime(dr, "CreatedOn");
						ReleasedOn = DB.RSFieldDateTime(dr, "DownloadReleasedOn");

						if (Status != DownloadItemStatus.Pending && ValidDays > 0)
						{
							ExpiresOn = ReleasedOn.AddDays(ValidDays);

							if (DateTime.Now > ReleasedOn.AddDays(ValidDays))
								Status = DownloadItemStatus.Expired;
						}
						ContentType = DownloadLocation.Length > 0 ? GetMimeType(Path.GetExtension(DownloadLocation)) : string.Empty;

					}
				}
			}
		}

		public void Create(int orderNumber, CartItem c)
		{
			var variant = new ProductVariant(c.VariantID);

			using (var cn = new SqlConnection(DB.GetDBConn()))
			{
				cn.Open();
				using (var cmd = new SqlCommand(@"update orders_ShoppingCart set 								
									DownloadCategory=@DownloadCategory, 
									DownloadValidDays=@DownloadValidDays,
									DownloadLocation=@DownloadLocation,
									DownloadStatus=@DownloadStatus
									where OrderNumber=@OrderNumber and ShoppingCartRecID=@ShoppingCartRecID", cn))
				{
					cmd.Parameters.Add(new SqlParameter("@DownloadCategory", SqlDbType.NText));
					cmd.Parameters.Add(new SqlParameter("@DownloadValidDays", SqlDbType.Int));
					cmd.Parameters.Add(new SqlParameter("@DownloadLocation", SqlDbType.NText));
					cmd.Parameters.Add(new SqlParameter("@DownloadStatus", SqlDbType.Int));
					cmd.Parameters.Add(new SqlParameter("@OrderNumber", SqlDbType.Int));
					cmd.Parameters.Add(new SqlParameter("@ShoppingCartRecID", SqlDbType.Int));

					cmd.Parameters["@DownloadCategory"].Value = AppLogic.GetFirstProductEntity(AppLogic.LookupHelper("Category", 0), c.ProductID, false, Localization.GetDefaultLocale());
					cmd.Parameters["@DownloadValidDays"].Value = variant.DownloadValidDays;
					cmd.Parameters["@DownloadLocation"].Value = variant.DownloadLocation;
					cmd.Parameters["@DownloadStatus"].Value = (int)DownloadItemStatus.Pending;
					cmd.Parameters["@OrderNumber"].Value = orderNumber;
					cmd.Parameters["@ShoppingCartRecID"].Value = c.ShoppingCartRecordID;

					cmd.ExecuteNonQuery();
				}
			}
		}

		public void Release(bool releaseMaxMindDelay)
		{
			Customer customer = new Customer(this.CustomerId);

			if (this.DownloadLocation == null || this.DownloadLocation.Length == 0)
			{
				string emailSubject = string.Format("{0 } - Download Delayed: Download Location is Empty.", AppLogic.AppConfig("StoreName"));
				string emailBody = string.Format("Download \"{ 0}\" for OrderNumber {1} and CustomerID {2} was not released to the customer due to there not being a download location specified.  It is in the pending state and can be manually released to the customer via the Orders page in the AspDotNetStorefront Admin once you have assigned a Download Location.  ", this.DownloadName, this.OrderNumber, this.CustomerId);

				NotifyAdminDelayedDownload(customer, emailSubject, emailBody);

				return;
			}

			string finalDownloadLocation = this.DownloadLocation;

			if (AppLogic.AppConfigBool("MaxMind.Enabled"))
			{
				Order order = new Order(this.OrderNumber);
				if (!releaseMaxMindDelay && order.MaxMindFraudScore >= AppLogic.AppConfigNativeDecimal("MaxMind.DelayDownloadThreshold"))
				{
					string emailSubject = string.Format("{0 } - Download Delayed: Exceeded MaxMind Fraud Score Threshold.", AppLogic.AppConfig("StoreName"));
					string emailBody = string.Format("Download \"{ 0}\" for OrderNumber {1} and CustomerID {2} was not released to the customer due to exceeding your MaxMind score threshold for download items.  It is in the pending state and can be manually released to the customer via the Orders page in the AspDotNetStorefront Admin.  ", this.DownloadName, this.OrderNumber, this.CustomerId);

					NotifyAdminDelayedDownload(customer, emailSubject, emailBody);
					return;
				}
			}

			if (AppLogic.AppConfigBool("Download.CopyFileForEachOrder") && !this.DownloadLocation.Contains("http:") && !this.DownloadLocation.Contains("https:"))
			{
				try
				{
					var downloadPath = CommonLogic.SafeMapPath(this.DownloadLocation);
					var filename = Path.GetFileName(downloadPath);
					var orderDownloadLocation = string.Format("~/orderdownloads/{0}_{1}", this.OrderNumber, this.CustomerId);
					var orderDownloadDirectory = CommonLogic.SafeMapPath(orderDownloadLocation);

					if (!Directory.Exists(orderDownloadDirectory))
						Directory.CreateDirectory(orderDownloadDirectory);

					var orderDownloadPath = string.Format("{0}/{1}", orderDownloadDirectory, filename);

					File.Copy(downloadPath, orderDownloadPath, true);

					finalDownloadLocation = string.Format("{0}/{1}", orderDownloadLocation, filename);
				}
				catch (Exception ex)
				{
					SysLog.LogException(ex, MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
					return;
				}
			}
			using (var cn = new SqlConnection(DB.GetDBConn()))
			{
				cn.Open();
				using (var cmd = new SqlCommand(@"update orders_ShoppingCart set 								
									DownloadReleasedOn=@DownloadReleasedOn,
									DownloadStatus=@DownloadStatus,
									DownloadLocation=@DownloadLocation
									where ShoppingCartRecID=@ShoppingCartRecID", cn))
				{
					cmd.Parameters.Add(new SqlParameter("@DownloadReleasedOn", SqlDbType.DateTime));
					cmd.Parameters.Add(new SqlParameter("@DownloadStatus", SqlDbType.Int));
					cmd.Parameters.Add(new SqlParameter("@DownloadLocation", SqlDbType.NText));
					cmd.Parameters.Add(new SqlParameter("@ShoppingCartRecID", SqlDbType.Int));

					cmd.Parameters["@DownloadReleasedOn"].Value = DateTime.Now;
					cmd.Parameters["@DownloadStatus"].Value = (int)DownloadItemStatus.Available;
					cmd.Parameters["@DownloadLocation"].Value = finalDownloadLocation;
					cmd.Parameters["@ShoppingCartRecID"].Value = this.ShoppingCartRecordId;

					cmd.ExecuteNonQuery();
				}
			}
		}

		public void SendDownloadEmailNotification(bool ignoreMaxMindDelay)
		{
			var order = new Order(OrderNumber);
			var maxMindEnabled = AppLogic.AppConfigBool("MaxMind.Enabled");
			var maxMindThresholdMet = order.MaxMindFraudScore >= AppLogic.AppConfigNativeDecimal("MaxMind.DelayDownloadThreshold");

			if(ignoreMaxMindDelay || (maxMindEnabled && maxMindThresholdMet))
			{
				var orderStoreId = Order.GetOrderStoreID(order.OrderNumber);

				var customer = new Customer(CustomerId);

				var subject = string.Format(
					AppLogic.GetString(
						key: "notification.downloadreleased.1", 
						storeId: orderStoreId),
					AppLogic.AppConfig(
						name: "StoreName", 
						storeId: orderStoreId, 
						cascadeToDefault: true));

				var result = AppLogic.RunXmlPackage(
					"notification.downloadreleased.xml.config",
					null,
					customer,
					customer.SkinID,
					string.Empty,
					string.Format("ShoppingCartRecID={0}&OrderStoreID={1}", ShoppingCartRecordId, orderStoreId),
					false,
					true);

				AppLogic.SendMail(
					subject: subject,
					body: result,
					useHtml: true,
					fromAddress: AppLogic.AppConfig("GotOrderEMailFrom"),
					fromName: AppLogic.AppConfig("GotOrderEMailFromName"),
					toAddress: customer.EMail,
					toName: customer.FullName(),
					bccAddresses: string.Empty,
					server: AppLogic.MailServer());
			}
		}

		public void NotifyAdminDelayedDownload(Customer customer, string emailSubject, string emailBody)
		{
			if (!AppLogic.AppConfigBool("TurnOffStoreAdminEMailNotifications"))
			{
				var SendToList = AppLogic.AppConfig("GotOrderEMailTo").Replace(",", ";");
				if (SendToList.IndexOf(';') != -1)
				{
					foreach (var s in SendToList.Split(';'))
					{
						AppLogic.SendMail(subject: emailSubject,
							body: emailBody + AppLogic.AppConfig("MailFooter"),
							useHtml: true,
							fromAddress: AppLogic.AppConfig("GotOrderEMailFrom"),
							fromName: AppLogic.AppConfig("GotOrderEMailFromName"),
							toAddress: s.Trim(),
							toName: s.Trim(),
							bccAddresses: string.Empty,
							server: AppLogic.MailServer());
					}
				}
				else
				{
					AppLogic.SendMail(subject: emailSubject,
						body: emailBody + AppLogic.AppConfig("MailFooter"),
						useHtml: true,
						fromAddress: AppLogic.AppConfig("GotOrderEMailFrom"),
						fromName: AppLogic.AppConfig("GotOrderEMailFromName"),
						toAddress: SendToList.Trim(),
						toName: SendToList.Trim(),
						bccAddresses: string.Empty,
						server: AppLogic.MailServer());
				}
			}
			else
			{
				SysLog.LogMessage(emailSubject, emailBody, MessageTypeEnum.Informational, MessageSeverityEnum.Alert);
			}
		}

		public void UpdateDownloadLocation(string downloadLocation)
		{
			this.DownloadLocation = downloadLocation;
			using (var cn = new SqlConnection(DB.GetDBConn()))
			{
				cn.Open();
				using (var cmd = new SqlCommand(@"update orders_ShoppingCart set 								
									DownloadLocation=@DownloadLocation
									where ShoppingCartRecID=@ShoppingCartRecID", cn))
				{
					cmd.Parameters.Add(new SqlParameter("@DownloadLocation", SqlDbType.NText));
					cmd.Parameters.Add(new SqlParameter("@ShoppingCartRecID", SqlDbType.Int));

					cmd.Parameters["@DownloadLocation"].Value = downloadLocation;
					cmd.Parameters["@ShoppingCartRecID"].Value = this.ShoppingCartRecordId;

					cmd.ExecuteNonQuery();
				}
			}
		}

		private string GetMimeType(string fileExtension)
		{
			Dictionary<string, string> mimeTypeMappings = new Dictionary<string, string>();
			mimeTypeMappings.Add(".7z", "application/x-7z-compressed");
			mimeTypeMappings.Add(".avi", "video/x-msvideo");
			mimeTypeMappings.Add(".bmp", "image/bmp");
			mimeTypeMappings.Add(".css", "text/css");
			mimeTypeMappings.Add(".csv", "text/csv");
			mimeTypeMappings.Add(".doc", "application/msword");
			mimeTypeMappings.Add(".docm", "application/vnd.ms-word.document.macroEnabled.12");
			mimeTypeMappings.Add(".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
			mimeTypeMappings.Add(".exe", "application/octet-stream");
			mimeTypeMappings.Add(".flv", "video/x-flv");
			mimeTypeMappings.Add(".gif", "image/gif");
			mimeTypeMappings.Add(".htm", "text/html");
			mimeTypeMappings.Add(".html", "text/html");
			mimeTypeMappings.Add(".ico", "image/x-icon");
			mimeTypeMappings.Add(".jpe", "image/jpeg");
			mimeTypeMappings.Add(".jpeg", "image/jpeg");
			mimeTypeMappings.Add(".jpg", "image/jpeg");
			mimeTypeMappings.Add(".js", "application/x-javascript");
			mimeTypeMappings.Add(".mfp", "application/x-shockwave-flash");
			mimeTypeMappings.Add(".mid", "audio/mid");
			mimeTypeMappings.Add(".midi", "audio/mid");
			mimeTypeMappings.Add(".mod", "video/mpeg");
			mimeTypeMappings.Add(".mov", "video/quicktime");
			mimeTypeMappings.Add(".movie", "video/x-sgi-movie");
			mimeTypeMappings.Add(".mp2", "video/mpeg");
			mimeTypeMappings.Add(".mp2v", "video/mpeg");
			mimeTypeMappings.Add(".mp3", "audio/mpeg");
			mimeTypeMappings.Add(".mp4", "video/mp4");
			mimeTypeMappings.Add(".mp4v", "video/mp4");
			mimeTypeMappings.Add(".mpa", "video/mpeg");
			mimeTypeMappings.Add(".mpe", "video/mpeg");
			mimeTypeMappings.Add(".mpeg", "video/mpeg");
			mimeTypeMappings.Add(".mpf", "application/vnd.ms-mediapackage");
			mimeTypeMappings.Add(".mpg", "video/mpeg");
			mimeTypeMappings.Add(".mpv2", "video/mpeg");
			mimeTypeMappings.Add(".pdf", "application/pdf");
			mimeTypeMappings.Add(".pic", "image/pict");
			mimeTypeMappings.Add(".pict", "image/pict");
			mimeTypeMappings.Add(".png", "image/png");
			mimeTypeMappings.Add(".pnz", "image/png");
			mimeTypeMappings.Add(".pps", "application/vnd.ms-powerpoint");
			mimeTypeMappings.Add(".ppsm", "application/vnd.ms-powerpoint.slideshow.macroEnabled.12");
			mimeTypeMappings.Add(".ppsx", "application/vnd.openxmlformats-officedocument.presentationml.slideshow");
			mimeTypeMappings.Add(".ppt", "application/vnd.ms-powerpoint");
			mimeTypeMappings.Add(".pptm", "application/vnd.ms-powerpoint.presentation.macroEnabled.12");
			mimeTypeMappings.Add(".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation");
			mimeTypeMappings.Add(".psd", "application/octet-stream");
			mimeTypeMappings.Add(".pub", "application/x-mspublisher");
			mimeTypeMappings.Add(".qtif", "image/x-quicktime");
			mimeTypeMappings.Add(".rtf", "application/rtf");
			mimeTypeMappings.Add(".swf", "application/x-shockwave-flash");
			mimeTypeMappings.Add(".tif", "image/tiff");
			mimeTypeMappings.Add(".txt", "text/plain");
			mimeTypeMappings.Add(".wav", "audio/wav");
			mimeTypeMappings.Add(".wave", "audio/wav");
			mimeTypeMappings.Add(".zip", "application/x-zip-compressed");

			return mimeTypeMappings.ContainsKey(fileExtension.ToLower()) ? mimeTypeMappings[fileExtension.ToLower()] : "text/plain";
		}

	}
}
