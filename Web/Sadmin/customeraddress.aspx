<%@ Page Language="C#" Inherits="AspDotNetStorefrontAdmin.CustomerAddress" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Theme="Admin_Default" Codebehind="customeraddress.aspx.cs" %>

<%@ Register TagPrefix="aspdnsf" Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">

	<aspdnsf:ReturnUrlTracker runat="server" ID="ReturnUrlTracker" DefaultReturnUrl="Customers.aspx" />
	<aspdnsf:AlertMessage runat="server" ID="AlertMessageDisplay" />

	<asp:SqlDataSource runat="server"
		ID="sqlAddressList"
		ConnectionString="<%$ ConnectionStrings:DBConn %>"
		ProviderName="System.Data.SqlClient"
		InsertCommand="aspdnsf_insAddress"
		InsertCommandType="StoredProcedure"
		SelectCommand="aspdnsf_getAddressesByCustomer"
		SelectCommandType="StoredProcedure"
		UpdateCommand="aspdnsf_updAddress"
		UpdateCommandType="StoredProcedure"
		DeleteCommand="aspdnsf_delAddressByID"
		DeleteCommandType="StoredProcedure">
		<SelectParameters>
			<asp:QueryStringParameter Name="CustomerID" QueryStringField="customerid" DbType="Int32" />
		</SelectParameters>
		<DeleteParameters>
			<asp:Parameter Name="AddressID" Type="Int32" />
		</DeleteParameters>
		<UpdateParameters>
			<asp:Parameter Name="AddressID" Type="Int32" />
			<asp:Parameter Name="NickName" Type="String" />
			<asp:Parameter Name="FirstName" Type="String" />
			<asp:Parameter Name="LastName" Type="String" />
			<asp:Parameter Name="Company" Type="String" />
			<asp:Parameter Name="Address1" Type="String" />
			<asp:Parameter Name="Address2" Type="String" />
			<asp:Parameter Name="Suite" Type="String" />
			<asp:Parameter Name="City" Type="String" />
			<asp:Parameter Name="Zip" Type="String" />
			<asp:Parameter Name="Phone" Type="String" />
			<asp:Parameter Name="Email" Type="String" />
		</UpdateParameters>
		<InsertParameters>
			<asp:QueryStringParameter Name="CustomerID" QueryStringField="customerid" DbType="Int32" />
			<asp:Parameter Name="NickName" Type="String" />
			<asp:Parameter Name="FirstName" Type="String" />
			<asp:Parameter Name="LastName" Type="String" />
			<asp:Parameter Name="Company" Type="String" />
			<asp:Parameter Name="Address1" Type="String" />
			<asp:Parameter Name="Address2" Type="String" />
			<asp:Parameter Name="Suite" Type="String" />
			<asp:Parameter Name="City" Type="String" />
			<asp:Parameter Name="Zip" Type="String" />
			<asp:Parameter Name="Phone" Type="String" />
			<asp:Parameter Name="Email" Type="String" />
		</InsertParameters>
	</asp:SqlDataSource>

	<div class="list-action-bar">
		<asp:HyperLink runat="server"
			CssClass="btn btn-default"
			NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>"
			Text="<%$Tokens:StringResource, admin.common.close %>" />
	</div>

	<div class="white-ui-box">
		<asp:DetailsView runat="server"
			ID="dtlAddressList"
			CssClass="table"
			GridLines="None"
			PagerStyle-CssClass="gridview-pager"
			AllowPaging="True"
			PagerSettings-Position="TopAndBottom"
			AutoGenerateRows="False"
			DataSourceID="sqlAddressList"
			DataKeyNames="AddressID"
			OnItemInserting="dtlAddressList_OnItemInserting"
			OnDataBound="dtlAddressList_OnDataBound"
			OnItemUpdating="dtlAddressList_OnItemUpdating"
			OnItemUpdated="dtlAddressList_ItemUpdated">
			<Fields>
				<asp:CommandField
					ShowDeleteButton="True"
					ShowEditButton="True"
					ShowInsertButton="True"
					ButtonType="Button"
					NewText="<%$ Tokens:StringResource, admin.common.AddNew %>"
					DeleteText="<%$ Tokens:StringResource, admin.common.Delete %>"
					EditText="<%$ Tokens:StringResource, admin.common.Edit %>"
					CancelText="<%$ Tokens:StringResource, admin.common.Cancel %>"
					InsertText="<%$ Tokens:StringResource, admin.common.Add %>"
					UpdateText="<%$ Tokens:StringResource, admin.common.Update %>"
					ControlStyle-CssClass="btn btn-default btn-sm" />

				<asp:TemplateField
					SortExpression="NickName"
					HeaderText="<%$ Tokens:StringResource, admin.editaddress.NickName %>">
					<EditItemTemplate>
						<asp:TextBox ID="txtNickName" runat="server" Text='<%# Bind("NickName") %>'></asp:TextBox>
					</EditItemTemplate>
					<InsertItemTemplate>
						<asp:TextBox ID="txtNickName" runat="server" Text='<%# Bind("NickName") %>'></asp:TextBox>
					</InsertItemTemplate>
					<ItemTemplate>
						<asp:Label ID="lblNickName" runat="server" Text='<%# Bind("NickName") %>'></asp:Label>
					</ItemTemplate>
				</asp:TemplateField>

				<asp:TemplateField SortExpression="FirstName">
					<HeaderTemplate>
						<span class="text-danger">*</span><asp:Literal ID="litFirstName" runat="server" Text='<%$ Tokens:StringResource, admin.editaddress.FirstName %>' />
					</HeaderTemplate>
					<EditItemTemplate>
						<asp:TextBox ID="txtFirstName" runat="server" Text='<%# Bind("FirstName") %>'></asp:TextBox>
						<asp:RequiredFieldValidator ID="vldFirstName" runat="server" CssClass="text-danger"
							ErrorMessage="<b>!!</b>" ControlToValidate="txtFirstName" />
					</EditItemTemplate>
					<InsertItemTemplate>
						<asp:TextBox ID="txtFirstName" runat="server" Text='<%# Bind("FirstName") %>'></asp:TextBox>
						<asp:RequiredFieldValidator ID="vldFirstName" runat="server" CssClass="text-danger"
							ErrorMessage="<b>!!</b>" ControlToValidate="txtFirstName" />
					</InsertItemTemplate>
					<ItemTemplate>
						<asp:Label ID="lblFirstName" runat="server" Text='<%# Bind("FirstName") %>'></asp:Label>
					</ItemTemplate>
				</asp:TemplateField>

				<asp:TemplateField SortExpression="LastName">
					<HeaderTemplate>
						<span class="text-danger">*</span><asp:Literal ID="litLastName" runat="server" Text='<%$ Tokens:StringResource, admin.editaddress.LastName %>' />
					</HeaderTemplate>
					<EditItemTemplate>
						<asp:TextBox ID="txtLastName" runat="server" Text='<%# Bind("LastName") %>'></asp:TextBox>
						<asp:RequiredFieldValidator ID="vldLastName" runat="server" CssClass="text-danger" ErrorMessage="<b>!!</b>"
							ControlToValidate="txtLastName" />
					</EditItemTemplate>
					<InsertItemTemplate>
						<asp:TextBox ID="txtLastName" runat="server" Text='<%# Bind("LastName") %>'></asp:TextBox>
						<asp:RequiredFieldValidator ID="vldLastName" runat="server" CssClass="text-danger" ErrorMessage="<b>!!</b>"
							ControlToValidate="txtLastName" />
					</InsertItemTemplate>
					<ItemTemplate>
						<asp:Label ID="lblLastName" runat="server" Text='<%# Bind("LastName") %>'></asp:Label>
					</ItemTemplate>
				</asp:TemplateField>

				<asp:TemplateField SortExpression="Company" HeaderText="<%$ Tokens:StringResource, admin.editaddress.Company %>">
					<EditItemTemplate>
						<asp:TextBox ID="txtCompany" runat="server" Text='<%# Bind("Company") %>'></asp:TextBox>
					</EditItemTemplate>
					<InsertItemTemplate>
						<asp:TextBox ID="txtCompany" runat="server" Text='<%# Bind("Company") %>'></asp:TextBox>
					</InsertItemTemplate>
					<ItemTemplate>
						<asp:Label ID="lblCompany" runat="server" Text='<%# Bind("Company") %>'></asp:Label>
					</ItemTemplate>
				</asp:TemplateField>

				<asp:TemplateField SortExpression="Address1">
					<HeaderTemplate>
						<span class="text-danger">*</span><asp:Literal ID="litAddress1" runat="server" Text='<%$ Tokens:StringResource, admin.editaddress.Address1 %>' />
					</HeaderTemplate>
					<EditItemTemplate>
						<asp:TextBox ID="txtAddress1" runat="server" Text='<%# Bind("Address1") %>'></asp:TextBox>
						<asp:RequiredFieldValidator ID="vldAddress1" runat="server" CssClass="text-danger" ErrorMessage="<b>!!</b>"
							ControlToValidate="txtAddress1" />
					</EditItemTemplate>
					<InsertItemTemplate>
						<asp:TextBox ID="txtAddress1" runat="server" Text='<%# Bind("Address1") %>'></asp:TextBox>
						<asp:RequiredFieldValidator ID="vldAddress1" runat="server" CssClass="text-danger" ErrorMessage="<b>!!</b>"
							ControlToValidate="txtAddress1" />
					</InsertItemTemplate>
					<ItemTemplate>
						<asp:Label id="lblAddress1" runat="server" Text='<%# Bind("Address1") %>'></asp:Label>
					</ItemTemplate>
				</asp:TemplateField>

				<asp:TemplateField SortExpression="Address2" HeaderText="<%$ Tokens:StringResource, admin.editaddress.Address2 %>">
					<EditItemTemplate>
						<asp:TextBox ID="txtAddress2" runat="server" Text='<%# Bind("Address2") %>'></asp:TextBox>
					</EditItemTemplate>
					<InsertItemTemplate>
						<asp:TextBox ID="txtAddress2" runat="server" Text='<%# Bind("Address2") %>'></asp:TextBox>
					</InsertItemTemplate>
					<ItemTemplate>
						<asp:Label id="lblAddress2" runat="server" Text='<%# Bind("Address2") %>'></asp:Label>
					</ItemTemplate>
				</asp:TemplateField>

				<asp:TemplateField SortExpression="Suite" HeaderText="<%$ Tokens:StringResource, admin.editaddress.Suite %>">
					<EditItemTemplate>
						<asp:TextBox ID="txtSuite" runat="server" Text='<%# Bind("Suite") %>'></asp:TextBox>
					</EditItemTemplate>
					<InsertItemTemplate>
						<asp:TextBox ID="txtSuite" runat="server" Text='<%# Bind("Suite") %>'></asp:TextBox>
					</InsertItemTemplate>
					<ItemTemplate>
						<asp:Label ID="lblSuite" runat="server" Text='<%# Bind("Suite") %>'></asp:Label>
					</ItemTemplate>
				</asp:TemplateField>

				<asp:TemplateField SortExpression="City">
					<HeaderTemplate>
						<span class="text-danger">*</span><asp:Literal ID="litCity" runat="server" Text='<%$ Tokens:StringResource, admin.editaddress.City %>' />
					</HeaderTemplate>
					<EditItemTemplate>
						<asp:TextBox ID="txtCity" runat="server" Text='<%# Bind("City") %>'></asp:TextBox>
						<asp:RequiredFieldValidator ID="vldCity" runat="server" CssClass="text-danger" ErrorMessage="<b>!!</b>"
							ControlToValidate="txtCity" />
					</EditItemTemplate>
					<InsertItemTemplate>
						<asp:TextBox ID="txtCity" runat="server" Text='<%# Bind("City") %>'></asp:TextBox>
						<asp:RequiredFieldValidator ID="vldCity" runat="server" CssClass="text-danger" ErrorMessage="<b>!!</b>"
							ControlToValidate="txtCity" />
					</InsertItemTemplate>
					<ItemTemplate>
						<asp:Label ID="lblCity" runat="server" Text='<%# Bind("City") %>'></asp:Label>
					</ItemTemplate>
				</asp:TemplateField>

				<asp:TemplateField>
					<HeaderTemplate>
						<span class="text-danger">*</span><asp:Literal ID="litState" runat="server" Text='<%$ Tokens:StringResource, admin.editaddress.State %>' />
					</HeaderTemplate>
					<ItemTemplate>
						<asp:Literal runat="server" ID="ltlState" Text='<%# Eval("State") %>' />
					</ItemTemplate>
					<EditItemTemplate>
						<asp:DropDownList runat="server" ID="ddlState" />
					</EditItemTemplate>
					<InsertItemTemplate>
						<asp:DropDownList runat="server" ID="ddlState" />
					</InsertItemTemplate>
				</asp:TemplateField>

				<asp:TemplateField SortExpression="Zip" HeaderText="<%$ Tokens:StringResource, admin.editaddress.Zip %>">
					<HeaderTemplate>
						<span class="text-danger">*</span><asp:Literal runat="server" Text='<%$ Tokens:StringResource, admin.editaddress.Zip %>' />
					</HeaderTemplate>
					<EditItemTemplate>
						<asp:TextBox ID="txtZip" runat="server" Text='<%# Bind("Zip") %>'></asp:TextBox>
						<asp:RequiredFieldValidator runat="server" CssClass="text-danger" ErrorMessage="<b>!!</b>" ControlToValidate="txtZip" />
					</EditItemTemplate>
					<InsertItemTemplate>
						<asp:TextBox ID="txtZip" runat="server" Text='<%# Bind("Zip") %>'></asp:TextBox>
						<asp:RequiredFieldValidator runat="server" CssClass="text-danger" ErrorMessage="<b>!!</b>" ControlToValidate="txtZip" />
					</InsertItemTemplate>
					<ItemTemplate>
						<asp:Label ID="lblZip" runat="server" Text='<%# Bind("Zip") %>'></asp:Label>
					</ItemTemplate>
				</asp:TemplateField>

				<asp:TemplateField>
					<HeaderTemplate>
						<span class="text-danger">*</span><asp:Literal ID="litCountry" runat="server" Text='<%$ Tokens:StringResource, admin.editaddress.Country %>' />
					</HeaderTemplate>
					<ItemTemplate>
						<asp:Literal runat="server" ID="ltlCountry" Text='<%# Eval("Country") %>' />
					</ItemTemplate>
					<EditItemTemplate>
						<asp:DropDownList runat="server" ID="ddlCountry" AutoPostBack="True" OnSelectedIndexChanged="ddlCountry_OnSelectedIndexChanged" />
					</EditItemTemplate>
					<InsertItemTemplate>
						<asp:DropDownList runat="server" ID="ddlCountry" AutoPostBack="True" OnSelectedIndexChanged="ddlCountry_OnSelectedIndexChanged" />
					</InsertItemTemplate>
				</asp:TemplateField>

				<asp:TemplateField SortExpression="ResidenceType">
					<HeaderTemplate>
						<span class="text-danger">*</span><asp:Literal ID="litResidenceType" runat="server" Text='<%$ Tokens:StringResource, admin.editaddress.ResidenceType %>' />
					</HeaderTemplate>
					<EditItemTemplate>
						<asp:DropDownList ID="ddlResidenceType" runat="server" />
					</EditItemTemplate>
					<InsertItemTemplate>
						<asp:DropDownList ID="ddlResidenceType" runat="server" />
					</InsertItemTemplate>
					<ItemTemplate>
						<asp:Label ID="lblResidenceType" runat="server"></asp:Label>
					</ItemTemplate>
				</asp:TemplateField>

				<asp:TemplateField SortExpression="Phone" HeaderText="<%$ Tokens:StringResource, admin.editaddress.Phone %>">
					<EditItemTemplate>
						<asp:TextBox ID="txtPhone" runat="server" Text='<%# Bind("Phone") %>'></asp:TextBox>
					</EditItemTemplate>
					<InsertItemTemplate>
						<asp:TextBox ID="txtPhone" runat="server" Text='<%# Bind("Phone") %>'></asp:TextBox>
					</InsertItemTemplate>
					<ItemTemplate>
						<asp:Label ID="lblPhone" runat="server" Text='<%# Bind("Phone") %>'></asp:Label>
					</ItemTemplate>
				</asp:TemplateField>

				<asp:TemplateField SortExpression="Email" HeaderText="<%$ Tokens:StringResource, admin.editaddress.Email %>">
					<EditItemTemplate>
						<asp:TextBox ID="txtEmail" runat="server" Text='<%# Bind("Email") %>'></asp:TextBox>
					</EditItemTemplate>
					<InsertItemTemplate>
						<asp:TextBox ID="txtEmail" runat="server" Text='<%# Bind("Email") %>'></asp:TextBox>
					</InsertItemTemplate>
					<ItemTemplate>
						<asp:Label ID="lblEmail" runat="server" Text='<%# Bind("Email") %>'></asp:Label>
					</ItemTemplate>
				</asp:TemplateField>

				<asp:CommandField
					ShowDeleteButton="True"
					ShowEditButton="True"
					ShowInsertButton="True"
					ButtonType="Button"
					NewText="<%$ Tokens:StringResource, admin.common.AddNew %>"
					DeleteText="<%$ Tokens:StringResource, admin.common.Delete %>"
					EditText="<%$ Tokens:StringResource, admin.common.Edit %>"
					CancelText="<%$ Tokens:StringResource, admin.common.Cancel %>"
					InsertText="<%$ Tokens:StringResource, admin.common.Add %>"
					UpdateText="<%$ Tokens:StringResource, admin.common.Update %>"
					ControlStyle-CssClass="btn btn-default btn-sm" />
			</Fields>
		</asp:DetailsView>

		<asp:Button runat="server"
			ID="btnMakeBilling"
			CssClass="btn btn-default btn-sm"
			Text="<%$ Tokens:StringResource, admin.editaddress.MakePrimaryBilling %>"
			OnClick="btnMakeBilling_OnClick" />

		<asp:Button runat="server"
			ID="btnMakeShipping"
			CssClass="btn btn-default btn-sm"
			Text="<%$ Tokens:StringResource, admin.editaddress.MakePrimaryShipping %>"
			OnClick="btnMakeShipping_OnClick" />
	</div>

	<div class="list-action-bar">
		<asp:HyperLink runat="server"
			CssClass="btn btn-default"
			NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>"
			Text="<%$Tokens:StringResource, admin.common.close %>" />
	</div>
</asp:Content>
