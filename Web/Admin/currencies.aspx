<%@ Page Language="c#" Inherits="AspDotNetStorefrontAdmin.currencies" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" CodeBehind="currencies.aspx.cs" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<div id="container">
		<h1>
			<i class="fa fa-money"></i>
			<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.title.currencies %>" />
		</h1>
		<aspdnsf:AlertMessage ID="ctlAlertMessage" runat="server" />

		<div class="white-ui-box">
			<div class="white-box-heading" runat="server" id="divInstalledCurrenciesHeader">
				<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.currencies.installed %>" />
			</div>
			<asp:GridView CssClass="table" ID="grdCurrencies" runat="server"
				PagerSettings-Position="TopAndBottom"
				AutoGenerateColumns="False"
				OnRowCommand="gMain_RowCommand"
				BorderStyle="None"
				BorderWidth="0px"
				CellPadding="0"
				GridLines="None"
				CellSpacing="-1"
				ShowFooter="false">
				<Columns>
					<asp:TemplateField HeaderText="ID" SortExpression="CurrencyID">
						<ItemTemplate>
							<asp:Literal runat="server" Text='<%# Eval("CurrencyID") %>' ID="litCurrencyID" />
						</ItemTemplate>
						<ItemStyle Width="5%" />
					</asp:TemplateField>

					<asp:TemplateField HeaderText="<%$ Tokens:StringResource, admin.common.name %>">
						<ItemTemplate>
							<asp:TextBox ID="txtName" Text='<%# Eval("Name") %>' runat="server" CssClass="form-control" />
							<asp:RequiredFieldValidator runat="server" ControlToValidate="txtName" ErrorMessage="<%$Tokens:StringResource, admin.currencies.required %>" ValidationGroup="gUpdate" Display="Dynamic" CssClass="text-danger" />
						</ItemTemplate>
						<ItemStyle Width="8%" />
					</asp:TemplateField>

					<asp:TemplateField HeaderText="<%$ Tokens:StringResource, admin.currencies.Code %>">
						<ItemTemplate>
							<asp:TextBox ID="txtCode" Text='<%# Eval("CurrencyCode") %>' runat="server" CssClass="form-control" />
							<asp:RequiredFieldValidator runat="server" ControlToValidate="txtCode" ErrorMessage="<%$Tokens:StringResource, admin.currencies.required %>" ValidationGroup="gUpdate" Display="Dynamic" CssClass="text-danger" />
						</ItemTemplate>
						<ItemStyle Width="8%" />
					</asp:TemplateField>

					<asp:TemplateField HeaderText="<%$ Tokens:StringResource, admin.currencies.ExchangeRate %>">
						<ItemTemplate>
							<asp:TextBox ID="txtRate" Text='<%# Eval("ExchangeRate") %>' runat="server" CssClass="form-control" />
							<asp:RequiredFieldValidator runat="server" ControlToValidate="txtRate" ErrorMessage="<%$Tokens:StringResource, admin.currencies.required %>" ValidationGroup="gUpdate" Display="Dynamic" CssClass="text-danger" />
							<asp:CompareValidator runat="server" Type="Double" Operator="DataTypeCheck" ControlToValidate="txtRate" ErrorMessage="<%$Tokens:StringResource, admin.currencies.decimalRequired %>" ValidationGroup="gUpdate" Display="Dynamic" CssClass="text-danger" />
							<asp:CompareValidator ControlToValidate="txtRate" Operator="GreaterThan" ValidationGroup="gUpdate" CssClass="text-danger" Type="Double" Display="Dynamic" runat="server" ErrorMessage="<%$Tokens:StringResource, admin.currencies.rateMinimum %>" ValueToCompare="0" />
						</ItemTemplate>
						<ItemStyle Width="8%" />
					</asp:TemplateField>

					<asp:TemplateField HeaderText="<%$ Tokens:StringResource, admin.currencies.DisplayLocale %>">
						<ItemTemplate>
							<asp:TextBox ID="txtDisplayFormat" Text='<%# Eval("DisplayLocaleFormat") %>' runat="Server" CssClass="form-control"></asp:TextBox>
						</ItemTemplate>
					</asp:TemplateField>

					<asp:TemplateField HeaderText="<%$ Tokens:StringResource, admin.currencies.DisplaySpec %>">
						<ItemTemplate>
							<asp:TextBox ID="txtDisplaySpec" Text='<%# Eval("DisplaySpec") %>' runat="Server" CssClass="form-control" ValidationGroup="gUpdate"></asp:TextBox>
						</ItemTemplate>
					</asp:TemplateField>

					<asp:TemplateField HeaderText="<%$ Tokens:StringResource, admin.common.Published %>">
						<ItemTemplate>
							<asp:CheckBox ID="cbxPublished" Checked='<%# Eval("Published").ToString() == "1" %>' runat="Server" />
						</ItemTemplate>
						<ItemStyle Width="8%" />
					</asp:TemplateField>

					<asp:TemplateField HeaderText="<%$ Tokens:StringResource, admin.currencies.LastUpdatedOn %>">
						<ItemTemplate>
							<asp:Literal ID="litUpdatedOn" Text='<%# Eval("LastUpdated") %>' runat="Server" />
						</ItemTemplate>
					</asp:TemplateField>

					<asp:TemplateField HeaderText="<%$ Tokens:StringResource, admin.common.DisplayOrder %>">
						<ItemTemplate>
							<asp:TextBox ID="txtDisplayOrder" Text='<%# Eval("DisplayOrder") %>' runat="Server" CssClass="form-control" ValidationGroup="gUpdate"></asp:TextBox>
							<asp:RequiredFieldValidator runat="server" ControlToValidate="txtDisplayOrder" ErrorMessage="<%$Tokens:StringResource, admin.currencies.required %>" ValidationGroup="gUpdate" Display="Dynamic" CssClass="text-danger" />
							<asp:CompareValidator runat="server" Type="Integer" Operator="DataTypeCheck" ControlToValidate="txtDisplayOrder" ErrorMessage="<%$Tokens:StringResource, admin.currencies.integerRequired %>" ValidationGroup="gUpdate" Display="Dynamic" CssClass="text-danger" />
						</ItemTemplate>
						<ItemStyle Width="8%" />
					</asp:TemplateField>

					<asp:TemplateField HeaderText="Delete">
						<ItemTemplate>
							<asp:LinkButton ID="Delete" ToolTip="Delete" Text='<i class="fa fa-times"></i> Delete' OnClientClick="return confirm('Really delete this currency?');" CssClass="delete-link" CommandName="DeleteItem" CommandArgument='<%# Eval("CurrencyID") %>' runat="Server" />
						</ItemTemplate>
						<ItemStyle Width="5%" />
					</asp:TemplateField>

				</Columns>
				<FooterStyle CssClass="gridFooter" />
				<RowStyle CssClass="gridRow" />
				<EditRowStyle CssClass="gridEdit2" />
				<PagerStyle CssClass="tablepagerGrid" />
				<HeaderStyle CssClass="gridHeader" />
				<AlternatingRowStyle CssClass="gridAlternatingRow" BorderWidth="0px" />
			</asp:GridView>

			<table class="table table-detail" runat="server" id="tblSaveButton">
				<tr>
					<td>
						<asp:Button runat="server" CausesValidation="true" ValidationGroup="gUpdate" ID="btnSaveCurrencies" OnClick="btnSaveCurrencies_Click" Text="<%$Tokens:StringResource, admin.common.Save %>" CssClass="btn btn-primary" />
					</td>
				</tr>
			</table>

			<table id="tblNewRow" runat="server" class="table table-detail">
				<tr runat="server" visible="false" id="tblNewRowHeader">
					<th>&nbsp;</th>
					<th>
						<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.name %>" /></th>
					<th>
						<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.currencies.Code %>" /></th>
					<th>
						<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.currencies.ExchangeRate %>" /></th>
					<th>
						<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.currencies.DisplayLocale %>" /></th>
					<th>
						<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.currencies.DisplaySpec %>" /></th>
					<th>
						<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.Published %>" /></th>
					<th>
						<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.currencies.LastUpdatedOn %>" /></th>
					<th>
						<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.DisplayOrder %>" /></th>
					<th>&nbsp;</th>
				</tr>
				<tr>
					<td style="width: 68px;">
						<asp:Literal runat="server" Text="New:" /></td>
					<td style="width: 8%;">
						<asp:TextBox ID="txtNewName" CssClass="form-control" runat="server" />
						<asp:RequiredFieldValidator runat="server" ControlToValidate="txtNewName" ErrorMessage="<%$Tokens:StringResource, admin.currencies.required %>" ValidationGroup="gAdd" Display="Dynamic" CssClass="text-danger" />
					</td>
					<td style="width: 8%;">
						<asp:TextBox ID="txtNewCode" CssClass="form-control" runat="server" />
						<asp:RequiredFieldValidator runat="server" ControlToValidate="txtNewCode" ErrorMessage="<%$Tokens:StringResource, admin.currencies.required %>" ValidationGroup="gAdd" Display="Dynamic" CssClass="text-danger" />
					</td>
					<td style="width: 8%;">
						<asp:TextBox ID="txtNewExchangeRate" CssClass="form-control" runat="server" />
						<asp:RequiredFieldValidator runat="server" ControlToValidate="txtNewExchangeRate" ErrorMessage="<%$Tokens:StringResource, admin.currencies.required %>" ValidationGroup="gAdd" Display="Dynamic" CssClass="text-danger" />
						<asp:CompareValidator runat="server" Type="Double" Operator="DataTypeCheck" ValidationGroup="gAdd" ControlToValidate="txtNewExchangeRate" ErrorMessage="<%$Tokens:StringResource, admin.currencies.decimalRequired %>" Display="Dynamic" CssClass="text-danger" />
						<asp:CompareValidator ControlToValidate="txtNewExchangeRate" Operator="GreaterThan" ValidationGroup="gAdd" CssClass="text-danger" Type="Double" Display="Dynamic" runat="server" ErrorMessage="<%$Tokens:StringResource, admin.currencies.rateMinimum %>" ValueToCompare="0" />
					</td>
					<td>
						<asp:TextBox ID="txtNewDisplayLocale" CssClass="form-control" runat="server" />
					</td>
					<td>
						<asp:TextBox ID="txtNewDisplaySpec" CssClass="form-control" runat="server" />
					</td>
					<td style="width: 8%;">
						<asp:CheckBox ID="cbxNewPublished" Checked="true" runat="server" />
					</td>
					<td style="width: 187px;">&nbsp;</td>
					<td style="width: 8%;">
						<asp:TextBox ID="txtNewDisplayOrder" CssClass="form-control" runat="server" />
						<asp:RequiredFieldValidator runat="server" ControlToValidate="txtNewDisplayOrder" ErrorMessage="<%$Tokens:StringResource, admin.currencies.required %>" ValidationGroup="gAdd" Display="Dynamic" CssClass="text-danger" />
						<asp:CompareValidator runat="server" Type="Integer" Operator="DataTypeCheck" ValidationGroup="gAdd" ControlToValidate="txtNewDisplayOrder" ErrorMessage="<%$Tokens:StringResource, admin.currencies.integerRequired %>" Display="Dynamic" CssClass="text-danger" />
					</td>
					<td style="width: 92px;">&nbsp;</td>
				</tr>
				<tr>
					<td colspan="10">
						<asp:Button runat="server" CausesValidation="true" ValidationGroup="gAdd" ID="btnAddCurrency" OnClick="btnAddCurrency_Click" Text="<%$Tokens:StringResource, admin.currencies.createCurrency %>" CssClass="btn btn-action" />
					</td>
				</tr>
			</table>
		</div>

		<asp:Panel runat="server" ID="pnlMultipleCurrencies" Visible="false">
			<%--Conversion Test--%>
			<div class="white-ui-box">
				<div class="white-box-heading">
					<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.currencies.conversionTest %>" />
				</div>

				<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.currencies.testAmount %>" />
				<asp:TextBox runat="server" ID="txtTestAmount" />

				<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.systemlog.Source %>" />
				<asp:DropDownList runat="server" ID="ddlSource" />

				<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.currencies.Target %>" />
				<asp:DropDownList runat="server" ID="ddlTarget" />

				<asp:TextBox runat="server" Enabled="false" ID="txtTestResult" Visible="false" />
				<asp:Button runat="server" ID="btnConversionTest" OnClick="btnConversionTest_Click" Text="Test Conversion" CssClass="btn btn-default" />
			</div>

			<%--Live Rates--%>
			<div class="white-ui-box">
				<div class="white-box-heading">
					<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.currencies.advancedLiveRates %>" />
				</div>
				<div class="row">
					<div class="col-md-2">
						<asp:Literal runat="server" ID="litLiveURL" />
					</div>
					<div class="col-md-4">
						<asp:TextBox runat="server" ID="txtLiveURL" CssClass="form-control" />
					</div>
					<div class="col-md-6">
						<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.currencies.EmptyString %>" />
					</div>
				</div>
				<div class="row">
					<div class="col-md-2">
						<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.currencies.CurrencyFeedBaseCurrencyCode %>" />
					</div>
					<div class="col-md-4">
						<asp:TextBox runat="server" ID="txtBaseCurrencyCode" CssClass="form-control" />
					</div>
					<div class="col-md-6">
						<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.currencies.CurrencyCodeValidity %>" />
					</div>
				</div>
				<div class="row">
					<div class="col-md-2">
						<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.currencies.CurrencyFeedXmlPackage %>" />
					</div>
					<div class="col-md-4">
						<asp:TextBox runat="server" ID="txtXmlPackage" CssClass="form-control" />
					</div>
					<div class="col-md-6">
						<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.currencies.EmptyString %>" />
					</div>
				</div>
				<asp:Button runat="server" ID="btnGetLiveRates" Text="<%$Tokens:StringResource, admin.currencies.GetLiveRates %>" OnClick="btnGetLiveRates_Click" CssClass="btn btn-primary" />

				<asp:Panel ID="pnlLiveRatesDebug" runat="server" Visible="false">
					<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.currencies.XmlPackageDoc %>" />
					<asp:TextBox runat="server" ID="txtXmlDoc" TextMode="MultiLine" CssClass="form-control" Rows="10" />
					<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.currencies.TransformMasterXml %>" />
					<asp:TextBox runat="server" ID="txtXmlTransform" TextMode="MultiLine" CssClass="form-control" Rows="10" />
				</asp:Panel>
			</div>
		</asp:Panel>

	</div>

	<asp:Literal ID="ltContent" runat="server" />
</asp:Content>
