<%@ Control Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontControls.Listing.StoreFilter" Codebehind="StoreFilter.ascx.cs" %>

<asp:SqlDataSource runat="server"
	ID="StoreDataSource"
	ConnectionString="<%$ ConnectionStrings:DBConn %>"
	SelectCommand = "select StoreID, Name from Store where Published = 1 and Deleted = 0" />

<div class="form-group">
	<asp:Label runat="server"
		ID="ValueLabel"
		AssociatedControlID="Value" />

	<asp:DropDownList runat="server"
		ID="Value"
		CssClass="form-control"
		DataSourceID="StoreDataSource"
		DataValueField = "StoreID"
		DataTextField = "Name"
		OnDataBound="Value_DataBound" />
</div>
