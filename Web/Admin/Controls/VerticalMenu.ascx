<%@ Control Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontControls.VerticalMenu" ClientIDMode="Static" Codebehind="VerticalMenu.ascx.cs" %>
<div class="menu-toggle-wrap">
	<a href="#" id="menu-toggle" class="menu-toggle">
		<i class="fa fa-ellipsis-v"></i>
	</a>
</div>
<div class="main-menu-wrapper">
	<div class="vertical-menu-bar clearfix">
		<div class="vert-menu-header">
			<a href="default.aspx"><i class="fa fa-home"></i> <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.header%>" /></a>
		</div>
	</div>
	<div class="main-menu">
		<ul runat="server" id="easyNav" class="admin-main-menu-column">
			<li class="flyout">
				<a href="#" class="toggle"><i class="fa fa-cube"></i><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.Products%>" /></a>
				<ul class="admin-menu">
					<li><a href="products.aspx"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.ProductMgr %>" /></a></li>
					<li><a href="product.aspx"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.products.AddNew %>" /></a></li>	
					<li><a href="bulkeditprices.aspx"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.pricesinventory%>" /></a></li>
					<li><a href="bulkeditweights.aspx"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.title.bulkeditweights%>" /></a></li>
					<li><a href="entities.aspx?entityname=category"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.entities %>" /></a></li>
					<li><a href="entitybulkdisplayorder.aspx"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.DisplayOrder %>" /></a></li>
					<li><a href="ratings.aspx"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.ratings %>" /></a></li>
					<li><a href="importproductsfromexcel.aspx"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.ProductImportExport%>" /></a></li>
				</ul>
			</li>
			<li class="flyout">
				<a href="#" class="toggle"><i class="fa fa-users"></i><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.customers %>" /></a>
				<ul class="admin-menu">
					<li><a href="customers.aspx"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.customersMgr %>" /></a></li>
					<li><a href="customer.aspx"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.customer.CreateNew %>" /></a></li>
					<li><a href="customerlevels.aspx"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.customerlevels %>" /></a></li>
					<li><a href="entities.aspx?entityname=distributor"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.distributors %>" /></a></li>
				</ul>	
			</li>
			<li class="flyout">
				<a href="#" class="toggle"><i class="fa fa-list"></i><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.Orders %>" /></a>
				<ul class="admin-menu">
					<li><a href="orders.aspx"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.ManageOrders %>" /></a></li>
					<li><a href="createorder.aspx"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.createorder %>" /></a></li>
					<li><a href="bulkshipping.aspx"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.ShippingImportExport %>" /></a></li>
					<li><a href="recurringorders.aspx"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.orderrecurringpending %>" /></a></li>
				</ul>	
			</li>	
			<li class="flyout">
				<a href="#" class="toggle"><i class="fa fa-copy"></i><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.Content %>" /></a>
				<ul class="admin-menu">
					<li><a href="topics.aspx"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.Topics %>" /></a></li>
					<li><a href="news.aspx"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.title.news %>" /></a></li>
					<li><a href="stringresources.aspx"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.StringResources %>" /></a></li>
					<li><a href="skinselector.aspx"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.skinselector %>" /></a></li>
				</ul>	
			</li>
			<li class="flyout">
				<a href="#" class="toggle"><i class="fa fa-shopping-cart"></i><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.Marketing %>" /></a>
				<ul class="admin-menu">
					<li><a href="affiliates.aspx"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.Affiliates %>" /></a></li>
					<li><a href="promotions.aspx"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.Promotions %>" /></a></li>
				</ul>
			</li>	
			<li>
				<a href="reports.aspx"><i class="fa fa-table"></i><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.reports %>" /></a>
			</li> 
		</ul>
	</div>
</div>
