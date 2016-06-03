<%@ Page Language="C#" Inherits="AspDotNetStorefrontAdmin.SystemLogs" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="systemlogs.aspx.cs" %>

<%@ Register TagPrefix="aspdnsf" Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls.Listing" %>
<%@ Register TagPrefix="filter" TagName="DateRangeFilter" Src="Controls/Listing/DateRangeFilter.ascx" %>
<%@ Register TagPrefix="filter" TagName="EnumSelectFilter" Src="Controls/Listing/EnumSelectFilter.ascx" %>

<asp:Content runat="server" ContentPlaceHolderID="head">
	<script type="text/javascript">
		var clearLogPrompt = "<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.systemlog.aspx.14 %>" />";

		jQuery(document)
			.ready(function() {
				$('.clear-log')
					.on('click', null, clearLogPrompt , promptAndConfirm);
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

	<aspdnsf:AlertMessage runat="server" ID="AlertMessage" />

	<h1>
		<i class="fa fa-exclamation-triangle"></i>
		<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.systemlog.aspx.11 %>" />
	</h1>

	<aspdnsf:FilteredListing runat="server"
		ID="FilteredListing"
		SqlQuery="select {0} * from aspdnsf_SysLog with(nolock) where {1}"
		SortExpression="CreatedOn"
		SortDirection="Descending">
		<ActionBarTemplate>
			<asp:Button runat="server" CssClass="btn btn-default" Text="<%$Tokens:StringResource, admin.systemlog.aspx.12 %>" OnClick="btnExport_Click" />
			<asp:Button runat="server" CssClass="btn btn-primary clear-log" Text="<%$Tokens:StringResource, admin.systemlog.aspx.13 %>" OnClick="btnClear_Click" />
		</ActionBarTemplate>
		<Filters>
			<filter:EnumSelectFilter runat="server" EnumType="AspDotNetStorefrontCore.MessageSeverityEnum, AspDotNetStorefrontCore" Label="Severity" FieldName="Severity" />
			<filter:EnumSelectFilter runat="server" EnumType="AspDotNetStorefrontCore.MessageTypeEnum, AspDotNetStorefrontCore" Label="Type" FieldName="Type" />
			<filter:DateRangeFilter runat="server"
				StartLabel="<%$Tokens:StringResource, admin.systemlog.aspx.7 %>"
				EndLabel="<%$Tokens:StringResource, admin.systemlog.aspx.8 %>"
				FieldName="CreatedOn" />
		</Filters>
		<ListingTemplate>
			<div class="white-ui-box">
				<asp:GridView runat="server"
					ID="gMain"
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
						<asp:BoundField
							HeaderText="ID"
							ItemStyle-Width="5%"
							DataField="SysLogID" />

						<asp:HyperLinkField
							HeaderText="<%$ Tokens:StringResource, admin.systemlog.aspx.2 %>"
							DataNavigateUrlFields="SysLogID"
							DataNavigateUrlFormatString="systemlog.aspx?logid={0}"
							DataTextField="Message"
							Text="<%$Tokens:StringResource, admin.nolinktext %>" />

						<asp:BoundField
							HeaderText="<%$Tokens:StringResource, admin.systemlog.aspx.6 %>"
							ItemStyle-Width="15%"
							DataField="CreatedOn" />

						<asp:BoundField
							HeaderText="<%$Tokens:StringResource, admin.systemlog.aspx.4 %>"
							DataField="Type" />

						<asp:BoundField
							HeaderText="<%$ Tokens:StringResource, admin.systemlog.aspx.5 %>"
							DataField="Severity" />
					</Columns>
				</asp:GridView>
		</ListingTemplate>
	</aspdnsf:FilteredListing>
</asp:Content>
