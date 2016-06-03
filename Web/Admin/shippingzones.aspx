<%@ Page Language="C#" Inherits="AspDotNetStorefrontAdmin.ShippingZones" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Theme="Admin_Default" Codebehind="shippingzones.aspx.cs" %>

<%@ Register TagPrefix="aspdnsf" Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls.Listing" %>
<%@ Register TagPrefix="filter" TagName="StringFilter" Src="Controls/Listing/StringFilter.ascx" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<aspdnsf:ReturnUrlTracker runat="server" ID="ReturnUrlTracker" DefaultReturnUrl="shipping.aspx" Blacklist="shippingzone.aspx" />
	<h1>
		<i class="fa fa-anchor"></i>
		<asp:Literal ID="litTitle" runat="server" Text="<%$Tokens:StringResource, admin.editshippingzone.ManageShippingZones %>" />
	</h1>
	<aspdnsf:AlertMessage runat="server" ID="ctlAlertMessage" />
	<aspdnsf:FilteredListing runat="server"
		ID="FilteredListing"
		SqlQuery="SELECT {0} ShippingZoneID, Name, ZipCodes FROM ShippingZone WITH (NOLOCK) where {1}"
		SortExpression="ShippingZoneID"
		LocaleSelectionEnabled="false">
		<ActionBarTemplate>
			<asp:HyperLink runat="server" CssClass="btn btn-default" NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>" Text="<%$Tokens:StringResource, admin.common.close %>" />
			<asp:HyperLink runat="server" CssClass="btn btn-action" Text="<%$Tokens:StringResource, admin.common.addnew %>" NavigateUrl="shippingzone.aspx" />
		</ActionBarTemplate>
		<Filters>
			<filter:StringFilter runat="server"
				Label="<%$Tokens:StringResource, admin.editshippingzone.ZipCodes %>"
				FieldName="ZipCodes" />
		</Filters>
		<ListingTemplate>
			<div class="white-ui-box">
				<asp:GridView CssClass="table" ID="ShippingZoneGrid" runat="server"
					DataSourceID="FilteredListingDataSource"
					AutoGenerateColumns="False"
					OnRowCommand="ShippingZoneGrid_RowCommand"
					OnRowDataBound="ShippingZoneGrid_RowDataBound"
					GridLines="None">
					<Columns>
						<asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.common.ID %>">
							<ItemTemplate>
								<asp:Literal runat="server" ID="ShippingZoneId" Text='<%# Eval("ShippingZoneID") %>' />
							</ItemTemplate>
							<ItemStyle Width="5%" />
						</asp:TemplateField>
						<asp:HyperLinkField HeaderText="<%$Tokens:StringResource, admin.editshippingzone.ZoneName %>" DataNavigateUrlFields="ShippingZoneId" DataNavigateUrlFormatString="shippingzone.aspx?shippingzoneid={0}" DataTextField="Name" Text="<%$Tokens:StringResource, admin.nolinktext %>" />
						<asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.editshippingzone.ZipCodes %>">
							<ItemTemplate>
								<asp:Label runat="server" Text='<%# Eval("ZipCodes") %>' />
							</ItemTemplate>
							<ItemStyle Width="75%" />
						</asp:TemplateField>
						<asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.common.Delete %>">
							<ItemTemplate>
								<asp:LinkButton ID="Delete" ToolTip="Delete" CssClass="delete-link" CommandName="DeleteItem" CommandArgument='<%# Eval("ShippingZoneID") %>' runat="Server">
										<i class="fa fa-times"></i> <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.Delete %>" />
								</asp:LinkButton>
							</ItemTemplate>
							<ItemStyle Width="5%" />
						</asp:TemplateField>
					</Columns>
				</asp:GridView>
			</div>
		</ListingTemplate>
	</aspdnsf:FilteredListing>
</asp:Content>