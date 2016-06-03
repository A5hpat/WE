<%@ Page Language="c#" Inherits="AspDotNetStorefrontAdmin.EntityBulkDisplayOrder" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="entitybulkdisplayorder.aspx.cs" %>
<%@ Import Namespace="AspDotNetStorefrontCore" %>

<asp:Content ID="Content1" ContentPlaceHolderID="bodyContentPlaceholder" runat="server">
	<div id="container">
		<div class="main-breadcrumb">
			<asp:HyperLink ID="lnkTopLevel" 
				runat="server" 
				Text="<%# TopLevelString %>" 
				NavigateUrl='<%# string.Format("entitybulkdisplayorder.aspx?entitytype={0}", EntityType)%>'
				Visible="<%# EntityId > 0 %>" />

			<asp:Label runat="server"
				Text="<%# TopLevelString %>"
				Visible="<%# EntityId == 0 %>" />

			<asp:Repeater ID="BreadCrumb" runat="server">
				<ItemTemplate>
					&gt;&#32;
					<a href='<%# string.Format("entitybulkdisplayorder.aspx?entitytype={0}&entityid={1}", Eval("EntityType"), Eval("ID"))%>'>
						<asp:Literal ID="txtParentName" runat="server" Text='<%# Eval("LocaleName") %>' />
					</a>
				</ItemTemplate>
			</asp:Repeater>

			<span runat="server" 
				visible="<%# !string.IsNullOrEmpty(CurrentEntityName) %>">
				&gt;&#32;
				<span>
					<asp:Literal ID="txtName" runat="server" Text='<%# CurrentEntityName %>' />
				</span>
			</span>
		</div>
		<div class="wrapper" id="divwrapper" runat="server">
			<h1 runat="server" id="header">
				<i class="fa fa-list-ol"></i>
				<asp:Label ID="lblHeader" runat="server" Text="<%$Tokens:StringResource, admin.menu.DisplayOrder %>" />
			</h1>

			<div>
				<asp:Label ID="lblHeaderTip" runat="server" Text="<%$Tokens:StringResource, admin.displayorder.HeaderTip %>" />
			</div>

			<aspdnsf:AlertMessage runat="server" ID="AlertMessageDisplay" />

			<div class="list-action-bar">
				<asp:Panel ID="pnlEntityType" runat="server" CssClass="other-actions" >
					<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entity.SelectEntityType %>" AssociatedControlID="ddEntityType" />
					<asp:DropDownList ID="ddEntityType" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddEntityType_SelectedIndexChanged" >
						<asp:ListItem Text="<%$ Tokens:StringResource, admin.common.Category %>" Value="category" />
						<asp:ListItem Text="<%$ Tokens:StringResource, admin.common.Manufacturer %>" Value="manufacturer" />
						<asp:ListItem Text="<%$ Tokens:StringResource, admin.common.Section %>" Value="section" />
					</asp:DropDownList>
					<asp:DropDownList ID="ddEntity" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddEntity_SelectedIndexChanged" />
				</asp:Panel>
				<asp:Button ID="btnTopUpdate" runat="server" CausesValidation="true" ValidationGroup="valUpdate" Text="<%$Tokens:StringResource, admin.common.Save %>" CssClass="btn btn-primary" OnClick="UpdateDisplayOrder" />
			</div>

			<div class="white-ui-box">
				<asp:GridView runat="server"
					ID="grdDisplayOrder"
					CssClass="table js-sortable-gridview"
					GridLines="None" 
					DataKeyNames="EntityId"
					AutoGenerateColumns="false"
					OnDataBinding="grdDisplayOrder_DataBinding">
					<EmptyDataTemplate>
						<div class="alert alert-info">
							<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.EmptyDataTemplate.NoResults %>" />
						</div>
					</EmptyDataTemplate>
					<Columns>
						<asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.common.DisplayOrderHeader %>" HeaderStyle-Width="10%">
							<ItemTemplate>
								<asp:TextBox ID="txtDisplayOrder" runat="server" CssClass="text-4" Text='<%# Eval("DisplayOrder") %>' />
							<asp:RequiredFieldValidator ErrorMessage="Fill in Display Order!" CssClass="text-danger" 
								ControlToValidate="txtDisplayOrder" ID="reqDisplayOrder" Display="Dynamic" 
								ValidationGroup="valUpdate" SetFocusOnError="true" runat="server" />
							<asp:CompareValidator ErrorMessage="Whole numbers only!" CssClass="text-danger" Operator="DataTypeCheck"
								 ControlToValidate="txtDisplayOrder" Type="Integer" ValidationGroup="valUpdate" 
								ID="cmpDisplayOrder" runat="server" Display="Dynamic" />	
							</ItemTemplate>
						</asp:TemplateField>

						<asp:BoundField
							HeaderText="<%$ Tokens: StringResource, admin.common.ID %>"
							HeaderStyle-Width="3%"
							DataField="EntityID" />

						<asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.common.Name %>">
							<ItemTemplate>
									<a runat='server'
										href='<%# string.Format("entitybulkdisplayorder.aspx?entitytype={0}&entityid={1}", Eval("EntityType"), Eval("EntityId"))%>'
										visible='<%# (int)Eval("ChildCount") > 0 %>'>

										<asp:Literal ID="txtName" runat="server" Text='<%# Eval("Name") %>' />
										(<asp:Literal ID="txtChildCount" runat="server" Text='<%# Eval("ChildCount") %>' />)
									</a>
									<span runat='server'
										visible='<%# (int)Eval("ChildCount") == 0 %>'>
										<asp:Literal ID="Literal1" runat="server" Text='<%# Eval("Name") %>' />
									</span>
							</ItemTemplate>
						</asp:TemplateField>

						<asp:TemplateField
							HeaderText="<%$ Tokens: StringResource, admin.common.Edit %>"
							HeaderStyle-Width="12%">
							<ItemTemplate>
								<asp:HyperLink runat="server"
									CssClass="clone-link fa-sitemap"
									ToolTip="<%$ Tokens: StringResource, admin.title.editentity %>"
									Text='<%# string.Format("{0} {1}", AppLogic.GetString("admin.common.Edit"), AppLogic.GetString(string.Format("admin.common.{0}", Eval("EntityType")))) %>'
									NavigateUrl='<%# string.Format("entity.aspx?entityid={0}&entityname={1}", Eval("EntityID"), Eval("EntityType"))%>' />
							</ItemTemplate>
						</asp:TemplateField>
					</Columns>
				</asp:GridView>
			</div>

			<div class="list-action-bar">
				<asp:Button ID="btnBotUpdate" CausesValidation="true" ValidationGroup="valUpdate" runat="server" Text="<%$Tokens:StringResource, admin.common.Save %>" class="btn btn-primary" OnClick="UpdateDisplayOrder" />
			</div>
		</div>
	</div>
</asp:Content>
