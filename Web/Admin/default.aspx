<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin._default"
	MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master"
	Theme="Admin_Default" Title="<%$Tokens:StringResource, admin.title.default %>" Codebehind="default.aspx.cs" %>

<%@ Import Namespace="AspDotNetStorefrontCore" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<%@ Register TagPrefix="aspdnsf" TagName="XmlPackage" Src="Controls/XmlPackageControl.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="QuickStart" Src="Controls/QuickStart.ascx" %>
<%@ Register TagPrefix="aspdnsf" Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls.Listing" %>
<%@ Register TagPrefix="aspdnsf" Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls" %>

<asp:Content runat="server" ContentPlaceHolderID="head">
	<link href="css/nv.d3.css" rel="stylesheet" />
</asp:Content>

<asp:Content ContentPlaceHolderID="bodyContentPlaceholder" runat="server">
	<h1>
		<i class="fa fa-home"></i>
		<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.dashboard.header %>" />
	</h1>

	<aspdnsf:AlertMessage runat="server" ID="AlertMessageDisplay" />

	<aspdnsf:QuickStart runat="server" />

	<div class="row">
		<div class="col-lg-8">
			<div class="white-ui-box">
				<div class="white-box-heading">
					<a href="orders.aspx" class="pull-right">
						<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.ViewOrders %>" /></a>
					<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.LatestOrders %>" />
				</div>
				<asp:GridView ID="gOrders" AutoGenerateColumns="False" ShowFooter="True" runat="server" Width="100%" GridLines="None" CellPadding="0" CssClass="table table-detail">
					<FooterStyle CssClass="gridFooter" />
					<RowStyle CssClass="gridRow" />
					<EditRowStyle CssClass="DataCellGridEdit" />
					<PagerStyle CssClass="pagerGrid" />
					<HeaderStyle CssClass="gridHeader" />
					<AlternatingRowStyle CssClass="gridAlternatingRow" />
					<Columns>
						<asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.default.LatestOrdersOrder %>">
							<ItemTemplate>
                                <%# string.Format("<a href=\"order.aspx?ordernumber={0}\">{1}</a>", DataBinder.Eval(Container.DataItem, "OrderNumber"), DataBinder.Eval(Container.DataItem, "OrderNumber")) %>
							</ItemTemplate>
							<ItemStyle CssClass="lighterData" Width="60px" />
						</asp:TemplateField>
						<asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.default.LatestOrdersDate %>">
							<ItemTemplate>
								<%# DataBinder.Eval(Container.DataItem, "OrderDate") %>
							</ItemTemplate>
							<ItemStyle CssClass="lightData" Width="160px" />
						</asp:TemplateField>
						<asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.default.LatestOrdersCustomer %>">
							<ItemTemplate>
								<%# string.Format("{0} {1}", DataBinder.Eval(Container.DataItem, "FirstName").ToString().Trim(), DataBinder.Eval(Container.DataItem, "LastName").ToString().Trim()) %>
							</ItemTemplate>
							<ItemStyle CssClass="normalData" Width="180px" />
						</asp:TemplateField>
						<asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.default.LatestOrdersShipping %>">
							<ItemTemplate>
								<%# DataBinder.Eval(Container.DataItem, "ShippingMethod") %>
							</ItemTemplate>
							<ItemStyle CssClass="normalData" Width="350px" />
						</asp:TemplateField>
						<asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.default.LatestOrdersTotal %>">
							<ItemTemplate>
								<%# AspDotNetStorefrontCore.Localization.CurrencyStringForDisplayWithoutExchangeRate((decimal)DataBinder.Eval(Container.DataItem, "OrderTotal"),false) %>
							</ItemTemplate>
							<ItemStyle CssClass="normalData" Width="100px" />
						</asp:TemplateField>
					</Columns>
				</asp:GridView>
			</div>
		</div>
		<div class="col-lg-4">
			<div class="tabcontainer" style="clear: left;">
				<!-- Nav tabs -->
				<ul class="nav nav-tabs" role="tablist">
					<li class="active">
						<a href="#news-tab" role="tab">
							<asp:Literal runat="server" ID="litTabNews" Text='<%$ Tokens:StringResource, admin.dashboard.News %>' />
					    </a>
					</li>
					<li>
						<a href="#quick-look-tab" role="tab">
							<asp:Literal runat="server" ID="litTabQuickLook" Text='<%$ Tokens:StringResource, admin.dashboard.QuickLook %>' />
						</a>
					</li>					
				</ul>
				<!-- Tab panes -->
				<div class="tab-content white-ui-box">
					<div class="tab-pane active" id="news-tab" style="height:350px;">
						<div class="form-group">							
							<div class="row">
								<div class="col-sm-12">
									<asp:Panel ID="Panel1" runat="server">
										<aspdnsf:XmlPackage ID="XmlPackage1" runat="server" PackageName="rss.aspdnsfrssconsumer.xml.config" RuntimeParams="channel=news&height=330" />
									</asp:Panel>
									<asp:Panel ID="Panel2" runat="server">
										<aspdnsf:XmlPackage ID="XmlPackage3" runat="server" PackageName="rss.aspdnsfrssconsumer.xml.config" RuntimeParams="channel=sponsors&height=330" />
									</asp:Panel>
								</div>
							</div>
						</div>					
					</div>
					<div class="tab-pane" id="quick-look-tab" style="height:350px;">
						<div class="form-group">
							<div class="row">
								<div class="col-sm-6">
									<a href="orders.aspx">
										<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.dashboard.CompletedOrders %>" />
									</a>
								</div>
								<div class="col-sm-6">
									<asp:Label ID="lblCompletedOrders" runat="server" />
								</div>
							</div>
							<div class="row">
								<div class="col-sm-6">
									<a href="customers.aspx">
										<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.dashboard.Customers %>" />
									</a>
								</div>
								<div class="col-sm-6">
									<asp:Label ID="lblCustomerWithOrders" runat="server" />
								</div>
							</div>
							<div class="row">
								<div class="col-sm-6">
									<a href="customers.aspx">
										<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.dashboard.Contacts %>" />
									</a>
								</div>
								<div class="col-sm-6">
									<asp:Label ID="lblCustomersWithOutOrders" runat="server" />
								</div>
							</div>
							<div class="row">
								<div class="col-sm-6">
									<a href="products.aspx">
										<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.dashboard.PublishedProducts %>" />
									</a>
								</div>
								<div class="col-sm-6">
									<asp:Label ID="lblPublishedProducts" runat="server" />
								</div>
							</div>
							<div class="row">
								<div class="col-sm-6">
									<a href="promotions.aspx">
										<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.dashboard.ActivePromotions %>" />
									</a>
								</div>
								<div class="col-sm-6">
									<asp:Label ID="lblActivePromotions" runat="server" />
								</div>
							</div>
							<div class="row" runat="server" id="divLowStockCount">
								<div class="col-sm-6">
									<a href="#divLowStock">
										<asp:Label runat="server" Text="Low Stock" />
									</a>
								</div>
								<div class="col-sm-6">
									<asp:Label ID="lblLowStock" runat="server" />
								</div>
							</div>
						</div>
						<div class="form-group">
							<div class="row">
								<div class="col-sm-6">
									<a href="wizard.aspx#payments">
										<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.dashboard.AcceptPayPal %>" />
									</a>
								</div>
								<div class="col-sm-6">
									<asp:Label ID="lblAcceptPayPal" runat="server" />
								</div>
							</div>
							<div class="row">
								<div class="col-sm-6">
									<a href="versioninfo.aspx?excludeImages=true">
										<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.dashboard.Version %>" />
									</a>
								</div>
								<div class="col-sm-6">
									<asp:Label ID="lblVersion" runat="server" />
								</div>
							</div>
						</div>
					</div>
				</div>
			</div>			
			<div class="white-ui-box">
				<div class="form-inline">
					<div class="form-group">
						<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.CachingIs %>" />
					</div>
					<div class="form-group caching-toggle">
						<asp:UpdatePanel runat="server">
							<ContentTemplate>
								<asp:Panel runat="server" ID="CacheToggleWrap" CssClass="toggle-wrap off">
									<asp:UpdateProgress runat="server">
										<ProgressTemplate>
											<div class="progress">
												<div class="progress-bar progress-bar-striped active" role="progressbar" style="width: 100%"></div>
											</div>
										</ProgressTemplate>
									</asp:UpdateProgress>
									<asp:LinkButton OnCommand="SetCache" runat="server" ID="CacheToggle" CssClass="toggle-switch">
										<asp:Label runat="server" class="toggle"></asp:Label>
										<asp:Label ID="CacheOn" CssClass="toggle-on toggle-bg" runat="server" Text="ON" />
										<asp:Label ID="CacheOff" CssClass="toggle-off toggle-bg" runat="server" Text="OFF" />
									</asp:LinkButton>
								</asp:Panel>
							</ContentTemplate>
						</asp:UpdatePanel>
					</div>
				</div>
			</div>
		</div>
	</div>
	<div class="row">
		<div class="col-lg-6">
			<div class="white-ui-box">
				<div class="white-box-heading">
					<a href="orders.aspx" class="pull-right">
						<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.ViewOrders %>" /></a>
					<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.RevenueHeader %>" />
					<select id="orders-length">
						<option value="day">One Day</option>
						<option value="month" selected="selected">One Month</option>
						<option value="year">One Year</option>
					</select>
				</div>
				<div id='ordersChart'>
					<svg class="dash-chart"></svg>
				</div>
			</div>
		</div>
		<div class="col-lg-6">
			<div class="white-ui-box">
				<div class="white-box-heading">
					<a href="products.aspx" class="pull-right">
						<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.ViewManageProducts %>" /></a>
					<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.TopProductsHeader %>" />
					<select id="products-length">
						<option value="day">One Day</option>
						<option value="month" selected="selected">One Month</option>
						<option value="year">One Year</option>
					</select>
				</div>
				<div id='productsChart'>
					<svg class="dash-chart"></svg>
				</div>
			</div>
		</div>
	</div>
	<%--used for the charts--%>
	<asp:HiddenField ID="currencySymbol" ClientIDMode="Static" runat="server" />
	<script type="text/javascript" src="Scripts/d3.v3.min.js"></script>
	<script type="text/javascript" src="Scripts/nv.d3.min.js"></script>
	<script type="text/javascript" src="Scripts/dashboardcharts.js"></script>
	<%--end of charts--%>
	<div runat="server" id="divLowStock">
		<a name="divLowStock"></a>
		<aspdnsf:FilteredListing runat="server"
			SqlQuery="
				select {0}
					Product.ProductID,
					ProductVariant.VariantID,
					Product.Name as Name,
					ProductVariant.Name as VariantName,
					i.Size,
					i.Color,
					case 
						when isnull(Quan, 0) &gt; 0 Then Quan 
						else Inventory 
					end [Inventory] 
				from 
					Product 
					inner join ProductVariant on ProductVariant.ProductId = Product.ProductId
					left join Inventory i on ProductVariant.VariantId = i.VariantId 
				where 
					Product.Deleted = 0 
					and ProductVariant.Deleted = 0 
					and 
						case 
							when isnull(Quan, 0) &gt; 0 then Quan 
							else Inventory 
						end &lt; @SendLowStockWarningsThreshold 
					and {1}"
			SortExpression="Product.ProductID, ProductVariant.VariantID"
			LocaleSelectionEnabled="false">
			<SqlParameters>
				<asp:Parameter Name="sendLowStockWarningsThreshold" DefaultValue="Sets the inventory threshold at which store administrators are notified that a product is running low on stock. Applies only in SendLowStockWarnings is set TRUE." />
			</SqlParameters>
			<ListingTemplate>
				<div class="white-ui-box">
					<div class="white-box-heading">
						<asp:Label runat="server" Text="The following products are below the inventory level you specified in the SendLowStockWarningsThreshold setting and may need to be reordered!" />
					</div>
					<asp:GridView runat="server"
						ID="LowStockGrid" 
						CssClass="table" 
						DataSourceID="FilteredListingDataSource"
						AutoGenerateColumns="False"
						GridLines="None">
						<EmptyDataTemplate>
							<div class="alert alert-info">
								<asp:Literal runat="server" Text="No Low Stock Items Found" />
							</div>
						</EmptyDataTemplate>
						<Columns>
							
							<asp:BoundField
								HeaderText="Product ID"
								HeaderStyle-Width="8%"
								DataField="ProductID" />
							
							<asp:HyperLinkField
								HeaderText="Product Name"
								DataNavigateUrlFields="Name"
								DataNavigateUrlFormatString="bulkeditprices.aspx?filter.0.0={0}"
								DataTextField="Name"
								Text="<i>(no data specified)</i>" />

							<asp:BoundField
								HeaderText="Variant ID"
								HeaderStyle-Width="8%"
								DataField="VariantID" />

							<asp:TemplateField 
								HeaderText="Variant Name" 
								HeaderStyle-Width="24%">
								<ItemTemplate>
									<%# 
									String.IsNullOrEmpty(XmlCommon.GetLocaleEntry(DataBinder.Eval(Container.DataItem, "VariantName").ToString(), ThisCustomer.LocaleSetting, true)) 
										? AppLogic.GetString("admin.nolinktext", ThisCustomer.LocaleSetting)
										: XmlCommon.GetLocaleEntry(DataBinder.Eval(Container.DataItem, "VariantName").ToString(), ThisCustomer.LocaleSetting, true) 
									%>
								</ItemTemplate>
							</asp:TemplateField>

							<asp:TemplateField 
								HeaderText="Size" 
								HeaderStyle-Width="12%">
								<ItemTemplate>
									<%# 
									String.IsNullOrEmpty(XmlCommon.GetLocaleEntry(DataBinder.Eval(Container.DataItem, "Size").ToString(), ThisCustomer.LocaleSetting, true)) 
										? "<i>(no data specified)</i>"
										: XmlCommon.GetLocaleEntry(DataBinder.Eval(Container.DataItem, "Size").ToString(), ThisCustomer.LocaleSetting, true) 
									%>
								</ItemTemplate>
							</asp:TemplateField>

							<asp:TemplateField 
								HeaderText="Color" 
								HeaderStyle-Width="12%">
								<ItemTemplate>
									<%# 
									String.IsNullOrEmpty(XmlCommon.GetLocaleEntry(DataBinder.Eval(Container.DataItem, "Color").ToString(), ThisCustomer.LocaleSetting, true)) 
										? "<i>(no data specified)</i>"
										: XmlCommon.GetLocaleEntry(DataBinder.Eval(Container.DataItem, "Color").ToString(), ThisCustomer.LocaleSetting, true) 
									%>
								</ItemTemplate>
							</asp:TemplateField>

							<asp:BoundField
								HeaderText="Quantity" 
								HeaderStyle-Width="8%"
								DataField="Inventory" />

						</Columns>
					</asp:GridView>
				</div>
			</ListingTemplate>
		</aspdnsf:FilteredListing>
	</div>
</asp:Content>