// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways.Processors;

namespace AspDotNetStorefrontAdmin
{
	/// <summary>
	/// Summary description for editaddressrecurring.
	/// </summary>
	public partial class editaddressrecurring : AspDotNetStorefront.Admin.AdminPageBase
	{
		int AddressID = 0;
		int OriginalRecurringOrderNumber = 0;
		Address theAddress = new Address();
		Customer AddressCustomer;

		protected void Page_Load(object sender, System.EventArgs e)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");

			AddressID = CommonLogic.QueryStringUSInt("AddressID");
			OriginalRecurringOrderNumber = CommonLogic.QueryStringUSInt("OriginalRecurringOrderNumber");
			theAddress.LoadFromDB(AddressID);
			AddressCustomer = new Customer(theAddress.CustomerID, true);
			ErrorMsgLabel.Text = string.Empty;
			if(!IsPostBack)
			{
				InitializePageContent();
			}
			else
			{
				OriginalRecurringOrderNumber = Localization.ParseNativeInt(ltOriginalRecurringOrderNumber.Text);
			}
			Page.Form.DefaultButton = btnSaveAddress.UniqueID;
		}

		public void btnSaveAddress_Click(object sender, EventArgs e)
		{
			ProcessForm(false, Convert.ToInt32(((Button)sender).CommandArgument));
		}

		private void ProcessForm(bool UseValidationService, int AddressID)
		{
			string ResidenceType = ddlResidenceType.SelectedValue;
			bool valid = true;
			string errormsg = string.Empty;
			bool CardIncluded = false;
			errormsg += "<ul>";

			if(string.IsNullOrEmpty(CommonLogic.FormCanBeDangerousContent("CardName")))
			{
				valid = false;
				errormsg += "<li>" + AppLogic.GetString("admin.editaddressrecurring.CardNameIsRequired", SkinID, LocaleSetting) + "</li>";
			}
			if(string.IsNullOrEmpty(CommonLogic.FormCanBeDangerousContent("CardType")))
			{
				valid = false;
				errormsg += "<li>" + AppLogic.GetString("admin.editaddressrecurring.CardTypeIsRequired", SkinID, LocaleSetting) + "</li>";
			}
			if(string.IsNullOrEmpty(CommonLogic.FormCanBeDangerousContent("CardNumber")))
			{
				valid = false;
				errormsg += "<li>" + AppLogic.GetString("admin.editaddressrecurring.CardNumberIsRequired", SkinID, LocaleSetting) + "</li>";
			}
			else
			{
				CardIncluded = true;
			}

			int iexpMonth = 0;
			int iexpYear = 0;
			string expMonth = CommonLogic.FormCanBeDangerousContent("CardExpirationMonth");
			string expYear = CommonLogic.FormCanBeDangerousContent("CardExpirationYear");

			if(string.IsNullOrEmpty(expMonth) ||
				!int.TryParse(expMonth, out iexpMonth) ||
				!(iexpMonth > 0))
			{
				valid = false;
				errormsg += "<li>" + AppLogic.GetString("admin.editaddressrecurring.CardExpirationMonthNotification", SkinID, LocaleSetting) + "</li>";
			}
			else
			{
				CardIncluded = true;
			}

			if(string.IsNullOrEmpty(expYear) ||
				!int.TryParse(expYear, out iexpYear) ||
				!(iexpYear > 0))
			{
				valid = false;
				errormsg += "<li>" + AppLogic.GetString("admin.editaddressrecurring.CardExpirationYearNotification", SkinID, LocaleSetting) + "</li>";
			}
			else
			{
				CardIncluded = true;
			}

			if(!CardIncluded)
			{
				valid = true;
			}

			if(!valid)
			{
				errormsg += "</ul>";
				ErrorMsgLabel.Text += errormsg;
				InitializePageContent();
				return;
			}
			else
			{
				ErrorMsgLabel.Text = String.Empty;
			}

			theAddress.AddressType = AddressTypes.Billing;
			theAddress.NickName = txtAddressNickName.Text;
			theAddress.FirstName = txtFirstName.Text;
			theAddress.LastName = txtLastName.Text;
			theAddress.Company = txtCompany.Text;
			theAddress.Address1 = txtAddress1.Text;
			theAddress.Address2 = txtAddress2.Text;
			theAddress.Suite = txtSuite.Text;
			theAddress.City = txtCity.Text;
			theAddress.State = ddlState.SelectedValue;
			theAddress.Zip = txtZip.Text;
			theAddress.Country = ddlCountry.SelectedValue;
			theAddress.Phone = txtPhone.Text;
			if(ResidenceType == "2")
			{
				theAddress.ResidenceType = ResidenceTypes.Commercial;
			}
			else if(ResidenceType == "1")
			{
				theAddress.ResidenceType = ResidenceTypes.Residential;
			}
			else
			{
				theAddress.ResidenceType = ResidenceTypes.Unknown;
			}

			if(CardIncluded)
			{
				theAddress.PaymentMethodLastUsed = AppLogic.ro_PMCreditCard;
				theAddress.CardName = CommonLogic.FormCanBeDangerousContent("CardName");
				theAddress.CardType = CommonLogic.FormCanBeDangerousContent("CardType");

				string tmpS = CommonLogic.FormCanBeDangerousContent("CardNumber");
				if(!tmpS.StartsWith("*"))
				{
					theAddress.CardNumber = tmpS;
				}
				theAddress.CardExpirationMonth = CommonLogic.FormCanBeDangerousContent("CardExpirationMonth");
				theAddress.CardExpirationYear = CommonLogic.FormCanBeDangerousContent("CardExpirationYear");
			}

			theAddress.UpdateDB();

			SetupCreditCardForm();

			RecurringOrderMgr rmgr = new RecurringOrderMgr();
			errormsg = rmgr.ProcessAutoBillAddressUpdate(OriginalRecurringOrderNumber, theAddress);
			ErrorMsgLabel.Text = errormsg != AppLogic.ro_OK ? errormsg : String.Empty;
			if(!AddressCustomer.MasterShouldWeStoreCreditCardInfo)
			{
				theAddress.ClearCCInfo();
				theAddress.UpdateDB();
			}
		}

		public void ddlCountry_OnChange(object sender, EventArgs e)
		{
			string sql = String.Empty;
			if(ddlCountry.SelectedIndex > 0)
			{
				sql = "select s.* from dbo.State s  with (NOLOCK)  join dbo.country c on s.countryid = c.countryid where c.name = " + DB.SQuote(ddlCountry.SelectedValue) + " order by s.DisplayOrder,s.Name";
			}
			else
			{
				sql = "select * from dbo.State   with (NOLOCK)  where countryid=(select countryid from country where name='United States') order by DisplayOrder,Name";
			}

			SetStateList(ddlCountry.SelectedValue);
		}

		private void InitializePageContent()
		{
			ltOriginalRecurringOrderNumber.Text = OriginalRecurringOrderNumber.ToString();
			btnSaveAddress.Text = AppLogic.GetString("editaddress.aspx.9", SkinID, LocaleSetting);
			btnSaveAddress.CommandArgument = AddressID.ToString();

			SetupCreditCardForm();

			txtAddressNickName.Text = theAddress.NickName;
			txtFirstName.Text = theAddress.FirstName;
			txtLastName.Text = theAddress.LastName;
			txtPhone.Text = theAddress.Phone;
			txtCompany.Text = theAddress.Company;
			ddlResidenceType.Items.Add(new ListItem(AppLogic.GetString("address.cs.55", SkinID, LocaleSetting), ((int)ResidenceTypes.Unknown).ToString()));
			ddlResidenceType.Items.Add(new ListItem(AppLogic.GetString("address.cs.56", SkinID, LocaleSetting), ((int)ResidenceTypes.Residential).ToString()));
			ddlResidenceType.Items.Add(new ListItem(AppLogic.GetString("address.cs.57", SkinID, LocaleSetting), ((int)ResidenceTypes.Commercial).ToString()));
			ddlResidenceType.SelectedValue = ((int)theAddress.ResidenceType).ToString();
			txtAddress1.Text = theAddress.Address1;
			txtAddress2.Text = theAddress.Address2;
			txtSuite.Text = theAddress.Suite;
			txtCity.Text = theAddress.City;
			txtZip.Text = theAddress.Zip;

			using(var dbconn = DB.dbConn())
			{
				dbconn.Open();
				using(var dr = DB.GetRS("select * from Country   with (NOLOCK)  where Published = 1 order by DisplayOrder,Name", dbconn))
				{
					ddlCountry.DataSource = dr;
					ddlCountry.DataTextField = "Name";
					ddlCountry.DataValueField = "Name";
					ddlCountry.DataBind();
				}
			}
			ddlCountry.SelectedValue = theAddress.Country;
			SetStateList(theAddress.Country);
			ddlState.SelectedValue = theAddress.State;
		}

		void SetupCreditCardForm()
		{
			//Do we need the CC form?
			var originalOrder = new Order(OriginalRecurringOrderNumber);
			if(originalOrder.PaymentMethod == AppLogic.ro_PMCreditCard)
				litCCForm.Text = theAddress.InputCardHTML(AddressCustomer, false);
		}

		private void SetStateList(string Country)
		{
			string sql = String.Empty;
			if(Country.Length > 0)
			{
				sql = "select s.* from dbo.State s  with (NOLOCK)  join dbo.country c on s.countryid = c.countryid where c.name = " + DB.SQuote(Country) + " order by s.DisplayOrder,s.Name";
			}
			else
			{
				sql = "select * from dbo.State   with (NOLOCK)  where countryid = 222 order by DisplayOrder,Name";
			}

			ddlState.ClearSelection();

			using(var dbconn = DB.dbConn())
			{
				dbconn.Open();
				using(var dr = DB.GetRS(sql, dbconn))
				{
					ddlState.DataSource = dr;
					ddlState.DataTextField = "Name";
					ddlState.DataValueField = "Abbreviation";
					ddlState.DataBind();
				}
			}
			if(ddlState.Items.Count == 0)
			{
				ddlState.Items.Insert(0, new ListItem("state.countrywithoutstates".StringResource(), "--"));
				ddlState.SelectedIndex = 0;
			}
		}

		override protected void OnPreInit(EventArgs e)
		{
			base.OnPreInit(e);
		}
	}
}
