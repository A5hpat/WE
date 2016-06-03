<%@ Page Language="c#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.securitylog" EnableEventValidation="false" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" CodeBehind="securitylog.aspx.cs" %>

<%@ Register TagPrefix="aspdnsf" Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls.Listing" %>
<%@ Register TagPrefix="filter" TagName="DateRangeFilter" Src="Controls/Listing/DateRangeFilter.ascx" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<h1>
		<i class="fa fa-lock"></i>
		365 Day Security Log
	</h1>

	<aspdnsf:AlertMessage runat="server" ID="AlertMessage" />

	<div class="white-ui-box">
		<aspdnsf:FilteredListing runat="server"
			ID="FilteredListing"
			SqlQuery="select {0} SecurityAction Action, Description, ActionDate Date, UpdatedBy CustomerID, c.EMail 
				from SecurityLog with (NOLOCK) 
				left outer join Customer c with (NOLOCK) on SecurityLog.UpdatedBy = c.CustomerID where {1}"
			SortExpression="ActionDate"
			SortDirection="Descending">
			<ActionBarTemplate>
				<asp:Button runat="server" CssClass="btn btn-default" Text="<%$Tokens:StringResource, admin.systemlog.aspx.12 %>" OnClick="btnExport_Click" />
			</ActionBarTemplate>
			<Filters>
				<filter:DateRangeFilter runat="server"
					StartLabel="<%$Tokens:StringResource, admin.systemlog.aspx.7 %>"
					EndLabel="<%$Tokens:StringResource, admin.systemlog.aspx.8 %>"
					FieldName="ActionDate" />
			</Filters>
			<ListingTemplate>
				<div class="white-ui-box">
					<asp:GridView runat="server"
						DataSourceID="FilteredListingDataSource"
						CssClass="table table-detail"
						GridLines="None"
						AutoGenerateColumns="False">
						<EmptyDataTemplate>
							<div class="alert alert-info">
								<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.EmptyDataTemplate.NoResults %>" />
							</div>
						</EmptyDataTemplate>
						<Columns>
							<asp:TemplateField HeaderText="Action">
								<ItemTemplate>
									<%# DecryptValue((string)Eval("Action")) %>
								</ItemTemplate>
							</asp:TemplateField>
								
							<asp:TemplateField HeaderText="Description">
								<ItemTemplate>
									<%# DecryptValue((string)Eval("Description")) %>
								</ItemTemplate>
							</asp:TemplateField>

							<asp:BoundField
								HeaderText="Date"
								DataField="Date" />

							<asp:BoundField
								HeaderText="CustomerID"
								DataField="CustomerID" />

							<asp:BoundField
								HeaderText="EMail"
								DataField="EMail" />
						</Columns>
					</asp:GridView>
			</ListingTemplate>
		</aspdnsf:FilteredListing>
	</div>
</asp:Content>
