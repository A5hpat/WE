<%@ Control Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontControls.Listing.IntegerFilter" Codebehind="IntegerFilter.ascx.cs" %>
<%@ Import Namespace="AspDotNetStorefrontCore" %>

<div class="form-group has-feedback">
	<label class="control-label"><%# Label %></label>
	<span class="feedback-icon">
		<input
			type="text"
			class="form-control"
			value='<%# Value %>'
			name='<%# ValueFilterName %>'
			data-bv-between="true"
			data-bv-between-max='<%# int.MaxValue%>' 
			data-bv-between-min="1" 
			data-bv-between-message="<%# AppLogic.GetString("admin.common.IntegerInvalid")%>" 
			data-bv-lessthan-inclusive="true" />
	</span>
</div>
