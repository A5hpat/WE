<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.Admin_setupFTS" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="setupfts.aspx.cs" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<script type="text/javascript">
		function CreateNew() {
			var textBoxCatalogName = document.getElementById('<%=txtNewCatalogName.ClientID%>');
			var textBoxCatalogPath = document.getElementById('<%=txtNewCatalogPath.ClientID%>');
			var radioReuse = document.getElementById('<%=radioReuse.ClientID%>');
			var radioCreate = document.getElementById('<%=radioCreate.ClientID%>');
			if (radioCreate.checked == true) {
				radioReuse.checked = false;
				var listBox = document.getElementById('<%= lstCatalogNames.ClientID %>');
				listBox.selectedIndex = -1;
				listBox.disabled = true;
				textBoxCatalogName.disabled = false;
				textBoxCatalogPath.disabled = false;
			}
		}
		function Reuse() {
			var textBoxCatalogName = document.getElementById('<%=txtNewCatalogName.ClientID%>');
			var textBoxCatalogPath = document.getElementById('<%=txtNewCatalogPath.ClientID%>');
			var radioReuse = document.getElementById('<%=radioReuse.ClientID%>');
			var radioCreate = document.getElementById('<%=radioCreate.ClientID%>');
			if (radioReuse.checked == true) {
				radioCreate.checked = false;
				var listBox = document.getElementById('<%= lstCatalogNames.ClientID %>');
				listBox.disabled = false;
				textBoxCatalogName.disabled = true;
				textBoxCatalogPath.disabled = true;
				textBoxCatalogName.value = "";
				textBoxCatalogPath.value = "";
			}
		}
		function CheckCatalog() {
			var textBoxCatalogName = document.getElementById('<%=txtNewCatalogName.ClientID%>');
			var textBoxCatalogPath = document.getElementById('<%=txtNewCatalogPath.ClientID%>');
			var radioReuse = document.getElementById('<%=radioReuse.ClientID%>');
			var radioCreate = document.getElementById('<%=radioCreate.ClientID%>');
			if (radioCreate.checked == true && radioReuse.checked == false) {
				if (textBoxCatalogName.value != "" && textBoxCatalogPath.value != "") {
					if (confirm("<%=JSwarnProceed %>" + textBoxCatalogName.value + "<%=JSwarnInLocation %>" + textBoxCatalogPath.value + "?")) {
						document.getElementById('<%= Page.Form.ClientID %>').submit;
					}
					else {
						return false;
					}
				}
				if (textBoxCatalogName.value != "" && textBoxCatalogPath.value == "") {
					if (confirm("<%=JSwarnProceed %>" + textBoxCatalogName.value + "<%=JSwarnInDefaultLocation %>")) {
						document.getElementById('<%= Page.Form.ClientID %>').submit;
					}
					else {
						return false;
					}
				}
				if (textBoxCatalogName.value == "" && textBoxCatalogPath.value != "") {
					alert("<%=JSwarnDefineName %>");
				return false;
			}
			if (textBoxCatalogName.value == "" && textBoxCatalogPath.value == "") {
				alert("<%=JSwarnDefineNamePath %>");
				return false;
			}
		}
		else if (radioReuse.checked == true && radioCreate.checked == false) {
			if (textBoxCatalogName.value == "" && textBoxCatalogPath.value == "") {
				var listBox = document.getElementById('<%=lstCatalogNames.ClientID %>');
				var text = "";
				for (i = 0; i < listBox.options.length; i++) {
					if (listBox.options[i].selected) {
						text = text + listBox.options[i].text;
					}
				}
				if (text == "") {
					alert("<%=JSwarnSelectCatalog %>");
					return false;
				}
				else {
					if (confirm("<%=JSwarnReuseCatalog %>" + text + "?")) {
						document.getElementById('<%= Page.Form.ClientID %>').submit;
					}
					else {
						return false;
					}
				}
			}
			if (textBoxCatalogName.value != "" || textBoxCatalogPath.value != "") {
				alert("<%=JSwarnDefineSingleCatalog %>");
				textBoxCatalogName.value = "";
				textBoxCatalogPath.value = "";
				return false;
			}
		}
		else {
			alert("<%=JSwarnSelectRadioButton %>");
			return false;
		}
		}
		function WarnUninstall() {
			if (confirm("<%=JSwarnUninstallFTS %>")) {
					return true;
				}
				else {
					return false;
				}
			}
		function WarnOptimize() {
			if (confirm("<%=JSwarnOptimizeFTS %>")) {
				return true;
			}
			else {
				return false;
			}
		}
	</script>
	<h1>
		<i class="fa fa-search"></i>
		<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.title.setupfts %>" />
	</h1>
	<aspdnsf:AlertMessage runat="server" ID="ctlAlertMessage1" />
	<aspdnsf:AlertMessage runat="server" ID="ctlAlertMessage2" />
	<aspdnsf:AlertMessage runat="server" ID="ctlAlertMessageFTSSetup" />
	<aspdnsf:AlertMessage runat="server" ID="ctlAlertMessageFTSDisabled" />
	<aspdnsf:AlertMessage runat="server" ID="AlertMessageFTSIsEnabledAndWorking" />

	<div class="white-ui-box">
		<div class="white-box-heading">
			<asp:Label ID="lblIntro" runat="server" Text="<%$ Tokens:StringResource, setupFTS.aspx.2 %>" />
		</div>
		<div class="form-group">
			<div class="row admin-row">
				<div class="col-sm-3">
					<asp:Label ID="lblLanguage" runat="server" Text="<%$ Tokens:StringResource, setupFTS.aspx.5 %>" />
				</div>
				<div class="col-sm-9">
					<asp:DropDownList ID="ddlLanguage" runat="server" Width="155px" ForeColor="Black">
						<asp:ListItem>Chinese-Simplified</asp:ListItem>
						<asp:ListItem>Chinese-Traditional</asp:ListItem>
						<asp:ListItem>Danish</asp:ListItem>
						<asp:ListItem>Dutch</asp:ListItem>
						<asp:ListItem>English-International</asp:ListItem>
						<asp:ListItem>English-US</asp:ListItem>
						<asp:ListItem>French</asp:ListItem>
						<asp:ListItem>German</asp:ListItem>
						<asp:ListItem>Italian</asp:ListItem>
						<asp:ListItem>Japanese</asp:ListItem>
						<asp:ListItem>Korean</asp:ListItem>
						<asp:ListItem Selected="True">Neutral</asp:ListItem>
						<asp:ListItem>Polish</asp:ListItem>
						<asp:ListItem>Portuguese</asp:ListItem>
						<asp:ListItem>Portuguese(Brazil)</asp:ListItem>
						<asp:ListItem>Russian</asp:ListItem>
						<asp:ListItem>Spanish</asp:ListItem>
						<asp:ListItem>Swedish</asp:ListItem>
						<asp:ListItem>Thai</asp:ListItem>
						<asp:ListItem>Turkish</asp:ListItem>
					</asp:DropDownList>
				</div>
			</div>
			<div class="row admin-row">
				<div class="col-sm-3">
					<asp:Label ID="lblRadioCreate" runat="server" Text="<%$ Tokens:StringResource, setupFTS.aspx.6 %>" />
				</div>
				<div class="col-sm-9">
					<asp:RadioButton ID="radioCreate" runat="server" />
				</div>
			</div>
			<div class="row admin-row">
				<div class="col-sm-3">
					<asp:Label ID="lblRadioReuse" runat="server" Text="<%$ Tokens:StringResource, setupFTS.aspx.7 %>" />
				</div>
				<div class="col-sm-9">
					<asp:RadioButton ID="radioReuse" runat="server" />
				</div>
			</div>
			<div class="row admin-row">
				<div class="col-sm-3">
					<asp:Label ID="lblNewCatalogName" runat="server" Text="<%$ Tokens:StringResource, setupFTS.aspx.8 %>" />
				</div>
				<div class="col-sm-9">
					<asp:TextBox ID="txtNewCatalogName" CssClass="form-control" runat="server" MaxLength="30" Enabled="False" Width="300px" />
				</div>
			</div>
			<div class="row admin-row">
				<div class="col-sm-3">
					<asp:Label ID="lblNewCatalogPath" runat="server" Text="<%$ Tokens:StringResource, setupFTS.aspx.9 %>" />
				</div>
				<div class="col-sm-9">
					<asp:TextBox ID="txtNewCatalogPath" CssClass="form-control" runat="server" MaxLength="80" Enabled="False" Width="300px" />
				</div>
			</div>
			<div class="row admin-row">
				<div class="col-sm-3">
					<asp:Label ID="lblCatalogList" runat="server" Text="<%$ Tokens:StringResource, setupFTS.aspx.10 %>" />
				</div>
				<div class="col-sm-9">
					<asp:ListBox ID="lstCatalogNames" runat="server" Width="300px" Rows="6" />
				</div>
			</div>
		</div>
		<div class="item-action-bar">
			<asp:Button ID="btnUninstallFTS" CssClass="btn btn-danger" runat="server" Text="<%$ Tokens:StringResource, setupFTS.aspx.21 %>"
				OnClientClick="return WarnUninstall()" OnClick="btnUninstallFTS_Click" />
			<asp:Button ID="hyperNoiseWord" runat="server" PostBackUrl="setupFTS_NoiseWords.aspx"
				Text="<%$ Tokens:StringResource, setupFTS.aspx.28 %>" CssClass="btn btn-default" Visible="False" />
			<asp:Button ID="btnInstallFTS" CssClass="btn btn-primary" runat="server" Text="<%$ Tokens:StringResource, setupFTS.aspx.11 %>"
				OnClientClick="return CheckCatalog()" />
			<asp:Button ID="btnOptimize" CssClass="btn btn-primary" runat="server" Text="<%$ Tokens:StringResource, setupFTS.aspx.22 %>"
				OnClientClick="return WarnOptimize()" OnClick="btnOptimize_Click" />
		</div>
	</div>
</asp:Content>