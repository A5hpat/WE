// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class salesprompt : AspDotNetStorefront.Admin.AdminPageBase
	{
		protected int? SalesPromptId;

		protected override void OnInit(EventArgs e)
		{
			int parsedSalesPromptId;
			SalesPromptId = Int32.TryParse(Request.QueryString["SalesPromptId"], out parsedSalesPromptId)
				? parsedSalesPromptId
				: (int?)null;

			base.OnInit(e);
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			if(!IsPostBack)
				LoadSalesPrompt();
		}

		protected void LocaleSelector_SelectedLocaleChanged(object sender, EventArgs e)
		{
			LoadSalesPrompt();
		}

		protected void ValidateUniqueName(object source, ServerValidateEventArgs args)
		{
			try
			{
				using(var connection = new SqlConnection(DB.GetDBConn()))
				using(var command = new SqlCommand())
				{
					command.Connection = connection;
					command.CommandText = @"
					select count(*) 
					from SalesPrompt
					cross apply dbo.ParseMlLocales(SalesPrompt.Name) as ML
					where 
						(@id is null or SalesPromptID != @id)
						and Deleted = 0
						and (ML.Locale is NULL or ML.Locale = @locale) 
						and ML.Value = @name";
					command.Parameters.AddWithValue("@id", (object)SalesPromptId ?? DBNull.Value);
					command.Parameters.AddWithValue("@locale", LocaleSelector.GetSelectedLocale().Name);
					command.Parameters.AddWithValue("@name", args.Value);

					connection.Open();
					args.IsValid = (int)command.ExecuteScalar() == 0;
				}
			}
			catch(Exception exception)
			{
				ctlAlertMessage.PushAlertMessage(String.Format("Could not validate:<br />{0}", exception.ToString()), AlertMessage.AlertType.Error);
			}
		}

		protected void btnSubmit_Click(object sender, EventArgs e)
		{
			Page.Validate();
			if(Page.IsValid)
			{
				var newId = SaveSalesPrompt();
				if(newId.HasValue)
					Response.Redirect(String.Format("SalesPrompt.aspx?SalesPromptID={0}", newId.Value));
			}
		}

		protected void btnSaveAndClose_Click(object sender, EventArgs e)
		{
			Page.Validate();
			if(Page.IsValid)
			{
				SaveSalesPrompt();
				Response.Redirect(ReturnUrlTracker.GetReturnUrl());
			}
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			DataBind();
		}

		void LoadSalesPrompt()
		{
			if(SalesPromptId == null)
				return;

			try
			{
				using(var connection = new SqlConnection(DB.GetDBConn()))
				using(var command = new SqlCommand())
				{
					command.Connection = connection;
					command.CommandText = "select dbo.GetMlValue(Name, @locale) [Name] from SalesPrompt with(nolock) where SalesPromptID = @id";
					command.Parameters.AddWithValue("@id", SalesPromptId);
					command.Parameters.AddWithValue("@locale", LocaleSelector.GetSelectedLocale().Name);

					connection.Open();
					using(var reader = command.ExecuteReader())
						if(reader.Read())
							NameTextBox.Text = reader.Field("Name");
						else
							ctlAlertMessage.PushAlertMessage(String.Format("No Sales Prompt with ID {0} exists", SalesPromptId), AlertMessage.AlertType.Error);
				}
			}
			catch(Exception exception)
			{
				ctlAlertMessage.PushAlertMessage(String.Format("Could not load:<br />{0}", exception.ToString()), AlertMessage.AlertType.Error);
			}
		}

		int? SaveSalesPrompt()
		{
			try
			{
				var salesPromptName = SalesPromptId.HasValue
					? AppLogic.FormLocaleXml("Name", NameTextBox.Text.Trim(), LocaleSelector.GetSelectedLocale().Name, "SalesPrompt", SalesPromptId.Value)
					: AppLogic.FormLocaleXml(NameTextBox.Text.Trim(), LocaleSelector.GetSelectedLocale().Name);

				using(var connection = new SqlConnection(DB.GetDBConn()))
				using(var command = new SqlCommand())
				{
					command.Connection = connection;
					command.CommandText = SalesPromptId.HasValue
						? "update SalesPrompt set Name = @name where SalesPromptID = @id"
						: "insert SalesPrompt (Name) values (@name); select cast(scope_identity() as int);";
					command.Parameters.AddWithValue("@id", (object)SalesPromptId ?? DBNull.Value);
					command.Parameters.AddWithValue("@name", salesPromptName);

					connection.Open();
					var newId = (int?)command.ExecuteScalar();
					ctlAlertMessage.PushAlertMessage("Sales prompt saved", AlertMessage.AlertType.Success);

					return newId;
				}
			}
			catch(Exception exception)
			{
				ctlAlertMessage.PushAlertMessage(String.Format("Could not save:<br />{0}", exception.ToString()), AlertMessage.AlertType.Error);
				return null;
			}
		}
	}
}
