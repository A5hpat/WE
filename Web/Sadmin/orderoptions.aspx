<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.OrderOptions"
	MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="orderoptions.aspx.cs" %>

<%@ Import Namespace="AspDotNetStorefrontCore" %>

<%@ Register TagPrefix="aspdnsf" Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls.Listing" %>
<%@ Register TagPrefix="aspdnsf" TagName="StringFilter" Src="Controls/Listing/StringFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="BooleanFilter" Src="Controls/Listing/BooleanFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="LinkGroupProducts" Src="Controls/LinkGroupProducts.ascx" %>

<asp:Content ContentPlaceHolderID="head" runat="server">
	<script type="text/javascript">
		var deleteConfirmPrompt = "<asp:Literal runat="server" Text="<%$Tokens:EscapedStringResource, admin.orderoptions.ConfirmDelete, javascript %>" />";
		jQuery(document).ready(function() {
			$('.confirm-delete')
				.on('click', null, deleteConfirmPrompt, promptAndConfirm);
		});

		function promptAndConfirm(event) {
			if(!confirm(event.data)) {
				event.preventDefault();
				event.stopPropagation();
				return false;
			}
		};
	</script>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">

	<h1>
		<i class="fa fa-cube"></i>
		<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.Products %>" />
		<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.breadcrumbseparator %>" />
		<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.orderoptions %>" />
	</h1>

	<aspdnsf:AlertMessage ID="ctlAlertMessage" runat="server" />

	<aspdnsf:FilteredListing runat="server"
		ID="FilteredListing"
		SqlQuery="SELECT {0} OrderOption.OrderOptionID, dbo.GetMlValue(OrderOption.Name, @_locale) OptionName, OrderOption.Cost, OrderOption.Description, OrderOption.DisplayOrder, OrderOption.CreatedOn, dbo.GetMlValue(TaxClass.Name, @_locale) TaxName 
			FROM OrderOption OrderOption 
			LEFT JOIN TaxClass TaxClass ON OrderOption.TaxClassID = TaxClass.TaxClassID WHERE {1}"
		SortExpression="OrderOption.OrderOptionID"
		LocaleSelectionEnabled="true">
		<ActionBarTemplate>
			<aspdnsf:LinkGroupProducts runat="server" ID="LinkGroupProducts" SelectedLink="orderoptions.aspx" />
			<asp:HyperLink runat="server" CssClass="btn btn-action" Text="<%$Tokens:StringResource, admin.editorderoption.CreateOrderOption %>" NavigateUrl="orderoption.aspx" />
		</ActionBarTemplate>
		<Filters>
			<aspdnsf:StringFilter runat="server" Label="Name" FieldName="OrderOption.Name" />
			<aspdnsf:StringFilter runat="server" Label="Description" FieldName="Description" />
		</Filters>
		<ListingTemplate>
			<div class="white-ui-box">
				<asp:GridView CssClass="table" ID="gMain" runat="server"
					DataSourceID="FilteredListingDataSource"
					PagerSettings-Position="TopAndBottom"
					AutoGenerateColumns="False"
					BorderStyle="None"
					BorderWidth="0px"
					CellPadding="0"
					GridLines="None"
					CellSpacing="-1"
					ShowFooter="True"
					OnRowCommand="DispatchGridCommand">
					<EmptyDataTemplate>
						<div class="alert alert-info">
							<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.EmptyDataTemplate.NoResults %>" />
						</div>
					</EmptyDataTemplate>
					<Columns>
						<asp:BoundField
							HeaderText="ID"
							ItemStyle-Width="5%"
							DataField="OrderOptionID" />

						<asp:HyperLinkField
							HeaderText="Name"
							DataNavigateUrlFields="OrderOptionID"
							DataNavigateUrlFormatString="orderoption.aspx?optionid={0}"
							DataTextField="OptionName"
							Text="<%$Tokens:StringResource, admin.nolinktext %>" />

						<asp:BoundField
							HeaderText="Cost"
							DataField="Cost" />

						<asp:BoundField
							HeaderText="Tax Class"
							DataField="TaxName" />

						<asp:BoundField
							HeaderText="Created On"
							ItemStyle-Width="15%"
							DataField="CreatedOn" />

						<asp:TemplateField
							HeaderText="<%$ Tokens: StringResource, admin.common.Deleted %>"
							HeaderStyle-Width="8%">
							<ItemTemplate>
								<asp:LinkButton runat="server" 
									ID="btnDelete" 
									CssClass="confirm-delete fa-times delete-link" 
									ToolTip='<%# AppLogic.GetString("admin.orderoption.DeleteTooltip", AppLogic.GetCurrentCustomer().LocaleSetting) %>'
									Text="<%$ Tokens:StringResource, admin.Common.Delete %>"
									CommandName="<%# DeleteOrderOptionCommand %>"
									CommandArgument='<%# DataBinder.Eval(Container.DataItem, "OrderOptionId") %>' />
							</ItemTemplate>
						</asp:TemplateField>
					</Columns>
					<FooterStyle CssClass="gridFooter" />
					<RowStyle CssClass="gridRow" />
					<EditRowStyle CssClass="gridEdit2" />
					<PagerStyle CssClass="tablepagerGrid" />
					<HeaderStyle CssClass="gridHeader" />
					<AlternatingRowStyle CssClass="gridAlternatingRow" BorderWidth="0px" />
				</asp:GridView>
			</div>
		</ListingTemplate>
	</aspdnsf:FilteredListing>
</asp:Content>
