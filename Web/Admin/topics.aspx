<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.Topics" MaintainScrollPositionOnPostback="false" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="topics.aspx.cs" %>

<%@ Import Namespace="AspDotNetStorefrontCore" %>

<%@ Register TagPrefix="listing" Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls.Listing" %>
<%@ Register TagPrefix="filter" TagName="StringFilter" Src="Controls/Listing/StringFilter.ascx" %>
<%@ Register TagPrefix="filter" TagName="DataQueryFilter" Src="Controls/Listing/DataQueryFilter.ascx" %>
<%@ Register TagPrefix="filter" TagName="BooleanFilter" Src="Controls/Listing/BooleanFilter.ascx" %>
<%@ Register TagPrefix="filter" TagName="LocaleFilter" Src="Controls/Listing/LocaleFilter.ascx" %>

<asp:Content ContentPlaceHolderID="head" runat="server">

	<script type="text/javascript">
		var deleteConfirmPrompt = "<asp:Literal runat="server" Text="<%$Tokens:EscapedStringResource, admin.common.ConfirmDeletion, javascript %>" />";
		var undeleteConfirmPrompt = "<asp:Literal runat="server" Text="<%$Tokens:EscapedStringResource, admin.common.ConfirmUnDeletion, javascript %>" />";

		jQuery(document).ready(function() {
			$('.confirm-delete > input[type=checkbox]')
				.on('change', null, deleteConfirmPrompt, FilteredListing.checkboxConfirmAndPostback);

			$('.confirm-undelete > input[type=checkbox]')
				.on('change', null, undeleteConfirmPrompt, FilteredListing.checkboxConfirmAndPostback);
		});
	</script>

</asp:Content>
<asp:Content ContentPlaceHolderID="bodyContentPlaceholder" runat="server">
	<div class="admin-module">
		<h1>
			<i class="fa fa-file-code-o"></i>
			<asp:Literal ID="litHeader" runat="server" Text="<%$Tokens:StringResource, admin.topic.list %>" />
		</h1>

		<aspdnsf:AlertMessage ID="AlertMessageDisplay" runat="server" />

		<listing:FilteredListing runat="server"
			ID="FilteredListing"
			SqlQuery="select {0} TopicID, dbo.GetMlValue(Name, @_locale) Name, dbo.GetMlValue(Title, @_locale) Title, Published, StoreId, Deleted from dbo.Topic with(nolock) where {1}"
			SortExpression="dbo.GetMlValue(Name, @_locale)"
			LocaleSelectionEnabled="true">
			<ActionBarTemplate>
				<asp:HyperLink CssClass="btn btn-action" runat="server" Text="<%$ Tokens: StringResource, admin.topic.addnew %>" NavigateUrl="topic.aspx" />
			</ActionBarTemplate>
			<Filters>
				<filter:StringFilter runat="server"
					Label="<%$Tokens:StringResource, admin.edittopic.TopicName %>"
					FieldName="Name" />

				<filter:StringFilter runat="server"
					Label="<%$Tokens:StringResource, admin.common.Description %>"
					FieldName="Description" />

				<filter:DataQueryFilter runat="server"
					Label="Mapped to Store"
					FieldName="StoreId"
					DataQuery="SELECT DISTINCT StoreId, Name FROM Store AS Name ORDER BY StoreId"
					DataTextField="Name"
					DataValueField="StoreId"
					UnspecifiedLabel="Any Store" />

				<filter:LocaleFilter runat="server"
					Label="Show Topics for Locale"
					FieldName="Title" />

				<filter:BooleanFilter runat="server"
					Label="Published"
					FieldName="Published" />

				<filter:BooleanFilter runat="server"
					Label="Show Frequently Used Topics"
					FieldName="IsFrequent" />

				<filter:BooleanFilter runat="server"
					Label="Deleted"
					FieldName="Deleted"
					DefaultValue="False" />
			</Filters>

			<ListingTemplate>
				<div class="white-ui-box">
					<asp:GridView runat="server"
						ID="Grid"
						CssClass="table js-sortable-gridview"
						GridLines="None"
						DataSourceID="FilteredListingDataSource"
						AllowSorting="true"
						AutoGenerateColumns="false"
						DataKeyNames="TopicID, Name"
						OnRowCommand="DispatchGridCommand">
						<EmptyDataTemplate>
							<div class="alert alert-info">
								<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.EmptyDataTemplate.NoResults %>" />
							</div>
						</EmptyDataTemplate>
						<Columns>
							<asp:HyperLinkField
								HeaderText="<%$ Tokens: StringResource, admin.common.ID %>"
								HeaderStyle-Width="7%"
								SortExpression="TopicID"
								DataTextField="TopicID"
								DataNavigateUrlFields="TopicID"
								DataNavigateUrlFormatString="topic.aspx?topicid={0}" />

							<asp:HyperLinkField
								HeaderText="<%$Tokens:StringResource, admin.common.Name %>"
								SortExpression="dbo.GetMlValue(Name, @_locale)"
								DataTextField="Name"
								DataNavigateUrlFields="TopicID"
								DataNavigateUrlFormatString="topic.aspx?topicid={0}"
								Text="<%$Tokens:StringResource, admin.nolinktext %>" />

							<asp:BoundField
								HeaderText="<%$Tokens:StringResource, admin.topic.Title %>"
								SortExpression="dbo.GetMlValue(Title, @_locale)"
								DataField="Title" />

							<asp:BoundField
								HeaderText="Published"
								SortExpression="Published"
								DataField="Published" />

							<asp:TemplateField
								HeaderText="<%$ Tokens: StringResource, admin.common.StoreName %>">
								<ItemTemplate>
									<%# (int)Eval("StoreId") == 0 ? "All Stores" : Store.GetStoreName((int)Eval("StoreId")) %>
								</ItemTemplate>
							</asp:TemplateField>

							<asp:TemplateField
								HeaderText="<%$ Tokens: StringResource, admin.common.Deleted %>"
								HeaderStyle-Width="8%">
								<ItemTemplate>
									<aspdnsf:CommandCheckBox runat="server"
										CssClass='<%# (byte)DataBinder.Eval(Container.DataItem, "Deleted") == 1 ? "confirm-undelete" : "confirm-delete" %>'
										CheckedCommandName="<%# DeleteTopicCommand %>"
										UncheckedCommandName="<%# UndeleteTopicCommand %>"
										CommandArgument='<%# DataBinder.Eval(Container.DataItem, "TopicID") %>'
										Checked='<%# (byte)DataBinder.Eval(Container.DataItem, "Deleted") == 1 %>' />
								</ItemTemplate>
							</asp:TemplateField>

						</Columns>
					</asp:GridView>
				</div>
			</ListingTemplate>
		</listing:FilteredListing>
		<hr />
		<div class="admin-row">
			<div id="divFileTopicsGrid" class="white-ui-box">
				<div class="white-box-heading">
					<asp:Literal ID="litgrdFileTopicsHead" runat="server" Text="File-Based Topics (Not Filtered)" />
				</div>
				<asp:GridView runat="server"
					ID="grdFileTopics"
					CssClass="table"
					GridLines="None"
					ShowHeader="true"
					AllowPaging="true"
					PageSize="20"
					OnPageIndexChanging="grdFileTopics_OnPageIndexChanging"
					AutoGenerateColumns="false">
					<Columns>
						<asp:HyperLinkField
							HeaderText="<%$Tokens:StringResource, admin.common.Name %>"
							DataTextField="Name"
							DataNavigateUrlFields="Link"
							Target="_blank" />

						<asp:BoundField
							HeaderText="Location"
							DataField="Location" />

					</Columns>
				</asp:GridView>
			</div>
		</div>

	</div>

</asp:Content>
