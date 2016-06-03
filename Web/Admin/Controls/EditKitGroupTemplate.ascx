<%@ Control Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.Controls.Admin_controls_editkitgrouptemplate" Codebehind="EditKitGroupTemplate.ascx.cs" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>
<%@ Import Namespace="AspDotNetStorefrontCore" %>
<%@ Import Namespace="System.Linq" %>

<% if(this.KitGroup.IsValid == false)
   { %>
<div class="admin_kit_group_validationErrors">
	Please correct the following errors and try again:
		<ul>
			<% foreach(ValidationError verror in this.KitGroup.ValidationErrors)
	  { %>
			<li><%= verror.ErrorMessage%>            </li>
			<% } %>
		</ul>
</div>
<% } %>
<div id="pnlGroupId" class="white-box-heading" runat="server">
	<% if(this.KitGroup.IsNew)
	{ %>
	<span class="admin_kit_group_newItem">[New]</span>
	<% }
	else
	{ %>
	<b>Group:</b> <%=this.KitGroup.Name %>
	<br />
	Id: <%= KitGroup.Id.ToString() %>
	<% } %>
</div>
<div class="form-group">
	<div class="row admin-row">
		<div class="col-sm-2">
			Name:
		</div>
		<div class="col-sm-6">
			<asp:TextBox ID="txtGroupName" runat="server" Text='<%# KitGroup.Name %>' CssClass="form-control" />
		</div>
	</div>
	<div class="row admin-row">
		<div class="col-sm-2">
			Summary:
		</div>
		<div class="col-sm-6">
			<asp:TextBox ID="txtGroupSummary" runat="server" TextMode="MultiLine" Text='<%# KitGroup.Summary %>' class="form-control" />
		</div>
	</div>
	<div class="row admin-row">
		<div class="col-sm-2">
			Description:
		</div>
		<div class="col-sm-6">
			<asp:TextBox ID="txtGroupDescription" runat="server" TextMode="MultiLine" Text='<%# KitGroup.Description %>' class="form-control" />
		</div>
	</div>
	<div class="row admin-row">
		<div class="col-sm-2">
			Required:
		</div>
		<div class="col-sm-6">
			<asp:CheckBox ID="chkRequired" runat="server" Checked='<%# KitGroup.IsRequired %>' />
		</div>
	</div>
	<div class="row admin-row">
		<div class="col-sm-2">
			Read Only:
		</div>
		<div class="col-sm-6">
			<asp:CheckBox ID="chkReadOnly" runat="server" Checked='<%# KitGroup.IsReadOnly %>' />
		</div>
	</div>
	<div class="row admin-row">
		<div class="col-sm-2">
			Display Order:
		</div>
		<div class="col-sm-6">
			<asp:TextBox ID="txtDisplayOrder" runat="server" MaxLength="3" CssClass="text-3" Text='<%# KitGroup.DisplayOrder%>' />
		</div>
	</div>
	<div class="row admin-row">
		<div class="col-sm-2">
			Group Type:
		</div>
		<div class="col-sm-6">
			<asp:DropDownList ID="cboGroupType" runat="server"
				DataSource='<%# this.GroupTypes %>'
				DataValueField="Id"
				DataTextField="Name"
				CssClass='<%# this.CssClassIfInvalid(KitGroup, "SelectionControl", "admin_kit_group_inputError")  %>'>
			</asp:DropDownList>
		</div>
	</div>
	<%if(KitGroup.IsNew == false)
   { %>
	<div class="row admin-row">
		<div class="col-sm-2">
			Images:
		</div>
		<div class="col-sm-6">
			<asp:HyperLink ID="lnkManageImages" runat="server" Text="Manage Images" NavigateUrl='<%# this.GenerateManageImagesLink()  %>' />
		</div>
	</div>
	<%} %>
</div>
<div class="form-group">
	<div class="admin_kit_groupItems tabcontainer">		
		<ul class="nav nav-tabs" role="tablist">
			<li class="active"><a href="#<%=general.ClientID %>" role="tab" data-toggle="tab">General</a></li>
			<li><a href="#<%=inventory.ClientID %>" role="tab" data-toggle="tab">Inventory Variant</a></li>
			<li><a href="#<%=pricing.ClientID %>" role="tab" data-toggle="tab">Pricing</a></li>
		</ul>
		<div class="tab-content edit-kit-tab-content">
			<div runat="server" class="tab-pane active" id="general">
				<asp:DataList ID="dlItemsGeneralGroup" CssClass="admin-table-no-border" runat="server" DataSource='<%# KitGroup.Items %>' OnItemCommand="dlItemsGeneralGroup_ItemCommand" OnItemCreated="dlItemsGeneralGroup_ItemCreated">
					<HeaderTemplate>
						<tr>
							<th>ID</th>
							<th>Order</th>
							<th>Name</th>
							<th>Description</th>
							<th>Default</th>
							<th>
								<%= ShowDeleteHeader()%>
							</th>
						</tr>
					</HeaderTemplate>
					<ItemTemplate>
						<tr>
							<td>
								<asp:HiddenField ID="hdfKitItemId" runat="server" Value='<%# Container.DataItemAs<KitItemData>().Id %>' />
								<asp:Label ID="lblItemId" runat="server"
									Text='<%# DetermineDisplayId(Container.DataItemAs<KitItemData>().Id) %>'
									CssClass='<%# this.CssClassIf(Container.DataItemAs<KitItemData>().IsNew, "admin_kit_group_newItem") %>'>
								</asp:Label>
							</td>
							<td>
								<asp:TextBox ID="txtKitItemDisplayOrder" CssClass="text-3" runat="server" MaxLength="3" Text='<%# Container.DataItemAs<KitItemData>().DisplayOrder %>'>
								</asp:TextBox>
							</td>
							<td>
								<asp:TextBox ID="txtKitItemName" runat="server" Text='<%# Container.DataItemAs<KitItemData>().Name %>' CssClass="text-md">
								</asp:TextBox>
							</td>
							<td>
								<asp:TextBox ID="txtKitItemDescription" runat="server" Columns="50" Text='<%# Container.DataItemAs<KitItemData>().Description %>' CssClass="text-lg" />
							</td>
							<td>
								<asp:CheckBox ID="chkKitItemDefault" runat="server" Checked='<%# Container.DataItemAs<KitItemData>().IsDefault %>' />
							</td>
							<td>
								<%--
									let's only show the delete button if this is an
									already existing line item and there are more than
									one none-new line items for this group
								--%>
								<asp:LinkButton class="delete-link" ID="cmdDeleteItem" runat="server"
									CommandName="Delete_KitItem"
									CommandArgument='<%# Container.DataItemAs<KitItemData>().Id %>'
									Visible='<%# Container.DataItemAs<KitItemData>().Id > 0 && Container.DataItemAs<KitItemData>().Group.NonNewItems.Count() > 1 %>' />
							</td>
						</tr>
					</ItemTemplate>
					<SeparatorTemplate>
						<h4></h4>
					</SeparatorTemplate>
				</asp:DataList>
			</div>
			<div runat="server" class="tab-pane" id="inventory">
				<asp:DataList ID="dltemsInventoryVariantGroup" runat="server" CssClass="admin-table-no-border" DataSource='<%# KitGroup.Items %>' OnItemDataBound="dltemsInventoryVariantGroup_ItemDataBound">
					<HeaderTemplate>
						<tr>
							<th>ID</th>
							<th>Variant Id</th>
							<th>Quantity Delta</th>
							<th>Size</th>
							<th>Color</th>
						</tr>
					</HeaderTemplate>
					<ItemTemplate>
						<tr>
							<td>
								<asp:HiddenField ID="hdfKitItemId" runat="server" Value='<%# Container.DataItemAs<KitItemData>().Id %>' />
								<asp:Label ID="lblItemId" runat="server"
									Text='<%# DetermineDisplayId(Container.DataItemAs<KitItemData>().Id) %>'
									CssClass='<%# this.CssClassIf(Container.DataItemAs<KitItemData>().IsNew, "admin_kit_group_newItem") %>'>
								</asp:Label>
							</td>
							<td>
								<asp:HyperLink ID="lnkSelect" runat="server" NavigateUrl="javascript:void(0);">Select</asp:HyperLink>
								<asp:TextBox ID="txtInventoryVariantId" runat="server" Text='<%# Container.DataItemAs<KitItemData>().InventoryVariantId %>' />
							</td>
							<td>
								<asp:TextBox ID="txtInventoryQuantityDelta" runat="server" Text='<%# Container.DataItemAs<KitItemData>().InventoryQuantityDelta %>' />
							</td>
							<td>
								<asp:TextBox ID="txtInventoryVariantSize" runat="server" Text='<%# Container.DataItemAs<KitItemData>().InventoryVariantSize %>' />
							</td>
							<td>
								<asp:TextBox ID="txtInventoryVariantColor" runat="server" Text='<%# Container.DataItemAs<KitItemData>().InventoryVariantColor %>' />
							</td>
						</tr>
					</ItemTemplate>
					<SeparatorTemplate>
						<h4></h4>
					</SeparatorTemplate>
				</asp:DataList>
			</div>
			<div runat="server" class="tab-pane" id="pricing">
				<asp:DataList ID="dlItemsPricingGroup" runat="server" CssClass="admin-table-no-border" DataSource='<%# KitGroup.Items %>'>
					<HeaderTemplate>
						<tr>
							<th>ID</th>
							<th>Price Delta</th>
							<th>Weight Delta</th>
						</tr>
					</HeaderTemplate>
					<ItemTemplate>
						<tr>
							<td>
								<asp:HiddenField ID="hdfKitItemId" runat="server" Value='<%# Container.DataItemAs<KitItemData>().Id %>' />
								<asp:Label ID="lblItemId" runat="server" Text='<%# DetermineDisplayId(Container.DataItemAs<KitItemData>().Id) %>' CssClass='<%# this.CssClassIf(Container.DataItemAs<KitItemData>().IsNew, "admin_kit_group_newItem") %>' />
							</td>
							<td>
								<asp:TextBox ID="txtPriceDelta" CssClass="text-md" runat="server" Text='<%# Container.DataItemAs<KitItemData>().PriceDelta %>' />
							</td>
							<td>
								<asp:TextBox ID="txtWeightDelta" CssClass="text-md" runat="server" Text='<%# Container.DataItemAs<KitItemData>().WeightDelta %>' />
							</td>
						</tr>
					</ItemTemplate>
					<SeparatorTemplate>
						<h4></h4>
					</SeparatorTemplate>
				</asp:DataList>
			</div>
		</div>					
	</div>
</div>