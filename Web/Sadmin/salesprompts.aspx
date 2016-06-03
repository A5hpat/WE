<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.salesprompts" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="salesprompts.aspx.cs" %>

<%@ Register TagPrefix="aspdnsf" Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls.Listing" %>
<%@ Register TagPrefix="aspdnsf" TagName="StringFilter" Src="Controls/Listing/StringFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="BooleanFilter" Src="Controls/Listing/BooleanFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="DataQueryFilter" Src="Controls/Listing/DataQueryFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="LinkGroupProducts" Src="Controls/LinkGroupProducts.ascx" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<h1>
		<i class="fa fa-cube"></i>
		<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.Products %>" />
		<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.breadcrumbseparator %>" />
		<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.menu.salesprompts %>" />
	</h1>

	<aspdnsf:AlertMessage runat="server" ID="ctrlAlertMessage" />

	<aspdnsf:FilteredListing runat="server"
		ID="FilteredListing"
		SqlQuery="
			select {0}
				SalesPromptID,
				nullif(dbo.GetMlValue(Name, @_locale),'') [Name]
			from
				SalesPrompt 
			where 
				Deleted = 0 
				and {1}"
		SortExpression="nullif(dbo.GetMlValue(Name, @_locale),'')"
		LocaleSelectionEnabled="true">
		<ActionBarTemplate>
			<aspdnsf:LinkGroupProducts runat="server" SelectedLink="salesprompts.aspx" />
			<asp:HyperLink runat="server" CssClass="btn btn-action" Text="<%$ Tokens:StringResource, admin.editsalesprompt.CreateSalesPrompt %>" NavigateUrl="SalesPrompt.aspx" />
		</ActionBarTemplate>
		<ListingTemplate>
			<div class="white-ui-box">
				<asp:GridView runat="server"
					CssClass="table js-sortable-gridview"
					DataSourceID="FilteredListingDataSource"
					AutoGenerateColumns="False"
					OnRowCommand="RowCommand"
					AllowSorting="true"
					GridLines="None">
					<EmptyDataTemplate>
						<div class="alert alert-info">
							<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.EmptyDataTemplate.NoResults %>" />
						</div>
					</EmptyDataTemplate>
					<Columns>
						<asp:BoundField
							HeaderText="ID"
							HeaderStyle-Width="5%"
							SortExpression="SalesPromptID"
							DataField="SalesPromptID" />

						<asp:HyperLinkField
							HeaderText="Sales Prompt"
							SortExpression="nullif(dbo.GetMlValue(Name, @_locale),'')"
							DataTextField="Name"
							DataNavigateUrlFields="SalesPromptID"
							DataNavigateUrlFormatString="SalesPrompt.aspx?SalesPromptId={0}"
							Text="<%$Tokens:StringResource, admin.nolinktext %>" />

						<asp:TemplateField
							HeaderStyle-Width="5%">
							<ItemTemplate>
								<asp:LinkButton runat="Server"
									CssClass="delete-link fa-times"
									ToolTip="Delete"
									CommandName="<%# DeleteCommand %>"
									CommandArgument='<%# Eval("SalesPromptID") %>'
									OnClientClick="return confirm('Are you sure you want to delete this sales prompt?')"
									Text="<%$Tokens:StringResource, admin.common.Delete %>" />
							</ItemTemplate>
						</asp:TemplateField>
					</Columns>
				</asp:GridView>
			</div>
		</ListingTemplate>
	</aspdnsf:FilteredListing>
</asp:Content>
