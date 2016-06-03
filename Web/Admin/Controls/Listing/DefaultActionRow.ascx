<%@ Control Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontControls.Listing.DefaultActionRow" Codebehind="DefaultActionRow.ascx.cs" %>
<%@ Import Namespace="AspDotNetStorefrontCore" %>

<asp:SqlDataSource runat="server"
	ID="LocaleDataSource"
	ConnectionString="<%$ ConnectionStrings:DBConn %>"
	SelectCommand="select Name, Description from LocaleSetting order by DisplayOrder, Name" />

<div class="list-action-bar">
	<asp:Panel runat="server"
		CssClass='other-actions'
		Visible='<%# (bool)DataBinder.Eval(Container, "LocaleSelectionEnabled") %>'>
		Locale
			<asp:DropDownList runat="server"
				CssClass="text-sm js-locale-selector"
				DataSourceID="LocaleDataSource"
				DataValueField="Name"
				DataTextField="Description"
				OnDataBound="LocaleSelector_DataBound" />
	</asp:Panel>

	<%-- This is where the action bar buttons will be inserted --%>
	<asp:PlaceHolder runat="server" ID="ActionBarPlaceholder" />
</div>