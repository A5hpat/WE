<%@ Page Language="C#" AutoEventWireup="true" Inherits="_PromotionGrid" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="promotions.aspx.cs" %>

<%@ Register TagPrefix="aspdnsf" Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls.Listing" %>
<%@ Register TagPrefix="aspdnsf" TagName="StringFilter" Src="Controls/Listing/StringFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="BooleanFilter" Src="Controls/Listing/BooleanFilter.ascx" %>
<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="head">
	<link type="text/css" href="css/promotions.css" rel="stylesheet" />
	<script type="text/javascript" src="Scripts/promotions.js"></script>
</asp:Content>

<asp:Content ID="Content2" runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<div class="admin-module">
		<h1>
			<i class="fa fa-barcode"></i>
			<asp:Literal ID="litHeader" runat="server" Text="<%$Tokens:StringResource, admin.PromotionEditor.cbTitle %>" />
		</h1>
		<aspdnsf:AlertMessage ID="ctlAlertMessage" runat="server" />

		<aspdnsf:FilteredListing runat="server"
			ID="FilteredListing"
			SqlQuery="select {0} Id, Name, Code, Priority, Active, AutoAssigned FROM Promotions WHERE {1}"
			SortExpression="Name">
			<ActionBarTemplate>
				<asp:HyperLink runat="server"
					CssClass="btn btn-action"
					NavigateUrl="promotion.aspx"
					Text="<%$Tokens:StringResource, admin.Promotions.CreatePromotion %>" />
			</ActionBarTemplate>
			<Filters>
				<aspdnsf:StringFilter runat="server"
					Label="<%$Tokens:StringResource, admin.PromotionEditor.NameHeaderText %>"
					FieldName="Name" />

				<aspdnsf:StringFilter runat="server"
					Label="<%$Tokens:StringResource, admin.PromotionEditor.CodeHeaderText %>"
					FieldName="Code" />

				<aspdnsf:BooleanFilter runat="server"
					Label="<%$Tokens:StringResource, admin.PromotionEditor.ActiveHeaderText %>"
					FieldName="Active" />

				<aspdnsf:BooleanFilter runat="server"
					Label="<%$Tokens:StringResource, admin.PromotionEditor.AutoAssignedHeaderText %>"
					FieldName="AutoAssigned" />

			</Filters>
			<ListingTemplate>
				<div id="PromoGrid" class="white-ui-box">
					<asp:GridView runat="server"
						ID="grdPromotions"
						DataSourceID="FilteredListingDataSource"
						CssClass="table table-detail"
						GridLines="None"
						AutoGenerateColumns="false"
						DataKeyNames="Id">
						<EmptyDataTemplate>
							<div class="alert alert-info">
								<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.EmptyDataTemplate.NoResults %>" />
							</div>
						</EmptyDataTemplate>
						<Columns>

							<asp:HyperLinkField
								HeaderText="<%$ Tokens: StringResource, admin.common.ID %>"
								HeaderStyle-Width="5%"
								SortExpression="Id"
								DataTextField="Id"
								DataNavigateUrlFields="Id"
								DataNavigateUrlFormatString="promotion.aspx?promotionid={0}" />

							<asp:HyperLinkField
								HeaderText="<%$Tokens:StringResource, admin.PromotionEditor.NameHeaderText %>"
								DataTextField="Name"
								DataNavigateUrlFields="Id"
								DataNavigateUrlFormatString="promotion.aspx?promotionid={0}"
								Text="<%$Tokens:StringResource, admin.nolinktext %>" />

							<asp:BoundField
								HeaderText="<%$Tokens:StringResource, admin.PromotionEditor.CodeHeaderText %>"
								DataField="Code" />

							<asp:BoundField
								HeaderText="<%$Tokens:StringResource, admin.PromotionEditor.PriorityHeaderText %>"
								DataField="Priority" />

							<asp:BoundField
								HeaderText="<%$Tokens:StringResource, admin.PromotionEditor.ActiveHeaderText %>"
								DataField="Active" />

							<asp:BoundField
								HeaderText="<%$Tokens:StringResource, admin.PromotionEditor.AutoAssignedHeaderText %>"
								DataField="AutoAssigned" />

							<asp:TemplateField 
								HeaderText="<%$ Tokens:StringResource, admin.Common.Delete %>"
								HeaderStyle-Width="10%">
								<ItemTemplate>
									<asp:LinkButton runat="server" 
										ID="btnDelete" 
										CommandArgument='<%# DataBinder.Eval(Container.DataItem, "Id") %>'
										CssClass="fa-times delete-link" 
										Text="<%$ Tokens:StringResource, admin.Common.Delete %>"
										OnClick="btnDelete_Click" />
								</ItemTemplate>
							</asp:TemplateField>

						</Columns>
					</asp:GridView>
				</div>
			</ListingTemplate>
		</aspdnsf:FilteredListing>
	</div>
</asp:Content>
