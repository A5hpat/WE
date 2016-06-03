<%@ Page Language="c#" Inherits="AspDotNetStorefrontAdmin.CustomerLevelCustomers" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Theme="Admin_Default" Codebehind="customerlevelcustomers.aspx.cs" %>

<%@ Register TagPrefix="listing" Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls.Listing" %>
<%@ Register TagPrefix="filter" TagName="IntegerFilter" Src="Controls/Listing/IntegerFilter.ascx" %>
<%@ Register TagPrefix="filter" TagName="StringFilter" Src="Controls/Listing/StringFilter.ascx" %>
<%@ Register TagPrefix="filter" TagName="MultiFieldStringFilter" Src="Controls/Listing/MultiFieldStringFilter.ascx" %>
<%@ Register TagPrefix="ajax" Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" %>
<%@ Register TagPrefix="telerik" Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" %>
<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<h1>
		<i class="fa fa-list-ol"></i>
		<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.title.CustomerLevelCustomers %>" /><asp:Label runat="server" ID="lblLevelNameHeader" />
	</h1>

	<aspdnsf:ReturnUrlTracker runat="server" ID="ReturnUrlTracker" DefaultReturnUrl="customerlevels.aspx" />

	<aspdnsf:AlertMessage ID="AlertMessageDisplay" runat="server" />

	<div class="item-action-bar">
		<div style="float: right;" >
			<asp:Literal runat="server" Text="Choose a Customer Level below to add customers to it." ID="litAddToLevelWarning" Visible="false" />
			<asp:Button runat="server" CssClass="btn btn-primary" OnClick="btnAddToLevel_Click" ID="btnAddTolevel" 
				Text="<%$Tokens:StringResource, admin.customerlevel.AddToLevel %>" ValidationGroup="valAddLevel" />
		</div>
		<div style="float: right;" runat="server" id="divAddToLevel">
			<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.customerlevel.EnterCustomerInfo %>" /><asp:Label runat="server" ID="lblLevelName" />:
			<asp:TextBox ID="txtNewEmail" runat="server" Width="200" ToolTip="<%$Tokens:StringResource,admin.editgiftcard.tooltip.imgCustomer %>" OnFocus="ClearCustomerEmailBackground()" />
			<asp:HiddenField ID="hdnCustomerId" runat="server" Value="" />
			<ajax:AutoCompleteExtender ID="autoCompleteCustomerId" runat="server"
				EnableCaching="true"
				MinimumPrefixLength="1"
				CompletionInterval="250"
				CompletionSetCount="15"
				TargetControlID="txtNewEmail"
				UseContextKey="True"
				ServiceMethod="GetCompletionList"
				OnClientItemSelected="AutoCompleteItemSelected"
				CompletionListCssClass="autoCompleteResults"
				FirstRowSelected="true" />
			<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$Tokens:StringResource, admin.editgiftcard.tooltip.imgCustomer %>">
				<i class="fa fa-question-circle fa-lg"></i>
			</asp:Label>
			<asp:RequiredFieldValidator ControlToValidate="txtNewEmail" 
				Display="Dynamic"
				ValidationGroup="valAddLevel" 
				ErrorMessage="<%$Tokens:StringResource, admin.customerlevel.IdOrEmailRequired %>" 
				CssClass="text-danger" 
				ID="reqCustEmail" 
				SetFocusOnError="true" 
				runat="server" />
		</div>

		<script type="text/javascript">// <![CDATA[
			function AutoCompleteItemSelected(sender, e) {
				var hdnCustomerId = document.getElementById('<%=hdnCustomerId.ClientID %>');
				hdnCustomerId.value = e.get_value();
			}
			
			function ClearCustomerEmailBackground(sender, e) {
				var txtNewEmail = document.getElementById('<%=txtNewEmail.ClientID %>');

				txtNewEmail.style.background = "#FFF";
				txtNewEmail.style.opacity = "1";
				txtNewEmail.value = "";
			}
			// ]]>
		</script>
	</div>

	<listing:FilteredListing runat="server"
		ID="FilteredListing"
		SqlQuery="
			select {0} CustomerID
		         , FirstName 
				 , LastName
		         , Email
		         , Customer.CustomerLevelID as LevelID
				 , dbo.GetMlValue(CustomerLevel.Name, @_locale) as LevelName
		      from dbo.Customer with(nolock) 
		inner join dbo.CustomerLevel 
				on dbo.Customer.CustomerLevelID = CustomerLevel.CustomerLevelID
		     where {1} 
		       and Customer.CustomerLevelID &gt; 0
			   and Customer.deleted = 0
			   and CustomerLevel.deleted = 0"
		SortExpression="Customer.Email"
		LocaleSelectionEnabled="true">
		<ActionBarTemplate>
			<asp:HyperLink runat="server"
				CssClass="btn btn-default"
				NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>"
				Text="<%$Tokens:StringResource, admin.common.close %>" />
		</ActionBarTemplate>
		<Filters>
			<filter:IntegerFilter runat="server"
				Label="<%$Tokens:StringResource, admin.customerlevels.CustomerLevelId %>"
				FieldName="Customer.CustomerLevelID" />

			<filter:StringFilter runat="server"
				Label="<%$Tokens:StringResource, admin.common.Email %>"
				FieldName="Email" />

			<filter:MultiFieldStringFilter runat="server"
				Label="<%$ Tokens:StringResource, admin.common.Name %>"
				Fields="FirstName,LastName" />
		</Filters>
		<ListingTemplate>
			<div class="white-ui-box">
				<asp:GridView runat="server"
					ID="grdCustomersInLevel"
					CssClass="table js-sortable-gridview"
					GridLines="None"
					OnRowCommand="grdCustomersInLevel_RowCommand"
					DataSourceID="FilteredListingDataSource"
					AllowSorting="true"
					AutoGenerateColumns="false"
					DataKeyNames="CustomerID">
					<EmptyDataTemplate>
						<div class="alert alert-info">
							<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.EmptyDataTemplate.NoResults %>" />
						</div>
					</EmptyDataTemplate>
					<Columns>

						<asp:BoundField
							DataField="CustomerID"
							HeaderText="<%$ Tokens: StringResource, admin.common.ID %>"
							HeaderStyle-Width="7%"
							SortExpression="Customer.CustomerID" />

						<asp:BoundField
							DataField="FirstName"
							HeaderText="<%$ Tokens: StringResource, admin.common.FirstName %>"
							SortExpression="Customer.FirstName" />

						<asp:BoundField
							DataField="LastName"
							HeaderText="<%$ Tokens: StringResource, admin.common.LastName %>"
							SortExpression="Customer.LastName" />

						<asp:BoundField
							DataField="Email"
							HeaderText="<%$Tokens:StringResource, admin.common.Email %>"
							SortExpression="Customer.Email" />

						<asp:BoundField
							DataField="LevelID"
							HeaderText="<%$Tokens:StringResource, admin.customerlevels.CustomerLevelID %>"
							SortExpression="Customer.CustomerLevelID" />

						<asp:BoundField
							DataField="LevelName"
							HeaderText="<%$Tokens:StringResource, admin.customerlevels.Name %>"
							SortExpression="CustomerLevel.Name" />

						<asp:TemplateField
							HeaderStyle-Width="10%">
							<ItemTemplate>
								<asp:LinkButton runat="Server"
									ToolTip="<%$Tokens:StringResource, admin.customerlevel.RemoveFromLevel %>"
									CssClass="delete-link fa-times"
									CommandName="ClearLevel"
									CommandArgument='<%# Eval("CustomerID") %>'
									Text="<%$Tokens:StringResource, admin.customerlevel.RemoveFromLevel %>" />
							</ItemTemplate>
						</asp:TemplateField>

					</Columns>
				</asp:GridView>
			</div>
		</ListingTemplate>
	</listing:FilteredListing>

</asp:Content>
