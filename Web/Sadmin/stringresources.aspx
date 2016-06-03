<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.stringresourcepage" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="stringresources.aspx.cs" %>

<%@ Import Namespace="AspDotNetStorefrontCore" %>

<%@ Register TagPrefix="aspdnsf" Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls.Listing" %>
<%@ Register TagPrefix="aspdnsf" TagName="StringFilter" Src="Controls/Listing/StringFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="BooleanFilter" Src="Controls/Listing/BooleanFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="DataQueryFilter" Src="Controls/Listing/DataQueryFilter.ascx" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<h1>
		<i class="fa fa-pencil-square-o"></i>
		<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.stringresources.StringResources %>" />
	</h1>

	<aspdnsf:AlertMessage runat="server" ID="AlertMessage" />

	<aspdnsf:FilteredListing runat="server"
		ID="FilteredListing"
		SqlQuery="
			select {0} 
				StringResource.StringResourceID,
				StringResource.Name,
				StringResource.ConfigValue,
				case when StringResource.Modified = 1 then 'Yes' else 'No' end [Modified],
				StringResource.StoreID,
				coalesce(LocaleSetting.Description, StringResource.LocaleSetting) [LocaleSetting],
				Store.Name [StoreName]
			from
				StringResource with (NOLOCK)
				left join Store on Store.StoreID = StringResource.StoreID 
				left join LocaleSetting on LocaleSetting.Name = StringResource.LocaleSetting
			where {1}"
		SortExpression="StringResource.Name">
		<ActionBarTemplate>

			<asp:Panel ID="pnlLocale" Visible="false" runat="server" CssClass="other-actions">
				<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.stringresources.Locale %>" AssociatedControlID="ddLocales" />
				<asp:DropDownList ID="ddLocales" runat="server" AutoPostBack="true" />
			</asp:Panel>

			<span class="dropdown">
				<button class="btn btn-default dropdown-toggle" type="button" data-toggle="dropdown" aria-expanded="true">
					<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.stringresources.ReloadFromExcelOnServer %>" />
					<span class="caret"></span>
				</button>
				<ul class="dropdown-menu dropdown-menu-right" role="menu">
					<asp:Repeater runat="server" DataSource="<%# LocaleSource.Locales.Where(locale => StringResourceManager.CheckStringResourceExcelFileExists(locale.Name)) %>">
						<ItemTemplate>
							<li role="presentation">
								<a href='<%# String.Format("ImportStringResourceFile1.aspx?showlocalesetting={0}&master=true", Eval("Name")) %>'>
									<%# Eval("Description") %>
								</a>
							</li>
						</ItemTemplate>
					</asp:Repeater>
				</ul>
			</span>

			<span class="dropdown">
				<button class="btn btn-default dropdown-toggle" type="button" data-toggle="dropdown" aria-expanded="true">
					<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.stringresources.ReloadFromExcelOnPC %>" />
					<span class="caret"></span>
				</button>
				<ul class="dropdown-menu dropdown-menu-right" role="menu">
					<asp:Repeater runat="server" DataSource="<%# LocaleSource.Locales %>">
						<ItemTemplate>
							<li role="presentation">
								<a href='<%# String.Format("ImportStringResourceFile1.aspx?showlocalesetting={0}", Eval("Name")) %>'>
									<%# Eval("Description") %>
								</a>
							</li>
						</ItemTemplate>
					</asp:Repeater>
				</ul>
			</span>

			<span runat="server" class="dropdown" visible='<%# LocaleSource.HasMultipleLocales() %>'>
				<button class="btn btn-default dropdown-toggle" type="button" data-toggle="dropdown" aria-expanded="true">
					<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.stringresources.ShowMissing %>" />
					<span class="caret"></span>
				</button>
				<ul class="dropdown-menu dropdown-menu-right" role="menu">
					<asp:Repeater runat="server" DataSource="<%# LocaleSource.Locales %>">
						<ItemTemplate>
							<li role="presentation">
								<a href='<%# String.Format("StringResourceRpt.aspx?reporttype=missing&ShowLocaleSetting={0}", Eval("Name")) %>'>
									<%# Eval("Description") %>
								</a>
							</li>
						</ItemTemplate>
					</asp:Repeater>
				</ul>
			</span>

			<span runat="server" class="dropdown" visible='<%# LocaleSource.HasMultipleLocales() %>'>
				<button class="btn btn-default dropdown-toggle" type="button" data-toggle="dropdown" aria-expanded="true">
					<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.stringresources.ClearLocale %>" />
					<span class="caret"></span>
				</button>
				<ul class="dropdown-menu dropdown-menu-right" role="menu">
					<asp:Repeater runat="server" DataSource="<%# LocaleSource.Locales %>">
						<ItemTemplate>
							<li role="presentation">
								<asp:LinkButton runat="server"
									class="js-confirm-delete"
									data-locale='<%# Eval("Name") %>'
									CommandName="<%# ClearLocalCommand %>"
									CommandArgument='<%# Eval("Name") %>'
									OnCommand="ClearLocaleLink_Command"
									Text='<%# Eval("Description") %>' />
							</li>
						</ItemTemplate>
					</asp:Repeater>
				</ul>
			</span>

			<asp:HyperLink runat="server" CssClass="btn btn-action" Text="<%$ Tokens:StringResource, admin.stringresources.AddNewString %>" NavigateUrl="stringresource.aspx" />
		</ActionBarTemplate>
		<Filters>
			<aspdnsf:StringFilter runat="server"
				Label="Name"
				FieldName="StringResource.Name" />
			<aspdnsf:StringFilter runat="server"
				Label="Value"
				FieldName="StringResource.ConfigValue" />
			<aspdnsf:DataQueryFilter runat="server"
				Label="Locale"
				FieldName="StringResource.LocaleSetting"
				DataQuery="select Name, Description from LocaleSetting with (NOLOCK) order by DisplayOrder, Description"
				DataTextField="Description"
				DataValueField="Name"
				QueryStringNames="filterlocale" />
			<aspdnsf:DataQueryFilter runat="server"
				Label="<%$Tokens:StringResource, admin.order.ForStore %>"
				FieldName="StringResource.StoreId"
				DataQuery="select StoreId, Name from Store"
				DataTextField="Name"
				DataValueField="StoreId" />
			<aspdnsf:BooleanFilter runat="server"
				Label="Modified"
				FieldName="StringResource.Modified" />
		</Filters>
		<ListingTemplate>
			<div class="white-ui-box">
				<asp:GridView runat="server"
					ID="gMain"
					CssClass="table js-sortable-gridview"
					DataSourceID="FilteredListingDataSource"
					AutoGenerateColumns="False"
					OnRowCommand="gMain_RowCommand"
					DataKeyNames="StringResourceID"
					AllowSorting="true"
					GridLines="None">
					<EmptyDataTemplate>
						<div class="alert alert-info">
							<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.EmptyDataTemplate.NoResults %>" />
						</div>
					</EmptyDataTemplate>
					<Columns>

						<asp:BoundField
							HeaderText="ID"
							HeaderStyle-Width="5%"
							SortExpression="StringResource.StringResourceID"
							DataField="StringResourceID" />

						<asp:HyperLinkField
							HeaderText="Name"
							SortExpression="StringResource.Name"
							DataNavigateUrlFields="StringResourceID"
							DataNavigateUrlFormatString="stringresource.aspx?stringid={0}"
							DataTextField="Name"
							Text="<%$Tokens:StringResource, admin.nolinktext %>" />

						<asp:BoundField
							HeaderText="Value"
							SortExpression="StringResource.ConfigValue"
							DataField="ConfigValue" />

						<asp:BoundField
							HeaderText="Modified"
							DataField="Modified" />

						<asp:BoundField
							HeaderText="Store"
							DataField="StoreName" />

						<asp:BoundField
							HeaderText="Locale"
							DataField="LocaleSetting" />

						<asp:TemplateField
							HeaderStyle-Width="5%">
							<ItemTemplate>
								<asp:LinkButton runat="Server"
									CssClass="delete-link fa-times"
									ToolTip="Delete"
									CommandName="<%# DeleteCommand %>"
									CommandArgument='<%# Eval("StringResourceID") %>'
									Text="<%$Tokens:StringResource, admin.common.Delete %>" />
							</ItemTemplate>
						</asp:TemplateField>

					</Columns>
				</asp:GridView>
			</div>
		</ListingTemplate>
	</aspdnsf:FilteredListing>

	<script>
		$('.js-confirm-delete').click(function(e) {
			var locale = $(this).data('locale');
			var prompt = 'Are you sure you want to delete all prompts in the ' + locale + ' locale from the database?'
			if(!confirm(prompt))
				e.preventDefault();
		});
	</script>
</asp:Content>
