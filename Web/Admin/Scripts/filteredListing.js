var FilteredListing = (function($) {

	var config = {
		filtering: {
			dataKey: {
				name: 'filterName',
				applyEmptyValue: 'filterApplyEmptyValue'
			},

			selector: {
				filterContainer: '.listing-filters',
				expandableFilters: '#ExpandableFilters',
				filterActionsContainer: '.filter-actions-container',
				submitButton: '.js-filter-submit',
				resetButton: '.js-filter-reset',
				showExpandableFiltersToggle: '#ShowExpandableFilters',
				hideExpandableFiltersToggle: '#HideExpandableFilters'
			}
		},

		paging: {
			dataKey: {
				enabled: 'pagingEnabled',
				index: 'pagingIndex',
				pageSizes: 'pagingPageSizes',
				selectedPageSize: 'pagingSelectedPageSize',
				defaultPageSize: 'pagingDefaultPageSize',
				fallbackPageSize: 'pagingFallbackPageSize'
			},

			queryKey: {
				size: 'paging.size',
				start: 'paging.start'
			},

			selector: {
				pagerContainer: '.pager-container',
				pageLink: 'a.js-paging-selector',
				pageSizeSelector: 'select.js-paging-size',
				pageJumpIndex: '.js-paging-jump-index',
				pageJumpButton: 'button.js-paging-jump'
			}
		},

		sorting: {
			dataKey: {
				expression: 'sortExpression',
				defaultExpression: 'sortingDefaultExpression',
				defaultDirection: 'sortingDefaultDirection'
			},

			queryKey: {
				direction: 'sorting.direction',
				expression: 'sorting.expression'
			},

			selector: {
				filteredListing: '.js-filtered-listing',
				columnHeader: '.js-sortable-gridview th a'
			},

			ascending: 'ascending',
			descending: 'descending'
		},

		locale: {
			queryKey: {
				selection: 'locale.selection'
			},

			selector: {
				localeSelector: 'select.js-locale-selector'
			}
		}
	};

	$(document).ready(function() {
		initializeFilters(config.filtering, config.paging.queryKey.start);
		initializePaging(config.paging);
		initializeSorting(config.sorting, config.paging.queryKey.start);
		initializeLocaleSelection(config.locale);
	});

	function initializeFilters(config, pagingStartQueryKey) {
		var $filterContainer = $(config.selector.filterContainer);
		var $expandableFilters = $(config.selector.expandableFilters);
		var $submitButton = $(config.selector.submitButton);
		var $resetButton = $(config.selector.resetButton);
		var $showExpandableFiltersToggle = $(config.selector.showExpandableFiltersToggle);
		var $hideExpandableFiltersToggle = $(config.selector.hideExpandableFiltersToggle);
		var $filterActionsContainer = $(config.selector.filterActionsContainer).first();

		initializeFilterExpanders();
		initializeFilterButtons();
		initializeFilterValidation();
		handleEnterKey();

		function initializeFilterExpanders() {
			// Set initial state
			var expandableFiltersVisibleKey = [location.protocol, '//', location.host, location.pathname, ':expandableFiltersVisible'].join('');
			var expandableFiltersVisible = sessionStorage.getItem(expandableFiltersVisibleKey) === 'true';
			if($expandableFilters == null)
				return;

			$expandableFilters.toggle(expandableFiltersVisible);
			$showExpandableFiltersToggle.toggle(!expandableFiltersVisible);
			$hideExpandableFiltersToggle.toggle(expandableFiltersVisible);
			$filterActionsContainer.toggle(!expandableFiltersVisible);

			// Enable toggle
			$($showExpandableFiltersToggle).add($hideExpandableFiltersToggle)
				.click(function(event) {
					$filterActionsContainer.slideToggle();
					$expandableFilters
						.slideToggle(function() {
							var isVisible = $expandableFilters.is(':visible');

							$showExpandableFiltersToggle.toggle(!isVisible);
							$hideExpandableFiltersToggle.toggle(isVisible);

							sessionStorage.setItem(expandableFiltersVisibleKey, isVisible.toString());
						});

					return false;
				});
		}

		function initializeFilterButtons() {
			$submitButton.click(function(e) {
				// Add/update the filter params in the query string
				setQueryStringValuesForFilters(function(filter) {
					var filterValue = filter.element.is(':checkbox')
						? filter.element.is(':checked')
						: filter.element.val();

					if(filterValue !== '' || filterValue === true || filter.applyEmptyValue)
						// Force to a string so boolean false is not removed by the query string library.
						return filterValue.toString();
					else
						// Remove the filter param from the query string by return no value for it
						return null;
				});

				return false;
			});

			$resetButton.click(function(e) {
				setQueryStringValuesForFilters(function(filter) {
					// Remove the filter param from the query string by return no value for it
					return null;
				});

				return false;
			});

			function setQueryStringValuesForFilters(filterValueCallback) {
				var queryString = $.query;

				$filterContainer.find('*:input:visible')
					.each(function() {
						var element = $(this);

						var filterName = element.data(config.dataKey.name) || element.attr('name');
						if(!filterName) 
							return;

						var filter = {
							name: filterName,
							element: element,
							applyEmptyValue: element.data(config.dataKey.applyEmptyValue) === true
						};

						var result = filterValueCallback && filterValueCallback(filter);
						if(result === undefined || result === null)
							queryString = queryString.remove(filterName);
						else
							queryString = queryString.set(filterName, result);
					});

				location.search = queryString
					.remove(pagingStartQueryKey)	// Reset page index on filter
					.toString();
			}
		}

		function initializeFilterValidation() {
			$filterContainer.bootstrapValidator({
				submitButtons: config.selector.submitButton,	// Has to be a selector string instead of a set of elements
				successClass: null
			});
		}

		function handleEnterKey() {
			$filterContainer.find('*:input').not('button').keypress(function(e) {
				if(e.which !== 13)
					return;

				$submitButton.not(':disabled').click();

				return false;
			});
		}
	}

	function initializePaging(config) {
		var $pagerContainer = $(config.selector.pagerContainer);
		var $pageLink = $(config.selector.pageLink);
		var $pageSizeSelector = $(config.selector.pageSizeSelector);
		var $pageJumpIndex = $(config.selector.pageJumpIndex);
		var $pageJumpButton = $(config.selector.pageJumpButton);

		initializePageIndexSelectors();
		initializePageSizeSelector();
		initializePageJump();
		initializePageJumpValidation();
		handleEnterKey();

		function initializePageIndexSelectors() {
			$pageLink.each(function() {
				var element = $(this);
				var enabled = element.data(config.dataKey.enabled);
				var index = element.data(config.dataKey.index);

				if(!enabled)
					return;

				var query;
				if(index > 0)
					query = $.query.set(config.queryKey.start, index);
				else
					query = $.query.remove(config.queryKey.start);

				element.attr('href', query.toString() || location.pathname);
			});
		}

		function initializePageSizeSelector() {
			$pageSizeSelector
				.each(function() {
					var element = $(this);
					var pageSizes = element.data(config.dataKey.pageSizes) || null;
					var currentPageSize = $.query.get(config.queryKey.size) || null;
					var selectedPageSize = element.data(config.dataKey.selectedPageSize) || null;
					var defaultPageSize = element.data(config.dataKey.defaultPageSize) || null;
					var fallbackPageSize = element.data(config.dataKey.fallbackPageSize) || null;

					// Populate the dropdown list with options
					var effectivePageSizes = pageSizes || [defaultPageSize || fallbackPageSize];
					for(var i = 0; i < effectivePageSizes.length; i++)
						element.append($('<option />', { value: effectivePageSizes[i] }).text(effectivePageSizes[i]));

					// Select the current page size
					var effectivePageSize = currentPageSize || selectedPageSize || defaultPageSize || pageSizes[0] || fallbackPageSize;
					element.val(effectivePageSize);
				})
				.change(function() {
					location.search = $.query
						.set(config.queryKey.size, $(this).val())
						.remove(config.queryKey.start);			// Reset page index on page size change
				});
		}

		function initializePageJump() {
			$pageJumpButton.click(function(e) {
				var container = $(this).closest(config.selector.pagerContainer);
				var jumpIndex = container.find(config.selector.pageJumpIndex).val();
				var pageSize = container.find(config.selector.pageSizeSelector).val();

				var start = ((Number(jumpIndex) || 1) - 1) * (Number(pageSize) || 1);

				if(start > 0)
					location.search = $.query.set(config.queryKey.start, start).toString();
				else
					location.search = $.query.remove(config.queryKey.start);

				return false;
			});
		}

		function initializePageJumpValidation() {
			$pagerContainer.bootstrapValidator({
				container: 'tooltip',
				verbose: false,
				submitButtons: config.selector.pageJumpButton,	// Has to be a selector string instead of a set of elements
				successClass: null
			});
		}

		function handleEnterKey() {
			$pageJumpIndex.keypress(function(e) {
				if(e.which !== 13)
					return;

				$(this)
					.closest(config.selector.pagerContainer)
					.find(config.selector.pageJumpButton).not(':disabled')
					.click();

				return false;
			});
		}
	}

	function initializeSorting(config, pagingStartQueryKey) {
		$filteredListing = $(config.selector.filteredListing);
		$columnHeader = $(config.selector.columnHeader);

		var defaultSortExpression = $filteredListing.data(config.dataKey.defaultExpression);
		var defaultSortDirection = $filteredListing.data(config.dataKey.defaultDirection);

		$columnHeader
			.each(function() {
				var element = $(this);

				// Extract the sort expression and store as data on the element
				var href = element.attr('href');

				var sortExpression = href.match(/','Sort\$(.*)'\)/)[1];
				if(sortExpression) {
					// The sortExpression is escaped, so we have to parse the escaped characters.
					// There has to be a better way than eval. Feel free to assume I'm an idiot and do the right thing here.
					var sortExpressionData = eval('"' + sortExpression + '"');
					var encodedSortExpression = btoa(sortExpressionData);
					element.data(config.dataKey.expression, encodedSortExpression);
				}

				// Clear hrefs
				element.attr('href', 'javascript:void(0)');
			})
			.click(function() {
				var element = $(this);
				var sortExpression = element.data(config.dataKey.expression);

				if(!sortExpression)
					return;

				var currentSortExpression = $.query.get(config.queryKey.expression) || defaultSortExpression;
				var currentSortDirection = $.query.get(config.queryKey.direction) || defaultSortDirection;

				var query = $.query;

				if(currentSortExpression === sortExpression)
					if(currentSortDirection === config.descending)
						query = query.set(config.queryKey.direction, config.ascending);
					else
						query = query.set(config.queryKey.direction, config.descending);
				else
					query = query
						.set(config.queryKey.expression, encodeURI(sortExpression))
						.set(config.queryKey.direction, config.ascending);

				if(query.get(config.queryKey.direction) === defaultSortDirection)
					query = query.remove(config.queryKey.direction);

				query = query.remove(pagingStartQueryKey);	// Reset page index on sort

				location.search = query;
			});
	}

	function initializeLocaleSelection(config) {
		$localeSelector = $(config.selector.localeSelector);

		$localeSelector.change(function() {
			location.search = $.query.set(config.queryKey.selection, $(this).val()).toString();
		});
	}

	return {
		// Allows asp.net checkboxes to auto-postback after prompting with a confirm message
		checkboxConfirmAndPostback: function(event) {
			if(confirm(event.data)) {
				// ASP.NET postback
				__doPostBack(this.id, '');
			} else {
				// Uncheck
				$(this).prop('checked', !$(this).prop('checked'));

				// Stop the event
				return false;
			}
		}
	};
})(jQuery);