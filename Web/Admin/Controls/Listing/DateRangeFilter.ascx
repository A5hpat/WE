<%@ Control Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontControls.Listing.DateRangeFilter" Codebehind="DateRangeFilter.ascx.cs" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<div class="row">
	<div class="col-md-6">
		<div class="form-group">
			<asp:Label runat="server" ID="StartValueLabel" AssociatedControlID="StartValue" /><br />
			<telerik:RadDatePicker ID="StartValue" runat="server" MinDate="1753-01-01" Width="100%">
				<Calendar runat="server" UseColumnHeadersAsSelectors="False" UseRowHeadersAsSelectors="False" ViewSelectorText="x" />
			</telerik:RadDatePicker>
		</div>
	</div>

	<div class="col-md-6">
		<div class="form-group">
			<asp:Label runat="server" ID="EndValueLabel" AssociatedControlID="EndValue" /><br />
			<telerik:RadDatePicker ID="EndValue" runat="server" MinDate="1753-01-01" Width="100%">
				<Calendar runat="server" UseColumnHeadersAsSelectors="False" UseRowHeadersAsSelectors="False" ViewSelectorText="x" />
			</telerik:RadDatePicker>
		</div>
	</div>
</div>
