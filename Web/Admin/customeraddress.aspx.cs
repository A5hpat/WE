// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Web.UI.WebControls;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class CustomerAddress : AspDotNetStorefront.Admin.AdminPageBase
	{
		int CustomerId;
		string AdminLocale;

		protected override void OnInit(System.EventArgs e)
		{
			CustomerId = CommonLogic.QueryStringNativeInt("customerid");
			if(CustomerId == 0)
				Response.Redirect(ReturnUrlTracker.GetReturnUrl());

			AdminLocale = AppLogic.GetCurrentCustomer().LocaleSetting;

			base.OnInit(e);
		}

		protected override void OnPreRender(EventArgs e)
		{
			var customer = new Customer(CustomerId);
			if(dtlAddressList.CurrentMode == DetailsViewMode.ReadOnly && !customer.HasAtLeastOneAddress())
				dtlAddressList.ChangeMode(DetailsViewMode.Insert);

			// don't "re"bind if editing or inserting
			if (dtlAddressList.CurrentMode == DetailsViewMode.ReadOnly)
				DataBind();

			btnMakeBilling.Visible
				= btnMakeShipping.Visible
				= (dtlAddressList.CurrentMode == DetailsViewMode.ReadOnly);

			base.OnPreRender(e);
		}

		protected void ddlCountry_OnSelectedIndexChanged(object sender, EventArgs e)
		{
			var countryDropDown = (DropDownList)dtlAddressList.FindControl("ddlCountry");
			var stateDropDown = (DropDownList)dtlAddressList.FindControl("ddlState");
			var row = (DataRowView)dtlAddressList.DataItem;

			var selectedCountryId = AppLogic.GetCountryID(countryDropDown.SelectedItem.Text);
			var stateList = State.GetAllStatesForCountry(selectedCountryId);

			stateDropDown.Items.Clear();

			foreach(var state in stateList)
				stateDropDown.Items.Add(new ListItem(state.Name, state.Abbreviation));

			if(row != null)
				stateDropDown.SelectFirstByValue(row.Row["State"].ToString());
		}

		protected void btnMakeBilling_OnClick(object sender, EventArgs e)
		{
			if(dtlAddressList.DataKey.Value == null || Convert.ToInt32(dtlAddressList.DataKey.Value) == 0)
				return;

			var customer = new Customer(CustomerId);
			var addressID = Convert.ToInt32(dtlAddressList.DataKey.Value);

			customer.SetPrimaryAddress(addressID, AddressTypes.Billing);

			AlertMessageDisplay.PushAlertMessage(
				AppLogic.GetString("admin.primarybillingchanged", AdminLocale),
				AlertMessage.AlertType.Success);
		}

		protected void btnMakeShipping_OnClick(object sender, EventArgs e)
		{
			if(dtlAddressList.DataKey.Value == null || Convert.ToInt32(dtlAddressList.DataKey.Value) == 0)
				return;

			var customer = new Customer(CustomerId);
			var addressID = Convert.ToInt32(dtlAddressList.DataKey.Value);

			customer.SetPrimaryAddress(addressID, AddressTypes.Shipping);

			AlertMessageDisplay.PushAlertMessage(
				AppLogic.GetString("admin.primaryshippingchanged", AdminLocale),
				AlertMessage.AlertType.Success);
		}

		protected void dtlAddressList_OnDataBound(object sender, EventArgs e)
		{
			var row = (DataRowView)dtlAddressList.DataItem;

			//Populate the Country and State Select Lists
			if(dtlAddressList.CurrentMode == DetailsViewMode.ReadOnly)
			{
				var lblResidenceType = (Label)dtlAddressList.FindControl("lblResidenceType");
				if(row == null || lblResidenceType == null)
					return;

				try
				{
					lblResidenceType.Text = Enum.GetName(typeof(ResidenceTypes), Convert.ToInt16(row.Row["ResidenceType"]));
				}
				catch
				{
					lblResidenceType.Text = ResidenceTypes.Unknown.ToString();
				}

				return;
			}

			//Populate the country select list as it is needed to populate the state
			var countryDropDown = (DropDownList)dtlAddressList.FindControl("ddlCountry");
			var selectedCountry = String.Empty;
			if(countryDropDown != null)
			{
				var countryList = Country.GetAll();

				foreach(var country in countryList)
					countryDropDown.Items.Add(new ListItem(country.Name, country.ID.ToString()));

				if(row != null)
				{
					selectedCountry = row.Row["Country"].ToString();

					if(!String.IsNullOrEmpty(selectedCountry))
						countryDropDown.SelectFirstByText(selectedCountry);
				}
				else
					selectedCountry = countryDropDown.Items[0].Text;
			}

			//Repeat to populate the state dropdown
			var stateDropDown = (DropDownList)dtlAddressList.FindControl("ddlState");
			if(stateDropDown != null)
			{
				var countryId = AppLogic.GetCountryID(selectedCountry);

				var stateList = State.GetAllStatesForCountry(countryId);
				foreach(var state in stateList)
					stateDropDown.Items.Add(new ListItem(state.Name, state.Abbreviation));

				if(row != null)
				{
					var selectedState = row.Row["State"].ToString();
					if(!String.IsNullOrEmpty(selectedState))
						stateDropDown.SelectFirstByValue(selectedState);
				}
			}

			//Populate the Residence Type Dropdown
			var ddlResidenceType = (DropDownList)dtlAddressList.FindControl("ddlResidenceType");
			if(ddlResidenceType != null)
			{
				ddlResidenceType.DataSource = Enum.GetNames(typeof(ResidenceTypes));
				ddlResidenceType.DataBind();

				if(row != null)
				{
					var selectedResidenceType = Convert.ToInt16(row.Row["ResidenceType"]);

					if(ddlResidenceType.Items[selectedResidenceType] != null)
						ddlResidenceType.Items[selectedResidenceType].Selected = true;
				}
			}
		}

		protected void dtlAddressList_OnItemInserting(object sender, DetailsViewInsertEventArgs e)
		{
			var countryDropDown = (DropDownList)dtlAddressList.FindControl("ddlCountry");
			var stateDropDown = (DropDownList)dtlAddressList.FindControl("ddlState");
			var residenceDropDown = (DropDownList)dtlAddressList.FindControl("ddlResidenceType");

			sqlAddressList.InsertParameters.Add(new Parameter("Country", DbType.String, countryDropDown.SelectedItem.Text));
			sqlAddressList.InsertParameters.Add(new Parameter("State", DbType.String, stateDropDown.SelectedValue));
			sqlAddressList.InsertParameters.Add(new Parameter("ResidenceType", DbType.Int16, residenceDropDown.SelectedIndex.ToString()));
		}

		protected void dtlAddressList_OnItemUpdating(object sender, DetailsViewUpdateEventArgs e)
		{
			var countryDropDown = (DropDownList)dtlAddressList.FindControl("ddlCountry");
			var stateDropDown = (DropDownList)dtlAddressList.FindControl("ddlState");
			var residenceDropDown = (DropDownList)dtlAddressList.FindControl("ddlResidenceType");

			sqlAddressList.UpdateParameters.Add(new Parameter("Country", DbType.String, countryDropDown.SelectedItem.Text));
			sqlAddressList.UpdateParameters.Add(new Parameter("State", DbType.String, stateDropDown.SelectedValue));
			sqlAddressList.UpdateParameters.Add(new Parameter("ResidenceType", DbType.Int16, residenceDropDown.SelectedIndex.ToString()));
		}

		protected void dtlAddressList_ItemUpdated(object sender, DetailsViewUpdatedEventArgs e)
		{
			var addressId = Convert.ToInt32(dtlAddressList.DataKey.Value);

			if (string.IsNullOrEmpty(((TextBox)dtlAddressList.FindControl("txtNickName")).Text))
				DB.ExecuteSQL(
					sql: "update Address set NickName = null where AddressId = @addressId",
					parameters: new System.Data.SqlClient.SqlParameter("@addressId", addressId));

			if(string.IsNullOrEmpty(((TextBox)dtlAddressList.FindControl("txtCompany")).Text))
				DB.ExecuteSQL(
					sql: "update Address set Company = null where AddressId = @addressId",
					parameters: new System.Data.SqlClient.SqlParameter("@addressId", addressId));

			if(string.IsNullOrEmpty(((TextBox)dtlAddressList.FindControl("txtAddress2")).Text))
				DB.ExecuteSQL(
					sql: "update Address set Address2 = null where AddressId = @addressId",
					parameters: new System.Data.SqlClient.SqlParameter("@addressId", addressId));

			if(string.IsNullOrEmpty(((TextBox)dtlAddressList.FindControl("txtSuite")).Text))
				DB.ExecuteSQL(
					sql: "update Address set Suite = null where AddressId = @addressId",
					parameters: new System.Data.SqlClient.SqlParameter("@addressId", addressId));

			if(string.IsNullOrEmpty(((TextBox)dtlAddressList.FindControl("txtPhone")).Text))
				DB.ExecuteSQL(
					sql: "update Address set Phone = null where AddressId = @addressId",
					parameters: new System.Data.SqlClient.SqlParameter("@addressId", addressId));

			if(string.IsNullOrEmpty(((TextBox)dtlAddressList.FindControl("txtEmail")).Text))
				DB.ExecuteSQL(
					sql: "update Address set Email = null where AddressId = @addressId",
					parameters: new System.Data.SqlClient.SqlParameter("@addressId", addressId));
		}
	}
}
