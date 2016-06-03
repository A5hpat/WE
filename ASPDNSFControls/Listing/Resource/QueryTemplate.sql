-- ------------------------------------------------------------------------------------------
-- Copyright AspDotNetStorefront.com.  All Rights Reserved.
-- http://www.aspdotnetstorefront.com
-- For details on this license please visit our homepage at the URL above.
-- THE ABOVE NOTICE MUST REMAIN INTACT.
-- ------------------------------------------------------------------------------------------
/*
	This query provides the core of the filtering, paging, and sorting used by
	the FilteredListing. All of the work is performed on the SQL server; only
	the final filtered, paged, and sorted results are returned from this query.

	This query is designed to wrap an arbitrary SQL query that contains a
	[Paging.RowIndex] column. That query is injected at the "{{0}}" token in this
	file.

	Sorting is performed by the Paging.RowIndex column in the injected query.
	See the RowSortTemplate.sql file for the column syntax.

	Filtering is performed by the where clause of the injected query combined
	with any provded SQL parameters.
	
	The bulk of this query is for supporting paging. It breaks up the requested
	row range (indicated by @_startingRowIndex and @_pageSize) into pages 
	correctly aligned on page boundaries calculated from the first row of data.
	It ensure that when requesting a page beyond those available, only the last
	page of data is retured, starting at the page boundary.
*/

-- Ensure temp tables are all cleared out
if object_id('tempdb..#NumberedRows') is not null 
	drop table #NumberedRows
if object_id('tempdb..#IndexConstrainedRows') is not null 
	drop table #IndexConstrainedRows
if object_id('tempdb..#PageConstrainedRows') is not null 
	drop table #PageConstrainedRows

declare @_localeId int,
		@_currentCustomerLocaleId int,
		@_paging_selectedPageExists bit,
		@paging_lastSelectedPageIndex int

-- Do a pre-lookup on the locale ID's
select @_localeId = LocaleSettingID 
from LocaleSetting 
where Name = @_locale

select @_currentCustomerLocaleId = LocaleSettingID 
from LocaleSetting 
where Name = @_currentCustomerLocale

-- Run the provided query and number the rows sequentially
;with __NumberedRows as (
{0}
)
select *
into #NumberedRows
from __NumberedRows

-- Determine the total number of rows and pages, and if the selected page is within the available rows
select 
	@_paging_rowCount = max([Paging.RowIndex]),
	@_paging_selectedPageExists  = max(case when [Paging.RowIndex] between (@_startingRowIndex + 1) and (@_startingRowIndex + @_pageSize) then 1 else 0 end),
	@_paging_pageCount = (max([Paging.RowIndex] - 1) / @_pageSize) + 1
from #NumberedRows

-- Pull out the rows for the selected page, or get the last page's worth of rows if the selected page is outside the available rows
select *
into #IndexConstrainedRows
from #NumberedRows
where 
	(@_paging_selectedPageExists = 1 and [Paging.RowIndex] between (@_startingRowIndex + 1) and (@_startingRowIndex + @_pageSize)) 
	or (@_paging_selectedPageExists = 0 and [Paging.RowIndex] between (@_paging_rowCount - @_pageSize + 1) and @_paging_rowCount)

-- Determine the highest page number in the selected page (this will be different than the selected page if it's outside the available rows)
select @paging_lastSelectedPageIndex = max([Paging.RowIndex] - 1) / @_pageSize
from #IndexConstrainedRows

-- Choose only the last page's worth of rows if we're outside the available rows, i.e. don't break over a page boundary
select *
into #PageConstrainedRows
from #IndexConstrainedRows
where
	(@_paging_selectedPageExists = 1) 
	or (@_paging_selectedPageExists = 0 and (([Paging.RowIndex] - 1) / @_pageSize) = @paging_lastSelectedPageIndex)

-- Get the first row, last row, and page index for the selected page after row and page constraints are applied
select
	@_paging_firstRowIndex = min([Paging.RowIndex]),
	@_paging_lastRowIndex = max([Paging.RowIndex]),
	@_paging_pageIndex = min(([Paging.RowIndex] - 1) / @_pageSize + 1)
from #PageConstrainedRows

-- Return the restults from the temp table
select *
from #PageConstrainedRows

-- Clean up
if object_id('tempdb..#NumberedRows') is not null	
	drop table #NumberedRows
if object_id('tempdb..#IndexConstrainedRows') is not null 
	drop table #IndexConstrainedRows
if object_id('tempdb..#PageConstrainedRows') is not null 
	drop table #PageConstrainedRows
