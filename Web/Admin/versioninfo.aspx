<%@ Page Language="C#" AutoEventWireup="true" Title="Version Info" Inherits="AspDotNetStorefrontAdmin.versioninfo" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="versioninfo.aspx.cs" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<h1>
		<i class="fa fa-info-circle"></i>
		<asp:Label ID="lblHeader" runat="server" Text="<%$Tokens:StringResource, admin.common.VersionInfo %>" />
	</h1>
	<div class="item-action-bar">
		<div class="col-list-action-bar">
			<asp:Button ID="btnExport" runat="server" OnClick="btnExport_Click" CssClass="btn btn-primary" Text="<%$Tokens:StringResource, admin.common.ExportResults %>" />
		</div>
	</div>
	<div class="white-ui-box">
		<div id="divReports" runat="server">
			<%-- PATCH INFORMATION --%>
			<asp:Panel runat="server" ID="pnlPatchInfo" Visible="false">
				<div class="white-box-heading">
					<h1>
						<asp:Label ID="PatchInfoHeader" runat="server" Text="<%$Tokens:StringResource, admin.common.PatchInfo %>" />
					</h1>			
				</div>
				<div>
					<div class="splash_sysInfoBody">
						<asp:Literal ID="litPatchInfo" runat="server" />
					</div>
				</div>			
			</asp:Panel>			
			<%-- END PATCH INFORMATION --%>
			<%-- SYSTEM INFORMATION --%>
			<div class="white-box-heading">
				<h1>
					<asp:Label ID="Label1" runat="server" Text="<%$Tokens:StringResource, admin.default.SystemInformation %>" />
				</h1>			
			</div>						
			<div>									
				<div class="splash_sysInfoBody">
					<table class="table wide-table-fix">
						<tr style="text-align:left;">
							<th><asp:Label ID="Label23" runat="server" Text="<%$Tokens:StringResource, admin.common.InfoType %>" /></th>
							<th><asp:Label ID="Label24" runat="server" Text="<%$Tokens:StringResource, admin.common.Info %>" /></th>
						</tr>
						<tr>
							<td>
								<asp:Label ID="Label2" runat="server" Text="<%$Tokens:StringResource, admin.default.Version %>" />
							</td>
							<td>
								<asp:Literal ID="ltStoreVersion" runat="server" />
							</td>
						</tr>
						<tr>
							<td>
								<asp:Label ID="Label3" runat="server" Text="<%$Tokens:StringResource, admin.default.CurrentServerDateTime %>" />
							</td>
							<td>
								<asp:Literal ID="ltDateTime" runat="server" />
							</td>
						</tr>
						<tr>
							<td>
								<asp:Label ID="lbTrustLevel" runat="server" Text="<%$Tokens:StringResource, admin.default.TrustLevel %>" />
							</td>
							<td>
								<asp:Literal ID="ltTrustLevel" runat="server" />
							</td>
						</tr>
						<tr>
							<td>
								<asp:Label ID="Label4" runat="server" Text="<%$Tokens:StringResource, admin.default.ExecutionMode %>" />
							</td>
							<td>
								<asp:Literal ID="ltExecutionMode" runat="server" />
							</td>
						</tr>
						<tr>
							<td>
								<asp:Label ID="Label5" runat="server" Text="<%$Tokens:StringResource, admin.default.UseSSL %>" />
							</td>
							<td>
								<asp:Literal ID="ltUseSSL" runat="server" />
							</td>
						</tr>
						<tr>
							<td>
								<asp:Label ID="Label6" runat="server" Text="<%$Tokens:StringResource, admin.default.OnLiveServer %>" />
							</td>
							<td>
								<asp:Literal ID="ltOnLiveServer" runat="server" />
							</td>
						</tr>
						<tr>
							<td>
								<asp:Label ID="Label7" runat="server" Text="<%$Tokens:StringResource, admin.default.IsSecureConnection %>" />
							</td>
							<td>
								<asp:Literal ID="ltServerPortSecure" runat="server" />
							</td>
						</tr>
						<tr>
							<td>
								<asp:Label ID="Label10" runat="server" Text="<%$Tokens:StringResource, admin.default.PrimaryStoreLocaleSetting %>" />
							</td>
							<td>
								<asp:Literal ID="ltWebConfigLocaleSetting" runat="server" />
							</td>
						</tr>
						<tr>
							<td>
								<asp:Label ID="Label11" runat="server" Text="<%$Tokens:StringResource, admin.default.SQLLocaleSetting %>" />
							</td>
							<td>
								<asp:Literal ID="ltSQLLocaleSetting" runat="server" />
							</td>
						</tr>
						<tr>
							<td>
								<asp:Label ID="Label12" runat="server" Text="<%$Tokens:StringResource, admin.default.CustomerLocaleSetting %>" />
							</td>
							<td>
								<asp:Literal ID="ltCustomerLocaleSetting" runat="server" />
							</td>
						</tr>
						<tr>
							<td>
								<asp:Label ID="Label13" runat="server" Text="<%$Tokens:StringResource, admin.default.PrimaryStoreCurrency %>" />
							</td>
							<td>
								<asp:Literal ID="PrimaryCurrency" runat="server" />
							</td>
						</tr>
						<tr>
							<td>
								<asp:Label ID="Label14" runat="server" Text="<%$Tokens:StringResource, admin.default.PaymentGateway %>" />
							</td>
							<td>
								<asp:Literal ID="ltPaymentGateway" runat="server" />
							</td>
						</tr>
						<tr>
							<td>
								<asp:Label ID="Label15" runat="server" Text="<%$Tokens:StringResource, admin.default.GatewayMode %>" />
							</td>
							<td>
								<asp:Literal ID="ltUseLiveTransactions" runat="server" />
							</td>
						</tr>
						<tr>
							<td>
								<asp:Label ID="Label16" runat="server" Text="<%$Tokens:StringResource, admin.default.TransactionMode %>" />
							</td>
							<td>
								<asp:Literal ID="ltTransactionMode" runat="server" />
							</td>
						</tr>
						<tr>
							<td>
								<asp:Label ID="Label17" runat="server" Text="<%$Tokens:StringResource, admin.default.PaymentMethods %>" />
							</td>
							<td>
								<asp:Literal ID="ltPaymentMethods" runat="server" />
							</td>
						</tr>
						<tr id="trMicropay" runat="server">
							<td>
								<asp:Label ID="Label18" runat="server" Text="<%$Tokens:StringResource, admin.default.MicroPayEnabled %>" />
							</td>
							<td>
								<asp:Literal ID="ltMicroPayEnabled" runat="server" />
							</td>
						</tr>
						<tr id="trCardinal" runat="server">
							<td>
								<asp:Label ID="Label19" runat="server" Text="<%$Tokens:StringResource, admin.default.CardinalEnabled %>" />
							</td>
							<td>
								<asp:Literal ID="CardinalEnabled" runat="server" />
							</td>
						</tr>
						<tr id="trStoreCC" runat="server">
							<td>
								<asp:Label ID="Label20" runat="server" Text="<%$Tokens:StringResource, admin.default.StoreCreditCards %>" />
							</td>
							<td>
								<asp:Literal ID="StoreCC" runat="server" />
							</td>
						</tr>
						<tr id="trGatewayRec" runat="server">
							<td>
								<asp:Label ID="Label21" runat="server" Text="<%$Tokens:StringResource, admin.default.UsingGatewayRecurringBilling %>" />
							</td>
							<td>
								<asp:Literal ID="GatewayRecurringBilling" runat="server" />
							</td>
						</tr>
					</table>
				</div>                                
			</div>				
			<%-- END SYSTEM INFORMATION --%>
			<%-- ASSEMBLY INFORMATION --%>
				<div class="white-box-heading">
					<h1>
						<asp:Label ID="Label8" runat="server" Text="<%$Tokens:StringResource, admin.common.AssemblyInfo %>" />
					</h1>			
				</div>
				<div>
					<div class="splash_sysInfoBody">
						<asp:Literal runat="server" ID="litAssemblyInfo" />
					</div>
				</div>
			<%-- END ASSEMBLY INFORMATION --%>
			<%-- FILE INFORMATION --%>
				<div class="white-box-heading">
					<h1>
						<asp:Label ID="Label9" runat="server" Text="<%$Tokens:StringResource, admin.common.FileInfo %>" />
					</h1>			
				</div>
				<div>
					<div class="splash_sysInfoBody">
						<asp:Literal runat="server" ID="litFileInfo" />
					</div>
				</div>				
			<%-- END FILE INFORMATION --%>
			<%-- CONFIGURATION INFORMATION --%>
				<div class="white-box-heading">
					<h1>
						<asp:Label ID="Label22" runat="server" Text="<%$Tokens:StringResource, admin.common.ConfigurationInfo %>" />
					</h1>			
				</div>
				<div>
					<div class="splash_sysInfoBody">
						<asp:Literal runat="server" ID="litConfigInfo" />
					</div>
				</div>				
			<%-- END CONFIGURATION INFORMATION --%>
		</div>		
    </div>	
	<div class="item-action-bar">
		<div class="col-list-action-bar">
			<asp:Button ID="Button1" runat="server" OnClick="btnExport_Click" CssClass="btn btn-primary" Text="<%$Tokens:StringResource, admin.common.ExportResults %>" />
		</div>
	</div>
</asp:Content>