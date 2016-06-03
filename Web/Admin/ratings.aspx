<%@ Page Language="c#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.ratings" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="ratings.aspx.cs" %>

<%@ Register TagPrefix="aspdnsf" Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls.Listing" %>
<%@ Register TagPrefix="aspdnsf" TagName="StringFilter" Src="Controls/Listing/StringFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="BooleanFilter" Src="Controls/Listing/BooleanFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="IntegerFilter" Src="Controls/Listing/IntegerFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="DataQueryFilter" Src="Controls/Listing/DataQueryFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="LinkGroupRatings" Src="Controls/LinkGroupRatings.ascx" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<h1>
		<i class="fa fa-thumbs-up"></i>
		<asp:Literal ID="litManageRatingsTitle" runat="server" Text="<%$Tokens:StringResource, admin.menu.Ratings %>" />
	</h1>
	<aspdnsf:AlertMessage ID="AlertMessage" runat="server" />

	<div class="list-action-bar">
		<aspdnsf:LinkGroupRatings runat="server" SelectedLink="ratings.aspx" />
	</div>
	<aspdnsf:FilteredListing runat="server"
		ID="FilteredListing"
		SqlQuery="select {0} * from Rating with (nolock) where HasComment = 1 and {1}"
		SortExpression="CreatedOn"
		SortDirection="Descending">
		<Filters>
			<aspdnsf:StringFilter runat="server" Label="Comments" FieldName="Comments" />
			<aspdnsf:IntegerFilter runat="server" Label="For Product ID" FieldName="ProductID" />
			<aspdnsf:DataQueryFilter runat="server"
				Label="<%$Tokens:StringResource, admin.order.ForStore %>"
				FieldName="StoreId"
				DataQuery="select StoreId, Name from Store"
				DataTextField="Name"
				DataValueField="StoreId" />
			<aspdnsf:BooleanFilter runat="server"
				Label="<%$ Tokens:StringResource, admin.manageratings.HasBadWords %>"
				FieldName="IsFilthy" />
		</Filters>
		<ListingTemplate>
			<div class="white-ui-box">
				<asp:GridView CssClass="table" ID="gMain" runat="server"
					DataSourceID="FilteredListingDataSource"
					PagerSettings-Position="TopAndBottom"
					AutoGenerateColumns="False"
					OnRowCommand="gMain_RowCommand"
					BorderStyle="None"
					BorderWidth="0px"
					CellPadding="0"
					GridLines="None"
					CellSpacing="-1"
					ShowFooter="True">
					<EmptyDataTemplate>
						<div class="alert alert-info">
							<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.EmptyDataTemplate.NoResults %>" />
						</div>
					</EmptyDataTemplate>
					<Columns>
						<asp:BoundField
							HeaderText="ID"
							ItemStyle-Width="5%"
							DataField="RatingID" />

						<asp:HyperLinkField
							HeaderText="<%$ Tokens:StringResource, admin.manageratings.Comments %>"
							DataNavigateUrlFields="RatingID"
							DataNavigateUrlFormatString="rating.aspx?ratingid={0}"
							DataTextField="Comments"
							Text="<%$Tokens:StringResource, admin.nolinktext %>" />

						<asp:BoundField
							HeaderText="<%$ Tokens:StringResource, admin.manageratings.ProductID %>"
							DataField="ProductID" />

						<asp:BoundField
							HeaderText="<%$ Tokens:StringResource, admin.manageratings.Rating %>"
							DataField="Rating" />

						<asp:TemplateField HeaderText="<%$ Tokens:StringResource, admin.manageratings.Date %>">
							<ItemTemplate>
								<asp:Literal runat="server" Text='<%# ((DateTime)Eval("CreatedOn")).ToShortDateString() %>' />
							</ItemTemplate>
						</asp:TemplateField>

						<asp:BoundField
							HeaderText="<%$ Tokens:StringResource, admin.manageratings.MarkedHelpful %>"
							DataField="FoundHelpful" />

						<asp:BoundField
							HeaderText="<%$ Tokens:StringResource, admin.manageratings.MarkedUnhelpful %>"
							DataField="FoundNotHelpful" />

						<asp:TemplateField HeaderText="<%$ Tokens:StringResource, admin.manageratings.HasBadWords %>">
							<ItemTemplate>
								<asp:CheckBox ID="chkHasBadWords" Enabled="false" runat="server" Checked='<%# Eval("IsFilthy").ToString() == "0" ? false : true %>' />
							</ItemTemplate>
						</asp:TemplateField>

						<asp:BoundField
							HeaderText="Store ID"
							DataField="StoreID" />

						<asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.common.Delete %>">
							<ItemTemplate>
								<asp:LinkButton ID="lnkDelete" CommandName="DeleteItem" CommandArgument='<%# Eval("RatingID") %>' runat="Server" Text='<i class="fa fa-times"></i> Delete' CssClass="delete-link" />
							</ItemTemplate>
							<ItemStyle CssClass="selectData" Width="5%" />
						</asp:TemplateField>
					</Columns>
				</asp:GridView>
			</div>
		</ListingTemplate>
	</aspdnsf:FilteredListing>
	<div class="list-action-bar">
		<aspdnsf:LinkGroupRatings runat="server" SelectedLink="ratings.aspx" />
	</div>
</asp:Content>
