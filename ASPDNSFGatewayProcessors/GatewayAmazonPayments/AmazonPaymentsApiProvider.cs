// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Xml.Serialization;
using AspDotNetStorefrontCore;
using OffAmazonPaymentsService;
using OffAmazonPaymentsService.Model;

namespace AspDotNetStorefrontGateways.Processors
{
	public class AmazonPaymentsApiProvider
	{
		public readonly AmazonPaymentsConfiguration Configuration;
		public readonly HttpClient HttpClient;

		public AmazonPaymentsApiProvider(AmazonPaymentsConfiguration configuration, HttpClient httpClient)
		{
			Configuration = configuration;
			HttpClient = httpClient;
		}

		#region order reference

		public SetOrderReferenceDetailsResponse SetOrderDetails(string amazonOrderReferenceId, int orderNumber, decimal orderTotal)
		{
			return
				CreateServiceClient()
				.SetOrderReferenceDetails(
					request: CreateSetOrderDetailRequest(amazonOrderReferenceId, orderNumber, orderTotal));
		}

		SetOrderReferenceDetailsRequest CreateSetOrderDetailRequest(string amazonOrderReferenceId, int orderNumber, decimal orderTotal)
		{
			return new SetOrderReferenceDetailsRequest
			{
				AmazonOrderReferenceId = amazonOrderReferenceId,
				SellerId = Configuration.MerchantId,
				OrderReferenceAttributes = new OrderReferenceAttributes
				{
					OrderTotal = new OrderTotal
					{
						Amount = orderTotal.ToString(),
						CurrencyCode = Configuration.StoreCurrency
					},
					SellerNote = Configuration.SellerNote,
					SellerOrderAttributes = new SellerOrderAttributes
					{
						SellerOrderId = orderNumber.ToString(),
						StoreName = Configuration.StoreName,
					}
				}
			};
		}

		public ConfirmOrderReferenceResponse ConfirmOrder(string amazonOrderReferenceId)
		{
			return
				CreateServiceClient()
				.ConfirmOrderReference(
					request: CreateConfirmOrderRequest(amazonOrderReferenceId));
		}

		ConfirmOrderReferenceRequest CreateConfirmOrderRequest(string amazonOrderReferenceId)
		{
			return new ConfirmOrderReferenceRequest
			{
				AmazonOrderReferenceId = amazonOrderReferenceId,
				SellerId = Configuration.MerchantId
			};
		}

		public GetOrderReferenceDetailsResponse GetOrderDetails(string amazonOrderReferenceId)
		{
			return
				CreateServiceClient()
				.GetOrderReferenceDetails(
					request: CreateGetOrderDetailRequest(amazonOrderReferenceId));
		}

		GetOrderReferenceDetailsRequest CreateGetOrderDetailRequest(string amazonOrderReferenceId)
		{
			return new GetOrderReferenceDetailsRequest
			{
				AmazonOrderReferenceId = amazonOrderReferenceId,
				SellerId = Configuration.MerchantId,
			};
		}

		#endregion

		#region order authorize

		public AuthorizeResponse Authorize(string amazonOrderReferenceId, decimal orderTotal, int orderNumber)
		{
			return
				CreateServiceClient()
				.Authorize(
					request: CreateAuthorizeRequest(amazonOrderReferenceId, orderTotal, orderNumber));
		}

		AuthorizeRequest CreateAuthorizeRequest(string amazonOrderReferenceId, decimal orderTotal, int orderNumber)
		{
			return new AuthorizeRequest
			{
				AmazonOrderReferenceId = amazonOrderReferenceId,
				SellerId = Configuration.MerchantId,
				SellerAuthorizationNote = Configuration.SellerNote,
				AuthorizationReferenceId = CreateAuthorizationReferenceId(orderNumber),
				CaptureNow = false,
				AuthorizationAmount = new Price
				{
					Amount = orderTotal.ToString(),
					CurrencyCode = Configuration.StoreCurrency
				},
				TransactionTimeout = Configuration.TransactionTimeout
			};
		}

		#endregion

		#region order close (void)

		public CloseOrderReferenceResponse CloseOrder(string amazonOrderReferenceId)
		{
			return
				CreateServiceClient()
				.CloseOrderReference(
					request: CreateCloseOrderRequest(amazonOrderReferenceId));
		}

		CloseOrderReferenceRequest CreateCloseOrderRequest(string amazonOrderReferenceId)
		{
			return new CloseOrderReferenceRequest
			{
				AmazonOrderReferenceId = amazonOrderReferenceId,
				SellerId = Configuration.MerchantId
			};
		}

		#endregion

		#region order capture

		public CaptureResponse Capture(string amazonAuthorizationId, decimal orderTotal, int orderNumber)
		{
			return
				CreateServiceClient()
				.Capture(
					request: CreateCaptureRequest(amazonAuthorizationId, orderTotal, orderNumber));
		}

		CaptureRequest CreateCaptureRequest(string amazonAuthorizationId, decimal orderTotal, int orderNumber)
		{
			return new CaptureRequest
			{
				AmazonAuthorizationId = amazonAuthorizationId,
				SellerId = Configuration.MerchantId,
				SellerCaptureNote = Configuration.SellerNote,
				CaptureReferenceId = CreatCaptureReferenceId(orderNumber),
				CaptureAmount = new Price
				{
					Amount = orderTotal.ToString(),
					CurrencyCode = Configuration.StoreCurrency
				}
			};
		}

		#endregion

		#region order refund

		public RefundResponse Refund(string amazonCaptureId, int orderNumber, decimal refundAmount, string refundReason)
		{
			return
				CreateServiceClient()
				.Refund(
					request: CreateRefundRequest(amazonCaptureId, orderNumber, refundAmount, refundReason));
		}

		RefundRequest CreateRefundRequest(string amazonCaptureId, int orderNumber, decimal refundAmount, string refundReason)
		{
			return new RefundRequest
			{
				AmazonCaptureId = amazonCaptureId,
				RefundReferenceId = CreateRefundReferenceId(orderNumber),
				SellerRefundNote = refundReason,
				SellerId = Configuration.MerchantId,
				RefundAmount = new Price
				{
					Amount = refundAmount.ToString(),
					CurrencyCode = Configuration.StoreCurrency
				}
			};
		}

		#endregion

		OffAmazonPaymentsServiceClient CreateServiceClient()
		{
			return new OffAmazonPaymentsServiceClient(
				applicationName: Configuration.ApplicationName,
				applicationVersion: Configuration.ApplicationVersion,
				awsAccessKeyId: Configuration.AccessKey,
				awsSecretAccessKey: Configuration.SecretAccessKey,
				config: new OffAmazonPaymentsServiceConfig
				{
					ServiceURL = Configuration.ServiceUrl
				});
		}

		string CreateAuthorizationReferenceId(int orderNumber)
		{
			return "A" + orderNumber.ToString();
		}

		string CreatCaptureReferenceId(int orderNumber)
		{
			return "C" + orderNumber.ToString();
		}

		string CreateRefundReferenceId(int orderNumber)
		{
			return "R" + orderNumber.ToString();
		}

		public UserProfile GetUserProfile(string accessToken)
		{
			var profileRequest = new HttpRequestMessage(HttpMethod.Get, Configuration.ProfileServiceUrl);
			profileRequest.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
			var response = HttpClient
				.SendAsync(profileRequest)
				.Result;

			if(!response.IsSuccessStatusCode)
				return null;

			return JsonConvert.DeserializeObject<UserProfile>(response.Content.ReadAsStringAsync().Result);
		}
	}

	public class AmazonPaymentsConfiguration
	{
		public readonly string ApplicationName;
		public readonly string ApplicationVersion;
		public readonly string StoreName;
		public readonly string StoreCurrency;
		public readonly uint TransactionTimeout;
		public readonly string MerchantId;
		public readonly string ClientId;
		public readonly string AccessKey;
		public readonly string SecretAccessKey;
		public readonly string SellerNote;
		public readonly string ServiceUrl;
		public readonly string ScriptUrl;
		public readonly string ProfileServiceUrl;

		public AmazonPaymentsConfiguration(string applicationName,
			string applicationVersion,
			string storeName,
			string storeCurrency,
			uint transactionTimeout,
			string merchantId,
			string clientId,
			string accessKey,
			string secretAccessKey,
			string sellerNote,
			string serviceUrl,
			string scriptUrl,
			string profileServiceUrl)
		{
			ApplicationName = applicationName;
			ApplicationVersion = applicationVersion;
			StoreName = storeName;
			StoreCurrency = storeCurrency;
			TransactionTimeout = transactionTimeout;
			MerchantId = merchantId;
			ClientId = clientId;
			AccessKey = accessKey;
			SecretAccessKey = secretAccessKey;
			SellerNote = sellerNote;
			ServiceUrl = serviceUrl;
			ScriptUrl = scriptUrl;
			ProfileServiceUrl = profileServiceUrl;
		}

		public static AmazonPaymentsConfiguration CreateDefaultConfiguration()
		{
			var useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");

			return new AmazonPaymentsConfiguration(
				applicationName: CommonLogic.GetProductName(),
				applicationVersion: CommonLogic.GetVersion(false),
				storeName: Store.GetStoreName(AppLogic.StoreID()),
				storeCurrency: Localization.StoreCurrency(false),
				transactionTimeout: 0,
				merchantId: AppLogic.AppConfig("AmazonPayments.MerchantId"),
				clientId: AppLogic.AppConfig("AmazonPayments.ClientId"),
				accessKey: AppLogic.AppConfig("AmazonPayments.AccessKey"),
				secretAccessKey: AppLogic.AppConfig("AmazonPayments.SecretAccessKey"),
				sellerNote: AppLogic.AppConfig("AmazonPayments.SellerNote"),
				serviceUrl: useLiveTransactions
					? AppLogic.AppConfig("AmazonPayments.LiveServiceUrl")
					: AppLogic.AppConfig("AmazonPayments.SandboxServiceUrl"),
				scriptUrl: useLiveTransactions
					? AppLogic.AppConfig("AmazonPayments.LiveScriptUrl")
					: AppLogic.AppConfig("AmazonPayments.SandboxScriptUrl"),
				profileServiceUrl: useLiveTransactions
					? AppLogic.AppConfig("AmazonPayments.LiveProfileServiceUrl")
					: AppLogic.AppConfig("AmazonPayments.SandboxProfileServiceUrl")
			);
		}
	}

	[Serializable]
	public class AmazonPaymentsOrderPhase
	{
		string id;
		string state;
		string reasonCode;

		public AmazonPaymentsOrderPhase()
		{
			id = string.Empty;
			state = string.Empty;
			reasonCode = string.Empty;
		}

		public string Id
		{
			get { return id; }
			set { id = value ?? string.Empty; }
		}
		public string State
		{
			get { return state; }
			set { state = value ?? string.Empty; }
		}
		public string ReasonCode
		{
			get { return reasonCode; }
			set { reasonCode = value ?? string.Empty; }
		}

	}

	[Serializable]
	public class AmazonPaymentsOrderTrackingDetail
	{
		AmazonPaymentsOrderPhase orderReference;
		AmazonPaymentsOrderPhase authorization;
		AmazonPaymentsOrderPhase cancel;
		AmazonPaymentsOrderPhase capture;
		AmazonPaymentsOrderPhase refund;

		public AmazonPaymentsOrderTrackingDetail()
		{
			orderReference = new AmazonPaymentsOrderPhase();
			authorization = new AmazonPaymentsOrderPhase();
			cancel = new AmazonPaymentsOrderPhase();
			capture = new AmazonPaymentsOrderPhase();
			refund = new AmazonPaymentsOrderPhase();
		}

		public AmazonPaymentsOrderPhase OrderReference
		{
			get { return orderReference; }
			set { orderReference = value; }
		}
		public AmazonPaymentsOrderPhase Authorization
		{
			get { return authorization; }
			set { authorization = value; }
		}
		public AmazonPaymentsOrderPhase Cancel
		{
			get { return cancel; }
			set { cancel = value; }
		}
		public AmazonPaymentsOrderPhase Capture
		{
			get { return capture; }
			set { capture = value; }
		}
		public AmazonPaymentsOrderPhase Refund
		{
			get { return refund; }
			set { refund = value; }
		}
	}

	public class AmazonPaymentsOrderTrackingDetailSerializer
	{

		public AmazonPaymentsOrderTrackingDetailSerializer() { }

		public string SerializeAmazonOrderTrackingDetail(AmazonPaymentsOrderTrackingDetail deserializedOrderDetail)
		{
			var serializer = new XmlSerializer(deserializedOrderDetail.GetType());

			using(var writer = new StringWriter())
			{
				serializer.Serialize(writer, deserializedOrderDetail);
				return writer.ToString();
			}
		}

		public AmazonPaymentsOrderTrackingDetail DeserializeAmazonOrderTrackingDetail(string serializedOrderDetail)
		{
			var serializer = new XmlSerializer(typeof(AmazonPaymentsOrderTrackingDetail));

			using(var reader = new StringReader(serializedOrderDetail))
				return (AmazonPaymentsOrderTrackingDetail)serializer.Deserialize(reader);
		}
	}

	public class UserProfile
	{
		[JsonProperty("user_id")]
		public string UserId
		{ get; set; }

		public string Name
		{ get; set; }

		public string Email
		{ get; set; }
	}
}
