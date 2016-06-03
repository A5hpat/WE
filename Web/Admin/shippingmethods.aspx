<%@ Page Language="c#" Inherits="AspDotNetStorefrontAdmin.ShippingMethods" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="shippingmethods.aspx.cs" %>

<%@ Register TagPrefix="aspdnsf" Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls.Listing" %>
<%@ Register TagPrefix="aspdnsf" TagName="StringFilter" Src="Controls/Listing/StringFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="BooleanFilter" Src="Controls/Listing/BooleanFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="DataQueryFilter" Src="Controls/Listing/DataQueryFilter.ascx" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<aspdnsf:ReturnUrlTracker runat="server" ID="ReturnUrlTracker" DefaultReturnUrl="shipping.aspx" Blacklist="shippingmethod.aspx" />
	<h1><i class="fa fa-anchor"></i>Shipping Methods</h1>
	<p>
		<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.shippingmethoddescription %>" />
	</p>
	<aspdnsf:AlertMessage ID="AlertMessage" runat="server" />
	<aspdnsf:FilteredListing runat="server"
		ID="FilteredListing"
		SqlQuery="select {0} nullif(dbo.GetMlValue(sm.Name, @_locale),'') Name, sm.ShippingMethodID, sm.DisplayOrder, sm.ImageFileName from ShippingMethod sm WITH (NOLOCK) WHERE sm.IsRTShipping = 0 and {1}"
		SortExpression="DisplayOrder"
		LocaleSelectionEnabled="true">
		<ActionBarTemplate>
			<asp:HyperLink runat="server" CssClass="btn btn-default" NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>" Text="<%$Tokens:StringResource, admin.common.close %>" />
			<asp:HyperLink runat="server" CssClass="btn btn-action" Text="<%$Tokens:StringResource, admin.common.addnew %>" NavigateUrl="shippingmethod.aspx" />
			<asp:Button runat="server" ID="btnSubmit" CssClass="btn btn-primary" Text="<%$Tokens:StringResource, admin.common.save %>" OnClick="btnUpdate_Click" />
		</ActionBarTemplate>
		<ListingTemplate>
			<div class="white-ui-box">
				<asp:GridView CssClass="table" ID="ShippingMethodGrid" runat="server"
					DataSourceID="FilteredListingDataSource"
					AutoGenerateColumns="False"
					OnRowCommand="ShippingMethodGrid_RowCommand"
					OnRowDataBound="ShippingMethodGrid_RowDataBound"
					GridLines="None">
					<EmptyDataTemplate>
						<div class="alert alert-info">
							<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.EmptyDataTemplate.NoShippingMethodsFound %>" />
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

						<asp:HyperLinkField
							HeaderText="Method"
							DataNavigateUrlFields="ShippingMethodID"
							DataNavigateUrlFormatString="shippingmethod.aspx?shippingmethodid={0}"
							DataTextField="Name"
							Text="<%$Tokens:StringResource, admin.nolinktext %>" />

						<asp:TemplateField HeaderText="Display Order" HeaderStyle-Width="95px">
							<ItemTemplate>
								<asp:TextBox ID="DisplayOrder" runat="Server" CssClass="text-2" Text='<%# Eval("DisplayOrder") %>' ValidationGroup="DisplayOrder"></asp:TextBox>
								<asp:RequiredFieldValidator runat="server" ControlToValidate="DisplayOrder" ErrorMessage="*Required" ValidationGroup="DisplayOrder" Display="Dynamic" CssClass="text-danger" />
							</ItemTemplate>
							<ItemStyle Width="10%" />
							<HeaderStyle HorizontalAlign="Center" />
						</asp:TemplateField>

						<asp:TemplateField HeaderText="Delete">
							<ItemTemplate>
								<asp:LinkButton ID="Delete" ToolTip="Delete" CssClass="delete-link" CommandName="DeleteItem" CommandArgument='<%# Eval("ShippingMethodID") %>' runat="Server">
										<i class="fa fa-times"></i> <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.Delete %>" />
								</asp:LinkButton>
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
