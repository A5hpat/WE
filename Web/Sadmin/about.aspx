<%@ Page Language="c#" Inherits="AspDotNetStorefrontAdmin.about" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master"  Codebehind="about.aspx.cs" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">    
	<div id="container">
		<div class="row admin-row">
			<div class="col-sm-6">
				<h1>
					<i class="fa fa-info-circle"></i>
					<asp:Label ID="lblHeader" runat="server" Text="<%$Tokens:StringResource, admin.title.about %>" />
				</h1>
			</div>
			<div class="col-sm-6 expandingLabel">
				<div><a href="http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=10000eula&type=licensing" target="_blank">End User License Agreement</a></div>
			</div>
		</div>
		<div class="white-ui-box">
			<div class="white-box-heading">
				<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.SecurityAudit %>" />
			</div>
			<div class="row admin-row">
				<div class="col-sm-12">
					<asp:Panel ID="pnlSecurity" runat="server">
						<div id="divSecurityAudit" runat="server">
							<div id="divSecurityAudit_Content" class="splash_divSecurityAudit" runat="server" visible="true">
								<asp:Table runat="server" ID="tblSecurityAudit" Width="100%" CellPadding="1" CellSpacing="0" />
							</div>
						</div>
					</asp:Panel>
				</div>				
			</div>
		</div>
		<div class="white-ui-box">
			<div class="white-box-heading">
				<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.about.SystemInformation %>" />
			</div>
			<div class="row admin-row">
				<div class="col-sm-3">
					<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.splash.aspx.sysinfo.VersionCodeDB %>" />
				</div>
				<div class="col-sm-9">
					<asp:Literal ID="ltStoreVersion" runat="server" /> - <a href="versioninfo.aspx?excludeImages=true"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.about.Details%>" /></a>
				</div>
			</div>
			<div class="row admin-row">
				<div class="col-sm-3">
					<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.CurrentServerDateTime %>" />
				</div>
				<div class="col-sm-9">
					<asp:Literal ID="ltDateTime" runat="server" />
				</div>
			</div>
			<div class="row admin-row">
				<div class="col-sm-3">
					<asp:Label ID="lbTrustLevel" runat="server" Text="<%$Tokens:StringResource, admin.default.TrustLevel %>" />
				</div>
				<div class="col-sm-9">
					<asp:Literal ID="ltTrustLevel" runat="server" />
				</div>
			</div>
			<div class="row admin-row">
				<div class="col-sm-3">
					<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.ExecutionMode %>" />
				</div>
				<div class="col-sm-9">
					<asp:Literal ID="ltExecutionMode" runat="server"></asp:Literal>
				</div>
			</div>
			<div class="row admin-row">
				<div class="col-sm-3">
					<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.OnLiveServer %>" />
				</div>
				<div class="col-sm-9">
					<asp:Literal ID="ltOnLiveServer" runat="server" />
				</div>
			</div>
			<div class="row admin-row">
				<div class="col-sm-3">
					<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.UseSSL %>" />
				</div>
				<div class="col-sm-9">
					<asp:Literal ID="ltUseSSL" runat="server" />
				</div>
			</div>
			<div class="row admin-row">
				<div class="col-sm-3">
					<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.IsSecureConnection %>" />
				</div>
				<div class="col-sm-9">
					<asp:Literal ID="ltServerPortSecure" runat="server" />
				</div>
			</div>
			<div class="row admin-row">
				<div class="col-sm-3">
					<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.CachingIs %>" />
				</div>
				<div class="col-sm-9">
					<asp:Literal ID="ltCaching" runat="server" />
				</div>
			</div>
			<div class="row admin-row">
				<div class="col-sm-3">
					<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.about.AdminDirectoryChanged %>" />
				</div>
				<div class="col-sm-9">
					<asp:Literal ID="ltAdminDirChanged" runat="server" />
				</div>
			</div>
		</div>
		<div class="white-ui-box">
			<div class="white-box-heading">
				<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.about.LicenseInformation %>" />
			</div>
			<div class="row admin-row">
				<div class="col-sm-12">
					<asp:Literal ID="pnlLicenseInformation" runat="server" />
				</div>				
			</div>			
		</div>
		<div class="white-ui-box">
			<div class="white-box-heading">
				<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.about.LocalizationInformation %>" />
			</div>
			<div class="row admin-row">
				<div class="col-sm-3">
					<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.splash.aspx.sysinfo.PrimaryStoreLocaleSetting %>" />
				</div>
				<div class="col-sm-9">
					<asp:Literal ID="ltWebConfigLocaleSetting" runat="server" />
				</div>
			</div>
			<div class="row admin-row">
				<div class="col-sm-3">
					<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.splash.aspx.sysinfo.SQLLocaleSetting %>" />
				</div>
				<div class="col-sm-9">
					<asp:Literal ID="ltSQLLocaleSetting" runat="server" />
				</div>
			</div>
			<div class="row admin-row">
				<div class="col-sm-3">
					<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.splash.aspx.sysinfo.CustomerLocaleSetting %>" />
				</div>
				<div class="col-sm-9">
					<asp:Literal ID="ltCustomerLocaleSetting" runat="server" />
				</div>
			</div>
			<div class="row admin-row">
				<div class="col-sm-3">
					<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.splash.aspx.sysinfo.PrimaryStoreCurrency %>" />
				</div>
				<div class="col-sm-9">
					<asp:Literal ID="ltPrimaryCurrency" runat="server" />
				</div>
			</div>
			<div class="row admin-row">
				<div class="col-sm-3">
					<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.about.LocalizationStoreCurrency %>" />
				</div>
				<div class="col-sm-9">
					<asp:Literal ID="ltLocalizationCurrencyCode" runat="server" />
				</div>
			</div>
			<div class="row admin-row">
				<div class="col-sm-3">
					<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.about.LocalizationStoreCurrencyCode %>" />
				</div>
				<div class="col-sm-9">
					<asp:Literal ID="ltLocalizationCurrencyNumericCode" runat="server" />
				</div>
			</div>
		</div>
		<div class="white-ui-box">
			<div class="white-box-heading">
				<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.about.GatewayInformation %>" />
			</div>
			<div class="row admin-row">
				<div class="col-sm-3">
					<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.PaymentGateway %>" />
				</div>
				<div class="col-sm-9">
					<asp:Literal ID="ltPaymentGateway" runat="server" />
				</div>
			</div>
			<div class="row admin-row">
				<div class="col-sm-3">
					<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.GatewayMode %>" />
				</div>
				<div class="col-sm-9">
					<asp:Literal ID="ltUseLiveTransactions" runat="server" />
				</div>
			</div>
			<div class="row admin-row">
				<div class="col-sm-3">
					<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.TransactionMode %>" />
				</div>
				<div class="col-sm-9">
					<asp:Literal ID="ltTransactionMode" runat="server" />
				</div>
			</div>
			<div class="row admin-row">
				<div class="col-sm-3">
					<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.PaymentMethods %>" />
				</div>
				<div class="col-sm-9">
					<asp:Literal ID="ltPaymentMethods" runat="server" />
				</div>
			</div>			
		</div>
		<div class="white-ui-box">
			<div class="white-box-heading">
				<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.about.ShippingInformation %>" />
			</div>
			<div class="row admin-row">
				<div class="col-sm-3">
					<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.about.ShippingCalculation %>" />
				</div>
				<div class="col-sm-9">
					<asp:Literal ID="ltShippingCalculation" runat="server" />
				</div>
			</div>
			<div class="row admin-row">
				<div class="col-sm-3">
					<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.about.OriginState %>" />
				</div>
				<div class="col-sm-9">
					<asp:Literal ID="ltOriginState" runat="server" />
				</div>
			</div>
			<div class="row admin-row">
				<div class="col-sm-3">
					<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.about.OriginZip %>" />
				</div>
				<div class="col-sm-9">
					<asp:Literal ID="ltOriginZip" runat="server" />
				</div>
			</div>
			<div class="row admin-row">
				<div class="col-sm-3">
					<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.about.OriginCountry %>" />
				</div>
				<div class="col-sm-9">
					<asp:Literal ID="ltOriginCountry" runat="server" />
				</div>
			</div>
			<div class="row admin-row">
				<div class="col-sm-3">
					<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.about.FreeShippingThreshold %>" />
				</div>
				<div class="col-sm-9">
					<asp:Literal ID="ltFreeShippingThreshold" runat="server" />
				</div>
			</div>
			<div class="row admin-row">
				<div class="col-sm-3">
					<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.about.FreeShippingThreshold %>" />
				</div>
				<div class="col-sm-9">
					<asp:Literal ID="ltFreeShippingMethods" runat="server" />
				</div>
			</div>
			<div class="row admin-row">
				<div class="col-sm-3">
					<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.about.FreeShippingRateSelection %>" />
				</div>
				<div class="col-sm-9">
					<asp:Literal ID="ltFreeShippingRateSelection" runat="server" />
				</div>
			</div>			
		</div>
    </div>    
</asp:Content>