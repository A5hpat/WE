<%@ Page Language="c#" Inherits="AspDotNetStorefrontAdmin.ShippingMethodsrealTime" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="shippingmethodsrealtime.aspx.cs" %>

<%@ Register TagPrefix="aspdnsf" Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls.Listing" %>
<%@ Register TagPrefix="aspdnsf" TagName="StringFilter" Src="Controls/Listing/StringFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="BooleanFilter" Src="Controls/Listing/BooleanFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="DataQueryFilter" Src="Controls/Listing/DataQueryFilter.ascx" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<aspdnsf:ReturnUrlTracker runat="server" ID="ReturnUrlTracker" DefaultReturnUrl="shipping.aspx" Blacklist="shippingmethod.aspx" />
	<aspdnsf:AlertMessage ID="RealTimeAlert" runat="server" />
	<h1><i class="fa fa-anchor"></i>Realtime Shipping Methods</h1>
	<p>
		<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.realtimeshippingmethoddescription %>" />
	</p>
	<aspdnsf:FilteredListing runat="server"
		ID="FilteredListing"
		SqlQuery="select {0} nullif(dbo.GetMlValue(sm.Name, @_locale),'') Name, dbo.GetMlValue(DisplayName, @_locale) DisplayName, sm.ShippingMethodID, sm.DisplayOrder, ImageFileName from ShippingMethod sm WITH (NOLOCK) WHERE sm.IsRTShipping = 1 and {1}"
		SortExpression="DisplayOrder"
		LocaleSelectionEnabled="true">
		<ActionBarTemplate>
			<asp:HyperLink runat="server"
				CssClass="btn btn-default"
				NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>"
				Text="<%$Tokens:StringResource, admin.common.close %>" />
		</ActionBarTemplate>
		<ListingTemplate>
			<div class="white-ui-box">
				<asp:GridView CssClass="table" ID="RealTimeGrid" runat="server"
					DataSourceID="FilteredListingDataSource"
					AutoGenerateColumns="False"
					OnRowCommand="ShippingMethodGrid_RowCommand"
					GridLines="None">
					<EmptyDataTemplate>
						<div class="alert alert-info">
							<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.EmptyDataTemplate.NoShippingMethodsRealtime %>" />
						</div>
					</EmptyDataTemplate>
					<Columns>
						<asp:TemplateField HeaderText="ID">
							<ItemTemplate>
								<asp:Literal runat="server" ID="ShippingMethodId" Text='<%# Eval("ShippingMethodID") %>' />
							</ItemTemplate>
							<ItemStyle Width="5%" />
						</asp:TemplateField>

						<asp:ImageField 
							HeaderText="Icon"
							DataImageUrlFormatString="../images/shipping/{0}" 
							DataImageUrlField="ImageFileName" 
							NullDisplayText="<%$ Tokens:StringResource, admin.common.None %>" 
							ControlStyle-CssClass="shipping-icon"
							ItemStyle-Width="5%"  />

						<asp:HyperLinkField HeaderText="Method" DataNavigateUrlFields="ShippingMethodID" DataNavigateUrlFormatString="shippingmethod.aspx?shippingmethodid={0}" DataTextField="Name" Text="<%$Tokens:StringResource, admin.nolinktext %>" />

						<asp:TemplateField HeaderText="DisplayName">
							<ItemTemplate>
								<asp:Literal runat="Server" ID="DisplayName" Text='<%# Eval("DisplayName") %>' />
							</ItemTemplate>
						</asp:TemplateField>

						<asp:TemplateField HeaderText="Delete">
							<ItemTemplate>
								<asp:LinkButton ID="Delete" ToolTip="Delete" CssClass="delete-link fa-times" CommandName="DeleteItem" CommandArgument='<%# Eval("ShippingMethodID") %>' runat="Server" Text="<%$Tokens:StringResource, admin.common.Delete %>" />
							</ItemTemplate>
							<ItemStyle Width="5%" />
						</asp:TemplateField>

						<asp:TemplateField HeaderText="Edit">
							<ItemTemplate>
								<asp:HyperLink ToolTip="Edit" CssClass="edit-link fa-share" runat="Server" Text="<%$Tokens:StringResource, admin.common.Edit %>" NavigateUrl='<%# Eval("ShippingMethodID", "shippingmethod.aspx?shippingmethodid={0}") %>' />
							</ItemTemplate>
							<ItemStyle Width="5%" />
						</asp:TemplateField>

					</Columns>
				</asp:GridView>
			</div>
		</ListingTemplate>
	</aspdnsf:FilteredListing>
</asp:Content>
