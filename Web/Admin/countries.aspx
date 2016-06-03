<%@ Page Language="c#" Inherits="AspDotNetStorefrontAdmin.countries" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="countries.aspx.cs" %>

<%@ Register TagPrefix="aspdnsf" Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls.Listing" %>
<%@ Register TagPrefix="aspdnsf" TagName="StringFilter" Src="Controls/Listing/StringFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="BooleanFilter" Src="Controls/Listing/BooleanFilter.ascx" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<aspdnsf:AlertMessage ID="AlertMessage" runat="server" />

	<h1>
		<i class="fa fa-globe"></i>
		<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.title.countries %>" />
	</h1>

	<aspdnsf:FilteredListing runat="server"
		SqlQuery="select {0} * from Country with(nolock) where {1}"
		SortExpression="country.Name">
		<ActionBarTemplate>
			<asp:HyperLink runat="server" CssClass="btn btn-action" Text="<%$Tokens:StringResource, admin.common.addnew %>" NavigateUrl="country.aspx" />
			<asp:Button runat="server" CssClass="btn btn-primary" Text="<%$Tokens:StringResource, admin.common.update %>" OnClick="btnUpdate_Click" />
		</ActionBarTemplate>
		<Filters>
			<aspdnsf:StringFilter runat="server" Label="Country" FieldName="country.Name" />
			<aspdnsf:StringFilter runat="server" Label="3 Letter Code" FieldName="country.ThreeLetterISOCode" />
			<aspdnsf:BooleanFilter runat="server" Label="Published" FieldName="country.Published" DefaultValue="true" />
		</Filters>
		<ListingTemplate>

			<div class="white-ui-box">
				<asp:GridView runat="server"
					ID="CountryGrid" 
					CssClass="table table-detail js-sortable-gridview" 
					DataSourceID="FilteredListingDataSource"
					DataKeyNames="CountryID"
					AutoGenerateColumns="False"
					GridLines="None"
					AllowSorting="true"
					OnRowCommand="HandleCommands">
					
					<Columns>

						<asp:BoundField
							HeaderText="ID"
							HeaderStyle-Width="5%"
							SortExpression="CountryID"
							DataField="CountryID" />

						<asp:HyperLinkField
							HeaderText="Country"
							SortExpression="Name"
							DataNavigateUrlFields="CountryID"
							DataNavigateUrlFormatString="country.aspx?countryid={0}"
							DataTextField="Name" />

						<asp:BoundField
							HeaderText="<%$Tokens:StringResource, admin.countries.TwoLetterISOCode %>"
							SortExpression="TwoLetterISOCode"
							DataField="TwoLetterISOCode" />

						<asp:BoundField
							HeaderText="<%$Tokens:StringResource, admin.countries.ThreeLetterISOCode %>"
							SortExpression="ThreeLetterISOCode"
							DataField="ThreeLetterISOCode" />

						<asp:BoundField
							HeaderText="<%$Tokens:StringResource, admin.countries.NumericISOCode %>"
							SortExpression="NumericISOCode"
							DataField="NumericISOCode" />

						<asp:TemplateField
							HeaderText="<%$Tokens:StringResource, admin.countries.Published %>">
							<ItemTemplate>
								<asp:CheckBox runat="server"
									ID="cbPublished"
									Checked='<%# (byte)Eval("Published") == 1 %>' />
							</ItemTemplate>
						</asp:TemplateField>

						<asp:TemplateField
							HeaderText="<%$Tokens:StringResource, admin.countries.PostalCodeRequired %>">
							<ItemTemplate>
								<asp:CheckBox runat="server"
									ID="cbPostalCodeRequired"
									Checked='<%# (byte)Eval("PostalCodeRequired") == 1 %>' />
							</ItemTemplate>
						</asp:TemplateField>

						<asp:BoundField
							HeaderText="<%$Tokens:StringResource, admin.countries.PostalCodeRegex %>"
							DataField="PostalCodeRegex" />

						<asp:BoundField
							HeaderText="<%$Tokens:StringResource, admin.countries.PostalCodeExample %>"
							DataField="PostalCodeExample" />

						<asp:TemplateField
							SortExpression="DisplayOrder">
							<HeaderTemplate>
								<span class="text-danger">*</span>
								<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.DisplayOrder %>" />
							</HeaderTemplate>
							<ItemTemplate>
								<asp:TextBox runat="Server"
									ID="DisplayOrder"
									CssClass="form-control"
									Text='<%# DataBinder.Eval(Container.DataItem, "DisplayOrder") %>'
									ValidationGroup="DisplayOrder" />
								<asp:RequiredFieldValidator runat="server"
									ControlToValidate="DisplayOrder"
									ErrorMessage="*Required"
									ValidationGroup="DisplayOrder"
									Display="Dynamic"
									CssClass="text-danger" />
							</ItemTemplate>
						</asp:TemplateField>

						<asp:TemplateField
							HeaderText="Delete"
							HeaderStyle-Width="5%">
							<ItemTemplate>
								<asp:LinkButton runat="Server"
									CssClass="delete-link fa-times js-confirm-prompt"
									data-confirmation-prompt='<%# Eval("Name", "Are you sure you want to delete the country {0}?") %>'
									ToolTip="Delete"
									CommandName="<%# DeleteCountryCommand %>"
									CommandArgument='<%# Eval("CountryID") %>'
									Text="<%$Tokens:StringResource, admin.common.Delete %>" />
							</ItemTemplate>
						</asp:TemplateField>

					</Columns>

					<EmptyDataTemplate>
						<div class="alert alert-info">
							<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.EmptyDataTemplate.NoResults %>" />
						</div>
					</EmptyDataTemplate>

				</asp:GridView>
			</div>

		</ListingTemplate>
	</aspdnsf:FilteredListing>
</asp:Content>
