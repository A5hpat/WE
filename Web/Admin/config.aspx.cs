// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontControls.Config;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class ConfigEditor : AspDotNetStorefront.Admin.AdminPageBase
	{
		protected const string SaveCommand = "config:save";
		protected const string DeleteCommand = "config:delete";
		protected const string SaveAndCloseCommand = "config:saveandclose";

		protected int? ConfigId;
		protected IConfigEditorContext ConfigEditorContext;

		ConfigEditorMode Mode;

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			if(!Enum.TryParse<ConfigEditorMode>(Request.QueryString["mode"], true, out Mode))
			{
				// Requires a mode query string param
				Response.Redirect("./");
			}

			switch(Mode)
			{
				case ConfigEditorMode.AppConfig:
					ConfigEditorContext = new AppConfigEditorContext();
					break;

				case ConfigEditorMode.GlobalConfig:
					ConfigEditorContext = new GlobalConfigEditorContext();
					break;
			}

			Title = AppLogic.GetString(ConfigEditorContext.GetTitleStringResourceName(), AppLogic.GetCurrentCustomer().LocaleSetting);

			// Parse ID query string
			int configId;
			if(String.IsNullOrEmpty(Request.QueryString["id"]))
			{
				// Create mode
				ConfigId = null;
				Name.ReadOnly = false;
				DataBind();
			}
			else if(!Int32.TryParse(Request.QueryString["id"], out configId))
			{
				// Invalid mode
				ctrlAlertMessage.PushAlertMessage("Invalid config ID", AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
				EditorPanel.Visible = SaveButtonsTop.Visible = ActionBarBottom.Visible = false;
				// Don't databind
			}
			else
			{
				// Edit mode
				ConfigId = configId;
				DataBind();
			}
		}

		public override void DataBind()
		{
			base.DataBind();

			using(var connection = new SqlConnection(DB.GetDBConn()))
			{
				connection.Open();

				// Bind group dropdown list
				using(var command = new SqlCommand(ConfigEditorContext.GetGroupSelectSql(), connection))
				using(var reader = command.ExecuteReader())
					while(reader.Read())
						if(!string.IsNullOrEmpty(reader["GroupName"].ToString()))
							Group.Items.Add((string)reader["GroupName"]);

				// Bind form values
				using(var command = new SqlCommand(ConfigEditorContext.GetSelectSql(), connection))
				{
					command.Parameters.Add(new SqlParameter("configId", ConfigId == null ? DBNull.Value : (object)ConfigId));
					command.Parameters.Add(new SqlParameter("defaultStoreName", "Default For All Stores"));

					using(var reader = command.ExecuteReader())
					{
						if(!reader.Read() || (ConfigId != null && reader["ConfigID"] is DBNull))
						{
							ctrlAlertMessage.PushAlertMessage("Setting not found", AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
							EditorPanel.Visible = SaveButtonsTop.Visible = ActionBarBottom.Visible = false;
							return;
						}

						var valueType = String.IsNullOrEmpty(reader.Field("ValueType"))
							? "string"
							: reader.Field("ValueType");
						var allowedValues = reader.Field("AllowableValues");
						var superOnly = reader.FieldBool("SuperOnly");
						var groupName = reader.Field("GroupName");

						if(superOnly && !AppLogic.GetCurrentCustomer().IsAdminSuperUser)
						{
							ctrlAlertMessage.PushAlertMessage("Insufficient permission", AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
							EditorPanel.Visible = SaveButtonsTop.Visible = ActionBarBottom.Visible = false;
							return;
						}

						// General values
						Name.Text = reader.Field("ConfigName");
						Description.Text = reader.Field("Description");
						SuperOnly.SelectFirstByValue(superOnly.ToString());
						ValueType.SelectFirstByValue(valueType);
						AllowedValues.Text = allowedValues;

						if(!String.IsNullOrEmpty(groupName))
							Group.SelectFirstByValue(groupName);
						else
							ctrlAlertMessage.PushAlertMessage("This Setting does not have a Group.  Please choose one from the dropdown.", AlertMessage.AlertType.Warning);

						// Show allowed values when appropriate Eumeration, Multi-Select, and Dynamic Invoke
						var typesThatRequireAllowedValues = new[] { "enum", "multiselect", "invoke" };
						AllowedValuesPanel.Visible = ConfigId == null || typesThatRequireAllowedValues.Contains(valueType, StringComparer.OrdinalIgnoreCase);

						// Store values
						var storeValues = new List<StoreValue>();
						string defaultValue = null;
						do
						{
							var allowedValuesCollection = allowedValues
								.ParseAsDelimitedList()
								.ToArray();

							var storeId = reader.FieldInt("StoreID");
							var value = reader.Field("ConfigValue");
							if(storeId == 0)
								defaultValue = value;

							storeValues.Add(new StoreValue(
								storeId: storeId,
								storeName: reader.Field("StoreName"),
								value: value,
								exists: reader.FieldBool("StoreConfigExists"),
								allowedValues: valueType == "invoke"
									? BuildInvokedValueList(allowedValuesCollection)
									: allowedValuesCollection,
								defaultValue: storeId == 0
									? null
									: defaultValue,
								editor: GetEditor(valueType)));
						} while(reader.Read());

						StoreValues.DataSource = storeValues;
						StoreValues.DataBind();
					}
				}
			}
		}

		// Replace the editor placeholder with the correct editor control
		protected void StoreValues_ItemDataBound(object sender, RepeaterItemEventArgs e)
		{
			if(e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
				return;

			var placeholder = e.Item.FindControl("ValueEditorPlaceholder");
			var storeValue = (StoreValue)e.Item.DataItem;

			placeholder.Controls.Add(storeValue.Editor);
			storeValue.Editor.SetValue(storeValue.StoreId, storeValue.Value, storeValue.Exists, storeValue.AllowedValues, storeValue.DefaultValue);
			storeValue.Editor.DataBind();
		}

		protected void HandleCommands(object sender, CommandEventArgs e)
		{
			if(e.CommandName == SaveCommand || e.CommandName == SaveAndCloseCommand)
			{
				if(!ApplyValidation())
					return;

				var updatedId = SaveConfig();
				ctrlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.appconfig.SettingUpdated", AppLogic.GetCurrentCustomer().LocaleSetting), AlertMessage.AlertType.Success);

				if(e.CommandName == SaveAndCloseCommand)
					Response.Redirect(ReturnUrlTracker.GetReturnUrl());
				else if(ConfigId == null && updatedId != null)
					Response.Redirect(String.Format("Config.aspx?mode={0}&id={1}", Mode, updatedId));
				else
					DataBind();
			}
			else if(e.CommandName == DeleteCommand)
			{
				int storeId;
				if(Int32.TryParse((string)e.CommandArgument, out storeId))
				{
					DeleteConfig(storeId);
					DataBind();
				}
			}
		}

		// Resolve the correct editor control based on the config value type
		ConfigEditorControl GetEditor(string valueType)
		{
			switch(valueType.ToLower())
			{
				case "integer":
					return (ConfigEditorControl)LoadControl("Controls/Config/IntegerEditor.ascx");

				case "decimal":
				case "double":
					return (ConfigEditorControl)LoadControl("Controls/Config/DecimalEditor.ascx");

				case "boolean":
					return (ConfigEditorControl)LoadControl("Controls/Config/BooleanEditor.ascx");

				case "enum":
				case "invoke":
					return (ConfigEditorControl)LoadControl("Controls/Config/EnumEditor.ascx");

				case "multiselect":
					return (ConfigEditorControl)LoadControl("Controls/Config/MultiselectEditor.ascx");

				default:
					return (ConfigEditorControl)LoadControl("Controls/Config/StringEditor.ascx");
			}
		}

		// Dynamically call a method that returns an IEnumerable<string> to populate the allowed values
		IEnumerable<string> BuildInvokedValueList(IEnumerable<string> allowedValues)
		{
			if(allowedValues.Count() < 3)
				return new[] { "Invoke failed: allowableValues not formatted correctly" };

			// format should be
			// {FullyQualifiedName},MethodName
			// Fully qualified name format is Namespace.TypeName,AssemblyName
			var typeName = allowedValues.First();
			var assemblyName = allowedValues.Skip(1).First();
			var methodName = allowedValues.Skip(2).First();
			var fullyQualifiedName = String.Format("{0},{1}", typeName, assemblyName);

			var type = Type.GetType(fullyQualifiedName, false);
			if(type == null)
				return new[] { String.Format("Invoke failed: Could not find type {0}", fullyQualifiedName) };

			var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
			if(method == null)
				return new[] { String.Format("Invoke failed: Could not find pulic static method {0} on type {1}", methodName, fullyQualifiedName) };

			if(!typeof(IEnumerable<string>).IsAssignableFrom(method.ReturnType))
				return new[] { "Invoke failed: method must return IEnumerable<string>" };

			try
			{
				var instance = Activator.CreateInstance(type);
				return (IEnumerable<string>)method.Invoke(instance, null);
			}
			catch(Exception exception)
			{
				return new[] { String.Format("Invoke failed: {0}", exception) };
			}
		}

		bool ApplyValidation()
		{
			Validate();
			if(IsValid)
				return true;

			var errorMessages = Validators
				.OfType<IValidator>()
				.Where(validator => !validator.IsValid)
				.ToDictionary(
					validator => validator,
					validator => validator.ErrorMessage);

			var invalidControls = errorMessages.Keys
				.OfType<BaseValidator>()
				.Select(validator => validator.Parent.FindControl(validator.ControlToValidate))
				.Where(control => control != null)
				.OfType<WebControl>();

			ctrlAlertMessage.PushAlertMessage(String.Join("<br />", errorMessages.Values), AspDotNetStorefrontControls.AlertMessage.AlertType.Error);

			foreach(var control in invalidControls)
				control.CssClass += " has-error";

			return false;
		}

		// Insert or update the config for each store
		int? SaveConfig()
		{
			var isSuperOnly = StringComparer.OrdinalIgnoreCase.Equals(SuperOnly.SelectedValue, bool.TrueString);

			var storeValues = StoreValues.Items
				.OfType<RepeaterItem>()
				.SelectMany(item => item
					.FindControl("ValueEditorPlaceholder")
					.Controls
					.OfType<ConfigEditorControl>())
				.Select(editor => editor.GetValue())
				.Select(value => new
				{
					Exists = value.Item3,
					Name = Name.Text,
					StoreId = value.Item1,
					Description = Description.Text,
					ConfigValue = value.Item2 == null
						? DBNull.Value
						: (object)value.Item2,
					ValueType = ValueType.SelectedValue,
					AllowableValues = String.IsNullOrEmpty(AllowedValues.Text)
						? DBNull.Value
						: (object)AllowedValues.Text,
					GroupName = Group.SelectedValue,
					SuperOnly = isSuperOnly,
				})
				.Where(storeValue => storeValue.StoreId == 0 || storeValue.Exists || !(storeValue.ConfigValue is DBNull))
				.ToArray();

			int? newConfigId = null;
			using(var connection = new SqlConnection(DB.GetDBConn()))
			{
				connection.Open();

				foreach(var storeValue in storeValues)
					using(var command = new SqlCommand(ConfigEditorContext.GetUpsertSql(), connection))
					{
						command.Parameters.Add(new SqlParameter("name", storeValue.Name));
						command.Parameters.Add(new SqlParameter("storeId", storeValue.StoreId));
						command.Parameters.Add(new SqlParameter("description", storeValue.Description));
						command.Parameters.Add(new SqlParameter("configValue", storeValue.ConfigValue));
						command.Parameters.Add(new SqlParameter("valueType", storeValue.ValueType));
						command.Parameters.Add(new SqlParameter("allowableValues", storeValue.AllowableValues));
						command.Parameters.Add(new SqlParameter("groupName", storeValue.GroupName));
						command.Parameters.Add(new SqlParameter("superOnly", storeValue.SuperOnly));

						var response = command.ExecuteScalar();
						if(!(response is DBNull))
							newConfigId = (int?)response;
					}

				if(isSuperOnly)
					Security.LogEvent(
						securityAction: "Setting Updated Success",
						description: string.Format("Parameter Changed: {0}", Name.Text),
						customerUpdated: 0,
						updatedBy: ThisCustomer.CustomerID,
						customerSessionId: ThisCustomer.CurrentSessionID);
			}

			return newConfigId;
		}

		// Delete the config for the given store
		void DeleteConfig(int storeId)
		{
			using(var connection = new SqlConnection(DB.GetDBConn()))
			using(var command = new SqlCommand(ConfigEditorContext.GetDeleteSql(), connection))
			{
				command.Parameters.Add(new SqlParameter("name", Name.Text));
				command.Parameters.Add(new SqlParameter("storeId", storeId));

				connection.Open();
				var updatedCount = command.ExecuteNonQuery();

				if(StringComparer.OrdinalIgnoreCase.Equals(SuperOnly.SelectedValue, bool.TrueString))
					Security.LogEvent(
						securityAction: "Setting Deleted Success",
						description: string.Format("Parameter Deleted: {0}", Name.Text),
						customerUpdated: 0,
						updatedBy: ThisCustomer.CustomerID,
						customerSessionId: ThisCustomer.CurrentSessionID);
			}
		}

		// DTO for data binding
		class StoreValue
		{
			public int StoreId { get { return _StoreId; } }
			public string StoreName { get { return _StoreName; } }
			public string Value { get { return _Value; } }
			public bool Exists { get { return _Exists; } }
			public IEnumerable<string> AllowedValues { get { return _AllowedValues; } }
			public string DefaultValue { get { return _DefaultValue; } }
			public ConfigEditorControl Editor { get { return _Editor; } }

			readonly int _StoreId;
			readonly string _StoreName;
			readonly string _Value;
			readonly bool _Exists;
			readonly IEnumerable<string> _AllowedValues;
			readonly string _DefaultValue;
			readonly ConfigEditorControl _Editor;

			public StoreValue(int storeId, string storeName, string value, bool exists, IEnumerable<string> allowedValues, string defaultValue, ConfigEditorControl editor)
			{
				_StoreId = storeId;
				_StoreName = storeName;
				_Value = value;
				_Exists = exists;
				_AllowedValues = allowedValues;
				_DefaultValue = defaultValue;
				_Editor = editor;
			}
		}
	}

	enum ConfigEditorMode
	{
		AppConfig,
		GlobalConfig,
	}

	public interface IConfigEditorContext
	{
		string GetTitleStringResourceName();
		string GetHeadingStringResourceName();
		string GetSelectSql();
		string GetUpsertSql();
		string GetDeleteSql();
		string GetGroupSelectSql();
		string GetListingUrl();
	}

	class AppConfigEditorContext : IConfigEditorContext
	{
		// Given an appconfig ID, this select statement gets all configs with the same name from all stores.
		// It also includes the default for all stores and an indicator if that config exists for each store.
		const string SelectSql = @"
			;with
				SelectedConfig as (select * from AppConfig where AppConfigID = @configID),
				RelatedAppConfigs as (select AppConfig.* from AppConfig inner join SelectedConfig on SelectedConfig.Name = AppConfig.Name),
				AppConfigStores as (select StoreID, Name from Store UNION ALL select 0, @defaultStoreName)
			select 
				store.StoreID,
				store.Name StoreName,
				(case 
					when config.AppConfigID is null then 0 
					else 1 end) as StoreConfigExists,
				config.AppConfigID ConfigID, 
				config.Name ConfigName,
				config.ConfigValue,
				config.Description,
				config.GroupName, 
				config.SuperOnly,
				config.ValueType,
				config.AllowableValues
			from AppConfigStores store
				left join RelatedAppConfigs config on config.StoreID = store.StoreID
				order by store.StoreID";

		// This SQL will either update or insert an appconfig based on if it exists for the given store or not.
		const string UpsertSql = @"
			if exists (select * from AppConfig where Name = @name and StoreID = @storeId)
				update
					AppConfig 
				set
					Description = @description,
					ConfigValue = @configValue,
					ValueType = @valueType,
					AllowableValues = @allowableValues,
					GroupName = @groupName,
					SuperOnly = @superOnly
				where
					Name = @name
					and StoreID = @storeId
			else 
				insert into AppConfig (AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn, UpdatedOn)
				values (newid(), @storeId, @name, @description, @configValue, @valueType, @allowableValues, @groupName, @superOnly, 0, getdate(), getdate())

			select top 1 
				cast(scope_identity() as int) 
			from AppConfig 
				where Name = @name
			order by
				StoreId asc";

		const string DeleteSql = @"delete from AppConfig where Name = @name and @storeID > 0 and StoreID = @storeID";

		const string GroupSelectSql = "select distinct GroupName from dbo.AppConfig";

		public string GetTitleStringResourceName()
		{
			return "admin.title.appconfig";
		}

		public string GetHeadingStringResourceName()
		{
			return "admin.common.AppConfig";
		}

		public string GetSelectSql()
		{
			return SelectSql;
		}

		public string GetUpsertSql()
		{
			return UpsertSql;
		}

		public string GetDeleteSql()
		{
			return DeleteSql;
		}

		public string GetGroupSelectSql()
		{
			return GroupSelectSql;
		}

		public string GetListingUrl()
		{
			return "AppConfigs.aspx";
		}
	}

	class GlobalConfigEditorContext : IConfigEditorContext
	{
		const string SelectSql = @"
			SELECT
				GlobalConfigID ConfigID,
				Name ConfigName,
				Description,
				ConfigValue,
				ValueType,
				EnumValues AllowableValues,
				GroupName,
				SuperOnly
			FROM GlobalConfig 
			WHERE GlobalConfigID = @configID";

		const string UpdateSql = @"
			UPDATE
				GlobalConfig 
			SET Description = @description,
				ConfigValue = @configValue,
				EnumValues = @allowableValues,
				GroupName = @groupName,
				SuperOnly = @superOnly
			WHERE
				Name = @name";

		public string GetTitleStringResourceName()
		{
			return "admin.title.GlobalConfig";
		}

		public string GetHeadingStringResourceName()
		{
			return "admin.title.GlobalConfig";
		}

		public string GetSelectSql()
		{
			return SelectSql;
		}

		public string GetUpsertSql()
		{
			return UpdateSql;
		}

		public string GetDeleteSql()
		{
			return "delete from GlobalConfig where Name = @name";
		}

		public string GetGroupSelectSql()
		{
			return "select distinct GroupName from dbo.GlobalConfig";
		}

		public string GetListingUrl()
		{
			return "GlobalConfigs.aspx";
		}
	}
}
