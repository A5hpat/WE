<%@ Page Language="c#" Inherits="AspDotNetStorefrontAdmin.ShippingByWeight" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master"  Codebehind="shippingbyweight.aspx.cs" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
    <h1>
		<i class="fa fa-anchor"></i>
		Shipping By Order Weight
    </h1>
	<aspdnsf:AlertMessage runat="server" ID="AlertMessage" />
	<div class="list-action-bar">
		<asp:HyperLink runat="server" NavigateUrl="shipping.aspx" CssClass="btn btn-default" Text="<%$Tokens:StringResource, admin.common.close %>" />
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
				OnRowDeleting="ShippingGrid_RowDeleting" 
				DataKeyNames="RowGuid"
				OnRowEditing="ShippingGrid_RowEditing" 
				OnRowCancelingEdit="ShippingGrid_RowCancelingEdit"
				OnRowUpdating="ShippingGrid_RowUpdating" 
				OnRowCreated="ShippingGrid_RowCreated" >

				<SelectedRowStyle CssClass="grid-view-action" />
			</asp:GridView>
			<asp:Button runat="server" ID="btnInsert" CssClass="btn btn-action btn-sm" Text="Add New Row" OnClick="btnInsert_Click" />
		</div>
		<div class="list-action-bar">
			<asp:HyperLink runat="server" NavigateUrl="shipping.aspx" CssClass="btn btn-default" Text="<%$Tokens:StringResource, admin.common.close %>" />
		</div>
	</asp:Panel>

</asp:Content>