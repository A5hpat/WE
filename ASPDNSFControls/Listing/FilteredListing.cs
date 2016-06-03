// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontControls.Listing
{
	public class FilteredListing : CompositeControl
	{
		const int FallbackDisplayedPageSelectorCount = 9;
		const int FallbackPageSize = 10;

		const string QueryTemplateResourceName = "AspDotNetStorefrontControls.Listing.Resource.QueryTemplate.sql";
		const string RowSortTemplateResourceName = "AspDotNetStorefrontControls.Listing.Resource.RowSortTemplate.sql";

		const string DefaultPagingControlsPath = "Controls/Listing/DefaultPagingControls.ascx";
		const string DefaultFilterControlsPath = "Controls/Listing/DefaultFilterControls.ascx";
		const string DefaultActionRowPath = "Controls/Listing/DefaultActionRow.ascx";
		const string DefaultFilterActionsPath = "Controls/Listing/DefaultFilterActions.ascx";

		public event EventHandler ListingTemplateInstantiated;

		readonly Pager _Pager;
		readonly FilterControlList _Filters;
		readonly FilterControlList _ExpandableFilters;
		readonly SqlDataSource _SqlDataSource;
		readonly ParameterCollection _SqlParameters;
		readonly QueryBuilder _QueryBuilder;
		readonly QueryBuilder.Templates _QueryBuilderTemplates;
		readonly PagingControlsContainer _UpperPagingControls;
		readonly PagingControlsContainer _LowerPagingControls;

		protected override HtmlTextWriterTag TagKey
		{ get { return HtmlTextWriterTag.Div; } }

		[PersistenceMode(PersistenceMode.InnerProperty)]
		[TemplateContainer(typeof(FilteredListing))]
		[TemplateInstance(TemplateInstance.Single)]
		public ITemplate FiltersTemplate
		{ get; set; }

		[PersistenceMode(PersistenceMode.InnerProperty)]
		[MergableProperty(false)]
		public FilterControlList Filters
		{ get { return _Filters; } }

		[PersistenceMode(PersistenceMode.InnerProperty)]
		[MergableProperty(false)]
		public FilterControlList ExpandableFilters
		{ get { return _ExpandableFilters; } }

		[PersistenceMode(PersistenceMode.InnerProperty)]
		[MergableProperty(false)]
		public ParameterCollection SqlParameters
		{ get { return _SqlParameters; } }

		[PersistenceMode(PersistenceMode.InnerProperty)]
		[TemplateContainer(typeof(FilteredListing))]
		[TemplateInstance(TemplateInstance.Single)]
		public ITemplate ListingTemplate
		{ get; set; }

		[PersistenceMode(PersistenceMode.InnerProperty)]
		[TemplateContainer(typeof(ActionRowContainer))]
		[TemplateInstance(TemplateInstance.Multiple)]
		public ITemplate ActionRowTemplate
		{ get; set; }

		[PersistenceMode(PersistenceMode.InnerProperty)]
		[TemplateContainer(typeof(ActionRowContainer))]
		[TemplateInstance(TemplateInstance.Multiple)]
		public ITemplate ActionBarTemplate
		{ get; set; }

		[PersistenceMode(PersistenceMode.InnerProperty)]
		[TemplateContainer(typeof(PagingControlsContainer))]
		[TemplateInstance(TemplateInstance.Multiple)]
		public ITemplate PagingControlsTemplate
		{ get; set; }

		public string SqlQuery
		{
			get { return (string)ViewState["SqlQuery"]; }
			set { ViewState["SqlQuery"] = value; }
		}

		public string SortExpression
		{
			get { return (string)ViewState["SortExpression"]; }
			set { ViewState["SortExpression"] = value; }
		}

		public SortDirection SortDirection
		{
			get { return (SortDirection?)ViewState["SortDirection"] ?? SortDirection.Ascending; }
			set { ViewState["SortDirection"] = value; }
		}

		public bool LocaleSelectionEnabled
		{
			get { return (bool?)ViewState["LocaleSelectionEnabled"] ?? false; }
			set { ViewState["LocaleSelectionEnabled"] = value; }
		}

		[TypeConverter(typeof(Int32ArrayConverter))]
		public int[] PageSizes
		{
			get { return (int[])ViewState["PageSizes"]; }
			set { ViewState["PageSizes"] = value; }
		}

		public int? DefaultPageSize
		{
			get { return (int?)ViewState["DefaultPageSize"]; }
			set { ViewState["DefaultPageSize"] = value; }
		}

		string CurrentCustomerLocale
		{ get { return AppLogic.GetCurrentCustomer().LocaleSetting; } }

		int? SelectedPageSize
		{
			get
			{
				int parsed;
				return Int32.TryParse(Page.Request.QueryString["paging.size"], out parsed)
					? (int?)parsed
					: null;
			}
		}

		long? SelectedPageStart
		{
			get
			{
				long parsed;
				return Int64.TryParse(Page.Request.QueryString["paging.start"], out parsed)
					? (long?)parsed
					: null;
			}
		}

		string SelectedDisplayLocale
		{
			get
			{
				var value = Page.Request.QueryString["locale.display"];
				return String.IsNullOrWhiteSpace(value)
					? null
					: value;
			}
		}

		string SelectedSortExpression
		{
			get
			{
				var value = Page.Request.QueryString["sorting.expression"];
				return String.IsNullOrWhiteSpace(value)
					? null
					: value;
			}
		}

		SortDirection? SelectedSortDirection
		{
			get
			{
				SortDirection parsed;
				return Enum.TryParse<SortDirection>(Page.Request.QueryString["sorting.direction"], true, out parsed)
					? (SortDirection?)parsed
					: null;
			}
		}

		int[] EffectivePageSizes;
		int EffectivePageSize;
		long EffectivePageStart;
		int EffectiveDisplayedPageSelectorCount;
		string EffectiveDisplayLocale;
		string EffectiveSortExpression;
		SortDirection EffectiveSortDirection;
		bool EffectiveLocaleSelectionEnabled;

		public FilteredListing()
		{
			_Pager = new Pager();
			_Filters = new FilterControlList();
			_SqlParameters = new ParameterCollection();
			_ExpandableFilters = new FilterControlList();
			_SqlDataSource = new SqlDataSource();
			_QueryBuilder = new QueryBuilder();
			_UpperPagingControls = new PagingControlsContainer();
			_LowerPagingControls = new PagingControlsContainer();

			// Load query builder templates from embedded resources
			using(var queryTemplateStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(QueryTemplateResourceName))
			using(var rowSortTemplateStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(RowSortTemplateResourceName))
			using(var queryTemplateReader = new System.IO.StreamReader(queryTemplateStream))
			using(var rowSortTemplateReader = new System.IO.StreamReader(rowSortTemplateStream))
				_QueryBuilderTemplates = new QueryBuilder.Templates(queryTemplateReader.ReadToEnd(), rowSortTemplateReader.ReadToEnd());
		}

		protected override void OnInit(EventArgs e)
		{
			Page.ClientScript.RegisterClientScriptInclude("filteredListing", "Scripts/filteredListing.js");
			Page.ClientScript.RegisterClientScriptInclude("filterState", "Scripts/filterState.js");

			CssClass = "js-filtered-listing";

			EffectivePageSizes = PageSizes
				?? new[] { 10, 50, 100 }		// Hard-code some default page sizes for now
				?? new[] { DefaultPageSize ?? FallbackPageSize };

			EffectivePageSize =
				SelectedPageSize
				?? DefaultPageSize
				?? EffectivePageSizes.Cast<int?>().FirstOrDefault()
				?? FallbackPageSize;

			EffectivePageStart = SelectedPageStart ?? 0;

			EffectiveDisplayedPageSelectorCount = FallbackDisplayedPageSelectorCount;

			EffectiveDisplayLocale = Page.Request.QueryString["locale.selection"] ?? CurrentCustomerLocale;

			EffectiveSortExpression = String.IsNullOrWhiteSpace(SelectedSortExpression)
				? SortExpression
				: Encoding.UTF8.GetString(Convert.FromBase64String(SelectedSortExpression));

			EffectiveSortDirection = SelectedSortDirection ?? SortDirection;

			EffectiveLocaleSelectionEnabled = LocaleSelectionEnabled && AppLogic.NumLocaleSettingsInstalled() > 1;

			base.OnInit(e);
		}

		protected override void CreateChildControls()
		{
			Controls.Clear();

			var effectiveActionRowTemplate = ActionRowTemplate ?? Page.LoadTemplate(DefaultActionRowPath);
			var effectiveFiltersTemplate = FiltersTemplate ?? Page.LoadTemplate(DefaultFilterControlsPath);
			var effectivePagingControlsTemplate = PagingControlsTemplate ?? Page.LoadTemplate(DefaultPagingControlsPath);

			// Add upper action row
			if(ActionBarTemplate != null || EffectiveLocaleSelectionEnabled)
			{
				var upperActionRow = new ActionRowContainer(EffectiveLocaleSelectionEnabled, ActionBarTemplate != null, EffectiveDisplayLocale);
				Controls.Add(upperActionRow);
				upperActionRow.CssClass = "listing-action-row listing-action-row-upper";

				effectiveActionRowTemplate.InstantiateIn(upperActionRow);
				var upperActionBarPlaceholder = upperActionRow
					.FindControlRecursive("ActionBarPlaceholder")
					.FirstOrDefault();

				if(upperActionBarPlaceholder != null && ActionBarTemplate != null)
					ActionBarTemplate.InstantiateIn(upperActionBarPlaceholder);
			}

			// Add filters
			if(Filters.Any() || ExpandableFilters.Any())
			{
				var filterContainer = new Panel();
				Controls.Add(filterContainer);
				filterContainer.CssClass = "listing-filters";
				effectiveFiltersTemplate.InstantiateIn(filterContainer);

				// Update filter contexts
				int controlIndex = 0;
				var displayLocaleParameter = new Parameter(QueryBuilder.DisplayLocaleParameterName, DbType.String, EffectiveDisplayLocale);
				var currentCustomerLocaleParameter = new Parameter(QueryBuilder.CurrentCustomerLocaleParameterName, DbType.String, CurrentCustomerLocale);

				foreach(var filterControl in _Filters ?? Enumerable.Empty<FilterControl>())
					filterControl.SetContext(
						new FilterControlContext(
							displayLocaleParameter,
							currentCustomerLocaleParameter,
							controlIndex++));

				foreach(var filterControl in _ExpandableFilters ?? Enumerable.Empty<FilterControl>())
					filterControl.SetContext(
						new FilterControlContext(
							displayLocaleParameter,
							currentCustomerLocaleParameter,
							controlIndex++));

				// Find control placeholders
				var filterControlsPlaceholder = filterContainer
					.FindControlRecursive("FilterControlsPlaceholder", 64)
					.FirstOrDefault();

				var expandableFilterControlsPlaceholder = filterContainer
					.FindControlRecursive("ExpandableFilterControlsPlaceholder", 64)
					.FirstOrDefault();

				// Group filters for each placeholder
				var defaultFilterActionsTemplate = Page.LoadTemplate(DefaultFilterActionsPath);
				var upperFilterActionsContainer = new PlaceHolder();
				var lowerFilterActionsContainer = new PlaceHolder();
				defaultFilterActionsTemplate.InstantiateIn(upperFilterActionsContainer);
				defaultFilterActionsTemplate.InstantiateIn(lowerFilterActionsContainer);

				var normalFilters =
					(expandableFilterControlsPlaceholder == null
						? (_Filters ?? Enumerable.Empty<Control>()).Concat(_ExpandableFilters)
						: (_Filters ?? Enumerable.Empty<Control>()))
					.Concat(new Control[] { upperFilterActionsContainer });

				var expandableFilters = expandableFilterControlsPlaceholder == null || _ExpandableFilters == null
					? Enumerable.Empty<Control>()
					: _ExpandableFilters.Concat(new Control[] { lowerFilterActionsContainer });

				// Apply filters to placeholders
				if(filterControlsPlaceholder != null)
					PopulateContainerWithFilterControls(normalFilters, filterControlsPlaceholder);

				if(expandableFilterControlsPlaceholder != null)
					PopulateContainerWithFilterControls(expandableFilters, expandableFilterControlsPlaceholder);
			}

			// Initialize filter query
			var filterClauses = BuildFilterClauses(
				(_Filters ?? Enumerable.Empty<FilterControl>())
				.Concat(_ExpandableFilters ?? Enumerable.Empty<FilterControl>()));

			var parameterizedQuery = _QueryBuilder.BuildDataSourceQuery(
				templates: _QueryBuilderTemplates,
				queryContext: new QueryBuilder.QueryContext(SqlQuery, EffectiveSortExpression, EffectiveSortDirection, EffectivePageStart, EffectivePageSize, EffectiveDisplayLocale, CurrentCustomerLocale),
				filterClauses: filterClauses);

			_SqlDataSource.SelectParameters.Clear();
			foreach(var parameter in parameterizedQuery.Parameters)
				_SqlDataSource.SelectParameters.Add(parameter);

			foreach(Parameter parameter in SqlParameters)
				_SqlDataSource.SelectParameters.Add(parameter);

			// Setup SqlDataSource
			_SqlDataSource.ID = "FilteredListingDataSource";
			_SqlDataSource.ConnectionString = DB.GetDBConn();
			_SqlDataSource.ProviderName = "System.Data.SqlClient";
			_SqlDataSource.DataSourceMode = SqlDataSourceMode.DataSet;
			_SqlDataSource.SelectCommandType = SqlDataSourceCommandType.Text;
			_SqlDataSource.SelectCommand = parameterizedQuery.Sql;
			_SqlDataSource.CancelSelectOnNullParameter = false;
			_SqlDataSource.EnableViewState = false;
			_SqlDataSource.Selecting += SqlDataSource_Selecting;
			_SqlDataSource.Selected += SqlDataSource_Selected;
			Controls.Add(_SqlDataSource);

			// Add upper paging controls
			_UpperPagingControls.Controls.Clear();
			Controls.Add(_UpperPagingControls);
			_UpperPagingControls.CssClass = "listing-paging listing-paging-upper";
			effectivePagingControlsTemplate.InstantiateIn(_UpperPagingControls);

			// Add listing template
			var listing = new NamingContainerPanel();
			Controls.Add(listing);
			listing.CssClass = "listing";
			if(ListingTemplate != null)
			{
				ListingTemplate.InstantiateIn(listing);

				if(ListingTemplateInstantiated != null)
					ListingTemplateInstantiated(this, EventArgs.Empty);
			}

			// Add lower paging controls
			_LowerPagingControls.Controls.Clear();
			Controls.Add(_LowerPagingControls);
			_LowerPagingControls.CssClass = "listing-paging listing-paging-lower";
			effectivePagingControlsTemplate.InstantiateIn(_LowerPagingControls);

			// Add lower action bar
			if(ActionBarTemplate != null || EffectiveLocaleSelectionEnabled)
			{
				var lowerActionRow = new ActionRowContainer(EffectiveLocaleSelectionEnabled, ActionBarTemplate != null, EffectiveDisplayLocale);
				Controls.Add(lowerActionRow);
				lowerActionRow.CssClass = "listing-action-row listing-action-row-lower";

				effectiveActionRowTemplate.InstantiateIn(lowerActionRow);
				var lowerActionBarPlaceholder = lowerActionRow
					.FindControlRecursive("ActionBarPlaceholder")
					.FirstOrDefault();

				if(lowerActionBarPlaceholder != null && ActionBarTemplate != null)
					ActionBarTemplate.InstantiateIn(lowerActionBarPlaceholder);
			}

			ClearChildViewState();
		}

		void PopulateContainerWithFilterControls(IEnumerable<Control> filterControls, Control container)
		{
			// Make Bootstrap rows of filter controls
			var columnCount = 0;
			Control rowPanel = null;
			foreach(var filterControl in filterControls)
			{
				if(columnCount <= 0)
				{
					rowPanel = new Panel { CssClass = "row" };
					container.Controls.Add(rowPanel);
					columnCount = 12;
				}

				rowPanel.Controls.Add(filterControl);
				if(filterControl.Visible)
					columnCount -= filterControl is IGridLayoutControl
						? ((IGridLayoutControl)filterControl).GridColumns
						: 3;	// Default if not specified
			}
		}

		public override void RenderBeginTag(HtmlTextWriter writer)
		{
			if(!String.IsNullOrEmpty(SortExpression))
				writer.AddAttribute("data-sorting-default-expression", Convert.ToBase64String(Encoding.UTF8.GetBytes(SortExpression)));

			writer.AddAttribute("data-sorting-default-direction", SortDirection.ToString().ToLower());

			base.RenderBeginTag(writer);
		}

		void SqlDataSource_Selecting(object sender, SqlDataSourceSelectingEventArgs e)
		{
			// Add output parameters if they don't already exist
			foreach(var outputParam in new[] { "@_paging_firstRowIndex", "@_paging_lastRowIndex", "@_paging_rowCount", "@_paging_pageIndex", "@_paging_pageCount" })
				if(!e.Command.Parameters.Contains(outputParam))
					e.Command.Parameters.Add(new SqlParameter
						{
							ParameterName = outputParam,
							DbType = DbType.Int64,
							Direction = ParameterDirection.Output
						});
		}

		void SqlDataSource_Selected(object sender, SqlDataSourceStatusEventArgs e)
		{
			var pagerContext = new PagerContext(EffectivePageSizes, EffectivePageSize, EffectiveDisplayedPageSelectorCount);
			var pageContext = _Pager.CreatePageContext(
				pagerContext,
				firstItemIndex: Math.Max(0, e.Command.Parameters["@_paging_firstRowIndex"].Value<long>() - 1),		// Convert from 1-based to 0-based
				lastItemIndex: Math.Max(0, e.Command.Parameters["@_paging_lastRowIndex"].Value<long>() - 1),		// Convert from 1-based to 0-based
				itemCount: e.Command.Parameters["@_paging_rowCount"].Value<long>(),
				pageIndex: Math.Max(0, e.Command.Parameters["@_paging_pageIndex"].Value<long>() - 1),				// Convert from 1-based to 0-based
				pageCount: e.Command.Parameters["@_paging_pageCount"].Value<long>());

			_UpperPagingControls.UpdatePaging(pagerContext, pageContext);
			_LowerPagingControls.UpdatePaging(pagerContext, pageContext);
		}

		IEnumerable<FilterClause> BuildFilterClauses(IEnumerable<FilterControl> filters)
		{
			return filters
				.Select(filter => filter.GetFilterClause())
				.Where(segment => segment != null)
				.ToArray();
		}

		/// <summary>
		/// Rebinds the data source and rebuilds the control tree.
		/// </summary>
		public void Rebind()
		{
			RecreateChildControls();
		}

		public ParameterizedSqlQuery GetFilterWhereClause()
		{
			var filterClauses = BuildFilterClauses(
				(_Filters ?? Enumerable.Empty<FilterControl>())
				.Concat(_ExpandableFilters ?? Enumerable.Empty<FilterControl>()));

			return _QueryBuilder.BuildFilterWhereClause(
				queryContext: new QueryBuilder.QueryContext(SqlQuery, EffectiveSortExpression, EffectiveSortDirection, EffectivePageStart, EffectivePageSize, EffectiveDisplayLocale, CurrentCustomerLocale),
				filterClauses: filterClauses,
				dataSourceControl: _SqlDataSource);
		}
	}
}
