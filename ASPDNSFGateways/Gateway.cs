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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml.Serialization;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways.Processors;

namespace AspDotNetStorefrontGateways
{
	/// <summary>
	/// Summary description for Gateway.
	/// </summary>
	public partial class Gateway
	{
		//these gateways have custom logic surrounding them and cannot be removed and fully abstracted
		public const string ro_GWPAYPALPRO = "PAYPALPRO";
		public const string ro_GWPAYPAL = "PAYPAL";
		public const string ro_GWPAYFLOWPRO = "PAYFLOWPRO";
		public const string ro_GWMICROPAY = "MICROPAY";
		public const string ro_GWAMAZONPAYMENTS = "AMAZONPAYMENTS";
		public const string ro_GWPAYPALEMBEDDEDCHECKOUT = AppLogic.ro_PMPayPalEmbeddedCheckout;
		public const string ro_GWTWOCHECKOUT = "TWOCHECKOUT";
		public const string ro_GWBRAINTREE = "BRAINTREE";

		//Used for determining how the customer chose to pay through Braintree
		public const string BraintreeCreditCardKey = "CreditCard";

		public Gateway() { }

		private static String DetermineGatwayToUse(String GatewayPassedIn)
		{
			String GWCleaned = AppLogic.CleanPaymentGateway(GatewayPassedIn);
			if(GWCleaned.Length == 0)
			{
				GWCleaned = AppLogic.ActivePaymentGatewayCleaned();
			}
			return GWCleaned;
		}

		public static string SerializeMaxMindResponse(MaxMind.MINFRAUD response)
		{
			try
			{
				var serializer = new XmlSerializer(response.GetType());
				using(var stream = new MemoryStream())
				{
					serializer.Serialize(stream, response);
					return Encoding.UTF8.GetString(stream.ToArray());
				}
			}
			catch
			{
				return string.Empty;
			}
		}

		// consult MaxMind documentation on Fraud Score Threshold Semantics. 0.0 = lowest risk. 10.0 = highest risk.
		public static Decimal MaxMindFraudCheck(int orderNumber, Customer thisCustomer, Address useBillingAddress, Address useShippingAddress, decimal orderAmount, string orderCurrency, string paymentMethod, out String fraudDetails)
		{
			fraudDetails = String.Empty;
			try
			{
				var email = (useBillingAddress.EMail ?? String.Empty).Trim();
				if(email == String.Empty)
					email = (thisCustomer.EMail ?? String.Empty).Trim();

				var billingEMailDomain = String.Empty;
				if(email.Contains("@") && !email.EndsWith("@"))
					billingEMailDomain = email.Substring(email.IndexOf("@") + 1);

				string transactionType;
				switch(paymentMethod.ToUpper())
				{
					case "CREDITCARD":
						transactionType = "creditcard";
						break;
					case "PAYPALEXPRESS":
						transactionType = "paypal";
						break;
					default:
						transactionType = "other";
						break;
				}

				string thisIP = thisCustomer.LastIPAddress;
				if(thisIP.Length == 0)
					thisIP = CommonLogic.CustomerIpAddress();

				var wsdl = AppLogic.AppConfig("MaxMind.SOAPURL").Trim();
				var endpointAddress = new System.ServiceModel.EndpointAddress(new Uri(wsdl));
				var binding = new System.ServiceModel.BasicHttpBinding();
				binding.Name = "minfraudWebServiceSoap";

				string cardNumber = String.Empty;
				if(useBillingAddress.CardNumber.Length > 6)
					cardNumber = useBillingAddress.CardNumber.Substring(0, 6);

				var request = new MaxMind.minfraud_soap14RequestBody
				{
					accept_language = thisCustomer.LocaleSetting,
					bin = cardNumber,
					city = useBillingAddress.City,
					country = useBillingAddress.Country,
					custPhone = useBillingAddress.Phone,
					domain = billingEMailDomain,
					emailMD5 = Security.GetMD5Hash(email),
					forwardedIP = CommonLogic.ServerVariables("HTTP_X_FORWARDED_FOR"),
					i = thisIP,
					license_key = AppLogic.AppConfig("MaxMind.LicenseKey"),
					requested_type = AppLogic.AppConfig("MaxMind.ServiceType"),
					order_amount = orderAmount.ToString(),
					order_currency = orderCurrency,
					postal = useBillingAddress.Zip,
					region = useBillingAddress.State,
					sessionID = "aspdnsf", // MaxMind requires this value to identify our cart, do not change
					shipAddr = useShippingAddress.Address1,
					shipCity = useShippingAddress.City,
					shipCountry = useShippingAddress.Country,
					shipPostal = useShippingAddress.Zip,
					shipRegion = useShippingAddress.State,
					txn_type = transactionType,
					txnID = orderNumber.ToString(),
					usernameMD5 = Security.GetMD5Hash(useBillingAddress.CardName.Trim().ToLowerInvariant())
				};

				MaxMind.minfraudWebServiceSoap mmind = new MaxMind.minfraudWebServiceSoapClient(binding, endpointAddress);
				MaxMind.MINFRAUD rsp = mmind.minfraud_soap14(new MaxMind.minfraud_soap14Request(request)).Body.minfraud_output;

				fraudDetails = SerializeMaxMindResponse(rsp);
				return Localization.ParseUSDecimal(rsp.riskScore);
			}
			catch(Exception ex)
			{
				fraudDetails = ex.Message;
			}
			return -1.0M; // don't let maxmind exception stop the order
		}

		// conceptually, marking an order as "cleared" now means setting it's TransactionState to 'CAPTURED'
		// AND processing all related actions for the order (processsing download files, drop ship notifications, gift card setup, etc, etc, etc...
		public static void ProcessOrderAsCaptured(int OrderNumber)
		{
			Order ord = new Order(OrderNumber, Localization.GetDefaultLocale());
			// only process if not already captured! :)
			if(!ord.TransactionIsCaptured())
			{
				// mark payment cleared:
				DB.ExecuteSQL("update orders set CapturedOn=getdate(), TransactionState=" + DB.SQuote(AppLogic.ro_TXStateCaptured) + " where OrderNumber=" + OrderNumber.ToString());

				// make sure inventory was deducted. safe to call repeatedly. proc protects against deducting twice
				DB.ExecuteSQL("aspdnsf_AdjustInventory " + OrderNumber.ToString() + ",-1");

				//Update the Micropay balances if any was purchased
				{
					int MicropayProductID = AppLogic.GetMicroPayProductID();
					int MicropayVariantID = AppLogic.GetProductsDefaultVariantID(MicropayProductID);
					decimal mpTotal = AppLogic.GetMicroPayBalance(ord.CustomerID);

					//Use the raw price for the amount because 
					// it may be discounted or on sale in the order
					decimal amount = AppLogic.GetVariantPrice(MicropayVariantID);
					foreach(CartItem c in ord.CartItems)
					{
						if(c.ProductID == MicropayProductID)
						{
							mpTotal += (amount * c.Quantity);
						}
					}
					DB.ExecuteSQL(String.Format("update Customer set MicroPayBalance={0} where CustomerID={1}", Localization.CurrencyStringForDBWithoutExchangeRate(mpTotal), ord.CustomerID));
				}

				if(ord.HasDownloadComponents(false) && AppLogic.AppConfig("Download.ReleaseOnAction").EqualsIgnoreCase("capture"))
				{
					DownloadItem downloadItem = new DownloadItem();
					foreach(CartItem c in ord.CartItems.Where(w => w.IsDownload))
					{
						downloadItem.Load(c.ShoppingCartRecordID);
						downloadItem.Release(false);
						downloadItem.SendDownloadEmailNotification(false);
					}

					if(ord.IsAllDownloadComponents())
					{
						SqlParameter[] updOrderParams = new SqlParameter[] { new SqlParameter("@OrderNumber", ord.OrderNumber),
																				new SqlParameter("@IsNew", "0"),
																				new SqlParameter("@ShippedVia", "DOWNLOAD") };

						DB.ExecuteSQL("UPDATE Orders SET IsNew = @IsNew, DownloadEMailSentOn = getdate(), ShippedOn = getdate(), ShippedVIA = @ShippedVia WHERE OrderNumber = @OrderNumber", updOrderParams);
					}
				}

				if(!AppLogic.AppConfigBool("DelayedDropShipNotifications") && ord.HasDistributorComponents())
				{
					// card authorized, ok to send distributor drop-ship e-mail notifications:
					bool delayDropShipNotification = AppLogic.AppConfigBool("DelayedDropShipNotifications");
					if(delayDropShipNotification || (AppLogic.AppConfigBool("MaxMind.Enabled") && ord.MaxMindFraudScore >= AppLogic.AppConfigNativeDecimal("MaxMind.DelayDropShipThreshold")))
					{
						delayDropShipNotification = true; // delay it anyway if maxmind fraud score is too high!
					}
					if(!delayDropShipNotification && ord.TransactionIsCaptured() && ord.DistributorEMailSentOn.Equals(System.DateTime.MinValue) && ord.HasDistributorComponents())
					{
						AppLogic.SendDistributorNotifications(new Order(ord.OrderNumber, ord.ViewInLocaleSetting)); // must reload order object to make this call work!
					}
				}

				//Serialize Gift Cards
				if(ord.ContainsGiftCard())
				{
					// find and process the GiftCards
					string gcSql = "select GiftCardID from GiftCard gc join Orders_Shoppingcart os on gc.ShoppingCartRecID = os.ShoppingCartRecID and os.OrderNumber = " + ord.OrderNumber.ToString() + " and os.CustomerID=" + ord.CustomerID.ToString() + " where gc.GiftCardTypeID <> 100 ";

					using(SqlConnection con = new SqlConnection(DB.GetDBConn()))
					{
						con.Open();
						using(IDataReader rs = DB.GetRS(gcSql, con))
						{
							while(rs.Read())
							{
								GiftCard card = new GiftCard(DB.RSFieldInt(rs, "GiftCardID"));
								card.SerializeGiftCard();
								card.UpdateCard(null, OrderNumber, null, null, null, null, null, null, null, null, null, null, null, null, null);
								card.RefreshCard();
							}
						}
					}
				}

				//Send Email Gift Card Email
				GiftCards gs = new GiftCards(ord.OrderNumber, GiftCardCollectionFilterType.OrderNumber);
				foreach(GiftCard g in gs)
				{
					g.SendGiftCardEmail();
				}
				try
				{
					// Added try/catch because it blew up the cart if jurisdiction could not be determined but did not prevent the order
					// call-out commit tax transaction add-ins
					if(AppLogic.AppConfigBool("AvalaraTax.Enabled"))
					{
						AvaTax avaTax = new AvaTax();
						avaTax.CommitTax(ord);
					}
				}
				catch(Exception Ex)
				{
					SysLog.LogException(Ex, MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
				}
			}
		}

		// NOTE: this does NOT execute any monetary transaction, it just sets the transactionstate to indicated refunded!
		public static String ForceRefundStatus(int OrderNumber)
		{
			using(SqlConnection con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(IDataReader rs = DB.GetRS(String.Format("Select CapturedOn from Orders  with (NOLOCK) where OrderNumber = {0}", OrderNumber), con))
				{
					if(rs.Read())
					{
						if(DB.RSFieldDateTime(rs, "CapturedOn") != System.DateTime.MinValue)
						{
							// make sure inventory was restored. safe to call repeatedly. proc protects against deducting twice
							DB.ExecuteSQL("aspdnsf_AdjustInventory " + OrderNumber.ToString() + ",1");
						}
					}
				}
			}

			// update transactionstate
			DB.ExecuteSQL("update Orders set RefundTXCommand=" + DB.SQuote("ADMIN FORCED REFUND") + ", RefundReason=" + DB.SQuote("ADMIN FORCED REFUND") + ", TransactionState=" + DB.SQuote(AppLogic.ro_TXStateRefunded) + ", RefundedOn=getdate(), IsNew=0 where OrderNumber=" + OrderNumber.ToString());
			return AppLogic.ro_OK;
		}

		/// <summary>
		/// Marks a transaction state as Voided, but does not communicate with a gateway
		/// </summary>
		/// <param name="OrderNumber">Order Number of the order to be voided</param>
		/// <returns>Returns string OK</returns>
		public static string ForceVoidStatus(int OrderNumber)
		{
			return ForceVoidStatus(OrderNumber, AppLogic.ro_TXStateVoided);
		}

		public static string ForceVoidStatus(int OrderNumber, string status)
		{

			int CouponType = 0;
			string CouponCode = "";
			decimal CouponDiscountAmount = 0.0M;

			using(SqlConnection con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(IDataReader rs = DB.GetRS("Select CouponType, CouponCode, CouponDiscountAmount from Orders  with (NOLOCK)  where OrderNumber=" + OrderNumber.ToString(), con))
				{
					if(rs.Read())
					{
						CouponType = DB.RSFieldInt(rs, "CouponType");
						CouponCode = DB.RSField(rs, "CouponCode");
						CouponDiscountAmount = DB.RSFieldDecimal(rs, "CouponDiscountAmount");
					}
				}
			}

			// make sure inventory was restored. safe to call repeatedly. proc protects against deducting twice
			DB.ExecuteSQL("aspdnsf_AdjustInventory " + OrderNumber.ToString() + ",1");

			DB.ExecuteSQL("update Orders set VoidTXCommand='ADMIN FORCED VOID', TransactionState=" + DB.SQuote(status) + ", VoidedOn=getdate(), IsNew=0 where OrderNumber=" + OrderNumber.ToString());

			DecrementMicropayProductsInOrder(OrderNumber);

			//Invalidate GiftCards ordered on this order
			GiftCards GCs = new GiftCards(OrderNumber, GiftCardCollectionFilterType.OrderNumber);
			foreach(GiftCard gc in GCs)
			{
				gc.GiftCardTransactions.Add(GiftCardUsageTransaction.CreateTransaction(gc.GiftCardID, GiftCardUsageReasons.FundsRemovedByAdmin, 0, 0, gc.Balance, ""));
				gc.UpdateCard(null, null, null, null, 1, null, null, null, null, null, null, null, null, null, null);
			}

			//Restore Amount to coupon used in paying for the order
			if((CouponTypeEnum)CouponType == CouponTypeEnum.GiftCard)
			{
				GiftCard gc = new GiftCard(CouponCode);
				if(gc.GiftCardID != 0)
				{
					gc.GiftCardTransactions.Add(GiftCardUsageTransaction.CreateTransaction(gc.GiftCardID, GiftCardUsageReasons.FundsAddedByAdmin, 0, 0, CouponDiscountAmount, ""));
				}
			}

			return AppLogic.ro_OK;
		}

		public static int CreateOrderRecord(ShoppingCart cart, int OrderNumber, Address UseBillingAddress)
		{
			StringBuilder sql = new StringBuilder(4096);
			String orderGUID = CommonLogic.GetNewGUID();

			if(OrderNumber == 0)
			{
				OrderNumber = AppLogic.GetNextOrderNumber();
			}

			using(SqlConnection con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(IDataReader rsCustomer = DB.GetRS("select * from customer  with (NOLOCK)  where customerid=" + cart.ThisCustomer.CustomerID.ToString(), con))
				{
					rsCustomer.Read();

					String PMCleaned = AppLogic.CleanPaymentMethod(UseBillingAddress.PaymentMethodLastUsed);


					Decimal CartTotal = cart.Total(true);
					Decimal dShippingTotal = cart.ShippingTotal(true, true);
					Decimal dSubTotal = cart.SubTotal(true, false, true, true);
					Decimal dTaxTotal = cart.TaxTotal();
					Decimal NetTotal = CartTotal - CommonLogic.IIF(cart.Coupon.CouponType == CouponTypeEnum.GiftCard, CommonLogic.IIF(CartTotal < cart.Coupon.DiscountAmount, CartTotal, cart.Coupon.DiscountAmount), 0);

					if(NetTotal > System.Decimal.Zero || !AppLogic.AppConfigBool("SkipPaymentEntryOnZeroDollarCheckout"))
					{
						AppLogic.ValidatePM(PMCleaned); // prevent PM Hacks!
					}

					// PayPalExpressMark is just for checkout flow, now that we are recording the order, save it as PayPalExpress
					if(PMCleaned == AppLogic.ro_PMPayPalExpressMark)
					{
						PMCleaned = AppLogic.ro_PMPayPalExpress;
						UseBillingAddress.PaymentMethodLastUsed = AppLogic.ro_PMPayPalExpress;
						UseBillingAddress.UpdateDB();
					}

					sql.Append("insert into Orders(OrderNumber,OrderGUID,VATRegistrationID,TransactionType,CartType,LocaleSetting,OrderWeight,TransactionState,PONumber,StoreVersion,CustomerID,Referrer,OrderNotes,FinalizationData,CustomerServiceNotes,OrderOptions,PaymentMethod,LastIPAddress,CustomerGUID,SkinID,LastName,FirstName,EMail,Phone,Notes,RegisterDate,AffiliateID,CouponCode,CouponType,CouponDescription,CouponDiscountAmount,CouponDiscountPercent,CouponIncludesFreeShipping,OKToEMail,Deleted,BillingEqualsShipping,BillingLastName,BillingFirstName,BillingCompany,BillingAddress1,BillingAddress2,BillingSuite,BillingCity,BillingState,BillingZip,BillingCountry,BillingPhone,ShippingLastName,ShippingFirstName,ShippingCompany,ShippingResidenceType,ShippingAddress1,ShippingAddress2,ShippingSuite,ShippingCity,ShippingState,ShippingZip,ShippingCountry,ShippingPhone,ShippingMethodID,ShippingMethod,ShippingCalculationID,RTShipRequest,RTShipResponse,CardType,CardName,Last4,CardExpirationMonth,CardExpirationYear,CardStartDate,CardIssueNumber,OrderSubtotal,OrderTax,OrderShippingCosts,OrderTotal,AuthorizationResult,AuthorizationCode,AuthorizationPNREF,TransactionCommand, StoreID) values (");
					sql.Append(OrderNumber.ToString() + ",");
					sql.Append(DB.SQuote(orderGUID) + ",");
					sql.Append(DB.SQuote(cart.ThisCustomer.VATRegistrationID) + ",");
					sql.Append("1,"); // always 1 here on create, except for ad-hoc refund type orders
					sql.Append(((int)cart.CartType).ToString() + ",");
					sql.Append(DB.SQuote(cart.ThisCustomer.LocaleSetting) + ",");
					sql.Append(Localization.DecimalStringForDB(cart.WeightTotal()) + ",");

					if(PMCleaned == AppLogic.ro_PMCreditCard || PMCleaned == AppLogic.ro_PMPayPalExpress)
					{
						sql.Append(DB.SQuote(AppLogic.TransactionMode()) + ",");
					}
					else
					{
						sql.Append("NULL,");
					}
					sql.Append(DB.SQuote(UseBillingAddress.PONumber) + ",");
					sql.Append(DB.SQuote(CommonLogic.GetVersion()) + ",");
					sql.Append(cart.ThisCustomer.CustomerID.ToString() + ",");
					sql.Append(DB.SQuote(DB.RSField(rsCustomer, "Referrer")) + ",");
					sql.Append(DB.SQuote(cart.OrderNotes) + ",");
					sql.Append(DB.SQuote(cart.FinalizationData) + ",");
					sql.Append(DB.SQuote(CommonLogic.IIF(cart.CartType == CartTypeEnum.RecurringCart, "Recurring Auto-Ship, Sequence #" + ((CartItem)cart.CartItems[0]).RecurringIndex.ToString(), "")) + ",");
					sql.Append(DB.SQuote(cart.GetOptionsList()) + ",");
					sql.Append(DB.SQuote(UseBillingAddress.PaymentMethodLastUsed) + ",");
					sql.Append(DB.SQuote(DB.RSField(rsCustomer, "LastIPAddress")) + ",");
					sql.Append(DB.SQuote(DB.RSFieldGUID(rsCustomer, "CustomerGUID")) + ",");
					sql.Append(cart.SkinID.ToString() + ",");
					sql.Append(DB.SQuote(DB.RSField(rsCustomer, "LastName")) + ",");
					sql.Append(DB.SQuote(DB.RSField(rsCustomer, "FirstName")) + ",");
					sql.Append(DB.SQuote(DB.RSField(rsCustomer, "EMail")) + ",");
					sql.Append(DB.SQuote(DB.RSField(rsCustomer, "Phone")) + ",");
					sql.Append(DB.SQuote(DB.RSField(rsCustomer, "Notes")) + ",");
					sql.Append(DB.DateQuote(Localization.ToDBDateTimeString(DB.RSFieldDateTime(rsCustomer, "RegisterDate"))) + ",");
					sql.Append(cart.ThisCustomer.AffiliateID.ToString() + ",");
					if(cart.Coupon.CouponCode.Length == 0)
					{
						sql.Append("NULL,");
					}
					else
					{
						sql.Append(DB.SQuote(cart.Coupon.CouponCode) + ",");
					}
					sql.Append(((int)cart.Coupon.CouponType).ToString() + ",");
					if(cart.HasCoupon() && cart.CouponIsValid)
					{
						sql.Append(DB.SQuote(cart.Coupon.Description) + ",");
						sql.Append(Localization.DecimalStringForDB(CommonLogic.IIF(cart.Coupon.CouponType == CouponTypeEnum.GiftCard, CommonLogic.IIF(CartTotal < cart.Coupon.DiscountAmount, CartTotal, cart.Coupon.DiscountAmount), cart.Coupon.DiscountAmount)) + ",");
						sql.Append(Localization.DecimalStringForDB(cart.Coupon.DiscountPercent) + ",");
						if(cart.Coupon.DiscountIncludesFreeShipping)
						{
							sql.Append("1,");
						}
						else
						{
							sql.Append("0,");
						}
					}
					else
					{
						sql.Append("NULL,");
						sql.Append("0,");
						sql.Append("0,");
						sql.Append("0,");
					}
					if(DB.RSFieldBool(rsCustomer, "OKToEMail"))
					{
						sql.Append("1,");
					}
					else
					{
						sql.Append("0,");
					}
					sql.Append("0,");
					if(DB.RSFieldBool(rsCustomer, "BillingEqualsShipping"))
					{
						sql.Append("1,");
					}
					else
					{
						sql.Append("0,");
					}

					sql.Append(DB.SQuote(UseBillingAddress.LastName) + ",");
					sql.Append(DB.SQuote(UseBillingAddress.FirstName) + ",");
					sql.Append(DB.SQuote(UseBillingAddress.Company) + ",");
					sql.Append(DB.SQuote(UseBillingAddress.Address1) + ",");
					sql.Append(DB.SQuote(UseBillingAddress.Address2) + ",");
					sql.Append(DB.SQuote(UseBillingAddress.Suite) + ",");
					sql.Append(DB.SQuote(UseBillingAddress.City) + ",");
					sql.Append(DB.SQuote(UseBillingAddress.State) + ",");
					sql.Append(DB.SQuote(UseBillingAddress.Zip) + ",");
					sql.Append(DB.SQuote(UseBillingAddress.Country) + ",");
					sql.Append(DB.SQuote(UseBillingAddress.Phone) + ",");

					if(cart.HasMultipleShippingAddresses())
					{
						sql.Append("NULL,");
						sql.Append("NULL,");
						sql.Append("NULL,");
						sql.Append("0,");
						sql.Append("NULL,");
						sql.Append("NULL,");
						sql.Append("NULL,");
						sql.Append("NULL,");
						sql.Append("NULL,");
						sql.Append("NULL,");
						sql.Append("NULL,");
						sql.Append("NULL,");
						sql.Append("0,");
						sql.Append("NULL,");
					}
					else
					{
						Address ShippingAddress = new Address();
						ShippingAddress.LoadByCustomer(cart.ThisCustomer.CustomerID, cart.ThisCustomer.PrimaryShippingAddressID, AddressTypes.Shipping);
						sql.Append(DB.SQuote(ShippingAddress.LastName) + ",");
						sql.Append(DB.SQuote(ShippingAddress.FirstName) + ",");
						sql.Append(DB.SQuote(ShippingAddress.Company) + ",");
						sql.Append(((int)ShippingAddress.ResidenceType).ToString() + ",");
						sql.Append(DB.SQuote(ShippingAddress.Address1) + ",");
						sql.Append(DB.SQuote(ShippingAddress.Address2) + ",");
						sql.Append(DB.SQuote(ShippingAddress.Suite) + ",");
						sql.Append(DB.SQuote(ShippingAddress.City) + ",");
						sql.Append(DB.SQuote(ShippingAddress.State) + ",");
						sql.Append(DB.SQuote(ShippingAddress.Zip) + ",");
						sql.Append(DB.SQuote(ShippingAddress.Country) + ",");
						sql.Append(DB.SQuote(ShippingAddress.Phone) + ",");
						if(cart.IsAllDownloadComponents())
						{
							sql.Append("0,");
							sql.Append(DB.SQuote("Download") + ",");
						}
						else if(cart.IsAllSystemComponents())
						{
							sql.Append("0,");
							sql.Append(DB.SQuote("System") + ",");
						}
						else
						{
							sql.Append(cart.FirstItem().ShippingMethodID.ToString() + ",");
							sql.Append(DB.SQuote(cart.FirstItem().ShippingMethod) + ",");
						}
					}

					sql.Append(((int)Shipping.GetActiveShippingCalculationID()).ToString() + ",");
					sql.Append(DB.SQuote(DB.RSField(rsCustomer, "RTShipRequest")) + ",");
					sql.Append(DB.SQuote(DB.RSField(rsCustomer, "RTShipResponse")) + ",");

					sql.Append(DB.SQuote(UseBillingAddress.CardType) + ",");
					sql.Append(DB.SQuote(UseBillingAddress.CardName) + ",");

					String Last4 = String.Empty;
					if((PMCleaned == AppLogic.ro_PMCreditCard || PMCleaned == AppLogic.ro_PMPayPalExpress))
					{
						Last4 = AppLogic.SafeDisplayCardNumberLast4(UseBillingAddress.CardNumber, String.Empty, 0);
					}

					sql.Append(DB.SQuote(Last4) + ",");
					sql.Append(DB.SQuote(UseBillingAddress.CardExpirationMonth) + ",");
					sql.Append(DB.SQuote(UseBillingAddress.CardExpirationYear) + ",");
					sql.Append(DB.SQuote(UseBillingAddress.CardStartDate) + ",");
					sql.Append(DB.SQuote(Security.MungeString(UseBillingAddress.CardIssueNumber)) + ",");

					sql.Append(Localization.CurrencyStringForDBWithoutExchangeRate(dSubTotal) + ",");
					sql.Append(Localization.CurrencyStringForDBWithoutExchangeRate(dTaxTotal) + ",");
					sql.Append(Localization.CurrencyStringForDBWithoutExchangeRate(dShippingTotal) + ",");
					sql.Append(Localization.CurrencyStringForDBWithoutExchangeRate(CartTotal) + ",");
					sql.Append(DB.SQuote(AppLogic.ro_TBD) + ","); // must update later to RawResponseString if TX is ok!
					sql.Append(DB.SQuote(AppLogic.ro_TBD) + ",");   // must update later if TX is ok!
					sql.Append(DB.SQuote(AppLogic.ro_TBD) + ",");  // must update later if TX is ok!
					sql.Append(DB.SQuote(AppLogic.ro_TBD) + ",");  // must update later if TX is ok!
					sql.Append(AppLogic.StoreID() + ")");  // Specify a stored id where the order is created.

					DB.ExecuteSQL(sql.ToString());

					// we can now store the cc if required, as we have to get the salt field from the order record just created!
					string saltKey = Order.StaticGetSaltKey(OrderNumber);

					String CC = String.Empty;
					if(PMCleaned == AppLogic.ro_PMCreditCard || PMCleaned == AppLogic.ro_PMPayPalExpress)
					{
						if(cart.ThisCustomer.MasterShouldWeStoreCreditCardInfo)
						{
							CC = Security.MungeString(UseBillingAddress.CardNumber, saltKey);
						}
						else
						{
							CC = AppLogic.ro_CCNotStoredString;
						}
					}

					DB.ExecuteSQL("update Orders set CardNumber=" + DB.SQuote(CC) + " where OrderNumber=" + OrderNumber.ToString());

					PromotionManager.FinalizePromotionsOnOrderComplete(cart, OrderNumber);

					// **** Partial fix for embedded checkout methods ****
					if(PMCleaned == AppLogic.ro_PMPayPalEmbeddedCheckout)
					{
						var checkoutAccountStatusProvider = DependencyResolver.Current.GetService<ICheckoutAccountStatusProvider>();
						var checkoutAccountStatus = checkoutAccountStatusProvider.GetCheckoutAccountStatus(cart.ThisCustomer, cart.ThisCustomer.EMail);
						if(checkoutAccountStatus.State == CheckoutAccountState.Unvalidated)
							DB.ExecuteSQL(
								"update Customer set IsRegistered = 1 where CustomerId = @customerId;",
								new SqlParameter("@customerId", cart.ThisCustomer.CustomerID));
					}
				}
			}

			return OrderNumber;
		}

		// this routine does NOT go through all the normal cart to order conversion mechanics. It sets up everything explicitely.
		// NOTE: cardnumber COULD be just last4 coming in on the UseBillingAddress.CardNumber. That is ok. it is up to the gateway to allow it or fail it
		public static String MakeAdHocOrder(String PaymentGatewayToUse, int OriginalOrderNumber, String OriginalTransactionID, Customer OrderCustomer, Address UseBillingAddress, String CardExtraCode, Decimal OrderTotal, AppLogic.TransactionTypeEnum OrderType, String OrderDescription, out int NewOrderNumber)
		{
			String status = AppLogic.ro_OK;
			NewOrderNumber = 0;
			String GWCleaned = DetermineGatwayToUse(PaymentGatewayToUse);

			if(!GatewayLoader.GetProcessor(GWCleaned).SupportsAdHocOrders() || GWCleaned == "TWOCHECKOUT")
			{
				status = "Error: Gateway does not support ad-hoc orders!";
				return status;
			}

			NewOrderNumber = AppLogic.GetNextOrderNumber();

			// try to run the card first:
			String AVSResult = String.Empty;
			String AuthorizationResult = String.Empty;
			String AuthorizationCode = String.Empty;
			String AuthorizationTransID = String.Empty;
			String TransactionCommand = String.Empty;
			String TransactionResponse = String.Empty;
			bool IsRefund = false;
			if(OrderType.Equals(AppLogic.TransactionTypeEnum.CREDIT))
			{
				IsRefund = true;
			}

			if(IsRefund)
			{
				// NOTE: the "OrderTotal" that was passed in here is the actual RefundAmount (not the original OrderTotal!)
				status = Gateway.ProcessRefund(OrderCustomer.CustomerID, OriginalOrderNumber, NewOrderNumber, OrderTotal, OrderDescription, UseBillingAddress);
			}
			else
			{
				if(GWCleaned == ro_GWBRAINTREE)
					return "Error: Gateway does not support ad-hoc charges!";	//We support partial refunds, but not ad-hoc orders for Braintree

				status = Gateway.ProcessCard(null, PaymentGatewayToUse, OrderCustomer.CustomerID, NewOrderNumber, UseBillingAddress, CardExtraCode, null, OrderTotal, AppLogic.AppConfigBool("UseLiveTransactions"), String.Empty, String.Empty, String.Empty, out AVSResult, out AuthorizationResult, out AuthorizationCode, out AuthorizationTransID, out TransactionCommand, out TransactionResponse, out GWCleaned);
				if(AVSResult == null)
				{
					AVSResult = String.Empty;
				}
				if(AuthorizationResult == null)
				{
					AuthorizationResult = String.Empty;
				}
				if(AuthorizationCode == null)
				{
					AuthorizationCode = String.Empty;
				}
				if(AuthorizationTransID == null)
				{
					AuthorizationTransID = String.Empty;
				}
				if(TransactionCommand == null)
				{
					TransactionCommand = String.Empty;
				}
				if(TransactionResponse == null)
				{
					TransactionResponse = String.Empty;
				}
			}

			String TransCMD = TransactionCommand;
			if(TransCMD.Length != 0 && UseBillingAddress.CardNumber != null && UseBillingAddress.CardNumber.Length != 0)
			{
				String tmp1 = AppLogic.SafeDisplayCardNumber(UseBillingAddress.CardNumber, String.Empty, 0);
				TransCMD = TransCMD.Replace(UseBillingAddress.CardNumber, tmp1);
			}
			if(TransCMD.Length != 0 && CardExtraCode != null && CardExtraCode.Length != 0)
			{
				String tmp2 = AppLogic.SafeDisplayCardExtraCode(CardExtraCode);
				TransCMD = TransCMD.Replace(CardExtraCode, tmp2);
			}
			// we dont' need it anymore. NUKE IT!
			TransactionCommand = "1".PadLeft(TransactionCommand.Length);
			TransactionCommand = String.Empty;

			String TransRES = AuthorizationResult;
			if(TransRES.Length != 0 && UseBillingAddress.CardNumber != null && UseBillingAddress.CardNumber.Length != 0)
			{
				String tmp3 = AppLogic.SafeDisplayCardNumber(UseBillingAddress.CardNumber, String.Empty, 0);
				TransRES = TransRES.Replace(UseBillingAddress.CardNumber, tmp3);
			}
			if(TransRES.Length != 0 && CardExtraCode != null && CardExtraCode.Length != 0)
			{
				String tmp4 = AppLogic.SafeDisplayCardExtraCode(CardExtraCode);
				TransRES = TransRES.Replace(CardExtraCode, tmp4);
			}
			// we dont' need it anymore. NUKE IT!
			AuthorizationResult = "1".PadLeft(AuthorizationResult.Length);
			AuthorizationResult = String.Empty;

			if(status == AppLogic.ro_OK)
			{
				// ok, we have a good charge/or refund, so now make the proper records in orders and orders_shoppingcart tables!

				StringBuilder sql = new StringBuilder(4096);

				String AdHocNotes = String.Format("This is a {0} order type for original order number {1}", OrderType, OriginalOrderNumber.ToString());

				int ShipCalcID = (int)Shipping.GetActiveShippingCalculationID();

				sql.Append("insert into Orders(OrderNumber,VATRegistrationID,TransactionType,ParentOrderNumber,CartType,PaymentGateway,LocaleSetting,StoreVersion,CustomerID,CustomerServiceNotes,PaymentMethod,LastIPAddress,CustomerGUID,SkinID,LastName,FirstName,EMail,Phone,AffiliateID,OKToEMail,BillingEqualsShipping,BillingLastName,BillingFirstName,BillingCompany,BillingAddress1,BillingAddress2,BillingSuite,BillingCity,BillingState,BillingZip,BillingCountry,BillingPhone,ShippingLastName,ShippingFirstName,ShippingCompany,ShippingAddress1,ShippingAddress2,ShippingSuite,ShippingCity,ShippingState,ShippingZip,ShippingCountry,ShippingPhone,CardType,CardName,Last4,CardExpirationMonth,CardExpirationYear,CardStartDate,CardIssueNumber,OrderSubtotal,OrderTax,OrderShippingCosts,OrderTotal,AuthorizationResult,AuthorizationCode,AuthorizationPNREF,TransactionCommand,ShippingCalculationID) values (");
				sql.Append(NewOrderNumber.ToString() + ",");
				sql.Append(DB.SQuote(OrderCustomer.VATRegistrationID) + ",");
				sql.Append(CommonLogic.IIF(IsRefund, "2", "1") + ",");
				if(OriginalOrderNumber != 0)
				{
					sql.Append(OriginalOrderNumber.ToString() + ",");
				}
				else
				{
					sql.Append("NULL,");
				}
				sql.Append(((int)CartTypeEnum.ShoppingCart).ToString() + ",");
				sql.Append(DB.SQuote(GWCleaned) + ",");
				sql.Append(DB.SQuote(OrderCustomer.LocaleSetting) + ",");
				sql.Append(DB.SQuote(CommonLogic.GetVersion()) + ",");
				sql.Append(OrderCustomer.CustomerID.ToString() + ",");
				sql.Append(DB.SQuote(AdHocNotes + ". " + OrderDescription) + ",");
				sql.Append(DB.SQuote(UseBillingAddress.PaymentMethodLastUsed) + ",");
				sql.Append(DB.SQuote(OrderCustomer.LastIPAddress) + ",");
				sql.Append(DB.SQuote(OrderCustomer.CustomerGUID) + ",");
				sql.Append(OrderCustomer.SkinID.ToString() + ",");
				sql.Append(DB.SQuote(OrderCustomer.LastName) + ",");
				sql.Append(DB.SQuote(OrderCustomer.FirstName) + ",");
				sql.Append(DB.SQuote(OrderCustomer.EMail) + ",");
				sql.Append(DB.SQuote(OrderCustomer.Phone) + ",");
				sql.Append(OrderCustomer.AffiliateID.ToString() + ",");
				if(OrderCustomer.OKToEMail)
				{
					sql.Append("1,");
				}
				else
				{
					sql.Append("0,");
				}
				sql.Append("1,"); // BillingEqualsShipping

				// billing:
				sql.Append(DB.SQuote(UseBillingAddress.LastName) + ",");
				sql.Append(DB.SQuote(UseBillingAddress.FirstName) + ",");
				sql.Append(DB.SQuote(UseBillingAddress.Company) + ",");
				sql.Append(DB.SQuote(UseBillingAddress.Address1) + ",");
				sql.Append(DB.SQuote(UseBillingAddress.Address2) + ",");
				sql.Append(DB.SQuote(UseBillingAddress.Suite) + ",");
				sql.Append(DB.SQuote(UseBillingAddress.City) + ",");
				sql.Append(DB.SQuote(UseBillingAddress.State) + ",");
				sql.Append(DB.SQuote(UseBillingAddress.Zip) + ",");
				sql.Append(DB.SQuote(UseBillingAddress.Country) + ",");
				sql.Append(DB.SQuote(UseBillingAddress.Phone) + ",");

				// shipping:
				sql.Append(DB.SQuote(UseBillingAddress.LastName) + ",");
				sql.Append(DB.SQuote(UseBillingAddress.FirstName) + ",");
				sql.Append(DB.SQuote(UseBillingAddress.Company) + ",");
				sql.Append(DB.SQuote(UseBillingAddress.Address1) + ",");
				sql.Append(DB.SQuote(UseBillingAddress.Address2) + ",");
				sql.Append(DB.SQuote(UseBillingAddress.Suite) + ",");
				sql.Append(DB.SQuote(UseBillingAddress.City) + ",");
				sql.Append(DB.SQuote(UseBillingAddress.State) + ",");
				sql.Append(DB.SQuote(UseBillingAddress.Zip) + ",");
				sql.Append(DB.SQuote(UseBillingAddress.Country) + ",");
				sql.Append(DB.SQuote(UseBillingAddress.Phone) + ",");

				sql.Append(DB.SQuote(UseBillingAddress.CardType) + ",");
				sql.Append(DB.SQuote(UseBillingAddress.CardName) + ",");
				String Last4 = String.Empty;
				String PMCleaned = AppLogic.ro_PMCreditCard;
				if(PMCleaned == AppLogic.ro_PMCreditCard || PMCleaned == AppLogic.ro_PMPayPalExpress)
				{
					Last4 = AppLogic.SafeDisplayCardNumberLast4(UseBillingAddress.CardNumber, "Address", UseBillingAddress.AddressID);
				}
				sql.Append(DB.SQuote(Last4) + ",");
				sql.Append(DB.SQuote(UseBillingAddress.CardExpirationMonth) + ",");
				sql.Append(DB.SQuote(UseBillingAddress.CardExpirationYear) + ",");
				sql.Append(DB.SQuote(UseBillingAddress.CardStartDate) + ",");
				sql.Append(DB.SQuote(Security.MungeString(UseBillingAddress.CardIssueNumber)) + ",");
				sql.Append(Localization.CurrencyStringForDBWithoutExchangeRate(OrderTotal) + ",");
				sql.Append(Localization.CurrencyStringForDBWithoutExchangeRate(0.0M) + ",");
				sql.Append(Localization.CurrencyStringForDBWithoutExchangeRate(0.0M) + ",");
				sql.Append(Localization.CurrencyStringForDBWithoutExchangeRate(OrderTotal) + ",");
				sql.Append(DB.SQuote(AuthorizationResult) + ","); // must update later to RawResponseString if TX is ok!
				sql.Append(DB.SQuote(AuthorizationCode) + ",");   // must update later if TX is ok!
				sql.Append(DB.SQuote(AuthorizationTransID) + ",");  // must update later if TX is ok!
				sql.Append(DB.SQuote(TransCMD) + ",");  // must update later if TX is ok!
				sql.Append(DB.SQuote(ShipCalcID.ToString()));
				sql.Append(")");

				DB.ExecuteSQL(sql.ToString());

				// now set trans state and info, we do this as a separate update, to be consistent with the code in how MakeOrder does it
				sql.Length = 0;
				sql.Append("update orders set ");
				sql.Append("PaymentGateway=" + DB.SQuote(GWCleaned) + ", ");
				sql.Append("AVSResult=" + DB.SQuote(AVSResult) + ", ");

				// we can now store the cc if required, as we have to get the salt field from the order record just created!
				// only store the CC# if this gateway needs it for void/capture/refund later 
				// i.e. (even if the store has store cc true, don't store it unless the gateway needs it)
				String CC = String.Empty;
				if(PMCleaned == AppLogic.ro_PMCreditCard || PMCleaned == AppLogic.ro_PMPayPalExpress)
				{
					if(OrderCustomer.MasterShouldWeStoreCreditCardInfo)
					{
						CC = Security.MungeString(UseBillingAddress.CardNumber, Order.StaticGetSaltKey(NewOrderNumber));
					}
					else
					{
						CC = AppLogic.ro_CCNotStoredString;
					}
					sql.Append("CardNumber=" + DB.SQuote(CC) + ", ");
				}

				sql.Append("AuthorizationResult=" + DB.SQuote(TransRES) + ", ");
				sql.Append("AuthorizationCode=" + DB.SQuote(AuthorizationCode) + ", ");
				sql.Append("AuthorizationPNREF=" + DB.SQuote(AuthorizationTransID) + ", ");
				sql.Append("TransactionCommand=" + DB.SQuote(TransCMD));
				sql.Append(" where OrderNumber=" + NewOrderNumber.ToString());
				DB.ExecuteSQL(sql.ToString());

				sql.Length = 0;
				sql.Append("insert into Orders_ShoppingCart(ShippingAddressID,ShippingDetail,ShoppingCartRecID,OrderNumber,CustomerID,ProductID,VariantID,Quantity,OrderedProductName,OrderedProductSKU,OrderedProductPrice,OrderedProductRegularPrice,ColorOptionPrompt,SizeOptionPrompt,TextOptionPrompt,CustomerEntersPricePrompt,Notes) values(");
				sql.Append("0,NULL,0,");
				sql.Append(NewOrderNumber.ToString() + ",");
				sql.Append(OrderCustomer.CustomerID.ToString() + ",");
				sql.Append(AppLogic.AdHocProductID.ToString() + ",");
				sql.Append(AppLogic.AdHocVariantID.ToString() + ",");
				sql.Append("1,");
				sql.Append(DB.SQuote(CommonLogic.IIF(IsRefund, "Ad Hoc Refund", "Ad Hoc Charge")) + ",");
				sql.Append(DB.SQuote(CommonLogic.IIF(IsRefund, "ADHOCREFUND", "ADHOCCHARGE")) + ",");
				sql.Append(Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal) + ",");
				sql.Append(Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal) + ",");
				sql.Append(DB.SQuote(String.Empty) + ",");
				sql.Append(DB.SQuote(String.Empty) + ",");
				sql.Append(DB.SQuote(String.Empty) + ",");
				sql.Append(DB.SQuote(String.Empty) + ",");
				sql.Append(DB.SQuote(AdHocNotes));
				sql.Append(")");
				DB.ExecuteSQL(sql.ToString());

				if(!IsRefund)
				{
					bool isAuthOnly = AppLogic.AppConfig("TransactionMode").Equals("AUTH", StringComparison.InvariantCultureIgnoreCase);
					List<SqlParameter> spa = new List<SqlParameter>(){
						new SqlParameter("IsNew", (isAuthOnly ? 1 : 0)), //auth only transactions should be marked as new so they can be processed by admins
						new SqlParameter("TransactionState", isAuthOnly ? AppLogic.ro_TXStateAuthorized : AppLogic.ro_TXStateCaptured), //the gateway will have followed the transactionmode appconfig, so we should mark the ad hoc order accordingly.
						new SqlParameter("OrderNumber", NewOrderNumber)
					};

					DB.ExecuteSQL("update orders set IsNew=@IsNew, TransactionState=@TransactionState, AuthorizedOn=getdate() where OrderNumber=@OrderNumber", spa.ToArray());
				}
				else
				{
					// copy over refundtxcommand from "original order" to become the transaction command for this "new" refund order, just in case it's needed later:
					String ParentOrderRefundCommand = String.Empty;

					using(SqlConnection con = new SqlConnection(DB.GetDBConn()))
					{
						con.Open();
						using(IDataReader rs = DB.GetRS("select RefundTXCommand from Orders  with (NOLOCK)  where OrderNumber=" + OriginalOrderNumber.ToString(), con))
						{
							if(rs.Read())
							{
								ParentOrderRefundCommand = DB.RSField(rs, "RefundTXCommand");
							}
						}
					}

					DB.ExecuteSQL("update orders set TransactionCommand=" + DB.SQuote(ParentOrderRefundCommand) + ", IsNew=0, TransactionState=" + DB.SQuote(AppLogic.ro_TXStateRefunded) + ", AuthorizedOn=getdate(), CapturedOn=getdate(), RefundedOn=getdate() where OrderNumber=" + NewOrderNumber.ToString());
				}
			}
			return status;
		}

		// note if RecurringSubscriptionID is not empty, the caller must setup additional RecurringSubscription Fields in the order tables
		// after they call this routine
		public static String MakeRecurringOrder(ShoppingCart cart, int OrderNumber, String RecurringSubscriptionID, String XID)
		{
			String Status = MakeOrder(String.Empty, AppLogic.TransactionMode(), cart, OrderNumber, String.Empty, String.Empty, XID, RecurringSubscriptionID);
			return Status;
		}


		// returns AppLogic.ro_OK or error Msg.
		//
		// if AppLogic.ro_OK then order was created successfully, and the cart is now empty (unless it's a recurring cart, in which dates are updated to next recurring date)
		//
		// if error msg, then shopping cart remains unchanged as it was before call
		//
		// if PaymentGatewayToUse is empty, we'll use the active store payment gateway
		//
		// NOTE: if RecurringSubscriptionID is not empty we just use this routine to localize "order creation" for an already approved
		// gateway autobill recurring order, so in this case, NO money is to change hands, and no live gateay all is to be made, the order should
		// be forced to captured state. The caller must patch up the ordertotal, shipping, and tax fields based on the recurring autobilling information
		// that was received back from the gateway. This routine will NOT do that part for AutoBill orders.
		//
		/// <summary>
		/// Processes the payment then creates an order, returns AppLogic.ro_OK or error Msg.
		/// </summary>
		/// <param name="PaymentGatewayToUse">Specify the payment gateay to use, if none is specified then the deafult is used</param>
		/// <param name="TransactionMode">Set to AUTH or AUTH-CAPTURE</param>
		/// <param name="cart">ShoppingCart object being processed into the order</param>
		/// <param name="OrderNumber">Ordernumber for the new order</param>
		/// <param name="CAVV"></param>
		/// <param name="ECI"></param>
		/// <param name="XID"></param>
		/// <param name="RecurringSubscriptionID">If specified the we just use this routine to localize "order creation" for an already approved gateway autobill recurring order, so in this case, NO money is to change hands, and no live gateay all is to be made, the order should be forced to captured state. The caller must patch up the ordertotal, shipping, and tax fields based on the recurring autobilling information that was received back from the gateway. This routine will NOT do that part for AutoBill orders.</param>
		/// <returns>AppLogic.ro_OK or error Msg.</returns>
		public static String MakeOrder(String PaymentGatewayToUse, String TransactionMode, ShoppingCart cart, int OrderNumber, String CAVV, String ECI, String XID, String RecurringSubscriptionID)
		{
			return MakeOrder(PaymentGatewayToUse, TransactionMode, cart, OrderNumber, CAVV, ECI, XID, RecurringSubscriptionID, new Dictionary<string, string>());
		}

		/// <summary>
		/// Processes the payment then creates an order, returns AppLogic.ro_OK or error Msg.
		/// </summary>
		/// <param name="PaymentGatewayToUse">Specify the payment gateay to use, if none is specified then the deafult is used</param>
		/// <param name="TransactionMode">Set to AUTH or AUTH-CAPTURE</param>
		/// <param name="cart">ShoppingCart object being processed into the order</param>
		/// <param name="OrderNumber">Ordernumber for the new order</param>
		/// <param name="CAVV"></param>
		/// <param name="ECI"></param>
		/// <param name="XID"></param>
		/// <param name="RecurringSubscriptionID">If specified the we just use this routine to localize "order creation" for an already approved gateway autobill recurring order, so in this case, NO money is to change hands, and no live gateay all is to be made, the order should be forced to captured state. The caller must patch up the ordertotal, shipping, and tax fields based on the recurring autobilling information that was received back from the gateway. This routine will NOT do that part for AutoBill orders.</param>
		/// <param name="TransactionContext">Transaction params</param>
		/// <returns>AppLogic.ro_OK or error Msg.</returns>
		public static String MakeOrder(String PaymentGatewayToUse, String TransactionMode, ShoppingCart cart, int OrderNumber, String CAVV, String ECI, String XID, String RecurringSubscriptionID, IDictionary<string, string> TransactionContext)
		{
			if(OrderNumber == 0)
			{
				OrderNumber = AppLogic.GetNextOrderNumber();
			}

			string AVSResult = string.Empty;
			string AuthorizationResult = string.Empty;
			string AuthorizationCode = string.Empty;
			string AuthorizationTransID = string.Empty;
			string TransactionCommand = string.Empty;
			string TransactionResponse = string.Empty;
			string serializedAmazonOrderTrackingDetail = string.Empty;
			var orderTrackingDetail = new Processors.AmazonPaymentsOrderTrackingDetail();
			var customer = HttpContext.Current.GetCustomer();

			Address UseBillingAddress = new Address();
			UseBillingAddress.LoadByCustomer(customer.CustomerID, customer.PrimaryBillingAddressID, AddressTypes.Billing);
			Address UseShippingAddress = null;
			if(!cart.HasMultipleShippingAddresses())
			{
				// if only one address, let's get it so the gateway can display it:
				UseShippingAddress = new Address();
				UseShippingAddress.LoadByCustomer(customer.CustomerID, cart.FirstItemShippingAddressID(), AddressTypes.Shipping);
			}

			string GW = DetermineGatwayToUse(PaymentGatewayToUse);

			string status = AppLogic.ro_OK;
			string maxmindstatus = AppLogic.ro_OK;
			string PM = AppLogic.CleanPaymentMethod(UseBillingAddress.PaymentMethodLastUsed);
			if(RecurringSubscriptionID.Length != 0)
			{
				PM = AppLogic.ro_PMBypassGateway;
			}

			Decimal CartTotal = cart.Total(true);
			Decimal OrderTotal = CartTotal - CommonLogic.IIF(cart.Coupon.CouponType == CouponTypeEnum.GiftCard, CommonLogic.IIF(CartTotal < cart.Coupon.DiscountAmount, CartTotal, cart.Coupon.DiscountAmount), 0);
			bool SetToCapturedState = false;

			Decimal FraudScore = -1.0M;
			String FraudDetails = String.Empty;
			String CardExtraCode = AppLogic.GetCardExtraCodeFromSession(customer);

			#region Avalara - Prevent Order if Tax Can't be Calculated
			//Avalara - Fail order if tax can't be calculated
			if(AppLogic.AppConfigBool("AvalaraTax.Enabled") && AppLogic.AppConfigBool("AvalaraTax.PreventOrderIfAddressValidationFails"))
			{
				AvaTax avaTax = new AvaTax();
				try
				{
					//Address Validation isn't enough to be sure tax can't be calculated
					//So we have to check this again to ensure all is well.  Yet another call to GetTaxRate :(
					//Exception means something is very wrong, anything else is ok
					avaTax.GetTaxRate(customer, cart.CartItems, cart.OrderOptions);
				}
				catch(Exception ex)
				{
					status = String.Format("<br />Please enter a valid address before continuing checkout or contact us for assistance.", avaTax.ValidateAddress(customer));
					SysLog.LogMessage(
						status,
						ex.Message,
						MessageTypeEnum.GeneralException,
						MessageSeverityEnum.Error);
					//log as failed transaction
					DB.ExecuteSQL(@"insert into FailedTransaction(CustomerID,OrderNumber,IPAddress,OrderDate,PaymentGateway,PaymentMethod,TransactionCommand,TransactionResult)
                            values(@CustomerID,@OrderNumber,@IPAddress,@OrderDate,@PaymentGateway,@PaymentMethod,@TransactionCommand,@TransactionResult)", new SqlParameter[] {
								new SqlParameter("@CustomerID", customer.CustomerID.ToString()),
								new SqlParameter("@OrderNumber", OrderNumber.ToString()),
								new SqlParameter("@IPAddress", customer.LastIPAddress),
								new SqlParameter("@OrderDate", DateTime.Now),
								new SqlParameter("@PaymentGateway", GW),
								new SqlParameter("@PaymentMethod", PM),
								new SqlParameter("@TransactionCommand", "AppConfig AvalaraTax.PreventOrderIfAddressValidationFails Stopped Order Processing"),
								new SqlParameter("@TransactionResult",ex.Message)});
					//bail immediately
					return status;
				}
			}
			#endregion

			if(OrderTotal == System.Decimal.Zero)
			{
				AuthorizationTransID = "ZeroCostOrder";
				status = AppLogic.ro_OK; // nothing to charge!
			}
			else
			{
				#region MaxMind Fraud Calculation
				// is maxmind prefraud score checking enabled:
				if(AppLogic.AppConfigBool("MaxMind.Enabled"))
				{
					Address UseShippingAddressX = new Address();
					UseShippingAddressX.LoadByCustomer(customer.CustomerID, cart.FirstItemShippingAddressID(), AddressTypes.Shipping);
					FraudScore = MaxMindFraudCheck(OrderNumber
						, customer
						, UseBillingAddress
						, UseShippingAddressX
						, OrderTotal
						, customer.CurrencySetting
						, PM
						, out FraudDetails);
				}
				#endregion

				#region Select Payment Method
				if(PM == AppLogic.ro_PMCreditCard)
				{
					if(FraudScore >= System.Decimal.Zero)
					{
						// ok, call worked, now let's determine what to do with it:
						if(FraudScore >= AppLogic.AppConfigUSDecimal("MaxMind.FailScoreThreshold"))
						{
							// score was higher than failure threshold set by the store admin, so fail this order!
							DB.ExecuteSQL("insert into FailedTransaction(CustomerID,OrderNumber,IPAddress,MaxMindFraudScore,MaxMindDetails,OrderDate,PaymentGateway,PaymentMethod,TransactionCommand,TransactionResult) values(" +
								customer.CustomerID.ToString() + "," +
								OrderNumber.ToString() + "," +
								DB.SQuote(customer.LastIPAddress) + "," +
								Localization.DecimalStringForDB(FraudScore) + "," +
								DB.SQuote(FraudDetails) + "," +
								"getdate()," +
								DB.SQuote("MAXMIND") + "," +
								DB.SQuote(PM) + "," +
								DB.SQuote("MAXMIND FRAUD SCORE=" + Localization.DecimalStringForDB(FraudScore)) + "," +
								DB.SQuote(AppLogic.ro_NotApplicable) + ")");
							maxmindstatus = "MAXMIND FRAUD CHECK FAILED";
						}
					}

					if(maxmindstatus == AppLogic.ro_OK)
					{
						// pre-fraud check was ok, so let's try the real card processing now:
						status = ProcessCard(cart, PaymentGatewayToUse, customer.CustomerID, OrderNumber, UseBillingAddress, CardExtraCode, UseShippingAddress, OrderTotal, AppLogic.AppConfigBool("UseLiveTransactions"), CAVV, ECI, XID, out AVSResult, out AuthorizationResult, out AuthorizationCode, out AuthorizationTransID, out TransactionCommand, out TransactionResponse, out GW);
						if(status != AppLogic.ro_OK && status != AppLogic.ro_3DSecure)
						{
							// store maxmind results in failed transaction table results:
							DB.ExecuteSQL("update FailedTransaction set MaxMindFraudScore=" + Localization.DecimalStringForDB(FraudScore) + ", MaxMindDetails=" + DB.SQuote(FraudDetails) + " where CustomerID=" + customer.CustomerID.ToString() + " and OrderNumber=" + OrderNumber.ToString());
						}
						SetToCapturedState = AppLogic.TransactionModeIsAuthCapture();
					}
					else
					{
						// store maxmind results in failed transaction table results:
						DB.ExecuteSQL("update FailedTransaction set MaxMindFraudScore=" + Localization.DecimalStringForDB(FraudScore) + ", MaxMindDetails=" + DB.SQuote(FraudDetails) + " where CustomerID=" + customer.CustomerID.ToString() + " and OrderNumber=" + OrderNumber.ToString());
						status = maxmindstatus;
					}
				}
				else if(PM == AppLogic.ro_PMAmazonPayments)
				{
					SetToCapturedState = TransactionMode != AppLogic.ro_TXModeAuthOnly;

					orderTrackingDetail.OrderReference.Id = UseBillingAddress.CardNumber;

					status = new AmazonPaymentsGateway()
						.AuthorizeOrder(
							orderTrackingDetail: ref orderTrackingDetail,
							orderNumber: OrderNumber,
							customerId: customer.CustomerID,
							orderTotal: OrderTotal,
							useLiveTransactions: AppLogic.AppConfigBool("UseLiveTransactions"),
							authAndCapture: SetToCapturedState);

					if (status != AppLogic.ro_OK)
					{
						UseBillingAddress.CardNumber = string.Empty;
						UseBillingAddress.UpdateDB();
					}

					var serializer = new Processors.AmazonPaymentsOrderTrackingDetailSerializer();
					serializedAmazonOrderTrackingDetail = serializer.SerializeAmazonOrderTrackingDetail(orderTrackingDetail);
				}
				else if(PM == AppLogic.ro_PMPayPalExpress || PM == AppLogic.ro_PMPayPalExpressMark)
				{
					String PayPalToken = CAVV;  //hack: needed place to pass token and payerID.
					String PayerID = ECI;
					// Note that we are reseting GW here for this order. (output parameter)
					// This is so that refund/void etc. will go through the proper gateway.
					status = ProcessExpressCheckout(cart, OrderTotal, OrderNumber, PayPalToken, PayerID, TransactionMode, out AuthorizationResult, out AuthorizationTransID, out GW);
					SetToCapturedState = false;
					if(TransactionMode != AppLogic.ro_TXModeAuthOnly
						|| AppLogic.AppConfigBool("PayPal.ForceCapture"))
					{
						SetToCapturedState = true;
					}
				}
				else if(PM == AppLogic.ro_PMMicropay)
				{
					status = MicropayController.ProcessTransaction(OrderNumber, customer.CustomerID, OrderTotal, AppLogic.AppConfigBool("UseLiveTransactions"), AppLogic.TransactionMode(), UseBillingAddress, String.Empty, UseShippingAddress, CAVV, ECI, XID, out AVSResult, out AuthorizationResult, out AuthorizationCode, out AuthorizationTransID, out TransactionCommand, out TransactionResponse);
					SetToCapturedState = AppLogic.TransactionModeIsAuthCapture();
				}
				else if(PM == AppLogic.ro_PMPurchaseOrder)
				{
					status = AppLogic.ro_OK;
					SetToCapturedState = false;
				}
				else if(PM == AppLogic.ro_PMCheckByMail)
				{
					status = AppLogic.ro_OK;
					SetToCapturedState = false;
				}
				else if(PM == AppLogic.ro_PMCOD)
				{
					status = AppLogic.ro_OK;
					SetToCapturedState = false;
				}
				else if(PM == AppLogic.ro_PMRequestQuote)
				{
					status = AppLogic.ro_OK;
					SetToCapturedState = false;
				}
				else if(PM == AppLogic.ro_PMBypassGateway)
				{
					status = AppLogic.ro_OK;
					SetToCapturedState = true;
				}
				else if(PM == AppLogic.ro_PMPayPalEmbeddedCheckout)
				{
					status = AppLogic.ro_OK;
					SetToCapturedState = false;
				}
				else
				{
					//PM has somehow ended up as something invalid.  Don't process the order.
					status = "Payment method is required";
				}
				#endregion
			}
			if(OrderTotal == System.Decimal.Zero || RecurringSubscriptionID.Length != 0)
			{
				SetToCapturedState = true; // zero dollar orders always get set to captured state right away!
			}
			#region If Status OK - Create Order
			if(status == AppLogic.ro_OK)
			{
				CreateOrderRecord(cart, OrderNumber, UseBillingAddress);

				//Address cleanup for offsite payment methods where we lock down the address until after the order is placed
				if(AppLogic.ro_OffsitePayMentMethods.Contains(PM))
				{
					//This should have the full address details from the offsite vendor now.  Allow it to be used/edited in the future.
					Address.ReleaseOffsiteAddress(customer, customer.PrimaryShippingAddress);

					Address.CleanupAbandonAddresses(customer, PM);
				}

				if(UseBillingAddress.CardNumber == null)
				{
					UseBillingAddress.CardNumber = string.Empty;
				}
				if(CardExtraCode == null)
				{
					CardExtraCode = string.Empty;
				}
				if(TransactionCommand == null)
				{
					TransactionCommand = string.Empty;
				}
				if(AuthorizationResult == null)
				{
					AuthorizationResult = string.Empty;
				}
				if(FraudDetails == null)
				{
					FraudDetails = string.Empty;
				}
				if(AVSResult == null)
				{
					AVSResult = string.Empty;
				}
				if(AuthorizationCode == null)
				{
					AuthorizationCode = string.Empty;
				}
				if(AuthorizationTransID == null)
				{
					AuthorizationTransID = string.Empty;
				}
				if(RecurringSubscriptionID == null)
				{
					RecurringSubscriptionID = string.Empty;
				}

				string TransCMD = TransactionCommand;
				if(TransCMD.Length != 0 && UseBillingAddress.CardNumber != null && UseBillingAddress.CardNumber.Length != 0)
				{
					string tmp1 = AppLogic.SafeDisplayCardNumber(UseBillingAddress.CardNumber, String.Empty, 0);
					TransCMD = TransCMD.Replace(UseBillingAddress.CardNumber, tmp1);
				}
				if(TransCMD.Length != 0 && CardExtraCode != null && CardExtraCode.Length != 0)
				{
					string tmp2 = AppLogic.SafeDisplayCardExtraCode(CardExtraCode);
					TransCMD = TransCMD.Replace(CardExtraCode, tmp2);
				}
				// we dont' need it anymore. NUKE IT!
				TransactionCommand = "1".PadLeft(TransactionCommand.Length);
				TransactionCommand = string.Empty;

				string TransRES = AuthorizationResult;
				if(TransRES.Length != 0 && UseBillingAddress.CardNumber != null && UseBillingAddress.CardNumber.Length != 0)
				{
					String tmp3 = AppLogic.SafeDisplayCardNumber(UseBillingAddress.CardNumber, String.Empty, 0);
					TransRES = TransRES.Replace(UseBillingAddress.CardNumber, tmp3);
				}
				if(TransRES.Length != 0 && CardExtraCode != null && CardExtraCode.Length != 0)
				{
					String tmp4 = AppLogic.SafeDisplayCardExtraCode(CardExtraCode);
					TransRES = TransRES.Replace(CardExtraCode, tmp4);
				}
				// we dont' need it anymore. NUKE IT!
				AuthorizationResult = "1".PadLeft(AuthorizationResult.Length);
				AuthorizationResult = String.Empty;

				String FraudRES = FraudDetails;
				if(FraudRES.Length != 0 && UseBillingAddress.CardNumber != null && UseBillingAddress.CardNumber.Length != 0)
				{
					String tmp5 = AppLogic.SafeDisplayCardNumber(UseBillingAddress.CardNumber, String.Empty, 0);
					FraudRES = FraudRES.Replace(UseBillingAddress.CardNumber, tmp5);
				}
				if(FraudRES.Length != 0 && CardExtraCode != null && CardExtraCode.Length != 0)
				{
					String tmp6 = AppLogic.SafeDisplayCardExtraCode(CardExtraCode);
					FraudRES = FraudRES.Replace(CardExtraCode, tmp6);
				}
				// we dont' need it anymore. NUKE IT!
				FraudDetails = "1".PadLeft(FraudDetails.Length);
				FraudDetails = String.Empty;

				// DO NOT set a captured state here, it will be set by ProcessOrderAsCaptured later in this routine, if appropriate:
				// we just handle the auth  here!
				string transState = AppLogic.isPendingPM(PM)
					? AppLogic.ro_TXStatePending
					: AppLogic.ro_TXStateAuthorized;

				string sql2 = string.Empty;
				if(PM == AppLogic.ro_PMAmazonPayments)
				{
					sql2 =
						string.Format("UPDATE Orders SET PaymentGateway={0}, TransactionCommand={1}, AuthorizationPNREF={2}, AuthorizationResult={3}, AuthorizationCode={4}, VoidTXCommand={5}, VoidTXResult={6}, CaptureTXCommand={7}, CaptureTXResult={8}, RefundTXCommand={9}, RefundTXResult={10}, TransactionState={11}, AuthorizedOn=GETDATE() WHERE OrderNumber={12}",
						DB.SQuote(GW), DB.SQuote(serializedAmazonOrderTrackingDetail), DB.SQuote(orderTrackingDetail.Authorization.Id), DB.SQuote(orderTrackingDetail.Authorization.State), DB.SQuote(orderTrackingDetail.Authorization.ReasonCode), DB.SQuote(orderTrackingDetail.Cancel.Id), DB.SQuote(orderTrackingDetail.Cancel.State), DB.SQuote(orderTrackingDetail.Capture.Id), DB.SQuote(orderTrackingDetail.Capture.State), DB.SQuote(orderTrackingDetail.Refund.Id), DB.SQuote(orderTrackingDetail.Refund.State), DB.SQuote(transState), OrderNumber.ToString());
				}
				else
				{
					sql2 =
						string.Format("update Orders set PaymentGateway={0}, AVSResult={1}, AuthorizationResult={2}, AuthorizationCode={3}, AuthorizationPNREF={4}, MaxMindFraudScore={5}, MaxMindDetails={6}, TransactionState={7}, AuthorizedOn=getdate(), TransactionCommand={8}, RecurringSubscriptionID={9} where OrderNumber={10}",
						DB.SQuote(GW), DB.SQuote(AVSResult), DB.SQuote(TransRES), DB.SQuote(AuthorizationCode), DB.SQuote(AuthorizationTransID),
							Localization.DecimalStringForDB(FraudScore), DB.SQuote(FraudRES), DB.SQuote(transState),
						DB.SQuote(TransCMD), DB.SQuote(RecurringSubscriptionID), OrderNumber.ToString());
				}
				DB.ExecuteSQL(sql2);

				if(RecurringSubscriptionID.Length != 0)
				{
					// remember to set special TransactionType for a gateway recurring autobill:
					sql2 = String.Format("update Orders set ParentOrderNumber={0}, TransactionType={1} where OrderNumber={2}", cart.OriginalRecurringOrderNumber.ToString(), ((int)AppLogic.TransactionTypeEnum.RECURRING_AUTO).ToString(), OrderNumber.ToString());
					DB.ExecuteSQL(sql2);
				}

				// order was ok, clean up shopping cart and move cart to order cart:
				if(cart.HasCoupon())
				{
					Order o = new Order(OrderNumber, customer.LocaleSetting);
					GiftCard gc = new GiftCard(cart.Coupon.CouponCode);
					decimal TransAmt = CommonLogic.IIF(o.Total() > gc.Balance, gc.Balance, o.Total());
					gc.AddTransaction(TransAmt, customer.CustomerID, OrderNumber);
				}

				//When processing Recurring order need to limit to the current _originalRecurringOrderNumber
				String RecurringOrderSql = String.Empty;
				if(cart.OriginalRecurringOrderNumber != 0)
				{
					RecurringOrderSql = String.Format(" and OriginalRecurringOrderNumber={0}", cart.OriginalRecurringOrderNumber);
				}

				int VatCountryID = AppLogic.AppConfigNativeInt("VAT.CountryID");

				//move the shopping cart records to orders_shoppingcart records.
				//check whether the product has manufacturerpartnumber as well as the variant, if either of them has it then get the manufacturerpartnumber 
				//of the product not the variant
				String sql4 = "INSERT INTO orders_ShoppingCart(OrderNumber,DistributorID,CartType,ShippingMethodID,ShippingMethod,"
					+ " Notes,ShippingAddressID,ExtensionData,ShoppingCartRecID,CustomerID,ProductID,"
					+ " VariantID,Quantity,ChosenColor,ChosenColorSKUModifier,ChosenSize,"
					+ " ChosenSizeSKUModifier,TextOption,ColorOptionPrompt,SizeOptionPrompt,TextOptionPrompt,CustomerEntersPricePrompt,"
					+ " OrderedProductName,OrderedProductVariantName,OrderedProductSKU,OrderedProductManufacturerPartNumber,OrderedProductWeight,"
					+ " OrderedProductPrice,CustomerEntersPrice,IsTaxable,IsShipSeparately,IsDownload,FreeShipping, IsAKit,"
					+ " IsSystem, TaxClassID, TaxRate, IsGift,GTIN)"
					+ String.Format(" SELECT {0},D.DistributorID,sc.CartType,sc.ShippingMethodID,sc.ShippingMethod,sc.Notes,"
					+ " sc.ShippingAddressID,sc.ExtensionData,ShoppingCartRecID,sc.CustomerID,sc.ProductID,"
					+ " sc.VariantID,sc.Quantity,ChosenColor,ChosenColorSKUModifier,ChosenSize,ChosenSizeSKUModifier,TextOption,Product.ColorOptionPrompt,"
					+ " Product.SizeOptionPrompt,Product.TextOptionPrompt,ProductVariant.CustomerEntersPricePrompt,Product.Name,ProductVariant.Name,ProductSKU,"
					+ " ISNULL(Product.ManufacturerPartNumber, '') + ISNULL(pv.ManufacturerPartNumber, '')"
					+ ",sc.ProductWeight,sc.ProductPrice,sc.CustomerEntersPrice,sc.IsTaxable,sc.IsShipSeparately,"
					+ " sc.IsDownload,sc.FreeShipping, sc.IsAKit, sc.IsSystem, sc.TaxClassID,"
					+ "(isnull(cr.taxrate, 0)+isnull(sr.taxrate, 0)+isnull(zr.taxrate, 0)), IsGift, sc.GTIN", OrderNumber)
					+ " from ((ShoppingCart sc  with (NOLOCK)  left outer join product  with (NOLOCK)  on sc.productid=product.productid)"
					+ " left outer join productvariant  with (NOLOCK)  on sc.variantid=productvariant.variantid)"
					+ " left outer join ProductDistributor D  with (NOLOCK)  on product.ProductID=D.ProductID"
					+ " left join ProductVariant pv on sc.VariantID = pv.VariantID "
					+ " left join address a on sc.ShippingAddressID = addressid "
					+ " left join country c on c.name = a.country "
					+ " left join state s on s.abbreviation = a.state and s.countryid = c.countryid"
					+ " left join countrytaxrate cr on cr.countryid = isnull(c.countryid, " + VatCountryID + ") and cr.TaxClassID = sc.TaxCLassID "
					+ " left join statetaxrate sr on sr.stateid = s.StateID and sr.TaxClassID = sc.TaxCLassID "
					+ " left join ZipTaxRate zr on zr.ZipCode = a.Zip and zr.TaxClassID = sc.TaxCLassID "
					+ string.Format("inner join (select distinct a.ProductID,a.StoreID from ShoppingCart a left join ProductStore b on a.ProductID = b.ProductID where ({0} = 0 or b.StoreID = a.StoreID)) productstore "
					+ "on sc.ProductID = productstore.ProductID and sc.StoreID = productstore.StoreID ", CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowProductFiltering") == true, 1, 0))
					+ String.Format(" where (sc.Quantity IS NOT NULL and sc.Quantity > 0) and sc.CartType={0} and sc.customerid={1} and ({2} = 0 or sc.StoreID = {3})",
					+(int)cart.CartType, customer.CustomerID, CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowShoppingcartFiltering") == true, 1, 0), AppLogic.StoreID())
					+ RecurringOrderSql;
				DB.ExecuteSQL(sql4);
				if(cart.HasGiftCards())
				{
					GiftCard.SyncOrderNumber(OrderNumber);
				}
				//For multi shipping address orders fix up the ShippingDetail if different from the primary Shipping address. 
				sql4 = "select ShoppingCartRecID, ShippingAddressID from ShoppingCart "
					+ String.Format(" where CartType={0} and CustomerID={1} ", (int)cart.CartType, customer.CustomerID)
					+ RecurringOrderSql;

				using(SqlConnection con = new SqlConnection(DB.GetDBConn()))
				{
					con.Open();
					using(IDataReader rso = DB.GetRS(sql4, con))
					{
						while(rso.Read())
						{
							int addressID = DB.RSFieldInt(rso, "ShippingAddressID");
							int cartID = DB.RSFieldInt(rso, "ShoppingCartRecID");
							if(addressID == 0)
							{
								addressID = customer.PrimaryShippingAddressID;
							}
							Address shipAddress = new Address();
							shipAddress.LoadFromDB(addressID);
							string sql = "update orders_shoppingcart set ShippingAddressID=" + addressID.ToString() + ", ShippingDetail=" + DB.SQuote(shipAddress.AsXml) + " where ShoppingCartRecID=" + cartID.ToString();
							DB.ExecuteSQL(sql);
						}
					}
				}

				String sql5 = "insert into orders_kitcart(OrderNumber,CartType,KitCartRecID,CustomerID,ShoppingCartRecID,ProductID,VariantID,ProductName,productVariantName,KitGroupID,KitGroupTypeID,InventoryVariantID,InventoryVariantColor,InventoryVariantSize,KitGroupName,KitGroupIsRequired,KitItemID,KitItemName,KitItemPriceDelta,KitItemWeightDelta,TextOption,Quantity)"
					+ String.Format(" select {0},CartType,KitCartRecID,CustomerID,ShoppingCartRecID,KitCart.ProductID,KitCart.VariantID,Product.Name,ProductVariant.Name,KitCart.KitGroupID,KitCart.KitGroupTypeID,KitItem.InventoryVariantID,KitItem.InventoryVariantColor,KitItem.InventoryVariantSize,KitGroup.Name,KitGroup.IsRequired,KitCart.KitItemID,KitItem.Name,KitItem.PriceDelta,KitItem.WeightDelta,KitCart.TextOption,Quantity", OrderNumber)
					+ " FROM ((((KitCart   with (NOLOCK)  INNER JOIN KitGroup   with (NOLOCK)  ON KitCart.KitGroupID = KitGroup.KitGroupID)"
					+ " INNER JOIN KitItem  with (NOLOCK)  ON KitCart.KitItemID = KitItem.KitItemID)"
					+ " INNER JOIN Product   with (NOLOCK)  ON KitCart.ProductID = Product.ProductID)"
					+ " INNER JOIN ProductVariant   with (NOLOCK)  ON KitCart.VariantID = ProductVariant.VariantID)"
					+ String.Format(" WHERE CartType={0} and customerid={1} and ShoppingCartRecID <> 0", (int)cart.CartType, customer.CustomerID)
					+ RecurringOrderSql;
				DB.ExecuteSQL(sql5);

				// download products
				if(cart.HasDownloadComponents())
				{
					bool autoRelease = AppLogic.AppConfig("Download.ReleaseOnAction").EqualsIgnoreCase("auto");

					DownloadItem downloadItem = new DownloadItem();
					foreach(CartItem c in cart.CartItems.Where(w => w.IsDownload))
					{
						downloadItem.Create(OrderNumber, c);
						if(autoRelease)
						{
							downloadItem.Load(c.ShoppingCartRecordID);
							downloadItem.SendDownloadEmailNotification(false);
							downloadItem.Release(false);
						}
					}
				}

				bool m_CustomerLevelAllowsQuantityDiscounts = QuantityDiscount.CustomerLevelAllowsQuantityDiscounts(customer.CustomerLevelID);
				// now set extended pricing info in the order cart to take into account all levels, quantities, etc...so the order object doesn't have to recompute cart stuff
				foreach(CartItem c in cart.CartItems)
				{
					if(!c.CustomerEntersPrice && !AppLogic.IsAKit(c.ProductID) && !c.IsUpsell)
					{
						int Q = c.Quantity;
						bool IsOnSale = false;
						decimal pr = 0.0M;
						if(cart.CartType == CartTypeEnum.RecurringCart || c.ProductID == 0)
						{
							pr = c.Price; // price is grandfathered
						}
						else
						{
							pr = AppLogic.DetermineLevelPrice(c.VariantID, customer.CustomerLevelID, out IsOnSale);
						}
						pr = pr * Q;
						Decimal DIDPercent = 0.0M;
						QuantityDiscount.QuantityDiscountType fixedPriceDID = QuantityDiscount.QuantityDiscountType.Percentage;
						if(m_CustomerLevelAllowsQuantityDiscounts)
						{
							DIDPercent = QuantityDiscount.GetQuantityDiscountTablePercentageForLineItem(c, out fixedPriceDID);
							if(DIDPercent != 0.0M)
							{
								if(fixedPriceDID.Equals(QuantityDiscount.QuantityDiscountType.FixedAmount))
								{
									pr = (c.Price - DIDPercent) * (Decimal)Q;
								}
								else
								{
									pr = (1.0M - (DIDPercent / 100.0M)) * pr;
								}
							}
						}
						decimal regular_pr = System.Decimal.Zero;
						decimal sale_pr = System.Decimal.Zero;
						decimal extended_pr = System.Decimal.Zero;
						if(cart.CartType != CartTypeEnum.RecurringCart)
						{
							regular_pr = AppLogic.GetVariantPrice(c.VariantID);
							sale_pr = AppLogic.GetVariantSalePrice(c.VariantID);
							extended_pr = AppLogic.GetVariantExtendedPrice(c.VariantID, customer.CustomerLevelID);

							// Adjust for color and size price modifiers
							Decimal PrMod = AppLogic.GetColorAndSizePriceDelta(c.ChosenColor, c.ChosenSize, c.TaxClassID, customer, true, true);

							if(PrMod != System.Decimal.Zero)
							{
								pr += Decimal.Round(PrMod * (1.0M - (DIDPercent / 100.0M)), 2, MidpointRounding.AwayFromZero) * Q;
							}
							if(pr < System.Decimal.Zero)
							{
								pr = System.Decimal.Zero;
							}
						}
						else
						{
							regular_pr = c.Price;
							sale_pr = System.Decimal.Zero;
							extended_pr = System.Decimal.Zero;
						}

						DB.ExecuteSQL("update orders_ShoppingCart set OrderedProductPrice=" + Localization.DecimalStringForDB(pr) + ", OrderedProductRegularPrice=" + Localization.CurrencyStringForDBWithoutExchangeRate(regular_pr) + ", OrderedProductSalePrice=" + Localization.CurrencyStringForDBWithoutExchangeRate(sale_pr) + ", OrderedProductExtendedPrice=" + Localization.CurrencyStringForDBWithoutExchangeRate(extended_pr) + " where OrderNumber=" + OrderNumber.ToString() + " and ShoppingCartRecID=" + c.ShoppingCartRecordID.ToString());
					}
					else if(!c.CustomerEntersPrice && !AppLogic.IsAKit(c.ProductID) && c.IsUpsell)
					{
						int Q = c.Quantity;
						decimal pr = c.Price * Q;
						DB.ExecuteSQL("update orders_ShoppingCart set OrderedProductPrice=" + Localization.DecimalStringForDB(pr) + ", OrderedProductRegularPrice=" + Localization.CurrencyStringForDBWithoutExchangeRate(c.Price) + " where OrderNumber=" + OrderNumber.ToString() + " and ShoppingCartRecID=" + c.ShoppingCartRecordID.ToString());
					}
					else
					{
						int Q = c.Quantity;
						decimal pr = c.Price * Q;
						DB.ExecuteSQL("update orders_ShoppingCart set OrderedProductPrice=" + Localization.DecimalStringForDB(pr) + " where OrderNumber=" + OrderNumber.ToString() + " and ShoppingCartRecID=" + c.ShoppingCartRecordID.ToString());
					}
				}

				// make sure inventory was deducted. safe to call repeatedly. proc protects against deducting twice
				DB.ExecuteSQL("aspdnsf_AdjustInventory " + OrderNumber.ToString() + ",-1");

				// TFS 821: Create an order shipment record for each shipment
				List<int> shippingAddressIds = Shipping.GetDistinctShippingAddressIDs(cart.CartItems);
				foreach(var shippingAddressId in shippingAddressIds)
				{
					var shippingAddress = new Address();
					shippingAddress.LoadFromDB(shippingAddressId);

					decimal shippingTotal = Prices.ShippingTotalForAddress(cart.CartItems, customer, shippingAddress, includeTax: true);
					DB.ExecuteSQL("insert into OrderShipment (OrderNumber, AddressID, ShippingTotal) select distinct OrderNumber, ShippingAddressID, @shippingTotal from Orders_ShoppingCart where OrderNumber = @orderNumber and ShippingAddressID = @shippingAddressID",
						new[] {
						new SqlParameter("@orderNumber", OrderNumber),
						new SqlParameter("@shippingAddressID", shippingAddressId),
						new SqlParameter("@shippingTotal", shippingTotal),
					});
				}

				// clear cart
				String RecurringVariantsList = AppLogic.GetRecurringVariantsList();

				if(cart.CartType == CartTypeEnum.ShoppingCart)
				{
					// clear "normal" items out of cart, but leave any recurring items or wishlist items still in there:
					DB.ExecuteSQL("delete from kitcart where CartType=" + ((int)CartTypeEnum.ShoppingCart).ToString() + CommonLogic.IIF(RecurringVariantsList.Length != 0, " and VariantID not in (" + RecurringVariantsList + ")", "") + " and customerid=" + customer.CustomerID.ToString());
					string query = string.Format("delete from ShoppingCart where ShoppingCartRecID in(select a.ShoppingCartRecID from ShoppingCart a with (nolock) inner join (select distinct a.ProductID,a.StoreID from ShoppingCart a with (nolock) left join ProductStore b with (nolock) on a.ProductID = b.ProductID " +
					"where ({0} = 0 or b.StoreID = a.StoreID)) b on a.ProductID = b.ProductID and a.StoreID = b.StoreID where CartType={1} and CustomerID={2} and ({3} = 0 or a.StoreID = {4}))", CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowProductFiltering") == true, 1, 0), ((int)CartTypeEnum.ShoppingCart).ToString() +
					CommonLogic.IIF(RecurringVariantsList.Length != 0, " and VariantID not in (" + RecurringVariantsList + ")", ""), customer.CustomerID, CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowShoppingcartFiltering") == true, 1, 0), AppLogic.StoreID());

					DB.ExecuteSQL(query);
				}

				if(RecurringVariantsList.Length != 0 && cart.ContainsRecurringAutoShip)
				{
					// WE HAVE RECURRING ITEMS! They should be left in the cart, so the next recurring process will still find them.

					DateTime NextRecurringShipDate = System.DateTime.Now.AddMonths(1); // default just for safety, should never be used
					if(cart.OriginalRecurringOrderNumber == 0)
					{
						// this is a completely NEW recurring order, so set the recurring master parameters:
						String ThisOrderDate = Localization.ToNativeDateTimeString(System.DateTime.Now);
						foreach(CartItem c in cart.CartItems)
						{
							if(c.IsRecurring)
							{
								switch(c.RecurringIntervalType)
								{
									case DateIntervalTypeEnum.Day:
										NextRecurringShipDate = System.DateTime.Now.AddDays(c.RecurringInterval);
										break;
									case DateIntervalTypeEnum.Week:
										NextRecurringShipDate = System.DateTime.Now.AddDays(7 * c.RecurringInterval);
										break;
									case DateIntervalTypeEnum.Month:
										NextRecurringShipDate = System.DateTime.Now.AddMonths(c.RecurringInterval);
										break;
									case DateIntervalTypeEnum.Year:
										NextRecurringShipDate = System.DateTime.Now.AddYears(c.RecurringInterval);
										break;
									case DateIntervalTypeEnum.NumberOfDays:
										NextRecurringShipDate = System.DateTime.Now.AddDays(c.RecurringInterval);
										break;
									case DateIntervalTypeEnum.Weekly:
										NextRecurringShipDate = System.DateTime.Now.AddDays(7);
										break;
									case DateIntervalTypeEnum.BiWeekly:
										NextRecurringShipDate = System.DateTime.Now.AddDays(14);
										break;
									case DateIntervalTypeEnum.EveryFourWeeks:
										NextRecurringShipDate = System.DateTime.Now.AddDays(28);
										break;
									case DateIntervalTypeEnum.Monthly:
										NextRecurringShipDate = System.DateTime.Now.AddMonths(1);
										break;
									case DateIntervalTypeEnum.Quarterly:
										NextRecurringShipDate = System.DateTime.Now.AddMonths(3);
										break;
									case DateIntervalTypeEnum.SemiYearly:
										NextRecurringShipDate = System.DateTime.Now.AddMonths(6);
										break;
									case DateIntervalTypeEnum.Yearly:
										NextRecurringShipDate = System.DateTime.Now.AddYears(1);
										break;
									default:
										NextRecurringShipDate = System.DateTime.Now.AddMonths(c.RecurringInterval);
										break;
								}

								if(AppLogic.AppConfigBool("Recurring.LimitCustomerToOneOrder"))
								{
									int MigrateDays = RecurringOrderMgr.ProcessAutoBillMigrateExisting(customer.CustomerID);
									if(MigrateDays != 0)
									{
										NextRecurringShipDate = NextRecurringShipDate.AddDays((double)MigrateDays);
									}
								}

								DB.ExecuteSQL("update ShoppingCart set BillingAddressID=" + UseBillingAddress.AddressID.ToString() + ",RecurringIndex=1, CartType=" + ((int)CartTypeEnum.RecurringCart).ToString() + ", CreatedOn=" + DB.DateQuote(Localization.ToDBShortDateString(DateTime.Parse(ThisOrderDate))) + ", NextRecurringShipDate=" + DB.DateQuote(Localization.ToDBShortDateString(NextRecurringShipDate)) + ", OriginalRecurringOrderNumber=" + OrderNumber.ToString() + " where (OriginalRecurringOrderNumber is null or OriginalRecurringOrderNumber=0) and VariantID=" + c.VariantID.ToString() + " and CartType=" + ((int)CartTypeEnum.ShoppingCart).ToString() + " and customerid=" + customer.CustomerID.ToString());
							}
						}
						DB.ExecuteSQL("update kitcart set CartType=" + ((int)CartTypeEnum.RecurringCart).ToString() + ", CreatedOn=" + DB.DateQuote(Localization.ToDBShortDateString(DateTime.Parse(ThisOrderDate))) + ", OriginalRecurringOrderNumber=" + OrderNumber.ToString() + " where (OriginalRecurringOrderNumber is null or OriginalRecurringOrderNumber=0) and VariantID in (" + RecurringVariantsList + ") and CartType=" + ((int)CartTypeEnum.ShoppingCart).ToString() + " and customerid=" + customer.CustomerID.ToString());



						//Recurring PayPal Express logic
						ExpressAPIType expressApiType = PayPalController.GetAppropriateExpressType();

						if((PM == AppLogic.ro_PMPayPalExpress && expressApiType == ExpressAPIType.PayPalExpress))
						{
							String ecRecurringProfileStatus = String.Empty;

							if(PM == AppLogic.ro_PMPayPalExpress)
								ecRecurringProfileStatus = MakeExpressCheckoutRecurringProfile(cart, OrderNumber, CAVV, ECI, NextRecurringShipDate);

							if(ecRecurringProfileStatus != AppLogic.ro_OK)
							{
								try
								{
									// send email notification to admin
									string emailSubject = String.Format("{0} - PayPal recurring subscription creation failed.", AppLogic.AppConfig("StoreName"));
									string emailBody = String.Format("Creation of a PayPal recurring subscription for order number {0} failed.  The initial order was completed.", OrderNumber.ToString());

									if(!AppLogic.AppConfigBool("TurnOffStoreAdminEMailNotifications"))
									{
										String SendToList = AppLogic.AppConfig("GotOrderEMailTo").Replace(",", ";");
										if(SendToList.IndexOf(';') != -1)
										{
											foreach(String s in SendToList.Split(';'))
											{
												AppLogic.SendMail(subject: emailSubject,
													body: emailBody + AppLogic.AppConfig("MailFooter"),
													useHtml: true,
													fromAddress: AppLogic.AppConfig("GotOrderEMailFrom"),
													fromName: AppLogic.AppConfig("GotOrderEMailFromName"),
													toAddress: s.Trim(),
													toName: s.Trim(),
													bccAddresses: String.Empty,
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
												bccAddresses: String.Empty,
												server: AppLogic.MailServer());
										}
									}
								}
								catch
								{
									SysLog.LogMessage(String.Format("Creation of a PayPal recurring subscription for order number {0} failed.  The initial order was completed.",
										OrderNumber.ToString()),
										ecRecurringProfileStatus,
										MessageTypeEnum.Informational,
										MessageSeverityEnum.Message);
								}
							}
						}

						if(AppLogic.AppConfigBool("Recurring.UseGatewayInternalBilling") && RecurringSubscriptionID.Length == 0
							&& (GW == "USAEPAY" || GW == "AUTHORIZENET" || GW == Gateway.ro_GWPAYFLOWPRO || GW == Gateway.ro_GWPAYPALEMBEDDEDCHECKOUT || GW == Gateway.ro_GWPAYPAL)
							&& PM == AppLogic.ro_PMCreditCard)
						{
							// Call gateway recurring subscription setup here to get SubscriptionID, etc
							// NOTES:
							//
							// 1) We have already charged the customer's card for this initial (starting) occurrence as part of creating this order prior to this point
							//
							// 2) ALL Cart items must have exact same recurring interval at this point!! If a single cart contained multiple
							//    products with different recurring intervals, they need to have been split out into different carts & orders
							//    before this point
							//
							// 3) TBD it is UNCLEAR at this point what to do if the create subscription call fails, as we've already created the
							//    order and charged the customer
							//
							CartItem firstcartrecurringitem = ((CartItem)cart.CartItems[0]);
							foreach(CartItem c in cart.CartItems)
							{
								if(c.IsRecurring)
								{
									firstcartrecurringitem = c;
									break;
								}
							}

							// This works if the cart has a single recurring item, or multiple items with same schedule.
							// TBD handling carts with multiple recurring items with differing schedules.
							ShoppingCart cartRecur = new ShoppingCart(customer.SkinID, customer, CartTypeEnum.RecurringCart, OrderNumber,
									false); // false will load recurring items that are not due yet, which we need to do here

							Decimal CartTotalRecur = Decimal.Round(cartRecur.Total(true), 2, MidpointRounding.AwayFromZero);
							Decimal RecurringAmount = CartTotalRecur - CommonLogic.IIF(cartRecur.Coupon.CouponType == CouponTypeEnum.GiftCard, CommonLogic.IIF(CartTotalRecur < cartRecur.Coupon.DiscountAmount, CartTotalRecur, cartRecur.Coupon.DiscountAmount), 0);

							String RecurringSubscriptionSetupStatus = String.Empty;
							String RecurringSubscriptionCommand = String.Empty;
							String RecurringSubscriptionResult = String.Empty;

							// dynamically load the gateway processor class via the name

							GatewayProcessor processor = GatewayLoader.GetProcessor(GW);

							if(processor != null)
							{
								if(PM == AppLogic.ro_PMPayPalExpress)
									XID = AuthorizationTransID;

								if(GW == Gateway.ro_GWPAYPAL && PM == AppLogic.ro_PMCreditCard)
									XID = CardExtraCode;


								RecurringSubscriptionSetupStatus = processor.RecurringBillingCreateSubscription(firstcartrecurringitem.ProductName,
													customer,
													UseBillingAddress,
													UseShippingAddress,
													RecurringAmount,
													NextRecurringShipDate,
													firstcartrecurringitem.RecurringInterval,
													firstcartrecurringitem.RecurringIntervalType,
													OrderNumber,
													XID,
													TransactionContext,
													out RecurringSubscriptionID,
													out RecurringSubscriptionCommand,
													out RecurringSubscriptionResult);
							}

							// wipe card #, if not done by the gateway, just to be safe
							if(RecurringSubscriptionCommand.Length != 0 && UseBillingAddress != null && UseBillingAddress.CardNumber != null && UseBillingAddress.CardNumber.Length > 0)
							{
								RecurringSubscriptionCommand = RecurringSubscriptionCommand.Replace(UseBillingAddress.CardNumber, AppLogic.SafeDisplayCardNumber(UseBillingAddress.CardNumber, "Orders", 0));
							}
							if(RecurringSubscriptionResult.Length != 0 && UseBillingAddress != null && UseBillingAddress.CardNumber != null && UseBillingAddress.CardNumber.Length > 0)
							{
								RecurringSubscriptionResult = RecurringSubscriptionResult.Replace(UseBillingAddress.CardNumber, AppLogic.SafeDisplayCardNumber(UseBillingAddress.CardNumber, "Orders", 0));
							}
							if(RecurringSubscriptionSetupStatus == AppLogic.ro_OK)
							{
								DB.ExecuteSQL("update ShoppingCart set RecurringSubscriptionID=" + DB.SQuote(RecurringSubscriptionID) + " where OriginalRecurringOrderNumber=" + OrderNumber.ToString() + " and CartType=" + ((int)CartTypeEnum.RecurringCart).ToString());
								String sqlsub = String.Format("update Orders set RecurringSubscriptionID={0}, RecurringSubscriptionCommand={1}, RecurringSubscriptionResult={2} where OrderNumber={3}", DB.SQuote(RecurringSubscriptionID), DB.SQuote(RecurringSubscriptionCommand), DB.SQuote(RecurringSubscriptionResult), OrderNumber.ToString());
								DB.ExecuteSQL(sqlsub);
							}
							else
							{
								// Card has been processed, Recurring subscription has failed. Admin needs to be notified.
								SysLog.LogMessage(
									String.Format("Recurring subscription creation failed for order: {0}", OrderNumber.ToString()),
									RecurringSubscriptionSetupStatus,
									MessageTypeEnum.Informational,
									MessageSeverityEnum.Error);

								//Send admin email, recurring sub needs to be set up manually
								AppLogic.SendMail(
subject: String.Format("Recurring subscription creation failed for order: {0}", OrderNumber.ToString()),
									body: "Recurring subscription needs to be manually created. - " + RecurringSubscriptionSetupStatus,
									useHtml: false);
								RecurringSubscriptionResult = "FAILED\r\n" + RecurringSubscriptionSetupStatus;
								String sqlsub = String.Format("update Orders set RecurringSubscriptionCommand={0}, RecurringSubscriptionResult={1} where OrderNumber={2}", DB.SQuote(RecurringSubscriptionCommand), DB.SQuote(RecurringSubscriptionResult), OrderNumber.ToString());
								DB.ExecuteSQL(sqlsub);
							}
						}

					}
					else
					{

						if(RecurringSubscriptionID.Length != 0 && XID.Length != 0)
						{
							DB.ExecuteSQL("update Orders set AuthorizationPNREF=" + DB.SQuote(XID) + ", ParentOrderNumber=" + cart.OriginalRecurringOrderNumber.ToString() + " where OrderNumber=" + OrderNumber.ToString());
						}
						else
						{
							DB.ExecuteSQL("update Orders set ParentOrderNumber=" + cart.OriginalRecurringOrderNumber.ToString() + " where OrderNumber=" + OrderNumber.ToString());
						}

						// this is a REPEAT recurring order process:
						NextRecurringShipDate = System.DateTime.Now.AddMonths(1); // default just for safety, should never be used, as it should be reset below!

						// don't reset their ship dates to today plus interval, use what "would" have been the proper order date
						// for this order, and then add the interval (in case the store administrator is processing this order early!)
						DateTime ProperNextRecurringShipDateStartsOn = ((CartItem)cart.CartItems[0]).NextRecurringShipDate;
						if(ProperNextRecurringShipDateStartsOn.Equals(System.DateTime.MinValue))
						{
							// safety check:
							ProperNextRecurringShipDateStartsOn = System.DateTime.Now;
						}
						
						foreach(CartItem c in cart.CartItems)
						{
							switch(c.RecurringIntervalType)
							{
								case DateIntervalTypeEnum.Day:
									NextRecurringShipDate = ProperNextRecurringShipDateStartsOn.AddDays(c.RecurringInterval);
									break;
								case DateIntervalTypeEnum.Week:
									NextRecurringShipDate = ProperNextRecurringShipDateStartsOn.AddDays(7 * c.RecurringInterval);
									break;
								case DateIntervalTypeEnum.Month:
									NextRecurringShipDate = ProperNextRecurringShipDateStartsOn.AddMonths(c.RecurringInterval);
									break;
								case DateIntervalTypeEnum.Year:
									NextRecurringShipDate = ProperNextRecurringShipDateStartsOn.AddYears(c.RecurringInterval);
									break;
								case DateIntervalTypeEnum.NumberOfDays:
									NextRecurringShipDate = ProperNextRecurringShipDateStartsOn.AddDays(c.RecurringInterval);
									break;
								case DateIntervalTypeEnum.Weekly:
									NextRecurringShipDate = ProperNextRecurringShipDateStartsOn.AddDays(7);
									break;
								case DateIntervalTypeEnum.BiWeekly:
									NextRecurringShipDate = ProperNextRecurringShipDateStartsOn.AddDays(14);
									break;
								case DateIntervalTypeEnum.EveryFourWeeks:
									NextRecurringShipDate = ProperNextRecurringShipDateStartsOn.AddDays(28);
									break;
								case DateIntervalTypeEnum.Monthly:
									NextRecurringShipDate = ProperNextRecurringShipDateStartsOn.AddMonths(1);
									break;
								case DateIntervalTypeEnum.Quarterly:
									NextRecurringShipDate = ProperNextRecurringShipDateStartsOn.AddMonths(3);
									break;
								case DateIntervalTypeEnum.SemiYearly:
									NextRecurringShipDate = ProperNextRecurringShipDateStartsOn.AddMonths(6);
									break;
								case DateIntervalTypeEnum.Yearly:
									NextRecurringShipDate = ProperNextRecurringShipDateStartsOn.AddYears(1);
									break;
								default:
									NextRecurringShipDate = ProperNextRecurringShipDateStartsOn.AddMonths(c.RecurringInterval);
									break;
							}
							DB.ExecuteSQL("update ShoppingCart set BillingAddressID=" + UseBillingAddress.AddressID.ToString() + ",RecurringIndex=RecurringIndex+1, NextRecurringShipDate=" + DB.DateQuote(Localization.ToDBShortDateString(NextRecurringShipDate)) + " where originalrecurringordernumber=" + cart.OriginalRecurringOrderNumber.ToString() + " and VariantID=" + c.VariantID.ToString() + " and CartType=" + ((int)CartTypeEnum.RecurringCart).ToString() + " and customerid=" + customer.CustomerID.ToString());
						}
					}
				}

				// clear CouponCode
				if(AppLogic.AppConfigBool("ClearCouponAfterOrdering"))
				{
					DB.ExecuteSQL("update customer set CouponCode=NULL where customerid=" + customer.CustomerID.ToString());
				}

				PromotionManager.RemoveUnusedPromotionsForOrder(OrderNumber);

				//  now we have to update their quantity discount fields in their "order cart", so we have them available for later
				// receipts (e.g. you may delete that quantity discount table tomorrow, but the customer wants to get their receipt again
				// next month, and we would have to reproduce the exact order conditions that they had on order, and we couldn't do that
				// if the discount table has been deleted, unless we store the discount info along with the order)
				if(QuantityDiscount.CustomerLevelAllowsQuantityDiscounts(customer.CustomerLevelID))
				{
					DB.ExecuteStoredProcInt("aspdnsf_updOrderitemQuantityDiscount", new SqlParameter[] { DB.CreateSQLParameter("@OrderNumber", SqlDbType.Int, 4, OrderNumber, ParameterDirection.Input) });
				}

				// now update their CustomerLevel info in the order record, if necessary:
				if(customer.CustomerID != 0)
				{
					using(SqlConnection con = new SqlConnection(DB.GetDBConn()))
					{
						con.Open();
						using(IDataReader rs_l = DB.GetRS("select * from CustomerLevel   with (NOLOCK)  where CustomerLevelID=" + customer.CustomerLevelID.ToString(), con))
						{
							if(rs_l.Read())
							{
								StringBuilder sql_l = new StringBuilder(4096);
								sql_l.Append("update orders set ");
								sql_l.Append("LevelID=" + customer.CustomerLevelID.ToString() + ",");
								sql_l.Append("LevelName=" + DB.SQuote(customer.CustomerLevelName) + ",");
								sql_l.Append("LevelDiscountPercent=" + Localization.DecimalStringForDB(DB.RSFieldDecimal(rs_l, "LevelDiscountPercent")) + ",");
								sql_l.Append("LevelDiscountAmount=" + Localization.CurrencyStringForDBWithoutExchangeRate(DB.RSFieldDecimal(rs_l, "LevelDiscountAmount")) + ",");
								sql_l.Append("LevelHasFreeShipping=" + CommonLogic.IIF(DB.RSFieldBool(rs_l, "LevelHasFreeShipping"), 1, 0).ToString() + ",");
								sql_l.Append("LevelAllowsQuantityDiscounts=" + CommonLogic.IIF(DB.RSFieldBool(rs_l, "LevelAllowsQuantityDiscounts"), 1, 0).ToString() + ",");
								sql_l.Append("LevelHasNoTax=" + CommonLogic.IIF(DB.RSFieldBool(rs_l, "LevelHasNoTax"), 1, 0).ToString() + ",");
								sql_l.Append("LevelAllowsCoupons=" + CommonLogic.IIF(DB.RSFieldBool(rs_l, "LevelAllowsCoupons"), 1, 0).ToString() + ",");
								sql_l.Append("LevelDiscountsApplyToExtendedPrices=" + CommonLogic.IIF(DB.RSFieldBool(rs_l, "LevelDiscountsApplyToExtendedPrices"), 1, 0).ToString() + " ");
								sql_l.Append("where OrderNumber=" + OrderNumber.ToString());
								DB.ExecuteSQL(sql_l.ToString());
							}
						}
					}
				}

				// call-out order packing add-ins
				if(AppLogic.AppConfigBool("AvalaraTax.Enabled"))
				{
					//create tax transaction for the order
					Order order = new Order(OrderNumber);

					//Add Shipping to Order Collection
					foreach(int ShipToId in cart.CartItems.Select(ci => ci.ShippingAddressID).Distinct())
					{
						CartItemCollection ciSingleShipmentCollection = new CartItemCollection();
						ciSingleShipmentCollection.AddRange(cart.CartItems.Where(ci => ci.ShippingAddressID == ShipToId).ToList());

						Address shipToAddress = new Address();
						shipToAddress.LoadFromDB(ShipToId);

						MultiShipOrder_Shipment shipment = new MultiShipOrder_Shipment();
						shipment.DestinationAddress = shipToAddress.AsXml;
						shipment.OrderNumber = OrderNumber;
						shipment.ShippingAmount = Prices.ShippingTotal(true, true, ciSingleShipmentCollection, customer, cart.OrderOptions);
						shipment.ShippingMethodId = ciSingleShipmentCollection[0].ShippingMethodID;
						shipment.ShippingAddressId = ciSingleShipmentCollection[0].ShippingAddressID;
						shipment.BillingAddressId = ciSingleShipmentCollection[0].BillingAddressID;

						shipment.Save();
					}

					try
					{
						// Added try/catch because it blew up the cart if jurisdiction could not be determined but did not prevent the order
						AvaTax avaTax = new AvaTax();
						avaTax.OrderPlaced(order);
					}
					catch(Exception Ex)
					{
						SysLog.LogException(Ex, MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
					}
				}

				if(SetToCapturedState)
				{
					ProcessOrderAsCaptured(OrderNumber);
				}

				AppLogic.eventHandler("NewOrder").CallEvent("&NewOrder=true&OrderNumber=" + OrderNumber.ToString());
			}
			#endregion
			return status;
		}

		// cart could be null, so be careful
		private static String ProcessCard(ShoppingCart cart, String GateWay, int CustomerID, int OrderNumber, Address UseBillingAddress, String CardExtraCode, Address UseShippingAddress, Decimal OrderTotal, bool useLiveTransactions, String CAVV, String ECI, String XID, out String AVSResult, out String AuthorizationResult, out String AuthorizationCode, out String AuthorizationTransID, out String TransactionCommand, out String TransactionResponse, out String GatewayUsed)
		{
			String GW = DetermineGatwayToUse(GateWay);
			GatewayUsed = DetermineGatwayToUse(GateWay);
			String Status = "NO GATEWAY SET, GATEWAY=" + DB.SQuote(GW);

			AVSResult = String.Empty;
			AuthorizationResult = String.Empty;
			AuthorizationCode = String.Empty;
			AuthorizationTransID = String.Empty;
			TransactionCommand = String.Empty;
			TransactionResponse = String.Empty;

			if(cart != null)
			{
				if(!cart.ThisCustomer.IsAdminUser && (AppLogic.ExceedsFailedTransactionsThreshold(cart.ThisCustomer) || AppLogic.IPIsRestricted(cart.ThisCustomer.LastIPAddress)))
				{
					return "Order Declined. Please contact Customer Service.";
				}
			}
			if(UseBillingAddress.PaymentMethodLastUsed.StartsWith(AppLogic.ro_PMPayPal, StringComparison.InvariantCultureIgnoreCase))
			{
				Status = PayPalController.ProcessPaypal(OrderNumber, CustomerID, OrderTotal, useLiveTransactions, AppLogic.ro_TXModeAuthCapture, UseBillingAddress, UseShippingAddress, CAVV, ECI, XID, out AVSResult, out AuthorizationResult, out AuthorizationCode, out AuthorizationTransID, out TransactionCommand, out TransactionResponse);
			}
			else
			{
				GatewayTransaction gwtran = new GatewayTransaction(OrderNumber, CustomerID, OrderTotal, UseBillingAddress, UseShippingAddress, CAVV, CardExtraCode, ECI, XID);

				// run the transaction.
				// all exception handling occurs within the transaction class
				gwtran.Process();

				AVSResult = gwtran.AVSResult;
				AuthorizationResult = gwtran.AuthorizationResult;
				AuthorizationCode = gwtran.AuthorizationCode;
				AuthorizationTransID = gwtran.AuthorizationTransactionID;
				TransactionCommand = gwtran.TransactionCommand;
				TransactionResponse = gwtran.TransactionResponse;
				Status = gwtran.Status;
				GatewayUsed = gwtran.GatewayUsed;

			}

			if(Status != AppLogic.ro_OK && Status != AppLogic.ro_3DSecure)
			{
				// record failed TX:
				try
				{
					String txout = TransactionCommand;
					String txresponse = TransactionResponse;
					if(UseBillingAddress.CardNumber != null && UseBillingAddress.CardNumber.Length != 0)
					{
						String tmp = AppLogic.SafeDisplayCardNumber(UseBillingAddress.CardNumber, "Address", UseBillingAddress.AddressID);
						if(!string.IsNullOrEmpty(txout) && txout.Length != 0)
						{
							txout = txout.Replace(UseBillingAddress.CardNumber, tmp);
						}
						if(!string.IsNullOrEmpty(txresponse) && txresponse.Length != 0)
						{
							txresponse = txresponse.Replace(UseBillingAddress.CardNumber, tmp);
						}
					}
					if(CardExtraCode != null && CardExtraCode.Length != 0)
					{
						String tmp = AppLogic.SafeDisplayCardExtraCode(CardExtraCode);
						if(!string.IsNullOrEmpty(txout) && txout.Length != 0)
						{
							txout = txout.Replace(CardExtraCode, tmp);
						}
						if(!string.IsNullOrEmpty(txresponse) && txresponse.Length != 0)
						{
							txresponse = txresponse.Replace(CardExtraCode, tmp);
						}
					}
					String IP = "";
					if(cart != null)
					{
						IP = cart.ThisCustomer.LastIPAddress;
					}
					String sql = "insert into FailedTransaction(CustomerID,OrderNumber,IPAddress,OrderDate,PaymentGateway,PaymentMethod,TransactionCommand,TransactionResult) values(" + CustomerID.ToString() + "," + OrderNumber.ToString() + "," + DB.SQuote(IP) + ",getdate()," + DB.SQuote(GW) + "," + DB.SQuote(AppLogic.ro_PMCreditCard) + "," + DB.SQuote(txout) + "," + DB.SQuote(txresponse) + ")";
					DB.ExecuteSQL(sql);
				}
				catch
				{
					throw new Exception("There is a problem with your gateway configuration! Check gateway settings or contact Support for assistance.");
				}
			}
			return Status;
		}

		public static string DispatchCapture(String GW, int OrderNumber)
		{
			Order o = new Order(OrderNumber, Localization.GetDefaultLocale());
			String Status = string.Empty;

			if(GW == Gateway.ro_GWAMAZONPAYMENTS)
			{
				// handle AmazonPayments as "pseudo gateway"
				var orderTrackingDetail = new AmazonPaymentsOrderTrackingDetail();
				var processor = new AmazonPaymentsGateway();
				var serializer = new AmazonPaymentsOrderTrackingDetailSerializer();
				var serializedAmazonOrderTrackingDetail = string.Empty;

				using(var conn = new SqlConnection(DB.GetDBConn()))
				{
					conn.Open();
					using(var rs = DB.GetRS("SELECT TransactionCommand FROM Orders(NOLOCK) WHERE OrderNumber=@orderNumber", new[] { new SqlParameter("orderNumber", o.OrderNumber.ToString()) }, conn))
					{
						if(rs.Read())
						{
							try
							{
								orderTrackingDetail = serializer.DeserializeAmazonOrderTrackingDetail(DB.RSField(rs, "TransactionCommand"));
							}
							catch
							{
								return "Amazon order tracking detail not found. Order was not refunded.";
							}
						}
					}
				}

				Status = processor.CaptureOrder(ref orderTrackingDetail, o.OrderNumber, o.CustomerID, o.Total());

				if(Status == AppLogic.ro_OK)
				{
					serializedAmazonOrderTrackingDetail = serializer.SerializeAmazonOrderTrackingDetail(orderTrackingDetail);

					string sql = string.Format("UPDATE Orders SET TransactionCommand={0}, AuthorizationPNREF={1}, AuthorizationResult={2}, AuthorizationCode={3}, VoidTXCommand={4}, VoidTXResult={5}, CaptureTXCommand={6}, CaptureTXResult={7}, RefundTXCommand={8}, RefundTXResult={9} WHERE OrderNumber={10}",
						DB.SQuote(serializedAmazonOrderTrackingDetail), DB.SQuote(orderTrackingDetail.Authorization.Id), DB.SQuote(orderTrackingDetail.Authorization.State), DB.SQuote(orderTrackingDetail.Authorization.ReasonCode), DB.SQuote(orderTrackingDetail.Cancel.Id), DB.SQuote(orderTrackingDetail.Cancel.State), DB.SQuote(orderTrackingDetail.Capture.Id), DB.SQuote(orderTrackingDetail.Capture.State), DB.SQuote(orderTrackingDetail.Refund.Id), DB.SQuote(orderTrackingDetail.Refund.State), o.OrderNumber.ToString());

					DB.ExecuteSQL(sql);
				}
			}
			else
			{
				// dynamically load the gateway processor class via the name
				GatewayProcessor processor = GatewayLoader.GetProcessor(GW);
				if(processor != null)
				{
					Status = processor.CaptureOrder(o);
				}
				else
				{
					Status = "Unknown PaymentGateway in Capture";
				}
			}

			if(Status.ToUpper(CultureInfo.InvariantCulture) == AppLogic.ro_OK)
			{
				Gateway.ProcessOrderAsCaptured(OrderNumber);
			}
			return Status;
		}

		public static String ForceCapture(int OrderNumber)
		{
			// update transaction state
			Gateway.ProcessOrderAsCaptured(OrderNumber);
			return AppLogic.ro_OK;
		}

		public static String ProcessRefund(int CustomerID, int OriginalOrderNumber, int NewOrderNumber, Decimal RefundAmount, String RefundReason, Address UseBillingAddress)
		{
			// get GW for this order, not the generic GW 
			string GW = String.Empty;
			decimal OrderTotal = 0.0M;
			int CouponType = 0;
			string CouponCode = "";
			decimal CouponDiscountAmount = 0.0M;
			string Status = string.Empty;

			using(SqlConnection con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(IDataReader rs = DB.GetRS("Select PaymentGateway, PaymentMethod, OrderTotal, OrderTax, CouponType, CouponCode, CouponDiscountAmount from Orders  with (NOLOCK)  where OrderNumber=@orderNumber", new[] { new SqlParameter("orderNumber", OriginalOrderNumber.ToString()) }, con))
				{
					if(rs.Read())
					{
						GW = AppLogic.CleanPaymentGateway(DB.RSField(rs, "PaymentGateway"));

						string PM = AppLogic.CleanPaymentMethod(DB.RSField(rs, "PaymentMethod"));
						if(PM == AppLogic.ro_PMMicropay)
						{
							GW = Gateway.ro_GWMICROPAY;
						}
						else if(PM == AppLogic.ro_PMAmazonPayments)
						{
							GW = Gateway.ro_GWAMAZONPAYMENTS;
						}
						else if(PM == AppLogic.ro_PMPayPalEmbeddedCheckout)
						{
							GW = Gateway.ro_GWPAYFLOWPRO;
						}

						OrderTotal = DB.RSFieldDecimal(rs, "OrderTotal");
						CouponType = DB.RSFieldInt(rs, "CouponType");
						CouponCode = DB.RSField(rs, "CouponCode");
						CouponDiscountAmount = DB.RSFieldDecimal(rs, "CouponDiscountAmount");
					}
				}
			}


			if(GW == "")
			{
				GW = AppLogic.ActivePaymentGatewayCleaned();
				Status = "NO GATEWAY SET, GATEWAY=" + DB.SQuote(GW);
			}


			if(GW == Gateway.ro_GWAMAZONPAYMENTS)
			{
				// handle AmazonPayments as "pseudo gateway"
				var orderTrackingDetail = new AmazonPaymentsOrderTrackingDetail();
				var processor = new AmazonPaymentsGateway();
				var serializer = new AmazonPaymentsOrderTrackingDetailSerializer();
				var serializedAmazonOrderTrackingDetail = string.Empty;

				using(var conn = new SqlConnection(DB.GetDBConn()))
				{
					conn.Open();
					using(var rs = DB.GetRS("SELECT TransactionCommand FROM Orders(NOLOCK) WHERE OrderNumber=" + OriginalOrderNumber.ToString(), conn))
					{
						if(rs.Read())
						{
							try
							{
								orderTrackingDetail = serializer.DeserializeAmazonOrderTrackingDetail(DB.RSField(rs, "TransactionCommand"));
							}
							catch
							{
								return "Amazon order tracking detail not found. Order was not refunded.";
							}
						}
					}
				}

				Status = processor.RefundOrder(ref orderTrackingDetail, OriginalOrderNumber, RefundAmount, RefundReason);

				if(Status == AppLogic.ro_OK)
				{
					serializedAmazonOrderTrackingDetail = serializer.SerializeAmazonOrderTrackingDetail(orderTrackingDetail);

					string sql = string.Format("UPDATE Orders SET TransactionCommand={0}, AuthorizationPNREF={1}, AuthorizationResult={2}, AuthorizationCode={3}, VoidTXCommand={4}, VoidTXResult={5}, CaptureTXCommand={6}, CaptureTXResult={7}, RefundTXCommand={8}, RefundTXResult={9}, AuthorizedOn=GETDATE() WHERE OrderNumber={10}",
						DB.SQuote(serializedAmazonOrderTrackingDetail), DB.SQuote(orderTrackingDetail.Authorization.Id), DB.SQuote(orderTrackingDetail.Authorization.State), DB.SQuote(orderTrackingDetail.Authorization.ReasonCode), DB.SQuote(orderTrackingDetail.Cancel.Id), DB.SQuote(orderTrackingDetail.Cancel.State), DB.SQuote(orderTrackingDetail.Capture.Id), DB.SQuote(orderTrackingDetail.Capture.State), DB.SQuote(orderTrackingDetail.Refund.Id), DB.SQuote(orderTrackingDetail.Refund.State), OriginalOrderNumber.ToString());

					DB.ExecuteSQL(sql);
				}
			}
			else
			{
				// dynamically load the gateway processor class via the name
				GatewayProcessor processor = GatewayLoader.GetProcessor(GW);
				if(processor != null)
				{
					Status = processor.RefundOrder(OriginalOrderNumber, NewOrderNumber, RefundAmount, RefundReason, UseBillingAddress);
				}
				else
				{
					Status = "Unknown PaymentGateway in RefundOrder";
				}
			}

			if(Status.ToUpper(CultureInfo.InvariantCulture) == AppLogic.ro_OK)
			{
				// was this a full refund) {
				if((RefundAmount == System.Decimal.Zero || RefundAmount == OrderTotal))
				{
					// make sure inventory was restored. safe to call repeatedly. proc protects against deducting twice
					DB.ExecuteSQL("aspdnsf_AdjustInventory " + OriginalOrderNumber.ToString() + ",1");

					DecrementMicropayProductsInOrder(OriginalOrderNumber);

					// update transactionstate
					DB.ExecuteSQL("update Orders set RefundReason=" + DB.SQuote(RefundReason) + ", TransactionState=" + DB.SQuote(AppLogic.ro_TXStateRefunded) + ", RefundedOn=getdate(), IsNew=0 where OrderNumber=" + OriginalOrderNumber.ToString());

					//Invalidate GiftCards ordered on this order
					GiftCards GCs = new GiftCards(OriginalOrderNumber, GiftCardCollectionFilterType.OrderNumber);
					foreach(GiftCard gc in GCs)
					{
						gc.GiftCardTransactions.Add(GiftCardUsageTransaction.CreateTransaction(gc.GiftCardID, GiftCardUsageReasons.FundsRemovedByAdmin, 0, 0, gc.Balance, ""));
						gc.UpdateCard(null, null, null, null, 1, null, null, null, null, null, null, null, null, null, null);
					}

					//Restore Amount to coupon used in paying for the order
					if((CouponTypeEnum)CouponType == CouponTypeEnum.GiftCard)
					{
						GiftCard gc = new GiftCard(CouponCode);
						if(gc.GiftCardID != 0)
						{
							gc.GiftCardTransactions.Add(GiftCardUsageTransaction.CreateTransaction(gc.GiftCardID, GiftCardUsageReasons.FundsAddedByAdmin, 0, 0, CouponDiscountAmount, ""));
						}
					}

					if(AppLogic.AppConfigBool("AvalaraTax.Enabled"))
					{
						Address originAddress = new Address
						{
							Address1 = AppLogic.AppConfig("RTShipping.OriginAddress"),
							Address2 = AppLogic.AppConfig("RTShipping.OriginAddress2"),
							City = AppLogic.AppConfig("RTShipping.OriginCity"),
							Country = AppLogic.AppConfig("RTShipping.OriginCountry"),
							State = AppLogic.AppConfig("RTShipping.OriginState"),
							Suite = String.Empty,
							Zip = AppLogic.AppConfig("RTShipping.OriginZip"),

							//pass in the ShippingTaxClassID
							NickName = AppLogic.AppConfigUSInt("ShippingTaxClassID").ToString(),
						};

						AvaTax avaTax = new AvaTax();
						avaTax.IssueRefund(new Order(OriginalOrderNumber), originAddress, 0);
					}
				}
				else
				{
					if(AppLogic.AppConfigBool("AvalaraTax.Enabled"))
					{
						Address originAddress = new Address
						{
							Address1 = AppLogic.AppConfig("RTShipping.OriginAddress"),
							Address2 = AppLogic.AppConfig("RTShipping.OriginAddress2"),
							City = AppLogic.AppConfig("RTShipping.OriginCity"),
							Country = AppLogic.AppConfig("RTShipping.OriginCountry"),
							State = AppLogic.AppConfig("RTShipping.OriginState"),
							Suite = String.Empty,
							Zip = AppLogic.AppConfig("RTShipping.OriginZip"),

							//pass in the ShippingTaxClassID
							NickName = AppLogic.AppConfigUSInt("ShippingTaxClassID").ToString(),
						};

						AvaTax avaTax = new AvaTax();
						avaTax.IssueRefund(new Order(OriginalOrderNumber), originAddress, RefundAmount);
					}
				}
			}


			return Status;
		}

		public static String DispatchVoid(String GW, int OrderNumber)
		{
			int CouponType = 0;
			string CouponCode = "";
			decimal CouponDiscountAmount = 0.0M;

			using(SqlConnection con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(IDataReader rs = DB.GetRS("Select PaymentGateway, PaymentMethod, OrderTotal, CouponType, CouponCode, CouponDiscountAmount from Orders  with (NOLOCK)  where OrderNumber=" + OrderNumber.ToString(), con))
				{
					if(rs.Read())
					{
						CouponType = DB.RSFieldInt(rs, "CouponType");
						CouponCode = DB.RSField(rs, "CouponCode");
						CouponDiscountAmount = DB.RSFieldDecimal(rs, "CouponDiscountAmount");
					}
				}
			}

			String Status = String.Empty;

			if(GW == Gateway.ro_GWAMAZONPAYMENTS)
			{
				// handle AmazonPayments as "pseudo gateway"
				var orderTrackingDetail = new AmazonPaymentsOrderTrackingDetail();
				var processor = new AmazonPaymentsGateway();
				var serializer = new AmazonPaymentsOrderTrackingDetailSerializer();
				var serializedAmazonOrderTrackingDetail = string.Empty;

				using(var conn = new SqlConnection(DB.GetDBConn()))
				{
					conn.Open();
					using(var rs = DB.GetRS("SELECT TransactionCommand FROM Orders(NOLOCK) WHERE OrderNumber=@orderNumber", new[] { new SqlParameter("orderNumber", OrderNumber.ToString()) }, conn))
					{
						if(rs.Read())
						{
							try
							{
								orderTrackingDetail = serializer.DeserializeAmazonOrderTrackingDetail(DB.RSField(rs, "TransactionCommand"));
							}
							catch
							{
								return "Amazon order tracking detail not found. Order was not refunded.";
							}
						}
					}
				}

				Status = processor.CancelOrder(ref orderTrackingDetail, OrderNumber);

				if(Status == AppLogic.ro_OK)
				{
					serializedAmazonOrderTrackingDetail = serializer.SerializeAmazonOrderTrackingDetail(orderTrackingDetail);

					string sql = string.Format("UPDATE Orders SET TransactionCommand={0}, AuthorizationPNREF={1}, AuthorizationResult={2}, AuthorizationCode={3}, VoidTXCommand={4}, VoidTXResult={5}, CaptureTXCommand={6}, CaptureTXResult={7}, RefundTXCommand={8}, RefundTXResult={9} WHERE OrderNumber={10}",
						DB.SQuote(serializedAmazonOrderTrackingDetail), DB.SQuote(orderTrackingDetail.Authorization.Id), DB.SQuote(orderTrackingDetail.Authorization.State), DB.SQuote(orderTrackingDetail.Authorization.ReasonCode), DB.SQuote(orderTrackingDetail.Cancel.Id), DB.SQuote(orderTrackingDetail.Cancel.State), DB.SQuote(orderTrackingDetail.Capture.Id), DB.SQuote(orderTrackingDetail.Capture.State), DB.SQuote(orderTrackingDetail.Refund.Id), DB.SQuote(orderTrackingDetail.Refund.State), OrderNumber.ToString());

					DB.ExecuteSQL(sql);
				}
			}
			else
			{
				// dynamically load the gateway processor class via the name
				GatewayProcessor processor = GatewayLoader.GetProcessor(GW);
				if(processor != null)
				{
					Status = processor.VoidOrder(OrderNumber);
				}
				else
				{
					Status = "Unknown PaymentGateway in Void";
				}
			}

			if(Status.ToUpper(CultureInfo.InvariantCulture) == AppLogic.ro_OK)
			{
				AppLogic.eventHandler("OrderVoided").CallEvent("&OrderVoided=true&OrderNumber=" + OrderNumber.ToString());

				// make sure inventory was restored. safe to call repeatedly. proc protects against deducting twice
				DB.ExecuteSQL("aspdnsf_AdjustInventory " + OrderNumber.ToString() + ",1");

				DecrementMicropayProductsInOrder(OrderNumber);

				// update transactionstate
				DB.ExecuteSQL("update Orders set TransactionState=" + DB.SQuote(AppLogic.ro_TXStateVoided) + ", VoidedOn=getdate(), IsNew=0 where ordernumber=" + OrderNumber.ToString());

				//Invalidate GiftCards ordered on this order
				GiftCards GCs = new GiftCards(OrderNumber, GiftCardCollectionFilterType.OrderNumber);
				foreach(GiftCard gc in GCs)
				{
					gc.GiftCardTransactions.Add(GiftCardUsageTransaction.CreateTransaction(gc.GiftCardID, GiftCardUsageReasons.FundsRemovedByAdmin, 0, 0, gc.Balance, ""));
					gc.UpdateCard(null, null, null, null, 1, null, null, null, null, null, null, null, null, null, null);
				}

				//Restore Amount to coupon used in paying for the order
				if((CouponTypeEnum)CouponType == CouponTypeEnum.GiftCard)
				{
					GiftCard gc = new GiftCard(CouponCode);
					if(gc.GiftCardID != 0)
					{
						gc.GiftCardTransactions.Add(GiftCardUsageTransaction.CreateTransaction(gc.GiftCardID, GiftCardUsageReasons.FundsAddedByAdmin, 0, 0, CouponDiscountAmount, ""));
					}
				}
			}
			return Status;
		}

		public static void DecrementMicropayProductsInOrder(int OrderNumber)
		{
			Order ord = new Order(OrderNumber, Localization.GetDefaultLocale());
			//Update (subtract back from) the customer's Micropay balances if any was purchased as part of this order
			if(ord.TransactionIsCaptured())
			{
				int MicropayProductID = AppLogic.GetMicroPayProductID();
				int MicropayVariantID = AppLogic.GetProductsDefaultVariantID(MicropayProductID);
				decimal mpTotal = AppLogic.GetMicroPayBalance(ord.CustomerID);

				//Use the raw price for the amount because 
				// it may be discounted or on sale in the order
				decimal amount = AppLogic.GetVariantPrice(MicropayVariantID);
				foreach(CartItem c in ord.CartItems)
				{
					if(c.ProductID == MicropayProductID)
					{
						mpTotal -= (amount * c.Quantity);
					}
				}
				if(mpTotal < System.Decimal.Zero)
				{
					mpTotal = System.Decimal.Zero;
				}
				DB.ExecuteSQL(String.Format("update Customer set MicroPayBalance={0} where CustomerID={1}", Localization.CurrencyStringForDBWithoutExchangeRate(mpTotal), ord.CustomerID));
			}
		}

		public static string StartExpressCheckout(ShoppingCart cart, bool boolBypassOrderReview = false, IDictionary<string, string> checkoutOptions = null)
		{
			checkoutOptions = checkoutOptions ?? new Dictionary<string, string>();

			switch(PayPalController.GetAppropriateExpressType())
			{
				case ExpressAPIType.PayFlowPro:
					return PayFlowProController.StartEC(cart, boolBypassOrderReview, checkoutOptions);
				case ExpressAPIType.PayPalExpress:
				case ExpressAPIType.PayPalAcceleratedBording:
				case ExpressAPIType.NoValidAPIType:
				default:
					return PayPalController.StartEC(cart, boolBypassOrderReview, checkoutOptions);
			}
		}

		public static String GetExpressCheckoutDetails(String PayPalToken, int CustomerID)
		{
			switch(PayPalController.GetAppropriateExpressType())
			{
				case ExpressAPIType.PayFlowPro:
					return PayFlowProController.GetECDetails(PayPalToken, CustomerID);
				case ExpressAPIType.PayPalExpress:
				case ExpressAPIType.PayPalAcceleratedBording:
				case ExpressAPIType.NoValidAPIType:
				default:
					return PayPalController.GetECDetails(PayPalToken, CustomerID);
			}
		}

		public static String ProcessExpressCheckout(ShoppingCart cart, decimal OrderTotal, int OrderNumber, String PayPalToken, String PayerID, String TransactionMode, out String AuthorizationResult, out String AuthorizationTransID, out String Gateway)
		{
			switch(PayPalController.GetAppropriateExpressType())
			{
				case ExpressAPIType.PayFlowPro:
					Gateway = ro_GWPAYFLOWPRO;
					return PayFlowProController.ProcessEC(cart, OrderTotal, OrderNumber, PayPalToken, PayerID, TransactionMode, out AuthorizationResult, out AuthorizationTransID);
				case ExpressAPIType.PayPalExpress:
				case ExpressAPIType.PayPalAcceleratedBording:
				case ExpressAPIType.NoValidAPIType:
				default:
					Gateway = ro_GWPAYPAL;
					return PayPalController.ProcessEC(cart, OrderTotal, OrderNumber, PayPalToken, PayerID, TransactionMode, out AuthorizationResult, out AuthorizationTransID);
			}
		}

		public static String MakeExpressCheckoutRecurringProfile(ShoppingCart cart, int orderNumber, String payPalToken, String payerID, DateTime nextRecurringShipDate)
		{
			switch(PayPalController.GetAppropriateExpressType())
			{
				case ExpressAPIType.PayPalExpress:
				case ExpressAPIType.PayPalAcceleratedBording:
				case ExpressAPIType.NoValidAPIType:
				default:
					return PayPalController.MakeECRecurringProfile(cart, orderNumber, payPalToken, payerID, nextRecurringShipDate);
			}
		}


		// ----------------------------------------------------------------------------------------------------------------------------------------
		// the following routines are master order mgmt/state transition routines, located here to centralize the code & logic. These routines used
		// to be in orderframe.aspx.cs and other places. They are centralized here now so that the WSI can also use the same logic for
		// order management processing requests.
		// NOTE: the Order object in memory is NOT updated after the call, only the master db records/tables are updated! if you need current
		// in memory Order object status, you should load a new one after the call, if successful
		// ----------------------------------------------------------------------------------------------------------------------------------------

		// returns AppLogic.ro_OK on success, otherwise error description. on error, order state is unchanged.
		public static string OrderManagement_DoVoid(Order order)
		{
			return OrderManagement_DoVoid(order, false);
		}

		public static string OrderManagement_DoVoid(Order order, bool force)
		{
			var status = AppLogic.ro_OK;
			if(order.OrderNumber == 0 || order.IsEmpty)
				return "Order Not Found";

			if(order.VoidedOn == System.DateTime.MinValue)
			{
				var paymentMethod = AppLogic.CleanPaymentMethod(order.PaymentMethod);
				var gateway = AppLogic.CleanPaymentGateway(order.PaymentGateway);

				if(force)
				{
					status = ForceVoidStatus(order.OrderNumber, AppLogic.ro_TXStateForceVoided);
				}
				else if(paymentMethod == AppLogic.ro_PMCreditCard
					|| paymentMethod == AppLogic.ro_PMMicropay
					|| paymentMethod == AppLogic.ro_PMPayPalExpress
					|| paymentMethod == AppLogic.ro_PMCheckByMail
					|| paymentMethod == AppLogic.ro_PMAmazonPayments
					|| paymentMethod == AppLogic.ro_PMPayPalEmbeddedCheckout)
				{
					if(paymentMethod == AppLogic.ro_PMMicropay)
					{
						gateway = ro_GWMICROPAY;
					}
					else if(paymentMethod == AppLogic.ro_PMAmazonPayments)
					{
						gateway = ro_GWAMAZONPAYMENTS;
					}
					else if(paymentMethod == AppLogic.ro_PMPayPalEmbeddedCheckout)
					{
						gateway = ro_GWPAYFLOWPRO;
					}

					if(paymentMethod == AppLogic.ro_PMCheckByMail)
					{
						// internal payment methods do not get dispatched, but are forced
						status = ForceVoidStatus(order.OrderNumber);
					}
					else
					{
						status = DispatchVoid(gateway, order.OrderNumber);
					}

					if(status == AppLogic.ro_OK)
					{
						try
						{
							var customer = new Customer(order.CustomerID);
							AppLogic.SendMail(
								subject: "Order Has Been Voided",
								body: AppLogic.RunXmlPackage(
                                    "notification.ordercanceled.xml.config",
									null,
									null,
									1,
									string.Empty,
									"ordernumber=" + order.OrderNumber.ToString(),
									false,
									false),
								useHtml: true,
								fromAddress: AppLogic.AppConfig("MailMe_FromAddress"),
								fromName: AppLogic.AppConfig("MailMe_FromName"),
								toAddress: customer.EMail,
								toName: customer.FullName(),
								bccAddresses: string.Empty,
								server: AppLogic.MailServer());
						}
						catch(Exception exception)
						{
							SysLog.LogException(exception, MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
						}
					}
				}
				else
				{
					status = string.Format("Void not supported for the {0} payment method!", order.PaymentMethod);
				}
			}
			else
			{
				status = string.Format("The payment for this order was already voided on {0}.", Localization.ToNativeDateTimeString(order.VoidedOn));
			}

			if(AppLogic.AppConfigBool("AvalaraTax.Enabled"))
			{
				var avaTax = new AvaTax();
				avaTax.VoidTax(order);
			}

			return status;
		}

		public static string OrderManagement_UpdateTransaction(Order order)
		{
			var status = AppLogic.ro_OK;
			if(order.OrderNumber == 0 || order.IsEmpty)
				return "Order Not Found";

			return status;
		}


		// returns AppLogic.ro_OK on success, otherwise error description. on error, order state is unchanged.
		public static string OrderManagement_DoCapture(Order order)
		{
			var status = AppLogic.ro_OK;
			if(order.OrderNumber == 0 || order.IsEmpty)
				return "Order Not Found";

			var paymentMethod = AppLogic.CleanPaymentMethod(order.PaymentMethod);

			if(order.CapturedOn == System.DateTime.MinValue)
			{
				if(order.TransactionState == AppLogic.ro_TXStateAuthorized || order.TransactionState == AppLogic.ro_TXStatePending)
				{
					status = AppLogic.ro_OK; // will be ok for all non credit card orders

					var gateway = AppLogic.CleanPaymentGateway(order.PaymentGateway);

					if(paymentMethod == AppLogic.ro_PMCreditCard
						|| paymentMethod == AppLogic.ro_PMMicropay
						|| paymentMethod == AppLogic.ro_PMPayPalExpress
						|| paymentMethod == AppLogic.ro_PMAmazonPayments
						|| paymentMethod == AppLogic.ro_PMPayPalEmbeddedCheckout)
					{
						if(paymentMethod == AppLogic.ro_PMMicropay)
						{
							gateway = ro_GWMICROPAY;
						}
						else if(paymentMethod == AppLogic.ro_PMAmazonPayments)
						{
							gateway = ro_GWAMAZONPAYMENTS;
						}
						else if(paymentMethod == AppLogic.ro_PMPayPalEmbeddedCheckout)
						{
							gateway = ro_GWPAYFLOWPRO;
						}
						status = DispatchCapture(gateway, order.OrderNumber);
					}
					else
					{
						status = ForceCapture(order.OrderNumber);
					}
				}
				else
				{
					status = string.Format("The transaction state ({0}) is not AUTH.", order.TransactionState);
				}
			}
			else
			{
				status = string.Format("The payment for this order was already captured on {0}.", Localization.ToNativeDateTimeString(order.CapturedOn));
			}

			//Low inventory notification
			if(AppLogic.AppConfigBool("SendLowStockWarnings") && status == AppLogic.ro_OK)
			{
				var purchasedVariants = new List<int>();
				foreach(CartItem ci in order.CartItems)
				{
					purchasedVariants.Add(ci.VariantID);
				}

				AppLogic.LowInventoryWarning(purchasedVariants);
			}

			return status;
		}

		static public string OrderManagement_DoFullRefund(Order order, string refundReason)
		{
			var status = AppLogic.ro_OK;
			if(order.OrderNumber == 0 || order.IsEmpty)
				return "Order Not Found";

			string paymentMethod = AppLogic.CleanPaymentMethod(order.PaymentMethod);
			if(order.RefundedOn == System.DateTime.MinValue)
			{
				if(order.CapturedOn != System.DateTime.MinValue)
				{
					if(order.TransactionState == AppLogic.ro_TXStateCaptured)
					{
						if(paymentMethod == AppLogic.ro_PMCreditCard
							|| paymentMethod == AppLogic.ro_PMMicropay
							|| paymentMethod == AppLogic.ro_PMPayPalExpress
							|| paymentMethod == AppLogic.ro_PMAmazonPayments
							|| paymentMethod == AppLogic.ro_PMPayPalEmbeddedCheckout)
						{
							status = ProcessRefund(order.CustomerID, order.OrderNumber, 0, order.Total(), refundReason, null);
						}
						else
						{
							status = ForceRefundStatus(order.OrderNumber);
						}
						if(status == AppLogic.ro_OK)
						{
							try
							{
								var customer = new Customer(order.CustomerID, true);
								AppLogic.SendMail(
									subject: "Order Was Refunded",
									body: AppLogic.RunXmlPackage(
										AppLogic.AppConfig("XmlPackage.RefundEmail"),
										null,
										customer,
										1,
										string.Empty,
										"ordernumber=" + order.OrderNumber.ToString(),
										false,
										false),
									useHtml: true,
									fromAddress: AppLogic.AppConfig("MailMe_FromAddress"),
									fromName: AppLogic.AppConfig("MailMe_FromName"),
									toAddress: customer.EMail,
									toName: customer.FullName(),
									bccAddresses: string.Empty,
									server: AppLogic.MailServer());
							}
							catch(Exception exception)
							{
								SysLog.LogException(exception, MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
							}
						}
					}
					else
					{
						status = string.Format(
							"This transaction has not yet been Captured. Use Void if required. The transaction state ({0}) is not {1}.",
							order.TransactionState,
							AppLogic.ro_TXModeAuthCapture);
					}
				}
				else
				{
					status = "The payment for this order has not yet been cleared.";
				}
			}
			else
			{
				status = string.Format("This transaction has already been refunded on {0}!", Localization.ToNativeDateTimeString(order.RefundedOn));
			}

			return status;
		}

		static public string OrderManagement_DoForceFullRefund(Order order)
		{
			var status = AppLogic.ro_OK;
			if(order.OrderNumber == 0 || order.IsEmpty)
				return "Order Not Found";

			status = ForceRefundStatus(order.OrderNumber);

			if(AppLogic.AppConfigBool("AvalaraTax.Enabled"))
			{
				var originAddress = new Address
				{
					Address1 = AppLogic.AppConfig("RTShipping.OriginAddress"),
					Address2 = AppLogic.AppConfig("RTShipping.OriginAddress2"),
					City = AppLogic.AppConfig("RTShipping.OriginCity"),
					Country = AppLogic.AppConfig("RTShipping.OriginCountry"),
					State = AppLogic.AppConfig("RTShipping.OriginState"),
					Suite = String.Empty,
					Zip = AppLogic.AppConfig("RTShipping.OriginZip"),

					//pass in the ShippingTaxClassID
					NickName = AppLogic.AppConfigUSInt("ShippingTaxClassID").ToString(),
				};

				var avaTax = new AvaTax();
				avaTax.IssueRefund(order, originAddress, 0);
			}

			return status;
		}

		static public String OrderManagement_MarkAsFraud(Order ord, String ViewInLocaleSetting)
		{
			String Status = AppLogic.ro_OK;
			if(ord.OrderNumber == 0 || ord.IsEmpty)
			{
				Status = String.Format("Order Not Found");
			}
			else
			{
				try
				{
					// ignore duplicates:
					Customer c = new Customer(ord.CustomerID, true);
					if(c.LastIPAddress.Length != 0)
					{
						DB.ExecuteSQL("insert RestrictedIP(IPAddress) values(" + DB.SQuote(c.LastIPAddress) + ")");
					}
				}
				catch { }
				Order.MarkOrderAsFraud(ord.OrderNumber, true);

				if(ord.TransactionIsCaptured())
				{
					if(AppLogic.AppConfigBool("AvalaraTax.Enabled"))
					{
						Address originAddress = new Address
						{
							Address1 = AppLogic.AppConfig("RTShipping.OriginAddress"),
							Address2 = AppLogic.AppConfig("RTShipping.OriginAddress2"),
							City = AppLogic.AppConfig("RTShipping.OriginCity"),
							Country = AppLogic.AppConfig("RTShipping.OriginCountry"),
							State = AppLogic.AppConfig("RTShipping.OriginState"),
							Suite = String.Empty,
							Zip = AppLogic.AppConfig("RTShipping.OriginZip"),

							//pass in the ShippingTaxClassID
							NickName = AppLogic.AppConfigUSInt("ShippingTaxClassID").ToString(),
						};

						AvaTax avaTax = new AvaTax();
						avaTax.IssueRefund(ord, originAddress, 0);
					}
				}
				else
				{
					if(AppLogic.AppConfigBool("AvalaraTax.Enabled"))
					{
						AvaTax avaTax = new AvaTax();
						avaTax.VoidTax(ord);
					}
				}
			}

			return Status;
		}

		static public String OrderManagement_ClearFraud(Order ord, String ViewInLocaleSetting)
		{
			String Status = AppLogic.ro_OK;
			if(ord.OrderNumber == 0 || ord.IsEmpty)
			{
				Status = String.Format("Order Not Found");
			}
			else
			{
				Customer c = new Customer(ord.CustomerID, true);
				if(c.LastIPAddress.Length != 0)
				{
					DB.ExecuteSQL("delete from RestrictedIP where IPAddress=" + DB.SQuote(c.LastIPAddress));
				}
				Order.MarkOrderAsFraud(ord.OrderNumber, false);
			}
			return Status;
		}

		static public String OrderManagement_ClearNewStatus(Order ord, String ViewInLocaleSetting)
		{
			String Status = AppLogic.ro_OK;
			if(ord.OrderNumber == 0 || ord.IsEmpty)
			{
				Status = String.Format("Order Not Found");
			}
			else
			{
				DB.ExecuteSQL("Update orders set IsNew=0 where OrderNumber=" + ord.OrderNumber.ToString());
			}
			return Status;
		}

		static public String OrderManagement_BlockIP(Order ord, String ViewInLocaleSetting)
		{
			String Status = AppLogic.ro_OK;
			if(ord.OrderNumber == 0 || ord.IsEmpty)
			{
				Status = String.Format("Order Not Found");
			}
			else
			{
				try
				{
					if(ord.LastIPAddress.Length != 0)
					{
						// ignore duplicates:
						DB.ExecuteSQL("insert RestrictedIP(IPAddress) values(" + DB.SQuote(ord.LastIPAddress) + ")");
					}
				}
				catch { }
			}
			return Status;
		}

		static public String OrderManagement_AllowIP(Order ord, String ViewInLocaleSetting)
		{
			String Status = AppLogic.ro_OK;
			if(ord.OrderNumber == 0 || ord.IsEmpty)
			{
				Status = String.Format("Order Not Found");
			}
			else
			{
				if(ord.LastIPAddress.Length != 0)
				{
					DB.ExecuteSQL("delete from RestrictedIP where IPAddress=" + DB.SQuote(ord.LastIPAddress));
				}
			}
			return Status;
		}

		static public String OrderManagement_SendDistributorNotification(Order ord, bool overrideDelay)
		{
			String status = AppLogic.ro_OK;
			if(ord.OrderNumber == 0 || ord.IsEmpty)
			{
				status = String.Format("Order Not Found");
			}
			else
			{
				if(ord.HasDistributorComponents())
				{
					bool delayDropShipNotification = AppLogic.AppConfigBool("DelayedDropShipNotifications");
					if(delayDropShipNotification || (AppLogic.AppConfigBool("MaxMind.Enabled") && ord.MaxMindFraudScore >= AppLogic.AppConfigNativeDecimal("MaxMind.DelayDropShipThreshold")))
					{
						delayDropShipNotification = true; // delay it anyway if maxmind fraud score is too high!
					}
					if(overrideDelay || (!delayDropShipNotification
						&& ord.TransactionIsCaptured()
						&& ord.DistributorEMailSentOn.Equals(System.DateTime.MinValue)
						&& ord.HasDistributorComponents()))
					{
						AppLogic.SendDistributorNotifications(new Order(ord.OrderNumber, ord.ViewInLocaleSetting)); // must reload order object to make this call work!
					}
				}
				else
				{
					status = "<p><b>NO DISTRIBUTOR ITEMS. DISTRIBUTOR E-MAIL(S) NOT SENT.";
				}

				if(ord.IsAllDownloadComponents() && ord.isAllDistributorComponents())
				{
					Order.MarkOrderAsShipped(ord.OrderNumber, "DISTRIBUTOR", string.Empty, DateTime.Now, false, true);
				}
			}
			return status;
		}

		static public String OrderManagement_ChangeOrderEMail(Order ord, String ViewInLocaleSetting, String NewEMail)
		{
			String Status = AppLogic.ro_OK;
			if(ord.OrderNumber == 0 || ord.IsEmpty)
			{
				Status = String.Format("Order Not Found");
			}
			else
			{
				if(NewEMail.Length != 0)
				{
					DB.ExecuteSQL("update Orders set EMail=" + DB.SQuote(NewEMail) + " where OrderNumber=" + ord.OrderNumber.ToString());

					// now, try to reassign the order to the customer who owns that e-mail address, IF and ONLY IF that e-mail address
					// is mapped to ONLY ONE customer record:
					if(DB.GetSqlN("select count(*) as N from Customer  with (NOLOCK)  where EMail=" + DB.SQuote(NewEMail) + " and Deleted=0") == 1)
					{
						// ok, we have one exact customer match, use it:
						int CustomerID = 0;

						using(SqlConnection con = new SqlConnection(DB.GetDBConn()))
						{
							con.Open();
							using(IDataReader rsCustomer = DB.GetRS("select CustomerID from Customer  with (NOLOCK)  where EMail=" + DB.SQuote(NewEMail) + " and Deleted=0", con))
							{
								if(rsCustomer.Read())
								{
									CustomerID = DB.RSFieldInt(rsCustomer, "CustomerID");
								}
							}
						}

						if(CustomerID != 0)
						{
							DB.ExecuteSQL("update Orders set CustomerID=" + CustomerID.ToString() + " where OrderNumber=" + ord.OrderNumber.ToString());
							DB.ExecuteSQL("update Orders_ShoppingCart set CustomerID=" + CustomerID.ToString() + " where OrderNumber=" + ord.OrderNumber.ToString());
							DB.ExecuteSQL("update Orders_KitCart set CustomerID=" + CustomerID.ToString() + " where OrderNumber=" + ord.OrderNumber.ToString());
						}
					}
					else
					{
						Status = "E-mail was changed, but the new address did not match an existing account so the order's CustomerID was not updated.";
					}
				}
			}
			return Status;
		}

		static public String OrderManagement_MarkAsReadyToShip(Order ord, String ViewInLocaleSetting)
		{
			String Status = AppLogic.ro_OK;
			if(ord.OrderNumber == 0 || ord.IsEmpty)
			{
				Status = String.Format("Order Not Found");
			}
			else
			{
				DB.ExecuteSQL("update Orders set ReadyToShip=1 where OrderNumber=" + ord.OrderNumber.ToString());
			}
			return Status;
		}

		static public String OrderManagement_ClearReadyToShip(Order ord, String ViewInLocaleSetting)
		{
			String Status = AppLogic.ro_OK;
			if(ord.OrderNumber == 0 || ord.IsEmpty)
			{
				Status = String.Format("Order Not Found");
			}
			else
			{
				DB.ExecuteSQL("update Orders set ReadyToShip=0 where OrderNumber=" + ord.OrderNumber.ToString());
			}
			return Status;
		}

		static public String OrderManagement_MarkAsShipped(Order ord, String ViewInLocaleSetting, String ShippedVIA, String TrackingNumber, DateTime ShippedOn)
		{
			String Status = AppLogic.ro_OK;
			if(ord.OrderNumber == 0 || ord.IsEmpty)
			{
				Status = String.Format("Order Not Found");
			}
			else
			{
				// make sure inventory was deducted. safe to call repeatedly. proc protects against deducting twice
				DB.ExecuteSQL("aspdnsf_AdjustInventory " + ord.OrderNumber.ToString() + ",-1");
				
				Order.MarkOrderAsShipped(ord.OrderNumber, ShippedVIA, TrackingNumber, ShippedOn, false, false);
			}
			return Status;
		}

		static public String OrderManagement_SetPrivateNotes(Order ord, String ViewInLocaleSetting, String Notes)
		{
			String Status = AppLogic.ro_OK;
			if(ord.OrderNumber == 0 || ord.IsEmpty)
			{
				Status = String.Format("Order Not Found");
			}
			else
			{
				DB.ExecuteSQL("Update orders set Notes=" + DB.SQuote(Notes) + " where OrderNumber=" + ord.OrderNumber.ToString());
			}
			return Status;
		}

		static public String OrderManagement_SetCustomerServiceNotes(Order ord, String ViewInLocaleSetting, String Notes)
		{
			String Status = AppLogic.ro_OK;
			if(ord.OrderNumber == 0 || ord.IsEmpty)
			{
				Status = String.Format("Order Not Found");
			}
			else
			{
				DB.ExecuteSQL("Update orders set CustomerServiceNotes=" + DB.SQuote(Notes) + " where OrderNumber=" + ord.OrderNumber.ToString());
			}
			return Status;
		}

		static public String OrderManagement_SetOrderWeight(Order ord, String ViewInLocaleSetting, Decimal NewWeight)
		{
			String Status = AppLogic.ro_OK;
			if(ord.OrderNumber == 0 || ord.IsEmpty)
			{
				Status = String.Format("Order Not Found");
			}
			else
			{
				DB.ExecuteSQL("update Orders set OrderWeight=" + Localization.DecimalStringForDB(NewWeight) + " where OrderNumber=" + ord.OrderNumber.ToString());
			}
			return Status;
		}

		static public String OrderManagement_SendToFedexShippingMgr(Order ord, String ViewInLocaleSetting)
		{
			String Status = AppLogic.ro_OK;
			if(ord.OrderNumber == 0 || ord.IsEmpty)
			{
				Status = String.Format("Order Not Found");
			}
			else
			{
				// make sure inventory was deducted. safe to call repeatedely. proc protects doing twice
				DB.ExecuteSQL("aspdnsf_AdjustInventory " + ord.OrderNumber.ToString() + ",-1");

				// clear out any old job if this is a re-send:
				DB.ExecuteSQL("delete from ShippingImportExport where OrderNumber=" + ord.OrderNumber);

				String sql = "insert into ShippingImportExport(OrderNumber,CustomerID,CompanyName,CustomerLastName,CustomerFirstName,CustomerEmail,CustomerPhone,Address1,Address2,Suite,City,State,Zip,Country,ServiceCarrierCode, Cost,Weight) values(";

				sql += ord.OrderNumber + ",";
				sql += ord.CustomerID + ",";
				sql += DB.SQuoteNotUnicode(CommonLogic.Left(ord.ShippingAddress.m_Company, 50)) + ",";
				sql += DB.SQuoteNotUnicode(CommonLogic.Left(ord.ShippingAddress.m_FirstName + " " + ord.ShippingAddress.m_LastName, 50)) + ",";
				sql += DB.SQuoteNotUnicode(CommonLogic.Left("Combined in last name per FedEx", 50)) + ",";
				sql += DB.SQuoteNotUnicode(CommonLogic.Left(ord.ShippingAddress.m_EMail, 100)) + ",";
				sql += DB.SQuoteNotUnicode(CommonLogic.Left(ord.ShippingAddress.m_Phone, 50)) + ",";
				sql += DB.SQuoteNotUnicode(CommonLogic.Left(ord.ShippingAddress.m_Address1, 100)) + ",";
				sql += DB.SQuoteNotUnicode(CommonLogic.Left(ord.ShippingAddress.m_Address2 + " " + ord.ShippingAddress.m_Suite, 100)) + ",";
				sql += DB.SQuoteNotUnicode(CommonLogic.Left("SUITE is added to Address2 per FedEx", 50)) + ",";
				sql += DB.SQuoteNotUnicode(CommonLogic.Left(ord.ShippingAddress.m_City, 100)) + ",";
				sql += DB.SQuoteNotUnicode(CommonLogic.Left(ord.ShippingAddress.m_State, 100)) + ",";
				sql += DB.SQuoteNotUnicode(CommonLogic.Left(ord.ShippingAddress.m_Zip, 10)) + ",";
				sql += DB.SQuoteNotUnicode(CommonLogic.Left(ord.ShippingAddress.m_Country, 100)) + ",";
				string temp = ord.ShippingMethod;

				//MOD START DV - Check to make sure the shipping method contains a pipe 
				if(temp.IndexOf("|") != -1)
				{
					temp = temp.Substring(0, temp.IndexOf("|"));
				}
				//END MOD

				sql += DB.SQuoteNotUnicode(CommonLogic.Left(temp, 50)) + ",";
				//sql += "1,";
				sql += ord.ShippingTotal() + ",";
				sql += ord.OrderWeight;
				sql += ")";

				DB.ExecuteSQL(sql.ToString());

				DB.ExecuteSQL("Update orders set ReadyToShip=1, IsNew=0, ShippedVIA=" + DB.SQuote(ord.ShippingMethod) + ", ShippingTrackingNumber=" + DB.SQuote("Pending From FedEx ShipManager") + ", ShippedOn=getdate() where OrderNumber=" + ord.OrderNumber.ToString());
			}
			return Status;
		}

		static public String OrderManagement_MarkAsPrinted(Order ord, String ViewInLocaleSetting)
		{
			String Status = AppLogic.ro_OK;
			if(ord.OrderNumber == 0 || ord.IsEmpty)
			{
				Status = String.Format("Order Not Found");
			}
			else
			{
				DB.ExecuteSQL("update Orders set IsPrinted=1 where OrderNumber=" + ord.OrderNumber.ToString());
			}
			return Status;
		}

		static public string OrderManagement_SendReceipt(Order ord, string ViewInLocaleSetting)
		{
			string status = AppLogic.ro_OK;

			if(ord.OrderNumber == 0 || ord.IsEmpty)
			{
				status = string.Format("Order Not Found");
			}
			else
			{
				var customer = new Customer(ord.CustomerID, true);
				var orderStoreId = Order.GetOrderStoreID(ord.OrderNumber);
				var mailServer = AppLogic.AppConfig("MailMe_Server", orderStoreId, true);

				if(!string.IsNullOrEmpty(ord.EMail) &&
					!string.IsNullOrEmpty(mailServer) &&
					mailServer != AppLogic.ro_TBD)
				{
					var subjectLine = new StringBuilder();

					if(AppLogic.AppConfigBool("UseLiveTransactions", orderStoreId, true))
						subjectLine.AppendFormat("{0} {1}", AppLogic.AppConfig("StoreName", orderStoreId, true), "Receipt");
					else
						subjectLine.AppendFormat("{0} {1}", AppLogic.AppConfig("StoreName", orderStoreId, true), string.Format("{0} Receipt (TEST MODE)", string.Empty));

					if(ord.PaymentMethod.Equals("REQUEST QUOTE", StringComparison.InvariantCultureIgnoreCase))
						subjectLine.AppendFormat(" {0}", "Request For Quote");

					AppLogic.SendMail(subject: subjectLine.ToString(),
						body: ord.Receipt(customer, true) + AppLogic.AppConfig("MailFooter", orderStoreId, true), 
						useHtml: true, 
						fromAddress: AppLogic.AppConfig("ReceiptEMailFrom", orderStoreId, true), 
						fromName: AppLogic.AppConfig("ReceiptEMailFromName", orderStoreId, true), 
						toAddress: ord.EMail, 
						toName: ord.EMail, 
						bccAddresses: string.Empty, 
						replyToAddress: AppLogic.AppConfig("ReceiptEMailFrom", orderStoreId, true));

					DB.ExecuteSQL("UPDATE Orders SET ReceiptEMailSentOn = GETDATE() WHERE OrderNumber = @orderNumber", new SqlParameter("@orderNumber", ord.OrderNumber));
				}
				else
				{
					status = "NO MAIL SERVER INFO OR NO CUSTOMER E-MAIL ADDRESS FOUND. RECEIPT E-MAIL NOT SENT";
				}
			}
			return status;
		}

		static public String OrderManagement_SetTracking(Order ord, String ViewInLocaleSetting, String ShippedVIA, String TrackingNumber)
		{
			String Status = AppLogic.ro_OK;
			if(ord.OrderNumber == 0 || ord.IsEmpty)
			{
				Status = String.Format("Order Not Found");
			}
			else
			{
				DB.ExecuteSQL("Update orders set ShippedVIA=" + DB.SQuote(ShippedVIA) + ", ShippingTrackingNumber=" + DB.SQuote(TrackingNumber) + " where OrderNumber=" + ord.OrderNumber.ToString());
			}
			return Status;
		}


	}
}
