<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.giftcards" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="giftcards.aspx.cs" %>

<%@ Register TagPrefix="aspdnsf" Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls.Listing" %>
<%@ Register TagPrefix="aspdnsf" TagName="StringFilter" Src="Controls/Listing/StringFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="MultiFieldStringFilter" Src="Controls/Listing/MultiFieldStringFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="ListFilter" Src="Controls/Listing/ListFilter.ascx" %>
<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<h1>
		<i class="fa fa-credit-card"></i>
		<asp:Literal ID="Literal2" runat="server" Text="<%$Tokens:StringResource, admin.giftcard.GiftCards %>" />
	</h1>

	<aspdnsf:AlertMessage runat="server" ID="AlertMessage" />

	<div class="admin-module">
		<aspdnsf:FilteredListing runat="server"
			ID="FilteredListing"
			SqlQuery="select {0} G.*, C.FirstName, C.LastName from GiftCard G with (NOLOCK) LEFT OUTER JOIN Customer C with (NOLOCK) ON G.PurchasedByCustomerID = C.CustomerID WHERE {1}"
			SortExpression="SerialNumber">
			<ActionBarTemplate>
				<asp:HyperLink runat="server"
					CssClass="btn btn-action"
					NavigateUrl="giftcard.aspx"
					Text="<%$Tokens:StringResource, admin.giftcard.CreateGiftCard %>" />
			</ActionBarTemplate>
			<Filters>
				<aspdnsf:StringFilter runat="server"
					Label="Serial Number"
					FieldName="SerialNumber" />

				<aspdnsf:MultiFieldStringFilter runat="server"
					Label="First or Last Name"
					Fields="C.FirstName, C.LastName" />

				<aspdnsf:ListFilter runat="server"
					Label="Type"
					FieldName="GiftCardTypeID">
					<Items>
						<asp:ListItem Value="100" Text="Physical" />
						<asp:ListItem Value="101" Text="Email" />
						<asp:ListItem Value="102" Text="Certificate" />
					</Items>
				</aspdnsf:ListFilter>
			</Filters>
			<ListingTemplate>
				<div id="OrderGrid" class="white-ui-box">
					<div class="white-box-heading">
						<asp:Literal ID="litGridHead" runat="server" Text="<%$Tokens:StringResource, admin.common.MatchingOrders %>" />
					</div>
					<asp:GridView
						OnRowDataBound="grdGiftCards_RowDataBound"
						OnRowCommand="grdGiftCards_RowCommand"
						runat="server"
						ID="grdGiftCards"
						DataSourceID="FilteredListingDataSource"
						CssClass="table table-detail"
						GridLines="None"
						AutoGenerateColumns="false"
						DataKeyNames="OrderNumber">
						<EmptyDataTemplate>
							<div class="alert alert-info">
								<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.EmptyDataTemplate.NoResults %>" />
							</div>
						</EmptyDataTemplate>
						<Columns>
							<asp:HyperLinkField
								HeaderText="<%$Tokens:StringResource, admin.common.ID %>"
								SortExpression="GiftCardID"
								DataTextField="GiftCardID"
								DataNavigateUrlFields="GiftCardID"
								DataNavigateUrlFormatString="giftcard.aspx?giftcardid={0}" />

							<asp:HyperLinkField
								HeaderText="<%$Tokens:StringResource, admin.giftcards.SerialHeader %>"
								DataTextField="SerialNumber"
								DataNavigateUrlFields="GiftCardID"
								DataNavigateUrlFormatString="giftcard.aspx?giftcardid={0}"
								Text="<%$Tokens:StringResource, admin.nolinktext %>" />

							<asp:TemplateField
								HeaderText="Type">
								<ItemTemplate>
									<asp:Literal runat="server" Text='<%# (AspDotNetStorefrontCore.GiftCardTypes)Eval("GiftCardTypeID") %>' />
								</ItemTemplate>
							</asp:TemplateField>

							<asp:BoundField
								HeaderText="<%$Tokens:StringResource, admin.giftcards.CreatedOn %>"
								DataField="CreatedOn"
								DataFormatString="{0:d}" />

							<asp:BoundField
								HeaderText="First name"
								DataField="FirstName" />

							<asp:BoundField
								HeaderText="Last name"
								DataField="LastName" />

							<asp:BoundField
								HeaderText="<%$Tokens:StringResource, admin.common.OrderNumber %>"
								DataField="OrderNumber" />

							<asp:TemplateField
								HeaderText="<%$Tokens:StringResource, admin.giftcards.InitialValue %>">
								<ItemTemplate>
									<%# ThisCustomer.CurrencyString((decimal)Eval("InitialAmount")) %>
								</ItemTemplate>
							</asp:TemplateField>

							<asp:TemplateField
								HeaderText="<%$Tokens:StringResource, admin.giftcards.RemainingAmount %>">
								<ItemTemplate>
									<%# ThisCustomer.CurrencyString((decimal)Eval("Balance")) %>
								</ItemTemplate>
							</asp:TemplateField>

							<asp:BoundField
								HeaderText="<%$Tokens:StringResource, admin.common.ExpiresOn %>"
								DataField="ExpirationDate"
								DataFormatString="{0:d}" />

							<asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.giftcards.UsageHistory %>">
								<ItemTemplate>
									<span style='white-space: nowrap;'>
										<a href='giftcardusage.aspx?giftcardid=<%# Eval("GiftCardID") %>'>
											<asp:Literal Text="<%$Tokens:StringResource, admin.giftcards.Usage %>" runat="server" /></a>
									</span>
								</ItemTemplate>
								<ItemStyle HorizontalAlign="Center" />
								<HeaderStyle HorizontalAlign="Center" />
							</asp:TemplateField>

							<asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.giftcards.Action %>">
								<ItemTemplate>
									<asp:LinkButton ID="lnkAction" CommandName="ItemAction" runat="Server"></asp:LinkButton>
								</ItemTemplate>
								<ItemStyle CssClass="selectData" HorizontalAlign="Center" />
								<HeaderStyle HorizontalAlign="Center" />
							</asp:TemplateField>
						</Columns>
					</asp:GridView>
				</div>
			</ListingTemplate>
		</aspdnsf:FilteredListing>
	</div>
</asp:Content>
