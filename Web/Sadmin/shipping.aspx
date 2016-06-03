<%@ Page Language="c#" MaintainScrollPositionOnPostback="true" Inherits="AspDotNetStorefrontAdmin.ShippingPage" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master"  Codebehind="shipping.aspx.cs" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
    <h1>
		<i class="fa fa-anchor"></i>
		<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.shipping.header %>" />
    </h1>
	<aspdnsf:AlertMessage runat="server" ID="AlertMessage" />
	<p><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.shipping.description %>" /></p>
	<asp:Panel runat="server" Visible="<%# FilterShipping %>" class="row">
		<div class="col-sm-3">
			<div class="store-selection-bar">
				<asp:Label runat="server" AssociatedControlID="StoreSelector" Text="Store:" />
				<asp:DropDownList runat="server" ID="StoreSelector" AutoPostBack="true" OnSelectedIndexChanged="StoreSelector_SelectedIndexChanged" CssClass="text-sm" />
			</div>
		</div>
		<div class="col-sm-9">
			<div class="alert alert-warning">
				<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.shipping.storewarning %>" />
			</div>
		</div>
	</asp:Panel>
	
	<div class="white-ui-box">
		<h2><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.shipping.freeshippingheader %>" /></h2>
		<p><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.shipping.freedescription %>" /></p>
		<table class="table">
			<tr>
				<td>
					<div class="shipping-calculation">
						<input type="radio" 
							value="<%= (int)AspDotNetStorefrontCore.Shipping.ShippingCalculationEnum.AllOrdersHaveFreeShipping %>" 
							onclick="setHiddenCalculationId(this.value)" 
							name="ShippingCalculationRadio" 
							<%= (int)AspDotNetStorefrontCore.Shipping.ShippingCalculationEnum.AllOrdersHaveFreeShipping == SelectedShippingCalculationId ? "checked='checked'" : "" %> />

						<asp:Label runat="server" ID="AllOrdersHaveFreeShippingName" />
					</div>
				</td>
			</tr>
		</table>
		<p><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.shipping.realtimeoptions %>" /></p>
		<ul>
			<li>
				<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.shipping.freeshipping %>" />
			</li>
			<li>
				<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.shipping.rtlocalpickup %>" />
			</li>
		</ul>
		<div class="table-buttons-bottom">
			<asp:Button runat="server" CssClass="btn btn-primary" Text="<%$Tokens:StringResource, admin.common.save %>" OnClick="btnSubmit_Click" />
		</div>
	</div>
	<div class="white-ui-box">
		<h2><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.shipping.realtimeheader %>" /></h2>
		<p><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.shipping.realtimedescription %>" /></p>
		<table class="table">
			<tr>
				<td>
					<div class="shipping-calculation">
						<input type="radio" 
							value="<%= (int)AspDotNetStorefrontCore.Shipping.ShippingCalculationEnum.UseRealTimeRates %>" 
							onclick="setHiddenCalculationId(this.value)" 
							name="ShippingCalculationRadio" 
							<%= (int)AspDotNetStorefrontCore.Shipping.ShippingCalculationEnum.UseRealTimeRates == SelectedShippingCalculationId ? "checked='checked'" : "" %> />

						<asp:Label runat="server" ID="UseRealTimeRatesName" />
					</div>
				</td>
				<td class="text-right">
					<a href="appconfigs.aspx?filter.group=RTSHIPPING" class="edit-link fa-share">Edit</a>
				</td>
			</tr>
		</table>
		<div class="table-buttons-bottom">
			<a href="shippingmethodsrealtime.aspx" class="btn btn-default btn-sm">
				<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.viewrealtimeshippingmethods %>" />
			</a>
			<asp:Button runat="server" CssClass="btn btn-primary" Text="<%$Tokens:StringResource, admin.common.save %>" OnClick="btnSubmit_Click" />
		</div>
	</div>
	<div class="white-ui-box">
		<h2><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.shipping.customcalculationsheader %>" /></h2>
		<p><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.shipping.customshippingdescription %>" /></p>
		<table class="table">
				<tr>
					<td>
						<div class="shipping-calculation">
							<input type="radio" 
								value="<%= (int)AspDotNetStorefrontCore.Shipping.ShippingCalculationEnum.UseIndividualItemShippingCosts %>" 
								onclick="setHiddenCalculationId(this.value)" 
								name="ShippingCalculationRadio" 
								<%= (int)AspDotNetStorefrontCore.Shipping.ShippingCalculationEnum.UseIndividualItemShippingCosts == SelectedShippingCalculationId ? "checked='checked'" : "" %> />

							<asp:Label runat="server" ID="UseIndividualItemShippingCostsName" />
						</div>
					</td>
					<td class="shippng-calculation-description" colspan="2">
						<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.shipping.individualshippingdescription %>" />
					</td>
				</tr>
				<tr>
					<td>
						<div class="shipping-calculation">
							<input type="radio" 
								value="<%= (int)AspDotNetStorefrontCore.Shipping.ShippingCalculationEnum.CalculateShippingByWeight %>" 
								onclick="setHiddenCalculationId(this.value)" 
								name="ShippingCalculationRadio" 
								<%= (int)AspDotNetStorefrontCore.Shipping.ShippingCalculationEnum.CalculateShippingByWeight == SelectedShippingCalculationId ? "checked='checked'" : "" %> />

							<asp:Label runat="server" ID="CalculateShippingByWeightName" />
						</div>
					</td>
					<td class="shippng-calculation-description">
						<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.shipping.shippingbyweightdescription %>" />
					</td>
					<td class="text-right">
						<a href="<%= String.Format("shippingbyweight.aspx?storeid={0}", SelectedStoreId) %>" class="edit-link fa-share">Edit</a>
					</td>
				</tr>
				<tr>
					<td>
						<div class="shipping-calculation">
							<input type="radio" 
								value="<%= (int)AspDotNetStorefrontCore.Shipping.ShippingCalculationEnum.CalculateShippingByTotal %>" 
								onclick="setHiddenCalculationId(this.value)" 
								name="ShippingCalculationRadio" 
								<%= (int)AspDotNetStorefrontCore.Shipping.ShippingCalculationEnum.CalculateShippingByTotal == SelectedShippingCalculationId ? "checked='checked'" : "" %> />

							<asp:Label runat="server" ID="CalculateShippingByTotalName" />
						</div>
					</td>
					<td class="shippng-calculation-description">
						<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.shipping.shippingbytotaldescription %>" />
					</td>
					<td class="text-right">
						<a href="<%= String.Format("shippingbytotal.aspx?storeid={0}", SelectedStoreId) %>" class="edit-link fa-share">Edit</a>
					</td>
				</tr>
				<tr>
					<td>
						<div class="shipping-calculation">
							<input type="radio" 
								value="<%= (int)AspDotNetStorefrontCore.Shipping.ShippingCalculationEnum.UseFixedPrice %>" 
								onclick="setHiddenCalculationId(this.value)" 
								name="ShippingCalculationRadio" 
								<%= (int)AspDotNetStorefrontCore.Shipping.ShippingCalculationEnum.UseFixedPrice == SelectedShippingCalculationId ? "checked='checked'" : "" %> />

							<asp:Label runat="server" ID="UseFixedPriceName" />
						</div>
					</td>
					<td class="shippng-calculation-description">
						<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.shipping.fixedpricedescription %>" />
					</td>
					<td class="text-right">
						<a href="<%= String.Format("shippingfixedprices.aspx?storeid={0}", SelectedStoreId) %>" class="edit-link fa-share">Edit</a>
					</td>
				</tr>
				<tr>
					<td>
						<div class="shipping-calculation">
							<input type="radio" 
								value="<%= (int)AspDotNetStorefrontCore.Shipping.ShippingCalculationEnum.UseFixedPercentageOfTotal %>" 
								onclick="setHiddenCalculationId(this.value)" 
								name="ShippingCalculationRadio" 
								<%= (int)AspDotNetStorefrontCore.Shipping.ShippingCalculationEnum.UseFixedPercentageOfTotal == SelectedShippingCalculationId ? "checked='checked'" : "" %> />

							<asp:Label runat="server" ID="UseFixedPercentageOfTotalName" />
						</div>
					</td>
					<td class="shippng-calculation-description">
						<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.shipping.fixedpercentageoftotaldescription %>" />
					</td>
					<td class="text-right">
						<a href="<%= String.Format("shippingpercentageoftotal.aspx?storeid={0}", SelectedStoreId) %>" class="edit-link fa-share">Edit</a>
					</td>
				</tr>
				<tr>
					<td>
						<div class="shipping-calculation">
							<input type="radio" 
								value="<%= (int)AspDotNetStorefrontCore.Shipping.ShippingCalculationEnum.CalculateShippingByTotalByPercent %>" 
								onclick="setHiddenCalculationId(this.value)" 
								name="ShippingCalculationRadio" 
								<%= (int)AspDotNetStorefrontCore.Shipping.ShippingCalculationEnum.CalculateShippingByTotalByPercent == SelectedShippingCalculationId ? "checked='checked'" : "" %> />

							<asp:Label runat="server" ID="CalculateShippingByTotalByPercentName" />
						</div>
					</td>
					<td>
						<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.shipping.totalbypercentdescription %>" />
					</td>
					<td class="text-right">
						<a href="<%= String.Format("shippingbytotalandpercent.aspx?storeid={0}", SelectedStoreId) %>" class="edit-link fa-share">Edit</a>
					</td>
				</tr>
			</table>
		<div class="table-buttons-bottom">
			<a href="shippingmethods.aspx" class="btn btn-default btn-sm">
				<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.viewshippingmethods %>" />
			</a>
			<asp:Button runat="server" CssClass="btn btn-primary" Text="<%$Tokens:StringResource, admin.common.save %>" OnClick="btnSubmit_Click" />
		</div>
	</div>
	<div class="white-ui-box">
		<h2><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.shipping.calculationsbyzoneheader %>" /></h2>
		<p><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.shipping.calculationsbyzonedescription %>" /></p>
		<table class="table">
			<tr>
				<td>
					<div class="shipping-calculation">
						<input type="radio" 
							value="<%= (int)AspDotNetStorefrontCore.Shipping.ShippingCalculationEnum.CalculateShippingByWeightAndZone %>" 
							onclick="setHiddenCalculationId(this.value)" 
							name="ShippingCalculationRadio" 
							<%= (int)AspDotNetStorefrontCore.Shipping.ShippingCalculationEnum.CalculateShippingByWeightAndZone == SelectedShippingCalculationId ? "checked='checked'" : "" %> />

						<asp:Label runat="server" ID="CalculateShippingByWeightAndZoneName" />
					</div>
				</td>
				<td>
					<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.shipping.weightandzonedescription %>" />
				</td>
				<td class="text-right">
					<a href="<%= String.Format("shippingbyweightandzone.aspx?storeid={0}", SelectedStoreId) %>" class="edit-link fa-share">Edit</a>
				</td>
			</tr>
			<tr>
				<td>
					<div class="shipping-calculation">
						<input type="radio" 
							value="<%= (int)AspDotNetStorefrontCore.Shipping.ShippingCalculationEnum.CalculateShippingByTotalAndZone %>" 
							onclick="setHiddenCalculationId(this.value)" 
							name="ShippingCalculationRadio" 
							<%= (int)AspDotNetStorefrontCore.Shipping.ShippingCalculationEnum.CalculateShippingByTotalAndZone == SelectedShippingCalculationId ? "checked='checked'" : "" %> />

						<asp:Label runat="server" ID="CalculateShippingByTotalAndZoneName" />
					</div>
				</td>
				<td>
					<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.shipping.totalandzonedescription %>" />
				</td>
				<td class="text-right">
					<a href="<%= String.Format("shippingbytotalandzone.aspx?storeid={0}", SelectedStoreId) %>" class="edit-link fa-share">Edit</a>
				</td>
			</tr>
		</table>
		<div class="table-buttons-bottom">
			<a href="shippingzones.aspx" class="btn btn-default btn-sm">
				<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.viewshippingzones %>" />
			</a>
			<a href="shippingmethods.aspx" class="btn btn-default btn-sm">
				<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.viewshippingmethods %>" />
			</a>
			<asp:Button runat="server" CssClass="btn btn-primary" Text="<%$Tokens:StringResource, admin.common.save %>" OnClick="btnSubmit_Click" />
		</div>

		<asp:HiddenField runat="server" ID="hdnSelectedShippingCalculationId" Value="<%#SelectedShippingCalculationId.ToString() %>" ClientIDMode="Static" />
		<script type="text/javascript">
			function setHiddenCalculationId(id) {
				$('#hdnSelectedShippingCalculationId').val(id)
			}
		</script>
	</div>

</asp:Content>