// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Web.Mvc;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontGateways.Processors
{
	public class AmazonPaymentsGateway
	{
		const string OffsiteSourceIdentifier = AppLogic.ro_PMAmazonPayments;
		readonly AmazonPaymentsApiProvider ApiProvider;

		public AmazonPaymentsGateway()
		{
			ApiProvider = DependencyResolver.Current.GetService<AmazonPaymentsApiProvider>();
		}

		public string AuthorizeOrder(
			ref AmazonPaymentsOrderTrackingDetail orderTrackingDetail,
			int orderNumber, int customerId, decimal orderTotal, bool useLiveTransactions, bool authAndCapture)
		{
			// validate incoming id
			if(string.IsNullOrEmpty(orderTrackingDetail.OrderReference.Id))
				return "Amazon order reference id could not be found. Order was not created.";

			try
			{
				// step 1: add further details to order reference object
				var setOrderDetailsResponse = ApiProvider.SetOrderDetails(orderTrackingDetail.OrderReference.Id, orderNumber, orderTotal);
				if(setOrderDetailsResponse == null || !setOrderDetailsResponse.IsSetSetOrderReferenceDetailsResult())
					return "Error completing order details. Order was not created.";

				// step 2: confirm purchase
				var confirmOrderResponse = ApiProvider.ConfirmOrder(orderTrackingDetail.OrderReference.Id);
				if(confirmOrderResponse == null)
					return "Error confirming order. Order was not created.";

				// step 3: request authorization
				//		authorization states:
				//			Pending: only returned on asynch calls (when TransactionTimeout is a non-zero, integer value [in minutes; increments of 5 only]) (NOT CURRENTLY UTILIZED)
				//			Open: payment method has been successfully authorized
				//			Declined: authorization has been declined
				//			Closed:
				//				: authorization remained in Open state for more than 30 days (production) or 2 days (sandbox); closed by Amazon
				//				-or- : capture has been completed for the order
				//				-or- : order reference object has been marked as Cancelled by us (Void)
				var authorizeResponse = ApiProvider.Authorize(orderTrackingDetail.OrderReference.Id, orderTotal, orderNumber);
				if(authorizeResponse == null ||
					!authorizeResponse.IsSetAuthorizeResult() ||
					!authorizeResponse.AuthorizeResult.IsSetAuthorizationDetails())
					return "Error authorizing order. Order was not created.";

				// step 4: get authorization details
				var authorizationDetails = authorizeResponse
					.AuthorizeResult
					.AuthorizationDetails;

				// step 5: make sure payment method was authorized successfully
				if(authorizationDetails.AuthorizationStatus.State == OffAmazonPaymentsService.Model.PaymentStatus.DECLINED) // (selected payment method was declined)
					return "Amazon has declined the selected payment method. Order was not created.";
				else if(authorizationDetails.AuthorizationStatus.State != OffAmazonPaymentsService.Model.PaymentStatus.OPEN) // (some other failure)
					return "Error processing payment. Order was not created.";

				// step 6: record updates / cleanup
				var customer = new Customer(customerId);

				var getOrderDetailResponse = ApiProvider.GetOrderDetails(orderTrackingDetail.OrderReference.Id);
				if(getOrderDetailResponse != null && getOrderDetailResponse.IsSetGetOrderReferenceDetailsResult())
				{
					var shippingAddress = getOrderDetailResponse
						.GetOrderReferenceDetailsResult
						.OrderReferenceDetails
						.Destination
						.PhysicalDestination;

					customer.PrimaryShippingAddress.NickName = shippingAddress.Name ?? string.Empty;
					customer.PrimaryShippingAddress.Address1 = shippingAddress.AddressLine1 ?? string.Empty;
					customer.PrimaryShippingAddress.Address2 = shippingAddress.AddressLine2 ?? string.Empty;
					customer.PrimaryShippingAddress.Phone = shippingAddress.Phone ?? string.Empty;
					customer.PrimaryShippingAddress.UpdateDB();

					orderTrackingDetail.OrderReference.State = getOrderDetailResponse.GetOrderReferenceDetailsResult.OrderReferenceDetails.OrderReferenceStatus.ToString();
				}

				orderTrackingDetail.Authorization.Id = authorizationDetails.AmazonAuthorizationId;
				orderTrackingDetail.Authorization.State = authorizationDetails.AuthorizationStatus.State.ToString();
				orderTrackingDetail.Authorization.ReasonCode = authorizationDetails.AuthorizationStatus.ReasonCode;
				
				if(authAndCapture)
					return CaptureOrder(ref orderTrackingDetail, orderNumber, customerId, orderTotal);

				return AppLogic.ro_OK;
			}
			catch(OffAmazonPaymentsService.OffAmazonPaymentsServiceException amazonServiceException)
			{
				return amazonServiceException.Message;
			}
		}

		public string CancelOrder(
			ref AmazonPaymentsOrderTrackingDetail orderTrackingDetail,
			int orderNumber)
		{
			var closeOrderResponse = ApiProvider.CloseOrder(orderTrackingDetail.OrderReference.Id);
			if(closeOrderResponse == null)
				return "Error closing order. Order was not created.";

			return AppLogic.ro_OK;
		}

		public string CaptureOrder(
			ref AmazonPaymentsOrderTrackingDetail orderTrackingDetail,
			int orderNumber, int customerId, decimal orderTotal)
		{
			// validate incoming id
			if(string.IsNullOrEmpty(orderTrackingDetail.Authorization.Id))
				return "Amazon authorization id could not be found. Order was not created.";

			try
			{
				// step 1: request capture
				//		capture states:
				//			Pending: only returned on asynch calls (when TransactionTimeout is a non-zero, integer value [in minutes; increments of 5 only]) (NOT CURRENTLY UTILIZED)
				//			Completed: payment method has been successfully captured (and charged)
				//			Declined: authorization has been declined
				//			Closed:
				//				: closed by Amazon (per docs) "if Amazon identifies a problem with the buyer's account"
				//				-or- : maximum amt of capture (lesser 15% or $75 above amount of capture [accommodating 'restocking' fees, etc.) has already been refunded
				//				-or- : maximum of 10 partial refunds have been requested
				var captureResponse = ApiProvider.Capture(orderTrackingDetail.Authorization.Id, orderTotal, orderNumber);
				if(captureResponse == null ||
					!captureResponse.IsSetCaptureResult() ||
					!captureResponse.CaptureResult.IsSetCaptureDetails())
					return "Error capturing order. Order was not created.";

				// step 2: get capture details
				var captureDetails = captureResponse
					.CaptureResult
					.CaptureDetails;

				// step 3: make sure payment method was captured successfully
				if(captureDetails.CaptureStatus.State == OffAmazonPaymentsService.Model.PaymentStatus.DECLINED) // (selected payment method was declined)
					return "Amazon has declined the selected payment method. Order was not created.";
				else if(captureDetails.CaptureStatus.State != OffAmazonPaymentsService.Model.PaymentStatus.COMPLETED) // (some other failure)
					return "Error processing payment. Order was not created.";

				// step 4: record updates / cleanup
				var closerOrderResponse = ApiProvider.CloseOrder(orderTrackingDetail.OrderReference.Id);
				if(closerOrderResponse == null)
					return "Error closing order. Order was not created.";

				// this does not work - we will need to add a separate call here to pull AmazonOrderRefence details
				//orderTrackingDetail.OrderReference.State = getOrderDetailResponse.GetOrderReferenceDetailsResult.OrderReferenceDetails.OrderReferenceStatus.ToString();

				orderTrackingDetail.Capture.Id = captureDetails.AmazonCaptureId;
				orderTrackingDetail.Capture.State = captureDetails.CaptureStatus.State.ToString();
				orderTrackingDetail.Capture.ReasonCode = captureDetails.CaptureStatus.ReasonCode;
				
				return AppLogic.ro_OK;
			}
			catch(OffAmazonPaymentsService.OffAmazonPaymentsServiceException amazonServiceException)
			{
				return amazonServiceException.Message;
			}
		}

		public string RefundOrder(
			ref AmazonPaymentsOrderTrackingDetail orderTrackingDetail,
			int orderNumber, decimal refundAmount, string refundReason)
		{
			// validate incoming id
			if(string.IsNullOrEmpty(orderTrackingDetail.Capture.Id))
				return "Amazon capture id could not be found. Order was not captured.";

			try
			{
				// step 1: request refund
				//		refund states:
				//			Pending: DOES NOT follow auth/capture TransactionTimeout (synch/asynch) pattern. We should always, initially, get a Pending status back.
				//				Not entirely sure how to handle this prior to implementing the IPN system. For now we will still "test" for 'DECLINED' state 
				//				(even though we should never get it) and act as though we have gotten a 'COMPLETED' state response in all cases.
				//			Completed: payment method has been successfully refunded (would arrive via IPN once refund request is processed by Amazon)
				//			Declined: refund request has been declined (would arrive via IPN once refund request is processed by Amazon)
				var refundResponse = ApiProvider.Refund(orderTrackingDetail.Capture.Id, orderNumber, refundAmount, refundReason);
				if(refundResponse == null ||
					!refundResponse.IsSetRefundResult() ||
					!refundResponse.RefundResult.IsSetRefundDetails())
					return "Error refunding order. Order was not refunded.";

				// step 2: get refund details
				var refundDetails = refundResponse
					.RefundResult
					.RefundDetails;

				// step 3: make sure payment method was captured successfully
				if(refundDetails.RefundStatus.State == OffAmazonPaymentsService.Model.PaymentStatus.DECLINED) // (refund request was declined)
					return "Amazon has declined the refund request. Order was not refunded.";

				// persist refund results
				orderTrackingDetail.Refund.Id = refundDetails.AmazonRefundId;
				orderTrackingDetail.Refund.State = refundDetails.RefundStatus.State.ToString();
				orderTrackingDetail.Refund.ReasonCode = refundDetails.RefundStatus.ReasonCode;

				return AppLogic.ro_OK;
			}
			catch(OffAmazonPaymentsService.OffAmazonPaymentsServiceException amazonServiceException)
			{
				return amazonServiceException.Message;
			}
		}
	}
}
