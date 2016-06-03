// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontControls
{
	public class LocaleSelector : WebControl, INamingContainer
	{
		public event EventHandler<EventArgs> SelectedLocaleChanged;

		readonly LocaleSource LocaleSource;
		readonly DropDownList ddlLocales;

		public LocaleSelector()
		{
			LocaleSource = new LocaleSource();
			ddlLocales = new DropDownList
				{
					AutoPostBack = true
				};
			ddlLocales.SelectedIndexChanged += ddlLocales_SelectedIndexChanged;
		}

		protected override void CreateChildControls()
		{
			Controls.Add(ddlLocales);

			if(!Page.IsPostBack)
			{
				var defaultLocale = LocaleSource.GetDefaultLocale();

				ddlLocales.Items.AddRange(
					LocaleSource
						.Locales
						.Select(locale => new ListItem(locale.Description, locale.Name)
						{
							Selected = locale == defaultLocale
						})
						.ToArray());
			}

			base.CreateChildControls();
		}

		void ddlLocales_SelectedIndexChanged(object sender, EventArgs e)
		{
			if(SelectedLocaleChanged != null)
				SelectedLocaleChanged(this, e);
		}

		public Locale GetSelectedLocale()
		{
			EnsureChildControls();

			var selectedLocale = LocaleSource
				.Locales
				.Where(locale => locale.Name == ddlLocales.SelectedValue)
				.FirstOrDefault();

			if(selectedLocale != null)
				return selectedLocale;

			return LocaleSource.GetDefaultLocale();
		}

		public bool HasMultipleLocales()
		{
			return LocaleSource.HasMultipleLocales();
		}
	}

	public class LocaleSource
	{
		public readonly IEnumerable<Locale> Locales;

		public LocaleSource()
		{
			Locales = LoadLocales().ToArray();
		}

		IEnumerable<Locale> LoadLocales()
		{
			foreach(DataRow record in Localization.GetLocales().Rows)
				yield return new Locale(DB.RowField(record, "Name"), DB.RowField(record, "Description"));
		}

		public Locale GetDefaultLocale()
		{
			var defaultLocale = Localization.GetDefaultLocale();

			var localeSelection = HttpContext.Current.Request.QueryString["locale.selection"];
			if(!string.IsNullOrEmpty(localeSelection))
			{
				localeSelection = Localization.CheckLocaleSettingForProperCase(localeSelection);
				if(Locales.Any(locale => locale.Name == localeSelection))
					defaultLocale = localeSelection;
			}

			return Locales
				.Where(l => l.Name == defaultLocale)
				.FirstOrDefault();
		}

		public bool HasMultipleLocales()
		{
			return Locales.Count() > 1;
		}
	}

	public class Locale
	{
		public string Description
		{ get { return _Description; } }

		public string Name
		{ get { return _Name; } }

		readonly string _Name;
		readonly string _Description;

		public Locale(string name, string description)
		{
			_Name = name;
			_Description = description;
		}
	}
}
