<%@ Control Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontControls.Listing.DefaultPagingControls" Codebehind="DefaultPagingControls.ascx.cs" %>

<%@ Import Namespace="AspDotNetStorefrontControls.Listing" %>
<%@ Import Namespace="Newtonsoft.Json" %>
<%@ Import Namespace="Newtonsoft.Json.Linq" %>

<div class="pager-container">
	<div>
		<div class="row">

			<div class="col-sm-6">
				<ul class="pagination pagination-sm">
					<asp:Repeater runat="server" DataSource='<%# DataBinder.Eval(Container, "PageContext.PageLinks") %>'>
						<ItemTemplate>
							<li class="<%# (bool)Eval("Current") ? "active" : String.Empty %>">
								<a
									class="js-paging-selector"
									data-paging-enabled='<%# JToken.FromObject(Eval("Enabled")).ToString(Formatting.None) %>'
									data-paging-index='<%# Eval("ItemIndex") %>'>
									<%# Eval("Display") %>
								</a>
							</li>
						</ItemTemplate>
					</asp:Repeater>
				</ul>
			</div>

			<div class="col-sm-6 text-right">

				<div class="form-inline">
					<div class="form-group">
				<span class="page-size">
					<label>
						Page size: 
						<select
							class="input-sm js-paging-size"
							data-paging-page-sizes='<%# JArray.FromObject(DataBinder.Eval(Container, "PagerContext.PageSizes")).ToString(Formatting.None) %>'
							data-paging-page-size='<%# DataBinder.Eval(Container, "PagerContext.PageSize") %>'>
						</select>
					</label>
				</span>

						<span class="page-number">
							<label>Page:</label>
							<input
								type="text"
								class="form-control input-sm text-3 js-paging-jump-index"
								value='<%# (long)DataBinder.Eval(Container, "PageContext.PageIndex") + 1 %>'
								name="pagingJumpIndex"
								data-bv-integer="true"
								data-bv-integer-message="Must be a number"
								data-bv-lessthan="true"
								data-bv-lessthan-value='<%# DataBinder.Eval(Container, "PageContext.PageCount") %>'
								data-bv-lessthan-message='<%# DataBinder.Eval(Container, "PageContext.PageCount", "Must be {0} or less") %>'
								data-bv-greaterthan="true"
								data-bv-greaterthan-value='1'
								data-bv-greaterthan-message="Must be 1 or more" />
							<span>of <%# DataBinder.Eval(Container, "PageContext.PageCount") %></span>
						</span>
					</div>

					<span class="page-nav-button">
						<button type="button" class="btn btn-default btn-sm js-paging-jump">Go to Page</button>
					</span>
				</div>

			</div>
		</div>
	</div>
</div>