<%@ Control Language="C#" Inherits="AspDotNetStorefrontControls.Listing.DefaultFilterActions" Codebehind="DefaultFilterActions.ascx.cs" %>

<label>&nbsp;</label>
<div class="filter-actions">
	<button type="button" class="btn btn-default js-filter-reset">
		<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.clearfilter %>" />
	</button>
	<button type="button" class="btn btn-primary js-filter-submit">
		<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.applyfilter %>" />
	</button>
</div>
