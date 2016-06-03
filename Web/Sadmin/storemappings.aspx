<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.StoreMappings" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="storemappings.aspx.cs" %>

<%@ Register TagPrefix="aspdnsf" Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls.Listing" %>
<%@ Register TagPrefix="filter" TagName="StringFilter" Src="Controls/Listing/StringFilter.ascx" %>
<%@ Register TagPrefix="filter" TagName="BooleanFilter" Src="Controls/Listing/BooleanFilter.ascx" %>
<%@ Register TagPrefix="filter" TagName="ListFilter" Src="Controls/Listing/ListFilter.ascx" %>
<%@ Register TagPrefix="filter" TagName="StoreFilter" Src="Controls/Listing/StoreFilter.ascx" %>

<%@ Import Namespace="AspDotNetStorefrontCore" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="System.Linq" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<h1>
		<i class="fa fa-building-o"></i>
		<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.menu.storemappings %>" />
	</h1>

	<aspdnsf:AlertMessage runat="server" ID="AlertMessage" />

	<div class="item-action-bar clearfix">
		<asp:Button runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
	</div>

	<aspdnsf:FilteredListing runat="server"
		ID="FilteredListing">
		<Filters>
			<filter:ListFilter runat="server"
				Label="Entity"
				HideUnspecifiedItem="true"
				SqlParameterNames="entityType">
				<Items>
					<asp:ListItem Value="Affiliate" Text="Affiliates" />
					<asp:ListItem Value="Category" Text="Categories" Selected="True" />
					<asp:ListItem Value="Promotion" Text="Promotions" />
					<asp:ListItem Value="GiftCard" Text="Gift Cards" />
					<asp:ListItem Value="Manufacturer" Text="Manufacturers" />
					<asp:ListItem Value="News" Text="News" />
					<asp:ListItem Value="OrderOption" Text="Cart Upsells" />
					<asp:ListItem Value="Product" Text="Products" />
					<asp:ListItem Value="Section" Text="Departments" />
					<asp:ListItem Value="ShippingMethod" Text="Shipping Methods" />
				</Items>
			</filter:ListFilter>
			<filter:StringFilter runat="server" 
				Label="Name"
				FieldName="EntityName" />

			<filter:StoreFilter runat="server"
				Label="Mapped to Store"
				SqlParameterNames="storeId" />
		</Filters>
		<ListingTemplate>

			<div class="white-ui-box">
				<asp:GridView runat="server"
					ID="ListingGrid"
					DataSourceID="FilteredListingDataSource"
					DataKeyNames="EntityID,EntityType"
					AutoGenerateColumns="false"
					CssClass="table"
					GridLines="None">
					<EmptyDataTemplate>
						<div class="alert alert-info">
							<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.EmptyDataTemplate.NoResults %>" />
						</div>
					</EmptyDataTemplate>
					<Columns>
						<asp:BoundField
							HeaderText="ID"
							DataField="EntityID" />

						<asp:BoundField
							HeaderText="Name"
							DataField="EntityName" />
					</Columns>
				</asp:GridView>
			</div>

		</ListingTemplate>
	</aspdnsf:FilteredListing>
</asp:Content>
