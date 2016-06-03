<%@ Page Language="c#" Inherits="AspDotNetStorefrontAdmin.zipcodes" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="zipcodes.aspx.cs" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<h1>
		<i class="fa fa-map-marker"></i>
		<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.title.zipcodes %>" />
	</h1>
	<aspdnsf:AlertMessage runat="server" ID="ctrlAlertMessage" />
	<asp:Literal ID="litZipCodesInstructions" runat="server" Text="<%$Tokens:StringResource, admin.zipcodes.instructions %>" />
	<asp:Panel runat="server" ID="pnlGrid">
		<div class="list-action-bar">
			<asp:Button runat="server" ID="btnAdd" CssClass="btn btn-action" PostBackUrl="zipcode.aspx" Text="<%$Tokens:StringResource, admin.common.AddNew %>" />
		</div>
		<div class="white-ui-box">
			<asp:GridView CssClass="table" ID="gMain" runat="server"
				PagerSettings-Position="TopAndBottom"
				AutoGenerateColumns="False"
				AllowPaging="True"
				PageSize="999999"
				AllowSorting="True"
				OnRowCommand="gMain_RowCommand"
				OnRowDataBound="gMain_RowDataBound"
				OnSorting="gMain_Sorting"
				OnPageIndexChanging="gMain_PageIndexChanging"
				BorderStyle="None"
				BorderWidth="0px"
				CellPadding="0"
				GridLines="None"
				CellSpacing="-1"
				ShowFooter="True">
				<Columns>
					<asp:TemplateField HeaderText="Zip Code" SortExpression="ZipCode">
						<ItemTemplate>
							<asp:HyperLink runat="server" NavigateUrl='<%# Eval("ZipCode", "zipcode.aspx?zipcode={0}")%>'>
								<%# DataBinder.Eval(Container.DataItem, "ZipCode") %>
							</asp:HyperLink>
						</ItemTemplate>
					</asp:TemplateField>

					<asp:TemplateField HeaderText="Country" SortExpression="Name">
						<ItemTemplate>
							<%# Eval("Name") %>
							<asp:HiddenField ID="hdfCountryID" runat="server" Value='<%# Eval("CountryID") %>' />
						</ItemTemplate>
					</asp:TemplateField>

					<asp:TemplateField HeaderText="Delete">
						<ItemTemplate>
							<asp:LinkButton ID="Delete" ToolTip="Delete" CssClass="delete-link" CommandName="DeleteItem" CommandArgument='<%# Eval("ZipCode").ToString() + "|" + Eval("Name").ToString() %>' runat="Server">
								<i class="fa fa-times"></i> Delete
							</asp:LinkButton>
						</ItemTemplate>
						<ItemStyle Width="5%" />
					</asp:TemplateField>
					<asp:BoundField Visible="False" DataField="EditName" ReadOnly="True" />
				</Columns>
				<PagerSettings FirstPageText="<%$Tokens:StringResource, admin.countries.FirstPage %>" LastPageText="<%$Tokens:StringResource, admin.countries.LastPage %>"
					Mode="NumericFirstLast" PageButtonCount="15" Position="TopAndBottom" />
			</asp:GridView>
		</div>
		<div class="list-action-bar">
			<asp:Button runat="server" ID="btnAddBottom" CssClass="btn btn-action" PostBackUrl="zipcode.aspx" Text="<%$Tokens:StringResource, admin.common.AddNew %>" />
		</div>
	</asp:Panel>
</asp:Content>