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
using System.Linq;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class CustomerLevelCustomers : AspDotNetStorefront.Admin.AdminPageBase
	{
		int CustomerLevelId = CommonLogic.QueryStringUSInt("filter.0.0");

		string customerLevelName
		{
			get
			{
				return DisplayLevelName(CustomerLevelId);
			}
		}

		protected void Page_Load(object sender, System.EventArgs e)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");						
			
			lblLevelNameHeader.Text = string.IsNullOrEmpty(customerLevelName) ? string.Empty : string.Format(": {0} ({1}={2})", customerLevelName, AppLogic.GetString("admin.common.id", ThisCustomer.LocaleSetting), CustomerLevelId);
			lblLevelName.Text = string.IsNullOrEmpty(customerLevelName) ? string.Empty : string.Format(" - {0} ({1}={2})", customerLevelName, AppLogic.GetString("admin.common.id", ThisCustomer.LocaleSetting), CustomerLevelId);

			if(CustomerLevelId == 0)
				DisableAddToLevelButton();
		}

		protected void grdCustomersInLevel_RowCommand(object sender, GridViewCommandEventArgs e)
		{
			if(e.CommandName == "ClearLevel")
			{
				grdCustomersInLevel.EditIndex = -1;
				DB.ExecuteSQL(String.Format("update Customer set CustomerLevelID=0 where CustomerID={0}", e.CommandArgument));
				AlertMessageDisplay.PushAlertMessage(AppLogic.GetString("admin.customerlevel.RemovedFromLevel", ThisCustomer.LocaleSetting), AspDotNetStorefrontControls.AlertMessage.AlertType.Success);
			}

			FilteredListing.Rebind();
		}

		protected void btnAddToLevel_Click(object sender, EventArgs e)
		{
			string addValue = txtNewEmail.Text.Trim();

			if(!string.IsNullOrEmpty(addValue))
			{
				int customerIdToAdd = 0;
				int parsedCustomerID;

				if(int.TryParse(addValue, out parsedCustomerID))	//Admin entered a customer ID
				{
					customerIdToAdd = parsedCustomerID;

					if(!CheckForValidCustomer(customerIdToAdd))
						customerIdToAdd = 0;
				}
				else	//Admin entered an email
					customerIdToAdd = Customer.GetIDFromEMail(addValue);

				if(customerIdToAdd != 0)
				{
					string customerLevelNameToAdd = DisplayLevelName(CustomerLevelId);
					if(!CheckIfCustomerLevelCustomerExists(customerIdToAdd, CustomerLevelId))
					{ 						
						// clear the carts for this customer. This is to ensure their product pricing is correct
						// their current cart can have customer level pricing, not retail pricing, and this prevents that:
						DB.ExecuteSQL(String.Format("delete from shoppingcart where customerid={0}", customerIdToAdd));
						DB.ExecuteSQL(String.Format("delete from kitcart where customerid={0}", customerIdToAdd));

						DB.ExecuteSQL(String.Format("Update customer set CustomerLevelID={0} where CustomerID={1}", CustomerLevelId, customerIdToAdd));
						AlertMessageDisplay.PushAlertMessage(string.Format("{0}{1}", AppLogic.GetString("admin.customerlevel.CustomerAdded", ThisCustomer.LocaleSetting), string.IsNullOrEmpty(customerLevelNameToAdd) ? string.Empty : string.Format(": {0} ({1}={2})", customerLevelName, AppLogic.GetString("admin.common.id", ThisCustomer.LocaleSetting), CustomerLevelId)), AspDotNetStorefrontControls.AlertMessage.AlertType.Success);
					}
					else
					{
						AlertMessageDisplay.PushAlertMessage(string.Format("{0}{1}", AppLogic.GetString("admin.customerlevel.AlreadyExistsInLevel", ThisCustomer.LocaleSetting), string.IsNullOrEmpty(customerLevelNameToAdd) ? string.Empty : string.Format(": {0} ({1}={2})", customerLevelName, AppLogic.GetString("admin.common.id", ThisCustomer.LocaleSetting), CustomerLevelId)), AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
					}
				}
				else
				{
					AlertMessageDisplay.PushAlertMessage(AppLogic.GetString("admin.customerlevel.NotInDatabase", ThisCustomer.LocaleSetting), AspDotNetStorefrontControls.AlertMessage.AlertType.Warning);
				}

				txtNewEmail.Text = string.Empty;
				FilteredListing.Rebind();
			}
			else
			{
				AlertMessageDisplay.PushAlertMessage(AppLogic.GetString("admin.customerlevel.IdOrEmailRequired", ThisCustomer.LocaleSetting), AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
			}			
		}

		/// <summary>
		/// Gets the customer level name, returns string empty if no valid customer level name exists
		/// </summary>
		/// <param name="levelId"></param>
		/// <returns></returns>
		protected string DisplayLevelName(int? levelId)
		{
			string customerLevelName = null;
			if(levelId != null)
			{
				var sql = "SELECT Name FROM CustomerLevel WHERE CustomerLevelID = @CustomerLevelID";

				using(var connection = new SqlConnection(DB.GetDBConn()))
				{
					connection.Open();

					using(var command = new SqlCommand(sql, connection))
					{
						command.Parameters.Add(new SqlParameter("CustomerLevelID", levelId));

						var response = command.ExecuteScalar();
						if(!(response is DBNull) && (response != null))
							customerLevelName = (string)response;
					}					
				}
			}
			if(!string.IsNullOrEmpty(customerLevelName))
			{
				return XmlCommon.GetLocaleEntry(customerLevelName, ThisCustomer.LocaleSetting, false);
			}
			else
			{
				return string.Empty;
			}				
		}

		private void DisableAddToLevelButton()
		{
			litAddToLevelWarning.Visible = true;
			divAddToLevel.Visible = false;
			btnAddTolevel.Enabled = false;
		}

		/// <summary>
		/// Determines if the customer already exists in the customer level
		/// </summary>
		/// <param name="customerId"></param>
		/// <param name="levelId"></param>
		/// <returns></returns>
		protected bool CheckIfCustomerLevelCustomerExists(int? customerId, int? levelId)
		{
			var customerExistsInCustomerLevel = false;

			if(customerId != null && customerId > 0 && levelId != null)
			{
				var sql = "SELECT CustomerID FROM Customer WHERE CustomerLevelID = @CustomerLevelID AND CustomerID = @CustomerID";

				using(var connection = new SqlConnection(DB.GetDBConn()))
				{
					connection.Open();

					using(var command = new SqlCommand(sql, connection))
					{
						command.Parameters.Add(new SqlParameter("CustomerLevelID", levelId));
						command.Parameters.Add(new SqlParameter("CustomerID", customerId));

						var response = command.ExecuteScalar();
						if(!(response is DBNull) && (response != null))
							customerExistsInCustomerLevel = (int)response > 0;
					}
				}
			}
			return customerExistsInCustomerLevel;
		}

		/// <summary>
		/// Determines if the customer exists and is not deleted
		/// </summary>
		/// <param name="customerId"></param>
		/// <returns></returns>
		protected bool CheckForValidCustomer(int? customerId)
		{
			var customerIsValid = false;

			if(customerId != null && customerId > 0)
			{
				var sql = "SELECT CustomerID FROM Customer WHERE CustomerID = @CustomerID and deleted = 0";

				using(var connection = new SqlConnection(DB.GetDBConn()))
				{
					connection.Open();

					using(var command = new SqlCommand(sql, connection))
					{
						command.Parameters.Add(new SqlParameter("CustomerID", customerId));

						var response = command.ExecuteScalar();
						if(!(response is DBNull) && (response != null))
							customerIsValid = (int)response > 0;
					}
				}
			}
			return customerIsValid;
		}

		[System.Web.Services.WebMethodAttribute(), System.Web.Script.Services.ScriptMethodAttribute()]
		public static string[] GetCompletionList(string prefixText, int count, string contextKey)
		{
			using(var dbconn = DB.dbConn())
			{
				dbconn.Open();
				SqlParameter[] spa = { new SqlParameter("@prefixText", prefixText + '%'), new SqlParameter("@count", count) };
                using (var rsCustomer = DB.GetRS("SELECT TOP (@count) CustomerId, Email FROM Customer WITH (NOLOCK) WHERE (Email <> '' AND Email LIKE @prefixText) OR FirstName LIKE @prefixText OR LastName LIKE @prefixText OR CustomerId LIKE @prefixText", spa, dbconn))
				{
					var txtCustomers = new List<string>();
					string customerItem;

					using(var dtCustomer = new DataTable())
					{
						dtCustomer.Columns.Add("CustomerId");
						dtCustomer.Columns.Add("Email");
						dtCustomer.Load(rsCustomer);
						foreach(DataRow row in dtCustomer.Rows)
						{
							customerItem = AjaxControlToolkit.AutoCompleteExtender.CreateAutoCompleteItem(DB.RowField(row, "Email"), DB.RowField(row, "CustomerID"));
							txtCustomers.Add(customerItem);
						}
						return txtCustomers.ToArray();
					}
				}
			}
		}

	}
}
