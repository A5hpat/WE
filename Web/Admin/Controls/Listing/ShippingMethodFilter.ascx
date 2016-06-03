<%@ Control Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontControls.Listing.ShippingMethodFilter" Codebehind="ShippingMethodFilter.ascx.cs" %>

<asp:SqlDataSource runat="server"
	ID="ValueDataSource"
	ConnectionString="<%$ ConnectionStrings:DBConn %>" />

<div class="form-group">
	<asp:Label runat="server"
		ID="ValueLabel"
		AssociatedControlID="Value" />

	<asp:DropDownList runat="server"
		ID="Value"
		CssClass="form-control"
		DataSourceID="ValueDataSource"
		OnDataBound="Value_DataBound"
		DataValueField="Value"
		DataTextField="Display" />
</div> 
 