<%@ Control Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontControls.Listing.LocaleFilter" Codebehind="LocaleFilter.ascx.cs" %>

<asp:SqlDataSource runat="server"
	ID="ValueDataSource"
	ConnectionString="<%$ ConnectionStrings:DBConn %>"
	SelectCommand="select Name, Description from LocaleSetting order by DisplayOrder, Name" />

<div class="form-group">
	<asp:Label runat="server" ID="ValueLabel" AssociatedControlID="Value" />
	<asp:DropDownList runat="server"
		ID="Value"
		CssClass="form-control"
		DataValueField="Name"
		DataTextField="Description"
		OnDataBound="Value_DataBound" />
</div>
