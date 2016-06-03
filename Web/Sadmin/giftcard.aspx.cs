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
using System.Text;
using System.Threading;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontControls;

namespace AspDotNetStorefrontAdmin
{
	public partial class giftcard : AspDotNetStorefront.Admin.AdminPageBase
	{
		protected string selectSQL = @"SELECT G.*, C.FirstName, C.LastName 
			from GiftCard G with (NOLOCK) 
			LEFT OUTER JOIN Customer C with (NOLOCK) ON G.PurchasedByCustomerID = C.CustomerID ";

		int giftCardId;

		protected void Page_Load(object sender, EventArgs e)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");

			giftCardId = CommonLogic.QueryStringNativeInt("giftcardid");

			if(giftCardId == 0)
			{
				lblGiftCardUsage.Visible = false;
				lnkGiftCardUsage.Visible = false;
				OrderNumberRow.Visible = false;
				RemainingBalanceRow.Visible = false;
				ltAmount.Visible = false;
				PurchasedByCustomerIDLiteralRow.Visible = false;
				GiftCardTypeDisplayRow.Visible = false;
				InitialAmountLiteralRow.Visible = false;
				PurchasedByCustomerIDTextRow.Visible = true;
				reqCustEmail.Enabled = true;
			}
			else
			{
				lnkGiftCardUsage.NavigateUrl = string.Format("{0}?giftcardid={1}", AppLogic.AdminLinkUrl("giftcardusage.aspx"), giftCardId);
				txtAmount.Visible = false; // cannot change after first created
				PurchasedByCustomerIDTextRow.Visible = false;
				reqCustEmail.Enabled = false;
				GiftCardTypeSelectRow.Visible = false;
				InitialAmountTextRow.Visible = false;
			}

			if(!IsPostBack)
			{
				txtDate.Culture = Thread.CurrentThread.CurrentUICulture;

				trEmail.Visible = giftCardId == 0;

				if(giftCardId > 0)
				{
					ltSerialNumber.Text = DB.GetSqlS(string.Format("SELECT SerialNumber AS S FROM GiftCard WHERE GiftCardID = {0}", giftCardId));

					LoadData();

					if(etsMapper.ObjectID != giftCardId)
					{
						etsMapper.ObjectID = giftCardId;
						etsMapper.DataBind();
					}
				}
				else
				{
					lblAction.Visible = false;
					rblAction.Visible = false;
					ltCurrentBalance.Text = "NA";

					ltSerialNumber.Text = "admin.editgiftcard.NewGiftCard".StringResource();
					rblAction.SelectedIndex = 0;

					var giftCardAssignmentXml = new XmlPackage("giftcardassignment.xml.config");
					System.Xml.XmlDocument xmlDoc = giftCardAssignmentXml.XmlDataDocument;
					txtSerial.Text = xmlDoc.SelectSingleNode("/root/GiftCardAssignment/row/CardNumber").InnerText;
					txtDate.SelectedDate = DateTime.Now.AddYears(1);
				}
			}

			var mapToStores = AppLogic.GlobalConfigBool("AllowGiftCardFiltering");

			storeMapperRow.Visible = mapToStores;
		}

		protected override void OnPreRender(EventArgs e)
		{
			DataBind();

			base.OnPreRender(e);
		}

		protected void LoadData()
		{
			using(var dbconn = DB.dbConn())
			{
				dbconn.Open();
				using(var rs = DB.GetRS(String.Format("SELECT * FROM GiftCard  with (NOLOCK)  WHERE GiftCardID={0}", giftCardId), dbconn))
				{
					if(!rs.Read())
					{
						rs.Close();
						AlertMessage.PushAlertMessage("Unable to retrieve data.", AlertMessage.AlertType.Success);
						return;
					}

					txtSerial.Text = DB.RSField(rs, "SerialNumber");
					var gcCustomerId = DB.RSFieldInt(rs, "PurchasedByCustomerID");
					ltCustomerID.Text = gcCustomerId.ToString();

					if(gcCustomerId != 0)
					{
						var gcCustomer = new Customer(gcCustomerId);
						ltCustomerEmail.Text = gcCustomer.EMail;
					}
					else
					{
						ltCustomerEmail.Text = "admin.editgiftcard.NACustomer".StringResource();
					}

					txtOrder.Text = DB.RSFieldInt(rs, "OrderNumber").ToString();
					txtDate.SelectedDate = DB.RSFieldDateTime(rs, "ExpirationDate");

					txtAmount.Text = Localization.CurrencyStringForDBWithoutExchangeRate(DB.RSFieldDecimal(rs, "InitialAmount"));
					ltAmount.Text = Localization.CurrencyStringForDBWithoutExchangeRate(DB.RSFieldDecimal(rs, "InitialAmount"));
					ltCurrentBalance.Text = Localization.CurrencyStringForDBWithoutExchangeRate(DB.RSFieldDecimal(rs, "Balance"));

					var giftCardTypeId = DB.RSFieldInt(rs, "GiftCardTypeID");
					var giftCardType = (GiftCardTypes)giftCardTypeId;

					ddType.Items.FindByValue(giftCardTypeId.ToString()).Selected = true;
					ltGiftCardType.Text = giftCardType.ToString();

					rblAction.ClearSelection();
					rblAction.SelectedIndex = CommonLogic.IIF(DB.RSFieldBool(rs, "DisabledByAdministrator"), 1, 0);
				}
			}
		}

		protected void btnClose_Click(object sender, EventArgs e)
		{
			if(SaveForm())
				Response.Redirect(ReturnUrlTracker.GetReturnUrl());
		}

		protected void btnSubmit_Click(object sender, EventArgs e)
		{
			if(SaveForm())
				Response.Redirect(String.Format("giftcard.aspx?giftcardid={0}", giftCardId));
		}

		private bool SaveForm()
		{
			if(Page.IsValid)
			{
				//giftCardId = 0 means we're creating a new giftcard
				var creatingGiftCard = giftCardId == 0;
				var giftCardType = Localization.ParseNativeInt(ddType.SelectedValue);
				var customerId = Localization.ParseNativeInt(hdnCustomerId.Value);

				//validate customer id if creating giftcard
				if(creatingGiftCard && customerId == 0)
				{
					AlertMessage.PushAlertMessage("admin.editgiftcard.InvalidEmail".StringResource(), AlertMessage.AlertType.Error);
					return false;
				}

				//validate email fields if we're creating an EmailGiftCard
				if(giftCardType == (int)GiftCardTypes.EMailGiftCard && creatingGiftCard)
				{
					if(txtEmailBody.Text.Length == 0 
						|| txtEmailName.Text.Length == 0
						|| txtEmailTo.Text.Length == 0)
					{
						AlertMessage.PushAlertMessage("admin.editgiftcard.EnterEmailPreferences".StringResource(), AlertMessage.AlertType.Error);
						return false;
					}

					//make sure the customer has set up their email properly
					if(AppLogic.AppConfig("MailMe_Server").Length == 0
						|| AppLogic.AppConfig("MailMe_FromAddress") == "sales@yourdomain.com")
					{
						//Customer has not configured their MailMe AppConfigs yet
						AlertMessage.PushAlertMessage("giftcard.email.error.2".StringResource(), AlertMessage.AlertType.Error);
						return false;
					}
				}

				//make sure the date is filled in
				if(txtDate.SelectedDate == null)
				{
					AlertMessage.PushAlertMessage("admin.common.FillinExpirationDate".StringResource(), AlertMessage.AlertType.Error);
					return false;
				}

				//check if valid SN
				var isDuplicateSerialNumberSql = string.Format("select count(GiftCardID) as N from GiftCard with (NOLOCK) where GiftCardID<>{0} and lower(SerialNumber)={1}",
					giftCardId,
					DB.SQuote(txtSerial.Text.ToLowerInvariant().Trim()));
				var isDuplicateSerialNumber = DB.GetSqlN(isDuplicateSerialNumberSql) > 0;

				if(isDuplicateSerialNumber)
				{
					AlertMessage.PushAlertMessage("admin.editgiftcard.ExistingGiftCard".StringResource(), AlertMessage.AlertType.Error);
					return false;
				}

				if(creatingGiftCard)
				{
					//insert a new card
					var newGiftCard = GiftCard.CreateGiftCard(customerId,
										txtSerial.Text,
										Localization.ParseNativeInt(txtOrder.Text),
										0,
										0,
										0,
										Localization.ParseNativeDecimal(txtAmount.Text),
										txtDate.SelectedDate.Value,
										Localization.ParseNativeDecimal(txtAmount.Text),
										ddType.SelectedValue,
										CommonLogic.Left(txtEmailName.Text, 100),
										CommonLogic.Left(txtEmailTo.Text, 100),
										txtEmailBody.Text,
										null,
										null,
										null,
										null,
										null,
										null);

					try
					{
						newGiftCard.SendGiftCardEmail();
					}
					catch
					{
						//reload page, but inform the admin the the email could not be sent
						AlertMessage.PushAlertMessage("giftcard.email.error.1".StringResource(), AlertMessage.AlertType.Success);
					}

					//reload page
					giftCardId = newGiftCard.GiftCardID;
					etsMapper.ObjectID = giftCardId;
					AlertMessage.PushAlertMessage("admin.editgiftcard.GiftCardAdded".StringResource(), AlertMessage.AlertType.Success);
				}
				else
				{
					//update existing card
					DB.ExecuteSQL(
						@"UPDATE GiftCard SET
							SerialNumber=@serialNumber,
							ExpirationDate=@expirationDate,
							DisabledByAdministrator=@disabledByAdministrator
						WHERE GiftCardID=@giftCardId",
					new[]
						{
							new SqlParameter("@serialNumber", txtSerial.Text),
							new SqlParameter("@expirationDate", Localization.ToDBShortDateString(Localization.ParseNativeDateTime(txtDate.SelectedDate.Value.ToString()))),
							new SqlParameter("@disabledByAdministrator", Localization.ParseNativeInt(rblAction.SelectedValue)),
							new SqlParameter("@giftCardId", giftCardId)
						});

					etsMapper.ObjectID = giftCardId;
					AlertMessage.PushAlertMessage("admin.editgiftcard.GiftCardUpdated".StringResource(), AlertMessage.AlertType.Success);
				}
				etsMapper.Save();
			}

			return true;
		}

		[System.Web.Services.WebMethod(), System.Web.Script.Services.ScriptMethod()]
		public static string[] GetCompletionList(string prefixText, int count, string contextKey)
		{
			using(var connection = new SqlConnection(DB.GetDBConn()))
			using(var command = connection.CreateCommand())
			{
				command.CommandText = @"select top (@count) CustomerId, Email 
										from Customer with(nolock) 
										where Email != '' 
										and Email like @prefixText
										or FirstName like @prefixText 
										or LastName like @prefixText 
										order by CustomerId";

				command.Parameters.AddWithValue("prefixText", prefixText + "%");
				command.Parameters.AddWithValue("count", count);

				var customers = new List<string>();

				connection.Open();
				using(var reader = command.ExecuteReader())
					while(reader.Read())
						customers.Add(AjaxControlToolkit.AutoCompleteExtender.CreateAutoCompleteItem(
							reader.Field("Email"),
							reader.FieldInt("CustomerID").ToString()));

				return customers.ToArray();
			}
		}
	}
}
