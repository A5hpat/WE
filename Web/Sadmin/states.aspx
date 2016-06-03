<%@ Page Language="c#" Inherits="AspDotNetStorefrontAdmin.states" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="states.aspx.cs" %>

<%@ Register TagPrefix="aspdnsf" Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls.Listing" %>
<%@ Register TagPrefix="aspdnsf" TagName="StringFilter" Src="Controls/Listing/StringFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="BooleanFilter" Src="Controls/Listing/BooleanFilter.ascx" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<aspdnsf:AlertMessage ID="AlertMessage" runat="server" />
	<h1><i class="fa fa-university"></i>States/Provinces</h1>
	<aspdnsf:FilteredListing runat="server"
		ID="FilteredListing"
		SortExpression="state.Name">
		<ActionBarTemplate>
			<asp:HyperLink runat="server" CssClass="btn btn-action" Text="<%$Tokens:StringResource, admin.common.addnew %>" NavigateUrl="state.aspx" />
			<asp:Button runat="server" CssClass="btn btn-primary" Text="<%$Tokens:StringResource, admin.common.save %>" OnClick="btnUpdate_Click" />
		</ActionBarTemplate>
		<Filters>
			<aspdnsf:StringFilter runat="server" Label="State" FieldName="state.Name" />
			<aspdnsf:StringFilter runat="server" Label="Abbreviation" FieldName="state.Abbreviation" />
			<aspdnsf:StringFilter runat="server" Label="Country" FieldName="country.Name" />
			<aspdnsf:BooleanFilter runat="server" Label="Published" FieldName="state.Published" DefaultValue="true" />
		</Filters>
		<ListingTemplate>
			<div class="white-ui-box">
				<asp:GridView CssClass="table" ID="gMain" runat="server"
					DataSourceID="FilteredListingDataSource"
					PagerSettings-Position="TopAndBottom"
					AutoGenerateColumns="False"
					OnRowCommand="gMain_RowCommand"
					OnRowDataBound="gMain_RowDataBound"
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
						<asp:TemplateField HeaderText="ID">
							<ItemTemplate>
								<asp:Literal runat="server" ID="StateID" Text='<%# DataBinder.Eval(Container.DataItem, "StateID") %>' />
							</ItemTemplate>
							<ItemStyle Width="5%" />
						</asp:TemplateField>

						<asp:TemplateField HeaderText="State/Province">
							<ItemTemplate>
								<asp:HyperLink runat="server" NavigateUrl='<%# Eval("StateID", "state.aspx?stateid={0}")%>'>
										<%# CreateLinkText(DataBinder.Eval(Container.DataItem, "Name")) %>
								</asp:HyperLink>
							</ItemTemplate>
						</asp:TemplateField>

						<asp:TemplateField HeaderText=" Abbrev.">
							<ItemTemplate>
								<%# DataBinder.Eval(Container.DataItem, "Abbreviation")%>
							</ItemTemplate>
							<ItemStyle />
						</asp:TemplateField>

						<asp:TemplateField HeaderText="Country">
							<ItemTemplate>
								<%# DataBinder.Eval(Container.DataItem, "Country")%>
							</ItemTemplate>
							<ItemStyle />
						</asp:TemplateField>

						<asp:TemplateField HeaderStyle-Width="95px">
							<HeaderTemplate>
								<span class="text-danger">*</span><asp:Literal runat="server" Text="Display Order" />
							</HeaderTemplate>
							<ItemTemplate>
								<asp:TextBox ID="DisplayOrder" runat="Server" CssClass="form-control" Text='<%# DataBinder.Eval(Container.DataItem, "DisplayOrder") %>' ValidationGroup="DisplayOrder"></asp:TextBox>
								<asp:RequiredFieldValidator runat="server" ControlToValidate="DisplayOrder" ErrorMessage="*Required" ValidationGroup="DisplayOrder" Display="Dynamic" CssClass="text-danger" />
							</ItemTemplate>
							<ItemStyle Width="10%" />
							<HeaderStyle HorizontalAlign="Center" />
						</asp:TemplateField>

						<asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.states.Published %>">
							<ItemTemplate>
								<asp:CheckBox ID="cbPublished" runat="server" Checked='<%# publishedCheck((object) Eval("Published")) %>' />
							</ItemTemplate>
							<ItemStyle Width="10%" />
						</asp:TemplateField>

						<asp:TemplateField HeaderText="Delete">
							<ItemTemplate>
								<asp:LinkButton ID="Delete" ToolTip="Delete" CssClass="delete-link" CommandName="DeleteItem" CommandArgument='<%# Eval("StateID") %>' runat="Server">
									<i class="fa fa-times"></i> <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.Delete %>" />
								</asp:LinkButton>
							</ItemTemplate>
							<ItemStyle Width="5%" />
						</asp:TemplateField>

						<asp:BoundField Visible="False" DataField="EditName" ReadOnly="True" />

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
