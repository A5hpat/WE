<%@ Control Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontControls.Listing.EntityFilter" Codebehind="EntityFilter.ascx.cs" %>

<div class="form-group">
	<asp:Label runat="server" ID="ValueLabel" AssociatedControlID="Value" />
	<asp:DropDownList ID="Value" runat="server" AutoPostBack="true">
		<asp:ListItem Value="" Text="Select an Entity" />
		<asp:ListItem Value="Affiliate" Text="Affiliates" />
		<asp:ListItem Value="Category" Text="Categories" />
		<asp:ListItem Value="Promotion" Text="Promotions" />
		<asp:ListItem Value="GiftCard" Text="GiftCards" />
		<asp:ListItem Value="Manufacturer" Text="Manufacturers" />
		<asp:ListItem Value="New" Text="News" />
		<asp:ListItem Value="OrderOption" Text="Cart Upsells" />
		<asp:ListItem Value="Product" Text="Products" />
		<asp:ListItem Value="Section" Text="Departments" />
		<asp:ListItem Value="ShippingMethod" Text="Shipping Methods" />
	</asp:DropDownList>
</div>
