<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.Feeds" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="feeds.aspx.cs" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">

	<script type="text/javascript">
		function EditFeed(feedid) {
			document.location.href = "feed.aspx?feedid=" + feedid;
		}
	</script>

	<h1>
		<i class="fa fa-list-alt"></i>
		<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.tabs.ProductFeeds %>" />
	</h1>

	<asp:Label ID="Label1" runat="server" Text="<%$Tokens:StringResource, admin.feeds.header %>" />

	<aspdnsf:AlertMessage runat="server" ID="AlertMessage" />

	<div class="item-action-bar">
		<asp:Button ID="btnAddFeed1" runat="server" CssClass="btn btn-default btn-sm" Text="<%$Tokens:StringResource, admin.feeds.AddnewFeed %>" OnClientClick="document.location.href='feed.aspx'; return false;" />
	</div>

	<div class="white-ui-box">
		<table class="table">
			<asp:Repeater ID="rptrFeeds" runat="server" OnItemCommand="rptrFeeds_ItemCommand" OnItemDataBound="rptrFeeds_ItemDataBound">
				<HeaderTemplate>
					<tr class="gridHeader">
						<th>ID</th>
						<th>
							<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.feeds.FeedName %>" />
						</th>
						<th>
							<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.feeds.XMLPackage %>" />
						</th>
						<th>
							<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.feeds.Storename%>" />
						</th>
						<th>
							<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.feeds.Edit %>" />
						</th>
						<th>
							<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.feeds.Execute %>" />
						</th>
						<th>
							<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.feeds.Delete %>" />
						</th>
					</tr>
				</HeaderTemplate>
				<ItemTemplate>
					<tr class="table-row2">
						<td><%# DataBinder.Eval(Container.DataItem, "FeedID") %></td>
						<td>
							<asp:HyperLink ID="lnkFeedEdit" runat="server" NavigateUrl='<%# "feed.aspx?feedid=" + DataBinder.Eval(Container.DataItem, "FeedID").ToString()%>' Text='<%# DataBinder.Eval(Container.DataItem, "Name") %>'></asp:HyperLink>
						</td>
						<td><%# DataBinder.Eval(Container.DataItem, "XmlPackage") %></td>
						<td><%# DataBinder.Eval(Container.DataItem, "StoreName") %></td>
						<td>
							<asp:Button ID="btnEditFeed" runat="server" CssClass="btn btn-default btn-sm" Text="<%$Tokens:StringResource, admin.feeds.EditFeed %>" OnClientClick='<%# "EditFeed(" + DataBinder.Eval(Container.DataItem, "FeedID").ToString() + "); return false;"%>' />
						</td>
						<td>
							<asp:Button ID="btnExecuteFeed" runat="server" CssClass="btn btn-default btn-sm" Text="<%$Tokens:StringResource, admin.feeds.ExecuteFeed %>" CommandArgument='<%# DataBinder.Eval(Container.DataItem, "FeedID") + ":" + DataBinder.Eval(Container.DataItem, "StoreID") %>' CommandName='execute' />
						</td>
						<td>
							<asp:Button ID="btnDeleteFeed" runat="server" CssClass="btn btn-default btn-sm" Text="<%$Tokens:StringResource, admin.feeds.DeleteFeed %>" CommandArgument='<%# DataBinder.Eval(Container.DataItem, "FeedID") %>' CommandName='delete' />
						</td>
					</tr>
				</ItemTemplate>
				<AlternatingItemTemplate>
					<tr class="table-alternatingrow2">
						<td><%# DataBinder.Eval(Container.DataItem, "FeedID") %></td>
						<td>
							<asp:HyperLink ID="lnkFeedEdit" runat="server" NavigateUrl='<%# "feed.aspx?feedid=" + DataBinder.Eval(Container.DataItem, "FeedID").ToString()%>' Text='<%# DataBinder.Eval(Container.DataItem, "Name") %>'></asp:HyperLink>
						</td>
						<td><%# DataBinder.Eval(Container.DataItem, "XmlPackage") %></td>
						<td><%# DataBinder.Eval(Container.DataItem, "StoreName") %></td>
						<td>
							<asp:Button ID="btnEditFeed" runat="server" CssClass="btn btn-default btn-sm" Text="<%$Tokens:StringResource, admin.feeds.EditFeed %>" OnClientClick='<%# "EditFeed(" + DataBinder.Eval(Container.DataItem, "FeedID").ToString() + "); return false;"%>' />
						</td>
						<td>
							<asp:Button ID="btnExecuteFeed" runat="server" CssClass="btn btn-default btn-sm" Text="<%$Tokens:StringResource, admin.feeds.ExecuteFeed %>" CommandArgument='<%# DataBinder.Eval(Container.DataItem, "FeedID") %>' CommandName='execute' />
						</td>
						<td>
							<asp:Button ID="btnDeleteFeed" runat="server" CssClass="btn btn-default btn-sm" Text="<%$Tokens:StringResource, admin.feeds.DeleteFeed %>" CommandArgument='<%# DataBinder.Eval(Container.DataItem, "FeedID") %>' CommandName='delete' />
						</td>
					</tr>
				</AlternatingItemTemplate>
			</asp:Repeater>
		</table>
	</div>

</asp:Content>
