<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin._InventoryControl" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="inventorycontrol.aspx.cs" %>
<%@ Register TagPrefix="aspdnsf" TagName="AppConfigInfo" Src="controls/appconfiginfo.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="StoreSelector" Src="controls/StoreSelector.ascx" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<h1>
		<i class="fa fa-tasks"></i>
		<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.menu.InventoryControl %>" />
	</h1>
	<div>
		<aspdnsf:AlertMessage runat="server" ID="ctrlAlertMessage" />
	</div>
	<div class="item-action-bar">
		<asp:Panel runat="server" Visible='<%# LocaleSelector.HasMultipleLocales() || StoreSelector.Items.Count > 1 %>' CssClass="other-actions">
			<aspdnsf:LocaleSelector ID="LocaleSelector" runat="server" Visible='<%# LocaleSelector.HasMultipleLocales() %>' OnSelectedLocaleChanged="LocaleSelector_SelectedLocaleChanged" />
			<asp:DropDownList runat="server" ID="StoreSelector" AutoPostBack="true" OnSelectedIndexChanged="StoreSelector_SelectedIndexChanged" CssClass="form-control-inline" />
		</asp:Panel>
		<asp:Button ID="btnUpdateTop" runat="server" OnClick="btnUpdate_Click" CssClass="btn btn-primary" Text="<%$Tokens:StringResource, admin.common.Save %>" />
	</div>

	<div class="white-ui-box">
		<div class="white-box-heading">
			<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.wizard.appconfigwarning %>" />
		</div>
		<h2><asp:Label runat="server" Text="<% $Tokens:StringResource, admin.InventoryControl.Header.TopSection %>" /></h2>
		<div class="row admin-row">
			<div class="col-sm-4">
				<div class="indent">
					<asp:Label CssClass="form-control-label" runat="server" AssociatedControlID="txtHideProductsWithLessThanThisInventoryLevel" Text="<% $Tokens:StringResource, admin.InventoryControl.HideProductsWithLessThanThisInventoryLevel %>" />
					<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.InventoryControl.Tooltip.HideProductsWithLessThanThisInventoryLevel %>">
						<i class="fa fa-question-circle fa-lg"></i>
					</asp:Label>
				</div>
			</div>
			<div class="col-sm-8">
				<asp:TextBox CssClass="form-control form-control-inline text-sm" ID="txtHideProductsWithLessThanThisInventoryLevel" runat="server" MaxLength="5" />
				<asp:CompareValidator runat="server" CssClass="text-danger validator-error-adjustments" Display="Dynamic" ErrorMessage="<%$Tokens:StringResource, admin.editquantitydiscounttable.EnterInteger %>" ControlToValidate="txtHideProductsWithLessThanThisInventoryLevel" Operator="DataTypeCheck" Type="Integer" />
				<asp:RequiredFieldValidator runat="server" CssClass="text-danger validator-error-adjustments" Display="Dynamic" ErrorMessage="<%$Tokens:StringResource, admin.InventoryControl.EnterGreaterThan0 %>" ControlToValidate="txtHideProductsWithLessThanThisInventoryLevel" />
			</div>
		</div>
		<div class="row admin-row">
			<div class="col-sm-4">
				<div class="indent">
					<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.InventoryControl.ProductPageOutOfStockRedirect %>" />
					<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.InventoryControl.Tooltip.ProductPageOutOfStockRedirect %>">
						<i class="fa fa-question-circle fa-lg"></i>
					</asp:Label>
				</div>
			</div>
			<div class="col-sm-8">
				<asp:RadioButtonList ID="rblProductPageOutOfStockRedirect" runat="server" RepeatColumns="2" RepeatDirection="horizontal" RepeatLayout="Flow">
					<asp:ListItem Value="true" Text="Yes" />
					<asp:ListItem Value="false" Text="No" />
				</asp:RadioButtonList>
			</div>
		</div>
		<div class="row admin-row">
			<div class="col-sm-4">
				<div class="indent">
					<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.InventoryControl.LimitCartInQuantityInHand %>" />
					<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.InventoryControl.Tooltip.LimitCartInQuantityInHand %>">
						<i class="fa fa-question-circle fa-lg"></i>
					</asp:Label>
				</div>
			</div>
			<div class="col-sm-8">
				<asp:RadioButtonList ID="rblLimitCartToQuantityOnHand" runat="server" RepeatColumns="2" RepeatDirection="horizontal" RepeatLayout="Flow">
					<asp:ListItem Value="true" Text="Yes" />
					<asp:ListItem Value="false" Text="No" />
				</asp:RadioButtonList>
			</div>
		</div>
	</div>

	<div class="white-ui-box">
		<div class="white-box-heading">
			<h2><asp:Label runat="server" Text="<% $Tokens:StringResource, admin.InventoryControl.Header.MiddleSection %>" /></h2>
			<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.InventoryControl.Header.MiddleSection.Subtext %>" />
		</div>
		<div class="row admin-row">
			<div class="col-sm-4">
				<div class="indent">
					<asp:Label runat="server" Text="<% $Tokens:StringResource, admin.InventoryControl.DisplayOutOfStockProducts %>" />
					<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.InventoryControl.Tooltip.DisplayOutOfStockProducts %>">
						<i class="fa fa-question-circle fa-lg"></i>
					</asp:Label>
				</div>
			</div>
			<div class="col-sm-8">
				<asp:RadioButtonList ID="rblDisplayOutOfStockMessage" runat="server" RepeatColumns="2" RepeatDirection="horizontal" RepeatLayout="Flow">
					<asp:ListItem Value="true" Text="Yes" />
					<asp:ListItem Value="false" Text="No" />
				</asp:RadioButtonList>
			</div>
		</div>
		<div class="row admin-row">
			<div class="col-sm-4">
				<div class="indent">
					<asp:Label CssClass="form-control-label" runat="server" Text="<% $Tokens:StringResource, admin.InventoryControl.OutOfStockThreshold %>" />
					<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.InventoryControl.Tooltip.OutOfStockThreshold %>">
						<i class="fa fa-question-circle fa-lg"></i>
					</asp:Label>
				</div>
			</div>
			<div class="col-sm-8">
				<asp:TextBox ID="txtOutOfStockThreshold" CssClass="form-control form-control-inline text-sm" runat="server" MaxLength="5" />
				<asp:CompareValidator runat="server" CssClass="text-danger validator-error-adjustments" ControlToValidate="txtOutOfStockThreshold" CultureInvariantValues="True" ErrorMessage="<%$Tokens:StringResource, admin.InventoryControl.InputNumber %>" Display="Dynamic" Operator="DataTypeCheck" Type="Integer" />
				<asp:RequiredFieldValidator runat="server" CssClass="text-danger validator-error-adjustments" Display="Dynamic" ErrorMessage="<%$Tokens:StringResource, admin.InventoryControl.InputNumber %>" ControlToValidate="txtOutOfStockThreshold" />
			</div>
		</div>
		<div class="row admin-row">
			<div class="col-sm-12">
				<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.InventoryControl.Header.MiddleSection.ProductPages %>" />
			</div>
		</div>
		<div class="row admin-row">
			<div class="col-sm-4">
				<div class="indent">
					<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.InventoryControl.ShowInProductPages %>" />
					<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.InventoryControl.Tooltip.ShowInProductPages %>">
						<i class="fa fa-question-circle fa-lg"></i>
					</asp:Label>
				</div>
			</div>
			<div class="col-sm-8">
				<asp:RadioButtonList ID="rblShowOutOfStockMessageOnProductPages" runat="server" RepeatColumns="2" RepeatDirection="horizontal" RepeatLayout="Flow">
					<asp:ListItem Value="true" Text="Yes" />
					<asp:ListItem Value="false" Text="No" />
				</asp:RadioButtonList>
			</div>
		</div>
		<div class="row admin-row">
			<div class="col-sm-4">
				<div class="indent">
					<asp:Label CssClass="form-control-label" runat="server" Text="<%$Tokens:StringResource, admin.InventoryControl.ProductInStockMessage %>" />
					<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.InventoryControl.Tooltip.ProductInStockMessage %>">
						<i class="fa fa-question-circle fa-lg"></i>
					</asp:Label>
				</div>
			</div>
			<div class="col-sm-8">
				<asp:TextBox CssClass="form-control text-md" ID="txtProductInStockMessage" runat="server" />
			</div>
		</div>
		<div class="row admin-row">
			<div class="col-sm-4">
				<div class="indent">
					<asp:Label CssClass="form-control-label" runat="server" Text="<%$Tokens:StringResource, admin.InventoryControl.ProductOutOfStockMessage %>" />
					<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.InventoryControl.Tooltip.ProductOutOfStockMessage %>">
						<i class="fa fa-question-circle fa-lg"></i>
					</asp:Label>
				</div>
			</div>
			<div class="col-sm-8">
				<asp:TextBox CssClass="form-control text-md" ID="txtProductOutOfStockMessage" runat="server" />
			</div>
		</div>
		<div class="row admin-row">
			<div class="col-sm-12">
				<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.InventoryControl.Header.MiddleSection.EntityPages %>" />
			</div>
		</div>
		<div class="row admin-row">
			<div class="col-sm-4">
				<div class="indent">
					<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.InventoryControl.ShowInEntityPages %>" />
					<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.InventoryControl.Tooltip.ShowInEntityPages %>">
						<i class="fa fa-question-circle fa-lg"></i>
					</asp:Label>
				</div>
			</div>
			<div class="col-sm-8">
				<asp:RadioButtonList ID="rblShowOutOfStockMessageOnEntityPages" runat="server" RepeatColumns="2" RepeatDirection="horizontal" RepeatLayout="Flow">
					<asp:ListItem Value="true" Text="Yes" />
					<asp:ListItem Value="false" Text="No" />
				</asp:RadioButtonList>
			</div>
		</div>
		<div class="row admin-row">
			<div class="col-sm-4">
				<div class="indent">
					<asp:Label CssClass="form-control-label" runat="server" Text="<%$Tokens:StringResource, admin.InventoryControl.EntityInStockMessage %>" />
					<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.InventoryControl.Tooltip.EntityInStockMessage %>">
						<i class="fa fa-question-circle fa-lg"></i>
					</asp:Label>
				</div>
			</div>
			<div class="col-sm-8">
				<asp:TextBox CssClass="form-control text-md" ID="txtEntityInStockMessage" runat="server" />
			</div>
		</div>
		<div class="row admin-row">
			<div class="col-sm-4">
				<div class="indent">
					<asp:Label CssClass="form-control-label" runat="server" Text="<%$Tokens:StringResource, admin.InventoryControl.EntityOutOfStockMessage %>" />
					<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.InventoryControl.Tooltip.EntityOutOfStockMessage %>">
						<i class="fa fa-question-circle fa-lg"></i>
					</asp:Label>
				</div>
			</div>
			<div class="col-sm-8">
				<asp:TextBox CssClass="form-control text-md" ID="txtEntityOutOfStockMessage" runat="server" />
			</div>
		</div>
	</div>

	<div class="white-ui-box">
		<div class="white-box-heading">
			<h2><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.InventoryControl.Header.BottomSection %>" /></h2>
				<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.InventoryControl.Header.BottomSection.Subtext %>" />
		</div>
		<div class="row admin-row">
			<div class="col-sm-4">
				<div class="indent">
					<asp:Label runat="server" AssociatedControlID="rdbShowOutOfStockMessage" Text="<%$Tokens:StringResource, admin.InventoryControl.Kits.ShowOutOfStock %>" />
					<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.InventoryControl.Tooltip.KitShowOutOfStock %>">
						<i class="fa fa-question-circle fa-lg"></i>
					</asp:Label>
				</div>
			</div>
			<div class="col-sm-8">
				<asp:RadioButtonList ID="rdbShowOutOfStockMessage" runat="server" RepeatColumns="2" RepeatDirection="horizontal" RepeatLayout="Flow">
					<asp:ListItem Value="true" Text="Yes" />
					<asp:ListItem Value="false" Text="No" />
				</asp:RadioButtonList>
			</div>
		</div>
		<div class="row admin-row">
			<div class="col-sm-4">
				<div class="indent">
					<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.InventoryControl.Kits.AllowSaleOfOutOfStock %>" />
					<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.InventoryControl.Tooltip.KitAllowOutOfStockPurchase %>">
						<i class="fa fa-question-circle fa-lg"></i>
					</asp:Label>
				</div>
			</div>
			<div class="col-sm-8">
				<asp:RadioButtonList ID="rblAllowSaleOfOutOfStock" runat="server" RepeatColumns="2" RepeatDirection="horizontal" RepeatLayout="Flow">
					<asp:ListItem Value="true" Text="Yes" />
					<asp:ListItem Value="false" Text="No" />
				</asp:RadioButtonList>
			</div>
		</div>
		<div class="row admin-row">
			<div class="col-sm-4">
				<div class="indent">
					<asp:Label CssClass="form-control-label" runat="server" AssociatedControlID="txtKitItemOutOfStockSellAnyway" Text="<%$Tokens:StringResource, admin.InventoryControl.KitItemOutOfStockSellAnyway %>" />
					<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.InventoryControl.Tooltip.KitItemOutOfStockSellAnyway %>">
						<i class="fa fa-question-circle fa-lg"></i>
					</asp:Label>
				</div>
			</div>
			<div class="col-sm-8">
				<asp:TextBox CssClass="form-control text-md" ID="txtKitItemOutOfStockSellAnyway" runat="server" />
			</div>
		</div>
	</div>
	<div class="item-action-bar">
		<asp:Button ID="btnUpdate" runat="server" OnClick="btnUpdate_Click" CssClass="btn btn-primary" Text="<%$Tokens:StringResource, admin.common.Save %>" />
	</div>
</asp:Content>
