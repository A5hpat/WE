// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Text;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontControls
{
	public partial class Admin_Controls_LocaleField : System.Web.UI.UserControl
	{

		/// <summary>
		/// Constructor
		/// </summary>
		public Admin_Controls_LocaleField()
		{
			Enabled = true;
			RequiredValidation = true;
		}

		# region "PROPERTIES"

			/// <summary>
			/// Gets text from the viewstate. Sets value to a default or sets it from a passed in value.
			/// </summary>
			public string Text
			{
				get
				{
					return (string)ViewState["LocaleText"] ?? string.Empty;
				}
				set
				{
					if(value != null)
					{
						ViewState["LocaleText"] = value.Trim();
					}
					else
					{
						ViewState["LocaleText"] = string.Empty;
					}
				}
			}

			/// <summary>
			/// Gets the field class. Sets value to a default or sets it from a passed in value.
			/// </summary>
			string textFieldClass;
			public string TextFieldClass
			{
				get
				{
					return SetTextFieldClass(textFieldClass);
				}
				set
				{
					textFieldClass = value;
				}
			}

			public string ValidationGroup { get; set; }
			public string DefaultLocaleSetting { get; set; }
			public bool Enabled { get; set; }
			public bool RequiredValidation { get; set; }

		# endregion

		# region "PAGE EVENTS"
		
			/// <summary>
			/// Page Load
			/// </summary>
			/// <param name="sender"></param>
			/// <param name="e"></param>
			protected void Page_Load(object sender, EventArgs e)
			{
				if(!IsPostBack)
				{
					BindData();
				}
			}

		# endregion

		# region "GET AND SET FIELDS"

			/// <summary>
			/// Gets the text from the local fields and wraps them in locale xml if more than one.
			/// </summary>
			/// <returns></returns>
			public string GetTextFromFields()
			{
				var localeString = new StringBuilder();
				if(AppLogic.NumLocaleSettingsInstalled() > 1)
				{
					localeString.Append("<ml>");
					foreach(RepeaterItem item in rptLocaleFields.Items)
					{
						var value = item.FindControl<TextBox>("txtValue").Text.Trim();
						var locale = item.FindControl<HiddenField>("localeName");
						localeString.AppendFormat(@"<locale name=""{0}"">{1}</locale>", locale.Value, value.Replace("<", String.Empty).Replace("&", String.Empty));
					}
					localeString.Append("</ml>");
				}
				else
				{
					localeString.Append(txtValue.Text.Trim());
				}

				return localeString.ToString();
			}

			/// <summary>
			/// Returns the passed in class or a default value if null.
			/// </summary>
			/// <param name="CssClass"></param>
			/// <returns></returns>
			public string SetTextFieldClass(string CssClass)
			{
				var fieldClass = string.IsNullOrEmpty(CssClass) ? "form-control" : CssClass;
				return fieldClass.Trim();
			}

		# endregion

		# region "DATABIND"

			/// <summary>
			/// Binds the locale fields in the control.
			/// </summary>
			public void BindData()
			{
				rptLocaleFields.DataSource = Localization.GetLocales();
				rptLocaleFields.DataBind();
				DataBind();
			}		

		# endregion
	}
}
