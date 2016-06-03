<%@ Page Language="c#" Inherits="AspDotNetStorefrontAdmin.ShippingPercentageOfTotal" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master"  Codebehind="shippingpercentageoftotal.aspx.cs" %>

<%@ Import namespace="AspDotNetStorefrontCore" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
    <h1>
		<i class="fa fa-anchor"></i>
		Fixed Percentage Shipping
    </h1>
	<aspdnsf:AlertMessage runat="server" ID="AlertMessage" />
	<div class="list-action-bar">
		<asp:HyperLink runat="server" NavigateUrl="shipping.aspx" CssClass="btn btn-default" Text="<%$Tokens:StringResource, admin.common.close %>" />
		<asp:Button runat="server" OnClick="Save_Click" Text="<%$Tokens:StringResource, admin.common.save %>" CssClass="btn btn-primary" />
	</div>
	<asp:Panel runat="server" ID="StoreSelectorPanel" CssClass="white-ui-box">
		<div class="row">
			<div class="col-sm-3">
				<div class="form-group">
					<asp:Label runat="server" AssociatedControlID="StoreSelector" Text="Store:" />
					<asp:DropDownList runat="server" ID="StoreSelector" AutoPostBack="true" OnSelectedIndexChanged="StoreSelector_SelectedIndexChanged" CssClass="text-md" />
				</div>
			</div>
			<div class="col-sm-9">
				<div class="alert alert-warning">
					<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.shipping.storewarning %>" />
				</div>
			</div>
		</div>
	</asp:Panel>

	<asp:Panel runat="server" ID="ShippingRatePanel">
		<div class="white-ui-box">
			<asp:GridView runat="server" ID="ShippingGrid" 
				CssClass="table table-bordered"
				AllowSorting="false"
				AutoGenerateColumns="false" 
				GridLines="None" 
				CellPadding="-1" 
				 >

				<Columns>
					<asp:TemplateField HeaderText="Shipping Method">
						<ItemTemplate>
							<asp:Literal runat="server" Text='<%# XmlCommon.GetLocaleEntry(Eval("Name").ToString(), LocaleSetting, true)%>' />
						</ItemTemplate>
					</asp:TemplateField>
					<asp:TemplateField HeaderText="Flat Percent Of Total Order Cost (In x.xx format)">
						<ItemTemplate>
							<asp:TextBox runat="server" ID="Rate" Text='<%# String.IsNullOrEmpty(Eval("FixedPercentOfTotal").ToString()) ? String.Empty : Localization.DecimalStringForDB((Decimal)Eval("FixedPercentOfTotal")) %>' />
							<asp:HiddenField runat="server" ID="ShippingMethodID" Value='<%# Eval("ShippingMethodID") %>' />
						</ItemTemplate>
					</asp:TemplateField>
				</Columns>

				<SelectedRowStyle CssClass="grid-view-action" />
			</asp:GridView>
		</div>
		<div class="list-action-bar">
			<asp:HyperLink runat="server" NavigateUrl="shipping.aspx" CssClass="btn btn-default" Text="<%$Tokens:StringResource, admin.common.close %>" />
			<asp:Button runat="server" OnClick="Save_Click" Text="<%$Tokens:StringResource, admin.common.save %>" CssClass="btn btn-primary" />
		</div>
	</asp:Panel>

</asp:Content>