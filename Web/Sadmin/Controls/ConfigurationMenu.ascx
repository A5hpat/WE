<%@ Control Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontControls.ConfigurationMenu" Codebehind="ConfigurationMenu.ascx.cs" %>
<div class="configuration-menu-wrap">
	<div id="configuration-menu">
		<div class="container-fluid">
			<div class="row">
				<div class="col-sm-4 configuration-menu-col">
					<h4 class="config-menu-header"><asp:Literal runat="server" Text="<%$Tokens:StringResource, configuration.menu.StoreSetup %>" /></h4>
					<ul class="configuration-menu-list">
						<li><a href="wizard.aspx">
							<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.wizard %>" /></a></li>
						<li><a href="inventorycontrol.aspx">
							<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.inventorycontrol %>" /></a></li>
						<li><a href="shipping.aspx">
							<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.shippingrates %>" /></a></li>
						<li><a href="stores.aspx">
							<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.stores %>" /></a></li>
						<li><a href="storemappings.aspx">
							<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.storemappings %>" /></a></li>
						<li><a href="entitybulkdisplayorder.aspx">
							<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.DisplayOrder %>" /></a></li>
						<li>
							<h4 class="config-menu-header">
								<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.taxes %>" />
							</h4>
							<ul>
								<li><a href="taxclass.aspx">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.taxclass %>" /></a></li>
								<li><a href="states.aspx">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.states %>" /></a></li>
								<li><a href="countries.aspx">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.countries %>" /></a></li>
								<li><a href="zipcodes.aspx">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.taxzip %>" /></a></li>
							</ul>
						</li>
						<li>
							<h4 class="config-menu-header">
								<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.payment %>" />
							</h4>
							<ul>
								<li><a href="wizard.aspx#country">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.PaymentMethods %>" /></a></li>
								<li><a href="creditcards.aspx">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.creditcards %>" /></a></li>
								<li><a href="bmlsettings.aspx">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.PayPalCredit %>" /></a></li>
								<li><a href="giftcards.aspx">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.giftcards %>" /></a></li>
								<li><a href="currencies.aspx">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.currencies %>" /></a></li>
							</ul>
						</li>
						
					</ul>
				</div>
				<div class="col-sm-4 configuration-menu-col">
					<h4 class="config-menu-header"><asp:Literal runat="server" Text="<%$Tokens:StringResource, configuration.menu.Configuration %>" /></h4>
					<ul class="configuration-menu-list">
						<li><a href="globalconfigs.aspx">
							<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.globalconfigparameters %>" /></a></li>
						<li><a href="appconfigs.aspx">
							<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.appconfigparameters %>" /></a></li>
						<li><a href="mailingtest.aspx">
							<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.mailingtest %>" /></a></li>
						<li><a href="localesettings.aspx">
							<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.localesettings %>" /></a></li>
					</ul>

					<h4 class="config-menu-header"><asp:Literal runat="server" Text="<%$Tokens:StringResource, configuration.menu.Maintenance %>" /></h4>
					<ul class="configuration-menu-list">
						<li><a href="storewide.aspx">
							<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.storewide %>" /></a></li>
						<li><a href="monthlymaintenance.aspx">
							<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.monthlymaintenance %>" /></a></li>
					</ul>
				</div>
				<div class="col-sm-4 configuration-menu-col">
					<h4 class="config-menu-header"><asp:Literal runat="server" Text="<%$Tokens:StringResource, configuration.menu.Advanced %>" /></h4>
					<ul class="configuration-menu-list">
						<li><a href="systemlogs.aspx">
							<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.viewsystemlog %>" /></a></li>
						<li><a href="securitylog.aspx">Security Log</a></li>
						<li><a href="changeencryptkey.aspx">
							<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.changeencryptkey %>" /></a></li>
						<li><a href="eventhandler.aspx">
							<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.eventhandlerparameters %>" /></a></li>
						<li><a href="runsql.aspx">
							<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.runsql %>" /></a></li>
						<li><a href="setupfts.aspx">
							<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.fulltextsearch %>" /></a></li>
						<li>
							<h4 class="config-menu-header">
								<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.help %>" />
							</h4>
							<ul>
								<li><a href="about.aspx">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.about %>" /></a></li>
								<li><a href="http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=10000manual&type=landingpage" target="_blank">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.OnlineManual %>" /></a></li>
								<li><a href="default.aspx?showquickstart=true">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.quickstart %>" /></a></li>
							</ul>
						</li>
					</ul>
				</div>
			</div>
		</div>
	</div>
</div>
