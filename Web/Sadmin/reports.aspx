<%@ Page Language="C#" AutoEventWireup="true" Title="Version Info" Inherits="AspDotNetStorefrontAdmin.Reports" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="reports.aspx.cs" %>

<%@ Register TagPrefix="telerik" Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" %>
<%@ Register TagPrefix="aspdnsf" TagName="TopicToStore" Src="controls/StoreSelector.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="GeneralInfo" Src="controls/GeneralInfo.ascx" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<h1>
		<i class="fa fa-table"></i>
		<asp:Label ID="lblHeader" runat="server" Text="<%$Tokens:StringResource, admin.title.reports %>" />
	</h1>
	<div>
		<aspdnsf:AlertMessage runat="server" ID="ctrlAlertMessage" />
	</div>
	<div class="wrapper">
		<div class="row">
			<div class="col-sm-3">
				<div class="white-ui-box">
					<div class="white-box-heading">
						<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.reports.builtin %>" />
					</div>
					<!-- Report Controls -->
					
					<%--Report Step 1 - Add your report to the report list. --%>
					<asp:DropDownList runat="server" CssClass="form-control" ID="ddReportType" AutoPostBack="true" OnSelectedIndexChanged="ddReportType_SelectedIndexChanged">
						<asp:ListItem Text="Choose a Report" Value="--" />
						<asp:ListItem Text="<%$Tokens:StringResource, admin.reports.abandonedcartlabel %>" Value="AbandonedCart" />
						<asp:ListItem Text="<%$Tokens:StringResource, admin.reports.inventorylevelslabel %>" Value="InventoryLevels" />
						<asp:ListItem Text="<%$Tokens:StringResource, admin.reports.customerslabel %>" Value="Customers" />
						<asp:ListItem Text="<%$Tokens:StringResource, admin.reports.promotionsusagelabel %>" Value="Promotions" />
						<asp:ListItem Text="<%$Tokens:StringResource, admin.reports.emptyentitieslabel %>" Value="EmptyEntities" />
						<asp:ListItem Text="<%$Tokens:StringResource, admin.reports.unmappedproductslabel %>" Value="UnmappedProducts" />
						<asp:ListItem Text="<%$Tokens:StringResource, admin.reports.affiliateslabel %>" Value="Affiliates" />
						<asp:ListItem Text="<%$Tokens:StringResource, admin.reports.referralslabel %>" Value="Referrals" />
						<asp:ListItem Text="<%$Tokens:StringResource, admin.reports.ordersbyentitylabel %>" Value="OrdersByEntity" />
						<asp:ListItem Text="<%$Tokens:StringResource, admin.reports.top10label %>" Value="Bestsellers" />
						<asp:ListItem Text="<%$Tokens:StringResource, admin.reports.customerswhoboughtlabel %>" Value="CustomersByProduct" />
						<asp:ListItem Text="<%$Tokens:StringResource, admin.reports.currentrecurringlabel %>" Value="CurrentRecurring" />
						<asp:ListItem Text="<%$Tokens:StringResource, admin.reports.customerlevelproductslabel %>" Value="ProductsByCustomerLevel" />
						<asp:ListItem Text="<%$Tokens:StringResource, admin.reports.ordersbydaterange %>" Value="OrdersByDateRange" />
						<asp:ListItem Text="<%$Tokens:StringResource, admin.reports.ordersbyitem %>" Value="OrdersByItem" />
						<asp:ListItem Text="<%$Tokens:StringResource, admin.reports.sename %>" Value="SEName" />
						<asp:ListItem Text="<%$Tokens:StringResource, admin.reports.emptyskulabel %>" Value="EmptySku" />
						<asp:ListItem Text="<%$Tokens:StringResource, admin.reports.orderstatslabel %>" Value="OrderStats" />
					</asp:DropDownList>
					<div>
						<asp:Literal runat="server" ID="litReportDescription" />
					</div>
					<table>
						<tr>
							<td colspan="2">
								<asp:Panel ID="pnlStores" runat="server" Visible="false">
									<table class="table">
										<tr>
											<td class="reports-title-small reports-title-default" colspan="2">
												<h4>
													<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.reports.forstore %>" />
												</h4>
											</td>
										</tr>
										<tr>
											<td>
												<div class="drop-down-wrap">
													<aspdnsf:TopicToStore runat="server" ShowText="false" SelectMode="SingleDropDown" ShowDefaultForAllStores="true" ID="ssOne" />
												</div>
											</td>
										</tr>
									</table>
								</asp:Panel>
								<asp:Panel ID="pnlDateSpecs" runat="server" Visible="false">
									<table class="table">
										<tr>
											<td class="reports-title-small reports-title-default" colspan="2">
												<h4>
													<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.reports.specs %>" />
												</h4>
											</td>
										</tr>
										<tr>
											<td colspan="2">
												<div class="wrapperBottom">
													<span class="titleMessage">
														<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.reports.selectdates %>" />
													</span>
												</div>
											</td>
										</tr>
										<tr>
											<td>
												<span class="subTitleSmall">
													<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.reports.startdate %>" />
												</span>
											</td>
											<td>
												<telerik:RadDatePicker ID="dateStart" Width="100%" runat="server" Style="z-index: 150000;" MaxDate="9999-12-31" MinDate="0001-01-01">
													<Calendar ID="Calendar1" runat="server" UseColumnHeadersAsSelectors="False" UseRowHeadersAsSelectors="False" ViewSelectorText="x" />
												</telerik:RadDatePicker>
												<asp:RequiredFieldValidator CssClass="text-danger" ErrorMessage="<%$Tokens:StringResource, admin.common.FillinStartDate %>" Display="Dynamic" ControlToValidate="dateStart" ID="rfvDateStart" SetFocusOnError="true" runat="server" ValidationGroup="getReports" />
											</td>
										</tr>
										<tr>
											<td>
												<span class="subTitleSmall">
													<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.reports.enddate %>" />
												</span>
											</td>
											<td>
												<telerik:RadDatePicker ID="dateEnd" Width="100%" runat="server" Style="z-index: 150000;" MaxDate="9999-12-31" MinDate="0001-01-01">
													<Calendar ID="Calendar2" runat="server" UseColumnHeadersAsSelectors="False" UseRowHeadersAsSelectors="False" ViewSelectorText="x" />
												</telerik:RadDatePicker>
												<asp:RequiredFieldValidator CssClass="text-danger" ErrorMessage="<%$Tokens:StringResource, admin.common.FillinEndDate %>" Display="Dynamic" ControlToValidate="dateEnd" ID="rfvDateEnd" SetFocusOnError="true" runat="server" ValidationGroup="getReports" />
												<asp:CompareValidator CssClass="text-danger" Display="Dynamic" ID="cpvSearchDateRange" runat="server" ControlToValidate="dateEnd" ControlToCompare="dateStart" Operator="GreaterThan" Type="Date" ErrorMessage="Invalid Date Range" ValidationGroup="getReports" />
											</td>
										</tr>
										<tr>
											<td colspan="2">
												<div class="wrapperTopBottom">
													<span class="titleMessage">
														<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.reports.selectrange %>" />
													</span>
												</div>
											</td>
										</tr>
										<tr>
											<td>
												<span class="subTitleSmall">&nbsp;</span>
											</td>
											<td>
												<asp:RadioButtonList runat="server" ID="rblRange" CellPadding="0" CellSpacing="0" RepeatColumns="1">
													<asp:ListItem Value="0" Text="<%$Tokens:StringResource, admin.reports.datesabove %>" Selected="true" />
													<asp:ListItem Value="1" Text="<%$Tokens:StringResource, admin.reports.today %>" />
													<asp:ListItem Value="2" Text="<%$Tokens:StringResource, admin.reports.yesterday %>" />
													<asp:ListItem Value="3" Text="<%$Tokens:StringResource, admin.reports.thisweek %>" />
													<asp:ListItem Value="4" Text="<%$Tokens:StringResource, admin.reports.lastweek %>" />
													<asp:ListItem Value="5" Text="<%$Tokens:StringResource, admin.reports.thismonth %>" />
													<asp:ListItem Value="6" Text="<%$Tokens:StringResource, admin.reports.lastmonth %>" />
													<asp:ListItem Value="7" Text="<%$Tokens:StringResource, admin.reports.thisyear %>" />
													<asp:ListItem Value="8" Text="<%$Tokens:StringResource, admin.reports.lastyear %>" />
												</asp:RadioButtonList>
											</td>
										</tr>
									</table>
								</asp:Panel>
								<%--Input Parameter Step 1 - Add your panel options as an invisible panel.--%>
								<asp:Panel ID="pnlProductId" runat="server" Visible="false">
									<table class="table">
										<tr>
											<td class="reports-title-small reports-title-default" colspan="2">
												<h4>
													<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.reports.productID %>" />
												</h4>
											</td>
										</tr>
										<tr>
											<td>
												<asp:TextBox ID="txtProductId" CssClass="form-control" runat="server" />
											</td>
										</tr>
									</table>
								</asp:Panel>
								<asp:Panel ID="pnlQuantity" runat="server" Visible="false">
									<table class="table">
										<tr>
											<td class="reports-title-small reports-title-default" colspan="2">
												<h4>
													<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.reports.inventorylevel %>" />
												</h4>
											</td>
										</tr>
										<tr>
											<td>
												<asp:TextBox ID="txtQuantity" CssClass="form-control" runat="server" />
												<asp:CompareValidator runat="server" CssClass="text-danger validator-error-adjustments" Operator="DataTypeCheck" Type="Integer" ControlToValidate="txtQuantity" ValidationGroup="getReports" SetFocusOnError="true" ErrorMessage="<%$Tokens:StringResource, admin.common.WholeNumber %>" />
											</td>
										</tr>
									</table>
								</asp:Panel>
								<asp:Panel ID="pnlEntityTypes" runat="server" Visible="false">
									<table class="table">
										<tr>
											<td class="reports-title-small reports-title-default" colspan="2">
												<h4>
													<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.reports.entitytype %>" />
												</h4>
											</td>
										</tr>
										<tr>
											<td>
												<asp:DropDownList CssClass="form-control" ID="ddEntity" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ddEntity_SelectedIndexChanged">
													<asp:ListItem Text="<%$Tokens:StringResource, admin.reports.categories %>" Value="Category" Selected="True" />
													<asp:ListItem Text="<%$Tokens:StringResource, admin.reports.manufacturers %>" Value="Manufacturer" />
													<asp:ListItem Text="<%$Tokens:StringResource, admin.reports.sections %>" Value="Section" />
													<asp:ListItem Text="<%$Tokens:StringResource, admin.reports.stores %>" Value="Stores" />
												</asp:DropDownList>
											</td>
										</tr>
									</table>
								</asp:Panel>
								<asp:Panel ID="pnlAffiliates" runat="server" Visible="false">
									<table class="table">
										<tr>
											<td class="reports-title-small reports-title-default" colspan="2">
												<h4>
													<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.reports.affiliate %>" />
												</h4>
											</td>
										</tr>
										<tr>
											<td>
												<asp:DropDownList CssClass="form-control" ID="ddAffiliates" runat="server" />
											</td>
										</tr>
									</table>
								</asp:Panel>
								<asp:Panel ID="pnlEntities" runat="server" Visible="false">
									<table class="table">
										<tr>
											<td class="reports-title-small reports-title-default" colspan="2">
												<h4>
													<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.reports.chooseentity %>" />
												</h4>
											</td>
										</tr>
										<tr>
											<td>
												<asp:Literal ID="litCategoriesdd" runat="server" Text="Categories:" />
												<br />
												<asp:DropDownList CssClass="form-control" ID="ddCategories" runat="server" />
											</td>
										</tr>
										<tr>
											<td>
												<asp:Literal ID="liteManufacturersdd" runat="server" Text="Manufacturers:" />
												<br />
												<asp:DropDownList CssClass="form-control" ID="ddManufacturers" runat="server" />
											</td>
										</tr>
										<tr>
											<td>
												<asp:Literal ID="litSectionsdd" runat="server" Text="Departments:" />
												<br />
												<asp:DropDownList CssClass="form-control" ID="ddSections" runat="server" />
											</td>
										</tr>
									</table>
								</asp:Panel>
								<asp:Panel ID="pnlCustomerLevels" runat="server" Visible="false">
									<table class="table">
										<tr>
											<td class="reports-title-small reports-title-default" colspan="2">
												<h4>
													<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.reports.customerlevel %>" />
												</h4>
											</td>
										</tr>
										<tr>
											<td>
												<asp:DropDownList CssClass="form-control" ID="ddCustomerLevel" runat="server" />
											</td>
										</tr>
									</table>
								</asp:Panel>
							</td>
						</tr>
					</table>
					<div class="col-list-action-bar">
						<asp:Button runat="server" ID="btnReport" ValidationGroup="getReports" CssClass="btn btn-primary" Visible="false" Text="<%$Tokens:StringResource, admin.reports.getreport %>" OnClick="BtnReport_Click" CausesValidation="true" />
					</div>
				</div>
				<div class="white-ui-box">
					<div class="white-box-heading">
						<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.reports.custom %>" />
					</div>
					<asp:DropDownList runat="server" CssClass="form-control" ID="ddCustomReportType" AutoPostBack="true" CellPadding="0" CellSpacing="0" RepeatColumns="1" OnSelectedIndexChanged="ddCustomReportType_Clicked">
						<%--This will be populated with custom reports (if any) from the DB --%>
						<asp:ListItem Text="Choose a Report" Value="--" />
					</asp:DropDownList>
					<div class="col-list-action-bar">
						<asp:Button runat="server" ID="btnCustomReport" ValidationGroup="getReports" CssClass="btn btn-primary" Visible="false" Text="<%$Tokens:StringResource, admin.reports.getreport %>" OnClick="BtnReport_Click" CausesValidation="true" />
					</div>
				</div>
			</div>
			<div class="col-sm-9">
				<div class="white-ui-box">
					<div class="white-box-heading">
						<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.reports.choosereport %>" />
					</div>
					<asp:Button ID="btnSaveReport" runat="server" CssClass="btn btn-primary" OnClick="BtnSaveReport_click" Text="<%$Tokens:StringResource, admin.reports.saveexcel %>" Visible="false" CausesValidation="true" />
					<div class="reports-scroll" id="divReportResults" runat="server">
						<asp:PlaceHolder ID="phReportResults" runat="server" />
						<asp:Panel ID="pnlSummary" runat="server" Visible="false">
							<table class="table">
								<tr>
									<td class="td-alt">
										<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.reports.registeredcustomers %>" />
									</td>
									<td>
										<asp:Literal ID="litRegisteredCustomers" Text="" runat="server" />
									</td>
								</tr>
								<tr>
									<td class="td-alt">
										<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.reports.anoncustomers %>" />
									</td>
									<td>
										<asp:Literal ID="litAnonymousCustomers" Text="" runat="server" />
									</td>
								</tr>
								<tr>
									<td class="td-alt">
										<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.reports.numorders %>" />
									</td>
									<td>
										<asp:Literal ID="litNumberOrders" Text="" runat="server" />
									</td>
								</tr>
								<tr>
									<td class="td-alt">
										<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.reports.ordertotals %>" />
									</td>
									<td>
										<asp:Literal ID="litOrderTotals" Text="" runat="server" />
									</td>
								</tr>
								<tr>
									<td class="td-alt">
										<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.reports.ordersubtotals %>" />
									</td>
									<td>
										<asp:Literal ID="litOrderSubtotals" Text="" runat="server" />
									</td>
								</tr>
								<tr>
									<td class="td-alt">
										<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.reports.ordershipping %>" />
									</td>
									<td>
										<asp:Literal ID="litOrderShipping" Text="" runat="server" />
									</td>
								</tr>
								<tr>
									<td class="td-alt">
										<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.reports.ordertax %>" />
									</td>
									<td>
										<asp:Literal ID="litOrderTax" Text="" runat="server" />
									</td>
								</tr>
								<tr>
									<td class="td-alt">
										<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.reports.averageorder %>" />
									</td>
									<td>
										<asp:Literal ID="litAverageOrder" Text="" runat="server" />
									</td>
								</tr>
							</table>
						</asp:Panel>
					</div>
				</div>
			</div>
		</div>
	</div>
</asp:Content>