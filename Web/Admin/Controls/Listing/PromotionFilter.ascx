<%@ Control Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontControls.Listing.PromotionFilter" Codebehind="PromotionFilter.ascx.cs" %>

<asp:SqlDataSource runat="server"
	ID="ValueDataSource"
	ConnectionString="<%$ ConnectionStrings:DBConn %>"
	SelectCommand="select Id, Name from Promotions order by Name" />

<div class="form-group">
	<asp:Label runat="server" ID="ValueLabel" AssociatedControlID="Value" />
	<asp:DropDownList runat="server"
		ID="Value"
		CssClass="form-control"
		DataValueField="Id"
		DataTextField="Name"
		OnDataBound="Value_DataBound" />
</div>
