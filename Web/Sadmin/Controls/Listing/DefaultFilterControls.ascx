<%@ Control Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontControls.Listing.DefaultFilterControls" Codebehind="DefaultFilterControls.ascx.cs" %>

<div class="panel panel-default">
	<div class="filter-heading panel-heading clearfix">
		<h4 class="panel-title">
			<a data-toggle="collapse" href="#filterCollapse">
				<asp:Literal Text="<%$Tokens:StringResource, admin.common.FilterLabel %>" runat="server" />
			</a>
		</h4>
	</div>
	<div id="filterCollapse" class="panel-collapse collapse in">
		<asp:Panel CssClass="panel-body" runat="server">
			<div class="normal-filters">
				<asp:PlaceHolder runat="server" ID="FilterControlsPlaceholder" />
			</div>
			<asp:Panel runat="server" Visible='<%# ((IEnumerable<AspDotNetStorefrontControls.Listing.FilterControl>)DataBinder.Eval(Container, "ExpandableFilters")).Any() %>'>
				<a href="#" id="ShowExpandableFilters"><i class="fa fa-plus-square-o"></i> Show More Filters</a>
				<a href="#" id="HideExpandableFilters"><i class="fa fa-minus-square-o"></i> Hide More Filters</a>
			</asp:Panel>

			<div id="ExpandableFilters" class="expandable-filters">
				<asp:PlaceHolder runat="server" ID="ExpandableFilterControlsPlaceholder" />
			</div>
		</asp:Panel>
	</div>
</div>
