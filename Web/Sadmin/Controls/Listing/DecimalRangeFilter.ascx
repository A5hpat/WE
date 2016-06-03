<%@ Control Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontControls.Listing.DecimalRangeFilter" Codebehind="DecimalRangeFilter.ascx.cs" %>

<div class="row">
	<div class="col-md-6">

		<div class="form-group has-feedback">
			<label class="control-label"><%# StartLabel %></label>
			<span class="feedback-icon">
				<input
					type="text"
					class="form-control"
					value='<%# StartValue %>'
					name='<%# StartValueFilterName %>'
					data-bv-numeric="true" 
					data-bv-numeric-message="Invalid number"
					data-bv-numeric-separator='<%# System.Threading.Thread.CurrentThread.CurrentUICulture.NumberFormat.CurrencyDecimalSeparator %>' />
			</span>
		</div>

	</div>
	<div class="col-md-6">

		<div class="form-group has-feedback">
			<label class="control-label"><%# EndLabel %></label>
			<span class="feedback-icon">
				<input
					type="text"
					class="form-control"
					value='<%# EndValue %>'
					name='<%# EndValueFilterName %>'
					data-bv-numeric="true" 
					data-bv-numeric-message="Invalid number"
					data-bv-numeric-separator='<%# System.Threading.Thread.CurrentThread.CurrentUICulture.NumberFormat.CurrencyDecimalSeparator %>' />
			</span>
		</div>

	</div>
</div>
